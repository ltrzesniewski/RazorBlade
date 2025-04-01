using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers;

internal class InputFile : IEquatable<InputFile>
{
    private readonly List<Diagnostic> _diagnostics;

    public AdditionalText AdditionalText { get; }
    public string? HintNamespace { get; }
    public string ClassName { get; }
    public Accessibility? Accessibility { get; }

    private InputFile(AdditionalText additionalText,
                      string? hintNamespace,
                      string className,
                      Accessibility? accessibility,
                      List<Diagnostic> diagnostics)
    {
        AdditionalText = additionalText;
        HintNamespace = hintNamespace;
        ClassName = className;
        Accessibility = accessibility;

        _diagnostics = diagnostics;
    }

    public static InputFile Create(AdditionalText additionalText, AnalyzerConfigOptions options)
    {
        var diagnostics = new List<Diagnostic>();
        var accessibility = (Accessibility?)null;

        if (!options.TryGetValue(Constants.FileOptions.HintNamespace, out var hintNamespace))
            hintNamespace = null;

        if (options.TryGetValue(Constants.FileOptions.Accessibility, out var accessibilityStr)
            && !string.IsNullOrEmpty(accessibilityStr))
        {
            if (GlobalOptions.TryParseTopLevelAccessibility(accessibilityStr, out var value))
                accessibility = value;
            else
                diagnostics.Add(Diagnostics.InvalidAccessibility(accessibilityStr));
        }

        return new InputFile(
            additionalText,
            hintNamespace,
            CSharpIdentifier.SanitizeIdentifier(Path.GetFileNameWithoutExtension(additionalText.Path)),
            accessibility,
            diagnostics
        );
    }

    public void ReportDiagnostics(SourceProductionContext context)
    {
        foreach (var diagnostic in _diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    public override bool Equals(object? obj)
        => Equals(obj as InputFile);

    public bool Equals(InputFile? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return AdditionalText.Equals(other.AdditionalText)
               && HintNamespace == other.HintNamespace
               && ClassName == other.ClassName
               && Accessibility == other.Accessibility;
    }

    public override int GetHashCode()
        => (AdditionalText, HintNamespace, ClassName, Accessibility).GetHashCode();
}
