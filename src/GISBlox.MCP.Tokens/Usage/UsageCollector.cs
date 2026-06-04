// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Complexity;
using GISBlox.MCP.Tokens.Costing;
using GISBlox.MCP.Tokens.Models;
using GISBlox.MCP.Tokens.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GISBlox.MCP.Tokens.Usage;

/// <summary>
/// Collects tool usage data, measures complexity, calculates cost, and dispatches usage tokens.
/// </summary>
public class UsageCollector(ICallerKeyAccessor callerKeyAccessor, IUsageDispatcher dispatcher, IOptions<UsageTokenOptions> options, ILogger<UsageCollector> logger) : IUsageCollector
{
   private readonly ICallerKeyAccessor _callerKeyAccessor = callerKeyAccessor;
   private readonly IUsageDispatcher _dispatcher = dispatcher;
   private readonly UsageTokenOptions _options = options.Value;
   private readonly ILogger<UsageCollector> _logger = logger;

   public async Task RecordAsync(string toolName, long durationMs, object? resultData, CancellationToken cancellationToken = default)
   {
      try
      {
         // Measure complexity
         ComplexityMeasurer.Measure(resultData, out long outputBytes, out int featureCount);

         // Calculate cost
         double cost = CostCalculator.Calculate(outputBytes, durationMs, _options.CostRules);

         // Get caller identity
         string? callerKey = _callerKeyAccessor.GetCallerKey();

         // Build usage token
         var token = new UsageToken
         {
            ToolName = toolName,
            CallerKey = callerKey,
            DurationMs = durationMs,
            OutputBytes = outputBytes,
            FeatureCount = featureCount,
            Cost = cost
         };

         // Dispatch (fire-and-forget)
         await _dispatcher.SendAsync(token, cancellationToken);

         _logger.LogDebug(
             "Recorded usage for {ToolName}: {DurationMs}ms, {OutputBytes} bytes, {FeatureCount} features, cost {Cost:F4}",
             toolName,
             durationMs,
             outputBytes,
             featureCount,
             cost);
      }
      catch (Exception ex)
      {
         _logger.LogWarning(ex, "Failed to record usage for {ToolName}. Usage tracking continues despite this error.", toolName);
      }
   }
}
