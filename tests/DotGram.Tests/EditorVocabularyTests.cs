using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the Visual Studio extension says about the notation's words and symbols is held to
/// what the generator knows, so that a change to the language cannot leave the editor behind
/// without a test failing.
/// </summary>
/// <remarks>
/// The editors' vocabulary (<see cref="GramVocabulary"/>) is part of the editor-neutral analysis,
/// so these run wherever the generator's tests run. The
/// parser keeps its words in no table: they are the strings it compares an identifier with,
/// and those are read out of its source here.
/// </remarks>
public sealed class EditorVocabularyTests
{
	/// <summary>A sample for every keyword, in which it stands where the parser reads it as one.</summary>
	static readonly Dictionary<string, string> KeywordSamples = new(StringComparer.Ordinal)
	{
		["using"]     = Namespaces,
		["namespace"] = Namespaces,
		["parse"]     = Publications,
		["find"]      = Publications,
		["as"]        = Publications,
		["with"]      = Publications,
		["public"]    = Publications,
		["internal"]  = Publications,
		["private"]   = Publications,
		["stream"]    = Publications,
		["bytes"]     = Publications,
		["yield"]     = Publications,
		["when"]      = Conditions,
		["is"]        = Conditions,
		["not"]       = Conditions,
		["and"]       = Conditions,
		["or"]        = Conditions,
		["switch"]    = "Text = 'a'\nField = switch @(1) { case 1: Text; default: none }\nparse Field",
		["case"]      = "Text = 'a'\nField = switch @(1) { case 1: Text; default: none }\nparse Field",
		["default"]   = "Text = 'a'\nField = switch @(1) { case 1: Text; default: none }\nparse Field",
		["recover"]   = "Rows = Row* recover eol\nRow = 'r' & eol\nparse Rows",
		["on"]        = "Name on fail \"Expected a name.\" = ['a'..'z']+\nparse Name",
		["fail"]      = "Name on fail \"Expected a name.\" = ['a'..'z']+\nparse Name",
		["context"]   = "context : @object\nstate : @int\nItem : @string = e: Word with state @(1) => @(e)\nWord = 'a'\nparse Item",
		["state"]     = "context : @object\nstate : @int\nItem : @string = e: Word with state @(1) => @(e)\nWord = 'a'\nparse Item",
	};

	const string Namespaces =
		"namespace Lexical\n{\n\tWord = ['a'..'z']+\n}\nnamespace Syntax\n{\n\tusing Lexical;\n\tPair = Word & ',' & Word\n}\nparse Syntax.Pair";

	const string Publications =
		"Item : @string = 'a'\nFeed : @string[] = Item*\nPoint = '.'\n" +
		"public parse Feed with (Point = ',') as Array\n" +
		"internal parse Feed as Nodes stream bytes yield : @string\n" +
		"private find Item as Items";

	const string Conditions =
		"Version = \"2000\" | \"2022\"\n" +
		"Test = 'a' & when (Version is \"2000\" or Version is \"2022\") and Version is not \"2008\"\n" +
		"parse Test with (Version = \"2000\") as ParseOld";

	[Fact]
	public void Every_word_the_parser_takes_is_a_keyword_the_editor_knows()
	{
		var taken = ParserWords();
		var known = GramVocabulary.Keywords.Keys.ToHashSet(StringComparer.Ordinal);

		Assert.True(
			taken.SetEquals(known),
			"Taken by the parser and not known to the editor: " + string.Join(", ", taken.Except(known).Order(StringComparer.Ordinal)) +
			"; known to the editor and taken by nothing: " + string.Join(", ", known.Except(taken).Order(StringComparer.Ordinal)));
	}

	[Fact]
	public void Completion_offers_every_keyword_and_every_built_in_rule()
	{
		var offered = GramVocabulary.CompletionWords.ToHashSet(StringComparer.Ordinal);

		Assert.Superset(ParserWords(), offered);
		Assert.Superset(GrammarBinder.BuiltIn.ToHashSet(StringComparer.Ordinal), offered);
	}

	[Fact]
	public void Every_keyword_has_a_sample_and_is_classified_as_a_keyword_in_it()
	{
		Assert.Equal(
			GramVocabulary.Keywords.Keys.Order(StringComparer.Ordinal),
			KeywordSamples.Keys.Order(StringComparer.Ordinal));

		foreach (var (word, sample) in KeywordSamples)
		{
			var document = GramLanguageService.Analyze(sample);

			Assert.True(
				document.Classifications.Any(span =>
					span.Kind == GramSyntaxKind.Keyword && sample.Substring(span.Position, span.Length) == word),
				$"'{word}' is not classified as a keyword in:\n{sample}");
		}
	}

