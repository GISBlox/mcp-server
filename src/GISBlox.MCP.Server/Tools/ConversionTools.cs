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
      try
      {
         WKT wktObj = new(wkt);
         string result = await gisbloxClient.Conversion.ToGeoJson(wktObj, asFeatureCollection, cancellationToken);

         string summary = BuildGeoJsonConversionSummary(result?.Length ?? 0, wkt?.Length ?? 0);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
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
      try
      {
         List<WKT> result = await gisbloxClient.Conversion.ToWkt(geoJson, cancellationToken);

         string summary = BuildWktConversionSummary(result, geoJson?.Length ?? 0);
         return ProcessResult(toolName, result, null, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
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
      try
      {
         string fileName = Path.GetFileName(localPath);
         using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
         List<WKT> result = await gisbloxClient.Conversion.ToWkt(stream, fileName, cancellationToken);

         string summary = BuildWktConversionSummary(result);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
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
      try
      {
         List<WKB> result = await gisbloxClient.Conversion.ToWkb(geoJson, cancellationToken);

         string summary = BuildWkbConversionSummary(result, geoJson?.Length ?? 0);
         return ProcessResult(toolName, result, null, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
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
      try
      {
         string fileName = Path.GetFileName(localPath);
         using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
         List<WKB> result = await gisbloxClient.Conversion.ToWkb(stream, fileName, cancellationToken);

         string summary = BuildWkbConversionSummary(result);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }

   #region Internal helpers

   private static string BuildGeoJsonConversionSummary(int geoJsonLength, int wktLength)
   {
      return $"I converted the WKT geometry string{ (wktLength > 0 ? $" with length {wktLength}" : "") } into a GeoJson Feature(Collection) string with length {geoJsonLength}.";
   }

   private static string BuildWkbConversionSummary(List<WKB> result, int length = 0)
   {
      return $"I converted the GeoJson Feature(Collection){ (length > 0 ? $" with length {length}" : "") } into {result?.Count ?? 0} WKB object(s).";
   }

   private static string BuildWktConversionSummary(List<WKT> result, int length = 0)
   {
      return $"I converted the GeoJson Feature(Collection){ (length > 0 ? $" with length {length}" : "") } into {result?.Count ?? 0} WKT object(s).";
   }

   #endregion
}