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
	readonly FixFieldOptions? options;
	int length;
	int dataTag;

	public FixContext(FixFieldOptions? options = null) => this.options = options;

	public long DataLimit { get; private set; }

	public bool BeginPair(int tag, ReadOnlySpan<char> field)
	{
		var start = field.IndexOf('=') + 1;
		return BeginPair(tag, Tag(field.Slice(start, field.Length - start - 1)));
	}

	public bool BeginPair(int tag, ReadOnlySpan<byte> field)
	{
		var start = field.IndexOf((byte)'=') + 1;
		return BeginPair(tag, Tag(field.Slice(start, field.Length - start - 1)));
	}

	bool BeginPair(int tag, int count)
	{
		dataTag = DataTag(tag);
		length = count;
		return dataTag > 0 && length >= 0;
	}

	public bool BeginData(int start)
	{
		DataLimit = (long)start + length;
		return true;
	}

	int DataTag(int tag)
	{
		var standard = FixSchema.DataTag(tag);
		if (standard != 0) return standard;
		if (options != null)
			foreach (var pair in options.DataPairs)
				if (pair.LengthTag == tag) return pair.DataTag;
		return 0;
	}

	bool IsDataTag(int tag) => FixSchema.LengthTag(tag) != 0 || (options?.IsDataTag(tag) ?? false);
	bool IsTextTag(int tag) => tag > 0 && DataTag(tag) == 0 && !IsDataTag(tag);
	public bool IsTextTag(ReadOnlySpan<char> tag) => IsTextTag(Tag(tag));
	public bool IsTextTag(ReadOnlySpan<byte> tag) => IsTextTag(Tag(tag));
	public bool IsDataTag(ReadOnlySpan<char> tag) => IsDataTag(Tag(tag));
	public bool IsDataTag(ReadOnlySpan<byte> tag) => IsDataTag(Tag(tag));
	public bool IsLengthTag(ReadOnlySpan<char> tag) => DataTag(Tag(tag)) != 0;
	public bool IsLengthTag(ReadOnlySpan<byte> tag) => DataTag(Tag(tag)) != 0;
	public bool IsPairData(ReadOnlySpan<char> tag) => Tag(tag) == dataTag;
	public bool IsPairData(ReadOnlySpan<byte> tag) => Tag(tag) == dataTag;
	public static bool IsUnknown(ReadOnlySpan<char> tag) => Tag(tag) > 0 && FixSchema.Type(Tag(tag)) == null;
	public static bool IsUnknown(ReadOnlySpan<byte> tag) => Tag(tag) > 0 && FixSchema.Type(Tag(tag)) == null;
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
