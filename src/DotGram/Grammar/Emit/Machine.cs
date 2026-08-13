using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// One rule compiled into a state machine that backtracks.
/// </summary>
/// <remarks>
/// <para>
/// The shape a recognizer had before this could answer one question — "does it match
/// here, and how far" — and having answered, could not be asked again. That is enough
/// for ordered choice on its own and not enough for anything after it: <c>'a'? &amp;
/// 'a'</c> would take the <c>a</c> greedily, fail on the second operand, and have no
/// way back. §11 of the language says ordered choice backtracks fully, and rests the
/// absence of a commit point on it.
/// </para>
/// <para>
/// So: states rather than nested calls, and an explicit stack of the points that could
/// have gone another way. Trying an alternative pushes the next alternative; consuming
/// one more repetition pushes the option of having stopped. Failure pops the most
/// recent of them and resumes there. Nothing is discarded until the stack is empty,
/// which is what "backtracks fully" means.
/// </para>
/// <para>
/// Transitions between states are <c>goto case</c>, which the compiler turns into a
/// direct branch — the jump table is entered exactly once, on resuming from the stack,
/// where the target genuinely is not known until run time.
/// </para>
/// <para>
/// Rule boundaries are still ordinary calls, so backtracking does not cross them. That
/// is a language question rather than an oversight, and it is written down in
/// docs/status.md rather than hidden here.
/// </para>
/// </remarks>
sealed class Machine
{
	/// <summary>The match is done; the position reached is the answer.</summary>
	public const int Accept = 0;

	/// <summary>
	/// Resume from the most recent point that could have gone another way, or — with
	/// none left — report no match.
	/// </summary>
	public const int Fail = 1;

	readonly List<Writer> _states = [];
	readonly List<string> _extra  = [];

	int _counters;
	int _lookaheads;

	/// <param name="results">What every rule's value is called; nothing may be null.</param>
	/// <param name="builds">What this machine builds, or null when it only recognizes.</param>
	public Machine(string name, ResultTypes results, Built? builds = null)
	{
		Name     = name;
		_results = results;
		_builds  = builds;

		// Settled before anything is compiled: every frame pushed carries one length per
		// sequence, so how wide a frame is has to be known before the first push.
		foreach (var slot in Layout.Sequences)
			_sequences.Add("l" + slot.Index);
	}

	/// <summary>
	/// Whether this is the machine of a lookahead rather than of a rule.
	/// </summary>
	/// <remarks>
	/// A lookahead asks a question about the input and consumes nothing, so how far it
	/// got before answering "no" is not how far the parse got — carrying its failures
	/// out would name a position the match never really needed. It is therefore the one
	/// machine that does not take the state.
	/// </remarks>
	public bool IsLookahead { get; private set; }

	readonly ResultTypes  _results;
	readonly Built?       _builds;
	readonly List<string> _sequences = [];

	/// <summary>The value a machine constructs, and where the parts of it are kept.</summary>
	public sealed record Built(
		string TypeName, IReadOnlyList<ResultMember> Members, CaptureLayout Layout);

	CaptureLayout Layout => _builds?.Layout ?? CaptureLayout.None;

	public string Name { get; }

	/// <summary>Methods this machine needed alongside itself — one per lookahead.</summary>
	public IReadOnlyList<string> Extra => _extra;

	public bool UsesStack  { get; private set; }
	public bool UsesResult { get; private set; }
	public bool UsesChar   { get; private set; }

	// ── Building ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Takes the next state number and a writer for its body, before that body is
	/// written — so a state may refer to states compiled after it.
	/// </summary>
	int Reserve(out Writer writer) => Reserve(out writer, null);

	/// <param name="of">
	/// What this state is for, written above it as the notation it came from.
	/// </param>
	/// <remarks>
	/// Always, not under a switch. A comment costs nothing at run time, so there is
	/// nothing to turn off — while generating different code in different
	/// configurations would mean a consumer's Release build differing from the Debug
	/// one they read, and a snapshot that depends on how it was built.
	/// </remarks>
	int Reserve(out Writer writer, Node? of, string? note = null)
	{
		writer = new Writer(0);

		if (of is not null || note is not null)
			writer.Line("// " + Comment(of, note));

		_states.Add(writer);

		return _states.Count - 1 + FirstState;
	}

