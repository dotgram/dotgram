using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;

namespace DotGram.Grammar;

/// <summary>
/// <c>Std.Nested(open, close)</c>: the one rule of the standard library that a scanner and a
/// lexer read with code of their own rather than with its body (§5.2).
/// </summary>
/// <remarks>
/// <para>
/// The rule reaches itself, so neither a scanner, which commits to a first reading by
/// inlining what it calls, nor the automaton a lexer runs, which reads regular languages,
/// can read it as written. What it means fits one counter: inside the pair, <c>close</c> is
/// asked first and ends a level, <c>open</c> begins one, and anything else is a character
/// passed over. So where a call to it is compiled into a scanner or into the rest of a token
/// the lexer begins, a fixed loop over that counter is written instead, and everywhere else
/// — the engine, the reader, the buffered input, the editor — the rule's own body is read.
/// </para>
/// <para>
/// The two agree because the body has at most one reading. Its repetition stops only at
/// <c>close</c>, which the guard refuses to pass, or where no turn can be taken, which is
/// the end of the text; and the inner call either reads a balanced pair or reaches the end
/// unclosed, taking the outer one with it. That holds where <c>open</c> and <c>close</c>
/// cannot both begin at one place, which is why the arguments are checked
/// (<see cref="Refusal"/>).
/// </para>
/// <para>
/// Recognized by its declaration, which a specialization keeps, and nothing else: a rule of
/// the same shape the author wrote is read as written.
/// </para>
/// </remarks>
static class NestedPair
{
	/// <summary>The rule's name in the library.</summary>
	public const string Name = "Nested";

	/// <summary>Whether <paramref name="rule"/> is <c>Std.Nested</c> or a specialization of it.</summary>
	public static bool Is(RuleSymbol rule)
	{
		return rule.Declaration is { Name: Name, Params.Count: 2 } &&
			rule.Namespace is { Name: StandardLibrary.Name, IsRoot: true, Parent.Name.Length: 0 };
	}

	/// <summary>What one specialization opens and closes with.</summary>
	public sealed record Delimiters(string Open, bool OpenFolds, string Close, bool CloseFolds)
	{
		/// <summary>
		/// Every character either delimiter can begin with, the folded forms of each included:
		/// the places worth stopping at, and nothing between them can begin either one.
		/// </summary>
		public IReadOnlyList<char> Stops
		{
			get
			{
				var stops = new List<char>();

				foreach (var (text, folds) in new[] { (Open, OpenFolds), (Close, CloseFolds) })
					foreach (var one in Begins(text[0], folds))
						if (!stops.Contains(one))
							stops.Add(one);

				return stops;
			}
		}
	}

	/// <summary>The delimiters of a specialization, read off its body, or null where the arguments do not qualify.</summary>
	public static Delimiters? Of(RuleSymbol rule, RecognitionGraph graph)
	{
		return graph.Bodies.TryGetValue(rule, out var body) ? Of(body) : null;
	}

	/// <inheritdoc cref="Of(RuleSymbol, RecognitionGraph)"/>
	public static Delimiters? Of(Node body)
	{
		return Shape(body) is var (open, close) && Refusal(open, close) is null
			? new Delimiters(open!.Text, open.IgnoreCase, close!.Text, close.IgnoreCase)
			: null;
	}

	/// <summary>
	/// Why the body of a specialization cannot be read with a counter, or null where it can.
	/// </summary>
	/// <remarks>
	/// The counter compares the delimiters at the characters it stops at and has to know which
	/// one stands there: so each is a literal, and at no place can both begin. Two that could
	/// would leave the rule a choice the counter does not make, and equal ones would never let
	/// a pair close.
	/// </remarks>
	public static string? Refusal(Node body)
	{
		var (open, close) = Shape(body);

		return Refusal(open, close);
	}

	static string? Refusal(Node.Literal? open, Node.Literal? close)
	{
		if (open is null || close is null)
			return "both arguments have to be literals";

		if (open.Text.Length == 0 || close.Text.Length == 0)
			return "neither argument may be empty";

		var opens = Begins(open.Text[0], open.IgnoreCase);

		if (Begins(close.Text[0], close.IgnoreCase).Any(opens.Contains))
			return $"\"{open.Text}\" and \"{close.Text}\" can begin at one place";

		return null;
	}

