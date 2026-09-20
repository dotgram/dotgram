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
		var series = RefusalLadders.All().Single(one => RefusalLadders.Id(one) == id);
		var result = RefusalLadders.Run(series);

		Assert.True(result.Thrown is null, $"{id}: the reader threw {result.Thrown} on a refused input instead of refusing it.");
		Assert.False(result.Faulty, $"{id}: the input of this series is ACCEPTED, so it no longer checks a refusal. Change the input, not the baseline.");

		Assert.False(
			result.Hung || result.Class == RefusalLadders.Class.Explosive,
			$"{id}: a refusal is EXPLOSIVE (exponent {result.Exponent:F1}{(result.Hung ? $", a call at {result.Last.N} {series.Unit} had not finished in {RefusalLadders.WatchdogMilliseconds} ms" : "")}). " +
			"That is the shape of `(A+)*`: a run that can be cut many ways, all of which a refusal tries. Make the run atomic. An explosion is never written into the baseline.");

		var measured = result.Class;

		if (!Baseline.Value.TryGetValue(id, out var allowed))
		{
			Assert.Fail($"{id}: not in RefusalBaseline.txt. A new series is written down at the class it measured; add the line `{id} = {measured}`.");

			return;
		}

		Assert.True(
			result.Exponent <= Limit(allowed),
			$"{id}: got WORSE. Baseline {allowed}, measured {measured} (time exponent {result.Exponent:F2}, {result.Length:N0} characters at the largest size). A refusal that grows faster than its head is a defect a caller can send.");

		if (measured < allowed)
			TestContext.Current.SendDiagnosticMessage($"{id}: better than its baseline ({allowed} -> {measured}, exponent {result.Exponent:F2}); tighten the line in RefusalBaseline.txt.");
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

			var name = line[(at + 3)..].Trim();

			Assert.True(Enum.TryParse<RefusalLadders.Class>(name, out var one) && one != RefusalLadders.Class.Explosive, $"RefusalBaseline.txt: `{line}`: `{name}` is not a class a baseline may hold (linear, superlinear, quadratic).");

			baseline[line[..at].Trim()] = one;
		}

		return baseline;
	}
}
