using System;
using System.Diagnostics;
using System.Globalization;
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
static class Stand
{
	/// <summary>
	/// One way of reading a row's input, which answers a number so that nothing it does can be
	/// optimized away.
	/// </summary>
	sealed record Reading(string Name, Func<int> Run);

	/// <summary>
	/// One input, read the hand-written way first and by every generated reading after it.
	/// </summary>
	/// <param name="Disagreement">Null where every reading answers the same; otherwise what differs.</param>
	sealed record Workload(string Family, string Name, Reading[] Readings, Func<string?> Disagreement)
	{
		public string Id => Family + "/" + Name;
	}

	sealed record Timed(string Reading, double Nanoseconds, double Bytes);

	sealed record Row(string Id, Timed[] Readings, double HandSpread);

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

	// Picked 2026-09-18 against the fast rows, where a round's batch is short enough that one
	// OS-level jitter or GC pause dominates its average: 7/25 left several under 100 ns at
	// 60-120% spread; 9/75 brought nearly every row under 15% but cost 4x the run; 8/40 (here)
	// keeps the run under 2x and clears most rows. A few of the fastest (sql/arithmetic,
	// el/interpolation) still swing 30-70% run to run at any of these settings — evidence it is
	// transient machine noise rather than something a bigger sample dilutes, since the same
	// settings gave one of them both 67% and 10% back to back. Reported to the architect rather
	// than chased further inside this budget.
	const int Rounds   = 8;
	const int SampleMs = 40;

	static int _sink;

	// ── Running ─────────────────────────────────────────────────────────────────

	public static void Run(string? directory, bool rebuild)
	{
		var pinned = Pin();
		var root   = Root();
		var output = directory ?? DefaultDirectory();

		Directory.CreateDirectory(output);

		if (rebuild)
			Rebuild(root);

		var workloads = Workloads();

		foreach (var workload in workloads)
			if (workload.Disagreement() is { } disagreement)
				throw new InvalidOperationException($"{workload.Id}: the readings disagree, so no ratio would mean anything.\n{disagreement}");

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
			FirstCalls(workloads),
			Streamed(),
			Generation(root),
			MissingGeneration(root));

		File.WriteAllText(Path.Combine(output, "stand.json"), JsonSerializer.Serialize(result, Json));
		File.WriteAllText(Path.Combine(output, "stand.md"), Markdown(result));

