// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[Description("Converts well-known text to GeoJson formats using the GISBlox Conversion API.")]
internal class ConversionTools
{
   [McpServerTool(Name = "conversion_wkt_to_geojson_get")]
   [Description("Converts a WKT geometry string into a GeoJson Feature(Collection) string.")]
   public static async Task<string> ConvertToGeoJson(GISBloxClient gisbloxClient, string wkt, bool asFeatureCollection, CancellationToken cancellationToken = default)
   {
      WKT wktObj = new(wkt);
      return await gisbloxClient.Conversion.ToGeoJson(wktObj, asFeatureCollection, cancellationToken);
   }
}