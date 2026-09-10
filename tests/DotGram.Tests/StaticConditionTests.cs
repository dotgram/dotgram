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
}
