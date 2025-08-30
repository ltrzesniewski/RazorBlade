using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace RazorBlade.Analyzers.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DiffRunner.Disabled = true;
        VerifySourceGenerators.Initialize();
        VerifyDiffPlex.Initialize();
    }
}
