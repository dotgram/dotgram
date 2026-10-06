using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Whose value somebody asks for: at every call of a rule that builds, whether the value that
/// call reads is ever built.
/// </summary>
/// <remarks>
/// <para>
/// A construction runs only for what the answer is made of (§7.2). The tape keeps that promise
/// by building nothing while it reads: when the parse has accepted, the walk goes from the
/// answer down through the captures and builds what it reaches, and a record nothing captured
/// is written and never read. What a guard names is the one thing built while reading, since
/// the guard needs it then. That is the reference this analysis states in advance, so a
/// carrier that builds where it reads can build the same things and no others.
/// </para>
/// <para>
/// Every call of a rule that builds, or that reaches one, is one of three things:
/// </para>
/// <list type="bullet">
/// <item><see cref="Kind.Never"/> — not captured. Nothing reads what the call built, so
/// nothing under it is built either: the walk never reaches its record, and the records
/// below it are reached only through it. <c>Held = HeldBody =&gt; @(new Held(span))</c>
/// reads a body to find where it ends and keeps a span, and a call inside a lookahead is
/// never captured at all.</item>
/// <item><see cref="Kind.Inherits"/> — captured, and read only by the construction of the
/// rule it stands in. Built exactly when that rule's value is.</item>
/// <item><see cref="Kind.Always"/> — captured, and named by something that runs while
/// reading: a guard, or the text of a <c>with state</c>, that stands after the capture.
/// Built whether or not the rule it stands in ever is. A call of a rule whose own guard
/// names the value so far of its fold (§4.3) is one too, captured or not: the guard runs
/// while the rule is read and asks for the rule's value, so every reading of it builds.</item>
/// </list>
/// <para>
/// A rule is <see cref="Report.ReadUnbuilt">read unbuilt</see> where some reading of it is
/// one whose value nobody asks for: reached through a <see cref="Kind.Never"/> call, and from
/// there through the calls that inherit. An <see cref="Kind.Always"/> call asks again for
/// what is under it, so demand starts over there. A reading may also begin with nothing asked
/// of it: an entry that reads a refused input a second time, for what the refusal says and not
/// for a value it will never hand out, is one — the rules it begins at are given to
/// <see cref="Of(RecognitionGraph, IEnumerable{RuleSymbol})"/>, and they and what inherits from
/// them are read unbuilt too.
/// </para>
/// <para>
/// A name is found in a guard's text as the scanner found it free there
/// (<see cref="RecognitionGraph.FreeNames"/>): an identifier written with a Unicode escape is
/// the same name, and one inside a string literal or after a dot is not. Without a scanner the
/// spelling decides, as a substring — <b>conservative in the direction that builds</b>, since a
/// value a guard asks for and does not get is a null in the author's code. A captured value
/// the construction never mentions still inherits: the tape hands the construction every member.
/// </para>
/// <para>
/// It reads the graph and nothing else, like <see cref="Replay"/> beside it, so it stands
/// before anything is emitted and any rendering that builds can ask it.
/// </para>
/// </remarks>
public static class Demand
{
	/// <summary>Whether the value one call reads is ever built.</summary>
	public enum Kind
	{
		/// <summary>When the rule it stands in is built: read only by that rule's construction.</summary>
		Inherits,

		/// <summary>Always: something that runs while reading names it.</summary>
		Always,

		/// <summary>Never: nothing captures it, so nothing under it is built.</summary>
		Never,
	}

	/// <summary>What the graph says about every call in it and every rule.</summary>
	public sealed class Report
	{
		readonly Dictionary<Node, Kind> _calls;
		readonly HashSet<RuleSymbol>    _unbuilt;
		readonly HashSet<RuleSymbol>    _builds;
		readonly HashSet<RuleSymbol>    _unasked;

		internal Report(Dictionary<Node, Kind> calls, HashSet<RuleSymbol> unbuilt, HashSet<RuleSymbol> builds, HashSet<RuleSymbol> unasked)
		{
			_calls   = calls;
			_unbuilt = unbuilt;
			_builds  = builds;
			_unasked = unasked;
		}

