// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

namespace CloudSmith.Monitoring.Health;

/// <summary>
/// Thread-safe store for the most recent system health snapshot.
/// Updated by HealthMonitorWorker; read by alert evaluators.
/// </summary>
public sealed class HealthSnapshotStore
{
    private volatile SystemHealthSnapshot? _latest;

    public void Update(SystemHealthSnapshot snapshot) => _latest = snapshot;

    public SystemHealthSnapshot? Latest => _latest;
}
