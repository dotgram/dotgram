using DotGram.Sql;
using DotGram.Sql.Standard;

namespace RecProbe
{
	public static class C
	{
		public static long Calls, Fails;
		public static long[] ByRule = new long[8192];

		public static void Enter(int rule, int pos)
		{
			Calls++;
			ByRule[rule]++;
		}

		// The instrument also hands over what a failing turn discarded; this probe is only
		// about how much work is done, so it counts the roll-back and looks at nothing.
		public static long Rollbacks, Begins, Opens, Retries, Writes;
		public static int Deepest;

		public static void Wrote(int ints, int reaches)
		{
			Writes += ints;

			if (reaches > Deepest)
				Deepest = reaches;
		}

		public static void Rolled(int[] log, int from, int to, int at)
		{
			Rollbacks++;
		}

		public static int Fail(int rule, int pos)
		{
			Fails++;

			return -1;
		}

		public static void Reset()
		{
			Calls = Fails = Rollbacks = Begins = Opens = Retries = Writes = 0;
			Deepest = 0;
			Array.Clear(ByRule);
		}
	}

	static class Program
	{
		static void Main()
		{
			// The six the stand times, by the same text and the same entry point: taken from
			// SqlStandardBenchmarks rather than invented again, so the count is about the rows
			// that were measured and not about inputs that resemble them.
			Console.WriteLine("How much work the generated parser does, per input and per token.");
			Console.WriteLine();
			Console.WriteLine("input             tokens        calls   calls/token      fails  rollbacks   log writes  deepest");

			Ask("literal", "1", text => SqlStandardParser.TryParseLiteral(text, out _));
			Ask("arithmetic", "(a + b) * c - d / 5", text => SqlStandardParser.TryParseValueExpression(text, out _));
			Ask("select1", "SELECT a FROM t", text => SqlStandardParser.TryParseQueryExpression(text, out _));
			Ask("select20", Select(20), text => SqlStandardParser.TryParseQueryExpression(text, out _));
			Ask("conditions100", Conditions(100), text => SqlStandardParser.TryParseSearchCondition(text, out _));
			Ask("conditions1000", Conditions(1000), text => SqlStandardParser.TryParseSearchCondition(text, out _));
		}

		static string Select(int columns)
		{
			return "SELECT " + string.Join(", ", Enumerable.Range(0, columns).Select(static at => "a" + at)) +
				" FROM t WHERE a0 = 1";
		}

		static string Conditions(int many)
		{
			return string.Join(" AND ", Enumerable.Range(0, many).Select(static at => "a" + at + " = " + at));
		}

		static void Ask(string name, string text, Func<string, bool> parse)
		{
			// Once to settle anything a first call builds, then the reading that counts.
			parse(text);
			C.Reset();

			var read = parse(text);

			if (!read)
				throw new InvalidOperationException($"'{name}' did not parse; the count would be about a refusal.");

			// The stand names its rows by the input's length, which is what "tokens" means in
			// its table: the same number, so the two can be put side by side.
			var n = text.Length;

			Console.WriteLine($"{name,-16} {n,6:N0} {C.Calls,12:N0} {C.Calls / (double)n,13:F2} {C.Fails,10:N0} {C.Rollbacks,10:N0} {C.Writes,12:N0} {C.Deepest,8:N0}");
		}
	}
}
