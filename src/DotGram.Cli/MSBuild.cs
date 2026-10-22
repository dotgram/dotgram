using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DotGram.Cli;

/// <summary>The .NET SDK, run as a process: <c>dotnet msbuild</c> and <c>dotnet --version</c>.</summary>
static class MSBuild
{
	/// <summary>The first SDK whose MSBuild answers <c>-getItem</c> and <c>-getProperty</c>.</summary>
	static readonly Version FirstSdk = new(8, 0, 100);

	/// <summary>The first SDK whose MSBuild writes its answer to a file of its own choosing (<c>-getResultOutputFile</c>).</summary>
	static readonly Version FirstSdkWithResultFile = new(9, 0, 100);

	/// <summary>The SDK's version, read as its numbers: <c>8.0.100</c> of <c>8.0.100-rc.1.23455.8</c>.</summary>
	/// <exception cref="ToolException">A version that is not one, or one older than the first with <c>-getItem</c>.</exception>
	public static Version RequireSdk(string version)
	{
		var numbers = version.Trim();
		var end     = numbers.IndexOfAny(['-', '+']);

		if (end >= 0)
			numbers = numbers.Substring(0, end);

		if (!Version.TryParse(numbers, out var parsed))
			throw ToolException.Failure($"cannot tell which .NET SDK builds the project: 'dotnet --version' said '{version.Trim()}'.");

		if (parsed < FirstSdk)
			throw ToolException.Failure($"the project builds with .NET SDK {version.Trim()}, and the tool needs .NET SDK 8.0.100 or newer: it asks MSBuild for the compiler's inputs.");

		return parsed;
	}

	/// <summary>The SDK that builds the project: the one <c>dotnet</c> picks in its directory, where a <c>global.json</c> may pin one.</summary>
	public static Version SdkOf(string project)
	{
		var directory = Path.GetDirectoryName(project)!;
		var (exit, output, error) = Run(directory, ["--version"]);

		if (exit != 0)
			throw ToolException.Failure($"'dotnet --version' failed in {directory} (a global.json may pin an SDK that is not installed):{Environment.NewLine}{(output + error).Trim()}");

		return RequireSdk(output);
	}

	/// <summary>The frameworks the project targets, in its order: <c>TargetFrameworks</c>, or its one <c>TargetFramework</c>.</summary>
	public static IReadOnlyList<string> Frameworks(string project, string configuration, Version sdk)
	{
		using var json = Ask(project, ["-p:Configuration=" + configuration, "-getProperty:TargetFramework,TargetFrameworks"], sdk, "evaluating " + project + " failed");

		string Property(string name)
		{
			return json.RootElement.GetProperty("Properties").TryGetProperty(name, out var value) ? value.GetString() ?? "" : "";
		}

		var several = Property("TargetFrameworks").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		return several.Length > 0 ? several : Property("TargetFramework") is { Length: > 0 } one ? [one] : [];
	}

	/// <summary>
	/// One <c>dotnet msbuild</c> of a project, not restoring, building nothing but what the targets
	/// asked for, and its JSON answer: read from a file where MSBuild writes one (the encoding is then
	/// its own), and otherwise from what it printed, as UTF-8.
	/// </summary>
	/// <param name="what">What the run is for, said as a failure: the message of one begins with it.</param>
	/// <exception cref="ToolException">MSBuild exits otherwise than 0, or says nothing the tool can read.</exception>
	public static JsonDocument Ask(string project, IEnumerable<string> arguments, Version sdk, string what)
	{
		var answer = sdk >= FirstSdkWithResultFile ? Path.Combine(Path.GetTempPath(), "dotgram-cli-" + Guid.NewGuid().ToString("N") + ".json") : null;

		try
		{
			var (exit, output, error) = Run(
				Path.GetDirectoryName(project)!,
				["msbuild", project, "-nologo", "-nodeReuse:false", "-v:q", .. arguments, .. answer is null ? Array.Empty<string>() : ["-getResultOutputFile:" + answer]]);

			if (exit != 0)
				throw Failed(project, what, exit, output + error);

			try
			{
				// A file holds the answer and nothing else; what MSBuild printed holds it after whatever the
				// project's targets printed before it.
				if (answer is not null && File.Exists(answer))
					return JsonDocument.Parse(File.ReadAllText(answer, Encoding.UTF8));

				return Answer(output) ?? throw ToolException.Failure($"{what}: dotnet msbuild exited 0 and printed no result.{Environment.NewLine}{(output + error).Trim()}");
			}
			catch (JsonException exception)
			{
				throw ToolException.Failure($"{what}: the result of dotnet msbuild is not JSON ({exception.Message}).");
			}
		}
		finally
		{
			if (answer is not null)
			{
				try
				{
					File.Delete(answer);
				}
				catch (IOException)
				{
					// The temporary folder is the system's to clear.
				}
			}
		}
	}

