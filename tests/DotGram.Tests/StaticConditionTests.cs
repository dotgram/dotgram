using System;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>when A is B</c>: a question about the grammar, answered while the parser is built.
/// </summary>
/// <remarks>
/// <para>
/// The point of it is <c>with</c>. A rule substituted at every one of its uses gives every
/// condition that reads it a different answer, so one grammar publishes several parsers
/// that read different languages — a version, a product, or the union of them all.
/// </para>
/// <para>
/// What is tested is the reading and not the shape of the tree: a condition that failed
/// takes its alternative with it, so the proof is that one parser refuses what the other
/// reads, from the same grammar and with nothing tested while either of them runs.
/// </para>
/// </remarks>
public sealed class StaticConditionTests
{
	/// <summary>
	/// One grammar, three parsers: an old dialect, a new one, and the union of both.
	/// </summary>
	/// <remarks>
	/// `*=` is an outer join SQL Server removed and `!=` one it kept, which is the shape of
	/// the problem a version chain cannot state: a real removal. `parse Both`, with no
	/// `with`, keeps the grammar's own `Version` — the choice of everything — so the
	/// permissive parser is the widest argument rather than a mode of its own.
	/// </remarks>
	const string Dialects = """
		[DotGram.Gram("Version = \"Old\" | \"New\"\nJoin : @string = t: (Name & \"*=\" & Name) & when Version is \"Old\" => @(t)\n | t: (Name & \"!=\" & Name) & when Version is \"New\" => @(t)\nName = ['a'..'z']+\nparse Join with (Version = \"Old\") as Old\nparse Join with (Version = \"New\") as New\nparse Join as Both")]
		public static partial class Dialects { }
		""";

	[Fact]
	public void A_condition_that_failed_takes_its_alternative_out_of_that_parser()
	{
		var made = GeneratorDriverTests.Build(Dialects).GetType("Dialects")!;

		bool Reads(string entry, string input)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, [input])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		// Each parser reads its own dialect and refuses the other's.
		Assert.True (Reads("Old", "a*=b"));
		Assert.False(Reads("Old", "a!=b"));

		Assert.False(Reads("New", "a*=b"));
		Assert.True (Reads("New", "a!=b"));

