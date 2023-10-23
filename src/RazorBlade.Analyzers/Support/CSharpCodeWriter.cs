using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace RazorBlade.Analyzers.Support;

internal class CSharpCodeWriter
{
    private readonly CodeWriter _writer;

    public CSharpCodeWriter(RazorCodeGenerationOptions options)
    {
        _writer = new CodeWriter(Environment.NewLine, options);
    }

    public CSharpCodeWriter WriteLine()
    {
        _writer.WriteLine();
        return this;
    }

    public CSharpCodeWriter WriteLine([LanguageInjection("csharp")] string value)
    {
        _writer.WriteLine(value);
        return this;
    }

    public CSharpCodeWriter Write([LanguageInjection("csharp")] string value)
    {
        _writer.Write(value);
        return this;
    }

    public CSharpCodeWriter WriteParameterSeparator()
    {
        _writer.WriteParameterSeparator();
        return this;
    }

    public override string ToString()
        => _writer.GenerateCode();

    public IndentDisposable IndentScope(int count = 1)
    {
        _writer.CurrentIndent += _writer.TabSize * count;
        return new IndentDisposable(_writer, count);
    }

    public struct IndentDisposable : IDisposable
    {
        private CodeWriter? _writer;
        private readonly int _count;

        public IndentDisposable(CodeWriter writer, int count)
        {
            _writer = writer;
            _count = count;
        }

        public void Dispose()
        {
            if (_writer is null)
                return;

            _writer.CurrentIndent -= _writer.TabSize * _count;
            _writer = null;
        }
    }
}
