using System;
using System.Collections.Generic;

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
	ExecutionPlan(IReadOnlyCollection<RuleSymbol> compiledInPlace)
	{
		CompiledInPlace = compiledInPlace;
	}

	/// <summary>
	/// The rules whose bodies are written where they are called, rather than once in a block
	/// of their own.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule that keeps no value buys nothing by having a boundary: the call costs a frame
	/// in the arena, a jump away and a dispatch back, and none of it is doing anything. The
	/// expansion terminates because the call graph beneath a rule outside every cycle is a
	/// DAG, and what the duplication costs is generated text.
	/// </para>
	/// <para>
	/// <b>Up to a size.</b> "What the duplication costs is generated text" was written as
	/// though text were free, and for the helper rules this was aimed at — a digit run, an
	/// octet, a set of comparison operators, four or six nodes each — it is. It is not free
	/// for a large one, and the expansion is compositional: a rule inlined into a rule that
	/// is itself inlined at a dozen sites is written a dozen times. The reserved-word list
	/// of standard SQL is 285 nodes and reaches a dozen places through the identifier that
	/// names it, and it came to <b>59% of the generated file</b> — 60,317 lines against
	/// 22,228 with the list removed altogether.
	/// </para>
	/// <para>
	/// So a boundary is kept where the body is large, and the trade is measured on both
	/// sides: a valueless call is 4.8 ns (<c>benchmarks/CallCost.cs</c>) and a copy is its
	/// size in emitted code, times however many places reach it.
	/// </para>
	/// <para>
	/// Never a rule lifted out of a marked repetition (§8.2): the boundary is what it was made for,
	/// and a guard in it reads from where it was called.
	/// </para>
	/// </remarks>
	public IReadOnlyCollection<RuleSymbol> CompiledInPlace { get; }

	/// <summary>
	/// Whether a repetition's body consumes exactly one character, as one test of it, and keeps
	/// nothing: an element that admits some character, a literal of one character, a rule
	/// written where it is called that is one, or a choice of them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A choice of such alternatives is a disjunction and not a choice: whichever one matched,
	/// the position afterwards is the same and so is the continuation, so there is nothing to
	/// come back to. The emitter writes such a body as one test of the character, and the
	/// analysis of repetitions that read a run two ways (<see cref="ExponentialRepetition"/>)
	/// reads it as one letter, from this same answer: a second reading of the body there would
	/// drift from what the emitter writes.
	/// </para>
	/// <para>
	/// An element that admits nothing, written as no test at all, is not one: no turn of it is
	/// ever read.
	/// </para>
	/// <para>
	/// Walked with a stack of its own and each rule's body once, so a chain or a diamond of calls
	/// written in place costs its size and never the depth of the thread's stack: the answer is
	/// whether every character test reached is one, which asking a rule twice does not change.
	/// </para>
	/// </remarks>
	/// <param name="inPlace">Whether a call to the rule is written where it is called, as the caller renders it.</param>
	/// <param name="step">
	/// Told of every node visited, for a caller that holds the walk to a budget; where it answers
	/// false the walk stops and the body is not one. Null for no budget.
	/// </param>
	public static bool OneCharacter(Node body, RecognitionGraph graph, Func<RuleSymbol, bool> inPlace, Func<bool>? step = null)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (inPlace is null)
			throw new ArgumentNullException(nameof(inPlace));

		var pending = new Stack<Node>();
		var entered = new HashSet<RuleSymbol>();

		pending.Push(body);

		while (pending.Count > 0)
		{
			if (step is not null && !step())
				return false;

			switch (pending.Pop())
			{
				case Node.Element element:
					if (!(element.IsNegated || element.Ranges.Count > 0 || element.Categories.Count > 0 ||
						element.References.Any(static reference => reference is CSharpSymbol)))
						return false;

					break;

				case Node.Literal { IgnoreCase: false, Text.Length: 1 }:
					break;

				case Node.Call(var rule, _) when inPlace(rule):
					if (entered.Add(rule))
						pending.Push(graph.Bodies[rule]);

					break;

				case Node.Sequence(var nodes) when nodes.Count == 1:
					pending.Push(nodes[0]);

					break;

				case Node.Choice(var alternatives) { Selection: null }:
					foreach (var alternative in alternatives)
						pending.Push(alternative);

					break;

				default:
					return false;
			}
		}

		return true;
	}

	/// <summary>
	/// How large a body may be, in nodes, and still be written where it is called.
	/// </summary>
	/// <remarks>
	/// Measured rather than chosen: across the three parsers in this repository the rules
	/// this admits have a median of four to six nodes, and the ones worth keeping a
	/// boundary for stand well clear — 42 for the keyword list of
	/// <c>ExpressionParser</c>, 92 for <c>Rfc3986</c>'s IPv6 address, 228 and 285 for
	/// standard SQL's data types and reserved words. There is no continuum here to cut in
	/// the middle of; there are small helpers and there are tables.
	/// </remarks>
	const int Copied = 64;

	/// <summary>Works out the plan for a graph.</summary>
	public static ExecutionPlan Of(RecognitionGraph graph)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var inPlace = new HashSet<RuleSymbol>();

		foreach (var rule in graph.Rules)
			if (!graph.Types.ContainsKey(rule)                &&
				 graph.Results[rule].Count == 0               &&
				!graph.Recursive.Contains(rule)               &&
				!graph.Climbing.ContainsKey(rule)             &&
				!graph.Lifted.ContainsKey(rule)               &&
				 graph.Bodies.TryGetValue(rule, out var body) &&
				!NodeWalk.Descendants(body).Any(node => node is Node.Capture or Node.Construct or Node.Bind) &&
				 NodeWalk.Descendants(body).Take(Copied + 1).Count() <= Copied)
			{
				inPlace.Add(rule);
			}

		return new ExecutionPlan(inPlace);
	}
}
