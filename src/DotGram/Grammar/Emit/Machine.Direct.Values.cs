using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Values in the direct rendering: what a rule records about what it captured, and the
/// walk that builds the values from those records once the parse has accepted.
/// </summary>
/// <remarks>
/// <para>
/// The engine records derivation as arena entries and materializes by walking linked lists
/// of them per completed call. Here a valued rule writes one record at its end, after its
/// children have written theirs: the rule, which of its factories, where it began and
/// ended, and then its members in order — a text capture as its start and end, a captured
/// rule as the index of that rule's record, a collected sequence as a count and then each
/// one. Every record begins with its own length, so building is one walk from the front,
/// each record's children already built by the time it is reached, and the last record
/// written is the root's.
/// </para>
/// <para>
/// Backtracking keeps §7.2's promise the same way the arena does: a construct that runs
/// again, or fails outward, first puts the log back to where it stood when the construct
/// began, so a record of a reading given back is gone before anything could build it.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>How one member of a rule's value stands in the record.</summary>
	enum MemberShape
	{
		/// <summary>Start and end, or -1 and -1 where nothing was captured.</summary>
		Text,

		/// <summary>A count, then the start and end of each piece: the text is their join.</summary>
		Pieces,

		/// <summary>The index of the captured rule's record, or -1.</summary>
		Record,

		/// <summary>A count, then the record of each element.</summary>
		Records,
	}

	sealed record DirectMember(ResultMember Member, int Index, MemberShape Shape, IReadOnlyList<int> Slots)
	{
		/// <summary>The slots as a mask, for the side stack to collect by.</summary>
		public long Mask
		{
			get
			{
				var mask = 0L;

				foreach (var slot in Slots)
					mask |= 1L << slot;

				return mask;
			}
		}
	}

	/// <summary>
	/// The members of a rule's value, each with the shape its record holds it in — the
	/// record of a fold step holding each as the one thing that step captured, which is
	/// how the step's factory takes it (§4.3), and the record of a base holding what the
	/// rule collects, as the base's factory takes it.
	/// </summary>
	/// <summary>Whether any rule collects this one across the turns of a repetition.</summary>
	/// <remarks>
	/// Which is a stack to gather on, of the collected rule's own type. Asked by the
	/// immediate carrier about an extent: it can carry one as a value, having the two
	/// positions in hand, and cannot gather one, there being no stack of spans — nothing
	/// else ever stores a span, so the value tables leave the type out.
	/// </remarks>
	bool Gathered(RuleSymbol rule)
	{
		foreach (var owner in _rules)
			foreach (var member in DirectMembers(owner))
				if (member.Shape == MemberShape.Records && ReferenceEquals(member.Member.Rule, rule))
					return true;

		return false;
	}

	List<DirectMember> DirectMembers(RuleSymbol rule, int factory = -1)
	{
		var members  = _graph.Results[rule];
		var shaped   = new List<DirectMember>(members.Count);
		var repeated = DirectRepeated(rule);

		// A step's record holds the step's own captures and no other — the members its
		// factory was written against, with the slots of that one alternative — each as
		// the one thing the step captured.
		if (IsStep(rule, factory))
		{
			var step = _factories[rule][factory];

			foreach (var mine in step.Members)
			{
				if (mine.Name == "parserText" || mine.Name == step.Accumulator)
					continue;

				for (var i = 0; i < members.Count; i++)
					if (members[i].Name == mine.Name)
						shaped.Add(new DirectMember(
							mine, i, mine.Rule is null ? MemberShape.Text : MemberShape.Record, mine.Slots));
			}

			return shaped;
		}

		// Every other record holds what its factory names, and nothing else. A rule of
		// several alternatives has one member per capture name across all of them, and a
		// record written for one alternative used to carry the lot: `ValueExpressionPrimary`
		// in standard SQL wrote five, four of them absent, on every one of its eight
		// alternatives — and the walk at the end read five back. Which member belongs to
		// which alternative is what the factory's parameters already say.
		if (factory >= 0 && factory < _factories[rule].Count)
		{
			var made = _factories[rule][factory];

			foreach (var wanted in made.Members)
			{
				if (wanted.Name == "parserText" || wanted.Name == made.Accumulator)
					continue;

				for (var i = 0; i < members.Count; i++)
					if (members[i].Name == wanted.Name)
						shaped.Add(new DirectMember(members[i], i, Shaped(members[i], wanted.Slots, repeated), wanted.Slots));
			}

			return shaped;
		}

		for (var i = 0; i < members.Count; i++)
			shaped.Add(new DirectMember(members[i], i, Shaped(members[i], members[i].Slots, repeated), members[i].Slots));

		return shaped;
	}

	/// <summary>How a record holds one member: the text it stands on, or the record it names.</summary>
	static MemberShape Shaped(ResultMember member, IReadOnlyList<int> slots, HashSet<int> repeated) =>
		member.Rule is null
			? slots.Any(repeated.Contains) ? MemberShape.Pieces : MemberShape.Text
			: member.IsSequence ? MemberShape.Records : MemberShape.Record;

	/// <summary>The capture slots of a rule under its fold's loop: those a step writes, rule-local.</summary>
	HashSet<int> DirectStepSlots(RuleSymbol rule)
	{
		if (_directStepSlots.TryGetValue(rule, out var known))
			return known;

		var found = new HashSet<int>();

		if (_graph.Folds.TryGetValue(rule, out var fold))
			foreach (var node in NodeWalk.Descendants(fold.Loop))
				if (node is Node.Capture && _captureSlots.TryGetValue(node, out var slot))
					found.Add(slot - _captureOffsets[rule]);

		_directStepSlots[rule] = found;

		return found;
	}

	readonly Dictionary<RuleSymbol, HashSet<int>> _directStepSlots = [];

	/// <summary>Whether a factory of a rule is a fold step, whose record leads with the value so far.</summary>
	bool IsStep(RuleSymbol rule, int factory) =>
		factory >= 0 && _factories[rule][factory].Accumulator is not null;

	/// <summary>The member a capture slot of a rule belongs to, if any — as the record that will hold it shapes it.</summary>
	DirectMember? MemberOfSlot(RuleSymbol rule, int slot)
	{
		var factory = -1;

		if (DirectStepSlots(rule).Contains(slot))
		{
			var factories = _factories[rule];

			for (var i = 0; i < factories.Count && factory < 0; i++)
				if (factories[i].Accumulator is not null)
					foreach (var mine in factories[i].Members)
						if (mine.Slots.Contains(slot))
							factory = i;
		}

		foreach (var member in DirectMembers(rule, factory))
			if (member.Slots.Contains(slot))
				return member;

		return null;
	}

	/// <summary>The rule a capture of the owner reads, or none where it reads text.</summary>
	/// <remarks>
	/// The results say which member a capture belongs to; they do not say which rule was read
	/// there, and one member may be captured in two places that read two different rules —
	/// <c>t: UnsignedLiteral =&gt; @(t)</c> beside <c>t: GeneralValueSpecification =&gt; @(t)</c>.
	/// A carrier handing values about by value type never has to ask, both of those building
	/// the type the member is declared as; one keeping a shape per rule has to, and the body
	/// is where the answer is.
	/// </remarks>
	internal RuleSymbol? RuleAt(RuleSymbol owner, int slot)
	{
		if (_read.TryGetValue((owner, slot), out var known))
			return known;

		var found = default(RuleSymbol);

		foreach (var node in NodeWalk.Descendants(_graph.Bodies[owner]))
			if (node is Node.Capture(_, var held) &&
				_captureSlots.TryGetValue(node, out var at) &&
				at - _captureOffsets[owner] == slot)
			{
				found = Called(held);

				break;
			}

		return _read[(owner, slot)] = found;

		static RuleSymbol? Called(Node node) =>
			node switch
			{
				Node.Call(var rule, _)                        => rule,
				Node.Construct(var body, _)                   => Called(body),
				Node.Atomic(var body)                         => Called(body),
				Node.Sequence(var parts) when parts.Count == 1 => Called(parts[0]),
				_                                             => null,
			};
	}

	readonly Dictionary<(RuleSymbol Owner, int Slot), RuleSymbol?> _read = [];

	/// <summary>Whether a rule keeps a value at all, and so writes a record.</summary>
	bool Valued(RuleSymbol rule) => ValueRule(rule) >= 0;

	/// <summary>Whether a rule writes its record where its body ends, having no construction of its own.</summary>
	bool RecordsAtEnd(RuleSymbol rule) => Valued(rule) && _factories[rule].Count == 0;

	/// <summary>
	/// Whether a rule can be read by methods with its value kept: everything the direct
	/// rendering knows how to record, and nothing it does not yet.
	/// </summary>
	bool DirectValuedRule(RuleSymbol rule)
	{
		if (!Valued(rule))
			return true;

		if (_graph.Externals.ContainsKey(rule))
			return false;

		var layout = CaptureLayout.Of(
			_graph.Bodies[rule], other => _results.QualifiedOf(other) is not null);

		return layout.Slots.Count <= 60;
	}

	/// <summary>
	/// Whether a record can be in the log that the root does not reach, so that the walk
	/// has to find out which ones before building any.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A valued rule that matched writes a record whether or not its caller kept the
	/// value, and §7.2 says a factory runs only for what the answer is made of — so the
	/// walk marks what the root reaches, back to front, before building anything. That is
	/// a pass over every record, an array of flags the width of the log, and a switch per
	/// record to say what it names.
	/// </para>
	/// <para>
	/// None of it is needed where every call to a valued rule is captured: then every
	/// record in the log is one somebody kept, and the root reaches all of them. Which is
	/// nearly every grammar that builds — a value nobody captures is a value written for
	/// nothing, and an author who wrote one meant to capture it.
	/// </para>
	/// <para>
	/// A lookahead is the exception and is refused here: what it read is put back, but its
	/// records are not, so a record inside one may be in the log with nothing naming it.
	/// </para>
	/// </remarks>
	bool DirectStrays(IReadOnlyList<RuleSymbol> rules)
	{
		// Over characters a rule's answer can be taken back after it has answered — the
		// publication's own reader runs again from the start when the whole input was not
		// read — and the record it wrote stays in the log. Over kinds nothing is taken
		// back (docs/syntax.md §4), so what is in the log is what was read.
		if (!OverKinds)
			return true;

		foreach (var rule in rules)
			if (_graph.Bodies.TryGetValue(rule, out var body) && Strays(body, false))
				return true;

		return false;

		bool Strays(Node node, bool kept)
		{
			switch (node)
			{
				case Node.Call(var called, _):
					return Valued(called) && !kept;

				case Node.Capture(_, var held):
					return Strays(
						held,
						held is Node.Call(var one, _) && Valued(one) ||
						held is Node.Repeat(Node.Call(var many, _), _, _) && Valued(many));

				case Node.Lookahead(_, var inside):
					return NodeWalk.Descendants(inside)
						.Any(one => one is Node.Call(var seen, _) && Valued(seen));

				case Node.Construct(var built, _):
					return Strays(built, kept);

				case Node.Sequence(var parts):
					return parts.Any(part => Strays(part, false));

				case Node.Choice(var alternatives):
					return alternatives.Any(one => Strays(one, kept));

				case Node.Repeat(var repeated, _, _):
					return Strays(repeated, kept);

				case Node.Atomic(var body):
					return Strays(body, kept);

				case Node.Marked(var body, _):
					return Strays(body, kept);

				default:
					return false;
			}
		}
	}

	/// <summary>
	/// Every record a machine can write, numbered: one arm per rule and alternative that
	/// writes one. The record carries this number and the walk switches on it once.
	/// </summary>
	/// <remarks>
	/// Numbered before a line is written, because the readers name an arm as they are
	/// rendered and the walk names the same arms afterwards. Allocated on demand from
	/// two places, the two would agree only by luck.
	/// </remarks>
	void DirectArms(IReadOnlyList<RuleSymbol> rules)
	{
		_directArms.Clear();
		_directArmed.Clear();

		foreach (var rule in rules)
		{
			if (!DirectRecords(rule))
				continue;

			var factories = _factories[rule];

			if (factories.Count == 0)
			{
				Armed(rule, -1);

				continue;
			}

			for (var i = 0; i < factories.Count; i++)
				if (!DirectForwards(rule, i))
					Armed(rule, i);
		}

		void Armed(RuleSymbol rule, int factory)
		{
			_directArms[(rule, factory)] = _directArms.Count;
			_directArmed.Add((rule, factory));
		}
	}

	readonly Dictionary<(RuleSymbol Rule, int Factory), int> _directArms = [];
	readonly List<(RuleSymbol Rule, int Factory)>            _directArmed = [];

	/// <summary>The rules a direct rendering is being written for, for the record's shape.</summary>
	internal IReadOnlyList<RuleSymbol> _directRules = [];

	/// <summary>Which arm a rule's alternative writes its record under.</summary>
	public int DirectArm(RuleSymbol rule, int factory) => _directArms[(rule, factory)];

	/// <summary>
	/// Whether a rule writes a record at all: one whose every alternative hands its
	/// operand up writes none, and the walk needs no arm for it.
	/// </summary>
	bool DirectRecords(RuleSymbol rule)
	{
		if (!Valued(rule))
			return false;

		var factories = _factories[rule];

		if (factories.Count == 0)
			return true;

		for (var i = 0; i < factories.Count; i++)
			if (!DirectForwards(rule, i))
				return true;

		return false;
	}

	/// <summary>
	/// Whether any arm of the walk reads the positions a record stands on: a factory that
	/// asks for the text or the span, or a terminal the lexer measured and a machine of
	/// its own rereads. Where none does, the two loads are not made.
	/// </summary>
	public bool DirectPositions(IReadOnlyList<RuleSymbol> rules)
	{
		foreach (var rule in rules)
		{
			if (!Valued(rule))
				continue;

			// An extent's value is the span its record stands on; nothing else is read
			// from it, and nothing else would be there to read.
			if (IsExtent(rule))
				return true;

			if (_reread is not null && _reread.Contains(rule))
				return true;

			foreach (var factory in _factories[rule])
				if (CSharpEmitter.WantsText(_graph, factory) ||
					CSharpEmitter.Asks(_graph, factory, "parserSpan"))
				{
					return true;
				}
		}

		return false;
	}

	/// <summary>
	/// Whether an alternative's value is exactly the value of the one rule it called, so
	/// that it needs no record of its own.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A ladder is written as a rule per level, and every level but the top hands its
	/// operand up unchanged: <c>SearchCondition = … | t: BooleanTerm =&gt; @(t)</c>. Written
	/// out, that was a record saying which rule and which alternative, a slot holding the
	/// record below, and a walk that read the slot back and called a factory whose whole
	/// body is <c>t</c>. Standard SQL has fourteen alternatives of that shape, and reading
	/// <c>a = 1</c> went through nine of them.
	/// </para>
	/// <para>
	/// Nothing is written now, and the rule's value is the record its callee left — which
	/// is what <c>ways.Last</c> already holds, and what a caller capturing this rule
	/// already reads. The types have to be the same for that to be true, so they are
	/// checked rather than assumed.
	/// </para>
	/// </remarks>
	/// <summary>
	/// The same, asked where the answer decides what the reader writes rather than what the
	/// walk reads: a carrier that hands a value between rules by rule and not by type has
	/// the caller looking somewhere the callee never wrote, and the alternative has to
	/// write after all. Asked here and not inside, because the carrier is chosen from what
	/// the machine turned out to hold and the analysis runs before it does.
	/// </summary>
	bool ForwardsInPlace(RuleSymbol? rule, int factory) =>
		Carrier.ForwardsInPlace && DirectForwards(rule, factory);

	bool DirectForwards(RuleSymbol? rule, int factory)
	{
		if (rule is null || factory < 0 || IsExtent(rule) || _reread is not null && _reread.Contains(rule))
			return false;

		var factories = _factories[rule];

		if (factory >= factories.Count)
			return false;

		var made = factories[factory];

		// A fold's step leads with the value so far, and an ambient argument is something
		// the record's own positions answer. Neither is the callee's value.
		if (made.Accumulator is not null ||
			CSharpEmitter.WantsText(_graph, made) ||
			CSharpEmitter.Asks(_graph, made, "parserSpan") ||
			CSharpEmitter.Asks(_graph, made, "parserInput") ||
			_graph.Context is not null && CSharpEmitter.Asks(_graph, made, "context") ||
			_graph.State is not null && CSharpEmitter.Asks(_graph, made, "parserState"))
		{
			return false;
		}

		if (made.Of is not Node.Construct(var body, Construction.Expression(var text, _)))
			return false;

		if (Alone(body) is not Node.Capture(var name, Node.Call(var called, { Count: 0 })))
			return false;

		return Valued(called) &&
			_results.QualifiedOf(called) == _results.QualifiedOf(rule) &&
			Bare(text) == (Keywords.Contains(name) ? "@" + name : name);
	}

	/// <summary>
	/// An expression with the parentheses around the whole of it taken off, which is how
	/// <c>=&gt; @(v)</c> reaches here from some places and not others.
	/// </summary>
	static string Bare(string text)
	{
		text = text.Trim();

		while (text.Length > 2 && text[0] == '(' && text[text.Length - 1] == ')')
		{
			var depth = 0;

			for (var i = 0; i < text.Length - 1; i++)
			{
				depth += text[i] == '(' ? 1 : text[i] == ')' ? -1 : 0;

				if (depth == 0)
					return text;
			}

			text = text.Substring(1, text.Length - 2).Trim();
		}

		return text;
	}

	/// <summary>
	/// The one operand an alternative is made of, where the seam woven around it is all
	/// that stands beside it (§4.5) — or the node itself where it is not a sequence.
	/// </summary>
	Node? Alone(Node body)
	{
		if (body is not Node.Sequence(var parts))
			return body;

		Node? only = null;

		foreach (var part in parts)
		{
			if (part is Node.Empty or Node.Glue or Node.Behind ||
				part is Node.Call(var seam, { Count: 0 }) && ReferenceEquals(seam, _seam))
			{
				continue;
			}

			if (only is not null)
				return null;

			only = part;
		}

		return only;
	}

	/// <summary>C#'s reserved words, for the parameter a capture's name becomes.</summary>
	static readonly HashSet<string> Keywords = ["base", "string", "object", "int", "value", "default", "params"];

	/// <summary>Where a direct walk writes a rule's value: into the table's held slot.</summary>
	string DirectInto(string type, string index) =>
		ValueInto(type, index) + (TableFor(type) >= 0 ? ".Value" : "");

	/// <summary>Where a direct walk reads one back.</summary>
	string DirectFrom(string type, string index) =>
		ValueFrom(type, index) + (TableFor(type) >= 0 ? ".Value" : "");

	/// <summary>The materializer for one direct machine: a walk over the log, a switch per rule.</summary>
	/// <remarks>
	/// Written twice where once will not do. The walk around the arms is half of what the
	/// method costs, and the arms alone were what the division measured — so a materializer
	/// could sit above the JIT's limit with every part of it under the divider's budget, and
	/// nothing divided it. It is measured whole here: rendered undivided, counted, and
	/// rendered again in parts only where the count says it must be.
	/// </remarks>
	string RenderDirectMaterializer(IReadOnlyList<RuleSymbol> rules)
	{
		var armed = new List<(RuleSymbol Rule, int Factory)>(_directArmed);
		var costs = new List<int>(armed.Count);
		var whole = 0;

		foreach (var (rule, factory) in armed)
		{
			var rendered = new Writer(0);

			MaterializeDirectArm(rendered, rule, factory);

			var cost = Branches(rendered.ToString());

			costs.Add(cost);

			whole += cost;
		}

		var once = RenderDirectWalk(rules, [armed]);

		return Branches(once) <= Limit || armed.Count < 2
			? once
			: RenderDirectWalk(rules, DirectParts(armed, costs, whole));
	}

	/// <summary>The walk itself, with its arms in one group or in several.</summary>
	string RenderDirectWalk(
		IReadOnlyList<RuleSymbol> rules, List<List<(RuleSymbol Rule, int Factory)>> parts)
	{
		var file = new Writer(0);

		file.Line("/// <summary>Builds the values a direct parse recorded, front to back (Machine.Direct.Values.cs).</summary>");


		// The root is the record whose value is wanted — the last one written, at the end;
		// a captured rule's, for a guard. `from` is where the walk may begin: a guard's
		// captures were recorded since its rule began, and nothing before that reaches them.
		using (file.Block(
			$"static void {DirectMaterializer}(" +
			$"{WaysType} ways, global::System.ReadOnlySpan<char> text, DirectValues values, int root, int from, int first" +
			$"{InputParameter}{TokensParameter}{ContextParameter})"))
		{
			// A guard builds while the text is read, so the walk at the end must know what
			// it already built; where no guard builds, nothing is ever built twice and the
			// flags and the clearing of them are work for a question with one answer.
			var twice  = _directBuilds;
			var strays = DirectStrays(rules);
			var placed = DirectPositions(rules);

			file.Line($"values.Room(ways.Records{(strays ? "" : ", live: false")});");
			file.Line();
			file.Line("var log   = ways.Log;");

			if (strays)
				file.Line("var live  = values.Live;");

			if (twice)
			{
				file.Line("var built = values.Built;");
				file.Line();
				// A record above the watermark was written since anything was built: whatever
				// its flag says is about a record that was put back with the log.
				file.Line("global::System.Array.Clear(built, ways.Built, ways.Records - ways.Built);");
			}

			file.Line();

			// What the root reaches, and nothing else: a valued rule that matched without being
			// captured is in the log, and its factory must not run (docs/syntax.md §7.2).
			if (strays)
			{
				file.Line("var starts = values.Starts;");
				file.Line("var listed = 0;");
				file.Line();
				file.Line("for (var at = from; at < ways.LogCount; at += log[at])");
				file.Then("starts[listed++] = at;");
				file.Line();
				file.Line("live[root] = true;");
				file.Line();
				using (file.Block("for (var back = listed - 1; back >= 0; back--)"))
				{
					file.Line("var at   = starts[back];");
					file.Line("var slot = first + back;");
					file.Line();
					file.Line("if (!live[slot]) continue;");
					file.Line();
					file.Line($"var read = at + {(DirectPositions(rules) ? 4 : 2)};");
					file.Line();

					if (parts.Count == 1)
					{
						using (file.Block("switch (log[at + 1])"))
							foreach (var (rule, factory) in _directArmed)
								MarkDirectArm(file, rule, factory);
					}
					else
					{
						// Divided the same way and into the same groups, so that a reader
						// looking for what a rule does finds both halves of it in the parts
						// named after the same number.
						file.Line("var kind = log[at + 1];");
						file.Line();

						for (var part = 0; part < parts.Count; part++)
							file.Line(
								part == 0
									? $"if (!{DirectMaterializer}_Reaches{part}(log, live, kind, read))"
									: part < parts.Count - 1
										? $"if (!{DirectMaterializer}_Reaches{part}(log, live, kind, read))"
										: $"	{DirectMaterializer}_Reaches{part}(log, live, kind, read);");
					}
				}
			}

			for (var i = 0; i < _valueTypes.Count; i++)
				file.Line($"var values{i} = values.V{i};");

			file.Line();

			if (UsesMarks)
			{
				// The marks standing over the walk's start: those opened before it and not
				// yet closed. Nothing else before the start is read.
				file.Line("var marked = 0;");
				file.Line();
				using (file.Block("for (var at = 0; at < from; at += log[at])"))
					using (file.Block("if (log[at + 1] < 0)"))
						DirectMark(file);
				file.Line();
			}

			using (file.Block("for (int at = from, slot = first; at < ways.LogCount; at += log[at], slot++)"))
			{
				if (UsesMarks)
				{
					using (file.Block("if (log[at + 1] < 0)"))
					{
						DirectMark(file);
						file.Line("continue;");
					}

					file.Line();
				}

				if (strays || twice)
				{
					file.Line(
						"if (" +
						string.Join(" || ", new[] { strays ? "!live[slot]" : null, twice ? "built[slot]" : null }
							.Where(one => one is not null)) +
						") continue;");
					file.Line();
				}

				if (placed)
				{
					file.Line("var start = log[at + 2];");
					file.Line("var end   = log[at + 3];");
				}

				file.Line($"var read  = at + {(placed ? 4 : 2)};");
				file.Line();

				if (twice)
				{
					file.Line("built[slot] = true;");
					file.Line();
				}

				using (file.Block("switch (log[at + 1])"))
				{
					if (parts.Count == 1)
					{
						foreach (var (rule, factory) in parts[0])
							MaterializeDirectArm(file, rule, factory);
					}
					else
					{
						for (var part = 0; part < parts.Count; part++)
						{
							foreach (var (rule, factory) in parts[part])
								file.Line($"case {DirectArm(rule, factory)}:");

							using (file.Indent())
							{
								file.Line(
									$"{DirectMaterializer}_Part{part}(text, log[at + 1], read, slot" +
									(placed ? ", start, end" : "") + ");");
								file.Line("break;");
							}
						}
					}
				}
			}

			if (twice)
			{
				file.Line();
				file.Line("ways.Built = ways.Records;");
			}

			// Local functions, for the reason the tape's materializer has them
			// (Machine.Materialization.cs): the compiler below stops optimizing a method
			// past about two thousand basic blocks, and this one is a walk with an arm per
			// valued rule — so it grows with the grammar and nothing else was dividing it.
			// The span cannot be a field of the frame a local function captures and is
			// handed over; `read` and `slot` are the iteration's own and are handed over
			// too, `read` because an arm advances it and no one after the arm reads it.
			if (parts.Count > 1)
				for (var part = 0; part < parts.Count; part++)
				{
					file.Line();

					using (file.Block(
						$"void {DirectMaterializer}_Part{part}(" +
						"global::System.ReadOnlySpan<char> text, int kind, int read, int slot" +
						(placed ? ", int start, int end" : "") + ")"))
					{
						using (file.Block("switch (kind)"))
							foreach (var (rule, factory) in parts[part])
								MaterializeDirectArm(file, rule, factory);
					}
				}

			// The other switch over the same arms, divided into the same groups. It says
			// what the root reaches and is walked before anything is built, and it was left
			// whole while the building half was divided — so a grammar with two hundred
			// valued rules had one method under the budget and one over it, and `GRAM5003`
			// named the method that held both.
			//
			// It answers whether it knew the kind, so the groups are asked in turn and the
			// one that knows it stops the chain. `read` is handed over by value: an arm
			// steps it and nothing after the switch reads it.
			if (strays && parts.Count > 1)
				for (var part = 0; part < parts.Count; part++)
				{
					file.Line();

					using (file.Block(
						$"static bool {DirectMaterializer}_Reaches{part}(" +
						"int[] log, bool[] live, int kind, int read)"))
					{
						using (file.Block("switch (kind)"))
						{
							foreach (var (rule, factory) in parts[part])
								MarkDirectArm(file, rule, factory);

							file.Line("default: return false;");
						}

						file.Line();
						file.Line("return true;");
					}
				}
		}

		return file.ToString();
	}

	/// <summary>
	/// The arms of a direct walk, in as few groups as will each keep inside the budget.
	/// </summary>
	/// <remarks>
	/// The same division `MaterializeParts` makes for the tape, and it was missing here: a
	/// direct materializer was one method however many rules it held, so a grammar large
	/// enough put it past the size at which the JIT stops optimizing and nothing divided it.
	/// `GRAM5003` said so and its advice — split the rule that is too big — had nothing to
	/// answer, because no one rule was: the method was the sum of all of them.
	/// </remarks>
	List<List<(RuleSymbol Rule, int Factory)>> DirectParts(
		List<(RuleSymbol Rule, int Factory)> armed, List<int> costs, int whole)
	{
		// Two at least. The arms are asked to divide because the *method* is over the line,
		// and most of what puts it there is the walk around them — so a count taken from the
		// arms' own total says one, which is the answer to a question nobody asked. What
		// moving them out buys is their bodies; what stays behind is a label each.
		var parts   = new List<List<(RuleSymbol Rule, int Factory)>>();
		var count   = global::System.Math.Max(2, (whole + Budget * 9 / 10 - 1) / (Budget * 9 / 10));
		var each    = whole / count + 1;
		var current = new List<(RuleSymbol Rule, int Factory)>();
		var carried = 0;

		for (var at = 0; at < armed.Count; at++)
		{
			if (current.Count > 0 && carried + costs[at] > each)
			{
				parts.Add(current);

				current = [];
				carried = 0;
			}

			current.Add(armed[at]);

			carried += costs[at];
		}

		if (current.Count > 0)
			parts.Add(current);

		return parts;
	}

	string DirectMaterializer => $"Materialize_DotGram{_tag}_Direct";

	/// <summary>A mark met on the walk: placed, its value goes on the stack a factory is shown; taken away, it comes off.</summary>
	void DirectMark(Writer file)
	{
		using (file.Block("if (log[at + 1] == -1)"))
		{
			file.Line("if (marked == values.MarkState.Length)");
			file.Then("global::System.Array.Resize(ref values.MarkState, marked * 2);");
			file.Line();
			file.Line($"values.MarkState[marked++] = {MarkValue("log[at + 2]")};");
		}

		file.Line("else");
		file.Then("marked--;");
	}

	/// <summary>
	/// One arm of the walk: a rule and the alternative that wrote the record, which
	/// together are the one number the record carries.
	/// </summary>
	void MaterializeDirectArm(Writer file, RuleSymbol rule, int factory)
	{
		var type = _results.QualifiedOf(rule)!;

		using (file.Block($"case {DirectArm(rule, factory)}:"))
		{
			if (IsExtent(rule))
			{
				// An extent is never put anywhere: whoever captured it reads its record.
				file.Line("break;");

				return;
			}

			if (_reread is not null && _reread.Contains(rule))
			{
				// A terminal that builds: the lexer measured it, and the character machine of its
				// own builds it from the text.
				file.Line($"{DirectInto(type, "slot")} = Value_{CSharpEmitter.IdentifierOf(rule)}_DotGram({Cut("start", "end - start")});");
				file.Line("break;");

				return;
			}

			var shaped = DirectMembers(rule, factory);

			// A fold's step leads with the value so far (§4.3).
			if (IsStep(rule, factory))
				file.Line("var accumulated = log[read++];");

			foreach (var member in shaped)
				ReadMember(file, member);

			if (factory < 0)
			{
				file.Line($"{DirectInto(type, "slot")} = new {type}(");

				using (file.Indent())
					for (var i = 0; i < shaped.Count; i++)
						file.Line(
							$"captured{shaped[i].Index}{(shaped[i].Member.IsOptional ? "" : "!")}" +
							(i + 1 < shaped.Count ? "," : ");"));
			}
			else
			{
				var made = _factories[rule][factory];

				file.Line(
					$"{DirectInto(type, "slot")} = " +
					$"{made.Method}({string.Join(", ", DirectArguments(rule, made, shaped))});");
			}

			file.Line("break;");
		}
	}

	/// <summary>Marks what one arm's record names as reached, given that the record itself is.</summary>
	void MarkDirectArm(Writer file, RuleSymbol rule, int factory)
	{
		using (file.Block($"case {DirectArm(rule, factory)}:"))
		{
			if (!IsExtent(rule) && (_reread is null || !_reread.Contains(rule)))
			{
				if (IsStep(rule, factory))
				{
					file.Line("live[log[read]] = true;");
					file.Line("read++;");
				}

				foreach (var member in DirectMembers(rule, factory))
					MarkMember(file, member);
			}

			file.Line("break;");
		}
	}

	/// <summary>Steps over one member of a record, marking what it names as reached.</summary>
	void MarkMember(Writer file, DirectMember member)
	{
		// An extent stands for its own record and is never built, so its liveness is
		// nobody's question — and what the log holds for it is a place, not a number.
		var extent = member.Member.Rule is { } rule && IsExtent(rule);

		switch (member.Shape)
		{
			case MemberShape.Text:
				file.Line("read += 2;");
				break;

			case MemberShape.Pieces:
				file.Line("read += 1 + log[read] * 2;");
				break;

			case MemberShape.Record:
				if (!extent)
					file.Line("if (log[read] >= 0) live[log[read]] = true;");

				file.Line("read++;");
				break;

			case MemberShape.Records:
				if (!extent)
				{
					file.Line("for (var item = 0; item < log[read]; item++)");
					file.Then("live[log[read + 1 + item]] = true;");
				}

				file.Line("read += 1 + log[read];");
				break;
		}
	}

	/// <summary>Reads one member out of the record into <c>captured{i}</c>.</summary>
	void ReadMember(Writer file, DirectMember member)
	{
		var i        = member.Index;
		var optional = member.Member.IsOptional;

		switch (member.Shape)
		{
			case MemberShape.Text:
				file.Line($"var from{i} = log[read++];");
				file.Line($"var to{i}   = log[read++];");
				file.Line(
					$"var captured{i} = from{i} < 0 ? {(optional ? "null" : "string.Empty")} : " +
					Cut($"from{i}", $"to{i} - from{i}") + ";");
				break;

			case MemberShape.Pieces:
				file.Line($"var count{i} = log[read++];");
				file.Line($"string{(optional ? "?" : "")} captured{i};");

				using (file.Block($"if (count{i} == 0)"))
					file.Line($"captured{i} = {(optional ? "null" : "string.Empty")};");

				using (file.Block($"else if (count{i} == 1)"))
				{
					file.Line($"captured{i} = {Cut("log[read]", "log[read + 1] - log[read]")};");
					file.Line("read += 2;");
				}

				using (file.Block("else"))
				{
					// §10's join, and the span where the span is the join — the reading the
					// tape already makes (Machine.Materialization.cs) and this did not.
					// Turns of a repetition that has nothing else in them are adjacent, and
					// then the pieces measure exactly the distance between the first start
					// and the last end: one cut and one string.
					//
					// Over kinds it is not an optimization but the answer. A position
					// indexes a token there, so a piece is a run of tokens and its length
					// is a count of them — what a piece is worth in characters is
					// somewhere else entirely, and what stands between two adjacent tokens
					// is part of the value the same reading over characters gives. Cut
					// whole, both are right; copied piece by piece, the buffer was sized in
					// tokens and the trivia between them was dropped.
					file.Line($"var length{i} = 0;");
					file.Line();
					file.Line($"for (var piece = 0; piece < count{i}; piece++)");
					file.Then($"length{i} += log[read + piece * 2 + 1] - log[read + piece * 2];");
					file.Line();
					file.Line($"var first{i} = log[read];");
					file.Line($"var last{i}  = log[read + (count{i} - 1) * 2 + 1];");
					file.Line();
					file.Line($"read += count{i} * 2;");
					file.Line();

					using (file.Block($"if (last{i} - first{i} == length{i})"))
						file.Line($"captured{i} = {Cut($"first{i}", $"length{i}")};");

					using (file.Block("else"))
					{
						if (OverKinds)
						{
							// The pieces do not tile, so what stands between them is not
							// part of the value and each run is cut on its own.
							file.Line(
								$"var built{i} = new global::System.Text.StringBuilder(last{i} - first{i});");
							file.Line($"var back{i}  = read - count{i} * 2;");
							file.Line();

							using (file.Block($"for (var piece = 0; piece < count{i}; piece++)"))
							{
								file.Line($"var pieceFrom = log[back{i} + piece * 2];");
								file.Line($"var pieceTo   = log[back{i} + piece * 2 + 1];");
								file.Line();
								file.Line($"built{i}.Append({Cut("pieceFrom", "pieceTo - pieceFrom")});");
							}

							file.Line();
							file.Line($"captured{i} = built{i}.ToString();");
						}
						else
						{
							file.Line($"var chars{i} = new char[length{i}];");
							file.Line($"var filled{i} = 0;");
							file.Line($"var back{i}   = read - count{i} * 2;");
							file.Line();

							using (file.Block($"for (var piece = 0; piece < count{i}; piece++)"))
							{
								file.Line($"var pieceFrom = log[back{i} + piece * 2];");
								file.Line($"var pieceTo   = log[back{i} + piece * 2 + 1];");
								file.Line();
								file.Line(
									$"text.Slice(pieceFrom, pieceTo - pieceFrom)" +
									$".CopyTo(new global::System.Span<char>(chars{i}, filled{i}, pieceTo - pieceFrom));");
								file.Line($"filled{i} += pieceTo - pieceFrom;");
							}

							file.Line();
							file.Line($"captured{i} = new string(chars{i});");
						}
					}
				}

				break;

			case MemberShape.Record:
			{
				var valueType = _results.ValueOf(member.Member.Rule);

				file.Line($"var record{i} = log[read++];");
				file.Line(
					optional
						? $"{valueType}? captured{i} = record{i} < 0 ? default({valueType}?) : {RecordValue(valueType, $"record{i}")};"
						: $"var captured{i} = {RecordValue(valueType, $"record{i}")};");
				break;
			}

			case MemberShape.Records:
			{
				var valueType = _results.ValueOf(member.Member.Rule);

				file.Line($"var count{i} = log[read++];");
				var bracket = valueType.IndexOf('[');
				var created = bracket < 0
					? $"new {valueType}[count{i}]"
					: $"new {valueType.Substring(0, bracket)}[count{i}]{valueType.Substring(bracket)}";

				file.Line($"var captured{i} = {created};");
				file.Line();
				using (file.Block($"for (var item = 0; item < count{i}; item++)"))
				{
					file.Line($"var record{i} = log[read++];");
					file.Line($"captured{i}[item] = {RecordValue(valueType, $"record{i}")};");
				}
				break;
			}
		}

		file.Line();
	}

	/// <summary>The value a record holds: from its type's table, or for an extent the record itself.</summary>
	string RecordValue(string type, string record) =>
		type == "SourceSpan"
			? Span($"log[{record} + 2]", $"log[{record} + 3] - log[{record} + 2]")
			: TableFor(type) is var table && table >= 0
				? $"values{table}[{record}].Value"
				: throw new InvalidOperationException($"No value table for '{type}'.");

	/// <summary>The factory's arguments as the walk over the log supplies them.</summary>
	List<string> DirectArguments(RuleSymbol rule, Factory factory, IReadOnlyList<DirectMember> members) =>
		DirectArguments(
			rule, factory, members,
			() => Cut("start", "end - start"), () => Span("start", "end - start"),
			() => RecordValue(_results.QualifiedOf(rule)!, "accumulated"),
			static member => $"captured{member.Index}");

	/// <summary>
	/// The factory's arguments, in the order the factory's parameters were written, from
	/// whatever the carrier has each member as.
	/// </summary>
	/// <remarks>
	/// The text, the span and the accumulator are asked for rather than handed in, because
	/// asking is not free: <see cref="Span"/> has the file emit the helper that makes one,
	/// and a grammar that never asked for a span must not be made to compile it.
	/// </remarks>
	List<string> DirectArguments(
		RuleSymbol rule, Factory factory, IReadOnlyList<DirectMember> members,
		Func<string> text, Func<string> span, Func<string> accumulator, Func<DirectMember, string> value)
	{
		var arguments = new List<string>();

		// In the order the factory's parameters are written (CSharpEmitter.EmitFactory).
		if (CSharpEmitter.WantsText(_graph, factory))
			arguments.Add(text());

		if (CSharpEmitter.Asks(_graph, factory, "parserSpan"))
			arguments.Add(span());

		if (CSharpEmitter.Asks(_graph, factory, "parserInput"))
			arguments.Add("parserInput");

		if (_graph.Context is not null && CSharpEmitter.Asks(_graph, factory, "context"))
			arguments.Add("context");

		// The stack as it stands, where there is one. `UsesMarks` counts the sites written
		// so far, which is an answer that changes as the emission goes; a reader that builds
		// as it reads asks this question while it is still writing them, and would hand one
		// construction the marks and the next `default`. Where the reader carries the stack
		// itself it is there whenever a state is declared, and that is the question asked.
		if (_graph.State is not null && CSharpEmitter.Asks(_graph, factory, "parserState"))
			arguments.Add(UsesMarks || CarriesImmediately
				? $"new global::System.ReadOnlySpan<{_graph.State}>(values.MarkState, 0, marked)"
				: "default");

		if (factory.Accumulator is not null)
			arguments.Add(accumulator());

		foreach (var wanted in factory.Members)
		{
			if (wanted.Name == "parserText" || wanted.Name == factory.Accumulator)
				continue;

			foreach (var member in members)
				if (member.Member.Name == wanted.Name)
				{
					arguments.Add(
						!wanted.IsOptional && member.Member is { Rule: not null, IsOptional: true }
							? $"({_results.ValueOf(member.Member.Rule)}){value(member)}!"
							: $"{value(member)}{(wanted.IsOptional ? "" : "!")}");
					break;
				}
		}

		return arguments;
	}
}
