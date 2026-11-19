using System;
using System.Globalization;
using System.Threading;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>Thrown where a program does what the tree's own rules say it never does.</summary>
sealed class IrContractViolation(string message) : Exception(message);

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

	public (int Way, int Decision) Enter(int last)
	{
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

	public IrWays Ways { get; private set; } = new();

	/// <summary>How many statements ran: what a replay costs, counted and not timed.</summary>
	public long Steps { get; private set; }

	public IrOutcome Parse(string entry, string input)
	{
		var function = program.Entries[entry];

		_text      = input;
		_p         = 0;
		_quiet     = 0;
		_unbuilt   = 0;
		_refusedAt = -1;
		_refused.Clear();
		Ways  = new IrWays();
		Steps = 0;

		return Run(function, out var value)
			? new IrOutcome(true, _p, value, [])
			: new IrOutcome(false, _refusedAt, null, [.. _refused.Select(index => program.Expected[index])]);
	}

	/// <summary>The same on a stack large enough for an input nested a hundred thousand deep.</summary>
	public IrOutcome ParseDeep(string entry, string input)
	{
		IrOutcome? outcome = null;
		Exception? thrown  = null;

		var thread = new Thread(() =>
		{
			try
			{
				outcome = Parse(entry, input);
			}
			catch (Exception exception)
			{
				thrown = exception;
			}
		}, 1 << 30);

		thread.Start();
		thread.Join();

		return thrown is null ? outcome! : throw new InvalidOperationException("The reading threw.", thrown);
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
		if (_quiet > 0)
			return;

		if (at > _refusedAt)
		{
			_refusedAt = at;
			_refused.Clear();
		}

		if (at == _refusedAt)
			_refused.Add(expected);
	}

	bool Exec(IrStmt stmt, object?[] frame)
	{
		Steps++;

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
				var (way, decision) = choice.Resumable ? Ways.Enter(last) : (-1, 0);

				for (var i = decision; i <= last; i++)
				{
					var kept = Keep(frame, choice.Resets[i]);

					if (Exec(choice.Alternatives[i], frame))
						return true;

					_p = start;
					PutBack(frame, choice.Resets[i], kept);

					if (choice.Resumable && i < last)
						Ways.Next(way, i + 1);
				}

				return false;
			}

			case IrStmt.Repeat repeat:
			{
				var min   = (int)Eval(repeat.Min, frame)!;
				var max   = repeat.Max is null ? int.MaxValue : (int)Eval(repeat.Max, frame)!;
				var turns = 0;

				while (turns < max)
				{
					var start = _p;
					var way   = -1;

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
							Ways.Next(way, 1);

						break;
					}

					turns++;

					if (_p == start)
						break;
				}

				return turns >= min;
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
					read = Sealed(look.Body, frame, look.Resets, kept);
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

				return Sealed(commit.Body, frame, commit.Resets, kept);
			}

			case IrStmt.Replay replay:
			{
				var start   = _p;
				var segment = Ways.Cursor;

				while (true)
				{
					_p = start;

					if (Exec(replay.Body, frame))
						return true;

					if (!(Ways.Cursor > segment && Ways.Retry(segment)))
						return false;
				}
			}

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
	bool Sealed(IrStmt body, object?[] frame, IReadOnlyList<IrSlot> resets, object?[] kept)
	{
		var start  = _p;
		var cursor = Ways.Cursor;
		var count  = Ways.Count;

		Ways.Cursor = count;

		bool read;

		while (true)
		{
			_p = start;

			if (read = Exec(body, frame))
				break;

			PutBack(frame, resets, kept);

			if (!(Ways.Count > count && Ways.Retry(count)))
				break;
		}

		Ways.Count  = count;
		Ways.Cursor = cursor;

		return read;
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
