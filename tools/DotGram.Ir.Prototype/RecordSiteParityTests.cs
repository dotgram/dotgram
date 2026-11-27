using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Plan;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// How far the evaluator's account of a refusal is from the generated C# parser's, per fixture and
/// per input, and what kind of difference is left: the sets the C# parser keeps (<c>First</c> then
/// <c>Tied</c>, in order) against the evaluator's, naive and through the lowering a refusal is read
/// with. Written to a file for reading; held to nothing but that it ran.
/// </summary>
/// <remarks>
/// Run apart from the other classes, as <see cref="RefusalSpikeTests"/> is: the allocation test
/// counts the bytes of the whole process while it measures.
/// </remarks>
[Collection(nameof(RefusalSpikeAlone))]
public sealed class RecordSiteParityTests
{
	/// <summary>How an input's sets differ, decided mechanically; <see cref="None"/> where they do not.</summary>
	enum Residual
	{
		None,

		/// <summary>The same sets (and the same items in each) in another order.</summary>
		Order,

		/// <summary>One side has a set whose items are the union of several the other has.</summary>
		Merge,

		/// <summary>One side names a rule where the other lists its items.</summary>
		Name,

		/// <summary>One side has a set the other lacks altogether.</summary>
		Missing,

		Other,
	}

	/// <summary>The sets a refusal says, each as its items, a guard spelled as the readings spell it.</summary>
	sealed record Sets(List<string[]> Each)
	{
		public override string ToString()
		{
			return string.Join(" ", Each.Select(static set => "[" + string.Join(" | ", set) + "]"));
		}
	}

	/// <summary>The lowering a refusal is read through today; the one place to change it for another.</summary>
	static IrProgram Lowered(string grammar)
	{
		return _lowered.GetOrAdd(grammar, static text => RefusalPlan.AsTheCSharpReader(IrLowering.Lower(Graphs.Of(text))));
	}

	static readonly ConcurrentDictionary<string, IrProgram> _lowered = new();

	static readonly ConcurrentDictionary<(string, string, string), string> _strategies = new();

