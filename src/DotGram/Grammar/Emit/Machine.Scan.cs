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
				? "Scan_" + CSharpEmitter.IdentifierOf(rule)
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

			var written = code.ToString();
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

		void Emit(Writer code, Node node, string fail)
		{
			switch (node)
			{
				case Node.Empty:
					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
				{
					if (text.Length == 0)
						break;

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

					code.Line($"if ((uint)p >= (uint)text.Length) goto {fail};");

					// `any` tests nothing; reading the character just to discard it would
					// be an unreachable branch in somebody's build.
					if (!string.Equals(test, "true", StringComparison.Ordinal))
					{
						_character = true;

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
						Emit(code, parts[0], fail);

						break;
					}

					var mark    = Mark();
					var restore = $"L{_labels++}_undo";
					var over    = $"L{_labels++}_on";
					var buffer  = new Writer(0);

					for (var i = 0; i < parts.Count; i++)
					{
						if (parts[i] is Node.Repeat scan && GuardOf(scan) is not null)
							EmitGuardedScan(buffer, scan);
						else
							Emit(buffer, parts[i], i == 0 ? fail : restore);
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

					for (var i = 0; i < alternatives.Count; i++)
					{
						if (i == alternatives.Count - 1)
						{
							Emit(code, alternatives[i], fail);

							break;
						}

						var next = $"L{_labels++}_or";

						Emit(code, alternatives[i], next);
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

						Emit(code, inside, seen);
						code.Line($"p = mark{mark};");
						code.Line($"goto {over};");
						code.Line($"{seen}:");
						code.Line($"goto {fail};");
						code.Line($"{over}: ;");
					}
					else
					{
						var absent = $"L{_labels++}_absent";

						Emit(code, inside, absent);
						code.Line($"p = mark{mark};");
						code.Line($"goto {fail};");
						code.Line($"{absent}: ;");
					}

					Unmark();

					break;
				}

				case Node.Atomic(var kept):
					Emit(code, kept, fail);

					break;

				case Node.Call(var called, _):
					Emit(code, graph.Bodies[called], fail);

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

		int Mark()
		{
			var mark = _marks++;

			_deepestMark = Math.Max(_deepestMark, _marks);

			return mark;
		}

		void Unmark() => _marks--;
	}
}
