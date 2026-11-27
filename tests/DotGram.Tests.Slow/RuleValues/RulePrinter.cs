using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// How a grammar is published: default (D), with <c>UseSpanCaptures</c> (S), or as a <c>stream bytes</c> parser (B).
/// </summary>
enum Mode
{
	/// <summary>The default string parser.</summary>
	D,

	/// <summary><c>UseSpanCaptures = true</c>: raw captures are <c>ReadOnlySpan&lt;char&gt;</c>.</summary>
	S,

	/// <summary><c>parse Start as By stream bytes</c>, parsed through a <c>Stream</c> of UTF-8.</summary>
	B,
}

/// <summary>
/// One operand call a route reads, outside lookaheads and <c>{0}</c>, with the input it reads.
/// </summary>
/// <param name="Node">The call in the shape's tree (a <see cref="Call"/>, <see cref="SelfCall"/> or <see cref="Parameter"/>).</param>
/// <param name="Text">The input that call reads.</param>
sealed record Read(Expr Node, string Text);

/// <summary>
/// One input of the published rule, and the calls of the rule under test it reads, in order. A rule under test reached
/// through a caller (<see cref="SeenFrom"/>) is read once, and its reads are those of that one reading.
/// </summary>
/// <param name="Input">The text parsed.</param>
/// <param name="Reads">The calls of the rule under test's body that this input goes through, in order.</param>
sealed record Route(string Input, ImmutableArray<Read> Reads);

/// <summary>
/// A shape written out: the grammar's rules, the host members, and one input per route.
/// </summary>
sealed class PrintedGrammar
{
	/// <summary>The grammar's rules, one per line, the published rule first; no publication.</summary>
	public required string Rules { get; init; }

	/// <summary>The inputs, one per route of the published rule (at most <see cref="RulePrinter.MaxRoutes"/>).</summary>
	public required ImmutableArray<Route> Routes { get; init; }

	/// <summary>The grammar is published with <c>Lexical = true</c>.</summary>
	public required bool Lexical { get; init; }

	/// <summary>The grammar uses the generated <c>SourceSpan</c> in host code.</summary>
	public required bool UsesShift { get; init; }

	/// <summary>The whole host source for a mode: the <c>[Gram]</c> attribute over a class of the given name.</summary>
	public string Host(Mode mode, string className)
	{
		var text = new StringBuilder();

		text.Append("[DotGram.Gram(\"\"\"\n");
		text.Append(Rules);
		text.Append("parse Start\n");

		if (mode == Mode.B)
			text.Append("parse Start as By stream bytes\n");

		text.Append("\"\"\"");

		if (mode == Mode.S)
			text.Append(", UseSpanCaptures = true");

		if (Lexical)
			text.Append(", Lexical = true");

		text.Append(")]\npublic partial class ").Append(className).Append("\n{\n");
		text.Append(RulePrinter.HostMembers);

		if (UsesShift)
			text.Append("\tstatic SourceSpan Shift(SourceSpan s) { return new SourceSpan(s.Start + 100, s.Length); }\n");

		text.Append("}\n");

		return text.ToString();
	}
}

/// <summary>
/// Writes a shape as grammar text and works out its inputs.
/// </summary>
static class RulePrinter
{
	/// <summary>The most inputs a grammar is parsed with.</summary>
	public const int MaxRoutes = 6;

