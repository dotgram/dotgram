using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix;
using DotGram.Finance.Generated;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What the generator wrote against what the reader loads, from the one file.
/// </summary>
/// <remarks>
/// <para>
/// The two roads of D71 over the same dictionary: one compiles it at build time into the code in
/// <c>Fix44Rules</c>, the other reads it at run time into a validator. They are given the same
/// file, so any difference between their answers is a defect in one of them and never a difference
/// in the data — which is what makes this the generator's own test rather than another comparison
/// of two readings of FIX 4.4.
/// </para>
/// <para>
/// The generated code itself is never committed. It is written into <c>obj/</c> by a real build of
/// <c>DotGram.Finance.Generated</c>, which names the corpus dictionary as an additional file.
/// </para>
/// </remarks>
public sealed class FixGeneratedRulesTests
{
	static FixValidator Loaded()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		using var file = File.OpenRead(Path.Combine(at!.FullName, "tests", "Corpus", "Fix", "FIX44.xml"));

		var validator = new FixValidator();

		validator.Load(FixDictionary.Load(file));

		return validator;
	}

	[Fact]
	public void The_generated_rules_answer_as_the_loaded_dictionary_does()
	{
		var loaded = Loaded();
		var differ = new List<string>();

		foreach (var data in FixFixtures.Messages())
		{
			var message = FixMessages.Parse((string)data[1]);
			var read    = message.Validate(loaded).Select(f => f.ToString()).ToArray();
			var written = message.Validate(Fix44Rules.Validator).Select(f => f.ToString()).ToArray();

			if (read.SequenceEqual(written))
				continue;

			foreach (var one in read.Except(written))
				differ.Add($"{data[0]} loaded only: {one}");

			foreach (var one in written.Except(read))
				differ.Add($"{data[0]} generated only: {one}");

			if (read.Length != written.Length)
				differ.Add($"{data[0]} counts: loaded {read.Length}, generated {written.Length}");
		}

		Assert.True(differ.Count == 0, differ.Count + " differences ||| " + string.Join(" ||| ", differ.Distinct().Take(8)));
	}

	[Fact]
	public void The_generated_table_holds_a_rule_for_every_type_the_dictionary_describes()
	{
		Assert.Equal(93, Fix44Rules.Entries.Count);
		Assert.True(Fix44Rules.Entries.ContainsKey("D"));

		// And the validator is built from that table, so the two cannot drift apart.
		foreach (var type in Fix44Rules.Entries.Keys)
			Assert.Same(Fix44Rules.Entries[type], Fix44Rules.Validator[type]);
	}

	/// <summary>A message the generated rules accept, and one they do not.</summary>
	[Fact]
	public void The_generated_rules_are_rules_and_not_an_empty_table()
	{
		const string header = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
		const string order  = "11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";

		Assert.Empty(FixFixtures.Message(header + order).Validate(Fix44Rules.Validator));

		Assert.Contains(
			FixFixtures.Message(header + order.Replace("11=ORDER123|", "")).Validate(Fix44Rules.Validator),
			f => f.Rule == FixRule.RequiredFieldMissing && f.Tag == 11);

		Assert.Contains(
			FixFixtures.Message(header + order.Replace("54=1|", "54=Z|")).Validate(Fix44Rules.Validator),
			f => f.Rule == FixRule.InvalidValue && f.Tag == 54);
	}
}
