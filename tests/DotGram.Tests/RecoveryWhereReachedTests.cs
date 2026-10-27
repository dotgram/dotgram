using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule that marks a repetition <c>recover</c> recovers as written wherever it is reached: as an
/// alternative of a choice, under an optional, inside a lookahead, at the head of a sequence or
/// behind a part that may read nothing, and as the element of a repetition (§8.2, "a rule means one
/// thing everywhere it is called"). Each reading is held to a model built from nothing but what
/// reading the rule itself decides at each place, over every input up to a length, with the direct
/// reader and without it, from every position of a string and whole through a reader.
/// </summary>
/// <remarks>
/// A recovering repetition steps over a bad element whatever it begins with, so it can begin with
/// any character. What a choice or an optional predicts from was what a good element begins with:
/// on <c>;.</c> the rule read its bad first element and stopped after the <c>.</c>, and every place
/// that predicted it before calling it was told no.
/// </remarks>
public sealed class RecoveryWhereReachedTests
{
	const string Grammar =
		"""
		Row      = 'a' & ';'
		Rows     = Row{0} recover ';' & '.'
		Choice   = Rows | 'q'
		Second   = 'q' | Rows
		Three    = 'q' | Rows | 'b'
		Optional = Rows?
		Ahead    = ?=(Rows | 'q') & any*
		Not      = ?!(Rows | 'q') & any*
		Prefix   = Rows & 'b'
		Nullable = 'b'? & Rows
		Repeated = (Rows | 'q')*
		parse Rows
		parse Choice
		parse Second
		parse Three
		parse Optional
		parse Ahead
		parse Not
		parse Prefix
		parse Nullable
		parse Repeated
		""";

	public static TheoryData<bool> Directs => [true, false];

	/// <summary>The rows at least once and not at all, with the direct reader and without it.</summary>
	public static TheoryData<string, bool> Ways => new()
	{
		{ "+", true },
		{ "+", false },
		{ "*", true },
		{ "*", false },
	};

	/// <summary>
	/// With the direct reader and without it, and cut in two (<c>Lexical</c>), whose syntactic half
	/// recovers on the engine only.
	/// </summary>
	public static TheoryData<bool, bool> DirectsAndCuts => new()
	{
		{ true,  false },
		{ false, false },
		{ false, true },
	};

	/// <summary>
	/// Every context reads as its model says from reading <c>Rows</c> where it stands, with rows
	/// wanted at least once and not at all: over every input of up to five characters of <c>a;.qb</c>. What <c>Rows</c> reads at a place is its
	/// reading from that position, compiled for the positional forms (§6.3), which give a rule's
	/// first reading; those forms of every context are held to it too, and the whole forms of both
	/// compilations.
	/// </summary>
	[Theory]
	[MemberData(nameof(Ways))]
	public void A_recovering_rule_reads_as_itself_wherever_it_is_reached(string repeated, bool direct)
	{
		var grammar    = Grammar.Replace("{0}", repeated);
		var positional = Compiled(grammar, direct, positional: true);
		var ended      = Compiled(grammar, direct, positional: false);
		var failures   = new List<string>();

		foreach (var input in Inputs("a;.qb", 5))
		{
			// Over characters a rule gives a reading back where what follows it fails, so what
			// Rows reads at a place is every place it can end, the nearest first: where reading it
			// whole over the text between succeeds. Its reading from the position is the first.
			var rows = Enumerable.Range(0, input.Length + 1)
				.Select(at => Enumerable.Range(at, input.Length - at + 1).Where(end => Whole(positional, "Rows", input[at..end], reader: false)).ToArray())
				.ToArray();

			for (var at = 0; at <= input.Length; at++)
				if (Reads(positional, "Rows", input, at) != rows[at].DefaultIfEmpty(-1).First())
					failures.Add($"Rows on \"{input}\" from {at}: read to {Reads(positional, "Rows", input, at)}, its nearest whole reading is {rows[at].DefaultIfEmpty(-1).First()}");

			IEnumerable<int> Rows(int at)
			{
				return at <= input.Length ? rows[at] : [];
			}

			bool Is(int at, char c)
			{
				return at < input.Length && input[at] == c;
			}

			IEnumerable<int> Choice(int at)
			{
				foreach (var end in Rows(at))
					yield return end;

				if (Is(at, 'q'))
					yield return at + 1;
			}

			IEnumerable<int> Repeated(int at)
			{
				foreach (var end in Choice(at))
					if (end > at)
						foreach (var further in Repeated(end))
							yield return further;

				yield return at;
			}

			bool Looked(int at)
			{
				return Choice(at).Any();
			}

			// What each reads from a place, first reading first.
			var models = new (string Rule, Func<int, IEnumerable<int>> Ends)[]
			{
				("Rows",     Rows),
				("Choice",   Choice),
				("Second",   at => (Is(at, 'q') ? [at + 1] : Array.Empty<int>()).Concat(Rows(at))),
				("Three",    at => (Is(at, 'q') ? [at + 1] : Array.Empty<int>()).Concat(Rows(at)).Concat(Is(at, 'b') ? [at + 1] : [])),
				("Optional", at => Rows(at).Append(at)),
				("Ahead",    at => Looked(at) ? [input.Length] : []),
				("Not",      at => !Looked(at) ? [input.Length] : []),
				("Prefix",   at => Rows(at).Where(end => Is(end, 'b')).Select(static end => end + 1)),
				("Nullable", at => (Is(at, 'b') ? Rows(at + 1) : []).Concat(Rows(at))),
				("Repeated", Repeated),
			};

			foreach (var (rule, ends) in models)
			{
				var whole = ends(0).Contains(input.Length);

				for (var at = 0; at <= input.Length; at++)
				{
					var expected = ends(at).DefaultIfEmpty(-1).First();
					var actual   = Reads(positional, rule, input, at);

					if (actual != expected)
						failures.Add($"{rule} on \"{input}\" from {at}: read to {actual}, expected {expected}");
				}

				foreach (var (type, compiled) in new[] { (positional, "positional"), (ended, "whole") })
					foreach (var reader in new[] { false, true })
						if (Whole(type, rule, input, reader) != whole)
							failures.Add($"{rule} on \"{input}\" whole {(reader ? "through a reader" : "as a string")}, compiled {compiled}: expected {(whole ? "a match" : "a refusal")}");
			}
		}

		Assert.True(failures.Count == 0, $"{failures.Count} disagreements:\n{string.Join("\n", failures.Take(40))}");
	}

