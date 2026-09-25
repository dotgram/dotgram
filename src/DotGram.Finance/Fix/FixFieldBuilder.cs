using System;

namespace DotGram.Finance.Fix;

/// <summary>The type of a field's value, which is the class the field is built as.</summary>
enum FixValueType : byte
{
	/// <summary>Nothing defines the tag: the field is a <see cref="FixField.Invalid"/>.</summary>
	None,
	Text,
	Character,
	Boolean,
	Integer,
	Decimal,
	Timestamp,
	Time,
	Date,
	MonthYear,
	Multiple,
	Data,
}

static class FixFieldBuilder
{
	public static FixField Value(int tag, FixValueType type, ReadOnlySpan<char> value)
	{
		return type switch
		{
			FixValueType.Text      => new FixField.Text     (tag, value.ToText()),
			FixValueType.Character => new FixField.Character(tag, value.ToCharacter()),
			FixValueType.Boolean   => new FixField.Boolean  (tag, value.ToBoolean()),
			FixValueType.Integer   => new FixField.Integer  (tag, value.ToInteger()),
			FixValueType.Decimal   => new FixField.Decimal  (tag, value.ToDecimal()),
			FixValueType.Timestamp => new FixField.Timestamp(tag, value.ToTimestamp()),
			FixValueType.Time      => new FixField.Time     (tag, value.ToTime()),
			FixValueType.Date      => new FixField.Date     (tag, value.ToDate()),
			FixValueType.MonthYear => new FixField.MonthYear(tag, value.ToMonthYear()),
			FixValueType.Multiple  => new FixField.Multiple (tag, value.ToMultiple()),
			FixValueType.Data      => new FixField.Data     (tag, value.ToData()),
			_                      => new FixField.Invalid  (tag, value),
		};
	}

	public static FixField Value(int tag, FixValueType type, ReadOnlySpan<byte> value)
	{
		return type switch
		{
			FixValueType.Text      => new FixField.Text     (tag, value.ToText()),
			FixValueType.Character => new FixField.Character(tag, value.ToCharacter()),
			FixValueType.Boolean   => new FixField.Boolean  (tag, value.ToBoolean()),
			FixValueType.Integer   => new FixField.Integer  (tag, value.ToInteger()),
			FixValueType.Decimal   => new FixField.Decimal  (tag, value.ToDecimal()),
			FixValueType.Timestamp => new FixField.Timestamp(tag, value.ToTimestamp()),
			FixValueType.Time      => new FixField.Time     (tag, value.ToTime()),
			FixValueType.Date      => new FixField.Date     (tag, value.ToDate()),
			FixValueType.MonthYear => new FixField.MonthYear(tag, value.ToMonthYear()),
			FixValueType.Multiple  => new FixField.Multiple (tag, value.ToMultiple()),
			FixValueType.Data      => new FixField.Data     (tag, value.ToData()),
			_                      => new FixField.Invalid  (tag, value),
		};
	}
}
