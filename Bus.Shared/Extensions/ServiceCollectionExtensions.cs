using Bus.Shared.Enums;
using Bus.Shared.Options;
using Bus.Shared.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedisApp.Servives;

namespace Bus.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusServices(this IServiceCollection services, IConfiguration configuration, BusType? defaultBusType = null)
    {
     // Configure options from config
    services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

// Add options as singleton for factory and override connection strings from Aspire
        services.AddSingleton<ServiceBusOption>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOption>>().Value;

            // Let caller override BusType if desired
        if (defaultBusType.HasValue)
            {
   options.BusType = defaultBusType.Value;
            }

       // ✅ OVERRIDE RabbitMQ connection string from Aspire (if present)
            // AppHost: .WithReference(rabbitMq) -> injects ConnectionStrings:rabbitmq
            var aspireRabbit = config.GetConnectionString("rabbitmq");
            if (!string.IsNullOrWhiteSpace(aspireRabbit))
        {
      options.RabbitMqConnectionString = aspireRabbit;
            }

            // (Optional) same pattern for Kafka / Redis if needed:
     var aspireKafka = config.GetConnectionString("kafka");
       if (!string.IsNullOrWhiteSpace(aspireKafka))
    {
   options.KafkaConnectionString = aspireKafka;
   }

   var aspireRedis = config.GetConnectionString("redis");
 if (!string.IsNullOrWhiteSpace(aspireRedis))
        {
      options.RedisConnectionString = aspireRedis; // or Host/Port, depending on your RedisOptions
            }

  return options;
        });

        // ✅ RabbitMQ using Aspire-populated ServiceBusOption
 services.AddKeyedSingleton<IBusService, RabbitMqBusService>(BusType.RabbitMQ, (sp, key) =>
        {
        var config = sp.GetRequiredService<IConfiguration>(); // now contains Aspire connection string
       var service = new RabbitMqBusService(config);

    // init on startup (consider making these async-friendly later)
   service.Init().Wait();
   service.CreateExchanges().Wait();

     return service;
        });

        // Kafka & Redis can stay as you already had them
        services.AddKeyedSingleton<IBusService, KafkaService>(BusType.Kafka, (sp, key) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new KafkaService(config);
    });

        services.AddKeyedSingleton<IBusService, RedisService>(BusType.Redis, (sp, key) =>
     {
            var config = sp.GetRequiredService<IConfiguration>();
 
            return new RedisService(config);
        }); 

    services.AddSingleton<IBusService>(sp =>
        {
        var options = sp.GetRequiredService<ServiceBusOption>();
     return sp.GetRequiredKeyedService<IBusService>(options.BusType);
        });

        services.AddSingleton<RabbitMqBusService>(sp =>
      (RabbitMqBusService)sp.GetRequiredKeyedService<IBusService>(BusType.RabbitMQ));

        services.AddSingleton<KafkaService>(sp =>
            (KafkaService)sp.GetRequiredKeyedService<IBusService>(BusType.Kafka));

        services.AddSingleton<RedisService>(sp =>
       (RedisService)sp.GetRequiredKeyedService<IBusService>(BusType.Redis));

        return services;
  }

public static IServiceCollection AddRabbitMQBus(this IServiceCollection services, IConfiguration configuration)
    {
   return services.AddBusServices(configuration, BusType.RabbitMQ);
    }

    public static IServiceCollection AddKafkaBus(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddBusServices(configuration, BusType.Kafka);
    }

    public static IServiceCollection AddRedisBus(this IServiceCollection services, IConfiguration configuration)
    {
     return services.AddBusServices(configuration, BusType.Redis);
    }

    public static T GetBusService<T>(this IServiceProvider serviceProvider, BusType busType) where T : class, IBusService
    {
        return (T)serviceProvider.GetRequiredKeyedService<IBusService>(busType);
    }
}