using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;
using RazorBlade.Tests.Support;
using VerifyNUnit;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class RazorBladeSourceGeneratorTests
{
    [Test]
    public Task should_generate_source()
    {
        var sourceResult = Generate("Hello!");
        var result = sourceResult.SourceText.ToString();

        result.ShouldContain("namespace TestNamespace");
        result.ShouldContain("internal partial class TestFile : global::RazorBlade.HtmlTemplate");
        return Verifier.Verify(sourceResult);
    }

    [Test]
    public Task should_write_members()
    {
        var result = Generate(@"
Hello, @Name!
@functions {{ public string Name; }}
");

        result.SourceText.ToString().ShouldContain("Write(Name)");
        return Verifier.Verify(result);
    }

    [Test]
    public Task should_set_namespace()
    {
        var result = Generate(@"
@namespace CustomNamespace
");

        result.SourceText.ToString().ShouldContain("namespace CustomNamespace");
        return Verifier.Verify(result);
    }

    [Test]
    public Task should_generate_model_constructor()
    {
        var result = Generate(@"
@using System
@inherits RazorBlade.HtmlTemplate<Tuple<DateTime, bool>>
");

        return Verifier.Verify(result);
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

        result.Diagnostics.ShouldBeEmpty();
        return result.Results.Single().GeneratedSources.Single();
    }
}
