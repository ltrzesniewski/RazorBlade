namespace RazorBlade;

/// <summary>
/// Base class for plaint text templates.
/// </summary>
/// <remarks>
/// Values will be written as-is, without escaping.
/// </remarks>
public abstract class PlainTextTemplate : RazorTemplate
{
    /// <inheritdoc />
    protected internal override void Write(object? value)
        => Output.Write(value);
}
