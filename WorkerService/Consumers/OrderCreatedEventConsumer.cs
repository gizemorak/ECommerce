using Bus.Shared.Events;
using Bus.Shared.ProducerSerializer;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderApplication;
using OrderApplication.Orders.DTOs;
using System.Text.Json;

public class OrderCreatedEventConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    : BackgroundService
{
    private IConsumer<int, OrderCreatedEvent> consumer;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
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
                    // Still commit to avoid reprocessing
                    consumer.Commit(cr);
                    continue;
                }

                var evt = cr.Message.Value;
                Console.WriteLine($"Processing OrderCreatedEvent: OrderId={evt.OrderId}, CustomerId={evt.CustomerId}, TotalAmount={evt.TotalAmount}");

                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var getOrderQuery = new OrderApplication.Orders.Queries.GetOrder.GetByIdOrderCommand(evt.OrderId);

                ServiceResult<OrderDto> orderResult = (ServiceResult<OrderDto>)await mediator.Send(getOrderQuery);
                if (orderResult is null)
                {
                    Console.WriteLine($"Order not found for OrderId={evt.OrderId}");
                    consumer.Commit(cr);
                    continue;
                }

                var order = orderResult.Data;

                if (order.OrderStatus == OrderStatusDto.Cancelled)
                {
                    Console.WriteLine($"Order {order.OrderId} is already cancelled");
                    consumer.Commit(cr);
                    continue;
                }

                var nowUtc = DateTime.UtcNow;

                if (order.PaymentDueAtUtc > nowUtc)
                {
                    Console.WriteLine($"Order {order.OrderId} payment not due yet. Due at: {order.PaymentDueAtUtc}");
                    consumer.Commit(cr);
                    continue;
                }

                // todo  send payment request
                //var result=await mediator.Send(new RequestPaymentCommand(order.Id), stoppingToken);

                Console.WriteLine($"Payment request sent for OrderId={orderResult.Data.OrderId}");

                // if result success turn ack
                consumer.Commit(cr);
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"ConsumeException: {ex.Error.Reason}, IsFatal: {ex.Error.IsFatal}, Code: {ex.Error.Code}");
                if (ex.Error.IsFatal)
                {
                    await Task.Delay(5000, stoppingToken);
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Consumer operation cancelled");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

}
