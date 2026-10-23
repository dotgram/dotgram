using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DotGram.Cli.PackageSmoke;

// What the tool package promises, asked of the package rather than of the repository: installed from
// a feed by version into a local tool manifest, with no project reference anywhere, it generates a
// project that does not reference DotGram and a lone grammar, and the files it wrote compile in a
// project without the generator — beside another generator that adds EmbeddedAttribute — and run.
// It is run on .NET 8 and on .NET 10 with the same input, and the two outputs must be the same bytes.
//
// Not in the solution, and run by CI after the pack step: it cannot be restored until the package
// it names exists.

static class Program
{
	const string Host = """"
		using System;

		using DotGram;

		namespace Smoke;

		[Gram("""
			trivia = [' ' | '\t']*

			Number : @int = ['0'..'9']+ => @(int.Parse(parserText))
			Sum    : @int = left: Number & '+' & right: Number => @(left + right)

			parse Sum
			""")]
		public partial class Adder
		{
		}

		static class Program
		{
			static int Main()
			{
				var match = Adder.TryParseSum("1 + 41");

				Console.WriteLine($"1 + 41 = {match.Value}");

				return match.IsSuccess && match.Value == 42 ? 0 : 1;
			}
		}
		"""";

	static string _root = "";
	static string _artifacts = "";
	static string _version = "";
	static string _work = "";
	static string _packages = "";

	static int Main()
	{
		_root      = RepositoryRoot();
		_artifacts = Path.Combine(_root, "artifacts");
		_version   = Regex.Match(File.ReadAllText(Path.Combine(_root, "Directory.Build.props")), "<Version>([^<]+)</Version>").Groups[1].Value;
		_work      = Path.Combine(Path.GetTempPath(), "dotgram-cli-smoke", Guid.NewGuid().ToString("N").Substring(0, 12));
		_packages  = Path.Combine(_work, "packages");

		Directory.CreateDirectory(_work);

		try
		{
			Smoke();
			Console.WriteLine("The tool package works.");

			return 0;
		}
		catch (SmokeFailure failure)
		{
			Console.Error.WriteLine(failure.Message);

			return 1;
		}
		finally
		{
			try
			{
				Directory.Delete(_work, recursive: true);
			}
			catch (IOException)
			{
			}
		}
	}

