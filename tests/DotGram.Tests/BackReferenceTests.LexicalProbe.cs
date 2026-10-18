using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Breaker probe: grammars cut in two (Lexical = true) with bindings in their terminals, held to
/// the scannerless twin of the same grammar over the same string. Cases come from a file named by
/// DOTGRAM_LEXPROBE (skipped where unset), so that new cases need no rebuild.
/// </summary>
public sealed class LexicalBindingProbe
{
	[ThreadStatic] internal static int Begun;
	[ThreadStatic] internal static int Other;
	[ThreadStatic] internal static int Windowed;

	internal sealed record Built(Assembly? Assembly, string Note, bool Cut);

	internal static Built Build(string grammar, bool lexical, bool direct)
	{
		GramCompilation result;

		try
		{
			result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Lexical       = lexical,
				Direct        = direct,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});
		}
		catch (Exception e)
		{
			return new Built(null, "GENERATOR THROW " + e.GetType().Name + ": " + e.Message, false);
		}

		var notes = string.Join("; ", result.Diagnostics
			.Where(static one => one.Severity != GramSeverity.Info)
			.Select(static one => one.Id + "@" + one.Position + " " + Short(one.Message)));

		if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error))
			return new Built(null, "REFUSED " + notes, false);

		var source = result.Sources[0].Text;
		var cut    = source.Contains("Tokenize_DotGram(", StringComparison.Ordinal);

		try
		{
			var assembly = EmittedCode.Compile(source, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

			return new Built(assembly, notes, cut);
		}
		catch (Exception e)
		{
			return new Built(null, "C# COMPILE FAIL " + Short(e.Message, 600), false);
		}
	}

	static string Short(string text, int most = 200)
	{
		text = text.Replace("\n", " ");

		return text.Length <= most ? text : text[..most] + "…";
	}

	internal static string Spelled(object? value)
	{
		return value switch
		{
			null => "<null>",
			string text => text,
			System.Collections.IEnumerable many => "[" + string.Join(",", many.Cast<object?>().Select(Spelled)) + "]",
			_ => value.ToString()!,
		};
	}

	internal static string Run(Assembly assembly, string method, string input)
	{
		try
		{
			var (ok, value, _, position) = EmittedCode.Match(assembly, "Grammar", method, input);

			return ok ? $"ok «{Escape(Spelled(value))}» @{position}" : $"no @{position}";
		}
		catch (TargetInvocationException e)
		{
			return "THROW " + e.InnerException!.GetType().Name + ": " + Short(e.InnerException.Message);
		}
	}


	internal static string At(Assembly assembly, string method, string input, int at)
	{
		try
		{
			var found = assembly.GetType("Grammar")!.GetMethod(method, [typeof(string), typeof(int)]);

			if (found is null)
				return "n/a";

			var match = found.Invoke(null, [input, at])!;
			var type  = match.GetType();
			var ok    = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;

			return ok ? $"ok «{Escape(Spelled(type.GetProperty("Value")!.GetValue(match)))}» @{type.GetProperty("Position")!.GetValue(match)}" : "no";
		}
		catch (TargetInvocationException e)
		{
			return "THROW " + e.InnerException!.GetType().Name + ": " + Short(e.InnerException.Message);
		}
	}

	internal static string Escape(string text)
	{
		return text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r");
	}

	static string Unescape(string text)
	{
		var built = new StringBuilder();

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] == '\\' && at + 1 < text.Length)
			{
				at++;
				built.Append(text[at] switch { 'n' => '\n', 't' => '\t', 'r' => '\r', 's' => ' ', 'e' => '\0', _ => text[at] });
			}
			else
				built.Append(text[at]);
		}

		return built.ToString().Replace("\0", "");
	}

	internal static IEnumerable<string> All(string alphabet, int most)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= most; length++)
		{
			var next = new List<string>();

			foreach (var prefix in level)
				foreach (var one in alphabet)
				{
					next.Add(prefix + one);

					yield return prefix + one;
				}

			level = next;
		}
	}

	/// <summary>The disagreements of one grammar over its inputs; the first line says how it built.</summary>
	internal static List<string> Compare(string grammar, string method, IEnumerable<string> inputs, bool values, out string header, int most = 30)
	{
		var told  = new List<string>();
		var clock = System.Diagnostics.Stopwatch.StartNew();
		var over  = Build(grammar, lexical: false, direct: true);
		var t1    = clock.ElapsedMilliseconds;
		var engineOnly = Environment.GetEnvironmentVariable("DOTGRAM_CHARS_ENGINE") is not null;
		var read  = Build(grammar, lexical: !engineOnly, direct: true);
		var t2    = clock.ElapsedMilliseconds - t1;
		var run   = Build(grammar, lexical: !engineOnly, direct: false);
		var t3    = clock.ElapsedMilliseconds - t1 - t2;

		header = $"build ms chars {t1}, reader {t2}, engine {t3}\n  chars: {(over.Assembly is null ? over.Note : "ok " + over.Note)}\n  lexical reader: {(read.Assembly is null ? read.Note : (read.Cut ? "CUT " : "NOT CUT ") + read.Note)}\n  lexical engine: {(run.Assembly is null ? run.Note : (run.Cut ? "CUT " : "NOT CUT ") + run.Note)}";

		if (over.Assembly is null || read.Assembly is null || run.Assembly is null)
			return told;

		var count = 0;

		foreach (var input in inputs)
		{
			count++;

			var a = Run(over.Assembly, method, input);
			var b = Run(read.Assembly, method, input);
			var c = Run(run.Assembly, method, input);

			string Key(string answer)
			{
				if (answer.StartsWith("no", StringComparison.Ordinal))
					return "no";

				return values ? answer : answer[..2] + answer[answer.LastIndexOf('@')..];
			}

			var refusedOnly = a.StartsWith("ok", StringComparison.Ordinal) && b.StartsWith("no", StringComparison.Ordinal) && c.StartsWith("no", StringComparison.Ordinal);

			if (Key(a) != Key(b) || Key(a) != Key(c) || b.StartsWith("THROW", StringComparison.Ordinal) || c.StartsWith("THROW", StringComparison.Ordinal))
			{
				if (refusedOnly)
					Begun++;
				else
					Other++;

				if (told.Count < most)
					told.Add($"  \"{Escape(input)}\": chars {a} | reader {b} | engine {c}");
				else if (told.Count == most)
					told.Add("  …");
			}
		}

		// The windowed form: the input twice, each half as a window.
		foreach (var input in inputs.Take(4000))
		{
			var doubled = input + input;

			foreach (var at in new[] { 0, input.Length })
			{
				string W(Assembly assembly)
				{
					try
					{
						var found = assembly.GetType("Grammar")!.GetMethod(method, [typeof(string), typeof(int), typeof(int)]);

						if (found is null)
							return "n/a";

						var match = found.Invoke(null, [doubled, at, input.Length])!;
						var type  = match.GetType();
						var ok    = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;

						return ok ? $"ok «{Escape(Spelled(type.GetProperty("Value")!.GetValue(match)))}» @{type.GetProperty("Position")!.GetValue(match)}" : "no";
					}
					catch (TargetInvocationException e)
					{
						return "THROW " + e.InnerException!.GetType().Name + ": " + Short(e.InnerException.Message);
					}
				}

				var a = W(over.Assembly);
				var b = W(read.Assembly);
				var c = W(run.Assembly);

				if (a == "n/a" || b == "n/a" || c == "n/a")
					continue;

				if (a != b || a != c)
				{
					Windowed++;

					if (told.Count < most)
						told.Add($"  window \"{Escape(doubled)}\" at {at}: chars {a} | reader {b} | engine {c}");
				}
			}
		}

		header += $"\n  inputs {count}";

		return told;
	}

	[Fact]
	public void Probe()
	{
		var path = Environment.GetEnvironmentVariable("DOTGRAM_LEXPROBE");

		if (path is null)
			return;

		var output = new StringBuilder();
		var blocks = File.ReadAllText(path).Split("\n===", StringSplitOptions.RemoveEmptyEntries);

		foreach (var block in blocks)
		{
			var halves = block.Split("\n---\n", 2);

			if (halves.Length < 2)
				continue;

			var head    = halves[0].Trim('\n', '=', ' ');
			var title   = head.Split('\n')[0];
			var grammar = head[(head.IndexOf('\n') + 1)..] + "\nparse Root\n";
			var inputs  = new List<string>();
			var values  = true;

			foreach (var line in halves[1].Split('\n'))
			{
				if (line.StartsWith("@all ", StringComparison.Ordinal))
				{
					var parts = line.Split(' ');

					inputs.AddRange(All(Unescape(parts[1]), int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture)));
				}
				else if (line == "@novalues")
					values = false;
				else if (line.Length > 0)
					inputs.Add(Unescape(line == "\\e" ? "" : line));
			}

			Begun = Other = Windowed = 0;

			File.WriteAllText(path + ".out", output.ToString() + $"### {title}: RUNNING\n");

			var told = Compare(grammar, "TryParseRoot", inputs, values, out var header);

			output.AppendLine($"### {title}\n  {header}\n  begun-refused {Begun}, other {Other}, windowed {Windowed}");

			foreach (var line in told)
				output.AppendLine(line);

			File.WriteAllText(path + ".out", output.ToString());
		}

		File.WriteAllText(path + ".out", output.ToString());
	}

	// ── Findings ──────────────────────────────────────────────────────────────

	/// <summary>A terminal that binds and reaches, through a choice in each rule, a diamond of rules that bind.</summary>
	static string Diamond(int depth)
	{
		var rules = new StringBuilder();

		for (var k = 0; k < depth; k++)
		{
			rules.Append(k == 0 ? $"\tR{k} = {{ $q{k}: (A{k + 1} | B{k + 1}) & $q{k} }}\n" : $"\tR{k} = $q{k}: (A{k + 1} | B{k + 1}) & $q{k}\n");
			rules.Append($"\tA{k + 1} = R{k + 1} & 'a'\n");
			rules.Append($"\tB{k + 1} = R{k + 1} & 'b'\n");
		}

		rules.Append($"\tR{depth} = 'x' | 'y'\n");

		return "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n" + rules +
			"}\nItem : @string = t: Lexical.R0 => @(t)\nRoot : @string = items: Item* => @(string.Join(\"|\", items))\nparse Root\n";
	}

	/// <summary>
	/// The beginning of a terminal that binds is computed by following every call on every path,
	/// so a diamond of rules — each a choice of two that call the same next rule — costs time
	/// exponential in its depth to cut, where the same grammar over characters compiles at once
	/// (and on main is refused at once, GRAM4034).
	/// </summary>
	[Fact]
	public void A_diamond_of_rules_that_bind_is_cut_in_reasonable_time()
	{
		var grammar = Diamond(18);
		var clock   = System.Diagnostics.Stopwatch.StartNew();

		GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		var characters = clock.ElapsedMilliseconds;

		clock.Restart();

		var cut = GramCompiler.Compile(grammar, new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		var tokens = clock.ElapsedMilliseconds;

		TestContext.Current.TestOutputHelper?.WriteLine($"over characters {characters} ms, cut in two {tokens} ms, cut: {cut.Sources[0].Text.Contains("Tokenize_DotGram(", StringComparison.Ordinal)}");

		Assert.True(tokens < 1000, $"cutting took {tokens} ms (over characters {characters} ms)");
	}

	/// <summary>
	/// Not this change's: the engine over characters (Direct = false) throws while it materializes
	/// a value after a negative lookahead on a reference gave its turn back. The reader reads it.
	/// Reproduces on main (920531d4) the same.
	/// </summary>
	[Fact(Skip = "Not this change's: the engine over characters throws in Materialize_DotGram here on main too; filed with the generator's own defects.")]
	public void The_engine_materializes_after_a_negative_lookahead_on_a_reference()
	{
		const string grammar =
			"""
			T = $q: ['a'..'b']* & ?!$q & 'a' | "ab"
			Item : @string = t: T => @(t)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var reader = Build(grammar, lexical: false, direct: true).Assembly!;
		var engine = Build(grammar, lexical: false, direct: false).Assembly!;

		Assert.Equal("ok «ba|ab» @0", Run(reader, "TryParseRoot", "baab"));
		Assert.Equal(Run(reader, "TryParseRoot", "baab"), Run(engine, "TryParseRoot", "baab"));
	}

	/// <summary>
	/// Not this change's: a window that holds no token — empty, or trivia alone — is refused over
	/// kinds, where over characters an empty repetition reads it. No binding anywhere; on main too.
	/// </summary>
	[Theory(Skip = "Not this change's: a window with no token in it is refused over kinds on main too, with no binding anywhere.")]
	[InlineData("", 0, 0)]
	[InlineData("ab  ab", 2, 2)]
	public void A_window_with_no_token_in_it_reads_as_over_characters(string input, int at, int length)
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				W = ['a'..'b']+
			}
			Item : @string = w: Lexical.W => @(w)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		string Window(Assembly assembly)
		{
			var match = assembly.GetType("Grammar")!.GetMethod("TryParseRoot", [typeof(string), typeof(int), typeof(int)])!.Invoke(null, [input, at, length])!;

			return ((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!).ToString();
		}

		Assert.Equal(Window(Build(grammar, lexical: false, direct: true).Assembly!), Window(Build(grammar, lexical: true, direct: true).Assembly!));
	}

	/// <summary>
	/// A beginning that spells other tokens — here `['a'..'z']+ & ':'`, an identifier and a mark — begins
	/// the terminal where they would stand; where the reference then fails, the position is read
	/// again without the terminal, and the identifier and the mark are read as over characters.
	/// </summary>
	[Theory]
	[InlineData("ab:ac")]
	[InlineData("ab:")]
	[InlineData("a:b")]
	public void A_beginning_that_spells_other_tokens_refuses_what_they_read(string input)
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T  = { $q: ['a'..'z']+ & ':' & $q }
				Id = ['a'..'z']+
			}
			Item : @string = t: Lexical.T => @("t:" + t) | i: Lexical.Id => @("id:" + i) | ':' => @(":")
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		Assert.Equal(Run(Build(grammar, lexical: false, direct: true).Assembly!, "TryParseRoot", input), Run(Build(grammar, lexical: true, direct: true).Assembly!, "TryParseRoot", input));
	}

	/// <summary>
	/// The same read from a position, where no end is demanded: the begun-and-refused token is read
	/// again as the word it also spells, and both forms read it.
	/// </summary>
	[Fact]
	public void A_begun_and_refused_token_shortens_a_reading_from_a_position()
	{
		const string grammar =
			"""
			trivia = { (' ' | '\n')* }
			namespace Lexical
			{
				trivia = none
				T = { 'x' & $q: 'a'* & eol & $q }
				W = ['b'..'z']+
			}
			Item : @string = t: Lexical.T => @("t:" + t) | w: Lexical.W => @("w:" + w)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		string From(Assembly assembly)
		{
			var match = assembly.GetType("Grammar")!.GetMethod("TryParseRoot", [typeof(string), typeof(int), typeof(int)])!.Invoke(null, ["xa\nb", 0, 4])!;

			return Spelled(match.GetType().GetProperty("IsSuccess")!.GetValue(match)) + " " + Spelled(match.GetType().GetProperty("Value")!.GetValue(match));
		}

		Assert.Equal(From(Build(grammar, lexical: false, direct: true).Assembly!), From(Build(grammar, lexical: true, direct: true).Assembly!));
	}

	// ── Random terminals with bindings ─────────────────────────────────────────

	static string RandomTerminal(Random random, int depth = 0)
	{
		string[] atoms = ["'a'", "'b'", "':'", "('a' | 'b')", "'a'+", "['a'..'b']*", "\"ab\"", "'b'?", "[^ ':']", "('a' & 'b')+"];
		var bound = new List<string>();
		var parts = new List<string>();
		var count = random.Next(2, 6);

		string Atom()
		{
			return atoms[random.Next(atoms.Length)];
		}

		for (var at = 0; at < count; at++)
		{
			var pick = bound.Count == 0 ? random.Next(3) : random.Next(12);
			var q    = bound.Count == 0 ? "" : bound[random.Next(bound.Count)];

			switch (pick)
			{
				case 0:
				case 1:
					var name = "q" + depth + bound.Count;
					var body = random.Next(5) switch
					{
						0 => Atom(),
						1 => "(" + Atom() + " & " + Atom() + ")",
						2 => "{ " + Atom() + " }",
						3 when depth < 1 => "(" + RandomTerminal(random, depth + 1) + ")",
						_ => "(" + Atom() + " | " + Atom() + " & " + Atom() + ")",
					};
					parts.Add($"${name}: {body}");
					bound.Add(name);
					break;
				case 2: parts.Add(Atom()); break;
				case 3:
				case 4: parts.Add("$" + q); break;
				case 5: parts.Add(random.Next(3) switch { 0 => $"'a'{{#{q}}}", 1 => $"any{{#{q},}}", _ => $"'b'{{0,#{q}}}" }); break;
				case 6: parts.Add(random.Next(2) == 0 ? $"?!(${q} & any)" : $"?=${q}"); break;
				case 7: parts.Add($"(?!${q} & any)*"); break;
				case 8: parts.Add($"(${q} | {Atom()})"); break;
				case 9: parts.Add($"(${q} & ':')*"); break;
				case 10: parts.Add("Sub"); break;
				default: parts.Add(Atom()); break;
			}
		}

		if (bound.Count > 0)
			parts.Add("$" + bound[random.Next(bound.Count)]);

		var sequence = string.Join(" & ", parts);

		if (depth == 0 && random.Next(4) == 0)
			sequence = sequence + " | " + Atom() + " & " + Atom();

		return sequence;
	}

	static readonly string[] Competitors =
	[
		"W = ['a'..'b']+",
		"S = ':' & ['a'..'b']* & ':'",
		"L = \"ab\"",
		"C = 'a' & ':'",
	];

	[Fact]
	public void RandomProbe()
	{
		var seeds = Environment.GetEnvironmentVariable("DOTGRAM_LEXRANDOM");

		if (seeds is null)
			return;

		var parts  = seeds.Split(':');
		var first  = int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
		var many   = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
		var output = new StringBuilder();
		var totals = RunRandom(first, many, output);

		output.Insert(0, string.Join("\n", totals.OrderBy(static one => one.Key, StringComparer.Ordinal).Select(static one => $"{one.Key}: {one.Value}")) + "\n\n");
		File.WriteAllText(Path.Combine(Path.GetTempPath(), $"lexical-binding-random-{first}-{many}.out"), output.ToString());
	}

	/// <summary>
	/// Random terminals that bind text, each the one terminal of a grammar cut in two: the tokens
	/// over kinds are what the terminal's first reading from each token start gives (the reference
	/// interpreter), with the spaces between skipped — no backtracking across tokens to explain a
	/// difference, so any difference is a defect.
	/// </summary>
	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	[InlineData(200)]
	public void Random_terminals_that_bind_are_tokenized_as_their_first_reading(int first)
	{
		BackReferenceEmission.RequireBoth();

		var output = new StringBuilder();
		var totals = RunRandom(first, 15, output, crowd: false);

		TestContext.Current.TestOutputHelper?.WriteLine(string.Join("\n", totals.OrderBy(static one => one.Key, StringComparer.Ordinal).Select(static one => $"{one.Key}: {one.Value}")));
		TestContext.Current.TestOutputHelper?.WriteLine(output.ToString());

		Assert.True(totals.GetValueOrDefault("tokens compared (bound)") > 5, "too few grammars were cut");
		Assert.Equal(0, totals.GetValueOrDefault("tokens DISAGREE (bound)"));
		Assert.Equal(0, totals.GetValueOrDefault("THROW"));
	}

	static Dictionary<string, int> RunRandom(int first, int many, StringBuilder output, bool crowd = true)
	{
		var inputs = All("ab:", 6).Concat(All("ab: ", 4).Where(static one => one.Contains(' ', StringComparison.Ordinal) && !one.StartsWith(' '))).Distinct().ToArray();
		var totals = new Dictionary<string, int>();

		void Count(string what, int by = 1)
		{
			totals[what] = totals.GetValueOrDefault(what) + by;
		}

		for (var seed = first; seed < first + many; seed++)
		{
			var random   = new Random(seed * 7919 + 13);
			var terminal = RandomTerminal(random);
			var sub      = terminal.Contains("Sub", StringComparison.Ordinal) ? "Sub = 'b' & $s: ('a' | ':')+ & $s | ':'" : "";
			var picked   = Competitors.Where(_ => random.Next(2) == 0).ToList();

			// Alone: the twin is an exact oracle, nothing competes with the terminal.
			var alone =
				"trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tT = { " + terminal + " }\n\t" + sub + "\n}\n" +
				"Item : @string = t: Lexical.T => @(t)\nRoot : @string = items: Item* => @(string.Join(\"|\", items))\nparse Root\n";

			var crowdGrammar =
				"trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tT = { " + terminal + " }\n\t" + sub + "\n\t" + string.Join("\n\t", picked) + "\n}\n" +
				"Item : @string = t: Lexical.T => @(\"t:\" + t)" +
				string.Concat(picked.Select(static one => { var n = one[..one.IndexOf(' ')]; return $" | x{n}: Lexical.{n} => @(\"{n}:\" + x{n})"; })) +
				" | ':' => @(\":\") | 'a' => @(\"a\") | 'b' => @(\"b\")\n" +
				"Root : @string = items: Item* => @(string.Join(\"|\", items))\nparse Root\n";

			// Tokens: the alone grammar over kinds must be what reading T's first reading from
			// each token start gives (the reference interpreter), with the spaces skipped between.
			{
				var lexical = Build(alone, lexical: true, direct: true);
				var engine  = Build(alone, lexical: true, direct: false);

				Count("tokens grammars");

				if (lexical.Assembly is null || engine.Assembly is null || !lexical.Cut)
					Count("tokens skipped (not cut or refused)");
				else
				{
					Count("tokens compared");

					if (terminal.Contains('$', StringComparison.Ordinal))
						Count("tokens compared (bound)");

					var graph = DotGram.Grammar.Model.GrammarNormalizer.Normalize(
						DotGram.Grammar.Binding.GrammarBinder.Bind(DotGram.Grammar.Parsing.GramParser.Parse(DotGram.Grammar.Parsing.GramLexer.Tokenize(alone, RoslynCSharpScanner.Instance)).File));
					var rule  = graph.Rules.First(static one => one.Name == "T");
					var lines = 0;

					foreach (var input in inputs)
					{
						var p      = 0;
						var tokens = new List<string>();
						var failed = false;

						while (true)
						{
							while (p < input.Length && input[p] == ' ')
								p++;

							if (p >= input.Length)
								break;

							var end = ReferenceInterpreter.Reads(graph, rule, input, p);

							if (end <= p)
							{
								failed = true;

								break;
							}

							tokens.Add(input[p..end]);
							p = end;
						}

						var expected = failed ? "no" : $"ok «{Escape(string.Join("|", tokens))}»";
						var b = Run(lexical.Assembly, "TryParseRoot", input);
						var c = Run(engine.Assembly, "TryParseRoot", input);

						string Key(string answer)
						{
							return answer.StartsWith("no", StringComparison.Ordinal) ? "no" : answer[..answer.LastIndexOf(" @", StringComparison.Ordinal)];
						}

						Count("tokens inputs");

						if (b.StartsWith("THROW", StringComparison.Ordinal) || c.StartsWith("THROW", StringComparison.Ordinal))
							Count("THROW");

						if (Key(b) != expected || Key(c) != expected)
						{
							Count("tokens DISAGREE");

							if (terminal.Contains('$', StringComparison.Ordinal))
								Count("tokens DISAGREE (bound)");

							if (lines++ < 6)
								output.AppendLine($"### tokens seed {seed}: T = {terminal}\n  \"{Escape(input)}\": expected {expected} | reader {b} | engine {c}");
						}
					}
				}
			}

			foreach (var (how, grammar) in crowd ? new[] { ("alone", alone), ("crowd", crowdGrammar) } : [])
			{
				Begun = Other = Windowed = 0;

				var told = Compare(grammar, "TryParseRoot", inputs, values: true, out var header, most: Environment.GetEnvironmentVariable("DOTGRAM_LEXRANDOM_ALL") is null ? 6 : int.MaxValue);

				Count(how + " grammars");

				if (header.Contains("REFUSED", StringComparison.Ordinal) && header.StartsWith("chars: REFUSED", StringComparison.Ordinal))
				{
					Count(how + " refused over characters");

					continue;
				}

				if (header.Contains("GENERATOR THROW", StringComparison.Ordinal) || header.Contains("C# COMPILE FAIL", StringComparison.Ordinal))
					Count(how + " BUILD BREAK");
				else if (header.Contains("REFUSED", StringComparison.Ordinal))
					Count(how + " lexical refused (GRAM4034 etc)");
				else if (header.Contains("NOT CUT", StringComparison.Ordinal))
					Count(how + " not cut (GRAM5004)");
				else
					Count(how + " cut and compared");

				Count(how + " begun-refused inputs", Begun);
				Count(how + " other inputs", Other);
				Count(how + " windowed inputs", Windowed);

				var interesting = (how == "alone" && (Begun > 0 || Other > 0 || Windowed > 0)) || Other > 0 || Windowed > 0 ||
					header.Contains("THROW", StringComparison.Ordinal) || header.Contains("COMPILE FAIL", StringComparison.Ordinal);

				if (interesting || Environment.GetEnvironmentVariable("DOTGRAM_LEXRANDOM_ALL") is not null)
				{
					output.AppendLine($"### seed {seed} {how}: T = {terminal}  [{string.Join("; ", picked)}]\n  {header}\n  begun-refused {Begun}, other {Other}, windowed {Windowed}");

					foreach (var line in told)
						output.AppendLine(line);
				}
			}
		}

		return totals;
	}
}
