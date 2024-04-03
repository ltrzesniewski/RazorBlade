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
    private ExecutionScope? _executionScope;
    private IRazorLayout? _layout;
    private Dictionary<string, Func<Task>>? _sections;

    private Dictionary<string, Func<Task>> Sections => _sections ??= new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The <see cref="TextWriter"/> which receives the output.
    /// </summary>
    protected internal TextWriter Output { get; internal set; } = TextWriter.Null;

    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected internal CancellationToken CancellationToken { get; private set; }

    /// <summary>
    /// The layout to use.
    /// </summary>
    private protected IRazorLayout? Layout
    {
        get => _layout;
        set
        {
            if (ReferenceEquals(value, _layout))
                return;

            _executionScope?.EnsureCanChangeLayout();
            _layout = value;
        }
    }

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

        var textWriter = new StringWriter();
        var renderTask = RenderAsyncCore(textWriter, cancellationToken);

        if (renderTask.IsCompleted)
        {
            renderTask.GetAwaiter().GetResult();
            return textWriter.ToString();
        }

        return Task.Run(
            async () =>
            {
                await renderTask.ConfigureAwait(false);
                return textWriter.ToString();
            },
            CancellationToken.None
        ).GetAwaiter().GetResult();
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

        var renderTask = RenderAsyncCore(textWriter, cancellationToken);

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

        var textWriter = new StringWriter();
        await RenderAsyncCore(textWriter, cancellationToken).ConfigureAwait(false);
        return textWriter.ToString();
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

        await RenderAsyncCore(textWriter, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Renders the template asynchronously including its layout and returns the result as a <see cref="StringBuilder"/>.
    /// </summary>
    private async Task RenderAsyncCore(TextWriter targetOutput, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var executionResult = await ExecuteAsyncCore(targetOutput, cancellationToken);

        while (executionResult.Layout is { } layout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            executionResult = await layout.ExecuteLayoutAsync(executionResult).ConfigureAwait(false);
        }

        switch (executionResult.Body)
        {
            case EncodedContent { Output: var bufferedOutput }:
                await WriteStringBuilderToOutputAndFlushAsync(bufferedOutput, targetOutput, cancellationToken).ConfigureAwait(false);
                break;

            case { } body: // Fallback case, shouldn't happen
                body.WriteTo(targetOutput);
                break;
        }
    }

    /// <summary>
    /// Calls the <see cref="ExecuteAsync"/> method in a new <see cref="ExecutionScope"/>.
    /// </summary>
    private protected virtual async Task<IRazorExecutionResult> ExecuteAsyncCore(TextWriter? targetOutput, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var executionScope = ExecutionScope.Start(this, targetOutput, cancellationToken);
        await ExecuteAsync().ConfigureAwait(false);
        return new ExecutionResult(executionScope);
    }

    /// <summary>
    /// Writes the buffered output to the target output then flushes the output stream.
    /// </summary>
    /// <remarks>
    /// This feature is not compatible with layouts.
    /// </remarks>
    protected Task FlushAsync()
        => _executionScope?.FlushAsync() ?? Task.CompletedTask;

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

    /// <summary>
    /// Writes the contents of a <see cref="StringBuilder"/> to a <see cref="TextWriter"/> asynchronuously.
    /// </summary>
    private static async Task WriteStringBuilderToOutputAndFlushAsync(StringBuilder input, TextWriter output, CancellationToken cancellationToken)
    {
        if (input.Length == 0)
            return;

#if NET6_0_OR_GREATER
        await output.WriteAsync(input, cancellationToken).ConfigureAwait(false);
#else
        await output.WriteAsync(input.ToString()).ConfigureAwait(false);
#endif

#if NET8_0_OR_GREATER
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
#else
        await output.FlushAsync().ConfigureAwait(false);
#endif
    }

    void IEncodedContent.WriteTo(TextWriter textWriter)
        => Render(textWriter, CancellationToken.None);

    /// <summary>
    /// Saves the current state, resets it, and restores it when disposed.
    /// </summary>
    private class ExecutionScope : IDisposable
    {
        private readonly ExecutionScope? _previousExecutionScope;
        private readonly Dictionary<string, Func<Task>>? _previousSections;
        private readonly TextWriter _previousOutput;
        private readonly CancellationToken _previousCancellationToken;
        private readonly IRazorLayout? _previousLayout;

        private readonly TextWriter? _targetOutput;
        private bool _layoutFrozen;

        public RazorTemplate Page { get; }
        public StringBuilder BufferedOutput { get; } = new();

        private ExecutionScope(RazorTemplate page, TextWriter? targetOutput, CancellationToken cancellationToken)
        {
            Page = page;
            _targetOutput = targetOutput;

            _previousExecutionScope = page._executionScope;
            _previousSections = page._sections;
            _previousOutput = page.Output;
            _previousCancellationToken = page.CancellationToken;
            _previousLayout = page.Layout;

            page._executionScope = this;
            page._layout = null;
            page._sections = null;
            page.Output = new StringWriter(BufferedOutput);
            page.CancellationToken = cancellationToken;
        }

        public static ExecutionScope Start(RazorTemplate page, TextWriter? targetOutput, CancellationToken cancellationToken)
            => new(page, targetOutput, cancellationToken);

        public void Dispose()
        {
            Page._executionScope = _previousExecutionScope;
            Page._layout = _previousLayout;
            Page._sections = _previousSections;
            Page.Output = _previousOutput;
            Page.CancellationToken = _previousCancellationToken;
        }

        public void FreezeLayout()
            => _layoutFrozen = true;

        public void EnsureCanChangeLayout()
        {
            if (_layoutFrozen)
                throw new InvalidOperationException("The layout can no longer be changed.");
        }

        public async Task FlushAsync()
        {
            if (Page.Layout is not null)
                throw new InvalidOperationException("The output cannot be flushed when a layout is used.");

            FreezeLayout();

            if (_targetOutput is not null)
            {
                await WriteStringBuilderToOutputAndFlushAsync(BufferedOutput, _targetOutput, Page.CancellationToken).ConfigureAwait(false);
                BufferedOutput.Clear();
            }
        }
    }

    /// <summary>
    /// Stores the result of a template execution.
    /// </summary>
    private class ExecutionResult : IRazorExecutionResult
    {
        private readonly RazorTemplate _page;
        private readonly IReadOnlyDictionary<string, Func<Task>>? _sections;

        public IEncodedContent Body { get; }
        public IRazorLayout? Layout { get; }
        public CancellationToken CancellationToken { get; }

        public ExecutionResult(ExecutionScope executionScope)
        {
            _page = executionScope.Page;
            _sections = _page._sections;
            Body = new EncodedContent(executionScope.BufferedOutput);
            Layout = _page.Layout;
            CancellationToken = _page.CancellationToken;
        }

        public bool IsSectionDefined(string name)
            => _sections?.ContainsKey(name) is true;

        public async Task<IEncodedContent?> RenderSectionAsync(string name)
        {
            if (_sections is null || !_sections.TryGetValue(name, out var sectionAction))
                return null;

            using var executionScope = ExecutionScope.Start(_page, null, CancellationToken);

            _page._layout = Layout; // The section might reference this instance.
            executionScope.FreezeLayout();

            await sectionAction().ConfigureAwait(false);
            return new EncodedContent(executionScope.BufferedOutput);
        }
    }

    /// <summary>
    /// Stores the output of a template execution as a <see cref="StringBuilder"/>.
    /// </summary>
    /// <remarks>
    /// StringBuilders can be combined more efficiently than strings, which is useful for layouts.
    /// <see cref="TextWriter"/> has a dedicated <c>Write</c> overload for <see cref="StringBuilder"/>.
    /// </remarks>
    private class EncodedContent : IEncodedContent
    {
        public StringBuilder Output { get; }

        public EncodedContent(StringBuilder value)
            => Output = value;

        public void WriteTo(TextWriter textWriter)
            => textWriter.Write(Output);

        public override string ToString()
            => Output.ToString();
    }
}
