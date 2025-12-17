// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Insights")]
[GISBlox.MCP.Server.Attributes.Tags("Map Analytics", "Analytics", "KPIs", "Engagement")]
[Description("Provides access to map analytics data using the GISBlox Map Analytics API.")]
internal class MapAnalyticsTools
{
   [McpServerTool(Name = "MapList")]
   [Description("Returns a list of maps that are tracked for a customer.")]
   public static async Task<List<CustomerMap>> ListTrackedMaps(GISBloxClient gisbloxClient, CancellationToken cancellationToken = default)
   {
      var result = await gisbloxClient.MapAnalytics.ListTrackedMaps(cancellationToken);
      return result.Maps;
   }

   [McpServerTool(Name = "AllMapsKpisList")]
   [Description("Gets the KPIs for all maps within a date range of 7, 14, 21 or 31 days.")]
   public static async Task<MapKpiRecord> GetMapsKpis(GISBloxClient gisbloxClient, int dateRange = (int)AnalyticsDateRangeEnum.OneWeek, string? endDate = null, CancellationToken cancellationToken = default)
   {  
      return await gisbloxClient.MapAnalytics.GetMapsKpis((AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);
   }

   [McpServerTool(Name = "MapKpisGet")]
   [Description("Gets the KPIs for a specific map within a date range of 7, 14, 21 or 31 days.")]
   public static async Task<MapKpiRecord> GetMapKpis(GISBloxClient gisbloxClient, string mapId, int dateRange = (int)AnalyticsDateRangeEnum.OneWeek, string? endDate = null, CancellationToken cancellationToken = default)
   {  
      return await gisbloxClient.MapAnalytics.GetMapKpis(mapId, (AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);
   }

   [McpServerTool(Name = "MapEngagementGet")]
   [Description("Gets engagement metrics for a specific map within a date range of 7, 14, 21 or 31 days.")]
   public static async Task<EngagementRecord> GetMapEngagement(GISBloxClient gisbloxClient, string mapId, int dateRange = (int)AnalyticsDateRangeEnum.OneWeek, string? endDate = null, CancellationToken cancellationToken = default)
   {
      return await gisbloxClient.MapAnalytics.GetMapEngagement(mapId, (AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);
   }

   #region Internal helpers

   private static DateTime? ParseDate(string? endDate)
   {
      DateTime? parsedEndDate = null;
      if (!string.IsNullOrEmpty(endDate))
      {
         if (!DateTime.TryParse(endDate, out var temp))
         {
            throw new ArgumentException($"Invalid date format: '{endDate}'. Expected ISO 8601 format (e.g., '2024-01-15').", nameof(endDate));
         }
         parsedEndDate = temp;
      }

      return parsedEndDate;
   }

   #endregion
}
