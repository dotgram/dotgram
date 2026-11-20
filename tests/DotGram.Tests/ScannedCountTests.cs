using System;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The characters a run of a character class steps over, counted under <c>DOTGRAM_COUNTS</c> beside
/// the rule entries, so that a reading which scans one run again and again is held to a count and
/// not to a clock.
/// </summary>
/// <remarks>
/// An entry counter cannot see this cost: a run scanned again from every place inside itself is one
/// entry a scan, and the scan is as long as what is left of the run. Each character a run's loop
/// steps over is counted once each time it is stepped over, in a direct reader by the rule the run
/// is in (<c>CountScanned_&lt;rule&gt;</c>) and in the engine by the machine (<c>Scanned</c>).
/// </remarks>
public sealed class ScannedCountTests
{
	/// <summary>A run nothing after it can want a character of, read once.</summary>
	const string Settled = """
		Start = Word & ('!' | Word & '?')
		Word  = ['a'..'z']+

		parse Start
		""";

	/// <summary>A run that has to give one character back to what follows it.</summary>
	const string GivesBack = """
		Start = ['a'..'z']* & 'a' & '!'

		parse Start
		""";

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_run_read_once_counts_each_character_once(bool direct)
	{
		var assembly = Counted(Settled, direct);

		foreach (var length in new[] { 1, 10, 100 })
			Assert.Equal(length, Scanned(assembly, new string('a', length) + "!"));
	}

	/// <summary>
	/// The run is read whole and refused at the end, then given back a character. A direct reader
	/// reads it again from the tape, which steps over one character before it takes the end the
	/// tape kept; the engine gives back without reading the run again.
	/// </summary>
	[Theory]
	[InlineData(true,  1)]
	[InlineData(false, 0)]
	public void A_run_that_gives_back_counts_what_its_readings_step_over(bool direct, int again)
	{
		var assembly = Counted(GivesBack, direct);

		foreach (var length in new[] { 10, 100 })
			Assert.Equal(length + again, Scanned(assembly, new string('a', length) + "!"));
	}

	[Fact]
	public void Nothing_is_counted_without_the_symbol()
	{
		foreach (var direct in new[] { true, false })
			Assert.DoesNotContain("Scanned", Source(Settled, direct, counts: false), StringComparison.Ordinal);
	}

	/// <summary>The characters one reading stepped over, from a fresh zero.</summary>
	static long Scanned(Assembly assembly, string input)
	{
		var counters = new[] { assembly.GetType("Grammar")! }
			.Concat(assembly.GetType("Grammar")!.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountScanned_", StringComparison.Ordinal) ||
				field.Name.StartsWith("Scanned", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	/// <summary>The grammar compiled with the counters written and turned on.</summary>
	static Assembly Counted(string grammar, bool direct)
	{
		var source = Source(grammar, direct, counts: true);

		Assert.Contains("#if DOTGRAM_COUNTS", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, "Grammar", symbols: ["DOTGRAM_COUNTS"]);
	}

	static string Source(string grammar, bool direct, bool counts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
			CountRules    = counts,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}
}
