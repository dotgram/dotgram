using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using DotGram.Benchmarks;
using DotGram.Sql;
using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The memo of successes (<c>Machine.Memo.cs</c>) held to the SQL grammars as they ship, on the
/// immediate carrier they ship on: the nests an ordered choice read once per alternative at every
/// level — a bracketed condition tried as a value and then as a condition, a row tried as a value
/// and then as a row, a bracketed subquery read and declined — cost their depth in rule entries;
/// and what the parser with the memo reads, refuses and builds is what the parser without it
/// does, answer for answer, over the SQL-92 tests' texts, the corpus statements the parser accepts,
/// a generated corpus of several thousand statements, nests to depth 32 and the T-SQL corpus, and
/// record for record on a located reading. Beside those, for the decision whether a grammar is
/// rewritten to read the shared text once instead, the rewrite's own numbers: how its answers
/// differ from the shipped grammar's, its size and its time.
/// </summary>
/// <remarks>
/// Not Alone: what is held here is counts and answers, which no neighbour changes. The differentials
/// print every kind of difference with samples, and the memo's hold that there is none; they read
/// their answers on several threads, each over a stretch of a plan drawn beforehand from the seed, so
/// that the answers asked, and the order the differences are told in, are what one thread would
/// have asked and told. The timings are <see cref="SqlSuccessMemoTimingTests"/>, which are Alone.
/// </remarks>
public sealed class SqlSuccessMemoTests(ITestOutputHelper output)
{
	void Log(string line)
	{
		output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("SUCCESSMEMO_OUT") is { Length: > 0 } file)
			File.AppendAllText(file, line + "\n");
	}

	/// <summary>Twice the depth may cost at most this many times the entries: linear, with a seventh over for the ends of the text.</summary>
	const double Doubling = 2.14;

	/// <summary>The SQL-92 nests a bracketed value holding a condition made exponential, each a level around <c>{0}</c>, and the shapes that were linear before.</summary>
	static readonly (string Name, string Shape, string Core)[] Sql92Shapes =
	[
		("case-cond",        "(CASE WHEN {0} THEN 1 END = 1)",                          "a = 1"),
		("case-value",       "(CASE WHEN {0} THEN 1 END) = 1",                          "a = 1"),
		("case-dbl-cond",    "((CASE WHEN {0} THEN 1 END) = 1)",                        "a = 1"),
		("case-isnull",      "(CASE WHEN {0} THEN 1 END IS NULL)",                      "a = 1"),
		("case-between",     "(CASE WHEN {0} THEN 1 END BETWEEN 1 AND 2)",              "a = 1"),
		("case-in",          "(CASE WHEN {0} THEN 1 END IN (1, 2))",                    "a = 1"),
		("case-like",        "(CASE WHEN {0} THEN 'a' END LIKE 'a')",                   "a = 1"),
		("case-and",         "(CASE WHEN {0} THEN 1 END = 1 AND b = 1)",                "a = 1"),
		("not-case",         "(NOT CASE WHEN {0} THEN 1 END = 1)",                      "a = 1"),
		("case-exists",      "(CASE WHEN EXISTS (SELECT a FROM t WHERE {0}) THEN 1 END = 1)", "a = 1"),
		("upper-case",       "(UPPER(CASE WHEN {0} THEN 'a' END) = 'a')",               "a = 1"),
		("plus-case",        "(1 + CASE WHEN {0} THEN 1 END = 2)",                      "a = 1"),
		("cast-case",        "(CAST(CASE WHEN {0} THEN 1 END AS INTEGER) = 1)",         "a = 1"),
		("coalesce-case",    "(COALESCE(CASE WHEN {0} THEN 1 END, 0) = 1)",             "a = 1"),
		("subq-cond",        "((SELECT a FROM t WHERE {0}) = 1)",                       "a = 1"),
		("subq-value",       "((SELECT a FROM t WHERE {0})) = 1",                       "a = 1"),
		("exists-cond",      "(EXISTS (SELECT a FROM t WHERE {0}))",                    "a = 1"),
		("in-subq-cond",     "(a IN (SELECT a FROM t WHERE {0}))",                      "a = 1"),
		("in-subq-list",     "(a IN ((SELECT a FROM t WHERE {0}), 2))",                 "a = 1"),
		("subq-union",       "(((SELECT a FROM t WHERE {0}) UNION (SELECT a FROM t)) = 1)", "a = 1"),
		("row-list",         "((CASE WHEN {0} THEN 1 END, 1) = (1, 1))",                "a = 1"),
		("row-nest",         "(({0}, 1) = (1, 1))",                                     "a"),
		("case-cond-unclosed","(CASE WHEN {0}",                                         "a = 1"),
		("case-cond-refused","(CASE WHEN {0} THEN 1 END = 1 x)",                        "a = 1"),
		("plain-brackets",   "({0})",                                                   "a = 1"),
		("value-brackets",   "({0})",                                                   "a"),
	];

	/// <summary>The T-SQL nests: the bracketed condition the exponential reading was found in, the forms around it, and a bracketed subquery in FROM.</summary>
	static readonly (string Name, string Shape, string Core)[] TSqlShapes =
	[
		("case-cond",        "(CASE WHEN {0} THEN 1 END = 1)",                          "a = 1"),
		("case-dbl-cond",    "((CASE WHEN {0} THEN 1 END) = 1)",                        "a = 1"),
		("case-isnull",      "(CASE WHEN {0} THEN 1 END IS NULL)",                      "a = 1"),
		("case-and",         "(CASE WHEN {0} THEN 1 END = 1 AND b = 1)",                "a = 1"),
		("iif-cond",         "(IIF({0}, 1, 0) = 1)",                                    "a = 1"),
		("subq-cond",        "((SELECT a FROM t WHERE {0}) = 1)",                       "a = 1"),
		("subq-value",       "((SELECT a FROM t WHERE {0})) = 1",                       "a = 1"),
		("exists-cond",      "(EXISTS (SELECT a FROM t WHERE {0}))",                    "a = 1"),
		("in-subq-list",     "(a IN ((SELECT a FROM t WHERE {0}), 2))",                 "a = 1"),
		("cast-case",        "(CAST(CASE WHEN {0} THEN 1 END AS INT) = 1)",             "a = 1"),
		("upper-case",       "(UPPER(CASE WHEN {0} THEN 'a' END) = 'a')",               "a = 1"),
		("choose-case",      "(CHOOSE(1, CASE WHEN {0} THEN 1 END) = 1)",               "a = 1"),
		("row-nest",         "(({0}, 1) = (1, 1))",                                     "a"),
		("value-nest",       "({0})",                                                   "a"),
		("case-cond-unclosed","(CASE WHEN {0}",                                         "a = 1"),
		("case-cond-refused","(CASE WHEN {0} THEN 1 END = 1 x)",                        "a = 1"),
	];

	/// <summary>The SQL:2023 nests: the bracketed subquery the bracket reads and declines, and the condition forms.</summary>
	static readonly (string Name, string Shape, string Core)[] StandardShapes =
	[
		("subq-declined",    "((SELECT a FROM t WHERE {0})) = 1",                       "a = 1"),
		("subq-cond",        "((SELECT a FROM t WHERE {0}) = 1)",                       "a = 1"),
		("case-cond",        "(CASE WHEN {0} THEN 1 END = 1)",                          "a = 1"),
		("case-dbl-cond",    "((CASE WHEN {0} THEN 1 END) = 1)",                        "a = 1"),
		("row-nest",         "(({0}, 1) = (1, 1))",                                     "a"),
		("case-cond-unclosed","(CASE WHEN {0}",                                         "a = 1"),
	];

	const string Prefix = "SELECT a FROM t WHERE ";

	static readonly int[] Depths = [4, 8, 16, 32];

	[Fact]
	public void Sql92_nests_cost_their_depth_with_the_memo()
	{
		Ladders("SQL-92", SqlVariants.Sql92, "TryParseSelect", Prefix, Sql92Shapes);
	}

	[Fact]
	public void TSql_nests_cost_their_depth_with_the_memo()
	{
		Ladders("T-SQL", SqlVariants.TransactSql, SqlVariants.Entry(SqlVariants.TransactSql, "TryParseSearchCondition"), "", TSqlShapes);
	}

	[Fact]
	public void Sql2023_nests_cost_their_depth_with_the_memo()
	{
		Ladders("SQL:2023", SqlVariants.Standard, "TryParseSearchCondition", "", StandardShapes);
	}

	/// <summary>
	/// Every shape at four depths a doubling apart, with the memo and without: with it, twice the depth
	/// costs at most <see cref="Doubling"/> times the entries; without it, the reading is counted to depth
	/// 16 and a shape that took more than thirty seconds is marked <c>t</c>.
	/// </summary>
	void Ladders(string title, string grammar, string publication, string prefix, (string Name, string Shape, string Core)[] shapes)
	{
		var kept   = Immediate(grammar, counts: true);
		var unkept = Immediate(grammar, counts: true, keeps: false);
		var worst  = ("", 1.0);

		Log($"{title}, rule entries per depth: without the memo of successes -> with it (r = refused; t = not counted: past depth 16 without the memo, or past thirty seconds)");

		foreach (var (name, shape, core) in shapes)
		{
			var line = $"{name,-20}";
			var last = 0L;
			var ratio = 0.0;

			foreach (var depth in Depths)
			{
				var text = prefix + Nest(shape, core, depth);
				var (one, okOne) = Count(unkept, publication, text, depth <= 16);
				var (two, okTwo) = Count(kept, publication, text);

				line += $" | {depth}: {Shown(one, okOne)} -> {Shown(two, okTwo)}";

				Assert.True(depth > 16 || okOne == okTwo || one == long.MaxValue / 4, $"{title} {name} at depth {depth}: accepted {okOne} without the memo, {okTwo} with it");

				if (last > 0)
					ratio = Math.Max(ratio, (double)two / last);

				last = two;
			}

			if (ratio > worst.Item2)
				worst = (name, ratio);

			Log(line + $" | worst doubling x{ratio:F2}");
		}

		Assert.True(worst.Item2 <= Doubling, $"{title}: '{worst.Item1}' still grows faster than its depth: x{worst.Item2:F2} for twice the depth.");
	}

	[Fact]
	public void Sql92_reads_the_same_with_the_memo()
	{
		var unkept  = Immediate(SqlVariants.Sql92, keeps: false);
		var kept    = Immediate(SqlVariants.Sql92);
		var unkeptL = Immediate(SqlVariants.Sql92, keeps: false, located: true).GetNestedType("Located")!;
		var keptL   = Immediate(SqlVariants.Sql92, located: true).GetNestedType("Located")!;
		var cover   = Immediate(SqlVariants.Sql92Cover);
		var split   = Immediate(SqlVariants.Sql92Split);
		var coverL  = Immediate(SqlVariants.Sql92Cover, located: true).GetNestedType("Located")!;

		foreach (var (name, type) in new[] { ("shipped, no memo", unkept), ("shipped + memo", kept), ("cover", cover), ("split", split) })
			Log($"size {name,-18}: generated C# {SqlVariants.Sizes[type].SourceChars:N0} chars, assembly {SqlVariants.Sizes[type].AssemblyBytes:N0} bytes");

		var generated = Generated(3000, seed: 92);
		var accepted  = generated.Where(text => Answer(unkept, "TryParseSelect", [text]).Accepted).ToList();
		var corpus    = Statements().Where(text => Answer(unkept, "TryParseSelect", [text]).Accepted || Answer(unkept, "TryParseQuery", [text]).Accepted).ToList();

		Log($"generated statements: {generated.Count}, of which the parser accepts {accepted.Count}; corpus statements the parser accepts as a select or a query: {corpus.Count} of {Statements().Count}");

		var texts = Harvested("Sql92")
			.Concat(Statements().Where((_, i) => i % 4 == 0))
			.Concat(corpus)
			.Concat(generated)
			// To depth 8: without the memo the subquery with a set operation costs a hundred million entries there, and three to the depth beyond.
			.Concat(Sql92Shapes.SelectMany(shape => new[] { 2, 5, 8 }.Select(depth => Prefix + Nest(shape.Shape, shape.Core, depth))))
			.Concat(Sql92Shapes.Select(shape => Nest(shape.Shape, shape.Core, 3)))
			.Concat(
			[
				"a", "(a)", "((a))", "(a) + 1", "(a, b) = (1, 2)", "(a, b) + 1 = 1", "(a = 1) = 1", "(a = 1) + 1", "-(a = 1)",
				"NOT a", "a AND b", "a IS TRUE", "(a = 1, 2) = (1, 2)", "(NULL, 1) = (1, 2)", "(NULL)", "(DEFAULT, 1) = (1, 1)",
				"1 + (a = 1)", "(SELECT a FROM t) = 1", "((SELECT a FROM t)) = 1", "a IN ((SELECT a FROM t), 2)",
				"(a = 1 AND b = 2) OR c = 3", "((a))) = 1", "(a +", "(a = ", "(a, ", "(CASE WHEN a = 1 THEN 1 END",
			])
			.Distinct()
			.ToList();

		var located = accepted.Concat(corpus).Concat(Harvested("Sql92")).ToList();

		// The memo: the same answer, byte for byte, and the same records.
		var differences = Differential("shipped: no memo vs memo", unkept, kept, texts, holds: true);

		Assert.True(differences == 0, $"{differences} answers differ between the shipped SQL-92 parser with the memo of successes and without; see the output.");
		Assert.Equal(0, LocatedDifferential("located: shipped, no memo vs memo", unkeptL, keptL, located));

		// The rewrites, for the table: what they would change.
		Differential("shipped + memo vs cover", kept, cover, texts, holds: false);
		Differential("shipped + memo vs split", kept, split, texts, holds: false);
		LocatedDifferential("located: shipped + memo vs cover", keptL, coverL, located);
	}

	[Fact]
	public void TSql_reads_the_same_with_the_memo()
	{
		var unkept = Immediate(SqlVariants.TransactSql, keeps: false);
		var kept   = Immediate(SqlVariants.TransactSql);

		foreach (var (name, type) in new[] { ("shipped, no memo", unkept), ("shipped + memo", kept) })
			Log($"size {name,-18}: generated C# {SqlVariants.Sizes[type].SourceChars:N0} chars, assembly {SqlVariants.Sizes[type].AssemblyBytes:N0} bytes");

		var statement = SqlVariants.Entry(SqlVariants.TransactSql, "TryParseStatement");
		var texts = Harvested("TransactSql")
			.Concat(Statements())
			.Concat(TSqlShapes.SelectMany(shape => new[] { 2, 5, 8 }.Select(depth => "SELECT a FROM t WHERE " + Nest(shape.Shape, shape.Core, depth))))
			.Concat(TSqlShapes.Select(shape => Nest(shape.Shape, shape.Core, 3)))
			.Distinct()
			.ToList();

		var accepted = Accepted(unkept, statement, Statements());

		Log($"corpus statements: {Statements().Count}, of which the parser accepts {accepted.Count}");

		var differences = Differential("T-SQL shipped: no memo vs memo", unkept, kept, texts, holds: true);

		Assert.True(differences == 0, $"{differences} answers differ between the shipped T-SQL parser with the memo of successes and without; see the output.");
	}

	[Fact]
	public void Sql2023_reads_the_same_with_the_memo()
	{
		var unkept = Immediate(SqlVariants.Standard, keeps: false);
		var kept   = Immediate(SqlVariants.Standard);

		foreach (var (name, type) in new[] { ("shipped, no memo", unkept), ("shipped + memo", kept) })
			Log($"size {name,-18}: generated C# {SqlVariants.Sizes[type].SourceChars:N0} chars, assembly {SqlVariants.Sizes[type].AssemblyBytes:N0} bytes");

		var texts = Harvested("SqlStandard")
			.Concat(Statements())
			.Concat(StandardShapes.SelectMany(shape => new[] { 2, 5, 8 }.Select(depth => "SELECT a FROM t WHERE " + Nest(shape.Shape, shape.Core, depth))))
			.Concat(StandardShapes.Select(shape => Nest(shape.Shape, shape.Core, 3)))
			.Distinct()
			.ToList();

		var accepted = Accepted(unkept, "TryParseStatement", Statements());

		Log($"corpus statements: {Statements().Count}, of which the parser accepts {accepted.Count}");

		var differences = Differential("SQL:2023 shipped: no memo vs memo", unkept, kept, texts, holds: true);

		Assert.True(differences == 0, $"{differences} answers differ between the shipped SQL:2023 parser with the memo of successes and without; see the output.");
	}

	/// <summary>The first <paramref name="limit"/> texts a parser accepts through one publication, in their order, read on several threads.</summary>
	internal static List<string> Accepted(Type parser, string publication, IEnumerable<string> texts, int limit = int.MaxValue)
	{
		var accepted = texts.AsParallel().AsOrdered().WithDegreeOfParallelism(ChunkedWork.Degree).Where(text => Answer(parser, publication, [text]).Accepted);

		// Take(int.MaxValue) makes the query size an array of that length.
		return limit == int.MaxValue ? [.. accepted] : [.. accepted.Take(limit)];
	}

	/// <summary>A variant on the immediate carrier, the shipped parsers'.</summary>
	internal static Type Immediate(string grammar, bool counts = false, bool keeps = true, bool located = false)
	{
		return SqlVariants.Parser(grammar, "off", true, counts, immediate: true, keeps, located);
	}

	/// <summary>One publication asked of one text, with the places the random draws chose for it.</summary>
	readonly record struct Ask(string Name, string Text, int Cut, int At, int Length, bool Whole)
	{
		/// <summary>Draws the cut, the place and the window the way a single pass over the plan would, so that the plan is the same whichever thread reads it.</summary>
		public static Ask Draw(string name, string text, Random random, bool whole)
		{
			var cut = random.Next(text.Length + 1);
			var at  = random.Next(text.Length + 1);
			var len = random.Next(text.Length - at + 1);

			return new Ask(name, text, cut, at, len, whole);
		}
	}

	/// <summary>What one stretch of a plan counted and found.</summary>
	sealed class Tally
	{
		public int Count;
		public int Nontrivial;
		public int AcceptedAnswers;
		public readonly SortedDictionary<int, int> Depths = [];
		public readonly Dictionary<string, List<string>> Kinds = new(StringComparer.Ordinal);
	}

	/// <summary>Every answer of every publication of both parsers over the texts, whole, cut and in windows, compared as text; the differences by kind, with samples; how many there were.</summary>
	int Differential(string title, Type before, Type after, List<string> texts, bool holds)
	{
		var all    = Publications(after);
		var random = new Random(92);
		var plan   = new List<Ask>();

		foreach (var text in texts)
			for (var i = 0; i < 3; i++)
				plan.Add(Ask.Draw(all[random.Next(all.Count)], text, random, whole: false));

		foreach (var name in all)
			foreach (var text in texts.Where(static one => one.Length < 120))
				plan.Add(Ask.Draw(name, text, random, whole: true));

		var kinds  = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		var count  = 0;
		var nontrivial = 0;
		var acceptedAnswers = 0;
		var depths = new SortedDictionary<int, int>();

		// The stretches are joined in the order of the plan: the samples of a kind are the first ones, as before.
		foreach (var part in ChunkedWork.Run(plan, static () => new Tally(), (tally, ask) => Compare(before, after, ask, tally)))
		{
			count           += part.Count;
			nontrivial      += part.Nontrivial;
			acceptedAnswers += part.AcceptedAnswers;

			foreach (var (depth, number) in part.Depths)
				depths[depth] = depths.GetValueOrDefault(depth) + number;

			foreach (var (kind, samples) in part.Kinds)
			{
				if (!kinds.TryGetValue(kind, out var into))
					kinds[kind] = into = [];

				into.AddRange(samples);
			}
		}

		var different = kinds.Values.Sum(static one => one.Count);

		Log($"{title}: {count:N0} answers compared, {nontrivial:N0} non-trivial (a side accepted, or refused past the first token), {acceptedAnswers:N0} accepted by the old parser; {different:N0} different = {100.0 * different / nontrivial:F2} % of the non-trivial");
		Log("  accepted answers by bracket depth of the text: " + string.Join(", ", depths.Select(static one => $"{one.Key}:{one.Value}")));

		foreach (var (kind, samples) in kinds.OrderByDescending(static one => one.Value.Count).ThenBy(static one => one.Key, StringComparer.Ordinal))
		{
			Log($"  {kind}: {samples.Count}");

			foreach (var sample in samples.Take(samples.Count > 40 ? 2 : 4))
				Log("    " + sample.Replace("\n", "\n    "));
		}

		var breaking = kinds.Where(static one => one.Key.StartsWith("outcome", StringComparison.Ordinal) || one.Key.StartsWith("tree", StringComparison.Ordinal)).Sum(static one => one.Value.Count);

		if (holds)
			Assert.True(breaking == 0, $"{title}: {breaking} answers differ in outcome or tree; see the output.");

		return different;
	}

	/// <summary>Every accepted answer of a located reading compared record for record and span for span; how many differ in any way.</summary>
	int LocatedDifferential(string title, Type before, Type after, List<string> texts)
	{
		var all = Publications(after);
		var compared = 0;
		var acceptedBoth = 0;
		var unequal = new List<string>();
		var spansDiffer = new List<string>();
		var outcome = new List<string>();

		foreach (var name in all)
			foreach (var text in texts)
			{
				var one = Invoke(before, name, [text]);
				var two = Invoke(after, name, [text]);

				compared++;

				if (one.Accepted != two.Accepted)
				{
					outcome.Add($"{name} of {Show(text)}: before {(one.Accepted ? "accepted" : "refused")}, after {(two.Accepted ? "accepted" : "refused")}");
					continue;
				}

				if (!one.Accepted)
					continue;

				acceptedBoth++;

				if (!Equals(one.Value, two.Value))
					unequal.Add($"{name} of {Show(text)}");

				var spansOne = new List<string>();
				var spansTwo = new List<string>();

				Walk(one.Value, spansOne);
				Walk(two.Value, spansTwo);

				if (!spansOne.SequenceEqual(spansTwo, StringComparer.Ordinal))
				{
					var at = Enumerable.Range(0, Math.Min(spansOne.Count, spansTwo.Count)).FirstOrDefault(i => spansOne[i] != spansTwo[i], Math.Min(spansOne.Count, spansTwo.Count));

					spansDiffer.Add($"{name} of {Show(text)}: {spansOne.Count} vs {spansTwo.Count} nodes, first difference at node {at}: {spansOne.ElementAtOrDefault(at)} vs {spansTwo.ElementAtOrDefault(at)}");
				}
			}

		Log($"{title}: {compared:N0} located answers, {acceptedBoth:N0} accepted by both; records unequal (Equals, spans included): {unequal.Count}; span walks differ: {spansDiffer.Count}; outcome differs: {outcome.Count}");

		foreach (var sample in unequal.Take(5).Concat(spansDiffer.Take(5)).Concat(outcome.Take(5)))
			Log("    " + sample);

		return unequal.Count + spansDiffer.Count + outcome.Count;
	}

	static void Compare(Type before, Type after, Ask ask, Tally tally)
	{
		var (name, text, cut, at, len, whole) = ask;

		(string Form, object[] Arguments)[] asked = whole
			? [("whole", [text])]
			: [("whole", [text]), ("cut", [text.Substring(0, cut)]), ("at", [text, at]), ("window", [text, at, len])];

		foreach (var (form, arguments) in asked)
		{
			var one = Answer(before, name, arguments);
			var two = Answer(after, name, arguments);

			tally.Count++;

			if (one.Accepted || two.Accepted || one.Position > 0 || two.Position > 0)
				tally.Nontrivial++;

			if (one.Accepted)
			{
				tally.AcceptedAnswers++;

				var depth = Depth((string)arguments[0]);

				tally.Depths[depth] = tally.Depths.GetValueOrDefault(depth) + 1;
			}

			if (one.Text == two.Text)
				continue;

			var kind =
				one.Text == "no such form" || two.Text == "no such form" ? "form" :
				one.Accepted != two.Accepted ? (one.Accepted ? "outcome: accepted before, refused after" : "outcome: refused before, accepted after") :
				one.Accepted ? "tree" :
				one.Position != two.Position ? "refusal position" :
				"refusal message, same position: " + Gained(one.Text, two.Text);

			if (!tally.Kinds.TryGetValue(kind, out var samples))
				tally.Kinds[kind] = samples = [];

			samples.Add($"{name} {form} ({string.Join(", ", arguments.Skip(1))}) of {Show((string)arguments[0])}:\n  before {one.Text}\n  after  {two.Text}");
		}
	}

	/// <summary>What the expected list gained and lost, as one signature: the kinds of message change, counted.</summary>
	static string Gained(string before, string after)
	{
		var one = Expected(before);
		var two = Expected(after);
		var gained = two.Except(one, StringComparer.Ordinal).ToList();
		var lost   = one.Except(two, StringComparer.Ordinal).ToList();

		if (gained.Count == 0 && lost.Count == 0)
			return one.SequenceEqual(two, StringComparer.Ordinal) ? "same items, other text" : "same items, another order";

		return "+{" + string.Join(" ", gained) + "} -{" + string.Join(" ", lost) + "}";
	}

	static List<string> Expected(string text)
	{
		var match = Regex.Match(text, "Expected (.*?)\\. \\|");

		if (!match.Success)
			return [];

		return [.. Regex.Split(match.Groups[1].Value, ", | or ").Select(static one => one.Trim())];
	}

	static int Depth(string text)
	{
		var depth = 0;
		var most  = 0;

		foreach (var c in text)
		{
			if (c == '(')
				most = Math.Max(most, ++depth);
			else if (c == ')')
				depth--;
		}

		return most;
	}

	static string Shown(long count, bool accepted)
	{
		return count == long.MaxValue / 4 ? "t" : count + (accepted ? "" : "r");
	}

	static string Show(string text)
	{
		var shown = text.Replace("\r", "\\r").Replace("\n", "\\n");

		return shown.Length > 160 ? shown.Substring(0, 157) + "..." : shown;
	}

	static string Nest(string shape, string core, int depth)
	{
		var text = core;

		for (var level = 0; level < depth; level++)
			text = string.Format(shape, text);

		return text;
	}

	/// <summary>Every node of a tree with its span, in the order of a walk over the public properties.</summary>
	static void Walk(object? value, List<string> into)
	{
		switch (value)
		{
			case null:
			case string:
				return;

			case ISqlNode node:
				into.Add(node.GetType().Name + "@" + node.Span);

				foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.GetIndexParameters().Length > 0 || property.Name is "Span" or "EqualityContract")
						continue;

					Walk(property.GetValue(node), into);
				}

				return;

			case System.Collections.IEnumerable items:
				foreach (var item in items)
					Walk(item, into);

				return;
		}
	}

	/// <summary>A generated SQL-92 corpus: selects over values and conditions nested to a bounded depth, seeded.</summary>
	internal static List<string> Generated(int count, int seed)
	{
		var random = new Random(seed);
		var texts  = new List<string>(count);

		for (var i = 0; i < count; i++)
		{
			var items = string.Join(", ", Enumerable.Range(0, 1 + random.Next(3)).Select(_ => Value(random, random.Next(4))));
			var text  = $"SELECT {items} FROM t";

			if (random.Next(4) > 0)
				text += " WHERE " + Condition(random, random.Next(5));

			if (random.Next(5) == 0)
				text += " GROUP BY a HAVING " + Condition(random, random.Next(3));

			texts.Add(text);
		}

		return texts;
	}

	static readonly string[] Columns = ["a", "b", "c", "t.a"];
	static readonly string[] Literals = ["1", "2", "0.5", "'x'", "'a%'"];

	static string Value(Random random, int depth)
	{
		if (depth == 0)
			return random.Next(2) == 0 ? Columns[random.Next(Columns.Length)] : Literals[random.Next(Literals.Length)];

		switch (random.Next(10))
		{
			case 0:
			case 1:
			case 2:
				return Value(random, 0);

			case 3:
				return "(" + Value(random, depth - 1) + ")";

			case 4:
			case 5:
				return Value(random, depth - 1) + " " + new[] { "+", "-", "*", "/", "||" }[random.Next(5)] + " " + Value(random, depth - 1);

			case 6:
				return "CASE WHEN " + Condition(random, depth - 1) + " THEN " + Value(random, depth - 1) + (random.Next(2) == 0 ? " ELSE " + Value(random, depth - 1) : "") + " END";

			case 7:
				return random.Next(3) switch
				{
					0 => "UPPER(" + Value(random, depth - 1) + ")",
					1 => "COALESCE(" + Value(random, depth - 1) + ", " + Value(random, depth - 1) + ")",
					_ => "CAST(" + Value(random, depth - 1) + " AS INTEGER)",
				};

			case 8:
				return "(SELECT a FROM t WHERE " + Condition(random, depth - 1) + ")";

			default:
				return "-" + Value(random, depth - 1);
		}
	}

	static string Condition(Random random, int depth)
	{
		if (depth == 0)
			return Value(random, 0) + " " + new[] { "=", "<>", "<", ">", "<=", ">=" }[random.Next(6)] + " " + Value(random, 0);

		switch (random.Next(14))
		{
			case 0:
			case 1:
			case 2:
			case 3:
				return Value(random, depth - 1) + " " + new[] { "=", "<>", "<", ">" }[random.Next(4)] + " " + Value(random, depth - 1);

			case 4:
			case 5:
				return "(" + Condition(random, depth - 1) + ")";

			case 6:
			case 7:
				return Condition(random, depth - 1) + (random.Next(2) == 0 ? " AND " : " OR ") + Condition(random, depth - 1);

			case 8:
				return "NOT " + Condition(random, depth - 1);

			case 9:
				return Value(random, depth - 1) + " BETWEEN " + Value(random, 0) + " AND " + Value(random, 0);

			case 10:
				return Value(random, depth - 1) + (random.Next(2) == 0 ? " IN (" + Value(random, 0) + ", " + Value(random, 0) + ")" : " IN (SELECT a FROM t WHERE " + Condition(random, depth - 1) + ")");

			case 11:
				return Value(random, depth - 1) + " IS " + (random.Next(2) == 0 ? "" : "NOT ") + "NULL";

			case 12:
				return "EXISTS (SELECT a FROM t WHERE " + Condition(random, depth - 1) + ")";

			default:
				return "(" + Value(random, 0) + ", " + Value(random, 0) + ") = (" + Value(random, 0) + ", " + Value(random, 0) + ")";
		}
	}

	/// <summary>What one form of a publication answers, written out, with the two things the kinds are told by.</summary>
	internal static (string Text, bool Accepted, long Position) Answer(Type parser, string name, object[] arguments)
	{
		var (match, text, accepted, position, _) = Invoke(parser, name, arguments);

		return (text, accepted, position);
	}

	static (object? Match, string Text, bool Accepted, long Position, object? Value) Invoke(Type parser, string name, object[] arguments)
	{
		var method = parser.GetMethod(
			name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null,
			[.. arguments.Select(static one => one.GetType())], modifiers: null);

		if (method is null)
			return (null, "no such form", false, -1, null);

		object match;

		try
		{
			match = method.Invoke(null, arguments)!;
		}
		catch (TargetInvocationException thrown)
		{
			return (null, "threw " + thrown.InnerException?.GetType().Name, false, -2, null);
		}

		var kind     = match.GetType();
		var accepted = (bool)kind.GetProperty("IsSuccess")!.GetValue(match)!;
		var position = Convert.ToInt64(kind.GetProperty("Position")!.GetValue(match));
		var value    = accepted ? kind.GetProperty("Value")!.GetValue(match) : null;

		var text =
			$"{kind.GetProperty("Outcome")!.GetValue(match)} at {position}" +
			$"+{kind.GetProperty("Length")!.GetValue(match)}: {kind.GetProperty("Error")!.GetValue(match)} | " +
			(accepted ? Written(value) : "");

		return (match, Normalized(text), accepted, position, value);
	}

	/// <summary>
	/// The lines of a stack trace inside a written record: <c>at Type.Method(…) in file:line n</c>, a
	/// member's name and its parameter list after <c>at</c> — not the answer's own <c>at {position}</c>,
	/// whose position is a number.
	/// </summary>
	static readonly Regex Frames = new(@"\s+at [\w.<>`$\[\],]+\([^()\r\n]*\)( in [^\r\n]*?:line \d+)?", RegexOptions.Compiled);

	/// <summary>
	/// An answer with the stack frames of a gap it keeps taken out, and nothing else: a record written
	/// out may hold an exception the tree keeps as a gap, whose stack trace names the frames the runtime
	/// inlined, which are not the tree's. The outcome, position, length, message and tree stay as written.
	/// </summary>
	static string Normalized(string text)
	{
		return Frames.Replace(text, "");
	}

	/// <summary>What the normalization keeps apart and what it takes out, held here so that a differential compares answers and not their absence.</summary>
	[Theory]
	[InlineData("Success at 0+15:  | SELECT 1 FROM t", "Success at 0+16:  | SELECT 99 FROM t")]
	[InlineData("Success at 0+15:  | SELECT 1 FROM t", "Success at 0+15:  | SELECT 2 FROM t")]
	[InlineData("NoMatch at 3+0: Expected 'a'. | ", "NoMatch at 4+0: Expected 'a'. | ")]
	[InlineData("NoMatch at 3+0: Expected 'a'. | ", "NoMatch at 3+0: Expected 'b'. | ")]
	public void Answers_that_differ_stay_apart_when_normalized(string one, string other)
	{
		Assert.NotEqual(Normalized(one), Normalized(other));
		Assert.Equal(one, Normalized(one));
	}

	[Fact]
	public void Only_the_frames_of_a_gap_are_taken_out()
	{
		const string frames =
			"Success at 0+15:  | Built { Gap = DotGram.Sql.TransactSql.BuildGap: Statement.Dbcc: not kept\n" +
			"   at DotGram.Sql.TransactSql.Build.Server.DbccOption(Dbcc owner, Clause clause) in /x/Build.Server.cs:line 930\n" +
			"   at DotGram.Sql.TransactSql.Build.Server.<>c__DisplayClass36_0.<Dbcc>b__1(Clause one) in /x/Build.Server.cs:line 905\n" +
			"   at DotGram.Sql.TransactSql.Build.Hold[T](T given, Func`2 build) in /x/Build.Built.cs:line 185, Given = Dbcc { } }";

		Assert.Equal(
			"Success at 0+15:  | Built { Gap = DotGram.Sql.TransactSql.BuildGap: Statement.Dbcc: not kept, Given = Dbcc { } }",
			Normalized(frames));
	}

	static string Written(object? value)
	{
		switch (value)
		{
			case null:
				return "null";

			case Expression.Extension extension:
				return $"extension {extension.Dialect}: {extension.Kind}";

			case ISqlNode node:
				// The T-SQL variant builds T-SQL's own tree, which the writer of the common tree refuses in places
				// (a literal it has no text for): the record written out as a record then, which compares as well.
				try
				{
					return SqlWriter.Write(node);
				}
				catch (ArgumentException)
				{
					return node.ToString() ?? "";
				}

			case System.Collections.IEnumerable items when value is not string:
				return "[" + string.Join("; ", items.Cast<object?>().Select(Written)) + "]";
		}

		return value.ToString() ?? "";
	}

	static List<string> Publications(Type parser)
	{
		return
		[
			.. parser.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.Where(static one => one.Name.StartsWith("TryParse", StringComparison.Ordinal) &&
					one.GetParameters() is [var only] && only.ParameterType == typeof(string) &&
					one.ReturnType.Name.StartsWith("Match", StringComparison.Ordinal))
				.Select(static one => one.Name)
				.Distinct()
				.OrderBy(static one => one, StringComparer.Ordinal),
		];
	}

	/// <summary>The rule entries one reading of a text costs, and whether it was accepted; a reading past thirty seconds is a timeout, marked <c>t</c>.</summary>
	static (long Count, bool Accepted) Count(Type parser, string publication, string text, bool worth = true)
	{
		if (!worth)
			return (long.MaxValue / 4, false);

		var counters = Counters(parser);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)
			?? throw new InvalidOperationException($"no {publication} on " + parser);

		object? match = null;
		Exception? error = null;
		var thread = new Thread(() =>
		{
			try { match = read.Invoke(null, [text]); }
			catch (Exception e) { error = e; }
		}, 256 * 1024 * 1024) { IsBackground = true };

		thread.Start();

		if (!thread.Join(TimeSpan.FromSeconds(30)))
			return (long.MaxValue / 4, false);

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		var ok = (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

		return (counters.Sum(counter => (long)counter.GetValue(null)!), ok);
	}

	static FieldInfo[] Counters(Type parser)
	{
		return
		[
			.. parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
				.SelectMany(type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
				.Where(field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal)),
		];
	}

	internal static List<string> Statements()
	{
		return _statements.Value;
	}

	static readonly Lazy<List<string>> _statements = new(static () => [.. CorpusRoundTrip.Statements(Corpus())]);

	static string Corpus([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "ScriptDom"));
	}

	static IEnumerable<string> Harvested(string prefix, [CallerFilePath] string here = "")
	{
		var tests = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "DotGram.Sql.Tests"));
		var found = new SortedSet<string>(StringComparer.Ordinal);

		foreach (var file in Directory.GetFiles(tests, prefix + "*.cs").OrderBy(static one => one, StringComparer.Ordinal))
			foreach (Match literal in Regex.Matches(File.ReadAllText(file), "\"((?:[^\"\\\\\\n]|\\\\.)*)\""))
			{
				try
				{
					found.Add(Regex.Unescape(literal.Groups[1].Value));
				}
				catch (ArgumentException)
				{
					found.Add(literal.Groups[1].Value);
				}
			}

		return found;
	}
}
