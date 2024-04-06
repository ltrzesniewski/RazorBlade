using System;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    public static void Main()
    {
        WriteSeparator();
        WriteTemplate(new TestTemplate { Name = "World" });
        WriteTemplate(new TestTemplateWithModel(new FooBarModelClass { Foo = "Foo", Bar = "Bar" }));
        WriteTemplate(new PageWithLayout());
        WriteTemplate(new PageWithFlush());
    }

    private static void WriteTemplate(RazorTemplate template)
    {
        Console.WriteLine(template.Render());
        WriteSeparator();
    }

    private static void WriteSeparator()
        => Console.WriteLine("--------------------------------------------------");
}

public class FooBarModelClass
{
    public string? Foo { get; set; }
    public string? Bar { get; set; }
}
