using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

namespace DotGram.Benchmarks;

/// <summary>
/// The generated expression-language parser against <see cref="HandExpression"/>, measured
/// round-robin.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SqlAgainst"/>'s twin, and for the same reason: a ratio is only worth printing
/// after the two have been shown to read the same language and build the same tree, and
/// only round-robin, so that a machine that grows warm or busy moves both numbers together.
/// </para>
/// <para>
/// Three readings are timed. The tape is what ships. The immediate carrier is the same
/// grammar with the constructions run where they are read, which is what the hand-written
/// parser does. And the hand-written one is the mark: a lexer over the whole input and a
/// recursive descent over the tokens, calling the same factories and handing the same
/// <see cref="ExpressionParser.State"/> the same spans.
/// </para>
/// </remarks>
static class ExpressionAgainst
{
	/// <summary>
	/// What is timed, graded by depth the way <see cref="ExpressionBenchmarks"/> grades it:
	/// the floor, the ladder, a nest, a block, and the refusals a backtracking reader does
	/// its worst work on.
	/// </summary>
	static readonly string[] Inputs =
	[
		"(int x) => x",
		"(int x) => x * x - 1",
		"(int x, int y) => (x + y) * 3 - x / 5",
		"(string s) => s.Length",
		"using System; (int x) => Math.Max(x, 1)",
		"(int x) => { x += 1; x *= 2; return x; }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }",
		"(int x) => ((((x + 1) + 1) + 1) + 1)",
		"(int x) => x * x -",

		// One parenthesis more each time, and nothing else: what a parenthesis costs is
		// one descent of the operator ladder, so the slope of these four is what a level
		// of it costs each reading.
		"(int x) => x",
		"(int x) => (((x)))",
		"(int x) => (((((x)))))",
		"(int x) => (((((((x)))))))",

		// The same shape twice over, once with a literal for the right operand and once
		// with a name: what separates the two rows is one number, and for a while what
		// separated them was a hundred nanoseconds of reading that number again with a
		// machine of its own. Kept because the pair is the shortest proof there is that a
		// parenthesis and an operator cost the generated reader what they cost a person.
		"(int x) => (x + 1)",
		"(int x) => (((x + 1) + 1) + 1)",

		"(int x) => (x + x)",
		"(int x) => (((x + x) + x) + x)",
	];

