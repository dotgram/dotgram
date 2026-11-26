using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Where a run of a class read before ends, kept while one input is read: a run begun inside one
/// already read ends where that one did.
/// </summary>
/// <remarks>
/// <para>
/// A rule ending in a run that gives back is read, where it is called before something that could
/// take none of it, by a settled copy (<see cref="RecognitionGraph.Settled"/>); the copy gives no
/// character back, but a caller that gives back still asks it to read again from each place the
/// caller gave back to — the name of an attribute read from every character of an element's name
/// refused at its end. Each of those readings scans to the same end. The first reading of the run
/// writes down where it began and where it ended; a reading that begins between the two takes the
/// end and reads nothing.
/// </para>
/// <para>
/// A function of the text alone: a run of one class from any place inside another run of it ends
/// where that one ends. So it is kept for the rule and its copy alike, which read the same class —
/// the copy's run is told by the node it was copied from — and what is written down is the end the
/// scan reached, never a shorter reading given back afterwards. Only in a reader over a string,
/// which a grammar with copies is read by alone, and only for a run with no most. One reading of
/// an input keeps it in the reader's registers, which a new reading begins with empty.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>Each run kept, by the node its class is written as in the rule a copy was made from, and its number.</summary>
	Dictionary<Node, int>? _runEnds;

	/// <summary>The runs this machine's readers keep, by number: the registers written.</summary>
	readonly SortedSet<int> _runEndsRead = [];

	/// <summary>
	/// The number of the run whose end a run of <paramref name="body"/> may take, where it is one a
	/// settled copy reads; null where it is not, or where nothing is kept.
	/// </summary>
	internal int? RunEndOf(Node body)
	{
		if (!_settings.RemembersRunEnds || BufferedInput || OverKinds || _graph.Settled.Count == 0)
			return null;

		_runEnds ??= RunEnds();

		if (!_runEnds.TryGetValue(_graph.Original(body), out var number))
			return null;

		_runEndsRead.Add(number);

		return number;
	}

	Dictionary<Node, int> RunEnds()
	{
		var ends = new Dictionary<Node, int>(NodeIdentity.Instance);

		foreach (var copy in _graph.Settled.Keys)
			if (_graph.Bodies.TryGetValue(copy, out var body))
				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Repeat { Max: null } repeat && RunTest(repeat.Body) is not null &&
						!ends.ContainsKey(_graph.Original(repeat.Body)))
						ends[_graph.Original(repeat.Body)] = ends.Count;

		return ends;
	}

	/// <summary>
	/// The registers of the runs kept: where each began, one past so that nought is none, and where it
	/// ended. Carried with the other registers onto a stack of its own and back.
	/// </summary>
	IEnumerable<(string Type, string Name)> RunEndRegisters()
	{
		foreach (var number in _runEndsRead)
		{
			yield return ("int", $"runFrom{number}");
			yield return ("int", $"runTo{number}");
		}
	}
}
