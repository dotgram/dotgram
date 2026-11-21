using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Frames that can be told the continuation after their answer is lost: a recursive rule asked
/// again whose answer is one everything after it has already failed with turns its own ways again
/// instead of answering, and the reading never goes back down the nest for it.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it is for.</b> A rule that answers with ways still open inside it hands its caller one
/// reading of several. Where everything after that answer fails, the latest open way is inside the
/// rule, and the retry that takes it reads the whole nest down to it again, comes back up with the
/// next reading and fails above again: a nest d deep whose innermost reading splits k ways costs d
/// times k. The tape remembers, for a frame that returned with open ways, what it answered; a retry
/// that takes a way inside it marks that answer lost; and where the same frame, entered at the same
/// place with the tape before it unchanged, answers the same again, it asks itself again at once.
/// </para>
/// <para>
/// <b>Where it may be used.</b> What follows a frame sees of it only its answer — the position —
/// unless something there looks at a value the frame built: a guard naming a capture whose reading
/// can reach the frame, a <c>context</c> or a state the frame may have changed, a back-reference,
/// the memo of failures (which would remember the frame as failed where it was only lost for this
/// caller), or a recovering repetition's element, whose failures say what was expected. A rule any
/// of these can see into is not a place, and a grammar with none writes exactly what it wrote before.
/// </para>
/// <para>
/// On a carrier that defers its constructions, a frame turns its ways again only where nothing is
/// being built (<c>unbuilt != 0</c>): the attempt that would have failed above is one the reading
/// without the memo would have made unbuilt too, so the constructions that run are the same. And a
/// loop that stops building clears what was written inside it before, since those answers were
/// lost to readings that built.
/// </para>
/// </remarks>
partial class Machine
{
	/// <summary>Whether the readers may turn a lost answer at once (<see cref="GramCompilerOptions.DoomContinuations"/>).</summary>
	internal bool DoomsContinuations => _settings.DoomsContinuations;

	HashSet<RuleSymbol>? _doomBarred;

	bool? _doomGrammar;

	/// <summary>
	/// Whether this rule's way back is a place: it can reach itself, it is read on the tape, and
	/// nothing after it can see more of it than its answer.
	/// </summary>
	internal bool IsDoomPlace(RuleSymbol rule)
	{
		return DoomGrammar && Recurses(rule) && !DoomBarred.Contains(rule);
	}

	/// <summary>Whether any rule of this machine may be a place, and so whether its loops clear what a change of carrier loses.</summary>
	internal bool Dooms
	{
		get
		{
			if (!DoomGrammar)
				return false;

			foreach (var rule in _rules)
				if (Recurses(rule) && !DoomBarred.Contains(rule))
					return true;

			return false;
		}
	}

	/// <summary>The number a place is known by on the tape.</summary>
	internal int DoomSite(RuleSymbol rule)
	{
		if (!_doomSites.TryGetValue(rule, out var site))
			_doomSites[rule] = site = _doomSites.Count + 1;

		return site;
	}

	readonly Dictionary<RuleSymbol, int> _doomSites = [];

	/// <summary>What bars every rule of the grammar at once.</summary>
	bool DoomGrammar
	{
		get
		{
			if (_doomGrammar is { } known)
				return known;

			var allowed = DoomsContinuations
				&& Tracing is null
				&& _graph.Context is null
				&& _graph.State is null
				&& !Memoises
				&& !(Carrier is ImmediateCarrier { Holds: true });

			if (allowed)
			{
				foreach (var body in _graph.Bodies.Values)
				{
					foreach (var node in NodeWalk.Descendants(body))
					{
						if (node is Node.Bind or Node.Same or Node.External { UsesContext: true })
						{
							allowed = false;

							break;
						}
					}

					if (!allowed)
						break;
				}
			}

			_doomGrammar = allowed;

			return allowed;
		}
	}

	bool Recurses(RuleSymbol rule)
	{
		return Reach(rule).Contains(rule);
	}

	readonly Dictionary<RuleSymbol, HashSet<RuleSymbol>> _reach = [];

	/// <summary>Every rule a reading of <paramref name="rule"/> can call, at any depth.</summary>
	HashSet<RuleSymbol> Reach(RuleSymbol rule)
	{
		if (_reach.TryGetValue(rule, out var known))
			return known;

		var seen    = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var called in Called(rule))
			if (seen.Add(called))
				pending.Push(called);

		while (pending.Count > 0)
		{
			foreach (var called in Called(pending.Pop()))
				if (seen.Add(called))
					pending.Push(called);
		}

		_reach[rule] = seen;

		return seen;
	}

	IEnumerable<RuleSymbol> Called(RuleSymbol rule)
	{
		if (!_graph.Bodies.TryGetValue(rule, out var body))
			yield break;

		foreach (var called in CalledUnder(body))
			yield return called;
	}

	IEnumerable<RuleSymbol> CalledUnder(Node node)
	{
		foreach (var each in NodeWalk.Reading(node, _graph))
			if (each is Node.Call(var called, _))
				yield return called;
	}

	/// <summary>The rules some later look can see a value of, or that a recovering element reads.</summary>
	HashSet<RuleSymbol> DoomBarred
	{
		get
		{
			if (_doomBarred is not null)
				return _doomBarred;

			var barred = new HashSet<RuleSymbol>();

			foreach (var body in _graph.Bodies.Values)
			{
				var captures = new List<Node.Capture>();
				var texts    = new List<string>();
				var opaque   = false;

				foreach (var node in NodeWalk.Reading(body, _graph))
				{
					switch (node)
					{
						case Node.Capture capture:
							captures.Add(capture);
							break;
						case Node.Guard guard:
							texts.Add(guard.Text);
							break;
						case Node.Choice { Selection: { } selection }:
							texts.Add(selection.Selector.Text);
							break;
						case Node.Condition:
							opaque = true;
							break;
					}
				}

				if (texts.Count == 0 && !opaque)
					continue;

				var named = new HashSet<string>(StringComparer.Ordinal);

				foreach (var text in texts)
					foreach (Match word in Regex.Matches(text, @"[\p{L}_][\p{L}\p{Nd}_]*"))
						named.Add(word.Value);

				foreach (var capture in captures)
				{
					if (!opaque && !named.Contains(capture.Name))
						continue;

					foreach (var called in CalledUnder(capture.Body))
					{
						barred.Add(called);
						barred.UnionWith(Reach(called));
					}
				}
			}

			// A recovering repetition's element: what it fails with is what it says was expected,
			// and a frame inside it that turns again instead of failing above says something else.
			foreach (var node in _graph.Recoveries.Keys)
			{
				foreach (var called in CalledUnder(node))
				{
					barred.Add(called);
					barred.UnionWith(Reach(called));
				}
			}

			_doomBarred = barred;

			return barred;
		}
	}
}
