using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>Where a navigation goes.</summary>
abstract record GramLocation
{
	GramLocation()
	{
	}

	/// <summary>A position of the document navigated from.</summary>
	public sealed record InDocument(int Position) : GramLocation;

	/// <summary>A place in a file of the solution, zero-based.</summary>
	public sealed record InFile(string Path, int Line, int Column) : GramLocation;

	/// <summary>A place in an included grammar, opened where the host's store says, zero-based.</summary>
	public sealed record InIncludedGrammar(StandaloneIncludedGrammar Grammar, int Line, int Column) : GramLocation;

	/// <summary>A rule of the standard library, opened as an included grammar is, zero-based.</summary>
	public sealed record InStandardLibrary(string Rule, int Line, int Column) : GramLocation
	{
		public StandaloneIncludedGrammar Grammar { get; } = new(StandardLibrary.Name, StandardLibrary.Text, null);
	}

	/// <summary>
	/// A C# name in an expression of the grammar, wherever the editor's own navigation takes it: the
	/// symbols to try in turn, or null where Roslyn found nothing.
	/// </summary>
	public sealed record HostSymbol(string Expression, int Position, RoslynDefinitionTargets? Targets) : GramLocation;

	/// <summary>The method a <c>parse</c> or <c>find</c> publishes, which the generator writes: the symbols to try in turn, or null.</summary>
	public sealed record PublishedMethod(string MethodName, RoslynDefinitionTargets? Targets) : GramLocation;
}

/// <summary>A span that navigates when clicked, and where to; a null target claims the span and goes nowhere.</summary>
readonly record struct GramNavigable(TextSpan Span, GramLocation? Target);

/// <summary>A place an occurrence of a symbol is highlighted, and whether it is the definition.</summary>
readonly record struct GramHighlight(TextSpan Span, bool IsDefinition);

/// <summary>A rule, capture or parameter at a position, its definition, and every use of it.</summary>
sealed class GramReferences(
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

	/// <summary>Whether the definition is outside this document, in an included grammar.</summary>
	public bool DefinedElsewhere { get; } = definedElsewhere;

	/// <summary>
	/// Whether renaming in this document alone renames the symbol: not where an included grammar
	/// declares or uses it, which this document cannot edit.
	/// </summary>
	public bool Renamable => !DefinedElsewhere && Elsewhere.Count == 0;
}

/// <summary>Go To Definition, Ctrl+click, Find All References and reference highlighting, in a grammar.</summary>
static class GramNavigationProvider
{
	/// <summary>The symbol at a position of a standalone document and its occurrences; null where there is none.</summary>
	public static GramReferences? References(GramStandaloneView view, int position)
	{
		var symbols = view.Document.Symbols;
		var current = symbols.FirstOrDefault(symbol =>
			symbol.Position <= position && position < symbol.Position + symbol.Length);

		return current.Length == 0
			? null
			: new GramReferences(
				current.Name,
				current.DefinitionPosition,
				symbols
					.Where(symbol => symbol.Name == current.Name &&
						symbol.DefinitionPosition == current.DefinitionPosition)
					.Select(symbol => symbol.Position)
					.ToArray(),
				view.IncludedReferences?.Invoke(current.Name, current.DefinitionPosition) ?? [],
				current.DefinitionPosition >= view.Text.Length);
	}

	/// <summary>The symbol at a position of a C# document's grammars and its occurrences in that grammar; null where there is none.</summary>
	public static GramReferences? References(EmbeddedGrammarView view, int position)
	{
		if (!view.Current)
			return null;

		var symbols = view.Symbols;
		var current = symbols.FirstOrDefault(symbol => symbol.Span.Contains(position));

		return current.Span.Length == 0
			? null
			: new GramReferences(
				current.Name,
				current.DefinitionSpan.Start,
				symbols
					.Where(symbol => symbol.Name == current.Name &&
						symbol.GrammarSpan == current.GrammarSpan &&
						symbol.DefinitionSpan == current.DefinitionSpan)
					.Select(symbol => symbol.Span.Start)
					.ToArray());
	}

