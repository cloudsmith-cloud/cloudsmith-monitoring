// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Monitoring.Alerts;
using CloudSmith.Monitoring.Health;
using CloudSmith.Monitoring.Metrics;
using CloudSmith.Sdk;
using CloudSmith.Sdk.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudSmith.Monitoring.Workers;

public sealed class HealthMonitorWorker : BackgroundService
{
    private readonly HttpHealthProbe     _probe;
    private readonly HealthAggregator    _aggregator;
    private readonly HealthSnapshotStore _store;
    private readonly IEnumerable<IAlertEvaluator> _evaluators;
    private readonly IPlatformEventBus   _events;
    private readonly MonitoringOptions   _opts;
    private readonly ILogger<HealthMonitorWorker> _logger;

    public HealthMonitorWorker(
        HttpHealthProbe probe,
        HealthAggregator aggregator,
        HealthSnapshotStore store,
        IEnumerable<IAlertEvaluator> evaluators,
        IPlatformEventBus events,
        IOptions<MonitoringOptions> opts,
        ILogger<HealthMonitorWorker> logger)
    {
        _probe      = probe;
        _aggregator = aggregator;
        _store      = store;
        _evaluators = evaluators;
        _events     = events;
        _opts       = opts.Value;
        _logger     = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("CS-MON-INFO-001: HealthMonitorWorker started. Polling interval: {Interval}s",
            _opts.HealthCheckIntervalSeconds);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CS-MON-ERR-001: Unhandled error in health monitor cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_opts.HealthCheckIntervalSeconds), ct);
        }
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        CloudSmithMetrics.HealthCheckRuns.Add(1);

        // Probe all configured endpoints in parallel
        var tasks = _opts.Endpoints.Select(e =>
            _probe.ProbeAsync(e.Name, e.ComponentType, e.HealthUrl, ct));
        var results = (IReadOnlyList<ComponentHealthResult>)(await Task.WhenAll(tasks)).ToList();

        var snapshot = _aggregator.Aggregate(results);
        _store.Update(snapshot);

        if (snapshot.OverallStatus != ComponentHealthStatus.Healthy)
        {
            CloudSmithMetrics.HealthCheckFailures.Add(1);
            _logger.LogWarning("CS-MON-WARN-002: System health is {Status}", snapshot.OverallStatus);
        }

        // Run alert evaluators (they read from _store which we just updated)
        foreach (var evaluator in _evaluators)
        {
            try
            {
                var alert = await evaluator.EvaluateAsync(ct);
                if (alert is not null)
                {
                    CloudSmithMetrics.AlertsFired.Add(1);
                    await _events.PublishAsync(alert, ct);
                    _logger.LogWarning("CS-MON-ALERT-{RuleId}: [{Severity}] {Message}",
                        alert.RuleId, alert.Severity, alert.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CS-MON-ERR-002: Alert evaluator '{RuleId}' threw", evaluator.RuleId);
            }
        }
    }
}
