using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplication.Orders.Queries.GetOrder;
using OrderApplication.Services;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;
using RedisApp.Servives;
using RedisApp.StreamConsumers;
using Microsoft.EntityFrameworkCore;


namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            builder.Services.AddHostedService<OrderCreatedEventConsumer>();
            builder.Services.AddHostedService<PaymentAutoStartWorker>();
            builder.Services.AddScoped<IPaymentService,PaymentService>();

            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetByIdOrderCommandHandler).Assembly));

            string connectionString = builder.Configuration.GetConnectionString("Database");

            builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, optionsBuilder) =>
            {

                optionsBuilder.UseSqlServer(connectionString);

            });

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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