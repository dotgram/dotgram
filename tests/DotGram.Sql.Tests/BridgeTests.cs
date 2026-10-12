using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The bridge from the old T-SQL tree to the new one, held by its round trip over every statement T-SQL
/// reads and by a baseline per family (docs/design/sql-tsql-bridge.md).
/// </summary>
public sealed class BridgeTests(ITestOutputHelper output)
{
	/// <summary>
	/// Every concrete old record is owned by exactly one family, and every name a family owns is a record.
	/// </summary>
	[Fact]
	public void Every_old_record_belongs_to_one_family()
	{
		Type[] roots   = [typeof(Statement), typeof(Query), typeof(Expression), typeof(TableReference), typeof(Clause), typeof(SetExpression)];
		var    records = typeof(Statement).Assembly.GetTypes()
			.Where(one => one is { IsClass: true, IsAbstract: false } && roots.Any(one.IsSubclassOf))
			.Select(Bridge.NameOf)
			.ToHashSet(StringComparer.Ordinal);

		var owned  = Bridge.Ownership.SelectMany(static family => family.Owns).ToList();
		var faults = new List<string>();

		faults.AddRange(records.Where(one => !owned.Contains(one)).Select(static one => $"{one}: no family owns it"));
		faults.AddRange(owned.GroupBy(static one => one).Where(static one => one.Count() > 1).Select(static one => $"{one.Key}: owned {one.Count()} times"));
		faults.AddRange(owned.Where(one => !records.Contains(one)).Select(static one => $"{one}: owned and not an old record"));

		Assert.Equal([], faults);
	}

	/// <summary>
	/// A name the old tree keeps as text is split into its parts, each with the style its quotes say.
	/// </summary>
	[Theory]
	[InlineData("t",           "t")]
	[InlineData("dbo.t",       "t|t")]
	[InlineData("[dbo].[t]",   "b|b")]
	[InlineData("[a.b].c",     "b|t")]
	[InlineData("[a]]b].c",    "b|t")]
	[InlineData("\"x\".y",     "d|t")]
	[InlineData("a..b",        "t|t|t")]
	[InlineData("#tmp",        "t")]
	[InlineData("s.db.dbo.t",  "t|t|t|t")]
	public void A_name_is_split_into_its_parts(string text, string styles)
	{
		var name = Bridge.Values.Name("test", text);

		Assert.Equal(text, string.Join('.', name.Parts.Select(static one => one.Text)));
		Assert.Equal(styles, string.Join('|', name.Parts.Select(static one => one.Style switch
		{
			Ast.IdentifierStyle.Bracketed => "b",
			Ast.IdentifierStyle.Delimited => "d",
			_                             => "t",
		})));
	}

	/// <summary>
	/// Every statement of the corpus and of <c>TransactSqlTests</c> carried, written and read back, and no
	/// family doing worse than its baseline: <c>converted</c> no lower, <c>refused</c>, <c>unread</c>,
	/// <c>different</c> and <c>shape</c> no higher.
	/// </summary>
	/// <remarks>
	/// <c>DOTGRAM_RECORD_BRIDGE_BASELINE=1</c> rewrites the baselines and docs/design/sql-tsql-bridge.md from
	/// this run instead of comparing; nothing is written otherwise.
	/// </remarks>
	[Fact]
	public void No_family_converts_worse_than_its_baseline()
	{
		var corpus = BridgeOracle.CorpusStatements().ToList();
		var tests  = BridgeOracle.TestStatements().ToList();

		Assert.Equal(BridgeOracle.CorpusCount(), corpus.Count);
		Assert.True(corpus.Count > 6_000, $"{corpus.Count} corpus statements");
		Assert.True(tests.Count > 1_000, $"{tests.Count} statements of TransactSqlTests");

		var fromCorpus = corpus.AsParallel().AsOrdered().Select(static one => BridgeOracle.Run(one.Statement, one.Source)).ToList();
		var fromTests  = tests.AsParallel().AsOrdered().Select(static one => BridgeOracle.Run(one.Statement, one.Source)).ToList();
		var outcomes   = fromCorpus.Concat(fromTests).ToList();

		var families = Bridge.Ownership
			.SelectMany(static family => family.Owns.Select(record => (Record: record, family.Family)))
			.ToDictionary(static one => one.Record, static one => one.Family, StringComparer.Ordinal);

		var counts  = Count(outcomes);
		var reasons = outcomes
			.Where(static one => one.Reason is not null)
			.GroupBy(static one => one.Counted)
			.ToDictionary(
				static group => group.Key,
				static group => group.GroupBy(static one => one.Reason!).ToDictionary(static one => one.Key, static one => one.Count()));

		Summarize("corpus", fromCorpus);
		Summarize("TransactSqlTests", fromTests);
		Summarize("both", outcomes, 60);
		Faults(outcomes);

		Assert.Equal([], counts.Keys.Where(one => !families.ContainsKey(one)).Select(static one => $"{one}: counted and owned by no family"));

		if (Environment.GetEnvironmentVariable(Baselines.Variable) is { Length: > 0 })
		{
			foreach (var (family, owns) in Bridge.Ownership)
				Baselines.Write(family, owns, counts);

			var occurrences = corpus
				.SelectMany(static one => Trees.Nodes(one.Statement))
				.GroupBy(Bridge.NameOf)
				.ToDictionary(static group => group.Key, static group => group.Count(), StringComparer.Ordinal);

			BridgeReport.Write(families, counts, reasons, occurrences);

			return;
		}

		var faults = new List<string>();
		var better = new List<string>();

		foreach (var (family, owns) in Bridge.Ownership)
		{
			var baseline = Baselines.Read(family);

			foreach (var record in owns)
			{
				var now  = counts.TryGetValue(record, out var found) ? found : Baselines.None;
				var then = baseline.TryGetValue(record, out var kept) ? kept : null;

				if (then is null)
				{
					faults.Add($"{family}: {record} is not in Baseline.{family}.txt. Add `{record} = {now}`.");

					continue;
				}

				if (now.Converted < then.Converted)
					faults.Add($"{family}: {record} converted {now.Converted} where the baseline converted {then.Converted}.");

				if (now.Refused > then.Refused)
					faults.Add($"{family}: {record} refused {now.Refused} against {then.Refused}.");

				if (now.Unread > then.Unread)
					faults.Add($"{family}: {record} unread {now.Unread} against {then.Unread}.");

				if (now.Different > then.Different)
					faults.Add($"{family}: {record} different {now.Different} against {then.Different}.");

				if (now.Shape > then.Shape)
					faults.Add($"{family}: {record} shape {now.Shape} against {then.Shape}.");

				if (now != then && now.Converted >= then.Converted && now.Refused <= then.Refused && now.Unread <= then.Unread
					&& now.Different <= then.Different && now.Shape <= then.Shape)
					better.Add($"{family}: {record} = {then} -> {now}");
			}
		}

		// Printed rather than asserted: the baselines are rewritten deliberately, not by a passing run.
		if (better.Count > 0)
			foreach (var line in better.Prepend($"changed for the better, so rewrite the baselines deliberately ({Baselines.Variable}=1):"))
				output.WriteLine(line);

		Assert.Equal([], faults);
	}

