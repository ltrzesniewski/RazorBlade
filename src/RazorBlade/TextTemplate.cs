namespace RazorBlade;

public abstract class TextTemplate : RazorTemplate
{
    protected void WriteLiteral(string? value)
        => Output.Write(value);

    protected void Write(object? value)
        => Output.Write(value);
}
