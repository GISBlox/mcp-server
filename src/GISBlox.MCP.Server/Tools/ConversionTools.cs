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
[GISBlox.MCP.Server.Attributes.Category("Data Shaping")]
[GISBlox.MCP.Server.Attributes.Tags("Conversion", "WKT", "WKB", "GeoJson")]
[Description("Converts GeoJson into WKB and WKT geometry objects, and vice versa, using the GISBlox Conversion API.")]
internal class ConversionTools : McpToolBase
{
   protected override string ToolGroupName => "Data Shaping";

   [McpServerTool(Name = "WktToGeoJson")]
   [Description("Converts a WKT geometry string into a GeoJson Feature(Collection) string.")]
   public async Task<McpToolOutput> ConvertToGeoJson(GISBloxClient gisbloxClient,
      [ParamDesc("The Well-Known Text (WKT) geometry string to convert.")]
      string wkt,
      [ParamDesc("If true, returns a GeoJson FeatureCollection; otherwise returns a single Feature.")]
      bool asFeatureCollection, CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { asFeatureCollection });

      return await ExecuteToolAsync(ct => gisbloxClient.Conversion.ToGeoJson((WKT)new(wkt), asFeatureCollection, ct),
         parameters, toolName, description, result => BuildGeoJsonConversionSummary(result?.Length ?? 0, wkt?.Length ?? 0), cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonToWkt")]
   [Description("Converts a GeoJson Feature(Collection) string into one or more WKT objects.")]
   public async Task<McpToolOutput> ConvertToWkt(
      GISBloxClient gisbloxClient,
      [ParamDesc("The GeoJson Feature(Collection) string to convert.")]
      string geoJson,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();

      return await ExecuteToolAsync(ct => gisbloxClient.Conversion.ToWkt(geoJson, ct),
         null, toolName, description, result => BuildWktConversionSummary(result, geoJson?.Length ?? 0), cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonFileToWkt")]
   [Description("Converts the contents of a GeoJson file into one or more WKT objects.")]
   public async Task<McpToolOutput> ConvertToWktFromFile(
      GISBloxClient gisbloxClient,
      [ParamDesc("The local file path to the GeoJson file to convert.")]
      string localPath,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { localPath });

      return await ExecuteToolAsync(
         async ct =>
         {
            string fileName = Path.GetFileName(localPath);
            using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            return await gisbloxClient.Conversion.ToWkt(stream, fileName, ct);
         },
         parameters, toolName, description, result => BuildWktConversionSummary(result), cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonToWkb")]
   [Description("Converts a GeoJson Feature(Collection) string into one or more WKB objects.")]
   public async Task<McpToolOutput> ConvertToWkb(
      GISBloxClient gisbloxClient,
      [ParamDesc("The GeoJson Feature(Collection) string to convert.")]
      string geoJson,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();

      return await ExecuteToolAsync(ct => gisbloxClient.Conversion.ToWkb(geoJson, ct),
         null, toolName, description, result => BuildWkbConversionSummary(result, geoJson?.Length ?? 0), cancellationToken);
   }

   [McpServerTool(Name = "GeoJsonFileToWkb")]
   [Description("Converts the contents of a GeoJson file into one or more WKB objects.")]
   public async Task<McpToolOutput> ConvertToWkbFromFile(
      GISBloxClient gisbloxClient,
      [ParamDesc("The local file path to the GeoJson file to convert.")]
      string localPath,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { localPath });

      return await ExecuteToolAsync(
         async ct =>
         {
            string fileName = Path.GetFileName(localPath);
            using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            return await gisbloxClient.Conversion.ToWkb(stream, fileName, ct);
         },
         parameters, toolName, description, result => BuildWkbConversionSummary(result), cancellationToken);
   }

   #region Internal helpers

   private static string BuildGeoJsonConversionSummary(int geoJsonLength, int wktLength)
   {
      return $"I converted the WKT geometry string{(wktLength > 0 ? $" with length {wktLength}" : "")} into a GeoJson Feature(Collection) string with length {geoJsonLength}.";
   }

   private static string BuildWkbConversionSummary(List<WKB>? result, int length = 0)
   {
      return $"I converted the GeoJson Feature(Collection){(length > 0 ? $" with length {length}" : "")} into {result?.Count ?? 0} WKB object(s).";
   }

   private static string BuildWktConversionSummary(List<WKT>? result, int length = 0)
   {
      return $"I converted the GeoJson Feature(Collection){(length > 0 ? $" with length {length}" : "")} into {result?.Count ?? 0} WKT object(s).";
   }

   #endregion
}