using Bus.Shared.Events;
using Bus.Shared.ProducerSerializer;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Bus.Shared.Services
{
    public class KafkaService(IConfiguration configuration)
    {
        public async Task CreateTopic(string topicName)
        {
            var config = new Confluent.Kafka.AdminClientConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            using var adminClient = new Confluent.Kafka.AdminClientBuilder(config).Build();

           
            var topicSpecification = new TopicSpecification()
            {
                Name = topicName,
                NumPartitions = 3,
                ReplicationFactor = 1 
           
            };


            await adminClient.CreateTopicsAsync([topicSpecification]);
        }



        public async Task SendMessage(string topic, OrderCreatedEvent orderCreatedEvent)
        {
          

            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                EnableIdempotence = true,
                RetryBackoffMs = 2000
            };

            using var producer = new ProducerBuilder<int, OrderCreatedEvent>(producerConfig)
                  .SetValueSerializer(new ProducerSerializer<OrderCreatedEvent>())
                  .SetKeySerializer(new ProducerSerializer<int>()).Build();


            var userId = Guid.NewGuid();

            var header = new Headers();
            header.Add("version", Encoding.UTF8.GetBytes("v1"));
            header.Add("content-type", Encoding.UTF8.GetBytes("json"));
            var kafkaMessage = new Message<int, OrderCreatedEvent>
            {
                Value = orderCreatedEvent,
                Key = orderCreatedEvent.OrderId,
                Headers = header
            };

            var deliveryResult = await producer.ProduceAsync(topic, kafkaMessage);

            Console.WriteLine(
                $"Message sent to topic {deliveryResult.Topic}, partition {deliveryResult.Partition}, offset {deliveryResult.Offset}");
        }




    }
}