using System;


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
		return tag > 0 && ((uint)tag >= (uint)Known.Length || !Known[tag]);
	}

	// The tags the standard defines, one flag a tag, from the constants that name them.
	static readonly bool[] Known = KnownTags();

	static bool[] KnownTags()
	{
		var tags  = typeof(FixTag).GetFields().Select(one => (int)one.GetRawConstantValue()!).ToArray();
		var known = new bool[tags.Max() + 1];

		foreach (var tag in tags)
			known[tag] = true;

		return known;
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
