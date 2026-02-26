// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Helpers;
using System.Collections;
using System.Reflection;
using System.Text;

namespace GISBlox.MCP.Server.ToolBase
{
   /// <summary>
   /// Abstract class that serves as a base for all MCP tools, 
   /// providing common functionality for processing tool results and errors, 
   /// and generating structured markdown output for display in the MCP interface.
   /// </summary>
   public abstract class McpToolBase
   {
      /// <summary>
      /// The name of the tool group/category that this tool belongs to.
      /// </summary>
      protected abstract string ToolGroupName { get; }

      /// <summary>
      /// Processes the result of a tool execution and returns an output object containing the tool's name, status, metadata, and formatted markdown.
      /// </summary>
      /// <param name="toolName">The name of the tool being processed. Used to identify the specific execution context.</param>
      /// <param name="result">The result of the tool execution. This can be any object representing the output data produced by the tool.</param>
      /// <param name="parameters">Optional parameters used during the tool execution.</param>
      /// <param name="metadata">Optional additional metadata related to the tool execution.</param>      
      /// <param name="notes">Optional notes to include in the output. Can be used to provide further insights or comments about the tool execution.</param>
      /// <param name="summary">Optional summary information describing the outcome of the tool execution. Included in the output for quick reference.</param>
      /// <returns>A McpToolOutput object containing the tool's name, summary, metadata, result data, and a formatted markdown representation of the execution.</returns>
      public McpToolOutput ProcessResult(string toolName, object? result, object? parameters = null, object? metadata = null, string? notes = null, string ? summary = null)
      {
         ResultEnvelope<object?> envelope = new()
         {
            ToolName = $"{ToolGroupName}.{toolName}",
            Status = $"{toolName} executed successfully.",
            Summary = summary,
            Metadata = new
            {
               Parameters = parameters,
               Extra = metadata
            },
            Data = result,
            Notes = notes
         };

         return new McpToolOutput
         {
            Tool = envelope.ToolName,
            Summary = envelope.Summary,
            Metadata = envelope.Metadata,
            Data = envelope.Data,
            Markdown = BuildMarkdown(envelope)
         };
      }

      /// <summary>
      /// Processes an exception that occurs during tool execution and returns a structured output containing error details for diagnostic purposes.
      /// </summary>      
      /// <param name="toolName">The name of the tool that encountered the error. Used to identify the source of the failure in the output.</param>
      /// <param name="ex">The exception that was thrown during the tool's execution. Provides details about the error that occurred.</param>
      /// <param name="metadata">Optional additional metadata related to the error.</param>
      /// <param name="notes">Optional notes to include in the error output. Can provide further insights or comments regarding the error.</param>
      /// <returns>A McpToolOutput instance containing the tool name, error status, metadata, error message, and a formatted markdown representation of the error.</returns>
      public McpToolOutput ProcessError(string toolName, Exception ex, object? metadata = null, string? notes = null)
      {
         ResultEnvelope<object?> envelope = new()
         {
            ToolName = $"{ToolGroupName}.{toolName}",
            Status = $"{toolName} failed with an exception.",
            Summary = $"### {ex.Message?.Replace("\\", "").Replace("\"", "")}",
            Metadata = metadata,
            Data = ex.Message,
            Notes = notes ?? "An error occurred during tool execution."
         };

         return new McpToolOutput
         {
            Tool = envelope.ToolName,
            Summary = envelope.Status,
            Metadata = envelope.Metadata,
            Data = envelope.Data,
            Markdown = BuildMarkdown(envelope)
         };
      }

      private static string BuildMarkdown(ResultEnvelope<object?> env)
      {
         StringBuilder sb = new();

         sb.AppendLine($"## 🧩 {env.ToolName}");
         sb.AppendLine();

         if (!string.IsNullOrWhiteSpace(env.Notes))
         {            
            sb.AppendLine(env.Notes);
            sb.AppendLine();
         }

         sb.AppendLine($"**{env.Status}**");
         sb.AppendLine();

         if (env.Summary != null)
         {
            sb.AppendLine($"{env.Summary}");
            sb.AppendLine();
         }

         sb.AppendLine("---");
         sb.AppendLine();

         if (env.Metadata != null)
         {
            dynamic metaDataObj = env.Metadata;
            var extra = metaDataObj.Extra;
            if (extra != null)
            {
               sb.AppendLine("### Metadata");
               sb.AppendLine();
               sb.AppendLine(ObjectToMarkdownTable(extra));
               sb.AppendLine();
               sb.AppendLine("---");
               sb.AppendLine();
            }
         }

         if (env.Data != null)
         {
            if (IsCollection(env.Data))
            {
               IEnumerable collection = (IEnumerable)env.Data;
               object? firstItem = collection.Cast<object>().FirstOrDefault();

               int count = collection.Cast<object>().Count();

               sb.AppendLine("### Data (Collection)");
               sb.AppendLine($"Count: **{count} items**");
               sb.AppendLine();

               if (firstItem != null)
               {
                  sb.AppendLine("#### Example Item Structure");
                  sb.AppendLine(ObjectToMarkdownTable(firstItem));
                  sb.AppendLine();
               }

               sb.AppendLine("_Full JSON available in the `data` field._");
            }
            else
            {
               sb.AppendLine("### Data");
               sb.AppendLine("This tool returned a single object.");
               sb.AppendLine();
               sb.AppendLine("#### Structure");
               sb.AppendLine(ObjectToMarkdownTable(env.Data));
               sb.AppendLine();
               sb.AppendLine("_Full JSON available in the `data` field._");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
         }         

         return sb.ToString();
      }

      private static bool IsCollection(object obj)
      {
         if (obj is string) return false;
         return obj is IEnumerable;
      }

      private static string ObjectToMarkdownTable(object obj)
      {
         PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
         StringBuilder sb = new();

         sb.AppendLine("| Field | Value |");
         sb.AppendLine("|-------|--------|");

         foreach (var prop in props)
         {            
            if (prop.GetIndexParameters().Length > 0)
               continue;

            string name = prop.Name;
            object? rawValue = prop.GetValue(obj);
            string value = FormatValue(rawValue);

            sb.AppendLine($"| {name} | {value} |");
         }

         return sb.ToString();
      }

      private static string FormatValue(object? value)
      {
         if (value == null)
            return "_null_";
                  
         if (value is string str)
            return str;

         // Check if it's a collection
         if (value is IEnumerable enumerable)
         {
            var items = enumerable.Cast<object>().ToList();
            int count = items.Count;

            if (count == 0)
               return "_empty collection_";

            // Show first few items with count
            var preview = string.Join(", ", items.Take(3).Select(i => i?.ToString() ?? "null"));

            if (count > 3)
               return $"[{count} items: {preview}, ...]";
            else
               return $"[{count} items: {preview}]";
         }
         return value.ToString() ?? "";
      }

      /// <summary>
      /// Extracts the tool name and description from the ToolAttribute applied to the method that calls this function.
      /// </summary>
      /// <param name="methodName">The name of the method that calls this function.</param>
      /// <returns>A tuple containing the tool name and description.</returns>
      protected (string toolName, string description) GetCurrentToolMetadata(
          [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
      {
          return ToolAttributeHelper.GetToolMetadata(this.GetType(), methodName);
      }    
   }
}