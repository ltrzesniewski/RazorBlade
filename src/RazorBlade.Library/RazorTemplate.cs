using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RazorBlade.Support;

namespace RazorBlade;

/// <summary>
/// Base class for Razor templates.
/// </summary>
public abstract class RazorTemplate : IEncodedContent
{
    private Dictionary<string, Func<Task>>? _sections;

    private Dictionary<string, Func<Task>> Sections => _sections ??= new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The <see cref="TextWriter"/> which receives the output.
    /// </summary>
    protected internal TextWriter Output { get; internal set; } = new StreamWriter(Stream.Null);

    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected internal CancellationToken CancellationToken { get; private set; }

    /// <summary>
    /// The layout to use.
    /// </summary>
    protected internal IRazorLayout? Layout { get; set; }

    /// <summary>
    /// Renders the template synchronously and returns the result as a string.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Use this only if the template does not use <c>@async</c> directives.
    /// </remarks>
    [ConditionalOnAsync(false, Message = $"The generated template is async. Use {nameof(RenderAsync)} instead.")]
    public string Render(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var renderTask = RenderAsyncCore(cancellationToken);
        if (renderTask.IsCompleted)
            return renderTask.GetAwaiter().GetResult().ToString();

        return Task.Run(async () => await renderTask.ConfigureAwait(false), CancellationToken.None).GetAwaiter().GetResult().ToString();
    }

    /// <summary>
    /// Renders the template synchronously to the given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Use this only if the template does not use <c>@async</c> directives.
    /// </remarks>
    [ConditionalOnAsync(false, Message = $"The generated template is async. Use {nameof(RenderAsync)} instead.")]
    public void Render(TextWriter textWriter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var renderTask = RenderAsync(textWriter, cancellationToken);
        if (renderTask.IsCompleted)
        {
            renderTask.GetAwaiter().GetResult();
            return;
        }

        Task.Run(async () => await renderTask.ConfigureAwait(false), CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Renders the template asynchronously and returns the result as a string.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Use this if the template uses <c>@async</c> directives.
    /// </remarks>
    public async Task<string> RenderAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var stringBuilder = await RenderAsyncCore(cancellationToken).ConfigureAwait(false);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Renders the template asynchronously to the given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Use this if the template uses <c>@async</c> directives.
    /// </remarks>
    public async Task RenderAsync(TextWriter textWriter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var stringBuilder = await RenderAsyncCore(cancellationToken).ConfigureAwait(false);

#if NET6_0_OR_GREATER
        await textWriter.WriteAsync(stringBuilder, cancellationToken).ConfigureAwait(false);
#else
        await textWriter.WriteAsync(stringBuilder.ToString()).ConfigureAwait(false);
#endif
    }

    private async Task<StringBuilder> RenderAsyncCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var executionResult = await ExecuteAsyncCore(cancellationToken);

        while (executionResult.Layout is { } layout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            executionResult = await layout.ExecuteLayoutAsync(executionResult).ConfigureAwait(false);
        }

        if (executionResult.Body is StringBuilderEncodedContent { StringBuilder: var outputStringBuilder })
            return outputStringBuilder;

        var outputStringWriter = new StringWriter();
        executionResult.Body.WriteTo(outputStringWriter);
        return outputStringWriter.GetStringBuilder();
    }

    private protected async Task<IRazorLayout.IExecutionResult> ExecuteAsyncCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var executionScope = new ExecutionScope(this, cancellationToken);
        await ExecuteAsync().ConfigureAwait(false);
        return new ExecutionResult(this, executionScope.Output);
    }

    /// <summary>
    /// Executes the template and appends the result to <see cref="Output"/>.
    /// </summary>
    [PublicAPI]
    protected internal virtual Task ExecuteAsync()
        => Task.CompletedTask; // The IDE complains when this method is abstract :(

    /// <summary>
    /// Writes a literal value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    [PublicAPI]
    protected internal virtual void WriteLiteral(string? value)
        => Output.Write(value);

    /// <summary>
    /// Write a value to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    [PublicAPI]
    protected internal abstract void Write(object? value);

