using System.Collections.Generic;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Every region a publication of this graph can reach.
	/// </summary>
	/// <remarks>
	/// Not yet read by anything: <c>Compile</c> still asks <see cref="Silent"/>,
	/// <see cref="Possessive"/> and <c>Deterministic</c> itself, threading <c>following</c>
	/// down the tree the way it always has. What is here is the classification those already
	/// make, gathered under the region each node, following and commitment belongs to, so
	/// that a later step can ask it instead of asking twice.
	/// </remarks>
	IReadOnlyCollection<Region> ComputeRegions() =>
		Regions.Of(
			_graph,
			(node, following, committed) => new DecisionClass(
				Silent(node, following),
				node is Node.Repeat(var body, _, _) && Possessive(body, following),
				Deterministic(node, [], following),
				committed),
			alternatives => Predictive(alternatives) is not null);
}
