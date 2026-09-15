using System;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>Indent-aware output. One per generated function, so nothing interleaves.</summary>
sealed class Writer(int depth)
{
	readonly StringBuilder _text = new();

	/// <summary>What a line loses from its end.</summary>
	/// <remarks>One array for every line: `TrimEnd(' ', '\t')` makes a new one each time it is called.</remarks>
	static readonly char[] Blanks = [' ', '\t'];

	int _depth = depth;

	/// <summary>How far in the next line will be written — what a nested writer starts at.</summary>
	public int Depth => _depth;

	/// <summary>One line at the current depth, ending in no whitespace.</summary>
	/// <remarks>
	/// Nothing the generator writes ends in whitespace: a blank line is an ending and not an
	/// indentation followed by one, and a run of `case 'a': ` built a label at a time loses the
	/// space it was built with. Generated code is committed as snapshots that have to be the
	/// generator's output to the byte, and with `trim_trailing_whitespace` set every one of
	/// them was an editor save away from failing.
	/// </remarks>
	public void Line(string text = "")
	{
		var trimmed = text.TrimEnd(Blanks);

		if (trimmed.Length == 0)
			_text.EndLine();
		else
			_text.Append('\t', _depth).AppendEndingWith(trimmed);
	}

	/// <summary>A line written exactly as given, at no indent at all.</summary>
	/// <remarks>
	/// For the two things whose column is the point rather than an accident: a `#line`
	/// directive, and the line under one, which is padded out to the column the grammar
	/// had so that a C# error lands where the author wrote the code (§7.6).
	/// </remarks>
	public void Exactly(string text) => _text.AppendEndingWith(text);

	/// <summary>A single indented line — the body of an <c>if</c> without braces.</summary>
	public void Then(string text)
	{
		_depth++;
		Line(text);
		_depth--;
	}

	public IDisposable Block(string header)
	{
		// A block with no header is one that opens under the line before it — the body of
		// an `if`, a state under its label. Writing the empty header would put a blank line
		// between the two, which is exactly where the brace should not be.
		if (header.Length > 0)
			Line(header);

		Line("{");
		_depth++;

		return new Closer(this);
	}

	/// <summary>Indents what follows, without braces around it — a switch section.</summary>
	public IDisposable Indent()
	{
		_depth++;

		return new Outdenter(this);
	}

	public void Append(Writer other) => _text.Append(other._text);

	/// <summary>
	/// Writes text that is already laid out, each line at the current depth.
	/// </summary>
	/// <remarks>
	/// The endings are normalized first, and that is not tidiness. What arrives here is a
	/// raw string literal, whose content is whatever the file it was typed in was saved
	/// with — so a generator whose own source went from CRLF to LF would silently start
	/// emitting every one of these blocks as a single unsplit line, indented once and
	/// flattened after that. Generated code must not depend on how the generator was
	/// saved.
	/// </remarks>
	public void Write(string text)
	{
		var normalized = Lines.Normalize(text);

		// The text is read as lines, each closed by an ending. Text that does not end with
		// one — a raw string literal, say — would lose its last line.
		if (!text.EndsWith(Lines.Ending, StringComparison.Ordinal))
			normalized += Lines.Ending;

		AppendLines(normalized, 0);
	}

	/// <summary>Appends another writer's text, shifted in to this one's depth.</summary>
	/// <remarks>
	/// Everything but a <c>#line</c> region. Inside one the column is the point — it is
	/// what puts a C# error under the code the author wrote (§7.6) — so shifting those
	/// lines in would move every error one tab to the right of where it belongs.
	/// </remarks>
	public void AppendIndented(Writer other, int extra = 1) => AppendLines(other.ToString(), extra);

	/// <summary>Each line of the text closed by an ending, shifted in; what follows the last ending is not a line.</summary>
	/// <remarks>
	/// Read in place rather than split: every state of every machine is appended this way, and
	/// an array of lines and a string for each was most of what writing a parser allocated.
	/// </remarks>
	void AppendLines(string text, int extra)
	{
		var kept = false;
		var at   = 0;

		for (int end; (end = text.IndexOf(Lines.Ending, at, StringComparison.Ordinal)) >= 0; at = end + Lines.Ending.Length)
		{
			var length = end - at;

			if (Leads(text, at, length, "#line"))
				kept = !Leads(text, at, length, "#line default");

			// Inside a `#line` region the text is the author's C#, copied as it was written:
			// its columns are what put an error under the code (§7.6), and a verbatim string
			// in it may hold whitespace that means something. Everything else is the
			// generator's, and loses whatever it would have ended in.
			if (!kept)
				while (length > 0 && text[at + length - 1] is ' ' or '\t')
					length--;

			if (length == 0)
				_text.EndLine();
			else if (kept || Leads(text, at, length, "#line default"))
				_text.Append(text, at, length).EndLine();
			else
				_text.Append('\t', _depth + extra).Append(text, at, length).EndLine();
		}

		static bool Leads(string text, int at, int length, string prefix) =>
			length >= prefix.Length && string.CompareOrdinal(text, at, prefix, 0, prefix.Length) == 0;
	}

	public override string ToString() => _text.ToString();

	sealed class Closer(Writer writer) : IDisposable
	{
		public void Dispose()
		{
			writer._depth--;
			writer.Line("}");
		}
	}

	sealed class Outdenter(Writer writer) : IDisposable
	{
		public void Dispose() => writer._depth--;
	}
}
