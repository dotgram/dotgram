using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Optional replacement length/data dictionary for FIX fields.
/// </summary>
public sealed class FixFieldOptions
{
	internal static readonly FixFieldOptions Default = new();
	readonly Dictionary<int, int>?           _pairs;
	readonly HashSet<int>?                   _dataTags;

	/// <param name="lengthDataPairs">Null uses the standard dictionary. A supplied dictionary replaces it and is copied.</param>
	public FixFieldOptions(IReadOnlyDictionary<int,int>? lengthDataPairs = null)
	{
		if (lengthDataPairs == null)
			return;

		_pairs    = new Dictionary<int, int>();
		_dataTags = [];

		foreach (var pair in lengthDataPairs)
		{
			if (pair.Key <= 0 || pair.Value <= 0 || pair.Key == pair.Value ||
				(FixSchema.Type(pair.Value) is { } type && type != "data") || !_dataTags.Add(pair.Value))
				throw new ArgumentException("Pairs require positive, distinct length tags and unique data tags with binary or unknown types.", nameof(lengthDataPairs));

			_pairs.Add(pair.Key, pair.Value);
		}

		foreach (var tag in _dataTags)
			if (_pairs.ContainsKey(tag))
				throw new ArgumentException("A data tag cannot also be a length tag.", nameof(lengthDataPairs));
	}

	internal int  DataTag(int tag)
	{
		return _pairs == null ? FixSchema.DataTag(tag) : _pairs.TryGetValue(tag, out var data) ? data : 0;
	}

	internal bool IsData (int tag)
	{
		return (uint)tag < (uint)SchemaData.Length && SchemaData[tag] || _dataTags?.Contains(tag) == true;
	}

	// Which standard tags are raw data, looked up once rather than as a string on every field.
	// The highest standard tag is 956; a custom data tag is answered by the options' own set.
	static readonly bool[] SchemaData = CreateSchemaData();

	static bool[] CreateSchemaData()
	{
		var data = new bool[957];

		for (var tag = 1; tag < data.Length; tag++)
			data[tag] = FixSchema.Type(tag) == "data";

		return data;
	}
}
