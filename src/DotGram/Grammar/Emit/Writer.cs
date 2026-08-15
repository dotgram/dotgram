using System;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>Indent-aware output. One per generated function, so nothing interleaves.</summary>
sealed class Writer(int depth)
{
	readonly StringBuilder _text = new();

	int _depth = depth;

	/// <summary>How far in the next line will be written — what a nested writer starts at.</summary>
	public int Depth => _depth;

	public void Line(string text = "")
	{
		if (text.Length == 0)
			_text.EndLine();
		else
			_text.Append('\t', _depth).AppendEndingWith(text);
	}

	/// <summary>A single indented line — the body of an <c>if</c> without braces.</summary>
	public void Then(string text)
	{
		_depth++;
		Line(text);
		_depth--;
	}

	public IDisposable Block(string header)
	{
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
		var written = new Writer(0);

		written._text.Append(Lines.Normalize(text));

		// AppendIndented reads the text as lines, each closed by an ending. Text that
		// does not end with one — a raw string literal, say — would lose its last line.
		if (!text.EndsWith(Lines.Ending, StringComparison.Ordinal))
			written._text.Append(Lines.Ending);

		AppendIndented(written, 0);
	}

	/// <summary>Appends another writer's text, shifted in to this one's depth.</summary>
	public void AppendIndented(Writer other, int extra = 1)
	{
		var lines = other._text.ToString().Split([Lines.Ending], StringSplitOptions.None);

		// The text ends with an ending, so the split leaves a final empty piece that is
		// not a line at all.
		for (var i = 0; i < lines.Length - 1; i++)
			if (lines[i].Length > 0)
				_text.Append('\t', _depth + extra).AppendEndingWith(lines[i]);
			else
				_text.EndLine();
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
