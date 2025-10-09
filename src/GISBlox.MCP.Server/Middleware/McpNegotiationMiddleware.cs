using GISBlox.MCP.Server.Http;
using System.Text;
using System.Text.Json;

namespace GISBlox.MCP.Server.Middleware;

/// <summary>
/// Middleware that inspects an incoming POST /mcp request and routes it to either
/// JSON-RPC handling (plain JSON body) or leaves it for the streaming MCP transport based on
/// Content-Type / Accept headers. If the request has Content-Type 'application/json' and the body
/// starts with '{' or '[' we treat it as JSON-RPC. Otherwise we let the pipeline continue.
/// </summary>
internal class McpNegotiationMiddleware(RequestDelegate next, string path)
{
    private readonly RequestDelegate _next = next;
    private readonly PathString _path = new(path);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.Path.Equals(_path))
        {
            await _next(context);
            return;
        }

        // If content-type explicitly application/json OR accept prefers JSON-RPC result and body looks like JSON -> JSON-RPC
        var contentType = context.Request.ContentType ?? string.Empty;
        var accept = context.Request.Headers.Accept.ToString();

        if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
            accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            // Peek body minimally
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            char[] preview = new char[1];
            int read = await reader.ReadAsync(preview, 0, 1);
            context.Request.Body.Position = 0;

            if (read == 1 && (preview[0] == '{' || preview[0] == '['))
            {
                // Handle JSON-RPC directly and short-circuit
                var jsonRpcResult = await McpRestEndpointsExtensions.JsonRpcEntryAsync(context);
                // Response already written if result is IActionResult; if it's a serializable object write JSON.
                if (jsonRpcResult is not IResult r)
                {
                    context.Response.ContentType = "application/json";
                    await JsonSerializer.SerializeAsync(context.Response.Body, jsonRpcResult, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    }, context.RequestAborted);
                }
                return;
            }
        }

        // Fallback to streaming transport
        await _next(context);
    }
}

internal static class McpNegotiationMiddlewareExtensions
{
    public static IApplicationBuilder UseMcpJsonRpcNegotiation(this IApplicationBuilder app, string path = "/mcp")
    {
        return app.UseMiddleware<McpNegotiationMiddleware>(path);
    }
}
