using System;
using System.Diagnostics.CodeAnalysis;

namespace RazorBlade;

/// <summary>
/// Base class for HTML templates.
/// </summary>
/// <remarks>
/// Special HTML characters will be escaped.
/// </remarks>
public abstract class HtmlTemplate : RazorTemplate
{
    // ReSharper disable once RedundantDisableWarningComment
#pragma warning disable CA1822

    /// <inheritdoc cref="HtmlHelper"/>
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    protected HtmlHelper Html => HtmlHelper.Instance;

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
            var idx = valueSpan.IndexOfAny("&<>\"");
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
        );
#endif
    }
}
