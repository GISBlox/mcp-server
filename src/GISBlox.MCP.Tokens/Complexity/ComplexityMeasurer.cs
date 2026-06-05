// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Text.Json;
using System.Text.RegularExpressions;

namespace GISBlox.MCP.Tokens.Complexity;

/// <summary>
/// Measures complexity metrics from tool output data: byte size, feature count, and vertex count.
/// Handles GeoJSON, custom API responses, WKT geometries, and coordinate strings.
/// </summary>
public static partial class ComplexityMeasurer
{
   private static readonly JsonSerializerOptions _jsonOptions = new()
   {
      WriteIndented = false
   };

   // Regex patterns for WKT and coordinate parsing
   [GeneratedRegex(@"POINT\s*\(([^)]+)\)", RegexOptions.IgnoreCase)]
   private static partial Regex WktPointRegex();

   [GeneratedRegex(@"LINESTRING\s*\(([^)]+)\)", RegexOptions.IgnoreCase)]
   private static partial Regex WktLineStringRegex();

   [GeneratedRegex(@"POLYGON\s*\(\(([^)]+)\)\)", RegexOptions.IgnoreCase)]
   private static partial Regex WktPolygonRegex();

   [GeneratedRegex(@"(-?\d+\.?\d*)\s+(-?\d+\.?\d*)")]
   private static partial Regex CoordinatePairRegex();

   /// <summary>
   /// Measures the output byte size, feature count, and vertex count from the tool result data.
   /// </summary>
   /// <param name="data">The tool output data object (typically McpToolOutput.Data).</param>
   /// <param name="outputBytes">The size of the JSON-serialized data in bytes.</param>
   /// <param name="featureCount">The number of features/geometries detected.</param>
   /// <param name="vertexCount">The total number of vertices across all geometries.</param>
   public static void Measure(object? data, out long outputBytes, out int featureCount, out int vertexCount)
   {
      outputBytes = 0;
      featureCount = 0;
      vertexCount = 0;

      if (data == null)
         return;

      // Serialize to measure byte size
      string json = JsonSerializer.Serialize(data, _jsonOptions);
      outputBytes = System.Text.Encoding.UTF8.GetByteCount(json);

      // Parse JSON and analyze structure
      try
      {
         using var doc = JsonDocument.Parse(json);
         var root = doc.RootElement;

         AnalyzeJsonElement(root, ref featureCount, ref vertexCount);
      }
      catch
      {
         // Parsing failed - counts remain 0
      }
   }

   private static void AnalyzeJsonElement(JsonElement element, ref int featureCount, ref int vertexCount)
   {
      switch (element.ValueKind)
      {
         case JsonValueKind.Object:
            AnalyzeObject(element, ref featureCount, ref vertexCount);
            break;

         case JsonValueKind.Array:
            AnalyzeArray(element, ref featureCount, ref vertexCount);
            break;
      }
   }

   private static void AnalyzeObject(JsonElement obj, ref int featureCount, ref int vertexCount)
   {
      // Pattern 1: GeoJSON FeatureCollection
      if (obj.TryGetProperty("type", out var typeProp) &&
          typeProp.ValueKind == JsonValueKind.String &&
          typeProp.GetString() == "FeatureCollection" &&
          obj.TryGetProperty("features", out var featuresProp) &&
          featuresProp.ValueKind == JsonValueKind.Array)
      {
         featureCount += featuresProp.GetArrayLength();

         // Count vertices in each feature's geometry
         foreach (var feature in featuresProp.EnumerateArray())
         {
            if (feature.TryGetProperty("geometry", out var geom))
            {
               vertexCount += CountVerticesInGeoJsonGeometry(geom);
            }
         }
         return;
      }

      // Pattern 2: Check for precomputed Vertices metadata (e.g., PostalCode API)
      if (TryExtractMetadataVertices(obj, out int metadataVertices))
      {
         vertexCount += metadataVertices;
         featureCount = Math.Max(featureCount, 1); // At least one feature if we found vertices
      }

      // Pattern 3: Object with a Geometry property
      if (obj.TryGetProperty("Geometry", out var geomProp))
      {
         featureCount = Math.Max(featureCount, 1);
         vertexCount += ExtractVerticesFromGeometryValue(geomProp);
         return;
      }

      // Recursively search nested objects and arrays
      foreach (var prop in obj.EnumerateObject())
      {
         AnalyzeJsonElement(prop.Value, ref featureCount, ref vertexCount);
      }
   }

   private static void AnalyzeArray(JsonElement array, ref int featureCount, ref int vertexCount)
   {
      bool hasGeometryProperty = false;

      foreach (var item in array.EnumerateArray())
      {
         if (item.ValueKind == JsonValueKind.Object &&
             item.TryGetProperty("Geometry", out var geomProp))
         {
            hasGeometryProperty = true;
            featureCount++;
            vertexCount += ExtractVerticesFromGeometryValue(geomProp);
         }
         else
         {
            // Recurse for nested structures
            AnalyzeJsonElement(item, ref featureCount, ref vertexCount);
         }
      }

      // If it's an array of objects with Geometry, we're done
      if (hasGeometryProperty)
         return;
   }

