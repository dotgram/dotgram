using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

// Spike probe driver. Reads cases from $BLH_IN, compares checked / unchecked / untailed.
public sealed class C1Probe
{
	internal const string Counter = """
		[System.ThreadStatic] static long calls;
		public static long Seen;
		[System.ThreadStatic] static long since;
		public static int Cap = 3000;
		public static long Ticks = System.Diagnostics.Stopwatch.Frequency * 2;
		public static void Reset() { calls = 0; since = System.Diagnostics.Stopwatch.GetTimestamp(); }
		static void Count() { Seen = calls + 1; if ((++calls > Cap && Cap != int.MaxValue) || System.Diagnostics.Stopwatch.GetTimestamp() - since > Ticks) throw new System.InvalidOperationException("went round"); }
		public sealed class Ctx { public int Depth; public int Bump() { return ++Depth; } }
		public static bool Pure(int x) { return x > 0; }
		""";

	internal const string SpanHost = Counter + """
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Dot(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') { p++; return true; } return false; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') p++; return true; }
		static bool OnB(System.ReadOnlySpan<char> input, ref int p) { Count(); return p < input.Length && input[p] == 'b'; }
		static bool Ws(System.ReadOnlySpan<char> input, ref int p) { Count(); while (p < input.Length && input[p] == ' ') p++; return true; }
		static bool Num(System.ReadOnlySpan<char> input, ref int p, out int value) { Count(); value = 0; return true; }
		""";

	internal const string ViewHost = Counter + """
		static bool Still(ParserInput<char> input, ref int p) { Count(); return true; }
		static bool Still(ParserInput<byte> input, ref int p) { Count(); return true; }
		static bool Dot(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') { p++; return true; } return false; }
		static bool Dot(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') { p++; return true; } return false; }
		static bool Half(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') p++; return true; }
		static bool Half(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') p++; return true; }
		static bool OnB(ParserInput<char> input, ref int p) { Count(); return input.Peek(p, out var c) && c == 'b'; }
		static bool OnB(ParserInput<byte> input, ref int p) { Count(); return input.Peek(p, out var c) && c == (byte)'b'; }
		static bool Ws(ParserInput<char> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == ' ') p++; return true; }
		static bool Ws(ParserInput<byte> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == (byte)' ') p++; return true; }
		""";

	static readonly MethodInfo CreateMethod = typeof(EmittedCode).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;

	internal enum Mode { Checked, Unchecked, Untailed }

	internal sealed record Built(Assembly Assembly, bool Marked, bool Tail, string Source, AssemblyLoadContext Context);

	internal static Built? Build(string grammar, bool direct, bool tape, bool buffered, Mode mode, string? publish = null, bool lexical = false)
	{
		publish ??= grammar.Contains("\nparse ", StringComparison.Ordinal) || grammar.StartsWith("parse ", StringComparison.Ordinal) || grammar.Contains("\nfind ", StringComparison.Ordinal) ? "" : "\nparse T";
		lexical |= grammar.Contains("namespace Lexical", StringComparison.Ordinal);

		GramCompilation result;
		var wasU = Reentry.Unchecked;

		try
		{
			Reentry.Unchecked = mode == Mode.Unchecked;
			result = GramCompiler.Compile(grammar + publish, new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				Lexical        = lexical,
				Carrier        = tape ? CarrierKind.Deferred : CarrierKind.Auto,
				BufferedInput  = buffered,
				BufferedBytes  = buffered,
				SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});
		}
		finally
		{
			Reentry.Unchecked = wasU;
		}

