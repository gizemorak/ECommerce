using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Order.Api.Endpoints;
using Order.Api.Middleware;

namespace Order.Api.Extensions;

public static class ApplicationBuilderExtensions
{
 public static WebApplication UseApi(this WebApplication app)
 {
 if (app.Environment.IsDevelopment())
 {
 app.UseSwagger();
 app.UseSwaggerUI();
 }

 app.UseHttpsRedirection();

 app.UseMiddleware<RateLimitingMiddleware>();

 //app.UseAuthentication();
 //app.UseAuthorization();

 app.MapHealthChecks("/health", new HealthCheckOptions
 {
 ResponseWriter = WriteHealthCheckResponse
 });

 app.MapHealthChecks("/health/ready", new HealthCheckOptions
 {
 Predicate = healthCheck => healthCheck.Tags.Contains("ready")
 });

 app.MapOrderEndpoints();

 return app;
 }

 private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
 {
 context.Response.ContentType = "application/json";

 var response = new
 {
 status = report.Status.ToString(),
 checks = report.Entries.Select(e => new
 {
 name = e.Key,
 status = e.Value.Status.ToString(),
 description = e.Value.Description
 })
 };

 await context.Response.WriteAsJsonAsync(response);
 }
}
