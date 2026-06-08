// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Tokens.Options;
using GISBlox.MCP.Tokens.Usage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GISBlox.MCP.Tokens.Extensions;

/// <summary>
/// Extension methods for registering usage token services in the DI container.
/// </summary>
public static class UsageTokenServiceExtensions
{
   /// <summary>
   /// Adds usage token tracking services to the service collection.
   /// Registers IUsageCollector, IUsageDispatcher, and typed HttpClient.
   /// </summary>
   /// <param name="services">The service collection.</param>
   /// <param name="configuration">Application configuration (for binding UsageTokenOptions).</param>
   /// <returns>The service collection for chaining.</returns>
   public static IServiceCollection AddUsageTokens(this IServiceCollection services, IConfiguration configuration)
   {
      // Bind configuration
      services.Configure<UsageTokenOptions>(configuration.GetSection("UsageTokens"));

      // Register usage collector
      services.AddScoped<IUsageCollector, UsageCollector>();

      // Register HTTP dispatcher with typed HttpClient
      services.AddHttpClient<IUsageDispatcher, HttpUsageDispatcher>()
          .ConfigureHttpClient((sp, client) =>
          {
             // Set a reasonable timeout for fire-and-forget POSTs
             client.Timeout = TimeSpan.FromSeconds(10);
          });

      return services;
   }
}