		if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error) || result.Sources.Count == 0)
		{
			LastErrors = string.Join("; ", result.Diagnostics.Where(static one => one.Severity == GramSeverity.Error).Select(static one => one.Id + " " + one.Message).Take(3));

			return null;
		}

		var compilation = (CSharpCompilation)CreateMethod.Invoke(null,
		[
			result.Sources[0].Text, "Probe", null, buffered ? ViewHost : SpanHost,
			result.Sources.Skip(1).Select(static one => one.Text).ToArray(), null, false,
		])!;

		using var stream = new MemoryStream();

		var emitted = compilation.Emit(stream);

		if (!emitted.Success)
			throw new InvalidOperationException(
				"C# failed:\n" + string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error).Take(5)) + "\n" + grammar);

		stream.Position = 0;

		var context = new AssemblyLoadContext("c1", isCollectible: true);
		var source  = string.Join("\n", result.Sources.Select(static one => one.Text));
		var info    = result.Diagnostics.FirstOrDefault(static one => one.Id == Reentry.Reenters);

		return new Built(
			context.LoadFromStream(stream),
			info is not null,
			info is not null && info.Message.Contains("would only repeat", StringComparison.Ordinal),
			source,
			context);
	}

	[ThreadStatic] internal static string? LastErrors;

	internal static string Answer(Assembly parser, string input, Type over, int cap = 3000)
	{
		var type = parser.GetType("Probe")!;

		type.GetField("Cap")!.SetValue(null, cap);
		type.GetMethod("Reset")!.Invoke(null, null);

		var context = type.GetNestedType("Ctx");
		var method  = type.GetMethods().FirstOrDefault(one =>
			one.Name == "TryParseT" && one.ReturnType != typeof(bool) &&
			one.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(
				(over == typeof(string) ? new[] { typeof(string) } : [over]).Concat(
					one.GetParameters().Any(parameter => parameter.ParameterType == context) ? [context!] : []).Concat(
					over == typeof(string) ? [] : [typeof(int?), typeof(int?)])));

		if (method is null)
			return "none";

		object match;

		var text = over == typeof(string) ? input : over == typeof(Stream) ? new MemoryStream(Encoding.ASCII.GetBytes(input)) : (object)new StringReader(input);
		var args = new List<object?> { text };

		if (method.GetParameters().Any(parameter => parameter.ParameterType == context))
			args.Add(Activator.CreateInstance(context!));

		if (over != typeof(string))
		{
			args.Add(1);
			args.Add(1 << 16);
		}

		try
		{
			match = method.Invoke(null, [.. args])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			return inner.Message == "went round" || inner is InsufficientExecutionStackException ? "LOOP" : "EXC " + inner.GetType().Name + ": " + inner.Message;
		}

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!} {(Read("Value") is byte[] bytes ? Encoding.ASCII.GetString(bytes) : Read("Value"))}"
			: $"no@{Read("Position")} {Read("Outcome")} {Read("Error")}";
	}

	internal static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var all  = new List<string> { "" };
		var last = new List<string> { "" };

		for (var length = 1; length <= longest; length++)
		{
			last = [.. last.SelectMany(prefix => alphabet.Select(c => prefix + c))];
			all.AddRange(last);
		}

		return all;
	}

	internal static string Alphabet(string grammar)
	{
		var letters = new SortedSet<char>();

		for (var at = 0; at + 2 < grammar.Length; at++)
			if (grammar[at] == '\'' && grammar[at + 2] == '\'' && grammar[at + 1] != '\\')
				letters.Add(grammar[at + 1]);

		if (grammar.Contains("..", StringComparison.Ordinal))
			letters.Add('1');

		letters.Add('.');

		return string.Concat(letters);
	}

	/// <summary>Compares one grammar; returns lines describing differences (empty = agree).</summary>
	internal static List<string> Compare(string grammar, string[]? inputs, int longest, bool allForms, out string info)
	{
		var problems = new List<string>();

		info = "";

		var alphabet = Alphabet(grammar);

		while (longest > 2 && Math.Pow(alphabet.Length, longest) > 600)
			longest--;

		var strings = inputs ?? Inputs(alphabet, longest).ToArray();

		var forms = allForms
			? (from direct in new[] { true, false } from tape in new[] { false, true } from buffered in new[] { false, true } select (direct, tape, buffered)).ToArray()
			: new[] { (true, false, false), (false, false, false) };

		foreach (var (direct, tape, buffered) in forms)
		{
			Built? checkd, oracle, untailed;

			try
			{
				checkd   = Build(grammar, direct, tape, buffered, Mode.Checked);
				oracle   = Build(grammar, direct, tape, buffered, Mode.Unchecked);
				untailed = checkd;
			}
			catch (InvalidOperationException e)
			{
				problems.Add("COMPILE " + e.Message.Split('\n')[0] + " " + e.Message.Split('\n').ElementAtOrDefault(1));

				return problems;
			}

			if (checkd is null || oracle is null || untailed is null)
			{
				info = "ERR " + LastErrors;

				return problems;
			}

			var reading = (direct ? "direct" : "engine") + (tape ? "/tape" : "/imm") + (buffered ? "/view" : "/span");

			if (info.Length == 0)
				info = $"marked={checkd.Marked} tail={checkd.Tail}";

			foreach (var form in buffered ? new[] { typeof(string), typeof(TextReader), typeof(Stream) } : [typeof(string)])
				foreach (var input in strings)
				{
					var expected = Answer(oracle.Assembly, input, form);
					var got      = Answer(checkd.Assembly, input, form, 60000);
					var a        = checkd.Tail ? Answer(untailed.Assembly, input, form, 60000) : got;

					if (expected == "none")
						continue;

					if (expected == "LOOP")
					{
						if (got == "LOOP" || got.StartsWith("EXC", StringComparison.Ordinal))
							problems.Add($"{reading}/{form.Name} \"{input}\": oracle LOOP | checked {got}");

						if (got != a && !a.StartsWith("LOOP", StringComparison.Ordinal))
							problems.Add($"VSA {reading}/{form.Name} \"{input}\": oracle LOOP | checked {got} | variantA {a}");
					}
					else if (expected != got)
						problems.Add($"{reading}/{form.Name} \"{input}\": oracle {expected} | checked {got} | variantA {a}");
				}

			checkd.Context.Unload();
			oracle.Context.Unload();
		}

		return problems;
	}

	[Fact]
	public void Probe()
	{
		var file = Environment.GetEnvironmentVariable("BLH_IN");

		if (file is null)
			return;

		var output = new StringBuilder();
		var all    = Environment.GetEnvironmentVariable("BLH_ALL") is not null;
		var length = int.Parse(Environment.GetEnvironmentVariable("BLH_LEN") ?? "5");
		var dump   = Environment.GetEnvironmentVariable("BLH_DUMP");

		var thread = new System.Threading.Thread(() =>
		{
			foreach (var block in File.ReadAllText(file).Split("\n====\n"))
			{
				var lines   = block.Split('\n').Where(static line => line.Length > 0 && !line.StartsWith('#')).ToList();
				var inLine  = lines.FirstOrDefault(static line => line.StartsWith("@in ", StringComparison.Ordinal));
				var grammar = string.Join("\n", lines.Where(static line => !line.StartsWith('@')));

				if (grammar.Length == 0)
					continue;

				var inputs = inLine?.Substring(4).Split('|');

				try
				{
					if (dump is not null)
					{
						var built = Build(grammar, true, false, false, Mode.Checked);

						File.WriteAllText(dump, built?.Source ?? LastErrors);
					}

					var problems = Compare(grammar, inputs, length, all, out var info);

					output.Append($"### {grammar.Replace("\n", " ;; ")}\n{info}; problems {problems.Count}\n");

					foreach (var problem in problems.Take(12))
						output.Append("  " + problem + "\n");
				}
				catch (Exception e)
				{
					output.Append($"### {grammar.Replace("\n", " ;; ")}\nTHREW {e.GetType().Name}: {e.Message.Split('\n')[0]}\n");
				}
			}
		}, 256 << 20);

		thread.Start();
		thread.Join();

		File.WriteAllText(Environment.GetEnvironmentVariable("BLH_OUT") ?? "/ramdisk/tmp/breaker-loopheads/probe-out.txt", output.ToString());
	}

	static string Item(Random random, int depth)
	{
		string[] atoms = ["'a'", "'b'", "'c'", "'.'", "@Still", "@Dot", "@Half", "@OnB", "A", "B", "A", "eof"];

		var roll = random.Next(depth > 1 ? 12 : 19);

		if (roll < 12)
			return atoms[random.Next(atoms.Length)];

		var inner = Alt(random, depth + 1);

		return roll switch
		{
			12 => $"({inner})?",
			13 => $"({inner})*",
			14 => $"?=({inner})",
			15 => $"?!({inner})",
			16 => $"{{ {inner} }}",
			17 => $"x{depth}: ({inner})",
			_  => $"({inner} | {Alt(random, depth + 1)})",
		};
	}

	static string Alt(Random random, int depth)
	{
		var count = 1 + random.Next(depth == 0 ? 2 : 2);
		var items = Enumerable.Range(0, count).Select(_ => Item(random, depth)).ToList();

		// A tail call back, the shape the cut at once is for.
		if (depth == 0 && random.Next(3) > 0)
		{
			string[] lead = ["@Still", "@Half", "@OnB", "(@Half)?", "(@Still | 'a')", "?!'b' & @Still", "@Still & 'c'?"];
			string[] call = ["A", "B", "A"];

			if (random.Next(2) == 0)
				items.Clear();

			items.Add(lead[random.Next(lead.Length)]);
			items.Add(call[random.Next(call.Length)]);
		}

		return string.Join(" & ", items);
	}

	static string FuzzGrammar(Random random)
	{
		string[] tops = ["T = A & eof", "T = A & 'c'", "T = A", "T = { A } & eof", "T = A & A & eof", "T = (A & ';')* & eof", "T = ?=(A) & A & eof", "T = A & 'c' | A & 'b'"];
		var rules = new List<string> { tops[random.Next(tops.Length)] };

		rules.Add("A = " + string.Join(" | ", Enumerable.Range(0, 2 + random.Next(2)).Select(_ => Alt(random, 0))));

		if (random.Next(2) == 0)
			rules.Add("B = " + string.Join(" | ", Enumerable.Range(0, 1 + random.Next(2)).Select(_ => Alt(random, 0))));
		else
			rules.Add("B = 'b'");

		return string.Join("\n", rules);
	}

	[Fact]
	public void Fuzz()
	{
		if (Environment.GetEnvironmentVariable("BLH_FUZZ") is not { } seedText)
			return;

		var seed   = int.Parse(seedText);
		var count  = int.Parse(Environment.GetEnvironmentVariable("BLH_COUNT") ?? "200");
		var output = Environment.GetEnvironmentVariable("BLH_OUT") ?? $"/ramdisk/tmp/breaker-loopheads/fuzz-{seed}.txt";
		var onlyTail = Environment.GetEnvironmentVariable("BLH_ANY") is null;
		var all    = Environment.GetEnvironmentVariable("BLH_ALL") is not null;

		var thread = new System.Threading.Thread(() =>
		{
			var random = new Random(seed);
			int built = 0, marked = 0, tails = 0, bad = 0;

			for (var i = 0; i < count; i++)
			{
				var grammar = FuzzGrammar(random);

				try
				{
					var probe = Build(grammar, true, false, false, Mode.Checked);

					if (probe is null)
						continue;

					built++;
					probe.Context.Unload();

					if (!probe.Marked)
						continue;

					marked++;

					if (onlyTail && !probe.Tail)
						continue;

					tails++;

					var problems = Compare(grammar, null, 4, all, out var info);

					if (problems.Count > 0)
					{
						bad++;
						File.AppendAllText(output, $"### {grammar.Replace("\n", " ;; ")}\n{info}; problems {problems.Count}\n" + string.Concat(problems.Take(6).Select(static one => "  " + one + "\n")));
					}
				}
				catch (Exception e)
				{
					File.AppendAllText(output, $"### {grammar.Replace("\n", " ;; ")}\nTHREW {e.GetType().Name}: {e.Message.Split('\n')[0]}\n");
				}
			}

			File.AppendAllText(output, $"seed {seed}: generated {count}, built {built}, marked {marked}, compared {tails}, with problems {bad}\n");
		}, 256 << 20);

		thread.Start();
		thread.Join();
	}

	[Fact]
	public void Timing()
	{
		if (Environment.GetEnvironmentVariable("BLH_TIME") is not { } file)
			return;

		var output = new StringBuilder();

		var thread = new System.Threading.Thread(() =>
		{
			foreach (var block in File.ReadAllText(file).Split("\n====\n"))
			{
				var lines   = block.Split('\n').Where(static line => line.Length > 0 && !line.StartsWith('#')).ToList();
				var grammar = string.Join("\n", lines.Where(static line => !line.StartsWith('@')));
				var inputs  = lines.Where(static line => line.StartsWith("@in ", StringComparison.Ordinal)).Select(static line => Expand(line.Substring(4))).ToList();
				var modes   = lines.FirstOrDefault(static line => line.StartsWith("@mode ", StringComparison.Ordinal))?.Substring(6).Split(' ') ?? ["checked"];
				var forms   = lines.FirstOrDefault(static line => line.StartsWith("@forms ", StringComparison.Ordinal))?.Substring(7).Split(' ') ?? ["direct/string"];
				var seconds = double.Parse(lines.FirstOrDefault(static line => line.StartsWith("@limit ", StringComparison.Ordinal))?.Substring(7) ?? "30");

				output.Append($"### {grammar.Replace("\n", " ;; ")}\n");

				foreach (var mode in modes)
					foreach (var form in forms)
					{
						var parts    = form.Split('/');
						var direct   = parts[0] == "direct";
						var tape     = parts[0].EndsWith("tape", StringComparison.Ordinal);
						var over     = parts[1] switch { "reader" => typeof(TextReader), "stream" => typeof(Stream), _ => typeof(string) };
						var buffered = parts[1] is "reader" or "stream" or "view";
						var built    = Build(grammar, direct || parts[0] == "directtape", tape, buffered, mode == "unchecked" ? Mode.Unchecked : mode == "untailed" ? Mode.Untailed : Mode.Checked);

						if (built is null)
						{
							output.Append($"  {mode} {form}: ERR {LastErrors}\n");

							continue;
						}

						var type = built.Assembly.GetType("Probe")!;

						type.GetField("Ticks")!.SetValue(null, (long)(System.Diagnostics.Stopwatch.Frequency * seconds));

						foreach (var input in inputs)
						{
							var watch  = System.Diagnostics.Stopwatch.StartNew();
							var answer = Answer(built.Assembly, input, over, int.MaxValue);

							watch.Stop();

							var shown = answer.Length > 160 ? answer.Substring(0, 160) + "…" : answer;
							output.Append($"  {mode} {form} len {input.Length}: {watch.Elapsed.TotalMilliseconds:F1} ms calls {type.GetField("Seen")!.GetValue(null)} {shown}\n");
							File.WriteAllText(Environment.GetEnvironmentVariable("BLH_OUT") ?? "/ramdisk/tmp/breaker-loopheads/time-out.txt", output.ToString());

							if (answer == "LOOP")
								break;
						}

						built.Context.Unload();
					}
			}
		}, 1 << 30);

		thread.Start();
		thread.Join();

		File.WriteAllText(Environment.GetEnvironmentVariable("BLH_OUT") ?? "/ramdisk/tmp/breaker-loopheads/time-out.txt", output.ToString());
	}

	/// <summary>
	/// Every publication of a grammar (parse, positional parse, find, yield, recover) held to the oracle, in four
	/// machines (direct/engine x span/buffered) over string, reader and stream. Cases from $C1_PUB: blocks of a grammar
	/// with its own publications and @in lines; output to $BLH_OUT.
	/// </summary>
	[Fact]
	public void Publications()
	{
		if (Environment.GetEnvironmentVariable("C1_PUB") is not { } file)
			return;

		var output = new StringBuilder();
		int compared = 0, differ = 0, loops = 0, ended = 0;

		var thread = new System.Threading.Thread(() =>
		{
			foreach (var block in File.ReadAllText(file).Split("\n====\n"))
			{
				var lines   = block.Split('\n').Where(static line => line.Length > 0 && !line.StartsWith('#')).ToList();
				var grammar = string.Join("\n", lines.Where(static line => !line.StartsWith('@')));
				var inputs  = lines.Where(static line => line.StartsWith("@in ", StringComparison.Ordinal)).SelectMany(static line => line.Substring(4).Split('|')).Select(Expand).ToList();

				if (grammar.Length == 0)
					continue;

				output.Append($"### {grammar.Replace("\n", " ;; ")}\n");

				foreach (var direct in new[] { true, false })
					foreach (var tape in new[] { false, true })
						foreach (var buffered in new[] { false, true })
						{
							Built? oracle, checkd;

							try
							{
								oracle = Build(grammar, direct, tape, buffered, Mode.Unchecked, publish: "");
								checkd = Build(grammar, direct, tape, buffered, Mode.Checked, publish: "");
							}
							catch (InvalidOperationException e)
							{
								output.Append($"  COMPILE {e.Message.Split('\n')[0]} {e.Message.Split('\n').ElementAtOrDefault(1)}\n");

								continue;
							}

							var reading = (direct ? "direct" : "engine") + (tape ? "/tape" : "/imm") + (buffered ? "/view" : "/span");

							if (oracle is null || checkd is null)
							{
								output.Append($"  {reading}: ERR {LastErrors}\n");

								continue;
							}

							output.Append($"  {reading}: same source {oracle.Source == checkd.Source}\n");

							foreach (var input in inputs)
								foreach (var (name, expected) in Publish(oracle.Assembly, input))
								{
									var got = Publish(checkd.Assembly, input, 60000).First(one => one.Name == name).Answer;

									compared++;

									if (expected.Contains("LOOP", StringComparison.Ordinal))
									{
										loops++;

										if (got.Contains("LOOP", StringComparison.Ordinal) || (got.Contains("EXC", StringComparison.Ordinal) && !got.Contains("EXC FormatException", StringComparison.Ordinal)))
										{
											differ++;
											output.Append($"  DIFF {reading} {name} \"{input}\": oracle {expected} | checked {got}\n");
										}
										else
											ended++;
									}
									else if (expected != got)
									{
										differ++;
										output.Append($"  DIFF {reading} {name} \"{input}\": oracle {expected} | checked {got}\n");
									}
								}

							oracle.Context.Unload();
							checkd.Context.Unload();
						}

				File.WriteAllText(Environment.GetEnvironmentVariable("BLH_OUT")!, output.ToString());
			}
		}, 1 << 28);

		thread.Start();
		thread.Join();

		output.Append($"\ncompared {compared}, differing {differ}, oracle went round {loops} (checked ended {ended})\n");
		File.WriteAllText(Environment.GetEnvironmentVariable("BLH_OUT")!, output.ToString());
	}

	/// <summary>What every publication answers on one input, by method and form.</summary>
	static List<(string Name, string Answer)> Publish(Assembly parser, string input, int cap = 3000)
	{
		var type    = parser.GetType("Probe")!;
		var answers = new List<(string, string)>();

		foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(static one => one.ToString(), StringComparer.Ordinal))
		{
			if (!(method.Name.StartsWith("Parse", StringComparison.Ordinal) || method.Name.StartsWith("TryParse", StringComparison.Ordinal) || method.Name.StartsWith("Find", StringComparison.Ordinal)))
				continue;

			var parameters = method.GetParameters().Select(static one => one.ParameterType).ToArray();
			var forms      = new List<(string Label, object?[] Args)>();

			if (parameters.SequenceEqual([typeof(string)]))
				forms.Add(("s", [input]));
			else if (parameters.SequenceEqual([typeof(string), typeof(int)]))
				for (var at = 0; at <= input.Length; at++)
					forms.Add(($"s@{at}", [input, at]));
			else if (parameters.SequenceEqual([typeof(string), typeof(int), typeof(int)]))
				for (var at = 0; at <= input.Length; at++)
					forms.Add(($"s@{at}+{input.Length - at}", [input, at, input.Length - at]));
			else if (parameters.SequenceEqual([typeof(TextReader), typeof(int?), typeof(int?)]))
				forms.Add(("r", [new StringReader(input), 1, 1 << 16]));
			else if (parameters.SequenceEqual([typeof(Stream), typeof(int?), typeof(int?)]))
				forms.Add(("b", [new MemoryStream(Encoding.ASCII.GetBytes(input)), 1, 1 << 16]));
			else
				continue;

			foreach (var (label, args) in forms)
			{
				type.GetField("Cap")!.SetValue(null, cap);
				type.GetMethod("Reset")!.Invoke(null, null);

				string answer;

				try
				{
					answer = Shown(method.Invoke(null, args));
				}
				catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
				{
					answer = inner.Message == "went round" || inner is InsufficientExecutionStackException ? "LOOP" : "EXC " + inner.GetType().Name + ": " + inner.Message;
				}
				catch (InvalidOperationException thrown) when (thrown.Message == "went round")
				{
					answer = "LOOP";
				}

				answers.Add((method.Name + "/" + label, answer));
			}
		}

		return answers;
	}

	static string Shown(object? value, int depth = 0)
	{
		if (depth > 4)
			return value?.ToString() ?? "null";

		switch (value)
		{
			case null:
				return "null";

			case string text:
				return "\"" + text + "\"";

			case byte[] bytes:
				return "\"" + Encoding.ASCII.GetString(bytes) + "\"";

			case System.Collections.IEnumerable items when value.GetType().GetProperty("IsSuccess") is null:
			{
				var shown = new List<string>();

				try
				{
					foreach (var item in items)
						shown.Add(Shown(item, depth + 1));
				}
				catch (Exception thrown) when (thrown is InvalidOperationException { Message: "went round" } or InsufficientExecutionStackException)
				{
					shown.Add("LOOP");
				}
				catch (Exception thrown) when (thrown is not OutOfMemoryException)
				{
					shown.Add("EXC " + thrown.GetType().Name + ": " + thrown.Message);
				}

				return "[" + string.Join(", ", shown) + "]";
			}
		}

		var type = value.GetType();

		if (type.IsPrimitive || type.IsEnum)
			return value.ToString()!;

		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(static one => one.GetIndexParameters().Length == 0)
			.OrderBy(static one => one.Name, StringComparer.Ordinal);

		var parts = new List<string>();

		foreach (var property in properties)
		{
			object? read;

			try
			{
				read = property.GetValue(value);
			}
			catch (TargetInvocationException)
			{
				continue;
			}

			parts.Add(property.Name + "=" + (read is null || read.GetType().IsPrimitive || read is string || read.GetType().IsEnum ? read?.ToString() ?? "null" : Shown(read, depth + 1)));
		}

		return "{" + string.Join(" ", parts) + "}";
	}

	/// <summary>"a*5 + b" style: pieces joined by " + ", each "text*count".</summary>
	static string Expand(string spec)
	{
		var builder = new StringBuilder();

		foreach (var piece in spec.Split(" + "))
		{
			var star = piece.LastIndexOf('*');

			if (star > 0 && int.TryParse(piece.AsSpan(star + 1), out var times))
				builder.Insert(builder.Length, piece.Substring(0, star), times);
			else
				builder.Append(piece);
		}

		return builder.ToString();
	}
}
