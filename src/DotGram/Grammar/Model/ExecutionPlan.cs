using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// How a grammar is to be executed, as against what it means.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RecognitionGraph"/> says what a grammar recognizes. This says what the
/// generated code should do about it — which so far amounts to one decision, and is
/// written down here so that the rest has somewhere to arrive.
/// </para>
/// <para>
/// The decisions divide sharply, and the division is the shape of the work ahead. Some are
/// about a rule and nothing else: whether it can be compiled where it is called depends on
/// what it keeps, whether it can reach itself, whether it carries a strength — and not at
/// all on who calls it. Those can be settled here, once. The rest — whether a repetition
/// can be run to its end and never asked to give any of it back, whether a choice needs an
/// entry written for the alternatives after it — depend on what follows, and what follows
/// depends on the caller. They are settled during compilation today, from a context
/// threaded down the tree, and they will move here when a region can carry a context of its
/// own.
/// </para>
/// <para>
/// Over every rule, not outward from the publications. Reaching outward was tried first, on
/// the principle that a rule nothing publishes should decide nothing — and it is the right
/// principle for a decision that depends on a caller, which this one does not. All it did
/// here was leave unpublished rules out of the set, so that their callers stopped compiling
/// them in place and the states shifted underneath everything. The reachable walk belongs
/// with the decisions that need a context, and arrives with them.
/// </para>
/// </remarks>
public sealed class ExecutionPlan
{
	ExecutionPlan(IReadOnlyCollection<RuleSymbol> compiledInPlace) =>
		CompiledInPlace = compiledInPlace;

	/// <summary>
	/// The rules whose bodies are written where they are called, rather than once in a block
	/// of their own.
	/// </summary>
	/// <remarks>
	/// A rule that keeps no value buys nothing by having a boundary: the call costs a frame
	/// in the arena, a jump away and a dispatch back, and none of it is doing anything. The
	/// expansion terminates because the call graph beneath a rule outside every cycle is a
	/// DAG, and what the duplication costs is generated text.
	/// </remarks>
	public IReadOnlyCollection<RuleSymbol> CompiledInPlace { get; }

	/// <summary>Works out the plan for a graph.</summary>
	public static ExecutionPlan Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var inPlace = new HashSet<RuleSymbol>();

		foreach (var rule in graph.Rules)
			if (!graph.Types.ContainsKey(rule) &&
				graph.Results[rule].Count == 0 &&
				!graph.Recursive.Contains(rule) &&
				!graph.Climbing.ContainsKey(rule) &&
				graph.Bodies.TryGetValue(rule, out var body) &&
				!NodeWalk.Descendants(body).Any(node => node is Node.Capture or Node.Construct))
			{
				inPlace.Add(rule);
			}

		return new ExecutionPlan(inPlace);
	}
}
