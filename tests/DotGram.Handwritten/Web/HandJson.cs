using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using DotGram.Web;

namespace DotGram.Handwritten.Web;

/// <summary>
/// RFC 8259 JSON, read by hand into the same <see cref="JsonValue"/> the generated
/// <c>Rfc8259</c> grammar builds.
/// </summary>
/// <remarks>
/// <para>
/// It reads exactly what the grammar reads (docs/design/architecture-decisions.md, D1): the
/// same accepted and refused text, the same values — a number as its text, a string with its
/// escapes undone and an escaped lone surrogate kept, an object's members in order with a name
/// written twice kept twice — and a refusal where the text stops being the beginning of any
/// JSON text.
/// </para>
/// <para>
/// JSON decides everything on one character, so this is a loop rather than a recursion: the
/// objects and arrays still open are a stack of its own, and a text nested a hundred thousand
/// deep costs a list that long rather than the thread's stack.
/// </para>
/// </remarks>
public static class HandJson
{
	/// <summary>A JSON text (§2), or false with the position where the text stops being one.</summary>
	public static bool TryParse(string text, out JsonValue? value, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var stacks = t_stacks ?? new Stacks();

		t_stacks = null;

		var reader = new Reader(text, stacks);

		value   = reader.Text();
		failure = value is null ? reader.Failure : -1;

		stacks.Members.Clear();
		stacks.Items.Clear();
		t_stacks = stacks;

		return value is not null;
	}

	/// <summary>
	/// What every open object and array has read so far, one stack for members and one for
	/// elements. Kept for the thread between texts, so that a large one grows them once and the
	/// next reuses them; a value closes by copying its own run off the top into an array of just
	/// its length. A list for each value would grow by doubling past what it holds and be copied
	/// again to close, and for a large one both would land on the large object heap.
	/// </summary>
	sealed class Stacks
	{
		public readonly List<KeyValuePair<string, JsonValue>> Members = [];
		public readonly List<JsonValue>                       Items   = [];
	}

	[ThreadStatic]
	static Stacks? t_stacks;

	/// <summary>An object or an array still open: where its members or elements begin.</summary>
	sealed class Open(bool isObject, int start)
	{
		public readonly bool IsObject = isObject;
		public readonly int  Start    = start;

		public string Name = "";
	}

	sealed class Reader(string text, Stacks stacks)
	{
		readonly string _text   = text;
		readonly Stacks _stacks = stacks;

		int _at;

		void Add(Open container, JsonValue value)
		{
			if (container.IsObject)
				_stacks.Members.Add(new KeyValuePair<string, JsonValue>(container.Name, value));
			else
				_stacks.Items.Add(value);
		}

		JsonValue Close(Open container)
		{
			if (container.IsObject)
			{
				var members = new KeyValuePair<string, JsonValue>[_stacks.Members.Count - container.Start];

				_stacks.Members.CopyTo(container.Start, members, 0, members.Length);
				_stacks.Members.RemoveRange(container.Start, members.Length);

				return new JsonValue.Object(members);
			}

			var items = new JsonValue[_stacks.Items.Count - container.Start];

			_stacks.Items.CopyTo(container.Start, items, 0, items.Length);
			_stacks.Items.RemoveRange(container.Start, items.Length);

			return new JsonValue.Array(items);
		}

		public int Failure { get; private set; }

		/// <summary>§2: whitespace, one value, whitespace, and nothing after.</summary>
		public JsonValue? Text()
		{
			var open = new List<Open>();

			Whitespace();

			while (true)
			{
				// A value begins here; an object or an array that begins opens and goes on to
				// its first member or element.
				JsonValue? value;

				switch (At(_at))
				{
					case '{':
						_at++;
						Whitespace();

						if (At(_at) == '}')
						{
							_at++;
							value = new JsonValue.Object([]);

							break;
						}

						var members = new Open(isObject: true, _stacks.Members.Count);

						open.Add(members);

						if (!Name(members))
							return null;

						continue;

					case '[':
						_at++;
						Whitespace();

						if (At(_at) == ']')
						{
							_at++;
							value = new JsonValue.Array([]);

							break;
						}

						open.Add(new Open(isObject: false, _stacks.Items.Count));

						continue;

					default:
						value = Scalar();

						if (value is null)
							return null;

						break;
				}

				// A value is complete: it goes to the container it is in, and every container it
				// completes closes in turn, until one goes on to another member or element.
				while (true)
				{
					Whitespace();

					if (open.Count == 0)
						return End() ? value : null;

					var innermost = open[open.Count - 1];

					Add(innermost, value);

					if (At(_at) == ',')
					{
						_at++;
						Whitespace();

						if (innermost.IsObject && !Name(innermost))
							return null;

						break;
					}

					if (At(_at) != (innermost.IsObject ? '}' : ']'))
						return Refused(_at);

					_at++;
					open.RemoveAt(open.Count - 1);
					value = Close(innermost);
				}
			}
		}

