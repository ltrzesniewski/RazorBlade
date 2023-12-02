using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// Base class for HTML layout pages.
/// </summary>
public abstract class HtmlLayout : HtmlTemplate, IRazorLayout
{
    private const string _contentsRequiredErrorMessage = "Layout pages can only be rendered when associated with a contents page.";

    private IRazorExecutionResult? _layoutInput;

    async Task<IRazorExecutionResult> IRazorLayout.ExecuteLayoutAsync(IRazorExecutionResult input)
    {
        try
        {
            _layoutInput = input;
            return await ExecuteAsyncCore(input.CancellationToken);
        }
        finally
        {
            _layoutInput = null;
        }
    }

    private protected override Task<IRazorExecutionResult> ExecuteAsyncCore(CancellationToken cancellationToken)
    {
        if (_layoutInput is null)
            throw new InvalidOperationException(_contentsRequiredErrorMessage);

        return base.ExecuteAsyncCore(cancellationToken);
    }

    /// <summary>
    /// Should not be used on layout pages.
    /// </summary>
    [Obsolete(_contentsRequiredErrorMessage, true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public new void Render(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(_contentsRequiredErrorMessage);

    /// <summary>
    /// Should not be used on layout pages.
    /// </summary>
    [Obsolete(_contentsRequiredErrorMessage, true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public new void Render(TextWriter textWriter, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(_contentsRequiredErrorMessage);

    /// <summary>
    /// Should not be used on layout pages.
    /// </summary>
    [Obsolete(_contentsRequiredErrorMessage, true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public new Task<string> RenderAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(_contentsRequiredErrorMessage);

    /// <summary>
    /// Should not be used on layout pages.
    /// </summary>
    [Obsolete(_contentsRequiredErrorMessage, true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public new Task RenderAsync(TextWriter textWriter, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(_contentsRequiredErrorMessage);

    /// <summary>
    /// Returns the inner page body.
    /// </summary>
    protected internal IEncodedContent RenderBody()
        => GetLayoutInput().Body;

    /// <summary>
    /// Renders a required section and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <returns>The content to write to the output.</returns>
    protected internal IEncodedContent RenderSection(string name)
        => RenderSection(name, true);

    /// <summary>
    /// Renders a section and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <param name="required">Whether the section is required.</param>
    /// <returns>The content to write to the output.</returns>
    protected internal IEncodedContent RenderSection(string name, bool required)
    {
        var renderTask = RenderSectionAsync(name, required);

        return renderTask.IsCompleted
            ? renderTask.GetAwaiter().GetResult()
            : Task.Run(async () => await renderTask.ConfigureAwait(false), CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Renders a required section asynchronously and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <returns>The content to write to the output.</returns>
    protected internal Task<IEncodedContent> RenderSectionAsync(string name)
        => RenderSectionAsync(name, true);

    /// <summary>
    /// Renders a section asynchronously and returns the result as encoded content.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <param name="required">Whether the section is required.</param>
    /// <returns>The content to write to the output.</returns>
    protected internal async Task<IEncodedContent> RenderSectionAsync(string name, bool required)
    {
        var result = await GetLayoutInput().RenderSectionAsync(name).ConfigureAwait(false);

        if (result is not null)
            return result;

        if (required)
            throw new InvalidOperationException($"Section '{name}' is not defined.");

        return HtmlString.Empty;
    }

    /// <summary>
    /// Indicates if a given section is defined.
    /// </summary>
    /// <param name="name">The section name.</param>
    protected internal bool IsSectionDefined(string name)
        => GetLayoutInput().IsSectionDefined(name);

    /// <summary>
    /// Ensures the template is being executed as a layout and returns the input data.
    /// </summary>
    private IRazorExecutionResult GetLayoutInput()
        => _layoutInput ?? throw new InvalidOperationException("The template is not being executed as a layout.");
}
