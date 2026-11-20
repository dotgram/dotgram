using System;
using System.Globalization;
using System.Threading;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>Thrown where a program does what the tree's own rules say it never does.</summary>
sealed class IrContractViolation(string message) : Exception(message);

/// <summary>Thrown where a reading ran more statements than it was given: what stops a reading that would not end.</summary>
sealed class IrStepsExceeded(long steps) : Exception("The reading ran more than " + steps.ToString(CultureInfo.InvariantCulture) + " statements.");

/// <summary>The host's side of a program: its fragments, called by number.</summary>
interface IIrHost
{
	object? Construct(IrFragment fragment, object?[] arguments);

	bool Guard(IrFragment fragment, object?[] arguments);
}

/// <summary>
/// The host every reading is compared under: a construction builds the text that says which one
/// it was and over what (<see cref="Twin.Canon"/>), and every call is written down as it runs.
/// </summary>
/// <param name="guards">What each guard answers; one with no answer here answers yes.</param>
/// <param name="throwOn">A construction, spelled as it is written down, that throws instead of building.</param>
sealed class RecordingHost(Func<IrFragment, object?[], bool>? guards = null, string? throwOn = null) : IIrHost
{
	public List<string> Log { get; } = [];

	public object? Construct(IrFragment fragment, object?[] arguments)
	{
		var call = fragment.Name + "(" + string.Join(",", arguments.Select(Twin.Canon)) + ")";

		Log.Add(call);

		if (call == throwOn)
			throw new TwinException(call);

		return new V(call);
	}

	public bool Guard(IrFragment fragment, object?[] arguments)
	{
		var answer = guards?.Invoke(fragment, arguments) ?? true;

		Log.Add(Twin.GuardCall(fragment.Id, arguments.Select(Twin.Canon)) + "=" + (answer ? "true" : "false"));

		return answer;
	}
}

/// <summary>
/// The tape the decisions of a reading are written on, which is what lets a reading be run again
/// and come to the same place.
/// </summary>
/// <remarks>
/// An entry is a decision and the last value it can take. Reading runs with a cursor: an entry
/// under the cursor is one an earlier run wrote, and is read back; past the end a new one is
/// opened. To ask for another reading is to move the last entry that can still move and to drop
/// what was written after it.
/// </remarks>
sealed class IrWays
{
	int[] _items = new int[32];

	public int Count  { get; set; }
	public int Cursor { get; set; }

	/// <summary>The most entries the tape held at once.</summary>
	public int Most { get; private set; }

	/// <summary>How many times a reading was asked for again.</summary>
	public int Retries { get; private set; }

	/// <summary>How many decisions were opened or read back.</summary>
	public long Entered { get; private set; }

	public (int Way, int Decision) Enter(int last)
	{
		Entered++;

		if (Cursor < Count)
		{
			var replayed = Cursor++;

			return (replayed, _items[replayed * 2]);
		}

		if (Count * 2 + 2 > _items.Length)
			Array.Resize(ref _items, _items.Length * 2);

		_items[Count * 2]     = 0;
		_items[Count * 2 + 1] = last;
		Count++;
		Cursor = Count;
		Most   = Math.Max(Most, Count);

		return (Count - 1, 0);
	}

	public void Next(int way, int value)
	{
		_items[way * 2] = value;
		Count           = way + 1;
		Cursor          = way + 1;
	}

	public bool Retry(int segment)
	{
		for (var way = Cursor - 1; way >= segment; way--)
		{
			if (_items[way * 2] < _items[way * 2 + 1])
			{
				_items[way * 2]++;
				Count  = way + 1;
				Cursor = segment;
				Retries++;

				return true;
			}
		}

		return false;
	}
}

/// <summary>What a reading of the whole input came to.</summary>
/// <param name="Position">Where the reading was refused furthest; the input's length where it was accepted.</param>
/// <param name="Expected">What was expected at that position, by name, in order.</param>
sealed record IrOutcome(bool Accepted, int Position, object? Value, IReadOnlyList<string> Expected)
{
	/// <summary>
	/// What was expected at that position in the order the reading said it, kept the way the
	/// generated C# parser keeps it: the first set, then every set that tied with it.
	/// </summary>
	public IReadOnlyList<string> Said { get; init; } = [];

