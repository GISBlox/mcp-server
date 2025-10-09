// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace GISBlox.MCP.Server.Http;

internal static partial class McpRestEndpointsExtensions
{
    private static class ToolCatalog
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static IReadOnlyList<ToolDescriptorDto> _descriptors = [];
        private static List<(Type Type, MethodInfo Method, ToolDescriptorDto Dto)> _methods = [];

        /// <summary>
        /// Returns the list of available tool descriptors (in-memory snapshot),
        /// filtering out infrastructure parameters like gisbloxClient and cancellationToken.
        /// No JSON-RPC envelope is applied here; the caller wraps as needed.
        /// </summary>
        public static IReadOnlyList<ToolDescriptorDto> GetDescriptors()
        {
            EnsureInitialized();

            static bool IsHidden(string? name)
                => name is not null &&
                   (name.Equals("gisbloxClient", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("cancellationToken", StringComparison.OrdinalIgnoreCase));
                        
            var filtered = _descriptors
                .Select(d =>
                {
                    var filteredParams = d.Parameters
                        .Where(p => !IsHidden(p.Name))
                        .ToList()
                        .AsReadOnly();
                    return d with { Parameters = filteredParams };
                })
                .ToList()
                .AsReadOnly();

            return filtered;
        }

        /// <summary>
        /// Run a tool by name with provided arguments. Returns the raw tool result (no envelope).
        /// For Task methods returns Task result (or null for non-generic Task).
        /// </summary>                
        public static async Task<object?> InvokeAsync(InvokeRequest request, IServiceProvider sp, CancellationToken ct)
        {
            EnsureInitialized();

            var match = ResolveMethod(request.Name);
            if (match is null)
                throw new InvalidOperationException($"Tool '{request.Name}' was not found.");

            var (type, method, _) = match.Value;

            // Prepare instance if method is instance-based
            object? target = null;
            if (!method.IsStatic)
            {
                target = ActivatorUtilities.CreateInstance(sp, type);
            }

            var args = BuildArguments(method, sp, request.Arguments, ct);
            var result = method.Invoke(target, args);

            if (result is Task t)
            {
                await t.ConfigureAwait(false);
                var taskType = t.GetType();
                if (taskType.IsGenericType)
                {
                    // Task<T> => get Result
                    return taskType.GetProperty("Result")?.GetValue(t);
                }
                return null; // non-generic Task
            }
            return result; // synchronous return value
        }

        private static (Type, MethodInfo, ToolDescriptorDto)? ResolveMethod(string name)
        {
            // Accept "Method" or "Type.Method" (case-insensitive)
            var cmp = StringComparison.OrdinalIgnoreCase;

            // First try exact FullName
            var m = _methods.FirstOrDefault(m =>
                string.Equals(m.Dto.FullName, name, cmp));

            if (m.Method is not null)
                return (m.Type, m.Method, m.Dto);

            // Then try simple Method name if uniquely resolvable
            var candidates = _methods.Where(m =>
                string.Equals(m.Dto.Name, name, cmp)).ToList();

            return candidates.Count switch
            {
                1 => (candidates[0].Type, candidates[0].Method, candidates[0].Dto),
                0 => null,
                _ => throw new InvalidOperationException($"Ambiguous tool name '{name}'. Use 'Type.Method'.")
            };
        }

        private static object?[] BuildArguments(MethodInfo method, IServiceProvider sp, Dictionary<string, JsonElement>? providedArgs, CancellationToken requestCt)
        {
            var pi = method.GetParameters();
            if (pi.Length == 0)
                return [];

            providedArgs ??= new(StringComparer.OrdinalIgnoreCase);
            var result = new object?[pi.Length];

            for (int i = 0; i < pi.Length; i++)
            {
                var p = pi[i];

                // Cancellation token: use request token
                if (p.ParameterType == typeof(CancellationToken))
                {
                    result[i] = requestCt;
                    continue;
                }

                // Service from DI?
                var service = sp.GetService(p.ParameterType);
                if (service is not null)
                {
                    result[i] = service;
                    continue;
                }

                // Provided argument?
                if (providedArgs.TryGetValue(p.Name!, out var json))
                {
                    result[i] = json.Deserialize(p.ParameterType, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    continue;
                }

                // Optional/default?
                if (p.HasDefaultValue)
                {
                    result[i] = p.DefaultValue;
                    continue;
                }

                // Nullable reference type default?
                if (!p.ParameterType.IsValueType || Nullable.GetUnderlyingType(p.ParameterType) is not null)
                {
                    result[i] = null;
                    continue;
                }

                throw new InvalidOperationException($"Missing required argument '{p.Name}' for tool '{method.DeclaringType?.Name}.{method.Name}'.");
            }            
            return result;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                var asm = Assembly.GetExecutingAssembly();
                var toolTypes = asm.GetTypes()
                    .Where(t => t.IsClass && t.IsDefined(typeof(McpServerToolTypeAttribute), inherit: false))
                    .ToArray();

                var methods = new List<(Type, MethodInfo, ToolDescriptorDto)>();

                foreach (var t in toolTypes)
                {
                    foreach (var mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if (!mi.IsDefined(typeof(McpServerToolAttribute), inherit: false))
                            continue;

                        var desc = mi.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;
                        var name = mi.Name;
                        var fullName = $"{t.Name}.{mi.Name}";

                        var parameters = mi.GetParameters()
                            .Select(p => new ToolParameterDto(
                                Name: p.Name ?? "",
                                Type: ToFriendlyTypeName(p.ParameterType),
                                IsOptional: p.IsOptional || p.HasDefaultValue,
                                HasDefaultValue: p.HasDefaultValue))
                            .ToList()
                            .AsReadOnly();

                        var dto = new ToolDescriptorDto(name, fullName, desc, parameters);
                        methods.Add((t, mi, dto));
                    }
                }

                _methods = methods;
                _descriptors = methods.Select(m => m.Item3).ToList().AsReadOnly();
                _initialized = true;
            }
        }

        private static string ToFriendlyTypeName(Type t)
        {
            if (t == typeof(string)) return "string";
            if (t == typeof(int)) return "int";
            if (t == typeof(long)) return "long";
            if (t == typeof(double)) return "double";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(CancellationToken)) return "CancellationToken";

            var underlying = Nullable.GetUnderlyingType(t);
            if (underlying is not null) return $"{ToFriendlyTypeName(underlying)}?";

            if (t.IsGenericType)
            {
                var name = t.Name;
                var backtick = name.IndexOf('`');
                if (backtick > 0) name = name[..backtick];
                var args = string.Join(", ", t.GetGenericArguments().Select(ToFriendlyTypeName));
                return $"{name}<{args}>";
            }

            return t.Name;
        }
    }

    #region DTOs    

    internal sealed record ToolDescriptorDto(string Name, string FullName, string? Description, IReadOnlyList<ToolParameterDto> Parameters);

    internal sealed record ToolParameterDto(string Name, string Type, bool IsOptional, bool HasDefaultValue);

    #endregion
}