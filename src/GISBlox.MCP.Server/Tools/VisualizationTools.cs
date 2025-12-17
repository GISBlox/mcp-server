// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Storytelling Layers")]
[GISBlox.MCP.Server.Attributes.Tags("Visualization", "Conversational Data", "Postal Codes", "GeoJSON", "ZipChat")]
[Description("Tools to visualize geometries using geojson.io.")]
internal class VisualizationTools
{
   private const string GeoJsonIoUrlPrefix = "https://geojson.io/#data=data:text/x-url,";
   private const string GISBloxServicesBaseUrl = "https://services.gisblox.com/v1";
   private const string ZipChatCopilotUrlPrefix = "https://zipchat.gisblox.com/?pc=";

   private static readonly JsonSerializerOptions CompactJsonOptions = new() { WriteIndented = false };


   [McpServerTool(Name = "PostalCodeVisualize")]
   [Description("Generates a geojson.io URL to visualize the geometry of a given postal code.")]
   public static async Task<string> VisualizePostalCode(GISBloxClient gisbloxClient, string postalCode, CancellationToken cancellationToken = default)
   {  
      string cleanId = SanitizePostalCodeId(postalCode, out bool isPostalCode4);
      string identifier = isPostalCode4 ? $"PC4_{cleanId}" : $"PC6_{cleanId}";

      string feature;
      if (isPostalCode4)
      {
         PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(cleanId, CoordinateSystem.WGS84, cancellationToken);

         var pc4 = GetFirstPostalCodeOrThrow(record.PostalCode, cleanId, "4");
         feature = await WktToFeature(gisbloxClient, pc4, cancellationToken);
      }
      else
      {
         PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(cleanId, CoordinateSystem.WGS84, cancellationToken);

         var pc6 = GetFirstPostalCodeOrThrow(record.PostalCode, cleanId, "6");
         feature = await WktToFeature(gisbloxClient, pc6, cancellationToken);
      }

      string geojson = CreateFeatureCollection([feature]);
      string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, identifier, cancellationToken);

