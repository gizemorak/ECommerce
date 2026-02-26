using Bus.Shared.Enums;
using Bus.Shared.Options;
using Bus.Shared.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bus.Shared.Factories;

public interface IBusServiceFactory
{
    IBusService CreateBusService(BusType busType);
    IBusService CreateBusService(ServiceBusOption options);
}

public class BusServiceFactory : IBusServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusServiceFactory> _logger;
    
    public BusServiceFactory(IServiceProvider serviceProvider, ILogger<BusServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public IBusService CreateBusService(BusType busType)
    {
      var options = _serviceProvider.GetRequiredService<ServiceBusOption>();
 options.BusType = busType;
        return CreateBusService(options);
    }
    
    public IBusService CreateBusService(ServiceBusOption options)
    {
        return options.BusType switch
    {
     BusType.RabbitMQ => _serviceProvider.GetRequiredService<RabbitMqBusService>(),
      BusType.Kafka => CreateKafkaBusService(options),
            BusType.Redis => CreateRedisBusService(options),
            _ => throw new NotSupportedException($"Bus type {options.BusType} is not supported")
 };
    }
 
    private IBusService CreateKafkaBusService(ServiceBusOption options)
    {
  // This will be implemented when we create KafkaBusService
        throw new NotImplementedException("KafkaBusService will be implemented in kafka branch");
    }

    private IBusService CreateRedisBusService(ServiceBusOption options)
    {
  // This will be implemented when we create RedisBusService
        throw new NotImplementedException("RedisBusService will be implemented in redis branch");
 }
}