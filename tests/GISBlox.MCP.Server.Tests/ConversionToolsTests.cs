// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
    [TestClass]
    public class ConversionToolsTests
    {
        private GISBloxClient _client = null!;

        const int API_QUOTA_DELAY = 1000;  // Allows to run all tests together without exceeding API call quota

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

        [TestMethod]
        public async Task ConvertPoint()
        {
            string wkt = "POINT (30 10 5)";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "POINT"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertMultiPoint()
        {
            string wkt ="MULTIPOINT ((10 40), (40 30 2), (20 20), (30 10))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "MULTIPOINT"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertLineString()
        {
            string wkt ="LINESTRING (30 10, 10 30, 40 40)";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "LINESTRING"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertMultiLineString()
        {
            string wkt ="MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "MULTILINESTRING"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertPolygon()
        {
            string wkt ="POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "POLYGON"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertPolygonWithInnerRing()
        {
            string wkt ="POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10),(20 30, 35 35, 30 20, 20 30))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "POLYGON"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertMultiPolygon()
        {
            string wkt ="MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)),((15 5, 40 10, 10 20, 5 10, 15 5)))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "MULTIPOLYGON"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        [TestMethod]
        public async Task ConvertMultiPolygonWithInnerRing()
        {
            string wkt ="MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))";
            string geoJson = await ConvertToGeoJson(wkt, false, CancellationToken.None);

            Assert.IsNotNull(geoJson, "Response is empty.");
            Assert.IsTrue(await IsValidGeoJson(geoJson, "MULTIPOLYGON"), "Invalid GeoJSON.");

            await Task.Delay(API_QUOTA_DELAY);
        }

        #region Private methods

        /// <summary>
        /// Calls the API and returns the results.
        /// </summary>
        /// <param name="wkt">A WKT geometry string.</param>
        /// <param name="asFeatureCollection">Indicates whether to include the GeoJson feature in a feature collection.</param>
        /// <returns>A GeoJson string with the converted WKT geometry.</returns>
        private async Task<string> ConvertToGeoJson(string wkt, bool asFeatureCollection, CancellationToken cancellationToken)
        {
            return await ConversionTools.ConvertToGeoJson(_client, wkt, asFeatureCollection, cancellationToken);
        }

        /// <summary>
        /// Performs a basic GeoJson validity test. It checks whether: 
        /// - The geometry type matches the expected type
        /// - The geometry contains any coordinates
        /// </summary>
        /// <param name="geoJson">A GeoJson string.</param>
        /// <param name="expectedType">The expected geometry type.</param>
        /// <returns>True if valid, False if not.</returns>
        private async static Task<bool> IsValidGeoJson(string geoJson, string expectedType)
        {
            bool isValid = false;
            try
            {
                JsonDocument doc = await Task.Run(() => JsonDocument.Parse(geoJson));
                JsonElement jsonObject = doc.RootElement;
                if (jsonObject.TryGetProperty("geometry", out var typeProperty) && typeProperty.ValueKind == JsonValueKind.Object)
                {
                    if (typeProperty.TryGetProperty("type", out var geomType) && geomType.ValueKind == JsonValueKind.String)
                    {
                        // Type check
                        string? typeName = geomType.GetString();
                        if (typeName != null && typeName.Equals(expectedType, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Any coordinates?
                            if (typeProperty.TryGetProperty("coordinates", out var coordinates) && coordinates.ValueKind == JsonValueKind.Array)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return isValid;
        }

        #endregion
    }
}