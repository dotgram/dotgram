using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DotGram.Tests.RuleValues;

/// <summary>The evaluation of an input: it parses the input by the grammar, then reads the value off the route the parse took.</summary>
sealed partial class Grammar
{
	/// <summary>A parsed region: the expression and where it lies; a call also holds the rule it matched and that rule's tree.</summary>
	sealed class Node(Expr expr, int start, int end)
	{
		public Expr Expr { get; } = expr;

		public int Start { get; } = start;

		public int End { get; } = end;

		public List<Node> Kids { get; } = [];

		public Rule? Rule { get; init; }

		public Node? Body { get; init; }

		/// <summary>The tree is a step of a left-recursive rule.</summary>
		public bool Step { get; init; }
	}

	sealed class Evaluation(Grammar grammar, Mode mode, string input)
	{
		readonly Dictionary<string, string> _bound = [];

		public Expectation Run()
		{
			var start = grammar.Start;
			var root  = Alternatives(start, 0).FirstOrDefault(m => m.End == input.Length);

			if (root is null)
				return new Expectation(Source.NoMatch, null, null, null, start.Groups[0].Row);

			return new Expectation(start.Groups[0].Source, Eval(start, root).Print, null, null, start.Groups[0].Row);
		}

		// ---- Matching: ordered choice and greedy repetition, each given back when what follows fails, and a left-recursive
		// rule as its seed and steps. Every way a piece can match is tried in the order of preference. ----

		IEnumerable<Node> Alternatives(Rule rule, int position)
		{
			if (!rule.LeftRecursive)
			{
				foreach (var alternative in rule.Alternatives)
					foreach (var matched in Match(alternative, position, rule))
						yield return matched;

				yield break;
			}

			foreach (var seed in rule.Seeds)
				foreach (var matched in Match(seed, position, rule))
					foreach (var grown in Grow(matched, rule))
						yield return grown;
		}

		// A step that moves on is taken as often as it can be, and given back one at a time.
		IEnumerable<Node> Grow(Node current, Rule rule)
		{
			foreach (var step in rule.Steps)
				foreach (var next in MatchStep(step, current, rule))
					if (next.End > current.End)
						foreach (var grown in Grow(next, rule))
							yield return grown;

			yield return current;
		}

		IEnumerable<Node> MatchStep(Expr alternative, Node previous, Rule rule)
		{
			var items = ((Sequence)alternative).Items;
			var self  = new Node(items[0], previous.Start, previous.End) { Rule = rule, Body = previous };

			foreach (var kids in MatchItems(items, 1, Seam(previous.End), rule, [self]))
			{
				var whole = new Node(alternative, previous.Start, kids[^1].End) { Step = true };

				whole.Kids.AddRange(kids);

				yield return whole;
			}
		}

		// The ways through items from the given one on, after the kids matched so far.
		IEnumerable<List<Node>> MatchItems(Expr[] items, int index, int at, Rule rule, List<Node> kids)
		{
			if (index == items.Length)
			{
				yield return kids;
				yield break;
			}

			foreach (var inner in Match(items[index], at, rule))
				foreach (var all in MatchItems(items, index + 1, Seam(inner.End), rule, [.. kids, inner]))
					yield return all;
		}

		// Trivia, where the grammar declares it, is read between the operands of a sequence.
		int Seam(int position)
		{
			while (grammar._shape.Trivia && position < input.Length && input[position] == ' ')
				position++;

			return position;
		}

