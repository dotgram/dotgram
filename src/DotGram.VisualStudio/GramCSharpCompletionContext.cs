using System;

namespace DotGram.VisualStudio;

public static class GramCSharpCompletionContext
{
	public static bool TryGetPrefix(string text, int position, out string prefix)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		if (position < 0 || position > text.Length)
			throw new ArgumentOutOfRangeException(nameof(position));

		var start = position;
		while (start > 0 && IsNameCharacter(text[start - 1]))
			start--;

		if (start == 0 || text[start - 1] != '@' ||
			start < text.Length && text[start] == '(')
		{
			prefix = "";
			return false;
		}

		prefix = text.Substring(start, position - start);
		return true;
	}

	public static bool TryGetExpression(
		string text,
		int position,
		out string expression,
		out int expressionStart,
		out int symbolStart,
		out int symbolLength)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));
		if (position < 0 || position > text.Length)
			throw new ArgumentOutOfRangeException(nameof(position));

		var at = position;
		while (at > 0 && IsNameCharacter(text[at - 1]))
			at--;
		if (at == 0 || text[at - 1] != '@' || at < text.Length && text[at] == '(')
		{
			expression = "";
			expressionStart = symbolStart = symbolLength = 0;
			return false;
		}

		var end = position;
		while (end < text.Length && IsNameCharacter(text[end]))
			end++;

		var wordStart = position;
		while (wordStart > at && IsIdentifierCharacter(text[wordStart - 1]))
			wordStart--;
		var wordEnd = position;
		while (wordEnd < end && IsIdentifierCharacter(text[wordEnd]))
			wordEnd++;

		expression = text.Substring(at, end - at);
		expressionStart = at;
		symbolStart = wordStart;
		symbolLength = wordEnd - wordStart;
		return symbolLength > 0;
	}

	static bool IsNameCharacter(char character) =>
		char.IsLetterOrDigit(character) || character is '_' or '.' or ':';

	static bool IsIdentifierCharacter(char character) =>
		char.IsLetterOrDigit(character) || character == '_';
}
