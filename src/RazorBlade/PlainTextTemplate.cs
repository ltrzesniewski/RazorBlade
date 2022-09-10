namespace RazorBlade;

/// <summary>
/// Base class for plaint text templates.
/// </summary>
/// <remarks>
/// Values will be written as-is, without escaping.
/// </remarks>
public abstract class PlainTextTemplate : RazorTemplate
{
    /// <summary>
    /// Write a value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void Write(object? value)
        => Output.Write(value);
}
