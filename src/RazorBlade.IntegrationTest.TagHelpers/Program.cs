using System;

namespace RazorBlade.IntegrationTest.TagHelpers;

public static class Program
{
    public static void Main()
    {
        var template = new TestTemplate();
        var result = template.RenderAsync().GetAwaiter().GetResult();

        Console.WriteLine(result);
    }
}
