using System;
using System.Collections.Generic;
using System.Linq;

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
		var seed = whole ? FollowSets.Continuation.End : FollowSets.Continuation.All;

		_roots.Clear();
		_seam = FollowSets.SeamOf(rule, _graph);

		var entry = Compile(BodyOf(rule, whole), Accept, seed);

		_roots.Add(entry);

		PlanLayout();

		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure)"))
		{
			file.Line("var p = pos;");

			if (UsesChar)
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

			// The same peephole RenderStates already applies between one state and the
			// next, applied to the jump in: falling into the first state written is what
			// this says, so where that state is the entry the line says nothing. The JIT
			// removes it either way — a jump to the block that follows is what basic-block
			// layout exists to fold, and the disassembly of this very method says so — but
			// the file is read by whoever the parser was generated for, and a line that
			// does nothing costs them a moment.
			//
			// The label goes with it where nothing else names the state, because C# warns
			// on a label nobody jumps to and this file is compiled in a build that may
			// treat that as an error.
			var first = _order.Count > 0 ? _order[0] + First : -1;
			var falls = first == Resolved(entry);

			if (!falls)
				file.Line($"goto {Label(Resolved(entry))};");

			RenderStates(file, dispatched: false);

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
			// Fail: makes, and there is no tie to accumulate either — a reference straight
			// into whichever array the generator already declared, nothing to allocate.
			file.Line("failure.Position = p;");
			file.Line("failure.Expected = expected;");
			file.Line("return -1;");
		}

		return file.ToString();
	}

	/// <summary>
	/// Whether a publication of a rule that builds a value can still be a plain method:
	/// captures kept as position locals, and the one construction run at Accept.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Everything <see cref="CanLower"/> asks, plus what a value adds. The captures must
	/// be spans of the input and nothing else — a capture of a rule's value collects an
	/// array, and every turn's record is the point — and none may sit under a repetition
	/// of more than one turn, where a local would keep only the last. The construction
	/// must be single and at the top, so that Accept knows the factory without a record,
	/// and it runs only there — after the whole parse is decided, which is the deferred
	/// construction §3 promises, kept without an arena to defer into.
	/// </para>
	/// <para>
	/// The silence question is then <see cref="Silent"/>'s own, asked with
	/// <see cref="_valuesInLocals"/> on — the same flag the rendering compiles under, so
	/// the two cannot disagree.
	/// </para>
	/// </remarks>
	public bool CanLowerValued(RuleSymbol rule, bool whole)
	{
		if (UsesInput ||
			!_graph.Types.ContainsKey(rule) ||
			_results.QualifiedOf(rule) is null ||
			_graph.Folds.ContainsKey(rule) ||
			_graph.Climbing.ContainsKey(rule) ||
			_graph.Recursive.Contains(rule))
			return false;

		if (_graph.Bodies[rule] is not Node.Construct(var built, _) construct)
			return false;

		if (_factories[rule] is not { Count: 1 } factories ||
			!ReferenceEquals(factories[0].Of, construct))
			return false;

		var factory = factories[0];

		if (CSharpEmitter.WantsText(factory) ||
			CSharpEmitter.Asks(factory, "parserSpan") ||
			CSharpEmitter.Asks(factory, "parserInput") ||
			factory.Accumulator is not null)
			return false;

		foreach (var member in factory.Members)
			if (member.Rule is not null || member.IsSequence || member.Slots.Count != 1)
				return false;

		if (!CapturesAreExtents(built, repeated: false))
			return false;

		_valuesInLocals = true;

		try
		{
			return Silent(BodyOf(rule, whole), whole ? FirstSets.First.End : FirstSets.First.All);
		}
		finally
		{
			_valuesInLocals = false;
		}
	}

	/// <summary>
	/// Every capture is a span two locals can hold: pure text below it, and no repetition
	/// above it that could run the locals over.
	/// </summary>
	bool CapturesAreExtents(Node node, bool repeated) =>
		node switch
		{
			Node.Capture(_, var captured)     => !repeated && Extent(captured),
			Node.Sequence(var parts)          => parts.All(part => CapturesAreExtents(part, repeated)),
			Node.Choice(var alternatives)     => alternatives.All(part => CapturesAreExtents(part, repeated)),
			Node.Repeat(var body, _, var max) => CapturesAreExtents(body, repeated || max != 1),
			Node.Atomic(var body)             => CapturesAreExtents(body, repeated),
			Node.Lookahead(_, var seen)       => NodeWalk.Descendants(seen).All(
			                                         static inner => inner is not Node.Capture),
			Node.Construct                    => false,
			Node.Guard                        => false,
			Node.External { HasValue: true }  => false,
			_                                 => true,
		};

	/// <summary>Matches text and could mean nothing else — the value is the extent.</summary>
	static bool Extent(Node node) =>
		node switch
		{
			Node.Empty or Node.Literal or Node.Element or Node.Behind => true,
			Node.Sequence(var parts)      => parts.All(Extent),
			Node.Choice(var alternatives) => alternatives.All(Extent),
			Node.Repeat(var body, _, _)   => Extent(body),
			Node.Atomic(var body)         => Extent(body),
			_                             => false,
		};

	/// <summary>
	/// The valued counterpart of <see cref="RenderFlat"/>: the same rendering, plus the
	/// capture locals and the one factory call at Accept.
	/// </summary>
	public string RenderFlatValued(RuleSymbol rule, string name, bool whole)
	{
		var seed    = whole ? FollowSets.Continuation.End : FollowSets.Continuation.All;
		var factory = _factories[rule][0];
		var type    = _results.QualifiedOf(rule);

		_roots.Clear();
		_seam           = FollowSets.SeamOf(rule, _graph);
		_valuesInLocals = true;

		var entry = Compile(BodyOf(rule, whole), Accept, seed);

		_valuesInLocals = false;

		_roots.Add(entry);

		PlanLayout();

		var slots = new SortedSet<int>();

		foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
			if (node is Node.Capture)
				slots.Add(_captureSlots[node]);

		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, out {type} value)"))
		{
			file.Line("var p = pos;");

			if (UsesChar)
				file.Line("var c = '\\0';");
			file.Line("string[]? expected = null;");

			// The sentinel start is what tells an optional capture that never ran from
			// one that matched nothing — the same convention the arena's materializer
			// reads out of a missing entry.
			foreach (var slot in slots)
			{
				file.Line($"var flat{slot}Start = -1;");
				file.Line($"var flat{slot}End = 0;");
			}

			var depths = new HashSet<int>();

			foreach (var turn in _turns)
				if (Written(turn.State))
					depths.Add(turn.Depth);

			for (var i = 0; i <= _depth + _turns.Count; i++)
				if (depths.Contains(i))
					file.Line($"var turn{i} = 0;");

			var first = _order.Count > 0 ? _order[0] + First : -1;
			var falls = first == Resolved(entry);

			if (!falls)
				file.Line($"goto {Label(Resolved(entry))};");

			RenderStates(file, dispatched: false);

			file.Line();
			file.Line("Accept:");
			if (whole)
				file.Line("if (p != text.Length) { expected = null; goto Fail; }");

			// The construction, deferred to here: the parse is decided, and only now does
			// anything the author wrote run. Argument order mirrors the arena
			// materializer's, which mirrors the factory's own parameters.
			var offset    = _captureOffsets[rule];
			var arguments = new List<string>();

			for (var index = 0; index < factory.Members.Count; index++)
			{
				var member = factory.Members[index];

				if (member.Name == "parserText")
					continue;

				var slot  = offset + member.Slots[0];
				var start = $"flat{slot}Start";
				var slice = $"text.Slice({start}, flat{slot}End - {start}).ToString()";

				file.Line(
					$"var captured{index} = {start} < 0 ? " +
					(member.IsOptional ? "null" : "string.Empty") + $" : {slice};");

				arguments.Add($"captured{index}");
			}

			file.Line($"value = {factory.Method}({string.Join(", ", arguments)});");
			file.Line("return p;");

			file.Line();
			file.Line("Fail:");
			file.Line("value = default!;");
			file.Line("failure.Position = p;");
			file.Line("failure.Expected = expected;");
			file.Line("return -1;");
		}

		return file.ToString();
	}
}
