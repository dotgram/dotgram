using System;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace DotGram.Finance.Fix;

/// <summary>
/// One field as the wire had it: its tag, its extent, and its value read as the type the tag has.
/// </summary>
/// <remarks>
/// A field is a class of the type of its value, not of its tag: an <c>OrderQty</c> is a
/// <see cref="Decimal"/> whose <see cref="Tag"/> is <see cref="FixTag.OrderQty"/>, matched as
/// <c>FixField.Decimal { Tag: FixTag.OrderQty } quantity</c>. The type is the standard's for the
/// tags FIX 4.4 defines and a loaded dictionary's for the ones it adds; a tag neither defines is an
/// <see cref="Invalid"/>.
/// </remarks>
public abstract class FixField : IFixLocation
{
	// The terminator's length sits in a byte beside IsValid, so the state before the value is four
	// words: Tag, Position, Length and this. One too wide for it, a log separator padded past 254
	// spaces, leaves the byte's maximum as a mark and is kept in Wide. Not in a field of its own:
	// four more bytes are eight with alignment, every field would carry them, and that is the whole
	// saving (a character field 40 bytes against 48) spent on input nobody sends.
	static readonly ConditionalWeakTable<FixField, WideLengths> Wide = new();

	byte _terminatorLength = 1;

	/// <summary>
	/// Initializes the tag and conversion status.
	/// </summary>
	/// <param name="tag">The FIX tag; recovery fields use zero.</param>
	/// <param name="isValid">Whether the supplied primitive value is valid.</param>
	protected FixField(int tag, bool isValid)
	{
		Tag     = tag;
		IsValid = isValid;
	}

	int TerminatorLength => _terminatorLength == byte.MaxValue ? Wide.GetValue(this, NewWide).Terminator : _terminatorLength;

	/// <summary>Whether a separator ends the field, rather than the end of the input.</summary>
	internal bool Terminated => TerminatorLength > 0;

	// The tag, its digits and the equals sign after them; nothing for skipped input, which has no tag.
	int PrefixLength
	{
		get
		{
			if (Tag == 0)
				return 0;

			var length = 2;

			for (var digits = Tag; digits >= 10; digits /= 10)
				length++;

			return length;
		}
	}

	/// <summary>
	/// Gets the FIX tag; an <see cref="Invalid"/> field of skipped input uses zero.
	/// </summary>
	/// <remarks>A tag FIX 4.4 does not name is still its number: <c>25005</c>.</remarks>
	public int Tag           { get; }
	/// <summary>
	/// Gets the zero-based start of the field in the original input.
	/// </summary>
	/// <remarks>Offsets count UTF-16 code units for character input and bytes for byte input.</remarks>
	public int Position      { get; private set; }
	/// <summary>
	/// Gets the number of input units occupied by the value, excluding tag headers and the terminator.
	/// For recovery fields it is the skipped raw length.
	/// </summary>
	public int Length        { get; private set; }
	/// <summary>
	/// Gets the zero-based start of the value in the original input.
	/// For recovery fields this equals <see cref="Position"/>.
	/// </summary>
	public int ValuePosition => Position + PrefixLength;

	/// <summary>
	/// Records source coordinates supplied by the parser and derives the value length.
	/// </summary>
	/// <param name="position">The absolute start of the field.</param>
	/// <param name="length">The complete matched extent, including headers and the configured terminator.</param>
	/// <remarks>
	/// The parser may call this more than once as enclosing rules finish. The final call
	/// supplies the complete field extent. Manually constructed fields have no source
	/// coordinates until this method is called; conversion validity is not changed.
	/// </remarks>
	public void Locate(int position, int length)
	{
		Position = position;
		Length   = length - PrefixLength - TerminatorLength;
	}

	internal FixField WithTerminator(int length)
	{
		if (length < byte.MaxValue)
			_terminatorLength = (byte)length;
		else
		{
			_terminatorLength = byte.MaxValue;
			Wide.GetValue(this, NewWide).Terminator = length;
		}

		return this;
	}

	internal bool HasWideLengths => Wide.TryGetValue(this, out _);

	static WideLengths NewWide(FixField field)
	{
		return new WideLengths();
	}

	sealed class WideLengths
	{
		public int Terminator;
	}

	/// <summary>
	/// Gets whether conversion to the field's primitive value succeeded.
	/// </summary>
	/// <remarks>
	/// This does not validate message structure, required fields or allowed code values.
	/// An invalid primitive remains a typed field with this property set to false;
	/// a syntax error recovered by the parser is represented by <see cref="Invalid"/>.
	/// </remarks>
	public bool IsValid { get; private protected set; }

