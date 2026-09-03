using System;
using System.Diagnostics;
using System.Text;

namespace DotGram.HandDeferred;

/// <summary>
/// Two safe readings of <c>Deferred.gram</c>, side by side: what they answer, and what
/// they cost.
/// </summary>
static class Program
{
	static void Main(string[] args)
	{
		if (args.Length >= 1 && args[0] == "--time")
		{
			var pairs  = args.Length >= 2 && int.TryParse(args[1], out var many) ? many : 400;
			var rounds = args.Length >= 3 && int.TryParse(args[2], out var each) ? each : 2_000;

			Time(pairs, rounds);

			return;
		}

		foreach (var input in new[] { "a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", "" })
			Show(input);
	}

	static void Show(string input)
	{
		Console.WriteLine($"\"{input}\"");

		Author.Forget();

		var reader = new Reader(input);
		var read   = reader.Recognize();

		Console.WriteLine($"  tape:     read {read}, and the author's code has run {Author.Constructions} times");

		foreach (var line in reader.Describe().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
			Console.WriteLine("            " + line);

		if (read)
			Console.WriteLine($"            value \"{reader.Construct()}\", after {Author.Constructions} of them");

		Author.Forget();

		var closures = new Closures(input);
		var arranged = closures.Recognize();

		Console.WriteLine($"  closures: read {arranged}, and the author's code has run {Author.Constructions} times");

		if (arranged)
			Console.WriteLine($"            value \"{closures.Construct()}\", after {Author.Constructions} of them");

		Author.Forget();

		var unboxed = new Unboxed(input);
		var held    = unboxed.Recognize();

		Console.WriteLine($"  structs:  read {held}, and the author's code has run {Author.Constructions} times");

		if (held)
			Console.WriteLine($"            value \"{unboxed.Construct()}\", after {Author.Constructions} of them");

		Author.Forget();

		var boxed  = new Boxed(input);
		var inside = boxed.Recognize();

		Console.WriteLine($"  boxed:    read {inside}, and the author's code has run {Author.Constructions} times");

		if (inside)
			Console.WriteLine($"            value \"{boxed.Construct()}\", after {Author.Constructions} of them");

		Author.Forget();

		var classes = new Classes(input);
		var linked  = classes.Recognize();

		Console.WriteLine($"  classes:  read {linked}, and the author's code has run {Author.Constructions} times");

		if (linked)
			Console.WriteLine($"            value \"{classes.Construct()}\", after {Author.Constructions} of them");

		Console.WriteLine();
	}

	/// <summary>
	/// Both readings over the same input, phase by phase.
	/// </summary>
	/// <remarks>
	/// The phases are timed apart because together they say nothing. This grammar's own
	/// construction is <c>l + "+" + r</c> over a left-leaning fold, which is quadratic in
	/// the number of pairs and allocates near a megabyte for a four-kilobyte input — the
	/// author's half swamps the parser's half, and both readings look the same because
	/// most of what is being timed is neither of them. Apart, recognition shows what the
	/// representation costs to build and construction shows what it costs to walk.
	/// </remarks>
	static void Time(int pairs, int rounds)
	{
		var input = Input(pairs);

		Console.WriteLine($"{pairs} pairs, {input.Length} characters, {rounds} rounds\n");

		// Round-robin rather than one after the other, so that a machine which is not idle
		// tilts both the same way.
		var read   = new Phase();
		var built  = new Phase();
		var piled  = new Phase();

		var arranged = new Phase();
		var called   = new Phase();

		var held   = new Phase();
		var folded = new Phase();

		var inside = new Phase();
		var opened = new Phase();

		var linked = new Phase();
		var walked = new Phase();

		for (var round = 0; round < rounds + 200; round++)
		{
			var measured = round >= 200;

			{
				var reader = new Reader(input);
				var watch  = Phase.Start();

				if (!reader.Recognize())
					throw new InvalidOperationException("The tape reader did not read the input.");

				if (measured) read.Add(watch);

				watch = Phase.Start();

				var value = reader.Construct();

				if (measured) built.Add(watch);

				GC.KeepAlive(value);

				watch = Phase.Start();

				var piledValue = reader.ConstructOnAStack();

				if (measured) piled.Add(watch);

				GC.KeepAlive(piledValue);
			}

			{
				var closures = new Closures(input);
				var watch    = Phase.Start();

				if (!closures.Recognize())
					throw new InvalidOperationException("The closure reader did not read the input.");

				if (measured) arranged.Add(watch);

				watch = Phase.Start();

				var value = closures.Construct();

				if (measured) called.Add(watch);

				GC.KeepAlive(value);
			}

			{
				var unboxed = new Unboxed(input);
				var watch   = Phase.Start();

				if (!unboxed.Recognize())
					throw new InvalidOperationException("The struct reader did not read the input.");

				if (measured) held.Add(watch);

				watch = Phase.Start();

				var value = unboxed.Construct();

				if (measured) folded.Add(watch);

				GC.KeepAlive(value);
			}

			{
				var boxed = new Boxed(input);
				var watch = Phase.Start();

				if (!boxed.Recognize())
					throw new InvalidOperationException("The boxed reader did not read the input.");

				if (measured) inside.Add(watch);

				watch = Phase.Start();

				var value = boxed.Construct();

				if (measured) opened.Add(watch);

				GC.KeepAlive(value);
			}

			{
				var classes = new Classes(input);
				var watch   = Phase.Start();

				if (!classes.Recognize())
					throw new InvalidOperationException("The class reader did not read the input.");

				if (measured) linked.Add(watch);

				watch = Phase.Start();

				var value = classes.Construct();

				if (measured) walked.Add(watch);

				GC.KeepAlive(value);
			}
		}

		Console.WriteLine(Unboxed.Sizes());
		Console.WriteLine();

		Console.WriteLine($"{"",-24}{"per parse",12}{"allocated",14}{"ratio",9}");
		Console.WriteLine();

		Row("recognize, tape",     read,     read);
		Row("recognize, closures", arranged, read);
		Row("recognize, structs",  held,     read);
		Row("recognize, boxed",    inside,   read);
		Row("recognize, classes",  linked,   read);
		Console.WriteLine();
		Row("construct, table",    built,    built);
		Row("construct, stack",    piled,    built);
		Row("construct, closures", called,   built);
		Row("construct, structs",  folded,   built);
		Row("construct, boxed",    opened,   built);
		Row("construct, classes",  walked,   built);

		void Row(string what, Phase phase, Phase against) =>
			Console.WriteLine(
				$"{what,-24}{phase.Micros(rounds),10:N1} us{phase.Bytes(rounds),12:N0} B" +
				$"{(double)phase.Ticks / against.Ticks,8:N2}x");
	}

	/// <summary>One phase of one reading, summed over the rounds that were measured.</summary>
	sealed class Phase
	{
		public long Ticks;

		long _bytes;

		public static (long Ticks, long Bytes) Start() =>
			(Stopwatch.GetTimestamp(), GC.GetAllocatedBytesForCurrentThread());

		public void Add((long Ticks, long Bytes) from)
		{
			Ticks  += Stopwatch.GetTimestamp() - from.Ticks;
			_bytes += GC.GetAllocatedBytesForCurrentThread() - from.Bytes;
		}

		public double Micros(int rounds) => Ticks * 1_000_000.0 / Stopwatch.Frequency / rounds;

		public long Bytes(int rounds) => _bytes / rounds;
	}

	/// <summary><c>ab = 0 + ab = 1 + …</c>, which is the shape that makes the fold deep.</summary>
	static string Input(int pairs)
	{
		var written = new StringBuilder();

		for (var i = 0; i < pairs; i++)
		{
			if (i > 0)
				written.Append(" + ");

			written.Append("ab = ").Append(i % 10);
		}

		return written.ToString();
	}
}
