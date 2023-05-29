using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace RazorBlade.Analyzers;

internal class RazorBladeTagHelperTargetExtension : IDefaultTagHelperTargetExtension
{
    private const string _tagHelperHandlerTypeName = "global::RazorBlade.TagHelperHandler";
    private const string _tagHelperHandlerProperty = "__tagHelperHandler";

    private static readonly string[] _fieldUnintializedModifiers = { "0649" };
    private static readonly string[] _privateModifiers = { "private" };

    private readonly DefaultTagHelperTargetExtension _inner;

    public RazorBladeTagHelperTargetExtension()
    {
        _inner = new DefaultTagHelperTargetExtension
        {
            CreateTagHelperMethodName = $"{_tagHelperHandlerProperty}.CreateTagHelper",
            StartTagHelperWritingScopeMethodName = $"{_tagHelperHandlerProperty}.StartTagHelperWritingScope",
            EndTagHelperWritingScopeMethodName = $"{_tagHelperHandlerProperty}.EndTagHelperWritingScope",
            BeginAddHtmlAttributeValuesMethodName = $"{_tagHelperHandlerProperty}.BeginAddHtmlAttributeValues",
            EndAddHtmlAttributeValuesMethodName = $"{_tagHelperHandlerProperty}.EndAddHtmlAttributeValues",
            BeginWriteTagHelperAttributeMethodName = $"{_tagHelperHandlerProperty}.BeginWriteTagHelperAttribute",
            EndWriteTagHelperAttributeMethodName = $"{_tagHelperHandlerProperty}.EndWriteTagHelperAttribute",
            MarkAsHtmlEncodedMethodName = $"{_tagHelperHandlerProperty}.HtmlRaw",
            FormatInvalidIndexerAssignmentMethodName = $"{_tagHelperHandlerProperty}.InvalidTagHelperIndexerAssignment",
            WriteTagHelperOutputMethod = $"{_tagHelperHandlerProperty}.Write"
        };
    }

    public void WriteTagHelperBody(CodeRenderingContext context, DefaultTagHelperBodyIntermediateNode node)
    {
        _inner.WriteTagHelperBody(context, node);
    }

    public void WriteTagHelperCreate(CodeRenderingContext context, DefaultTagHelperCreateIntermediateNode node)
    {
        _inner.WriteTagHelperCreate(context, node);
    }

    public void WriteTagHelperExecute(CodeRenderingContext context, DefaultTagHelperExecuteIntermediateNode node)
    {
        _inner.WriteTagHelperExecute(context, node);
    }

    public void WriteTagHelperHtmlAttribute(CodeRenderingContext context, DefaultTagHelperHtmlAttributeIntermediateNode node)
    {
        _inner.WriteTagHelperHtmlAttribute(context, node);
    }

    public void WriteTagHelperProperty(CodeRenderingContext context, DefaultTagHelperPropertyIntermediateNode node)
    {
        _inner.WriteTagHelperProperty(context, node);
    }

    public void WriteTagHelperRuntime(CodeRenderingContext context, DefaultTagHelperRuntimeIntermediateNode node)
    {
        _inner.WriteTagHelperRuntime(context, node);

        const string fieldName = "__backed" + _tagHelperHandlerProperty;

        context.CodeWriter.WriteField(_fieldUnintializedModifiers, _privateModifiers, _tagHelperHandlerTypeName, fieldName);

        context.CodeWriter
               .Write("private ")
               .Write(_tagHelperHandlerTypeName)
               .Write(" ")
               .Write(_tagHelperHandlerProperty)
               .Write(" => ")
               .Write(fieldName)
               .Write(" ??= new ")
               .Write(_tagHelperHandlerTypeName)
               .WriteLine("(this);");
    }
}
