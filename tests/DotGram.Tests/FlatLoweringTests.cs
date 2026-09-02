using System;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A publication needing none of the three things the arena is for — no recursion, no
/// backtracking, no deferred construction — compiles to a plain method instead of a state
/// in the shared automaton.
/// </summary>
/// <remarks>
/// One disqualifying rule anywhere in the grammar and nothing here applies: the fallback
/// path is exactly today's, unchanged. See docs/next.md, "Future optimization gate".
/// </remarks>
public sealed class FlatLoweringTests
{
	static string Emit(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info);

		return Assert.Single(result.Sources).Text;
	}

	static (bool Matched, string Value) Run(string grammar, string input)
	{
		var (isSuccess, value, _, _) =
			EmittedCode.Match(EmittedCode.Compile(Emit(grammar)), "Grammar", "TryParseStart", input);

		return (isSuccess, (string)(value ?? ""));
	}

	static void AssertLowered(string source)
	{
		// A flat-lowered rule still gets a Recognize_DotGram_ExpectedN table per terminal
		// (§11's first-tier diagnostics) — plain static arrays, declared under the same
		// synthetic-helper prefix guards already use, no arena, no dispatch, no pooling.
		// What actually distinguishes lowering is the absence of the arena machinery
		// itself, which is what these two check.
		Assert.DoesNotContain("ParserArena", source);
		Assert.DoesNotContain("RentParser", source);
	}

	/// <summary>
	/// Not the flat path: the engine and its arena, or the methods and their tape of ways
	/// back. Either holds what a flat method cannot — a way back that outlives the locals.
	/// </summary>
	static void AssertNotLowered(string source)
	{
		Assert.True(
			source.Contains("ParserArena", StringComparison.Ordinal) ||
			source.Contains("sealed class Ways", StringComparison.Ordinal),
			"Neither the arena nor the tape of ways back is in the source: the grammar lowered.");
		Assert.Contains("Recognize_DotGram", source);
	}

	[Fact]
	public void A_single_literal_lowers_and_still_matches_correctly()
	{
		const string grammar = "Start = \"h\"\nparse Start";

		AssertLowered(Emit(grammar));

		Assert.Equal((true, "h"), Run(grammar, "h"));
		Assert.Equal((false, ""), Run(grammar, "i"));
		Assert.Equal((false, ""), Run(grammar, "hh")); // parse demands the whole input
	}

	[Fact]
	public void A_predictive_choice_after_a_possessive_repeat_lowers()
	{
		// 'a'+ is possessive ahead of something it cannot itself begin with, and "cd"|"ef"
		// is predictive — nothing here ever needs to write down a way back.
		const string grammar = "Start = 'a'+ & (\"cd\" | \"ef\")\nparse Start";

		AssertLowered(Emit(grammar));

		Assert.Equal((true, "aaacd"), Run(grammar, "aaacd"));
		Assert.Equal((true, "aef"),   Run(grammar, "aef"));
		Assert.Equal((false, ""),     Run(grammar, "aaa"));
		Assert.Equal((false, ""),     Run(grammar, "cd")); // needs at least one 'a'
	}

	[Fact]
	public void A_capture_anywhere_in_the_grammar_does_not_lower()
	{
		// Everything else about this grammar is silent; the one named capture — deferred
		// construction needs it kept — is the sole disqualifying feature.
		const string grammar = "Start = value: \"h\"\nparse Start";

		AssertNotLowered(Emit(grammar));
	}

	[Fact]
	public void An_ambiguous_choice_lowers_with_its_way_back_in_locals()
	{
		// Shorter first, so the shorter one takes the position wherever the longer would
		// have and the choice needs coming back to. That used to be the arena's job and
		// cost the whole engine; the checkpoint class holds the way back in three locals
		// and resumes it from below `Fail:`, so the method stays flat.
		const string grammar = "Start = \"a\" | \"ab\"\nparse Start";
		var source = Emit(grammar);

		Assert.DoesNotContain("private sealed class Parser", source);
		Assert.Contains("way1 = p;", source);
	}

	[Fact]
	public void But_a_repetition_over_one_still_does_not_lower()
	{
		// One set of locals holds one activation, and every turn of the repetition would
		// need a pending way back of its own — so no checkpoint site may open under a
		// repetition, and the choice keeps its way back where locals cannot hold it.
		const string grammar = "Start = (\"a\" | \"ab\")* & 'c'\nparse Start";

		AssertNotLowered(Emit(grammar));
	}
}
