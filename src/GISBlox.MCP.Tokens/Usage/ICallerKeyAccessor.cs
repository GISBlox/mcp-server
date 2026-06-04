// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Tokens.Usage;

/// <summary>
/// Abstraction for retrieving the current caller's service key or identity.
/// Implemented by the MCP server to avoid direct HTTP dependencies in the Tokens library.
/// </summary>
public interface ICallerKeyAccessor
{
   /// <summary>
   /// Gets the current caller's service key (e.g., from Authorization header).
   /// Returns null if no caller identity is available (stdio mode, health checks, etc.).
   /// </summary>
   string? GetCallerKey();
}
