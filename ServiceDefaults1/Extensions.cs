using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting
{
    // Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
    // This project should be referenced by each service project in your solution.
    // To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
    public static class Extensions
    {
        private const string HealthEndpointPath = "/health";
        private const string AlivenessEndpointPath = "/alive";

        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            // Uncomment the following to restrict the allowed schemes for service discovery.
            // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            // {
            //     options.AllowedSchemes = ["https"];
            // });

            return builder;
        }

        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(tracing =>
                            // Exclude health check requests from tracing
                            tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                        )
                        // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                        //.AddGrpcClientInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            // Aspire injects OTEL_EXPORTER_OTLP_ENDPOINT into services it runs.
            // If this is missing, the Dashboard will not receive traces/metrics.
            var configuredEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

            // To make local development smoother, enable OTLP exporter automatically in Development.
            // If the endpoint is provided (Aspire), explicitly use it.
            var shouldExport = builder.Environment.IsDevelopment() || !string.IsNullOrWhiteSpace(configuredEndpoint);

            if (shouldExport)
            {
                // Use the exporter extension overload available in this package version.
                // If Aspire provides an endpoint, pass it explicitly.
                if (!string.IsNullOrWhiteSpace(configuredEndpoint) && Uri.TryCreate(configuredEndpoint, UriKind.Absolute, out var uri))
                {
                    builder.Services.AddOpenTelemetry().UseOtlpExporter(OpenTelemetry.Exporter.OtlpExportProtocol.Grpc, uri);
                }
                else
                {
                    builder.Services.AddOpenTelemetry().UseOtlpExporter();
                }
            }

            // Log the resolved endpoint to make it obvious why the dashboard is empty.
            builder.Services.AddHostedService(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("OpenTelemetry");
                var env = sp.GetRequiredService<IHostEnvironment>();
                var endpoint = sp.GetRequiredService<IConfiguration>()["OTEL_EXPORTER_OTLP_ENDPOINT"];

                return new StartupLogHostedService(logger, env.EnvironmentName, endpoint);
            });

            return builder;
        }

        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }

        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            // Adding health checks endpoints to applications in non-development environments has security implications.
            // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
            if (app.Environment.IsDevelopment())
            {
                // All health checks must pass for app to be considered ready to accept traffic after starting
                app.MapHealthChecks(HealthEndpointPath);

                // Only health checks tagged with the "live" tag must pass for app to be considered alive
                app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live")
                });
            }

            return app;
        }

        private sealed class StartupLogHostedService(ILogger logger, string environmentName, string? otlpEndpoint) : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    logger.LogWarning("OpenTelemetry OTLP exporter enabled in {Environment}, but OTEL_EXPORTER_OTLP_ENDPOINT is not set. Aspire Dashboard will not receive telemetry unless an OTLP endpoint is configured.", environmentName);
                }
                else
                {
                    logger.LogInformation("OpenTelemetry OTLP endpoint: {OtlpEndpoint}", otlpEndpoint);
                }

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
