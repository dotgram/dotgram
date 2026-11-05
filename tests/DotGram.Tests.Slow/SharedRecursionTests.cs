using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The shared-recursion analysis (<see cref="SharedRecursion"/>, GRAM4054) over the grammars that
/// ship and over one small grammar per shape: what it flags, that it flags the instances that were
/// measured exponential, and — the ratchet — that the grammars that ship have exactly the pairs
/// listed here, so that a new one fails the build until the shape is written to read its text once
/// or the pair is listed with its reading.
/// </summary>
/// <remarks>
/// Every grammar's pairs are printed with their witnesses. The pairs listed are read once per level
/// by the memo of successes on the immediate carrier (<see cref="SqlSuccessMemoTests"/> holds the
/// SQL nests to their depth); the list is what the grammars have, not what they should.
/// </remarks>
public sealed class SharedRecursionTests(ITestOutputHelper output)
{
	const string Split = "trivia = { ' '* }\nnamespace Token { trivia = none\n Id = ['a'..'z']+ }\n";

	void Log(string line)
	{
		output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("DETECTOR_OUT") is { Length: > 0 } file)
			File.AppendAllText(file, line + "\n");
	}

	/// <summary>The grammars that ship: the three SQL dialects, the expression language, FIX and the web formats.</summary>
	public static TheoryData<string> Shipped { get; } = new()
	{
		"T-SQL",
		"SQL:2023",
		"SQL-92",
		"EL",
		"FIX",
		"RFC 3339",
		"RFC 3986",
		"RFC 5322",
		"RFC 5646",
		"RFC 6265",
		"RFC 6266",
		"RFC 6570",
		"RFC 6901",
		"RFC 7239",
		"RFC 8259",
		"RFC 8288",
		"RFC 9110",
		"RFC 9651",
	};

	/// <summary>
	/// The ratchet: every pair at the levels the memo answers (<see cref="SharedRecursion.Remembered"/>)
	/// in each grammar that ships, as <c>L{level} {rule} {shape} {alternatives}</c>, <c>guarded</c>
	/// where a <c>when</c> on the way may separate the two. Written as a choice is read, so one rule
	/// may list a pair twice where two of its choices have the shape.
	/// </summary>
	static readonly Dictionary<string, string[]> Flagged = new(StringComparer.Ordinal)
	{
		["T-SQL"] =
		[
			"L3 BooleanPrimary | 1/2",
			"L3 DatePart | 2/3",
			"L3 RowValueConstructor | 1/2",
			"L3 RowValueConstructor | 1/3 guarded",
			"L3 RowValueConstructor | 2/3 guarded",
			"L3 SetFunctionSpecification ? 1/2",
			"L3 TSqlPredicate | 5/7 guarded",
			"L3 TSqlPrimaryCore | 1/8",
			"L3 TSqlPrimaryCore | 10/12 guarded",
			"L3 TSqlTablePrimary | 8/9 guarded",
			"L3 TSqlValueFunction * 1/2 guarded",
			"L3 TSqlValueFunction ? 1/2",
			"L3 TSqlValueFunction ? 1/2 guarded",
			"L3 Top | 1/2",
			"L3 ValueFunction ? 1/2",
			"L3 ValueFunction ? 1/2",
			"L2 CallTail | 2/4",
			"L2 GroupByExpression | 2/3",
			"L2 GroupingSet | 1/4 guarded",
			"L2 GroupingSet | 2/4 guarded",
			"L2 GroupingSet | 3/4",
			"L2 GroupingSetItem | 1/3 guarded",
			"L2 GroupingSetItem | 2/3 guarded",
			"L2 InPredicateValue | 1/2",
			"L2 RowsetArgument | 1/2",
			"L2 RowsetArgument | 5/7",
			"L2 RowsetArgument | 6/7",
			"L2 RowsetFunction ? 1/2",
			"L2 RowsetValue | 1/4",
			"L2 TSqlGrouping | 1/4 guarded",
			"L2 TSqlGrouping | 2/4 guarded",
			"L2 TSqlTablePrimary | 1/6",
			"L2 TSqlTablePrimary | 2/6",
			"L2 TSqlTablePrimary | 4/6",
			"L2 TSqlTablePrimary ? 1/2 guarded",
			"L2 TSqlTablePrimary ? 1/2 guarded",
		],
		["SQL:2023"] =
		[
			"L3 JSONArrayConstructor | 1/2",
			"L3 JSONPredicatePrimary | 2/3",
			"L3 NegatablePredicatePart2 | 1/2 guarded",
			"L3 NumericValueFunction ? 1/2 guarded",
			"L3 NumericValueFunction ?= 1/2",
			"L3 PrimaryReading | 1/2",
			"L3 TablePrimary | 7/10",
			"L2 ComparisonTail | 1/2",
			"L2 ContextuallyTypedRowValueExpression | 2/4",
			"L2 ContextuallyTypedRowValueExpression | 3/4",
			"L2 InsertValues | 1/2",
			"L2 JSONNameAndValue | 1/2 guarded",
			"L2 WindowFrameBound | 1/3",
			"L2 WindowSpecificationDetails ? 1/2 guarded",
		],
		["SQL-92"] =
		[
			"L3 BooleanPrimary | 1/2",
			"L3 InPredicateValue | 1/2",
			"L3 RowValueConstructor | 1/2",
			"L3 RowValueConstructor | 1/3",
			"L3 RowValueConstructor | 2/3",
			"L3 TablePrimary | 2/3",
			"L3 ValueExpressionPrimary | 1/6",
		],
		["EL"] =
		[
			"L3 Assignment | 1/2 guarded",
			"L3 Block * 1/2 guarded",
			"L3 Body | 2/3 guarded",
			"L3 Control | 6/7 guarded",
			"L3 Control | 6/8 guarded",
			"L3 Control | 7/8 guarded",
			"L3 If | 1/2 guarded",
			"L3 If | 1/2 guarded",
			"L3 If | 1/2 guarded",
			"L3 IfValue | 2/3 guarded",
			"L3 IfValue | 2/3 guarded",
			"L3 Postfix | 1/2 guarded",
			"L3 Postfix | 1/3 guarded",
			"L3 Postfix ? 1/2 guarded",
			"L3 Primary | 1/2 guarded",
			"L3 Statement | 1/2 guarded",
			"L3 Statement | 1/3 guarded",
			"L3 Statement | 1/8 guarded",
			"L3 Statement | 2/3 guarded",
			"L3 Statement | 2/4 guarded",
			"L3 Statement | 2/6 guarded",
			"L3 Statement | 2/7 guarded",
			"L3 Statement | 3/4 guarded",
			"L3 Statement | 3/6 guarded",
			"L3 Statement | 3/7 guarded",
			"L2 Arm | 1/2 guarded",
			"L2 Inferred | 2/3",
			"L2 InferredUnsettled | 2/3",
			"L2 Local | 2/3",
			"L2 Return | 2/3",
		],
		["FIX"]      = [],
		["RFC 3339"] = [],
		["RFC 3986"] = [],
		["RFC 5322"] = [],
		["RFC 5646"] = [],
		["RFC 6265"] = [],
		["RFC 6266"] = [],
		["RFC 6570"] = [],
		["RFC 6901"] = [],
		["RFC 7239"] = [],
		["RFC 8259"] = [],
		["RFC 8288"] = [],
		["RFC 9110"] = [],
		["RFC 9651"] = [],
	};

	/// <summary>A grammar's text as the generator reads it, and whether it is cut in two.</summary>
	static (string Text, bool Lexical) Source(string name)
	{
		var root = SourceRoot.Directory;

		return name switch
		{
			"T-SQL"    => (File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "TransactSql", "TransactSql.gram")), true),
			"SQL:2023" => (File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "Standard", "SqlStandard.gram")), true),
			"SQL-92"   => (File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "Standard", "SqlStandard92.gram")), true),
			"EL"       => Inline(Path.Combine(root, "src", "DotGram.ExpressionLanguage", "ExpressionParser.cs")),
			"FIX"      => Inline(Path.Combine(root, "src", "DotGram.Finance", "Fix", "FixGrammar.cs")),
			"PostgreSQL 91bf1d63" => (File.ReadAllText(Path.Combine(root, "tests", "DotGram.Tests.Slow", "Fixtures", "SharedRecursion", "PostgreSql-91bf1d63.gram")), true),
			"PostgreSQL 15cf5f88" => (File.ReadAllText(Path.Combine(root, "tests", "DotGram.Tests.Slow", "Fixtures", "SharedRecursion", "PostgreSql-15cf5f88.gram")), true),
			_ when name.StartsWith("RFC ", StringComparison.Ordinal) => Inline(Path.Combine(root, "src", "DotGram.Web", "Rfc" + name.Substring(4) + ".cs")),
			_          => throw new ArgumentOutOfRangeException(nameof(name)),
		};
	}

	/// <summary>The grammar written into the first <c>[Gram]</c> of a source, read as the compiler reads the string, and whether that attribute cuts it in two.</summary>
	static (string Text, bool Lexical) Inline(string path)
	{
		var root = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
			if (attribute.Name.ToString() == "Gram" &&
				attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literal &&
				literal.Token.ValueText.Contains('\n', StringComparison.Ordinal))
				return (literal.Token.ValueText, attribute.ArgumentList.Arguments.Any(static one => one.ToString().Replace(" ", "", StringComparison.Ordinal) == "Lexical=true"));

		throw new InvalidOperationException($"No inline grammar in {path}.");
	}

	static string Key(SharedRecursion.Pair pair)
	{
		return $"L{pair.Level} {pair.Rule.Name} {pair.Shape} {pair.First}/{pair.Second}{(pair.Guarded ? " guarded" : "")}";
	}

	[Theory]
	[MemberData(nameof(Shipped))]
	public void Ratchet(string name)
	{
		var (text, lexical) = Source(name);
		var result   = Report(name, text, lexical);
		var found    = result.Found.Where(static pair => pair.Level >= SharedRecursion.Remembered).Select(Key).OrderBy(static one => one, StringComparer.Ordinal).ToList();
		var expected = Flagged[name].OrderBy(static one => one, StringComparer.Ordinal).ToList();
		var missing  = expected.ToList();
		var added    = new List<string>();

		foreach (var key in found)
			if (!missing.Remove(key))
				added.Add(key);

		Assert.True(
			added.Count == 0 && missing.Count == 0,
			$"{name}: the pairs the analysis finds are not the ones listed. " +
			(added.Count > 0 ? $"New: {string.Join("; ", added)} — write the alternatives to read their shared text once (docs/syntax.md section 4.7), or list the pair here with its nest laddered in SqlSuccessMemoTests. " : "") +
			(missing.Count > 0 ? $"Gone: {string.Join("; ", missing)} — remove them from the list." : ""));
	}

	/// <summary>The PostgreSQL grammar of another branch as it was and after its FROM and GROUP BY shapes were factored: the instances measured exponential are flagged, and gone after.</summary>
	[Theory]
	[InlineData("PostgreSQL 91bf1d63", new[] { "TableRefPrimary", "GroupItem" }, new string[0])]
	[InlineData("PostgreSQL 15cf5f88", new string[0], new[] { "TableRefPrimary", "GroupItem" })]
	public void Recall(string name, string[] flagged, string[] silent)
	{
		var (text, lexical) = Source(name);
		var result  = Report(name, text, lexical);
		var missing = flagged.Where(rule => !result.Found.Any(pair => pair.Level >= SharedRecursion.Remembered && Named(pair.Rule, rule))).ToList();
		var present = silent.Where(rule => result.Found.Any(pair => pair.Level >= SharedRecursion.Remembered && Named(pair.Rule, rule))).ToList();

		Assert.Empty(missing);
		Assert.Empty(present);
	}

	static bool Named(RuleSymbol rule, string name)
	{
		return rule.Name == name || rule.Name.EndsWith("." + name, StringComparison.Ordinal);
	}

	/// <summary>One small grammar per shape: what is flagged at error level and what is not.</summary>
	public static TheoryData<string, string, string, bool> Shapes { get; } = new()
	{
		// The textbook doubling, the shared prefix in two rules so that factoring cannot join it.
		{
			"textbook", "A",
			"A = B | C | 'a'\nB = '(' & A & ')' & 'x'\nC = '(' & A & ')' & 'y'\nStart = A & eof\nparse Start\n",
			true
		},

		// The same written in one rule: nothing factors the shared prefix out, and it is the same shape.
		{
			"textbook, in one rule", "A",
			"A = '(' & A & ')' & 'x' | '(' & A & ')' & 'y' | 'a'\nStart = A & eof\nparse Start\n",
			true
		},

		// A named shape against a call by the same name: `cast ( cast ( …`.
		{
			"cast against a call", "V",
			Split + "V = Cast | Call | Token.Id\nCast = \"cast\" & '(' & V & \"as\" & Token.Id & ')'\n" +
				"Call = Token.Id & '(' & V & ')'\nStart = V & eof\nparse Start\n",
			true
		},

		// The same with the call refusing the named shape's head: separated by `?!`.
		{
			"cast against a call, ?! between", "V",
			Split + "V = Cast | Call | Token.Id\nCast = \"cast\" & '(' & V & \"as\" & Token.Id & ')'\n" +
				"Call = ?!(\"cast\" & '(') & Token.Id & '(' & V & ')'\nStart = V & eof\nparse Start\n",
			false
		},

		// An element against a row that begins with one: `( ( ( a`.
		{
			"row against a bracket", "Row",
			Split + "Row = Elem | '(' & Elem & (',' & Elem)+ & ')'\nElem = '(' & Row & ')' | Token.Id\n" +
				"Start = Row & eof\nparse Start\n",
			true
		},

		// The cover: the bracket read once and its tail chosen by a switch.
		{
			"row against a bracket, switch cover", "Row",
			Split + "Row = '(' & Row & switch @(0) { case 0: ')' case 1: (',' & Row)+ & ')' } | Token.Id\n" +
				"Start = Row & eof\nparse Start\n",
			false
		},
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Shape(string name, string rule, string grammar, bool flagged)
	{
		var result = Report(name, grammar, lexical: grammar.StartsWith(Split, StringComparison.Ordinal));
		var told   = result.Found.Any(pair => pair.Level >= 2 && Named(pair.Rule, rule));

		Assert.Equal(flagged, told);
	}

	/// <summary>
	/// The letter-depth cut-off: a shared prefix of twenty letters before the nested construct is
	/// past it, and the doubling behind it is not seen; five letters are not.
	/// </summary>
	[Theory]
	[InlineData(5, true)]
	[InlineData(20, false)]
	public void LongPrefix(int letters, bool flagged)
	{
		var prefix  = string.Join(" & ", Enumerable.Repeat("'p'", letters));
		var grammar = $"A = B | C | 'a'\nB = {prefix} & '(' & A & ')' & 'x'\nC = {prefix} & '(' & A & ')' & 'y'\nStart = A & eof\nparse Start\n";
		var result  = Report($"shared prefix of {letters} letters", grammar, lexical: false);
		var told    = result.Found.Any(pair => pair.Level >= 2 && Named(pair.Rule, "A"));

		Log($"  shared prefix of {letters} letters: {(told ? "flagged" : "missed")}");

		Assert.Equal(flagged, told);
	}

	/// <summary>
	/// Pairs flagged at the levels the memo answers, each nested through the construct its witness
	/// names, read on the immediate carrier with the memo of successes: the rule entries a parse
	/// costs at each depth grow by a constant a level, where a count that doubled every level was
	/// the shape the analysis is about.
	/// </summary>
	[Theory]
	[InlineData(SqlVariants.Sql92, "InPredicateValue", "TryParseSelect", "SELECT a FROM t WHERE ", "a IN ((SELECT a FROM t WHERE {0}), 2)", "a = 1")]
	[InlineData(SqlVariants.Sql92, "ValueExpressionPrimary", "TryParseSelect", "SELECT a FROM t WHERE ", "((SELECT a FROM t WHERE {0}) UNION (SELECT a FROM t)) = 1", "a = 1")]
	[InlineData(SqlVariants.Sql92, "TablePrimary", "TryParseSelect", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.Sql92, "BooleanPrimary", "TryParseSelect", "SELECT a FROM t WHERE ", "(CASE WHEN {0} THEN 1 END = 1)", "a = 1")]
	[InlineData(SqlVariants.Sql92, "RowValueConstructor", "TryParseSelect", "SELECT a FROM t WHERE ", "((CASE WHEN {0} THEN 1 END, 1) = (1, 1))", "a = 1")]
	[InlineData(SqlVariants.TransactSql, "BooleanPrimary", "TryParseSyntaxSelect", "SELECT a FROM t WHERE ", "(CASE WHEN {0} THEN 1 END = 1)", "a = 1")]
	[InlineData(SqlVariants.TransactSql, "TSqlTablePrimary", "TryParseSyntaxSelect", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.TransactSql, "InPredicateValue", "TryParseSyntaxSelect", "SELECT a FROM t WHERE ", "a IN ((SELECT a FROM t WHERE {0}), 2)", "a = 1")]
	[InlineData(SqlVariants.TransactSql, "Top", "TryParseSyntaxSelect", "SELECT TOP ", "((SELECT TOP {0} a FROM t)) a FROM t", "(1)")]
	[InlineData(SqlVariants.TransactSql, "SetFunctionSpecification", "TryParseSyntaxSelect", "SELECT ", "COUNT({0})", "a")]
	[InlineData(SqlVariants.TransactSql, "TSqlValueFunction", "TryParseSyntaxSelect", "SELECT ", "IIF({0} = 1, 1, 2)", "a")]
	[InlineData(SqlVariants.TransactSql, "ValueFunction", "TryParseSyntaxSelect", "SELECT ", "CHAR_LENGTH({0})", "'a'")]
	[InlineData(SqlVariants.TransactSql, "GroupingSet", "TryParseSyntaxSelect", "SELECT a FROM t GROUP BY ", "ROLLUP((({0})))", "a")]
	[InlineData(SqlVariants.TransactSql, "TSqlPrimaryCore", "TryParseSyntaxSelect", "SELECT a FROM t WHERE ", "((SELECT a FROM t WHERE {0})) = 1", "a = 1")]
	[InlineData(SqlVariants.Standard, "TablePrimary", "TryParseStatement", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.Standard, "ComparisonTail", "TryParseStatement", "SELECT a FROM t WHERE ", "a = SOME ((SELECT a FROM t WHERE {0}))", "a = 1")]
	[InlineData(SqlVariants.Standard, "PrimaryReading", "TryParseStatement", "SELECT a FROM t WHERE ", "((SELECT a FROM t WHERE {0})) = 1", "a = 1")]
	public void Ladder(string grammar, string rule, string publication, string head, string shape, string core)
	{
		var parser = SqlVariants.Parser(grammar, "off", memo: true, counts: true, immediate: true);
		var line   = new StringBuilder($"  ladder {grammar} {rule}:");
		var counts = new List<long>();

		foreach (var depth in new[] { 2, 4, 6, 8, 10, 12 })
		{
			var nest = core;

			for (var level = 0; level < depth; level++)
				nest = shape.Replace("{0}", nest, StringComparison.Ordinal);

			var (count, accepted) = Entries(parser, publication, head + nest);

			counts.Add(count);
			line.Append(System.Globalization.CultureInfo.InvariantCulture, $" {depth}: {count}{(accepted ? "" : " refused")}");
		}

		Log(line.ToString());

		// Linear: what two levels add settles to a constant, so the last two levels add at most what the
		// two before them did, and a little more for a constant that creeps.
		var last   = counts[5] - counts[4];
		var before = counts[4] - counts[3];

		Assert.True(last <= 1.5 * before + 50, $"{grammar} {rule}: the last two levels add {last} entries where the two before added {before}: {line}");
	}

	/// <summary>The rule entries one parse makes, and whether it was accepted.</summary>
	static (long Count, bool Accepted) Entries(Type parser, string publication, string text)
	{
		var counters = parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
			.ToArray();

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)
			?? throw new InvalidOperationException("no " + publication + " on " + parser);

		object?    match = null;
		Exception? error = null;

		var thread = new Thread(() =>
		{
			try
			{
				match = read.Invoke(null, [text]);
			}
			catch (TargetInvocationException thrown)
			{
				error = thrown;
			}
		}, 256 * 1024 * 1024) { IsBackground = true };

		thread.Start();

		if (!thread.Join(TimeSpan.FromSeconds(60)))
			return (-1, false);

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		var accepted = (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

		return (counters.Sum(static counter => (long)counter.GetValue(null)!), accepted);
	}

	SharedRecursion.Result Report(string name, string grammar, bool lexical)
	{
		var watch  = Stopwatch.StartNew();
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(parsed.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var model = GrammarBinder.Bind(parsed.File, null, grammar.Length);
		var graph = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var split = lexical ? LexicalSplit.Of(graph) : null;

		if (lexical)
			Assert.NotNull(split);

		var built = watch.Elapsed.TotalMilliseconds;

		watch.Restart();

		var result = SharedRecursion.Check(graph, split);
		var asked  = watch.Elapsed.TotalMilliseconds;
		var text   = new StringBuilder();

		text.AppendLine(string.Create(
			System.Globalization.CultureInfo.InvariantCulture,
			$"== {name} [{(split is null ? "chars" : "kinds")}]: {result.Rules} rules, {result.Cycles} cycles of {result.Recursive} rules, " +
			$"{result.Choices} choices, {result.Pairs} pairs, {result.Found.Count} flagged; cut: {result.PastStates} past the state budget, " +
			$"{result.PastDepth} past the letter depth, {result.CallsCut} calls; {asked:F0} ms (graph {built:F0} ms)"));

		foreach (var pair in result.Found.OrderByDescending(static pair => pair.Level).ThenBy(static pair => pair.Guarded).ThenBy(static pair => pair.Rule.Name, StringComparer.Ordinal))
			text.AppendLine("  " + SharedRecursion.Describe(pair));

		text.Append(string.Create(
			System.Globalization.CultureInfo.InvariantCulture,
			$"error-level pairs: {result.Found.Count(static pair => pair.Level >= 2)} " +
			$"(L3 {result.Found.Count(static pair => pair.Level == 3)}, L2 {result.Found.Count(static pair => pair.Level == 2)}, " +
			$"guarded {result.Found.Count(static pair => pair.Level >= 2 && pair.Guarded)}); information: {result.Found.Count(static pair => pair.Level == 1)}"));

		Log(text.ToString());

		return result;
	}
}