	static Dictionary<string, Baselines.Counts> Count(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		var counts = new Dictionary<string, Baselines.Counts>(StringComparer.Ordinal);

		foreach (var one in outcomes)
		{
			var was = counts.TryGetValue(one.Counted, out var found) ? found : Baselines.None;

			counts[one.Counted] = one.Status switch
			{
				BridgeOracle.Status.Converted => was with { Converted = was.Converted + 1 },
				BridgeOracle.Status.Gap       => was with { Gap = was.Gap + 1 },
				BridgeOracle.Status.Refused   => was with { Refused = was.Refused + 1 },
				BridgeOracle.Status.Unread    => was with { Unread = was.Unread + 1 },
				_                             => was with { Different = was.Different + 1 },
			};

			if (one.Shape)
				counts[one.Counted] = counts[one.Counted] with { Shape = counts[one.Counted].Shape + 1 };
		}

		return counts;
	}

	void Summarize(string name, IReadOnlyList<BridgeOracle.Outcome> outcomes, int reasons = 10)
	{
		int Of(BridgeOracle.Status status)
		{
			return outcomes.Count(one => one.Status == status);
		}

		output.WriteLine(
			$"{name}: {outcomes.Count} statements; converted {Of(BridgeOracle.Status.Converted)} " +
			$"({outcomes.Count(static one => one.ByCase)} with the case of a key word aside), gap {Of(BridgeOracle.Status.Gap)}, " +
			$"refused {Of(BridgeOracle.Status.Refused)}, unread {Of(BridgeOracle.Status.Unread)}, different {Of(BridgeOracle.Status.Different)}; " +
			$"read by the standard's parser {outcomes.Count(static one => one.Standard)}, shape {outcomes.Count(static one => one.Shape)}, " +
			$"read with another meaning {outcomes.Count(static one => one.Dialect is not null)}");

		foreach (var (family, owns) in Bridge.Ownership)
		{
			var mine = outcomes.Where(one => owns.Contains(one.Counted)).ToList();

			if (mine.Count > 0)
				output.WriteLine(
					$"  {family}: converted {mine.Count(static one => one.Status == BridgeOracle.Status.Converted)}, " +
					$"gap {mine.Count(static one => one.Status == BridgeOracle.Status.Gap)}, " +
					$"refused {mine.Count(static one => one.Status == BridgeOracle.Status.Refused)}, " +
					$"unread {mine.Count(static one => one.Status == BridgeOracle.Status.Unread)}, " +
					$"different {mine.Count(static one => one.Status == BridgeOracle.Status.Different)}, " +
					$"shape {mine.Count(static one => one.Shape)}");
		}

		output.WriteLine($"  the {reasons} commonest gap reasons:");

		foreach (var reason in outcomes.Where(static one => one.Reason is not null).GroupBy(static one => $"{one.Counted}: {one.Reason}").OrderByDescending(static one => one.Count()).Take(reasons))
			output.WriteLine($"    {reason.Count(),6}  {reason.Key}");
	}

	/// <summary>
	/// Every statement that was refused, unread, different or of another shape, with what explains it.
	/// </summary>
	void Faults(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		foreach (var one in outcomes.Where(static one => one.Status is BridgeOracle.Status.Refused or BridgeOracle.Status.Unread or BridgeOracle.Status.Different || one.Shape || one.Dialect is not null).Take(300))
		{
			output.WriteLine($"{one.Status}{(one.Shape ? " (shape)" : "")}{(one.Dialect is { } meaning ? $" (another meaning: {meaning})" : "")} {one.Root}: {Flat(one.Text)}");

			if (one.Written is not null)
				output.WriteLine($"    written: {one.Written}");

			if (one.Detail is not null)
				output.WriteLine($"    {one.Detail}");
		}
	}

	static string Flat(string text)
	{
		return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
	}
}
