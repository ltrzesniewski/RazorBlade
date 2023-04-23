using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers;

internal sealed class TagHelperTypeVisitor : SymbolVisitor
{
    private readonly INamedTypeSymbol _tagHelperInterface;

    public List<INamedTypeSymbol> Results { get; } = new();

    public TagHelperTypeVisitor(INamedTypeSymbol tagHelperInterface)
    {
        _tagHelperInterface = tagHelperInterface;
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
            Visit(member);
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (IsTagHelper(symbol))
            Results.Add(symbol);
    }

    private bool IsTagHelper(INamedTypeSymbol symbol)
        => symbol is
           {
               TypeKind: TypeKind.Class or TypeKind.Struct,
               DeclaredAccessibility: Accessibility.Public,
               IsAbstract: false,
               IsGenericType: false
           }
           && symbol.AllInterfaces.Contains(_tagHelperInterface);
}
