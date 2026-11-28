using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Plan;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What each place of a reader records where it refuses, as the planning stage says it: every place
/// the readers print has its answer in the plan, a settled copy's places are its rule's, and a place
/// the plan has no answer for, or two answers, throws.
/// </summary>
public class RecordSiteTests
{
	/// <summary>
	/// The readers of every snapshot, in every rendering, ask the plan what each place records, and the
	/// plan has an answer for each: a place it did not answer throws while printing, which no snapshot
	/// does. Held here by the count of places of each kind.
	/// </summary>
	[Fact]
	public void The_plan_says_what_every_place_of_the_snapshot_readers_records()
	{
		var kinds = new SortedDictionary<string, int>(StringComparer.Ordinal);

		// Every place is said again where it is printed, and held to the plan's: no answer depends on what the key does not hold.
		RecordSites.CheckedWhilePrinting = true;

		try
		{
			foreach (var plan in Plans())
				foreach (var machine in plan.Machines)
				{
					Assert.Equal(machine.Strategy == ReadingStrategy.Reader, machine.Sites is not null);
					Assert.Equal(machine.Strategy == ReadingStrategy.Reader, machine.ReadInPlace is not null);

					if (machine.Sites is { } sites)
						foreach (var (_, _, _, site) in sites.All())
							kinds[site.GetType().Name] = kinds.TryGetValue(site.GetType().Name, out var count) ? count + 1 : 1;
				}
		}
		finally
		{
			RecordSites.CheckedWhilePrinting = false;
		}

		Assert.Equal(ExpectedKinds, string.Join(", ", kinds.Select(static one => $"{one.Key} {one.Value}")));
	}

	/// <summary>
	/// Every grammar of the repository read over characters, compiled for the reader: printed without a
	/// place the plan did not answer, and with no place answered twice otherwise.
	/// </summary>
	[Fact]
	public void Every_reader_of_the_repository_finds_its_places_in_the_plan()
	{
		var faults = new System.Collections.Concurrent.ConcurrentBag<string>();
		var places = 0;

		System.Threading.Tasks.Parallel.ForEach(
			ExponentialRepetitionTests.Grammars(SourceRoot.Directory).Where(static one => one.Modes.Contains(false) && one.Included.Count == 0),
			grammar =>
			{
				// And each place said again where it is printed, held to the plan's (RecordSites.CheckedWhilePrinting).
				RecordSites.CheckedWhilePrinting = true;

				try
				{
					var result = GramCompiler.Compile(grammar.Text, Options(settle: true));

					// A generator that throws is told as a diagnostic.
					foreach (var told in result.Diagnostics)
						if (told.Message.Contains("place of", StringComparison.Ordinal))
							faults.Add($"{grammar.Name}: {told.Message}");

					foreach (var machine in result.Plan?.Machines ?? [])
						if (machine.Sites is { } sites)
							System.Threading.Interlocked.Add(ref places, sites.Count);
				}
				catch (InvalidOperationException error) when (error.Message.Contains("place of", StringComparison.Ordinal))
				{
					faults.Add($"{grammar.Name}: {error.Message}");
				}
				finally
				{
					RecordSites.CheckedWhilePrinting = false;
				}
			});

		TestContext.Current.SendDiagnosticMessage($"{places} places");

		Assert.True(places > 1000, $"only {places} places");
		Assert.True(faults.IsEmpty, string.Join("\n", faults.Order(StringComparer.Ordinal)));
	}

	/// <summary>
	/// A grammar whose run-ending rule gets a settled copy: the plan's places are keyed by the rule the
	/// copy was made from and by its nodes, so that no place names the copy or a node of it, and the
	/// copy's places are the rule's own places.
	/// </summary>
	[Fact]
	public void A_settled_copy_and_its_rule_share_their_places()
	{
		var result = GramCompiler.Compile(Settling, Options(settle: true));
		var graph  = result.Graph!;

		Assert.NotEmpty(graph.Settled);

		var copies = graph.Settled.Keys.ToHashSet();
		var copied = NodeWalk.ByIdentity(copies.SelectMany(copy => NodeWalk.Descendants(graph.Bodies[copy])));
		var sites  = result.Plan!.Machines.Single(static one => one.Sites is not null).Sites!.All().ToList();

		Assert.DoesNotContain(sites, site => copies.Contains(site.Owner));
		Assert.DoesNotContain(sites, site => copied.Contains(site.Node));

		// The places the copies stand at are the rule's: the same keys without the copies.
		var plain = GramCompiler.Compile(Settling, Options(settle: false));

		Assert.Equal(Keys(plain.Plan!.Machines.Single(static one => one.Sites is not null).Sites!.All(), plain.Graph!), Keys(sites, graph));
	}

