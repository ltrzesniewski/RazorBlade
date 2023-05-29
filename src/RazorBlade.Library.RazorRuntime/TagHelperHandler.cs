using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorBlade;

public sealed class TagHelperHandler
{
    private readonly ITagHelperTemplate _template;

    public TagHelperHandler(ITagHelperTemplate template)
    {
        _template = template;
    }

    public TTagHelper CreateTagHelper<TTagHelper>()
        where TTagHelper : ITagHelper
    {
        return Activator.CreateInstance<TTagHelper>();
    }

    public void StartTagHelperWritingScope(HtmlEncoder encoder)
    {
        throw new NotImplementedException();
    }

    public TagHelperContent EndTagHelperWritingScope()
    {
        throw new NotImplementedException();
    }

    public void BeginAddHtmlAttributeValues(TagHelperExecutionContext executionContext,
                                            string attributeName,
                                            int attributeValuesCount,
                                            HtmlAttributeValueStyle attributeValueStyle)
    {
        throw new NotImplementedException();
    }

    public void EndAddHtmlAttributeValues(TagHelperExecutionContext executionContext)
    {
        throw new NotImplementedException();
    }

    public void BeginWriteTagHelperAttribute()
    {
        throw new NotImplementedException();
    }

    public string EndWriteTagHelperAttribute()
    {
        throw new NotImplementedException();
    }

    public string InvalidTagHelperIndexerAssignment(string attributeName,
                                                    string tagHelperTypeName,
                                                    string propertyName)
    {
        throw new NotImplementedException();
    }

    public IHtmlContent HtmlRaw(object? value)
    {
        throw new NotImplementedException();
    }

    public void Write(object? value)
    {
        throw new NotImplementedException();
    }
}

public interface ITagHelperTemplate
{
}
