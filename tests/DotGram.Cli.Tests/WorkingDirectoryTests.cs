using System;
using System.Text.Json;

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
	/// Off Windows a directory of any length can be a current directory, so the project's own is used: a
	/// copy of its <c>global.json</c> elsewhere may choose another SDK.
	/// </summary>
	[Fact]
	public void Off_Windows_a_long_directory_is_the_working_directory()
	{
		if (OperatingSystem.IsWindows())
			return;

		var deep = "/" + string.Join("/", Enumerable.Repeat(new string('d', 40), 10));

		var (directory, temporary) = MSBuild.WorkingDirectoryFor(deep);

		Assert.Equal(deep, directory);
		Assert.Null(temporary);
	}

	/// <summary>
	/// The copy of a <c>global.json</c> whose <c>sdk.paths</c> are relative to it names the same folders: the
	/// muxer reads them against the file's folder, and the copy is in another. <c>$host$</c> and a full
	/// path are kept as they are, and so is the rest of the file.
	/// </summary>
	[Fact]
	public void A_relative_sdk_path_in_the_copied_global_json_still_names_the_folder_beside_the_original()
	{
		using var scratch = new Scratch();

		var full = scratch.Path("elsewhere");

		scratch.Write("global.json", $$"""
			{
			  // the SDK in the repository's own folder first
			  "sdk": { "version": "10.0.100", "rollForward": "latestMajor", "paths": [ ".dotnet", "$host$", {{JsonSerializer.Serialize(full)}}, "../tools/sdk" ] },
			  "msbuild-sdks": { "My.Sdk": "1.0.0" },
			}
			""");

		var deep = scratch.Path("a", "b");

		Directory.CreateDirectory(deep);

		var (directory, temporary) = MSBuild.WorkingDirectoryFor(deep, limit: 1);

		try
		{
			using var copy = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "global.json")));

			var sdk = copy.RootElement.GetProperty("sdk");

			Assert.Equal(
				[scratch.Path(".dotnet"), "$host$", full, Path.GetFullPath(Path.Combine(scratch.Root, "..", "tools", "sdk"))],
				sdk.GetProperty("paths").EnumerateArray().Select(static path => path.GetString()));
			Assert.Equal("10.0.100", sdk.GetProperty("version").GetString());
			Assert.Equal("latestMajor", sdk.GetProperty("rollForward").GetString());
			Assert.Equal("1.0.0", copy.RootElement.GetProperty("msbuild-sdks").GetProperty("My.Sdk").GetString());
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
