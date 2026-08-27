using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Spacing the turns of a list.
/// </summary>
/// <remarks>
/// <para>
/// §4.5 seams the turns of a repetition whose body is a sequence, and leaves a
/// repetition of a single operand alone — that is how a lexeme is written, and spacing
/// <c>['0'..'9']+</c> would make <c>1 2</c> one number. But a repetition of a rule that
/// builds a value is not the inside of a lexeme: it is a collection, the very shape
/// §4.1 case 2 gathers, and a grammar that separates its operands with trivia expects
/// its collections separated the same way. <c>entries: Entry*</c> must read
/// <c>a; b;</c>, not only <c>a;b;</c>.
/// </para>
/// <para>
/// Valuedness is the line, and it is what makes both halves keep their meaning: a
/// valueless rule is a fragment of text — <c>Name = Letter+</c> stays one word — while
/// a valued one is a thing an author collects, and things are spaced. The line needs
/// the types, so this runs after <c>ComputeTypes</c> rather than inside lowering, and
/// after <c>CollectSequences</c>, whose implicit capture it must see already written.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Seam the turns of every repetition of a valued rule, captured or bare.
	/// </summary>
	void SpaceLists()
	{
		foreach (var rule in _rules)
		{
			// A fold's loop and a climb's calls are node-keyed facts; a rule that owns
			// either keeps its shape. A rule with no seam has nothing to insert.
			if (_folds.ContainsKey(rule) ||
				_climbing.ContainsKey(rule) ||
				!_trivia.TryGetValue(rule, out var seam))
				continue;

			_bodies[rule] = Spaced(_bodies[rule], seam);
		}

		Node Spaced(Node node, Node seam)
		{
			switch (node)
			{
				case Node.Repeat(var turn, var min, var max) repeat
					when max is not (0 or 1) && Listed(turn):
				{
					var spaced = new Node.Repeat(
						new Node.Sequence([seam, Spaced(turn, seam)]), min, max);

					// `recover` was recorded against the node being replaced, and
					// everything downstream looks it up by identity.
					if (_recoveries.TryGetValue(node, out var recovery))
					{
						_recoveries.Remove(node);
						_recoveries[spaced] = recovery;
					}

					return spaced;
				}

				case Node.Sequence(var parts):
				{
					var rebuilt = Rebuilt(parts, seam);

					return rebuilt is null ? node : new Node.Sequence(rebuilt);
				}

				case Node.Choice(var alternatives):
				{
					var rebuilt = Rebuilt(alternatives, seam);

					return rebuilt is null ? node : new Node.Choice(rebuilt);
				}

				case Node.Repeat(var body, var min, var max) other:
					return Spaced(body, seam) is var inner && ReferenceEquals(inner, body)
						? node
						: Rekeyed(other, new Node.Repeat(inner, min, max));

				case Node.Atomic(var kept):
					return Spaced(kept, seam) is var atomic && ReferenceEquals(atomic, kept)
						? node
						: new Node.Atomic(atomic);

				case Node.Capture(var name, var captured):
					return Spaced(captured, seam) is var held && ReferenceEquals(held, captured)
						? node
						: new Node.Capture(name, held);

				case Node.Construct(var built, var how):
					return Spaced(built, seam) is var made && ReferenceEquals(made, built)
						? node
						: new Node.Construct(made, how);

				default:
					return node;
			}
		}

		IReadOnlyList<Node>? Rebuilt(IReadOnlyList<Node> nodes, Node seam)
		{
			List<Node>? rebuilt = null;

			for (var i = 0; i < nodes.Count; i++)
			{
				var spaced = Spaced(nodes[i], seam);

				if (rebuilt is null && !ReferenceEquals(spaced, nodes[i]))
					rebuilt = [.. nodes.Take(i)];

				rebuilt?.Add(spaced);
			}

			return rebuilt;
		}

		Node.Repeat Rekeyed(Node old, Node.Repeat rewritten)
		{
			if (_recoveries.TryGetValue(old, out var recovery))
			{
				_recoveries.Remove(old);
				_recoveries[rewritten] = recovery;
			}

			return rewritten;
		}
	}

	/// <summary>A turn that is a valued rule, captured or bare — a list's element.</summary>
	bool Listed(Node turn) =>
		turn switch
		{
			Node.Call(var called, { Count: 0 })                   => _types.ContainsKey(called),
			Node.Capture(_, Node.Call(var called, { Count: 0 })) => _types.ContainsKey(called),
			_                                                     => false,
		};
}
