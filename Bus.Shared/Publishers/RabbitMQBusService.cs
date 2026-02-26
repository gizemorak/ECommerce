using Bus.Shared;
using Bus.Shared.Abstract;
using Bus.Shared.Events;
using Bus.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text.Json;
using Polly;

namespace Bus.Shared.Publishers
{
    public class RabbitMqBusService(IConfiguration _configuration) : IBusService
    {
        private IChannel? _channelWithAck;
        public static Dictionary<object, string> ExchangeList = new();
      

        static RabbitMqBusService()
        {
            ExchangeList.Add(typeof(OrderCreatedEvent), GetExchangeName<OrderCreatedEvent>());
        }


        private IConnection? _connection;

            public async Task Init()
            {
            // ✅ Get connection string from Aspire (injected by .WithReference(rabbitmq))
            var aspireConnectionString = _configuration.GetConnectionString("rabbitmq");

            // ✅ Use Aspire connection string if available, otherwise fall back to ServiceBusOption
            var connectionString = aspireConnectionString;
            
            Console.WriteLine($"🔍 RabbitMQ connection string: {connectionString}");
            
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(connectionString) // ✅ Use the resolved connection string
            };
            
            _connection = await connectionFactory.CreateConnectionAsync();
            _channelWithAck = await _connection!.CreateChannelAsync(new CreateChannelOptions(true, true));

            }


        // Add this policy (as a field so it’s reused, not recreated per message)
        private static readonly ResiliencePipeline PublishRetryPipeline =
            new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = args =>
                    {
                        // attemptNumber starts at 0
                        var delaySeconds = Math.Pow(2, args.AttemptNumber); // 1,2,4
                        return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(delaySeconds));
                    }
                })
                .Build();

        public async Task PublishAsync<T>(T message, string? topic = null, Dictionary<string, object>? headers = null, CancellationToken ct = default) where T : BaseEvent
        {

            string eventAsJsonData = JsonSerializer.Serialize(message);

            byte[] body = System.Text.Encoding.UTF8.GetBytes(eventAsJsonData);



            BasicProperties properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString()
            };

            if (headers is not null) properties.Headers = headers!;

            await PublishRetryPipeline.ExecuteAsync(async token =>
            {
                if (_channelWithAck is null)
                    throw new InvalidOperationException("RabbitMQ channel is not initialized. Call Init() first.");

                await _channelWithAck.BasicPublishAsync(
                    exchange: "",
                    routingKey: "delayqueue",
                    mandatory: true,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: token);

            }, ct);
        }

        public static string GetExchangeName<T>()
        {
            return $"{typeof(T).Name.ToLower()}-exchange";
        }

        public async Task CreateExchanges()
        {


            IChannel channel = await _connection!.CreateChannelAsync();
            foreach (KeyValuePair<object, string> exchange in ExchangeList)
            {
                await channel.ExchangeDeclareAsync(exchange.Value, ExchangeType.Topic, true, false, null);
            }
            await channel.DisposeAsync();
        }

        public Task<IChannel> CreateChannel()
        {
            return _connection!.CreateChannelAsync();
        }

        public Task Publish<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
        {
            throw new NotImplementedException();
        }

        public IDatabase GetDatabase(int db = 0)
        {
            throw new NotImplementedException();
        }
    }
}
