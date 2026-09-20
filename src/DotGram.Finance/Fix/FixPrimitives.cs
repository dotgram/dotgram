using System;

namespace DotGram.Finance.Fix;

static class FixPrimitives
{
	/// <summary>
	/// The type codes, as <c>FixSchema.TypeCode</c> answers them, plus the two a dictionary can
	/// name that the standard's own tables never carry. Nested so that <c>Types.String</c> is a
	/// constant and not the type of the same name.
	/// </summary>
	public static class Types
	{
		public const byte
			Amt = 1, Boolean = 2, Char = 3, Country = 4, Currency = 5, Data = 6, Exchange = 7,
			Float = 8, Int = 9, Length = 10, LocalMktDate = 11, MonthYear = 12,
			MultipleValueString = 13, NumInGroup = 14, Percentage = 15, Price = 16,
			PriceOffset = 17, Qty = 18, SeqNum = 19, String = 20, UTCDateOnly = 21,
			UTCTimeOnly = 22, UTCTimestamp = 23, TagNum = 24, DayOfMonth = 25;
	}

	/// <summary>The code of a type named as this package spells it, or zero for one it does not model.</summary>
	/// <remarks>
	/// Only a loaded dictionary needs this, and it needs it once a tag rather than once a field:
	/// the name is the file's and the code is what every check reads.
	/// </remarks>
	public static byte Code(string? type) => type switch
	{
		"Amt"                 => Types.Amt,
		"Boolean"             => Types.Boolean,
		"char"                => Types.Char,
		"Country"             => Types.Country,
		"Currency"            => Types.Currency,
		"data"                => Types.Data,
		"Exchange"            => Types.Exchange,
		"float"               => Types.Float,
		"int"                 => Types.Int,
		"Length"              => Types.Length,
		"LocalMktDate"        => Types.LocalMktDate,
		"MonthYear"           => Types.MonthYear,
		"MultipleValueString" => Types.MultipleValueString,
		"NumInGroup"          => Types.NumInGroup,
		"Percentage"          => Types.Percentage,
		"Price"               => Types.Price,
		"PriceOffset"         => Types.PriceOffset,
		"Qty"                 => Types.Qty,
		"SeqNum"              => Types.SeqNum,
		"String"              => Types.String,
		"UTCDateOnly"         => Types.UTCDateOnly,
		"UTCTimeOnly"         => Types.UTCTimeOnly,
		"UTCTimestamp"        => Types.UTCTimestamp,
		"TagNum"              => Types.TagNum,
		"DayOfMonth"          => Types.DayOfMonth,
		_                     => 0,
	};

	/// <summary>Whether a value fits its tag's type, and is one of its code set where it has one.</summary>
	/// <remarks>
	/// The type arrives as a code and not as a name. The schema holds it as a byte, and spelling it
	/// so that this could parse the spelling back was a string switch on every field of every
	/// message — work that only existed because the two halves disagreed about the currency.
	/// </remarks>
	public static bool Valid(FixFieldView field, byte type, string[]? codes)
	{
		var value = field.Value;
		if (type == 0) return false;
		if (type == Types.Data) return true;
		if (value.IsEmpty) return false;
		foreach (var c in value)
			if (c < 32 || c > 126) return false;
		var valid = type switch
		{
			Types.Int => Numeric(value, false, true),
			Types.Length or Types.NumInGroup => Numeric(value, false, false),
			Types.SeqNum or Types.TagNum => Numeric(value, false, false) && Nonzero(value),
			Types.DayOfMonth => Numeric(value, false, false) && FixConvert.Tag(value) is >= 1 and <= 31,
			Types.Float or Types.Qty or Types.Price or Types.PriceOffset or Types.Amt or Types.Percentage => Numeric(value, true, true),
			Types.Char => value.Length == 1,
			Types.Boolean => value.Length == 1 && value[0] is 'Y' or 'N',
			Types.Currency => Letters(value, 3),
			Types.Country => Letters(value, 2),
			Types.Exchange => Market(value),
			Types.UTCTimestamp => value.Length >= 17 && Date(value.Slice(0, 8)) && value[8] == '-' && Time(value.Slice(9)),
			Types.UTCTimeOnly => Time(value),
			Types.UTCDateOnly or Types.LocalMktDate => Date(value),
			Types.MonthYear => MonthYear(value),
			Types.MultipleValueString => Multiple(value),
			Types.String => true,
			_ => false,
		};
		if (!valid || codes == null) return valid;
		// IOIQty explicitly permits either a numeric quantity or a relative-size code.
		if (field.Tag == 27 && Numeric(value, true, false)) return true;
		if (type == Types.MultipleValueString)
		{
			for (var i = 0; i < value.Length; i += 2)
				if (!Code(value.Slice(i, 1), codes)) return false;
			return true;
		}
		return Code(value, codes);
	}

	// Left as a plain scan on purpose. Filtering on the length and the first character before
	// comparing was tried on 2026-09-20 and measured SLOWER — 9,230 ns to 9,355 ns on a 391-field
	// ExecutionReport — because the sets a message actually meets are short and SequenceEqual is
	// already cheap, so the extra branches cost more than the comparisons they skipped.
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
