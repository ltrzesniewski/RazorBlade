﻿//HintName: TestNamespace.TestFile.Razor.g.cs
#pragma checksum "./TestFile.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e3c68a27bd0267aae003e34da6acc594dabfdc58"
// <auto-generated/>
#pragma warning disable 1591
namespace TestNamespace
{
    #line hidden
#nullable restore
#line 1 "./TestFile.cshtml"
using System;

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
#line 2 "./TestFile.cshtml"
   Func<string, object> bold = 

#line default
#line hidden
#nullable disable
            item => new HelperResult(async(__razor_template_writer) => {
                PushWriter(__razor_template_writer);
                WriteLiteral("<b>");
#nullable restore
#line (2,37)-(2,41) 6 "./TestFile.cshtml"
Write(item);

#line default
#line hidden
#nullable disable
                WriteLiteral("</b>");
                PopWriter();
            }
            )
#nullable restore
#line 2 "./TestFile.cshtml"
                                            ; 

#line default
#line hidden
#nullable disable
#nullable restore
#line (3,2)-(3,36) 6 "./TestFile.cshtml"
Write(bold("Bold but not <i>italic</i>"));

#line default
#line hidden
#nullable disable
        }
        #pragma warning restore 1998
    }
}
#pragma warning restore 1591
