using System;
using System.IO;

using BenchmarkDotNet.Attributes;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// What loading and applying a FIX dictionary costs, phase by phase (D143, e2d8204e): reading its
/// text into a <see cref="FixDictionary"/> value, applying it to a context, waiting for every check
/// it describes to compile, and holding a message to it right after applying, before the background
/// compile has necessarily caught up. <see cref="Fix44Context.Default"/> alone, with no dictionary,
/// is the baseline every consumer pays regardless.
/// </summary>
/// <remarks>
/// This class used to compare the shared ADT grammar against an older build of the parser
/// (<c>Previous</c>/<c>Simplified</c>, the older build loaded through <c>DOTGRAM_FIX_BASELINE</c>);
/// that comparison is <see cref="FixGrammarComparisonBenchmarks"/>'s job, and reusing its
/// <see cref="FixGrammarComparisonBenchmarks.Bind"/> here read a class initializer no one asked
/// about a dictionary. Since the FIX API change made a dictionary a value applied in its own step
/// (D143), the interesting initialization cost moved from the grammar's class constructor to that
/// step, and this class follows it: its numbers are not comparable with the historical results filed
/// under its old name.
/// </remarks>
[MemoryDiagnoser]
public class FixInitializationBenchmarks
{
	// A small fragment, one message and a handful of fields -- what a test adds over the standard
	// (Fix44Context.With's own example), and the same literal FixDictionaryTests holds this package's
	// behavior to. Applying the QuickFIX/n copy of FIX44.xml alone, without its errata, is refused
	// (it places a group the model carries no member for); this fragment is small on purpose, but
	// whole, so both Dictionary values apply cleanly and only their size differs.
	const string Venue =
		"""
		<fix>
		  <messages>
		    <message name="Logon" msgtype="A">
		      <field name="EncryptMethod" required="Y" />
		      <field name="HeartBtInt" required="Y" />
		      <field name="ResetSeqNumFlag" required="Y" />
		    </message>
		  </messages>
		  <components>
		    <component name="Instrument">
		      <field name="Symbol" required="Y" />
		    </component>
		  </components>
		  <fields>
		    <field number="40" name="OrdType" type="CHAR">
		      <value enum="1" description="MARKET" />
		      <value enum="2" description="LIMIT" />
		    </field>
		  </fields>
		</fix>
		""";

	/// <summary>A small venue fragment (one message, a handful of fields) against the full FIX44 standard with its QuickFIX/n errata (571 checks, D143's own reading).</summary>
	[Params("Venue", "Full")]
	public string Dictionary { get; set; } = "Full";

	/// <summary>Which step's cumulative cost is measured; each runs every step up to and including its own.</summary>
	[Params("Default", "Read", "Apply", "Prepare", "FirstValidate")]
	public string Phase { get; set; } = "Apply";

	string[] texts = [];
	string wire = "";

	[GlobalSetup]
	public void Setup()
	{
		texts = Dictionary == "Venue"
			? [Venue]
			: [File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "FIX44.xml")), File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "quickfixn-fix44-errata.xml"))];
		wire = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");

		Run(); // the phase must run once, cleanly, before it is trusted to a measured loop
	}

	[Benchmark]
	public int Run()
	{
		if (Phase == "Default")
			return Validate(Fix44Context.Default);

		var dictionary = texts.Length == 1 ? FixDictionary.Parse(texts[0]) : FixDictionary.Parse(texts[0]).Merge(FixDictionary.Parse(texts[1]));

		if (Phase == "Read")
			return dictionary.Messages.Count;

		var context = Fix44Context.Default.With(dictionary);

		if (Phase == "Apply")
			return 0;

		if (Phase == "Prepare")
		{
			context.Prepare();
			return 0;
		}

		// FirstValidate: no Prepare -- a message that arrives before the background compile has
		// caught up compiles its own check, on this thread, once (D143). A background compile begun
		// by an earlier iteration may still be running when this one starts; that contention is part
		// of what the phase measures, not noise to filter out.
		return Validate(context);
	}

	int Validate(Fix44Context context)
	{
		var message = FixParser.ParseMessage(wire);

		message.Validate(context);

		return message.InvalidFindings?.Count ?? 0;
	}
}
