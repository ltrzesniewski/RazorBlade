using System;
using System.Collections.Immutable;
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
    private static readonly SymbolDisplayFormat _paramSignatureFormat
        = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType, // For enums
            parameterOptions: SymbolDisplayParameterOptions.IncludeName
                              | SymbolDisplayParameterOptions.IncludeType
                              | SymbolDisplayParameterOptions.IncludeParamsRefOut
                              | SymbolDisplayParameterOptions.IncludeDefaultValue,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                                  | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                                  | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    private readonly RazorCSharpDocument _generatedDoc;
    private readonly Compilation _compilation;
    private readonly CSharpParseOptions _parseOptions;
    private readonly CodeWriter _writer;
    private bool _hasCode;

    private INamedTypeSymbol? _classSymbol;
    private ImmutableArray<Diagnostic> _diagnostics;

    public LibraryCodeGenerator(RazorCSharpDocument generatedDoc, Compilation compilation, CSharpParseOptions parseOptions)
    {
        _generatedDoc = generatedDoc;
        _compilation = compilation;
        _parseOptions = parseOptions;

        _writer = new CodeWriter(Environment.NewLine, generatedDoc.Options);
    }

    public string Generate(CancellationToken cancellationToken)
    {
        Analyze(cancellationToken);

        if (_classSymbol is not null)
        {
            _writer.WriteLine();
            _writer.WriteLine("// RazorSharp-specific code");
            _writer.WriteLine();

            if (!_generatedDoc.Options.SuppressNullabilityEnforcement)
            {
                _writer.WriteLine("#nullable restore");
                _writer.WriteLine();
            }

            using (_writer.BuildNamespace(_classSymbol.ContainingNamespace.ToDisplayString()))
            using (_writer.BuildClassDeclaration(new[] { "partial" }, _classSymbol.Name, null, Array.Empty<string>(), Array.Empty<TypeParameter>(), useNullableContext: false))
            {
                GenerateConstructors();
                GenerateConditionalOnAsync();
            }
        }

        return _hasCode ? _writer.GenerateCode() : string.Empty;
    }

    private void Analyze(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var syntaxTree = CSharpSyntaxTree.ParseText(
            _generatedDoc.GeneratedCode,
            _parseOptions,
            cancellationToken: cancellationToken
        );

        var compilation = _compilation.AddSyntaxTrees(syntaxTree)
                                      .WithOptions(_compilation.Options.WithReportSuppressedDiagnostics(true));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var classDeclarationNode = syntaxTree.GetRoot(cancellationToken)
                                             .DescendantNodes()
                                             .FirstOrDefault(static i => i.IsKind(SyntaxKind.ClassDeclaration));

        _classSymbol = classDeclarationNode is ClassDeclarationSyntax classDeclarationSyntax
            ? semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken)
            : null;

        _diagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);
    }

    private void GenerateConstructors()
    {
        if (_classSymbol?.BaseType is not { } baseType)
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
                   .Write(_classSymbol!.Name);

            WriteParametersSignature(ctor);

            using (_writer.IndentScope())
            {
                _writer.Write(": base");
                WriteParametersCall(ctor);
                _writer.WriteLine();
            }

            _writer.WriteLine("{")
                   .WriteLine("}");
        }
    }

    private void GenerateConditionalOnAsync()
    {
        var conditionalOnAsyncAttribute = _compilation.GetTypeByMetadataName("RazorBlade.Support.ConditionalOnAsyncAttribute");
        if (conditionalOnAsyncAttribute is null)
            return;

        var executeMethodSymbol = _classSymbol?.GetMembers("ExecuteAsync")
                                              .OfType<IMethodSymbol>()
                                              .FirstOrDefault(i => i.Parameters.IsEmpty && i.IsAsync);

        var methodLocation = executeMethodSymbol?.Locations.FirstOrDefault();
        if (methodLocation is null)
            return;

        var isTemplateSync = _diagnostics.Any(i => i.Id == "CS1998" && i.Location == methodLocation);

        var currentClass = _classSymbol?.BaseType;
        while (currentClass is not null)
        {
            if (currentClass.SpecialType == SpecialType.System_Object)
                break;

            foreach (var methodSymbol in currentClass.GetMembers().OfType<IMethodSymbol>().Where(i => i.DeclaredAccessibility == Accessibility.Public))
            {
                var attributeData = methodSymbol.GetAttribute(conditionalOnAsyncAttribute);

                if (attributeData?.ConstructorArguments.FirstOrDefault().Value is bool onlyOnAsync && onlyOnAsync == isTemplateSync)
                {
                    StartMember();

                    _writer.WriteLine("/// <inheritdoc />");

                    // This currently doesn't have the intended effect :'(
                    _writer.WriteLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");

                    if (!isTemplateSync)
                        _writer.WriteLine($@"[global::System.Obsolete(""This method should not be used on an asynchronous template."", DiagnosticId = ""{Diagnostics.GetDiagnosticId(Diagnostics.Id.SyncMethodOnAsyncTemplate)}"")]");

                    _writer.Write("public new ")
                           .Write(methodSymbol.ReturnType.ToDisplayString(_paramSignatureFormat))
                           .Write(" ")
                           .Write(methodSymbol.Name);

                    WriteParametersSignature(methodSymbol);

                    using (_writer.IndentScope())
                    {
                        _writer.Write("=> base.").Write(methodSymbol.Name);
                        WriteParametersCall(methodSymbol);
                        _writer.WriteLine(";");
                    }
                }
            }

            currentClass = currentClass.BaseType;
        }
    }

    private void StartMember()
    {
        if (_hasCode)
            _writer.WriteLine();
        else
            _hasCode = true;
    }

    private void WriteParametersSignature(IMethodSymbol methodSymbol)
    {
        _writer.Write("(");

        foreach (var param in methodSymbol.Parameters)
        {
            if (param.Ordinal != 0)
                _writer.WriteParameterSeparator();

            _writer.Write(param.ToDisplayString(_paramSignatureFormat));
        }

        _writer.WriteLine(")");
    }

    private void WriteParametersCall(IMethodSymbol methodSymbol)
    {
        _writer.Write("(");

        foreach (var param in methodSymbol.Parameters)
        {
            if (param.Ordinal != 0)
                _writer.WriteParameterSeparator();

            _writer.Write(param.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In  => "in ",
                _           => string.Empty
            });

            _writer.Write(param.Name.EscapeCSharpKeyword());
        }

        _writer.Write(")");
    }
}
