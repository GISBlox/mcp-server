// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Attributes;

/// <summary>
/// Provides a description for a method parameter in MCP tool definitions.
/// <remarks>
/// Initializes a new instance of the <see cref="ParamDescAttribute"/> class.
/// </remarks>
/// <param name="description">The description of the parameter.</param>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ParamDescAttribute(string description) : Attribute
{
   /// <summary>
   /// Gets the description of the parameter. 
   /// </summary>
   public string Description { get; } = description;
}
