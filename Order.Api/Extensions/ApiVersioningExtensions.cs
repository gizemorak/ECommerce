using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Api.Extensions;

public static class ApiVersioningExtensions
{
 public static IServiceCollection AddApiVersioning(this IServiceCollection services)
 {
 services.AddApiVersioning(options =>
 {
 options.DefaultApiVersion = new ApiVersion(1, 0);
 options.AssumeDefaultVersionWhenUnspecified = true;
 options.ReportApiVersions = true;

 options.ApiVersionReader = ApiVersionReader.Combine(
 new UrlSegmentApiVersionReader(),
 new HeaderApiVersionReader("x-api-version")
 );
 })
 .AddApiExplorer(options =>
 {
 options.GroupNameFormat = "'v'VVV";
 options.SubstituteApiVersionInUrl = true;
 });

 return services;
 }
}
