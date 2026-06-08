// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.Services.SDK.Models;
using System.Text;
using System.Text.Json;

namespace GISBlox.MCP.Server.Helpers
{
   public static class MarkdownHelper
   {
      private enum AnalysisMode { Analysis, Targeting };
      private const string DATA_ATTRIBUTION = "The data used for this analysis is sourced from CBS and Esri Netherlands. For more information, click this [link](https://www.cbs.nl/nl-nl/dossier/nederland-regionaal/geografische-data/gegevens-per-postcode).";

      public static string BuildAudienceAnalysisResponse(string preset, List<AudienceAnalysisResult> results, string? weightsJson)
      {
         StringBuilder sb = new();                 
         AnalysisMode mode = preset.Equals("neutraal", StringComparison.InvariantCultureIgnoreCase) ? AnalysisMode.Analysis : AnalysisMode.Targeting;

         sb.AppendLine("[REPORT_START]\r\n");

         switch (mode)
         {
            case AnalysisMode.Analysis:               

               sb.AppendLine("## 🔍 Analysis Report");
               sb.AppendLine("This report shows the demographic composition of each of the selected postal codes.");
               sb.AppendLine();

               sb.AppendLine("| Postcode | Starters | Young Families | Seniors | Persona |");
               sb.AppendLine("|----------|----------|----------------|---------|---------|");
               sb.AppendLine(BuildNeutralScoresTable(results));
               sb.AppendLine();               
               sb.AppendLine("> Neutral scores are relative indicators that compare demographic patterns across groups; they are not percentages and do not sum to 100%.");
               sb.AppendLine();

               sb.AppendLine("### Weight configuration");
               sb.AppendLine("The neutral weight set was used to determine the dominant persona for each postcode.");
               sb.AppendLine("These weights are balanced and do not favor any specific target group.");
               sb.AppendLine();
               sb.AppendLine($"> {DATA_ATTRIBUTION}");
               sb.AppendLine();
               break;

            case AnalysisMode.Targeting:
               
               sb.AppendLine($"## 🎯 Analysis Report — Targeting mode");
               sb.AppendLine($"This report shows how well the selected postal codes align with the **{preset}** target audience.");
               sb.AppendLine();

               sb.AppendLine(BuildTargetingScoresTable(results, preset));
               sb.AppendLine();

               sb.AppendLine("### Weight configuration");
               sb.AppendLine($"The following overrides were applied to calculate the targeting scores for **{preset}**.");
               sb.AppendLine("Only the values included below override the preset defaults.");
               sb.AppendLine();
               sb.AppendLine(BuildAudienceWeightsTable(weightsJson));
               sb.AppendLine();
               sb.AppendLine($"> {DATA_ATTRIBUTION}");
               sb.AppendLine();
               break;

            default:
               break;
         }

         sb.AppendLine("[REPORT_END]\r\n");
         return sb.ToString();
      }

      private static string BuildNeutralScoresTable(List<AudienceAnalysisResult> results)
      {  
         StringBuilder sb = new();
         foreach (AudienceAnalysisResult result in results)
         {
            Dictionary<string, object> insights = result.NeutralScores;

            // Get values
            string postcode = result.PostalCode ?? "Unknown";
            string persona = GetStringValue(result.NeutralScoresPersona, "PersonaLabel");
            double starterScore = GetDoubleValue(insights, "StarterScore");
            double youngFamiliesScore = GetDoubleValue(insights, "JongeGezinnenScore");
            double seniorsScore = GetDoubleValue(insights, "SeniorenScore");

            // Format scores
            string starterScoreStr = FormatScore(starterScore, false);
            string youngFamiliesScoreStr = FormatScore(youngFamiliesScore, false);
            string seniorsScoreStr = FormatScore(seniorsScore, false);
            
            sb.AppendLine($"| {postcode} | {starterScoreStr} | {youngFamiliesScoreStr} | {seniorsScoreStr} | {persona} |");
         }      
         return sb.ToString();
      }

      private static string BuildTargetingScoresTable(List<AudienceAnalysisResult> results, string preset)
      {
         StringBuilder sb = new();

         // Build header based on preset
         sb.AppendLine($"| Postcode | {preset} | Fit |");
         sb.AppendLine("|----------|----------|-----|");

         // Get values
         foreach (AudienceAnalysisResult result in results)
         {
            Dictionary<string, object> insights = result.TargetingScores;
            
            string postcode = result.PostalCode ?? "Unknown";
            double score = preset.ToLower() switch
            {
               "starters" => GetDoubleValue(insights, "StarterScore"),
               "jongegezinnen" => GetDoubleValue(insights, "JongeGezinnenScore"),
               "senioren" => GetDoubleValue(insights, "SeniorenScore"),
               _ => 0
            };

            string scoreStr = FormatScore(score, true);
            string scoreFit = GetTargetingFitSymbol(score);
            sb.AppendLine($"| {postcode} | {scoreStr} | {scoreFit} |");
         }
         return sb.ToString();
      }

