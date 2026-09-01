using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RazorBlade.Analyzers.Support;
using RazorBlade.MetaAnalyzers;

namespace RazorBlade.Analyzers;

[Generator]
public class EmbeddedLibrarySourceGenerator : IIncrementalGenerator
{
    public const LanguageVersion MinimumSupportedLanguageVersion = LanguageVersion.CSharp10;

    // language=csharp
    private const string _embeddedAttributeSource = """
        namespace Microsoft.CodeAnalysis
        {
            internal sealed partial class EmbeddedAttribute : global::System.Attribute
            { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var embeddedLibrary = EmbeddedLibraryFlagProvider(context);

        var langVersion = context.ParseOptionsProvider
                                 .Select(static (parseOptions, _) => ((CSharpParseOptions)parseOptions).LanguageVersion);

        var input = embeddedLibrary.Combine(langVersion);

        context.RegisterSourceOutput(
            input,
            static (context, input) =>
            {
                var (embeddedLibrary, langVersion) = input;

                if (embeddedLibrary == EmbeddedLibraryFlag.False)
                    return;

                if (langVersion < MinimumSupportedLanguageVersion)
                {
                    context.ReportDiagnostic(Diagnostics.EmbeddedLibraryUnsupportedCSharpVersion(MinimumSupportedLanguageVersion));
                    return;
                }

                foreach (var file in EmbeddedLibrary.Files)
                    context.AddSource($"{file.Name}.g.cs", TransformEmbeddedSource(file, embeddedLibrary));

                if (embeddedLibrary == EmbeddedLibraryFlag.Private)
                    context.AddSource("EmbeddedAttribute.g.cs", _embeddedAttributeSource);
            }
        );
    }

    private static IncrementalValueProvider<EmbeddedLibraryFlag> EmbeddedLibraryFlagProvider(IncrementalGeneratorInitializationContext context)
        => context.AnalyzerConfigOptionsProvider
                  .Select(static (i, _) => i.GlobalOptions.GetEnumValue<EmbeddedLibraryFlag>(Constants.GlobalOptions.EmbeddedLibrary));

    public static IncrementalValueProvider<ImmutableArray<SyntaxTree>> EmbeddedLibraryProvider(IncrementalGeneratorInitializationContext context)
        => context.ParseOptionsProvider
                  .Combine(EmbeddedLibraryFlagProvider(context))
                  .Select(static (pair, cancellationToken) =>
                  {
                      var (parseOptions, embeddedLibrary) = pair;

                      if (embeddedLibrary == EmbeddedLibraryFlag.False)
                          return ImmutableArray<SyntaxTree>.Empty;

                      var additionalFiles = new List<SyntaxTree>();

                      foreach (var file in EmbeddedLibrary.Files)
                      {
                          additionalFiles.Add(
                              CSharpSyntaxTree.ParseText(
                                  TransformEmbeddedSource(file, embeddedLibrary),
                                  (CSharpParseOptions?)parseOptions,
                                  cancellationToken: cancellationToken
                              )
                          );
                      }

                      if (embeddedLibrary == EmbeddedLibraryFlag.Private)
                      {
                          additionalFiles.Add(
                              CSharpSyntaxTree.ParseText(
                                  _embeddedAttributeSource,
                                  (CSharpParseOptions?)parseOptions,
                                  cancellationToken: cancellationToken
                              ));
                      }

                      return additionalFiles.ToImmutableArray();
                  });

    private static string TransformEmbeddedSource(EmbeddedLibrary.File file, EmbeddedLibraryFlag flag)
    {
        var source = file.Source;

        return flag switch
        {
            EmbeddedLibraryFlag.True    => source.Replace(Contracts.EmbeddedComment, string.Empty),
            EmbeddedLibraryFlag.Private => source.Replace(Contracts.EmbeddedComment, "[global::Microsoft.CodeAnalysis.Embedded] "),
            _                           => string.Empty,
        };
    }

    private enum EmbeddedLibraryFlag
    {
        False,
        True,
        Private
    }
}
