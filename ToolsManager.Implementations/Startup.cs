using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations.Models;
using ToolsManager.Implementations.Services;
using ToolsManager.Implementations.Settings;

namespace ToolsManager.Implementations;

public static class Startup
{
    public static void InstallToolsAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
        builder.Services.AddAuthorizationBuilder();
        builder.Services.AddDbContext<AuthDbContext>(optionsAction: options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb"), optionsBuilder =>
            {
                optionsBuilder.SetPostgresVersion(9, 6);
            });
        });

        builder.Services.AddIdentityCore<ToolsManagerUser>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddApiEndpoints();
    }

    public static void MapToolsAuthentication(this IEndpointRouteBuilder host)
    {
        host.MapGroup("/api").MapIdentityApi<ToolsManagerUser>();
    }
    
    public static void InstallToolsServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidateOptions<ToolsSettings>, ToolsSettingsValidator>();
        builder.Services.Configure<ToolsSettings>(builder.Configuration.GetSection(ToolsSettings.SettingsName));
        builder.Services.AddSingleton<IToolsService, ToolsService>();

        builder.Services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.AddConsumer<ToolsEventHandler>();

            configurator.UsingInMemory((context, config) => config.ConfigureEndpoints(context));
        });
    }
    
    public static void ValidateToolsServices(this IHost host)
    {
        var toolsSettings = host.Services.GetRequiredService<IOptions<ToolsSettings>>();
        var toolsSettingsValidator = host.Services.GetRequiredService<IValidateOptions<ToolsSettings>>();

        toolsSettingsValidator.Validate(null, toolsSettings.Value);
    }
}