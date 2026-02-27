// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
using GISBlox.MCP.Server.Helpers;
using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.Collections;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("World Views")]
[GISBlox.MCP.Server.Attributes.Tags("Projection", "Coordinates", "RDNew", "WGS84")]
[Description("Reprojects WGS84 coordinates to RDNew, and vice versa, using the GISBlox Projection API.")]
internal class ProjectionTools : McpToolBase
{
   protected override string ToolGroupName => "World Views";

   [McpServerTool(Name = "Wgs84ToRds")]
   [Description("Reprojects a Coordinate (WGS84) to an RDPoint (Amersfoort / EPSG:28992).")]
   public async Task<McpToolOutput> ToRDSFromCoordinate(
      GISBloxClient gisbloxClient,
      [ParamDesc("The latitude of the WGS84 coordinate.")]
      double lat,
      [ParamDesc("The longitude of the WGS84 coordinate.")]
      double lon,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { lat, lon });      
      
      return await ExecuteToolAsync(ct => gisbloxClient.Projection.ToRDS((Coordinate)new(lat, lon), ct),
         parameters, toolName, description, result => BuildProjectionSummary(result == null || result.X == -9999), cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsComplete")]
   [Description("Reprojects a Coordinate (WGS84) to a Location (WGS84 / RDNew). Includes the sources coordinate.")]
   public async Task<McpToolOutput> ToRDSFromCoordinateComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("The latitude of the WGS84 coordinate.")]
      double lat,
      [ParamDesc("The longitude of the WGS84 coordinate.")]
      double lon,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { lat, lon });
      
      return await ExecuteToolAsync(ct => gisbloxClient.Projection.ToRDSComplete((Coordinate)new(lat, lon), ct),
         parameters, toolName, description, result => BuildProjectionSummary(result == null || result.Lat == 0), cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsList")]
   [Description("Reprojects an array of WGS84 coordinates to an array of RDPoints (Amersfoort / EPSG:28992).")]
   public async Task<McpToolOutput> ToRDSFromCoordinateList(
   GISBloxClient gisbloxClient,
      [ParamDesc("Array of WGS84 coordinates, where each element is a [latitude, longitude] array.")]
      double[][] coordinates,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { coordinates });

      return await ExecuteToolAsync(
         async ct =>
         {
            if (coordinates == null || coordinates.Length == 0)
               throw new ArgumentException("Coordinates array cannot be null or empty.", nameof(coordinates));
            var coordList = ConvertToCoordinateList(coordinates);
            return await gisbloxClient.Projection.ToRDS(coordList, ct);
         },
         parameters, toolName, description, BuildProjectionSummary, cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsCompleteList")]
   [Description("Reprojects an array of WGS84 coordinates to an array of Locations (WGS84 / RDNew). Includes the sources coordinates.")]
   public async Task<McpToolOutput> ToRDSFromCoordinateListComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of WGS84 coordinates, where each element is a [latitude, longitude] array.")]
      double[][] coordinates,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { coordinates });

      return await ExecuteToolAsync(
         async ct =>
         {
            if (coordinates == null || coordinates.Length == 0)
               throw new ArgumentException("Coordinates array cannot be null or empty.", nameof(coordinates));
            var coordList = ConvertToCoordinateList(coordinates);
            return await gisbloxClient.Projection.ToRDSComplete(coordList, ct);
         },
         parameters, toolName, description, BuildProjectionSummary, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84")]
   [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Coordinate (WGS84). Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
   public async Task<McpToolOutput> ToWGS84FromRDPoint(
      GISBloxClient gisbloxClient,
      [ParamDesc("The X coordinate of the RDPoint.")]
      int x,
      [ParamDesc("The Y coordinate of the RDPoint.")]
      int y,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { x, y, decimals });

      return await ExecuteToolAsync(
         ct => gisbloxClient.Projection.ToWGS84((RDPoint)new(x, y), decimals, ct),
         parameters, toolName, description, result => BuildProjectionSummary(result == null || result.Lat == 0), cancellationToken);
   }
      
   [McpServerTool(Name = "RdsToWgs84Complete")]
   [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Location (WGS84 / RDNew). Includes the sources RDPoint. Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
   public async Task<McpToolOutput> ToWGS84FromRDPointComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("The X coordinate of the RDPoint.")]
      int x,
      [ParamDesc("The Y coordinate of the RDPoint.")]
      int y,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { x, y, decimals });
      
      return await ExecuteToolAsync(
         ct => gisbloxClient.Projection.ToWGS84Complete((RDPoint)new(x, y), decimals, ct),
         parameters, toolName, description, result => BuildProjectionSummary(result == null || result.Lat == 0), cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84List")]
   [Description("Reprojects an array of RDPoints (Amersfoort / EPSG:28992) to an array of Coordinates (WGS84). Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
   public async Task<McpToolOutput> ToWGS84FromRDPointList(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of RD (Rijksdriehoek) points, where each element is a [x, y] array.")]
      int[][] points,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { points, decimals });
      
      return await ExecuteToolAsync(
         async ct =>
         {
            if (points == null || points.Length == 0)
               throw new ArgumentException("Points array cannot be null or empty.", nameof(points));
            var rdPoints = ConvertToRDPointList(points);
            return await gisbloxClient.Projection.ToWGS84(rdPoints, decimals, ct);
         },
         parameters, toolName, description, BuildProjectionSummary, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84CompleteList")]
   [Description("Reprojects an array of RDPoints (Amersfoort / EPSG:28992) to an array of Locations (WGS84 / RDNew). Includes the sources RDPoints. Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
   public async Task<McpToolOutput> ToWGS84FromRDPointListComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of RD (Rijksdriehoek) points, where each element is a [x, y] array.")]
      int[][] points,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { points, decimals });
      
      return await ExecuteToolAsync(
         async ct =>
         {
            if (points == null || points.Length == 0)
               throw new ArgumentException("Points array cannot be null or empty.", nameof(points));
            var rdPoints = ConvertToRDPointList(points);
            return await gisbloxClient.Projection.ToWGS84Complete(rdPoints, decimals, ct);
         },
         parameters, toolName, description, BuildProjectionSummary, cancellationToken);
   }

   #region Internal Helpers

   private static List<RDPoint> ConvertToRDPointList(int[][] points)
   {
      if (points == null || points.Length == 0)
         throw new ArgumentException("Points array cannot be null or empty.", nameof(points));

      return [.. points.Select((point, i) =>
      {
         if (point == null || point.Length != 2)
            throw new ArgumentException($"Each point must be an array of [x, y]. Invalid point at index {i}: {System.Text.Json.JsonSerializer.Serialize(point)}");
         return new RDPoint(point[0], point[1]);
      })];
   }

   private static List<Coordinate> ConvertToCoordinateList(double[][] coordinates)
   {
      if (coordinates == null || coordinates.Length == 0)
         throw new ArgumentException("Coordinates array cannot be null or empty.", nameof(coordinates));

      return [.. coordinates.Select((coord, i) =>
      {
         if (coord == null || coord.Length != 2)
            throw new ArgumentException($"Each coordinate must be an array of [latitude, longitude]. Invalid coordinate at index {i}: {System.Text.Json.JsonSerializer.Serialize(coord)}");
         return new Coordinate(coord[0], coord[1]);
      })];
   }

   private static string BuildProjectionSummary(bool isNull)
   {
      return $"The reprojection was successful{(isNull ? ", but the result is null or invalid. Check if the input parameters are correct" : string.Empty)}.";
   }

   private static string BuildProjectionSummary(IEnumerable? collection)
   {
      int invalidCount = 0;
      var items = collection?.Cast<object>().ToList() ?? [];
      foreach (var item in items)
      {
         if (item == null)
         {
            invalidCount++;
         } 
         else
         {
            var latProperty = item.GetType().GetProperties().FirstOrDefault(p => p.Name.Equals("Lat", StringComparison.OrdinalIgnoreCase));
            if (latProperty != null)
            {
               var latValue = latProperty.GetValue(item);
               if (latValue == null || (latValue is double latDouble && latDouble == 0))
               {
                  invalidCount++;
               }
            }
            else
            {
               invalidCount++;
            }
         }
      }
      return $"The reprojection was successful{(invalidCount > 0 ? $", but **{invalidCount}** items are null or invalid. Check if the input parameters are correct" : string.Empty)}.";      
   }

   #endregion
}