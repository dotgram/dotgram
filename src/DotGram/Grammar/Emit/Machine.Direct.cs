using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The gate to reading by methods, and what the reader's methods are handed.
/// </summary>
/// <remarks>
/// <para>
/// This file held the first rendering by methods: one method per rule, grown out of the
/// automaton and keeping its vocabulary — a construct a labelled region, a failure a jump,
/// and four passes to take the dead jumps, labels, marks and locals back out. The reader
/// (<c>Machine.Reader.cs</c>) replaced it, construct by construct, until every grammar in
/// the repository read the same tree either way; then the writer went (docs/next.md, "the
/// direct path goes").
/// </para>
/// <para>
/// What stays is what both needed and the reader still does. <see cref="CanDirect"/> says
/// whether a publication may be read by methods at all — the engine keeps what it refuses:
/// a stream, a <c>find</c>, a recovery, a captured lookahead, a call with arguments — and
/// says why (<see cref="Refusal"/>, reported as <c>GRAM5005</c>). The rest is the plumbing
/// a method takes beyond the text and the position: the value tables where a guard builds,
/// the tokens where a guard or a <c>~</c> reads them, the context, the strength a climb is
/// entered at, and the back edges that need a stack check.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// Whether every publication in a group can be written as methods.
	/// </summary>
	public bool CanDirect(IReadOnlyList<Publication> publications)
	{
		Refusal = null;

		if (publications.Count == 0)
			return Refused(null, "there is nothing published");

		foreach (var publication in publications)
		{
			if (publication.Kind != PublishKind.Parse)
				return Refused(publication.Rule, $"it is published with '{Directive(publication.Kind)}'");

			if (!DirectReachable(publication.Rule))
				return false;
		}

		// A guard handed a value builds it from the log while the text is read, and a
		// factory that asks for the input would have to be handed it there: not yet.
		DirectGuardNeeds(DirectRules(publications));

		return !_directBuilds || !UsesInput ||
			Refused(null, "a guard is handed a value whose factory asks for the input");
	}

	/// <summary>
	/// Why the methods could not be written, where they could not: a rule and what about
	/// it, in words a grammar's author can act on. Null where they were.
	/// </summary>
	/// <remarks>
	/// Kept because the answer matters beyond the emitter's own choice. Over kinds the
	/// language says a rule's answer stands (docs/syntax.md §4), and it is the methods
	/// that say it: a machine that falls back to the engine backtracks as it always did,
	/// so the grammar is told (GRAM5005) rather than left to differ silently.
	/// </remarks>
	public (RuleSymbol? Rule, string Why)? Refusal { get; private set; }

	bool Refused(RuleSymbol? rule, string why)
	{
		Refusal ??= (rule, why);

		return false;
	}

	static string Directive(PublishKind kind) => kind switch
	{
		PublishKind.Find => "find",
		_                => kind.ToString().ToLowerInvariant(),
	};

	/// <summary>What a machine that could not be written as methods is refused for (§4, over kinds).</summary>
	public const string Backtracks = "GRAM5005";

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

			if (_graph.Externals.ContainsKey(rule))
				return Refused(rule, "it is an external recognizer that keeps a value");

			if (!_graph.Bodies.TryGetValue(rule, out var body))
				return Refused(rule, "it has no body here");

			if (!DirectValuedRule(rule))
				return Refused(rule, "of what it builds");

			var bodies = new List<Node> { body };

			if (_graph.Trivia.TryGetValue(rule, out var seam))
				bodies.Add(seam);

			foreach (var one in bodies)
				foreach (var node in NodeWalk.Descendants(one))
				{
					if (_graph.Recoveries.ContainsKey(node))
						return Refused(rule, "it recovers from a bad element");

					switch (node)
					{
						case Node.Empty or Node.Literal or Node.Element or Node.Sequence or Node.Choice
							or Node.Repeat or Node.Lookahead or Node.Behind or Node.Atomic or Node.Glue:
							break;

						case Node.External { HasValue: false }:
							break;

						// What a lookahead saw is a capture the engine compiles as a machine
						// of its own; not here yet.
						case Node.Capture(_, Node.Lookahead):
							return Refused(rule, "it captures what a lookahead saw");

						case Node.Capture or Node.Construct or Node.Marked:
							break;

						case Node.Guard guard:
							if (!DirectGuard(rule, guard))
								return Refused(rule, "of what one of its guards is handed");

							break;

						case Node.Call(var called, var arguments):
							if (arguments.Count > 0)
								return Refused(rule, "it calls a rule with arguments");

							pending.Push(called);
							break;

						default:
							return Refused(rule, $"of a {node.GetType().Name.ToLowerInvariant()} in it");
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

	/// <summary>Whether the readers ask about a gap, which over kinds needs the tokens.</summary>
	bool _directGlue;

	void DirectGuardNeeds(IReadOnlyList<RuleSymbol> rules)
	{
		_directBuilds = _directGuardContext = _directGuards = _directGlue = false;

		foreach (var rule in rules)
			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
			{
				if (node is Node.Glue)
					_directGlue = true;

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
	}

	/// <summary>Whether the readers carry the context: a guard names it, or a guard builds a value whose factory might.</summary>
	bool DirectReaderContext => UsesContext && (_directGuardContext || _directBuilds);

	/// <summary>What a reader takes beyond the text, the position, the failure and the tape.</summary>
	string DirectReaderParameters =>
		(_directBuilds ? ", DirectValues values" : "") +
		((_directGuards || _directGlue) && OverKinds ? TokensParameter : "") +
		(DirectReaderContext ? ContextParameter : "");

	string DirectReaderArguments =>
		(_directBuilds ? ", values" : "") +
		((_directGuards || _directGlue) && OverKinds ? TokensArgument : "") +
		(DirectReaderContext ? ContextArgument : "");

	/// <summary>
	/// The strength a rule written with binding powers is read at, where it is one
	/// (§4.3.1). Last of the parameters rather than beside the position, because every
	/// other reader in the file has the same shape without it.
	/// </summary>
	string DirectStrength(RuleSymbol rule) => _graph.Climbing.ContainsKey(rule) ? ", int power" : "";

	/// <summary>
	/// The strength a call reads its operand at: what <c>&lt;&lt;</c> or <c>&gt;&gt;</c>
	/// recorded against this call, and 0 — everything — where it recorded nothing.
	/// </summary>
	string DirectStrengthOf(Node call, RuleSymbol called) =>
		_graph.Climbing.ContainsKey(called)
			? ", " + (_graph.Powers.TryGetValue(call, out var requested) ? requested : 0)
			: "";

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

	/// <summary>
	/// The calls that close a cycle: from a rule to one still being entered above it. Every
	/// cycle of the call graph has one, so a stack check before each is a check per level of
	/// nesting rather than per rule, which for a ladder of a dozen rules is a dozen times
	/// fewer.
	/// </summary>
	readonly HashSet<(RuleSymbol From, RuleSymbol To)> _backEdges = [];

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

	/// <summary>The name of the tape of ways back, shared by every direct rendering in a file.</summary>
	internal const string WaysType = "Ways";

}
