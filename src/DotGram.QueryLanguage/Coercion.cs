using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace DotGram.QueryLanguage;

/// <summary>
/// What a value passed with a text may be, for the type its parameter has: a value of that type, or one that
/// converts to it without loss beyond the target type's own precision and without a culture. A string is never
/// a number, a number never a string, a <c>bool</c> never a number; an enum and a <c>char</c> are neither.
/// </summary>
/// <remarks>
/// Strict now is loose later without breaking anyone; the other way round breaks every caller who relied on it.
/// </remarks>
static class Coercion
{
	/// <summary>The value as <paramref name="type"/>, or the reason it is not one.</summary>
	public static bool TryConvert(object value, ScalarType type, out object result)
	{
		result = value;

		switch (type.Type)
		{
			case QlType.Bool:
				return value is bool;

			case QlType.Int:
				return Integer(value, int.MinValue, int.MaxValue, out var small) && Set(out result, (int)small);

			case QlType.Long:
				return Integer(value, long.MinValue, long.MaxValue, out var large) && Set(out result, large);

			case QlType.Decimal:
				return ToDecimal(value, out var exact) && Set(out result, exact);

			case QlType.Double:
				return ToDouble(value, out var real) && Set(out result, real);

			case QlType.String:
				return value is string;

			case QlType.Timestamp:
				if (value is DateTime)
					return true;

				return value is string text && TryTimestamp(text, out var timestamp) && Set(out result, timestamp);

			case QlType.TimestampTz:
				if (value is DateTimeOffset)
					return true;

				if (value is DateTime { Kind: DateTimeKind.Utc } utc)
					return Set(out result, new DateTimeOffset(utc));

				return value is string zoned && TryTimestampTz(zoned, out var offset) && Set(out result, offset);

			case QlType.Guid:
				if (value is Guid)
					return true;

				return value is string id && Guid.TryParseExact(id, "D", out var guid) && Set(out result, guid);

			default:
				// A type outside the language: a value of that type and nothing else, since nothing here knows how
				// to make one from anything else without loss or a culture. The host converts it itself.
				return type.Core.IsInstanceOfType(value);
		}
	}

	static bool Set(out object result, object value)
	{
		result = value;

		return true;
	}

	/// <summary>An integer of any CLR integer type, a <c>decimal</c> with an integer value, or a <c>double</c> that is an integer up to 2^53.</summary>
	static bool Integer(object value, long min, long max, out long result)
	{
		result = 0;

		switch (value)
		{
			case sbyte or byte or short or ushort or int or uint or long:
				result = System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
				break;

			case ulong unsigned:
				if (unsigned > long.MaxValue)
					return false;

				result = (long)unsigned;
				break;

			case decimal exact:
				if (exact != decimal.Truncate(exact) || exact < long.MinValue || exact > long.MaxValue)
					return false;

				result = (long)exact;
				break;

			case double real:
				if (real != Math.Truncate(real) || Math.Abs(real) > TwoTo53)
					return false;

				result = (long)real;
				break;

			default:
				return false;
		}

		return result >= min && result <= max;
	}

	const double TwoTo53 = 9007199254740992d;

	/// <summary>The largest integer every integer below which a double holds exactly: 2^53.</summary>
	const long Exact = 9007199254740992L;