	/// <summary>
	/// What the two are held to before anything is timed. Every part of the language the
	/// hand-written parser claims to read is written here once, and every shape that told
	/// the two apart while it was being written stayed.
	/// </summary>
	static readonly string[] Corpus =
	[
		// The floor, the ladder, and every level of it.
		"(int x) => x",
		"(int x) => x + 1",
		"(int x) => x * x - 1",
		"(int x) => x * (x - 1)",
		"(int x, int y) => (x + y) * 3 - x / 5",
		"(int x) => x % 3",
		"(int x) => x < 1 || x > 9",
		"(int x) => x > 0 && x < 9",
		"(int x) => x >= 0 && x <= 9",
		"(int x) => x == 1 || x != 2",
		"(int x) => (x & 3) | (x ^ 1)",
		"(int x) => x << 2",
		"(int x) => x >> 2",
		"(int x) => x << 2 >> 1",
		"(int x) => -x",
		"(int x) => +x",
		"(int x) => ~x",
		"(bool b) => !b",
		"(int x) => - -x",
		"(int x) => 1 - -x",

		// Association and precedence, on every pair of levels that could be confused.
		"(int a, int b, int c) => a - b + c",
		"(int a, int b, int c) => a + b * c",
		"(int a, int b, int c) => a * b / c",
		"(int a, int b, int c) => a / b * c",
		"(int a, int b, int c) => a << b >> c",
		"(bool a, bool b, bool c) => a || b && c",
		"(bool a, bool b, bool c) => a && b || c",
		"(int a, int b) => a < b == true",
		"(int a, int b, int c) => a & b | c",
		"(int a, int b, int c) => a | b ^ c",

		// Right associativity, and the two that have it.
		"(int x) => x > 1 ? x : -x",
		"(int x) => x > 1 ? 1 : x > 0 ? 2 : 3",
		"(string s) => s ?? \"none\"",
		"(string s, string t) => s ?? t ?? \"none\"",

		// Assignment, plain and compound, to a name, a member and an element.
		"(int x) => { x = 1; x }",
		"(int x) => { x += 1; x -= 2; x *= 3; x /= 4; x %= 5; x }",
		"(int x) => { x &= 1; x |= 2; x ^= 3; x <<= 1; x >>= 1; x }",
		"(int[] a) => { a[0] = 1; a[0] }",

		// Members, calls, indices, and the chains they make.
		"(string s) => s.Length",
		"(string s) => s.Trim()",
		"(string s) => s.Trim().Length",
		"(string s) => s.Substring(1, 2)",
		"(int[] a) => a.Length + a[0]",
		"(int[][] a) => a[0][1]",
		"using System; (int x) => Math.Max(x, 1)",
		"(int x) => System.Math.Max(x, 1)",
		"using System; (int x) => Math.PI > x",
		"(string s) => string.Concat(s, s)",

		// The types, which are what a cast and a declaration are told apart by.
		"(int x) => (long)x",
		"(double d) => (int)d + 1",
		"(int x) => (object)x",
		"(object o) => o as string",
		"(object o) => o is string",
		"(int x) => new int[3]",
		"(int x) => new int[] { x, 1 }",
		"(int x) => new int[] { }",
		"(int x) => new string('a', x)",

		// Statements, and the scopes they open.
		"(int x) => { int y = x + 1; return y * y; }",
		"(int x) => { int y = 1; { int z = 2; x += z; } x + y }",
		"(int x) => { if (x > 0) return 1; return 0; }",
		"(int x) => { if (x > 0) x = 1; else x = 2; x }",
		"(int x) => if (x > 0) 1 else 2",
		"(int x) => { int y = if (x > 0) 1 else 2; y }",
		"(int x) => { while (x < 10) { x += 1; } return x; }",
		"(int x) => { do { x += 1; } while (x < 10); x }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { if (i % 2 == 0) continue; sum += i; } sum }",
		"(int n) => { while (true) { if (n > 3) break; n += 1; } n }",
		"(int n) => { switch (n) { case 1: n = 10; break; default: n = 0; break; } n }",

		// The marks, which change what is built and nothing about what is read.
		"(int x) => checked(x + 1)",
		"(int x) => unchecked(x * 2)",
		"(int x) => checked(x + unchecked(x * 2))",
		"(int x) => checked(x + 1) + x",

		// The constants, every form of them.
		"(int x) => 1",
		"(int x) => 2147483648",
		"(int x) => 1u",
		"(int x) => 1L",
		"(int x) => 1UL",
		"(int x) => 1_000_000",
		"(int x) => 0x1F",
		"(int x) => 0xFFu",
		"(int x) => 0b1010",
		"(int x) => 1.5",
		"(int x) => .5",
		"(int x) => 1e3",
		"(int x) => 1.5e-3",
		"(int x) => 1.5f",
		"(int x) => 1.5d",
		"(int x) => 1.5m",
		"(int x) => 1m",
		"(bool b) => true",
		"(bool b) => false",
		"(string s) => null",
		"(string s) => \"text\"",
		"(string s) => \"a\\tb\\n\"",
		"(string s) => \"\\u0041\"",
		"(string s) => @\"a\"\"b\"",
		"(char c) => 'a'",
		"(char c) => '\\n'",

		// What is left of the language: the constructs a person writes rarely and a
		// parser has to read all the same.
		"using System; (int x) => { try { x += 1; } catch (Exception e) { x = 0; } x }",
		"using System; (int x) => { try { x += 1; } catch (Exception e) { x = 0; } finally { x += 1; } x }",
		"(int x) => { try { x += 1; } finally { x += 1; } x }",
		"using System; (int x) => { if (x < 0) throw new Exception(\"no\"); x }",
		"(int x) => new System.Collections.Generic.List<int>()",
		"(int x) => new System.Collections.Generic.List<int> { x, 1 }",
		"(int x) => new System.Collections.Generic.Dictionary<int, string> { { x, \"a\" } }",
		"(int x) => new System.Text.StringBuilder(16).Length",
		"(int x) => System.Convert.ToString(x)",

		// A `using`, two of them, and a nested type reached through one.
		"using System.Text; (int x) => new StringBuilder(16).Length",
		"using System; using System.Text; (int x) => Math.Max(new StringBuilder(x).Length, 1)",
		"using System; (Environment.SpecialFolder f) => f",
		"(int x) => { int[] a = new int[2]; a[0] = x; a[0] }",
		"(int x) => ++x",
		"(int x) => --x",
		"(int x) => x++",
		"(int x) => x--",

		// And the refusals: a refusal is an answer, and has to be the same answer.
		"(int x) => x *",
		"(int x) =>",
		"(int x) => { x += 1;",
		"(int x) => y",
		"(int x) => x.NoSuchMember",
		"using System.Nowhere; (int x) => x",
		"(int x) => Math.Max(x, 1)",
		"(int x) => (",
		"(int x) => )",
		"int x => x",
		"(int x) x",
		"",
		"(int x) => x > > 1",
		"(int x) => x | | 1",
	];

