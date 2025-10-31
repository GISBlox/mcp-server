// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using GISBlox.MCP.Server.Middleware;
using GISBlox.MCP.Server.Models;
using GISBlox.Services.SDK;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning);

#region Get app settings

bool stdioEnabled = builder.Configuration.GetValue<bool?>("Mcp:Stdio:Enabled")
                     ?? string.Equals(Environment.GetEnvironmentVariable("MCP_STDIO_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);
bool httpEnabled = builder.Configuration.GetValue<bool?>("Mcp:Http:Enabled")
                   ?? string.Equals(Environment.GetEnvironmentVariable("MCP_HTTP_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);
int httpPort = builder.Configuration.GetValue<int?>("Mcp:Http:Port")
               ?? (int.TryParse(Environment.GetEnvironmentVariable("MCP_HTTP_PORT"), out var parsed) ? parsed : 8080);

var serviceUrl = builder.Configuration["Gisblox:ServiceUrl"]
                 ?? Environment.GetEnvironmentVariable("GISBLOX_SERVICE_URL")
                 ?? "https://services.gisblox.com";

// Precedence: explicit process env var (e.g. from .mcp.json) > config value > empty string
string? envServiceKey = Environment.GetEnvironmentVariable("GISBLOX_SERVICE_KEY");
string? configServiceKey = builder.Configuration["Gisblox:ServiceKey"];
var baseServiceKey = !string.IsNullOrWhiteSpace(envServiceKey)
                        ? envServiceKey
                        : (!string.IsNullOrWhiteSpace(configServiceKey) ? configServiceKey : string.Empty);

bool runningOnCloud = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
if (runningOnCloud)
{
    stdioEnabled = false;
}

bool forceStdio = string.Equals(Environment.GetEnvironmentVariable("MCP_STDIO_FORCE"), "true", StringComparison.OrdinalIgnoreCase);
if (forceStdio)
{
    stdioEnabled = true;
}

if (httpEnabled && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}");
}

#endregion

var mcp = builder.Services
    .AddMcpServer()
    .WithToolsFromAssembly();

if (stdioEnabled)
{
    mcp.WithStdioServerTransport();

    builder.Services.AddScoped<GISBloxClient>(sp =>
    {        
        return GISBloxClient.CreateClient(serviceUrl, baseServiceKey);
    });
}

if (httpEnabled)
{
    mcp.WithHttpTransport();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<GISBloxClient>(sp =>
    {
        var accessor = sp.GetRequiredService<IHttpContextAccessor>();
        var ctx = accessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context.");
        
        if (string.Equals(ctx.Request.Path, "/health", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ctx.Request.Path, "/", StringComparison.OrdinalIgnoreCase))
        {
            return GISBloxClient.CreateClient(serviceUrl, string.Empty);
        }
    
        if (!ctx.Request.Headers.TryGetValue("Authorization", out var authValues))
            throw new UnauthorizedAccessException("Missing Authorization header.");

        const string bearerPrefix = "Bearer ";
        var auth = authValues.ToString();
        if (!auth.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Authorization header must use Bearer scheme.");

        var key = auth[bearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(key))
            throw new UnauthorizedAccessException("Empty GISBlox service key.");
                
        return GISBloxClient.CreateClient(serviceUrl, key);
    });

    builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
}

var app = builder.Build();

// Validate and log transport settings
app.Logger.LogInformation("[MCP-Startup] serviceUrl={serviceUrl}", serviceUrl);
app.Logger.LogInformation("[MCP-Startup] stdioEnabled={StdioEnabled} httpEnabled={HttpEnabled} runningOnCloud={RunningOnCloud} forceStdio={ForceStdio}",
    stdioEnabled, httpEnabled, runningOnCloud, forceStdio);

if (!stdioEnabled && !httpEnabled)
{
    app.Logger.LogError("[MCP-Startup] No transports enabled (enable MCP stdio or HTTP).");
}

// Configure the HTTP request pipeline.
if (httpEnabled)
{
    app.UseCors();
    app.UseRequestLogging();
    app.MapGet("/", () => Results.Redirect("/health"));
    app.MapGet("/health", () =>
    {
        return Results.Ok(new StatusResponse
        {
            Status = "ok",
            Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            Environment = app.Environment.EnvironmentName,
            MCP = new MCPInfo
            {
                Server = "gisblox",
                Name = "@gisblox/mcp-server",
                Description = "GISBlox MCP Server",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown"
            }
        });
    });

    app.UseMcpJsonRpcNegotiation("/mcp");
    app.MapMcp("/mcp");
}

await app.RunAsync();