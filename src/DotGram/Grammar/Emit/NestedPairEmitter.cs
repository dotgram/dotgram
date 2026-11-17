using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The loop that reads <c>Std.Nested(open, close)</c> where a scanner or a lexer reads it with code of
/// its own (<see cref="NestedPair"/>): the grammar side says what the pair is and where it may stop,
/// and this writes the C# that searches for the next stop and counts the levels.
/// </summary>
static partial class CSharpEmitter
{
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
	/// <param name="starves">
	/// Whether a window may feed what is written: a delimiter that does not fit before the end of the
	/// text is then one the end may have cut, and its room test says so, as every test of the end in such
	/// code does (Machine.Ran).
	/// </param>
	internal static void WriteNestedRest(Writer code, NestedPair.Delimiters pair, string p, string id, IReadOnlyList<string> unclosed, bool starves = false)
	{
		var stops = pair.Stops;
		var depth = "depth" + id;
		var next  = "next" + id;

		var search = stops.Count <= 3
			? $"global::System.MemoryExtensions.IndexOfAny(text.Slice({p}), {string.Join(", ", stops.Select(Char))})"
			: $"global::System.MemoryExtensions.IndexOfAny(text.Slice({p}), global::System.MemoryExtensions.AsSpan({Quoted(new string(stops.ToArray()))}))";

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
			using (code.Block($"if ({NestedAt(pair.Close, pair.CloseFolds, p, starves)})"))
			{
				code.Line($"{p} += {pair.Close.Length};");
				code.Line();
				code.Line($"if (--{depth} == 0)");
				code.Then("break;");
			}

			code.Line($"else if ({NestedAt(pair.Open, pair.OpenFolds, p, starves)})");

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
	static string NestedAt(string literal, bool folds, string p, bool starves)
	{
		var tests = new List<string>();

		if (literal.Length > 1)
			tests.Add(starves ? $"(text.Length - {p} >= {literal.Length} || !(failure.Starved = true))" : $"text.Length - {p} >= {literal.Length}");

		for (var i = 0; i < literal.Length; i++)
		{
			var read = i == 0 ? $"text[{p}]" : $"text[{p} + {i}]";

			tests.Add(folds
				? $"!({Differs(read, literal[i], true)})"
				: $"{read} == {Char(literal[i])}");
		}

		return string.Join(" && ", tests);
	}
}
