using System.IO;
using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// Base class for Razor templates.
/// </summary>
/// <remarks>
/// This class lacks <c>Write</c> methods. Use one of the derived classes in your templates.
/// </remarks>
public abstract class RazorTemplate
{
    /// <summary>
    /// The <see cref="TextWriter"/> which receives the output.
    /// </summary>
    public TextWriter Output { get; set; } = new StringWriter();

    /// <summary>
    /// Renders the template synchronously and returns the result as a string.
    /// </summary>
    /// <remarks>
    /// Use this only if the template does not use <c>@async</c> directives.
    /// </remarks>
    public string Render()
    {
        var renderTask = RenderAsync();
        if (renderTask.IsCompleted)
            return renderTask.Result;

        return Task.Run(async () => await renderTask.ConfigureAwait(false)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Renders the template asynchronously and returns the result as a string.
    /// </summary>
    /// <remarks>
    /// Use this if the template uses <c>@async</c> directives.
    /// </remarks>
    public async Task<string> RenderAsync()
    {
        var previousOutput = Output;

        try
        {
            var output = new StringWriter();
            Output = output;
            await ExecuteAsync().ConfigureAwait(false);
            return output.ToString();
        }
        finally
        {
            Output = previousOutput;
        }
    }

    /// <summary>
    /// Executes the template and appends the result to <see cref="Output"/>.
    /// </summary>
    public virtual Task ExecuteAsync()
        => Task.CompletedTask; // The IDE complains when this method is abstract :(

    /// <summary>
    /// Returns <see cref="Output"/> as a string.
    /// </summary>
    public override string ToString()
        => Output.ToString() ?? string.Empty;

    /// <summary>
    /// Writes a literal value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void WriteLiteral(string? value)
        => Output.Write(value);
}
