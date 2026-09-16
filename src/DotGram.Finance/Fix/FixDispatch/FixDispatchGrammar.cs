using System;

using DotGram;

namespace DotGram.Finance.Fix;

[Gram("FixDispatchGrammar.gram", LocationType = typeof(IFixLocation), SpanCaptures = true, BufferedInput = true, Direct = false, Portable = false)]
sealed partial class FixDispatchGrammar;

sealed class FixDispatchContext(FixDispatchOptions options)
{
	public long DataLimit { get; private set; }
	int Kind(int tag) => tag <= 0 ? -1 : options.DataTag(tag) != 0 ? 1 : options.IsData(tag) ? -1 : 0;

	public int Kind(ReadOnlySpan<char> tag) => Kind(FixContext.Tag(tag));
	public bool BeginData(ReadOnlySpan<char> tag, ReadOnlySpan<char> size, ReadOnlySpan<char> dataTag, int start)
	{
		var length = FixContext.Tag(size);
		DataLimit = (long)start + length;
		return length >= 0 && options.DataTag(FixContext.Tag(tag)) == FixContext.Tag(dataTag);
	}

	public FixField Create(ReadOnlySpan<char> wire, int start)
	{
		var equals = wire.IndexOf('=');
		var tag = FixContext.Tag(wire.Slice(0, equals));
		var dataTag = options.DataTag(tag);
		if (dataTag == 0) return FixDispatchFactory.Value(tag, wire.Slice(equals + 1));
		var second = wire.IndexOf(options.Separator) + 1;
		var payload = second + wire.Slice(second).IndexOf('=') + 1;
		var value = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);
		return FixDispatchFactory.Binary(dataTag, value.Data).WithBinary(value, start);
	}

	public int Kind(ReadOnlySpan<byte> tag) => Kind(FixContext.Tag(tag));
	public bool BeginData(ReadOnlySpan<byte> tag, ReadOnlySpan<byte> size, ReadOnlySpan<byte> dataTag, int start)
	{
		var length = FixContext.Tag(size);
		DataLimit = (long)start + length;
		return length >= 0 && options.DataTag(FixContext.Tag(tag)) == FixContext.Tag(dataTag);
	}

	public FixField Create(ReadOnlySpan<byte> wire, int start)
	{
		var equals = wire.IndexOf((byte)'=');
		var tag = FixContext.Tag(wire.Slice(0, equals));
		var dataTag = options.DataTag(tag);
		if (dataTag == 0) return FixDispatchFactory.Value(tag, wire.Slice(equals + 1));
		var second = wire.IndexOf((byte)options.Separator) + 1;
		var payload = second + wire.Slice(second).IndexOf((byte)'=') + 1;
		var value = new FixBinaryValue(FixConvert.Data(wire.Slice(payload)), start + payload);
		return FixDispatchFactory.Binary(dataTag, value.Data).WithBinary(value, start);
	}
}
