using System;
using System.Collections.Generic;

using DotGram.Language;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>
/// One grammar of a C# document and what in it can be navigated to, as the analysis of that
/// grammar gave it: the symbols stay with the grammar they came from.
/// </summary>
/// <param name="Name">What the user knows the grammar by (<see cref="EmbeddedGrammar.Name"/>); null where nothing names it.</param>
/// <param name="Span">The string the grammar is written in.</param>
/// <param name="HostSpan">The declaration the grammar belongs to (<see cref="EmbeddedGrammar.HostSpan"/>).</param>
sealed record EmbeddedGrammarOutline(
	string?                           Name,
	TextSpan                          Span,
	TextSpan                          HostSpan,
	IReadOnlyList<HostDocumentSymbol> Symbols);

/// <summary>An entry of the right-hand list: a rule or another symbol of one grammar.</summary>
sealed record EmbeddedNavigationItem(
	string Display,
	int    Position,
	int    Length,
	int    SelectionPosition,
	int    SelectionLength);

/// <summary>An entry of the left-hand list: a grammar, with the entries the right-hand list shows for it.</summary>
sealed record EmbeddedGrammarItem(
	string                                Display,
	TextSpan                              Span,
	TextSpan                              HostSpan,
	IReadOnlyList<EmbeddedNavigationItem> Symbols);

/// <summary>
/// The two lists of the navigation bar over a C# document with grammars, apart from the bar:
/// nothing here is a Visual Studio type, so that what is listed and what is chosen at the caret
/// can be tested without one.
/// </summary>
static class EmbeddedNavigationItems
{
	/// <summary>
	/// The grammars, sorted by name as C# sorts its types. A name two grammars share, or none at
	/// all, has the line the grammar starts on after it, so that each entry says which one it is.
	/// </summary>
	/// <param name="lineAt">The one-based line number of a position in the document.</param>
	public static IReadOnlyList<EmbeddedGrammarItem> Grammars(
		IReadOnlyList<EmbeddedGrammarOutline> outlines,
		Func<int, int>                        lineAt)
	{
		var counts = new Dictionary<string, int>(StringComparer.Ordinal);

		foreach (var outline in outlines)
			if (outline.Name is { } name)
				counts[name] = counts.TryGetValue(name, out var count) ? count + 1 : 1;

		var result = new List<EmbeddedGrammarItem>(outlines.Count);

		foreach (var outline in outlines)
		{
			var display = outline.Name is { } name && counts[name] == 1
				? name
				: (outline.Name ?? "Grammar") + " (line " + lineAt(outline.Span.Start) + ")";

			result.Add(new EmbeddedGrammarItem(display, outline.Span, outline.HostSpan, Symbols(outline.Symbols)));
		}

		result.Sort(static (left, right) =>
		{
			var byName = StringComparer.OrdinalIgnoreCase.Compare(left.Display, right.Display);
			return byName != 0 ? byName : left.Span.Start.CompareTo(right.Span.Start);
		});

		return result;
	}

	/// <summary>A grammar's symbols, nested ones beside their parents, sorted by name.</summary>
	public static IReadOnlyList<EmbeddedNavigationItem> Symbols(IReadOnlyList<HostDocumentSymbol> symbols)
	{
		var result = new List<EmbeddedNavigationItem>();

		Append(symbols, result);
		result.Sort(static (left, right) =>
		{
			var byName = StringComparer.OrdinalIgnoreCase.Compare(left.Display, right.Display);
			return byName != 0 ? byName : left.Position.CompareTo(right.Position);
		});

		return result;
	}

	/// <summary>
	/// The index of the grammar the left-hand list shows at <paramref name="position"/>, or -1 where
	/// there is none. As C#'s type list does, it always shows one when the document has any: the
	/// grammar whose string holds the position; else the innermost whose declaration does (a caret
	/// on the class or its attributes); else the nearest one before the position; else the first.
	/// </summary>
	public static int GrammarAt(IReadOnlyList<EmbeddedGrammarItem> grammars, int position)
	{
		var inString = Innermost(grammars, position, static grammar => grammar.Span);
		if (inString >= 0)
			return inString;

		var inHost = Innermost(grammars, position, static grammar => grammar.HostSpan);
		if (inHost >= 0)
			return inHost;

		var before = -1;
		var first  = -1;

		for (var index = 0; index < grammars.Count; index++)
		{
			var host = grammars[index].HostSpan;

			if (host.End <= position &&
				(before < 0 || host.End > grammars[before].HostSpan.End ||
				 host.End == grammars[before].HostSpan.End && host.Start > grammars[before].HostSpan.Start))
				before = index;

			if (first < 0 || grammars[index].Span.Start < grammars[first].Span.Start)
				first = index;
		}

		return before >= 0 ? before : first;
	}

	/// <summary>
	/// The index of the innermost symbol whose span holds <paramref name="position"/>, or -1 where
	/// none does: outside every rule, or outside the grammar.
	/// </summary>
	public static int SymbolAt(IReadOnlyList<EmbeddedNavigationItem> items, int position)
	{
		var selected = -1;

		for (var index = 0; index < items.Count; index++)
		{
			var item = items[index];

			if (item.Position <= position && position < item.Position + item.Length &&
				(selected < 0 || item.Length < items[selected].Length))
				selected = index;
		}

		return selected;
	}

	/// <summary>The innermost grammar a span of which holds the position; the earlier one in the document where two are as small.</summary>
	static int Innermost(IReadOnlyList<EmbeddedGrammarItem> grammars, int position, Func<EmbeddedGrammarItem, TextSpan> spanOf)
	{
		var selected = -1;

		for (var index = 0; index < grammars.Count; index++)
		{
			var span = spanOf(grammars[index]);
			if (span.Start > position || position >= span.End)
				continue;

			if (selected < 0)
			{
				selected = index;
				continue;
			}

			var best = spanOf(grammars[selected]);
			if (span.Length < best.Length ||
				span.Length == best.Length && grammars[index].Span.Start < grammars[selected].Span.Start)
				selected = index;
		}

		return selected;
	}

	static void Append(IReadOnlyList<HostDocumentSymbol> symbols, List<EmbeddedNavigationItem> result)
	{
		foreach (var symbol in symbols)
		{
			result.Add(new EmbeddedNavigationItem(
				symbol.Name.TrimStart(' ', '\t'),
				symbol.Span.Start,
				symbol.Span.Length,
				symbol.SelectionSpan.Start,
				symbol.SelectionSpan.Length));
			Append(symbol.Children, result);
		}
	}
}
