using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What explaining a refusal reached in very many ways costs once the list of the sets that tied is
/// long: each further tie past the cap is dropped without a look at the list, and what is asked of the
/// list at every way — whether it holds a set, or a rule's own words — is answered by its count rather
/// than by a search. A search of the whole list each time made a refusal of a dozen characters or so take
/// seconds in the engine and up to a minute in a reader, all of it in the pass that records the refusal;
/// the pass that only answers was milliseconds throughout.
/// </summary>
/// <remarks>
/// Gated by counting rather than by a clock: the parser is compiled with <c>DOTGRAM_COUNTS</c>, and
/// <c>TiesVisited_DotGram</c> sums the sets each search of the list looked at. The messages and the
/// number of sets the match keeps are those the cap gives (<see cref="RefusalTiesTests"/>).
/// </remarks>
public sealed class RefusalTiesCostTests
{
	const string Runs = "X = 'b'* & 'b'* & 'b'* & 'b'* & 'b'* & 'b'* & 'b'* & 'b'*\n";

	/// <summary>Eight runs that give back, then a terminal: every way of splitting the input fails on <c>'d'</c> at its end.</summary>
	internal const string Starved = "T = X & 'd'\n" + Runs + "parse T\n";

	/// <summary>The same, with two optional calls before it: three sets take turns, which a reader's check against the last tie does not drop.</summary>
	internal const string TakingTurns = "T = X & C? & E? & 'd'\nC = 'c' & 'x'\nE = 'e' & 'y'\n" + Runs + "parse T\n";

	/// <summary>
	/// The same, ending in a choice whose widest group begins with a call: where nothing the choice
	/// holds comes next, the call refuses, and the choice says its own set over the call's
	/// (<c>Refuse_DotGram_Over</c>) at every way, asking the list whether it holds a rule's own words and
	/// the set the call said.
	/// </summary>
	internal const string CalledOver =
		"T = X & C? & E? & (W | '!')\nW : @int = t: ['g'..'z']+ => @(1)\nC = 'c' & 'x'\nE = 'e' & 'y'\n" + Runs + "parse T\n";

	/// <summary>Two guards that say no and a terminal, after the runs: the guards ask the list whether it holds their sets at every way.</summary>
	internal const string Guarded = "T = X & (when @(false) | when @(1 == 2) | 'd')\n" + Runs + "parse T\n";

	/// <summary>
	/// The engine records every way's set, so the list is full after the first sixteen thousand of
	/// some quarter of a million ways; a reader drops a repeat of the last tie, so it fills where three
	/// sets take turns. After the one search at the cap that appends the mark, nothing looks at the list.
	/// </summary>
	[Theory]
	[InlineData(Starved, false, "Expected 'd', among others.")]
	[InlineData(TakingTurns, true, "Expected \"cx\", \"ey\" or 'd', among others.")]
	public void A_full_list_of_ties_is_not_searched_again(string grammar, bool direct, string said)
	{
		var parser  = Counted(grammar, direct);
		var visited = Visited(parser);
		var input   = new string('b', 16);

		visited.SetValue(null, 0L);

		var (tied, cap, message) = TieShapes.Ask(parser, "TryParseT", input);

		Assert.Equal(said, message);
		Assert.Equal(TieShapes.Cap + 1, tied);
		Assert.Equal(TieShapes.Cap, cap);
		Assert.Equal((long)TieShapes.Cap, (long)visited.GetValue(null)!);
	}

	/// <summary>
	/// What is asked of the list at every way is answered by its count once it is long: the list is
	/// searched about as often as it is filled, whatever the number of ways. The messages are those the
	/// searches gave.
	/// </summary>
	[Theory]
	[InlineData(CalledOver, true, "?", "Expected \"cx\", \"ey\" or ['g'..'z' | '!'], among others.")]
	[InlineData(Guarded, false, "", "Expected 'd', among others.")]
	public void What_is_asked_of_a_long_list_of_ties_is_not_searched_for(string grammar, bool direct, string after, string said)
	{
		var parser  = Counted(grammar, direct);
		var visited = Visited(parser);

		foreach (var length in new[] { 12, 14 })
		{
			visited.SetValue(null, 0L);

			var (tied, _, message) = TieShapes.Ask(parser, "TryParseT", new string('b', length) + after);

			Assert.Equal(said, message);
			Assert.Equal(TieShapes.Cap + 1, tied);
			Assert.True((long)visited.GetValue(null)! < 3L * TieShapes.Cap, $"{visited.GetValue(null)} sets looked at in the list, at {length} characters.");
		}
	}

