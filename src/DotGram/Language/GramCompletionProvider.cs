using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>One completion item, as an editor lists it.</summary>
/// <param name="Label">What the list shows, and what the description is looked up by.</param>
/// <param name="Description">What the editor shows beside the selected item.</param>
/// <param name="InsertText">What is inserted; null for an item that is its label in every respect.</param>
/// <param name="Suffix">What the list shows after the label: a rule's parameters, Roslyn's inline description.</param>
/// <param name="SortText">What the list sorts by; the label where null.</param>
/// <param name="FilterText">What typing filters by; the label where null.</param>
/// <param name="FromCSharp">Whether Roslyn offered the item, for the C# around a grammar.</param>
sealed record GramCompletionItem(
	string Label,
	string Description,
	string? InsertText = null,
	string Suffix = "",
	string? SortText = null,
	string? FilterText = null,
	bool FromCSharp = false)
{
	/// <summary>An item that is its label in every respect, which an editor makes with nothing but the label.</summary>
	public bool Plain => InsertText is null;
}

/// <summary>
/// What completion offers at a position of a grammar: its rules and words, the standard library after
/// <c>Std.</c>, the names a construction is handed and the captures in scope inside C#, and a DSL's
/// expected literals; the rest of the C# is Roslyn's, asked through the editor.
/// </summary>
/// <remarks>One per document kind: a standalone <c>.gram</c> file, and grammars embedded in a C# file.</remarks>
abstract class GramCompletionProvider
{
	/// <summary>Where completion at the position would replace text, or null where it offers nothing.</summary>
	public TextSpan? Start(string text, int position)
	{
		if (!IsApplicable(text, position))
			return null;

		return WordSpan(text, position);
	}

	/// <summary>The items at a position.</summary>
	/// <param name="text">The document.</param>
	/// <param name="position">Where completion was asked for.</param>
	/// <param name="applicable">What <see cref="Start"/> answered.</param>
	/// <param name="csharp">Roslyn's items for a C# expression prefix; null where there is no Roslyn to ask.</param>
	/// <param name="cancellationToken">Cancels the request.</param>
	public async Task<IReadOnlyList<GramCompletionItem>> ItemsAsync(
		string text,
		int position,
		TextSpan applicable,
		Func<string, CancellationToken, Task<IReadOnlyList<GramCompletionItem>>>? csharp,
		CancellationToken cancellationToken)
	{
		var dslItems = DslCompletions(position);
		if (dslItems.Count > 0)
			return dslItems.Select(DslCompletion).ToArray();

		var grammar = GrammarAt(text, position);

		// C# where the grammar's own lexer says so; the text before the caret only says what to ask Roslyn.
		if (GramCSharpCompletionContext.TryGetPrefix(text, position, out var prefix) &&
			(grammar is not { } lexed || GramVocabulary.InCSharp(lexed.Text, lexed.Position)))
		{
			var csharpItems = csharp is null
				? []
				: await csharp(prefix, cancellationToken).ConfigureAwait(false);
			var csharpNames = new HashSet<string>(csharpItems.Select(static item => item.Label), StringComparer.Ordinal);

			// What the parser supplies, and the captures and parameters in scope, are no part of
			// the C# Roslyn is asked about, so it never offers them. Which of them the C# here is
			// handed depends on whether it is a construction's or a recovery's.
			if (grammar is not { } at || !GramVocabulary.SuppliedNamesApply(text, position, prefix))
				return csharpItems;

			var action = GramVocabulary.ActionAt(at.Text, at.Position, out _);
			var local  = action is GramAction.Construction or GramAction.Guard
				? Definitions(text, position)
					.Where(static pair => pair.Value.IsLocal)
					.Select(static pair => (pair.Key, pair.Value.Description))
				: [];
			var extra = GramVocabulary.SuppliedAt(at.Text, at.Position)
				.Select(static name => (Key: name, Description: GramVocabulary.Supplied(name)!))
				.Concat(local)
				.Where(pair => !csharpNames.Contains(pair.Key))
				.GroupBy(static pair => pair.Key, StringComparer.Ordinal)
				.Select(static group => group.Last())
				.ToArray();

			return [.. csharpItems, .. extra.Select(static pair => new GramCompletionItem(pair.Key, pair.Description))];
		}

		// `Std.` in the grammar itself; in C# above, `Std` is whatever C# says it is.
		if (grammar is { } qualified && GrammarPosition.InGrammarText(grammar) && GramVocabulary.IsStandardQualifier(
			qualified.Text, qualified.Position - (position - applicable.Start)))
			return StandardCompletions();

		var definitions = Definitions(text, position);
		var words = grammar is { } arguments && GramVocabulary.InCallArguments(arguments.Text, arguments.Position)
			? GramVocabulary.CompletionWords.Concat(GramVocabulary.SuppliedAt(arguments.Text, arguments.Position))
			: GramVocabulary.CompletionWords;
		var names = definitions.Keys.Concat(words).Distinct(StringComparer.Ordinal).OrderBy(static name => name);
		var items = new List<GramCompletionItem>();

		foreach (var name in names)
		{
			var definition  = definitions.TryGetValue(name, out var found) ? found : default;
			var description = definition.Description is not null
				? definition.Description
				: GramVocabulary.DescribeWord(name);

			items.Add(definition.Signature is null
				? new GramCompletionItem(name, description)
				: new GramCompletionItem(
					name,
					description,
					definition.ParameterCount > 0 ? name + "(" : name,
					definition.Signature.Substring(name.Length),
					name,
					name));
		}

		return items;
	}