		IEnumerable<Node> Match(Expr expr, int position, Rule rule)
		{
			switch (expr)
			{
				case Literal literal:
					if (position + literal.Text.Length <= input.Length && string.CompareOrdinal(input, position, literal.Text, 0, literal.Text.Length) == 0)
						yield return new Node(expr, position, position + literal.Text.Length);

					break;

				case Lexeme:
					if (Letters(position, 2) is { } end)
						yield return new Node(expr, position, end);

					break;

				case AnyItems any:
					if (position + any.Count <= input.Length)
						yield return new Node(expr, position, position + any.Count);

					break;

				case Guard or SwitchOn:
					yield return new Node(expr, position, position);
					break;

				case TriviaRef:
					yield return new Node(expr, position, Seam(position));
					break;

				case BackRef backRef:
				{
					var text = _bound[backRef.Name];

					if (position + text.Length <= input.Length && string.CompareOrdinal(input, position, text, 0, text.Length) == 0)
						yield return new Node(expr, position, position + text.Length);

					break;
				}

				case Call call:
					if (MatchLeaf(call, position) is { } leaf)
						yield return leaf;

					break;

				case SelfCall:
					foreach (var body in Alternatives(rule, position))
						yield return new Node(expr, position, body.End) { Rule = rule, Body = body };

					break;

				case RuleRef reference:
					foreach (var body in Alternatives(reference.Rule, position))
						yield return new Node(expr, position, body.End) { Rule = reference.Rule, Body = body };

					break;

				case Named named:
					foreach (var inner in Match(named.Inner, position, rule))
						yield return Around(expr, inner);

					break;

				case Bind bind:
					foreach (var inner in Match(bind.Inner, position, rule))
					{
						_bound[bind.Name] = input.Substring(inner.Start, inner.End - inner.Start);

						yield return Around(expr, inner);
					}

					break;

				case Group group:
					foreach (var inner in Match(group.Inner, position, rule))
						yield return inner;

					break;

				case Sequence sequence:
					foreach (var kids in MatchItems(sequence.Items, 0, position, rule, []))
					{
						var whole = new Node(expr, position, kids.Count == 0 ? position : kids[^1].End);

						whole.Kids.AddRange(kids);

						yield return whole;
					}

					break;

				case Choice choice:
					foreach (var alternative in choice.Alternatives)
						foreach (var inner in Match(alternative, position, rule))
							yield return Around(expr, inner);

					break;

				case Wrapped wrapped:
					foreach (var matched in MatchWrapped(wrapped, position, rule))
						yield return matched;

					break;

				default:
					throw new InvalidOperationException(expr.GetType().Name);
			}
		}

		static Node Around(Expr expr, Node inner)
		{
			var node = new Node(expr, inner.Start, inner.End);

			node.Kids.Add(inner);

			return node;
		}

		IEnumerable<Node> MatchWrapped(Wrapped wrapped, int position, Rule rule)
		{
			switch (wrapped.Wrap)
			{
				case Wrap.Optional:
					foreach (var inner in Match(wrapped.Inner, position, rule))
						yield return Around(wrapped, inner);

					yield return new Node(wrapped, position, position);
					break;

				case Wrap.Star or Wrap.Plus:
					foreach (var kids in Turns(wrapped, position, rule, []))
						if (kids.Count > 0 || wrapped.Wrap == Wrap.Star)
						{
							var node = new Node(wrapped, position, kids.Count == 0 ? position : kids[^1].End);

							node.Kids.AddRange(kids);

							yield return node;
						}

					break;

				case Wrap.Zero:
					yield return new Node(wrapped, position, position);
					break;

				case Wrap.Ahead:
					if (Match(wrapped.Inner, position, rule).Any())
						yield return new Node(wrapped, position, position);

					break;

				default:
					if (!Match(wrapped.Inner, position, rule).Any())
						yield return new Node(wrapped, position, position);

					break;
			}
		}

		// The turns of a repetition: as many as can be read first, then one fewer at a time.
		IEnumerable<List<Node>> Turns(Wrapped wrapped, int position, Rule rule, List<Node> kids)
		{
			var at = kids.Count == 0 ? position : kids[^1].End;

			foreach (var turn in Match(wrapped.Inner, at, rule))
				if (turn.End > at)
					foreach (var more in Turns(wrapped, position, rule, [.. kids, turn]))
						yield return more;

			yield return kids;
		}

		int? Letters(int position, int count)
		{
			return Scan(position, count, static c => c is >= 'a' and <= 'z');
		}

		int? Digits(int position, int count)
		{
			return Scan(position, count, static c => c is >= '0' and <= '9');
		}

		int? Scan(int position, int count, Func<char, bool> test)
		{
			if (position + count > input.Length)
				return null;

			for (var i = 0; i < count; i++)
				if (!test(input[position + i]))
					return null;

			return position + count;
		}

