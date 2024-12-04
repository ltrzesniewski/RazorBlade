using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

    /// <summary>
    /// The <see cref="TextWriter"/> which receives the output.
    /// </summary>
    protected internal TextWriter Output => _executionScope?.Output ?? TextWriter.Null;

    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected internal CancellationToken CancellationToken => _executionScope?.CancellationToken ?? CancellationToken.None;

    /// <summary>
    /// The layout to use.
    /// </summary>
    private protected IRazorLayout? Layout
    {
        get => _executionScope?.Layout;
        set => (_executionScope ?? throw new InvalidOperationException("The layout can only be set while the template is executing.")).SetLayout(value);
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

        var renderTask = RenderAsync(cancellationToken);

        if (renderTask.IsCompleted)
            return renderTask.GetAwaiter().GetResult();

        return Task.Run(async () => await renderTask.ConfigureAwait(false), CancellationToken.None).GetAwaiter().GetResult();
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
        var body = await RenderAsyncCore(null, cancellationToken).ConfigureAwait(false);

        switch (body)
        {
            case BufferedContent { Output: var bufferedOutput }:
                return bufferedOutput.ToString();

            default: // Fallback case, shouldn't happen
                var writer = new StringWriter();
                body.WriteTo(writer);
                return writer.ToString();
        }
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
        var body = await RenderAsyncCore(textWriter, cancellationToken).ConfigureAwait(false);

        switch (body)
        {
            case BufferedContent { Output: var bufferedOutput }:
                await WriteStringBuilderToOutputAsync(bufferedOutput, textWriter, cancellationToken).ConfigureAwait(false);
                break;

            default: // Fallback case, shouldn't happen
                body.WriteTo(textWriter);
                break;
        }
    }

    /// <summary>
    /// Renders the template and its layout stack.
    /// </summary>
    private async Task<IEncodedContent> RenderAsyncCore(TextWriter? targetOutput, CancellationToken cancellationToken)
    {
        var executionResult = await ExecuteAsyncCore(targetOutput, cancellationToken).ConfigureAwait(false);

        while (executionResult.Layout is { } layout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            executionResult = await layout.ExecuteLayoutAsync(executionResult).ConfigureAwait(false);
        }

        return executionResult.Body;
    }

    /// <summary>
    /// Calls the <see cref="ExecuteAsync"/> method in a new <see cref="ExecutionScope"/>.
    /// </summary>
    private protected virtual async Task<IRazorExecutionResult> ExecuteAsyncCore(TextWriter? targetOutput, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var executionScope = ExecutionScope.StartBody(this, targetOutput, cancellationToken);
        await ExecuteAsync().ConfigureAwait(false);
        return new ExecutionResult(executionScope);
    }

    /// <summary>
    /// Writes the buffered output to the target output then flushes the output stream.
    /// </summary>
    /// <returns>
    /// An empty <see cref="IEncodedContent"/>, which allows using a direct expression: <c>@await FlushAsync()</c>
    /// </returns>
    /// <remarks>
    /// This feature is not compatible with layouts.
    /// </remarks>
    [PublicAPI]
    protected internal async Task<IEncodedContent> FlushAsync()
    {
        if (_executionScope is not { } executionScope)
            throw new InvalidOperationException("The template is not executing.");

        await executionScope.FlushAsync().ConfigureAwait(false);
        return HtmlString.Empty;
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
        if (_executionScope is not { } executionScope)
            throw new InvalidOperationException("Sections can only be defined while the template is executing.");

        executionScope.DefineSection(name, action);
    }

    /// <summary>
    /// Pushes a writer on the stack.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal void PushWriter(TextWriter writer)
    {
        if (_executionScope is not { } executionScope)
            throw new InvalidOperationException("The writer stack can only be manipulated when the template is executing.");

        executionScope.PushWriter(writer);
    }

    /// <summary>
    /// Pops the last writer pushed on the stack.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected internal TextWriter PopWriter()
    {
        if (_executionScope is not { } executionScope)
            throw new InvalidOperationException("The writer stack can only be manipulated when the template is executing.");

        return executionScope.PopWriter();
    }

    /// <summary>
    /// Writes the contents of a <see cref="StringBuilder"/> to a <see cref="TextWriter"/> asynchronuously.
    /// </summary>
    private static Task WriteStringBuilderToOutputAsync(StringBuilder input, TextWriter output, CancellationToken cancellationToken)
    {
        if (input.Length == 0)
            return Task.CompletedTask;

#if NET6_0_OR_GREATER
        return output.WriteAsync(input, cancellationToken);
#else
        return output.WriteAsync(input.ToString());
#endif
    }

    void IEncodedContent.WriteTo(TextWriter textWriter)
        => Render(textWriter, CancellationToken.None);

    /// <summary>
    /// Stores the state of a template execution.
    /// </summary>
    private class ExecutionScope : IDisposable
    {
        private readonly RazorTemplate _page;
        private readonly ScopeKind _kind;
        private readonly TextWriter? _targetOutput;
        private readonly StringWriter? _bufferedOutput;
        private readonly ExecutionScope? _previousExecutionScope;

        private IRazorLayout? _layout;
        private bool _layoutFrozen;
        private Dictionary<string, Func<Task>>? _sections;

        public TextWriter Output { get; }

        public IRazorLayout? Layout => _layout;
        public CancellationToken CancellationToken { get; }

        public static ExecutionScope StartBody(RazorTemplate page, TextWriter? targetOutput, CancellationToken cancellationToken)
            => Start(new ExecutionScope(page, ScopeKind.Body, targetOutput, cancellationToken));

        private static ExecutionScope StartSection(ExecutionScope parent)
            => Start(new ExecutionScope(parent._page, ScopeKind.Section, null, parent.CancellationToken)
            {
                _layout = parent._layout, // The section might reference the layout instance.
                _layoutFrozen = true
            });

        private static void StartWriter(ExecutionScope parent, TextWriter writer)
            => Start(new ExecutionScope(parent._page, ScopeKind.Writer, writer, parent.CancellationToken));

        private static ExecutionScope Start(ExecutionScope executionScope)
        {
            executionScope._page._executionScope = executionScope;
            return executionScope;
        }

        private ExecutionScope(RazorTemplate page, ScopeKind kind, TextWriter? writer, CancellationToken cancellationToken)
        {
            _page = page;
            _kind = kind;
            CancellationToken = cancellationToken;

            switch (kind)
            {
                case ScopeKind.Body:
                    _targetOutput = writer;
                    _bufferedOutput = new StringWriter();
                    Output = _bufferedOutput;
                    break;

                case ScopeKind.Section:
                    Debug.Assert(writer is null);
                    _targetOutput = null;
                    _bufferedOutput = new StringWriter();
                    Output = _bufferedOutput;
                    break;

                case ScopeKind.Writer:
                    _targetOutput = writer;
                    _bufferedOutput = null;
                    Output = _targetOutput ?? throw new ArgumentNullException(nameof(writer));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            _previousExecutionScope = page._executionScope;
        }

        public void Dispose()
        {
            if (ReferenceEquals(_page._executionScope, this))
                _page._executionScope = _previousExecutionScope;
        }

        public void SetLayout(IRazorLayout? layout)
        {
            if (ReferenceEquals(layout, _layout))
                return;

            if (_layoutFrozen)
                throw new InvalidOperationException("The layout can no longer be changed.");

            _layout = layout;
        }

        public async Task FlushAsync()
        {
            if (_kind == ScopeKind.Writer)
            {
                await FlushTargetOutputWriter().ConfigureAwait(false);
                return;
            }

            if (_layout is not null)
                throw new InvalidOperationException("The output cannot be flushed when a layout is used.");

            // A part of the output will be written to the target output and discarded,
            // so disallow setting a layout later on, as that would lead to inconsistent results.
            _layoutFrozen = true;

            if (_targetOutput is null)
                return;

            if (_bufferedOutput?.GetStringBuilder() is { } bufferedOutput)
            {
                await WriteStringBuilderToOutputAsync(bufferedOutput, _targetOutput, CancellationToken).ConfigureAwait(false);
                bufferedOutput.Clear();
            }

            await FlushTargetOutputWriter().ConfigureAwait(false);

            Task FlushTargetOutputWriter()
            {
                if (_targetOutput is null)
                    return Task.CompletedTask;

#if NET8_0_OR_GREATER
                return _targetOutput.FlushAsync(CancellationToken);
#else
                return _targetOutput.FlushAsync();
#endif
            }
        }

        public BufferedContent ToBufferedContent()
            => new(_bufferedOutput?.GetStringBuilder() ?? new());

        public bool IsSectionDefined(string name)
            => _sections is { } sections && sections.ContainsKey(name);

        public void DefineSection(string name, Func<Task> action)
        {
            var sections = _sections ??= new(StringComparer.OrdinalIgnoreCase);

#if NET6_0_OR_GREATER
            if (!sections.TryAdd(name, action))
                throw new InvalidOperationException($"Section '{name}' is already defined.");
#else
            if (sections.ContainsKey(name))
                throw new InvalidOperationException($"Section '{name}' is already defined.");

            sections[name] = action;
#endif
        }

        public void PushWriter(TextWriter writer)
            => StartWriter(this, writer);

        public TextWriter PopWriter()
        {
            if (_kind != ScopeKind.Writer)
                throw new InvalidOperationException("The writer stack is empty.");

            Dispose();
            return Output;
        }

        public async Task<IEncodedContent?> RenderSectionAsync(string name)
        {
            if (_sections is not { } sections || !sections.TryGetValue(name, out var sectionAction))
                return null;

            using var sectionScope = StartSection(this);
            await sectionAction().ConfigureAwait(false);
            return sectionScope.ToBufferedContent();
        }

        private enum ScopeKind
        {
            Body,
            Section,
            Writer
        }
    }

    /// <summary>
    /// Stores the result of a template execution.
    /// </summary>
    private class ExecutionResult : IRazorExecutionResult
    {
        private readonly ExecutionScope _executionScope;

        public IEncodedContent Body { get; }
        public IRazorLayout? Layout => _executionScope.Layout;
        public CancellationToken CancellationToken => _executionScope.CancellationToken;

        public ExecutionResult(ExecutionScope executionScope)
        {
            _executionScope = executionScope;
            Body = executionScope.ToBufferedContent();
        }

        public bool IsSectionDefined(string name)
            => _executionScope.IsSectionDefined(name);

        public Task<IEncodedContent?> RenderSectionAsync(string name)
            => _executionScope.RenderSectionAsync(name);
    }

    /// <summary>
    /// Stores the output of a template execution as a <see cref="StringBuilder"/>.
    /// </summary>
    /// <remarks>
    /// StringBuilders can be combined more efficiently than strings, which is useful for layouts.
    /// <see cref="TextWriter"/> has a dedicated <c>Write</c> overload for <see cref="StringBuilder"/> in some frameworks.
    /// </remarks>
    private class BufferedContent : IEncodedContent
    {
        public StringBuilder Output { get; }

        public BufferedContent(StringBuilder value)
            => Output = value;

        public void WriteTo(TextWriter textWriter)
            => textWriter.Write(Output);

        public override string ToString()
            => Output.ToString();
    }

    /// <summary>
    /// Represents a deferred write operation.
    /// </summary>
    [PublicAPI]
    protected internal class HelperResult : IEncodedContent
    {
        private readonly Func<TextWriter, Task> _action;

        /// <summary>
        /// Creates a deferred operation.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public HelperResult(Func<TextWriter, Task> action)
            => _action = action;

        /// <inheritdoc />
        public void WriteTo(TextWriter textWriter)
            => _action.Invoke(textWriter).GetAwaiter().GetResult();
    }
}
