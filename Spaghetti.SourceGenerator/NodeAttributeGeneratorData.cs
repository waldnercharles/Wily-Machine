using Microsoft.CodeAnalysis;

namespace Spaghetti.SourceGenerator;

internal class NodeAttributeGeneratorData
{
    public string ClassName { get; }

    public string? Path { get; }
    public string PascalName { get; }
    public string SnakeName { get; }
    public string LowerName { get; }
    public string MemberName { get; }
    public string CamelName { get; }

    public string? Type { get; }
    public bool IsNullable { get; }

    private NodeAttributeGeneratorData(ISymbol symbol, string? nodePath)
    {
        ClassName = symbol.ContainingType.GetClassName();

        Path = nodePath;
        MemberName = symbol.Name;
        PascalName = MemberName.ToPascalCase();
        SnakeName = MemberName.ToSnakeCase();
        LowerName = MemberName.ToLowerInvariant();
        CamelName = MemberName.ToCamelCase();
    }

    public NodeAttributeGeneratorData(IPropertySymbol property, string? nodePath) :
        this(property as ISymbol, nodePath)
    {
        Type = property.Type.ToString().Replace("?", "");
        IsNullable = property.Type.ToString().EndsWith("?");
    }

    public NodeAttributeGeneratorData(IFieldSymbol field, string? nodePath) :
        this(field as ISymbol, nodePath)
    {
        Type = field.Type.ToString().Replace("?", "");
        IsNullable = field.Type.ToString().EndsWith("?");
    }
}
