using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Generation;

namespace DotGram.Cli.Tests;

/// <summary>A directory of its own for one test, outside the repository, deleted afterwards.</summary>
/// <remarks>
/// Outside, so that the repository's <c>Directory.Build.props</c> — its LangVersion, its implicit
/// usings, its warnings as errors — is not what a scratch project is evaluated with; with the
/// repository's <c>global.json</c> copied in, so that the SDK that builds it is the one the
/// repository builds with.
/// </remarks>
sealed class Scratch : IDisposable
{
	public string Root { get; }

	public Scratch()
	{
		Root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotgram-cli-tests", Guid.NewGuid().ToString("N").Substring(0, 12));

		Directory.CreateDirectory(Root);
		File.Copy(System.IO.Path.Combine(Repository.Root, "global.json"), System.IO.Path.Combine(Root, "global.json"));
	}

	public string Path(params string[] parts)
	{
		return System.IO.Path.Combine([Root, .. parts]);
	}

	/// <summary>Writes a file under the root, creating its directory; the text as given, UTF-8 without a byte order mark.</summary>
	public string Write(string relative, string text)
	{
		var path = Path(relative);

		Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
		File.WriteAllText(path, text, new UTF8Encoding(false));

		return path;
	}

	public void Dispose()
	{
		try
		{
			Directory.Delete(Root, recursive: true);
		}
		catch (IOException)
		{
			// A build server may hold a file a moment longer; the temporary folder is cleared anyway.
		}
	}
}

/// <summary>Where the repository is, from this file.</summary>
static class Repository
{
	public static string Root { get; } = Find();

	static string Find([CallerFilePath] string path = "")
	{
		var directory = System.IO.Path.GetDirectoryName(path);

		while (directory is not null && !File.Exists(System.IO.Path.Combine(directory, "DotGram.slnx")))
			directory = System.IO.Path.GetDirectoryName(directory);

		return directory ?? throw new InvalidOperationException("The repository root was not found above " + path);
	}

	public static string Snapshots => System.IO.Path.Combine(Root, "tests", "Snapshots");
}

/// <summary>What one run of the tool printed and exited with.</summary>
sealed record ToolRun(int Exit, string Output, string Error)
{
	public override string ToString()
	{
		return $"exit {Exit}\n--- output\n{Output}\n--- error\n{Error}";
	}
}

/// <summary>The tool, run in this process.</summary>
static class Tool
{
	public static ToolRun Run(string currentDirectory, params string[] arguments)
	{
		var output = new StringWriter();
		var error  = new StringWriter();
		var exit   = Cli.Run(arguments, output, error, currentDirectory);

		return new ToolRun(exit, output.ToString(), error.ToString());
	}
}

/// <summary>
/// <c>dotnet build</c> of a scratch project with the generator as its analyzer, the generated files
/// written out as the compiler writes them.
/// </summary>
static class Build
{
	/// <summary>The generator the tool carries, which a scratch project takes as its analyzer.</summary>
	public static string Generator => typeof(GramGenerator).Assembly.Location;

	/// <summary>The item that makes a scratch project run the generator, as the package's targets would.</summary>
	public static string AnalyzerItems => $"""
		<ItemGroup>
		  <Analyzer Include="{Generator}" />
		  <CompilerVisibleProperty Include="DotGramTrace" />
		  <CompilerVisibleProperty Include="DotGramPositionalFollow" />
		</ItemGroup>
		""";

	/// <summary>Builds the project; the folder the compiler wrote the generator's files into.</summary>
	public static string Generated(string project, params string[] properties)
	{
		var (exit, text, folder) = TryGenerated(project, properties);

		if (exit != 0)
			throw new InvalidOperationException($"dotnet build {project} exited {exit}:\n{text}");

		return folder;
	}

	/// <summary>
	/// Builds the project, which may fail: the compiler writes the generator's files before it
	/// compiles them, so the folder holds them either way. In Release unless a configuration is given,
	/// the tool's own default.
	/// </summary>
	public static (int Exit, string Output, string Folder) TryGenerated(string project, params string[] properties)
	{
		var generated = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(project)!, "obj", "generated-by-build");

		if (!properties.Any(static property => property.StartsWith("Configuration=", StringComparison.Ordinal)))
			properties = ["Configuration=Release", .. properties];

		var (exit, text) = Try(project, [.. properties, "EmitCompilerGeneratedFiles=true", "CompilerGeneratedFilesOutputPath=" + generated]);

		return (exit, text, System.IO.Path.Combine(generated, "DotGram", typeof(GramGenerator).FullName!));
	}

	/// <summary>Builds the project and fails with its output where it does not build.</summary>
	public static string Run(string project, params string[] properties)
	{
		var (exit, text) = Try(project, properties);

		if (exit != 0)
			throw new InvalidOperationException($"dotnet build {project} exited {exit}:\n{text}");

		return text;
	}

	/// <summary>Builds the project; the exit code and everything it printed.</summary>
	public static (int Exit, string Output) Try(string project, params string[] properties)
	{
		return Dotnet.Run(
			System.IO.Path.GetDirectoryName(project)!,
			["build", project, "-nologo", "-nodeReuse:false", "-v:q", "-clp:NoSummary", .. properties.Select(static property => "-p:" + property)]);
	}
}

/// <summary>The SDK, run as a process in a directory.</summary>
static class Dotnet
{
	public static (int Exit, string Output) Run(string directory, params string[] arguments)
	{
		var start = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
			WorkingDirectory       = directory,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		start.Environment["MSBUILDDISABLENODEREUSE"]     = "1";
		start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
		start.Environment["DOTNET_NOLOGO"]               = "1";

		using var process = Process.Start(start)!;

		var output = process.StandardOutput.ReadToEndAsync();
		var error  = process.StandardError.ReadToEndAsync();

		process.WaitForExit();

		return (process.ExitCode, output.Result + error.Result);
	}

	/// <summary>Restores a project, which the tool needs and never does itself.</summary>
	public static void Restore(string project)
	{
		Ok(System.IO.Path.GetDirectoryName(project)!, "restore", project, "-nologo");
	}

	/// <summary>Runs it and fails with what it printed where it exits otherwise than 0.</summary>
	public static string Ok(string directory, params string[] arguments)
	{
		var (exit, output) = Run(directory, arguments);

		if (exit != 0)
			throw new InvalidOperationException($"dotnet {string.Join(" ", arguments)} exited {exit}:\n{output}");

		return output;
	}
}
