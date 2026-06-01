// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
   [TestClass]
   public class PostalCodeToolsTests
   {
      private GISBloxClient _client = null!;
      private PostalCodeTools _postalCodeTools = null!;

      const int API_QUOTA_DELAY = 1000;  // Allows to run all tests together without exceeding API call quota

      private static T? GetData<T>(McpToolOutput output) where T : class => output.Data as T;

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey, applicationName: "GISBlox.MCP.Server.Tests");
         _postalCodeTools = new PostalCodeTools();
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

         var result = await _postalCodeTools.GetPostalCodeRecord(_client, id, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155029 463048)");
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4RecordCached()
      {
         string id = "3811";

         var result = await _postalCodeTools.GetPostalCodeRecord(_client, id, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode4 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155029 463048)");

         var cachedOutput = await _postalCodeTools.GetPostalCodeRecord(_client, id, cancellationToken: CancellationToken.None);
         PostalCode4Record? recordCached = GetData<PostalCode4Record>(cachedOutput);

         Assert.IsNotNull(recordCached, "Response is empty.");
         Assert.AreEqual(recordCached.MetaData.Query, record.MetaData.Query);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode4Neighbours()
      {
         string id = "3811";
         bool includeSource = false;

         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, 28992, includeWktGeometries, CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, buffer, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, buffer, (int)CoordinateSystem.RDNew, (int)CoordinateSystem.WGS84, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(2, record.PostalCode);

         List<string> expectedIDs = ["1011", "1012"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         string centroid1011 = record.PostalCode.First(pc => pc.Id == "1011").Location.Geometry.Centroid;
         Assert.AreEqual("POINT (4.905333126288753 52.37154228233867)", centroid1011);

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

         var result = await _postalCodeTools.GetPostalCodeByArea(_client, gemeenteId, wijkId, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeByArea(_client, gemeenteId, wijkId, includeWktGeometries: true, cancellationToken: CancellationToken.None);
         PostalCode4Record? record = GetData<PostalCode4Record>(result);

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
         
         var result = await _postalCodeTools.GetKeyFigures(_client, id, CancellationToken.None);
         KerncijferRecord? record = GetData<KerncijferRecord>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(37, record.MetaData.TotalAttributes);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }
      
      [TestMethod]
      public async Task RunAudienceAnalysisNeutral()
      {
         string ids = "1011,1012";
         string preset = "Neutraal";

         var result = await _postalCodeTools.RunAudienceAnalysis(_client, ids, preset, cancellationToken: CancellationToken.None);
         AudienceAnalysisRecord? analysisRecord = GetData<AudienceAnalysisRecord>(result);

         Assert.IsNotNull(analysisRecord, "Response is empty.");
         Assert.HasCount(2, analysisRecord.Results, "Unexpected number of items in the analysis result.");

         AudienceAnalysisResult? result1011 = analysisRecord.Results.Find(result => result.PostalCode == "1011");
         double seniorenScore1011 = GetDictionaryValue(result1011?.TargetingScores, "SeniorenScore");
         Assert.AreEqual(0.43, seniorenScore1011, 0.001, "SeniorenScore insight is not as expected.");

         AudienceAnalysisResult? result1012 = analysisRecord.Results.Find(result => result.PostalCode == "1012");
         double seniorenScore1012 = GetDictionaryValue(result1012?.TargetingScores, "SeniorenScore");
         Assert.AreEqual(0.408, seniorenScore1012, 0.001, "SeniorenScore insight is not as expected.");
      }

      [TestMethod]
      public async Task RunAudienceAnalysisTargetedNoWeights()
      {
         string ids = "1011,1012";
         string preset = "Senioren";

         var result = await _postalCodeTools.RunAudienceAnalysis(_client, ids, preset, cancellationToken:CancellationToken.None);
         AudienceAnalysisRecord? analysisRecord = GetData<AudienceAnalysisRecord>(result);
         
         Assert.IsNotNull(analysisRecord, "Response is empty.");
         Assert.HasCount(2, analysisRecord.Results, "Unexpected number of items in the analysis result.");

         AudienceAnalysisResult? result1011 = analysisRecord.Results.Find(result => result.PostalCode == "1011");
         double seniorenScore1011 = GetDictionaryValue(result1011?.TargetingScores, "SeniorenScore");
         Assert.AreEqual(0.751, seniorenScore1011, 0.001, "SeniorenScore insight is not as expected.");

         AudienceAnalysisResult? result1012 = analysisRecord.Results.Find(result => result.PostalCode == "1012");
         double seniorenScore1012 = GetDictionaryValue(result1012?.TargetingScores, "SeniorenScore");
         Assert.AreEqual(0.696, seniorenScore1012, 0.001, "SeniorenScore insight is not as expected.");
      }

      [TestMethod]
      public async Task RunAudienceAnalysisTargetedWeights()
      {
         string ids = "1011,1012";
         string preset = "Starters";
         string weights = """{"Senior": { "65Plus": 0.4, "Alleen": 0.1 }}""";

         var result = await _postalCodeTools.RunAudienceAnalysis(_client, ids, preset, weights, cancellationToken: CancellationToken.None);
         AudienceAnalysisRecord? analysisRecord = GetData<AudienceAnalysisRecord>(result);

         Assert.IsNotNull(analysisRecord, "Response is empty.");
         Assert.HasCount(2, analysisRecord.Results, "Unexpected number of items in the analysis result.");

         AudienceAnalysisResult? result1011 = analysisRecord.Results.Find(result => result.PostalCode == "1011");
         double starterScore1011 = GetDictionaryValue(result1011?.TargetingScores, "StarterScore");
         Assert.AreEqual(0.597, starterScore1011, 0.001, "StarterScore insight is not as expected.");

         AudienceAnalysisResult? result1012 = analysisRecord.Results.Find(result => result.PostalCode == "1012");
         double starterScore1012 = GetDictionaryValue(result1012?.TargetingScores, "StarterScore");
         Assert.AreEqual(0.688, starterScore1012, 0.001, "StarterScore insight is not as expected.");
      }

      #endregion

      #region PC6

      [TestMethod]
      public async Task GetPostalCode6Record()
      {
         string id = "3811CJ";
         
         var result = await _postalCodeTools.GetPostalCodeRecord(_client, id, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

         Assert.IsNotNull(record, "Response is empty.");

         PostalCode6 pc = record.PostalCode[0];
         Assert.IsTrue(pc.Location.Gemeente == "Amersfoort" && pc.Location.Geometry.Centroid == "POINT (155156 463160)");
         Assert.IsNull(pc.Location.Geometry.WKT);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetPostalCode6Neighbours()
      {
         string id = "3069BS";
         bool includeSource = false;
         
         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeNeighbours(_client, id, includeSource, 28992, includeGeometries, CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, streetLevelPostCodes: true, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, buffer, streetLevelPostCodes: true, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetPostalCodeByGeometry(_client, wkt, buffer, (int)CoordinateSystem.RDNew, (int)CoordinateSystem.WGS84, streetLevelPostCodes: true, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.HasCount(12, record.PostalCode);

         List<string> expectedIDs = ["1011MA", "1011JV", "1011JT", "1011JS", "1011JR", "1011JP", "1011HB", "1011ME", "1011GD", "1012CR", "1012CS", "1012CW"];
         Assert.IsTrue(record.PostalCode.All(pc => expectedIDs.Contains(pc.Id)));

         string centroid1011JV = record.PostalCode.First(pc => pc.Id == "1011JV").Location.Geometry.Centroid;
         Assert.AreEqual("POINT (4.899542319809449 52.37146607902682)", centroid1011JV);

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

         var result = await _postalCodeTools.GetPostalCodeByArea(_client, gemeenteId, wijkId, buurtId, (int)CoordinateSystem.WGS84, streetLevelPostCodes: true, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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

         var result = await _postalCodeTools.GetPostalCodeByArea(_client, gemeenteId, wijkId, buurtId, (int)CoordinateSystem.WGS84, streetLevelPostCodes: true, includeWktGeometries: true, cancellationToken: CancellationToken.None);
         PostalCode6Record? record = GetData<PostalCode6Record>(result);

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
         
         var result = await _postalCodeTools.GetKeyFigures(_client, id, CancellationToken.None);
         KerncijferRecord? record = GetData<KerncijferRecord>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(35, record.MetaData.TotalAttributes);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      #endregion

      #region Helpers

      private static double GetDictionaryValue(Dictionary<string, object>? dict, string key)
      {
         return dict?.TryGetValue(key, out object? value) == true
            ? value switch
            {
               JsonElement { ValueKind: JsonValueKind.Number } jsonNumber => jsonNumber.GetDouble(),
               JsonElement { ValueKind: JsonValueKind.String } jsonString when double.TryParse(jsonString.GetString(), out double parsed) => parsed,
               _ => Convert.ToDouble(value)
            } : 0;
      }

      #endregion
   }
}