using System;

using DotGram.Web;

namespace DotGram.Handwritten.Web;

/// <summary>
/// RFC 3339 timestamps, read by hand into the same <see cref="Timestamp"/>,
/// <see cref="FullDate"/> and <see cref="FullTime"/> the generated <c>Rfc3339</c> grammar builds.
/// </summary>
/// <remarks>
/// <para>
/// It reads exactly what the grammar reads (docs/design/architecture-decisions.md, D1): the
/// same accepted and refused text, the same values, and a refusal at the same position — where
/// a character is wrong, or, for a date or time that is well formed but does not exist, just
/// after it, where the grammar asks §5.7's questions.
/// </para>
/// <para>
/// §5.7's rules are written out here again rather than borrowed from the grammar's helpers, so
/// that the two agreeing means something.
/// </para>
/// </remarks>
public static class HandDateTime
{
	/// <summary>A date-time, or false with the position where the text stops being one.</summary>
	public static bool TryParseTimestamp(string text, out Timestamp? timestamp, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var at = 0;

		timestamp = null;

		if (!ReadDate(text, ref at, out var date, out failure) || !Separator(text, ref at, out failure) ||
			!ReadTime(text, ref at, out var time, out failure) || !End(text, at, out failure))
			return false;

		timestamp = new Timestamp(date, time);

		return true;
	}

	/// <summary>A full-date, or false with the position where the text stops being one.</summary>
	public static bool TryParseFullDate(string text, out FullDate? date, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var at = 0;

		date = null;

		if (!ReadDate(text, ref at, out var read, out failure) || !End(text, at, out failure))
			return false;

		date = read;

		return true;
	}

	/// <summary>A full-time, or false with the position where the text stops being one.</summary>
	public static bool TryParseFullTime(string text, out FullTime? time, out int failure)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var at = 0;

		time = null;

		if (!ReadTime(text, ref at, out var read, out failure) || !End(text, at, out failure))
			return false;

		time = read;

		return true;
	}

	// ── §5.6 ────────────────────────────────────────────────────────────────────

	/// <summary><c>full-date</c>: yyyy-mm-dd, a day its month has.</summary>
	static bool ReadDate(string text, ref int at, out FullDate date, out int failure)
	{
		date = null!;

		if (!Digits(text, ref at, 4, out var year, out failure) || !Is(text, ref at, '-', out failure) ||
			!Digits(text, ref at, 2, out var month, out failure) || !Is(text, ref at, '-', out failure) ||
			!Digits(text, ref at, 2, out var day, out failure))
			return false;

		if (month < 1 || month > 12 || day < 1 || day > DaysIn(year, month))
		{
			failure = at;

			return false;
		}

		date = new FullDate(year, month, day);

		return true;
	}

	/// <summary><c>full-time</c>: hh:mm:ss, a fraction if there is one, and an offset.</summary>
	static bool ReadTime(string text, ref int at, out FullTime time, out int failure)
	{
		time = null!;

		if (!Digits(text, ref at, 2, out var hour, out failure) || !Is(text, ref at, ':', out failure) ||
			!Digits(text, ref at, 2, out var minute, out failure) || !Is(text, ref at, ':', out failure) ||
			!Digits(text, ref at, 2, out var second, out failure))
			return false;

		// A point with no digit after it is no fraction, and nothing else may stand there, so it
		// is refused at the digit it lacks.
		string? fraction = null;

		if (At(text, at) == '.')
		{
			var start = at + 1;
			var end   = start;

			while (IsDigit(At(text, end)))
				end++;

			if (end == start)
			{
				failure = start;

				return false;
			}

			fraction = text.Substring(start, end - start);
			at       = end;
		}

		var offset  = TimeSpan.Zero;
		var unknown = false;

		switch (At(text, at))
		{
			case 'Z' or 'z':
				at++;
				break;

			case '+' or '-':
				var sign = text[at++];

				if (!Digits(text, ref at, 2, out var offsetHour, out failure) || !Is(text, ref at, ':', out failure) ||
					!Digits(text, ref at, 2, out var offsetMinute, out failure))
					return false;

				if (offsetHour > 23 || offsetMinute > 59)
				{
					failure = at;

					return false;
				}

				offset  = new TimeSpan(offsetHour, offsetMinute, 0);
				offset  = sign == '-' ? -offset : offset;
				unknown = sign == '-' && offset == TimeSpan.Zero;
				break;

			default:
				failure = at;

				return false;
		}

		// §5.7: a leap second is the last second of a UTC day, so with the offset taken off the
		// time has to be 23:59.
		var utcMinute = ((hour * 60 + minute - (int)offset.TotalMinutes) % 1440 + 1440) % 1440;

		if (hour > 23 || minute > 59 || second > 60 || second == 60 && utcMinute != 23 * 60 + 59)
		{
			failure = at;

			return false;
		}

		time = new FullTime(hour, minute, second, fraction, offset, unknown);

		return true;
	}

	// ── §5.7 and Appendix C ─────────────────────────────────────────────────────

	static int DaysIn(int year, int month)
	{
		switch (month)
		{
			case 2:
				return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? 29 : 28;

			case 4 or 6 or 9 or 11:
				return 30;

			default:
				return 31;
		}
	}

	// ── Reading ─────────────────────────────────────────────────────────────────

	/// <summary>Exactly <paramref name="count"/> digits, as a number.</summary>
	static bool Digits(string text, ref int at, int count, out int value, out int failure)
	{
		value = 0;

		for (var end = at + count; at < end; at++)
		{
			if (!IsDigit(At(text, at)))
			{
				failure = at;

				return false;
			}

			value = value * 10 + (text[at] - '0');
		}

		failure = -1;

		return true;
	}

	/// <summary>The 'T' between a date and a time, in either case.</summary>
	static bool Separator(string text, ref int at, out int failure)
	{
		if (At(text, at) is 'T' or 't')
		{
			at++;
			failure = -1;

			return true;
		}

		failure = at;

		return false;
	}

	static bool Is(string text, ref int at, char c, out int failure)
	{
		if (At(text, at) == c)
		{
			at++;
			failure = -1;

			return true;
		}

		failure = at;

		return false;
	}

	static bool End(string text, int at, out int failure)
	{
		failure = at < text.Length ? at : -1;

		return at == text.Length;
	}

	static bool IsDigit(char c)
	{
		return c is >= '0' and <= '9';
	}

	/// <summary>The character at a position, or NUL past the end, which nothing accepts.</summary>
	static char At(string text, int position)
	{
		return position < text.Length ? text[position] : '\0';
	}
}
