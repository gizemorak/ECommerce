using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using RedisApp.Servives;
using RedisApp.StreamConsumers;


namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            builder.Services.AddHostedService<OrderCreatedEventConsumer>();
            builder.Services.AddHostedService<PaymentAutoStartWorker>();

            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });

            builder.Services.AddSingleton<RedisService>(sp =>
            {
                var redisHost = builder.Configuration["RedisOption:Host"];
                var redisPort = builder.Configuration["RedisOption:Port"];
                return new RedisService(redisHost!, redisPort!);
            });

            var host = builder.Build();
            host.Run();
        }
    }
}