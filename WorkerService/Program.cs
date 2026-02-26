using Bus.Shared;
using Bus.Shared.Enums;
using Bus.Shared.Extensions;
using Bus.Shared.Options;
using Bus.Shared.Publishers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Queries.GetOrder;
using OrderApplication.Services;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;
using RedisApp.StreamConsumers;
using WorkerService.Consumers;
using WorkerService.Extensions;
using WorkerService.Services;

var builder = Host.CreateApplicationBuilder(args);

// ? Add bus services first (without immediate initialization)
builder.Services.AddBusServices(builder.Configuration, BusType.RabbitMQ);

// ? Add bus initialization service to initialize connections after startup
builder.Services.AddHostedService<BusInitializationService>();

// Add consumer services
builder.Services.AddHostedService<OrderCreatedEventRabbitConsumer>();
builder.Services.AddHostedService<OrderCreatedEventKafkaConsumer>();
builder.Services.AddHostedService<PaymentAutoStartKafkaWorker>();
builder.Services.AddHostedService<OrderCreatedEventRedisConsumer>();
builder.Services.AddHostedService<PaymentAutoStartRedisWorker>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationDbContext(builder.Configuration);

builder.AddKafkaProducer<string, string>("kafka");

var host = builder.Build();
host.Run();