using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Razor;
using RazorBlade.Analyzers.Support;

#nullable enable

namespace RazorBlade.Analyzers;

[Generator]
public class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var razorOptions = context.AnalyzerConfigOptionsProvider
                                  .Combine(context.ParseOptionsProvider)
                                  .Select((pair, _) => GetRazorOptions(pair.Left, pair.Right));

        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                                .Combine(context.AnalyzerConfigOptionsProvider)
                                .Select(static (item, _) =>
                                {
                                    var (additionalText, optionsProvider) = item;
                                    var options = optionsProvider.GetOptions(additionalText);

                                    options.TryGetValue("build_metadata.AdditionalFiles.IsRazorBlade", out var isTargetFile);
                                    if (!string.Equals(isTargetFile, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                                        return null;

                                    options.TryGetValue("build_metadata.AdditionalFiles.Namespace", out var ns);

                                    return new RazorFile(additionalText, ns, Path.GetFileNameWithoutExtension(additionalText.Path));
                                })
                                .WhereNotNull()
                                .Combine(razorOptions);

        context.RegisterSourceOutput(inputFiles, static (context, args) => Generate(context, args.Left, args.Right));
    }

    private static RazorOptions GetRazorOptions(AnalyzerConfigOptionsProvider configOptionsProvider, ParseOptions parseOptions)
    {
        configOptionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

        return new RazorOptions(
            rootNamespace ?? "Razor",
            ((CSharpParseOptions)parseOptions).LanguageVersion
        );
    }

    private static void Generate(SourceProductionContext context, RazorFile file, RazorOptions razorOptions)
    {
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(Path.GetDirectoryName(file.AdditionalText.Path)),
            cfg =>
            {
                cfg.SetCSharpLanguageVersion(razorOptions.LanguageVersion);

                cfg.SetNamespace(file.Namespace ?? "Razor"); // TODO: Use SetRootNamespace instead?

                cfg.ConfigureClass((_, node) =>
                {
                    node.ClassName = file.ClassName;

                    node.Modifiers.Clear();
                    node.Modifiers.Add("internal");
                });
            }
        );

        var sourceText = file.AdditionalText.GetText();
        if (sourceText is null)
            return;

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText.ToString(), file.AdditionalText.Path, sourceText.Encoding),
            FileKinds.GetFileKindFromFilePath(file.AdditionalText.Path),
            Array.Empty<RazorSourceDocument>(),
            Array.Empty<TagHelperDescriptor>()
        );

        var csharpDoc = codeDoc.GetCSharpDocument();

        foreach (var diagnostic in csharpDoc.Diagnostics)
            context.ReportDiagnostic(diagnostic.ToDiagnostic());

        context.AddSource($"{file.Namespace}.{file.ClassName}.g.cs", csharpDoc.GeneratedCode);
    }

    private record RazorFile(AdditionalText AdditionalText, string? Namespace, string ClassName);

    private record RazorOptions(string RootNamespace, LanguageVersion LanguageVersion);
}
