// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[Description("Reprojects coordinates to WGS84 and RDNew using the GISBlox Projection API.")]
internal class ProjectionTools
{
    [McpServerTool]
    [Description("Reprojects a Coordinate (WGS84) to an RDPoint (Amersfoort / EPSG:28992).")]
    public static async Task<RDPoint> ToRDSFromCoordinate(GISBloxClient gisbloxClient, Coordinate coordinate, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToRDS(coordinate, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects a Coordinate (WGS84) to a Location (WGS84 / RDNew). Includes the sources coordinate.")]
    public static async Task<Location> ToRDSFromCoordinateComplete(GISBloxClient gisbloxClient, Coordinate coordinate, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToRDSComplete(coordinate, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects a List of Coordinates (WGS84) to a List of RDPoints (Amersfoort / EPSG:28992).")]
    public static async Task<List<RDPoint>> ToRDSFromCoordinateList(GISBloxClient gisbloxClient, List<Coordinate> coordinates, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToRDS(coordinates, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects a List of Coordinates (WGS84) to a List of Locations (WGS84 / RDNew). Includes the sources coordinates.")]
    public static async Task<List<Location>> ToRDSFromCoordinateListComplete(GISBloxClient gisbloxClient, List<Coordinate> coordinates, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToRDSComplete(coordinates, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Coordinate (WGS84). Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
    public static async Task<Coordinate> ToWGS84FromRDPoint(GISBloxClient gisbloxClient, RDPoint rdPoint, int decimals = -1, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToWGS84(rdPoint, decimals, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects an RDPoint (Amersfoort / EPSG:28992) to a Location (WGS84 / RDNew). Includes the sources RDPoint. Optionally rounds the result to the specified number of decimals (default -1, no rounding).")]
    public static async Task<Location> ToWGS84FromRDPointComplete(GISBloxClient gisbloxClient, RDPoint rdPoint, int decimals = -1, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToWGS84Complete(rdPoint, decimals, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects a List of RDPoints (Amersfoort / EPSG:28992) to a List of Coordinates (WGS84). Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
    public static async Task<List<Coordinate>> ToWGS84FromRDPointList(GISBloxClient gisbloxClient, List<RDPoint> rdPoints, int decimals = -1, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToWGS84(rdPoints, decimals, cancellationToken);
    }

    [McpServerTool]
    [Description("Reprojects a List of RDPoints (Amersfoort / EPSG:28992) to a List of Locations (WGS84 / RDNew). Includes the sources RDPoints. Optionally rounds the results to the specified number of decimals (default -1, no rounding).")]
    public static async Task<List<Location>> ToWGS84FromRDPointListComplete(GISBloxClient gisbloxClient, List<RDPoint> rdPoints, int decimals = -1, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.Projection.ToWGS84Complete(rdPoints, decimals, cancellationToken);
    }
}