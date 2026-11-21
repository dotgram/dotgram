using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>What a printed parser said of one input: a head line, and the host calls it made.</summary>
/// <param name="Head">
/// <c>A</c>, the end and what was built; <c>R</c>, the furthest position and the numbers of what
/// was expected there; or <c>T</c> and why, where the reading threw or ran out of stack.
/// </param>
sealed record SliceAnswer(string Head, List<string> Log)
{
	/// <summary>
	/// What a parser printed to count said it did, where it was asked: steps (functions entered,
	/// turns begun), decisions opened or read back, readings asked for again, turns gone straight
	/// to, host calls.
	/// </summary>
	public long[]? Counts { get; init; }

	/// <summary>
	/// A reading restarted by budget: in the evaluator's answer, how many host calls the reading given
	/// up had made when it was given up; in a printed parser's, how many it made after (the poison's
	/// forward work, which the evaluator does not model). Null where nothing restarted.
	/// </summary>
	public long? Restart { get; init; }

	/// <summary>
	/// Whether a printed parser's host calls are the evaluator's: the same, or where both restarted,
	/// the evaluator's calls up to the restart, the printed parser's calls after the trip, then the
	/// evaluator's resuming calls.
	/// </summary>
	public static bool SameCalls(SliceAnswer expected, SliceAnswer actual)
	{
		if (expected.Restart is not { } at)
			return actual.Restart is null && expected.Log.SequenceEqual(actual.Log);

		if (actual.Restart is not { } after)
			return false;

		var k = (int)at;
		var m = (int)after;

		return actual.Log.Count == expected.Log.Count + m &&
			actual.Log.Take(k).SequenceEqual(expected.Log.Take(k)) &&
			actual.Log.Skip(k + m).SequenceEqual(expected.Log.Skip(k));
	}
}

/// <summary>
/// A program printed in another language, built there and run over a batch of inputs.
/// </summary>
/// <remarks>
/// The tools are the machine's: a node that runs TypeScript by stripping its types
/// (<see cref="Node"/>), and gcc. A test that needs one that is absent is skipped.
/// </remarks>
static class Slices
{
	static readonly Lazy<(string[]? Arguments, IReadOnlyDictionary<string, string> Environment)> _node = new(FindNode);

	static readonly Lazy<string?> _gcc = new(() => Probe("gcc", ["--version"]) is null ? null : "gcc");

	/// <summary>
	/// The arguments that make node run a TypeScript file, or a skip where it cannot.
	/// </summary>
	/// <remarks>
	/// Node strips types itself from 22.6 on — unless it was built without that, as a
	/// distribution's package may be. Then the types are stripped by the amaro package, through a
	/// hook among the hand-written files: at <c>DOTGRAM_IR_AMARO</c> (its <c>dist/index.js</c>),
	/// or where <c>npm install --prefix ~/.cache/dotgram-ir-prototype amaro</c> puts it.
	/// </remarks>
	public static (string[] Arguments, IReadOnlyDictionary<string, string> Environment) Node =>
		_node.Value.Arguments is { } arguments
			? (arguments, _node.Value.Environment)
			: ([Skipped("node cannot run TypeScript here: it is absent, older than 22.6, or built without type stripping and the amaro package is not installed")], _node.Value.Environment);

	public static string Gcc => _gcc.Value ?? Skipped("gcc is not installed");

