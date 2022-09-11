using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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

    /// <summary>
    /// HTML-encodes the provided value.
    /// </summary>
    /// <param name="value">Value to HTML-encode.</param>
    public string Encode(object? value)
    {
        var valueString = value?.ToString();
        if (valueString is null or "")
            return string.Empty;

#if NET6_0_OR_GREATER
        var valueSpan = valueString.AsSpan();
        var sb = new StringBuilder();

        while (true)
        {
            var idx = valueSpan.IndexOfAny('&', '<', '>');
            if (idx < 0)
                break;

            if (idx != 0)
                sb.Append(valueSpan[..idx]);

            sb.Append(valueSpan[idx] switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                _   => throw new InvalidOperationException()
            });

            valueSpan = valueSpan[(idx + 1)..];
        }

        if (valueSpan.Length != 0)
            sb.Append(valueSpan);

        return sb.ToString();
#else
        return valueString.Replace("&", "&amp;")
                          .Replace("<", "&lt;")
                          .Replace(">", "&gt;");
#endif
    }
}
