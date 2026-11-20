using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace DotGram.Ir.Prototype;

/// <summary>
/// Refused inputs that grow, read at doubling sizes, with what each reading cost counted: the
/// statements the evaluator ran, the decisions it opened or read back, the readings asked for
/// again, the turns it went straight to and the host calls it made.
/// </summary>
/// <remarks>
/// A count, not a clock: twice the turns at twice the count is a reading that costs its length,
/// at four times its square. The generated C# parser is read beside it, timed (a direction on a
/// shared machine) and with its host calls counted.
/// </remarks>
static class Ladders
{
	/// <summary>A refused input of a fixture, as long as its turns.</summary>
	public sealed record Shape(string Name, string Fixture, Func<int, string> Input);

	/// <summary>What one reading of one size cost; <see cref="Over"/> where it ran past its budget and was stopped.</summary>
	public sealed record Rung(int Turns, long Steps, long Entered, long Retries, long Jumps, long HostCalls, string Line, bool Over)
	{
		/// <summary>The most records the evaluator's store held at once, stale ones included; 0 where not counted.</summary>
		public long Stored { get; init; }
	}

	public static readonly int[] Sizes = [1000, 2000, 4000, 8000, 16000, 32000, 64000];

	static string Times(string piece, int count)
	{
		return string.Concat(Enumerable.Repeat(piece, count));
	}

	public static readonly Shape[] Shapes =
	[
		new("loop",            "Loop",          static n => Times("a,", n) + "!"),
		new("settled config",  "Config",        static n => Times("k=ab;", n) + "!"),
		new("nested settled",  "NestedSettled", static n => Times("12;", n) + Times("a,", n) + "!"),
		new("arithmetic",      "Arithmetic",    static n => Times("1+", n)),
		new("(a) settled",     "SettledBefore", static n => Times("0", n) + Times("a,", n) + "!"),
		new("(b) sealed",      "SealedBefore",  static n => Times("0", n) + Times("a,", n) + "!"),
		new("(c) called",      "CalledBefore",  static n => Times(" ", n) + Times("a,", n) + "!"),
		new("(a') failed turn",       "FailedTurnBefore",    static n => Times("a", n) + "!"),
		new("nested braces",          "NestedSealedBefore",  static n => "<" + Times("0", n) + ">" + Times("a,", n) + "!"),
		new("long look",              "LongLookBefore",      static n => Times("0", n) + Times("a,", n) + "!"),
		new("braces with captures",   "SealedCaptureBefore", static n => Times("0", n) + Times("x", n) + ";" + Times("a,", n) + "!"),
		new("(e) deciding nest",      "DecidingNestBefore",  static n => Times("(", n) + "y" + Times(")", n - 1) + Times("a,", n) + "!"),
		new("back-reference",         "SameBefore",          static n => Times("0", n) + "=" + Times("0", n) + Times("a,", n) + "!"),
		new("yaml line",              "YamlLineBefore",      static n => Times(" ", n) + Times("1", n) + ":" + Times("a,", n) + "!"),
		new("(d) deep split",         "DeepSplit",           static n => Times("(", n) + Times("b", n) + Times(")", n)),
		new("(d) control: shallow",   "DeepSplit",           static n => "(" + Times("b", n) + ")"),
		new("(d) control: deep, two", "DeepSplit",           static n => Times("(", n) + "bb" + Times(")", n)),
		new("(d) deep split, no way back up", "DeepSplitOpen", static n => Times("(", n) + Times("b", n)),
		new("records below a cut",    "StaleRecord",         static n => Times("a", n) + Times("b", n) + "c"),
		new("config, inner replays",  "Config",              static n => Times("k=ax;", n) + "!"),
	];

	/// <summary>
	/// A host that only counts its calls: what a ladder reads, where writing every call down would
	/// cost the square of the input in text whatever the reading costs.
	/// </summary>
	public sealed class CountingHost(Func<IrFragment, object?[], bool>? guards) : IIrHost
	{
		static readonly V Built = new("built");

		public long Calls { get; private set; }

		public object? Construct(IrFragment fragment, object?[] arguments)
		{
			Calls++;

			return Built;
		}

		public bool Guard(IrFragment fragment, object?[] arguments)
		{
			Calls++;

			return guards?.Invoke(fragment, arguments) ?? true;
		}
	}

