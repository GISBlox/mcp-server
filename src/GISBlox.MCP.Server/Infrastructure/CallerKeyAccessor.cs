// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Usage;

namespace GISBlox.MCP.Server.Infrastructure;

/// <summary>
/// Retrieves the caller's service key from the current HTTP context (Authorization header).
/// Returns null gracefully when no HTTP context is available (stdio mode, health checks, etc.).
/// </summary>
public class CallerKeyAccessor(IHttpContextAccessor httpContextAccessor) : ICallerKeyAccessor
{
   private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

   public string? GetCallerKey()
   {
      var context = _httpContextAccessor.HttpContext;
      if (context == null)
         return null;

      // Skip health/status endpoints
      if (string.Equals(context.Request.Path, "/health", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(context.Request.Path, "/", StringComparison.OrdinalIgnoreCase))
      {
         return null;
      }

      // Extract Bearer token from Authorization header
      if (context.Request.Headers.TryGetValue("Authorization", out var authValues))
      {
         const string bearerPrefix = "Bearer ";
         var auth = authValues.ToString();

         if (auth.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
         {
            var key = auth[bearerPrefix.Length..].Trim();
            return string.IsNullOrWhiteSpace(key) ? null : key;
         }
      }

      return null;
   }
}
