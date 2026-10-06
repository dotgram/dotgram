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
	/// space before or after it over characters, and over tokens — where the lexer skips that
	/// space — its publication refuses trivia before its first token or after its last.
	/// </summary>
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
	/// A published terminal that builds a value keeps the grammar over characters (GRAM5004),
	/// where the parser compiles and reads the value.
	/// </summary>
	/// <remarks>
	/// Over tokens the value of such a terminal is read again from its token where a rule calls
	/// it, and its own publication has no such read: the parser written for it did not compile
	/// (CS1501).
	/// </remarks>
	[Fact]
	public void A_published_terminal_that_builds_a_value_is_read_over_characters()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tNum : @int = ['0'..'9']+ => @int.Parse(parserText)\n}\n" +
			"Start = Lex.Num & ',' & Lex.Num\nparse Start\nparse Lex.Num\n",
			Options(lexical: true, direct: true));

		Assert.Contains(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);
		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.NotCut));

		Assert.Equal(12, EmittedCode.Match(EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseNum", "12").Value);
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
	/// Over tokens <c>"a#"</c> read from 0 answered a reading of <c>a</c>: the cutting ended at
	/// <c>#</c>, and the syntactic half took the end of the tokens for the end of the input. A
	/// reading from a position or in a window now cuts past such a character, which becomes a
	/// token of no kind: no terminal reads it, <c>any</c> reads it as one character, and it is
	/// not the end of the text. Every rendering answers as the reading over characters does, in
	/// the match forms and the ones that move <c>at</c>.
	/// </remarks>
	[Theory]
	[InlineData("a#", 0, null)]
	[InlineData("a#", 0, 2)]
	[InlineData("a #", 0, null)]
	[InlineData("a #", 0, 3)]
	[InlineData("a #", 0, 2)]
	[InlineData("a#a", 0, null)]
	[InlineData("a#a", 2, null)]
	[InlineData("#a", 1, null)]
	[InlineData("#a", 1, 1)]
	[InlineData("aa", 0, null)]
	[InlineData("aaz", 0, null)]
	[InlineData("a z#", 0, null)]
	[InlineData("a z#", 0, 3)]
	[InlineData("a b", 0, null)]
	public void Eof_over_tokens_is_not_where_the_tokens_stopped(string input, int at, int? length)
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a'* & ('z' | 'z' & 'q')? & eof\nOther = 'c' & 'b'\nparse Start\nparse Other\n";

		ReadsAsOverCharacters(grammar, input, at, length);
	}

	/// <summary>
	/// A refusal at the end says the same in every rendering over tokens — the engine, and the
	/// direct reader on the tape and the immediate carrier — in the whole form, from a position
	/// and in a window: the same place and the same message, naming the end of the input as
	/// over characters.
	/// </summary>
	/// <remarks>
	/// Over kinds <c>eof</c> is rewritten through to its body, <c>?!any</c>, and its name went
	/// with it: the engine said <c>?![^ ]</c>, and the reader, which records nothing inside a
	/// rule's own look, said only that the input did not match, at the position it began. Nor
	/// did the reader say the optional before it: it does not try a turn whose door is shut,
	/// and the reading that records now tries it, as the engine does, so that what refused it
	/// is half of the message. On <c>"a b"</c> every rendering now refuses at the <c>b</c>,
	/// wanting <c>'z'</c> or the end of the input.
	/// <para>
	/// Over characters the same, where the place is the same: <c>'a'*</c> there is a run of
	/// characters, and the space between two of them ends it. The two alternatives' <c>'z'</c>
	/// is printed as the class it is read as there, <c>['z']</c>, but by the engine where the end
	/// is written in place, which is not what this holds.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("a b",   null, null, "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("a b",   0,    null, "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("a b",   0,    3,    "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("a b",   1,    null, "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("a b",   1,    2,    "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("a b",   0,    2,    "accepted",                                    null)]
	[InlineData("a a b", 0,    null, "NoMatch at 4: Expected 'z' or end of input.", "NoMatch at 2: Expected 'z' or end of input.")]
	[InlineData("a c b", 2,    null, "NoMatch at 2: Expected 'z' or end of input.", null)]
	[InlineData("ab z",  null, null, "NoMatch at 1: Expected 'z' or end of input.", null)]
	public void A_refusal_at_the_end_over_tokens_is_said_alike_in_every_rendering(string input, int? at, int? length, string said, string? saidOverCharacters)
	{
		foreach (var end in new[] { "eof", "?!any" })
		{
			var grammar = $"trivia = {{ ' '* }}\nStart = 'a'* & ('z' | 'z' & 'q')? & {end}\nOther = 'c' & 'b'\nparse Start\nparse Other\n";

			foreach (var assembly in OverTokens(grammar))
				Assert.Equal(said, Refusal(assembly, input, at, length));

			foreach (var assembly in OverCharacters(grammar))
				Assert.Equal(saidOverCharacters ?? said, Refusal(assembly, input, at, length).Replace("['z']", "'z'"));
		}
	}

	/// <summary>
	/// The reading that records answers what the quiet reading answered: a turn it tries behind a
	/// shut door, for what refused it, runs no code of the host — no guard and no construction
	/// that stands before the turn's first item.
	/// </summary>
	/// <remarks>
	/// On <c>"a"</c> the quiet reading passes both optionals by at their doors, so the last guard
	/// sees nothing counted and refuses. Were the reading that records to enter them, the first
	/// would count a guard and the second a construction of what is empty, both before refusing
	/// at the <c>a</c>, and the last guard would then let the parse through: a refusal that the
	/// <c>Match</c> form, which reads twice, turned into an acceptance. Held on the engine, the
	/// tape and the immediate carrier, over tokens and over characters: the <c>bool</c> form,
	/// which only reads quietly, and the <c>Match</c> form agree on the answer and on both counts.
	/// </remarks>
	[Fact]
	public void A_turn_tried_behind_a_shut_door_runs_no_code_of_the_host()
	{
		const string grammar = """
			trivia = { ' '* }
			Start = (when @(++Guards > 0) & 'z')? & (m: Made & 'y')? & 'a' & when @(Guards + Made_ == 1)
			Made : @int = 'q'? => @(++Made_)
			parse Start
			""";

		const string members = "public static int Guards; public static int Made_;";

		foreach (var lexical in new[] { true, false })
			foreach (var (direct, carrier) in new (bool, CarrierKind?)[] { (false, null), (true, CarrierKind.Tape), (true, CarrierKind.Immediate) })
			{
				var options = Options(lexical, direct);

				if (carrier is { } kind)
					options.Carrier = kind;

				var result = GramCompiler.Compile(grammar, options);

				EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.NotCut));

				var assembly = EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members);
				var type     = assembly.GetType("Grammar")!;

				(int, int) Counted()
				{
					var counted = ((int)type.GetField("Guards")!.GetValue(null)!, (int)type.GetField("Made_")!.GetValue(null)!);

					type.GetField("Guards")!.SetValue(null, 0);
					type.GetField("Made_")!.SetValue(null, 0);

					return counted;
				}

				Counted();

				var quiet = (WholeRead(assembly, "a"), Counted());
				var both  = (Whole(assembly, "a").Outcome == "Success", Counted());

				Assert.True(quiet == both, $"lexical {lexical}, direct {direct}, {carrier}: quietly {quiet}, recording {both}");
				Assert.Equal((false, (0, 0)), quiet);
			}
	}

	/// <summary>
	/// <c>any</c> over tokens reads a token, and a character no token begins with as one
	/// character, as it reads one over characters; what follows it is still there, and
	/// <c>eof</c> is not met until the text ends.
	/// </summary>
	[Theory]
	[InlineData("a#", 0, null)]
	[InlineData("a#", 0, 2)]
	[InlineData("a#b", 0, null)]
	[InlineData("a#b", 0, 2)]
	[InlineData("a#c", 0, null)]
	[InlineData("ac", 0, null)]
	[InlineData("a c#", 0, 3)]
	public void Any_over_tokens_reads_a_character_no_token_begins_with_as_one(string input, int at, int? length)
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a' & any & eof & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n";

		ReadsAsOverCharacters(grammar, input, at, length);
	}

	/// <summary>
	/// A complement is any item except those it names (§3.1), and over tokens a character no
	/// token begins with is an item, as <c>any</c> reads it: <c>[^ 'b']</c> reads it, <c>?=</c>
	/// over the complement sees it and <c>?!</c> is refused by it — as over characters, where
	/// the same complement reads the same character.
	/// </summary>
	[Theory]
	[InlineData("'a' & [^ 'b'] & eof", "a#", 0, null)]
	[InlineData("'a' & [^ 'b'] & eof", "a#", 0, 2)]
	[InlineData("'a' & [^ 'b'] & eof", "a#b", 0, null)]
	[InlineData("'a' & [^ 'b'] & eof", "ab", 0, null)]
	[InlineData("'a' & [^ 'b'] & eof", "ac", 0, null)]
	[InlineData("'a' & [^ 'b'] & eof", "a c#", 0, 3)]
	[InlineData("'a' & ?=[^ 'b'] & any & eof", "a#", 0, null)]
	[InlineData("'a' & ?=[^ 'b'] & any & eof", "a#", 0, 2)]
	[InlineData("'a' & ?=[^ 'b'] & any & eof", "ab", 0, null)]
	[InlineData("'a' & ?![^ 'b'] & any & eof", "a#", 0, null)]
	[InlineData("'a' & ?![^ 'b'] & any & eof", "a#", 0, 2)]
	[InlineData("'a' & ?![^ 'b'] & any & eof", "ab", 0, null)]
	[InlineData("'a' & ?![^ 'b'] & any & eof", "ac", 0, null)]
	public void A_complement_over_tokens_reads_a_character_no_token_begins_with(string start, string input, int at, int? length)
	{
		var grammar = $"trivia = {{ ' '* }}\nStart = {start} & ('z' | 'z' & 'q')?\nOther = ('b' | 'c')\nparse Start\nparse Other\n";

		ReadsAsOverCharacters(grammar, input, at, length);
	}

	/// <summary>
	/// A reading from a position is not refused by a character it never read (§6.3): one no
	/// token begins with, standing before the position, leaves what follows it readable — as
	/// the window form over the same text reads it.
	/// </summary>
	/// <remarks>
	/// The form from a position cut the whole input and the cutting ended at the <c>#</c>
	/// before <c>at</c>, so no token was left at or after it and the answer was Starved. The
	/// cutting now goes on past it.
	/// </remarks>
	[Fact]
	public void A_character_no_token_begins_with_before_the_position_does_not_refuse_the_reading()
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a' & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n";

		foreach (var assembly in OverTokens(grammar))
		{
			Assert.Equal((true, (string?)null, 1L, 1L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "#a", 1, 1));
			Assert.Equal((true, (string?)null, 1L, 1L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "#a", 1));
			Assert.Equal((true, (object?)"a", 2), EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "#a", 1));
			Assert.Equal((true, (object?)"a", 2), EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "#a", 1, 1));

			// Past a second one, and between two statements of a text read a piece at a time.
			Assert.Equal((true, (string?)null, 4L, 3L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "a#$ a z#", 4));
			Assert.Equal((true, (object?)"a z", 7), EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "a#$ a z#", 3));
		}

		foreach (var (input, at) in new[] { ("#a", 1), ("a#$ a z#", 4), ("a#$ a z#", 3), ("a#a", 2), ("#a ", 1) })
			ReadsAsOverCharacters(grammar, input, at, null);
	}

	/// <summary>
	/// A reading that begins at a character no token begins with is answered by that character
	/// as it is over characters (§6.3), whether or not a token is left after it: a rule that
	/// must read something is refused there (<c>NoMatch</c>), and one that can read nothing
	/// reads nothing there. Where nothing but trivia is left there is nothing to read over
	/// tokens and the answer is <c>Starved</c>.
	/// </summary>
	/// <remarks>
	/// The last column is the reading over characters, written out where it differs. Where
	/// nothing but trivia is left the two are not compared: over tokens that is <c>Starved</c>
	/// by §6.3, and over characters it depends on whether the rule can read nothing past the
	/// trivia, which is not this question.
	/// </remarks>
	[Theory]
	[InlineData("'a'", "#a", 0, null, "NoMatch 0 0", "same")]
	[InlineData("'a'", "#a", 0, 2, "NoMatch 0 0", "same")]
	[InlineData("'a'", " #a", 0, null, "NoMatch 1 0", "same")]
	[InlineData("'a'", "a #a", 1, null, "NoMatch 2 0", "same")]
	[InlineData("'a'?", "#a", 0, null, "Success 0 0", "same")]
	[InlineData("'a'?", "#a", 0, 2, "Success 0 0", "same")]
	[InlineData("'a'?", "a #a", 1, 3, "Success 2 0", "same")]
	[InlineData("'a'", "#", 0, null, "NoMatch 0 0", "same")]
	[InlineData("'a'", "a #", 1, 2, "NoMatch 2 0", "same")]
	[InlineData("'a'?", "#", 0, null, "Success 0 0", "same")]
	[InlineData("'a'?", "a# #", 1, null, "Success 1 0", "same")]
	[InlineData("'a'?", "a# #a", 1, 3, "Success 1 0", "same")]
	[InlineData("'a'", "a  ", 1, null, "Starved 3 0", null)]
	[InlineData("'a'?", "a  ", 1, null, "Starved 3 0", null)]
	[InlineData("'a'?", "a  ", 1, 2, "Starved 3 0", null)]
	public void A_reading_that_begins_at_a_character_no_token_begins_with(
		string start, string input, int at, int? length, string tokens, string? characters)
	{
		var grammar = $"trivia = {{ ' '* }}\nStart = {start} & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n";
		var said    = $"\"{input}\" at {at}, length {length?.ToString() ?? "none"}";

		foreach (var assembly in OverTokens(grammar))
		{
			var match = Positioned(assembly, input, at, length);

			Assert.True(tokens == $"{match.Outcome} {match.Position} {match.Length}", said + ": " + match);
			Assert.Equal(
				(match.Outcome == "Success", match.Outcome == "Success" ? (int)(match.Position + match.Length) : at),
				Moved(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", input, at, length)));
		}

		if (characters is null)
			return;

		var over = Positioned(Built(grammar, lexical: false, direct: true), input, at, length);

		Assert.True((characters == "same" ? tokens : characters) == $"{over.Outcome} {over.Position} {over.Length}", said + ": " + over);
	}

	/// <summary>
	/// A host reading a growing text a statement at a time is told <c>Starved</c> only while the
	/// text may still grow into something: past the trivia after the last statement, or inside
	/// a token. A character no token begins with is there to answer, whatever follows it.
	/// </summary>
	/// <remarks>
	/// Each row is a prefix of the text as the host holds it, read from the position after the
	/// first statement. The <c>#</c> answers <c>NoMatch</c> at once, as over characters, and does
	/// not wait for what might come after it; an optional statement reads nothing there.
	/// </remarks>
	[Theory]
	[InlineData("Item", "a",       1, "Starved 1 0")]
	[InlineData("Item", "a ",      1, "Starved 2 0")]
	[InlineData("Item", "a #",     1, "NoMatch 2 0")]
	[InlineData("Item", "a # ",    1, "NoMatch 2 0")]
	[InlineData("Item", "a #a",    1, "NoMatch 2 0")]
	[InlineData("Item?", "a #",     1, "Success 2 0")]
	[InlineData("Item?", "a # a",   1, "Success 2 0")]
	[InlineData("Item", "a \"bc",  1, "Starved 5 0")]
	[InlineData("Item", "a #\"bc", 1, "NoMatch 2 0")]
	public void A_growing_text_is_starved_only_where_it_may_still_grow(string start, string input, int at, string tokens)
	{
		var grammar = $"{CutShort}Start = {start} & ('!' | '!' & '?')?\n";

		foreach (var assembly in OverTokens(grammar))
		{
			var match = Positioned(assembly, input, at, null);

			Assert.True(tokens == $"{match.Outcome} {match.Position} {match.Length}", $"\"{input}\" at {at}: {match}");
		}
	}

	/// <summary>
	/// A position outside the input is <c>NoMatch</c> in the form that 	/// A position outside the input is <c>NoMatch</c> in the form that answers with a match
	/// (§6.3), over tokens as over characters — and not a reading begun at the first token.
	/// </summary>
	[Theory]
	[InlineData("a z", -1)]
	[InlineData("a z", 4)]
	[InlineData("", 1)]
	public void A_position_outside_the_input_is_no_match_over_tokens(string input, int at)
	{
		const string grammar = "trivia = { ' '* }\nStart = 'a' & ('z' | 'z' & 'q')?\nOther = 'c'\nparse Start\nparse Other\n";

		foreach (var assembly in OverTokens(grammar))
		{
			var type  = assembly.GetType("Grammar")!;
			var match = type.GetMethod("TryParseStart", [typeof(string), typeof(int)])!.Invoke(null, [input, at])!;

			Assert.Equal("NoMatch", match.GetType().GetProperty("Outcome")!.GetValue(match)!.ToString());
			Assert.Equal((long)at, Convert.ToInt64(match.GetType().GetProperty("Position")!.GetValue(match)));
		}
	}

	/// <summary>
	/// The engine and the direct reader are two renderings of one grammar read over tokens, and
	/// answer the same input the same — where a rule's answer stands (§4).
	/// </summary>
	/// <remarks>
	/// The engine gave back into a called rule over tokens as it does over characters (§11):
	/// <c>R2</c> read the <c>b</c>, <c>'b'</c> after it failed, and the engine took the
	/// <c>b</c> back from <c>R2</c> and read it again. The direct reader refused, as §4 says:
	/// once a rule has answered, nothing that fails after it sends the parse back into it. A
	/// rule marked <c>?</c> gives back inside itself and not at its boundary, so it stands there
	/// too. The engine now reads each call over kinds as if braced.
	/// </remarks>
	[Theory]
	[InlineData("trivia = { ' '* }\nR1 = (R2 & 'b')\nR2 = 'b'?\nparse R1\n", "TryParseR1", "b|bb|bbb|")]
	[InlineData("trivia = { ' '* }\nR1 = ('c' | R2 & ['a'..'b'])\nR2 = ('b' | 'a')?\nparse R1\n", "TryParseR1", "b|a|ab|ba|c|")]
	[InlineData("trivia = { ' '* }\nR1 = 'a' & R2 & 'b'\nR2 = 'b'?\nparse R1\n", "TryParseR1", "ab|abb|a b|abbb")]
	[InlineData("trivia = { ' '* }\nR1 = R2 & 'b'\nR2? = 'a' & 'b'?\nparse R1\n", "TryParseR1", "ab|abb|a|")]
	[InlineData("trivia = { ' '* }\nR1 = (R2 & 'c' | 'a' & 'b' & 'd')\nR2 = ('a' & 'b' | 'a')\nparse R1\n", "TryParseR1", "abc|ac|abd|ab")]
	public void The_engine_and_the_direct_reader_over_tokens_agree(string grammar, string method, string inputs)
	{
		var renderings = OverTokens(grammar).ToArray();

		foreach (var input in inputs.Split('|'))
		{
			var expected = EmittedCode.Match(renderings[0], "Grammar", method, input);

			foreach (var assembly in renderings.Skip(1))
			{
				var match = EmittedCode.Match(assembly, "Grammar", method, input);

				Assert.True(expected.IsSuccess == match.IsSuccess, $"\"{input}\":\n{grammar}");
				Assert.True(expected.Position == match.Position, $"\"{input}\":\n{grammar}");
			}
		}
	}

	/// <summary>
	/// A called rule over tokens has the answer it gives on its own: where it read what its
	/// caller wanted next, the caller refuses, in every rendering and every carrier, from a
	/// position and in a window too.
	/// </summary>
	[Theory]
	[InlineData("b", 0, null, false)]
	[InlineData("bb", 0, null, true)]
	[InlineData("x b", 1, null, false)]
	[InlineData("x b", 1, 2, false)]
	[InlineData("x bb", 1, 3, true)]
	[InlineData("x bb", 1, 2, false)]
	public void A_called_rule_over_tokens_is_not_sent_back_into(string input, int at, int? length, bool read)
	{
		const string grammar = "trivia = { ' '* }\nR1 = R2 & 'b' & ('z' | 'z' & 'q')?\nR2 = 'b'?\nOther = 'x'\nparse R1\nparse Other\n";

		var answers = OverTokens(grammar)
			.Select(assembly => EmittedCode.Positioned(assembly, "Grammar", "TryParseR1", input, at, length))
			.ToArray();

		Assert.All(answers, one => Assert.Equal(answers[0], one));
		Assert.Equal(read, answers[0].IsSuccess);
	}

	/// <summary>
	/// Inside a rule over tokens a repetition, an optional or a choice that has read what fits
	/// keeps it, and what the rule wanted after it is not there any more — in every rendering
	/// and every carrier, whole, from a position and in a window. Over characters each gives
	/// back, and a rule marked <c>?</c> gives back over tokens too.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Over kinds a choice is decided by the token in front of it and never revisited, and only
	/// inside a rule marked <c>?</c> is a choice or a repetition revisited when something later
	/// in the same rule fails (§4); GRAM5009 is the warning for the shape. The direct reader read
	/// it so. The engine braced each call over kinds but revisited what matched inside a rule, so
	/// <c>Lex.Text* &amp; Lex.Text</c> read <c>"bc"</c> on the engine and was starved on the
	/// reader; and the flat method lowered from the engine, which a rule as small as
	/// <c>Choice</c> is read by in every rendering, braced nothing, so it went back into a choice
	/// and into a call alike. Both now read each call, and each choice and repetition of a rule
	/// not marked <c>?</c>, as if braced.
	/// </para>
	/// <para>
	/// A rule marked <c>?</c> gives back inside itself and stands at its boundary: <c>Given</c>
	/// reads two texts by giving the last turn back to its own terminal, and a terminal after
	/// <c>Given</c> does not get one back from it.
	/// </para>
	/// <para>
	/// Starved, where the refusal is at the end of the text: the repetition's next turn wanted
	/// more (§7.5), as <c>{ Lex.Text* } &amp; Lex.Text</c> is starved over characters.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("Lex.Text* & Lex.Text",       "\"bc\"",           null, null, "Starved 4 0",  "Success 0 4")]
	[InlineData("Lex.Text* & Lex.Text",       "\"a\" \"bc\"",     null, null, "Starved 8 0",  "Success 0 8")]
	[InlineData("Lex.Text* & Lex.Text",       "x \"a\" \"bc\"",   2,    null, "Starved 10 0", "Success 2 8")]
	[InlineData("Lex.Text* & Lex.Text",       "\"a\" \"bc\" x",   0,    8,    "Starved 8 0",  "Success 0 8")]
	[InlineData("Lex.Text* & Lex.Text",       "x \"a\" \"bc\" x", 2,    8,    "Starved 10 0", "Success 2 8")]
	[InlineData("Lex.Text+ & Lex.Text",       "\"a\" \"bc\"",     null, null, "Starved 8 0",  "Success 0 8")]
	[InlineData("Lex.Text{1,3} & Lex.Text",   "\"a\" \"bc\"",     null, null, "Starved 8 0",  "Success 0 8")]
	[InlineData("Lex.Text? & Lex.Text",       "\"bc\"",           null, null, "Starved 4 0",  "Success 0 4")]
	[InlineData("Lex.Text? & Lex.Text",       "x \"bc\" x",       2,    4,    "Starved 6 0",  "Success 2 4")]
	[InlineData("Lex.Text? & Lex.Text",       "\"a\" \"bc\"",     null, null, "Success 0 8",  "same")]
	[InlineData("Choice",                     "\"a\" \"b\"",          null, null, "Success 0 7",  "same")]
	[InlineData("Choice",                     "\"a\" \"b\" \"c\"",     null, null, "NoMatch 8 0",  "Success 0 11")]
	[InlineData("Pair & Lex.Text",            "\"a\" \"b\"",          null, null, "Starved 7 0",  "Success 0 7")]
	[InlineData("Pair & Lex.Text",            "\"a\" \"b\" \"c\"",     null, null, "Success 0 11", "same")]
	[InlineData("Given",                      "\"a\" \"bc\"",         null, null, "Success 0 8",  "same")]
	[InlineData("Given",                      "x \"a\" \"bc\" x",     2,    8,    "Success 2 8",  "same")]
	[InlineData("Given & Lex.Text",           "\"bc\" \"d\"",         null, null, "Starved 8 0",  "Success 0 8")]
	public void A_repetition_inside_a_rule_over_tokens_keeps_what_it_read(
		string start, string input, int? at, int? length, string tokens, string characters)
	{
		var grammar =
			"trivia = { ' '* }\n" +
			"namespace Lex\n{\n\ttrivia = none\n\tText = '\"' & ['a'..'z']* & '\"'\n}\n" +
			$"Start = {start}\n" +
			(start.Contains("Given", StringComparison.Ordinal) ? "Given? = Lex.Text* & Lex.Text\n" : "") +
			(start.Contains("Choice", StringComparison.Ordinal) ? "Choice = (Lex.Text | Lex.Text & Lex.Text) & Lex.Text\n" : "") +
			(start.Contains("Pair", StringComparison.Ordinal) ? "Pair = (Lex.Text & Lex.Text | Lex.Text)\n" : "") +
			"parse Start\n";

		foreach (var assembly in OverTokens(grammar))
			Assert.Equal(tokens, Answer(assembly));

		foreach (var assembly in OverCharacters(grammar))
			Assert.Equal(characters == "same" ? tokens : characters, Answer(assembly));

		string Answer(Assembly assembly)
		{
			if (at is { } from)
			{
				var (outcome, position, read) = Positioned(assembly, input, from, length);

				return $"{outcome} {position} {read}";
			}

			var whole = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			return whole.IsSuccess
				? $"Success {whole.Position} {input.Length}"
				: $"{Whole(assembly, input).Outcome} {whole.Position} 0";
		}
	}

	/// <summary>
	/// The constructs whose engine rendering tries a way out first and comes back — a recovering
	/// repetition followed by the end, called, followed by what its elements begin with, and
	/// called inside a lookahead; a positive and a negative lookahead; a fold, by recursion on the
	/// left and by binding powers — read alike over tokens in every rendering, whole, from every
	/// position and in windows, and as over characters. The last column says what the tape and
	/// the immediate carrier run: the reader's methods, a flat method, or the engine.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A recovering repetition tries the complete continuation after it before each turn (§8.2)
	/// and comes back for the turn when that fails. Braced as an ordinary repetition is over
	/// kinds, the probe's way out was committed: <c>Row* recover ';' &amp; 'a' &amp; eof</c>
	/// refused <c>a;a</c> on the engine. So was a call holding one: <c>Rows &amp; 'z'</c> refused
	/// <c>a;z</c>, <c>Rows</c> answering with no element before <c>'z'</c> was tried — read only
	/// while the repetition was taken for silent and the call's braces compiled to nothing. The
	/// probe is the recovery's machinery and not a reading the author chose: nothing that holds
	/// one is braced, so the continuation it tries is the one §8.2 names, past the end of its rule,
	/// and the answer is the one over characters. That is a reading of §8.2 against §4's "a rule's
	/// answer stands", which the maintainer may decide otherwise.
	/// </para>
	/// <para>
	/// Every recovering row runs the engine in all three renderings: the reader reads a recovery
	/// only with a factory, a factory is refused in a spaced grammar (GRAM4010, the element being
	/// the seam and the call), and a grammar without trivia is not cut. Nor is a stream asked:
	/// yielding with implicit trivia is refused (GRAM4027). The inputs have no spaces where a
	/// valueless repetition's turns meet, which over characters have no seam.
	/// </para>
	/// <para>
	/// Where a turn has to be recovered before what follows the repetition can read — <c>aa;z</c>
	/// through <c>Rows</c>, <c>aa;a</c> before <c>'a' &amp; eof</c> — the engine over tokens
	/// refused what it reads over characters: the split carried the synchronization across as
	/// written, so the engine tested the kinds for the character <c>;</c> and resumed at the end
	/// of the input. The rows read it as a literal, a rule, a choice, two tokens, and a mark no
	/// element names; called, called inside a call, and at the end of the input.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("Row = 'a' & ';'\nStart = Row* recover ';' & eof\nparse Start\n", "a;|a;a;|aa;a;|a;aa;|a||aa|a;aa", "engine")]
	[InlineData("Row = 'a' & ';'\nRows = Row* recover ';'\nStart = Rows & 'z'\nparse Start\n", "a;z|z|a;a;z|a;|aa;z|a a ;z|aa ; z|aa;aa;z", "engine")]
	[InlineData("Row = 'a' & ';'\nStart = Row* recover ';' & 'a' & eof\nparse Start\n", "a|a;a|a;a;a|a;|aa;a|aa;aa;a", "engine")]
	[InlineData("Row = 'a' & ';'\nStart = Row* recover ';' & 'z' & eof\nparse Start\n", "aa;z|a;z|z|aa|aa;", "engine")]
	[InlineData("Row = 'a' & ';'\nSep = ';'\nStart = Row* recover Sep & 'z' & eof\nparse Start\n", "aa;z|a;z|z|aa", "engine")]
	[InlineData("Row = 'a' & ';'\nStart = Row* recover (';' | '#') & 'z' & eof\nparse Start\n", "aa#z|aa;z|a;a#z|#z|z", "engine")]
	[InlineData("Row = 'a' & ';'\nStart = Row* recover ('x' & 'y') & 'z' & eof\nparse Start\n", "aa;xyz|aa;x yz|a;xyz|xyz|z|aa;x", "engine")]
	[InlineData("Row = 'a' & 'b'\nStart = Row* recover '#' & eof\nparse Start\n", "ab|aab#ab|ab#|a#|#|a", "engine")]
	[InlineData("Row = 'a' & ';'\nRows = Row* recover ';'\nBlock = '[' & Rows & ']'\nStart = Block & 'z' & eof\nparse Start\n", "[aa;]z|[a;]z|[]z|[aa;a;]z|[aa]z|[a;aa", "engine")]
	[InlineData("Cell = 'a' & ','\nRow = '[' & Cell* recover ',' & ']' & ';'\nStart = Row* recover ';' & eof\nparse Start\n", "[a,];|[aa,];|[a,[;|[a,];[;[a,];|[a,a,];|;|[a,]|[a,];[", "engine")]
	[InlineData("Row = 'a' & ';'\nRows = Row* recover ';' & 'z'\nStart = ?=Rows & 'a' & ';' & 'z' & eof\nparse Start\n", "a;z|z|a;a;z", "engine")]
	[InlineData("Start = ?= ('a' & 'b' | 'a') & 'a' & ('b' | 'c')\nparse Start\n", "a b|a c|a|b", "flat")]
	[InlineData("Start = ?! ('a' & 'a' | 'b') & ['a'..'c']+\nparse Start\n", "aa|ab|b|ca|a", "flat")]
	[InlineData(Numbers + "Start : @int = left: Start & '-' & right: Lex.Num => @(left - right) | value: Lex.Num => @(value)\nparse Start\n", "9 - 2 - 3|9|9 -|- 9", "methods")]
	[InlineData(Numbers + "Start : @int = left: Start & '-' & right: Start << 1 => @(left - right) | left: Start & '*' & right: Start << 2 => @(left * right) | value: Lex.Num => @(value)\nparse Start\n", "9 - 2 * 3 - 1|2 * 3|9 -|9 - 2 *", "methods")]
	public void A_construct_that_tries_a_way_out_first_reads_alike_over_tokens(string grammar, string inputs, string carried)
	{
		grammar = "trivia = { ' '* }\n" + grammar;

		// What the tape and the immediate carrier run: the reader's methods, a flat method, or the
		// engine where the methods refuse the grammar (GRAM5005) — which every recovery over tokens is, a
		// recovery the reader reads needing a factory and a factory being refused in a spaced
		// grammar (GRAM4010). Those rows ask the engine three times, and say so.
		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
		{
			var options = Options(lexical: true, direct: true);

			options.Carrier = carrier;

			var text = GramCompiler.Compile(grammar, options).Sources[0].Text;

			Assert.Equal(
				carried,
				text.Contains("ref struct Reader_", StringComparison.Ordinal) ? "methods" :
				text.Contains("entries.Add(new ParserEntry(", StringComparison.Ordinal) ? "engine" :
				"flat");
		}

		var tokens     = OverTokens(grammar, "GRAM5005", "GRAM5007").ToArray();
		var characters = OverCharacters(grammar).ToArray();

		foreach (var input in inputs.Split('|'))
		{
			var expected = Read(tokens[0], input);

			foreach (var assembly in tokens.Skip(1))
				Assert.Equal(expected, Read(assembly, input));

			// Over characters the same answer and value, and the same place where it is accepted:
			// a refusal is placed by what each reading counts as where it stopped.
			foreach (var assembly in characters)
				Assert.Equal(Accepted(expected), Accepted(Read(assembly, input)));

			if (tokens[0].GetType("Grammar")!.GetMethod("TryParseStart", [typeof(string), typeof(int)]) is null)
				continue;

			for (var at = 0; at <= input.Length; at++)
				foreach (var length in new int?[] { null, input.Length - at, (input.Length - at) / 2 })
				{
					var from = Positioned(tokens[0], input, at, length);

					foreach (var assembly in tokens.Skip(1))
						Assert.Equal(from, Positioned(assembly, input, at, length));
				}
		}

		string Read(Assembly assembly, string input)
		{
			var match  = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
			var said   = match.IsSuccess
				? $"Success {Spelled(match.Value)}"
				: $"{EmittedCode.Outcome(assembly, "Grammar", "TryParseStart", input)} {match.Position}";

			if (assembly.GetType("Grammar")!.GetMethod("Lazy", [typeof(string)]) is null)
				return said;

			try
			{
				return said + " / " + Spelled(EmittedCode.Streamed(assembly, "Grammar", "Lazy", input));
			}
			catch (TargetInvocationException thrown)
			{
				return said + " / " + thrown.InnerException!.GetType().Name;
			}
			catch (FormatException thrown)
			{
				return said + " / " + thrown.GetType().Name;
			}
		}

		static string Accepted(string said)
		{
			return said.StartsWith("Success", StringComparison.Ordinal) ? said : "refused";
		}

		static string Spelled(object? value)
		{
			return value is System.Collections.IEnumerable many and not string
				? "[" + string.Join(",", many.Cast<object?>().Select(Spelled)) + "]"
				: value?.ToString() ?? "null";
		}
	}

	/// <summary>
	/// What a repetition marked <c>recover</c> synchronizes on is a terminal of the lexer as a
	/// body's terminal is, and by the same rule: the inventory is the same whether a literal is
	/// written in the synchronization or in a body, and so is every answer over tokens.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A literal is a token and longest match decides it (§4.5: a token cannot be half spent),
	/// so <c>recover ";;"</c> makes <c>;;</c> one token that <c>';' &amp; ';'</c> cannot read,
	/// exactly as <c>";;"</c> in a body does — not what the parse would read over characters,
	/// which is what the same literal in a body is not either. Before the synchronization was
	/// walked, <c>;;</c> was two tokens here and the synchronization matched nothing.
	/// </para>
	/// <para>
	/// A word in the synchronization is a keyword, and a keyword is an identifier too: a class
	/// that would have matched it stands for its kind as well, so <c>Lex.Name</c> reads
	/// <c>end</c> whether <c>"end"</c> is what a repetition synchronizes on or an operand.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(
		"Row = 'a' & ';'\nRows = Row* recover \";;\"\nStart = ';' & ';' & Rows & eof\nparse Start\n",
		"Row = 'a' & ';'\nRows = Row* recover ';'\nStart = ';' & ';' & Rows & \";;\"? & eof\nparse Start\n",
		";;|; ;|; ; ;;|; ; a;|; ; aa;;|;;;;|; ; a;a;;;|; ;a")]
	[InlineData(
		"wordboundary = ['a'..'z']\nnamespace Lex\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n}\nRow = Lex.Name & ';'\nStart = Row* recover \"end\" & eof\nparse Start\n",
		"wordboundary = ['a'..'z']\nnamespace Lex\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n}\nRow = Lex.Name & ';'\nStart = Row* recover ';' & \"end\"? & eof\nparse Start\n",
		"a;|end;|a end;|aend;|enda;|a;end|end|a end|ab cd;end")]
	public void A_synchronization_is_lexed_as_a_body_is(string synced, string bodied, string inputs)
	{
		synced = "trivia = { ' '* }\n" + synced;
		bodied = "trivia = { ' '* }\n" + bodied;

		var ofSynced = TerminalInventory.Of(Graph(synced));
		var ofBodied = TerminalInventory.Of(Graph(bodied));

		Assert.Equal(Kinds(ofBodied), Kinds(ofSynced));

		var overSynced = OverTokens(synced, "GRAM5005", "GRAM5007").ToArray();
		var overBodied = OverTokens(bodied, "GRAM5005", "GRAM5007").ToArray();

		foreach (var input in inputs.Split('|'))
		{
			var expected = Answer(overBodied[0], input);

			foreach (var assembly in overBodied.Skip(1).Concat(overSynced))
				Assert.Equal(expected, Answer(assembly, input));
		}

		static RecognitionGraph Graph(string grammar)
		{
			return GrammarNormalizer.Normalize(
				GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
		}

		static string Kinds(TerminalInventory inventory)
		{
			return string.Join(
				"; ",
				inventory.Patterns.Select(pattern =>
					pattern + " = " + string.Join(",", inventory.KindsOf(pattern).Select(one => $"{one.From}..{one.To}"))));
		}

		static string Answer(Assembly assembly, string input)
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			return match.IsSuccess
				? $"Success {match.Value}"
				: $"{EmittedCode.Outcome(assembly, "Grammar", "TryParseStart", input)} {match.Position}";
		}
	}

	/// <summary>A number the lexer reads, valued where a rule calls it.</summary>
	const string Numbers ="namespace Lex\n{\n\ttrivia = none\n\tNum : @int = ['0'..'9']+ => @(int.Parse(parserText))\n}\n";

	// ── A token the text ends inside of ─────────────────────────────────────────

	/// <summary>
	/// The tokens of the next two tests: a name, a string, an operator of three characters and a
	/// comment the syntax reads, each of which the text can end inside of.
	/// </summary>
	const string CutShort =
		"trivia = { ' '* }\n" +
		"namespace Lex\n{\n\ttrivia = none\n" +
		"\tName  = ['a'..'z']+\n" +
		"\tText  = '\"' & [^ '\"']* & '\"'\n" +
		"\tArrow = \"<=>\"\n" +
		"\tNote  = \"{-\" & (?!\"-}\" & any)* & \"-}\"\n}\n" +
		"Item  = (Lex.Name | Lex.Text | Lex.Arrow | Lex.Note)\n" +
		"Other = 'c'\n" +
		"parse Start\nparse Other\n";

	/// <summary>
	/// A text that ends inside a token — an unclosed string or comment, half an operator — is
	/// starved where a reading reaches that token, from a position and in a window as over
	/// characters (§7.5): more input could finish it. The lexer used to stop there as at a
	/// character no token begins with, and the answer was <c>NoMatch</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A token the end of the window leaves complete is read, though more input would make it
	/// longer: <c>ab c</c> is two names. A character no token begins with is still refused as
	/// one, before the unfinished token or without one.
	/// </para>
	/// <para>
	/// The last column is the reading over characters, written out where it differs. A literal
	/// that wants more characters than remain is starved there where it begins, and over tokens
	/// at the end of the text, as every token cut short is. A position inside a token begins
	/// over tokens at the next one, of which there is none here. And a reading that ends in an
	/// optional that read nothing counts the space before it over characters, which is not this
	/// question.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("Item & Item", "a \"bc", 0, null, "Starved 5 0", "same")]
	[InlineData("Item & Item", "a \"bc\"", 0, 4, "Starved 4 0", "same")]
	[InlineData("Item & Item", "a <=", 0, null, "Starved 4 0", "Starved 2 0")]
	[InlineData("Item & Item", "a <=>", 0, 4, "Starved 4 0", "Starved 2 0")]
	[InlineData("Item & Item", "a {- x", 0, null, "Starved 6 0", "same")]
	[InlineData("Item & Item", "a {- x -}", 0, 6, "Starved 6 0", "same")]
	[InlineData("Item & Item", "a \"bc", 2, null, "Starved 5 0", "same")]
	[InlineData("Item & Item", "a \"bc", 2, 2, "Starved 4 0", "same")]
	[InlineData("Item & Item", "a \"bc", 3, null, "Starved 5 0", "Success 3 2")]
	[InlineData("Item & Item", "ab cd", 0, 4, "Success 0 4", "same")]
	[InlineData("Item & Item", "ab c", 0, null, "Success 0 4", "same")]
	[InlineData("Item & Item", "a #", 0, null, "NoMatch 2 0", "same")]
	[InlineData("Item & Item", "# \"bc", 0, null, "NoMatch 0 0", "same")]
	[InlineData("Item & Item", "a # \"bc", 0, null, "NoMatch 2 0", "same")]
	[InlineData("Item & Item?", "a \"bc", 0, null, "Success 0 1", "Success 0 2")]
	[InlineData("Item & Item?", "a \"bc", 0, 5, "Success 0 1", "Success 0 2")]
	[InlineData("Item & Item?", "a \"bc\" d", 0, 4, "Success 0 1", "Success 0 2")]
	[InlineData("Item & Item?", "a \"bc\" d", 0, 6, "Success 0 6", "same")]
	[InlineData("Item? & Lex.Name", "a \"bc", 2, null, "Starved 5 0", "same")]
	[InlineData("Item", "a \"bc", 0, null, "Success 0 1", "Success 0 2")]
	[InlineData("Item", "a \"bc", 0, 4, "Success 0 1", "Success 0 2")]
	public void A_reading_that_reaches_a_token_the_text_ends_inside_of_is_starved(
		string start, string input, int at, int? length, string tokens, string characters)
	{
		var grammar = $"{CutShort}Start = {start} & ('!' | '!' & '?')?\n";
		var said    = $"\"{input}\" at {at}, length {length?.ToString() ?? "none"}";

		foreach (var assembly in OverTokens(grammar))
		{
			var match = Positioned(assembly, input, at, length);

			Assert.True(tokens == $"{match.Outcome} {match.Position} {match.Length}", said + ", over tokens: " + match);
			Assert.Equal(
				(match.Outcome == "Success", match.Outcome == "Success" ? (int)(match.Position + match.Length) : at),
				Moved(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", input, at, length)));
		}

		var over = Positioned(Built(grammar, lexical: false, direct: true), input, at, length);

		Assert.True((characters == "same" ? tokens : characters) == $"{over.Outcome} {over.Position} {over.Length}", said + ", over characters: " + over);
	}

	/// <summary>
	/// A whole reading of a text that ends inside a token is starved, in every rendering and as
	/// over characters, and its <c>bool</c> form refuses it; a character no token begins with
	/// still refuses it as one.
	/// </summary>
	/// <remarks>
	/// Where the reading is refused by the end it needs, at the unfinished token, it is starved
	/// over tokens too, since what the token would have been is not known until it ends; over
	/// characters the end is refused by the token's first character (the last row).
	/// </remarks>
	[Theory]
	[InlineData("Item & Item?", "a \"bc", "Starved 5", "same")]
	[InlineData("Item & Item?", "\"bc", "Starved 3", "same")]
	[InlineData("Item & Item?", "a <=", "Starved 4", "Starved 2")]
	[InlineData("Item & Item?", "a {- x", "Starved 6", "same")]
	[InlineData("Item & Item?", "a {- x -", "Starved 8", "same")]
	[InlineData("Item & Item?", "a {- x -}", "Success 0", "same")]
	[InlineData("Item & Item?", "ab", "Success 0", "same")]
	[InlineData("Item & Item?", "a #", "NoMatch 2", "same")]
	[InlineData("Item & Item?", "# \"bc", "NoMatch 0", "same")]
	[InlineData("Item & Item", "a \"bc", "Starved 5", "same")]
	[InlineData("Item", "a \"bc", "Starved 5", "NoMatch 2")]
	public void A_whole_reading_of_a_text_that_ends_inside_a_token_is_starved(string start, string input, string tokens, string characters)
	{
		var grammar = $"{CutShort}Start = {start} & ('!' | '!' & '?')?\n";

		foreach (var assembly in OverTokens(grammar))
		{
			var match = Whole(assembly, input);

			Assert.True(tokens == $"{match.Outcome} {match.Position}", $"\"{input}\", over tokens: {match}");
			Assert.Equal(match.Outcome == "Success", WholeRead(assembly, input));
		}

		var over = Whole(Built(grammar, lexical: false, direct: true), input);

		Assert.True((characters == "same" ? tokens : characters) == $"{over.Outcome} {over.Position}", $"\"{input}\", over characters: {over}");
	}

	/// <summary>
	/// A token the automaton only begins and a rule measures is cut short where the rule ran out
	/// of text at the furthest it got, however far it then gave back: the rule here reads every
	/// <c>a</c> and gives them back one at a time to find <c>abc</c>, asking for <c>abc</c> again
	/// at each, where the room left is more each time.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The last of those asks used to be the one remembered, so <c>@aab</c> — which a <c>c</c>
	/// finishes — was refused as a character no token begins with, and over characters, where the
	/// same rule is read by the same machine, it was refused at the <c>b</c>.
	/// </para>
	/// <para>
	/// The columns: from the position or in the window over tokens, the whole reading over tokens,
	/// and both over characters, which say the same. Over characters a literal wanting more than is
	/// left is starved where it begins, over tokens at the end. Over tokens <c>@aabx</c> from a
	/// position is characters no token begins with, which answer it as they do over characters (§6.3).
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("@aab", 0, null, "Starved 4", "Starved 4", "Starved 3")]
	[InlineData("@aaab", 0, null, "Starved 5", "Starved 5", "Starved 4")]
	[InlineData("@aaaab", 0, null, "Starved 6", "Starved 6", "Starved 5")]
	[InlineData("@ab", 0, null, "Starved 3", "Starved 3", "Starved 2")]
	[InlineData("@aa", 0, null, "Starved 3", "Starved 3", "Starved 3")]
	[InlineData("@", 0, null, "Starved 1", "Starved 1", "Starved 1")]
	[InlineData("@aabc", 0, 4, "Starved 4", null, "Starved 3")]
	[InlineData("@aaabc !", 0, 5, "Starved 5", null, "Starved 4")]
	[InlineData("@aabc", 0, null, "Success 0", "Success 0", "Success 0")]
	[InlineData("@aabx", 0, null, "NoMatch 0", "NoMatch 0", "NoMatch 4")]
	[InlineData("@aabcz", 0, null, "NoMatch 0", "NoMatch 0", "NoMatch 3")]
	public void A_token_a_rule_measures_is_cut_short_where_the_rule_ran_out_furthest(
		string input, int at, int? length, string tokens, string? whole, string characters)
	{
		const string grammar =
			"trivia = { ' '* }\n" +
			"namespace Lex\n{\n\ttrivia = none\n\tT = '@' & Tail\n\tTail = 'a'* & \"abc\" & ?!'z'\n}\n" +
			"Start = Lex.T & ('!' | '!' & '?')?\nOther = 'c'\nparse Start\nparse Other\n";

		var said = $"\"{input}\" at {at}, length {length?.ToString() ?? "none"}";

		foreach (var assembly in OverTokens(grammar))
		{
			var match = Positioned(assembly, input, at, length);

			Assert.True(tokens == $"{match.Outcome} {match.Position}", said + ", over tokens: " + match);

			if (whole is not null)
			{
				var read = Whole(assembly, input);

				Assert.True(whole == $"{read.Outcome} {read.Position}", said + ", whole, over tokens: " + read);
			}
		}

		var chars = Built(grammar, lexical: false, direct: true);
		var over  = Positioned(chars, input, at, length);

		Assert.True(characters == $"{over.Outcome} {over.Position}", said + ", over characters: " + over);

		if (whole is not null)
		{
			var read = Whole(chars, input);

			Assert.True(characters == $"{read.Outcome} {read.Position}", said + ", whole, over characters: " + read);
		}
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
	/// A scanner commits the first reading of each choice, and exclusive first sets do not
	/// license that where an earlier alternative reads nothing at all: it matches in front of
	/// every later one. Such a rule is not scanned.
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
	/// On <c>xby</c> <c>'b'{2}</c> reads one <c>b</c> and fails; the scanner has to put the
	/// position back before it tries <c>'a'</c> and leaves the optional, or <c>R1</c> answers
	/// having read the <c>b</c> and <c>'y'</c> matches.
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
	/// The flat recognizer assigned a <c>turn</c> local it never declared (CS0103) — the same
	/// dead alternative as the scanner's CS0162, met on the flat path.
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

	/// <summary>
	/// The renderings of a grammar read over tokens: the engine, and the direct reader on each
	/// carrier it is read on.
	/// </summary>
	static IEnumerable<Assembly> OverTokens(string grammar, params string[] allowed)
	{
		yield return Built(grammar, lexical: true, direct: false);

		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
		{
			var options = Options(lexical: true, direct: true);

			options.Carrier = carrier;

			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics.Where(one => one.Id != GramCompiler.NotCut && !allowed.Contains(one.Id)));

			yield return EmittedCode.Compile(result.Sources[0].Text);
		}
	}

	/// <summary>The renderings of a grammar read over characters: the engine, and the direct reader on each carrier.</summary>
	static IEnumerable<Assembly> OverCharacters(string grammar)
	{
		yield return Built(grammar, lexical: false, direct: false);

		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
		{
			var options = Options(lexical: false, direct: true);

			options.Carrier = carrier;

			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			yield return EmittedCode.Compile(result.Sources[0].Text);
		}
	}

	/// <summary>
	/// What a reading of <c>Start</c> said: the whole input where no position is given, from the
	/// position or in the window where one is — its outcome, where and why, or that it accepted.
	/// </summary>
	static string Refusal(Assembly assembly, string input, int? at, int? length)
	{
		if (at is not { } from)
		{
			var whole = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			return whole.IsSuccess
				? "accepted"
				: $"{EmittedCode.Outcome(assembly, "Grammar", "TryParseStart", input)} at {whole.Position}: {whole.Error}";
		}

		var match = Positioned(assembly, input, from, length);

		return match.Outcome == "Success"
			? "accepted"
			: $"{match.Outcome} at {match.Position}: {EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", input, from, length).Error}";
	}

	/// <summary>
	/// Whether every rendering over tokens reads a piece of the input as the reading over
	/// characters does: the match form and the form that moves <c>at</c>, from a position or in
	/// a window, and the two over tokens agreeing with each other on where and how far.
	/// </summary>
	static void ReadsAsOverCharacters(string grammar, string input, int at, int? length)
	{
		var said     = $"\"{input}\" at {at}, length {length?.ToString() ?? "none"}";
		var expected = EmittedCode.Positioned(Built(grammar, lexical: false, direct: true), "Grammar", "TryParseStart", input, at, length);
		var first    = ((bool IsSuccess, string? Error, long Position, long Length)?)null;

		foreach (var assembly in OverTokens(grammar))
		{
			var match = EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", input, at, length);
			var read  = EmittedCode.Answered(assembly, "Grammar", "TryParseStart", input, at, length);

			Assert.True(expected.IsSuccess == match.IsSuccess, said);
			Assert.True(match.IsSuccess == read.Read, said);
			Assert.True(read.At == (match.IsSuccess ? match.Position + match.Length : at), said);

			if (match.IsSuccess)
				Assert.True(match.Position == (first ?? match).Position && match.Length == (first ?? match).Length, said);

			first ??= match;
		}
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

	/// <summary>A reading from a position or in a window: its outcome, where it began and how far it read.</summary>
	static (string Outcome, long Position, long Length) Positioned(Assembly assembly, string input, int at, int? length)
	{
		var type  = assembly.GetType("Grammar")!;
		var match = length is { } seen
			? type.GetMethod("TryParseStart", [typeof(string), typeof(int), typeof(int)])!.Invoke(null, [input, at, seen])!
			: type.GetMethod("TryParseStart", [typeof(string), typeof(int)])!.Invoke(null, [input, at])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (Read("Outcome")!.ToString()!, Convert.ToInt64(Read("Position")), Convert.ToInt64(Read("Length")));
	}

	/// <summary>A whole reading: its outcome and where it stopped.</summary>
	static (string Outcome, long Position) Whole(Assembly assembly, string input)
	{
		var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (Read("Outcome")!.ToString()!, Convert.ToInt64(Read("Position")));
	}

	/// <summary>The whole reading's <c>bool</c> form: only whether it read.</summary>
	static bool WholeRead(Assembly assembly, string input)
	{
		var found = assembly.GetType("Grammar")!.GetMethods().Single(one =>
			one.Name == "TryParseStart" && one.GetParameters() is { Length: 2 } taken && taken[1].IsOut);

		return (bool)found.Invoke(null, [input, null])!;
	}

	static (bool Read, int At) Moved((bool Read, object? Value, int At) answer)
	{
		return (answer.Read, answer.At);
	}
}
