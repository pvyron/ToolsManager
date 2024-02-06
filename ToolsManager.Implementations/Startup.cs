using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations.Services;
using ToolsManager.Implementations.Settings;

namespace ToolsManager.Implementations;

public static class Startup
{
    public static void InstallToolsServices(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ToolsSettings>(builder.Configuration.GetSection(ToolsSettings.SettingsName));
        builder.Services.AddSingleton<IToolsService, ToolsService>();
    }
}