﻿//HintName: TestNamespace.TestFile.g.cs
#pragma checksum "./TestFile.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0f387a306a025bdecaddfb34c3e73317fb71c2ac"
// <auto-generated/>
#pragma warning disable 1591
namespace TestNamespace
{
    #line hidden
    #nullable restore
    internal partial class TestFile : Foo.BaseClass
    #nullable disable
    {
        #pragma warning disable 1998
        protected async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
        }
        #pragma warning restore 1998

        #nullable restore

        /// <inheritdoc cref="M:Foo.BaseClass.#ctor(System.Nullable{System.Int32},System.String)" />
        public TestFile(int? foo, string? bar)
            : base(foo, bar)
        {
        }

        /// <inheritdoc cref="M:Foo.BaseClass.#ctor(System.Single,System.String,System.DayOfWeek)" />
        public TestFile(float @double, string str = "foo\\\"bar", global::System.DayOfWeek day = global::System.DayOfWeek.Friday)
            : base(@double, str, day)
        {
        }

        /// <inheritdoc cref="M:Foo.BaseClass.#ctor(System.Int32@,System.Int32@,System.Int32@,System.Int32[])" />
        public TestFile(in int foo, ref int bar, out int baz, params int[] qux)
            : base(in foo, ref bar, out baz, qux)
        {
        }
    }
}
#pragma warning restore 1591