	/// <summary>
	/// What every host class holds: <c>T</c> makes a string of any capture, <c>N</c> a number, <c>Seen</c> and
	/// <c>Pick</c> read any name, and <c>Box</c> and <c>Pr</c> are the host types (Box's constructors include a tie on
	/// <c>value</c>).
	/// </summary>
	public const string HostMembers =
		"\tstatic string T(string s) { return s; }\n" +
		"\tstatic string T(System.ReadOnlySpan<char> s) { return s.ToString(); }\n" +
		"\tstatic string T(System.ReadOnlySpan<byte> s) { return System.Text.Encoding.UTF8.GetString(s); }\n" +
		"\tstatic int N(string s) { return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture); }\n" +
		"\tstatic int N(System.ReadOnlySpan<char> s) { return N(s.ToString()); }\n" +
		"\tstatic int N(System.ReadOnlySpan<byte> s) { return N(T(s)); }\n" +
		"\tstatic bool Seen(object? v) { return true; }\n" +
		"\tstatic bool Seen(System.ReadOnlySpan<char> v) { return true; }\n" +
		"\tstatic bool Seen(System.ReadOnlySpan<byte> v) { return true; }\n" +
		"\tstatic int Pick(object? v) { return 1; }\n" +
		"\tstatic int Pick(System.ReadOnlySpan<char> v) { return 1; }\n" +
		"\tstatic int Pick(System.ReadOnlySpan<byte> v) { return 1; }\n" +
		"\tpublic sealed class Box\n" +
		"\t{\n" +
		"\t\tpublic readonly string V;\n" +
		"\t\tpublic Box(string name) { V = \"box:\" + name; }\n" +
		"\t\tpublic Box(Box inner) { V = \"box:[\" + inner.V + \"]\"; }\n" +
		"\t\tpublic Box(int value) { V = \"box:int\" + value; }\n" +
		"\t\tpublic Box(char value) { V = \"box:char\" + value; }\n" +
		"\t\tpublic override string ToString() { return V; }\n" +
		"\t}\n" +
		"\tpublic sealed class Pr\n" +
		"\t{\n" +
		"\t\tpublic Pr? L { get; set; }\n" +
		"\t\tpublic Pr? R { get; set; }\n" +
		"\t\tpublic string? T { get; set; }\n" +
		"\t\tpublic override string ToString() { return \"pr:\" + T + (L is null ? \"\" : \"[\" + L + \"]\") + (R is null ? \"\" : \"[\" + R + \"]\"); }\n" +
		"\t}\n";

	/// <summary>Writes the shape.</summary>
	public static PrintedGrammar Print(RuleShape shape)
	{
		var rules  = new StringBuilder();
		var called = new SortedSet<string>(StringComparer.Ordinal);
		var name   = shape.RuleName;
		var self   = shape.Template ? "P(x)" : name;
		var type   = shape.Type;

		string Body()
		{
			return string.Join(" | ", shape.Alternatives.Select(a => Write(a, self, called)));
		}

		string Typed(string rule)
		{
			return type.Text is null ? rule : rule + " : " + type.Text;
		}

		switch (shape.SeenFrom)
		{
			case SeenFrom.Itself when shape.Template:
				rules.Append("Start : ").Append(type.Text).Append(" = P(").Append(type.Taken!.Name).Append(")\n");
				rules.Append("P(x) : x = ").Append(Body()).Append('\n');
				called.Add(type.Taken.Name);
				break;

			case SeenFrom.Itself:
				rules.Append(Typed("Start")).Append(" = ").Append(Body()).Append('\n');
				break;

			case SeenFrom.InSequence:
			{
				var beside = BesideInSequence(type);

				rules.Append("Start : @").Append(type.ClrType).Append("[] = R & ';' & ").Append(beside.Name).Append('\n');
				rules.Append(Typed("R")).Append(" = ").Append(Body()).Append('\n');
				called.Add(beside.Name);
				break;
			}

			case SeenFrom.TakenBy:
				rules.Append("Start : R = '[' & R & ']'\n");
				rules.Append(Typed("R")).Append(" = ").Append(Body()).Append('\n');
				break;

			case SeenFrom.BesideFilling:
				rules.Append("Start : R = ").Append(string.Join(" & ", RuleShapes.Filling(type.Family)!.Select(e => Write(e, self, called)))).Append(" & R\n");
				rules.Append(Typed("R")).Append(" = ").Append(Body()).Append('\n');
				break;
		}

		if (type.Taken is { } taken)
			called.Add(taken.Name);

		// The helpers the grammar calls, and the ones they call, each once.
		var pending = new Queue<string>(called);

		while (pending.Count > 0)
			foreach (var inner in Helper.All[pending.Dequeue()].Calls)
				if (called.Add(inner))
					pending.Enqueue(inner);

		var text = rules.ToString();

		if (shape.Lexical)
		{
			// Over tokens the terminals are a namespace of their own, with no trivia between their characters.
			text = "using Lexical;\ntrivia = { [' ']* }\n" + text
				+ "namespace Lexical\n{\n\ttrivia = none\n"
				+ string.Concat(called.Select(static h => "\t" + Helper.All[h].Definition + "\n")) + "}\n";
		}
		else
		{
			text += string.Concat(called.Select(static h => Helper.All[h].Definition + "\n")) + (shape.Trivia ? "trivia = { [' ']* }\n" : "");
		}

		return new PrintedGrammar
		{
			Rules     = text,
			Lexical   = shape.Lexical,
			Routes    = Routes(shape),
			UsesShift = called.Contains(Helper.Sp.Name),
		};
	}

