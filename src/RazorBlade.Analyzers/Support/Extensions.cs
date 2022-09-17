using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers.Support;

internal static class Extensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
        where T : class
        => provider.SelectMany(static (item, _) => item is not null ? ImmutableArray.Create(item) : ImmutableArray<T>.Empty);
}
