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
/// The shared-recursion analysis (<see cref="SharedRecursion"/>) over the SQL and expression
/// grammars and over one small grammar per shape: what it flags, and whether it flags the
/// instances that were measured exponential.
/// </summary>
/// <remarks>
/// A prototype's report, not yet a diagnostic: every grammar's pairs are printed with their
/// witnesses, and what is held is recall on the known instances, silence on the shapes that
/// were mended, and the limit of the letter-depth cut-off.
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

	/// <summary>A grammar of the repository, and the rules whose pairs it must flag at level 2 or more, or must not.</summary>
	public static TheoryData<string> Grammars { get; } = new()
	{
		"T-SQL",
		"SQL:2023",
		"SQL-92",
		"SQL-92 before the rewrite",
		"EL",
		"PostgreSQL 91bf1d63",
		"PostgreSQL 15cf5f88",
	};

	/// <summary>The rules each grammar must have a pair of at error level in, and those it must not.</summary>
	static (string[] Flagged, string[] Silent) Expected(string name)
	{
		return name switch
		{
			"T-SQL"                     => (["BooleanPrimary", "RowValueConstructor", "TSqlPrimaryCore"], []),
			"SQL:2023"                  => (["PrimaryReading"], []),
			// The rewrite reads a bracket once for a condition and a row; three pairs stay at error
			// level, each a bracketed query read and then refused (a list after it, an alias after it,
			// a set operation inside it), and each doubles a level in Ladder: InPredicateValue,
			// TablePrimary, ValueExpressionPrimary against ScalarSubquery.
			"SQL-92"                    => ([], ["BooleanPrimary", "RowValueConstructor"]),
			"SQL-92 before the rewrite" => (["BooleanPrimary", "RowValueConstructor", "InPredicateValue"], []),
			"EL"                        => (["Postfix", "Primary"], []),
			"PostgreSQL 91bf1d63"       => (["TableRefPrimary", "GroupItem"], []),
			"PostgreSQL 15cf5f88"       => ([], ["TableRefPrimary", "GroupItem"]),
			_                           => throw new ArgumentOutOfRangeException(nameof(name)),
		};
	}

	static string Text(string name)
	{
		var root = SourceRoot.Directory;

		return name switch
		{
			"T-SQL"                     => File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "TransactSql", "TransactSql.gram")),
			"SQL:2023"                  => File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "Standard", "SqlStandard.gram")),
			"SQL-92"                    => File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "Standard", "SqlStandard92.gram")),
			"SQL-92 before the rewrite" => File.ReadAllText(Path.Combine(root, "tests", "DotGram.Tests.Slow", "Fixtures", "ReadOnce", "SqlStandard92.gram")),
			"EL"                        => Inline(Path.Combine(root, "src", "DotGram.ExpressionLanguage", "ExpressionParser.cs")),
			"PostgreSQL 91bf1d63"       => File.ReadAllText(Path.Combine(root, "tests", "DotGram.Tests.Slow", "Fixtures", "SharedRecursion", "PostgreSql-91bf1d63.gram")),
			"PostgreSQL 15cf5f88"       => File.ReadAllText(Path.Combine(root, "tests", "DotGram.Tests.Slow", "Fixtures", "SharedRecursion", "PostgreSql-15cf5f88.gram")),
			_                           => throw new ArgumentOutOfRangeException(nameof(name)),
		};
	}

	/// <summary>The grammar written into the first <c>[Gram]</c> of a source, read as the compiler reads the string.</summary>
	static string Inline(string path)
	{
		var root = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
			if (attribute.Name.ToString() == "Gram" &&
				attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literal &&
				literal.Token.ValueText.Contains('\n', StringComparison.Ordinal))
				return literal.Token.ValueText;

		throw new InvalidOperationException($"No inline grammar in {path}.");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void Shipped(string name)
	{
		var result = Report(name, Text(name), lexical: true);

		var (flagged, silent) = Expected(name);
		var missing           = flagged.Where(rule => !result.Found.Any(pair => pair.Level >= 2 && Named(pair.Rule, rule))).ToList();
		var present           = silent.Where(rule => result.Found.Any(pair => pair.Level >= 2 && Named(pair.Rule, rule))).ToList();

		foreach (var rule in flagged)
			Log($"  recall {rule}: {(missing.Contains(rule) ? "MISSED" : "flagged")}");

		foreach (var rule in silent)
			Log($"  gone {rule}: {(present.Contains(rule) ? "STILL FLAGGED" : "yes")}");

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
	/// Pairs flagged at error level, each nested through the construct its witness names, with the
	/// rule entries a parse costs at each depth: a count that doubles every level is the shape the
	/// analysis is about, one that grows by a constant is not. The SQL-92 rows are what the rewritten
	/// grammar still has; the others are a sample of the larger grammars' pairs.
	/// </summary>
	/// <remarks>Reported, not held: what the counts say is the report's to weigh.</remarks>
	[Theory]
	[InlineData(SqlVariants.Sql92, "InPredicateValue", "TryParseSelect", "SELECT a FROM t WHERE ", "a IN ((SELECT a FROM t WHERE {0}), 2)", "a = 1")]
	[InlineData(SqlVariants.Sql92, "ValueExpressionPrimary", "TryParseSelect", "SELECT a FROM t WHERE ", "((SELECT a FROM t WHERE {0}) UNION (SELECT a FROM t)) = 1", "a = 1")]
	[InlineData(SqlVariants.Sql92, "TablePrimary", "TryParseSelect", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.TransactSql, "TSqlTablePrimary", "TryParseSyntaxSelect", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.TransactSql, "InPredicateValue", "TryParseSyntaxSelect", "SELECT a FROM t WHERE ", "a IN ((SELECT a FROM t WHERE {0}), 2)", "a = 1")]
	[InlineData(SqlVariants.TransactSql, "Top", "TryParseSyntaxSelect", "SELECT TOP ", "((SELECT TOP {0} a FROM t)) a FROM t", "(1)")]
	[InlineData(SqlVariants.TransactSql, "SetFunctionSpecification", "TryParseSyntaxSelect", "SELECT ", "COUNT({0})", "a")]
	[InlineData(SqlVariants.TransactSql, "TSqlValueFunction", "TryParseSyntaxSelect", "SELECT ", "IIF({0} = 1, 1, 2)", "a")]
	[InlineData(SqlVariants.TransactSql, "ValueFunction", "TryParseSyntaxSelect", "SELECT ", "CHAR_LENGTH({0})", "'a'")]
	[InlineData(SqlVariants.TransactSql, "GroupingSet", "TryParseSyntaxSelect", "SELECT a FROM t GROUP BY ", "ROLLUP((({0})))", "a")]
	[InlineData(SqlVariants.Standard, "TablePrimary", "TryParseStatement", "SELECT a FROM ", "((SELECT a FROM {0}) AS q)", "t")]
	[InlineData(SqlVariants.Standard, "ComparisonTail", "TryParseStatement", "SELECT a FROM t WHERE ", "a = SOME ((SELECT a FROM t WHERE {0}))", "a = 1")]
	public void Ladder(string grammar, string rule, string publication, string head, string shape, string core)
	{
		var parser = SqlVariants.Parser(grammar, counts: true);
		var line   = new StringBuilder($"  ladder {grammar} {rule}:");
		var last   = 0L;

		foreach (var depth in new[] { 2, 4, 6, 8, 10 })
		{
			var nest = core;

			for (var level = 0; level < depth; level++)
				nest = shape.Replace("{0}", nest, StringComparison.Ordinal);

			var (count, accepted) = Entries(parser, publication, head + nest);

			line.Append(System.Globalization.CultureInfo.InvariantCulture, $" {depth}: {count}{(accepted ? "" : " refused")}");

			if (last > 0)
				line.Append(System.Globalization.CultureInfo.InvariantCulture, $" (x{(double)count / last:F2})");

			last = count;
		}

		Log(line.ToString());
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