	/// <summary>The operand a sequence collects beside the rule under test: the family's built rule, or its implicit one.</summary>
	public static Helper BesideInSequence(RuleType type)
	{
		return RuleShapes.AtomFor(type.Family, Atom.Built) ?? RuleShapes.AtomFor(type.Family, Atom.Implicit)!;
	}

	static bool Simple(Expr expr)
	{
		return expr is Literal or Lexeme or Call or SelfCall or Parameter or Group or Choice or AnyItems or BackRef or TriviaRef;
	}

	/// <summary>Writes an expression of the rule under test's body.</summary>
	static string Write(Expr expr, string self, ISet<string> called)
	{
		switch (expr)
		{
			case Literal literal:
				return "'" + literal.Text + "'";

			case AnyItems any:
				return "any{" + any.Count + "}";

			case Lexeme lexeme:
				return lexeme.Pattern;

			case Call call:
				called.Add(call.Rule.Name);
				return call.Rule.Name;

			case SelfCall:
				return self;

			case Parameter:
				return "x";

			case BackRef backRef:
				return "$" + backRef.Name;

			case TriviaRef:
				return "trivia";

			case Bind bind:
				return "$" + bind.Name + ": " + Write(bind.Inner, self, called);

			case Named named:
				return named.Name + ": " + (Simple(named.Inner) || named.Inner is Wrapped ? Write(named.Inner, self, called) : "(" + Write(named.Inner, self, called) + ")");

			case Sequence sequence:
				return string.Join(" & ", sequence.Items.Select(i => i is Sequence ? "(" + Write(i, self, called) + ")" : Write(i, self, called)));

			case Choice choice:
				return "(" + string.Join(" | ", choice.Alternatives.Select(a => Write(a, self, called))) + ")";

			case Group group:
				return "(" + Write(group.Inner, self, called) + ")";

			case Wrapped wrapped:
			{
				// A suffix cannot follow a quantifier: a quantified atom is parenthesised like any other compound.
				var inner = Simple(wrapped.Inner) && wrapped.Inner is not Lexeme { Pattern: [.., '}' or '?' or '*' or '+'] } ? Write(wrapped.Inner, self, called) : "(" + Write(wrapped.Inner, self, called) + ")";

				return wrapped.Wrap switch
				{
					Wrap.Optional => inner + "?",
					Wrap.Star     => inner + "*",
					Wrap.Plus     => inner + "+",
					Wrap.Zero     => inner + "{0}",
					Wrap.Ahead    => "?=(" + Write(wrapped.Inner, self, called) + ")",
					_             => "?!(" + Write(wrapped.Inner, self, called) + ")",
				};
			}

			case Guard guard:
				return "when @(" + guard.Code + ")";

			case SwitchOn on:
				return "switch @(Pick(" + on.Reads + ")) { case 1: any{0}; default: any{0} }";

			default:
				throw new InvalidOperationException(expr.GetType().Name);
		}
	}

	/// <summary>A way through an expression: its input and the calls it reads.</summary>
	sealed record Variant(string Text, ImmutableArray<Read> Reads)
	{
		public static readonly Variant Empty = new("", []);

		public Variant Then(Variant next)
		{
			return new Variant(Text + next.Text, Reads.AddRange(next.Reads));
		}
	}

	sealed class Walk(RuleShape shape, Samples samples)
	{
		readonly Dictionary<string, string> _bound = [];

		/// <summary>The input the rule under test reads where it calls itself; null while it is being worked out.</summary>
		public Variant? SelfInput { get; set; }

