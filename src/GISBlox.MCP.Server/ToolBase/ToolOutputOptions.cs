// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.ToolBase
{
   /// <summary>
   /// Controls which parts of a tool result are included in the final MCP output.
   /// </summary>
   public sealed record ToolOutputOptions
   {
      /// <summary>
      /// Indicates whether the raw result should be included in the <c>Data</c> field.
      /// </summary>
      public bool IncludeData { get; init; } = true;

      /// <summary>
      /// Optional note to show when result data is intentionally omitted.
      /// </summary>
      public string? DataOmittedReason { get; init; }
   }
}