// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
   [TestClass]
   public class ConversionToolsTests
   {
      private GISBloxClient _client = null!;

      private static readonly byte[] WKB_POINT_30_10_5_BYTES = [1, 233, 3, 0, 0, 0, 0, 0, 0, 0, 0, 62, 64, 0, 0, 0, 0, 0, 0, 36, 64, 0, 0, 0, 0, 0, 0, 20, 64];

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";         

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey);
      }

      [TestCleanup]
      public void Cleanup()
      {
         if (_client is IDisposable d)
         {
            d.Dispose();
         }
      }

      #endregion

      #region WKT -> GeoJson

      [TestMethod]
      public async Task WktToGeoJson_ConvertMultiPolygon()
      {
         string wkt = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)),((15 5, 40 10, 10 20, 5 10, 15 5)))";
         string geoJson = await ConversionTools.ConvertToGeoJson(_client, wkt, true, CancellationToken.None);

         Assert.IsNotNull(geoJson, "Response is empty.");
         Assert.IsTrue(IsValidGeoJson(geoJson), "Invalid GeoJSON.");
      }

      [TestMethod]
      public async Task WktToGeoJson_ConvertMultiPolygonWithInnerRing()
      {
         string wkt = "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))";
         string geoJson = await ConversionTools.ConvertToGeoJson(_client, wkt, false, CancellationToken.None);

         Assert.IsNotNull(geoJson, "Response is empty.");
         Assert.IsTrue(IsValidGeoJson(geoJson), "Invalid GeoJSON.");
      }

      #endregion

      #region GeoJson -> WKT 

      [TestMethod]
      public async Task GeoJsonToWkt_FromString()
      {
         string geoJson = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[30,10,5]},\"properties\":{\"zValue\":23,\"name\":\"Single Point\"}}";

         var wktList = await ConversionTools.ConvertToWkt(_client, geoJson, CancellationToken.None);

         Assert.IsNotNull(wktList, "Returned WKT list is null.");
         Assert.IsGreaterThan(0, wktList.Count, "Returned WKT list is empty.");
         Assert.IsFalse(string.IsNullOrWhiteSpace(wktList[0].Geometry), "WKT geometry is empty.");

         var wkt = wktList.FirstOrDefault();
         Assert.IsNotNull(wkt, "Returned WKT object is null.");
         Assert.AreEqual("POINT Z (30 10 5)", wkt.Geometry);

         Assert.IsNotNull(wkt.Properties, "WKT properties are null.");
         Assert.IsGreaterThan(0, wkt.Properties.Count, "WKT properties are empty.");

         Assert.AreEqual(23L, wkt.Properties[0]["zValue"]);
         Assert.AreEqual("Single Point", wkt.Properties[0]["name"]);
      }

      [TestMethod]
      public async Task GeoJsonToWkt_FromFile()
      {
         string geoJson = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[30,10,5]},\"properties\":{\"zValue\":23,\"name\":\"Single Point\"}}";

         string localPath = Path.Combine(Path.GetTempPath(), "test-wkt.json");
         try
         {
            await File.WriteAllTextAsync(localPath, geoJson, CancellationToken.None);

            var wktList = await ConversionTools.ConvertToWktFromFile(_client, localPath, CancellationToken.None);

            Assert.IsNotNull(wktList, "Returned WKT list is null.");
            Assert.IsGreaterThan(0, wktList.Count, "Returned WKT list is empty.");

            var wkt = wktList.FirstOrDefault();
            Assert.IsNotNull(wkt, "Returned WKT object is null.");
            Assert.AreEqual("POINT Z (30 10 5)", wkt.Geometry);

            Assert.IsNotNull(wkt.Properties, "WKT properties are null.");
            Assert.IsGreaterThan(0, wkt.Properties.Count, "WKT properties are empty.");

            Assert.AreEqual(23L, wkt.Properties[0]["zValue"]);
            Assert.AreEqual("Single Point", wkt.Properties[0]["name"]);
         }
         finally
         {
            if (File.Exists(localPath))
            {
               File.Delete(localPath);
            }
         }
      }

      #endregion

      #region GeoJson -> WKB

      [TestMethod]
      public async Task GeoJsonToWkb_FromString()
      {
         string geoJson = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[30,10,5]},\"properties\":{\"zValue\":23,\"name\":\"Single Point\"}}";

         var wkbList = await ConversionTools.ConvertToWkb(_client, geoJson, CancellationToken.None);

         Assert.IsNotNull(wkbList, "Returned WKB list is null.");
         Assert.IsGreaterThan(0, wkbList.Count, "Returned WKB list is empty.");

         var wkb = wkbList.FirstOrDefault();
         Assert.IsNotNull(wkb, "Returned WKB object is null.");
         Assert.IsNotNull(wkb.Geometry, "WKB geometry is null.");
         Assert.IsGreaterThan(0, wkb.Geometry.Length, "WKB geometry is empty.");
         CollectionAssert.AreEqual(WKB_POINT_30_10_5_BYTES, wkb.Geometry, "WKB geometry does not match expected value.");

         Assert.IsNotNull(wkb.Properties, "WKB properties are null.");
         Assert.IsGreaterThan(0, wkb.Properties.Count, "WKB properties are empty.");
         Assert.AreEqual(23L, wkb.Properties[0]["zValue"]);
         Assert.AreEqual("Single Point", wkb.Properties[0]["name"]);
      }

      [TestMethod]
      public async Task GeoJsonToWkb_FromFile()
      {
         string geoJson = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[30,10,5]},\"properties\":{\"zValue\":23,\"name\":\"Single Point\"}}";

         string localPath = Path.Combine(Path.GetTempPath(), "test-wkb.json");
         try
         {
            await File.WriteAllTextAsync(localPath, geoJson, CancellationToken.None);

            var wkbList = await ConversionTools.ConvertToWkbFromFile(_client, localPath, CancellationToken.None);

            Assert.IsNotNull(wkbList, "Returned WKB list is null.");
            Assert.IsGreaterThan(0, wkbList.Count, "Returned WKB list is empty.");

            var wkb = wkbList.FirstOrDefault();
            Assert.IsNotNull(wkb, "Returned WKB object is null.");
            Assert.IsNotNull(wkb.Geometry, "WKB geometry is null.");
            Assert.IsGreaterThan(0, wkb.Geometry.Length, "WKB geometry is empty.");

            CollectionAssert.AreEqual(WKB_POINT_30_10_5_BYTES, wkb.Geometry, "WKB geometry does not match expected value.");

            Assert.IsNotNull(wkb.Properties, "WKB properties are null.");
            Assert.IsGreaterThan(0, wkb.Properties.Count, "WKB properties are empty.");

            Assert.AreEqual(23L, wkb.Properties[0]["zValue"]);
            Assert.AreEqual("Single Point", wkb.Properties[0]["name"]);
         }
         finally
         {
            if (File.Exists(localPath))
            {
               File.Delete(localPath);
            }
         }
      }

      #endregion

      #region Private methods

      public static bool IsValidGeoJson(string geoJson)
      {
         bool isValid = false;
         try
         {
            JsonDocument doc = JsonDocument.Parse(geoJson);
            JsonElement jsonObject = doc.RootElement;

            // Check if root element is an object
            if (jsonObject.ValueKind == JsonValueKind.Object)
            {
               // Check for FeatureCollection
               if (jsonObject.TryGetProperty("type", out var typeProp) &&
                   typeProp.ValueKind == JsonValueKind.String &&
                   string.Equals(typeProp.GetString(), "FeatureCollection", StringComparison.InvariantCultureIgnoreCase))
               {
                  if (jsonObject.TryGetProperty("features", out var featuresProp) &&
                      featuresProp.ValueKind == JsonValueKind.Array)
                  {
                     foreach (var feature in featuresProp.EnumerateArray())
                     {
                        // Validate each feature
                        if (feature.ValueKind == JsonValueKind.Object &&
                            feature.TryGetProperty("geometry", out var geometryProp) &&
                            geometryProp.ValueKind == JsonValueKind.Object &&
                            geometryProp.TryGetProperty("type", out var geomType) &&
                            geomType.ValueKind == JsonValueKind.String &&
                            ExpectedTypes().Contains(geomType.GetString(), StringComparer.InvariantCultureIgnoreCase) &&
                            geometryProp.TryGetProperty("coordinates", out var coordinates) &&
                            coordinates.ValueKind == JsonValueKind.Array)
                        {
                           isValid = true;
                           break; // At least one valid feature is enough
                        }
                     }
                  }
               }
               // Check for single feature
               else if (jsonObject.TryGetProperty("geometry", out var typeProperty) && typeProperty.ValueKind == JsonValueKind.Object)
               {
                  if (typeProperty.TryGetProperty("type", out var geomType) && geomType.ValueKind == JsonValueKind.String)
                  {
                     string? typeName = geomType.GetString();
                     if (ExpectedTypes().Contains(typeName, StringComparer.InvariantCultureIgnoreCase))
                     {
                        if (typeProperty.TryGetProperty("coordinates", out var coordinates) && coordinates.ValueKind == JsonValueKind.Array)
                        {
                           isValid = true;
                        }
                     }
                  }
               }
            }
         }
         catch
         {
            isValid = false;
         }
         return isValid;
      }

      private static List<string> ExpectedTypes()
      {
         return
         [
            "Point",
            "MultiPoint",
            "LineString",
            "MultiLineString",
            "Polygon",
            "MultiPolygon",
            "GeometryCollection"
         ];
      }

      #endregion
   }
}