	/// <summary>A contextual keyword is a keyword where it is read as one, and a name everywhere else.</summary>
	[Fact]
	public void A_contextual_keyword_is_an_identifier_outside_its_place()
	{
		Assert.Subset(GramVocabulary.Keywords.Keys.ToHashSet(StringComparer.Ordinal), GramVocabulary.ContextualKeywords.ToHashSet(StringComparer.Ordinal));

		foreach (var word in GramVocabulary.ContextualKeywords)
		{
			var sample   = $"Start = 'a' & {word}\n{word} = 'b'\nparse Start";
			var document = GramLanguageService.Analyze(sample);

			Assert.All(
				document.Classifications.Where(span => sample.Substring(span.Position, span.Length) == word),
				span => Assert.Equal(GramSyntaxKind.Identifier, span.Kind));
		}
	}

	[Fact]
	public void Every_built_in_rule_is_described_and_classified()
	{
		Assert.Equal(
			GrammarBinder.BuiltIn.Order(StringComparer.Ordinal),
			GramVocabulary.BuiltInDescriptions.Keys.Order(StringComparer.Ordinal));

		foreach (var name in GrammarBinder.BuiltIn)
		{
			var sample   = $"Start = 'a' & {name}\nparse Start";
			var document = GramLanguageService.Analyze(sample);
			var span     = document.Classifications.Single(span => span.Position == sample.IndexOf(name, StringComparison.Ordinal));

			Assert.Equal(GramSyntaxKind.Keyword, span.Kind);
			Assert.StartsWith("DotGram built-in rule " + name + ":", GramVocabulary.Describe(span.Kind, name), StringComparison.Ordinal);
		}
	}

	[Fact]
	public void Every_supplied_name_is_described()
	{
		Assert.Equal(
			SuppliedNames.All.Order(StringComparer.Ordinal),
			GramVocabulary.SuppliedDescriptions.Keys.Order(StringComparer.Ordinal));

		foreach (var name in SuppliedNames.All)
			Assert.StartsWith("DotGram supplied name " + name + ":", GramVocabulary.Describe(GramSyntaxKind.Identifier, name), StringComparison.Ordinal);
	}

	/// <summary>Every token the lexer can make has a classification, and every fixed spelling a description.</summary>
	[Fact]
	public void Every_symbol_is_classified_and_described()
	{
		var spelled = Enum.GetValues<TokenKind>().Where(static kind => kind.Spelling() is not null).ToArray();

		Assert.Equal(
			spelled.Order(),
			GramVocabulary.SymbolDescriptions.Keys.Order());

		foreach (var kind in spelled)
		{
			var spelling = kind.Spelling()!;
			var tokens   = GramLexer.Tokenize(spelling).Tokens;

			Assert.Equal(kind, tokens[0].Kind);
			Assert.StartsWith($"DotGram '{spelling}':", GramVocabulary.Symbol(spelling), StringComparison.Ordinal);
		}

		// Every kind of token is coloured as something: one the classifier does not know
		// would come out as the default, which is no colour at all.
		var sample   = "@using System;\nStart : @int on fail \"x\" = $q: 'a'i & \"b\"i & X{#q} & [\\p{Lu}] & ?=Y & ?!Y & Y ~ Y & d: [^ '0'..'9'] << 1 => @(1)\n" +
			"X = 'x'\nY = \"y\" | @Z\nparse Start";
		var document = GramLanguageService.Analyze(sample);

		foreach (var token in GramLexer.Tokenize(sample).Tokens.Where(static token => token.Kind != TokenKind.EndOfFile))
			Assert.Contains(document.Classifications, span =>
				span.Position <= token.Position && token.Position < span.Position + span.Length);
	}

	[Fact]
	public void Every_Unicode_category_is_described()
	{
		var abbreviations = Enum.GetValues<System.Globalization.UnicodeCategory>()
			.Select(UnicodeCategories.Abbreviation)
			.Concat(Enumerable.Range('A', 26).Select(static letter => ((char)letter).ToString()).Where(UnicodeCategories.Exists));

		foreach (var abbreviation in abbreviations)
		{
			var description = GramVocabulary.Category($"\\p{{{abbreviation}}}");

			Assert.NotNull(description);
			Assert.All(UnicodeCategories.Expand(abbreviation), name => Assert.Contains(name, description, StringComparison.Ordinal));
		}

		Assert.Null(GramVocabulary.Category("\\p{Xx}"));
	}

	/// <summary>Quick Info, completion, signature help and Go To Definition know every rule of the standard library.</summary>
	[Fact]
	public void Every_rule_of_the_standard_library_is_known()
	{
		var declared = GramCompiler.Read(StandardLibrary.Text, StandardLibrary.Scanner).File.Decls
			.OfType<Decl.Rule>()
			.ToArray();

		Assert.NotEmpty(declared);
		Assert.Equal(
			declared.Select(static rule => rule.Name).Order(StringComparer.Ordinal),
			GramVocabulary.StandardRules.Keys.Order(StringComparer.Ordinal));

		var lines = StandardLibrary.Text.Split('\n');

		foreach (var rule in declared)
		{
			var known = GramVocabulary.StandardRules[rule.Name];

			Assert.Equal(rule.Params.Count, known.ParameterCount);
			Assert.StartsWith("Std." + rule.Name, known.Signature, StringComparison.Ordinal);
			Assert.StartsWith("Std." + rule.Name, known.Description, StringComparison.Ordinal);
			Assert.Equal(rule.Name, lines[known.Line].Substring(known.Column, rule.Name.Length));

			var sample = $"Start = Std.{rule.Name}";

			Assert.Equal(known, GramVocabulary.QualifiedStandardRule(sample, sample.Length - rule.Name.Length, rule.Name.Length));
		}
	}

