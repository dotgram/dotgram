using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	int _prefixCount;

	// Only non-overlapping, case-sensitive prefixes with no preceding effects. The
	// original choice remains the failure path, preserving diagnostics and starvation.
	// Bare literal choices already have their own specialization and stay there.
	//
	// A plan exists only when every alternative begins with its own literal and no literal
	// is a prefix of another, so the table it builds is exhaustive: a character it has no
	// way on for is one that every alternative fails at. The fast failure in
	// CompilePrefixChoice rests on that. A plan that covered only some alternatives would
	// have to send a miss on to the ones it left out, and must not reach that code as it is.
	static string[]? PrefixPlan(IReadOnlyList<Node> alternatives)
	{
		if (alternatives.Count < 4) return null;
		var prefixes = new string[alternatives.Count];
		for (var i = 0; i < alternatives.Count; i++)
		{
			if (alternatives[i] is Node.Literal) return null;
			if (PrefixLiteral(alternatives[i]) is not { } literal) return null;
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

	// The literal an alternative begins with, through what consumes nothing before it.
	static Node.Literal? PrefixLiteral(Node node)
	{
		while (true)
		{
			if (node is Node.Construct construct) node = construct.Body;
			else if (node is Node.Atomic atomic) node = atomic.Body;
			else if (node is Node.Capture capture) node = capture.Body;
			else if (node is Node.Sequence sequence && sequence.Nodes.Count > 0) node = sequence.Nodes[0];
			else break;
		}

		return node is Node.Literal { IgnoreCase: false, Text.Length: > 0 } literal ? literal : null;
	}

	int CompilePrefixChoice(IReadOnlyList<Node> alternatives, string[] prefixes, int next, FollowSets.Continuation following)
	{
		Debug.Assert(prefixes.Length == alternatives.Count, "A prefix table covers every alternative or none.");

		var heads = new Dictionary<Node, int>(NodeIdentity.Instance);
		var fallback = Dispatchable(alternatives) is { } groups
			? CompileDispatchedChoice(alternatives, groups, next, following, heads)
			: CompileChainedChoice(alternatives, next, following, prefixHeads: heads);
		var low = prefixes.Min(static text => text.Min());
		var width = prefixes.Max(static text => text.Max()) - low + 1;
		var rows = new List<int[]>();
		var paths = new List<string>();
		int AddRow(string path)
		{
			rows.Add(Enumerable.Repeat(-1, width).ToArray());
			paths.Add(path);
			return rows.Count - 1;
		}
		AddRow("");
		for (var i = 0; i < prefixes.Length; i++)
		{
			var row = 0;
			for (var at = 0; at < prefixes[i].Length; at++)
			{
				var column = prefixes[i][at] - low;
				if (at == prefixes[i].Length - 1) rows[row][column] = -i - 2;
				else
				{
					if (rows[row][column] == -1) rows[row][column] = AddRow(prefixes[i].Substring(0, at + 1));
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

		// The alternative whose prefix the input holds, or where it was decided that none
		// does: -1 where nothing is decided — the input ran out, or its first character
		// begins no prefix — and -1 - r where row r, one or more characters in, has no way
		// on for the next one. Row 0 is the root, so a state of 0 has read nothing.
		using (helper.Block($"private static int {name}({InputType} text, int p)"))
		{
			helper.Line("var state = 0;");
			using (helper.Block($"while ({Room(1)})"))
			{
				helper.Line($"var column = (int){ReadAt("p")} - {(int)low};");
				helper.Line($"if ((uint)column >= {width}u) return state == 0 ? -1 : -1 - state;");
				helper.Line($"var next = {name}_Next[state * {width} + column];");
				helper.Line("if (next < 0) return next == -1 ? (state == 0 ? -1 : -1 - state) : -next - 2;");
				helper.Line("state = next;");
				helper.Line("p++;");
			}
			helper.Line("return -1;");
		}

		EmitPrefixMiss(helper, name, alternatives, prefixes, paths);
		EmitPrefixFailure(helper, name);

		_extra.Add(helper.ToString());
		var entry = Reserve(out var writer);
		writer.Line($"var prefixed = {name}(text, p);");
		using (writer.Block("switch (prefixed)"))
			for (var i = 0; i < alternatives.Count; i++)
				writer.Line($"case {i}: goto {Label(writer, heads[alternatives[i]])};");
		// Out of line: the recognizer this sits in is as large as the JIT will optimize, and a
		// miss's bookkeeping kept in it cost the stream form's hits four per cent.
		writer.Line($"if (prefixed < -1 && {name}_Miss(text, ref p, -1 - prefixed, lookahead, ref failure, out expected)) goto {Label(writer, _fail)};");
		writer.Line($"goto {Label(writer, fallback)};");
		return entry;
	}

	/// <summary>
	/// What a miss at each row of a prefix table reports: how far the input went, the
	/// alternatives that went that far, and how much input their group needs to be told apart.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The fallback would try, in their order, the alternatives whose literal begins with the
	/// first character — dispatch hands it that character's group, and a plain chain steps
	/// over the rest on the character without writing anything down. Each fails where its
	/// literal and the input part: the literal moves the position past what matched before
	/// it reports, so a longer shared start is a further failure. A miss at row r, d
	/// characters in, is one where the input follows r's path and then leaves every literal:
	/// those that begin with the whole path fail at d, the furthest, and the others fail
	/// short of it, where anything they record is replaced by the first at d. So the failure
	/// is the d-deep literals' expected arrays, at d, in their order. The last is left to
	/// <c>Fail:</c>, which moves a recovery's reach and backtracks as the last alternative
	/// would have.
	/// </para>
	/// <para>
	/// A literal that finds too little input records that it starved, at the choice. Where
	/// the group's longest literal could, the fallback decides, and nothing it would have
	/// said is lost.
	/// </para>
	/// </remarks>
	void EmitPrefixMiss(Writer helper, string name, IReadOnlyList<Node> alternatives, string[] prefixes, List<string> paths)
	{
		var depth   = new int[paths.Count];
		var longest = new int[paths.Count];
		var members = new List<int>[paths.Count];

		for (var r = 0; r < paths.Count; r++)
		{
			members[r] = [];

			if (r == 0)
				continue;

			depth[r]   = paths[r].Length;
			longest[r] = prefixes.Where(text => text[0] == paths[r][0]).Max(static text => text.Length);

			for (var i = 0; i < prefixes.Length; i++)
				if (prefixes[i].StartsWith(paths[r], StringComparison.Ordinal))
					members[r].Add(i);

			Debug.Assert(members[r].Count > 0, "A row is on the way to at least one literal.");
		}

		helper.Line($"private static readonly int[] {name}_Depth = new int[] {{ {string.Join(", ", depth)} }};");
		helper.Line($"private static readonly int[] {name}_Longest = new int[] {{ {string.Join(", ", longest)} }};");
		helper.Line($"private static readonly int[][] {name}_Members = new int[][]");
		using (helper.Block(""))
			foreach (var row in members)
				helper.Line($"new int[] {{ {string.Join(", ", row)} }},");
		helper.Line(";");

		// By a switch rather than an array of arrays: the expected arrays are static fields
		// of this class too, and initializers across its files run in no order it can rely on.
		using (helper.Block($"private static string[] {name}_Expected(int alternative)"))
		{
			using (helper.Block("switch (alternative)"))
				for (var i = 0; i < alternatives.Count; i++)
				{
					var array = DeclareExpected(Displays(PrefixLiteral(alternatives[i])!));

					_expectedUsed.Add(array);
					helper.Line($"case {i}: return {array};");
				}

			helper.Line("return null!;");
		}
	}

	void EmitPrefixFailure(Writer helper, string name)
	{
		var longest = $"{name}_Longest[row]";

		// True with the expected array to fail with and the position moved to where it is
		// reported; false where too little input is buffered to decide, and nothing is moved.
		using (helper.Block($"private static bool {name}_Miss({InputType} text, ref int p, int row, int lookahead, ref Failure failure, out string[] expected)"))
		{
			helper.Line("expected = null!;");
			helper.Line(BufferedInput ? $"if (!text.Ensure(p, {longest})) return false;" : $"if (text.Length - p < {longest}) return false;");
			helper.Line($"var missed = {name}_Members[row];");

			using (helper.Block("if (lookahead < 0)"))
			{
				helper.Line($"p += {name}_Depth[row];");

				using (helper.Block("for (var tie = 0; tie < missed.Length - 1; tie++)"))
				{
					helper.Line($"var said = {name}_Expected(missed[tie]);");
					helper.Line("if (p > failure.Position)");

					using (helper.Block(""))
					{
						helper.Line("failure.Position = p;");
						helper.Line("failure.Expected = said;");
						helper.Line("failure.ExpectedMore?.Clear();");
					}

					helper.Line("else if (p == failure.Position)");
					helper.Then("(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>()).Add(said);");
				}
			}

			helper.Line($"expected = {name}_Expected(missed[missed.Length - 1]);");
			helper.Line("return true;");
		}
	}
}
