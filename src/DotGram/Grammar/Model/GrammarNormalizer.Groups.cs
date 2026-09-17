using System;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// A nested construction owns its captures and value, just like a named rule.
	/// Collection elements take their expected type from the enclosing collection.
	/// </summary>
	void LowerGroupValues()
	{
		for (var i = 0; i < _rules.Count; i++)
		{
			var owner = _rules[i];

			if (!_types.TryGetValue(owner, out var type))
				continue;

			var offered = ValueAlternatives(_bodies[owner]);

			if (!Constructs(_bodies[owner]).Any(node => !offered.Contains(node)))
				continue;

			var expected = type.EndsWith("[]", StringComparison.Ordinal)
				? type.Substring(0, type.Length - 2)
				: type;

			_bodies[owner] = Visit(_bodies[owner], true);

			Node Visit(Node node, bool alternative)
			{
				if (!alternative && ValueAlternatives(node).Any(part => part is Node.Construct))
				{
					var name = owner.Name + "_Group" + _rules.Count;

					while (_rules.Exists(rule => rule.Namespace == owner.Namespace && rule.Name == name))
						name += "_";

					var declaration = owner.Declaration! with
					{
						Name     = name,
						Params   = [],
						Type     = new TypeRef(true, expected, false, owner.Declaration!.At),
						OnFail   = null,
					};
					var group = new RuleSymbol(name, owner.Namespace, declaration);

					_rules.Add(group);
					_bodies[group] = node;
					_types[group] = expected;

					if (_trivia.TryGetValue(owner, out var trivia))
						_trivia[group] = trivia;

					return new Node.Call(group, []);
				}

				var rebuilt = node switch
				{
					Node.Sequence sequence => sequence with { Nodes = sequence.Nodes.Select(part => Visit(part, false)).ToArray() },
					Node.Choice choice     => choice.Rebuild(choice.Nodes.Select(part => Visit(part, alternative)).ToArray()),
					Node.Construct built   => built with { Body = Visit(built.Body, false) },
					Node.Repeat repeat     => repeat with { Body = Visit(repeat.Body, false) },
					Node.Capture capture   => capture with { Body = Visit(capture.Body, false) },
					Node.Atomic atomic     => atomic with { Body = Visit(atomic.Body, alternative) },
					Node.Marked marked     => marked with { Body = Visit(marked.Body, alternative) },
					Node.Lookahead look    => look with { Body = Visit(look.Body, false) },
					_                      => node,
				};

				Carry(node, rebuilt);

				return rebuilt;
			}
		}
	}
}
