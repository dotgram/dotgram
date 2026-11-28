using System;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>Which of the statements a replay can reach keep a record.</summary>
enum ResumptionCriterion
{
	/// <summary>
	/// A statement keeps where it decides itself, or where something after it, before its replay
	/// region ends, can decide — for only a decision moved at or after a statement makes a replay
	/// read it again. The set before <see cref="FailsBack"/> narrowed it, kept to be measured against.
	/// </summary>
	Decided,

	/// <summary>Every repetition and every unbounded statement a replay can reach.</summary>
	Wide,

	/// <summary>
	/// <see cref="Decided"/> for repetitions only: braces, looks, recursions and texts read by
	/// reference are read again by every replay. What the cost of keeping those is measured against.
	/// </summary>
	Repetitions,

	/// <summary>
	/// A repetition only where a decision can open inside its turns and outlive them; other
	/// statements as <see cref="Wide"/>. Measured, not used: a repetition with no decision of its
	/// own is read again by every replay of a decision after it.
	/// </summary>
	Narrowed,

	/// <summary>
	/// The one in force: <see cref="Decided"/>, and moreover only where something can fail back to a decision that
	/// keeps the statement (itself, or the first after it) before braces, a look or the end of the
	/// input seal it: after that decision, or inside it after a decision of its own that it does
	/// not take back itself. Only such a failure makes a replay read the statement again.
	/// </summary>
	FailsBack,

	/// <summary>A mutation of <see cref="FailsBack"/>, for the tests: something can always fail back. Keeps what <see cref="Decided"/> keeps.</summary>
	FailsBackAlways,

	/// <summary>A mutation of <see cref="FailsBack"/>, for the tests: nothing can ever fail. Leaves out statements a replay reads again.</summary>
	FailsBackNever,

	/// <summary>
	/// A mutation of <see cref="FailsBack"/>, for the tests: only what comes after the deciding
	/// statement is asked, not what fails inside it after a decision of its own.
	/// </summary>
	FailsBackOutsideOnly,
}

/// <summary>
/// Which statements of a program a resuming reading keeps a record of, and why: the places where
/// a replay would otherwise read again, in full, work it already did before the decision it was
/// asked to change.
/// </summary>
/// <remarks>
/// <para>
/// A replay runs its region again from the start, and everything before the decision it moved is
/// read over the same text, the same decisions and the same slots as the reading before it. Two
/// kinds of statement keep what that reading came to, behind a decision of their own that can
/// never be moved and names the statement's entry on the tape (its sentinel): every repetition of
/// unbounded turns (where each of its turns began, and where it ended), and every other statement
/// whose own work is unbounded (where it ended). A replay that reads such a sentinel back takes
/// what was kept and goes on from there.
/// </para>
/// <para>
/// What is unbounded is computed, not listed: a call into a function that can reach itself; a
/// text read again by reference; a guard handed a text read over unbounded input, or a list; and
/// a look or braces whose body holds any of these or any repetition — because everything decided
/// inside braces is taken off the tape where they end, and a replay reads them again from nothing.
/// Whatever is unbounded inside anything else is kept where it stands, so that a call into a
/// function that cannot reach itself, or a repetition of a bounded number of turns, costs a replay
/// no more than the grammar is long, and needs nothing of its own.
/// </para>
/// <para>
/// Only a statement a replay can reach is kept: one inside a replay region, or inside a function
/// called from one, or inside braces or a look whose body decides — there the braces' own reading
/// again is the replay. Inside braces or a look that decide nothing, nothing kept would outlive them.
/// </para>
/// </remarks>
sealed class ResumptionSites
{
	static readonly ConditionalWeakTable<IrProgram, Dictionary<ResumptionCriterion, ResumptionSites>> _known = [];

	readonly ResumptionCriterion _criterion;

	// The statement each statement of a function's body stands in, and the calls of each function.
	readonly Dictionary<IrStmt, IrStmt> _parents = new(ReferenceEqualityComparer.Instance);
	readonly Dictionary<IrFunction, List<IrStmt.Call>> _callers = [];
	readonly Dictionary<IrStmt, IrFunction> _owners = new(ReferenceEqualityComparer.Instance);
	readonly Dictionary<IrFunction, bool> _followed = [];

