using Bus.Shared;
using Bus.Shared.Events;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;

namespace RedisApp.Servives;

public class RedisService:IBusService
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private readonly IConfiguration _configuration;

    public RedisService(IConfiguration configuration)
    {

        _configuration = configuration;


        _connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis") ?? configuration["ServiceBusOption:RedisConnectionString"]
                            ?? "localhost:9092");
    }

    public async Task Init()
    {
        // Redis connection is established in constructor
        // Just verify the connection is working
        var db = GetDatabase();
        await db.PingAsync();
        Console.WriteLine("✅ Redis service initialized");
    }

    public Task<IChannel> CreateChannel()
    {
        throw new NotImplementedException();
    }

    public IDatabase GetDatabase(int db = 0)
    {
        return _connectionMultiplexer.GetDatabase(db);
    }

    public ISubscriber GetSubscriber()
    {
        return _connectionMultiplexer.GetSubscriber();
    }

    public Task Publish<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
    {
        throw new NotImplementedException();
    }

    public async Task PublishAsync<T>(T message, string? topic = null, Dictionary<string, object>? headers = null, CancellationToken ct = default) where T : BaseEvent
    {
        var db = GetDatabase();

        var maxRetries = 3;
        var retryCount = 0;
        RedisValue messageId = RedisValue.Null;

        var eventId = Guid.NewGuid().ToString("N");

        NameValueEntry[] streamEntries;

        if (message is OrderCreatedEvent orderEvent)
        {
            streamEntries = new NameValueEntry[]
            {
                new("order", JsonSerializer.Serialize(orderEvent)),
                new("eventId", eventId),
                new("created_date", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
            };
        }
        else
        {
            throw new ArgumentException($"Unsupported event type: {typeof(T).Name}");
        }

        while (retryCount < maxRetries)
        {
            try
            {
                messageId = await db.StreamAddAsync("ordercreated", streamEntries, null, null, false);

                if (messageId.HasValue)
                {
                    break;
                }

                retryCount++;
                if (retryCount >= maxRetries)
                {
                    throw new Exception("message can not send redis stream");
                }
            }
            catch (Exception)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    throw;
                }

                await Task.Delay(100 * retryCount);
            }
        }
    }
}