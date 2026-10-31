using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Language;

namespace DotGram.LanguageServer;

/// <summary>A document's classifications as <c>textDocument/semanticTokens/full</c> carries them.</summary>
/// <remarks>
/// A kind is sent as the protocol's standard type where one means the same, and as a type of
/// the extension's own otherwise, which <c>package.json</c> declares with the standard type it
/// falls back to: a theme that knows nothing of DotGram still colours it by family, as the
/// Visual Studio extension's classifications are based on the editor's own.
/// </remarks>
static class SemanticTokensMap
{
	/// <summary>The legend: a token's type is its index here.</summary>
	public static IReadOnlyList<string> TokenTypes { get; } =
	[
		"comment",
		"keyword",
		"variable",
		"number",
		"string",
		"operator",
		"macro",
		"gramInvalid",
		"gramEmbeddedCode",
		"gramCaseInsensitive",
		"gramSpecialSymbol",
		"gramPunctuation",
	];

	public static IReadOnlyList<string> TokenModifiers { get; } = [];

	public static string TypeOf(GramSyntaxKind kind)
	{
		return kind switch
		{
			GramSyntaxKind.Comment                  => "comment",
			GramSyntaxKind.Keyword                  => "keyword",
			GramSyntaxKind.Identifier               => "variable",
			GramSyntaxKind.Binding                  => "variable",
			GramSyntaxKind.Number                   => "number",
			GramSyntaxKind.Character                => "string",
			GramSyntaxKind.String                   => "string",
			GramSyntaxKind.CharacterClass           => "string",
			GramSyntaxKind.CaseInsensitiveCharacter => "gramCaseInsensitive",
			GramSyntaxKind.CaseInsensitiveString    => "gramCaseInsensitive",
			GramSyntaxKind.EmbeddedCode             => "gramEmbeddedCode",
			GramSyntaxKind.Transition               => "macro",
			GramSyntaxKind.SpecialSymbol            => "gramSpecialSymbol",
			GramSyntaxKind.Operator                 => "operator",
			GramSyntaxKind.Punctuation              => "gramPunctuation",
			_                                       => "gramInvalid",
		};
	}

	static readonly Dictionary<string, int> Index = TokenTypes
		.Select(static (type, index) => (type, index))
		.ToDictionary(static pair => pair.type, static pair => pair.index, StringComparer.Ordinal);

	/// <summary>
	/// The tokens, one per line a span covers — a client draws no token across a line break — with
	/// a span that starts inside the one before it left out, as the protocol allows no overlap.
	/// </summary>
	public static IReadOnlyList<(int Line, int Character, int Length, int Type)> Tokens(
		IReadOnlyList<GramClassifiedSpan> classifications,
		LineMap lines)
	{
		var result = new List<(int Line, int Character, int Length, int Type)>(classifications.Count);
		var end    = 0;

		foreach (var item in classifications.OrderBy(static item => item.Position))
		{
			if (item.Length <= 0 || item.Position < end)
				continue;

			var type  = Index[TypeOf(item.Kind)];
			var start = item.Position;
			var stop  = Math.Min(item.Position + item.Length, lines.Length);
			var first = lines.Position(start).Line;
			var last  = lines.Position(stop).Line;

			for (var line = first; line <= last; line++)
			{
				var from = Math.Max(start, lines.LineStart(line));
				var to   = Math.Min(stop, lines.LineEnd(line));

				if (to > from)
					result.Add((line, from - lines.LineStart(line), to - from, type));
			}

			end = stop;
		}

		return result;
	}

	/// <summary>The tokens as the protocol's relative integers: five to a token.</summary>
	public static SemanticTokens Encode(IReadOnlyList<(int Line, int Character, int Length, int Type)> tokens)
	{
		var data      = new int[tokens.Count * 5];
		var line      = 0;
		var character = 0;

		for (var index = 0; index < tokens.Count; index++)
		{
			var token = tokens[index];
			var at    = index * 5;

			data[at]     = token.Line - line;
			data[at + 1] = token.Line == line ? token.Character - character : token.Character;
			data[at + 2] = token.Length;
			data[at + 3] = token.Type;
			data[at + 4] = 0;

			line      = token.Line;
			character = token.Character;
		}

		return new SemanticTokens(data);
	}

	public static SemanticTokens Map(IReadOnlyList<GramClassifiedSpan> classifications, LineMap lines)
	{
		return Encode(Tokens(classifications, lines));
	}
}
