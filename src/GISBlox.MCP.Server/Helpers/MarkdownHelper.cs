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

      public static string BuildAudienceAnalysisResponse(string preset, List<AudienceAnalysisResult> results, string? weightsJson)
      {
         StringBuilder sb = new();                 
         AnalysisMode mode = preset.Equals("neutraal", StringComparison.InvariantCultureIgnoreCase) ? AnalysisMode.Analysis : AnalysisMode.Targeting;

         switch (mode)
         {
            case AnalysisMode.Analysis:               

               sb.AppendLine("## 🔍 Analysis Report");
               sb.AppendLine("This report shows the general population of each of the selected postal codes.");
               sb.AppendLine();

               sb.AppendLine("The underlying demographic composition using the neutral weight configuration is as follows:");
               sb.AppendLine();               
               sb.AppendLine("| Postcode | Starters | Young Families | Seniors | Persona |");
               sb.AppendLine("|----------|----------|----------------|---------|---------|");
               sb.AppendLine(BuildAudienceScoresTable(results, true));
               sb.AppendLine();               
               sb.AppendLine("> Neutral scores are relative indicators that compare demographic patterns across groups; they are not percentages and do not sum to 100%.");
               sb.AppendLine();

               sb.AppendLine("### Weight configuration");
               sb.AppendLine("The neutral weight set was used to determine the dominant persona for each postcode.");
               sb.AppendLine("These weights are balanced and do not favor any specific target group.");
               break;

            case AnalysisMode.Targeting:

               sb.AppendLine($"## 🎯 Analysis Report — Targeting mode");
               sb.AppendLine($"This report shows how well the selected postal codes align with the **{preset}** target audience.");
               sb.AppendLine();

               sb.AppendLine("The targeting scores using the specified weight configuration are as follows:");
               sb.AppendLine();               
               sb.AppendLine("| Postcode | Starters | Young Families | Seniors | Persona |");
               sb.AppendLine("|----------|----------|----------------|---------|---------|");
               sb.AppendLine(BuildAudienceScoresTable(results, false));
               sb.AppendLine();
               sb.AppendLine("> The persona is based on the neutral weights, even when targeting scores are shown, to maintain consistency in persona assignment across analyses.");
               sb.AppendLine();

               sb.AppendLine("### Weight configuration");
               sb.AppendLine($"The following overrides were applied to calculate the targeting scores for **{preset}**.");
               sb.AppendLine("Only the values included below override the preset defaults.");
               sb.AppendLine();
               sb.AppendLine(BuildAudienceWeightsTable(weightsJson));
               sb.AppendLine();
               break;

            default:
               break;
         }
         return sb.ToString();
      }

      private static string BuildAudienceScoresTable(List<AudienceAnalysisResult> results, bool isNeutralScoring)
      {  
         StringBuilder sb = new();
         bool toPercentages = !isNeutralScoring;

         foreach (AudienceAnalysisResult result in results)
         {
            Dictionary<string, object> insights = isNeutralScoring ? result.NeutralScores : result.TargetingScores;

            // Get values
            string postcode = result.PostalCode ?? "Unknown";
            string persona = GetStringValue(result.NeutralScoresPersona, "PersonaLabel");
            double starterScore = GetDoubleValue(insights, "StarterScore");
            double youngFamiliesScore = GetDoubleValue(insights, "JongeGezinnenScore");
            double seniorsScore = GetDoubleValue(insights, "SeniorenScore");

            // Format scores
            string starterScoreStr = toPercentages ? $"{Math.Round(starterScore * 100, 1)}%" : Math.Round(starterScore, 2).ToString().Replace(",", ".");
            string youngFamiliesScoreStr = toPercentages ? $"{Math.Round(youngFamiliesScore * 100, 1)}%" : Math.Round(youngFamiliesScore, 2).ToString().Replace(",", ".");
            string seniorsScoreStr = toPercentages ? $"{Math.Round(seniorsScore * 100, 1)}%" : Math.Round(seniorsScore, 2).ToString().Replace(",", ".");
            
            sb.AppendLine($"| {postcode} | {starterScoreStr} | {youngFamiliesScoreStr} | {seniorsScoreStr} | {persona} |");
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
   }
}