	/// <summary>
	/// The JSON answer in what MSBuild printed: the text from a line that begins with <c>{</c> to the end
	/// of the output. A target may print a brace of its own earlier, in a message or in the middle of a
	/// line; none of that is the answer, which is the one that reads as JSON to the end.
	/// </summary>
	/// <returns>The answer, or null where nothing printed is one.</returns>
	public static JsonDocument? Answer(string output)
	{
		for (var start = output.IndexOf('{'); start >= 0; start = output.IndexOf('{', start + 1))
		{
			if (start > 0 && output[start - 1] != '\n')
				continue;

			try
			{
				return JsonDocument.Parse(output.Substring(start));
			}
			catch (JsonException)
			{
				// Not the answer: a line of something else that happens to begin with a brace.
			}
		}

		return null;
	}

	/// <summary>One <c>dotnet msbuild</c> that answers nothing the tool reads: it only has to succeed.</summary>
	public static void Build(string project, IEnumerable<string> arguments, string what)
	{
		var (exit, output, error) = Run(Path.GetDirectoryName(project)!, ["msbuild", project, "-nologo", "-nodeReuse:false", "-v:q", .. arguments]);

		if (exit != 0)
			throw Failed(project, what, exit, output + error);
	}

	/// <summary>A failed run, said by its errors: those MSBuild reports, or all it printed where it reports none.</summary>
	static ToolException Failed(string project, string what, int exit, string printed)
	{
		// A project that was not restored: said the one way, whichever step found it.
		if (printed.Contains("NETSDK1004", StringComparison.Ordinal) || printed.Contains("NETSDK1005", StringComparison.Ordinal))
			return ToolException.Failure($"{project} is not restored: run `dotnet restore` (or build it once); the tool never restores.");

		var errors = printed
			.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(static line => line.Contains(": error ", StringComparison.Ordinal))
			.Distinct(StringComparer.Ordinal)
			.Take(20)
			.ToList();

		var text = errors.Count > 0 ? string.Join(Environment.NewLine, errors) : Trim(printed);

		return ToolException.Failure($"{what} (dotnet msbuild exited {exit}):{Environment.NewLine}{text}");

		static string Trim(string text)
		{
			text = text.Trim();

			return text.Length > 2000 ? text.Substring(0, 2000) + " ..." : text;
		}
	}

	static (int Exit, string Output, string Error) Run(string directory, IEnumerable<string> arguments)
	{
		var start = new ProcessStartInfo(DotnetHost())
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding  = Encoding.UTF8,
			UseShellExecute        = false,
			WorkingDirectory       = directory,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		// Not a build of our own: no MSBuild node left behind, and no telemetry line in the output the
		// JSON may be read from.
		start.Environment["MSBUILDDISABLENODEREUSE"]     = "1";
		start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
		start.Environment["DOTNET_NOLOGO"]               = "1";

		Process process;

		try
		{
			process = Process.Start(start)!;
		}
		catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
		{
			throw ToolException.Failure($"cannot run '{start.FileName}' to ask MSBuild about the project: {exception.Message}");
		}

		Running.TryAdd(process, 0);

		try
		{
			var output = process.StandardOutput.ReadToEndAsync();
			var error  = process.StandardError.ReadToEndAsync();

			process.WaitForExit();

			return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
		}
		finally
		{
			// However the run ends, nothing of MSBuild's is left running: not the process, and not what a
			// target of the project started under it.
			Stop(process);
			Running.TryRemove(process, out _);
			process.Dispose();
		}
	}

	/// <summary>The <c>dotnet msbuild</c> processes this tool is waiting for.</summary>
	static readonly ConcurrentDictionary<Process, byte> Running = new();

	/// <summary>How many <c>dotnet msbuild</c> processes the tool is waiting for.</summary>
	public static int RunningCount
	{
		get { return Running.Count; }
	}

	/// <summary>
	/// Ends every <c>dotnet msbuild</c> the tool started, and what they started: for Ctrl+C and any
	/// other end of the tool. A build has no timeout, so this is the only thing that ends one early.
	/// </summary>
	public static void StopRunning()
	{
		foreach (var process in Running.Keys)
			Stop(process);
	}

	static void Stop(Process process)
	{
		try
		{
			if (!process.HasExited)
				process.Kill(entireProcessTree: true);
		}
		catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
		{
			// It ended by itself in the meantime.
		}
	}

	/// <summary>
	/// The <c>dotnet</c> that runs this tool, where it can be told; otherwise the one on the path.
	/// </summary>
	static string DotnetHost()
	{
		if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host && File.Exists(host))
			return host;

		if (Environment.ProcessPath is { } process &&
			Path.GetFileNameWithoutExtension(process).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			return process;

		return "dotnet";
	}
}
