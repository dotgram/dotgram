using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// One text made of several, and the map that says which one a position came from.
/// </summary>
/// <remarks>
/// <para>
/// A grammar that includes another is compiled as one text — one parse, one position
/// space — because the alternative is for every <see cref="Parsing.Location"/> to carry
/// where it came from, and that is a field on everything to serve a question asked at the
/// end. So the texts are joined and this undoes the joining afterwards: a position in the
/// whole is found in its segment, translated back to that segment's own offsets, and
/// handed to the map the segment came with.
/// </para>
/// <para>
/// The first segment starts at zero and is the grammar the author is editing, which is
/// what makes the common case cost nothing: its positions are the ones they always were,
/// and only what follows needs translating.
/// </para>
/// </remarks>
public sealed class SplicedLineMap : ILineMap
{
	/// <summary>One text inside the joined one.</summary>
	/// <param name="Start">Where it begins in the joined text.</param>
	/// <param name="Length">How much of the joined text is its.</param>
	/// <param name="Map">Where a position inside it belongs, in its own offsets.</param>
	public readonly record struct Segment(int Start, int Length, ILineMap? Map)
	{
		public int End => Start + Length;
	}

	readonly Segment[] _segments;

	/// <param name="segments">
	/// In the order they were joined, each beginning where the one before it ended. Not
	/// checked for gaps: they are built by the same code that built the text, and a
	/// position landing in one would be answered `false` rather than wrongly.
	/// </param>
	public SplicedLineMap(IReadOnlyList<Segment> segments)
	{
		if (segments is null)
			throw new ArgumentNullException(nameof(segments));

		_segments = [.. segments];
	}

	/// <summary>Which segment holds this position, or -1.</summary>
	/// <remarks>
	/// Public because the diagnostics half asks the same question for a different reason:
	/// a `#line` needs the segment's line map and a squiggle needs the segment's host, and
	/// working out which segment twice from two sets of numbers is how the two come to
	/// disagree.
	/// </remarks>
	public int SegmentAt(int position)
	{
		// Linear: a chain of grammars is two or three long, and the search that would beat
		// it costs more to read than it saves.
		for (var at = 0; at < _segments.Length; at++)
			if (position >= _segments[at].Start && position < _segments[at].End)
				return at;

		// The very end of the last segment is a position a diagnostic may name — a rule
		// that wanted one more character fails where that character would have gone.
		return _segments.Length > 0 && position == _segments[_segments.Length - 1].End
			? _segments.Length - 1
			: -1;
	}

	public IReadOnlyList<Segment> Segments => _segments;

	public bool TryMap(int position, out string file, out int line, out int column)
	{
		file   = "";
		line   = 0;
		column = 0;

		if (SegmentAt(position) is var at && at < 0)
			return false;

		var segment = _segments[at];

		return segment.Map is { } map && map.TryMap(position - segment.Start, out file, out line, out column);
	}
}
