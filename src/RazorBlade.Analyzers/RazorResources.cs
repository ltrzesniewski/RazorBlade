using System.Reflection;

// TODO Generate own code

namespace Microsoft.AspNetCore.Razor.Language
{
    internal static partial class Resources
    {
//         private static global::System.Resources.ResourceManager s_resourceManager;
//         internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(Resources)));
//         internal static global::System.Globalization.CultureInfo Culture { get; set; }
// #if !NET20
//         [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
// #endif
//         internal static string GetResourceString(string resourceKey, string defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture);
//
//         private static string GetResourceString(string resourceKey, string[] formatterNames)
//         {
//            var value = GetResourceString(resourceKey);
//            if (formatterNames != null)
//            {
//                for (var i = 0; i < formatterNames.Length; i++)
//                {
//                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
//                }
//            }
//            return value;
//         }

        internal static global::System.Globalization.CultureInfo Culture { get; set; }
        internal static string GetResourceString(string resourceKey)
            => resourceKey;

        /// <summary>Register Tag Helpers for use in the current document.</summary>
        internal static string @AddTagHelperDirective_Description => GetResourceString("AddTagHelperDirective_Description");
        /// <summary>Add tag helpers from the specified type name and assembly name. Specify '*' for the type name to include all tag helper types from the specified assembly.</summary>
        internal static string @AddTagHelperDirective_StringToken_Description => GetResourceString("AddTagHelperDirective_StringToken_Description");
        /// <summary>TypeName, AssemblyName</summary>
        internal static string @AddTagHelperDirective_StringToken_Name => GetResourceString("AddTagHelperDirective_StringToken_Name");
        /// <summary>Value cannot be null or an empty string.</summary>
        internal static string @ArgumentCannotBeNullOrEmpty => GetResourceString("ArgumentCannotBeNullOrEmpty");
        /// <summary>Block cannot be built because a Type has not been specified in the BlockBuilder</summary>
        internal static string @Block_Type_Not_Specified => GetResourceString("Block_Type_Not_Specified");
        /// <summary>Block directive '{0}' cannot be imported.</summary>
        internal static string @BlockDirectiveCannotBeImported => GetResourceString("BlockDirectiveCannotBeImported");
        /// <summary>Block directive '{0}' cannot be imported.</summary>
        internal static string FormatBlockDirectiveCannotBeImported(object p0)
           => string.Format(Culture, GetResourceString("BlockDirectiveCannotBeImported"), p0);

        /// <summary>code</summary>
        internal static string @BlockName_Code => GetResourceString("BlockName_Code");
        /// <summary>explicit expression</summary>
        internal static string @BlockName_ExplicitExpression => GetResourceString("BlockName_ExplicitExpression");
        /// <summary>Invalid newline sequence '{0}'. Support newline sequences are '\r\n' and '\n'.</summary>
        internal static string @CodeWriter_InvalidNewLine => GetResourceString("CodeWriter_InvalidNewLine");
        /// <summary>Invalid newline sequence '{0}'. Support newline sequences are '\r\n' and '\n'.</summary>
        internal static string FormatCodeWriter_InvalidNewLine(object p0)
           => string.Format(Culture, GetResourceString("CodeWriter_InvalidNewLine"), p0);

        /// <summary>&lt;&lt;character literal&gt;&gt;</summary>
        internal static string @CSharpToken_CharacterLiteral => GetResourceString("CSharpToken_CharacterLiteral");
        /// <summary>&lt;&lt;comment&gt;&gt;</summary>
        internal static string @CSharpToken_Comment => GetResourceString("CSharpToken_Comment");
        /// <summary>&lt;&lt;identifier&gt;&gt;</summary>
        internal static string @CSharpToken_Identifier => GetResourceString("CSharpToken_Identifier");
        /// <summary>&lt;&lt;integer literal&gt;&gt;</summary>
        internal static string @CSharpToken_IntegerLiteral => GetResourceString("CSharpToken_IntegerLiteral");
        /// <summary>&lt;&lt;keyword&gt;&gt;</summary>
        internal static string @CSharpToken_Keyword => GetResourceString("CSharpToken_Keyword");
        /// <summary>&lt;&lt;newline sequence&gt;&gt;</summary>
        internal static string @CSharpToken_Newline => GetResourceString("CSharpToken_Newline");
        /// <summary>&lt;&lt;real literal&gt;&gt;</summary>
        internal static string @CSharpToken_RealLiteral => GetResourceString("CSharpToken_RealLiteral");
        /// <summary>&lt;&lt;string literal&gt;&gt;</summary>
        internal static string @CSharpToken_StringLiteral => GetResourceString("CSharpToken_StringLiteral");
        /// <summary>&lt;&lt;white space&gt;&gt;</summary>
        internal static string @CSharpToken_Whitespace => GetResourceString("CSharpToken_Whitespace");
        /// <summary>The document type '{0}' does not support the extension '{1}'.</summary>
        internal static string @Diagnostic_CodeTarget_UnsupportedExtension => GetResourceString("Diagnostic_CodeTarget_UnsupportedExtension");
        /// <summary>The document type '{0}' does not support the extension '{1}'.</summary>
        internal static string FormatDiagnostic_CodeTarget_UnsupportedExtension(object p0, object p1)
           => string.Format(Culture, GetResourceString("Diagnostic_CodeTarget_UnsupportedExtension"), p0, p1);

        /// <summary>Invalid directive keyword '{0}'. Directives must have a non-empty keyword that consists only of letters.</summary>
        internal static string @DirectiveDescriptor_InvalidDirectiveKeyword => GetResourceString("DirectiveDescriptor_InvalidDirectiveKeyword");
        /// <summary>Invalid directive keyword '{0}'. Directives must have a non-empty keyword that consists only of letters.</summary>
        internal static string FormatDirectiveDescriptor_InvalidDirectiveKeyword(object p0)
           => string.Format(Culture, GetResourceString("DirectiveDescriptor_InvalidDirectiveKeyword"), p0);

