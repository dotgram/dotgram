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
	const string Namespace = "DotGram.Snapshots";

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
				sources.Skip(1).Select(static source => source.Text));

			foreach (var source in sources)
				Held(source.HintName, source.Text);
		}
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

	/// <summary>
	/// What a grammar here is compiled with: the defaults, unless the grammar is one of the few
	/// that exist to hold a path the defaults do not reach.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Until this was written every snapshot was taken with four options — the class name, the
	/// namespace, the scanner and the line map — and everything else at its default. So the
	/// buffered reader, the byte machine, the lexical half and a second rendering of one grammar
	/// could not appear in any diff, however many grammars were added: they are not a property of
	/// a grammar but of what it is compiled with. Two changes to emitted code went unseen that
	/// way in one day.
	/// </para>
	/// <para>
	/// **A line per path, not a matrix.** Each of these grammars is small and holds one thing;
	/// there is no sweep over the options, because a set that runs for minutes stops being read,
	/// and a snapshot nobody reads is the same defect wearing a test’s coat.
	/// </para>
	/// <para>
	/// What comes back is the options and, where the grammar names what the host declares,
	/// those declarations: they are compiled beside the generated file, as they are in a
	/// consumer’s project, and a real symbol resolver is built over them.
	/// </para>
	/// </remarks>
	static IEnumerable<(GramCompilerOptions Options, string? Declarations)> Renderings(string name, string text)
	{
		GramCompilerOptions Options() => new()
		{
			ClassName = name,
			Namespace = Namespace,

			// A grammar here may hand C# across, and an inline `@(...)` needs the
			// scanner to find where it ends.
			CSharpScanner = RoslynCSharpScanner.Instance,

			// The file name alone, not where this checkout happens to be: a snapshot
			// holding an absolute path would differ on every machine that read it.
			LineMap = new GrammarLineMap(text, name + ".gram"),
		};

		switch (name)
		{
			// The reader over buffered input, over characters and over bytes, with a retention
			// bound small enough to be read in the file rather than taken on trust.
			case "Buffered":
			{
				var buffered = Options();

				buffered.BufferedInput = true;
				buffered.BufferedBytes = true;
				buffered.MaxRetained   = 4096;

				yield return (buffered, null);

				break;
			}

			// One grammar rendered twice, which is what the expression language ships and what
			// the stand measures in every paired run: the second carries a suffix of its own and
			// builds where it reads.
			case "Twice":
			{
				yield return (Options(), null);

				var immediate = Options();

				immediate.Suffix         = "Immediate";
				immediate.SuffixDeclared = true;
				immediate.Carrier        = CarrierKind.Immediate;

				yield return (immediate, null);

				break;
			}

			// Where a value was, and a carrier the author asked for rather than `Auto` chose — both
			// of them the host's to ask for. The declarations beside it are the consumer's own
			// types, so this is also the snapshot compiled under a real symbol resolver: what a
			// grammar names is looked up in a compilation rather than taken on trust.
			case "Located":
			{
				var located = Options();

				located.LocationType   = "ILocated";
				located.Carrier        = CarrierKind.Tape;
				located.SymbolResolver = new RoslynSymbolResolver(
					CSharpCompilation.Create(
						name,
						[CSharpSyntaxTree.ParseText($"namespace {Namespace} {{ public partial class {name} {{ {Declarations} }} }}")],
						EmittedCode.References,
						new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
					$"{Namespace}.{name}");

				yield return (located, Declarations);

				break;
			}

			// One grammar in several files, and the size that divides them. A part is a complete
			// group of its own, so a consumer whose compiler chokes on one enormous file gets
			// several and nothing else changes; the division is a wish and never an error
			// (`SourceFileSize`, and `PartSize` for how finely a recognizer is cut inside a file).
			// The number here is small on purpose: the point is that the file divides and that the
			// parts still compile as one compilation, which is how they land.
			case "Split":
			{
				var split = Options();

				split.SourceFileSize = 20000;
				split.PartSize       = 400;

				yield return (split, null);

				break;
			}

			// Read over kinds rather than characters: the tokenizer, the kept cutting and the
			// scanners of the seam are all in the file only here.
			case "Lexical":
			{
				var lexical = Options();

				lexical.Lexical = true;

				yield return (lexical, null);

				// And the same again with the transition tables off, which is the chain of
				// alternatives they replace: the tables are on by default and not offered in the
				// attribute, so nothing else in this set can show what they are instead of.
				var chained = Options();

				chained.Lexical      = true;
				chained.PrefixTables = false;
				chained.Suffix       = "Chained";
				chained.SuffixDeclared = true;

				yield return (chained, null);

				break;
			}

			default:
				yield return (Options(), null);

				break;
		}
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

	/// <summary>
	/// What the host of <c>Located.gram</c> declares: the interface a reader offers a range to,
	/// and the two types the grammar builds.
	/// </summary>
	/// <remarks>
	/// Written at the C# 8 floor, like everything else this harness compiles — no records, no
	/// primary constructors — because the declaration is parsed at the floor beside the generated
	/// file.
	/// </remarks>
	const string Declarations = """
		public interface ILocated
		{
			void Locate(int at, int length);
		}

		public sealed class Place : ILocated
		{
			public Place(string name) { Name = name; }

			public string Name { get; }
			public int At { get; private set; }
			public int Length { get; private set; }

			public void Locate(int at, int length) { At = at; Length = length; }
		}

		public sealed class Entry : ILocated
		{
			public Entry(Place key, string value) { Key = key; Value = value; }

			public Place Key { get; }
			public string Value { get; }
			public int At { get; private set; }
			public int Length { get; private set; }

			public void Locate(int at, int length) { At = at; Length = length; }
		}
		""";

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
