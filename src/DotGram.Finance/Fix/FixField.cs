using System;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace DotGram.Finance.Fix;

/// <summary>
/// One case in the FIX field algebra, with its original source extent.
/// </summary>
public abstract class FixField : IFixLocation
{
	// The two header lengths sit in narrow fields beside IsValid, so the state before the value
	// is four words: Tag, Position, Length and these. A length too wide for its field, which
	// takes a data length written with tens of thousands of leading zeros or a log separator
	// padded past 254 spaces, leaves the field's maximum as a mark and is kept in Wide.
	// Not in a field of its own: four more bytes are eight with alignment, every field would
	// carry them, and that is the whole saving (a character field 40 bytes against 48) spent
	// on input nobody sends. Only such input writes to the table or reads from it.
	static readonly ConditionalWeakTable<FixField, WideLengths> Wide = new();

	ushort _prefixLength;
	byte   _terminatorLength = 1;

	/// <summary>
	/// Initializes the tag, conversion status and default tag-prefix length.
	/// </summary>
	/// <param name="tag">The numeric FIX tag; recovery fields use zero.</param>
	/// <param name="valid">Whether the supplied primitive value is valid.</param>
	protected FixField(int tag, bool valid)
	{
		Tag           = tag;
		_prefixLength = (ushort)TagPrefixLength(tag);
		IsValid       = valid;
	}

	internal bool IsBinary     => this is not Invalid && PrefixLength != TagPrefixLength(Tag);
	internal int  DataPosition => ValuePosition - TagPrefixLength(Tag);

	int PrefixLength     => _prefixLength     == ushort.MaxValue ? Wide.GetValue(this, NewWide).Prefix     : _prefixLength;
	int TerminatorLength => _terminatorLength == byte.MaxValue   ? Wide.GetValue(this, NewWide).Terminator : _terminatorLength;

	// The tag, its digits and the equals sign after them.
	static int TagPrefixLength(int tag)
	{
		var length = 2;

		for (var digits = tag; digits >= 10; digits /= 10)
			length++;

		return length;
	}

	/// <summary>
	/// Gets the numeric FIX tag. A combined length/data field uses the data tag;
	/// an <see cref="Invalid"/> field uses zero.
	/// </summary>
	public int Tag           { get; }
	/// <summary>
	/// Gets the zero-based start of the field in the original input.
	/// For a combined binary field, this is the start of its preceding length tag.
	/// </summary>
	/// <remarks>Offsets count UTF-16 code units for character input and bytes for byte input.</remarks>
	public int Position      { get; private set; }
	/// <summary>
	/// Gets the number of input units occupied by the value, excluding tag headers and the terminator.
	/// For binary fields this is the payload length; for recovery fields it is the skipped raw length.
	/// </summary>
	public int Length        { get; private set; }
	/// <summary>
	/// Gets the zero-based start of the value or binary payload in the original input.
	/// For recovery fields this equals <see cref="Position"/>.
	/// </summary>
	public int ValuePosition => Position + PrefixLength;

	/// <summary>
	/// Records source coordinates supplied by the parser and derives the value length.
	/// </summary>
	/// <param name="position">The absolute start of the field, or of the length tag for a binary pair.</param>
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

