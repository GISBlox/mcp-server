// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.ToolBase
{
   public class ResultEnvelope<T>
   {
      /// <summary>
      /// Fully qualified tool name, e.g. "Group.ToolName".
      /// </summary>
      public string ToolName { get; set; } = "";

      /// <summary>
      /// Human-readable summary of what happened.
      /// </summary>
      public string Summary { get; set; } = "";
      
      /// <summary>
      /// Additional metadata related to the result.
      /// </summary>
      public object? Metadata { get; set; }

      /// <summary>
      /// Raw JSON result (object or array) or error information (if an exception was thrown).
      /// </summary>
      public T? Data { get; set; }

      /// <summary>
      /// Any additional notes or information about the execution, such as warnings, tips, or next steps.
      /// </summary>
      public string? Notes { get; set; }     
   }
}
