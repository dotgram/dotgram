using System;
using System.Globalization;
using System.Threading;

namespace DotGram.QueryLanguage.Probes.Core;

/// <summary>The mark one link of the JSON chain hands to the next; the text itself is in the thread's buffer.</summary>
readonly struct QlJsonState
{
}

/// <summary>
/// The JSON writer of the final projection, as the library would hold it: internal, one method per value kind, each
/// writing into a thread-static buffer. linq2db compiles the calls into its materializer, so one row is written by
/// one synchronous call on one thread.
/// </summary>
static class QlJson
{
	[ThreadStatic]
	static char[]? buffer;

	[ThreadStatic]
	static int length;

	/// <summary>Set by <see cref="NameById"/>: the next value is written without a separator of its own.</summary>
	[ThreadStatic]
	static bool named;

	/// <summary>The names of the row being written, set once per row by <see cref="SetNames"/> (an experiment of probe 11).</summary>
	[ThreadStatic]
	static string[]? rowNames;

	[ThreadStatic]
	static int nameIndex;

	/// <summary>Object-format names by id: the tree carries an int, not the name's text (an experiment of probe 11).</summary>
	internal static readonly string?[] Names = new string?[256];

	/// <summary>How many rows were started, across threads: tells whether an operator printed rows at all.</summary>
	internal static long Starts;

	/// <summary>A format id's delegate: the fallback where the tree carries an int instead of the delegate.</summary>
	internal static readonly Delegate?[] Formats = new Delegate?[64];

	static void Begin(char open)
	{
		Interlocked.Increment(ref Starts);

		buffer ??= new char[256];
		length   = 0;

		Append(open);
	}

	static void Ensure(int more)
	{
		if (length + more <= buffer!.Length)
		{
			return;
		}

		var grown = new char[Math.Max(buffer.Length * 2, length + more)];

		Array.Copy(buffer, grown, length);

		buffer = grown;
	}

	static void Append(char c)
	{
		Ensure(1);

		buffer![length++] = c;
	}

	static void Append(string s)
	{
		Ensure(s.Length);

		s.CopyTo(0, buffer!, length, s.Length);

		length += s.Length;
	}

	static void Separate(string? name)
	{
		if (named)
		{
			named = false;

			return;
		}

		if (rowNames != null && name == null)
		{
			name = rowNames[nameIndex++];
		}

		var last = buffer![length - 1];

		if (last != '[' && last != '{')
		{
			Append(',');
		}

		if (name != null)
		{
			Append(name);
		}
	}

	static void Formattable<T>(T value, string? format) where T : ISpanFormattable
	{
		Ensure(64);

		if (!value.TryFormat(buffer.AsSpan(length), out var written, format, CultureInfo.InvariantCulture))
		{
			Append(value.ToString(format, CultureInfo.InvariantCulture));

			return;
		}

		length += written;
	}

	static void Quoted<T>(T value, string? format) where T : ISpanFormattable
	{
		Append('"');
		Formattable(value, format);
		Append('"');
	}

	static void Text(string value)
	{
		Append('"');

		foreach (var c in value)
		{
			switch (c)
			{
				case '"':  Append("\\\""); break;
				case '\\': Append("\\\\"); break;
				case '\b': Append("\\b");  break;
				case '\f': Append("\\f");  break;
				case '\n': Append("\\n");  break;
				case '\r': Append("\\r");  break;
				case '\t': Append("\\t");  break;

				default:
					if (c < ' ' || c == '\u2028' || c == '\u2029' || c == '<' || c == '>' || c == '&' || c == '\'' || char.IsSurrogate(c))
					{
						Append("\\u");
						Formattable((int)c, "X4");
					}
					else
					{
						Append(c);
					}

					break;
			}
		}

		Append('"');
	}

	static void Null()
	{
		Append("null");
	}

	/// <summary>Evaluated before the chain (the first argument of <see cref="EndObjectNamed"/>): one constant per row.</summary>
	public static QlJsonState SetNames(string[] names)
	{
		rowNames  = names;
		nameIndex = 0;

		return default;
	}

	public static string EndObjectNamed(QlJsonState names, QlJsonState state)
	{
		rowNames = null;

		return End('}');
	}

	public static QlJsonState NameById(QlJsonState state, int name)
	{
		Separate(Names[name]);

		named = true;

		return state;
	}

	// ── start and end: the anchor makes the first link depend on the row ──

