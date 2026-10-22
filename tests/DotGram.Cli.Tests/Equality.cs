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
	/// <c>EmbeddedAttribute</c>'s text is Roslyn's, not the generator's, and differs between compiler
	/// versions: that one file is not compared by bytes. It must be in both folders and parse; every
	/// other file is held byte for byte, with no normalisation.
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
				var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(System.Text.Encoding.UTF8.GetString(b));

				Assert.Empty(tree.GetDiagnostics());
				Assert.Contains("class EmbeddedAttribute", tree.GetRoot().ToFullString(), StringComparison.Ordinal);
				continue;
			}

			if (!a.AsSpan().SequenceEqual(b))
				Assert.Fail($"{name}: the tool's file differs from the build's ({a.Length} and {b.Length} bytes); diff {Path.Combine(built, name!)} {Path.Combine(tool, name!)}");
		}
	}
}
