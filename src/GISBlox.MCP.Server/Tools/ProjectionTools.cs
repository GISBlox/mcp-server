// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("World Views")]
[GISBlox.MCP.Server.Attributes.Tags("Projection", "Coordinates", "RDNew", "WGS84")]
[Description("Reprojects WGS84 coordinates to RDNew, and vice versa, using the GISBlox Projection API.")]
internal class ProjectionTools
{
   [McpServerTool(Name = "Wgs84ToRds")]
   [Description("Reprojects a Coordinate (WGS84) to an RDPoint (Amersfoort / EPSG:28992).")]
   public static async Task<RDPoint> ToRDSFromCoordinate(
      GISBloxClient gisbloxClient,
      [ParamDesc("The latitude of the WGS84 coordinate.")]
      double lat,      
      [ParamDesc("The longitude of the WGS84 coordinate.")]
      double lon,
      CancellationToken cancellationToken = default)
   {
      Coordinate coordinate = new(lat, lon);
      return await gisbloxClient.Projection.ToRDS(coordinate, cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsComplete")]
   [Description("Reprojects a Coordinate (WGS84) to a Location (WGS84 / RDNew). Includes the sources coordinate.")]
   public static async Task<Location> ToRDSFromCoordinateComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("The latitude of the WGS84 coordinate.")]
      double lat,
      [ParamDesc("The longitude of the WGS84 coordinate.")]
      double lon,
      CancellationToken cancellationToken = default)
   {
      Coordinate coordinate = new(lat, lon);
      return await gisbloxClient.Projection.ToRDSComplete(coordinate, cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsList")]
   [Description("Reprojects an array of WGS84 coordinates to an array of RDPoints (Amersfoort / EPSG:28992).")]
   public static async Task<List<RDPoint>> ToRDSFromCoordinateList(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of WGS84 coordinates, where each element is a [latitude, longitude] array.")]
      double[][] coordinates,
      CancellationToken cancellationToken = default)
   {
      if (coordinates == null || coordinates.Length == 0)
         throw new ArgumentException("Coordinates array cannot be null or empty.", nameof(coordinates));

      var coordList = ConvertToCoordinateList(coordinates);
      return await gisbloxClient.Projection.ToRDS(coordList, cancellationToken);
   }

   [McpServerTool(Name = "Wgs84ToRdsCompleteList")]
   [Description("Reprojects an array of WGS84 coordinates to an array of Locations (WGS84 / RDNew). Includes the sources coordinates.")]
   public static async Task<List<Location>> ToRDSFromCoordinateListComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of WGS84 coordinates, where each element is a [latitude, longitude] array.")]
      double[][] coordinates,
      CancellationToken cancellationToken = default)
   {
      if (coordinates == null || coordinates.Length == 0)
         throw new ArgumentException("Coordinates array cannot be null or empty.", nameof(coordinates));

      var coordList = ConvertToCoordinateList(coordinates);
      return await gisbloxClient.Projection.ToRDSComplete(coordList, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84")]
   [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Coordinate (WGS84). Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
   public static async Task<Coordinate> ToWGS84FromRDPoint(
      GISBloxClient gisbloxClient,
      [ParamDesc("The X coordinate of the RDPoint.")]
      int x,
      [ParamDesc("The Y coordinate of the RDPoint.")]
      int y,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      RDPoint rdPoint = new(x, y);
      return await gisbloxClient.Projection.ToWGS84(rdPoint, decimals, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84Complete")]
   [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Location (WGS84 / RDNew). Includes the sources RDPoint. Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
   public static async Task<Location> ToWGS84FromRDPointComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("The X coordinate of the RDPoint.")]
      int x,
      [ParamDesc("The Y coordinate of the RDPoint.")]
      int y,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      RDPoint rdPoint = new(x, y);
      return await gisbloxClient.Projection.ToWGS84Complete(rdPoint, decimals, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84List")]
   [Description("Reprojects an array of RDPoints (Amersfoort / EPSG:28992) to an array of Coordinates (WGS84). Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
   public static async Task<List<Coordinate>> ToWGS84FromRDPointList(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of RD (Rijksdriehoek) points, where each element is a [x, y] array.")]
      int[][] points,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      if (points == null || points.Length == 0)
         throw new ArgumentException("Points array cannot be null or empty.", nameof(points));

      var rdPoints = ConvertToRDPointList(points);
      return await gisbloxClient.Projection.ToWGS84(rdPoints, decimals, cancellationToken);
   }

   [McpServerTool(Name = "RdsToWgs84CompleteList")]
   [Description("Reprojects an array of RDPoints (Amersfoort / EPSG:28992) to an array of Locations (WGS84 / RDNew). Includes the sources RDPoints. Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
   public static async Task<List<Location>> ToWGS84FromRDPointListComplete(
      GISBloxClient gisbloxClient,
      [ParamDesc("Array of RD (Rijksdriehoek) points, where each element is a [x, y] array.")]
      int[][] points,
      [ParamDesc("Number of decimal places to round to. Use -1 for no rounding.")]
      int decimals = -1,
      CancellationToken cancellationToken = default)
   {
      if (points == null || points.Length == 0)
         throw new ArgumentException("Points array cannot be null or empty.", nameof(points));

      var rdPoints = ConvertToRDPointList(points);
      return await gisbloxClient.Projection.ToWGS84Complete(rdPoints, decimals, cancellationToken);
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

   #endregion
}