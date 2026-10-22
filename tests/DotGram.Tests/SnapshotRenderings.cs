using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Tests;

/// <summary>
/// The options each snapshot grammar is compiled with, and what its host declares.
/// </summary>
/// <remarks>
/// A class of its own, compiled into the determinism tests as well, so that the generator run on
/// another runtime is asked for the same files the snapshots hold. It needs nothing of the test
/// framework: the references a declaration is compiled against are handed in.
/// </remarks>
static class SnapshotRenderings
{
	public const string Namespace = "DotGram.Snapshots";

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
	public static IEnumerable<(GramCompilerOptions Options, string? Declarations)> Of(
		string name, string text, ImmutableArray<MetadataReference> references)
	{
		GramCompilerOptions Options()
		{
			return new()
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
		}

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
				// The tape, named, as the expression language names it: the generator would carry
				// both readings immediately otherwise, and there would be one reading twice.
				var tape = Options();

				tape.Carrier = CarrierKind.Tape;

				yield return (tape, null);

				var immediate = Options();

				immediate.Suffix         = "Immediate";
				immediate.SuffixDeclared = true;
				immediate.Carrier        = CarrierKind.Immediate;

				yield return (immediate, null);

				// And a third, traced (docs/syntax.md, §6.9): what a trace build adds beside the same
				// parser — the sink, its scope, the tables and the sinks — and what it writes into the
				// readers, every return of a rule reporting it, read against the file above it.
				var traced = Options();

				traced.Suffix  = "Traced";
				traced.Trace   = true;
				traced.Carrier = CarrierKind.Tape;

				yield return (traced, null);

				break;
			}

			// The engine traced (docs/syntax.md, §6.9): what a trace build writes into the automaton —
			// a call entered where its entry is pushed, left at `Return:`, failed or taken back where
			// `Fail:` pops it, a scanned rule around its scanner — read against the file above it.
			case "Url":
			{
				yield return (Options(), null);

				var traced = Options();

				traced.Suffix = "Traced";
				traced.Trace  = true;

				yield return (traced, null);

				// And with the engine's step trace written in, which a parser compiled without
				// DOTGRAM_STEPS carries no text of: the calls at every step and the methods they call.
				var stepped = Options();

				stepped.Suffix = "Stepped";
				stepped.Steps  = true;

				yield return (stepped, null);

				break;
			}

			// Where a value was, and a carrier the author asked for rather than `Auto` chose — both
			// of them the host's to ask for. The declarations beside it are the consumer's own
			// types, so this is also the snapshot compiled under a real symbol resolver: what a
			// grammar names is looked up in a compilation rather than taken on trust.
			// The materializer's fast path, which no snapshot reached until this grammar existed:
			// a guard at every level of a tower builds the record where it stands rather than
			// walking the log. The carrier is named rather than left to `Auto`, which reads this
			// language as one that need not replay and gives it the immediate carrier — and an
			// immediate reader has no walk to skip. The path belongs to the tape, so the tape is
			// what it is compiled for, as `Located` names its carrier for the same kind of reason.
			case "Tower":
			{
				var tower = Options();

				tower.Carrier = CarrierKind.Tape;

				yield return (tower, null);

				// And with the walk's invariants written in, which a parser compiled without
				// DOTGRAM_CHECKS carries no text of: this is the one place their text is read, and
				// it is compiled with the symbol defined.
				var checks = Options();

				checks.Carrier = CarrierKind.Tape;
				checks.Checks  = true;
				checks.Suffix  = "Checked";

				yield return (checks, null);

				break;
			}

			case "Located":
			{
				var located = Options();

				located.LocationType   = "ILocated";
				located.Carrier        = CarrierKind.Tape;
				located.SymbolResolver = new RoslynSymbolResolver(
					CSharpCompilation.Create(
						name,
						[CSharpSyntaxTree.ParseText($"namespace {Namespace} {{ public partial class {name} {{ {Declarations} }} }}")],
						references,
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

				// And for a consumer whose C# is 11 or later, where the transition table is a UTF-8
				// literal rather than an array of every cell; the default is the C# 8 floor's form.
				var utf8 = Options();

				utf8.Lexical        = true;
				utf8.Utf8Literals   = true;
				utf8.Suffix         = "Utf8";
				utf8.SuffixDeclared = true;

				yield return (utf8, null);

				break;
			}

			// Read over kinds, with terminals that bind text: the lexer begins each and its
			// own rule measures the token from its start, which only this file shows.
			case "Begun":
			{
				var begun = Options();

				begun.Lexical = true;

				// The tape, named: the syntax rereads an item's first token across its
				// alternatives, which the generator would otherwise say a word about.
				begun.Carrier = CarrierKind.Tape;

				yield return (begun, null);

				break;
			}

			default:
				yield return (Options(), null);

				break;
		}
	}

	/// <summary>
	/// What the host of <c>Located.gram</c> declares: the interface a reader offers a range to,
	/// and the two types the grammar builds.
	/// </summary>
	/// <remarks>
	/// Written at the C# 8 floor, like everything else this harness compiles — no records, no
	/// primary constructors — because the declaration is parsed at the floor beside the generated
	/// file.
	/// </remarks>
	public const string Declarations = """
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
}
