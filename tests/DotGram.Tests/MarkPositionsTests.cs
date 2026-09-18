using System;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// `parserMarks` (§7.8): where each mark standing over a construction was placed, beside
/// `parserState` and in its order, in the units of `parserSpan.Start`.
/// </summary>
/// <remarks>
/// Asked of every way a value can be built — the engine's walk, the reader's walk, and the
/// reader that builds where it reads — over characters and over tokens, since over tokens a
/// machine's position is a token's number and what a construction is owed is where that token
/// begins in the text. An abandoned reading's marks go with it, positions and all.
/// </remarks>
public sealed class MarkPositionsTests
{
	const string Grammar =
		"state : @int\n" +
		"Start : @string = '(' & inner: Start with state @(9) & ';' => @(\"<\" + inner + \">\")\n" +
		"                | '(' & inner: Start with state @(1) & ')' => @(inner)\n" +
		"                | '[' & inner: Start with state @(2) & ']' => @(inner)\n" +
		"                | t: Word => @(string.Join(\",\", parserState.ToArray()) + \"@\" + " +
		"string.Join(\",\", parserMarks.ToArray()) + \":\" + t)\n" +
		"Word : @string = t: ['a'..'z']+ => @(t)\n" +
		"parse Start\n";

	public static TheoryData<string, bool> Readings => new()
	{
		{ "engine",    false },
		{ "tape",      false },
		{ "immediate", false },
		{ "engine",    true },
		{ "tape",      true },
		{ "immediate", true },
	};

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_construction_is_told_where_each_mark_over_it_was_placed(string reading, bool lexical)
	{
		var probe = Compiled(reading, lexical);

		Assert.Equal("@:a",         Value(probe, "a"));
		Assert.Equal("1@1:ab",      Value(probe, "(ab)"));
		Assert.Equal("1,2@1,2:a",   Value(probe, "([a])"));
		Assert.Equal("2,1,1@1,2,3:a", Value(probe, "[((a))]"));

		// The first alternative places a mark, reads the operand under it and is given up at
		// the `)`: what it placed goes with it.
		Assert.Equal("1@1:a", Value(probe, "(a)"));
		Assert.Equal("<9@1:a>", Value(probe, "(a;"));
	}

	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void Over_tokens_a_position_is_a_character_and_not_a_token(string reading)
	{
		var probe = Compiled(reading, lexical: true, trivia: true);

		// `(`, `[` and `a` are tokens 0, 1 and 2, and begin at characters 0, 3 and 7.
		Assert.Equal("1,2@3,7:a", Value(probe, "(  [   a ] )"));
	}

	static string? Value(Assembly probe, string input)
	{
		var match = EmittedCode.Match(probe, "Marked.Probe", "TryParseStart", input);

		Assert.True(match.IsSuccess, $"\"{input}\": {match.Error}");

		return match.Value?.ToString();
	}

	static Assembly Compiled(string reading, bool lexical, bool trivia = false)
	{
		var result = GramCompiler.Compile((trivia ? "trivia = ' '*\n" : "") + Grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Marked",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
			Direct        = reading != "engine",
			Carrier       = reading == "immediate" ? CarrierKind.Immediate : CarrierKind.Tape,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		if (reading == "engine")
			Assert.DoesNotContain("DirectValues", source, StringComparison.Ordinal);
		else if (reading == "immediate")
			Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		else
			Assert.Contains("DirectValues", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, "Probe", "Marked");
	}
}
