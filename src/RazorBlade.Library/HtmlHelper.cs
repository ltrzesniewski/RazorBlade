using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RazorBlade;

// ReSharper disable once RedundantDisableWarningComment
#pragma warning disable CA1822

/// <summary>
/// Utilities for HTML Razor templates.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class HtmlHelper
{
#if NET8_0_OR_GREATER
    private static readonly System.Buffers.SearchValues<char> _charsToEscape = System.Buffers.SearchValues.Create("&<>\"\'");
#elif NET6_0_OR_GREATER
    private const string _charsToEscape = "&<>\"\'";
#endif

    internal static HtmlHelper Instance { get; } = new();

    /// <summary>
    /// Returns markup that is not HTML encoded.
    /// </summary>
    /// <param name="value">The HTML markup.</param>
    public HtmlString Raw(object? value)
        => new(value?.ToString());

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
        var sb = default(StringBuilder);

        while (true)
        {
            var idx = valueSpan.IndexOfAny(_charsToEscape);
            if (idx < 0)
                break;

            sb ??= new StringBuilder();

            if (idx != 0)
                sb.Append(valueSpan[..idx]);

            sb.Append(valueSpan[idx] switch
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

        if (sb is null)
            return valueString;

        if (valueSpan.Length != 0)
            sb.Append(valueSpan);

        return sb.ToString();
#else
        return valueString.Replace("&", "&amp;")
                          .Replace("<", "&lt;")
                          .Replace(">", "&gt;")
                          .Replace("\"", "&quot;")
                          .Replace("\'", "&#x27;");
#endif
    }
}
