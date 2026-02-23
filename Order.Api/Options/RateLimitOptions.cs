namespace Order.Api.Options;

public class RateLimitOptions
{
 public int PermitLimit { get; set; } = 100;
 public int WindowInSeconds { get; set; } = 60;
 public int QueueLimit { get; set; } = 2;
}