	/// <summary>The verdict, the position and the value in one line, the way every reading is compared.</summary>
	public string Line => (Accepted ? "A " : "R ") + Position.ToString(CultureInfo.InvariantCulture) + (Accepted ? " " + Twin.Canon(Value) : "");
}

/// <summary>
/// The reference reading of an <see cref="IrProgram"/>: each statement does what its
/// documentation says and nothing cleverer, and what the tree promises never to do is checked.
/// </summary>
sealed class IrEvaluator(IrProgram program, IIrHost host)
{
	static readonly object Unset = new();

	string _text    = "";
	int    _p;
	int    _quiet;
	int    _unbuilt;
	int    _refusedAt;

	readonly SortedSet<int> _refused = [];

	// The same refusal as the generated C# parser keeps it (Refuse_DotGram, Tie_DotGram): the
	// sets in the order they were said, the first apart from those that tied with it.
	readonly List<int> _said = [];

	// A reading that records nothing but how far it got, and one that builds nothing: the two
	// readings a refused input is given where the first is made quietly.
	bool _silent;
	int  _unasked;

	public IrWays Ways { get; private set; } = new();

	/// <summary>
	/// Whether a replay goes straight to where the reading before it stood, rather than reading
	/// again everything before the decision it was asked to change: each statement
	/// <see cref="ResumptionSites"/> keeps opens a decision of its own that can never be moved —
	/// its sentinel, which names this entry of it on the tape — and a replay that reads a sentinel
	/// back takes what was kept of it: where a repetition's last kept turn began, or where the
	/// statement ended and what it left in the slots it writes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What a reading keeps it keeps only while it reads decisions back (the cursor short of the
	/// tape's end): the first reading of anything keeps nothing, so a reading that is accepted
	/// pays a sentinel at each kept statement and nothing a turn. The first replay to pass a
	/// statement keeps it; the replays after go straight past.
	/// </para>
	/// <para>
	/// A record is good while every decision read before the place it names is still on the
	/// tape: its key is the cursor at that place (the start of a turn, the end of a statement),
	/// and wherever the tape is cut below a key the record goes. Records are kept on one stack in
	/// the order of their keys, which is what lets a cut pop them from the top; a record pushed
	/// below the top is a broken promise and stops the reading (<see cref="IrContractViolation"/>).
	/// </para>
	/// </remarks>
	public bool Resumes { get; init; }

	/// <summary>
	/// Wrong on purpose: a sentinel takes the latest record of any owner kept past it, as a store
	/// keyed by the position on the tape alone would, rather than its own; and nothing checks the
	/// owner. What the test of the owner check runs to show that the check is what holds.
	/// </summary>
	public bool AnyOwner { get; init; }

	/// <summary>
	/// Whether the reading builds throughout, where the plan says a loop that asks for another
	/// reading stops building after its second give-back (<see cref="IrStmt.Replay.Defers"/>):
	/// what a reading is held to where the reading it is compared with does not defer.
	/// </summary>
	public bool BuildsThroughout { get; init; }

	/// <summary>How many times a replay went straight to a turn of a repetition.</summary>
	public long Jumps { get; private set; }

	/// <summary>How many times a replay went straight past a statement it had read before.</summary>
	public long Skips { get; private set; }

	/// <summary>How many sentinels a reading opened, read back ones apart: what a reading pays for being resumable.</summary>
	public long Sentinels { get; private set; }

	/// <summary>Where a turn of a repetition began, or where a kept statement ended, and what it held there.</summary>
	/// <param name="Owner">The sentinel of the statement's entry.</param>
	/// <param name="Key">The cursor at that place: the record is good while the tape is not cut below it.</param>
	/// <param name="Prev">The owner's record before this one, or -1.</param>
	sealed record Kept(
		bool IsEnd, int Owner, IrStmt Site, int Key, int Prev, int Position, int Turns, bool Read, int Unbuilt, object?[] Held, int[] Counts);

