// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;

namespace GISBlox.MCP.Server.Helpers;

internal static class ToolAttributeHelper
{
   /// <summary>
   /// Extracts the tool name and description from method attributes.
   /// </summary>
   /// <param name="type">The type containing the method.</param>
   /// <param name="methodName">The name of the method.</param>
   /// <param name="defaultToolName">Default tool name if attribute is not found.</param>
   /// <param name="defaultDescription">Default description if attribute is not found.</param>
   /// <returns>A tuple containing the tool name and description.</returns>
   public static (string ToolName, string Description) GetToolMetadata(Type type, string methodName, string? defaultToolName = null, string? defaultDescription = null)
   {
      var method = type.GetMethod(methodName);
      var toolNameAttr = method?.GetCustomAttribute<McpServerToolAttribute>();
      var descriptionAttr = method?.GetCustomAttribute<DescriptionAttribute>();

      string toolName = toolNameAttr?.Name ?? defaultToolName ?? "";
      string description = descriptionAttr?.Description ?? defaultDescription ?? "";

      return (toolName, description);
   }
}