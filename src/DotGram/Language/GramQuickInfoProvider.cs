using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>What a Quick Info is about.</summary>
enum GramQuickInfoKind
{
	/// <summary>A token of the grammar: its rule's definition, or what the word or symbol means.</summary>
	Grammar,

	/// <summary>C# inside the grammar, as Roslyn describes it.</summary>
	CSharp,

	/// <summary>A name in a DSL literal, and what it stands for.</summary>
	DslSymbol,

	/// <summary>A DSL literal, and the language and rule it is read with.</summary>
	DslSite,

	/// <summary>A place inside an embedded grammar with nothing to say, where the C# string's own Quick Info would mislead.</summary>
	Suppressed,
}

/// <summary>The picture beside a Quick Info.</summary>
enum GramQuickInfoIcon
{
	None,

	/// <summary>A rule's definition.</summary>
	Method,

	/// <summary>A name with no definition of its own.</summary>
	LocalVariable,

	/// <summary>A word or symbol of the notation.</summary>
	Keyword,

	/// <summary>What Roslyn says.</summary>
	LightBulb,
}

/// <summary>How a run of Quick Info text is coloured.</summary>
enum GramQuickInfoStyle
{
	/// <summary>In the tooltip's own colour, unclassified.</summary>
	Plain,

	/// <summary>Classified as plain text.</summary>
	Text,

	/// <summary>Coloured as the grammar's token of <see cref="GramQuickInfoRun.Kind"/>.</summary>
	Syntax,

	/// <summary>Coloured as Roslyn's text tag <see cref="GramQuickInfoRun.Tag"/>.</summary>
	CSharp,
}

/// <summary>A run of Quick Info text and how it is coloured.</summary>
readonly record struct GramQuickInfoRun(string Text, GramQuickInfoStyle Style, GramSyntaxKind Kind = default, string? Tag = null);

/// <summary>One Quick Info, as an editor shows it.</summary>
/// <param name="Kind">What it is about.</param>
/// <param name="Span">The text it is about.</param>
/// <param name="Icon">The picture beside it.</param>
/// <param name="Sections">The text shown at once, one list of runs per paragraph.</param>
/// <param name="Details">
/// The referenced rules, shown on request: one list of runs per line. Empty where there are none.
/// </param>
/// <param name="Lines">Plain lines, for the DSL kinds.</param>
sealed record GramQuickInfo(
	GramQuickInfoKind Kind,
	TextSpan Span,
	GramQuickInfoIcon Icon,
	IReadOnlyList<IReadOnlyList<GramQuickInfoRun>> Sections,
	IReadOnlyList<IReadOnlyList<GramQuickInfoRun>> Details,
	IReadOnlyList<string> Lines);

/// <summary>What Quick Info shows at a position: an item, and the messages of the squiggles there, which go in front of it.</summary>
sealed record GramQuickInfoAnswer(GramQuickInfo? Item, IReadOnlyList<string> Diagnostics);

/// <summary>Quick Info for a position of a grammar, in a <c>.gram</c> file or embedded in C#.</summary>
static class GramQuickInfoProvider
{
	const string ReferencedRule     = "Referenced rule:";
	const string RecursiveReference = "Recursive reference:";

	/// <summary>Quick Info in a standalone <c>.gram</c> document.</summary>
	/// <param name="view">The document.</param>
	/// <param name="position">The position asked about.</param>
	/// <param name="csharp">Roslyn's Quick Info for a C# expression and a position in it; null where there is no Roslyn to ask.</param>
	/// <param name="cancellationToken">Cancels the request.</param>
	public static async Task<GramQuickInfoAnswer> StandaloneAsync(
		GramStandaloneView view,
		int position,
		Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? csharp,
		CancellationToken cancellationToken)
	{
		var text        = view.Text;
		var item        = await StandaloneItemAsync(view, position, csharp, cancellationToken).ConfigureAwait(false);
		var diagnostics = GramDiagnosticText.At(
			view.Document.Diagnostics.Select(static diagnostic =>
				(diagnostic.Position, diagnostic.Length, diagnostic)),
			position,
			text.Length);

		return new GramQuickInfoAnswer(item, diagnostics);
	}