		Console.WriteLine();
		Console.WriteLine(Markdown(result));
		Console.WriteLine($"Written to {output}");
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
		Console.WriteLine("| row | reading | before /hand | after /hand | change | before B | after B |");
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
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		return
		[
			.. Fix("One",        "55=ABC\u0001"),
			.. Fix("Order",      order),
			.. Fix("BinaryMany", string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64))),
			.. Fix("Orders128",  string.Concat(Enumerable.Repeat(order, 128))),

			Expression("floor",         "(int x) => x"),
			Expression("ladder",        "(int x, int y) => (x + y) * 3 - x / 5"),
			Expression("nest7",         "(int x) => (((((((x)))))))"),
			Expression("block",         "(int x) => { x += 1; x *= 2; return x; }"),
			Expression("loop",          "(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }"),
			Expression("overloads",     "(int x) => System.Math.Max(x, 1)"),
			Expression("interpolation", "(int x) => $\"{x,5:D3} and {x + 1}\""),
			Expression("untyped",       "using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()", immediate: false),

			Sql<Ast.LiteralValue, Ast.LiteralValue>("literal", "1", SqlStandardParser.TryParseLiteral, HandSqlStandard.TryParseLiteral),
			Sql<Ast.Expression, Ast.Expression>("column", "a.b.c", SqlStandardParser.TryParseColumnReference, HandSqlStandard.TryParseColumnReference),
			Sql<Ast.Expression, Ast.Expression>("arithmetic", "(a + b) * c - d / 5", SqlStandardParser.TryParseValueExpression, HandSqlStandard.TryParseValueExpression),
			Sql<Ast.Expression, Ast.Expression>("nest8", "((((((((a))))))))", SqlStandardParser.TryParseValueExpression, HandSqlStandard.TryParseValueExpression),
			Sql<Ast.Expression, Ast.Expression>("condition", "x = 1 AND y IS NOT NULL OR z BETWEEN 1 AND 2", SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("select1", "SELECT a FROM t", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("select20", "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement.Select, Ast.Statement.Select>("values", "VALUES (1)", SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
			Sql<Ast.Statement, Ast.Statement>("create", "CREATE TABLE t (a INT NOT NULL, b VARCHAR(20) DEFAULT 'x', PRIMARY KEY (a))", SqlStandardParser.TryParseSQLSchemaStatement, HandSqlStandard.TryParseSQLSchemaStatement),
		];
	}

	/// <summary>
	/// One FIX input three ways: a string, bytes already in memory, and a stream read lazily.
	/// </summary>
	static IEnumerable<Workload> Fix(string name, string text)
	{
		var bytes = Encoding.Latin1.GetBytes(text);

		yield return FixForm(name + ".text",
			() => HandFixParser.Parse(text),
			() => FixParser.Parse(text));

		yield return FixForm(name + ".bytes",
			() => HandFixParser.Parse(bytes),
			() => FixParser.Parse(bytes));

		yield return FixForm(name + ".stream",
			() => HandFixParser.Parse(new MemoryStream(bytes, false)),
			() => FixParser.Parse(new MemoryStream(bytes, false)));
	}

	static Workload FixForm(string name, Func<IEnumerable<FixField>> hand, Func<IEnumerable<FixField>> generated)
	{
		return new Workload(
			"fix",
			name,
			[new Reading("hand", () => Count(hand())), new Reading("generated", () => Count(generated()))],
			() => Differ(hand().Select(Describe), generated().Select(Describe)));

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
	/// The immediate carrier is left out of an untyped lambda: it builds the body before its
	/// types are known and throws where the tape reads (D3, D8).
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
	/// Every reading of a row, warmed, then timed round-robin with the order turned every round,
	/// and the control timed between rounds. A round a generation 1 or 2 collection fell inside
	/// is not a reading of the parser but of the collector, so it is redone rather than kept
	/// (D4: on a few hundred iterations, one such pause dominates the round's average outright).
	/// </summary>
	static Row Measure(Workload workload, List<double> controls)
	{
		foreach (var reading in workload.Readings)
			for (var i = 0; i < 3; i++)
				Time(reading.Run, Iterations(reading.Run));

		var iterations = Iterations(workload.Readings[0].Run);
		var taken      = workload.Readings.Select(_ => new List<double>()).ToArray();
		var redone     = 0;

		for (var round = 0; round < Rounds; round++)
		{
			controls.Add(Time(Control, ControlIterations));

			for (var k = 0; k < workload.Readings.Length; k++)
			{
				var i = round % 2 == 0 ? k : workload.Readings.Length - 1 - k;

				taken[i].Add(TimeSteady(workload.Readings[i].Run, iterations, ref redone));
			}
		}

		if (redone > 0)
			Console.WriteLine($"  {workload.Id}: {redone} round(s) redone after a gen1/gen2 collection");

		var hand = taken[0];

		return new Row(
			workload.Id,
			[.. workload.Readings.Select((reading, i) => new Timed(reading.Name, Median(taken[i]), Allocated(reading.Run)))],
			(hand.Max() - hand.Min()) / Median(hand));
	}

	const int MaxRetries = 5;

	/// <summary>
	/// One round's time, redone up to <see cref="MaxRetries"/> times when a generation 1 or 2
	/// collection ran during it — gen0 is left alone: it is part of ordinary allocation cost,
	/// not the rare pause this filters out.
	/// </summary>
	static double TimeSteady(Func<int> run, int iterations, ref int redone)
	{
		for (var attempt = 0; ; attempt++)
		{
			var gen1    = GC.CollectionCount(1);
			var gen2    = GC.CollectionCount(2);
			var elapsed = Time(run, iterations);

			if (GC.CollectionCount(1) == gen1 && GC.CollectionCount(2) == gen2)
				return elapsed;

			if (attempt == MaxRetries - 1)
				return elapsed;

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

	static readonly string[] FirstRows = ["fix/One.text", "fix/One.bytes", "el/floor", "sql/literal", "sql/select20"];

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

	static string? Root()
	{
		for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
			if (File.Exists(Path.Combine(directory.FullName, "DotGram.slnx")))
				return directory.FullName;

		return null;
	}

	static string Commit(string? root)
	{
		if (root is null)
			return "?";

		var head  = Git(root, "rev-parse --short HEAD");
		var dirty = Git(root, "status --porcelain --untracked-files=no");

		return dirty.Length > 0 ? head + "+changes" : head;
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

		foreach (var reading in row.Readings)
			text.Append(CultureInfo.InvariantCulture, $" {reading.Reading} {reading.Nanoseconds,10:F1} ns");

		foreach (var reading in row.Readings.Skip(1))
			text.Append(CultureInfo.InvariantCulture, $"  {reading.Reading}/hand {reading.Nanoseconds / hand:F2}x");

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
		text.AppendLine("| row | hand ns | reading | ns | /hand | hand B | B | hand spread |");
		text.AppendLine("| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var row in result.Rows)
		{
			var hand = row.Readings[0];

			foreach (var reading in row.Readings.Skip(1))
				text.AppendLine(CultureInfo.InvariantCulture,
					$"| {row.Id} | {hand.Nanoseconds:F1} | {reading.Reading} | {reading.Nanoseconds:F1} | {reading.Nanoseconds / hand.Nanoseconds:F2}x | {hand.Bytes:F0} | {reading.Bytes:F0} | {row.HandSpread:P0} |");
		}

		text.AppendLine();
		text.AppendLine("First call in a fresh process, median of three:");
		text.AppendLine();
		text.AppendLine("| row | reading | ms |");
		text.AppendLine("| --- | --- | ---: |");

		foreach (var call in result.FirstCalls)
			text.AppendLine(CultureInfo.InvariantCulture, $"| {call.Id} | {call.Reading} | {call.Milliseconds:F2} |");

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
