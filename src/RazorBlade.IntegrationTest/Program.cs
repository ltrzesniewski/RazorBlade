using System;

namespace RazorBlade.IntegrationTest;

public static class Program
{
    public static void Main()
    {
        var template = new TestTemplate { Name = "World" };
        template.ExecuteAsync().GetAwaiter().GetResult();
        Console.WriteLine(template.ToString());
    }
}
