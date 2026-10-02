using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Benchmarks;
using DotGram.Sql;
using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The memo of failures changes no answer: every SQL parser compiled with it and without it reads
/// the same texts to the same end, refuses them at the same place with the same message, and builds
/// the same tree — whole, from a position and in a window, under every value of the positional
/// follow.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it can be wrong.</b> A remembered failure answers an entry without reading it, and a
/// reading leaves more than its answer: what it expected, recorded on the failure, and the records a
/// carrier keeps. The memo is sound only where a second reading would have left nothing the first
/// had not, and the analysis that says where is the generator's own (<c>Machine.Memo.cs</c>). This
/// holds it to the parser without the memo, compiled from the same grammar the same way but for
/// <c>DOTGRAM_NO_MEMO</c>.
/// </para>
/// <para>
/// <b>What is read.</b> For T-SQL, every statement of <c>tests/Corpus/ScriptDom</c> as the round trip
/// cuts it, whole; each once more cut short at a place chosen at random, which puts a refusal
/// anywhere in it rather than only where the corpus has one; from a place in it and in a window of
/// it; and every sixteenth through every publication the parser has. For SQL:2023 and SQL-92, every
/// text their own tests are written with, and every fourth corpus statement, the same ways. And for
/// all three, the nests of brackets the memo is for, accepted and refused, at fifty levels. The
/// random places are seeded, so a failure can be run again.
/// </para>
/// <para>
/// <b>What an answer is.</b> The outcome, where the match stands and how long it is, the message a
/// refusal gives — which carries what was expected — and the tree as the package's writer writes it
/// back.
/// </para>
/// <para>
/// One class for each value of the positional follow, so that the three run beside each other:
/// what they cost is mostly compiling the grammars, twice each.
/// </para>
/// </remarks>
public abstract class FailureMemoEqualityTests(ITestOutputHelper output, string follow)
{
	[Fact]
	public void TSql_reads_the_corpus_the_same_with_the_memo_and_without()
	{
		var with    = SqlVariants.Parser(SqlVariants.TransactSql, follow, memo: true);
		var without = SqlVariants.Parser(SqlVariants.TransactSql, follow, memo: false);
		var all     = Publications(with);
		var random  = new Random(follow.Length * 7919);
		var wrong   = new List<string>();
		var count   = 0;
		var index   = 0;

		foreach (var statement in Statements())
		{
			Compare(with, without, "TryParseStatement", statement, random, ref count, wrong);

			if (index++ % 16 == 0)
				foreach (var name in all)
					Compare(with, without, name, statement, random, ref count, wrong);
		}

		foreach (var (name, text) in Nests(Tsql: true))
			Compare(with, without, name, text, random, ref count, wrong);

		Report("T-SQL", count, wrong, 50_000);
	}

	[Fact]
	public void SQL_2023_reads_its_tests_the_same_with_the_memo_and_without()
	{
		var with    = SqlVariants.Parser(SqlVariants.Standard, follow, memo: true);
		var without = SqlVariants.Parser(SqlVariants.Standard, follow, memo: false);

		Across(with, without, "SQL:2023", Harvested("SqlStandard"));
	}

	[Fact]
	public void SQL_92_reads_its_tests_the_same_with_the_memo_and_without()
	{
		var with    = SqlVariants.Parser(SqlVariants.Sql92, follow, memo: true);
		var without = SqlVariants.Parser(SqlVariants.Sql92, follow, memo: false);

		Across(with, without, "SQL-92", Harvested("Sql92"));
	}

	/// <summary>Texts through every publication of a parser, three of them a text, and the nests through every one.</summary>
	void Across(Type with, Type without, string grammar, IEnumerable<string> texts)
	{
		var all    = Publications(with);
		var random = new Random(grammar.Length * 31 + follow.Length);
		var wrong  = new List<string>();
		var count  = 0;

		foreach (var text in texts.Concat(Statements().Where((_, i) => i % 4 == 0)))
			for (var i = 0; i < 3; i++)
				Compare(with, without, all[random.Next(all.Count)], text, random, ref count, wrong);

		foreach (var (_, text) in Nests(Tsql: false))
			foreach (var name in all)
				Compare(with, without, name, text, random, ref count, wrong);

		Report(grammar, count, wrong, 20_000);
	}

	void Report(string grammar, int count, List<string> wrong, int least)
	{
		output.WriteLine($"{grammar}, positional follow {follow}: {count:N0} answers compared, {wrong.Count} different.");

		Assert.True(count >= least, $"{grammar}: only {count} answers were compared.");
		Assert.True(wrong.Count == 0, $"{grammar}, positional follow {follow}: of {count} answers, these differ:\n" + string.Join("\n", wrong.Take(20)));
	}

	/// <summary>
	/// One publication asked of a text four ways: whole, cut short at a random place, from a random
	/// place, and in a random window from there.
	/// </summary>
	static void Compare(Type with, Type without, string name, string text, Random random, ref int count, List<string> wrong)
	{
		var cut    = random.Next(text.Length + 1);
		var at     = random.Next(text.Length + 1);
		var length = random.Next(text.Length - at + 1);

		(string Form, object[] Arguments)[] asked =
		[
			("whole", [text]),
			("cut", [text.Substring(0, cut)]),
			("at", [text, at]),
			("window", [text, at, length]),
		];

		foreach (var (form, arguments) in asked)
		{
			var one   = Answer(with, name, arguments);
			var other = Answer(without, name, arguments);

			count++;

			if (!string.Equals(one, other, StringComparison.Ordinal))
				wrong.Add($"{name} {form} ({string.Join(", ", arguments.Skip(1))}) of {PositionalSplitWebTests.Show((string)arguments[0])}:\n  with    {one}\n  without {other}");
		}
	}

