extern alias plain;
extern alias traced;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Tests;

using Xunit;

using PlainSql  = plain::DotGram.Sql.TransactSql.TransactSqlParser;
using TracedSql = traced::DotGram.Sql.TransactSql.TransactSqlParser;

namespace DotGram.Trace.Tests;

/// <summary>
/// What a trace build costs where it is used: memory on the deepest refusal there is, and time on
/// the largest grammar there is. Bounds an order of magnitude past what was measured, so that a
/// noisy machine does not fail them and a change of the kind that matters does.
/// </summary>
[Collection(typeof(Alone))]
public sealed class TraceCostTests(ITestOutputHelper output)
{
	/// <summary>The collection xunit runs after every parallel one, with nothing beside it: what is timed is timed alone.</summary>
	[CollectionDefinition(DisableParallelization = true)]
	public sealed class Alone;

	/// <summary>
	/// GramWhy over a JSON text nested fifty thousand deep and refused at its end stays under 256 MB,
	/// the peak working set of a process that does nothing else: a refusal costs it one entry, however
	/// many rules are open, where a copy of the stack at each would be the depth squared.
	/// </summary>
	[Fact]
	public void A_deep_refusal_is_explained_in_bounded_memory()
	{
		var here = AppContext.BaseDirectory;
		var deep = Path.GetFullPath(Path.Combine(here, "..", "..", "..", "..", "DotGram.Trace.Deep",
			Path.GetRelativePath(Path.GetFullPath(Path.Combine(here, "..", "..", "..")), here), "DotGram.Trace.Deep.dll"));

		Assert.True(File.Exists(deep), deep);

		var start = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
		};

		start.ArgumentList.Add(deep);
		start.ArgumentList.Add("50000");

		using var process = Process.Start(start)!;

		var said  = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();

		process.WaitForExit();

		output.WriteLine(said);
		output.WriteLine(error);

		var lines = said.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(static line => line.Split('=', 2))
			.ToDictionary(static pair => pair[0], static pair => pair[1]);

		Assert.Equal(0, process.ExitCode);
		Assert.Equal("True", lines["refused"]);
		Assert.Equal("True", lines["same"]);
		Assert.True(int.Parse(lines["paths"]) > 0);
		Assert.True(int.Parse(lines["deepest"]) >= 50_000, lines["deepest"]);
		Assert.True(long.Parse(lines["peak"]) <= 256L * 1024 * 1024, lines["peak"]);
	}

	/// <summary>
	/// The T-SQL corpus read with <c>GramProfile</c> attached, a timestamp at every rule's entry and
	/// exit, takes at most thirty times what the library takes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>What it costs.</b> Twelve times, give or take one, once both builds are compiled as the JIT
	/// finally compiles them; twenty-three where nothing is recompiled with what the run has seen
	/// (<c>DOTNET_TieredCompilation=0</c>). Each is the least of alternating rounds on an otherwise idle
	/// machine. A tenfold regression of the profile is over a hundred, and a doubling of its cost is
	/// most of the way to the bound.
	/// </para>
	/// <para>
	/// <b>Why it was held to ten, and failed.</b> Timed as a median of five after one reading of each,
	/// the library was still being compiled up from its first, unoptimized code: 80 to 95 ms a corpus
	/// where it settles at 20, and the ratio read three. Wherever the library had been warmed first —
	/// the rest of this assembly reads the same corpus — it read twelve, over the bound, and whether a
	/// run failed depended on what ran before it.
	/// </para>
	/// <para>
	/// <b>How it is timed.</b> Both read in turn for <see cref="WarmSeconds"/> first, so that what is
	/// compared is the code the JIT keeps; then as the scaling classes time a ratio
	/// (<c>ScalingClock</c>): alternating rounds, the least of each, and a ratio over the bound measured
	/// again before it is believed, so that other test assemblies running beside this one — which
	/// <see cref="Alone"/> does not hold off — cannot decide it.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_profile_of_the_largest_grammar_costs_less_than_thirty_times_the_reading()
	{
		var target = Targets.Named("T-SQL");
		var inputs = target.Seeds.Concat(target.Refused).ToList();

		void Plain()
		{
			foreach (var text in inputs)
				PlainSql.TryParseSql(text);
		}

		void Profiled()
		{
			var profile = new TracedSql.GramProfile();

			using (TracedSql.Tracing(profile))
				foreach (var text in inputs)
					TracedSql.TryParseSql(text);
		}

		for (var warm = Stopwatch.StartNew(); warm.Elapsed.TotalSeconds < WarmSeconds;)
		{
			Plain();
			Profiled();
		}

		var reading = ScalingClock.Settle(Plain, Profiled, bound: 30);

		output.WriteLine(
			$"{inputs.Count} inputs: {reading.Shorter / 1000:F1} ms as shipped, {reading.Longer / 1000:F1} ms profiled, " +
			$"{reading.Ratio:F2}x (attempts {reading.Readings})");

		Assert.True(reading.Within, $"{reading.Ratio:F2}x over {reading.Readings}");
	}

	/// <summary>How long both readings are run before either is timed, so that the JIT has recompiled what it will.</summary>
	const double WarmSeconds = 5;

	/// <summary>A refused T-SQL script of a hundred kilobytes is explained in under a second.</summary>
	[Fact]
	public void A_large_refused_script_is_explained_in_under_a_second()
	{
		var target = Targets.Named("T-SQL");
		var script = new StringBuilder();

		foreach (var seed in target.Seeds)
		{
			if (script.Length >= 100_000)
				break;

			script.Append(seed).Append(";\n");
		}

		script.Append("SELECT * FROM t WHERE (;\n");

		var text = script.ToString();

		Assert.False(PlainSql.TryParseSql(text).IsSuccess);

		var best = double.MaxValue;
		Explained? why = null;

		for (var i = 0; i < 3; i++)
		{
			var clock = Stopwatch.StartNew();

			why  = target.Traced(text);
			best = Math.Min(best, clock.Elapsed.TotalMilliseconds);
		}

		output.WriteLine($"{text.Length} characters: explained in {best:F1} ms, {why!.Stacks.Length} paths: {why.Message}");

		Assert.Equal(why.Answer.Error, why.Message);
		Assert.NotEmpty(why.Stacks);
		Assert.True(best < 1000, $"{best:F1} ms");
	}
}
