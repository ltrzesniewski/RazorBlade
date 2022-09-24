using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RazorBlade.Analyzers.Support;

internal static class Extensions
{
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat
        = SymbolDisplayFormat.FullyQualifiedFormat
                             .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
        where T : class
        => provider.Where(static item => item is not null)!;

    public static IncrementalValuesProvider<T> WithLambdaComparer<T>(this IncrementalValuesProvider<T> source, Func<T, T, bool> equals, Func<T, int> getHashCode)
        => source.WithComparer(new LambdaComparer<T>(equals, getHashCode));

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                return true;
        }

        return false;
    }

    public static string ToFullyQualifiedName(this ISymbol symbol)
        => symbol.ToDisplayString(_fullyQualifiedFormat);

    public static string EscapeCSharpKeyword(this string name)
        => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;

    private sealed class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public LambdaComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            _equals = equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
            => _equals(x, y);

        public int GetHashCode(T obj)
            => _getHashCode(obj);
    }
}
