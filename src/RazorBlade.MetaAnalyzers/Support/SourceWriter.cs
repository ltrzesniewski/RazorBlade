using System;
using System.CodeDom.Compiler;
using System.IO;
using JetBrains.Annotations;

namespace RazorBlade.MetaAnalyzers.Support;

internal class SourceWriter
{
    private const string _indentString = "    ";

    private readonly IndentedTextWriter _writer = new(new StringWriter(), _indentString);

    public int Indent
    {
        get => _writer.Indent;
        set => _writer.Indent = value;
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

    public IndentDisposable IndentScope()
    {
        _writer.Indent++;
        return new IndentDisposable(this, null);
    }

    public IndentDisposable BlockScope()
    {
        _writer.WriteLine("{");
        _writer.Indent++;
        return new IndentDisposable(this, "}");
    }

    public struct IndentDisposable(SourceWriter writer, string? suffix) : IDisposable
    {
        private SourceWriter? _writer = writer;

        public void Dispose()
        {
            if (_writer is null)
                return;

            _writer._writer.Indent--;

            if (suffix is not null)
                _writer.WriteLine(suffix);

            _writer = null;
        }
    }
}
