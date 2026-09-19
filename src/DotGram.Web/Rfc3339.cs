using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using DotGram;

namespace DotGram.Web;

// RFC 3339, Date and Time on the Internet: Timestamps. The grammar is §5.6's ABNF, and §5.7's
// restrictions are what the ABNF only writes in comments: which day a month has, which hour and
// minute there are, and where a second may be 60. They are a `when` each, over the digits read:
//
//   * A day is within its month, February counting the Gregorian leap years of Appendix C.
//   * An hour is 00 to 23 — §5.7 refuses ISO 8601's 24 — a minute and an offset's minute 00 to 59.
//   * A second of 60 is a leap second, which happens at 23:59:60 UTC: the time with its offset
//     taken off has to be the last minute of the day. Which days had one is the IERS's to say and
//     changes, so no date is asked.
//
// `T` and `Z` may be written lowercase, as §5.6 notes ABNF and ISO 8601 allow. A space in place of
// `T` is an application's choice the RFC mentions and the grammar does not make, so it is refused.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	Digit = ['0'..'9']

	// §5.6's fields, each a rule whose value is the text it read. A capture names one of them
	// rather than a repetition of a digit, so that what a `when` is handed is the whole field;
	// and the fraction is a rule that may read nothing, rather than a capture inside an optional
	// group, whose value would be the text read or null.
	DateFullYear = Digit{4}
	TwoDigits    = Digit{2}
	SecFrac      = ('.' & Digit+)?

	// date-time.
	Timestamp : @Timestamp = date: FullDate & ['T' | 't'] & time: FullTime => @(new Timestamp(date, time))

	// full-date, and §5.7's days of the month.
	FullDate : @FullDate
		= year: DateFullYear & '-' & month: TwoDigits & '-' & day: TwoDigits & when @(Rfc3339.IsDate(year, month, day))
		=> @(new FullDate(Rfc3339.Number(year), Rfc3339.Number(month), Rfc3339.Number(day)))

	// full-time: partial-time and time-offset, and §5.7's hours, minutes and leap second.
	FullTime : @FullTime
		= hour: TwoDigits & ':' & minute: TwoDigits & ':' & second: TwoDigits & fraction: SecFrac & offset: TimeOffset
		  & when @(Rfc3339.IsTime(hour, minute, second, offset))
		=> @(Rfc3339.Time(hour, minute, second, fraction, offset))

	// time-offset. What it means is worked out beside the time it belongs to.
	TimeOffset = ['Z' | 'z'] | ['+' | '-'] & TwoDigits & ':' & TwoDigits

	parse Timestamp as ParseTimestamp
	parse FullDate  as ParseFullDate
	parse FullTime  as ParseFullTime
	""", SpanCaptures = true)]
static partial class Rfc3339
{
	// ParseTimestamp, ParseFullDate, ParseFullTime and their Try forms are generated here; Timestamp.Parse,
	// FullDate.Parse and FullTime.Parse are the ways in.

	// The grammar has read these as digits, and at most four of them.
	internal static int Number(ReadOnlySpan<char> digits)
	{
		var value = 0;

		foreach (var digit in digits)
			value = value * 10 + (digit - '0');

		return value;
	}

	/// <summary>§5.7: a month of the year, and a day within it.</summary>
	internal static bool IsDate(ReadOnlySpan<char> year, ReadOnlySpan<char> month, ReadOnlySpan<char> day)
	{
		var y = Number(year);
		var m = Number(month);
		var d = Number(day);

		if (m is < 1 or > 12 || d < 1)
			return false;

		// Appendix C, the Gregorian rule.
		var leap = y % 4 == 0 && (y % 100 != 0 || y % 400 == 0);

		return d <= m switch
		{
			2               => leap ? 29 : 28,
			4 or 6 or 9 or 11 => 30,
			_               => 31,
		};
	}

	/// <summary>§5.7: an hour, a minute and an offset in range, and a leap second only at 23:59 UTC.</summary>
	internal static bool IsTime(ReadOnlySpan<char> hour, ReadOnlySpan<char> minute, ReadOnlySpan<char> second, ReadOnlySpan<char> offset)
	{
		var h = Number(hour);
		var m = Number(minute);
		var s = Number(second);

		if (h > 23 || m > 59 || s > 60)
			return false;

		var shift = 0;

		if (offset.Length > 1)
		{
			var offsetHour   = Number(offset.Slice(1, 2));
			var offsetMinute = Number(offset.Slice(4, 2));

			if (offsetHour > 23 || offsetMinute > 59)
				return false;

			shift = (offset[0] == '-' ? -1 : 1) * (offsetHour * 60 + offsetMinute);
		}

		if (s < 60)
			return true;

		// The minute of the day in UTC, which a leap second has to be the last of.
		var utc = ((h * 60 + m - shift) % 1440 + 1440) % 1440;

		return utc == 23 * 60 + 59;
	}

	/// <param name="fraction">The fraction with its point, or nothing where none was written.</param>
	internal static FullTime Time(ReadOnlySpan<char> hour, ReadOnlySpan<char> minute, ReadOnlySpan<char> second, ReadOnlySpan<char> fraction, ReadOnlySpan<char> offset)
	{
		var digits = fraction.Length == 0 ? null : fraction.Slice(1).ToString();

		if (offset.Length == 1)
			return new FullTime(Number(hour), Number(minute), Number(second), digits, TimeSpan.Zero, false);

		var size    = new TimeSpan(Number(offset.Slice(1, 2)), Number(offset.Slice(4, 2)), 0);
		var unknown = offset.SequenceEqual("-00:00".AsSpan());

		return new FullTime(Number(hour), Number(minute), Number(second), digits, offset[0] == '-' ? -size : size, unknown);
	}
}

/// <summary>A date-time (RFC 3339 §5.6): a full-date, and a full-time on it.</summary>
public sealed record Timestamp(FullDate Date, FullTime Time)
{
	/// <summary>A date-time, with §5.7's days, hours, minutes and leap second.</summary>
	/// <exception cref="FormatException">The text is no date-time, or names a time that is not; the message says where.</exception>
	public static Timestamp Parse(string text) =>
		Rfc3339.ParseTimestamp(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A date-time, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out Timestamp? timestamp)
	{
		var match = Rfc3339.TryParseTimestamp(text ?? throw new ArgumentNullException(nameof(text)));

		timestamp = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>The instant, as .NET holds one.</summary>
	/// <remarks>
	/// A fraction finer than a tick is cut off, since a tick is as fine as .NET goes. An unknown local
	/// offset (<c>-00:00</c>) is taken as UTC, which is the instant the RFC says it stands for (§4.3).
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// The second is a leap second, which <see cref="DateTimeOffset"/> has no way to hold.
	/// </exception>
	public DateTimeOffset ToDateTimeOffset()
	{
		if (Time.Second == 60)
			throw new InvalidOperationException("A leap second has no DateTimeOffset: the second after 59 is the next minute's 00.");

		var ticks = 0L;

		if (Time.Fraction is { } fraction)
			for (var at = 0; at < 7; at++)
				ticks = ticks * 10 + (at < fraction.Length ? fraction[at] - '0' : 0);

		return new DateTimeOffset(Date.Year, Date.Month, Date.Day, Time.Hour, Time.Minute, Time.Second, Time.Offset)
			.AddTicks(ticks);
	}

	/// <summary>The date-time as §5.6 writes it, with an uppercase <c>T</c> and <c>Z</c> as §5.6 says to.</summary>
	public override string ToString() => $"{Date}T{Time}";
}

/// <summary>A full-date (RFC 3339 §5.6): a day of the Gregorian calendar.</summary>
public sealed record FullDate(int Year, int Month, int Day)
{
	/// <summary>A full-date, a day its month has.</summary>
	/// <exception cref="FormatException">The text is no full-date, or names a day that is not; the message says where.</exception>
	public static FullDate Parse(string text) =>
		Rfc3339.ParseFullDate(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A full-date, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out FullDate? date)
	{
		var match = Rfc3339.TryParseFullDate(text ?? throw new ArgumentNullException(nameof(text)));

		date = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	public override string ToString() =>
		string.Format(CultureInfo.InvariantCulture, "{0:D4}-{1:D2}-{2:D2}", Year, Month, Day);
}

/// <summary>A full-time (RFC 3339 §5.6): a time of day and the offset from UTC it was written in.</summary>
/// <param name="Second">00 to 59, or 60 for a leap second.</param>
/// <param name="Fraction">The digits after the point as written, however many, or null.</param>
/// <param name="Offset">What to add to UTC to get this time: zero for <c>Z</c>, <c>+00:00</c> and <c>-00:00</c>.</param>
/// <param name="LocalOffsetUnknown">
/// Whether the offset was written <c>-00:00</c>: the time is UTC and the local offset was not known (§4.3).
/// </param>
public sealed record FullTime(int Hour, int Minute, int Second, string? Fraction, TimeSpan Offset, bool LocalOffsetUnknown)
{
	/// <summary>A full-time, a leap second only at 23:59 UTC.</summary>
	/// <exception cref="FormatException">The text is no full-time, or names a time that is not; the message says where.</exception>
	public static FullTime Parse(string text) =>
		Rfc3339.ParseFullTime(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A full-time, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out FullTime? time)
	{
		var match = Rfc3339.TryParseFullTime(text ?? throw new ArgumentNullException(nameof(text)));

		time = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	public override string ToString()
	{
		var text = string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", Hour, Minute, Second);

		if (Fraction is not null)
			text += "." + Fraction;

		if (LocalOffsetUnknown)
			return text + "-00:00";

		if (Offset == TimeSpan.Zero)
			return text + "Z";

		var size = Offset.Duration();

		return text + (Offset < TimeSpan.Zero ? "-" : "+") +
			string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", size.Hours, size.Minutes);
	}
}