		// The shape of what each helper that is a single token reads.
		Node? MatchLeaf(Call call, int position)
		{
			int? end = call.Rule.Name switch
			{
				"Num" or "Dg" => Digits(position, 2),
				"Ch"          => Letters(position, 1),
				"Esc"         => position + 2 <= input.Length && string.CompareOrdinal(input, position, "\\n", 0, 2) == 0 ? position + 2 : null,
				"Pair"        => Digits(position, 2) is { } a && a < input.Length && input[a] == '=' ? Digits(a + 1, 2) : null,
				_             => Letters(position, 2),
			};

			return end is { } e ? new Node(call, position, e) : null;
		}

		// ---- Values ----

		Val Leaf(string name, int start, int end)
		{
			var text = input.Substring(start, end - start);

			return name switch
			{
				"Word"        => Val.Str("W:" + text),
				"Nm" or "Dg"  => Val.Str(text),
				"Num"         => Val.Int(int.Parse(text, CultureInfo.InvariantCulture)),
				"O"           => new Val("object", "O:" + text, "\"O:" + text + "\""),
				"Ch"          => Val.Char('c'),
				"Esc"         => Val.Char('\n'),
				"Bx"          => Val.Host("Box", "box:" + text),
				"Bxa"         => Val.Host("Box", "box:A" + text),
				"Lf"          => Val.Host("Pr", "pr:" + text),
				"Lfa"         => Val.Host("Pr", "pr:A" + text),
				"Item"        => Val.Host("Item", "Item{X=\"" + text + "\"}"),
				"Pair"        => Val.Host("Pair", "Pair{A=" + text.Substring(0, 2) + ",B=" + text.Substring(3, 2) + "}"),
				"Sp"          => Val.Span(start + 100, end + 100),
				"Span" or "Gs" => Val.Span(start, end),
				_             => mode == Mode.B ? Val.Bytes(text) : Val.Str(text),
			};
		}

		/// <summary>The value of a call: a helper's, or the value of the rule it called.</summary>
		Val Value(Node call)
		{
			return call.Expr is Call c ? Leaf(c.Rule.Name, call.Start, call.End) : Eval(call.Rule!, call.Body!);
		}

		Val Eval(Rule rule, Node node)
		{
			switch (rule.Kind)
			{
				case Kind.S:
					return Val.Span(node.Start, node.End);

				case Kind.C:
				{
					var text = new StringBuilder();

					if (rule.OwnText)
						text.Append(input, node.Start, node.End - node.Start);
					else
						Join(node, text);

					return mode == Mode.B ? Val.Bytes(text.ToString()) : Val.Str(text.ToString());
				}

				case Kind.A:
				{
					var items = new List<Val>();

					Collect(node, rule.Type!, items);

					return Val.Array(items);
				}

				default:
					return EvalO(rule, node);
			}
		}

		// Rule C: the pieces of the route, joined. Names change nothing; trivia, lookaheads, {0} and guards give nothing.
		void Join(Node node, StringBuilder into)
		{
			switch (node.Expr)
			{
				case Literal or Lexeme or AnyItems:
					into.Append(input, node.Start, node.End - node.Start);
					break;

				case Call c:
					into.Append(Leaf(c.Rule.Name, node.Start, node.End).Raw);
					break;

				case SelfCall or RuleRef:
					if (node.Rule!.Kind == Kind.Untyped)
						Join(node.Body!, into);
					else
						into.Append(Eval(node.Rule, node.Body!).Raw);

					break;

				case BackRef:
					into.Append(input, node.Start, node.End - node.Start);
					break;

				case Bind:
					Join(node.Kids[0], into);
					break;

				case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero } or Guard or SwitchOn or TriviaRef:
					break;

				default:
					foreach (var kid in node.Kids)
						Join(kid, into);

					break;
			}
		}

