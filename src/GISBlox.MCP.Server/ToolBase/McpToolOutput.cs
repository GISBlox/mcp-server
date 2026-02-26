// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Text.Json.Serialization;

namespace GISBlox.MCP.Server.ToolBase
{
   /// <summary>
   /// Represents the output of an MCP tool execution, including the tool's name, summary, metadata, data, and a formatted markdown representation.
   /// </summary>
   public class McpToolOutput
   {
      /// <summary>
      /// Fully qualified tool name, e.g. "Group.ToolName".
      /// </summary>
      [JsonPropertyOrder(0)]
      public string Tool { get; set; } = "";

      /// <summary>
      /// Human-readable summary of what happened.
      /// </summary>
      [JsonPropertyOrder(1)]
      public string? Summary { get; set; } = "";

      /// <summary>
      /// Execution metadata (count, duration, etc.) or any other relevant information about the execution context.
      /// </summary>
      [JsonPropertyOrder(2)]
      public object? Metadata { get; set; }

      /// <summary>
      /// Raw JSON result (object or array) or error information (if an exception was thrown).
      /// </summary>
      [JsonPropertyOrder(4)]
      public object? Data { get; set; }

      /// <summary>
      /// Clean Markdown representation for display and use by LLMs. 
      /// Should be concise and human-readable, summarizing the result or error in a way that is easy to understand.
      /// </summary>
      [JsonPropertyOrder(3)]
      public string Markdown { get; set; } = "";
   }
}