	public static void Run(int rounds, int iterations)
	{
		Agree();

		Console.WriteLine();
		Console.Write($"{"",-40}");

		foreach (var (name, _) in Methods)
			Console.Write($" {name,11}");

		Console.WriteLine("   tape/hand  immediate/hand");

		// Every method over every input before any of them is measured. Warming one input
		// at a time leaves the first one paying for the whole process coming up to speed:
		// it read a third slower than the same input read again at the end, which is a
		// third of the difference this table is about.
		foreach (var input in Inputs)
			foreach (var (_, measure) in Methods)
				Time(input, measure, iterations);

		foreach (var input in Inputs)
		{
			var taken = new List<double>[Methods.Length];

			for (var i = 0; i < Methods.Length; i++)
				taken[i] = [];

			var costs = new List<double>();

			for (var warm = 0; warm < 2; warm++)
			{
				Time(input, Nothing, iterations);

				foreach (var (_, measure) in Methods)
					Time(input, measure, iterations);
			}

			for (var round = 0; round < rounds; round++)
			{
				costs.Add(Time(input, Nothing, iterations));

				for (var i = 0; i < Methods.Length; i++)
					taken[i].Add(Time(input, Methods[i].Measure, iterations));
			}

			var overhead = Median(costs);

			Report(input, [.. taken.Select(times => Median(times) - overhead)]);
		}
	}

	/// <summary>
	/// One input read over and over, for a profiler rather than for a number.
	/// </summary>
	/// <remarks>
	/// One reading at a time, and the same input: three parsers through one process would
	/// share every line a profile is attributed to, and two inputs through one parser
	/// would share the parser's.
	/// </remarks>
	public static void Spin(int seconds, int which, string reading)
	{
		var text  = Inputs[which];
		var until = DateTime.UtcNow.AddSeconds(seconds);
		var read  = 0;

		_input = text;

		while (DateTime.UtcNow < until)
			for (var i = 0; i < 2000; i++)
				read += reading switch
				{
					"hand"      => HandExpression.Parse(text) ? 1 : 0,
					"immediate" => Read(true)  ? 1 : 0,
					_           => Read(false) ? 1 : 0,
				};

		Console.WriteLine($"{read:N0} {reading} readings of \"{text}\"");
	}

	static readonly (string Name, Func<string, int> Measure)[] Methods =
	[
		("generated", static input => Read(false) ? 1 : 0),
		("immediate", static input => Read(true)  ? 1 : 0),
		("by hand",   static input => HandExpression.Parse(input) ? 1 : 0),
		("its lexer", static input => HandExpression.LexOnly(input)),
	];

