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

		if (start > 0 && text[start - 1] == '@' &&
			(start >= text.Length || text[start] != '('))
		{
			prefix = text.Substring(start, position - start);
			return true;
		}

		if (TryGetParenthesizedBounds(text, position, out var expressionStart, out _))
		{
			prefix = text.Substring(expressionStart, position - expressionStart);
			return true;
		}

		prefix = "";
		return false;
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
		int end;
		if (at > 0 && text[at - 1] == '@' && (at >= text.Length || text[at] != '('))
		{
			end = position;
			while (end < text.Length && IsNameCharacter(text[end]))
				end++;
		}
		else if (!TryGetParenthesizedBounds(text, position, out at, out end))
		{
			expression = "";
			expressionStart = symbolStart = symbolLength = 0;
			return false;
		}

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

	static bool TryGetParenthesizedBounds(
		string text, int position, out int expressionStart, out int expressionEnd)
	{
		var depth = 0;
		var open = -1;
		for (var index = position - 1; index >= 0; index--)
		{
			if (text[index] == ')')
				depth++;
			else if (text[index] == '(')
			{
				if (depth > 0)
					depth--;
				else if (index > 0 && text[index - 1] == '@')
				{
					open = index;
					break;
				}
			}
		}

		if (open < 0)
		{
			expressionStart = expressionEnd = 0;
			return false;
		}

		expressionStart = open + 1;
		expressionEnd = text.Length;
		depth = 0;
		for (var index = expressionStart; index < text.Length; index++)
		{
			if (text[index] == '(')
				depth++;
			else if (text[index] == ')')
			{
				if (depth == 0)
				{
					expressionEnd = index;
					break;
				}
				depth--;
			}
		}

		return position >= expressionStart && position <= expressionEnd;
	}

	static bool IsNameCharacter(char character) =>
		char.IsLetterOrDigit(character) || character is '_' or '.' or ':';

	static bool IsIdentifierCharacter(char character) =>
		char.IsLetterOrDigit(character) || character == '_';
}
