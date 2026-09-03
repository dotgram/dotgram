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
			// A climb is entered at a strength, which is what the rendering beside this one
			// learned last and this one has not.
			if (_graph.Climbing.ContainsKey(rule))
				return Unreadable(rule, "it climbs");

			if (_graph.Externals.ContainsKey(rule))
				return Unreadable(rule, "it is an external recognizer");

			if (IsExtent(rule))
				return Unreadable(rule, "its value is the text it matched");

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

		// Over kinds a rule's answer stands, so there is no way back to write and nothing
		// on the tape. Over characters there is, and what it can give back so far is a
		// character at a time: a repetition of one element hands back the difference by
		// arithmetic, where a repetition of anything longer needs a way for every turn.
		if (OverKinds)
		{
			foreach (var publication in publications)
				if (publication.Rule.GivesBack)
					return Unreadable(publication.Rule, "it is marked '?' and gives back");

			return true;
		}

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
			var inner  = OverKinds ? ReaderOf(rule) : ReaderOf(rule) + "_Body";

			if (!OverKinds)
				RenderWayBack(file, rule);

			file.Line(
				OverKinds
					? $"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>"
					: $"/// <summary>What <c>{rule.Name}</c> is, one reading of it at a time.</summary>");

			using (file.Block(
				$"static int {inner}(" +
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

	/// <summary>
	/// The way back into a rule: the tape, as a loop.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Over characters a rule's answer does not stand. A repetition that swallowed a
	/// character something after it needed has to hand it back, and an alternative that
	/// matched has to give way to the next when what follows cannot be read — which is
	/// asking a rule that has already answered for its next answer.
	/// </para>
	/// <para>
	/// The rendering this replaces wrote that as a label at the top of the rule and a jump
	/// to it from the bottom. Here the rule is two methods: what it is, and the way back
	/// into it. The body is called until it answers or until the tape has nothing left to
	/// move on, and the tape is what makes a second call different from the first — every
	/// decision the body took is recorded on it, and a replay reads them back rather than
	/// taking them again (<c>Support.cs</c>, <c>Ways.Retry</c>).
	/// </para>
	/// <para>
	/// One segment for the rule and not one per construct, which the rendering beside this
	/// one has. <c>Retry</c> takes the latest way with an alternative left wherever it
	/// stands, so a segment per rule reaches every way opened inside it; what the finer
	/// segments buy is running less of the rule again, and that is a measurement to make
	/// rather than a thing to assume.
	/// </para>
	/// </remarks>
	void RenderWayBack(Writer file, RuleSymbol rule)
	{
		file.Line($"/// <summary><c>{rule.Name}</c>, and the way back into it.</summary>");

		using (file.Block(
			$"static int {ReaderOf(rule)}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
		{
			file.Line("var s  = ways.Cursor;");
			file.Line("var lm = ways.LogCount;");
			file.Line("var rb = ways.RefsCount;");
			file.Line();

			using (file.Block("while (true)"))
			{
				file.Line(
					$"var q = {ReaderOf(rule)}_Body(text, pos, ref failure, ways{DirectReaderArguments});");
				file.Line();
				file.Line("if (q >= 0)");
				file.Then("return q;");
				file.Line();
				file.Line("ways.LogCount  = lm;");
				file.Line("ways.RefsCount = rb;");
				file.Line();
				file.Line("if (ways.Cursor > s && ways.Retry(s))");
				file.Then("continue;");
				file.Line();
				file.Line("return -1;");
			}
		}

		file.Line();
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
	/// <param name="handed">
	/// Whether the value built so far arrived as an argument, which every part of a folded
	/// rule but its body does.
	/// </param>
	sealed class ReaderWriter(
		Machine machine, RuleSymbol owner, IReadOnlyList<int>? given = null,
		IReadOnlyList<int>? taken = null, bool handed = false)
	{
		/// <summary>
		/// Whether the rule was left-recursive, and so is a base and a loop of steps over it.
		/// </summary>
		/// <remarks>
		/// A step's record leads with the value built so far — the record of the base or of
		/// the step before it (§4.3) — which is one local, written after every record the
		/// rule makes and read by the next step. The rule's body and every part of it are
		/// separate methods here, so the local is handed between them by reference.
		/// </remarks>
		readonly bool _folds = machine._graph.Folds.ContainsKey(owner);

		/// <summary>Whether anything the rule keeps is gathered across the turns of a repetition.</summary>
		readonly bool _gathers = machine.DirectMembers(owner)
			.Exists(one => one.Shape is MemberShape.Pieces or MemberShape.Records);

		readonly RecognitionGraph _graph = machine._graph;

		/// <summary>What stands in this method already, and so is not declared in it.</summary>
		readonly HashSet<int> _handed = [.. given ?? [], .. taken ?? []];

		/// <summary>The alternatives written as methods of their own, and their bodies.</summary>
		public List<(string Name, string Taken, string Body)> Parts { get; } = [];

		bool _character;

		/// <summary>Ways opened and marks taken, which only a reading over characters has.</summary>
		int _ways;
		int _marks;
		int _turns;

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

			var written = code.ToString();
			var head    = new Writer(0);

			head.Line("var p = pos;");

			if (_character)
				head.Line("var c = '\\0';");

			if (_records)
				head.Line("var rb = ways.RefsCount;");

			// Only where something in the method names it: a rule folds, but a method of it
			// that neither writes a record nor hands the value on has nothing to do with it,
			// and a local nothing reads is an error in somebody else's build.
			if (_folds && !handed && written.Contains("fold", StringComparison.Ordinal))
				head.Line("var fold = -1;");

			foreach (var slot in _kept.OrderBy(static one => one))
			{
				// A position handed in is already a name in this method.
				if (_handed.Contains(slot))
					continue;

				switch (machine.MemberOfSlot(owner, slot)?.Shape)
				{
					case MemberShape.Text:
						head.Line($"var a{slot} = -1;");
						head.Line($"var b{slot} = -1;");
						break;

					// Where it was pushed from is all a gathered run of text keeps: the end
					// is the position the push is written at.
					case MemberShape.Pieces:
						head.Line($"var a{slot} = -1;");
						break;

					default:
						head.Line($"var r{slot} = -1;");
						break;
				}
			}

			head.Write(written);

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

				// What a repetition gathers is pushed as it goes and collected when the
				// record is written: the pushes are on the tape, which every method of the
				// rule shares, so nothing has to be handed between them.
				case MemberShape.Pieces:
					code.Line($"a{slot} = p;");
					Emit(code, held);
					code.Line($"ways.Push({slot}, a{slot}, p);");
					break;

				case MemberShape.Records:
					Emit(code, held);
					code.Line($"ways.Push({slot}, ways.Last, -1);");
					break;

				default:
					throw new InvalidOperationException(
						$"A {member.Shape} capture passed CanRead and the reader cannot keep it.");
			}

			if (member.Shape != MemberShape.Records)
				_kept.Add(slot);
		}

		/// <summary>The rule's record: which arm wrote it, and each member the factory names.</summary>
		void EmitRecord(Writer code, int factory)
		{
			// An alternative that only hands its operand up writes no record of its own, and
			// the operand's is the value: for a folded rule that is what the step after it
			// builds on, so the local still moves.
			if (machine.DirectForwards(owner, factory))
			{
				if (_folds)
					code.Line("fold = ways.Last;");

				return;
			}

			_records = true;

			code.Line($"ways.Begin({machine.DirectArm(owner, factory)});");

			// A fold step's first member is the value so far, and each of the rest is the
			// one thing the step captured (§4.3).
			if (machine.IsStep(owner, factory))
				code.Line("ways.Put(fold);");

			foreach (var member in machine.DirectMembers(owner, factory))
				code.Line(member.Shape switch
				{
					MemberShape.Text    => $"ways.Put(a{member.Slots[0]}, b{member.Slots[0]});",
					MemberShape.Pieces  => $"ways.Collect(rb, {member.Mask}L, true);",
					MemberShape.Records => $"ways.Collect(rb, {member.Mask}L, false);",
					_                   => $"ways.Put(r{member.Slots[0]});",
				});

			code.Line("ways.End(rb);");

			if (_folds)
				code.Line("fold = ways.Last;");
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

			if (!machine.OverKinds)
			{
				EmitChoiceOverCharacters(code, alternatives, tried);

				return;
			}

			code.Line($"var {tried} = -1;");

			for (var i = 0; i < alternatives.Count - 1; i++)
			{
				var (part, undo) = Called(alternatives[i]);

				using (code.Block($"if ({tried} < 0)"))
				{
					// What an alternative that failed pushed is not the rule's, and the
					// record written after it collects everything pushed since it began.
					var back = _gathers ? _ways++ : -1;

					if (back >= 0)
					{
						code.Line($"var rr{back} = ways.RefsCount;");
						code.Line();
					}

					code.Line($"{tried} = {part};");

					if (back >= 0 || undo.Length > 0)
					{
						code.Line();

						using (code.Block($"if ({tried} < 0)"))
						{
							if (back >= 0)
								code.Line($"ways.RefsCount = rr{back};");

							if (undo.Length > 0)
								code.Line(undo);
						}
					}
				}
			}

			using (code.Block($"if ({tried} < 0)"))
			{
				Emit(code, alternatives[alternatives.Count - 1]);
				code.Line($"{tried} = p;");
			}

			code.Line($"p = {tried};");
		}

		/// <summary>
		/// A choice over characters: which alternative was taken is on the tape, so that a
		/// failure after the choice can come back and ask for the next one.
		/// </summary>
		/// <remarks>
		/// The first reading opens a way standing at the first alternative and reaching to
		/// the last; a reading that is a replay finds the way already there and takes what
		/// it says. An alternative that fails where it stands moves the way on itself, so
		/// the run continues into the next without the tape having to be asked again.
		/// </remarks>
		void EmitChoiceOverCharacters(Writer code, IReadOnlyList<Node> alternatives, string tried)
		{
			var way  = $"w{_ways}";
			var took = $"d{_ways++}";

			code.Line($"var {way}  = -1;");
			code.Line($"var {took} = 0;");

			using (code.Block("if (ways.Cursor < ways.Count)"))
			{
				code.Line($"{way}  = ways.Cursor;");
				code.Line($"{took} = ways.Items[{way} * 2];");
				code.Line("ways.Cursor++;");
			}

			code.Line("else");

			using (code.Block(""))
				code.Line($"{way} = ways.Open(0, {alternatives.Count - 1});");

			code.Line();
			code.Line($"var {tried} = -1;");

			for (var i = 0; i < alternatives.Count; i++)
			{
				using (code.Block($"if ({tried} < 0 && {took} <= {i})"))
				{
					// An alternative is asked for every reading it has before the choice
					// moves on. Without this the way the choice stands on is spent while a
					// run inside the alternative still had a shorter reading to give, and
					// that reading becomes unreachable: the tape says the choice has moved
					// past the alternative it was in.
					var segment = _ways++;

					code.Line($"var s{segment}  = ways.Cursor;");
					code.Line($"var lm{segment} = ways.LogCount;");
					code.Line($"var rr{segment} = ways.RefsCount;");
					code.Line();

					var (call, undo) = Called(alternatives[i]);

					using (code.Block("while (true)"))
					{
						code.Line($"{tried} = {call};");
						code.Line();
						code.Line($"if ({tried} >= 0)");
						code.Then("break;");
						code.Line();
						code.Line($"ways.LogCount  = lm{segment};");
						code.Line($"ways.RefsCount = rr{segment};");

						if (undo.Length > 0)
							code.Line(undo);

						code.Line();
						code.Line($"if (ways.Cursor > s{segment} && ways.Retry(s{segment}))");
						code.Then("continue;");
						code.Line();
						code.Line("break;");
					}

					if (i < alternatives.Count - 1)
					{
						code.Line();
						code.Line($"if ({tried} < 0)");
						code.Then($"ways.Next({way}, {i + 1}, {alternatives.Count - 1});");
					}
				}
			}

			code.Line();
			code.Line($"if ({tried} < 0)");
			code.Then("return -1;");
			code.Line();
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
		string Calling(Node part) => Called(part).Call;

		/// <summary>
		/// The call, and what has to be put back where it failed.
		/// </summary>
		/// <remarks>
		/// A position handed over by reference is written by the part whether the part goes
		/// on to answer or not, and a part that failed did not capture what it wrote there.
		/// Leaving it is the defect this exists for: the alternative after it writes a
		/// record naming the position, and gets the abandoned one.
		/// </remarks>
		(string Call, string Undo) Called(Node part)
		{
			var (captured, used) = Reaches(part);
			var elsewhere        = Elsewhere(part);

			var taken = captured.Where(elsewhere.Contains).OrderBy(static one => one).ToList();
			var given = used.Where(one => !captured.Contains(one)).OrderBy(static one => one).ToList();

			// What the part hands back is a local of this method, so this method declares it.
			foreach (var slot in taken)
				_kept.Add(slot);

			var name  = machine.ReaderOf(owner) + "_Part" + machine._readerPart++;
			var apart = new ReaderWriter(machine, owner, given, taken, _folds);

			Parts.Add((name, Handing(given, taken, "int "), apart.Render(part)));

			foreach (var made in apart.Parts)
				Parts.Add(made);

			var undo = new System.Text.StringBuilder();

			foreach (var slot in taken)
				foreach (var name2 in Names(slot))
					undo.Append(name2).Append(" = -1; ");

			return (
				$"{name}(text, p, ref failure, ways{machine.DirectReaderArguments}" +
					$"{Handing(given, taken, "")})",
				undo.ToString().TrimEnd());
		}

		/// <summary>The positions handed over, as a signature or as a call.</summary>
		string Handing(IReadOnlyList<int> given, IReadOnlyList<int> taken, string type)
		{
			var text = new System.Text.StringBuilder();

			if (_folds)
				text.Append(", ref ").Append(type).Append("fold");

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

			if (!machine.OverKinds)
			{
				if (body is Node.Element element)
					EmitRun(code, element, min, max);
				else
					EmitTurns(code, repeat);

				return;
			}
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
				var back = _gathers ? _ways++ : -1;
				var (call, undo) = Called(body);

				if (back >= 0)
				{
					code.Line($"var rr{back} = ways.RefsCount;");
					code.Line();
				}

				code.Line($"var {turn} = {call};");
				code.Line();

				using (code.Block($"if ({turn} < 0 || {turn} == p)"))
				{
					if (back >= 0)
						code.Line($"ways.RefsCount = rr{back};");

					if (undo.Length > 0)
						code.Line(undo);

					code.Line("break;");
				}
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

		/// <summary>
		/// A run of one element, read where it stands and given back a character at a time.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A method a turn is what a repetition of anything else costs, and for a character
		/// class it is all cost: the turn is a bounds check, a read and a test. So it is
		/// written as the loop it is.
		/// </para>
		/// <para>
		/// And the loop takes everything it can, which is not always what the rule wanted.
		/// What it can give back is the difference between where it stopped and the fewest
		/// turns it was allowed — one number, so the tape carries how many characters were
		/// handed back rather than a way for each. Replayed, the run reaches the same end
		/// and hands back what the tape says.
		/// </para>
		/// </remarks>
		void EmitRun(Writer code, Node.Element element, int min, int? max)
		{
			var name  = machine.DeclareExpected([element.ToString()]);
			var first = FirstSets.Of(element, _graph);
			var mark  = $"m{_marks++}";

			_character = true;

			code.Line($"var {mark} = p;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if (p - {mark} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				code.Line("if ((uint)p >= (uint)text.Length)");
				code.Then("break;");
				code.Line();
				code.Line("c = text[p];");
				code.Line();
				code.Line($"if (!({machine.RangesTest(first.Ranges, machine.Tabulate)}))");
				code.Then("break;");
				code.Line();
				code.Line("p++;");
			}

			code.Line();

			if (min > 0)
			{
				using (code.Block($"if (p < {mark} + {min})"))
					Refused(code, name);

				code.Line();
			}

			var floor = min == 0 ? mark : $"({mark} + {min})";
			var gave  = $"d{_ways++}";

			using (code.Block($"if (p > {floor})"))
			{
				code.Line($"var {gave} = 0;");
				code.Line();

				using (code.Block("if (ways.Cursor < ways.Count)"))
				{
					code.Line($"{gave} = ways.Items[ways.Cursor * 2];");
					code.Line("ways.Cursor++;");
				}

				code.Line("else");

				using (code.Block(""))
					code.Line($"ways.Open(p - {floor});");

				code.Line();
				code.Line($"p -= {gave};");
			}
		}

		/// <summary>
		/// A repetition of anything longer than one element, over characters: a way for
		/// every turn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// What a run of characters gives back is a count, because every turn of it is one
		/// character and a shorter run is the same scan stopped earlier. A turn of anything
		/// else is not a character and not the same size as its neighbours, so what has to
		/// be given back is the turn itself — and the tape carries that as a way per turn,
		/// each standing at "went round again" and reaching to "stopped here".
		/// </para>
		/// <para>
		/// The way is opened before the turn rather than after it, and only where stopping
		/// is allowed: below the minimum there is nothing to offer, because stopping there
		/// is not a reading the repetition has. A turn that fails spends its own way — the
		/// way now says "stopped here" — and a turn that fails below the minimum fails the
		/// repetition.
		/// </para>
		/// </remarks>
		void EmitTurns(Writer code, Node.Repeat repeat)
		{
			var (body, min, max) = repeat;

			if (max == 0)
				return;

			var turn     = min > 0 || max is not null ? $"t{_turns++}" : null;
			var nullable = FirstSets.Nullable(body, _graph);
			var (call, undo) = Called(body);

			if (turn is not null)
				code.Line($"var {turn} = 0;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if ({turn} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				var way   = _ways++;
				var stops = min > 0 ? $"{turn} >= {min}" : null;

				code.Line($"var w{way} = -1;");
				code.Line($"var d{way} = 0;");
				code.Line();

				// Below the minimum there is nothing to offer: stopping there is not a
				// reading the repetition has.
				var offering = stops is null ? null : code.Block($"if ({stops})");

				using (code.Block("if (ways.Cursor < ways.Count)"))
				{
					code.Line($"w{way} = ways.Cursor;");
					code.Line($"d{way} = ways.Items[w{way} * 2];");
					code.Line("ways.Cursor++;");
				}

				code.Line("else");

				using (code.Block(""))
					code.Line($"w{way} = ways.Open(1);");

				code.Line();
				code.Line($"if (d{way} == 1)");
				code.Then("break;");

				offering?.Dispose();

				code.Line();

				// The turn is asked for every reading it has before it is called spent,
				// which is the alternative's rule (EmitChoiceOverCharacters) over again.
				var segment = _ways++;
				var took    = $"q{_calls++}";

				code.Line($"var s{segment}  = ways.Cursor;");
				code.Line($"var lm{segment} = ways.LogCount;");
				code.Line($"var rr{segment} = ways.RefsCount;");
				code.Line($"var {took} = -1;");
				code.Line();

				using (code.Block("while (true)"))
				{
					code.Line($"{took} = {call};");
					code.Line();
					code.Line($"if ({took} >= 0)");
					code.Then("break;");
					code.Line();
					code.Line($"ways.LogCount  = lm{segment};");
					code.Line($"ways.RefsCount = rr{segment};");
					code.Line();
					code.Line($"if (ways.Cursor > s{segment} && ways.Retry(s{segment}))");
					code.Then("continue;");
					code.Line();
					code.Line("break;");
				}

				code.Line();

				using (code.Block($"if ({took} < 0)"))
				{
					if (undo.Length > 0)
					{
						code.Line(undo);
						code.Line();
					}

					if (stops is null)
					{
						code.Line($"ways.Next(w{way}, 1);");
					}
					else
					{
						// The turn is spent, so the way that offered it now says "stopped
						// here" — and a turn short of the minimum is not a stop but a
						// refusal, which the body has already said everything about.
						code.Line($"if ({stops})");
						code.Then($"ways.Next(w{way}, 1);");
						code.Line();
						code.Line($"if ({turn} < {min})");
						code.Then("return -1;");
					}

					code.Line();
					code.Line("break;");
				}

				code.Line();

				if (nullable)
				{
					code.Line($"if ({took} == p)");
					code.Then("break;");
					code.Line();
				}

				code.Line($"p = {took};");

				if (turn is not null)
					code.Line($"{turn}++;");
			}
		}

		void EmitLookahead(Writer code, bool positive, Node inside)
		{
			var seen = $"q{_calls++}";

			if (machine.OverKinds)
			{
				code.Line($"var {seen} = {Calling(inside)};");
			}
			else
			{
				// What a look decided is its own and nothing after it may reopen: its
				// outcome is one bit, and a second reading of it can only say the same.
				var segment = $"s{_ways++}";

				code.Line($"var {segment} = ways.Cursor;");
				code.Line($"var {seen} = {Calling(inside)};");
				code.Line($"ways.Seal({segment});");
			}

			code.Line();
			code.Line($"if ({seen} {(positive ? "<" : ">=")} 0)");
			code.Then("return -1;");
		}

		void Refused(Writer code, string expected)
		{
			// Declaring the array is not asking for it: what is written out is what
			// something wrote a reference to, and until the reader said so the only thing
			// that ever did was the rendering beside it.
			machine._expectedUsed.Add(expected);

			code.Line($"{Refusing}(ref failure, p, {expected}, ways);");
			code.Line("return -1;");
		}
	}
}
