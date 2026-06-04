// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Tokens.Costing;

/// <summary>
/// Calculates the cost of a tool invocation based on output size, duration, and configured rules.
/// </summary>
public static class CostCalculator
{
   /// <summary>
   /// Calculates the usage cost based on output bytes and duration.
   /// Formula: (outputBytes / ByteDivisor) + (durationMs / DurationDivisor)
   /// </summary>
   /// <param name="outputBytes">Size of the output in bytes.</param>
   /// <param name="durationMs">Execution duration in milliseconds.</param>
   /// <param name="rules">Cost calculation rules (divisors).</param>
   /// <returns>Calculated cost value.</returns>
   public static double Calculate(long outputBytes, long durationMs, CostRules rules)
   {
      double byteCost = rules.ByteDivisor > 0 ? outputBytes / rules.ByteDivisor : 0;
      double durationCost = rules.DurationDivisor > 0 ? durationMs / rules.DurationDivisor : 0;

      return byteCost + durationCost;
   }
}
