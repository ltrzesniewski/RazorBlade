using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;
using RazorBlade.Tests.Support;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class RazorBladeSourceGeneratorTests
{
    [Test]
    public void should_generate_source()
    {
        var result = Generate("Hello!").SourceText.ToString();

        result.ShouldContain("namespace TestNamespace");
        result.ShouldContain("internal partial class TestFile : global::RazorBlade.HtmlTemplate");
    }

    [Test]
    public void should_write_members()
    {
        var result = Generate(@"
Hello, @Name!
@functions {{ public string Name; }}
");

        result.SourceText.ToString().ShouldContain("Write(Name)");
    }

    private static GeneratedSourceResult Generate(string input)
    {
        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(
                                               MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                               MetadataReference.CreateFromFile(typeof(RazorTemplate).Assembly.Location)
                                           );

        var result = CSharpGeneratorDriver.Create(new RazorBladeSourceGenerator())
                                          .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new AdditionalTextMock(input)))
                                          .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProviderMock())
                                          .RunGenerators(compilation)
                                          .GetRunResult();

        return result.Results.Single().GeneratedSources.Single();
    }
}
