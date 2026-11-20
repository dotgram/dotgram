using System;
using System.Globalization;

namespace DotGram.QueryLanguage;

// The JSON writer of the final projection. The translator ends a query with a chain of calls to the methods below,
// one a column — `End(Int(String(Start(anchor, names, formats), c1), c2))` — and the provider compiles the chain into
// its reading of the rows: each value goes from the reader, typed, straight into the writer, with nothing boxed and
// no object between them. One row is written by one synchronous call of the provider's materializer, so one thread
// writes it, into a buffer of that thread, whatever thread the next row is read on.
//
// None of the calls may be something a provider translates into SQL (string.Concat, string.Format and interpolation
// are), which is why the text is put together here and not in the tree.

/// <summary>What one call of the chain hands the next: nothing but the order of the calls; the text is in the thread's writer.</summary>
readonly struct QlJsonState;

/// <summary>Writes the rows of <c>QueryJsonRow</c>: the calls the final projection of its tree is made of.</summary>
static class QlJson
{
	/// <summary>A buffer past this size after a row is let go, so that one wide row does not hold memory for the thread's life.</summary>
	internal const int KeptBuffer = 64 * 1024;

	/// <summary>The row a thread is writing.</summary>
	sealed class Writer
	{
		public char[] Buffer = new char[256];
		public int    Length;

		/// <summary>The object format's <c>"name":</c> prefixes, in column order; null in the array format.</summary>
		public string[]? Names;
		public int       Name;

		/// <summary>The host's formats of the formatted columns, in column order.</summary>
		public Delegate[]? Formats;
		public int         Format;

		public void Ensure(int more)
		{
			if (Length + more <= Buffer.Length)
				return;

			var grown = new char[Math.Max(Buffer.Length * 2, Length + more)];

			Array.Copy(Buffer, grown, Length);

			Buffer = grown;
		}

		public void Append(char c)
		{
			Ensure(1);

			Buffer[Length++] = c;
		}

		public void Append(string s)
		{
			Ensure(s.Length);

			s.CopyTo(0, Buffer, Length, s.Length);

			Length += s.Length;
		}

		/// <summary>The comma before every value but the first, and the value's name in the object format.</summary>
		public void Next()
		{
			if (Length > 1)
				Append(',');

			if (Names is not null)
				Append(Names[Name++]);
		}
	}

	[ThreadStatic]
	static Writer? _writer;

	static Writer Current
	{
		get
		{
			return _writer ??= new Writer();
		}
	}

	// ── The row: started on a value of the row, so that no provider can compute a link once for all rows ──

	/// <summary>
	/// Starts a row: an array, or an object when <paramref name="names"/> are given. <paramref name="anchor"/> is a
	/// value the row reads, and only makes this call — and so every call after it — depend on the row. The writer is
	/// reset here, not at the end, so that a row after one a failure left half written starts clean.
	/// </summary>
	public static QlJsonState Start<TAnchor>(TAnchor anchor, string[]? names, Delegate[]? formats)
	{
		var writer = Current;

		writer.Length  = 0;
		writer.Names   = names;
		writer.Name    = 0;
		writer.Formats = formats;
		writer.Format  = 0;

		writer.Append(names is null ? '[' : '{');

		return default;
	}

	/// <summary>Ends the row and returns its text.</summary>
	public static string End(QlJsonState state)
	{
		var writer = Current;

		writer.Append(writer.Names is null ? ']' : '}');

		var text = new string(writer.Buffer, 0, writer.Length);

		if (writer.Buffer.Length > KeptBuffer)
			_writer = null;

		return text;
	}

	// ── One overload a kind, and one for its nullable ──

	public static QlJsonState Bool(QlJsonState state, bool value)
	{
		var writer = Current;

		writer.Next();
		writer.Append(value ? "true" : "false");

		return state;
	}

	public static QlJsonState Bool(QlJsonState state, bool? value)
	{
		return value is { } v ? Bool(state, v) : Null(state);
	}

	public static QlJsonState Int(QlJsonState state, int value)
	{
		var writer = Current;

		writer.Next();
		Number(writer, value, null);

		return state;
	}

	public static QlJsonState Int(QlJsonState state, int? value)
	{
		return value is { } v ? Int(state, v) : Null(state);
	}

	public static QlJsonState Long(QlJsonState state, long value)
	{
		var writer = Current;

		writer.Next();
		Number(writer, value, null);

		return state;
	}

	public static QlJsonState Long(QlJsonState state, long? value)
	{
		return value is { } v ? Long(state, v) : Null(state);
	}

