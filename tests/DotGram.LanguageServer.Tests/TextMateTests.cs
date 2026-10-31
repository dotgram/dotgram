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

	/// <summary>The words of a rule written <c>\b(?:one|two|…)\b</c>, in order.</summary>
	static string[] Words(string rule)
	{
		var match = Repository.Value.GetProperty(rule).GetProperty("match").GetString()!;
		var words = Regex.Match(match, @"^\\b\(\?:([A-Za-z|]+)\)\\b$");

		Assert.True(words.Success, $"The '{rule}' rule is not an alternation of words: {match}");

		return [.. words.Groups[1].Value.Split('|').Order(StringComparer.Ordinal)];
	}
}
