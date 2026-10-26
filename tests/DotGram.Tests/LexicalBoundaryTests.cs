using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where a token ends: a terminal the lexer reads (<c>Lexical = true</c>) held to the same
/// grammar over characters, its scannerless twin, on every short input — the same outcome, and on
/// a reading the same tokens, which is the twin's accepted length taken as the token's.
/// </summary>
/// <remarks>
/// The terminals are untils, <c>(?!L &amp; X)* &amp; …</c>, the one lookahead the automaton builds
/// rather than refuses: where its delimiter follows it or not, overlaps itself or not, is all items
/// or not, comes before its own run, and with nothing after the run. Each is asked in braces, where
/// the terminal's own rule reads the token again from its start, and bare, where the automaton's
/// reading is the token; and with a second terminal of one character beside it and without, so
/// that a token ending early shows as the characters after it read as the second terminal.
/// </remarks>
public sealed class LexicalBoundaryTests
{
	static string Grammar(string token, bool other)
	{
		return
			$$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = {{token}}
				{{(other ? "U = ['a'..'c']" : "")}}
			}
			Item : @string = t: Lexical.T => @("t:" + t){{(other ? " | u: Lexical.U => @(\"u:\" + u)" : "")}}
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";
	}

	/// <summary>The outcome, and what was read where something was: the place of a refusal is not compared.</summary>
	static string Said(Assembly assembly, string input, int? at)
	{
		var type  = assembly.GetType("Grammar")!;
		var match = at is { } from
			? type.GetMethod("TryParseRoot", [typeof(string), typeof(int)])!.Invoke(null, [input, from])!
			: type.GetMethod("TryParseRoot", [typeof(string)])!.Invoke(null, [input])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"read {Read("Length")} «{Read("Value")}»"
			: $"{Read("Outcome")}";
	}

	public static readonly TheoryData<string> Shapes =
	[
		"(?!\"ab\" & ['a'..'c'])* & \"ab\"",
		"(?!\"aa\" & ['a'..'c'])* & \"aa\"",
		"(?!\"aba\" & ['a'..'c'])* & \"aba\"",
		"(?!\"ab\" & ['a'|'c'])* & \"ab\"",
		"(?!\"aaa\" & ['a'|'c'])* & \"aaa\"",
		"(?!\"ab\" & ['a'|'b'])* & 'b'",
		"(?!\"aa\" & ['a'..'c'])* & 'a'",
		"(?!\"ab\" & ['a'..'c'])* & 'c'",
		"(?!\"abc\" & ['a'..'c'])* & \"bc\"",
		"(?!\"abc\" & ['a'..'c'])* & 'b'",
		"(?!\"ab\" & ['a'..'c'])* & \"ab\" & ['b'|'c']",
		"(?!\"ab\" & ['a'..'c'])* & ('b' | 'c' & 'b')",
		"'c' & (?!\"ab\" & ['a'..'c'])* & \"ab\"",
		"\"ab\" & (?!\"ab\" & ['a'..'c'])* & \"ab\"",
		"(?!'b' & ['a'..'c'])* & 'b'",
		"'c' & (?!\"ab\" & ['a'..'c'])*",
	];

	/// <summary>
	/// Every input of up to five characters over the shape's alphabet, read whole and from its
	/// start, by the twin, by the reader over tokens and by the engine over tokens.
	/// </summary>
	[Theory]
	[MemberData(nameof(Shapes))]
	public void An_until_ends_its_token_where_the_rule_ends_it(string shape)
	{
		var log    = TestContext.Current.TestOutputHelper;
		var inputs = LexicalBindingProbe.All("abc", 5).ToList();
		var told   = new List<string>();
		var asked  = 0;

		foreach (var braced in new[] { true, false })
			foreach (var other in new[] { false, true })
			{
				var token   = braced ? "{ " + shape + " }" : shape;
				var grammar = Grammar(token, other);
				var chars   = LexicalBindingProbe.Build(grammar, lexical: false, direct: true);
				var reader  = LexicalBindingProbe.Build(grammar, lexical: true, direct: true);
				var engine  = LexicalBindingProbe.Build(grammar, lexical: true, direct: false);

				Assert.True(chars.Assembly is not null, chars.Note);
				Assert.True(reader.Assembly is not null, reader.Note);
				Assert.True(engine.Assembly is not null, engine.Note);

				// A grammar that cannot be cut, which an until the automaton cannot read exactly
				// is, compiles over characters, and there is nothing to hold apart.
				if (!reader.Cut)
				{
					log?.WriteLine($"{token}, second terminal {other}: not cut — {reader.Note}");

					continue;
				}

				foreach (var input in inputs)
					foreach (var at in new int?[] { null, 0 })
					{
						// Nothing at all read from a position is a window with no token in it,
						// which a reading over tokens refuses as starved where the twin reads
						// nothing: the token window's own answer, not where a token ends.
						if (input.Length == 0 && at is not null)
							continue;

						asked++;

						var twin = Said(chars.Assembly!, input, at);
						var read = Said(reader.Assembly!, input, at);
						var ran  = Said(engine.Assembly!, input, at);

						if (twin != read || twin != ran)
							told.Add($"{token}, second terminal {other}, '{input}'{(at is null ? "" : " from 0")}: twin {twin}, reader {read}, engine {ran}");
					}
			}

		log?.WriteLine($"{asked} readings asked, {told.Count} disagree");

		Assert.True(told.Count == 0, string.Join("\n", told.Take(20)));
	}

	/// <summary>
	/// The run of an until stops where its delimiter begins, even where the delimiter runs on
	/// past the run: here at the start, where <c>ab</c> begins, so the <c>'b'</c> after it finds an
	/// <c>a</c> and the terminal is refused. The automaton read the run as the strings the
	/// delimiter does not occur in, took <c>a</c> into it and <c>b</c> after it, and so read to the
	/// end, where it said that more input was wanted.
	/// </summary>
	[Theory]
	[InlineData("ab")]
	[InlineData("aab")]
	[InlineData("bab")]
	public void A_delimiter_begun_in_the_run_and_ended_past_it_stops_the_run(string input)
	{
		var grammar = Grammar("{ (?!\"ab\" & ['a'|'b'])* & 'b' }", other: false);
		var chars   = LexicalBindingProbe.Build(grammar, lexical: false, direct: true).Assembly!;
		var reader  = LexicalBindingProbe.Build(grammar, lexical: true, direct: true);

		Assert.True(reader.Cut, reader.Note);
		Assert.Equal("NoMatch", Said(chars, input, null));
		Assert.Equal("NoMatch", Said(reader.Assembly!, input, null));
	}
}
