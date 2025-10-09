// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

[McpServerToolType]
[Description("Tools to visualize geometries using geojson.io.")]
internal class VisualizationTools
{
    private const string GeoJsonIoUrlPrefix = "https://geojson.io/#data=data:text/x-url,";
    private const string GISBloxServicesBaseUrl = "https://services.gisblox.com/v1";
    private const string ZipChatCopilotUrlPrefix = "https://zipchat.gisblox.com/?pc=";    

    private static readonly JsonSerializerOptions CompactJsonOptions = new() { WriteIndented = false };

    [McpServerTool]
    [Description("Generates a geojson.io URL to visualize the geometry of a given postal code (4 digits).")]
    public static async Task<string> VisualizePostalCode4(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
    {
        ValidatePostalCodeId(postalCodeId);
        PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode4Record>(postalCodeId, CoordinateSystem.WGS84, cancellationToken);

        var pc = GetFirstPostalCodeOrThrow(record.PostalCode, postalCodeId, "4");
        string feature = await WktToFeature(gisbloxClient, pc.Id, pc.Location.Geometry.WKT, cancellationToken);

        string geojson = CreateFeatureCollection([feature]);                
        string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC4_{postalCodeId}", cancellationToken);

        return CreateGeoJsonIoUrl(dataLakeUrl);
    }

    [McpServerTool]
    [Description("Generates a geojson.io URL to visualize the geometry of a given postal code (4 digits) and its neighbouring postal codes.")]
    public static async Task<string> VisualizePostalCode4Neighbours(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
    {
        ValidatePostalCodeId(postalCodeId);
        PostalCode4Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode4Record>(postalCodeId, false, CoordinateSystem.WGS84, cancellationToken);

        if (record.PostalCode is null || record.PostalCode.Count == 0)
            throw new InvalidOperationException($"No neighbouring postal codes returned for '{postalCodeId}'.");

        List<string> features = new(record.PostalCode.Count);

        foreach (PostalCode4 pc in record.PostalCode)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string feature = await WktToFeature(gisbloxClient, pc.Id, pc.Location.Geometry.WKT, cancellationToken);
            features.Add(feature);

            await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
        }

        string geojson = CreateFeatureCollection(features);
        string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC4N_{postalCodeId}", cancellationToken);

        return CreateGeoJsonIoUrl(dataLakeUrl);
    }

    [McpServerTool]
    [Description("Generates a geojson.io URL to visualize the geometry of a given postal code (6 digits).")]
    public static async Task<string> VisualizePostalCode6(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
    {
        ValidatePostalCodeId(postalCodeId);
        PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeRecord<PostalCode6Record>(postalCodeId, CoordinateSystem.WGS84, cancellationToken);

        var pc = GetFirstPostalCodeOrThrow(record.PostalCode, postalCodeId, "6");

        string feature = await WktToFeature(gisbloxClient, pc.Id, pc.Location.Geometry.WKT, cancellationToken);

        string geojson = CreateFeatureCollection([feature]);
        string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC6_{postalCodeId}", cancellationToken);

        return CreateGeoJsonIoUrl(dataLakeUrl);
    }

    [McpServerTool]
    [Description("Generates a geojson.io URL to visualize the geometry of a given postal code (6 digits) and its neighbouring postal codes.")]
    public static async Task<string> VisualizePostalCode6Neighbours(GISBloxClient gisbloxClient, string postalCodeId, CancellationToken cancellationToken = default)
    {
        ValidatePostalCodeId(postalCodeId);
        PostalCode6Record record = await gisbloxClient.PostalCodes.GetPostalCodeNeighbours<PostalCode6Record>(postalCodeId, false, CoordinateSystem.WGS84, cancellationToken);

        if (record.PostalCode is null || record.PostalCode.Count == 0)
            throw new InvalidOperationException($"No neighbouring postal codes returned for '{postalCodeId}'.");

        List<string> features = new(record.PostalCode.Count);

        foreach (PostalCode6 pc in record.PostalCode)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string feature = await WktToFeature(gisbloxClient, pc.Id, pc.Location.Geometry.WKT, cancellationToken);
            features.Add(feature);

            await Task.Delay(495, cancellationToken); // To avoid exceeding API call quota
        }

        string geojson = CreateFeatureCollection(features);
        string dataLakeUrl = await UploadToDataLakeAndCreatePublicUrl(gisbloxClient, geojson, $"PC6N_{postalCodeId}", cancellationToken);

        return CreateGeoJsonIoUrl(dataLakeUrl);
    }

    [McpServerTool]
    [Description("Generates a ZipChat Copilot URL to retrieve detailed information about a given postal code (4 or 6 digits) or its neighbours.")]
    public static string AskZipChatCopilot(string postalCodeId, bool showNeighbours = false)
    {
        return $"{ZipChatCopilotUrlPrefix}{postalCodeId}&c=1&n={(showNeighbours ? "1" : "0")}";
    }

    #region Internal Helpers

    private static void ValidatePostalCodeId(string postalCodeId)
    {
        if (string.IsNullOrWhiteSpace(postalCodeId))
            throw new ArgumentException("Postal code id must be provided.", nameof(postalCodeId));
    }

    private static TPostalCode GetFirstPostalCodeOrThrow<TPostalCode>(IList<TPostalCode>? list, string requestedId, string kind)
    {
        if (list is null || list.Count == 0)
            throw new InvalidOperationException($"No {kind}-digit postal code geometry found for '{requestedId}'.");
        return list[0];
    }

    private static async Task<string> WktToFeature(GISBloxClient gisbloxClient, string postalCodeId, string wkt, CancellationToken cancellationToken = default)
    {
        string feature = await ConversionTools.ConvertToGeoJson(gisbloxClient, wkt, false, cancellationToken);
        return AddPostalCodeProperty(feature, postalCodeId);
    }

    private static string AddPostalCodeProperty(string geoJson, string postalCodeId)
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

                props["postcode"] = postalCodeId;
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
        
        string filename = $"viz_{fileIdentifier}_{DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.json";

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

    #endregion
}