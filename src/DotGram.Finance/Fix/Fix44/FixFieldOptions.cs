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

/// <summary>Syntax options only: the delimiter and vendor binary-field boundaries.</summary>
public sealed class FixFieldOptions
{
	readonly FixDataPair[] pairs;

	public FixFieldOptions(char separator = '\u0001', params FixDataPair[] dataPairs)
	{
		if (separator != '\u0001' && separator != '|') throw new ArgumentOutOfRangeException(nameof(separator));
		if (dataPairs == null) throw new ArgumentNullException(nameof(dataPairs));
		Separator = separator;
		pairs = (FixDataPair[])dataPairs.Clone();
		var tags = new HashSet<int>();
		foreach (var pair in pairs)
			if (pair.LengthTag <= 0 || pair.DataTag <= 0 || !tags.Add(pair.LengthTag) || !tags.Add(pair.DataTag))
				throw new ArgumentException("Length/data pairs require distinct positive tags.", nameof(dataPairs));
		DataPairs = Array.AsReadOnly(pairs);
	}

	public char Separator { get; }
	public IReadOnlyList<FixDataPair> DataPairs { get; }

	internal bool IsLengthTag(int tag)
	{
		foreach (var pair in pairs) if (pair.LengthTag == tag) return true;
		return false;
	}

	internal bool IsDataTag(int tag)
	{
		foreach (var pair in pairs) if (pair.DataTag == tag) return true;
		return false;
	}
}