		public List<Variant> Of(Expr expr)
		{
			switch (expr)
			{
				case Literal literal:
					return [new Variant(literal.Text, [])];

				case Lexeme lexeme:
					return [new Variant(lexeme.Sample(samples), [])];

				case Call call:
				{
					var text = call.Count ? Samples.Count() : call.Rule.Sample(samples);

					// A rule with two routes is read through each of them, one after the other.
					if (call.Rule.Alternate is { } alternate)
					{
						var other = alternate(samples);

						return [new Variant(text, [new Read(call, text)]), new Variant(other, [new Read(call, other)])];
					}

					return [new Variant(text, call.Atom is null ? [] : [new Read(call, text)])];
				}

				case Parameter parameter:
				{
					var text = shape.TemplateArgument!.Sample(samples);

					return [new Variant(text, [new Read(parameter, text)])];
				}

				case SelfCall call:
					return SelfInput is null ? [] : [new Variant(SelfInput.Text, [new Read(call, SelfInput.Text)])];

				case Named named:
					return Of(named.Inner);

				case Bind bind:
				{
					var variants = Of(bind.Inner);

					_bound[bind.Name] = variants[0].Text;

					return variants;
				}

				case BackRef backRef:
					return [new Variant(_bound[backRef.Name], [])];

				case TriviaRef:
					return [new Variant(" ", [])];

				case Group group:
					return Of(group.Inner);

				case Choice choice:
					return [.. choice.Alternatives.SelectMany(Of)];

				case Sequence sequence:
					return OfSequence(sequence.Items);

				case Wrapped { Wrap: Wrap.Optional } optional:
					return [.. Of(optional.Inner), Variant.Empty];

				case Wrapped { Wrap: Wrap.Star } star:
					return [Variant.Empty, .. Of(star.Inner).Select(v => Twice(v, star.Inner)).OfType<Variant>()];

				case Wrapped { Wrap: Wrap.Plus } plus:
					return [.. Of(plus.Inner).Select(v => Twice(v, plus.Inner)).OfType<Variant>()];

				case Wrapped:
				case Guard:
				case SwitchOn:
					return [Variant.Empty];

				default:
					throw new InvalidOperationException(expr.GetType().Name);
			}
		}

		Variant? Twice(Variant first, Expr inner)
		{
			var second = Of(inner).FirstOrDefault();

			return second is null ? null : first.Then(second);
		}

		/// <summary>
		/// The ways through a sequence: every item's first way, then each further way of one item with the first ways of
		/// the rest. A lookahead and the <c>any{n}</c> after it go together: the lookahead's way is the input both read.
		/// </summary>
		List<Variant> OfSequence(Expr[] items)
		{
			var parts = new List<List<Variant>>();

			for (var i = 0; i < items.Length; i++)
			{
				if (items[i] is Wrapped { Wrap: Wrap.Ahead } ahead && i + 1 < items.Length && items[i + 1] is AnyItems)
				{
					// The first way only: a rule with two routes of different width would not match the items after it.
					parts.Add([.. Of(ahead.Inner).Take(1).Select(static v => new Variant(v.Text, []))]);
					i++;
					continue;
				}

				parts.Add(Of(items[i]));
			}

			if (parts.Any(static p => p.Count == 0))
				return [];

			var result = new List<Variant>();

			Variant Combine(int at, Variant chosen)
			{
				var whole = Variant.Empty;

				for (var j = 0; j < parts.Count; j++)
				{
					var part = j == at ? chosen : parts[j][0];

					// Trivia is woven between operands: one space where both sides have text.
					if (shape.Trivia && whole.Text.Length > 0 && part.Text.Length > 0)
						whole = whole.Then(new Variant(" ", []));

					whole = whole.Then(part);
				}

				return whole;
			}

			result.Add(Combine(-1, Variant.Empty));

			for (var j = 0; j < parts.Count; j++)
				foreach (var variant in parts[j].Skip(1))
					result.Add(Combine(j, variant));

			return result;
		}
	}

	/// <summary>The inputs of the published rule.</summary>
	static ImmutableArray<Route> Routes(RuleShape shape)
	{
		var samples = new Samples();
		var walk    = new Walk(shape, samples);

		List<Variant> Body()
		{
			return [.. shape.Alternatives.SelectMany(walk.Of)];
		}

		walk.SelfInput = Body().FirstOrDefault();

		var body   = walk.SelfInput is null ? [] : Body();
		var routes = new List<Route>();

		foreach (var variant in body)
		{
			var input = shape.SeenFrom switch
			{
				SeenFrom.InSequence    => variant.Text + ";" + BesideInSequence(shape.Type).Sample(samples),
				SeenFrom.TakenBy       => "[" + variant.Text + "]",
				SeenFrom.BesideFilling => string.Concat(RuleShapes.Filling(shape.Type.Family)!.Select(e => walk.Of(e)[0].Text)) + variant.Text,
				_                      => variant.Text,
			};

			if (routes.All(r => r.Input != input))
				routes.Add(new Route(input, variant.Reads));

			if (routes.Count == MaxRoutes)
				break;
		}

		return [.. routes];
	}
}
