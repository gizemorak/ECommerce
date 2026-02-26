using Bus.Shared.Events;
using Bus.Shared.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Bus.Shared.Abstract;

public abstract class BaseBusService : IBusService
{
    protected readonly ServiceBusOption _options;
    protected readonly ILogger _logger;
    
  protected BaseBusService(ServiceBusOption options, ILogger logger)
    {
        _options = options;
        _logger = logger;
    }

    // Abstract initialization method that implementations must provide
    public abstract Task Init();

    // ? Abstract methods that implementations must provide - with CancellationToken
    public abstract Task PublishAsync<T>(T message, string? topic = null, Dictionary<string, object>? headers = null, CancellationToken ct = default) where T : BaseEvent;
    
  // ? Legacy support - with proper signature
    public virtual Task Publish<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
    {
   return PublishAsync(message, null, headers, CancellationToken.None);
    }
    
    // Utility methods
  protected virtual string GetDefaultTopic<T>() where T : BaseEvent
    {
        return typeof(T).Name.ToLowerInvariant().Replace("event", "");
    }
    
    protected virtual async Task RetryAsync(Func<Task> operation, int maxRetries = 3)
    {
        var attempts = 0;
        Exception? lastException = null;
        
        while (attempts < maxRetries)
        {
   try
{
                await operation();
     return;
            }
 catch (Exception ex)
         {
         lastException = ex;
   attempts++;
  
      if (attempts >= maxRetries)
      break;
     
    _logger.LogWarning("Operation failed, attempt {Attempt}/{MaxRetries}: {Error}", 
            attempts, maxRetries, ex.Message);
      await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts))); // Exponential backoff
   }
        }
        
        throw lastException ?? new Exception("Operation failed after retries");
    }

    // ? Add overload with CancellationToken support for retry operations
    protected virtual async Task RetryAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default, int maxRetries = 3)
    {
        var attempts = 0;
        Exception? lastException = null;
        
 while (attempts < maxRetries && !cancellationToken.IsCancellationRequested)
        {
     try
  {
          await operation(cancellationToken);
                return;
     }
     catch (Exception ex) when (ex is not OperationCanceledException)
     {
   lastException = ex;
        attempts++;
      
     if (attempts >= maxRetries)
    break;
     
     _logger.LogWarning("Operation failed, attempt {Attempt}/{MaxRetries}: {Error}", 
            attempts, maxRetries, ex.Message);
     
       try
     {
     await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)), cancellationToken);
       }
          catch (OperationCanceledException)
     {
 throw; // Re-throw cancellation
      }
    }
    }
     
        cancellationToken.ThrowIfCancellationRequested();
    throw lastException ?? new Exception("Operation failed after retries");
    }

    // ? Add helper method for timeout operations
    protected virtual async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        
      try
        {
    return await task.WaitAsync(combinedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
  {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    // ? Add helper method for timeout operations (Task without return value)
  protected virtual async Task WithTimeoutAsync(Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
    using var timeoutCts = new CancellationTokenSource(timeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        
  try
        {
 await task.WaitAsync(combinedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    public Task<IChannel> CreateChannel()
    {
        throw new NotImplementedException();
    }

    public IDatabase GetDatabase(int db = 0)
    {
        throw new NotImplementedException();
    }
}