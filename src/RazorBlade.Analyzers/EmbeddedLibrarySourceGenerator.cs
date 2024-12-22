using System.Collections.Immutable;
using System.Linq;
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

        context.RegisterSourceOutput(
            input,
            static (context, input) =>
            {
                var (embeddedLibrary, langVersion) = input;

                if (!embeddedLibrary)
                    return;

                if (langVersion < MinimumSupportedLanguageVersion)
                {
                    context.ReportDiagnostic(Diagnostics.EmbeddedLibraryUnsupportedCSharpVersion(MinimumSupportedLanguageVersion));
                    return;
                }

                foreach (var file in EmbeddedLibrary.Files)
                    context.AddSource($"{file.Name}.g.cs", file.Source);
            }
        );
    }

    private static IncrementalValueProvider<bool> EmbeddedLibraryFlagProvider(IncrementalGeneratorInitializationContext context)
        => context.AnalyzerConfigOptionsProvider
                  .Select(
                      static (i, _) => i.GlobalOptions.TryGetValue(Constants.GlobalOptions.EmbeddedLibrary, out var embeddedLibraryStr)
                                       && bool.TryParse(embeddedLibraryStr, out var embeddedLibrary)
                                       && embeddedLibrary
                  );

    public static IncrementalValueProvider<ImmutableArray<SyntaxTree>> EmbeddedLibraryProvider(IncrementalGeneratorInitializationContext context)
        => context.ParseOptionsProvider
                  .Combine(EmbeddedLibraryFlagProvider(context))
                  .Select(static (pair, cancellationToken) =>
                  {
                      var (parseOptions, embeddedLibrary) = pair;

                      if (!embeddedLibrary)
                          return ImmutableArray<SyntaxTree>.Empty;

                      return EmbeddedLibrary.Files
                                            .Select(file => CSharpSyntaxTree.ParseText(
                                                        file.Source,
                                                        (CSharpParseOptions?)parseOptions,
                                                        cancellationToken: cancellationToken
                                                    ))
                                            .ToImmutableArray();
                  });
}
