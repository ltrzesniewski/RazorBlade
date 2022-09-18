using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Razor;
using RazorBlade.Analyzers.Support;

namespace RazorBlade.Analyzers;

[Generator]
public partial class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var globalOptions = context.ParseOptionsProvider
                                   .Select(static (parseOptions, _) => GetGlobalOptions(parseOptions));

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (pair, _) => GetInputFile(pair.Left, pair.Right))
                                .WhereNotNull()
                                .Combine(globalOptions);

        context.RegisterSourceOutput(inputFiles, static (context, args) => GenerateSafe(context, args.Left, args.Right));
    }

    private static GlobalOptions GetGlobalOptions(ParseOptions parseOptions)
    {
        return new GlobalOptions(
            ((CSharpParseOptions)parseOptions).LanguageVersion
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

        return new InputFile(additionalText, ns, Path.GetFileNameWithoutExtension(additionalText.Path));
    }

    private static void GenerateSafe(SourceProductionContext context, InputFile file, GlobalOptions globalOptions)
    {
        try
        {
            Generate(context, file, globalOptions);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostics.InternalError(ex.Message, Location.Create(file.AdditionalText.Path, default, default)));
        }
    }

    private static void Generate(SourceProductionContext context, InputFile file, GlobalOptions globalOptions)
    {
        OnGenerate();

        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Empty,
            cfg =>
            {
                cfg.SetCSharpLanguageVersion(globalOptions.LanguageVersion);

                var configurationFeature = cfg.Features.OfType<DefaultDocumentClassifierPassFeature>().Single();

                configurationFeature.ConfigureNamespace.Add((_, node) => node.Content = file.Namespace ?? "Razor");

                configurationFeature.ConfigureClass.Add((_, node) =>
                {
                    node.ClassName = file.ClassName;
                    node.BaseType = "global::RazorBlade.HtmlTemplate";

                    node.Modifiers.Clear();
                    node.Modifiers.Add("internal");
                    node.Modifiers.Add("partial");
                });

                configurationFeature.ConfigureMethod.Add((_, node) =>
                {
                    node.Modifiers.Clear();
                    node.Modifiers.Add("protected");
                    node.Modifiers.Add("async");
                    node.Modifiers.Add("override");
                });
            }
        );

        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return;

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText.ToString(), file.AdditionalText.Path, sourceText.Encoding ?? Encoding.UTF8),
            FileKinds.GetFileKindFromFilePath(file.AdditionalText.Path),
            Array.Empty<RazorSourceDocument>(),
            Array.Empty<TagHelperDescriptor>()
        );

        var csharpDoc = codeDoc.GetCSharpDocument();

        foreach (var diagnostic in csharpDoc.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        context.AddSource($"{file.Namespace}.{file.ClassName}.g.cs", csharpDoc.GeneratedCode);
    }

    static partial void OnGenerate();

    private record InputFile(AdditionalText AdditionalText, string? Namespace, string ClassName);

    private record GlobalOptions(LanguageVersion LanguageVersion);
}
