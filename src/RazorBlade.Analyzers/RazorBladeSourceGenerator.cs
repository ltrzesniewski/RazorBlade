using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Razor;
using RazorBlade.Analyzers.Features;
using RazorBlade.Analyzers.Support;

namespace RazorBlade.Analyzers;

[Generator]
public partial class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var engine = context.ParseOptionsProvider
                            .Combine(context.AnalyzerConfigOptionsProvider)
                            .Combine(EmbeddedLibrarySourceGenerator.EmbeddedLibraryProvider(context))
                            .Select(static (pair, _) =>
                            {
                                var ((parseOptions, optionsProvider), embeddedLibrary) = pair;
                                return GlobalOptions.Create((CSharpParseOptions)parseOptions, optionsProvider, embeddedLibrary);
                            })
                            .Select(static (globalOptions, _) => new RazorBladeEngine(globalOptions));

        var imports = context.AdditionalTextsProvider
                             .Where(static additionalText => IsImportFilePath(additionalText.Path))
                             .Combine(context.AnalyzerConfigOptionsProvider)
                             .Select(static (pair, _) => (AdditionalText: pair.Left, Options: pair.Right.GetOptions(pair.Left)))
                             .Where(static pair => IsRazorBladeFile(pair.Options))
                             .Select(static (pair, _) => pair.AdditionalText)
                             .Collect();

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static additionalText => IsInputFilePath(additionalText.Path))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (pair, _) => (AdditionalText: pair.Left, Options: pair.Right.GetOptions(pair.Left)))
                                .Where(static pair => IsRazorBladeFile(pair.Options))
                                .Select(static (pair, _) => InputFile.Create(pair.AdditionalText, pair.Options));

        context.RegisterSourceOutput(
            engine,
            static (context, engine) => engine.GlobalOptions.ReportDiagnostics(context)
        );

        context.RegisterSourceOutput(
            inputFiles.Combine(imports)
                      .Combine(engine)
                      .Combine(context.CompilationProvider)
                      .WithLambdaComparer(static (a, b) => a.Left.Equals(b.Left), static pair => pair.Left.GetHashCode()), // Ignore the compilation for updates
            static (context, pair) =>
            {
                var (((inputFile, allImports), engine), compilation) = pair;

                try
                {
                    Generate(context, inputFile, allImports, engine, compilation);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    context.ReportDiagnostic(Diagnostics.InternalError(ex.Message, Location.Create(inputFile.AdditionalText.Path, default, default)));
                }
            }
        );

        static bool IsInputFilePath(string path)
            => path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
               && !IsImportFilePath(path);

        static bool IsImportFilePath(string path)
            => string.Equals(Path.GetFileName(path), "_ViewImports.cshtml", StringComparison.OrdinalIgnoreCase);

        static bool IsRazorBladeFile(AnalyzerConfigOptions options)
            => options.GetBooleanValue(Constants.FileOptions.IsRazorBlade);
    }

    private static void Generate(SourceProductionContext context,
                                 InputFile inputFile,
                                 ImmutableArray<AdditionalText> allImports,
                                 RazorBladeEngine engine,
                                 Compilation compilation)
    {
        OnGenerate();

        inputFile.ReportDiagnostics(context);

        var csharpDoc = engine.Process(inputFile, allImports);
        if (csharpDoc is null)
            return;

        var libraryCode = GenerateLibrarySpecificCode(csharpDoc, engine.GlobalOptions, compilation, context.CancellationToken);

        foreach (var diagnostic in csharpDoc.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        context.AddSource(
            $"{inputFile.HintNamespace}.{inputFile.ClassName}.Razor.g.cs",
            csharpDoc.GeneratedCode
        );

        if (!string.IsNullOrEmpty(libraryCode))
        {
            context.AddSource(
                $"{inputFile.HintNamespace}.{inputFile.ClassName}.RazorBlade.g.cs",
                libraryCode
            );
        }
    }

    private static string GenerateLibrarySpecificCode(RazorCSharpDocument generatedDoc,
                                                      GlobalOptions globalOptions,
                                                      Compilation compilation,
                                                      CancellationToken cancellationToken)
    {
        var generator = new LibraryCodeGenerator(
            generatedDoc,
            compilation,
            globalOptions.ParseOptions,
            globalOptions.AdditionalSyntaxTrees
        );

        return generator.Generate(cancellationToken);
    }

    [SuppressMessage("ReSharper", "PartialMethodWithSinglePart")]
    static partial void OnGenerate();
}
