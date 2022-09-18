﻿using System.IO;
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
    protected internal TextWriter Output { get; internal set; } = new StreamWriter(Stream.Null);

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
    /// Renders the template synchronously to the given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
    /// <remarks>
    /// Use this only if the template does not use <c>@async</c> directives.
    /// </remarks>
    public void Render(TextWriter textWriter)
    {
        var renderTask = RenderAsync(textWriter);
        if (renderTask.IsCompleted)
        {
            renderTask.GetAwaiter().GetResult();
            return;
        }

        Task.Run(async () => await renderTask.ConfigureAwait(false)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Renders the template asynchronously and returns the result as a string.
    /// </summary>
    /// <remarks>
    /// Use this if the template uses <c>@async</c> directives.
    /// </remarks>
    public async Task<string> RenderAsync()
    {
        var output = new StringWriter();
        await RenderAsync(output).ConfigureAwait(false);
        return output.ToString();
    }

    /// <summary>
    /// Renders the template asynchronously to the given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
    /// <remarks>
    /// Use this if the template uses <c>@async</c> directives.
    /// </remarks>
    public async Task RenderAsync(TextWriter textWriter)
    {
        var previousOutput = Output;

        try
        {
            Output = textWriter;
            await ExecuteAsync().ConfigureAwait(false);
        }
        finally
        {
            Output = previousOutput;
        }
    }

    /// <summary>
    /// Executes the template and appends the result to <see cref="Output"/>.
    /// </summary>
    protected internal virtual Task ExecuteAsync()
        => Task.CompletedTask; // The IDE complains when this method is abstract :(

    /// <summary>
    /// Writes a literal value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void WriteLiteral(string? value)
        => Output.Write(value);

    /// <summary>
    /// Write a value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal abstract void Write(object? value);

    /// <summary>
    /// Render another template to the output.
    /// </summary>
    /// <param name="template">The template to render.</param>
    protected internal void Write(RazorTemplate? template)
        => template?.Render(Output);
}
