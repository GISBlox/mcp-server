// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Atlas")]
[GISBlox.MCP.Server.Attributes.Tags("Municipalities", "Districts", "Neighborhoods", "Netherlands")]
[Description("Helper tools to work with Dutch postal codes, municipalities, districts and neighborhood codes.")]
internal class PostalCodeAreaCodeHelperTools
{
   [McpServerTool(Name = "GemeenteGet")]
   [Description("Returns the municipality (gemeente) details for a given municipality name.")]
   public static async Task<GWB> GetGemeente(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente) to retrieve.")]
      string name,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetGemeente(name, cancellationToken);
   }

   [McpServerTool(Name = "GemeentenList")]
   [Description("Returns the list of all municipalities (gemeenten) in the Netherlands.")]
   public async static Task<GWBRecord> GetGemeenten(
      GISBloxClient gisbloxClient,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetGemeenten(cancellationToken);
   }

   [McpServerTool(Name = "WijkenByGemeenteIdList")]
   [Description("Returns the districts (wijken) for a given municipality ID.")]
   public async static Task<GWBRecord> GetWijkenByGemeenteId(
      GISBloxClient gisbloxClient,
      [ParamDesc("The unique identifier of the municipality (gemeente).")]
      int gemeenteId,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeenteId, cancellationToken);
   }

   [McpServerTool(Name = "WijkenByGemeenteNameList")]
   [Description("Returns the districts (wijken) for a given municipality name.")]
   public async static Task<GWBRecord> GetWijkenByGemeenteName(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente).")]
      string gemeente,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeente, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByWijkIdList")]
   [Description("Returns the neighborhoods (buurten) for a given district (wijk) ID.")]
   public async static Task<GWBRecord> GetBuurtenByWijkId(
      GISBloxClient gisbloxClient,
      [ParamDesc("The identifier of the district (wijk).")]
      int wijkId,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(wijkId, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByGemeenteAndWijkIdsList")]
   [Description("Returns the neighborhoods (buurten) for a given municipality (gemeente) ID and district (wijk) ID.")]
   public async static Task<GWBRecord> GetBuurtenByGemeenteAndWijkIds(
      GISBloxClient gisbloxClient,
      [ParamDesc("The identifier of the municipality (gemeente).")]
      int gemeenteId,
      [ParamDesc("The identifier of the district (wijk).")]
      int wijkId,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeenteId, wijkId, cancellationToken);
   }

   [McpServerTool(Name = "BuurtenByGemeenteAndWijkNamesList")]
   [Description("Returns the neighborhoods (buurten) for a given municipality (gemeente) name and district (wijk) name.")]
   public async static Task<GWBRecord> GetBuurtenByGemeenteAndWijkNames(
      GISBloxClient gisbloxClient,
      [ParamDesc("The name of the municipality (gemeente).")]
      string gemeente,
      [ParamDesc("The name of the district (wijk).")]
      string wijk,
      CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeente, wijk, cancellationToken);
   }
}