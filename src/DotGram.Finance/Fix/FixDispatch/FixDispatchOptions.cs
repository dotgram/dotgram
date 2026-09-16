using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>Delimiter and optional replacement length/data dictionary for FixDispatch.</summary>
public sealed class FixDispatchOptions
{
	internal static readonly FixDispatchOptions Default = new();
	readonly Dictionary<int, int>? pairs;
	readonly HashSet<int>? dataTags;

	/// <param name="lengthDataPairs">Null uses the standard dictionary. A supplied dictionary replaces it and is copied.</param>
	/// <param name="separator">SOH for wire input or pipe for logs.</param>
	public FixDispatchOptions(char separator = '\u0001', IReadOnlyDictionary<int, int>? lengthDataPairs = null)
	{
		if (separator != '\u0001' && separator != '|') throw new ArgumentOutOfRangeException(nameof(separator));
		Separator = separator;
		if (lengthDataPairs == null) return;
		pairs = new Dictionary<int, int>();
		dataTags = new HashSet<int>();
		foreach (var pair in lengthDataPairs)
		{
			if (pair.Key <= 0 || pair.Value <= 0 || pair.Key == pair.Value ||
				(FixSchema.Type(pair.Value) is { } type && type != "data") || !dataTags.Add(pair.Value))
				throw new ArgumentException("Pairs require positive, distinct length tags and unique data tags with binary or unknown types.", nameof(lengthDataPairs));
			pairs.Add(pair.Key, pair.Value);
		}
		foreach (var tag in dataTags)
			if (pairs.ContainsKey(tag)) throw new ArgumentException("A data tag cannot also be a length tag.", nameof(lengthDataPairs));
	}

	public char Separator { get; }
	internal int DataTag(int tag) => pairs == null ? FixSchema.DataTag(tag) : pairs.TryGetValue(tag, out var data) ? data : 0;
	internal bool IsData(int tag) => FixSchema.Type(tag) == "data" || dataTags?.Contains(tag) == true;
}
