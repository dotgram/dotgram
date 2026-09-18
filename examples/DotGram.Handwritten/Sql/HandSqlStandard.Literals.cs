using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §5.3, the literals, and §10.1's interval qualifier, which an interval
// literal ends in.
partial class HandSqlStandard
{
	/// <summary><c>&lt;literal&gt;</c>: a signed number, or a general literal.</summary>
	static bool Literal(ref SqlCursor cursor, out LiteralValue value)
	{
		// <signed numeric literal>: the sign and the number are one literal, and its text is both,
		// whatever stood between them.
		if (cursor.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
		{
			var save = cursor;
			var sign = cursor.Kind == SqlTokenKind.Plus ? "+" : "-";

			cursor.Take();

			if (cursor.Kind == SqlTokenKind.Number)
			{
				value = NumericLiteral(sign + cursor.TextOf(cursor.Token));

				cursor.Take();

				return true;
			}

			cursor = save;
			value  = null!;

			return false;
		}

		if (cursor.Kind == SqlTokenKind.Number)
		{
			value = NumericLiteral(cursor.TextOf(cursor.Token));

			cursor.Take();

			return true;
		}

		return GeneralLiteral(ref cursor, out value);
	}

	/// <summary><c>&lt;unsigned literal&gt;</c>.</summary>
	static bool UnsignedLiteral(ref SqlCursor cursor, out LiteralValue value)
	{
		if (cursor.Kind == SqlTokenKind.Number)
		{
			value = NumericLiteral(cursor.TextOf(cursor.Token));

			cursor.Take();

			return true;
		}

		return GeneralLiteral(ref cursor, out value);
	}

	/// <summary><c>&lt;general literal&gt;</c>: a string of one of the four kinds, a datetime, an interval, or a truth value.</summary>
	static bool GeneralLiteral(ref SqlCursor cursor, out LiteralValue value)
	{
		switch (cursor.Kind)
		{
			case SqlTokenKind.National:
				value = StringLiteral(ref cursor, StringLiteralKind.National);

				return true;

			case SqlTokenKind.UnicodeString:
				value = StringLiteral(ref cursor, StringLiteralKind.Unicode);

				return true;

			case SqlTokenKind.String:
				value = StringLiteral(ref cursor, StringLiteralKind.Character);

				return true;

			case SqlTokenKind.Binary:
			{
				var token = cursor.Token;

				value = new LiteralValue.Binary(cursor.Text.Substring(token.Quote, token.End - token.Quote));

				cursor.Take();

				return true;
			}

			case SqlTokenKind.Word:
				switch (cursor.Word)
				{
					case SqlWord.Date     : return DatetimeLiteral(ref cursor, DateTimeLiteralKind.Date, out value);
					case SqlWord.Time     : return DatetimeLiteral(ref cursor, DateTimeLiteralKind.Time, out value);
					case SqlWord.Timestamp: return DatetimeLiteral(ref cursor, DateTimeLiteralKind.Timestamp, out value);
					case SqlWord.Interval : return IntervalLiteral(ref cursor, out value);

					case SqlWord.True:
						value = new LiteralValue.Boolean(BooleanLiteral.True);
						cursor.Take();

						return true;

					case SqlWord.False:
						value = new LiteralValue.Boolean(BooleanLiteral.False);
						cursor.Take();

						return true;

					case SqlWord.Unknown:
						value = new LiteralValue.Boolean(BooleanLiteral.Unknown);
						cursor.Take();

						return true;
				}

				break;
		}

		value = null!;

		return false;
	}

	/// <summary>
	/// A string literal of any kind: what was written from its first quote to its last, the
	/// character set its introducer named, and a Unicode literal's escape character.
	/// </summary>
	static LiteralValue StringLiteral(ref SqlCursor cursor, StringLiteralKind kind)
	{
		var token = cursor.Token;
		var text  = cursor.Text;

		// Where the literal's own text begins: the quote the lexer stopped at, which is past the
		// introducer. A quote inside a delimited name in the introducer — `_u&"'s".x'a'` — belongs
		// to that name, and searching the token for the first one would take it instead.
		var quote = token.Quote;

		CharacterSetName? characterSet = null;

		if (text[token.Start] == '_')
		{
			// `_latin1'a'`, and `_latin1U&'a'`, whose introducer ends two characters earlier.
			var end = kind == StringLiteralKind.Unicode ? quote - 2 : quote;

			characterSet = CharacterSet(text.AsSpan(token.Start + 1, end - token.Start - 1));
		}

		var literal = new LiteralValue.String(
			text.Substring(quote, token.Body - quote),
			kind,
			characterSet,
			token.End > token.Body ? '\\' : null);

		cursor.Take();

		return literal;
	}

	/// <summary>
	/// Where one name in an introducer ends and the next begins, or -1 where this is the last.
	/// </summary>
	/// <remarks>
	/// A delimited name may hold a period of its own — `_u&"a.b".latin1'x'` names a set of two parts
	/// and not three — so the periods inside one are passed over. A doubled quote inside a delimited
	/// name is one character of it and does not end it.
	/// </remarks>
	static int Divider(ReadOnlySpan<char> dotted)
	{
		for (var at = 0; at < dotted.Length; at++)
		{
			if (dotted[at] == '.')
				return at;

			if (dotted[at] != '\"')
				continue;

			for (at++; at < dotted.Length; at++)
				if (dotted[at] == '\"')
				{
					if (at + 1 >= dotted.Length || dotted[at + 1] != '\"')
						break;

					at++;
				}
		}

		return -1;
	}

	/// <summary>How a name in an introducer was written: `"a"`, `U&"a"`, or neither.</summary>
	static IdentifierStyle Styled(ReadOnlySpan<char> part) =>
		part.Length > 2 && (part[0] | 0x20) == 'u' && part[1] == '&' && part[2] == '"' ? IdentifierStyle.UnicodeDelimited :
		part.Length > 0 && part[0] == '"' ? IdentifierStyle.Delimited :
		IdentifierStyle.Regular;

	/// <summary>A character set as an introducer writes it, divided at the periods between its parts.</summary>
	static CharacterSetName CharacterSet(ReadOnlySpan<char> dotted)
	{
		var names = new List<Identifier>();

		while (true)
		{
			var dot  = Divider(dotted);
			var part = dot < 0 ? dotted : dotted[..dot];

			names.Add(new Identifier(part.ToString(), Styled(part)));

			if (dot < 0)
				return new CharacterSetName(new QualifiedName(names.ToArray()));

			dotted = dotted[(dot + 1)..];
		}
	}

	/// <summary>An <c>&lt;unsigned numeric literal&gt;</c>, and what its spelling makes it.</summary>
	static LiteralValue NumericLiteral(string text)
	{
		var body = text.Length > 0 && (text[0] == '+' || text[0] == '-') ? text.AsSpan(1) : text.AsSpan();
		var kind =
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'x' ? NumericLiteralKind.HexInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'o' ? NumericLiteralKind.OctalInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'b' ? NumericLiteralKind.BinaryInteger :
			body.IndexOfAny('e', 'E') >= 0                              ? NumericLiteralKind.Approximate :
			body.IndexOf('.') >= 0                                      ? NumericLiteralKind.Decimal :
			NumericLiteralKind.DecimalInteger;

		return new LiteralValue.Numeric(text, kind);
	}

	// ── §5.3 <datetime literal>, <interval literal> ────────────────────────────

	/// <summary>
	/// <c>DATE '2020-01-01'</c>, <c>TIME '12:00:00'</c>, <c>TIMESTAMP '2020-01-01 12:00:00'</c>:
	/// the key word, and a string whose characters are a value of the type.
	/// </summary>
	static bool DatetimeLiteral(ref SqlCursor cursor, DateTimeLiteralKind kind, out LiteralValue value)
	{
		var save = cursor;

		cursor.Take();

		if (Quoted(ref cursor, out var body, out var text) && DatetimeString(body, kind))
		{
			value = new LiteralValue.DateTime(kind, text);

			cursor.Take();

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	/// <summary><c>INTERVAL '1' DAY</c>: a sign may stand before the string, and a qualifier follows it.</summary>
	static bool IntervalLiteral(ref SqlCursor cursor, out LiteralValue value)
	{
		var save = cursor;

		cursor.Take();

		UnaryOperator? sign = null;

		if (cursor.Kind == SqlTokenKind.Plus)
		{
			sign = UnaryOperator.Plus;
			cursor.Take();
		}
		else if (cursor.Kind == SqlTokenKind.Minus)
		{
			sign = UnaryOperator.Minus;
			cursor.Take();
		}

		if (Quoted(ref cursor, out var body, out var text) && IntervalString(body))
		{
			cursor.Take();

			if (IntervalQualifier(ref cursor, out var qualifier))
			{
				value = new LiteralValue.Interval(text, qualifier, sign);

				return true;
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	/// <summary>
	/// A string written in one run, which is what a date, a time and an interval string are: its
	/// characters between the quotes, and the token's own text for the tree.
	/// </summary>
	static bool Quoted(ref SqlCursor cursor, out ReadOnlySpan<char> body, out string text)
	{
		var token = cursor.Token;

		if (token.Kind != SqlTokenKind.String || token.Parts != 1 || token.Quote != token.Start)
		{
			body = default;
			text = null!;

			return false;
		}

		body = cursor.Text.AsSpan(token.Start + 1, token.End - token.Start - 2);
		text = cursor.TextOf(token);

		return true;
	}

	/// <summary>Whether the characters are a <c>&lt;date value&gt;</c>, a <c>&lt;time value&gt;</c> or both.</summary>
	static bool DatetimeString(ReadOnlySpan<char> body, DateTimeLiteralKind kind)
	{
		var at = 0;

		switch (kind)
		{
			case DateTimeLiteralKind.Date:
				return Date(body, ref at) && at == body.Length;

			case DateTimeLiteralKind.Time:
				return Time(body, ref at) && Zone(body, ref at) && at == body.Length;

			default:
				// A <timestamp string> divides its date from its time with one space.
				return Date(body, ref at) && Take(body, ref at, ' ') && Time(body, ref at) && Zone(body, ref at) && at == body.Length;
		}
	}

	/// <summary><c>&lt;date value&gt;</c>: three unsigned integers, periods apart.</summary>
	static bool Date(ReadOnlySpan<char> body, ref int at) =>
		Decimal(body, ref at) && Take(body, ref at, '-') && Decimal(body, ref at) && Take(body, ref at, '-') && Decimal(body, ref at);

	/// <summary><c>&lt;time value&gt;</c>: hours, minutes, and a seconds value.</summary>
	static bool Time(ReadOnlySpan<char> body, ref int at) =>
		Decimal(body, ref at) && Take(body, ref at, ':') && Decimal(body, ref at) && Take(body, ref at, ':') && Seconds(body, ref at);

	/// <summary><c>&lt;time zone interval&gt;</c>, where one was written.</summary>
	static bool Zone(ReadOnlySpan<char> body, ref int at)
	{
		if (at >= body.Length || body[at] != '+' && body[at] != '-')
			return true;

		at++;

		return Decimal(body, ref at) && Take(body, ref at, ':') && Decimal(body, ref at);
	}

	/// <summary><c>&lt;seconds value&gt;</c>: an integer, and a fraction after it where one was written.</summary>
	static bool Seconds(ReadOnlySpan<char> body, ref int at)
	{
		if (!Decimal(body, ref at))
			return false;

		if (at < body.Length && body[at] == '.')
		{
			at++;

			Decimal(body, ref at);
		}

		return true;
	}

	/// <summary>
	/// Whether the characters are an <c>&lt;interval string&gt;</c>. A year-month literal and a
	/// day-time one begin alike — <c>'5'</c> is a year, a day, an hour, a minute and a second — so
	/// each form is asked to reach the end of the string before the next is tried.
	/// </summary>
	static bool IntervalString(ReadOnlySpan<char> body)
	{
		if (body.Length > 0 && (body[0] == '+' || body[0] == '-'))
			body = body[1..];

		// <year-month literal>: `1` or `1-2`.
		var at = 0;

		if (Decimal(body, ref at))
		{
			if (at == body.Length)
				return true;

			if (body[at] == '-')
			{
				at++;

				if (Decimal(body, ref at) && at == body.Length)
					return true;
			}
		}

		// <day-time interval>: `1`, `1 2`, `1 2:3`, `1 2:3:4.5`.
		at = 0;

		if (Decimal(body, ref at))
		{
			if (at < body.Length && body[at] == ' ')
			{
				at++;

				if (Decimal(body, ref at))
				{
					if (at == body.Length)
						return true;

					if (body[at] == ':')
					{
						at++;

						if (Decimal(body, ref at))
						{
							if (at == body.Length)
								return true;

							if (body[at] == ':')
							{
								at++;

								if (Seconds(body, ref at) && at == body.Length)
									return true;
							}
						}
					}
				}
			}
		}

		// <time interval>: `1:2:3.4`, `1:2`, `1.5`.
		at = 0;

		if (Decimal(body, ref at) && at < body.Length && body[at] == ':')
		{
			at++;

			if (Decimal(body, ref at))
			{
				if (at == body.Length)
					return true;

				if (body[at] == ':')
				{
					at++;

					if (Seconds(body, ref at) && at == body.Length)
						return true;
				}
			}
		}

		at = 0;

		if (Decimal(body, ref at) && at < body.Length && body[at] == ':')
		{
			at++;

			if (Seconds(body, ref at) && at == body.Length)
				return true;
		}

		at = 0;

		return Seconds(body, ref at) && at == body.Length;
	}

	/// <summary><c>&lt;unsigned decimal integer&gt;</c>: digits, an underscore allowed between two of them.</summary>
	static bool Decimal(ReadOnlySpan<char> body, ref int at)
	{
		if (at >= body.Length || !SqlCursor.IsDigit(body[at]))
			return false;

		at++;

		while (at < body.Length)
		{
			if (SqlCursor.IsDigit(body[at]))
				at++;
			else if (body[at] == '_' && at + 1 < body.Length && SqlCursor.IsDigit(body[at + 1]))
				at += 2;
			else
				break;
		}

		return true;
	}

	static bool Take(ReadOnlySpan<char> body, ref int at, char ch)
	{
		if (at >= body.Length || body[at] != ch)
			return false;

		at++;

		return true;
	}

	// ── §10.1 Interval qualifier ───────────────────────────────────────────────

	/// <summary><c>&lt;interval qualifier&gt;</c>: a single field, or a start field and an end field.</summary>
	static bool IntervalQualifier(ref SqlCursor cursor, out IntervalQualifier qualifier)
	{
		qualifier = null!;

		var field = Field(cursor.Word);

		if (field is null)
			return false;

		var save   = cursor;
		var second = field == DateTimeField.Second;

		cursor.Take();

		int? leading = null, fractional = null;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			if (!Unsigned(ref cursor, out leading))
			{
				cursor = save;

				return false;
			}

			// <interval fractional seconds precision>, which only a single SECOND may name here.
			if (second && cursor.Take(SqlTokenKind.Comma) && !Unsigned(ref cursor, out fractional))
			{
				cursor = save;

				return false;
			}

			if (!cursor.Take(SqlTokenKind.RightParen))
			{
				cursor = save;

				return false;
			}
		}

		// `YEAR TO MONTH`, `DAY(2) TO SECOND(3)`: a start field is no SECOND, and an end one keeps
		// its own precision — the fractional one where it is a SECOND.
		if (!second && cursor.Take(SqlWord.To))
		{
			var end = Field(cursor.Word);

			if (end is null)
			{
				cursor = save;

				return false;
			}

			cursor.Take();

			int? precision = null;

			// Only a SECOND names a precision at the end, and the one it names is fractional.
			if (end == DateTimeField.Second && cursor.Take(SqlTokenKind.LeftParen))
			{
				if (!Unsigned(ref cursor, out precision) || !cursor.Take(SqlTokenKind.RightParen))
				{
					cursor = save;

					return false;
				}
			}

			qualifier = new IntervalQualifier(field.Value, leading, end.Value, precision);

			return true;
		}

		qualifier = new IntervalQualifier(field.Value, leading, null, fractional);

		return true;
	}

	/// <summary>Which <c>&lt;primary datetime field&gt;</c> a word is, where it is one.</summary>
	static DateTimeField? Field(SqlWord word) =>
		word switch
		{
			SqlWord.Year   => DateTimeField.Year,
			SqlWord.Month  => DateTimeField.Month,
			SqlWord.Day    => DateTimeField.Day,
			SqlWord.Hour   => DateTimeField.Hour,
			SqlWord.Minute => DateTimeField.Minute,
			SqlWord.Second => DateTimeField.Second,
			_              => null,
		};

	/// <summary>An <c>&lt;unsigned integer&gt;</c> where the BNF writes one inside a production.</summary>
	static bool Unsigned(ref SqlCursor cursor, out int? value)
	{
		if (cursor.Kind != SqlTokenKind.Number)
		{
			value = null;

			return false;
		}

		if (!IsUnsignedInteger(cursor.Span))
		{
			value = null;

			return false;
		}

		value = Integer(cursor.Span);

		cursor.Take();

		return true;
	}

	/// <summary>
	/// Whether a number is an <c>&lt;unsigned integer&gt;</c> rather than a decimal or an
	/// approximate one: <c>1.5</c> and <c>1E5</c> are neither. A radix other than ten writes its
	/// digits with letters and says so in its first two characters.
	/// </summary>
	static bool IsUnsignedInteger(ReadOnlySpan<char> span)
	{
		if (span.Length > 1 && span[0] == '0' && (span[1] | 0x20) is 'x' or 'o' or 'b')
			return true;

		foreach (var ch in span)
			if (ch == '.' || (ch | 0x20) == 'e')
				return false;

		return true;
	}
}
