using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers;

internal class GlobalOptions : IEquatable<GlobalOptions>
{
    private readonly List<Diagnostic> _diagnostics;

    public CSharpParseOptions ParseOptions { get; }
    public Accessibility? DefaultAccessibility { get; }
    public ImmutableArray<SyntaxTree> AdditionalSyntaxTrees { get; }

    private GlobalOptions(CSharpParseOptions parseOptions,
                          Accessibility? defaultAccessibility,
                          ImmutableArray<SyntaxTree> additionalSyntaxTrees,
                          List<Diagnostic> diagnostics)
    {
        ParseOptions = parseOptions;
        DefaultAccessibility = defaultAccessibility;
        AdditionalSyntaxTrees = additionalSyntaxTrees;

        _diagnostics = diagnostics;
    }

    public static GlobalOptions CreateEmpty()
    {
        return new GlobalOptions(
            CSharpParseOptions.Default,
            null,
            ImmutableArray<SyntaxTree>.Empty,
            new List<Diagnostic>()
        );
    }

    public static GlobalOptions Create(CSharpParseOptions parseOptions,
                                       AnalyzerConfigOptionsProvider optionsProvider,
                                       ImmutableArray<SyntaxTree> additionalSyntaxTrees)
    {
        var diagnostics = new List<Diagnostic>();

        var defaultAccessibility = (Accessibility?)null;

        if (optionsProvider.GlobalOptions.TryGetValue(Constants.GlobalOptions.DefaultAccessibility, out var defaultAccessibilityStr)
            && !string.IsNullOrEmpty(defaultAccessibilityStr))
        {
            if (TryParseTopLevelAccessibility(defaultAccessibilityStr, out var value))
                defaultAccessibility = value;
            else
                diagnostics.Add(Diagnostics.InvalidAccessibility(defaultAccessibilityStr));
        }

        return new GlobalOptions(
            parseOptions,
            defaultAccessibility,
            additionalSyntaxTrees,
            diagnostics
        );
    }

    public static bool TryParseTopLevelAccessibility(string value, out Accessibility accessibility)
    {
        if (string.Equals(value, SyntaxFacts.GetText(SyntaxKind.InternalKeyword), StringComparison.OrdinalIgnoreCase))
        {
            accessibility = Accessibility.Internal;
            return true;
        }

        if (string.Equals(value, SyntaxFacts.GetText(SyntaxKind.PublicKeyword), StringComparison.OrdinalIgnoreCase))
        {
            accessibility = Accessibility.Public;
            return true;
        }

        accessibility = default;
        return false;
    }

    public void ReportDiagnostics(SourceProductionContext context)
    {
        foreach (var diagnostic in _diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    public override bool Equals(object? obj)
        => Equals(obj as GlobalOptions);

    public bool Equals(GlobalOptions? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return ParseOptions.Equals(other.ParseOptions)
               && DefaultAccessibility == other.DefaultAccessibility
               && AdditionalSyntaxTrees.SequenceEqual(other.AdditionalSyntaxTrees);
    }

    public override int GetHashCode()
        => (ParseOptions, DefaultAccessibility, AdditionalSyntaxTrees.Length).GetHashCode();
}
