using System.Collections.Generic;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

/// <summary>Spike: the events a trace build writes into a reader.</summary>
sealed partial class Machine
{
	Dictionary<RuleSymbol, int>? _traceIds;

	/// <summary>A rule's index in the grammar's own order, the id its events carry.</summary>
	internal int TraceIdOf(RuleSymbol rule)
	{
		if (_traceIds is null)
		{
			_traceIds = new Dictionary<RuleSymbol, int>();

			for (var at = 0; at < _graph.Rules.Count; at++)
				_traceIds[_graph.Rules[at]] = at;
		}

		return _traceIds.TryGetValue(rule, out var id) ? id : -1;
	}

	/// <summary>0 a rule with a reader of its own, 1 a trivial leaf (a token or a few).</summary>
	internal int TraceKindOf(RuleSymbol rule)
	{
		return _graph.Bodies.ContainsKey(rule) && Trivial(rule) ? 1 : 0;
	}

	static readonly Regex Returned = new(@"\breturn ([^;]+);", RegexOptions.Compiled);

	/// <summary>Every <c>return e;</c> of a rule's body, reporting <c>e</c> as where the rule ended.</summary>
	static string TracedReturns(string body, int id)
	{
		return Returned.Replace(body, match =>
		{
			var value = match.Groups[1].Value;

			return value == "-1"
				? $"{{ failure.Trace?.Exit({id}, pos, -1); return -1; }}"
				: $"{{ var traceEnd_DotGram = {value}; failure.Trace?.Exit({id}, pos, traceEnd_DotGram); return traceEnd_DotGram; }}";
		});
	}
}
