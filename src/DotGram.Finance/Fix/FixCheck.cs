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
/// MessageEncoding — read the fields in wire order and are not in the walk. Their findings ask the
/// message which entries the reader put the field in (<see cref="IFixCarrier"/>), from the groups the
/// message has, so the path is the same whatever checks a context runs, and the finding is whole
/// when it is added.
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
		_shared   = null;
		_spare    = this;
	}

	/// <summary>Adds one finding, said in the groups and entries the check is in now.</summary>
	public void Add(FixRule rule, int tag, int position, FixField? field)
	{
		_findings.Add(FixFinding.Shared(rule, tag, position, field, Path()));
	}

	/// <summary>
	/// Adds one finding of the whole message about a field, said in the entries the reader put
	/// <paramref name="at"/> in: asked of the message's own groups, by the field's tag and position, so it
	/// does not depend on which checks a context runs, and is the finding's whole path when it is added.
	/// </summary>
	public void AddAt(FixRule rule, int tag, FixField field, IFixCarrier message, FixField at)
	{
		FixGroupStep[]? steps = null;

		var depth   = 0;
		var carrier = message;

		while (carrier.Holder(at.Tag, at.Position, out var step) is { } entry)
		{
			if (steps is null)
				steps = new FixGroupStep[4];
			else if (depth == steps.Length)
				Array.Resize(ref steps, depth * 2);

			steps[depth++] = step;
			carrier        = entry;
		}

		if (steps is not null && depth < steps.Length)
			Array.Resize(ref steps, depth);

		_findings.Add(FixFinding.At(rule, tag, field.Position, field, steps));
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

/// <summary>What carries repeating groups — a message or an entry — as a finding of the whole message asks it.</summary>
interface IFixCarrier
{
	/// <summary>
	/// The entry of one of its groups the reader put a field of this tag and position in, and the step to it;
	/// null where the tag is not one of its groups'.
	/// </summary>
	/// <remarks>
	/// The reader puts a field of a group's tag in the entry opened last before it, so that entry is the one
	/// whose first field is the last before the position, duplicates the entry did not keep included.
	/// </remarks>
	IFixEntry? Holder(int tag, int position, out FixGroupStep step);
}

/// <summary>An entry of a repeating group, as a finding of the whole message asks it.</summary>
interface IFixEntry : IFixCarrier
{
	/// <summary>The field the entry opened with.</summary>
	FixField Opener { get; }

	/// <summary>Whether a field of this tag goes in an entry of this group: its own, or one of a group inside it.</summary>
	bool Has(int tag);
}

/// <summary>The search a carrier's <see cref="IFixCarrier.Holder"/> makes in one of its groups.</summary>
static class FixEntries
{
	/// <summary>The entry of this group that holds a field of the tag at the position, by the entries' first fields, or null.</summary>
	internal static IFixEntry? Holding<TEntry>(List<TEntry>? entries, int counter, int tag, int position, out FixGroupStep step)
		where TEntry : class, IFixEntry
	{
		step = default;

		if (entries is null || entries.Count == 0 || !entries[0].Has(tag) || entries[0].Opener.Position > position)
			return null;

		// The last entry whose first field is at or before the position: the entries are in wire order.
		var low  = 0;
		var high = entries.Count - 1;

		while (low < high)
		{
			var middle = (low + high + 1) / 2;

			if (entries[middle].Opener.Position <= position)
				low = middle;
			else
				high = middle - 1;
		}

		step = new FixGroupStep(counter, low);

		return entries[low];
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