	/// <summary>The shape read by the evaluator at every size, until a reading runs past the budget.</summary>
	public static List<Rung> Evaluated(Shape shape, IrLowering.Options? options, bool resumes, long budget, IReadOnlyList<int>? sizes = null)
	{
		var fixture = Cases.Fixtures[shape.Fixture];
		var program = Reading.Program(fixture.Grammar, options);
		var rungs   = new List<Rung>();

		foreach (var turns in sizes ?? Sizes)
		{
			var host      = new CountingHost(fixture.Guards);
			var evaluator = new IrEvaluator(program, host) { Resumes = resumes, StepBudget = budget };

			try
			{
				var outcome = evaluator.Parse(fixture.Entry, shape.Input(turns));

				rungs.Add(new Rung(turns, evaluator.Steps, evaluator.Ways.Entered, evaluator.Ways.Retries, evaluator.Jumps + evaluator.Skips, host.Calls, outcome.Accepted ? "A " + outcome.Position.ToString(CultureInfo.InvariantCulture) : outcome.Line, false)
				{
					Stored = evaluator.MostStored,
				});
			}
			catch (IrStepsExceeded)
			{
				rungs.Add(new Rung(turns, evaluator.Steps, evaluator.Ways.Entered, evaluator.Ways.Retries, evaluator.Jumps + evaluator.Skips, host.Calls, "", true));

				break;
			}
		}

		return rungs;
	}

	/// <summary>The shape read by the generated C# parser at every size, until a reading takes longer than <paramref name="most"/>.</summary>
	public static List<(int Turns, double Milliseconds, long HostCalls, string Line)> Generated(Shape shape, TimeSpan most, IReadOnlyList<int>? sizes = null)
	{
		var fixture = Cases.Fixtures[shape.Fixture];
		var method  = fixture.Parser.GetMethod("Try" + fixture.Entry, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
		var rungs   = new List<(int, double, long, string)>();

		foreach (var turns in sizes ?? Sizes)
		{
			var input = shape.Input(turns);

			object? match = null;

			var watch = new Stopwatch();

			List<string> log;

			try
			{
				// Once to warm, once timed.
				Twin.Record(() => method.Invoke(null, [input]));

				watch.Start();
				log = Twin.Record(() => match = method.Invoke(null, [input]));
				watch.Stop();
			}
			catch (TargetInvocationException thrown)
			{
				rungs.Add((turns, watch.Elapsed.TotalMilliseconds, 0, "T " + thrown.InnerException?.GetType().Name));

				break;
			}

			var type     = match!.GetType();
			var accepted = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;
			var position = (long)type.GetProperty("Position")!.GetValue(match)!;

			rungs.Add((turns, watch.Elapsed.TotalMilliseconds, log.Count, (accepted ? "A " : "R ") + position.ToString(CultureInfo.InvariantCulture)));

			if (watch.Elapsed > most)
				break;
		}

		return rungs;
	}

	/// <summary>
	/// The shape read by a printed parser at every size, in one process, with the parser's own
	/// counters: TypeScript in the frames given, or C where <paramref name="frames"/> is null.
	/// </summary>
	public static List<Rung> Printed(Shape shape, IrLowering.Options? options, SliceFrames? frames, IReadOnlyList<int>? sizes = null)
	{
		var fixture = Cases.Fixtures[shape.Fixture];
		var program = Reading.Program(fixture.Grammar, options);
		var turns   = sizes ?? Sizes;
		var inputs  = turns.Select(shape.Input).ToList();
		var name    = "Ladder-" + shape.Fixture + (options is null ? "" : "-open");

		// Stacks for the deepest nest a ladder reads, sixty-four thousand levels.
		var answers = frames is { } kept
			? Slices.TypeScript(name, program, fixture.Entry, inputs, frames: kept, counted: true, stackKilobytes: 1_000_000, systemStackKilobytes: 2_000_000)
			: Slices.InC(name, program, fixture.Entry, inputs, counted: true, stackMegabytes: 2048);

		return [.. answers.Select((answer, i) =>
		{
			var counts = answer.Counts ?? throw new InvalidOperationException("The printed parser said no counts.");

			return new Rung(turns[i], counts[0], counts[1], counts[2], counts[3], counts[4], answer.Head, false);
		})];
	}

	/// <summary>How much more the last rung cost than the one before, by a count; zero where there are not two.</summary>
	public static double Ratio(IReadOnlyList<Rung> rungs, Func<Rung, long> count)
	{
		var whole = rungs.Where(static rung => !rung.Over).ToList();

		return whole.Count < 2 || count(whole[^2]) == 0 ? 0 : count(whole[^1]) / (double)count(whole[^2]);
	}

	public static string Table(string title, IReadOnlyList<Rung> rungs)
	{
		var lines = new List<string> { "# " + title, "turns\tsteps\tentered\tretries\tjumps\thost calls\tstore peak\tanswer" };

		foreach (var rung in rungs)
		{
			lines.Add(string.Create(CultureInfo.InvariantCulture,
				$"{rung.Turns}\t{(rung.Over ? ">" : "")}{rung.Steps}\t{rung.Entered}\t{rung.Retries}\t{rung.Jumps}\t{rung.HostCalls}\t{rung.Stored}\t{(rung.Over ? "over budget" : rung.Line)}"));
		}

		return string.Join("\n", lines) + "\n";
	}
}
