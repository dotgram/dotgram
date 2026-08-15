using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The map for a grammar that is its own file: a position stands for itself.
/// </summary>
/// <remarks>
/// Pure, so it lives on this side — counting lines in a string needs nothing a host
/// knows. The inline case is the one that does.
/// </remarks>
public sealed class GrammarLineMap : ILineMap
{
	readonly string     _path;
	readonly List<int>  _starts = [0];
	readonly int        _length;

	/// <param name="path">Written into the directive, so it is what an editor will open.</param>
	public GrammarLineMap(string text, string path)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		_path   = path ?? throw new ArgumentNullException(nameof(path));
		_length = text.Length;

		for (var i = 0; i < text.Length; i++)
			if (text[i] == '\n')
				_starts.Add(i + 1);
	}

	public bool TryMap(int position, out string file, out int line, out int column)
	{
		file   = _path;
		line   = 0;
		column = 0;

		if (position < 0 || position > _length)
			return false;

		// The last line that starts at or before the position. Binary search because a
		// grammar of a thousand lines is asked this once per construction and once per
		// guard, and a linear scan would be a thousand of those.
		var low  = 0;
		var high = _starts.Count - 1;

		while (low < high)
		{
			var middle = (low + high + 1) / 2;

			if (_starts[middle] <= position)
				low = middle;
			else
				high = middle - 1;
		}

		line   = low + 1;
		column = position - _starts[low] + 1;

		return true;
	}
}
