using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers.Support;

internal static class Extensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        => new(items);

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
        where T : class
        => provider.Where(static item => item is not null)!;

    public static IncrementalValuesProvider<T> WithLambdaComparer<T>(this IncrementalValuesProvider<T> source, Func<T, T, bool> equals, Func<T, int> getHashCode)
        => source.WithComparer(new LambdaComparer<T>(equals, getHashCode));

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                return attribute;
        }

        return null;
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
        => symbol.GetAttribute(attributeType) is not null;

    public static string EscapeCSharpKeyword(this string name)
        => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;

    public static IEnumerable<INamedTypeSymbol> SelfAndBaseTypes(this INamedTypeSymbol? symbol)
    {
        while (symbol is not null)
        {
            yield return symbol;
            symbol = symbol.BaseType;
        }
    }

    public static bool GetBooleanValue(this AnalyzerConfigOptions configOptions, string key)
        => configOptions.TryGetValue(key, out var stringValue)
           && bool.TryParse(stringValue, out var boolValue)
           && boolValue;

    private sealed class LambdaComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
            => x is not null
                ? y is not null && equals(x, y)
                : y is null;

        public int GetHashCode(T obj)
            => getHashCode(obj);
    }
}
