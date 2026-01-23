
using Bus.Shared;
using Bus.Shared.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Commands.CancelOrder;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplication.Orders.Queries.GetOrder;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;

namespace Order.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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


            builder.Services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssemblyContaining<GetByIdOrderCommandHandler>());



            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
            {
                ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();

                RabbitMqBusService rabbitMqBus = new RabbitMqBusService(serviceBusOptions);
                rabbitMqBus.Init().Wait();
                rabbitMqBus.CreateExchanges().Wait();
                return rabbitMqBus;
            });

        
            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();



            app.MapPost("/sendorder", async (CreateOrderCommand request,IMediator mediator) =>
            {

                await mediator.Send(request);

                return Results.Ok();
               
            })
            .WithName("SendOrder")
            .WithOpenApi();

            app.MapPost("/cancelorder", async (CancelOrderCommand request, IMediator mediator) =>
            {
                await mediator.Send(request);

                return Results.Ok();

            })
          .WithName("CancelOrder")
          .WithOpenApi();

            app.Run();
        }
    }
}
