using System.Collections.Immutable;
using System.IO;
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
@functions { public string? Name { get; set; } }
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
@inherits RazorBlade.HtmlTemplate<Tuple<DateTime, string?>>
");

        return Verifier.Verify(result);
    }

    [Test]
    public Task should_forward_constructor_from_compilation()
    {
        var result = Generate(@"
@inherits Foo.BaseClass
", @"
using System;
using RazorBlade.Support;

namespace Foo;

public abstract class BaseClass : RazorBlade.HtmlTemplate
{
    protected BaseClass(int notIncluded)
    {
    }

    [TemplateConstructor]
    protected BaseClass(int? foo, string? bar)
    {
    }

    [TemplateConstructor]
    protected BaseClass(float @double, string str = @""foo\""""bar"", DayOfWeek day = DayOfWeek.Friday)
    {
    }

    [TemplateConstructor]
    protected BaseClass(in int foo, ref int bar, out int baz, params int[] qux)
    {
        baz = 42;
    }
}
");

        return Verifier.Verify(result);
    }

    private static GeneratedSourceResult Generate(string input, string? csharpCode = null)
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(
                                               MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                                               MetadataReference.CreateFromFile(typeof(RazorTemplate).Assembly.Location)
                                           )
                                           .AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharpCode ?? string.Empty))
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));

        var result = CSharpGeneratorDriver.Create(new RazorBladeSourceGenerator())
                                          .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new AdditionalTextMock(input)))
                                          .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProviderMock())
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics)
                                          .GetRunResult();

        updatedCompilation.GetDiagnostics().ShouldBeEmpty();
        diagnostics.ShouldBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
        return result.Results.Single().GeneratedSources.Single();
    }
}
