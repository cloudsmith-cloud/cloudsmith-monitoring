// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Monitoring.Alerts;
using CloudSmith.Monitoring.Health;
using CloudSmith.Monitoring.Metrics;
using CloudSmith.Monitoring.Workers;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace CloudSmith.Monitoring;

public static class MonitoringExtensions
{
    /// <summary>
    /// Registers health probing, alert evaluation, OTel metrics, and the HealthMonitorWorker.
    /// Also call app.UseOpenTelemetryPrometheusScrapingEndpoint() in the host to expose /metrics.
    /// </summary>
    public static IServiceCollection AddCloudSmithMonitoring(
        this IServiceCollection services,
        Action<MonitoringOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<MonitoringOptions>(_ => { });

        // OTel meter
        services.AddOpenTelemetry()
            .WithMetrics(mb => mb
                .AddMeter(CloudSmithMetrics.MeterName)
                .AddPrometheusExporter());

        // Health plumbing
        services.AddHttpClient<HttpHealthProbe>()
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));
        services.AddSingleton<HealthAggregator>();
        services.AddSingleton<HealthSnapshotStore>();

        // Alert evaluators
        services.AddSingleton<IAlertEvaluator, HealthStatusAlertEvaluator>();

        // Background worker
        services.AddHostedService<HealthMonitorWorker>();

        return services;
    }
}
