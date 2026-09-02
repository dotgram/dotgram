using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A publication compiled as one method per rule, with the automaton nowhere near it.
/// </summary>
/// <remarks>
/// <para>
/// The engine exists so that a rule can be resumed after it has returned — an
/// alternative inside a callee taken up again once the caller has failed past it. That is
/// what the arena holds, and it is what every rule call pays for, whether or not anything
/// ever comes back: a frame written and rewritten, two passes through the dispatcher, the
/// locals of the whole machine living in memory because the states are local functions.
/// Measured against a hand-written recursive descent of the same shape it is five to
/// twenty times slower (docs/next.md).
/// </para>
/// <para>
/// Here a rule is a C# method and a call is a call. What the arena kept is kept in two
/// places instead. Backtracking that stays inside one construct — a sequence that fails
/// in its third part, a choice whose first alternative does not fit — restores the
/// position from a local and carries on, exactly as a hand-written parser does.
/// Backtracking that has to reach <em>into</em> something already finished — a choice
/// that matched, and then the text after it did not — is served by a tape of the ways
/// back still open: each choice that could be taken differently, each repetition that
/// could have stopped a turn earlier, records one small entry. On failure the innermost
/// construct that owns an open way advances it and runs itself again from its own start,
/// replaying the tape up to the way it changed. Nothing is resumed in the middle; what is
/// re-executed is only the construct that failed, and nothing outside it moves.
/// </para>
/// <para>
/// The semantics are the automaton's to the letter: alternatives are tried in order, the
/// innermost way back is taken first, and a repetition gives back its latest turn before
/// an earlier one. What differs is the cost. A grammar the proofs settle — choices told
/// apart by one character, repetitions nothing after them can begin — writes no way back
/// at all and runs at the speed of the calls; only the constructs the proofs cannot
/// settle pay for their tape entries, and only a failure ever reads them.
/// </para>
/// <para>
/// Values are recorded rather than built while matching, as §7.2 requires, but into a log
/// of records rather than an arena of entries (<c>Machine.Direct.Values.cs</c>). What this
/// rendering does not do yet, and hands back to the engine: folds, guards, marks, a
/// context, recovery, streaming, climbing, <c>find</c>. <see cref="CanDirect"/> is the
/// gate, and it refuses rather than guesses.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// Whether every publication in a group can be written as methods.
	/// </summary>
	public bool CanDirect(IReadOnlyList<Publication> publications)
	{
		if (publications.Count == 0)
			return false;

		foreach (var publication in publications)
			if (publication.Kind != PublishKind.Parse || !DirectReachable(publication.Rule))
				return false;

		// A guard handed a value builds it from the log while the text is read, and a
		// factory that asks for the input would have to be handed it there: not yet.
		DirectGuardNeeds(DirectRules(publications));

		return !(_directBuilds && UsesInput);
	}

	bool DirectReachable(RuleSymbol root)
	{
		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!seen.Add(rule))
				continue;

			if (_graph.Climbing.ContainsKey(rule) || _graph.Externals.ContainsKey(rule) ||
				!_graph.Bodies.TryGetValue(rule, out var body) || !DirectValuedRule(rule))
			{
				return false;
			}

			var bodies = new List<Node> { body };

			if (_graph.Trivia.TryGetValue(rule, out var seam))
				bodies.Add(seam);

			foreach (var one in bodies)
				foreach (var node in NodeWalk.Descendants(one))
				{
					if (_graph.Recoveries.ContainsKey(node))
						return false;

					switch (node)
					{
						case Node.Empty or Node.Literal or Node.Element or Node.Sequence or Node.Choice
							or Node.Repeat or Node.Lookahead or Node.Behind or Node.Atomic:
							break;

						case Node.External { HasValue: false }:
							break;

						// What a lookahead saw is a capture the engine compiles as a machine
						// of its own; not here yet.
						case Node.Capture(_, Node.Lookahead):
							return false;

						case Node.Capture or Node.Construct or Node.Marked:
							break;

						case Node.Guard guard:
							if (!DirectGuard(rule, guard))
								return false;

							break;

						case Node.Call(var called, var arguments):
							if (arguments.Count > 0)
								return false;

							pending.Push(called);
							break;

						default:
							return false;
					}
				}
		}

		return true;
	}

	/// <summary>
	/// Whether a guard can be run by a reader: what it names has to be something the
	/// reader's locals can hand it, and a capture repeated inside a loop is not — its
	/// pieces are on the side stack, gathered only when the rule ends.
	/// </summary>
	bool DirectGuard(RuleSymbol rule, Node.Guard guard)
	{
		if (CSharpEmitter.Uses(_graph, guard.Text, "parserInput"))
			return false;

		foreach (var (member, _) in GuardMembers(rule, guard))
			if (member.Rule is null && DirectRepeated(rule).Overlaps(member.Slots))
				return false;

		return true;
	}

	/// <summary>
	/// The members a guard is handed: those captured before it that its condition names,
	/// each with the slots that stand before it. Read as text, as the engine reads it — a
	/// name inside a string literal costs one value built for nothing.
	/// </summary>
	List<(ResultMember Member, IReadOnlyList<int> Slots)> GuardMembers(RuleSymbol rule, Node.Guard guard)
	{
		var layout = CaptureLayout.Of(
			_graph.Bodies[rule],
			other => _graph.Results[other].Count > 0 || _graph.Types.ContainsKey(other),
			_graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null);
		var before  = layout.Before(guard);
		var visible = new List<(ResultMember, IReadOnlyList<int>)>();

		foreach (var member in _graph.Results[rule])
		{
			var slots = new List<int>();

			foreach (var slot in member.Slots)
				if (slot < before)
					slots.Add(slot);

			if (slots.Count == 0 || !guard.Text.Contains(ResultTypes.ParameterOf(member)))
				continue;

			var optional = member.IsOptional || slots.Count != member.Slots.Count;

			visible.Add((member with { IsOptional = optional }, slots));
		}

		return visible;
	}

	/// <summary>Whether any guard the readers run is handed a value, which the reader then builds from the log.</summary>
	bool _directBuilds;

	/// <summary>Whether any guard the readers run names the context.</summary>
	bool _directGuardContext;

	/// <summary>Whether the readers run a guard at all.</summary>
	bool _directGuards;

	void DirectGuardNeeds(IReadOnlyList<RuleSymbol> rules)
	{
		_directBuilds = _directGuardContext = _directGuards = false;

		foreach (var rule in rules)
			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
				if (node is Node.Guard guard)
				{
					_directGuards = true;

					if (_graph.ContextOf(rule) is not null && CSharpEmitter.Uses(_graph, guard.Text, "context"))
						_directGuardContext = true;

					foreach (var (member, _) in GuardMembers(rule, guard))
						if (member.Rule is not null)
							_directBuilds = true;
				}
	}

	/// <summary>Whether the readers carry the context: a guard names it, or a guard builds a value whose factory might.</summary>
	bool DirectReaderContext => UsesContext && (_directGuardContext || _directBuilds);

	/// <summary>What a reader takes beyond the text, the position, the failure and the tape.</summary>
	string DirectReaderParameters =>
		(_directBuilds ? ", DirectValues values" : "") +
		(_directGuards && OverKinds ? TokensParameter : "") +
		(DirectReaderContext ? ContextParameter : "");

	string DirectReaderArguments =>
		(_directBuilds ? ", values" : "") +
		(_directGuards && OverKinds ? TokensArgument : "") +
		(DirectReaderContext ? ContextArgument : "");

	/// <summary>What the entry's own reader takes: the tokens whenever there are tokens, and what the readers take.</summary>
	string DirectCoreParameters =>
		TokensParameter +
		(_directBuilds ? ", DirectValues values" : "") +
		(DirectReaderContext ? ContextParameter : "");

	string DirectCoreArguments =>
		TokensArgument +
		(_directBuilds ? ", values" : "") +
		(DirectReaderContext ? ContextArgument : "");

	/// <summary>
	/// Whether a rule the plan writes in place may be written in place here: one with a
	/// guard or a mark in it may not, because both need the rule's own start.
	/// </summary>
	bool DirectInlinable(RuleSymbol rule) =>
		!NodeWalk.Descendants(_graph.Bodies[rule]).Any(static node => node is Node.Guard or Node.Marked);

	/// <summary>
	/// Whether a rule may be written in place wherever a method has room for it: one
	/// that keeps no value and captures nothing, so that its body means the same thing
	/// wherever it stands — the plan's rules, and the recursive ones the plan refuses
	/// because the engine would need a frame for them.
	/// </summary>
	bool DirectCopyable(RuleSymbol rule)
	{
		if (_directCopyable.TryGetValue(rule, out var known))
			return known;

		var copyable =
			!Valued(rule) && _graph.Results[rule].Count == 0 &&
			!_graph.Climbing.ContainsKey(rule) && !_graph.Externals.ContainsKey(rule) &&
			_graph.Bodies.TryGetValue(rule, out var body) &&
			!NodeWalk.Descendants(body).Any(node =>
				node is Node.Guard or Node.Marked or Node.Capture or Node.Construct or Node.Call { Arguments.Count: > 0 } ||
				_graph.Recoveries.ContainsKey(node));

		_directCopyable[rule] = copyable;

		return copyable;
	}

	readonly Dictionary<RuleSymbol, bool> _directCopyable = [];

	/// <summary>
	/// What a rule's body costs in branches written with everything under it called — a
	/// floor for what it costs written in place, so a body that cannot fit is not written
	/// into a buffer at every site only to be thrown away.
	/// </summary>
	int DirectCost(RuleSymbol rule)
	{
		if (!_directCost.TryGetValue(rule, out var cost))
		{
			var seam = _seam;

			_seam = FollowSets.SeamOf(rule, _graph);

			// Measured and discarded: nothing this rendering registers is a reader wanted.
			var wanted = new List<RuleSymbol>(_readersWanted);

			cost = Branches(new DirectWriter(this) { Inline = false }.Render(_graph.Bodies[rule], FollowOf(rule), whole: false, rule));

			_readersWanted.Clear();
			_readersWanted.UnionWith(wanted);
			_seam = seam;
			_directCost[rule] = cost;
		}

		return cost;
	}

	readonly Dictionary<RuleSymbol, int> _directCost = [];

	/// <summary>
	/// The capture slots of a rule that a turn of a loop writes again — a fold's loop
	/// excepted, whose turns are steps that each consume what they captured (§4.3).
	/// </summary>
	HashSet<int> DirectRepeated(RuleSymbol rule)
	{
		if (_directRepeated.TryGetValue(rule, out var known))
			return known;

		var found   = new HashSet<int>();
		var offset  = _captureOffsets[rule];
		var loop    = _graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null;
		var pending = new Stack<(Node Node, bool Inside)>();

		pending.Push((_graph.Bodies[rule], false));

		while (pending.Count > 0)
		{
			var (node, inside) = pending.Pop();

			if (inside && node is Node.Capture && _captureSlots.TryGetValue(node, out var slot))
				found.Add(slot - offset);

			var loops = node is Node.Repeat(_, _, var most) && most != 1 && !ReferenceEquals(node, loop);

			foreach (var child in node.Children)
				pending.Push((child, inside || loops));
		}

		_directRepeated[rule] = found;

		return found;
	}

	readonly Dictionary<RuleSymbol, HashSet<int>> _directRepeated = [];

	/// <summary>
	/// Whether a rule's alternatives can each be read by a method of its own: the body is
	/// a choice, not a fold's, and every alternative builds its own value or none does —
	/// a record written where the rule ends would need the alternatives' captures, which
	/// would then be locals of another method.
	/// </summary>
	bool DirectSplittable(RuleSymbol rule)
	{
		if (_graph.Folds.ContainsKey(rule) || _graph.Bodies[rule] is not Node.Choice(var alternatives) || alternatives.Count < 2)
			return false;

		if (!Valued(rule))
			return _graph.Results[rule].Count == 0;

		return alternatives.All(EndsInConstructs);
	}

	/// <summary>
	/// Whether every reading of a node ends by writing a record: a construct, or a
	/// sequence ending in a choice of them — the shape a shared head leaves behind
	/// (GrammarNormalizer.Factoring.cs), with the head's captures and the constructs that
	/// consume them together in one method.
	/// </summary>
	static bool EndsInConstructs(Node node) =>
		node switch
		{
			Node.Construct                       => true,
			Node.Sequence(var parts)             => parts.Count > 0 && EndsInConstructs(parts[parts.Count - 1]),
			Node.Choice(var alternatives)        => alternatives.All(EndsInConstructs),
			Node.Atomic(var kept)                => EndsInConstructs(kept),
			_                                    => false,
		};

	/// <summary>The rules a group of publications reaches, in a stable order.</summary>
	List<RuleSymbol> DirectRules(IReadOnlyList<Publication> publications)
	{
		var seen  = new HashSet<RuleSymbol>();
		var order = new List<RuleSymbol>();

		void Reach(RuleSymbol rule)
		{
			if (!seen.Add(rule))
				return;

			order.Add(rule);

			var bodies = new List<Node> { _graph.Bodies[rule] };

			if (_graph.Trivia.TryGetValue(rule, out var seam))
				bodies.Add(seam);

			foreach (var one in bodies)
				foreach (var node in NodeWalk.Descendants(one))
					if (node is Node.Call(var called, _))
						Reach(called);
		}

		foreach (var publication in publications)
			Reach(publication.Rule);

		return order;
	}

	/// <summary>The method a rule is read by, tagged like everything else this machine writes.</summary>
	string ReaderOf(RuleSymbol rule) => "Read_" + CSharpEmitter.IdentifierOf(rule) + _tag;

	/// <summary>The whole rendering: an entry per publication, a reader per rule reached, and the materializer.</summary>
	public string RenderDirect(IReadOnlyList<Publication> publications)
	{
		var file    = new Writer(0);
		var entries = new HashSet<RuleSymbol>();
		var rules   = DirectRules(publications);

		BackEdges(publications);
		DirectGuardNeeds(rules);

		// Cleared before the entries: an entry's call to a rule written in place that
		// still needs a reader is a wanted reader too.
		_readersWanted.Clear();

		foreach (var publication in publications)
		{
			if (!entries.Add(publication.Rule))
				continue;

			RenderDirectEntry(file, publication.Rule);
		}

		// Every rule with a boundary gets a reader, and so does a rule written in place that
		// some reader over the budget chose to call instead — which is only known once that
		// reader is rendered, so the queue grows as it is drained.
		var pending  = new Queue<RuleSymbol>(rules.Where(rule => !CanInline(rule)));
		var rendered = new HashSet<RuleSymbol>();

		foreach (var wanted in _readersWanted)
			pending.Enqueue(wanted);

		_readersWanted.Clear();

		while (pending.Count > 0)
		{
			var rule = pending.Dequeue();

			if (!rendered.Add(rule))
				continue;

			_seam = FollowSets.SeamOf(rule, _graph);

			var writer = new DirectWriter(this);
			var body   = writer.Render(_graph.Bodies[rule], FollowOf(rule), whole: false, rule);

			// Over the budget with its helpers written in place: written again with them
			// called, which is what keeps the JIT optimizing the method (Machine.Sizes.cs).
			if (Branches(body) > Budget)
			{
				writer = new DirectWriter(this) { Inline = false };
				body   = writer.Render(_graph.Bodies[rule], FollowOf(rule), whole: false, rule);
			}

			// Still over it with nothing left to call: a choice of many alternatives, each
			// building its own value — Primary in an expression language. Each alternative
			// becomes a method of its own, called where it stood, and the choice keeps only
			// the dispatch. An alternative is a body like any rule's: it begins where the
			// rule began, and what it records it records itself.
			var parts = new List<(string Name, string Body)>();

			if (Branches(body) > Budget && DirectSplittable(rule))
			{
				var alternatives = ((Node.Choice)_graph.Bodies[rule]).Nodes;
				var named        = new Dictionary<Node, string>(NodeIdentity.Instance);

				for (var i = 0; i < alternatives.Count; i++)
					named[alternatives[i]] = ReaderOf(rule) + "_Part" + i;

				writer = new DirectWriter(this) { Inline = false, Parts = named };
				body   = writer.Render(_graph.Bodies[rule], FollowOf(rule), whole: false, rule);

				foreach (var alternative in alternatives)
					parts.Add((
						named[alternative],
						new DirectWriter(this) { Inline = false }.Render(alternative, FollowOf(rule), whole: false, rule)));
			}

			file.Line($"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>");

			using (file.Block(
				$"static int {ReaderOf(rule)}(" +
				$"global::System.ReadOnlySpan<char> text, int pos, " +
				$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
			{
				file.Write(body);
			}

			file.Line();

			foreach (var (name, part) in parts)
			{
				file.Line($"/// <summary>One alternative of <c>{rule.Name}</c>, read where it stood.</summary>");

				using (file.Block(
					$"static int {name}(" +
					$"global::System.ReadOnlySpan<char> text, int pos, " +
					$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
				{
					file.Write(part);
				}

				file.Line();
			}

			foreach (var wanted in _readersWanted)
				if (!rendered.Contains(wanted))
					pending.Enqueue(wanted);

			_readersWanted.Clear();
		}

		if (rules.Any(Valued))
		{
			file.Write(RenderDirectMaterializer(rules));
			file.Line();
		}

		return file.ToString();
	}

	/// <summary>
	/// The entry a publication's wrapper calls: reads the whole input, on a deeper stack if
	/// this one runs out, and builds the value where there is one.
	/// </summary>
	void RenderDirectEntry(Writer file, RuleSymbol rule)
	{
		_seam = FollowSets.SeamOf(rule, _graph);

		// A call and not the body: the rule's record is the root value, and only the rule's
		// own reader writes it.
		Node body = _graph.Trivia.TryGetValue(rule, out var seam)
			? new Node.Sequence([seam, new Node.Call(rule, []), seam])
			: new Node.Call(rule, []);
		var core   = CSharpEmitter.MethodOf(rule) + "_Read";
		var type   = _results.QualifiedOf(rule);
		var valued = type is not null;
		var value  = valued ? $", out {type} value" : "";

		file.Line($"/// <summary>The whole input as <c>{rule.Name}</c>, read by methods.</summary>");

		// The parameters in the order the wrapper hands them: the value, the input, the
		// tokens, the context (CSharpEmitter.EmitPublication).
		using (file.Block(
			$"static int {CSharpEmitter.MethodOf(rule)}_Whole(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{value}{InputParameter}{TokensParameter}{ContextParameter})"))
		{
			file.Line($"var ways = {WaysType}.Rent();");

			if (valued || _directBuilds)
				file.Line("var values = DirectValues.Rent();");

			file.Line();
			file.Line("try");

			using (file.Block(""))
			{
				file.Line("int end;");
				file.Line();
				file.Line("try");

				using (file.Block(""))
					file.Line($"end = {core}(text, pos, ref failure, ways{DirectCoreArguments});");

				// An input nested deeper than this thread's stack allows is read again on a
				// thread with a deep one. The span cannot cross to another thread, so the
				// input is copied — once, on the one path that is about to reserve a stack
				// many times its size anyway.
				file.Line("catch (global::System.InsufficientExecutionStackException)");

				using (file.Block(""))
				{
					// Copied into locals of this block so that the closure lives here: a lambda
					// capturing a parameter would have its display class made at the entry
					// of the method, on every call, for a catch that almost never runs.
					file.Line("var from   = pos;");

					if (OverKinds)
					{
						file.Line("var lexedSource  = parserSource;");
						file.Line("var lexedStarts  = parserStarts;");
						file.Line("var lexedLengths = parserLengths;");
					}

					if (_directBuilds)
						file.Line("var built  = values;");

					if (DirectReaderContext)
						file.Line("var held   = context;");

					file.Line("var copied = text.ToArray();");
					file.Line("var deep   = failure;");
					file.Line($"var deeper = {WaysType}.Rent();");
					file.Line("var got    = -1;");
					file.Line("var reader = new global::System.Threading.Thread(");
					file.Line(
						$"\t() => got = {core}(copied, from, ref deep, deeper{TokensLocals}" +
						$"{(_directBuilds ? ", built" : "")}{(DirectReaderContext ? ", held" : "")}),");
					file.Line($"\t{DeepStack});");
					file.Line();
					file.Line("reader.Start();");
					file.Line("reader.Join();");
					file.Line("failure = deep;");
					file.Line("ways    = deeper;");
					file.Line("end     = got;");
				}

				file.Line();

				if (valued)
				{
					using (file.Block("if (end < 0)"))
					{
						file.Line("value = default!;");
						file.Line();
						file.Line("return end;");
					}

					file.Line();
					file.Line($"{DirectMaterializer}(ways, text, values, ways.Last, 0{InputArgument}{TokensArgument}{ContextArgument});");
					file.Line(
						$"value = {(IsExtent(rule) ? RecordValue(type!, "ways.Last").Replace("log[", "ways.Log[") : ValueFrom(type!, "ways.Last").Replace("values", "values.V"))};");
					file.Line();
				}

				file.Line("return end;");
			}

			file.Line("finally");

			using (file.Block(""))
			{
				file.Line($"{WaysType}.Return(ways);");

				if (valued || _directBuilds)
					file.Line("DirectValues.Return(values);");
			}
		}

		file.Line();
		file.Line($"/// <summary>What <c>{rule.Name}</c> is read by, whichever stack it is read on.</summary>");

		using (file.Block(
			$"static int {core}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectCoreParameters})"))
		{
			file.Write(new DirectWriter(this).Render(body, FollowSets.Continuation.End, whole: true));
		}

		file.Line();
	}

	/// <summary>
	/// The calls that close a cycle: from a rule to one still being entered above it. Every
	/// cycle of the call graph has one, so a stack check before each is a check per level of
	/// nesting rather than per rule, which for a ladder of a dozen rules is a dozen times
	/// fewer.
	/// </summary>
	readonly HashSet<(RuleSymbol From, RuleSymbol To)> _backEdges = [];

	/// <summary>Rules written in place by the plan that a reader over the budget called instead.</summary>
	readonly HashSet<RuleSymbol> _readersWanted = [];

	void BackEdges(IReadOnlyList<Publication> publications)
	{
		_backEdges.Clear();

		var done  = new HashSet<RuleSymbol>();
		var above = new HashSet<RuleSymbol>();

		void Visit(RuleSymbol rule)
		{
			if (!above.Add(rule))
				return;

			if (!done.Contains(rule))
				foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
					if (node is Node.Call(var called, _))
					{
						if (above.Contains(called))
							_backEdges.Add((rule, called));
						else
							Visit(called);
					}

			done.Add(rule);
			above.Remove(rule);
		}

		foreach (var publication in publications)
			Visit(publication.Rule);
	}

	/// <summary>
	/// How much stack the deep reader is given. A frame of a direct reader is a few
	/// hundred bytes at most, so this is a nesting of some hundreds of thousands — and
	/// reserved rather than committed, so an input that does not need it pays nothing.
	/// </summary>
	const int DeepStack = 256 * 1024 * 1024;

	/// <summary>The name of the tape of ways back, shared by every direct rendering in a file.</summary>
	internal const string WaysType = "Ways";

	/// <summary>
	/// Writes one rule body as the inside of a method: <c>p</c> from <c>pos</c> to where
	/// the body ends, or <c>-1</c> once every way back inside it is spent.
	/// </summary>
	/// <remarks>
	/// Every construct's code either falls through with <c>p</c> past it, or jumps to its
	/// fail label with <c>p</c> where the construct began. A construct that opened ways
	/// back is a segment: its fail label first asks the tape for the latest way still
	/// open at or after the segment's start, and if there is one, takes it and runs the
	/// construct again from its own mark. Only when none is left does the failure go on
	/// outward. That is the whole engine, in two labels per construct.
	/// </remarks>
	sealed class DirectWriter(Machine machine)
	{
		readonly RecognitionGraph _graph = machine._graph;

		int _labels;
		int _marks;
		int _turns;
		int _ways;
		int _calls;

		/// <summary>The rules being written in place above the point being written, innermost on top.</summary>
		readonly Stack<RuleSymbol> _inlining = new();

		/// <summary>How many branches this method has taken in from rules written in place.</summary>
		int _copied;

		/// <summary>
		/// How much a method takes in before the rest is called, in the branches the JIT
		/// counts: enough for a ladder of levels to collapse into the reader at its top,
		/// and short of what the JIT declines to optimize (Machine.Sizes.cs) once the
		/// method's own body is added.
		/// </summary>
		const int CopyBudget = 700;

		/// <summary>
		/// How large a rule may be to be written in place beyond what the plan already
		/// copies: a level of a ladder, not a list of forty keywords. A large body gains
		/// nothing by losing its call and costs the method it lands in its registers.
		/// </summary>
		const int CopyPiece = 120;
		int _segments;
		bool _character;

		/// <summary>The rule whose body is being written, for the back edges out of it and the captures in it.</summary>
		RuleSymbol? _owner;

		/// <summary>Where the owner's capture slots begin in the machine's numbering.</summary>
		int _offset;

		/// <summary>
		/// Whether the next refusal stands at the door of a settled loop, where the turn
		/// not beginning is the loop ending and not a failure to record — the same silence
		/// the engine keeps at a loop it leaves through the door.
		/// </summary>
		bool _quiet;

		/// <summary>
		/// Whether a rule the plan compiles in place is written in place here. Off for a
		/// second rendering of a body that came out over the budget the first time: the
		/// calls stay calls, and the method stays one the JIT will optimize.
		/// </summary>
		public bool Inline { get; set; } = true;

		/// <summary>The refusal recorder, shared by every direct rendering in a file.</summary>
		const string Refuse = "Refuse_DotGram";

		public string Render(Node body, FollowSets.Continuation following, bool whole, RuleSymbol? owner = null)
		{
			_owner  = owner;
			_offset = owner is not null ? machine._captureOffsets[owner] : 0;

			var code    = new Writer(0);
			var segment = Segment();
			var valued  = owner is not null && machine.Valued(owner);

			// A guard reads the side stack and the log from where the rule began, whether
			// or not the rule keeps a value of its own.
			var guarded = owner is not null && NodeWalk.Descendants(body).Any(static node => node is Node.Guard);

			var inner = new Writer(0);

			Emit(inner, body, "Fail", following);

			if (whole)
			{
				inner.Line("if (p != text.Length)");
				using (inner.Block(""))
				{
					inner.Line($"{Refuse}(ref failure, p, null, ways);");
					inner.Line("goto Fail;");
				}
			}

			if (owner is not null && machine.RecordsAtEnd(owner))
				EmitRecord(inner, -1);

			var fallible = inner.ToString().Contains("goto Fail;", StringComparison.Ordinal);

			code.Line("Again:");
			code.Line("p = pos;");

			if (valued)
			{
				LogBack(code, "lm");
				code.Line("ways.RefsCount = rb;");
			}

			code.Write(inner.ToString());
			code.Line("return p;");

			// A body that cannot fail has no failure path, and one it does not have is not written.
			if (fallible)
			{
				code.Line("Fail:");
				code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto Again;");

				if (valued)
				{
					LogBack(code, "lm");
					code.Line("ways.RefsCount = rb;");
				}

				code.Line("return -1;");
			}

			var written = Unused(ScanWriter.Threaded(code.ToString()));
			var head    = new Writer(0);

			head.Line("var p = pos;");

			if (_character)
				head.Line("var c = '\\0';");

			head.Line($"var {segment} = ways.Cursor;");

			if (valued || guarded)
			{
				head.Line("var lm = ways.LogCount;");
				head.Line("var rb = ways.RefsCount;");
			}

			for (var i = 0; i < _marks; i++)
				if (Mentions(written, $"m{i}"))
					head.Line($"var m{i} = 0;");

			for (var i = 0; i < _turns; i++)
				if (Mentions(written, $"t{i}"))
					head.Line($"var t{i} = 0;");

			for (var i = 1; i < _segments; i++)
			{
				if (Mentions(written, $"s{i}"))
					head.Line($"var s{i} = 0;");
				if (Mentions(written, $"lm{i}"))
					head.Line($"var lm{i} = 0;");
				if (Mentions(written, $"rr{i}"))
					head.Line($"var rr{i} = 0;");
			}

			for (var i = 0; i < _ways; i++)
			{
				if (Mentions(written, $"w{i}"))
					head.Line($"var w{i} = 0;");
				if (Mentions(written, $"d{i}"))
					head.Line($"var d{i} = 0;");
			}

			for (var i = 0; i < _calls; i++)
				if (Mentions(written, $"q{i}"))
					head.Line($"var q{i} = 0;");

			// The capture locals: where a text capture began and ended, or which record a
			// captured rule wrote. Started over each time the rule runs again from the top.
			var captures = new List<string>();

			for (var slot = 0; slot < 64; slot++)
				foreach (var prefix in new[] { "a", "b", "r" })
					if (Mentions(written, $"{prefix}{slot}"))
						captures.Add($"{prefix}{slot}");

			// A fold's value so far: the record of the base, then of each step (§4.3).
			if (Mentions(written, "fold"))
				captures.Add("fold");

			foreach (var local in captures)
				head.Line($"var {local} = -1;");

			if (captures.Count > 0)
				written = written.Replace(
					"Again:\r\np = pos;",
					"Again:\r\np = pos;\r\n" + string.Join("\r\n", captures.Select(local => $"{local} = -1;")));

			head.Line();
			head.Write(written);

			return head.ToString();
		}

		/// <summary>
		/// Takes out the marks nothing restores from and the segments nothing retries or
		/// seals: a construct whose body turned out unable to fail wrote them for a failure
		/// path it does not have, and an assigned-never-read local is a warning in somebody
		/// else's build.
		/// </summary>
		string Unused(string written)
		{
			// Until nothing changes: taking out a dead jump can leave a label unreferenced,
			// and taking out the label can leave the code after it dead.
			for (var pass = 0; pass < 4; pass++)
			{
				var before = written;

				written = Cleaned(written);

				if (written == before)
					break;
			}

			return written;
		}

		string Cleaned(string written)
		{
			var lines = new List<string>(written.Split('\n'));

			// A mark read anywhere but its own assignment stays: restored from, or the start
			// a run measures its length against.
			for (var i = 0; i < _marks; i++)
				if (!Mentions(written.Replace($"m{i} = p;", ""), $"m{i}"))
					lines.RemoveAll(line => line.TrimEnd('\r').TrimStart('\t') == $"m{i} = p;");

			for (var i = 1; i < _segments; i++)
				if (!written.Contains($"Retry(s{i})", StringComparison.Ordinal) &&
					!written.Contains($"Seal(s{i})", StringComparison.Ordinal))
				{
					lines.RemoveAll(line => line.TrimEnd('\r').TrimStart('\t') == $"s{i} = ways.Cursor;");
				}

			// What stands after an unconditional jump, up to the next label at the same depth,
			// is never reached: the failure path of a construct whose every jump to it was
			// threaded away. A block opened inside it goes with it.
			var depth = 0;
			var dead  = -1;

			for (var i = 0; i < lines.Count; i++)
			{
				var trimmed = lines[i].Trim();

				if (dead >= 0)
				{
					if (depth == dead && ScanWriter.IsLabel(lines[i], out _, out _) ||
						trimmed.StartsWith("}", StringComparison.Ordinal) && depth <= dead)
					{
						dead = -1;
					}
					else
					{
						depth += Count(trimmed, '{') - Count(trimmed, '}');
						lines[i] = "";

						continue;
					}
				}

				depth += Count(trimmed, '{') - Count(trimmed, '}');

				if (trimmed.StartsWith("goto ", StringComparison.Ordinal) && trimmed.EndsWith(";", StringComparison.Ordinal) ||
					trimmed.StartsWith("return ", StringComparison.Ordinal))
				{
					dead = depth;
				}
			}

			lines.RemoveAll(static line => line.Length == 0);

			// Threaded again: a label whose every jump stood in the code just taken out is
			// unreferenced now, and that pass is what takes those out.
			return ScanWriter.Threaded(string.Join("\n", lines));
		}

		static int Count(string line, char symbol)
		{
			var count = 0;

			foreach (var c in line)
				if (c == symbol)
					count++;

			return count;
		}

		/// <summary>Whether a local is named at all — as a whole name, since <c>m1</c> is inside <c>m10</c>.</summary>
		static bool Mentions(string written, string name) =>
			System.Text.RegularExpressions.Regex.IsMatch(written, $@"\b{name}\b");

		string Segment() => $"s{_segments++}";

		string Mark() => $"m{_marks++}";

		string Label(string what) => $"L{_labels++}_{what}";

		string Expected(IReadOnlyList<string> display)
		{
			var name = machine.DeclareExpected(display);

			machine._expectedUsed.Add(name);

			return name;
		}

		void Refused(Writer code, string at, string? expected, string fail)
		{
			if (!_quiet)
				code.Line($"{Refuse}(ref failure, {at}, {expected ?? "null"}, ways);");

			code.Line($"goto {fail};");
		}

		/// <summary>
		/// Where a construct runs again from its mark, <c>c</c> may hold whatever the code
		/// between the first run and the retry last read. The position is the mark again,
		/// so the character is read again where the construct was entered with it loaded.
		/// </summary>
		void Reloaded(Writer code, bool loaded)
		{
			if (loaded)
				code.Line("c = text[p];");
		}

		/// <summary>Something was consumed: from here on a refusal is a failure.</summary>
		void Consumed(Writer code, string advance)
		{
			code.Line(advance);
			_quiet = false;
		}

		/// <summary>Whether a piece of written code records anything a failure would have to take back.</summary>
		static bool Writes(string written) =>
			written.Contains("ways.Begin(", StringComparison.Ordinal) ||
			written.Contains("ways.Push(", StringComparison.Ordinal) ||
			written.Contains("ways.Mark(", StringComparison.Ordinal) ||
			written.Contains("= Read_", StringComparison.Ordinal);

		/// <summary>The log put back to where a segment began, on the paths that give its reading up.</summary>
		void Unwritten(Writer code, string segment, bool writes, string written = "")
		{
			if (writes)
			{
				LogBack(code, $"lm{segment.Substring(1)}");
				code.Line($"ways.RefsCount = rr{segment.Substring(1)};");
			}

			// A capture made inside the reading given up is not a capture: its locals go back
			// to nothing, the way the engine takes its entries off the arena.
			foreach (var local in Assigned(written))
				code.Line($"{local} = -1;");
		}

		/// <summary>The capture locals a piece of written code assigns, each once.</summary>
		static IEnumerable<string> Assigned(string written)
		{
			var seen = new HashSet<string>();

			foreach (System.Text.RegularExpressions.Match assigned in System.Text.RegularExpressions.Regex.Matches(written, @"\b([abr]\d+) = "))
				if (seen.Add(assigned.Groups[1].Value))
					yield return assigned.Groups[1].Value;
		}

		/// <summary>
		/// The log put back to a count — and with it the watermark of what a guard built,
		/// where anything builds: a record above the watermark is one written since.
		/// </summary>
		void LogBack(Writer code, string count)
		{
			code.Line($"ways.LogCount  = {count};");

			if (machine._directBuilds)
				code.Line($"if (ways.Built > {count}) ways.Built = {count};");
		}

		/// <summary>Where the log stood when a segment began, kept beside its way-back segment.</summary>
		static void Marked(Writer code, string segment, bool writes)
		{
			if (!writes)
				return;

			code.Line($"lm{segment.Substring(1)} = ways.LogCount;");
			code.Line($"rr{segment.Substring(1)} = ways.RefsCount;");
		}

		/// <param name="loaded">
		/// Whether <c>c</c> already holds <c>text[p]</c> with the position proven in
		/// bounds — true right after a choice's front test, and carried only as far as
		/// nothing has consumed.
		/// </param>
		/// <summary>The alternatives read by a method of their own, where the rule's are (Machine.Direct.cs, the budget).</summary>
		public IReadOnlyDictionary<Node, string>? Parts { get; set; }

		void Emit(Writer code, Node node, string fail, FollowSets.Continuation following, bool loaded = false)
		{
			if (Parts is not null && Parts.TryGetValue(node, out var part))
			{
				var result = $"q{_calls++}";

				code.Line($"{result} = {part}(text, p, ref failure, ways{machine.DirectReaderArguments});");
				code.Line($"if ({result} < 0) goto {fail};");
				Consumed(code, $"p = {result};");

				return;
			}

			switch (node)
			{
				case Node.Empty:
					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
					EmitLiteral(code, node, text, folded, fail, loaded);
					break;

				case Node.Element element:
				{
					var test = CSharpEmitter.Test(element, machine.Tabulate);
					var name = Expected([node.ToString()]);

					if (!loaded)
					{
						code.Line("if ((uint)p >= (uint)text.Length)");
						using (code.Block(""))
							Refused(code, "p", name, fail);
					}

					if (!string.Equals(test, "true", StringComparison.Ordinal))
					{
						_character = true;

						if (!loaded)
							code.Line("c = text[p];");

						code.Line($"if (!({test}))");
						using (code.Block(""))
							Refused(code, "p", name, fail);
					}

					Consumed(code, "p++;");

					break;
				}

				case Node.Sequence(var parts):
					EmitSequence(code, parts, fail, following, loaded);
					break;

				case Node.Choice(var alternatives):
					EmitChoice(code, alternatives, fail, following, loaded);
					break;

				case Node.Repeat repeat:
					EmitRepeat(code, repeat, fail, following);
					break;

				case Node.Lookahead(var positive, var inside):
					EmitLookahead(code, positive, inside, fail, loaded);
					break;

				case Node.Behind(var boundary):
				{
					_character = true;

					var name = Expected([node.ToString()]);

					code.Line("if (p > 0)");
					using (code.Block(""))
					{
						code.Line("c = text[p - 1];");
						code.Line($"if ({CSharpEmitter.Test(boundary, machine.Tabulate)})");
						using (code.Block(""))
							Refused(code, "p", name, fail);

						// The character behind was read into `c`; what stands here goes back, where
						// something after this was told it is loaded.
						Reloaded(code, loaded);
					}

					break;
				}

				case Node.Atomic(var kept):
					EmitAtomic(code, kept, fail, following, loaded);
					break;

				case Node.Guard guard:
					EmitGuard(code, guard, fail);
					break;

				// A mark is a record of its own: it goes with the log wherever the log is put
				// back, which is the whole of what an abandoned reading owes it (§7.8).
				case Node.Marked(var marked, var text):
				{
					var site = machine.MarkSite(text);

					code.Line($"ways.Mark(-1, {site}, p);");
					Emit(code, marked, fail, following, loaded);
					code.Line($"ways.Mark(-2, {site}, p);");

					break;
				}

				case Node.External(var method):
					code.Line($"if (!{method}(text, ref p))");
					using (code.Block(""))
						Refused(code, "p", null, fail);

					_quiet = false;

					break;

				case Node.Call(var called, _):
				{
					// Written in place where the plan says so, and also where the rule keeps
					// nothing and this method has room: a ladder of levels is a call per level
					// per operand otherwise, and each call is a frame, a prologue and a return
					// for a body that is often one loop. A rule already being written in place
					// above this point is called instead, which is what breaks every cycle;
					// the budget is what keeps the method one the JIT will optimize.
					if (Inline && machine.DirectCopyable(called) && !_inlining.Contains(called) &&
						!ReferenceEquals(called, _owner) &&
						(machine.CanInline(called) || machine.DirectCost(called) <= CopyPiece) &&
						_copied + machine.DirectCost(called) <= CopyBudget &&
						ReferenceEquals(FollowSets.SeamOf(called, _graph), machine._seam))
					{
						// Written first and measured after, in the JIT's own units: what a
						// body costs in branches is only known once it is written, and a rule
						// written in place inside it has already been counted into the total.
						// What a discarded rendering learned about the method is forgotten
						// with it, or a local would be declared for a use that was thrown away.
						var before    = _copied;
						var character = _character;
						var quiet     = _quiet;
						var buffer    = new Writer(0);

						_inlining.Push(called);
						Emit(buffer, _graph.Bodies[called], fail, following, loaded);
						_inlining.Pop();

						var copied = buffer.ToString();
						var cost   = Branches(copied);

						if (before + cost <= CopyBudget)
						{
							_copied = before + cost;
							code.Write(copied);

							break;
						}

						_copied    = before;
						_character = character;
						_quiet     = quiet;
					}

					// A rule that could have been written in place, called instead: it needs
					// a reader, and the queue only seeded the ones the plan calls.
					if (machine.CanInline(called) || machine.DirectCopyable(called))
						machine._readersWanted.Add(called);

					var result = $"q{_calls++}";

					// The stack check goes on the edge that closes a cycle in the call graph,
					// and inside a body written in place that edge is the inlined rule's.
					var from = _inlining.Count > 0 ? _inlining.Peek() : _owner;

					if (from is not null && machine._backEdges.Contains((from, called)))
						code.Line("global::System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();");

					code.Line($"{result} = {machine.ReaderOf(called)}(text, p, ref failure, ways{machine.DirectReaderArguments});");
					code.Line($"if ({result} < 0) goto {fail};");
					Consumed(code, $"p = {result};");

					break;
				}

				case Node.Capture(_, var held):
					EmitCapture(code, node, held, fail, following, loaded);
					break;

				case Node.Construct(var built, _):
					Emit(code, built, fail, following, loaded);
					EmitRecord(code, machine._constructs[node]);
					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed CanDirect but has no direct emission.");
			}
		}

		/// <summary>
		/// A capture: where the text began and ended, kept in locals; a captured rule's
		/// record, kept the same way; and either of those inside a repetition, pushed to
		/// the side stack for the rule to gather at its end.
		/// </summary>
		void EmitCapture(Writer code, Node capture, Node held, string fail, FollowSets.Continuation following, bool loaded)
		{
			var slot   = machine._captureSlots[capture] - _offset;
			var member = _owner is null ? null : machine.MemberOfSlot(_owner, slot);

			if (member is null)
			{
				Emit(code, held, fail, following, loaded);

				return;
			}

			switch (member.Shape)
			{
				case MemberShape.Text:
					code.Line($"a{slot} = p;");
					Emit(code, held, fail, following, loaded);
					code.Line($"b{slot} = p;");
					break;

				case MemberShape.Pieces:
					code.Line($"a{slot} = p;");
					Emit(code, held, fail, following, loaded);
					code.Line($"ways.Push({slot}, a{slot}, p);");
					break;

				case MemberShape.Record:
					Emit(code, held, fail, following, loaded);
					code.Line($"r{slot} = ways.Last;");
					break;

				case MemberShape.Records:
					Emit(code, held, fail, following, loaded);
					code.Line($"ways.Push({slot}, ways.Last, -1);");
					break;
			}
		}

		/// <summary>The rule's record: which factory, where it stood, and each member in order.</summary>
		void EmitRecord(Writer code, int factory)
		{
			if (_owner is null)
				return;

			code.Line($"ways.Begin({machine._ruleIds[_owner]}, {factory}, pos, p);");

			// A fold step's first member is the value so far — the record of the base or of
			// the step before it — and each of its members is the one thing the step
			// captured (§4.3).
			if (machine.IsStep(_owner, factory))
				code.Line("ways.Put(fold);");

			foreach (var member in machine.DirectMembers(_owner, factory))
				switch (member.Shape)
				{
					case MemberShape.Text:
						code.Line($"ways.Put({First("a", "a", member.Slots)}, {First("a", "b", member.Slots)});");
						break;

					case MemberShape.Pieces:
						code.Line($"ways.Collect(rb, {member.Mask}L, true);");
						break;

					case MemberShape.Record:
						code.Line($"ways.Put({First("r", "r", member.Slots)});");
						break;

					case MemberShape.Records:
						code.Line($"ways.Collect(rb, {member.Mask}L, false);");
						break;
				}

			code.Line("ways.End(rb);");

			if (_graph.Folds.ContainsKey(_owner))
				code.Line("fold = ways.Last;");
		}

		/// <summary>
		/// A <c>when</c>, run where it stands with what the rule has captured so far (§7.7).
		/// A text capture is cut from the locals that hold it; a captured rule's value is
		/// built now, from the records already in the log, and stays built — the walk at
		/// the end skips what a guard built, so no factory runs twice.
		/// </summary>
		void EmitGuard(Writer code, Node.Guard guard, string fail)
		{
			var rule       = machine._owners[guard];
			var method     = $"Recognize_DotGram{machine._tag}_Guard" + machine._guards++;
			var helper     = new Writer(0);
			var parameters = new List<string>();
			var arguments  = new List<string>();
			var text       = guard.Text;

			if (CSharpEmitter.Uses(_graph, text, "parserText"))
			{
				parameters.Add("string parserText");
				arguments.Add(machine.Cut("pos", "p - pos"));
			}

			// The rule from where it began to where the parse now stands: what "the
			// current rule's span" means at a point that is not its end.
			if (CSharpEmitter.Uses(_graph, text, "parserSpan"))
			{
				parameters.Add("SourceSpan parserSpan");
				arguments.Add(machine.Span("pos", "p - pos"));
			}

			// Typed by this rule's own contract; the argument upcasts.
			if (_graph.ContextOf(rule) is { } contract && CSharpEmitter.Uses(_graph, text, "context"))
			{
				parameters.Add($"{contract} context");
				arguments.Add("context");
			}

			foreach (var (member, slots) in machine.GuardMembers(rule, guard))
			{
				var handed = $"g{_guardLocals++}";
				var type   = member.Rule is null ? "string" : machine._results.ValueOf(member.Rule);

				parameters.Add(
					$"{type}{(member.IsSequence ? "[]" : member.IsOptional ? "?" : "")} " +
					ResultTypes.ParameterOf(member));
				arguments.Add(handed);

				if (member.Rule is null)
				{
					code.Line($"var {handed}From = {First("a", "a", slots)};");
					code.Line($"var {handed}To   = {First("a", "b", slots)};");
					code.Line(
						$"var {handed} = {handed}From < 0 ? {(member.IsOptional ? "null" : "string.Empty")} : " +
						machine.Cut($"{handed}From", $"{handed}To - {handed}From") + ";");

					continue;
				}

				var build = type == "SourceSpan"
					? ""
					: $"{machine.DirectMaterializer}(ways, text, values, {{0}}, lm" +
						$"{machine.TokensArgument}{machine.ContextArgument});";

				if (!member.IsSequence)
				{
					code.Line($"var {handed}At = {First("r", "r", slots)};");

					if (build.Length > 0)
						code.Line($"if ({handed}At >= 0) {string.Format(build, handed + "At")}");

					code.Line(member.IsOptional
						? $"{type}? {handed} = {handed}At < 0 ? default({type}?) : {ValueAt(type, handed + "At")};"
						: $"var {handed} = {ValueAt(type, handed + "At")};");

					continue;
				}

				// Collected turn by turn on the side stack, and gathered here the way the
				// rule's end would gather them.
				var mask    = 0L;
				var bracket = type.IndexOf('[');

				foreach (var slot in slots)
					mask |= 1L << slot;

				code.Line($"var {handed}Count = 0;");
				code.Line("for (var at = rb; at < ways.RefsCount; at += 3)");
				code.Then($"if (({mask}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
				code.Line(
					$"var {handed} = new {(bracket < 0 ? type : type.Substring(0, bracket))}[{handed}Count]" +
					$"{(bracket < 0 ? "" : type.Substring(bracket))};");
				code.Line($"{handed}Count = 0;");

				using (code.Block("for (var at = rb; at < ways.RefsCount; at += 3)"))
				{
					code.Line($"if (({mask}L & (1L << ways.Refs[at])) == 0) continue;");

					if (build.Length > 0)
						code.Line(string.Format(build, "ways.Refs[at + 1]"));

					code.Line($"{handed}[{handed}Count++] = {ValueAt(type, "ways.Refs[at + 1]")};");
				}
			}

			helper.Line($"static bool {method}({string.Join(", ", parameters)}) =>");
			CSharpEmitter.Handed(helper, machine._lines, guard.At, text + ";");
			machine._extra.Add(helper.ToString());

			// A refused guard is a failure with nothing expected, as the engine has it.
			code.Line($"if (!{method}({string.Join(", ", arguments)}))");
			using (code.Block(""))
				Refused(code, "p", null, fail);
		}

		int _guardLocals;

		/// <summary>A record's value as a reader sees it: from the tables, or for an extent the record itself.</summary>
		string ValueAt(string type, string record) =>
			type == "SourceSpan"
				? machine.RecordValue(type, record).Replace("log[", "ways.Log[")
				: $"values.V{machine.TableFor(type)}[{record}]";

		/// <summary>
		/// The local of the first slot that was captured, where a member has several — one
		/// per alternative that names it — and -1 where none was.
		/// </summary>
		static string First(string test, string take, IReadOnlyList<int> slots)
		{
			if (slots.Count == 1)
				return $"{take}{slots[0]}";

			var chain = "-1";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"{test}{slots[i]} >= 0 ? {take}{slots[i]} : {chain}";

			return $"({chain})";
		}

		void EmitLiteral(Writer code, Node node, string text, bool folded, string fail, bool loaded)
		{
			if (text.Length == 0)
				return;

			var name = Expected([node.ToString()]);

			if (loaded && !folded && text.Length == 1)
			{
				code.Line($"if (c != {CSharpEmitter.Char(text[0])})");
				using (code.Block(""))
					Refused(code, "p", name, fail);

				Consumed(code, "p += 1;");

				return;
			}

			if (text.Length == 1)
			{
				var read = folded
					? "global::System.Char.ToUpperInvariant(text[p])"
					: "text[p]";
				var want = CSharpEmitter.Char(folded ? char.ToUpperInvariant(text[0]) : text[0]);

				code.Line($"if ((uint)p >= (uint)text.Length || {read} != {want})");
				using (code.Block(""))
					Refused(code, "p", name, fail);

				Consumed(code, "p += 1;");

				return;
			}

			code.Line($"if ({Short(text.Length)})");
			using (code.Block(""))
			{
				code.Line("failure.OutOfInput = p + 1;");
				Refused(code, "p", name, fail);
			}

			if (!folded)
			{
				code.Line(
					"if (!global::System.MemoryExtensions.SequenceEqual(" +
					$"text.Slice(p, {text.Length}), {Spanned(text)}))");
				using (code.Block(""))
					Refused(code, $"Reach_DotGram(text, p, {Spanned(text)})", name, fail);
			}
			else
			{
				code.Line(
					"if (!global::System.MemoryExtensions.Equals(" +
					$"text.Slice(p, {text.Length}), {Spanned(text)}, " +
					"global::System.StringComparison.OrdinalIgnoreCase))");
				using (code.Block(""))
					Refused(code, "p", name, fail);
			}

			Consumed(code, $"p += {text.Length};");
		}

		/// <summary>
		/// Parts one after another. A part after the first that fails puts the position
		/// back, and a sequence that opened ways back is a segment: those are retried by
		/// running the sequence again before its failure goes outward.
		/// </summary>
		void EmitSequence(
			Writer code, IReadOnlyList<Node> parts, string fail, FollowSets.Continuation following,
			bool loaded)
		{
			if (parts.Count == 1)
			{
				Emit(code, parts[0], fail, following, loaded);

				return;
			}

			// What follows each part is what precedes the part after it.
			var seam    = machine._seam;
			var follows = new FollowSets.Continuation[parts.Count];
			var next    = following;

			for (var i = parts.Count - 1; i >= 0; i--)
			{
				follows[i] = next;
				next       = FollowSets.Precedes(parts[i], next, _graph, seam);
			}

			var mark    = Mark();
			var segment = Segment();
			var again   = Label("again");
			var undo    = Label("undo");
			var over    = Label("on");
			var buffer  = new Writer(0);
			var carry   = loaded;

			for (var i = 0; i < parts.Count; i++)
			{
				Emit(buffer, parts[i], undo, follows[i], carry);

				carry = carry && parts[i] is Node.Empty or Node.Lookahead or Node.Behind;
			}

			var written = buffer.ToString();

			if (!written.Contains($"goto {undo};", StringComparison.Ordinal))
			{
				code.Write(written);

				return;
			}

			var writes = Writes(written);

			code.Line($"{mark} = p;");
			code.Line($"{segment} = ways.Cursor;");
			Marked(code, segment, writes);
			code.Line($"{again}:");
			Reloaded(code, loaded);
			code.Write(written);
			code.Line($"goto {over};");
			code.Line($"{undo}:");
			code.Line($"p = {mark};");
			Unwritten(code, segment, writes, written);
			code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");
			code.Line($"goto {fail};");
			code.Line($"{over}: ;");
		}

		/// <summary>
		/// Alternatives in order. Where one character tells them apart, or none can begin
		/// where another does, the first that matches is the only one that could, and no
		/// way back is written. Otherwise the choice records which alternative it took, so
		/// that a failure after it can take the next.
		/// </summary>
		void EmitChoice(
			Writer code, IReadOnlyList<Node> alternatives, string fail, FollowSets.Continuation following,
			bool loaded)
		{
			// An alternative that cannot fail is the last one ever tried: whatever follows it
			// in the choice is never reached, and code nothing reaches is a warning in
			// somebody else's build.
			for (var i = 0; i < alternatives.Count - 1; i++)
				if (machine.Infallible(alternatives[i]))
				{
					alternatives = alternatives.Take(i + 1).ToList();

					break;
				}

			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0], fail, following, loaded);

				return;
			}

			var took = Label("took");
			var mark = Mark();

			if (machine.Predictive(alternatives) is { } predicted)
			{
				_character = true;

				// The union of what the alternatives could have begun with, as the engine
				// displays a dispatch that found none of them.
				var name   = Expected([machine.PredictedDisplay(alternatives)]);
				var labels = alternatives.Select(_ => Label("alt")).ToList();

				code.Line($"{mark} = p;");

				if (!loaded)
				{
					code.Line("if ((uint)p >= (uint)text.Length)");
					using (code.Block(""))
						Refused(code, "p", name, fail);
					code.Line("c = text[p];");
				}

				for (var i = 0; i < alternatives.Count; i++)
					code.Line($"if ({predicted[i]}) goto {labels[i]};");

				Refused(code, "p", name, fail);

				for (var i = 0; i < alternatives.Count; i++)
				{
					code.Line($"{labels[i]}:");
					EmitAlternative(code, alternatives[i], mark, fail, following, loaded: true);
					code.Line($"goto {took};");
				}

				code.Line($"{took}: ;");

				return;
			}

			// Exclusive only where each alternative must begin with something of its own: one
			// that may match nothing is never told apart from the next by what it begins with.
			var exclusive = alternatives.All(one =>
				!FirstSets.Nullable(one, _graph) && FirstSets.Of(one, _graph).IsKnown);

			for (var i = 0; i < alternatives.Count && exclusive; i++)
				for (var j = i + 1; j < alternatives.Count && exclusive; j++)
					exclusive = machine.Exclusive(alternatives[i], alternatives[j]);

			// The gate before each alternative: what it can begin with, tested on the
			// character standing here before the alternative is entered at all. This is
			// §5's filter, and without it an operand walks every alternative of the choice
			// it stands in — eight of them in standard SQL, sixteen more inside one — each
			// refusing at its first token and recording that it did.
			var gates = alternatives.Select(Gate).ToList();
			var gated = gates.All(static gate => gate is not null);
			var union = gated ? Expected(alternatives.Select(static one => one.ToString()).ToList()) : null;

			if (gated && !loaded)
			{
				_character = true;

				code.Line("if ((uint)p >= (uint)text.Length)");
				using (code.Block(""))
					Refused(code, "p", union, fail);
				code.Line("c = text[p];");

				loaded = true;
			}

			if (exclusive)
			{
				code.Line($"{mark} = p;");

				for (var i = 0; i < alternatives.Count; i++)
				{
					var next = i == alternatives.Count - 1 ? fail : Label("or");

					if (gated)
					{
						code.Line($"if (!({gates[i]}))");
						using (code.Block(""))
						{
							if (i == alternatives.Count - 1)
								Refused(code, "p", union, fail);
							else
								code.Line($"goto {next};");
						}
					}

					EmitAlternative(code, alternatives[i], mark, next, following, loaded);

					if (i < alternatives.Count - 1)
					{
						code.Line($"goto {took};");
						code.Line($"{next}: ;");
						// The alternative that failed may have read past here; what stands here is loaded again.
						Reloaded(code, loaded);
					}
				}

				code.Line($"{took}: ;");

				return;
			}

			// The general case: one way back, its value the alternative in force.
			var way    = _ways++;
			var chosen = alternatives.Select(_ => Label("alt")).ToList();

			code.Line($"{mark} = p;");
			code.Line($"if (ways.Cursor < ways.Count) {{ w{way} = ways.Cursor; d{way} = ways.Items[w{way} * 2]; ways.Cursor++; }}");
			code.Line($"else {{ w{way} = ways.Open({alternatives.Count - 1}); d{way} = 0; }}");
			code.Line($"switch (d{way})");
			using (code.Block(""))
				for (var i = 0; i < alternatives.Count; i++)
					code.Line($"case {i}: goto {chosen[i]};");

			for (var i = 0; i < alternatives.Count; i++)
			{
				var spent = Label("spent");

				code.Line($"{chosen[i]}:");

				if (i > 0)
					Reloaded(code, loaded);

				if (gated)
				{
					// Gated out: spent without having been entered. The way still moves on,
					// so that a replay reads the same decision in the same place.
					code.Line($"if (!({gates[i]}))");
					using (code.Block(""))
					{
						if (i == alternatives.Count - 1)
							Refused(code, "p", union, fail);
						else
						{
							code.Line($"ways.Next(w{way}, {i + 1});");
							code.Line($"goto {chosen[i + 1]};");
						}
					}
				}

				EmitAlternative(code, alternatives[i], mark, spent, following, loaded);
				code.Line($"goto {took};");
				code.Line($"{spent}:");

				if (i < alternatives.Count - 1)
				{
					// This alternative is spent, and the way records that: the next one
					// is what the tape now says, so a replay from outside arrives there.
					code.Line($"ways.Next(w{way}, {i + 1});");
					code.Line($"goto {chosen[i + 1]};");
				}
				else
				{
					code.Line($"goto {fail};");
				}
			}

			code.Line($"{took}: ;");
		}

		/// <summary>
		/// The test that lets an alternative be entered, or null where none can be written:
		/// an alternative that may match nothing, or whose first set is not known, has to be
		/// entered to be refused.
		/// </summary>
		string? Gate(Node alternative)
		{
			var first = FirstSets.Of(alternative, _graph);

			if (!first.IsKnown || first.Ends || first.Ranges.Count == 0 ||
				FirstSets.Nullable(alternative, _graph))
			{
				return null;
			}

			return machine.RangesTest(first.Ranges, machine.Tabulate);
		}

		/// <summary>
		/// One alternative as a segment of its own: ways back opened inside it are
		/// retried by running it again from the mark before it is called spent.
		/// </summary>
		void EmitAlternative(
			Writer code, Node alternative, string mark, string spent, FollowSets.Continuation following,
			bool loaded)
		{
			var segment = Segment();
			var again   = Label("again");
			var failed  = Label("failed");
			var over    = Label("on");
			var buffer  = new Writer(0);

			Emit(buffer, alternative, failed, following, loaded);

			var written = buffer.ToString();

			if (!written.Contains($"goto {failed};", StringComparison.Ordinal))
			{
				code.Write(written);

				return;
			}

			var writes = Writes(written);

			code.Line($"{segment} = ways.Cursor;");
			Marked(code, segment, writes);
			code.Line($"{again}:");
			Reloaded(code, loaded);
			code.Write(written);
			code.Line($"goto {over};");
			code.Line($"{failed}:");
			code.Line($"p = {mark};");
			Unwritten(code, segment, writes, written);
			code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");
			code.Line($"goto {spent};");
			code.Line($"{over}: ;");
		}

		/// <summary>
		/// Turns until one does not fit. Where nothing after the repetition can begin where
		/// a turn does, every turn taken is final; otherwise each turn past the minimum is
		/// a way back — the option of having stopped before it.
		/// </summary>
		void EmitRepeat(Writer code, Node.Repeat repeat, string fail, FollowSets.Continuation following)
		{
			if (machine.RunTest(repeat.Body) is { } test)
			{
				EmitRun(code, repeat, test, fail, following);

				return;
			}

			var (body, min, max) = repeat;
			var loop     = Label("turn");
			var done     = Label("done");
			var again    = Label("again");
			var failed   = Label("failed");
			var counted  = min > 0 || max is not null;
			var turn     = counted ? _turns++ : -1;
			var mark     = Mark();
			var segment  = Segment();
			var nullable = FirstSets.Nullable(body, _graph);

			// A count with no range in it has no turn to give back, whatever follows.
			var settled = max == min || Determinism.NeverGivesBack(repeat, following, _graph, machine._seam);
			var way     = settled ? -1 : _ways++;

			// Inside the loop what follows a turn is another turn, or what follows the loop.
			var inside = new FollowSets.Continuation(
				FirstSets.Of(body, _graph).Or(following.Plain),
				FirstSets.Of(body, _graph).Or(following.AfterSeam));

			var buffer = new Writer(0);

			_quiet = settled;
			Emit(buffer, body, failed, inside);
			_quiet = false;

			var written  = buffer.ToString();
			var fallible = written.Contains($"goto {failed};", StringComparison.Ordinal);
			var writes   = Writes(written);

			if (counted)
				code.Line($"t{turn} = 0;");

			code.Line($"{loop}:");

			// A fold's step captures afresh each turn: what the step before captured is not
			// this step's, and a member this step does not write reads as not captured.
			if (_owner is not null && _graph.Folds.TryGetValue(_owner, out var fold) && ReferenceEquals(fold.Loop, repeat))
				foreach (var local in Assigned(written))
					code.Line($"{local} = -1;");

			if (max is { } limit)
				code.Line($"if (t{turn} >= {limit}) goto {done};");

			if (!settled)
			{
				var eligible = min > 0 ? $"if (t{turn} >= {min}) " : "";

				code.Line(
					$"{eligible}{{ if (ways.Cursor < ways.Count) {{ w{way} = ways.Cursor; d{way} = ways.Items[w{way} * 2]; ways.Cursor++; }} " +
					$"else {{ w{way} = ways.Open(1); d{way} = 0; }} if (d{way} == 1) goto {done}; }}");
			}

			code.Line($"{mark} = p;");
			code.Line($"{segment} = ways.Cursor;");
			Marked(code, segment, writes && fallible);
			code.Line($"{again}:");
			code.Write(written);

			if (counted)
				code.Line($"t{turn}++;");

			if (nullable)
				code.Line($"if (p == {mark}) goto {done};");

			code.Line($"goto {loop};");

			if (fallible)
			{
				code.Line($"{failed}:");
				code.Line($"p = {mark};");
				Unwritten(code, segment, writes, written);
				code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");

				if (!settled)
				{
					// The turn is spent, so the way that offered it now says "stopped here".
					var stop = min > 0 ? $"if (t{turn} >= {min}) " : "";

					code.Line($"{stop}ways.Next(w{way}, 1);");
				}

				if (min > 0)
					code.Line($"if (t{turn} < {min}) goto {fail};");
			}

			code.Line($"{done}: ;");
		}

		/// <summary>
		/// A repetition of one character test: a scan, with one way back for the whole run
		/// instead of one per turn. The body matched a character or it did not, so all a
		/// failure after the run can ask for is a shorter one — and a shorter run is the same
		/// scan stopped earlier, which the way counts down from the end. The door is quiet,
		/// as the engine's run is: what ends a run is not a failure, and the run leaves no
		/// trace at all where it has nothing to give back.
		/// </summary>
		void EmitRun(Writer code, Node.Repeat repeat, string test, string fail, FollowSets.Continuation following)
		{
			var (body, min, max) = repeat;

			if (max == 0)
				return;

			var mark    = Mark();
			var settled = max == min || Determinism.NeverGivesBack(repeat, following, _graph, machine._seam);
			var way     = settled ? -1 : _ways++;

			code.Line($"{mark} = p;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
					code.Line($"if (p - {mark} >= {limit}) break;");

				code.Line("if ((uint)p >= (uint)text.Length) break;");

				if (!string.Equals(test, "true", StringComparison.Ordinal))
				{
					_character = true;

					code.Line("c = text[p];");
					code.Line($"if (!({test})) break;");
				}

				code.Line("p++;");
			}

			var floor = min == 0 ? mark : $"({mark} + {min})";

			if (min > 0)
			{
				var name = Expected([body.ToString()]);

				code.Line($"if (p < {floor})");
				using (code.Block(""))
					Refused(code, "p", name, fail);
			}

			// The way's value is how many characters were handed back; replayed, the scan
			// reaches the same end and hands back the same number.
			if (!settled)
				code.Line(
					$"if (p > {floor}) {{ if (ways.Cursor < ways.Count) {{ d{way} = ways.Items[ways.Cursor * 2]; ways.Cursor++; }} " +
					$"else {{ ways.Open(p - {floor}); d{way} = 0; }} p -= d{way}; }}");

			_quiet = false;
		}

		/// <summary>
		/// A look that consumes nothing. What was decided inside is sealed once the look is
		/// over, and what was recorded inside is dropped: nothing after it can come back
		/// into it, because its outcome is one bit. A failing look records no expectation,
		/// the same as the engine, whose failure bookkeeping is off while a lookahead is open.
		/// </summary>
		void EmitLookahead(Writer code, bool positive, Node inside, string fail, bool loaded)
		{
			var mark    = Mark();
			var segment = Segment();
			var again   = Label("again");
			var failed  = Label("failed");
			var over    = Label("on");
			var buffer  = new Writer(0);

			Emit(buffer, inside, failed, FollowSets.Continuation.All, loaded);

			var written  = buffer.ToString();
			var fallible = written.Contains($"goto {failed};", StringComparison.Ordinal);
			var writes   = Writes(written);
			var decides  = fallible || written.Contains("ways.Open(", StringComparison.Ordinal);

			code.Line($"{mark} = p;");

			if (decides)
				code.Line($"{segment} = ways.Cursor;");

			Marked(code, segment, writes);
			code.Line("ways.Lookahead++;");
			code.Line($"{again}:");
			Reloaded(code, loaded && fallible);
			code.Write(written);
			code.Line($"p = {mark};");
			Reloaded(code, loaded);
			Unwritten(code, segment, writes, written);
			code.Line("ways.Lookahead--;");

			if (decides)
				code.Line($"ways.Seal({segment});");

			// A look that cannot fail always passes when positive and always fails when
			// negative, and the failure path it does not have is not written.
			if (!fallible)
			{
				if (!positive)
					code.Line($"goto {fail};");

				return;
			}

			if (positive)
			{
				code.Line($"goto {over};");
				code.Line($"{failed}:");
				code.Line($"p = {mark};");
				Unwritten(code, segment, writes, written);
				code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");
				Reloaded(code, loaded);
				code.Line("ways.Lookahead--;");
				code.Line($"goto {fail};");
				code.Line($"{over}: ;");
			}
			else
			{
				code.Line($"goto {fail};");
				code.Line($"{failed}:");
				code.Line($"p = {mark};");
				Unwritten(code, segment, writes, written);
				code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");
				Reloaded(code, loaded);
				code.Line("ways.Lookahead--;");
				code.Line($"{over}: ;");
			}
		}

		/// <summary>First-match-commits: what the group decided inside is sealed when it succeeds.</summary>
		void EmitAtomic(
			Writer code, Node kept, string fail, FollowSets.Continuation following, bool loaded)
		{
			var mark    = Mark();
			var segment = Segment();
			var again   = Label("again");
			var failed  = Label("failed");
			var over    = Label("on");
			var buffer  = new Writer(0);

			Emit(buffer, kept, failed, following, loaded);

			var written = buffer.ToString();
			var writes  = Writes(written);

			if (!written.Contains($"goto {failed};", StringComparison.Ordinal))
			{
				// Nothing inside can fail, so there is nothing to retry — but what was decided
				// inside is still committed: a loop that took its turns may not give one back.
				if (written.Contains("ways.Open(", StringComparison.Ordinal))
				{
					code.Line($"{segment} = ways.Cursor;");
					code.Write(written);
					code.Line($"ways.Seal({segment});");
				}
				else
					code.Write(written);

				return;
			}

			code.Line($"{mark} = p;");
			code.Line($"{segment} = ways.Cursor;");
			Marked(code, segment, writes);
			code.Line($"{again}:");
			Reloaded(code, loaded);
			code.Write(written);
			code.Line($"ways.Seal({segment});");
			code.Line($"goto {over};");
			code.Line($"{failed}:");
			code.Line($"p = {mark};");
			Unwritten(code, segment, writes, written);
			code.Line($"if (ways.Cursor > {segment} && ways.Retry({segment})) goto {again};");
			code.Line($"goto {fail};");
			code.Line($"{over}: ;");
		}
	}

	/// <summary>What may follow a rule wherever it is called, both with and without the seam.</summary>
	FollowSets.Continuation FollowOf(RuleSymbol rule)
	{
		_follows ??= FollowSets.Of(_graph);

		return _follows.TryGetValue(rule, out var following)
			? following
			: FollowSets.Continuation.All;
	}
}
