using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace ToolsManager.Implementations.Models;

public sealed class ToolTableEntity : ITableEntity
{
    [JsonIgnore]
    public string UserId => PartitionKey;
    [JsonIgnore]
    public Guid ToolId => Guid.Parse(RowKey);
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public required DateTimeOffset UploadDate { get; set; }
    public string? UpdateUrl { get; set; }
    public DateTimeOffset? LastUpdate { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}