using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// What one more term of a given shape costs a reading over kinds, by hand and generated,
/// and how much of that is reading the tokens rather than the grammar.
/// </summary>
/// <remarks>
/// <para>
/// The same method the character path was taken apart with: one shape of term, at two
/// lengths, and the difference over the count is what one more of that term costs. A ratio
/// over a whole input mixes a fixed cost, a shape mixture and a size, and moves when any of
/// the three does; a slope is one shape and says what it is worth.
/// </para>
/// <para>
/// The lexer is measured by <see cref="SqlAgainst.Lexers"/>' trick: a `)` in front is
/// refused at the first token, so the parse is the entry and one refusal — but the input
/// was tokenized all the same. Over a slope the entry cancels, so what is left is the
/// generated lexer against the hand-written one, per term.
/// </para>
/// <para>
/// <b>The steadiest reading here is the generated lexer against itself.</b> The two
/// shapes that differ only in the length of one name, and the two that differ only in
/// the spaces between three tokens, say what the token machine costs a character and
/// what the seam costs one — both inside the same file, so nothing about the other
/// parser enters into it. The hand-written lexer is the noisiest column, being the
/// cheapest thing measured: it moves by a fifth between runs where the generated
/// columns hold to a per cent.
/// </para>
/// <para>
/// Each reading is timed in a loop of its own and the whole thing wants
/// <c>DOTNET_TieredCompilation=0</c>: interleaved, or with tiering on, every row moves by
/// half between runs.
/// </para>
/// </remarks>
static class SqlSlope
{
	const int Few  = 32;
	const int Many = 160;

	static readonly (string Name, string Term)[] Shapes =
	[
		("a name and a number", "a{0} = 1"),
		("  and one operator",  "a{0} = 1 + 2"),
		("  and two more",      "a{0} = 1 + 2 + 3 + 4"),
		("parenthesized",       "(a{0} + b) * c > d"),
		("a null test",         "a{0} IS NOT NULL"),
		("a list of three",     "a{0} IN (1, 2, 3)"),
		("a cast",              "CAST(a{0} AS INTEGER) = 5"),
		("a string",            "a{0} LIKE 'abc'"),

		// The same three tokens with twenty-two more characters in the first, so that what
		// a character costs can be told from what a token costs.
		("a short name",        "a{0} = 1"),
		("a long one",          "averyveryverylongname{0} = 1"),

		// And the same three tokens with six more spaces between them, so that what the
		// seam costs a character can be told from what the token machine costs one.
		("  spaced out",        "a{0}    =    1"),

		// What a person actually writes, for the shapes above are all short on purpose.
		("named as people do", "customer_id{0} = 42"),
		("and a string",       "order_status{0} = 'SHIPPED'"),
		("a whole predicate",  "invoice_total{0} BETWEEN 1000 AND 20000"),

		// Heavier terms, where the parse is most of the term and the lexer's share of the
		// noise is small. What the reading over kinds costs is what these say.
		("a list of eight",    "a{0} IN (1, 2, 3, 4, 5, 6, 7, 8)"),
		("nested five deep",   "((((a{0} + 1) * 2) - 3) / 4) > 0"),
		("a case of three",    "CASE WHEN a{0} > 1 THEN 2 WHEN a{0} > 3 THEN 4 ELSE 5 END = 6"),
		("a chain of eight",   "a{0} + b + c + d + e + f + g + h > 0"),
	];

	public static void Run(int parses)
	{
		foreach (var (_, term) in Shapes)
			foreach (var terms in new[] { Few, Many })
			{
				var input = Input(term, terms);

				for (var i = 0; i < 200; i++)
				{
					HandSqlTokens.Build(input);
					HandSqlTokens.LexOnly(input);
					SqlStandard92.TryParseSearchCondition(input);
					ImmediateSql.TryParseSearchCondition(input);
					SqlStandard92.TryParseSearchCondition(")" + input);
				}
			}

		foreach (var (name, term) in Shapes)
		{
			var input = Input(term, Few);

			if (HandSqlTokens.Build(input) is null || !SqlStandard92.TryParseSearchCondition(input).IsSuccess)
				Console.WriteLine($"!! {name} is not read");
		}

		Console.WriteLine(
			$"{"shape",-21} {"whole",9} {"by hand",9} | {"lexing",9} {"by hand",9} | " +
			$"{"parsing",9} {"by hand",9}   whole   lex   parse");

		foreach (var (name, term) in Shapes)
		{
			var one = Cost(term, Few,  parses);
			var two = Cost(term, Many, parses);

			var over  = (double)(Many - Few);
			var hand  = (two.Hand  - one.Hand ) / over * 1000;
			var tape  = (two.Tape  - one.Tape ) / over * 1000;
			var now   = (two.Now   - one.Now  ) / over * 1000;
			var lexed = (two.Lexed - one.Lexed) / over * 1000;
			var mine  = (two.Mine  - one.Mine ) / over * 1000;

			// What is left when the tokenizing is taken off each side, which is the half the
			// scanner is not answerable for.
			var read = now  - mine;
			var theirs = hand - lexed;

			Console.WriteLine(
				$"{name,-21} {now,7:F3} us {hand,7:F3} us | {mine,7:F3} us {lexed,7:F3} us | " +
				$"{read,7:F3} us {theirs,7:F3} us  {now / hand,5:F2}x {mine / lexed,5:F2}x {read / theirs,5:F2}x");
		}
	}

	static (double Hand, double Tape, double Now, double Lexed, double Mine) Cost(
		string term, int terms, int parses)
	{
		var input   = Input(term, terms);
		var refused = ")" + input;

		return (Median(parses, () => HandSqlTokens.Build(input)),
		        Median(parses, () => SqlStandard92.TryParseSearchCondition(input)),
		        Median(parses, () => ImmediateSql.TryParseSearchCondition(input)),
		        Median(parses, () => HandSqlTokens.LexOnly(input)),
		        Median(parses, () => SqlStandard92.TryParseSearchCondition(refused)));
	}

	static string Input(string term, int terms) =>
		string.Join(" AND ", Enumerable.Range(0, terms).Select(i => string.Format(term, i)));

	static double Median(int parses, Action once)
	{
		var taken = new double[parses];
		var watch = new Stopwatch();

		for (var i = 0; i < parses; i++)
		{
			watch.Restart();
			once();
			taken[i] = watch.Elapsed.TotalMilliseconds;
		}

		Array.Sort(taken);

		return taken[parses / 2];
	}
}
