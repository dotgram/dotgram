using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using DotGram.Parsers;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// The same corpus put to the engine, which is the only source here that is not somebody's
/// reading of the language.
/// </summary>
/// <remarks>
/// <para>
/// <c>--kinds</c> compares this grammar with ScriptDom, and there is a limit to what two
/// parsers can settle between them: where they differ, neither is evidence. SQL Server
/// answers instead. <c>SET PARSEONLY ON</c> makes it parse a statement and compile nothing,
/// so no table has to exist and nothing runs — which is exactly the question a syntax has an
/// answer to.
/// </para>
/// <para>
/// <b>Four cells, and three of them are worth something.</b> Where the engine and this
/// grammar agree there is nothing to say. Where the engine reads and this does not, that is
/// the work list — the same one <c>--kinds</c> makes, now with an authority behind it rather
/// than another parser's opinion. Where <em>this</em> reads and the engine does not, that is
/// a defect here and the half of correctness a refusal count cannot see. And where ScriptDom
/// reads what the engine refuses, that is a finding about ScriptDom.
/// </para>
/// <para>
/// Nothing in CI depends on any of it: it needs a SQL Server on the machine, and it says so
/// and stops where there is none.
/// </para>
/// </remarks>
static class Engine
{
	public static void Run(string? root, string version, int shown)
	{
		root ??= Corpus.Checked();

		if (!Directory.Exists(root))
		{
			Console.WriteLine($"No corpus at {root}.");

			return;
		}

		if (Kinds.Version(version) is not { } splitter)
		{
			Console.WriteLine($"No such version as {version}. One of 80 90 100 … 180.");

			return;
		}

		using var connection = Connected(version);

		if (connection is null)
			return;

		ParseOnly(connection);

		var both     = 0;
		var engine   = 0;   // the engine reads it and this does not — the work list
		var ours     = 0;   // this reads it and the engine does not — a defect here
		var elsewhere = 0;  // this reads it and the engine is another product's
		var neither  = 0;
		var overRead = new List<string>();
		var elsewhereShown = new List<string>();
		var gaps     = new Dictionary<string, (int Count, List<string> Like)>(StringComparer.Ordinal);
		var said      = new Dictionary<int, int>();
		var saidAbout = new Dictionary<int, string>();

		foreach (var file in Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories))
		{
			var text = File.ReadAllText(file);

			using var reader = new StringReader(text);

			if (splitter.Parse(reader, out var errors) is not TSqlScript script || errors.Count > 0)
				continue;

			foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
			{
				if (!Kinds.Modelled(statement.GetType().Name))
					continue;

				var one = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();

				var here    = TransactSql.TryParseStatement(one).IsSuccess;
				var message = Answer(connection, one);
				var says    = message == 0 || AboutNames(message);

				if (message != 0)
				{
					said[message] = said.TryGetValue(message, out var before) ? before + 1 : 1;

					// One statement per message, so that what a number means can be read off
					// the report rather than looked up. Which side of `AboutNames` a message
					// belongs on is a judgement about what the engine did — `Msg 7801` is
					// "the required parameter LISTENER_PORT was not specified", which is it
					// having read the statement and objected to what was in it — and a
					// judgement wants its evidence beside it.
					if (!saidAbout.ContainsKey(message))
						saidAbout[message] = Corpus.One(one);
				}

				if (here && says)
				{
					both++;
				}
				else if (says)
				{
					engine++;

					var why = Corpus.Stopped(one, (int)TransactSql.TryParseStatement(one).Position);

					var (count, like) = gaps.TryGetValue(why, out var before) ? before : (0, new List<string>());

					if (like.Count < shown)
						like.Add(Corpus.One(one));

					gaps[why] = (count + 1, like);
				}
				else if (here && Elsewhere(one, message))
				{
					elsewhere++;

					if (elsewhereShown.Count < shown)
						elsewhereShown.Add(Corpus.One(one));
				}
				else if (here)
				{
					ours++;

					if (overRead.Count < shown * 20)
						overRead.Add(Corpus.One(one));
				}
				else
				{
					neither++;
				}
			}
		}

