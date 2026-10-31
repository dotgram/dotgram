using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// What one editor analysis of the repository's largest grammars costs, phase by phase: reported, never
/// asserted.
/// </summary>
/// <remarks>
/// <para>
/// An editor re-analyses a grammar from its whole text after every pause in typing, so the cost of one
/// analysis is what decides whether the editor needs anything cleverer — a faster tier that leaves out
/// part of the compiler, or re-analysis of only what an edit touched. That decision is to be made on
/// numbers, and this is where they are taken, the same way every time: the analysis the editor's
/// session runs (<see cref="GramAnalysisSession.Analyze(string, StandaloneGrammarContext?, bool, GramAnalysisClock?)"/>),
/// its timing as the session records it (the compiler's part and the editor's after it, and what it
/// allocates), and the compiler's first stages — reading, binding, normalization — timed apart, so
/// that the rest of the compiler's part is the checks and emission together.
/// </para>
/// <para>
/// Medians of three runs after one to warm up, in one process. Written to standard output, as a
/// diagnostic message and to <c>EditorAnalysisCost.txt</c> beside the assembly. Nothing fails on a
/// time: a shared machine or a CI runner makes any limit either flaky or meaningless.
/// </para>
/// </remarks>
public sealed class EditorAnalysisCostTests
{
	const int Runs = 3;

	[Fact]
	public void The_cost_of_one_analysis_is_reported_by_phase()
	{
		var report = new StringBuilder();

		report.Append("# One editor analysis per grammar, medians of ").Append(Runs).Append(" runs in ms, ")
			.Append(DateTime.UtcNow.ToString("u", System.Globalization.CultureInfo.InvariantCulture)).Append("; ")
			.Append(Environment.ProcessorCount).Append(" processors, ").Append(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription).Append('\n');
		report.Append("grammar | chars | total | compile | of it: read | bind | normalize | checks + emission | post-processing | allocated MB\n");

		foreach (var (name, text) in Grammars())
		{
			var readings = Enumerable.Range(0, Runs + 1).Select(_ => Measure(text)).Skip(1).ToArray();

			Assert.All(readings, static reading => Assert.True(reading.Timing.Compile > TimeSpan.Zero));

			var total      = Median(readings.Select(static reading => reading.Timing.Total.TotalMilliseconds));
			var compile    = Median(readings.Select(static reading => reading.Timing.Compile.TotalMilliseconds));
			var read       = Median(readings.Select(static reading => reading.Read));
			var bind       = Median(readings.Select(static reading => reading.Bind));
			var normalize  = Median(readings.Select(static reading => reading.Normalize));
			var post       = Median(readings.Select(static reading => reading.Timing.PostProcessing.TotalMilliseconds));
			var allocated  = Median(readings.Select(static reading => reading.Timing.AllocatedBytes / 1048576.0));
			var rest       = Math.Max(0, compile - read - bind - normalize);
			var line       = FormattableString.Invariant(
				$"{name} | {text.Length} | {total:F0} | {compile:F0} ({Share(compile, total)}) | {read:F0} | {bind:F0} | {normalize:F0} ({Share(normalize, total)}) | {rest:F0} ({Share(rest, total)}) | {post:F0} ({Share(post, total)}) | {allocated:F0}");

			report.Append(line).Append('\n');
			Console.WriteLine("editor analysis cost | " + line);
			TestContext.Current.SendDiagnosticMessage("editor analysis cost | " + line);
		}

		File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "EditorAnalysisCost.txt"), report.ToString());
	}

	sealed record Reading(GramAnalysisTiming Timing, double Read, double Bind, double Normalize);

	static Reading Measure(string text)
	{
		// What the editor's session runs for a grammar with a host that reads it as tokens.
		var context = new StandaloneGrammarContext("", [], Lexical: true);
		var clock   = GramAnalysisClock.Start();
		var result  = GramAnalysisSession.Analyze(text, context, suppressContextDiagnostics: false, clock);

		Assert.NotEmpty(result.Document.Classifications);

		// The compiler's first stages, as Compile runs them.
		var watch  = Stopwatch.StartNew();
		var parsed = GramCompiler.Read(text, RoslynCSharpScanner.Instance);
		var read   = watch.Elapsed.TotalMilliseconds;

		watch.Restart();
		var model = GrammarBinder.Bind(parsed.File, null, text.Length);
		var bind  = watch.Elapsed.TotalMilliseconds;

		watch.Restart();
		GrammarNormalizer.Normalize(model, null, RoslynCSharpScanner.Instance);
		var normalize = watch.Elapsed.TotalMilliseconds;

		return new Reading(clock.Timing(text.Length, published: true), read, bind, normalize);
	}

	static double Median(IEnumerable<double> values)
	{
		var sorted = values.OrderBy(static value => value).ToArray();

		return sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;
	}

	static string Share(double part, double total)
	{
		return total <= 0 ? "-" : FormattableString.Invariant($"{100 * part / total:F0}%");
	}

	/// <summary>The three grammars the editor's cost is decided on: the two largest SQL dialects, and the expression language.</summary>
	static IEnumerable<(string Name, string Text)> Grammars()
	{
		yield return ("TransactSql.gram", File.ReadAllText(Path.Combine(Root, "src", "DotGram.Sql", "TransactSql", "TransactSql.gram")));
		yield return ("SqlStandard.gram", File.ReadAllText(Path.Combine(Root, "src", "DotGram.Sql", "Standard", "SqlStandard.gram")));

		var source = File.ReadAllText(Path.Combine(Root, "src", "DotGram.ExpressionLanguage", "ExpressionParser.cs"));
		var grammar = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>()
			.Where(static attribute => attribute.Name.ToString() == "Gram")
			.Select(static attribute => attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression)
			.Select(static expression => expression is null ? null : EditorCorpusTests.Constant(expression))
			.First(static text => text is not null && text.IndexOf('\n') >= 0)!;

		yield return ("ExpressionParser.cs", grammar);
	}

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
