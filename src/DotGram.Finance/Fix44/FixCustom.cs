using System;

namespace DotGram.Finance.Fix44;

/// <summary>What the value of a tag FIX 4.4 does not define is: one of the standard's types.</summary>
/// <remarks>
/// <para>
/// A counterparty's own fields are of the types the standard's are, so declaring one is naming its
/// type: <c>FixFieldFactory = tag =&gt; tag == 25005 ? FixCustom.Integer : null</c>.
/// Its field is then a <see cref="FixField.Custom{T}"/> of that tag, read by the conversion the
/// standard's fields of that type are read by, and <see cref="FixField.IsValid"/> false where the
/// value does not convert.
/// </para>
/// <para>
/// A consumer who wants a class of their own, to name the field or to hold its value to more, derives
/// from <see cref="FixField.Custom{T}"/> and says so with <see cref="FixCustom{T}.As"/>:
/// <c>FixCustom.Integer.As((tag, value) =&gt; new Status(tag, value))</c>.
/// </para>
/// </remarks>
public abstract class FixCustom
{
	private protected FixCustom()
	{
	}

	/// <summary>Text: String, Currency, Country, Exchange, Language.</summary>
	public static FixCustom<string> Text { get; } = new(static raw => (true, FixConvert.Text(raw)), static raw => (true, FixConvert.Text(raw)));

	/// <summary>A character: char.</summary>
	public static FixCustom<char> Character { get; } = new(FixConvert.Character, FixConvert.Character);

	/// <summary>Y or N: Boolean.</summary>
	public static FixCustom<bool> Boolean { get; } = new(FixConvert.Boolean, FixConvert.Boolean);

	/// <summary>A whole number: int, Length, SeqNum, NumInGroup, DayOfMonth, TagNum.</summary>
	public static FixCustom<long> Integer { get; } = new(FixConvert.Integer, FixConvert.Integer);

	/// <summary>A decimal number: float, Qty, Price, PriceOffset, Amt, Percentage.</summary>
	public static FixCustom<decimal> Decimal { get; } = new(FixConvert.Decimal, FixConvert.Decimal);

	/// <summary>A moment in UTC: UTCTimestamp, TZTimestamp.</summary>
	public static FixCustom<DateTimeOffset> Timestamp { get; } = new(FixConvert.Timestamp, FixConvert.Timestamp);

	/// <summary>A time of day: UTCTimeOnly, TZTimeOnly.</summary>
	public static FixCustom<TimeOnly> Time { get; } = new(FixConvert.Time, FixConvert.Time);

	/// <summary>A date: UTCDateOnly, LocalMktDate.</summary>
	public static FixCustom<DateOnly> Date { get; } = new(FixConvert.Date, FixConvert.Date);

	/// <summary>A month and year, with an optional day or week: MonthYear.</summary>
	public static FixCustom<string> MonthYear { get; } = new(FixConvert.MonthYear, FixConvert.MonthYear);

	/// <summary>Values separated by spaces: MultipleValueString, MultipleCharValue, MultipleStringValue.</summary>
	public static FixCustom<string[]> Multiple { get; } = new(FixConvert.Multiple, FixConvert.Multiple);

	/// <summary>Octets, measured by the length before them: data, XMLData.</summary>
	/// <remarks>The tag is data only as the second of a pair in <see cref="FixContext.LengthDataPairs"/>.</remarks>
	public static FixCustom<ReadOnlyMemory<byte>> Data { get; } = new(FixConvert.Data, FixConvert.Data);

