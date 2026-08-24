using System.Linq;
using Microsoft.VisualStudio.Text;

namespace DotGram.VisualStudio;

sealed class GramFindReferencesTarget(string name, int definitionPosition, int[] positions)
{
	public string Name { get; } = name;
	public int DefinitionPosition { get; } = definitionPosition;
	public int[] Positions { get; } = positions;

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
				symbols.Where(symbol => symbol.Name == current.Name).Select(symbol => symbol.Position).ToArray());
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
