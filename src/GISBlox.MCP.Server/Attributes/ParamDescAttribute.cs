// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Attributes;

/// <summary>
/// Provides a description for a method parameter in MCP tool definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class ParamDescAttribute(string description) : Attribute
{
   public string Description { get; } = description;
}
