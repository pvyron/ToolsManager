using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ToolsManager.Implementations.Settings;

public sealed class ToolsSettings
{
    public static string SettingsName = "Tools";

    [Required] public string StorageConnectionString { get; set; } = null!;

    [Required] public string TableName { get; set; } = null!;

    [Required] public string BlobName { get; set; } = null!;
}

[OptionsValidator]
public partial class ToolsSettingsValidator : IValidateOptions<ToolsSettings>
{
    
}