using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace RazorBlade.Analyzers;

internal class ErrorOnTagHelperSyntaxTreePass : RazorEngineFeatureBase, IRazorSyntaxTreePass
{
    public int Order => 150;

    public RazorSyntaxTree Execute(RazorCodeDocument codeDocument, RazorSyntaxTree syntaxTree)
    {
        var visitor = new DirectiveVisitor();
        visitor.Visit(syntaxTree.Root);

        if (visitor.Directives.Count == 0)
            return syntaxTree;

        return RazorSyntaxTree.Create(
            syntaxTree.Root,
            syntaxTree.Source,
            syntaxTree.Diagnostics.Concat(
                visitor.Directives.Select(d => Diagnostics.TagHelpersNotSupported(d.GetSourceSpan(codeDocument.Source)))
            ),
            syntaxTree.Options
        );
    }

    private class DirectiveVisitor : SyntaxWalker
    {
        public List<RazorDirectiveSyntax> Directives { get; } = new();

        public override void VisitRazorDirective(RazorDirectiveSyntax node)
        {
            base.VisitRazorDirective(node);

            // Tag helper directives don't have a DirectiveDescriptor, so we need to check the syntax

            if (node.Body is RazorDirectiveBodySyntax
                {
                    Keyword: RazorMetaCodeSyntax
                    {
                        MetaCode:
                        [
                            { Content: SyntaxConstants.CSharp.AddTagHelperKeyword or SyntaxConstants.CSharp.RemoveTagHelperKeyword or SyntaxConstants.CSharp.TagHelperPrefixKeyword }
                        ]
                    }
                })
            {
                Directives.Add(node);
            }
        }
    }
}