	static IReadOnlyList<GramCompletionItem> StandardCompletions()
	{
		var items = new List<GramCompletionItem>();

		foreach (var rule in GramVocabulary.StandardRules.Values.OrderBy(static rule => rule.Name, StringComparer.Ordinal))
		{
			var signature = rule.Signature.Substring(GramLibraryPrefix.Length + rule.Name.Length);

			items.Add(new GramCompletionItem(
				rule.Name,
				rule.Description,
				rule.ParameterCount > 0 ? rule.Name + "(" : rule.Name,
				signature,
				rule.Name,
				rule.Name));
		}

		return items;
	}

	static string GramLibraryPrefix { get; } = StandardLibrary.Name + ".";

	/// <summary>The grammar the position stands in, and the position's place in it; null outside a grammar.</summary>
	protected abstract (string Text, int Position)? GrammarAt(string text, int position);

	protected abstract bool IsApplicable(string text, int position);

	protected abstract IReadOnlyDictionary<string, RuleCompletion> Definitions(string text, int position);

	protected virtual IReadOnlyList<DslLiteralCompletion> DslCompletions(int position)
	{
		return [];
	}

	static GramCompletionItem DslCompletion(DslLiteralCompletion suggestion)
	{
		return new GramCompletionItem(
			suggestion.Insertion,
			$"DotGram expected literal: {suggestion.Display}",
			suggestion.Insertion,
			"",
			suggestion.Insertion,
			suggestion.Insertion);
	}

	protected readonly struct RuleCompletion(string description, string signature, int parameterCount, bool isLocal = false)
	{
		public string Description { get; } = description;
		public string Signature { get; } = signature;
		public int ParameterCount { get; } = parameterCount;

		/// <summary>A capture or a parameter, which C# inside the rule may name, rather than a rule.</summary>
		public bool IsLocal { get; } = isLocal;
	}

	protected static RuleCompletion LocalCompletion(string name, GramSymbolKind kind)
	{
		return new(
			kind == GramSymbolKind.Parameter
				? $"{name}: DotGram rule parameter"
				: $"{name}: DotGram capture",
			name,
			0,
			isLocal: true);
	}

	static TextSpan WordSpan(string text, int position)
	{
		var start = position;
		var end   = position;

		while (start > 0 && IsNameCharacter(text[start - 1])) start--;
		while (end < text.Length && IsNameCharacter(text[end])) end++;

		return new TextSpan(start, end - start);
	}

	static bool IsNameCharacter(char character)
	{
		return char.IsLetterOrDigit(character) || character == '_';
	}
}

/// <summary>Completion in a standalone <c>.gram</c> document.</summary>
sealed class StandaloneGramCompletionProvider(GramDocument document) : GramCompletionProvider
{
	protected override bool IsApplicable(string text, int position)
	{
		return true;
	}