	[Theory]
	[InlineData("Start = Std.Integer",   12, true)]
	[InlineData("Start = MyStd.Integer", 14, false)]
	[InlineData("Start = Std.Nothing",   12, false)]
	[InlineData("Start = Integer",       8,  false)]
	public void A_standard_rule_is_one_written_after_Std(string text, int nameStart, bool found)
	{
		var end = nameStart;

		while (end < text.Length && char.IsLetter(text[end]))
			end++;

		Assert.Equal(found, GramVocabulary.QualifiedStandardRule(text, nameStart, end - nameStart) is not null);
	}

	[Theory]
	[InlineData("A = 'a' => @(par|)",                true)]
	[InlineData("A = 'a' => @(x + par|)",            true)]
	[InlineData("A = 'a' => @(x.par|)",              false)]
	[InlineData("A = 'a' => @par|",                  false)]
	public void Supplied_names_are_offered_where_C_sharp_names_a_value_on_its_own(string marked, bool offered)
	{
		var position = marked.IndexOf('|', StringComparison.Ordinal);
		var text     = marked.Remove(position, 1);
		var start    = text.LastIndexOf('@') + (text[text.LastIndexOf('@') + 1] == '(' ? 2 : 1);

		Assert.Equal(offered, GramVocabulary.SuppliedNamesApply(text, position, text[start..position]));
	}

	[Theory]
	[InlineData("A = 'a' => @Hold(parserInput, |)", true)]
	[InlineData("A = 'a' => @Ns.Hold(|)",           true)]
	[InlineData("A = 'a' => @Hold(B(|))",           false)]
	[InlineData("A = ('a' | |)",                    false)]
	[InlineData("A = 'a' & |",                      false)]
	public void Call_arguments_are_where_a_grammar_may_name_a_supplied_value(string marked, bool inside)
	{
		var position = marked.IndexOf('|', StringComparison.Ordinal);

		Assert.Equal(inside, GramVocabulary.InCallArguments(marked.Remove(position, 1), position));
	}

	/// <summary>
	/// The words the parser compares an identifier's text with: what <c>AtKeyword</c> and
	/// <c>TakeIfKeyword</c> are asked about, and every literal a token's <c>Value</c> is held to
	/// by <c>==</c>, a pattern or a switch.
	/// </summary>
	static HashSet<string> ParserWords()
	{
		var path  = Path.Combine(Root, "src", "DotGram", "Grammar", "Parsing", "GramParser.cs");
		var root  = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
		var words = new HashSet<string>(StringComparer.Ordinal);

		foreach (var literal in root.DescendantNodes().OfType<LiteralExpressionSyntax>())
		{
			if (!literal.IsKind(SyntaxKind.StringLiteralExpression) || !IsComparedWord(literal))
				continue;

			words.Add(literal.Token.ValueText);
		}

		return words;
	}

	static bool IsComparedWord(LiteralExpressionSyntax literal)
	{
		var text = literal.Token.ValueText;

		if (text.Length == 0 || !text.All(char.IsLetter))
			return false;

		// `AtKeyword("parse")`, `TakeIfKeyword("with")`.
		if (literal.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } &&
			invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "AtKeyword" or "TakeIfKeyword" })
			return true;

		// `Next.Value == "using"`.
		if (literal.Parent is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.EqualsExpression))
			return IsTokenText(binary.Left == literal ? binary.Right : binary.Left);

		// `Next.Value is "parse" or "find"`, and the arms of `Take().Value switch { "internal" => … }`.
		for (SyntaxNode? node = literal.Parent; node is PatternSyntax or SwitchExpressionArmSyntax; node = node.Parent)
		{
			if (node.Parent is IsPatternExpressionSyntax isPattern)
				return IsTokenText(isPattern.Expression);

			if (node.Parent is SwitchExpressionArmSyntax { Parent: SwitchExpressionSyntax switchExpression } arm && arm.Pattern == node)
				return IsTokenText(switchExpression.GoverningExpression);
		}

		return false;
	}

	/// <summary>A token's text: <c>Next.Value</c>, <c>Take().Value!</c>.</summary>
	static bool IsTokenText(ExpressionSyntax expression)
	{
		while (expression is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } suppressed)
			expression = suppressed.Operand;

		return expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Value" };
	}

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
