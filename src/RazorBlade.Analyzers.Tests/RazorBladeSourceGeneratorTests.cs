using System.Collections.Immutable;
using System.IO;
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
        return Verify("Hello!");
    }

    [Test]
    public Task should_write_members()
    {
        return Verify(@"
Hello, @Name!
@functions { public string? Name { get; set; } }
");
    }

    [Test]
    public Task should_write_attributes()
    {
        return Verify(@"
Hello, <a href=""@Link"">World</a>!
@functions { public string? Link { get; set; } }
");
    }

    [Test]
    public Task should_set_namespace()
    {
        return Verify(@"
@namespace CustomNamespace
");
    }

    [Test]
    public Task should_generate_model_constructor()
    {
        return Verify(@"
@using System
@inherits RazorBlade.HtmlTemplate<Tuple<DateTime, string?>>
");
    }

    [Test]
    public Task should_forward_constructor_from_compilation()
    {
        return Verify(@"
@inherits Foo.BaseClass
",
                      @"
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
    }

    [Test]
    public Task should_reject_model_directive()
    {
        return Verify(@"
@model FooBar
");
    }

    private static GeneratorDriverRunResult Generate(string input, string? csharpCode = null)
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
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                                          .GetRunResult();

        updatedCompilation.GetDiagnostics().ShouldBeEmpty();
        return result;
    }

    private static Task Verify(string input, string? csharpCode = null)
    {
        var result = Generate(input, csharpCode);
        return Verifier.Verify(result);
    }
}
