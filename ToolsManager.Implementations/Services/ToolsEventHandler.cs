using MassTransit;
using ToolsManager.Implementations.Models;

namespace ToolsManager.Implementations.Services;

public class ToolsEventHandler : IConsumer<ToolCreatedMessage>
{
    private readonly ILogger<ToolsEventHandler> _logger;

    public ToolsEventHandler(ILogger<ToolsEventHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<ToolCreatedMessage> context)
    {
        var recipients = context.Message.ShareWith.ToArray();
        
        if (recipients.Length <= 0)
            return;
            
        _logger.LogInformation("Mailing tool with id: {id} recipients: {recipients}", context.Message.ToolId, string.Join(';', recipients));

        await Task.CompletedTask;
    }
}