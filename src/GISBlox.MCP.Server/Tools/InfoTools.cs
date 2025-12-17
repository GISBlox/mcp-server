// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Info")]
[GISBlox.MCP.Server.Attributes.Tags("Account", "Info", "Subscriptions")]
[Description("Provides account information using the GISBlox Info API.")]
internal class InfoTools
{
   [McpServerTool(Name = "Subscriptions")]
   [Description("Returns the subscriptions of the authorized GISBlox user.")]
   public static async Task<List<Subscription>> GetSubscriptions(GISBloxClient gisbloxClient, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.Info.GetSubscriptions(cancellationToken);
   }
}

