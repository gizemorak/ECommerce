using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Bus.Shared.Events;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace Bus.Shared.Publishers;

public class KafkaService : IBusService
{
    private readonly IConfiguration _configuration;
    private readonly string _bootstrapServers;
    private readonly HashSet<string> _ensuredTopics = new();
    private readonly SemaphoreSlim _topicLock = new(1, 1);

    public KafkaService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // ✅ Priority: Aspire connection string > appsettings > default
        _bootstrapServers = configuration.GetConnectionString("kafka") 
                            ?? configuration["ServiceBusOption:KafkaConnectionString"]
                            ?? "localhost:9092";
                            
        Console.WriteLine($"🔍 Kafka bootstrap servers: {_bootstrapServers}");
    }

    public async Task Init()
    {
        // Kafka doesn't require explicit initialization like RabbitMQ
        // Connection is established when first needed
        await Task.CompletedTask;
        Console.WriteLine("✅ Kafka service initialized");
    }

    public Task<RabbitMQ.Client.IChannel> CreateChannel()
    {
        throw new NotImplementedException();
    }

    public async Task CreateTopic(string topicName)
    {
        if (_ensuredTopics.Contains(topicName))
        {
            return;
        }

        await _topicLock.WaitAsync();
        try
        {
            if (_ensuredTopics.Contains(topicName))
            {
                return;
            }

            var config = new AdminClientConfig
            {
                BootstrapServers = _bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var topicSpecification = new TopicSpecification
            {
                Name = topicName,
                NumPartitions = 3,
                ReplicationFactor = 1
            };

            try
            {
                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
                Console.WriteLine($"✅ Topic '{topicName}' created successfully");
                _ensuredTopics.Add(topicName);
            }
            catch (CreateTopicsException ex)
            {
                var allTopicsExist = ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists);
                
                if (allTopicsExist)
                {
                    Console.WriteLine($"ℹ️ Topic '{topicName}' already exists");
                    _ensuredTopics.Add(topicName);
                }
                else
                {
                    foreach (var result in ex.Results)
                    {
                        Console.WriteLine($"❌ Topic error: {result.Topic} - {result.Error.Reason}");
                    }
                    throw;
                }
            }
        }
        finally
        {
            _topicLock.Release();
        }
    }

    public IDatabase GetDatabase(int db = 0)
    {
        throw new NotImplementedException();
    }

    public async Task Publish<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
    {
        await PublishAsync(message, null, headers, CancellationToken.None);
    }

    public async Task PublishAsync<T>(T message, string? topic = null, Dictionary<string, object>? headers = null, CancellationToken ct = default) 
        where T : BaseEvent
    {
        ct.ThrowIfCancellationRequested();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            EnableIdempotence = true,
            RetryBackoffMs = 2000,
            CompressionType = CompressionType.Snappy,
            SocketTimeoutMs = 60000,
            RequestTimeoutMs = 30000
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig)
            .SetKeySerializer(Confluent.Kafka.Serializers.Utf8)
            .SetValueSerializer(Confluent.Kafka.Serializers.Utf8)
            .SetErrorHandler((_, e) => Console.WriteLine($"❌ Kafka Error: {e.Reason}"))
            .Build();

        var topicName = topic ?? GetDefaultTopicName<T>();
        await CreateTopic(topicName);

        var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var messageKey = GenerateMessageKey(message);

        var kafkaHeaders = new Headers
        {
            { "version", Encoding.UTF8.GetBytes("v1") },
            { "content-type", Encoding.UTF8.GetBytes("application/json") },
            { "message-type", Encoding.UTF8.GetBytes(typeof(T).Name) },
            { "timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) }
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                var headerValue = header.Value switch
                {
                    string str => Encoding.UTF8.GetBytes(str),
                    byte[] bytes => bytes,
                    _ => Encoding.UTF8.GetBytes(header.Value.ToString() ?? string.Empty)
                };
                kafkaHeaders.Add(header.Key, headerValue);
            }
        }

        var kafkaMessage = new Message<string, string>
        {
            Value = messageJson,
            Key = messageKey,
            Headers = kafkaHeaders,
            Timestamp = new Timestamp(DateTimeOffset.UtcNow)
        };

        try
        {
            var deliveryResult = await producer.ProduceAsync(topicName, kafkaMessage, ct);

            Console.WriteLine(
                $"✅ Message sent to '{deliveryResult.Topic}' " +
                $"[partition: {deliveryResult.Partition}, offset: {deliveryResult.Offset}]");
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"❌ Failed to send message: {ex.Error.Reason}");
            throw;
        }
    }

    private string GenerateMessageKey<T>(T message) where T : BaseEvent
    {
        if (message is OrderCreatedEvent orderEvent)
        {
            return $"order-{orderEvent.OrderId}";
        }

        return Guid.NewGuid().ToString();
    }

    private string GetDefaultTopicName<T>() where T : BaseEvent
    {
        var typeName = typeof(T).Name;
        if (typeName.EndsWith("Event"))
        {
            typeName = typeName[..^5];
        }

        return string.Concat(typeName.Select((x, i) =>
            i > 0 && char.IsUpper(x) ? "-" + x.ToString().ToLowerInvariant()
            : x.ToString().ToLowerInvariant()));
    }
}

