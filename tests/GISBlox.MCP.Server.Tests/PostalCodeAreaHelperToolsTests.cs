// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GISBlox.MCP.Server.Tests
{
   [TestClass]
   public class PostalCodeAreaHelperToolsTests
   {
      private GISBloxClient _client = null!;
      private PostalCodeAreaCodeHelperTools _postalCodeAreaCodeHelperTools = null!;

      const int API_QUOTA_DELAY = 1000;  // Allows to run all tests together without exceeding API call quota
      
      private static T? GetData<T>(McpToolOutput output) where T : class => output.Data as T;

      #region Initialization and cleanup

      [TestInitialize]
      public void Init()
      {
         var serviceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
         var serviceUrl = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL") ?? "https://services.gisblox.com";

         _client = GISBloxClient.CreateClient(serviceUrl, serviceKey, applicationName: "GISBlox.MCP.Server.Tests");
         _postalCodeAreaCodeHelperTools = new PostalCodeAreaCodeHelperTools();
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
      public async Task GetGemeente()
      {
         int gemeenteId = 307;
         string gemeenteNaam = "Amersfoort";
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetGemeente(_client, gemeenteNaam, CancellationToken.None);
         GWB? gemeente = GetData<GWB>(result);

         Assert.IsNotNull(gemeente, "Response is empty.");
         Assert.AreEqual(gemeenteId, gemeente.ID);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetGemeenten()
      {
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetGemeenten(_client, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(345, record.MetaData.TotalRecords);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetWijkenByGemeenteId()
      {
         int gemeenteIdAmersfoort = 307;
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetWijkenByGemeenteId(_client, gemeenteIdAmersfoort, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);
         
         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(33, record.MetaData.TotalRecords);         

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetWijkenByGemeenteName()
      {
         string gemeente = "Amersfoort";
         
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetWijkenByGemeenteName(_client, gemeente, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);

         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(33, record.MetaData.TotalRecords);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);         
      }

      [TestMethod]
      public async Task GetBuurtenByWijkId()
      {
         int wijkId = 30701;
         
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetBuurtenByWijkId(_client, wijkId, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);
         
         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(9, record.MetaData.TotalRecords);

         string buurtnaam = "Hof";
         int expectedBuurtIdHof = 3070100;

         var buurt = record.RecordSet.SingleOrDefault(buurt => buurt.Naam == buurtnaam);
         Assert.IsNotNull(buurt, $"Buurt '{buurtnaam}' not found.");
         int buurtIdHof = buurt.ID;
         Assert.AreEqual(expectedBuurtIdHof, buurtIdHof);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetBuurtenByGemeenteAndWijkIds()
      {
         int gemeenteIdAmersfoort = 307;
         int wijkIdStadskern = 30701;
         
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetBuurtenByGemeenteAndWijkIds(_client, gemeenteIdAmersfoort, wijkIdStadskern, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);
        
         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(9, record.MetaData.TotalRecords);

         string buurtnaam = "Hof";
         int expectedBuurtIdHof = 3070100;

         var buurt = record.RecordSet.SingleOrDefault(buurt => buurt.Naam == buurtnaam);
         Assert.IsNotNull(buurt, $"Buurt '{buurtnaam}' not found.");
         int buurtIdHof = buurt.ID;
         Assert.AreEqual(expectedBuurtIdHof, buurtIdHof);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetBuurtenByGemeenteAndWijkNames()
      {
         string gemeente = "Amersfoort";
         string wijk = "Stadskern";
         
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetBuurtenByGemeenteAndWijkNames(_client, gemeente, wijk, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);
         
         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(9, record.MetaData.TotalRecords);

         string buurtnaam = "Stadhuisplein";
         int expectedBuurtIdStadhuisplein = 3070107;

         var buurt = record.RecordSet.SingleOrDefault(buurt => buurt.Naam == buurtnaam);
         Assert.IsNotNull(buurt, $"Buurt '{buurtnaam}' not found.");
         int buurtIdHof = buurt.ID;
         Assert.AreEqual(expectedBuurtIdStadhuisplein, buurtIdHof);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }

      [TestMethod]
      public async Task GetBuurtenByGemeenteAndWijkNamesCached()
      {
         string gemeente = "Amersfoort";
         string wijk = "Stadskern";
         
         McpToolOutput result = await _postalCodeAreaCodeHelperTools.GetBuurtenByGemeenteAndWijkNames(_client, gemeente, wijk, CancellationToken.None);
         GWBRecord? record = GetData<GWBRecord>(result);
         
         Assert.IsNotNull(record, "Response is empty.");
         Assert.AreEqual(9, record.MetaData.TotalRecords);

         string buurtnaam = "Stadhuisplein";
         int expectedBuurtIdStadhuisplein = 3070107;

         var buurt = record.RecordSet.SingleOrDefault(buurt => buurt.Naam == buurtnaam);
         Assert.IsNotNull(buurt, $"Buurt '{buurtnaam}' not found.");
         int buurtIdHof = buurt.ID;
         Assert.AreEqual(expectedBuurtIdStadhuisplein, buurtIdHof);

         McpToolOutput resultCached = await _postalCodeAreaCodeHelperTools.GetBuurtenByGemeenteAndWijkNames(_client, gemeente, wijk, CancellationToken.None);
         Assert.IsNotNull(resultCached, "Response is empty.");
         GWBRecord? recordCached = GetData<GWBRecord>(resultCached);
         Assert.IsNotNull(recordCached, "Response is empty.");
         Assert.AreEqual(9, recordCached.MetaData.TotalRecords);

         await Task.Delay(API_QUOTA_DELAY, CancellationToken.None);
      }
   }
}