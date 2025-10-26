using System;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using NUnit.Framework;
using RazorBlade.Analyzers.Tests.Support;

namespace RazorBlade.Analyzers.Tests;

[TestFixture]
public class BuildTimeTemplateRunnerTests
{
    [Test]
    public void should_debug()
    {
        var input =
            // language=razor
            """
            @inherits RazorBlade.CSharpGenerator

            public static class BuildTimeGeneratedClass
            {
            @for (var i = 0; i < 5; i++)
            {
                @:public static string Foo@(i) => "Bar@(i)";
            }
            }

            """;

        var engine = new RazorBladeEngine(null);
        var codeDocument = engine.Process(InputFile.Create(new AdditionalTextMock(input, "input.cshtml"), null), []);
        var csharpDocument = codeDocument.GetCSharpDocument();

        Console.WriteLine(csharpDocument.GeneratedCode);

        var output = BuildTimeTemplateRunner.TryGenerate(codeDocument, CancellationToken.None);

        Console.WriteLine("-----");
        Console.WriteLine(output);
    }
}