	static (string[]? Arguments, IReadOnlyDictionary<string, string> Environment) FindNode()
	{
		var none = new Dictionary<string, string>();

		if (Probe("node", ["--version"]) is not { } version ||
			!Version.TryParse(version.Trim().TrimStart('v'), out var parsed) || parsed < new Version(22, 6))
			return (null, none);

		var probe = Path.Combine(Path.GetTempPath(), "dotgram-ir-probe-" + Guid.NewGuid().ToString("N")[..8] + ".ts");

		File.WriteAllText(probe, "const one: number = 1; console.log(one);\n");

		try
		{
			if (Probe("node", ["--experimental-strip-types", "--no-warnings", probe]) is not null)
				return (["--experimental-strip-types", "--no-warnings"], none);

			var amaro = Environment.GetEnvironmentVariable("DOTGRAM_IR_AMARO") ?? Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "dotgram-ir-prototype", "node_modules", "amaro", "dist", "index.js");

			var hooked    = new Dictionary<string, string> { ["DOTGRAM_IR_AMARO"] = amaro };
			var arguments = new[] { "--no-warnings", "--import", new Uri(Path.Combine(Hosts("ts"), "register.mjs")).AbsoluteUri };

			return File.Exists(amaro) && Probe("node", [.. arguments, probe], hooked) is not null ? (arguments, hooked) : (null, none);
		}
		finally
		{
			File.Delete(probe);
		}
	}

	static string Skipped(string why)
	{
		Assert.Skip(why);

		return "";
	}

	static string? Probe(string tool, string[] arguments, IReadOnlyDictionary<string, string>? environment = null)
	{
		try
		{
			var (exit, output, _) = Run(tool, arguments, Path.GetTempPath(), environment);

			return exit == 0 ? output : null;
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return null;
		}
	}

	public static (int Exit, string Output, string Error) Run(
		string tool, IEnumerable<string> arguments, string directory, IReadOnlyDictionary<string, string>? environment = null)
	{
		var start = new ProcessStartInfo(tool)
		{
			WorkingDirectory       = directory,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			StandardOutputEncoding = Encoding.UTF8,
			UseShellExecute        = false,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		if (environment is not null)
			foreach (var (name, value) in environment)
				start.Environment[name] = value;

		using var process = Process.Start(start)!;

		var error  = process.StandardError.ReadToEndAsync();
		var output = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		return (process.ExitCode, output, error.Result);
	}

	/// <summary>The hand-written files of a language, which the build copies beside the tests.</summary>
	public static string Hosts(string language)
	{
		return Path.Combine(AppContext.BaseDirectory, "Hosts", language);
	}

	/// <summary>Where the printed parsers are left to be read, if anywhere: <c>IR_KEEP</c> names a directory.</summary>
	static readonly string? Kept = Environment.GetEnvironmentVariable("IR_KEEP");

	static void Discard(string directory)
	{
		if (Kept is null)
			Directory.Delete(directory, recursive: true);
	}

	/// <summary>A directory of its own for one printed parser, with the language's hand-written files in it.</summary>
	public static string Workspace(string name, string language)
	{
		var directory = Kept is null
			? Path.Combine(Path.GetTempPath(), "dotgram-ir-slices", name + "-" + language + "-" + Guid.NewGuid().ToString("N")[..8])
			: Path.Combine(Kept, name + "-" + language);

		if (Directory.Exists(directory))
			Directory.Delete(directory, recursive: true);

		Directory.CreateDirectory(directory);

		foreach (var file in Directory.GetFiles(Hosts(language)))
			File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));

		return directory;
	}

	/// <summary>The inputs as the drivers read them: a count, then each input's length and its UTF-16 code units.</summary>
	public static void WriteBatch(string path, IReadOnlyList<string> inputs)
	{
		using var writer = new BinaryWriter(File.Create(path));

		writer.Write(inputs.Count);

		foreach (var input in inputs)
		{
			writer.Write(input.Length);

			foreach (var unit in input)
				writer.Write((ushort)unit);
		}
	}

	public static List<SliceAnswer> Answers(string output)
	{
		var answers = new List<SliceAnswer>();

		string?      head   = null;
		List<string> log    = [];
		long[]?      counts = null;
		long?        after  = null;

		foreach (var line in output.Split('\n'))
		{
			if (line == ".")
			{
				answers.Add(new SliceAnswer(head ?? "", log) { Counts = counts, Restart = after });
				head   = null;
				log    = [];
				counts = null;
				after  = null;
			}
			else if (line.StartsWith("C ", StringComparison.Ordinal))
				after = long.Parse(line[2..], CultureInfo.InvariantCulture);
			else if (line.StartsWith("L ", StringComparison.Ordinal))
				log.Add(line[2..]);
			else if (line.StartsWith("K ", StringComparison.Ordinal))
				counts = [.. line[2..].Split(' ').Select(static one => long.Parse(one, CultureInfo.InvariantCulture))];
			else if (line.Length > 0)
				head = line;
		}

		return answers;
	}

	/// <summary>The program printed in TypeScript and run by node over the inputs.</summary>
	/// <param name="host">The host module among the hand-written files.</param>
	/// <param name="guardAnswers">What each guard answers, a line a call, for the recording host.</param>
	/// <param name="stackKilobytes">The stack V8 may use, where not its default (about a megabyte).</param>
	/// <param name="systemStackKilobytes">The stack the process is given, where not the system's default.</param>
	public static List<SliceAnswer> TypeScript(
		string name, IrProgram program, string entry, IReadOnlyList<string> inputs,
		string host = "recording.ts", IEnumerable<string>? guardAnswers = null, string? throwOn = null,
		int? stackKilobytes = null, int? systemStackKilobytes = null, SliceFrames frames = SliceFrames.Recursive, bool counted = false, bool anyOwner = false,
		bool noCutCheck = false)
	{
		var (node, environment) = Node;

		var directory = Workspace(name, "ts");

		if (counted || anyOwner || noCutCheck)
		{
			var more = new Dictionary<string, string>(environment);

			if (counted)
				more["IR_COUNTS"] = "1";

			if (anyOwner)
				more["IR_ANY_OWNER"] = "1";

			if (noCutCheck)
				more["IR_NO_CUT_CHECK"] = "1";

			environment = more;
		}

		try
		{
			File.WriteAllText(Path.Combine(directory, "parser.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, counted));
			File.WriteAllLines(Path.Combine(directory, "answers.txt"), guardAnswers ?? []);
			WriteBatch(Path.Combine(directory, "batch.bin"), inputs);

			List<string> arguments = [.. node];

			if (stackKilobytes is { } stack)
				arguments.Add("--stack-size=" + stack.ToString(CultureInfo.InvariantCulture));

			arguments.AddRange(["driver.ts", Path.Combine(directory, "parser.ts"), Path.Combine(directory, host), entry, "batch.bin", "answers.txt"]);

			if (throwOn is not null)
				arguments.Add(throwOn);

			// V8 is told how much stack it may use; how much there is, is the system's to say,
			// and is raised for the one process through the shell.
			var (exit, output, error) = systemStackKilobytes is { } system
				? Run("sh", ["-c", "ulimit -s " + system.ToString(CultureInfo.InvariantCulture) + " && exec \"$@\"", "sh", "node", .. arguments], directory, environment)
				: Run("node", arguments, directory, environment);

			Assert.True(exit == 0, "node: " + error);

			return Answers(output);
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>A script among the hand-written TypeScript files, run over the program printed beside it; what it wrote, by line.</summary>
	/// <remarks>The program is printed three times, a file a way of keeping frames: <c>parser.ts</c> without, <c>parser-recursive.ts</c>, <c>parser-all.ts</c>.</remarks>
	public static List<string> TypeScriptScript(string name, IrProgram program, string script, params string[] more)
	{
		var (node, environment) = Node;

		var directory = Workspace(name, "ts");

		try
		{
			File.WriteAllText(Path.Combine(directory, "parser.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript));
			File.WriteAllText(Path.Combine(directory, "parser-recursive.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, SliceFrames.Recursive));
			File.WriteAllText(Path.Combine(directory, "parser-all.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, SliceFrames.All));

			var (exit, output, error) = Run("node", [.. node, script, directory, .. more], directory, environment);

			Assert.True(exit == 0, "node: " + error);

			return [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries)];
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>
	/// The program printed in TypeScript with and without resumption, in every way of keeping
	/// frames, timed over accepted inputs by <c>accept.ts</c>: a line a way of keeping frames.
	/// </summary>
	public static List<string> AcceptingPath(string name, IrProgram program, string entry, IReadOnlyList<string> inputs, ResumptionCriterion criterion = ResumptionCriterion.FailsBack)
	{
		var (node, environment) = Node;

		var directory = Workspace(name, "ts");

		try
		{
			foreach (var frames in Enum.GetValues<SliceFrames>())
			{
				var form = frames.ToString().ToLowerInvariant();

				File.WriteAllText(Path.Combine(directory, $"parser-on-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, criterion: criterion));
				File.WriteAllText(Path.Combine(directory, $"parser-off-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, resumes: false));
				File.WriteAllText(Path.Combine(directory, $"parser-sentinels-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, criterion: criterion, ablation: SliceAblation.SentinelOnly));
				File.WriteAllText(Path.Combine(directory, $"parser-wrapper-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, criterion: criterion, ablation: SliceAblation.WrapperOnly));
				File.WriteAllText(Path.Combine(directory, $"parser-nodoors-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, criterion: criterion, ablation: SliceAblation.NoDoors));
				File.WriteAllText(Path.Combine(directory, $"parser-nocalls-{form}.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames, criterion: criterion, ablation: SliceAblation.NoCallSites));
			}

			WriteBatch(Path.Combine(directory, "batch.bin"), inputs);

			var (exit, output, error) = Run("node", [.. node, "accept.ts", directory, entry, "batch.bin"], directory, environment);

			Assert.True(exit == 0, "node: " + error);

			return [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries)];
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>
	/// The program printed in C with resumption (whole, sentinels alone, the code round them alone,
	/// without doors) and without, each compiled on its own with a host that builds nothing, run in
	/// turns over the batch by <c>accept.c</c>: a line a variant, the median of the ratios to the parser without.
	/// </summary>
	public static List<string> AcceptingPathInC(string name, IrProgram program, string entry, IReadOnlyList<string> inputs)
	{
		var gcc       = Gcc;
		var directory = Workspace(name, "c");

		try
		{
			WriteBatch(Path.Combine(directory, "batch.bin"), inputs);
			File.WriteAllText(Path.Combine(directory, "parser.h"), SlicePrinter.PrintHeaderInC(program));
			File.WriteAllText(Path.Combine(directory, "fragments.c"), SlicePrinter.PrintNullHostInC(program));

			// A parser that restarts by budget is two translation units: the first reading and the resuming set.
			var resuming = SlicePrinter.PrintResumingSet(program);

			File.WriteAllText(Path.Combine(directory, "resume.c"), resuming);

			var variants = new (string Name, string Source)[]
			{
				("off", SlicePrinter.Print(program, SliceLanguage.C, resumes: false)),
				("on", SlicePrinter.Print(program, SliceLanguage.C)),
				("sentinels", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.SentinelOnly)),
				("wrapper", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.WrapperOnly)),
				("nodoors", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.NoDoors)),
				("nocalls", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.NoCallSites)),
				("loops", SlicePrinter.Print(program, SliceLanguage.C, criterion: ResumptionCriterion.Repetitions)),
				("ceiling", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.Ceiling)),
				("copy", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.SecondCopy)),
				("twin", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ReplayTwin)),
				("twinbare", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ReplayTwinBare)),
				("twinevery", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ReplayTwinEvery)),
				("offevery", SlicePrinter.Print(program, SliceLanguage.C, resumes: false, ablation: SliceAblation.ReplayTwinEvery)),
				("ceilingevery", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.CeilingEvery)),
				("twinkeyed", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ReplayTwinKeyed)),
				("twinkeyedsplit", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ReplayTwinKeyedSplit)),
				("offhot", SlicePrinter.Print(program, SliceLanguage.C, resumes: false, hotRuntime: true)),
				("offeveryhot", SlicePrinter.Print(program, SliceLanguage.C, resumes: false, ablation: SliceAblation.ReplayTwinEvery, hotRuntime: true)),
				("resumingalone", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ResumingSetAlone)),
				("resumingalonehot", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.ResumingSetAlone, hotTwins: true)),
				("restart", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.RestartByBudget)),
				("restarthot", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.RestartByBudget, hotRuntime: true)),
				("restart4", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.RestartByBudget, budget: 4)),
				("restarthot4", SlicePrinter.Print(program, SliceLanguage.C, ablation: SliceAblation.RestartByBudget, hotRuntime: true, budget: 4)),
			};

			// IR_C_VARIANTS: the variants to time against "off", where not all of them.
			if (Environment.GetEnvironmentVariable("IR_C_VARIANTS") is { } chosen)
			{
				var names = chosen.Split(',');

				variants = [.. variants.Where(variant => variant.Name == "off" || names.Contains(variant.Name))];
			}

			foreach (var (variant, source) in variants)
			{
				File.WriteAllText(Path.Combine(directory, $"parser-{variant}.c"), source);

				// IR_C_ALWAYS_INLINE: every variant with the runtime's functions inlined wherever they are called.
				List<string> flags = Environment.GetEnvironmentVariable("IR_C_ALWAYS_INLINE") == "1" ? ["-DRT_ALWAYS_INLINE"] : [];

				List<string> more = variant.StartsWith("restart", StringComparison.Ordinal) ? ["resume.c"] : [];

				var (built, _, complaints) = Run(gcc, ["-std=gnu11", "-O2", "-D_GNU_SOURCE", .. flags, "-o", "accept-" + variant, "accept.c", $"parser-{variant}.c", .. more, "fragments.c"], directory);

				Assert.True(built == 0, "gcc: " + complaints);
			}

			(int Accepted, double Milliseconds, int Restarts) Read(string variant, long reps)
			{
				var (exit, output, error) = Run(Path.Combine(directory, "accept-" + variant), [entry, "batch.bin", reps.ToString(CultureInfo.InvariantCulture)], directory);

				Assert.True(exit == 0, "accept: " + error);

				var parts = output.Trim().Split(' ');

				return (int.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture), int.Parse(parts[2], CultureInfo.InvariantCulture));
			}

			// The size of the machine code of a variant's program, where size can say it.
			string Text(string variant)
			{
				try
				{
					var (exit, output, _) = Run("size", ["-A", Path.Combine(directory, "accept-" + variant)], directory);
					var line = output.Split('\n').FirstOrDefault(static one => one.StartsWith(".text", StringComparison.Ordinal));

					return exit == 0 && line is not null ? line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1] : "-";
				}
				catch (System.ComponentModel.Win32Exception)
				{
					return "-";
				}
			}

			// Instructions retired by a run of the batch, where perf can count them: deterministic, unlike time.
			string Instructions(string variant)
			{
				try
				{
					var (exit, _, error) = Run("perf", ["stat", "-x,", "-e", "instructions:u", Path.Combine(directory, "accept-" + variant), entry, "batch.bin", "200"], directory);

					var line = error.Split('\n').FirstOrDefault(static one => one.Contains("instructions", StringComparison.Ordinal));

					return exit == 0 && line is not null ? line.Split(',')[0] : "-";
				}
				catch (System.ComponentModel.Win32Exception)
				{
					return "-";
				}
			}

			// Enough repetitions of the batch for a reading of some fifty milliseconds.
			long repetitions = 1;

			while (Read("off", repetitions).Milliseconds < 50)
				repetitions *= 2;

			var rounds = int.Parse(Environment.GetEnvironmentVariable("IR_BENCH_ROUNDS") ?? "15", CultureInfo.InvariantCulture);
			var lines  = new List<string>();

			foreach (var (variant, _) in variants.Skip(1))
			{
				var ratios = new List<double>();
				var on     = new List<double>();
				var off    = new List<double>();
				var count  = 0;

				for (var round = 0; round < rounds; round++)
				{
					var first  = round % 2 == 0 ? variant : "off";
					var second = round % 2 == 0 ? "off" : variant;
					var a      = Read(first, repetitions);
					var b      = Read(second, repetitions);
					var (timeOn, timeOff) = round % 2 == 0 ? (a.Milliseconds, b.Milliseconds) : (b.Milliseconds, a.Milliseconds);

					count = round % 2 == 0 ? a.Accepted : b.Accepted;
					on.Add(timeOn / repetitions);
					off.Add(timeOff / repetitions);
					ratios.Add(timeOn / timeOff);
				}

				ratios.Sort();
				on.Sort();
				off.Sort();

				lines.Add(string.Create(CultureInfo.InvariantCulture,
					$"{variant}\t{count}/{inputs.Count}\t{on[on.Count / 2]:F4}\t{off[off.Count / 2]:F4}\t{ratios[ratios.Count / 2]:F3}\t{ratios[0]:F3}-{ratios[^1]:F3}\t{Instructions(variant)}\t{Instructions("off")}\t{Text(variant)}\t{Text("off")}\t{Read(variant, 1).Restarts}"));
			}

			return lines;
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>The program printed in C, compiled by gcc with a host and run over the inputs.</summary>
	/// <param name="host">The host's source among the hand-written files; the recording host where null.</param>
	/// <param name="stackMegabytes">The stack of a thread made for the reading, where not the process's own.</param>
	public static List<SliceAnswer> InC(
		string name, IrProgram program, string entry, IReadOnlyList<string> inputs,
		string? host = null, IEnumerable<string>? guardAnswers = null, int? stackMegabytes = null, bool counted = false, bool anyOwner = false,
		bool noCutCheck = false, SliceAblation? ablation = null, int? budget = null, bool noLookClause = false)
	{
		var gcc       = Gcc;
		var directory = Workspace(name, "c");

		try
		{
			var form = ablation ?? Ablation;

			File.WriteAllText(Path.Combine(directory, "parser.c"), SlicePrinter.Print(program, SliceLanguage.C, counted: counted, ablation: form, budget: budget ?? Budget, noLookClause: noLookClause));
			File.WriteAllText(Path.Combine(directory, "parser.h"), SlicePrinter.PrintHeaderInC(program));
			File.WriteAllLines(Path.Combine(directory, "answers.txt"), guardAnswers ?? []);
			WriteBatch(Path.Combine(directory, "batch.bin"), inputs);

			List<string> sources = ["driver.c", "parser.c"];

			if (form == SliceAblation.RestartByBudget)
			{
				File.WriteAllText(Path.Combine(directory, "resume.c"), SlicePrinter.PrintResumingSet(program, counted));
				sources.Add("resume.c");
			}

			if (host is null)
			{
				File.WriteAllText(Path.Combine(directory, "fragments.c"), SlicePrinter.PrintRecordingHostInC(program));
				sources.AddRange(["fragments.c", "recording.c"]);
			}
			else
				sources.Add(host);

			var (built, _, complaints) = Run(gcc, ["-std=gnu11", "-O2", "-D_GNU_SOURCE", "-o", "slice", .. sources, "-lpthread"], directory);

			Assert.True(built == 0, "gcc: " + complaints);

			var environment = new Dictionary<string, string>();

			if (stackMegabytes is { } stack)
				environment["IR_STACK_MB"] = stack.ToString(CultureInfo.InvariantCulture);

			if (counted)
				environment["IR_COUNTS"] = "1";

			if (anyOwner)
				environment["IR_ANY_OWNER"] = "1";

			if (noCutCheck)
				environment["IR_NO_CUT_CHECK"] = "1";

			var (exit, output, error) = Run(Path.Combine(directory, "slice"), [entry, "batch.bin", "answers.txt"], directory, environment);

			Assert.True(exit == 0, "the C parser: exit " + exit.ToString(CultureInfo.InvariantCulture) + " " + error);

			return Answers(output);
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>
	/// <c>IR_C_ABLATION</c>: the form the C parser is printed in where a test does not name one
	/// (SecondCopy, ReplayTwin, …), to check what it answers; <c>IR_C_BUDGET</c>, the budget of
	/// <see cref="SliceAblation.RestartByBudget"/> (8 where not given).
	/// </summary>
	public static SliceAblation Ablation =>
		Environment.GetEnvironmentVariable("IR_C_ABLATION") is { } named ? Enum.Parse<SliceAblation>(named) : SliceAblation.Whole;

	public static int Budget =>
		Environment.GetEnvironmentVariable("IR_C_BUDGET") is { } budget ? int.Parse(budget, CultureInfo.InvariantCulture) : 8;

	/// <summary>How many readings the evaluator gave up under restart by budget in <see cref="Evaluated"/>, since the process began.</summary>
	public static long EvaluatedRestarts;

	/// <summary>
	/// What the evaluator says of an input, in the words the drivers use. Where the C parser is
	/// printed to restart by budget, the host calls are those of the evaluator restarting by the same
	/// budget, and its answer is held to the evaluator's without restart, resuming and not.
	/// </summary>
	public static SliceAnswer Evaluated(IrProgram program, string entry, string input, Func<IrFragment, object?[], bool>? guards)
	{
		if (Ablation == SliceAblation.RestartByBudget)
			return EvaluatedRestarting(program, entry, input, guards, Budget);

		var host    = new RecordingHost(guards);
		var outcome = new IrEvaluator(program, host) { Resumes = true }.Parse(entry, input);

		var head = outcome.Accepted
			? outcome.Line
			: "R " + outcome.Position.ToString(CultureInfo.InvariantCulture) + " " +
				string.Join(",", outcome.Expected.Select(label => program.Expected.IndexOf(label)).Order().Select(static index => index.ToString(CultureInfo.InvariantCulture)));

		return new SliceAnswer(head, host.Log);
	}

	/// <summary>
	/// <see cref="Evaluated"/> restarting by budget: the answer, the value and the refusal checked
	/// against the evaluator without restart in both modes (an assertion), the host calls those of
	/// the reading that restarts.
	/// </summary>
	public static SliceAnswer EvaluatedRestarting(IrProgram program, string entry, string input, Func<IrFragment, object?[], bool>? guards, int budget, bool noLookClause = false)
	{
		long? restart = null;

		var host       = new RecordingHost(guards);
		var evaluator  = new IrEvaluator(program, host) { RestartBudget = budget, NoLookClause = noLookClause, Restarting = () => restart = host.Log.Count };
		var outcome    = evaluator.Parse(entry, input);
		var resuming   = new IrEvaluator(program, new RecordingHost(guards)) { Resumes = true }.Parse(entry, input);
		var nonresumed = new IrEvaluator(program, new RecordingHost(guards)).Parse(entry, input);

		foreach (var other in new[] { resuming, nonresumed })
		{
			Assert.True(other.Line == outcome.Line && other.Expected.SequenceEqual(outcome.Expected),
				$"On {Twin.Quoted(input)}: restarting by budget {budget} reads {outcome.Line}, without restart {other.Line}");
		}

		Interlocked.Add(ref EvaluatedRestarts, evaluator.Restarts);

		var head = outcome.Accepted
			? outcome.Line
			: "R " + outcome.Position.ToString(CultureInfo.InvariantCulture) + " " +
				string.Join(",", outcome.Expected.Select(label => program.Expected.IndexOf(label)).Order().Select(static index => index.ToString(CultureInfo.InvariantCulture)));

		return new SliceAnswer(head, host.Log) { Restart = restart };
	}
}
