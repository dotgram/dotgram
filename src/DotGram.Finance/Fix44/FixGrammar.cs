using System;

namespace DotGram.Finance.Fix44;

[Gram("""
	@using DotGram.Finance.Fix44;

	parse Fields                                 as ParseFields    stream bytes
	parse Fields                                 as ReadFields     stream bytes yield : @FixField
	parse Fields with (Separator = LogSeparator) as ParseLogFields stream bytes
	parse Fields with (Separator = LogSeparator) as ReadLogFields  stream bytes yield : @FixField

	context : @FixReading

	Separator    = ['\u0001']
	LogSeparator = ' '* & '|' & ' '*
	Text         = (?!Separator & any)+

	Tag  : @int = value: { ['1'..'9'] & ['0'..'9']* } => @(value.ToTag())
	Size : @int = value: { ['0'..'9']+ }              => @(value.ToTag())

	Field : @FixField =
		wire: (tag: Tag & '=' & switch @(context.Kind(tag)) {
			case 0: Text
			case 1: Size
			case 2: @ReadData
		}) & end: (Separator | eof)
		=> @(context.Field(tag, wire).WithTerminator(end.Length))

	Fields : @FixField[] =
		Field*
		recover Separator => @(new FixField.Invalid(parserText, parserSpan.Start, parserMessage))
	""",
	LocationType  = typeof(IFixLocation),
	SpanCaptures  = true,
	BufferedInput = true,
	MaxRetained   = 16 * 1024 * 1024,
	Portable      = false)]
sealed partial class FixGrammar
{
	// The data of a pair: as many as the length before it said, separators included.
	static bool ReadData(ParserInput<char> input, ref int position, FixReading context)
	{
		return input.TryAdvance(ref position, context.Size);
	}

	static bool ReadData(ParserInput<byte> input, ref int position, FixReading context)
	{
		return input.TryAdvance(ref position, context.Size);
	}

	/// <summary>One reading: the context, and the pair a length field has begun.</summary>
	public sealed class FixReading(FixContext context)
	{
		int _expected;

		/// <summary>The length the last length field gave.</summary>
		public int Size { get; private set; }

		/// <summary>
		/// 0 an ordinary value, 1 a length, 2 the data the length before it measures. A data tag
		/// anywhere else is -1, which no case takes: the field is refused and recovered from.
		/// </summary>
		public int Kind(int tag)
		{
			var expected = _expected;

			_expected = 0;

			if (tag == expected)
				return 2;

			return tag <= 0 ? -1 : context.Kind(tag);
		}

		/// <summary>
		/// The field of a tag, from its wire text: what follows the first '='. A length that is a number
		/// makes the next field its data, of that size.
		/// </summary>
		public FixField Field(int tag, ReadOnlySpan<char> wire)
		{
			var value = wire.Slice(wire.IndexOf('=') + 1);

			if (context.Kind(tag) > 0)
				Expect(tag, value.ToTag());

			return FixFieldBuilder.Value((FixTag)tag, context.Type(tag), value);
		}

		public FixField Field(int tag, ReadOnlySpan<byte> wire)
		{
			var value = wire.Slice(wire.IndexOf((byte)'=') + 1);

			if (context.Kind(tag) > 0)
				Expect(tag, value.ToTag());

			return FixFieldBuilder.Value((FixTag)tag, context.Type(tag), value);
		}

		void Expect(int tag, int size)
		{
			if (size >= 0)
			{
				_expected = context.DataTag(tag);
				Size      = size;
			}
		}
	}
}
