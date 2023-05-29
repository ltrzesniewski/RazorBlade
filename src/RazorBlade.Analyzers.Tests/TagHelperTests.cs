using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;
using RazorBlade.Tests.Support;
using VerifyNUnit;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class TagHelperTests
{
    [Test]
    public Task should_use_tag_helpers()
    {
        return Verify(
            """
            @addTagHelper *, TestAssembly

            Hello, <span bold>this should be in bold.</span>
            """,
            """
            using Microsoft.AspNetCore.Razor.TagHelpers;

            [HtmlTargetElement(Attributes = "bold")]
            public class BoldTagHelper : TagHelper
            {
                public override void Process(TagHelperContext context, TagHelperOutput output)
                {
                    output.Attributes.RemoveAll("bold");
                    output.PreContent.SetHtmlContent("<strong>");
                    output.PostContent.SetHtmlContent("</strong>");
                }
            }
            """
        );
    }

    private static GeneratorDriverRunResult Generate(string input, string? csharpCode)
    {
        var metadataReferences = new List<MetadataReference>();

        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var outputDir = Path.GetDirectoryName(typeof(TagHelperTests).Assembly.Location)!;

        metadataReferences.AddRange(new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(RazorTemplate).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TagHelperHandler).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HtmlEncoder).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(outputDir, "Microsoft.AspNetCore.Razor.dll")),
            MetadataReference.CreateFromFile(Path.Combine(outputDir, "Microsoft.AspNetCore.Razor.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(outputDir, "Microsoft.AspNetCore.Html.Abstractions.dll"))
        });

        var analyzerConfigOptionsProvider = new AnalyzerConfigOptionsProviderMock
        {
            { "IsRazorBlade", "True" },
            { "Namespace", "TestNamespace" }
        };

        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(metadataReferences)
                                           .AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharpCode ?? string.Empty))
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                                                        .WithSpecificDiagnosticOptions(new[] { KeyValuePair.Create("CS1701", ReportDiagnostic.Suppress) })
                                                        .WithNullableContextOptions(NullableContextOptions.Enable));

        var result = CSharpGeneratorDriver.Create(new RazorBladeSourceGenerator())
                                          .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new AdditionalTextMock(input, "./TestFile.cshtml")))
                                          .WithUpdatedAnalyzerConfigOptions(analyzerConfigOptionsProvider)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                                          .GetRunResult();

        var diagnostics = updatedCompilation.GetDiagnostics();

        if (!diagnostics.IsEmpty)
            Console.WriteLine(result.GeneratedTrees.FirstOrDefault());

        diagnostics.ShouldBeEmpty();

        return result;
    }

    private static Task Verify([StringSyntax("razor")] string input,
                               [StringSyntax("csharp")] string? csharpCode)
    {
        var result = Generate(input, csharpCode);
        return Verifier.Verify(result);
    }
}
