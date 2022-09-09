namespace RazorBlade;

public abstract class PlainTextTemplate : RazorTemplate
{
    protected void WriteLiteral(string? value)
        => Output.Write(value);

    protected void Write(object? value)
        => Output.Write(value);
}