	/// <summary>The notation a node came from, on one line and not too much of it.</summary>
	/// <remarks>
	/// On one line means every character C# ends a line at, not only the two obvious
	/// ones: U+2028 and U+2029 terminate a line in C# source as surely as U+000A, so a
	/// grammar matching one would otherwise cut its own comment in half and take the
	/// code after it along.
	/// </remarks>
	static string Comment(Node? node, string? note)
	{
		var one = Comment(node?.ToString() ?? "");

		if (one.Length > 64)
			one = one.Substring(0, 61) + "...";

		return note is null ? one : one.Length == 0 ? note : $"{one} — {note}";
	}

	/// <summary>Everything C# would read as the end of the line, spelled out instead.</summary>
	static string Comment(string source)
	{
		var text = new System.Text.StringBuilder(source.Length);

		foreach (var c in source)
			if (c is '\r' or '\n' or '\t' or '\u0085' or '\u2028' or '\u2029' || char.IsControl(c))
				text.Append("\\u").Append(((int)c).ToString("X4"));
			else
				text.Append(c);

		return text.ToString();
	}

	/// <summary>Accept and Fail take the first two numbers and are written by hand.</summary>
	const int FirstState = 2;

	/// <summary>
	/// A pattern broken across comment lines, at spaces, never mid-token.
	/// </summary>
	/// <remarks>
	/// Nothing is dropped here, unlike the note above a single state: the header is the
	/// one place the whole rule is readable, and a rule long enough to wrap is exactly
	/// the one worth reading.
	/// </remarks>
	static IEnumerable<string> Wrap(string pattern, int width = 92)
	{
		var line = new System.Text.StringBuilder();

		foreach (var word in Comment(pattern).Split(' '))
		{
			if (line.Length > 0 && line.Length + 1 + word.Length > width)
			{
				yield return line.ToString();

				line.Clear().Append("    ");            // a continuation, visibly so
			}
			else if (line.Length > 0)
			{
				line.Append(' ');
			}

			line.Append(word);
		}

		if (line.Length > 0)
			yield return line.ToString();
	}

	string NewCounter() => "c" + _counters++;

	/// <summary>
	/// How wide one backtracking frame is: where to resume, where the input was, one
	/// saved value — a repetition's count — and the length of every sequence being
	/// collected.
	/// </summary>
	/// <remarks>
	/// A sequence cannot be forgotten by assigning a constant the way a single value can:
	/// what an abandoned attempt appended has to be taken off, and how much it appended is
	/// only known at run time. The length at the moment of the push is that number, and
	/// the frame is where it belongs — it is already the record of "what was true here".
	/// It is also why this works for a repetition inside a repetition: giving back an
	/// outer iteration truncates to what the inner ones had collected before it began.
	/// </remarks>
	int Frame => 3 + _sequences.Count;

	/// <summary>Records a point the match could return to.</summary>
	void Push(Writer writer, int state, string saved)
	{
		UsesStack = true;

		writer.Line($"if (sp + {Frame} > bt.Length) bt = Grow(bt);");

		var frame = new System.Text.StringBuilder(
			$"bt[sp] = {state}; bt[sp + 1] = p; bt[sp + 2] = {saved};");

		for (var i = 0; i < _sequences.Count; i++)
			frame.Append($" bt[sp + {i + 3}] = {_sequences[i]}.Count;");

		writer.Line(frame.Append($" sp += {Frame};").ToString());
	}

	/// <summary>
	/// The state to resume at instead of <paramref name="target"/>, having first forgotten
	/// everything the attempt that is being abandoned could have captured.
	/// </summary>
	/// <remarks>
	/// Slots are numbered in the order the notation writes them, so "everything written
	/// since <paramref name="from"/> began" is a suffix of them, and which suffix is known
	/// while generating. That is the whole of the bookkeeping: no journal, no marks, and
	/// nothing at all on the path that does not backtrack.
	/// <para>
	/// A state of its own rather than the first lines of the target, because a target is
	/// not always reached by resuming — a repetition falls through to its exit when the
	/// upper bound is met, and an empty alternative's entry is the continuation itself.
	/// </para>
	/// </remarks>
	int Forget(int target, int first)
	{
		// A sequence is not among them: its slot is restored from the frame, because how
		// much to take off is not a constant.
		var forgotten = 0;

		for (var i = first; i < Layout.Slots.Count; i++)
			if (!Layout.Slots[i].IsSequence)
				forgotten++;

		if (forgotten == 0)
			return target;

		var state = Reserve(out var writer, null, "forget what the abandoned attempt captured");

		for (var i = first; i < Layout.Slots.Count; i++)
			if (!Layout.Slots[i].IsSequence)
				writer.Line(Layout.Slots[i].Rule is null
					? $"s{i}_from = s{i}_to = -1;"
					: $"v{i} = null;");

		writer.Line($"goto case {target};");

		return state;
	}

