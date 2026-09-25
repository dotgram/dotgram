using DotGram.Sql.Standard;

namespace RecProbe
{
	public static class C
	{
		public static long[] ByRule = new long[8192];

		public static void Reset()
		{
			Array.Clear(ByRule);
		}

		public static long Total()
		{
			var all = 0L;

			foreach (var one in ByRule)
				all += one;

			return all;
		}
	}

	static class Program
	{
		// The input StackDepthTests uses and the counts were taken through, and the same closed:
		// the open one is refused, and whether the quadratic needs the refusal is the question.
		static void Main()
		{
			// The reading runs on a thread with room, so that nothing is carried onto a stack of
			// its own: a hand-off inside the reading would be counted as the reading's work.
			var thread = new Thread(Count, 256 * 1024 * 1024);

			thread.Start();
			thread.Join();
		}

		static void Count()
		{
			foreach (var shape in new[] { "refused", "closed" })
				foreach (var n in new[] { 200, 400 })
					Ask(shape, n, 1);

			// A filed count of the refused input came out exactly twice these numbers, while the
			// accepted one agreed to the digit. A factor of two on the refusal ONLY cannot be a
			// double-counted reading, which would double both. What differs between the two forms of
			// a publication is exactly that: `Match<T>` reads a refused input a second time to
			// record what was expected where, and `TryParse(out T)` does not (the package's own
			// release notes: "a refusal costs it about half"). So the two forms are counted here.
			Console.WriteLine();
			Console.WriteLine("### the refused input through the Match<T> form, which records a message");

			foreach (var n in new[] { 200, 400 })
				Ask("refused", n, 1, match: true);
		}

		static void Ask(string shape, int n, int readings, bool match = false)
		{
			var text = shape == "closed"
				? new string('(', n) + "a = 1" + new string(')', n)
				: new string('(', n) + "a = 1";

			// Once to settle what a first call builds, then the reading that counts.
			if (match)
				SqlStandardParser.TryParseSearchCondition(text);
			else
				SqlStandardParser.TryParseSearchCondition(text, out _);
			C.Reset();

			var read = false;

			for (var again = 0; again < readings; again++)
				read = match
					? SqlStandardParser.TryParseSearchCondition(text).IsSuccess
					: SqlStandardParser.TryParseSearchCondition(text, out _);

			Console.WriteLine();
			Console.WriteLine($"== {shape} n={n}, {(match ? "Match<T>" : "TryParse(out)")}: {text.Length} chars, {(read ? "read" : "refused")}, " +
				$"{C.Total():N0} entries, {C.Total() / (double)text.Length:F2} a char");

			var order = Enumerable.Range(0, Names.All.Length)
				.Where(at => C.ByRule[at] > 0)
				.OrderByDescending(at => C.ByRule[at])
				.Take(14);

			foreach (var at in order)
				Console.WriteLine($"{C.ByRule[at],13:N0}  {Names.All[at]}");
		}
	}
}
