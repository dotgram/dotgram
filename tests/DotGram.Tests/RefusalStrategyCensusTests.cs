using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a refusal can say at each place of a grammar, counted by how the place is printed. A refusal's
/// expected set is recorded where the parser was printed to record it: the structured reader lists items
/// terminal by terminal, while the jump machine and the flat one say a rule's name where a run is scanned
/// and one merged class where a choice is predicted by its first character. This compiles every grammar
/// of the repository read over characters twice, as it is built and as the engine where the reader would
/// be, and holds each place to what the reader said in the first and the engine or the flat machine in
/// the second. A baseline, not a rule: nothing asserts how many
/// places differ.
/// </summary>
/// <remarks>
/// With <c>CENSUS_REPORT</c> set to a directory, the places and the totals are written there as
/// <c>strategy-census.tsv</c> and <c>strategy-census-summary.tsv</c>; otherwise the totals are told.
/// </remarks>
public sealed class RefusalStrategyCensusTests
{
	const string Predicted = "(predicted)";

	/// <summary>The displays a place said, each distinct array once, by how the place was printed.</summary>
	sealed class Places
	{
		/// <summary>By the reader.</summary>
		public readonly Dictionary<string, Dictionary<string, string[]>> Reader = new(StringComparer.Ordinal);

		/// <summary>By the jump machine, the flat one or a search.</summary>
		public readonly Dictionary<string, Dictionary<string, string[]>> Other = new(StringComparer.Ordinal);

		/// <summary>Displays told for a node that stands in no rule's body: made while printing.</summary>
		public int Unplaced;

		/// <summary>The names of the rules, as a display says one.</summary>
		public readonly HashSet<string> Rules = new(StringComparer.Ordinal);

		/// <summary>How many machines the plan gave each strategy.</summary>
		public string Machines = "";
	}

	sealed record Row(string Grammar, string Place, string Reader, string Engine, string Kind);

	[Fact]
	public void What_a_refusal_can_say_at_each_place_by_how_the_place_is_printed()
	{
		var corpus  = Named(ExponentialRepetitionTests.Grammars(SourceRoot.Directory).Where(static grammar => grammar.Modes.Contains(false)));
		var rows    = new ConcurrentBag<Row>();
		var summary = new ConcurrentBag<string[]>();
		var skipped = new ConcurrentBag<string>();
		var readers = 0;
		var others  = 0;

		System.Threading.Tasks.Parallel.ForEach(
			corpus,
			new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 4 },
			grammar =>
			{
				Places? asBuilt, engine;

				try
				{
					asBuilt = Compile(grammar.Item, direct: true);
					engine  = Compile(grammar.Item, direct: false);
				}
				catch (Exception error)
				{
					skipped.Add($"{grammar.Name}: threw {error.GetType().Name}");

					return;
				}

				if (asBuilt is null || engine is null)
				{
					skipped.Add($"{grammar.Name}: needs its host");

					return;
				}

				System.Threading.Interlocked.Add(ref readers, asBuilt.Reader.Count);
				System.Threading.Interlocked.Add(ref others, engine.Other.Count);

				// What a reader said is read from the build as it is; what the engine and the flat machine said, from
				// the build that prints the engine where the reader would be: a machine the plan gives to a reader is
				// asked its displays as an engine's too, so the build as it is has no places of its own for the others.
				// Counted: compared, equal, differ, name, merge, only the others said it, only the reader did, of the
				// first: naming a rule, predicted; of the second: predicted; places the others told in the build as it
				// is, and in the engine build.
				var counts = new int[12];

				counts[10] = asBuilt.Other.Count;
				counts[11] = engine.Other.Count;

				foreach (var place in asBuilt.Reader.Keys.Concat(engine.Other.Keys).Distinct(StringComparer.Ordinal))
				{
					var reader = Arrays(place, asBuilt.Reader);
					var other  = Arrays(place, engine.Other);

					if (reader.Count == 0)
					{
						var names     = other.SelectMany(static array => array).Any(asBuilt.Rules.Contains);
						var predicted = place.EndsWith(" (predicted)", StringComparison.Ordinal);

						counts[5]++;
						counts[7] += names ? 1 : 0;
						counts[8] += predicted ? 1 : 0;

						rows.Add(new Row(grammar.Name, place, "", Said(other), "engine only" + (names ? " (names a rule)" : "") + (predicted ? " (predicted)" : "")));

						continue;
					}

					if (other.Count == 0)
					{
						counts[6]++;
						counts[9] += place.EndsWith(" (predicted)", StringComparison.Ordinal) ? 1 : 0;

						rows.Add(new Row(grammar.Name, place, Said(reader), "", "reader only" + (place.EndsWith(" (predicted)", StringComparison.Ordinal) ? " (predicted)" : "")));

						continue;
					}

					var kind = Classify(reader, other, asBuilt.Rules);

					counts[0]++;

					if (kind == "equal")
					{
						counts[1]++;

						continue;
					}

					counts[2]++;

					if (kind == "name")
						counts[3]++;
					else if (kind == "merge")
						counts[4]++;

					rows.Add(new Row(grammar.Name, place, Said(reader), Said(other), kind));
				}

				summary.Add(
				[
					grammar.Name, counts[0].ToString(), counts[2].ToString(), counts[3].ToString(), counts[4].ToString(),
					(counts[2] - counts[3] - counts[4]).ToString(), counts[5].ToString(), counts[7].ToString(), counts[8].ToString(),
					counts[6].ToString(), counts[9].ToString(), counts[10].ToString(), counts[11].ToString(),
					(asBuilt.Unplaced + engine.Unplaced).ToString(), asBuilt.Machines + " / " + engine.Machines,
				]);
			});