		/// <summary>
		/// What one call of a rule body asks of its value. A call this report has no answer
		/// for — one that builds nothing and reaches nothing that does, or a node that is not
		/// a call of the graph's bodies — is <see cref="Kind.Inherits"/>, which asks nothing
		/// a rendering does not already do.
		/// </summary>
		public Kind Of(Node.Call call)
		{
			return _calls.TryGetValue(call ?? throw new ArgumentNullException(nameof(call)), out var kind) ? kind : Kind.Inherits;
		}

		/// <summary>Whether this report has an answer for the call, as opposed to the default.</summary>
		public bool Knows(Node.Call call)
		{
			return _calls.ContainsKey(call ?? throw new ArgumentNullException(nameof(call)));
		}

		/// <summary>Whether some reading of this rule is one whose value nobody asks for.</summary>
		public bool ReadUnbuilt(RuleSymbol rule)
		{
			return _unbuilt.Contains(rule);
		}

		/// <summary>Whether the rule builds a value, or reaches one that does: whether a reading of it can build anything at all.</summary>
		public bool Builds(RuleSymbol rule)
		{
			return _builds.Contains(rule ?? throw new ArgumentNullException(nameof(rule)));
		}

		/// <summary>
		/// Whether an entry may read this rule with nothing asked of its value: one of the rules
		/// given to <see cref="Of(RecognitionGraph, IEnumerable{RuleSymbol})"/>, unless its own
		/// guard asks for the value so far, which no reading of it can do without.
		/// </summary>
		public bool ReadUnasked(RuleSymbol rule)
		{
			return _unasked.Contains(rule ?? throw new ArgumentNullException(nameof(rule)));
		}

		/// <summary>Whether any call of the graph is <see cref="Kind.Never"/>: whether anything is read and not built.</summary>
		public bool Discards => _unbuilt.Count > 0;
	}

	/// <summary>Every call of every body in the graph, and every rule.</summary>
	public static Report Of(RecognitionGraph graph)
	{
		return Of(graph, []);
	}

	/// <summary>
	/// Every call of every body in the graph, and every rule — where the rules named are also
	/// read with nothing asked of their value.
	/// </summary>
	/// <param name="unasked">
	/// The rules an entry reads with nothing asked of the value: they, and whatever inherits
	/// from them, are <see cref="Report.ReadUnbuilt">read unbuilt</see> besides what the calls say.
	/// </param>
	public static Report Of(RecognitionGraph graph, IEnumerable<RuleSymbol> unasked)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (unasked is null)
			throw new ArgumentNullException(nameof(unasked));

		var builds = Builds(graph);
		var calls  = new Dictionary<Node, Kind>(NodeIdentity.Instance);

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				Walk(graph, rule, body, builds, calls);

		// A rule whose guard reads the value so far asks for its own value while it is read: every
		// call of it builds, and an entry of it cannot read it unasked.
		var selfAsking = SelfAsking(graph);

		foreach (var call in calls.Keys.ToList())
			if (selfAsking.Contains(((Node.Call)call).Rule))
				calls[call] = Kind.Always;

		var entries = new HashSet<RuleSymbol>(unasked);

		entries.ExceptWith(selfAsking);

