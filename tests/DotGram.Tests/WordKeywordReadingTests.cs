using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where a keyword may be recognized: a set of grammars and inputs on which the reading over
/// characters is the reference, and every lexer the generator can write must answer as it does.
/// </summary>
/// <remarks>
/// <para>
/// A lexer that reads a whole word first and then looks it up as a keyword, and one that carries
/// a path for each keyword in its automaton, are two ways to the same sentences: a keyword is
/// never taken from inside a longer word, and a literal that is not a whole word keeps its own
/// spelling. These tests hold the two to each other and to the reading over characters, so they
/// are written against the answers and name no lexer.
/// </para>
/// <para>
/// A keyword glued to the character before it (<c>1go</c>) is not asked here: over tokens the
/// character before a token is not looked at, which is a difference of the reading and not of a
/// lexer, and <c>TokenReadingFindingsTests</c> pins it.
/// </para>
/// </remarks>
public sealed class WordKeywordReadingTests
{
	const string Word = "wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']\n";

	const string Names =
		"namespace Lex\n{\n\ttrivia = none\n\tName = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']*\n}\n";

	const string NamesAndNumbers =
		"namespace Lex\n{\n\ttrivia = none\n\tName = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']*\n\tNum = ['0'..'9']+\n}\n";

	const string Numbers =
		"namespace Lex\n{\n\ttrivia = none\n\tNum = ['0'..'9']+\n}\n";

	// ── Keywords with and without a name token ──────────────────────────────────

