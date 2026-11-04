﻿using System;
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
/// The SQL-92 grammar with every bracket read once, held against the grammar as it was: what each
/// reads, refuses and builds — the records compared with <c>Equals</c> and every node's span walked
/// on a located reading — over the SQL-92 tests' texts, the corpus statements the parser accepts, a
/// generated corpus of several thousand SQL-92 statements, and nests to depth 32; the rule entries a
/// nest costs at each depth; the emitted size of each variant; and the time of each on accepted texts.
/// </summary>
/// <remarks>
/// A prototype's measurement, not a test to keep: it prints every kind of difference with samples and
/// holds only that no accepted text is refused, none refused is accepted, and no accepted text builds
/// another tree. Alone, because the head-guard experiment is a static switch of the emitter.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ReadOnceBracketTests(ITestOutputHelper output)
{
	void Log(string line)
	{
		output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("READONCE_OUT") is { Length: > 0 } file)
			File.AppendAllText(file, line + "\n");
	}

	/// <summary>The nests a bracketed value holding a condition made exponential, each a level around <c>{0}</c>.</summary>
	static readonly (string Name, string Shape, string Core)[] Shapes =
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
		("row-list",         "((CASE WHEN {0} THEN 1 END, 1) = (1, 1))",                "a = 1"),
		("row-nest",         "(({0}, 1) = (1, 1))",                                     "a"),
		("case-cond-unclosed","(CASE WHEN {0}",                                         "a = 1"),
		("case-cond-refused","(CASE WHEN {0} THEN 1 END = 1 x)",                        "a = 1"),
		("plain-brackets",   "({0})",                                                   "a = 1"),
		("value-brackets",   "({0})",                                                   "a"),
	];

	const string Prefix = "SELECT a FROM t WHERE ";

	[Fact]
	public void Sql92_costs_its_depth_once_a_bracket_is_read_once()
	{
		var before = SqlVariants.Parser(SqlVariants.Sql92Before, counts: true);
		var after  = SqlVariants.Parser(SqlVariants.Sql92, counts: true);
		var split  = SqlVariants.Parser(SqlVariants.Sql92Split, "off", true, true, headGuard: true, located: false);
		var worst  = 1.0;

		Log("rule entries per depth: before -> cover -> split+headguard (r = refused, t = timed out at 30 s)");

		foreach (var (name, shape, core) in Shapes)
		{
			var line  = $"{name,-20}";
			var last  = 0L;
			var ratio = 0.0;

			foreach (var depth in new[] { 4, 8, 16, 32 })
			{
				var text = Prefix + Nest(shape, core, depth);
				var (one, okOne) = Count(before, text, depth <= 16);
				var (two, okTwo) = Count(after, text, true);
				var (three, okThree) = Count(split, text, true);

				line += $" | {depth}: {Shown(one, okOne)} -> {Shown(two, okTwo)} -> {Shown(three, okThree)}";

				if (last > 0)
					ratio = (double)two / last;

				last = two;
			}

			worst = Math.Max(worst, ratio);
			Log(line + $" | cover 32/16 x{ratio:F2}");
		}

		// Linear in the depth: twice the depth costs at most twice the entries, with a tenth for the fixed cost.
		Assert.True(worst < 2.2, $"a nest still grows faster than its depth: x{worst:F2} for twice the depth.");
	}

	[Fact]
	public void Sql92_reads_the_same_with_a_bracket_read_once()
	{
		var before   = SqlVariants.Parser(SqlVariants.Sql92Before);
		var cover    = SqlVariants.Parser(SqlVariants.Sql92);
		var coverHg  = SqlVariants.Parser(SqlVariants.Sql92, "off", true, false, headGuard: true, located: false);
		var splitHg  = SqlVariants.Parser(SqlVariants.Sql92Split, "off", true, false, headGuard: true, located: false);
		var beforeL  = SqlVariants.Parser(SqlVariants.Sql92Before, "off", true, false, headGuard: false, located: true).GetNestedType("Located")!;
		var coverHgL = SqlVariants.Parser(SqlVariants.Sql92, "off", true, false, headGuard: true, located: true).GetNestedType("Located")!;
		var splitHgL = SqlVariants.Parser(SqlVariants.Sql92Split, "off", true, false, headGuard: true, located: true).GetNestedType("Located")!;

		foreach (var (name, type) in new[] { ("before", before), ("cover", cover), ("cover+headguard", coverHg), ("split+headguard", splitHg) })
			Log($"size {name,-16}: generated C# {SqlVariants.Sizes[type.Assembly.GetTypes().First(static one => SqlVariants.Sizes.ContainsKey(one))].SourceChars:N0} chars, assembly {SqlVariants.Sizes[type.Assembly.GetTypes().First(static one => SqlVariants.Sizes.ContainsKey(one))].AssemblyBytes:N0} bytes");

		var generated = Generated(3000, seed: 92);
		var accepted  = generated.Where(text => Answer(before, "TryParseSelect", [text]).Accepted).ToList();
		var corpus    = Statements().Where(text => Answer(before, "TryParseSelect", [text]).Accepted || Answer(before, "TryParseQuery", [text]).Accepted).ToList();

		Log($"generated statements: {generated.Count}, of which the old parser accepts {accepted.Count}; corpus statements the old parser accepts as a select or a query: {corpus.Count} of {Statements().Count}");

		var texts = Harvested("Sql92")
			.Concat(Statements().Where((_, i) => i % 4 == 0))
			.Concat(corpus)
			.Concat(generated)
			.Concat(Shapes.SelectMany(shape => new[] { 2, 5, 8, 16 }.Select(depth => Prefix + Nest(shape.Shape, shape.Core, depth))))
			.Concat(Shapes.Select(shape => Nest(shape.Shape, shape.Core, 3)))
			.Concat(
			[
				"a", "(a)", "((a))", "(a) + 1", "(a, b) = (1, 2)", "(a, b) + 1 = 1", "(a = 1) = 1", "(a = 1) + 1", "-(a = 1)",
				"NOT a", "a AND b", "a IS TRUE", "(a = 1, 2) = (1, 2)", "(NULL, 1) = (1, 2)", "(NULL)", "(DEFAULT, 1) = (1, 1)",
				"1 + (a = 1)", "(SELECT a FROM t) = 1", "((SELECT a FROM t)) = 1", "a IN ((SELECT a FROM t), 2)",
				"(a = 1 AND b = 2) OR c = 3", "((a))) = 1", "(a +", "(a = ", "(a, ", "(CASE WHEN a = 1 THEN 1 END",
			])
			.Distinct()
			.ToList();

		Differential("before vs cover", before, cover, texts, holds: true);
		Differential("before vs cover+headguard", before, coverHg, texts, holds: false);
		Differential("before vs split+headguard", before, splitHg, texts, holds: false);

		LocatedDifferential("located: before vs cover+headguard", beforeL, coverHgL, accepted.Concat(corpus).Concat(Harvested("Sql92")).ToList());
		LocatedDifferential("located: before vs split+headguard", beforeL, splitHgL, accepted.Concat(corpus).Concat(Harvested("Sql92")).ToList());

		Timing(accepted, [("before", before), ("cover", cover), ("cover+headguard", coverHg), ("split+headguard", splitHg)]);
	}

	void Differential(string title, Type before, Type after, List<string> texts, bool holds)
	{
		var all    = Publications(after);
		var random = new Random(92);
		var kinds  = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		var count  = 0;
		var nontrivial = 0;
		var acceptedAnswers = 0;
		var depths = new SortedDictionary<int, int>();

		foreach (var text in texts)
			for (var i = 0; i < 3; i++)
				Compare(before, after, all[random.Next(all.Count)], text, random, ref count, ref nontrivial, ref acceptedAnswers, depths, kinds);

		foreach (var name in all)
			foreach (var text in texts.Where(static one => one.Length < 120))
				Compare(before, after, name, text, random, ref count, ref nontrivial, ref acceptedAnswers, depths, kinds, whole: true);

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
	}

	void LocatedDifferential(string title, Type before, Type after, List<string> texts)
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
	}

	void Timing(List<string> accepted, (string Name, Type Parser)[] parsers)
	{
		var texts   = accepted.Take(1500).ToList();
		var methods = parsers.Select(one => one.Parser.GetMethod("TryParseSelect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)!).ToArray();
		var medians = new double[parsers.Length];
		var rounds  = new List<double>[parsers.Length];

		for (var i = 0; i < parsers.Length; i++)
			rounds[i] = [];

		// Warm up each, then five rounds, the parsers interleaved so that the machine's state is shared.
		for (var round = 0; round < 6; round++)
			for (var i = 0; i < parsers.Length; i++)
			{
				var clock = Stopwatch.StartNew();

				foreach (var text in texts)
					methods[i].Invoke(null, [text]);

				clock.Stop();

				if (round > 0)
					rounds[i].Add(clock.Elapsed.TotalMilliseconds * 1000.0 / texts.Count);
			}

		for (var i = 0; i < parsers.Length; i++)
		{
			var sorted = rounds[i].OrderBy(static one => one).ToList();

			medians[i] = sorted[sorted.Count / 2];
		}

		Log($"time per accepted generated statement ({texts.Count} texts, median of 5 rounds, directional on a shared machine):");

		for (var i = 0; i < parsers.Length; i++)
			Log($"  {parsers[i].Name,-16} {medians[i],8:F2} us  x{medians[i] / medians[0]:F3} of before  (rounds: {string.Join(" ", rounds[i].Select(static one => one.ToString("F2")))})");
	}

	static void Compare(Type before, Type after, string name, string text, Random random, ref int count, ref int nontrivial, ref int acceptedAnswers, SortedDictionary<int, int> depths, Dictionary<string, List<string>> kinds, bool whole = false)
	{
		var cut = random.Next(text.Length + 1);
		var at  = random.Next(text.Length + 1);
		var len = random.Next(text.Length - at + 1);

		(string Form, object[] Arguments)[] asked = whole
			? [("whole", [text])]
			: [("whole", [text]), ("cut", [text.Substring(0, cut)]), ("at", [text, at]), ("window", [text, at, len])];

		foreach (var (form, arguments) in asked)
		{
			var one = Answer(before, name, arguments);
			var two = Answer(after, name, arguments);

			count++;

			if (one.Accepted || two.Accepted || one.Position > 0 || two.Position > 0)
				nontrivial++;

			if (one.Accepted)
			{
				acceptedAnswers++;

				var depth = Depth((string)arguments[0]);

				depths[depth] = depths.GetValueOrDefault(depth) + 1;
			}

			if (one.Text == two.Text)
				continue;

			var kind =
				one.Text == "no such form" || two.Text == "no such form" ? "form" :
				one.Accepted != two.Accepted ? (one.Accepted ? "outcome: accepted before, refused after" : "outcome: refused before, accepted after") :
				one.Accepted ? "tree" :
				one.Position != two.Position ? "refusal position" :
				"refusal message, same position: " + Gained(one.Text, two.Text);

			if (!kinds.TryGetValue(kind, out var samples))
				kinds[kind] = samples = [];

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
	static List<string> Generated(int count, int seed)
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
	static (string Text, bool Accepted, long Position) Answer(Type parser, string name, object[] arguments)
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

		return (match, text, accepted, position, value);
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
				return SqlWriter.Write(node);

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

	static (long Count, bool Accepted) Count(Type parser, string text, bool worth)
	{
		if (!worth)
			return (long.MaxValue / 4, false);

		var counters = Counters(parser);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod("TryParseSelect", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)
			?? throw new InvalidOperationException("no TryParseSelect on " + parser);

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

	static List<string> Statements()
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
