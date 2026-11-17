using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Language;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The extension's TextMate grammar, which colours a file before the server answers, says what the
/// language core says the words of the notation are: a word added to the parser cannot be left out
/// of the first paint without a test failing.
/// </summary>
public sealed class TextMateTests
{
	static readonly Lazy<JsonElement> Repository = new(static () =>
		JsonDocument.Parse(File.ReadAllText(Path.Combine(LspClient.SourceRoot(), "editors", "vscode", "syntaxes", "gram.tmLanguage.json")))
			.RootElement.GetProperty("repository").Clone());

	[Fact]
	public void The_keywords_are_the_vocabularys()
	{
		Assert.Equal(GramVocabulary.Keywords.Keys.Order(StringComparer.Ordinal), Words("keywords"));
	}

	[Fact]
	public void The_built_in_rules_are_the_binders()
	{
		Assert.Equal(GrammarBinder.BuiltIn.Order(StringComparer.Ordinal), Words("builtins"));
	}

	/// <summary>
	/// The symbols of the notation — choice, sequence, the predicates, repetition, ranges — are coloured apart
	/// from names and keywords in every default theme, which colours <c>keyword.control</c> and not
	/// <c>keyword.operator</c>: the first paint takes every symbol the core classifies as one, and the
	/// semantic token is mapped to the same scope; an operator such as <c>=</c> stays an operator in both.
	/// </summary>
	[Fact]
	public void The_symbols_of_the_notation_are_painted_apart_from_names()
	{
		const string Grammar = "Start = ('a' | 'b')* & ~'c' ?= 'd' ?! 'e' ['a'..'z']+ 'f'? ^ <1> Rest\nRest => 'g' - 'h'\nparse Start\n";

		var symbols   = new Regex("^(?:" + Repository.Value.GetProperty("symbols").GetProperty("match").GetString() + ")$");
		var operators = new Regex("^(?:" + Repository.Value.GetProperty("operators").GetProperty("match").GetString() + ")$");
		var scope     = Repository.Value.GetProperty("symbols").GetProperty("name").GetString()!;
		var mapped    = Manifest.Value.GetProperty("contributes").GetProperty("semanticTokenScopes")[0].GetProperty("scopes")
			.GetProperty("gramSpecialSymbol")[0].GetString();
		var document  = GramLanguageService.Analyze(Grammar);
		var special   = document.Classifications.Where(static item => item.Kind == GramSyntaxKind.SpecialSymbol).ToArray();
		var plain     = document.Classifications.Where(static item => item.Kind == GramSyntaxKind.Operator).ToArray();

		Assert.True(special.Length >= 10, $"{special.Length} symbols classified");
		Assert.NotEmpty(plain);

		foreach (var item in special)
			Assert.Matches(symbols, Grammar.Substring(item.Position, item.Length));

		foreach (var item in plain)
			Assert.Matches(operators, Grammar.Substring(item.Position, item.Length));

		Assert.StartsWith("keyword.control.", scope, StringComparison.Ordinal);
		Assert.Equal(scope, mapped);

		// A keyword is not: VS Code paints the standard keyword token as keyword.control unless told
		// otherwise, so it is mapped to the first paint's scope for keywords.
		var keywords = Manifest.Value.GetProperty("contributes").GetProperty("semanticTokenScopes")[0].GetProperty("scopes")
			.GetProperty("keyword")[0].GetString();

		Assert.Equal(Repository.Value.GetProperty("keywords").GetProperty("name").GetString(), keywords);
		Assert.False(keywords!.StartsWith("keyword.control.", StringComparison.Ordinal), keywords);
	}

	static readonly Lazy<JsonElement> Manifest = new(static () =>
		JsonDocument.Parse(File.ReadAllText(Path.Combine(LspClient.SourceRoot(), "editors", "vscode", "package.json"))).RootElement.Clone());

	/// <summary>The words of a rule written <c>\b(?:one|two|…)\b</c>, in order.</summary>
	static string[] Words(string rule)
	{
		var match = Repository.Value.GetProperty(rule).GetProperty("match").GetString()!;
		var words = Regex.Match(match, @"^\\b\(\?:([A-Za-z|]+)\)\\b$");

		Assert.True(words.Success, $"The '{rule}' rule is not an alternation of words: {match}");

		return [.. words.Groups[1].Value.Split('|').Order(StringComparer.Ordinal)];
	}
}
