using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// What each rule's value would be carried as, if the parser carried values as typed
/// shapes rather than as records on a tape.
/// </summary>
/// <remarks>
/// <para>
/// The first stage of the redesign in <c>docs/next.md</c>: a report, not an emitter. Before
/// anything is generated differently, this says — for one grammar, and for every grammar in
/// the repository — how many rules could be structs held by value inside their parents, how
/// many have to be classes because they can reach themselves, how large the structs come to,
/// where the folds are, and which rules carry the things the lab did not try: guards,
/// gathered members, climbing.
/// </para>
/// <para>
/// The decision rule is the one <c>benchmarks/DotGram.HandDeferred</c> measured. A value
/// nested by value inside another has a type whose size is known at compile time, and a
/// rule that can reach itself has not: a <c>Sum</c> inside a <c>Pair</c> inside a
/// <c>Sum</c> is a struct that contains itself. Something on the cycle must be a
/// reference, and nothing off it needs to be. Left recursion is not a cycle for this
/// purpose — §4.3 has already turned it into a loop, and after normalization the rule no
/// longer calls itself — so a fold is a run of one element type, which is an array, not a
/// reference.
/// </para>
/// </remarks>
public static class Shapes
{
	/// <summary>How one rule's value would be carried.</summary>
	public enum Carrier
	{
		/// <summary>The rule builds nothing; it recognizes and is not a shape at all.</summary>
		None,

		/// <summary>A struct, held by value wherever it is captured.</summary>
		Struct,

		/// <summary>A class, because the rule can reach itself and a value cannot contain itself.</summary>
		Class,
	}

	/// <summary>One rule, as the redesign would see it.</summary>
	/// <param name="Symbol">The rule.</param>
	/// <param name="Carrier">Struct, class, or nothing.</param>
	/// <param name="Folds">Whether §4.3 turned the rule into a base and a run of steps.</param>
	/// <param name="Guarded">Whether a <c>when</c> under it asks for values before the derivation is accepted.</param>
	/// <param name="Climbs">Whether it takes a precedence level.</param>
	/// <param name="Texts">Members that are a span of text.</param>
	/// <param name="Records">Members that are another rule's value.</param>
	/// <param name="Sequences">Members gathered across a repetition, of either kind.</param>
	/// <param name="Bytes">
	/// What the shape's fields come to: eight for a span or a reference, the whole of a
	/// struct member nested by value. An estimate of what a copy costs, not a layout.
	/// </param>
	public sealed record Rule(
		RuleSymbol Symbol, Carrier Carrier, bool Folds, bool Guarded, bool Climbs,
		int Texts, int Records, int Sequences, int Bytes);

	/// <summary>Rules that can reach one another, and so cannot all be values.</summary>
	public sealed record Cycle(IReadOnlyList<RuleSymbol> Rules);

	/// <summary>One published rule, and the mode it is read in.</summary>
	/// <param name="Publication">The publication.</param>
	/// <param name="Streams">Whether it is read a window at a time, which a method cannot be.</param>
	public sealed record Entry(Publication Publication, bool Streams);

	/// <summary>The whole answer for one grammar.</summary>
	public sealed record Report(
		IReadOnlyList<Rule> Rules, IReadOnlyList<Cycle> Cycles, IReadOnlyList<Entry> Entries, bool Recovers)
	{
		public int Structs => Rules.Count(one => one.Carrier == Carrier.Struct);

		public int Classes => Rules.Count(one => one.Carrier == Carrier.Class);

		public int Valued => Rules.Count(one => one.Carrier != Carrier.None);

		/// <summary>The rules, one to a line, widest first so the outliers are at the top.</summary>
		public string Table()
		{
			var text = new StringBuilder();

			text.Append($"{"rule",-28} {"carrier",-7} {"bytes",5}  {"texts",5} {"records",7} {"seqs",4}  flags\n");

			foreach (var rule in Rules.Where(one => one.Carrier != Carrier.None).OrderByDescending(one => one.Bytes))
			{
				var flags = string.Join(" ", new[]
				{
					rule.Folds   ? "fold"  : "",
					rule.Guarded ? "guard" : "",
					rule.Climbs  ? "climb" : "",
				}.Where(one => one.Length > 0));

				text.Append(
					$"{rule.Symbol.Name,-28} {rule.Carrier,-7} {rule.Bytes,5}  {rule.Texts,5} {rule.Records,7} {rule.Sequences,4}  {flags}\n");
			}

			return text.ToString();
		}

		/// <summary>The totals, in one line.</summary>
		public string Summary() =>
			$"{Rules.Count} rules: {Valued} valued, {Structs} structs, {Classes} classes in {Cycles.Count} cycle(s); " +
			$"{Rules.Count(one => one.Folds)} folds, {Rules.Count(one => one.Guarded)} guarded, " +
			$"{Rules.Count(one => one.Sequences > 0)} gathering, {Rules.Count(one => one.Climbs)} climbing; " +
			$"{Entries.Count} entries, {Entries.Count(one => one.Streams)} streaming" +
			(Recovers ? ", recovers" : "");
	}

