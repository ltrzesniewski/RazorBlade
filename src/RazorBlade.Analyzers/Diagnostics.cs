using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers;

internal static class Diagnostics
{
    private const string _prefix = "RB";
    public const string Category = "RazorBlade";

    private static readonly DiagnosticDescriptor _internalErrorDescriptor
        = RoslynError(1, "Internal RazorBlade error", "Internal RazorBlade error: {0}");

    public static Diagnostic InternalError(string message, Location location)
        => Diagnostic.Create(_internalErrorDescriptor, location, message);

    private static readonly RazorDiagnosticDescriptor _modelDirectiveNotSupportedDescriptor
        = RazorError(2, "The @model directive is not supported in RazorBlade due to lack of IDE support. Use the @inherits directive instead: @inherits RazorBlade.HtmlTemplate<{0}>");

    public static RazorDiagnostic ModelDirectiveNotSupported(SourceSpan span, string modelType)
        => RazorDiagnostic.Create(_modelDirectiveNotSupportedDescriptor, span, modelType);

    private static DiagnosticDescriptor RoslynError(int index, string title, string messageFormat)
        => new($"{_prefix}{index:D3}", title, messageFormat, Category, DiagnosticSeverity.Error, true);

    private static RazorDiagnosticDescriptor RazorError(int index, string messageFormat)
        => new($"{_prefix}{index:D3}", () => messageFormat, RazorDiagnosticSeverity.Error);
}
