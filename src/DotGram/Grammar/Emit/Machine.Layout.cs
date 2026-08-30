using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What the state table looks like once it is written out, which is not the order it was
/// built in.
/// </summary>
/// <remarks>
/// Compilation reserves a state whenever it needs somewhere to come back to and numbers
/// them as it goes, so the table it leaves behind holds states nothing can reach and states
/// that do nothing but point at another. Deciding what is written, and in what order, is a
/// pass of its own over the finished table — separate from building it, and separate here
/// for the same reason.
/// </remarks>
sealed partial class Machine
{
	// ── Layout (§the state table, as it is finally written) ─────────────────────────

	/// <summary>Each state's text, once every jump in it has been followed to its end.</summary>
	string[] _bodies = [];

	/// <summary>Where a state really goes, for a state that does nothing but go somewhere.</summary>
	int[] _resolved = [];

	/// <summary>The order the states are written in, and which of them are written at all.</summary>
	List<int> _order = [];

	/// <summary>
	/// Decides what the state table looks like once it is written out, which is not the
	/// order it was built in.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Compilation reserves a state whenever it needs somewhere to come back to, and numbers
	/// them as it goes. That leaves two kinds of waste in the text. A state whose whole body
	/// is <c>goto</c> somewhere else is a signpost standing where the road could have gone
	/// directly: it costs a slot in the dispatch table, a label, and a branch. And a state
	/// that ends by jumping to another is written nowhere near it, so the jump is a jump
	/// rather than the next line.
	/// </para>
	/// <para>
	/// Both are decided here, before a character is written. Signposts are followed to
	/// wherever they end and then not written at all, and everything that pointed at one —
	/// a <c>goto</c>, a resume point recorded in the arena, a case of the dispatch — is made
	/// to point where it was really going. What is left is laid out in chains, each state
	/// followed by the one it jumps to where that one is still unplaced, and the jump at the
	/// end of a chained state is dropped: the next line is already where it was going.
	/// </para>
	/// <para>
	/// A jitted method has budgets — for how much it will look at, and how hard — and this
	/// is a generator that inlines freely. Text that says nothing is worth removing before
	/// those budgets are spent on reading it.
	/// </para>
	/// </remarks>
	void PlanLayout()
	{
		var signposts = new int?[_states.Count];

		_bodies = new string[_states.Count];
		_resumed.Clear();

		for (var i = 0; i < _states.Count; i++)
		{
			_bodies[i]   = _states[i].ToString();
			signposts[i] = JumpOnly(_bodies[i]);
		}

		// Follow each chain of signposts to its end. The guard is against a grammar whose
		// states point round in a circle, which nothing should produce and which would
		// otherwise not terminate.
		_resolved = new int[_states.Count];

		for (var i = 0; i < _states.Count; i++)
		{
			var at    = i + First;
			var steps = 0;

			while (at - First is var index and >= 0 &&
				index < signposts.Length &&
				signposts[index] is { } onward &&
				steps++ <= signposts.Length)
			{
				at = onward;
			}

			_resolved[i] = at;
		}

		for (var i = 0; i < _bodies.Length; i++)
			_bodies[i] = Redirect(_bodies[i]);

		// What is left is what can still be got to. A rule compiled into every one of its
		// callers is called from nowhere, and its own copy — entry, body and all — is text
		// nothing will ever reach. So is a signpost, now that everything which pointed at one
		// points past it.
		var reachable = new bool[_states.Count];
		var pending   = new Stack<int>();

		foreach (var root in _roots)
			pending.Push(Resolved(root));

		// Nothing said where the parse begins: keep everything rather than guess.
		if (_roots.Count == 0)
			for (var i = 0; i < _states.Count; i++)
				pending.Push(i + First);

		while (pending.Count > 0)
		{
			var index = pending.Pop() - First;

			// A signpost is never written: everything that pointed at one now points past it,
			// so its block would be text nothing can reach — which the C# compiler says out
			// loud, and rightly.
			if (index < 0 || index >= reachable.Length || reachable[index] || signposts[index] is not null)
				continue;

			reachable[index] = true;

			// What the sites that wrote this state said about where it can go. Only a
			// resumable kind was ever recorded as a resume: the others name a capture slot
			// or a factory, and there is nothing here to mistake one for the other.
			var edges = Recorded(index);

			foreach (var target in edges.Jumps)
				pending.Push(Resolved(target));

			foreach (var target in edges.Resumes)
			{
				var resumed = Resolved(target);

				_resumed.Add(resumed);
				pending.Push(resumed);
			}
		}

		// Written in the order the parse runs them, not the order they were compiled in.
		// Compilation is continuation-passing — what a state jumps to is compiled before the
		// state that jumps to it — so ascending index order is very nearly *reverse*
		// execution order. It reads backwards, and worse, a state's trailing jump almost
		// never names the state written next, which is the one case `RenderStates` can drop
		// the jump for. Following each chain to its end before starting another puts them
		// side by side and the jumps between them go.
		_order = new List<int>(_states.Count);

		var placed  = new bool[_states.Count];
		var waiting = new Stack<int>();

		foreach (var root in _roots.OrderByDescending(static state => state))
			waiting.Push(Resolved(root));

		if (_roots.Count == 0)
			for (var i = _states.Count - 1; i >= 0; i--)
				waiting.Push(i + First);

		while (waiting.Count > 0)
		{
			var at = waiting.Pop() - First;

			while (at >= 0 && at < placed.Length && reachable[at] && !placed[at])
			{
				placed[at] = true;
				_order.Add(at);

				// The one it ends by jumping to is where the chain goes on; everything else
				// it can reach is a chain of its own, started once this one runs out.
				var tail = Tail(_bodies[at]) is { } ends ? Resolved(ends) : (int?)null;

				var edges = Recorded(at);

				foreach (var jump in edges.Jumps)
					if (Resolved(jump) is var target && target != tail)
						waiting.Push(target);

				foreach (var resume in edges.Resumes)
					waiting.Push(Resolved(resume));

				at = tail is { } onward ? onward - First : -1;
			}
		}

		// Anything reachable the walk did not thread — nothing should be, and a state left
		// out would be a state with no block for the dispatch to jump to.
		for (var i = 0; i < _states.Count; i++)
			if (reachable[i] && !placed[i])
				_order.Add(i);

		_written = reachable;

		Verify();
	}