    /// <summary>
    /// Write already encoded content to the output.
    /// </summary>
    /// <param name="content">The template to render.</param>
    protected internal virtual void Write(IEncodedContent? content)
        => content?.WriteTo(Output);

    /// <summary>
    /// Begins writing an attribute.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="prefix">The attribute prefix, which is the text from the whitespace preceding the attribute name to the quote before the attribute value.</param>
    /// <param name="prefixOffset">The prefix offset in the Razor file.</param>
    /// <param name="suffix">The suffix, consisting of the end quote.</param>
    /// <param name="suffixOffset">The suffix offset in the Razor file.</param>
    /// <param name="attributeValuesCount">The count of attribute value parts, which is the count of subsequent <see cref="WriteAttributeValue"/> calls.</param>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal abstract void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);

    /// <summary>
    /// Writes part of an attribute value.
    /// </summary>
    /// <param name="prefix">The value prefix, consisting of the whitespace preceding the value.</param>
    /// <param name="prefixOffset">The prefix offset in the Razor file.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="valueOffset">The value offset in the Razor file.</param>
    /// <param name="valueLength">The value length in the Razor file.</param>
    /// <param name="isLiteral">Whether the value is a literal.</param>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal abstract void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral);

    /// <summary>
    /// Ends writing an attribute.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal abstract void EndWriteAttribute();

    /// <summary>
    /// Defines a section.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    /// <param name="action">The action which renders the section.</param>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal void DefineSection(string name, Func<Task> action)
    {
#if NET6_0_OR_GREATER
        if (!Sections.TryAdd(name, action))
            throw new InvalidOperationException($"Section '{name}' is already defined.");
#else
        if (Sections.ContainsKey(name))
            throw new InvalidOperationException($"Section '{name}' is already defined.");

        Sections[name] = action;
#endif
    }

    void IEncodedContent.WriteTo(TextWriter textWriter)
        => Render(textWriter, CancellationToken.None);

    private class ExecutionResult : IRazorLayout.IExecutionResult
    {
        private readonly RazorTemplate _page;
        private readonly IReadOnlyDictionary<string, Func<Task>>? _sections;

        public IEncodedContent Body { get; }
        public IRazorLayout? Layout { get; }
        public CancellationToken CancellationToken { get; }

        public ExecutionResult(RazorTemplate page, StringBuilder body)
        {
            _page = page;
            _sections = page._sections;
            Body = new StringBuilderEncodedContent(body);
            Layout = page.Layout;
            CancellationToken = page.CancellationToken;
        }

        public async Task<IEncodedContent?> RenderSectionAsync(string name)
        {
            if (_sections is null || !_sections.TryGetValue(name, out var sectionAction))
                return null;

            using var executionScope = new ExecutionScope(_page, CancellationToken);
            await sectionAction().ConfigureAwait(false);
            return new StringBuilderEncodedContent(executionScope.Output);
        }
    }

    private protected class StringBuilderEncodedContent : IEncodedContent
    {
        public static IEncodedContent Empty { get; } = new StringBuilderEncodedContent(new StringBuilder());

        public StringBuilder StringBuilder { get; }

        public StringBuilderEncodedContent(StringBuilder stringBuilder)
            => StringBuilder = stringBuilder;

        public void WriteTo(TextWriter textWriter)
            => textWriter.Write(StringBuilder);

        public override string ToString()
            => StringBuilder.ToString();
    }

    private class ExecutionScope : IDisposable
    {
        private readonly RazorTemplate _page;

        private readonly Dictionary<string, Func<Task>>? _sections;
        private readonly TextWriter _output;
        private readonly CancellationToken _cancellationToken;
        private readonly IRazorLayout? _layout;

        public StringBuilder Output { get; }

        public ExecutionScope(RazorTemplate page, CancellationToken cancellationToken)
        {
            _page = page;

            _sections = page._sections;
            _output = page.Output;
            _cancellationToken = page.CancellationToken;
            _layout = page.Layout;

            Output = new StringBuilder();
            page.Output = new StringWriter(Output);
            page.CancellationToken = cancellationToken;
        }

        public void Dispose()
        {
            _page._sections = _sections;
            _page.Output = _output;
            _page.CancellationToken = _cancellationToken;
            _page.Layout = _layout;
        }
    }
}
