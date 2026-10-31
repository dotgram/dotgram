using System;
using System.Collections.Generic;

namespace DotGram.Language;

/// <summary>What one version of a standalone <c>.gram</c> document is known as, for the feature providers.</summary>
/// <remarks>
/// The text is read only where a question needs it: an editor's text is not a string, and copying a
/// large grammar's whole text for every caret move would be most of what the question costs.
/// </remarks>
sealed class GramStandaloneView
{
	readonly Lazy<string> _text;

	/// <param name="text">The document's text at that version.</param>
	/// <param name="document">Its analysis; empty while that is not ready.</param>
	/// <param name="externalDefinition">Where a name at a position is defined in an included grammar; null where nothing is included.</param>
	/// <param name="includedReferences">The uses of a rule, by name and definition, in the grammars the host includes.</param>
	public GramStandaloneView(
		string text,
		GramDocument document,
		Func<int, StandaloneDefinition?>? externalDefinition = null,
		Func<string, int, IReadOnlyList<StandaloneReference>>? includedReferences = null)
		: this(() => text, text.Length, document, externalDefinition, includedReferences)
	{
	}

	/// <param name="text">The document's text at that version, asked for only where it is needed.</param>
	/// <param name="length">Its length.</param>
	/// <param name="document">Its analysis; empty while that is not ready.</param>
	/// <param name="externalDefinition">Where a name at a position is defined in an included grammar; null where nothing is included.</param>
	/// <param name="includedReferences">The uses of a rule, by name and definition, in the grammars the host includes.</param>
	public GramStandaloneView(
		Func<string> text,
		int length,
		GramDocument document,
		Func<int, StandaloneDefinition?>? externalDefinition = null,
		Func<string, int, IReadOnlyList<StandaloneReference>>? includedReferences = null)
	{
		_text              = new Lazy<string>(text);
		Length             = length;
		Document           = document;
		ExternalDefinition = externalDefinition;
		IncludedReferences = includedReferences;
	}

	public string Text => _text.Value;

	public int Length { get; }

	public GramDocument Document { get; }

	public Func<int, StandaloneDefinition?>? ExternalDefinition { get; }

	public Func<string, int, IReadOnlyList<StandaloneReference>>? IncludedReferences { get; }
}

/// <summary>The grammar a position of a document stands in, as the generator reads it, and the position's place in it.</summary>
/// <remarks>
/// In a <c>.gram</c> file that is the file. In C#, it is the value of the <c>[Gram("…")]</c> literal
/// the position is in, decoded as C# decodes it — escapes, doubled quotes and a raw string's
/// indentation included — so that what is asked of the grammar is asked of the text the generator
/// compiles, not of its spelling.
/// </remarks>
static class GrammarPosition
{
	public static (string Text, int Position)? Standalone(string text, int position)
	{
		return (text, position);
	}

	public static (string Text, int Position)? Embedded(
		string text, int position, IReadOnlyList<HostClassification> classifications)
	{
		foreach (var item in classifications)
		{
			var span = item.GrammarSpan;

			if (position < span.Start || position > span.End)
				continue;

			return CSharpStringMap.TryDecode(text.Substring(span.Start, span.Length), position - span.Start, out var grammar, out var at)
				? (grammar, at)
				: null;
		}

		return null;
	}

	/// <summary>Whether the position is in the grammar's own text, and not in C# inside it.</summary>
	public static bool InGrammarText((string Text, int Position)? grammar)
	{
		return grammar is { } at && !GramVocabulary.InCSharp(at.Text, at.Position);
	}
}
