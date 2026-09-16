using System;

namespace DotGram.Finance.Fix;

static class FixPrimitives
{
	public static bool Valid(FixFieldView field, string? type, string[]? codes)
	{
		var value = field.Value;
		if (type == null) return false;
		if (type == "data") return true;
		if (value.IsEmpty) return false;
		foreach (var c in value)
			if (c < 32 || c > 126) return false;
		var valid = type switch
		{
			"int" => Numeric(value, false, true),
			"Length" or "NumInGroup" => Numeric(value, false, false),
			"SeqNum" or "TagNum" => Numeric(value, false, false) && Nonzero(value),
			"DayOfMonth" => Numeric(value, false, false) && FixContext.Tag(value) is >= 1 and <= 31,
			"float" or "Qty" or "Price" or "PriceOffset" or "Amt" or "Percentage" => Numeric(value, true, true),
			"char" => value.Length == 1,
			"Boolean" => value.Length == 1 && value[0] is 'Y' or 'N',
			"Currency" => Letters(value, 3),
			"Country" => Letters(value, 2),
			"Exchange" => Market(value),
			"UTCTimestamp" => value.Length >= 17 && Date(value.Slice(0, 8)) && value[8] == '-' && Time(value.Slice(9)),
			"UTCTimeOnly" => Time(value),
			"UTCDateOnly" or "LocalMktDate" => Date(value),
			"MonthYear" => MonthYear(value),
			"MultipleValueString" => Multiple(value),
			"String" => true,
			_ => false,
		};
		if (!valid || codes == null) return valid;
		// IOIQty explicitly permits either a numeric quantity or a relative-size code.
		if (field.Tag == 27 && Numeric(value, true, false)) return true;
		if (type == "MultipleValueString")
		{
			for (var i = 0; i < value.Length; i += 2)
				if (!Code(value.Slice(i, 1), codes)) return false;
			return true;
		}
		return Code(value, codes);
	}

	static bool Code(ReadOnlySpan<char> value, string[] codes)
	{
		foreach (var code in codes)
			if (value.SequenceEqual(code.AsSpan())) return true;
		return false;
	}

	static bool Numeric(ReadOnlySpan<char> value, bool fractional, bool signed)
	{
		var start = signed && value.Length > 0 && value[0] == '-' ? 1 : 0;
		var digits = 0;
		var point = false;
		for (var i = start; i < value.Length; i++)
		{
			if (value[i] is >= '0' and <= '9') { digits++; continue; }
			if (fractional && !point && value[i] == '.') { point = true; continue; }
			return false;
		}
		return digits > 0;
	}

	static bool Nonzero(ReadOnlySpan<char> value)
	{
		foreach (var c in value) if (c != '0') return true;
		return false;
	}

	static bool Letters(ReadOnlySpan<char> value, int length)
	{
		if (value.Length != length) return false;
		foreach (var c in value) if (c < 'A' || c > 'Z') return false;
		return true;
	}

	static bool Multiple(ReadOnlySpan<char> value)
	{
		if ((value.Length & 1) == 0) return false;
		for (var i = 0; i < value.Length; i++)
			if ((i & 1) == 1 ? value[i] != ' ' : value[i] == ' ') return false;
		return true;
	}

	static bool Market(ReadOnlySpan<char> value)
	{
		if (value.Length != 4) return false;
		foreach (var c in value)
			if (!(c is >= 'A' and <= 'Z' or >= '0' and <= '9')) return false;
		return true;
	}

	static int Digits(ReadOnlySpan<char> value)
	{
		var result = 0;
		foreach (var c in value)
		{
			if (c < '0' || c > '9') return -1;
			result = result * 10 + c - '0';
		}
		return result;
	}

	static bool Date(ReadOnlySpan<char> value)
	{
		if (value.Length != 8) return false;
		var year = Digits(value.Slice(0, 4));
		var month = Digits(value.Slice(4, 2));
		var day = Digits(value.Slice(6, 2));
		if (year < 0 || month < 1 || month > 12 || day < 1) return false;
		var days = month == 2 ? (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? 29 : 28) : month is 4 or 6 or 9 or 11 ? 30 : 31;
		return day <= days;
	}

	static bool Time(ReadOnlySpan<char> value)
	{
		if (value.Length != 8 && value.Length != 12) return false;
		if (value[2] != ':' || value[5] != ':') return false;
		var hour = Digits(value.Slice(0, 2));
		var minute = Digits(value.Slice(3, 2));
		var second = Digits(value.Slice(6, 2));
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60) return false;
		if (second == 60 && (hour != 23 || minute != 59)) return false;
		return value.Length == 8 || value[8] == '.' && Digits(value.Slice(9, 3)) >= 0;
	}

	static bool MonthYear(ReadOnlySpan<char> value)
	{
		if (value.Length != 6 && value.Length != 8) return false;
		var year = Digits(value.Slice(0, 4));
		var month = Digits(value.Slice(4, 2));
		if (year < 0 || month < 1 || month > 12) return false;
		return value.Length == 6 || value[6] == 'w' && value[7] is >= '1' and <= '5' || Date(value);
	}
}
