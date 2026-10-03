using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TraceSpike;

/// <summary>A grammar's tables and the three ways to attach a sink to it.</summary>
sealed record Sinks(
	string[] Rules, byte[] Kinds,
	Func<Recorder, IDisposable> Full,
	Func<IDisposable> Null,
	Func<(IDisposable Scope, long[] Counts)> Count);

sealed record Target(string Name, Func<string, object> TryParse, List<string> Seeds, Func<Sinks?> Sinks);

static class Program
{
	static string Repo = "";

	static int Main(string[] args)
	{
		Repo = Environment.GetEnvironmentVariable("SPIKE_REPO") ?? "/ramdisk/agents/dotgram/trace-spike";

		var targets = Targets();

		switch (args[0])
		{
			case "why":
				return Why(targets, args[1]);
			case "time":
				return Time(targets, args[1].Split(','), args[2].Split(','), int.Parse(args[3]));
			case "deep":
				return Deep();
			case "big":
				return Big(targets.Single(one => one.Name == "TSql"));
			case "corpus":
				foreach (var target in targets)
					Console.WriteLine($"{target.Name}\tseeds {target.Seeds.Count}\trefused {Refused(target).Count}");
				return 0;
		}

		return 2;
	}

	static Sinks? NoSinks()
	{
		return null;
	}

	static List<Target> Targets()
	{
		var list = new List<Target>
		{
			new("Twice", s => Twice.TryParseSum(s),
				["1", "1+2", "(1+2)+3", "3^1+(2+4^2)", "((7))+8^8", "12 + (34 + 5^5) + 6"],
#if DOTGRAM_GRAMTRACE
				TwiceSinks.Of),
#else
				NoSinks),
#endif
			new("Url", s => Url.TryParseUrl(s),
				["https://user:pw@example.com:8080/a/b?x=1#frag", "http://[::1]/x", "ftp://192.168.0.1/pub", "http://[2001:db8::7]:80/a"],
#if DOTGRAM_GRAMTRACE
				UrlSinks.Of),
#else
				NoSinks),
#endif
			new("Notation.Primary", s => Notation.TryParsePrimary(s),
				["1", "1()", "42()()()"],
#if DOTGRAM_GRAMTRACE
				NotationSinks.Of),
#else
				NoSinks),
#endif
			new("Notation.Small", s => Notation.TryParseSmall(s),
				["1", "5", "abc!"],
#if DOTGRAM_GRAMTRACE
				NotationSinks.Of),
#else
				NoSinks),
#endif
			new("Web.Json", s => global::DotGram.Web.Rfc8259.TryParseJson(s),
				[
					"{\"a\": [1, 2.5e3, true, null, {\"b\": \"x\\u0041y\"}], \"c\": {}}",
					"[[], {}, \"s\", -0.1, 0, 1E+2]",
					" { \"name\" : \"value\" , \"list\" : [ 1 , 2 , 3 ] } ",
					"\"just a string with \\\"escapes\\\" and \\n\"",
				],
#if DOTGRAM_GRAMTRACE
				JsonSinks.Of),
#else
				NoSinks),
#endif
			new("Web.Uri", s => global::DotGram.Web.Rfc3986.TryParseUri(s),
				[
					"https://user@example.com:8080/a/b%20c?q=1&r=2#frag",
					"urn:isbn:0451450523",
					"mailto:someone@example.org",
					"http://[2001:db8::1]:80/path/to?x",
					"file:///etc/hosts",
				],
#if DOTGRAM_GRAMTRACE
				UriSinks.Of),
#else
				NoSinks),
#endif
			new("EL", s => global::DotGram.ExpressionLanguage.ExpressionParser.TryParse(s, typeof(Program).Assembly),
				ElSeeds(),
#if DOTGRAM_GRAMTRACE
				ElSinks.Of),
#else
				NoSinks),
#endif
			new("TSql", s => global::DotGram.Sql.TransactSql.TransactSqlParser.TryParseSql(s),
				SqlSeeds(),
#if DOTGRAM_GRAMTRACE
				SqlSinks.Of),
#else
				NoSinks),
