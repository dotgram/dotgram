using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	// A first conservative proof: the complete result is one unbounded repetition
	// of a non-nullable rule. There is no suffix or alternative that could reclaim
	// a yielded element, and no collection factory whose work would be bypassed.
	void LowerYieldPublications()
	{
		var publications = new List<Publication>(_publications);
		for (var i = 0; i < _publications.Count; i++)
		{
			var publication = _publications[i];
			if (!publication.Yield) continue;
			var root = publication.Rule;
			string? reason = null;
			if (publication.Kind != PublishKind.Parse)
				reason = "'yield' is a modifier of 'parse'; 'find' already yields matches.";
			else if (!_types.TryGetValue(root, out var result) || !result.EndsWith("[]", StringComparison.Ordinal))
				reason = "The published rule must produce a sequence declared as '@T[]'.";

			Node? body = null;
			var seen = new HashSet<RuleSymbol>();
			var owner = root;
			while (reason is null)
			{
				if (!seen.Add(owner) || !_bodies.TryGetValue(owner, out body))
				{
					reason = "A recursive wrapper cannot be decomposed into yielded elements.";
					break;
				}
				if (_trivia.ContainsKey(owner))
				{
					reason = "Yielding a collection with implicit trivia is not supported yet; put delimiters in the element rule.";
					break;
				}
				var collects = false;
				while (true)
				{
					if (body is Node.Atomic atomic) body = atomic.Body;
					else if (body is Node.Capture capture) body = capture.Body;
					else if (body is Node.Construct { How: Construction.Sequence or Construction.Operand } constructed)
					{
						collects |= constructed.How is Construction.Sequence;
						body = constructed.Body;
					}
					else break;
				}
				if (body is Node.Call wrapper)
				{
					if (collects)
					{
						reason = "A collection around another collection is not a transparent wrapper. Use a rule-typed forwarding wrapper.";
						break;
					}
					owner = wrapper.Rule;
					continue;
				}
				break;
			}

			Node? item = body is Node.Repeat repeated ? repeated.Body : null;
			while (item is Node.Capture capture) item = capture.Body;
			if (reason is null && (body is not Node.Repeat { Min: 0 or 1, Max: null } || item is not Node.Call))
				reason = "Yield currently requires a complete Rule* or Rule+ sequence. Collection factories, choices and suffixes cannot be yielded safely by this implementation.";
			if (reason is null && body is Node.Repeat repeat && item is Node.Call call)
			{
				var element = call.Rule;
				var collectedType = _types[root].Substring(0, _types[root].Length - 2);
				var type = publication.YieldType ?? new TypeRef(true, collectedType, false, publication.At);
				if (_recoveries.ContainsKey(repeat))
					reason = "Yield with recovery is not supported yet.";
				else if ((_nullable.TryGetValue(element, out var nullable) && nullable) || element.GivesBack)
					reason = "A yielded element must consume input and must not give back a successful match.";
				else if (!type.IsCSharp || type.IsSequence)
					reason = "The yield element type must be a C# type written as '@T'.";
				else if (!_types.TryGetValue(element, out var actual) || !_resolver.IsAssignable(actual, collectedType))
					reason = $"The result of '{element.Name}' is not an element of the published collection '{collectedType}[]'.";
				else if (!_resolver.IsAssignable(actual, type.Name))
					reason = $"The result of '{element.Name}' is not assignable to the yield type '{type.Name}'.";
				else
					publications[i] = publication with { Kind = PublishKind.Yield, Rule = element, YieldType = type, YieldMinimum = repeat.Min };
			}
			if (reason is not null)
				_diagnostics.Add(new GramDiagnostic(UnsafeYield, reason, publication.At.Position, publication.At.Length, GramSeverity.Error));
		}
		_publications = publications;
	}
}
