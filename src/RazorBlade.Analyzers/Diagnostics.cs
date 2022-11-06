using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers;

internal static class Diagnostics
{
    public const string Category = "RazorBlade";

    public enum Id
    {
        InternalError = 1,
        ModelDirectiveNotSupported = 2,
        ConditionalOnAsync = 3
    }

    private static readonly DiagnosticDescriptor _internalErrorDescriptor
        = RoslynError(Id.InternalError, "Internal RazorBlade error", "Internal RazorBlade error: {0}");

    public static Diagnostic InternalError(string message, Location location)
        => Diagnostic.Create(_internalErrorDescriptor, location, message);

    private static readonly RazorDiagnosticDescriptor _modelDirectiveNotSupportedDescriptor
        = RazorError(Id.ModelDirectiveNotSupported, "The @model directive is not supported in RazorBlade due to lack of IDE support. Use the @inherits directive instead: @inherits RazorBlade.HtmlTemplate<{0}>");

    public static RazorDiagnostic ModelDirectiveNotSupported(SourceSpan span, string modelType)
        => RazorDiagnostic.Create(_modelDirectiveNotSupportedDescriptor, span, modelType);

    private static DiagnosticDescriptor RoslynError(Id id, string title, string messageFormat)
        => new(GetDiagnosticId(id), title, messageFormat, Category, DiagnosticSeverity.Error, true);

    private static RazorDiagnosticDescriptor RazorError(Id id, string messageFormat)
        => new(GetDiagnosticId(id), () => messageFormat, RazorDiagnosticSeverity.Error);

    public static string GetDiagnosticId(Id id)
        => $"RB{(int)id:D4}";
}