		// And the one that named no version reads both.
		Assert.True (Reads("Both", "a*=b"));
		Assert.True (Reads("Both", "a!=b"));
	}

	[Fact]
	public void A_range_is_a_rule_and_needs_no_operator_of_its_own()
	{
		// `is` is non-empty intersection, so `Version is Since2` asks exactly what
		// `Version is "V2"` asks — which is what lets one construct serve versions, which
		// are ordered, and products, which are not.
		var made = GeneratorDriverTests.Build("""
			[DotGram.Gram("Version = \"V1\" | \"V2\" | \"V3\"\nSince2 = \"V2\" | \"V3\"\nWord : @string = t: (Name & \"!\") & when Version is Since2 => @(t)\n | t: Name => @(t)\nName = ['a'..'z']+\nparse Word with (Version = \"V1\") as Early\nparse Word with (Version = \"V3\") as Late")]
			public static partial class Ranged { }
			""").GetType("Ranged")!;

		bool Reads(string entry)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, ["ab!"])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		// The gated alternative is the one that reads the `!`, so the early parser has no way
		// to take it and the late one does.
		Assert.False(Reads("Early"));
		Assert.True (Reads("Late"));
	}

	[Fact]
	public void A_side_that_is_not_a_listable_set_of_strings_is_said_rather_than_guessed()
	{
		// Refusing to decide out loud is the only honest answer: deciding it either way
		// silently changes what the parser reads.
		var run = GeneratorDriverTests.RunGenerator("""
			[DotGram.Gram("Version = \"V1\"\nAny = ['a'..'z']+\nWord : @string = t: (Name & \"!\") & when Any is Version => @(t)\nName = ['a'..'z']+\nparse Word")]
			public static partial class Undecided { }
			""");

		Assert.Contains(run.Diagnostics, one => one.Id == "GRAM4021");
	}

	[Fact]
	public void A_word_boundary_is_no_obstacle_to_a_side()
	{
		// A keyword in a grammar with a `wordboundary` is the literal between two looks — no
		// word before it, none after. Asked of a side read as a whole input both hold, and
		// before they were understood every version written with digits was undecidable once
		// digits continued a word, which is T-SQL's case.
		var made = GeneratorDriverTests.Build("""
			[DotGram.Gram("trivia = ' '*\nwordboundary = ['a'..'z' | '0'..'9']\nVersion = \"v100\" | \"v160\"\nSince160 = \"v160\"\nName = { ['a'..'z']+ }\nWord : @string\n = t: (Name & \"!\") & when Version is Since160 => @(t)\n | t: Name => @(t)\nparse Word with (Version = \"v100\") as Early\nparse Word with (Version = \"v160\") as Late")]
			public static partial class Bounded { }
			""").GetType("Bounded")!;

		bool Reads(string entry, string input)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, [input])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		Assert.False(Reads("Early", "ab!"));
		Assert.True (Reads("Late",  "ab!"));
	}

	[Fact]
	public void Words_with_trivia_between_them_are_not_a_listable_set()
	{
		// `"is" & "not"` has trivia woven between its words and a boundary on each, so what it
		// reads is `is`, one space or more, and `not` — no bound on the spaces, and not a set
		// that can be listed. Spelling the trivia as nothing joined the words and had the
		// boundary refuse the join, so `Pair is "is not"` came out false — of the one string
		// anybody would write for it. (`isnot` is not in the pair at all: it is a syntax error.)
		var run = GeneratorDriverTests.RunGenerator("""
			[DotGram.Gram("trivia = ' '*\nwordboundary = ['a'..'z']\nPair = \"is\" & \"not\"\nName = { ['a'..'z']+ }\nWord : @string = t: (Name & \"?\") & when Pair is \"is not\" => @(t)\nparse Word")]
			public static partial class Spaced { }
			""");

		Assert.Contains(run.Diagnostics, one => one.Id == "GRAM4021");
	}

	[Fact]
	public void The_logic_is_C_sharps_and_binds_the_way_C_sharps_does()
	{
		// `is not`, `and`, `or` and brackets, with `and` binding tighter than `or`. There is
		// no combinator inside a pattern: `Version is ("V1" | "V2")` already says what
		// `Version is "V1" or "V2"` would, and the language keeps one way to say a thing.
		var made = GeneratorDriverTests.Build("""
			[DotGram.Gram("Version = \"V1\" | \"V2\" | \"V3\"\nName = ['a'..'z']+\nWord : @string\n = t: (Name & \"1\") & when Version is not \"V1\" => @(t)\n | t: (Name & \"2\") & when Version is \"V1\" or Version is \"V3\" => @(t)\n | t: (Name & \"3\") & when (Version is \"V1\" or Version is \"V2\") and Version is not \"V2\" => @(t)\nparse Word with (Version = \"V1\") as One\nparse Word with (Version = \"V2\") as Two")]
			public static partial class Logic { }
			""").GetType("Logic")!;

		bool Reads(string entry, string input)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, [input])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		// V1: `is not "V1"` fails, `is "V1" or is "V3"` holds, `("V1" or "V2") and not "V2"` holds.
		Assert.False(Reads("One", "ab1"));
		Assert.True (Reads("One", "ab2"));
		Assert.True (Reads("One", "ab3"));

		// V2: the first holds, the second does not, and the third loses its `and`.
		Assert.True (Reads("Two", "ab1"));
		Assert.False(Reads("Two", "ab2"));
		Assert.False(Reads("Two", "ab3"));
	}

	[Fact]
	public void A_literal_written_case_insensitively_is_every_way_of_spelling_it()
	{
		// `"v1"i` accepts "v1" and "V1", so it meets both spellings and each of them meets
		// it. Folding each side to one spelling instead answered `"v1"i is "v1"` with false,
		// which is the plainest possible wrong answer.
		var made = GeneratorDriverTests.Build("""
			[DotGram.Gram("Name = ['a'..'z']+\nWord : @string\n = t: (Name & \"1\") & when Version is \"v1\" => @(t)\n | t: (Name & \"2\") & when Version is \"V1\" => @(t)\nVersion = \"v1\"\nparse Word with (Version = \"v1\"i) as Loose\nparse Word with (Version = \"v1\") as Exact")]
			public static partial class Cased { }
			""").GetType("Cased")!;

		bool Reads(string entry, string input)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, [input])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		// Asked with `"v1"i`, both alternatives are in: it meets the lower spelling and the
		// upper one.
		Assert.True (Reads("Loose", "ab1"));
		Assert.True (Reads("Loose", "ab2"));

		// Asked with `"v1"`, only the alternative that spells it that way survives.
		Assert.True (Reads("Exact", "ab1"));
		Assert.False(Reads("Exact", "ab2"));
	}

	[Fact]
	public void A_condition_and_a_C_sharp_guard_stand_in_one_when()
	{
		// The fold gives a residue rather than a verdict: the static half prunes and the
		// C# half stays. `false and @(g)` deletes the alternative and never compiles `@(g)`;
		// `true and @(g)` leaves `@(g)` behind as an ordinary guard.
		var made = GeneratorDriverTests.Build("""
			[DotGram.Gram("Version = \"V1\" | \"V2\"\nDigits : @int = ['0'..'9']+ => @(int.Parse(parserText))\nSmall : @int\n = n: Digits & when Version is \"V1\" and @(n < 10) => @(n)\n | \"x\" => @(0)\nparse Small with (Version = \"V1\") as One\nparse Small with (Version = \"V2\") as Two")]
			public static partial class Mixed { }
			""").GetType("Mixed")!;

		bool Reads(string entry, string input)
		{
			var match = made.GetMethod("Try" + entry, [typeof(string)])!.Invoke(null, [input])!;

			return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
		}

		// V1 keeps the alternative, so the guard is what decides — and it still decides.
		Assert.True (Reads("One", "7"));
		Assert.False(Reads("One", "42"));

		// V2 does not have it at all, so no number is read and the guard was never built.
		Assert.False(Reads("Two", "7"));
		Assert.False(Reads("Two", "42"));

		// The alternative with no condition on it is in both.
		Assert.True (Reads("One", "x"));
		Assert.True (Reads("Two", "x"));
	}
}
