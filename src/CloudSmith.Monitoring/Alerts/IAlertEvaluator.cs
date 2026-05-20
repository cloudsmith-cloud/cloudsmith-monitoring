// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

namespace CloudSmith.Monitoring.Alerts;

/// <summary>
/// Evaluates a single alert rule and returns a firing event if the condition is met,
/// or null if the condition is not met (cleared).
/// </summary>
public interface IAlertEvaluator
{
    string RuleId { get; }
    AlertSeverity Severity { get; }
    TimeSpan EvaluationInterval { get; }
    Task<AlertEvent?> EvaluateAsync(CancellationToken ct = default);
}
