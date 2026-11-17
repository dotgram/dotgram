using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// The analysis of a C# document's grammars as the server tells it: the diagnostics published on the C#
/// document, and the colours as the <c>dotgram/embeddedColors</c> notification carries them.
/// </summary>
/// <remarks>
/// The colours are not semantic tokens: a client lets one provider of those colour a document, and a
/// C# document's is the C# extension's. The extension paints them as decorations over whatever colours
/// the C# extension gives, in the same families a <c>.gram</c> file's semantic tokens are painted in —
/// the names here are the token types of <see cref="SemanticTokensMap"/>, and the DSL roles' are the
/// protocol's standard types, as Visual Studio bases each DSL classification on its own C# one.
/// </remarks>
static class EmbeddedMap
{
	/// <summary>The kind a DSL role is painted as, by the role's name; a role not named here is not painted.</summary>
	static readonly Dictionary<string, string> Roles = new(StringComparer.Ordinal)
	{
		["Keyword"]     = "keyword",
		["Identifier"]  = "variable",
		["Type"]        = "type",
		["Variable"]    = "variable",
		["Function"]    = "function",
		["Method"]      = "function",
		["Property"]    = "property",
		["Number"]      = "number",
		["String"]      = "string",
		["Comment"]     = "comment",
		["Operator"]    = "operator",
		["Punctuation"] = "gramPunctuation",
		["Namespace"]   = "namespace",
		["Parameter"]   = "parameter",
		["Label"]       = "label",
	};

	/// <summary>The kinds a client may be told: the grammar's, then the DSLs' own.</summary>
	public static IReadOnlyList<string> Kinds { get; } =
	[
		.. SemanticTokensMap.TokenTypes,
		.. Roles.Values.Except(SemanticTokensMap.TokenTypes, StringComparer.Ordinal).Distinct(StringComparer.Ordinal),
	];

	/// <summary>The kind a DSL role is painted as, or null where it is not painted.</summary>
	public static string? KindOf(string role)
	{
		return Roles.TryGetValue(role, out var kind) ? kind : null;
	}

	/// <summary>The colours of one version, and where its grammars are, for the client to paint.</summary>
	public static EmbeddedColorsParams Colors(OpenCSharp document, GramVersion version, EmbeddedGrammarView view)
	{
		var lines  = version.Lines;
		var length = version.Text.Length;
		var used   = new List<string>();
		var index  = new Dictionary<string, int>(StringComparer.Ordinal);
		var spans  = new List<(TextSpan Span, string Kind)>(view.Classifications.Count + view.DslClassifications.Count);

		foreach (var item in view.Classifications)
			spans.Add((item.Span, SemanticTokensMap.TypeOf(item.Kind)));

		foreach (var item in view.DslClassifications)
			if (KindOf(item.Role) is { } kind)
				spans.Add((item.Span, kind));

		var data = new List<int>(spans.Count * 5);

		foreach (var (span, kind) in spans.OrderBy(static item => item.Span.Start))
		{
			if (span.Length <= 0 || span.End > length)
				continue;

			if (!index.TryGetValue(kind, out var type))
			{
				index.Add(kind, type = used.Count);
				used.Add(kind);
			}

			var range = lines.Range(span.Start, span.Length);

			data.Add(range.Start.Line);
			data.Add(range.Start.Character);
			data.Add(range.End.Line);
			data.Add(range.End.Character);
			data.Add(type);
		}

		return new EmbeddedColorsParams(document.Uri, document.Session, version.Number, used, data, Grammars(view, length).Select(span => lines.Range(span.Start, span.Length)).ToArray());
	}

	/// <summary>
	/// Where the document's grammars are: each grammar's string, and each DSL literal; what the extension
	/// asks the server about, and nothing else of a C# document.
	/// </summary>
	public static IReadOnlyList<TextSpan> Grammars(EmbeddedGrammarView view, int length)
	{
		return view.Outlines.Select(static outline => outline.Span)
			.Concat(view.Classifications.Select(static item => item.GrammarSpan))
			.Concat(view.DslSites.Select(static site => site.Span))
			.Where(span => span.Length > 0 && span.End <= length)
			.Distinct()
			.OrderBy(static span => span.Start)
			.ToArray();
	}

	/// <summary>Whether a position is in one of the document's grammars, its ends included.</summary>
	public static bool InGrammar(EmbeddedGrammarView view, int position)
	{
		foreach (var span in Grammars(view, int.MaxValue))
			if (span.Start <= position && position <= span.End)
				return true;

		return false;
	}

	public static IReadOnlyList<Diagnostic> Diagnostics(IReadOnlyList<HostDiagnostic> diagnostics, GramVersion version)
	{
		var lines  = version.Lines;
		var length = version.Text.Length;

		return diagnostics
			.Select(diagnostic =>
			{
				var (position, size) = GramDiagnosticText.Span(diagnostic.Span.Start, diagnostic.Span.Length, length);

				return new Diagnostic(
					lines.Range(position, size),
					DiagnosticsMap.Severity(diagnostic.Diagnostic.Severity),
					diagnostic.Diagnostic.Id,
					DiagnosticsMap.Source,
					diagnostic.Diagnostic.Message);
			})
			.ToArray();
	}

	/// <summary>Where a symbol at a position of the document is defined, as long as its name.</summary>
	public static Range Definition(EmbeddedGrammarView view, int position, LineMap lines)
	{
		var length = view.Symbols.FirstOrDefault(symbol => symbol.IsDefinition && symbol.Span.Start == position).Span.Length;

		return lines.Range(position, length);
	}
}