	/// <summary>
	/// The reported shape: a choice, an optional and a look of one over <c>;.</c>, over characters
	/// and cut in two, where a rule over kinds keeps its first reading and so is held to no model
	/// above.
	/// </summary>
	[Theory]
	[MemberData(nameof(DirectsAndCuts))]
	public void A_bad_first_element_is_stepped_over_through_a_choice(bool direct, bool lexical)
	{
		// Cut in two only where something is trivia: a space, which the input does not hold.
		var type = Compiled(
			(lexical ? "trivia = { ' '* }\n" : "") +
			"""
			Row  = 'a' & ';'
			Rows = Row+ recover ';' & '.'
			R3   = Rows | 'q'
			Opt  = Rows?
			Look = ?=R3 & Rows
			parse Rows
			parse R3
			parse Opt
			parse Look
			""",
			direct,
			positional: false,
			lexical);

		// A grammar cut in two has no reader yet.
		foreach (var rule in new[] { "Rows", "R3", "Opt", "Look" })
			foreach (var reader in lexical ? new[] { false } : [false, true])
				Assert.True(Whole(type, rule, ";.", reader), $"{rule} on \";.\" {(reader ? "through a reader" : "as a string")}");
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>Every text of up to <paramref name="length"/> characters of <paramref name="alphabet"/>, the empty one first.</summary>
	static IEnumerable<string> Inputs(string alphabet, int length)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var size = 1; size <= length; size++)
		{
			level = [.. level.SelectMany(prefix => alphabet.Select(one => prefix + one))];

			foreach (var input in level)
				yield return input;
		}
	}

	/// <summary>Where reading the rule from <paramref name="at"/> stops, or -1 where it is refused there.</summary>
	static int Reads(Type type, string rule, string input, int at)
	{
		var found = type.GetMethods().Single(one =>
			one.Name == "TryParse" + rule &&
			one.GetParameters() is { Length: 3 } taken &&
			taken[0].ParameterType == typeof(string) &&
			taken[1].ParameterType.IsByRef && !taken[1].IsOut);

		var parameters = new object?[] { input, at, null };

		return (bool)found.Invoke(null, parameters)! ? (int)parameters[1]! : -1;
	}

	/// <summary>Whether a whole parse reads the input, as a string or through a reader.</summary>
	static bool Whole(Type type, string rule, string input, bool reader)
	{
		var match = reader
			? type.GetMethod("TryParse" + rule, [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), 1, null])!
			: type.GetMethod("TryParse" + rule, [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static Type Compiled(string grammar, bool direct, bool positional, bool lexical = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName        = "Grammar",
			Direct           = direct,
			PositionalFollow = positional,
			Lexical          = lexical,
			BufferedInput    = !lexical,
			CSharpScanner    = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return assembly.GetType("Grammar")!;
	}
}
