using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
        return Verify("Hello!");
    }

    [Test]
    public Task should_write_members()
    {
        return Verify(
            """
            Hello, @Name!
            @functions { public string? Name { get; set; } }
            """
        );
    }

    [Test]
    public Task should_write_attributes()
    {
        return Verify(
            """
            Hello, <a href="@Link">World</a>!
            @functions { public string? Link { get; set; } }
            """
        );
    }

    [Test]
    public Task should_set_namespace()
    {
        return Verify(
            """
            @namespace CustomNamespace
            """
        );
    }

    [Test]
    public Task should_generate_model_constructor()
    {
        return Verify(
            """
            @using System
            @inherits RazorBlade.HtmlTemplate<Tuple<DateTime, string?>>
            """
        );
    }

    [Test]
    public Task should_forward_constructor_from_compilation()
    {
        return Verify(
            """
            @inherits Foo.BaseClass
            """,
            """
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
                protected BaseClass(float @double, string str = @"foo\""bar", DayOfWeek day = DayOfWeek.Friday)
                {
                }

                [TemplateConstructor]
                protected BaseClass(in int foo, ref int bar, out int baz, params int[] qux)
                {
                    baz = 42;
                }
            }
            """
        );
    }

    [Test]
    public Task should_not_forward_conflicting_constructor()
    {
        return Verify(
            """
            @inherits Foo.BaseClass

            @functions {
                internal TestFile(ref int value)
                    : base(value)
                {
                }
            }
            """,
            """
            using RazorBlade.Support;

            namespace Foo;

            public abstract class BaseClass : RazorBlade.HtmlTemplate
            {
                [TemplateConstructor]
                protected BaseClass(int value)
                {
                }

                [TemplateConstructor]
                protected BaseClass(in int value)
                {
                }

                [TemplateConstructor]
                protected BaseClass(string value)
                {
                }
            }
            """
        );
    }

    [Test]
    public Task should_not_forward_generic_conflicting_constructor()
    {
        return Verify(
            """
            @inherits Foo.BaseClass<string>

            @functions {
                internal TestFile(string value)
                    : base(value)
                {
                }
            }
            """,
            """
            using RazorBlade.Support;

            namespace Foo;

            public abstract class BaseClass<T> : RazorBlade.HtmlTemplate<T>
            {
                [TemplateConstructor]
                protected BaseClass(T value)
                {
                }

                [TemplateConstructor]
                protected BaseClass(int value)
                {
                }
            }
            """
        );
    }

    [Test]
    public Task should_not_forward_private_constructor()
    {
        return Verify(
            """
            @inherits Foo.BaseClass
            """,
            """
            using RazorBlade.Support;

            namespace Foo;

            public abstract class BaseClass : RazorBlade.HtmlTemplate
            {
                protected BaseClass()
                {
                }

                [TemplateConstructor]
                private BaseClass(int value)
                {
                }

                [TemplateConstructor]
                private protected BaseClass(long value)
                {
                }
            }
            """
        );
    }

    [Test]
    public Task should_forward_constructor_from_embedded_library()
    {
        return Verify(
            """
            @inherits RazorBlade.HtmlTemplate<string>
            """,
            embeddedLibrary: true
        );
    }

    [Test]
    public Task should_reject_model_directive()
    {
        return Verify(
            """
            @model FooBar
            """,
            expectErrors: true
        );
    }

    [Test]
    public Task should_mark_sync_methods_as_obsolete_on_async_templates()
    {
        return Verify(
            """
            @using System.Threading.Tasks
            @await Task.FromResult(42)
            """
        );
    }

    [Test]
    public Task should_mark_sync_methods_as_obsolete_on_async_templates_netstandard()
    {
        return Verify(
            """
            @using System.Threading.Tasks
            @await Task.FromResult(42)
            """,
            netstandard: true
        );
    }

    [Test]
    public Task should_handle_conditional_on_async_attribute()
    {
        return Verify(
            """
            @inherits Foo.BaseClassC
            @using System.Threading.Tasks
            @await Task.FromResult(42)
            """,
            """
            using System;
            using System.Threading.Tasks;
            using RazorBlade.Support;

            namespace Foo;

            public abstract class BaseClassA<TModel>
            {
                protected abstract Task ExecuteAsync();
                protected void Write(object? value) {}
                protected void WriteLiteral(object? value) {}

                [ConditionalOnAsyncAttribute(false, Message = "Hello!")]
                public void OnlyOnSync(int i) {}

                [ConditionalOnAsyncAttribute(true, Message = "Hello")]
                public void OnlyOnAsync(int i) {}

                [ConditionalOnAsyncAttribute(false, Message = "Hello!")]
                public void Generic(TModel i) {}

                [ConditionalOnAsyncAttribute(false, Message = "Hello!")]
                public void Generic<TValueA, @int>(TModel i, TValueA j) {}
            }

            public abstract class BaseClassB : BaseClassA<string>
            {
                [ConditionalOnAsyncAttribute(false, Message = "Hello")]
                public void SomeParams(float @double, string? str = @"foo\""bar", DayOfWeek day = DayOfWeek.Friday) {}

                [ConditionalOnAsyncAttribute(false, Message = @"\""\")]
                public void SomeParams(in int foo, ref int bar, out int baz, params int[] qux) { baz = 42; }
            }

            public abstract class BaseClassC : BaseClassB
            {
                [ConditionalOnAsyncAttribute(false, Message = "Hello, world!")]
                public new void OnlyOnSync(int i) {}

                [ConditionalOnAsyncAttribute(false, Message = "Hello, double")]
                public void OnlyOnSync(double i) {}
            }
            """
        );
    }

    [Test]
    public Task should_reject_tag_helper_directives()
    {
        return Verify(
            """
            @addTagHelper *, Foo
            """,
            expectErrors: true
        );
    }

    [Test]
    public Task should_handle_sections()
    {
        return Verify(
            """
            Before section
            @section SectionName { Section content }
            After section
            @section OtherSectionName { Answer is @(42) }
            """
        );
    }

    [Test]
    public Task should_detect_async_sections()
    {
        return Verify(
            """
            @using System.Threading.Tasks
            @if (42.ToString() == "42") {
                @section SectionName { @await Task.FromResult(42) }
            }
            """
        );
    }

    private static GeneratorDriverRunResult Generate(string input,
                                                     string? csharpCode,
                                                     bool embeddedLibrary,
                                                     bool netstandard,
                                                     bool expectErrors)
    {
        var metadataReferences = new List<MetadataReference>();

        if (netstandard)
        {
            metadataReferences.Add(
                MetadataReference.CreateFromFile(
                    Path.Combine(
                        Path.GetDirectoryName(typeof(RazorBladeSourceGeneratorTests).Assembly.Location)!,
                        "netstandard",
                        "netstandard.dll"
                    )
                )
            );
        }
        else
        {
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

            metadataReferences.AddRange([
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll"))
            ]);
        }

        var analyzerConfigOptionsProvider = new AnalyzerConfigOptionsProviderMock
        {
            { "IsRazorBlade", "True" },
            { "Namespace", "TestNamespace" }
        };

        if (embeddedLibrary)
        {
            analyzerConfigOptionsProvider.Add("RazorBladeEmbeddedLibrary", "true");
        }
        else
        {
            if (netstandard)
            {
                metadataReferences.Add(
                    MetadataReference.CreateFromFile(
                        Path.Combine(
                            Path.GetDirectoryName(typeof(RazorBladeSourceGeneratorTests).Assembly.Location)!,
                            "netstandard",
                            "RazorBlade.dll"
                        )
                    )
                );
            }
            else
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(typeof(RazorTemplate).Assembly.Location));
            }
        }

        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .AddReferences(metadataReferences)
                                           .AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharpCode ?? string.Empty))
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));

        var result = CSharpGeneratorDriver.Create(new RazorBladeSourceGenerator())
                                          .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new AdditionalTextMock(input, "./TestFile.cshtml")))
                                          .WithUpdatedAnalyzerConfigOptions(analyzerConfigOptionsProvider)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                                          .GetRunResult();

        var diagnostics = updatedCompilation.GetDiagnostics();

        if (expectErrors)
        {
            result.Diagnostics.ShouldContain(i => i.Severity == DiagnosticSeverity.Error);
        }
        else
        {
            result.Diagnostics.ShouldBeEmpty();

            if (!embeddedLibrary) // Don't validate the embedded library generator here, assume the final output will compile.
            {
                if (!diagnostics.IsEmpty)
                    Console.WriteLine(result.GeneratedTrees.FirstOrDefault());

                diagnostics.ShouldBeEmpty();
            }
        }

        return result;
    }

    private static Task Verify([StringSyntax("razor")] string input,
                               [StringSyntax("csharp")] string? csharpCode = null,
                               bool embeddedLibrary = false,
                               bool netstandard = false,
                               bool expectErrors = false)
    {
        var result = Generate(input, csharpCode, embeddedLibrary, netstandard, expectErrors);
        return Verifier.Verify(result);
    }
}
