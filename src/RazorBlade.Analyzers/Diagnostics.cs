using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers;

internal static class Diagnostics
{
    private const string _category = "RazorBlade";
    private const string _prefix = "RB";

    private static DiagnosticDescriptor InternalErrorDescriptor { get; }
        = Error(1, "Internal RazorBlade error", "Internal RazorBlade error: {0}");

    public static Diagnostic InternalError(string message, Location location)
        => Diagnostic.Create(InternalErrorDescriptor, location, message);

    private static DiagnosticDescriptor Error(int index, string title, string messageFormat)
        => new($"{_prefix}{index:D3}", title, messageFormat, _category, DiagnosticSeverity.Error, true);
}
