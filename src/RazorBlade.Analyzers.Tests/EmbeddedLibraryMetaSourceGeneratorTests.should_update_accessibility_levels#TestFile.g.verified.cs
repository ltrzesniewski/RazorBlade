﻿//HintName: TestFile.g.cs
// <auto-generated />

namespace RazorBlade.Analyzers;

internal static partial class EmbeddedLibrary
{
    //language=csharp
    private static string TestFile => """
        // This file is part of the RazorBlade library.
        
        #nullable enable
        
        internal abstract class TestClass
        {
            class InnerClass { }
            public class InnerPublicClass { }
            private class InnerPrivateClass { }
        
            public delegate void InnerDelegate();
        
            protected Method() { }
            protected string Property { get; set; }
        }
        
        class TestClass2 { }
        
        internal readonly struct TestStruct { }
        internal interface TestInterface { }
        internal record TestRecordClass { }
        internal record struct TestRecordStruct { }
        internal enum TestEnum { }
        internal delegate void TestDelegate();
        """;
}
