namespace ToolsManager.Abstractions.Models;

public sealed record ToolInfo
{
    public required string UserId { get; init; }
    public required string ToolId { get; set; }
    public string? Key { get; init; }
    public string? Name { get; init; }
    public string? Extension { get; init; }

    public static ToolInfo Parse(string toolId, IDictionary<string, string> input)
    {
        var fileInfo = ToolFileInfo.Parse(input);
        
        return new ToolInfo
        {
            UserId = fileInfo.UserId,
            ToolId = toolId,
            Key = fileInfo.Key,
            Name = fileInfo.Name,
            Extension = fileInfo.Extension
        };
    }
}