using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Razor;
using RazorBlade.Analyzers.Support;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace RazorBlade.Analyzers;

[Generator]
public partial class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var globalOptions = context.ParseOptionsProvider
                                   .Combine(context.AnalyzerConfigOptionsProvider)
                                   .Combine(EmbeddedLibrarySourceGenerator.EmbeddedLibraryProvider(context))
                                   .Select(static (pair, _) =>
                                   {
                                       var ((parseOptions, optionsProvider), embeddedLibrary) = pair;
                                       return GlobalOptions.Create((CSharpParseOptions)parseOptions, optionsProvider, embeddedLibrary);
                                   });

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
            globalOptions,
            static (context, globalOptions) => globalOptions.ReportDiagnostics(context)
        );

        context.RegisterSourceOutput(
            inputFiles.Combine(imports)
                      .Combine(globalOptions)
                      .Combine(context.CompilationProvider)
                      .WithLambdaComparer(static (a, b) => a.Left.Equals(b.Left), static pair => pair.Left.GetHashCode()), // Ignore the compilation for updates
            static (context, pair) =>
            {
                var (((inputFile, imports), globalOptions), compilation) = pair;

                try
                {
                    Generate(context, inputFile, imports, globalOptions, compilation);
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
                                 InputFile file,
                                 ImmutableArray<AdditionalText> imports,
                                 GlobalOptions globalOptions,
                                 Compilation compilation)
    {
        OnGenerate();

        file.ReportDiagnostics(context);

        var csharpDoc = GenerateRazorCode(file, imports, globalOptions);
        if (csharpDoc is null)
            return;

        var libraryCode = GenerateLibrarySpecificCode(csharpDoc, globalOptions, compilation, context.CancellationToken);

        foreach (var diagnostic in csharpDoc.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        context.AddSource(
            $"{file.HintNamespace}.{file.ClassName}.Razor.g.cs",
            csharpDoc.GeneratedCode
        );

        if (!string.IsNullOrEmpty(libraryCode))
        {
            context.AddSource(
                $"{file.HintNamespace}.{file.ClassName}.RazorBlade.g.cs",
                libraryCode
            );
        }
    }

    private static RazorCSharpDocument? GenerateRazorCode(InputFile file,
                                                          ImmutableArray<AdditionalText> imports,
                                                          GlobalOptions globalOptions)
    {
        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return null;

        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Empty,
            cfg =>
            {
                ModelDirective.Register(cfg);
                SectionDirective.Register(cfg);

                cfg.SetCSharpLanguageVersion(globalOptions.ParseOptions.LanguageVersion);

                var configurationFeature = cfg.Features.OfType<DefaultDocumentClassifierPassFeature>().Single();

                configurationFeature.ConfigureNamespace.Add((codeDoc, node) =>
                {
                    node.Content = NamespaceVisitor.GetNamespaceDirectiveContent(codeDoc)
                                   ?? file.HintNamespace
                                   ?? "Razor";
                });

                configurationFeature.ConfigureClass.Add((_, node) =>
                {
                    node.ClassName = file.ClassName;
                    node.BaseType = "global::RazorBlade.HtmlTemplate";

                    node.Modifiers.Clear();
                    node.Modifiers.Add(SyntaxFacts.GetText(file.Accessibility ?? globalOptions.DefaultAccessibility ?? Accessibility.Internal));
                    node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.PartialKeyword));

                    // Enable nullable reference types for the class definition node, as they may be needed for the base class.
                    node.Annotations[CommonAnnotations.NullableContext] = CommonAnnotations.NullableContext;
                });

                configurationFeature.ConfigureMethod.Add((_, node) =>
                {
                    node.Modifiers.Clear();
                    node.Modifiers.Add(SyntaxFacts.GetText(Accessibility.Protected));
                    node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.AsyncKeyword));
                    node.Modifiers.Add(SyntaxFacts.GetText(SyntaxKind.OverrideKeyword));
                });

                cfg.Features.Add(new ErrorOnTagHelperSyntaxTreePass());

                cfg.AddTargetExtension(new TemplateTargetExtension { TemplateTypeName = "HelperResult" });
            }
        );

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText.ToString(), file.AdditionalText.Path, sourceText.Encoding ?? Encoding.UTF8),
            FileKinds.Legacy,
            GetImportsToApply(file.AdditionalText, imports),
            []
        );

        return codeDoc.GetCSharpDocument();

        static RazorSourceDocument[] GetImportsToApply(AdditionalText sourceText, ImmutableArray<AdditionalText> imports)
        {
            if (imports.IsEmpty)
                return [];

            var sourceDirPath = NormalizeDirectoryPath(sourceText.Path);

            return imports.Select(i => (Import: i, DirectoryPath: NormalizeDirectoryPath(i.Path)))
                          .Where(i => !string.IsNullOrEmpty(i.DirectoryPath) && sourceDirPath.StartsWith(i.DirectoryPath, StringComparison.OrdinalIgnoreCase))
                          .OrderBy(i => i.DirectoryPath.Length)
                          .Select(i => (i.Import, Text: i.Import.GetText()!))
                          .Where(i => i.Text is not null)
                          .Select(i => RazorSourceDocument.Create(i.Text.ToString(), i.Import.Path, i.Text.Encoding ?? Encoding.UTF8))
                          .ToArray();

            static string NormalizeDirectoryPath(string path)
                => Path.GetFullPath(Path.GetDirectoryName(path) ?? string.Empty).Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/') + '/';
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
