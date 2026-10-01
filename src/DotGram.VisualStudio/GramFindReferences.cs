using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Text;

namespace DotGram.VisualStudio;

sealed class GramFindReferencesTarget(
	string name,
	int definitionPosition,
	int[] positions,
	IReadOnlyList<StandaloneReference>? elsewhere = null,
	bool definedElsewhere = false)
{
	public string Name { get; } = name;
	public int DefinitionPosition { get; } = definitionPosition;
	public int[] Positions { get; } = positions;

	/// <summary>Uses, and the definition, in the grammars the host includes.</summary>
	public IReadOnlyList<StandaloneReference> Elsewhere { get; } = elsewhere ?? [];

	/// <summary>Whether the definition is outside this buffer, in an included grammar.</summary>
	public bool DefinedElsewhere { get; } = definedElsewhere;

	/// <summary>
	/// Whether renaming in this buffer alone renames the symbol: not where an included grammar
	/// declares or uses it, which this buffer cannot edit.
	/// </summary>
	public bool Renamable => !DefinedElsewhere && Elsewhere.Count == 0;

	public static GramFindReferencesTarget? Standalone(
		ITextSnapshot snapshot,
		int position,
		GramBufferAnalysis analysis)
	{
		var symbols = analysis.Document(snapshot).Symbols;
		var current = symbols.FirstOrDefault(symbol =>
			symbol.Position <= position && position < symbol.Position + symbol.Length);

		return current.Length == 0
			? null
			: new GramFindReferencesTarget(
				current.Name,
				current.DefinitionPosition,
				symbols
					.Where(symbol => symbol.Name == current.Name &&
						symbol.DefinitionPosition == current.DefinitionPosition)
					.Select(symbol => symbol.Position)
					.ToArray(),
				analysis.IncludedReferences(snapshot, current.Name, current.DefinitionPosition),
				current.DefinitionPosition >= snapshot.Length);
	}

	public static GramFindReferencesTarget? Embedded(
		ITextSnapshot snapshot,
		int position,
		EmbeddedGrammarBufferAnalysis analysis)
	{
		if (!analysis.TryGetSymbols(snapshot, out var symbols))
			return null;

		var current = symbols.FirstOrDefault(symbol => symbol.Span.Contains(position));

		return current.Span.Length == 0
			? null
			: new GramFindReferencesTarget(
				current.Name,
				current.DefinitionSpan.Start,
				symbols
					.Where(symbol => symbol.Name == current.Name &&
						symbol.GrammarSpan == current.GrammarSpan &&
						symbol.DefinitionSpan == current.DefinitionSpan)
					.Select(symbol => symbol.Span.Start)
					.ToArray());
	}
}