	/// <summary>
	/// A list that counts what it holds answers as a search of it does, through every way it is changed:
	/// ties, a tail taken back, a set taken out, a guard given up, and a <c>Clear</c> it is not told of.
	/// </summary>
	[Fact]
	public void A_counted_list_answers_as_a_search_does()
	{
		var parser  = Counted(CalledOver, direct: true);
		var failed  = parser.GetNestedType("Failure", BindingFlags.NonPublic)!;
		var counted = parser.GetNestedType("TieList_DotGram", BindingFlags.NonPublic)!;
		var tie     = parser.GetMethod("Tie_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var holds   = parser.GetMethod("HoldsTie_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var removed = parser.GetMethod("Removed_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var untied  = parser.GetMethod("Untied_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var spoke   = parser.GetMethod("Spoke_DotGram", BindingFlags.NonPublic | BindingFlags.Static, [typeof(List<string[]>)])!;
		var sets    = (List<string[]>)Activator.CreateInstance(counted)!;
		var failure = Activator.CreateInstance(failed)!;
		var random  = new Random(20261124);
		var flag    = counted.GetField("Counted", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var asked   = 0;

		string[][] pool =
		[
			["'a'"], ["'b'"], ["'c'", "'d'"], ["\u0000its own words"], ["'e'", "\u0000its own words"],
			["\u0002when @(false)"], ["\u0002when @(true == false)"], ["'f'"],
		];

		failed.GetField("ExpectedMore")!.SetValue(failure, sets);

		for (var step = 0; step < 20000; step++)
		{
			var roll = random.Next(100);

			if (roll < 90)
			{
				object[] arguments = [failure, pool[random.Next(pool.Length)]];

				tie.Invoke(null, arguments);
				failure = arguments[0];
			}
			else if (roll < 94 && sets.Count > 0)
			{
				untied.Invoke(null, [sets, random.Next(sets.Count)]);
			}
			else if (roll < 99 && sets.Count > 0)
			{
				removed.Invoke(null, [sets, random.Next(sets.Count)]);
			}
			else
			{
				sets.Clear();
			}

			foreach (var set in pool)
				Assert.Equal(sets.Contains(set), (bool)holds.Invoke(null, [sets, set])!);

			Assert.Equal(sets.Exists(static set => set.Any(static one => one.Length > 0 && one[0] == '\u0000')), (bool)spoke.Invoke(null, [sets])!);

			if ((bool)flag.GetValue(sets)!)
				asked++;
		}

		Assert.True(asked > 5000, $"the list was counted at {asked} steps of 20000.");
	}

	/// <summary>
	/// The one path on which the list is searched after it was cut: sets taken back below the cap (a
	/// call refused over the ties, a replayed reading), a guard let in, a set that names something
	/// taking the guard's place, and the mark of the cut appended again. Past it, every tie is dropped,
	/// a guard's as well.
	/// </summary>
	/// <remarks>Over a list that counts what it holds and over a plain one, which a search answers.</remarks>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_list_cut_and_taken_back_lets_a_guard_in_and_out_again(bool counting)
	{
		var parser  = Counted(Starved, direct: false);
		var visited = Visited(parser);
		var failed  = parser.GetNestedType("Failure", BindingFlags.NonPublic)!;
		var tie     = parser.GetMethod("Tie_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var mark    = (string[])parser.GetField("Truncated_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var guard   = new[] { "\u0002false" };
		var counted = parser.GetNestedType("TieList_DotGram", BindingFlags.NonPublic)!;
		var sets    = counting ? (List<string[]>)Activator.CreateInstance(counted)! : new List<string[]>();
		var failure = Activator.CreateInstance(failed)!;

		failed.GetField("ExpectedMore")!.SetValue(failure, sets);

		void Tie(string[] expected)
		{
			object[] arguments = [failure, expected];

			tie.Invoke(null, arguments);
			failure = arguments[0];
		}

		long Looked()
		{
			var looked = (long)visited.GetValue(null)!;

			visited.SetValue(null, 0L);

			return looked;
		}

		for (var one = 0; one < TieShapes.Cap; one++)
			sets.Add(["'x'"]);

		Looked();

		// At the cap: one search, no guard found, the mark appended.
		Tie(["'y'"]);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal(TieShapes.Cap, Looked());

		// Past it: dropped at once, a guard's set as well.
		for (var one = 0; one < 1000; one++)
			Tie(["'z'"]);

		Tie(guard);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal(0, Looked());

		// A tail taken back, the mark with it: the list is below the cap, and a guard enters once. Whether
		// the list holds it is asked once, the list being full after it: a search, or the count taken.
		sets.RemoveRange(TieShapes.Cap - 1, 2);
		Tie(guard);
		Tie(guard);
		Assert.Equal(TieShapes.Cap, sets.Count);
		Assert.Same(guard, sets[^1]);
		Assert.Equal(TieShapes.Cap - 1, Looked());

		// At the cap a set that names something takes the guard's place: one search.
		Tie(["'w'"]);
		Assert.Equal(TieShapes.Cap, sets.Count);
		Assert.DoesNotContain(sets, static one => one.Length == 1 && one[0][0] == '\u0002');
		Assert.Equal(new[] { "'w'" }, sets[^1]);
		Assert.Equal(TieShapes.Cap, Looked());

		// The next finds no guard and appends the mark again: one more search, and none after it.
		Tie(["'v'"]);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal(TieShapes.Cap, Looked());

		for (var one = 0; one < 1000; one++)
			Tie(["'u'"]);

		Tie(guard);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal(0, Looked());

		if (!counting)
			return;

		// The count is what the list holds.
		var held = (Dictionary<string[], int>)counted.GetField("Held", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sets)!;

		Assert.True((bool)counted.GetField("Counted", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sets)!);
		Assert.Equal(sets.Count, held.Values.Sum());

		foreach (var (set, times) in held)
			Assert.Equal(sets.Count(one => ReferenceEquals(one, set)), times);
	}

	/// <summary>
	/// The count a long list keeps is right only while every set added to it or taken out of it, but by
	/// <c>Clear</c>, goes through the support's three helpers. Nothing else in what the generator writes,
	/// in its emitters or in the files it wrote for the snapshots, adds to the list of ties or takes from it.
	/// </summary>
	[Fact]
	public void Only_the_helpers_change_the_list_of_ties()
	{
		var root    = Path.GetDirectoryName(Path.GetDirectoryName(SnapshotTests.Directory)!)!;
		var changes = new Regex(@"\b(more|tied|ties|ExpectedMore)\??\.(Add|AddRange|Insert|InsertRange|Remove|RemoveAt|RemoveRange|RemoveAll|Reverse|Sort)\(|\b(more|tied|ties)\[[^\]]+\]\s*=[^=]");

		string[] helpers = ["more.Add(set);", "more.RemoveAt(at);", "more.RemoveRange(from, more.Count - from);"];

		var files = Directory.GetFiles(Path.Combine(root, "src", "DotGram", "Grammar", "Emit"), "*.cs")
			.Concat(Directory.GetFiles(SnapshotTests.Directory, "*.g.cs"));

		var found = new List<string>();

		foreach (var file in files)
		{
			var lines = File.ReadAllLines(file);

			for (var at = 0; at < lines.Length; at++)
				if (changes.IsMatch(lines[at]) && !helpers.Contains(lines[at].Trim()))
					found.Add($"{Path.GetFileName(file)}:{at + 1}: {lines[at].Trim()}");
		}

		Assert.True(found.Count == 0, "the list of ties changed outside Added_DotGram, Removed_DotGram and Untied_DotGram:\n" + string.Join("\n", found));
	}

	static readonly ConcurrentDictionary<(string, bool), Type> Built = new();

	/// <summary>A grammar compiled with the counters of <c>DOTGRAM_COUNTS</c>, over a string, without buffering.</summary>
	internal static Type Counted(string grammar, bool direct)
	{
		return Built.GetOrAdd((grammar, direct), static key =>
		{
			var result = GramCompiler.Compile(key.Item1, new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Direct        = key.Item2,
				CountRules    = true,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics);

			return EmittedCode.Compile(
				result.Sources[0].Text,
				sourceParts: result.Sources.Skip(1).Select(static one => one.Text),
				symbols: ["DOTGRAM_COUNTS"]).GetType("Grammar")!;
		});
	}

	internal static FieldInfo Visited(Type parser)
	{
		return parser.GetField("TiesVisited_DotGram", BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new InvalidOperationException("the parser counts no visits to its ties; is the counter still written under DOTGRAM_COUNTS?");
	}
}
