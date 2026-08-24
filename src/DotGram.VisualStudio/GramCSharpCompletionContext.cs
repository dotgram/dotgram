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

	static bool IsNameCharacter(char character) =>
		char.IsLetterOrDigit(character) || character is '_' or '.' or ':';
}
