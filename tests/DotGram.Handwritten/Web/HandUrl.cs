using System;

using DotGram.Web;

namespace DotGram.Handwritten.Web;

/// <summary>
/// RFC 3986 URI references, read by hand into the same <see cref="UriReference"/> the
/// generated <c>Rfc3986</c> grammar builds.
/// </summary>
/// <remarks>
/// <para>
/// It reads exactly what the grammar reads (docs/design/architecture-decisions.md, D1): the
/// same accepted and refused text, the same parts, and a refusal at the same position. The
/// grammar backtracks fully, so the position it reports is the furthest any of its readings
/// got; for a language this regular that is where the text stops being the beginning of any
/// reference, and this parser is written to notice that at the first character it can.
/// </para>
/// <para>
/// Most of a reference decides itself on one character: <c>//</c> begins an authority, a
/// <c>/</c> an absolute path, <c>?</c> and <c>#</c> end the path. The two places that do not
/// are the scheme, which a reference only has if a <c>:</c> follows it, and the host, which
/// is an address only if nothing but digits and dots make it up. The first is settled by
/// reading the text as a URI and, failing that, as a relative reference. The second needs no
/// settling at all: an IPv4 address is also a registered name, and the part comes back as the
/// text either way.
/// </para>
/// </remarks>
public static class HandUrl
{
	/// <summary>
	/// A URI or a relative reference (§4.1), or false with the position where the text stops
	/// being one.
	/// </summary>
	public static bool TryParseReference(string text, out UriReference? reference, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var reader = new Reader(text);

		reference = reader.Uri() ?? reader.Relative();
		failure   = reference is null ? reader.Furthest : -1;

		return reference is not null;
	}

	/// <summary>
	/// A URI (§3), a reference with a scheme, or false with the position where the text stops
	/// being one.
	/// </summary>
	public static bool TryParseUri(string text, out UriReference? uri, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var reader = new Reader(text);

		uri     = reader.Uri();
		failure = uri is null ? reader.Furthest : -1;

		return uri is not null;
	}

	/// <summary>
	/// One reading of a text: where it is, where each part read so far lies, and the furthest
	/// position any attempt was refused at. A struct, and the parts kept as positions until the
	/// reading is done, so that a reference costs the one record it is and the text of its parts,
	/// and a refusal costs nothing.
	/// </summary>
	struct Reader(string text)
	{
		readonly string _text = text;

		int _at;

		Part _scheme;
		Part _userInfo;
		Part _host;
		Part _port;
		Part _path;
		Part _query;
		Part _fragment;

		public int Furthest { get; private set; }

		// ── The two readings of a reference ──────────────────────────────────────

		/// <summary>The text as a URI: a scheme, a colon and a hierarchical part, to the end.</summary>
		public UriReference? Uri()
		{
			Start();

			if (!Is(IsAlpha))
				return null;

			while (Is(IsSchemeCharacter))
			{
			}

			var scheme = new Part(0, _at);

			if (!Is(':') || !Hierarchical(relative: false) || !Rest())
				return null;

			_scheme = scheme;

			return Parts();
		}

		/// <summary>The text as a relative reference, to the end.</summary>
		public UriReference? Relative()
		{
			Start();

			return Hierarchical(relative: true) && Rest() ? Parts() : null;
		}

		void Start()
		{
			_at     = 0;
			_scheme = _userInfo = _host = _port = _path = _query = _fragment = default;
		}

		UriReference Parts()
		{
			return new UriReference(
				Text(_scheme), Text(_userInfo), Text(_host), Text(_port), Text(_path) ?? "", Text(_query), Text(_fragment));
		}

		string? Text(Part part)
		{
			return part.Present ? _text.Substring(part.Start, part.End - part.Start) : null;
		}

		/// <summary>
		/// §3 and §4.2: an authority and the path after it, or a path alone. A relative
		/// reference's first segment may not hold a colon, or it would have been a scheme.
		/// </summary>
		bool Hierarchical(bool relative)
		{
			if (Peek('/') && Peek('/', 1))
			{
				_at += 2;

				if (!Authority())
					return false;

				var start = _at;

				while (Is('/'))
					Segment();

				_path = new Part(start, _at);

				return true;
			}

			var from = _at;

			if (Is('/'))
			{
				// An absolute path: its first segment, if it has one, is not empty, which is
				// what keeps "//" above from being read as a path.
				if (Segment() > 0)
					while (Is('/'))
						Segment();
			}
			else if (relative ? FirstSegmentWithoutColon() > 0 : Segment() > 0)
			{
				while (Is('/'))
					Segment();
			}

			_path = new Part(from, _at);

			return true;
		}

