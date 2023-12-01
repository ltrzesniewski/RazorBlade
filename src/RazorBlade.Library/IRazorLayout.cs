using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// Represents a Razor layout page.
/// </summary>
public interface IRazorLayout
{
    /// <summary>
    /// Renders the layout for a given page.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <returns>The output data after rendering the layout, which can be used for the next layout.</returns>
    Task<IExecutionResult> RenderLayoutAsync(IExecutionResult input);

    /// <summary>
    /// The execution result of a page.
    /// </summary>
    public interface IExecutionResult
    {
        /// <summary>
        /// The rendered body contents.
        /// </summary>
        IEncodedContent Body { get; }

        /// <summary>
        /// The layout this execution result needs to be wrapped in.
        /// </summary>
        IRazorLayout? Layout { get; }

        /// <summary>
        /// The sections this page has defined.
        /// </summary>
        IReadOnlyDictionary<string, Func<Task>> Sections { get; }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
