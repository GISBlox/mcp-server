// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Info")]
[GISBlox.MCP.Server.Attributes.Tags("Account", "Info", "Subscriptions")]
[Description("Provides account information using the GISBlox Info API.")]
internal class InfoTools : McpToolBase
{
   protected override string ToolGroupName => "Info";

   [McpServerTool(Name = "SubscriptionsList")]
   [Description("Returns the subscription(s) of the current user.")]
   public async Task<McpToolOutput> GetSubscriptions(
      GISBloxClient gisbloxClient,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      try
      {
         List<Subscription> result = await gisbloxClient.Info.GetSubscriptions(cancellationToken);

         string summary = BuildSummary(result);
         return ProcessResult(toolName, result, null, null, description, summary);         
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }

   #region Internal helpers

   private static string BuildSummary(List<Subscription> result)
   {
      return $"I found **{result?.Count ?? 0}** subscription(s) for this user.";
   }

   #endregion
}