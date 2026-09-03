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
			if (_graph.Externals.ContainsKey(rule))
				return Unreadable(rule, "it is an external recognizer");

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

					case Node.Capture or Node.Construct or Node.Atomic or Node.Guard or Node.Marked:
						break;

					default:
						return Unreadable(rule, $"of a node it cannot write: {node.GetType().Name.ToLowerInvariant()}");
				}
		}

		// Over kinds a rule's answer stands, so there is no way back to write and nothing
		// on the tape. Over characters there is, and what it can give back so far is a
		// character at a time: a repetition of one element hands back the difference by
		// arithmetic, where a repetition of anything longer needs a way for every turn.
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
			var tape   = !OverKinds || rule.GivesBack;
			var inner  = tape ? ReaderOf(rule) + "_Body" : ReaderOf(rule);

			if (tape)
				RenderWayBack(file, rule, DirectStrength(rule), seal: OverKinds);

			file.Line(
				tape
					? $"/// <summary>What <c>{rule.Name}</c> is, one reading of it at a time.</summary>"
					: $"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>");

			using (file.Block(
				$"static int {inner}(" +
				$"global::System.ReadOnlySpan<char> text, int pos, " +
				$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters}{DirectStrength(rule)})"))
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
	void RenderWayBack(Writer file, RuleSymbol rule, string strength, bool seal = false) =>
		RenderWayBack(file, ReaderOf(rule), $"/// <summary><c>{rule.Name}</c>, and the way back into it.</summary>", strength, seal);

	/// <param name="seal">
	/// Whether the ways the body opened are sealed once it has answered: a rule marked
	/// <c>?</c> over kinds gives back inside itself, and once it has answered the answer
	/// stands. Sealed rather than dropped, so that a replay reads the same decisions.
	/// </param>
	void RenderWayBack(Writer file, string name, string summary, string strength, bool seal = false)
	{
		file.Line(summary);

		using (file.Block(
			$"static int {name}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters}{strength})"))
		{
			file.Line("var s  = ways.Cursor;");
			file.Line("var lm = ways.LogCount;");
			file.Line("var rb = ways.RefsCount;");
			file.Line();

			using (file.Block("while (true)"))
			{
				file.Line(
					$"var q = {name}_Body(text, pos, ref failure, ways{DirectReaderArguments}{(strength.Length > 0 ? ", power" : "")});");
				file.Line();
				if (seal)
				{
					using (file.Block("if (q >= 0)"))
					{
						file.Line("ways.Seal(s);");
						file.Line();
						file.Line("return q;");
					}
				}
				else
				{
					file.Line("if (q >= 0)");
					file.Then("return q;");
				}

				file.Line();
				file.Line("ways.LogCount  = lm;");

				if (_directBuilds)
					file.Line("if (ways.Built > lm) ways.Built = lm;");

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
		var climbs = _graph.Climbing.ContainsKey(rule);
		var asked  = climbs ? ", power" : "";

		file.Line($"/// <summary>The whole input as <c>{rule.Name}</c>, read by methods.</summary>");

		// The parameters in the order the wrapper hands them: the strength beside the
		// position, then the value, the input, the tokens, the context (CSharpEmitter.EmitPublication).
		using (file.Block(
			$"static int {core}(" +
			$"global::System.ReadOnlySpan<char> text, int pos{(climbs ? ", int power" : "")}, " +
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
				file.Line($"var end = {core}_Read(text, pos, ref failure, ways{DirectReaderArguments}{asked});");
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
						// An extent's value is the span its record stands on; every other value is
						// in the tables the walk filled.
						$"value = {(IsExtent(rule) ? RecordValue(type!, "ways.Last").Replace("log[", "ways.Log[") : DirectFrom(type!, "ways.Last").Replace("values", "values.V"))};");
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

		// Over characters the whole input is a reading like any other: the rule may have
		// answered with less than all of it, and then it is asked for its next answer
		// rather than refused. Without this the very first reading that reached the end
		// short was the last, whatever the tape still had to offer.
		if (!OverKinds)
			RenderWayBack(file, core + "_Read", $"/// <summary>The whole input as <c>{rule.Name}</c>, and the way back into it.</summary>", DirectStrength(rule));

		file.Line($"/// <summary>What <c>{rule.Name}</c> is read by, whichever stack it is read on.</summary>");

		using (file.Block(
			$"static int {core}_Read{(OverKinds ? "" : "_Body")}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters}{DirectStrength(rule)})"))
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

		/// <summary>
		/// Whether a record carries where it stood, which the walk reads where a factory
		/// asks for the text or the span and where a terminal is reread from its span.
		/// </summary>
		/// <remarks>
		/// Where it does, the walk reads a four-word header and a record written with two
		/// puts it two words out of step with every record after it. The start it carries
		/// is the rule's, which in a part is not that method's own <c>pos</c>: it arrives
		/// as <c>start</c>.
		/// </remarks>
		readonly bool _positions = machine._directRules is { } placed && machine.DirectPositions(placed);

		/// <summary>Whether this method is a part of a rule rather than its body.</summary>
		readonly bool _part = given is not null;

		/// <summary>
		/// Whether the rule is written with binding powers (§4.3.1), and so is entered at a
		/// strength: every method of it takes <c>power</c>, and an alternative below the
		/// strength asked for is refused without a word.
		/// </summary>
		readonly bool _climbs = machine._graph.Climbing.ContainsKey(owner);

		/// <summary>Whether this is the entry's own reading, which calls the rule at the strength it was asked.</summary>
		bool _entry;

		/// <summary>
		/// Whether this rule writes on the tape: every rule over characters, and over kinds
		/// the one marked <c>?</c> (§4), which gives back inside itself — its choices and
		/// runs are recorded, its own failures are retried, and once it has answered the
		/// answer stands and is sealed. A caller, which commits, is never sent back into it.
		/// </summary>
		readonly bool _tape = !machine.OverKinds || owner.GivesBack;

		/// <summary>
		/// Whether the rule has a <c>when</c> in it (§7.7), which reads what the rule has
		/// captured so far and the text from where the rule began: every method of the rule
		/// is handed the rule's start and its log mark for that.
		/// </summary>
		readonly bool _guarded = NodeWalk.Descendants(machine._graph.Bodies[owner]).Any(static one => one is Node.Guard);

		/// <summary>
		/// Whether anything in this reading writes a record, and so a failed attempt can
		/// leave records on the log that nothing will ever refer to.
		/// </summary>
		/// <remarks>
		/// The log is put back when it does. Nothing was wrong while it was not — the walk
		/// at the end follows references and an abandoned record has none — but the log
		/// grew with every alternative that failed, the value tables were sized to it, and
		/// returning them cleared what had never been used. Half a microsecond a parse on
		/// the SQL yardstick, measured (docs/next.md).
		/// </remarks>
		readonly bool _logs = machine._directRules is { } rules && rules.Any(machine.Valued);

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

			_entry = whole;

			Emit(code, body);

			// A rule whose value is the record of its captures and nothing more has no
			// construction to write that record at, so it is written where the rule ends —
			// by the rule's body and not by a part of it, and not by the entry that reads
			// the whole input through it.
			if (!whole && !_part && machine.RecordsAtEnd(owner))
				EmitRecord(code, -1);

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

			// The refs mark: where this method writes a record, and where the rule gathers
			// and this is its body, which hands the mark to every part whether or not it
			// writes one itself.
			if (_records || (_gathers && !_part))
				head.Line("var rb = ways.RefsCount;");

			// Only where something in the method names it: a rule folds, but a method of it
			// that neither writes a record nor hands the value on has nothing to do with it,
			// and a local nothing reads is an error in somebody else's build.
			if (_folds && !handed && written.Contains("fold", StringComparison.Ordinal))
				head.Line("var fold = -1;");

			// Where the log stood when the rule began, for a guard that builds a value from
			// what has been recorded since.
			if (_guarded && !_part)
				head.Line("var lm = ways.LogCount;");

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

		/// <param name="loaded">
		/// Whether <c>c</c> already holds <c>text[p]</c> and the position is known to be in
		/// bounds — true right after a choice's dispatch, and carried only as far as
		/// nothing has consumed. What it saves is what every alternative of a dispatched
		/// choice used to do over again: its own bounds check and its own read of the
		/// character the switch had just read.
		/// </param>
		void Emit(Writer code, Node node, bool loaded = false)
		{
			// An alternative of a rule written with binding powers is entered only at a
			// strength it allows. Refused without a word: what could not be entered here was
			// never expected here, and the alternatives after it still can be.
			if (machine._owners.TryGetValue(node, out var climbs) &&
				_graph.Climbing.TryGetValue(climbs, out var levels) &&
				levels.TryGetValue(node, out var level))
			{
				code.Line($"if ({level} < power)");
				code.Then("return -1;");
				code.Line();
			}

			switch (node)
			{
				case Node.Empty:
					break;

				// Two tokens with nothing between them (`~`): over kinds the next has to begin
				// where the last ended, which the token positions say; over characters there
				// is nothing between two characters to begin with.
				case Node.Glue:
					if (machine.OverKinds)
					{
						using (code.Block(
							"if (p > 0 && p < text.Length && " +
							"parserStarts[p - 1] + parserLengths[p - 1] != parserStarts[p])"))
						{
							Refused(code, machine.DeclareExpected([node.ToString()]));
						}
					}

					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
					EmitLiteral(code, node, text, folded, loaded);
					break;

				case Node.Element element:
					EmitElement(code, element, loaded);
					break;

				case Node.Sequence(var parts):
					for (var i = 0; i < parts.Count; i++)
						Emit(code, parts[i], loaded && i == 0);

					break;

				case Node.Choice(var alternatives):
					EmitChoice(code, alternatives);
					break;

				case Node.Repeat repeat:
					EmitRepeat(code, repeat);
					break;

				case Node.Call(var called, _):
					EmitCall(code, node, called);
					break;

				case Node.Lookahead(var positive, var inside):
					EmitLookahead(code, positive, inside);
					break;

				case Node.Capture(_, var held):
					EmitCapture(code, node, held, loaded);
					break;

				// A look at the character behind, which reads into `c`: where the token in
				// hand was in `c`, it is put back after.
				case Node.Behind(var boundary):
				{
					var name = machine.DeclareExpected([node.ToString()]);

					_character = true;

					using (code.Block("if (p > 0)"))
					{
						code.Line("c = text[p - 1];");
						code.Line();

						using (code.Block($"if ({CSharpEmitter.Test(boundary, machine.Tabulate)})"))
							Refused(code, name);

						if (loaded)
						{
							code.Line();
							code.Line("c = text[p];");
						}
					}

					break;
				}

				// A recognizer the author wrote, handed the position by reference: it says
				// yes or no, and where it said no is where it left the position.
				case Node.External(var method):
					using (code.Block($"if (!{method}(text, ref p))"))
					{
						code.Line($"{Refusing}(ref failure, p, null, ways);");
						code.Line("return -1;");
					}

					break;

				case Node.Atomic(var kept):
					EmitAtomic(code, kept, loaded);
					break;

				case Node.Guard guard:
					EmitGuard(code, guard);
					break;

				// A mark is a record of its own: it goes with the log wherever the log is put
				// back, which is the whole of what an abandoned reading owes it (§7.8).
				case Node.Marked(var marked, var text):
				{
					var site = machine.MarkSite(text);

					code.Line($"ways.Mark(-1, {site}, p);");
					Emit(code, marked, loaded);
					code.Line($"ways.Mark(-2, {site}, p);");

					break;
				}

				case Node.Construct(var built, _):
					Emit(code, built, loaded);
					EmitRecord(code, machine._constructs[node]);
					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed CanRead and the reader has no statement for it.");
			}
		}

		/// <summary>What the rule keeps of what it read: a record it names, or the text.</summary>
		void EmitCapture(Writer code, Node capture, Node held, bool loaded = false)
		{
			// Slots are numbered across the whole machine and members are numbered inside
			// one rule, so the rule's first slot is where the two meet (Machine.Direct.cs).
			var slot   = machine._captureSlots[capture] - machine._captureOffsets[owner];
			var member = machine.MemberOfSlot(owner, slot);

			if (member is null)
			{
				Emit(code, held, loaded);

				return;
			}

			switch (member.Shape)
			{
				case MemberShape.Text:
					code.Line($"a{slot} = p;");
					Emit(code, held, loaded);
					code.Line($"b{slot} = p;");
					break;

				case MemberShape.Record:
					Emit(code, held, loaded);
					code.Line($"r{slot} = ways.Last;");
					break;

				// What a repetition gathers is pushed as it goes and collected when the
				// record is written: the pushes are on the tape, which every method of the
				// rule shares, so nothing has to be handed between them.
				case MemberShape.Pieces:
					code.Line($"a{slot} = p;");
					Emit(code, held, loaded);
					code.Line($"ways.Push({slot}, a{slot}, p);");
					break;

				case MemberShape.Records:
					Emit(code, held, loaded);
					code.Line($"ways.Push({slot}, ways.Last, -1);");
					break;

				default:
					throw new InvalidOperationException(
						$"A {member.Shape} capture passed CanRead and the reader cannot keep it.");
			}

			if (member.Shape != MemberShape.Records)
				_kept.Add(slot);
		}

		/// <summary>Where the rule's pushes begin: this method's own mark in the body, the handed one in a part.</summary>
		string Refs => _part && _gathers ? "refs" : "rb";

		/// <summary>
		/// The one of a member's slots that was written: the same name in two alternatives is
		/// one member with a slot per alternative, and the record takes whichever is set.
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

		/// <summary>The rule's record: which arm wrote it, and each member the factory names.</summary>
		void EmitRecord(Writer code, int factory)
		{
			// An alternative that only hands its operand up writes no record of its own, and
			// the operand's is the value: for a folded rule that is what the step after it
			// builds on, so the local still moves.
			if (factory >= 0 && machine.DirectForwards(owner, factory))
			{
				if (_folds)
					code.Line("fold = ways.Last;");

				return;
			}

			_records = true;

			code.Line(
				_positions
					? $"ways.Begin({machine.DirectArm(owner, factory)}, {(_part ? "start" : "pos")}, p);"
					: $"ways.Begin({machine.DirectArm(owner, factory)});");

			// A fold step's first member is the value so far, and each of the rest is the
			// one thing the step captured (§4.3).
			if (machine.IsStep(owner, factory))
				code.Line("ways.Put(fold);");

			foreach (var member in machine.DirectMembers(owner, factory))
				code.Line(member.Shape switch
				{
					MemberShape.Text    => $"ways.Put({First("a", "a", member.Slots)}, {First("a", "b", member.Slots)});",
					MemberShape.Pieces  => $"ways.Collect({Refs}, {member.Mask}L, true);",
					MemberShape.Records => $"ways.Collect({Refs}, {member.Mask}L, false);",
					_                   => $"ways.Put({First("r", "r", member.Slots)});",
				});

			code.Line("ways.End(rb);");

			if (_folds)
				code.Line("fold = ways.Last;");
		}

		readonly HashSet<int> _kept = [];

		void EmitLiteral(Writer code, Node node, string text, bool folded, bool loaded = false)
		{
			if (text.Length == 0)
				return;

			var name = machine.DeclareExpected([node.ToString()]);

			if (text.Length == 1)
			{
				// The token the dispatch read is the one this wants, and is in a register:
				// there is nothing to bound and nothing to read.
				var read = loaded
					? folded ? "global::System.Char.ToUpperInvariant(c)" : "c"
					: folded ? "global::System.Char.ToUpperInvariant(text[p])" : "text[p]";
				var want = CSharpEmitter.Char(folded ? char.ToUpperInvariant(text[0]) : text[0]);
				var room = loaded ? "" : "(uint)p >= (uint)text.Length || ";

				if (loaded)
					_character = true;

				using (code.Block($"if ({room}{read} != {want})"))
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

		void EmitElement(Writer code, Node.Element element, bool loaded = false)
		{
			var name  = machine.DeclareExpected([element.ToString()]);
			var first = FirstSets.Of(element, _graph);

			_character = true;

			if (!loaded)
			{
				using (code.Block("if ((uint)p >= (uint)text.Length)"))
					Refused(code, name);

				code.Line("c = text[p];");
			}

			var test = CSharpEmitter.Test(element, machine.Tabulate);

			if (!string.Equals(test, "true", StringComparison.Ordinal))
				using (code.Block($"if (!({test}))"))
					Refused(code, name);

			code.Line("p++;");
		}

		void EmitCall(Writer code, Node call, RuleSymbol called)
		{
			var result = $"q{_calls++}";

			if (machine._backEdges.Contains((owner, called)))
				code.Line("global::System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();");

			// The strength the operand is read at: what `<<` or `>>` recorded against this
			// call, everything where nothing was recorded, and for the entry's own call of
			// the rule it reads through, whatever the entry was asked.
			var strength = _entry && ReferenceEquals(called, owner) && _graph.Climbing.ContainsKey(called)
				? ", power"
				: machine.DirectStrengthOf(call, called);

			code.Line(
				$"var {result} = {machine.ReaderOf(called)}(text, p, ref failure, ways{machine.DirectReaderArguments}{strength});");
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

			if (machine.Dispatchable(alternatives) is { } groups)
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
							// A group is what one token cannot tell apart. Reaching it is a
							// jump table; inside it there is nothing to tell the members
							// apart by, so they are tried in order — and a group that fails
							// fails the choice, no other group's first set holding the token
							// that chose this one.
							EmitAmong(code, group.Members, loaded: true);
							code.Line("break;");
						}
					}

					code.Line("default:");

					using (code.Indent())
						Refused(code, name);
				}

				return;
			}

			EmitAmong(code, alternatives);
		}

		/// <summary>
		/// Alternatives with nothing to tell them apart by: one attempt after another.
		/// </summary>
		/// <remarks>
		/// Each but the last is a method, so that its failure is a number rather than a
		/// jump out of the middle of this one. Called at the top of a choice nothing can
		/// dispatch, and inside a group of one that can.
		/// </remarks>
		void EmitAmong(Writer code, IReadOnlyList<Node> alternatives, bool loaded = false)
		{
			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0], loaded);

				return;
			}

			var tried = $"q{_calls++}";

			// Where the token in hand can begin none of the alternatives, the choice is
			// refused here and as one thing — the expectation the rendering beside this one
			// reports — rather than alternative by alternative, each recording its own
			// refusal at the same place and the message listing them all.
			if (alternatives[alternatives.Count - 1] is not Node.Empty && Door(alternatives) is { } gate)
			{
				var whole = machine.DeclareExpected([machine.PredictedDisplay(alternatives)]);

				if (!loaded)
				{
					using (code.Block("if ((uint)p >= (uint)text.Length)"))
						Refused(code, whole);

					code.Line("c = text[p];");
					code.Line();
				}

				using (code.Block($"if (!({gate}))"))
					Refused(code, whole);

				code.Line();
			}

			if (_tape)
			{
				EmitChoiceOverCharacters(code, alternatives, tried);

				return;
			}

			// An optional is a choice whose last alternative is nothing, and trying the
			// others where the token in hand cannot begin any of them is a refusal for
			// each — recorded, and on a tie between them allocated — for a reading that was
			// always going to be the empty one. So the token is looked at first, which is
			// the door the rendering beside this one has always had.
			var door = alternatives[alternatives.Count - 1] is Node.Empty
				? Door(alternatives.Take(alternatives.Count - 1))
				: null;

			IDisposable? opened = null;

			if (door is not null)
			{
				// A door that does not open is still what the rule wanted here, and says so
				// — the rendering beside this one records the first test of the alternative
				// it did not take, and a message that leaves it out is a worse message —
				// but it is a note and not a failure: the reading goes on with nothing.
				var wanted = machine.DeclareExpected(
					[machine.PredictedDisplay(alternatives.Take(alternatives.Count - 1).ToList())]);

				if (!loaded)
				{
					code.Line("if ((uint)p >= (uint)text.Length)");
					code.Then(Noted(wanted));
					code.Line("else");

					var outer = code.Block("");

					code.Line("c = text[p];");
					code.Line();
					code.Line($"if (!({door}))");
					code.Then(Noted(wanted));
					code.Line("else");

					opened = new Both(outer, code.Block(""));
				}
				else
				{
					code.Line($"if (!({door}))");
					code.Then(Noted(wanted));
					code.Line("else");

					opened = code.Block("");
				}
			}

			code.Line($"var {tried} = -1;");

			for (var i = 0; i < alternatives.Count - 1; i++)
			{
				var (part, undo) = Called(alternatives[i]);

				using (code.Block($"if ({tried} < 0)"))
				{
					// What an alternative that failed wrote is not the rule's: what it
					// pushed, the record written after it would collect, and what it
					// logged would size the value tables.
					var back = _gathers || _logs ? _ways++ : -1;

					if (back >= 0)
					{
						if (_gathers)
							code.Line($"var rr{back} = ways.RefsCount;");

						if (_logs)
							code.Line($"var lm{back} = ways.LogCount;");

						code.Line();
					}

					code.Line($"{tried} = {part};");

					if (back >= 0 || undo.Length > 0)
					{
						code.Line();

						using (code.Block($"if ({tried} < 0)"))
						{
							if (_gathers && back >= 0)
								code.Line($"ways.RefsCount = rr{back};");

							if (_logs && back >= 0)
								LogBack(code, $"lm{back}");

							if (undo.Length > 0)
								code.Line(undo);
						}
					}
				}
			}

			using (code.Block($"if ({tried} < 0)"))
			{
				// The one written in place, and the only one that can still use the token
				// the dispatch read: what came before it was a method, which reads its own.
				Emit(code, alternatives[alternatives.Count - 1], loaded);
				code.Line($"{tried} = p;");
			}

			code.Line($"p = {tried};");

			opened?.Dispose();
		}

		/// <summary>Two blocks, closed in the order they were opened in reverse.</summary>
		sealed class Both(IDisposable outer, IDisposable inner) : IDisposable
		{
			public void Dispose()
			{
				inner.Dispose();
				outer.Dispose();
			}
		}

		/// <summary>
		/// The test that the token in hand can begin one of the alternatives, or null where
		/// the first sets cannot say.
		/// </summary>
		string? Door(IEnumerable<Node> alternatives)
		{
			FirstSets.First? whole = null;

			foreach (var alternative in alternatives)
			{
				if (machine.Decidable(alternative) is not { Ends: false } first)
					return null;

				whole = whole is null ? first : whole.Or(first);
			}

			if (whole is null)
				return null;

			_character = true;

			return machine.RangesTest(whole.Ranges, machine.Tabulate);
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
						LogBack(code, $"lm{segment}");
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

			// The rule's start, for the records a part writes and the text a guard reads; the
			// body's is its own `pos`.
			if (_positions || _guarded)
				text.Append(", ").Append(type.Length > 0 ? "int start" : _part ? "start" : "pos");

			// And the rule's log mark, for what a guard builds.
			if (_guarded)
				text.Append(", ").Append(type.Length > 0 ? "int lmark" : _part ? "lmark" : "lm");

			// The strength the rule was entered at, which every method of it reads.
			if (_climbs)
				text.Append(", ").Append(type).Append("power");

			// Where the rule gathers across turns, what a record collects is everything pushed
			// since the rule began — not since the part did — so the rule's mark is handed on.
			if (_gathers)
				text.Append(", ").Append(type.Length > 0 ? "int refs" : _part ? "refs" : "rb");

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
			switch (machine.MemberOfSlot(owner, slot)?.Shape)
			{
				case MemberShape.Text:
					yield return "a" + slot;
					yield return "b" + slot;
					break;

				case MemberShape.Pieces:
					yield return "a" + slot;
					break;

				case MemberShape.Records:
					break;

				default:
					yield return "r" + slot;
					break;
			}
		}

		/// <summary>
		/// Whether a position is kept in a local that another method could need.
		/// </summary>
		/// <remarks>
		/// What a repetition gathers is pushed onto the tape by the turn that captured it
		/// and collected from the tape by the record, and the tape is shared by every
		/// method of the rule: nothing about it crosses a method boundary in a local, so
		/// nothing about it is handed over.
		/// </remarks>
		bool Handed(int slot) =>
			machine.MemberOfSlot(owner, slot)?.Shape is MemberShape.Text or MemberShape.Record;

		/// <summary>What a part captures, and what the records inside it read.</summary>
		(HashSet<int> Captured, HashSet<int> Used) Reaches(Node part)
		{
			var captured = new HashSet<int>();
			var used     = new HashSet<int>();

			foreach (var one in NodeWalk.Descendants(part))
			{
				if (one is Node.Capture &&
					machine._captureSlots[one] - machine._captureOffsets[owner] is var slot && Handed(slot))
				{
					captured.Add(slot);
				}

				// Every slot of a member and not its first: the record takes whichever of a
				// member's slots was written, so it names all of them.
				if (one is Node.Construct && !machine.DirectForwards(owner, machine._constructs[one]))
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						if (Handed(member.Slots[0]))
							foreach (var named in member.Slots)
								used.Add(named);

				// A guard reads what the rule has captured so far.
				if (one is Node.Guard guard)
					foreach (var (_, slots) in machine.GuardMembers(owner, guard))
						foreach (var named in slots)
							if (Handed(named))
								used.Add(named);
			}

			return (captured, used);
		}

		/// <summary>What the records outside this part read.</summary>
		HashSet<int> Elsewhere(Node part)
		{
			var mine = NodeWalk.ByIdentity(NodeWalk.Descendants(part));
			var used = new HashSet<int>();

			// A rule with no construction writes the record of its captures where it ends,
			// which is outside every part of it.
			if (machine.RecordsAtEnd(owner))
				foreach (var member in machine.DirectMembers(owner, -1))
					if (Handed(member.Slots[0]))
						foreach (var slot in member.Slots)
							used.Add(slot);

			foreach (var one in NodeWalk.Descendants(_graph.Bodies[owner]))
				if (!mine.Contains(one) && one is Node.Construct &&
					!machine.DirectForwards(owner, machine._constructs[one]))
				{
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						if (Handed(member.Slots[0]))
							foreach (var slot in member.Slots)
								used.Add(slot);
				}
				else if (!mine.Contains(one) && one is Node.Guard guard)
				{
					foreach (var (_, slots) in machine.GuardMembers(owner, guard))
						foreach (var slot in slots)
							if (Handed(slot))
								used.Add(slot);
				}

			return used;
		}

		void EmitRepeat(Writer code, Node.Repeat repeat)
		{
			var (body, min, max) = repeat;

			if (_tape)
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

				// The turn is not tried where the token in hand cannot begin it. What ends
				// a repetition is not a failure, and trying the turn made it one: a
				// refusal recorded at the token, and — nine levels of a ladder each asking
				// for their operator at the same token — a tie between them, which
				// allocates. This is the door the rendering beside this one has always had.
				if (Door([body]) is { } door)
				{
					code.Line("if ((uint)p >= (uint)text.Length)");
					code.Then("break;");
					code.Line();
					code.Line("c = text[p];");
					code.Line();
					code.Line($"if (!({door}))");
					code.Then("break;");
					code.Line();
				}

				var turn = $"q{_calls++}";
				var back = _gathers || _logs ? _ways++ : -1;
				var (call, undo) = Called(body);

				if (back >= 0)
				{
					if (_gathers)
						code.Line($"var rr{back} = ways.RefsCount;");

					if (_logs)
						code.Line($"var lm{back} = ways.LogCount;");

					code.Line();
				}

				code.Line($"var {turn} = {call};");
				code.Line();

				using (code.Block($"if ({turn} < 0 || {turn} == p)"))
				{
					if (_gathers && back >= 0)
						code.Line($"ways.RefsCount = rr{back};");

					if (_logs && back >= 0)
						LogBack(code, $"lm{back}");

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
				var test = CSharpEmitter.Test(element, machine.Tabulate);

				if (!string.Equals(test, "true", StringComparison.Ordinal))
				{
					code.Line($"if (!({test}))");
					code.Then("break;");
					code.Line();
				}

				code.Line("p++;");
			}

			code.Line();

			if (min > 0)
			{
				using (code.Block($"if (p < {mark} + {min})"))
					Refused(code, name);

				code.Line();
			}

			// A run of a fixed count stops where it was told to, and has nothing to give.
			if (max == min)
				return;

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

				// The turn is not tried where the token in hand cannot begin it — the same
				// door the reading over kinds has, and for the same two reasons: a turn that
				// cannot begin is the loop ending and not a failure to record, and a turn not
				// tried is a call not made.
				if (Door([body]) is { } door)
				{
					var open = $"o{_ways++}";

					code.Line($"var {open} = (uint)p < (uint)text.Length;");
					code.Line();

					using (code.Block($"if ({open})"))
					{
						code.Line("c = text[p];");
						code.Line($"{open} = {door};");
					}

					code.Line();

					using (code.Block($"if (!{open})"))
					{
						// A door that does not open below the minimum is not the loop ending
						// but the rule failing, and says what it wanted.
						if (min > 0)
						{
							using (code.Block($"if ({turn} < {min})"))
								Refused(code, machine.DeclareExpected([body.ToString()]));

							code.Line();
						}

						code.Line("break;");
					}

					code.Line();
				}

				// A count with no range in it has no turn to give back: it stops where it
				// was told to and nowhere else, so there is no way to record.
				var records = max != min;

				if (records)
				{
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
				}

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
					LogBack(code, $"lm{segment}");
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
						if (records)
							code.Line($"ways.Next(w{way}, 1);");
					}
					else
					{
						// The turn is spent, so the way that offered it now says "stopped
						// here" — and a turn short of the minimum is not a stop but a
						// refusal, which the body has already said everything about.
						if (records)
						{
							code.Line($"if ({stops})");
							code.Then($"ways.Next(w{way}, 1);");
							code.Line();
						}

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

		/// <summary>
		/// A group in braces: its first reading is its only one.
		/// </summary>
		/// <remarks>
		/// Over kinds every reading is already the only one, and the braces say nothing the
		/// rendering does not. Over characters the group is asked for a reading until it has
		/// one, and then what it decided is sealed: nothing after it may come back into it.
		/// </remarks>
		void EmitAtomic(Writer code, Node kept, bool loaded)
		{
			if (!_tape)
			{
				Emit(code, kept, loaded);

				return;
			}

			var segment      = _ways++;
			var took         = $"q{_calls++}";
			var (call, undo) = Called(kept);

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
				LogBack(code, $"lm{segment}");
				code.Line($"ways.RefsCount = rr{segment};");

				if (undo.Length > 0)
					code.Line(undo);

				code.Line();
				code.Line($"if (ways.Cursor > s{segment} && ways.Retry(s{segment}))");
				code.Then("continue;");
				code.Line();
				code.Line("break;");
			}

			code.Line();
			code.Line($"if ({took} < 0)");
			code.Then("return -1;");
			code.Line();
			code.Line($"ways.Seal(s{segment});");
			code.Line($"p = {took};");
		}

		/// <summary>
		/// A <c>when</c>, run where it stands with what the rule has captured so far (§7.7).
		/// </summary>
		/// <remarks>
		/// A text capture is cut from the locals that hold it; a captured rule's value is
		/// built now, from the records already in the log, and stays built — the walk at
		/// the end skips what a guard built, so no factory runs twice. The predicate itself
		/// is a method of its own under a <c>#line</c> pointing at the grammar, handed the
		/// captures by name. A refused guard is a failure with nothing expected.
		/// </remarks>
		void EmitGuard(Writer code, Node.Guard guard)
		{
			var rule       = machine._owners[guard];
			var method     = $"Recognize_DotGram{machine._tag}_Guard" + machine._guards++;
			var helper     = new Writer(0);
			var parameters = new List<string>();
			var arguments  = new List<string>();
			var text       = guard.Text;
			var begun      = _part ? "start" : "pos";
			var mark       = _part ? "lmark" : "lm";

			if (CSharpEmitter.Uses(_graph, text, "parserText"))
			{
				parameters.Add("string parserText");
				arguments.Add(machine.Cut(begun, $"p - {begun}"));
			}

			if (CSharpEmitter.Uses(_graph, text, "parserSpan"))
			{
				parameters.Add("SourceSpan parserSpan");
				arguments.Add(machine.Span(begun, $"p - {begun}"));
			}

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
					: $"{machine.DirectMaterializer}(ways, text, values, {{0}}, {mark}" +
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

				// Gathered turn by turn on the tape, and collected here the way the rule's end
				// would collect them.
				var bits    = 0L;
				var bracket = type.IndexOf('[');

				foreach (var slot in slots)
					bits |= 1L << slot;

				code.Line($"var {handed}Count = 0;");
				code.Line($"for (var at = {Refs}; at < ways.RefsCount; at += 3)");
				code.Then($"if (({bits}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
				code.Line(
					$"var {handed} = new {(bracket < 0 ? type : type.Substring(0, bracket))}[{handed}Count]" +
					$"{(bracket < 0 ? "" : type.Substring(bracket))};");
				code.Line($"{handed}Count = 0;");

				using (code.Block($"for (var at = {Refs}; at < ways.RefsCount; at += 3)"))
				{
					code.Line($"if (({bits}L & (1L << ways.Refs[at])) == 0) continue;");

					if (build.Length > 0)
						code.Line(string.Format(build, "ways.Refs[at + 1]"));

					code.Line($"{handed}[{handed}Count++] = {ValueAt(type, "ways.Refs[at + 1]")};");
				}
			}

			helper.Line($"static bool {method}({string.Join(", ", parameters)}) =>");
			CSharpEmitter.Handed(helper, machine._lines, guard.At, text + ";");
			machine._extra.Add(helper.ToString());

			using (code.Block($"if (!{method}({string.Join(", ", arguments)}))"))
			{
				code.Line($"{Refusing}(ref failure, p, null, ways);");
				code.Line("return -1;");
			}
		}

		int _guardLocals;

		/// <summary>A record's value as a guard sees it: from the tables, or for an extent the record itself.</summary>
		string ValueAt(string type, string record) =>
			type == "SourceSpan"
				? machine.RecordValue(type, record).Replace("log[", "ways.Log[")
				: $"values.V{machine.TableFor(type)}[{record}].Value";

		void EmitLookahead(Writer code, bool positive, Node inside)
		{
			var seen = $"q{_calls++}";

			if (!_tape)
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

		/// <summary>
		/// The log put back to a count — and with it the watermark of what a guard built,
		/// where anything builds: a record above the watermark is one written since, and
		/// a value a guard built in a derivation that was then abandoned is not the value
		/// of the record the next derivation writes at the same place.
		/// </summary>
		void LogBack(Writer code, string count)
		{
			code.Line($"ways.LogCount  = {count};");

			if (machine._directBuilds)
				code.Line($"if (ways.Built > {count}) ways.Built = {count};");
		}

		/// <summary>A refusal recorded and not acted on: what was wanted here, for the message.</summary>
		string Noted(string expected)
		{
			machine._expectedUsed.Add(expected);

			return $"{Refusing}(ref failure, p, {expected}, ways);";
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
