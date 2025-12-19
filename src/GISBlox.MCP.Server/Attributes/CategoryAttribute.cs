// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Attributes;

/// <summary>
/// Specifies the category for a tool type, allowing tools to be grouped for better organization.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CategoryAttribute"/> class.
/// </remarks>
/// <param name="category">The category name for the tool.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CategoryAttribute(string category) : Attribute
{
   /// <summary>
   /// Gets the category name for the tool.
   /// </summary>
   public string Category { get; } = category;
}
