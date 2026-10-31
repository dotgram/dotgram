using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Language;

namespace DotGram.LanguageServer;

/// <summary>Folding and the outline as <c>textDocument/foldingRange</c> and <c>textDocument/documentSymbol</c> carry them.</summary>
static class StructureMap
{
	/// <summary>The ranges that span lines: the protocol folds whole lines, and one that ends where it starts folds nothing.</summary>
	public static FoldingRange[] Folding(IReadOnlyList<GramFoldingRange> ranges, LineMap lines)
	{
		var result = new List<FoldingRange>(ranges.Count);

		foreach (var range in ranges)
		{
			var start = lines.Position(range.Position);
			var end   = lines.Position(range.Position + range.Length);

			if (end.Line > start.Line)
				result.Add(new FoldingRange(start.Line, start.Character, end.Line, end.Character, range.CollapsedText));
		}

		return [.. result];
	}

	public static DocumentSymbol[] Symbols(IReadOnlyList<GramDocumentSymbol> symbols, LineMap lines)
	{
		return symbols.Select(symbol => Symbol(symbol, lines)).ToArray();
	}

	static DocumentSymbol Symbol(GramDocumentSymbol symbol, LineMap lines)
	{
		var start          = symbol.Position;
		var end            = symbol.Position + symbol.Length;
		var selectionStart = Math.Max(start, Math.Min(symbol.SelectionPosition, end));
		var selectionEnd   = Math.Max(selectionStart, Math.Min(symbol.SelectionPosition + symbol.SelectionLength, end));

		// A client refuses an outline whose selection is not inside its range.
		return new DocumentSymbol(
			symbol.Name,
			symbol.Kind switch
			{
				GramDocumentSymbolKind.Namespace   => Lsp.SymbolNamespace,
				GramDocumentSymbolKind.Publication => Lsp.SymbolMethod,
				_                                  => Lsp.SymbolFunction,
			},
			lines.Range(start, end - start),
			lines.Range(selectionStart, selectionEnd - selectionStart),
			Symbols(symbol.Children, lines));
	}
}
