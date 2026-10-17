using System;
using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace DotGram.Finance.FixShape;

// FixShape --window <W1|W1a|W1b|W2|W3|W4|dry|dry8> [BenchmarkDotNet arguments]: one BenchmarkDotNet run of a window,
// every column a job (A1 and A2 are the same build, the A/A). FixShape --print: the checksum of every row
// for the column this build was made against, and the findings of the validate rows.
static class Program
{
	static readonly string[] Fix44All =
	[
		"P1_ParseOrder", "P2_ParseSnapshot", "V1_ParseValidateOrder", "V2_ParseValidateSnapshot",
		"I1_GroupsForeach", "I2_GroupsFor", "I3_NestedForeach", "I4_FieldsForeach", "I5_LinqCount",
		"I6_MultipleForeach", "I7_AbsentForeach", "I8_LinqCountSystemLinq", "E1_ParseIterateSnapshot", "E2_ParseIterateOrder",
	];

	static readonly string[] Others = ["P2_ParseSnapshot", "E1_ParseIterateSnapshot", "E2_ParseIterateOrder"];

	static int Main(string[] args)
	{
		if (args.Length == 1 && args[0] == "--print")
		{
			Environment.SetEnvironmentVariable("FIXH1_PRINT", "1");

			var bench = new FixCollectionShapeBenchmarks();

			bench.Setup();

			foreach (var (row, _) in FixCollectionShapeBenchmarks.Expected)
				Console.WriteLine($"{row} {bench.Run(row)} {bench.Report(row)}");

			return 0;
		}

		if (args.Length == 1 && args[0] == "--probe")
			return FixCollectionShapeBenchmarks.Probe();

		var at = Array.IndexOf(args, "--window");

		if (at < 0 || at + 1 >= args.Length)
		{
			Console.Error.WriteLine("usage: FixShape --window <W1|W1a|W1b|W2|W3|W4|dry|dry8> [BenchmarkDotNet arguments] | --print");
			return 2;
		}

		var window  = args[at + 1];
		var rest    = args.Where((_, i) => i != at && i != at + 1).ToArray();
		var columns = (Environment.GetEnvironmentVariable("FIXH1_COLUMNS") ?? "A1,A2,P,Q").Split(',');

		string[] rows = window switch
		{
			"dry"          => [.. Fix44All.Select(r => "Fix44_" + r), .. Others.Select(r => "Fix42_" + r), .. Others.Select(r => "Fix50_" + r)],
			"W1"           => [.. Fix44All.Select(r => "Fix44_" + r)],
			"W1a"          => [.. Fix44All.Where(r => r[0] is 'P' or 'V' or 'E').Select(r => "Fix44_" + r)],
			"W1b"          => [.. Fix44All.Where(r => r[0] == 'I').Select(r => "Fix44_" + r)],
			"W2"           => [.. Others.Select(r => "Fix42_" + r), .. Others.Select(r => "Fix50_" + r)],
			"W3" or "dry8" => [.. Fix44All.Select(r => "Fix44_" + r)],
			"W4"           => [.. Fix44All.Where(r => r[0] == 'I' && r[1] != '6').Select(r => "Fix44_" + r)],
			_              => throw new ArgumentException("No window " + window),
		};

		// A row list of the caller's own, to re-run the rows of a window whose A/A was too wide.
		if (Environment.GetEnvironmentVariable("FIXH1_ROWS") is { Length: > 0 } only)
			rows = [.. rows.Where(r => only.Split(',').Any(o => r.EndsWith(o, StringComparison.Ordinal)))];

		var runtime = window is "W3" or "dry8" ? CoreRuntime.Core80 : CoreRuntime.Core10_0;
		var config  = DefaultConfig.Instance
			.AddExporter(JsonExporter.Full)
			.AddFilter(new NameFilter(name => rows.Contains(name)))
			.WithOption(ConfigOptions.JoinSummary, true);

		foreach (var column in columns)
		{
			var job = (window.StartsWith("dry", StringComparison.Ordinal) ? Job.Dry : Job.Default.WithLaunchCount(3).WithWarmupCount(5).WithIterationCount(15))
				.WithRuntime(runtime)
				.WithArguments([new MsBuildArgument("/p:FixShape=" + column[..1])])
				.WithEnvironmentVariable("FIXH1_COLUMN", column)
				.WithId(column);

			if (window == "W4")
				job = job.WithEnvironmentVariable("DOTNET_TieredPGO", "0");

			config = config.AddJob(job);
		}

		BenchmarkRunner.Run<FixCollectionShapeBenchmarks>(config, rest);

		return 0;
	}
}
