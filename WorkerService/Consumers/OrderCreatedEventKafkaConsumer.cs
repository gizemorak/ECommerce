using Bus.Shared;
using Bus.Shared.Enums;
using Bus.Shared.Events;
using Bus.Shared.ProducerSerializer;
using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using OrderApplication;
using OrderApplication.Orders.DTOs;
using OrderApplication.Services;
using OrderDomain;
using System.Text.Json;

public class OrderCreatedEventKafkaConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    : BackgroundService
{
    private IConsumer<int, OrderCreatedEvent> consumer;
    private readonly IBusService _busService =
         serviceProvider.GetRequiredKeyedService<IBusService>(BusType.Kafka);

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration.GetConnectionString("kafka"),
            GroupId = "orders-payment-checker",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        consumer = new ConsumerBuilder<int, OrderCreatedEvent>(consumerConfig)
                  .SetKeyDeserializer(new ConsumerDeserializer<int>())
                  .SetValueDeserializer(new ConsumerDeserializer<OrderCreatedEvent>()).Build();
        consumer.Subscribe("ordercreatedtopic");
        return base.StartAsync(cancellationToken);

    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        consumer.Close();
        consumer.Dispose();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await Task.Delay(2000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {


                var cr = consumer.Consume(stoppingToken);

                Console.WriteLine("Waiting for message...");


                if (cr == null || cr.Message == null)
                {
                    Console.WriteLine("ConsumeResult or Message is null");
                    continue;
                }



                if (cr.Message.Value == null)
                {
                    Console.WriteLine("Message.Value is null - deserialization may have failed");

                    consumer.Commit(cr);
                    continue;
                }

                var evt = cr.Message.Value;
                Console.WriteLine($"Processing OrderCreatedEvent: OrderId={evt.OrderId}, CustomerId={evt.CustomerId}, TotalAmount={evt.TotalAmount}");

                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();


                var getOrderCommand = new OrderApplication.Orders.Queries.GetOrder.GetByIdOrderCommand(evt.OrderId);
                ServiceResult<OrderDto> order = await mediator.Send(getOrderCommand);

                if (!order.IsSuccess || order.Data == null)
                {
                    Console.WriteLine($"Order not found for OrderId={evt.OrderId}");
                    consumer.Commit(cr);
                    continue;
                }

                var orderdata = order.Data;

                if (orderdata.OrderStatus == OrderStatusDto.Cancelled)
                {
                    Console.WriteLine($"Order {orderdata.OrderId} is already cancelled");
                    consumer.Commit(cr);
                    continue;
                }


                // if result success turn ack
                consumer.Commit(cr);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

}

