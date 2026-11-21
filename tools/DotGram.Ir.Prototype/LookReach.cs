using System;

namespace DotGram.Ir.Prototype;

/// <summary>
/// How far a look can read past where it begins, for restart by budget: a look's refusals are said
/// to no one, so a reading again whose work is a long look is charged by the look's reach instead.
/// </summary>
/// <remarks>
/// The reach of a statement is at most the units it consumes plus the units it inspects beyond them,
/// summed along a sequence (an over-estimate, which over-charges and is safe for the bound). It is
/// unbounded where a repetition without a number for its most turns reads anything, where a text is
/// read by reference, and where a call reaches round a cycle. A look whose reach is bounded raises the
/// charge's position to its start plus its reach where it ends; refusals inside a look whose reach is
/// not raise it themselves, in the functions such a look reaches and in its own body only.
/// </remarks>
sealed class LookReach
{
	readonly Dictionary<IrFunction, int?> _functions = [];
	readonly HashSet<IrFunction>          _visiting  = [];

	/// <summary>The functions an unbounded look's body calls, directly or further.</summary>
	public HashSet<IrFunction> Reached { get; } = [];

	/// <summary>The looks whose reach is unbounded.</summary>
	public HashSet<IrStmt.Look> Unbounded { get; } = new(ReferenceEqualityComparer.Instance);

	public LookReach(IrProgram program)
	{
		foreach (var function in program.Functions)
			Find(function.Body);

		var pending = new Stack<IrFunction>();

		foreach (var look in Unbounded)
			foreach (var target in Calls(look.Body))
				pending.Push(target);

		while (pending.TryPop(out var next))
			if (Reached.Add(next))
				foreach (var further in Calls(next.Body))
					pending.Push(further);
	}

	void Find(IrStmt stmt)
	{
		if (stmt is IrStmt.Look look && Of(look.Body) is null)
			Unbounded.Add(look);

		foreach (var child in stmt.Children)
			Find(child);
	}

	static IEnumerable<IrFunction> Calls(IrStmt stmt)
	{
		if (stmt is IrStmt.Call call)
			yield return call.Target;

		foreach (var child in stmt.Children)
			foreach (var target in Calls(child))
				yield return target;
	}

	/// <summary>The reach of a statement, or null where it has none.</summary>
	public int? Of(IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.MatchSet:
				return 1;

			case IrStmt.MatchText text:
				return text.Text.Length;

			case IrStmt.MatchSame:
				return null;

			case IrStmt.Call call:
				return Function(call.Target);

			case IrStmt.Choice choice:
			{
				var most = 0;

				foreach (var alternative in choice.Alternatives)
				{
					if (Of(alternative) is not { } one)
						return null;

					most = Math.Max(most, one);
				}

				return most;
			}

			case IrStmt.Repeat repeat:
			{
				if (Of(repeat.Body) is not { } one)
					return null;

				if (one == 0)
					return 0;

				return repeat.Max is IrExpr.Number max ? checked(max.Value * one) : null;
			}

			case IrStmt.Block block:
			{
				var sum = 0;

				foreach (var item in block.Items)
				{
					if (Of(item) is not { } one)
						return null;

					sum = checked(sum + one);
				}

				return sum;
			}

			default:
			{
				var sum = 0;

				foreach (var child in stmt.Children)
				{
					if (Of(child) is not { } one)
						return null;

					sum = checked(sum + one);
				}

				return sum;
			}
		}
	}

	int? Function(IrFunction function)
	{
		if (_functions.TryGetValue(function, out var known))
			return known;

		// Round a cycle the reach is as long as the input.
		if (!_visiting.Add(function))
			return null;

		var reach = Of(function.Body);

		_visiting.Remove(function);
		_functions[function] = reach;

		return reach;
	}
}