	/// <summary>
	/// The references in this document and in the grammars its host includes, as one list over
	/// several files. A carried grammar whose copy could not be made has nowhere to be listed at.
	/// </summary>
	public static CSharpFindReferences Across(GramReferences found, string filePath, string text, IIncludedGrammarStore store)
	{
		var references = new List<CSharpFindReference>(found.Positions.Length + found.Elsewhere.Count);

		foreach (var position in found.Positions)
			references.Add(new CSharpFindReference(filePath, text, position, found.Name.Length));

		foreach (var reference in found.Elsewhere)
			if (store.PathToOpen(reference.Location.Grammar) is { } path)
				references.Add(new CSharpFindReference(
					path,
					reference.Location.Grammar.Text,
					reference.Location.Offset,
					reference.Length));

		return new CSharpFindReferences(found.Name, references);
	}

	/// <summary>Every occurrence of the symbol at a position of a standalone document, the definition marked.</summary>
	public static IReadOnlyList<GramHighlight> Highlights(GramStandaloneView view, int position)
	{
		var symbols = view.Document.Symbols;
		var current = symbols.FirstOrDefault(symbol =>
			symbol.Position <= position && position < symbol.Position + symbol.Length);

		if (current.Length == 0)
			return [];

		return symbols
			.Where(symbol => symbol.Name == current.Name && symbol.DefinitionPosition == current.DefinitionPosition)
			.Select(static symbol => new GramHighlight(new TextSpan(symbol.Position, symbol.Length), symbol.IsDefinition))
			.ToArray();
	}

	/// <summary>Every occurrence of the symbol at a position of a C# document's grammar, the definition marked.</summary>
	public static IReadOnlyList<GramHighlight> Highlights(EmbeddedGrammarView view, int position)
	{
		if (!view.Current)
			return [];

		var symbols = view.Symbols;
		var current = symbols.FirstOrDefault(symbol => symbol.Span.Contains(position));

		if (current.Span.Length == 0)
			return [];

		return symbols
			.Where(symbol => symbol.Name == current.Name &&
				symbol.GrammarSpan == current.GrammarSpan &&
				symbol.DefinitionSpan == current.DefinitionSpan)
			.Select(static symbol => new GramHighlight(symbol.Span, symbol.IsDefinition))
			.ToArray();
	}

	/// <summary>What a click at a position of a standalone document navigates to.</summary>
	public static GramNavigable? Navigable(GramStandaloneView view, int position)
	{
		if (view.ExternalDefinition?.Invoke(position) is { } external)
			return new GramNavigable(
				new TextSpan(external.Position, external.Length),
				new GramLocation.InIncludedGrammar(external.Target.Grammar, external.Target.Line, external.Target.Column));

		// A definition past the end of the document is in an included grammar that could not be
		// placed; there is nowhere in this document to go.
		foreach (var item in view.Document.Symbols)
			if (item.Position <= position && position < item.Position + item.Length)
				return item.DefinitionPosition >= view.Text.Length
					? null
					: new GramNavigable(new TextSpan(item.Position, item.Length), new GramLocation.InDocument(item.DefinitionPosition));

		return null;
	}

	/// <summary>What a click at a position of a C# document navigates to.</summary>
	public static async Task<GramNavigable?> NavigableAsync(
		string text,
		EmbeddedGrammarView view,
		int position,
		RoslynGramCompletion? roslyn,
		CancellationToken cancellationToken)
	{
		var grammarFile = roslyn is null
			? null
			: await roslyn.GrammarFileSourceAsync(position, cancellationToken).ConfigureAwait(false);
		if (grammarFile is { } file)
			return new GramNavigable(
				new TextSpan(position, position < text.Length ? 1 : 0),
				new GramLocation.InFile(file.FilePath, file.Line, file.Column));

		if (!view.Current)
			return null;

		foreach (var item in view.DslSymbols)
			if (item.Span.Contains(position) && item.DefinitionPath is not null)
				return new GramNavigable(
					item.Span,
					new GramLocation.InFile(item.DefinitionPath, item.DefinitionLine, item.DefinitionColumn));

		foreach (var item in view.Symbols)
			if (item.Span.Contains(position))
				return new GramNavigable(item.Span, new GramLocation.InDocument(item.DefinitionSpan.Start));

		// Returning nothing here lets an editor's own C# navigation treat the containing string
		// literal as one large navigable symbol. Claim positions inside an embedded grammar even
		// while its semantic spans are being refreshed after an edit.
		if (view.Classifications.Any(item => item.GrammarSpan.Contains(position)))
			return new GramNavigable(new TextSpan(position, position < text.Length ? 1 : 0), null);

		return null;
	}