		// Rule A: the value of every call of the element type, in order; the value so far of a left-recursive rule is its elements.
		void Collect(Node node, string element, List<Val> into)
		{
			switch (node.Expr)
			{
				case Call c:
					if (LeafInfo(c.Rule.Name).Type == element)
						into.Add(Leaf(c.Rule.Name, node.Start, node.End));

					break;

				case SelfCall:
					into.AddRange(Eval(node.Rule!, node.Body!).Items!);
					break;

				case RuleRef:
					if (node.Rule!.Kind == Kind.Untyped)
						Collect(node.Body!, element, into);
					else if (node.Rule.Type == element && node.Rule.Kind == Kind.A)
						into.AddRange(Eval(node.Rule, node.Body!).Items!);
					else if (node.Rule.Type == element)
						into.Add(Eval(node.Rule, node.Body!));

					break;

				case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero } or Guard or SwitchOn:
					break;

				default:
					foreach (var kid in node.Kids)
						Collect(kid, element, into);

					break;
			}
		}

		// Rule O: the one operand, or the construction the names fill. A step of a left-recursive rule reads the value so far as its call.
		Val EvalO(Rule rule, Node node)
		{
			var decision = rule.Groups[node.Step ? 1 : 0];

			if (decision.Source == Source.Operand)
			{
				var operands = new List<Node>();

				Operands(node, rule, false, operands);

				if (operands.Count != 1)
					throw new InvalidOperationException("the oracle read " + operands.Count + " operands where the rule has one on every route");

				return Value(operands[0]);
			}

			var captured = new Dictionary<string, Val>(StringComparer.Ordinal);

			Captures(node, Parameters(rule.Type), captured);

			switch (rule.Type)
			{
				case "Box":
					if (captured.TryGetValue("inner", out var other) && other.Type != "Box")
						throw new InvalidOperationException("a name that fills a parameter of the type Box holds a " + other.Type);

					return captured.TryGetValue("inner", out var inner)
						? Val.Host("Box", "box:[" + inner.Raw + "]")
						: Val.Host("Box", "box:" + captured["name"].Raw);

				case "Pr":
					return Val.Host("Pr", "pr:" + (captured.TryGetValue("t", out var t) ? t.Raw : "")
						+ (captured.TryGetValue("l", out var l) ? "[" + l.Raw + "]" : "")
						+ (captured.TryGetValue("r", out var r) ? "[" + r.Raw + "]" : ""));

				case "Item":
					return Val.Host("Item", "Item{X=\"" + captured["x"].Raw + "\"}");

				case "Pair":
					return Val.Host("Pair", "Pair{A=" + captured["a"].Raw + ",B=" + captured["b"].Raw + "}");

				default:
					throw new InvalidOperationException("a construction of " + rule.Type);
			}
		}

		void Operands(Node node, Rule rule, bool author, List<Node> into)
		{
			switch (node.Expr)
			{
				case Call or SelfCall or RuleRef:
					if (!author && grammar.IsOperand(node.Expr, rule))
						into.Add(node);

					break;

				case Named named:
					foreach (var kid in node.Kids)
						Operands(kid, rule, author || NamesACall(named.Inner), into);

					break;

				case Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead or Wrap.Zero }:
					break;

				default:
					foreach (var kid in node.Kids)
						Operands(kid, rule, author, into);

					break;
			}
		}

		// A name's capture: the value of the call it names, or the text it matched.
		void Captures(Node node, string[] parameters, Dictionary<string, Val> into)
		{
			switch (node.Expr)
			{
				case Named named when parameters.Contains(named.Name):
				{
					var inner = node.Kids[0];

										while (inner.Expr is Group or Wrapped && inner.Kids.Count == 1)
						inner = inner.Kids[0];

					var text = IsCall(inner.Expr) ? null : input.Substring(inner.Start, inner.End - inner.Start);

					// A text capture repeated by turns is the turns, joined.
					into[named.Name] = text is null ? Value(inner) : Val.Str(into.TryGetValue(named.Name, out var earlier) && earlier.Type == "string" ? earlier.Raw + text : text);
					break;
				}

				// A name written in X{0} fills its parameter with an empty text.
				case Wrapped { Wrap: Wrap.Zero } zero:
					foreach (var named in Walk([zero.Inner], true).OfType<Named>())
						if (parameters.Contains(named.Name))
							into[named.Name] = Val.Str("");

					break;

				case Call or SelfCall or RuleRef or Wrapped { Wrap: Wrap.Ahead or Wrap.NotAhead }:
					break;

				default:
					foreach (var kid in node.Kids)
						Captures(kid, parameters, into);

					break;
			}
		}
	}
}