	/// <summary>A decimal with the scale the provider read it with: <c>0.10</c> stays <c>0.10</c>.</summary>
	public static QlJsonState Decimal(QlJsonState state, decimal value)
	{
		var writer = Current;

		writer.Next();
		Number(writer, value, null);

		return state;
	}

	public static QlJsonState Decimal(QlJsonState state, decimal? value)
	{
		return value is { } v ? Decimal(state, v) : Null(state);
	}

	/// <summary>
	/// A double: the shortest text that reads back the same value where the runtime has one (.NET Core 3.0 and later),
	/// seventeen digits elsewhere; not a number and the infinities, which JSON has no number for, as strings.
	/// </summary>
	public static QlJsonState Double(QlJsonState state, double value)
	{
		var writer = Current;

		writer.Next();

		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			writer.Append('"');
			Number(writer, value, null);
			writer.Append('"');
		}
		else
		{
			Number(writer, value, DoubleFormat);
		}

		return state;
	}

	public static QlJsonState Double(QlJsonState state, double? value)
	{
		return value is { } v ? Double(state, v) : Null(state);
	}

	public static QlJsonState String(QlJsonState state, string? value)
	{
		var writer = Current;

		writer.Next();

		if (value is null)
			writer.Append("null");
		else
			Quote(writer, value);

		return state;
	}

	public static QlJsonState Timestamp(QlJsonState state, DateTime value)
	{
		var writer = Current;

		writer.Next();
		writer.Append('"');
		Number(writer, value, "O");
		writer.Append('"');

		return state;
	}

	public static QlJsonState Timestamp(QlJsonState state, DateTime? value)
	{
		return value is { } v ? Timestamp(state, v) : Null(state);
	}

	public static QlJsonState TimestampTz(QlJsonState state, DateTimeOffset value)
	{
		var writer = Current;

		writer.Next();
		writer.Append('"');
		Number(writer, value, "O");
		writer.Append('"');

		return state;
	}

	public static QlJsonState TimestampTz(QlJsonState state, DateTimeOffset? value)
	{
		return value is { } v ? TimestampTz(state, v) : Null(state);
	}

	public static QlJsonState Guid(QlJsonState state, Guid value)
	{
		var writer = Current;

		writer.Next();
		writer.Append('"');
		Number(writer, value, "D");
		writer.Append('"');

		return state;
	}

	public static QlJsonState Guid(QlJsonState state, Guid? value)
	{
		return value is { } v ? Guid(state, v) : Null(state);
	}

	static QlJsonState Null(QlJsonState state)
	{
		var writer = Current;

		writer.Next();
		writer.Append("null");

		return state;
	}

	// ── The host's formats, and the print of a type outside the language when it has none ──

	/// <summary>A value printed by the host's format of its type, as a JSON string; null, or a null the format returns, as null.</summary>
	public static QlJsonState Formatted<T>(QlJsonState state, T value)
	{
		var writer = Current;

		// Taken whether or not the value is null: the formats are in column order.
		var format = (Func<T, string?>)writer.Formats![writer.Format++];

		writer.Next();

		if (value is null)
		{
			writer.Append("null");

			return state;
		}

		Text(writer, Host(format, value));

		return state;
	}

	public static QlJsonState FormattedNullable<T>(QlJsonState state, T? value) where T : struct
	{
		if (value is { } v)
			return Formatted(state, v);

		var writer = Current;

		writer.Format++;

		return Null(state);
	}

	/// <summary>A value of a type outside the language that prints itself with a format: <c>ToString(null, InvariantCulture)</c>, as a JSON string.</summary>
	public static QlJsonState Formattable<T>(QlJsonState state, T value) where T : IFormattable
	{
		var writer = Current;

		writer.Next();

		if (value is null)
		{
			writer.Append("null");

			return state;
		}

		Text(writer, Host(Invariant<T>.Print, value));

		return state;
	}

	public static QlJsonState FormattableNullable<T>(QlJsonState state, T? value) where T : struct, IFormattable
	{
		return value is { } v ? Formattable(state, v) : Null(state);
	}

	/// <summary>A value of a type outside the language that has no format: <c>ToString()</c>, as a JSON string.</summary>
	public static QlJsonState Printed<T>(QlJsonState state, T value)
	{
		var writer = Current;

		writer.Next();

		if (value is null)
		{
			writer.Append("null");

			return state;
		}

		Text(writer, Host(Plain<T>.Print, value));

		return state;
	}

	public static QlJsonState PrintedNullable<T>(QlJsonState state, T? value) where T : struct
	{
		return value is { } v ? Printed(state, v) : Null(state);
	}

	/// <summary>
	/// Calls the host's code — a format, or a type's own <c>ToString</c> — with the thread's writer put aside: code that
	/// runs a query of its own on this thread writes that query's rows into a writer of its own, not into this row.
	/// </summary>
	static string? Host<T>(Func<T, string?> print, T value)
	{
		var writer = _writer;

		_writer = null;

		try
		{
			return print(value);
		}
		finally
		{
			_writer = writer;
		}
	}

	/// <summary>The print a value of a type with a format of its own gets: constrained, so a value type is not boxed.</summary>
	static class Invariant<T> where T : IFormattable
	{
		public static readonly Func<T, string?> Print = Make();

		static Func<T, string?> Make()
		{
			return value => value.ToString(null, CultureInfo.InvariantCulture);
		}
	}

	static class Plain<T>
	{
		public static readonly Func<T, string?> Print = Make();

		static Func<T, string?> Make()
		{
			return value => value!.ToString();
		}
	}

	static void Text(Writer writer, string? text)
	{
		if (text is null)
			writer.Append("null");
		else
			Quote(writer, text);
	}

	// ── Numbers and strings ──

