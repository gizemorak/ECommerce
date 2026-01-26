using StackExchange.Redis;
using RedisApp.Servives;

namespace RedisApp.StreamConsumers
{
    public class PaymentAutoStartWorker(RedisService redisService, ILogger<PaymentAutoStartWorker> logger)
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



                        await StartPaymentIfEligibleAsync(orderId, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // normal shutdown
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in PaymentAutoStartWorker loop");
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }

        private async Task StartPaymentIfEligibleAsync(string orderId, CancellationToken ct)
        {
            // IMPORTANT: Here you MUST protect against user cancel / already paid.
            // Best practice:
            // - In DB do: UPDATE Orders SET Status='PaymentStarting'
            //   WHERE Id=@id AND Status='WaitingPayment'
            // - If affected rows == 1 => you own payment start
            // - Else => do nothing

            logger.LogInformation("Auto-start payment triggered for OrderId={OrderId}", orderId);

            // TODO: call your payment start process here
            // await _paymentService.StartPaymentAsync(Guid.Parse(orderId), ct);

            await Task.CompletedTask;
        }
    }
}
