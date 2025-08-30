using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace RazorBlade.Library.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DiffRunner.Disabled = true;
        VerifyDiffPlex.Initialize();
    }
}
