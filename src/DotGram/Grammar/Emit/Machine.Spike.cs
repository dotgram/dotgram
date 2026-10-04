using System.Linq;
using System.Text;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>SPIKE: the machine's rule count, back edges, memo slots and MemoWords, one per line.</summary>
	internal string SpikeDump()
	{
		var text = new StringBuilder();

		text.Append("rules ").Append(_rules.Count).Append(" direct ").Append(_directRules.Count).Append('\n');
		text.Append("memoWords ").Append(MemoWords).Append(" memo ").Append(_memo.Count).Append('\n');

		foreach (var line in _backEdges.Select(edge => "back " + CSharpEmitter.IdentifierOf(edge.From) + " -> " + CSharpEmitter.IdentifierOf(edge.To)).OrderBy(line => line, System.StringComparer.Ordinal))
			text.Append(line).Append('\n');

		foreach (var pair in _memo.OrderBy(pair => pair.Value))
			text.Append("memo ").Append(pair.Value).Append(' ').Append(CSharpEmitter.IdentifierOf(pair.Key)).Append('\n');

		foreach (var rule in _directRules)
			text.Append("order ").Append(CSharpEmitter.IdentifierOf(rule)).Append('\n');

		return text.ToString();
	}
}
