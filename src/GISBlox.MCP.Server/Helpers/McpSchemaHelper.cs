// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Helpers
{
   public static class McpSchemaHelper
   {
      /// <summary>
      /// Creates a standard output schema for structured data representation. Enables a crystal-clear contract for the LLM.
      /// </summary>     
      /// <returns>A dictionary representing the output schema, which includes properties for 'markdown' and 'data'.</returns>
      public static Dictionary<string, object> CreateStandardOutputSchema()
      {
         return new Dictionary<string, object>
         {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
               ["markdown"] = new Dictionary<string, object>
               {
                  ["type"] = "string",
                  ["description"] = "Human-readable Markdown summary. Use this for reasoning and display."
               },
               ["data"] = new Dictionary<string, object>
               {
                  ["type"] = "object",
                  ["description"] = "Raw JSON data. Use this for structured logic."
               }
            },
            ["required"] = new[] { "markdown", "data" }
         };
      }
   }
}
