using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers;

[Generator]
public class EmbeddedLibrarySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var embedLibrary = context.AnalyzerConfigOptionsProvider
                                  .Select(
                                      static (i, _) => i.GlobalOptions.TryGetValue("build_property.RazorBladeEmbeddedLibrary", out var embedLibraryStr)
                                                       && bool.TryParse(embedLibraryStr, out var embedLibrary)
                                                       && embedLibrary
                                  );

        context.RegisterSourceOutput(embedLibrary, static (context, embedLibrary) =>
        {
            if (embedLibrary)
                EmbeddedLibrary.AddSource(context);
        });
    }
}
