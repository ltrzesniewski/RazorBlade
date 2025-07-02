using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Razor.Language;
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
    public Task should_handle_new_csharp_features()
    {
        return Verify(
            """
            @using System.Collections.Generic
            @functions
            {
                public partial string Name { get; }
                public partial string Name => "Foo";

                public T Foo<T>(T value) where T : allows ref struct => value;
                public string Foo<T>(params IEnumerable<T> values) => "a\eb";
            }
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
            using System.Collections.Generic;
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

                [TemplateConstructor]
                protected BaseClass(in int foo, ref int bar, out int baz, params IEnumerable<int> qux)
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
            config: new()
            {
                EmbeddedLibrary = true
            }
        );
    }

    [Test]
    public Task should_reject_model_directive()
    {
        return Verify(
            """
            @model string
            """,
            config: new()
            {
                ExpectErrors = true
            }
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
            config: new()
            {
                NetStandard = true
            }
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
            config: new()
            {
                ExpectErrors = true
            }
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

    [Test]
    public Task should_handle_templated_delegates()
    {
        return Verify(
            """
            @using System
            @{ Func<string, object> bold = @<b>@item</b>; }
            @bold("Bold but not <i>italic</i>")
            """
        );
    }

    [Test]
    public Task should_set_accessibility()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ConfigOptions = { [Constants.FileOptions.Accessibility] = "public" }
            }
        );
    }

    [Test]
    public Task should_report_invalid_accessibility()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ConfigOptions = { [Constants.FileOptions.Accessibility] = "foo" },
                ExpectErrors = true
            }
        );
    }

    [Test]
    public Task should_set_default_accessibility()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ConfigOptions = { [Constants.GlobalOptions.DefaultAccessibility] = "public" }
            }
        );
    }

    [Test]
    public Task should_report_invalid_default_accessibility()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ConfigOptions = { [Constants.GlobalOptions.DefaultAccessibility] = "foo" },
                ExpectErrors = true
            }
        );
    }

    [Test]
    public Task should_prioritize_accessibility_over_default_accessibility()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ConfigOptions =
                {
                    [Constants.FileOptions.Accessibility] = "public",
                    [Constants.GlobalOptions.DefaultAccessibility] = "internal"
                }
            }
        );
    }

    [Test]
    public Task should_import_relevant_files()
    {
        return Verify(
            """
            Hello
            """,
            """
            class WrongBase : RazorBlade.HtmlTemplate;
            class CorrectBase : RazorBlade.HtmlTemplate;
            """,
            config: new()
            {
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@inherits WrongBase", "./Path/Dir/Unrelated/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits CorrectBase", "./Path/Dir/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./Path/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./Path/OtherDir/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./OtherPath/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./_ViewImports.cshtml")
                ]
            }
        );
    }

    [Test]
    public Task should_import_relevant_files_reversed()
    {
        return Verify(
            """
            Hello
            """,
            """
            class WrongBase : RazorBlade.HtmlTemplate;
            class CorrectBase : RazorBlade.HtmlTemplate;
            """,
            config: new()
            {
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@inherits WrongBase", "./_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./OtherPath/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./Path/OtherDir/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./Path/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits CorrectBase", "./Path/Dir/_ViewImports.cshtml"),
                    RazorAdditionalText("@inherits WrongBase", "./Path/Dir/Unrelated/_ViewImports.cshtml")
                ]
            }
        );
    }

    [Test]
    public Task should_import_all_relevant_files()
    {
        return Verify(
            """
            Hello
            """,
            """
            namespace A { class Foo; }
            namespace B { class Foo; }
            namespace C { class Foo; }
            namespace D { class Foo; }
            namespace E { class Foo; }
            namespace F { class Foo; }
            """,
            config: new()
            {
                ExpectDiagnostics = true,
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@using A", "./Path/Dir/Unrelated/_ViewImports.cshtml"),
                    RazorAdditionalText("@using B", "./Path/Dir/_ViewImports.cshtml"),
                    RazorAdditionalText("@using C", "./Path/_ViewImports.cshtml"),
                    RazorAdditionalText("@using D", "./Path/OtherDir/_ViewImports.cshtml"),
                    RazorAdditionalText("@using E", "./OtherPath/_ViewImports.cshtml"),
                    RazorAdditionalText("@using F", "./_ViewImports.cshtml")
                ]
            }
        );
    }

    [Test]
    public Task should_not_generate_code_for_imports_file()
    {
        return Verify(
            """
            @namespace Test
            """,
            config: new()
            {
                FilePath = "./Path/Dir/_ViewImports.cshtml"
            }
        );
    }

    [Test]
    public Task should_resolve_namespace_from_imports()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@namespace WrongNamespace", "./Path/Dir/Unrelated/_ViewImports.cshtml"),
                    RazorAdditionalText("@namespace CorrectNamespace", "./Path/Dir/_ViewImports.cshtml"),
                    RazorAdditionalText("@namespace WrongNamespace", "./Path/_ViewImports.cshtml"),
                    RazorAdditionalText("@namespace WrongNamespace", "./Path/OtherDir/_ViewImports.cshtml"),
                    RazorAdditionalText("@namespace WrongNamespace", "./OtherPath/_ViewImports.cshtml"),
                    RazorAdditionalText("@namespace WrongNamespace", "./_ViewImports.cshtml")
                ]
            }
        );
    }

    [Test]
    public Task should_override_namespace_from_imports()
    {
        return Verify(
            """
            @namespace OverridenNamespace
            Hello
            """,
            config: new()
            {
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@namespace NamespaceFromImports", "./Path/Dir/_ViewImports.cshtml"),
                ]
            }
        );
    }

    [Test]
    public Task should_error_on_model_in_imports()
    {
        return Verify(
            """
            Hello
            """,
            config: new()
            {
                ExpectErrors = true,
                FilePath = "./Path/Dir/TestFile.cshtml",
                AdditionalTexts =
                [
                    RazorAdditionalText("@model string", "./Path/Dir/_ViewImports.cshtml"),
                ]
            }
        );
    }

    [Test]
    public Task should_handle_typeparam()
    {
        return Verify(
            """
            @typeparam T1
            @typeparam T2 where T2 : struct
            Hello, @typeof(T1) and @typeof(T2)!
            @NeedsStruct(default(T2))

            @functions {
                T NeedsStruct<T>(T value) where T : struct => value;
            }
            """
        );
    }

    [Test]
    public Task should_handle_typeparam_with_additional_library_code()
    {
        return Verify(
            """
            @typeparam T1
            @typeparam T2 where T2 : struct
            @inherits Foo.BaseClass<T2>
            """,
            """
            using RazorBlade.Support;

            namespace Foo;

            public abstract class BaseClass<T> : RazorBlade.HtmlTemplate<T>
                where T : struct
            {
                [TemplateConstructor]
                protected BaseClass(T value)
                {
                }
            }
            """
        );
    }

    [Test]
    public Task should_error_on_invalid_typeparam()
    {
        return Verify(
            """
            @typeparam TFoo TBar
            """,
            config: new()
            {
                ExpectErrors = true
            }
        );
    }

    [Test]
    public Task should_handle_attribute()
    {
        return Verify(
            """
            @using System
            @attribute [Obsolete("Hello!")]
            """
        );
    }

    [Test]
    public Task should_error_on_invalid_attribute()
    {
        return Verify(
            """
            @using System
            @attribute [Obsolete
            """,
            config: new()
            {
                ExpectDiagnostics = true
            }
        );
    }

    [Test]
    public Task should_handle_implements()
    {
        return Verify(
            """
            @using System
            @implements IDisposable
            @functions {
                public void Dispose() { }
            }
            """
        );
    }

    [Test]
    public Task should_support_known_directives()
    {
        var engine = new RazorBladeEngine(null);
        var feature = engine.RazorEngine.Engine.GetFeature<DefaultRazorDirectiveFeature>().ShouldNotBeNull();

        var directives = feature.DirectivesByFileKind[FileKinds.Legacy]
                                .Select(i => i.Directive)
                                .Order()
                                .ToList();

        return Verifier.Verify(directives);
    }

    private static GeneratorDriverRunResult Generate(string input,
                                                     string? csharpCode,
                                                     TestConfig config)
    {
        var metadataReferences = new List<MetadataReference>();

        if (config.NetStandard)
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

        var analyzerConfigOptionsProvider = new AnalyzerConfigOptionsProviderMock();

        foreach (var (key, value) in config.ConfigOptions)
            analyzerConfigOptionsProvider.Add(key, value);

        if (config.EmbeddedLibrary)
        {
            analyzerConfigOptionsProvider.Add(Constants.GlobalOptions.EmbeddedLibrary, bool.TrueString);
        }
        else
        {
            if (config.NetStandard)
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
                                          .AddAdditionalTexts([new AdditionalTextMock(input.ReplaceLineEndings("\r\n"), config.FilePath), ..config.AdditionalTexts])
                                          .WithUpdatedAnalyzerConfigOptions(analyzerConfigOptionsProvider)
                                          .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _)
                                          .GetRunResult();

        var diagnostics = updatedCompilation.GetDiagnostics();

        if (config.ExpectErrors)
        {
            result.Diagnostics.ShouldContain(i => i.Severity == DiagnosticSeverity.Error);
        }
        else
        {
            result.Diagnostics.ShouldBeEmpty();

            if (!config.EmbeddedLibrary) // Don't validate the embedded library generator here, assume the final output will compile.
            {
                if (!diagnostics.IsEmpty)
                    Console.WriteLine(result.GeneratedTrees.FirstOrDefault());

                if (!config.ExpectDiagnostics)
                    diagnostics.ShouldBeEmpty();
            }
        }

        return result;
    }

    [MustUseReturnValue]
    private static Task Verify([StringSyntax("razor")] string input,
                               [StringSyntax("csharp")] string? csharpCode = null,
                               TestConfig? config = null)
    {
        var result = Generate(input, csharpCode, config ?? new());
        return Verifier.Verify(result);
    }

    private static AdditionalTextMock RazorAdditionalText([StringSyntax("razor")] string text, string path)
        => new(text.ReplaceLineEndings("\r\n"), path);

    private class TestConfig
    {
        public Dictionary<string, string> ConfigOptions { get; } = new()
        {
            { Constants.FileOptions.IsRazorBlade, bool.TrueString },
            { Constants.FileOptions.HintNamespace, "TestNamespace" }
        };

        public bool EmbeddedLibrary { get; init; }
        public bool NetStandard { get; init; }
        public bool ExpectErrors { get; init; }
        public bool ExpectDiagnostics { get; init; }
        public string FilePath { get; init; } = "./TestFile.cshtml";
        public AdditionalTextMock[] AdditionalTexts { get; init; } = [];
    }
}
