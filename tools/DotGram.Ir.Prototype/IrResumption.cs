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
sealed class ResumptionSites
{
	static readonly ConditionalWeakTable<IrProgram, ResumptionSites> _known = [];

	readonly Dictionary<IrStmt, string> _sites = new(ReferenceEqualityComparer.Instance);

	readonly Dictionary<IrFunction, HashSet<IrFunction>> _reach = [];

	readonly Dictionary<IrFunction, string?> _residual = [];
	readonly Dictionary<IrFunction, string?> _raw      = [];

	// Slots that hold a text read over input of no bound.
	readonly HashSet<IrSlot> _longTexts = [];

	readonly HashSet<(IrFunction, bool)> _visited = [];

	ResumptionSites(IrProgram program)
	{
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

		foreach (var entry in program.Entries.Values)
			Visit(entry, inReplay: false);
	}

	/// <summary>What is kept of a program, worked out once.</summary>
	public static ResumptionSites Of(IrProgram program)
	{
		return _known.GetValue(program, static one => new ResumptionSites(one));
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

		var why = stmt switch
		{
			IrStmt.Repeat => "a repetition",
			IrStmt.Call or IrStmt.Look or IrStmt.Commit or IrStmt.MatchSame or IrStmt.Guard => Residual(stmt),
			_ => null,
		};

		if (why is not null)
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
