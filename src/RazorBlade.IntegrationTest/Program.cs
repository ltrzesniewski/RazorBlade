using System;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    public static void Main()
    {
        var template = new TestTemplate();
        template.ExecuteAsync().GetAwaiter().GetResult();
        Console.WriteLine(template.ToString());
    }
}
