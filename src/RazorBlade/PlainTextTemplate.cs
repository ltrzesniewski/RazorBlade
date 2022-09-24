using System;
using RazorBlade.Support;

namespace RazorBlade;

/// <summary>
/// Base class for plaint text templates.
/// </summary>
/// <remarks>
/// Values will be written as-is, without escaping.
/// </remarks>
public abstract class PlainTextTemplate : RazorTemplate
{
    /// <inheritdoc />
    protected internal override void Write(object? value)
    {
        if (value is IEncodedContent encodedContent)
            encodedContent.WriteTo(Output);
        else
            Output.Write(value);
    }
}

/// <summary>
/// Base class for plaint text templates with a model.
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