		var ordered = summary.OrderBy(static one => one[0], StringComparer.Ordinal).ToList();
		var totals  = new string[15];

		totals[0] = $"TOTAL ({ordered.Count} grammars, {skipped.Count} skipped)";

		for (var i = 1; i <= 13; i++)
			totals[i] = ordered.Sum(one => int.Parse(one[i])).ToString();

		totals[14] = "";

		var summaryText = "grammar\tplaces compared\tdiffer\tname\tmerge\tother\tonly engine or flat\tof them naming a rule\tof them predicted\tonly reader\tof them predicted\tplaces the others told in the build as it is (not compared)\tplaces the others told in the engine build\tunplaced\tmachines as built / engine build\n" +
			string.Concat(ordered.Select(static one => string.Join("\t", one) + "\n")) + string.Join("\t", totals) + "\n";
		var rowsText    = "grammar\tplace\treader displays\tengine displays\tclass\n" +
			string.Concat(
				rows.OrderBy(static one => one.Grammar, StringComparer.Ordinal).ThenBy(static one => one.Place, StringComparer.Ordinal)
					.Select(static one => $"{one.Grammar}\t{one.Place}\t{one.Reader}\t{one.Engine}\t{one.Kind}\n"));

		if (Environment.GetEnvironmentVariable("CENSUS_REPORT") is { Length: > 0 } directory)
		{
			Directory.CreateDirectory(directory);

			File.WriteAllText(Path.Combine(directory, "strategy-census.tsv"), rowsText, new UTF8Encoding(false));
			File.WriteAllText(Path.Combine(directory, "strategy-census-summary.tsv"), summaryText, new UTF8Encoding(false));
			File.WriteAllText(Path.Combine(directory, "strategy-census-skipped.txt"), string.Concat(skipped.Order(StringComparer.Ordinal).Select(static one => one + "\n")), new UTF8Encoding(false));
		}
		else
			TestContext.Current.SendDiagnosticMessage(summaryText);

