// Copyright 2026 CloudSmith Contributors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CloudSmith.Monitoring.Health;

/// <summary>
/// Probes a remote /health/ready endpoint and maps the response to a ComponentHealthResult.
/// Used by the monitoring worker to poll API, portal, and other services in the stack.
/// </summary>
public sealed class HttpHealthProbe
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpHealthProbe> _logger;

    public HttpHealthProbe(HttpClient http, ILogger<HttpHealthProbe> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<ComponentHealthResult> ProbeAsync(
        string componentName,
        string componentType,
        string healthUrl,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _http.GetAsync(healthUrl, ct);
            sw.Stop();

            var status = response.IsSuccessStatusCode
                ? ComponentHealthStatus.Healthy
                : ComponentHealthStatus.Degraded;

            return new ComponentHealthResult(componentName, componentType, status,
                Description:$"HTTP {(int)response.StatusCode}",
                CheckedAt: DateTimeOffset.UtcNow,
                Duration: sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning("CS-MON-WARN-001: Health probe failed for '{Component}': {Message}",
                componentName, ex.Message);
            return new ComponentHealthResult(componentName, componentType,
                ComponentHealthStatus.Unhealthy,
                Description:ex.Message,
                CheckedAt: DateTimeOffset.UtcNow,
                Duration: sw.Elapsed);
        }
    }
}
