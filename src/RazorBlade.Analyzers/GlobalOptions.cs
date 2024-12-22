using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RazorBlade.Analyzers;

internal class GlobalOptions : IEquatable<GlobalOptions>
{
    public CSharpParseOptions ParseOptions { get; }
    public ImmutableArray<SyntaxTree> AdditionalSyntaxTrees { get; }

    private GlobalOptions(CSharpParseOptions parseOptions,
                          ImmutableArray<SyntaxTree> additionalSyntaxTrees)
    {
        ParseOptions = parseOptions;
        AdditionalSyntaxTrees = additionalSyntaxTrees;
    }

    public static GlobalOptions Create(CSharpParseOptions parseOptions,
                                       ImmutableArray<SyntaxTree> additionalSyntaxTrees)
    {
        return new GlobalOptions(
            parseOptions,
            additionalSyntaxTrees
        );
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
               && AdditionalSyntaxTrees.SequenceEqual(other.AdditionalSyntaxTrees);
    }

    public override int GetHashCode()
        => (ParseOptions, AdditionalSyntaxTrees.Length).GetHashCode();
}