	/// <summary>The literals the body begins and ends with, where it is the shape the library wrote.</summary>
	static (Node.Literal? Open, Node.Literal? Close) Shape(Node body)
	{
		return body is Node.Sequence({ Count: 3 } parts) && parts[1] is Node.Repeat
			? (parts[0] as Node.Literal, parts[2] as Node.Literal)
			: (null, null);
	}

	/// <summary>The characters a literal beginning with <paramref name="first"/> can begin with.</summary>
	/// <remarks>
	/// Wider than the matcher where folding is concerned (<see cref="CaseFold.Of"/> pairs the
	/// Kelvin sign with <c>k</c>, which the matcher's ASCII test does not): a wider set costs a
	/// stop that compares and moves on, and a narrower one would pass a delimiter by.
	/// </remarks>
	static IReadOnlyList<char> Begins(char first, bool folds)
	{
		if (!folds)
			return [first];

		var all = new List<char> { first };

		foreach (var one in CaseFold.Of(first))
			if (!all.Contains(one))
				all.Add(one);

		return all;
	}

	/// <summary>
	/// The loop that reads the rest of a pair, from <paramref name="p"/> just past its
	/// <c>open</c>, to just past the <c>close</c> that balances it.
	/// </summary>
	/// <param name="code">Where it is written; <c>text</c> is the span it reads.</param>
	/// <param name="p">The position, a local the loop moves.</param>
	/// <param name="id">What makes the loop's own locals unique in the method.</param>
	/// <param name="unclosed">
	/// What is done where the text ends inside the pair: <paramref name="p"/> stands at the
	/// position the last stop was found at, and the lines must leave the loop.
	/// </param>
	public static void WriteRest(Writer code, Delimiters pair, string p, string id, IReadOnlyList<string> unclosed)
	{
		var stops = pair.Stops;
		var depth = "depth" + id;
		var next  = "next" + id;

		var search = stops.Count <= 3
			? $"global::System.MemoryExtensions.IndexOfAny(text.Slice({p}), {string.Join(", ", stops.Select(CSharpEmitter.Char))})"
			: $"global::System.MemoryExtensions.IndexOfAny(text.Slice({p}), global::System.MemoryExtensions.AsSpan({CSharpEmitter.Quoted(new string(stops.ToArray()))}))";

		code.Line($"var {depth} = 1;");
		code.Line();

		using (code.Block("while (true)"))
		{
			code.Line($"var {next} = {search};");
			code.Line();

			using (code.Block($"if ({next} < 0)"))
			{
				foreach (var line in unclosed)
					code.Line(line);
			}

			code.Line();
			code.Line($"{p} += {next};");
			code.Line();

			// `close` first, as the rule's guard asks it first; the two cannot both stand here.
			using (code.Block($"if ({At(pair.Close, pair.CloseFolds, p)})"))
			{
				code.Line($"{p} += {pair.Close.Length};");
				code.Line();
				code.Line($"if (--{depth} == 0)");
				code.Then("break;");
			}

			code.Line($"else if ({At(pair.Open, pair.OpenFolds, p)})");

			using (code.Block(""))
			{
				code.Line($"{p} += {pair.Open.Length};");
				code.Line($"{depth}++;");
			}

			code.Line("else");
			code.Then($"{p}++;");
		}
	}

	/// <summary>The test that <paramref name="literal"/> stands at <paramref name="p"/>.</summary>
	static string At(string literal, bool folds, string p)
	{
		var tests = new List<string>();

		if (literal.Length > 1)
			tests.Add($"text.Length - {p} >= {literal.Length}");

		for (var i = 0; i < literal.Length; i++)
		{
			var read = i == 0 ? $"text[{p}]" : $"text[{p} + {i}]";

			tests.Add(folds
				? $"!({CSharpEmitter.Differs(read, literal[i], true)})"
				: $"{read} == {CSharpEmitter.Char(literal[i])}");
		}

		return string.Join(" && ", tests);
	}
}
