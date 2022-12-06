using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers.Tests.Support;

internal class AnalyzerConfigOptionsProviderMock : AnalyzerConfigOptionsProvider, IEnumerable
{
    private readonly Dictionary<string, string> _values = new();

    public override AnalyzerConfigOptions GlobalOptions { get; }

    public AnalyzerConfigOptionsProviderMock()
        => GlobalOptions = new AnalyzerConfigOptionsMock(this);

    public void Add(string key, string value)
    {
        _values.Add($"build_metadata.AdditionalFiles.{key}", value);
        _values.Add($"build_property.{key}", value);
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        => GlobalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        => GlobalOptions;

    public IEnumerator GetEnumerator()
        => _values.GetEnumerator();

    private class AnalyzerConfigOptionsMock : AnalyzerConfigOptions
    {
        private readonly AnalyzerConfigOptionsProviderMock _mock;

        public AnalyzerConfigOptionsMock(AnalyzerConfigOptionsProviderMock mock)
            => _mock = mock;

        public override bool TryGetValue(string key, out string value)
            => _mock._values.TryGetValue(key, out value!);
    }
}
