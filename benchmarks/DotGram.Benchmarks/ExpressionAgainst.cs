using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

namespace DotGram.Benchmarks;

/// <summary>
/// The generated expression-language parser against <see cref="HandExpression"/>, measured
/// round-robin.
/// </summary>
/// <remarks>
/// <para>
/// The same shape as <see cref="Standard"/>'s handwritten comparison, and for the same reason:
/// a ratio is only worth printing
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

		// `Math.Max(x, 1)` is the dearest row there is, and these take it apart one piece at
		// a time, the way the parenthesis pair above takes a level of the ladder apart. Over
		// the bare lambda, the first is what recording a `using` costs. Over that, the second
		// is what turning a name into a type costs — `Environment` is read as a variable
		// first and asked about afterwards — with no arguments and no overloads behind it.
		// The imported `Math.Max` row above, over this one, is the overloads. And the dotted
		// name here, against the imported one, is what searching the imports costs.
		"using System; (int x) => x",
		"using System; (int x) => Environment.NewLine",
		"(int x) => System.Math.Max(x, 1)",

		// A call with no type name in it at all, beside the member that pays none of the
		// call's own cost: `s.Length` is a property and `s.Trim()` chooses among three.
		"(string s) => s.Trim()",

		// How the choosing grows with the number of candidates, which is the one thing a
		// ratio on a mixed input cannot say. `ToUpperInvariant` is the only one of its name,
		// `Trim` is three, `Math.Max` is thirteen — the same act each time, over a longer
		// list. It grew linearly, which said the cost was in asking each candidate whether it
		// applies rather than in weighing every pair against every other — and `Trim`, which
		// pays it with no arguments to convert at all, said the asking was reading metadata.
		"(string s) => s.ToUpperInvariant()",

		// The strings that read part of themselves again: holes, each over its own window of
		// the text, and a raw string's lines, which are cut and unindented.
		"(int x) => $\"a{x}b\"",
		"(int x) => $\"{x,5:D3} and {x + 1}\"",
		"(int x) => \"\"\"a\"b\"\"\"",
		"(int x) => $$\"\"\"{{x}} {x}\"\"\"",

		// Lambdas that say no types, whose bodies are read to find where they end and read
		// again once the call they are handed to has chosen their types.
		"using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()",
		"using System.Linq; (int[] a) => a.Where(n => n > 1).Count()",
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
			for (var i = 0; i < Methods.Length; i++)
				Time(input, Methods[i].Measure, iterations);

		foreach (var input in Inputs)
		{
			var taken = new List<double>[Methods.Length];

			for (var i = 0; i < Methods.Length; i++)
				taken[i] = [];

			var costs = new List<double>();

			for (var warm = 0; warm < 2; warm++)
			{
				Time(input, Nothing, iterations);

				for (var i = 0; i < Methods.Length; i++)
					Time(input, Methods[i].Measure, iterations);
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

	/// <summary>What each reading allocates for one parse, which a ratio of times cannot say.</summary>
	/// <remarks>
	/// An allocation is a fact where a time is a measurement, so this needs no rounds, no
	/// warming and no idle machine, and it answers whether a timing experiment is worth
	/// running at all. The measured methods take the input out of the field rather than the
	/// argument — a lambda closing over it would allocate once a call and be measured doing
	/// it — so it is set here as <see cref="Time"/> sets it.
	/// </remarks>
	public static void Bytes(int iterations)
	{
		Agree();

		Console.WriteLine();
		Console.Write($"{"",-40}");

		foreach (var (name, _) in Methods)
			Console.Write($" {name,11}");

		Console.WriteLine("   immediate over hand");

		foreach (var input in Inputs)
		{
			var taken = new double[Methods.Length];

			for (var i = 0; i < Methods.Length; i++)
			{
				_input = input;

				Methods[i].Measure(input);

				var before = GC.GetAllocatedBytesForCurrentThread();
				var sink   = 0;

				for (var one = 0; one < iterations; one++)
					sink += Methods[i].Measure(input);

				taken[i] = (GC.GetAllocatedBytesForCurrentThread() - before) / (double)iterations;
				_sink    = sink;
			}

			var shown = input.Length <= 38 ? input : input.Substring(0, 35) + "...";

			Console.Write($"{shown,-40}");

			foreach (var one in taken)
				Console.Write(double.IsNaN(one) ? $" {"-",8}   " : $" {one,8:N0} b ");

			Console.WriteLine(double.IsNaN(taken[1]) ? "" : $"    {taken[1] - taken[2],+8:N0} b");
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
					"hand"      => Read(Reading.Hand)      ? 1 : 0,
					"immediate" => Read(Reading.Immediate) ? 1 : 0,
					_           => Read(Reading.Tape)      ? 1 : 0,
				};

		Console.WriteLine($"{read:N0} {reading} readings of \"{text}\"");
	}

	static readonly (string Name, Func<string, int> Measure)[] Methods =
	[
		("generated", static input => Read(Reading.Tape)      ? 1 : 0),
		("immediate", static input => Read(Reading.Immediate) ? 1 : 0),
		("by hand",   static input => Read(Reading.Hand)      ? 1 : 0),
		("its lexer", static input => HandExpression.LexOnly(input)),
	];

	// The measured methods take the input from the field, because a lambda that closes
	// over it would allocate once per call and be measured doing it.
	static string _input = "";

	/// <summary>Who every reading is on behalf of, asked once: asking each reading is a stack crawl, and the same one for all three.</summary>
	static readonly Assembly Caller = typeof(ExpressionAgainst).Assembly;

	/// <summary>A reading of one text, on behalf of <see cref="Caller"/>.</summary>
	static ExpressionParser.State Fresh(string text)
	{
		return new ExpressionParser.State(Caller) { Text = text };
	}

	static bool Read(Reading which)
	{
		var state = new ExpressionParser.State(Caller) { Text = _input };

		try
		{
			return which switch
			{
				Reading.Hand      => HandExpression.TryParseLambda(_input, state).IsSuccess,
				Reading.Immediate => ExpressionParser.Immediate.TryParseLambda(_input, state).IsSuccess,
				_                 => ExpressionParser.TryParseLambda(_input, state).IsSuccess,
			};
		}
		catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
		{
			return false;
		}
	}

	/// <summary>
	/// The three answering the same, before anything is timed. Throws rather than warns: a
	/// ratio measured against a parser that reads a different language is worse than no ratio.
	/// </summary>
	/// <remarks>
	/// The hand-written parser is held to the tape over the whole shared corpus and over what
	/// is timed, answer for answer — the tree, or where and how the text was refused
	/// (<see cref="ExpressionCorpus.Answer"/>). The immediate carrier is held to the tape over the
	/// same, with one allowance its contract makes: it runs the constructions of a derivation it
	/// then abandons, so on text the tape refuses it may throw one of the refusals the host
	/// throws instead of refusing (<see cref="Agrees"/>).
	/// </remarks>
	public static void Agree()
	{
		foreach (var text in ExpressionCorpus.Shapes.Concat(Inputs))
		{
			var tape   = ExpressionCorpus.Answer(text, ExpressionParser.TryParseLambda, Fresh(text));
			var handed = ExpressionCorpus.Answer(text, HandExpression.TryParseLambda, Fresh(text));

			if (tape != handed)
				throw new InvalidOperationException(
					$"About \"{text}\": the generated parser and the hand-written one disagree.\n" +
					$"  generated {tape}\n" +
					$"  by hand   {handed}");
		}

		foreach (var text in ExpressionCorpus.Shapes.Concat(Inputs))
		{
			var tape      = ExpressionCorpus.Answer(text, ExpressionParser.TryParseLambda, Fresh(text));
			var immediate = ExpressionCorpus.Answer(text, ExpressionParser.Immediate.TryParseLambda, Fresh(text));

			if (!Agrees(tape, immediate))
				throw new InvalidOperationException(
					$"About \"{text}\": the tape and the immediate carrier read one grammar differently.\n" +
					$"  tape      {tape}\n" +
					$"  immediate {immediate}");
		}

		Console.WriteLine(
			$"The hand-written parser answers as the tape over {ExpressionCorpus.Shapes.Length + Inputs.Length} shapes, " +
			"and so does the immediate carrier.");
	}

	/// <summary>Whether the immediate carrier answers as the tape does.</summary>
	/// <remarks>
	/// The same tree where the tape builds one, and the same refusal where it refuses — or a
	/// refusal thrown instead: the carrier runs the constructions of a derivation it then
	/// abandons, and one of them may throw what the host throws for text it refuses
	/// (CarrierKind.Immediate; the architect's ruling of 2026-09-18).
	/// </remarks>
	static bool Agrees(string tape, string immediate) =>
		tape == immediate ||
		tape.StartsWith("refused ", StringComparison.Ordinal) && immediate.StartsWith("threw ", StringComparison.Ordinal);

	enum Reading { Tape, Immediate, Hand }

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
			Console.Write(double.IsNaN(median) ? $" {"-",8}   " : $" {median,8:N1} ns");

		Console.WriteLine(
			$"   {medians[0] / medians[2],8:N2}x " +
			(double.IsNaN(medians[1]) ? $"{"-",10}" : $"{medians[1] / medians[2],9:N2}x"));
	}

	static double Median(List<double> times)
	{
		var ordered = times.Order().ToArray();

		return ordered.Length % 2 == 1
			? ordered[ordered.Length / 2]
			: (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2;
	}
}
