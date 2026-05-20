// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Monitoring.Alerts;
using CloudSmith.Monitoring.Health;
using Xunit;

namespace CloudSmith.Monitoring.Tests.Health;

public class HealthStatusAlertEvaluatorTests
{
    private static ComponentHealthResult Make(string name, ComponentHealthStatus status) =>
        new(name, "service", status, null, DateTimeOffset.UtcNow, TimeSpan.Zero);

    private static HealthSnapshotStore StoreWith(params ComponentHealthResult[] components)
    {
        var aggregator = new HealthAggregator();
        var store      = new HealthSnapshotStore();
        store.Update(aggregator.Aggregate(components));
        return store;
    }

    [Fact]
    public async Task AllHealthy_ReturnsNull()
    {
        var evaluator = new HealthStatusAlertEvaluator(
            StoreWith(Make("api", ComponentHealthStatus.Healthy)));
        Assert.Null(await evaluator.EvaluateAsync());
    }

    [Fact]
    public async Task NoSnapshot_ReturnsNull()
    {
        var evaluator = new HealthStatusAlertEvaluator(new HealthSnapshotStore());
        Assert.Null(await evaluator.EvaluateAsync());
    }

    [Fact]
    public async Task Degraded_ReturnsWarningAlert()
    {
        var evaluator = new HealthStatusAlertEvaluator(
            StoreWith(Make("db", ComponentHealthStatus.Degraded)));
        var alert = await evaluator.EvaluateAsync();
        Assert.NotNull(alert);
        Assert.Equal(AlertSeverity.Warning, alert.Severity);
        Assert.Equal(AlertState.Firing, alert.State);
        Assert.Contains("db", alert.Labels["components"]);
    }

    [Fact]
    public async Task Unhealthy_ReturnsCriticalAlert()
    {
        var evaluator = new HealthStatusAlertEvaluator(
            StoreWith(Make("api", ComponentHealthStatus.Unhealthy)));
        var alert = await evaluator.EvaluateAsync();
        Assert.NotNull(alert);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
    }
}