	/// <summary>
	/// Measured: the kept statements that may be entered again, while an earlier entry of theirs is
	/// still being read, without the position having moved — through calls made before any input is
	/// read, back to their own function, and from its start to them with nothing read on the way.
	/// An over-approximation.
	/// </summary>
	public HashSet<IrStmt> ReentersEmpty { get; } = new(ReferenceEqualityComparer.Instance);


	/// <summary>The functions a reading of the statement may call before it reads anything.</summary>
	void LeftCorner(IrStmt stmt, HashSet<IrFunction> into)
	{
		switch (stmt)
		{
			case IrStmt.Call call:
				if (into.Add(call.Target))
					LeftCorner(call.Target.Body, into);

				return;

			case IrStmt.Block block:
				foreach (var item in block.Items)
				{
					LeftCorner(item, into);

					if (Begins(item) is { First: not null, Empty: false })
						return;
				}

				return;

			default:
				foreach (var child in stmt.Children)
					LeftCorner(child, into);

				return;
		}
	}

	/// <summary>Whether the statement may be the first thing its function reads: everything before it on the way down can read nothing.</summary>
	bool AtStart(IrStmt stmt)
	{
		var at = stmt;

		while (_parents.TryGetValue(at, out var parent))
		{
			if (parent is IrStmt.Block block)
			{
				var index = IndexOf(block.Items, at);

				for (var i = 0; i < index; i++)
					if (Begins(block.Items[i]) is { First: not null, Empty: false })
						return false;
			}

			at = parent;
		}

		return true;
	}

	void FindReentries()
	{
		foreach (var stmt in _sites.Keys)
		{
			var reached = new HashSet<IrFunction>();

			LeftCorner(stmt, reached);

			if (reached.Contains(_owners[stmt]) && AtStart(stmt))
				ReentersEmpty.Add(stmt);
		}
	}

	/// <summary>For every kept statement, the decision that keeps it under <see cref="ResumptionCriterion.Decided"/>, in words.</summary>
	public Dictionary<IrStmt, string> Kept { get; } = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrFunction, bool> _failFollowed = [];
	readonly Dictionary<IrFunction, bool> _keptByCallers = [];

	/// <summary>Every statement a replay can reach that the criterion leaves without a record, and why.</summary>
	public Dictionary<IrStmt, string> Left { get; } = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrStmt, string> _sites = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrFunction, HashSet<IrFunction>> _reach = [];

	readonly Dictionary<IrFunction, string?> _raw      = [];

	// Slots that hold a text read over input of no bound.
	readonly HashSet<IrSlot> _longTexts = [];

	readonly HashSet<(IrFunction, bool)> _visited = [];

	ResumptionSites(IrProgram program, ResumptionCriterion criterion)
	{
		_criterion = criterion;

		foreach (var function in program.Functions)
		{
			_callers.TryAdd(function, []);
			Index(function, function.Body, null);
		}

		FindFollowed();
		FindFailFollowed();
		FindKeptByCallers();

		foreach (var function in program.Functions)
		{
			var reached = new HashSet<IrFunction>();
			var pending = new Stack<IrFunction>(Calls(function.Body));

			while (pending.TryPop(out var next))
				if (reached.Add(next))
					foreach (var further in Calls(next.Body))
						pending.Push(further);

			_reach.Add(function, reached);
		}

		foreach (var function in program.Functions)
			FindLongTexts(function.Body);

		foreach (var function in program.Functions)
			FindGuardArguments(function.Body);

		foreach (var entry in program.Entries.Values)
			Visit(entry, inReplay: false);

		FindReentries();
	}

	/// <summary>
	/// For a kept statement that reads input only from items it can begin with, those items: where
	/// the next item is none of them, the statement reads nothing (or fails at once), its work is
	/// bounded, and no sentinel is opened — the same answer on every replay, which reads the same
	/// item there. Absent where any item can begin it, or the beginning cannot be told.
	/// </summary>
	public Dictionary<IrStmt, IReadOnlyList<CharRange>> Doors { get; } = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrFunction, List<CharRange>?> _firsts = [];