	public static QlJsonState StartArray<TAnchor>(TAnchor anchor)
	{
		Begin('[');

		return default;
	}

	public static QlJsonState StartObject<TAnchor>(TAnchor anchor)
	{
		Begin('{');

		return default;
	}

	public static string EndArray(QlJsonState state)
	{
		return End(']');
	}

	public static string EndObject(QlJsonState state)
	{
		return End('}');
	}

	static string End(char close)
	{
		Append(close);

		var text = new string(buffer!, 0, length);

		if (buffer!.Length > 64 * 1024)
		{
			buffer = null;
		}

		return text;
	}

	// ── one overload per kind; `name` is null in the array format, the escaped `"name":` in the object format ──

	public static QlJsonState Bool(QlJsonState state, string? name, bool value)
	{
		Separate(name);
		Append(value ? "true" : "false");

		return state;
	}

	public static QlJsonState Bool(QlJsonState state, string? name, bool? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Bool(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState Int32(QlJsonState state, string? name, int value)
	{
		Separate(name);
		Formattable(value, null);

		return state;
	}

	public static QlJsonState Int32(QlJsonState state, string? name, int? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Int32(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState Int64(QlJsonState state, string? name, long value)
	{
		Separate(name);
		Formattable(value, null);

		return state;
	}

	public static QlJsonState Int64(QlJsonState state, string? name, long? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Int64(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState Decimal(QlJsonState state, string? name, decimal value)
	{
		Separate(name);
		Formattable(value, null);

		return state;
	}

	public static QlJsonState Decimal(QlJsonState state, string? name, decimal? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Decimal(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState Double(QlJsonState state, string? name, double value)
	{
		Separate(name);

		if (double.IsFinite(value))
		{
			Formattable(value, "R");
		}
		else
		{
			Quoted(value, "R");
		}

		return state;
	}

	public static QlJsonState Double(QlJsonState state, string? name, double? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Double(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState String(QlJsonState state, string? name, string? value)
	{
		Separate(name);

		if (value == null)
		{
			Null();
		}
		else
		{
			Text(value);
		}

		return state;
	}

	public static QlJsonState Timestamp(QlJsonState state, string? name, DateTime value)
	{
		Separate(name);
		Quoted(value, "O");

		return state;
	}

	public static QlJsonState Timestamp(QlJsonState state, string? name, DateTime? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Timestamp(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState TimestampTz(QlJsonState state, string? name, DateTimeOffset value)
	{
		Separate(name);
		Quoted(value, "O");

		return state;
	}

	public static QlJsonState TimestampTz(QlJsonState state, string? name, DateTimeOffset? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return TimestampTz(state, name, value.GetValueOrDefault());
	}

	public static QlJsonState Guid(QlJsonState state, string? name, Guid value)
	{
		Separate(name);
		Quoted(value, "D");

		return state;
	}

	public static QlJsonState Guid(QlJsonState state, string? name, Guid? value)
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Guid(state, name, value.GetValueOrDefault());
	}

	// ── a host format, and the default print of a type outside the nine ──

	public static QlJsonState Formatted<T>(QlJsonState state, string? name, Func<T, string?> format, T value)
	{
		Separate(name);

		if (value is null)
		{
			Null();

			return state;
		}

		var text = format(value);

		if (text == null)
		{
			Null();
		}
		else
		{
			Text(text);
		}

		return state;
	}

	public static QlJsonState FormattedNullable<T>(QlJsonState state, string? name, Func<T, string?> format, T? value) where T : struct
	{
		if (value == null)
		{
			Separate(name);
			Null();

			return state;
		}

		return Formatted(state, name, format, value.GetValueOrDefault());
	}

	public static QlJsonState FormattedById<T>(QlJsonState state, string? name, int format, T value)
	{
		return Formatted(state, name, (Func<T, string?>)Formats[format]!, value);
	}

	public static QlJsonState DefaultFormattable<T>(QlJsonState state, string? name, T value) where T : IFormattable
	{
		Separate(name);

		if (value is null)
		{
			Null();
		}
		else
		{
			Text(value.ToString(null, CultureInfo.InvariantCulture));
		}

		return state;
	}

	public static QlJsonState DefaultToString<T>(QlJsonState state, string? name, T value)
	{
		Separate(name);

		var text = value?.ToString();

		if (text == null)
		{
			Null();
		}
		else
		{
			Text(text);
		}

		return state;
	}
}