	protected override (string Text, int Position)? GrammarAt(string text, int position)
	{
		return GrammarPosition.Standalone(text, position);
	}

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(string text, int position)
	{
		var definitions = document.Classifications
			.Where(static item => item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionPosition == item.Position && item.QuickInfo is not null)
			.GroupBy(item => text.Substring(item.Position, item.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		foreach (var symbol in document.Symbols
			.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
				symbol.IsDefinition &&
				symbol.ScopeStart <= position && position < symbol.ScopeEnd)
			.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
		{
			var local = symbol.OrderBy(static item => item.Kind).First();
			definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
		}

		return definitions;
	}
}

/// <summary>Completion in the grammars and DSL literals of a C# document.</summary>
sealed class EmbeddedGramCompletionProvider(EmbeddedGrammarView view) : GramCompletionProvider
{
	protected override bool IsApplicable(string text, int position)
	{
		return view.Current && view.Classifications.Any(item => item.GrammarSpan.Contains(position)) ||
			view.TryGetDslCompletions(position, out var expected) &&
			LiteralCompletions(expected).Count > 0;
	}

	protected override (string Text, int Position)? GrammarAt(string text, int position)
	{
		return view.Current
			? GrammarPosition.Embedded(text, position, view.Classifications)
			: null;
	}

	protected override IReadOnlyList<DslLiteralCompletion> DslCompletions(int position)
	{
		return view.TryGetDslCompletions(position, out var expected)
			? LiteralCompletions(expected)
			: [];
	}

	static IReadOnlyList<DslLiteralCompletion> LiteralCompletions(IReadOnlyList<string> expected)
	{
		return expected
		.SelectMany(DslLiteralCompletionParser.ParseAll)
		.Distinct()
		.ToArray();
	}

	protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(string text, int position)
	{
		if (!view.Current)
			return new Dictionary<string, RuleCompletion>();

		var definitions = view.Classifications
			.Where(item =>
				item.GrammarSpan.Contains(position) &&
				item.SymbolKind == GramSymbolKind.Rule &&
				item.DefinitionSpan == item.Span &&
				item.QuickInfo is not null)
			.GroupBy(item => text.Substring(item.Span.Start, item.Span.Length), StringComparer.Ordinal)
			.ToDictionary(
				group => group.Key,
				group => new RuleCompletion(
					group.First().QuickInfo!,
					group.First().RuleSignature!,
					group.First().RuleParameterCount),
				StringComparer.Ordinal);

		foreach (var symbol in view.Symbols
			.Where(symbol => symbol.Kind != GramSymbolKind.Rule &&
				symbol.IsDefinition &&
				symbol.GrammarSpan.Contains(position) &&
				symbol.ScopeSpan.Contains(position))
			.GroupBy(symbol => symbol.Name, StringComparer.Ordinal))
		{
			var local = symbol.OrderBy(static item => item.Kind).First();
			definitions[local.Name] = LocalCompletion(local.Name, local.Kind);
		}

		return definitions;
	}
}

internal readonly record struct DslLiteralCompletion(string Display, string Insertion);

internal static class DslLiteralCompletionParser
{
	public static IReadOnlyList<DslLiteralCompletion> ParseAll(string expected)
	{
		if (Parse(expected) is { } literal)
			return [literal];
		if (expected.Length < 2 || expected[0] != '[' || expected[expected.Length - 1] != ']')
			return [];

		var parts = expected.Substring(1, expected.Length - 2).Split(
			new[] { " | " },
			StringSplitOptions.None);
		var result = parts.Select(Parse).ToArray();
		return result.All(static item => item is not null)
			? result.Select(static item => item!.Value).ToArray()
			: [];
	}

	public static DslLiteralCompletion? Parse(string expected)
	{
		var literal = expected.EndsWith("i", StringComparison.Ordinal)
			? expected.Substring(0, expected.Length - 1)
			: expected;
		if (literal.Length >= 2 && literal[0] == '"' && literal[literal.Length - 1] == '"')
			return new DslLiteralCompletion(expected, literal.Substring(1, literal.Length - 2));
		if (literal.Length < 3 || literal[0] != '\'' || literal[literal.Length - 1] != '\'')
			return null;

		var body = literal.Substring(1, literal.Length - 2);
		var insertion = body switch
		{
			"\\n"  => "\n",
			"\\r"  => "\r",
			"\\t"  => "\t",
			"\\'"  => "'",
			"\\\\" => "\\",
			_ when body.Length == 1 => body,
			_ => null,
		};
		return insertion is null ? null : new DslLiteralCompletion(expected, insertion);
	}
}