	/// <summary>The report for one graph — the syntactic half, where the grammar was cut in two.</summary>
	/// <param name="graph">The graph as the emitter would see it.</param>
	/// <param name="overKinds">Whether positions are tokens, which is what rules out streaming.</param>
	public static Report Of(RecognitionGraph graph, bool overKinds = false)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var calls  = graph.Calls;
		var valued = new HashSet<RuleSymbol>(
			graph.Rules.Where(one => graph.Results[one].Count > 0 || graph.Types.ContainsKey(one)));

		var sizes = new Dictionary<RuleSymbol, int>();
		var rules = new List<Rule>();

		foreach (var rule in graph.Rules)
		{
			var carrier = !valued.Contains(rule) ? Carrier.None :
				calls.Recurses(rule)             ? Carrier.Class :
				                                   Carrier.Struct;

			var members   = graph.Results[rule];
			var texts     = members.Count(one => !IsRecord(one));
			var records   = members.Count(IsRecord);
			var sequences = members.Count(one => one.IsSequence);

			rules.Add(new Rule(
				rule, carrier,
				Folds:   graph.Folds.ContainsKey(rule),
				Guarded: NodeWalk.Descendants(graph.Bodies[rule]).Any(one => one is Node.Guard),
				Climbs:  graph.Climbing.ContainsKey(rule),
				texts, records, sequences,
				Bytes:   carrier == Carrier.None ? 0 : SizeOf(rule)));
		}

		var cycles = calls.Components
			.Where(one => one.Count > 1 || (one.Count == 1 && calls.Recurses(one[0])))
			.Select(one => new Cycle(one))
			.ToList();

		var entries = graph.Publications
			.Select(one => new Entry(one, Streams(graph, one, overKinds)))
			.ToList();

		return new Report(rules, cycles, entries, graph.Recoveries.Count > 0);

		bool IsRecord(ResultMember member) => member.Rule is not null && valued.Contains(member.Rule);

		// A span and a reference are both eight bytes; a struct nested by value is as big as
		// it is, all the way down — which terminates because anything on a cycle is a
		// reference, and nothing else can come back to itself.
		int SizeOf(RuleSymbol rule)
		{
			if (sizes.TryGetValue(rule, out var known))
				return known;

			var bytes = 0;

			foreach (var member in graph.Results[rule])
				bytes += member.IsSequence                                        ? 8 :
				         !IsRecord(member)                                        ? 8 :
				         calls.Recurses(member.Rule!) || !valued.Contains(member.Rule!) ? 8 :
				                                                                    SizeOf(member.Rule!);

			return sizes[rule] = bytes;
		}
	}

	/// <summary>
	/// Whether a publication is read a window at a time. The same question the streaming
	/// emitter asks, asked here of the model so the report can say which entries a method
	/// cannot be.
	/// </summary>
	static bool Streams(RecognitionGraph graph, Publication publication, bool overKinds) =>
		!overKinds &&
		(publication.Kind == PublishKind.Find
			? Retention.Reads(graph, publication.Rule) is null &&
				Retention.ExtentOf(graph).TryGetValue(publication.Rule, out var extent) &&
				extent != LineExtent.Beyond
			: Retention.StreamedParse(graph, publication.Rule) is null);
}
