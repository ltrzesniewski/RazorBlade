using System;
using System.Globalization;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.Analyzers.Support;

internal static class RazorExtensions
{
    public static Diagnostic ToDiagnostic(this RazorDiagnostic razorDiagnostic)
    {
        var descriptor = new DiagnosticDescriptor(
            razorDiagnostic.Id,
            razorDiagnostic.GetMessage(CultureInfo.CurrentCulture),
            razorDiagnostic.GetMessage(CultureInfo.CurrentCulture),
            Diagnostics.Category,
            razorDiagnostic.Severity switch
            {
                RazorDiagnosticSeverity.Error   => DiagnosticSeverity.Error,
                RazorDiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                _                               => DiagnosticSeverity.Hidden,
            },
            isEnabledByDefault: true
        );

        return Diagnostic.Create(descriptor, razorDiagnostic.Span.ToLocation());
    }

    private static Location ToLocation(this SourceSpan span)
    {
        if (span == SourceSpan.Undefined)
            return Location.None;

        var linePosition = new LinePositionSpan(
            new LinePosition(span.LineIndex, span.CharacterIndex),
            new LinePosition(span.LineIndex, span.CharacterIndex + span.Length)
        );

        return Location.Create(
            span.FilePath,
            span.ToTextSpan(),
            linePosition
        );
    }

    private static TextSpan ToTextSpan(this SourceSpan sourceSpan)
        => new(sourceSpan.AbsoluteIndex, sourceSpan.Length);

    public static IndentDisposable IndentScope(this CodeWriter writer)
    {
        writer.CurrentIndent += writer.TabSize;
        return new IndentDisposable(writer);
    }

    public struct IndentDisposable(CodeWriter writer) : IDisposable
    {
        private CodeWriter? _writer = writer;

        public void Dispose()
        {
            if (_writer is null)
                return;

            _writer.CurrentIndent -= _writer.TabSize;
            _writer = null;
        }
    }
}