	/// <summary>
	/// The items a statement's reading can begin with, and whether it can read none; null where it
	/// cannot be told (a negated set, a reference, a look, what this sketch does not read).
	/// </summary>
	(List<CharRange>? First, bool Empty) Begins(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.MatchSet { IsNegated: false } set:
				return ([.. set.Ranges], false);

			case IrStmt.MatchText { IgnoreCase: false, Text.Length: > 0 } text:
				return ([new CharRange(text.Text[0], text.Text[0])], false);

			case IrStmt.MatchEnd or IrStmt.Assign or IrStmt.Append or IrStmt.Construct or IrStmt.Guard:
				return ([], true);

			case IrStmt.Call call:
				return (FirstOf(call.Target), Nullable(call.Target.Body));

			case IrStmt.Block block:
			{
				var first = new List<CharRange>();

				foreach (var item in block.Items)
				{
					var (one, empty) = Begins(item);

					if (one is null)
						return (null, false);

					first.AddRange(one);

					if (!empty)
						return (first, false);
				}

				return (first, true);
			}

			case IrStmt.Choice choice:
			{
				var first = new List<CharRange>();
				var empty = false;

				foreach (var alternative in choice.Alternatives)
				{
					var (one, none) = Begins(alternative);

					if (one is null)
						return (null, false);

					first.AddRange(one);
					empty |= none;
				}

				return (first, empty);
			}

			case IrStmt.Repeat repeat:
			{
				var (one, empty) = Begins(repeat.Body);

				return (one, empty || repeat.Min is not IrExpr.Number { Value: > 0 });
			}

			case IrStmt.Scope or IrStmt.Replay or IrStmt.Commit:
				return Begins(stmt.Children.Single());

			default:
				return (null, false);
		}
	}

	bool Nullable(IrStmt stmt)
	{
		return Begins(stmt) is { First: not null, Empty: true };
	}

	List<CharRange>? FirstOf(IrFunction function)
	{
		if (_firsts.TryGetValue(function, out var known))
			return known;

		// A function met again on the way to its own beginning cannot be told.
		_firsts[function] = null;

		return _firsts[function] = Begins(function.Body).First;
	}

	/// <summary>The ranges in order, those that meet or touch made one.</summary>
	static List<CharRange> Merged(List<CharRange> ranges)
	{
		var merged = new List<CharRange>();

		foreach (var range in ranges.OrderBy(static one => one.From))
		{
			if (merged.Count > 0 && range.From <= merged[^1].To + 1)
			{
				if (range.To > merged[^1].To)
					merged[^1] = new CharRange(merged[^1].From, range.To);
			}
			else
				merged.Add(range);
		}

		return merged;
	}

	/// <summary>The slots some guard is handed: a reading that builds nothing still fills them.</summary>
	public HashSet<IrSlot> ReadByGuards { get; } = [];

	/// <summary>What is kept of a program under a criterion, worked out once.</summary>
	public static ResumptionSites Of(IrProgram program, ResumptionCriterion criterion = ResumptionCriterion.FailsBack)
	{
		var known = _known.GetValue(program, static _ => []);

		lock (known)
		{
			if (!known.TryGetValue(criterion, out var sites))
				known.Add(criterion, sites = new ResumptionSites(program, criterion));

			return sites;
		}
	}

	void Index(IrFunction function, IrStmt stmt, IrStmt? parent)
	{
		_owners[stmt] = function;

		if (parent is not null)
			_parents[stmt] = parent;

		if (stmt is IrStmt.Call call)
		{
			_callers.TryAdd(call.Target, []);
			_callers[call.Target].Add(call);
		}

		foreach (var child in stmt.Children)
			Index(function, child, stmt);
	}

	/// <summary>
	/// Whether a decision can open after the statement and before the end of the replay region it
	/// is read in: in what follows it in its sequence, in a later turn of a repetition round it,
	/// and, past the end of its function, after any call of the function. Braces and looks end the
	/// search: what is decided inside them is taken off the tape where they end.
	/// </summary>
	bool DecidesAfter(IrStmt stmt)
	{
		return Within(stmt) switch
		{
			true  => true,
			false => false,
			null  => _followed[_owners[stmt]],
		};
	}

	/// <summary><see cref="DecidesAfter"/> inside the statement's own function: null where the search reaches the function's end.</summary>
	bool? Within(IrStmt stmt)
	{
		var at = stmt;

		while (_parents.TryGetValue(at, out var parent))
		{
			switch (parent)
			{
				case IrStmt.Block block:
				{
					var index = IndexOf(block.Items, at);

					for (var i = index + 1; i < block.Items.Count; i++)
						if (IrLowering.Decides(block.Items[i]))
							return true;

					break;
				}

				case IrStmt.Repeat repeat when IrLowering.Decides(repeat):
					return true;

				case IrStmt.Look or IrStmt.Commit:
					return false;
			}

			at = parent;
		}

		return null;
	}

	static int IndexOf(IReadOnlyList<IrStmt> items, IrStmt item)
	{
		for (var i = 0; i < items.Count; i++)
			if (ReferenceEquals(items[i], item))
				return i;

		return -1;
	}

	/// <summary>
	/// Of every function, whether a decision can open after some call of it before that call's
	/// replay region ends: the least answer that holds through every call, worked out by rounds.
	/// </summary>
	void FindFollowed()
	{
		foreach (var function in _callers.Keys)
			_followed[function] = false;

		var changed = true;

		while (changed)
		{
			changed = false;

			foreach (var (function, calls) in _callers)
			{
				if (_followed[function])
					continue;

				foreach (var call in calls)
				{
					if (Within(call) ?? _followed[_owners[call]])
					{
						_followed[function] = true;
						changed             = true;

						break;
					}
				}
			}
		}
	}

	/// <summary>The first decision after the statement inside its function, the enclosing deciding repetition counted; null where none before its end, and the reason in words.</summary>
	(IrStmt? Decider, bool Sealed, string Where) FirstDecider(IrStmt stmt)
	{
		var at = stmt;

		while (_parents.TryGetValue(at, out var parent))
		{
			switch (parent)
			{
				case IrStmt.Block block:
				{
					var index = IndexOf(block.Items, at);

					for (var i = index + 1; i < block.Items.Count; i++)
						if (IrLowering.Decides(block.Items[i]))
							return (block.Items[i], false, "a later " + Spell(block.Items[i]) + " in " + _owners[stmt].Name);

					break;
				}

				case IrStmt.Repeat repeat when IrLowering.Decides(repeat):
					return (repeat, false, "a later turn of a deciding repetition in " + _owners[stmt].Name);

				case IrStmt.Look or IrStmt.Commit:
					return (null, true, "");
			}

			at = parent;
		}

		return (null, false, "");
	}

	static string Spell(IrStmt stmt)
	{
		return stmt is IrStmt.Call call ? "call of " + call.Target.Name : stmt.GetType().Name.ToLowerInvariant();
	}

	/// <summary>
	/// Whether the statement can fail: what reads input or asks; not what only assigns or builds, nor
	/// a repetition that may take no turn, nor a choice tried in order with an alternative that cannot.
	/// A choice told by its first item tries one alternative, and fails where the item begins none.
	/// </summary>
	/// <remarks>
	/// An over-approximation everywhere but in the two answers of no: a statement that only assigns
	/// or builds cannot fail (the host's throwing is no failure), a repetition of fewest nought
	/// stops before a turn that fails, and an ordered choice with an alternative that cannot fail
	/// ends there at the latest. Everything else that reads, asks, calls or looks is taken to fail.
	/// </remarks>
	bool Failable(IrStmt stmt)
	{
		if (_criterion == ResumptionCriterion.FailsBackNever)
			return false;

		return stmt switch
		{
			IrStmt.Assign or IrStmt.Append or IrStmt.Construct => false,
			IrStmt.Block block => block.Items.Any(Failable),
			IrStmt.Scope or IrStmt.Replay or IrStmt.Commit => Failable(stmt.Children.Single()),
			IrStmt.Choice { Dispatch: not null } => true,
			IrStmt.Choice choice => choice.Alternatives.All(Failable),
			IrStmt.Repeat repeat => repeat.Min is not IrExpr.Number { Value: 0 } && Failable(repeat.Body),
			_ => true,
		};
	}

	/// <summary>
	/// Whether a reading of the statement can fail inside it after a decision it opened inside, and
	/// leave that decision on the tape for a retry from outside: a decision followed in a sequence
	/// inside it by something that can fail, or a repetition whose first two turns or more, which
	/// have no decision of their own to fail to, decide and can fail. Not inside a call, whose
	/// replay retries its own decisions until none can move, nor inside braces or a look.
	/// </summary>
	/// <remarks>
	/// An over-approximation: a failure after a decision inside an alternative of a choice that
	/// reads it again over its own decisions, or inside a turn that has a decision of its own, is
	/// taken back where it happens and never reaches a retry from outside — those are counted too.
	/// </remarks>
	bool LeavesInside(IrStmt stmt)
	{
		if (_criterion is ResumptionCriterion.FailsBackNever or ResumptionCriterion.FailsBackOutsideOnly)
			return false;

		switch (stmt)
		{
			case IrStmt.Call or IrStmt.Look or IrStmt.Commit:
				return false;

			case IrStmt.Block block:
			{
				var decided = false;

				foreach (var item in block.Items)
				{
					if (decided && Failable(item) || LeavesInside(item))
						return true;

					decided |= IrLowering.Decides(item);
				}

				return false;
			}

			case IrStmt.Repeat repeat when repeat.Min is not IrExpr.Number { Value: < 2 } && IrLowering.Decides(repeat.Body) && Failable(repeat.Body):
				return true;
		}

		return stmt.Children.Any(LeavesInside);
	}

	/// <summary>Whether something after the statement inside its function can fail before braces or a look seal it; null where the search reaches the function's end.</summary>
	bool? FailsWithin(IrStmt stmt)
	{
		var at = stmt;

		while (_parents.TryGetValue(at, out var parent))
		{
			switch (parent)
			{
				case IrStmt.Block block:
				{
					var index = IndexOf(block.Items, at);

					for (var i = index + 1; i < block.Items.Count; i++)
						if (Failable(block.Items[i]))
							return true;

					break;
				}

				case IrStmt.Look or IrStmt.Commit:
					return false;
			}

			at = parent;
		}

		return null;
	}

	bool FailsAfter(IrStmt stmt)
	{
		return FailsWithin(stmt) ?? _failFollowed[_owners[stmt]];
	}

	/// <summary>Of every function, whether something can fail after some call of it before a seal: the least answer, by rounds.</summary>
	void FindFailFollowed()
	{
		foreach (var function in _callers.Keys)
			_failFollowed[function] = false;

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var (function, calls) in _callers)
			{
				if (!_failFollowed[function] && calls.Any(call => FailsWithin(call) ?? _failFollowed[_owners[call]]))
				{
					_failFollowed[function] = true;
					changed                 = true;
				}
			}
		}
	}

	/// <summary>Of every function, whether after some call of it a decision comes that something after can fail back to: the least answer, by rounds.</summary>
	void FindKeptByCallers()
	{
		foreach (var function in _callers.Keys)
			_keptByCallers[function] = false;

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var (function, calls) in _callers)
			{
				if (_keptByCallers[function])
					continue;

				foreach (var call in calls)
				{
					var (decider, sealedOff, _) = FirstDecider(call);

					if (decider is not null ? FailsAfter(decider) || LeavesInside(decider) : !sealedOff && _keptByCallers[_owners[call]])
					{
						_keptByCallers[function] = true;
						changed                  = true;

						break;
					}
				}
			}
		}
	}

	/// <summary>Whether, under <see cref="ResumptionCriterion.FailsBack"/>, a decision that keeps the statement has something after it that can fail back.</summary>
	bool FailsBack(IrStmt stmt)
	{
		if (_criterion == ResumptionCriterion.FailsBackAlways)
			return true;

		if (IrLowering.Decides(stmt) && (FailsAfter(stmt) || LeavesInside(stmt)))
			return true;

		var (decider, sealedOff, _) = FirstDecider(stmt);

		return decider is not null ? FailsAfter(decider) || LeavesInside(decider) : !sealedOff && _keptByCallers[_owners[stmt]];
	}

	/// <summary>The decision that keeps a statement under <see cref="ResumptionCriterion.Decided"/>, in words.</summary>
	string Justified(IrStmt stmt)
	{
		if (IrLowering.Decides(stmt))
			return "it decides itself";

		var (decider, _, where) = FirstDecider(stmt);

		if (decider is not null)
			return where;

		// Past the function's end: the first caller whose continuation decides.
		foreach (var call in _callers[_owners[stmt]])
		{
			var (after, _, said) = FirstDecider(call);

			if (after is not null)
				return "after a call from " + _owners[call].Name + ": " + said;
		}

		return "after a call, further up";
	}

	/// <summary>Whether a statement's closure reaches a fragment that names what the parser supplies.</summary>
	static bool NamesParser(IrStmt stmt, HashSet<IrFunction> seen)
	{
		switch (stmt)
		{
			case IrStmt.Guard { Fragment.NamesParser: true } or IrStmt.Construct { Fragment.NamesParser: true }:
				return true;

			case IrStmt.Call call when seen.Add(call.Target) && NamesParser(call.Target.Body, seen):
				return true;
		}

		foreach (var child in stmt.Children)
			if (NamesParser(child, seen))
				return true;

		return false;
	}

	/// <summary>Every statement kept, with why: a repetition, or what makes the statement's work unbounded.</summary>
	public IReadOnlyDictionary<IrStmt, string> Sites => _sites;

	public bool Keeps(IrStmt stmt)
	{
		return _sites.ContainsKey(stmt);
	}

	static IEnumerable<IrFunction> Calls(IrStmt stmt)
	{
		if (stmt is IrStmt.Call call)
			yield return call.Target;

		foreach (var child in stmt.Children)
			foreach (var target in Calls(child))
				yield return target;
	}

	bool Recursive(IrFunction function)
	{
		return _reach[function].Contains(function);
	}

	void Visit(IrFunction function, bool inReplay)
	{
		if (_visited.Add((function, inReplay)))
			Visit(function.Body, inReplay);
	}

	void Visit(IrStmt stmt, bool inReplay)
	{
		switch (stmt)
		{
			case IrStmt.Replay replay:
				Visit(replay.Body, inReplay: true);

				return;

			case IrStmt.Look or IrStmt.Commit:
				Keep(stmt, inReplay);

				// Inside, what is kept is taken off the tape where the braces end: worth keeping
				// only for the braces' own reading again, which there is only where the body decides.
				foreach (var child in stmt.Children)
					Visit(child, IrLowering.Decides(child));

				return;

			case IrStmt.Call call:
				Keep(stmt, inReplay);
				Visit(call.Target, inReplay);

				return;

			default:
				Keep(stmt, inReplay);

				foreach (var child in stmt.Children)
					Visit(child, inReplay);

				return;
		}
	}

	void Keep(IrStmt stmt, bool inReplay)
	{
		if (!inReplay || _sites.ContainsKey(stmt))
			return;

		// What is unbounded inside anything else is kept where it stands, and goes straight past
		// there: a repetition of a bounded number of turns, and a call into a function that cannot
		// reach itself, cost no more on a replay than the grammar is long.
		var why = stmt switch
		{
			IrStmt.Repeat { Max: not IrExpr.Number } => "a repetition",
			IrStmt.Call call when Recursive(call.Target) => "a call into a recursion",
			IrStmt.Look or IrStmt.Commit => Raw(stmt.Children.Single()) is { } inside ? "braces or a look over " + inside : null,
			IrStmt.MatchSame => "a text read again by reference",
			IrStmt.Guard guard => LongArgument(guard.Arguments),
			_ => null,
		};

		if (why is null || Left.ContainsKey(stmt))
			return;

		var left = NamesParser(stmt, []) ? "it reads what the parser supplies, which no record holds"
			: _criterion == ResumptionCriterion.Repetitions && stmt is not IrStmt.Repeat ? "only repetitions are kept"
			: _criterion is ResumptionCriterion.Decided or ResumptionCriterion.Repetitions && !IrLowering.Decides(stmt) && !DecidesAfter(stmt) ? "nothing after it can decide"
			: _criterion == ResumptionCriterion.Narrowed && stmt is IrStmt.Repeat repeat && !IrLowering.Decides(repeat.Body) && !repeat.Resumable ? "no decision of its own outlives a turn"
			: _criterion is ResumptionCriterion.FailsBack or ResumptionCriterion.FailsBackAlways or ResumptionCriterion.FailsBackNever or ResumptionCriterion.FailsBackOutsideOnly &&
				(!IrLowering.Decides(stmt) && !DecidesAfter(stmt) || !FailsBack(stmt)) ? "nothing can fail back to a decision that keeps it"
			: null;

		if (left is not null)
			Left.Add(stmt, why + ": " + left);
		else
		{
			_sites.Add(stmt, why);
			Kept[stmt] = Justified(stmt);

			// A guard's or a reference's work does not hang on the next item.
			var door = stmt switch
			{
				IrStmt.Repeat turns => Begins(turns.Body).First,
				IrStmt.Look or IrStmt.Guard or IrStmt.MatchSame => null,
				_ => Begins(stmt).First,
			};

			if (door is not null)
				Doors.Add(stmt, Merged(door));
		}
	}

	/// <summary>Why a statement's work is unbounded when it is read from nothing, repetitions included; null where it is bounded.</summary>
	string? Raw(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.Repeat { Max: not IrExpr.Number }:
				return "a repetition";

			case IrStmt.MatchSame:
				return "a text read again by reference";

			case IrStmt.Guard guard:
				return LongArgument(guard.Arguments);

			case IrStmt.Call call:
				return Recursive(call.Target) ? "a call into a recursion" : Raw(call.Target);

			default:
				foreach (var child in stmt.Children)
					if (Raw(child) is { } why)
						return why;

				return null;
		}
	}

	string? Raw(IrFunction function)
	{
		if (_raw.TryGetValue(function, out var known))
			return known;

		_raw[function] = null;

		return _raw[function] = Raw(function.Body);
	}

	string? LongArgument(IReadOnlyList<IrExpr> arguments)
	{
		foreach (var argument in arguments)
		{
			if (argument is IrExpr.Items || argument is IrExpr.Slot slot && _longTexts.Contains(slot.Of))
				return "a guard handed an unbounded text or a list";
		}

		return null;
	}

	void FindGuardArguments(IrStmt stmt)
	{
		if (stmt is IrStmt.Guard guard)
		{
			foreach (var argument in guard.Arguments)
			{
				if (argument is IrExpr.Slot slot)
					ReadByGuards.Add(slot.Of);
				else if (argument is IrExpr.Items items)
					ReadByGuards.Add(items.Of);
			}
		}

		foreach (var child in stmt.Children)
			FindGuardArguments(child);
	}

	/// <summary>The slots a capture fills with the text of a body that reads unbounded input.</summary>
	void FindLongTexts(IrStmt stmt)
	{
		if (stmt is IrStmt.Block { Items: [IrStmt.Assign { Value: IrExpr.Position, Slot: var from }, var body, var take] })
		{
			var into = take switch
			{
				IrStmt.Assign { Value: IrExpr.Text text } assign when text.From == from => assign.Slot,
				IrStmt.Append { Value: IrExpr.Text text } append when text.From == from => append.List,
				_ => null,
			};

			if (into is not null && Raw(body) is not null)
				_longTexts.Add(into);
		}

		foreach (var child in stmt.Children)
			FindLongTexts(child);
	}
}
