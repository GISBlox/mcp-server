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

      public McpToolOutput ProcessResult(string toolName, object? result, object? metadata = null, string? notes = null, string ? summary = null)
      {
         ResultEnvelope<object?> envelope = new()
         {
            ToolName = $"{ToolGroupName}.{toolName}",
            Summary = summary ?? $"{toolName} executed successfully.",
            Metadata = metadata,
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
         var errorData = (
            Error: ex.Message,
            Stack: ex.StackTrace
         );

         ResultEnvelope<object?> envelope = new()
         {
            ToolName = $"{ToolGroupName}.{toolName}",
            Summary = $"{toolName} failed with an exception.",

            Metadata = metadata,
            Data = errorData,
            Notes = notes ?? "An error occurred during tool execution."
         };

         return new McpToolOutput
         {
            Tool = envelope.ToolName,
            Summary = envelope.Summary,
            Metadata = envelope.Metadata,
            Data = errorData,
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

         sb.AppendLine($"**{env.Summary}**");
         sb.AppendLine();
         sb.AppendLine("---");
         sb.AppendLine();

         if (env.Metadata != null)
         {
            sb.AppendLine("### Metadata");
            sb.AppendLine();
            sb.AppendLine(ObjectToMarkdownTable(env.Metadata));
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
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
            string name = prop.Name;
            object? value = prop.GetValue(obj) ?? "";
            sb.AppendLine($"| {name} | {value} |");
         }

         return sb.ToString();
      }
            
      protected (string toolName, string description) GetCurrentToolMetadata(
          [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
      {
          return ToolAttributeHelper.GetToolMetadata(this.GetType(), methodName);
      }
   }
}