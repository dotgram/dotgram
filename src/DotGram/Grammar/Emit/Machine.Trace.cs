using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What a trace build writes into the engine and into a flat method (GramCompilerOptions.Trace):
/// where a reading begins on the engine, a rule read by a scanner, a guard asked.
/// </summary>
/// <remarks>
/// <para>
/// The engine's rules have no methods to enter and leave. A call is an entry on the arena, and
/// what happens to the entry is what happens to the rule: pushed, it is entered; at
/// <c>Return:</c> it is left with the end; popped by <c>Fail:</c> while it is the call open,
/// it failed, and popped after it returned, what it read is taken back. A way back resumed in a
/// call that has returned goes on inside it, so the call is taken back and entered again. The
/// reading's <c>GramRead</c> keeps, by an entry's index, the rule it stands for and whether it is
/// open, which is all the four need.
/// </para>
/// <para>
/// A rule the engine inlines into its caller has no entry, and a rule a scanner reads within
/// another is code of that one: neither reports anything. A rule called through a scanner is
/// entered and left where the scan began and ended.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The rules published from this machine, by the states a reading of one begins at.</summary>
	readonly Dictionary<int, RuleSymbol> _tracedRoots = [];

	/// <summary>
	/// The reading's first entry, the call the engine begins with, told: the rule published at the
	/// state the reading begins at, or no rule where that is not one — the trivia before one, a
	/// node read on its own.
	/// </summary>
	void TraceRoot(Writer file)
	{
		if (Tracing is null)
			return;

		var roots = _tracedRoots
			.Select(pair => (State: Numbered(pair.Key), Rule: Tracing.RuleOf(pair.Value)))
			.Distinct()
			.OrderBy(static one => one.State)
			.ToList();

		if (roots.Count == 0)
		{
			file.Line("failure.Trace?.Called(0, -1, -1, pos);");

			return;
		}

		using (file.Block("if (failure.Trace != null)"))
		{
			using (file.Block("switch (state)"))
			{
				foreach (var (state, rule) in roots)
					file.Line($"case {state}: failure.Trace.Called(0, -1, {rule}, pos); break;");

				file.Line("default: failure.Trace.Called(0, -1, -1, pos); break;");
			}
		}
	}

	/// <summary>A scanner's call, wrapped where the build traces so that the rule it reads is entered and left.</summary>
	string Scanning(RuleSymbol rule, string call)
	{
		if (Tracing is null)
			return call;

		Tracing.Scans = true;

		return $"Scanned_DotGram(ref failure, {Tracing.RuleOf(rule)}, p, {call})";
	}
}
