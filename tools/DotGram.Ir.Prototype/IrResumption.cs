using System;
using System.Runtime.CompilerServices;

namespace DotGram.Ir.Prototype;

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
/// never be moved and names the statement's entry on the tape (its sentinel): every repetition
/// (where each of its turns began, and where it ended), and every other statement whose work is
/// not bounded by the grammar once repetitions go straight to their turns (where it ended). A
/// replay that reads such a sentinel back takes what was kept and goes on from there.
/// </para>
/// <para>
/// What is unbounded is computed, not listed: a call into a function that can reach itself; a
/// text read again by reference; a guard handed a text read over unbounded input, or a list; and
/// a look or braces whose body holds any of these or any repetition — because everything decided
/// inside braces is taken off the tape where they end, and a replay reads them again from nothing.
/// A call into a function that holds only repetitions needs nothing of its own: they keep their own.
/// </para>
/// <para>
/// Only a statement a replay can reach is kept: one inside a replay region, or inside a function
/// called from one, or inside braces or a look whose body decides — there the braces' own reading
/// again is the replay. Inside braces or a look that decide nothing, nothing kept would outlive them.
/// </para>
/// </remarks>
/// <summary>Which of the statements a replay can reach keep a record.</summary>
enum ResumptionCriterion
{
	/// <summary>
	/// The one decided: a statement keeps where it decides itself, or where something after it,
	/// before its replay region ends, can decide — for only a decision moved at or after a
	/// statement makes a replay read it again.
	/// </summary>
	Decided,

	/// <summary>Every repetition and every unbounded statement a replay can reach.</summary>
	Wide,

	/// <summary>
	/// A repetition only where a decision can open inside its turns and outlive them; other
	/// statements as <see cref="Wide"/>. Measured, not used: a repetition with no decision of its
	/// own is read again by every replay of a decision after it.
	/// </summary>
	Narrowed,
}

sealed class ResumptionSites
{
	static readonly ConditionalWeakTable<IrProgram, Dictionary<ResumptionCriterion, ResumptionSites>> _known = [];

	readonly ResumptionCriterion _criterion;

	// The statement each statement of a function's body stands in, and the calls of each function.
	readonly Dictionary<IrStmt, IrStmt> _parents = new(ReferenceEqualityComparer.Instance);
	readonly Dictionary<IrFunction, List<IrStmt.Call>> _callers = [];
	readonly Dictionary<IrStmt, IrFunction> _owners = new(ReferenceEqualityComparer.Instance);
	readonly Dictionary<IrFunction, bool> _followed = [];

	/// <summary>Every statement a replay can reach that the criterion leaves without a record, and why.</summary>
	public Dictionary<IrStmt, string> Left { get; } = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrStmt, string> _sites = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrFunction, HashSet<IrFunction>> _reach = [];

	readonly Dictionary<IrFunction, string?> _residual = [];
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
	}

	/// <summary>The slots some guard is handed: a reading that builds nothing still fills them.</summary>
	public HashSet<IrSlot> ReadByGuards { get; } = [];

	/// <summary>What is kept of a program under a criterion, worked out once.</summary>
	public static ResumptionSites Of(IrProgram program, ResumptionCriterion criterion = ResumptionCriterion.Decided)
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

		// A repetition of a bounded number of turns is bounded work unless its body is not.
		var why = stmt switch
		{
			IrStmt.Repeat { Max: IrExpr.Number } bounded => Residual(bounded.Body) is { } inside ? "a bounded repetition over " + inside : null,
			IrStmt.Repeat => "a repetition",
			IrStmt.Call or IrStmt.Look or IrStmt.Commit or IrStmt.MatchSame or IrStmt.Guard => Residual(stmt),
			_ => null,
		};

		if (why is null || Left.ContainsKey(stmt))
			return;

		var left = NamesParser(stmt, []) ? "it reads what the parser supplies, which no record holds"
			: _criterion == ResumptionCriterion.Decided && !IrLowering.Decides(stmt) && !DecidesAfter(stmt) ? "nothing after it can decide"
			: _criterion == ResumptionCriterion.Narrowed && stmt is IrStmt.Repeat repeat && !IrLowering.Decides(repeat.Body) && !repeat.Resumable ? "no decision of its own outlives a turn"
			: null;

		if (left is not null)
			Left.Add(stmt, why + ": " + left);
		else
			_sites.Add(stmt, why);
	}

	/// <summary>Why a statement's work is unbounded once repetitions go straight to their turns, or null where it is not.</summary>
	string? Residual(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.MatchSame:
				return "a text read again by reference";

			case IrStmt.Guard guard:
				return LongArgument(guard.Arguments);

			case IrStmt.Call call:
				return Recursive(call.Target) ? "a call into a recursion" : Residual(call.Target);

			case IrStmt.Look or IrStmt.Commit:
				return Raw(stmt.Children.Single()) is { } inside ? "braces or a look over " + inside : null;

			case IrStmt.Repeat repeat:
				return Residual(repeat.Body);

			default:
				foreach (var child in stmt.Children)
					if (Residual(child) is { } why)
						return why;

				return null;
		}
	}

	string? Residual(IrFunction function)
	{
		if (_residual.TryGetValue(function, out var known))
			return known;

		_residual[function] = null;

		return _residual[function] = Residual(function.Body) is { } why ? "a call that reads " + why : null;
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
