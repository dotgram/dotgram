using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

	/// <summary>
	/// The longest directory a process can be started in: Win32 <c>CreateProcess</c> rejects a current
	/// directory of <c>MAX_PATH</c> (260) characters, including the terminator, less room for an 8.3 file
	/// name, whatever the long-path opt-in says. Fewer than this is always accepted. Elsewhere any directory
	/// the project can be in can be a current directory, and the project's own is always used.
	/// </summary>
	internal static readonly int MaxWorkingDirectory = OperatingSystem.IsWindows() ? 248 : int.MaxValue;

	/// <summary>
	/// Where to start <c>dotnet</c> for a project in <paramref name="directory"/>. The directory itself, while it
	/// is short enough: the <c>dotnet</c> muxer reads <c>global.json</c> from the current directory and up, so
	/// that is what makes it pick the SDK <c>dotnet build</c> picks there. A longer one cannot be a current
	/// directory, so the process starts in a fresh short folder that holds a copy of the <c>global.json</c> the
	/// project's directory would find, its relative <c>sdk.paths</c> made full (<see cref="Relocated"/>), so
	/// that the same SDK is chosen; MSBuild itself reads the project, its <c>Directory.Build.props</c> and the
	/// rest by their full paths, not through the current directory, and finds <c>msbuild-sdks</c> in the
	/// <c>global.json</c> above the project.
	/// </summary>
	/// <returns>The directory to start in, and the folder to delete afterwards (null where there is none).</returns>
	internal static (string Directory, string? Temporary) WorkingDirectoryFor(string directory, int? limit = null)
	{
		if (directory.Length < (limit ?? MaxWorkingDirectory))
			return (directory, null);

		var temporary = Path.Combine(Path.GetTempPath(), "dotgram-cli-" + Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(temporary);

		try
		{
			var copy = Path.Combine(temporary, "global.json");

			for (var folder = new DirectoryInfo(directory); folder is not null; folder = folder.Parent)
			{
				var pinned = Path.Combine(folder.FullName, "global.json");

				if (File.Exists(pinned))
				{
					if (Relocated(pinned) is { } text)
						File.WriteAllText(copy, text);
					else
						File.Copy(pinned, copy);

					return (temporary, temporary);
				}
			}

			// The project's directory finds no global.json: neither may the temporary folder, whose own
			// ancestors may hold one. An empty one ends the muxer's search upwards and pins nothing.
			File.WriteAllText(copy, "{}\n");

			return (temporary, temporary);
		}
		catch
		{
			Directory.Delete(temporary, recursive: true);
			throw;
		}
	}

	/// <summary>
	/// The text of a copy of the <c>global.json</c> at <paramref name="path"/> that chooses the SDK the
	/// original does from another folder. The muxer reads each <c>sdk.paths</c> entry other than
	/// <c>$host$</c> against the file's own folder, so a relative one is made full; nothing else in the file
	/// depends on where it is.
	/// </summary>
	/// <returns>The text, or null where nothing needs to change or the file does not read as JSON here
	/// (the muxer then reads the copy as it reads the original), and the file is copied as it is.</returns>
	internal static string? Relocated(string path)
	{
		// What the muxer takes, System.Text.Json may not (a key given twice, which the muxer reads as the
		// last): such a file is copied as it is, and only a file whose sdk.paths are relative is rewritten.
		try
		{
			var root = JsonNode.Parse(
				File.ReadAllText(path),
				documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

			if (root is not JsonObject { } file || file["sdk"] is not JsonObject sdk || sdk["paths"] is not JsonArray paths)
				return null;

			var folder  = Path.GetDirectoryName(Path.GetFullPath(path))!;
			var changed = false;

			for (var i = 0; i < paths.Count; i++)
			{
				if (paths[i] is JsonValue value && value.TryGetValue<string>(out var entry) && entry != "$host$" && entry.Length > 0 && !Path.IsPathRooted(entry))
				{
					paths[i] = Path.GetFullPath(entry, folder);
					changed  = true;
				}
			}

			return changed ? file.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) : null;
		}
		catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
		{
			return null;
		}
	}

	static (int Exit, string Output, string Error) Run(string directory, IEnumerable<string> arguments)
	{
		var (workingDirectory, temporary) = WorkingDirectoryFor(directory);

		try
		{
			return Run(workingDirectory, directory, arguments);
		}
		finally
		{
			if (temporary is not null)
			{
				try
				{
					Directory.Delete(temporary, recursive: true);
				}
				catch (IOException)
				{
					// The temporary folder is the system's to clear.
				}
			}
		}
	}

	static (int Exit, string Output, string Error) Run(string workingDirectory, string directory, IEnumerable<string> arguments)
	{
		var start = new ProcessStartInfo(DotnetHost())
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding  = Encoding.UTF8,
			UseShellExecute        = false,
			WorkingDirectory       = workingDirectory,
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
			throw ToolException.Failure($"cannot run '{start.FileName}' to ask MSBuild about the project in {directory}: {exception.Message}");
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
