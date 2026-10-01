using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Published rules the lexer holds, readings from a position over tokens, and the choices a
/// scanner reads — attacked from the outside, against the reference interpreter, the reading
/// over characters and the spec's own sentences.
/// </summary>
/// <remarks>
/// The facts that hold are kept as guards. The ones that do not are written as what should be
/// true, and each says which side is wrong.
/// </remarks>
public sealed class TokenReadingFindingsTests
{
	// ── Published rules the lexer holds ─────────────────────────────────────────

	/// <summary>
	/// Every shape of a published rule that turns out to be one token generates, and the
	/// engine, the direct reader and the reading over characters answer it the same.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = 'a'\nparse Start\n", "TryParseStart", "a| a |aa| |")]
	[InlineData("trivia = { ' '* }\nStart = \"ab\"\nparse Start\n", "TryParseStart", "ab| ab |a b|")]
	[InlineData("trivia = { ' '* }\nStart = ['a'..'c']\nparse Start\n", "TryParseStart", "a| b |ab|d")]
	[InlineData("trivia = { ' '* }\nStart = (\"a\" | \"b\")?\nparse Start\n", "TryParseStart", "| |a|ab")]
	[InlineData("trivia = { ' '* }\nKw = (\"if\" | \"do\")\nStart = Kw & Kw\nparse Start\nparse Kw\n", "TryParseKw", "if| do |ifdo|if do")]
	[InlineData("trivia = { ' '* }\nX = (\"a\" | \"ab\")\nY = \"abc\"\nStart = X & Y\nparse Start\nparse X\n", "TryParseX", "a|ab|abc")]
	[InlineData("trivia = { ' '* }\nStart = (\"a\" | \"b\") & eof\nparse Start\n", "TryParseStart", "a|a |ab")]
	public void A_published_rule_the_lexer_holds_reads_as_it_does_over_characters(string grammar, string method, string inputs)
	{
		var chars  = Built(grammar, lexical: false, direct: true);
		var engine = Built(grammar, lexical: true,  direct: false);
		var direct = Built(grammar, lexical: true,  direct: true);

		foreach (var input in inputs.Split('|'))
		{
			var expected = EmittedCode.Match(chars, "Grammar", method, input).IsSuccess;

			Assert.Equal(expected, EmittedCode.Match(engine, "Grammar", method, input).IsSuccess);
			Assert.Equal(expected, EmittedCode.Match(direct, "Grammar", method, input).IsSuccess);
		}
	}

