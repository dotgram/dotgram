extern alias plain;
extern alias traced;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Tests;

using Xunit;

using PlainAccept  = plain::DotGram.Web.Rfc9110;
using PlainSql     = plain::DotGram.Sql.TransactSql.TransactSqlParser;
using TracedAccept = traced::DotGram.Web.Rfc9110;
using TracedSql    = traced::DotGram.Sql.TransactSql.TransactSqlParser;

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
	/// A corpus read with <c>GramProfile</c> attached, a timestamp at every rule's entry and exit,
	/// takes at most ten times what the library takes: T-SQL's, the largest grammar there is, read
	/// by methods, and an Accept field's, read by the engine, whose every call and every way back is
	/// told — a small grammar's corpus read over and over, so that a round is long enough to time.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>What it costs.</b> About three times on T-SQL, once both builds are compiled as the JIT
	/// finally compiles them, the least of alternating rounds on an otherwise idle machine; three on
	/// the Accept field. A tenfold regression of the profile is over thirty.
	/// </para>
	/// <para>
	/// <b>Why it once read twelve.</b> A refused call told every sink the match's message, and
	/// wording it costs a refused call more than reading it did; the profile reads no message, and
	/// says so (<c>HearsRejections</c>), so that its calls are not worded. Before that, a null sink
	/// cost ten times the reading and the profile two more.
	/// </para>
	/// <para>
	/// <b>Why it was read as three, and twelve.</b> Timed as a median of five after one reading of
	/// each, the library was still being compiled up from its first, unoptimized code: 80 to 95 ms a
	/// corpus where it settles at 20. Its largest methods are among the last the JIT recompiles, some
	/// six seconds into a run that reads both in turn, so both are read for <see cref="WarmSeconds"/>
	/// first; then as the scaling classes time a ratio (<c>ScalingClock</c>): alternating rounds, the
	/// least of each, and a ratio over the bound measured again before it is believed, so that other
	/// test assemblies running beside this one — which <see cref="Alone"/> does not hold off — cannot
	/// decide it. The Windows CI job also runs this assembly by itself, after the others have finished,
	/// for the same reason: a hosted runner has too few cores to time a ratio beside them.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("T-SQL", 1)]
	[InlineData("Accept", 400)]
	public void A_profile_costs_less_than_ten_times_the_reading(string name, int passes)
	{
		var target  = Targets.Named(name);
		var inputs  = Enumerable.Repeat(target.Seeds.Concat(target.Refused), passes).SelectMany(static one => one).ToList();
		var readers = Readers(name);

		void Plain()
		{
			foreach (var text in inputs)
				readers.Plain(text);
		}

		void Profiled()
		{
			using (readers.Profiling())
				foreach (var text in inputs)
					readers.Traced(text);
		}

		for (var warm = Stopwatch.StartNew(); warm.Elapsed.TotalSeconds < WarmSeconds;)
		{
			Plain();
			Profiled();
		}

		var reading = ScalingClock.Settle(Plain, Profiled, bound: 10);

		output.WriteLine(
			$"{name}, {inputs.Count} inputs: {reading.Shorter / 1000:F1} ms as shipped, {reading.Longer / 1000:F1} ms profiled, " +
			$"{reading.Ratio:F2}x (attempts {reading.Readings})");

		Assert.True(reading.Within, $"{reading.Ratio:F2}x over {reading.Readings}");
	}

	/// <summary>How long both readings are run before either is timed, so that the JIT has recompiled what it will.</summary>
	const double WarmSeconds = 10;

	/// <summary>A target read as shipped, a scope with a fresh profile over its trace build, and the trace build read.</summary>
	static (Action<string> Plain, Func<IDisposable> Profiling, Action<string> Traced) Readers(string name)
	{
		return name switch
		{
			"T-SQL" => (
				static text => PlainSql.TryParseSql(text),
				static () => TracedSql.Tracing(new TracedSql.GramProfile()),
				static text => TracedSql.TryParseSql(text)),
			"Accept" => (
				static text => PlainAccept.TryParseAccept(text),
				static () => TracedAccept.Tracing(new TracedAccept.GramProfile()),
				static text => TracedAccept.TryParseAccept(text)),
			_ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
		};
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
}
