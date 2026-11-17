using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// What sets the native build of the tool apart from the .NET tool: a project is the .NET tool's, and what
/// the .NET tool would read from the generator when it runs is written into the tool when it is built.
/// Run with <c>DOTGRAM_CLI_NATIVE</c> naming the native executable, these hold that executable.
/// </summary>
public sealed class NativeTests
{
	/// <summary>The native tool answers a project with the .NET tool, and the .NET tool reads one.</summary>
	[Fact]
	public void A_project_is_the_NET_tool_s()
	{
		using var scratch = new Scratch();

		var project = scratch.Write("p/p.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");

		if (Tool.Native is null)
		{
			Assert.True(Cli.ReadsProjects);

			var refusal = Cli.NoProjects(project);

			Assert.Equal(ExitCode.Failure, refusal.ExitCode);
			Assert.Contains("dotnet tool install DotGram.Cli", refusal.Message, StringComparison.Ordinal);
			return;
		}

		var run = Tool.Process(Tool.Native, scratch.Root, ["generate", project, "-o", scratch.Path("out")]);

		Assert.Equal(ExitCode.Failure, run.Exit);
		Assert.True(run.Error.Contains($"dotnet dotgram: error: {Cli.NoProjects(project).Message}", StringComparison.Ordinal), run.ToString());
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>
	/// The oldest compiler the generator loads into, written into the tool when it is built, is the
	/// version of <c>Microsoft.CodeAnalysis</c> the generator references.
	/// </summary>
	[Fact]
	public void The_compiler_dotgram_needs_is_the_one_it_references()
	{
		var referenced = typeof(DotGram.Generation.GramGenerator).Assembly.GetReferencedAssemblies()
			.Single(static reference => reference.Name == "Microsoft.CodeAnalysis")
			.Version;

		Assert.Equal(referenced, AnalyzerGenerators.DotGramNeeds);
	}
}
