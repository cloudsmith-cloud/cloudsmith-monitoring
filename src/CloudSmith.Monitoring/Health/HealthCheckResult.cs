// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

namespace CloudSmith.Monitoring.Health;

public enum ComponentHealthStatus { Healthy, Degraded, Unhealthy, Unknown }

public sealed record ComponentHealthResult(
    string               ComponentName,
    string               ComponentType,
    ComponentHealthStatus Status,
    string?              Description,
    DateTimeOffset       CheckedAt,
    TimeSpan             Duration);

public sealed record SystemHealthSnapshot(
    ComponentHealthStatus OverallStatus,
    IReadOnlyList<ComponentHealthResult> Components,
    DateTimeOffset CapturedAt);
