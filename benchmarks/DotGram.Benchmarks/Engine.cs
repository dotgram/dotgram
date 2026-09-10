using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using DotGram.Parsers.Sql;

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
		var mine      = new Dictionary<int, int>();
		var mineAbout = new Dictionary<int, string>();

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

				var here    = Parse(version, one).Read;
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

					var why = Corpus.Stopped(one, Parse(version, one).At);

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

					// Tallied apart from everything else, because this is the bucket that is
					// work: what the engine answered the statements this grammar should not
					// have read, most first, with one of them beside each number.
					mine[message] = mine.TryGetValue(message, out var again) ? again + 1 : 1;

					if (!mineAbout.ContainsKey(message))
						mineAbout[message] = Corpus.One(one);

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
			said, saidAbout, mine, mineAbout, shown);
	}

	/// <summary>
	/// This grammar's answer at one compatibility level: whether it read the statement, and
	/// where it stopped.
	/// </summary>
	/// <remarks>
	/// Each level is a reading of the grammar published under its own name, and all of them
	/// share one machine. A level with no parser of its own is asked of the one that names
	/// none, which reads what every level reads.
	/// </remarks>
	internal static (bool Read, int At) Parse(string version, string text)
	{
		var match = version switch
		{
			"100" => TransactSql.TryParseStatement100(text),
			"110" => TransactSql.TryParseStatement110(text),
			"120" => TransactSql.TryParseStatement120(text),
			"130" => TransactSql.TryParseStatement130(text),
			"140" => TransactSql.TryParseStatement140(text),
			"150" => TransactSql.TryParseStatement150(text),
			"160" => TransactSql.TryParseStatement160(text),
			"170" => TransactSql.TryParseStatement170(text),
			_     => TransactSql.TryParseStatement(text),
		};

		return (match.IsSuccess, (int)match.Position);
	}

	/// <summary>
	/// The same for a text of statements, which is what the engine is sent when it is asked
	/// about a line: `SET ONLINE SELECT 1` is two statements to it and one text here.
	/// </summary>
	internal static (bool Read, int At) ParseText(string version, string text)
	{
		var match = version switch
		{
			"100" => TransactSql.TryParseSql100(text),
			"110" => TransactSql.TryParseSql110(text),
			"120" => TransactSql.TryParseSql120(text),
			"130" => TransactSql.TryParseSql130(text),
			"140" => TransactSql.TryParseSql140(text),
			"150" => TransactSql.TryParseSql150(text),
			"160" => TransactSql.TryParseSql160(text),
			"170" => TransactSql.TryParseSql170(text),
			_     => TransactSql.TryParseSql(text),
		};

		return (match.IsSuccess, (int)match.Position);
	}

	/// <summary>Puts the connection into the mode where it reads and does nothing else.</summary>
	internal static void ParseOnly(SqlConnection connection)
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
	internal static int Answer(SqlConnection connection, string statement, bool again = false)
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
		catch (SqlException failed) when (failed.Number == 596 && !again)
		{
			// "Cannot continue the execution because the session is in the kill state": the
			// statement before this one took the session down, and this is not an answer about
			// this one. Asked again on a session of its own.
			connection.Close();
			connection.Open();

			ParseOnly(connection);

			return Answer(connection, statement, again: true);
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
		// 40514 "'…' is not supported in this version of SQL Server", and 40517 the same
		// answer about one keyword or option rather than a whole feature.
		message is 40514 or 40517 ||
		// 33161 "Database master keys without password are not supported in this version of
		// SQL Server" — the same answer about one statement, which Azure SQL Database reads.
		message is 33161 ||
		OtherProducts.Any(word => Compact(statement).IndexOf(Compact(word), StringComparison.OrdinalIgnoreCase) >= 0);

	/// <summary>A text with its whitespace taken out, so that `with(order(A))` is `WITH (ORDER (A))`.</summary>
	static string Compact(string text) => string.Concat(text.Where(static one => !char.IsWhiteSpace(one)));

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

		// Azure SQL Edge: rows deleted when they are old enough. The engine refuses the clause
		// as syntax, except where a unit it does not know makes it read it and object (47305).
		"DATA_DELETION",

		// Synapse and PolyBase: what is read from outside the database.
		"EXTERNAL DATA SOURCE",
		"EXTERNAL FILE FORMAT",
		"EXTERNAL TABLE",

		// Synapse dedicated pools: the resource governor's own vocabulary there.
		"WORKLOAD GROUP",
		"WORKLOAD CLASSIFIER",

		// Synapse dedicated pools: partitions split, merged and switched over data already in
		// them, and a columnstore ordered inside its WITH. "Partitioning tables in dedicated SQL
		// pool" on learn.microsoft.com; SQL Server writes an ordered columnstore's ORDER outside.
		"TRUNCATE_TARGET",
		"SPLIT RANGE",
		"MERGE RANGE",
		"WITH (ORDER (",

		// Synapse serverless and Fabric: a Parquet file read as a rowset.
		"'PARQUET'",

		// Azure SQL Database: automatic tuning inherited from the logical server, and the two
		// index recommendations only it makes. SQL Server has FORCE_LAST_GOOD_PLAN alone.
		"AUTOMATIC_TUNING = INHERIT",
		"CREATE_INDEX =",
		"DROP_INDEX =",
		"MAINTAIN_INDEX =",
	];

	internal static bool AboutNames(int message) =>
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
			//   1003  Line …: … clause allowed only for ….
			//   1020  Sub-entity lists cannot be specified for entity-level permissions.
			//   1054  Syntax '…' is not allowed in schema-bound objects.
			//   7801  The required parameter … was not specified.
			//   7853  The URL specified as the path … must begin with "/".
			//   7861  "…" endpoints can only be of the "FOR …" type.
			//  13539  Setting SYSTEM_VERSIONING to ON failed because history table ….
			//  15151  Cannot … the … , because it does not exist or you do not have permission.
			or 135 or 148
			or 1003 or 1020 or 1054
			or 7801 or 7853 or 7861
			or 13539 or 15151

			// The same audit run again over what was left, which is how a list like this is
			// meant to grow: the report tallies the messages of the refusals alone, and each
			// number is looked up before it moves.
			//
			//    140  Can only use IF UPDATE within a CREATE TRIGGER statement.
			//    174  The … function requires … argument(s).
			//   1039  Option '…' is specified more than once.
			//   1052  Conflicting … options "…" and "…".
			//   1062  The TOP N WITH TIES clause is not allowed without a corresponding ORDER BY.
			//   1092  In this context … statistics name(s) cannot be specified for option '…'.
			//   1098  The specified event type(s) is/are not valid on the specified target object.
			//   8169  Conversion failed when converting from a character string to uniqueidentifier.
			//  10714  An action of type '…' cannot appear more than once in a MERGE statement.
			//  10779  The durability option 'schema_only' is supported only with memory optimized tables.
			//  10790  The option '…' can be specified only for hash indexes.
			//  10794  The … '…' is not supported with ….
			or 140 or 174
			or 1039 or 1052 or 1062 or 1092 or 1098
			or 8169
			or 10714 or 10779 or 10790 or 10794

			// And a third time, over what level 150's own parser left, 2026-09-10.
			//
			//    136  Cannot use a CONTINUE statement outside the scope of a WHILE statement.
			//    173  The definition for column '…' must include a data type.
			//   1036  File option … is required in this CREATE/ALTER DATABASE statement.
			//   1095  "…" has already been specified as an event type.
			//   4122  Remote table-valued function calls are not allowed.
			//   4136  The hint '…' cannot be used with the hint '…'.
			//   7864  CREATE/ALTER ENDPOINT cannot be used to update the endpoint with this information.
			//   7887  The IPv6 address specified is not supported.
			//  10324  WITH ENCRYPTION option of CREATE TRIGGER is only applicable to T-SQL triggers.
			//  10704  To rethrow an error, a THROW statement must be used inside a CATCH block.
			//  10757  The function '…' may not have a WITHIN GROUP clause.
			//  10797  Only one MEMORY_OPTIMIZED_DATA filegroup is allowed per database.
			//  14808  Cannot disable REMOTE_DATA_ARCHIVE when migration is enabled.
			or 136 or 173
			or 1036 or 1095
			or 4122 or 4136
			or 7864 or 7887
			or 10324 or 10704 or 10757 or 10797
			or 14808

			// And two the index options' catalogue turned up.
			//
			//   1080  The integer value … is out of range.
			//  11431  The … option is not permitted as the … option is not turned '…'.
			or 1080 or 11431

			// And the ones the database's settings and scoped configuration turned up.
			//
			//   5091  ALTER DATABASE change tracking option '…' was specified more than once.
			//  10770  The SERVER option and one of CREDENTIAL or FEDERATED_SERVICE_ACCOUNT = ON ….
			//  10771  The CREDENTIAL option cannot be used with the FEDERATED_SERVICE_ACCOUNT = ON option.
			//  12108  '…' is out of range for the database scoped configuration option '…'.
			//  12109  Statement '…' failed, because it attempted to set the value to '…' for the primary ….
			//  12110  Statement '…' failed, because it attempted to set the '…' option for the secondaries ….
			//  12121  Time value … used with PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES is not valid ….
			//  12401  The … option '…' was specified more than once.
			//  12417  Only one Query Store option can be given in ALTER DATABASE statement.
			//  15701  Statement … failed, because it attempted to set the Automatic Tuning option … multiple times.
			//  31207  Invalid value for Full-Text index version is specified.
			or 5091 or 10770 or 10771
			or 12108 or 12109 or 12110 or 12121 or 12401 or 12417
			or 15701 or 31207

			// And a table's.
			//
			//  10737  … when a partition is specified in a DATA_COMPRESSION clause, PARTITION=ALL must be ….
			//  10798  This is not a valid data compression setting for this object.
			//  13743  … is not a valid value for system versioning history retention period.
			//  13744  '…' is not a valid history retention period unit for system versioning.
			//  14860  '…' expects parameter '…', which was not supplied.
			//  14912  REMOTE_DATA_ARCHIVE with value set to OFF_WITHOUT_DATA_RECOVERY is not supported at ….
			//  33411  The option '…' is only valid when used on a FileTable.
			or 10737 or 10798
			or 13743 or 13744
			or 14860 or 14912
			or 33411

			// And a user's.
			//
			//  33234  The parameter … cannot be provided for users that cannot authenticate in a database.
			//  33235  The parameter … cannot be provided for users that cannot authenticate in a database. Remove the WITHOUT LOGIN or PASSWORD clause.
			or 33234 or 33235

			// And a database's creation.
			//
			//    188  Cannot specify a log file in a CREATE DATABASE statement without also specifying at least one data file.
			or 188

			// And the hints.
			//
			//   1065  The NOLOCK and READUNCOMMITTED lock hints are not allowed for target tables of ….
			//   1069  Index hints are only allowed in a FROM or OPTION clause.
			//  10724  The FORCESEEK hint is not allowed for target tables of INSERT, UPDATE, or DELETE statements.
			or 1065 or 1069 or 10724;

		// Not 153, 155 or 487 — an option the engine does not know, or one where it does not
		// belong. They stood here while this grammar read every option list as an open
		// vocabulary; with the catalogue written from the published syntax they are refusals
		// it has to share, and a statement answered with one is not a statement read.

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
	internal static SqlConnection? Connected(string version)
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
		Dictionary<int, int> mine,
		Dictionary<int, string> mineAbout,
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

		if (mine.Count > 0)
		{
			Console.WriteLine("  what it answered the ones read here and refused, which is the work:");
			Console.WriteLine();

			foreach (var (message, count) in mine.OrderByDescending(one => one.Value).ThenBy(one => one.Key))
			{
				Console.WriteLine($"  {count,5}  Msg {message}");

				if (mineAbout.TryGetValue(message, out var about))
					Console.WriteLine($"         {about}");
			}

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
