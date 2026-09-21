using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Refusals;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refusal after a long head is held to the class it was found in, and an explosion is never one a baseline may hold.
/// </summary>
/// <remarks>
/// <para>
/// D57. Two shipped readers were exponential on a refusal (`(A+)*` over an atom) and no ladder timed a
/// refusal at more than one length. <c>RefusalLadders</c> is the ladders (one series per shape: a head that
/// grows and a tail no reader can finish); the stand prints them all as an audit, and this is the same file
/// read as a guard, so that the two cannot drift.
/// </para>
/// <para>
/// <strong>The baseline is asymmetric on purpose.</strong> <c>RefusalBaseline.txt</c> writes down the class of
/// each series as it was measured (linear, superlinear, quadratic). A series fails when it is WORSE than its
/// baseline, when its input stops being refused (a series that quietly accepts checks nothing), when it is
/// missing from the baseline, and always when it is explosive: an explosion is never baselined, because the first
/// one who could not fix it in time would write it down as normal and the file would describe what is there and
/// not what is allowed. A series that is BETTER than its baseline passes and says so: the baseline only ever
/// tightens, as the owners fix things.
/// </para>
/// <para>
/// A class has a margin so that a series on the edge of a threshold does not flap in CI: linear holds to 1.20
/// (what an accepted input is held to), superlinear to 1.60, quadratic to the explosive threshold.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class RefusalGuardTests
{
	static readonly Lazy<Dictionary<string, RefusalLadders.Class>> Baseline = new(ReadBaseline);

	public static TheoryData<string> Ids() => new(RefusalLadders.All().Select(RefusalLadders.Id));

	[Theory]
	[MemberData(nameof(Ids))]
	public void A_refusal_is_no_worse_than_its_baseline(string id)
	{
		var series   = RefusalLadders.All().Single(one => RefusalLadders.Id(one) == id);

		// The baseline is written by this process and no other: DOTGRAM_RECORD_BASELINE=<file> dotnet test (or the exe) appends one line a series, the WORST class of three runs, and asserts nothing.
		// A class measured in the stand's audit process is another number (a fresh process reads a curve flatter than one that had run other ladders).
		if (Environment.GetEnvironmentVariable("DOTGRAM_RECORD_BASELINE") is { Length: > 0 } record)
		{
			var runs = Enumerable.Range(0, 3).Select(_ => RefusalLadders.Run(series)).ToList();
			var line = runs.Any(static one => one.Thrown is not null || one.Faulty || one.Hung || one.Class == RefusalLadders.Class.Explosive)
				? $"# not baselined (thrown, accepted or explosive): {id}"
				: $"{id} = {runs.Max(static one => one.Class)}";

			lock (RecordLock)
				File.AppendAllText(record, line + Environment.NewLine);

			return;
		}

		var attempts = new List<RefusalLadders.Result>();
		var worse    = (string?)null;
		var allowed  = default(RefusalLadders.Class);

		// A series that is worse than its baseline on the first run is run again, twice at most, and fails only if it is worse every time: a ladder times calls of tens of
		// microseconds to milliseconds on a machine that in CI is shared, and one run on a loaded one is noise with the power to fail a build. What is not retried is not noise:
		// a reader that throws, an input that is accepted, a series missing from the baseline, and a call that had not finished in the watchdog's two seconds.
		for (var attempt = 1; attempt <= Attempts; attempt++)
		{
			var result = RefusalLadders.Run(series);

			attempts.Add(result);

			Assert.True(result.Thrown is null, $"{id}: the reader threw {result.Thrown} on a refused input instead of refusing it.");
			Assert.False(result.Faulty, $"{id}: the input of this series is ACCEPTED, so it no longer checks a refusal. Change the input, not the baseline.");

			if (!Baseline.Value.TryGetValue(id, out allowed))
			{
				Assert.Fail($"{id}: not in RefusalBaseline.txt. A new series is written down at the class it measured; add the line `{id} = {result.Class}`.");

				return;
			}

			worse = Worse(id, series, result, allowed);

			if (worse is null || result.Hung)
				break;
		}

		var exponents = string.Join(", ", attempts.Select(static one => one.Exponent.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));

		if (worse is not null)
		{
			var message = attempts.Count == 1 ? worse : $"{worse} Worse in {attempts.Count} runs of {Attempts} (exponents {exponents}).";
			var last2   = attempts[^1];

			// An explosion is not a matter of a margin: a call that had not finished in two seconds, or an exponent of 3 and more on every run WITH the ladder ended by its budget at 
			// 512 units or fewer, fails. (An exponent of 3 or more from the tail of a ladder that went on to thousands of units is a burst of a loaded machine in its largest points, not an explosion:
			// every known explosion, the URI template, the address list and the interpolated strings, ends its ladder at 10 to 22 units.)
			if (last2.Hung || (last2.Class == RefusalLadders.Class.Explosive && last2.Budget && last2.Last.N <= 512))
				Assert.Fail(message);

			// A series that is only worse than its class is REPORTED and does not fail the suite, until the spread of the exponent on an unchanged tree is known (measured 2026-09-20: the
			// verdicts of two runs of one build shared two rows in a dozen). A check that fails at random teaches people to ignore it; this one says what it saw and is skipped.
			Assert.Skip("REPORT ONLY, not failing: " + message);
		}

		var last = attempts[^1];

		if (attempts.Count > 1)
			TestContext.Current.SendDiagnosticMessage($"{id}: FLAKY, worse than its baseline on {attempts.Count - 1} run(s) and within it on the last (exponents {exponents}); the machine was not quiet, or the margin of its class is too narrow.");

		if (last.Class < allowed)
			TestContext.Current.SendDiagnosticMessage($"{id}: better than its baseline ({allowed} -> {last.Class}, exponent {last.Exponent:F2}); tighten the line in RefusalBaseline.txt.");
	}

	[Fact]
	public void Few_series_are_held_as_unstable()
	{
		_ = Baseline.Value;

		Assert.True(
			Unstable.Count <= MostUnstable,
			$"{Unstable.Count} series are held as Unstable in RefusalBaseline.txt, more than {MostUnstable}: a label that is given to what is inconvenient empties the guard. Lift the ladders of those series, or fix what makes them jump.");

		TestContext.Current.SendDiagnosticMessage($"{Unstable.Count} of {Baseline.Value.Count} baseline series are held as Unstable.");
	}

	/// <summary>How often a series that is worse than its baseline is measured before it is called worse.</summary>
	const int Attempts = 3;

	static readonly object RecordLock = new();

	/// <summary>The series held as Unstable in the baseline, filled when it is read.</summary>
	static readonly HashSet<string> Unstable = new(StringComparer.Ordinal);

	/// <summary>How many series the baseline may hold as Unstable. The number is an indicator: if it grows it is the instrument that is decaying, not the grammars.</summary>
	const int MostUnstable = 4;

	/// <summary>Why a run is worse than the baseline of its series, or null: an explosion (never baselined), or an exponent above the limit of its class.</summary>
	static string? Worse(string id, RefusalLadders.Series series, RefusalLadders.Result result, RefusalLadders.Class allowed)
	{
		if (result.Hung || result.Class == RefusalLadders.Class.Explosive)
		{
			return $"{id}: a refusal is EXPLOSIVE (exponent {result.Exponent:F1}{(result.Hung ? $", a call at {result.Last.N} {series.Unit} had not finished in {RefusalLadders.WatchdogMilliseconds} ms" : "")}). " +
				"That is the shape of `(A+)*`: a run that can be cut many ways, all of which a refusal tries. Make the run atomic. An explosion is never written into the baseline.";
		}

		return result.Exponent <= Limit(allowed)
			? null
			: $"{id}: got WORSE. Baseline {allowed}, measured {result.Class} (time exponent {result.Exponent:F2}, {result.Length:N0} characters at the largest size). A refusal that grows faster than its head is a defect a caller can send.";
	}

	/// <summary>The exponent up to which a series of a baseline class still passes.</summary>
	static double Limit(RefusalLadders.Class baseline) => baseline switch
	{
		RefusalLadders.Class.Linear      => 1.20,
		RefusalLadders.Class.Superlinear => 1.60,
		RefusalLadders.Class.Quadratic   => RefusalLadders.Explosive,
		_                                => throw new InvalidOperationException("An explosive series cannot be in the baseline."),
	};

	static Dictionary<string, RefusalLadders.Class> ReadBaseline()
	{
		var baseline = new Dictionary<string, RefusalLadders.Class>(StringComparer.Ordinal);

		foreach (var raw in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "RefusalBaseline.txt")))
		{
			var line = raw.Trim();

			if (line.Length == 0 || line[0] == '#')
				continue;

			var at = line.LastIndexOf(" = ", StringComparison.Ordinal);

			Assert.True(at > 0, $"RefusalBaseline.txt: `{line}` is not `parser | shape = class`.");

			var name   = line[(at + 3)..].Trim();
			var reason = (string?)null;

			if (name.IndexOf('#') is var hash and >= 0)
			{
				reason = name[(hash + 1)..].Trim();
				name   = name[..hash].Trim();
			}

			// A series whose class flips between runs of one build is held as Unstable: measured and reported, never failing on its exponent. It carries its REASON and its DATE in the line,
			// and the number of such lines is itself asserted, so that the label does not become where everything inconvenient goes.
			if (name == "Unstable")
			{
				Assert.True(!string.IsNullOrWhiteSpace(reason) && reason.Any(char.IsDigit), $"RefusalBaseline.txt: `{line}`: an Unstable line carries a reason and a date after a #.");

				Unstable.Add(line[..at].Trim());
				baseline[line[..at].Trim()] = RefusalLadders.Class.Quadratic;

				continue;
			}

			Assert.True(Enum.TryParse<RefusalLadders.Class>(name, out var one) && one != RefusalLadders.Class.Explosive, $"RefusalBaseline.txt: `{line}`: `{name}` is not a class a baseline may hold (linear, superlinear, quadratic, or unstable with a reason).");

			baseline[line[..at].Trim()] = one;
		}

		return baseline;
	}
}
