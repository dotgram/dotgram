using System;
using System.Globalization;
using System.Text;

namespace DotGram.Finance.Fix44;

static class FixConvert
{
	public static int Tag(ReadOnlySpan<char> value)
	{
		var result = 0;

		if (value.IsEmpty)
			return -1;

		foreach (var c in value)
		{
			var digit = c - '0';

			if (digit < 0 || digit > 9 || result > (int.MaxValue - digit) / 10)
				return -1;

			result = result * 10 + digit;
		}

		return result;
	}

	public static int Tag(ReadOnlySpan<byte> value)
	{
		var result = 0;

		if (value.IsEmpty)
			return -1;

		foreach (var c in value)
		{
			var digit = c - '0';

			if (digit < 0 || digit > 9 || result > (int.MaxValue - digit) / 10)
				return -1;

			result = result * 10 + digit;
		}

		return result;
	}

	public static (bool Valid, long Value) Integer(ReadOnlySpan<char> raw)
	{
		var valid = Integer(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, long Value) Integer(ReadOnlySpan<byte> raw)
	{
		var valid = Integer(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, decimal Value) Decimal(ReadOnlySpan<char> raw)
	{
		var valid = Decimal(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, decimal Value) Decimal(ReadOnlySpan<byte> raw)
	{
		var valid = Decimal(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, bool Value) Boolean(ReadOnlySpan<char> raw)
	{
		var valid = Boolean(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, bool Value) Boolean(ReadOnlySpan<byte> raw)
	{
		var valid = Boolean(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, char Value) Character(ReadOnlySpan<char> raw)
	{
		var valid = Character(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, char Value) Character(ReadOnlySpan<byte> raw)
	{
		var valid = Character(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, ReadOnlyMemory<byte> Value) Data(ReadOnlySpan<char> raw)
	{
		var valid = Data(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, ReadOnlyMemory<byte> Value) Data(ReadOnlySpan<byte> raw)
	{
		var valid = Data(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, DateTimeOffset Value) Timestamp(ReadOnlySpan<char> raw)
	{
		var valid = Timestamp(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, DateTimeOffset Value) Timestamp(ReadOnlySpan<byte> raw)
	{
		var valid = Timestamp(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, TimeOnly Value) Time(ReadOnlySpan<char> raw)
	{
		var valid = Time(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, TimeOnly Value) Time(ReadOnlySpan<byte> raw)
	{
		var valid = Time(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, DateOnly Value) Date(ReadOnlySpan<char> raw)
	{
		var valid = Date(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, DateOnly Value) Date(ReadOnlySpan<byte> raw)
	{
		var valid = Date(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, string Value) MonthYear(ReadOnlySpan<char> raw)
	{
		var valid = MonthYear(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, string Value) MonthYear(ReadOnlySpan<byte> raw)
	{
		var valid = MonthYear(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, string[] Value) Multiple(ReadOnlySpan<char> raw)
	{
		var valid = Multiple(raw, out var value);
		return (valid, value);
	}

	public static (bool Valid, string[] Value) Multiple(ReadOnlySpan<byte> raw)
	{
		var valid = Multiple(raw, out var value);
		return (valid, value);
	}

	public static bool Integer(ReadOnlySpan<char> raw, out long value)
	{
		value = default;

		var start = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;

		if (start == raw.Length)
			return false;

		// Eighteen digits always fit a long, so they are added up without a check. The criterion
		// is the length of the text, so a value with more leading zeros than that takes the
		// checked path below, which is the same loop asking at every digit whether the next
		// still fits.
		if (raw.Length - start <= 18)
		{
			long number = 0;

			for (var i = start; i < raw.Length; i++)
			{
				var digit = raw[i] - '0';

				if (digit < 0 || digit > 9)
					return false;

				number = number * 10 + digit;
			}

			value = start == 1 ? -number : number;

			return true;
		}

		// Added up below zero, so that the one value with no positive twin, long.MinValue, is read.
		long negated = 0;

		for (var i = start; i < raw.Length; i++)
		{
			var digit = raw[i] - '0';

			if (digit < 0 || digit > 9 || negated < (long.MinValue + digit) / 10)
				return false;

			negated = negated * 10 - digit;
		}

		if (start == 0 && negated == long.MinValue)
			return false;

		value = start == 1 ? negated : -negated;

		return true;
	}

	public static bool Integer(ReadOnlySpan<byte> raw, out long value)
	{
		value = default;

		var start = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;

		if (start == raw.Length)
			return false;

		// The reader over characters, digit for digit: eighteen digits unchecked, more of them
		// checked at every step and added up below zero.
		if (raw.Length - start <= 18)
		{
			long number = 0;

			for (var i = start; i < raw.Length; i++)
			{
				var digit = raw[i] - '0';

				if (digit < 0 || digit > 9)
					return false;

				number = number * 10 + digit;
			}

			value = start == 1 ? -number : number;

			return true;
		}

		long negated = 0;

		for (var i = start; i < raw.Length; i++)
		{
			var digit = raw[i] - '0';

			if (digit < 0 || digit > 9 || negated < (long.MinValue + digit) / 10)
				return false;

			negated = negated * 10 - digit;
		}

		if (start == 0 && negated == long.MinValue)
			return false;

		value = start == 1 ? negated : -negated;

		return true;
	}

	public static bool Decimal(ReadOnlySpan<char> raw, out decimal value)
	{
		value = default;

		var start       = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;
		var dot         = -1;
		var lastNonzero = -1;
		var digits      = 0;

		for (var i = start; i < raw.Length; i++)
		{
			if (raw[i] == '.' && dot < 0)
			{
				dot = i;
				continue;
			}

			if (raw[i] < '0' || raw[i] > '9')
				return false;

			if (raw[i] != '0')
				lastNonzero = i;

			digits++;
		}

		if (digits == 0)
			return false;

		// Up to eighteen digits the mantissa fits a long exactly, so the decimal is built from
		// it and its scale, trailing zeros included, with nothing to round. A zero takes the
		// general path, which decides the sign it keeps.
		if (digits <= 18)
		{
			ulong mantissa = 0;

			for (var i = start; i < raw.Length; i++)
				if (i != dot)
					mantissa = mantissa * 10 + (ulong)(raw[i] - '0');

			if (mantissa != 0)
			{
				var scale = dot < 0 ? 0 : raw.Length - dot - 1;

				value = new decimal(unchecked((int)mantissa), (int)(mantissa >> 32), 0, start == 1, (byte)scale);

				return true;
			}
		}

		if (!decimal.TryParse(raw, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
			return false;

		return ExactDecimal(ref value, dot < 0 ? 0 : Math.Max(0, lastNonzero - dot));
	}

	public static bool Decimal(ReadOnlySpan<byte> raw, out decimal value)
	{
		value = default;

		var start       = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;
		var dot         = -1;
		var lastNonzero = -1;
		var digits      = 0;

		for (var i = start; i < raw.Length; i++)
		{
			if (raw[i] == '.' && dot < 0)
			{
				dot = i;
				continue;
			}

			if (raw[i] < '0' || raw[i] > '9')
				return false;

			if (raw[i] != '0')
				lastNonzero = i;

			digits++;
		}

		if (digits == 0)
			return false;

		// Up to eighteen digits the mantissa fits a long exactly, so the decimal is built from
		// it and its scale, trailing zeros included, with nothing to round. A zero takes the
		// general path, which decides the sign it keeps.
		if (digits <= 18)
		{
			ulong mantissa = 0;

			for (var i = start; i < raw.Length; i++)
				if (i != dot)
					mantissa = mantissa * 10 + (ulong)(raw[i] - '0');

			if (mantissa != 0)
			{
				var scale = dot < 0 ? 0 : raw.Length - dot - 1;

				value = new decimal(unchecked((int)mantissa), (int)(mantissa >> 32), 0, start == 1, (byte)scale);

				return true;
			}
		}

		if (!(System.Buffers.Text.Utf8Parser.TryParse(raw, out value, out var consumed) && consumed == raw.Length))
			return false;

		return ExactDecimal(ref value, dot < 0 ? 0 : Math.Max(0, lastNonzero - dot));
	}

	static bool ExactDecimal(ref decimal value, int requiredScale)
	{
		// .NET parsing can round fractional digits to fit decimal. Reject that loss.
#if NETSTANDARD2_0
		var scale = (decimal.GetBits(value)[3] >> 16) & 255;
#else
		Span<int> bits = stackalloc int[4];

		decimal.GetBits(value, bits);

		var scale = (bits[3] >> 16) & 255;
#endif
		if (scale >= requiredScale)
			return true;

		value = default;

		return false;
	}

	static int Part(ReadOnlySpan<char> raw)
	{
		return Tag(raw);
	}

	static int Part(ReadOnlySpan<byte> raw)
	{
		return Tag(raw);
	}

	public static string Text(ReadOnlySpan<char> raw)
	{
		return raw.ToString();
	}

	// Each byte becomes the character with the same code, which is what Latin-1 decodes to.
	// Only the string is allocated: a character array beside it doubled every text field.
	public static string Text(ReadOnlySpan<byte> raw)
	{
#if NETSTANDARD2_0
		Span<char> chars = raw.Length <= 256 ? stackalloc char[raw.Length] : new char[raw.Length];

		for (var i = 0; i < raw.Length; i++)
			chars[i] = (char)raw[i];

		return chars.ToString();
#else
		return Encoding.Latin1.GetString(raw);
#endif
	}

	public static bool Data(ReadOnlySpan<char> raw, out ReadOnlyMemory<byte> value)
	{
		var bytes = new byte[raw.Length];

		for (var i = 0; i < raw.Length; i++)
		{
			if (raw[i] > 255)
			{
				value = default;
				return false;
			}

			bytes[i] = (byte)raw[i];
		}

		value = bytes;

		return true;
	}

	public static bool Data(ReadOnlySpan<byte> raw, out ReadOnlyMemory<byte> value)
	{
		value = raw.ToArray();
		return true;
	}

	static int Days(int year, int month)
	{
		return month == 2 ? (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? 29 : 28) : month is 4 or 6 or 9 or 11 ? 30 : 31;
	}

	public static bool Boolean(ReadOnlySpan<char> raw, out bool value)
	{
		value = raw.Length == 1 && raw[0] == 'Y';
		return raw.Length == 1 && (raw[0] == 'Y' || raw[0] == 'N');
	}

	public static bool Character(ReadOnlySpan<char> raw, out char value)
	{
		value = raw.Length == 1 ? (char)raw[0] : default;
		return raw.Length == 1;
	}

	public static bool Multiple(ReadOnlySpan<char> raw, out string[] value)
	{
		var text = Text(raw);

		value = text.Split(' ');

		foreach (var item in value)
			if (item.Length != 1)
				return false;

		return true;
	}

	// ── Dates and times ─────────────────────────────────────────────────────────
	//
	// A FIX date is YYYYMMDD, a time HH:MM:SS with an optional fraction of any number of digits,
	// and a timestamp the two joined by a dash. They are held in DateOnly, TimeOnly and a
	// DateTimeOffset at offset zero. Two things the wire may say do not fit those: the leap
	// second, 23:59:60, which the protocol admits and which is read as the second after it, so
	// that the value is the instant and only the notation is lost; and the year 0000, which the
	// protocol admits and no calendar type holds, and which is not a value. A fraction is kept
	// to the tick, a hundred nanoseconds; FIX 4.4 writes at most milliseconds.

	public static bool Date(ReadOnlySpan<char> raw, out DateOnly value)
	{
		value = default;

		if (!DateParts(raw, out var year, out var month, out var day))
			return false;

		value = new DateOnly(year, month, day);

		return true;
	}

	public static bool Time(ReadOnlySpan<char> raw, out TimeOnly value)
	{
		value = default;

		if (!TimeParts(raw, out var ticks, out var leap))
			return false;

		value = new TimeOnly(leap ? ticks + TimeSpan.TicksPerSecond - TimeSpan.TicksPerDay : ticks);

		return true;
	}

	public static bool Timestamp(ReadOnlySpan<char> raw, out DateTimeOffset value)
	{
		value = default;

		if (raw.Length < 17 || raw[8] != '-'
			|| !DateParts(raw.Slice(0, 8), out var year, out var month, out var day)
			|| !TimeParts(raw.Slice(9), out var ticks, out var leap))
			return false;

		return Composed(year, month, day, ticks, leap, out value);
	}

	public static bool MonthYear(ReadOnlySpan<char> raw, out string value)
	{
		value = Text(raw);

		if (raw.Length != 6 && raw.Length != 8)
			return false;

		var year  = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));

		if (year < 0 || month < 1 || month > 12)
			return false;

		if (raw.Length == 8)
		{
			if (raw[6] == 'w')
				return raw[7] is >= '1' and <= '5';

			var day = Part(raw.Slice(6, 2));

			return day >= 1 && day <= Days(year, month);
		}

		return true;
	}

	static bool DateParts(ReadOnlySpan<char> raw, out int year, out int month, out int day)
	{
		year = month = day = 0;

		if (raw.Length != 8)
			return false;

		year  = Part(raw.Slice(0, 4));
		month = Part(raw.Slice(4, 2));
		day   = Part(raw.Slice(6, 2));

		return year >= 1 && month >= 1 && month <= 12 && day >= 1 && day <= Days(year, month);
	}

	// The time of day in ticks, and whether it was the leap second, in which case the ticks are
	// those of 23:59:59 plus the fraction, and the caller adds the second where it can.
	static bool TimeParts(ReadOnlySpan<char> raw, out long ticks, out bool leap)
	{
		ticks = 0;
		leap  = false;

		if (raw.Length < 8 || raw[2] != ':' || raw[5] != ':')
			return false;

		var hour   = Part(raw.Slice(0, 2));
		var minute = Part(raw.Slice(3, 2));
		var second = Part(raw.Slice(6, 2));

		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60 || second == 60 && (hour != 23 || minute != 59))
			return false;

		if (raw.Length > 8)
		{
			if (raw.Length < 10 || raw[8] != '.')
				return false;

			var scale = TimeSpan.TicksPerSecond / 10;

			foreach (var c in raw.Slice(9))
			{
				if (c < '0' || c > '9')
					return false;

				ticks += (c - '0') * scale;
				scale /= 10;
			}
		}

		leap   = second == 60;
		ticks += hour * TimeSpan.TicksPerHour + minute * TimeSpan.TicksPerMinute + (leap ? 59 : second) * TimeSpan.TicksPerSecond;

		return true;
	}

	static bool Composed(int year, int month, int day, long ticks, bool leap, out DateTimeOffset value)
	{
		value = default;

		var all = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc).Ticks + ticks + (leap ? TimeSpan.TicksPerSecond : 0);

		if (all > DateTime.MaxValue.Ticks)
			return false;

		value = new DateTimeOffset(all, TimeSpan.Zero);

		return true;
	}

	public static bool Boolean(ReadOnlySpan<byte> raw, out bool value)
	{
		value = raw.Length == 1 && raw[0] == 'Y';
		return raw.Length == 1 && (raw[0] == 'Y' || raw[0] == 'N');
	}

	public static bool Character(ReadOnlySpan<byte> raw, out char value)
	{
		value = raw.Length == 1 ? (char)raw[0] : default;
		return raw.Length == 1;
	}

	public static bool Multiple(ReadOnlySpan<byte> raw, out string[] value)
	{
		var text = Text(raw);

		value = text.Split(' ');

		foreach (var item in value)
			if (item.Length != 1)
				return false;

		return true;
	}

	public static bool Date(ReadOnlySpan<byte> raw, out DateOnly value)
	{
		value = default;

		if (!DateParts(raw, out var year, out var month, out var day))
			return false;

		value = new DateOnly(year, month, day);

		return true;
	}

	public static bool Time(ReadOnlySpan<byte> raw, out TimeOnly value)
	{
		value = default;

		if (!TimeParts(raw, out var ticks, out var leap))
			return false;

		value = new TimeOnly(leap ? ticks + TimeSpan.TicksPerSecond - TimeSpan.TicksPerDay : ticks);

		return true;
	}

	public static bool Timestamp(ReadOnlySpan<byte> raw, out DateTimeOffset value)
	{
		value = default;

		if (raw.Length < 17 || raw[8] != '-'
			|| !DateParts(raw.Slice(0, 8), out var year, out var month, out var day)
			|| !TimeParts(raw.Slice(9), out var ticks, out var leap))
			return false;

		return Composed(year, month, day, ticks, leap, out value);
	}

	public static bool MonthYear(ReadOnlySpan<byte> raw, out string value)
	{
		value = Text(raw);

		if (raw.Length != 6 && raw.Length != 8)
			return false;

		var year  = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));

		if (year < 0 || month < 1 || month > 12)
			return false;

		if (raw.Length == 8)
		{
			if (raw[6] == 'w')
				return raw[7] is >= (byte)'1' and <= (byte)'5';

			var day = Part(raw.Slice(6, 2));

			return day >= 1 && day <= Days(year, month);
		}

		return true;
	}

	static bool DateParts(ReadOnlySpan<byte> raw, out int year, out int month, out int day)
	{
		year = month = day = 0;

		if (raw.Length != 8)
			return false;

		year  = Part(raw.Slice(0, 4));
		month = Part(raw.Slice(4, 2));
		day   = Part(raw.Slice(6, 2));

		return year >= 1 && month >= 1 && month <= 12 && day >= 1 && day <= Days(year, month);
	}

	static bool TimeParts(ReadOnlySpan<byte> raw, out long ticks, out bool leap)
	{
		ticks = 0;
		leap  = false;

		if (raw.Length < 8 || raw[2] != ':' || raw[5] != ':')
			return false;

		var hour   = Part(raw.Slice(0, 2));
		var minute = Part(raw.Slice(3, 2));
		var second = Part(raw.Slice(6, 2));

		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60 || second == 60 && (hour != 23 || minute != 59))
			return false;

		if (raw.Length > 8)
		{
			if (raw.Length < 10 || raw[8] != '.')
				return false;

			var scale = TimeSpan.TicksPerSecond / 10;

			foreach (var c in raw.Slice(9))
			{
				if (c < '0' || c > '9')
					return false;

				ticks += (c - '0') * scale;
				scale /= 10;
			}
		}

		leap   = second == 60;
		ticks += hour * TimeSpan.TicksPerHour + minute * TimeSpan.TicksPerMinute + (leap ? 59 : second) * TimeSpan.TicksPerSecond;

		return true;
	}

}
