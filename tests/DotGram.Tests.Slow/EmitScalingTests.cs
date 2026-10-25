using System;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Writing the parser of a grammar of many rules costs time in proportion to the rules, not the
/// square of them.
/// </summary>
/// <remarks>
/// <para>
/// After the machines are written, the walks a literal the input ends inside calls are dropped where
/// nothing calls them, and a walk only a dropped one called goes with it. That was done by finding
/// one, removing it from the text, and starting again from the top of the text: for a file with
/// several hundred such walks, a read of all of it and a copy of all of it for each, so that a grammar
/// of six hundred rules read as a buffer took twenty minutes to write.
/// </para>
/// <para>
/// The grammar is a number of copies of one shape that has such walks, read as a buffer (the shape
/// the nested-look sweep is made of). Four times the rules take four to seven times as long as they
/// do, and sixteen times as long or more when some step is the square of them; the two sizes are timed
/// in one run (<see cref="ScalingClock"/>) against the square of four, and the text is made before
/// the clock.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class EmitScalingTests
{
	const int Small = 20;

	const int Large = 80;

	/// <summary>
	/// The square of four. The write is a little worse than linear on its own (about seven times for four
	/// times the rules, measured), and the defect this holds out read thirty-seven times for four.
	/// </summary>
	const double Limit = 16;

	[Fact]
	public void Four_times_the_rules_do_not_cost_sixteen_times_as_long_to_write()
	{
		var few  = Grammar(Small);
		var many = Grammar(Large);

		Assert.DoesNotContain(Emit(few).Diagnostics, static one => one.Severity == GramSeverity.Error);

		var reading = ScalingClock.Settle(() => Emit(few), () => Emit(many), Limit);

		TestContext.Current.SendDiagnosticMessage($"{Large} copies took {reading.Ratio:F1} times as long to write as {Small} ({reading.Shorter / 1000:F0} ms against {reading.Longer / 1000:F0} ms).");

		Assert.True(
			reading.Within,
			$"{Large} copies took {reading.Ratio:F1} times as long to write as {Small} " +
			$"({reading.Shorter / 1000:F0} ms against {reading.Longer / 1000:F0} ms), on every one of " +
			$"{ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	static GramCompilation Emit(string grammar)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			BufferedInput = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	static string Grammar(int copies)
	{
		var text = new StringBuilder();

		for (var n = 0; n < copies; n++)
		{
			text.Append($"S{n} = ?!(('a' | \"ab\") & ?!'b') & (W{n} & 'c')? & ['a'..'c']*\n");
			text.Append($"W{n} = ['a'..'b']+\n");
			text.Append($"D{n} = (L{n} & ['a'..'c']+ | '.')? & '.'\n");
			text.Append($"L{n} = ?!(('a' | \"ab\") & ?!'b')\n");
			text.Append($"parse S{n} stream bytes\n");
			text.Append($"parse D{n} stream bytes\n");
		}

		return text.ToString();
	}
}
