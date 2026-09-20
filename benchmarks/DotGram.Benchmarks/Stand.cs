using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using DotGram.ExpressionLanguage;
using DotGram.Finance.Fix;
using DotGram.Handwritten;
using DotGram.Handwritten.Fix;
using DotGram.Sql.Standard;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Benchmarks;

/// <summary>
/// Where every generated parser stands against the hand-written one it is measured by, in one
/// run: time and allocation a parse, the first call in a fresh process, what a streamed parse
/// holds, and what the generator took to write each parser (docs/design/architecture-decisions.md,
/// Q7.4 and Q7.6).
/// </summary>
/// <remarks>
/// <para>
/// <c>--stand [directory]</c> writes <c>stand.md</c> and <c>stand.json</c> there, by default into
/// a directory of its own under <c>T:\TEMP\dotgram-stand</c>, a RAM disk where losing a result
/// costs nothing. <c>--stand-compare before.json after.json</c> puts two runs side by side.
/// <c>--rebuild</c> rebuilds every grammar-hosting project first, so the generator's reports are
/// this build's rather than whatever an earlier incremental one left; left out, a row missing
/// its report is named and why, rather than the table silently coming up short.
/// </para>
/// <para>
/// Nothing is timed until both parsers of a row have answered the same (D1). The process pins
/// itself to logical processors 0 to 15 at high priority (D4), and a row of plain arithmetic
/// is timed in every round beside the parsers: two runs whose control rows differ were taken
/// on different machines, whatever the clock says. Runs are compared by their ratios to the
/// hand-written parsers, which a slower machine moves on both sides alike.
/// </para>
/// </remarks>
static partial class Stand
{
	/// <summary>
	/// One way of reading a row's input, which answers a number so that nothing it does can be
	/// optimized away.
	/// </summary>
	sealed record Reading(string Name, Func<int> Run);

	/// <summary>
	/// One input, read the hand-written way first and by every generated reading after it — or,
	/// where no hand-written parser of the format exists (the web's), by the generated one first,
	/// which is then the base every ratio is taken against.
	/// </summary>
	/// <param name="Disagreement">Null where every reading answers the same; otherwise what differs.</param>
	sealed record Workload(string Family, string Name, Reading[] Readings, Func<string?> Disagreement)
	{
		public string Id => Family + "/" + Name;
	}

	/// <summary>One round's time and, beside it, what the collector did during it.</summary>
	sealed record RoundSample(double Nanoseconds, int Gen0, int Gen1, int Gen2);

	sealed record Timed(string Reading, double Nanoseconds, double Bytes, int Warmup, RoundSample[] Rounds);

	sealed record Row(string Id, Timed[] Readings, double HandSpread, Dictionary<string, string>? Ranges = null);

	sealed record FirstCall(string Id, string Reading, double Milliseconds);

	sealed record Held(string Reading, double Kilobytes, long Fields);

	sealed record Generated(string Host, int Rules, long Bytes, double Milliseconds, string Mode, DateTime Written);

	sealed record Result(
		string      Commit,
		string      Machine,
		string      Runtime,
		DateTime    Taken,
		bool        Pinned,
		double      Control,
		Row[]       Rows,
		FirstCall[] FirstCalls,
		Held[]      Streamed,
		Generated[] Generation,
		string[]    MissingGeneration);

	// Picked 2026-09-18 against 7/25 (the original, most rows at 60-120% spread), 9/75 (closer
	// to clearing every row but 4x the run, over budget) and this one, which keeps the run under
	// 2x. Two rows resisted every Rounds/SampleMs combination on their own: sql/arithmetic and
	// el/interpolation, both still mid-JIT-tier-promotion at the fixed three-sample warmup this
	// used to be — see WarmUntilStable, which fixed most of it (see docs/design/stand-2026-09-18.md,
	// "sql/arithmetic: open" for what tuning alone could not).
	const int Rounds   = 8;
	const int SampleMs = 40;

	static int _sink;

	// ── Running ─────────────────────────────────────────────────────────────────

	public static void Run(string? directory, bool rebuild, string? only = null, string? against = null)
	{
		var pinned = Pin();
		var root   = Root();
		var output = directory ?? DefaultDirectory();

		Directory.CreateDirectory(output);

		if (rebuild)
			Rebuild(root);

		var workloads = Agreed();

		if (only is not null)
		{
			workloads = [.. workloads.Where(one => Matches(one.Id, only))];

			if (workloads.Length == 0)
				throw new ArgumentException($"No row has an id containing '{only}'.");
		}

		Console.WriteLine($"{workloads.Length} rows, every reading agreeing. Timing {Rounds} rounds.");

		var controls = new List<double>();
		var rows     = new List<Row>();

		foreach (var workload in workloads)
		{
			rows.Add(Measure(workload, controls));
			Console.WriteLine(Describe(rows[^1]));
		}

		var result = new Result(
			Commit(root),
			Environment.MachineName,
			System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
			DateTime.Now,
			pinned,
			Median(controls),
			[.. rows],
			only is null ? FirstCalls(workloads) : [],
			only is null ? Streamed() : [],
			only is null ? Generation(root) : [],
			only is null ? MissingGeneration(root) : []);

		var report = Markdown(result) + (only is null ? GenerationGate(result, root, against) : "");

		File.WriteAllText(Path.Combine(output, "stand.json"), JsonSerializer.Serialize(result, Json));
		File.WriteAllText(Path.Combine(output, "stand.md"), report);

		Console.WriteLine();
		Console.WriteLine(report);
		Console.WriteLine($"Written to {output}");
	}

	/// <summary>
	/// Every row, with its readings held to one another. Nothing is timed, so it is the check to
	/// make after writing a row and before asking anyone for a quiet machine.
	/// </summary>
	public static void Check() => Console.WriteLine($"{Agreed().Length} rows, every reading agreeing.");

	/// <summary>The rows of a pair held to what they say, both sides, and to one another; nothing is timed and nothing is pinned.</summary>
	public static void PairedCheck(string beforeDir, string afterDir, string? only)
	{
		PairedFixContent(beforeDir, afterDir);
		var workloads = PairedWorkloads(new PairedSide("before", beforeDir), new PairedSide("after", afterDir))
			.Where(one => only is null || Matches(one.Id, only))
			.ToArray();

		foreach (var workload in workloads)
			Check(workload);

		Console.WriteLine($"{workloads.Length} rows, every reading of both sides agreeing.");
	}

	/// <summary>What <see cref="GenerationGate"/> says of a result already taken, against another.</summary>
	public static void Gate(string now, string against) =>
		Console.WriteLine(GenerationGate(JsonSerializer.Deserialize<Result>(File.ReadAllText(now), Json)!, null, against));

	/// <summary>Whether a row's id contains any of the comma-separated pieces of an `--only`.</summary>
	static bool Matches(string id, string only) => only.Split(',').Any(piece => id.Contains(piece, StringComparison.Ordinal));

	/// <summary>
	/// Before a row is timed: each reading on its own does what the row says (a row whose name says
	/// "refused" is refused by every reading, any other is accepted by every one), and then the readings agree.
	/// The first "good" stock count was a thousand lines that both parsers refused, and nothing said so.
	/// </summary>
	static void Check(Workload workload)
	{
		if (Unasserted(workload) is { } wrong)
			throw new InvalidOperationException($"{workload.Id}: a reading does not do what the row says.\n{wrong}");

		if (workload.Disagreement() is { } disagreement)
			throw new InvalidOperationException($"{workload.Id}: the readings disagree, so no ratio would mean anything.\n{disagreement}");
	}

	/// <summary>An empty input is read to nothing: the slope's first row answers zero and is still an acceptance.</summary>
	static string? Unasserted(Workload workload)
	{
		var refuses = workload.Name.Contains("refused", StringComparison.Ordinal);
		var empty   = workload.Name.Contains("slope-0", StringComparison.Ordinal);

		foreach (var reading in workload.Readings)
		{
			var accepted = reading.Run() != 0 || empty;

			if (accepted == refuses)
				return $"  the {reading.Name} reading {(accepted ? "accepts" : "refuses")} it, and the row says it {(refuses ? "refuses" : "accepts")}";
		}

		return null;
	}

	static Workload[] Agreed()
	{
		var workloads = Workloads();

		foreach (var workload in workloads)
			Check(workload);

		return workloads;
	}

