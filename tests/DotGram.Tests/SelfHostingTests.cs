using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Examples;
using DotGram.Generation;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The hand-written parser and the generated one, on the same corpus.
/// </summary>
/// <remarks>
/// <para>
/// Two implementations of the notation exist: <see cref="GramParser"/>, written by hand
/// and used by the compiler, and <c>GramGrammar</c>, generated from the grammar of the
/// notation written in itself. They must agree on what is a grammar. This holds them to
/// it over every <c>.gram</c> in the repository — the differential the self-hosting work
/// was started for.
/// </para>
/// <para>
/// The comparison also carries the one honest performance figure the benchmarks project
/// cannot: it deliberately references the generator as an analyzer and nothing else, so
/// the compiler's own front end is not measurable there, and this is where the two sides
/// meet. Round-robin and in-process, the <c>--against</c> discipline: whatever the
/// machine does to one side it does to the other, so the ratio survives what the
/// absolute numbers do not. The work differs in kind — the hand parser builds the
/// compiler's own tree with positions and diagnostics, the generated one builds the
/// example's records — so the ratio is a scale, not a verdict.
/// </para>
/// </remarks>
public sealed class SelfHostingTests(Xunit.ITestOutputHelper output)
{
	[Fact]
	public void Both_implementations_agree_on_the_corpus()
	{
		foreach (var (name, text) in Corpus())
		{
			var handed    = GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance));
			var generated = GramGrammar.TryParseFile(text);

			Assert.True(
				!handed.HasErrors == generated.IsSuccess,
				$"{name}: the hand-written parser says {(handed.HasErrors ? "no" : "yes")}, " +
				$"the generated one says {(generated.IsSuccess ? "yes" : "no")}.");
		}
	}

	[Fact]
	public void And_this_is_what_each_costs()
	{
		const int Parses = 60;

		var corpus = Corpus();

		// Warm both sides thoroughly first. The generated engine is one enormous method,
		// and tiered compilation re-jits it tens of calls in, for tens of milliseconds; a
		// mean over a window holding that spike reported the generated side at 140 times
		// the hand-written one, when the parses themselves were at five. Medians of
		// individual parses are what survive it — the same lesson as the layout lottery,
		// relearned against the JIT.
		foreach (var (_, text) in corpus)
			for (var i = 0; i < 50; i++)
			{
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance));
				GramGrammar.TryParseFile(text);
			}

		foreach (var (name, text) in corpus)
		{
			var hand      = new double[Parses];
			var generated = new double[Parses];
			var watch     = new Stopwatch();

			for (var i = 0; i < Parses; i++)
			{
				watch.Restart();
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance));
				hand[i] = watch.Elapsed.TotalMilliseconds;

				watch.Restart();
				GramGrammar.TryParseFile(text);
				generated[i] = watch.Elapsed.TotalMilliseconds;
			}

			Array.Sort(hand);
			Array.Sort(generated);

			var handMedian      = hand[Parses / 2];
			var generatedMedian = generated[Parses / 2];

			output.WriteLine(
				$"{name,-14} hand {handMedian,7:F3} ms   generated {generatedMedian,7:F3} ms   " +
				$"ratio {generatedMedian / handMedian,5:F2}");
		}
	}

	/// <summary>
	/// Where the two implementations are known to part: a keyword inside a word.
	/// </summary>
	/// <remarks>
	/// §11's ordered choice with full backtracking lets `parse Xas y` be read as
	/// `parse X as y`: the identifier gives two characters back and the keyword matches
	/// mid-word, since §4.6's boundary guards only what <em>follows</em> a keyword. A
	/// lexer tokenizes `Xas` maximally and no such reading exists. The two implementations
	/// therefore accept different languages at this point — found by the stage-1 lexical
	/// inventory, which refused to route these decisions precisely because the proofs
	/// cannot equate them. Pinned so that whichever way the specification resolves it,
	/// a test flips consciously rather than silently.
	/// </remarks>
	[Fact]
	public void The_two_implementations_part_at_a_keyword_inside_a_word()
	{
		var text = "X = 'x'" + '\n' + "parse Xas y" + '\n';

		Assert.True(GramGrammar.TryParseFile(text).IsSuccess, "generated: §11 reads Xas as X-as");
		Assert.True(
			GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).HasErrors,
			"hand-written: Xas is one token");
	}

	/// <summary>The checked-in grammars, the same way the corpus test finds them.</summary>
	static (string Name, string Text)[] Corpus() =>
		[.. Directory
			.GetFiles(Snapshots, "*.gram")
			.Select(path => (Path.GetFileName(path), File.ReadAllText(path)))];

	static string Snapshots =>
		Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
