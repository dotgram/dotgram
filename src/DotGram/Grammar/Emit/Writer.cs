using System;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>Indent-aware output for generated code, including bodies written into a shared destination.</summary>
sealed class Writer(int depth)
{
	readonly StringBuilder _text = new();

	int _depth = depth;

	readonly string? _observing;

	/// <summary>Observes a substring within written lines without retaining their text.</summary>
	public Writer(int depth, string observing) : this(depth) => _observing = observing;

	public bool Observed { get; private set; }

	bool Observe(string text)
	{
		if (_observing is null)
			return false;

		Observed = Observed || text.Contains(_observing, StringComparison.Ordinal);
		return true;
	}

	/// <summary>How far in the next line will be written — what a nested writer starts at.</summary>
	public int Depth => _depth;

	/// <summary>The insertion position after everything already written.</summary>
	public int Length => _text.Length;

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
		if (Observe(text))
			return;

		var length = text.Length;

		while (length > 0 && text[length - 1] is ' ' or '\t')
			length--;

		if (length == 0)
			_text.EndLine();
		else
			_text.Append('\t', _depth).Append(text, 0, length).EndLine();
	}

	/// <summary>Inserts a declaration before an existing body and returns the next insertion position.</summary>
	public int InsertLine(int at, string text)
	{
		if (Observe(text))
			return at;

		var length = text.Length;

		while (length > 0 && text[length - 1] is ' ' or '\t')
			length--;

		if (length > 0)
		{
			for (var i = 0; i < _depth; i++)
				_text.Insert(at++, '\t');

			_text.Insert(at, length == text.Length ? text : text.Substring(0, length));
			at += length;
		}

		_text.Insert(at, Lines.Ending);

		return at + Lines.Ending.Length;
	}

	/// <summary>Checks emitted text without materializing a copy of the buffer.</summary>
	public bool Contains(string value, int start)
	{
		if (value.Length == 0)
			return true;

		for (var at = start; at <= _text.Length - value.Length; at++)
		{
			if (_text[at] != value[0])
				continue;

			var length = 1;

			while (length < value.Length && _text[at + length] == value[length])
				length++;

			if (length == value.Length)
				return true;
		}

		return false;
	}

	/// <summary>A line written exactly as given, at no indent at all.</summary>
	/// <remarks>
	/// For the two things whose column is the point rather than an accident: a `#line`
	/// directive, and the line under one, which is padded out to the column the grammar
	/// had so that a C# error lands where the author wrote the code (§7.6).
	/// </remarks>
	public void Exactly(string text)
	{
		if (!Observe(text))
			_text.AppendEndingWith(text);
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

	public void Append(Writer other)
	{
		if (_observing is null)
			_text.Append(other._text);
		else
			Observe(other.ToString());
	}

	/// <summary>Adds a small header before an already emitted body without copying the body.</summary>
	public void Prepend(Writer header)
	{
		var text = header.ToString();
		if (!Observe(text))
			_text.Insert(0, text);
	}

	/// <summary>
	/// Writes text that is already laid out, each line at the current depth.
	/// </summary>
	/// <remarks>
	/// Raw literals may use LF or CRLF depending on how the generator source was saved.
	/// Read their line ranges directly and emit CRLF, without copying the whole block to
	/// normalize its endings or to append a final ending.
	/// </remarks>
	public void Write(string text) => AppendLines(text, 0, normalize: true);

	/// <summary>Appends another writer's text, shifted in to this one's depth.</summary>
	/// <remarks>
	/// Everything but a <c>#line</c> region. Inside one the column is the point — it is
	/// what puts a C# error under the code the author wrote (§7.6) — so shifting those
	/// lines in would move every error one tab to the right of where it belongs.
	/// </remarks>
	public void AppendIndented(Writer other, int extra = 1) => AppendLines(other.ToString(), extra);

	/// <summary>Appends line ranges, optionally normalizing raw text and closing its last line.</summary>
	/// <remarks>
	/// Read in place rather than split: every state of every machine is appended this way, and
	/// an array of lines and a string for each was most of what writing a parser allocated.
	/// </remarks>
	void AppendLines(string text, int extra, bool normalize = false)
	{
		if (Observe(text))
			return;

		var kept = false;
		var at   = 0;

		for (;;)
		{
			var end = normalize
				? text.IndexOf('\n', at)
				: text.IndexOf(Lines.Ending, at, StringComparison.Ordinal);
			var final = end < 0;

			if (final)
			{
				// Preserve Write's final-line convention, including the extra blank line
				// for raw text ending in a bare LF rather than CRLF.
				if (!normalize || text.EndsWith(Lines.Ending, StringComparison.Ordinal))
					break;

				end = text.Length;
			}

			var length = end - at;

			if (normalize && !final && length > 0 && text[end - 1] == '\r')
				length--;

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

			if (final)
				break;

			at = end + (normalize ? 1 : Lines.Ending.Length);
		}

		static bool Leads(string text, int at, int length, string prefix) =>
			length >= prefix.Length && string.CompareOrdinal(text, at, prefix, 0, prefix.Length) == 0;
	}

	/// <summary>A sink for complete method/reader groups; host fields never use this path.</summary>
	public Func<string, bool>? SeparateMethods { get; set; }

	public void Methods(string text)
	{
		if (SeparateMethods?.Invoke(text) != true)
			Write(text);
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
