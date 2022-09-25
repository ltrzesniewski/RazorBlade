using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RazorBlade.Analyzers.Support;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace RazorBlade.Analyzers;

internal class LibraryCodeGenerator
{
    private readonly RazorCSharpDocument _generatedDoc;
    private readonly Compilation _compilation;
    private readonly CSharpParseOptions _parseOptions;
    private readonly CodeWriter _writer;
    private bool _hasCode;

    public LibraryCodeGenerator(RazorCSharpDocument generatedDoc, Compilation compilation, CSharpParseOptions parseOptions)
    {
        _generatedDoc = generatedDoc;
        _compilation = compilation;
        _parseOptions = parseOptions;

        _writer = new CodeWriter(Environment.NewLine, generatedDoc.Options);
    }

    public string Generate(CancellationToken cancellationToken)
    {
        var classSymbol = GetGeneratedClassSymbol(cancellationToken);

        if (classSymbol is not null)
        {
            _writer.WriteLine();
            _writer.WriteLine("// RazorSharp-specific code");
            _writer.WriteLine();

            if (!_generatedDoc.Options.SuppressNullabilityEnforcement)
            {
                _writer.WriteLine("#nullable restore");
                _writer.WriteLine();
            }

            using (_writer.BuildNamespace(classSymbol.ContainingNamespace.ToDisplayString()))
            using (_writer.BuildClassDeclaration(new[] { "partial" }, classSymbol.Name, null, Array.Empty<string>(), Array.Empty<TypeParameter>(), useNullableContext: false))
            {
                GenerateConstructors(classSymbol);
            }
        }

        return _hasCode ? _writer.GenerateCode() : string.Empty;
    }

    private INamedTypeSymbol? GetGeneratedClassSymbol(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var syntaxTree = CSharpSyntaxTree.ParseText(
            _generatedDoc.GeneratedCode,
            _parseOptions,
            cancellationToken: cancellationToken
        );

        var semanticModel = _compilation.AddSyntaxTrees(syntaxTree)
                                        .GetSemanticModel(syntaxTree);

        var classDeclaration = syntaxTree.GetRoot(cancellationToken)
                                         .DescendantNodes()
                                         .FirstOrDefault(static i => i.IsKind(SyntaxKind.ClassDeclaration)) as ClassDeclarationSyntax;

        if (classDeclaration is null)
            return null;

        return semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
    }

    private void GenerateConstructors(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.BaseType is not { } baseType)
            return;

        var templateCtorAttribute = _compilation.GetTypeByMetadataName("RazorBlade.Support.TemplateConstructorAttribute");
        if (templateCtorAttribute is null)
            return;

        foreach (var ctor in baseType.InstanceConstructors)
        {
            if (!ctor.HasAttribute(templateCtorAttribute))
                continue;

            StartMember();

            _writer.Write("public ")
                   .Write(classSymbol.Name)
                   .Write("(");

            foreach (var param in ctor.Parameters)
            {
                if (param.Ordinal != 0)
                    _writer.WriteParameterSeparator();

                WriteRefKind(param.RefKind);

                if (param.IsParams)
                    _writer.Write("params ");

                _writer.Write(param.Type.ToFullyQualifiedName())
                       .Write(" ")
                       .Write(param.Name.EscapeCSharpKeyword());
            }

            _writer.WriteLine(")");

            _writer.CurrentIndent += _writer.TabSize;
            _writer.Write(": base(");

            foreach (var param in ctor.Parameters)
            {
                if (param.Ordinal != 0)
                    _writer.WriteParameterSeparator();

                WriteRefKind(param.RefKind);
                _writer.Write(param.Name.EscapeCSharpKeyword());
            }

            _writer.WriteLine(")");
            _writer.CurrentIndent -= _writer.TabSize;

            _writer.WriteLine("{")
                   .WriteLine("}");
        }

        void WriteRefKind(RefKind refKind)
        {
            _writer.Write(refKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In  => "in ",
                _           => string.Empty
            });
        }
    }

    private void StartMember()
    {
        if (_hasCode)
            _writer.WriteLine();
        else
            _hasCode = true;
    }
}
