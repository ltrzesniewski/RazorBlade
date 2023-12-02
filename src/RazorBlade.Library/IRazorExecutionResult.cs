using System.Threading;
using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// The execution result of a Razor template.
/// </summary>
internal interface IRazorExecutionResult
{
    /// <summary>
    /// The rendered body contents.
    /// </summary>
    IEncodedContent Body { get; }

    /// <summary>
    /// The layout this execution result should be wrapped in.
    /// </summary>
    IRazorLayout? Layout { get; }

    /// <summary>
    /// The cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Renders a section.
    /// </summary>
    /// <param name="name">The section name.</param>
    /// <returns>The rendered output, or null if the section is not defined.</returns>
    Task<IEncodedContent?> RenderSectionAsync(string name);

    /// <summary>
    /// Indicates if a given section is defined.
    /// </summary>
    /// <param name="name">The section name.</param>
    bool IsSectionDefined(string name);
}
