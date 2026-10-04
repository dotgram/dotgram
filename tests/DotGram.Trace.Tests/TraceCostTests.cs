extern alias plain;
extern alias traced;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
	/// exit, takes at most ten times what the library takes.
	/// </summary>
	[Fact]
	public void A_profile_of_the_largest_grammar_costs_less_than_ten_times_the_reading()
	{
		var target = Targets.Named("T-SQL");
		var inputs = target.Seeds.Concat(target.Refused).ToList();

		double Plain()
		{
			var clock = Stopwatch.StartNew();

			foreach (var text in inputs)
				PlainSql.TryParseSql(text);

			return clock.Elapsed.TotalMilliseconds;
		}

		double Profiled()
		{
			var profile = new TracedSql.GramProfile();
			var clock   = Stopwatch.StartNew();

			using (TracedSql.Tracing(profile))
				foreach (var text in inputs)
					TracedSql.TryParseSql(text);

			return clock.Elapsed.TotalMilliseconds;
		}

		Plain();
		Profiled();

		var plainTimes    = new List<double>();
		var profiledTimes = new List<double>();

		for (var i = 0; i < 5; i++)
		{
			plainTimes.Add(Plain());
			profiledTimes.Add(Profiled());
		}

		var ratio = Median(profiledTimes) / Median(plainTimes);

		output.WriteLine($"{inputs.Count} inputs: {Median(plainTimes):F1} ms as shipped, {Median(profiledTimes):F1} ms profiled, {ratio:F2}x");

		Assert.True(ratio <= 10, $"{ratio:F2}x");
	}

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

	static double Median(List<double> values)
	{
		var sorted = values.OrderBy(static one => one).ToList();

		return sorted[sorted.Count / 2];
	}
}
