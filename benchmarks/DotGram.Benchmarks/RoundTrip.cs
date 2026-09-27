using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Sql;


namespace DotGram.Benchmarks;

/// <summary>
/// Whether the tree says what the text said, checked against ScriptDom rather than against
/// ourselves.
/// </summary>
/// <remarks>
/// <para>
/// A refusal count cannot see the worst kind of defect: the parser reads the text, answers
/// yes, and builds something else. Nothing in this repository could have caught that, because
/// the only thing that knows what the tree should hold is the tree.
/// </para>
/// <para>
/// So a second parser is asked. For each statement:
/// </para>
/// <list type="number">
/// <item>ScriptDom parses the original and prints it — call that <b>A</b>;</item>
/// <item>this grammar parses the original, <see cref="SqlWriter"/> prints it, ScriptDom
/// parses <em>that</em> and prints it — call that <b>B</b>.</item>
/// </list>
/// <para>
/// <b>ScriptDom's generator is the normal form on both sides</b>, which is the whole trick:
/// keyword casing, line breaks, redundant brackets, `INNER` against nothing and every other
/// way of writing the same statement are erased before the comparison. What survives a
/// difference between A and B is a difference in meaning.
/// </para>
/// <para>
/// <b>The number this reports is completeness, not correctness alone.</b> Everything the tree
/// reads and drops — `TOP`, `OVER`, the hints, `OUTPUT`, a named query's `WITH`, the option
/// lists of the DDL — cannot be printed back, so it lands here as a difference. That is the
/// point: this is the first measurement of how much the tree throws away, statement by
/// statement, and the per-kind table is the list of work to make it lossless.
/// </para>
/// </remarks>
static class RoundTrip
{
	public static void Run(string? root, string version, string? wanted = null, int most = 2)
	{
		root ??= Corpus.Checked();

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");

			return;
		}

		if (Kinds.Version(version) is null)
		{
			Console.WriteLine($"No such version as {version}. One of 80 90 100 … 180.");

			return;
		}

		// The cutting, the reading and the comparing are CorpusRoundTrip.cs, which DotGram.Tests.Slow
		// compiles as well: this prints what it counts, CorpusRoundTripTests holds the same counts to a
		// baseline, and there is one implementation of the measurement rather than two to keep in step.
		var shown  = new List<string>();
		var result = CorpusRoundTrip.Run(root, version, trouble => Show(shown, wanted, most, trouble));

		var read = result.ByFile.Values.Sum(static one => one.Read);
		var same = result.ByFile.Values.Sum(static one => one.Same);
		var bad  = result.ByFile.Values.Sum(static one => one.Threw);

		Report(read, same, read - same - bad, bad, result.ByKind, shown);
	}

	// The tally, the generators, the version capping and the whitespace normaliser moved into
	// CorpusRoundTrip.cs when the measurement did: they were what the loop stood on, and leaving copies
	// here would be the two implementations the move was made to avoid.

	static void Report(
		int read, int same, int lost, int bad,
		Dictionary<string, CorpusRoundTrip.Tally> counts, List<string> shown)
	{
		Console.WriteLine();
		Console.WriteLine($"  {read} statements read by both, printed back and put to ScriptDom again");
		Console.WriteLine();
		Console.WriteLine($"  {same,6}  the same statement          {Share(same, read)}");
		Console.WriteLine($"  {lost,6}  read, printed, and different {Share(lost, read)}");
		Console.WriteLine($"  {bad,6}  printed into something ScriptDom will not read {Share(bad, read)}");
		Console.WriteLine();
		Console.WriteLine($"  {"kind",-46}{"count",7}{"same",8}{"share",9}");

		foreach (var (kind, tally) in counts.OrderByDescending(static one => one.Value.All - one.Value.Same)
			.ThenByDescending(static one => one.Value.All))
		{
			if (tally.All == tally.Same)
				continue;

			Console.WriteLine($"  {kind,-46}{tally.All,7}{tally.Same,8}{Share(tally.Same, tally.All),9}");
		}

		var whole = counts.Values.Count(static one => one.All == one.Same);

		Console.WriteLine();
		Console.WriteLine($"  and {whole} kinds that come back whole");

		if (shown.Count == 0)
			return;

		Console.WriteLine();

		foreach (var one in shown)
			Console.WriteLine(one);
	}

	static string Share(int part, int all)
	{
		return all == 0 ? "" : $"{100.0 * part / all,7:0.0}%";
	}

	/// <summary>
	/// A few examples of each kind, so that the list stays readable — or a great many of one
	/// kind, which is what a wave of work on that kind needs.
	/// </summary>
	static void Show(
		List<string> shown, string? wanted, int most, CorpusRoundTrip.Trouble trouble)
	{
		var kind = trouble.Kind;

		if (wanted is not null && !string.Equals(kind, wanted, StringComparison.OrdinalIgnoreCase))
			return;

		var already = shown.Count(one => one.StartsWith("  " + kind + " ", StringComparison.Ordinal));

		if (already >= most)
			return;

		// Where both sides printed, the finding is the place they part; where only one did, the reason
		// is all there is to say.
		var how = trouble.Why;

		if (how is null && trouble.Theirs is { } theirs && trouble.Ours is { } ours)
		{
			var (left, right) = Parted(theirs, ours);

			how = "A: " + left + "\n     B: " + right;
		}

		shown.Add($"  {kind} — {One(trouble.Original)}\n     {how}");
	}

	static string One(string text)
	{
		var line = string.Join(' ', text.Split('\n', '\r').Select(static one => one.Trim())
			.Where(static one => one.Length > 0));

		return line.Length > 140 ? line[..140] + "…" : line;
	}

	/// <summary>The part of two printings where they part, with a little of each side round it.</summary>
	/// <remarks>
	/// Both sides are whole statements and one place in them is the finding: a
	/// <c>CREATE TABLE</c> of three thousand characters parts at its last constraint, and the
	/// first hundred and forty are where it begins rather than where it differs. Twice in one
	/// day the cause was read out of the writer instead of out of this report, which is the
	/// report failing at the one thing it is for. The two are compared as
	/// <see cref="Normalized"/> leaves them, since that is what decided they differ.
	/// </remarks>
	static (string A, string B) Parted(string a, string b)
	{
		const int Round = 60;

		var left  = Normalized(a);
		var right = Normalized(b);
		var at    = 0;

		while (at < left.Length && at < right.Length && left[at] == right[at])
			at++;

		var from = Math.Max(0, at - Round);

		return (Window(left, from), Window(right, from));

		static string Window(string text, int from)
		{
			var part = text.Substring(from, Math.Min(Round * 3, text.Length - from));

			return (from > 0 ? "…" : "") + part + (from + part.Length < text.Length ? "…" : "");
		}
	}

	/// <summary>
	/// Whitespace is not meaning. Both sides come out of the same generator, so the only
	/// difference this erases is the one a line break makes to a string comparison.
	/// </summary>
	static string Normalized(string text)
	{
		return string.Join(' ', text.Split(' ', '\t', '\n', '\r')
			.Where(static one => one.Length > 0));
	}
}