	/// <summary>Whether Go To Definition has anything to do at a position of a standalone document.</summary>
	public static bool CanGoToDefinition(GramStandaloneView view, int position)
	{
		return PublishedApi(view.Document.PublishedApis, position) is not null ||
			CSharpContext(view.Text, position, inGrammar: true) is not null ||
			References(view, position) is not null ||
			StandardRuleAt(view.Text, GrammarPosition.Standalone(view.Text, position), position) is not null;
	}

	/// <summary>Where Go To Definition goes from a position of a standalone document; null where nowhere.</summary>
	public static async Task<GramLocation?> DefinitionAsync(
		GramStandaloneView view,
		int position,
		RoslynGramCompletion? roslyn,
		CancellationToken cancellationToken)
	{
		var text = view.Text;

		if (PublishedApi(view.Document.PublishedApis, position) is { } publishedApi)
			return new GramLocation.PublishedMethod(publishedApi, roslyn is null
				? null
				: await roslyn.DefinitionTargetsAsync(publishedApi, 0, cancellationToken).ConfigureAwait(false));

		if (StandardRuleAt(text, GrammarPosition.Standalone(text, position), position) is { } standard)
			return Standard(standard);

		if (CSharpContext(text, position, inGrammar: true) is { } csharp)
			return new GramLocation.HostSymbol(csharp.Expression, csharp.Position, roslyn is null
				? null
				: await roslyn.DefinitionTargetsAsync(csharp.Expression, csharp.Position, cancellationToken).ConfigureAwait(false));

		if (view.ExternalDefinition?.Invoke(position) is { } external)
			return new GramLocation.InIncludedGrammar(external.Target.Grammar, external.Target.Line, external.Target.Column);

		var found = References(view, position);

		return found is null || found.DefinedElsewhere ? null : new GramLocation.InDocument(found.DefinitionPosition);
	}

	/// <summary>Where Go To Definition goes from a position of a C# document; null where nowhere, and then the editor's own C# definition is not asked either.</summary>
	public static async Task<GramLocation?> DefinitionAsync(
		string text,
		EmbeddedGrammarView view,
		int position,
		RoslynGramCompletion? roslyn,
		CancellationToken cancellationToken)
	{
		if (view.Current && view.DslSymbols.FirstOrDefault(item => item.Span.Contains(position)) is { DefinitionPath: { } path } symbol)
			return new GramLocation.InFile(path, symbol.DefinitionLine, symbol.DefinitionColumn);

		if (roslyn is not null)
		{
			if (await roslyn.GrammarFileSourceAsync(position, cancellationToken).ConfigureAwait(false) is { } file)
				return new GramLocation.InFile(file.FilePath, file.Line, file.Column);

			if (await roslyn.GeneratedApiSourceAsync(position, cancellationToken).ConfigureAwait(false) is { } generated)
				return new GramLocation.InFile(generated.FilePath, generated.Line, generated.Column);
		}

		var publications = view.Current ? view.PublishedApis : [];

		if (publications.FirstOrDefault(item => item.Span.Contains(position)).MethodName is { } publishedApi)
			return new GramLocation.PublishedMethod(publishedApi, roslyn is null
				? null
				: await roslyn.DefinitionTargetsAsync(publishedApi, 0, cancellationToken).ConfigureAwait(false));

		var grammar = view.Current ? GrammarPosition.Embedded(text, position, view.Classifications) : null;

		if (StandardRuleAt(text, grammar, position) is { } standard)
			return Standard(standard);

		var inGrammar = view.Current && view.Classifications.Any(item => item.GrammarSpan.Contains(position));

		if (CSharpContext(text, position, inGrammar) is { } csharp)
			return new GramLocation.HostSymbol(csharp.Expression, csharp.Position, roslyn is null
				? null
				: await roslyn.DefinitionTargetsAsync(csharp.Expression, csharp.Position, cancellationToken).ConfigureAwait(false));

		var found = References(view, position);

		return found is null || found.DefinedElsewhere ? null : new GramLocation.InDocument(found.DefinitionPosition);
	}

