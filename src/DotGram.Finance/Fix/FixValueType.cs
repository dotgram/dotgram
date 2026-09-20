namespace DotGram.Finance.Fix;

/// <summary>A FIX wire type, as a schema declares one.</summary>
/// <remarks>
/// What the schema holds and what every check reads. <see cref="FixSchema.TypeName"/> spells one
/// where a name is wanted — for a message to a reader, or to compare with a dictionary's own
/// vocabulary — and nothing on a hot path asks for the spelling.
/// </remarks>
public enum FixValueType : byte
{
	/// <summary>No type: a tag the schema does not define, whose value nothing can be said about.</summary>
	None = 0,

	/// <summary>An amount, signed and fractional.</summary>
	Amt = 1,

	/// <summary><c>Y</c> or <c>N</c>.</summary>
	Boolean = 2,

	/// <summary>One character.</summary>
	Char = 3,

	/// <summary>An ISO 3166 country, two letters.</summary>
	Country = 4,

	/// <summary>An ISO 4217 currency, three letters.</summary>
	Currency = 5,

	/// <summary>Raw octets, measured by the length field before them and checked no further.</summary>
	Data = 6,

	/// <summary>An ISO 10383 market identifier, four characters.</summary>
	Exchange = 7,

	/// <summary>A number, signed and fractional.</summary>
	Float = 8,

	/// <summary>A whole number, signed.</summary>
	Int = 9,

	/// <summary>The length of the data field that follows it.</summary>
	Length = 10,

	/// <summary>A local market date, <c>YYYYMMDD</c>.</summary>
	LocalMktDate = 11,

	/// <summary>A month and year, with an optional day or week.</summary>
	MonthYear = 12,

	/// <summary>Several single-character values, separated by spaces.</summary>
	MultipleValueString = 13,

	/// <summary>The number of entries in the repeating group that follows it.</summary>
	NumInGroup = 14,

	/// <summary>A percentage, as a fraction.</summary>
	Percentage = 15,

	/// <summary>A price.</summary>
	Price = 16,

	/// <summary>A price offset, which may be negative.</summary>
	PriceOffset = 17,

	/// <summary>A quantity.</summary>
	Qty = 18,

	/// <summary>A sequence number, positive.</summary>
	SeqNum = 19,

	/// <summary>Printable characters, with no further shape.</summary>
	String = 20,

	/// <summary>A UTC date, <c>YYYYMMDD</c>.</summary>
	UTCDateOnly = 21,

	/// <summary>A UTC time, <c>HH:MM:SS</c> with optional milliseconds.</summary>
	UTCTimeOnly = 22,

	/// <summary>A UTC timestamp, a date and a time joined by a hyphen.</summary>
	UTCTimestamp = 23,

	/// <summary>A tag number, positive. No tag of FIX 4.4 has this type; a dictionary may name it.</summary>
	TagNum = 24,

	/// <summary>A day of the month, 1 to 31. No tag of FIX 4.4 has this type; a dictionary may name it.</summary>
	DayOfMonth = 25,
}
