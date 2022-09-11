using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers.Tests.Support;

internal class AnalyzerConfigOptionsProviderMock : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        => new AnalyzerConfigOptionsMock();

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        => new AnalyzerConfigOptionsMock();

    public override AnalyzerConfigOptions GlobalOptions { get; } = new AnalyzerConfigOptionsMock();

    private class AnalyzerConfigOptionsMock : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            value = key switch
            {
                "build_metadata.AdditionalFiles.IsRazorBlade" => "true",
                "build_metadata.AdditionalFiles.Namespace"    => "TestNamespace",
                _                                             => string.Empty
            };

            return !string.IsNullOrEmpty(value);
        }
    }
}
