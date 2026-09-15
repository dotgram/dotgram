using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram;

namespace DotGram.Finance.Fix;

static class FixGrammar
{
	// The grammar recognizes both tags and the length field. This recognizer only
	// advances across the length-delimited payload, which may contain SOH itself.
	internal static bool ReadData(ReadOnlySpan<char> input, ref int position)
	{
		var dataTagStart = position - 2;
		while (dataTagStart >= 0 && input[dataTagStart] != '\u0001') dataTagStart--;
		if (dataTagStart < 0) return false;
		var lengthEnd = dataTagStart;
		var lengthStart = lengthEnd - 1;
		while (lengthStart >= 0 && input[lengthStart] >= '0' && input[lengthStart] <= '9') lengthStart--;
		if (lengthStart < 0 || input[lengthStart] != '=') return false;
		if (!int.TryParse(input.Slice(lengthStart + 1, lengthEnd - lengthStart - 1), NumberStyles.None, CultureInfo.InvariantCulture, out var length)) return false;
		if (length >= input.Length - position || input[position + length] != '\u0001') return false;
		position += length;
		return true;
	}
}

sealed class FixContext
{
	readonly List<(FixNode Counter, int Remaining)> groupStack = new();
	public FixParseError? Error { get; set; }
	public bool HasEntry => groupStack.Count > 0 && groupStack[groupStack.Count - 1].Remaining > 0;

	public bool BeginGroup(FixNode counter)
	{
		if (!counter.Field(Source).TryGetInt64(out var count) || count < 0 || count > Source.Length / 4)
		{
			Error = new FixParseError(counter.ValuePosition, counter.Tag, MessageType, "Invalid or impossible NumInGroup.");
			return false;
		}
		groupStack.Add((counter, (int)count));
		return true;
	}

	public bool ConsumeEntry()
	{
		if (!HasEntry) return false;
		var frame = groupStack[groupStack.Count - 1];
		groupStack[groupStack.Count - 1] = (frame.Counter, frame.Remaining - 1);
		return true;
	}

	public bool EndGroup()
	{
		var frame = groupStack[groupStack.Count - 1];
		if (frame.Remaining != 0)
		{
			Error = new FixParseError(frame.Counter.ValuePosition, frame.Counter.Tag, MessageType, "Fewer group entries than NumInGroup specifies.");
			return false;
		}
		groupStack.RemoveAt(groupStack.Count - 1);
		return true;
	}
	public FixContext(string source, FixParseMode mode, FixParseOptions? options = null)
	{
		Source = source;
		Mode = mode;
		Options = options;
	}

	public string Source { get; }
	public static FixNode[] Header(FixNode begin, FixNode length, FixNode type, FixNode[] rest)
	{
		var result = new FixNode[rest.Length + 3];
		result[0] = begin;
		result[1] = length;
		result[2] = type;
		Array.Copy(rest, 0, result, 3, rest.Length);
		return result;
	}
	public FixParseMode Mode { get; }
	public FixParseOptions? Options { get; }
	public string MessageType { get; set; } = "";
	public bool SetMessageType(FixNode node)
	{
		MessageType = node.Field(Source).ToString();
		return true;
	}
	public bool IsExtension(int start, int length)
	{
		var tag = TagAt(start, length);
		return tag > 0 && FixSchema.Type(tag) == null && (Options?.LengthTag(tag) ?? 0) == 0 && (Mode == FixParseMode.Lenient || (Options?.DataTag(tag) ?? 0) != 0);
	}
	public bool IsExtensionData(int start, int length) => (Options?.LengthTag(TagAt(start, length)) ?? 0) != 0;
	public int TagAt(int start, int length) => Tag(Source.AsSpan(start, length));
	public bool IsPlainBody(int start, int length)
	{
		var tag = TagAt(start, length);
		return tag > 0 && tag != 10 && tag != 89 && tag != 93 && FixSchema.Type(tag) != "data" && (Options?.LengthTag(tag) ?? 0) == 0;
	}
	public static int Tag(ReadOnlySpan<char> value) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var tag) ? tag : -1;
}
