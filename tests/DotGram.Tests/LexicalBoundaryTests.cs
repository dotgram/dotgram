using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

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
		"(?!\"aa\" & ['a'..'c'])* & \"aa\" & \"a\"",
		"(?!\"aaa\" & ['a'..'c'])* & \"aaa\" & \"a\"",
		"(?!\"aa\" & ['a'])* & \"a\"",
		"(?!\"aa\" & ['a'..'c'])* & \"a\"{2}",
		"(?!\"aba\" & ['a'..'c'])* & \"ba\"",
		"(?!\"ab\" & ['a'..'c'])* & (?!\"ba\" & ['a'..'c'])* & 'a'",
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

	/// <summary>
	/// The machine of <paramref name="untils"/> untils of one delimiter of <paramref name="length"/>
	/// characters in a row and a <c>'b'</c> after them: how many states its construction met, and how
	/// many the deterministic machine has.
	/// </summary>
	static (int Built, int States) Chained(int untils, int length)
	{
		return Chained(untils, [new string('a', length)], "['a'|'b']");
	}

	/// <summary>The same with the delimiters taken in turn from <paramref name="delimiters"/>.</summary>
	static (int Built, int States) Chained(int untils, string[] delimiters, string item)
	{
		var runs    = Enumerable.Range(0, untils).Select(i => $"(?!\"{delimiters[i % delimiters.Length]}\" & {item})*");
		var grammar =
			$$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = { {{string.Join(" & ", runs)}} & 'b' }
			}
			Root = Lexical.T
			parse Root
			""";

		var clock     = Stopwatch.StartNew();
		var inventory = TerminalInventory.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));
		var machine   = inventory.Machine!;

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"{untils} untils of {string.Join(",", delimiters)}: {machine.Nondeterministic} states built, {machine.Next.Count} deterministic, {clock.ElapsedMilliseconds} ms");

		return (machine.Nondeterministic, machine.Next.Count);
	}

	/// <summary>
	/// What a run hands on is carried by the subset construction, once a state and debt, and not by
	/// a copy of the machine of what follows for every debt: copied, each until copied the copies of
	/// the ones after it, and ten untils of a twelve-character delimiter came to a million and a half
	/// states for a machine of thirteen. Doubling the untils at most doubles what is built, and
	/// doubling the delimiter at most squares it — a debt is a set of the delimiter's prefixes, met
	/// at each state of the later runs.
	/// </summary>
	[Fact]
	public void Untils_in_a_row_cost_the_states_of_each_and_no_product_of_them()
	{
		var byUntils = new[] { 1, 2, 4, 8 }.Select(untils => Chained(untils, 4)).ToList();
		var byLength = new[] { 4, 8, 16 }.Select(length => Chained(8, length)).ToList();

		for (var i = 1; i < byUntils.Count; i++)
			Assert.True(byUntils[i].Built <= byUntils[i - 1].Built * 2 + 16, $"{byUntils[i - 1].Built} then {byUntils[i].Built} states built for twice the untils");

		for (var i = 1; i < byLength.Count; i++)
			Assert.True(byLength[i].Built <= byLength[i - 1].Built * 4 + 16, $"{byLength[i - 1].Built} then {byLength[i].Built} states built for twice the delimiter");

		// One state a length of matched delimiter, and the end: the deterministic machine is the same
		// however many runs it took.
		Assert.All(byUntils, one => Assert.Equal(5, one.States));
		Assert.Equal([5, 9, 17], byLength.Select(static one => one.States));

		Assert.True(Chained(10, 12).Built < 2_000);
	}

	/// <summary>
	/// Untils of eight delimiters of two to eight characters taken in turn: a debt is a set of
	/// prefixes for each delimiter still owed, so what is built grows faster than the untils while
	/// delimiters are still being added to the mix (three times from sixteen to thirty-two) and
	/// then nearly as they do (2.4 times from thirty-two to sixty-four); the deterministic machine
	/// grows as they do throughout. A product of the untils would be thousands of times.
	/// </summary>
	[Fact]
	public void Untils_of_mixed_delimiters_in_a_row_cost_no_product_of_them()
	{
		string[] mixed = ["ab", "abab", "aab", "abcab", "aaab", "ababab", "abcabca", "aabaabaa"];

		var built = new[] { 16, 32, 64 }.Select(untils => Chained(untils, mixed, "['a'..'c']")).ToList();

		for (var i = 1; i < built.Count; i++)
		{
			Assert.True(built[i].Built <= built[i - 1].Built * 7 / 2, $"{built[i - 1].Built} then {built[i].Built} states built for twice the untils");
			Assert.True(built[i].States <= built[i - 1].States * 9 / 4, $"{built[i - 1].States} then {built[i].States} deterministic states for twice the untils");
		}
	}

	/// <summary>
	/// <paramref name="runs"/> turns of an until of <paramref name="length"/> <c>a</c>s and an
	/// <c>'a'</c> after it, then a <c>'b'</c>: each turn's run hands on the prefixes it still
	/// matched, and the next turns' runs match more of the same delimiter.
	/// </summary>
	static LexicalAutomaton Repeated(int runs, int length)
	{
		var grammar =
			$$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = { ((?!"{{new string('a', length)}}" & ['a'|'b'])* & 'a'){{{runs}}} & 'b' }
			}
			Root = Lexical.T
			parse Root
			""";

		var machine = TerminalInventory.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!))).Machine!;

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"{runs} turns of {length}: {machine.Nondeterministic} states built, {machine.Next.Count} deterministic, {machine.Walked} walked");

		return machine;
	}

	/// <summary>
	/// A debt keeps a prefix only where no longer one it holds answers for it, so the debts a
	/// repetition of runs hands on are a prefix each and not every set of borders: eight turns of
	/// sixteen built a quarter of a million states for a machine of 270. Doubling the turns and the
	/// delimiter together now builds about seven times as much, which is what the turns times the
	/// delimiter's length squared allow.
	/// </summary>
	[Fact]
	public void Turns_of_an_until_hand_on_one_prefix_and_not_every_set_of_them()
	{
		var built = new[] { (4, 8), (8, 16), (16, 32) }.Select(one => Repeated(one.Item1, one.Item2)).ToList();

		for (var i = 1; i < built.Count; i++)
			Assert.True(built[i].Nondeterministic <= built[i - 1].Nondeterministic * 8, $"{built[i - 1].Nondeterministic} then {built[i].Nondeterministic} states built");

		Assert.Equal([64, 270, 1114], built.Select(static one => one.Next.Count));
	}
}
