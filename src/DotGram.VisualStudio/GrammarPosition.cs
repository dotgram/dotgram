using System;
using System.Collections.Generic;

using DotGram.Language;

using Microsoft.VisualStudio.Text;

namespace DotGram.VisualStudio;

/// <summary>The grammar a point of a buffer stands in, as the generator reads it, and the point's place in it.</summary>
/// <remarks>
/// In a <c>.gram</c> file that is the file. In C#, it is the value of the <c>[Gram("…")]</c> literal
/// the point is in, decoded as C# decodes it — escapes, doubled quotes and a raw string's
/// indentation included — so that what is asked of the grammar is asked of the text the generator
/// compiles, not of its spelling.
/// </remarks>
static class GrammarPosition
{
	public static (string Text, int Position)? Standalone(ITextSnapshot snapshot, int position)
	{
		return (snapshot.GetText(), position);
	}

	public static (string Text, int Position)? Embedded(
		ITextSnapshot snapshot, int position, IReadOnlyList<HostClassification> classifications)
	{
		foreach (var item in classifications)
		{
			var span = item.GrammarSpan;

			if (position < span.Start || position > span.End)
				continue;

			return CSharpStringMap.TryDecode(snapshot.GetText(span.Start, span.Length), position - span.Start, out var grammar, out var at)
				? (grammar, at)
				: null;
		}

		return null;
	}

	/// <summary>Whether the point is in the grammar's own text, and not in C# inside it.</summary>
	public static bool InGrammarText((string Text, int Position)? grammar)
	{
		return grammar is { } at && !GramVocabulary.InCSharp(at.Text, at.Position);
	}
}
