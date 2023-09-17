using System.Runtime.CompilerServices;
using VerifyTests;

namespace RazorBlade.Analyzers.Tests;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
        VerifyDiffPlex.Initialize();
    }
}
