// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Text.Json;

namespace GISBlox.MCP.Tokens.Complexity;

/// <summary>
/// Measures complexity metrics from tool output data: byte size and feature count (for GeoJSON).
/// </summary>
public static class ComplexityMeasurer
{
   private static readonly JsonSerializerOptions _jsonOptions = new()
   {
      WriteIndented = false
   };

   /// <summary>
   /// Measures the output byte size and feature count from the tool result data.
   /// </summary>
   /// <param name="data">The tool output data object (typically McpToolOutput.Data).</param>
   /// <param name="outputBytes">The size of the JSON-serialized data in bytes.</param>
   /// <param name="featureCount">The number of features if the data is a GeoJSON FeatureCollection; otherwise 0.</param>
   public static void Measure(object? data, out long outputBytes, out int featureCount)
   {
      outputBytes = 0;
      featureCount = 0;

      if (data == null)
         return;

      // Serialize to measure byte size
      string json = JsonSerializer.Serialize(data, _jsonOptions);
      outputBytes = System.Text.Encoding.UTF8.GetByteCount(json);

      // Attempt to parse as GeoJSON FeatureCollection to count features
      try
      {
         using var doc = JsonDocument.Parse(json);
         var root = doc.RootElement;

         // Check if it's a GeoJSON FeatureCollection
         if (root.ValueKind == JsonValueKind.Object &&
             root.TryGetProperty("type", out var typeProp) &&
             typeProp.ValueKind == JsonValueKind.String &&
             typeProp.GetString() == "FeatureCollection" &&
             root.TryGetProperty("features", out var featuresProp) &&
             featuresProp.ValueKind == JsonValueKind.Array)
         {
            featureCount = featuresProp.GetArrayLength();
         }
      }
      catch
      {
         // Not a valid GeoJSON FeatureCollection, or parsing failed - featureCount remains 0
      }
   }
}
