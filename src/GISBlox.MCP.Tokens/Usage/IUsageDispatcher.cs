// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Models;

namespace GISBlox.MCP.Tokens.Usage;

/// <summary>
/// Abstraction for dispatching usage tokens to persistent storage or an external service.
/// </summary>
public interface IUsageDispatcher
{
   /// <summary>
   /// Sends a usage token to the configured storage backend (e.g., HTTP endpoint, queue, database).
   /// Fire-and-forget: failures should be logged but not surfaced to the caller.
   /// </summary>
   /// <param name="token">The usage token to dispatch.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   Task SendAsync(UsageToken token, CancellationToken cancellationToken = default);
}
