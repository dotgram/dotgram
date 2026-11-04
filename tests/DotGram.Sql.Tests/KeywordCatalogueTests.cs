using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Sql.PostgreSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// That the PostgreSQL grammar's four word lists are the keyword catalogue's classes, word for word.
/// </summary>
/// <remarks>
/// Two things say which words are key words and where each may be a name: <c>KeywordCatalogue</c>, written from
/// the server's <c>kwlist.h</c>, and the lexical rules of <c>PostgreSql.gram</c>, which decide what a position
/// reads. Nothing writes the second from the first, so this holds them to one another: a word the grammar refuses
/// as a name and the catalogue calls unreserved would be a name the server reads and this does not, and the other
/// way round a key word read where the server refuses it.
/// </remarks>
public sealed class KeywordCatalogueTests
{
	static readonly Dictionary<string, Func<(string Word, KeywordCatalogue.Category Category, bool BareLabel), bool>> Lists = new(StringComparer.Ordinal)
	{
		["ReservedKeyword"]         = static one => one.Category == KeywordCatalogue.Category.Reserved,
		["TypeFunctionNameKeyword"] = static one => one.Category == KeywordCatalogue.Category.TypeOrFunctionName,
	};

	[Fact]
	public void The_catalogue_has_the_server_counts()
	{
		var words = KeywordCatalogue.Words;

		Assert.Equal(494, words.Count);
		Assert.Equal(78,  words.Count(static one => one.Category == KeywordCatalogue.Category.Reserved));
		Assert.Equal(23,  words.Count(static one => one.Category == KeywordCatalogue.Category.TypeOrFunctionName));
		Assert.Equal(63,  words.Count(static one => one.Category == KeywordCatalogue.Category.ColumnName));
		Assert.Equal(330, words.Count(static one => one.Category == KeywordCatalogue.Category.Unreserved));
		Assert.Equal(39,  words.Count(static one => !one.BareLabel));
		Assert.Equal(words.Count, words.Select(static one => one.Word).Distinct(StringComparer.Ordinal).Count());
	}

	[Theory]
	[InlineData("ReservedKeyword")]
	[InlineData("TypeFunctionNameKeyword")]
	public void The_grammar_lists_exactly_the_catalogue_class(string rule)
	{
		var expected = KeywordCatalogue.Words.Where(Lists[rule]).Select(static one => one.Word).Order(StringComparer.Ordinal).ToList();
		var written  = WordsOf(rule).Order(StringComparer.Ordinal).ToList();

		Assert.Equal(expected, written);
	}

	/// <summary>The words of a lexical rule of the grammar: every <c>"WORD"i</c> between its name and the next rule.</summary>
	static IEnumerable<string> WordsOf(string rule)
	{
		var text  = File.ReadAllText(Path.Combine(SourceRoot.Directory, "src", "DotGram.Sql", "PostgreSql", "PostgreSql.gram"));
		var start = Regex.Match(text, @"^\t" + rule + @"\s*$", RegexOptions.Multiline);

		Assert.True(start.Success, $"{rule} is not a rule of the grammar");

		var end  = Regex.Match(text[(start.Index + start.Length)..], @"^\t[A-Za-z]+\s*(=|:)|^}", RegexOptions.Multiline);
		var body = text.Substring(start.Index + start.Length, end.Success ? end.Index : text.Length - start.Index - start.Length);

		return Regex.Matches(body, "\"([A-Z_0-9]+)\"i").Select(static one => one.Groups[1].Value);
	}
}
