using System.Security.Claims;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using ToolsManager.Abstractions.Models;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations.Models;
using ToolsManager.Implementations.Settings;

namespace ToolsManager.Implementations.Services;

public sealed class ToolsService : IToolsService
{
    private readonly ILogger<ToolsService> _logger;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly TableClient _tableClient;
    
    public ToolsService(IOptions<ToolsSettings> options, ILogger<ToolsService> logger)
    {
        _logger = logger;
        _blobContainerClient = new BlobContainerClient(options.Value.StorageConnectionString, options.Value.BlobName);
        _tableClient = new TableClient(options.Value.StorageConnectionString, options.Value.TableName);
    }
    
    public async ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, CancellationToken cancellationToken)
    {
        var toolId = Guid.NewGuid();

        //_logger.LogInformation("Generated new guid: {toolId}", toolId);
        ToolTableEntity toolEntity = new()
        {
            PartitionKey = info.UserId,
            RowKey = toolId.ToString(),
            UploadDate = DateTimeOffset.UtcNow
        };
        var toolBlobTask = _blobContainerClient.UploadBlobAsync(toolId.ToString(), toolStream, cancellationToken);
        //_logger.LogInformation("Starter uploading id: {toolId}", toolId);
        var toolTableResult = await _tableClient.AddEntityAsync(toolEntity, cancellationToken);

        if (toolTableResult.IsError)
        {
            if (!toolBlobTask.IsCompleted) toolBlobTask.Dispose();
            
            return ToolTableEntryFailed;
        }
        
        //_logger.LogInformation("Uploaded to table storage id: {toolId}", toolId);
        var toolBlobResult = await toolBlobTask;

        if (!toolBlobResult.HasValue)
        {
            await _tableClient.DeleteEntityAsync(toolEntity.PartitionKey, toolEntity.RowKey, cancellationToken: cancellationToken);

            return ToolBlobUploadFailed;
        }
        
        var metaDataResult = await _blobContainerClient.GetBlobClient(toolId.ToString())
            .SetMetadataAsync(info.ToMetadataDictionary(), cancellationToken: cancellationToken);

        if (!metaDataResult.HasValue)
        {
            return ToolBlobMetadataFailed;
        }
        
        //_logger.LogInformation("Completed upload of id: {toolId}", toolId);
        return new UploadedTool(toolId, info);
    }


    private static readonly Error<UploadedTool> ToolTableEntryFailed = new("Tools.Upload.Table", "Failed to index file");
    private static readonly Error<UploadedTool> ToolBlobUploadFailed = new("Tools.Upload.Blob", "Failed to upload file");
    private static readonly Error<UploadedTool> ToolBlobMetadataFailed = new("Tools.Upload.Metadata", "Failed to attach metadata to file");
}

