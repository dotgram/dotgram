using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The reader: a grammar as the methods a person would have written.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a second rendering by methods.</b> The first one (<c>Machine.Direct.cs</c>) was
/// grown out of the automaton and kept its vocabulary: a rule is one method, but inside it
/// every construct is a labelled region and every failure is a jump. That shape is right
/// for one machine of a thousand states, which is a graph and nothing else; for a method it
/// is a graph nobody asked for. Four passes exist to take dead jumps, dead labels, dead
/// marks and unused locals back out of it, a fifth was written and was wrong, and the
/// reason it was wrong is that a question about one construct had to be asked of the whole
/// method.
/// </para>
/// <para>
/// Here a construct is a statement. A sequence is statements one after another; a failure
/// is <c>return -1</c>; a repetition is a <c>while</c>; a choice is a switch on what the
/// alternatives begin with, or one attempt after another where the first token does not
/// divide them. An alternative that can fail halfway and has a sibling after it becomes a
/// method of its own, because that is how a caller learns it failed without a jump — and
/// where the choice dispatches, no alternative needs one, since failing the alternative the
/// token chose is failing the choice.
/// </para>
/// <para>
/// <b>What it does not do yet</b>, and hands back to the rendering it is replacing: values,
/// guards, marks, folds, climbing, and the tape of ways back that reading characters needs
/// (§4 — over kinds a rule's answer stands, and there is no tape at all). Each of those
/// arrives with its own entry in <c>docs/next.md</c>. <see cref="CanRead"/> is the gate and
/// it refuses rather than guesses.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The refusal recorder the emitted readers call.</summary>
	const string Refusing = "Refuse_DotGram";

	/// <summary>Whether every publication in a group can be written as a reader.</summary>
	public bool CanRead(IReadOnlyList<Publication> publications)
	{
		Unread = null;

		if (publications.Count == 0)
			return Unreadable(null, "there is nothing published");

		if (!CanDirect(publications))
			return Unreadable(Refusal?.Rule, Refusal?.Why ?? "the methods refused it");

		foreach (var rule in DirectRules(publications))
		{
			// A fold accumulates across turns and a climb is entered at a strength; both
			// are what the rendering beside this one learned last and this one has not.
			if (_graph.Folds.ContainsKey(rule))
				return Unreadable(rule, "it folds");

			if (_graph.Climbing.ContainsKey(rule))
				return Unreadable(rule, "it climbs");

			if (_graph.Externals.ContainsKey(rule))
				return Unreadable(rule, "it is an external recognizer");

			if (IsExtent(rule))
				return Unreadable(rule, "its value is the text it matched");

			// A capture gathered across the turns of a repetition lives on the side stack
			// until the rule ends, which is a second place to put things and not yet here.
			foreach (var member in DirectMembers(rule))
				if (member.Shape is MemberShape.Pieces or MemberShape.Records)
					return Unreadable(rule, $"'{member.Member.Name}' is gathered across turns");

			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
				switch (node)
				{
					case Node.Empty or Node.Literal or Node.Element or Node.Sequence
						or Node.Choice or Node.Repeat or Node.Glue or Node.Behind:
						break;

					case Node.Lookahead(_, var inside) when !NodeWalk.Descendants(inside)
						.Any(static one => one is Node.Capture or Node.Construct or Node.Guard):
						break;

					case Node.External { HasValue: false }:
						break;

					case Node.Call(_, { Count: 0 }):
						break;

					case Node.Capture or Node.Construct:
						break;

					default:
						return Unreadable(rule, $"of a node it cannot write: {node.GetType().Name.ToLowerInvariant()}");
				}
		}

		// The way back is a loop over a tape, and the loop is written but the tape is not.
		if (!OverKinds)
			return Unreadable(null, "it reads characters, where a rule's answer can be taken back");

		foreach (var publication in publications)
			if (publication.Rule.GivesBack)
				return Unreadable(publication.Rule, "it is marked '?' and gives back");

		return true;
	}

	/// <summary>Why the reader could not write a machine, where it could not.</summary>
	public (RuleSymbol? Rule, string Why)? Unread { get; private set; }

	bool Unreadable(RuleSymbol? rule, string why)
	{
		Unread ??= (rule, why);

		return false;
	}

	/// <summary>
	/// How many methods the rule being written has been cut into.
	/// </summary>
	/// <remarks>
	/// One counter for the rule and not one per writer. A part may extract a part of its
	/// own — a choice inside an alternative, a repetition inside one — and a counter
	/// belonging to the writer numbered that one from zero again, giving two methods one
	/// name.
	/// </remarks>
	int _readerPart;

	/// <summary>Every rule of a reading, each as a method, with the entries above them.</summary>
	public string RenderReader(IReadOnlyList<Publication> publications)
	{
		var file  = new Writer(0);
		var rules = DirectRules(publications);
		var seen  = new HashSet<RuleSymbol>();

		BackEdges(publications);
		DirectGuardNeeds(rules);
		DirectArms(rules);

		_directRules = rules;

		foreach (var publication in publications)
		{
			if (seen.Add(publication.Rule))
				RenderReaderEntry(file, publication.Rule);
		}

		foreach (var rule in rules)
		{
			_seam = FollowSets.SeamOf(rule, _graph);

			_readerPart = 0;

			var reader = new ReaderWriter(this, rule);
			var body   = reader.Render(_graph.Bodies[rule]);

			file.Line($"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>");

			using (file.Block(
				$"static int {ReaderOf(rule)}(" +
				$"global::System.ReadOnlySpan<char> text, int pos, " +
				$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
			{
				file.Write(body);
			}

			file.Line();

			foreach (var (name, taken, part) in reader.Parts)
			{
				file.Line($"/// <summary>One alternative of <c>{rule.Name}</c>, read where it stood.</summary>");

				using (file.Block(
					$"static int {name}(" +
					$"global::System.ReadOnlySpan<char> text, int pos, " +
					$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways" +
					$"{DirectReaderParameters}{taken})"))
				{
					file.Write(part);
				}

				file.Line();
			}
		}

		if (rules.Any(Valued))
		{
			file.Write(RenderDirectMaterializer(rules));
			file.Line();
		}

		return file.ToString();
	}

	/// <summary>The whole input as one rule, which is what a publication asks for.</summary>
	void RenderReaderEntry(Writer file, RuleSymbol rule)
	{
		_seam = FollowSets.SeamOf(rule, _graph);

		var core   = CSharpEmitter.MethodOf(rule) + "_Whole";
		var type   = _results.QualifiedOf(rule);
		var valued = type is not null;
		var value  = valued ? $", out {type} value" : "";

		file.Line($"/// <summary>The whole input as <c>{rule.Name}</c>, read by methods.</summary>");

		using (file.Block(
			$"static int {core}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{value}{InputParameter}{TokensParameter}{ContextParameter})"))
		{
			var reader = new ReaderWriter(this, rule);
			var body   = _graph.Trivia.TryGetValue(rule, out var seam)
				? new Node.Sequence([seam, new Node.Call(rule, []), seam])
				: (Node)new Node.Call(rule, []);

			// The tape is what a refusal inside a lookahead is kept quiet by, and what the
			// records of a parse are written on; the tables are what the walk at the end
			// builds into.
			file.Line($"var ways = {WaysType}.Rent();");

			if (valued)
				file.Line("var values = DirectValues.Rent();");

			file.Line();

			using (file.Block("try"))
			{
				file.Line($"var end = {core}_Read(text, pos, ref failure, ways{DirectReaderArguments});");
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
					file.Line(
						$"{DirectMaterializer}(ways, text, values, ways.Last, 0" +
						$"{InputArgument}{TokensArgument}{ContextArgument});");
					file.Line(
						$"value = {DirectFrom(type!, "ways.Last").Replace("values", "values.V")};");
					file.Line();
				}

				file.Line("return end;");
			}

			file.Line("finally");

			using (file.Block(""))
			{
				file.Line($"{WaysType}.Return(ways);");

				if (valued)
					file.Line("DirectValues.Return(values);");
			}
		}

		file.Line();
		file.Line($"/// <summary>What <c>{rule.Name}</c> is read by, whichever stack it is read on.</summary>");

		using (file.Block(
			$"static int {core}_Read(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
		{
			var reader = new ReaderWriter(this, rule);
			var body   = _graph.Trivia.TryGetValue(rule, out var around)
				? new Node.Sequence([around, new Node.Call(rule, []), around])
				: (Node)new Node.Call(rule, []);

			file.Write(reader.Render(body, whole: true));
		}

		file.Line();
	}

	/// <summary>
	/// One rule's body as statements.
	/// </summary>
	/// <remarks>
	/// The position is a local, and every construct that reads moves it. Nothing else is
	/// carried: there is no tape while the readers commit, so a construct that fails has
	/// nothing to put back but the position, and the position is the caller's own copy.
	/// </remarks>
	/// <param name="given">Positions the rule captured before this part, which it only reads.</param>
	/// <param name="taken">Positions this part captures and something after it reads.</param>
	sealed class ReaderWriter(
		Machine machine, RuleSymbol owner, IReadOnlyList<int>? given = null, IReadOnlyList<int>? taken = null)
	{
		readonly RecognitionGraph _graph = machine._graph;

		/// <summary>What stands in this method already, and so is not declared in it.</summary>
		readonly HashSet<int> _handed = [.. given ?? [], .. taken ?? []];

		/// <summary>The alternatives written as methods of their own, and their bodies.</summary>
		public List<(string Name, string Taken, string Body)> Parts { get; } = [];

		bool _character;

		/// <summary>Whether this method writes a record, and so needs the side stack mark.</summary>
		bool _records;

		public string Render(Node body, bool whole = false)
		{
			var code = new Writer(0);

			Emit(code, body);

			if (whole)
			{
				using (code.Block("if (p != text.Length)"))
				{
					code.Line($"{Refusing}(ref failure, p, null, ways);");
					code.Line("return -1;");
				}
			}

			code.Line("return p;");

			var head = new Writer(0);

			head.Line("var p = pos;");

			if (_character)
				head.Line("var c = '\\0';");

			if (_records)
				head.Line("var rb = ways.RefsCount;");

			foreach (var slot in _kept.OrderBy(static one => one))
			{
				// A position handed in is already a name in this method.
				if (_handed.Contains(slot))
					continue;

				var member = machine.MemberOfSlot(owner, slot);

				if (member?.Shape == MemberShape.Text)
				{
					head.Line($"var a{slot} = -1;");
					head.Line($"var b{slot} = -1;");
				}
				else
				{
					head.Line($"var r{slot} = -1;");
				}
			}

			head.Write(code.ToString());

			return head.ToString();
		}

		void Emit(Writer code, Node node)
		{
			switch (node)
			{
				case Node.Empty or Node.Glue:
					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
					EmitLiteral(code, node, text, folded);
					break;

				case Node.Element element:
					EmitElement(code, element);
					break;

				case Node.Sequence(var parts):
					foreach (var part in parts)
						Emit(code, part);

					break;

				case Node.Choice(var alternatives):
					EmitChoice(code, alternatives);
					break;

				case Node.Repeat repeat:
					EmitRepeat(code, repeat);
					break;

				case Node.Call(var called, _):
					EmitCall(code, called);
					break;

				case Node.Lookahead(var positive, var inside):
					EmitLookahead(code, positive, inside);
					break;

				case Node.Capture(_, var held):
					EmitCapture(code, node, held);
					break;

				case Node.Construct(var built, _):
					Emit(code, built);
					EmitRecord(code, machine._constructs[node]);
					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed CanRead and the reader has no statement for it.");
			}
		}

		/// <summary>What the rule keeps of what it read: a record it names, or the text.</summary>
		void EmitCapture(Writer code, Node capture, Node held)
		{
			// Slots are numbered across the whole machine and members are numbered inside
			// one rule, so the rule's first slot is where the two meet (Machine.Direct.cs).
			var slot   = machine._captureSlots[capture] - machine._captureOffsets[owner];
			var member = machine.MemberOfSlot(owner, slot);

			if (member is null)
			{
				Emit(code, held);

				return;
			}

			switch (member.Shape)
			{
				case MemberShape.Text:
					code.Line($"a{slot} = p;");
					Emit(code, held);
					code.Line($"b{slot} = p;");
					break;

				case MemberShape.Record:
					Emit(code, held);
					code.Line($"r{slot} = ways.Last;");
					break;

				default:
					throw new InvalidOperationException(
						$"A {member.Shape} capture passed CanRead and the reader cannot keep it.");
			}

			_kept.Add(slot);
		}

		/// <summary>The rule's record: which arm wrote it, and each member the factory names.</summary>
		void EmitRecord(Writer code, int factory)
		{
			if (machine.DirectForwards(owner, factory))
				return;

			_records = true;

			code.Line($"ways.Begin({machine.DirectArm(owner, factory)});");

			foreach (var member in machine.DirectMembers(owner, factory))
				code.Line(
					member.Shape == MemberShape.Text
						? $"ways.Put(a{member.Slots[0]}, b{member.Slots[0]});"
						: $"ways.Put(r{member.Slots[0]});");

			code.Line("ways.End(rb);");
		}

		readonly HashSet<int> _kept = [];

		void EmitLiteral(Writer code, Node node, string text, bool folded)
		{
			if (text.Length == 0)
				return;

			var name = machine.DeclareExpected([node.ToString()]);

			if (text.Length == 1)
			{
				var read = folded ? "global::System.Char.ToUpperInvariant(text[p])" : "text[p]";
				var want = CSharpEmitter.Char(folded ? char.ToUpperInvariant(text[0]) : text[0]);

				using (code.Block($"if ((uint)p >= (uint)text.Length || {read} != {want})"))
					Refused(code, name);

				code.Line($"p += {text.Length};");

				return;
			}

			var comparison = folded
				? $"!global::System.MemoryExtensions.Equals(text.Slice(p, {text.Length}), " +
					$"{Spanned(text)}, global::System.StringComparison.OrdinalIgnoreCase)"
				: $"!global::System.MemoryExtensions.SequenceEqual(text.Slice(p, {text.Length}), {Spanned(text)})";

			using (code.Block($"if ((uint)(p + {text.Length}) > (uint)text.Length || {comparison})"))
				Refused(code, name);

			code.Line($"p += {text.Length};");
		}

		void EmitElement(Writer code, Node.Element element)
		{
			var name  = machine.DeclareExpected([element.ToString()]);
			var first = FirstSets.Of(element, _graph);

			_character = true;

			using (code.Block("if ((uint)p >= (uint)text.Length)"))
				Refused(code, name);

			code.Line("c = text[p];");

			using (code.Block($"if (!({machine.RangesTest(first.Ranges, machine.Tabulate)}))"))
				Refused(code, name);

			code.Line("p++;");
		}

		void EmitCall(Writer code, RuleSymbol called)
		{
			var result = $"q{_calls++}";

			if (machine._backEdges.Contains((owner, called)))
				code.Line("global::System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();");

			code.Line(
				$"var {result} = {machine.ReaderOf(called)}(text, p, ref failure, ways{machine.DirectReaderArguments});");
			code.Line($"if ({result} < 0) return -1;");
			code.Line($"p = {result};");
		}

		int _calls;

		/// <summary>
		/// Alternatives in order: the one the token chooses where a token chooses, and one
		/// attempt after another where none does.
		/// </summary>
		/// <remarks>
		/// Where the choice dispatches, an alternative that fails is a choice that fails —
		/// no other could have matched here — so each is written where it stands and ends
		/// the reader. Where it does not, every alternative but the last becomes a method,
		/// because <c>-1</c> is how one tells its caller to try the next.
		/// </remarks>
		void EmitChoice(Writer code, IReadOnlyList<Node> alternatives)
		{
			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0]);

				return;
			}

			if (machine.Dispatchable(alternatives) is { } groups && groups.All(one => one.Members.Count == 1))
			{
				var name = machine.DeclareExpected([machine.PredictedDisplay(alternatives)]);

				_character = true;

				using (code.Block("if ((uint)p >= (uint)text.Length)"))
					Refused(code, name);

				code.Line("c = text[p];");

				using (code.Block("switch (c)"))
				{
					foreach (var group in groups)
					{
						var labels = "";

						foreach (var range in group.Set.Ranges)
							for (var one = range.From; ; one++)
							{
								labels += $"case {CSharpEmitter.Char(one)}: ";

								if (one == range.To)
									break;
							}

						code.Line(labels);

						using (code.Indent())
						using (code.Block(""))
						{
							Emit(code, group.Members[0]);
							code.Line("break;");
						}
					}

					code.Line("default:");

					using (code.Indent())
						Refused(code, name);
				}

				return;
			}

			// One attempt after another. Each but the last is a method, so that its failure
			// is a number rather than a jump out of the middle of this one.
			var tried = $"q{_calls++}";

			code.Line($"var {tried} = -1;");

			for (var i = 0; i < alternatives.Count - 1; i++)
			{
				var part = Calling(alternatives[i]);

				using (code.Block($"if ({tried} < 0)"))
					code.Line($"{tried} = {part};");
			}

			using (code.Block($"if ({tried} < 0)"))
			{
				Emit(code, alternatives[alternatives.Count - 1]);
				code.Line($"{tried} = p;");
			}

			code.Line($"p = {tried};");
		}

		/// <summary>
		/// One part of a rule as a method, called: the position it reached, or -1.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A method cannot see a local of the method that called it, and the normalizer
		/// makes that matter: alternatives that begin alike are factored, so a head they
		/// share is read — and captured — before the choice, and the record an alternative
		/// writes names something the alternative did not read. Those positions are handed
		/// over, which is the only thing a method has to hand anything over with.
		/// </para>
		/// <para>
		/// Two directions, told apart because the difference is worth an argument. A
		/// position the part only reads goes by value. One the part captures and something
		/// after it reads goes by reference, and is the same local seen from two methods.
		/// Measured against writing the alternative in place, handing them over costs
		/// nothing at either width tried (benchmarks/README.md).
		/// </para>
		/// </remarks>
		string Calling(Node part)
		{
			var (captured, used) = Reaches(part);
			var elsewhere        = Elsewhere(part);

			var taken = captured.Where(elsewhere.Contains).OrderBy(static one => one).ToList();
			var given = used.Where(one => !captured.Contains(one)).OrderBy(static one => one).ToList();

			// What the part hands back is a local of this method, so this method declares it.
			foreach (var slot in taken)
				_kept.Add(slot);

			var name  = machine.ReaderOf(owner) + "_Part" + machine._readerPart++;
			var apart = new ReaderWriter(machine, owner, given, taken);

			Parts.Add((name, Handing(given, taken, "int "), apart.Render(part)));

			foreach (var made in apart.Parts)
				Parts.Add(made);

			return $"{name}(text, p, ref failure, ways{machine.DirectReaderArguments}" +
				$"{Handing(given, taken, "")})";
		}

		/// <summary>The positions handed over, as a signature or as a call.</summary>
		string Handing(IReadOnlyList<int> given, IReadOnlyList<int> taken, string type)
		{
			var text = new System.Text.StringBuilder();

			foreach (var slot in given)
				foreach (var name in Names(slot))
					text.Append(", ").Append(type).Append(name);

			foreach (var slot in taken)
				foreach (var name in Names(slot))
					text.Append(", ref ").Append(type).Append(name);

			return text.ToString();
		}

		/// <summary>What a position is called: two names where it is a run of text, one where it is a record.</summary>
		IEnumerable<string> Names(int slot)
		{
			if (machine.MemberOfSlot(owner, slot)?.Shape == MemberShape.Text)
			{
				yield return "a" + slot;
				yield return "b" + slot;
			}
			else
			{
				yield return "r" + slot;
			}
		}

		/// <summary>What a part captures, and what the records inside it read.</summary>
		(HashSet<int> Captured, HashSet<int> Used) Reaches(Node part)
		{
			var captured = new HashSet<int>();
			var used     = new HashSet<int>();

			foreach (var one in NodeWalk.Descendants(part))
			{
				if (one is Node.Capture)
					captured.Add(machine._captureSlots[one] - machine._captureOffsets[owner]);

				if (one is Node.Construct && !machine.DirectForwards(owner, machine._constructs[one]))
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						used.Add(member.Slots[0]);
			}

			return (captured, used);
		}

		/// <summary>What the records outside this part read.</summary>
		HashSet<int> Elsewhere(Node part)
		{
			var mine = NodeWalk.ByIdentity(NodeWalk.Descendants(part));
			var used = new HashSet<int>();

			foreach (var one in NodeWalk.Descendants(_graph.Bodies[owner]))
				if (!mine.Contains(one) && one is Node.Construct &&
					!machine.DirectForwards(owner, machine._constructs[one]))
				{
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						used.Add(member.Slots[0]);
				}

			return used;
		}

		void EmitRepeat(Writer code, Node.Repeat repeat)
		{
			var (body, min, max) = repeat;
			var turns = min > 0 || max is not null ? $"t{_calls++}" : null;

			if (turns is not null)
				code.Line($"var {turns} = 0;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if ({turns} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				var turn = $"q{_calls++}";

				code.Line($"var {turn} = {Calling(body)};");
				code.Line();
				code.Line($"if ({turn} < 0 || {turn} == p)");
				code.Then("break;");
				code.Line();
				code.Line($"p = {turn};");

				if (turns is not null)
					code.Line($"{turns}++;");
			}

			if (min > 0)
			{
				code.Line();
				code.Line($"if ({turns} < {min})");
				code.Then("return -1;");
			}
		}

		void EmitLookahead(Writer code, bool positive, Node inside)
		{
			var seen = $"q{_calls++}";

			code.Line($"var {seen} = {Calling(inside)};");
			code.Line();
			code.Line($"if ({seen} {(positive ? "<" : ">=")} 0)");
			code.Then("return -1;");
		}

		void Refused(Writer code, string expected)
		{
			code.Line($"{Refusing}(ref failure, p, {expected}, ways);");
			code.Line("return -1;");
		}
	}
}
