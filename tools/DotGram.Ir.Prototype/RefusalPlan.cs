using System;
using System.Text;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>
/// Where the generated C# reader says what it expected, put on a lowered program as facts of the
/// tree: which repetitions stand behind a door, which choices tell their alternatives apart by the
/// next input item, and which sets say nothing.
/// </summary>
/// <remarks>
/// A spike: the rules here were read off the reader the generator prints for the fixture grammars,
/// not asked of it. They are the shape of what a shared plan would hand every target.
/// </remarks>
static class RefusalPlan
{
	/// <summary>What something begins with: the input items, how the reader would name them, and whether it can read nothing.</summary>
	sealed record Begins(List<CharRange> Ranges, List<string> Items, bool Nullable);

	public static IrProgram AsTheCSharpReader(IrProgram program)
	{
		// A rule that is one set is named for itself wherever it refuses.
		foreach (var function in program.Functions)
			if (Bare(function.Body) is IrStmt.MatchSet set && !program.Entries.ContainsValue(function))
				function.Body = Rewrite(program, function.Body, static (_, one) => one is IrStmt.MatchSet only ? only with { Expected = -2 } : one);

		foreach (var function in program.Functions)
		{
			var named = function;

			function.Body = Rewrite(program, function.Body, (_, one) =>
				one is IrStmt.MatchSet { Expected: -2 } only ? only with { Expected = Expect(program, named.Name) } : one);
		}

		foreach (var function in program.Functions)
			function.Body = Rewrite(program, function.Body, Plan);

		return program;
	}

	static IrStmt Plan(IrProgram program, IrStmt stmt)
	{
		switch (stmt)
		{
			case IrStmt.Repeat { Resumable: false } repeat when Bare(repeat.Body) is IrStmt.MatchSet { IsNegated: false } set:
				// A run of one set is scanned: past its least it says nothing where it stops.
				return repeat with { Door = set.Ranges };

			case IrStmt.Repeat { Resumable: false } repeat
				when Bare(repeat.Body) is IrStmt.Block { Items: [IrStmt.MatchSet { IsNegated: false } or IrStmt.MatchText { IgnoreCase: false }, ..] } block
					&& Of(program, block, []) is { Nullable: false } begins:
				// A turn that begins with a terminal of its own is not tried where the terminal is not next.
				return repeat with { Door = begins.Ranges };

			case IrStmt.Choice { Resumable: false, Alternatives.Count: > 1 } choice:
			{
				var all   = new List<Begins>();
				var items = new List<string>();

				foreach (var alternative in choice.Alternatives)
				{
					if (Of(program, alternative, []) is not { Nullable: false } begins)
						return stmt;

					foreach (var other in all)
						if (Overlap(other.Ranges, begins.Ranges))
							return stmt;

					all.Add(begins);

					foreach (var item in begins.Items)
						if (!items.Contains(item))
							items.Add(item);
				}

				return choice with
				{
					Dispatch = new IrDispatch([.. all.Select(static one => one.Ranges)], Expect(program, "[" + string.Join(" | ", items) + "]")),
				};
			}

			default:
				return stmt;
		}
	}

	static int Expect(IrProgram program, string label)
	{
		var index = program.Expected.IndexOf(label);

		if (index < 0)
		{
			index = program.Expected.Count;
			program.Expected.Add(label);
		}

		return index;
	}

	/// <summary>A statement out of the scopes and one-step blocks around it.</summary>
	static IrStmt Bare(IrStmt stmt)
	{
		while (true)
		{
			switch (stmt)
			{
				case IrStmt.Scope scope:              stmt = scope.Body; break;
				case IrStmt.Block { Items: [var one] }: stmt = one;      break;
				default:                              return stmt;
			}
		}
	}

