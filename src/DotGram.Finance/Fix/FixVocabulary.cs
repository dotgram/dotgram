namespace DotGram.Finance.Fix;

/// <summary>A dictionary's spelling of a type, in this package's terms.</summary>
/// <remarks>
/// <para>
/// A dictionary names its types its own way — <c>UTCTIMESTAMP</c> where this package writes
/// <see cref="FixValueType.UTCTimestamp"/> — and a value held to a name nobody knows would be
/// called wrong every time. Where there is no counterpart the answer is
/// <see cref="FixValueType.None"/> and the value is not checked, which is the honest answer:
/// nothing can be said about a value against a type this package does not model.
/// <c>TZTIMEONLY</c> and <c>TZTIMESTAMP</c> are the two names of FIX 4.4 that land there.
/// </para>
/// <para>
/// In a file of its own because the generator compiles this same source. The map exists once: a
/// second copy is a second thing to disagree with the first, and it would disagree silently, by
/// leaving a tag unchecked rather than by failing.
/// </para>
/// </remarks>
static class FixVocabulary
{
	/// <summary>The type a dictionary's spelling names, or None for one this package does not model.</summary>
	public static FixValueType Of(string? declared) => declared?.ToUpperInvariant() switch
	{
		"STRING" or "LANGUAGE"     => FixValueType.String,
		"CHAR"                     => FixValueType.Char,
		"INT"                      => FixValueType.Int,
		"LENGTH"                   => FixValueType.Length,
		"NUMINGROUP"               => FixValueType.NumInGroup,
		"SEQNUM"                   => FixValueType.SeqNum,
		"TAGNUM"                   => FixValueType.TagNum,
		"DAYOFMONTH"               => FixValueType.DayOfMonth,
		"FLOAT"                    => FixValueType.Float,
		"QTY"                      => FixValueType.Qty,
		"PRICE"                    => FixValueType.Price,
		"PRICEOFFSET"              => FixValueType.PriceOffset,
		"AMT"                      => FixValueType.Amt,
		"PERCENTAGE"               => FixValueType.Percentage,
		"BOOLEAN"                  => FixValueType.Boolean,
		"CURRENCY"                 => FixValueType.Currency,
		"COUNTRY"                  => FixValueType.Country,
		"EXCHANGE"                 => FixValueType.Exchange,
		"MONTHYEAR"                => FixValueType.MonthYear,
		"LOCALMKTDATE"             => FixValueType.LocalMktDate,
		"UTCDATEONLY" or "UTCDATE" => FixValueType.UTCDateOnly,
		"UTCTIMEONLY"              => FixValueType.UTCTimeOnly,
		"UTCTIMESTAMP"             => FixValueType.UTCTimestamp,
		"DATA" or "XMLDATA"        => FixValueType.Data,
		"MULTIPLEVALUESTRING" or "MULTIPLESTRINGVALUE" or "MULTIPLECHARVALUE" => FixValueType.MultipleValueString,
		_                          => FixValueType.None,
	};
}