	// ── Compilation ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Compiles <paramref name="node"/> so that matching it continues at
	/// <paramref name="next"/>, and returns the state to enter it by.
	/// </summary>
	public int Compile(Node node, int next)
	{
		switch (node)
		{
			// Nothing to match and nothing to check: the continuation is the whole of it,
			// so no state is spent. A guard tests a value, and values do not exist yet.
			case Node.Empty:
			case Node.Guard:
				return next;

			case Node.Literal(var value) when value.Length == 0:
				return next;

			case Node.Literal(var value):
			{
				var state = Reserve(out var writer, node);

				writer.Line($"if (p + {value.Length} > text.Length)");
				writer.Then($"goto case {Fail};");

				for (var i = 0; i < value.Length; i++)
				{
					writer.Line($"if (text[p + {i}] != {CSharpEmitter.Char(value[i])})");
					writer.Then($"goto case {Fail};");
				}

				writer.Line($"p += {value.Length};");
				writer.Line($"goto case {next};");

				return state;
			}

			case Node.Element element:
			{
				var state = Reserve(out var writer, node);
				var test  = CSharpEmitter.Test(element);

				if (test == "false")
				{
					writer.Line($"goto case {Fail};");

					return state;
				}

				writer.Line("if (p >= text.Length)");
				writer.Then($"goto case {Fail};");

				if (test != "true")
				{
					// One shared local rather than one per state: sections of a switch
					// share a scope, so a declaration in each would collide.
					UsesChar = true;

					writer.Line();
					writer.Line("c = text[p];");
					writer.Line();
					writer.Line($"if (!{test})");
					writer.Then($"goto case {Fail};");
					writer.Line();
				}

				writer.Line("p++;");
				writer.Line($"goto case {next};");

				return state;
			}

			// Threaded back to front: each operand continues into the one after it, and
			// the last into whatever follows the sequence.
			case Node.Sequence(var nodes):
			{
				var target = next;

				for (var i = nodes.Count - 1; i >= 0; i--)
					target = Compile(nodes[i], target);

				return target;
			}

			// Every alternative starts where the choice started. All but the last are
			// entered with the following one recorded, so failing anywhere after this
			// point comes back and tries it.
			case Node.Choice(var alternatives):
			{
				var attempt = Compile(alternatives[alternatives.Count - 1], next);

				for (var i = alternatives.Count - 2; i >= 0; i--)
				{
					var entry = Compile(alternatives[i], next);
					var state = Reserve(out var writer, alternatives[i], "try this one, or the next");

					Push(writer, Forget(attempt, Layout.Before(node)), "0");
					writer.Line($"goto case {entry};");

					attempt = state;
				}

				return attempt;
			}

			case Node.Repeat repeat:
				return CompileRepeat(repeat, next);

			case Node.Lookahead(var isPositive, var body):
			{
				var method = CompileLookahead(body);
				var state  = Reserve(out var writer, node);

				// Consumes nothing either way: a lookahead asks a question about the
				// input. Its own choices cannot matter outside it — the answer is yes or
				// no — so it is a call, and nothing of it reaches this stack.
				writer.Line($"if ({method}(text, p) {(isPositive ? ">=" : "<")} 0)");
				writer.Then($"goto case {next};");
				writer.Line($"goto case {Fail};");

				return state;
			}

			case Node.Call call:
				return CompileCall(call, next, into: -1);

			// A capture of a rule that builds a value keeps the value; anything else is
			// text, and what is kept is the extent it covered.
			case Node.Capture(_, var captured) when _builds is null:
				return Compile(captured, next);

			case Node.Capture(_, var captured):
			{
				var slot = Layout.SlotOf(node);

				// A sequence appends where a single value would have been assigned, and the
				// append is on the successful path only — the call writes its `out` whether
				// it matched or not, so an iteration that failed contributes nothing.
				if (Layout.Slots[slot].IsSequence)
				{
					var appended = Reserve(out var atAppend, node, "one more, collected");

					atAppend.Line($"l{slot}.Add(v{slot}!);");
					atAppend.Line($"goto case {next};");

					return CompileCall((Node.Call)captured, appended, slot);
				}

				if (Layout.Slots[slot].Rule is not null)
					return CompileCall((Node.Call)captured, next, slot);

				var close = Reserve(out var atClose, node, "captured to here");

				atClose.Line($"s{slot}_to = p;");
				atClose.Line($"goto case {next};");

				var inner = Compile(captured, close);
				var open  = Reserve(out var atOpen, node, "capture starts here");

				atOpen.Line($"s{slot}_from = p;");
				atOpen.Line($"goto case {inner};");

				return open;
			}

			case Node.Construct(var built, _):
				return Compile(built, next);

			default:
			{
				var state = Reserve(out var writer);

				writer.Line($"goto case {Fail};");

				return state;
			}
		}
	}

