using System;
using System.IO;
using System.Text;

namespace RazorBlade;

#pragma warning disable CA1822

/// <summary>
/// Utilities for HTML Razor templates.
/// </summary>
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
                var c => c.ToString() // Unreachable
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

    /// <summary>
    /// HTML-encodes the provided value to the writer.
    /// </summary>
    /// <param name="value">Value to HTML-encode.</param>
    /// <param name="writer">Destination writer.</param>
    internal static void Encode(object? value, TextWriter writer)
    {
        var valueString = value?.ToString();
        if (valueString is null or "")
            return;

#if NET6_0_OR_GREATER
        var valueSpan = valueString.AsSpan();

        while (true)
        {
            var idx = valueSpan.IndexOfAny(_charsToEscape);
            if (idx < 0)
                break;

            if (idx != 0)
                writer.Write(valueSpan[..idx]);

            writer.Write(valueSpan[idx] switch
            {
                '&'   => "&amp;",
                '<'   => "&lt;",
                '>'   => "&gt;",
                '"'   => "&quot;",
                '\''  => "&#x27;",
                var c => c.ToString() // Unreachable
            });

            valueSpan = valueSpan[(idx + 1)..];
        }

        if (valueSpan.Length != 0)
            writer.Write(valueSpan);
#else
        writer.Write(Instance.Encode(valueString));
#endif
    }
}
