using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>The tool's folder held to the files a build generated, byte for byte.</summary>
static class Equality
{
	/// <summary>
	/// Every file the build generated is in the tool's folder with the same bytes, and the tool wrote
	/// nothing the build did not, its own manifest and host aside.
	/// </summary>
	/// <remarks>
	/// <c>EmbeddedAttribute</c>'s text is Roslyn's, and the tool's Roslyn is newer than the SDK's: the
	/// two are held to the same text with line ends made alike, and whether it compiles is what the
	/// pack smoke test asks.
	/// </remarks>
	public static void Same(string built, string tool, params string[] alsoWritten)
	{
		const string Embedded = "Microsoft.CodeAnalysis.EmbeddedAttribute.cs";

		var builtFiles = Directory.GetFiles(built).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToList();
		var toolFiles  = Directory.GetFiles(tool).Select(Path.GetFileName)
			.Where(name => name != OutputFolder.ManifestName && !alsoWritten.Contains(name))
			.Order(StringComparer.Ordinal)
			.ToList();

		Assert.Equal(builtFiles, toolFiles);

		foreach (var name in builtFiles)
		{
			var a = File.ReadAllBytes(Path.Combine(built, name!));
			var b = File.ReadAllBytes(Path.Combine(tool, name!));

			if (name == Embedded)
			{
				Assert.Equal(Normal(a), Normal(b));
				continue;
			}

			if (!a.AsSpan().SequenceEqual(b))
				Assert.Fail($"{name}: the tool's file differs from the build's ({a.Length} and {b.Length} bytes); diff {Path.Combine(built, name!)} {Path.Combine(tool, name!)}");
		}

		static string Normal(byte[] bytes)
		{
			return System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF').Replace("\r\n", "\n", StringComparison.Ordinal);
		}
	}
}
