// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

namespace CloudSmith.Monitoring.Health;

public sealed class HealthAggregator
{
    public SystemHealthSnapshot Aggregate(IReadOnlyList<ComponentHealthResult> components)
    {
        var overall = components.Count == 0
            ? ComponentHealthStatus.Unknown
            : components.Any(c => c.Status == ComponentHealthStatus.Unhealthy)
                ? ComponentHealthStatus.Unhealthy
                : components.Any(c => c.Status == ComponentHealthStatus.Degraded)
                    ? ComponentHealthStatus.Degraded
                    : ComponentHealthStatus.Healthy;

        return new SystemHealthSnapshot(overall, components, DateTimeOffset.UtcNow);
    }
}