	/// <summary>
	/// A field that is not one: malformed input skipped while recovering to the next separator, or a
	/// tag nothing builds a field of.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Skipped input has <see cref="Tag"/> zero, and its extent covers what was skipped, the
	/// synchronization separator and any padding it consumes excluded.
	/// </para>
	/// <para>
	/// A tag neither the version nor a dictionary loaded into the context defines keeps its tag and its
	/// value: <see cref="RawBytes"/> is the value's octets, from character input as from byte input,
	/// and the extent is the field's as any field's is.
	/// </para>
	/// <para>
	/// <see cref="IsValid"/> is false either way. Character and byte input retain their original
	/// representation.
	/// </para>
	/// </remarks>
	public sealed class Invalid : FixField
	{
		// A tag nothing defines: its value, kept as it was read.
		internal Invalid(int tag, ReadOnlySpan<char> value) : base(tag, false)
		{
			RawBytes = FixConvert.ToData(value).Value;
			Message  = Unknown(tag);
		}

		internal Invalid(int tag, ReadOnlySpan<byte> value) : base(tag, false)
		{
			RawBytes = value.ToArray();
			Message  = Unknown(tag);
		}

		static string Unknown(int tag)
		{
			return $"Tag {tag} is not a field the version or a loaded dictionary defines.";
		}

		/// <summary>
		/// Creates a recovery field from character input.
		/// </summary>
		/// <param name="raw">The skipped input without the synchronization separator.</param>
		/// <param name="position">The absolute zero-based start of the skipped input.</param>
		/// <param name="message">The diagnostic describing the recognition failure.</param>
		/// <exception cref="ArgumentNullException"><paramref name="raw"/> or <paramref name="message"/> is null.</exception>
		public Invalid(string raw, int position, string message) : base(0, false)
		{
			RawText = raw ?? throw new ArgumentNullException(nameof(raw));
			Message = message ?? throw new ArgumentNullException(nameof(message));

			_terminatorLength = 0;

			Locate(position, raw.Length);
		}

		/// <summary>
		/// Creates a recovery field from character input, copying the span into a string.
		/// </summary>
		/// <param name="raw">The skipped input without the synchronization separator.</param>
		/// <param name="position">The absolute zero-based start of the skipped input.</param>
		/// <param name="message">The diagnostic describing the recognition failure.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
		public Invalid(ReadOnlySpan<char> raw, int position, string message) : this(raw.ToString(), position, message) { }

		/// <summary>
		/// Creates a recovery field from byte input, copying the span into owned storage.
		/// </summary>
		/// <param name="raw">The skipped input without the synchronization separator.</param>
		/// <param name="position">The absolute zero-based start of the skipped input.</param>
		/// <param name="message">The diagnostic describing the recognition failure.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
		public Invalid(ReadOnlySpan<byte> raw, int position, string message) : base(0, false)
		{
			RawBytes = raw.ToArray();
			Message  = message ?? throw new ArgumentNullException(nameof(message));

			_terminatorLength = 0;

			Locate(position, raw.Length);
		}

		/// <summary>
		/// Gets the skipped character input, or a tag's value read as characters; null when the source was bytes.
		/// </summary>
		public string?              RawText { get; }
		/// <summary>
		/// Gets an owned copy of the skipped bytes, or of a tag's value read as bytes; empty memory when the source was characters.
		/// </summary>
		public ReadOnlyMemory<byte> RawBytes { get; }
		/// <summary>
		/// Gets whether this recovery field was created from bytes, including an empty byte span.
		/// </summary>
		public bool                 IsByteInput => RawText == null;
		/// <summary>
		/// Gets the recognition diagnostic. Its wording depends on the parser implementation.
		/// </summary>
		public string               Message { get; }
	}

	/// <summary>
	/// Provides typed value storage and conversion status for concrete FIX field cases.
	/// </summary>
	/// <typeparam name="T">The CLR representation of the field's primitive value.</typeparam>
	/// <remarks>
	/// Value access does not perform message validation or retry a failed conversion.
	/// Use <see cref="TryGetValue"/> when conversion failure is an expected outcome.
	/// </remarks>
	public abstract class Typed<T> : FixField
	{
		readonly T _value;

		/// <summary>
		/// Initializes a field with a value already considered valid.
		/// </summary>
		/// <param name="tag">The FIX tag.</param>
		/// <param name="value">The typed value to store.</param>
		protected Typed(int tag, T value) : base(tag, true)
		{
			_value = value;
		}

