using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RazorBlade.Analyzers;

[Generator]
public class EmbeddedLibrarySourceGenerator : IIncrementalGenerator
{
    public const LanguageVersion MinimumSupportedLanguageVersion = LanguageVersion.CSharp10;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var embeddedLibrary = EmbeddedLibraryFlagProvider(context);

        var langVersion = context.ParseOptionsProvider
                                 .Select((parseOptions, _) => ((CSharpParseOptions)parseOptions).LanguageVersion);

        var input = embeddedLibrary.Combine(langVersion);

        context.RegisterSourceOutput(input, static (context, input) =>
        {
            var (embedLibrary, langVersion) = input;

            if (!embedLibrary)
                return;

            if (langVersion < MinimumSupportedLanguageVersion)
            {
                context.ReportDiagnostic(Diagnostics.EmbeddedLibraryUnsupportedCSharpVersion(MinimumSupportedLanguageVersion));
                return;
            }

            foreach (var file in EmbeddedLibrary.Files)
                context.AddSource($"{file.Name}.g.cs", file.Source);
        });
    }

    public static IncrementalValueProvider<bool> EmbeddedLibraryFlagProvider(IncrementalGeneratorInitializationContext context)
        => context.AnalyzerConfigOptionsProvider
                  .Select(
                      static (i, _) => i.GlobalOptions.TryGetValue("build_property.RazorBladeEmbeddedLibrary", out var embedLibraryStr)
                                       && bool.TryParse(embedLibraryStr, out var embedLibrary)
                                       && embedLibrary
                  );
}
