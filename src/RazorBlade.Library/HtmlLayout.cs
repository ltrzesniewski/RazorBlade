using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// Base class for HTML layout pages.
/// </summary>
public abstract class HtmlLayout : HtmlTemplate, IRazorLayout
{
    private IRazorLayout.IExecutionResult? _layoutInput;

    private IRazorLayout.IExecutionResult LayoutInput => _layoutInput ?? throw new InvalidOperationException("No layout is being rendered.");

    async Task<IRazorLayout.IExecutionResult> IRazorLayout.RenderLayoutAsync(IRazorLayout.IExecutionResult input)
    {
        input.CancellationToken.ThrowIfCancellationRequested();
        var previousStatus = (Output, CancellationToken);

        try
        {
            _layoutInput = input;

            var stringWriter = new StringWriter();

            Output = stringWriter;
            CancellationToken = input.CancellationToken;

            await ExecuteAsync().ConfigureAwait(false);

            return new ExecutionResult
            {
                Body = new StringBuilderEncodedContent(stringWriter.GetStringBuilder()),
                Layout = Layout,
                Sections = _sections,
                CancellationToken = CancellationToken
            };
        }
        finally
        {
            _layoutInput = null;
            (Output, CancellationToken) = previousStatus;
        }
    }

    /// <summary>
    /// Returns the inner page body.
    /// </summary>
    protected IEncodedContent RenderBody()
        => LayoutInput.Body;

    /// <summary>
    /// Renders a required section and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <returns>The content to write to the output.</returns>
    protected IEncodedContent RenderSection(string name)
        => RenderSection(name, true);

    /// <summary>
    /// Renders a section and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <param name="required">Whether the section is required.</param>
    /// <returns>The content to write to the output.</returns>
    protected IEncodedContent RenderSection(string name, bool required)
    {
        var renderTask = RenderSectionAsync(name, required);

        return renderTask.IsCompleted
            ? renderTask.GetAwaiter().GetResult()
            : Task.Run(async () => await renderTask.ConfigureAwait(false)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Renders a required section asynchronously and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <returns>The content to write to the output.</returns>
    protected Task<IEncodedContent> RenderSectionAsync(string name)
        => RenderSectionAsync(name, true);

    /// <summary>
    /// Renders a section asynchronously and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <param name="required">Whether the section is required.</param>
    /// <returns>The content to write to the output.</returns>
    protected async Task<IEncodedContent> RenderSectionAsync(string name, bool required)
    {
        if (!LayoutInput.Sections.TryGetValue(name, out var sectionAction))
        {
            if (required)
                throw new InvalidOperationException($"Section '{name}' is not defined.");

            return StringBuilderEncodedContent.Empty;
        }

        var previousOutput = Output;

        try
        {
            var stringWriter = new StringWriter();
            Output = stringWriter;

            await sectionAction.Invoke().ConfigureAwait(false);
            return new StringBuilderEncodedContent(stringWriter.GetStringBuilder());
        }
        finally
        {
            Output = previousOutput;
        }
    }
}
