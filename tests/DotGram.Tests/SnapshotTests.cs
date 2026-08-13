using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A whole grammar compiled, and the whole file it produces checked in beside it.
/// </summary>
/// <remarks>
/// <para>
/// The unit tests each hold one feature still and ask whether it works. This asks a
/// different question: what does the generator actually produce, all of it, for a
/// grammar somebody might write. Reviewing a diff of the answer is how a change is
/// seen — the first look at a generated file found a call to a recognizer that was
/// never emitted, which every passing unit test had missed because no test grammar
/// happened to use a standard-library rule.
/// </para>
/// <para>
/// Roc tested its macro this way and it is the one thing from that project that
/// transfers unchanged; <c>GrammarNormalizerTests</c> does the same a stage earlier.
/// </para>
/// </remarks>
public sealed class SnapshotTests
{
	const string Namespace = "DotGram.Snapshots";

	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Generated_code_matches_what_is_checked_in(string name)
	{
		var result = GramCompiler.Compile(
			File.ReadAllText(Path.Combine(Directory, name + ".gram")),
			new GramCompilerOptions { ClassName = name, Namespace = Namespace });

		Assert.Empty(result.Diagnostics);

		var actual   = Assert.Single(result.Sources).Text;
		var expected = Path.Combine(Directory, name + ".gram.g.cs");

		// Compiled first: a snapshot that matches but does not build is worse than no
		// snapshot, because it makes the wrong thing look approved.
		EmittedCode.Compile(actual, name, Namespace);

		if (!File.Exists(expected))
		{
			File.WriteAllText(expected, actual, CSharpFile);

			Assert.Fail(
				$"No snapshot for '{name}'; wrote one to {expected}. Read it, and commit it if it is right.");
		}

		if (Normalize(File.ReadAllText(expected)) == Normalize(actual))
			return;

		// The failure message cannot show a file this size usefully, so the answer goes
		// next to the question and the diff is taken in the editor.
		var rejected = expected + ".actual";

		File.WriteAllText(rejected, actual, CSharpFile);

		Assert.Fail($"Generated code differs from the snapshot. Diff {expected} against {rejected}.");
	}

	/// <summary>
	/// UTF-8 with a byte-order mark — what Visual Studio writes, and what CLAUDE.md
	/// requires of every <c>.cs</c> file in the repository. A snapshot written without one
	/// is committed as an encoding change over the whole file.
	/// </summary>
	static Encoding CSharpFile { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

	public static TheoryData<string> Snapshots =>
		new(System.IO.Directory
			.GetFiles(Directory, "*.gram")
			.Select(Path.GetFileNameWithoutExtension)
			.OfType<string>());

	static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd();

	/// <summary>
	/// Where the snapshots live.
	/// </summary>
	/// <remarks>
	/// In the source tree rather than the output directory, because updating one has to
	/// change the file that is committed — a snapshot nobody can diff is not a snapshot.
	/// <para>
	/// Beside the test project rather than inside it: an expected <c>.g.cs</c> is a
	/// fixture, and a fixture inside a compiled project has to be excluded from
	/// compilation and told apart from the real generated files that land in
	/// <c>obj/GeneratedFiles</c>. Out here it is neither, and no csproj has to say so.
	/// </para>
	/// </remarks>
	static string Directory => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
