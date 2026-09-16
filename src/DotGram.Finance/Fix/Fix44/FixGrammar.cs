using System;
using System.Globalization;

using DotGram;

namespace DotGram.Finance.Fix;

[Gram("FixGrammar.gram", LocationType = typeof(IFixLocation), SpanCaptures = true, BufferedInput = true, Direct = false, PartSize = 1000, Portable = false)]
sealed partial class FixGrammar : FixFieldGrammar;

[Gram("FixField.gram", IncludedAs = "Known", Portable = false)]
abstract partial class FixFieldGrammar;

sealed class FixContext
{
	public long DataLimit { get; private set; }

	public bool BeginData(ReadOnlySpan<char> size, int start) => BeginData(Tag(size), start);
	public bool BeginData(ReadOnlySpan<byte> size, int start) => BeginData(Tag(size), start);
	bool BeginData(int size, int start)
	{
		DataLimit = (long)start + size;
		return size >= 0;
	}

	static bool IsUnknownText(int tag) => tag > 0 && FixSchema.Type(tag) == null;
	public static bool IsUnknownText(ReadOnlySpan<char> tag) => IsUnknownText(Tag(tag));
	public static bool IsUnknownText(ReadOnlySpan<byte> tag) => IsUnknownText(Tag(tag));
	public static int Tag(ReadOnlySpan<char> value) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var tag) ? tag : -1;
	public static int Tag(ReadOnlySpan<byte> value)
	{
		var result = 0;
		if (value.IsEmpty) return -1;
		foreach (var c in value)
		{
			var digit = c - '0';
			if (digit < 0 || digit > 9 || result > (int.MaxValue - digit) / 10) return -1;
			result = result * 10 + digit;
		}
		return result;
	}
}
