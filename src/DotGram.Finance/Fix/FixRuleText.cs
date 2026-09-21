using System;
using System.Globalization;
using System.Text;

namespace DotGram.Finance.Fix;

/// <summary>
/// The two places a counterparty's dictionary reaches the text a validator is compiled from,
/// and the one place that text is prepared for a compiler that has no comments.
/// </summary>
/// <remarks>
/// A dictionary is a file somebody else wrote. Every name and every value in it arrives here as
/// text, and text on its way into source code is the one thing in this package that has to be
/// assumed hostile: a value is written as a <em>literal</em>, never as a piece of code, and a
/// name is written into a comment with everything that could end the comment taken out of it.
/// Getting that right once is the whole of it; there is no later place to fix it.
///
/// <para>
/// The expression language has no comments — its trivia is whitespace and nothing else — so a
/// text carrying them would not compile. It carries them anyway, and <see cref="Blank"/> hands
/// the compiler a copy in which every comment has become spaces of the same length, newlines
/// kept. The geometry is then identical character for character, so a diagnostic at line 40,
/// column 5 of what the compiler read is line 40, column 5 of what a person reads. That is the
/// whole reason the comments may exist: a reader looking at a failure in code they did not write
/// needs the line to name the dictionary record they did write.
/// </para>
/// </remarks>
static class FixRuleText
{
	/// <summary>What a comment may run to before it is cut, in characters.</summary>
	const int Longest = 160;

	/// <summary>
	/// Appends <paramref name="value"/> as a quoted literal, escaped.
	/// </summary>
	/// <remarks>
	/// Plain quoted literals only — never verbatim, never raw, never interpolated. That is not a
	/// simplification, it is what makes <see cref="Blank"/> sound: a scanner is only as correct as
	/// the alphabet it has to read, and this is the method that keeps the alphabet closed.
	/// </remarks>
	public static void Literal(StringBuilder text, string value)
	{
		if (text is null) throw new ArgumentNullException(nameof(text));
		if (value is null) throw new ArgumentNullException(nameof(value));

		text.Append('"');

		foreach (var character in value)
			switch (character)
			{
				case '"':  text.Append("\\\""); break;
				case '\\': text.Append("\\\\"); break;
				case '\r': text.Append("\\r");  break;
				case '\n': text.Append("\\n");  break;
				case '\t': text.Append("\\t");  break;

				default:
					// Everything a compiler could read as anything but a character of a string:
					// the C0 and C1 ranges, and the two separators that are line breaks to some
					// readers and not to others.
					if (character < ' ' || character is '\u007f' or '\u0085' or '\u2028' or '\u2029' ||
						character is >= '\u0080' and <= '\u009f')
						text.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
					else
						text.Append(character);

					break;
			}

		text.Append('"');
	}

	/// <summary>
	/// The text of a comment naming a dictionary record: one line, bounded, nothing that ends a
	/// comment or starts a new line.
	/// </summary>
	/// <remarks>
	/// A line comment cannot be ended by anything but a line break, so a line break is what is
	/// taken out. <c>*&#47;</c> is taken out too, and a run of spaces is collapsed, because the
	/// text is read by a person and a name padded across forty columns in the file it came from
	/// should not be padded across forty columns here.
	/// </remarks>
	public static string Note(string raw)
	{
		if (raw is null) throw new ArgumentNullException(nameof(raw));

		var note  = new StringBuilder(Math.Min(raw.Length, Longest) + 4);
		var space = false;

		foreach (var character in raw)
		{
			if (note.Length >= Longest)
			{
				note.Append("...");

				break;
			}

			// Anything that is not a printable character of this line becomes one space, and
			// several in a row become one: a control character, a line break, a tab.
			if (character <= ' ' || character is '\u007f' or '\u0085' or '\u2028' or '\u2029' ||
				character is >= '\u0080' and <= '\u009f')
			{
				space = note.Length > 0;

				continue;
			}

			if (space)
			{
				note.Append(' ');

				space = false;
			}

			// Neither marker survives, in either direction: a comment this package writes is a
			// line comment, and a block comment it never writes is a block comment somebody
			// else's file asked for.
			if (character is '/' && note.Length > 0 && note[note.Length - 1] == '*')
				note[note.Length - 1] = ' ';

			if (character is '*' && note.Length > 0 && note[note.Length - 1] == '/')
				note[note.Length - 1] = ' ';

			note.Append(character);
		}

		return note.ToString();
	}

	/// <summary>
	/// The same text with every comment turned into spaces, character for character.
	/// </summary>
	/// <remarks>
	/// A scanner, not a search. <c>//</c> inside a string literal is two characters of a value
	/// that came out of somebody's dictionary, and a blanker that could not tell the difference
	/// would quietly eat the rest of that value — which is the same failure as every other one
	/// guarded against in this file, arriving through the one door nobody watches.
	/// </remarks>
	public static string Blank(string text)
	{
		if (text is null) throw new ArgumentNullException(nameof(text));

		var blanked = null as StringBuilder;

		for (var at = 0; at < text.Length; at++)
			switch (text[at])
			{
				case '"':
					at = PastQuoted(text, at, '"');
					break;

				case '\'':
					at = PastQuoted(text, at, '\'');
					break;

				case '/' when at + 1 < text.Length && text[at + 1] == '/':
					blanked ??= new StringBuilder(text);

					// To the end of the line, and the line break itself is left where it is: it
					// is what holds every following line at the number it had.
					while (at < text.Length && text[at] is not ('\r' or '\n'))
						blanked[at++] = ' ';

					at--;

					break;
			}

		return blanked?.ToString() ?? text;
	}

	/// <summary>The index of the quote that closes the one at <paramref name="opened"/>.</summary>
	/// <remarks>
	/// An unterminated literal returns the last index rather than throwing: this is not the place
	/// that reports a malformed text, and the compiler that reads the blanked copy says it better
	/// than a helper could.
	/// </remarks>
	static int PastQuoted(string text, int opened, char quote)
	{
		for (var at = opened + 1; at < text.Length; at++)
		{
			if (text[at] == '\\')
			{
				at++;

				continue;
			}

			if (text[at] == quote)
				return at;
		}

		return text.Length - 1;
	}
}
