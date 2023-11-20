using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.Analyzers.Tests.Support;

internal class AdditionalTextMock(string text, string path) : AdditionalText
{
    public string Text { get; } = text;
    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => SourceText.From(Text);
}
