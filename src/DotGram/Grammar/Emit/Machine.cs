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
	/// <summary>
	/// Slots of backtracking stack a recognizer keeps on the C# stack before spilling.
	/// </summary>
	/// <remarks>
	/// Two costs pull opposite ways. Too few and <c>Grow</c> runs on ordinary matches,
	/// which allocates; too many and every nested rule call carries the unused remainder,
	/// which is what bounds how deep a grammar may nest before the process dies. Measured
	/// rather than guessed: see docs/status.md.
	/// </remarks>
	public const int Backtracking = 48;

	/// <summary>The match is done; the position reached is the answer.</summary>
	public const int Accept = 0;

	/// <summary>
	/// Resume from the most recent point that could have gone another way, or — with
	/// none left — report no match.
	/// </summary>
	public const int Fail = 1;

	readonly List<Writer> _states = [];
	readonly List<string> _extra  = [];
	readonly RuleSymbol?  _recursiveRule;

	int _counters;
	int _lookaheads;

	/// <param name="results">What every rule's value is called; nothing may be null.</param>
	/// <param name="builds">What this machine builds, or null when it only recognizes.</param>
	public Machine(
		string name, ResultTypes results, Built? builds = null, RuleSymbol? recursiveRule = null)
	{
		Name           = name;
		_results       = results;
		_builds        = builds;
		_recursiveRule = recursiveRule;

		// Settled before anything is compiled: every frame pushed carries one length per
		// sequence, so how wide a frame is has to be known before the first push.
		foreach (var slot in Layout.Sequences)
			_sequences.Add("l" + slot.Index);

		// Which fold step matched, in order. Numbers rather than values: a value built
		// while matching is a value built on a parse that may not happen, so the
		// factories run at the accepting state and read this (§4.3).
		if (Folding)
			_sequences.Add(Steps);
	}

	/// <summary>Which alternative built the value, and which fold steps followed it.</summary>
	const string Chosen = "built";
	const string Steps  = "steps";

	int IndexOf(Factory factory)
	{
		for (var i = 0; i < Factories.Count; i++)
			if (ReferenceEquals(Factories[i], factory))
				return i;

		return -1;
	}

	/// <summary>Whether any alternative of this rule is a fold step.</summary>
	bool Folding
	{
		get
		{
			foreach (var factory in Factories)
				if (factory.Accumulator is not null)
					return true;

			return false;
		}
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
	public bool IsLookahead { get; set; }

	readonly ResultTypes  _results;
	readonly Built?       _builds;
	readonly List<string> _sequences = [];

	/// <summary>The value a machine constructs, and where the parts of it are kept.</summary>
	/// <param name="Factories">
	/// One method per <c>=&gt;</c> the rule wrote, in the order the alternatives are
	/// written. Empty when the value is the generated type, which is built by calling its
	/// constructor with the same arguments.
	/// </param>
	public sealed record Built(
		string TypeName,
		IReadOnlyList<ResultMember> Members,
		CaptureLayout Layout,
		IReadOnlyList<Factory>? Factories = null);

	/// <summary>
	/// One <c>=&gt;</c>: the alternative it is on, the method it became, and the members
	/// that alternative can have captured — which are its parameters.
	/// </summary>
	/// <param name="Accumulator">
	/// The name a fold step's <c>=&gt;</c> knows the value built so far by, or null when
	/// this alternative is not a fold step (§4.3).
	/// </param>
	public sealed record Factory(
		Node Of,
		string Method,
		IReadOnlyList<ResultMember> Members,
		string? Accumulator = null);

	IReadOnlyList<Factory> Factories => _builds?.Factories ?? [];

	CaptureLayout Layout => _builds?.Layout ?? CaptureLayout.None;

	public string Name { get; }

	/// <summary>Methods this machine needed alongside itself — one per lookahead.</summary>
	public IReadOnlyList<string> Extra => _extra;

	public bool UsesStack  { get; private set; }
	public bool UsesResult { get; private set; }
	public bool UsesChar   { get; private set; }
	public bool UsesCallStack => _recursiveCalls.Count > 0;

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

		// Written at the end rather than here: which value slots carry a flag is only
		// known once every alternative has been compiled.
		_forgets.Add((writer, first, target));

		return state;
	}

	readonly List<(Writer Writer, int First, int Target)> _forgets = [];
	readonly List<(Writer Writer, int Slot, int Target)>  _marks   = [];

	/// <summary>How many slots belong to the alternatives that start a fold, if it folds.</summary>
	int BaseSlots
	{
		get
		{
			foreach (var factory in Factories)
				if (factory.Accumulator is not null)
					return Layout.Before(factory.Of);

			return Layout.Slots.Count;
		}
	}

	/// <summary>
	/// The states whose bodies wait on what the rest of the machine turned out to need.
	/// </summary>
	void WriteDeferred()
	{
		foreach (var (writer, slot, target) in _marks)
		{
			if (_flagged.Contains(slot))
				writer.Line($"v{slot}_set = true;");

			writer.Line($"goto case {target};");
		}

		foreach (var (writer, first, target) in _forgets)
		{
			// Which alternative built the value goes back with the slots, but only where
			// the alternative itself is being abandoned: a fold step given back does not
			// unmake the base that started the chain.
			if (Factories.Count > 1 && first <= BaseSlots)
				writer.Line($"{Chosen} = -1;");

			for (var i = first; i < Layout.Slots.Count; i++)
				if (!Layout.Slots[i].IsSequence)
					writer.Line(Layout.Slots[i].Rule is null
						? $"s{i}_from = s{i}_to = -1;"
						: _flagged.Contains(i) ? $"v{i}_set = false;" : $"v{i} = default!;");

			writer.Line($"goto case {target};");
		}
	}

	// ── Compilation ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Compiles <paramref name="node"/> so that matching it continues at
	/// <paramref name="next"/>, and returns the state to enter it by.
	/// </summary>
	public int Compile(Node node, int next)
	{
		// An alternative of a climbing rule's loop is guarded by its own strength before
		// anything of it is matched. Ahead of the dispatch because it applies whatever the
		// alternative turns out to be made of.
		if (_levels.TryGetValue(node, out var level))
			return CompileLevel(node, level, CompileUnguarded(node, next));

		return CompileUnguarded(node, next);
	}

	int CompileUnguarded(Node node, int next)
	{
		switch (node)
		{
			// Nothing to match and nothing to check: the continuation is the whole of it,
			// so no state is spent.
			case Node.Empty:
				return next;

			case Node.Guard(var condition):
				return CompileGuard(node, condition, next);

			case Node.Literal(var value) when value.Length == 0:
				return next;

			case Node.Literal(var value):
			{
				var state = Reserve(out var writer, node);

				// Running out of input is not the same as not matching, and only here is it
				// known which happened: `p` has not moved, so a caller reading through a
				// window would be told the element broke at `p` when it merely did not fit.
				writer.Line($"if (p + {value.Length} > text.Length)");

				if (Starves)
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						writer.Line($"goto case {Fail};");
					}
				else
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

				if (Starves)
					using (writer.Block(""))
					{
						writer.Line("failure.Starved = true;");
						writer.Line($"goto case {Fail};");
					}
				else
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

			// §7.1: a C# method that reads the input itself. It is handed the parser's own
			// position, because the `ref` in its signature is it saying that it moves one —
			// nothing here copies it away and nothing checks what came back. Where it says
			// no, that is an ordinary non-match and the stack has somewhere to resume.
			case Node.External(var method):
			{
				var state = Reserve(out var writer, node);

				writer.Line($"if (!{method}(text, ref p))");
				writer.Then($"goto case {Fail};");
				writer.Line($"goto case {next};");

				return state;
			}

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
				if (Layout.Slots[slot] is { IsSequence: true, Rule: not null })
				{
					var appended = Reserve(out var atAppend, node, "one more, collected");

					atAppend.Line($"l{slot}.Add(v{slot});");
					atAppend.Line($"goto case {next};");

					return CompileCall((Node.Call)captured, appended, slot);
				}

				if (Layout.Slots[slot].Rule is not null)
				{
					// Written afterwards, because whether this slot carries a flag at all
					// is only settled once every member has been read.
					var marked = Reserve(out var atMark, node, "captured");

					_marks.Add((atMark, slot, next));

					return CompileCall((Node.Call)captured, marked, slot);
				}

				// §3.4: a lookahead produces the value of what it saw, and consumes nothing.
				// Taking the extent from `p` would take nothing, because `p` is where it
				// started — so the answer comes from what the lookahead returned, which is
				// how far it got before giving the position back.
				if (captured is Node.Lookahead(var ahead, var seen))
				{
					var looked = CompileLookahead(seen);
					var asked  = Reserve(out var atAsk, node, "captured, and consumes nothing");

					atAsk.Line($"s{slot}_from = p;");
					atAsk.Line($"s{slot}_to   = {looked}(text, p);");
					atAsk.Line();
					atAsk.Line($"if (s{slot}_to {(ahead ? "<" : ">=")} 0)");
					atAsk.Then($"goto case {Fail};");
					atAsk.Line();

					// A negative lookahead saw nothing by definition — it succeeded because
					// what it looked for was not there.
					if (!ahead)
						atAsk.Line($"s{slot}_to = p;");

					atAsk.Line($"goto case {next};");

					return asked;
				}

				var close = Reserve(out var atClose, node, "captured to here");

				atClose.Line(Layout.Slots[slot].IsSequence
					? $"l{slot}.Add(text.Slice(s{slot}_from, p - s{slot}_from).ToString());"
					: $"s{slot}_to = p;");

				atClose.Line($"goto case {next};");

				var inner = Compile(captured, close);
				var open  = Reserve(out var atOpen, node, "capture starts here");

				atOpen.Line($"s{slot}_from = p;");
				atOpen.Line($"goto case {inner};");

				return open;
			}

			// Which `=>` fired is which alternative matched, and the value is built from it
			// once the whole match has succeeded. Recording the number rather than building
			// there keeps the promise of §7.2: the C# runs on the parse that happened.
			case Node.Construct(var pattern, _):
			{
				Factory? chosen = null;

				foreach (var factory in Factories)
					if (ReferenceEquals(factory.Of, node))
						chosen = factory;

				// The one factory of a rule builds at the accepting state, where nothing
				// can still be given back. More than one, and which fired has to be
				// recorded — as the value itself, appended to a list the backtracking
				// frame truncates, so an abandoned alternative takes its value with it.
				if (chosen is null || Factories.Count < 2)
					return Compile(pattern, next);

				var which = IndexOf(chosen);
				var state = Reserve(out var writer, node, "this alternative matched");

				// The number, not the value. Building here would build on attempts that
				// are given back; the accepting state builds from what is left of this.
				writer.Line(chosen.Accumulator is null
					? $"{Chosen} = {which};"
					: $"{Steps}.Add({which});");

				writer.Line($"goto case {next};");

				return Compile(pattern, state);
			}

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

		if (ReferenceEquals(call.Rule, _recursiveRule))
		{
			_recursiveCalls.Add((writer, next));

			return state;
		}

		var value = _results.QualifiedOf(call.Rule);

		UsesResult = true;

		// A rule of binding powers is asked at a strength (§4.3.1). What that strength is
		// was worked out where the alternative was rewritten; anywhere else — a call from
		// outside, a call this rule makes to a different one — it is 0, which admits
		// everything.
		var strength = Climbs(call.Rule)
			? ", " + (_powers.TryGetValue(call, out var power) ? power.ToString() : "0")
			: "";

		// The state is threaded through rather than returned: a callee's failure is the
		// caller's too, and what goes in it — the expected set, an outcome that tells a
		// broken record from no record — arrives later without changing this line again.
		writer.Line(value is null
			? $"r = {CSharpEmitter.MethodOf(call.Rule)}(text, p{strength}, ref failure);"
			: $"r = {CSharpEmitter.MethodOf(call.Rule)}(text, p{strength}, ref failure, out {(into < 0 ? $"{value} _" : $"v{into}")});");

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

		var recovers = _recovery is not null && ReferenceEquals(_recovering, repeat);

		atEntry.Line($"{counter} = 0;");

		if (run >= 0)
			atEntry.Line($"s{run}_from = s{run}_to = p;");

		if (recovers)
		{
			_marked = true;

			atEntry.Line($"{Mark} = sp;");
		}

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
		var giveUp = Forget(exit, max == 1 ? Layout.Before(repeat) : Layout.After(repeat));

		// A recovering repetition has one more way out of the loop, taken exactly where
		// the ordinary one would have ended it: an element that began and broke is an
		// error, and an element that never began is the end of the sequence (§8.2).
		if (recovers)
		{
			var broken = CompileRecovery(repeat, loop, counter);
			var asked  = Reserve(out var atAsked, repeat, "was there an element here, or not?");

			atAsked.Line("if (failure.Reach > p)");
			atAsked.Then($"goto case {broken};");
			atAsked.Line($"goto case {giveUp};");

			// How far the attempt starting here reached — read at `asked`, which is entered
			// only by that attempt failing, because the repetition gives nothing back.
			atLoop.Line("failure.Reach = p;");

			// Possessive: an element it took was either good or explicitly rejected, so
			// there is no shorter reading of it to come back for. Dropping what the
			// iterations recorded is what keeps `asked` answerable — with the frames left
			// in place, a failure after the repetition would resume inside it, at a
			// position whose element had matched, and be told an element broke there.
			atExit.Line($"sp = {Mark};");

			giveUp = asked;
		}

		Push(atLoop, giveUp, counter);
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

	// ── Binding powers (§4.3.1) ──────────────────────────────────────────────────

	/// <summary>The strength a climbing rule is being parsed at, in the generated code.</summary>
	const string Power = "power";

	IReadOnlyDictionary<RuleSymbol, IReadOnlyDictionary<Node, int>> _climbing =
		new Dictionary<RuleSymbol, IReadOnlyDictionary<Node, int>>();

	IReadOnlyDictionary<Node, int> _powers = new Dictionary<Node, int>();
	IReadOnlyDictionary<Node, int> _levels = new Dictionary<Node, int>();

	/// <summary>
	/// What this machine needs to know about binding powers, anywhere in the grammar.
	/// </summary>
	/// <param name="climbing">
	/// Every rule whose recognizer takes a strength — needed at every call site, not only
	/// in the rule that climbs.
	/// </param>
	/// <param name="powers">At what strength each self-call parses its operand.</param>
	/// <param name="levels">
	/// At what strength each alternative of this rule's loop may be entered, empty for a
	/// rule that does not climb.
	/// </param>
	public void Climbs(
		IReadOnlyDictionary<RuleSymbol, IReadOnlyDictionary<Node, int>> climbing,
		IReadOnlyDictionary<Node, int>                                  powers,
		IReadOnlyDictionary<Node, int>                                  levels)
	{
		_climbing = climbing;
		_powers   = powers;
		_levels   = levels;
	}

	bool Climbs(RuleSymbol rule) => _climbing.ContainsKey(rule);

	/// <summary>Whether this machine's own rule is one of them.</summary>
	public bool TakesPower { get; set; }

	/// <summary>
	/// The one test that turns a fold into precedence climbing: an alternative weaker than
	/// what the caller asked for is not this call's to take (§4.3.1).
	/// </summary>
	/// <remarks>
	/// Weaker and not weaker-or-equal, so that an operator can appear again at its own
	/// strength — which is what makes <c>&gt;&gt; n</c>, recording <c>n</c> rather than
	/// <c>n + 1</c>, group to the right.
	/// </remarks>
	int CompileLevel(Node alternative, int level, int next)
	{
		var state = Reserve(out var writer, alternative, $"only at strength {level} or weaker");

		writer.Line($"if ({level} < {Power})");
		writer.Then($"goto case {Fail};");
		writer.Line($"goto case {next};");

		return state;
	}

	/// <summary>Where the stack stood when the recovering repetition began.</summary>
	const string Mark = "spr";

	Node?     _recovering;
	Recovery? _recovery;
	string    _recoverMaker = "";
	int       _syncs;
	int       _recoverySlot = -1;
	bool      _marked;

	/// <summary>
	/// Whether this machine records how far an attempt reached.
	/// </summary>
	/// <remarks>
	/// Set on every machine of a grammar that recovers anywhere, and not only on the one
	/// that does the recovering: the question "did an element begin here, or was there
	/// none" is answered by how far the rule that failed had got, and that rule knows
	/// nothing about the repetition calling it.
	/// </remarks>
	public bool Reaches { get; set; }

	/// <summary>
	/// Where the grammar's own C# came from, so a guard is written under a <c>#line</c>
	/// pointing at where the author wrote it (§7.6). Null emits none.
	/// </summary>
	public ILineMap? LineMap { get; set; }

	/// <summary>
	/// Whether this machine says when it ran out of input rather than out of matches.
	/// </summary>
	/// <remarks>
	/// Only a windowed parse can tell the difference, and only it needs to. Over a string
	/// the end of the input is the end of the input, and a rule wanting one more character
	/// was simply wrong. Over a window it may be right and merely early — and the position
	/// it failed at is where the missing character would have gone, which is *before* the
	/// end of what is held. Nothing else distinguishes the two, which is how a feed of
	/// records whose lengths vary lost one every time an element straddled the buffer.
	/// </remarks>
	public bool Starves { get; set; }

	/// <summary>What the repetition of this rule was told to do about a bad element.</summary>
	public void Recovers(Node repetition, Recovery recovery, int into, string factory)
	{
		_recovering   = repetition;
		_recovery     = recovery;
		_recoverySlot = into;
		_recoverMaker = factory;
		Reaches       = true;
	}

	/// <summary>
	/// The way out of a repetition that an element took by beginning and breaking (§8.2).
	/// </summary>
	/// <remarks>
	/// It does not fail. The extent from where the element began to where the parse can
	/// pick up again is handed to the factory, the result takes its place in the
	/// sequence, and the loop goes round — which is the whole of "report it and carry
	/// on".
	/// </remarks>
	int CompileRecovery(Node.Repeat repeat, int loop, string counter)
	{
		var sync  = CompileSync(_recovery!.Sync);
		var state = Reserve(out var writer, repeat, "this element began and broke");

		UsesResult = true;

		writer.Line("var from = p;");
		writer.Line("var to   = p;");
		writer.Line("r        = -1;");
		writer.Line();

		Scan(writer, sync);

		writer.Line();

		if (_recovery.Factory is null)
		{
			// §8.3: no `=>`, so the element is dropped rather than collected, and what it
			// was goes to the hook instead. Everything here is an argument and nothing is a
			// statement, which is what lets the whole line — the substring and both scans
			// included — be removed when nobody implements it.
			writer.Line(
				$"{CSharpEmitter.RecoveredMethod}(\"{Element}\", {Supplied("parserText", counter)}, " +
				$"{Supplied("parserPosition", counter)}, {Supplied("parserLine", counter)}, " +
				$"{Supplied("parserColumn", counter)}, {counter}, {Supplied("parserMessage", counter)});");
		}
		else if (_recoverySlot >= 0)
		{
			var arguments = new List<string>();

			foreach (var name in _recovery.Asks)
				arguments.Add(Supplied(name, counter));

			writer.Line($"l{_recoverySlot}.Add({_recoverMaker}({string.Join(", ", arguments)}));");
		}

		writer.Line($"{counter}++;");
		writer.Line("p = r;");
		writer.Line($"goto case {loop};");

		return state;
	}

	/// <summary>Looks forward for the point the parse may pick up again at.</summary>
	static void Scan(Writer writer, string sync)
	{
		using (writer.Block("while (to <= text.Length)"))
		{
			writer.Line($"r = {sync}(text, to);");
			writer.Line();

			// A synchronization point that consumed nothing would leave the loop where it
			// started, so it is not one.
			writer.Line("if (r > to)");
			writer.Then("break;");
			writer.Line();
			writer.Line("r = -1;");
			writer.Line("to++;");
		}

		writer.Line();
		writer.Line("if (r < 0)");
		writer.Then("to = r = text.Length;");
	}

	/// <summary>
	/// What one of the names §8.2 supplies is, where a broken element is being made into
	/// one of the sequence.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>parserMessage</c> is the one that has to be built rather than read. What is known
	/// about a broken element is which rule it should have been and how far into it the
	/// input stopped being that — <c>failure.Reach</c>, which is what said the element began
	/// at all. The set of what was expected there would say more and is not carried yet.
	/// </para>
	/// </remarks>
	string Supplied(string name, string counter) => name switch
	{
		"parserText"     => "text.Slice(from, to - from).ToString()",
		"parserPosition" => "from",
		"parserOrdinal"  => counter,
		"parserLine"     => "LineAt(text, from)",
		"parserColumn"   => "ColumnAt(text, from)",
		"parserSpan"     => "new global::DotGram.SourceSpan(from, to - from)",
		"parserMessage"  => $"\"Input does not match '{Element}' at \" + {Number("failure.Reach")} + \".\"",
		_                => "default",
	};

	/// <summary>A number in a message, spelled the same wherever it is read.</summary>
	static string Number(string expression) =>
		$"{expression}.ToString(global::System.Globalization.CultureInfo.InvariantCulture)";

	/// <summary>
	/// What the broken element should have been, for the message and for the hook.
	/// </summary>
	/// <remarks>
	/// The rule the repetition collects, which is only known when the elements are captured
	/// — <c>rows: Row*</c> and not a bare <c>Row*</c>. Without a capture there is no slot to
	/// read it from and the element is described by the repetition instead.
	/// </remarks>
	string Element =>
		_recoverySlot >= 0 && Layout.Slots[_recoverySlot].Rule is { } rule ? rule.Name : "an element";

	/// <summary>Where the parse may pick up again — a machine of its own, like a lookahead.</summary>
	string CompileSync(Node sync)
	{
		var machine = new Machine($"{Name}_Sync{_syncs++}", _results)
		{
			IsLookahead = true,
			LineMap     = LineMap,
		};
		var entry   = machine.Compile(sync, Accept);

		foreach (var extra in machine.Extra)
			_extra.Add(extra);

		_extra.Add(machine.Render(entry, $"where {Name} picks up again: {sync}"));

		return machine.Name;
	}

	/// <summary>One iteration of a captured run: match it, and move the end of the run.</summary>
	int CompileRun(Node.Capture capture, int slot, int next)
	{
		var close = Reserve(out var atClose, capture, "one more iteration is part of the run");

		atClose.Line($"s{slot}_to = p;");
		atClose.Line($"goto case {next};");

		return Compile(capture.Body, close);
	}

	/// <summary>
	/// A <c>when</c> guard: a question asked of the values, answered in C#.
	/// </summary>
	/// <remarks>
	/// <para>
	/// It runs <b>during</b> the match, which is what makes it recognition — a guard that
	/// says no is a non-match, and a sibling alternative is tried (docs/syntax.md §8.1).
	/// So it may run more than once, and §7.2 requires the C# to bear that.
	/// </para>
	/// <para>
	/// What it may look at is what was captured <b>before</b> it. A capture further along
	/// has not been written yet, and passing it would mean handing over a slot that reads
	/// as a negative offset; leaving it out makes naming it an ordinary C# error about a
	/// name that is not there.
	/// </para>
	/// </remarks>
	int CompileGuard(Node guard, string condition, int next)
	{
		var before  = Layout.Before(guard);
		var visible = new List<ResultMember>();

		foreach (var member in _builds?.Members ?? [])
		{
			// Only the slots this guard could have seen. A name captured in more than one
			// alternative has one of its slots here and the rest elsewhere, and elsewhere
			// has not happened.
			var reachable = new List<int>();

			foreach (var slot in member.Slots)
				if (slot < before)
					reachable.Add(slot);

			if (reachable.Count == 0)
				continue;

			// Certain only when the member is written on every path *and* every path to it
			// is behind us. Anything less is a `?`, which is the truth at this point.
			visible.Add(member with
			{
				Slots      = reachable,
				IsOptional = member.IsOptional || reachable.Count != member.Slots.Count,
			});
		}

		var method     = $"{Name}_Guard{_guards++}";
		var parameters = new List<string> { "string parserText" };
		var arguments  = new List<string> { "text.Slice(pos, p - pos).ToString()" };

		foreach (var member in visible)
		{
			if (member.Name == "parserText")
				continue;

			parameters.Add(
				_results.ValueOf(member.Rule) +
				(member.IsSequence ? "[]" : member.IsOptional ? "?" : "") +
				" " + ResultTypes.ParameterOf(member));

			arguments.Add(Value(member));
		}

		var body = new Writer(0);

		body.Line($"// {Comment(guard, null)}");
		body.Line($"static bool {method}({string.Join(", ", parameters)}) =>");

		CSharpEmitter.Handed(
			body, LineMap, guard is Node.Guard { At: var at } ? at : -1, condition + ";");

		_extra.Add(body.ToString());

		var state = Reserve(out var writer, guard);

		writer.Line($"if (!{method}({string.Join(", ", arguments)}))");
		writer.Then($"goto case {Fail};");
		writer.Line($"goto case {next};");

		return state;
	}

	int _guards;

	string CompileLookahead(Node body)
	{
		var machine = new Machine($"{Name}_Look{_lookaheads++}", _results)
		{
			IsLookahead = true,
			LineMap     = LineMap,
		};
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
	/// <summary>
	/// A call to one alternative's factory, made where the match is known to have
	/// succeeded.
	/// </summary>
	/// <param name="into">What is being assigned — the value, or the fold's running one.</param>
	/// <param name="step">
	/// The iteration a fold step is being applied for, which is what indexes its
	/// captures: each of them collected one entry per iteration.
	/// </param>
	string Apply(Factory factory, string into, string? step)
	{
		// The matched extent, which §7.3 supplies under the name `text`. Always passed:
		// what the C# does with it is the C# compiler's business, and an argument nobody
		// reads costs nothing.
		var arguments = new List<string> { "text.Slice(pos, p - pos).ToString()" };

		// In the same order the parameters were written.
		if (CSharpEmitter.Asks(factory, "parserSpan"))
			arguments.Add("new global::DotGram.SourceSpan(pos, p - pos)");

		if (factory.Accumulator is not null)
			arguments.Add(into);

		foreach (var member in factory.Members)
			arguments.Add(step is null || !member.IsSequence
				? Value(member)
				: $"l{member.Slots[0]}[{step}]");

		return $"{into} = {factory.Method}({string.Join(", ", arguments)});";
	}

	void Construct(Writer file, Built built)
	{
		var factories = built.Factories ?? [];

		if (factories.Count == 0)
		{
			var arguments = new List<string>();

			foreach (var member in built.Members)
				arguments.Add(Value(member));

			file.Line($"value = new {built.TypeName}(");

			using (file.Indent())
				for (var i = 0; i < arguments.Count; i++)
					file.Line(arguments[i] + (i < arguments.Count - 1 ? "," : ");"));

			return;
		}

		if (factories.Count == 1)
		{
			file.Line(Apply(factories[0], "value", null));

			return;
		}

		// Which alternative matched was recorded while matching; the value is built from
		// it now, so the C# runs on the parse that happened and on no other.
		Switch(file, Chosen, factories, (_, factory) => factory.Accumulator is null, "value", null);

		if (!Folding)
			return;

		// Then the chain, in the order the steps matched, each applied to what the ones
		// before it built.
		foreach (var factory in factories)
			if (factory.Accumulator is not null)
				file.Line($"var n{IndexOf(factory)} = 0;");

		file.Line();

		using (file.Block($"for (var i = 0; i < {Steps}.Count; i++)"))
			Switch(file, $"{Steps}[i]", factories, (_, factory) => factory.Accumulator is not null, "value", true);
	}

	/// <summary>One <c>switch</c> over the alternatives that answer to a recorded number.</summary>
	void Switch(
		Writer file, string on, IReadOnlyList<Factory> factories,
		Func<int, Factory, bool> wanted, string into, bool? counting)
	{
		using (file.Block($"switch ({on})"))
			for (var i = 0; i < factories.Count; i++)
			{
				if (!wanted(i, factories[i]))
					continue;

				file.Line($"case {i}:");

				using (file.Indent())
				{
					file.Line(Apply(factories[i], into, counting is null ? null : $"n{i}"));

					if (counting is not null)
						file.Line($"n{i}++;");

					file.Line("break;");
				}

				file.Line();
			}
	}

	/// <summary>
	/// One member: the slot that was written, or null when none of them was.
	/// </summary>
	/// <remarks>
	/// More than one slot when the same name is captured in more than one alternative.
	/// They are tried in the order the notation writes them, which is the order in which
	/// at most one of them can have been reached.
	/// </remarks>
	string Value(ResultMember member)
	{
		// A sequence is never absent and never shared between names: no iterations is an
		// empty array, so there is nothing to test and nothing to fall through to.
		if (member.IsSequence)
			return $"l{member.Slots[0]}.ToArray()";

		if (member.Slots.Count == 1 && !member.IsOptional)
			return Read(member, member.Slots[0]);

		var expression = member.IsOptional ? "null" : $"default({Type(member)})!";

		for (var i = member.Slots.Count - 1; i >= 0; i--)
			expression = $"{Written(member, member.Slots[i])} ? {Read(member, member.Slots[i])} : {expression}";

		return expression;
	}

	string Type(ResultMember member) => _results.ValueOf(member.Rule);

	static string Read(ResultMember member, int slot) =>
		member.Rule is null
			? $"text.Slice(s{slot}_from, s{slot}_to - s{slot}_from).ToString()"
			: $"v{slot}";

	/// <summary>
	/// Whether a slot was written. A pair of positions says so by itself; a value needs a
	/// flag beside it, because a rule may declare itself <c>: @int</c> and a value type
	/// has no null to mean "not written".
	/// </summary>
	/// <remarks>
	/// Which slots need one is only known once everything is compiled, so it is recorded
	/// here and the declarations are written afterwards — a flag nobody reads is a
	/// warning in the consumer's build, and warnings there are ours to prevent.
	/// </remarks>
	string Written(ResultMember member, int slot)
	{
		if (member.Rule is null)
			return $"s{slot}_from >= 0";

		_flagged.Add(slot);

		return $"v{slot}_set";
	}

	/// <summary>
	/// The value slots whose written-ness is ever asked about — a member that may be
	/// absent, or one filled from more than one place. Anywhere else the value is simply
	/// there, and a flag nobody reads is a warning in the consumer's build.
	/// </summary>
	readonly HashSet<int> _flagged = [];
	readonly List<(Writer Writer, int Next)> _recursiveCalls = [];

	/// <summary>The whole machine as one method.</summary>
	/// <param name="pattern">
	/// What it recognizes, in notation, written above it. The states below carry the
	/// fragment each one is; this is the only place the whole of it is legible, and
	/// after normalization — so it is what the method does rather than what was typed.
	/// </param>
	public string Render(int entry, string? pattern = null)
	{
		WriteDeferred();

		foreach (var (writer, next) in _recursiveCalls)
		{
			writer.Line("if (cp + 3 > calls.Length) calls = Grow(calls);");
			writer.Line($"calls[cp] = {next}; calls[cp + 1] = p; calls[cp + 2] = bp; cp += 3;");
			writer.Line("bp = sp;");
			writer.Line($"state = {entry};");
			writer.Line("continue;");
		}

		var file = new Writer(0);

		if (pattern is not null)
			foreach (var line in Wrap(pattern))
				file.Line("// " + line);

		var built = _builds is null ? "" : $", out {_builds.TypeName} value";
		var failure = IsLookahead ? "" : $", ref {CSharpEmitter.FailureType} failure";

		// Only a rule of binding powers takes one (§4.3.1). Every other recognizer keeps
		// the shape it has always had, so a grammar that never reaches for them is
		// generated exactly as it was.
		var strength = TakesPower ? $", int {Power}" : "";

		using (file.Block(
			$"static int {Name}(global::System.ReadOnlySpan<char> text, int pos{strength}{failure}{built})"))
		{
			if (_builds is not null)
			{
				// Assigned before anything can fail, so every way out of the method has it
				// assigned — including the ones that report no match at all.
				file.Line("value = default!;");
				file.Line();
			}

			if (UsesStack)
			{
				// Enough for the great majority of matches; Grow takes over when it is
				// not, so nothing is allocated in the common case and nothing overflows
				// in the uncommon one.
				file.Line($"global::System.Span<int> bt = stackalloc int[{Backtracking}];");
				file.Line();
				file.Line("var sp    = 0;");
				file.Line("var saved = 0;");

				if (_marked)
					file.Line($"var {Mark}   = 0;");
			}
			else if (_recursiveCalls.Count > 0)
			{
				file.Line("var sp = 0;");
			}

			if (_recursiveCalls.Count > 0)
			{
				file.Line($"// Recursive component: {_recursiveRule!.Name}.");
				file.Line("// Each frame is return state, call position, and caller backtracking base.");
				file.Line($"global::System.Span<int> calls = stackalloc int[{Backtracking}];");
				file.Line();
				file.Line("var cp = 0;");
				file.Line("var bp = 0;");
			}

			file.Line("var p     = pos;");

			if (UsesResult)
				file.Line("var r     = 0;");

			// A lookahead takes no failure of its own and must still call rules, which do.
			// One of its own, thrown away with it: how far it looked before answering "no"
			// is not how far the parse got, and this is what keeps that true.
			if (IsLookahead && UsesResult)
				file.Line($"var failure = new {CSharpEmitter.FailureType}();");

			if (UsesChar)
				file.Line("var c     = '\\0';");

			for (var i = 0; i < _counters; i++)
				file.Line($"var c{i}    = 0;");

			file.Line($"var state = {entry};");


			// One pair of positions per text capture, one reference per captured value.
			// Unwritten is -1 and null, which is what tells "matched nothing here" from
			// "was never reached" — an optional capture is the difference.
			if (Factories.Count > 1)
			{
				file.Line();
				file.Line($"var {Chosen} = -1;");

				if (Folding)
					file.Line(
						$"var {Steps} = new global::System.Collections.Generic.List<int>();");
			}

			if (Layout.Slots.Count > 0)
			{
				file.Line();

				foreach (var slot in Layout.Slots)
					file.Line(slot switch
					{
							// A fold step's text captures collect too: its `=>` is applied once
						// per iteration and wants that iteration's text.
						{ Rule: null, IsSequence: true } =>
							$"var s{slot.Index}_from = -1; " +
							$"var l{slot.Index} = new global::System.Collections.Generic.List<string>();",

						{ Rule: null } => $"var s{slot.Index}_from = -1; var s{slot.Index}_to = -1;",

						// The one the call writes into, and the one it is appended to. Two,
						// because an `out` needs somewhere to land whether or not it matched.
						{ IsSequence: true } =>
							$"{_results.QualifiedOf(slot.Rule)} v{slot.Index} = default!; " +
							$"var l{slot.Index} = new global::System.Collections.Generic.List<" +
							$"{_results.QualifiedOf(slot.Rule)}>();",

						// A flag rather than null, because a rule may declare itself
						// `: @int` and a value type has no null to mean "not written".
						_ =>
							$"{_results.QualifiedOf(slot.Rule)} v{slot.Index} = default!;" +
							(_flagged.Contains(slot.Index) ? $" var v{slot.Index}_set = false;" : ""),
					});
			}

			file.Line();

			// The loop exists only for the one `continue` that resumes from the stack.
			// Without a stack nothing leaves the switch except by returning, and wrapping
			// it would be scaffolding around nothing.
			using (UsesStack || _recursiveCalls.Count > 0 ? file.Block("while (true)") : null)
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

					if (_recursiveCalls.Count > 0)
					{
						file.Line("if (cp == 0)");
						file.Then("return p;");
						file.Line();
						file.Line("sp = bp;");
						file.Line("cp -= 3;");
						file.Line("state = calls[cp];");
						file.Line("bp = calls[cp + 2];");
						file.Line("continue;");
					}
					else
					{
						file.Line("return p;");
					}
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

						if (Reaches)
						{
							file.Line("if (p > failure.Reach)");
							file.Then("failure.Reach = p;");
							file.Line();
						}
					}

					if (_recursiveCalls.Count > 0)
					{
						file.Line("if (sp == bp)");

						using (file.Block(""))
						{
							file.Line("if (cp == 0)");
							file.Then("return -1;");
							file.Line();
							file.Line("sp = bp;");
							file.Line("cp -= 3;");
							file.Line("p = calls[cp + 1];");
							file.Line("bp = calls[cp + 2];");
							file.Line($"state = {Fail};");
							file.Line("continue;");
						}

						file.Line();
					}

					if (UsesStack)
					{
						if (_recursiveCalls.Count == 0)
						{
							file.Line("if (sp == 0)");
							file.Then("return -1;");
							file.Line();
						}

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