		/// <summary>§4: a member's name, whitespace, a colon and whitespace.</summary>
		bool Name(Open members)
		{
			if (At(_at) != '"')
			{
				Refused(_at);

				return false;
			}

			var name = String();

			if (name is null)
				return false;

			Whitespace();

			if (At(_at) != ':')
			{
				Refused(_at);

				return false;
			}

			_at++;
			Whitespace();

			members.Name = name;

			return true;
		}

		/// <summary>A literal name, a number or a string.</summary>
		JsonValue? Scalar()
		{
			var c = At(_at);

			switch (c)
			{
				case 'f':
					return Literal("false") ? JsonValue.Boolean.False : null;

				case 'n':
					return Literal("null") ? JsonValue.Null.Instance : null;

				case 't':
					return Literal("true") ? JsonValue.Boolean.True : null;

				case '"':
					return String() is { } value ? new JsonValue.String(value) : null;

				case '-' or >= '0' and <= '9':
					return Number();

				default:
					return Refused(_at);
			}
		}

		// A literal name is read whole or not at all. One that is not there is refused at the first
		// character that does not fit it, and where the text ends inside it, where it would have
		// begun: the generated parser's answer in every rendering.
		bool Literal(string name)
		{
			for (var index = 0; index < name.Length; index++)
				if (At(_at + index) != name[index])
				{
					Refused(_at + name.Length <= _text.Length ? _at + index : _at);

					return false;
				}

			_at += name.Length;

			return true;
		}

		/// <summary>
		/// §6: an optional minus, an integer without a leading zero, a fraction and an exponent
		/// each of one digit at least, kept as it was written.
		/// </summary>
		JsonValue? Number()
		{
			var start = _at;

			if (At(_at) == '-')
				_at++;

			if (At(_at) == '0')
				_at++;
			else if (IsDigit(At(_at)))
				Digits();
			else
				return Refused(_at);

			if (At(_at) == '.')
			{
				_at++;

				if (!IsDigit(At(_at)))
					return Refused(_at);

				Digits();
			}

			if (At(_at) is 'e' or 'E')
			{
				_at++;

				if (At(_at) is '+' or '-')
					_at++;

				if (!IsDigit(At(_at)))
					return Refused(_at);

				Digits();
			}

			return new JsonValue.Number(_text.Substring(start, _at - start));
		}

		void Digits()
		{
			while (IsDigit(At(_at)))
				_at++;
		}

		/// <summary>
		/// §7: a string between quotation marks, with its escapes undone. A <c>\u</c> escape is
		/// one UTF-16 unit, so a lone surrogate written as one stays one (§8.2).
		/// </summary>
		string? String()
		{
			var start = ++_at;

			// Most strings hold no escape, and are a substring.
			while (true)
			{
				var c = At(_at);

				if (c == '"')
				{
					_at++;

					return _text.Substring(start, _at - 1 - start);
				}

				if (c == '\\')
					break;

				if (c < ' ' || _at >= _text.Length)
				{
					Refused(_at);

					return null;
				}

				_at++;
			}

			var built = new StringBuilder(_text, start, _at - start, _at - start + 16);

			while (true)
			{
				var c = At(_at);

				if (c == '"')
				{
					_at++;

					return built.ToString();
				}

				if (c < ' ' || _at >= _text.Length)
				{
					Refused(_at);

					return null;
				}

				if (c != '\\')
				{
					built.Append(c);
					_at++;

					continue;
				}

				var escaped = At(++_at);

				switch (escaped)
				{
					case '"' or '\\' or '/':
						built.Append(escaped);
						break;

					case 'b': built.Append('\b'); break;
					case 'f': built.Append('\f'); break;
					case 'n': built.Append('\n'); break;
					case 'r': built.Append('\r'); break;
					case 't': built.Append('\t'); break;

					case 'u':
						for (var digit = 1; digit <= 4; digit++)
							if (!IsHex(At(_at + digit)))
							{
								Refused(_at + digit);

								return null;
							}

						built.Append((char)int.Parse(_text.AsSpan(_at + 1, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
						_at += 4;
						break;

					default:
						Refused(_at);

						return null;
				}

				_at++;
			}
		}

		/// <summary>§2's <c>ws</c>.</summary>
		void Whitespace()
		{
			while (At(_at) is ' ' or '\t' or '\n' or '\r')
				_at++;
		}

		bool End()
		{
			if (_at == _text.Length)
				return true;

			Refused(_at);

			return false;
		}

		JsonValue? Refused(int position)
		{
			Failure = position;

			return null;
		}

		/// <summary>The character at a position, or NUL past the end.</summary>
		char At(int position)
		{
			return position < _text.Length ? _text[position] : '\0';
		}
	}

	static bool IsDigit(char c)
	{
		return c is >= '0' and <= '9';
	}

	static bool IsHex(char c)
	{
		return c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
	}
}
