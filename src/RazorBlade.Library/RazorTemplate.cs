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

    /// <summary>
    /// The <see cref="TextWriter"/> which receives the output.
    /// </summary>
    protected internal TextWriter Output => _executionScope?.Output ?? TextWriter.Null;

    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected internal CancellationToken CancellationToken => _executionScope?.CancellationToken ?? CancellationToken.None;

    /// <summary>
    /// The layout to be used with this template.
    /// </summary>
    private protected IRazorLayout? Layout => _executionScope?.Layout;

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
            case BufferedContent bufferedContent:
                return bufferedContent.ToString();

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
        if (textWriter is null)
            throw new ArgumentNullException(nameof(textWriter));

        var body = await RenderAsyncCore(textWriter, cancellationToken).ConfigureAwait(false);

        switch (body)
        {
            case BufferedContent bufferedContent:
                await bufferedContent.WriteToAsync(textWriter, cancellationToken).ConfigureAwait(false);
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

        using var bodyScope = ExecutionScope.StartBody(this, targetOutput, CreateLayoutInternal(), cancellationToken);
        await ExecuteAsync().ConfigureAwait(false);
        return new ExecutionResult(bodyScope);
    }

    /// <summary>
    /// Flushes the output stream.
    /// </summary>
    /// <returns>
    /// An empty <see cref="IEncodedContent"/>, which allows using a direct expression: <c>@await FlushAsync()</c>
    /// </returns>
    [PublicAPI]
    protected internal async Task<IEncodedContent> FlushAsync()
    {
#if NET8_0_OR_GREATER
        await Output.FlushAsync(CancellationToken).ConfigureAwait(false);
#else
        CancellationToken.ThrowIfCancellationRequested();
        await Output.FlushAsync().ConfigureAwait(false);
#endif

        return BufferedContent.Empty;
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
    [PublicAPI]
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
        if (_executionScope is not ExecutionScope.BodyScope executionScope)
            throw new InvalidOperationException("Sections can only be defined in a template body white it is executing.");

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
    /// Creates the layout instance to use with this template, or returns null when no layout should be used. This is called before rendering.
    /// </summary>
    /// <remarks>
    /// Using a layout disables direct rendering to <see cref="Output"/>. Intermediate output will need to be buffered.
    /// </remarks>
    private protected virtual IRazorLayout? CreateLayoutInternal()
        => null;

    void IEncodedContent.WriteTo(TextWriter textWriter)
        => Render(textWriter, CancellationToken.None);

    /// <summary>
    /// Stores the state of a template execution.
    /// </summary>
    private abstract class ExecutionScope : IDisposable
    {
        private readonly RazorTemplate _page;
        private readonly ExecutionScope? _previousExecutionScope;

#if NET5_0_OR_GREATER
        public TextWriter Output { get; private init; } = TextWriter.Null;
#else
        public TextWriter Output { get; private set; } = TextWriter.Null;
#endif

        public IRazorLayout? Layout { get; }
        public CancellationToken CancellationToken { get; }

        public static BodyScope StartBody(RazorTemplate page, TextWriter? targetOutput, IRazorLayout? layout, CancellationToken cancellationToken)
            => Start(new BodyScope(page, targetOutput, layout, cancellationToken));

        private static SectionScope StartSection(ExecutionScope parent)
            => Start(new SectionScope(parent));

        private static void StartWriter(ExecutionScope parent, TextWriter writer)
            => Start(new WriterScope(parent, writer));

        private static TScope Start<TScope>(TScope executionScope)
            where TScope : ExecutionScope
        {
            executionScope._page._executionScope = executionScope;
            return executionScope;
        }

        private ExecutionScope(RazorTemplate page, IRazorLayout? layout, CancellationToken cancellationToken)
        {
            _page = page;

            Layout = layout;
            CancellationToken = cancellationToken;

            _previousExecutionScope = page._executionScope;
        }

        private ExecutionScope(ExecutionScope parent)
            : this(parent._page, parent.Layout, parent.CancellationToken)
        {
        }

        public void Dispose()
        {
            if (ReferenceEquals(_page._executionScope, this))
                _page._executionScope = _previousExecutionScope;
        }

        public void PushWriter(TextWriter writer)
            => StartWriter(this, writer);

        public virtual TextWriter PopWriter()
            => throw new InvalidOperationException("The writer stack is empty.");

        public sealed class BodyScope : ExecutionScope
        {
            private readonly StringWriter? _bufferedOutput;

            private Dictionary<string, Func<Task>>? _sections;

            public BodyScope(RazorTemplate page, TextWriter? targetOutput, IRazorLayout? layout, CancellationToken cancellationToken)
                : base(page, layout, cancellationToken)
            {
                Output = targetOutput is not null && layout is null
                    ? targetOutput
                    : _bufferedOutput = new StringWriter();
            }

            public IEncodedContent ToBufferedContent()
                => _bufferedOutput is not null
                    ? new BufferedContent(_bufferedOutput.GetStringBuilder())
                    : BufferedContent.Empty;

            public bool IsSectionDefined(string name)
                => _sections?.ContainsKey(name) is true;

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

            public async Task<IEncodedContent?> RenderSectionAsync(string name)
            {
                if (_sections is not { } sections || !sections.TryGetValue(name, out var sectionAction))
                    return null;

                using var sectionScope = StartSection(this);
                await sectionAction().ConfigureAwait(false);
                return sectionScope.ToBufferedContent();
            }
        }

        private sealed class SectionScope : ExecutionScope
        {
            private readonly StringWriter _bufferedOutput = new();

            public SectionScope(ExecutionScope parent)
                : base(parent)
            {
                Output = _bufferedOutput;
            }

            public IEncodedContent ToBufferedContent()
                => new BufferedContent(_bufferedOutput.GetStringBuilder());
        }

        private sealed class WriterScope : ExecutionScope
        {
            public WriterScope(ExecutionScope parent, TextWriter writer)
                : base(parent)
            {
                Output = writer ?? throw new ArgumentNullException(nameof(writer));
            }

            public override TextWriter PopWriter()
            {
                Dispose();
                return Output;
            }
        }
    }

    /// <summary>
    /// Stores the result of a template execution.
    /// </summary>
    private sealed class ExecutionResult : IRazorExecutionResult
    {
        private readonly ExecutionScope.BodyScope _executionScope;

        public IEncodedContent Body { get; }
        public IRazorLayout? Layout => _executionScope.Layout;
        public CancellationToken CancellationToken => _executionScope.CancellationToken;

        public ExecutionResult(ExecutionScope.BodyScope executionScope)
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
    private sealed class BufferedContent : IEncodedContent
    {
        public static BufferedContent Empty { get; } = new(null);

        private readonly StringBuilder? _output;

        public BufferedContent(StringBuilder? value)
            => _output = value;

        public void WriteTo(TextWriter textWriter)
            => textWriter.Write(_output);

        public Task WriteToAsync(TextWriter textWriter, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            return textWriter.WriteAsync(_output, cancellationToken);
#else
            cancellationToken.ThrowIfCancellationRequested();
            return textWriter.WriteAsync(ToString());
#endif
        }

        public override string ToString()
            => _output?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Represents a deferred write operation.
    /// </summary>
    [PublicAPI]
    protected internal sealed class HelperResult : IEncodedContent
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
