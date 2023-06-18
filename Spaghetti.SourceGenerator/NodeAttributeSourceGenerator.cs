using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Spaghetti.SourceGenerator;

[Generator]
internal class NodeAttributeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AllClassesSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = context.SyntaxReceiver as AllClassesSyntaxReceiver ?? throw new Exception();

        var classSymbols = receiver.AllClasses
            .Select(c =>
            {
                var symbol = context.Compilation.GetSemanticModel(c.SyntaxTree).GetDeclaredSymbol(c);

                if (symbol == null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "Spaghetti.SourceGenerator.0001",
                                "Inspection",
                                $"Unable to find declared symbol for {c}. Skipping.",
                                "Spaghetti.SourceGenerator.Parsing",
                                DiagnosticSeverity.Warning,
                                true),
                            c.GetLocation()));
                }

                return symbol;
            })
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<INamedTypeSymbol>();

        foreach (var symbol in classSymbols)
        {
            if (symbol != null)
            {
                var autoAttributeGeneratorData = new List<NodeAttributeGeneratorData>();

                foreach (var memberAttribute in GetAllNodeAttributes(symbol))
                {
                    if (memberAttribute.Symbol is IPropertySymbol prop)
                    {
                        autoAttributeGeneratorData.Add(
                            new NodeAttributeGeneratorData(prop, memberAttribute.Attribute.NodePath));
                    }
                    else if (memberAttribute.Symbol is IFieldSymbol field)
                    {
                        autoAttributeGeneratorData.Add(
                            new NodeAttributeGeneratorData(field, memberAttribute.Attribute.NodePath));
                    }
                }

                if (autoAttributeGeneratorData.Count > 0)
                {
                    var @namespace = symbol.NamespaceOrNull();
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("using Godot;");
                    stringBuilder.AppendLine("using System;");
                    stringBuilder.AppendLine("using Spaghetti;");
                    stringBuilder.AppendLine();

                    if (@namespace != null)
                    {
                        stringBuilder.AppendLine($"namespace {@namespace};");
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine($"partial class {symbol.GetClassName()}");
                    stringBuilder.AppendLine("{");
                    stringBuilder.AppendLine("    void RegisterNodes()");
                    stringBuilder.AppendLine("    {");

                    foreach (var node in autoAttributeGeneratorData)
                    {
                        stringBuilder.AppendLine(
                            $"        {node.MemberName} = GetNodeOrNull<{node.Type}>(\"{node.Path ?? node.PascalName}\") ?? GetNodeOrNull<{node.Type}>(\"%{node.PascalName}\") ?? GetNodeOrNull<{node.Type}>(\"{node.SnakeName}\") ?? GetNodeOrNull<{node.Type}>(\"%{node.SnakeName}\") ?? GetNodeOrNull<{node.Type}>(\"{node.CamelName}\") ?? GetNodeOrNull<{node.Type}>(\"%{node.CamelName}\") ?? GetNodeOrNull<{node.Type}>(\"{node.LowerName}\") ?? GetNodeOrNull<{node.Type}>(\"%{node.LowerName}\");");

                        if (!node.IsNullable)
                        {
                            stringBuilder.AppendLine(
                                $"        Log.Assert({node.MemberName} != null, \"Node \\\"{node.MemberName}\\\" could not be found.\");");
                        }
                    }

                    stringBuilder.AppendLine("    }");
                    stringBuilder.AppendLine("}");

                    context.AddSource(GenerateFilename(symbol), stringBuilder.ToString());
                }
            }
        }
    }

    private List<(ISymbol Symbol, NodeAttribute Attribute)> GetAllNodeAttributes(ITypeSymbol symbol,
        bool excludePrivate = false)
    {
        var result = new List<(ISymbol, NodeAttribute)>();

        if (symbol.BaseType != null)
        {
            result.AddRange(GetAllNodeAttributes(symbol.BaseType, true));
        }

        var members = symbol.GetMembers()
            .Where(x => !excludePrivate || x.DeclaredAccessibility != Accessibility.Private)
            .Select(member => (member,
                member.GetAttributes().FirstOrDefault(x => x?.AttributeClass?.Name == nameof(NodeAttribute))))
            .Where(x => x.Item2 != null)
            .Select(x => (x.Item1, new NodeAttribute((string?)x.Item2?.ConstructorArguments[0].Value)));

        result.AddRange(members);
        return result;
    }

    private string GenerateFilename(ISymbol symbol)
    {
        return $"{string.Join("_", $"{symbol}".Split(Path.GetInvalidFileNameChars()))}.g.cs";
    }

    private class AllClassesSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> AllClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                AllClasses.Add(cds);
            }
        }
    }
}
