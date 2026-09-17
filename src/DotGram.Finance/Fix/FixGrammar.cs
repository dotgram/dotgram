using System;

namespace DotGram.Finance.Fix;

[Gram("""
	@using DotGram.Finance.Fix;

	parse Fields                                 as ParseFields    stream bytes
	parse Fields                                 as ReadFields     stream bytes yield : @FixField
	parse Fields with (Separator = LogSeparator) as ParseLogFields stream bytes
	parse Fields with (Separator = LogSeparator) as ReadLogFields  stream bytes yield : @FixField

	context : @FixContext

	Separator    = ['\u0001']
	LogSeparator = ['|']
	Text         = (?!Separator & any)+

	Tag  : @int = value: { ['1'..'9'] & ['0'..'9']* } => @(FixConvert.Tag(value))
	Size : @int = value: { ['0'..'9']+ }             => @(FixConvert.Tag(value))

	Data = @ReadData

	Field : @FixField =
		{ ?=any } & wire: (tag: Tag & '=' & switch @(context.Kind(tag)) {
			case 0: Text
			case 1:
				size: Size & Separator & dataTag: Tag & '='
				& when @(context.BeginData(tag, size, dataTag, parserSpan.Start + parserSpan.Length)) & Data
		}) & end: (Separator | eof)
		=> @(context.Create(tag, wire, parserSpan.Start).WithTerminator(end.Length))

	Fields : @FixField[] = Field* recover Separator => @(new FixField.Invalid(parserText, parserSpan.Start, parserMessage))
""",
	LocationType  = typeof(IFixLocation),
	SpanCaptures  = true,
	BufferedInput = true,
	Direct        = false,
	Portable      = false)]
sealed partial class FixGrammar
{
	static bool ReadData(ParserInput<char> input, ref int position, FixContext context)
	{
		var length = context.DataLimit - position;

		return length >= 0 && length <= int.MaxValue && input.TryAdvance(ref position, (int)length);
	}

	static bool ReadData(ParserInput<byte> input, ref int position, FixContext context)
	{
		var length = context.DataLimit - position;

		return length >= 0 && length <= int.MaxValue && input.TryAdvance(ref position, (int)length);
	}

	public sealed class FixContext(FixOptions options)
	{
		public long DataLimit { get; private set; }

		public int Kind(int tag)
		{
			return tag <= 0 ? -1 : options.DataTag(tag) != 0 ? 1 : options.IsData(tag) ? -1 : 0;
		}

		// Captures belonging to only one switch arm are optional in the generated guard.
		public bool BeginData(int tag, int? size, int? dataTag, int start)
		{
			if (size is not int length || length < 0 || options.DataTag(tag) != dataTag)
				return false;

			DataLimit = (long)start + length;

			return true;
		}

		public FixField Create(int tag, ReadOnlySpan<char> wire, int start)
		{
			var equals  = wire.IndexOf('=');
			var dataTag = options.DataTag(tag);

			if (dataTag == 0)
				return FixFactory.Value(tag, wire.Slice(equals + 1));

			var second  = wire.IndexOf(options.Separator) + 1;
			var payload = second + wire.Slice(second).IndexOf('=') + 1;
			var value   = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);

			return FixFactory.Binary(dataTag, value.Data).WithBinary(value, start);
		}

		public FixField Create(int tag, ReadOnlySpan<byte> wire, int start)
		{
			var equals  = wire.IndexOf((byte)'=');
			var dataTag = options.DataTag(tag);

			if (dataTag == 0)
				return FixFactory.Value(tag, wire.Slice(equals + 1));

			var second  = wire.IndexOf((byte)options.Separator) + 1;
			var payload = second + wire.Slice(second).IndexOf((byte)'=') + 1;
			var value   = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);

			return FixFactory.Binary(dataTag, value.Data).WithBinary(value, start);
		}
	}
}
