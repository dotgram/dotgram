using System;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What <see cref="CSharpEmitter.Emit"/> is handed beside the options: what the compiler worked
/// out about the grammar before emission, and the collections emission adds to.
/// </summary>
/// <remarks>
/// The options say what the host asked for and travel unchanged from the compiler to here; this
/// is what is not an option — a result of an earlier stage, or a place to put what the file
/// produces besides its text.
/// </remarks>
sealed class EmitContext
{
	/// <summary>
	/// The split the graph is the syntactic half of, where it is one. Then the file carries
	/// a lexical machine as well, the seam is skipped rather than woven, and a position is a
	/// token — so the text a value is cut from travels beside the kinds rather than being what
	/// is read (docs/design/lexical-adt-design.md).
	/// </summary>
	public LexicalSplit? Lexical { get; init; }

	/// <summary>
	/// Where what emission finds to say is added: the carriers, the memo, a method too large
	/// to optimize. Null drops it.
	/// </summary>
	public ICollection<GramDiagnostic>? Diagnostics { get; init; }

	/// <summary>The grammar's text to write onto the class as its source, or null to write none.</summary>
	public string? GrammarSource { get; init; }

	/// <summary>
	/// Where the parts of a file divided by <c>SourceFileSize</c> are added; null keeps the
	/// file whole.
	/// </summary>
	public ICollection<string>? SourceParts { get; init; }

	/// <summary>Where the carriers report is written, line by line; null writes none.</summary>
	public ICollection<string>? Carriers { get; init; }

	/// <summary>
	/// The choices that read a nested instance of their own recursion, as the analysis found
	/// them: what the readers remember successes for, and what is told about each.
	/// </summary>
	public IReadOnlyList<SharedRecursion.Pair>? SharedChoices { get; init; }
}
