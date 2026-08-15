using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// §7.6: an error in the author's C# is reported where the author wrote it.
/// </summary>
/// <remarks>
/// <para>
/// Everything in a generated file is machine-written and an error in it is this
/// compiler's bug — except the C# a grammar hands across, which is the author's and which
/// the C# compiler will have its own things to say about. Without a <c>#line</c> over it
/// those things are said inside a file the author did not write, cannot edit, and will be
/// told is auto-generated.
/// </para>
/// <para>
/// What is asserted here is the directive, not the experience: that the line and column
/// are the ones the grammar has, that the generated file still compiles with the
/// directives in it, and that a caller with no file to point at gets none. The last is
/// the one worth stating out loud — a directive naming a file that does not exist sends
/// the reader somewhere worse than the generated code.
/// </para>
/// </remarks>
public sealed class LineDirectiveTests
{
	const string Grammar =
		"""
		Number : @int = digits: ['0'..'9']+ => @int.Parse(digits)
		Start  : @int = value: Number & eol => @(value * 2)
		parse Start
		""";

	[Fact]
	public void The_C_sharp_a_grammar_supplied_is_written_under_a_directive()
	{
		var generated = Generate(Grammar, "Numbers.gram");

		// Line 1 is where `int.Parse(digits)` is written, and column 45 is where in it.
		Assert.Contains("#line 1 \"Numbers.gram\"", generated, StringComparison.Ordinal);
		Assert.Contains("#line 2 \"Numbers.gram\"", generated, StringComparison.Ordinal);

		var under = LineAfter(generated, "#line 1 \"Numbers.gram\"");

		Assert.Equal("int.Parse(digits);", under.TrimStart());
		Assert.Equal(Grammar.IndexOf("int.Parse", StringComparison.Ordinal) + 1, under.Length - under.TrimStart().Length + 1);
	}

	[Fact]
	public void A_guard_gets_one_too()
	{
		var generated = Generate(
			"Start = digits: ['0'..'9']+ & where @(digits.Length == 4) & eol\nparse Start",
			"Guarded.gram");

		Assert.Contains("#line 1 \"Guarded.gram\"", generated, StringComparison.Ordinal);
		Assert.Contains("(digits.Length == 4);", LineAfter(generated, "#line 1 \"Guarded.gram\""), StringComparison.Ordinal);
	}

	[Fact]
	public void Every_directive_is_closed()
	{
		var generated = Generate(Grammar, "Numbers.gram");

		// `#line default` hands the next line back to the file it is really in. Without
		// it the rest of the generated file would go on being attributed to the grammar,
		// and this compiler's own bugs would be reported on the author's line.
		Assert.Equal(
			generated.Split('\n').Count(line => line.StartsWith("#line ", StringComparison.Ordinal)) -
			generated.Split('\n').Count(line => line.StartsWith("#line default", StringComparison.Ordinal)),
			generated.Split('\n').Count(line => line.StartsWith("#line default", StringComparison.Ordinal)));
	}

	[Fact]
	public void The_generated_file_still_compiles_with_them() =>
		// Directives sit at column 0 inside a class body, which is legal and is worth
		// pinning: a `#line` in the wrong place is a C# error in the consumer's build.
		EmittedCode.Compile(Generate(Grammar, "Numbers.gram"), "Grammar", null);

	[Fact]
	public void Without_a_map_there_are_no_directives() =>
		Assert.DoesNotContain("#line", Generate(Grammar, path: null), StringComparison.Ordinal);

	static string Generate(string grammar, string? path)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				LineMap       = path is null ? null : new GrammarLineMap(grammar, path),
			});

		Assert.Empty(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}

	/// <summary>The line following the one that is exactly this directive.</summary>
	static string LineAfter(string generated, string directive)
	{
		var lines = generated.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();

		for (var i = 0; i < lines.Length - 1; i++)
			if (lines[i] == directive)
				return lines[i + 1];

		Assert.Fail($"No line reads '{directive}'.");

		return "";
	}
}