	// The measured methods take the input from the field, because a lambda that closes
	// over it would allocate once per call and be measured doing it.
	static string _input = "";

	static bool Read(bool immediate)
	{
		var state = new ExpressionParser.State();

		try
		{
			return immediate
				? ExpressionParser.Immediate.TryParseLambda(_input, state).IsSuccess
				: ExpressionParser.TryParseLambda(_input, state).IsSuccess;
		}
		catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
		{
			return false;
		}
	}

	/// <summary>
	/// The three answering the same about every shape, before anything is timed. Throws
	/// rather than warns: a ratio measured against a parser that reads a different language
	/// is worse than no ratio.
	/// </summary>
	public static void Agree()
	{
		foreach (var text in Corpus.Concat(Inputs))
		{
			var tape      = Shown(text, Reading.Tape);
			var immediate = Shown(text, Reading.Immediate);
			var handed    = Shown(text, Reading.Hand);

			if (tape != immediate)
				throw new InvalidOperationException(
					$"About \"{text}\": the tape and the immediate carrier read one grammar differently.\n" +
					$"  tape      {tape}\n" +
					$"  immediate {immediate}");

			if (tape != handed)
				throw new InvalidOperationException(
					$"About \"{text}\": the generated parser and the hand-written one disagree.\n" +
					$"  generated {tape}\n" +
					$"  by hand   {handed}");
		}

		Console.WriteLine(
			$"All three read the same language and build the same tree over " +
			$"{Corpus.Length + Inputs.Length} shapes.");
	}

	enum Reading { Tape, Immediate, Hand }

	/// <summary>What one of the three makes of the text, as it prints, or why it made nothing.</summary>
	static string Shown(string text, Reading which)
	{
		try
		{
			LambdaExpression? lambda;

			if (which == Reading.Hand)
			{
				lambda = HandExpression.Build(text);
			}
			else
			{
				var state = new ExpressionParser.State();
				var match = which == Reading.Immediate
					? ExpressionParser.Immediate.TryParseLambda(text, state)
					: ExpressionParser.TryParseLambda(text, state);

				lambda = match.IsSuccess ? match.Value : null;
			}

			return lambda is null ? "<refused>" : lambda.ToString();
		}
		catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
		{
			// What the API refuses it refuses in its own words, and both readings meet it
			// the same way. The type of the refusal is the answer; its message is the
			// API's and may name the reading that reached it.
			return "<" + exception.GetType().Name + ">";
		}
	}

	static int Nothing(string input) => input.Length & 1;

	/// <remarks>
	/// Nothing is done about the collector between samples, and that was tried: settling
	/// the heap before each one costs the sample its warm caches, which for the lexer —
	/// a hundred nanoseconds a call — was worth more than the noise it removed. What the
	/// noise wanted was longer samples, and the default iteration count is what gives them.
	/// </remarks>
	static double Time(string input, Func<string, int> measure, int iterations)
	{
		_input = input;

		var watch = Stopwatch.StartNew();
		var sink  = 0;

		for (var i = 0; i < iterations; i++)
			sink += measure(input);

		watch.Stop();
		_sink = sink;

		return watch.Elapsed.TotalMilliseconds * 1e6 / iterations;
	}

	static int _sink;

	static void Report(string input, IReadOnlyList<double> medians)
	{
		var shown = input.Length <= 38 ? input : input.Substring(0, 35) + "...";

		Console.Write($"{shown,-40}");

		foreach (var median in medians)
			Console.Write($" {median,8:N1} ns");

		Console.WriteLine($"   {medians[0] / medians[2],8:N2}x {medians[1] / medians[2],9:N2}x");
	}

	static double Median(List<double> times)
	{
		var ordered = times.Order().ToArray();

		return ordered.Length % 2 == 1
			? ordered[ordered.Length / 2]
			: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}
}
