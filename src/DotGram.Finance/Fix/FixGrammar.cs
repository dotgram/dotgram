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
	Tag          = ['1'..'9'] & ['0'..'9']*
	Size         = ['0'..'9']+
	Text         = (?!Separator & any)+

	Data = @ReadData

	Field : @FixField =
		{ ?=any } & wire: (tag: Tag & '=' & switch @(context.Kind(tag)) {
			case 0: Text
			case 1:
				size: Size & Separator & dataTag: Tag & '='
				& when @(context.BeginData(tag, size, dataTag, parserSpan.Start + parserSpan.Length)) & Data
		}) & end: (Separator | eof)
		=> @(context.Create(wire, parserSpan.Start).WithTerminator(end.Length))

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

		int Kind(int tag)
		{
			return tag <= 0 ? -1 : options.DataTag(tag) != 0 ? 1 : options.IsData(tag) ? -1 : 0;
		}

		public int Kind(ReadOnlySpan<char> tag)
		{
			return Kind(FixConvert.Tag(tag));
		}

		public bool BeginData(ReadOnlySpan<char> tag, ReadOnlySpan<char> size, ReadOnlySpan<char> dataTag, int start)
		{
			var length = FixConvert.Tag(size);

			DataLimit = (long)start + length;

			return length >= 0 && options.DataTag(FixConvert.Tag(tag)) == FixConvert.Tag(dataTag);
		}

		public FixField Create(ReadOnlySpan<char> wire, int start)
		{
			var equals  = wire.IndexOf('=');
			var tag     = FixConvert.Tag(wire.Slice(0, equals));
			var dataTag = options.DataTag(tag);

			if (dataTag == 0)
				return FixFactory.Value(tag, wire.Slice(equals + 1));

			var second  = wire.IndexOf(options.Separator) + 1;
			var payload = second + wire.Slice(second).IndexOf('=') + 1;
			var value   = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);

			return FixFactory.Binary(dataTag, value.Data).WithBinary(value, start);
		}

		public int Kind(ReadOnlySpan<byte> tag)
		{
			return Kind(FixConvert.Tag(tag));
		}

		public bool BeginData(ReadOnlySpan<byte> tag, ReadOnlySpan<byte> size, ReadOnlySpan<byte> dataTag, int start)
		{
			var length = FixConvert.Tag(size);

			DataLimit = (long)start + length;

			return length >= 0 && options.DataTag(FixConvert.Tag(tag)) == FixConvert.Tag(dataTag);
		}

		public FixField Create(ReadOnlySpan<byte> wire, int start)
		{
			var equals  = wire.IndexOf((byte)'=');
			var tag     = FixConvert.Tag(wire.Slice(0, equals));
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