	/// <summary>
	/// A count of a text owes its turns below the fewest: where its door shuts there, what a turn begins with is said
	/// by the door itself, and the refusal where the count's bounds cross is a place of its own. Both used to be one
	/// place said two ways, and the grammar did not compile.
	/// </summary>
	[Theory]
	[InlineData("R = $q: 'a'* & '-' & \"xy\"{#q}\nparse R")]
	[InlineData("R = $q: 'a'* & '-' & $r: 'c'* & '-' & \"xy\"{#q,#r}\nparse R")]
	public void A_count_says_its_owed_turn_at_its_door(string grammar)
	{
		var result = GramCompiler.Compile(grammar, Options(settle: true));

		Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics));

		var sites = result.Plan!.Machines.Single(static one => one.Sites is not null).Sites!.All().Select(static one => one.Site).ToList();

		Assert.Contains(sites, static site => site is DoorSite { Items: ["\"xy\""] });
	}

	/// <summary>A place the plan does not hold throws when it is asked for, and one place is never answered two ways.</summary>
	[Fact]
	public void A_place_the_plan_does_not_hold_throws()
	{
		var plan  = GramCompiler.Compile("A = '(' & A & ')' | 'x'\nparse A", Options(settle: true)).Plan!;
		var held  = plan.Machines.First(static one => one.Sites is not null).Sites!;
		var rule  = new RuleSymbol("A", new GrammarNamespace("", null, isRoot: true), null);
		var node  = new Node.Empty();
		var other = new RecordSites();

		Assert.NotEqual(0, held.Count);
		Assert.Throws<InvalidOperationException>(() => held.Of<TerminalSite>(rule, node, 0));

		other.Record(rule, node, 0, new TerminalSite { Items = ["'x'"] });
		other.Record(rule, node, 0, new TerminalSite { Items = ["'x'"] });
		other.Record(rule, node, 0, new DoorSite());

		Assert.Throws<InvalidOperationException>(() => other.Record(rule, node, 0, new TerminalSite { Items = ["'y'"] }));
		Assert.Throws<InvalidOperationException>(() => other.Record(rule, node, 0, new TerminalSite { Items = ["'x'"], Noted = true }));
		Assert.Throws<InvalidOperationException>(() => other.Record(rule, node, 0, new DoorSite { QuietOnly = true }));
		Assert.Throws<InvalidOperationException>(() => other.Of<PredictedSite>(rule, node, 0));
		Assert.Equal(["'x'"], other.Of<TerminalSite>(rule, node, 0).Items);
		Assert.Equal(2, other.Count);
	}

	/// <summary>Each place said by the rule it stands in, the child indexes down to its node, its kind and what it records.</summary>
	static List<string> Keys(IEnumerable<(RuleSymbol Owner, Node Node, int Count, RecordSite Site)> sites, RecognitionGraph graph)
	{
		var at = new Dictionary<Node, string>(NodeIdentity.Instance);

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body) && !graph.Settled.ContainsKey(rule))
				Walk(rule.Name, body, "", static node => node);

		// A rule read only by its copies is said where its copies stand: the node of the rule is what the plan keys.
		foreach (var (copy, rule) in graph.Settled)
			Walk(rule.Name, graph.Bodies[copy], "", graph.Original);

		return [.. sites
			.Select(site => $"{site.Owner.Name} {(at.TryGetValue(site.Node, out var path) ? path : "?")} {site.Count}: {site.Site}")
			.Order(StringComparer.Ordinal)];

		void Walk(string rule, Node node, string path, Func<Node, Node> keyed)
		{
			at.TryAdd(keyed(node), $"{rule}{path}");

			var index = 0;

			foreach (var child in node.Children)
				Walk(rule, child, path + "/" + index++, keyed);
		}
	}

	static IEnumerable<ReadingPlan> Plans()
	{
		foreach (var name in SnapshotTests.Snapshots.Select(static row => row.Data).Order(StringComparer.Ordinal))
		{
			var text = System.IO.File.ReadAllText(System.IO.Path.Combine(SnapshotTests.Directory, name + ".gram"));

			foreach (var (options, _) in SnapshotTests.Renderings(name, text))
			{
				var result = GramCompiler.Compile(text, options);

				Assert.NotNull(result.Plan);

				yield return result.Plan;
			}
		}
	}

	static GramCompilerOptions Options(bool settle)
	{
		return new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			SettleRunTails = settle,
		};
	}

	/// <summary>
	/// A name that gives characters back where an attribute's name may follow it, and none before an
	/// attribute's <c>=</c>, where it is read by a settled copy.
	/// </summary>
	const string Settling = """
		trivia = none

		Word  = ['a'..'z'] & ['a'..'z' | '-']*
		Attr  : @string = [' ']* & name: Word & '=' & '"' & [^ '"']* & '"' => @(name)
		Start : @string = '<' & name: Word & Attr* & [' ']* & '>' & eof => @(name)

		parse Start
		""";

	/// <summary>The places of the snapshot readers, by kind.</summary>
	const string ExpectedKinds =
		"DispatchSite 55, DoorSite 79, EndSite 39, NamedSite 3, OverSite 40, PredictedSite 3, RunSite 2, SilentSite 11, TailsSite 2, TerminalSite 330";
}
