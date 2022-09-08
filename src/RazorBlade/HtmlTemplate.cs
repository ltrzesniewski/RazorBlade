using System;

namespace RazorBlade;

public abstract class HtmlTemplate : RazorTemplate
{
    protected void WriteLiteral(string? value)
        => Output.Write(value);

    protected void Write(object? value)
    {
        var valueString = value?.ToString();
        if (valueString is null or "")
            return;

#if NET6_0_OR_GREATER
        var valueSpan = valueString.AsSpan();

        while (true)
        {
            var idx = valueSpan.IndexOfAny('&', '<', '>');
            if (idx < 0)
                break;

            if (idx != 0)
                Output.Write(valueSpan[..idx]);

            Output.Write(valueSpan[idx] switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                _   => throw new InvalidOperationException()
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
        );
#endif
    }
}
