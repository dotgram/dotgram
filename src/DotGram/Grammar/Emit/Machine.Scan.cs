using System;
using System.Collections.Generic;

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
		if (_scanners.TryGetValue(rule, out var known))
			return known;

		var name =
			_graph.Bodies.TryGetValue(rule, out var body) &&
			body is Node.Atomic(var kept) &&
			!KeepsRecords(kept) &&
			Scannable(kept, FirstSets.First.None)
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

	/// <summary>Every scanner the compiled states call, rendered as methods.</summary>
	public string RenderScanners()
	{
		var file = new Writer(0);

		foreach (var pair in _scanners)
		{
			if (pair.Value is not { } name || _graph.Bodies[pair.Key] is not Node.Atomic(var body))
				continue;

			var rule = pair.Key;

			var scan  = new ScanWriter(_graph);
			var inner = scan.Render(body);

			file.Line($"/// <summary><c>{rule.Name}</c>, recognized with nothing written down.</summary>");

			using (file.Block(
				$"static int {name}(global::System.ReadOnlySpan<char> text, int pos)"))
			{
				file.Write(inner);
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
						if (scan.Min != 0 ||
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

					next = FollowSets.Plainly(parts[i], next, _graph);
				}

				return true;
			}

			case Node.Choice(var alternatives):
			{
				for (var i = 0; i < alternatives.Count; i++)
					for (var j = i + 1; j < alternatives.Count; j++)
						if (!Exclusive(alternatives[i], alternatives[j]))
							return false;

				foreach (var alternative in alternatives)
					if (!Scannable(alternative, after, seen))
						return false;

				return true;
			}

			case Node.Repeat(var body, _, _):
				return
					!FirstSets.Nullable(body, _graph) &&
					(after.Nothing || !FirstSets.Of(body, _graph).Overlaps(after)) &&
					Scannable(body, after.Nothing ? after : FirstSets.Of(body, _graph).Or(after), seen);

			case Node.Lookahead(_, var inside):
				return Scannable(inside, FirstSets.First.All, seen);

			case Node.Behind:
				return true;

			case Node.Atomic(var kept):
				return Scannable(kept, after, seen);

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
	/// Whether a guarded scan's turn is a bare character, so the pair it belongs to is a
	/// search for its delimiter and nothing else.
	/// </summary>
	/// <remarks>
	/// A turn that consumes anything at all — <c>?!"*&#47;" &amp; any</c> — stops exactly
	/// where the delimiter first occurs. A turn narrower than that — <c>?!X &amp;
	/// [^ '\n']</c> — can also stop because its own character test refused, which a
	/// search for the delimiter would run straight past, so that shape keeps the loop.
	/// </remarks>
	static bool ScansUntil(Node.Repeat repeat, RecognitionGraph graph) =>
		repeat.Body is Node.Sequence({ Count: 2 } parts) && Anything(parts[1], graph, []);

	/// <summary>Whether a node is one character, whichever character it is.</summary>
	/// <remarks>
	/// <c>any</c> is a rule of the standard library rather than an element written in
	/// place, so the call is followed — the same unwrapping every other analysis here
	/// does, with a ring of calls refused rather than walked forever.
	/// </remarks>
	static bool Anything(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen) =>
		node switch
		{
			Node.Element element     => string.Equals(
			                                CSharpEmitter.Test(element), "true", StringComparison.Ordinal),
			Node.Atomic(var kept)    => Anything(kept, graph, seen),
			Node.Call(var called, _) => seen.Add(called) &&
			                            graph.Bodies.TryGetValue(called, out var body) &&
			                            Anything(body, graph, seen),
			_                        => false,
		};

	/// <summary>The literal a guarded scan stops at, where the repetition is one.</summary>
	static string? GuardOf(Node.Repeat repeat) =>
		repeat.Body is Node.Sequence(var parts) &&
		parts.Count >= 1 &&
		parts[0] is Node.Lookahead(false, Node.Literal(var guard) { IgnoreCase: false })
			? guard
			: null;

	/// <summary>Whether at most one of the two can match at any one position.</summary>
	bool Exclusive(Node one, Node other)
	{
		if (!FirstSets.Of(one, _graph).Overlaps(FirstSets.Of(other, _graph)))
			return true;

		return LeadingLiteral(one) is { } mine && LeadingLiteral(other) is { } theirs &&
			Differ(mine, theirs);
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

	string? LeadingLiteral(Node node) => node switch
	{
		Node.Literal(var text) { IgnoreCase: false } => text,
		Node.Sequence(var parts) when parts.Count > 0 => LeadingLiteral(parts[0]),
		Node.Call(var called, _) when _graph.Bodies.TryGetValue(called, out var body)
			=> LeadingLiteral(body),
		_ => null,
	};

	/// <summary>
	/// The checkpoint emitter. Every node's code either falls through with <c>p</c>
	/// advanced past it, or jumps to its fail label with <c>p</c> exactly where it was.
	/// </summary>
	sealed class ScanWriter(RecognitionGraph graph)
	{
		int _labels;
		int _marks;
		int _deepestMark;
		int _turns;
		bool _character;

		public string Render(Node body)
		{
			var code = new Writer(0);

			Emit(code, body, "Refuse");

			var written = Threaded(code.ToString());
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

			head.Line();
			head.Write(written);
			head.Line();
			head.Line("return p;");

			if (written.Contains("goto Refuse;", StringComparison.Ordinal))
			{
				head.Line();
				head.Line("Refuse:");
				head.Line("return -1;");
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
						code.Line(
							$"if (p + {text.Length} > text.Length || " +
							"!global::System.MemoryExtensions.SequenceEqual(" +
							$"text.Slice(p, {text.Length}), {Spanned(text)}))");
						code.Then($"goto {fail};");
					}
					else
					{
						code.Line($"if (p + {text.Length} > text.Length) goto {fail};");

						for (var i = 0; i < text.Length; i++)
						{
							var read = folded
								? $"global::System.Char.ToUpperInvariant(text[p + {i}])"
								: $"text[p + {i}]";
							var want = CSharpEmitter.Char(folded ? char.ToUpperInvariant(text[i]) : text[i]);

							code.Line($"if ({read} != {want}) goto {fail};");
						}
					}

					code.Line($"p += {text.Length};");

					break;
				}

				case Node.Element element:
				{
					var test = CSharpEmitter.Test(element);

					if (!loaded)
						code.Line($"if ((uint)p >= (uint)text.Length) goto {fail};");

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

					for (var i = 0; i < parts.Count; i++)
					{
						if (parts[i] is Node.Repeat scan && GuardOf(scan) is { } guard)
						{
							carry = false;

							// Scannable admitted this repetition only as the pair, so the
							// literal it stops at is the part after it and the two are
							// emitted together — as one search where the body is a bare
							// character, and as loop plus literal where it is more.
							if (ScansUntil(scan, graph))
							{
								EmitScanUntil(buffer, guard, i == 0 ? fail : restore);
								i++;

								continue;
							}

							EmitGuardedScan(buffer, scan);

							continue;
						}

						Emit(buffer, parts[i], i == 0 ? fail : restore, carry);

						// The invariant survives only what consumes nothing.
						carry = carry && parts[i] is Node.Empty or Node.Lookahead or Node.Behind;
					}

					var written = buffer.ToString();

					if (written.Contains($"goto {restore};", StringComparison.Ordinal))
					{
						code.Line($"mark{mark} = p;");
						code.Write(written);
						code.Line($"goto {over};");
						code.Line($"{restore}:");
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
							code.Line($"if ((uint)p >= (uint)text.Length) goto {fail};");
							code.Line("c = text[p];");
						}
						code.Line($"if (!({front})) goto {fail};");

						// The front test just proved both halves of the invariant for
						// every alternative: the position is in bounds and `c` is the
						// character standing there.
						loaded = true;
					}

					for (var i = 0; i < alternatives.Count; i++)
					{
						if (i == alternatives.Count - 1)
						{
							Emit(code, alternatives[i], fail, loaded);

							break;
						}

						var next = $"L{_labels++}_or";

						Emit(code, alternatives[i], next, loaded);
						code.Line($"goto {took};");
						code.Line($"{next}: ;");
					}

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

					if (positive)
					{
						var over = $"L{_labels++}_past";
						var seen = $"L{_labels++}_seen";

						Emit(code, inside, seen, loaded);
						code.Line($"p = mark{mark};");
						code.Line($"goto {over};");
						code.Line($"{seen}:");
						code.Line($"goto {fail};");
						code.Line($"{over}: ;");
					}
					else
					{
						var absent = $"L{_labels++}_absent";

						Emit(code, inside, absent, loaded);
						code.Line($"p = mark{mark};");
						code.Line($"goto {fail};");
						code.Line($"{absent}: ;");
					}

					Unmark();

					break;
				}

				case Node.Behind(var boundary):
					_character = true;

					code.Line($"if (p > 0)");
					using (code.Block(""))
					{
						code.Line("c = text[p - 1];");
						code.Line($"if ({CSharpEmitter.Test(boundary)}) goto {fail};");
					}

					break;

				case Node.Atomic(var kept):
					Emit(code, kept, fail, loaded);

					break;

				case Node.Call(var called, _):
					Emit(code, graph.Bodies[called], fail, loaded);

					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed Scannable but has no scan emission.");
			}
		}

		void EmitRepeat(Writer code, Node.Repeat repeat, string fail)
		{
			var (body, min, max) = repeat;
			var loop    = $"L{_labels++}_turn";
			var done    = $"L{_labels++}_done";
			var counted = min > 0 || max is not null;
			var turn    = counted ? _turns++ : -1;

			if (counted)
				code.Line($"turns{turn} = 0;");

			code.Line($"{loop}:");

			if (max is { } limit)
				code.Line($"if (turns{turn} >= {limit}) goto {done};");

			Emit(code, body, done);

			if (counted)
				code.Line($"turns{turn}++;");

			code.Line($"goto {loop};");
			code.Line($"{done}:");

			if (min > 0)
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
		/// </remarks>
		void EmitScanUntil(Writer code, string delimiter, string fail)
		{
			var found = $"found{_founds++}";

			code.Line(
				$"var {found} = global::System.MemoryExtensions.IndexOf(text.Slice(p), " +
				$"{Spanned(delimiter)});");
			code.Line($"if ({found} < 0) goto {fail};");
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

		string? FrontTest(IReadOnlyList<Node> alternatives) =>
			Front(alternatives) is { Anything: false, Nothing: false, Ends: false } union &&
			union.Ranges.Count is > 0 and <= Emitted
				? RangesTest(union.Ranges)
				: null;

		int Mark()
		{
			var mark = _marks++;

			_deepestMark = Math.Max(_deepestMark, _marks);

			return mark;
		}

		void Unmark() => _marks--;

		/// <summary>
		/// The jump threading the emission cannot do for itself: a jump to a label whose
		/// whole block is another jump goes where that jump goes, a jump to the label it
		/// falls into anyway disappears, and a label nothing jumps to any more goes with
		/// it. Emission is compositional — a choice does not know its taken-exit falls
		/// straight into the loop's own back-edge — so the seams it leaves are threaded
		/// here, over the finished text, where following a chain is following lines.
		/// </summary>
		static string Threaded(string written)
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

		static bool IsLabel(string line, out string name, out string rest)
		{
			name = "";
			rest = "";

			var trimmed = line.TrimStart('\t', ' ');
			var colon   = trimmed.IndexOf(':');

			if (colon <= 0 || trimmed.Contains("goto ", StringComparison.Ordinal) &&
				trimmed.IndexOf("goto ", StringComparison.Ordinal) < colon)
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
