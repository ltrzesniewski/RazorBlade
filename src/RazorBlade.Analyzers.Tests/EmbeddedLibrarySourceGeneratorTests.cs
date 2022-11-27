using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;
using RazorBlade.Tests.Support;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class EmbeddedLibrarySourceGeneratorTests
{
    [Test, Explicit] // TODO: Make this pass
    public void should_generate_valid_source()
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(
                                               MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll"))
                                           )
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));

        CSharpGeneratorDriver.Create(new EmbeddedLibrarySourceGenerator())
                             .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProviderMock
                             {
                                 { "RazorBladeEmbeddedLibrary", "True" }
                             })
                             .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                             .GetRunResult();

        var diagnostics = updatedCompilation.GetDiagnostics();
        diagnostics.ShouldBeEmpty();
    }
}