		/// <summary>The query and the fragment, and then nothing.</summary>
		bool Rest()
		{
			if (Is('?'))
				_query = Run(IsQueryCharacter);

			if (Is('#'))
				_fragment = Run(IsQueryCharacter);

			if (_at < _text.Length)
			{
				Refused(_at);

				return false;
			}

			return true;
		}

		// ── §3.2, the authority ──────────────────────────────────────────────────

		bool Authority()
		{
			var start = _at;

			// A userinfo is everything before the first '@', if it comes before anything a
			// userinfo may not hold.
			var userInfo = default(Part);
			var user     = Run(IsUserInfoCharacter);

			if (Is('@'))
				userInfo = user;
			else
				_at = start;

			Part host;

			if (Peek('['))
			{
				var literal = _at;

				if (!IpLiteral())
					return false;

				host = new Part(literal, _at);
			}
			else
				host = Run(IsRegisteredNameCharacter);

			var port = default(Part);

			if (Is(':'))
			{
				var digits = _at;

				while (Is(IsDigit))
				{
				}

				port = new Part(digits, _at);
			}

			_userInfo = userInfo;
			_host     = host;
			_port     = port;

			return true;
		}

		/// <summary>
		/// §3.2.2: an IPv6 address or a future one, in brackets. They cannot both read the
		/// same text — one begins with a hex digit or a colon, the other with a 'v' — so each
		/// is tried and the one that reads is the host.
		/// </summary>
		bool IpLiteral()
		{
			var start = ++_at;
			var end   = Ipv6(start);

			if (end < 0)
				end = IpvFuture(start);

			if (end < 0)
				return false;

			if (At(end) != ']')
			{
				Refused(end);

				return false;
			}

			_at = end + 1;

			return true;
		}

		/// <summary>
		/// An IPv6 address from <paramref name="at"/>: the position after it, or -1.
		/// </summary>
		/// <remarks>
		/// The nine forms of §3.2.2 say one thing between them: eight groups of up to four hex
		/// digits, the last two of which may be an IPv4 address, or fewer with a single
		/// <c>::</c> standing for the rest — and since it stands for at least one, seven at
		/// most either side of it together. So this counts groups rather than trying the forms,
		/// and stops at the first character no address could have.
		/// </remarks>
		int Ipv6(int at)
		{
			var groups     = 0;
			var compressed = false;
			var mayEnd     = false;

			// A leading colon is only ever the first half of '::', and a '::' that is not there
			// is refused at its second character, or where it would have begun if the text ends
			// before one: the generated parser's answer in every rendering.
			if (At(at) == ':')
			{
				if (At(at + 1) != ':')
				{
					Refused(at + 2 <= _text.Length ? at + 1 : at);

					return -1;
				}

				compressed = true;
				mayEnd     = true;
				at        += 2;
			}

			while (true)
			{
				var room = (compressed ? 7 : 8) - groups;

				// The last two groups may be written as an IPv4 address, which ends it.
				if (room >= 2 && (compressed || groups == 6) && Ipv4(at) is var address and >= 0)
					return address;

				if (room < 1 || !IsHex(At(at)))
				{
					Refused(at);

					return mayEnd ? at : -1;
				}

				var next = at + 1;

				while (next < at + 4 && IsHex(At(next)))
					next++;

				groups++;
				mayEnd = compressed || groups == 8;

				// A colon goes on only where another group, or the '::', still fits.
				if (At(next) != ':' || groups == (compressed ? 7 : 8))
				{
					Refused(next);

					return mayEnd ? next : -1;
				}

				if (At(next + 1) == ':')
				{
					if (compressed)
					{
						Refused(next + 1);

						return next;
					}

					compressed = true;
					mayEnd     = true;
					at         = next + 2;

					continue;
				}

				mayEnd = false;
				at     = next + 1;
			}
		}

		int Ipv4(int at)
		{
			for (var octet = 0; octet < 4; octet++)
			{
				if (octet > 0)
				{
					if (At(at) != '.')
					{
						Refused(at);

						return -1;
					}

					at++;
				}

				at = DecimalOctet(at);

				if (at < 0)
					return -1;
			}

			return at;
		}

		/// <summary>
		/// A number from 0 to 255 written without a leading zero, as §3.2.2's <c>dec-octet</c>
		/// spells it.
		/// </summary>
		int DecimalOctet(int at)
		{
			if (!IsDigit(At(at)))
			{
				Refused(at);

				return -1;
			}

			if (At(at) == '0')
				return at + 1;

			var value = 0;
			var end   = at;

			while (end < at + 3 && IsDigit(At(end)))
			{
				if (value * 10 + (At(end) - '0') > 255)
					break;

				value = value * 10 + (At(end) - '0');
				end++;
			}

			return end;
		}

