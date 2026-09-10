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
}
