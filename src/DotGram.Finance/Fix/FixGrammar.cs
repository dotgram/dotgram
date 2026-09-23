using System;

namespace DotGram.Finance.Fix;

[Gram("""
	@using DotGram.Finance.Fix;

	parse Fields                                 as ParseFields    stream bytes
	parse Fields                                 as ReadFields     stream bytes yield : @FixField
	parse Fields with (Separator = LogSeparator) as ParseLogFields stream bytes
	parse Fields with (Separator = LogSeparator) as ReadLogFields  stream bytes yield : @FixField

	context : @FixReading

	Separator    = ['\u0001']
	LogSeparator = ' '* & '|' & ' '*
	Text         = (?!Separator & any)+

	Tag  : @int = value: { ['1'..'9'] & ['0'..'9']* } => @(FixConvert.Tag(value))
	Size : @int = value: { ['0'..'9']+ }              => @(FixConvert.Tag(value))

	Field : @FixField =
		wire: (tag: Tag & '=' & switch @(context.Kind(tag)) {
			case 0: Text
			case 1:
				size: Size & Separator & dataTag: Tag & '='
				& when @(context.BeginData(tag, size, dataTag, parserSpan.Start + parserSpan.Length)) & @ReadData
		}) & end: (Separator | eof)
		=> @((dataTag is int data ? context.Binary(data, wire, parserSpan.Start) : context.Text(tag, wire)).WithTerminator(end.Length))

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
	static bool ReadData(ParserInput<char> input, ref int position, FixReading context)
	{
		var length = context.DataLimit - position;

		return length is >= 0 and <= int.MaxValue && input.TryAdvance(ref position, (int)length);
	}

	static bool ReadData(ParserInput<byte> input, ref int position, FixReading context)
	{
		var length = context.DataLimit - position;

		return length is >= 0 and <= int.MaxValue && input.TryAdvance(ref position, (int)length);
	}

	public sealed class FixReading(FixContext context)
	{
		public long DataLimit { get; private set; }

		// One read of a table the tag indexes. Whether a consumer declared pairs of their own was
		// settled when the context were built, so nothing is asked of anybody here.
		public int Kind(int tag)
		{
			return tag <= 0 ? -1 : context.Kind(tag);
		}

		// Captures belonging to only one switch arm are optional in the generated guard.
		public bool BeginData(int tag, int? size, int? dataTag, int start)
		{
			if (size is not int length || length < 0 || context.DataTag(tag) != dataTag)
				return false;

			DataLimit = (long)start + length;

			return true;
		}

		// A text field: its value is what follows the '=' of its wire text. Found again here rather
		// than captured, because a capture costs the engine more on every field than this search.
		public FixField Text(int tag, ReadOnlySpan<char> wire)
		{
			return FixFieldFactory.Value(tag, wire.Slice(wire.IndexOf('=') + 1), context.CustomFields);
		}

		public FixField Text(int tag, ReadOnlySpan<byte> wire)
		{
			return FixFieldFactory.Value(tag, wire.Slice(wire.IndexOf((byte)'=') + 1), context.CustomFields);
		}

		// A length/data pair, whose data tag the grammar has read: the payload follows the second
		// '=' of the pair's wire text.
		public FixField Binary(int dataTag, ReadOnlySpan<char> wire, int start)
		{
			var second  = wire.IndexOf('=') + 1;
			var payload = second + wire.Slice(second).IndexOf('=') + 1;
			var value   = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);

			return FixFieldFactory.Binary(dataTag, value.Data, context.CustomFields).WithBinary(value, start);
		}

		public FixField Binary(int dataTag, ReadOnlySpan<byte> wire, int start)
		{
			var second  = wire.IndexOf((byte)'=') + 1;
			var payload = second + wire.Slice(second).IndexOf((byte)'=') + 1;
			var value   = new FixBinaryValue(FixConvert.Data(wire[payload..]), start + payload);

			return FixFieldFactory.Binary(dataTag, value.Data, context.CustomFields).WithBinary(value, start);
		}
	}
}
