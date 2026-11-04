using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

using DotGram.Benchmarks;
using DotGram.Sql;
using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The SQL-92 grammar with every bracket read once, held against the grammar as it was: what each
/// reads, refuses and builds over the SQL-92 tests' texts, a quarter of the corpus and the nests
/// whose readings doubled; and the rule entries a nest costs at each depth, on both.
/// </summary>
/// <remarks>
/// A prototype's differential, not a test to keep: it prints every kind of difference with samples,
/// and holds only that no accepted text is refused, none refused is accepted, and no accepted text
/// builds another tree.
/// </remarks>
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

	[Fact]
	public void Sql92_costs_its_depth_once_a_bracket_is_read_once()
	{
		var before = SqlVariants.Parser(SqlVariants.Sql92Before, counts: true);
		var after  = SqlVariants.Parser(SqlVariants.Sql92, counts: true);
		var worst  = 1.0;

		foreach (var (name, shape, core) in Shapes)
		{
			var line = $"{name,-20}";
			var last = 0L;
			var ratio = 0.0;

			foreach (var depth in new[] { 2, 4, 6, 8, 10, 12 })
			{
				var text = "SELECT a FROM t WHERE " + Nest(shape, core, depth);
				var (one, okOne, msOne) = Count(before, "TryParseSelect", text);
				var (two, okTwo, msTwo) = Count(after, "TryParseSelect", text);

				line += $" | {depth}: {one}{(okOne ? "" : "r")} -> {two}{(okTwo ? "" : "r")}";

				if (last > 0)
					ratio = (double)two / last;

				last = two;
			}

			worst = Math.Max(worst, ratio);
			Log(line + $" | after, last doubling x{ratio:F2}");
		}

		// Linear in the depth: two levels more cost at most a constant more, so the ratio of the counts at
		// 12 and 10 is at most 1.3 (a linear count's 12/10 = 1.2, with room for the fixed cost).
		Assert.True(worst < 1.3, $"a nest still grows faster than its depth: x{worst:F2} for two levels more.");
	}

	[Fact]
	public void Sql92_reads_the_same_with_a_bracket_read_once()
	{
		var before = SqlVariants.Parser(SqlVariants.Sql92Before);
		var after  = SqlVariants.Parser(SqlVariants.Sql92);
		var all    = Publications(after);
		var random = new Random(92);
		var kinds  = new Dictionary<string, List<string>>(StringComparer.Ordinal);
		var count  = 0;

		var texts = Harvested("Sql92")
			.Concat(Statements().Where((_, i) => i % 4 == 0))
			.Concat(Shapes.SelectMany(shape => new[] { 2, 5 }.Select(depth => "SELECT a FROM t WHERE " + Nest(shape.Shape, shape.Core, depth))))
			.Concat(Shapes.Select(shape => Nest(shape.Shape, shape.Core, 3)))
			.Concat(
			[
				"a", "(a)", "((a))", "(a) + 1", "(a, b) = (1, 2)", "(a, b) + 1 = 1", "(a = 1) = 1", "(a = 1) + 1", "-(a = 1)",
				"NOT a", "a AND b", "a IS TRUE", "(a = 1, 2) = (1, 2)", "(NULL, 1) = (1, 2)", "(NULL)", "(DEFAULT, 1) = (1, 1)",
				"1 + (a = 1)", "(SELECT a FROM t) = 1", "((SELECT a FROM t)) = 1", "a IN ((SELECT a FROM t), 2)",
				"(a = 1 AND b = 2) OR c = 3", "((a))) = 1", "(a +", "(a = ", "(a, ", "(CASE WHEN a = 1 THEN 1 END",
			])
			.ToList();

		foreach (var text in texts)
			for (var i = 0; i < 3; i++)
				Compare(before, after, all[random.Next(all.Count)], text, random, ref count, kinds);

		foreach (var name in all)
			foreach (var text in texts.Where(static one => one.Length < 120))
				Compare(before, after, name, text, random, ref count, kinds, whole: true);

		Log($"SQL-92 before/after, {count:N0} answers compared, {kinds.Values.Sum(static one => one.Count)} different:");

		foreach (var (kind, samples) in kinds.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			Log($"  {kind}: {samples.Count}");

			foreach (var sample in samples.Take(12))
				Log("    " + sample.Replace("\n", "\n    "));
		}

		var breaking = kinds.Where(static one => one.Key.StartsWith("outcome", StringComparison.Ordinal) || one.Key.StartsWith("tree", StringComparison.Ordinal)).Sum(static one => one.Value.Count);

		Assert.True(breaking == 0, $"{breaking} answers differ in outcome or tree; see the output.");
	}

	static void Compare(Type before, Type after, string name, string text, Random random, ref int count, Dictionary<string, List<string>> kinds, bool whole = false)
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

			if (one.Text == two.Text)
				continue;

			var kind =
				one.Text == "no such form" || two.Text == "no such form" ? "form" :
				one.Accepted != two.Accepted ? (one.Accepted ? "outcome: accepted before, refused after" : "outcome: refused before, accepted after") :
				one.Accepted ? "tree" :
				one.Position != two.Position ? "refusal position" :
				"refusal message, same position";

			if (!kinds.TryGetValue(kind, out var samples))
				kinds[kind] = samples = [];

			samples.Add($"{name} {form} ({string.Join(", ", arguments.Skip(1))}) of {Show((string)arguments[0])}:\n  before {one.Text}\n  after  {two.Text}");
		}
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

	/// <summary>What one form of a publication answers, written out, with the two things the kinds are told by.</summary>
	static (string Text, bool Accepted, long Position) Answer(Type parser, string name, object[] arguments)
	{
		var method = parser.GetMethod(
			name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null,
			[.. arguments.Select(static one => one.GetType())], modifiers: null);

		if (method is null)
			return ("no such form", false, -1);

		object match;

		try
		{
			match = method.Invoke(null, arguments)!;
		}
		catch (TargetInvocationException thrown)
		{
			return ("threw " + thrown.InnerException?.GetType().Name, false, -2);
		}

		var kind     = match.GetType();
		var accepted = (bool)kind.GetProperty("IsSuccess")!.GetValue(match)!;
		var position = Convert.ToInt64(kind.GetProperty("Position")!.GetValue(match));

		var text =
			$"{kind.GetProperty("Outcome")!.GetValue(match)} at {position}" +
			$"+{kind.GetProperty("Length")!.GetValue(match)}: {kind.GetProperty("Error")!.GetValue(match)} | " +
			(accepted ? Written(kind.GetProperty("Value")!.GetValue(match)) : "");

		return (text, accepted, position);
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

	static (long Count, bool Accepted, long Ms) Count(Type parser, string publication, string text)
	{
		var counters = Counters(parser);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)
			?? throw new InvalidOperationException("no " + publication + " on " + parser);

		object? match = null;
		Exception? error = null;
		var clock = Stopwatch.StartNew();
		var thread = new Thread(() =>
		{
			try { match = read.Invoke(null, [text]); }
			catch (Exception e) { error = e; }
		}, 256 * 1024 * 1024) { IsBackground = true };

		thread.Start();

		if (!thread.Join(TimeSpan.FromSeconds(30)))
			return (long.MaxValue / 4, false, clock.ElapsedMilliseconds);

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		var ok = (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

		return (counters.Sum(counter => (long)counter.GetValue(null)!), ok, clock.ElapsedMilliseconds);
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
