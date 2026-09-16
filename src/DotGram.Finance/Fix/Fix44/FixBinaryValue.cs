using System;

namespace DotGram.Finance.Fix;

readonly struct FixBinaryValue
{
	public FixBinaryValue((bool Valid, ReadOnlyMemory<byte> Value) data, int position) { Data = data; Position = position; }
	public (bool Valid, ReadOnlyMemory<byte> Value) Data { get; }
	public int Position { get; }
}
