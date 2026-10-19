using System;
using System.Collections;

namespace DotGram.Finance.Fix;

/// <summary>
/// One run of the checks over one message: where its findings go, and the groups and entries the
/// check is in now, which every finding made there is said with.
/// </summary>
/// <remarks>
/// <para>
/// Nothing of a run is kept on the message, so a message validates the same however often and from
/// however many threads it is asked, each run against the context it is given. The run itself is
/// not made per call either: a thread keeps one between its calls and lends it to the next, and
/// only a run that starts inside another on the same thread — a custom message validating another
/// from <c>OnValidate</c> — makes its own. A valid message is validated without allocating.
/// </para>
/// <para>
/// The path is values: the counter of each group the check has entered and the entry it is at,
/// set by the walk over a group's entries. The array a finding carries is made when the first
/// finding of an entry is, and shared by the findings that follow it there.
/// </para>
/// </remarks>
sealed class FixCheck
{
	// The run a thread lends to its next Validate, or null while it is lent.
	[ThreadStatic] static FixCheck? _spare;

	// What a run without a caller's collection fills, kept for the next; dropped when one message made
	// it larger than a few hundred findings, so that one bad message does not hold its memory for good.
	List<FixFinding>        _buffer = [];
	ICollection<FixFinding> _findings;
	FixGroupStep[]          _steps  = new FixGroupStep[8];
	int                     _depth;
	FixGroupStep[]?         _path;

	FixCheck()
	{
		_findings = _buffer;
	}

	/// <summary>Where this run's findings go: the caller's collection, or the run's own.</summary>
	public ICollection<FixFinding> Findings => _findings;

	/// <summary>A run that adds its findings to the caller's collection, or, given none, to its own.</summary>
	public static FixCheck Rent(ICollection<FixFinding>? findings)
	{
		var check = _spare ?? new FixCheck();

		_spare          = null;
		check._findings = findings ?? check._buffer;

		return check;
	}

	/// <summary>What the run found, when it was given no collection: a new array, or the empty one.</summary>
	public IReadOnlyList<FixFinding> Found()
	{
		return _buffer.Count == 0 ? Array.Empty<FixFinding>() : _buffer.ToArray();
	}

	/// <summary>Gives the run back to the thread, emptied, for its next Validate.</summary>
	public void Return()
	{
		if (_buffer.Count > 256)
			_buffer = [];
		else
			_buffer.Clear();

		_findings = _buffer;
		_depth    = 0;
		_path     = null;
		_spare    = this;
	}

	/// <summary>Adds one finding, said in the groups and entries the check is in now.</summary>
	public void Add(FixRule rule, int tag, int position, FixField? field)
	{
		if (_depth > 0 && _path is null)
		{
			_path = new FixGroupStep[_depth];

			Array.Copy(_steps, _path, _depth);
		}

		_findings.Add(FixFinding.At(rule, tag, position, field, _depth == 0 ? null : _path));
	}

	/// <summary>Enters the group a counter counts, before its first entry.</summary>
	public void Enter(int counter)
	{
		if (_depth == _steps.Length)
			Array.Resize(ref _steps, _depth * 2);

		_steps[_depth++] = new FixGroupStep(counter, 0);
		_path            = null;
	}

	/// <summary>Moves to an entry of the group entered last.</summary>
	public void At(int index)
	{
		_steps[_depth - 1] = new FixGroupStep(_steps[_depth - 1].CounterTag, index);
		_path              = null;
	}

	/// <summary>Leaves the group entered last.</summary>
	public void Leave()
	{
		_depth--;
		_path = null;
	}
}

/// <summary>What reading found wrong with a message: added to while it is read, and only read after.</summary>
/// <remarks>Not a list a caller can cast back and change: the findings of a read message are its own.</remarks>
sealed class FixReadFindings : IReadOnlyList<FixFinding>
{
	FixFinding[] _items = new FixFinding[4];
	int          _count;

	public int Count => _count;

	public FixFinding this[int index] => (uint)index < (uint)_count ? _items[index] : throw new ArgumentOutOfRangeException(nameof(index));

	public void Add(FixFinding finding)
	{
		if (_count == _items.Length)
			Array.Resize(ref _items, _count * 2);

		_items[_count++] = finding;
	}

	public IEnumerator<FixFinding> GetEnumerator()
	{
		for (var i = 0; i < _count; i++)
			yield return _items[i];
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
