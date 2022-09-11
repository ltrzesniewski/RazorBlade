namespace RazorBlade;

#nullable enable

/// <summary>
/// Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public interface IHtmlString
{
    /// <summary>
    /// Returns an HTML-encoded string.
    /// </summary>
    public string ToHtmlString();
}

/// <inheritdoc cref="RazorBlade.IHtmlString"/>
public class HtmlString : IHtmlString
{
    private readonly string _value;

    /// <summary>
    /// Creates a HTML-encoded string.
    /// </summary>
    public HtmlString(string? value)
        => _value = value ?? string.Empty;

    /// <inheritdoc cref="IHtmlString.ToHtmlString"/>
    public string ToHtmlString()
        => _value;

    /// <inheritdoc />
    public override string ToString()
        => _value;
}
