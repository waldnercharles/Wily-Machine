// ReSharper disable once CheckNamespace

namespace Spaghetti;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NodeAttribute : Attribute
{
    public string? NodePath { get; }

    public NodeAttribute(string? nodePath = null)
    {
        NodePath = nodePath;
    }
}