		/// <summary>
		/// Initializes a field from the result of a primitive conversion.
		/// </summary>
		/// <param name="tag">The FIX tag.</param>
		/// <param name="parsed">The conversion status and its resulting value.</param>
		protected Typed(int tag, (bool Valid, T Value) parsed)
			: base(tag, parsed.Valid)
		{
			_value = parsed.Value;
		}

		/// <summary>
		/// Gets the stored typed value when primitive conversion succeeded.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> is false.</exception>
		public T Value => IsValid ? _value : throw new InvalidOperationException("The field has no valid typed value; inspect its original wire value.");

		/// <summary>
		/// Returns the stored value and reports whether primitive conversion succeeded.
		/// </summary>
		/// <param name="result">
		/// The stored conversion result. When this method returns false, do not treat this value
		/// as a successfully parsed value; it is still assigned and is not guaranteed to be default.
		/// </param>
		/// <returns>The value of <see cref="IsValid"/>.</returns>
		public bool TryGetValue(out T result)
		{
			result = _value;
			return IsValid;
		}
	}

	/// <summary>
	/// A field whose value is text: FIX <c>String</c>, <c>Currency</c>, <c>Exchange</c>, <c>Country</c>.
	/// </summary>
	/// <remarks>
	/// The value is the text itself, kept as the wire had it; it is always valid.
	/// </remarks>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">The field text.</param>
	public sealed class Text(int tag, string value)
		: Typed<string>(tag, value);

	/// <summary>
	/// A field whose value is a single character: FIX <c>char</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToCharacter(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Character(int tag, (bool Valid, char Value) value)
		: Typed<char>(tag, value);

	/// <summary>
	/// A field whose value is a boolean, <c>Y</c> or <c>N</c>: FIX <c>Boolean</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToBoolean(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Boolean(int tag, (bool Valid, bool Value) value)
		: Typed<bool>(tag, value);

	/// <summary>
	/// A field whose value is an integer: FIX <c>int</c>, <c>Length</c>, <c>SeqNum</c>, <c>NumInGroup</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToInteger(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Integer(int tag, (bool Valid, long Value) value)
		: Typed<long>(tag, value);

	/// <summary>
	/// A field whose value is a decimal number: FIX <c>float</c>, <c>Qty</c>, <c>Price</c>, <c>PriceOffset</c>, <c>Amt</c>, <c>Percentage</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToDecimal(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Decimal(int tag, (bool Valid, decimal Value) value)
		: Typed<decimal>(tag, value);

	/// <summary>
	/// A field whose value is an instant: FIX <c>UTCTimestamp</c>, at offset zero, and <c>TZTimestamp</c>, at the offset it was written with.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToTimestamp(ReadOnlySpan{char})"/> or <see cref="FixConvert.ToZonedTimestamp(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Timestamp(int tag, (bool Valid, DateTimeOffset Value) value)
		: Typed<DateTimeOffset>(tag, value);

	/// <summary>
	/// A field whose value is a UTC time of day: FIX <c>UTCTimeOnly</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToTime(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Time(int tag, (bool Valid, TimeOnly Value) value)
		: Typed<TimeOnly>(tag, value);

	/// <summary>
	/// A field whose value is a time of day and its offset from UTC: FIX <c>TZTimeOnly</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToZonedTime(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class ZonedTime(int tag, (bool Valid, (TimeOnly Time, TimeSpan Offset) Value) value)
		: Typed<(TimeOnly Time, TimeSpan Offset)>(tag, value);

	/// <summary>
	/// A field whose value is a date: FIX <c>UTCDateOnly</c>, <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToDate(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Date(int tag, (bool Valid, DateOnly Value) value)
		: Typed<DateOnly>(tag, value);

	/// <summary>
	/// A field whose value is a month and year: FIX <c>MonthYear</c>.
	/// </summary>
	/// <remarks>
	/// The value is the text itself, <c>YYYYMM</c>, <c>YYYYMMDD</c> or <c>YYYYMMwN</c>.
	/// </remarks>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToMonthYear(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class MonthYear(int tag, (bool Valid, string Value) value)
		: Typed<string>(tag, value);

	/// <summary>
	/// A field whose value is a list of values separated by spaces: FIX <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToMultiple(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Multiple(int tag, (bool Valid, string[] Value) value)
		: Typed<string[]>(tag, value);

	/// <summary>
	/// A field whose value is octets: FIX <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The data of a length/data pair is read by the length before it, so embedded separators are
	/// part of the value.
	/// </remarks>
	/// <param name="tag">The FIX tag.</param>
	/// <param name="value">What <see cref="FixConvert.ToData(ReadOnlySpan{char})"/> answers: whether the value is valid, and the value.</param>
	public sealed class Data(int tag, (bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(tag, value);
}
