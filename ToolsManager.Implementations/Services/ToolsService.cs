using Azure.Data.Tables;
using Azure.Storage.Blobs;
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
    
    public async ValueTask<UploadedTool> UploadNewTool(Stream toolStream, string userName, CancellationToken cancellationToken)
    {
        var toolId = Guid.NewGuid();
        _logger.LogInformation("Generated new guid: {toolId}", toolId);
        ToolTableEntity toolEntity = new()
        {
            PartitionKey = userName,
            RowKey = toolId.ToString(),
            UploadDate = DateTimeOffset.UtcNow
        };
        var toolBlobTask = _blobContainerClient.UploadBlobAsync(toolId.ToString(), toolStream, cancellationToken);
        _logger.LogInformation("Starter uploading id: {toolId}", toolId);
        var toolTableResult = await _tableClient.AddEntityAsync(toolEntity, cancellationToken);
        _logger.LogInformation("Uploaded to table storage id: {toolId}", toolId);
        var toolBlobResult = await toolBlobTask;
        _logger.LogInformation("Completed upload of id: {toolId}", toolId);
        return new UploadedTool(toolId);
    }
}