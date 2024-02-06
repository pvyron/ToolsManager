using Microsoft.Extensions.Options;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations.Services;
using ToolsManager.Implementations.Settings;

namespace ToolsManager.Implementations;

public static class Startup
{
    public static void InstallToolsServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidateOptions<ToolsSettings>, ToolsSettingsValidator>();
        builder.Services.Configure<ToolsSettings>(builder.Configuration.GetSection(ToolsSettings.SettingsName));
        builder.Services.AddSingleton<IToolsService, ToolsService>();
    }
    
    public static void ValidateToolsServices(this IHost host)
    {
        var toolsSettings = host.Services.GetRequiredService<IOptions<ToolsSettings>>();
        var toolsSettingsValidator = host.Services.GetRequiredService<IValidateOptions<ToolsSettings>>();

        toolsSettingsValidator.Validate(null, toolsSettings.Value);
    }
}