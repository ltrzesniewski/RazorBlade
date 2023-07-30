using System;
using System.IO;
using System.Text.Encodings.Web;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorBlade;

public sealed class TagHelperHandler
{
    private readonly ITagHelperTemplate _template;
    private StringWriter? _valueBuffer;
    private TagHelperAttributeInfo _tagHelperAttributeInfo;

    public TagHelperHandler(ITagHelperTemplate template)
    {
        _template = template;
    }

    [PublicAPI]
    public TTagHelper CreateTagHelper<TTagHelper>()
        where TTagHelper : ITagHelper
    {
        return Activator.CreateInstance<TTagHelper>();
    }

    [PublicAPI]
    public void StartTagHelperWritingScope(HtmlEncoder? encoder)
    {
        _template.PushWriter(new StringWriter());
    }

    [PublicAPI]
    public TagHelperContent EndTagHelperWritingScope()
    {
        var content = (StringWriter)_template.PopWriter();

        var tagHelperContent = new DefaultTagHelperContent();
        return tagHelperContent.AppendHtml(content.ToString());
    }

    [PublicAPI]
    public void BeginAddHtmlAttributeValues(TagHelperExecutionContext executionContext,
                                            string attributeName,
                                            int attributeValuesCount,
                                            HtmlAttributeValueStyle attributeValueStyle)
    {
        _tagHelperAttributeInfo = new TagHelperAttributeInfo(
            executionContext,
            attributeName,
            attributeValuesCount,
            attributeValueStyle
        );
    }

    [PublicAPI]
    public void EndAddHtmlAttributeValues(TagHelperExecutionContext executionContext)
    {
        if (!_tagHelperAttributeInfo.Suppressed)
        {
            var content = _valueBuffer == null
                ? Microsoft.AspNetCore.Html.HtmlString.Empty
                : new Microsoft.AspNetCore.Html.HtmlString(_valueBuffer.ToString());

            _valueBuffer?.GetStringBuilder().Clear();

            executionContext.AddHtmlAttribute(_tagHelperAttributeInfo.Name, content, _tagHelperAttributeInfo.AttributeValueStyle);
        }
    }

    [PublicAPI]
    public void BeginWriteTagHelperAttribute()
    {
        _template.PushWriter(_valueBuffer ??= new());
    }

    [PublicAPI]
    public string EndWriteTagHelperAttribute()
    {
        var writer = _template.PopWriter();

        if (!ReferenceEquals(writer, _valueBuffer))
            throw new InvalidOperationException("Invalid operation order.");

        var content = _valueBuffer.ToString();
        _valueBuffer.GetStringBuilder().Clear();

        return content;
    }

    [PublicAPI]
    public string InvalidTagHelperIndexerAssignment(string attributeName, string tagHelperTypeName, string propertyName)
        => $"Unable to perform '{attributeName}' assignment. Tag helper property '{tagHelperTypeName}.{propertyName}' must not be null.";

    [PublicAPI]
    public IHtmlContent HtmlRaw(object? value)
        => new Microsoft.AspNetCore.Html.HtmlString(value?.ToString());

    [PublicAPI]
    public void Write(object? value)
    {
        if (value is IHtmlContent htmlContent)
            value = new HtmlContentAdapter(htmlContent);

        _template.Write(value);
    }

    private struct TagHelperAttributeInfo
    {
        public TagHelperAttributeInfo(
            TagHelperExecutionContext tagHelperExecutionContext,
            string name,
            int attributeValuesCount,
            HtmlAttributeValueStyle attributeValueStyle)
        {
            ExecutionContext = tagHelperExecutionContext;
            Name = name;
            AttributeValuesCount = attributeValuesCount;
            AttributeValueStyle = attributeValueStyle;

            Suppressed = false;
        }

        public string Name { get; }
        public TagHelperExecutionContext ExecutionContext { get; }
        public int AttributeValuesCount { get; }
        public HtmlAttributeValueStyle AttributeValueStyle { get; }

        public bool Suppressed { get; set; } // TODO support attribute suppression
    }

    private class HtmlContentAdapter : IEncodedContent
    {
        private readonly IHtmlContent _htmlContent;

        public HtmlContentAdapter(IHtmlContent htmlContent)
            => _htmlContent = htmlContent;

        public void WriteTo(TextWriter textWriter)
            => _htmlContent.WriteTo(textWriter, HtmlEncoder.Default);
    }
}

public interface ITagHelperTemplate
{
    void Write(object? value);
    void PushWriter(TextWriter writer);
    TextWriter PopWriter();
}
