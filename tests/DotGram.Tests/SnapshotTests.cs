using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
	const string Namespace = SnapshotRenderings.Namespace;

	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Generated_code_matches_what_is_checked_in(string name)
	{
		var text = File.ReadAllText(Path.Combine(Directory, name + ".gram"));

		foreach (var (options, declarations) in Renderings(name, text))
		{
			var result = GramCompiler.Compile(text, options);

			EmittedCode.Quiet(result.Diagnostics);

			var sources = result.Sources;

			Assert.NotEmpty(sources);

			// Compiled first, and the parts of a split file together, since they are one
			// compilation where they land: a snapshot that matches but does not build is worse
			// than no snapshot, because it makes the wrong thing look approved. A grammar that
			// names what the host declares is compiled beside those declarations, as it is in a
			// consumer’s project.
			EmittedCode.Compile(
				sources[0].Text, options.ClassName, options.Namespace, declarations,
				sources.Skip(1).Select(static source => source.Text),
				symbols: Symbols(options), utf8Literals: options.Utf8Literals);

			foreach (var source in sources)
				Held(source.HintName, source.Text);
		}
	}

	/// <summary>The symbols a rendering is compiled with: those its options write text behind.</summary>
	static string[]? Symbols(GramCompilerOptions options)
	{
		var symbols = new List<string>();

		if (options.Checks)
			symbols.Add("DOTGRAM_CHECKS");

		if (options.Steps)
			symbols.Add("DOTGRAM_STEPS");

		return symbols.Count == 0 ? null : symbols.ToArray();
	}

	/// <summary>
	/// The file a source is checked in as, written where there is none and compared where
	/// there is.
	/// </summary>
	/// <remarks>
	/// Named after the hint name the generator gave it, which is what a consumer’s build calls
	/// it: <c>Name.gram.g.cs</c> for one file, <c>Name.Suffix.gram.g.cs</c> for a second
	/// rendering of the same grammar, and <c>…part-0001.g.cs</c> for what a split file adds.
	/// So a grammar that produces several files is snapshot in several, without the harness
	/// inventing a naming of its own.
	/// </remarks>
	static void Held(string hint, string actual)
	{
		var expected = Path.Combine(Directory, hint);

		if (!File.Exists(expected))
		{
			File.WriteAllText(expected, actual, CSharpFile);

			Assert.Fail(
				$"No snapshot for '{hint}'; wrote one to {expected}. Read it, and commit it if it is right.");
		}

		if (Normalize(File.ReadAllText(expected)) == Normalize(actual))
			return;

		// The failure message cannot show a file this size usefully, so the answer goes
		// next to the question and the diff is taken in the editor.
		var rejected = expected + ".actual";

		File.WriteAllText(rejected, actual, CSharpFile);

		Assert.Fail($"Generated code differs from the snapshot. Diff {expected} against {rejected}.");
	}

	/// <summary>What a grammar here is compiled with (<see cref="SnapshotRenderings"/>).</summary>
	internal static IEnumerable<(GramCompilerOptions Options, string? Declarations)> Renderings(string name, string text)
	{
		return SnapshotRenderings.Of(name, text, EmittedCode.References);
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

	static string Normalize(string text)
	{
		return text.Replace("\r\n", "\n").TrimEnd();
	}

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
	internal static string Directory => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
