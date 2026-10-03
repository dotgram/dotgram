using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every nested shape of a look inside a look or an atomic group, held to the reference
/// interpreter on every rendering. DotGram.Tests holds the shapes each defect was found on.
/// </summary>
/// <remarks>
/// Swept rather than listed: parts that read one character, give back or read another way; the
/// looks nested after or before them; sequences, choices and repetitions around both; and the
/// whole in a negative look, a positive one and an atomic group — written in place at the head of
/// a rule, and as a rule of its own that an optional and a choice decide on by what it begins
/// with. Over tokens the same, with the word a token, captured where the expression language
/// captures it.
/// </remarks>
public sealed class NestedLookaheadSweepTests
{
	/// <summary>Parts that read one character, give back, or read another way.</summary>
	static readonly string[] Parts = ["['a'..'b']", "['a'..'b']+", "W", "('a' | \"ab\")", "('a' & 'b')?"];

	/// <summary>The looks nested inside: over a character a part reads, over one none does, over two.</summary>
	static readonly string[] Inner = ["?!'b'", "?='b'", "?!'c'", "?!('b' & 'c')"];

	/// <summary>How a part and a nested look stand together in the outer body.</summary>
	static readonly string[] Arrangements =
	[
		"{0} & {1}",
		"{1} & {0}",
		"{0} & {1} & ['a'..'c']",
		"({0} & {1})+",
		"({0} & {1} | 'c')",
		"{0} & ({1} | 'c')",
	];

	/// <summary>What the body stands in: a negative look, a positive one, an atomic group.</summary>
	static readonly string[] Outer = ["?!({0})", "?=({0})", "{{ {0} }}"];

	/// <summary>
	/// Every shape twice: in place, at the head of <c>S</c>, and as a rule <c>L</c> of its own that
	/// <c>D</c> decides on by what it begins with. A part that is a call has a rule <c>W</c> of its
	/// own per shape, called after the look as well, so that what one shape tells a rule about its
	/// callers is not told by another.
	/// </summary>
	static (string Grammar, List<(string Rule, string Shown, IReadOnlyList<string> Inputs)> Rules) Characters()
	{
		var text   = new StringBuilder();
		var rules  = new List<(string, string, IReadOnlyList<string>)>();
		var whole  = NestedLookahead.Inputs("abc", 3);
		var closed = whole.Select(static one => one + ".").Append("..").ToList();

		foreach (var outer in Outer)
			foreach (var arrangement in Arrangements)
				foreach (var part in Parts)
					foreach (var inner in Inner)
					{
						// A turn that may read nothing is refused (GRAM4001).
						if (part.EndsWith("?", StringComparison.Ordinal) && arrangement.EndsWith(")+", StringComparison.Ordinal))
							continue;

						var n    = rules.Count / 2;
						var look = string.Format(outer, string.Format(arrangement, part.Replace("W", $"W{n}"), inner));

						text.Append($"S{n} = {look} & (W{n} & 'c')? & ['a'..'c']*\n");
						text.Append($"W{n} = ['a'..'b']+\n");
						text.Append($"D{n} = (L{n} & ['a'..'c']+ | '.')? & '.'\n");
						text.Append($"L{n} = {look}\n");
						text.Append($"parse S{n} stream bytes\n");
						text.Append($"parse D{n} stream bytes\n");

						rules.Add(($"S{n}", look, whole));
						rules.Add(($"D{n}", look, closed));
					}

		return (text.ToString(), rules);
	}

	/// <summary>The looks nested inside, over tokens.</summary>
	static readonly string[] InnerTokens = ["?!'{'", "?='{'", "?!Lex.Word", "?=Lex.Word"];

	/// <summary>How the word and a nested look stand together, over tokens.</summary>
	static readonly string[] TokenArrangements =
	[
		"Lex.Word & {0}",
		"{0} & Lex.Word",
		"(Lex.Word & {0} | '}}')",
		"(Lex.Word & {0})+",
	];

	/// <summary>
	/// The shapes over tokens: each at the head of a published rule, and in a rule of its own that
	/// an optional decides on — after a word, which it captures, as the expression language's `with`
	/// does, and between braces.
	/// </summary>
	/// <param name="atomic">
	/// The word held to one reading: what the lexer makes of it, and so what the reference
	/// interpreter, which reads characters, is to be given.
	/// </param>
	static (string Grammar, List<(string Rule, string Shown, IReadOnlyList<string> Inputs)> Rules) Tokens(bool atomic)
	{
		var text  = new StringBuilder();
		var rules = new List<(string, string, IReadOnlyList<string>)>();

		// Five characters, so that two words can stand apart before a brace.
		var inputs = NestedLookahead.Inputs("a {}", 5);

		text.Append("trivia = { ' '* }\n");
		text.Append("namespace Lex\n{\n\ttrivia = none\n");
		text.Append(atomic ? "\tWord = { ['a'..'b']+ }\n" : "\tWord = ['a'..'b']+\n");
		text.Append("}\n");

		// Any tokens, spaced: a repetition of a choice has no seam between its turns (§4.5), and a
		// sequence does.
		text.Append("Tail = (Lex.Word | '{' | '}') & Tail?\n");

		foreach (var outer in new[] { "?!({0})", "?=({0})" })
			foreach (var arrangement in TokenArrangements)
				foreach (var inner in InnerTokens)
				{
					var look = string.Format(outer, string.Format(arrangement, inner));
					var n    = rules.Count;

					text.Append($"S{n} = {look} & Tail?\n");
					text.Append($"S{n + 1} = Lex.Word & L{n + 1}?\n");
					text.Append($"L{n + 1} = {look} & w: Lex.Word & '{{' & '}}'\n");
					text.Append($"S{n + 2} = '}}' & L{n + 2}? & '}}'\n");
					text.Append($"L{n + 2} = {look} & Lex.Word & '{{'\n");
					text.Append($"parse S{n}\nparse S{n + 1}\nparse S{n + 2}\n");

					rules.Add(($"S{n}", look, inputs));
					rules.Add(($"S{n + 1}", look, inputs));
					rules.Add(($"S{n + 2}", look, inputs));
				}

		return (text.ToString(), rules);
	}

	public static TheoryData<string> Renderings()
	{
		return [.. NestedLookahead.Renderings];
	}

	[Theory]
	[MemberData(nameof(Renderings))]
	public void Every_nested_shape_answers_as_the_reference(string rendering)
	{
		List<string> wrong;

		if (rendering.StartsWith("tokens", StringComparison.Ordinal))
		{
			var (grammar, rules) = Tokens(atomic: false);

			wrong = NestedLookahead.Disagreements(grammar, rendering, rules, reference: Tokens(atomic: true).Grammar);
		}
		else
		{
			var (grammar, rules) = Characters();

			wrong = NestedLookahead.Disagreements(grammar, rendering, rules);
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} answers differ:\n" + string.Join("\n", wrong.Take(30)));
	}
}