	static GramLocation Standard(StandardRule rule)
	{
		return new GramLocation.InStandardLibrary(rule.Name, rule.Line, rule.Column);
	}

	static string? PublishedApi(IReadOnlyList<GramPublishedApi> publications, int position)
	{
		return publications
			.FirstOrDefault(item => position >= item.Position && position < item.Position + item.Length)
			.MethodName;
	}

	/// <summary>The C# expression a position is in, and the position's place in it; null outside C#.</summary>
	static (string Expression, int Position)? CSharpContext(string text, int position, bool inGrammar)
	{
		if (!inGrammar ||
			!GramCSharpCompletionContext.TryGetExpression(text, position, out var expression, out var expressionStart, out _, out _))
			return null;

		return (expression, position - expressionStart);
	}

	/// <summary>
	/// The standard library's rule under the position, written <c>Std.Name</c> in a grammar's own text
	/// and not in C# inside it. Only found here: the read-only copy it opens in is made when the
	/// navigation happens, not when an editor asks whether it can.
	/// </summary>
	static StandardRule? StandardRuleAt(string text, (string Text, int Position)? grammar, int position)
	{
		if (!GrammarPosition.InGrammarText(grammar))
			return null;

		var start = position;
		var end   = position;

		while (start > 0 && IsNameCharacter(text[start - 1]))
			start--;

		while (end < text.Length && IsNameCharacter(text[end]))
			end++;

		if (start == end)
			return null;

		var from = Math.Max(0, start - StandardLibrary.Name.Length - 2);

		return GramVocabulary.QualifiedStandardRule(text.Substring(from, end - from), start - from, end - start);
	}

	static bool IsNameCharacter(char character)
	{
		return char.IsLetterOrDigit(character) || character == '_';
	}
}

/// <summary>One replacement in a document.</summary>
readonly record struct GramTextEdit(TextSpan Span, string NewText);

/// <summary>What renaming a symbol does: the edits in this document, or why it is refused.</summary>
/// <param name="Edits">Every occurrence replaced; empty where refused or where the name stays the same.</param>
/// <param name="Conflicts">Why the rename cannot be made from here; empty where it can.</param>
sealed record GramRename(IReadOnlyList<GramTextEdit> Edits, IReadOnlyList<string> Conflicts);

/// <summary>Rename of a rule, capture or parameter, within the document it is in.</summary>
static class GramRenameProvider
{
	/// <summary>Why the symbol cannot be renamed from here, or null where it can.</summary>
	/// <remarks>
	/// A rule an included grammar declares or uses is renamed there too, which this document
	/// cannot do; renaming half of it is worse than not renaming it.
	/// </remarks>
	public static string? Refusal(GramReferences found)
	{
		return found.Renamable
			? null
			: $"'{found.Name}' is declared or used in an included grammar and cannot be renamed from here.";
	}

	/// <summary>The edits that rename the symbol to <paramref name="replacement"/>.</summary>
	public static GramRename Rename(GramReferences found, string replacement)
	{
		if (Refusal(found) is { } refusal)
			return new GramRename([], [refusal]);

		if (replacement == found.Name)
			return new GramRename([], []);

		return new GramRename(
			found.Positions.Select(position => new GramTextEdit(new TextSpan(position, found.Name.Length), replacement)).ToArray(),
			[]);
	}
}
