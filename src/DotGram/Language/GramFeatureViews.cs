using System;
using System.Collections.Generic;

namespace DotGram.Language;

/// <summary>What one version of a standalone <c>.gram</c> document is known as, for the feature providers.</summary>
/// <param name="Text">The document's text at that version.</param>
/// <param name="Document">Its analysis; empty while that is not ready.</param>
/// <param name="ExternalDefinition">Where a name at a position is defined in an included grammar; null where nothing is included.</param>
/// <param name="IncludedReferences">The uses of a rule, by name and definition, in the grammars the host includes.</param>
sealed record GramStandaloneView(
	string Text,
	GramDocument Document,
	Func<int, StandaloneDefinition?>? ExternalDefinition = null,
	Func<string, int, IReadOnlyList<StandaloneReference>>? IncludedReferences = null);

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
