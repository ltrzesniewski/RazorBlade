using System.Diagnostics.CodeAnalysis;

namespace RazorBlade;

#nullable enable

// ReSharper disable once RedundantDisableWarningComment
#pragma warning disable CA1822

/// <summary>
/// Utilities for HTML Razor templates.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class HtmlHelper
{
    internal static HtmlHelper Instance { get; } = new();

    /// <summary>
    /// Returns markup that is not HTML encoded.
    /// </summary>
    /// <param name="value">The HTML markup.</param>
    public IHtmlString Raw(object? value)
        => new HtmlString(value?.ToString());
}
