// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Tokens.Models;

/// <summary>
/// Represents a single usage record for an MCP tool invocation, including caller identity, execution metrics, and calculated cost.
/// </summary>
public record UsageToken
{
   /// <summary>
   /// Unique identifier for this usage token.
   /// </summary>
   public Guid TokenId { get; init; } = Guid.NewGuid();

   /// <summary>
   /// Fully qualified tool name (e.g., "Spatial Insights.MapsList").
   /// </summary>
   public required string ToolName { get; init; }

   /// <summary>
   /// The caller's service key or identity (from Authorization header). Null for stdio/anonymous calls.
   /// </summary>
   public string? CallerKey { get; init; }

   /// <summary>
   /// UTC timestamp when the tool was invoked.
   /// </summary>
   public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

   /// <summary>
   /// Tool execution duration in milliseconds.
   /// </summary>
   public long DurationMs { get; init; }

   /// <summary>
   /// Size of the tool output in bytes (JSON-serialized Data field).
   /// </summary>
   public long OutputBytes { get; init; }

   /// <summary>
   /// Number of features in the response (for GeoJSON FeatureCollection outputs). 
   /// Zero for non-GeoJSON outputs.
   /// </summary>
   public int FeatureCount { get; init; }

   /// <summary>
   /// Calculated cost based on configured rules (duration, bytes, feature/vertex count).
   /// </summary>
   public double Cost { get; init; }
}