	static Begins? Of(IrProgram program, IrStmt stmt, HashSet<IrFunction> within)
	{
		switch (stmt)
		{
			case IrStmt.MatchSet { IsNegated: false } set:
			{
				var label = set.Expected >= 0 ? program.Expected[set.Expected] : "";
				var items = label.StartsWith('[') && label.EndsWith(']') ? label[1..^1].Split(" | ").ToList() : [label];

				return new Begins([.. set.Ranges], items, false);
			}

			case IrStmt.MatchText { IgnoreCase: false, Text.Length: > 0 } text:
				return new Begins([new CharRange(text.Text[0], text.Text[0])], [Quoted(text.Text[0])], false);

			case IrStmt.Scope scope:
				return Of(program, scope.Body, within);

			case IrStmt.Replay replay:
				return Of(program, replay.Body, within);

			case IrStmt.Commit commit:
				return Of(program, commit.Body, within);

			case IrStmt.Call call:
			{
				if (!within.Add(call.Target))
					return null;

				var begins = Of(program, call.Target.Body, within);

				within.Remove(call.Target);

				// What a rule begins with is named as a set of its own, in the order of the items.
				return begins is null ? null : begins with { Items = [.. Merged(begins.Ranges).Select(Named)] };
			}

			case IrStmt.Block block:
			{
				var ranges = new List<CharRange>();
				var items  = new List<string>();

				foreach (var item in block.Items)
				{
					if (item is IrStmt.Assign or IrStmt.Append or IrStmt.Construct)
						continue;

					if (Of(program, item, within) is not { } begins)
						return null;

					ranges.AddRange(begins.Ranges);
					items.AddRange(begins.Items);

					if (!begins.Nullable)
						return new Begins(ranges, items, false);
				}

				return new Begins(ranges, items, true);
			}

			case IrStmt.Choice choice:
			{
				var ranges   = new List<CharRange>();
				var items    = new List<string>();
				var nullable = false;

				foreach (var alternative in choice.Alternatives)
				{
					if (Of(program, alternative, within) is not { } begins)
						return null;

					ranges.AddRange(begins.Ranges);
					items.AddRange(begins.Items);
					nullable |= begins.Nullable;
				}

				return new Begins(ranges, items, nullable);
			}

			case IrStmt.Repeat { Min: IrExpr.Number min } repeat:
				return Of(program, repeat.Body, within) is { } turn ? turn with { Nullable = turn.Nullable || min.Value == 0 } : null;

			default:
				return null;
		}
	}

	static bool Overlap(List<CharRange> one, List<CharRange> other)
	{
		foreach (var a in one)
			foreach (var b in other)
				if (a.From <= b.To && b.From <= a.To)
					return true;

		return false;
	}

	static List<CharRange> Merged(List<CharRange> ranges)
	{
		var merged = new List<CharRange>();

		foreach (var range in ranges.OrderBy(static one => one.From))
		{
			if (merged.Count > 0 && range.From <= merged[^1].To + 1)
				merged[^1] = new CharRange(merged[^1].From, (char)Math.Max(merged[^1].To, range.To));
			else
				merged.Add(range);
		}

		return merged;
	}

	static string Named(CharRange range)
	{
		return range.From == range.To ? Quoted(range.From) : Quoted(range.From) + ".." + Quoted(range.To);
	}

	static string Quoted(char unit)
	{
		return unit switch
		{
			'\t' => @"'\t'",
			'\n' => @"'\n'",
			'\r' => @"'\r'",
			'\\' => @"'\\'",
			'\'' => @"'\''",
			_    => "'" + unit + "'",
		};
	}

	/// <summary>The tree with every statement, innermost first, replaced by what is said of it.</summary>
	static IrStmt Rewrite(IrProgram program, IrStmt stmt, Func<IrProgram, IrStmt, IrStmt> change)
	{
		IrStmt Again(IrStmt one)
		{
			return Rewrite(program, one, change);
		}

		IrStmt inner = stmt switch
		{
			IrStmt.Block block   => block with { Items = [.. block.Items.Select(Again)] },
			IrStmt.Scope scope   => scope with { Body = Again(scope.Body) },
			IrStmt.Choice choice => choice with { Alternatives = [.. choice.Alternatives.Select(Again)] },
			IrStmt.Repeat repeat => repeat with { Body = Again(repeat.Body) },
			IrStmt.Look look     => look with { Body = Again(look.Body) },
			IrStmt.Commit commit => commit with { Body = Again(commit.Body) },
			IrStmt.Replay replay => replay with { Body = Again(replay.Body) },
			_                    => stmt,
		};

		return change(program, inner);
	}
}