	readonly List<Kept>           _kept  = [];
	readonly Dictionary<int, int> _heads = [];

	ResumptionSites  _sites    = null!;
	ResumptionSites? _measured;

	/// <summary>Which statements keep a record (<see cref="ResumptionSites"/>).</summary>
	public ResumptionCriterion Criterion { get; init; }

	/// <summary>A criterion the reading is measured against: <see cref="WouldHaveJumped"/> counts where it would have left a record out.</summary>
	public ResumptionCriterion? Measured { get; init; }

	/// <summary>How many times a replay went straight to a turn or past a statement that <see cref="Measured"/> would have left without a record.</summary>
	public long WouldHaveJumped { get; private set; }

	void Took(IrStmt site)
	{
		if (_measured is not null && !_measured.Keeps(site))
			WouldHaveJumped++;
	}

	/// <summary>The tape was cut: what was kept of places no longer behind it goes.</summary>
	void Cut()
	{
		if (!Resumes)
			return;

		var count = Ways.Count;

		while (_kept.Count > 0 && _kept[^1].Key >= count)
		{
			var kept = _kept[^1];

			_heads[kept.Owner] = kept.Prev;
			_kept.RemoveAt(_kept.Count - 1);
		}
	}

	/// <summary>The latest record of a sentinel's entry, or null.</summary>
	Kept? Head(int sentinel, IrStmt site)
	{
		if (AnyOwner)
			return _kept.Count > 0 && _kept[^1].Key > sentinel ? _kept[^1] : null;

		if (!_heads.TryGetValue(sentinel, out var head) || head < 0)
			return null;

		var found = _kept[head];

		if (found.Owner != sentinel || !ReferenceEquals(found.Site, site))
			throw new IrContractViolation("A sentinel found a record that is not its own.");

		return found;
	}

	/// <summary>
	/// Whether a record can stand for what the reading would do: one made while building can, in
	/// any reading — a reading that builds nothing reads no slot a record restores, but those its
	/// guards read, which are filled alike; one made unbuilt only in a reading that builds nothing.
	/// </summary>
	bool Usable(Kept kept)
	{
		return kept.Unbuilt == 0 || _unbuilt > 0;
	}

	/// <summary>Whether a record of the key can go on the stack without breaking its order.</summary>
	bool Fits(int key)
	{
		return _kept.Count == 0 || _kept[^1].Key <= key;
	}

	void Record(Kept kept)
	{
		if (_kept.Count > 0 && _kept[^1].Key > kept.Key)
			throw new IrContractViolation("A record was kept below the top of the stack: its key " + kept.Key.ToString(CultureInfo.InvariantCulture) + ", the top's " + _kept[^1].Key.ToString(CultureInfo.InvariantCulture) + ".");

		var prev = _heads.TryGetValue(kept.Owner, out var head) ? head : -1;

		_heads[kept.Owner] = _kept.Count;
		_kept.Add(kept with { Prev = prev });
	}

	/// <summary>What the slots a statement writes hold now: values, and the length of each list with the list itself.</summary>
	static (object?[] Held, int[] Counts) Hold(object?[] frame, IReadOnlyList<IrSlot> slots)
	{
		var held   = new object?[slots.Count];
		var counts = new int[held.Length];

		for (var i = 0; i < held.Length; i++)
		{
			held[i]   = frame[slots[i].Index];
			counts[i] = held[i] is List<object?> items ? items.Count : 0;
		}

		return (held, counts);
	}

	/// <summary>
	/// Wrong on purpose: going straight to a turn or past a statement leaves the slots as the
	/// replay found them rather than as they were kept. What the test of the turns that write and
	/// put back their slots runs to show that the record is what holds them.
	/// </summary>
	public bool IgnoresHeld { get; init; }

	void Restore(object?[] frame, IReadOnlyList<IrSlot> slots, Kept kept)
	{
		if (IgnoresHeld)
			return;

		// As many as both have: a record of another statement's (taken only where the owner check is off on purpose) may hold others.
		for (var i = 0; i < Math.Min(kept.Held.Length, slots.Count); i++)
		{
			if (kept.Held[i] is List<object?> items)
				items.RemoveRange(kept.Counts[i], items.Count - kept.Counts[i]);

			frame[slots[i].Index] = kept.Held[i];
		}
	}

