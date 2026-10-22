using System;
using System.Diagnostics;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// Tests that end the MSBuild processes the tool is waiting for. They end every one, so no other test
/// runs beside them.
/// </summary>
[CollectionDefinition("MSBuild children", DisableParallelization = true)]
public sealed class MSBuildChildren
{
}

[Collection("MSBuild children")]
public sealed class ChildProcessTests
{
	/// <summary>
	/// A build the tool waits for does not outlive it: ending the tool's children ends MSBuild and what
	/// the project's target started under it, and the run is a failure, not a hang.
	/// </summary>
	[Fact]
	public async Task Stopping_the_tool_ends_the_build_it_waits_for_and_what_that_started()
	{
		using var scratch = new Scratch();

		scratch.Write("p/A.cs", "class A { }");

		var project = scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			  <Target Name="Hang" BeforeTargets="CoreCompile">
			    <MakeDir Directories="$(MSBuildProjectDirectory)/obj" />
			    <Exec Condition="!$([MSBuild]::IsOSPlatform('Windows'))" Command="echo %24%24 &gt; '$(MSBuildProjectDirectory)/obj/pid'; sleep 120" />
			    <Exec Condition="$([MSBuild]::IsOSPlatform('Windows'))" Command="ping -n 120 127.0.0.1 &gt; nul" />
			  </Target>
			</Project>
			""");

		Dotnet.Restore(project);

		var pidFile = scratch.Path("p", "obj", "pid");
		var run     = Task.Run(() => Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out")), TestContext.Current.CancellationToken);

		// The target is running once it has written its process id; where there is no such file, once MSBuild has had a while.
		Assert.True(SpinWait.SpinUntil(() => OperatingSystem.IsWindows() ? MSBuild.RunningCount > 0 : File.Exists(pidFile), TimeSpan.FromSeconds(120)), "the target did not start");

		if (OperatingSystem.IsWindows())
			await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

		MSBuild.StopRunning();

		var result = await run.WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

		Assert.Equal(ExitCode.Failure, result.Exit);
		Assert.Equal(0, MSBuild.RunningCount);
		Assert.False(Directory.Exists(scratch.Path("out")));

		if (!OperatingSystem.IsWindows())
		{
			var pid = int.Parse(File.ReadAllText(pidFile).Trim(), System.Globalization.CultureInfo.InvariantCulture);

			Assert.True(SpinWait.SpinUntil(() => !Exists(pid), TimeSpan.FromSeconds(20)), $"process {pid} started by the build is still running");
		}

		static bool Exists(int pid)
		{
			try
			{
				using var process = Process.GetProcessById(pid);

				return !process.HasExited;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
	}
}
