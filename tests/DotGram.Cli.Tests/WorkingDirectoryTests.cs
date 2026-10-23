using System;

using Xunit;

namespace DotGram.Cli.Tests;

public sealed class WorkingDirectoryTests
{
	/// <summary>A short project directory is where <c>dotnet</c> starts, so its <c>global.json</c> picks the SDK.</summary>
	[Fact]
	public void A_short_directory_is_the_working_directory()
	{
		using var scratch = new Scratch();

		var (directory, temporary) = MSBuild.WorkingDirectoryFor(scratch.Root);

		Assert.Equal(scratch.Root, directory);
		Assert.Null(temporary);
	}

	/// <summary>A directory too long to start a process in is replaced by a short one holding the <c>global.json</c> the long one would find.</summary>
	[Fact]
	public void A_long_directory_is_replaced_by_a_short_one_with_the_same_global_json()
	{
		using var scratch = new Scratch();

		var deep = scratch.Path("a", "b");

		Directory.CreateDirectory(deep);

		var (directory, temporary) = MSBuild.WorkingDirectoryFor(deep, limit: 1);

		try
		{
			Assert.NotNull(temporary);
			Assert.Equal(temporary, directory);
			Assert.Equal(File.ReadAllText(scratch.Path("global.json")), File.ReadAllText(Path.Combine(directory, "global.json")));
		}
		finally
		{
			Directory.Delete(temporary!, recursive: true);
		}
	}

	/// <summary>
	/// A project in a directory of more than 260 characters is asked about like one in a short directory: the same SDK.
	/// On Windows such a directory cannot be a current directory, which is what this guards.
	/// </summary>
	[Fact]
	public void The_sdk_of_a_project_in_a_very_long_directory_is_the_one_of_a_short_directory()
	{
		using var scratch = new Scratch();

		var shortProject = scratch.Write("p/p.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
		var parts        = new List<string> { "deep" };

		while (scratch.Path([.. parts]).Length < 300)
			parts.Add(new string('d', 40));

		parts.Add("p.csproj");

		string longProject;

		try
		{
			longProject = scratch.Write(Path.Combine([.. parts]), File.ReadAllText(shortProject));
		}
		catch (Exception exception) when (exception is PathTooLongException or DirectoryNotFoundException or IOException)
		{
			// The machine cannot hold such a path (Windows without long paths): nothing to check.
			return;
		}

		Assert.True(longProject.Length > 260);
		Assert.Equal(MSBuild.SdkOf(shortProject), MSBuild.SdkOf(longProject));
	}
}
