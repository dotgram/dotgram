using System;

namespace DotGram.Finance.Fix44;

readonly struct FixBinaryValue((bool Valid, ReadOnlyMemory<byte> Value) data, int position)
{
	public (bool Valid, ReadOnlyMemory<byte> Value) Data     { get; } = data;
	public int                                      Position { get; } = position;
}
