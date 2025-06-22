using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using SyntaxWalker = Microsoft.AspNetCore.Razor.Language.Syntax.SyntaxWalker;

namespace RazorBlade.Analyzers.Features;

internal static class RazorBladeDocumentFeature
{
    public static void Register(RazorProjectEngineBuilder builder, InputFile file, GlobalOptions globalOptions)
    {
        var config = GetDefaultDocumentClassifierPassFeature(builder);

        config.ConfigureNamespace.Add((codeDoc, node) =>
        {
            node.Content = NamespaceVisitor.GetNamespaceDirectiveContent(codeDoc)
                           ?? file.HintNamespace
                           ?? "Razor";
        });

        config.ConfigureClass.Add((_, node) =>
        {
            node.ClassName = file.ClassName;
            node.BaseType = "global::RazorBlade.HtmlTemplate";

            node.Modifiers.Clear();
            node.Modifiers.Add(SyntaxFacts.GetText(file.Accessibility ?? globalOptions.DefaultAccessibility ?? Accessibility.Internal));
            node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.PartialKeyword));

            // Enable nullable reference types for the class definition node, as they may be needed for the base class.
            node.Annotations[CommonAnnotations.NullableContext] = CommonAnnotations.NullableContext;
        });

        config.ConfigureMethod.Add((_, node) =>
        {
            node.Modifiers.Clear();
            node.Modifiers.Add(SyntaxFacts.GetText(Accessibility.Protected));
            node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.AsyncKeyword));
            node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.OverrideKeyword));
        });
    }

    /// <summary>
    /// Taken from <see cref="RazorProjectEngineBuilderExtensions"/>
    /// </summary>
    private static DefaultDocumentClassifierPassFeature GetDefaultDocumentClassifierPassFeature(RazorProjectEngineBuilder builder)
    {
        var configurationFeature = builder.Features.OfType<DefaultDocumentClassifierPassFeature>().FirstOrDefault();
        if (configurationFeature == null)
        {
            configurationFeature = new DefaultDocumentClassifierPassFeature();
            builder.Features.Add(configurationFeature);
        }

        return configurationFeature;
    }

    private class NamespaceVisitor : SyntaxWalker
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
}