	/// <summary>
	/// The states the dispatch has to be able to land on, in ascending order.
	/// </summary>
	/// <remarks>
	/// A state is arrived at by the dispatch only when an arena entry names it, and the only
	/// other way into the table is from outside it. Every other state is reached by falling
	/// into it or by a <c>goto</c> written inside the method, and a case for one of those is
	/// a slot in the jump table and a jump stub that nothing can execute. Across every
	/// grammar in this repository that was 82% of the table — 677 cases of 735 in `Url`.
	/// </remarks>
	IEnumerable<int> Dispatched()
	{
		// Nothing said where the parse begins, so anything could be a beginning. `PlanLayout`
		// kept every state for that same reason; the dispatch has to be able to land on every
		// one of them. A grammar that publishes nothing is the case — the emitter writes a
		// recognizer for each of its rules rather than for what was asked for.
		if (_roots.Count == 0)
		{
			for (var i = 0; i < _states.Count; i++)
				if (Written(Resolved(i + First)))
					yield return i + First;

			yield break;
		}

		// The roots are named from outside and are named unresolved, which is how the
		// wrapper passes them; everything else was read back out of a body that Redirect
		// had already been over.
		var live = new SortedSet<int>(_roots);

		foreach (var state in _resumed)
			live.Add(state);

		foreach (var state in live)
			// Below `First` are the three fixed cases, which are written whatever happens —
			// a rule's own continuation is `Return`, so entries name them — and the two
			// kinds that carry a nesting count rather than a state, which is always 0.
			if (state >= First && Written(Resolved(state)))
				yield return state;
	}

	/// <summary>Every state an arena entry names, after redirection.</summary>
	readonly HashSet<int> _resumed = [];

	/// <summary>Which states are written at all.</summary>
	bool[] _written = [];

	/// <summary>Whether a state has a label in the output — the three fixed ones always do.</summary>
	bool Written(int state) =>
		state - First is var index && (index < 0 || _written.Length == 0 || (index < _written.Length && _written[index]));

	/// <summary>The state a body is, where the body is one unconditional jump and nothing else.</summary>
	static int? JumpOnly(string body)
	{
		int? only = null;

		foreach (var line in body.Split('\n'))
		{
			var written   = line.TrimEnd();
			var statement = written.TrimStart();

			if (statement.Length == 0)
				continue;

			if (only is not null || written.Length != statement.Length || Jump(statement) is not { } target)
				return null;

			only = target;
		}

		return only;
	}

	/// <summary>The state a body ends by jumping to, where its last statement is that jump.</summary>
	static int? Tail(string body)
	{
		var lines = body.Split('\n');

		for (var i = lines.Length - 1; i >= 0; i--)
		{
			var written   = lines[i].TrimEnd();
			var statement = written.TrimStart();

			if (statement.Length == 0)
				continue;

			// Indented means it is inside something — a branch taken only sometimes, which
			// the line after it is not.
			return written.Length == statement.Length ? Jump(statement) : null;
		}

		return null;
	}

