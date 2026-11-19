using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>What reads the library's own host fragments, which are C#.</summary>
static partial class StandardLibrary
{
	/// <summary>
	/// The scanner the library's own C# is read with, whatever the caller brought.
	/// </summary>
	/// <remarks>
	/// A grammar compiled with no scanner is refused its <c>@(...)</c> (GRAM1007), and that
	/// stays the contract for the author's text. The library is not the author's text: it
	/// comes along whether or not they brought a scanner, so it brings its own. Brackets
	/// balanced outside string and character literals — enough for C# written to be read
	/// this way, which the library's is, and nothing more. What the expressions ask of the
	/// parser is worked out by whoever is normalizing, with their scanner or the fallback
	/// search; this one answers null and leaves it to them.
	/// </remarks>
	public static ICSharpScanner Scanner { get; } = new OwnScanner();

	sealed class OwnScanner : ICSharpScanner
	{
		public bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
		{
			var depth = 0;

			for (var at = openParenthesisIndex; at < text.Length; at++)
			{
				switch (text[at])
				{
					case '(':
						depth++;
						break;

					case ')':
						if (--depth == 0)
						{
							closeParenthesisIndex = at;
							return true;
						}

						break;

					case '"':
					case '\'':
						at = PastLiteral(text, at);
						break;
				}
			}

			closeParenthesisIndex = -1;
			return false;
		}

		/// <summary>The index of the quote that closes the literal opening at <paramref name="at"/>.</summary>
		static int PastLiteral(string text, int at)
		{
			var quote = text[at];

			for (at++; at < text.Length; at++)
			{
				if (text[at] == '\\')
					at++;
				else if (text[at] == quote)
					return at;
			}

			return text.Length;
		}

		public IReadOnlyCollection<string>? FreeNames(string expression)
		{
			return null;
		}

		public IReadOnlyCollection<string>? NamesNotAsValues(string expression)
		{
			return null;
		}

		public string? Renamed(string expression, IReadOnlyDictionary<string, string> names)
		{
			return null;
		}
	}
}
