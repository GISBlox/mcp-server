// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
   [TestClass]
   public class PostalCodeToolsTests
   {
      private GISBloxClient _client = null!;

      const int API_QUOTA_DELAY = 1000;  // Allows to run all tests together without exceeding API call quota

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey, applicationName: "GISBlox.MCP.Server.Tests");
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

      #region PC4

      [TestMethod]
      public async Task GetPostalCode4Record()
      {
         string id = "3811";
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4Record(_client, id, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155029.15793771204 463047.87594218826)");
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4RecordCached()
      {
         string id = "3811";
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4Record(_client, id, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155029.15793771204 463047.87594218826)");

         PostalCode4Record recordCached = await PostalCodeTools.GetPostalCode4Record(_client, id, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(recordCached, "Response is empty.");
         Assert.AreEqual(recordCached.MetaData.Query, record.MetaData.Query);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4Neighbours()
      {
         string id = "3811";
         bool includeSource = false;
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4Neighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(6, record.PostalCode);

         List<string> expectedIDs = ["3817", "3814", "3816", "3813", "3812", "3818"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4NeighboursWithSource()
      {
         string id = "3811";
         bool includeSource = true;
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4Neighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(7, record.PostalCode);

         List<string> expectedIDs = ["3811", "3817", "3814", "3816", "3813", "3812", "3818"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.IsTrue(record.PostalCode.All(pc => pc.Location.Geometry.WKT == null));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4NeighboursWithSourceAndWktGeometries()
      {
         string id = "3811";
         bool includeSource = true;
         bool includeWktGeometries = true;
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4Neighbours(_client, id, includeSource, 28992, includeWktGeometries, CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(7, record.PostalCode);

         List<string> expectedIDs = ["3811", "3817", "3814", "3816", "3813", "3812", "3818"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.IsTrue(record.PostalCode.All(pc => pc.Location.Geometry.WKT != null));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }


      [TestMethod]
      public async Task GetPostalCode4ByGeometry()
      {
         string wkt = "LINESTRING(109935 561725, 110341 564040, 111430 565908)";
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4ByGeometry(_client, wkt, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(3, record.PostalCode);

         List<string> expectedIDs = ["1791", "1796", "1797"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4ByGeometryWithBuffer()
      {
         string wkt = "LINESTRING(109935 561725, 110341 564040, 111430 565908)";
         int buffer = 5000;    // meters, since CS of WKT is 28992.
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4ByGeometry(_client, wkt, buffer, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(5, record.PostalCode);

         List<string> expectedIDs = ["1791", "1793", "1795", "1796", "1797"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4ByGeometryWithBufferAndDifferentTargetEpsg()
      {
         string wkt = "POINT(121843 487293)";
         int buffer = 200;   // meters, since CS of WKT is 28992.
         PostalCode4Record record = await PostalCodeTools.GetPostalCode4ByGeometry(_client, wkt, buffer, (int)CoordinateSystem.RDNew, (int)CoordinateSystem.WGS84, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(2, record.PostalCode);

         List<string> expectedIDs = ["1011", "1012"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.AreEqual("POINT (4.905333126288753 52.37154228233867)", record.PostalCode[1].Location.Geometry.Centroid);

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4ByArea()
      {
         int gemeenteId = 513;
         string expectedGemeente = "Gouda";

         int wijkId = 51309;
         string expectedWijk = "Westergouwe";

         string expectedPostalCode = "2809";

         PostalCode4Record record = await PostalCodeTools.GetPostalCode4ByArea(_client, gemeenteId, wijkId, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.AreEqual(expectedPostalCode, pc.Id);
         Assert.AreEqual(expectedGemeente, pc.Location.Gemeente);
         Assert.AreEqual(expectedWijk, pc.Location.Wijken);
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4ByAreaIncludeWktGeometries()
      {
         int gemeenteId = 513;
         string expectedGemeente = "Gouda";

         int wijkId = 51309;
         string expectedWijk = "Westergouwe";

         string expectedPostalCode = "2809";

         PostalCode4Record record = await PostalCodeTools.GetPostalCode4ByArea(_client, gemeenteId, wijkId, includeWktGeometries: true, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.AreEqual(expectedPostalCode, pc.Id);
         Assert.AreEqual(expectedGemeente, pc.Location.Gemeente);
         Assert.AreEqual(expectedWijk, pc.Location.Wijken);
         Assert.IsNotNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetKeyFigures4()
      {
         string id = "3811";
         KerncijferRecord record = await PostalCodeTools.GetKeyFigures(_client, id, CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(37, record.MetaData.TotalAttributes);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      #endregion

      #region PC6

      [TestMethod]
      public async Task GetPostalCode6Record()
      {
         string id = "3811CJ";
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6Record(_client, id, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode6 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155155.51254284632 463159.828901163)");
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6Neighbours()
      {
         string id = "3069BS";
         bool includeSource = false;
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6Neighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(7, record.PostalCode);

         List<string> expectedIDs = ["3069BK", "3069BL", "3069BN", "3069BP", "3069BR", "3069BM", "3069BT"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6NeighboursWithSource()
      {
         string id = "3069BS";
         bool includeSource = true;
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6Neighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(8, record.PostalCode);

         List<string> expectedIDs = ["3069BS", "3069BK", "3069BL", "3069BN", "3069BP", "3069BR", "3069BM", "3069BT"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.IsTrue(record.PostalCode.All(pc => pc.Location.Geometry.WKT == null));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6NeighboursWithSourceAndWktGeometries()
      {
         string id = "3069BS";
         bool includeSource = true;
         bool includeGeometries = true;
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6Neighbours(_client, id, includeSource, 28992, includeGeometries, CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(8, record.PostalCode);

         List<string> expectedIDs = ["3069BS", "3069BK", "3069BL", "3069BN", "3069BP", "3069BR", "3069BM", "3069BT"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.IsTrue(record.PostalCode.All(pc => pc.Location.Geometry.WKT != null));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6ByGeometry()
      {
         string wkt = "LINESTRING(109935 561725, 110341 564040, 111430 565908)";
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6ByGeometry(_client, wkt, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(3, record.PostalCode);

         List<string> expectedIDs = ["1791PB", "1796AZ", "1797RT"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6ByGeometryWithBuffer()
      {
         string wkt = "LINESTRING(109935 561725, 110341 564040, 111430 565908)";
         int buffer = 750;    // meters, since CS of WKT is 28992.
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6ByGeometry(_client, wkt, buffer, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(6, record.PostalCode);

         List<string> expectedIDs = ["1791PB", "1796AZ", "1797RT", "1791NT", "1796MV", "1791PE"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6ByGeometryWithBufferAndDifferentTargetEpsg()
      {
         string wkt = "POINT(121843 487293)";
         int buffer = 50;   // meters, since CS of WKT is 28992.
         PostalCode6Record record = await PostalCodeTools.GetPostalCode6ByGeometry(_client, wkt, buffer, (int)CoordinateSystem.RDNew, (int)CoordinateSystem.WGS84, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(12, record.PostalCode);

         List<string> expectedIDs = ["1011MA", "1011JV", "1011JT", "1011JS", "1011JR", "1011JP", "1011HB", "1011ME", "1011GD", "1012CR", "1012CS", "1012CW"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));
         Assert.AreEqual("POINT (4.899542319809449 52.37146607902682)", record.PostalCode[1].Location.Geometry.Centroid);

         await Task.Delay(API_QUOTA_DELAY * 2, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6ByArea()
      {
         int gemeenteId = 513;
         string expectedGemeente = "Gouda";

         int wijkId = 51309;
         string expectedWijk = "Westergouwe";

         int buurtId = 5130904;
         string expectedBuurt = "Tuinenbuurt";

         string expectedPostalCode = "2809RA";

         PostalCode6Record record = await PostalCodeTools.GetPostalCode6ByArea(_client, gemeenteId, wijkId, buurtId, (int)CoordinateSystem.WGS84, cancellationToken: CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode6 pc = record.PostalCode[0];
         Assert.AreEqual(expectedPostalCode, pc.Id);
         Assert.AreEqual(expectedGemeente, pc.Location.Gemeente);
         Assert.AreEqual(expectedWijk, pc.Location.Wijk);
         Assert.AreEqual(expectedBuurt, pc.Location.Buurt);
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6ByAreaIncludeWktGeometries()
      {
         int gemeenteId = 513;
         string expectedGemeente = "Gouda";

         int wijkId = 51309;
         string expectedWijk = "Westergouwe";

         int buurtId = 5130904;
         string expectedBuurt = "Tuinenbuurt";

         string expectedPostalCode = "2809RA";

         PostalCode6Record record = await PostalCodeTools.GetPostalCode6ByArea(_client, gemeenteId, wijkId, buurtId, (int)CoordinateSystem.WGS84, true, CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode6 pc = record.PostalCode[0];
         Assert.AreEqual(expectedPostalCode, pc.Id);
         Assert.AreEqual(expectedGemeente, pc.Location.Gemeente);
         Assert.AreEqual(expectedWijk, pc.Location.Wijk);
         Assert.AreEqual(expectedBuurt, pc.Location.Buurt);
         Assert.IsNotNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetKeyFigures6()
      {
         string id = "3811BB";
         KerncijferRecord record = await PostalCodeTools.GetKeyFigures(_client, id, CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(35, record.MetaData.TotalAttributes);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      #endregion
   }
}