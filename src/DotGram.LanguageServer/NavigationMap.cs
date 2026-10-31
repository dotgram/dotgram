using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;

namespace DotGram.LanguageServer;

/// <summary>Definition, references and highlights as the protocol carries them.</summary>
/// <remarks>
/// An included grammar that has no file of its own is opened as the store's read-only copy, under
/// a <c>file:</c> URI, as Visual Studio opens it; a C# name goes where Roslyn found it in source,
/// and nowhere when it is only in metadata.
/// </remarks>
static class NavigationMap
{
	public static async Task<Location?> DefinitionAsync(
		GramLocation location,
		string uri,
		GramStandaloneView view,
		LineMap lines,
		IIncludedGrammarStore store,
		CancellationToken cancellationToken)
	{
		switch (location)
		{
			case GramLocation.InDocument target:
				var definition = view.Document.Symbols.FirstOrDefault(symbol => symbol.Position == target.Position && symbol.IsDefinition);

				return new Location(uri, lines.Range(target.Position, definition.Length));

			case GramLocation.InFile target:
				return new Location(FileUri(target.Path), Point(target.Line, target.Column));

			case GramLocation.InIncludedGrammar target:
				return Included(store, target.Grammar, target.Line, target.Column);

			case GramLocation.InStandardLibrary target:
				return Included(store, target.Grammar, target.Line, target.Column);

			case GramLocation.HostSymbol target:
				return await SourceAsync(target.Targets, cancellationToken).ConfigureAwait(false);

			case GramLocation.PublishedMethod target:
				return await SourceAsync(target.Targets, cancellationToken).ConfigureAwait(false);

			default:
				return null;
		}
	}

	/// <summary>
	/// A place in an included grammar, given as Roslyn counts lines — which also breaks them at U+0085,
	/// U+2028 and U+2029 — told as the protocol counts them.
	/// </summary>
	static Location? Included(IIncludedGrammarStore store, StandaloneIncludedGrammar grammar, int line, int column)
	{
		if (store.PathToOpen(grammar) is not { } path)
			return null;

		var lines  = grammar.Lines.Lines;
		var offset = line < lines.Count ? Math.Min(lines[line].Start + column, grammar.Text.Length) : grammar.Text.Length;

		return new Location(FileUri(path), new LineMap(grammar.Lines).Range(offset, 0));
	}

	/// <summary>What a C# name binds to, or else the first of its declarations, where it is in source.</summary>
	static async Task<Location?> SourceAsync(RoslynDefinitionTargets? targets, CancellationToken cancellationToken)
	{
		if (targets is null)
			return null;

		if (targets.Bound is { } bound && Source(bound) is { } location)
			return location;

		if (targets.DeclarationsAsync is null)
			return null;

		foreach (var symbol in await targets.DeclarationsAsync(cancellationToken).ConfigureAwait(false))
			if (Source(symbol) is { } declared)
				return declared;

		return null;
	}

	static Location? Source(ISymbol symbol)
	{
		foreach (var location in symbol.Locations)
		{
			if (!location.IsInSource || location.SourceTree?.FilePath is not { Length: > 0 } path)
				continue;

			var lines = new LineMap(location.SourceTree.GetText());

			return new Location(FileUri(path), lines.Range(location.SourceSpan.Start, location.SourceSpan.Length));
		}

		return null;
	}

	/// <summary>Every occurrence of the symbol, in this document and in the grammars the host includes.</summary>
	public static Location[] References(GramReferences found, bool includeDeclaration, string uri, LineMap lines, IIncludedGrammarStore store)
	{
		var result = new List<Location>(found.Positions.Length + found.Elsewhere.Count);

		foreach (var position in found.Positions)
			if (includeDeclaration || position != found.DefinitionPosition)
				result.Add(new Location(uri, lines.Range(position, found.Name.Length)));

		foreach (var reference in found.Elsewhere)
		{
			if (!includeDeclaration && reference.IsDefinition)
				continue;

			if (store.PathToOpen(reference.Location.Grammar) is not { } path)
				continue;

			var grammar = new LineMap(reference.Location.Grammar.Text);

			result.Add(new Location(FileUri(path), grammar.Range(reference.Location.Offset, reference.Length)));
		}

		return [.. result];
	}

	public static DocumentHighlight[] Highlights(IReadOnlyList<GramHighlight> highlights, LineMap lines)
	{
		return highlights
			.Select(highlight => new DocumentHighlight(
				lines.Range(highlight.Span.Start, highlight.Span.Length),
				highlight.IsDefinition ? Lsp.HighlightWrite : Lsp.HighlightRead))
			.ToArray();
	}

	static Range Point(int line, int column)
	{
		var position = new Position(line, column);

		return new Range(position, position);
	}

	public static string FileUri(string path)
	{
		return FileUris.ToUri(path);
	}
}
