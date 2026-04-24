// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Text;
using System.Text.Json;

namespace GISBlox.MCP.Server.Helpers
{
   public static class MarkdownHelper
   {
      private enum AnalysisMode { Analysis, Targeting };

      public static string BuildAudienceAnalysisResponse(string preset, Dictionary<string, object> insights, Dictionary<string, object> persona, string weightsJson)
      {
         StringBuilder sb = new();

         AnalysisMode mode = preset.Equals("neutraal", StringComparison.InvariantCultureIgnoreCase) ? AnalysisMode.Analysis : AnalysisMode.Targeting;
         switch (mode)
         {
            case AnalysisMode.Analysis:

               sb.AppendLine("## 🔍 Analysis Mode");
               sb.AppendLine("Shows the dominant persona of this postcode group based on neutral demographic scoring.");
               sb.AppendLine();

               string? personaName = persona.TryGetValue("PersonaLabel", out object? value) ? value.ToString() : "Unnamed Persona";
               sb.AppendLine("### Persona");
               sb.AppendLine($"**{personaName}**");
               sb.AppendLine();

               sb.AppendLine("### Neutral scores");
               sb.AppendLine("| Target Group     | Score |");
               sb.AppendLine("|------------------|-------|");
               sb.AppendLine(BuildAudienceScoresTable(insights, false));
               sb.AppendLine();

               sb.AppendLine("> These scores reflect the underlying demographic composition of the selected postcode group(s).");
               sb.AppendLine("> Neutral scores are relative indicators that compare demographic patterns across groups; they are not percentages and do not sum to 100%.");
               sb.AppendLine();

               sb.AppendLine("### Weight configuration (Neutral)");
               sb.AppendLine("The neutral weight set is used to determine the persona.");
               sb.AppendLine("These weights are balanced and do not favor any specific target group.");
               break;

            case AnalysisMode.Targeting:

               sb.AppendLine($"## 🎯 Targeting Mode — {preset}");
               sb.AppendLine($"Shows how well this postcode group aligns with the **{preset}** target audience.");
               sb.AppendLine();

               sb.AppendLine("### Targeting scores");
               sb.AppendLine("| Target Group     | Score |");
               sb.AppendLine("|------------------|-------|");
               sb.AppendLine(BuildAudienceScoresTable(insights, true));
               sb.AppendLine();

               sb.AppendLine("> These scores show how strongly this area matches the selected target audience.");
               sb.AppendLine();

               sb.AppendLine("### Weight configuration (Targeting)");
               sb.AppendLine($"The following overrides were applied to calculate the targeting scores for **{preset}**.");
               sb.AppendLine("Only the values included override the preset defaults.");
               sb.AppendLine();
               sb.AppendLine(BuildAudienceWeightsTable(weightsJson));
               sb.AppendLine();

               break;

            default:
               break;
         }
         return sb.ToString();
      }

      private static string BuildAudienceScoresTable(Dictionary<string, object> insights, bool toPercentages)
      {
         StringBuilder sb = new();
         decimal starterScore = insights.TryGetValue("StarterScore", out object? starterValue) ? Convert.ToDecimal(starterValue) : -1;
         decimal youngFamiliesScore = insights.TryGetValue("JongeGezinnenScore", out object? youngFamiliesValue) ? Convert.ToDecimal(youngFamiliesValue) : -1;
         decimal seniorsScore = insights.TryGetValue("SeniorenScore", out object? seniorsValue) ? Convert.ToDecimal(seniorsValue) : -1;

         string starterScoreStr = toPercentages ? $"{Math.Round(starterScore * 100, 1)}%" : Math.Round(starterScore, 2).ToString().Replace(",", ".");
         string youngFamiliesScoreStr = toPercentages ? $"{Math.Round(youngFamiliesScore * 100, 1)}%" : Math.Round(youngFamiliesScore, 2).ToString().Replace(",", ".");
         string seniorsScoreStr = toPercentages ? $"{Math.Round(seniorsScore * 100, 1)}%" : Math.Round(seniorsScore, 2).ToString().Replace(",", ".");

         sb.AppendLine($"| Starters        | {starterScoreStr} |");
         sb.AppendLine($"| Young Families  | {youngFamiliesScoreStr} |");
         sb.AppendLine($"| Seniors         | {seniorsScoreStr} |");
         return sb.ToString();
      }

      private static string BuildAudienceWeightsTable(string weightsJson)
      {
         if (string.IsNullOrEmpty(weightsJson) || weightsJson.Length == 0)
         {
            return "> No custom weights applied. Using preset defaults.";
         }

         StringBuilder sb = new();
         decimal weight;
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

         sb.AppendLine("| Weight Name     | Value |");
         sb.AppendLine("|-----------------|-------|");
         
         foreach (var item in appliedWeights)
         {
            sb.AppendLine($"| {item.Key}   | {item.Value.ToString().Replace(",", ".")} |");
         }
         return sb.ToString();
      }

      private static decimal GetWeightValue(JsonElement seniorElement, string propertyName)
      {
         return seniorElement.TryGetProperty(propertyName, out JsonElement element) ? element.GetDecimal() : -1;
      }
   }
}
