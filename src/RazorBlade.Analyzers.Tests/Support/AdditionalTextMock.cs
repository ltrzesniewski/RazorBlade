using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.Analyzers.Tests.Support;

internal class AdditionalTextMock : AdditionalText
{
    public string Text { get; }
    public override string Path { get; }

    public AdditionalTextMock(string text, string path)
    {
        Text = text;
        Path = path;
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => SourceText.From(Text);
}