	/// <summary>
	/// A call to another rule. Still a call, and still the boundary backtracking does not
	/// cross (docs/status.md) — what is new is that a rule which builds a value hands it
	/// back, into <paramref name="into"/> when a capture asked for it and nowhere
	/// otherwise.
	/// </summary>
	int CompileCall(Node.Call call, int next, int into)
	{
		var state = Reserve(out var writer, call);
		var value = _results.QualifiedOf(call.Rule);

		UsesResult = true;

		// The state is threaded through rather than returned: a callee's failure is the
		// caller's too, and what goes in it — the expected set, an outcome that tells a
		// broken record from no record — arrives later without changing this line again.
		writer.Line(value is null
			? $"r = {CSharpEmitter.MethodOf(call.Rule)}(text, p, ref failure);"
			: $"r = {CSharpEmitter.MethodOf(call.Rule)}(text, p, ref failure, out {(into < 0 ? $"{value} _" : $"v{into}")});");

		writer.Line();
		writer.Line("if (r < 0)");
		writer.Then($"goto case {Fail};");
		writer.Line();
		writer.Line("p = r;");
		writer.Line($"goto case {next};");

		return state;
	}

	/// <summary>
	/// Greedy repetition: take one more when one more can be taken, having first
	/// recorded that stopping here was allowed.
	/// </summary>
	/// <remarks>
	/// The recorded point carries the count as well as the position, because the state
	/// it resumes at has to know how many iterations had actually happened — that is
	/// what decides whether the lower bound was met.
	/// <para>
	/// There is no run-time guard against a body that consumes nothing. A repetition of
	/// a nullable body is refused as GRAM4001 before anything is emitted, and a guard
	/// here would be a second, weaker statement of the same rule in a place where it
	/// costs a comparison per iteration.
	/// </para>
	/// </remarks>
	int CompileRepeat(Node.Repeat repeat, int next)
	{
		var (body, min, max) = repeat;

		var counter = NewCounter();

		var exit  = Reserve(out var atExit,  repeat, "stop, and check the count");
		var loop  = Reserve(out var atLoop,  repeat, "take another, or leave stopping open");
		var after = Reserve(out var atAfter, repeat, "one more taken");
		var entry = Reserve(out var atEntry, repeat, "start counting");

		// A capture that is the whole of what repeats spans the run rather than the last
		// iteration of it (§7.3): opened once, before counting, and closed again by every
		// iteration that succeeds. An iteration that fails never reaches the close, so the
		// extent at the exit is the one the successful iterations left — which is why the
		// exit forgets only what was captured after the repetition, and not the run.
		var run = _builds is not null && max != 1 && body is Node.Capture &&
			Layout.Slots[Layout.SlotOf(body)].Rule is null
				? Layout.SlotOf(body)
				: -1;

		var start = run < 0 ? Compile(body, after) : CompileRun((Node.Capture)body, run, after);

		atEntry.Line($"{counter} = 0;");

		if (run >= 0)
			atEntry.Line($"s{run}_from = s{run}_to = p;");

		atEntry.Line($"goto case {loop};");

		if (max is { } limit)
		{
			// `saved` set on the way out, because the state being jumped to reads the
			// count from there — it cannot tell arriving from resuming.
			using (atLoop.Block($"if ({counter} >= {limit})"))
			{
				atLoop.Line($"saved = {counter};");
				atLoop.Line($"goto case {exit};");
			}

			atLoop.Line();
		}

		// An option either happened or did not, so returning to its exit means it did not,
		// and what it captured goes with it. A run returns to its exit having done one
		// iteration fewer, and what the earlier ones captured stands.
		Push(atLoop, Forget(exit, max == 1 ? Layout.Before(repeat) : Layout.After(repeat)), counter);
		atLoop.Line($"goto case {start};");

		atAfter.Line($"{counter}++;");
		atAfter.Line($"goto case {loop};");

		// Reached two ways: straight through, with the count in hand, or by resuming,
		// with it on the stack. Taking it from `saved` covers both, because falling
		// through leaves `saved` holding the count already.
		atExit.Line($"{counter} = saved;");

		if (min > 0)
		{
			atExit.Line();
			atExit.Line($"if ({counter} < {min})");
			atExit.Then($"goto case {Fail};");
		}

		atExit.Line();
		atExit.Line($"goto case {next};");

		return entry;
	}

