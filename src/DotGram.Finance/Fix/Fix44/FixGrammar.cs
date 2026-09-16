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
	readonly string? text;
	readonly byte[]? bytes;
	public FixContext(string source, FixParseMode mode, FixParseOptions? options = null, char separator = '\u0001')
	{
		text = source;
		Mode = mode;
		Options = options;
		Separator = separator;
	}
	public FixContext(byte[] source, FixParseMode mode, FixParseOptions? options = null, char separator = '\u0001')
	{
		bytes = source;
		Mode = mode;
		Options = options;
		Separator = separator;
	}
	public FixParseMode Mode { get; }
	public FixParseOptions? Options { get; }
	public char Separator { get; }
	public string MessageType { get; set; } = "";
	int Length => text?.Length ?? bytes!.Length;
	int At(int index) => text != null ? text[index] : bytes![index];
	int Number(int start, int count)
	{
		if (text != null) return Tag(text.AsSpan(start, count));
		return Tag(bytes.AsSpan(start, count));
	}
	public int DataLimit { get; private set; }
	public bool BeginData(int start) { DataLimit = DataEnd(start); return DataLimit >= start; }
	int DataEnd(int start)
	{
		// Only the preceding length/data pair affects token boundaries. All group
		// interpretation is deferred to the semantic pass. Each atomic Data rule
		// installs its own bound before consuming bytes; alternatives cannot reuse it.
		var equal = start - 1;
		var tagStart = equal - 1;
		while (tagStart >= 0 && At(tagStart) != Separator) tagStart--;
		if (tagStart < 0) return -1;
		var dataTag = Number(tagStart + 1, equal - tagStart - 1);
		var lengthTag = FixSchema.LengthTag(dataTag);
		if (lengthTag == 0) lengthTag = Options?.LengthTag(dataTag) ?? 0;
		if (lengthTag == 0) return -1;
		var lengthEnd = tagStart;
		var lengthStart = lengthEnd - 1;
		while (lengthStart >= 0 && At(lengthStart) >= '0' && At(lengthStart) <= '9') lengthStart--;
		if (lengthStart < 0 || At(lengthStart) != '=') return -1;
		var previousStart = lengthStart - 1;
		while (previousStart >= 0 && At(previousStart) != Separator) previousStart--;
		if (Number(previousStart + 1, lengthStart - previousStart - 1) != lengthTag) return -1;
		var length = Number(lengthStart + 1, lengthEnd - lengthStart - 1);
		return length >= 0 && length < Length - start && At(start + length) == Separator ? start + length : -1;
	}
	public bool UnknownTag(ReadOnlySpan<char> tag) => UnknownTag(Tag(tag));
	public bool UnknownTag(ReadOnlySpan<byte> tag) => UnknownTag(Tag(tag));
	bool UnknownTag(int tag) => tag > 0 && FixSchema.Type(tag) == null;
	public bool IsDataTag(ReadOnlySpan<char> tag) => (Options?.LengthTag(Tag(tag)) ?? 0) != 0;
	public bool IsDataTag(ReadOnlySpan<byte> tag) => (Options?.LengthTag(Tag(tag)) ?? 0) != 0;
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
