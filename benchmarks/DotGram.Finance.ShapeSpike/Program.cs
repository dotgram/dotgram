// SPIKE (not for merging): the launcher. One BenchmarkDotNet invocation runs every column of a window as a
// job of its own; a column's job builds the benchmark with /p:FixShape=<column>, which picks that column's
// DotGram.Finance by HintPath and the symbol the rows are written against.
//
//   --columns A1,A2,B,C,F   the jobs (A1 and A2 are one build, run twice: the A/A that gives the noise; D is a column too)
//   --runtime net10.0|net8.0
//   --pgo 0                  runs the process under DOTNET_TieredPGO=0
//   --checksums              prints the constants of Expected.cs (this build's column answers them)
//   --job dry                one iteration of every case, to see that every row and column runs
//   --columns-dir <path>     where the column DLLs are (default /ramdisk/build/dotgram/fix-h1-spike/columns)
//
// Everything else is BenchmarkDotNet's own (--filter, --anyCategories, --artifacts, ...).

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

static class Program
{
	static string? Take(List<string> args, string name)
	{
		var at = args.IndexOf(name);

		if (at < 0)
			return null;

		var value = args[at + 1];

		args.RemoveRange(at, 2);

		return value;
	}

	static int Main(string[] arguments)
	{
		if (arguments.Length == 1 && arguments[0] == "--checksums")
		{
			// What every row answers once, as the source of Expected.cs wants it.
			Environment.SetEnvironmentVariable("FIX_SPIKE_PRINT", "1");
			new FixCollectionShapeBenchmarks().Setup();

			return 0;
		}

		var args    = arguments.ToList();
		var dry     = Take(args, "--job") == "dry";
		var columns = (Take(args, "--columns") ?? "A1,A2,B,C,F").Split(',');
		var runtime = Take(args, "--runtime") ?? "net10.0";
		var pgo     = Take(args, "--pgo");
		var dir     = Take(args, "--columns-dir") ?? "/ramdisk/build/dotgram/fix-h1-spike/columns";

		if (!args.Contains("--filter") && !args.Contains("--anyCategories") && !args.Contains("--allCategories"))
			args.AddRange(["--filter", "*"]);

		var config = ManualConfig.Create(DefaultConfig.Instance);

		config.AddExporter(JsonExporter.Full);

		foreach (var column in columns)
		{
			var job = (dry ? Job.Dry : Job.Default.WithLaunchCount(3).WithWarmupCount(5).WithIterationCount(15))
				.WithRuntime(runtime == "net8.0" ? CoreRuntime.Core80 : CoreRuntime.Core10_0)
				.WithArguments([new MsBuildArgument("/p:FixShape=" + column[0]), new MsBuildArgument("/p:ColumnsDir=" + dir)])
				.WithEnvironmentVariable("FIX_SPIKE_COLUMN", column)
				.WithId(column);

			if (pgo is not null)
				job = job.WithEnvironmentVariable("DOTNET_TieredPGO", pgo);

			config.AddJob(job);
		}

		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args.ToArray(), config);

		return 0;
	}
}
