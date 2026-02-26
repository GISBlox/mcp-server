// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Attributes;
using GISBlox.MCP.Server.Helpers;
using GISBlox.MCP.Server.ToolBase;
using GISBlox.Services.SDK;
using GISBlox.Services.SDK.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

[McpServerToolType]
[GISBlox.MCP.Server.Attributes.Category("Spatial Insights")]
[GISBlox.MCP.Server.Attributes.Tags("Map Analytics", "Analytics", "KPIs", "Engagement")]
[Description("Provides access to map analytics data using the GISBlox Map Analytics API.")]
internal class MapAnalyticsTools : McpToolBase
{
   protected override string ToolGroupName => "Spatial Insights";

   [McpServerTool(Name = "MapsList")]
   [Description("Returns a list of maps that are tracked for a customer.")]
   public async Task<McpToolOutput> ListTrackedMaps(
      GISBloxClient gisbloxClient,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      try
      {
         CustomerMapRecord result = await gisbloxClient.MapAnalytics.ListTrackedMaps(cancellationToken);

         string summary = BuildMapResultSummary(result);
         return ProcessResult(toolName, result.Maps, null, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }

   [McpServerTool(Name = "MapsKpisListAll")]
   [Description("Gets the KPIs for all maps within a date range of 7, 14, 21 or 31 days.")]
   public async Task<McpToolOutput> GetMapsKpis(
      GISBloxClient gisbloxClient,
      [ParamDesc("Date range in days: 7 (OneWeek), 14 (TwoWeeks), 21 (ThreeWeeks), or 31 (OneMonth).")]
      int dateRange = (int)AnalyticsDateRangeEnum.OneWeek,
      [ParamDesc("Optional end date in ISO 8601 format (e.g., '2026-01-15'). If not specified, the end date will be set to yesterday.")]
      string? endDate = null,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { dateRange, endDate });
      
      try
      {
         MapKpiRecord result = await gisbloxClient.MapAnalytics.GetMapsKpis((AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);
         
         string summary = BuildKpiResultSummary(result);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }

   [McpServerTool(Name = "MapKpisGet")]
   [Description("Gets the KPIs for a specific map within a date range of 7, 14, 21 or 31 days.")]
   public async Task<McpToolOutput> GetMapKpis(
      GISBloxClient gisbloxClient,
      [ParamDesc("The unique identifier of the map.")]
      string mapId,
      [ParamDesc("Date range in days: 7 (OneWeek), 14 (TwoWeeks), 21 (ThreeWeeks), or 31 (OneMonth).")]
      int dateRange = (int)AnalyticsDateRangeEnum.OneWeek,
      [ParamDesc("Optional end date in ISO 8601 format (e.g., '2024-01-15'). If not specified, the end date will be set to yesterday.")]
      string? endDate = null,
      CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { mapId, dateRange, endDate });
      
      try
      {
         MapKpiRecord result = await gisbloxClient.MapAnalytics.GetMapKpis(mapId, (AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);
         
         string summary = BuildKpiResultSummary(result);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
   }

   [McpServerTool(Name = "MapEngagementGet")]
   [Description("Gets engagement metrics for a specific map within a date range of 7, 14, 21 or 31 days.")]
   public async Task<McpToolOutput> GetMapEngagement(
   GISBloxClient gisbloxClient,
   [ParamDesc("The unique identifier of the map.")]
      string mapId,
   [ParamDesc("Date range in days: 7 (OneWeek), 14 (TwoWeeks), 21 (ThreeWeeks), or 31 (OneMonth).")]
      int dateRange = (int)AnalyticsDateRangeEnum.OneWeek,
   [ParamDesc("Optional end date in ISO 8601 format (e.g., '2024-01-15'). If not specified, the end date will be set to yesterday.")]
      string? endDate = null,
   CancellationToken cancellationToken = default)
   {
      var (toolName, description) = GetCurrentToolMetadata();
      var parameters = ToolParameterHelper.Extract(new { mapId, dateRange, endDate });
      
      try
      {
         EngagementRecord result = await gisbloxClient.MapAnalytics.GetMapEngagement(mapId, (AnalyticsDateRangeEnum)dateRange, ParseDate(endDate), cancellationToken);

         string summary = BuildEngagementSummary(result);
         return ProcessResult(toolName, result, parameters, null, description, summary);
      }
      catch (Exception ex)
      {
         return ProcessError(toolName, ex);
      }
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

   private static string BuildMapResultSummary(CustomerMapRecord customerMapRecord)
   {
      return $"I found **{customerMapRecord?.Maps?.Count ?? 0}** tracked map(s) for this customer.";
   }

   private static string BuildKpiResultSummary(MapKpiRecord kpiRecord)
   {  
      StringBuilder sb = new();
      if (kpiRecord != null && kpiRecord.MapKpis != null)
      {
         sb.AppendLine($"I found the following KPIs for **{kpiRecord.MapKpis.Count}** tracked map(s):");

         MapKpi? kpi = kpiRecord.MapKpis.Take(1).FirstOrDefault();
         kpi?.Kpis.ForEach(k => sb.AppendLine($"- {k.Name}"));
      }
      return sb.ToString();
   }

   private static string BuildEngagementSummary(EngagementRecord engagementRecord)
   {
      StringBuilder sb = new();
      if (engagementRecord != null && engagementRecord.Engagements != null)
      {
         sb.AppendLine($"I found the following engagement metrics for map '{engagementRecord.MapName}':");         
         sb.AppendLine("- Interactions");
         sb.AppendLine("- MarkerClickCount");
         sb.AppendLine("- PanCount");
         sb.AppendLine("- ViewDuration");
         sb.AppendLine("- Views");
         sb.AppendLine("- ZoomCount");
      }      
      return sb.ToString();
   }

   #endregion
}