// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
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
      try
      {
         WKT wktObj = new(wkt);
         string result = await gisbloxClient.Conversion.ToGeoJson(wktObj, asFeatureCollection, cancellationToken);

         return ProcessResult(toolName, result, null, description);
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
         return ProcessResult(toolName, result, null, description);
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
      try
      {
         string fileName = Path.GetFileName(localPath);
         using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
         List<WKT> result = await gisbloxClient.Conversion.ToWkt(stream, fileName, cancellationToken);

         return ProcessResult(toolName, result, null, description);
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
         return ProcessResult(toolName, result, null, description);
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
      try
      {
         string fileName = Path.GetFileName(localPath);
         using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
         List<WKB> result = await gisbloxClient.Conversion.ToWkb(stream, fileName, cancellationToken);

         return ProcessResult(toolName, result, null, description);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }
}