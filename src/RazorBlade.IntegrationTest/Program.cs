using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    private const string _reset = "\e[0m";
    private const string _bold = "\e[1m";
    private const string _red = "\e[91m";
    private const string _green = "\e[92m";

    private static bool _success;

    public static int Main(string[] args)
    {
        _success = true;

        CheckTemplates(args.Contains("--write"));
        CheckNamespaces();
        CheckAccessibility();

        if (args.Contains("--aot"))
            CheckAot();

        Console.WriteLine();
        Console.WriteLine($"{_bold}Integration tests: {(_success ? $"{_green}PASSED" : $"{_red}FAILED")}{_reset}");
        Console.WriteLine();

        return _success ? 0 : 1;
    }

    private static void CheckTemplates(bool write)
    {
        Header("Templates");

        CheckTemplate(new TestTemplate { Name = "World" });
        CheckTemplate(new TestTemplateWithModel(new FooBarModelClass { Foo = "Foo", Bar = "Bar" }));
        CheckTemplate(new TestGenericTemplate<string, int>("Hello"));
        CheckTemplate(new PageWithLayout());
        CheckTemplate(new PageWithFlush());

        CheckTemplate(new Imports.ImportA());
        CheckTemplate(new Imports.Inner.ImportB());
        CheckTemplate(new NamespaceOfImportC.ImportC());

        CheckTemplate(new Examples.TemplateWithPartials());

        return;

        void CheckTemplate(RazorTemplate template, [CallerArgumentExpression(nameof(template))] string? code = null)
        {
            try
            {
                var result = template.Render();

                if (write)
                {
                    Console.WriteLine();
                    Console.WriteLine($"""{_bold}/----- {code} -----\{_reset}""");
                    Console.WriteLine(result);
                    Console.WriteLine($"""{_bold}\{new string('-', (code?.Length ?? 0) + 2 * 6)}/{_reset}""");
                    Console.WriteLine();
                }

                Pass(code);
            }
            catch (Exception ex)
            {
                Fail($"{code}: {ex.Message}");
            }
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    private static void CheckNamespaces()
    {
        // These will fail the build when incorrect

        _ = typeof(global::RazorBlade.IntegrationTest.TestTemplate);
        _ = typeof(global::RazorBlade.IntegrationTest.Examples.ExampleTemplate);
        _ = typeof(global::FooBar.OtherNamespace.TestTemplateWithNamespace);
    }

    private static void CheckAccessibility()
    {
        Header("Accessibility");

        Check(typeof(TestTemplate).IsNotPublic);
        Check(typeof(PublicTemplate).IsPublic);
    }

    private static void CheckAot()
    {
        Header("AOT");

        Check(!RuntimeFeature.IsDynamicCodeSupported);
        Check(string.IsNullOrEmpty(GetAssemblyLocation()));

        return;

        [UnconditionalSuppressMessage("SingleFile", "IL3000")]
        static string GetAssemblyLocation()
            => typeof(Program).Assembly.Location;
    }

    private static void Header(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"{_bold}{title}{_reset}");
    }

    private static void Check(bool success, [CallerArgumentExpression(nameof(success))] string? code = null)
    {
        if (success)
            Pass(code);
        else
            Fail(code);
    }

    private static void Pass(string? message)
    {
        Console.WriteLine($"  {_green}PASSED:{_reset} {message}");
    }

    private static void Fail(string? message)
    {
        Console.WriteLine($"  {_red}FAILED:{_reset} {message}");
        _success = false;
    }
}

public class FooBarModelClass
{
    public string? Foo { get; set; }
    public string? Bar { get; set; }
}
