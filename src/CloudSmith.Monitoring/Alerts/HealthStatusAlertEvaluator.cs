// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Monitoring.Health;

namespace CloudSmith.Monitoring.Alerts;

/// <summary>
/// Fires a Critical alert when any component in the latest health snapshot is Unhealthy,
/// or a Warning when any component is Degraded. Reads from HealthSnapshotStore.
/// </summary>
public sealed class HealthStatusAlertEvaluator : IAlertEvaluator
{
    private readonly HealthSnapshotStore _store;

    public string        RuleId             => "CS-ALERT-001";
    public AlertSeverity Severity           => AlertSeverity.Critical;
    public TimeSpan      EvaluationInterval => TimeSpan.FromSeconds(30);

    public HealthStatusAlertEvaluator(HealthSnapshotStore store) => _store = store;

    public Task<AlertEvent?> EvaluateAsync(CancellationToken ct = default)
    {
        var snapshot = _store.Latest;
        if (snapshot is null ||
            snapshot.OverallStatus is ComponentHealthStatus.Healthy or ComponentHealthStatus.Unknown)
            return Task.FromResult<AlertEvent?>(null);

        var severity = snapshot.OverallStatus == ComponentHealthStatus.Unhealthy
            ? AlertSeverity.Critical
            : AlertSeverity.Warning;

        var degraded = snapshot.Components
            .Where(c => c.Status is ComponentHealthStatus.Unhealthy or ComponentHealthStatus.Degraded)
            .Select(c => c.ComponentName)
            .ToList();

        var labels = new Dictionary<string, string>
        {
            ["overall_status"] = snapshot.OverallStatus.ToString(),
            ["components"]     = string.Join(",", degraded),
        };

        return Task.FromResult<AlertEvent?>(new AlertEvent(
            RuleId,
            "System Health Degraded",
            severity,
            AlertState.Firing,
            $"Components degraded/unhealthy: {string.Join(", ", degraded)}",
            DateTimeOffset.UtcNow,
            labels));
    }
}
