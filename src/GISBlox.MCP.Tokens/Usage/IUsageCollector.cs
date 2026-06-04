// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Tokens.Usage;

/// <summary>
/// Service for collecting and recording tool usage data.
/// Injected into tool base classes to capture execution metrics.
/// </summary>
public interface IUsageCollector
{
   /// <summary>
   /// Records usage data for a tool invocation.
   /// </summary>
   /// <param name="toolName">Fully qualified tool name (e.g., "Spatial Insights.MapsList").</param>
   /// <param name="durationMs">Tool execution duration in milliseconds.</param>
   /// <param name="resultData">The tool's output data object (for complexity measurement).</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   Task RecordAsync(
       string toolName,
       long durationMs,
       object? resultData,
       CancellationToken cancellationToken = default);
}