	static (string[]? First, string[][]? Tied, bool Accepted) Theirs(Type parser, string entry, string input)
	{
		var method = parser.GetMethod("Try" + entry, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
		object? match = null;

		Twin.Record(() => match = method.Invoke(null, [input]));

		var type = match!.GetType();

		return (
			(string[]?)type.GetField("_expected", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match),
			(string[][]?)type.GetField("_tied", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match),
			(bool)type.GetProperty("IsSuccess")!.GetValue(match)!);
	}

	static string Item(string one)
	{
		return one.Length > 0 && one[0] == '\u0002' || one.StartsWith("when ", StringComparison.Ordinal) ? "<guard>" : one;
	}

	/// <summary>What the generated parser kept of a refusal, set by set in its order.</summary>
	static Sets Kept(string[]? first, string[][]? tied)
	{
		var sets = new List<string[]>();

		if (first is not null)
			sets.Add([.. first.Select(Item)]);

		foreach (var one in tied ?? [])
			sets.Add([.. one.Select(Item)]);

		return new Sets(sets);
	}

	/// <summary>What the evaluator said: one set a label, as the comparison of the refusal spike takes it.</summary>
	static Sets Said(IrOutcome outcome)
	{
		return new Sets([.. outcome.Said.Select(static one => new[] { Item(one) })]);
	}

	static List<(string Fixture, string Input)> Refused()
	{
		var cases = new List<(string Fixture, string Input)>();

		foreach (var name in Cases.Fixtures.Keys)
			foreach (var input in Cases.InputsOf(name))
				cases.Add((name, input));

		foreach (var (_, input) in HostTests.JsonCorpus())
			cases.Add(("Json", input));

		return cases;
	}

	/// <summary>
	/// Which machine the generated parser prints for the entry: the generator's plan of the graph,
	/// the machine whose publications include the entry's method.
	/// </summary>
	static string Strategy(Cases.Fixture fixture)
	{
		var carrier = CarrierKind.Auto;

		if (fixture.Parser.GetCustomAttributes(false).FirstOrDefault(static one => one.GetType().Name == "GramAttribute") is { } attribute
			&& attribute.GetType().GetProperty("Carrier")?.GetValue(attribute) is { } asked)
			carrier = Enum.Parse<CarrierKind>(asked.ToString()!);

		return _strategies.GetOrAdd((fixture.Grammar, fixture.Entry, carrier.ToString()), static key =>
		{
			var options = new GramCompilerOptions { Carrier = Enum.Parse<CarrierKind>(key.Item3) };
			var context = new EmitContext { Diagnostics = new List<GramDiagnostic>() };

			CSharpEmitter.Emit(Graphs.Of(key.Item1), options, context);

			var machine = (context.Plan ?? throw new InvalidOperationException("The generator made no plan of the grammar."))
				.Machines.FirstOrDefault(one => one.Publications.Any(publication => publication.MethodName == key.Item2));

			return machine?.Strategy.ToString().ToLowerInvariant() ?? "none";
		});
	}

	static HashSet<string> RuleNames(string grammar)
	{
		return [.. Graphs.Of(grammar).Rules.Select(static rule => rule.Name)];
	}

	static string[] Normal(string[] set)
	{
		return [.. set.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
	}

	static string Key(string[] set)
	{
		return string.Join("\u0001", Normal(set));
	}

	/// <summary>
	/// Why two accounts of one refusal differ, decided from the sets alone. In this order: the same
	/// sets in another order; a set on one side only; one side's set the union of several of the
	/// other's; an item that is a rule's name on the side that lists it where the other lists its
	/// items; anything else.
	/// </summary>
	static Residual Classify(Sets theirs, Sets ours, HashSet<string> rules)
	{
		var a = theirs.Each.Select(Normal).ToList();
		var b = ours.Each.Select(Normal).ToList();

		if (a.Count == b.Count && a.Select(Key).Order(StringComparer.Ordinal).SequenceEqual(b.Select(Key).Order(StringComparer.Ordinal), StringComparer.Ordinal))
			return a.Select(Key).SequenceEqual(b.Select(Key), StringComparer.Ordinal) ? Residual.None : Residual.Order;

		// What is left once the sets both sides have are taken off.
		var onlyA = a.ToList();
		var onlyB = b.ToList();

		foreach (var set in a.ToList())
		{
			var index = onlyB.FindIndex(other => Key(other) == Key(set));

			if (index >= 0)
			{
				onlyB.RemoveAt(index);
				onlyA.RemoveAt(onlyA.FindIndex(other => Key(other) == Key(set)));
			}
		}

		if (onlyA.Count == 0 || onlyB.Count == 0)
			return Residual.Missing;

		var itemsA = onlyA.SelectMany(static set => set).ToHashSet(StringComparer.Ordinal);
		var itemsB = onlyB.SelectMany(static set => set).ToHashSet(StringComparer.Ordinal);

		if (itemsA.SetEquals(itemsB) && onlyA.Count != onlyB.Count)
			return Residual.Merge;

		if (itemsA.Except(itemsB).Concat(itemsB.Except(itemsA)).Any(rules.Contains))
			return Residual.Name;

		return Residual.Other;
	}

	static string Quote(string input)
	{
		return Twin.Quoted(input.Length > 60 ? input[..60] : input);
	}

	[Fact]
	public void WhereTheEvaluatorsAccountOfARefusalIsNotTheGeneratedParsers()
	{
		var report = Environment.GetEnvironmentVariable("IR_REPORT");
		var rows   = new StringBuilder();
		var counts = new SortedDictionary<string, Tally>(StringComparer.Ordinal);
		var rules  = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
		var family = new HashSet<string>(StringComparer.Ordinal);

		foreach (var (name, fixture) in Cases.Fixtures)
			if (GiveBackGrammarTexts.Contains(fixture.Grammar))
				family.Add(name);

		foreach (var (name, input) in Refused())
		{
			var fixture = Cases.Fixtures[name];
			var theirs  = Theirs(fixture.Parser, fixture.Entry, input);

			if (theirs.Accepted)
				continue;

			var (naive, _, _)  = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);
			var (lowered, _, _) = new IrEvaluator(Lowered(fixture.Grammar), new RecordingHost(fixture.Guards)).ParseQuietlyFirst(fixture.Entry, input);

			if (!rules.TryGetValue(fixture.Grammar, out var names))
				rules[fixture.Grammar] = names = RuleNames(fixture.Grammar);

			var kept          = Kept(theirs.First, theirs.Tied);
			var ours          = Said(naive);
			var plans         = Said(lowered);
			var naiveEqual    = Canon(ours) == kept.ToString();
			var loweredEqual  = Canon(plans) == kept.ToString();
			var residual      = loweredEqual ? Residual.None : Classify(kept, plans, names);
			var strategy      = Strategy(fixture);
			var tally         = counts.GetValueOrDefault(name) ?? (counts[name] = new Tally(strategy));

			tally.Refused++;
			tally.NaiveEqual   += naiveEqual ? 1 : 0;
			tally.LoweredEqual += loweredEqual ? 1 : 0;

			if (!loweredEqual)
				tally.Classes[residual] = tally.Classes.GetValueOrDefault(residual) + 1;

			rows.Append(name).Append('\t').Append(strategy).Append('\t').Append(Quote(input))
				.Append('\t').Append(kept)
				.Append('\t').Append(Canon(ours))
				.Append('\t').Append(Canon(plans))
				.Append('\t').Append(naiveEqual ? "=" : "differs")
				.Append('\t').Append(loweredEqual ? "=" : "differs")
				.Append('\t').Append(loweredEqual ? "" : residual.ToString().ToLowerInvariant())
				.Append('\n');
		}

		var classes = Enum.GetValues<Residual>().Where(static one => one != Residual.None).ToList();
		var summary = new StringBuilder("fixture\tstrategy\trefused\tnaive equal\tlowered equal");

		foreach (var one in classes)
			summary.Append('\t').Append(one.ToString().ToLowerInvariant());

		summary.Append('\n');

		foreach (var (name, tally) in counts)
		{
			summary.Append(name).Append('\t').Append(tally.Strategy).Append('\t')
				.Append(tally.Refused).Append('\t').Append(tally.NaiveEqual).Append('\t').Append(tally.LoweredEqual);

			foreach (var one in classes)
				summary.Append('\t').Append(tally.Classes.GetValueOrDefault(one));

			summary.Append('\n');
		}

		var refused = counts.Values.Sum(static one => one.Refused);

		summary.Append(Totals("all", counts.Values, classes))
			.Append(Totals("all but Json", counts.Where(static one => one.Key != "Json").Select(static one => one.Value), classes))
			.Append(Totals("give-back grammars", counts.Where(one => family.Contains(one.Key)).Select(static one => one.Value), classes))
			.Append(string.Create(CultureInfo.InvariantCulture,
				$"give-back family members without inputs, skipped\t{GiveBackFamily.Members.Length}\n"));

		foreach (var strategy in counts.Values.Select(static one => one.Strategy).Distinct().Order(StringComparer.Ordinal))
			summary.Append(Totals("strategy " + strategy, counts.Values.Where(one => one.Strategy == strategy), classes));

		if (report is not null)
		{
			File.WriteAllText(Path.Combine(report, "parity.tsv"),
				"fixture\tstrategy\tinput\tC# sets\tnaive\tlowered\tnaive equal\tlowered equal\tclass\n" + rows);
			File.WriteAllText(Path.Combine(report, "parity-summary.tsv"), summary.ToString());
		}
		else
		{
			TestContext.Current.SendDiagnosticMessage(summary.ToString());
		}

		Assert.True(refused > 300, "refused " + refused);

		// On JSON every refusal is said as the generated parser says it, set for set in its order.
		Assert.Equal((183, 183), (counts["Json"].Refused, counts["Json"].LoweredEqual));
	}

	/// <summary>The grammars of <see cref="GiveBackGrammars"/>, which are the fixtures of the give-back family that have inputs.</summary>
	static readonly HashSet<string> GiveBackGrammarTexts = new(StringComparer.Ordinal)
	{
		GiveBackGrammars.GivesBackInTurn,
		GiveBackGrammars.GivesBackInTurnBuilt,
		GiveBackGrammars.GivesBackPastStop,
		GiveBackGrammars.GivesBackPastStopBuilt,
		GiveBackGrammars.TurnDefers,
		GiveBackGrammars.LastAlternativeGivesBack,
	};

	/// <summary>The spelling the refusal spike compares in: each of the evaluator's labels a set of its own.</summary>
	static string Canon(Sets sets)
	{
		return sets.ToString();
	}

	static string Totals(string label, IEnumerable<Tally> tallies, List<Residual> classes)
	{
		var all  = tallies.ToList();
		var text = new StringBuilder(label).Append("\t\t")
			.Append(all.Sum(static one => one.Refused)).Append('\t')
			.Append(all.Sum(static one => one.NaiveEqual)).Append('\t')
			.Append(all.Sum(static one => one.LoweredEqual));

		foreach (var one in classes)
			text.Append('\t').Append(all.Sum(tally => tally.Classes.GetValueOrDefault(one)));

		return text.Append('\n').ToString();
	}

	sealed class Tally(string strategy)
	{
		public string Strategy { get; } = strategy;

		public int Refused;
		public int NaiveEqual;
		public int LoweredEqual;

		public Dictionary<Residual, int> Classes { get; } = [];
	}
}
