using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RazorBlade.Support;

namespace RazorBlade;

/// <summary>
/// Base class for HTML templates.
/// </summary>
/// <remarks>
/// Special HTML characters will be escaped.
/// </remarks>
public abstract class HtmlTemplate : RazorTemplate
{
    private AttributeInfo _currentAttribute;

    // ReSharper disable once RedundantDisableWarningComment
#pragma warning disable CA1822

    /// <inheritdoc cref="HtmlHelper"/>
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    protected HtmlHelper Html => HtmlHelper.Instance;

    /// <inheritdoc cref="HtmlHelper.Raw"/>
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    protected internal HtmlString Raw(object? value)
        => HtmlHelper.Instance.Raw(value);

#pragma warning restore CA1822

    /// <inheritdoc />
    protected internal override void Write(object? value)
    {
        if (value is IEncodedContent encodedContent)
        {
            encodedContent.WriteTo(Output);
            return;
        }

        var valueString = value?.ToString();
        if (valueString is null or "")
            return;

#if NET6_0_OR_GREATER
        var valueSpan = valueString.AsSpan();

        while (true)
        {
            var idx = valueSpan.IndexOfAny("&<>\"\'");
            if (idx < 0)
                break;

            if (idx != 0)
                Output.Write(valueSpan[..idx]);

            Output.Write(valueSpan[idx] switch
            {
                '&'   => "&amp;",
                '<'   => "&lt;",
                '>'   => "&gt;",
                '"'   => "&quot;",
                '\''  => "&#x27;",
                var c => c.ToString() // Won't happen
            });

            valueSpan = valueSpan[(idx + 1)..];
        }

        if (valueSpan.Length != 0)
            Output.Write(valueSpan);
#else
        Output.Write(
            valueString.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("\'", "&#x27;")
        );
#endif
    }

    /// <inheritdoc />
    protected internal override void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
    {
        _currentAttribute = new(name, prefix, suffix, attributeValuesCount);

        if (_currentAttribute.AttributeValuesCount != 1)
            WriteLiteral(prefix);
    }

    /// <inheritdoc />
    protected internal override void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
    {
        // This implements the Razor semantics of ASP.NET (conditional attributes):

        // When an attribute consists of a single value part (without whitespace): foo="@bar"
        //  - if bar evaluates to false or null, omit the attribute entirely
        //  - if bar evaluates to true, write the attribute name as the value: foo="foo"
        //  - otherwise, write the value of bar as usual

        // When an attribute contains multiple value parts: class="foo @bar"
        //  - if bar evaluates to null, omit it and its whitespace prefix: class="foo"
        //  - otherwise, write the value of bar as usual (even if it evaluates to a boolean)

        // Note that if an attribute name starts with "data-", these attribute-specific methods are not called,
        // and Write is used instead, effectively bypassing these rules and always writing the attribute value as-is.

        if (_currentAttribute.AttributeValuesCount == 1)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                if (value is bool boolValue)
                    value = boolValue ? _currentAttribute.Name : null;

                if (value is null)
                {
                    _currentAttribute.Suppressed = true;
                    return;
                }
            }

            WriteLiteral(_currentAttribute.Prefix);
        }

        if (value is not null)
        {
            WriteLiteral(prefix);

            if (isLiteral)
                WriteLiteral(value.ToString());
            else
                Write(value);
        }
    }

    /// <inheritdoc />
    protected internal override void EndWriteAttribute()
    {
        if (!_currentAttribute.Suppressed)
            WriteLiteral(_currentAttribute.Suffix);
    }

    private struct AttributeInfo
    {
        public readonly string? Name;
        public readonly string? Prefix;
        public readonly string? Suffix;
        public readonly int AttributeValuesCount;
        public bool Suppressed;

        public AttributeInfo(string name, string prefix, string suffix, int attributeValuesCount)
        {
            Name = name;
            Prefix = prefix;
            Suffix = suffix;
            AttributeValuesCount = attributeValuesCount;

            Suppressed = false;
        }
    }
}

/// <summary>
/// Base class for HTML templates with a model.
/// </summary>
/// <remarks>
/// Special HTML characters will be escaped.
/// </remarks>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class HtmlTemplate<TModel> : HtmlTemplate
{
    /// <summary>
    /// The model for the template.
    /// </summary>
    [UsedImplicitly]
    public TModel Model { get; }

    /// <summary>
    /// Initializes a new instance of the template.
    /// </summary>
    /// <param name="model">The model for the template.</param>
    [TemplateConstructor]
    protected HtmlTemplate(TModel model)
    {
        Model = model;
    }

    /// <summary>
    /// This constructor is provided for the designer only. Do not use.
    /// </summary>
    protected HtmlTemplate()
    {
        throw new NotSupportedException("Use the constructor overload that takes a model.");
    }
}
