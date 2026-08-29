using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Collapsing rules that only pass a value along.
/// </summary>
/// <remarks>
/// <para>
/// A layered grammar reads naturally as a tower — an operand is a guard or a quantified
/// thing, a quantified thing wraps a prefixed thing — and several floors of the tower do
/// nothing but forward: <c>Operand : @T = o: Guard =&gt; @(o) | o: Quantified =&gt; @(o)</c>.
/// Compiled as written, every such floor costs a call frame, a completion, a rule
/// capture, a pass-through construction and a return, per operand, at run time — work a
/// hand-written parser does not do, which under this project's standing rule makes it a
/// proof obligation rather than architecture.
/// </para>
/// <para>
/// The proof is by identity. An alternative that is exactly a captured call handed back
/// unchanged means nothing the call does not already mean, so every call of the rule can
/// stand where the choice of its sources would: <c>e: Operand</c> becomes
/// <c>(e: Guard | e: Quantified)</c>, ordered as written, values flowing from the same
/// producers they always flowed from. §11 is untouched — inlining a choice at its call
/// site is the definition of calling it — and the rule itself stays in the graph for
/// whatever still reaches it by name, unreachable states being the layout's to drop.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Inline every call to a rule whose alternatives only forward another rule's value.
	/// </summary>
	void CollapseTransparent()
	{
		// Whose calls can be replaced, and by the choice of which sources.
		var transparent = new Dictionary<RuleSymbol, IReadOnlyList<RuleSymbol>>();

		foreach (var rule in _rules)
		{
			if (rule.Declaration is not { Params.Count: 0 } ||
				!_types.TryGetValue(rule, out var type))
				continue;

			var sources = new List<RuleSymbol>();

			foreach (var alternative in Alternatives(_bodies[rule]))
			{
				if (alternative is Node.Construct(
						Node.Capture(var name, Node.Call(var source, { Count: 0 })),
						Construction.Expression(var text, _)) &&
					Forwards(text, name) &&
					_types.TryGetValue(source, out var sourceType) &&
					string.Equals(sourceType, type, StringComparison.Ordinal))
				{
					sources.Add(source);

					continue;
				}

				sources = null!;

				break;
			}

			if (sources is { Count: > 0 })
				transparent[rule] = sources;
		}

		if (transparent.Count == 0)
			return;

		// A transparent rule may forward to another. Resolve the chains up front, and a
		// ring — which forwarding alone cannot make terminate — drops out whole.
		foreach (var rule in transparent.Keys.ToList())
		{
			var resolved = new List<RuleSymbol>();
			var ring     = false;

			void Resolve(RuleSymbol at, HashSet<RuleSymbol> path)
			{
				if (!path.Add(at))
				{
					ring = true;

					return;
				}

				if (transparent.TryGetValue(at, out var onward))
					foreach (var source in onward)
						Resolve(source, path);
				else
					resolved.Add(at);

				path.Remove(at);
			}

			Resolve(rule, []);

			if (ring)
				transparent.Remove(rule);
			else
				transparent[rule] = resolved;
		}

		foreach (var rule in _rules)
			_bodies[rule] = Inline(_bodies[rule]);

		// Identity-preserving on purpose: everything before this pass has already keyed
		// facts by node reference — binding powers, sequence captures, recoveries, the loop
		// a fold runs — and a clone of an untouched subtree would orphan them. Only a path
		// that actually holds a replaced call is rebuilt, and what is rebuilt hands its
		// facts on: `Carry` is what a node that is going away owes the one taking its place.
		Node Inline(Node node)
		{
			var inlined = Rebuild(node);

			Carry(node, inlined);

			return inlined;
		}

		Node Rebuild(Node node)
		{
			switch (node)
			{
				// The capture distributes over the sources, so the value keeps flowing
				// from the rule that produced it — which is all the collapsed rule ever
				// said.
				case Node.Capture(var name, Node.Call(var called, { Count: 0 }))
					when transparent.TryGetValue(called, out var sources):
					return sources.Count == 1
						? new Node.Capture(name, CallTo(sources[0], []))
						: new Node.Choice([
							.. sources.Select(source => (Node)new Node.Capture(name, CallTo(source, [])))]);

				case Node.Call(var called, { Count: 0 })
					when transparent.TryGetValue(called, out var sources):
					return sources.Count == 1
						? CallTo(sources[0], [])
						: new Node.Choice([.. sources.Select(source => CallTo(source, []))]);

				case Node.Sequence(var parts):
				{
					var inlined = Rebuilt(parts);

					return inlined is null ? node : new Node.Sequence(inlined);
				}

				case Node.Choice(var alternatives):
				{
					var inlined = Rebuilt(alternatives);

					return inlined is null ? node : new Node.Choice(inlined);
				}

				case Node.Repeat(var body, var min, var max):
					return Inline(body) is var repeated && ReferenceEquals(repeated, body)
						? node
						: new Node.Repeat(repeated, min, max);

				case Node.Atomic(var kept):
					return Inline(kept) is var atomic && ReferenceEquals(atomic, kept)
						? node
						: new Node.Atomic(atomic);

				case Node.Marked(var kept, var text):
					return Inline(kept) is var marked && ReferenceEquals(marked, kept)
						? node
						: new Node.Marked(marked, text);

				case Node.Capture(var name, var captured):
					return Inline(captured) is var inner && ReferenceEquals(inner, captured)
						? node
						: new Node.Capture(name, inner);

				case Node.Construct(var built, var how):
					return Inline(built) is var body2 && ReferenceEquals(body2, built)
						? node
						: new Node.Construct(body2, how);

				case Node.Lookahead(var positive, var seen):
					return Inline(seen) is var looked && ReferenceEquals(looked, seen)
						? node
						: new Node.Lookahead(positive, looked);

				default:
					return node;
			}
		}

		IReadOnlyList<Node>? Rebuilt(IReadOnlyList<Node> nodes)
		{
			List<Node>? rebuilt = null;

			for (var i = 0; i < nodes.Count; i++)
			{
				var inlined = Inline(nodes[i]);

				if (rebuilt is null && !ReferenceEquals(inlined, nodes[i]))
					rebuilt = [.. nodes.Take(i)];

				rebuilt?.Add(inlined);
			}

			return rebuilt;
		}
	}

	/// <summary>Whether the expression hands the capture back and does nothing else.</summary>
	static bool Forwards(string expression, string capture)
	{
		var text = expression.Trim();

		if (text.StartsWith("(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
			text = text[1..^1].Trim();

		return string.Equals(text, capture, StringComparison.Ordinal);
	}
}
