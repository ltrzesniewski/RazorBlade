using System.Threading.Tasks;

namespace RazorBlade;

/// <summary>
/// Represents a Razor layout page.
/// </summary>
internal interface IRazorLayout
{
    /// <summary>
    /// Renders the layout for a given page.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <returns>The output data after rendering the layout, which can be used for the next layout.</returns>
    Task<IRazorExecutionResult> ExecuteLayoutAsync(IRazorExecutionResult input);
}
