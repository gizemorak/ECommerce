using Bus.Shared.Enums;

namespace Bus.Shared.Models;

public class MessageWrapper<T>
{
    public T Payload { get; set; } = default!;
    public Dictionary<string, object> Headers { get; set; } = new();
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.AtLeastOnce;
    public string? ReplyTo { get; set; }
 public TimeSpan? Expiration { get; set; }
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public string? Source { get; set; }
    public string? Destination { get; set; }
    
    public MessageWrapper() { }
    
  public MessageWrapper(T payload)
 {
        Payload = payload;
  }
    
    public MessageWrapper(T payload, Dictionary<string, object>? headers = null) : this(payload)
  {
     if (headers != null)
      {
            Headers = headers;
        }
    }
    
    public void AddHeader(string key, object value)
    {
      Headers[key] = value;
    }
    
    public THeader? GetHeader<THeader>(string key)
    {
        if (Headers.TryGetValue(key, out var value) && value is THeader headerValue)
      {
   return headerValue;
 }
  return default;
    }
    
   public bool HasHeader(string key)
    {
   return Headers.ContainsKey(key);
   }
    
 public void IncrementRetryCount()
    {
     RetryCount++;
    }
    
    public bool CanRetry()
    {
  return RetryCount < MaxRetries;
    }
}