		Assert.True(ordered.Count >= 20, $"only {ordered.Count} grammars were compared, {skipped.Count} skipped");
		Assert.True(readers > 0 && others > 0, $"a side told nothing: {readers} places of readers, {others} of the others");
	}

	/// <summary>The corpus with a name each: grammars of one file name, found in several places, are told apart.</summary>
	static List<(ExponentialRepetitionTests.CorpusGrammar Item, string Name)> Named(IEnumerable<ExponentialRepetitionTests.CorpusGrammar> grammars)
	{
		var seen   = new Dictionary<string, int>(StringComparer.Ordinal);
		var result = new List<(ExponentialRepetitionTests.CorpusGrammar, string)>();

		foreach (var grammar in grammars)
		{
			seen.TryGetValue(grammar.Name, out var count);

			seen[grammar.Name] = count + 1;

			result.Add((grammar, count == 0 ? grammar.Name : $"{grammar.Name}#{count + 1}"));
		}

		return result;
	}

	/// <summary>
	/// What was said at every place of the grammar compiled one way, told apart by who printed the place; null
	/// where it does not compile without its host. Includes are joined as the generator joins them.
	/// </summary>
	static Places? Compile(ExponentialRepetitionTests.CorpusGrammar grammar, bool direct)
	{
		var options = new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			Direct         = direct,
			SettleRunTails = false,
		};
		var text    = grammar.Text;

		if (grammar.Included.Count > 0)
		{
			var (joined, map) = GrammarSplice.Join(
				new GrammarSplice.Part(text, null, null),
				[.. grammar.Included.Select(static one => new GrammarSplice.Part(one.Text, one.Name, null))]);

			options.Own  = text.Length;
			options.Cuts = GrammarSplice.Cuts(joined, map.Segments.Skip(1).Select(static one => (one.Start, one.Length)));
			text         = joined;
		}

		var reader = new List<(Node Node, IReadOnlyList<string> Said)>();
		var other  = new List<(Node Node, IReadOnlyList<string> Said)>();

		DotGram.Grammar.Emit.Machine.DisplaysTold          = (node, said) => reader.Add((node, said));
		DotGram.Grammar.Emit.Machine.NonReaderDisplaysTold = (node, said) => other.Add((node, said));

		GramCompilation result;

		try
		{
			result = GramCompiler.Compile(text, options);
		}
		finally
		{
			DotGram.Grammar.Emit.Machine.DisplaysTold          = null;
			DotGram.Grammar.Emit.Machine.NonReaderDisplaysTold = null;
		}

		if (result.HasErrors || result.Graph is not { } graph)
			return null;

		var at     = Paths(graph);
		var places = new Places();

		foreach (var rule in graph.Rules)
		{
			places.Rules.Add(rule.Name);
			places.Rules.Add(Qualified(rule));
		}

		if (result.Plan is { } plan)
			places.Machines = string.Join(" ", plan.Machines.GroupBy(static one => one.Strategy).OrderBy(static one => one.Key).Select(static one => $"{one.Key}:{one.Count()}"));

		Add(reader, places.Reader);
		Add(other, places.Other);

		return places;

		void Add(List<(Node Node, IReadOnlyList<string> Said)> told, Dictionary<string, Dictionary<string, string[]>> into)
		{
			foreach (var (node, said) in told)
			{
				if (!at.TryGetValue(graph.Original(node), out var place))
				{
					places.Unplaced++;

					continue;
				}

				// A choice said by what it predicts is another question than its first alternative said alone.
				var where = $"{Qualified(graph.Counted(place.Rule))} {place.Path}{(said.Count > 0 && said[0] == Predicted ? " (predicted)" : "")}";

				if (!into.TryGetValue(where, out var arrays))
					into[where] = arrays = new Dictionary<string, string[]>(StringComparer.Ordinal);

				var items = said.Where(static one => one != Predicted).ToArray();

				arrays[string.Join("\u0001", items)] = items;
			}
		}
	}

	/// <summary>The rule each node of a graph's bodies stands in, and where: the child indexes down to it.</summary>
	static Dictionary<Node, (RuleSymbol Rule, string Path)> Paths(RecognitionGraph graph)
	{
		var at = new Dictionary<Node, (RuleSymbol, string)>(NodeIdentity.Instance);

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				Walk(rule, body, "");

		return at;

		void Walk(RuleSymbol rule, Node node, string path)
		{
			at.TryAdd(node, (rule, path));

			var index = 0;

			foreach (var child in node.Children)
				Walk(rule, child, path + "/" + index++);
		}
	}

	static string Qualified(RuleSymbol rule)
	{
		return rule.Namespace.Name is { Length: > 0 } name ? name + "." + rule.Name : rule.Name;
	}

	/// <summary>Every array said at a place, in a fixed order.</summary>
	static List<string[]> Arrays(string place, Dictionary<string, Dictionary<string, string[]>> said)
	{
		return said.TryGetValue(place, out var arrays) ? [.. arrays.OrderBy(static one => one.Key, StringComparer.Ordinal).Select(static one => one.Value)] : [];
	}

	static string Said(List<string[]> arrays)
	{
		// One line a place, whatever a display holds.
		return string.Join(" ; ", arrays.Select(static array => "[" + string.Join(" | ", array) + "]"))
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\r", "\\r", StringComparison.Ordinal)
			.Replace("\n", "\\n", StringComparison.Ordinal)
			.Replace("\t", "\\t", StringComparison.Ordinal);
	}

	/// <summary>
	/// How the two ways of printing differ at a place: <c>equal</c>; <c>name</c> where one lists an item that is
	/// a rule's name and the other does not; <c>merge</c> where both list the same items but cut them into
	/// other arrays; <c>other</c>.
	/// </summary>
	static string Classify(List<string[]> reader, List<string[]> other, HashSet<string> rules)
	{
		var a = reader.Select(static array => string.Join("\u0001", array)).ToHashSet(StringComparer.Ordinal);
		var b = other.Select(static array => string.Join("\u0001", array)).ToHashSet(StringComparer.Ordinal);

		if (a.SetEquals(b))
			return "equal";

		var itemsA = reader.SelectMany(static array => array).ToHashSet(StringComparer.Ordinal);
		var itemsB = other.SelectMany(static array => array).ToHashSet(StringComparer.Ordinal);

		if (itemsA.Except(itemsB).Concat(itemsB.Except(itemsA)).Any(rules.Contains))
			return "name";

		if (itemsA.SetEquals(itemsB))
			return "merge";

		return "other";
	}
}