	/// <summary>The slots a kept statement writes: what is restored where a replay goes past it.</summary>
	static IReadOnlyList<IrSlot> Writes(IrStmt stmt)
	{
		return stmt switch
		{
			IrStmt.Repeat repeat => repeat.Resets,
			IrStmt.Commit commit => commit.Resets,
			IrStmt.Look look     => look.Resets,
			IrStmt.Call { Into: { } into } => [into],
			_                    => [],
		};
	}

	/// <summary>
	/// A kept statement other than a repetition: its sentinel, and either what was kept of where
	/// it ended, or the statement read and kept where the reading is a replay.
	/// </summary>
	bool Site(IrStmt stmt, object?[] frame)
	{
		var again = Ways.Cursor < Ways.Count;
		var (sentinel, _) = Ways.Enter(0);
		var writes = Writes(stmt);

		if (!again)
		{
			Sentinels++;
			_heads[sentinel] = -1;
		}
		else if (Head(sentinel, stmt) is { IsEnd: true } found && Usable(found))
		{
			_p          = found.Position;
			Ways.Cursor = found.Key;
			Restore(frame, writes, found);
			Skips++;
			Took(stmt);

			return found.Read;
		}

		var read = ExecInner(stmt, frame);

		// Kept once a reading; again only where what was kept was made unbuilt and this reading builds.
		if (Ways.Cursor < Ways.Count && Head(sentinel, stmt) is var head && (head is null || !Usable(head) && Fits(Ways.Cursor)))
		{
			var (held, counts) = Hold(frame, writes);

			Record(new Kept(true, sentinel, stmt, Ways.Cursor, -1, _p, 0, read, _unbuilt, held, counts));
		}

		return read;
	}

	/// <summary>How many statements ran: what a replay costs, counted and not timed.</summary>
	public long Steps { get; private set; }

	/// <summary>How many statements a reading may run before it is stopped (<see cref="IrStepsExceeded"/>).</summary>
	public long StepBudget { get; init; } = long.MaxValue;

