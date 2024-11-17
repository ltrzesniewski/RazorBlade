using System.IO;

namespace RazorBlade;

/// <summary>
/// Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public sealed class HtmlString : IEncodedContent
{
    private readonly string _value;

    /// <summary>
    /// Represents an empty HTML-encoded string.
    /// </summary>
    public static HtmlString Empty { get; } = new(string.Empty);

    /// <summary>
    /// Creates an HTML-encoded string.
    /// </summary>
    public HtmlString(string? value)
        => _value = value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString()
        => _value;

    void IEncodedContent.WriteTo(TextWriter textWriter)
        => textWriter.Write(_value);
}
