﻿// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions; // For name sanitization

namespace GISBlox.MCP.Server.Http;

internal static partial class McpRestEndpointsExtensions
{
    // Mapping from safe tool name -> full method name for invocation resolution.
    private static readonly Dictionary<string, string> _toolNameMap = new(StringComparer.OrdinalIgnoreCase);

    // Optional feature flag to include a secondary json block (off by default for client compatibility)
    private static readonly bool IncludeJsonBlock =
        string.Equals(Environment.GetEnvironmentVariable("GISBLOX_MCP_INCLUDE_JSON_BLOCK"), "true", StringComparison.OrdinalIgnoreCase);

    internal static async Task<object?> JsonRpcEntryAsync(HttpContext ctx)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(ctx.Request.Body, cancellationToken: ctx.RequestAborted);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                var responses = new List<object?>();
                foreach (var item in root.EnumerateArray())
                {
                    responses.Add(await HandleSingleAsync(item, ctx.RequestServices, ctx.RequestAborted));
                }
                return responses;
            }

            return await HandleSingleAsync(root, ctx.RequestServices, ctx.RequestAborted);
        }
        catch (JsonException jex)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return JsonRpcError(null, -32700, "Parse error", jex.Message);
        }
    }

    public static IEndpointRouteBuilder MapMcpJsonRpc(this IEndpointRouteBuilder app, string path = "/mcp")
    {
        var trimmed = TrimEndSlash(path);

        app.MapPost(trimmed, async (HttpContext ctx, IServiceProvider sp) =>
        {
            var result = await JsonRpcEntryAsync(ctx);
            return Results.Json(result, JsonOptions);
        });

        return app;
    }

    private static async Task<object> HandleSingleAsync(JsonElement elem, IServiceProvider sp, CancellationToken ct)
    {
        if (elem.ValueKind != JsonValueKind.Object)
        {
            return JsonRpcError(null, -32600, "Invalid Request", "Payload must be an object or batch array.");
        }

        object? id = ExtractId(elem);

        if (!elem.TryGetProperty("jsonrpc", out var jsonrpcProp) || jsonrpcProp.GetString() != "2.0")
        {
            return JsonRpcError(id, -32600, "Invalid Request", "Missing or invalid jsonrpc version.");
        }

        if (!elem.TryGetProperty("method", out var methodProp) || methodProp.ValueKind != JsonValueKind.String)
        {
            return JsonRpcError(id, -32600, "Invalid Request", "Missing method.");
        }

        var methodName = methodProp.GetString()!;
        bool hasParams = elem.TryGetProperty("params", out JsonElement @params);

        try
        {
            switch (methodName)
            {
                case "initialize":
                {
                    string requestedProtocol = "2025-05-01";
                    if (hasParams && @params.ValueKind == JsonValueKind.Object &&
                        @params.TryGetProperty("protocolVersion", out var pvProp) && pvProp.ValueKind == JsonValueKind.String)
                    {
                        requestedProtocol = pvProp.GetString() ?? requestedProtocol;
                    }

                    var asm = Assembly.GetExecutingAssembly().GetName();
                    var serverInfo = new
                    {
                        name = "@gisblox/mcp-server",
                        version = asm.Version?.ToString() ?? "unknown",
                        description = "GISBlox MCP Server"
                    };

                    var capabilities = new
                    {
                        tools = new { }
                    };

                    var result = new
                    {
                        protocolVersion = requestedProtocol,
                        serverInfo,
                        server = serverInfo, // legacy alias
                        capabilities,
                        instructions = "Connected to GISBlox MCP server. Use tools/list then tools/invoke."
                    };
                    return JsonRpcResult(id, result);
                }
                case "shutdown":
                {
                    return JsonRpcResult(id, new { });
                }
                case "exit":
                {
                    return id is null ? (object)new { } : JsonRpcResult(id, new { });
                }
                case "ping":
                {
                    return JsonRpcResult(id, new { pong = true, timestamp = DateTimeOffset.UtcNow.ToString("o") });
                }
                case "tools/list":
                {
                    var descriptors = ToolCatalog.GetDescriptors();

                    var duplicates = descriptors
                        .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                        .Where(g => g.Count() > 1)
                        .ToDictionary(g => g.Key, _ => true, StringComparer.OrdinalIgnoreCase);

                    var usedSafeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _toolNameMap.Clear();

                    var tools = descriptors.Select(d =>
                    {
                        string baseName = duplicates.ContainsKey(d.Name) ? d.FullName : d.Name;
                        string safe = SanitizeToolName(baseName);
                        if (!usedSafeNames.Add(safe))
                        {
                            int i = 2;
                            while (true)
                            {
                                var candidate = safe;
                                var suffix = "_" + i.ToString();
                                if (candidate.Length + suffix.Length > 64)
                                {
                                    candidate = candidate.Substring(0, Math.Max(1, 64 - suffix.Length));
                                }
                                candidate += suffix;
                                if (usedSafeNames.Add(candidate))
                                {
                                    safe = candidate;
                                    break;
                                }
                                i++;
                            }
                        }

                        _toolNameMap[safe] = d.FullName;
                        _toolNameMap[d.Name] = d.FullName;
                        _toolNameMap[d.FullName] = d.FullName;

                        var props = new Dictionary<string, object>();
                        var required = new List<string>();
                        foreach (var p in d.Parameters)
                        {
                            var schemaType = MapParameterTypeToJsonSchemaType(p.Type);
                            props[p.Name] = new { type = schemaType };
                            if (!p.IsOptional && !p.HasDefaultValue)
                            {
                                required.Add(p.Name);
                            }
                        }

                        var inputSchema = new Dictionary<string, object?>
                        {
                            ["type"] = "object",
                            ["properties"] = props,
                            ["additionalProperties"] = false
                        };
                        if (required.Count > 0)
                        {
                            inputSchema["required"] = required;
                        }

                        return new
                        {
                            name = safe,
                            description = d.Description ?? string.Empty,
                            inputSchema
                        };
                    }).ToArray();

                    return JsonRpcResult(id, new { tools });
                }

                case "tools/invoke":
                case "tool/invoke":
                case "tool/call":
                case "tools/call":
                {
                    if (!hasParams || @params.ValueKind != JsonValueKind.Object)
                        return JsonRpcError(id, -32602, "Invalid params", "Expected object for params.");

                    if (!@params.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                        return JsonRpcError(id, -32602, "Invalid params", "Missing 'name'.");

                    string toolName = nameProp.GetString()!;

                    if (_toolNameMap.TryGetValue(toolName, out var fullName))
                    {
                        toolName = fullName; // use canonical for underlying invocation
                    }

                    Dictionary<string, JsonElement>? arguments = null;

                    if (@params.TryGetProperty("arguments", out var argsProp) && argsProp.ValueKind == JsonValueKind.Object)
                    {
                        arguments = new(StringComparer.OrdinalIgnoreCase);
                        foreach (var p in argsProp.EnumerateObject())
                        {
                            arguments[p.Name] = p.Value;
                        }
                    }

                    var invokeReq = new InvokeRequest
                    {
                        Name = toolName,
                        Arguments = arguments
                    };

                    var invokeResult = await ToolCatalog.InvokeAsync(invokeReq, sp, ct);

                    var formatted = FormatToolResult(invokeResult);
                    return JsonRpcResult(id, new { content = formatted, isError = false });
                }

                default:
                    return JsonRpcError(id, -32601, "Method not found", methodName);
            }
        }
        catch (TargetInvocationException tex) when (tex.InnerException is not null)
        {
            return JsonRpcError(id, -32603, "Internal error", tex.InnerException.Message);
        }
        catch (OperationCanceledException)
        {
            return JsonRpcError(id, -32603, "Internal error", "Request cancelled.");
        }
        catch (InvalidOperationException ioex)
        {
            return JsonRpcError(id, -32602, "Invalid params", ioex.Message);
        }
        catch (Exception ex)
        {
            return JsonRpcError(id, -32603, "Internal error", ex.Message);
        }
    }

    private static object[] FormatToolResult(object? value)
    {
        if (value is null)
            return [new { type = "text", text = "null" }];

        if (value is string s)
            return [new { type = "text", text = s }];

        // Serialize object to JSON text for maximum client compatibility
        var json = JsonSerializer.Serialize(value, JsonOptionsPlain);
        if (!IncludeJsonBlock)
        {
            return [new { type = "text", text = json }];
        }

        // Include both text and json blocks when feature flag is enabled
        return [
            new { type = "text", text = json },
            new { type = "json", data = value }
        ];
    }

    private static string SanitizeToolName(string value)
    {
        var sanitized = Regex.Replace(value, "[^a-zA-Z0-9_-]", "_");
        if (sanitized.Length == 0) sanitized = "tool";
        if (sanitized.Length > 64) sanitized = sanitized.Substring(0, 64);
        return sanitized;
    }

    private static string MapParameterTypeToJsonSchemaType(string type)
    {
        var t = type.TrimEnd('?');
        return t switch
        {
            "string" => "string",
            "int" or "long" => "integer",
            "double" or "float" or "decimal" => "number",
            "bool" or "boolean" => "boolean",
            _ => "string" // fallback
        };
    }

    private static object JsonRpcResult(object? id, object result) => new
    {
        jsonrpc = "2.0",
        id,
        result
    };

    private static object JsonRpcError(object? id, int code, string message, object? data = null) => new
    {
        jsonrpc = "2.0",
        id,
        error = new
        {
            code,
            message,
            data
        }
    };

    private static object? ExtractId(JsonElement elem)
    {
        if (!elem.TryGetProperty("id", out var idProp)) return null;
        return idProp.ValueKind switch
        {
            JsonValueKind.String => idProp.GetString(),
            JsonValueKind.Number => idProp.TryGetInt64(out var l) ? l : idProp.GetDouble(),
            JsonValueKind.Null => null,
            _ => null
        };
    }

    private static string TrimEndSlash(string s) => s.EndsWith('/') ? s[..^1] : s;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions JsonOptionsPlain = new()
    {
        WriteIndented = false
    };

    private sealed class InvokeRequest
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, JsonElement>? Arguments { get; set; }
    }
}