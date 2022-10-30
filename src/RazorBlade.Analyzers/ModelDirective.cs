using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace RazorBlade.Analyzers;

internal static class ModelDirective
{
    private static readonly DirectiveDescriptor _directive = DirectiveDescriptor.CreateDirective(
        "model",
        DirectiveKind.SingleLine,
        builder =>
        {
            builder.AddTypeToken("TypeName", "The model type.");
            builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
            builder.Description = "Specify the model type.";
        });

    public static void Register(RazorProjectEngineBuilder builder)
    {
        builder.AddDirective(_directive);
        builder.Features.Add(new Pass());
    }

    private class Pass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var usages = documentNode.FindDirectiveReferences(_directive);

            foreach (var usage in usages)
            {
                var modelType = (usage.Node as DirectiveIntermediateNode)?.Tokens.FirstOrDefault()?.Content ?? "TModel";
                usage.Node.Diagnostics.Add(Diagnostics.ModelDirectiveNotSupported(usage.Node.Source ?? SourceSpan.Undefined, modelType));
            }
        }
    }
}
