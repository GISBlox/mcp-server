// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Data Shaping")]
[GISBlox.MCP.Server.Attributes.Tags("Conversion", "WKT", "WKB", "GeoJson")]
[Description("Converts GeoJson into WKB and WKT geometry objects, and vice versa, using the GISBlox Conversion API.")]
internal class ConversionTools
{
   [McpServerTool(Name = "WktToGeoJson")]
   [Description("Converts a WKT geometry string into a GeoJson Feature(Collection) string.")]   
   public static async Task<string> ConvertToGeoJson(GISBloxClient gisbloxClient, string wkt, bool asFeatureCollection, CancellationToken cancellationToken = default)
   {
      WKT wktObj = new(wkt);
      return await gisbloxClient.Conversion.ToGeoJson(wktObj, asFeatureCollection, cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonToWkt")]
   [Description("Converts a GeoJson Feature(Collection) string into one or more WKT objects.")]
   public static async Task<List<WKT>> ConvertToWkt(GISBloxClient gisbloxClient, string geoJson, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.Conversion.ToWkt(geoJson, cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonFileToWkt")]
   [Description("Converts the contents of a GeoJson file into one or more WKT objects.")]
   public static async Task<List<WKT>> ConvertToWktFromFile(GISBloxClient gisbloxClient, string localPath, CancellationToken cancellationToken = default)
   {
      string fileName = Path.GetFileName(localPath);
      using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
      return await gisbloxClient.Conversion.ToWkt(stream, fileName, cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonToWkb")]
   [Description("Converts a GeoJson Feature(Collection) string into one or more WKB objects.")]
   public static async Task<List<WKB>> ConvertToWkb(GISBloxClient gisbloxClient, string geoJson, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.Conversion.ToWkb(geoJson, cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonFileToWkb")]
   [Description("Converts the contents of a GeoJson file into one or more WKB objects.")]
   public static async Task<List<WKB>> ConvertToWkbFromFile(GISBloxClient gisbloxClient, string localPath, CancellationToken cancellationToken = default)
   {
      string fileName = Path.GetFileName(localPath);
      using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
      return await gisbloxClient.Conversion.ToWkb(stream, fileName, cancellationToken);
   }
}