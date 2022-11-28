using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;
using RazorBlade.MetaAnalyzers;
using RazorBlade.Tests.Support;
using VerifyNUnit;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class EmbeddedLibraryMetaSourceGeneratorTests
{
    [Test]
    public Task should_update_accessibility_levels()
    {
        return Verify("""
            public abstract class TestClass
            {
                class InnerClass { }
                public class InnerPublicClass { }
                private class InnerPrivateClass { }

                public delegate void InnerDelegate();

                protected internal Method() { }
                protected internal string Property { get; internal set; }
            }

            class TestClass2 { }

            public readonly struct TestStruct { }
            public interface TestInterface { }
            public record TestRecordClass { }
            public record struct TestRecordStruct { }
            public enum TestEnum { }
            public delegate void TestDelegate();
            """);
    }

    [Test]
    public Task should_remove_jetbrains_annotations()
    {
        return Verify("""
            using System;
            using JetBrains.Annotations;
            using System.Text;

            public class TestClass
            {
                [PublicAPI, Obsolete, UsedImplicitly]
                public string TestMethodA([NotNull, UsedImplicitly] StringBuilder sb)
                    => sb.ToString();

                [Obsolete, UsedImplicitly]
                public string TestMethodB(StringBuilder sb)
                    => sb.ToString();

                [UsedImplicitly, Obsolete]
                public string TestMethodC(StringBuilder sb)
                    => sb.ToString();

                /// <summary>
                /// Trivia.
                /// </summary>
                [UsedImplicitly]
                public string TestMethodD(StringBuilder sb)
                    => sb.ToString();

                /// <summary>
                /// Trivia.
                /// </summary>
                [PublicAPI]
                [Obsolete]
                [UsedImplicitly]
                public string TestMethodE([NotNull, UsedImplicitly] StringBuilder sb)
                    => sb.ToString();
            }
            """);
    }

    private static GeneratorDriverRunResult Generate(string input)
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(
                                               MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
                                               MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll"))
                                           )
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));

        var result = CSharpGeneratorDriver.Create(new EmbeddedLibraryMetaSourceGenerator
                                          {
                                              SkipAddSource = true
                                          })
                                          .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new AdditionalTextMock(input, "./TestFile.cs")))
                                          .WithUpdatedAnalyzerConfigOptions(new AnalyzerConfigOptionsProviderMock
                                          {
                                              { "Role", "Library" }
                                          })
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                                          .GetRunResult();

        var diagnostics = updatedCompilation.GetDiagnostics();

        if (!diagnostics.IsEmpty)
            Console.WriteLine(result.GeneratedTrees.FirstOrDefault());

        diagnostics.ShouldBeEmpty();

        return result;
    }

    private static Task Verify([StringSyntax("csharp")] string input)
    {
        var result = Generate(input);
        return Verifier.Verify(result);
    }
}
