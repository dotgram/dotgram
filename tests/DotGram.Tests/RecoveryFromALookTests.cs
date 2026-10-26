using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule that marks a repetition <c>recover</c> recovers as written wherever it is reached from:
/// called directly, asked by a lookahead, or looked for as a synchronization (§8.2, "a rule plans
/// its own"). Each answer is held to what reading the rule itself decides, over every input up to
/// a length, with the direct reader and without it, as a string and through a reader.
/// </summary>
/// <remarks>
/// Such a rule that records nothing was read by a scanner, which commits to what it reads and so
/// read the marked repetition as a plain one: a lookahead or a synchronization asking for it was
/// told no where the rule itself, and the reader over a <see cref="TextReader"/>, said yes.
/// </remarks>
public sealed class RecoveryFromALookTests
{
	/// <summary>
	/// The rule, reached from a positive and a negative lookahead and directly. <c>Row+</c> wants one
	/// row at least, so a bad row stepped over is the difference between reading <c>Rows</c> and not.
	/// </summary>
	const string Looked =
		"""
		Row    = 'a' & ';'
		Rows   = Row+ recover ';' & '.'
		Ahead  = ?=Rows & any*
		Not    = ?!Rows & any*
		Direct = Rows & any*
		parse Rows
		parse Ahead
		parse Not
		parse Direct
		""";

	/// <summary>The same rule as the synchronization of another recovering repetition.</summary>
	const string Synced =
		"""
		Row   = 'a' & ';'
		Rows  = Row+ recover ';' & '.'
		Item  = 'x'
		Outer = Item* recover Rows & 'z'
		parse Rows
		parse Outer
		""";

	public static TheoryData<bool> Directs => [true, false];

	/// <summary>
	/// What a lookahead answers is what reading <c>Rows</c> where it stands decides, and a call of it
	/// in a sequence reads it the same way: over every input of up to six characters of <c>a;.b</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Directs))]
	public void A_look_answers_as_reading_the_rule_itself_decides(bool direct)
	{
		var type = Compiled(Looked, direct);

		foreach (var input in Inputs("a;.b", 6))
		{
			var itself = Reads(type, "TryParseRows", input, 0) >= 0;

			foreach (var reader in new[] { false, true })
			{
				var how = $"\"{input}\" {(reader ? "through a reader" : "as a string")}";

				Assert.True(Whole(type, "TryParseAhead", input, reader) == itself, $"?=Rows on {how}: reading Rows says {itself}");
				Assert.True(Whole(type, "TryParseDirect", input, reader) == itself, $"Rows & any* on {how}: reading Rows says {itself}");
				Assert.True(Whole(type, "TryParseNot", input, reader) == !itself, $"?!Rows on {how}: reading Rows says {itself}");
			}
		}
	}

	/// <summary>
	/// Where a bad element is stepped over to, the synchronization is found as reading <c>Rows</c>
	/// finds it: the first place at or after the bad element where <c>Rows</c> reads, and past what it
	/// read. Held to a model of §8.2 over the rule's own positional reading, over every input of up to
	/// five characters of <c>xa;.bz</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Directs))]
	public void A_synchronization_is_found_as_reading_the_rule_itself_finds_it(bool direct)
	{
		var type = Compiled(Synced, direct);

		foreach (var input in Inputs("xa;.bz", 5))
		{
			var expected = Outer(input, at => Reads(type, "TryParseRows", input, at));

			foreach (var reader in new[] { false, true })
				Assert.True(
					Whole(type, "TryParseOuter", input, reader) == expected,
					$"\"{input}\" {(reader ? "through a reader" : "as a string")}: expected {(expected ? "a match" : "a refusal")}");
		}
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// <c>Item* recover Rows &amp; 'z'</c> read whole, as §8.2 says it: at each boundary the rest of
	/// the parse is tried first; then an element; an element that fails where input remains is a bad
	/// one, stepped over past the first place from it where <c>Rows</c> reads, or to the end where
	/// there is none.
	/// </summary>
	/// <param name="rows">Where reading <c>Rows</c> from a place stops, or -1 where it refuses.</param>
	static bool Outer(string input, Func<int, int> rows)
	{
		var at = 0;

		while (true)
		{
			if (input.Substring(at) == "z")
				return true;

			if (at < input.Length && input[at] == 'x')
			{
				at++;

				continue;
			}

			if (at >= input.Length)
				return false;

			var resumed = input.Length;

			for (var from = at; from < input.Length; from++)
			{
				var end = rows(from);

				if (end >= 0)
				{
					resumed = end;

					break;
				}
			}

			at = resumed;
		}
	}

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
	static int Reads(Type type, string method, string input, int at)
	{
		var found = type.GetMethods().Single(one =>
			one.Name == method &&
			one.GetParameters() is { Length: 3 } taken &&
			taken[1].ParameterType.IsByRef && !taken[1].IsOut);

		var parameters = new object?[] { input, at, null };

		return (bool)found.Invoke(null, parameters)! ? (int)parameters[1]! : -1;
	}

	/// <summary>Whether a whole parse reads the input, as a string or through a reader.</summary>
	static bool Whole(Type type, string method, string input, bool reader)
	{
		var match = reader
			? type.GetMethod(method, [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), 1, null])!
			: type.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static Type Compiled(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Direct        = direct,
			BufferedInput = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return assembly.GetType("Grammar")!;
	}
}