		return new Report(calls, Unbuilt(graph, calls, entries), builds, entries);
	}

	/// <summary>
	/// The left-recursive rules a step of which runs something while reading — a guard, the text
	/// of a <c>with state</c> — that names the value so far (§4.3): the rule's own value, asked
	/// for before the rule has answered. Found as a name is found in a guard's text elsewhere here
	/// (<see cref="Names"/>).
	/// </summary>
	static HashSet<RuleSymbol> SelfAsking(RecognitionGraph graph)
	{
		var found = new HashSet<RuleSymbol>();

		foreach (var fold in graph.Folds)
			foreach (var step in fold.Value.Accumulators)
				foreach (var node in NodeWalk.Descendants(step.Key))
				{
					var text = node switch
					{
						Node.Guard(var guard, _)   => guard,
						Node.Marked(_, var marked) => marked,
						_                          => null,
					};

					if (text is not null && Names(graph, text, step.Value))
					{
						found.Add(fold.Key);
						break;
					}
				}

		return found;
	}

	/// <summary>The rules that build a value, or reach one that does.</summary>
	static HashSet<RuleSymbol> Builds(RecognitionGraph graph)
	{
		var found = new HashSet<RuleSymbol>();

		foreach (var rule in graph.Rules)
			if (Valued(graph, rule))
				found.Add(rule);

		var again = true;

		while (again)
		{
			again = false;

			foreach (var rule in graph.Rules)
				if (!found.Contains(rule) && graph.Bodies.TryGetValue(rule, out var body))
					foreach (var node in NodeWalk.Descendants(body))
						if (node is Node.Call(var called, _) && found.Contains(called))
						{
							found.Add(rule);
							again = true;

							break;
						}
		}

		return found;
	}

	/// <summary>What the emitter counts as a rule with a value (<c>Machine.ValueRule</c>).</summary>
	static bool Valued(RecognitionGraph graph, RuleSymbol rule)
	{
		return graph.Results.TryGetValue(rule, out var members) && members.Count > 0 || graph.Types.ContainsKey(rule);
	}

	/// <summary>One body: every call in it that builds, answered.</summary>
	static void Walk(
		RecognitionGraph graph, RuleSymbol owner, Node body, HashSet<RuleSymbol> builds,
		Dictionary<Node, Kind> calls)
	{
		var layout = CaptureLayout.Of(
			body,
			other => Valued(graph, other),
			graph.Folds.TryGetValue(owner, out var fold) ? fold.Loop : null);

		// What runs while reading, with how many slots stand before it: a guard is handed
		// the captures before it and no others (Machine.GuardMembers).
		var readers = new List<(string Text, int Before)>();

		foreach (var node in NodeWalk.Descendants(body))
			switch (node)
			{
				case Node.Guard(var text, _):
					readers.Add((text, layout.Before(node)));
					break;

				case Node.Marked(_, var text):
					readers.Add((text, layout.Before(node)));
					break;
			}

		var kept = new Dictionary<Node, Kind>(NodeIdentity.Instance);

		// Captured calls: the capture's own call, or the call a captured repetition repeats
		// — the two shapes whose record a caller keeps (Machine.DirectStrays).
		foreach (var node in NodeWalk.Descendants(body))
		{
			if (node is not Node.Capture(var name, var held))
				continue;

			var call = held switch
			{
				Node.Call one                  => one,
				Node.Repeat(Node.Call many, _, _) => many,
				_                              => null,
			};

			if (call is null || !Valued(graph, call.Rule))
				continue;

			var slot = layout.SlotOrNone(node);

			kept[call] = Named(graph, name, slot, readers) ? Kind.Always : Kind.Inherits;
		}

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Call call && builds.Contains(call.Rule))
				calls[call] = kept.TryGetValue(call, out var kind) ? kind : Kind.Never;
	}

	/// <summary>Whether something that runs while reading, after this capture, names it.</summary>
	static bool Named(RecognitionGraph graph, string name, int slot, List<(string Text, int Before)> readers)
	{
		foreach (var (text, before) in readers)
			if ((slot < 0 || slot < before) && Names(graph, text, name))
				return true;

		return false;
	}

	/// <summary>
	/// Whether the C# names the identifier: what the scanner found free in it where the graph has
	/// a scanner's answer, and the spelling, as a substring, where it has none.
	/// </summary>
	static bool Names(RecognitionGraph graph, string text, string name)
	{
		return graph.FreeNames.TryGetValue(text, out var free)
			? free.Contains(name)
			: text.Contains(name);
	}

	/// <summary>
	/// The rules some reading of which nobody asks the value of: reached through a call that
	/// is never built, or begun at by an entry that asks nothing, and from there through calls
	/// that inherit.
	/// </summary>
	static HashSet<RuleSymbol> Unbuilt(RecognitionGraph graph, Dictionary<Node, Kind> calls, IEnumerable<RuleSymbol> unasked)
	{
		var found   = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var entry in calls)
			if (entry.Value == Kind.Never && found.Add(((Node.Call)entry.Key).Rule))
				pending.Push(((Node.Call)entry.Key).Rule);

		foreach (var rule in unasked)
			if (found.Add(rule))
				pending.Push(rule);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!graph.Bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call call &&
					calls.TryGetValue(call, out var kind) && kind != Kind.Always &&
					found.Add(call.Rule))
					pending.Push(call.Rule);
		}

		return found;
	}
}