	static void Smoke()
	{
		// The packages this build just made, and nothing else called DotGram from anywhere.
		Write("nuget.config", $"""
			<?xml version="1.0" encoding="utf-8"?>
			<configuration>
				<packageSources>
					<clear />
					<add key="artifacts" value="{_artifacts}" />
					<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
				</packageSources>
				<packageSourceMapping>
					<packageSource key="artifacts">
						<package pattern="DotGram" />
						<package pattern="DotGram.*" />
					</packageSource>
					<packageSource key="nuget.org">
						<package pattern="*" />
					</packageSource>
				</packageSourceMapping>
			</configuration>
			""");

		File.Copy(Path.Combine(_root, "global.json"), Path.Combine(_work, "global.json"));

		Run(_work, "dotnet", "new", "tool-manifest");
		Run(_work, "dotnet", "tool", "install", "DotGram.Cli", "--version", _version, "--configfile", "nuget.config");

		var version = Run(_work, "dotnet", "dotgram", "--version");
		Require(version.Contains($"dotnet-dotgram {_version}") && version.Contains("(generator "), $"--version printed '{version.Trim()}'");

		Require(Run(_work, "dotnet", "dotgram", "--help").Contains("Usage: dotnet dotgram generate"), "--help has no usage");
		Require(Run(_work, "dotnet", "dotgram", "generate", "--help").Contains("--line-paths"), "generate --help lists no options");
		Require(Run(_work, "dotnet", "dotgram", "--skill").Contains("name: dotgram-cli"), "--skill printed no skill");

		// A project that does not reference DotGram, generated, then compiled and run without it.
		Write("App/App.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
				<PropertyGroup>
					<OutputType>Exe</OutputType>
					<TargetFramework>net10.0</TargetFramework>
				</PropertyGroup>
			</Project>
			""");
		Write("App/Program.cs", Host);

		Run(_work, "dotnet", "restore", "App/App.csproj");

		var onEight = Runtime(roll: "Minor");
		var onTen   = Runtime(roll: "LatestMajor");

		// The same folder on both runtimes, the first moved aside before the second: the #line paths are relative to it.
		Run(_work, "dotnet", ["dotgram", "generate", "App/App.csproj", "-o", "App/Generated"], onEight);
		Directory.Move(Path.Combine(_work, "App", "Generated"), Path.Combine(_work, "Eight"));
		Run(_work, "dotnet", ["dotgram", "generate", "App/App.csproj", "-o", "App/Generated"], onTen);
		AssertEqualFolders("Eight", "App/Generated", "the project, on .NET 8 and on .NET 10");

		Require(Run(_work, "dotnet", "run", "--project", "App/App.csproj", "--no-restore").Contains("1 + 41 = 42"), "the project's parser read something else");

		// A lone grammar.
		Write("adder.gram", """
			trivia = [' ' | '\t']*

			Number : @int = ['0'..'9']+ => @(int.Parse(parserText))
			Sum    : @int = left: Number & '+' & right: Number => @(left + right)

			parse Sum
			""");

		Run(_work, "dotnet", ["dotgram", "generate", "adder.gram", "-o", "FromGram/Generated", "--namespace", "Smoke", "--class", "Adder", "--public"], onEight);
		Directory.Move(Path.Combine(_work, "FromGram", "Generated"), Path.Combine(_work, "EightGram"));
		Run(_work, "dotnet", ["dotgram", "generate", "adder.gram", "-o", "FromGram/Generated", "--namespace", "Smoke", "--class", "Adder", "--public"], onTen);
		AssertEqualFolders("EightGram", "FromGram/Generated", "the grammar, on .NET 8 and on .NET 10");

		Write("FromGram/FromGram.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
				<PropertyGroup>
					<OutputType>Exe</OutputType>
					<TargetFramework>net10.0</TargetFramework>
				</PropertyGroup>
			</Project>
			""");
		Write("FromGram/Program.cs", """
			using System;

			static class Program
			{
				static int Main()
				{
					var match = Smoke.Adder.TryParseSum("1 + 41");

					Console.WriteLine($"1 + 41 = {match.Value}");

					return match.IsSuccess && match.Value == 42 ? 0 : 1;
				}
			}
			""");

		Require(Run(_work, "dotnet", "run", "--project", "FromGram/FromGram.csproj").Contains("1 + 41 = 42"), "the grammar's parser read something else");

		// The generated files beside another generator that adds EmbeddedAttribute, as many do.
		Write("Other/Other.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
				<PropertyGroup>
					<TargetFramework>netstandard2.0</TargetFramework>
					<LangVersion>latest</LangVersion>
					<IsRoslynComponent>true</IsRoslynComponent>
					<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
				</PropertyGroup>
				<ItemGroup>
					<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" PrivateAssets="all" />
				</ItemGroup>
			</Project>
			""");
		Write("Other/Generator.cs", """
			using System;
			using Microsoft.CodeAnalysis;

			[Generator]
			public sealed class EmbeddedGenerator : IIncrementalGenerator
			{
				public void Initialize(IncrementalGeneratorInitializationContext context)
				{
					context.RegisterPostInitializationOutput(static output => output.AddSource(
						"Microsoft.CodeAnalysis.EmbeddedAttribute.g.cs",
						"namespace Microsoft.CodeAnalysis { internal sealed partial class EmbeddedAttribute : System.Attribute { } }"));
				}
			}
			""");
		Write("Beside/Beside.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
				<PropertyGroup>
					<OutputType>Exe</OutputType>
					<TargetFramework>net10.0</TargetFramework>
				</PropertyGroup>
				<ItemGroup>
					<ProjectReference Include="../Other/Other.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
				</ItemGroup>
			</Project>
			""");
		Write("Beside/Program.cs", Host);

		Run(_work, "dotnet", "build", "Other/Other.csproj");
		Run(_work, "dotnet", "restore", "Beside/Beside.csproj");
		Run(_work, "dotnet", "dotgram", "generate", "Beside/Beside.csproj", "-o", "Beside/Generated");
		Require(Run(_work, "dotnet", "run", "--project", "Beside/Beside.csproj", "--no-restore").Contains("1 + 41 = 42"), "the parser beside another generator read something else");

		// Run without installing, on .NET 10.
		var run = Run(_work, "dotnet", "dnx", "DotGram.Cli", "--version", _version, "--configfile", Path.Combine(_work, "nuget.config"), "--yes", "--", "--version");
		Require(run.Contains($"dotnet-dotgram {_version}"), $"dnx printed '{run.Trim()}'");
	}

