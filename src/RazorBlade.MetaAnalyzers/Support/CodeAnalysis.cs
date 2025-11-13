using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis;

[Embedded]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class StringSyntaxAttribute : Attribute
{
    public StringSyntaxAttribute(string syntax)
    {
    }
}
