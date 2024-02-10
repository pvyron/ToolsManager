using System.Text;

namespace ToolsManager.Abstractions.Models;

public sealed record ToolFileInfo
{
    public required string UserId { get; init; }
    public string? Key { get; init; }
    public string? Name { get; init; }
    public string? Extension { get; init; }

    public IDictionary<string, string> ToMetadataDictionary()
    {
        return new Dictionary<string, string>()
        {
            { "user_id", UserId },
            { "original_tag", Key is null ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(Key)) ?? ""},
            { "original_name", Name is null ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(Name)) },
            { "original_extension", Extension is null ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(Extension)) ?? "" }
        };
    }

    public static ToolFileInfo Parse(IDictionary<string, string> metadata)
    {
        return new ToolFileInfo
        {
            UserId = metadata["user_id"],
            Key = Encoding.UTF8.GetString(Convert.FromBase64String(metadata["original_tag"])),
            Name = Encoding.UTF8.GetString(Convert.FromBase64String(metadata["original_name"])),
            Extension = Encoding.UTF8.GetString(Convert.FromBase64String(metadata["original_extension"]))
        };
    }
}