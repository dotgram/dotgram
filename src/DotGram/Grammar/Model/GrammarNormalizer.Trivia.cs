using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Taking the seam back out of the rules that are the seam.
/// </summary>
/// <remarks>
/// <para>
/// §4.5 weaves <c>trivia</c> between the operands of every sequence in a spaced namespace,
/// and the rules <c>trivia</c> is itself made of are sequences in that namespace like any
/// others. So <c>LineComment = "--" &amp; [^ '\n']*</c> comes out as <c>"--" &amp; trivia
/// &amp; [^ '\n']*</c>, and <c>trivia</c> is a choice that holds <c>LineComment</c> — a rule
/// woven with itself.
/// </para>
/// <para>
/// It says nothing. A seam is <c>trivia</c>, <c>trivia</c> matches the empty string, and
/// <c>A &amp; trivia &amp; B</c> accepts exactly what <c>A &amp; B</c> accepts. What it
/// costs is a call at every seam inside every comment and every run of whitespace, compiled
/// into a scanner that then calls itself.
/// </para>
/// <para>
/// And it costs one thing more, which is how it was noticed: it makes trivia's rules
/// irregular. A lexical machine reads its patterns together as one automaton
/// (`docs/lexical-adt-design.md`), and a rule that reaches itself is not a shape a Thompson
/// construction has — so trivia could not be a pattern, and a split grammar had to be handed
/// its tokens with the whitespace already skipped by hand.
/// </para>
/// <para>
/// Only where the seam is nullable, which is where taking it out changes nothing. A grammar
/// whose <c>trivia</c> must match something has said that operands are separated, and that
/// is a statement about its own rules too, however odd it would be to mean it.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	void UnweaveTrivia()
	{
		// Over the rules the model names as trivia, not over the namespaces that name them:
		// two namespaces may name the same rule, and what is unwoven is a rule.
		foreach (var trivia in _model.Trivia.Values.Distinct())
		{
			if (!Nullable(trivia, []))
				continue;

			foreach (var rule in Reaches(trivia))
				if (_bodies.TryGetValue(rule, out var body) && _trivia.TryGetValue(rule, out var seam))
					_bodies[rule] = Unwoven(body, seam);
		}
	}

	/// <summary>Every rule the trivia rule reaches, itself included.</summary>
	IReadOnlyCollection<RuleSymbol> Reaches(RuleSymbol root)
	{
		var reached = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>([root]);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!reached.Add(rule) || !_bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var called, _))
					pending.Push(called);
		}

		return reached;
	}

	/// <summary>Whether a rule can match the empty string.</summary>
	/// <remarks>
	/// Asked of the lowered bodies rather than of <c>FirstSets</c>, because this runs before
	/// the graph exists — and asked with a guard, since a trivia rule reaches itself, which
	/// is the whole reason this pass is here.
	/// </remarks>
	bool Nullable(RuleSymbol rule, HashSet<RuleSymbol> seen) =>
		seen.Add(rule) && _bodies.TryGetValue(rule, out var body) && Nullable(body, seen);

	bool Nullable(Node node, HashSet<RuleSymbol> seen) =>
		node switch
		{
			Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue => true,
			Node.Literal(var text)        => text.Length == 0,
			Node.Element                  => false,
			Node.External                 => false,
			Node.Repeat(_, var min, _)    => min == 0,
			Node.Sequence(var parts)      => parts.All(part => Nullable(part, seen)),
			Node.Choice(var alternatives) => alternatives.Any(one => Nullable(one, seen)),
			Node.Call(var called, _)      => Nullable(called, seen),
			Node.Atomic(var kept)         => Nullable(kept, seen),
			Node.Marked(var kept, _)      => Nullable(kept, seen),
			Node.Capture(_, var held)     => Nullable(held, seen),
			Node.Construct(var built, _)  => Nullable(built, seen),
			_                             => false,
		};

	/// <summary>The same body with every call to the seam taken out.</summary>
	Node Unwoven(Node node, Node seam)
	{
		switch (node)
		{
			case Node.Sequence(var parts):
			{
				var kept = new List<Node>(parts.Count);

				foreach (var part in parts)
					if (!IsSeam(part, seam))
						kept.Add(Unwoven(part, seam));

				return kept.Count switch
				{
					0 => Node.Empty.Instance,
					1 => kept[0],
					_ => new Node.Sequence(kept),
				};
			}

			case Node.Choice(var alternatives):
				return new Node.Choice([.. alternatives.Select(one => Unwoven(one, seam))]);

			case Node.Repeat(var body, var min, var max):
				return new Node.Repeat(Unwoven(body, seam), min, max);

			case Node.Atomic(var kept):
				return new Node.Atomic(Unwoven(kept, seam));

			case Node.Marked(var kept, var text):
				return new Node.Marked(Unwoven(kept, seam), text);

			case Node.Capture(var name, var held):
				return new Node.Capture(name, Unwoven(held, seam));

			case Node.Construct(var built, var how):
				return new Node.Construct(Unwoven(built, seam), how);

			case Node.Lookahead(var positive, var watched):
				return new Node.Lookahead(positive, Unwoven(watched, seam));

			default:
				return node;
		}
	}
}
