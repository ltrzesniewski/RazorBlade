using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers.Support;

internal static class Extensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
        where T : class
        => provider.Where(static item => item is not null)!;
}
