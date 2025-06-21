using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Components;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace RazorBlade.Analyzers;

internal static class TypeParamDirective
{
    public static DirectiveDescriptor Directive => ComponentConstrainedTypeParamDirective.Directive;

    public static void Register(RazorProjectEngineBuilder builder)
    {
        builder.AddDirective(Directive);
        builder.Features.Add(new Pass());
    }

    /// <summary>
    /// Logic taken from <see cref="ComponentDocumentClassifierPass"/>
    /// </summary>
    private class Pass : IntermediateNodePassBase, IRazorDocumentClassifierPass
    {
        /// <summary>
        /// Same order as in <see cref="DefaultDocumentClassifierPass"/>, but this pass is added later.
        /// </summary>
        public override int Order => DefaultFeatureOrder;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var typeParamReferences = documentNode.FindDirectiveReferences(Directive);
            if (typeParamReferences.Count == 0)
                return;

            var @class = documentNode.FindPrimaryClass();
            if (@class == null)
                return;

            foreach (var typeParamReference in typeParamReferences)
            {
                var typeParamNode = (DirectiveIntermediateNode)typeParamReference.Node;
                if (typeParamNode.HasDiagnostics)
                    continue;

                @class.TypeParameters.Add(new TypeParameter
                {
                    ParameterName = typeParamNode.Tokens.First().Content,
                    Constraints = typeParamNode.Tokens.Skip(1).FirstOrDefault()?.Content
                });
            }
        }
    }
}
