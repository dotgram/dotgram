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
	int fieldStart;
	int lengthStart;
	int lengthEnd = -1;
	int length = -1;

	public FixContext(FixFieldOptions? options = null) => this.options = options;

	public long DataLimit { get; private set; }

	public static FixField InvalidField(int position) => throw new FormatException("Invalid FIX field at offset " + position + ".");

	public bool BeginField(int start)
	{
		fieldStart = start;
		return true;
	}

	public bool BeginLength(int start)
	{
		lengthStart = start;
		return true;
	}

	public bool RecordLength(int end, ReadOnlySpan<char> value)
	{
		lengthEnd = end;
		length = Tag(value.Slice(value.Length - (end - lengthStart)));
		return true;
	}

	public bool RecordLength(int end, ReadOnlySpan<byte> value)
	{
		lengthEnd = end;
		length = Tag(value.Slice(value.Length - (end - lengthStart)));
		return true;
	}

	public bool BeginData(int start)
	{
		// The adjacent Length value determines the raw extent. Pair membership is
		// a semantic check. Source positions make speculative calls idempotent.
		if ((long)lengthEnd + 1 != fieldStart || length < 0) return false;
		DataLimit = (long)start + length;
		return true;
	}

	public bool IsDataTag(ReadOnlySpan<char> tag) => options?.IsDataTag(Tag(tag)) ?? false;
	public bool IsDataTag(ReadOnlySpan<byte> tag) => options?.IsDataTag(Tag(tag)) ?? false;
	public bool IsLengthTag(ReadOnlySpan<char> tag) => options?.IsLengthTag(Tag(tag)) ?? false;
	public bool IsLengthTag(ReadOnlySpan<byte> tag) => options?.IsLengthTag(Tag(tag)) ?? false;
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
