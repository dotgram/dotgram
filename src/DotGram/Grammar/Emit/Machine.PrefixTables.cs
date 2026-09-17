using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	int _prefixCount;

	// Only non-overlapping, case-sensitive prefixes with no preceding effects. The
	// original choice remains the failure path, preserving diagnostics and starvation.
	// Bare literal choices already have their own specialization and stay there.
	static string[]? PrefixPlan(IReadOnlyList<Node> alternatives)
	{
		if (alternatives.Count < 4) return null;
		var prefixes = new string[alternatives.Count];
		for (var i = 0; i < alternatives.Count; i++)
		{
			if (alternatives[i] is Node.Literal) return null;
			var node = alternatives[i];
			while (true)
			{
				if (node is Node.Construct construct) node = construct.Body;
				else if (node is Node.Atomic atomic) node = atomic.Body;
				else if (node is Node.Capture capture) node = capture.Body;
				else if (node is Node.Sequence sequence && sequence.Nodes.Count > 0) node = sequence.Nodes[0];
				else break;
			}
			if (node is not Node.Literal { IgnoreCase: false, Text.Length: > 0 } literal) return null;
			prefixes[i] = literal.Text;
		}
		var sorted = prefixes.OrderBy(static text => text, StringComparer.Ordinal).ToArray();
		for (var i = 1; i < sorted.Length; i++)
			if (sorted[i].StartsWith(sorted[i - 1], StringComparison.Ordinal)) return null;
		var low = prefixes.Min(static text => text.Min());
		var width = prefixes.Max(static text => text.Max()) - low + 1;
		// Bound generated data as well as alphabet width. Wide Unicode choices retain
		// the existing implementation; this experimental strategy targets small alphabets.
		return width <= 128 && (long)width * prefixes.Sum(static text => text.Length) <= 262144 ? prefixes : null;
	}

	int CompilePrefixChoice(IReadOnlyList<Node> alternatives, string[] prefixes, int next, FollowSets.Continuation following)
	{
		var heads = new Dictionary<Node, int>(NodeIdentity.Instance);
		var fallback = Dispatchable(alternatives) is { } groups
			? CompileDispatchedChoice(alternatives, groups, next, following, heads)
			: CompileChainedChoice(alternatives, next, following, prefixHeads: heads);
		var low = prefixes.Min(static text => text.Min());
		var width = prefixes.Max(static text => text.Max()) - low + 1;
		var rows = new List<int[]>();
		int AddRow()
		{
			rows.Add(Enumerable.Repeat(-1, width).ToArray());
			return rows.Count - 1;
		}
		AddRow();
		for (var i = 0; i < prefixes.Length; i++)
		{
			var row = 0;
			for (var at = 0; at < prefixes[i].Length; at++)
			{
				var column = prefixes[i][at] - low;
				if (at == prefixes[i].Length - 1) rows[row][column] = -i - 2;
				else
				{
					if (rows[row][column] == -1) rows[row][column] = AddRow();
					row = rows[row][column];
				}
			}
		}
		var name = "Prefix_DotGram" + _tag + "_" + _prefixCount++;
		var helper = new Writer(0);
		helper.Line($"private static readonly int[] {name}_Next = new int[]");
		using (helper.Block(""))
			foreach (var row in rows) helper.Line(string.Join(", ", row) + ",");
		helper.Line(";");
		using (helper.Block($"private static int {name}({InputType} text, int p)"))
		{
			helper.Line("var state = 0;");
			using (helper.Block($"while ({Room(1)})"))
			{
				helper.Line($"var column = (int){ReadAt("p")} - {(int)low};");
				helper.Line($"if ((uint)column >= {width}u) return -1;");
				helper.Line($"state = {name}_Next[state * {width} + column];");
				helper.Line("if (state < 0) return -state - 2;");
				helper.Line("p++;");
			}
			helper.Line("return -1;");
		}
		_extra.Add(helper.ToString());
		var entry = Reserve(out var writer);
		using (writer.Block($"switch ({name}(text, p))"))
			for (var i = 0; i < alternatives.Count; i++)
				writer.Line($"case {i}: goto {Label(writer, heads[alternatives[i]])};");
		writer.Line($"goto {Label(writer, fallback)};");
		return entry;
	}
}