		Report(
			version, both, engine, ours, elsewhere, neither, gaps, overRead, elsewhereShown,
			said, saidAbout, shown);
	}

	/// <summary>Puts the connection into the mode where it reads and does nothing else.</summary>
	static void ParseOnly(SqlConnection connection)
	{
		using var command = connection.CreateCommand();

		command.CommandText = "SET PARSEONLY ON";
		command.ExecuteNonQuery();
	}

	/// <summary>
	/// The message the engine answered with, or zero where it had nothing to say.
	/// </summary>
	/// <remarks>
	/// A statement in this corpus can take the connection down with it — a severity the
	/// server ends the session over rather than answers. So the state is checked before each
	/// one and the session is built again where it has gone, which costs nothing on the
	/// thousands that do not and is the difference between a number and a stack trace.
	/// </remarks>
	static int Answer(SqlConnection connection, string statement)
	{
		if (connection.State != ConnectionState.Open)
		{
			connection.Close();
			connection.Open();

			ParseOnly(connection);
		}

		using var command = connection.CreateCommand();

		command.CommandText = statement;

		try
		{
			command.ExecuteNonQuery();

			return 0;
		}
		catch (SqlException failed)
		{
			return failed.Number;
		}
	}

	/// <summary>
	/// What the engine says that is not about syntax, and so is not an answer to the
	/// question being asked.
	/// </summary>
	/// <remarks>
	/// <c>PARSEONLY</c> compiles nothing, and it still resolves some names: an undeclared
	/// variable (137) is far and away the commonest thing in this corpus, since a statement
	/// cut out of a script leaves its <c>DECLARE</c> behind. A function nobody has heard of
	/// (195), a table (208) or a column (207) that does not exist, an undeclared table
	/// variable (1087), a collation nobody has heard of (448), a name with more prefixes
	/// than a name may have (117), two locking hints that contradict each other (1047), a
	/// window function with no `ORDER BY` (4112), a hint name it does not know (10715), and
	/// a bulk format it cannot apply to the file it was given (5369, 5371, 5374) are all
	/// the same kind of thing: the statement was read, and the engine then had an opinion
	/// about what it named.
	/// <para>
	/// The set is small and open on purpose, and the report tallies every message number it
	/// saw so that what is missing from it shows up as a row rather than as a wrong total.
	/// </para>
	/// </remarks>
	/// <summary>
	/// Whether the engine refused this because it is not the engine the statement is for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The server on this machine is one product of a family, and the corpus is a parser's,
	/// so it holds the rest of the family too: Azure SQL Database, Synapse and the Fabric
	/// warehouse. <c>CREATE DATABASE d (SERVICE_OBJECTIVE = 'basic')</c> is real T-SQL that
	/// this server cannot accept and never will, and a grammar taught to refuse it would be
	/// refusing what it is for.
	/// </para>
	/// <para>
	/// So they are counted apart rather than as a defect here. Two ways of telling, and the
	/// first is the engine's own: <b>Msg 40514</b> is "not supported in this version of SQL
	/// Server", which is it saying exactly this. The second is a list of names, because for
	/// syntax it has never heard of the engine answers Msg 102 like any other error — and a
	/// list is what an auditable answer looks like when the authority has none. Each name is
	/// here because a statement using it was seen in the corpus and read by this grammar; the
	/// product it belongs to is in the comment beside it.
	/// </para>
	/// <para>
	/// It errs towards calling something ours: a statement is only counted elsewhere when one
	/// of these words is in it, so anything unlisted lands in the work list where it will be
	/// looked at.
	/// </para>
	/// </remarks>
	static bool Elsewhere(string statement, int message) =>
		message == 40514 ||
		OtherProducts.Any(word => statement.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

	/// <summary>What names a statement as another product's, and which product that is.</summary>
	static readonly string[] OtherProducts =
	[
		// Azure SQL Database: what a database is sized and priced as, and how one is copied.
		"SERVICE_OBJECTIVE",
		"ELASTIC_POOL",
		"AS COPY OF",
		"MAXSIZE",
		"EDITION =",

		// Azure SQL Database, retired: federations.
		"USE FEDERATION",
		"FEDERATED ON",

		// Synapse and the Fabric warehouse: how a table is spread and what stands over it.
		"MATERIALIZED VIEW",
		"DISTRIBUTION =",
		"CLUSTERED COLUMNSTORE INDEX ORDER",
		"CLONE AT",
		"CLUSTER BY",

		// Synapse and PolyBase: what is read from outside the database.
		"EXTERNAL DATA SOURCE",
		"EXTERNAL FILE FORMAT",
		"EXTERNAL TABLE",

		// Synapse dedicated pools: the resource governor's own vocabulary there.
		"WORKLOAD GROUP",
		"WORKLOAD CLASSIFIER",
	];

	static bool AboutNames(int message) =>
		message is 117 or 137 or 195 or 207 or 208 or 448 or 1047 or 1087
			or 4104 or 4112 or 4145
			or 5369 or 5371 or 5374
			or 10715

			// Read, and then objected to on grounds that are not the shape of the statement.
			// Each of these is the engine's own wording, taken from `sys.messages` rather
			// than remembered — an endpoint that names no port was understood and found
			// wanting, which is a different answer from not being understood.
			//
			//    135  Cannot use a BREAK statement outside the scope of a WHILE statement.
			//    148  Incorrect time syntax in time string '…' used with WAITFOR.
			//    153  Invalid usage of the option … in the … statement.
			//   1003  Line …: … clause allowed only for ….
			//   1020  Sub-entity lists cannot be specified for entity-level permissions.
			//   1054  Syntax '…' is not allowed in schema-bound objects.
			//   7801  The required parameter … was not specified.
			//   7853  The URL specified as the path … must begin with "/".
			//   7861  "…" endpoints can only be of the "FOR …" type.
			//  13539  Setting SYSTEM_VERSIONING to ON failed because history table ….
			//  15151  Cannot … the … , because it does not exist or you do not have permission.
			or 135 or 148 or 153
			or 1003 or 1020 or 1054
			or 7801 or 7853 or 7861
			or 13539 or 15151;

	/// <summary>
	/// The local engine, on a database whose compatibility level is the version being asked
	/// about — or null and a line saying why not.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One database per level, made by hand and named for it: `DotGram_100` through
	/// `DotGram_170`. Asking a 2025 server about version 130 on a 170 database would have it
	/// confirm syntax that version never had, which is the whole reason the level is set.
	/// </para>
	/// <para>
	/// <b>And it only goes so far, which is worth knowing before the numbers are read.</b>
	/// The engine has one parser and the level gates part of what it reads, not all of it:
	/// the `WINDOW` clause is refused below 160 and `FOR JSON`, `OFFSET`/`FETCH`,
	/// `IS DISTINCT FROM` and `FOR SYSTEM_TIME` all read at 100. So the engine is the
	/// authority on whether something is T-SQL and only a partial one on which version it
	/// belongs to — where ScriptDom, whose twelve parsers are twelve grammars, is the other
	/// way round. Neither is the oracle alone; that is why there are three.
	/// </para>
	/// <para>
	/// SQL Server 2025 will not go below 100, which settles which source is the authority
	/// where: <b>ScriptDom for 80 and 90, the engine for everything above.</b> The two older
	/// versions have no engine that can be asked about them at all, and ScriptDom's parsers
	/// for them are the last reading of those languages anybody wrote down; from 100 up the
	/// engine can be asked, and what it says outranks what any parser thinks.
	/// </para>
	/// </remarks>
	static SqlConnection? Connected(string version)
	{
		if (version is "80" or "90")
		{
			Console.WriteLine();
			Console.WriteLine(
				$"No engine to ask about {version}: a 2025 server will not go below " +
				"compatibility level 100, and asking on a higher one would answer about the " +
				$"wrong language. Below 100 the authority is ScriptDom — `--kinds {version}`.");

			return null;
		}

		var connection = new SqlConnection(
			$"Server=localhost;Database=DotGram_{version};Integrated Security=true;" +
			"TrustServerCertificate=true;Connect Timeout=5;Application Name=DotGram");

		try
		{
			connection.Open();

			return connection;
		}
		catch (Exception failed)
		{
			Console.WriteLine();
			Console.WriteLine($"No engine to ask: {failed.Message}");
			Console.WriteLine(
				$"This one needs a SQL Server on the machine and a database `DotGram_{version}` " +
				$"at compatibility level {version}. Nothing else here needs either.");

			connection.Dispose();

			return null;
		}
	}

	static void Report(
		string version, int both, int engine, int ours, int elsewhere, int neither,
		Dictionary<string, (int Count, List<string> Like)> gaps,
		List<string> overRead,
		List<string> elsewhereShown,
		Dictionary<int, int> said,
		Dictionary<int, string> saidAbout,
		int shown)
	{
		var all = both + engine + ours + elsewhere + neither;

		Console.WriteLine();
		Console.WriteLine(
			$"against the engine at compatibility level {version}, " +
			$"on the kinds this grammar has a rule for");
		Console.WriteLine();
		Console.WriteLine($"  {all} statements");
		Console.WriteLine($"  {both,6}  both read");
		Console.WriteLine($"  {engine,6}  the engine reads and this does not — the work list");
		Console.WriteLine($"  {ours,6}  this reads and the engine does not — a defect here");
		Console.WriteLine(
			$"  {elsewhere,6}  this reads and the engine is not the one to ask — another product's");
		Console.WriteLine($"  {neither,6}  neither, which is the corpus being a corpus of errors too");
		Console.WriteLine();

		if (ours > 0)
		{
			Console.WriteLine("  read here and refused by the engine:");

			foreach (var one in overRead)
				Console.WriteLine($"      {one}");

			Console.WriteLine();
		}

		if (elsewhere > 0)
		{
			Console.WriteLine("  read here and refused because this server is not that product:");

			foreach (var one in elsewhereShown)
				Console.WriteLine($"      {one}");

			Console.WriteLine();
		}

		Console.WriteLine("  and what it answered with, message by message:");
		Console.WriteLine();

		foreach (var (message, count) in said.OrderByDescending(one => one.Value).ThenBy(one => one.Key))
		{
			Console.WriteLine(
				$"  {count,5}  Msg {message}{(AboutNames(message) ? "  (read, and about names rather than syntax)" : "")}");

			if (saidAbout.TryGetValue(message, out var about))
				Console.WriteLine($"         {about}");
		}

		Console.WriteLine();
		Console.WriteLine("  where the work list stopped:");
		Console.WriteLine();

		foreach (var (why, one) in gaps.OrderByDescending(one => one.Value.Count).ThenBy(one => one.Key))
		{
			Console.WriteLine($"  {one.Count,5}  stops at {why}");

			foreach (var example in one.Like.Take(shown))
				Console.WriteLine($"           {example}");
		}
	}
}
