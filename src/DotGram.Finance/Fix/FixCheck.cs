using System;
using System.Collections;
using System.Collections.ObjectModel;

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
/// set by the walk over a group's entries. The path a finding carries is made when the first
/// finding of an entry is, and shared by the findings that follow it there.
/// </para>
/// <para>
/// The checks of the whole message — a length not followed by its data, an encoded field without
/// MessageEncoding — read the fields in wire order, before the walk, and do not know which entry a
/// field is in. Such a finding is kept as waiting, and the walk, which knows, gives it the path of
/// the innermost entry that holds its field. Nothing waits on a valid message, and the walk then
/// asks one flag an entry.
/// </para>
/// </remarks>
sealed class FixCheck
{
	// The run a thread lends to its next Validate, or null while it is lent.
	[ThreadStatic] static FixCheck? _spare;

	// What a run without a caller's collection fills, kept for the next; dropped when one message made
	// it larger than a few hundred findings, so that one bad message does not hold its memory for good.
	List<FixFinding>                  _buffer = [];
	ICollection<FixFinding>           _findings;
	FixGroupStep[]                    _steps  = new FixGroupStep[8];
	int                               _depth;
	ReadOnlyCollection<FixGroupStep>? _shared;

	// The findings of the whole message that wait for the walk to say which entry their field is in.
	(FixFinding Finding, FixField At)[]? _waiting;
	int                                  _waitingCount;

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

		if (_waiting is not null)
			Array.Clear(_waiting, 0, _waitingCount);

		_findings     = _buffer;
		_depth        = 0;
		_shared       = null;
		_waitingCount = 0;
		_spare        = this;
	}

	/// <summary>Adds one finding, said in the groups and entries the check is in now.</summary>
	public void Add(FixRule rule, int tag, int position, FixField? field)
	{
		_findings.Add(FixFinding.Shared(rule, tag, position, field, Path()));
	}

	/// <summary>
	/// Adds one finding of the whole message, about a field, whose path is the entry that holds <paramref name="at"/>:
	/// the walk, which knows the entries, says it.
	/// </summary>
	public void AddAt(FixRule rule, int tag, FixField field, FixField at)
	{
		var finding = FixFinding.Shared(rule, tag, field.Position, field, null);

		if (_waiting is null)
			_waiting = new (FixFinding, FixField)[4];
		else if (_waitingCount == _waiting.Length)
			Array.Resize(ref _waiting, _waitingCount * 2);

		_waiting[_waitingCount++] = (finding, at);
		_findings.Add(finding);
	}

	/// <summary>Whether a finding of the whole message waits for the walk to say where its field is.</summary>
	public bool Waiting => _waitingCount > 0;

	/// <summary>Gives every waiting finding whose field the entry the check is at holds the path to it; a deeper entry, asked later, says it again.</summary>
	public void Locate(IFixEntry entry)
	{
		for (var i = 0; i < _waitingCount; i++)
			if (entry.Holds(_waiting![i].At))
				_waiting[i].Finding.Within(Path()!);
	}

	// The path of the entry the check is at, made once an entry and shared; null outside every group.
	ReadOnlyCollection<FixGroupStep>? Path()
	{
		if (_depth == 0)
			return null;

		if (_shared is null)
		{
			var steps = new FixGroupStep[_depth];

			Array.Copy(_steps, steps, _depth);

			_shared = new ReadOnlyCollection<FixGroupStep>(steps);
		}

		return _shared;
	}

	/// <summary>Enters the group a counter counts, before its first entry.</summary>
	public void Enter(int counter)
	{
		if (_depth == _steps.Length)
			Array.Resize(ref _steps, _depth * 2);

		_steps[_depth++] = new FixGroupStep(counter, 0);
		_shared          = null;
	}

	/// <summary>Moves to an entry of the group entered last.</summary>
	public void At(int index)
	{
		_steps[_depth - 1] = new FixGroupStep(_steps[_depth - 1].CounterTag, index);
		_shared            = null;
	}

	/// <summary>Leaves the group entered last.</summary>
	public void Leave()
	{
		_depth--;
		_shared = null;
	}
}

/// <summary>An entry of a repeating group, as the check asks it: whether a field is one of its own.</summary>
/// <remarks>Its own fields only, the blocks it carries included; an entry of a group inside it answers for its own.</remarks>
interface IFixEntry
{
	/// <summary>Whether the field is one this entry holds.</summary>
	bool Holds(FixField field);
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
