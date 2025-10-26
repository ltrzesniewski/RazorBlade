using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

        var cancellationToken = context.CancellationToken;

        inputFile.ReportDiagnostics(context);

        if (engine.Process(inputFile, allImports) is not { } codeDocument)
            return;

        if (codeDocument.GetCSharpDocument() is not { } csharpDocument)
            return;

        foreach (var diagnostic in csharpDocument.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        if (BuildTimeTemplateRunner.TryGenerate(codeDocument, cancellationToken) is { } buildTimeTemplateOutput)
        {
            context.AddSource(
                $"{inputFile.HintNamespace}.{inputFile.ClassName}.CSharp.g.cs",
                buildTimeTemplateOutput
            );

            return;
        }

        var libraryCode = GenerateLibrarySpecificCode(csharpDocument, engine.GlobalOptions, compilation, context.CancellationToken);

        context.AddSource(
            $"{inputFile.HintNamespace}.{inputFile.ClassName}.Razor.g.cs",
            csharpDocument.GeneratedCode
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