		/// <summary>
		/// §3.2.2's <c>IPvFuture</c>: a 'v', a hex version, a dot and at least one character.
		/// </summary>
		int IpvFuture(int at)
		{
			if (At(at) != 'v')
			{
				Refused(at);

				return -1;
			}

			var start = ++at;

			while (IsHex(At(at)))
				at++;

			if (at == start || At(at) != '.')
			{
				Refused(at);

				return -1;
			}

			start = ++at;

			while (IsUnreserved(At(at)) || IsSubDelimiter(At(at)) || At(at) == ':')
				at++;

			Refused(at);

			return at == start ? -1 : at;
		}

		// ── §3.3, path segments ──────────────────────────────────────────────────

		/// <summary>A segment of path characters: how many characters it took.</summary>
		int Segment()
		{
			var start = _at;

			while (PathCharacter(colon: true))
			{
			}

			return _at - start;
		}

		/// <summary>The first segment of a relative path, which may not hold a colon.</summary>
		int FirstSegmentWithoutColon()
		{
			var start = _at;

			while (PathCharacter(colon: false))
			{
			}

			return _at - start;
		}

		/// <summary>One <c>pchar</c> (§3.3), an escape counting as one.</summary>
		bool PathCharacter(bool colon)
		{
			var c = At(_at);

			if (IsUnreserved(c) || IsSubDelimiter(c) || c == '@' || colon && c == ':')
			{
				_at++;

				return true;
			}

			return Escape();
		}

		// ── Reading ──────────────────────────────────────────────────────────────

		/// <summary>A run of characters and escapes, as where it lies.</summary>
		Part Run(Func<char, bool> accepts)
		{
			var start = _at;

			while (true)
			{
				if (accepts(At(_at)))
					_at++;
				else if (!Escape())
					break;
			}

			return new Part(start, _at);
		}

		/// <summary>
		/// §2.1: a '%' and two hex digits. A '%' that is not followed by them refuses the text
		/// where the digit should have been.
		/// </summary>
		bool Escape()
		{
			if (At(_at) != '%')
			{
				Refused(_at);

				return false;
			}

			if (!IsHex(At(_at + 1)))
			{
				Refused(_at + 1);

				return false;
			}

			if (!IsHex(At(_at + 2)))
			{
				Refused(_at + 2);

				return false;
			}

			_at += 3;

			return true;
		}

		bool Is(char c)
		{
			if (At(_at) == c)
			{
				_at++;

				return true;
			}

			Refused(_at);

			return false;
		}

		bool Is(Func<char, bool> accepts)
		{
			if (accepts(At(_at)))
			{
				_at++;

				return true;
			}

			Refused(_at);

			return false;
		}

		bool Peek(char c, int ahead = 0)
		{
			return At(_at + ahead) == c;
		}

		/// <summary>The character at a position, or NUL past the end, which nothing accepts.</summary>
		char At(int position)
		{
			return position >= 0 && position < _text.Length ? _text[position] : '\0';
		}

		void Refused(int position)
		{
			if (position > Furthest)
				Furthest = position;
		}
	}

	// ── §2, the character sets ───────────────────────────────────────────────────

	/// <summary>Where a part lies in the text; the default is a part that is not there.</summary>
	readonly struct Part(int start, int end)
	{
		public readonly int  Start   = start;
		public readonly int  End     = end;
		public readonly bool Present = true;
	}

	static bool IsAlpha(char c)
	{
		return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
	}

	static bool IsDigit(char c)
	{
		return c is >= '0' and <= '9';
	}

	static bool IsHex(char c)
	{
		return c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
	}

	static bool IsUnreserved(char c)
	{
		return IsAlpha(c) || IsDigit(c) || c is '-' or '.' or '_' or '~';
	}

	static bool IsSubDelimiter(char c)
	{
		return c is '!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '=';
	}

	static bool IsSchemeCharacter(char c)
	{
		return IsAlpha(c) || IsDigit(c) || c is '+' or '-' or '.';
	}

	static bool IsUserInfoCharacter(char c)
	{
		return IsUnreserved(c) || IsSubDelimiter(c) || c == ':';
	}

	static bool IsRegisteredNameCharacter(char c)
	{
		return IsUnreserved(c) || IsSubDelimiter(c);
	}

	static bool IsQueryCharacter(char c)
	{
		return IsUnreserved(c) || IsSubDelimiter(c) || c is ':' or '@' or '/' or '?';
	}
}
