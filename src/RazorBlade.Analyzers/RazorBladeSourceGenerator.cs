using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;
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

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (pair, _) =>
                                {
                                    var (additionalText, optionsProvider) = pair;
                                    return InputFile.Create(additionalText, optionsProvider);
                                })
                                .WhereNotNull();

        context.RegisterSourceOutput(
            globalOptions,
            static (context, globalOptions) => globalOptions.ReportDiagnostics(context)
        );

        context.RegisterSourceOutput(
            inputFiles.Combine(globalOptions)
                      .Combine(context.CompilationProvider)
                      .WithLambdaComparer((a, b) => a.Left.Equals(b.Left), pair => pair.Left.GetHashCode()), // Ignore the compilation for updates
            static (context, pair) =>
            {
                var ((inputFile, globalOptions), compilation) = pair;

                try
                {
                    Generate(context, inputFile, globalOptions, compilation);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    context.ReportDiagnostic(Diagnostics.InternalError(ex.Message, Location.Create(inputFile.AdditionalText.Path, default, default)));
                }
            }
        );
    }

    private static void Generate(SourceProductionContext context, InputFile file, GlobalOptions globalOptions, Compilation compilation)
    {
        OnGenerate();

        file.ReportDiagnostics(context);

        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return;

        var csharpDoc = GenerateRazorCode(sourceText, file, globalOptions);
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

    private static RazorCSharpDocument GenerateRazorCode(SourceText sourceText, InputFile file, GlobalOptions globalOptions)
    {
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
                    node.BaseType = new BaseTypeWithModel("global::RazorBlade.HtmlTemplate");

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
            RazorSourceDocument.Create(
                SourceText.From(sourceText.ToString(), sourceText.Encoding),
                RazorSourceDocumentProperties.Create(file.AdditionalText.Path, relativePath: null)
            ),
            FileKinds.Legacy,
            [],
            []
        );

        return codeDoc.GetCSharpDocument();
    }

    private static string GenerateLibrarySpecificCode(RazorCSharpDocument generatedDoc, GlobalOptions globalOptions, Compilation compilation, CancellationToken cancellationToken)
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
