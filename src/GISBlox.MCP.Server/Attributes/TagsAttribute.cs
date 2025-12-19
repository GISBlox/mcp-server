// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Attributes;

/// <summary>
/// Specifies tags for a tool type, allowing tools to be tagged with keywords for filtering and discovery.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TagsAttribute"/> class.
/// </remarks>
/// <param name="tags">The tags for the tool.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TagsAttribute(params string[] tags) : Attribute
{
   /// <summary>
   /// Gets the tags for the tool.
   /// </summary>
   public IReadOnlyList<string> Tags { get; } = tags ?? [];
}
