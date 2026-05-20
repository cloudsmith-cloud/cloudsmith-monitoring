// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Monitoring.Health;
using Xunit;

namespace CloudSmith.Monitoring.Tests.Health;

public class HealthAggregatorTests
{
    private readonly HealthAggregator _aggregator = new();

    private static ComponentHealthResult Make(string name, ComponentHealthStatus status) =>
        new(name, "service", status, null, DateTimeOffset.UtcNow, TimeSpan.Zero);

    [Fact]
    public void AllHealthy_ReturnsHealthy()
    {
        var result = _aggregator.Aggregate([
            Make("api", ComponentHealthStatus.Healthy),
            Make("portal", ComponentHealthStatus.Healthy),
        ]);
        Assert.Equal(ComponentHealthStatus.Healthy, result.OverallStatus);
    }

    [Fact]
    public void AnyDegraded_ReturnsDegraded()
    {
        var result = _aggregator.Aggregate([
            Make("api", ComponentHealthStatus.Healthy),
            Make("db", ComponentHealthStatus.Degraded),
        ]);
        Assert.Equal(ComponentHealthStatus.Degraded, result.OverallStatus);
    }

    [Fact]
    public void AnyUnhealthy_ReturnsUnhealthy_EvenWithDegraded()
    {
        var result = _aggregator.Aggregate([
            Make("api", ComponentHealthStatus.Degraded),
            Make("db", ComponentHealthStatus.Unhealthy),
        ]);
        Assert.Equal(ComponentHealthStatus.Unhealthy, result.OverallStatus);
    }

    [Fact]
    public void Empty_ReturnsUnknown()
    {
        var result = _aggregator.Aggregate([]);
        Assert.Equal(ComponentHealthStatus.Unknown, result.OverallStatus);
    }

    [Fact]
    public void Snapshot_ContainsAllComponents()
    {
        var result = _aggregator.Aggregate([
            Make("a", ComponentHealthStatus.Healthy),
            Make("b", ComponentHealthStatus.Healthy),
        ]);
        Assert.Equal(2, result.Components.Count);
    }
}