	/// <summary>What one form of a publication answers, written out.</summary>
	static string Answer(Type parser, string name, object[] arguments)
	{
		var method = parser.GetMethod(
			name, BindingFlags.Public | BindingFlags.Static, binder: null,
			[.. arguments.Select(static one => one.GetType())], modifiers: null);

		if (method is null)
			return "no such form";

		object match;

		try
		{
			match = method.Invoke(null, arguments)!;
		}
		catch (TargetInvocationException thrown)
		{
			return "threw " + thrown.InnerException?.GetType().Name;
		}

		var kind = match.GetType();

		return
			$"{kind.GetProperty("Outcome")!.GetValue(match)} at {kind.GetProperty("Position")!.GetValue(match)}" +
			$"+{kind.GetProperty("Length")!.GetValue(match)}: {kind.GetProperty("Error")!.GetValue(match)} | " +
			((bool)kind.GetProperty("IsSuccess")!.GetValue(match)! ? Written(kind.GetProperty("Value")!.GetValue(match)) : "");
	}

	/// <summary>A tree written back by the writer of the tree it is, or member by member where there is none.</summary>
	static string Written(object? value)
	{
		switch (value)
		{
			case null:
				return "null";

			case ISqlNode node:
				return Sql2023Writer.Write(node);

			case System.Collections.IEnumerable items when value is not string:
				return "[" + string.Join("; ", items.Cast<object?>().Select(Written)) + "]";
		}

		var writer = typeof(SqlWriter).GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(one => one.Name == "Write" && one.GetParameters() is [var only] && only.ParameterType.IsInstanceOfType(value))
			.OrderBy(one => Depth(one.GetParameters()[0].ParameterType))
			.LastOrDefault();

		try
		{
			return writer is not null
				? (string)writer.Invoke(null, [value])!
				: PositionalSplitWebTests.Dump(value);
		}
		catch (TargetInvocationException thrown)
		{
			return "the writer threw " + thrown.InnerException?.GetType().Name + "; " + PositionalSplitWebTests.Dump(value);
		}
	}

	static int Depth(Type type)
	{
		var depth = 0;

		for (var at = type; at is not null; at = at.BaseType)
			depth++;

		return depth;
	}

	/// <summary>Every publication of a parser that answers with a match.</summary>
	static List<string> Publications(Type parser)
	{
		return
		[
			.. parser.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(static one => one.Name.StartsWith("TryParse", StringComparison.Ordinal) &&
					one.GetParameters() is [var only] && only.ParameterType == typeof(string) &&
					one.ReturnType.Name.StartsWith("Match", StringComparison.Ordinal))
				.Select(static one => one.Name)
				.Distinct()
				.OrderBy(static one => one, StringComparer.Ordinal),
		];
	}

	/// <summary>The nests the memo is for, at fifty levels, with the publication each is read by in T-SQL.</summary>
	static IEnumerable<(string Name, string Text)> Nests(bool Tsql)
	{
		const int n = 50;

		var open  = new string('(', n);
		var close = new string(')', n);

		yield return ("TryParseSearchCondition", open + "a = 1" + close);
		yield return ("TryParseSearchCondition", open + "a = 1");
		yield return ("TryParseSearchCondition", open + "a" + close + " = 1");
		yield return ("TryParseSearchCondition", open + "a" + close + " =");
		yield return ("TryParseSearchCondition", open + open + "a" + close + " = 1" + close);
		yield return ("TryParseStatement", "SELECT " + open + "1");
		yield return ("TryParseStatement", "SELECT * FROM " + string.Concat(Enumerable.Repeat("(SELECT * FROM ", n)) + "t");
		yield return ("TryParseStatement", "SELECT 1 WHERE " + open + "a = 1" + close);

		if (!Tsql)
		{
			yield return ("", "SELECT " + open + "a" + close + " FROM t");
			yield return ("", "SELECT " + open + "a");
			yield return ("", open + "a + 1" + close + " * 2");
			yield return ("", open + "a + 1");
		}
	}

	/// <summary>The T-SQL corpus, as the round trip cuts it.</summary>
	static List<string> Statements()
	{
		return _statements.Value;
	}

	static readonly Lazy<List<string>> _statements = new(static () => [.. CorpusRoundTrip.Statements(Corpus())]);

	static string Corpus([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Corpus", "ScriptDom"));
	}

	/// <summary>The string literals of the SQL tests whose file names begin so.</summary>
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

/// <inheritdoc cref="FailureMemoEqualityTests"/>
public sealed class FailureMemoEqualityWithoutFollowTests(ITestOutputHelper output) : FailureMemoEqualityTests(output, "off");

/// <inheritdoc cref="FailureMemoEqualityTests"/>
public sealed class FailureMemoEqualityFollowTests(ITestOutputHelper output) : FailureMemoEqualityTests(output, "true");

/// <inheritdoc cref="FailureMemoEqualityTests"/>
public sealed class FailureMemoEqualitySplitFollowTests(ITestOutputHelper output) : FailureMemoEqualityTests(output, "split");