#endif
		};

		return list;
	}

	// --- corpora --------------------------------------------------------------------------

	static List<string>? _elAll;

	/// <summary>Every input of the expression language's refusal record, accepted and refused.</summary>
	static List<string> ElAll()
	{
		if (_elAll is not null)
			return _elAll;

		_elAll = new List<string>();

		foreach (var line in File.ReadLines(Path.Combine(Repo, "tests/DotGram.Tests/ExpressionLanguage/ExpressionRefusalTests.txt")))
		{
			var text = line.TrimStart('\uFEFF');

			if (!text.StartsWith('"'))
				continue;

			var built = new StringBuilder();
			var at    = 1;

			for (; at < text.Length && text[at] != '"'; at++)
			{
				if (text[at] == '\\' && at + 1 < text.Length)
				{
					at++;

					switch (text[at])
					{
						case 'n': built.Append('\n'); break;
						case 'r': built.Append('\r'); break;
						case 't': built.Append('\t'); break;
						case '0': built.Append('\0'); break;
						case 'u': built.Append((char)Convert.ToInt32(text.Substring(at + 1, 4), 16)); at += 4; break;
						default: built.Append(text[at]); break;
					}
				}
				else
				{
					built.Append(text[at]);
				}
			}

			_elAll.Add(built.ToString());
		}

		return _elAll;
	}

	static List<string> ElSeeds()
	{
		return ElAll().Where(one => Matches.Read(global::DotGram.ExpressionLanguage.ExpressionParser.TryParse(one, typeof(Program).Assembly)).Ok).ToList();
	}

	static List<string> SqlSeeds()
	{
		var seeds = new List<string>();
		var go    = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

		foreach (var file in Directory.GetFiles(Path.Combine(Repo, "tests/Corpus/ScriptDom/TestScripts"), "*.sql").OrderBy(one => one, StringComparer.Ordinal))
		{
			foreach (var chunk in go.Split(File.ReadAllText(file)))
			{
				var text = chunk.Trim();

				if (text.Length == 0 || text.Length > 4000)
					continue;

				if (Matches.Read(global::DotGram.Sql.TransactSql.TransactSqlParser.TryParseSql(text)).Ok)
					seeds.Add(text);
			}
		}

		return seeds;
	}

	/// <summary>Refused inputs made from the accepted ones: cuts, a character left out, one too many.</summary>
	static List<string> Refused(Target target)
	{
		if (target.Name == "EL")
			return ElAll().Where(one => !Matches.Read(target.TryParse(one)).Ok).ToList();

		var random = new Random(42);
		var made   = new HashSet<string>(StringComparer.Ordinal);
		var list   = new List<string>();
		var per    = target.Name == "TSql" ? 3 : 12;
		var cap    = target.Name == "TSql" ? 2000 : 1000;
		var seeds  = target.Seeds;

		if (target.Name == "TSql")
		{
			// A spread of the corpus rather than its first files.
			var step = Math.Max(1, seeds.Count / 600);
			seeds = seeds.Where((_, at) => at % step == 0).ToList();
		}

		void Try(string text)
		{
			if (list.Count < cap && made.Add(text) && !Matches.Read(target.TryParse(text)).Ok)
				list.Add(text);
		}

		foreach (var seed in seeds)
		{
			for (var i = 0; i < per; i++)
			{
				var cut = random.Next(1, Math.Max(2, seed.Length));
				Try(seed.Substring(0, Math.Min(cut, seed.Length - 1 < 1 ? 0 : cut)));

				var gone = random.Next(seed.Length);
				Try(seed.Remove(gone, 1));

				var junk = "@)(;,.'\"x1 "[random.Next(11)];
				Try(seed.Insert(random.Next(seed.Length + 1), junk.ToString()));
			}
		}

		return list;
	}

	// --- why ------------------------------------------------------------------------------

	static int Why(List<Target> targets, string output)
	{
		using var writer = new StreamWriter(output);

		writer.WriteLine("grammar\tindex\tposition\texpected\tcandidates\tkept\tkeptAtMatch\tmismatched\tstacks\terror");

		foreach (var target in targets)
		{
			var sinks    = target.Sinks();
			var refused  = Refused(target);
			var recorder = new Recorder { Mode = Recorder.Kind.Why };

			for (var index = 0; index < refused.Count; index++)
			{
				recorder.Reset();

				object match;

				if (sinks is null)
				{
					match = target.TryParse(refused[index]);
				}
				else
				{
					using (sinks.Full(recorder))
						match = target.TryParse(refused[index]);
				}

				var read  = Matches.Read(match);
				var sets  = new List<string[]>();

				if (read.Expected is not null)
					sets.Add(read.Expected);

				if (read.Tied is not null)
					sets.AddRange(read.Tied);

				// Anchored on the final answer: a candidate is kept where its set is one of the sets the
				// message names, at the furthest position any such candidate stands at.
				var kept = recorder.Candidates
					.Where(one => one.Expected is not null && sets.Any(set => ReferenceEquals(set, one.Expected)))
					.ToList();

				if (kept.Count > 0)
				{
					var furthest = kept.Max(one => one.Position);
					kept = kept.Where(one => one.Position == furthest).ToList();
				}

				var atMatch = kept.Count(one => one.Position == read.Position);
				var stacks  = sinks is null
					? ""
					: string.Join("|", kept.Select(one => string.Join(">", one.Stack.Select(rule => sinks.Rules[rule] + (sinks.Kinds[rule] == 1 ? "~" : "")))).Distinct());
				var said = string.Join(";", sets.Select(set => string.Join(",", set)));

				writer.WriteLine($"{target.Name}\t{index}\t{read.Position}\t{Clean(said)}\t{recorder.Refusals}\t{kept.Count}\t{atMatch}\t{recorder.Mismatched}\t{stacks}\t{Clean(read.Error ?? "").Substring(0, Math.Min(160, (read.Error ?? "").Length))}");
			}

			Console.WriteLine($"{target.Name}: {refused.Count} refused inputs");
		}

		return 0;
	}

	static string Clean(string text)
	{
		return text.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
	}

	// --- time -----------------------------------------------------------------------------

	static int Time(List<Target> targets, string[] names, string[] modes, int samples)
	{
		foreach (var target in targets.Where(one => names.Contains(one.Name)))
		{
			var accepted = target.Seeds;
			var refused  = Refused(target);

			foreach (var (label, inputs) in new[] { ("accepted", accepted), ("refused", refused) })
			{
				var chars = inputs.Sum(one => (long)one.Length);
				var reps  = Math.Max(1, (int)(4_000_000 / Math.Max(1, chars)));

				foreach (var mode in modes)
				{
					var sinks = target.Sinks();

					if (mode != "none" && sinks is null)
						continue;

					var recorder = new Recorder { Mode = Recorder.Kind.Profile };
					var times    = new List<double>();

					for (var sample = -3; sample < samples; sample++)
					{
						var watch = Stopwatch.StartNew();

						for (var rep = 0; rep < reps; rep++)
						{
							IDisposable? scope = mode switch
							{
								"null"    => sinks!.Null(),
								"count"   => sinks!.Count().Scope,
								"profile" => sinks!.Full(recorder),
								_         => null,
							};

							foreach (var input in inputs)
								target.TryParse(input);

							scope?.Dispose();
						}

						watch.Stop();

						if (sample >= 0)
							times.Add(watch.Elapsed.TotalMilliseconds / reps);
					}

					times.Sort();
					var median = times[times.Count / 2];
					var low    = times[0];

					Console.WriteLine($"{target.Name}\t{label}\t{mode}\t{inputs.Count}\t{chars}\t{median:F3}\t{low:F3}");
				}
			}
		}

		return 0;
	}

	// --- big: GramWhy on a refused script of about 100 KB ------------------------------

	static int Big(Target target)
	{
		var text = new StringBuilder();

		foreach (var seed in target.Seeds)
		{
			if (text.Length > 100_000)
				break;

			// Statements only, one batch: a seed that does not read after the others is left out.
			var next = text.Length == 0 ? seed : text + "\n;\n" + seed;

			if (Matches.Read(target.TryParse(next)).Ok)
				text.Clear().Append(next);
		}

		var script = text + "\n;\nSELECT a FROM t WHERE (b = 1;\n";
		var sinks  = target.Sinks();

		Console.WriteLine($"script {script.Length} chars, refused={!Matches.Read(target.TryParse(script)).Ok}");

		foreach (var mode in new[] { "none", "why", "none", "why" })
		{
			if (mode == "why" && sinks is null)
				continue;

			var times = new List<double>();
			var recorder = new Recorder { Mode = Recorder.Kind.Why };
			object match = null!;

			for (var run = 0; run < 7; run++)
			{
				recorder.Reset();
				var watch = Stopwatch.StartNew();

				if (mode == "why")
				{
					using (sinks!.Full(recorder))
						match = target.TryParse(script);
				}
				else
				{
					match = target.TryParse(script);
				}

				times.Add(watch.Elapsed.TotalMilliseconds);
			}

			times.Sort();
			var read = Matches.Read(match);
			Console.WriteLine($"{mode}: median {times[3]:F1} ms, first {times[6]:F1} ms max; enters {recorder.Enters}, refusals {recorder.Refusals}, kept {recorder.Taken.Count}, at {read.Position}");
		}

		return 0;
	}

	// --- deep -----------------------------------------------------------------------------

	static int Deep()
	{
#if DOTGRAM_GRAMTRACE
		var recorder = new Recorder { Mode = Recorder.Kind.Why, WatchThreads = true };

		// Deep enough that the reader hands the read to a stack of its own several times over.
		foreach (var depth in new[] { 100, 20_000, 50_000 })
		{
			var json = new string('[', depth) + "1" + new string(']', depth);
			var bad  = new string('[', depth) + "1," + new string(']', depth);

			foreach (var (label, text) in new[] { ("accepted", json), ("refused", bad) })
			{
				recorder.Reset();
				lock (recorder.Threads) recorder.Threads.Clear();

				object match;

				using (global::DotGram.Web.Rfc8259.Tracing(new JsonFull(recorder)))
					match = global::DotGram.Web.Rfc8259.TryParseJson(text);

				var read = Matches.Read(match);
				var kept = recorder.Candidates.Count(one => read.Expected is not null && ReferenceEquals(one.Expected, read.Expected));
				var deepest = recorder.Taken.Count == 0 ? 0 : recorder.Taken.Max(one => one.Top?.Depth ?? 0);

				Console.WriteLine(
					$"json depth {depth} {label}: ok={read.Ok} at={read.Position} enters={recorder.Enters} exits={recorder.Exits} " +
					$"mismatched={recorder.Mismatched} maxDepth={recorder.MaxDepth} open={recorder.Depth} threads={recorder.Threads.Count} " +
					$"refusals={recorder.Refusals} kept={kept} deepestStack={deepest}");
			}
		}

		// The scope flows into a task, and is restored when disposed.
		{
			recorder.Reset();
			lock (recorder.Threads) recorder.Threads.Clear();

			using (global::DotGram.Web.Rfc8259.Tracing(new JsonFull(recorder)))
				Task.Run(() => global::DotGram.Web.Rfc8259.TryParseJson("[1, {\"a\": 2}]")).Wait();

			var inside = recorder.Enters;

			global::DotGram.Web.Rfc8259.TryParseJson("[1, {\"a\": 2}]");

			Console.WriteLine($"task.run under scope: enters={inside}; after dispose: enters={recorder.Enters - inside}");
		}

		// Nested reads: the expression language parses an interpolation hole from an action.
		{
			recorder.Reset();

			using (global::DotGram.ExpressionLanguage.ExpressionParser.Tracing(new ElFull(recorder)))
				global::DotGram.ExpressionLanguage.ExpressionParser.TryParse("(int x) => $\"{x + 1}\"", typeof(Program).Assembly);

			Console.WriteLine($"EL interpolation: enters={recorder.Enters} exits={recorder.Exits} mismatched={recorder.Mismatched} maxDepth={recorder.MaxDepth}");
		}
#endif

		return 0;
	}
}
