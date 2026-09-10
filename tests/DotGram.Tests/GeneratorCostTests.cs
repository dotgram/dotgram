using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where the time goes when the generator compiles a grammar.
/// </summary>
/// <remarks>
/// <para>
/// Measured from here rather than from <c>benchmarks/</c>, which is deliberate and says so
/// in its own project file: that project references the generator as an analyzer and
/// nothing else, so that it stays the thing a consumer is. This one references both.
/// </para>
/// <para>
/// <b>Cumulative rather than isolated.</b> Each row runs the pipeline from the top and
/// stops one stage later, so a stage's cost is the difference between two rows. Timing a
/// stage on its own would mean building its input first and holding it — and the inputs are
/// what the stages produce, so that is the same work done twice with the caches in a
/// different state. The differences are what the table prints.
/// </para>
/// <para>
/// <b>Medians of whole compilations, not a loop.</b> One compilation of
/// <c>TransactSql.gram</c> is tens of milliseconds, which is far above anything a stopwatch
/// or a tier-zero loop can distort, and the median across runs is what survives the
/// re-jitting spike that <see cref="SelfHostingTests"/> records next door. There is no loop
/// to get wrong here, which is the one advantage of measuring something slow.
/// </para>
/// <para>
/// This is a report and not an assertion: it passes as long as the pipeline runs, and what
/// it is for is that a change to any stage moves a number somebody is looking at.
/// </para>
/// </remarks>
public sealed class GeneratorCostTests(Xunit.ITestOutputHelper output)
{
	const int Runs = 9;

	[Fact]
	public void And_this_is_where_the_generator_spends_it()
	{
		foreach (var (name, text) in Grammars())
		{
			// Warmed on the grammar itself: the stages are one enormous graph of methods and
			// the first compilation of anything pays for all of them at tier zero.
			for (var i = 0; i < 3; i++)
				GramCompiler.Compile(text, Options(name));

			var spliced  = Median(() => StandardLibrary.SplicedOnto(text));
			var lexed    = Median(() => Lex(text));
			var parsed   = Median(() => GramCompiler.Read(text, RoslynCSharpScanner.Instance));
			var bound    = Median(() => Bind(text));
			var shaped   = Median(() => Shape(text));
			var checked_ = Median(() => Checked(text));
			var whole    = Median(() => GramCompiler.Compile(text, Options(name)));


			var made = GramCompiler.Compile(text, Options(name));
			var bad  = made.Diagnostics.Count(one => one.Severity == GramSeverity.Error);
			var size = made.Sources.Sum(one => one.Text.Length);

			output.WriteLine(
				$"{name}  ({text.Length} chars, {bad} errors, " +
				$"{(size > 0 ? size + " chars emitted" : "nothing emitted")}, " +
				$"{size / Math.Max(whole - checked_, 0.001) / 1000:F1} MB/s emitted)");
			output.WriteLine(
				$"  {"splice",-12}{spliced,9:F2} ms" +
				$"{Share(spliced, whole),8}");
			output.WriteLine(
				$"  {"lex",-12}{lexed - spliced,9:F2} ms{Share(lexed - spliced, whole),8}");
			output.WriteLine(
				$"  {"parse",-12}{parsed - lexed,9:F2} ms{Share(parsed - lexed, whole),8}");
			output.WriteLine(
				$"  {"bind",-12}{bound - parsed,9:F2} ms{Share(bound - parsed, whole),8}");
			output.WriteLine(
				$"  {"normalize",-12}{shaped - bound,9:F2} ms{Share(shaped - bound, whole),8}");
			output.WriteLine(
				$"  {"check",-12}{checked_ - shaped,9:F2} ms{Share(checked_ - shaped, whole),8}");
			output.WriteLine(
				$"  {"emit",-12}{whole - checked_,9:F2} ms{Share(whole - checked_, whole),8}");
			output.WriteLine($"  {"whole",-12}{whole,9:F2} ms");
			output.WriteLine("");
		}
	}

	static object Lex(string text) =>
		GramLexer.Tokenize(
			StandardLibrary.SplicedOnto(text), RoslynCSharpScanner.Instance, text.Length,
			StandardLibrary.Scanner);

	static object Bind(string text) =>
		GrammarBinder.Bind(
			GramCompiler.Read(text, RoslynCSharpScanner.Instance).File, PermissiveSymbolResolver.Instance, text.Length);

	static object Shape(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramCompiler.Read(text, RoslynCSharpScanner.Instance).File, PermissiveSymbolResolver.Instance, text.Length),
			PermissiveSymbolResolver.Instance, RoslynCSharpScanner.Instance);

	/// <summary>Everything up to and including the two checks a sound grammar earns.</summary>
	static object Checked(string text)
	{
		var graph = (RecognitionGraph)Shape(text);

		return (Retention.Check(graph).Count(), FirstSets.Check(graph).Count());
	}

	static GramCompilerOptions Options(string name) =>
		new()
		{
			ClassName    = "Measured",
			Namespace    = "DotGram.Tests.Measured",
			CSharpScanner  = RoslynCSharpScanner.Instance,

			// Every `@Type` accepted, which is what lets a grammar written against a host's
			// own types be compiled here at all: without it `TransactSql.gram` is five
			// hundred unresolved names and nothing is emitted, so the row measures a
			// compilation that stopped rather than one that finished.
			SymbolResolver = PermissiveSymbolResolver.Instance,
			Lexical        = name == "SqlStandard92.gram",
		};

	/// <summary>The median of nine whole runs, which is what survives a re-jitting spike.</summary>
	static double Median(Func<object> once)
	{
		var taken = new double[Runs];
		var watch = new Stopwatch();

		for (var i = 0; i < Runs; i++)
		{
			// A collection triggered by the run before lands in this one otherwise, and the
			// stages allocate a whole graph each: the first shape of this table had a stage
			// costing more than the pipeline that contains it.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			watch.Restart();

			var kept = once();

			taken[i] = watch.Elapsed.TotalMilliseconds;

			GC.KeepAlive(kept);
		}

		Array.Sort(taken);

		return taken[Runs / 2];
	}

	static string Share(double part, double whole) =>
		whole <= 0 ? "" : $"{100.0 * part / whole,6:F1}%";

	/// <summary>
	/// One grammar the size of a specification and the small ones beside it.
	/// </summary>
	/// <remarks>
	/// <c>TransactSql.gram</c> is the obvious other heavy one and is not here: it names
	/// <c>Sql92.Lexical.CharacterStringLiteral</c> and the rest of what its host gives it
	/// with <c>[GramInclude]</c>, so compiled from the file alone it is five hundred
	/// unresolved names and nothing is emitted — a row measuring a compilation that stopped,
	/// beside rows measuring compilations that finished. What it costs in place is what the
	/// build of <c>DotGram.Parsers</c> costs, and that is a different measurement.
	/// </remarks>
	static (string Name, string Text)[] Grammars() =>
	[
		("SqlStandard92.gram", File.ReadAllText(Path.Combine(Parsers, "Sql", "Standard", "SqlStandard92.gram"))),
		("SqlStandard92.gram, whole", File.ReadAllText(Path.Combine(Parsers, "Sql", "Standard", "SqlStandard92.gram"))),
		.. Directory
			.GetFiles(Snapshots, "*.gram")
			.Select(path => (Path.GetFileName(path), File.ReadAllText(path))),
	];

	static string Parsers =>
		Path.Combine(Root, "src", "DotGram.Parsers");

	static string Snapshots =>
		Path.Combine(Root, "tests", "Snapshots");

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