	/// <summary>
	/// A rule of a namespace with no trivia reads none around itself: published, it refuses a
	/// space before or after it over characters. Over tokens the lexer skips that space before
	/// the published rule's one token test ever runs, so the same publication accepts it.
	/// </summary>
	/// <remarks>
	/// Red: over tokens <c>" if"</c> and <c>"12 "</c> are read. The split keeps the rule as a
	/// test of its kinds and does not keep its namespace's trivia, so `Lexical` changes which
	/// texts the publication accepts — which §6.8 allows only where a rule's answer stands.
	/// Either the whole reading over kinds refuses trivia around such a publication, or the
	/// split refuses the grammar (GRAM5004).
	/// </remarks>
	[Theory]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tKw = (\"if\" | \"do\")\n}\nStart = Lex.Kw & Lex.Kw\nparse Start\nparse Lex.Kw\n", "TryParseKw", " if")]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tKw = (\"if\" | \"do\")\n}\nStart = Lex.Kw & Lex.Kw\nparse Start\nparse Lex.Kw\n", "TryParseKw", "if ")]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tDigits = ['0'..'9']+\n}\nStart = Lex.Digits & ',' & Lex.Digits\nparse Start\nparse Lex.Digits\n", "TryParseDigits", " 12")]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tDigits = ['0'..'9']+\n}\nStart = Lex.Digits & ',' & Lex.Digits\nparse Start\nparse Lex.Digits\n", "TryParseDigits", "12 ")]
	public void A_published_rule_of_a_namespace_without_trivia_reads_none_over_tokens(string grammar, string method, string input)
	{
		Assert.False(EmittedCode.Match(Built(grammar, lexical: false, direct: true), "Grammar", method, input).IsSuccess);

		foreach (var direct in new[] { false, true })
			Assert.False(EmittedCode.Match(Built(grammar, lexical: true, direct: direct), "Grammar", method, input).IsSuccess);
	}

	/// <summary>
	/// A published terminal that builds a value, over tokens.
	/// </summary>
	/// <remarks>
	/// Red, and not new: the generated parser does not compile (CS1501, the whole recognizer
	/// called with arguments it does not take). The generator should write a parser or refuse
	/// the split, not hand the consumer a file that fails their build.
	/// </remarks>
	[Fact]
	public void A_published_terminal_that_builds_a_value_compiles_over_tokens()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tNum : @int = ['0'..'9']+ => @int.Parse(parserText)\n}\n" +
			"Start = Lex.Num & ',' & Lex.Num\nparse Start\nparse Lex.Num\n",
			Options(lexical: true, direct: true));

		EmittedCode.Quiet(result.Diagnostics);
		EmittedCode.Compile(result.Sources[0].Text);
	}

	// ── Readings from a position over tokens ────────────────────────────────────

	/// <summary>
	/// Every form that begins where it is told, at every position and every window of every
	/// short input, answers consistently over tokens: the bool form moves <c>at</c> to where the
	/// match form says the value ends, nothing has a negative length, and nothing is placed
	/// outside the window it was read in. The engine and the direct reader say the same.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = ('b' | 'b' & 'c')?\nOther = 'a' & 'c'\nparse Start\nparse Other\n")]
	[InlineData("trivia = { ' '* }\nStart = (\"ab\" | 'a')* & ('z' | 'z' & 'q')?\nOther = 'c' & 'b'\nparse Start\nparse Other\n")]
	[InlineData("trivia = { ' '* }\nStart = 'a'? & 'b'? & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n")]
	public void Every_reading_from_a_position_over_tokens_answers_consistently(string grammar)
	{
		var engine = Built(grammar, lexical: true, direct: false);
		var direct = Built(grammar, lexical: true, direct: true);

		foreach (var input in Inputs("ab #", 3))
			for (var at = 0; at <= input.Length; at++)
				foreach (var length in new int?[] { null }.Concat(Enumerable.Range(0, input.Length - at + 1).Select(static one => (int?)one)))
				{
					var said  = $"\"{input}\" at {at}, length {length?.ToString() ?? "none"}";
					var match = EmittedCode.Positioned(engine, "Grammar", "TryParseStart", input, at, length);
					var read  = EmittedCode.Answered(engine, "Grammar", "TryParseStart", input, at, length);

					Assert.True(match.IsSuccess == read.Read, said);
					Assert.Equal(match, EmittedCode.Positioned(direct, "Grammar", "TryParseStart", input, at, length));
					Assert.Equal((read.Read, read.At), Moved(EmittedCode.Answered(direct, "Grammar", "TryParseStart", input, at, length)));

					if (!match.IsSuccess)
					{
						Assert.True(read.At == at, said);

						continue;
					}

					Assert.True(match.Length >= 0, said);
					Assert.True(match.Position >= at && match.Position + match.Length <= (length is { } seen ? at + seen : input.Length), said);
					Assert.True(read.At == match.Position + match.Length, said);
				}
	}

	/// <summary>
	/// <c>eof</c> is the end of the whole input, and inside a window the end of the window
	/// (§6.3) — not the place over tokens where the lexer met a character no token begins with.
	/// </summary>
	/// <remarks>
	/// Red, and not new: over tokens <c>"a#"</c> read from 0 answers a reading of <c>a</c>,
	/// because the tokens end at <c>#</c> and the syntactic half's end of input is the end of
	/// the tokens. Over characters, and by §6.3, there is no <c>eof</c> there.
	/// </remarks>
	[Theory]
	[InlineData(null)]
	[InlineData(2)]
	public void Eof_over_tokens_is_not_where_the_tokens_stopped(int? length)
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a'* & ('z' | 'z' & 'q')? & eof\nOther = 'c' & 'b'\nparse Start\nparse Other\n";

		Assert.False(EmittedCode.Positioned(Built(grammar, lexical: false, direct: true), "Grammar", "TryParseStart", "a#", 0, length).IsSuccess);

		foreach (var direct in new[] { false, true })
			Assert.False(EmittedCode.Positioned(Built(grammar, lexical: true, direct: direct), "Grammar", "TryParseStart", "a#", 0, length).IsSuccess);
	}

	/// <summary>
	/// A reading from a position is not refused by a character it never read (§6.3): one no
	/// token begins with, standing before the position, leaves what follows it readable — as
	/// the window form over the same text already reads it.
	/// </summary>
	/// <remarks>
	/// Red, and not new: the form from a position cuts the whole input, the tokens end at the
	/// <c>#</c> before <c>at</c>, no token is left at or after it, and the answer is Starved.
	/// </remarks>
	[Fact]
	public void A_character_no_token_begins_with_before_the_position_does_not_refuse_the_reading()
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a' & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n";

		foreach (var direct in new[] { false, true })
		{
			var assembly = Built(grammar, lexical: true, direct: direct);

			Assert.Equal((true, (string?)null, 1L, 1L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "#a", 1, 1));
			Assert.Equal((true, (string?)null, 1L, 1L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "#a", 1));
		}
	}

	/// <summary>
	/// The engine and the direct reader are two renderings of one grammar read over tokens, and
	/// answer the same whole input the same.
	/// </summary>
	/// <remarks>
	/// Red, and not new: over tokens the engine reads <c>b</c> (it gives the <c>b</c> that
	/// <c>R2</c> read back to <c>'b'</c>, as §11 does over characters) and the direct reader
	/// refuses it (<c>R2</c>'s answer stands, as §4 says it does over kinds). By §4 the direct
	/// reader is the one that is right.
	/// </remarks>
	[Theory]
	[InlineData("trivia = { ' '* }\nR1 = (R2 & 'b')\nR2 = 'b'?\nparse R1\n", "TryParseR1", "b")]
	[InlineData("trivia = { ' '* }\nR1 = ('c' | R2 & ['a'..'b'])\nR2 = ('b' | 'a')?\nparse R1\n", "TryParseR1", "b")]
	public void The_engine_and_the_direct_reader_over_tokens_agree(string grammar, string method, string input)
	{
		Assert.Equal(
			EmittedCode.Match(Built(grammar, lexical: true, direct: true), "Grammar", method, input).IsSuccess,
			EmittedCode.Match(Built(grammar, lexical: true, direct: false), "Grammar", method, input).IsSuccess);
	}

	// ── Choices a scanner reads ─────────────────────────────────────────────────

	/// <summary>
	/// Inside atomic braces a choice commits the first alternative that matches, so an
	/// alternative that cannot fail ends it — wherever it stands, however deep, and whatever
	/// repeats it. Each shape compiles without a warning and reads as §11 says.
	/// </summary>
	[Theory]
	[InlineData("('b'? | 'a')")]
	[InlineData("('a' | 'b'? | 'c')")]
	[InlineData("('a' | 'b'?)")]
	[InlineData("(('a' | 'b'?) | 'c')")]
	[InlineData("(('b'? | 'a') | 'c')")]
	[InlineData("('a' | ('b' | 'c'?))")]
	[InlineData("('b'{0,2} | 'a')")]
	[InlineData("('b'* & 'c'? | 'a')")]
	[InlineData("({ 'b'? } | 'a')")]
	[InlineData("(('b' | 'c')* | 'a')")]
	[InlineData("(none | 'a')")]
	[InlineData("('a' | 'b' | 'c'?)?")]
	public void A_braced_choice_with_an_alternative_that_cannot_fail_reads_as_the_semantics_say(string shape)
	{
		foreach (var spaced in new[] { false, true })
		{
			var grammar = (spaced ? "trivia = { ' '* }\n" : "") + $"Start = 'x' & R1 & 'y'\nR1 = {{ {shape} }}\nparse Start\n";

			foreach (var lexical in new[] { false, true })
				foreach (var direct in new[] { false, true })
					AgreesWithTheSemantics(grammar, lexical, direct, "xy|xay|xby|xbby|xcy|xbcy|xaay|xa y|x b y|xbay|xaby");
		}
	}

	/// <summary>
	/// Without the braces a rule gives back (§4): <c>('b'? | 'a')</c> reads nothing on
	/// <c>a</c>, and when what follows fails the choice is reopened and <c>'a'</c> is taken.
	/// </summary>
	/// <remarks>
	/// Red, and not new: the rule is compiled as a scanner, which commits the first reading of
	/// each choice. Scanning an unbraced rule is licensed by its alternatives being exclusive
	/// (Machine.Scan.cs, <c>Exclusive</c>), and exclusive first sets are not enough where an
	/// earlier alternative reads nothing at all: it matches in front of every later one. The
	/// CS0162 this shape used to raise was the compiler pointing at exactly that dead
	/// alternative; ending the choice there silenced the warning and kept the wrong reading.
	/// </remarks>
	[Theory]
	[InlineData("Start = R1\nR1 = ('b'? | 'a'i)\nparse Start\n", "a")]
	[InlineData("trivia = { ' '* }\nStart = R1\nR1 = ('b'? | 'a'i)\nparse Start\n", "a")]
	[InlineData("trivia = { ' '* }\nStart = 'x' & R1 & 'y'\nR1 = ('b'? | 'a')\nparse Start\n", "xay")]
	[InlineData("trivia = { ' '* }\nStart = 'x' & R1 & 'y'\nR1 = ('a' | 'b'? | 'c')\nparse Start\n", "xcy")]
	[InlineData("trivia = { ' '* }\nStart = 'x' & R1 & 'y'\nR1 = (['a'..'b']* | 'c')\nparse Start\n", "xcy")]
	[InlineData("trivia = { ' '* }\nStart = 'x' & R1 & 'y'\nR1 = ((?! 'a') | 'b')\nparse Start\n", "xby")]
	[InlineData("trivia = { ' '* }\nStart = ['a'..'b']? | (R2 | ['a'..'b'])\nR2 = ('c'* | (\"b\"i & \"a\"i))\nparse Start\n", "ba")]
	public void An_unbraced_choice_whose_earlier_alternative_reads_nothing_still_gives_back(string grammar, string input)
	{
		foreach (var direct in new[] { false, true })
			AgreesWithTheSemantics(grammar, lexical: false, direct, input);
	}

	/// <summary>
	/// A bounded repetition that fails part of the way, inside a scanned choice, gives back
	/// the characters it read before the next alternative is tried.
	/// </summary>
	/// <remarks>
	/// Red, and not new: on <c>xby</c> <c>'b'{2}</c> reads one <c>b</c>, fails, and the scanner
	/// goes on to <c>'a'</c> and then out of the optional without putting the position back,
	/// so <c>R1</c> answers having read the <c>b</c> and <c>'y'</c> matches.
	/// </remarks>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_bounded_repetition_that_fails_part_way_in_a_scanned_choice_gives_back(bool spaced)
	{
		var grammar = (spaced ? "trivia = { ' '* }\n" : "") + "Start = 'x' & R1 & 'y'\nR1 = { ('b'{2} | 'a')? }\nparse Start\n";

		foreach (var direct in new[] { false, true })
			AgreesWithTheSemantics(grammar, lexical: false, direct, "xby|xbby|xay|xy");
	}

	/// <summary>
	/// A choice in a rule compiled flat, after an alternative that reads nothing.
	/// </summary>
	/// <remarks>
	/// Red, and not new: the flat recognizer assigns a <c>turn</c> local it never declared
	/// (CS0103) — the same dead alternative as the scanner's CS0162, met on the flat path.
	/// </remarks>
	[Theory]
	[InlineData("{ (\"ab\"? | 'a' | \"abc\") }")]
	[InlineData("{ (('a' | 'b')? | ('c' | 'a')?) }")]
	public void A_flat_choice_after_an_alternative_that_reads_nothing_compiles(string shape)
	{
		var result = GramCompiler.Compile($"Start = 'x' & R1 & 'y'\nR1 = {shape}\nparse Start\n", Options(lexical: false, direct: true));

		EmittedCode.Quiet(result.Diagnostics);
		EmittedCode.Compile(result.Sources[0].Text);
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	static GramCompilerOptions Options(bool lexical, bool direct)
	{
		return new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = lexical, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		};
	}

	static Assembly Built(string grammar, bool lexical, bool direct)
	{
		var result = GramCompiler.Compile(grammar, Options(lexical, direct));

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.NotCut));

		return EmittedCode.Compile(result.Sources[0].Text);
	}

	static void AgreesWithTheSemantics(string grammar, bool lexical, bool direct, string inputs)
	{
		var assembly = Built(grammar, lexical, direct);
		var graph    = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
		var start    = graph.Rules.First(static rule => rule.Name == "Start");

		foreach (var input in inputs.Split('|'))
			Assert.True(
				ReferenceInterpreter.Parses(graph, start, input) == EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess,
				$"lexical {lexical}, direct {direct}, \"{input}\":\n{grammar}");
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var all = new List<string> { "" };

		for (var length = 1; length <= longest; length++)
			all.AddRange(all.Where(one => one.Length == length - 1).SelectMany(one => alphabet.Select(c => one + c)).ToList());

		return all;
	}

	static (bool Read, int At) Moved((bool Read, object? Value, int At) answer)
	{
		return (answer.Read, answer.At);
	}
}
