﻿//HintName: TestNamespace.TestFile.g.cs
#pragma checksum "./TestFile.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e11bcd3837d8e9f6063b76eeb7ffc8e8d5fecd7c"
// <auto-generated/>
#pragma warning disable 1591
namespace TestNamespace
{
    #line hidden
#nullable restore
#line 1 "./TestFile.cshtml"
using System.Threading.Tasks;

#line default
#line hidden
#nullable disable
    #nullable restore
    internal partial class TestFile : global::RazorBlade.HtmlTemplate
    #nullable disable
    {
        #pragma warning disable 1998
        protected async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line (2,2)-(2,27) 6 "./TestFile.cshtml"
Write(await Task.FromResult(42));

#line default
#line hidden
#nullable disable
        }
        #pragma warning restore 1998

        #nullable restore

        /// <inheritdoc cref="M:RazorBlade.RazorTemplate.Render(System.Threading.CancellationToken)" />
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("The generated template is async. Use RenderAsync instead.", DiagnosticId = "RB0003")]
        public new string Render(global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            => base.Render(cancellationToken);

        /// <inheritdoc cref="M:RazorBlade.RazorTemplate.Render(System.IO.TextWriter,System.Threading.CancellationToken)" />
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("The generated template is async. Use RenderAsync instead.", DiagnosticId = "RB0003")]
        public new void Render(global::System.IO.TextWriter textWriter, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            => base.Render(textWriter, cancellationToken);
    }
}
#pragma warning restore 1591
