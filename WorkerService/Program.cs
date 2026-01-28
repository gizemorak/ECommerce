using Bus.Shared;
using Bus.Shared.Options;
using Bus.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Queries.GetOrder;
using OrderApplication.Services;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;


namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<OrderCreatedEventConsumer>();
            builder.Services.AddHostedService<PaymentSchedulerWorker>();

            builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssemblyContaining<GetByIdOrderCommandHandler>());

            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });

            string connectionString = builder.Configuration.GetConnectionString("Database");

            builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, optionsBuilder) =>
            {

                optionsBuilder.UseSqlServer(connectionString);

            });






            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            builder.Services.AddSingleton<KafkaService>();

            builder.Services.AddScoped<IPaymentService,PaymentService>();


            var host = builder.Build();
            host.Run();
        }
    }
}