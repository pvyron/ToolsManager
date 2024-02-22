using MassTransit;

namespace ToolsManager.Implementations.Services;

public class MessagePublisher : BackgroundService
{
    private readonly IBus _bus;

    public MessagePublisher(IBus bus)
    {
        _bus = bus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new
            {

            });
        }
    }
}