        /// <summary>A non-optional directive token cannot follow an optional directive token.</summary>
        internal static string @DirectiveDescriptor_InvalidNonOptionalToken => GetResourceString("DirectiveDescriptor_InvalidNonOptionalToken");
        /// <summary>The '{0}' directive expects an identifier.</summary>
        internal static string @DirectiveExpectsIdentifier => GetResourceString("DirectiveExpectsIdentifier");
        /// <summary>The '{0}' directive expects an identifier.</summary>
        internal static string FormatDirectiveExpectsIdentifier(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsIdentifier"), p0);

        /// <summary>The '{0}' directive expects a namespace name.</summary>
        internal static string @DirectiveExpectsNamespace => GetResourceString("DirectiveExpectsNamespace");
        /// <summary>The '{0}' directive expects a namespace name.</summary>
        internal static string FormatDirectiveExpectsNamespace(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsNamespace"), p0);

        /// <summary>The '{0}' directive expects a string surrounded by double quotes.</summary>
        internal static string @DirectiveExpectsQuotedStringLiteral => GetResourceString("DirectiveExpectsQuotedStringLiteral");
        /// <summary>The '{0}' directive expects a string surrounded by double quotes.</summary>
        internal static string FormatDirectiveExpectsQuotedStringLiteral(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsQuotedStringLiteral"), p0);

        /// <summary>The '{0}' directive expects a type name.</summary>
        internal static string @DirectiveExpectsTypeName => GetResourceString("DirectiveExpectsTypeName");
        /// <summary>The '{0}' directive expects a type name.</summary>
        internal static string FormatDirectiveExpectsTypeName(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsTypeName"), p0);

        /// <summary>The '{0}` directive must appear at the start of the line.</summary>
        internal static string @DirectiveMustAppearAtStartOfLine => GetResourceString("DirectiveMustAppearAtStartOfLine");
        /// <summary>The '{0}` directive must appear at the start of the line.</summary>
        internal static string FormatDirectiveMustAppearAtStartOfLine(object p0)
           => string.Format(Culture, GetResourceString("DirectiveMustAppearAtStartOfLine"), p0);

        /// <summary>The '{0}' directives value(s) must be separated by whitespace.</summary>
        internal static string @DirectiveTokensMustBeSeparatedByWhitespace => GetResourceString("DirectiveTokensMustBeSeparatedByWhitespace");
        /// <summary>The '{0}' directives value(s) must be separated by whitespace.</summary>
        internal static string FormatDirectiveTokensMustBeSeparatedByWhitespace(object p0)
           => string.Format(Culture, GetResourceString("DirectiveTokensMustBeSeparatedByWhitespace"), p0);

        /// <summary>The document of kind '{0}' does not have a '{1}'. The document classifier must set a value for '{2}'.</summary>
        internal static string @DocumentMissingTarget => GetResourceString("DocumentMissingTarget");
        /// <summary>The document of kind '{0}' does not have a '{1}'. The document classifier must set a value for '{2}'.</summary>
        internal static string FormatDocumentMissingTarget(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("DocumentMissingTarget"), p0, p1, p2);

        /// <summary>The '{0}' directive may only occur once per document.</summary>
        internal static string @DuplicateDirective => GetResourceString("DuplicateDirective");
        /// <summary>The '{0}' directive may only occur once per document.</summary>
        internal static string FormatDuplicateDirective(object p0)
           => string.Format(Culture, GetResourceString("DuplicateDirective"), p0);

        /// <summary>"EndBlock" was called without a matching call to "StartBlock".</summary>
        internal static string @EndBlock_Called_Without_Matching_StartBlock => GetResourceString("EndBlock_Called_Without_Matching_StartBlock");
        /// <summary>line break</summary>
        internal static string @ErrorComponent_Newline => GetResourceString("ErrorComponent_Newline");
        /// <summary>The '{0}' feature requires a '{1}' provided by the '{2}'.</summary>
        internal static string @FeatureDependencyMissing => GetResourceString("FeatureDependencyMissing");
        /// <summary>The '{0}' feature requires a '{1}' provided by the '{2}'.</summary>
        internal static string FormatFeatureDependencyMissing(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("FeatureDependencyMissing"), p0, p1, p2);

        /// <summary>The feature must be initialized by setting the '{0}' property.</summary>
        internal static string @FeatureMustBeInitialized => GetResourceString("FeatureMustBeInitialized");
        /// <summary>The feature must be initialized by setting the '{0}' property.</summary>
        internal static string FormatFeatureMustBeInitialized(object p0)
           => string.Format(Culture, GetResourceString("FeatureMustBeInitialized"), p0);

        /// <summary>Specify a C# code block.</summary>
        internal static string @FunctionsDirective_Description => GetResourceString("FunctionsDirective_Description");
        /// <summary>&lt;&lt;newline sequence&gt;&gt;</summary>
        internal static string @HtmlToken_NewLine => GetResourceString("HtmlToken_NewLine");
        /// <summary>&lt;&lt;razor comment&gt;&gt;</summary>
        internal static string @HtmlToken_RazorComment => GetResourceString("HtmlToken_RazorComment");
        /// <summary>&lt;&lt;text&gt;&gt;</summary>
        internal static string @HtmlToken_Text => GetResourceString("HtmlToken_Text");
        /// <summary>&lt;&lt;white space&gt;&gt;</summary>
        internal static string @HtmlToken_WhiteSpace => GetResourceString("HtmlToken_WhiteSpace");
        /// <summary>Specify the base class for the current document.</summary>
        internal static string @InheritsDirective_Description => GetResourceString("InheritsDirective_Description");
        /// <summary>The base type that the current page inherits.</summary>
        internal static string @InheritsDirective_TypeToken_Description => GetResourceString("InheritsDirective_TypeToken_Description");
        /// <summary>TypeName</summary>
        internal static string @InheritsDirective_TypeToken_Name => GetResourceString("InheritsDirective_TypeToken_Name");
        /// <summary>The '{0}' operation is not valid when the builder is empty.</summary>
        internal static string @IntermediateNodeBuilder_PopInvalid => GetResourceString("IntermediateNodeBuilder_PopInvalid");
        /// <summary>The '{0}' operation is not valid when the builder is empty.</summary>
        internal static string FormatIntermediateNodeBuilder_PopInvalid(object p0)
           => string.Format(Culture, GetResourceString("IntermediateNodeBuilder_PopInvalid"), p0);

        /// <summary>The node '{0}' has a read-only child collection and cannot be modified.</summary>
        internal static string @IntermediateNodeReference_CollectionIsReadOnly => GetResourceString("IntermediateNodeReference_CollectionIsReadOnly");
        /// <summary>The node '{0}' has a read-only child collection and cannot be modified.</summary>
        internal static string FormatIntermediateNodeReference_CollectionIsReadOnly(object p0)
           => string.Format(Culture, GetResourceString("IntermediateNodeReference_CollectionIsReadOnly"), p0);

        /// <summary>The reference is invalid. The node '{0}' could not be found as a child of '{1}'.</summary>
        internal static string @IntermediateNodeReference_NodeNotFound => GetResourceString("IntermediateNodeReference_NodeNotFound");
        /// <summary>The reference is invalid. The node '{0}' could not be found as a child of '{1}'.</summary>
        internal static string FormatIntermediateNodeReference_NodeNotFound(object p0, object p1)
           => string.Format(Culture, GetResourceString("IntermediateNodeReference_NodeNotFound"), p0, p1);

        /// <summary>The reference is invalid. References initialized with the default constructor cannot modify nodes.</summary>
        internal static string @IntermediateNodeReference_NotInitialized => GetResourceString("IntermediateNodeReference_NotInitialized");
        /// <summary>The '{0}' node type can only be used as a direct child of a '{1}' node.</summary>
        internal static string @IntermediateNodes_InvalidParentNode => GetResourceString("IntermediateNodes_InvalidParentNode");
        /// <summary>The '{0}' node type can only be used as a direct child of a '{1}' node.</summary>
        internal static string FormatIntermediateNodes_InvalidParentNode(object p0, object p1)
           => string.Format(Culture, GetResourceString("IntermediateNodes_InvalidParentNode"), p0, p1);

        /// <summary>The node '{0}' is not the owner of change '{1}'.</summary>
        internal static string @InvalidOperation_SpanIsNotChangeOwner => GetResourceString("InvalidOperation_SpanIsNotChangeOwner");
        /// <summary>The node '{0}' is not the owner of change '{1}'.</summary>
        internal static string FormatInvalidOperation_SpanIsNotChangeOwner(object p0, object p1)
           => string.Format(Culture, GetResourceString("InvalidOperation_SpanIsNotChangeOwner"), p0, p1);

        /// <summary>Invalid tag helper directive look up text '{0}'. The correct look up text format is: "name, assemblyName".</summary>
        internal static string @InvalidTagHelperLookupText => GetResourceString("InvalidTagHelperLookupText");
        /// <summary>Invalid tag helper directive look up text '{0}'. The correct look up text format is: "name, assemblyName".</summary>
        internal static string FormatInvalidTagHelperLookupText(object p0)
           => string.Format(Culture, GetResourceString("InvalidTagHelperLookupText"), p0);

        /// <summary>Invalid tag helper directive '{0}' value. '{1}' is not allowed in prefix '{2}'.</summary>
        internal static string @InvalidTagHelperPrefixValue => GetResourceString("InvalidTagHelperPrefixValue");
        /// <summary>Invalid tag helper directive '{0}' value. '{1}' is not allowed in prefix '{2}'.</summary>
        internal static string FormatInvalidTagHelperPrefixValue(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("InvalidTagHelperPrefixValue"), p0, p1, p2);

        /// <summary>The key must not be null.</summary>
        internal static string @KeyMustNotBeNull => GetResourceString("KeyMustNotBeNull");
        /// <summary>Cannot use built-in RazorComment handler, language characteristics does not define the CommentStart, CommentStar and CommentBody known token types or parser does not override TokenizerBackedParser.OutputSpanBeforeRazorComment</summary>
        internal static string @Language_Does_Not_Support_RazorComment => GetResourceString("Language_Does_Not_Support_RazorComment");
        /// <summary>The specified encoding '{0}' does not match the content's encoding '{1}'.</summary>
        internal static string @MismatchedContentEncoding => GetResourceString("MismatchedContentEncoding");
        /// <summary>The specified encoding '{0}' does not match the content's encoding '{1}'.</summary>
        internal static string FormatMismatchedContentEncoding(object p0, object p1)
           => string.Format(Culture, GetResourceString("MismatchedContentEncoding"), p0, p1);

        /// <summary>The "@" character must be followed by a ":", "(", or a C# identifier.  If you intended to switch to markup, use an HTML start tag, for example:
        ///
        /// @if(isLoggedIn) {{
        ///     &lt;p&gt;Hello, @user!&lt;/p&gt;
        /// }}</summary>
        internal static string @ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start => GetResourceString("ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start");
        /// <summary>End of file was reached before the end of the block comment.  All comments started with "/*" sequence must be terminated with a matching "*/" sequence.</summary>
        internal static string @ParseError_BlockComment_Not_Terminated => GetResourceString("ParseError_BlockComment_Not_Terminated");
        /// <summary>Directive '{0}' must have a value.</summary>
        internal static string @ParseError_DirectiveMustHaveValue => GetResourceString("ParseError_DirectiveMustHaveValue");
        /// <summary>Directive '{0}' must have a value.</summary>
        internal static string FormatParseError_DirectiveMustHaveValue(object p0)
           => string.Format(Culture, GetResourceString("ParseError_DirectiveMustHaveValue"), p0);

        /// <summary>An opening "{0}" is missing the corresponding closing "{1}".</summary>
        internal static string @ParseError_Expected_CloseBracket_Before_EOF => GetResourceString("ParseError_Expected_CloseBracket_Before_EOF");
        /// <summary>An opening "{0}" is missing the corresponding closing "{1}".</summary>
        internal static string FormatParseError_Expected_CloseBracket_Before_EOF(object p0, object p1)
           => string.Format(Culture, GetResourceString("ParseError_Expected_CloseBracket_Before_EOF"), p0, p1);

        /// <summary>The {0} block is missing a closing "{1}" character.  Make sure you have a matching "{1}" character for all the "{2}" characters within this block, and that none of the "{1}" characters are being interpreted as markup.</summary>
        internal static string @ParseError_Expected_EndOfBlock_Before_EOF => GetResourceString("ParseError_Expected_EndOfBlock_Before_EOF");
        /// <summary>The {0} block is missing a closing "{1}" character.  Make sure you have a matching "{1}" character for all the "{2}" characters within this block, and that none of the "{1}" characters are being interpreted as markup.</summary>
        internal static string FormatParseError_Expected_EndOfBlock_Before_EOF(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("ParseError_Expected_EndOfBlock_Before_EOF"), p0, p1, p2);

        /// <summary>The {0} directive is not supported.</summary>
        internal static string @ParseError_HelperDirectiveNotAvailable => GetResourceString("ParseError_HelperDirectiveNotAvailable");
        /// <summary>The {0} directive is not supported.</summary>
        internal static string FormatParseError_HelperDirectiveNotAvailable(object p0)
           => string.Format(Culture, GetResourceString("ParseError_HelperDirectiveNotAvailable"), p0);

        /// <summary>Optional quote around the directive '{0}' is missing the corresponding opening or closing quote.</summary>
        internal static string @ParseError_IncompleteQuotesAroundDirective => GetResourceString("ParseError_IncompleteQuotesAroundDirective");
        /// <summary>Optional quote around the directive '{0}' is missing the corresponding opening or closing quote.</summary>
        internal static string FormatParseError_IncompleteQuotesAroundDirective(object p0)
           => string.Format(Culture, GetResourceString("ParseError_IncompleteQuotesAroundDirective"), p0);

        /// <summary>Inline markup blocks (@&lt;p&gt;Content&lt;/p&gt;) cannot be nested.  Only one level of inline markup is allowed.</summary>
        internal static string @ParseError_InlineMarkup_Blocks_Cannot_Be_Nested => GetResourceString("ParseError_InlineMarkup_Blocks_Cannot_Be_Nested");
        /// <summary>Markup in a code block must start with a tag and all start tags must be matched with end tags.  Do not use unclosed tags like "&lt;br&gt;".  Instead use self-closing tags like "&lt;br/&gt;".</summary>
        internal static string @ParseError_MarkupBlock_Must_Start_With_Tag => GetResourceString("ParseError_MarkupBlock_Must_Start_With_Tag");
        /// <summary>The "{0}" element was not closed.  All elements must be either self-closing or have a matching end tag.</summary>
        internal static string @ParseError_MissingEndTag => GetResourceString("ParseError_MissingEndTag");
        /// <summary>The "{0}" element was not closed.  All elements must be either self-closing or have a matching end tag.</summary>
        internal static string FormatParseError_MissingEndTag(object p0)
           => string.Format(Culture, GetResourceString("ParseError_MissingEndTag"), p0);

        /// <summary>Namespace imports and type aliases cannot be placed within code blocks.  They must immediately follow an "@" character in markup.  It is recommended that you put them at the top of the page, as in the following example:
        ///
        /// @using System.Drawing;
        /// @{{
        ///     // O ...</summary>
        internal static string @ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock => GetResourceString("ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock");
        /// <summary>Outer tag is missing a name. The first character of a markup block must be an HTML tag with a valid name.</summary>
        internal static string @ParseError_OuterTagMissingName => GetResourceString("ParseError_OuterTagMissingName");
        /// <summary>End of file was reached before the end of the block comment.  All comments that start with the "@*" sequence must be terminated with a matching "*@" sequence.</summary>
        internal static string @ParseError_RazorComment_Not_Terminated => GetResourceString("ParseError_RazorComment_Not_Terminated");
        /// <summary>"{0}" is a reserved word and cannot be used in implicit expressions.  An explicit expression ("@()") must be used.</summary>
        internal static string @ParseError_ReservedWord => GetResourceString("ParseError_ReservedWord");
        /// <summary>"{0}" is a reserved word and cannot be used in implicit expressions.  An explicit expression ("@()") must be used.</summary>
        internal static string FormatParseError_ReservedWord(object p0)
           => string.Format(Culture, GetResourceString("ParseError_ReservedWord"), p0);

        /// <summary>Section blocks ("{0}") cannot be nested.  Only one level of section blocks are allowed.</summary>
        internal static string @ParseError_Sections_Cannot_Be_Nested => GetResourceString("ParseError_Sections_Cannot_Be_Nested");
        /// <summary>Section blocks ("{0}") cannot be nested.  Only one level of section blocks are allowed.</summary>
        internal static string FormatParseError_Sections_Cannot_Be_Nested(object p0)
           => string.Format(Culture, GetResourceString("ParseError_Sections_Cannot_Be_Nested"), p0);

        /// <summary>Single-statement control-flow statements in Razor documents statements cannot contain markup. Markup should be enclosed in "{{" and "}}".</summary>
        internal static string @ParseError_SingleLine_ControlFlowStatements_CannotContainMarkup => GetResourceString("ParseError_SingleLine_ControlFlowStatements_CannotContainMarkup");
        /// <summary>"&lt;text&gt;" and "&lt;/text&gt;" tags cannot contain attributes.</summary>
        internal static string @ParseError_TextTagCannotContainAttributes => GetResourceString("ParseError_TextTagCannotContainAttributes");
        /// <summary>"{0}" is not valid at the start of a code block.  Only identifiers, keywords, comments, "(" and "{{" are valid.</summary>
        internal static string @ParseError_Unexpected_Character_At_Start_Of_CodeBlock => GetResourceString("ParseError_Unexpected_Character_At_Start_Of_CodeBlock");
        /// <summary>"{0}" is not valid at the start of a code block.  Only identifiers, keywords, comments, "(" and "{{" are valid.</summary>
        internal static string FormatParseError_Unexpected_Character_At_Start_Of_CodeBlock(object p0)
           => string.Format(Culture, GetResourceString("ParseError_Unexpected_Character_At_Start_Of_CodeBlock"), p0);

        /// <summary>End-of-file was found after the "@" character.  "@" must be followed by a valid code block.  If you want to output an "@", escape it using the sequence: "@@"</summary>
        internal static string @ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock => GetResourceString("ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock");
        /// <summary>Unexpected "{{" after "@" character. Once inside the body of a code block (@if {{}}, @{{}}, etc.) you do not need to use "@{{" to switch to code.</summary>
        internal static string @ParseError_Unexpected_Nested_CodeBlock => GetResourceString("ParseError_Unexpected_Nested_CodeBlock");
        /// <summary>A space or line break was encountered after the "@" character.  Only valid identifiers, keywords, comments, "(" and "{{" are valid at the start of a code block and they must occur immediately following "@" with no space in between.</summary>
        internal static string @ParseError_Unexpected_WhiteSpace_At_Start_Of_CodeBlock => GetResourceString("ParseError_Unexpected_WhiteSpace_At_Start_Of_CodeBlock");
        /// <summary>Encountered end tag "{0}" with no matching start tag.  Are your start/end tags properly balanced?</summary>
        internal static string @ParseError_UnexpectedEndTag => GetResourceString("ParseError_UnexpectedEndTag");
        /// <summary>Encountered end tag "{0}" with no matching start tag.  Are your start/end tags properly balanced?</summary>
        internal static string FormatParseError_UnexpectedEndTag(object p0)
           => string.Format(Culture, GetResourceString("ParseError_UnexpectedEndTag"), p0);

        /// <summary>End of file or an unexpected character was reached before the "{0}" tag could be parsed.  Elements inside markup blocks must be complete. They must either be self-closing ("&lt;br /&gt;") or have matching end tags ("&lt;p&gt;Hello&lt;/p&gt;").  If you intended to display a  ...</summary>
        internal static string @ParseError_UnfinishedTag => GetResourceString("ParseError_UnfinishedTag");
        /// <summary>End of file or an unexpected character was reached before the "{0}" tag could be parsed.  Elements inside markup blocks must be complete. They must either be self-closing ("&lt;br /&gt;") or have matching end tags ("&lt;p&gt;Hello&lt;/p&gt;").  If you intended to display a  ...</summary>
        internal static string FormatParseError_UnfinishedTag(object p0)
           => string.Format(Culture, GetResourceString("ParseError_UnfinishedTag"), p0);

        /// <summary>Unterminated string literal.  Strings that start with a quotation mark (") must be terminated before the end of the line.  However, strings that start with @ and a quotation mark (@") can span multiple lines.</summary>
        internal static string @ParseError_Unterminated_String_Literal => GetResourceString("ParseError_Unterminated_String_Literal");
        /// <summary>Parser was started with a null Context property.  The Context property must be set BEFORE calling any methods on the parser.</summary>
        internal static string @Parser_Context_Not_Set => GetResourceString("Parser_Context_Not_Set");
        /// <summary>Cannot complete the tree, StartBlock must be called at least once.</summary>
        internal static string @ParserContext_CannotCompleteTree_NoRootBlock => GetResourceString("ParserContext_CannotCompleteTree_NoRootBlock");
        /// <summary>Cannot complete the tree, there are still open blocks.</summary>
        internal static string @ParserContext_CannotCompleteTree_OutstandingBlocks => GetResourceString("ParserContext_CannotCompleteTree_OutstandingBlocks");
        /// <summary>Cannot finish span, there is no current block. Call StartBlock at least once before finishing a span</summary>
        internal static string @ParserContext_NoCurrentBlock => GetResourceString("ParserContext_NoCurrentBlock");
        /// <summary>The '{0}' phase requires a '{1}' provided by the '{2}'.</summary>
        internal static string @PhaseDependencyMissing => GetResourceString("PhaseDependencyMissing");
        /// <summary>The '{0}' phase requires a '{1}' provided by the '{2}'.</summary>
        internal static string FormatPhaseDependencyMissing(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("PhaseDependencyMissing"), p0, p1, p2);

        /// <summary>The phase must be initialized by setting the '{0}' property.</summary>
        internal static string @PhaseMustBeInitialized => GetResourceString("PhaseMustBeInitialized");
        /// <summary>The phase must be initialized by setting the '{0}' property.</summary>
        internal static string FormatPhaseMustBeInitialized(object p0)
           => string.Format(Culture, GetResourceString("PhaseMustBeInitialized"), p0);

        /// <summary>Path must begin with a forward slash '/'.</summary>
        internal static string @RazorProject_PathMustStartWithForwardSlash => GetResourceString("RazorProject_PathMustStartWithForwardSlash");
        /// <summary>Remove Tag Helpers for use in the current document.</summary>
        internal static string @RemoveTagHelperDirective_Description => GetResourceString("RemoveTagHelperDirective_Description");
        /// <summary>Remove tag helpers from the specified type name and assembly name. Specify '*' for the type name to remove all tag helper types from the specified assembly.</summary>
        internal static string @RemoveTagHelperDirective_StringToken_Description => GetResourceString("RemoveTagHelperDirective_StringToken_Description");
        /// <summary>TypeName, AssemblyName</summary>
        internal static string @RemoveTagHelperDirective_StringToken_Name => GetResourceString("RemoveTagHelperDirective_StringToken_Name");
        /// <summary>The '{0}' requires a '{1}' delegate to be set.</summary>
        internal static string @RenderingContextRequiresDelegate => GetResourceString("RenderingContextRequiresDelegate");
        /// <summary>The '{0}' requires a '{1}' delegate to be set.</summary>
        internal static string FormatRenderingContextRequiresDelegate(object p0, object p1)
           => string.Format(Culture, GetResourceString("RenderingContextRequiresDelegate"), p0, p1);

        /// <summary>Attribute '{0}' on tag helper element '{1}' requires a value. Tag helper bound attributes of type '{2}' cannot be empty or contain only whitespace.</summary>
        internal static string @RewriterError_EmptyTagHelperBoundAttribute => GetResourceString("RewriterError_EmptyTagHelperBoundAttribute");
        /// <summary>Attribute '{0}' on tag helper element '{1}' requires a value. Tag helper bound attributes of type '{2}' cannot be empty or contain only whitespace.</summary>
        internal static string FormatRewriterError_EmptyTagHelperBoundAttribute(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("RewriterError_EmptyTagHelperBoundAttribute"), p0, p1, p2);

        /// <summary>Define a section to be rendered in the configured layout page.</summary>
        internal static string @SectionDirective_Description => GetResourceString("SectionDirective_Description");
        /// <summary>The name of the section.</summary>
        internal static string @SectionDirective_NameToken_Description => GetResourceString("SectionDirective_NameToken_Description");
        /// <summary>SectionName</summary>
        internal static string @SectionDirective_NameToken_Name => GetResourceString("SectionDirective_NameToken_Name");
        /// <summary>@section Header { ... }</summary>
        internal static string @SectionExample => GetResourceString("SectionExample");
        /// <summary>&lt;&lt;unknown&gt;&gt;</summary>
        internal static string @Token_Unknown => GetResourceString("Token_Unknown");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with name '{2}' because the name contains a '{3}' character.</summary>
        internal static string @TagHelper_InvalidBoundAttributeName => GetResourceString("TagHelper_InvalidBoundAttributeName");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with name '{2}' because the name contains a '{3}' character.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributeName(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributeName"), p0, p1, p2, p3);

        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with name '{2}' because the name starts with '{3}'.</summary>
        internal static string @TagHelper_InvalidBoundAttributeNameStartsWith => GetResourceString("TagHelper_InvalidBoundAttributeNameStartsWith");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with name '{2}' because the name starts with '{3}'.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributeNameStartsWith(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributeNameStartsWith"), p0, p1, p2, p3);

        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with a null or empty name.</summary>
        internal static string @TagHelper_InvalidBoundAttributeNullOrWhitespace => GetResourceString("TagHelper_InvalidBoundAttributeNullOrWhitespace");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with a null or empty name.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributeNullOrWhitespace(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributeNullOrWhitespace"), p0, p1);

        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with prefix '{2}' because the prefix contains a '{3}' character.</summary>
        internal static string @TagHelper_InvalidBoundAttributePrefix => GetResourceString("TagHelper_InvalidBoundAttributePrefix");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with prefix '{2}' because the prefix contains a '{3}' character.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributePrefix(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributePrefix"), p0, p1, p2, p3);

        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with prefix '{2}' because the prefix starts with '{3}'.</summary>
        internal static string @TagHelper_InvalidBoundAttributePrefixStartsWith => GetResourceString("TagHelper_InvalidBoundAttributePrefixStartsWith");
        /// <summary>Invalid tag helper bound property '{1}' on tag helper '{0}'. Tag helpers cannot bind to HTML attributes with prefix '{2}' because the prefix starts with '{3}'.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributePrefixStartsWith(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributePrefixStartsWith"), p0, p1, p2, p3);

        /// <summary>Invalid restricted child '{1}' for tag helper '{0}'. Tag helpers cannot restrict child elements that contain a '{2}' character.</summary>
        internal static string @TagHelper_InvalidRestrictedChild => GetResourceString("TagHelper_InvalidRestrictedChild");
        /// <summary>Invalid restricted child '{1}' for tag helper '{0}'. Tag helpers cannot restrict child elements that contain a '{2}' character.</summary>
        internal static string FormatTagHelper_InvalidRestrictedChild(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidRestrictedChild"), p0, p1, p2);

        /// <summary>Invalid restricted child for tag helper '{0}'. Name cannot be null or whitespace.</summary>
        internal static string @TagHelper_InvalidRestrictedChildNullOrWhitespace => GetResourceString("TagHelper_InvalidRestrictedChildNullOrWhitespace");
        /// <summary>Invalid restricted child for tag helper '{0}'. Name cannot be null or whitespace.</summary>
        internal static string FormatTagHelper_InvalidRestrictedChildNullOrWhitespace(object p0)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidRestrictedChildNullOrWhitespace"), p0);

        /// <summary>Tag helpers cannot target attribute name '{0}' because it contains a '{1}' character.</summary>
        internal static string @TagHelper_InvalidTargetedAttributeName => GetResourceString("TagHelper_InvalidTargetedAttributeName");
        /// <summary>Tag helpers cannot target attribute name '{0}' because it contains a '{1}' character.</summary>
        internal static string FormatTagHelper_InvalidTargetedAttributeName(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidTargetedAttributeName"), p0, p1);

        /// <summary>Targeted attribute name cannot be null or whitespace.</summary>
        internal static string @TagHelper_InvalidTargetedAttributeNameNullOrWhitespace => GetResourceString("TagHelper_InvalidTargetedAttributeNameNullOrWhitespace");
        /// <summary>Tag helpers cannot target parent tag name '{0}' because it contains a '{1}' character.</summary>
        internal static string @TagHelper_InvalidTargetedParentTagName => GetResourceString("TagHelper_InvalidTargetedParentTagName");
        /// <summary>Tag helpers cannot target parent tag name '{0}' because it contains a '{1}' character.</summary>
        internal static string FormatTagHelper_InvalidTargetedParentTagName(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidTargetedParentTagName"), p0, p1);

        /// <summary>Targeted parent tag name cannot be null or whitespace.</summary>
        internal static string @TagHelper_InvalidTargetedParentTagNameNullOrWhitespace => GetResourceString("TagHelper_InvalidTargetedParentTagNameNullOrWhitespace");
        /// <summary>Tag helpers cannot target tag name '{0}' because it contains a '{1}' character.</summary>
        internal static string @TagHelper_InvalidTargetedTagName => GetResourceString("TagHelper_InvalidTargetedTagName");
        /// <summary>Tag helpers cannot target tag name '{0}' because it contains a '{1}' character.</summary>
        internal static string FormatTagHelper_InvalidTargetedTagName(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidTargetedTagName"), p0, p1);

        /// <summary>Targeted tag name cannot be null or whitespace.</summary>
        internal static string @TagHelper_InvalidTargetedTagNameNullOrWhitespace => GetResourceString("TagHelper_InvalidTargetedTagNameNullOrWhitespace");
        /// <summary>Tag helper directive assembly name cannot be null or empty.</summary>
        internal static string @TagHelperAssemblyNameCannotBeEmptyOrNull => GetResourceString("TagHelperAssemblyNameCannotBeEmptyOrNull");
        /// <summary>The tag helper attribute '{0}' in element '{1}' is missing a key. The syntax is '&lt;{1} {0}{{ key }}="value"&gt;'.</summary>
        internal static string @TagHelperBlockRewriter_IndexerAttributeNameMustIncludeKey => GetResourceString("TagHelperBlockRewriter_IndexerAttributeNameMustIncludeKey");
        /// <summary>The tag helper attribute '{0}' in element '{1}' is missing a key. The syntax is '&lt;{1} {0}{{ key }}="value"&gt;'.</summary>
        internal static string FormatTagHelperBlockRewriter_IndexerAttributeNameMustIncludeKey(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelperBlockRewriter_IndexerAttributeNameMustIncludeKey"), p0, p1);

        /// <summary>TagHelper attributes must be well-formed.</summary>
        internal static string @TagHelperBlockRewriter_TagHelperAttributeListMustBeWellFormed => GetResourceString("TagHelperBlockRewriter_TagHelperAttributeListMustBeWellFormed");
        /// <summary>The parent &lt;{0}&gt; tag helper does not allow non-tag content. Only child tag helper(s) targeting tag name(s) '{1}' are allowed.</summary>
        internal static string @TagHelperParseTreeRewriter_CannotHaveNonTagContent => GetResourceString("TagHelperParseTreeRewriter_CannotHaveNonTagContent");
        /// <summary>The parent &lt;{0}&gt; tag helper does not allow non-tag content. Only child tag helper(s) targeting tag name(s) '{1}' are allowed.</summary>
        internal static string FormatTagHelperParseTreeRewriter_CannotHaveNonTagContent(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelperParseTreeRewriter_CannotHaveNonTagContent"), p0, p1);

        /// <summary>Found an end tag (&lt;/{0}&gt;) for tag helper '{1}' with tag structure that disallows an end tag ('{2}').</summary>
        internal static string @TagHelperParseTreeRewriter_EndTagTagHelperMustNotHaveAnEndTag => GetResourceString("TagHelperParseTreeRewriter_EndTagTagHelperMustNotHaveAnEndTag");
        /// <summary>Found an end tag (&lt;/{0}&gt;) for tag helper '{1}' with tag structure that disallows an end tag ('{2}').</summary>
        internal static string FormatTagHelperParseTreeRewriter_EndTagTagHelperMustNotHaveAnEndTag(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("TagHelperParseTreeRewriter_EndTagTagHelperMustNotHaveAnEndTag"), p0, p1, p2);

        /// <summary>Tag helpers '{0}' and '{1}' targeting element '{2}' must not expect different {3} values.</summary>
        internal static string @TagHelperParseTreeRewriter_InconsistentTagStructure => GetResourceString("TagHelperParseTreeRewriter_InconsistentTagStructure");
        /// <summary>Tag helpers '{0}' and '{1}' targeting element '{2}' must not expect different {3} values.</summary>
        internal static string FormatTagHelperParseTreeRewriter_InconsistentTagStructure(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("TagHelperParseTreeRewriter_InconsistentTagStructure"), p0, p1, p2, p3);

        /// <summary>The &lt;{0}&gt; tag is not allowed by parent &lt;{1}&gt; tag helper. Only child tags with name(s) '{2}' are allowed.</summary>
        internal static string @TagHelperParseTreeRewriter_InvalidNestedTag => GetResourceString("TagHelperParseTreeRewriter_InvalidNestedTag");
        /// <summary>The &lt;{0}&gt; tag is not allowed by parent &lt;{1}&gt; tag helper. Only child tags with name(s) '{2}' are allowed.</summary>
        internal static string FormatTagHelperParseTreeRewriter_InvalidNestedTag(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("TagHelperParseTreeRewriter_InvalidNestedTag"), p0, p1, p2);

        /// <summary>Specify a prefix that is required in an element name for it to be included in Tag Helper processing.</summary>
        internal static string @TagHelperPrefixDirective_Description => GetResourceString("TagHelperPrefixDirective_Description");
        /// <summary>The tag prefix to apply to tag helpers.</summary>
        internal static string @TagHelperPrefixDirective_PrefixToken_Description => GetResourceString("TagHelperPrefixDirective_PrefixToken_Description");
        /// <summary>Prefix</summary>
        internal static string @TagHelperPrefixDirective_PrefixToken_Name => GetResourceString("TagHelperPrefixDirective_PrefixToken_Name");
        /// <summary>Tag Helper '{0}'s attributes must have names.</summary>
        internal static string @TagHelpers_AttributesMustHaveAName => GetResourceString("TagHelpers_AttributesMustHaveAName");
        /// <summary>Tag Helper '{0}'s attributes must have names.</summary>
        internal static string FormatTagHelpers_AttributesMustHaveAName(object p0)
           => string.Format(Culture, GetResourceString("TagHelpers_AttributesMustHaveAName"), p0);

        /// <summary>The tag helper '{0}' must not have C# in the element's attribute declaration area.</summary>
        internal static string @TagHelpers_CannotHaveCSharpInTagDeclaration => GetResourceString("TagHelpers_CannotHaveCSharpInTagDeclaration");
        /// <summary>The tag helper '{0}' must not have C# in the element's attribute declaration area.</summary>
        internal static string FormatTagHelpers_CannotHaveCSharpInTagDeclaration(object p0)
           => string.Format(Culture, GetResourceString("TagHelpers_CannotHaveCSharpInTagDeclaration"), p0);

        /// <summary>Code blocks (e.g. @{{var variable = 23;}}) must not appear in non-string tag helper attribute values.
        ///  Already in an expression (code) context. If necessary an explicit expression (e.g. @(@readonly)) may be used.</summary>
        internal static string @TagHelpers_CodeBlocks_NotSupported_InAttributes => GetResourceString("TagHelpers_CodeBlocks_NotSupported_InAttributes");
        /// <summary>Inline markup blocks (e.g. @&lt;p&gt;content&lt;/p&gt;) must not appear in non-string tag helper attribute values.
        ///  Expected a '{0}' attribute value, not a string.</summary>
        internal static string @TagHelpers_InlineMarkupBlocks_NotSupported_InAttributes => GetResourceString("TagHelpers_InlineMarkupBlocks_NotSupported_InAttributes");
        /// <summary>Inline markup blocks (e.g. @&lt;p&gt;content&lt;/p&gt;) must not appear in non-string tag helper attribute values.
        ///  Expected a '{0}' attribute value, not a string.</summary>
        internal static string FormatTagHelpers_InlineMarkupBlocks_NotSupported_InAttributes(object p0)
           => string.Format(Culture, GetResourceString("TagHelpers_InlineMarkupBlocks_NotSupported_InAttributes"), p0);

        /// <summary>Found a malformed '{0}' tag helper. Tag helpers must have a start and end tag or be self closing.</summary>
        internal static string @TagHelpersParseTreeRewriter_FoundMalformedTagHelper => GetResourceString("TagHelpersParseTreeRewriter_FoundMalformedTagHelper");
        /// <summary>Found a malformed '{0}' tag helper. Tag helpers must have a start and end tag or be self closing.</summary>
        internal static string FormatTagHelpersParseTreeRewriter_FoundMalformedTagHelper(object p0)
           => string.Format(Culture, GetResourceString("TagHelpersParseTreeRewriter_FoundMalformedTagHelper"), p0);

        /// <summary>Missing close angle for tag helper '{0}'.</summary>
        internal static string @TagHelpersParseTreeRewriter_MissingCloseAngle => GetResourceString("TagHelpersParseTreeRewriter_MissingCloseAngle");
        /// <summary>Missing close angle for tag helper '{0}'.</summary>
        internal static string FormatTagHelpersParseTreeRewriter_MissingCloseAngle(object p0)
           => string.Format(Culture, GetResourceString("TagHelpersParseTreeRewriter_MissingCloseAngle"), p0);

        /// <summary>Unreachable code. This can happen when a new {0} is introduced.</summary>
        internal static string @UnexpectedDirectiveKind => GetResourceString("UnexpectedDirectiveKind");
        /// <summary>Unreachable code. This can happen when a new {0} is introduced.</summary>
        internal static string FormatUnexpectedDirectiveKind(object p0)
           => string.Format(Culture, GetResourceString("UnexpectedDirectiveKind"), p0);

        /// <summary>Unexpected literal following the '{0}' directive. Expected '{1}'.</summary>
        internal static string @UnexpectedDirectiveLiteral => GetResourceString("UnexpectedDirectiveLiteral");
        /// <summary>Unexpected literal following the '{0}' directive. Expected '{1}'.</summary>
        internal static string FormatUnexpectedDirectiveLiteral(object p0, object p1)
           => string.Format(Culture, GetResourceString("UnexpectedDirectiveLiteral"), p0, p1);

        /// <summary>Unexpected end of file following the '{0}' directive. Expected '{1}'.</summary>
        internal static string @UnexpectedEOFAfterDirective => GetResourceString("UnexpectedEOFAfterDirective");
        /// <summary>Unexpected end of file following the '{0}' directive. Expected '{1}'.</summary>
        internal static string FormatUnexpectedEOFAfterDirective(object p0, object p1)
           => string.Format(Culture, GetResourceString("UnexpectedEOFAfterDirective"), p0, p1);

        /// <summary>The hash algorithm '{0}' is not supported for checksum generation. Supported algorithms are: '{1}'. Set '{2}' to '{3}' to suppress automatic checksum generation.</summary>
        internal static string @UnsupportedChecksumAlgorithm => GetResourceString("UnsupportedChecksumAlgorithm");
        /// <summary>The hash algorithm '{0}' is not supported for checksum generation. Supported algorithms are: '{1}'. Set '{2}' to '{3}' to suppress automatic checksum generation.</summary>
        internal static string FormatUnsupportedChecksumAlgorithm(object p0, object p1, object p2, object p3)
           => string.Format(Culture, GetResourceString("UnsupportedChecksumAlgorithm"), p0, p1, p2, p3);

        /// <summary>The '{0}.{1}' property must not be null.</summary>
        internal static string @PropertyMustNotBeNull => GetResourceString("PropertyMustNotBeNull");
        /// <summary>The '{0}.{1}' property must not be null.</summary>
        internal static string FormatPropertyMustNotBeNull(object p0, object p1)
           => string.Format(Culture, GetResourceString("PropertyMustNotBeNull"), p0, p1);

        /// <summary>The '{0}' is missing feature '{1}'.</summary>
        internal static string @RazorProjectEngineMissingFeatureDependency => GetResourceString("RazorProjectEngineMissingFeatureDependency");
        /// <summary>The '{0}' is missing feature '{1}'.</summary>
        internal static string FormatRazorProjectEngineMissingFeatureDependency(object p0, object p1)
           => string.Format(Culture, GetResourceString("RazorProjectEngineMissingFeatureDependency"), p0, p1);

        /// <summary>The Razor language version '{0}' is unrecognized or not supported by this version of Razor.</summary>
        internal static string @RazorLanguageVersion_InvalidVersion => GetResourceString("RazorLanguageVersion_InvalidVersion");
        /// <summary>The Razor language version '{0}' is unrecognized or not supported by this version of Razor.</summary>
        internal static string FormatRazorLanguageVersion_InvalidVersion(object p0)
           => string.Format(Culture, GetResourceString("RazorLanguageVersion_InvalidVersion"), p0);

        /// <summary>File path '{0}' does not belong to the directory '{1}'.</summary>
        internal static string @VirtualFileSystem_FileDoesNotBelongToDirectory => GetResourceString("VirtualFileSystem_FileDoesNotBelongToDirectory");
        /// <summary>File path '{0}' does not belong to the directory '{1}'.</summary>
        internal static string FormatVirtualFileSystem_FileDoesNotBelongToDirectory(object p0, object p1)
           => string.Format(Culture, GetResourceString("VirtualFileSystem_FileDoesNotBelongToDirectory"), p0, p1);

        /// <summary>The file path '{0}' is invalid. File path is the root relative path of the file starting with '/' and should not contain any '\' characters.</summary>
        internal static string @VirtualFileSystem_InvalidRelativePath => GetResourceString("VirtualFileSystem_InvalidRelativePath");
        /// <summary>The file path '{0}' is invalid. File path is the root relative path of the file starting with '/' and should not contain any '\' characters.</summary>
        internal static string FormatVirtualFileSystem_InvalidRelativePath(object p0)
           => string.Format(Culture, GetResourceString("VirtualFileSystem_InvalidRelativePath"), p0);

        /// <summary>Not enough stack space to continue parsing this document. Razor doesn't support deeply nested elements.</summary>
        internal static string @Rewriter_InsufficientStack => GetResourceString("Rewriter_InsufficientStack");
        /// <summary>Specify the base namespace for the document.</summary>
        internal static string @NamespaceDirective_Description => GetResourceString("NamespaceDirective_Description");
        /// <summary>The namespace for the document.</summary>
        internal static string @NamespaceDirective_NamespaceToken_Description => GetResourceString("NamespaceDirective_NamespaceToken_Description");
        /// <summary>Namespace</summary>
        internal static string @NamespaceDirective_NamespaceToken_Name => GetResourceString("NamespaceDirective_NamespaceToken_Name");
        /// <summary>Invalid tag helper bound attribute parameter '{1}' on bound attribute '{0}'. Tag helpers cannot bind to HTML attribute parameters with name '{1}' because the name contains a '{3}' character.</summary>
        internal static string @TagHelper_InvalidBoundAttributeParameterName => GetResourceString("TagHelper_InvalidBoundAttributeParameterName");
        /// <summary>Invalid tag helper bound attribute parameter '{1}' on bound attribute '{0}'. Tag helpers cannot bind to HTML attribute parameters with name '{1}' because the name contains a '{3}' character.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributeParameterName(object p0, object p1, object p3)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributeParameterName"), p0, p1, p3);

        /// <summary>Invalid tag helper bound attribute parameter '{0}'. Tag helpers cannot bind to HTML attribute parameters with a null or empty name.</summary>
        internal static string @TagHelper_InvalidBoundAttributeParameterNullOrWhitespace => GetResourceString("TagHelper_InvalidBoundAttributeParameterNullOrWhitespace");
        /// <summary>Invalid tag helper bound attribute parameter '{0}'. Tag helpers cannot bind to HTML attribute parameters with a null or empty name.</summary>
        internal static string FormatTagHelper_InvalidBoundAttributeParameterNullOrWhitespace(object p0)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundAttributeParameterNullOrWhitespace"), p0);

        /// <summary>The '{0}' directive expects a C# attribute.</summary>
        internal static string @DirectiveExpectsCSharpAttribute => GetResourceString("DirectiveExpectsCSharpAttribute");
        /// <summary>The '{0}' directive expects a C# attribute.</summary>
        internal static string FormatDirectiveExpectsCSharpAttribute(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsCSharpAttribute"), p0);

        /// <summary>Invalid tag helper bound directive attribute '{1}' on tag helper '{0}'. The directive attribute '{2}' should start with a '@' character.</summary>
        internal static string @TagHelper_InvalidBoundDirectiveAttributeName => GetResourceString("TagHelper_InvalidBoundDirectiveAttributeName");
        /// <summary>Invalid tag helper bound directive attribute '{1}' on tag helper '{0}'. The directive attribute '{2}' should start with a '@' character.</summary>
        internal static string FormatTagHelper_InvalidBoundDirectiveAttributeName(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundDirectiveAttributeName"), p0, p1, p2);

        /// <summary>Invalid tag helper bound directive attribute '{1}' on tag helper '{0}'. Tag helpers cannot bind to directive attributes with prefix '{2}' because the prefix doesn't start with a '@' character.</summary>
        internal static string @TagHelper_InvalidBoundDirectiveAttributePrefix => GetResourceString("TagHelper_InvalidBoundDirectiveAttributePrefix");
        /// <summary>Invalid tag helper bound directive attribute '{1}' on tag helper '{0}'. Tag helpers cannot bind to directive attributes with prefix '{2}' because the prefix doesn't start with a '@' character.</summary>
        internal static string FormatTagHelper_InvalidBoundDirectiveAttributePrefix(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidBoundDirectiveAttributePrefix"), p0, p1, p2);

        /// <summary>Invalid tag helper required directive attribute '{0}'. The directive attribute '{1}' should start with a '@' character.</summary>
        internal static string @TagHelper_InvalidRequiredDirectiveAttributeName => GetResourceString("TagHelper_InvalidRequiredDirectiveAttributeName");
        /// <summary>Invalid tag helper required directive attribute '{0}'. The directive attribute '{1}' should start with a '@' character.</summary>
        internal static string FormatTagHelper_InvalidRequiredDirectiveAttributeName(object p0, object p1)
           => string.Format(Culture, GetResourceString("TagHelper_InvalidRequiredDirectiveAttributeName"), p0, p1);

        /// <summary>The '{0}' directive expects a boolean literal.</summary>
        internal static string @DirectiveExpectsBooleanLiteral => GetResourceString("DirectiveExpectsBooleanLiteral");
        /// <summary>The '{0}' directive expects a boolean literal.</summary>
        internal static string FormatDirectiveExpectsBooleanLiteral(object p0)
           => string.Format(Culture, GetResourceString("DirectiveExpectsBooleanLiteral"), p0);

        /// <summary>@import rules are not supported within scoped CSS files because the loading order would be undefined. @import may only be placed in non-scoped CSS files.</summary>
        internal static string @CssRewriter_ImportNotAllowed => GetResourceString("CssRewriter_ImportNotAllowed");
        /// <summary>The type parameter in the generic type constraint '{1}' does not match the type parameter '{2}' defined in the directive '{0}'.</summary>
        internal static string @DirectiveGenericTypeParameterIdentifierMismatch => GetResourceString("DirectiveGenericTypeParameterIdentifierMismatch");
        /// <summary>The type parameter in the generic type constraint '{1}' does not match the type parameter '{2}' defined in the directive '{0}'.</summary>
        internal static string FormatDirectiveGenericTypeParameterIdentifierMismatch(object p0, object p1, object p2)
           => string.Format(Culture, GetResourceString("DirectiveGenericTypeParameterIdentifierMismatch"), p0, p1, p2);

        /// <summary>'{0}' is not valid in this position. Valid options are '{1}'</summary>
        internal static string @ParseError_Unexpected_Identifier_At_Position => GetResourceString("ParseError_Unexpected_Identifier_At_Position");
        /// <summary>'{0}' is not valid in this position. Valid options are '{1}'</summary>
        internal static string FormatParseError_Unexpected_Identifier_At_Position(object p0, object p1)
           => string.Format(Culture, GetResourceString("ParseError_Unexpected_Identifier_At_Position"), p0, p1);

        /// <summary>Component '{0}' expects a value for the parameter '{1}', but a value may not have been provided.</summary>
        internal static string @Component_EditorRequiredParameterNotSpecified => GetResourceString("Component_EditorRequiredParameterNotSpecified");
        /// <summary>Component '{0}' expects a value for the parameter '{1}', but a value may not have been provided.</summary>
        internal static string FormatComponent_EditorRequiredParameterNotSpecified(object p0, object p1)
           => string.Format(Culture, GetResourceString("Component_EditorRequiredParameterNotSpecified"), p0, p1);

        /// <summary>Encountered diagnostic '{0}'.</summary>
        internal static string @RazorDiagnosticDescriptor_DefaultError => GetResourceString("RazorDiagnosticDescriptor_DefaultError");
        /// <summary>Encountered diagnostic '{0}'.</summary>
        internal static string FormatRazorDiagnosticDescriptor_DefaultError(object p0)
           => string.Format(Culture, GetResourceString("RazorDiagnosticDescriptor_DefaultError"), p0);


    }

    internal static partial class ComponentResources
    {
//         private static global::System.Resources.ResourceManager s_resourceManager;
//         internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(ComponentResources)));
//         internal static global::System.Globalization.CultureInfo Culture { get; set; }
// #if !NET20
//         [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
// #endif
//         internal static string GetResourceString(string resourceKey, string defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture);
//
//         private static string GetResourceString(string resourceKey, string[] formatterNames)
//         {
//            var value = GetResourceString(resourceKey);
//            if (formatterNames != null)
//            {
//                for (var i = 0; i < formatterNames.Length; i++)
//                {
//                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
//                }
//            }
//            return value;
//         }

        internal static global::System.Globalization.CultureInfo Culture { get; set; }
        internal static string GetResourceString(string resourceKey)
            => resourceKey;

        /// <summary>The C# attribute that will be applied to the current class.</summary>
        internal static string @AttributeDirective_AttributeToken_Description => GetResourceString("AttributeDirective_AttributeToken_Description");
        /// <summary>Attribute</summary>
        internal static string @AttributeDirective_AttributeToken_Name => GetResourceString("AttributeDirective_AttributeToken_Name");
        /// <summary>Specifies the C# attribute that will be applied to the current class.</summary>
        internal static string @AttributeDirective_Description => GetResourceString("AttributeDirective_Description");
        /// <summary>Binds the provided expression to the '{0}' property and a change event delegate to the '{1}' property of the component.</summary>
        internal static string @BindTagHelper_Component_Documentation => GetResourceString("BindTagHelper_Component_Documentation");
        /// <summary>Binds the provided expression to the '{0}' property and a change event delegate to the '{1}' property of the component.</summary>
        internal static string FormatBindTagHelper_Component_Documentation(object p0, object p1)
           => string.Format(Culture, GetResourceString("BindTagHelper_Component_Documentation"), p0, p1);

        /// <summary>Specifies an action to run after the new value has been set.</summary>
        internal static string @BindTagHelper_Element_After_Documentation => GetResourceString("BindTagHelper_Element_After_Documentation");
        /// <summary>Specifies the culture to use for conversions.</summary>
        internal static string @BindTagHelper_Element_Culture_Documentation => GetResourceString("BindTagHelper_Element_Culture_Documentation");
        /// <summary>Binds the provided expression to the '{0}' attribute and a change event delegate to the '{1}' attribute.</summary>
        internal static string @BindTagHelper_Element_Documentation => GetResourceString("BindTagHelper_Element_Documentation");
        /// <summary>Binds the provided expression to the '{0}' attribute and a change event delegate to the '{1}' attribute.</summary>
        internal static string FormatBindTagHelper_Element_Documentation(object p0, object p1)
           => string.Format(Culture, GetResourceString("BindTagHelper_Element_Documentation"), p0, p1);

        /// <summary>Specifies the event handler name to attach for change notifications for the value provided by the '{0}' attribute.</summary>
        internal static string @BindTagHelper_Element_Event_Documentation => GetResourceString("BindTagHelper_Element_Event_Documentation");
        /// <summary>Specifies the event handler name to attach for change notifications for the value provided by the '{0}' attribute.</summary>
        internal static string FormatBindTagHelper_Element_Event_Documentation(object p0)
           => string.Format(Culture, GetResourceString("BindTagHelper_Element_Event_Documentation"), p0);

        /// <summary>Specifies a format to convert the value specified by the '{0}' attribute. The format string can currently only be used with expressions of type &lt;code&gt;DateTime&lt;/code&gt;.</summary>
        internal static string @BindTagHelper_Element_Format_Documentation => GetResourceString("BindTagHelper_Element_Format_Documentation");
        /// <summary>Specifies a format to convert the value specified by the '{0}' attribute. The format string can currently only be used with expressions of type &lt;code&gt;DateTime&lt;/code&gt;.</summary>
        internal static string FormatBindTagHelper_Element_Format_Documentation(object p0)
           => string.Format(Culture, GetResourceString("BindTagHelper_Element_Format_Documentation"), p0);

        /// <summary>Specifies the expression to use for binding the value to the attribute.</summary>
        internal static string @BindTagHelper_Element_Get_Documentation => GetResourceString("BindTagHelper_Element_Get_Documentation");
        /// <summary>Specifies the expression to use for updating the bound value when a new value is available.</summary>
        internal static string @BindTagHelper_Element_Set_Documentation => GetResourceString("BindTagHelper_Element_Set_Documentation");
        /// <summary>Binds the provided expression to an attribute and a change event, based on the naming of the bind attribute. For example: &lt;code&gt;@bind-value="..."&lt;/code&gt; and &lt;code&gt;@bind-value:event="onchange"&lt;/code&gt; will assign the current value of the expression to the 'v ...</summary>
        internal static string @BindTagHelper_Fallback_Documentation => GetResourceString("BindTagHelper_Fallback_Documentation");
        /// <summary>Specifies the event handler name to attach for change notifications for the value provided by the '{0}' attribute.</summary>
        internal static string @BindTagHelper_Fallback_Event_Documentation => GetResourceString("BindTagHelper_Fallback_Event_Documentation");
        /// <summary>Specifies the event handler name to attach for change notifications for the value provided by the '{0}' attribute.</summary>
        internal static string FormatBindTagHelper_Fallback_Event_Documentation(object p0)
           => string.Format(Culture, GetResourceString("BindTagHelper_Fallback_Event_Documentation"), p0);

        /// <summary>Specifies a format to convert the value specified by the corresponding bind attribute. For example: &lt;code&gt;@bind-value:format="..."&lt;/code&gt; will apply a format string to the value specified in &lt;code&gt;@bind-value="..."&lt;/code&gt;. The format string can currently o ...</summary>
        internal static string @BindTagHelper_Fallback_Format_Documentation => GetResourceString("BindTagHelper_Fallback_Format_Documentation");
        /// <summary>Specifies the parameter name for the '{0}' child content expression.</summary>
        internal static string @ChildContentParameterName_Documentation => GetResourceString("ChildContentParameterName_Documentation");
        /// <summary>Specifies the parameter name for the '{0}' child content expression.</summary>
        internal static string FormatChildContentParameterName_Documentation(object p0)
           => string.Format(Culture, GetResourceString("ChildContentParameterName_Documentation"), p0);

        /// <summary>Specifies the parameter name for all child content expressions.</summary>
        internal static string @ChildContentParameterName_TopLevelDocumentation => GetResourceString("ChildContentParameterName_TopLevelDocumentation");
        /// <summary>Specifies the type of the type parameter {0} for the {1} component.</summary>
        internal static string @ComponentTypeParameter_Documentation => GetResourceString("ComponentTypeParameter_Documentation");
        /// <summary>Specifies the type of the type parameter {0} for the {1} component.</summary>
        internal static string FormatComponentTypeParameter_Documentation(object p0, object p1)
           => string.Format(Culture, GetResourceString("ComponentTypeParameter_Documentation"), p0, p1);

        /// <summary>Sets the '{0}' attribute to the provided string or delegate value. A delegate value should be of type '{1}'.</summary>
        internal static string @EventHandlerTagHelper_Documentation => GetResourceString("EventHandlerTagHelper_Documentation");
        /// <summary>Sets the '{0}' attribute to the provided string or delegate value. A delegate value should be of type '{1}'.</summary>
        internal static string FormatEventHandlerTagHelper_Documentation(object p0, object p1)
           => string.Format(Culture, GetResourceString("EventHandlerTagHelper_Documentation"), p0, p1);

        /// <summary>Specifies whether to cancel (if cancelable) the default action that belongs to the '{0}' event.</summary>
        internal static string @EventHandlerTagHelper_PreventDefault_Documentation => GetResourceString("EventHandlerTagHelper_PreventDefault_Documentation");
        /// <summary>Specifies whether to cancel (if cancelable) the default action that belongs to the '{0}' event.</summary>
        internal static string FormatEventHandlerTagHelper_PreventDefault_Documentation(object p0)
           => string.Format(Culture, GetResourceString("EventHandlerTagHelper_PreventDefault_Documentation"), p0);

        /// <summary>Specifies whether to prevent further propagation of the '{0}' event in the capturing and bubbling phases.</summary>
        internal static string @EventHandlerTagHelper_StopPropagation_Documentation => GetResourceString("EventHandlerTagHelper_StopPropagation_Documentation");
        /// <summary>Specifies whether to prevent further propagation of the '{0}' event in the capturing and bubbling phases.</summary>
        internal static string FormatEventHandlerTagHelper_StopPropagation_Documentation(object p0)
           => string.Format(Culture, GetResourceString("EventHandlerTagHelper_StopPropagation_Documentation"), p0);

        /// <summary>Declares an interface implementation for the current class.</summary>
        internal static string @ImplementsDirective_Description => GetResourceString("ImplementsDirective_Description");
        /// <summary>The interface type implemented by the current class.</summary>
        internal static string @ImplementsDirective_TypeToken_Description => GetResourceString("ImplementsDirective_TypeToken_Description");
        /// <summary>TypeName</summary>
        internal static string @ImplementsDirective_TypeToken_Name => GetResourceString("ImplementsDirective_TypeToken_Name");
        /// <summary>Ensures that the component or element will be preserved across renders if (and only if) the supplied key value matches.</summary>
        internal static string @KeyTagHelper_Documentation => GetResourceString("KeyTagHelper_Documentation");
        /// <summary>Declares a layout type for the current document.</summary>
        internal static string @LayoutDirective_Description => GetResourceString("LayoutDirective_Description");
        /// <summary>The interface type implemented by the current document.</summary>
        internal static string @LayoutDirective_TypeToken_Description => GetResourceString("LayoutDirective_TypeToken_Description");
        /// <summary>TypeName</summary>
        internal static string @LayoutDirective_TypeToken_Name => GetResourceString("LayoutDirective_TypeToken_Name");
        /// <summary>The '@{0}' directive specified in {1} file will not be imported. The directive must appear at the top of each Razor file</summary>
        internal static string @PageDirectiveCannotBeImported => GetResourceString("PageDirectiveCannotBeImported");
        /// <summary>The '@{0}' directive specified in {1} file will not be imported. The directive must appear at the top of each Razor file</summary>
        internal static string FormatPageDirectiveCannotBeImported(object p0, object p1)
           => string.Format(Culture, GetResourceString("PageDirectiveCannotBeImported"), p0, p1);

        /// <summary>Mark the page as a routable component.</summary>
        internal static string @PageDirective_Description => GetResourceString("PageDirective_Description");
        /// <summary>An optional route template for the component.</summary>
        internal static string @PageDirective_RouteToken_Description => GetResourceString("PageDirective_RouteToken_Description");
        /// <summary>route template</summary>
        internal static string @PageDirective_RouteToken_Name => GetResourceString("PageDirective_RouteToken_Name");
        /// <summary>True if whitespace should be preserved, otherwise false.</summary>
        internal static string @PreserveWhitespaceDirective_BooleanToken_Description => GetResourceString("PreserveWhitespaceDirective_BooleanToken_Description");
        /// <summary>Preserve</summary>
        internal static string @PreserveWhitespaceDirective_BooleanToken_Name => GetResourceString("PreserveWhitespaceDirective_BooleanToken_Name");
        /// <summary>Specifies whether or not whitespace should be preserved exactly. Defaults to false for better performance.</summary>
        internal static string @PreserveWhitespaceDirective_Description => GetResourceString("PreserveWhitespaceDirective_Description");
        /// <summary>Populates the specified field or property with a reference to the element or component.</summary>
        internal static string @RefTagHelper_Documentation => GetResourceString("RefTagHelper_Documentation");
        /// <summary>Merges a collection of attributes into the current element or component.</summary>
        internal static string @SplatTagHelper_Documentation => GetResourceString("SplatTagHelper_Documentation");
        /// <summary>The constraints applied to the type parameter.</summary>
        internal static string @TypeParamDirective_Constraint_Description => GetResourceString("TypeParamDirective_Constraint_Description");
        /// <summary>type parameter constraint</summary>
        internal static string @TypeParamDirective_Constraint_Name => GetResourceString("TypeParamDirective_Constraint_Name");
        /// <summary>Declares a generic type parameter for the generated component class.</summary>
        internal static string @TypeParamDirective_Description => GetResourceString("TypeParamDirective_Description");
        /// <summary>The name of the type parameter.</summary>
        internal static string @TypeParamDirective_Token_Description => GetResourceString("TypeParamDirective_Token_Description");
        /// <summary>type parameter</summary>
        internal static string @TypeParamDirective_Token_Name => GetResourceString("TypeParamDirective_Token_Name");

    }
}
