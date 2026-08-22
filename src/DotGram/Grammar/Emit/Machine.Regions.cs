using System.Collections.Generic;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Every region a publication of this graph can reach.
	/// </summary>
	/// <remarks>
	/// Not yet read by anything: <c>Compile</c> still asks <see cref="Silent"/> and
	/// <see cref="Possessive"/> itself, threading <c>following</c> down the tree the way it
	/// always has. What is here is the classification those two already make, gathered
	/// under the region each node and following pair belongs to, so that a later step can
	/// ask it instead of asking twice.
	/// </remarks>
	IReadOnlyCollection<Region> ComputeRegions() =>
		Regions.Of(_graph, (node, following) =>
			new DecisionClass(
				Silent(node, following),
				node is Node.Repeat(var body, _, _) && Possessive(body, following)));
}