	static bool ToDecimal(object value, out decimal result)
	{
		result = 0;

		switch (value)
		{
			case decimal exact:
				result = exact;
				return true;

			case double real:
			{
				if (double.IsNaN(real) || double.IsInfinity(real))
					return false;

				// The shortest text that reads back as the same double, and the decimal that text says: 0.1 is 0.1m,
				// not 0.1000000000000000055511151231257827m, so that `where price == @p` finds the row on every runtime.
				return decimal.TryParse(Shortest(real), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
			}

			case ulong unsigned:
				result = unsigned;
				return true;

			default:
				if (!Integer(value, long.MinValue, long.MaxValue, out var integer))
					return false;

				result = integer;
				return true;
		}
	}

	/// <summary>The shortest round-trip text of a double: "R" where the runtime gives the shortest, G15 or else G17 where it does not.</summary>
	static string Shortest(double value)
	{
#if NET
		return value.ToString("R", CultureInfo.InvariantCulture);
#else
		var text = value.ToString("G15", CultureInfo.InvariantCulture);

		return double.Parse(text, CultureInfo.InvariantCulture) == value ? text : value.ToString("G17", CultureInfo.InvariantCulture);
#endif
	}

	static bool ToDouble(object value, out double result)
	{
		result = 0;

		switch (value)
		{
			case double real:
				result = real;

				return !double.IsNaN(real) && !double.IsInfinity(real);

			case decimal exact:
				result = (double)exact;
				return true;

			case ulong unsigned:
				if (unsigned > TwoTo53)
					return false;

				result = unsigned;
				return true;

			default:
				// An integer beyond 2^53 is refused: two different ids would become one value.
				if (!Integer(value, long.MinValue, long.MaxValue, out var integer) || integer > Exact || integer < -Exact)
					return false;

				result = integer;
				return true;
		}
	}

	static readonly string[] Unzoned =
	[
		"yyyy'-'MM'-'dd",
		"yyyy'-'MM'-'dd'T'HH':'mm",
		"yyyy'-'MM'-'dd'T'HH':'mm':'ss",
		"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFF",
		"yyyy'-'MM'-'dd' 'HH':'mm",
		"yyyy'-'MM'-'dd' 'HH':'mm':'ss",
		"yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'FFFFFFF",
	];

	static readonly string[] Utc   = Suffixed("'Z'");
	static readonly string[] Zoned = Suffixed("zzz");

	static string[] Suffixed(string suffix)
	{
		var formats = new string[Unzoned.Length - 1];

		for (var i = 1; i < Unzoned.Length; i++)
			formats[i - 1] = Unzoned[i] + suffix;

		return formats;
	}

	/// <summary>An ISO 8601 date and time without an offset (kind unspecified) or with <c>Z</c> (UTC). One with an offset is refused: it would be read in the server's zone.</summary>
	public static bool TryTimestamp(string text, out DateTime value)
	{
		if (DateTime.TryParseExact(text, Unzoned, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
			return true;

		return DateTime.TryParseExact(text, Utc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value);
	}

	/// <summary>An ISO 8601 date and time with an offset or <c>Z</c>; one without is refused, having no offset to keep.</summary>
	public static bool TryTimestampTz(string text, out DateTimeOffset value)
	{
		if (DateTimeOffset.TryParseExact(text, Zoned, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
			return true;

		return DateTimeOffset.TryParseExact(text, Utc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value);
	}

	/// <summary>
	/// The items of a list value, enumerated once and no further than one item past <paramref name="max"/>, so that a
	/// long or endless sequence is refused for its length without being read to its end; null where the value is not
	/// a list (a string and a byte array are not).
	/// </summary>
	public static List<object?>? Items(object value, int max)
	{
		if (value is string or byte[] || value is not IEnumerable items)
			return null;

		var list = new List<object?>();

		foreach (var item in items)
		{
			list.Add(item);

			if (list.Count > max)
				break;
		}

		return list;
	}

	/// <summary>
	/// What a value weighs against <see cref="QlOptions.MaxParameterBytes"/>: a string as UTF-16, a value of the
	/// language's types as its size in memory, a byte array by its length and a char array as UTF-16, any other array
	/// as 16 bytes an item, and any other value of a type outside the language as 16 bytes.
	/// </summary>
	public static long Bytes(object? value)
	{
		return value switch
		{
			null                                   => 0,
			string text                            => 2L * text.Length,
			byte[] bytes                           => bytes.LongLength,
			char[] chars                           => 2L * chars.LongLength,
			Array array                            => 16L * array.LongLength,
			bool                                   => 1,
			int                                    => 4,
			long or double or DateTime             => 8,
			decimal or Guid or DateTimeOffset      => 16,
			_                                      => 16,
		};
	}
}
