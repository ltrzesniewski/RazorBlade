using System;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    public static void Main()
    {
        var template = new TestTemplate { Name = "World" };
        var result = template.Render();

        Console.WriteLine(result);

        _ = new TestTemplateWithModel(new FooBarModelClass());
    }
}

public class FooBarModelClass
{
    public string? Foo { get; set; }
    public string? Bar { get; set; }
}
