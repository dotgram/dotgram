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

			foreach (Match match in Gotos.Matches(_bodies[index]))
				pending.Push(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));

			foreach (Match match in Resumes.Matches(_bodies[index]))
				pending.Push(int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
		}

		_order = new List<int>(_states.Count);

		for (var i = 0; i < _states.Count; i++)
			if (reachable[i])
				_order.Add(i);

		_written = reachable;
	}

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
			$"new ParserEntry(ParserEntry.{match.Groups[1].Value}, " +
			Resolved(int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)) + ",");
	}

	int Resolved(int state) =>
		state - First is var index && index >= 0 && index < _resolved.Length ? _resolved[index] : state;

	static readonly Regex Gotos   = new(@"goto S(\d+);", RegexOptions.Compiled);
	static readonly Regex Resumes = new(@"new ParserEntry\(ParserEntry\.(\w+), (\d+),", RegexOptions.Compiled);
}
