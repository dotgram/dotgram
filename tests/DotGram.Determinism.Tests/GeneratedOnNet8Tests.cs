using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar;
using DotGram.Tests;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Determinism.Tests;

/// <summary>
/// The generator on .NET 8 writes the snapshots .NET 10 wrote, byte for byte.
/// </summary>
/// <remarks>
/// <para>
/// The generator runs in the compiler's process, and so on whatever runtime that is: the .NET SDK's,
/// an older one under a roll-forward, .NET Framework in Visual Studio's own build. What it writes
/// must not depend on which. The one way it did was by asking the runtime about characters — the
/// general category of a code unit, which each runtime answers from its own Unicode version; .NET
/// 8's is older than .NET 10's by 22 code units of the plane, enough to move a first set of
/// <c>\p{L}</c>. The snapshots are taken by DotGram.Tests on .NET 10; this compiles every one again
/// here, with the same options, and holds it to the file checked in.
/// </para>
/// <para>
/// .NET Framework is not run here: it has no host on the machines the tests run on by default.
/// </para>
/// </remarks>
public sealed class GeneratedOnNet8Tests
{
	[Fact]
	public void The_runtime_is_not_the_one_the_snapshots_were_written_on()
	{
		Assert.Equal(8, Environment.Version.Major);
	}

	[Theory]
	[MemberData(nameof(Snapshots))]
	public void A_snapshot_is_what_the_generator_writes_here(string name)
	{
		var text    = File.ReadAllText(Path.Combine(Directory, name + ".gram"));
		var written = new List<string>();

		foreach (var (options, _) in SnapshotRenderings.Of(name, text, References))
		{
			var result = GramCompiler.Compile(text, options);

			Assert.NotEmpty(result.Sources);

			foreach (var source in result.Sources)
			{
				written.Add(source.HintName);

				var held = Path.Combine(Directory, source.HintName);

				Assert.True(File.Exists(held), $"No snapshot '{source.HintName}' to hold the output to.");
				Assert.True(
					Normalize(File.ReadAllText(held)) == Normalize(source.Text),
					$"On .NET {Environment.Version} the generator wrote '{source.HintName}' differently from the snapshot.");
			}
		}

		// And the files themselves: a part the snapshots hold and the generator no longer writes, or
		// one it writes beside them, is a difference too. Every file of a grammar is named after it.
		var snapshots = System.IO.Directory
			.GetFiles(Directory, name + ".*.g.cs")
			.Select(Path.GetFileName)
			.OfType<string>()
			.OrderBy(static one => one, StringComparer.Ordinal);

		Assert.Equal(snapshots, written.OrderBy(static one => one, StringComparer.Ordinal));
	}

	public static TheoryData<string> Snapshots =>
		new(System.IO.Directory
			.GetFiles(Directory, "*.gram")
			.Select(Path.GetFileNameWithoutExtension)
			.OfType<string>()
			.OrderBy(static one => one, StringComparer.Ordinal));

	/// <summary>What a host's declarations are compiled against: this runtime's own assemblies.</summary>
	static ImmutableArray<MetadataReference> References { get; } =
	[
		.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)),
	];

	static string Normalize(string text)
	{
		return text.Replace("\r\n", "\n").TrimEnd();
	}

	static string Directory => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
