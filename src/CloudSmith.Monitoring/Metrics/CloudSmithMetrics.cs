// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics.Metrics;

namespace CloudSmith.Monitoring.Metrics;

/// <summary>
/// Central OTel meter and named instruments for all CloudSmith metrics.
/// Consumers record observations; Prometheus scrapes via the /metrics endpoint.
/// </summary>
public static class CloudSmithMetrics
{
    public const string MeterName = "CloudSmith";

    private static readonly Meter _meter = new(MeterName, "0.1.0");

    // Cluster health
    public static readonly ObservableGauge<int> ClusterCount =
        _meter.CreateObservableGauge<int>("cloudsmith.clusters.total",
            description: "Total number of registered clusters");

    public static readonly ObservableGauge<int> ClusterHealthy =
        _meter.CreateObservableGauge<int>("cloudsmith.clusters.healthy",
            description: "Number of clusters in healthy state");

    // Node health
    public static readonly ObservableGauge<int> NodeCount =
        _meter.CreateObservableGauge<int>("cloudsmith.nodes.total",
            description: "Total number of registered nodes");

    public static readonly ObservableGauge<int> NodeOnline =
        _meter.CreateObservableGauge<int>("cloudsmith.nodes.online",
            description: "Number of nodes with online status");

    // VM inventory
    public static readonly ObservableGauge<int> VmCount =
        _meter.CreateObservableGauge<int>("cloudsmith.vms.total",
            description: "Total number of registered virtual machines");

    public static readonly ObservableGauge<int> VmRunning =
        _meter.CreateObservableGauge<int>("cloudsmith.vms.running",
            description: "Number of VMs in running state");

    // API health check results
    public static readonly Counter<long> HealthCheckRuns =
        _meter.CreateCounter<long>("cloudsmith.healthcheck.runs",
            description: "Total health check evaluations");

    public static readonly Counter<long> HealthCheckFailures =
        _meter.CreateCounter<long>("cloudsmith.healthcheck.failures",
            description: "Total health check failures");

    // Alert counts
    public static readonly Counter<long> AlertsFired =
        _meter.CreateCounter<long>("cloudsmith.alerts.fired",
            description: "Total alert rules that have fired");

    public static readonly Counter<long> AlertsResolved =
        _meter.CreateCounter<long>("cloudsmith.alerts.resolved",
            description: "Total alert rules that have resolved");
}