	/// <summary>
	/// The child a first call is taken in: a fresh process, which reads one row once.
	/// </summary>
	public static void First(string id, string reading)
	{
		Pin();

		var workload = Array.Find(Workloads(), one => one.Id == id)
			?? throw new ArgumentException($"No row {id}.");
		var run      = Array.Find(workload.Readings, one => one.Name == reading)
			?? throw new ArgumentException($"{id} has no reading {reading}.");
		var watch    = Stopwatch.StartNew();

		_sink = run.Run();

		Console.WriteLine(watch.Elapsed.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture));
	}

	public static void Compare(string before, string after)
	{
		var a = JsonSerializer.Deserialize<Result>(File.ReadAllText(before), Json)!;
		var b = JsonSerializer.Deserialize<Result>(File.ReadAllText(after), Json)!;

		Console.WriteLine($"before {a.Commit}, {a.Taken:yyyy-MM-dd HH:mm}, control {a.Control:F1} ns");
		Console.WriteLine($"after  {b.Commit}, {b.Taken:yyyy-MM-dd HH:mm}, control {b.Control:F1} ns ({Change(a.Control, b.Control)})");
		Console.WriteLine();
		Console.WriteLine("| row | reading | before /base | after /base | change | before B | after B |");
		Console.WriteLine("| --- | --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var row in b.Rows)
		{
			var old = Array.Find(a.Rows, one => one.Id == row.Id);

			if (old is null)
				continue;

			foreach (var reading in row.Readings.Skip(1))
			{
				var was = Array.Find(old.Readings, one => one.Reading == reading.Reading);

				if (was is null)
					continue;

				var then = was.Nanoseconds / old.Readings[0].Nanoseconds;
				var now  = reading.Nanoseconds / row.Readings[0].Nanoseconds;

				Console.WriteLine(
					$"| {row.Id} | {reading.Reading} | {then:F2}x | {now:F2}x | {Change(then, now)} | {was.Bytes:F0} | {reading.Bytes:F0} |");
			}
		}
	}

	static string Change(double before, double after)
	{
		return (after / before - 1).ToString("+0.0%;-0.0%;0.0%", CultureInfo.InvariantCulture);
	}

	// ── What is read ────────────────────────────────────────────────────────────

	static Workload[] Workloads()
	{
		var order          = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";
		var orderMalformed = order.Replace("\u000140=2\u0001", "\u000140X=2\u0001");

		return
		[
			.. Fix("One",        "55=ABC\u0001", 1, regex: true),
			.. Fix("Order",      order, 11, regex: true),
			.. Fix("BinaryMany", string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64)), 64),
			.. Fix("Orders128",  string.Concat(Enumerable.Repeat(order, 128)), 11 * 128, regex: true),

			// Q7.2 (expr-2d, 2026-09-18): a malformed field late in the message, not the
			// first one — the recovery rule catches it mid-message, past what already read.
			.. Fix("OrderMalformed", orderMalformed, 11, 1),

			// D13 (Igor, 2026-09-18): the same field repeated N times, string form only, so a
			// least-squares fit over these six points separates a parse's fixed cost from what
			// one more field adds — "initialization against a field" (performance-3f).
			//
			// A third reading, "ideal" (performance-ff, 2026-09-18): IdealFixParser, the least a
			// reader of plain text fields can do. It is an ideal, not a reference (D1), and it
			// hands anything but plain text fields to HandFixParser, so it belongs on these rows
			// alone — never on one with binary fields or errors.
			//
			// And a regular expression, twice, as "regex-lesser": the split into tag and value and
			// nothing else, so much less work than a parse that it is only a floor to look up at.
			.. FixSlopeCounts.Select(n => FixForm(
				$"slope-{n}.text",
				() => HandFixParser.Parse(FixSlopeText(n)),
				() => FixParser.Parse(FixSlopeText(n)),
				() => IdealFixParser.Parse(FixSlopeText(n)),
				FixSlopeText(n),
				(n, 0))),

			// The same fields read from bytes already in memory (performance-ff's C2, .bytes forms).
			.. FixSlopeCounts.Select(n => FixForm(
				$"slope-{n}.bytes",
				() => HandFixParser.Parse(FixSlopeBytes(n)),
				() => FixParser.Parse(FixSlopeBytes(n)),
				expected: (n, 0))),

			// And through a stream over them, the windowed form (performance-ff's buffered reader with recover).
			.. FixSlopeCounts.Select(n => FixForm(
				$"slope-{n}.stream",
				() => HandFixParser.Parse(new MemoryStream(FixSlopeBytes(n), false)),
				() => FixParser.Parse(new MemoryStream(FixSlopeBytes(n), false)),
				expected: (n, 0))),

			// The web's formats (architect for Igor, 2026-09-18): generated, and a regular
			// expression where one can be written honestly. Their base is the generated reading.
			.. WebWorkloads(),

			// T-SQL against ScriptDom, which is the base of these rows (architect for Igor, 2026-09-18).
			.. TsqlWorkloads(),
			Tsql("comment", SqlWithComments),

			// A stock count, hand and generated (finance-24, for performance-ff's C2).
			.. FeedWorkloads(),

			// The FIX message layer: Parse and Build of a NewOrderSingle, generated alone.
			.. FixMessageWorkloads(),

			Expression("floor",         "(int x) => x"),
			Expression("ladder",        "(int x, int y) => (x + y) * 3 - x / 5"),
			Expression("nest7",         "(int x) => (((((((x)))))))"),
			Expression("block",         "(int x) => { x += 1; x *= 2; return x; }"),
			Expression("try",           "using System; (int n) => { int r = 0; try { r = 10 / n; } catch (DivideByZeroException e) { r = -1; } catch (Exception e) { r = -2; } r }"),
			Expression("loop",          "(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }"),
			Expression("overloads",     "(int x) => System.Math.Max(x, 1)"),
			Expression("string",        "(int x) => \"a plain string literal, with no escape in it\""),
			Expression("interpolation", "(int x) => $\"{x,5:D3} and {x + 1}\""),
			Expression("untyped",       "using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()"),

			// Q7.2: refused, not merely a construct not yet handled — an incomplete
			// expression, early and late, so the fast/quiet path's cost on a refusal is on
			// the record before and after it starts skipping what an accepted reading needs.
			Expression("refused-early", "(int x) => x +"),
			Expression("refused-late",  "(int x) => x + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10 +"),

			Sql<Ast.LiteralValue, Ast.LiteralValue>("literal", "1", SqlStandardParser.TryParseLiteral, HandSqlStandard.TryParseLiteral),
			Sql<Ast.Expression, Ast.Expression>("column", "a.b.c", SqlStandardParser.TryParseColumnReference, HandSqlStandard.TryParseColumnReference),
			Sql<Ast.Expression, Ast.Expression>("arithmetic", "(a + b) * c - d / 5", SqlStandardParser.TryParseValueExpression, HandSqlStandard.TryParseValueExpression),
			Sql<Ast.Expression, Ast.Expression>("nest8", "((((((((a))))))))", SqlStandardParser.TryParseValueExpression, HandSqlStandard.TryParseValueExpression),
			Sql<Ast.Expression, Ast.Expression>("condition", "x = 1 AND y IS NOT NULL OR z BETWEEN 1 AND 2", SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("select1", "SELECT a FROM t", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("select20", "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("values", "VALUES (1)", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("comment", SqlWithComments, SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Expression, Ast.Expression>("conditions100", SqlConditions(100), SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
			Sql<Ast.Expression, Ast.Expression>("conditions1000", SqlConditions(1000), SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
			Sql<Ast.Statement, Ast.Statement>("create", "CREATE TABLE t (a INT NOT NULL, b VARCHAR(20) DEFAULT 'x', PRIMARY KEY (a))", SqlStandardParser.TryParseSQLSchemaStatement, HandSqlStandard.TryParseSQLSchemaStatement),

			// Q7.2: a select refused near its end, not at the first token — HandSqlStandard's
			// TryParse exposes no position, so agreement here is accept/refuse only (expr-2d,
			// 2026-09-18); a row where either side accepts is still a disagreement.
			SqlRefused<Ast.Statement.Select, Ast.Statement.Select>(
				"refused-late", "SELECT a, b, c FROM t WHERE a = 1 AND b = 2 AND c = ", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
		];
	}

	/// <summary>The field counts D13's slope rows fit a line over.</summary>
	/// <summary>
	/// A query with a line comment after each part, the comments long enough to be most of the text:
	/// what a lexer's search for the end of a comment, or a loop over its characters, is timed on.
	/// </summary>
	const string SqlWithComments =
		"SELECT a, -- the first column, which carries the name of the customer and is read on every row of the report below\n" +
		"b, -- the second column, which carries the total of the order the customer placed last, in the currency of the ledger\n" +
		"c\n" +
		"FROM t -- the table of orders, one row for each order line and one for each shipment made against the line\n" +
		"WHERE a = 1 -- only the orders of the year still open in the ledger, and none that were cancelled after they shipped";

	/// <summary>
	/// A search condition of <paramref name="predicates"/> predicates joined by AND: the input on which the
	/// SQL:2023 parser was quadratic in time and in allocation (164 MB a call at a thousand), which no row
	/// of the stand held enough of a list to show.
	/// </summary>
	static string SqlConditions(int predicates) => string.Join(" AND ", Enumerable.Range(0, predicates).Select(static i => "a" + i + " = 1"));

	static readonly int[] FixSlopeCounts = [0, 1, 2, 4, 8, 16];

	static byte[] FixSlopeBytes(int fields) => Encoding.Latin1.GetBytes(FixSlopeText(fields));

	static string FixSlopeText(int fields) => string.Concat(Enumerable.Repeat("55=ABC\u0001", fields));

	/// <summary>
	/// One FIX input three ways: a string, bytes already in memory, and a stream read lazily.
	/// </summary>
	static IEnumerable<Workload> Fix(string name, string text, int fields, int invalid = 0, bool regex = false)
	{
		var bytes = Encoding.Latin1.GetBytes(text);

		yield return FixForm(name + ".text",
			() => HandFixParser.Parse(text),
			() => FixParser.Parse(text),
			regexText: regex ? text : null,
			expected: (fields, invalid));

		yield return FixForm(name + ".bytes",
			() => HandFixParser.Parse(bytes),
			() => FixParser.Parse(bytes),
			expected: (fields, invalid));

		yield return FixForm(name + ".stream",
			() => HandFixParser.Parse(new MemoryStream(bytes, false)),
			() => FixParser.Parse(new MemoryStream(bytes, false)),
			expected: (fields, invalid));
	}

	/// <summary>
	/// What the input of a FIX row says of itself, held to each reading on its own: how many fields it has and
	/// how many of them are invalid. Two readings agreeing that an input has no fields at all proved nothing.
	/// </summary>
	static string? FixNotWhatItSays(string name, (int Fields, int Invalid) expected, (string Reading, Func<IEnumerable<FixField>> Read)[] readings)
	{
		foreach (var (reading, read) in readings)
		{
			var fields  = read().ToArray();
			var invalid = fields.Count(static one => one is FixField.Invalid);

			if (fields.Length != expected.Fields || invalid != expected.Invalid)
				return $"  {name}: the {reading} reading finds {fields.Length} fields, {invalid} invalid, and the row says {expected.Fields}, {expected.Invalid}";
		}

		return null;
	}

	static Workload FixForm(
		string name,
		Func<IEnumerable<FixField>> hand,
		Func<IEnumerable<FixField>> generated,
		Func<IEnumerable<FixField>>? ideal = null,
		string? regexText = null,
		(int Fields, int Invalid)? expected = null)
	{
		var readings = new List<Reading>
		{
			new("hand", () => Count(hand())),
			new("generated", () => Count(generated())),
		};

		if (ideal is not null)
			readings.Add(new Reading("ideal", () => Count(ideal())));

		if (regexText is not null)
		{
			readings.Add(new Reading("regex-lesser", () => FixRegex(FixSplit.Interpreted.Value, regexText)));
			readings.Add(new Reading("regex-compiled-lesser", () => FixRegex(FixSplit.Compiled.Value, regexText)));
		}

		return new Workload(
			"fix",
			name,
			[.. readings],
			() => Differ(hand().Select(Describe), generated().Select(Describe))
				?? (ideal is null ? null : Differ(hand().Select(Describe), ideal().Select(Describe))?.Replace("generated", "ideal    "))
				?? (regexText is null ? null : FixRegexDisagreement(regexText, hand(), FixSplit.Compiled.Value))
				?? (expected is null ? null : FixNotWhatItSays(name, expected.Value, [("hand", hand), ("generated", generated), .. ideal is null ? Array.Empty<(string, Func<IEnumerable<FixField>>)>() : [("ideal", ideal)]])));

		static int Count(IEnumerable<FixField> fields)
		{
			var count = 0;

			foreach (var field in fields)
				count += field.Tag;

			return count;
		}

		static string Describe(FixField field)
		{
			return field is FixField.Invalid invalid
				? $"Invalid:{invalid.Position}:{invalid.Length}:{invalid.RawText}"
				: field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
		}
	}

	static readonly System.Reflection.Assembly Caller = typeof(Stand).Assembly;

	/// <summary>
	/// One expression, read by hand, on the tape and, where it can be, by the immediate carrier.
	/// </summary>
	/// <remarks>
	/// Every reading gets a state of its own, as a caller's parse would, and pays for it alike.
	/// Since performance-3f's item c (140f07e4, 2026-09-18), the immediate carrier reads an
	/// untyped lambda the way the tape does; before that it built the body ahead of its types
	/// and threw where the tape read (D3, D8), which is why <paramref name="immediate"/> still
	/// exists for a row that needs to leave it out again.
	/// </remarks>
	static Workload Expression(string name, string text, bool immediate = true)
	{
		var readings = new List<Reading>
		{
			new("hand", () => Read(HandExpression.TryParseLambda)),
			new("tape", () => Read(ExpressionParser.TryParseLambda)),
		};

		if (immediate)
			readings.Add(new Reading("immediate", () => Read(ExpressionParser.Immediate.TryParseLambda)));

		return new Workload("el", name, [.. readings], Disagreement);

		int Read(Func<string, ExpressionParser.State, ExpressionParser.Match<System.Linq.Expressions.LambdaExpression>> read)
		{
			return read(text, new ExpressionParser.State(Caller) { Text = text }).IsSuccess ? 1 : 0;
		}

		string? Disagreement()
		{
			var hand = ExpressionCorpus.Answer(text, HandExpression.TryParseLambda, new ExpressionParser.State(Caller) { Text = text });
			var tape = ExpressionCorpus.Answer(text, ExpressionParser.TryParseLambda, new ExpressionParser.State(Caller) { Text = text });

			if (hand != tape)
				return $"  hand {hand}\n  tape {tape}";

			if (!immediate)
				return null;

			var eager = ExpressionCorpus.Answer(text, ExpressionParser.Immediate.TryParseLambda, new ExpressionParser.State(Caller) { Text = text });

			return eager == tape ? null : $"  tape      {tape}\n  immediate {eager}";
		}
	}

	delegate bool HandRead<T>(string input, out T value);

	/// <summary>
	/// One SQL:2023 production over one input, by hand and by the generated parser, which must
	/// also accept it: a row that times two refusals says nothing about reading SQL.
	/// </summary>
	static Workload Sql<TGenerated, THand>(
		string name, string text, Func<string, SqlStandardParser.Match<TGenerated>> generated, HandRead<THand> hand)
	{
		return new Workload(
			"sql",
			name,
			[
				new Reading("hand",      () => hand(text, out _) ? 1 : 0),
				new Reading("generated", () => generated(text).IsSuccess ? 1 : 0),
			],
			Disagreement);

		string? Disagreement()
		{
			var match = generated(text);

			if (!match.IsSuccess)
				return "  the generated parser refuses it";

			if (!hand(text, out var value))
				return "  the hand-written parser refuses it";

			var expected = Standard.Dump(match.Value);
			var actual   = Standard.Dump(value);

			return expected == actual ? null : $"  generated {expected}\n  by hand   {actual}";
		}
	}

	/// <summary>
	/// One SQL:2023 production over an input both sides must refuse. HandSqlStandard's
	/// <c>TryParse</c> exposes no position or diagnostic to check against (Q7.2, expr-2d,
	/// 2026-09-18), so agreement here is accept/refuse alone — a row where either side
	/// accepts is a disagreement, the same as <see cref="Sql{TGenerated, THand}"/> the other
	/// way around.
	/// </summary>
	static Workload SqlRefused<TGenerated, THand>(
		string name, string text, Func<string, SqlStandardParser.Match<TGenerated>> generated, HandRead<THand> hand)
	{
		return new Workload(
			"sql",
			name,
			[
				new Reading("hand",      () => hand(text, out _) ? 1 : 0),
				new Reading("generated", () => generated(text).IsSuccess ? 1 : 0),
			],
			Disagreement);

		string? Disagreement()
		{
			var byGenerated = generated(text).IsSuccess;
			var byHand      = hand(text, out _);

			return byGenerated || byHand
				? $"  expected both to refuse; generated {(byGenerated ? "accepted" : "refused")}, hand {(byHand ? "accepted" : "refused")}"
				: null;
		}
	}

	// ── Paired: two builds, one process ─────────────────────────────────────────

	/// <summary>
	/// One build's generated parsers, loaded from its own directory into an isolated
	/// <see cref="AssemblyLoadContext"/> so two builds' same-named types can coexist in one
	/// process and be timed alternating, round-robin (2026-09-18, architect, after this
	/// caught a real regression a two-process comparison read as noise: Q7.3's Fix44 stream
	/// form). Reflection touches only the generated side; the hand-written side stays this
	/// process's own statically-referenced one, since a paired compare targets a generator
	/// change and the hand parsers do not move with it (D1).
	/// </summary>
	sealed class PairedSide
	{
		readonly Type _sql;
		readonly Type _elTape;
		readonly Type _elImmediate;
		readonly Type _elState;
		readonly Type _fix;
		readonly Type _fixOptions;
		readonly Type _fixMessages;
		readonly Type? _fixParseMode;
		readonly Type? _fixParseOptions;
		readonly Type? _stock;
		readonly Type? _uri;
		readonly Type _tsql;
		readonly Type? _json;
		readonly Type? _config;

		/// <summary>Whether the side was given DotGram.Benchmarks.dll, and so has the document grammar to read.</summary>
		public bool HasConfig => _config is not null;

		/// <summary>Whether the side was given DotGram.Examples, and so has a stock count to read.</summary>
		public bool HasStock => _stock is not null;

		/// <summary>Whether the side was given DotGram.Web, and so has URLs and JSON to read.</summary>
		public bool HasWeb => _uri is not null;

		public PairedSide(string name, string directory)
		{
			directory = Path.GetFullPath(directory);

			var alc = new AssemblyLoadContext(name, isCollectible: false);

			alc.Resolving += (context, requested) =>
			{
				var path = Path.Combine(directory, requested.Name + ".dll");

				return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
			};

			Type Load(string assembly, string type) =>
				alc.LoadFromAssemblyPath(Path.Combine(directory, assembly + ".dll")).GetType(type)
					?? throw new InvalidOperationException($"{name}: {type} not found in {assembly}");

			_sql         = Load("DotGram.Sql", "DotGram.Sql.Standard.SqlStandardParser");
			_tsql        = Load("DotGram.Sql", "DotGram.Sql.TransactSql.TransactSqlParser");
			_elTape      = Load("DotGram.ExpressionLanguage", "DotGram.ExpressionLanguage.ExpressionParser");
			_elImmediate = Load("DotGram.ExpressionLanguage", "DotGram.ExpressionLanguage.ExpressionParser+Immediate");
			_elState     = Load("DotGram.ExpressionLanguage", "DotGram.ExpressionLanguage.ExpressionParser+State");
			_fix         = Load("DotGram.Finance", "DotGram.Finance.Fix.FixParser");
			_fixOptions  = Load("DotGram.Finance", "DotGram.Finance.Fix.FixFieldOptions");
			_fixMessages = Load("DotGram.Finance", "DotGram.Finance.Fix.FixMessages");
			// FixParseMode is gone from a side after D53/D69 (the schema check moved to FixMessage.Validate()): a side may have it, or the options, or neither.
			var finance = alc.LoadFromAssemblyPath(Path.Combine(directory, "DotGram.Finance.dll"));

			_fixParseMode    = finance.GetType("DotGram.Finance.Fix.FixParseMode");
			_fixParseOptions = finance.GetType("DotGram.Finance.Fix.FixParseOptions");

			// Only a side that was given DotGram.Web has URLs and JSON to read.
			if (File.Exists(Path.Combine(directory, "DotGram.Web.dll")))
			{
				_uri  = Load("DotGram.Web", "DotGram.Web.UriReference");
				_json = Load("DotGram.Web", "DotGram.Web.JsonValue");
			}

			// Only a side that was given DotGram.Benchmarks has the document grammar (Documents.cs) to read.
			_config = File.Exists(Path.Combine(directory, "DotGram.Benchmarks.dll"))
				? Load("DotGram.Benchmarks", "DotGram.Benchmarks.Config")
				: null;

			// Only a side that was given DotGram.Examples has a stock count to read.
			_stock = File.Exists(Path.Combine(directory, "DotGram.Examples.dll"))
				? Load("DotGram.Examples", "DotGram.Examples.Feeds.StockCountReader")
				: null;
		}

		public Func<int> Sql(string method, string text)
		{
			var call = _sql.GetMethod(method, [typeof(string)])
				?? throw new InvalidOperationException($"SqlStandardParser.{method}(string) not found");

			return () => IsSuccess(call.Invoke(null, [text])!);
		}

		/// <summary>
		/// The quiet form of this side's SQL:2023 parser, <c>bool Try{Rule}(string, out value)</c> (expr's dba87a9e), by
		/// reflection; null where the side has none.
		/// </summary>
		public Func<int>? SqlBool(string rule, string text)
		{
			var call = _sql.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(one => one.Name == rule
				&& one.ReturnType == typeof(bool) && one.GetParameters() is [{ ParameterType.Name: "String" }, { IsOut: true }]);

			return call is null ? null : () => (bool)call.Invoke(null, [text, null])! ? 1 : 0;
		}

		/// <summary>The quiet form of this side's expression parser, <c>bool TryParseLambda(string, State, out value)</c>, by reflection; null where the side has none.</summary>
		public Func<int>? ElBool(string rule, string text, bool immediate)
		{
			var type = immediate ? _elImmediate : _elTape;
			var call = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(one => one.Name == rule
				&& one.ReturnType == typeof(bool) && one.GetParameters() is [{ ParameterType.Name: "String" }, var state, { IsOut: true }] && state.ParameterType == _elState);

			if (call is null)
				return null;

			var ctor = _elState.GetConstructor([typeof(Assembly)]) ?? throw new InvalidOperationException("ExpressionParser.State(Assembly) not found");
			var textProperty = _elState.GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				?? throw new InvalidOperationException("ExpressionParser.State.Text not found");

			return () =>
			{
				var state = ctor.Invoke([typeof(Stand).Assembly]);

				textProperty.SetValue(state, text);

				return (bool)call.Invoke(null, [text, state, null])! ? 1 : 0;
			};
		}

		/// <summary>
		/// A script read a statement at a time, <c>TransactSqlParser.TryParseStatement(string, int at)</c> in a loop from the end
		/// of the last match: how many statements it read. The positional form tokenizes the whole input on every call.
		/// </summary>
		public Func<int> TsqlScript(string text)
		{
			var call = _tsql.GetMethod("TryParseStatement", [typeof(string), typeof(int)])
				?? throw new InvalidOperationException("TransactSqlParser.TryParseStatement(string, int) not found");
			PropertyInfo? success = null, position = null, length = null;

			return () =>
			{
				var at    = 0;
				var count = 0;

				while (at < text.Length)
				{
					var match = call.Invoke(null, [text, at])!;

					success  ??= match.GetType().GetProperty("IsSuccess");
					position ??= match.GetType().GetProperty("Position");
					length   ??= match.GetType().GetProperty("Length");

					if (!(bool)success!.GetValue(match)!)
						break;

					count++;
					at = Convert.ToInt32(position!.GetValue(match)) + Convert.ToInt32(length!.GetValue(match));

					while (at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == ';'))
						at++;
				}

				return count;
			};
		}

		/// <summary>
		/// The same script read through the bool positional form, <c>bool TryParseStatement(string input, ref int at, out Statement value)</c>:
		/// null where the side has none. <c>at</c> is moved to the end of what was read.
		/// </summary>
		public Func<int>? TsqlScriptBool(string text)
		{
			var call = _tsql.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(one => one.Name == "TryParseStatement"
				&& one.ReturnType == typeof(bool) && one.GetParameters() is [{ ParameterType.Name: "String" }, { ParameterType.IsByRef: true, ParameterType.Name: "Int32&" }, { IsOut: true }]);

			if (call is null)
				return null;

			return () =>
			{
				var at    = 0;
				var count = 0;

				while (at < text.Length)
				{
					var arguments = new object?[] { text, at, null };

					if (!(bool)call.Invoke(null, arguments)!)
						break;

					count++;
					at = (int)arguments[1]!;

					while (at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == ';'))
						at++;
				}

				return count;
			};
		}

		/// <summary>
		/// The positional <c>(string, at)</c> or window <c>(string, at, length)</c> form of a rule of the SQL:2023 or the T-SQL parser, by
		/// reflection: whether it read the statement that begins at <paramref name="at"/>.
		/// </summary>
		public Func<int> SqlPositional(bool tsql, string method, string text, int at, int? length)
		{
			var call = (tsql ? _tsql : _sql).GetMethod(method, length is null ? [typeof(string), typeof(int)] : [typeof(string), typeof(int), typeof(int)])
				?? throw new InvalidOperationException($"{method}(string, int{(length is null ? "" : ", int")}) not found");
			object[] arguments = length is null ? [text, at] : [text, at, length.Value];

			return () => IsSuccess(call.Invoke(null, arguments)!);
		}

		/// <summary>The token scanner of a parser ("sql", "tsql" or "el"), looped over a text: how many tokens it found.</summary>
		public Func<int> Scan(string parser, string text)
		{
			var type   = parser switch { "tsql" => _tsql, "el" => _elTape, _ => _sql };
			var method = type.GetMethod("Scan", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				?? throw new InvalidOperationException($"{type.Name}.Scan not found");
			var scan   = (ScanFunction)Delegate.CreateDelegate(typeof(ScanFunction), method);

			return () => ScanCount(scan, text);
		}

		/// <summary>
		/// A form of <c>FixMessages</c> over one strict wire message: "parse-stream" and "parse-reader" (<c>Parse(Stream | TextReader, mode, size)</c>),
		/// "read-stream" and "read-reader" (<c>ReadMessages</c>, lazy, over <paramref name="messages"/> messages), and "parse-span"
		/// (<c>Parse(ReadOnlySpan&lt;char&gt;, mode)</c>, through a dynamic method, since a span cannot be handed to <c>Invoke</c>).
		/// </summary>
		public Func<int> FixMessagesForm(string form, string wire, int messages)
		{
			var text = string.Concat(Enumerable.Repeat(wire, messages));
			var one  = Encoding.Latin1.GetBytes(wire);
			var many = Encoding.Latin1.GetBytes(text);

			if (form == "parse-span")
			{
				var (target, withMode) = FixMessagesEntry("Parse", [typeof(ReadOnlySpan<char>)]);
				var call         = SpanCall(target);
				var validateSpan = withMode ? null : FixValidate();

				return () => Checked(call(wire), validateSpan) is null ? 0 : 1;
			}

			var parse = form.StartsWith("parse", StringComparison.Ordinal);
			var input = form.EndsWith("stream", StringComparison.Ordinal) ? typeof(Stream) : typeof(TextReader);
			var (entry, mode) = FixMessagesEntry(parse ? "Parse" : "ReadMessages", [input, typeof(int)]);
			var strict   = mode ? FixStrictMode() : null;
			var validate = mode ? null : FixValidate();

			return () =>
			{
				var source = input == typeof(Stream) ? (object)new MemoryStream(parse ? one : many, false) : new StringReader(parse ? wire : text);
				var result = entry.Invoke(null, mode ? [source, strict, 4096] : [source, 4096]);

				if (parse)
					return Checked(result, validate) is null ? 0 : 1;

				var count = 0;

				foreach (var message in (IEnumerable)result!)
				{
					Checked(message, validate);
					count++;
				}

				return count;
			};
		}

		/// <summary>
		/// The entry of <c>FixMessages</c> named <paramref name="name"/> taking <paramref name="plain"/>: where this side still has <c>FixParseMode</c> (before D53/D69) the one that takes
		/// the mode after its first parameter, which is what those sides always measured, and where it does not the one without it.
		/// </summary>
		(MethodInfo Method, bool WithMode) FixMessagesEntry(string name, Type[] plain)
		{
			if (_fixParseMode is not null)
			{
				var withMode = new Type[plain.Length + 1];

				withMode[0] = plain[0];
				withMode[1] = _fixParseMode;

				Array.Copy(plain, 1, withMode, 2, plain.Length - 1);

				if (_fixMessages.GetMethod(name, withMode) is { } found)
					return (found, true);
			}

			return (_fixMessages.GetMethod(name, plain) ?? throw new InvalidOperationException($"FixMessages.{name}({string.Join(", ", plain.Select(static one => one.Name))}) not found, with or without FixParseMode"), false);
		}

		/// <summary>Strict, the first member of the enum: what the old sides were always asked for.</summary>
		object FixStrictMode() => Enum.ToObject(_fixParseMode!, 0);

		/// <summary>
		/// On a side with no <c>FixParseMode</c> the strict parse is gone and its schema check is <c>FixMessage.Validate()</c>: the nearest work to what the other side does in one call
		/// is the parse and then this, so that is what a row of the message layer measures there (the header of a pair says so). Null where a side has none.
		/// </summary>
		Func<object, object?>? FixValidate()
		{
			var message  = _fixMessages.Assembly.GetType("DotGram.Finance.Fix.FixMessage");
			var validate = message?.GetMethod("Validate", Type.EmptyTypes);

			if (validate is null)
				return null;

			var parameter = System.Linq.Expressions.Expression.Parameter(typeof(object));

			return System.Linq.Expressions.Expression.Lambda<Func<object, object?>>(
				System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(parameter, message!), validate), typeof(object)),
				parameter).Compile();
		}

		static object? Checked(object? message, Func<object, object?>? validate)
		{
			if (message is not null)
				validate?.Invoke(message);

			return message;
		}

		/// <summary>FixParser.Parse(ReadOnlySpan&lt;char&gt;) of this side, through a dynamic method: how many fields it read.</summary>
		public Func<int> FixSpan(string text)
		{
			var target = _fix.GetMethod("Parse", [typeof(ReadOnlySpan<char>), _fixOptions])
				?? throw new InvalidOperationException("FixParser.Parse(ReadOnlySpan<char>, FixFieldOptions) not found");
			var call   = SpanCall(target);

			return () => ((Array)call(text)!).Length;
		}

		/// <summary>StreamingFeedReader.Read(TextReader) of this side (DotGram.Examples), walked: how many parts it yielded.</summary>
		public Func<int> StreamingFeed(string text)
		{
			var owner = (_stock ?? throw new InvalidOperationException("The side has no DotGram.Examples.dll")).Assembly.GetType("DotGram.Examples.Feeds.StreamingFeedReader")
				?? throw new InvalidOperationException("StreamingFeedReader not found");
			var call  = owner.GetMethod("Read", [typeof(TextReader)]) ?? throw new InvalidOperationException("StreamingFeedReader.Read(TextReader) not found");

			return () =>
			{
				var count = 0;

				foreach (var part in (IEnumerable)call.Invoke(null, [new StringReader(text)])!)
					count++;

				return count;
			};
		}

		/// <summary>A static method taking a span of chars first, as a delegate over a string: the other parameters are null, or the enum's zero.</summary>
		static Func<string, object?> SpanCall(MethodInfo target)
		{
			var asSpan  = typeof(MemoryExtensions).GetMethod(nameof(MemoryExtensions.AsSpan), [typeof(string)])!;
			var dynamic = new System.Reflection.Emit.DynamicMethod("span", typeof(object), [typeof(string)], typeof(Stand).Module, skipVisibility: true);
			var il      = dynamic.GetILGenerator();

			il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
			il.Emit(System.Reflection.Emit.OpCodes.Call, asSpan);

			foreach (var parameter in target.GetParameters().Skip(1))
			{
				if (parameter.ParameterType.IsEnum)
					il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
				else
					il.Emit(System.Reflection.Emit.OpCodes.Ldnull);
			}

			il.Emit(System.Reflection.Emit.OpCodes.Call, target);

			if (target.ReturnType.IsValueType)
				il.Emit(System.Reflection.Emit.OpCodes.Box, target.ReturnType);

			il.Emit(System.Reflection.Emit.OpCodes.Ret);

			return (Func<string, object?>)dynamic.CreateDelegate(typeof(Func<string, object?>));
		}

		/// <summary>TransactSqlParser.TryParseStatement(string) of this side, by reflection: whether it read the statement.</summary>
		public Func<int> Tsql(string text)
		{
			var call = _tsql.GetMethod("TryParseStatement", [typeof(string)])
				?? throw new InvalidOperationException("TransactSqlParser.TryParseStatement(string) not found");

			return () => IsSuccess(call.Invoke(null, [text])!);
		}

		public Func<int> El(string method, string text, bool immediate)
		{
			var type = immediate ? _elImmediate : _elTape;
			var call = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
				null, [typeof(string), _elState], null)
				?? throw new InvalidOperationException($"{type.Name}.{method}(string, State) not found");
			var ctor = _elState.GetConstructor([typeof(Assembly)])
				?? throw new InvalidOperationException("ExpressionParser.State(Assembly) not found");
			var textProperty = _elState.GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				?? throw new InvalidOperationException("ExpressionParser.State.Text not found");

			return () =>
			{
				var state = ctor.Invoke([typeof(Stand).Assembly]);

				textProperty.SetValue(state, text);

				return IsSuccess(call.Invoke(null, [text, state])!);
			};
		}

		/// <summary>
		/// The whole-stream form of this side's FIX parser, <c>FixGrammar.ParseFields(Stream | TextReader, context)</c>, by
		/// reflection over the internal grammar class: it reads everything and returns the array.
		/// </summary>
		public Func<object, object> FixWhole(bool textReader)
		{
			var grammar = _fix.Assembly.GetType("DotGram.Finance.Fix.FixGrammar") ?? throw new InvalidOperationException("FixGrammar not found");
			var context = grammar.GetNestedType("FixContext", BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException("FixGrammar.FixContext not found");
			var options = _fixOptions.GetField("Default", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null);
			var make    = context.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [_fixOptions]) ?? throw new InvalidOperationException("FixContext(FixFieldOptions) not found");
			var method  = grammar.GetMethod("ParseFields", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
				[textReader ? typeof(TextReader) : typeof(Stream), context, typeof(int?), typeof(int?)], null)
				?? throw new InvalidOperationException("FixGrammar.ParseFields(Stream | TextReader, context, int?, int?) not found");

			return input => method.Invoke(null, [input, make.Invoke([options]), null, null])!;
		}

		/// <summary>
		/// The lazy form of this side's FIX grammar over memory, <c>FixGrammar.ReadFields(string | ReadOnlyMemory&lt;byte&gt;, context)</c>
		/// (the architect and performance-ff, 2026-09-19: over a string the yield publication is run by the engine, over a stream by
		/// methods, and the one that is read more often is the slow one), by reflection over the internal grammar class. Null where
		/// the side has no such publication.
		/// </summary>
		public Func<int>? FixYieldMemory(bool memory, byte[] bytes, string text)
		{
			var grammar = _fix.Assembly.GetType("DotGram.Finance.Fix.FixGrammar") ?? throw new InvalidOperationException("FixGrammar not found");
			var context = grammar.GetNestedType("FixContext", BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException("FixGrammar.FixContext not found");
			var options = _fixOptions.GetField("Default", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null);
			var make    = context.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [_fixOptions]) ?? throw new InvalidOperationException("FixContext(FixFieldOptions) not found");
			var method  = grammar.GetMethod("ReadFields", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null,
				[memory ? typeof(ReadOnlyMemory<byte>) : typeof(string), context], null);

			if (method is null)
				return null;

			object input = memory ? new ReadOnlyMemory<byte>(bytes) : text;

			return FixCount(() => method.Invoke(null, [input, make.Invoke([options])])!);
		}

		/// <summary>The whole-stream form (<see cref="FixWhole"/>) over the bytes or the text, its fields' tags summed as every FIX row does.</summary>
		public Func<int> FixWholeCount(bool textReader, byte[] bytes, string text)
		{
			var parse = FixWhole(textReader);

			return FixCount(() => parse(textReader ? new StringReader(text) : new MemoryStream(bytes, false)));
		}

		/// <summary><c>DotGram.Examples.{type}.{method}(string)</c> of this side, by reflection, its result left unread: 1 when it returned.</summary>
		public Func<int> Example(string type, string method, string text)
		{
			// The examples' types, and the benchmarks' own (Levels) from a side that was given DotGram.Benchmarks.dll.
			var owner = new[] { _stock, _config }.Where(static one => one is not null).SelectMany(static one => one!.Assembly.GetTypes())
				.FirstOrDefault(one => one.Name == type && one.Namespace!.StartsWith("DotGram.", StringComparison.Ordinal))
				?? throw new InvalidOperationException($"{type} not found in the side's Examples or Benchmarks");
			var call = owner.GetMethod(method, [typeof(string)])
				?? throw new InvalidOperationException($"{type}.{method}(string) not found");

			return () => call.Invoke(null, [text]) is null ? 0 : 1;
		}

		/// <summary>RecoveringFeedReader.Read(string) of this side, by reflection: how many lines it read, the ones it rejected among them.</summary>
		public Func<int> RecoveringFeed(string text)
		{
			var owner = (_stock ?? throw new InvalidOperationException("The side has no DotGram.Examples.dll")).Assembly.GetType("DotGram.Examples.Feeds.RecoveringFeedReader")
				?? throw new InvalidOperationException("RecoveringFeedReader not found");
			var call = owner.GetMethod("Read", [typeof(string)])
				?? throw new InvalidOperationException("RecoveringFeedReader.Read(string) not found");

			return () => ((System.Collections.ICollection)call.Invoke(null, [text])!).Count;
		}

		/// <summary>Config.Read(string) of this side, by reflection: how many settings it read.</summary>
		public Func<int> ConfigRead(string text)
		{
			var call = (_config ?? throw new InvalidOperationException("The side has no DotGram.Benchmarks.dll")).GetMethod("Read", [typeof(string)])
				?? throw new InvalidOperationException("Config.Read(string) not found");

			return () => ((Array)call.Invoke(null, [text])!).Length;
		}

		/// <summary>UriReference.TryParse(string, out UriReference) of this side, by reflection: whether it read the text.</summary>
		public Func<int> WebUrl(string text) => WebTryParse(_uri, "UriReference", text);

		/// <summary>
		/// <c>DotGram.Web.{type}.{method}(string, out ...)</c> of this side, by reflection: whether it read the text.
		/// </summary>
		public Func<int> WebTry(string type, string method, string text)
		{
			var owner = (_uri ?? throw new InvalidOperationException("The side has no DotGram.Web.dll")).Assembly.GetType("DotGram.Web." + type)
				?? throw new InvalidOperationException($"DotGram.Web.{type} not found");
			var call = owner.GetMethods()
				.FirstOrDefault(one => one.Name == method && one.GetParameters() is [{ ParameterType.Name: "String" }, { IsOut: true }]);

			if (call is not null)
				return () => (bool)call.Invoke(null, [text, null])! ? 1 : 0;

			// A format with no non-throwing text form (JSON Patch): its Parse, refused when it throws.
			var parse = owner.GetMethod(method, [typeof(string)]) ?? throw new InvalidOperationException($"DotGram.Web.{type}.{method}(string) not found");

			return () =>
			{
				try
				{
					return parse.Invoke(null, [text]) is null ? 0 : 1;
				}
				catch (TargetInvocationException)
				{
					return 0;
				}
			};
		}

		/// <summary>JsonValue.TryParse(string, out JsonValue) of this side, by reflection: whether it read the text.</summary>
		public Func<int> WebJson(string text) => WebTryParse(_json, "JsonValue", text);

		static Func<int> WebTryParse(Type? type, string name, string text)
		{
			var call = (type ?? throw new InvalidOperationException("The side has no DotGram.Web.dll")).GetMethods()
				.First(one => one.Name == "TryParse" && one.GetParameters() is [{ ParameterType.Name: "String" }, { IsOut: true }])
				?? throw new InvalidOperationException($"{name}.TryParse(string, out) not found");

			return () => (bool)call.Invoke(null, [text, null])! ? 1 : 0;
		}

		/// <summary>StockCountReader.TryParseCount(string) of this side, by reflection: the match it returns.</summary>
		public Func<object> StockText(string text)
		{
			var call = (_stock ?? throw new InvalidOperationException("The side has no DotGram.Examples.dll")).GetMethod("TryParseCount", [typeof(string)])
				?? throw new InvalidOperationException("StockCountReader.TryParseCount(string) not found");

			return () => call.Invoke(null, [text])!;
		}

		/// <summary>StockCountReader.TryParseCount(TextReader, int?, int?) over a StringReader, with the buffer size given (null: the default).</summary>
		public Func<object> StockReader(string text, int? bufferSize)
		{
			var call = (_stock ?? throw new InvalidOperationException("The side has no DotGram.Examples.dll")).GetMethod("TryParseCount", [typeof(TextReader), typeof(int?), typeof(int?)])
				?? throw new InvalidOperationException("StockCountReader.TryParseCount(TextReader, int?, int?) not found");

			return () => call.Invoke(null, [new StringReader(text), bufferSize, null])!;
		}

		/// <summary>Whether a match was a success, and the total and the number of lines of what it read.</summary>
		public static (bool Ok, int Total, int Lines) StockSummary(object match)
		{
			var type = match.GetType();

			if (!(bool)type.GetProperty("IsSuccess")!.GetValue(match)!)
				return (false, 0, 0);

			var value = type.GetProperty("Value")!.GetValue(match)!;
			var lines = (System.Collections.ICollection)value.GetType().GetProperty("Lines")!.GetValue(value)!;

			return (true, (int)value.GetType().GetProperty("Total")!.GetValue(value)!, lines.Count);
		}

		/// <summary>FixMessages.Parse of a wire message, strict, by reflection: what it read, as 1.</summary>
		public Func<int> FixMessageParse(string wire)
		{
			var (call, mode) = FixMessagesEntry("Parse", [typeof(string)]);
			var strict   = mode ? FixStrictMode() : null;
			var validate = mode ? null : FixValidate();

			return () => Checked(call.Invoke(null, mode ? [wire, strict] : [wire]), validate) is null ? 0 : 1;
		}

		/// <summary>FixMessages.Build over the fields this side's FixParser reads from the wire, by reflection.</summary>
		public Func<int> FixMessageBuild(string wire)
		{
			var fields = FixCall("Parse", [typeof(string), _fixOptions], [wire, null]);
			var array  = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!.MakeGenericMethod(_fix.Assembly.GetType("DotGram.Finance.Fix.FixField")!);
			var build  = (_fixParseOptions is null ? null : _fixMessages.GetMethod("Build", [typeof(string), array.ReturnType, _fixParseOptions]))
				?? _fixMessages.GetMethod("Build", [typeof(string), array.ReturnType])
				?? throw new InvalidOperationException("FixMessages.Build(string, FixField[]) or (string, FixField[], FixParseOptions) not found");
			var withOptions = build.GetParameters().Length == 3;

			return () => build.Invoke(null, withOptions ? [wire, array.Invoke(null, [fields()]), null] : [wire, array.Invoke(null, [fields()])]) is null ? 0 : 1;
		}

		/// <summary>FixParser.ParseLog (the `|`-separated log form) of this side: "text", "bytes", or "stream" (the yield form over a stream).</summary>
		public Func<int> FixLog(string form, string text)
		{
			var bytes = Encoding.Latin1.GetBytes(text);

			return form switch
			{
				"bytes"  => FixCount(FixCall("ParseLog", [typeof(byte[]), _fixOptions], [bytes, null])),
				"stream" => FixCount(() => FixCall("ParseLog", [typeof(Stream), _fixOptions, typeof(int), typeof(int?)], [new MemoryStream(bytes, false), null, 4096, null])()),
				_        => FixCount(FixCall("ParseLog", [typeof(string), _fixOptions], [text, null])),
			};
		}

		public Func<int> FixText(string text) => FixCount(FixCall("Parse", [typeof(string), _fixOptions], [text, null]));
		public Func<int> FixBytes(byte[] bytes) => FixCount(FixCall("Parse", [typeof(byte[]), _fixOptions], [bytes, null]));

		/// <summary>FixParser.Parse(TextReader) of this side, the yield form over a reader: the fields' tags summed.</summary>
		public IEnumerable<object> FixYieldReaderFields(TextReader input, int? maxRetained = null) =>
			(IEnumerable<object>)((IEnumerable)FixCall("Parse", [typeof(TextReader), _fixOptions, typeof(int), typeof(int?)], [input, null, 4096, maxRetained])()).Cast<object>();

		public Func<int> FixYieldReader(string text) => FixCount(() =>
			FixCall("Parse", [typeof(TextReader), _fixOptions, typeof(int), typeof(int?)], [new StringReader(text), null, 4096, null])());

		/// <summary>
		/// Every field of the text read by this side's FIX parser, serialized: what it built, not how many it found (the critic, 2026-09-19).
		/// <paramref name="form"/> is "text", "bytes" or "stream".
		/// </summary>
		public string[] FixFields(string form, string text)
		{
			var bytes  = Encoding.Latin1.GetBytes(text);
			var fields = form switch
			{
				"bytes"  => FixCall("Parse", [typeof(byte[]), _fixOptions], [bytes, null])(),
				"stream" => FixCall("Parse", [typeof(Stream), _fixOptions, typeof(int), typeof(int?)], [new MemoryStream(bytes, false), null, 4096, null])(),
				_        => FixCall("Parse", [typeof(string), _fixOptions], [text, null])(),
			};

			return [.. ((IEnumerable)fields).Cast<object>().Select(DescribeFixField)];
		}

		Func<int> FixCountPlaceholder() => () => 0;

		public Func<int> FixStream(byte[] bytes) => FixCount(() =>
			FixCall("Parse", [typeof(Stream), _fixOptions, typeof(int), typeof(int?)], [new MemoryStream(bytes, false), null, 4096, null])());

		/// <summary>The stream form of this side, as the lazy sequence it returns, for a held-memory reading.</summary>
		public IEnumerable FixStreamFields(Stream stream, int? maxRetained = null) =>
			(IEnumerable)FixCall("Parse", [typeof(Stream), _fixOptions, typeof(int), typeof(int?)], [stream, null, 4096, maxRetained])();

		Func<object> FixCall(string method, Type[] parameters, object?[] arguments)
		{
			var call = _fix.GetMethod(method, parameters)
				?? throw new InvalidOperationException($"FixParser.{method} not found for the given parameters");

			return () => call.Invoke(null, arguments)!;
		}

		/// <summary>
		/// Sums the fields' tags, the same reading <see cref="FixForm"/> takes — by reflection,
		/// since the field is this side's own <c>FixField</c>, not the process's.
		/// </summary>
		static Func<int> FixCount(Func<object> read)
		{
			PropertyInfo? tagProperty = null;

			return () =>
			{
				var count = 0;

				foreach (var field in (IEnumerable)read())
				{
					tagProperty ??= field.GetType().GetProperty("Tag") ?? throw new InvalidOperationException("FixField.Tag not found");
					count += (int)tagProperty.GetValue(field)!;
				}

				return count;
			};
		}

		static int IsSuccess(object match)
		{
			var property = match.GetType().GetProperty("IsSuccess") ?? throw new InvalidOperationException("Match<T>.IsSuccess not found");

			return (bool)property.GetValue(match)! ? 1 : 0;
		}
	}

	static bool HandAccepts(string method, string text)
	{
		var call = typeof(HandSqlStandard).GetMethod(method, BindingFlags.Static | BindingFlags.Public)
			?? throw new InvalidOperationException($"HandSqlStandard.{method} not found");

		return (bool)call.Invoke(null, [text, null])!;
	}

	/// <summary>
	/// The same rows <see cref="Workloads"/> times, read from two builds instead of one
	/// process's own — same texts (kept in both places; update together), same families.
	/// SQL's agreement drops the tree comparison <see cref="Sql{TGenerated, THand}"/> makes
	/// (an AST type from one ALC is not the AST type from another): accept/refuse against
	/// this process's own hand parser is what both sides are held to here, same as
	/// <see cref="SqlRefused{TGenerated, THand}"/> already settles for a refusal.
	/// </summary>
	static Workload[] PairedWorkloads(PairedSide before, PairedSide after)
	{
		var order          = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";
		var orderMalformed = order.Replace("\u000140=2\u0001", "\u000140X=2\u0001");
		var binaryMany     = string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64));
		var orders128      = string.Concat(Enumerable.Repeat(order, 128));

		return
		[
			.. PairedFix("One", "55=ABC\u0001", before, after),
			.. PairedFix("Order", order, before, after),
			.. PairedFix("BinaryMany", binaryMany, before, after),
			.. PairedFix("Orders128", orders128, before, after),
			.. PairedFix("OrderMalformed", orderMalformed, before, after),

			// D13: same rows as Workloads(), text and bytes forms.
			.. FixSlopeCounts.Select(n => PairedFixForm(
				$"slope-{n}.text",
				() => HandFixParser.Parse(FixSlopeText(n)),
				before.FixText(FixSlopeText(n)),
				after.FixText(FixSlopeText(n)))),

			.. FixSlopeCounts.Select(n => PairedFixForm(
				$"slope-{n}.bytes",
				() => HandFixParser.Parse(FixSlopeBytes(n)),
				before.FixBytes(FixSlopeBytes(n)),
				after.FixBytes(FixSlopeBytes(n)))),

			.. FixSlopeCounts.Select(n => PairedFixForm(
				$"slope-{n}.stream",
				() => HandFixParser.Parse(new MemoryStream(FixSlopeBytes(n), false)),
				before.FixStream(FixSlopeBytes(n)),
				after.FixStream(FixSlopeBytes(n)))),

			.. PairedFixMessages(before, after),

			// The largest size of each linearity series, held before and after like any row: a change that makes a parser
			// superlinear shows here, in the pair of the commit that does it, and not a day later in `linearity`.
			.. PairedSweeps(before, after),

			// The published forms no row read (docs/design/stand-coverage-2026-09-19.md): positional and window, the token scanner, FixMessages'
			// streams, readers and lazy reading, a span, the streaming feed.
			.. PairedForms(before, after),

			// FIX's log forms: the separator is `|` between spaces, and the way opened at it is what performance-ff's 55c46da6 removes.
			.. PairedFixLog(before, after),

			// The quiet form beside the match (expr's dba87a9e): only where the after side has it.
			.. PairedBool(before, after),

			// The whole-stream forms, FixGrammar.ParseFields(Stream | TextReader): the yield form `.stream` is another driver.
			.. PairedWholeStreams(before, after),

			// The rows of the libraries a side was not given are left out: a pair of Finance alone is a pair of FIX.
			.. (before.HasStock && after.HasStock ? PairedFeeds(before, after) : []),
			.. (before.HasStock && after.HasStock ? PairedRecovering(before, after) : []),

			.. (before.HasWeb && after.HasWeb ? PairedWeb(before, after) : []),
			.. (before.HasWeb && after.HasWeb ? PairedWebParsers(before, after) : []),

			// The document grammar of the benchmarks project: the one grammar that is neither a library's nor an example's.
			.. (before.HasConfig && after.HasConfig ? PairedConfig(before, after) : []),

			PairedExpression("floor",         "(int x) => x", before, after),
			PairedExpression("ladder",        "(int x, int y) => (x + y) * 3 - x / 5", before, after),
			PairedExpression("nest7",         "(int x) => (((((((x)))))))", before, after),
			PairedExpression("block",         "(int x) => { x += 1; x *= 2; return x; }", before, after),
			PairedExpression("try",           "using System; (int n) => { int r = 0; try { r = 10 / n; } catch (DivideByZeroException e) { r = -1; } catch (Exception e) { r = -2; } r }", before, after),
			PairedExpression("loop",          "(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }", before, after),
			// The linearity family's chain, at the sizes where a walk of the log from its start shows: a term is `+ x`.
			PairedExpression("terms100",      "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", 100)), before, after),
			PairedExpression("terms1000",     "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", 1000)), before, after),
			PairedExpression("overloads",     "(int x) => System.Math.Max(x, 1)", before, after),
			PairedExpression("string",        "(int x) => \"a plain string literal, with no escape in it\"", before, after),
			PairedExpression("interpolation", "(int x) => $\"{x,5:D3} and {x + 1}\"", before, after),
			PairedExpression("untyped",       "using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()", before, after, immediate: false),
			PairedExpression("refused-early", "(int x) => x +", before, after),
			PairedExpression("refused-late",  "(int x) => x + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10 +", before, after),

			PairedSql("literal", "TryParseLiteral", "1", before, after),
			PairedSql("comment", "TryParseQueryExpression", SqlWithComments, before, after),
			PairedSql("conditions100", "TryParseSearchCondition", SqlConditions(100), before, after),
			PairedSql("conditions1000", "TryParseSearchCondition", SqlConditions(1000), before, after),
			PairedTsql("comment", SqlWithComments, before, after),
			PairedSql("column", "TryParseColumnReference", "a.b.c", before, after),
			PairedSql("arithmetic", "TryParseValueExpression", "(a + b) * c - d / 5", before, after),
			PairedSql("nest8", "TryParseValueExpression", "((((((((a))))))))", before, after),
			PairedSql("condition", "TryParseSearchCondition", "x = 1 AND y IS NOT NULL OR z BETWEEN 1 AND 2", before, after),
			PairedSql("select1", "TryParseQueryExpression", "SELECT a FROM t", before, after),
			PairedSql("select20", "TryParseQueryExpression", "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1", before, after),
			PairedSql("values", "TryParseQueryExpression", "VALUES (1)", before, after),
			PairedSql("create", "TryParseSQLSchemaStatement", "CREATE TABLE t (a INT NOT NULL, b VARCHAR(20) DEFAULT 'x', PRIMARY KEY (a))", before, after),
			PairedSql("refused-late", "TryParseQueryExpression", "SELECT a, b, c FROM t WHERE a = 1 AND b = 2 AND c = ", before, after),
		];
	}

	static IEnumerable<Workload> PairedFix(string name, string text, PairedSide before, PairedSide after)
	{
		var bytes = Encoding.Latin1.GetBytes(text);

		yield return PairedFixForm(name + ".text", () => HandFixParser.Parse(text), before.FixText(text), after.FixText(text));
		yield return PairedFixForm(name + ".bytes", () => HandFixParser.Parse(bytes), before.FixBytes(bytes), after.FixBytes(bytes));
		yield return PairedFixForm(name + ".stream", () => HandFixParser.Parse(new MemoryStream(bytes, false)), before.FixStream(bytes), after.FixStream(bytes));
	}

	/// <summary>
	/// Agreement here is the fields' tags summed, not the field-by-field comparison
	/// <see cref="FixForm"/> makes — the same trade as <see cref="PairedSql"/>, for the same
	/// cross-ALC reason.
	/// </summary>
	/// <summary>
	/// The message layer's Parse and Build of a NewOrderSingle, before and after, with this tree's own
	/// FixMessages as the control where the hand parser is elsewhere. The first reading is called
	/// "hand" because the paired report's ratios are taken against it; it is not a hand-written layer.
	/// </summary>
	static IEnumerable<Workload> PairedFixMessages(PairedSide before, PairedSide after)
	{
		var wire = FixMessageWire();

		yield return PairedFixMessage("Order.parse", () => FixMessages.Parse(wire) is null ? 0 : 1, before.FixMessageParse(wire), after.FixMessageParse(wire));
		yield return PairedFixMessage("Order.build", () => FixMessages.Build(wire, [.. FixParser.Parse(wire)]) is null ? 0 : 1, before.FixMessageBuild(wire), after.FixMessageBuild(wire));
	}

	static Workload PairedFixMessage(string name, Func<int> control, Func<int> before, Func<int> after)
	{
		return new Workload(
			"fixmsg",
			name,
			[new Reading("control", control), new Reading("before", before), new Reading("after", after)],
			() => control() == 1 && before() == 1 && after() == 1
				? null
				: $"  every side must read the wire: control {control()}, before {before()}, after {after()}");
	}

	static Workload PairedFixForm(string name, Func<IEnumerable<FixField>> hand, Func<int> before, Func<int> after)
	{
		return new Workload(
			"fix",
			name,
			[new Reading("hand", () => Count(hand())), new Reading("before", before), new Reading("after", after)],
			Disagreement);

		string? Disagreement()
		{
			var h = Count(hand());
			var b = before();
			var a = after();

			return h == b && b == a ? null : $"  hand {h}, before {b}, after {a}";
		}

		static int Count(IEnumerable<FixField> fields)
		{
			var count = 0;

			foreach (var field in fields)
				count += field.Tag;

			return count;
		}
	}

	static Workload PairedExpression(string name, string text, PairedSide before, PairedSide after, bool immediate = true)
	{
		var readings = new List<Reading>
		{
			new("hand",   () => HandExpression.TryParseLambda(text, new ExpressionParser.State(Caller) { Text = text }).IsSuccess ? 1 : 0),
			new("before", before.El("TryParseLambda", text, immediate: false)),
			new("after",  after.El("TryParseLambda", text, immediate: false)),
		};

		if (immediate)
		{
			readings.Add(new Reading("before-immediate", before.El("TryParseLambda", text, immediate: true)));
			readings.Add(new Reading("after-immediate",  after.El("TryParseLambda", text, immediate: true)));
		}

		return new Workload("el", name, [.. readings], Disagreement);

		string? Disagreement()
		{
			var hand = HandExpression.TryParseLambda(text, new ExpressionParser.State(Caller) { Text = text }).IsSuccess;
			var b    = before.El("TryParseLambda", text, false)() == 1;
			var a    = after.El("TryParseLambda", text, false)() == 1;

			if (hand != b || b != a)
				return $"  hand {(hand ? "accepted" : "refused")}, before {(b ? "accepted" : "refused")}, after {(a ? "accepted" : "refused")}";

			if (!immediate)
				return null;

			var bi = before.El("TryParseLambda", text, true)() == 1;
			var ai = after.El("TryParseLambda", text, true)() == 1;

			return bi == ai ? null : $"  before-immediate {(bi ? "accepted" : "refused")}, after-immediate {(ai ? "accepted" : "refused")}";
		}
	}

	/// <summary>T-SQL of two builds against each other, with ScriptDom as the control the hand parser is elsewhere.</summary>
	static Workload PairedTsql(string name, string text, PairedSide before, PairedSide after)
	{
		return new Workload(
			"tsql",
			name,
			[
				new Reading("scriptdom", () => ScriptDomAccepts(text) ? 1 : 0),
				new Reading("before", before.Tsql(text)),
				new Reading("after",  after.Tsql(text)),
			],
			() => before.Tsql(text)() == 1 && after.Tsql(text)() == 1 ? null : "  a side refuses the statement");
	}

	static Workload PairedSql(string name, string method, string text, PairedSide before, PairedSide after)
	{
		return new Workload(
			"sql",
			name,
			[
				new Reading("hand",   () => HandAccepts(method, text) ? 1 : 0),
				new Reading("before", before.Sql(method, text)),
				new Reading("after",  after.Sql(method, text)),
			],
			Disagreement);

		string? Disagreement()
		{
			var hand = HandAccepts(method, text);
			var b    = before.Sql(method, text)() == 1;
			var a    = after.Sql(method, text)() == 1;

			return hand == b && b == a
				? null
				: $"  hand {(hand ? "accepted" : "refused")}, before {(b ? "accepted" : "refused")}, after {(a ? "accepted" : "refused")}";
		}
	}

	/// <summary>
	/// Runs <see cref="PairedWorkloads"/> and writes <c>paired.md</c>: hand, before and after
	/// in one table, the same control and pinning as <c>--stand</c>. <paramref name="only"/>
	/// keeps rows whose id contains it, for a cheaper rerun of one row a full run flagged.
	/// </summary>
	public static void Paired(string beforeDir, string afterDir, string? directory, string? only = null)
	{
		var pinned = Pin();
		PairedFixContent(beforeDir, afterDir);
		var output = directory ?? DefaultDirectory();

		Directory.CreateDirectory(output);

		var before = new PairedSide("before", beforeDir);
		var after  = new PairedSide("after", afterDir);

		var workloads = PairedWorkloads(before, after);

		if (only is not null)
			workloads = [.. workloads.Where(one => Matches(one.Id, only))];

		foreach (var workload in workloads)
			Check(workload);

		Console.WriteLine($"{workloads.Length} rows, every reading agreeing. Timing {Rounds} rounds.");

		var controls = new List<double>();
		var rows     = new List<Row>();

		foreach (var workload in workloads)
		{
			rows.Add(Measure(workload, controls));
			Console.WriteLine(Describe(rows[^1]));
		}

		var text = PairedMarkdown(pinned, Median(controls), [.. rows], SideNote(beforeDir, afterDir));

		File.WriteAllText(Path.Combine(output, "paired.md"), text);
		File.WriteAllText(Path.Combine(output, "paired.json"), JsonSerializer.Serialize(new Taken(Median(controls), [.. rows]), Json));

		Console.WriteLine();
		Console.WriteLine(text);
		Console.WriteLine($"Written to {output}");
	}

	/// <summary>
	/// What the two sides of a pair are, from the <c>build.txt</c> that <c>benchmarks/Build-Side.ps1</c> writes into a side's folder (its commit and the
	/// build properties it was given): a pair of two branches of one commit differs by those properties and nothing else, and the report says so.
	/// Empty where a folder has no such file.
	/// </summary>
	/// <summary>Whether this side's DotGram.Finance still has FixParseMode: the enum's name is in the assembly's metadata strings.</summary>
	static bool HasFixParseMode(string directory)
	{
		var path = Path.Combine(directory, "DotGram.Finance.dll");

		return File.Exists(path) && File.ReadAllBytes(path).AsSpan().IndexOf("FixParseMode"u8) >= 0;
	}

	static string FixModeNote(string beforeDir, string afterDir)
	{
		var before = HasFixParseMode(beforeDir);
		var after  = HasFixParseMode(afterDir);

		return before == after ? "" : $" **The FIX message rows (fixmsg/) compare two different readings: the {(before ? "before" : "after")} side still has FixParseMode and reads strict (schema checked in the parse), the {(before ? "after" : "before")} side has none and is read as Parse plus FixMessage.Validate(), the nearest work to what the strict parse did, and not identical to it. A difference there is the price of the move, not a speed.**";
	}

	static string SideNote(string beforeDir, string afterDir)
	{
		static string Read(string directory)
		{
			var path = Path.Combine(directory, "build.txt");

			return File.Exists(path) ? string.Join(", ", File.ReadAllLines(path).Where(static line => line.Length > 0)) : "no build.txt";
		}

		static string Line(string directory, string key)
		{
			var path = Path.Combine(directory, "build.txt");

			return File.Exists(path) ? File.ReadAllLines(path).FirstOrDefault(line => line.StartsWith(key + " ", StringComparison.Ordinal)) ?? "" : "";
		}

		var before = Read(beforeDir);
		var after  = Read(afterDir);

		if (before == "no build.txt" && after == "no build.txt")
			return FixModeNote(beforeDir, afterDir).TrimStart();

		var note = $"Sides: before ({before}); after ({after}). (The check that the two emitted different code applies to two sides of one commit given different properties; its silence says nothing about two commits.)";

		// Two sides given different properties that emitted the same code are not a pair of branches: no generator read the property.
		var propertiesBefore = Line(beforeDir, "properties");
		var propertiesAfter  = Line(afterDir, "properties");

		if (propertiesBefore != propertiesAfter && Line(beforeDir, "emitted").Length > 0 && Line(beforeDir, "emitted") == Line(afterDir, "emitted"))
			note += " **THE TWO SIDES EMITTED THE SAME CODE (byte for byte, worktree paths aside) although their properties differ: the generator read none of them, and this is not a pair of two branches.**";

		return note + FixModeNote(beforeDir, afterDir);
	}

	/// <summary>The A/A of the parent for one row: the median change of its build against itself, and the range over the runs; empty where the row was not in the A/A.</summary>
	static string AaCell(Dictionary<string, Row> aa, string id, string suffix)
	{
		if (!aa.TryGetValue(id, out var row))
			return "";

		var before = row.Readings.FirstOrDefault(one => one.Reading == "before" + suffix);
		var after  = row.Readings.FirstOrDefault(one => one.Reading == "after" + suffix);

		return before is null || after is null ? "" : $"{Change(before.Nanoseconds, after.Nanoseconds)} {(row.Ranges is { } ranges && ranges.TryGetValue(suffix, out var range) ? range : "")}";
	}

	static string PairedMarkdown(bool pinned, double control, Row[] rows, string sides = "", Dictionary<string, Row>? aa = null)
	{
		var text = new StringBuilder();

		text.AppendLine(CultureInfo.InvariantCulture, $"# Paired stand, {DateTime.Now:yyyy-MM-dd HH:mm}");
		text.AppendLine();
		text.AppendLine(CultureInfo.InvariantCulture,
			$"{Environment.MachineName}, {(pinned ? "pinned to 0-15, high priority" : "NOT pinned")}, control {control:F1} ns.");
		text.AppendLine();
		text.AppendLine(CultureInfo.InvariantCulture, $"This binary, and the libraries it holds as the control, was built from {BinaryCommit()}.");
		text.AppendLine();

		if (sides.Length > 0)
		{
			text.AppendLine(sides);
			text.AppendLine();
		}

		if (rows.Any(static row => row.Id.EndsWith(".bool", StringComparison.Ordinal) || row.Id.Contains(".bool", StringComparison.Ordinal)))
		{
			text.AppendLine("**The rows named `.bool` compare two APIs, not two commits: `before` is the Match form of the before side and `after` the bool form of the after side (`StandBool.cs`), so their difference is the price of the Match form against the bool form. It is there in an A/A of one build and in a pair of commits that have nothing to do with it, and it is not an effect of the commit under test.**");
			text.AppendLine();
		}

		text.AppendLine("The base of a row is what every ratio is over, and its name says what it is: `hand` is a hand-written parser (`DotGram.Handwritten`), `scriptdom` is Microsoft's parser, and `control` is this process's own build of the same generated parser, held constant so that the two sides are compared and nothing is claimed against a hand-written one.");
		text.AppendLine();
		text.AppendLine("The last two columns are what a reader a month later cannot get from a message: **the base's spread between the runs** (how much the machine's speed varied from one run to the next; a row with a large one had a disturbed run, and its medians are read with the range beside them) and the **change of each run** (the smallest and the largest, and how many of the runs were positive).");
		text.AppendLine();
		if (aa is not null)
		{
			text.AppendLine("The last column is the **A/A of the parent** taken in the same slot, on the same rows: the parent's build against itself, its median change and its range over the runs. What the stand says of a row when nothing changed; a change is read as its excess over it (D50).");
			text.AppendLine();
		}

		text.AppendLine("| row | reading | base | base ns | before ns | before/base | after ns | after/base | change | before B | after B | base spread | change over the runs |" + (aa is null ? "" : " A/A of the parent |"));
		text.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |" + (aa is null ? "" : " --- |"));

		foreach (var row in rows)
		{
			var hand = row.Readings[0];

			foreach (var reading in row.Readings.Where(static one => one.Reading.StartsWith("before", StringComparison.Ordinal)))
			{
				var suffix = reading.Reading["before".Length..];
				var after  = row.Readings.First(one => one.Reading == "after" + suffix);
				var name   = suffix.Length == 0 ? "generated" : suffix.TrimStart('-');
				var aaCell = aa is null ? "" : $" {AaCell(aa, row.Id, suffix)} |";

				text.AppendLine(CultureInfo.InvariantCulture,
					$"| {row.Id} | {name} | {hand.Reading} | {hand.Nanoseconds:F1} | {reading.Nanoseconds:F1} | {reading.Nanoseconds / hand.Nanoseconds:F2}x | " +
					$"{after.Nanoseconds:F1} | {after.Nanoseconds / hand.Nanoseconds:F2}x | {Change(reading.Nanoseconds, after.Nanoseconds)} | {reading.Bytes:F0} | {after.Bytes:F0} | {row.HandSpread:P0} | {(row.Ranges is { } ranges && ranges.TryGetValue(suffix, out var range) ? range : "")} |{aaCell}");
			}
		}

		return text.ToString();
	}

	static string? Differ(IEnumerable<string> hand, IEnumerable<string> generated)
	{
		var a = hand.ToArray();
		var b = generated.ToArray();

		for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
			if (a[i] != b[i])
				return $"  field {i}\n  hand      {a[i]}\n  generated {b[i]}";

		return a.Length == b.Length ? null : $"  hand read {a.Length} fields, generated {b.Length}";
	}

	// ── Timing ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Every reading of a row, warmed until stable, then timed round-robin with the order
	/// turned every round, and the control timed between rounds. A round a generation 1 or 2
	/// collection fell inside is not a reading of the parser but of the collector, so it is
	/// redone rather than kept (D4: on a few hundred iterations, one such pause dominates the
	/// round's average outright). Every round is kept, not only its median — gen0/1/2 deltas
	/// beside it — so a row that turns out bimodal (2026-09-18: sql/arithmetic, el/interpolation)
	/// is visible in the json without a second, diagnostic-only run.
	/// </summary>
	/// <summary>
	/// Which reading is taken k-th in a round: the readings rotate by one place each round, and every n rounds the order is reversed, so that over 2n rounds every reading
	/// has stood in every place the same number of times. It used to be forward in one round and reversed in the next, which for three readings (the base, before, after)
	/// puts `before` in the middle of every round and `after` at the two ends: the position of a reading was then a difference between the sides, and the A/A of one build
	/// against itself read a few percent apart (D50, 2026-09-20). Measured with an A/A of one build after the change.
	/// </summary>
	static int InRound(int round, int k, int n)
	{
		var place = (k + round % n) % n;

		return (round / n) % 2 == 0 ? place : n - 1 - place;
	}

	static Row Measure(Workload workload, List<double> controls)
	{
		var iterations = Iterations(workload.Readings[0].Run);
		var warmups    = new int[workload.Readings.Length];

		// A sample is as many calls as take SampleMs of the base reading, which a reading much slower than
		// it (the stock count's generated parser is 50-90x the hand one on a thousand lines) would take
		// seconds over, eight times a round for five runs. It takes at most four times the calls it would
		// need for a sample of its own: within 4x of the base nothing changes, beyond it the sample is a
		// few times SampleMs long and no more.
		var each = workload.Readings.Select((reading, i) => i == 0 ? iterations : Math.Min(iterations, 4 * Iterations(reading.Run))).ToArray();

		// The control is a yardstick for the machine, so it is read after the tiered JIT has
		// settled: in a run of one row the first samples were tier 0 code (138 ns against 30).
		if (controls.Count == 0)
			WarmUntilStable(Control, ControlIterations);

		for (var i = 0; i < workload.Readings.Length; i++)
			warmups[i] = WarmUntilStable(workload.Readings[i].Run);

		var taken  = workload.Readings.Select(_ => new List<RoundSample>()).ToArray();
		var redone = 0;

		// At least Rounds rounds, and a multiple of 2n so that every reading stands in every place equally (InRound).
		var rounds = (Rounds + 2 * workload.Readings.Length - 1) / (2 * workload.Readings.Length) * 2 * workload.Readings.Length;

		for (var round = 0; round < rounds; round++)
		{
			controls.Add(Time(Control, ControlIterations));

			for (var k = 0; k < workload.Readings.Length; k++)
			{
				var i = InRound(round, k, workload.Readings.Length);

				taken[i].Add(TimeSteady(workload.Readings[i].Run, each[i], ref redone));
			}
		}

		if (redone > 0)
			Console.WriteLine($"  {workload.Id}: {redone} round(s) redone after a gen1/gen2 collection");

		var hand = taken[0].Select(static one => one.Nanoseconds).ToList();

		return new Row(
			workload.Id,
			[.. workload.Readings.Select((reading, i) => new Timed(
				reading.Name, Median(taken[i].Select(static one => one.Nanoseconds).ToList()), Allocated(reading.Run),
				warmups[i], [.. taken[i]]))],
			(hand.Max() - hand.Min()) / Median(hand));
	}

	const double WarmupCapSeconds = 2.0;

	/// <summary>
	/// Warms a reading until two consecutive samples agree within 5%, capped at
	/// <see cref="WarmupCapSeconds"/> — a fixed three-sample warmup left sql/arithmetic and
	/// el/interpolation still mid-JIT-tier-promotion in some runs (2026-09-18, architect: the
	/// stand keeps re-JITting new methods as later rows start, which resets the tiering
	/// call-counting delay for whichever row is warming up). Returns how many samples that
	/// took, kept beside the row's timings.
	/// </summary>
	static int WarmUntilStable(Func<int> run, int iterations = 0)
	{
		var watch    = Stopwatch.StartNew();
		var previous = double.NaN;
		var samples  = 0;

		while (watch.Elapsed.TotalSeconds < WarmupCapSeconds)
		{
			var elapsed = Time(run, iterations > 0 ? iterations : Iterations(run));

			samples++;

			if (!double.IsNaN(previous) && Math.Abs(elapsed - previous) / ((elapsed + previous) / 2) <= 0.05)
				return samples;

			previous = elapsed;
		}

		return samples;
	}

	const int MaxRetries = 5;

	/// <summary>
	/// One round's time and what the collector did during it, redone up to
	/// <see cref="MaxRetries"/> times when a generation 1 or 2 collection ran during it — gen0
	/// is left alone: it is part of ordinary allocation cost, not the rare pause this filters
	/// out. The gen0/1/2 counts kept are the winning attempt's, not the discarded ones'.
	/// </summary>
	static RoundSample TimeSteady(Func<int> run, int iterations, ref int redone)
	{
		for (var attempt = 0; ; attempt++)
		{
			var gen0    = GC.CollectionCount(0);
			var gen1    = GC.CollectionCount(1);
			var gen2    = GC.CollectionCount(2);
			var elapsed = Time(run, iterations);
			var d0      = GC.CollectionCount(0) - gen0;
			var d1      = GC.CollectionCount(1) - gen1;
			var d2      = GC.CollectionCount(2) - gen2;

			if ((d1 == 0 && d2 == 0) || attempt == MaxRetries - 1)
				return new RoundSample(elapsed, d0, d1, d2);

			redone++;
		}
	}

	/// <summary>
	/// How many calls make a sample of <see cref="SampleMs"/> of the hand-written reading, which
	/// the generated readings then take longer over.
	/// </summary>
	static int Iterations(Func<int> run)
	{
		for (var iterations = 1; ; iterations *= 2)
		{
			var watch = Stopwatch.StartNew();

			for (var i = 0; i < iterations; i++)
				_sink += run();

			if (watch.ElapsedMilliseconds >= SampleMs || iterations >= 1 << 24)
				return iterations;
		}
	}

	static double Time(Func<int> run, int iterations)
	{
		var watch = Stopwatch.StartNew();
		var sink  = 0;

		for (var i = 0; i < iterations; i++)
			sink += run();

		watch.Stop();
		_sink += sink;

		return watch.Elapsed.TotalMilliseconds * 1e6 / iterations;
	}

	static double Allocated(Func<int> run)
	{
		const int calls = 64;

		_sink += run();

		var before = GC.GetAllocatedBytesForCurrentThread();

		for (var i = 0; i < calls; i++)
			_sink += run();

		return (GC.GetAllocatedBytesForCurrentThread() - before) / (double)calls;
	}

	const int ControlIterations = 200_000;

	static readonly ulong[] ControlData = [.. Enumerable.Range(1, 64).Select(static i => (ulong)i * 0x9E3779B97F4A7C15UL)];

	/// <summary>
	/// Arithmetic over a small array, no allocation and no call: what the machine itself does
	/// in a nanosecond, which is what two runs must agree on before their parsers are compared.
	/// </summary>
	static int Control()
	{
		var hash = 14695981039346656037UL;

		foreach (var value in ControlData)
			hash = (hash ^ value) * 1099511628211UL;

		return (int)hash;
	}

	static double Median(List<double> values)
	{
		var ordered = values.Order().ToArray();

		return ordered.Length == 0
			? double.NaN
			: ordered.Length % 2 == 1
				? ordered[ordered.Length / 2]
				: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}

	// ── The first call ──────────────────────────────────────────────────────────

	static readonly string[] FirstRows = ["fix/One.text", "fix/One.bytes", "el/floor", "sql/literal", "sql/select20", "fixmsg/Order44.strict"];

	const int FirstRuns = 3;

	/// <summary>
	/// The first parse in a fresh process, readings alternated across processes, the median of
	/// three: what loading, type initialization and the JIT cost before a parse is warm. It
	/// excludes starting the process, which is the same for both.
	/// </summary>
	static FirstCall[] FirstCalls(Workload[] workloads)
	{
		var calls = new List<FirstCall>();

		foreach (var id in FirstRows)
		{
			var workload = Array.Find(workloads, one => one.Id == id);

			if (workload is null)
				continue;

			var taken = workload.Readings.Select(_ => new List<double>()).ToArray();

			for (var run = 0; run < FirstRuns; run++)
				for (var k = 0; k < workload.Readings.Length; k++)
				{
					var i = run % 2 == 0 ? k : workload.Readings.Length - 1 - k;

					taken[i].Add(Child(id, workload.Readings[i].Name));
				}

			for (var i = 0; i < workload.Readings.Length; i++)
				calls.Add(new FirstCall(id, workload.Readings[i].Name, Median(taken[i])));
		}

		return [.. calls];
	}

	static double Child(string id, string reading)
	{
		var host  = Environment.ProcessPath!;
		var start = new ProcessStartInfo(host)
		{
			RedirectStandardOutput = true,
			UseShellExecute        = false,
		};

		// Run as `dotnet DotGram.Benchmarks.dll`, the process is the host, and the child needs
		// the assembly named again.
		if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			start.ArgumentList.Add(typeof(Stand).Assembly.Location);

		start.ArgumentList.Add("--stand-first");
		start.ArgumentList.Add(id);
		start.ArgumentList.Add(reading);

		using var process = Process.Start(start)!;

		var text = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		return double.Parse(text.Trim(), CultureInfo.InvariantCulture);
	}

	// ── What a streamed parse holds ─────────────────────────────────────────────

	const long StreamedFields = 2_000_000;

	/// <summary>
	/// FIX read lazily from a stream made as it is read, so that nothing but the parser can hold
	/// it: the highest live heap seen, above what was live before.
	/// </summary>
	/// <remarks>
	/// Live means after a full collection: the heap between collections holds what was allocated
	/// since the last one, which says how often the collector runs and not what the parser keeps.
	/// A full collection is dear, so it is taken every 262,144 fields, eight times a run.
	/// </remarks>
	static Held[] Streamed()
	{
		return
		[
			Hold("hand",      stream => HandFixParser.Parse(stream)),
			Hold("generated", stream => FixParser.Parse(stream)),
		];

		static Held Hold(string reading, Func<Stream, IEnumerable<FixField>> parse)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var floor  = GC.GetTotalMemory(true);
			var peak   = floor;
			var fields = 0L;

			foreach (var field in parse(new MadeFix(StreamedFields)))
			{
				fields++;

				if ((fields & 0x3FFFF) == 0)
					peak = Math.Max(peak, GC.GetTotalMemory(true));
			}

			return new Held(reading, (peak - floor) / 1024.0, fields);
		}
	}

	/// <summary>
	/// FIX wire fields made as they are read and never kept: ordinary fields with a length and
	/// data pair among them whose payload holds the separator.
	/// </summary>
	sealed class MadeFix(long fields) : Stream
	{
		static readonly byte[] Chunk = Encoding.Latin1.GetBytes("55=ABC\u000144=12.50\u000195=3\u000196=a\u0001b\u000138=100\u0001");

		const int ChunkFields = 4;

		readonly long _length = fields / ChunkFields * Chunk.Length;
		long          _position;

		public override bool CanRead  => true;
		public override bool CanSeek  => false;
		public override bool CanWrite => false;
		public override long Length   => _length;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Read(buffer.AsSpan(offset, count));
		}

		public override int Read(Span<byte> buffer)
		{
			var count = (int)Math.Min(buffer.Length, _length - _position);

			for (var i = 0; i < count; i++)
				buffer[i] = Chunk[(int)((_position + i) % Chunk.Length)];

			_position += count;

			return count;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}

	// ── What the generator took ─────────────────────────────────────────────────

	/// <summary>How far a host's generation time may move from the previous base before it is named.</summary>
	const double GenerationTolerance = 0.20;

	/// <summary>
	/// A host that moved by less than this is not named however large the ratio: a grammar that
	/// takes 4 ms and then 6 is not news.
	/// </summary>
	const double GenerationFloorMilliseconds = 100;

	/// <summary>
	/// The generator's time per host, held to the previous base's: every host that moved by more
	/// than <see cref="GenerationTolerance"/> is named on a line of its own, and a run with
	/// nothing to name says that it compared. A commit that made the generator 20 times slower on
	/// T-SQL (2026-09-18, 4 s to 86 s) was not noticed for an afternoon; this is the line that
	/// would have caught it in the first run after it.
	/// </summary>
	static string GenerationGate(Result now, string? root, string? against)
	{
		var text = new StringBuilder();

		text.AppendLine();
		text.AppendLine("Generator time against the previous base, as history: the milliseconds move with the machine (the morning's base, rebuilt that evening, read 22-39% higher), so the gate to quote is `benchmarks/Gate-Generation.ps1`, which rebuilds the base alternately with the head in one run and holds their ratio:");
		text.AppendLine();

		var path = against ?? PreviousBase(root);

		if (path is null || !File.Exists(path))
		{
			text.AppendLine("Nothing to compare with: no `--against`, and no benchmarks/results/stand-*.json.");

			return text.ToString();
		}

		var was = JsonSerializer.Deserialize<Result>(File.ReadAllText(path), Json)!;

		if (now.Generation.Length == 0 || was.Generation.Length == 0)
		{
			text.AppendLine(CultureInfo.InvariantCulture, $"Nothing to compare: {(now.Generation.Length == 0 ? "this run" : $"the base {Path.GetFileName(path)}")} has no generator report. Use `--rebuild`.");

			return text.ToString();
		}

		var named    = new List<string>();
		var compared = 0;

		foreach (var one in now.Generation)
		{
			var before = Array.Find(was.Generation, other => other.Host == one.Host);

			if (before is null)
			{
				named.Add($"{one.Host}: new, {one.Milliseconds:F0} ms");

				continue;
			}

			compared++;

			var change = one.Milliseconds / before.Milliseconds - 1;

			if (Math.Abs(change) > GenerationTolerance && Math.Abs(one.Milliseconds - before.Milliseconds) >= GenerationFloorMilliseconds)
				named.Add(string.Create(CultureInfo.InvariantCulture, $"{one.Host}: {before.Milliseconds:F0} ms to {one.Milliseconds:F0} ms ({change:+0%;-0%})"));
		}

		foreach (var gone in was.Generation)
			if (Array.Find(now.Generation, other => other.Host == gone.Host) is null)
				named.Add($"{gone.Host}: was {gone.Milliseconds:F0} ms, no report now");

		text.AppendLine(CultureInfo.InvariantCulture,
			$"Base {Path.GetFileName(path)} ({was.Commit}, {was.Taken:yyyy-MM-dd HH:mm}); {compared} hosts compared, tolerance {GenerationTolerance:P0} and {GenerationFloorMilliseconds:F0} ms.");
		text.AppendLine();

		if (named.Count == 0)
			text.AppendLine("None moved by more than that.");

		foreach (var line in named)
			text.AppendLine("- DEVIATION " + line);

		return text.ToString();
	}

	/// <summary>The newest result kept in the repository, by name, which carries its date.</summary>
	static string? PreviousBase(string? root)
	{
		var directory = root is null ? null : Path.Combine(root, "benchmarks", "results");

		return directory is not null && Directory.Exists(directory)
			? Directory.GetFiles(directory, "stand-*.json").OrderBy(static one => one, StringComparer.Ordinal).LastOrDefault()
			: null;
	}

	static readonly Regex Summary = new(
		@"DotGram: (?<host>[^,]+), (?<rules>\d+) normalized rules, (?<bytes>\d+) bytes UTF-8 C#, (?<ms>[\d.]+) ms generation, mode=(?<mode>\w+)",
		RegexOptions.CultureInvariant);

	/// <summary>
	/// The generator's own report on every grammar the last build of this tree compiled: rules,
	/// bytes of C# and milliseconds. As fresh as that build, which is why each says when it was
	/// written.
	/// </summary>
	static Generated[] Generation(string? root)
	{
		if (root is null)
			return [];

		var found = new Dictionary<string, Generated>(StringComparer.Ordinal);

		foreach (var top in new[] { "src", "examples" })
		{
			var directory = Path.Combine(root, top);

			if (!Directory.Exists(directory))
				continue;

			foreach (var file in Directory.EnumerateFiles(directory, "*.DotGramReport.g.cs", SearchOption.AllDirectories))
			{
				var match = Summary.Match(File.ReadAllText(file));

				if (!match.Success)
					continue;

				var written = File.GetLastWriteTime(file);
				var host    = match.Groups["host"].Value;

				if (found.TryGetValue(host, out var known) && known.Written >= written)
					continue;

				found[host] = new Generated(
					host,
					int.Parse(match.Groups["rules"].Value, CultureInfo.InvariantCulture),
					long.Parse(match.Groups["bytes"].Value, CultureInfo.InvariantCulture),
					double.Parse(match.Groups["ms"].Value, CultureInfo.InvariantCulture),
					match.Groups["mode"].Value,
					written);
			}
		}

		return [.. found.Values.OrderByDescending(static one => one.Bytes)];
	}

	/// <summary>
	/// Every project under src/ and examples/ that hosts a grammar, found the same way the
	/// generator's report requires: a reference to <c>DotGram.csproj</c> built as an analyzer.
	/// A hand-written project (DotGram.Handwritten) or one without a grammar of its own is not
	/// in this list, and neither is anything under tests/ or benchmarks/ — Generation() never
	/// looks there either.
	/// </summary>
	static string[] ReportProjects(string root)
	{
		var projects = new List<string>();

		foreach (var top in new[] { "src", "examples" })
		{
			var directory = Path.Combine(root, top);

			if (!Directory.Exists(directory))
				continue;

			foreach (var project in Directory.EnumerateFiles(directory, "*.csproj", SearchOption.AllDirectories))
			{
				var text = File.ReadAllText(project);

				if (text.Contains("DotGram.csproj", StringComparison.Ordinal) &&
				    text.Contains("OutputItemType=\"Analyzer\"", StringComparison.Ordinal))
					projects.Add(project);
			}
		}

		return [.. projects];
	}

	/// <summary>
	/// Rebuilds every grammar-hosting project, so the reports Generation() reads are this
	/// build's: DotGram.targets deletes the old ones before CoreCompile runs, and an
	/// incremental build that found nothing to recompile leaves none behind.
	/// </summary>
	/// <remarks>
	/// Node reuse and the compiler server carry an earlier process's rights into a directory
	/// this session made, which reads as "access denied" and not as a stale build; both are
	/// turned off for the same reason the generator's own profiling harness turns them off.
	/// </remarks>
	static void Rebuild(string? root)
	{
		if (root is null)
			throw new InvalidOperationException("--rebuild found no repository root (no DotGram.slnx above this process).");

		var projects = ReportProjects(root);

		Console.WriteLine($"Rebuilding {projects.Length} grammar-hosting projects for fresh generator reports...");

		foreach (var project in projects)
		{
			Console.WriteLine($"  {Path.GetFileNameWithoutExtension(project)}");

			var start = new ProcessStartInfo("dotnet")
			{
				UseShellExecute = false,
			};

			start.ArgumentList.Add("build");
			start.ArgumentList.Add(project);
			start.ArgumentList.Add("-c");
			start.ArgumentList.Add("Release");
			start.ArgumentList.Add("-t:Rebuild");
			start.ArgumentList.Add("-v:quiet");
			start.ArgumentList.Add("-m:1");
			start.ArgumentList.Add("-nodeReuse:false");
			start.ArgumentList.Add("-p:UseSharedCompilation=false");

			start.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";

			using var process = Process.Start(start)!;

			process.WaitForExit();

			if (process.ExitCode != 0)
				throw new InvalidOperationException($"Rebuilding {project} exited {process.ExitCode}.");
		}
	}

	/// <summary>
	/// Grammar-hosting projects whose directory holds no report from the last build: either the
	/// build skipped them (nothing to recompile, incrementally) or it has not run since
	/// DotGram.targets last deleted their reports. <c>--rebuild</c> is what closes this list.
	/// </summary>
	static string[] MissingGeneration(string? root)
	{
		if (root is null)
			return [];

		var missing = new List<string>();

		foreach (var project in ReportProjects(root))
		{
			var directory = Path.GetDirectoryName(project)!;

			if (!Directory.EnumerateFiles(directory, "*.DotGramReport.g.cs", SearchOption.AllDirectories).Any())
				missing.Add(Path.GetFileNameWithoutExtension(project));
		}

		return [.. missing.Order(StringComparer.Ordinal)];
	}

	// ── Where and on what ───────────────────────────────────────────────────────

	/// <summary>
	/// Logical processors 0 to 15 at high priority, where the machine has them (D4).
	/// </summary>
	static bool Pin()
	{
		try
		{
			var process = Process.GetCurrentProcess();

			if (Environment.ProcessorCount >= 16 && OperatingSystem.IsWindows())
				process.ProcessorAffinity = 0xFFFF;

			process.PriorityClass = ProcessPriorityClass.High;

			return Environment.ProcessorCount >= 16;
		}
		catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or PlatformNotSupportedException)
		{
			return false;
		}
	}

	/// <summary>
	/// The repository the reports and the previous base are read from: the one this program was built in,
	/// or the one <c>DOTGRAM_ROOT</c> names — a copy of the binaries kept elsewhere (a RAM disk, so that
	/// a build in the tree cannot move them under a run) has no repository above it to find.
	/// </summary>
	static string? Root()
	{
		if (Environment.GetEnvironmentVariable("DOTGRAM_ROOT") is { Length: > 0 } named && File.Exists(Path.Combine(named, "DotGram.slnx")))
			return named;

		for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
			if (File.Exists(Path.Combine(directory.FullName, "DotGram.slnx")))
				return directory.FullName;

		return null;
	}

	/// <summary>
	/// The commit this binary was built from, the source revision the SDK embeds in the informational version of the assembly: a measurement is of
	/// that, whatever the tree it is run in has moved on to (a binary has no date that shows it). Eight characters.
	/// </summary>
	internal static string BinaryCommit()
	{
		var version = typeof(Stand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
		var plus    = version.IndexOf('+');

		return plus < 0 ? "unknown" : version[(plus + 1)..Math.Min(version.Length, plus + 9)];
	}

	/// <summary>The commit the binary was built from and, only where the tree it is run in is at another, that tree's.</summary>
	static string Commit(string? root)
	{
		var built = BinaryCommit();

		if (root is null)
			return built;

		var head  = Git(root, "rev-parse --short HEAD");
		var dirty = Git(root, "status --porcelain --untracked-files=no");
		var tree  = dirty.Length > 0 ? head + "+changes" : head;

		return head.Length >= 7 && built.StartsWith(head[..7], StringComparison.Ordinal) ? (dirty.Length > 0 ? built + "+changes" : built) : $"{built} (run in a tree at {tree})";
	}

	static string Git(string root, string arguments)
	{
		try
		{
			var start = new ProcessStartInfo("git", $"-C \"{root}\" {arguments}")
			{
				RedirectStandardOutput = true,
				UseShellExecute        = false,
			};

			using var process = Process.Start(start)!;

			var text = process.StandardOutput.ReadToEnd().Trim();

			process.WaitForExit();

			return text;
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return "?";
		}
	}

	static string DefaultDirectory()
	{
		var temp = Directory.Exists(@"T:\TEMP") ? @"T:\TEMP" : Path.GetTempPath();

		return Path.Combine(temp, "dotgram-stand", DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture));
	}

	// ── Writing it down ─────────────────────────────────────────────────────────

	static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

	static string Describe(Row row)
	{
		var hand = row.Readings[0].Nanoseconds;
		var text = new StringBuilder($"{row.Id,-22}");
		var @base = row.Readings[0].Reading;

		foreach (var reading in row.Readings)
			text.Append(CultureInfo.InvariantCulture, $" {reading.Reading} {reading.Nanoseconds,10:F1} ns");

		foreach (var reading in row.Readings.Skip(1))
			text.Append(CultureInfo.InvariantCulture, $"  {reading.Reading}/{@base} {reading.Nanoseconds / hand:F2}x");

		return text.ToString();
	}

	static string Markdown(Result result)
	{
		var text = new StringBuilder();

		text.AppendLine(CultureInfo.InvariantCulture, $"# Stand, {result.Commit}, {result.Taken:yyyy-MM-dd HH:mm}");
		text.AppendLine();
		text.AppendLine(CultureInfo.InvariantCulture,
			$"{result.Machine}, {result.Runtime}, {(result.Pinned ? "pinned to 0-15, high priority" : "NOT pinned")}, control {result.Control:F1} ns.");
		text.AppendLine();
		var references = result.Rows.Any(static row => row.Readings.Any(static one => one.Reading.StartsWith("reference", StringComparison.Ordinal)));

		// One table per base: the hand-written reading where there is one, and the generated one
		// on the rows of a format nobody wrote a parser of by hand (the web's).
		if (references)
		{
			text.AppendLine("A `reference-*` reading is another library's reader over the same text. The row's check holds it to what the generated parser does: both accept the input, or, on a `refused` row, both refuse it. It says how fast that reader is here, and not what this grammar costs against a hand-written reader; it has no ratio.");
			text.AppendLine();
		}

		foreach (var group in result.Rows.GroupBy(static row => row.Readings[0].Reading))
		{
			var @base = group.Key;

			if (@base != "hand")
			{
				text.AppendLine();
				text.AppendLine($"Rows with no hand-written parser, so the base is the {@base} reading:");
				text.AppendLine();
			}

			text.AppendLine($"| row | {@base} ns | reading | ns | /{@base} | {@base} B | B | {@base} spread |");
			text.AppendLine("| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |");

			foreach (var row in group)
			{
				var first = row.Readings[0];

				if (row.Readings.Length == 1)
					text.AppendLine(CultureInfo.InvariantCulture,
						$"| {row.Id} | {first.Nanoseconds:F1} | — (N/A) | | | {first.Bytes:F0} | | {row.HandSpread:P0} |");

				foreach (var reading in row.Readings.Skip(1))
					text.AppendLine(CultureInfo.InvariantCulture,
						$"| {row.Id} | {first.Nanoseconds:F1} | {reading.Reading} | {reading.Nanoseconds:F1} | {(reading.Reading.StartsWith("reference", StringComparison.Ordinal) ? "reference" : $"{reading.Nanoseconds / first.Nanoseconds:F2}x")} | {first.Bytes:F0} | {reading.Bytes:F0} | {row.HandSpread:P0} |");
			}
		}

		// A run of some rows (`--only`) has no first calls, no streamed run and no reports to show.
		if (result.FirstCalls.Length == 0 && result.Streamed.Length == 0 && result.Generation.Length == 0 && result.MissingGeneration.Length == 0)
			return text.ToString();

		text.AppendLine();
		text.AppendLine("First call in a fresh process, median of three:");
		text.AppendLine();
		text.AppendLine("| row | reading | ms |");
		text.AppendLine("| --- | --- | ---: |");

		foreach (var call in result.FirstCalls)
			text.AppendLine(CultureInfo.InvariantCulture, $"| {call.Id} | {call.Reading} | {call.Milliseconds:F2} |");

		if (result.FirstCalls.Any(static call => call.Id == "fixmsg/Order44.strict"))
		{
			text.AppendLine();
			text.AppendLine("**`fixmsg/Order44.strict`, `reference-QuickFIXn`: its first call includes building the data dictionary (`new DataDictionary(path)` reads FIX44.xml, about a megabyte), and it is the cost of starting to use that reader, paid once per process and not per message.** An application that reads a million messages spreads it to nothing; dividing this number by ours reads as \"a hundred times faster\" and is the wrong quantity. The generated reading's first call builds no dictionary: its dictionary is compiled into the code and its 447 slots are filled as they are asked.");
		}

		text.AppendLine();
		text.AppendLine(CultureInfo.InvariantCulture, $"Held while {StreamedFields:N0} FIX fields are read lazily from a stream:");
		text.AppendLine();

		foreach (var held in result.Streamed)
			text.AppendLine(CultureInfo.InvariantCulture, $"- {held.Reading}: {held.Kilobytes:F1} KB above the floor, {held.Fields:N0} fields");

		text.AppendLine();
		text.AppendLine("What the generator took, from the last build's reports:");
		text.AppendLine();

		// A build that compiled nothing leaves no report: DotGram.targets deletes the old ones before
		// every compile, so that only this compiler's are printed, and a skipped compile writes none.
		if (result.Generation.Length == 0)
			text.AppendLine("None found: the last build compiled no grammar. Rebuild the projects (`-t:Rebuild`, or `--stand --rebuild`) to have them.");
		else
		{
			text.AppendLine("| host | rules | MB of C# | ms | mode | written |");
			text.AppendLine("| --- | ---: | ---: | ---: | --- | --- |");

			foreach (var one in result.Generation)
				text.AppendLine(CultureInfo.InvariantCulture,
					$"| {one.Host} | {one.Rules} | {one.Bytes / 1024.0 / 1024.0:F2} | {one.Milliseconds:F0} | {one.Mode} | {one.Written:yyyy-MM-dd HH:mm} |");
		}

		if (result.MissingGeneration.Length > 0)
		{
			text.AppendLine();
			text.AppendLine(
				"No report from the last build (compile skipped it, or nothing has recompiled it since " +
				"DotGram.targets last deleted the old one) — rebuild with `-t:Rebuild`, or `--stand --rebuild`:");
			text.AppendLine();

			foreach (var project in result.MissingGeneration)
				text.AppendLine($"- {project}");
		}

		return text.ToString();
	}
}
