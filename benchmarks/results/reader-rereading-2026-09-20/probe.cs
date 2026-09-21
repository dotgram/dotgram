using DotGram.Web;

namespace RecProbe
{
	public static class C
	{
		public static long Calls, Fails, Begins, Opens, Retries;
		public static long[] ByRule = new long[8192];
		public static void Enter(int rule, int pos) { Calls++; ByRule[rule]++; }
		public static int Fail(int rule, int pos) { Fails++; return -1; }

		// D63: a failing turn restores the log to a mark, and the next attempt writes into the
		// same place. If what it writes is always what was discarded, the roll-back and the
		// rewriting are both waste on this path, and a replay could keep the records instead of
		// making them again. If it ever differs, they cannot be kept and the cheap fix is the
		// ceiling. Keyed by the mark, because that is the place being written twice.
		static readonly Dictionary<(int Mark, int At), int[]> Discarded = new();
		public static long Rollbacks, Rewrites, Same, Different;
		public static int DifferentAt = -1;

		// How much of a rewritten segment is the same as the one discarded. A give-back reads
		// one turn fewer, so the tail must differ - but if the leading records are identical
		// every time, they are repeated work and could be kept, and only the tail rewritten.
		public static long PrefixKept, PrefixTotal;

		public static void Rolled(int[] log, int from, int to, int at)
		{
			Rollbacks++;

			var length = to - from;
			if (length <= 0)
				return;

			var now = new int[length];
			Array.Copy(log, from, now, 0, length);

			if (Discarded.TryGetValue((from, at), out var before))
			{
				Rewrites++;
				if (before.AsSpan().SequenceEqual(now))
					Same++;
				else
				{
					Different++;
					if (DifferentAt < 0) DifferentAt = at;
				}

				var common = 0;
				while (common < before.Length && common < now.Length && before[common] == now[common])
					common++;

				PrefixKept  += common;
				PrefixTotal += before.Length;
			}

			Discarded[(from, at)] = now;
		}

		public static void Reset()
		{
			Calls = Fails = Begins = Opens = Retries = 0;
			Rollbacks = Rewrites = Same = Different = 0;
			PrefixKept = PrefixTotal = 0;
			DifferentAt = -1;
			Discarded.Clear();
			Array.Clear(ByRule);
		}
	}

	static class Program
	{
		static void Main()
		{
			Console.WriteLine("Does what a failing turn discards come back the same?");
			Console.WriteLine();
			Console.WriteLine("shape                                          n  begins  rollbacks  rewritten   same   differ   shared prefix");

			Ask("addresses, an unclosed quoted string (refuses)",
				n => "\"" + new string('a', n),
				text => EmailAddress.TryParseList(text, out _));

			Ask("addresses, a valid list (accepts)",
				n => string.Join(", ", Enumerable.Range(0, n).Select(one => $"a{one}@example.com")),
				text => EmailAddress.TryParseList(text, out _));

			Ask("media type, parameters then unclosed string (refuses)",
				n => "text/plain" + Repeat(";a=b", n) + ";c=\"",
				text => MediaType.TryParse(text, out _));

			Ask("language tag, variants then a bare hyphen (refuses)",
				n => "en" + Repeat("-1abc", n) + "-",
				text => LanguageTag.TryParse(text, out _));

			Ask("structured field, items then an unclosed ( (refuses)",
				n => string.Join(", ", Enumerable.Range(0, n).Select(one => $"a{one}")) + ", (",
				text => StructuredField.TryParseList(text, out _));
		}

		static string Repeat(string one, int times) =>
			string.Concat(Enumerable.Repeat(one, times));

		static void Ask(string title, Func<int, string> shape, Func<string, bool> parse)
		{
			parse(shape(4));

			foreach (var n in new[] { 16, 64 })
			{
				C.Reset();
				parse(shape(n));
				var differ = C.Different == 0 ? "none" : $"{C.Different:N0} (first at position {C.DifferentAt})";
				var shared = C.PrefixTotal == 0 ? "-" : $"{100.0 * C.PrefixKept / C.PrefixTotal:F1}% of {C.PrefixTotal:N0}";
				Console.WriteLine($"{title,-46} {n,3} {C.Begins,7:N0} {C.Rollbacks,10:N0} {C.Rewrites,10:N0} {C.Same,6:N0}   {differ,-28} {shared}");
			}
		}
	}
}
