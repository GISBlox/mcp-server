// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Diagnostics;

namespace GISBlox.MCP.Server.Middleware
{
    public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                // Suppress logging of this 401 below.
            }
            sw.Stop();

            var status = context.Response.StatusCode;

            // Skip noisy routine / probe / unauthorized hits
            if (status is StatusCodes.Status401Unauthorized or StatusCodes.Status404NotFound)
                return;

            _logger.LogInformation("{method} {path} => {status} in {elapsed}ms",
                context.Request.Method,
                context.Request.Path.Value,
                status,
                sw.Elapsed.TotalMilliseconds.ToString("0.0"));
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
