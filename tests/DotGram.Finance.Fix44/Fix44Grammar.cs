using System;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Fix44;

[Gram("Fix44Grammar.gram", LocationType = typeof(IFixLocation), SpanCaptures = true, BufferedInput = true, PartSize = 1000, Portable = false)]
sealed partial class Fix44Grammar : FixFieldGrammar;

[Gram("FixField.gram", IncludedAs = "Known", Portable = false)]
abstract partial class FixFieldGrammar;

sealed class Fix44Context
{
	public long DataLimit { get; private set; }

	public bool BeginData(ReadOnlySpan<char> size, int start)
	{
		return BeginData(Tag(size), start);
	}

	public bool BeginData(ReadOnlySpan<byte> size, int start)
	{
		return BeginData(Tag(size), start);
	}

	bool BeginData(int size, int start)
	{
		DataLimit = (long)start + size;
		return size >= 0;
	}

	static bool IsUnknownText(int tag)
	{
		return tag > 0 && FixNames.Name(tag) is null;
	}

	public static bool IsUnknownText(ReadOnlySpan<char> tag)
	{
		return IsUnknownText(Tag(tag));
	}

	public static bool IsUnknownText(ReadOnlySpan<byte> tag)
	{
		return IsUnknownText(Tag(tag));
	}

	public static int Tag(ReadOnlySpan<char> value)
	{
		return FixConvert.Tag(value);
	}

	public static int Tag(ReadOnlySpan<byte> value)
	{
		return FixConvert.Tag(value);
	}
}