	static async Task<GramQuickInfo?> StandaloneItemAsync(
		GramStandaloneView view,
		int position,
		Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? csharp,
		CancellationToken cancellationToken)
	{
		var text = view.Text;

		if (GramCSharpCompletionContext.TryGetExpression(
			text, position,
			out var expression, out var expressionStart, out var symbolStart, out var symbolLength) &&
			csharp is not null)
		{
			var described = await csharp(expression, position - expressionStart, cancellationToken).ConfigureAwait(false);
			if (described is not null)
				return CSharp(symbolStart, symbolLength, described);
		}

		foreach (var item in view.Document.Classifications)
			if (Contains(item.Position, item.Length, position))
				return Grammar(
					text,
					item.Position,
					item.Length,
					item.Kind,
					item.QuickInfo ?? StandardQuickInfo(
						text, item.Position, item.Length, item.Kind, GrammarPosition.Standalone(text, item.Position)));

		return null;
	}

	/// <summary>Quick Info in a C# document with grammars and DSL literals in it; null while the document's analysis is not ready.</summary>
	public static async Task<GramQuickInfoAnswer?> EmbeddedAsync(
		string text,
		EmbeddedGrammarView view,
		int position,
		Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? csharp,
		CancellationToken cancellationToken)
	{
		if (!view.Current)
			return null;

		var item     = await EmbeddedItemAsync(text, view, position, csharp, cancellationToken).ConfigureAwait(false);
		var messages = GramDiagnosticText.At(
			view.Diagnostics.Select(static diagnostic =>
				(diagnostic.Span.Start, diagnostic.Span.Length, diagnostic.Diagnostic)),
			position,
			text.Length);

		return new GramQuickInfoAnswer(item, messages);
	}

	static async Task<GramQuickInfo?> EmbeddedItemAsync(
		string text,
		EmbeddedGrammarView view,
		int position,
		Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? csharp,
		CancellationToken cancellationToken)
	{
		var classifications = view.Classifications;

		if (view.DslSymbols
				.Where(symbol => symbol.Span.Contains(position))
				.OrderBy(symbol => symbol.Target.IndexOf('.') < 0)
				.ThenBy(symbol => symbol.Span.Length)
				.FirstOrDefault() is { Span.Length: > 0 } dslSymbol)
			return new GramQuickInfo(
				GramQuickInfoKind.DslSymbol,
				dslSymbol.Span,
				GramQuickInfoIcon.None,
				[],
				[],
				[$"{dslSymbol.Role}: {dslSymbol.Target}"]);

		foreach (var site in view.DslSites)
			if (site.Span.Contains(position))
				return new GramQuickInfo(
					GramQuickInfoKind.DslSite,
					site.Span,
					GramQuickInfoIcon.None,
					[],
					[],
					[$"DotGram language: {site.LanguageId}", $"Entry rule: {site.EntryRule}"]);

		if (classifications.Any(item => item.GrammarSpan.Contains(position)) &&
			GramCSharpCompletionContext.TryGetExpression(
				text, position,
				out var expression, out var expressionStart, out var symbolStart, out var symbolLength) &&
			csharp is not null)
		{
			var described = await csharp(expression, position - expressionStart, cancellationToken).ConfigureAwait(false);
			if (described is not null)
				return CSharp(symbolStart, symbolLength, described);
		}

		foreach (var item in classifications)
			if (item.Span.Contains(position))
				return Grammar(
					text,
					item.Span.Start,
					item.Span.Length,
					item.Kind,
					item.QuickInfo ?? StandardQuickInfo(
						text, item.Span.Start, item.Span.Length, item.Kind,
						GrammarPosition.Embedded(text, item.Span.Start, classifications)));

		foreach (var item in classifications)
			if (item.GrammarSpan.Contains(position))
				return new GramQuickInfo(GramQuickInfoKind.Suppressed, new TextSpan(position, 0), GramQuickInfoIcon.None, [], [], []);

		return null;
	}

	/// <summary>
	/// The definition of the standard library's rule a name written <c>Std.Name</c> ends in, which
	/// the analysis of the document itself does not carry: the library is no part of its text. Not
	/// in C# inside the grammar, where <c>Std</c> is whatever C# says it is.
	/// </summary>
	internal static string? StandardQuickInfo(
		string text, int position, int length, GramSyntaxKind kind, (string Text, int Position)? grammar)
	{
		if (kind != GramSyntaxKind.Identifier || !GrammarPosition.InGrammarText(grammar))
			return null;

		var start = Math.Max(0, position - StandardLibrary.Name.Length - 2);
		var shown = text.Substring(start, position + length - start);

		return GramVocabulary.QualifiedStandardRule(shown, position - start, length)?.Description;
	}

