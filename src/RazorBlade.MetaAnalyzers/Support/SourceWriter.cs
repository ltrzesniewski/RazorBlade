using System;
using System.CodeDom.Compiler;
using System.IO;
using JetBrains.Annotations;

namespace RazorBlade.MetaAnalyzers.Support;

internal class SourceWriter
{
    private const string _indentString = "    ";

    private readonly IndentedTextWriter _writer;

    public SourceWriter()
    {
        _writer = new IndentedTextWriter(new StringWriter(), _indentString);
    }

    public void WriteLine()
        => _writer.WriteLine();

    public void WriteLine([LanguageInjection("csharp")] string value)
        => _writer.WriteLine(value);

    public void Write([LanguageInjection("csharp")] string value)
        => _writer.Write(value);

    public void WriteIndent()
        => Write(_indentString);

    public void WriteVerbatimString(string value)
        => _writer.Write($"@\"{value.Replace("\"", "\"\"")}\"");

    public override string ToString()
        => _writer.InnerWriter.ToString();

    public IndentDisposable BlockScope()
    {
        _writer.WriteLine("{");
        _writer.Indent++;
        return new IndentDisposable(this, "}");
    }

    public struct IndentDisposable : IDisposable
    {
        private SourceWriter? _writer;
        private readonly string? _suffix;

        public IndentDisposable(SourceWriter writer, string? suffix)
        {
            _writer = writer;
            _suffix = suffix;
        }

        public void Dispose()
        {
            if (_writer is null)
                return;

            _writer._writer.Indent--;

            if (_suffix is not null)
                _writer.WriteLine(_suffix);

            _writer = null;
        }
    }
}
