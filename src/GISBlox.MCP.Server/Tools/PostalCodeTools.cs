// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[Description("Retrieves information about Dutch postal codes using the GISBlox Postal Codes API.")]
internal class PostalCodeTools
{
    [McpServerTool]
    [Description("Returns the postal code (4 digits) record for a given postal code ID.")]
    public static async Task<PostalCode4Record> GetPostalCode4Record(GISBloxClient gisbloxClient, string id, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(id, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the postal code (6 digits) record for a given postal code ID.")]
    public static async Task<PostalCode6Record> GetPostalCode6Record(GISBloxClient gisbloxClient, string id, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(id, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns neighbouring postal codes (4 digits) for a given postal code ID, with option to include the source postal code.")]
    public static async Task<PostalCode4Record> GetPostalCode4Neighbours(GISBloxClient gisbloxClient, string id, bool includeSourcePostalCode = false, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(id, includeSourcePostalCode, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns neighbouring postal codes (6 digits) for a given postal code ID, with option to include the source postal code.")]
    public static async Task<PostalCode6Record> GetPostalCode6Neighbours(GISBloxClient gisbloxClient, string id, bool includeSourcePostalCode = false, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(id, includeSourcePostalCode, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the postal codes (4 digits) for a given geometry in WKT format, with optional buffer in meters.")]
    public static async Task<PostalCode4Record> GetPostalCode4ByGeometry(GISBloxClient gisbloxClient, string wkt, int buffer = 0, CoordinateSystem wktEpsg = CoordinateSystem.RDNew, CoordinateSystem targetEpsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode4Record>(wkt, buffer, wktEpsg, targetEpsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the postal codes (6 digits) for a given geometry in WKT format, with optional buffer in meters.")]
    public static async Task<PostalCode6Record> GetPostalCode6ByGeometry(GISBloxClient gisbloxClient, string wkt, int buffer = 0, CoordinateSystem wktEpsg = CoordinateSystem.RDNew, CoordinateSystem targetEpsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode6Record>(wkt, buffer, wktEpsg, targetEpsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the postal codes (4 digits) for a given municipality ID, district ID and optionally neighborhood ID.")]
    public static async Task<PostalCode4Record> GetPostalCode4ByArea(GISBloxClient gisbloxClient, int gemeenteId, int wijkId, int buurtId = -1, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode4Record>(gemeenteId, wijkId, buurtId, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the postal codes (6 digits) for a given municipality ID, district ID and optionally neighborhood ID.")]
    public static async Task<PostalCode6Record> GetPostalCode6ByArea(GISBloxClient gisbloxClient, int gemeenteId, int wijkId, int buurtId = -1, CoordinateSystem epsg = CoordinateSystem.RDNew, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode6Record>(gemeenteId, wijkId, buurtId, epsg, cancellationToken);
    }

    [McpServerTool]
    [Description("Returns the key figures (kerncijfers) for a given postal code (4 or 6 digits).")]
    public static async Task<KerncijferRecord> GetKeyFigures(GISBloxClient gisbloxClient, string id, CancellationToken cancellationToken = default)
    {
        return await gisbloxClient.PostalCodes.GetKeyFigures(id, cancellationToken);
    }   
}