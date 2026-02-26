// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Dynamic;

namespace GISBlox.MCP.Server.Helpers
{
   public static class ToolParameterHelper
   {
      /// <summary>
      /// Extracts parameter names and values, and returns them as a dynamic object containing key-value pairs.
      /// </summary>      
      /// <param name="parameters">The parameters to process.</param>
      /// <returns>A dynamic object representing the parameters as key-value pairs.</returns>
      public static object Extract(object parameters)
      {
         Dictionary<string, object?> dict = [];

         foreach (var prop in parameters.GetType().GetProperties())
         {
            dict[prop.Name] = prop.GetValue(parameters);
         }

         return ToDynamicObject(dict);
      }

      private static object ToDynamicObject(Dictionary<string, object?> dict)
      {
         IDictionary<string, object?> expando = new ExpandoObject();

         foreach (var kvp in dict)
            expando[kvp.Key] = kvp.Value;

         return (ExpandoObject)expando;
      }
   }
}