	/// <summary>One iteration of a captured run: match it, and move the end of the run.</summary>
	int CompileRun(Node.Capture capture, int slot, int next)
	{
		var close = Reserve(out var atClose, capture, "one more iteration is part of the run");

		atClose.Line($"s{slot}_to = p;");
		atClose.Line($"goto case {next};");

		return Compile(capture.Body, close);
	}

	string CompileLookahead(Node body)
	{
		var machine = new Machine($"{Name}_Look{_lookaheads++}", _results) { IsLookahead = true };
		var entry   = machine.Compile(body, Accept);

		foreach (var extra in machine.Extra)
			_extra.Add(extra);

		_extra.Add(machine.Render(entry, $"the lookahead {body}"));

		return machine.Name;
	}

	// ── Rendering ────────────────────────────────────────────────────────────────

	/// <summary>
	/// The value, built once, where the match is known to have succeeded.
	/// </summary>
	/// <remarks>
	/// One expression and one place. Everything before this point only recorded where
	/// things were, so an attempt that is abandoned costs nothing to undo — and the C# a
	/// grammar supplies runs once, on the parse that actually happened, rather than once
	/// per attempt (§7.2).
	/// </remarks>
	static void Construct(Writer file, Built built)
	{
		file.Line($"value = new {built.TypeName}(");

		using (file.Indent())
			for (var i = 0; i < built.Members.Count; i++)
				file.Line(Value(built.Members[i]) + (i < built.Members.Count - 1 ? "," : ");"));
	}

	/// <summary>
	/// One member: the slot that was written, or null when none of them was.
	/// </summary>
	/// <remarks>
	/// More than one slot when the same name is captured in more than one alternative.
	/// They are tried in the order the notation writes them, which is the order in which
	/// at most one of them can have been reached.
	/// </remarks>
	static string Value(ResultMember member)
	{
		// A sequence is never absent and never shared between names: no iterations is an
		// empty array, so there is nothing to test and nothing to fall through to.
		if (member.IsSequence)
			return $"l{member.Slots[0]}.ToArray()";

		if (member.Slots.Count == 1 && !member.IsOptional)
			return Read(member, member.Slots[0]) + (member.Rule is null ? "" : "!");

		var expression = "null";

		for (var i = member.Slots.Count - 1; i >= 0; i--)
			expression = $"{Written(member, member.Slots[i])} ? {Read(member, member.Slots[i])} : {expression}";

		// The member is written on every path, so one of the tests holds — which the
		// compiler has no way of knowing.
		return member.IsOptional ? expression : $"({expression})!";
	}

	static string Read(ResultMember member, int slot) =>
		member.Rule is null
			? $"text.Slice(s{slot}_from, s{slot}_to - s{slot}_from).ToString()"
			: $"v{slot}";

	static string Written(ResultMember member, int slot) =>
		member.Rule is null ? $"s{slot}_from >= 0" : $"v{slot} != null";

