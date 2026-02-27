// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
using GISBlox.MCP.Server.Helpers;
using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Atlas")]
[GISBlox.MCP.Server.Attributes.Tags("Municipalities", "Districts", "Neighborhoods", "Netherlands")]
[Description("Helper tools to work with Dutch postal codes, municipalities, districts and neighborhood codes.")]
internal class PostalCodeAreaCodeHelperTools : McpToolBase
{
   protected override string ToolGroupName => "Spatial Atlas";

   [McpServerTool(Name = "GemeenteGet")]
   [Description("Returns the municipality (gemeente) details for a given municipality name.")]
   public async Task<McpToolOutput> GetGemeente(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente) to retrieve.")]
      string name,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { name });

      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetGemeente(name, ct),
         parameters, toolName, description, null, cancellationToken);
   }

   [McpServerTool(Name = "GemeentenList")]
   [Description("Returns the list of all municipalities (gemeenten) in the Netherlands.")]
   public async Task<McpToolOutput> GetGemeenten(
      GISBloxClient gisbloxClient,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();

      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetGemeenten(ct),
         null, toolName, description, BuildGWBSummary, cancellationToken);
   }

   [McpServerTool(Name = "WijkenByGemeenteIdList")]
   [Description("Returns the districts (wijken) for a given municipality ID.")]
   public async Task<McpToolOutput> GetWijkenByGemeenteId(
      GISBloxClient gisbloxClient,
      [ParamDesc("The unique identifier of the municipality (gemeente).")]
      int gemeenteId,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { gemeenteId });
      
      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeenteId, ct),
         parameters, toolName, description, BuildGWBSummary, cancellationToken);
   }

   [McpServerTool(Name = "WijkenByGemeenteNameList")]
   [Description("Returns the districts (wijken) for a given municipality name.")]
   public async Task<McpToolOutput> GetWijkenByGemeenteName(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente).")]
      string gemeente,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { gemeente });
      
      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeente, ct),
         parameters, toolName, description, BuildGWBSummary, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByWijkIdList")]
   [Description("Returns the neighborhoods (buurten) for a given district (wijk) ID.")]
   public async Task<McpToolOutput> GetBuurtenByWijkId(
      GISBloxClient gisbloxClient,
      [ParamDesc("The identifier of the district (wijk).")]
      int wijkId,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { wijkId });
      
      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetBuurten(wijkId, ct),
         parameters, toolName, description, BuildGWBSummary, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByGemeenteAndWijkIdsList")]
   [Description("Returns the neighborhoods (buurten) for a given municipality (gemeente) ID and district (wijk) ID.")]
   public async Task<McpToolOutput> GetBuurtenByGemeenteAndWijkIds(
      GISBloxClient gisbloxClient,
      [ParamDesc("The identifier of the municipality (gemeente).")]
      int gemeenteId,
      [ParamDesc("The identifier of the district (wijk).")]
      int wijkId,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { gemeenteId, wijkId });

      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeenteId, wijkId, ct),
         parameters, toolName, description, BuildGWBSummary, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByGemeenteAndWijkNamesList")]
   [Description("Returns the neighborhoods (buurten) for a given municipality (gemeente) name and district (wijk) name.")]
   public async Task<McpToolOutput> GetBuurtenByGemeenteAndWijkNames(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente).")]
      string gemeente,
      [ParamDesc("The name of the district (wijk).")]
      string wijk,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { gemeente, wijk });
      
      return await ExecuteToolAsync(ct => gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeente, wijk, ct),
         parameters, toolName, description, BuildGWBSummary, cancellationToken);
   }

   #region Internal helpers
   
   private static string BuildGWBSummary(GWBRecord? result)
   {
      if (result == null)
         return "I found no data.";
      return result.RecordSet.Count switch
      {
         0 => "I found no records.",
         1 => "I found 1 record.",
         _ => $"I found {result.RecordSet.Count} records."
      };
   }

   #endregion
}