using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RazorBlade.Analyzers;

internal class InputFile : IEquatable<InputFile>
{
    public AdditionalText AdditionalText { get; }
    public string? HintNamespace { get; }
    public string ClassName { get; }

    private InputFile(AdditionalText additionalText,
                      string? hintNamespace,
                      string className)
    {
        AdditionalText = additionalText;
        HintNamespace = hintNamespace;
        ClassName = className;
    }

    public static InputFile? Create(AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var options = optionsProvider.GetOptions(additionalText);

        options.TryGetValue(Constants.FileOptions.IsRazorBlade, out var isTargetFile);
        if (!string.Equals(isTargetFile, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!options.TryGetValue(Constants.FileOptions.HintNamespace, out var hintNamespace))
            hintNamespace = null;

        return new InputFile(
            additionalText,
            hintNamespace,
            CSharpIdentifier.SanitizeIdentifier(Path.GetFileNameWithoutExtension(additionalText.Path))
        );
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
               && ClassName == other.ClassName;
    }

    public override int GetHashCode()
        => (AdditionalText, HintNamespace, ClassName).GetHashCode();
}