	/// <summary>
	/// A keyword that is also a name, a keyword where no name covers the longer word, and a
	/// keyword before a digit: the longer word is never two tokens.
	/// </summary>
	[Theory]
	[InlineData(Word + Names + "Start = ((\"if\" | \"go\" | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "if;|iff;|go go;|goto;|if go x;|;|g;|ifgo;|if  go ;|go_;|go1;|_go;")]
	[InlineData(Word + NamesAndNumbers + "Start = ((\"if\" | \"go\" | Lex.Name | Lex.Num) & '.'?)* & ';' & eof\nparse Start\n", "if 1;|go1;|go 1;|1 go;|if1go;|go1go1;|12;|a1;")]
	[InlineData(Word + "Start = ((\"if\" | \"go\") & '.'?)* & ';' & eof\nparse Start\n", "if;|go;|ifgo;|if go;|gogo;|iff;|go if go;|g;|;|go  if ;")]
	[InlineData(Word + "Start = \"go\" & ['x'..'y'] & eof\nparse Start\n", "gox|go x|go|gox x|goy|go y")]
	[InlineData(Word + Numbers + "Start = \"go\" & Lex.Num & eof\nparse Start\n", "go1|go 1|go|1|go12|go 12")]
	[InlineData(Word + "namespace Lex\n{\n\ttrivia = none\n\tG = 'g' & 'o'\n}\nStart = ((\"go\" & '!' | Lex.G & ['x'..'y']) & '.'?)* & ';' & eof\nparse Start\n", "gox;|go!;|go x;|go!gox;|go!go!;")]
	public void A_keyword_is_taken_only_where_a_word_ends(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	/// <summary>A keyword that is a prefix of another keyword: the longer one wins only as a whole word.</summary>
	[Theory]
	[InlineData(Word + "Start = ((\"go\" | \"goto\") & '.'?)* & ';' & eof\nparse Start\n", "go;|goto;|gogoto;|go goto;|gotogo;|got;|goto goto;|gotoo;|goto go;|gogo;")]
	[InlineData(Word + Names + "Start = ((\"go\" | \"goto\" | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "go;|goto;|gogoto;|go goto;|gotogo;|got;|gotox;|go to;|gotoo;")]
	[InlineData(Word + "Start = ((\"a\" | \"ab\" | \"abc\") & '.'?)* & ';' & eof\nparse Start\n", "a;|ab;|abc;|abcd;|a b c;|aab;|ababc;|abc abc;|b;")]
	public void A_keyword_that_begins_another_is_told_from_it(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	// ── Case ────────────────────────────────────────────────────────────────────

	/// <summary>Keywords that fold their case, alone and beside a name that does not.</summary>
	[Theory]
	[InlineData(Word + "Start = ((\"go\"i | \"if\"i) & '.'?)* & ';' & eof\nparse Start\n", "GO;|Go gO;|gOgo;|goX;|IF go;|iF;|ifgo;|;|G;")]
	[InlineData(Word + Names + "Start = ((\"go\"i | \"if\"i | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "GO;|Go gO;|gOgo;|goX;|IF go;|iF;|ifgo;|gO1;|x;")]
	[InlineData(Word + Names + "Start = ((\"go\" | \"GO\"i | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "go;|GO;|Go;|gO go;|goo;|GOO;")]
	[InlineData(Word + "Start = ((\"kelvin\"i | \"sharp\"i) & '.'?)* & ';' & eof\nparse Start\n", "kelvin;|\u212Aelvin;|KELVIN;|sharp;|\u017Fharp;|SHARP;|\u017F\u017F;")]
	public void A_keyword_that_folds_its_case_is_matched_as_the_reading_over_characters_matches_it(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	// ── Literals that are not whole words ───────────────────────────────────────

	/// <summary>
	/// A keyword with a character that is not part of a word in it, a symbol, and a literal of
	/// several words: each keeps its own spelling.
	/// </summary>
	[Theory]
	[InlineData(Word + "Start = ((\"go-to\" | \"go\") & '.'?)* & ';' & eof\nparse Start\n", "go-to;|go -to;|go;|go-;|go-t;|go-to go;|gogo-to;|go- to;")]
	[InlineData(Word + Names + "Start = ((\"go-to\" | \"go\" | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "go-to;|go;|go-;|go-t;|go-to go;|goto;|go-too;")]
	[InlineData(Word + "Start = ((\"<=\" | \"<\" | \"=\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "<=;|<;|< =;|<<=;|if<=;|if <=;|ifif;|=<;|<=<=;")]
	[InlineData(Word + "Start = ((\"order by\" | \"order\") & '.'?)* & ';' & eof\nparse Start\n", "order by;|order  by;|orderby;|order byx;|order;|order order by;|order by order;|orderorder;")]
	[InlineData(Word + Names + "Start = ((\"order by\" | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "order by;|order  by;|orderby;|order byx;|order;|order x;|x order by;")]
	[InlineData(Word + "Start = ((\"@go\" | \"go\") & '.'?)* & ';' & eof\nparse Start\n", "@go;|go;|@go go;|@@go;|go@go;")]
	public void A_literal_that_is_not_a_whole_word_keeps_its_spelling(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	// ── What continues a word ───────────────────────────────────────────────────

	/// <summary>
	/// Digits and Unicode letters continue a word where the boundary says so and not where it
	/// does not.
	/// </summary>
	[Theory]
	[InlineData("wordboundary = [\\p{L} | \\p{Nd} | '_']\nStart = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "go;|go1;|go\u00E9;|go\u0434;|go \u00E9;|if\u0663;|go_;|go if;|go\u2022;")]
	[InlineData("wordboundary = ['a'..'z']\nStart = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "go;|go1;|go\u00E9;|go\u0434;|go 1;|go if;|gogo;")]
	[InlineData("wordboundary = [^ ' ' | ';']\nStart = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "go;|go go;|gogo;|go!;|go if;|go\t;")]
	[InlineData("wordboundary = ['a'..'z' | '0'..'9']\nnamespace Lex\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n\tNum = ['0'..'9']+\n}\nStart = ((\"go\" | Lex.Name | Lex.Num) & '.'?)* & ';' & eof\nparse Start\n", "go1;|go;|gogo;|1go;|go1go;|1go1;|go1go1;")]
	public void A_word_goes_on_through_what_the_boundary_names(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	/// <summary>
	/// Namespaces with their own boundary, and none: a keyword is asked about the boundary it was
	/// written under.
	/// </summary>
	[Theory]
	[InlineData("wordboundary = ['a'..'z' | '0'..'9']\nnamespace Inner\n{\n\twordboundary = ['a'..'z']\n\tPart = \"go\" & ['1'..'2'] & eof\n}\nStart = \"hi\" & ['1'..'2'] & eof | Inner.Part\nparse Start\n", "go1|go 1|hi1|hi 1|gogo1|higo1")]
	[InlineData("wordboundary = ['a'..'z']\nnamespace Inner\n{\n\twordboundary = none\n\tPart = \"go\"* & ';'\n}\nStart = Inner.Part & eof | \"go\" & '!' & eof\nparse Start\n", "gogo;|go!|gogo!|gogo;go")]
	[InlineData("wordboundary = ['a'..'z']\nnamespace Inner\n{\n\twordboundary = none\n\tPart = (\"ab\" | \"c\")* & ';'\n}\nStart = Inner.Part & eof | (\"ab\" | \"c\") & '!' & eof\nparse Start\n", "abc;|ab!|cab;|abc!|cc;")]
	[InlineData("Start = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "gogo;|go if;|goif;|go;|gogoif;")]
	[InlineData("Start = ((\"go\" | \"goto\") & '.'?)* & ';' & eof\nparse Start\n", "gogo;|goto;|gotogo;|go goto;")]
	public void A_keyword_follows_the_boundary_of_its_namespace(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	// ── Ends, looks and trivia ──────────────────────────────────────────────────

	/// <summary>A keyword at the end of the input, with and without something after it.</summary>
	[Theory]
	[InlineData(Word + "Start = \"go\" & eof\nparse Start\n", "go|gox|go |go1|g|goo|\u0067o|go_")]
	[InlineData(Word + Names + "Start = (\"go\" | Lex.Name) & eof\nparse Start\n", "go|gox|go |go1|g|goo|go_")]
	[InlineData(Word + "Start = \"go-to\" & eof\nparse Start\n", "go-to|go-too|go-t|go|go-to ")]
	public void A_keyword_at_the_end_of_the_input_is_read(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	/// <summary>A look at a keyword, ahead and against, is taken as the reading over characters takes it.</summary>
	[Theory]
	[InlineData(Word + "Start = ?=\"go\" & (\"go\" | \"gone\") & eof\nparse Start\n", "go|gone|gon|go go|gogo")]
	[InlineData(Word + "Start = ?!\"go\" & (\"gone\" | \"if\") & eof\nparse Start\n", "go|gone|if|gon|ifif")]
	[InlineData(Word + Names + "Start = ?!\"go\" & Lex.Name & eof\nparse Start\n", "go|gone|if|gox|g|go1")]
	[InlineData(Word + Names + "Start = ?=\"go\" & Lex.Name & eof\nparse Start\n", "go|gone|if|gox|g|go1")]
	[InlineData(Word + "Start = ((?!\"go\" & ['a'..'z']+ | \"go\") & '.'?)* & ';' & eof\nparse Start\n", "go;|gone;|if go;|ifgo;|gogo;")]
	public void A_look_over_a_keyword_agrees(string grammar, string inputs)
	{
		Agree(grammar, inputs);
	}

	/// <summary>Trivia between tokens, spaces, tabs and a comment: a keyword ends at it.</summary>
	[Theory]
	[InlineData("trivia = { (' ' | '\\t' | '\\n')* }\n", Word + "Start = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "go\tgo;|go\ngo;|go\n;|go\tif ;|go\u00A0go;|gogo;|go;")]
	[InlineData("trivia = { (' ' | '#' & [^ '\\n']* & '\\n')* }\n", Word + "Start = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", "go #x\nif;|go#x\nif;|go# x\n;|go #\ngo;|go#;|gogo #\n;")]
	[InlineData("trivia = { (' ' | '#' & [^ '\\n']* & '\\n')* }\n", Word + Names + "Start = ((\"go\" | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", "go#x\nif;|go #x\ngo;|go#\nx;|x#\ngo;")]
	public void A_keyword_ends_at_trivia(string trivia, string grammar, string inputs)
	{
		Agree(trivia, grammar, inputs);
	}

	// ── Which lexer is written ──────────────────────────────────────────────────

	/// <summary>
	/// The keyword table is written where the grammar has keywords it can hold, and only when asked:
	/// the answers above would be the same if it quietly were not.
	/// </summary>
	[Theory]
	[InlineData(Word + "Start = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", true)]
	[InlineData(Word + "Start = ((\"go\"i | \"if\"i) & '.'?)* & ';' & eof\nparse Start\n", true)]
	[InlineData(Word + Names + "Start = ((\"go\"i | Lex.Name) & '.'?)* & ';' & eof\nparse Start\n", true)]
	[InlineData(Word + "Start = ((\"go-to\" | \"go\") & '.'?)* & ';' & eof\nparse Start\n", true)]
	[InlineData(Word + "Start = ((\"\u0434\u0430\"i | \"go\") & '.'?)* & ';' & eof\nparse Start\n", true)]
	[InlineData(Word + "Start = ((\"<=\" | \"<\") & '.'?)* & ';' & eof\nparse Start\n", false)]
	[InlineData(Word + "Start = ((\"\u0434\u0430\"i) & '.'?)* & ';' & eof\nparse Start\n", false)]
	[InlineData("Start = ((\"go\" | \"if\") & '.'?)* & ';' & eof\nparse Start\n", false)]
	public void The_keyword_table_is_written_where_there_are_keywords_to_hold(string grammar, bool written)
	{
		grammar = "trivia = { ' '* }\n" + grammar;

		Assert.Equal(written, Generated(grammar, table: true).Contains("Words_Keys", StringComparison.Ordinal));
		Assert.DoesNotContain("Words_Keys", Generated(grammar, table: false), StringComparison.Ordinal);
	}

	/// <summary>
	/// A keyword the table does not hold keeps its path in the automaton, and the table has the rest.
	/// The table is of one way of comparing: where the keywords are of both, the larger group is held
	/// and the other stays.
	/// </summary>
	[Fact]
	public void A_keyword_the_table_cannot_hold_stays_in_the_automaton()
	{
		var grammar = "trivia = { ' '* }\n" + Word + "Start = ((\"go-to\" | \"go\" | \"if\"i) & '.'?)* & ';' & eof\nparse Start\n";
		var source  = Generated(grammar, table: true);

		Assert.Contains("\"IF\"", source, StringComparison.Ordinal);
		Assert.DoesNotContain("\"GO\"", source, StringComparison.Ordinal);
		Assert.DoesNotContain("\"go\"", source, StringComparison.Ordinal);
	}

	// ── The harness ─────────────────────────────────────────────────────────────

	static string Generated(string grammar, bool table)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName = "Grammar", Lexical = true, KeywordTable = table, Direct = true,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		return result.Sources[0].Text;
	}

	static void Agree(string grammar, string inputs)
	{
		Agree("trivia = { ' '* }\n", grammar, inputs);
	}

	static void Agree(string trivia, string grammar, string inputs)
	{
		grammar = trivia + grammar;

		var characters = Renderings(grammar, lexical: false, table: false).ToArray();

		// Over tokens twice: the lexer that carries a path for each keyword, and the one that reads a
		// word whole and looks it up.
		var tokens = Renderings(grammar, lexical: true, table: false)
			.Concat(Renderings(grammar, lexical: true, table: true))
			.ToArray();

		var disagreed = new List<string>();

		foreach (var input in inputs.Split('|'))
		{
			var expected = EmittedCode.Match(characters[0], "Grammar", "TryParseStart", input).IsSuccess;

			var answers = string.Join(
				", ",
				characters.Concat(tokens).Select(one => EmittedCode.Match(one, "Grammar", "TryParseStart", input).IsSuccess ? "read" : "refused"));

			if (answers != string.Join(", ", Enumerable.Repeat(expected ? "read" : "refused", characters.Length + tokens.Length)))
				disagreed.Add($"\"{Escaped(input)}\": over characters, then over tokens: {answers}");
		}

		Assert.True(disagreed.Count == 0, string.Join("\n", disagreed));
	}

	static string Escaped(string text)
	{
		return text.Replace("\n", "\\n").Replace("\t", "\\t");
	}

	/// <summary>The engine, and the direct reader on each carrier it is read on.</summary>
	static IEnumerable<Assembly> Renderings(string grammar, bool lexical, bool table)
	{
		yield return Build(grammar, lexical, table, direct: false, CarrierKind.Auto);

		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
			yield return Build(grammar, lexical, table, direct: true, carrier);
	}

	static Assembly Build(string grammar, bool lexical, bool table, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName = "Grammar", Lexical = lexical, KeywordTable = table, Direct = direct, Carrier = carrier,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.NotCut));

		return EmittedCode.Compile(result.Sources[0].Text);
	}
}
