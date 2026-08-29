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
	/// A keyword does not match inside a word, in either implementation.
	/// </summary>
	/// <remarks>
	/// This used to pin a divergence: §11's backtracking let the generated parser read
	/// `parse Xas y` as `parse X as y`, handing the identifier's tail to the keyword,
	/// because §4.6 guarded only what follows a keyword. The ruling made `Xas` one
	/// lexeme; §4.6 became symmetric — a woven lookbehind refuses a word literal whose
	/// preceding character continues a word — and the two implementations agree again.
	/// </remarks>
	[Fact]
	public void A_keyword_does_not_match_inside_a_word()
	{
		var text = "X = 'x'" + '\n' + "parse Xas y" + '\n';

		Assert.False(GramGrammar.TryParseFile(text).IsSuccess, "generated: Xas is one lexeme");
		Assert.True(
			GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).HasErrors,
			"hand-written: Xas is one token");
	}

	/// <summary>
	/// Both implementations read the two declarations of §7.7 and §7.8, and both still read
	/// a rule that happens to be called by either name.
	/// </summary>
	/// <remarks>
	/// A word and a colon and a type, with nothing after it: what makes `context = 'x'` a
	/// rule rather than a declaration is the `=`, and both halves have to agree about that
	/// or the notation has two meanings.
	/// </remarks>
	[Theory]
	[InlineData("context : @Names\nStart = 'x'\n")]
	[InlineData("state : @Overflow\nStart = 'x'\n")]
	[InlineData("context : @Names\nstate : @Overflow\nStart = 'x'\n")]
	[InlineData("context = 'c'\nstate = 's'\nStart = context & state\n")]
	[InlineData("state : @int\nStart = ('a' & 'b') with state @(1) & 'c'\n")]
	[InlineData("state : @int\nStart = x: 'a'+ with state @(Overflow.Checked)\n")]
	[InlineData("state : @int\nA = 'a'\nStart = A with (A = 'b') with state @(1)\n")]
	public void Both_implementations_read_the_supplied_declarations(string text)
	{
		Assert.True(GramGrammar.TryParseFile(text).IsSuccess, "generated");
		Assert.False(
			GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).HasErrors,
			"hand-written");
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