	/// <summary>A grammar token's Quick Info: its definition where it has one, else what it is, the referenced rules held back.</summary>
	internal static GramQuickInfo Grammar(string text, int position, int length, GramSyntaxKind kind, string? quickInfo)
	{
		var isDefinition = quickInfo is not null;
		var described    = quickInfo ?? GramVocabulary.Describe(kind, text.Substring(position, length));
		var referenced   = described.IndexOf("\n\n" + ReferencedRule, StringComparison.Ordinal);
		var recursive    = described.IndexOf("\n\n" + RecursiveReference, StringComparison.Ordinal);
		var split = referenced < 0
			? recursive
			: recursive < 0 ? referenced : Math.Min(referenced, recursive);
		var summary = split < 0 ? described : described.Substring(0, split);
		var icon = isDefinition
			? GramQuickInfoIcon.Method
			: kind == GramSyntaxKind.Identifier ? GramQuickInfoIcon.LocalVariable : GramQuickInfoIcon.Keyword;

		return new GramQuickInfo(
			GramQuickInfoKind.Grammar,
			new TextSpan(position, length),
			icon,
			[Classified(summary, kind, isDefinition)],
			split < 0 ? [] : Details(described.Substring(split).TrimStart()),
			[]);
	}

	static GramQuickInfo CSharp(int position, int length, RoslynGramQuickInfo quickInfo)
	{
		return new GramQuickInfo(
			GramQuickInfoKind.CSharp,
			new TextSpan(position, length),
			GramQuickInfoIcon.LightBulb,
			quickInfo.Sections
				.Select(static section => (IReadOnlyList<GramQuickInfoRun>)section
					.Select(static part => new GramQuickInfoRun(part.Text, GramQuickInfoStyle.CSharp, Tag: part.Tag))
					.ToArray())
				.ToArray(),
			[],
			[]);
	}

	/// <summary>The referenced rules, a line each: their headings plain, their definitions coloured as grammar.</summary>
	static IReadOnlyList<IReadOnlyList<GramQuickInfoRun>> Details(string text)
	{
		var lines  = text.Replace("\r", string.Empty).Split('\n');
		var result = new List<IReadOnlyList<GramQuickInfoRun>>(lines.Length);

		foreach (var line in lines)
			if (line.StartsWith(ReferencedRule, StringComparison.Ordinal) ||
				line.StartsWith(RecursiveReference, StringComparison.Ordinal))
				result.Add([new GramQuickInfoRun(line, GramQuickInfoStyle.Plain)]);
			else if (line.Length > 0)
				result.Add(Classified(line, GramSyntaxKind.Identifier, true));
			else
				result.Add([]);

		return result;
	}

	/// <summary>
	/// Text cut into runs by the grammar's own classification, where it is grammar; else one run in the
	/// colour of the token it describes.
	/// </summary>
	internal static IReadOnlyList<GramQuickInfoRun> Classified(string text, GramSyntaxKind fallbackKind, bool grammar)
	{
		var result   = new List<GramQuickInfoRun>();
		var position = 0;

		if (grammar)
			foreach (var item in GramLanguageService.Analyze(text).Classifications)
			{
				if (item.Position < position || item.Position + item.Length > text.Length)
					continue;

				if (item.Position > position)
					result.Add(new GramQuickInfoRun(text.Substring(position, item.Position - position), GramQuickInfoStyle.Text));

				result.Add(new GramQuickInfoRun(text.Substring(item.Position, item.Length), GramQuickInfoStyle.Syntax, item.Kind));
				position = item.Position + item.Length;
			}

		if (position < text.Length)
			result.Add(grammar
				? new GramQuickInfoRun(text.Substring(position), GramQuickInfoStyle.Text)
				: new GramQuickInfoRun(text.Substring(position), GramQuickInfoStyle.Syntax, fallbackKind));

		return result;
	}

	static bool Contains(int start, int length, int position)
	{
		return start <= position && position < start + length;
	}
}