      return CreateGeoJsonIoUrl(dataLakeUrl);
   }

   [McpServerTool(Name = "PostalCodeVisualizeNeighbours")]
   [Description("Generates a geojson.io URL to visualize the geometry of a given postal code and its neighbouring postal codes.")]
   public static async Task<string> VisualizePostalCodeNeighbours(GISBloxClient gisbloxClient, string postalCode, CancellationToken cancellationToken = default)
   {
      string cleanId = SanitizePostalCodeId(postalCode, out bool isPostalCode4);
      string identifier = isPostalCode4 ? $"PC4N_{cleanId}" : $"PC6N_{cleanId}";

      List<string> features = [];
      if (isPostalCode4)
      {
         PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(cleanId, false, CoordinateSystem.WGS84, cancellationToken);

         if (record.PostalCode is null || record.PostalCode.Count == 0)
            throw new InvalidOperationException($"No neighbouring postal codes returned for '{cleanId}'.");

         features = new(record.PostalCode.Count);
         foreach (PostalCode4 pc4 in record.PostalCode)
         {
            cancellationToken.ThrowIfCancellationRequested();
            string feature = await WktToFeature(gisbloxClient, pc4, cancellationToken);
            features.Add(feature);

            await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
         }
      }
      else
      {
         PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(cleanId, false, CoordinateSystem.WGS84, cancellationToken);

         if (record.PostalCode is null || record.PostalCode.Count == 0)
            throw new InvalidOperationException($"No neighbouring postal codes returned for '{cleanId}'.");

         features = new(record.PostalCode.Count);
         foreach (PostalCode6 pc6 in record.PostalCode)
         {
            cancellationToken.ThrowIfCancellationRequested();
            string feature = await WktToFeature(gisbloxClient, pc6, cancellationToken);
            features.Add(feature);

            await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
         }
      }

      string geojson = CreateFeatureCollection(features);
      string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, identifier, cancellationToken);

      return CreateGeoJsonIoUrl(dataLakeUrl);
   }


   //[McpServerTool(Name = "visualize_pc4_get")]
   //[Description("Generates a geojson.io URL to visualize the geometry of a given postal code (4 digits).")]
   //public static async Task<string> VisualizePostalCode4(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
   //{
   //   ValidatePostalCodeId(postalCodeId);
   //   PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(postalCodeId, CoordinateSystem.WGS84, cancellationToken);

   //   var pc4 = GetFirstPostalCodeOrThrow(record.PostalCode, postalCodeId, "4");
   //   string feature = await WktToFeature(gisbloxClient, pc4, cancellationToken);

   //   string geojson = CreateFeatureCollection([feature]);
   //   string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC4_{postalCodeId}", cancellationToken);

   //   return CreateGeoJsonIoUrl(dataLakeUrl);
   //}

   //[McpServerTool(Name = "visualize_pc4_neighbours_list")]
   //[Description("Generates a geojson.io URL to visualize the geometry of a given postal code (4 digits) and its neighbouring postal codes.")]
   //public static async Task<string> VisualizePostalCode4Neighbours(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
   //{
   //   ValidatePostalCodeId(postalCodeId);
   //   PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(postalCodeId, false, CoordinateSystem.WGS84, cancellationToken);

   //   if (record.PostalCode is null || record.PostalCode.Count == 0)
   //      throw new InvalidOperationException($"No neighbouring postal codes returned for '{postalCodeId}'.");

   //   List<string> features = new(record.PostalCode.Count);

   //   foreach (PostalCode4 pc4 in record.PostalCode)
   //   {
   //      cancellationToken.ThrowIfCancellationRequested();
   //      string feature = await WktToFeature(gisbloxClient, pc4, cancellationToken);
   //      features.Add(feature);

   //      await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
   //   }

   //   string geojson = CreateFeatureCollection(features);
   //   string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC4N_{postalCodeId}", cancellationToken);

   //   return CreateGeoJsonIoUrl(dataLakeUrl);
   //}

   //[McpServerTool(Name = "visualize_pc6_get")]
   //[Description("Generates a geojson.io URL to visualize the geometry of a given postal code (6 digits).")]
   //public static async Task<string> VisualizePostalCode6(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
   //{
   //   ValidatePostalCodeId(postalCodeId);
   //   PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(postalCodeId, CoordinateSystem.WGS84, cancellationToken);

   //   var pc6 = GetFirstPostalCodeOrThrow(record.PostalCode, postalCodeId, "6");

   //   string feature = await WktToFeature(gisbloxClient, pc6, cancellationToken);

   //   string geojson = CreateFeatureCollection([feature]);
   //   string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC6_{postalCodeId}", cancellationToken);

   //   return CreateGeoJsonIoUrl(dataLakeUrl);
   //}

   //[McpServerTool(Name = "visualize_pc6_neighbours_list")]
   //[Description("Generates a geojson.io URL to visualize the geometry of a given postal code (6 digits) and its neighbouring postal codes.")]
   //public static async Task<string> VisualizePostalCode6Neighbours(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
   //{
   //   ValidatePostalCodeId(postalCodeId);
   //   PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(postalCodeId, false, CoordinateSystem.WGS84, cancellationToken);

   //   if (record.PostalCode is null || record.PostalCode.Count == 0)
   //      throw new InvalidOperationException($"No neighbouring postal codes returned for '{postalCodeId}'.");

   //   List<string> features = new(record.PostalCode.Count);

   //   foreach (PostalCode6 pc6 in record.PostalCode)
   //   {
   //      cancellationToken.ThrowIfCancellationRequested();
   //      string feature = await WktToFeature(gisbloxClient, pc6, cancellationToken);
   //      features.Add(feature);

   //      await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
   //   }

   //   string geojson = CreateFeatureCollection(features);
   //   string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC6N_{postalCodeId}", cancellationToken);

   //   return CreateGeoJsonIoUrl(dataLakeUrl);
   //}

   [McpServerTool(Name = "ZipChatQuery")]
   [Description("Generates a ZipChat Copilot URL to retrieve detailed information about a given postal code or its neighbours.")]
   public static string AskZipChatCopilot(string postalCodeId, bool showNeighbours = false)
   {
      return $"{ZipChatCopilotUrlPrefix}{postalCodeId}&c=1&n={(showNeighbours ? "1" : "0")}";
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

   private static TPostalCode GetFirstPostalCodeOrThrow<TPostalCode>(IList<TPostalCode>? list, string requestedId, string kind)
   {
      if (list is null || list.Count == 0)
         throw new InvalidOperationException($"No {kind}-digit postal code geometry found for '{requestedId}'.");
      return list[0];
   }   

   private static async Task<string> WktToFeature(GISBloxClient gisbloxClient, PostalCode4 pc4, CancellationToken cancellationToken = default)
   {
      string feature = await ConversionTools.ConvertToGeoJson(gisbloxClient, pc4.Location.Geometry.WKT, false, cancellationToken);
      return AddFeatureProperties(feature, new Dictionary<string, string>
      {
         { "postcode", pc4.Id },
         { "wijk(en)", pc4.Location.Wijken },
         { "gemeente", pc4.Location.Gemeente },
         { "omtrek", FormatDouble(pc4.Location.Geometry.PerimeterM) + " meter" },
         { "oppervlakte", FormatDouble(pc4.Location.Geometry.AreaM2) + " m2" }
      });
   }

   private static async Task<string> WktToFeature(GISBloxClient gisbloxClient, PostalCode6 pc6, CancellationToken cancellationToken = default)
   {
      string feature = await ConversionTools.ConvertToGeoJson(gisbloxClient, pc6.Location.Geometry.WKT, false, cancellationToken);
      return AddFeatureProperties(feature, new Dictionary<string, string>
      {
         { "postcode", pc6.Id },
         { "buurt", pc6.Location.Buurt },
         { "wijk", pc6.Location.Wijk },
         { "gemeente", pc6.Location.Gemeente },
         { "omtrek", FormatDouble(pc6.Location.Geometry.PerimeterM) + " meter" },
         { "oppervlakte", FormatDouble(pc6.Location.Geometry.AreaM2) + " m2" }
      });
   }

   private static string AddFeatureProperties(string geoJson, Dictionary<string, string> featureProperties)
   {
      try
      {
         JsonNode? root = JsonNode.Parse(geoJson);
         if (root is JsonObject obj)
         {           
            JsonObject props;
            if (obj["properties"] is JsonObject existing)
            {
               props = existing;
            }
            else
            {
               props = new JsonObject();
               obj["properties"] = props;
            }

            foreach (var item in featureProperties)
            {
               props[item.Key] = item.Value;
            }

            return root.ToJsonString(CompactJsonOptions);
         }
      }
      catch (JsonException ex)
      {
         System.Diagnostics.Debug.WriteLine($"Failed to parse or modify GeoJSON: {ex.Message}");
      }
      return geoJson;
   }

   private static async Task<string> UploadToDataLakeAndCreatePublicUrl(GISBloxClient gisbloxClient, string geojson, string fileIdentifier, CancellationToken cancellationToken)
   {
      cancellationToken.ThrowIfCancellationRequested();

      string filename = await UploadToDataLake(gisbloxClient, geojson, fileIdentifier, cancellationToken);
      if (string.IsNullOrWhiteSpace(filename))
         throw new IOException("Upload to Data Lake did not return a valid filename.");

      cancellationToken.ThrowIfCancellationRequested();

      var customerFolder = await GetCustomerDataLakeFolder(gisbloxClient, cancellationToken);
      if (string.IsNullOrWhiteSpace(customerFolder))
         throw new IOException("Could not retrieve customer folder ID from Data Lake.");

      return $"{GISBloxServicesBaseUrl}/datalake/load/{filename}?folderId={customerFolder}";
   }

   private static async Task<string> UploadToDataLake(GISBloxClient gisbloxClient, string geojson, string fileIdentifier, CancellationToken cancellationToken)
   {
      cancellationToken.ThrowIfCancellationRequested();

      string filename = $"viz_{fileIdentifier}_{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.geojson";

      if (!await gisbloxClient.DataLake.UploadFileData(geojson, filename, cancellationToken))
         throw new IOException($"Failed to upload '{filename}' to Data Lake.");

      return filename;
   }

   private static async Task<string> GetCustomerDataLakeFolder(GISBloxClient gisbloxClient, CancellationToken cancellationToken)
   {
      cancellationToken.ThrowIfCancellationRequested();

      var customerFolder = await gisbloxClient.DataLake.GetCustomerFolder(cancellationToken);
      return customerFolder.FolderId;
   }

   private static string CreateGeoJsonIoUrl(string geojsonDataUrl)
   {
      return GeoJsonIoUrlPrefix + EncodeUriComponent(geojsonDataUrl);
   }

   private static string CreateFeatureCollection(IReadOnlyList<string> features)
   {
      return "{ \"type\": \"FeatureCollection\", \"features\": [" + string.Join(",", features) + "] }";
   }

   private static string EncodeUriComponent(string value) => Uri.EscapeDataString(value);
      
   private static string FormatDouble(double value)
   {      
      return Math.Round(value)
         .ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
         .Replace(",", ".");
   }

   #endregion
}