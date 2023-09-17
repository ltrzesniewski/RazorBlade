using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RazorBlade.Analyzers;

internal static class Diagnostics
{
    public const string Category = "RazorBlade";

    public enum Id
    {
        InternalError = 1,
        ModelDirectiveNotSupported = 2,
        ConditionalOnAsync = 3,
        EmbeddedLibraryUnsupportedCSharpVersion = 4,
        TagHelpersNotSupported = 5,
    }

    private static DiagnosticDescriptor RoslynError(Id id, string title, string messageFormat)
        => new(GetDiagnosticId(id), title, messageFormat, Category, DiagnosticSeverity.Error, true);

    private static RazorDiagnosticDescriptor RazorError(Id id, string messageFormat)
        => new(GetDiagnosticId(id), () => messageFormat, RazorDiagnosticSeverity.Error);

    public static string GetDiagnosticId(Id id)
        => $"RB{(int)id:D4}";

    // Internal error

    private static readonly DiagnosticDescriptor _internalErrorDescriptor
        = RoslynError(Id.InternalError, "Internal RazorBlade error", "Internal RazorBlade error: {0}");

    public static Diagnostic InternalError(string message, Location location)
        => Diagnostic.Create(_internalErrorDescriptor, location, message);

    // Model directive not supported

    private static readonly RazorDiagnosticDescriptor _modelDirectiveNotSupportedDescriptor
        = RazorError(Id.ModelDirectiveNotSupported, "The @model directive is not supported in RazorBlade due to lack of IDE support. Use the @inherits directive instead: @inherits RazorBlade.HtmlTemplate<{0}>");

    public static RazorDiagnostic ModelDirectiveNotSupported(SourceSpan span, string modelType)
        => RazorDiagnostic.Create(_modelDirectiveNotSupportedDescriptor, span, modelType);

    // Unsupported C# version for embedded library

    private static readonly DiagnosticDescriptor _embeddedLibraryUnsupportedCSharpVersionDescriptor
        = RoslynError(Id.EmbeddedLibraryUnsupportedCSharpVersion, "Unsupported C# version for embedded RazorBlade library", "The embedded RazorBlade library requires C# {0} or later. Please upgrade to a newer version.");

    public static Diagnostic EmbeddedLibraryUnsupportedCSharpVersion(LanguageVersion requiredVersion)
        => Diagnostic.Create(_embeddedLibraryUnsupportedCSharpVersionDescriptor, location: null, requiredVersion.ToDisplayString());

    // Tag helpers not supported

    private static readonly RazorDiagnosticDescriptor _tagHelpersNotSupportedDescriptor
        = RazorError(Id.TagHelpersNotSupported, "Tag helpers are not supported in RazorBlade.");

    public static RazorDiagnostic TagHelpersNotSupported(SourceSpan span)
        => RazorDiagnostic.Create(_tagHelpersNotSupportedDescriptor, span);
}
