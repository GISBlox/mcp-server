// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Helpers;
using System.Collections;
using System.Reflection;
using System.Text;

namespace GISBlox.MCP.Server.ToolBase
{
   public abstract class McpToolBase
   {
      protected abstract string ToolGroupName { get; }

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

      protected (string toolName, string description) GetCurrentToolMetadata(
          [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
      {
          return ToolAttributeHelper.GetToolMetadata(this.GetType(), methodName);
      }    
   }
}