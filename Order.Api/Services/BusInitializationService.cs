using Bus.Shared;
using Bus.Shared.Publishers;
using RedisApp.Servives;

namespace Order.Api.Services;

public class BusInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusInitializationService> _logger;

    public BusInitializationService(IServiceProvider serviceProvider, ILogger<BusInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Waiting for infrastructure to be ready...");
            
            // Wait a bit to ensure infrastructure is ready before initializing connections
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            _logger.LogInformation("Initializing bus services...");

         // Initialize RabbitMQ
        var rabbitMqService = _serviceProvider.GetRequiredService<RabbitMqBusService>();
   await rabbitMqService.Init();
await rabbitMqService.CreateExchanges();
      _logger.LogInformation("RabbitMQ service initialized successfully");

         // Initialize other bus services if needed
   var kafkaService = _serviceProvider.GetRequiredService<KafkaService>();
     await kafkaService.Init();
     _logger.LogInformation("Kafka service initialized successfully");

  var redisService = _serviceProvider.GetRequiredService<RedisService>();
 await redisService.Init();
    _logger.LogInformation("Redis service initialized successfully");
            
      _logger.LogInformation("All bus services initialized successfully");
        }
        catch (OperationCanceledException)
   {
            _logger.LogInformation("Bus initialization was cancelled");
     }
        catch (Exception ex)
   {
            _logger.LogError(ex, "Failed to initialize bus services");
 // Don't throw - allow the service to continue running
        }
    }
}