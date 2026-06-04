// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Costing;

namespace GISBlox.MCP.Tokens.Options;

/// <summary>
/// Configuration options for the usage token tracking system.
/// Bind to "UsageTokens" configuration section.
/// </summary>
public class UsageTokenOptions
{
   /// <summary>
   /// HTTP endpoint URL where usage tokens should be POSTed (e.g., "https://services.gisblox.com/api/usage/ingest").
   /// Leave empty to disable HTTP dispatching.
   /// </summary>
   public string IngestUrl { get; set; } = string.Empty;

   /// <summary>
   /// Rules for calculating usage cost based on output size, duration, and vertex count.
   /// </summary>
   public CostRules CostRules { get; set; } = new();
}
