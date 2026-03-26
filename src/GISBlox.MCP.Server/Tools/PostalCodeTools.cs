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
using System.Text;
using System.Text.RegularExpressions;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Atlas")]
[GISBlox.MCP.Server.Attributes.Tags("Postal Codes", "Netherlands")]
[Description("Retrieves information about Dutch postal codes using the GISBlox Postal Codes API.")]
internal class PostalCodeTools : McpToolBase
{
   protected override string ToolGroupName => "Spatial Atlas";

   [McpServerTool(Name = "PostalCodeLookup")]
   [Description("Returns the postal code record for a given postal code. Can include its WKT geometries if includeWktGeometries is true.")]
   public async Task<McpToolOutput> GetPostalCodeRecord(
      GISBloxClient gisbloxClient,
      [ParamDesc("A Dutch postal code (4 digits like '1234' or 6 characters like '1234AB').")]
      string id,
      [ParamDesc("The EPSG code of the target coordinate system. Currently supports EPSG codes 4326 and 28992 only.")]
      int epsg = (int)CoordinateSystem.RDNew,
      [ParamDesc("If true, includes WKT geometry strings in the response.")]
      bool includeWktGeometries = false,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { id, epsg, includeWktGeometries });

      return await ExecuteToolAsync<IPostalCodeRecord>(
         async ct =>
         {
            string cleanId = SanitizePostalCodeId(id, out bool isPostalCode4);
            if (isPostalCode4)
            {
               var pc4Record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(cleanId, (CoordinateSystem)epsg, ct);
               return includeWktGeometries ? pc4Record : RemoveWktGeometries(pc4Record);
            }

            var pc6Record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(cleanId, (CoordinateSystem)epsg, ct);
            return includeWktGeometries ? pc6Record : RemoveWktGeometries(pc6Record);
         },
         parameters, toolName, description, null, cancellationToken);
   }

   [McpServerTool(Name = "PostalCodeNeighboursList")]
   [Description("Returns neighbouring postal codes for a given postal code, with option to include the source postal code. Can include WKT geometries if includeWktGeometries is true.")]
   public async Task<McpToolOutput> GetPostalCodeNeighbours(
      GISBloxClient gisbloxClient,
      [ParamDesc("A Dutch postal code (4 digits like '1234' or 6 characters like '1234AB').")]
      string id,
      [ParamDesc("Determines whether to include the source postal code in the results.")]
      bool includeSourcePostalCode = false,
      [ParamDesc("The EPSG code of the target coordinate system. Currently supports EPSG codes 4326 and 28992 only.")]
      int epsg = (int)CoordinateSystem.RDNew,
      [ParamDesc("If true, includes WKT geometry strings in the response.")]
      bool includeWktGeometries = false,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { id, includeSourcePostalCode, epsg, includeWktGeometries });

      return await ExecuteToolAsync<IPostalCodeRecord>(
         async ct =>
         {
            string cleanId = SanitizePostalCodeId(id, out bool isPostalCode4);
            if (isPostalCode4)
            {
               var neighbours = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(cleanId, includeSourcePostalCode, (CoordinateSystem)epsg, ct);
               return includeWktGeometries ? neighbours : RemoveWktGeometries(neighbours);
            }

            var neighbours6 = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(cleanId, includeSourcePostalCode, (CoordinateSystem)epsg, ct);
            return includeWktGeometries ? neighbours6 : RemoveWktGeometries(neighbours6);
         },
         parameters, toolName, description, BuildNeighbourSummary, cancellationToken);
   }

   [McpServerTool(Name = "GeometryToPostalCodes")]
   [Description("Returns the postal codes for a given geometry in WKT format, with optional buffer in meters. Can include WKT geometries if includeWktGeometries is true. Will return 6 digit postal codes if streetLevelPostCodes is true, else it returns 4 digit ones (default).")]
   public async Task<McpToolOutput> GetPostalCodeByGeometry(
      GISBloxClient gisbloxClient,
      [ParamDesc("The Well-Known Text (WKT) geometry string to search within.")]
      string wkt,
      [ParamDesc("Optional buffer distance in meters to expand the search area.")]
      int buffer = 0,
      [ParamDesc("The EPSG code of the input WKT geometry. Currently supports EPSG codes 4326 and 28992 only.")]
      int wktEpsg = (int)CoordinateSystem.RDNew,
      [ParamDesc("The EPSG code of the target coordinate system. Currently supports EPSG codes 4326 and 28992 only.")]
      int targetEpsg = (int)CoordinateSystem.RDNew,
      [ParamDesc("If true, returns 6-digit postal codes; if false, returns 4-digit postal codes.")]
      bool streetLevelPostCodes = false,
      [ParamDesc("If true, includes WKT geometry strings in the response.")]
      bool includeWktGeometries = false,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { buffer, wktEpsg, targetEpsg, streetLevelPostCodes, includeWktGeometries });

      return await ExecuteToolAsync<IPostalCodeRecord>(
         async ct =>
         {
            if (!streetLevelPostCodes)
            {
               var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode4Record>(wkt, buffer, (CoordinateSystem)wktEpsg, (CoordinateSystem)targetEpsg, ct);
               return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
            }

            var pcRecord6 = await gisbloxClient.PostalCodes.GetPostalCodeByGeometry<PostalCode6Record>(wkt, buffer, (CoordinateSystem)wktEpsg, (CoordinateSystem)targetEpsg, ct);
            return includeWktGeometries ? pcRecord6 : RemoveWktGeometries(pcRecord6);
         },
         parameters, toolName, description, BuildGeometrySummary, cancellationToken);
   }
  
   [McpServerTool(Name = "AreaToPostalCodes")]
   [Description("Returns the postal codes for a given municipality ID, district ID and optionally neighborhood ID. Can include WKT geometries if includeWktGeometries is true. Will return 6 digit postal codes if streetLevelPostCodes is true, else it returns 4 digit ones (default).")]
   public async Task<McpToolOutput> GetPostalCodeByArea(
      GISBloxClient gisbloxClient,
      [ParamDesc("The identifier of the municipality (gemeente).")]
      int gemeenteId,
      [ParamDesc("The identifier of the district (wijk).")]
      int wijkId,
      [ParamDesc("Optional identifier of the neighborhood (buurt). Use -1 to omit.")]
      int buurtId = -1,
      [ParamDesc("The EPSG code of the target coordinate system. Currently supports EPSG codes 4326 and 28992 only.")]
      int epsg = (int)CoordinateSystem.RDNew,
      [ParamDesc("If true, returns 6-digit postal codes; if false, returns 4-digit postal codes.")]
      bool streetLevelPostCodes = false,
      [ParamDesc("If true, includes WKT geometry strings in the response.")]
      bool includeWktGeometries = false,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { gemeenteId, wijkId, buurtId, epsg, streetLevelPostCodes, includeWktGeometries });

      return await ExecuteToolAsync<IPostalCodeRecord>(
         async ct =>
         {
            if (!streetLevelPostCodes)
            {
               var pcRecord = await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode4Record>(gemeenteId, wijkId, buurtId, (CoordinateSystem)epsg, ct);
               return includeWktGeometries ? pcRecord : RemoveWktGeometries(pcRecord);
            }

            var pcRecord6 = await gisbloxClient.PostalCodes.GetPostalCodeByArea<PostalCode6Record>(gemeenteId, wijkId, buurtId, (CoordinateSystem)epsg, ct);
            return includeWktGeometries ? pcRecord6 : RemoveWktGeometries(pcRecord6);

         },
         parameters, toolName, description, BuildAreaSummary, cancellationToken);
   }

   [McpServerTool(Name = "PostalCodeKeyFiguresList")]
   [Description("Returns the key figures (kerncijfers) for a given postal code.")]
   public async Task<McpToolOutput> GetKeyFigures(
      GISBloxClient gisbloxClient,
      [ParamDesc("A Dutch postal code (4 digits like '1234' or 6 characters like '1234AB').")]
      string id,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { id });

      return await ExecuteToolAsync<KerncijferRecord>(
         async ct =>
         {
            string cleanId = SanitizePostalCodeId(id, out _);
            return await gisbloxClient.PostalCodes.GetKeyFigures(cleanId, ct);
         },
         parameters, toolName, description, BuildKerncijferSummary, cancellationToken);
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

   private static (string? sourcePC, IEnumerable<string>? postalCodes) ParsePostalCodeRecord(IPostalCodeRecord record)
   {
      return record switch
      {
         IPostalCode4Record pc4 => (pc4.MetaData.Query, pc4.PostalCode?.Select(pc => pc.Id)),
         IPostalCode6Record pc6 => (pc6.MetaData.Query, pc6.PostalCode?.Select(pc => pc.Id)),
         _ => (null, null)
      };
   }

   private static string BuildNeighbourSummary(IPostalCodeRecord? record)
   {
      if (record == null)
         return "I found **0** neighbouring postal codes.";

      var (sourcePC, postalCodes) = ParsePostalCodeRecord(record);
      if (postalCodes == null)
         return "I found **0** neighbouring postal codes.";

      var neighbors = postalCodes.Where(id => id != sourcePC).ToList();
      
      if (neighbors.Count == 0)
         return $"I found **0** neighbour(s) of postal code {sourcePC}.";

      StringBuilder sb = new();
      foreach (string id in neighbors)
         sb.AppendLine($"- {id}");

      return $"I found **{neighbors.Count}** neighbour(s) of postal code {sourcePC}:\r\n{sb}";
   }

   private static string BuildGeometrySummary(IPostalCodeRecord? record)
   {
      if (record == null)
         return "I found **0** postal codes in the given geometry.";

      var (_, postalCodes) = ParsePostalCodeRecord(record);      
      if (postalCodes == null)
         return "I found **0** postal codes in the given geometry.";

      StringBuilder sb = new();
      foreach (string id in postalCodes)
         sb.AppendLine($"- {id}");

      return $"I found **{postalCodes.Count()}** postal code(s) in the given geometry:\r\n{sb}";
   }

   private static string BuildAreaSummary(IPostalCodeRecord? record)
   {
      if (record == null)
         return "I found **0** postal codes in the given area.";
      
      var (_, postalCodes) = ParsePostalCodeRecord(record);
      if (postalCodes == null)
         return "I found **0** postal codes in the given area.";

      StringBuilder sb = new();
      foreach (string id in postalCodes)
         sb.AppendLine($"- {id}");

      return $"I found **{postalCodes.Count()}** postal code(s) in the given area:\r\n{sb}";
   }   

   private static string BuildKerncijferSummary(KerncijferRecord? record)
   {
      if (record == null)
         return "I found no key figures for the given postal code.";

      return $"I found **{record.MetaData?.TotalAttributes ?? 0}** key figures for the given postal code.";
   }

   #endregion
}