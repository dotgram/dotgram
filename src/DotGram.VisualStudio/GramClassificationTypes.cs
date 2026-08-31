using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

static class GramClassificationTypes
{
	public const string Invalid      = "DotGram invalid";
	public const string Comment      = "DotGram comment";
	public const string Keyword      = "DotGram keyword";
	public const string Identifier   = "DotGram identifier";
	public const string Number       = "DotGram number";
	public const string Literal      = "DotGram literal";
	public const string EmbeddedCode = "DotGram embedded code";
	public const string Transition   = "DotGram transition";
	public const string TransitionStyle = PredefinedClassificationTypeNames.Keyword;
	public const string SpecialSymbol  = "regex - quantifier";
	public const string Operator     = "DotGram operator";
	public const string Punctuation  = "DotGram punctuation";
	public const string DslType      = "DotGram DSL type";
	public const string DslVariable  = "DotGram DSL variable";
	public const string DslFunction  = "DotGram DSL function";
	public const string DslProperty  = "DotGram DSL property";
	public const string DslNamespace = "DotGram DSL namespace";
	public const string DslParameter = "DotGram DSL parameter";
	public const string DslLabel     = "DotGram DSL label";

	#pragma warning disable CS0414 // MEF discovers and supplies these exported definitions.

	[Export, Name(Invalid), BaseDefinition(PredefinedClassificationTypeNames.ExcludedCode)]
	static readonly ClassificationTypeDefinition InvalidDefinition = null!;

	[Export, Name(Comment), BaseDefinition(PredefinedClassificationTypeNames.Comment)]
	static readonly ClassificationTypeDefinition CommentDefinition = null!;

	[Export, Name(Identifier), BaseDefinition("local name")]
	static readonly ClassificationTypeDefinition IdentifierDefinition = null!;

	[Export, Name(Keyword), BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
	static readonly ClassificationTypeDefinition KeywordDefinition = null!;

	[Export, Name(Number), BaseDefinition(PredefinedClassificationTypeNames.Number)]
	static readonly ClassificationTypeDefinition NumberDefinition = null!;

	[Export, Name(Literal), BaseDefinition(PredefinedClassificationTypeNames.String)]
	static readonly ClassificationTypeDefinition LiteralDefinition = null!;

	[Export, Name(EmbeddedCode), BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
	static readonly ClassificationTypeDefinition EmbeddedCodeDefinition = null!;

	[Export, Name(Transition), BaseDefinition(PredefinedClassificationTypeNames.PreprocessorKeyword)]
	static readonly ClassificationTypeDefinition TransitionDefinition = null!;

	[Export, Name(Operator), BaseDefinition(PredefinedClassificationTypeNames.Operator)]
	static readonly ClassificationTypeDefinition OperatorDefinition = null!;

	[Export, Name(Punctuation), BaseDefinition(PredefinedClassificationTypeNames.Punctuation)]
	static readonly ClassificationTypeDefinition PunctuationDefinition = null!;

	[Export, Name(DslType), BaseDefinition("class name")]
	static readonly ClassificationTypeDefinition DslTypeDefinition = null!;

	[Export, Name(DslVariable), BaseDefinition("local name")]
	static readonly ClassificationTypeDefinition DslVariableDefinition = null!;

	[Export, Name(DslFunction), BaseDefinition("method name")]
	static readonly ClassificationTypeDefinition DslFunctionDefinition = null!;

	[Export, Name(DslProperty), BaseDefinition("property name")]
	static readonly ClassificationTypeDefinition DslPropertyDefinition = null!;

	[Export, Name(DslNamespace), BaseDefinition("namespace name")]
	static readonly ClassificationTypeDefinition DslNamespaceDefinition = null!;

	[Export, Name(DslParameter), BaseDefinition("parameter name")]
	static readonly ClassificationTypeDefinition DslParameterDefinition = null!;

	[Export, Name(DslLabel), BaseDefinition("label name")]
	static readonly ClassificationTypeDefinition DslLabelDefinition = null!;

	#pragma warning restore CS0414
}

abstract class GramFormatDefinition : ClassificationFormatDefinition
{
	protected GramFormatDefinition(string displayName) => DisplayName = displayName;
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Invalid)]
[Name(GramClassificationTypes.Invalid)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramInvalidFormat : GramFormatDefinition
{
	public GramInvalidFormat() : base("DotGram Invalid") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Comment)]
[Name(GramClassificationTypes.Comment)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramCommentFormat : GramFormatDefinition
{
	public GramCommentFormat() : base("DotGram Comment") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Keyword)]
[Name(GramClassificationTypes.Keyword)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramKeywordFormat : GramFormatDefinition
{
	public GramKeywordFormat() : base("DotGram Keyword") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Identifier)]
[Name(GramClassificationTypes.Identifier)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramIdentifierFormat : GramFormatDefinition
{
	public GramIdentifierFormat() : base("DotGram Identifier") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Number)]
[Name(GramClassificationTypes.Number)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramNumberFormat : GramFormatDefinition
{
	public GramNumberFormat() : base("DotGram Number") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Literal)]
[Name(GramClassificationTypes.Literal)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramLiteralFormat : GramFormatDefinition
{
	public GramLiteralFormat() : base("DotGram Literal") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.EmbeddedCode)]
[Name(GramClassificationTypes.EmbeddedCode)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramEmbeddedCodeFormat : GramFormatDefinition
{
	public GramEmbeddedCodeFormat() : base("DotGram Embedded Code") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Transition)]
[Name(GramClassificationTypes.Transition)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramTransitionFormat : GramFormatDefinition
{
	public GramTransitionFormat() : base("DotGram Transition") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Operator)]
[Name(GramClassificationTypes.Operator)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramOperatorFormat : GramFormatDefinition
{
	public GramOperatorFormat() : base("DotGram Operator") { }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = GramClassificationTypes.Punctuation)]
[Name(GramClassificationTypes.Punctuation)]
[UserVisible(true)]
[Order(Before = Priority.Default)]
sealed class GramPunctuationFormat : GramFormatDefinition
{
	public GramPunctuationFormat() : base("DotGram Punctuation") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslType)]
[Name(GramClassificationTypes.DslType), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslTypeFormat : GramFormatDefinition
{
	public GramDslTypeFormat() : base("DotGram DSL Type") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslVariable)]
[Name(GramClassificationTypes.DslVariable), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslVariableFormat : GramFormatDefinition
{
	public GramDslVariableFormat() : base("DotGram DSL Variable") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslFunction)]
[Name(GramClassificationTypes.DslFunction), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslFunctionFormat : GramFormatDefinition
{
	public GramDslFunctionFormat() : base("DotGram DSL Function") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslProperty)]
[Name(GramClassificationTypes.DslProperty), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslPropertyFormat : GramFormatDefinition
{
	public GramDslPropertyFormat() : base("DotGram DSL Property") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslNamespace)]
[Name(GramClassificationTypes.DslNamespace), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslNamespaceFormat : GramFormatDefinition
{
	public GramDslNamespaceFormat() : base("DotGram DSL Namespace") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslParameter)]
[Name(GramClassificationTypes.DslParameter), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslParameterFormat : GramFormatDefinition
{
	public GramDslParameterFormat() : base("DotGram DSL Parameter") { }
}

[Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = GramClassificationTypes.DslLabel)]
[Name(GramClassificationTypes.DslLabel), UserVisible(true), Order(Before = Priority.Default)]
sealed class GramDslLabelFormat : GramFormatDefinition
{
	public GramDslLabelFormat() : base("DotGram DSL Label") { }
}
