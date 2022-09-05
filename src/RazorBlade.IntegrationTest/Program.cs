using System;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    public static void Main()
    {
        var template = new Razor.Template();
        template.ExecuteAsync().GetAwaiter().GetResult();
        Console.WriteLine(template.ToString());
    }
}
