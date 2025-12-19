// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Atlas")]
[GISBlox.MCP.Server.Attributes.Tags("Postal Codes", "Netherlands")]
[Description("Retrieves information about Dutch postal codes using the GISBlox Postal Codes API.")]
internal class PostalCodeTools
{
   [McpServerTool(Name = "PostalCodeLookup")]
   [Description("Returns the postal code record for a given postal code. Can include its WKT geometries if includeWktGeometries is true.")]
   public static async Task<IPostalCodeRecord> GetPostalCodeRecord(GISBloxClient gisbloxClient, string id, int epsg = (int)CoordinateSystem.RDNew, bool includeWktGeometries = false, CancellationToken cancellationToken = default)
   {
      string cleanId = SanitizePostalCodeId(id, out bool isPostalCode4);

      if (isPostalCode4)
      {
         var pc4Record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(cleanId, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? pc4Record : RemoveWktGeometries(pc4Record);
      }
      else
      {
         var pc6Record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(cleanId, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? pc6Record : RemoveWktGeometries(pc6Record);
      }
   }

   [McpServerTool(Name = "PostalCodeNeighboursList")]
   [Description("Returns neighbouring postal codes for a given postal code, with option to include the source postal code. Can include WKT geometries if includeWktGeometries is true.")]
   public static async Task<IPostalCodeRecord> GetPostalCodeNeighbours(GISBloxClient gisbloxClient, string id, bool includeSourcePostalCode = false, int epsg = (int)CoordinateSystem.RDNew, bool includeWktGeometries = false, CancellationToken cancellationToken = default)
   {
      string cleanId = SanitizePostalCodeId(id, out bool isPostalCode4);

      if (isPostalCode4)
      {
         var neighbours = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(cleanId, includeSourcePostalCode, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? neighbours : RemoveWktGeometries(neighbours);
      }
      else
      {
         var neighbours = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(cleanId, includeSourcePostalCode, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? neighbours : RemoveWktGeometries(neighbours);
      }
   }

   [McpServerTool(Name = "GeometryToPostalCodes")]
   [Description("Returns the postal codes for a given geometry in WKT format, with optional buffer in meters. Can include WKT geometries if includeWktGeometries is true. Will return 6 digit postal codes if streetLevelPostCodes is true, else it returns 4 digit ones (default).")]
   public static async Task<IPostalCodeRecord> GetPostalCodeByGeometry(GISBloxClient gisbloxClient, string wkt, int buffer = 0, int wktEpsg = (int)CoordinateSystem.RDNew, int targetEpsg = (int)CoordinateSystem.RDNew, bool streetLevelPostCodes = false, bool includeWktGeometries = false, CancellationToken cancellationToken = default)
   {
      if (!streetLevelPostCodes)
      {
         var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode4Record>(wkt, buffer, (CoordinateSystem)wktEpsg, (CoordinateSystem)targetEpsg, cancellationToken);
         return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
      }
      else
      {
         var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode6Record>(wkt, buffer, (CoordinateSystem)wktEpsg, (CoordinateSystem)targetEpsg, cancellationToken);
         return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
      }
   }

   [McpServerTool(Name = "AreaToPostalCodes")]
   [Description("Returns the postal codes for a given municipality ID, district ID and optionally neighborhood ID. Can include WKT geometries if includeWktGeometries is true. Will return 6 digit postal codes if streetLevelPostCodes is true, else it returns 4 digit ones (default).")]
   public static async Task<IPostalCodeRecord> GetPostalCodeByArea(GISBloxClient gisbloxClient, int gemeenteId, int wijkId, int buurtId = -1, int epsg = (int)CoordinateSystem.RDNew, bool streetLevelPostCodes = false, bool includeWktGeometries = false, CancellationToken cancellationToken = default)
   {
      if (!streetLevelPostCodes)
      {
         var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode4Record>(gemeenteId, wijkId, buurtId, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
      }
      else
      {
         var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode6Record>(gemeenteId, wijkId, buurtId, (CoordinateSystem)epsg, cancellationToken);
         return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
      }
   }

   [McpServerTool(Name = "PostalCodeKeyFiguresList")]
   [Description("Returns the key figures (kerncijfers) for a given postal code.")]
   public static async Task<KerncijferRecord> GetKeyFigures(GISBloxClient gisbloxClient, string id, CancellationToken cancellationToken = default)
   {
      string cleanId = SanitizePostalCodeId(id, out bool _);
      return await gisbloxClient.PostalCodes.GetKeyFigures(cleanId, cancellationToken);
   }

   #region Internal Helpers

   private static string SanitizePostalCodeId(string id, out bool isPC4)
   {
      string cleanId = id?.Replace(" ", string.Empty) ?? string.Empty;
      bool isValid = IsValidPostalCode4(cleanId) || IsValidPostalCode6(cleanId);
      if (!isValid)
      {
         throw new ArgumentException("Invalid Dutch postal code.", nameof(id));
      }

      isPC4 = cleanId.Length == 4;
      return cleanId;
   }

   private static bool IsValidPostalCode4(string postalCode)
   {
      string dutchPostalCode = @"^[1-9][0-9]{3}$";
      return Regex.IsMatch(postalCode, dutchPostalCode, RegexOptions.IgnoreCase);
   }

   private static bool IsValidPostalCode6(string postalCode)
   {
      string dutchPostalCode = @"^[1-9][0-9]{3}?(?!sa|sd|ss)[a-z]{2}$";
      return Regex.IsMatch(postalCode, dutchPostalCode, RegexOptions.IgnoreCase);
   }

   private static PostalCode4Record RemoveWktGeometries(PostalCode4Record record)
   {
      record.PostalCode.ForEach(pc => { pc.Location.Geometry.WKT = null; });
      return record;
   }

   private static PostalCode6Record RemoveWktGeometries(PostalCode6Record record)
   {
      record.PostalCode.ForEach(pc => { pc.Location.Geometry.WKT = null; });
      return record;
   }

   #endregion
}