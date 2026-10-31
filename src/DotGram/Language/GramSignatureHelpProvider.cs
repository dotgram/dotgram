using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>A parameter of a rule's signature, and where in the label it is written.</summary>
readonly record struct GramSignatureParameter(string Name, TextSpan Locus, string Documentation);

/// <summary>One signature of a parameterized rule.</summary>
/// <param name="Label">The rule's signature, as written: <c>Digits(n)</c>.</param>
/// <param name="Documentation">The definition after it.</param>
/// <param name="Parameters">Its parameters, in order.</param>
sealed record GramSignature(string Label, string Documentation, IReadOnlyList<GramSignatureParameter> Parameters);

/// <summary>Signature help at a position inside a call of a parameterized rule.</summary>
/// <param name="ApplicableSpan">From the call's opening parenthesis to the position asked about, at least one character.</param>
/// <param name="Signatures">The rule's signatures; there is one.</param>
/// <param name="ActiveSignature">The signature shown.</param>
/// <param name="ActiveParameter">The parameter of the argument the position is in; the last one past the end; -1 for a rule with none.</param>
sealed record GramSignatureHelp(TextSpan ApplicableSpan, IReadOnlyList<GramSignature> Signatures, int ActiveSignature, int ActiveParameter);

/// <summary>Signature help in a grammar: in a standalone <c>.gram</c> file, or embedded in C#.</summary>
abstract class GramSignatureHelpProvider
{
	/// <summary>The help at a position of a document, or null outside a call of a rule with parameters.</summary>
	public GramSignatureHelp? At(string text, int position)
	{
		if (!TryFindCall(text, position, out var name, out var nameStart, out var openParen, out var argument))
			return null;

		var available = Definitions(text, position).ToArray();
		var definition = (InGrammar(text, nameStart) ? StandardRule(text, name, nameStart) : null) ??
			available.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.Ordinal));

		if (definition.Signature is null)
			return null;

		var signature  = Signature(definition.Signature, Documentation(definition.Signature, definition.Description));
		var parameters = signature.Parameters.Count;

		return new GramSignatureHelp(
			new TextSpan(openParen, Math.Max(1, position - openParen)),
			[signature],
			0,
			argument < parameters ? argument : parameters - 1);
	}

	/// <summary>A signature's parameters, read from its label: the names between its parentheses, by comma.</summary>
	internal static GramSignature Signature(string content, string documentation)
	{
		var parameters = new List<GramSignatureParameter>();
		var open = content.IndexOf('(');
		var close = content.LastIndexOf(')');

		if (open >= 0 && close > open)
		{
			var start = open + 1;

			foreach (var part in content.Substring(start, close - start).Split(','))
			{
				var trimmed = part.Trim();
				var offset = content.IndexOf(trimmed, start, StringComparison.Ordinal);

				parameters.Add(new GramSignatureParameter(trimmed, new TextSpan(offset, trimmed.Length), "DotGram rule parameter"));
				start = offset + trimmed.Length;
			}
		}

		return new GramSignature(content, documentation, parameters);
	}

	static string Documentation(string signature, string description)
	{
		return description.StartsWith(signature, StringComparison.Ordinal)
			? description.Substring(signature.Length).TrimStart()
			: description;
	}

	/// <summary>The grammar a position stands in, and its place there; null outside a grammar.</summary>
	protected abstract (string Text, int Position)? GrammarAt(string text, int position);

	/// <summary>Whether a position is in a grammar's own text, and not in C# inside it or outside it.</summary>
	bool InGrammar(string text, int position)
	{
		return GrammarPosition.InGrammarText(GrammarAt(text, position));
	}

	/// <summary>The standard library's rule a call written <c>Std.Name(</c> names, which no document declares.</summary>
	static RuleSignature? StandardRule(string text, string name, int nameStart)
	{
		var start = Math.Max(0, nameStart - StandardLibrary.Name.Length - 2);
		var shown = text.Substring(start, nameStart + name.Length - start);

		return GramVocabulary.QualifiedStandardRule(shown, nameStart - start, name.Length) is { ParameterCount: > 0 } rule
			? new RuleSignature(name, rule.Signature, rule.Description)
			: null;
	}

	protected abstract IEnumerable<RuleSignature> Definitions(string text, int position);

	protected readonly struct RuleSignature(string name, string signature, string description)
	{
		public string Name { get; } = name;
		public string Signature { get; } = signature;
		public string Description { get; } = description;
	}

	static bool TryFindCall(
		string text,
		int position,
		out string name,
		out int nameStart,
		out int openParen,
		out int argument)
	{
		var depth = 0;
		argument = 0;

		for (var current = Math.Min(position, text.Length) - 1; current >= 0; current--)
			switch (text[current])
			{
				case ')': depth++; break;
				case '(' when depth > 0: depth--; break;
				case ',' when depth == 0: argument++; break;
				case '(':
					openParen = current;
					var end = current;
					var start = end;

					while (start > 0 && char.IsWhiteSpace(text[start - 1])) start--;
					end = start;
					while (start > 0 && (char.IsLetterOrDigit(text[start - 1]) || text[start - 1] == '_')) start--;

					name      = text.Substring(start, end - start);
					nameStart = start;
					return name.Length > 0;
			}

		name      = "";
		nameStart = 0;
		openParen = 0;
		return false;
	}
}

/// <summary>Signature help in a standalone <c>.gram</c> document.</summary>
sealed class StandaloneGramSignatureHelpProvider(GramDocument document) : GramSignatureHelpProvider
{
	protected override (string Text, int Position)? GrammarAt(string text, int position)
	{
		return GrammarPosition.Standalone(text, position);
	}

	protected override IEnumerable<RuleSignature> Definitions(string text, int position)
	{
		return document.Classifications
			.Where(static item => item.DefinitionPosition == item.Position && item.RuleParameterCount > 0)
			.Select(item => new RuleSignature(
				text.Substring(item.Position, item.Length),
				item.RuleSignature!,
				item.QuickInfo!));
	}
}

/// <summary>Signature help in the grammars of a C# document.</summary>
sealed class EmbeddedGramSignatureHelpProvider(EmbeddedGrammarView view) : GramSignatureHelpProvider
{
	protected override (string Text, int Position)? GrammarAt(string text, int position)
	{
		return view.Current
			? GrammarPosition.Embedded(text, position, view.Classifications)
			: null;
	}

	protected override IEnumerable<RuleSignature> Definitions(string text, int position)
	{
		if (!view.Current)
			return [];

		return view.Classifications
			.Where(item =>
				item.GrammarSpan.Contains(position) &&
				item.DefinitionSpan == item.Span &&
				item.RuleParameterCount > 0)
			.Select(item => new RuleSignature(
				text.Substring(item.Span.Start, item.Span.Length),
				item.RuleSignature!,
				item.QuickInfo!));
	}
}