	/// <summary>The tool on one runtime only: .NET 8 where it may not leave the major, the newest installed where it is not.</summary>
	static Dictionary<string, string> Runtime(string roll)
	{
		return new Dictionary<string, string> { ["DOTNET_ROLL_FORWARD"] = roll };
	}

	static void AssertEqualFolders(string first, string second, string what)
	{
		var left  = Files(Path.Combine(_work, first));
		var right = Files(Path.Combine(_work, second));

		Require(left.Count > 0, $"{what}: nothing was written");
		Require(left.Keys.SequenceEqual(right.Keys), $"{what}: the files differ ({string.Join(", ", left.Keys)} against {string.Join(", ", right.Keys)})");

		foreach (var (name, bytes) in left)
			Require(bytes.SequenceEqual(right[name]), $"{what}: {name} differs");
	}

	/// <summary>The folder's files by name; the manifest holds the tool's own version and is the same on both runtimes, so it is held too.</summary>
	static SortedDictionary<string, byte[]> Files(string folder)
	{
		var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);

		foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
			files[Path.GetRelativePath(folder, path)] = File.ReadAllBytes(path);

		return files;
	}

	static void Write(string relative, string text)
	{
		var path = Path.Combine(_work, relative);

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, text, new UTF8Encoding(false));
	}

	static string Run(string directory, string file, params string[] arguments)
	{
		return Run(directory, file, arguments, null);
	}

	static string Run(string directory, string file, string[] arguments, Dictionary<string, string>? environment)
	{
		var start = new ProcessStartInfo(file)
		{
			WorkingDirectory       = directory,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
		};

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		// Both of them the run's own: the packages a tool is restored into, and the cache that remembers where
		// a local tool was restored, which would otherwise point at the folder of an earlier run.
		start.Environment["NUGET_PACKAGES"]                 = _packages;
		start.Environment["DOTNET_CLI_HOME"]                = Path.Combine(_work, "home");
		start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"]    = "1";
		start.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
		start.Environment["DOTNET_NOLOGO"]                  = "1";

		foreach (var (name, value) in environment ?? [])
			start.Environment[name] = value;

		using var process = Process.Start(start)!;
		var error  = process.StandardError.ReadToEndAsync();
		var output = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		var text = output + error.Result;

		if (process.ExitCode != 0)
			throw new SmokeFailure($"{file} {string.Join(' ', arguments)} exited {process.ExitCode}:\n{text}");

		return text;
	}

	static void Require(bool condition, string message)
	{
		if (!condition)
			throw new SmokeFailure(message);
	}

	static string RepositoryRoot([CallerFilePath] string source = "")
	{
		var at = new DirectoryInfo(Path.GetDirectoryName(source)!);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("DotGram.slnx was not found above the smoke's source.");
	}

	sealed class SmokeFailure(string message) : Exception(message);
}
