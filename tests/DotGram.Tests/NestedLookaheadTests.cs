using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A look inside a look, and a look inside an atomic group, held to the reference interpreter on
/// every rendering: the shapes each defect was found on. DotGram.Tests.Slow sweeps the rest.
/// </summary>
/// <remarks>
/// <para>
/// The body of a look and the contents of an atomic group are each asked for their first reading
/// only, and four places took that to mean that nothing inside could be asked for another one.
/// Each held where the body ends in something that reads, and each was wrong where it ends in
/// something that reads nothing and can refuse — a look of its own, most often.
/// </para>
/// </remarks>
public sealed class NestedLookaheadTests
{
	/// <summary>Each shape: its grammar, the rule asked, and the alphabet its inputs are spelled in.</summary>
	static readonly Dictionary<string, (string Grammar, string Alphabet, int Longest)> Shapes = new()
	{
		// What may follow a part of the body was worked out as though nothing could begin after the
		// body, so past `?!'c'` only `c` could, and the word before it was read as never giving a
		// letter back: the look refused every word, not only the one a `c` follows.
		["FollowPastRefusal"] = ("Start = ?!(W & ?!'c') & W & 'c'\nW = ['a'..'b']+\nparse Start stream bytes\n", "abc", 4),

		// The direct reader asked the body once and sealed the ways it opened: `ab` matches the body
		// by giving the `b` back, and that reading was never tried.
		["BodyRetried"] = ("Start = ?!(['a'..'b']+ & ?='b') & ['a'..'c']*\nparse Start stream bytes\n", "abc", 4),

		// An optional at the end of an atomic group, before a refusal: its skip was not kept.
		["SkipKept"] = ("Start = { ('a' & 'b')? & ?!'b' } & ['a'..'c']*\nparse Start stream bytes\n", "abc", 4),

		// A scanner for a rule that is an atomic group read greedily up to the look.
		["ScannerTail"] = ("Start = L & ['a'..'c']+\nL = { ['a'..'b']+ & ?='b' }\nparse Start stream bytes\n", "abc", 4),

		// A refusal of one character with a look inside was taken to refuse the character wherever
		// it stands, and the optional that decides on what the rule begins with never entered it.
		["OneCharacter"] = ("Start = 'c' & L? & 'c'\nL = ?!(['a'..'b'] & ?!'c') & ['a'..'b'] & 'c'\nparse Start stream bytes\n", "abc", 4),
	};

	/// <summary>
	/// The expression language's `with`, as the issue found it: over tokens a rule that opened with
	/// <c>?!(Word &amp; ?!'{')</c> was told it could begin only with a brace, so the optional that
	/// calls it was never entered after a word.
	/// </summary>
	const string Change = """
		trivia = { ' '* }
		namespace Lex
		{
			trivia = none
			Word = ['a'..'b']+
		}
		Start = Lex.Word & Change?
		Change = ?!(Lex.Word & ?!'{') & w: Lex.Word & '{' & '}'
		parse Start

		""";

	public static TheoryData<string, string> Cases()
	{
		var data = new TheoryData<string, string>();

		foreach (var shape in Shapes.Keys)
			foreach (var rendering in NestedLookahead.Renderings)
				if (!rendering.StartsWith("tokens", StringComparison.Ordinal))
					data.Add(shape, rendering);

		data.Add("Change", "tokens");
		data.Add("Change", "tokens-direct");

		return data;
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void A_nested_look_answers_as_the_reference(string shape, string rendering)
	{
		var wrong = shape == "Change"
			? NestedLookahead.Disagreements(
				Change, rendering,
				[("Start", "the expression language's `with`", NestedLookahead.Inputs("a {}", 5))],
				reference: Change.Replace("['a'..'b']+", "{ ['a'..'b']+ }"))
			: NestedLookahead.Disagreements(
				Shapes[shape].Grammar, rendering,
				[("Start", shape, NestedLookahead.Inputs(Shapes[shape].Alphabet, Shapes[shape].Longest))]);

		Assert.True(wrong.Count == 0, $"{wrong.Count} answers differ:\n" + string.Join("\n", wrong.Take(20)));
	}
}
