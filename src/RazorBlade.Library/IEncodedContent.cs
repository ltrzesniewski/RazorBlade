using System.IO;

namespace RazorBlade;

/// <summary>
/// Encoded content to be written to the output as-is.
/// </summary>
public interface IEncodedContent
{
    /// <summary>
    /// Writes the content to the provided <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter"><see cref="TextWriter"/> to write the content to.</param>
    void WriteTo(TextWriter textWriter);
}
