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

		diagnostics.AddRange(model.Diagnostics);

		var graph = GrammarNormalizer.Normalize(model, options.SymbolResolver);

		diagnostics.AddRange(graph.Diagnostics);

		// What the grammar asked for and cannot have, said where the asking is (§6.3).
		// After normalization because it is a question about the graph, and only when the
		// grammar is otherwise sound: telling an author what a broken rule will not get is
		// answering a question they are not asking yet.
		if (!HasErrors(diagnostics))
			diagnostics.AddRange(Retention.Check(graph));

		// Every stage runs even after an earlier one failed — a grammar with one bad rule
		// should still report what is wrong with the other twelve (implementation.md §0).
		// Only emission is skipped, because code built from a broken grammar would bury
		// the real message under compiler errors in the consumer's build.
		if (!HasErrors(diagnostics))
			sources.Add(new GeneratedSource(
				$"{options.ClassName}.gram.g.cs",
				CSharpEmitter.Emit(graph, options.ClassName, options.Namespace)));

		return new GramCompilation(sources, diagnostics);
	}

	static bool HasErrors(List<GramDiagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
			if (diagnostic.Severity == GramSeverity.Error)
				return true;

		return false;
	}

	/// <summary>
	/// Emits <c>[Gram]</c> and the one support type. Always internal, always present, one
	/// copy per compilation and independent of any grammar in it.
	/// </summary>
	public static GeneratedSource EmitMarkerAttributes() =>
		new("DotGram.Attributes.g.cs", SupportEmitter.Attributes);
}
