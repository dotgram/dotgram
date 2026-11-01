using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Reading a grammar takes time that grows with how deep its `when`s nest inside each other, not
/// with two to the power of it.
/// </summary>
/// <remarks>
/// <para>
/// A `when` is looked at once to tell a condition from a C# guard, by reading its operand, and then
/// read for real. An operand holding another `when` — in a call's arguments, `when P(when Q(…))` —
/// had it looked at when the outer one was and again when that was read for real, and so on down:
/// every level doubled the reading of everything inside it, and twenty-six levels of a valid grammar
/// took half a minute, well inside the nesting the parser allows. The look is now kept by where it
/// was asked, so each level is read a bounded number of times.
/// </para>
/// <para>
/// A ladder, so that what is held is the growth: every depth up to forty, each within a time that
/// any doubling would have passed by the twenty-fifth.
/// </para>
/// </remarks>
public sealed class ParserLadderTests
{
	public static TheoryData<string> Shapes()
	{
		return new TheoryData<string>("arguments", "guards", "conditions", "groups");
	}

	static string Grammar(string shape, int depth)
	{
		return shape switch
		{
			// A condition on a call whose argument is the next condition: `when P(…) is 'x'`.
			"arguments"  => "P(x) = x\nA = " + Nested(depth, "'x'", static inner => $"when P({inner}) is 'x'") + " & 'x'\nparse A\n",

			// The same with a C# guard among the conditions at every level.
			"guards"     => "P(x) = x\nA = " + Nested(depth, "'x'", static inner => $"when P({inner}) is 'x' and @(true)") +
				" & 'x'\nparse A\n",

			// One condition, its parentheses inside each other.
			"conditions" => "A = when " + new string('(', depth) + "'x' is 'x'" + new string(')', depth) + " & 'x'\nparse A\n",

			// Conditions inside groups.
			"groups"     => "X = 'x'\nA = " + string.Concat(Enumerable.Repeat("(when X is X & ", depth)) + "'x'" +
				new string(')', depth) + "\nparse A\n",

			_ => throw new ArgumentOutOfRangeException(nameof(shape)),
		};
	}

	static string Nested(int depth, string bottom, Func<string, string> level)
	{
		var text = bottom;

		for (var at = 0; at < depth; at++)
			text = level(text);

		return text;
	}

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Nested_whens_are_read_in_time_that_does_not_double_with_each_level(string shape)
	{
		// Once untimed: the first reading loads and compiles the lexer, the parser and Roslyn's
		// scanner of C#, which on a loaded machine is a second on its own.
		GramCompiler.Read(Grammar(shape, 1), RoslynCSharpScanner.Instance);

		for (var depth = 1; depth <= 40; depth++)
		{
			var grammar = Grammar(shape, depth);
			var watch   = Stopwatch.StartNew();

			var parsed  = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
			var elapsed = watch.Elapsed;

			// A grammar that reads: one recovered from would be timed on the recovery.
			Assert.True(
				parsed.Diagnostics.Count == 0,
				$"{shape} at {depth} levels: {string.Join("; ", parsed.Diagnostics)}");

			// Measured: under two milliseconds at forty once kept; doubling, past a second at twenty-one.
			Assert.True(
				elapsed.TotalSeconds < 1,
				$"{shape} at {depth} levels took {elapsed.TotalMilliseconds:F0} ms.");
		}
	}

	/// <summary>The look leaves nothing behind: a rule lifted out of a rebinding it read is not declared twice.</summary>
	[Fact]
	public void Looking_at_a_when_lifts_nothing()
	{
		var parsed = GramCompiler.Read(
			"P(x) = x\nB = 'b'\nX = 'x'\nA = when P(X with (X = (B | 'y'))) is 'x' & 'z'\nparse A\n", RoslynCSharpScanner.Instance);

		Assert.Single(parsed.File.Decls.OfType<Decl.Rule>(), static rule => rule.Replaces is not null);
	}
}