	/// <summary>The declaration of a type as a QuickFIX dictionary names it, or null for a name it does not know.</summary>
	internal static FixCustom? Of(string? type)
	{
		return type?.ToUpperInvariant() switch
		{
			"CHAR"                                                                           => Character,
			"BOOLEAN"                                                                        => Boolean,
			"INT" or "LENGTH" or "SEQNUM" or "NUMINGROUP" or "DAYOFMONTH" or "TAGNUM"        => Integer,
			"FLOAT" or "QTY" or "QUANTITY" or "PRICE" or "PRICEOFFSET" or "AMT" or "PERCENTAGE" => Decimal,
			"UTCTIMESTAMP" or "TZTIMESTAMP" or "TIME"                                        => Timestamp,
			"UTCTIMEONLY" or "TZTIMEONLY"                                                    => Time,
			"UTCDATEONLY" or "UTCDATE" or "LOCALMKTDATE" or "DATE"                           => Date,
			"MONTHYEAR"                                                                      => MonthYear,
			"MULTIPLEVALUESTRING" or "MULTIPLECHARVALUE" or "MULTIPLESTRINGVALUE"            => Multiple,
			"DATA" or "XMLDATA"                                                              => Data,
			null                                                                             => null,
			_                                                                                => Text,
		};
	}

	internal abstract FixField Build(int tag, ReadOnlySpan<char> value);
	internal abstract FixField Build(int tag, ReadOnlySpan<byte> value);
	internal abstract FixField Build(int tag, (bool Valid, ReadOnlyMemory<byte> Value) data);

	// ── what the reader does with a tag it has no class for ───────────────────────────────────────

	internal static FixField Build(int tag, ReadOnlySpan<char> value, Func<int, FixCustom?>? custom)
	{
		return custom?.Invoke(tag) is { } declared ? declared.Build(tag, value) : new FixField.Invalid(tag, value);
	}

	internal static FixField Build(int tag, ReadOnlySpan<byte> value, Func<int, FixCustom?>? custom)
	{
		return custom?.Invoke(tag) is { } declared ? declared.Build(tag, value) : new FixField.Invalid(tag, value);
	}

	internal static FixField Build(int tag, (bool Valid, ReadOnlyMemory<byte> Value) data, Func<int, FixCustom?>? custom)
	{
		return custom?.Invoke(tag) is { } declared ? declared.Build(tag, data) : new FixField.Invalid(tag, data.Value);
	}
}

/// <summary>A declared type whose value is a <typeparamref name="T"/>.</summary>
/// <typeparam name="T">What the value converts to.</typeparam>
public sealed class FixCustom<T> : FixCustom
{
	internal delegate (bool Valid, T Value) Chars(ReadOnlySpan<char> raw);
	internal delegate (bool Valid, T Value) Bytes(ReadOnlySpan<byte> raw);

	readonly Chars                                          _chars;
	readonly Bytes                                          _bytes;
	readonly Func<int, (bool Valid, T Value), FixField.Custom<T>>? _create;

	internal FixCustom(Chars chars, Bytes bytes, Func<int, (bool Valid, T Value), FixField.Custom<T>>? create = null)
	{
		_chars  = chars;
		_bytes  = bytes;
		_create = create;
	}

	/// <summary>The same type, its field built as a class of the consumer's: <c>(tag, value) =&gt; new Status(tag, value)</c>.</summary>
	/// <param name="create">Builds the field from its tag and the converted value.</param>
	/// <exception cref="ArgumentNullException"><paramref name="create"/> is null.</exception>
	public FixCustom<T> As(Func<int, (bool Valid, T Value), FixField.Custom<T>> create)
	{
		return new(_chars, _bytes, create ?? throw new ArgumentNullException(nameof(create)));
	}

	FixField.Custom<T> Make(int tag, (bool Valid, T Value) value)
	{
		if (_create is null)
			return new FixField.Custom<T>(tag, value);

		var made = _create(tag, value);

		return made.Tag == tag
			? made
			: throw new InvalidOperationException($"The field declared for tag {tag} was built with tag {made.Tag}.");
	}

	internal override FixField Build(int tag, ReadOnlySpan<char> value)
	{
		return Make(tag, _chars(value));
	}

	internal override FixField Build(int tag, ReadOnlySpan<byte> value)
	{
		return Make(tag, _bytes(value));
	}

	internal override FixField Build(int tag, (bool Valid, ReadOnlyMemory<byte> Value) data)
	{
		return this is FixCustom<ReadOnlyMemory<byte>> octets
			? octets.Make(tag, data)
			: Make(tag, _bytes(data.Value.Span));
	}
}
