using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// What validating a built message costs, by the two roads a validator can come from.
/// </summary>
/// <remarks>
/// Taken before the dictionary road is rebuilt (D92): today <c>Load(dictionary)</c> fills the
/// table with a closure that walks the dictionary's own tables, and the plan is to compose that
/// walk as text and compile it through the expression language instead. A number taken after the
/// rebuild has nothing to stand against unless one is taken before it, on this tree, which is
/// what this mode is for.
///
/// Two sides, measured in one process and interleaved, because the question is their ratio and a
/// ratio taken from two processes measures the machine as much as the code:
///
/// * <c>Standard</c> — <c>message.Validate()</c>, this package's own FIX 4.4 tables compiled into
///   the assembly. It is not what changes, which is exactly why it is here: it is the control, and
///   a run where it moves says the run is not to be believed.
/// * <c>Dictionary</c> — <c>message.Validate(loaded)</c>, over <c>tests/Corpus/Fix/FIX44.xml</c>.
///   This is the side the expression language would replace.
///
/// The two do not answer the same findings — their tables disagree about 26 message compositions
/// — so the check before timing is not that the counts match but that each side is stable: a side
/// whose answer changes between rounds is measuring something other than its rules.
/// </remarks>
static class FixValidationBenchmarks
{
	public static void Run(string[] args)
	{
		var rounds = args.Length > 0 ? int.Parse(args[0], CultureInfo.InvariantCulture) : 7;
		var repeat = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 2000;

		var messages = Corpus();
		var loaded   = Loaded();

		Console.WriteLine($"{messages.Length} messages, {rounds} rounds of {repeat} passes each.");

		// Both sides answer once before anything is timed, and the answers are what every later
		// round is held to: a validator that stops finding what it found is not faster, it is
		// broken, and a timing loop is the last place that shows.
		var standard   = messages.Sum(message => message.Validate().Length);
		var dictionary = messages.Sum(message => message.Validate(loaded).Length);

		Console.WriteLine($"findings: standard {standard}, dictionary {dictionary}");

		// Per message, because a corpus that finds nothing times the walk that finds nothing, and
		// the two are not the same price. Printed rather than asserted: what each side finds is
		// its own business, and the run is about how long finding it takes.
		foreach (var message in messages)
			Console.WriteLine(
				$"  {message.MessageType,-2} {message.OriginalWire.Length,4} octets: " +
				$"standard {message.Validate().Length}, dictionary {message.Validate(loaded).Length}");

		// Warm-up is timed, not counted. One round of each was not enough: the first four rounds
		// of a seven-round run came back five times slower than the last three, which is tier-0
		// code being measured and then promoted in the middle of the run. So both sides are run
		// until the clock says two seconds have gone into each, which is past the promotion.
		Warm(messages, null,   repeat, TimeSpan.FromSeconds(2));
		Warm(messages, loaded, repeat, TimeSpan.FromSeconds(2));

		var perMessage = messages.Length * repeat;
		var sides      = (Standard: new double[rounds], Dictionary: new double[rounds]);

		for (var round = 0; round < rounds; round++)
		{
			var a = Time(messages, null, repeat);
			var b = Time(messages, loaded, repeat);

			if (a.Findings != standard || b.Findings != dictionary)
				throw new InvalidOperationException($"Round {round} answered {a.Findings}/{b.Findings}.");

			sides.Standard[round]   = Nanoseconds(a.Ticks, perMessage);
			sides.Dictionary[round] = Nanoseconds(b.Ticks, perMessage);

			Console.WriteLine(
				$"round {round}: standard {sides.Standard[round]:F0} ns/message, " +
				$"dictionary {sides.Dictionary[round]:F0} ns/message");
		}

		// The median, and the extremes beside it rather than instead of it. A fifteen-round run
		// of this pair produced two rounds 40% under every other round on one side and not on the
		// other, which is not the code being faster for a moment; a best-of picks exactly those
		// and reports them as the answer.
		Report("standard  ", sides.Standard);
		Report("dictionary", sides.Dictionary);

		Console.WriteLine($"dictionary/standard {Median(sides.Dictionary) / Median(sides.Standard):F2}x, by medians");
	}

	static void Report(string side, double[] rounds) =>
		Console.WriteLine(
			$"{side}: median {Median(rounds):F0} ns/message, fastest round {rounds.Min():F0}, slowest {rounds.Max():F0}");

