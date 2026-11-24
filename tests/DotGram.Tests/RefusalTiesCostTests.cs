using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What explaining a refusal reached in very many ways costs once the list of the sets that tied is
/// full: each further tie is dropped without a look at the list. A search of the whole list on every
/// tie past the cap made a refusal of sixteen characters take seconds in the engine and half a minute
/// in a reader, all of it in the pass that records the refusal; the pass that only answers was
/// milliseconds throughout.
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
	/// The one path on which the list is searched after it was cut: sets taken back below the cap (a
	/// call refused over the ties, a replayed reading), a guard let in, a set that names something
	/// taking the guard's place, and the mark of the cut appended again. Past it, every tie is dropped,
	/// a guard's as well.
	/// </summary>
	[Fact]
	public void A_list_cut_and_taken_back_lets_a_guard_in_and_out_again()
	{
		var parser  = Counted(Starved, direct: false);
		var visited = Visited(parser);
		var failed  = parser.GetNestedType("Failure", BindingFlags.NonPublic)!;
		var tie     = parser.GetMethod("Tie_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!;
		var mark    = (string[])parser.GetField("Truncated_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var guard   = new[] { "\u0002false" };
		var sets    = new List<string[]>();
		var failure = Activator.CreateInstance(failed)!;

		failed.GetField("ExpectedMore")!.SetValue(failure, sets);

		void Tie(string[] expected)
		{
			object[] arguments = [failure, expected];

			tie.Invoke(null, arguments);
			failure = arguments[0];
		}

		for (var one = 0; one < TieShapes.Cap; one++)
			sets.Add(["'x'"]);

		visited.SetValue(null, 0L);

		// At the cap: one search, no guard found, the mark appended.
		Tie(["'y'"]);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal((long)TieShapes.Cap, (long)visited.GetValue(null)!);

		// Past it: dropped at once, a guard's set as well.
		for (var one = 0; one < 1000; one++)
			Tie(["'z'"]);

		Tie(guard);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal((long)TieShapes.Cap, (long)visited.GetValue(null)!);

		// A tail taken back, the mark with it: the list is below the cap, and a guard enters once.
		sets.RemoveRange(TieShapes.Cap - 1, 2);
		Tie(guard);
		Tie(guard);
		Assert.Equal(TieShapes.Cap, sets.Count);
		Assert.Same(guard, sets[^1]);

		// At the cap a set that names something takes the guard's place: one search.
		Tie(["'w'"]);
		Assert.Equal(TieShapes.Cap, sets.Count);
		Assert.DoesNotContain(sets, static one => one.Length == 1 && one[0][0] == '\u0002');
		Assert.Equal(new[] { "'w'" }, sets[^1]);
		Assert.Equal(2L * TieShapes.Cap, (long)visited.GetValue(null)!);

		// The next finds no guard and appends the mark again: one more search, and none after it.
		Tie(["'v'"]);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);

		for (var one = 0; one < 1000; one++)
			Tie(["'u'"]);

		Tie(guard);
		Assert.Equal(TieShapes.Cap + 1, sets.Count);
		Assert.Same(mark, sets[^1]);
		Assert.Equal(3L * TieShapes.Cap, (long)visited.GetValue(null)!);
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
