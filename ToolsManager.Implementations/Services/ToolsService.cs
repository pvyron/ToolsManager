using System.Runtime.CompilerServices;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using MassTransit;
using Microsoft.Extensions.Options;
using ToolsManager.Abstractions.Models;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations.Models;
using ToolsManager.Implementations.Settings;

namespace ToolsManager.Implementations.Services;

public sealed class ToolsService : IToolsService
{
    private readonly ILogger<ToolsService> _logger;
    private readonly IBus _messageBus;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly TableClient _tableClient;
    
    public ToolsService(IOptions<ToolsSettings> options, ILogger<ToolsService> logger, IBus messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
        _blobContainerClient = new BlobContainerClient(options.Value.StorageConnectionString, options.Value.BlobName);
        _tableClient = new TableClient(options.Value.StorageConnectionString, options.Value.TableName);
    }

    public ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info,
        CancellationToken cancellationToken)
    {
        return UploadNewTool(toolStream, info, Enumerable.Empty<string>(), cancellationToken);
    }

    public async ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, IEnumerable<string> shareWith, CancellationToken cancellationToken)
    {
        var toolId = Guid.NewGuid();
        
        _logger.LogInformation("Uploading new tool with id: {toolId} with info: {info}", toolId, info);
        
        ToolTableEntity toolEntity = new()
        {
            PartitionKey = info.UserId,
            RowKey = toolId.ToString(),
            UploadDate = DateTimeOffset.UtcNow
        };
        
        var toolBlobTask = _blobContainerClient.UploadBlobAsync(toolId.ToString(), toolStream, cancellationToken);
        var toolTableResult = await _tableClient.AddEntityAsync(toolEntity, cancellationToken);

        if (toolTableResult?.IsError ?? true)
        {
            toolBlobTask.Dispose();
            _logger.LogWarning("New tool upload failed with error: {error} with info: {info}", ToolTableEntryFailed, info);
            return ToolTableEntryFailed;
        }
        
        var toolBlobResult = await toolBlobTask;
        if (!(toolBlobResult?.HasValue ?? false))
        {
            await _tableClient.DeleteEntityAsync(toolEntity.PartitionKey, toolEntity.RowKey, cancellationToken: cancellationToken);
            _logger.LogWarning("New tool upload failed with error: {error} with info: {info}", ToolBlobUploadFailed, info);
            return ToolBlobUploadFailed;
        }

        await _messageBus.Publish(new ToolCreatedMessage(toolId, info, shareWith), cancellationToken);
        
        var metaDataResult = await _blobContainerClient.GetBlobClient(toolId.ToString())
            .SetMetadataAsync(info.ToMetadataDictionary(), cancellationToken: cancellationToken);

        if (!(metaDataResult?.HasValue ?? false))
        {
            _logger.LogWarning("New tool upload failed with error: {error} with info: {info}", metaDataResult, info);
            return ToolBlobMetadataFailed;
        }
        
        _logger.LogInformation("New tool uploaded with id: {toolId} with info: {info}", toolId, info);
        return new UploadedTool(toolId, info);
    }

    public async Task<Result<List<ToolInfo>>> GetUserTools(string userId,
        CancellationToken cancellationToken)
    {
        var query = _tableClient.QueryAsync<ToolTableEntity>(toolEntity => toolEntity.PartitionKey == userId);

        List<ValueTask<ToolInfo?>> getInoTasks = [];
        await foreach (var entityPage in QueryToolsByPartitionKey(_tableClient, userId, cancellationToken))
        {
            foreach (var entity in entityPage.Values)
            {
                var getInfoTask = GetToolFileMetadata(_blobContainerClient, entity.RowKey, cancellationToken);
                
                getInoTasks.Add(getInfoTask);
            }
        }
        
        List<ToolInfo> infos = [];
        for (var i = 0; i < getInoTasks.Count; i++)
        {
            var info = await getInoTasks[i];
            
            if (info is null)
                continue;
            
            infos.Add(info);
        }
        
        return infos;
    }

    private static async IAsyncEnumerable<Page<ToolTableEntity>> QueryToolsByPartitionKey(TableClient tableClient, string partitionKey,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = tableClient.QueryAsync<ToolTableEntity>(toolEntity => toolEntity.PartitionKey == partitionKey);
        
        await foreach (var entityPage in query.AsPages().WithCancellation(cancellationToken))
        {
            yield return entityPage;
        }
    }

    private static async ValueTask<ToolInfo?> GetToolFileMetadata(BlobContainerClient blobContainerClient, string id, CancellationToken cancellationToken)
    {
        var propertiesResponse = await blobContainerClient.GetBlobClient(id).GetPropertiesAsync(cancellationToken: cancellationToken);

        if (!propertiesResponse.HasValue)
            return null;

        return ToolInfo.Parse(id, propertiesResponse.Value.Metadata);
    }


    private static readonly Error<UploadedTool> ToolTableEntryFailed = new("Tools.Upload.Table", "Failed to index file");
    private static readonly Error<UploadedTool> ToolBlobUploadFailed = new("Tools.Upload.Blob", "Failed to upload file");
    private static readonly Error<UploadedTool> ToolBlobMetadataFailed = new("Tools.Upload.Metadata", "Failed to attach metadata to file");
}

