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
/// way back. §10 of the language says ordered choice backtracks fully, and rests the
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

	public Machine(string name) => Name = name;

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
		var source = node?.ToString() ?? "";
		var text   = new System.Text.StringBuilder(source.Length);

		foreach (var c in source)
			if (c is '\r' or '\n' or '\t' or '\u0085' or '\u2028' or '\u2029' || char.IsControl(c))
				text.Append("\\u").Append(((int)c).ToString("X4"));
			else
				text.Append(c);

		var one = text.ToString();

		if (one.Length > 64)
			one = one.Substring(0, 61) + "...";

		return note is null ? one : one.Length == 0 ? note : $"{one} — {note}";
	}

	/// <summary>Accept and Fail take the first two numbers and are written by hand.</summary>
	const int FirstState = 2;

	string NewCounter() => "c" + _counters++;

	/// <summary>
	/// Records a point the match could return to: where to resume, where the input was,
	/// and one saved value — a repetition's count, for the states that need theirs back.
	/// </summary>
	void Push(Writer writer, int state, string saved)
	{
		UsesStack = true;

		writer.Line($"if (sp + 3 > bt.Length) bt = Grow(bt);");
		writer.Line($"bt[sp] = {state}; bt[sp + 1] = p; bt[sp + 2] = {saved}; sp += 3;");
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

					Push(writer, attempt, "0");
					writer.Line($"goto case {entry};");

					attempt = state;
				}

				return attempt;
			}

			case Node.Repeat(var body, var min, var max):
				return CompileRepeat(body, min, max, next);

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

			case Node.Call(var rule, _):
			{
				var state = Reserve(out var writer, node);

				UsesResult = true;

				writer.Line($"r = {CSharpEmitter.MethodOf(rule)}(text, p);");
				writer.Line();
				writer.Line("if (r < 0)");
				writer.Then($"goto case {Fail};");
				writer.Line();
				writer.Line("p = r;");
				writer.Line($"goto case {next};");

				return state;
			}

			// Transparent while a rule's value is the text it matched.
			case Node.Capture(_, var captured):
				return Compile(captured, next);

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
	int CompileRepeat(Node body, int min, int? max, int next)
	{
		var counter = NewCounter();

		var repeat = new Node.Repeat(body, min, max);

		var exit  = Reserve(out var atExit,  repeat, "stop, and check the count");
		var loop  = Reserve(out var atLoop,  repeat, "take another, or leave stopping open");
		var after = Reserve(out var atAfter, repeat, "one more taken");
		var entry = Reserve(out var atEntry, repeat, "start counting");

		var start = Compile(body, after);

		atEntry.Line($"{counter} = 0;");
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

		Push(atLoop, exit, counter);
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

	string CompileLookahead(Node body)
	{
		var machine = new Machine($"{Name}_Look{_lookaheads++}");
		var entry   = machine.Compile(body, Accept);

		foreach (var extra in machine.Extra)
			_extra.Add(extra);

		_extra.Add(machine.Render(entry));

		return machine.Name;
	}

	// ── Rendering ────────────────────────────────────────────────────────────────

	/// <summary>The whole machine as one method.</summary>
	public string Render(int entry)
	{
		var file = new Writer(0);

		using (file.Block($"static int {Name}(global::System.ReadOnlySpan<char> text, int pos)"))
		{
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
			file.Line();

			// The loop exists only for the one `continue` that resumes from the stack.
			// Without a stack nothing leaves the switch except by returning, and wrapping
			// it would be scaffolding around nothing.
			using (UsesStack ? file.Block("while (true)") : null)
			using (file.Block("switch (state)"))
			{
				file.Line($"case {Accept}:");

				using (file.Indent())
					file.Line("return p;");

				file.Line();
				file.Line($"case {Fail}:");

				using (file.Indent())
				{
					if (UsesStack)
					{
						file.Line("if (sp == 0)");
						file.Then("return -1;");
						file.Line();
						file.Line("sp    -= 3;");
						file.Line("state  = bt[sp];");
						file.Line("p      = bt[sp + 1];");
						file.Line("saved  = bt[sp + 2];");
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
