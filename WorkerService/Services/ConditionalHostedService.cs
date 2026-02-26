using Bus.Shared.Enums;
using Bus.Shared.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WorkerService.Services;

public sealed class ConditionalHostedService<TInner>(
    IServiceProvider services,
    IConfiguration configuration)
    : IHostedService
    where TInner : IHostedService
{
    private IHostedService? _inner;

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        var busParameter = configuration["BUS_TYPE"];


        if (!Enum.TryParse<BusType>(busParameter, ignoreCase: true, out var busType))
            busType = BusType.RabbitMQ;

        if (!ShouldRun(typeof(TInner), busType))
            return;

        _inner = services.GetRequiredService<TInner>();
        await _inner.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => _inner?.StopAsync(cancellationToken) ?? Task.CompletedTask;

    private static bool ShouldRun(Type innerType, BusType busType) =>
        busType switch
        {
            BusType.RabbitMQ => innerType.Name.Contains("Rabbit", StringComparison.OrdinalIgnoreCase),
            BusType.Kafka => innerType.Name.Contains("Kafka", StringComparison.OrdinalIgnoreCase),
            BusType.Redis => innerType.Name.Contains("Redis", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
}