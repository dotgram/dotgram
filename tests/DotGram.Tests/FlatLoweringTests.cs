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

	static void AssertNotLowered(string source)
	{
		Assert.Contains("ParserArena", source);
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
	public void A_capture_anywhere_in_the_grammar_falls_back_to_the_shared_engine()
	{
		// Everything else about this grammar is silent; the one named capture — deferred
		// construction needs it kept — is the sole disqualifying feature.
		const string grammar = "Start = value: \"h\"\nparse Start";

		AssertNotLowered(Emit(grammar));
	}

	[Fact]
	public void An_ambiguous_choice_falls_back_to_the_shared_engine()
	{
		// "ab" and "a" share a first character, so the choice is not predictive and needs
		// a resume point to come back for the other alternative.
		const string grammar = "Start = \"ab\" | \"a\"\nparse Start";

		AssertNotLowered(Emit(grammar));
	}
}
