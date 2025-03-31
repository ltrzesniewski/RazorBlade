using System;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace RazorBlade.Analyzers;

internal class NamespaceVisitor : SyntaxWalker
{
    private string? _lastNamespaceContent;

    public static string? GetNamespaceDirectiveContent(RazorCodeDocument codeDocument)
    {
        var visitor = new NamespaceVisitor();

        if (codeDocument.GetImportSyntaxTrees() is { } importSyntaxTrees)
        {
            foreach (var importSyntaxTree in importSyntaxTrees)
            {
                if (importSyntaxTree != null)
                    visitor.Visit(importSyntaxTree.Root);
            }
        }

        visitor.Visit(codeDocument.GetSyntaxTree().Root);
        return visitor._lastNamespaceContent;
    }

    public override void VisitRazorDirective(RazorDirectiveSyntax node)
    {
        if (node.DirectiveDescriptor == NamespaceDirective.Directive)
        {
            var directiveContent = node.Body?.GetContent();

            if (directiveContent != null && directiveContent.StartsWith(NamespaceDirective.Directive.Directive, StringComparison.Ordinal))
                _lastNamespaceContent = directiveContent.Substring(NamespaceDirective.Directive.Directive.Length).Trim();
        }

        base.VisitRazorDirective(node);
    }
}