	static double Median(double[] rounds)
	{
		var sorted = (double[])rounds.Clone();

		Array.Sort(sorted);

		return sorted.Length % 2 == 1
			? sorted[sorted.Length / 2]
			: (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;
	}

	static void Warm(FixMessage[] messages, FixValidator? validator, int repeat, TimeSpan budget)
	{
		var clock = Stopwatch.StartNew();

		while (clock.Elapsed < budget)
			Time(messages, validator, repeat);
	}

	/// <summary>
	/// How much of validating a message is reading the characters of its values, which is the
	/// part no way of writing the rules can remove.
	/// </summary>
	/// <remarks>
	/// The question behind it: compiling the rules replaces table lookups with constants, so the
	/// most it can win is what the lookups cost — and a share that is mostly value reading puts a
	/// ceiling on the whole road before a line of it is written. Measured against the same corpus
	/// the run above times, by difference: walking every field and doing nothing, then walking
	/// every field and holding its value to its type. The code sets are left out, so this is a
	/// floor for the value share and therefore a CEILING for what compiling could win.
	/// </remarks>
	public static void Share(string[] args)
	{
		var rounds = args.Length > 0 ? int.Parse(args[0], CultureInfo.InvariantCulture) : 15;
		var repeat = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 20000;

		var messages = Corpus();
		var fields   = messages.Sum(message => message.AllFields.Count());
		var typed    = messages.Sum(message => message.AllFields.Count(field => FixValues.Type(field.Tag) != FixValueType.None));

		Console.WriteLine($"{messages.Length} messages, {fields} fields, {typed} of them with a type the schema gives.");

		Warm(messages, walk: true,  repeat, TimeSpan.FromSeconds(2));
		Warm(messages, walk: false, repeat, TimeSpan.FromSeconds(2));

		var perMessage = messages.Length * repeat;
		var walking    = new double[rounds];
		var checking   = new double[rounds];

		for (var round = 0; round < rounds; round++)
		{
			walking[round]  = Nanoseconds(Walked(messages, walk: true,  repeat), perMessage);
			checking[round] = Nanoseconds(Walked(messages, walk: false, repeat), perMessage);
		}

		Report("walk only ", walking);
		Report("walk+value", checking);

		Console.WriteLine(
			$"the values cost {Median(checking) - Median(walking):F0} ns/message of the " +
			$"{Median(checking):F0} ns this walk takes");
	}

	static void Warm(FixMessage[] messages, bool walk, int repeat, TimeSpan budget)
	{
		var clock = Stopwatch.StartNew();

		while (clock.Elapsed < budget)
			Walked(messages, walk, repeat);
	}

	static long Walked(FixMessage[] messages, bool walk, int repeat)
	{
		var sink  = 0L;
		var clock = Stopwatch.StartNew();

		for (var pass = 0; pass < repeat; pass++)
			foreach (var message in messages)
				foreach (var field in message.AllFields)
					if (walk)
					{
						sink += field.Tag;
					}
					else
					{
						var type = FixValues.Type(field.Tag);

						if (type != FixValueType.None)
							sink += FixValues.Valid(field, type, null) ? 1 : 0;
					}

		clock.Stop();

		if (sink == long.MinValue)
			throw new InvalidOperationException("Unreachable, and here so that the loop is not elided.");

		return clock.ElapsedTicks;
	}

	static (long Ticks, int Findings) Time(FixMessage[] messages, FixValidator? validator, int repeat)
	{
		var findings = 0;
		var clock    = Stopwatch.StartNew();

		for (var pass = 0; pass < repeat; pass++)
			foreach (var message in messages)
				findings += validator is null ? message.Validate().Length : message.Validate(validator).Length;

		clock.Stop();

		return (clock.ElapsedTicks, findings / repeat);
	}

	static double Nanoseconds(long ticks, int operations) =>
		ticks * (1_000_000_000.0 / Stopwatch.Frequency) / operations;

	/// <summary>Messages of several shapes, valid and not, built once and validated many times.</summary>
	/// <remarks>
	/// A valid message and a broken one cost different amounts — the second finds things, and
	/// finding is where the work is — so both are here, and so is one carrying a repeating group,
	/// which is the walk's only recursive part.
	/// </remarks>
	static FixMessage[] Corpus() =>
	[
		FixMessages.Parse(Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|")),
		FixMessages.Parse(Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|453=2|448=A|447=D|452=1|448=B|447=D|452=2|")),
		FixMessages.Parse(Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=9|60=not-a-time|38=abc|40=2|")),
		FixMessages.Parse(Fix44Benchmarks.Wire("8", "37=ORDER|17=EXEC|150=0|39=0|55=ABC|54=1|38=100|14=0|6=0|")),
		FixMessages.Parse(Fix44Benchmarks.Wire("A", "98=0|108=30|")),
	];

	static FixValidator Loaded()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		if (at is null)
			throw new InvalidOperationException("The repository root was not found above the benchmark's output.");

		var validator = new FixValidator();

		using (var file = File.OpenRead(Path.Combine(at.FullName, "tests", "Corpus", "Fix", "FIX44.xml")))
			validator.Load(FixDictionary.Load(file));

		return validator;
	}
}
