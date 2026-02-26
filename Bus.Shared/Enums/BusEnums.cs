namespace Bus.Shared.Enums;

public enum BusType
{
    RabbitMQ = 1,
    Kafka = 2,
    Redis = 3
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum DeliveryMode
{
    AtMostOnce = 0,     // Fire and forget
    AtLeastOnce = 1,    // Guaranteed delivery
    ExactlyOnce = 2     // Exactly once delivery (Kafka)
}