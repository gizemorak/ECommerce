using Bus.Shared;
using Bus.Shared.Enums;
using Bus.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using RedisApp.Servives;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisApp.StreamConsumers
{
    public class OrderCreatedEventRedisConsumer(ILogger<OrderCreatedEventRedisConsumer> logger, IServiceProvider serviceProvider)
        : BackgroundService
    {
        private IDatabase db;

        private readonly IBusService _busService =
           serviceProvider.GetRequiredKeyedService<IBusService>(BusType.Redis);

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            db = _busService.GetDatabase();


            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var streamName = "ordercreated";
            var consumerGroupName = "myconsumergroup-1";
            var consumerName = "orderconsumer";
            var processedEventsKey = "ordercreated:processed-events";


            var streamExists = await db.KeyExistsAsync(streamName);
            if (!streamExists)
            {
                await db.StreamAddAsync(streamName, "init", "stream-created");
                logger.LogInformation("Stream {StreamName} created", streamName);
            }

            var groups = await db.StreamGroupInfoAsync(streamName);

            if (groups.All(g => g.Name != consumerGroupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, consumerGroupName, StreamPosition.Beginning);
                logger.LogInformation("Consumer group {ConsumerGroup} created on stream {StreamName}",
                    consumerGroupName, streamName);
            }
            else
            {
                logger.LogInformation("Consumer group {ConsumerGroup} already exists on stream {StreamName}",
                    consumerGroupName, streamName);
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var entries = await db.StreamReadGroupAsync(
                        streamName, consumerGroupName, consumerName, ">", 2,
                        CommandFlags.DemandMaster);




                    foreach (var entry in entries)
                    {


                        try
                        {

                            var eventIdVal = entry.Values.FirstOrDefault(v => v.Name == "eventId").Value;
                            var eventId = eventIdVal.ToString();

                            var firstTime = await db.SetAddAsync(processedEventsKey, eventId);
                            if (!firstTime)
                            {
                                logger.LogInformation(
                                    "Duplicate event skipped. EventId={EventId} EntryId={EntryId}",
                                    eventId, entry.Id);

                                await db.StreamAcknowledgeAsync(streamName, consumerGroupName, entry.Id);
                                continue;
                            }
                            foreach (var item in entry.Values)
                            {

                                if (item.Name == "order")
                                {
                                    var orderCreatedEvent =
                                            JsonSerializer.Deserialize<OrderCreatedEvent>(item.Value.ToString());

                                    if (orderCreatedEvent != null)
                                    {

                                        var zsetKey = "orders:auto-pay";
                                        var dueAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
                                        await db.SortedSetAddAsync(zsetKey, orderCreatedEvent.OrderId.ToString(), dueAt);

                                        logger.LogInformation(
                                            "Scheduled auto-payment. OrderId={OrderId} DueAtUtc={DueAtUtc}",
                                            orderCreatedEvent.OrderId,
                                            DateTimeOffset.FromUnixTimeSeconds(dueAt));
                                    }
                                }







                            }

                            await db.StreamAcknowledgeAsync(streamName, consumerGroupName, entry.Id);
                        }

                        catch (Exception e)
                        {
                            logger.LogError(e, e.Message);
                            // optionally: do NOT rethrow so one bad message doesn't kill the service
                        }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Normal during shutdown from Task.Delay / redis calls
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception in OrderCreatedEventConsumer loop.");
                    // Optional: small backoff to avoid tight error loop
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}