	internal FixField WithBinary(FixBinaryValue value, int start)
	{
		// The source extent begins at the length tag; the value begins after both headers.
		var length = value.Position - start;

		if (length < ushort.MaxValue)
			_prefixLength = (ushort)length;
		else
		{
			_prefixLength = ushort.MaxValue;
			Wide.GetValue(this, NewWide).Prefix = length;
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
		public int Prefix;
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
	public bool IsValid { get; }

	/// <summary>
	/// Represents malformed input skipped while recovering to the next field separator or end of input.
	/// </summary>
	/// <remarks>
	/// The raw input excludes the synchronization separator and any padding it consumes.
	/// <see cref="Tag"/> is zero, <see cref="IsValid"/> is false, and the source extent
	/// covers the skipped input. Character and byte input retain their original representation.
	/// </remarks>
	public sealed class Invalid : FixField
	{
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

			_prefixLength     = 0;
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

			_prefixLength = 0;
			_terminatorLength = 0;

			Locate(position, raw.Length);
		}

		/// <summary>
		/// Gets the skipped character input, or null when the source was bytes.
		/// </summary>
		public string?              RawText { get; }
		/// <summary>
		/// Gets an owned copy of the skipped bytes, or empty memory when the source was characters.
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
	/// Represents a tag without a dedicated field class, retaining its value as bytes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The numeric tag is preserved. A configured custom length/data pair also produces this
	/// case when its data tag is unknown. Byte input is copied unchanged; character input
	/// uses the same octet conversion and validity reporting as binary field values.
	/// </para>
	/// <para>
	/// This is what the package builds for a tag it was told nothing about. A consumer who
	/// supplies <see cref="FixCustomFields"/> builds their own field for such a tag instead,
	/// and then this case does not arise for it at all.
	/// </para>
	/// </remarks>
	public sealed class Custom : Typed<ReadOnlyMemory<byte>>
	{
		internal Custom(int tag, (bool Valid, ReadOnlyMemory<byte> Value) parsed)
			: base(tag, parsed)
		{
		}
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
		/// <param name="tag">The numeric FIX tag.</param>
		/// <param name="value">The typed value to store.</param>
		protected Typed(int tag, T value) : base(tag, true)
		{
			_value = value;
		}

		/// <summary>
		/// Initializes a field from the result of a primitive conversion.
		/// </summary>
		/// <param name="tag">The numeric FIX tag.</param>
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
	/// Represents Account, FIX tag 1, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Account(string value)
		: Typed<string>(1, value);

	/// <summary>
	/// Represents AdvId, FIX tag 2, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AdvId(string value)
		: Typed<string>(2, value);

	/// <summary>
	/// Represents AdvRefID, FIX tag 3, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AdvRefID(string value)
		: Typed<string>(3, value);

	/// <summary>
	/// Represents AdvSide, FIX tag 4, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AdvSide((bool Valid, char Value) value)
		: Typed<char>(4, value);

	/// <summary>
	/// Represents AdvTransType, FIX tag 5, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AdvTransType(string value)
		: Typed<string>(5, value);

	/// <summary>
	/// Represents AvgPx, FIX tag 6, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AvgPx((bool Valid, decimal Value) value)
		: Typed<decimal>(6, value);

	/// <summary>
	/// Represents BeginSeqNo, FIX tag 7, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BeginSeqNo((bool Valid, long Value) value)
		: Typed<long>(7, value);

	/// <summary>
	/// Represents BeginString, FIX tag 8, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BeginString(string value)
		: Typed<string>(8, value);

	/// <summary>
	/// Represents BodyLength, FIX tag 9, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BodyLength((bool Valid, long Value) value)
		: Typed<long>(9, value);

	/// <summary>
	/// Represents CheckSum, FIX tag 10, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CheckSum(string value)
		: Typed<string>(10, value);

	/// <summary>
	/// Represents ClOrdID, FIX tag 11, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ClOrdID(string value)
		: Typed<string>(11, value);

	/// <summary>
	/// Represents Commission, FIX tag 12, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Commission((bool Valid, decimal Value) value)
		: Typed<decimal>(12, value);

	/// <summary>
	/// Represents CommType, FIX tag 13, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CommType((bool Valid, char Value) value)
		: Typed<char>(13, value);

	/// <summary>
	/// Represents CumQty, FIX tag 14, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CumQty((bool Valid, decimal Value) value)
		: Typed<decimal>(14, value);

	/// <summary>
	/// Represents Currency, FIX tag 15, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Currency(string value)
		: Typed<string>(15, value);

	/// <summary>
	/// Represents EndSeqNo, FIX tag 16, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EndSeqNo((bool Valid, long Value) value)
		: Typed<long>(16, value);

	/// <summary>
	/// Represents ExecID, FIX tag 17, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ExecID(string value)
		: Typed<string>(17, value);

	/// <summary>
	/// Represents ExecInst, FIX tag 18, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecInst((bool Valid, string[] Value) value)
		: Typed<string[]>(18, value);

	/// <summary>
	/// Represents ExecRefID, FIX tag 19, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ExecRefID(string value)
		: Typed<string>(19, value);

	/// <summary>
	/// Represents HandlInst, FIX tag 21, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HandlInst((bool Valid, char Value) value)
		: Typed<char>(21, value);

	/// <summary>
	/// Represents SecurityIDSource, FIX tag 22, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityIDSource(string value)
		: Typed<string>(22, value);

	/// <summary>
	/// Represents IOIID, FIX tag 23, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class IOIID(string value)
		: Typed<string>(23, value);

	/// <summary>
	/// Represents IOIQltyInd, FIX tag 25, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IOIQltyInd((bool Valid, char Value) value)
		: Typed<char>(25, value);

	/// <summary>
	/// Represents IOIRefID, FIX tag 26, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class IOIRefID(string value)
		: Typed<string>(26, value);

	/// <summary>
	/// Represents IOIQty, FIX tag 27, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class IOIQty(string value)
		: Typed<string>(27, value);

	/// <summary>
	/// Represents IOITransType, FIX tag 28, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IOITransType((bool Valid, char Value) value)
		: Typed<char>(28, value);

	/// <summary>
	/// Represents LastCapacity, FIX tag 29, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastCapacity((bool Valid, char Value) value)
		: Typed<char>(29, value);

	/// <summary>
	/// Represents LastMkt, FIX tag 30, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LastMkt(string value)
		: Typed<string>(30, value);

	/// <summary>
	/// Represents LastPx, FIX tag 31, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastPx((bool Valid, decimal Value) value)
		: Typed<decimal>(31, value);

	/// <summary>
	/// Represents LastQty, FIX tag 32, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastQty((bool Valid, decimal Value) value)
		: Typed<decimal>(32, value);

	/// <summary>
	/// Represents NoLinesOfText, FIX tag 33, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoLinesOfText((bool Valid, long Value) value)
		: Typed<long>(33, value);

	/// <summary>
	/// Represents MsgSeqNum, FIX tag 34, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MsgSeqNum((bool Valid, long Value) value)
		: Typed<long>(34, value);

	/// <summary>
	/// Represents MsgType, FIX tag 35, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MsgType(string value)
		: Typed<string>(35, value);

	/// <summary>
	/// Represents NewSeqNo, FIX tag 36, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NewSeqNo((bool Valid, long Value) value)
		: Typed<long>(36, value);

	/// <summary>
	/// Represents OrderID, FIX tag 37, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrderID(string value)
		: Typed<string>(37, value);

	/// <summary>
	/// Represents OrderQty, FIX tag 38, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderQty((bool Valid, decimal Value) value)
		: Typed<decimal>(38, value);

	/// <summary>
	/// Represents OrdStatus, FIX tag 39, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrdStatus((bool Valid, char Value) value)
		: Typed<char>(39, value);

	/// <summary>
	/// Represents OrdType, FIX tag 40, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrdType((bool Valid, char Value) value)
		: Typed<char>(40, value);

	/// <summary>
	/// Represents OrigClOrdID, FIX tag 41, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrigClOrdID(string value)
		: Typed<string>(41, value);

	/// <summary>
	/// Represents OrigTime, FIX tag 42, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrigTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(42, value);

	/// <summary>
	/// Represents PossDupFlag, FIX tag 43, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PossDupFlag((bool Valid, bool Value) value)
		: Typed<bool>(43, value);

	/// <summary>
	/// Represents Price, FIX tag 44, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Price((bool Valid, decimal Value) value)
		: Typed<decimal>(44, value);

	/// <summary>
	/// Represents RefSeqNum, FIX tag 45, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RefSeqNum((bool Valid, long Value) value)
		: Typed<long>(45, value);

	/// <summary>
	/// Represents SecurityID, FIX tag 48, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityID(string value)
		: Typed<string>(48, value);

	/// <summary>
	/// Represents SenderCompID, FIX tag 49, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SenderCompID(string value)
		: Typed<string>(49, value);

	/// <summary>
	/// Represents SenderSubID, FIX tag 50, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SenderSubID(string value)
		: Typed<string>(50, value);

	/// <summary>
	/// Represents SendingTime, FIX tag 52, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SendingTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(52, value);

	/// <summary>
	/// Represents Quantity, FIX tag 53, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Quantity((bool Valid, decimal Value) value)
		: Typed<decimal>(53, value);

	/// <summary>
	/// Represents Side, FIX tag 54, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Side((bool Valid, char Value) value)
		: Typed<char>(54, value);

	/// <summary>
	/// Represents Symbol, FIX tag 55, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Symbol(string value)
		: Typed<string>(55, value);

	/// <summary>
	/// Represents TargetCompID, FIX tag 56, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TargetCompID(string value)
		: Typed<string>(56, value);

	/// <summary>
	/// Represents TargetSubID, FIX tag 57, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TargetSubID(string value)
		: Typed<string>(57, value);

	/// <summary>
	/// Represents Text, FIX tag 58, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Text(string value)
		: Typed<string>(58, value);

	/// <summary>
	/// Represents TimeInForce, FIX tag 59, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TimeInForce((bool Valid, char Value) value)
		: Typed<char>(59, value);

	/// <summary>
	/// Represents TransactTime, FIX tag 60, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TransactTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(60, value);

	/// <summary>
	/// Represents Urgency, FIX tag 61, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Urgency((bool Valid, char Value) value)
		: Typed<char>(61, value);

	/// <summary>
	/// Represents ValidUntilTime, FIX tag 62, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ValidUntilTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(62, value);

	/// <summary>
	/// Represents SettlType, FIX tag 63, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlType((bool Valid, char Value) value)
		: Typed<char>(63, value);

	/// <summary>
	/// Represents SettlDate, FIX tag 64, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(64, value);

	/// <summary>
	/// Represents SymbolSfx, FIX tag 65, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SymbolSfx(string value)
		: Typed<string>(65, value);

	/// <summary>
	/// Represents ListID, FIX tag 66, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ListID(string value)
		: Typed<string>(66, value);

	/// <summary>
	/// Represents ListSeqNo, FIX tag 67, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ListSeqNo((bool Valid, long Value) value)
		: Typed<long>(67, value);

	/// <summary>
	/// Represents TotNoOrders, FIX tag 68, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoOrders((bool Valid, long Value) value)
		: Typed<long>(68, value);

	/// <summary>
	/// Represents ListExecInst, FIX tag 69, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ListExecInst(string value)
		: Typed<string>(69, value);

	/// <summary>
	/// Represents AllocID, FIX tag 70, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocID(string value)
		: Typed<string>(70, value);

	/// <summary>
	/// Represents AllocTransType, FIX tag 71, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocTransType((bool Valid, char Value) value)
		: Typed<char>(71, value);

	/// <summary>
	/// Represents RefAllocID, FIX tag 72, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RefAllocID(string value)
		: Typed<string>(72, value);

	/// <summary>
	/// Represents NoOrders, FIX tag 73, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoOrders((bool Valid, long Value) value)
		: Typed<long>(73, value);

	/// <summary>
	/// Represents AvgPxPrecision, FIX tag 74, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AvgPxPrecision((bool Valid, long Value) value)
		: Typed<long>(74, value);

	/// <summary>
	/// Represents TradeDate, FIX tag 75, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(75, value);

	/// <summary>
	/// Represents PositionEffect, FIX tag 77, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PositionEffect((bool Valid, char Value) value)
		: Typed<char>(77, value);

	/// <summary>
	/// Represents NoAllocs, FIX tag 78, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoAllocs((bool Valid, long Value) value)
		: Typed<long>(78, value);

	/// <summary>
	/// Represents AllocAccount, FIX tag 79, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocAccount(string value)
		: Typed<string>(79, value);

	/// <summary>
	/// Represents AllocQty, FIX tag 80, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocQty((bool Valid, decimal Value) value)
		: Typed<decimal>(80, value);

	/// <summary>
	/// Represents ProcessCode, FIX tag 81, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ProcessCode((bool Valid, char Value) value)
		: Typed<char>(81, value);

	/// <summary>
	/// Represents NoRpts, FIX tag 82, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoRpts((bool Valid, long Value) value)
		: Typed<long>(82, value);

	/// <summary>
	/// Represents RptSeq, FIX tag 83, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RptSeq((bool Valid, long Value) value)
		: Typed<long>(83, value);

	/// <summary>
	/// Represents CxlQty, FIX tag 84, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CxlQty((bool Valid, decimal Value) value)
		: Typed<decimal>(84, value);

	/// <summary>
	/// Represents NoDlvyInst, FIX tag 85, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoDlvyInst((bool Valid, long Value) value)
		: Typed<long>(85, value);

	/// <summary>
	/// Represents AllocStatus, FIX tag 87, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocStatus((bool Valid, long Value) value)
		: Typed<long>(87, value);

	/// <summary>
	/// Represents AllocRejCode, FIX tag 88, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocRejCode((bool Valid, long Value) value)
		: Typed<long>(88, value);

	/// <summary>
	/// Represents Signature, FIX tag 89, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Signature((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(89, value);

	/// <summary>
	/// Represents SecureDataLen, FIX tag 90, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecureDataLen((bool Valid, long Value) value)
		: Typed<long>(90, value);

	/// <summary>
	/// Represents SecureData, FIX tag 91, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecureData((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(91, value);

	/// <summary>
	/// Represents SignatureLength, FIX tag 93, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SignatureLength((bool Valid, long Value) value)
		: Typed<long>(93, value);

	/// <summary>
	/// Represents EmailType, FIX tag 94, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EmailType((bool Valid, char Value) value)
		: Typed<char>(94, value);

	/// <summary>
	/// Represents RawDataLength, FIX tag 95, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RawDataLength((bool Valid, long Value) value)
		: Typed<long>(95, value);

	/// <summary>
	/// Represents RawData, FIX tag 96, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RawData((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(96, value);

	/// <summary>
	/// Represents PossResend, FIX tag 97, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PossResend((bool Valid, bool Value) value)
		: Typed<bool>(97, value);

	/// <summary>
	/// Represents EncryptMethod, FIX tag 98, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncryptMethod((bool Valid, long Value) value)
		: Typed<long>(98, value);

	/// <summary>
	/// Represents StopPx, FIX tag 99, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StopPx((bool Valid, decimal Value) value)
		: Typed<decimal>(99, value);

	/// <summary>
	/// Represents ExDestination, FIX tag 100, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ExDestination(string value)
		: Typed<string>(100, value);

	/// <summary>
	/// Represents CxlRejReason, FIX tag 102, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CxlRejReason((bool Valid, long Value) value)
		: Typed<long>(102, value);

	/// <summary>
	/// Represents OrdRejReason, FIX tag 103, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrdRejReason((bool Valid, long Value) value)
		: Typed<long>(103, value);

	/// <summary>
	/// Represents IOIQualifier, FIX tag 104, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IOIQualifier((bool Valid, char Value) value)
		: Typed<char>(104, value);

	/// <summary>
	/// Represents Issuer, FIX tag 106, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Issuer(string value)
		: Typed<string>(106, value);

	/// <summary>
	/// Represents SecurityDesc, FIX tag 107, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityDesc(string value)
		: Typed<string>(107, value);

	/// <summary>
	/// Represents HeartBtInt, FIX tag 108, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HeartBtInt((bool Valid, long Value) value)
		: Typed<long>(108, value);

	/// <summary>
	/// Represents MinQty, FIX tag 110, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MinQty((bool Valid, decimal Value) value)
		: Typed<decimal>(110, value);

	/// <summary>
	/// Represents MaxFloor, FIX tag 111, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaxFloor((bool Valid, decimal Value) value)
		: Typed<decimal>(111, value);

	/// <summary>
	/// Represents TestReqID, FIX tag 112, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TestReqID(string value)
		: Typed<string>(112, value);

	/// <summary>
	/// Represents ReportToExch, FIX tag 113, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ReportToExch((bool Valid, bool Value) value)
		: Typed<bool>(113, value);

	/// <summary>
	/// Represents LocateReqd, FIX tag 114, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LocateReqd((bool Valid, bool Value) value)
		: Typed<bool>(114, value);

	/// <summary>
	/// Represents OnBehalfOfCompID, FIX tag 115, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OnBehalfOfCompID(string value)
		: Typed<string>(115, value);

	/// <summary>
	/// Represents OnBehalfOfSubID, FIX tag 116, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OnBehalfOfSubID(string value)
		: Typed<string>(116, value);

	/// <summary>
	/// Represents QuoteID, FIX tag 117, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteID(string value)
		: Typed<string>(117, value);

	/// <summary>
	/// Represents NetMoney, FIX tag 118, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NetMoney((bool Valid, decimal Value) value)
		: Typed<decimal>(118, value);

	/// <summary>
	/// Represents SettlCurrAmt, FIX tag 119, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlCurrAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(119, value);

	/// <summary>
	/// Represents SettlCurrency, FIX tag 120, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlCurrency(string value)
		: Typed<string>(120, value);

	/// <summary>
	/// Represents ForexReq, FIX tag 121, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ForexReq((bool Valid, bool Value) value)
		: Typed<bool>(121, value);

	/// <summary>
	/// Represents OrigSendingTime, FIX tag 122, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrigSendingTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(122, value);

	/// <summary>
	/// Represents GapFillFlag, FIX tag 123, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class GapFillFlag((bool Valid, bool Value) value)
		: Typed<bool>(123, value);

	/// <summary>
	/// Represents NoExecs, FIX tag 124, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoExecs((bool Valid, long Value) value)
		: Typed<long>(124, value);

	/// <summary>
	/// Represents ExpireTime, FIX tag 126, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExpireTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(126, value);

	/// <summary>
	/// Represents DKReason, FIX tag 127, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DKReason((bool Valid, char Value) value)
		: Typed<char>(127, value);

	/// <summary>
	/// Represents DeliverToCompID, FIX tag 128, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class DeliverToCompID(string value)
		: Typed<string>(128, value);

	/// <summary>
	/// Represents DeliverToSubID, FIX tag 129, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class DeliverToSubID(string value)
		: Typed<string>(129, value);

	/// <summary>
	/// Represents IOINaturalFlag, FIX tag 130, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IOINaturalFlag((bool Valid, bool Value) value)
		: Typed<bool>(130, value);

	/// <summary>
	/// Represents QuoteReqID, FIX tag 131, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteReqID(string value)
		: Typed<string>(131, value);

	/// <summary>
	/// Represents BidPx, FIX tag 132, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidPx((bool Valid, decimal Value) value)
		: Typed<decimal>(132, value);

	/// <summary>
	/// Represents OfferPx, FIX tag 133, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferPx((bool Valid, decimal Value) value)
		: Typed<decimal>(133, value);

	/// <summary>
	/// Represents BidSize, FIX tag 134, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidSize((bool Valid, decimal Value) value)
		: Typed<decimal>(134, value);

	/// <summary>
	/// Represents OfferSize, FIX tag 135, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferSize((bool Valid, decimal Value) value)
		: Typed<decimal>(135, value);

	/// <summary>
	/// Represents NoMiscFees, FIX tag 136, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoMiscFees((bool Valid, long Value) value)
		: Typed<long>(136, value);

	/// <summary>
	/// Represents MiscFeeAmt, FIX tag 137, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MiscFeeAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(137, value);

	/// <summary>
	/// Represents MiscFeeCurr, FIX tag 138, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MiscFeeCurr(string value)
		: Typed<string>(138, value);

	/// <summary>
	/// Represents MiscFeeType, FIX tag 139, with wire type <c>String</c>.
	/// </summary>
	/// <remarks>
	/// FIX 4.4 declares this field <c>char</c> and, in the same table, publishes the values 10, 11
	/// and 12 for it, which a single character cannot hold. Where the declared type cannot hold
	/// what the specification publishes, this package reads the field as a string: it keeps every
	/// value the document prints, and which of them are allowed is the code set's answer rather
	/// than the type's shape.
	/// </remarks>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MiscFeeType(string value)
		: Typed<string>(139, value);

	/// <summary>
	/// Represents PrevClosePx, FIX tag 140, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PrevClosePx((bool Valid, decimal Value) value)
		: Typed<decimal>(140, value);

	/// <summary>
	/// Represents ResetSeqNumFlag, FIX tag 141, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ResetSeqNumFlag((bool Valid, bool Value) value)
		: Typed<bool>(141, value);

	/// <summary>
	/// Represents SenderLocationID, FIX tag 142, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SenderLocationID(string value)
		: Typed<string>(142, value);

	/// <summary>
	/// Represents TargetLocationID, FIX tag 143, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TargetLocationID(string value)
		: Typed<string>(143, value);

	/// <summary>
	/// Represents OnBehalfOfLocationID, FIX tag 144, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OnBehalfOfLocationID(string value)
		: Typed<string>(144, value);

	/// <summary>
	/// Represents DeliverToLocationID, FIX tag 145, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class DeliverToLocationID(string value)
		: Typed<string>(145, value);

	/// <summary>
	/// Represents NoRelatedSym, FIX tag 146, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoRelatedSym((bool Valid, long Value) value)
		: Typed<long>(146, value);

	/// <summary>
	/// Represents Subject, FIX tag 147, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Subject(string value)
		: Typed<string>(147, value);

	/// <summary>
	/// Represents Headline, FIX tag 148, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Headline(string value)
		: Typed<string>(148, value);

	/// <summary>
	/// Represents URLLink, FIX tag 149, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class URLLink(string value)
		: Typed<string>(149, value);

	/// <summary>
	/// Represents ExecType, FIX tag 150, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecType((bool Valid, char Value) value)
		: Typed<char>(150, value);

	/// <summary>
	/// Represents LeavesQty, FIX tag 151, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LeavesQty((bool Valid, decimal Value) value)
		: Typed<decimal>(151, value);

	/// <summary>
	/// Represents CashOrderQty, FIX tag 152, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CashOrderQty((bool Valid, decimal Value) value)
		: Typed<decimal>(152, value);

	/// <summary>
	/// Represents AllocAvgPx, FIX tag 153, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocAvgPx((bool Valid, decimal Value) value)
		: Typed<decimal>(153, value);

	/// <summary>
	/// Represents AllocNetMoney, FIX tag 154, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocNetMoney((bool Valid, decimal Value) value)
		: Typed<decimal>(154, value);

	/// <summary>
	/// Represents SettlCurrFxRate, FIX tag 155, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlCurrFxRate((bool Valid, decimal Value) value)
		: Typed<decimal>(155, value);

	/// <summary>
	/// Represents SettlCurrFxRateCalc, FIX tag 156, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlCurrFxRateCalc((bool Valid, char Value) value)
		: Typed<char>(156, value);

	/// <summary>
	/// Represents NumDaysInterest, FIX tag 157, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NumDaysInterest((bool Valid, long Value) value)
		: Typed<long>(157, value);

	/// <summary>
	/// Represents AccruedInterestRate, FIX tag 158, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AccruedInterestRate((bool Valid, decimal Value) value)
		: Typed<decimal>(158, value);

	/// <summary>
	/// Represents AccruedInterestAmt, FIX tag 159, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AccruedInterestAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(159, value);

	/// <summary>
	/// Represents SettlInstMode, FIX tag 160, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlInstMode((bool Valid, char Value) value)
		: Typed<char>(160, value);

	/// <summary>
	/// Represents AllocText, FIX tag 161, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocText(string value)
		: Typed<string>(161, value);

	/// <summary>
	/// Represents SettlInstID, FIX tag 162, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlInstID(string value)
		: Typed<string>(162, value);

	/// <summary>
	/// Represents SettlInstTransType, FIX tag 163, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlInstTransType((bool Valid, char Value) value)
		: Typed<char>(163, value);

	/// <summary>
	/// Represents EmailThreadID, FIX tag 164, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class EmailThreadID(string value)
		: Typed<string>(164, value);

	/// <summary>
	/// Represents SettlInstSource, FIX tag 165, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlInstSource((bool Valid, char Value) value)
		: Typed<char>(165, value);

	/// <summary>
	/// Represents SecurityType, FIX tag 167, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityType(string value)
		: Typed<string>(167, value);

	/// <summary>
	/// Represents EffectiveTime, FIX tag 168, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EffectiveTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(168, value);

	/// <summary>
	/// Represents StandInstDbType, FIX tag 169, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StandInstDbType((bool Valid, long Value) value)
		: Typed<long>(169, value);

	/// <summary>
	/// Represents StandInstDbName, FIX tag 170, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StandInstDbName(string value)
		: Typed<string>(170, value);

	/// <summary>
	/// Represents StandInstDbID, FIX tag 171, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StandInstDbID(string value)
		: Typed<string>(171, value);

	/// <summary>
	/// Represents SettlDeliveryType, FIX tag 172, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlDeliveryType((bool Valid, long Value) value)
		: Typed<long>(172, value);

	/// <summary>
	/// Represents BidSpotRate, FIX tag 188, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidSpotRate((bool Valid, decimal Value) value)
		: Typed<decimal>(188, value);

	/// <summary>
	/// Represents BidForwardPoints, FIX tag 189, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidForwardPoints((bool Valid, decimal Value) value)
		: Typed<decimal>(189, value);

	/// <summary>
	/// Represents OfferSpotRate, FIX tag 190, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferSpotRate((bool Valid, decimal Value) value)
		: Typed<decimal>(190, value);

	/// <summary>
	/// Represents OfferForwardPoints, FIX tag 191, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferForwardPoints((bool Valid, decimal Value) value)
		: Typed<decimal>(191, value);

	/// <summary>
	/// Represents OrderQty2, FIX tag 192, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderQty2((bool Valid, decimal Value) value)
		: Typed<decimal>(192, value);

	/// <summary>
	/// Represents SettlDate2, FIX tag 193, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlDate2((bool Valid, FixDate Value) value)
		: Typed<FixDate>(193, value);

	/// <summary>
	/// Represents LastSpotRate, FIX tag 194, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastSpotRate((bool Valid, decimal Value) value)
		: Typed<decimal>(194, value);

	/// <summary>
	/// Represents LastForwardPoints, FIX tag 195, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastForwardPoints((bool Valid, decimal Value) value)
		: Typed<decimal>(195, value);

	/// <summary>
	/// Represents AllocLinkID, FIX tag 196, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocLinkID(string value)
		: Typed<string>(196, value);

	/// <summary>
	/// Represents AllocLinkType, FIX tag 197, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocLinkType((bool Valid, long Value) value)
		: Typed<long>(197, value);

	/// <summary>
	/// Represents SecondaryOrderID, FIX tag 198, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryOrderID(string value)
		: Typed<string>(198, value);

	/// <summary>
	/// Represents NoIOIQualifiers, FIX tag 199, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoIOIQualifiers((bool Valid, long Value) value)
		: Typed<long>(199, value);

	/// <summary>
	/// Represents MaturityMonthYear, FIX tag 200, with wire type <c>MonthYear</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaturityMonthYear((bool Valid, FixMonthYear Value) value)
		: Typed<FixMonthYear>(200, value);

	/// <summary>
	/// Represents PutOrCall, FIX tag 201, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PutOrCall((bool Valid, long Value) value)
		: Typed<long>(201, value);

	/// <summary>
	/// Represents StrikePrice, FIX tag 202, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StrikePrice((bool Valid, decimal Value) value)
		: Typed<decimal>(202, value);

	/// <summary>
	/// Represents CoveredOrUncovered, FIX tag 203, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CoveredOrUncovered((bool Valid, long Value) value)
		: Typed<long>(203, value);

	/// <summary>
	/// Represents OptAttribute, FIX tag 206, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OptAttribute((bool Valid, char Value) value)
		: Typed<char>(206, value);

	/// <summary>
	/// Represents SecurityExchange, FIX tag 207, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityExchange(string value)
		: Typed<string>(207, value);

	/// <summary>
	/// Represents NotifyBrokerOfCredit, FIX tag 208, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NotifyBrokerOfCredit((bool Valid, bool Value) value)
		: Typed<bool>(208, value);

	/// <summary>
	/// Represents AllocHandlInst, FIX tag 209, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocHandlInst((bool Valid, long Value) value)
		: Typed<long>(209, value);

	/// <summary>
	/// Represents MaxShow, FIX tag 210, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaxShow((bool Valid, decimal Value) value)
		: Typed<decimal>(210, value);

	/// <summary>
	/// Represents PegOffsetValue, FIX tag 211, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegOffsetValue((bool Valid, decimal Value) value)
		: Typed<decimal>(211, value);

	/// <summary>
	/// Represents XmlDataLen, FIX tag 212, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class XmlDataLen((bool Valid, long Value) value)
		: Typed<long>(212, value);

	/// <summary>
	/// Represents XmlData, FIX tag 213, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class XmlData((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(213, value);

	/// <summary>
	/// Represents SettlInstRefID, FIX tag 214, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlInstRefID(string value)
		: Typed<string>(214, value);

	/// <summary>
	/// Represents NoRoutingIDs, FIX tag 215, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoRoutingIDs((bool Valid, long Value) value)
		: Typed<long>(215, value);

	/// <summary>
	/// Represents RoutingType, FIX tag 216, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RoutingType((bool Valid, long Value) value)
		: Typed<long>(216, value);

	/// <summary>
	/// Represents RoutingID, FIX tag 217, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RoutingID(string value)
		: Typed<string>(217, value);

	/// <summary>
	/// Represents Spread, FIX tag 218, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Spread((bool Valid, decimal Value) value)
		: Typed<decimal>(218, value);

	/// <summary>
	/// Represents BenchmarkCurveCurrency, FIX tag 220, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BenchmarkCurveCurrency(string value)
		: Typed<string>(220, value);

	/// <summary>
	/// Represents BenchmarkCurveName, FIX tag 221, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BenchmarkCurveName(string value)
		: Typed<string>(221, value);

	/// <summary>
	/// Represents BenchmarkCurvePoint, FIX tag 222, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BenchmarkCurvePoint(string value)
		: Typed<string>(222, value);

	/// <summary>
	/// Represents CouponRate, FIX tag 223, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CouponRate((bool Valid, decimal Value) value)
		: Typed<decimal>(223, value);

	/// <summary>
	/// Represents CouponPaymentDate, FIX tag 224, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CouponPaymentDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(224, value);

	/// <summary>
	/// Represents IssueDate, FIX tag 225, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IssueDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(225, value);

	/// <summary>
	/// Represents RepurchaseTerm, FIX tag 226, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RepurchaseTerm((bool Valid, long Value) value)
		: Typed<long>(226, value);

	/// <summary>
	/// Represents RepurchaseRate, FIX tag 227, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RepurchaseRate((bool Valid, decimal Value) value)
		: Typed<decimal>(227, value);

	/// <summary>
	/// Represents Factor, FIX tag 228, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Factor((bool Valid, decimal Value) value)
		: Typed<decimal>(228, value);

	/// <summary>
	/// Represents TradeOriginationDate, FIX tag 229, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeOriginationDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(229, value);

	/// <summary>
	/// Represents ExDate, FIX tag 230, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(230, value);

	/// <summary>
	/// Represents ContractMultiplier, FIX tag 231, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContractMultiplier((bool Valid, decimal Value) value)
		: Typed<decimal>(231, value);

	/// <summary>
	/// Represents NoStipulations, FIX tag 232, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoStipulations((bool Valid, long Value) value)
		: Typed<long>(232, value);

	/// <summary>
	/// Represents StipulationType, FIX tag 233, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StipulationType(string value)
		: Typed<string>(233, value);

	/// <summary>
	/// Represents StipulationValue, FIX tag 234, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StipulationValue(string value)
		: Typed<string>(234, value);

	/// <summary>
	/// Represents YieldType, FIX tag 235, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class YieldType(string value)
		: Typed<string>(235, value);

	/// <summary>
	/// Represents Yield, FIX tag 236, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Yield((bool Valid, decimal Value) value)
		: Typed<decimal>(236, value);

	/// <summary>
	/// Represents TotalTakedown, FIX tag 237, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalTakedown((bool Valid, decimal Value) value)
		: Typed<decimal>(237, value);

	/// <summary>
	/// Represents Concession, FIX tag 238, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Concession((bool Valid, decimal Value) value)
		: Typed<decimal>(238, value);

	/// <summary>
	/// Represents RepoCollateralSecurityType, FIX tag 239, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RepoCollateralSecurityType(string value)
		: Typed<string>(239, value);

	/// <summary>
	/// Represents RedemptionDate, FIX tag 240, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RedemptionDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(240, value);

	/// <summary>
	/// Represents UnderlyingCouponPaymentDate, FIX tag 241, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingCouponPaymentDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(241, value);

	/// <summary>
	/// Represents UnderlyingIssueDate, FIX tag 242, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingIssueDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(242, value);

	/// <summary>
	/// Represents UnderlyingRepoCollateralSecurityType, FIX tag 243, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingRepoCollateralSecurityType(string value)
		: Typed<string>(243, value);

	/// <summary>
	/// Represents UnderlyingRepurchaseTerm, FIX tag 244, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingRepurchaseTerm((bool Valid, long Value) value)
		: Typed<long>(244, value);

	/// <summary>
	/// Represents UnderlyingRepurchaseRate, FIX tag 245, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingRepurchaseRate((bool Valid, decimal Value) value)
		: Typed<decimal>(245, value);

	/// <summary>
	/// Represents UnderlyingFactor, FIX tag 246, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingFactor((bool Valid, decimal Value) value)
		: Typed<decimal>(246, value);

	/// <summary>
	/// Represents UnderlyingRedemptionDate, FIX tag 247, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingRedemptionDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(247, value);

	/// <summary>
	/// Represents LegCouponPaymentDate, FIX tag 248, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegCouponPaymentDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(248, value);

	/// <summary>
	/// Represents LegIssueDate, FIX tag 249, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegIssueDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(249, value);

	/// <summary>
	/// Represents LegRepoCollateralSecurityType, FIX tag 250, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegRepoCollateralSecurityType(string value)
		: Typed<string>(250, value);

	/// <summary>
	/// Represents LegRepurchaseTerm, FIX tag 251, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegRepurchaseTerm((bool Valid, long Value) value)
		: Typed<long>(251, value);

	/// <summary>
	/// Represents LegRepurchaseRate, FIX tag 252, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegRepurchaseRate((bool Valid, decimal Value) value)
		: Typed<decimal>(252, value);

	/// <summary>
	/// Represents LegFactor, FIX tag 253, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegFactor((bool Valid, decimal Value) value)
		: Typed<decimal>(253, value);

	/// <summary>
	/// Represents LegRedemptionDate, FIX tag 254, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegRedemptionDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(254, value);

	/// <summary>
	/// Represents CreditRating, FIX tag 255, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CreditRating(string value)
		: Typed<string>(255, value);

	/// <summary>
	/// Represents UnderlyingCreditRating, FIX tag 256, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCreditRating(string value)
		: Typed<string>(256, value);

	/// <summary>
	/// Represents LegCreditRating, FIX tag 257, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegCreditRating(string value)
		: Typed<string>(257, value);

	/// <summary>
	/// Represents TradedFlatSwitch, FIX tag 258, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradedFlatSwitch((bool Valid, bool Value) value)
		: Typed<bool>(258, value);

	/// <summary>
	/// Represents BasisFeatureDate, FIX tag 259, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BasisFeatureDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(259, value);

	/// <summary>
	/// Represents BasisFeaturePrice, FIX tag 260, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BasisFeaturePrice((bool Valid, decimal Value) value)
		: Typed<decimal>(260, value);

	/// <summary>
	/// Represents MDReqID, FIX tag 262, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDReqID(string value)
		: Typed<string>(262, value);

	/// <summary>
	/// Represents SubscriptionRequestType, FIX tag 263, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SubscriptionRequestType((bool Valid, char Value) value)
		: Typed<char>(263, value);

	/// <summary>
	/// Represents MarketDepth, FIX tag 264, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MarketDepth((bool Valid, long Value) value)
		: Typed<long>(264, value);

	/// <summary>
	/// Represents MDUpdateType, FIX tag 265, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDUpdateType((bool Valid, long Value) value)
		: Typed<long>(265, value);

	/// <summary>
	/// Represents AggregatedBook, FIX tag 266, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AggregatedBook((bool Valid, bool Value) value)
		: Typed<bool>(266, value);

	/// <summary>
	/// Represents NoMDEntryTypes, FIX tag 267, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoMDEntryTypes((bool Valid, long Value) value)
		: Typed<long>(267, value);

	/// <summary>
	/// Represents NoMDEntries, FIX tag 268, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoMDEntries((bool Valid, long Value) value)
		: Typed<long>(268, value);

	/// <summary>
	/// Represents MDEntryType, FIX tag 269, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntryType((bool Valid, char Value) value)
		: Typed<char>(269, value);

	/// <summary>
	/// Represents MDEntryPx, FIX tag 270, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntryPx((bool Valid, decimal Value) value)
		: Typed<decimal>(270, value);

	/// <summary>
	/// Represents MDEntrySize, FIX tag 271, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntrySize((bool Valid, decimal Value) value)
		: Typed<decimal>(271, value);

	/// <summary>
	/// Represents MDEntryDate, FIX tag 272, with wire type <c>UTCDateOnly</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntryDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(272, value);

	/// <summary>
	/// Represents MDEntryTime, FIX tag 273, with wire type <c>UTCTimeOnly</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntryTime((bool Valid, FixTime Value) value)
		: Typed<FixTime>(273, value);

	/// <summary>
	/// Represents TickDirection, FIX tag 274, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TickDirection((bool Valid, char Value) value)
		: Typed<char>(274, value);

	/// <summary>
	/// Represents MDMkt, FIX tag 275, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDMkt(string value)
		: Typed<string>(275, value);

	/// <summary>
	/// Represents QuoteCondition, FIX tag 276, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteCondition((bool Valid, string[] Value) value)
		: Typed<string[]>(276, value);

	/// <summary>
	/// Represents TradeCondition, FIX tag 277, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeCondition((bool Valid, string[] Value) value)
		: Typed<string[]>(277, value);

	/// <summary>
	/// Represents MDEntryID, FIX tag 278, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDEntryID(string value)
		: Typed<string>(278, value);

	/// <summary>
	/// Represents MDUpdateAction, FIX tag 279, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDUpdateAction((bool Valid, char Value) value)
		: Typed<char>(279, value);

	/// <summary>
	/// Represents MDEntryRefID, FIX tag 280, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDEntryRefID(string value)
		: Typed<string>(280, value);

	/// <summary>
	/// Represents MDReqRejReason, FIX tag 281, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDReqRejReason((bool Valid, char Value) value)
		: Typed<char>(281, value);

	/// <summary>
	/// Represents MDEntryOriginator, FIX tag 282, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDEntryOriginator(string value)
		: Typed<string>(282, value);

	/// <summary>
	/// Represents LocationID, FIX tag 283, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LocationID(string value)
		: Typed<string>(283, value);

	/// <summary>
	/// Represents DeskID, FIX tag 284, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class DeskID(string value)
		: Typed<string>(284, value);

	/// <summary>
	/// Represents DeleteReason, FIX tag 285, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DeleteReason((bool Valid, char Value) value)
		: Typed<char>(285, value);

	/// <summary>
	/// Represents OpenCloseSettlFlag, FIX tag 286, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OpenCloseSettlFlag((bool Valid, string[] Value) value)
		: Typed<string[]>(286, value);

	/// <summary>
	/// Represents SellerDays, FIX tag 287, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SellerDays((bool Valid, long Value) value)
		: Typed<long>(287, value);

	/// <summary>
	/// Represents MDEntryBuyer, FIX tag 288, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDEntryBuyer(string value)
		: Typed<string>(288, value);

	/// <summary>
	/// Represents MDEntrySeller, FIX tag 289, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MDEntrySeller(string value)
		: Typed<string>(289, value);

	/// <summary>
	/// Represents MDEntryPositionNo, FIX tag 290, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDEntryPositionNo((bool Valid, long Value) value)
		: Typed<long>(290, value);

	/// <summary>
	/// Represents FinancialStatus, FIX tag 291, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class FinancialStatus((bool Valid, string[] Value) value)
		: Typed<string[]>(291, value);

	/// <summary>
	/// Represents CorporateAction, FIX tag 292, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CorporateAction((bool Valid, string[] Value) value)
		: Typed<string[]>(292, value);

	/// <summary>
	/// Represents DefBidSize, FIX tag 293, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DefBidSize((bool Valid, decimal Value) value)
		: Typed<decimal>(293, value);

	/// <summary>
	/// Represents DefOfferSize, FIX tag 294, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DefOfferSize((bool Valid, decimal Value) value)
		: Typed<decimal>(294, value);

	/// <summary>
	/// Represents NoQuoteEntries, FIX tag 295, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoQuoteEntries((bool Valid, long Value) value)
		: Typed<long>(295, value);

	/// <summary>
	/// Represents NoQuoteSets, FIX tag 296, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoQuoteSets((bool Valid, long Value) value)
		: Typed<long>(296, value);

	/// <summary>
	/// Represents QuoteStatus, FIX tag 297, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteStatus((bool Valid, long Value) value)
		: Typed<long>(297, value);

	/// <summary>
	/// Represents QuoteCancelType, FIX tag 298, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteCancelType((bool Valid, long Value) value)
		: Typed<long>(298, value);

	/// <summary>
	/// Represents QuoteEntryID, FIX tag 299, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteEntryID(string value)
		: Typed<string>(299, value);

	/// <summary>
	/// Represents QuoteRejectReason, FIX tag 300, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteRejectReason((bool Valid, long Value) value)
		: Typed<long>(300, value);

	/// <summary>
	/// Represents QuoteResponseLevel, FIX tag 301, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteResponseLevel((bool Valid, long Value) value)
		: Typed<long>(301, value);

	/// <summary>
	/// Represents QuoteSetID, FIX tag 302, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteSetID(string value)
		: Typed<string>(302, value);

	/// <summary>
	/// Represents QuoteRequestType, FIX tag 303, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteRequestType((bool Valid, long Value) value)
		: Typed<long>(303, value);

	/// <summary>
	/// Represents TotNoQuoteEntries, FIX tag 304, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoQuoteEntries((bool Valid, long Value) value)
		: Typed<long>(304, value);

	/// <summary>
	/// Represents UnderlyingSecurityIDSource, FIX tag 305, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityIDSource(string value)
		: Typed<string>(305, value);

	/// <summary>
	/// Represents UnderlyingIssuer, FIX tag 306, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingIssuer(string value)
		: Typed<string>(306, value);

	/// <summary>
	/// Represents UnderlyingSecurityDesc, FIX tag 307, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityDesc(string value)
		: Typed<string>(307, value);

	/// <summary>
	/// Represents UnderlyingSecurityExchange, FIX tag 308, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityExchange(string value)
		: Typed<string>(308, value);

	/// <summary>
	/// Represents UnderlyingSecurityID, FIX tag 309, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityID(string value)
		: Typed<string>(309, value);

	/// <summary>
	/// Represents UnderlyingSecurityType, FIX tag 310, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityType(string value)
		: Typed<string>(310, value);

	/// <summary>
	/// Represents UnderlyingSymbol, FIX tag 311, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSymbol(string value)
		: Typed<string>(311, value);

	/// <summary>
	/// Represents UnderlyingSymbolSfx, FIX tag 312, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSymbolSfx(string value)
		: Typed<string>(312, value);

	/// <summary>
	/// Represents UnderlyingMaturityMonthYear, FIX tag 313, with wire type <c>MonthYear</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingMaturityMonthYear((bool Valid, FixMonthYear Value) value)
		: Typed<FixMonthYear>(313, value);

	/// <summary>
	/// Represents UnderlyingPutOrCall, FIX tag 315, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingPutOrCall((bool Valid, long Value) value)
		: Typed<long>(315, value);

	/// <summary>
	/// Represents UnderlyingStrikePrice, FIX tag 316, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingStrikePrice((bool Valid, decimal Value) value)
		: Typed<decimal>(316, value);

	/// <summary>
	/// Represents UnderlyingOptAttribute, FIX tag 317, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingOptAttribute((bool Valid, char Value) value)
		: Typed<char>(317, value);

	/// <summary>
	/// Represents UnderlyingCurrency, FIX tag 318, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCurrency(string value)
		: Typed<string>(318, value);

	/// <summary>
	/// Represents SecurityReqID, FIX tag 320, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityReqID(string value)
		: Typed<string>(320, value);

	/// <summary>
	/// Represents SecurityRequestType, FIX tag 321, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecurityRequestType((bool Valid, long Value) value)
		: Typed<long>(321, value);

	/// <summary>
	/// Represents SecurityResponseID, FIX tag 322, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityResponseID(string value)
		: Typed<string>(322, value);

	/// <summary>
	/// Represents SecurityResponseType, FIX tag 323, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecurityResponseType((bool Valid, long Value) value)
		: Typed<long>(323, value);

	/// <summary>
	/// Represents SecurityStatusReqID, FIX tag 324, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityStatusReqID(string value)
		: Typed<string>(324, value);

	/// <summary>
	/// Represents UnsolicitedIndicator, FIX tag 325, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnsolicitedIndicator((bool Valid, bool Value) value)
		: Typed<bool>(325, value);

	/// <summary>
	/// Represents SecurityTradingStatus, FIX tag 326, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecurityTradingStatus((bool Valid, long Value) value)
		: Typed<long>(326, value);

	/// <summary>
	/// Represents HaltReason, FIX tag 327, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HaltReason((bool Valid, char Value) value)
		: Typed<char>(327, value);

	/// <summary>
	/// Represents InViewOfCommon, FIX tag 328, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class InViewOfCommon((bool Valid, bool Value) value)
		: Typed<bool>(328, value);

	/// <summary>
	/// Represents DueToRelated, FIX tag 329, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DueToRelated((bool Valid, bool Value) value)
		: Typed<bool>(329, value);

	/// <summary>
	/// Represents BuyVolume, FIX tag 330, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BuyVolume((bool Valid, decimal Value) value)
		: Typed<decimal>(330, value);

	/// <summary>
	/// Represents SellVolume, FIX tag 331, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SellVolume((bool Valid, decimal Value) value)
		: Typed<decimal>(331, value);

	/// <summary>
	/// Represents HighPx, FIX tag 332, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HighPx((bool Valid, decimal Value) value)
		: Typed<decimal>(332, value);

	/// <summary>
	/// Represents LowPx, FIX tag 333, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LowPx((bool Valid, decimal Value) value)
		: Typed<decimal>(333, value);

	/// <summary>
	/// Represents Adjustment, FIX tag 334, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Adjustment((bool Valid, long Value) value)
		: Typed<long>(334, value);

	/// <summary>
	/// Represents TradSesReqID, FIX tag 335, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradSesReqID(string value)
		: Typed<string>(335, value);

	/// <summary>
	/// Represents TradingSessionID, FIX tag 336, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradingSessionID(string value)
		: Typed<string>(336, value);

	/// <summary>
	/// Represents ContraTrader, FIX tag 337, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ContraTrader(string value)
		: Typed<string>(337, value);

	/// <summary>
	/// Represents TradSesMethod, FIX tag 338, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesMethod((bool Valid, long Value) value)
		: Typed<long>(338, value);

	/// <summary>
	/// Represents TradSesMode, FIX tag 339, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesMode((bool Valid, long Value) value)
		: Typed<long>(339, value);

	/// <summary>
	/// Represents TradSesStatus, FIX tag 340, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesStatus((bool Valid, long Value) value)
		: Typed<long>(340, value);

	/// <summary>
	/// Represents TradSesStartTime, FIX tag 341, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesStartTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(341, value);

	/// <summary>
	/// Represents TradSesOpenTime, FIX tag 342, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesOpenTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(342, value);

	/// <summary>
	/// Represents TradSesPreCloseTime, FIX tag 343, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesPreCloseTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(343, value);

	/// <summary>
	/// Represents TradSesCloseTime, FIX tag 344, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesCloseTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(344, value);

	/// <summary>
	/// Represents TradSesEndTime, FIX tag 345, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesEndTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(345, value);

	/// <summary>
	/// Represents NumberOfOrders, FIX tag 346, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NumberOfOrders((bool Valid, long Value) value)
		: Typed<long>(346, value);

	/// <summary>
	/// Represents MessageEncoding, FIX tag 347, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MessageEncoding(string value)
		: Typed<string>(347, value);

	/// <summary>
	/// Represents EncodedIssuerLen, FIX tag 348, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedIssuerLen((bool Valid, long Value) value)
		: Typed<long>(348, value);

	/// <summary>
	/// Represents EncodedIssuer, FIX tag 349, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedIssuer((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(349, value);

	/// <summary>
	/// Represents EncodedSecurityDescLen, FIX tag 350, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedSecurityDescLen((bool Valid, long Value) value)
		: Typed<long>(350, value);

	/// <summary>
	/// Represents EncodedSecurityDesc, FIX tag 351, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedSecurityDesc((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(351, value);

	/// <summary>
	/// Represents EncodedListExecInstLen, FIX tag 352, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedListExecInstLen((bool Valid, long Value) value)
		: Typed<long>(352, value);

	/// <summary>
	/// Represents EncodedListExecInst, FIX tag 353, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedListExecInst((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(353, value);

	/// <summary>
	/// Represents EncodedTextLen, FIX tag 354, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedTextLen((bool Valid, long Value) value)
		: Typed<long>(354, value);

	/// <summary>
	/// Represents EncodedText, FIX tag 355, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedText((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(355, value);

	/// <summary>
	/// Represents EncodedSubjectLen, FIX tag 356, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedSubjectLen((bool Valid, long Value) value)
		: Typed<long>(356, value);

	/// <summary>
	/// Represents EncodedSubject, FIX tag 357, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedSubject((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(357, value);

	/// <summary>
	/// Represents EncodedHeadlineLen, FIX tag 358, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedHeadlineLen((bool Valid, long Value) value)
		: Typed<long>(358, value);

	/// <summary>
	/// Represents EncodedHeadline, FIX tag 359, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedHeadline((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(359, value);

	/// <summary>
	/// Represents EncodedAllocTextLen, FIX tag 360, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedAllocTextLen((bool Valid, long Value) value)
		: Typed<long>(360, value);

	/// <summary>
	/// Represents EncodedAllocText, FIX tag 361, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedAllocText((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(361, value);

	/// <summary>
	/// Represents EncodedUnderlyingIssuerLen, FIX tag 362, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedUnderlyingIssuerLen((bool Valid, long Value) value)
		: Typed<long>(362, value);

	/// <summary>
	/// Represents EncodedUnderlyingIssuer, FIX tag 363, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedUnderlyingIssuer((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(363, value);

	/// <summary>
	/// Represents EncodedUnderlyingSecurityDescLen, FIX tag 364, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedUnderlyingSecurityDescLen((bool Valid, long Value) value)
		: Typed<long>(364, value);

	/// <summary>
	/// Represents EncodedUnderlyingSecurityDesc, FIX tag 365, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedUnderlyingSecurityDesc((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(365, value);

	/// <summary>
	/// Represents AllocPrice, FIX tag 366, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(366, value);

	/// <summary>
	/// Represents QuoteSetValidUntilTime, FIX tag 367, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteSetValidUntilTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(367, value);

	/// <summary>
	/// Represents QuoteEntryRejectReason, FIX tag 368, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteEntryRejectReason((bool Valid, long Value) value)
		: Typed<long>(368, value);

	/// <summary>
	/// Represents LastMsgSeqNumProcessed, FIX tag 369, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastMsgSeqNumProcessed((bool Valid, long Value) value)
		: Typed<long>(369, value);

	/// <summary>
	/// Represents RefTagID, FIX tag 371, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RefTagID((bool Valid, long Value) value)
		: Typed<long>(371, value);

	/// <summary>
	/// Represents RefMsgType, FIX tag 372, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RefMsgType(string value)
		: Typed<string>(372, value);

	/// <summary>
	/// Represents SessionRejectReason, FIX tag 373, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SessionRejectReason((bool Valid, long Value) value)
		: Typed<long>(373, value);

	/// <summary>
	/// Represents BidRequestTransType, FIX tag 374, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidRequestTransType((bool Valid, char Value) value)
		: Typed<char>(374, value);

	/// <summary>
	/// Represents ContraBroker, FIX tag 375, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ContraBroker(string value)
		: Typed<string>(375, value);

	/// <summary>
	/// Represents ComplianceID, FIX tag 376, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ComplianceID(string value)
		: Typed<string>(376, value);

	/// <summary>
	/// Represents SolicitedFlag, FIX tag 377, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SolicitedFlag((bool Valid, bool Value) value)
		: Typed<bool>(377, value);

	/// <summary>
	/// Represents ExecRestatementReason, FIX tag 378, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecRestatementReason((bool Valid, long Value) value)
		: Typed<long>(378, value);

	/// <summary>
	/// Represents BusinessRejectRefID, FIX tag 379, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BusinessRejectRefID(string value)
		: Typed<string>(379, value);

	/// <summary>
	/// Represents BusinessRejectReason, FIX tag 380, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BusinessRejectReason((bool Valid, long Value) value)
		: Typed<long>(380, value);

	/// <summary>
	/// Represents GrossTradeAmt, FIX tag 381, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class GrossTradeAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(381, value);

	/// <summary>
	/// Represents NoContraBrokers, FIX tag 382, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoContraBrokers((bool Valid, long Value) value)
		: Typed<long>(382, value);

	/// <summary>
	/// Represents MaxMessageSize, FIX tag 383, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaxMessageSize((bool Valid, long Value) value)
		: Typed<long>(383, value);

	/// <summary>
	/// Represents NoMsgTypes, FIX tag 384, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoMsgTypes((bool Valid, long Value) value)
		: Typed<long>(384, value);

	/// <summary>
	/// Represents MsgDirection, FIX tag 385, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MsgDirection((bool Valid, char Value) value)
		: Typed<char>(385, value);

	/// <summary>
	/// Represents NoTradingSessions, FIX tag 386, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoTradingSessions((bool Valid, long Value) value)
		: Typed<long>(386, value);

	/// <summary>
	/// Represents TotalVolumeTraded, FIX tag 387, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalVolumeTraded((bool Valid, decimal Value) value)
		: Typed<decimal>(387, value);

	/// <summary>
	/// Represents DiscretionInst, FIX tag 388, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionInst((bool Valid, char Value) value)
		: Typed<char>(388, value);

	/// <summary>
	/// Represents DiscretionOffsetValue, FIX tag 389, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionOffsetValue((bool Valid, decimal Value) value)
		: Typed<decimal>(389, value);

	/// <summary>
	/// Represents BidID, FIX tag 390, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BidID(string value)
		: Typed<string>(390, value);

	/// <summary>
	/// Represents ClientBidID, FIX tag 391, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ClientBidID(string value)
		: Typed<string>(391, value);

	/// <summary>
	/// Represents ListName, FIX tag 392, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ListName(string value)
		: Typed<string>(392, value);

	/// <summary>
	/// Represents TotNoRelatedSym, FIX tag 393, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoRelatedSym((bool Valid, long Value) value)
		: Typed<long>(393, value);

	/// <summary>
	/// Represents BidType, FIX tag 394, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidType((bool Valid, long Value) value)
		: Typed<long>(394, value);

	/// <summary>
	/// Represents NumTickets, FIX tag 395, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NumTickets((bool Valid, long Value) value)
		: Typed<long>(395, value);

	/// <summary>
	/// Represents SideValue1, FIX tag 396, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SideValue1((bool Valid, decimal Value) value)
		: Typed<decimal>(396, value);

	/// <summary>
	/// Represents SideValue2, FIX tag 397, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SideValue2((bool Valid, decimal Value) value)
		: Typed<decimal>(397, value);

	/// <summary>
	/// Represents NoBidDescriptors, FIX tag 398, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoBidDescriptors((bool Valid, long Value) value)
		: Typed<long>(398, value);

	/// <summary>
	/// Represents BidDescriptorType, FIX tag 399, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidDescriptorType((bool Valid, long Value) value)
		: Typed<long>(399, value);

	/// <summary>
	/// Represents BidDescriptor, FIX tag 400, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BidDescriptor(string value)
		: Typed<string>(400, value);

	/// <summary>
	/// Represents SideValueInd, FIX tag 401, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SideValueInd((bool Valid, long Value) value)
		: Typed<long>(401, value);

	/// <summary>
	/// Represents LiquidityPctLow, FIX tag 402, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LiquidityPctLow((bool Valid, decimal Value) value)
		: Typed<decimal>(402, value);

	/// <summary>
	/// Represents LiquidityPctHigh, FIX tag 403, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LiquidityPctHigh((bool Valid, decimal Value) value)
		: Typed<decimal>(403, value);

	/// <summary>
	/// Represents LiquidityValue, FIX tag 404, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LiquidityValue((bool Valid, decimal Value) value)
		: Typed<decimal>(404, value);

	/// <summary>
	/// Represents EFPTrackingError, FIX tag 405, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EFPTrackingError((bool Valid, decimal Value) value)
		: Typed<decimal>(405, value);

	/// <summary>
	/// Represents FairValue, FIX tag 406, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class FairValue((bool Valid, decimal Value) value)
		: Typed<decimal>(406, value);

	/// <summary>
	/// Represents OutsideIndexPct, FIX tag 407, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OutsideIndexPct((bool Valid, decimal Value) value)
		: Typed<decimal>(407, value);

	/// <summary>
	/// Represents ValueOfFutures, FIX tag 408, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ValueOfFutures((bool Valid, decimal Value) value)
		: Typed<decimal>(408, value);

	/// <summary>
	/// Represents LiquidityIndType, FIX tag 409, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LiquidityIndType((bool Valid, long Value) value)
		: Typed<long>(409, value);

	/// <summary>
	/// Represents WtAverageLiquidity, FIX tag 410, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class WtAverageLiquidity((bool Valid, decimal Value) value)
		: Typed<decimal>(410, value);

	/// <summary>
	/// Represents ExchangeForPhysical, FIX tag 411, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExchangeForPhysical((bool Valid, bool Value) value)
		: Typed<bool>(411, value);

	/// <summary>
	/// Represents OutMainCntryUIndex, FIX tag 412, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OutMainCntryUIndex((bool Valid, decimal Value) value)
		: Typed<decimal>(412, value);

	/// <summary>
	/// Represents CrossPercent, FIX tag 413, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CrossPercent((bool Valid, decimal Value) value)
		: Typed<decimal>(413, value);

	/// <summary>
	/// Represents ProgRptReqs, FIX tag 414, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ProgRptReqs((bool Valid, long Value) value)
		: Typed<long>(414, value);

	/// <summary>
	/// Represents ProgPeriodInterval, FIX tag 415, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ProgPeriodInterval((bool Valid, long Value) value)
		: Typed<long>(415, value);

	/// <summary>
	/// Represents IncTaxInd, FIX tag 416, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IncTaxInd((bool Valid, long Value) value)
		: Typed<long>(416, value);

	/// <summary>
	/// Represents NumBidders, FIX tag 417, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NumBidders((bool Valid, long Value) value)
		: Typed<long>(417, value);

	/// <summary>
	/// Represents BidTradeType, FIX tag 418, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidTradeType((bool Valid, char Value) value)
		: Typed<char>(418, value);

	/// <summary>
	/// Represents BasisPxType, FIX tag 419, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BasisPxType((bool Valid, char Value) value)
		: Typed<char>(419, value);

	/// <summary>
	/// Represents NoBidComponents, FIX tag 420, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoBidComponents((bool Valid, long Value) value)
		: Typed<long>(420, value);

	/// <summary>
	/// Represents Country, FIX tag 421, with wire type <c>Country</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Country(string value)
		: Typed<string>(421, value);

	/// <summary>
	/// Represents TotNoStrikes, FIX tag 422, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoStrikes((bool Valid, long Value) value)
		: Typed<long>(422, value);

	/// <summary>
	/// Represents PriceType, FIX tag 423, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriceType((bool Valid, long Value) value)
		: Typed<long>(423, value);

	/// <summary>
	/// Represents DayOrderQty, FIX tag 424, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DayOrderQty((bool Valid, decimal Value) value)
		: Typed<decimal>(424, value);

	/// <summary>
	/// Represents DayCumQty, FIX tag 425, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DayCumQty((bool Valid, decimal Value) value)
		: Typed<decimal>(425, value);

	/// <summary>
	/// Represents DayAvgPx, FIX tag 426, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DayAvgPx((bool Valid, decimal Value) value)
		: Typed<decimal>(426, value);

	/// <summary>
	/// Represents GTBookingInst, FIX tag 427, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class GTBookingInst((bool Valid, long Value) value)
		: Typed<long>(427, value);

	/// <summary>
	/// Represents NoStrikes, FIX tag 428, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoStrikes((bool Valid, long Value) value)
		: Typed<long>(428, value);

	/// <summary>
	/// Represents ListStatusType, FIX tag 429, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ListStatusType((bool Valid, long Value) value)
		: Typed<long>(429, value);

	/// <summary>
	/// Represents NetGrossInd, FIX tag 430, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NetGrossInd((bool Valid, long Value) value)
		: Typed<long>(430, value);

	/// <summary>
	/// Represents ListOrderStatus, FIX tag 431, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ListOrderStatus((bool Valid, long Value) value)
		: Typed<long>(431, value);

	/// <summary>
	/// Represents ExpireDate, FIX tag 432, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExpireDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(432, value);

	/// <summary>
	/// Represents ListExecInstType, FIX tag 433, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ListExecInstType((bool Valid, char Value) value)
		: Typed<char>(433, value);

	/// <summary>
	/// Represents CxlRejResponseTo, FIX tag 434, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CxlRejResponseTo((bool Valid, char Value) value)
		: Typed<char>(434, value);

	/// <summary>
	/// Represents UnderlyingCouponRate, FIX tag 435, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingCouponRate((bool Valid, decimal Value) value)
		: Typed<decimal>(435, value);

	/// <summary>
	/// Represents UnderlyingContractMultiplier, FIX tag 436, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingContractMultiplier((bool Valid, decimal Value) value)
		: Typed<decimal>(436, value);

	/// <summary>
	/// Represents ContraTradeQty, FIX tag 437, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContraTradeQty((bool Valid, decimal Value) value)
		: Typed<decimal>(437, value);

	/// <summary>
	/// Represents ContraTradeTime, FIX tag 438, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContraTradeTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(438, value);

	/// <summary>
	/// Represents LiquidityNumSecurities, FIX tag 441, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LiquidityNumSecurities((bool Valid, long Value) value)
		: Typed<long>(441, value);

	/// <summary>
	/// Represents MultiLegReportingType, FIX tag 442, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MultiLegReportingType((bool Valid, char Value) value)
		: Typed<char>(442, value);

	/// <summary>
	/// Represents StrikeTime, FIX tag 443, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StrikeTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(443, value);

	/// <summary>
	/// Represents ListStatusText, FIX tag 444, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ListStatusText(string value)
		: Typed<string>(444, value);

	/// <summary>
	/// Represents EncodedListStatusTextLen, FIX tag 445, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedListStatusTextLen((bool Valid, long Value) value)
		: Typed<long>(445, value);

	/// <summary>
	/// Represents EncodedListStatusText, FIX tag 446, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedListStatusText((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(446, value);

	/// <summary>
	/// Represents PartyIDSource, FIX tag 447, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PartyIDSource((bool Valid, char Value) value)
		: Typed<char>(447, value);

	/// <summary>
	/// Represents PartyID, FIX tag 448, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PartyID(string value)
		: Typed<string>(448, value);

	/// <summary>
	/// Represents NetChgPrevDay, FIX tag 451, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NetChgPrevDay((bool Valid, decimal Value) value)
		: Typed<decimal>(451, value);

	/// <summary>
	/// Represents PartyRole, FIX tag 452, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PartyRole((bool Valid, long Value) value)
		: Typed<long>(452, value);

	/// <summary>
	/// Represents NoPartyIDs, FIX tag 453, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoPartyIDs((bool Valid, long Value) value)
		: Typed<long>(453, value);

	/// <summary>
	/// Represents NoSecurityAltID, FIX tag 454, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSecurityAltID((bool Valid, long Value) value)
		: Typed<long>(454, value);

	/// <summary>
	/// Represents SecurityAltID, FIX tag 455, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityAltID(string value)
		: Typed<string>(455, value);

	/// <summary>
	/// Represents SecurityAltIDSource, FIX tag 456, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecurityAltIDSource(string value)
		: Typed<string>(456, value);

	/// <summary>
	/// Represents NoUnderlyingSecurityAltID, FIX tag 457, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoUnderlyingSecurityAltID((bool Valid, long Value) value)
		: Typed<long>(457, value);

	/// <summary>
	/// Represents UnderlyingSecurityAltID, FIX tag 458, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityAltID(string value)
		: Typed<string>(458, value);

	/// <summary>
	/// Represents UnderlyingSecurityAltIDSource, FIX tag 459, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecurityAltIDSource(string value)
		: Typed<string>(459, value);

	/// <summary>
	/// Represents Product, FIX tag 460, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Product((bool Valid, long Value) value)
		: Typed<long>(460, value);

	/// <summary>
	/// Represents CFICode, FIX tag 461, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CFICode(string value)
		: Typed<string>(461, value);

	/// <summary>
	/// Represents UnderlyingProduct, FIX tag 462, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingProduct((bool Valid, long Value) value)
		: Typed<long>(462, value);

	/// <summary>
	/// Represents UnderlyingCFICode, FIX tag 463, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCFICode(string value)
		: Typed<string>(463, value);

	/// <summary>
	/// Represents TestMessageIndicator, FIX tag 464, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TestMessageIndicator((bool Valid, bool Value) value)
		: Typed<bool>(464, value);

	/// <summary>
	/// Represents BookingRefID, FIX tag 466, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BookingRefID(string value)
		: Typed<string>(466, value);

	/// <summary>
	/// Represents IndividualAllocID, FIX tag 467, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class IndividualAllocID(string value)
		: Typed<string>(467, value);

	/// <summary>
	/// Represents RoundingDirection, FIX tag 468, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RoundingDirection((bool Valid, char Value) value)
		: Typed<char>(468, value);

	/// <summary>
	/// Represents RoundingModulus, FIX tag 469, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RoundingModulus((bool Valid, decimal Value) value)
		: Typed<decimal>(469, value);

	/// <summary>
	/// Represents CountryOfIssue, FIX tag 470, with wire type <c>Country</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CountryOfIssue(string value)
		: Typed<string>(470, value);

	/// <summary>
	/// Represents StateOrProvinceOfIssue, FIX tag 471, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StateOrProvinceOfIssue(string value)
		: Typed<string>(471, value);

	/// <summary>
	/// Represents LocaleOfIssue, FIX tag 472, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LocaleOfIssue(string value)
		: Typed<string>(472, value);

	/// <summary>
	/// Represents NoRegistDtls, FIX tag 473, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoRegistDtls((bool Valid, long Value) value)
		: Typed<long>(473, value);

	/// <summary>
	/// Represents MailingDtls, FIX tag 474, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MailingDtls(string value)
		: Typed<string>(474, value);

	/// <summary>
	/// Represents InvestorCountryOfResidence, FIX tag 475, with wire type <c>Country</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class InvestorCountryOfResidence(string value)
		: Typed<string>(475, value);

	/// <summary>
	/// Represents PaymentRef, FIX tag 476, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PaymentRef(string value)
		: Typed<string>(476, value);

	/// <summary>
	/// Represents DistribPaymentMethod, FIX tag 477, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DistribPaymentMethod((bool Valid, long Value) value)
		: Typed<long>(477, value);

	/// <summary>
	/// Represents CashDistribCurr, FIX tag 478, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribCurr(string value)
		: Typed<string>(478, value);

	/// <summary>
	/// Represents CommCurrency, FIX tag 479, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CommCurrency(string value)
		: Typed<string>(479, value);

	/// <summary>
	/// Represents CancellationRights, FIX tag 480, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CancellationRights((bool Valid, char Value) value)
		: Typed<char>(480, value);

	/// <summary>
	/// Represents MoneyLaunderingStatus, FIX tag 481, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MoneyLaunderingStatus((bool Valid, char Value) value)
		: Typed<char>(481, value);

	/// <summary>
	/// Represents MailingInst, FIX tag 482, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MailingInst(string value)
		: Typed<string>(482, value);

	/// <summary>
	/// Represents TransBkdTime, FIX tag 483, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TransBkdTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(483, value);

	/// <summary>
	/// Represents ExecPriceType, FIX tag 484, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecPriceType((bool Valid, char Value) value)
		: Typed<char>(484, value);

	/// <summary>
	/// Represents ExecPriceAdjustment, FIX tag 485, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecPriceAdjustment((bool Valid, decimal Value) value)
		: Typed<decimal>(485, value);

	/// <summary>
	/// Represents DateOfBirth, FIX tag 486, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DateOfBirth((bool Valid, FixDate Value) value)
		: Typed<FixDate>(486, value);

	/// <summary>
	/// Represents TradeReportTransType, FIX tag 487, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeReportTransType((bool Valid, long Value) value)
		: Typed<long>(487, value);

	/// <summary>
	/// Represents CardHolderName, FIX tag 488, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CardHolderName(string value)
		: Typed<string>(488, value);

	/// <summary>
	/// Represents CardNumber, FIX tag 489, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CardNumber(string value)
		: Typed<string>(489, value);

	/// <summary>
	/// Represents CardExpDate, FIX tag 490, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CardExpDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(490, value);

	/// <summary>
	/// Represents CardIssNum, FIX tag 491, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CardIssNum(string value)
		: Typed<string>(491, value);

	/// <summary>
	/// Represents PaymentMethod, FIX tag 492, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PaymentMethod((bool Valid, long Value) value)
		: Typed<long>(492, value);

	/// <summary>
	/// Represents RegistAcctType, FIX tag 493, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistAcctType(string value)
		: Typed<string>(493, value);

	/// <summary>
	/// Represents Designation, FIX tag 494, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Designation(string value)
		: Typed<string>(494, value);

	/// <summary>
	/// Represents TaxAdvantageType, FIX tag 495, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TaxAdvantageType((bool Valid, long Value) value)
		: Typed<long>(495, value);

	/// <summary>
	/// Represents RegistRejReasonText, FIX tag 496, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistRejReasonText(string value)
		: Typed<string>(496, value);

	/// <summary>
	/// Represents FundRenewWaiv, FIX tag 497, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class FundRenewWaiv((bool Valid, char Value) value)
		: Typed<char>(497, value);

	/// <summary>
	/// Represents CashDistribAgentName, FIX tag 498, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribAgentName(string value)
		: Typed<string>(498, value);

	/// <summary>
	/// Represents CashDistribAgentCode, FIX tag 499, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribAgentCode(string value)
		: Typed<string>(499, value);

	/// <summary>
	/// Represents CashDistribAgentAcctNumber, FIX tag 500, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribAgentAcctNumber(string value)
		: Typed<string>(500, value);

	/// <summary>
	/// Represents CashDistribPayRef, FIX tag 501, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribPayRef(string value)
		: Typed<string>(501, value);

	/// <summary>
	/// Represents CashDistribAgentAcctName, FIX tag 502, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CashDistribAgentAcctName(string value)
		: Typed<string>(502, value);

	/// <summary>
	/// Represents CardStartDate, FIX tag 503, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CardStartDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(503, value);

	/// <summary>
	/// Represents PaymentDate, FIX tag 504, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PaymentDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(504, value);

	/// <summary>
	/// Represents PaymentRemitterID, FIX tag 505, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PaymentRemitterID(string value)
		: Typed<string>(505, value);

	/// <summary>
	/// Represents RegistStatus, FIX tag 506, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RegistStatus((bool Valid, char Value) value)
		: Typed<char>(506, value);

	/// <summary>
	/// Represents RegistRejReasonCode, FIX tag 507, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RegistRejReasonCode((bool Valid, long Value) value)
		: Typed<long>(507, value);

	/// <summary>
	/// Represents RegistRefID, FIX tag 508, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistRefID(string value)
		: Typed<string>(508, value);

	/// <summary>
	/// Represents RegistDtls, FIX tag 509, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistDtls(string value)
		: Typed<string>(509, value);

	/// <summary>
	/// Represents NoDistribInsts, FIX tag 510, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoDistribInsts((bool Valid, long Value) value)
		: Typed<long>(510, value);

	/// <summary>
	/// Represents RegistEmail, FIX tag 511, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistEmail(string value)
		: Typed<string>(511, value);

	/// <summary>
	/// Represents DistribPercentage, FIX tag 512, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DistribPercentage((bool Valid, decimal Value) value)
		: Typed<decimal>(512, value);

	/// <summary>
	/// Represents RegistID, FIX tag 513, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RegistID(string value)
		: Typed<string>(513, value);

	/// <summary>
	/// Represents RegistTransType, FIX tag 514, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RegistTransType((bool Valid, char Value) value)
		: Typed<char>(514, value);

	/// <summary>
	/// Represents ExecValuationPoint, FIX tag 515, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExecValuationPoint((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(515, value);

	/// <summary>
	/// Represents OrderPercent, FIX tag 516, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderPercent((bool Valid, decimal Value) value)
		: Typed<decimal>(516, value);

	/// <summary>
	/// Represents OwnershipType, FIX tag 517, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OwnershipType((bool Valid, char Value) value)
		: Typed<char>(517, value);

	/// <summary>
	/// Represents NoContAmts, FIX tag 518, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoContAmts((bool Valid, long Value) value)
		: Typed<long>(518, value);

	/// <summary>
	/// Represents ContAmtType, FIX tag 519, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContAmtType((bool Valid, long Value) value)
		: Typed<long>(519, value);

	/// <summary>
	/// Represents ContAmtValue, FIX tag 520, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContAmtValue((bool Valid, decimal Value) value)
		: Typed<decimal>(520, value);

	/// <summary>
	/// Represents ContAmtCurr, FIX tag 521, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ContAmtCurr(string value)
		: Typed<string>(521, value);

	/// <summary>
	/// Represents OwnerType, FIX tag 522, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OwnerType((bool Valid, long Value) value)
		: Typed<long>(522, value);

	/// <summary>
	/// Represents PartySubID, FIX tag 523, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PartySubID(string value)
		: Typed<string>(523, value);

	/// <summary>
	/// Represents NestedPartyID, FIX tag 524, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class NestedPartyID(string value)
		: Typed<string>(524, value);

	/// <summary>
	/// Represents NestedPartyIDSource, FIX tag 525, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NestedPartyIDSource((bool Valid, char Value) value)
		: Typed<char>(525, value);

	/// <summary>
	/// Represents SecondaryClOrdID, FIX tag 526, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryClOrdID(string value)
		: Typed<string>(526, value);

	/// <summary>
	/// Represents SecondaryExecID, FIX tag 527, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryExecID(string value)
		: Typed<string>(527, value);

	/// <summary>
	/// Represents OrderCapacity, FIX tag 528, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderCapacity((bool Valid, char Value) value)
		: Typed<char>(528, value);

	/// <summary>
	/// Represents OrderRestrictions, FIX tag 529, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderRestrictions((bool Valid, string[] Value) value)
		: Typed<string[]>(529, value);

	/// <summary>
	/// Represents MassCancelRequestType, FIX tag 530, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MassCancelRequestType((bool Valid, char Value) value)
		: Typed<char>(530, value);

	/// <summary>
	/// Represents MassCancelResponse, FIX tag 531, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MassCancelResponse((bool Valid, char Value) value)
		: Typed<char>(531, value);

	/// <summary>
	/// Represents MassCancelRejectReason, FIX tag 532, with wire type <c>String</c>.
	/// </summary>
	/// <remarks>
	/// FIX 4.4 declares this field <c>char</c> and, in the same table, publishes the value 99 for
	/// it, which a single character cannot hold. Where the declared type cannot hold what the
	/// specification publishes, this package reads the field as a string: it keeps every value the
	/// document prints, and which of them are allowed is the code set's answer rather than the
	/// type's shape.
	/// </remarks>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MassCancelRejectReason(string value)
		: Typed<string>(532, value);

	/// <summary>
	/// Represents TotalAffectedOrders, FIX tag 533, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalAffectedOrders((bool Valid, long Value) value)
		: Typed<long>(533, value);

	/// <summary>
	/// Represents NoAffectedOrders, FIX tag 534, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoAffectedOrders((bool Valid, long Value) value)
		: Typed<long>(534, value);

	/// <summary>
	/// Represents AffectedOrderID, FIX tag 535, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AffectedOrderID(string value)
		: Typed<string>(535, value);

	/// <summary>
	/// Represents AffectedSecondaryOrderID, FIX tag 536, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AffectedSecondaryOrderID(string value)
		: Typed<string>(536, value);

	/// <summary>
	/// Represents QuoteType, FIX tag 537, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteType((bool Valid, long Value) value)
		: Typed<long>(537, value);

	/// <summary>
	/// Represents NestedPartyRole, FIX tag 538, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NestedPartyRole((bool Valid, long Value) value)
		: Typed<long>(538, value);

	/// <summary>
	/// Represents NoNestedPartyIDs, FIX tag 539, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNestedPartyIDs((bool Valid, long Value) value)
		: Typed<long>(539, value);

	/// <summary>
	/// Represents TotalAccruedInterestAmt, FIX tag 540, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalAccruedInterestAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(540, value);

	/// <summary>
	/// Represents MaturityDate, FIX tag 541, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaturityDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(541, value);

	/// <summary>
	/// Represents UnderlyingMaturityDate, FIX tag 542, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingMaturityDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(542, value);

	/// <summary>
	/// Represents InstrRegistry, FIX tag 543, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class InstrRegistry(string value)
		: Typed<string>(543, value);

	/// <summary>
	/// Represents CashMargin, FIX tag 544, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CashMargin((bool Valid, char Value) value)
		: Typed<char>(544, value);

	/// <summary>
	/// Represents NestedPartySubID, FIX tag 545, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class NestedPartySubID(string value)
		: Typed<string>(545, value);

	/// <summary>
	/// Represents Scope, FIX tag 546, with wire type <c>MultipleValueString</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Scope((bool Valid, string[] Value) value)
		: Typed<string[]>(546, value);

	/// <summary>
	/// Represents MDImplicitDelete, FIX tag 547, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MDImplicitDelete((bool Valid, bool Value) value)
		: Typed<bool>(547, value);

	/// <summary>
	/// Represents CrossID, FIX tag 548, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CrossID(string value)
		: Typed<string>(548, value);

	/// <summary>
	/// Represents CrossType, FIX tag 549, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CrossType((bool Valid, long Value) value)
		: Typed<long>(549, value);

	/// <summary>
	/// Represents CrossPrioritization, FIX tag 550, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CrossPrioritization((bool Valid, long Value) value)
		: Typed<long>(550, value);

	/// <summary>
	/// Represents OrigCrossID, FIX tag 551, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrigCrossID(string value)
		: Typed<string>(551, value);

	/// <summary>
	/// Represents NoSides, FIX tag 552, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSides((bool Valid, long Value) value)
		: Typed<long>(552, value);

	/// <summary>
	/// Represents Username, FIX tag 553, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Username(string value)
		: Typed<string>(553, value);

	/// <summary>
	/// Represents Password, FIX tag 554, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Password(string value)
		: Typed<string>(554, value);

	/// <summary>
	/// Represents NoLegs, FIX tag 555, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoLegs((bool Valid, long Value) value)
		: Typed<long>(555, value);

	/// <summary>
	/// Represents LegCurrency, FIX tag 556, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegCurrency(string value)
		: Typed<string>(556, value);

	/// <summary>
	/// Represents TotNoSecurityTypes, FIX tag 557, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoSecurityTypes((bool Valid, long Value) value)
		: Typed<long>(557, value);

	/// <summary>
	/// Represents NoSecurityTypes, FIX tag 558, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSecurityTypes((bool Valid, long Value) value)
		: Typed<long>(558, value);

	/// <summary>
	/// Represents SecurityListRequestType, FIX tag 559, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecurityListRequestType((bool Valid, long Value) value)
		: Typed<long>(559, value);

	/// <summary>
	/// Represents SecurityRequestResult, FIX tag 560, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecurityRequestResult((bool Valid, long Value) value)
		: Typed<long>(560, value);

	/// <summary>
	/// Represents RoundLot, FIX tag 561, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class RoundLot((bool Valid, decimal Value) value)
		: Typed<decimal>(561, value);

	/// <summary>
	/// Represents MinTradeVol, FIX tag 562, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MinTradeVol((bool Valid, decimal Value) value)
		: Typed<decimal>(562, value);

	/// <summary>
	/// Represents MultiLegRptTypeReq, FIX tag 563, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MultiLegRptTypeReq((bool Valid, long Value) value)
		: Typed<long>(563, value);

	/// <summary>
	/// Represents LegPositionEffect, FIX tag 564, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegPositionEffect((bool Valid, char Value) value)
		: Typed<char>(564, value);

	/// <summary>
	/// Represents LegCoveredOrUncovered, FIX tag 565, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegCoveredOrUncovered((bool Valid, long Value) value)
		: Typed<long>(565, value);

	/// <summary>
	/// Represents LegPrice, FIX tag 566, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(566, value);

	/// <summary>
	/// Represents TradSesStatusRejReason, FIX tag 567, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradSesStatusRejReason((bool Valid, long Value) value)
		: Typed<long>(567, value);

	/// <summary>
	/// Represents TradeRequestID, FIX tag 568, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeRequestID(string value)
		: Typed<string>(568, value);

	/// <summary>
	/// Represents TradeRequestType, FIX tag 569, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeRequestType((bool Valid, long Value) value)
		: Typed<long>(569, value);

	/// <summary>
	/// Represents PreviouslyReported, FIX tag 570, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PreviouslyReported((bool Valid, bool Value) value)
		: Typed<bool>(570, value);

	/// <summary>
	/// Represents TradeReportID, FIX tag 571, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeReportID(string value)
		: Typed<string>(571, value);

	/// <summary>
	/// Represents TradeReportRefID, FIX tag 572, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeReportRefID(string value)
		: Typed<string>(572, value);

	/// <summary>
	/// Represents MatchStatus, FIX tag 573, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MatchStatus((bool Valid, char Value) value)
		: Typed<char>(573, value);

	/// <summary>
	/// Represents MatchType, FIX tag 574, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MatchType(string value)
		: Typed<string>(574, value);

	/// <summary>
	/// Represents OddLot, FIX tag 575, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OddLot((bool Valid, bool Value) value)
		: Typed<bool>(575, value);

	/// <summary>
	/// Represents NoClearingInstructions, FIX tag 576, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoClearingInstructions((bool Valid, long Value) value)
		: Typed<long>(576, value);

	/// <summary>
	/// Represents ClearingInstruction, FIX tag 577, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ClearingInstruction((bool Valid, long Value) value)
		: Typed<long>(577, value);

	/// <summary>
	/// Represents TradeInputSource, FIX tag 578, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeInputSource(string value)
		: Typed<string>(578, value);

	/// <summary>
	/// Represents TradeInputDevice, FIX tag 579, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeInputDevice(string value)
		: Typed<string>(579, value);

	/// <summary>
	/// Represents NoDates, FIX tag 580, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoDates((bool Valid, long Value) value)
		: Typed<long>(580, value);

	/// <summary>
	/// Represents AccountType, FIX tag 581, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AccountType((bool Valid, long Value) value)
		: Typed<long>(581, value);

	/// <summary>
	/// Represents CustOrderCapacity, FIX tag 582, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CustOrderCapacity((bool Valid, long Value) value)
		: Typed<long>(582, value);

	/// <summary>
	/// Represents ClOrdLinkID, FIX tag 583, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ClOrdLinkID(string value)
		: Typed<string>(583, value);

	/// <summary>
	/// Represents MassStatusReqID, FIX tag 584, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class MassStatusReqID(string value)
		: Typed<string>(584, value);

	/// <summary>
	/// Represents MassStatusReqType, FIX tag 585, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MassStatusReqType((bool Valid, long Value) value)
		: Typed<long>(585, value);

	/// <summary>
	/// Represents OrigOrdModTime, FIX tag 586, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrigOrdModTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(586, value);

	/// <summary>
	/// Represents LegSettlType, FIX tag 587, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegSettlType((bool Valid, char Value) value)
		: Typed<char>(587, value);

	/// <summary>
	/// Represents LegSettlDate, FIX tag 588, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegSettlDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(588, value);

	/// <summary>
	/// Represents DayBookingInst, FIX tag 589, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DayBookingInst((bool Valid, char Value) value)
		: Typed<char>(589, value);

	/// <summary>
	/// Represents BookingUnit, FIX tag 590, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BookingUnit((bool Valid, char Value) value)
		: Typed<char>(590, value);

	/// <summary>
	/// Represents PreallocMethod, FIX tag 591, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PreallocMethod((bool Valid, char Value) value)
		: Typed<char>(591, value);

	/// <summary>
	/// Represents UnderlyingCountryOfIssue, FIX tag 592, with wire type <c>Country</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCountryOfIssue(string value)
		: Typed<string>(592, value);

	/// <summary>
	/// Represents UnderlyingStateOrProvinceOfIssue, FIX tag 593, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingStateOrProvinceOfIssue(string value)
		: Typed<string>(593, value);

	/// <summary>
	/// Represents UnderlyingLocaleOfIssue, FIX tag 594, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingLocaleOfIssue(string value)
		: Typed<string>(594, value);

	/// <summary>
	/// Represents UnderlyingInstrRegistry, FIX tag 595, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingInstrRegistry(string value)
		: Typed<string>(595, value);

	/// <summary>
	/// Represents LegCountryOfIssue, FIX tag 596, with wire type <c>Country</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegCountryOfIssue(string value)
		: Typed<string>(596, value);

	/// <summary>
	/// Represents LegStateOrProvinceOfIssue, FIX tag 597, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegStateOrProvinceOfIssue(string value)
		: Typed<string>(597, value);

	/// <summary>
	/// Represents LegLocaleOfIssue, FIX tag 598, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegLocaleOfIssue(string value)
		: Typed<string>(598, value);

	/// <summary>
	/// Represents LegInstrRegistry, FIX tag 599, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegInstrRegistry(string value)
		: Typed<string>(599, value);

	/// <summary>
	/// Represents LegSymbol, FIX tag 600, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSymbol(string value)
		: Typed<string>(600, value);

	/// <summary>
	/// Represents LegSymbolSfx, FIX tag 601, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSymbolSfx(string value)
		: Typed<string>(601, value);

	/// <summary>
	/// Represents LegSecurityID, FIX tag 602, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityID(string value)
		: Typed<string>(602, value);

	/// <summary>
	/// Represents LegSecurityIDSource, FIX tag 603, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityIDSource(string value)
		: Typed<string>(603, value);

	/// <summary>
	/// Represents NoLegSecurityAltID, FIX tag 604, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoLegSecurityAltID((bool Valid, long Value) value)
		: Typed<long>(604, value);

	/// <summary>
	/// Represents LegSecurityAltID, FIX tag 605, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityAltID(string value)
		: Typed<string>(605, value);

	/// <summary>
	/// Represents LegSecurityAltIDSource, FIX tag 606, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityAltIDSource(string value)
		: Typed<string>(606, value);

	/// <summary>
	/// Represents LegProduct, FIX tag 607, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegProduct((bool Valid, long Value) value)
		: Typed<long>(607, value);

	/// <summary>
	/// Represents LegCFICode, FIX tag 608, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegCFICode(string value)
		: Typed<string>(608, value);

	/// <summary>
	/// Represents LegSecurityType, FIX tag 609, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityType(string value)
		: Typed<string>(609, value);

	/// <summary>
	/// Represents LegMaturityMonthYear, FIX tag 610, with wire type <c>MonthYear</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegMaturityMonthYear((bool Valid, FixMonthYear Value) value)
		: Typed<FixMonthYear>(610, value);

	/// <summary>
	/// Represents LegMaturityDate, FIX tag 611, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegMaturityDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(611, value);

	/// <summary>
	/// Represents LegStrikePrice, FIX tag 612, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegStrikePrice((bool Valid, decimal Value) value)
		: Typed<decimal>(612, value);

	/// <summary>
	/// Represents LegOptAttribute, FIX tag 613, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegOptAttribute((bool Valid, char Value) value)
		: Typed<char>(613, value);

	/// <summary>
	/// Represents LegContractMultiplier, FIX tag 614, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegContractMultiplier((bool Valid, decimal Value) value)
		: Typed<decimal>(614, value);

	/// <summary>
	/// Represents LegCouponRate, FIX tag 615, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegCouponRate((bool Valid, decimal Value) value)
		: Typed<decimal>(615, value);

	/// <summary>
	/// Represents LegSecurityExchange, FIX tag 616, with wire type <c>Exchange</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityExchange(string value)
		: Typed<string>(616, value);

	/// <summary>
	/// Represents LegIssuer, FIX tag 617, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegIssuer(string value)
		: Typed<string>(617, value);

	/// <summary>
	/// Represents EncodedLegIssuerLen, FIX tag 618, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedLegIssuerLen((bool Valid, long Value) value)
		: Typed<long>(618, value);

	/// <summary>
	/// Represents EncodedLegIssuer, FIX tag 619, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedLegIssuer((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(619, value);

	/// <summary>
	/// Represents LegSecurityDesc, FIX tag 620, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecurityDesc(string value)
		: Typed<string>(620, value);

	/// <summary>
	/// Represents EncodedLegSecurityDescLen, FIX tag 621, with wire type <c>Length</c>.
	/// </summary>
	/// <remarks>
	/// When configured as a binary length tag, the parser combines this field with the
	/// following data field and returns the data case instead of a separate length case.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedLegSecurityDescLen((bool Valid, long Value) value)
		: Typed<long>(621, value);

	/// <summary>
	/// Represents EncodedLegSecurityDesc, FIX tag 622, with wire type <c>data</c>.
	/// </summary>
	/// <remarks>
	/// The parser reads the payload using the preceding length field, so embedded separators
	/// are part of the value. The resulting field covers the complete length/data pair.
	/// </remarks>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EncodedLegSecurityDesc((bool Valid, ReadOnlyMemory<byte> Value) value)
		: Typed<ReadOnlyMemory<byte>>(622, value);

	/// <summary>
	/// Represents LegRatioQty, FIX tag 623, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegRatioQty((bool Valid, decimal Value) value)
		: Typed<decimal>(623, value);

	/// <summary>
	/// Represents LegSide, FIX tag 624, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegSide((bool Valid, char Value) value)
		: Typed<char>(624, value);

	/// <summary>
	/// Represents TradingSessionSubID, FIX tag 625, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradingSessionSubID(string value)
		: Typed<string>(625, value);

	/// <summary>
	/// Represents AllocType, FIX tag 626, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocType((bool Valid, long Value) value)
		: Typed<long>(626, value);

	/// <summary>
	/// Represents NoHops, FIX tag 627, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoHops((bool Valid, long Value) value)
		: Typed<long>(627, value);

	/// <summary>
	/// Represents HopCompID, FIX tag 628, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class HopCompID(string value)
		: Typed<string>(628, value);

	/// <summary>
	/// Represents HopSendingTime, FIX tag 629, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HopSendingTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(629, value);

	/// <summary>
	/// Represents HopRefID, FIX tag 630, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class HopRefID((bool Valid, long Value) value)
		: Typed<long>(630, value);

	/// <summary>
	/// Represents MidPx, FIX tag 631, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MidPx((bool Valid, decimal Value) value)
		: Typed<decimal>(631, value);

	/// <summary>
	/// Represents BidYield, FIX tag 632, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidYield((bool Valid, decimal Value) value)
		: Typed<decimal>(632, value);

	/// <summary>
	/// Represents MidYield, FIX tag 633, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MidYield((bool Valid, decimal Value) value)
		: Typed<decimal>(633, value);

	/// <summary>
	/// Represents OfferYield, FIX tag 634, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferYield((bool Valid, decimal Value) value)
		: Typed<decimal>(634, value);

	/// <summary>
	/// Represents ClearingFeeIndicator, FIX tag 635, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ClearingFeeIndicator(string value)
		: Typed<string>(635, value);

	/// <summary>
	/// Represents WorkingIndicator, FIX tag 636, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class WorkingIndicator((bool Valid, bool Value) value)
		: Typed<bool>(636, value);

	/// <summary>
	/// Represents LegLastPx, FIX tag 637, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegLastPx((bool Valid, decimal Value) value)
		: Typed<decimal>(637, value);

	/// <summary>
	/// Represents PriorityIndicator, FIX tag 638, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriorityIndicator((bool Valid, long Value) value)
		: Typed<long>(638, value);

	/// <summary>
	/// Represents PriceImprovement, FIX tag 639, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriceImprovement((bool Valid, decimal Value) value)
		: Typed<decimal>(639, value);

	/// <summary>
	/// Represents Price2, FIX tag 640, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Price2((bool Valid, decimal Value) value)
		: Typed<decimal>(640, value);

	/// <summary>
	/// Represents LastForwardPoints2, FIX tag 641, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastForwardPoints2((bool Valid, decimal Value) value)
		: Typed<decimal>(641, value);

	/// <summary>
	/// Represents BidForwardPoints2, FIX tag 642, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BidForwardPoints2((bool Valid, decimal Value) value)
		: Typed<decimal>(642, value);

	/// <summary>
	/// Represents OfferForwardPoints2, FIX tag 643, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OfferForwardPoints2((bool Valid, decimal Value) value)
		: Typed<decimal>(643, value);

	/// <summary>
	/// Represents RFQReqID, FIX tag 644, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RFQReqID(string value)
		: Typed<string>(644, value);

	/// <summary>
	/// Represents MktBidPx, FIX tag 645, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MktBidPx((bool Valid, decimal Value) value)
		: Typed<decimal>(645, value);

	/// <summary>
	/// Represents MktOfferPx, FIX tag 646, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MktOfferPx((bool Valid, decimal Value) value)
		: Typed<decimal>(646, value);

	/// <summary>
	/// Represents MinBidSize, FIX tag 647, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MinBidSize((bool Valid, decimal Value) value)
		: Typed<decimal>(647, value);

	/// <summary>
	/// Represents MinOfferSize, FIX tag 648, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MinOfferSize((bool Valid, decimal Value) value)
		: Typed<decimal>(648, value);

	/// <summary>
	/// Represents QuoteStatusReqID, FIX tag 649, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteStatusReqID(string value)
		: Typed<string>(649, value);

	/// <summary>
	/// Represents LegalConfirm, FIX tag 650, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegalConfirm((bool Valid, bool Value) value)
		: Typed<bool>(650, value);

	/// <summary>
	/// Represents UnderlyingLastPx, FIX tag 651, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingLastPx((bool Valid, decimal Value) value)
		: Typed<decimal>(651, value);

	/// <summary>
	/// Represents UnderlyingLastQty, FIX tag 652, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingLastQty((bool Valid, decimal Value) value)
		: Typed<decimal>(652, value);

	/// <summary>
	/// Represents LegRefID, FIX tag 654, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegRefID(string value)
		: Typed<string>(654, value);

	/// <summary>
	/// Represents ContraLegRefID, FIX tag 655, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ContraLegRefID(string value)
		: Typed<string>(655, value);

	/// <summary>
	/// Represents SettlCurrBidFxRate, FIX tag 656, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlCurrBidFxRate((bool Valid, decimal Value) value)
		: Typed<decimal>(656, value);

	/// <summary>
	/// Represents SettlCurrOfferFxRate, FIX tag 657, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlCurrOfferFxRate((bool Valid, decimal Value) value)
		: Typed<decimal>(657, value);

	/// <summary>
	/// Represents QuoteRequestRejectReason, FIX tag 658, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteRequestRejectReason((bool Valid, long Value) value)
		: Typed<long>(658, value);

	/// <summary>
	/// Represents SideComplianceID, FIX tag 659, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SideComplianceID(string value)
		: Typed<string>(659, value);

	/// <summary>
	/// Represents AcctIDSource, FIX tag 660, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AcctIDSource((bool Valid, long Value) value)
		: Typed<long>(660, value);

	/// <summary>
	/// Represents AllocAcctIDSource, FIX tag 661, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocAcctIDSource((bool Valid, long Value) value)
		: Typed<long>(661, value);

	/// <summary>
	/// Represents BenchmarkPrice, FIX tag 662, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BenchmarkPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(662, value);

	/// <summary>
	/// Represents BenchmarkPriceType, FIX tag 663, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BenchmarkPriceType((bool Valid, long Value) value)
		: Typed<long>(663, value);

	/// <summary>
	/// Represents ConfirmID, FIX tag 664, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ConfirmID(string value)
		: Typed<string>(664, value);

	/// <summary>
	/// Represents ConfirmStatus, FIX tag 665, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ConfirmStatus((bool Valid, long Value) value)
		: Typed<long>(665, value);

	/// <summary>
	/// Represents ConfirmTransType, FIX tag 666, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ConfirmTransType((bool Valid, long Value) value)
		: Typed<long>(666, value);

	/// <summary>
	/// Represents ContractSettlMonth, FIX tag 667, with wire type <c>MonthYear</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContractSettlMonth((bool Valid, FixMonthYear Value) value)
		: Typed<FixMonthYear>(667, value);

	/// <summary>
	/// Represents DeliveryForm, FIX tag 668, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DeliveryForm((bool Valid, long Value) value)
		: Typed<long>(668, value);

	/// <summary>
	/// Represents LastParPx, FIX tag 669, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastParPx((bool Valid, decimal Value) value)
		: Typed<decimal>(669, value);

	/// <summary>
	/// Represents NoLegAllocs, FIX tag 670, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoLegAllocs((bool Valid, long Value) value)
		: Typed<long>(670, value);

	/// <summary>
	/// Represents LegAllocAccount, FIX tag 671, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegAllocAccount(string value)
		: Typed<string>(671, value);

	/// <summary>
	/// Represents LegIndividualAllocID, FIX tag 672, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegIndividualAllocID(string value)
		: Typed<string>(672, value);

	/// <summary>
	/// Represents LegAllocQty, FIX tag 673, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegAllocQty((bool Valid, decimal Value) value)
		: Typed<decimal>(673, value);

	/// <summary>
	/// Represents LegAllocAcctIDSource, FIX tag 674, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegAllocAcctIDSource(string value)
		: Typed<string>(674, value);

	/// <summary>
	/// Represents LegSettlCurrency, FIX tag 675, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSettlCurrency(string value)
		: Typed<string>(675, value);

	/// <summary>
	/// Represents LegBenchmarkCurveCurrency, FIX tag 676, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegBenchmarkCurveCurrency(string value)
		: Typed<string>(676, value);

	/// <summary>
	/// Represents LegBenchmarkCurveName, FIX tag 677, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegBenchmarkCurveName(string value)
		: Typed<string>(677, value);

	/// <summary>
	/// Represents LegBenchmarkCurvePoint, FIX tag 678, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegBenchmarkCurvePoint(string value)
		: Typed<string>(678, value);

	/// <summary>
	/// Represents LegBenchmarkPrice, FIX tag 679, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegBenchmarkPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(679, value);

	/// <summary>
	/// Represents LegBenchmarkPriceType, FIX tag 680, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegBenchmarkPriceType((bool Valid, long Value) value)
		: Typed<long>(680, value);

	/// <summary>
	/// Represents LegBidPx, FIX tag 681, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegBidPx((bool Valid, decimal Value) value)
		: Typed<decimal>(681, value);

	/// <summary>
	/// Represents LegIOIQty, FIX tag 682, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegIOIQty(string value)
		: Typed<string>(682, value);

	/// <summary>
	/// Represents NoLegStipulations, FIX tag 683, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoLegStipulations((bool Valid, long Value) value)
		: Typed<long>(683, value);

	/// <summary>
	/// Represents LegOfferPx, FIX tag 684, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegOfferPx((bool Valid, decimal Value) value)
		: Typed<decimal>(684, value);

	/// <summary>
	/// Represents LegPriceType, FIX tag 686, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegPriceType((bool Valid, long Value) value)
		: Typed<long>(686, value);

	/// <summary>
	/// Represents LegQty, FIX tag 687, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegQty((bool Valid, decimal Value) value)
		: Typed<decimal>(687, value);

	/// <summary>
	/// Represents LegStipulationType, FIX tag 688, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegStipulationType(string value)
		: Typed<string>(688, value);

	/// <summary>
	/// Represents LegStipulationValue, FIX tag 689, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegStipulationValue(string value)
		: Typed<string>(689, value);

	/// <summary>
	/// Represents LegSwapType, FIX tag 690, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegSwapType((bool Valid, long Value) value)
		: Typed<long>(690, value);

	/// <summary>
	/// Represents Pool, FIX tag 691, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Pool(string value)
		: Typed<string>(691, value);

	/// <summary>
	/// Represents QuotePriceType, FIX tag 692, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuotePriceType((bool Valid, long Value) value)
		: Typed<long>(692, value);

	/// <summary>
	/// Represents QuoteRespID, FIX tag 693, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class QuoteRespID(string value)
		: Typed<string>(693, value);

	/// <summary>
	/// Represents QuoteRespType, FIX tag 694, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteRespType((bool Valid, long Value) value)
		: Typed<long>(694, value);

	/// <summary>
	/// Represents QuoteQualifier, FIX tag 695, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QuoteQualifier((bool Valid, char Value) value)
		: Typed<char>(695, value);

	/// <summary>
	/// Represents YieldRedemptionDate, FIX tag 696, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class YieldRedemptionDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(696, value);

	/// <summary>
	/// Represents YieldRedemptionPrice, FIX tag 697, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class YieldRedemptionPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(697, value);

	/// <summary>
	/// Represents YieldRedemptionPriceType, FIX tag 698, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class YieldRedemptionPriceType((bool Valid, long Value) value)
		: Typed<long>(698, value);

	/// <summary>
	/// Represents BenchmarkSecurityID, FIX tag 699, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BenchmarkSecurityID(string value)
		: Typed<string>(699, value);

	/// <summary>
	/// Represents ReversalIndicator, FIX tag 700, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ReversalIndicator((bool Valid, bool Value) value)
		: Typed<bool>(700, value);

	/// <summary>
	/// Represents YieldCalcDate, FIX tag 701, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class YieldCalcDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(701, value);

	/// <summary>
	/// Represents NoPositions, FIX tag 702, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoPositions((bool Valid, long Value) value)
		: Typed<long>(702, value);

	/// <summary>
	/// Represents PosType, FIX tag 703, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PosType(string value)
		: Typed<string>(703, value);

	/// <summary>
	/// Represents LongQty, FIX tag 704, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LongQty((bool Valid, decimal Value) value)
		: Typed<decimal>(704, value);

	/// <summary>
	/// Represents ShortQty, FIX tag 705, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ShortQty((bool Valid, decimal Value) value)
		: Typed<decimal>(705, value);

	/// <summary>
	/// Represents PosQtyStatus, FIX tag 706, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosQtyStatus((bool Valid, long Value) value)
		: Typed<long>(706, value);

	/// <summary>
	/// Represents PosAmtType, FIX tag 707, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PosAmtType(string value)
		: Typed<string>(707, value);

	/// <summary>
	/// Represents PosAmt, FIX tag 708, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(708, value);

	/// <summary>
	/// Represents PosTransType, FIX tag 709, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosTransType((bool Valid, long Value) value)
		: Typed<long>(709, value);

	/// <summary>
	/// Represents PosReqID, FIX tag 710, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PosReqID(string value)
		: Typed<string>(710, value);

	/// <summary>
	/// Represents NoUnderlyings, FIX tag 711, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoUnderlyings((bool Valid, long Value) value)
		: Typed<long>(711, value);

	/// <summary>
	/// Represents PosMaintAction, FIX tag 712, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosMaintAction((bool Valid, long Value) value)
		: Typed<long>(712, value);

	/// <summary>
	/// Represents OrigPosReqRefID, FIX tag 713, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrigPosReqRefID(string value)
		: Typed<string>(713, value);

	/// <summary>
	/// Represents PosMaintRptRefID, FIX tag 714, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PosMaintRptRefID(string value)
		: Typed<string>(714, value);

	/// <summary>
	/// Represents ClearingBusinessDate, FIX tag 715, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ClearingBusinessDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(715, value);

	/// <summary>
	/// Represents SettlSessID, FIX tag 716, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlSessID(string value)
		: Typed<string>(716, value);

	/// <summary>
	/// Represents SettlSessSubID, FIX tag 717, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlSessSubID(string value)
		: Typed<string>(717, value);

	/// <summary>
	/// Represents AdjustmentType, FIX tag 718, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AdjustmentType((bool Valid, long Value) value)
		: Typed<long>(718, value);

	/// <summary>
	/// Represents ContraryInstructionIndicator, FIX tag 719, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ContraryInstructionIndicator((bool Valid, bool Value) value)
		: Typed<bool>(719, value);

	/// <summary>
	/// Represents PriorSpreadIndicator, FIX tag 720, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriorSpreadIndicator((bool Valid, bool Value) value)
		: Typed<bool>(720, value);

	/// <summary>
	/// Represents PosMaintRptID, FIX tag 721, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class PosMaintRptID(string value)
		: Typed<string>(721, value);

	/// <summary>
	/// Represents PosMaintStatus, FIX tag 722, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosMaintStatus((bool Valid, long Value) value)
		: Typed<long>(722, value);

	/// <summary>
	/// Represents PosMaintResult, FIX tag 723, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosMaintResult((bool Valid, long Value) value)
		: Typed<long>(723, value);

	/// <summary>
	/// Represents PosReqType, FIX tag 724, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosReqType((bool Valid, long Value) value)
		: Typed<long>(724, value);

	/// <summary>
	/// Represents ResponseTransportType, FIX tag 725, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ResponseTransportType((bool Valid, long Value) value)
		: Typed<long>(725, value);

	/// <summary>
	/// Represents ResponseDestination, FIX tag 726, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ResponseDestination(string value)
		: Typed<string>(726, value);

	/// <summary>
	/// Represents TotalNumPosReports, FIX tag 727, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalNumPosReports((bool Valid, long Value) value)
		: Typed<long>(727, value);

	/// <summary>
	/// Represents PosReqResult, FIX tag 728, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosReqResult((bool Valid, long Value) value)
		: Typed<long>(728, value);

	/// <summary>
	/// Represents PosReqStatus, FIX tag 729, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PosReqStatus((bool Valid, long Value) value)
		: Typed<long>(729, value);

	/// <summary>
	/// Represents SettlPrice, FIX tag 730, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(730, value);

	/// <summary>
	/// Represents SettlPriceType, FIX tag 731, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlPriceType((bool Valid, long Value) value)
		: Typed<long>(731, value);

	/// <summary>
	/// Represents UnderlyingSettlPrice, FIX tag 732, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingSettlPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(732, value);

	/// <summary>
	/// Represents UnderlyingSettlPriceType, FIX tag 733, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingSettlPriceType((bool Valid, long Value) value)
		: Typed<long>(733, value);

	/// <summary>
	/// Represents PriorSettlPrice, FIX tag 734, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriorSettlPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(734, value);

	/// <summary>
	/// Represents NoQuoteQualifiers, FIX tag 735, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoQuoteQualifiers((bool Valid, long Value) value)
		: Typed<long>(735, value);

	/// <summary>
	/// Represents AllocSettlCurrency, FIX tag 736, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocSettlCurrency(string value)
		: Typed<string>(736, value);

	/// <summary>
	/// Represents AllocSettlCurrAmt, FIX tag 737, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocSettlCurrAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(737, value);

	/// <summary>
	/// Represents InterestAtMaturity, FIX tag 738, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class InterestAtMaturity((bool Valid, decimal Value) value)
		: Typed<decimal>(738, value);

	/// <summary>
	/// Represents LegDatedDate, FIX tag 739, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegDatedDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(739, value);

	/// <summary>
	/// Represents LegPool, FIX tag 740, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegPool(string value)
		: Typed<string>(740, value);

	/// <summary>
	/// Represents AllocInterestAtMaturity, FIX tag 741, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocInterestAtMaturity((bool Valid, decimal Value) value)
		: Typed<decimal>(741, value);

	/// <summary>
	/// Represents AllocAccruedInterestAmt, FIX tag 742, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocAccruedInterestAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(742, value);

	/// <summary>
	/// Represents DeliveryDate, FIX tag 743, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DeliveryDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(743, value);

	/// <summary>
	/// Represents AssignmentMethod, FIX tag 744, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AssignmentMethod((bool Valid, char Value) value)
		: Typed<char>(744, value);

	/// <summary>
	/// Represents AssignmentUnit, FIX tag 745, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AssignmentUnit((bool Valid, decimal Value) value)
		: Typed<decimal>(745, value);

	/// <summary>
	/// Represents OpenInterest, FIX tag 746, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OpenInterest((bool Valid, decimal Value) value)
		: Typed<decimal>(746, value);

	/// <summary>
	/// Represents ExerciseMethod, FIX tag 747, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExerciseMethod((bool Valid, char Value) value)
		: Typed<char>(747, value);

	/// <summary>
	/// Represents TotNumTradeReports, FIX tag 748, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNumTradeReports((bool Valid, long Value) value)
		: Typed<long>(748, value);

	/// <summary>
	/// Represents TradeRequestResult, FIX tag 749, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeRequestResult((bool Valid, long Value) value)
		: Typed<long>(749, value);

	/// <summary>
	/// Represents TradeRequestStatus, FIX tag 750, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeRequestStatus((bool Valid, long Value) value)
		: Typed<long>(750, value);

	/// <summary>
	/// Represents TradeReportRejectReason, FIX tag 751, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeReportRejectReason((bool Valid, long Value) value)
		: Typed<long>(751, value);

	/// <summary>
	/// Represents SideMultiLegReportingType, FIX tag 752, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SideMultiLegReportingType((bool Valid, long Value) value)
		: Typed<long>(752, value);

	/// <summary>
	/// Represents NoPosAmt, FIX tag 753, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoPosAmt((bool Valid, long Value) value)
		: Typed<long>(753, value);

	/// <summary>
	/// Represents AutoAcceptIndicator, FIX tag 754, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AutoAcceptIndicator((bool Valid, bool Value) value)
		: Typed<bool>(754, value);

	/// <summary>
	/// Represents AllocReportID, FIX tag 755, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocReportID(string value)
		: Typed<string>(755, value);

	/// <summary>
	/// Represents NoNested2PartyIDs, FIX tag 756, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNested2PartyIDs((bool Valid, long Value) value)
		: Typed<long>(756, value);

	/// <summary>
	/// Represents Nested2PartyID, FIX tag 757, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Nested2PartyID(string value)
		: Typed<string>(757, value);

	/// <summary>
	/// Represents Nested2PartyIDSource, FIX tag 758, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested2PartyIDSource((bool Valid, char Value) value)
		: Typed<char>(758, value);

	/// <summary>
	/// Represents Nested2PartyRole, FIX tag 759, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested2PartyRole((bool Valid, long Value) value)
		: Typed<long>(759, value);

	/// <summary>
	/// Represents Nested2PartySubID, FIX tag 760, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Nested2PartySubID(string value)
		: Typed<string>(760, value);

	/// <summary>
	/// Represents BenchmarkSecurityIDSource, FIX tag 761, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class BenchmarkSecurityIDSource(string value)
		: Typed<string>(761, value);

	/// <summary>
	/// Represents SecuritySubType, FIX tag 762, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecuritySubType(string value)
		: Typed<string>(762, value);

	/// <summary>
	/// Represents UnderlyingSecuritySubType, FIX tag 763, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingSecuritySubType(string value)
		: Typed<string>(763, value);

	/// <summary>
	/// Represents LegSecuritySubType, FIX tag 764, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegSecuritySubType(string value)
		: Typed<string>(764, value);

	/// <summary>
	/// Represents AllowableOneSidednessPct, FIX tag 765, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllowableOneSidednessPct((bool Valid, decimal Value) value)
		: Typed<decimal>(765, value);

	/// <summary>
	/// Represents AllowableOneSidednessValue, FIX tag 766, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllowableOneSidednessValue((bool Valid, decimal Value) value)
		: Typed<decimal>(766, value);

	/// <summary>
	/// Represents AllowableOneSidednessCurr, FIX tag 767, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllowableOneSidednessCurr(string value)
		: Typed<string>(767, value);

	/// <summary>
	/// Represents NoTrdRegTimestamps, FIX tag 768, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoTrdRegTimestamps((bool Valid, long Value) value)
		: Typed<long>(768, value);

	/// <summary>
	/// Represents TrdRegTimestamp, FIX tag 769, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TrdRegTimestamp((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(769, value);

	/// <summary>
	/// Represents TrdRegTimestampType, FIX tag 770, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TrdRegTimestampType((bool Valid, long Value) value)
		: Typed<long>(770, value);

	/// <summary>
	/// Represents TrdRegTimestampOrigin, FIX tag 771, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TrdRegTimestampOrigin(string value)
		: Typed<string>(771, value);

	/// <summary>
	/// Represents ConfirmRefID, FIX tag 772, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ConfirmRefID(string value)
		: Typed<string>(772, value);

	/// <summary>
	/// Represents ConfirmType, FIX tag 773, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ConfirmType((bool Valid, long Value) value)
		: Typed<long>(773, value);

	/// <summary>
	/// Represents ConfirmRejReason, FIX tag 774, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ConfirmRejReason((bool Valid, long Value) value)
		: Typed<long>(774, value);

	/// <summary>
	/// Represents BookingType, FIX tag 775, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class BookingType((bool Valid, long Value) value)
		: Typed<long>(775, value);

	/// <summary>
	/// Represents IndividualAllocRejCode, FIX tag 776, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class IndividualAllocRejCode((bool Valid, long Value) value)
		: Typed<long>(776, value);

	/// <summary>
	/// Represents SettlInstMsgID, FIX tag 777, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlInstMsgID(string value)
		: Typed<string>(777, value);

	/// <summary>
	/// Represents NoSettlInst, FIX tag 778, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSettlInst((bool Valid, long Value) value)
		: Typed<long>(778, value);

	/// <summary>
	/// Represents LastUpdateTime, FIX tag 779, with wire type <c>UTCTimestamp</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastUpdateTime((bool Valid, FixTimestamp Value) value)
		: Typed<FixTimestamp>(779, value);

	/// <summary>
	/// Represents AllocSettlInstType, FIX tag 780, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocSettlInstType((bool Valid, long Value) value)
		: Typed<long>(780, value);

	/// <summary>
	/// Represents NoSettlPartyIDs, FIX tag 781, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSettlPartyIDs((bool Valid, long Value) value)
		: Typed<long>(781, value);

	/// <summary>
	/// Represents SettlPartyID, FIX tag 782, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlPartyID(string value)
		: Typed<string>(782, value);

	/// <summary>
	/// Represents SettlPartyIDSource, FIX tag 783, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlPartyIDSource((bool Valid, char Value) value)
		: Typed<char>(783, value);

	/// <summary>
	/// Represents SettlPartyRole, FIX tag 784, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlPartyRole((bool Valid, long Value) value)
		: Typed<long>(784, value);

	/// <summary>
	/// Represents SettlPartySubID, FIX tag 785, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlPartySubID(string value)
		: Typed<string>(785, value);

	/// <summary>
	/// Represents SettlPartySubIDType, FIX tag 786, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlPartySubIDType((bool Valid, long Value) value)
		: Typed<long>(786, value);

	/// <summary>
	/// Represents DlvyInstType, FIX tag 787, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DlvyInstType((bool Valid, char Value) value)
		: Typed<char>(787, value);

	/// <summary>
	/// Represents TerminationType, FIX tag 788, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TerminationType((bool Valid, long Value) value)
		: Typed<long>(788, value);

	/// <summary>
	/// Represents NextExpectedMsgSeqNum, FIX tag 789, with wire type <c>SeqNum</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NextExpectedMsgSeqNum((bool Valid, long Value) value)
		: Typed<long>(789, value);

	/// <summary>
	/// Represents OrdStatusReqID, FIX tag 790, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrdStatusReqID(string value)
		: Typed<string>(790, value);

	/// <summary>
	/// Represents SettlInstReqID, FIX tag 791, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SettlInstReqID(string value)
		: Typed<string>(791, value);

	/// <summary>
	/// Represents SettlInstReqRejCode, FIX tag 792, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SettlInstReqRejCode((bool Valid, long Value) value)
		: Typed<long>(792, value);

	/// <summary>
	/// Represents SecondaryAllocID, FIX tag 793, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryAllocID(string value)
		: Typed<string>(793, value);

	/// <summary>
	/// Represents AllocReportType, FIX tag 794, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocReportType((bool Valid, long Value) value)
		: Typed<long>(794, value);

	/// <summary>
	/// Represents AllocReportRefID, FIX tag 795, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AllocReportRefID(string value)
		: Typed<string>(795, value);

	/// <summary>
	/// Represents AllocCancReplaceReason, FIX tag 796, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocCancReplaceReason((bool Valid, long Value) value)
		: Typed<long>(796, value);

	/// <summary>
	/// Represents CopyMsgIndicator, FIX tag 797, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CopyMsgIndicator((bool Valid, bool Value) value)
		: Typed<bool>(797, value);

	/// <summary>
	/// Represents AllocAccountType, FIX tag 798, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocAccountType((bool Valid, long Value) value)
		: Typed<long>(798, value);

	/// <summary>
	/// Represents OrderAvgPx, FIX tag 799, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderAvgPx((bool Valid, decimal Value) value)
		: Typed<decimal>(799, value);

	/// <summary>
	/// Represents OrderBookingQty, FIX tag 800, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderBookingQty((bool Valid, decimal Value) value)
		: Typed<decimal>(800, value);

	/// <summary>
	/// Represents NoSettlPartySubIDs, FIX tag 801, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoSettlPartySubIDs((bool Valid, long Value) value)
		: Typed<long>(801, value);

	/// <summary>
	/// Represents NoPartySubIDs, FIX tag 802, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoPartySubIDs((bool Valid, long Value) value)
		: Typed<long>(802, value);

	/// <summary>
	/// Represents PartySubIDType, FIX tag 803, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PartySubIDType((bool Valid, long Value) value)
		: Typed<long>(803, value);

	/// <summary>
	/// Represents NoNestedPartySubIDs, FIX tag 804, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNestedPartySubIDs((bool Valid, long Value) value)
		: Typed<long>(804, value);

	/// <summary>
	/// Represents NestedPartySubIDType, FIX tag 805, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NestedPartySubIDType((bool Valid, long Value) value)
		: Typed<long>(805, value);

	/// <summary>
	/// Represents NoNested2PartySubIDs, FIX tag 806, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNested2PartySubIDs((bool Valid, long Value) value)
		: Typed<long>(806, value);

	/// <summary>
	/// Represents Nested2PartySubIDType, FIX tag 807, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested2PartySubIDType((bool Valid, long Value) value)
		: Typed<long>(807, value);

	/// <summary>
	/// Represents AllocIntermedReqType, FIX tag 808, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocIntermedReqType((bool Valid, long Value) value)
		: Typed<long>(808, value);

	/// <summary>
	/// Represents UnderlyingPx, FIX tag 810, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingPx((bool Valid, decimal Value) value)
		: Typed<decimal>(810, value);

	/// <summary>
	/// Represents PriceDelta, FIX tag 811, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PriceDelta((bool Valid, decimal Value) value)
		: Typed<decimal>(811, value);

	/// <summary>
	/// Represents ApplQueueMax, FIX tag 812, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ApplQueueMax((bool Valid, long Value) value)
		: Typed<long>(812, value);

	/// <summary>
	/// Represents ApplQueueDepth, FIX tag 813, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ApplQueueDepth((bool Valid, long Value) value)
		: Typed<long>(813, value);

	/// <summary>
	/// Represents ApplQueueResolution, FIX tag 814, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ApplQueueResolution((bool Valid, long Value) value)
		: Typed<long>(814, value);

	/// <summary>
	/// Represents ApplQueueAction, FIX tag 815, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ApplQueueAction((bool Valid, long Value) value)
		: Typed<long>(815, value);

	/// <summary>
	/// Represents NoAltMDSource, FIX tag 816, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoAltMDSource((bool Valid, long Value) value)
		: Typed<long>(816, value);

	/// <summary>
	/// Represents AltMDSourceID, FIX tag 817, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AltMDSourceID(string value)
		: Typed<string>(817, value);

	/// <summary>
	/// Represents SecondaryTradeReportID, FIX tag 818, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryTradeReportID(string value)
		: Typed<string>(818, value);

	/// <summary>
	/// Represents AvgPxIndicator, FIX tag 819, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AvgPxIndicator((bool Valid, long Value) value)
		: Typed<long>(819, value);

	/// <summary>
	/// Represents TradeLinkID, FIX tag 820, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeLinkID(string value)
		: Typed<string>(820, value);

	/// <summary>
	/// Represents OrderInputDevice, FIX tag 821, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class OrderInputDevice(string value)
		: Typed<string>(821, value);

	/// <summary>
	/// Represents UnderlyingTradingSessionID, FIX tag 822, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingTradingSessionID(string value)
		: Typed<string>(822, value);

	/// <summary>
	/// Represents UnderlyingTradingSessionSubID, FIX tag 823, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingTradingSessionSubID(string value)
		: Typed<string>(823, value);

	/// <summary>
	/// Represents TradeLegRefID, FIX tag 824, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TradeLegRefID(string value)
		: Typed<string>(824, value);

	/// <summary>
	/// Represents ExchangeRule, FIX tag 825, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ExchangeRule(string value)
		: Typed<string>(825, value);

	/// <summary>
	/// Represents TradeAllocIndicator, FIX tag 826, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeAllocIndicator((bool Valid, long Value) value)
		: Typed<long>(826, value);

	/// <summary>
	/// Represents ExpirationCycle, FIX tag 827, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ExpirationCycle((bool Valid, long Value) value)
		: Typed<long>(827, value);

	/// <summary>
	/// Represents TrdType, FIX tag 828, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TrdType((bool Valid, long Value) value)
		: Typed<long>(828, value);

	/// <summary>
	/// Represents TrdSubType, FIX tag 829, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TrdSubType((bool Valid, long Value) value)
		: Typed<long>(829, value);

	/// <summary>
	/// Represents TransferReason, FIX tag 830, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TransferReason(string value)
		: Typed<string>(830, value);

	/// <summary>
	/// Represents TotNumAssignmentReports, FIX tag 832, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNumAssignmentReports((bool Valid, long Value) value)
		: Typed<long>(832, value);

	/// <summary>
	/// Represents AsgnRptID, FIX tag 833, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AsgnRptID(string value)
		: Typed<string>(833, value);

	/// <summary>
	/// Represents ThresholdAmount, FIX tag 834, with wire type <c>PriceOffset</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ThresholdAmount((bool Valid, decimal Value) value)
		: Typed<decimal>(834, value);

	/// <summary>
	/// Represents PegMoveType, FIX tag 835, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegMoveType((bool Valid, long Value) value)
		: Typed<long>(835, value);

	/// <summary>
	/// Represents PegOffsetType, FIX tag 836, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegOffsetType((bool Valid, long Value) value)
		: Typed<long>(836, value);

	/// <summary>
	/// Represents PegLimitType, FIX tag 837, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegLimitType((bool Valid, long Value) value)
		: Typed<long>(837, value);

	/// <summary>
	/// Represents PegRoundDirection, FIX tag 838, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegRoundDirection((bool Valid, long Value) value)
		: Typed<long>(838, value);

	/// <summary>
	/// Represents PeggedPrice, FIX tag 839, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PeggedPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(839, value);

	/// <summary>
	/// Represents PegScope, FIX tag 840, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PegScope((bool Valid, long Value) value)
		: Typed<long>(840, value);

	/// <summary>
	/// Represents DiscretionMoveType, FIX tag 841, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionMoveType((bool Valid, long Value) value)
		: Typed<long>(841, value);

	/// <summary>
	/// Represents DiscretionOffsetType, FIX tag 842, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionOffsetType((bool Valid, long Value) value)
		: Typed<long>(842, value);

	/// <summary>
	/// Represents DiscretionLimitType, FIX tag 843, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionLimitType((bool Valid, long Value) value)
		: Typed<long>(843, value);

	/// <summary>
	/// Represents DiscretionRoundDirection, FIX tag 844, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionRoundDirection((bool Valid, long Value) value)
		: Typed<long>(844, value);

	/// <summary>
	/// Represents DiscretionPrice, FIX tag 845, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(845, value);

	/// <summary>
	/// Represents DiscretionScope, FIX tag 846, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DiscretionScope((bool Valid, long Value) value)
		: Typed<long>(846, value);

	/// <summary>
	/// Represents TargetStrategy, FIX tag 847, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TargetStrategy((bool Valid, long Value) value)
		: Typed<long>(847, value);

	/// <summary>
	/// Represents TargetStrategyParameters, FIX tag 848, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TargetStrategyParameters(string value)
		: Typed<string>(848, value);

	/// <summary>
	/// Represents ParticipationRate, FIX tag 849, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ParticipationRate((bool Valid, decimal Value) value)
		: Typed<decimal>(849, value);

	/// <summary>
	/// Represents TargetStrategyPerformance, FIX tag 850, with wire type <c>float</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TargetStrategyPerformance((bool Valid, decimal Value) value)
		: Typed<decimal>(850, value);

	/// <summary>
	/// Represents LastLiquidityInd, FIX tag 851, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastLiquidityInd((bool Valid, long Value) value)
		: Typed<long>(851, value);

	/// <summary>
	/// Represents PublishTrdIndicator, FIX tag 852, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PublishTrdIndicator((bool Valid, bool Value) value)
		: Typed<bool>(852, value);

	/// <summary>
	/// Represents ShortSaleReason, FIX tag 853, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ShortSaleReason((bool Valid, long Value) value)
		: Typed<long>(853, value);

	/// <summary>
	/// Represents QtyType, FIX tag 854, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class QtyType((bool Valid, long Value) value)
		: Typed<long>(854, value);

	/// <summary>
	/// Represents SecondaryTrdType, FIX tag 855, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SecondaryTrdType((bool Valid, long Value) value)
		: Typed<long>(855, value);

	/// <summary>
	/// Represents TradeReportType, FIX tag 856, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TradeReportType((bool Valid, long Value) value)
		: Typed<long>(856, value);

	/// <summary>
	/// Represents AllocNoOrdersType, FIX tag 857, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AllocNoOrdersType((bool Valid, long Value) value)
		: Typed<long>(857, value);

	/// <summary>
	/// Represents SharedCommission, FIX tag 858, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class SharedCommission((bool Valid, decimal Value) value)
		: Typed<decimal>(858, value);

	/// <summary>
	/// Represents ConfirmReqID, FIX tag 859, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class ConfirmReqID(string value)
		: Typed<string>(859, value);

	/// <summary>
	/// Represents AvgParPx, FIX tag 860, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AvgParPx((bool Valid, decimal Value) value)
		: Typed<decimal>(860, value);

	/// <summary>
	/// Represents ReportedPx, FIX tag 861, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class ReportedPx((bool Valid, decimal Value) value)
		: Typed<decimal>(861, value);

	/// <summary>
	/// Represents NoCapacities, FIX tag 862, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoCapacities((bool Valid, long Value) value)
		: Typed<long>(862, value);

	/// <summary>
	/// Represents OrderCapacityQty, FIX tag 863, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class OrderCapacityQty((bool Valid, decimal Value) value)
		: Typed<decimal>(863, value);

	/// <summary>
	/// Represents NoEvents, FIX tag 864, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoEvents((bool Valid, long Value) value)
		: Typed<long>(864, value);

	/// <summary>
	/// Represents EventType, FIX tag 865, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EventType((bool Valid, long Value) value)
		: Typed<long>(865, value);

	/// <summary>
	/// Represents EventDate, FIX tag 866, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EventDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(866, value);

	/// <summary>
	/// Represents EventPx, FIX tag 867, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EventPx((bool Valid, decimal Value) value)
		: Typed<decimal>(867, value);

	/// <summary>
	/// Represents EventText, FIX tag 868, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class EventText(string value)
		: Typed<string>(868, value);

	/// <summary>
	/// Represents PctAtRisk, FIX tag 869, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class PctAtRisk((bool Valid, decimal Value) value)
		: Typed<decimal>(869, value);

	/// <summary>
	/// Represents NoInstrAttrib, FIX tag 870, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoInstrAttrib((bool Valid, long Value) value)
		: Typed<long>(870, value);

	/// <summary>
	/// Represents InstrAttribType, FIX tag 871, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class InstrAttribType((bool Valid, long Value) value)
		: Typed<long>(871, value);

	/// <summary>
	/// Represents InstrAttribValue, FIX tag 872, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class InstrAttribValue(string value)
		: Typed<string>(872, value);

	/// <summary>
	/// Represents DatedDate, FIX tag 873, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DatedDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(873, value);

	/// <summary>
	/// Represents InterestAccrualDate, FIX tag 874, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class InterestAccrualDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(874, value);

	/// <summary>
	/// Represents CPProgram, FIX tag 875, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CPProgram((bool Valid, long Value) value)
		: Typed<long>(875, value);

	/// <summary>
	/// Represents CPRegType, FIX tag 876, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CPRegType(string value)
		: Typed<string>(876, value);

	/// <summary>
	/// Represents UnderlyingCPProgram, FIX tag 877, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCPProgram(string value)
		: Typed<string>(877, value);

	/// <summary>
	/// Represents UnderlyingCPRegType, FIX tag 878, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingCPRegType(string value)
		: Typed<string>(878, value);

	/// <summary>
	/// Represents UnderlyingQty, FIX tag 879, with wire type <c>Qty</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingQty((bool Valid, decimal Value) value)
		: Typed<decimal>(879, value);

	/// <summary>
	/// Represents TrdMatchID, FIX tag 880, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TrdMatchID(string value)
		: Typed<string>(880, value);

	/// <summary>
	/// Represents SecondaryTradeReportRefID, FIX tag 881, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class SecondaryTradeReportRefID(string value)
		: Typed<string>(881, value);

	/// <summary>
	/// Represents UnderlyingDirtyPrice, FIX tag 882, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingDirtyPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(882, value);

	/// <summary>
	/// Represents UnderlyingEndPrice, FIX tag 883, with wire type <c>Price</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingEndPrice((bool Valid, decimal Value) value)
		: Typed<decimal>(883, value);

	/// <summary>
	/// Represents UnderlyingStartValue, FIX tag 884, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingStartValue((bool Valid, decimal Value) value)
		: Typed<decimal>(884, value);

	/// <summary>
	/// Represents UnderlyingCurrentValue, FIX tag 885, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingCurrentValue((bool Valid, decimal Value) value)
		: Typed<decimal>(885, value);

	/// <summary>
	/// Represents UnderlyingEndValue, FIX tag 886, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UnderlyingEndValue((bool Valid, decimal Value) value)
		: Typed<decimal>(886, value);

	/// <summary>
	/// Represents NoUnderlyingStips, FIX tag 887, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoUnderlyingStips((bool Valid, long Value) value)
		: Typed<long>(887, value);

	/// <summary>
	/// Represents UnderlyingStipType, FIX tag 888, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingStipType(string value)
		: Typed<string>(888, value);

	/// <summary>
	/// Represents UnderlyingStipValue, FIX tag 889, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingStipValue(string value)
		: Typed<string>(889, value);

	/// <summary>
	/// Represents MaturityNetMoney, FIX tag 890, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MaturityNetMoney((bool Valid, decimal Value) value)
		: Typed<decimal>(890, value);

	/// <summary>
	/// Represents MiscFeeBasis, FIX tag 891, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MiscFeeBasis((bool Valid, long Value) value)
		: Typed<long>(891, value);

	/// <summary>
	/// Represents TotNoAllocs, FIX tag 892, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNoAllocs((bool Valid, long Value) value)
		: Typed<long>(892, value);

	/// <summary>
	/// Represents LastFragment, FIX tag 893, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastFragment((bool Valid, bool Value) value)
		: Typed<bool>(893, value);

	/// <summary>
	/// Represents CollReqID, FIX tag 894, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollReqID(string value)
		: Typed<string>(894, value);

	/// <summary>
	/// Represents CollAsgnReason, FIX tag 895, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollAsgnReason((bool Valid, long Value) value)
		: Typed<long>(895, value);

	/// <summary>
	/// Represents CollInquiryQualifier, FIX tag 896, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollInquiryQualifier((bool Valid, long Value) value)
		: Typed<long>(896, value);

	/// <summary>
	/// Represents NoTrades, FIX tag 897, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoTrades((bool Valid, long Value) value)
		: Typed<long>(897, value);

	/// <summary>
	/// Represents MarginRatio, FIX tag 898, with wire type <c>Percentage</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MarginRatio((bool Valid, decimal Value) value)
		: Typed<decimal>(898, value);

	/// <summary>
	/// Represents MarginExcess, FIX tag 899, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class MarginExcess((bool Valid, decimal Value) value)
		: Typed<decimal>(899, value);

	/// <summary>
	/// Represents TotalNetValue, FIX tag 900, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotalNetValue((bool Valid, decimal Value) value)
		: Typed<decimal>(900, value);

	/// <summary>
	/// Represents CashOutstanding, FIX tag 901, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CashOutstanding((bool Valid, decimal Value) value)
		: Typed<decimal>(901, value);

	/// <summary>
	/// Represents CollAsgnID, FIX tag 902, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollAsgnID(string value)
		: Typed<string>(902, value);

	/// <summary>
	/// Represents CollAsgnTransType, FIX tag 903, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollAsgnTransType((bool Valid, long Value) value)
		: Typed<long>(903, value);

	/// <summary>
	/// Represents CollRespID, FIX tag 904, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollRespID(string value)
		: Typed<string>(904, value);

	/// <summary>
	/// Represents CollAsgnRespType, FIX tag 905, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollAsgnRespType((bool Valid, long Value) value)
		: Typed<long>(905, value);

	/// <summary>
	/// Represents CollAsgnRejectReason, FIX tag 906, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollAsgnRejectReason((bool Valid, long Value) value)
		: Typed<long>(906, value);

	/// <summary>
	/// Represents CollAsgnRefID, FIX tag 907, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollAsgnRefID(string value)
		: Typed<string>(907, value);

	/// <summary>
	/// Represents CollRptID, FIX tag 908, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollRptID(string value)
		: Typed<string>(908, value);

	/// <summary>
	/// Represents CollInquiryID, FIX tag 909, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class CollInquiryID(string value)
		: Typed<string>(909, value);

	/// <summary>
	/// Represents CollStatus, FIX tag 910, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollStatus((bool Valid, long Value) value)
		: Typed<long>(910, value);

	/// <summary>
	/// Represents TotNumReports, FIX tag 911, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TotNumReports((bool Valid, long Value) value)
		: Typed<long>(911, value);

	/// <summary>
	/// Represents LastRptRequested, FIX tag 912, with wire type <c>Boolean</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LastRptRequested((bool Valid, bool Value) value)
		: Typed<bool>(912, value);

	/// <summary>
	/// Represents AgreementDesc, FIX tag 913, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AgreementDesc(string value)
		: Typed<string>(913, value);

	/// <summary>
	/// Represents AgreementID, FIX tag 914, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AgreementID(string value)
		: Typed<string>(914, value);

	/// <summary>
	/// Represents AgreementDate, FIX tag 915, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AgreementDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(915, value);

	/// <summary>
	/// Represents StartDate, FIX tag 916, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StartDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(916, value);

	/// <summary>
	/// Represents EndDate, FIX tag 917, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EndDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(917, value);

	/// <summary>
	/// Represents AgreementCurrency, FIX tag 918, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class AgreementCurrency(string value)
		: Typed<string>(918, value);

	/// <summary>
	/// Represents DeliveryType, FIX tag 919, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class DeliveryType((bool Valid, long Value) value)
		: Typed<long>(919, value);

	/// <summary>
	/// Represents EndAccruedInterestAmt, FIX tag 920, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EndAccruedInterestAmt((bool Valid, decimal Value) value)
		: Typed<decimal>(920, value);

	/// <summary>
	/// Represents StartCash, FIX tag 921, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StartCash((bool Valid, decimal Value) value)
		: Typed<decimal>(921, value);

	/// <summary>
	/// Represents EndCash, FIX tag 922, with wire type <c>Amt</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class EndCash((bool Valid, decimal Value) value)
		: Typed<decimal>(922, value);

	/// <summary>
	/// Represents UserRequestID, FIX tag 923, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UserRequestID(string value)
		: Typed<string>(923, value);

	/// <summary>
	/// Represents UserRequestType, FIX tag 924, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UserRequestType((bool Valid, long Value) value)
		: Typed<long>(924, value);

	/// <summary>
	/// Represents NewPassword, FIX tag 925, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class NewPassword(string value)
		: Typed<string>(925, value);

	/// <summary>
	/// Represents UserStatus, FIX tag 926, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class UserStatus((bool Valid, long Value) value)
		: Typed<long>(926, value);

	/// <summary>
	/// Represents UserStatusText, FIX tag 927, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UserStatusText(string value)
		: Typed<string>(927, value);

	/// <summary>
	/// Represents StatusValue, FIX tag 928, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class StatusValue((bool Valid, long Value) value)
		: Typed<long>(928, value);

	/// <summary>
	/// Represents StatusText, FIX tag 929, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StatusText(string value)
		: Typed<string>(929, value);

	/// <summary>
	/// Represents RefCompID, FIX tag 930, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RefCompID(string value)
		: Typed<string>(930, value);

	/// <summary>
	/// Represents RefSubID, FIX tag 931, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class RefSubID(string value)
		: Typed<string>(931, value);

	/// <summary>
	/// Represents NetworkResponseID, FIX tag 932, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class NetworkResponseID(string value)
		: Typed<string>(932, value);

	/// <summary>
	/// Represents NetworkRequestID, FIX tag 933, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class NetworkRequestID(string value)
		: Typed<string>(933, value);

	/// <summary>
	/// Represents LastNetworkResponseID, FIX tag 934, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LastNetworkResponseID(string value)
		: Typed<string>(934, value);

	/// <summary>
	/// Represents NetworkRequestType, FIX tag 935, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NetworkRequestType((bool Valid, long Value) value)
		: Typed<long>(935, value);

	/// <summary>
	/// Represents NoCompIDs, FIX tag 936, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoCompIDs((bool Valid, long Value) value)
		: Typed<long>(936, value);

	/// <summary>
	/// Represents NetworkStatusResponseType, FIX tag 937, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NetworkStatusResponseType((bool Valid, long Value) value)
		: Typed<long>(937, value);

	/// <summary>
	/// Represents NoCollInquiryQualifier, FIX tag 938, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoCollInquiryQualifier((bool Valid, long Value) value)
		: Typed<long>(938, value);

	/// <summary>
	/// Represents TrdRptStatus, FIX tag 939, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class TrdRptStatus((bool Valid, long Value) value)
		: Typed<long>(939, value);

	/// <summary>
	/// Represents AffirmStatus, FIX tag 940, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class AffirmStatus((bool Valid, long Value) value)
		: Typed<long>(940, value);

	/// <summary>
	/// Represents UnderlyingStrikeCurrency, FIX tag 941, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class UnderlyingStrikeCurrency(string value)
		: Typed<string>(941, value);

	/// <summary>
	/// Represents LegStrikeCurrency, FIX tag 942, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class LegStrikeCurrency(string value)
		: Typed<string>(942, value);

	/// <summary>
	/// Represents TimeBracket, FIX tag 943, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class TimeBracket(string value)
		: Typed<string>(943, value);

	/// <summary>
	/// Represents CollAction, FIX tag 944, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollAction((bool Valid, long Value) value)
		: Typed<long>(944, value);

	/// <summary>
	/// Represents CollInquiryStatus, FIX tag 945, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollInquiryStatus((bool Valid, long Value) value)
		: Typed<long>(945, value);

	/// <summary>
	/// Represents CollInquiryResult, FIX tag 946, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class CollInquiryResult((bool Valid, long Value) value)
		: Typed<long>(946, value);

	/// <summary>
	/// Represents StrikeCurrency, FIX tag 947, with wire type <c>Currency</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class StrikeCurrency(string value)
		: Typed<string>(947, value);

	/// <summary>
	/// Represents NoNested3PartyIDs, FIX tag 948, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNested3PartyIDs((bool Valid, long Value) value)
		: Typed<long>(948, value);

	/// <summary>
	/// Represents Nested3PartyID, FIX tag 949, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Nested3PartyID(string value)
		: Typed<string>(949, value);

	/// <summary>
	/// Represents Nested3PartyIDSource, FIX tag 950, with wire type <c>char</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested3PartyIDSource((bool Valid, char Value) value)
		: Typed<char>(950, value);

	/// <summary>
	/// Represents Nested3PartyRole, FIX tag 951, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested3PartyRole((bool Valid, long Value) value)
		: Typed<long>(951, value);

	/// <summary>
	/// Represents NoNested3PartySubIDs, FIX tag 952, with wire type <c>NumInGroup</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class NoNested3PartySubIDs((bool Valid, long Value) value)
		: Typed<long>(952, value);

	/// <summary>
	/// Represents Nested3PartySubID, FIX tag 953, with wire type <c>String</c>.
	/// </summary>
	/// <param name="value">The field text, stored directly without primitive conversion.</param>
	public sealed class Nested3PartySubID(string value)
		: Typed<string>(953, value);

	/// <summary>
	/// Represents Nested3PartySubIDType, FIX tag 954, with wire type <c>int</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class Nested3PartySubIDType((bool Valid, long Value) value)
		: Typed<long>(954, value);

	/// <summary>
	/// Represents LegContractSettlMonth, FIX tag 955, with wire type <c>MonthYear</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegContractSettlMonth((bool Valid, FixMonthYear Value) value)
		: Typed<FixMonthYear>(955, value);

	/// <summary>
	/// Represents LegInterestAccrualDate, FIX tag 956, with wire type <c>LocalMktDate</c>.
	/// </summary>
	/// <param name="value">The primitive conversion result: its success flag and typed value.</param>
	public sealed class LegInterestAccrualDate((bool Valid, FixDate Value) value)
		: Typed<FixDate>(956, value);
}
