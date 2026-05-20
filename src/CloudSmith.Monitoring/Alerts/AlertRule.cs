// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using CloudSmith.Sdk.Events;

namespace CloudSmith.Monitoring.Alerts;

public enum AlertSeverity { Info, Warning, Critical }

public enum AlertState { Ok, Firing }

public sealed record AlertRule(
    string        RuleId,
    string        Name,
    string        Description,
    AlertSeverity Severity,
    TimeSpan      EvaluationInterval);

public sealed record AlertEvent(
    string        RuleId,
    string        Name,
    AlertSeverity Severity,
    AlertState    State,
    string        Message,
    DateTimeOffset FiredAt,
    IReadOnlyDictionary<string, string> Labels) : ICloudSmithEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt => FiredAt;
    public string SourceModuleId => "cloudsmith-monitoring";
    public string? OrgId { get; init; }
}
