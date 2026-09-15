using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>A vendor length/data pair. Both tag numbers must be outside the FIX 4.4 schema.</summary>
public readonly struct FixDataPair
{
	public FixDataPair(int lengthTag, int dataTag) { LengthTag = lengthTag; DataTag = dataTag; }
	public int LengthTag { get; }
	public int DataTag { get; }
}

/// <summary>An immutable parsing policy, reusable concurrently across messages.</summary>
public sealed class FixParseOptions
{
	readonly FixDataPair[] pairs;

	public FixParseOptions(FixParseMode mode = FixParseMode.Strict, params FixDataPair[] dataPairs) : this('\u0001', mode, dataPairs) { }

	/// <summary>Use SOH for wire input or pipe for a lossless log rendering.</summary>
	public FixParseOptions(char separator, FixParseMode mode = FixParseMode.Strict, params FixDataPair[] dataPairs)
	{
		if (separator != '\u0001' && separator != '|') throw new ArgumentOutOfRangeException(nameof(separator));
		Separator = separator;
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) throw new ArgumentOutOfRangeException(nameof(mode));
		if (dataPairs == null) throw new ArgumentNullException(nameof(dataPairs));
		Mode = mode;
		pairs = (FixDataPair[])dataPairs.Clone();
		var tags = new HashSet<int>();
		foreach (var pair in pairs)
		{
			if (pair.LengthTag <= 0 || pair.DataTag <= 0 || FixSchema.Type(pair.LengthTag) != null || FixSchema.Type(pair.DataTag) != null || !tags.Add(pair.LengthTag) || !tags.Add(pair.DataTag))
				throw new ArgumentException("Vendor pairs require unique positive tags outside the standard schema.", nameof(dataPairs));
		}
		DataPairs = Array.AsReadOnly(pairs);
	}

	public char Separator { get; }
	public FixParseMode Mode { get; }
	public IReadOnlyList<FixDataPair> DataPairs { get; }

	internal int LengthTag(int dataTag)
	{
		foreach (var pair in pairs) if (pair.DataTag == dataTag) return pair.LengthTag;
		return 0;
	}

	internal int DataTag(int lengthTag)
	{
		foreach (var pair in pairs) if (pair.LengthTag == lengthTag) return pair.DataTag;
		return 0;
	}

	internal string? Type(int tag) => LengthTag(tag) != 0 ? "data" : DataTag(tag) != 0 ? "Length" : null;
}
