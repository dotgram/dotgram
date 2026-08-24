using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A publication that needs none of the three things the arena is for, compiled as an
/// ordinary method instead of a state in the shared automaton.
/// </summary>
/// <remarks>
/// Reuses <see cref="Machine.Compile"/> and <see cref="Machine.PlanLayout"/> completely
/// unchanged — this is a different rendering of the same states, not a second compiler.
/// Safe only because the caller (<c>CSharpEmitter.Emit</c>) only reaches here when every
/// publication in the grammar is <see cref="Machine.CanLower"/>-eligible: this method
/// mutates <c>_roots</c> and re-runs layout for its own entry alone, which would corrupt
/// <see cref="Machine.RenderEngine"/>'s output if the two were ever asked of the same
/// instance. See docs/next.md, "Future optimization gate" — this is the lever it names.
/// </remarks>
sealed partial class Machine
{
	/// <summary>The recognizer itself: a plain method, no arena, no dispatch.</summary>
	public string RenderFlat(RuleSymbol rule, string name, bool whole)
	{
		var seed = whole ? FirstSets.First.End : FirstSets.First.All;

		_roots.Clear();

		var entry = Compile(BodyOf(rule, whole), Accept, seed);

		_roots.Add(entry);

		PlanLayout();

		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure)"))
		{
			file.Line("var p = pos;");

			if (_usesChar)
				file.Line("var c = '\\0';");
			file.Line("string[]? expected = null;");

			// One per possessive repetition written as a loop — the same locals
			// RenderEngine declares, for the same reason: settled only once the states that
			// might read one are known.
			var depths = new HashSet<int>();

			foreach (var turn in _turns)
				if (Written(turn.State))
					depths.Add(turn.Depth);

			for (var i = 0; i <= _depth + _turns.Count; i++)
				if (depths.Contains(i))
					file.Line($"var turn{i} = 0;");

			file.Line($"goto {Label(Resolved(entry))};");

			RenderStates(file);

			file.Line();
			file.Line("Accept:");
			if (whole)
				file.Line("if (p != text.Length) { expected = null; goto Fail; }");
			file.Line("return p;");

			file.Line();
			file.Line("Fail:");
			// Deterministic throughout, so there is only ever one attempt: wherever it gave
			// up is the furthest the input was followed, with nothing to compare it to —
			// so this is an unconditional assignment, not the max-comparison RenderEngine's
			// Fail: makes, and there is no tie to accumulate either.
			file.Line("failure.Position = p;");
			file.Line(
				"failure.Expected = expected is null ? null : " +
				"new global::System.Collections.Generic.List<string>(expected);");
			file.Line("return -1;");
		}

		return file.ToString();
	}

	/// <summary>
	/// The thin wrapper <c>CSharpEmitter.EmitPublication</c> calls — same name, same
	/// signature <see cref="RenderWrapper"/> would have produced, so the caller cannot tell
	/// which one it got.
	/// </summary>
	public string RenderFlatWrapper(RuleSymbol root, string name, string flatName)
	{
		var file   = new Writer(0);
		var type   = _results.QualifiedOf(root);
		var output = type is null ? "" : $", out {type} value";

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{output})"))
		{
			file.Line($"var end = {flatName}(text, pos, ref failure);");

			if (IsExtent(root))
				file.Line("value = end < 0 ? default : new SourceSpan(pos, end - pos);");
			else if (type is not null)
				// Not reachable: CanLower only admits a rule whose value is its own extent,
				// since silence already rules out every capture and construction a typed,
				// non-extent value would need.
				file.Line("value = default!;");

			file.Line("return end;");
		}

		return file.ToString();
	}
}
