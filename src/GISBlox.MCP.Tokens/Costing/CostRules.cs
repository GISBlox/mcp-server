// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Tokens.Costing;

/// <summary>
/// Configuration record defining the divisors used to calculate usage cost.
/// Cost formula: (OutputBytes / ByteDivisor) + (DurationMs / DurationDivisor) + (VertexCount / VertexDivisor)
/// </summary>
public record CostRules
{
   /// <summary>
   /// Divisor for output byte size. Default: 3000 (one cost unit per 3KB).
   /// </summary>
   public double ByteDivisor { get; init; } = 3000;

   /// <summary>
   /// Divisor for execution duration in milliseconds. Default: 40 (one cost unit per 40ms).
   /// </summary>
   public double DurationDivisor { get; init; } = 40;

   /// <summary>
   /// Divisor for vertex count (reserved for future use). Default: 400 (one cost unit per 400 vertices).
   /// </summary>
   public double VertexDivisor { get; init; } = 400;
}
