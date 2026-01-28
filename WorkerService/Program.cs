using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Queries.GetOrder;
using OrderApplication.Services;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;
using WorkerService.Consumers;
using Microsoft.EntityFrameworkCore;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            builder.Services.AddHostedService<UserCreatedEventConsumer>();

            builder.Services.AddScoped<IPaymentService,PaymentService>();

            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });

            builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssemblyContaining<GetByIdOrderCommandHandler>());


            string connectionString = builder.Configuration.GetConnectionString("Database");

            builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, optionsBuilder) =>
            {

                optionsBuilder.UseSqlServer(connectionString);

            });

    

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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