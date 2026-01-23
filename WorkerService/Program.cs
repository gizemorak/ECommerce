using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using WorkerService.Consumers;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            builder.Services.AddHostedService<UserCreatedEventConsumer>();

            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });

            builder.Services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
            {
                ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

                RabbitMqBusService rabbitMqBus = new RabbitMqBusService(serviceBusOptions);

                rabbitMqBus.Init().Wait();
                return rabbitMqBus;
            });

            var host = builder.Build();
            host.Run();
        }
    }
}