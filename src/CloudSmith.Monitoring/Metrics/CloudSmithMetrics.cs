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

    // Latest observed values; updated by the monitoring worker and read by the OTel callback.
    private static int _clusterCount, _clusterHealthy, _nodeCount, _nodeOnline, _vmCount, _vmRunning;

    public static void SetClusterCount(int v)   => _clusterCount = v;
    public static void SetClusterHealthy(int v) => _clusterHealthy = v;
    public static void SetNodeCount(int v)      => _nodeCount = v;
    public static void SetNodeOnline(int v)     => _nodeOnline = v;
    public static void SetVmCount(int v)        => _vmCount = v;
    public static void SetVmRunning(int v)      => _vmRunning = v;

    // Cluster health
    public static readonly ObservableGauge<int> ClusterCount =
        _meter.CreateObservableGauge<int>("cloudsmith.clusters.total", () => _clusterCount,
            description: "Total number of registered clusters");

    public static readonly ObservableGauge<int> ClusterHealthy =
        _meter.CreateObservableGauge<int>("cloudsmith.clusters.healthy", () => _clusterHealthy,
            description: "Number of clusters in healthy state");

    // Node health
    public static readonly ObservableGauge<int> NodeCount =
        _meter.CreateObservableGauge<int>("cloudsmith.nodes.total", () => _nodeCount,
            description: "Total number of registered nodes");

    public static readonly ObservableGauge<int> NodeOnline =
        _meter.CreateObservableGauge<int>("cloudsmith.nodes.online", () => _nodeOnline,
            description: "Number of nodes with online status");

    // VM inventory
    public static readonly ObservableGauge<int> VmCount =
        _meter.CreateObservableGauge<int>("cloudsmith.vms.total", () => _vmCount,
            description: "Total number of registered virtual machines");

    public static readonly ObservableGauge<int> VmRunning =
        _meter.CreateObservableGauge<int>("cloudsmith.vms.running", () => _vmRunning,
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
