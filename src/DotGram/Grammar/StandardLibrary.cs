using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Grammar;

/// <summary>
/// The standard library: a grammar every grammar has, under <c>Std</c> (§5.2).
/// </summary>
/// <remarks>
/// <para>
/// A <c>.gram</c> file carried inside the generator as a resource, and spliced onto every
/// grammar the way an included grammar is — into a namespace of its own, after everything
/// the author wrote, so that nothing they can see moves. It has no class behind it: its
/// rules are compiled into whichever grammar calls them, and the C# they carry names
/// nothing but the framework, in full.
/// </para>
/// <para>
/// Always there, the way the framework is to C#: <c>Std.Integer</c> names it in full with
/// nothing declared, and <c>using Std;</c> opens the namespace as it opens any other. What
/// is not called is pruned, so a grammar that never names it pays a few milliseconds of
/// compilation and nothing in what comes out.
/// </para>
/// </remarks>
public static class StandardLibrary
{
	/// <summary>The namespace a grammar reaches the library through.</summary>
	public const string Name = "Std";

	static string? _text;

	/// <summary>The library's own text, as it was written.</summary>
	public static string Text => _text ??= Read();

	static string Read()
	{
		using var stream = typeof(StandardLibrary).Assembly.GetManifestResourceStream("DotGram.Std.gram")
			?? throw new InvalidOperationException("The standard library is not in this assembly.");
		using var reader = new StreamReader(stream);

		return reader.ReadToEnd();
	}

	/// <summary>
	/// The grammar with the library spliced onto its end, under <see cref="Name"/>.
	/// </summary>
	public static string SplicedOnto(string grammarText) =>
		GrammarSplice.Join(
			new GrammarSplice.Part(grammarText, null, null),
			[new GrammarSplice.Part(Text, Name, null)]).Text;

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

		public IReadOnlyCollection<string>? FreeNames(string expression) => null;
	}
}
