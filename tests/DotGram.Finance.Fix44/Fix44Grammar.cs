using System;


namespace DotGram.Finance.Fix44;

[Gram("Fix44Grammar.gram", LocationType = typeof(IFixLocation), SpanCaptures = true, BufferedInput = true, PartSize = 1000, Portable = false)]
sealed partial class Fix44Grammar : FixFieldGrammar;

[Gram("FixField.gram", IncludedAs = "Known", Portable = false)]
abstract partial class FixFieldGrammar;

sealed class Fix44Context
{
	public long DataLimit { get; private set; }

	int _pending, _current, _size;

	// Each field takes the expectation the one before it left, and leaves none unless it is a length.
	public bool Next()
	{
		_current = _pending;
		_pending = 0;
		return true;
	}

	public bool Expect(int dataTag, ReadOnlySpan<char> size)
	{
		return Expect(dataTag, Tag(size));
	}

	public bool Expect(int dataTag, ReadOnlySpan<byte> size)
	{
		return Expect(dataTag, Tag(size));
	}

	bool Expect(int dataTag, int size)
	{
		_pending = dataTag;
		_size    = size;
		return size >= 0;
	}

	// A capture of one alternative reaches a guard as optional, though it is always there.
	public bool Takes(int dataTag, int? start)
	{
		DataLimit = (long)(start ?? 0) + _size;
		return _current == dataTag;
	}

	static bool IsUnknownText(int tag)
	{
		return tag > 0 && ((uint)tag >= (uint)Known.Length || !Known[tag]);
	}

	// The tags the standard defines, one flag a tag, from the constants that name them.
	static readonly bool[] Known = KnownTags();

	static bool[] KnownTags()
	{
		var tags  = Enum.GetValues<FixTag>().Select(one => (int)one).ToArray();
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
		return FixConvert.ToTag(value);
	}

	public static int Tag(ReadOnlySpan<byte> value)
	{
		return FixConvert.ToTag(value);
	}
}