	/// <summary>Reads the whole input as a publication.</summary>
	/// <remarks>
	/// The evaluator is recursive in the tree and in the grammar both, some thirty frames a level
	/// of nesting in the input, so anything but a short input is read on a thread with a stack of
	/// its own, large enough for an input nested a hundred thousand deep.
	/// </remarks>
	public IrOutcome Parse(string entry, string input)
	{
		if (input.Length <= 256)
			return Read(entry, input);

		IrOutcome? outcome = null;

		System.Runtime.ExceptionServices.ExceptionDispatchInfo? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				outcome = Read(entry, input);
			}
			catch (Exception exception)
			{
				thrown = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
			}
		}, 1 << 30);

		thread.Start();
		thread.Join();
		thrown?.Throw();

		return outcome!;
	}

	/// <summary>
	/// Reads the whole input the way a published C# parse does: quietly first, writing down only
	/// how far it got, and where that reading refuses, a second time for what the refusal says,
	/// with nothing built.
	/// </summary>
	/// <returns>The outcome, and what the quiet reading alone came to.</returns>
	public (IrOutcome Outcome, IrOutcome Quiet, long QuietSteps) ParseQuietlyFirst(string entry, string input)
	{
		_silent = true;

		IrOutcome quiet;

		try
		{
			quiet = Parse(entry, input);
		}
		finally
		{
			_silent = false;
		}

		var steps = Steps;

		if (quiet.Accepted)
			return (quiet, quiet, steps);

		_unasked = 1;

		try
		{
			return (Parse(entry, input), quiet, steps);
		}
		finally
		{
			_unasked = 0;
		}
	}

	IrOutcome Read(string entry, string input)
	{
		var function = program.Entries[entry];

		_text      = input;
		_p         = 0;
		_quiet     = 0;
		_unbuilt   = _unasked;
		_refusedAt = -1;
		_refused.Clear();
		_said.Clear();
		_kept.Clear();
		_heads.Clear();
		_sites    = ResumptionSites.Of(program, Criterion);
		_measured = Measured is { } measured ? ResumptionSites.Of(program, measured) : null;

		WouldHaveJumped = 0;
		Ways      = new IrWays();
		Steps     = 0;
		Jumps     = 0;
		Skips     = 0;
		Sentinels = 0;

		return Run(function, out var value)
			? new IrOutcome(true, _p, value, [])
			: new IrOutcome(false, _refusedAt, null, [.. _refused.Select(index => program.Expected[index])])
			{
				Said = [.. _said.Select(index => program.Expected[index])],
			};
	}

	bool Run(IrFunction function, out object? value)
	{
		var frame = new object?[function.SlotCount];

		Array.Fill(frame, Unset);

		var read = Exec(function.Body, frame);

		value = read && function.Result is { } result && frame[result.Index] != Unset ? frame[result.Index] : null;

		return read;
	}

	void Refuse(int at, int expected)
	{
		if (_quiet > 0 || expected < 0)
			return;

		if (_silent)
		{
			if (at > _refusedAt)
				_refusedAt = at;

			return;
		}

		if (at > _refusedAt)
		{
			_refusedAt = at;
			_refused.Clear();
			_said.Clear();
			_said.Add(expected);
		}
		else if (at == _refusedAt && _said[0] != expected && !(_said.Count > 1 && _said[^1] == expected))
		{
			// A guard's set is said once however often it is asked; any other may come again
			// after something else has.
			if (!(program.Guarding.Contains(expected) && _said.Contains(expected)))
				_said.Add(expected);
		}

		if (at == _refusedAt)
			_refused.Add(expected);
	}

	bool Exec(IrStmt stmt, object?[] frame)
	{
		if (++Steps > StepBudget)
			throw new IrStepsExceeded(StepBudget);

		if (Resumes && stmt is not IrStmt.Repeat && _sites.Keeps(stmt))
			return Site(stmt, frame);

		return ExecInner(stmt, frame);
	}

	bool ExecInner(IrStmt stmt, object?[] frame)
	{
		switch (stmt)
		{
			case IrStmt.Block block:
				foreach (var item in block.Items)
					if (!Exec(item, frame))
						return false;

				return true;

			case IrStmt.Scope scope:
				foreach (var slot in scope.Slots)
					frame[slot.Index] = Unset;

				return Exec(scope.Body, frame);

			case IrStmt.MatchSet set:
				if (_p < _text.Length && Within(set, _text[_p]))
				{
					_p++;

					return true;
				}

				Refuse(_p, set.Expected);

				return false;

			case IrStmt.MatchText literal:
			{
				var agreed = 0;

				while (agreed < literal.Text.Length && _p + agreed < _text.Length && Same(_text[_p + agreed], literal.Text[agreed], literal.IgnoreCase))
					agreed++;

				if (agreed == literal.Text.Length)
				{
					_p += agreed;

					return true;
				}

				Refuse(_p + agreed, literal.Expected);

				return false;
			}

			case IrStmt.MatchSame same:
			{
				var from   = frame[same.From.Index] == Unset ? 0 : (int)frame[same.From.Index]!;
				var length = frame[same.To.Index] == Unset ? 0 : (int)frame[same.To.Index]! - from;

				if (_p + length <= _text.Length && string.CompareOrdinal(_text, from, _text, _p, length) == 0)
				{
					_p += length;

					return true;
				}

				Refuse(_p, same.Expected);

				return false;
			}

			case IrStmt.MatchEnd end:
				if (_p == _text.Length)
					return true;

				Refuse(_p, end.Expected);

				return false;

			case IrStmt.Call call:
			{
				if (!Run(call.Target, out var value))
					return false;

				// Inside a look nothing is built, so there is nothing to take.
				if (call.Into is { } into && _unbuilt == 0)
				{
					if (call.Appends)
						Add(frame, into, value);
					else
						Set(frame, into, value);
				}

				return true;
			}

			case IrStmt.Choice choice:
			{
				var start = _p;
				var last  = choice.Alternatives.Count - 1;

				if (choice.Dispatch is { } dispatch)
				{
					var chosen = -1;

					for (var i = 0; i <= last && chosen < 0; i++)
						if (_p < _text.Length && Within(dispatch.Begins[i], _text[_p]))
							chosen = i;

					if (chosen < 0)
					{
						Refuse(_p, dispatch.Otherwise);

						return false;
					}

					var held = Keep(frame, choice.Resets[chosen]);

					if (Exec(choice.Alternatives[chosen], frame))
						return true;

					_p = start;
					PutBack(frame, choice.Resets[chosen], held);

					return false;
				}

				var (way, decision) = choice.Resumable ? Ways.Enter(last) : (-1, 0);

				for (var i = decision; i <= last; i++)
				{
					var kept = Keep(frame, choice.Resets[i]);

					if (Exec(choice.Alternatives[i], frame))
						return true;

					_p = start;
					PutBack(frame, choice.Resets[i], kept);

					if (choice.Resumable && i < last)
					{
						Ways.Next(way, i + 1);
						Cut();
					}
				}

				return false;
			}

			case IrStmt.Repeat repeat:
			{
				var min   = (int)Eval(repeat.Min, frame)!;
				var max   = repeat.Max is null ? int.MaxValue : (int)Eval(repeat.Max, frame)!;
				var turns = 0;
				var owner = -1;

				if (Resumes && _sites.Keeps(repeat))
				{
					var again = Ways.Cursor < Ways.Count;

					(owner, _) = Ways.Enter(0);

					if (!again)
					{
						Sentinels++;
						_heads[owner] = -1;
					}
					else if (Head(owner, repeat) is { } found && Usable(found))
					{
						// Every decision read before the place kept is still on the tape, so the
						// reading goes on from there: past the whole repetition, or from a turn.
						_p          = found.Position;
						turns       = found.Turns;
						Ways.Cursor = found.Key;
						Restore(frame, repeat.Resets, found);

						Took(repeat);

						if (found.IsEnd)
						{
							Skips++;

							return found.Read;
						}

						Jumps++;
					}
				}

				while (turns < max)
				{
					var start = _p;
					var way   = -1;

					if (repeat.Door is { } door && turns >= min && !(_p < _text.Length && Within(door, _text[_p])))
						break;

					// A replay keeps where each turn begins, once: the turn a replay went straight to is kept already.
					if (owner >= 0 && Ways.Cursor < Ways.Count)
					{
						var latest = Head(owner, repeat);

						if (latest is null || !latest.IsEnd && latest.Turns < turns || !Usable(latest) && Fits(Ways.Cursor))
						{
							var (held, counts) = Hold(frame, repeat.Resets);

							Record(new Kept(false, owner, repeat, Ways.Cursor, -1, start, turns, false, _unbuilt, held, counts));
						}
					}

					if (repeat.Resumable && turns >= min)
					{
						(way, var decision) = Ways.Enter(1);

						if (decision == 1)
							break;
					}

					var kept = Keep(frame, repeat.Resets);

					if (!Exec(repeat.Body, frame))
					{
						_p = start;
						PutBack(frame, repeat.Resets, kept);

						if (way >= 0)
						{
							Ways.Next(way, 1);
							Cut();
						}

						break;
					}

					turns++;

					if (_p == start)
						break;
				}

				var read = turns >= min;

				if (owner >= 0 && Ways.Cursor < Ways.Count && Head(owner, repeat) is var last && (last is not { IsEnd: true } || !Usable(last) && Fits(Ways.Cursor)))
				{
					var (held, counts) = Hold(frame, repeat.Resets);

					Record(new Kept(true, owner, repeat, Ways.Cursor, -1, _p, turns, read, _unbuilt, held, counts));
				}

				return read;
			}

			case IrStmt.Look look:
			{
				var start = _p;
				var kept  = Keep(frame, look.Resets);

				_quiet++;
				_unbuilt++;

				bool read;

				try
				{
					read = Sealed(look.Body, frame, look.Resets, kept, defers: false);
				}
				finally
				{
					_quiet--;
					_unbuilt--;
				}

				_p = start;
				PutBack(frame, look.Resets, kept);

				if (read == look.IsPositive)
					return true;

				Refuse(_p, look.Expected);

				return false;
			}

			case IrStmt.Commit commit:
			{
				var kept = Keep(frame, commit.Resets);

				return Sealed(commit.Body, frame, commit.Resets, kept, commit.Defers);
			}

			case IrStmt.Replay replay:
			{
				var start    = _p;
				var segment  = Ways.Cursor;
				var deferral = new Deferral();

				while (true)
				{
					_p = start;

					if (Exec(replay.Body, frame))
					{
						if (Stood(ref deferral, segment))
							continue;

						return true;
					}

					if (!(Ways.Cursor > segment && Ways.Retry(segment)))
					{
						Undeferred(deferral);

						return false;
					}

					Cut();

					if (replay.Defers && !BuildsThroughout)
						Retried(ref deferral);
				}
			}

			// A reading that builds nothing takes no text and no list where only a construction would
			// read it: a guard decides, and is handed what it reads in any reading.
			case IrStmt.Assign { Value: IrExpr.Text or IrExpr.Items } assign when _unbuilt > 0 && !BuildsThroughout && !_sites.ReadByGuards.Contains(assign.Slot):
				return true;

			case IrStmt.Append { Value: IrExpr.Text } append when _unbuilt > 0 && !BuildsThroughout && !_sites.ReadByGuards.Contains(append.List):
				return true;

			case IrStmt.Assign assign:
				Set(frame, assign.Slot, Eval(assign.Value, frame));

				return true;

			case IrStmt.Append append:
				Add(frame, append.List, Eval(append.Value, frame));

				return true;

			case IrStmt.Guard guard:
				if (host.Guard(guard.Fragment, [.. guard.Arguments.Select(argument => Eval(argument, frame))]))
					return true;

				Refuse(_p, guard.Expected);

				return false;

			case IrStmt.Construct build:
				if (_unbuilt == 0)
					Set(frame, build.Into, host.Construct(build.Fragment, [.. build.Arguments.Select(argument => Eval(argument, frame))]));

				return true;

			default:
				throw new NotSupportedException("The sketch does not read " + stmt + ".");
		}
	}

	/// <summary>
	/// A body whose decisions are nobody's but its own: they are written past everything on the
	/// tape, the body is run again over them until it reads or they run out, and then they are gone.
	/// </summary>
	bool Sealed(IrStmt body, object?[] frame, IReadOnlyList<IrSlot> resets, object?[] kept, bool defers)
	{
		var start    = _p;
		var cursor   = Ways.Cursor;
		var count    = Ways.Count;
		var deferral = new Deferral();

		Ways.Cursor = count;

		bool read;

		while (true)
		{
			_p = start;

			if (read = Exec(body, frame))
			{
				if (!Stood(ref deferral, count))
					break;

				PutBack(frame, resets, kept);

				continue;
			}

			PutBack(frame, resets, kept);

			if (!(Ways.Count > count && Ways.Retry(count)))
			{
				Undeferred(deferral);

				break;
			}

			Cut();

			if (defers && !BuildsThroughout)
				Retried(ref deferral);
		}

		Ways.Count  = count;
		Ways.Cursor = cursor;
		Cut();

		return read;
	}

	/// <summary>
	/// Where a loop that asks for another reading stands since it gave a way back, as the generated
	/// parser keeps it: 0 building, as every loop begins; 1 building still, one way given back; 2
	/// reading unbuilt, the count raised by this loop; 3 reading once more, built, the attempt that
	/// stood — with what was refused kept from before that reading, to be put back after it.
	/// </summary>
	struct Deferral
	{
		public int State;

		public int RefusedAt;

		public int[]? Refused;
		public int[]? Said;
	}

	/// <summary>A way given back: the first is read building still; the second, where the reading was building, stops building.</summary>
	void Retried(ref Deferral deferral)
	{
		if (deferral.State == 1)
		{
			_unbuilt       = 1;
			deferral.State = 2;
		}
		else if (deferral.State == 0 && _unbuilt == 0)
			deferral.State = 1;
	}

	/// <summary>
	/// An attempt stood: where it was read unbuilt, it is to be read once more from the loop's
	/// start, building, over the decisions it took (true); where that reading has stood, what it
	/// said of a refusal is taken back, for the attempt it read again had said it already.
	/// </summary>
	bool Stood(ref Deferral deferral, int segment)
	{
		if (deferral.State == 2)
		{
			deferral.RefusedAt = _refusedAt;
			deferral.Refused   = [.. _refused];
			deferral.Said      = [.. _said];
			deferral.State     = 3;
			_unbuilt           = 0;
			Ways.Cursor        = segment;

			return true;
		}

		if (deferral.State == 3)
			Replayed(deferral);

		return false;
	}

	/// <summary>A loop that ends without an answer puts back what it raised: the count, or what was refused before the reading once more.</summary>
	void Undeferred(Deferral deferral)
	{
		if (deferral.State == 2)
			_unbuilt = 0;
		else if (deferral.State == 3)
			Replayed(deferral);
	}

	void Replayed(Deferral deferral)
	{
		_refusedAt = deferral.RefusedAt;
		_refused.Clear();
		_refused.UnionWith(deferral.Refused!);
		_said.Clear();
		_said.AddRange(deferral.Said!);
	}

	static bool Within(IrStmt.MatchSet set, char unit)
	{
		var inside = false;

		foreach (var range in set.Ranges)
		{
			if (unit >= range.From && unit <= range.To)
			{
				inside = true;

				break;
			}
		}

		return inside != set.IsNegated;
	}

	static bool Within(IReadOnlyList<CharRange> ranges, char unit)
	{
		foreach (var range in ranges)
			if (unit >= range.From && unit <= range.To)
				return true;

		return false;
	}

	static bool Same(char one, char other, bool ignoreCase)
	{
		return one == other || ignoreCase && char.ToUpperInvariant(one) == char.ToUpperInvariant(other);
	}

	static void Set(object?[] frame, IrSlot slot, object? value)
	{
		// The promise of a slot that is not rebound: one assignment in one lifetime.
		if (slot.Kind != IrSlotKind.Rebound && frame[slot.Index] != Unset)
			throw new IrContractViolation(slot + " is assigned a second time in one lifetime.");

		frame[slot.Index] = value;
	}

	static void Add(object?[] frame, IrSlot list, object? value)
	{
		if (frame[list.Index] is not List<object?> items)
			frame[list.Index] = items = [];

		items.Add(value);
	}

	/// <summary>What the slots an alternative may write hold before it runs.</summary>
	static object?[] Keep(object?[] frame, IReadOnlyList<IrSlot> slots)
	{
		if (slots.Count == 0)
			return [];

		var kept = new object?[slots.Count];

		for (var i = 0; i < kept.Length; i++)
			kept[i] = frame[slots[i].Index] is List<object?> items ? items.Count : frame[slots[i].Index];

		return kept;
	}

	static void PutBack(object?[] frame, IReadOnlyList<IrSlot> slots, object?[] kept)
	{
		for (var i = 0; i < kept.Length; i++)
		{
			if (frame[slots[i].Index] is List<object?> items && kept[i] is int count)
				items.RemoveRange(count, items.Count - count);
			else
				frame[slots[i].Index] = kept[i];
		}
	}

	object? Eval(IrExpr expr, object?[] frame)
	{
		switch (expr)
		{
			case IrExpr.Position:
				return _p;

			case IrExpr.Slot slot:
				return frame[slot.Of.Index] == Unset ? null : frame[slot.Of.Index];

			case IrExpr.Text text:
				var from = (int)frame[text.From.Index]!;

				return _text.Substring(from, _p - from);

			case IrExpr.Items items:
				return frame[items.Of.Index] is List<object?> list ? list.ToArray() : Array.Empty<object?>();

			case IrExpr.Number number:
				return number.Value;

			case IrExpr.Length length:
				return frame[length.From.Index] == Unset || frame[length.To.Index] == Unset
					? 0
					: (int)frame[length.To.Index]! - (int)frame[length.From.Index]!;

			default:
				throw new ArgumentOutOfRangeException(nameof(expr));
		}
	}
}
