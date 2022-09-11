using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.Analyzers.Tests.Support;

internal class AdditionalTextMock : AdditionalText
{
    public string Text { get; }

    public AdditionalTextMock(string text)
    {
        Text = text;
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => SourceText.From(Text);

    public override string Path => "./TestFile.cshtml";
}
