using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DotGram.Parsers.Sql;

namespace DotGram.Benchmarks;

/// <summary>
/// What the first parse of a grammar costs, which is the one no benchmark can see.
/// </summary>
/// <remarks>
/// <para>
/// A grammar's statics — its character classes, its keyword trie, the transition table of a
/// lexical namespace — are built when something first touches them, and its methods are
/// jitted at tier zero when they are first called. Every measuring instrument in this
/// directory is built to make that go away: BenchmarkDotNet warms until the distribution
/// settles, and it is right to, because for a server the first call is a rounding error
/// against the millionth.
/// </para>
/// <para>
/// It is not a rounding error everywhere. A command-line tool, a source generator and an
/// editor extension may parse a handful of documents and exit, and what they pay is mostly
/// this. So it is measured the only way it can be: one call, once, with nothing having
/// touched that grammar before — no loop, no median, no warming, because warming is the
/// thing being measured.
/// </para>
/// <para>
/// The runtime itself is warm by then and this parser is not, which is the position a
/// consumer's process is in. The order matters and is printed: pass a name to put one
/// grammar first — <c>--prepare Url</c> — and that one's figure is unshared.
/// </para>
/// <para>
/// What a warm call costs is <see cref="PreparationBenchmarks"/>'s question, and it is
/// asked with BenchmarkDotNet because it has to be. The first shape of this harness timed
/// both by hand and read the URL grammar at 430 ns where BenchmarkDotNet reads 160: a
/// timing loop is a hot loop, and one written where it runs once does not leave tier zero.
/// </para>
/// </remarks>
static class Preparation
{
	/// <summary>A grammar, and the least it will read.</summary>
	readonly record struct Subject(string Name, string Shape, string Least, Func<string, bool> Read);

	public static void Run(string? first)
	{
		var subjects = Subjects();

		if (first is not null)
		{
			var wanted = subjects.FindIndex(one => one.Name.Equals(first, StringComparison.OrdinalIgnoreCase));

			if (wanted < 0)
			{
				Console.WriteLine(
					$"No such grammar as {first}. One of {string.Join(", ", subjects.Select(one => one.Name))}.");

				return;
			}

			var moved = subjects[wanted];

			subjects.RemoveAt(wanted);
			subjects.Insert(0, moved);
		}

		var cold = new double[subjects.Count];

		// All of them before anything is printed: a Console.WriteLine between two of these
		// is work the second one did not have to do and the first one did.
		for (var i = 0; i < subjects.Count; i++)
		{
			var watch = Stopwatch.StartNew();

			subjects[i].Read(subjects[i].Least);

			cold[i] = watch.Elapsed.TotalMilliseconds;
		}

		Console.WriteLine();
		Console.WriteLine("what the first parse of a grammar costs");
		Console.WriteLine();
		Console.WriteLine($"  {"grammar",-16}{"shape",-14}{"rules",8}{"first call",14}");
		Console.WriteLine($"  {new string('-', 16 + 14 + 8 + 14)}");

		for (var i = 0; i < subjects.Count; i++)
			Console.WriteLine(
				$"  {subjects[i].Name,-16}{subjects[i].Shape,-14}{Rules(subjects[i].Name),8}" +
				$"{cold[i],11:F1} ms");

		Console.WriteLine();
		Console.WriteLine(
			"  One call each, in the order printed, with nothing having touched that grammar");
		Console.WriteLine(
			"  before: its statics built and its methods jitted at tier zero. What a warm one");
		Console.WriteLine(
			"  costs is --filter *PreparationBenchmarks*.");
	}

	/// <summary>How many rules the grammar has, which is what the first call is a function of.</summary>
	static string Rules(string name) => name switch
	{
		"Levels"      => "4",
		"Url"         => "14",
		"Config"      => "9",
		"Sql-92"      => "~130",
		"TransactSql" => "~640",
		_             => "",
	};

	/// <summary>Two grammars a page long, two that fit on a screen, and one that is four rules.</summary>
	/// <remarks>
	/// Over kinds and over characters both, because they do not pay the same way: a reading
	/// over kinds carries a lexer with a transition table and a keyword trie of its own, and
	/// those are built before the first rule runs.
	/// </remarks>
	static List<Subject> Subjects() =>
	[
		new("Levels", "characters", "1", static text => Levels.TryLevelled(text).IsSuccess),
		new("Url", "characters", "http://a", static text => Urls.TryParseUrl(text).IsSuccess),
		new("Config", "trivia", "a=b;", static text => Config.Read(text).Length >= 0),
		new("Sql-92", "kinds", "a > 1", static text => SqlStandard92.TryParseSearchCondition(text).IsSuccess),
		new("TransactSql", "kinds", "SELECT 1", static text => TransactSql.TryParseStatement(text).IsSuccess),
	];
}
