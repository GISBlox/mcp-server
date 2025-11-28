// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

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
   public class MapAnalyticsToolsTests
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

      [TestMethod]
      public async Task ListTrackedMaps()
      {
         var maps = await MapAnalyticsTools.ListTrackedMaps(_client, CancellationToken.None);

         Assert.IsNotNull(maps);
      }

      [TestMethod]
      public async Task GetMapsKpis()
      {
         DateTime endDate = DateTime.Parse("2025-11-21");
         AnalyticsDateRangeEnum dateRange = AnalyticsDateRangeEnum.TwoWeeks;

         var kpis = await MapAnalyticsTools.GetMapsKpis(_client, (int)dateRange, endDate.ToString("yyyy-MM-dd"), CancellationToken.None);

         Assert.IsNotNull(kpis);
         
         Assert.AreEqual(dateRange.ToString(), kpis.DateRange);
         Assert.AreEqual(endDate.Add(new TimeSpan(23, 59, 59)), kpis.EndDate);
         Assert.AreEqual(endDate.AddDays(-(int)dateRange + 1), kpis.StartDate);

         Assert.IsTrue(kpis.MapKpis.Any(k => k.Kpis.Count == 4));

         foreach (var map in kpis.MapKpis)
         {
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "Views"), "Views KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "Interactions"), "Interactions KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "ViewDuration"), "ViewDuration KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "ViewDurationAvg"), "ViewDurationAvg KPI missing.");
         }
      }

      [TestMethod]
      public async Task GetMapKpis()
      {
         DateTime endDate = DateTime.Parse("2025-11-21");
         string mapId = "7D6C2135-C878-4945-8622-60D3FE9B4BC3";
         AnalyticsDateRangeEnum dateRange = AnalyticsDateRangeEnum.TwoWeeks;

         var kpis = await MapAnalyticsTools.GetMapKpis(_client, mapId, (int)dateRange, endDate.ToString("yyyy-MM-dd"), CancellationToken.None);

         Assert.IsNotNull(kpis);
         Assert.AreEqual(dateRange.ToString(), kpis.DateRange);
         Assert.AreEqual(endDate.Add(new TimeSpan(23, 59, 59)), kpis.EndDate);
         Assert.AreEqual(endDate.AddDays(-(int)dateRange + 1), kpis.StartDate);

         Assert.IsTrue(kpis.MapKpis.Any(k => k.Kpis.Count == 4));

         foreach (var map in kpis.MapKpis)
         {
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "Views"), "Views KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "Interactions"), "Interactions KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "ViewDuration"), "ViewDuration KPI missing.");
            Assert.IsTrue(map.Kpis.Any(k => k.Name == "ViewDurationAvg"), "ViewDurationAvg KPI missing.");
         }
      }

      [TestMethod]
      public async Task GetMapEngagement()
      {
         DateTime endDate = DateTime.Parse("2025-11-21");
         string mapId = "7D6C2135-C878-4945-8622-60D3FE9B4BC3";
         AnalyticsDateRangeEnum dateRange = AnalyticsDateRangeEnum.ThreeWeeks;

         var record = await MapAnalyticsTools.GetMapEngagement(_client, mapId, (int)dateRange, endDate.ToString("yyyy-MM-dd"), CancellationToken.None);

         Assert.IsNotNull(record, "Response is empty.");

         Assert.AreEqual(mapId, record.MapId);
         Assert.AreEqual(dateRange.ToString(), record.DateRange);
         Assert.AreEqual(endDate.Add(new TimeSpan(23, 59, 59)), record.EndDate);
         Assert.AreEqual(endDate.AddDays(-(int)dateRange + 1), record.StartDate);

         var engagements = record.Engagements;

         Assert.HasCount(21, engagements);
      }
   }
}
