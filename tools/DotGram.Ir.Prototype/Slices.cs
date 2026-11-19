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
sealed record SliceAnswer(string Head, List<string> Log);

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

		string?      head = null;
		List<string> log  = [];

		foreach (var line in output.Split('\n'))
		{
			if (line == ".")
			{
				answers.Add(new SliceAnswer(head ?? "", log));
				head = null;
				log  = [];
			}
			else if (line.StartsWith("L ", StringComparison.Ordinal))
				log.Add(line[2..]);
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
		int? stackKilobytes = null, int? systemStackKilobytes = null, SliceFrames frames = SliceFrames.Recursive)
	{
		var (node, environment) = Node;

		var directory = Workspace(name, "ts");

		try
		{
			File.WriteAllText(Path.Combine(directory, "parser.ts"), SlicePrinter.Print(program, SliceLanguage.TypeScript, frames));
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

	/// <summary>The program printed in C, compiled by gcc with a host and run over the inputs.</summary>
	/// <param name="host">The host's source among the hand-written files; the recording host where null.</param>
	/// <param name="stackMegabytes">The stack of a thread made for the reading, where not the process's own.</param>
	public static List<SliceAnswer> InC(
		string name, IrProgram program, string entry, IReadOnlyList<string> inputs,
		string? host = null, IEnumerable<string>? guardAnswers = null, int? stackMegabytes = null)
	{
		var gcc       = Gcc;
		var directory = Workspace(name, "c");

		try
		{
			File.WriteAllText(Path.Combine(directory, "parser.c"), SlicePrinter.Print(program, SliceLanguage.C));
			File.WriteAllText(Path.Combine(directory, "parser.h"), SlicePrinter.PrintHeaderInC(program));
			File.WriteAllLines(Path.Combine(directory, "answers.txt"), guardAnswers ?? []);
			WriteBatch(Path.Combine(directory, "batch.bin"), inputs);

			List<string> sources = ["driver.c", "parser.c"];

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

			var (exit, output, error) = Run(Path.Combine(directory, "slice"), [entry, "batch.bin", "answers.txt"], directory, environment);

			Assert.True(exit == 0, "the C parser: exit " + exit.ToString(CultureInfo.InvariantCulture) + " " + error);

			return Answers(output);
		}
		finally
		{
			Discard(directory);
		}
	}

	/// <summary>What the evaluator says of an input, in the words the drivers use.</summary>
	public static SliceAnswer Evaluated(IrProgram program, string entry, string input, Func<IrFragment, object?[], bool>? guards)
	{
		var host    = new RecordingHost(guards);
		var outcome = new IrEvaluator(program, host).Parse(entry, input);

		var head = outcome.Accepted
			? outcome.Line
			: "R " + outcome.Position.ToString(CultureInfo.InvariantCulture) + " " +
				string.Join(",", outcome.Expected.Select(label => program.Expected.IndexOf(label)).Order().Select(static index => index.ToString(CultureInfo.InvariantCulture)));

		return new SliceAnswer(head, host.Log);
	}
}