#if NET
	const string DoubleFormat = "R";

	/// <summary>A value formatted culture-free straight into the buffer.</summary>
	static void Number<T>(Writer writer, T value, string? format) where T : ISpanFormattable
	{
		writer.Ensure(64);

		int written;

		while (!value.TryFormat(writer.Buffer.AsSpan(writer.Length), out written, format, CultureInfo.InvariantCulture))
			writer.Ensure(writer.Buffer.Length - writer.Length + 64);

		writer.Length += written;
	}
#else
	// .NET Framework's "R" does not read back every double; seventeen digits always do.
	const string DoubleFormat = "G17";

	/// <summary>A value formatted culture-free, through a string: the frameworks before .NET Core 2.1 cannot write into a span.</summary>
	static void Number<T>(Writer writer, T value, string? format) where T : IFormattable
	{
		writer.Append(value.ToString(format, CultureInfo.InvariantCulture));
	}
#endif

	/// <summary>
	/// A string as a JSON string. Escaped: the quote and the backslash; the control characters, with their short form
	/// where JSON has one; the line and paragraph separators, which JavaScript reads as line breaks; <c>&lt; &gt; &amp; '</c>,
	/// so that the text is safe inside HTML; and a surrogate without its pair, which would not survive the
	/// host's encoding to UTF-8. Everything else, every other character outside ASCII included, is written as it is.
	/// </summary>
	static void Quote(Writer writer, string value)
	{
		writer.Ensure(value.Length + 2);
		writer.Append('"');

		var run = 0;

		for (var i = 0; i < value.Length; i++)
		{
			var c = value[i];

			string? escape;

			switch (c)
			{
				case '"':  escape = "\\\""; break;
				case '\\': escape = "\\\\"; break;
				case '\b': escape = "\\b";  break;
				case '\f': escape = "\\f";  break;
				case '\n': escape = "\\n";  break;
				case '\r': escape = "\\r";  break;
				case '\t': escape = "\\t";  break;

				default:
				{
					if (c >= ' ' && c is not ('<' or '>' or '&' or '\'' or '\u2028' or '\u2029') && !char.IsSurrogate(c))
						continue;

					// A pair is a character outside the basic plane, written as it is.
					if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
					{
						i++;

						continue;
					}

					escape = null;

					break;
				}
			}

			Copy(writer, value, run, i);

			if (escape is not null)
				writer.Append(escape);
			else
				Unicode(writer, c);

			run = i + 1;
		}

		Copy(writer, value, run, value.Length);

		writer.Append('"');
	}

	static void Copy(Writer writer, string value, int from, int to)
	{
		var count = to - from;

		if (count == 0)
			return;

		writer.Ensure(count);

		value.CopyTo(from, writer.Buffer, writer.Length, count);

		writer.Length += count;
	}

	/// <summary><c>\uXXXX</c>, upper-case hex.</summary>
	static void Unicode(Writer writer, char c)
	{
		const string hex = "0123456789ABCDEF";

		writer.Ensure(6);

		var buffer = writer.Buffer;
		var at     = writer.Length;

		buffer[at]     = '\\';
		buffer[at + 1] = 'u';
		buffer[at + 2] = hex[(c >> 12) & 0xF];
		buffer[at + 3] = hex[(c >> 8) & 0xF];
		buffer[at + 4] = hex[(c >> 4) & 0xF];
		buffer[at + 5] = hex[c & 0xF];

		writer.Length = at + 6;
	}

	/// <summary><paramref name="value"/> as a JSON string, the way a row writes it: for the names of a document and of the object format.</summary>
	internal static string Quote(string value)
	{
		var writer = new Writer();

		Quote(writer, value);

		return new string(writer.Buffer, 0, writer.Length);
	}
}
