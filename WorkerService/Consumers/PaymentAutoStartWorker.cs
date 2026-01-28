using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderApplication;
using OrderApplication.Orders.DTOs;
using OrderApplication.Services;
using OrderDomain.Orders;
using RedisApp.Servives;
using StackExchange.Redis;

namespace RedisApp.StreamConsumers
{
    public class PaymentAutoStartWorker(RedisService redisService, IServiceProvider serviceProvider, ILogger<PaymentAutoStartWorker> logger)
        : BackgroundService
    {
        private IDatabase _db = default!;
        private const string AutoPayZsetKey = "orders:auto-pay";

        private const string PopDueLua = @"
local key = KEYS[1]
local now = tonumber(ARGV[1])
local n = tonumber(ARGV[2])

local items = redis.call('ZRANGEBYSCORE', key, '-inf', now, 'LIMIT', 0, n)
if (#items > 0) then
  redis.call('ZREM', key, unpack(items))
end
return items
";

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _db = redisService.GetDatabase();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int batchSize = 50;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    var result = await _db.ScriptEvaluateAsync(
                        PopDueLua,
                        new RedisKey[] { AutoPayZsetKey },
                        new RedisValue[] { now, batchSize });

                    var items = (RedisResult[])result;

                    if (items.Length == 0)
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    foreach (var item in items)
                    {
                        var orderId = item.ToString();
                        if (string.IsNullOrWhiteSpace(orderId))
                            continue;

                        using var scope = serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var getOrderQuery = new OrderApplication.Orders.Queries.GetOrder.GetByIdOrderCommand(Convert.ToInt32(orderId));

                        ServiceResult<OrderDto> orderResult = (ServiceResult<OrderDto>)await mediator.Send(getOrderQuery);
                        if (orderResult is null)
                        {
                            Console.WriteLine($"Order not found for OrderId={orderId}");
                           
                            continue;
                        }

                        var order = orderResult.Data;

                        if (order.OrderStatus == OrderStatusDto.Cancelled)
                        {
                            Console.WriteLine($"Order {order.OrderId} is already cancelled");
                        
                            continue;
                        }

                        await StartPaymentAsync(orderId,order.BuyerId,order.TotalPrice, stoppingToken);
                    }
                }

                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in PaymentAutoStartWorker loop");
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }

        private async Task StartPaymentAsync(string orderId,Guid buyerId, decimal totalPrice,CancellationToken ct)
        {
          

            logger.LogInformation("Auto-start payment triggered for OrderId={OrderId}", orderId);

            using var scope = serviceProvider.CreateScope();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            await paymentService.CheckPayment(Convert.ToInt32(orderId),buyerId,totalPrice);

            await Task.CompletedTask;
        }
    }
}