   private static bool TryExtractMetadataVertices(JsonElement obj, out int vertices)
   {
      vertices = 0;

      // Look for nested metadata with Vertices field (e.g., PostalCode -> Location -> Geometry -> Vertices)
      foreach (var prop in obj.EnumerateObject())
      {
         if (prop.Value.ValueKind == JsonValueKind.Object)
         {
            if (prop.Value.TryGetProperty("Vertices", out var vertProp) &&
                vertProp.ValueKind == JsonValueKind.Number)
            {
               vertices = vertProp.GetInt32();
               return true;
            }

            // Recurse into nested objects
            if (TryExtractMetadataVertices(prop.Value, out int nested))
            {
               vertices = nested;
               return true;
            }
         }
         else if (prop.Value.ValueKind == JsonValueKind.Array)
         {
            // Check array items for nested metadata
            foreach (var item in prop.Value.EnumerateArray())
            {
               if (item.ValueKind == JsonValueKind.Object &&
                   TryExtractMetadataVertices(item, out int arrayNested))
               {
                  vertices += arrayNested;
               }
            }
            if (vertices > 0)
               return true;
         }
      }

      return false;
   }

   private static int ExtractVerticesFromGeometryValue(JsonElement geomValue)
   {
      if (geomValue.ValueKind == JsonValueKind.String)
      {
         string? geomString = geomValue.GetString();
         if (!string.IsNullOrWhiteSpace(geomString))
         {
            return CountVerticesInGeometryString(geomString);
         }
      }
      else if (geomValue.ValueKind == JsonValueKind.Object)
      {
         // GeoJSON-style geometry object
         return CountVerticesInGeoJsonGeometry(geomValue);
      }

      return 0;
   }

   private static int CountVerticesInGeometryString(string geom)
   {
      // Try WKT POINT
      var pointMatch = WktPointRegex().Match(geom);
      if (pointMatch.Success)
         return 1;

      // Try WKT LINESTRING
      var lineMatch = WktLineStringRegex().Match(geom);
      if (lineMatch.Success)
      {
         return CoordinatePairRegex().Matches(lineMatch.Groups[1].Value).Count;
      }

      // Try WKT POLYGON
      var polyMatch = WktPolygonRegex().Match(geom);
      if (polyMatch.Success)
      {
         return CoordinatePairRegex().Matches(polyMatch.Groups[1].Value).Count;
      }

      // Try raw coordinate string (e.g., "POLYGON((4.5 51.9, 4.6 52.0, ...))")
      if (geom.Contains("POLYGON", StringComparison.OrdinalIgnoreCase))
      {
         return CoordinatePairRegex().Matches(geom).Count;
      }

      return 0;
   }

   private static int CountVerticesInGeoJsonGeometry(JsonElement geom)
   {
      if (!geom.TryGetProperty("coordinates", out var coords))
         return 0;

      if (!geom.TryGetProperty("type", out var typeProp) ||
          typeProp.ValueKind != JsonValueKind.String)
         return 0;

      string? geomType = typeProp.GetString();

      return geomType switch
      {
         "Point" => 1,
         "LineString" => coords.ValueKind == JsonValueKind.Array ? coords.GetArrayLength() : 0,
         "Polygon" => CountPolygonVertices(coords),
         "MultiPoint" => coords.ValueKind == JsonValueKind.Array ? coords.GetArrayLength() : 0,
         "MultiLineString" => CountMultiLineStringVertices(coords),
         "MultiPolygon" => CountMultiPolygonVertices(coords),
         _ => 0
      };
   }

   private static int CountPolygonVertices(JsonElement coords)
   {
      if (coords.ValueKind != JsonValueKind.Array)
         return 0;

      int total = 0;
      foreach (var ring in coords.EnumerateArray())
      {
         if (ring.ValueKind == JsonValueKind.Array)
            total += ring.GetArrayLength();
      }
      return total;
   }

   private static int CountMultiLineStringVertices(JsonElement coords)
   {
      if (coords.ValueKind != JsonValueKind.Array)
         return 0;

      int total = 0;
      foreach (var lineString in coords.EnumerateArray())
      {
         if (lineString.ValueKind == JsonValueKind.Array)
            total += lineString.GetArrayLength();
      }
      return total;
   }

   private static int CountMultiPolygonVertices(JsonElement coords)
   {
      if (coords.ValueKind != JsonValueKind.Array)
         return 0;

      int total = 0;
      foreach (var polygon in coords.EnumerateArray())
      {
         total += CountPolygonVertices(polygon);
      }
      return total;
   }
}