using System;

namespace RazorBlade.Support;

/// <summary>
/// Specifies that this constructor needs to be provided by the generated template class.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class TemplateConstructorAttribute : Attribute
{
}
