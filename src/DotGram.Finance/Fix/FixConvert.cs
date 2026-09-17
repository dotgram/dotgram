using System;
using System.Globalization;
using System.Numerics;

namespace DotGram.Finance.Fix;

static class FixConvert
{
	public static int Tag(ReadOnlySpan<char> value)
	{
		return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var tag) ? tag : -1;
	}

	public static int Tag(ReadOnlySpan<byte> value)
	{
		var result = 0;
		if (value.IsEmpty) return -1;
		foreach (var c in value)
		{
			var digit = c - '0';
			if (digit < 0 || digit > 9 || result > (int.MaxValue - digit) / 10) return -1;
			result = result * 10 + digit;
		}
		return result;
	}

	public static (bool Valid, BigInteger Value) Integer(ReadOnlySpan<char> raw)
	{
		var valid = Integer(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, BigInteger Value) Integer(ReadOnlySpan<byte> raw)
	{
		var valid = Integer(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixDecimal Value) Decimal(ReadOnlySpan<char> raw)
	{
		var valid = Decimal(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixDecimal Value) Decimal(ReadOnlySpan<byte> raw)
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
	public static (bool Valid, FixTimestamp Value) Timestamp(ReadOnlySpan<char> raw)
	{
		var valid = Timestamp(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixTimestamp Value) Timestamp(ReadOnlySpan<byte> raw)
	{
		var valid = Timestamp(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixTime Value) Time(ReadOnlySpan<char> raw)
	{
		var valid = Time(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixTime Value) Time(ReadOnlySpan<byte> raw)
	{
		var valid = Time(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixDate Value) Date(ReadOnlySpan<char> raw)
	{
		var valid = Date(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixDate Value) Date(ReadOnlySpan<byte> raw)
	{
		var valid = Date(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixMonthYear Value) MonthYear(ReadOnlySpan<char> raw)
	{
		var valid = MonthYear(raw, out var value);
		return (valid, value);
	}
	public static (bool Valid, FixMonthYear Value) MonthYear(ReadOnlySpan<byte> raw)
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
	public static bool Integer(ReadOnlySpan<char> raw, out BigInteger value)
	{
		value = default;
		var start = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;
		if (start == raw.Length) return false;
		for (var i = start; i < raw.Length; i++) if (raw[i] < '0' || raw[i] > '9') return false;
#if NETSTANDARD2_0
		return BigInteger.TryParse(raw.ToString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
#else
		return BigInteger.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
#endif
	}
	public static bool Integer(ReadOnlySpan<byte> raw, out BigInteger value)
	{
		value = BigInteger.Zero;
		var negative = !raw.IsEmpty && raw[0] == '-';
		var start = negative ? 1 : 0;
		if (start == raw.Length) return false;
		// Nine digits per BigInteger operation; no text decoding or decimal rounding.
		uint chunk = 0;
		var digits = 0;
		for (var i = start; i < raw.Length; i++)
		{
			var digit = raw[i] - '0';
			if (digit < 0 || digit > 9) return false;
			chunk = chunk * 10 + (uint)digit;
			if (++digits == 9) { value = value * 1000000000 + chunk; chunk = 0; digits = 0; }
		}
		uint factor = 1;
		for (var i = 0; i < digits; i++) factor *= 10;
		value = value * factor + chunk;
		if (negative) value = -value;
		return true;
	}
	public static bool Decimal(ReadOnlySpan<char> raw, out FixDecimal value)
	{
		value = default;
		var dot = raw.IndexOf('.');
		if (dot < 0) { var valid = Integer(raw, out var integer); value = new FixDecimal(integer, 0); return valid; }
		var start = !raw.IsEmpty && raw[0] == '-' ? 1 : 0;
		if (raw.Length - start < 2 || dot < start || raw.Slice(dot + 1).IndexOf('.') >= 0) return false;
		for (var i = start; i < raw.Length; i++) if (i != dot && (raw[i] < '0' || raw[i] > '9')) return false;
		var digits = raw.Slice(0, dot).ToString() + raw.Slice(dot + 1).ToString();
		if (!Integer(digits.AsSpan(), out var coefficient)) return false;
		value = new FixDecimal(coefficient, raw.Length - dot - 1);
		return true;
	}
	public static bool Decimal(ReadOnlySpan<byte> raw, out FixDecimal value)
	{
		value = default;
		var dot = raw.IndexOf((byte)'.');
		if (dot < 0) { var valid = Integer(raw, out var integer); value = new FixDecimal(integer, 0); return valid; }
		var negative = !raw.IsEmpty && raw[0] == '-';
		var start = negative ? 1 : 0;
		if (raw.Length - start < 2 || dot < start) return false;
		var coefficient = BigInteger.Zero;
		for (var i = start; i < raw.Length; i++)
		{
			if (i == dot) continue;
			var digit = raw[i] - '0';
			if (digit < 0 || digit > 9) return false;
			coefficient = coefficient * 10 + digit;
		}
		value = new FixDecimal(negative ? -coefficient : coefficient, raw.Length - dot - 1);
		return true;
	}
	static int Part(ReadOnlySpan<char> raw) => int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : -1;
	static int Part(ReadOnlySpan<byte> raw) => FixConvert.Tag(raw);
	public static string Text(ReadOnlySpan<char> raw) => raw.ToString();
	public static string Text(ReadOnlySpan<byte> raw)
	{
		var chars = new char[raw.Length];
		for (var i = 0; i < raw.Length; i++) chars[i] = (char)raw[i];
		return new string(chars);
	}
	public static bool Data(ReadOnlySpan<char> raw, out ReadOnlyMemory<byte> value)
	{
		var bytes = new byte[raw.Length];
		for (var i = 0; i < raw.Length; i++) { if (raw[i] > 255) { value = default; return false; } bytes[i] = (byte)raw[i]; }
		value = bytes;
		return true;
	}
	public static bool Data(ReadOnlySpan<byte> raw, out ReadOnlyMemory<byte> value) { value = raw.ToArray(); return true; }
	static int Days(int year, int month) => month == 2 ? (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? 29 : 28) : month is 4 or 6 or 9 or 11 ? 30 : 31;
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
		foreach (var item in value) if (item.Length != 1) return false;
		return true;
	}
	public static bool Date(ReadOnlySpan<char> raw, out FixDate value)
	{
		value = default;
		if (raw.Length != 8) return false;
		var year = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));
		var day = Part(raw.Slice(6, 2));
		if (year < 0 || month < 1 || month > 12 || day < 1 || day > Days(year, month)) return false;
		value = new FixDate(year, month, day);
		return true;
	}
	public static bool Time(ReadOnlySpan<char> raw, out FixTime value)
	{
		value = default;
		if (raw.Length < 8 || raw[2] != ':' || raw[5] != ':') return false;
		var hour = Part(raw.Slice(0, 2));
		var minute = Part(raw.Slice(3, 2));
		var second = Part(raw.Slice(6, 2));
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60 || second == 60 && (hour != 23 || minute != 59)) return false;
		var fraction = "";
		if (raw.Length > 8)
		{
			if (raw.Length < 10 || raw[8] != '.') return false;
			foreach (var c in raw.Slice(9)) if (c < '0' || c > '9') return false;
			fraction = Text(raw.Slice(9));
		}
		value = new FixTime(hour, minute, second, fraction);
		return true;
	}
	public static bool Timestamp(ReadOnlySpan<char> raw, out FixTimestamp value)
	{
		value = default;
		if (raw.Length < 17 || raw[8] != '-' || !Date(raw.Slice(0, 8), out var date) || !Time(raw.Slice(9), out var time)) return false;
		value = new FixTimestamp(date, time);
		return true;
	}
	public static bool MonthYear(ReadOnlySpan<char> raw, out FixMonthYear value)
	{
		value = default;
		if (raw.Length != 6 && raw.Length != 8) return false;
		var year = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));
		if (year < 0 || month < 1 || month > 12) return false;
		int? day = null, week = null;
		if (raw.Length == 8)
		{
			if (raw[6] == 'w') { week = raw[7] - '0'; if (week < 1 || week > 5) return false; }
			else { day = Part(raw.Slice(6, 2)); if (day < 1 || day > Days(year, month)) return false; }
		}
		value = new FixMonthYear(year, month, day, week);
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
		foreach (var item in value) if (item.Length != 1) return false;
		return true;
	}
	public static bool Date(ReadOnlySpan<byte> raw, out FixDate value)
	{
		value = default;
		if (raw.Length != 8) return false;
		var year = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));
		var day = Part(raw.Slice(6, 2));
		if (year < 0 || month < 1 || month > 12 || day < 1 || day > Days(year, month)) return false;
		value = new FixDate(year, month, day);
		return true;
	}
	public static bool Time(ReadOnlySpan<byte> raw, out FixTime value)
	{
		value = default;
		if (raw.Length < 8 || raw[2] != ':' || raw[5] != ':') return false;
		var hour = Part(raw.Slice(0, 2));
		var minute = Part(raw.Slice(3, 2));
		var second = Part(raw.Slice(6, 2));
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60 || second == 60 && (hour != 23 || minute != 59)) return false;
		var fraction = "";
		if (raw.Length > 8)
		{
			if (raw.Length < 10 || raw[8] != '.') return false;
			foreach (var c in raw.Slice(9)) if (c < '0' || c > '9') return false;
			fraction = Text(raw.Slice(9));
		}
		value = new FixTime(hour, minute, second, fraction);
		return true;
	}
	public static bool Timestamp(ReadOnlySpan<byte> raw, out FixTimestamp value)
	{
		value = default;
		if (raw.Length < 17 || raw[8] != '-' || !Date(raw.Slice(0, 8), out var date) || !Time(raw.Slice(9), out var time)) return false;
		value = new FixTimestamp(date, time);
		return true;
	}
	public static bool MonthYear(ReadOnlySpan<byte> raw, out FixMonthYear value)
	{
		value = default;
		if (raw.Length != 6 && raw.Length != 8) return false;
		var year = Part(raw.Slice(0, 4));
		var month = Part(raw.Slice(4, 2));
		if (year < 0 || month < 1 || month > 12) return false;
		int? day = null, week = null;
		if (raw.Length == 8)
		{
			if (raw[6] == 'w') { week = raw[7] - '0'; if (week < 1 || week > 5) return false; }
			else { day = Part(raw.Slice(6, 2)); if (day < 1 || day > Days(year, month)) return false; }
		}
		value = new FixMonthYear(year, month, day, week);
		return true;
	}
}
