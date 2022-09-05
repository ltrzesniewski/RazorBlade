using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;

#nullable enable

namespace RazorBlade.Analyzers;

[Generator]
public class RazorBladeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var inputFiles = context.AdditionalTextsProvider
                                .Where(static i => i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase));

        context.RegisterSourceOutput(inputFiles, Generate);
    }

    private void Generate(SourceProductionContext context, AdditionalText inputFile)
    {
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(Path.GetDirectoryName(inputFile.Path)),
            _ => { }
        );

        var sourceText = inputFile.GetText();
        if (sourceText is null)
            return;

        var codeDoc = engine.Process(
            RazorSourceDocument.Create(sourceText.ToString(), inputFile.Path, sourceText.Encoding),
            FileKinds.GetFileKindFromFilePath(inputFile.Path),
            Array.Empty<RazorSourceDocument>(),
            Array.Empty<TagHelperDescriptor>()
        );

        var csharpDoc = codeDoc.GetCSharpDocument();

        context.AddSource(Path.GetFileName(inputFile.Path), csharpDoc.GeneratedCode);
    }
}
