using System;
using System.Collections.Generic;

using DotGram.Grammar.Emit;

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
		//   GramLexer        .Tokenize  (text)            -> TokenList
		//   GramParser       .Parse     (tokens)          -> SyntaxTree
		//   GrammarBinder    .Bind      (tree, symbols)   -> GrammarModel
		//   GrammarNormalizer.Normalize (model)           -> RecognitionGraph
		//   CSharpEmitter    .Emit      (graph)           -> GeneratedSource[]
		//
		// None of them exists yet; this method is the shape they will be wired into.

		return new GramCompilation(sources, diagnostics);
	}

	/// <summary>
	/// Emits the support types a generated parser needs. Independent of any grammar —
	/// one copy per compilation, not per grammar.
	/// </summary>
	public static GeneratedSource EmitSupportTypes(SupportAccessibility accessibility) =>
		new("DotGram.Support.g.cs", SupportEmitter.SupportTypes(accessibility));

	/// <summary>
	/// Emits the marker attributes (<c>[Gram]</c>, <c>[assembly: GramRuntime]</c>).
	/// Always internal, always present, in every compilation.
	/// </summary>
	public static GeneratedSource EmitMarkerAttributes() =>
		new("DotGram.Attributes.g.cs", SupportEmitter.Attributes);
}
