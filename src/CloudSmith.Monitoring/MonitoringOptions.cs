// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

namespace CloudSmith.Monitoring;

public sealed class MonitoringOptions
{
    public const string SectionName = "Monitoring";

    public int HealthCheckIntervalSeconds { get; set; } = 30;
    public List<EndpointConfig> Endpoints { get; set; } = [];
}

public sealed class EndpointConfig
{
    public string Name          { get; set; } = string.Empty;
    public string ComponentType { get; set; } = "service";
    public string HealthUrl     { get; set; } = string.Empty;
}