	/// <summary>The whole machine as one method.</summary>
	/// <param name="pattern">
	/// What it recognizes, in notation, written above it. The states below carry the
	/// fragment each one is; this is the only place the whole of it is legible, and
	/// after normalization — so it is what the method does rather than what was typed.
	/// </param>
	public string Render(int entry, string? pattern = null)
	{
		var file = new Writer(0);

		if (pattern is not null)
			foreach (var line in Wrap(pattern))
				file.Line("// " + line);

		var built = _builds is null ? "" : $", out {_builds.TypeName} value";
		var failure = IsLookahead ? "" : $", ref {CSharpEmitter.FailureType} failure";

		using (file.Block(
			$"static int {Name}(global::System.ReadOnlySpan<char> text, int pos{failure}{built})"))
		{
			if (_builds is not null)
			{
				// Assigned before anything can fail, so every way out of the method has it
				// assigned — including the ones that report no match at all.
				file.Line("value = null!;");
				file.Line();
			}

			if (UsesStack)
			{
				// Enough for the great majority of matches; Grow takes over when it is
				// not, so nothing is allocated in the common case and nothing overflows
				// in the uncommon one.
				file.Line("global::System.Span<int> bt = stackalloc int[48];");
				file.Line();
				file.Line("var sp    = 0;");
				file.Line("var saved = 0;");
			}

			file.Line("var p     = pos;");

			if (UsesResult)
				file.Line("var r     = 0;");

			if (UsesChar)
				file.Line("var c     = '\\0';");

			for (var i = 0; i < _counters; i++)
				file.Line($"var c{i}    = 0;");

			file.Line($"var state = {entry};");

			// One pair of positions per text capture, one reference per captured value.
			// Unwritten is -1 and null, which is what tells "matched nothing here" from
			// "was never reached" — an optional capture is the difference.
			if (Layout.Slots.Count > 0)
			{
				file.Line();

				foreach (var slot in Layout.Slots)
					file.Line(slot switch
					{
						{ Rule: null } => $"var s{slot.Index}_from = -1; var s{slot.Index}_to = -1;",

						// The one the call writes into, and the one it is appended to. Two,
						// because an `out` needs somewhere to land whether or not it matched.
						{ IsSequence: true } =>
							$"{_results.QualifiedOf(slot.Rule)}? v{slot.Index} = null; " +
							$"var l{slot.Index} = new global::System.Collections.Generic.List<" +
							$"{_results.QualifiedOf(slot.Rule)}>();",

						_ => $"{_results.QualifiedOf(slot.Rule)}? v{slot.Index} = null;",
					});
			}

			file.Line();

			// The loop exists only for the one `continue` that resumes from the stack.
			// Without a stack nothing leaves the switch except by returning, and wrapping
			// it would be scaffolding around nothing.
			using (UsesStack ? file.Block("while (true)") : null)
			using (file.Block("switch (state)"))
			{
				file.Line($"case {Accept}:");

				using (file.Indent())
				{
					if (_builds is not null)
					{
						Construct(file, _builds);
						file.Line();
					}

					file.Line("return p;");
				}

				file.Line();
				file.Line($"case {Fail}:");

				using (file.Indent())
				{
					if (!IsLookahead)
					{
						// The one place the machine gives up on where it is, and so the one
						// place worth asking how far it had got. `p` is about to be restored
						// to an earlier position, and only rises here, so the answer at the
						// end is the furthest the input was ever followed.
						file.Line("if (p > failure.Position)");
						file.Then("failure.Position = p;");
						file.Line();
					}

					if (UsesStack)
					{
						file.Line("if (sp == 0)");
						file.Then("return -1;");
						file.Line();
						file.Line($"sp    -= {Frame};");
						file.Line("state  = bt[sp];");
						file.Line("p      = bt[sp + 1];");
						file.Line("saved  = bt[sp + 2];");

						// What the abandoned attempt appended comes off. Only ever a
						// shortening, and only of what this frame did not have.
						for (var i = 0; i < _sequences.Count; i++)
						{
							var list = _sequences[i];

							file.Line();
							file.Line($"if ({list}.Count > bt[sp + {i + 3}])");
							file.Then($"{list}.RemoveRange(bt[sp + {i + 3}], {list}.Count - bt[sp + {i + 3}]);");
						}

						file.Line();
						file.Line("// The one transition whose target is not known until now,");
						file.Line("// and so the one that goes through the switch again.");
						file.Line("continue;");
					}
					else
					{
						file.Line("return -1;");
					}
				}

				for (var i = 0; i < _states.Count; i++)
				{
					file.Line();
					file.Line($"case {i + FirstState}:");
					file.AppendIndented(_states[i]);
				}

				file.Line();
				file.Line("default:");

				using (file.Indent())
					file.Line("return -1;");
			}
		}

		return file.ToString();
	}
}
