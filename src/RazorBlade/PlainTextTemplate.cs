using System;
using JetBrains.Annotations;
using RazorBlade.Support;

namespace RazorBlade;

/// <summary>
/// Base class for plain text templates.
/// </summary>
/// <remarks>
/// Values will be written as-is, without escaping.
/// </remarks>
public abstract class PlainTextTemplate : RazorTemplate
{
    private string? _currentAttributeSuffix;

    /// <inheritdoc />
    protected internal override void Write(object? value)
    {
        if (value is IEncodedContent encodedContent)
            encodedContent.WriteTo(Output);
        else
            Output.Write(value);
    }

    /// <inheritdoc />
    protected internal override void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
    {
        WriteLiteral(prefix);
        _currentAttributeSuffix = suffix;
    }

    /// <inheritdoc />
    protected internal override void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
    {
        WriteLiteral(prefix);

        if (isLiteral)
            WriteLiteral(value?.ToString());
        else
            Write(value);
    }

    /// <inheritdoc />
    protected internal override void EndWriteAttribute()
    {
        WriteLiteral(_currentAttributeSuffix);
        _currentAttributeSuffix = null;
    }
}

/// <summary>
/// Base class for plain text templates with a model.
/// </summary>
/// <remarks>
/// Values will be written as-is, without escaping.
/// </remarks>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class PlainTextTemplate<TModel> : PlainTextTemplate
{
    /// <summary>
    /// The model for the template.
    /// </summary>
    [UsedImplicitly]
    public TModel Model { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextTemplate{TModel}"/> class.
    /// </summary>
    /// <param name="model">The model for the template.</param>
    [TemplateConstructor]
    protected PlainTextTemplate(TModel model)
    {
        Model = model;
    }

    /// <summary>
    /// This constructor is provided for the designer only. Do not use.
    /// </summary>
    protected PlainTextTemplate()
    {
        throw new NotSupportedException("Use the constructor overload that takes a model.");
    }
}
