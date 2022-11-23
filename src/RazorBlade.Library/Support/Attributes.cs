using System;
using JetBrains.Annotations;

namespace RazorBlade.Support;

/// <summary>
/// Specifies that this constructor needs to be provided by the generated template class.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class TemplateConstructorAttribute : Attribute
{
}

/// <summary>
/// Specifies if a method should be used depending on the template being sync or async.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class ConditionalOnAsyncAttribute : Attribute
{
    /// <summary>
    /// The message to display.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Marks a method as meant to be used in a sync or async template.
    /// </summary>
    /// <param name="async">True for methods meant to be used in async templates, and false for methods meant to be used for sync templates.</param>
    public ConditionalOnAsyncAttribute(bool async)
    {
    }
}
