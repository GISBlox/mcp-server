// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[Description("Helper tools to work with Dutch postal codes, municipalities, districts and neighborhood codes.")]
internal class PostalCodeAreaCodeHelperTools
{
   [McpServerTool(Name = "postalcodes_gwb_gemeente_get")]
   [Description("Returns the municipality (gemeente) details for a given municipality name.")]
   public static async Task<GWB> GetGemeente(GISBloxClient gisbloxClient, string name, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetGemeente(name, cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_gemeente_list")]
   [Description("Returns the list of all municipalities (gemeenten) in the Netherlands.")]
   public async static Task<GWBRecord> GetGemeenten(GISBloxClient gisbloxClient, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetGemeenten(cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_wijken_in_gemeente_id_list")]
   [Description("Returns the districts (wijken) for a given municipality ID.")]
   public async static Task<GWBRecord> GetWijkenByGemeenteId(GISBloxClient gisbloxClient, int gemeenteId, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeenteId, cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_wijken_in_gemeente_name_list")]
   [Description("Returns the districts (wijken) for a given municipality name.")]
   public async static Task<GWBRecord> GetWijkenByGemeenteName(GISBloxClient gisbloxClient, string gemeente, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetWijken(gemeente, cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_buurten_in_wijk_id_list")]
   [Description("Returns the neighborhoods (buurten) for a given wijk ID.")]
   public async static Task<GWBRecord> GetBuurtenByWijkId(GISBloxClient gisbloxClient, int wijkId, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(wijkId, cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_buurten_in_gemeente_and_wijk_ids_list")]
   [Description("Returns the neighborhoods (buurten) for a given gemeente ID and wijk ID.")]
   public async static Task<GWBRecord> GetBuurtenByGemeenteAndWijkIds(GISBloxClient gisbloxClient, int gemeenteId, int wijkId, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeenteId, wijkId, cancellationToken);
   }

   [McpServerTool(Name = "postalcodes_gwb_buurten_in_gemeente_and_wijk_names_list")]
   [Description("Returns the neighborhoods (buurten) for a given gemeente name and wijk name.")]
   public async static Task<GWBRecord> GetBuurtenByGemeenteAndWijkNames(GISBloxClient gisbloxClient, string gemeente, string wijk, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.PostalCodes.AreaHelper.GetBuurten(gemeente, wijk, cancellationToken);
   }
}