      private static string BuildAudienceWeightsTable(string? weightsJson)
      {
         if (string.IsNullOrEmpty(weightsJson) || weightsJson.Length == 0)
         {
            return "> No custom weights were applied.";
         }

         decimal weight;
         StringBuilder sb = new();         
         Dictionary<string, decimal> appliedWeights = [];

         JsonDocument weights = JsonDocument.Parse(weightsJson);         
  
         if (weights.RootElement.TryGetProperty("Starter", out JsonElement starterElement))
         {
            weight = GetWeightValue(starterElement, "Young");
            if (weight != -1) appliedWeights["Young"] = weight;
            
            weight = GetWeightValue(starterElement, "Meergezins");
            if (weight != -1) appliedWeights["Meergezins"] = weight;
            
            weight = GetWeightValue(starterElement, "Sociaal");
            if (weight != -1) appliedWeights["Sociaal"] = weight;
            
            weight = GetWeightValue(starterElement, "Huishoud");
            if (weight != -1) appliedWeights["Huishoud"] = weight;
            
            weight = GetWeightValue(starterElement, "SeniorPenalty");
            if (weight != -1) appliedWeights["SeniorPenalty"] = weight;
         }
 
         if (weights.RootElement.TryGetProperty("Gezin", out JsonElement gezinElement))
         {
            weight = GetWeightValue(gezinElement, "YoungAdults");
            if (weight != -1) appliedWeights["YoungAdults"] = weight;
            
            weight = GetWeightValue(gezinElement, "Gezinnen");
            if (weight != -1) appliedWeights["Gezinnen"] = weight;
            
            weight = GetWeightValue(gezinElement, "Eengezins");
            if (weight != -1) appliedWeights["Eengezins"] = weight;
            
            weight = GetWeightValue(gezinElement, "Huishoud");
            if (weight != -1) appliedWeights["Huishoud"] = weight;
         }

         if (weights.RootElement.TryGetProperty("Senior", out JsonElement seniorElement))
         {
            weight = GetWeightValue(seniorElement, "65Plus");
            if (weight != -1) appliedWeights["65Plus"] = weight;

            weight = GetWeightValue(seniorElement, "Meergezins");
            if (weight != -1) appliedWeights["Meergezins"] = weight;

            weight = GetWeightValue(seniorElement, "Alleen");
            if (weight != -1) appliedWeights["Alleen"] = weight;

            weight = GetWeightValue(seniorElement, "GezinPenalty");
            if (weight != -1) appliedWeights["GezinPenalty"] = weight;
         }

         sb.AppendLine("| Weight | Value |");
         sb.AppendLine("|--------|-------|");
         
         foreach (var item in appliedWeights)
         {
            sb.AppendLine($"| {item.Key} | {item.Value.ToString().Replace(",", ".")} |");
         }
         return sb.ToString();
      }

      private static decimal GetWeightValue(JsonElement element, string propertyName)
      {
         return element.TryGetProperty(propertyName, out JsonElement propertyElement) ? propertyElement.GetDecimal() : -1;
      }

      private static double GetDoubleValue(Dictionary<string, object>? dict, string key)
      {
         if (dict?.TryGetValue(key, out object? value) != true || value is null)
            return 0;

         return value switch
         {
            JsonElement { ValueKind: JsonValueKind.Number } jsonNumber => jsonNumber.GetDouble(),
            JsonElement { ValueKind: JsonValueKind.String } jsonString when double.TryParse(jsonString.GetString(), out double parsed) => parsed,
            double d => d,
            int i => i,
            decimal m => (double)m,
            float f => f,
            _ => 0
         };
      }

      private static string GetStringValue(Dictionary<string, object>? dict, string key)
      {
         return (dict?.TryGetValue(key, out object? value) == true ? value?.ToString() : null) ?? "Unknown";
      }

      private static string FormatScore(double score, bool toPercentage)
      {
         return toPercentage ? $"{Math.Round(score * 100, 1)}%" : Math.Round(score, 2).ToString().Replace(",", ".");
      }

      public static string GetTargetingFitSymbol(double targetingFit)
      {
         if (double.IsNaN(targetingFit))
            return "·"; 

         if (targetingFit < 0.20)
            return "○"; 
         if (targetingFit < 0.40)
            return "◔"; 
         if (targetingFit < 0.60)
            return "◑"; 
         if (targetingFit < 0.80)
            return "◕"; 

         return "●";
      }
   }
}
