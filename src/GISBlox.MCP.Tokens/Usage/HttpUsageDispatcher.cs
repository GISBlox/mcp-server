// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Models;
using GISBlox.MCP.Tokens.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace GISBlox.MCP.Tokens.Usage;

/// <summary>
/// HTTP-based usage dispatcher that POSTs usage tokens to a configured ingestion endpoint.
/// Fire-and-forget: failures are logged but do not surface to the caller.
/// </summary>
public class HttpUsageDispatcher(HttpClient httpClient, IOptions<UsageTokenOptions> options, ILogger<HttpUsageDispatcher> logger) : IUsageDispatcher
{
   private readonly HttpClient _httpClient = httpClient;
   private readonly UsageTokenOptions _options = options.Value;
   private readonly ILogger<HttpUsageDispatcher> _logger = logger;

   public async Task SendAsync(UsageToken token, CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrWhiteSpace(_options.IngestUrl))
      {
         _logger.LogWarning("Usage token ingest URL is not configured. Skipping dispatch for token {TokenId}", token.TokenId);
         return;
      }

      try
      {
         var response = await _httpClient.PostAsJsonAsync(_options.IngestUrl, token, cancellationToken);

         if (!response.IsSuccessStatusCode)
         {
            _logger.LogWarning(
                "Failed to dispatch usage token {TokenId} to {IngestUrl}. Status: {StatusCode}",
                token.TokenId,
                _options.IngestUrl,
                response.StatusCode);
         }
         else
         {
            _logger.LogDebug("Successfully dispatched usage token {TokenId} to {IngestUrl}", token.TokenId, _options.IngestUrl);
         }
      }
      catch (Exception ex)
      {
         _logger.LogWarning(
             ex,
             "Exception occurred while dispatching usage token {TokenId} to {IngestUrl}. Usage data may be lost.",
             token.TokenId,
             _options.IngestUrl);
      }
   }
}
