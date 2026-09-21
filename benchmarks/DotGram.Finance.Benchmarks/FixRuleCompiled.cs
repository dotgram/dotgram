using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using DotGram.ExpressionLanguage;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// The composed rules compiled and run: first held against the walk they replace, then timed
/// beside it.
/// </summary>
/// <remarks>
/// <para>
/// The comparison is through the consumer's own call — <c>message.Validate(validator)</c> on both
/// sides — and not over the tables the two roads read. Two readings of one dictionary can agree
/// about every table and still disagree about a message, and the message is what anybody has.
/// </para>
/// <para>
/// Findings are compared POSITION BY POSITION, not as sets. Two validators that report the same
/// things in a different order are a difference a reader sees, and a comparison that sorts them
/// away is a comparison that cannot see it.
/// </para>
/// <para>
/// Nothing is timed until both sides answer the same, because a rule that is faster because it
/// does less is not faster.
/// </para>
/// </remarks>
static class FixRuleCompiled
{
	public static void Run(string[] args)
	{
		var rounds = args.Length > 0 ? int.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture) : 9;
		var repeat = args.Length > 1 ? int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture) : 200;
		var only   = args.Length > 2 ? args[2].Split(',') : null;

		var root = Repository();

		FixDictionary dictionary;

		using (var input = File.OpenRead(Path.Combine(root, "tests", "Corpus", "Fix", "FIX44.xml")))
			dictionary = FixDictionary.Load(input);

		var loaded = new FixValidator();

		loaded.Load(dictionary);

		var composer = new FixRuleComposer(dictionary);
		var clock    = Stopwatch.StartNew();
		var composed = Compile(dictionary, composer);
		var built    = clock.ElapsedMilliseconds;

		var compiled = new FixValidator();

		compiled.Load(composed);

		Console.WriteLine($"{composed.Count} rules composed, compiled and installed in {built:N0} ms.");

		var messages = Fixtures(root);

		Console.WriteLine($"{messages.Count} fixtures.");

		var disagreed = 0;
		var findings  = 0;

		foreach (var message in messages)
		{
			var walked = message.Validate(loaded);
			var ran    = message.Validate(compiled);

			findings += walked.Length;

			if (Same(walked, ran))
				continue;

			if (disagreed++ < 10)
				Console.WriteLine(
					$"  {message.MessageType}: walk [{string.Join("; ", walked)}] vs composed [{string.Join("; ", ran)}]");
		}

		Console.WriteLine($"{findings} findings over the fixtures; {disagreed} messages disagree.");

		if (disagreed != 0)
		{
			Console.WriteLine("not timed: the two sides do not answer the same.");

			return;
		}

		// A session sees a few message types over and over, not ninety-three in turn, and the
		// difference is not a detail: the rules are big, so which of them are hot is the
		// measurement's main variable. Naming types on the command line is how that is asked.
		var corpus = only is null
			? messages.ToArray()
			: messages.Where(message => only.Contains(message.MessageType, StringComparer.Ordinal)).ToArray();

		Console.WriteLine($"timing {corpus.Length} messages of {corpus.Select(m => m.MessageType).Distinct().Count()} types.");

		Warm(corpus, loaded,   repeat);
		Warm(corpus, compiled, repeat);

		var walk = new double[rounds];
		var made = new double[rounds];

		for (var round = 0; round < rounds; round++)
		{
			walk[round] = Nanoseconds(Time(corpus, loaded,   repeat), corpus.Length * repeat);
			made[round] = Nanoseconds(Time(corpus, compiled, repeat), corpus.Length * repeat);
		}

		Array.Sort(walk);
		Array.Sort(made);

		Console.WriteLine($"walked   median {walk[rounds / 2]:F0} ns/message, fastest {walk[0]:F0}, slowest {walk[rounds - 1]:F0}");
		Console.WriteLine($"composed median {made[rounds / 2]:F0} ns/message, fastest {made[0]:F0}, slowest {made[rounds - 1]:F0}");
		Console.WriteLine($"composed/walked {made[rounds / 2] / walk[rounds / 2]:F2}x, by medians");

		// Per message type, against how many arms that type's switch has. A ratio that climbs with
		// the arms says the cost is in the switch; a flat one says it is in what every scope does.
		if (only is not null)
			return;

		Console.WriteLine();
		Console.WriteLine("type                 arms  walked  composed  ratio");

		foreach (var group in corpus.GroupBy(message => message.MessageType).OrderBy(one => one.Key, StringComparer.Ordinal))
		{
			var one  = group.ToArray();
			var arms = composer.Arms(group.Key);
			var a    = Fastest(one, loaded,   repeat, rounds);
			var b    = Fastest(one, compiled, repeat, rounds);

			Console.WriteLine($"{group.Key,-20} {arms,5} {a,7:F0} {b,9:F0} {b / a,6:F2}");
		}
	}

	static Dictionary<string, FixMessageRule> Compile(FixDictionary dictionary, FixRuleComposer composer)
	{
		var tables   = new DictionaryTables(dictionary);
		var assembly = typeof(FixMessage).Assembly;
		var rules    = new Dictionary<string, FixMessageRule>(StringComparer.Ordinal);

		foreach (var type in dictionary.MessageTypes)
		{
			var text  = FixRuleText.Blank(composer.Rule(type));

			// Compiled as an Action and wrapped afterwards. The language builds the lambda the text
			// describes, and System.Linq.Expressions will not hand back an Action<A, B> as a
			// FixMessageRule even though the two have one signature; the usual way round that —
			// rebinding the method to the named type — is refused as well, because a lambda
			// compiled at run time has no runtime MethodInfo to rebind.
			//
			// So there is one delegate hop per message in this measurement that a package doing
			// the same thing would want to be rid of: what it costs is below.
			var build  = ExpressionParser.Compile<Func<FixTables, Action<FixMessage, List<FixFinding>>>>(text, assembly);
			var action = build(tables);

			rules.Add(type, (message, found) => action(message, found));
		}

		return rules;
	}

	static List<FixMessage> Fixtures(string root)
	{
		var file     = Path.Combine(root, "tests", "DotGram.Finance.Tests", "Fixtures.json");
		var messages = new List<FixMessage>();

		using var fixtures = JsonDocument.Parse(File.ReadAllText(file));

		foreach (var fixture in fixtures.RootElement.EnumerateArray())
		{
			var wire = fixture.GetProperty("wire").GetString()!;

			if (FixMessages.TryParse(wire, out var message, out _))
				messages.Add(message!);
		}

		return messages;
	}

	static bool Same(FixFinding[] walked, FixFinding[] composed)
	{
		if (walked.Length != composed.Length)
			return false;

		for (var i = 0; i < walked.Length; i++)
			if (!walked[i].Equals(composed[i]))
				return false;

		return true;
	}

	/// <summary>The fastest of several rounds over one type's messages, in nanoseconds each.</summary>
	static double Fastest(FixMessage[] corpus, FixValidator validator, int repeat, int rounds)
	{
		var best = double.MaxValue;

		for (var round = 0; round < rounds; round++)
			best = Math.Min(best, Nanoseconds(Time(corpus, validator, repeat), corpus.Length * repeat));

		return best;
	}

	static void Warm(FixMessage[] corpus, FixValidator validator, int repeat)
	{
		var clock = Stopwatch.StartNew();

		while (clock.Elapsed < TimeSpan.FromSeconds(2))
			Time(corpus, validator, repeat);
	}

	static long Time(FixMessage[] corpus, FixValidator validator, int repeat)
	{
		var sink  = 0;
		var clock = Stopwatch.StartNew();

		for (var pass = 0; pass < repeat; pass++)
			foreach (var message in corpus)
				sink += message.Validate(validator).Length;

		clock.Stop();

		if (sink == int.MinValue)
			throw new InvalidOperationException("Unreachable, and here so that the loop is not elided.");

		return clock.ElapsedTicks;
	}

	static double Nanoseconds(long ticks, int operations) =>
		ticks * (1_000_000_000.0 / Stopwatch.Frequency) / operations;

	static string Repository()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
	}
}
