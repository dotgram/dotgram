using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Where a state can go on to, recorded where the text that means it is written.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PlanLayout"/> needs the same thing and recovers it by reading the finished
/// text back with two regular expressions. That makes every jump's spelling load-bearing,
/// and the two halves fail differently when a spelling drifts. A missed <c>goto</c> leaves
/// its target judged unreachable and so unwritten, and the jump then names a label that is
/// not there — which the C# compiler says out loud. A missed resume leaves the state out of
/// the dispatch instead: the block is written, the code compiles, and a parse that should
/// have resumed there falls to the default and refuses input it ought to accept.
/// </para>
/// <para>
/// The second is the one worth spending on. So the edge is recorded by the same call that
/// writes the text — there is no second spelling to keep in step — and <see cref="Verify"/>
/// holds the recorded graph against the recovered one on every grammar this repository
/// compiles. Both exist until the recorded one has earned the right to be the only one.
/// </para>
/// </remarks>
sealed partial class Machine
{
	// ── The graph (§where a state goes, said once) ─────────────────────────────────

	/// <summary>What a state's own text says about where the parse can go on from it.</summary>
	sealed class Edges
	{
		/// <summary>States a <c>goto</c> written in this one names.</summary>
		public readonly List<int> Jumps = [];

		/// <summary>States an arena entry pushed in this one names, to resume at.</summary>
		public readonly List<int> Resumes = [];
	}

	/// <summary>
	/// The edges of each state, by the writer that is its body.
	/// </summary>
	/// <remarks>
	/// A state's body is one writer and is never composed from others, so the writer is the
	/// state as far as recording goes. Anything else written to — the file, a header — is not
	/// in the table and is absent here, which is what makes a lookup that finds nothing the
	/// right answer rather than a missed edge.
	/// </remarks>
	readonly Dictionary<Writer, Edges> _edges = [];

	/// <summary>
	/// The label of a state, recording that <paramref name="at"/> can jump to it.
	/// </summary>
	string Label(Writer at, int state)
	{
		if (_edges.TryGetValue(at, out var edges))
			edges.Jumps.Add(state);

		return Label(state);
	}

	/// <summary>
	/// The state an arena entry resumes at, recording that <paramref name="at"/> can put it
	/// there.
	/// </summary>
	int Resuming(Writer at, int state)
	{
		if (_edges.TryGetValue(at, out var edges))
			edges.Resumes.Add(state);

		return state;
	}

	/// <summary>
	/// That the recorded graph and the one read back out of the text say the same thing.
	/// </summary>
	/// <remarks>
	/// Held after <see cref="PlanLayout"/> has redirected the bodies, so the recovered side
	/// names states that are already resolved and the recorded side is resolved to match. A
	/// state that resolves to <c>Return</c>, <c>Accept</c> or <c>Fail</c> is written as that
	/// label and is no longer a numbered jump in the text, so neither side counts it.
	/// </remarks>
	void Verify()
	{
		for (var i = 0; i < _states.Count; i++)
		{
			Agree(i, "jumps to", Settled(Recorded(i).Jumps), Recovered(_bodies[i], Gotos, 1));
			Agree(i, "resumes at", Settled(Recorded(i).Resumes), Resumable(_bodies[i]));
		}
	}

	/// <summary>What the state at an index recorded, which is nothing where it wrote nothing.</summary>
	Edges Recorded(int index) =>
		index >= 0 && index < _states.Count && _edges.TryGetValue(_states[index], out var edges)
			? edges
			: None;

	static readonly Edges None = new();

	/// <summary>The states a set of recorded targets really names, once collapsed.</summary>
	HashSet<int> Settled(List<int> states)
	{
		var settled = new HashSet<int>();

		foreach (var state in states)
			if (Resolved(state) is var landed && landed >= First)
				settled.Add(landed);

		return settled;
	}

	/// <summary>The states a body's text names, by the pattern that finds them.</summary>
	static HashSet<int> Recovered(string body, Regex pattern, int group)
	{
		var found = new HashSet<int>();

		foreach (Match match in pattern.Matches(body))
			if (int.Parse(match.Groups[group].Value, CultureInfo.InvariantCulture) is var state && state >= First)
				found.Add(state);

		return found;
	}

	/// <summary>The states a body's arena entries name to resume at.</summary>
	static HashSet<int> Resumable(string body)
	{
		var found = new HashSet<int>();

		foreach (Match match in Resumes.Matches(body))
			if (MeansAState(match.Groups[1].Value) &&
				int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) is var state && state >= First)
			{
				found.Add(state);
			}

		return found;
	}

	/// <summary>Or says which way they differ, and stops.</summary>
	/// <remarks>
	/// Both directions are a defect and neither is the same defect. A state the text names
	/// and the record does not is a site that jumps without saying so — layout still finds
	/// it, so nothing is wrong yet, but the record is no longer the whole graph. The other
	/// way round is the one that would have shipped: the site said where it goes and the
	/// text says it in a spelling the recovery does not match, so layout leaves the state
	/// out of the dispatch and a parse that should resume there refuses instead.
	/// </remarks>
	/// <exception cref="InvalidOperationException">They do not.</exception>
	static void Agree(int index, string direction, HashSet<int> recorded, HashSet<int> recovered)
	{
		if (recorded.SetEquals(recovered))
			return;

		var unsaid = Missing(recovered, recorded);
		var unseen = Missing(recorded, recovered);

		throw new InvalidOperationException(
			$"State {index + First} {direction} {Listed(recorded)} by the sites that wrote it, " +
			$"and {Listed(recovered)} by reading that text back." +
			(unseen.Count > 0
				? $" Recorded and not recovered: {Listed(unseen)} — written in a spelling the " +
					"recovery does not match, which leaves the state out of the dispatch and a " +
					"parse that should resume there refusing instead."
				: "") +
			(unsaid.Count > 0
				? $" Recovered and not recorded: {Listed(unsaid)} — a site that names a state " +
					"without saying so, which leaves the recorded graph short of the whole."
				: "") +
			" (Machine.Graph.cs)");
	}

	static HashSet<int> Missing(HashSet<int> of, HashSet<int> from)
	{
		var missing = new HashSet<int>(of);

		missing.ExceptWith(from);

		return missing;
	}

	static string Listed(HashSet<int> states) =>
		states.Count == 0 ? "nothing" : string.Join(", ", states.OrderBy(static state => state));
}
