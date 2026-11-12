using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotGram.Tests;

/// <summary>
/// A plan of independent steps read on several threads, each over one stretch of it, and the stretches
/// handed back in the order of the plan: for the sweeps whose cost is the number of answers asked, one
/// at a time, and whose random draws were made beforehand, so that what is asked does not depend on
/// which thread reads it.
/// </summary>
static class ChunkedWork
{
	/// <summary>How many threads one sweep may use: the rest of the machine is other classes' beside it.</summary>
	public const int Degree = 8;

	/// <summary>The most stretches a plan is cut into.</summary>
	const int Stretches = 256;

	/// <summary>Runs <paramref name="step"/> over every item of <paramref name="plan"/>, a state for each stretch, in the order of the plan.</summary>
	public static TState[] Run<TItem, TState>(IReadOnlyList<TItem> plan, Func<TState> create, Action<TState, TItem> step)
	{
		var count  = Math.Min(plan.Count, Stretches);
		var states = new TState[count];

		Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = Degree }, at =>
		{
			var state = create();

			for (var i = (int)((long)plan.Count * at / count); i < (int)((long)plan.Count * (at + 1) / count); i++)
				step(state, plan[i]);

			states[at] = state;
		});

		return states;
	}
}
