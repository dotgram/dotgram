using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refusal after a long accepted run costs what the run is long, not two to its power.
/// </summary>
/// <remarks>
/// <para>
/// Both of these were real, both were found on one day, and neither was visible in any test that
/// asked only what a reader answers. `(A+)*` — a repeated rule whose own body repeats — can cut a
/// run of n characters into any of 2^(n-1) sequences, all of them meaning the same thing; a parse
/// that fails after the run then tries every one of them before saying so.
/// </para>
/// <para>
/// `UriTemplate` was `Part*` over `Literals = (LiteralChar | PctEncoded)+`: a refused template
/// with twenty-four literal characters before the brace took 3.2 seconds, and each further
/// character doubled it. `EmailAddress.ParseList` was `ObsPhrase = WordText & (WordText | …)*`
/// over `AtomText = Atext+`: sixteen characters before a bad `@` took 78 ms and twenty-eight took
/// **107 seconds**. And `ObsRoute` repeats over `Cfws`, which repeats over
/// `Fws = (Crlf? & Wsp)+`, so a run of folding whitespace inside angle brackets was the same
/// shape a third time: twenty spaces, 189 ms. And `Cfws` repeats over comments as well as over
/// folding, so sealing the folding closed one of the two ways in and left the other: twenty
/// comments in the same route, 311 ms. All four runs are atomic now.
/// </para>
/// <para>
/// <strong>The budget, and why it is a budget rather than a ratio.</strong> The first version of
/// this test timed both lengths and compared them, and against the unfixed grammars it did not
/// fail — it ran for twenty minutes and was still running, because eleven reads of a hundred-second
/// refusal is what it had asked for. A guard that takes twenty minutes to say "this is exponential"
/// reports nothing in a build; it times the build out instead. So the long read is given a budget
/// and abandoned when it passes it. Failing takes a second and a half however bad the grammar is.
/// </para>
/// <para>
/// The abandoned read goes on running until the process ends. That is the price of the fast
/// failure, it is paid only on a build that is already broken, and this project is the one where
/// a spare core for a minute is affordable.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class WebRefusalScalingTests
{
	/// <summary>Twelve more characters is four thousand times the work if the run can be re-cut.</summary>
	static readonly TimeSpan Budget = TimeSpan.FromMilliseconds(1_500);

	[Theory]
	[InlineData("a URI template whose brace never closes")]
	[InlineData("an address list whose domain holds a space")]
	[InlineData("an obsolete route made of folding whitespace")]
	[InlineData("an obsolete route made of comments")]
	public async Task A_refusal_after_a_long_run_does_not_double_with_its_length(string shape)
	{
		var refuse = Reader(shape);

		// Short first: it compiles the path and it is quick even when the grammar is wrong.
		var shorter = Best(refuse, 16);

		// One read, abandoned if it outstays the budget. This is the assertion that catches the
		// catastrophe, and it catches it in a second and a half however bad the grammar is.
		var read  = Task.Run(() => refuse(28), TestContext.Current.CancellationToken);
		var spent = Task.Delay(Budget, TestContext.Current.CancellationToken);

		Assert.True(
			await Task.WhenAny(read, spent) == read,
			$"{shape}: a refusal of twenty-eight characters had not finished in {Budget.TotalSeconds:F1} s, " +
			$"where sixteen took {shorter:F0} µs. That is the shape of `(A+)*` — the run can be cut many " +
			$"ways and a refusal tries them all. Make the run atomic: `{{ X+ }}`.");

		// It finished inside the budget, so the catastrophe is gone. Now the milder regression:
		// one that would not blow the budget at twenty-eight characters but would at forty. Timed
		// the SAME way as the short one — the budgeted read above went through a task, and a task
		// costs a thousand microseconds to start, which is a thousand times the work being timed.
		var longer = Best(refuse, 28);

		Assert.True(
			longer / Math.Max(shorter, 1) < 50,
			$"{shape}: twelve more characters took {longer / Math.Max(shorter, 1):F0} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs).");
	}

	static Func<int, bool> Reader(string shape)
	{
		return shape[..8] switch
		{
			"a URI te" => length => UriTemplate.TryParse(new string('a', length) + "{unclosed", out _),
			"an addre" => length => EmailAddress.TryParseList(new string('a', length) + "@ex ample.com", out _),
			"an obsol" when shape.EndsWith("whitespace", StringComparison.Ordinal)
					   => length => EmailAddress.TryParseList("<" + new string(' ', length) + "@a:b@c.d", out _),
			_ => length => EmailAddress.TryParseList(
							  "<" + string.Concat(Enumerable.Repeat("(a)", length)) + "@a:b@c.d", out _),
		};
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<int, bool> refuse, int length)
	{
		Assert.False(refuse(length));

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var best = double.MaxValue;

		for (var run = 0; run < 11; run++)
		{
			var watch = Stopwatch.StartNew();

			refuse(length);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}
}
