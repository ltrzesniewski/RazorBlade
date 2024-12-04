using System;
using System.Collections.Immutable;
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
using Microsoft.CodeAnalysis.Text;
using RazorBlade.Analyzers.Support;

namespace RazorBlade.Analyzers;

[Generator]
public partial class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var globalOptions = context.ParseOptionsProvider
                                   .Combine(EmbeddedLibrarySourceGenerator.EmbeddedLibraryProvider(context))
                                   .Select(static (pair, _) =>
                                   {
                                       var (parseOptions, embeddedLibrary) = pair;

                                       return new GlobalOptions(
                                           (CSharpParseOptions)parseOptions,
                                           embeddedLibrary
                                       );
                                   });

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (pair, _) => GetInputFile(pair.Left, pair.Right))
                                .WhereNotNull();

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

    private static InputFile? GetInputFile(AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var options = optionsProvider.GetOptions(additionalText);

        options.TryGetValue("build_metadata.AdditionalFiles.IsRazorBlade", out var isTargetFile);
        if (!string.Equals(isTargetFile, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!options.TryGetValue("build_metadata.AdditionalFiles.Namespace", out var ns))
            ns = null;

        return new InputFile(
            additionalText,
            ns,
            CSharpIdentifier.SanitizeIdentifier(Path.GetFileNameWithoutExtension(additionalText.Path))
        );
    }

    private static void Generate(SourceProductionContext context, InputFile file, GlobalOptions globalOptions, Compilation compilation)
    {
        OnGenerate();

        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return;

        var csharpDoc = GenerateRazorCode(sourceText, file, globalOptions);
        var libraryCode = GenerateLibrarySpecificCode(csharpDoc, globalOptions, compilation, context.CancellationToken);

        foreach (var diagnostic in csharpDoc.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        context.AddSource(
            $"{file.Namespace}.{file.ClassName}.Razor.g.cs",
            csharpDoc.GeneratedCode
        );

        if (!string.IsNullOrEmpty(libraryCode))
        {
            context.AddSource(
                $"{file.Namespace}.{file.ClassName}.RazorBlade.g.cs",
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
                                   ?? file.Namespace
                                   ?? "Razor";
                });

                configurationFeature.ConfigureClass.Add((_, node) =>
                {
                    node.ClassName = file.ClassName;
                    node.BaseType = "global::RazorBlade.HtmlTemplate";

                    node.Modifiers.Clear();
                    node.Modifiers.Add("internal");
                    node.Modifiers.Add("partial");

                    // Enable nullable reference types for the class definition node, as they may be needed for the base class.
                    node.Annotations[CommonAnnotations.NullableContext] = CommonAnnotations.NullableContext;
                });

                configurationFeature.ConfigureMethod.Add((_, node) =>
                {
                    node.Modifiers.Clear();
                    node.Modifiers.Add("protected");
                    node.Modifiers.Add("async");
                    node.Modifiers.Add("override");
                });

                cfg.Features.Add(new ErrorOnTagHelperSyntaxTreePass());

                cfg.AddTargetExtension(new TemplateTargetExtension { TemplateTypeName = "HelperResult" });
            }
        );

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText.ToString(), file.AdditionalText.Path, sourceText.Encoding ?? Encoding.UTF8),
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

    static partial void OnGenerate();

    private record InputFile(AdditionalText AdditionalText, string? Namespace, string ClassName);

    private record GlobalOptions(CSharpParseOptions ParseOptions, ImmutableArray<SyntaxTree> AdditionalSyntaxTrees);
}