	/// <summary>The state a single <c>goto</c> statement names, by label or by number.</summary>
	static int? Jump(string statement)
	{
		if (!statement.StartsWith("goto ", StringComparison.Ordinal) ||
			!statement.EndsWith(";", StringComparison.Ordinal))
		{
			return null;
		}

		var label = statement.Substring("goto ".Length, statement.Length - "goto ".Length - 1);

		return label switch
		{
			"Return" => Return,
			"Accept" => Accept,
			"Fail"   => Fail,
			"S"      => null,
			_        => label.StartsWith("S", StringComparison.Ordinal) &&
						int.TryParse(label.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out var state)
							? state
							: null,
		};
	}

	/// <summary>
	/// The same text with every state it names replaced by the state that one really is.
	/// </summary>
	/// <remarks>
	/// Two places name a state: a <c>goto</c>, and the second argument of a
	/// <c>ParserEntry</c>, which is where the parse resumes. The second matters as much as
	/// the first — a resume point pointing at a signpost pays the dispatch twice.
	/// </remarks>
	string Redirect(string body)
	{
		body = Gotos.Replace(body, match =>
			$"goto {Label(Resolved(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)))};");

		return Resumes.Replace(body, match =>
			!MeansAState(match.Groups[1].Value)
				? match.Value
				:
			$"new ParserEntry(ParserEntry.{match.Groups[1].Value}, " +
			Resolved(int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)) + ",");
	}

	int Resolved(int state) =>
		state - First is var index && index >= 0 && index < _resolved.Length ? _resolved[index] : state;

	static readonly Regex Gotos   = new(@"goto S(\d+);", RegexOptions.Compiled);
	static readonly Regex Resumes = new(@"new ParserEntry\(ParserEntry\.(\w+), (\d+),", RegexOptions.Compiled);

	/// <summary>The entry kinds whose second field is a state, and not something else.</summary>
	/// <remarks>
	/// It is not a state in four of them, and rewriting it as one is silent corruption:
	/// <c>Capture</c> and <c>RuleCapture</c> hold a capture slot there, <c>Construct</c> a
	/// factory's number and <c>Recovery</c> a recovery's. A slot that happened to equal a
	/// collapsed state's number came back as that state's, so the value it named was never
	/// built and the construction was handed a null.
	///
	/// Latent until something made a rule's entry state collapsible — the collapse is what
	/// makes a rewrite happen at all — and found by trying to take a `Trace` call out of one.
	/// </remarks>
	/// <summary>What an arena entry's second field means, for each kind there is.</summary>
	/// <remarks>
	/// One entry per kind and no default: <see cref="MeansAState"/> throws for a kind nobody
	/// has decided about. The list this replaces named the eight resumable kinds and said
	/// nothing about the other ten, so a kind added later carrying a state there would have
	/// had its target quietly collapse — and one added carrying something else would have
	/// been read as a state if anyone had put it in by reflex. Two kinds were added this
	/// week.
	/// </remarks>
	enum Second
	{
		/// <summary>A state to resume at. These are the ones layout must follow.</summary>
		State,

		/// <summary>A capture's slot.</summary>
		Slot,

		/// <summary>Which factory, or which recovery, or which `with state` site.</summary>
		Choice,

		/// <summary>Nothing that is numbered — a marker, whose second field is always zero.</summary>
		Nothing,
	}

	static readonly Dictionary<string, Second> SecondField = new()
	{
		["Choice"]          = Second.State,
		["Call"]            = Second.State,
		["Lookahead"]       = Second.State,
		["Completed"]       = Second.State,
		["Dead"]            = Second.State,
		["Run"]             = Second.State,
		["PendingRecovery"] = Second.State,
		["LoopExit"]        = Second.State,

		["Capture"]         = Second.Slot,
		["RuleCapture"]     = Second.Slot,
		["CaptureOpen"]     = Second.Slot,

		["Construct"]       = Second.Choice,
		["Recovery"]        = Second.Choice,
		["StateSet"]        = Second.Choice,
		["StateEnd"]        = Second.Choice,

		["Atomic"]          = Second.Nothing,
		["Repeat"]          = Second.Nothing,
		["TurnDone"]        = Second.Nothing,
	};

	/// <summary>Whether this kind's second field is a state to resume at.</summary>
	/// <exception cref="InvalidOperationException">Nobody has decided.</exception>
	static bool MeansAState(string kind) =>
		SecondField.TryGetValue(kind, out var means)
			? means == Second.State
			: throw new InvalidOperationException(
				$"No decision about what a '{kind}' entry's second field is. Layout rewrites " +
				"state numbers and follows them; a slot or a factory read as one is silent " +
				"corruption, and a state not read as one is text nothing can reach " +
				"(Machine.Layout.cs).");
}
