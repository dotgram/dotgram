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
		_checkpoints.Clear();
		_namedOutside.Clear();
		_checkpointIds      = 0;
		_seam               = FollowSets.SeamOf(rule, _graph);
		_checkpointsAllowed = true;

		var entry = Compile(BodyOf(rule, whole), Accept, seed);

		_checkpointsAllowed = false;

		_roots.Add(entry);

		PlanLayout();

		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{ContextParameter})"))
		{
			file.Line("var p = pos;");

			if (UsesChar)
				file.Line("var c = '\\0';");
			file.Line("string[]? expected = null;");
			// Set where a room check fails and read where a failure is recorded, so
			// what it says is of the furthest failure and not of any (§7.5).

			// Three locals per checkpoint site, and the one that says which site is
			// innermost — the stack of open ways back, flattened into locals because a
			// site with no repetition over it holds at most one activation.
			if (_checkpoints.Count > 0)
			{
				file.Line("var pending = 0;");

				foreach (var site in _checkpoints.OrderBy(static site => site.Id))
				{
					file.Line($"var way{site.Id} = 0;");
					file.Line($"var alt{site.Id} = 0;");
					file.Line($"var over{site.Id} = 0;");
				}
			}

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
			{
				file.Line($"goto {Label(Resolved(entry))};");
				_namedOutside.Add(entry);
			}

			RenderStates(file, dispatched: false);

			file.Line();
			file.Line("Accept:");
			if (whole)
				file.Line("if (p != text.Length) { expected = null; goto Fail; }");
			file.Line("return p;");

			file.Line();
			file.Line("Fail:");

			if (_checkpoints.Count == 0)
			{
				// Deterministic throughout, so there is only ever one attempt: wherever it
				// gave up is the furthest the input was followed, with nothing to compare
				// it to — so this is an unconditional assignment, not the max-comparison
				// RenderEngine's Fail: makes, and there is no tie to accumulate either — a
				// reference straight into whichever array the generator already declared,
				// nothing to allocate.
				file.Line("failure.Position = p;");
				file.Line("failure.Expected = expected;");
					file.Line("return -1;");
			}
			else
			{
				// Checkpoint sites make this the engine's Fail: without the engine. Every
				// failure is recorded against the furthest one seen — the max-comparison
				// RenderEngine makes, ties added rather than replaced — and then the
				// innermost open site resumes its next alternative, or closes and hands
				// the failure to the site it opened over, until none is open.
				file.Line("if (p > failure.Position)");
				using (file.Block(""))
				{
					file.Line("failure.Position = p;");
					file.Line("failure.Expected = expected;");
					file.Line("failure.ExpectedMore = null;");
				}
				using (file.Block("else if (p == failure.Position && expected != null)"))
				{
					file.Line(
						"(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>())" +
						".Add(expected);");
				}
								file.Line();
				file.Line("Resume:");
				using (file.Block("switch (pending)"))
					foreach (var site in _checkpoints.OrderBy(static site => site.Id))
					{
						file.Line($"case {site.Id}:");
						using (file.Indent())
						{
							for (var at = 0; at < site.Retries.Count; at++)
								file.Line(
									$"if (alt{site.Id} == {at + 1}) " +
									$"goto {Label(Resolved(site.Retries[at]))};");
							file.Line($"pending = over{site.Id};");
							file.Line("goto Resume;");
						}
					}
				file.Line("return -1;");
			}
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
		if (UsesInput || ReadsState || !FlatValued(rule))
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
	/// The structural half of <see cref="CanLowerValued"/>, and the whole of what a rule
	/// must be to have its value built inside another flat method: constructions that are
	/// the body — one at the top, or one per alternative, told apart at Accept by a tag
	/// local — over captures that are spans of the input or sites of further such rules.
	/// </summary>
	/// <remarks>
	/// Memoized, and seeded false before computing: rules in a call cycle are already
	/// refused by <see cref="RecognitionGraph.Recursive"/>, so the seed only ever answers
	/// a query the exclusion makes unreachable, and it answers it safely.
	/// </remarks>
	bool FlatValued(RuleSymbol rule)
	{
		if (_flatValued.TryGetValue(rule, out var known))
			return known;

		_flatValued[rule] = false;

		var answer = ComputeFlatValued(rule);

		_flatValued[rule] = answer;

		return answer;
	}

	readonly Dictionary<RuleSymbol, bool> _flatValued = [];

	bool ComputeFlatValued(RuleSymbol rule)
	{
		if (!_graph.Types.ContainsKey(rule) ||
			_results.QualifiedOf(rule) is null ||
			_graph.Folds.ContainsKey(rule) ||
			_graph.Climbing.ContainsKey(rule) ||
			_graph.Recursive.Contains(rule) ||
			!_graph.Bodies.TryGetValue(rule, out var body) ||
			!_factories.TryGetValue(rule, out var factories) ||
			factories.Count == 0)
			return false;

		// The constructions must be the whole of the body, in the order the factories
		// were gathered — which is document order, the same walk.
		IReadOnlyList<Node> constructs = body is Node.Choice(var alternatives)
			? alternatives
			: new[] { body };

		if (constructs.Count != factories.Count)
			return false;

		for (var i = 0; i < constructs.Count; i++)
			if (constructs[i] is not Node.Construct || !ReferenceEquals(factories[i].Of, constructs[i]))
				return false;

		foreach (var factory in factories)
		{
			// What this rendering can hand a construction is said once, in `Renderings.cs`,
			// and read here rather than listed again. The list is what kept going out of
			// date: a name added to the signature and not to one of these was a call short
			// an argument, in somebody else's build.
			if (!Renderings.Supplies(Renderings.Rendering.Flat, factory, _graph) ||
				factory.Accumulator is not null)
				return false;

			foreach (var member in factory.Members)
				if (member.IsSequence)
					return false;
		}

		foreach (var construct in constructs)
			if (!CapturesAreExtents(((Node.Construct)construct).Body, repeated: false))
				return false;

		return true;
	}

	/// <summary>
	/// The rule whose value a capture takes from a call, where that call can be built
	/// flat in place — or null where the capture is not that shape.
	/// </summary>
	/// <remarks>
	/// The seam has to match the capturing rule's: an inlined body composes its
	/// continuations against its own namespace's trivia, and a crossing would degrade
	/// them to "anything", which is exactly what the silence proofs cannot survive.
	/// </remarks>
	RuleSymbol? SiteCallee(Node node) =>
		node is Node.Capture(_, Node.Call(var called, { Count: 0 })) &&
		FlatValued(called) &&
		_owners.TryGetValue(node, out var owner) &&
		ReferenceEquals(FollowSets.SeamOf(called, _graph), FollowSets.SeamOf(owner, _graph))
			? called
			: null;

	/// <summary>
	/// Every capture is a span two locals can hold — pure text below it, or a flat-valued
	/// call — and no repetition above it that could run the locals over.
	/// </summary>
	bool CapturesAreExtents(Node node, bool repeated) =>
		node switch
		{
			Node.Capture(_, var captured)     => !repeated &&
			                                     (Extent(captured) || SiteCallee(node) is not null),
			Node.Sequence(var parts)          => parts.All(part => CapturesAreExtents(part, repeated)),
			Node.Choice(var alternatives)     => alternatives.All(part => CapturesAreExtents(part, repeated)),
			Node.Repeat(var body, _, var max) => CapturesAreExtents(body, repeated || max != 1),
			Node.Atomic(var body)             => CapturesAreExtents(body, repeated),
			Node.Marked(var body, _)          => CapturesAreExtents(body, repeated),

			Node.Lookahead(_, var seen)       => NodeWalk.Descendants(seen).All(
			                                         static inner => inner is not Node.Capture),
			Node.Construct                    => false,
			Node.Guard                        => false,
			Node.External { HasValue: true }  => false,
			_                                 => true,
		};

	/// <summary>Matches text and could mean nothing else — the value is the extent.</summary>
	/// <remarks>
	/// A call to a valueless rule belongs: what it matched is text, and text is the whole
	/// of what capturing it can mean (§4.1 case 4). The flat path still gates every such
	/// capture through <see cref="Silent"/>, which is what refuses the calls it cannot
	/// compile without an arena; a sited capture needs no such gate — its records unwind.
	/// </remarks>
	bool Extent(Node node) =>
		node switch
		{
			Node.Empty or Node.Literal or Node.Element or Node.Behind => true,
			Node.Sequence(var parts)      => parts.All(Extent),
			Node.Choice(var alternatives) => alternatives.All(Extent),
			Node.Repeat(var body, _, _)   => Extent(body),
			Node.Atomic(var body)         => Extent(body),
			Node.Call(var called, _)      => _graph.Results[called].Count == 0 &&
			                                 !_graph.Types.ContainsKey(called),
			_                             => false,
		};

	/// <summary>A capture whose value another flat-valued rule builds, compiled in place.</summary>
	/// <param name="Id">The instance its own capture locals are named under.</param>
	/// <param name="Rule">Whose body was compiled there.</param>
	/// <param name="Parent">The instance the capturing slot belongs to.</param>
	/// <param name="Slot">The capturing slot — its sentinel says whether the site ran.</param>
	sealed record FlatSite(int Id, RuleSymbol Rule, int Parent, int Slot);

	readonly List<FlatSite> _flatSites = [];
	readonly HashSet<(int Instance, int Slot, bool Valued)> _flatLocals = [];
	readonly HashSet<int> _flatTags = [];
	readonly Dictionary<int, RuleSymbol> _flatRuleOf = [];
	int _flatInstance;
	int _flatInstances;

	/// <summary>
	/// The valued counterpart of <see cref="RenderFlat"/>: the same rendering, plus the
	/// capture locals and the factory calls at Accept — inner sites before the rules that
	/// captured them, the root last, each site guarded by its capturing slot's sentinel.
	/// </summary>
	public string RenderFlatValued(RuleSymbol rule, string name, bool whole)
	{
		var seed = whole ? FollowSets.Continuation.End : FollowSets.Continuation.All;
		var type = _results.QualifiedOf(rule);

		_roots.Clear();
		_checkpoints.Clear();
		_namedOutside.Clear();
		_flatSites.Clear();
		_flatLocals.Clear();
		_flatTags.Clear();
		_flatRuleOf.Clear();
		_flatInstance   = 0;
		_flatInstances  = 1;
		_flatRuleOf[0]  = rule;
		_seam           = FollowSets.SeamOf(rule, _graph);
		_valuesInLocals = true;

		var entry = Compile(BodyOf(rule, whole), Accept, seed);

		_valuesInLocals = false;

		_roots.Add(entry);

		PlanLayout();

		var file = new Writer(0);

		using (file.Block(
			$"static int {name}(global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, out {type} value{ContextParameter})"))
		{
			file.Line("var p = pos;");

			if (UsesChar)
				file.Line("var c = '\\0';");
			file.Line("string[]? expected = null;");
			// Set where a room check fails and read where a failure is recorded, so
			// what it says is of the furthest failure and not of any (§7.5).

			// The sentinel start is what tells an optional capture that never ran from
			// one that matched nothing — the same convention the arena's materializer
			// reads out of a missing entry.
			foreach (var (instance, slot, valued) in _flatLocals.OrderBy(static local => (local.Instance, local.Slot)))
			{
				file.Line($"var flat{instance}_{slot}Start = -1;");

				if (!valued)
					file.Line($"var flat{instance}_{slot}End = 0;");
			}

			foreach (var tagged in _flatTags.OrderBy(static tag => tag))
				file.Line($"var flatWhich{tagged} = -1;");

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
			{
				file.Line($"goto {Label(Resolved(entry))};");
				_namedOutside.Add(entry);
			}

			RenderStates(file, dispatched: false);

			file.Line();
			file.Line("Accept:");
			if (whole)
				file.Line("if (p != text.Length) { expected = null; goto Fail; }");

			// The constructions, deferred to here: the parse is decided, and only now
			// does anything the author wrote run. Inner sites first — a child's id is
			// always above its parent's, so reverse order is dependency order.
			for (var at = _flatSites.Count - 1; at >= 0; at--)
			{
				var site = _flatSites[at];

				file.Line($"{_results.QualifiedOf(site.Rule)} value{site.Id} = default!;");

				using (file.Block($"if (flat{site.Parent}_{site.Slot}Start >= 0)"))
					EmitFlatConstruction(file, site.Id, site.Rule, $"value{site.Id}");
			}

			if (_factories[rule].Count > 1)
				file.Line("value = default!;");

			EmitFlatConstruction(file, 0, rule, "value");
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

	/// <summary>One rule's value, from its locals — a switch on the tag where it has one.</summary>
	void EmitFlatConstruction(Writer file, int instance, RuleSymbol rule, string target)
	{
		var factories = _factories[rule];

		if (factories.Count == 1)
		{
			EmitFlatFactoryCall(file, instance, rule, factories[0], target);

			return;
		}

		using (file.Block($"switch (flatWhich{instance})"))
			for (var index = 0; index < factories.Count; index++)
			{
				file.Line($"case {index}:");

				// Braced: the sections of one switch share a declaration scope, and
				// every case declares its captures under the same names.
				using (file.Indent())
				using (file.Block(""))
				{
					EmitFlatFactoryCall(file, instance, rule, factories[index], target);
					file.Line("break;");
				}
			}
	}

	/// <summary>
	/// One factory call. Argument order mirrors the arena materializer's, which mirrors
	/// the factory's own parameters; a member captured in more than one alternative reads
	/// the slot that ran, first written first.
	/// </summary>
	void EmitFlatFactoryCall(Writer file, int instance, RuleSymbol rule, Factory factory, string target)
	{
		var offset    = _captureOffsets[rule];
		var arguments = new List<string>();

		// The grammar's own state, where the factory names it (§7.7). Nothing else supplied
		// can reach here — `ComputeFlatValued` refuses a factory wanting the matched text,
		// the span or the input, and `CanLowerValued` refuses a machine that reads marks —
		// so this is the whole of what a flat construction is handed beyond its captures.
		// It was missing, and the two halves disagreed in silence: the publication passed a
		// context the recognizer did not take, and the recognizer called a factory without
		// the one it did.
		if (UsesContext && CSharpEmitter.Asks(factory, "context"))
			arguments.Add("context");

		for (var index = 0; index < factory.Members.Count; index++)
		{
			var member = factory.Members[index];

			if (member.Name == "parserText")
				continue;

			var local = $"captured{instance}_{index}";

			if (member.Rule is null)
			{
				var expression = member.IsOptional ? "null" : "string.Empty";

				for (var at = member.Slots.Count - 1; at >= 0; at--)
				{
					var slot  = offset + member.Slots[at];
					var start = $"flat{instance}_{slot}Start";

					expression =
						$"{start} < 0 ? {expression} : " +
						$"text.Slice({start}, flat{instance}_{slot}End - {start}).ToString()";
				}

				file.Line($"var {local} = {expression};");
			}
			else
			{
				var valueType  = _results.ValueOf(member.Rule);
				var expression = member.IsOptional || member.Slots.Count > 1
					? member.IsOptional ? $"default({valueType}?)" : SiteValue(instance, offset + member.Slots[member.Slots.Count - 1])
					: SiteValue(instance, offset + member.Slots[0]);

				var from = member.IsOptional ? member.Slots.Count - 1 : member.Slots.Count - 2;

				for (var at = from; at >= 0; at--)
				{
					var slot = offset + member.Slots[at];

					expression =
						$"flat{instance}_{slot}Start < 0 ? {expression} : {SiteValue(instance, slot)}";
				}

				file.Line(member.IsOptional
					? $"{valueType}? {local} = {expression};"
					: $"var {local} = {expression};");
			}

			arguments.Add(local);
		}

		file.Line($"{target} = {factory.Method}({string.Join(", ", arguments)});");
	}

	/// <summary>The value local of the site a capturing slot was compiled as.</summary>
	string SiteValue(int instance, int slot)
	{
		foreach (var site in _flatSites)
			if (site.Parent == instance && site.Slot == slot)
				return "value" + site.Id;

		throw new InvalidOperationException("A rule capture has no compiled site.");
	}
}
