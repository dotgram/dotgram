using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>
/// Compiles grammar text into C# source. This is the whole grammar half: everything a
/// caller needs, in one call, with no Roslyn involved.
/// </summary>
/// <remarks>
/// The source generator is a shell over this — it reads <c>.gram</c> files out of the
/// compilation, calls <see cref="Compile"/>, and hands the results back to Roslyn.
/// Anything else that wants to compile a grammar (a test, a CLI, a playground) calls
/// the same method and gets the same answer, which is the point: generated code can be
/// obtained as a string and inspected as code.
/// </remarks>
public static class GramCompiler
{
	/// <summary>Compiles one grammar.</summary>
	/// <param name="grammarText">Contents of a <c>.gram</c> file.</param>
	/// <param name="options">Compilation options; defaults are used when null.</param>
	public static GramCompilation Compile(string grammarText, GramCompilerOptions? options = null)
	{
		if (grammarText is null)
			throw new ArgumentNullException(nameof(grammarText));

		options ??= new GramCompilerOptions();

		var sources     = new List<GeneratedSource>();
		var diagnostics = new List<GramDiagnostic>();

		// The pipeline, each stage its own type with its own contract so it can be
		// exercised and diffed on its own:
		//
		//   GramLexer        .Tokenize  (text)          -> TokenList
		//   GramParser       .Parse     (tokens)        -> ParseResult
		//   GrammarBinder    .Bind      (file, symbols) -> GrammarModel
		//   GrammarNormalizer.Normalize (model)         -> RecognitionGraph
		//   CSharpEmitter    .Emit      (graph)         -> C#

		var parsed = GramParser.Parse(GramLexer.Tokenize(grammarText, options.CSharpScanner));

		diagnostics.AddRange(parsed.Diagnostics);

		var model = GrammarBinder.Bind(parsed.File, options.SymbolResolver);
		var graph = GrammarNormalizer.Normalize(model, options.SymbolResolver);

		// What the later stages made of a declaration whose syntax did not come out is
		// about a tree that was guessed at, not about the grammar. Dropped rather than
		// reported, and only for the declarations that actually broke — a rule that is
		// wrong should still be told about when the one above it is unreadable
		// (implementation.md §0).
		var broken = Broken(parsed);

		foreach (var diagnostic in model.Diagnostics)
			if (!Inside(broken, diagnostic))
				diagnostics.Add(diagnostic);

		foreach (var diagnostic in graph.Diagnostics)
			if (!Inside(broken, diagnostic))
				diagnostics.Add(diagnostic);

		// What the grammar asked for and cannot have, said where the asking is (§6.3).
		// After normalization because it is a question about the graph, and only when the
		// grammar is otherwise sound: telling an author what a broken rule will not get is
		// answering a question they are not asking yet.
		if (!HasErrors(diagnostics))
		{
			diagnostics.AddRange(Retention.Check(graph));
			diagnostics.AddRange(FirstSets.Check(graph));
		}

		// Every stage runs even after an earlier one failed — a grammar with one bad rule
		// should still report what is wrong with the other twelve (implementation.md §0).
		// Only emission is skipped, because code built from a broken grammar would bury
		// the real message under compiler errors in the consumer's build.
		if (!HasErrors(diagnostics))
			sources.Add(new GeneratedSource(
				$"{options.ClassName}.gram.g.cs",
				CSharpEmitter.Emit(graph, options.ClassName, options.Namespace, options.LineMap)));

		return new GramCompilation(sources, OnePerPosition(diagnostics));
	}

	/// <summary>
	/// One error per position, in the order they were raised.
	/// </summary>
	/// <remarks>
	/// The second thing said about a place is the first stage's failure told again by the
	/// next one — <c>Unexpected character '~'</c> and then <c>Expected a rule, a scope or
	/// a publication directive</c>, both pointing at the same character. The earlier one
	/// is the one that says what actually happened; the later one describes the wreckage.
	/// Warnings and information are left alone: they are about the grammar rather than
	/// about what went wrong reading it, and two of them in one place can both be true.
	/// </remarks>
	static List<GramDiagnostic> OnePerPosition(List<GramDiagnostic> diagnostics)
	{
		var kept = new List<GramDiagnostic>(diagnostics.Count);
		var seen = new HashSet<int>();

		foreach (var diagnostic in diagnostics)
			if (diagnostic.Severity != GramSeverity.Error || seen.Add(diagnostic.Position))
				kept.Add(diagnostic);

		return kept;
	}

	/// <summary>
	/// The extent of every declaration the parser could not read whole.
	/// </summary>
	/// <remarks>
	/// A syntax error leaves the tree holding a guess, and everything a later stage says
	/// about that guess is about the guess. The declaration is the unit because it is the
	/// one the parser recovers at: what is around it was read normally and still deserves
	/// to be checked.
	/// </remarks>
	static List<(int From, int To)> Broken(ParseResult parsed)
	{
		var starts = new List<int>();

		foreach (var declaration in parsed.File.Decls)
			starts.Add(declaration.At.Position);

		starts.Sort();

		var extents = new List<(int, int)>();

		foreach (var diagnostic in parsed.Diagnostics)
		{
			if (diagnostic.Severity != GramSeverity.Error)
				continue;

			// From the start of the declaration the error is in, because a later stage
			// reports against the declaration rather than against the character; and up to
			// the next one, because that is where the parser found its feet again. A
			// declaration whose own start was never read leaves the extent open at the
			// front, which is the safe direction.
			var from = 0;
			var to   = int.MaxValue;

			foreach (var start in starts)
				if (start <= diagnostic.Position)
					from = start;
				else
				{
					to = start;

					break;
				}

			extents.Add((from, to));
		}

		return extents;
	}

	static bool Inside(List<(int From, int To)> extents, GramDiagnostic diagnostic)
	{
		foreach (var (from, to) in extents)
			if (from <= diagnostic.Position && diagnostic.Position < to)
				return true;

		return false;
	}

	static bool HasErrors(List<GramDiagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
			if (diagnostic.Severity == GramSeverity.Error)
				return true;

		return false;
	}

	/// <summary>
	/// Emits <c>[Gram]</c>, which is the whole of what a compilation gets before any grammar
	/// in it has been looked at. Always internal, always present, one copy per compilation.
	/// <c>SourceSpan</c> used to be here beside it and is emitted into each host class
	/// instead, where it can be public without two assemblies colliding over it.
	/// </summary>
	public static GeneratedSource EmitMarkerAttributes() =>
		new("DotGram.Attributes.g.cs", SupportEmitter.Attributes);
}
