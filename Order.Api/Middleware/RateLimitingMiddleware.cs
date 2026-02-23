using Order.Api.Options;

namespace Order.Api.Middleware;

public class RateLimitingMiddleware
{
 private readonly RequestDelegate _next;
 private readonly RateLimitOptions _rateLimitOptions;
 private static readonly Dictionary<string, (DateTime ResetTime, int Count)> ClientRequests = new();

 public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions rateLimitOptions)
 {
 _next = next;
 _rateLimitOptions = rateLimitOptions;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 var clientId = GetClientId(context);
 var now = DateTime.UtcNow;

 if (!ClientRequests.ContainsKey(clientId))
 {
 ClientRequests[clientId] = (now.AddSeconds(_rateLimitOptions.WindowInSeconds), 1);
 await _next(context);
 return;
 }

 var (resetTime, count) = ClientRequests[clientId];

 if (now > resetTime)
 {
 ClientRequests[clientId] = (now.AddSeconds(_rateLimitOptions.WindowInSeconds), 1);
 await _next(context);
 return;
 }

 if (count >= _rateLimitOptions.PermitLimit)
 {
 context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
 context.Response.ContentType = "application/json";
 await context.Response.WriteAsJsonAsync(new
 {
 error = "Too many requests",
 message = $"Rate limit exceeded. Maximum {_rateLimitOptions.PermitLimit} requests per {_rateLimitOptions.WindowInSeconds} seconds allowed",
 retryAfter = (int)(resetTime - now).TotalSeconds
 });
 return;
 }

 ClientRequests[clientId] = (resetTime, count + 1);
 await _next(context);
 }

 private static string GetClientId(HttpContext context)
 {
 var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
 if (!string.IsNullOrEmpty(userId))
 {
 return userId;
 }

 return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
 }
}
