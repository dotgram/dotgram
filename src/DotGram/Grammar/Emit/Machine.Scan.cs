using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A rule that recognizes and remembers nothing, compiled as a plain method with no
/// arena at all.
/// </summary>
/// <remarks>
/// <para>
/// The rule must wear atomic braces and keep no records, and its body must pass
/// <see cref="Scannable"/>. The braces are the licence: an atomic group commits its
/// first reading, so a compilation that finds the first reading and nothing else — each
/// choice committing the first alternative that matches, each repetition greedy — is not
/// an approximation of §11 inside them, it is §11 inside them. Backtracking state that
/// the automaton would keep in the arena lives in locals, restored on the spot.
/// </para>
/// <para>
/// What this buys is the seam. Trivia is such a rule in every grammar that takes §4.5's
/// advice, it is applied at every seam, and through the automaton each application cost
/// an atomic entry, a repeat entry, their unwinding and a commit walk. Through a scanner
/// it costs what a hand-written <c>SkipWhitespaceAndComments</c> costs: one call.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The scanner a call to <paramref name="rule"/> may become, if any.</summary>
	string? ScannerOf(RuleSymbol rule)
	{
		if (BufferedInput)
			return null;

		if (_scanners.TryGetValue(rule, out var known))
			return known;

		var name =
			_graph.Bodies.TryGetValue(rule, out var body) && Scans(rule, body)
				// Tagged like every other name this machine emits: a grammar with two
				// publications has two machines in one class, and both may reach the same
				// scanner — `trivia` does, in every spaced grammar with more than one
				// thing published — which without this is one method defined twice.
				? "Scan_" + CSharpEmitter.IdentifierOf(rule) + _tag
				: null;

		_scanners[rule] = name;

		return name;
	}

	readonly Dictionary<RuleSymbol, string?> _scanners = [];

	sealed record ScalarScan(string Name, Node.Element Head, Node.Element Tail);

	readonly Dictionary<RuleSymbol, ScalarScan?> _scalarScans = [];

	// A single full-span capture can keep the ordinary deferred-value protocol while
	// recognizing its character run in a method. Failure takes the original path, so
	// diagnostics and recovery still see precisely the terminals that refused.
	ScalarScan? ScalarScanner(RuleSymbol rule)
	{
		if (!Caches || _valuesInLocals || OverKinds)
			return null;

		if (_scalarScans.TryGetValue(rule, out var known))
			return known;

		_scalarScans[rule] = null;

		if (!SitedValued(rule) ||
			_graph.Bodies[rule] is not Node.Construct { Body: Node.Capture capture })
			return null;

		var body = capture.Body;
		var atomic = body is Node.Atomic;

		if (body is Node.Atomic kept)
			body = kept.Body;

		Node.Element head;
		Node.Element tail;

		if (body is Node.Repeat { Body: Node.Element element, Min: 1, Max: null })
		{
			head = tail = element;
		}
		else if (body is Node.Sequence { Nodes.Count: 2 } sequence &&
			sequence.Nodes[0] is Node.Element first &&
			sequence.Nodes[1] is Node.Repeat { Body: Node.Element rest, Min: 0, Max: null })
		{
			head = first;
			tail = rest;
		}
		else
			return null;

		// Start with concrete character ranges. In particular, no user predicate may
		// run twice when the first character sends us back to the ordinary recognizer.
		if (head is not { Ranges.Count: > 0, Categories.Count: 0, References.Count: 0 } ||
			tail is not { Ranges.Count: > 0, Categories.Count: 0, References.Count: 0 })
			return null;

		var following = Follows(rule);
		if (!atomic && !Scannable(body, following.Plain.Or(following.AfterSeam)))
			return null;

		var plan = new ScalarScan("Read_" + CSharpEmitter.IdentifierOf(rule) + _tag, head, tail);
		_scalarScans[rule] = plan;
		return plan;
	}

	void RenderScalarScanners(Writer file)
	{
		foreach (var plan in _scalarScans.Values)
		{
			if (plan is null)
				continue;

			using (file.Block($"static int {plan.Name}({InputType} text, int p{ScannerParameters})"))
			{
				file.Line(_starves ? $"if ({Short(1)}) {{ failure.Starved = true; return -1; }}" : $"if ({Short(1)}) return -1;");
				file.Line($"var c = {ReadAt("p")};");
				file.Line($"if (!({CSharpEmitter.Test(plan.Head, Tabulate)})) return -1;");
				file.Line("p++;");
				using (file.Block("while (true)"))
				{
					using (file.Block($"if ({Short(1)})"))
					{
						if (_starves)
							file.Line("failure.Starved = true;");
						file.Line("break;");
					}
					file.Line($"c = {ReadAt("p")};");
					file.Line($"if (!({CSharpEmitter.Test(plan.Tail, Tabulate)})) break;");
					file.Line("p++;");
				}
				file.Line("return p;");
			}
			file.Line();
		}
	}

	/// <summary>
	/// Whether a rule may be read by committing, and so compiled with nothing written down.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Braces are the author saying so: an atomic group commits its first reading by
	/// definition, so nothing after it can force this search back, and <see cref="Scannable"/>
	/// is asked with nothing following.
	/// </para>
	/// <para>
	/// Without braces the compiler proves the same sentence. What may follow the rule is
	/// <see cref="FollowSets"/>, and <see cref="Scannable"/> already knows what to do with it
	/// — a greedy repetition is safe exactly where what comes after cannot begin where a turn
	/// begins. The set is the union over every call site, so a rule reached from two places
	/// is judged against both, and one place needing the input back keeps it out. `Rfc3986`
	/// has no braces anywhere and forty-four rules that qualify.
	/// </para>
	/// <para>
	/// Two things the proof cannot see, both found by a test rather than by reasoning.
	/// <b>The seam</b>: §4.5 weaves it between operands, and a publication weaves it again
	/// around what it publishes — and that last is not a call site the graph records, so the
	/// follow set has never heard of the one place where giving the spaces back is the whole
	/// parse. <b>Kinds</b>: a split grammar cuts its values out of the extents of the tokens
	/// it ran over, and a scanner swallows tokens without recording any.
	/// </para>
	/// </remarks>
	bool Scans(RuleSymbol rule, Node body)
	{
		// Not a rule lifted out of a marked repetition (§8.2): its body is a repetition marked
		// `recover`, which a scanner, committing to what it reads and recording nothing, would read
		// as a plain one.
		if (_graph.Lifted.ContainsKey(rule))
			return false;

		if (body is Node.Atomic(var kept))
			return !KeepsRecords(kept) && Scannable(kept, FirstSets.First.None);

		if (KeepsRecords(body))
			return false;

		// Not over kinds. What a scanner is worth is swallowing a run of input in one call
		// with nothing written down, and over kinds a step is a whole token — there are no
		// runs to swallow, so all that is left is the call. Measured, it made `SqlStandard92`
		// take twice as long.
		if (OverKinds)
			return false;

		// Not a rule whose body names itself better than the rule does. A scanner is one
		// call, so the only thing its refusal can name is the rule — `Expected B.` where the
		// same grammar compiled in place says `Expected "abqy".`, which is what a reader
		// wanted. That is a loss for a literal and a gain for everything else: `Expected
		// RegName.` beats a hundred character ranges, and a class or a repetition has no
		// name of its own to lose.
		if (Spells(body))
			return false;

		foreach (var seam in _graph.Trivia.Values)
			if (seam is Node.Call(var woven, _) && woven == rule)
				return false;

		_follows ??= FollowSets.Of(_graph);

		// Both readings of what follows, with a seam woven in and without: a rule whose
		// callers disagree about the seam is judged against the wider of the two. Asked of the
		// view (Continuation.Taught): a scanner keeps only the first reading, and that loses
		// nothing only where what follows cannot fail and ask for another — a question the
		// stop after a yielded element or a positional reading answers.
		return _follows.TryGetValue(rule, out var following) &&
			Scannable(body, following.Taught);
	}

	/// <summary>
	/// <see cref="Scannable(Node, FirstSets.First, HashSet{RuleSymbol})"/> against both readings
	/// of a continuation, with a seam woven in and without.
	/// </summary>
	bool Scannable(Node body, FollowSets.Continuation following)
	{
		return Scannable(body, following.Plain.Or(following.AfterSeam));
	}

	IReadOnlyDictionary<RuleSymbol, FollowSets.Continuation>? _follows;

	/// <summary>Whether what a body would have said about itself beats naming the rule.</summary>
	/// <remarks>
	/// Literals, and choices of them. Those are the shapes whose expectation is worth
	/// reading — a run of text somebody can look for — and the shapes a scanner would
	/// flatten into its own name.
	/// </remarks>
	static bool Spells(Node node)
	{
		return node switch
		{
			Node.Literal(var text) => text.Length > 0,
			Node.Choice(var alternatives) => alternatives.All(Spells),
			Node.Atomic(var kept) => Spells(kept),
			Node.Marked(var kept, _) => Spells(kept),
			_ => false,
		};
	}

	/// <summary>Whether a node matches at every position — whether it cannot refuse.</summary>
	/// <remarks>
	/// <para>
	/// Not "can it match the empty string", which is what <c>FirstSets.Nullable</c> answers
	/// and what the scanner's caller used to ask. A lookahead matches the empty string when
	/// it succeeds and refuses when it does not: nullable and fallible at once. Reading such
	/// a scanner's answer as a position without asking whether it is one puts <c>-1</c> in
	/// <c>p</c>, and what happens next is whatever the rest of the rule does with a negative
	/// position — in the grammar that found this, the right answer by accident, and in the
	/// one before it the wrong one.
	/// </para>
	/// <para>
	/// Conservative in the safe direction: a false answer costs a comparison the parse did
	/// not need, and a true one has to be true.
	/// </para>
	/// </remarks>
	bool Infallible(Node node)
	{
		return Infallible(node, []);
	}

	bool Infallible(Node node, HashSet<RuleSymbol> seen)
	{
		return node switch
		{
			Node.Empty => true,
			Node.Literal(var text) => text.Length == 0,
			Node.Repeat(_, var min, _) => min == 0,
			Node.Sequence(var parts) => parts.All(part => Infallible(part, seen)),
			Node.Choice(var alternatives) => alternatives.Any(one => Infallible(one, seen)),
			Node.Atomic(var kept) => Infallible(kept, seen),
			Node.Marked(var kept, _) => Infallible(kept, seen),
			Node.Capture(_, var held) => Infallible(held, seen),
			Node.Construct(var built, _) => Infallible(built, seen),
			Node.Call(var called, _) => seen.Add(called) &&
				_graph.Bodies.TryGetValue(called, out var body) &&
				Infallible(body, seen),
			_ => false,
		};
	}

	/// <summary>
	/// The scanner one rule becomes, asked for rather than found by compiling.
	/// </summary>
	/// <remarks>
	/// A machine registers a scanner when it compiles a call to one. A lexical machine has
	/// no calls to compile and still needs the seam — the tokenizer skips trivia between
	/// terminals — so it asks for the one rule it wants and renders that.
	/// </remarks>
	public string? Scanner(RuleSymbol rule)
	{
		return ScannerOf(rule);
	}

	/// <summary>
	/// Whether this machine's scanners say where a pair they count was left open by the end of the
	/// text: set on the seam of a grammar cut into a lexer, whose tokenizer refuses there.
	/// </summary>
	/// <remarks>
	/// The counter of <c>Std.Nested</c> reads to the end of the text before it knows a pair is
	/// unclosed. Where the trivia stopped short of such a pair, the tokenizer would otherwise read
	/// on and ask the trivia again inside it, each asking reading to the end again: the square of
	/// the text's length on a text of unclosed openings. A scanner that records it takes
	/// <c>ref int ranOut</c>, raised to where the pair opened.
	/// </remarks>
	internal bool RecordsRunOut { get; init; }

	/// <summary>Whether the scanner of <paramref name="rule"/> takes <c>ranOut</c>: it reads a counted pair.</summary>
	internal bool ScannerRunsOut(RuleSymbol rule)
	{
		if (!RecordsRunOut)
			return false;

		if (_scannerRunsOut.TryGetValue(rule, out var known))
			return known;

		var runsOut = new ScanWriter(_graph, static _ => null, static _ => "true", _tag, _starves, StopCharacters, recordsRunOut: true)
			.Render(ScanBody(rule))
			.Contains("ranOut", StringComparison.Ordinal);

		_scannerRunsOut[rule] = runsOut;

		return runsOut;
	}

	readonly Dictionary<RuleSymbol, bool> _scannerRunsOut = [];

	/// <summary>
	/// Whether the scanner of <paramref name="rule"/> can match short of where it read to: it gives
	/// input back to itself, so the furthest it reached is worth handing back on a match too.
	/// </summary>
	/// <remarks>
	/// Asked of the scanner's own text, written once with nothing declared — the class tables and
	/// sets a real rendering declares are asked for later, where it is rendered, in the order they
	/// always were.
	/// </remarks>
	internal bool ScannerReports(RuleSymbol rule)
	{
		if (_scannerReports.TryGetValue(rule, out var known))
			return known;

		// The seam reports too where a turn that ends it began to agree with a literal and stopped —
		// a comment's `--` or `/*` the input ends inside, or does not go on with — which the readers
		// record as they try the turn (§7.5). Only that: its records are kept only where they are
		// read (ScanWriter.Exits), so it is asked written as it would be read, and a seam whose
		// turns hold no such literal reports nothing and is written as it was. Not the seam a lexer
		// skips with, which says where the text ran out on its own (RecordsRunOut) and records no
		// refusal; asked by its tag, since this is asked while the machine is being built.
		var body    = ScanBody(rule);
		var reports = SeamReached.Contains(rule)
			? _tag != SeamTag && new ScanWriter(_graph, static _ => null, static _ => "true", _tag, _starves, StopCharacters) { Seam = true }
				.Render(body, reports: true)
				.Contains("> furthest", StringComparison.Ordinal)
			: new ScanWriter(_graph, static _ => null, static _ => "true", _tag, _starves, StopCharacters)
				.Render(body)
				.Contains("furthest", StringComparison.Ordinal);

		_scannerReports[rule] = reports;

		return reports;
	}

	readonly Dictionary<RuleSymbol, bool> _scannerReports = [];

	/// <summary>What the scanner of <paramref name="rule"/> is written from.</summary>
	/// <remarks>
	/// The body, inside its braces; or, for Std.Nested itself, a call to it, which is read with
	/// the counter rather than with the body the counter stands for.
	/// </remarks>
	Node ScanBody(RuleSymbol rule)
	{
		if (NestedPair.Is(rule))
			return new Node.Call(rule, []);

		return _graph.Bodies[rule] is Node.Atomic(var kept) ? kept : _graph.Bodies[rule];
	}

	/// <summary>Every scanner the compiled states call, rendered as methods.</summary>
	/// <summary>What a scanner takes beyond the input, where it has anything to say.</summary>
	string ScannerParameters =>
		_starves ? $", ref {CSharpEmitter.FailureType} failure" : "";

	/// <summary>And what a call to one hands over.</summary>
	string ScannerArguments => _starves ? ", ref failure" : "";

	public string RenderScanners()
	{
		var file    = new Writer(0);
		var reaches = false;

		RenderScalarScanners(file);

		foreach (var pair in _scanners)
		{
			if (pair.Value is not { } name)
				continue;

			var rule = pair.Key;
			var body = ScanBody(rule);

			var scan  = new ScanWriter(_graph, Tabulate, one => RangesTest(one, Tabulate), _tag, _starves, StopCharacters, RecordsRunOut)
			{
				Seam = SeamReached.Contains(rule) && _tag != SeamTag,
			};
			var inner = scan.Render(body, ScannerReports(rule));

			reaches |= scan.Reaches;

			file.Line($"/// <summary><c>{rule.Name}</c>, recognized with nothing written down.</summary>");

			using (file.Block(
				$"static int {name}(global::System.ReadOnlySpan<char> text, int pos{ScannerParameters}" +
				(ScannerRunsOut(rule) ? ", ref int ranOut" : "") +
				(ScannerReports(rule) ? ", out int reachedAt)" : ")")))
			{
				file.Write(inner);
			}

			file.Line();
		}

		if (reaches)
		{
			file.Line("/// <summary>How much of a run matched, asked only when it did not.</summary>");
			file.Line("/// <remarks>");
			file.Line("/// A scanner compares a literal run in one go, which is what it costs when the");
			file.Line("/// run matches. When it does not, a refusal has to say how far it came — so");
			file.Line("/// this walks the run, on the path that was going to refuse anyway.");
			file.Line("/// </remarks>");

			using (file.Block(
				$"static int Reach_DotGram{_tag}(" +
				"global::System.ReadOnlySpan<char> text, int pos, global::System.ReadOnlySpan<char> want)"))
			{
				file.Line("var room = text.Length - pos;");
				file.Line();
				file.Line("if (want.Length < room)");
				file.Then("room = want.Length;");
				file.Line();
				file.Line("var at = 0;");
				file.Line();
				file.Line("while (at < room && text[pos + at] == want[at])");
				file.Then("at++;");
				file.Line();
				file.Line("return pos + at;");
			}

			file.Line();
		}

		return file.ToString();
	}

	/// <summary>
	/// Whether committing the first reading is the same as finding it by backtracking.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <paramref name="after"/> is what still has to match <em>inside the group</em>
	/// once this node has; <see cref="FirstSets.First.None"/> means nothing does — the
	/// tail, where success is the commit and greed cannot be wrong. Everywhere short of
	/// the tail a construct must have at most one reading, because something after it may
	/// yet fail and this search will not be coming back.
	/// </para>
	/// <para>
	/// So a choice qualifies when its alternatives are mutually exclusive — disjoint
	/// first sets, or leading literals neither of which begins the other — and committing
	/// the first match loses nothing. A repetition qualifies at the tail, where whatever
	/// it greedily takes is final; or when what follows it cannot begin where a turn
	/// begins, which is the settledness proof again; or as the guarded scan
	/// <c>(?!X &amp; …)* &amp; X</c>, judged as a pair in the sequence walk, which stops
	/// exactly at the first <c>X</c> by construction.
	/// </para>
	/// </remarks>
	bool Scannable(Node node, FirstSets.First after, HashSet<RuleSymbol>? seen = null)
	{
		switch (node)
		{
			case Node.Empty:
			case Node.Literal:
				return true;

			// A reference in an element set is a C# predicate over one character; the
			// test renderer writes the call, and one character keeps the reading unique.
			case Node.Element:
				return true;

			case Node.Sequence(var parts):
			{
				var next = after;

				for (var i = parts.Count - 1; i >= 0; i--)
				{
					if (parts[i] is Node.Repeat scan && GuardOf(scan) is { } guard)
					{
						// Only `*`: the pair is written as a search, which counts no turns, so a bound on
						// them — `{0,2}` — is left to the readings that count them.
						if (scan.Min != 0 || scan.Max is not null ||
							i + 1 >= parts.Count ||
							parts[i + 1] is not Node.Literal(var stop) { IgnoreCase: false } ||
							!string.Equals(stop, guard, StringComparison.Ordinal) ||
							FirstSets.Nullable(scan.Body, _graph) ||
							!Scannable(scan.Body, FirstSets.First.All, seen))
						{
							return false;
						}
					}
					else if (!Scannable(parts[i], next, seen))
						return false;

					next = Before(parts[i], next);
				}

				return true;
			}

			case Node.Choice(var alternatives):
			{
				// An alternative that can read nothing matches at every position, so where it is
				// not the last, what stands after it is never tried however the first sets part:
				// `('b'? | 'a')` reads nothing on `a`, and where what follows then fails, the choice
				// has to be reopened for `'a'`. As the last it is the way out when an earlier one
				// read and what follows refused, and committing to the earlier one loses nothing
				// only where what follows cannot begin where it began — `Word & ('=' & Word)?`
				// before `,` — the settledness argument again. Where nothing follows inside the
				// group, the first reading is the answer either way.
				if (!after.Nothing && alternatives.Count > 1)
				{
					for (var i = 0; i < alternatives.Count; i++)
						if (FirstSets.Nullable(alternatives[i], _graph) &&
							(i < alternatives.Count - 1 ||
							alternatives.Take(i).Any(one => FirstSets.Of(one, _graph).Overlaps(after))))
						{
							return false;
						}
				}

				for (var i = 0; i < alternatives.Count; i++)
					for (var j = i + 1; j < alternatives.Count; j++)
						if (!Exclusive(alternatives[i], alternatives[j]))
							return false;

				foreach (var alternative in alternatives)
					if (!Scannable(alternative, after, seen))
						return false;

				return true;
			}

			// A turn is at the tail only where no turn after it is owed: at the tail a repetition
			// that may stop after its first turn takes what it greedily takes, but one that must
			// turn again has its next turn still to match. `{ ('b'+){2} }` reads `bb` by giving
			// the first run's second `b` back, which a greedy first turn read and then refused.
			case Node.Repeat(var body, var min, _):
				return
					!FirstSets.Nullable(body, _graph) &&
					(after.Nothing || !FirstSets.Of(body, _graph).Overlaps(after)) &&
					Scannable(body, after.Nothing && min <= 1 ? after : FirstSets.Of(body, _graph).Or(after), seen);

			case Node.Lookahead(_, var inside):
				return Scannable(inside, FirstSets.First.All, seen);

			case Node.Behind:
				return true;

			// Over characters the seam was never woven and there is nothing left to ask,
			// so a glued sequence scans like any other. Over kinds it is a question about
			// the tokens, which this rendering does not carry.
			case Node.Glue:
				return !OverKinds;

			case Node.Atomic(var kept):
				return Scannable(kept, after, seen);

			// Std.Nested reaches itself and has one reading all the same, which a counter reads
			// (NestedPair): whatever follows it, there is nothing to come back for.
			case Node.Call(var called, _) when NestedPair.Is(called) && NestedPair.Of(called, _graph) is not null:
				return true;

			case Node.Call(var called, _):
			{
				seen ??= [];

				if (!seen.Add(called) || !_graph.Bodies.TryGetValue(called, out var calledBody))
					return false;

				var can = Scannable(calledBody, after, seen);

				seen.Remove(called);

				return can;
			}

			default:
				return false;
		}
	}

	/// <summary>
	/// What still has to match inside the group where a part of a sequence begins, given what
	/// has to where it ends.
	/// </summary>
	/// <remarks>
	/// The tail stops being one at a part that reads nothing and can refuse: `{ ['a'..'b']+ &amp;
	/// ?='b' }` matches `ab` by giving the `b` back, and greed, which is right where nothing can
	/// fail after it, read both and refused. What then has to match is where the part lets the
	/// reading through — what a positive look looks for, what a refusal of one character does
	/// not refuse (<see cref="FirstSets.Past"/>), and anything for whatever else refuses.
	/// Short of the tail, what reads after the part already says it, the look being only
	/// narrower.
	/// </remarks>
	FirstSets.First Before(Node part, FirstSets.First after)
	{
		if (!after.Nothing || !FirstSets.Nullable(part, _graph) || FirstSets.Unfailing(part, _graph))
			return FollowSets.Plainly(part, after, _graph);

		return part is Node.Lookahead(true, var seen) && !FirstSets.Nullable(seen, _graph) &&
			FirstSets.Of(seen, _graph) is { Anything: false, Nothing: false } wanted
				? wanted
				: FirstSets.Past(part, FirstSets.First.Stop, _graph);
	}

	/// <summary>
	/// Whether a guarded scan's turn is a bare character, so the pair it belongs to is a
	/// search for its delimiter and nothing else.
	/// </summary>
	/// <remarks>
	/// A turn that consumes anything at all — <c>?!"*&#47;" &amp; any</c> — stops exactly
	/// where the delimiter first occurs. A turn narrower than that — <c>?!X &amp;
	/// [^ '\n']</c> — can also stop because its own character test refused, which a
	/// search for the delimiter alone would run straight past: where the characters it refuses
	/// are few they are searched for in what that search passed over (<see cref="EmitScanUntil"/>),
	/// and otherwise that shape keeps the loop.
	/// </remarks>
	/// <returns>
	/// The characters the turn refuses: none for a turn of <c>any</c>, null where the pair is
	/// not a search.
	/// </returns>
	static char[]? ScansUntil(Node.Repeat repeat, RecognitionGraph graph)
	{
		if (repeat.Body is not Node.Sequence({ Count: 2 } parts))
			return null;

		if (Anything(parts[1], graph, []))
			return [];

		return GuardOf(repeat) is { Length: > 0 } && Except(parts[1], graph, []) is { Length: > 0 } except && !except.Any(char.IsSurrogate)
			? except
			: null;
	}

	/// <summary>
	/// The characters a node that is one character refuses, where it is a negated class of at most
	/// five characters and nothing else; null otherwise.
	/// </summary>
	static char[]? Except(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Element { IsNegated: true, Categories.Count: 0, References.Count: 0 } element when !ReadsFewOneByOne.Value:
			{
				var made = new List<char>();

				foreach (var range in element.Ranges)
					for (int c = range.From; c <= range.To; c++)
					{
						if (made.Contains((char)c))
							continue;

						if (made.Count == 5)
							return null;

						made.Add((char)c);
					}

				return [.. made];
			}

			case Node.Atomic(var kept):
				return Except(kept, graph, seen);

			case Node.Call(var called, _) when seen.Add(called) && graph.Bodies.TryGetValue(called, out var body):
				return Except(body, graph, seen);

			default:
				return null;
		}
	}

	/// <summary>Whether a node is one character, whichever character it is.</summary>
	/// <remarks>
	/// <c>any</c> is a built-in rule rather than an element written in
	/// place, so the call is followed — the same unwrapping every other analysis here
	/// does, with a ring of calls refused rather than walked forever.
	/// </remarks>
	static bool Anything(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		return node switch
		{
			Node.Element element => string.Equals(
											CSharpEmitter.Test(element), "true", StringComparison.Ordinal),
			Node.Atomic(var kept) => Anything(kept, graph, seen),
			Node.Call(var called, _) => seen.Add(called) &&
										graph.Bodies.TryGetValue(called, out var body) &&
										Anything(body, graph, seen),
			_ => false,
		};
	}

	/// <summary>The literal a guarded scan stops at, where the repetition is one.</summary>
	static string? GuardOf(Node.Repeat repeat)
	{
		return repeat.Body is Node.Sequence(var parts) &&
		parts.Count >= 1 &&
		parts[0] is Node.Lookahead(false, Node.Literal(var guard) { IgnoreCase: false })
			? guard
			: null;
	}

	/// <summary>Whether at most one of the two can match at any one position.</summary>
	bool Exclusive(Node one, Node other)
	{
		if (!FirstSets.Of(one, _graph).Overlaps(FirstSets.Of(other, _graph)))
			return true;

		// A negative lookahead in front of one says where it cannot begin, and where that is
		// wherever the other's leading literal stands the two never meet: `"/*" & …` against
		// `?!"/*" & any`, a comment inside a comment against a character of it.
		if (Refuses(one, LeadingLiteral(other)) || Refuses(other, LeadingLiteral(one)))
			return true;

		if (LeadingLiteral(one) is not { } mine || LeadingLiteral(other) is not { } theirs)
			return false;

		if (Differ(mine, theirs))
			return true;

		// The same literal in front of both: what stands after it decides, where each must
		// read something there and the two cannot read the same thing — `'(' & Expression
		// & ')'` against `'(' & "SELECT" & …` part ways at the second token, and a choice
		// between them needs no way back. The trivia both read after the literal decides
		// nothing and is looked past.
		return mine == theirs &&
			PastLiteral(one) is { } restOne && PastLiteral(other) is { } restOther &&
			!FirstSets.Nullable(restOne, _graph) && !FirstSets.Nullable(restOther, _graph) &&
			FirstSets.Of(restOne, _graph).IsKnown && FirstSets.Of(restOther, _graph).IsKnown &&
			Exclusive(restOne, restOther);
	}

	/// <summary>
	/// The node with its leading literal taken off, and the seam behind that literal with
	/// it — or null where the front is not a literal that stands as a part of its own.
	/// </summary>
	Node? PastLiteral(Node node)
	{
		switch (node)
		{
			case Node.Sequence(var parts) when parts.Count > 1 && parts[0] is Node.Literal { IgnoreCase: false }:
			{
				var rest = parts.Skip(1).ToList();

				if (rest.Count > 1 && _seam is not null &&
					rest[0] is Node.Call(var called, { Count: 0 }) && ReferenceEquals(called, _seam))
				{
					rest.RemoveAt(0);
				}

				return rest.Count == 1 ? rest[0] : new Node.Sequence(rest);
			}

			case Node.Sequence(var parts) when parts.Count > 1 && PastLiteral(parts[0]) is { } front:
				return new Node.Sequence([front, .. parts.Skip(1)]);

			case Node.Call(var called, _) when _graph.Bodies.TryGetValue(called, out var body):
				return PastLiteral(body);

			default:
				return null;
		}
	}

	/// <summary>Neither begins the other: they part ways within the shorter's length.</summary>
	static bool Differ(string one, string other)
	{
		var shared = Math.Min(one.Length, other.Length);

		for (var i = 0; i < shared; i++)
			if (one[i] != other[i])
				return true;

		return false;
	}

	string? LeadingLiteral(Node node)
	{
		return node switch
		{
			Node.Literal(var text) { IgnoreCase: false } => text,
			Node.Sequence(var parts) when parts.Count > 0 => LeadingLiteral(parts[0]),
			Node.Call(var called, _) when _graph.Bodies.TryGetValue(called, out var body)
				=> LeadingLiteral(body),
			_ => null,
		};
	}

	/// <summary>Whether a node refuses wherever <paramref name="literal"/> stands.</summary>
	/// <remarks>
	/// Asked of the negative lookaheads it opens with, the one place a refusal is written
	/// down: <c>?!"/*" &amp; any</c> refuses wherever <c>/*</c> begins, and so wherever
	/// <c>/**</c> does.
	/// </remarks>
	static bool Refuses(Node node, string? literal)
	{
		if (literal is null)
			return false;

		IReadOnlyList<Node> parts = node is Node.Sequence(var sequence) ? sequence : [node];

		foreach (var part in parts)
		{
			if (part is not Node.Lookahead(var positive, var inside))
				return false;

			if (!positive && inside is Node.Literal(var guard) { IgnoreCase: false } &&
				guard.Length > 0 && literal.StartsWith(guard, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The checkpoint emitter. Every node's code either falls through with <c>p</c>
	/// advanced past it, or jumps to its fail label with <c>p</c> exactly where it was.
	/// </summary>
	/// <param name="starves">
	/// Whether the scanner is to say that its reading ran into the end of the input.
	/// </param>
	sealed class ScanWriter(
		RecognitionGraph graph,
		Func<IReadOnlyList<CharRange>, string?> tabulate,
		Func<IReadOnlyList<CharRange>, string> ranges,
		string tag = "",
		bool starves = false,
		Func<Node, char[]?>? stops = null,
		bool recordsRunOut = false)
	{
		static string Short(int count)
		{
			return count == 1 ? "(uint)p >= (uint)text.Length" : $"text.Length - p < {count}";
		}

		int _labels;
		int _marks;

		/// <summary>
		/// Where a read that ran into the end of the text goes: to <paramref name="fail"/>, saying first that it
		/// ran out where a look stands around it and the scanner is to say so.
		/// </summary>
		/// <remarks>
		/// A look puts the position back and raises nothing on how far the scan got, so neither of the exits
		/// can tell afterwards that it read to the end; this read can. Outside a look the refusal or the match
		/// that follows says it from how far the scan got.
		/// </remarks>
		string RanOut(string fail)
		{
			return starves && _lookingAhead > 0 ? $"{{ failure.Starved = true; goto {fail}; }}" : $"goto {fail};";
		}

		/// <summary>
		/// <see cref="RanOut"/> for a read of one character where it stands. Inside a look it stands where
		/// the look does or past something the look read first: the way back a look's refusal takes raises how
		/// far the scan got to there, the end, and the exits say so — except in the seam, whose way back past a
		/// look is quiet.
		/// </summary>
		string RanOutHere(string fail)
		{
			return Seam ? RanOut(fail) : $"goto {fail};";
		}

		/// <summary>How many lookaheads the code being written stands in: a pair left open there is not recorded.</summary>
		int _lookingAhead;
		int _deepestMark;
		int _turns;

		/// <summary>The failure labels that put the position back where a sequence began.</summary>
		readonly HashSet<string> _restores = [];

		/// <summary>The labels a repetition's turn fails into where the repetition ends.</summary>
		readonly HashSet<string> _exits = [];

		/// <summary>The labels an alternative fails into where the next one is tried.</summary>
		readonly HashSet<string> _alternatives = [];

		/// <summary>
		/// Whether a literal that fails into <paramref name="fail"/> raises how far the scan got only where
		/// it began to agree: a repetition's end and the next alternative are where the scan goes on, and
		/// the readers record such a literal only as far as it agreed — a turn or an alternative that does
		/// not begin is how every such loop and choice goes on.
		/// </summary>
		bool AgreesFirst(string fail)
		{
			return _exits.Contains(fail) || _alternatives.Contains(fail);
		}

		/// <summary>Whether this is the scan of the seam, whose refusals the readers do not record.</summary>
		public bool Seam { get; init; }

		/// <summary>
		/// Where a look that fails into <paramref name="fail"/> goes: in the seam, a sequence's way back
		/// past the line that raises how far the scan got. A look there puts the position back, and the
		/// readers record nothing it refused — only a literal of the seam's that began to agree says how
		/// far it got — so the place it stands is not how far the scan came. Elsewhere the readers record
		/// what a look refused, and the way back raises as for any part.
		/// </summary>
		string Quiet(string fail)
		{
			return Seam && _restores.Contains(fail) ? fail + "_q" : fail;
		}

		/// <summary>
		/// Whether a run that fails into <paramref name="fail"/> has to put the position back
		/// itself: not into a sequence's own way back, which puts it further back still, and not
		/// into the scanner's refusal, which reports how far it got and not where it stands.
		/// </summary>
		bool GivesBackTo(string fail)
		{
			return fail != "Refuse" && !_restores.Contains(fail);
		}

		/// <summary>
		/// Whether a refusal that fails into <paramref name="fail"/> raises how far the scan got: the
		/// scan's own refusal, a sequence's way back, which raises it to where the sequence stopped
		/// (a literal's matching beginning included), and a turn that ends its repetition, whose
		/// refusal the readers record as they try it — but never inside a look, which puts the
		/// position back and reaches nothing, as the reader's looks record nothing.
		/// </summary>
		bool Records(string fail)
		{
			return _lookingAhead == 0 && (fail == "Refuse" || _restores.Contains(fail) || AgreesFirst(fail));
		}
		/// <summary>
		/// The marks around what a turn that ends its repetition records (<see cref="Exits"/>).
		/// </summary>
		const string ExitOpen  = "/*exit*/";
		const string ExitElse  = "/*else*/";
		const string ExitClose = "/*end*/";

		/// <summary>
		/// What a turn that ends its repetition recorded of how far it got, kept where anything reads
		/// it — the scan's refusal, the place it reports, its running out on a match — and taken out
		/// where nothing does: the seam's loops end at every operand, and a record nobody reads there
		/// is a comparison on the path that matches.
		/// </summary>
		string Exits(string written, bool read)
		{
			if (written.IndexOf(ExitOpen, StringComparison.Ordinal) < 0)
				return written;

			var keep = read || written.Contains("goto Refuse;", StringComparison.Ordinal);
			var into = keep ? "$1" : "$2";

			// Each mark holds both: what records, then what does not.
			written = Regex.Replace(
				written, @"[ \t]*/\*exit\*/\r?\n(.*?)[ \t]*/\*else\*/\r?\n(.*?)[ \t]*/\*end\*/\r?\n", into,
				RegexOptions.Singleline | RegexOptions.CultureInvariant);
			written = Regex.Replace(written, @"/\*exit\*/(.*?)/\*else\*/(.*?)/\*end\*/", into, RegexOptions.Singleline | RegexOptions.CultureInvariant);

			// A walk nothing calls any more is not asked for.
			_reaches = written.Contains("Reach_DotGram", StringComparison.Ordinal);

			return written;
		}

		bool _character;
		bool _reaches;

		/// <summary>Whether anything written asked how far a literal run matched.</summary>
		public bool Reaches => _reaches;

		/// <param name="reports">
		/// Whether the scan hands back how far it read, matched or not, in <c>reachedAt</c>: where it
		/// gave input back to itself, it may match short of a place it read to and refused at.
		/// </param>
		public string Render(Node body, bool reports = false)
		{
			var code = new Writer(0);

			Emit(code, body, "Refuse");

			var written = Exits(Threaded(code.ToString()), reports || starves);
			var head    = new Writer(0);

			head.Line("var p = pos;");

			if (_character)
				head.Line("var c = '\\0';");

			// Only the locals the code actually reads: a mark reserved for a sequence
			// whose later parts turned out unable to fail is never touched, and an
			// assigned-never-read local is a warning in somebody else's build.
			for (var i = 0; i < _deepestMark; i++)
				if (written.Contains($"mark{i}", StringComparison.Ordinal))
				head.Line($"var mark{i} = 0;");

			for (var i = 0; i < _turns; i++)
				if (written.Contains($"turns{i}", StringComparison.Ordinal))
				head.Line($"var turns{i} = 0;");

			// How far the scan ever came, which is not where it ends up: a scanner gives
			// input back to itself as it tries the ways through, and what a refusal has to
			// report is the furthest it reached and not the place it happened to stop.
			// Declared where it is written, the same as every other local here.
			if (written.Contains("furthest", StringComparison.Ordinal))
				head.Line("var furthest = pos;");

			head.Line();
			head.Write(written);
			head.Line();

			// A scan that succeeded still has to say whether it was cut short, and until
			// this it did not: the furthest it reached was reported on a refusal and
			// thrown away on a match. So a rule reading `18.25` out of a window that ends
			// at `18.` matched `18`, and the caller's failure on the `.` that followed
			// looked like an ordinary mismatch one short of the end rather than what it
			// was — a record the window had cut in half. A stream then closed a repetition
			// that had not ended and refused the input at the next thing (docs/next.md).
			//
			// Only where something streams, because only a stream can do anything with the
			// answer, and asking costs a store on a path that is otherwise free.
			if (starves)
			{
				head.Line(
					written.Contains("furthest", StringComparison.Ordinal)
						? "if (p >= text.Length || furthest >= text.Length) failure.Starved = true;"
						: "if (p >= text.Length) failure.Starved = true;");
				head.Line();
			}

			if (reports)
				head.Line("reachedAt = furthest;");

			head.Line("return p;");

			if (written.Contains("goto Refuse;", StringComparison.Ordinal))
			{
				head.Line();
				head.Line("Refuse:");

				// And a refusal says it as the match does. A caller that is a look throws away how far the
				// scan reached — its failure is quiet and stands where the look began — so without this a
				// refusal at the end of a window read as the end of the input: the element was answered
				// rather than read again with more of the input in hand.
				if (written.Contains("furthest", StringComparison.Ordinal))
				{
					head.Line("if (p > furthest) furthest = p;");

					if (starves)
						head.Line("if (furthest >= text.Length) failure.Starved = true;");

					head.Line();

					if (reports)
						head.Line("reachedAt = furthest;");

					// Negated, so one value says both that it refused and where it reached,
					// and a caller wanting only the first can still ask `< 0`.
					head.Line("return -1 - furthest;");
				}
				else
				{
					if (starves)
						head.Line("if (p >= text.Length) failure.Starved = true;");

					head.Line("return -1 - p;");
				}
			}

			return head.ToString();
		}

		/// <param name="loaded">
		/// Whether <c>c</c> already holds <c>text[p]</c> and the position is proven in
		/// bounds — true right after a choice's front test, and carried only as far as
		/// nothing has consumed. What it saves is the first thing every alternative used
		/// to do over again: its own bounds check and its own read of the same character.
		/// </param>
		void Emit(Writer code, Node node, string fail, bool loaded = false)
		{
			switch (node)
			{
				case Node.Empty:
					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
				{
					if (text.Length == 0)
						break;

					if (loaded && !folded && text.Length == 1)
					{
						code.Line($"if (c != {CSharpEmitter.Char(text[0])}) goto {fail};");
						code.Line("p += 1;");

						break;
					}

					if (!folded && text.Length > 1)
					{
						// One comparison for the whole run, which is what it costs when it
						// matches. When it does not, how much of it did is worth knowing —
						// `"abqy"` and `"abcdefx"` both begin `ab`, and a message should name
						// the one that got furthest — and working that out on the path that
						// is about to refuse anyway costs the path that matches nothing.
						//
						// Inside a look the input ending inside the literal is asked first and on its own: the
						// look records nothing, and this is the one place that knows the scan ran out.
						if (starves && _lookingAhead > 0)
							code.Line($"if ({Short(text.Length)}) {RanOut(fail)}");

						code.Line(
							(starves && _lookingAhead > 0 ? "if (" : $"if ({Short(text.Length)} || ") +
							"!global::System.MemoryExtensions.SequenceEqual(" +
							$"text.Slice(p, {text.Length}), {Spanned(text)}))");

						// Where this refusal is the scan's answer or a sequence's way back, and at
						// a repetition's end — where the readers record the turn they tried — only
						// where the turn began to agree, the turn that does not begin being how
						// every such loop ends; and that only where something reads how far the
						// scan got (Exits): the seam's loops end at every operand.
						if (Records(fail))
						{
							_reaches = true;

							var exit = AgreesFirst(fail);

							if (exit)
								code.Line(ExitOpen);

							using (code.Block(""))
							{
								if (exit)
									code.Line($"if ((uint)p >= (uint)text.Length || text[p] != {CSharpEmitter.Char(text[0])}) goto {fail};");

								code.Line($"var reached = Reach_DotGram{tag}(text, p, {Spanned(text)});");
								code.Line("if (reached > furthest) furthest = reached;");
								code.Line($"goto {fail};");
							}

							if (exit)
							{
								code.Line(ExitElse);
								code.Then($"goto {fail};");
								code.Line(ExitClose);
							}
						}
						else
						{
							code.Then($"goto {fail};");
						}
					}
					else
					{
						// Input that ends inside the literal: the refusal reaches past every
						// character there that agrees, to the end where they all do.
						if (Records(fail) && text.Length > 1)
						{
							var exit = AgreesFirst(fail);

							if (exit)
								code.Line(ExitOpen);

							code.Line($"if ({Short(text.Length)})");

							using (code.Block(""))
							{
								code.Line("var reached = p;");
								Machine.ReachLadder(code, text, folded, "reached", "text[reached]", "(uint)reached < (uint)text.Length");
								code.Line(exit ? "if (reached > p && reached > furthest) furthest = reached;" : "if (reached > furthest) furthest = reached;");
								code.Line($"goto {fail};");
							}

							if (exit)
							{
								code.Line(ExitElse);
								code.Line($"if ({Short(text.Length)}) goto {fail};");
								code.Line(ExitClose);
							}
						}
						else
						{
							code.Line($"if ({Short(text.Length)}) {RanOut(fail)}");
						}

						for (var i = 0; i < text.Length; i++)
						{
							var test = CSharpEmitter.Differs($"text[p + {i}]", text[i], folded);

							// The offset is known here and no walk is needed: a run refusing
							// at its fourth character reached its fourth character.
							var records = $"if ({test}) {{ if (p + {i} > furthest) furthest = p + {i}; goto {fail}; }}";
							var plain   = $"if ({test}) goto {fail};";

							code.Line(
								i == 0 || !Records(fail) ? plain
								: AgreesFirst(fail) ? ExitOpen + records + ExitElse + plain + ExitClose
								: records);
						}
					}

					code.Line($"p += {text.Length};");

					break;
				}

				case Node.Element element:
				{
					var test = CSharpEmitter.Test(element, tabulate);

					if (!loaded)
						code.Line($"if ((uint)p >= (uint)text.Length) {RanOutHere(fail)}");

					// `any` tests nothing; reading the character just to discard it would
					// be an unreachable branch in somebody's build.
					if (!string.Equals(test, "true", StringComparison.Ordinal))
					{
						_character = true;

						if (!loaded)
							code.Line("c = text[p];");
						code.Line($"if (!({test})) goto {fail};");
					}

					code.Line("p++;");

					break;
				}

				case Node.Sequence(var parts):
				{
					// The first part may fail straight out, since p has not moved yet;
					// anything later has to put p back first. Whether anything later can
					// fail at all is only known once it is written, so the parts go into
					// a buffer and the undo machinery is added exactly when referenced.
					if (parts.Count == 1)
					{
						Emit(code, parts[0], fail, loaded);

						break;
					}

					var mark    = Mark();
					var restore = $"L{_labels++}_undo";
					var over    = $"L{_labels++}_on";
					var buffer  = new Writer(0);
					var carry   = loaded;

					_restores.Add(restore);

					for (var i = 0; i < parts.Count; i++)
					{
						if (parts[i] is Node.Repeat scan && GuardOf(scan) is { } guard)
						{
							carry = false;

							// Scannable admitted this repetition only as the pair, so the
							// literal it stops at is the part after it and the two are
							// emitted together — as one search where the body is a bare
							// character, and as loop plus literal where it is more.
							if (ScansUntil(scan, graph) is { } except)
							{
								EmitScanUntil(buffer, guard, except, i == 0 ? fail : restore);
								i++;

								continue;
							}

							EmitGuardedScan(buffer, scan);

							continue;
						}

						Emit(buffer, parts[i], i == 0 ? fail : restore, carry);

						// The invariant survives only what consumes nothing.
						carry = carry && parts[i] is Node.Empty or Node.Lookahead or Node.Behind or Node.Glue;
					}

					var written = buffer.ToString();

					var raises = written.Contains($"goto {restore};", StringComparison.Ordinal);
					var quiet  = written.Contains($"goto {restore}_q;", StringComparison.Ordinal);

					if (raises || quiet)
					{
						code.Line($"mark{mark} = p;");
						code.Write(written);
						code.Line($"goto {over};");

						if (raises)
						{
							code.Line($"{restore}:");
							code.Line("if (p > furthest) furthest = p;");
						}

						if (quiet)
							code.Line($"{restore}_q:");

						code.Line($"p = mark{mark};");
						code.Line($"goto {fail};");
						code.Line($"{over}: ;");
					}
					else
						code.Write(written);

					Unmark();

					break;
				}

				case Node.Choice(var alternatives):
				{
					var took = $"L{_labels++}_took";

					// One test for the whole chain, where every alternative must begin
					// with a character and the union of what they begin with is small
					// enough to write. The scanner's commonest answer is "no trivia
					// here", and without this that answer costs one test per alternative
					// — a bounds check and a span compare apiece for the two comment
					// forms, where the character says no to both at once. At the end of
					// the input it is the same test: nothing that must consume can.
					if (alternatives.Count > 1 && FrontTest(alternatives) is { } front)
					{
						_character = true;

						if (!loaded)
						{
							code.Line($"if ((uint)p >= (uint)text.Length) {RanOutHere(fail)}");
							code.Line("c = text[p];");
						}
						code.Line($"if (!({front})) goto {fail};");

						// The front test just proved both halves of the invariant for
						// every alternative: the position is in bounds and `c` is the
						// character standing there.
						loaded = true;
					}

					var taken = false;

					for (var i = 0; i < alternatives.Count; i++)
					{
						if (i == alternatives.Count - 1)
						{
							Emit(code, alternatives[i], fail, loaded);

							break;
						}

						var next   = $"L{_labels++}_or";
						var buffer = new Writer(0);

						_alternatives.Add(next);
						Emit(buffer, alternatives[i], next, loaded);

						// An alternative that cannot fail — `'b'?`, `X*` — is where the choice ends:
						// what is written after it is never reached, and C# says so with CS0162 in
						// a file the consumer did not write, an error under warnings-as-errors.
						if (!buffer.ToString().Contains($"goto {next};", StringComparison.Ordinal))
						{
							code.Write(buffer.ToString());

							break;
						}

						code.Write(buffer.ToString());
						code.Line($"goto {took};");
						code.Line($"{next}: ;");

						taken = true;
					}

					if (taken)
						code.Line($"{took}: ;");

					break;
				}

				case Node.Repeat repeat:
					EmitRepeat(code, repeat, fail);

					break;

				case Node.Lookahead(var positive, var inside):
				{
					var mark = Mark();

					code.Line($"mark{mark} = p;");

					// A pair left open inside a lookahead is not one the trivia opened: the
					// lookahead put the position back, and what the scan goes on to read decides.
					_lookingAhead++;

					if (positive)
					{
						var over = $"L{_labels++}_past";
						var seen = $"L{_labels++}_seen";

						Emit(code, inside, seen, loaded);
						// A lookahead's own advance is not distance covered: it looked and
						// put the position back. Counting it would have `eof` — which is
						// `?!any` — report a refusal one character past where the input
						// failed to end.
						code.Line($"p = mark{mark};");
						code.Line($"goto {over};");
						code.Line($"{seen}:");
						code.Line($"goto {Quiet(fail)};");
						code.Line($"{over}: ;");
					}
					else
					{
						var absent = $"L{_labels++}_absent";

						Emit(code, inside, absent, loaded);
						code.Line($"p = mark{mark};");
						code.Line($"goto {Quiet(fail)};");
						code.Line($"{absent}: ;");
					}

					_lookingAhead--;

					Unmark();

					break;
				}

				// Only ever reached over characters (see `Scannable`), where the seam was
				// never woven and adjacency is what the absence of one already says.
				case Node.Glue:
					break;

				case Node.Behind(var boundary):
					_character = true;

					code.Line($"if (p > 0)");
					using (code.Block(""))
					{
						code.Line("c = text[p - 1];");
						code.Line($"if ({CSharpEmitter.Test(boundary, tabulate)}) goto {fail};");
					}

					break;

				case Node.Atomic(var kept):
					Emit(code, kept, fail, loaded);

					break;

				case Node.Call(var called, _) when NestedPair.Is(called) && NestedPair.Of(called, graph) is { } pair:
					EmitNested(code, pair, fail, loaded);

					break;

				case Node.Call(var called, _):
					Emit(code, graph.Bodies[called], fail, loaded);

					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed Scannable but has no scan emission.");
			}
		}

		/// <summary>
		/// Std.Nested, read with its counter: <c>open</c> as any literal is, then the loop to the
		/// <c>close</c> that balances it (NestedPair). A pair the text ends inside of is refused
		/// where it opened, having reached the end — where the rule's own reading reaches too.
		/// </summary>
		void EmitNested(Writer code, NestedPair.Delimiters pair, string fail, bool loaded)
		{
			var mark = Mark();

			code.Line($"mark{mark} = p;");
			Emit(code, new Node.Literal(pair.Open) { IgnoreCase = pair.OpenFolds }, fail, loaded);

			using (code.Block(""))
			{
				NestedPair.WriteRest(code, pair, "p", _labels++.ToString(System.Globalization.CultureInfo.InvariantCulture),
				[
					"if (text.Length > furthest) furthest = text.Length;",
					.. recordsRunOut && _lookingAhead == 0 ? (string[])[$"if (mark{mark} > ranOut) ranOut = mark{mark};"] : [],
					$"p = mark{mark};",
					$"goto {fail};",
				]);
			}

			Unmark();
		}

		void EmitRepeat(Writer code, Node.Repeat repeat, string fail)
		{
			var (body, min, max) = repeat;

			// A run that stops only at one of a few characters is a search for them, as the
			// engine's and the reader's are (Machine.EmitSearch): a field's value in FIX, a
			// string's body.
			if (max is null && stops?.Invoke(body) is { } stop)
			{
				var from  = $"from{_labels}";
				var found = $"found{_labels++}";

				if (min > 0)
					code.Line($"var {from} = p;");

				code.Line(
					$"var {found} = " + (stop.Length switch
					{
						1    => $"global::System.MemoryExtensions.IndexOf(text.Slice(p), {CSharpEmitter.Char(stop[0])});",
						<= 3 => $"global::System.MemoryExtensions.IndexOfAny(text.Slice(p), {string.Join(", ", stop.Select(CSharpEmitter.Char))});",
						_    => $"global::System.MemoryExtensions.IndexOfAny(text.Slice(p), global::System.MemoryExtensions.AsSpan({CSharpEmitter.Quoted(new string(stop))}));",
					}));
				// Where the scanner has to say that it ran into the end, it says so as the loop did:
				// a search that finds nothing stops at the end, and the loop raised nothing else.
				code.Line($"p = {found} < 0 ? text.Length : p + {found};");

				// Short of the least, the run read characters it now gives back: what is tried
				// next begins where the run did.
				if (min > 1 && GivesBackTo(fail))
				{
					code.Line($"if (p - {from} < {min})");

					using (code.Block(""))
					{
						code.Line("if (p > furthest) furthest = p;");
						code.Line($"p = {from};");
						code.Line($"goto {fail};");
					}
				}
				else if (min > 0)
					code.Line($"if (p - {from} < {min}) goto {fail};");

				return;
			}
			var loop    = $"L{_labels++}_turn";
			var done    = $"L{_labels++}_done";
			var counted = min > 0 || max is not null;
			var turn    = counted ? _turns++ : -1;

			// A run that needs two turns or more can fail after reading some: `'b'{2}` on `by`
			// reads the `b`, and whatever is tried next has to begin where the run began. Where
			// it fails into a sequence's own way back, that puts the position further back still.
			var mark = min > 1 && GivesBackTo(fail) ? Mark() : -1;

			if (mark >= 0)
				code.Line($"mark{mark} = p;");

			if (counted)
				code.Line($"turns{turn} = 0;");

			code.Line($"{loop}:");

			if (max is { } limit)
				code.Line($"if (turns{turn} >= {limit}) goto {done};");

			_exits.Add(done);
			Emit(code, body, done);

			if (counted)
				code.Line($"turns{turn}++;");

			code.Line($"goto {loop};");
			code.Line($"{done}:");

			if (mark >= 0)
			{
				code.Line($"if (turns{turn} < {min})");

				using (code.Block(""))
				{
					code.Line("if (p > furthest) furthest = p;");
					code.Line($"p = mark{mark};");
					code.Line($"goto {fail};");
				}

				Unmark();
			}
			else if (min > 0)
				code.Line($"if (turns{turn} < {min}) goto {fail};");
			else
				code.Line(";");
		}

		/// <summary>
		/// <c>(?!X &amp; …)* &amp; …</c>: run turns until the guard holds, and never
		/// fail — where the scan stops is for what follows to judge.
		/// </summary>
		void EmitGuardedScan(Writer code, Node.Repeat repeat)
		{
			var loop = $"L{_labels++}_scan";
			var done = $"L{_labels++}_done";

			code.Line($"{loop}:");
			Emit(code, repeat.Body, done);
			code.Line($"goto {loop};");
			code.Line($"{done}: ;");
		}

		/// <summary>
		/// <c>(?!L &amp; any)* &amp; L</c> — the two parts together, as the search they
		/// are: find the first <c>L</c>, and take it.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Written out, the pair reads the delimiter twice at every position: once as the
		/// guard, refusing the turn, and once more as the literal after the loop, where
		/// the turn that refused already proved it. Between them the machinery of a turn
		/// — a mark taken, a rewind, a character consumed — runs per character of the
		/// comment, string, or section this is scanning through.
		/// </para>
		/// <para>
		/// What the pair means is one search, and the runtime's own is vectorized where
		/// this loop could not be. The semantics carried over exactly: found is the first
		/// occurrence, which is where the guard would first have held; not found is the
		/// literal failing after a scan to the end, which is the pair failing.
		/// </para>
		/// <para>
		/// Where the turn is <c>?!L &amp; [^ S]</c>, the turns stop at a character of <c>S</c> as
		/// well, where the turn's own item refuses: the loop ends there and <c>L</c> after it fails,
		/// unless <c>L</c> begins there. So the pair is the first <c>L</c> where no character of
		/// <c>S</c> stands before it, and two searches: one for <c>L</c>, and one for <c>S</c> in what
		/// it passed over, or in all of the rest where there is no <c>L</c>. A character of <c>S</c>
		/// where <c>L</c> begins is not before it, which is right when <c>L</c> begins with a
		/// character of <c>S</c>. A refusal reaches where the loop stopped, as the loop's does.
		/// </para>
		/// </remarks>
		void EmitScanUntil(Writer code, string delimiter, char[] except, string fail)
		{
			var found = $"found{_founds++}";

			code.Line(
				$"var {found} = global::System.MemoryExtensions.IndexOf(text.Slice(p), " +
				$"{Spanned(delimiter)});");

			// Not found is the loop reading to the end and the literal after it running out there.
			if (except.Length == 0)
			{
				code.Line(_lookingAhead == 0
					? $"if ({found} < 0) {{ if (text.Length > furthest) furthest = text.Length; goto {fail}; }}"
					: $"if ({found} < 0) {RanOut(fail)}");
				code.Line($"p += {found} + {delimiter.Length};");

				return;
			}

			// Where the loop ends short of L, its last turn refused there and so did L after it: the
			// furthest the scanner reached is that place, a character of S or the end, as the loop's
			// own refusals record it. The turns before it refused nothing a scanner records.
			var stop   = $"stop{_founds - 1}";
			var within = $"{found} < 0 ? text.Slice(p) : text.Slice(p, {found})";

			code.Line($"var {stop} = " + (except.Length switch
			{
				1    => $"global::System.MemoryExtensions.IndexOf({within}, {CSharpEmitter.Char(except[0])});",
				<= 3 => $"global::System.MemoryExtensions.IndexOfAny({within}, {string.Join(", ", except.Select(CSharpEmitter.Char))});",
				_    => $"global::System.MemoryExtensions.IndexOfAny({within}, global::System.MemoryExtensions.AsSpan({CSharpEmitter.Quoted(new string(except))}));",
			}));
			//
			// At the character of S the loop stops at, L is read and refused: past what of it agrees
			// there, which is the end where the input ends inside it. Not inside a look, as above.
			if (_lookingAhead == 0)
			{
				code.Line($"if ({stop} >= 0)");

				using (code.Block(""))
				{
					code.Line($"var reached = p + {stop};");
					Machine.ReachLadder(code, delimiter, false, "reached", "text[reached]", "(uint)reached < (uint)text.Length");
					code.Line("if (reached > furthest) furthest = reached;");
					code.Line($"goto {fail};");
				}

				code.Line($"if ({found} < 0) {{ if (text.Length > furthest) furthest = text.Length; goto {fail}; }}");
			}
			else
			{
				code.Line($"if ({stop} >= 0) goto {fail};");
				code.Line($"if ({found} < 0) {RanOut(fail)}");
			}

			code.Line($"p += {found} + {delimiter.Length};");
		}

		int _founds;

		/// <summary>
		/// The one character test that stands for a whole choice, or null where the
		/// alternatives do not all have to begin with one.
		/// </summary>
		FirstSets.First? Front(IReadOnlyList<Node> alternatives)
		{
			var union = FirstSets.First.None;

			foreach (var alternative in alternatives)
			{
				if (FirstSets.Nullable(alternative, graph))
					return null;

				union = union.Or(FirstSets.Of(alternative, graph));
			}

			return union;
		}

		string? FrontTest(IReadOnlyList<Node> alternatives)
		{
			return Front(alternatives) is { Anything: false, Nothing: false, Ends: false } union &&
			union.Ranges.Count is > 0 and <= Emitted
				? ranges(union.Ranges)
				: null;
		}

		int Mark()
		{
			var mark = _marks++;

			_deepestMark = Math.Max(_deepestMark, _marks);

			return mark;
		}

		void Unmark()
		{
			_marks--;
		}

		/// <summary>
		/// The jump threading the emission cannot do for itself: a jump to a label whose
		/// whole block is another jump goes where that jump goes, a jump to the label it
		/// falls into anyway disappears, and a label nothing jumps to any more goes with
		/// it. Emission is compositional — a choice does not know its taken-exit falls
		/// straight into the loop's own back-edge — so the seams it leaves are threaded
		/// here, over the finished text, where following a chain is following lines.
		/// </summary>
		internal static string Threaded(string written)
		{
			var lines = new List<string>(written.Split('\n'));

			for (var i = 0; i < lines.Count; i++)
				lines[i] = lines[i].TrimEnd('\r');

			// Where each label resolves: past empty statements and other labels, and
			// through an unconditional jump, to the label whose block does something.
			string Resolve(string label, HashSet<string> walked)
			{
				if (!walked.Add(label))
					return label;

				for (var i = 0; i < lines.Count; i++)
				{
					if (!IsLabel(lines[i], out var name, out var rest) || name != label)
						continue;

					var first = rest.Trim();

					for (var j = i + 1; first.Length == 0 || first == ";"; j++)
					{
						if (j >= lines.Count)
							return label;

						first = IsLabel(lines[j], out _, out var beyond)
							? beyond.Trim()
							: lines[j].Trim();
					}

					return first.StartsWith("goto ", StringComparison.Ordinal) &&
						first.EndsWith(";", StringComparison.Ordinal)
							? Resolve(first["goto ".Length..^1].Trim(), walked)
							: label;
				}

				return label;
			}

			// Thread every jump to where its target resolves.
			for (var i = 0; i < lines.Count; i++)
			{
				var trimmed = lines[i].Trim();

				if (!trimmed.StartsWith("goto ", StringComparison.Ordinal) ||
					!trimmed.EndsWith(";", StringComparison.Ordinal))
					continue;

				var target   = trimmed["goto ".Length..^1].Trim();
				var resolved = Resolve(target, []);

				if (resolved != target)
					lines[i] = lines[i].Replace($"goto {target};", $"goto {resolved};");
			}

			for (var pass = 0; pass < 2; pass++)
			{
			// A jump to the label it falls into anyway says nothing.
			for (var i = 0; i < lines.Count; i++)
			{
				var trimmed = lines[i].Trim();

				if (!trimmed.StartsWith("goto ", StringComparison.Ordinal) ||
					!trimmed.EndsWith(";", StringComparison.Ordinal))
					continue;

				var target = trimmed["goto ".Length..^1].Trim();

				for (var j = i + 1; j < lines.Count; j++)
				{
					var next = lines[j].Trim();

					if (next.Length == 0 || next == ";")
						continue;

					if (IsLabel(lines[j], out var name, out var rest))
					{
						if (name == target)
						{
							lines[i] = "";

							break;
						}

						if (string.IsNullOrWhiteSpace(rest) || rest.Trim() == ";")
							continue;
					}

					break;
				}
			}

			// A label nothing jumps to goes, and takes its empty statement with it.
			var referenced = new HashSet<string>(StringComparer.Ordinal);

			foreach (var line in lines)
			{
				var at = 0;

				while ((at = line.IndexOf("goto ", at, StringComparison.Ordinal)) >= 0)
				{
					var end = line.IndexOf(';', at);

					if (end < 0)
						break;

					referenced.Add(line[(at + "goto ".Length)..end].Trim());
					at = end + 1;
				}
			}

			for (var i = 0; i < lines.Count; i++)
				if (IsLabel(lines[i], out var name, out var rest) &&
					name != "Refuse" &&
					!referenced.Contains(name))
					lines[i] = string.IsNullOrWhiteSpace(rest) || rest.Trim() == ";" ? "" : rest;

			// A jump right after an unconditional jump, with no label between, is dead —
			// the seam a removed label leaves behind — and unreachable code is a warning
			// in somebody else's build. A jump that is the branch of a two-line `if` is
			// conditional, whatever its own line says.
			var falling   = true;
			var dependent = false;

			for (var i = 0; i < lines.Count; i++)
			{
				var trimmed = lines[i].Trim();

				if (trimmed.Length == 0 || trimmed == ";")
					continue;

				if (IsLabel(lines[i], out _, out var rest) &&
					(string.IsNullOrWhiteSpace(rest) || rest.Trim() == ";"))
				{
					falling = true;

					continue;
				}

				if (dependent)
				{
					dependent = false;
					falling   = true;

					continue;
				}

				if (trimmed.StartsWith("if ", StringComparison.Ordinal) &&
					!trimmed.EndsWith(";", StringComparison.Ordinal))
				{
					dependent = true;
					falling   = true;

					continue;
				}

				if (!falling && trimmed.StartsWith("goto ", StringComparison.Ordinal) &&
					trimmed.EndsWith(";", StringComparison.Ordinal))
				{
					lines[i] = "";

					continue;
				}

				falling = !trimmed.StartsWith("goto ", StringComparison.Ordinal) &&
					!trimmed.StartsWith("return ", StringComparison.Ordinal);
			}

			}

			var kept = new List<string>(lines.Count);

			foreach (var line in lines)
				if (line.Length > 0)
					kept.Add(line);

			return string.Join("\r\n", kept) + "\r\n";
		}

		internal static bool IsLabel(string line, out string name, out string rest)
		{
			name = "";
			rest = "";

			var trimmed = line.TrimStart('\t', ' ');
			var colon   = trimmed.IndexOf(':');

			if (colon <= 0 || trimmed.Contains("goto ", StringComparison.Ordinal) &&
				trimmed.IndexOf("goto ", StringComparison.Ordinal) < colon)
				return false;

			// `global::System` is a name, not a label: a label's colon stands alone.
			if (colon + 1 < trimmed.Length && trimmed[colon + 1] == ':')
				return false;

			var candidate = trimmed[..colon];

			foreach (var symbol in candidate)
				if (!char.IsLetterOrDigit(symbol) && symbol != '_')
					return false;

			// `case 3:` is a label to C# but not to this pass; nothing here emits
			// switches, and a name that is a keyword or starts with a digit is not ours.
			if (candidate.Length == 0 || char.IsDigit(candidate[0]) || candidate == "default")
				return false;

			name = candidate;
			rest = trimmed[(colon + 1)..];

			return true;
		}
	}
}
