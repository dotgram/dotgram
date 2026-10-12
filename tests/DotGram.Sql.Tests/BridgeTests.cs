using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

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
	/// The statements expected to fail the round trip because of a known writer defect, each with the status
	/// it fails with, as flattened text. Listed one by one, so that a statement newly broken cannot hide
	/// behind one fixed: one that fails and is not here fails the test, and one here that passes is printed.
	/// </summary>
	static readonly Dictionary<string, BridgeOracle.Status> Expected = new(StringComparer.Ordinal)
	{
	};

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
	/// Free text compares token by token: a key word without case, and nothing else — not a name in brackets,
	/// not a word that is no key word, not a string.
	/// </summary>
	[Theory]
	[InlineData("with (delayed_durability = on)", "WITH (DELAYED_DURABILITY = ON)", true)]
	[InlineData("varchar (10)",                   "VARCHAR(10)",                    true)]
	[InlineData("national character varying(5)", "NATIONAL CHARACTER VARYING(5)",  true)]
	[InlineData("AUTHORIZATION [a b]",            "AUTHORIZATION [ab]",             false)]
	[InlineData("BEGIN TRAN T1",                  "BEGIN TRAN t1",                  false)]
	[InlineData("WITH MARK 'A'",                  "WITH MARK 'a'",                  false)]
	[InlineData("AS dbo.MyInt",                   "AS dbo.myint",                   false)]
	public void Free_text_compares_key_words_without_case_and_nothing_else(string one, string two, bool same)
	{
		Assert.Equal(same, Trees.SameTokens(new object(), "Statement.Transaction.Tail", one, two));
	}

	/// <summary>
	/// A dialect pattern exempts its own subtree and no more: the same construct with a difference elsewhere is
	/// a shape mismatch.
	/// </summary>
	[Theory]
	[InlineData("SELECT TOP (1) a FROM t WHERE b = 1", "SELECT TOP (1) a FROM t WHERE b = 1", BridgeOracle.Top, false)]
	[InlineData("SELECT TOP (1) a FROM t WHERE b = 1", "SELECT TOP (1) a FROM t WHERE b = 2", BridgeOracle.Top, true)]
	[InlineData("UPDATE t SET u.c = 1 WHERE b = 1",    "UPDATE t SET u.c = 1 WHERE b = 1",    BridgeOracle.SetTc, false)]
	[InlineData("UPDATE t SET u.c = 1 WHERE b = 1",    "UPDATE t SET u.c = 1 WHERE b = 2",    BridgeOracle.SetTc, true)]
	[InlineData("OPEN GLOBAL c",                        "OPEN GLOBAL c",                       BridgeOracle.Global, false)]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'x')", "SELECT a FROM t WHERE FREETEXT ((b), 'x')", BridgeOracle.FullText, false)]
	[InlineData("SELECT a FROM t WHERE FREETEXT ((b), 'x')", "SELECT a FROM t WHERE FREETEXT ((b), 'y')", BridgeOracle.FullText, true)]
	public void A_dialect_pattern_exempts_its_subtree_and_no_more(string bridged, string standard, string pattern, bool shape)
	{
		var made        = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(bridged), bridged);
		var differences = Trees.Differences(made, SqlStandardParser.ParseStatement(standard), old: false);
		var exempted    = BridgeOracle.Meaning(differences, out var unexplained);

		Assert.Contains(pattern, exempted);
		Assert.Equal(shape, unexplained.Count > 0);
	}

	/// <summary>
	/// The word AS of an alias is a word: a name <c>"as"</c> before the alias is not it.
	/// </summary>
	[Theory]
	[InlineData("SELECT a FROM \"as\" x",    false)]
	[InlineData("SELECT a FROM [as] x",      false)]
	[InlineData("SELECT a FROM \"as\" AS x", true)]
	public void A_quoted_name_is_not_the_word_AS(string text, bool said)
	{
		var made = (Ast.Statement.Select)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
		var from = ((Ast.Query.Select)made.Query).From!.Sources[0];

		Assert.Equal(said, ((Ast.TableSource.Named)from).Alias!.AsKeyword);
	}

	/// <summary>
	/// A schema's owner is one name; what follows it is a schema element the old tree keeps as text.
	/// </summary>
	[Theory]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo]", false)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION [dbo] CREATE TABLE t (c int)", true)]
	[InlineData("CREATE SCHEMA s AUTHORIZATION dbo CREATE TABLE t (c int)", true)]
	public void A_schema_owner_is_one_name(string text, bool gap)
	{
		var old = TransactSqlParser.Located.ParseStatement(text);

		if (gap)
			Assert.Throws<BridgeGap>(() => Bridge.ToAst(old, text));
		else
			Assert.Equal("[dbo]", ((Ast.Statement.CreateSchema)Bridge.ToAst(old, text)).Authorization!.Name.Text);
	}

	/// <summary>
	/// Every statement of the corpus and of <c>TransactSqlTests</c> carried, written and read back, and no
	/// family doing worse than its baseline: <c>converted</c> no lower, the other five no higher; every
	/// counter of <c>Baseline.Counters.txt</c> no higher; every failure of the round trip one of
	/// <see cref="Expected"/>.
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

		var counts   = Count(outcomes);
		var counters = Counters(outcomes);
		var gaps     = outcomes
			.SelectMany(static one => one.Gaps ?? [])
			.GroupBy(static one => one.Record)
			.ToDictionary(
				static group => group.Key,
				static group => group.GroupBy(static one => one.Reason).ToDictionary(static one => one.Key, static one => one.Count()),
				StringComparer.Ordinal);

		Summarize("corpus", fromCorpus);
		Summarize("TransactSqlTests", fromTests);
		Summarize("both", outcomes, 60);
		Faults(outcomes);

		Assert.Equal([], counts.Keys.Where(one => !families.ContainsKey(one)).Select(static one => $"{one}: counted and owned by no family"));

		if (Environment.GetEnvironmentVariable(Baselines.Variable) is { Length: > 0 })
		{
			foreach (var (family, owns) in Bridge.Ownership)
				Baselines.Write(family, owns, counts);

			Baselines.WriteCounters(counters);

			var occurrences = corpus.Concat(tests)
				.SelectMany(static one => Trees.Nodes(one.Statement))
				.GroupBy(Bridge.NameOf)
				.ToDictionary(static group => group.Key, static group => group.Count(), StringComparer.Ordinal);

			BridgeReport.Write(families, counts, gaps, occurrences, counters, Expected);

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

				var worse = new List<string>();

				if (now.Converted < then.Converted)
					worse.Add($"converted {now.Converted} where the baseline converted {then.Converted}");

				Rise(worse, "refused", now.Refused, then.Refused);
				Rise(worse, "crashed", now.Crashed, then.Crashed);
				Rise(worse, "unread", now.Unread, then.Unread);
				Rise(worse, "different", now.Different, then.Different);
				Rise(worse, "shape", now.Shape, then.Shape);

				faults.AddRange(worse.Select(one => $"{family}: {record} {one}."));

				if (now != then && worse.Count == 0)
					better.Add($"{family}: {record} = {then} -> {now}");
			}
		}

		var recorded = Baselines.Rows("Counters").ToDictionary(static one => one.Key, static one => int.Parse(one.Value), StringComparer.Ordinal);

		foreach (var (key, now) in counters)
		{
			var then = recorded.TryGetValue(key, out var kept) ? kept : 0;

			if (now > then)
				faults.Add($"Counters: {key} = {now} against {then}.");
			else if (now < then)
				better.Add($"Counters: {key} = {then} -> {now}");
		}

		foreach (var one in outcomes.Where(static one => one.Status is not (BridgeOracle.Status.Converted or BridgeOracle.Status.Gap)))
		{
			if (!Expected.TryGetValue(Flat(one.Text), out var status) || status != one.Status)
				faults.Add($"{one.Status} and not expected: {Flat(one.Text)}");
		}

		foreach (var (text, status) in Expected)
			if (!outcomes.Any(one => one.Status == status && Flat(one.Text) == text))
				better.Add($"expected {status} and no longer: {text}");

		// Printed rather than asserted: the baselines are rewritten deliberately, not by a passing run.
		if (better.Count > 0)
			foreach (var line in better.Prepend($"changed for the better, so rewrite the baselines deliberately ({Baselines.Variable}=1):"))
				output.WriteLine(line);

		Assert.Equal([], faults);
	}

	static void Rise(List<string> worse, string what, int now, int then)
	{
		if (now > then)
			worse.Add($"{what} {now} against {then}");
	}

	static Dictionary<string, Baselines.Counts> Count(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		var counts = new Dictionary<string, Baselines.Counts>(StringComparer.Ordinal);

		foreach (var one in outcomes)
		{
			var was = counts.TryGetValue(one.Counted, out var found) ? found : Baselines.None;

			was = one.Status switch
			{
				BridgeOracle.Status.Converted => was with { Converted = was.Converted + 1 },
				BridgeOracle.Status.Gap       => was with { Gap = was.Gap + 1 },
				BridgeOracle.Status.Refused   => was with { Refused = was.Refused + 1 },
				BridgeOracle.Status.Crashed   => was with { Crashed = was.Crashed + 1 },
				BridgeOracle.Status.Unread    => was with { Unread = was.Unread + 1 },
				_                             => was with { Different = was.Different + 1 },
			};

			counts[one.Counted] = one.Shape ? was with { Shape = was.Shape + 1 } : was;
		}

		return counts;
	}

	/// <summary>
	/// The debts: each member passed only with key words' case aside, each read from the text, each dialect
	/// pattern's exempted subtrees. Every name of the closed lists has a row, zero or not.
	/// </summary>
	static Dictionary<string, int> Counters(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		var counters = new Dictionary<string, int>(StringComparer.Ordinal);

		foreach (var member in Trees.FreeText)
			counters["ByCase " + member] = 0;

		foreach (var member in Bridge.Recoverable)
			counters["Recovered " + member] = 0;

		foreach (var pattern in BridgeOracle.Patterns)
			counters["Meaning " + pattern] = 0;

		foreach (var one in outcomes)
		{
			foreach (var member in one.ByCase ?? [])
				counters["ByCase " + member]++;

			foreach (var member in one.Recovered ?? [])
				counters["Recovered " + member]++;

			foreach (var pattern in one.Dialects ?? [])
				counters["Meaning " + pattern]++;
		}

		return counters;
	}

	void Summarize(string name, IReadOnlyList<BridgeOracle.Outcome> outcomes, int reasons = 10)
	{
		int Of(BridgeOracle.Status status)
		{
			return outcomes.Count(one => one.Status == status);
		}

		output.WriteLine(
			$"{name}: {outcomes.Count} statements; converted {Of(BridgeOracle.Status.Converted)} " +
			$"({outcomes.Count(static one => one.ByCase is { Count: > 0 })} with the case of a key word aside), gap {Of(BridgeOracle.Status.Gap)}, " +
			$"refused {Of(BridgeOracle.Status.Refused)}, crashed {Of(BridgeOracle.Status.Crashed)}, unread {Of(BridgeOracle.Status.Unread)}, " +
			$"different {Of(BridgeOracle.Status.Different)}; read by the standard's parser {outcomes.Count(static one => one.Standard)}, " +
			$"shape {outcomes.Count(static one => one.Shape)}, read with another meaning {outcomes.Count(static one => one.Dialect)}");

		foreach (var (family, owns) in Bridge.Ownership)
		{
			var mine = outcomes.Where(one => owns.Contains(one.Counted)).ToList();

			if (mine.Count > 0)
				output.WriteLine(
					$"  {family}: converted {mine.Count(static one => one.Status == BridgeOracle.Status.Converted)}, " +
					$"gap {mine.Count(static one => one.Status == BridgeOracle.Status.Gap)}, " +
					$"refused {mine.Count(static one => one.Status == BridgeOracle.Status.Refused)}, " +
					$"crashed {mine.Count(static one => one.Status == BridgeOracle.Status.Crashed)}, " +
					$"unread {mine.Count(static one => one.Status == BridgeOracle.Status.Unread)}, " +
					$"different {mine.Count(static one => one.Status == BridgeOracle.Status.Different)}, " +
					$"shape {mine.Count(static one => one.Shape)}");
		}

		output.WriteLine($"  the {reasons} commonest first gaps:");

		foreach (var reason in outcomes.Where(static one => one.Reason is not null).GroupBy(static one => $"{one.Counted}: {one.Reason}").OrderByDescending(static one => one.Count()).Take(reasons))
			output.WriteLine($"    {reason.Count(),6}  {reason.Key}");
	}

	/// <summary>
	/// Every statement that was refused, crashed, unread, different, of another shape or read with another
	/// meaning, with what explains it.
	/// </summary>
	void Faults(IEnumerable<BridgeOracle.Outcome> outcomes)
	{
		foreach (var one in outcomes.Where(static one => one.Status is not (BridgeOracle.Status.Converted or BridgeOracle.Status.Gap) || one.Shape || one.Dialect).Take(300))
		{
			output.WriteLine($"{one.Status}{(one.Shape ? " (shape)" : "")}{(one.Dialect ? $" (another meaning: {string.Join("; ", one.Dialects!)})" : "")} {one.Root}: {Flat(one.Text)}");

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
