using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using RazorBlade.Analyzers.Features;

namespace RazorBlade.Analyzers;

internal class RazorBladeEngine
{
    public GlobalOptions GlobalOptions { get; }
    public DefaultRazorProjectEngine RazorEngine { get; }

    public RazorBladeEngine(GlobalOptions? globalOptions)
    {
        GlobalOptions = globalOptions ?? GlobalOptions.CreateEmpty();

        RazorEngine = (DefaultRazorProjectEngine)RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Empty,
            builder =>
            {
                builder.SetCSharpLanguageVersion(GlobalOptions.ParseOptions.LanguageVersion);

                RazorBladeDocumentFeature.Register(builder, GlobalOptions);

                ModelDirective.Register(builder);
                SectionDirective.Register(builder);
                TagHelperDirective.Register(builder);
                TypeParamDirective.Register(builder);

                builder.AddTargetExtension(new TemplateTargetExtension { TemplateTypeName = "HelperResult" });
            }
        );
    }

    public RazorCSharpDocument? Process(InputFile inputFile, ImmutableArray<AdditionalText> allImports)
    {
        var codeDocument = CreateRazorCodeDocument(inputFile, allImports);
        if (codeDocument is null)
            return null;

        RazorEngine.Engine.Process(codeDocument);

        return codeDocument.GetCSharpDocument();
    }

    private RazorCodeDocument? CreateRazorCodeDocument(InputFile inputFile, ImmutableArray<AdditionalText> allImports)
    {
        var sourceText = inputFile.AdditionalText.GetText();
        if (sourceText is null)
            return null;

        var sourceDocument = RazorSourceDocument.Create(
            sourceText.ToString(),
            inputFile.AdditionalText.Path,
            sourceText.Encoding ?? Encoding.UTF8
        );

        var codeDocument = RazorEngine.CreateCodeDocumentCore(
            sourceDocument,
            FileKinds.Legacy,
            GetImportsToApply(inputFile.AdditionalText, allImports)
        );

        // We have to create the RazorCodeDocument manually in order to be able to set this item before the engine starts processing.
        codeDocument.Items[typeof(InputFile)] = inputFile;

        return codeDocument;
    }

    private static RazorSourceDocument[] GetImportsToApply(AdditionalText sourceText, ImmutableArray<AdditionalText> allImports)
    {
        if (allImports.IsEmpty)
            return [];

        var sourceDirPath = NormalizeDirectoryPath(sourceText.Path);

        return allImports.Select(i => (Import: i, DirectoryPath: NormalizeDirectoryPath(i.Path)))
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
