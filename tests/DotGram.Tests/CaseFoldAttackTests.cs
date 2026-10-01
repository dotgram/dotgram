using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Ignore-case literals over characters whose cases are not a pair: every rendering of one
/// random grammar against the reference semantics, on an alphabet made of them.
/// </summary>
public sealed class CaseFoldAttackTests
{
	/// <summary>
	/// s/S/ſ, k/K/Kelvin, σ/ς/Σ, the titlecase triple, ß/ẞ, i/I/ı/İ, μ/Μ/micro, and one
	/// letter with an ordinary pair.
	/// </summary>
	static string Alphabet = Unpaired;

	const string Unpaired = "sS\u017FkK\u212A\u03C3\u03C2\u03A3\u01C5\u01C6\u01C4\u00DF\u1E9EiI\u0131\u0130\u00B5\u03BC\u039Ca";

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(9)]
	[InlineData(10)]
	[InlineData(11)]
	[InlineData(12)]
	public void Every_rendering_agrees_with_the_semantics_on_unpaired_cases(int seed)
	{
		Fuzz(seed, Unpaired);
	}

	/// <summary>The same over ASCII pairs alone: what is wrong here is not about the fold.</summary>
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	public void Every_rendering_agrees_with_the_semantics_on_ascii_pairs(int seed)
	{
		Fuzz(seed, "aAbBkK1");
	}

	static void Fuzz(int seed, string alphabet)
	{
		Alphabet = alphabet;

		var random   = new Random(seed * 104729);
		var compiled = 0;
		var failures = new List<string>();

		while (compiled < 25 && failures.Count < 20)
		{
			var grammar = Generate(random);
			var modes   = new List<(string Name, Assembly Assembly, bool Buffered)>();

			foreach (var (name, options) in Modes())
			{
				// A grammar cut into tokens reads trivia between every two of them, which the
				// semantics here does not: it is asked of the table below instead.
				if (options.Lexical)
					continue;

				GramCompilation result;

				try
				{
					result = GramCompiler.Compile(grammar, options);
				}
				catch (Exception thrown)
				{
					failures.Add($"{name}: the generator threw {thrown.GetType().Name}: {thrown.Message}\n{grammar}");

					continue;
				}

				if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error))
					continue;

				try
				{
					modes.Add((name, EmittedCode.Compile(result.Sources[0].Text), options.BufferedInput));
				}
				catch (Exception thrown) when (thrown.Message.Contains("CS0162", StringComparison.Ordinal))
				{
					// An alternative after an optional one is unreachable, and the emitted code
					// says so with a warning: the same over ASCII and on the parent commit, and
					// asked of PositionalFormFindingsTests rather than here.
				}
			}

			if (modes.Count == 0)
				continue;

			compiled++;

			var graph = Normalized(grammar);
			var start = graph.Rules.First(static rule => rule.Name == "Start");

			for (var round = 0; round < 40; round++)
			{
				var input  = Input(random);
				var oracle = ReferenceInterpreter.Parses(graph, start, input);

				foreach (var (name, assembly, buffered) in modes)
				{
					try
					{
						var answer = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

						if (answer.IsSuccess != oracle)
							failures.Add($"{name}: {answer.IsSuccess}, semantics {oracle}; input \"{input}\"\n{grammar}");

						var quiet = Quietly(assembly, input);

						if (quiet != oracle)
							failures.Add($"{name}, bool form: {quiet}, semantics {oracle}; input \"{input}\"\n{grammar}");

						// A reading begun where it is told stops where the rule does: the same in
						// a window of a longer text as over the text alone.
						// Only where the publication has the forms (a plain method has not).
						if (Positional(assembly))
						{
							var begun  = EmittedCode.Answered(assembly, "Grammar", "TryParseStart", input, 0);
							var window = EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "##" + input + "##", 2, input.Length);
							var told   = (begun.Read, begun.At);

							if ((window.Read, window.At - 2) != told)
								failures.Add($"{name}, window: {(window.Read, window.At - 2)}, alone {told}; input \"{input}\"\n{grammar}");
						}

						if (buffered)
						{
							var streamed = Streamed(assembly, input);

							if (streamed != oracle)
								failures.Add($"{name}, TextReader: {streamed}, semantics {oracle}; input \"{input}\"\n{grammar}");
						}
					}
					catch (Exception thrown)
					{
						failures.Add($"{name}: threw {thrown.GetBaseException().GetType().Name}: {thrown.GetBaseException().Message}; input \"{input}\"\n{grammar}");
					}
				}
			}
		}

		Assert.True(failures.Count == 0, $"seed {seed}:\n" + string.Join("\n", failures.Distinct().Take(10)));
	}

	/// <summary>
	/// The titlecase letter is a case of each of its pair and they are not cases of it: the
	/// relation is the literal's. Asked of every rendering, single and inside a keyword.
	/// </summary>
	[Theory]
	[InlineData("ǅ",  "Ǆ", true)]
	[InlineData("ǅ",  "ǆ", true)]
	[InlineData("Ǆ",  "ǅ", false)]
	[InlineData("ǆ",  "ǅ", false)]
	[InlineData("Ǆ",  "ǆ", true)]
	[InlineData("aǅ", "AǄ", true)]
	[InlineData("aǄ", "Aǅ", false)]
	[InlineData("ǅa", "ǆA", true)]
	[InlineData("Ǆa", "ǅA", false)]
	[InlineData("ßs", "ẞS", false)]
	[InlineData("ẞs", "ßS", true)]
	[InlineData("ςσ", "ΣΣ", true)]
	[InlineData("ΣΣ", "ςσ", false)]
	[InlineData("sς", "ſΣ", false)]
	[InlineData("k\u03BC", "K\u039C", true)]
	[InlineData("k\u03BC", "K\u00B5", false)]
	public void A_literal_reads_what_its_own_characters_fold_to_in_every_rendering(string literal, string input, bool expected)
	{
		var grammar = $"trivia = {{ ' '* }}\nStart = \"{literal}\"i | '(' & \"{literal}\"i & ')' | \"{literal}\"i & \"{literal}\"i\nparse Start\n";
		var failures = new List<string>();

		foreach (var (name, options) in Modes())
		{
			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text);

			foreach (var text in new[] { input, "(" + input + ")", input + input, " " + input + " " })
			{
				var answer = EmittedCode.Match(assembly, "Grammar", "TryParseStart", text).IsSuccess;

				if (answer != expected)
					failures.Add($"{name}: \"{literal}\"i against \"{text}\" read {answer}, wanted {expected}");

				if (options.BufferedInput && Streamed(assembly, text) != expected)
					failures.Add($"{name}, TextReader: \"{literal}\"i against \"{text}\" read {!expected}, wanted {expected}");
			}
		}

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	/// <summary>
	/// What an ignore-case literal reads is decided where the parser is generated, so it must
	/// not hang on which casing tables the generating process happened to find.
	/// </summary>
	/// <remarks>
	/// <c>char.ToUpperInvariant</c> reads the system ICU where there is one and the runtime's
	/// own tables in invariant globalization mode (and NLS under .NET Framework, which is what
	/// an editor may host the generator on). On one machine, .NET 10 over ICU 78 pairs U+A7CE
	/// with U+A7CF (Unicode 17) and the same .NET 10 with
	/// <c>DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1</c> does not; .NET 8 and 9 in invariant mode
	/// do not pair U+0264 with U+A7CB (Unicode 16) and .NET 10 does. So one grammar generates
	/// two parsers on two build machines. The expected answers are one fixed table's (Unicode
	/// 16, what .NET 10's own tables say); any fixed table would do, as long as it is the
	/// generator's own.
	/// </remarks>
	[Theory]
	[InlineData('\uA7CE', null)]
	[InlineData('\uA7CF', null)]
	[InlineData('\uA7D2', null)]
	[InlineData('\uA7D4', null)]
	[InlineData('\uA7CB', '\u0264')]
	[InlineData('\u0264', '\uA7CB')]
	[InlineData('\u019B', '\uA7DC')]
	public void What_a_literal_reads_does_not_depend_on_the_host_casing_tables(char letter, char? partner)
	{
		var expected = partner is { } other ? new[] { letter, other }.OrderBy(static one => one).ToArray() : [letter];

		Assert.Equal(expected, CaseFold.Of(letter));
	}

	/// <summary>
	/// Keywords beyond ASCII under recovery and <c>find</c>, over a string and over a reader
	/// that hands one character a call: every rendering reads the same rows, refuses at the same
	/// place with the same message, and finds the same rows.
	/// </summary>
	[Theory]
	[InlineData("\u03A3\u0395\u039B \u01C4AB;")]
	[InlineData("\u03C2\u03B5\u03BB;")]
	[InlineData("\u03C3\u03B5\u03BB \u01C5ab;\u03C2\u03B5\u03BB;sel;")]
	[InlineData("\u01C6ab;\u01C5AB;\u01C4ab;")]
	[InlineData("\u00DFS;\u1E9Es;")]
	[InlineData("\u017Fel;SEL;")]
	[InlineData("K\u00B5;k\u039C;K\u03BC;")]
	[InlineData("\u03C3\u03B5")]
	[InlineData("sel \u03C3\u03B5\u03BB ; \u01C5a")]
	[InlineData("")]
	public void Recovery_and_find_agree_in_every_rendering(string input)
	{
		const string grammar = """
			trivia = { ' '* }
			Kw = "\u03C3\u03B5\u03BB"i | "\u01C5ab"i | "\u1E9Es"i | "sel"i | "k\u03BC"i
			Row = Kw & Kw? & ';'
			Start = Row* recover ';' & eof
			parse Start
			parse Row as Rows
			find Row
			""";

		var graph    = Normalized(grammar);
		var row      = graph.Rules.First(static rule => rule.Name == "Row");
		var answers  = new List<(string Name, string Answer)>();

		foreach (var (name, options) in Modes())
		{
			if (options.Lexical)
				continue;

			var result = GramCompiler.Compile(grammar, options);

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text);
			var whole    = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
			var rows     = EmittedCode.Match(assembly, "Grammar", "TryRows", input);
			var found    = string.Join(",", EmittedCode.FoundAt(assembly, "Grammar", "FindRow", input));
			var answer   = $"start {whole.IsSuccess} at {whole.Position}: {whole.Error}; rows {rows.IsSuccess} at {rows.Position}: {rows.Error}; found {found}";

			Assert.Equal(ReferenceInterpreter.Parses(graph, row, input), rows.IsSuccess);

			if (options.BufferedInput)
			{
				var streamed = string.Join(",", FoundAtReader(assembly, input));

				answers.Add((name + ", TextReader find", streamed == found ? "same" : $"found {streamed}, over the string {found}"));
			}

			answers.Add((name, answer));
		}

		var expected = answers[0].Answer;

		Assert.All(answers, one => Assert.True(
			one.Answer == expected || one.Answer == "same", $"{one.Name}: {one.Answer}\nengine: {expected}"));
	}

	static IEnumerable<long> FoundAtReader(Assembly assembly, string input)
	{
		var type   = assembly.GetType("Grammar")!;
		var method = type.GetMethods().First(static one =>
			one.Name == "FindRow" && one.GetParameters() is { Length: >= 1 } taken && taken[0].ParameterType == typeof(TextReader));
		var arguments = method.GetParameters().Select(static one => one.HasDefaultValue ? one.DefaultValue : null).ToArray();

		arguments[0] = new Trickle(input);

		foreach (var match in (System.Collections.IEnumerable)method.Invoke(null, arguments)!)
			yield return Convert.ToInt64(match!.GetType().GetProperty("Position")!.GetValue(match));
	}

	static IEnumerable<(string Name, GramCompilerOptions Options)> Modes()
	{
		yield return ("engine", Options(static one => one.Direct = false));
		yield return ("direct", Options(static _ => { }));
		yield return ("direct, tape", Options(static one => one.Carrier = CarrierKind.Tape));
		yield return ("no prefix tables", Options(static one => { one.Direct = false; one.PrefixTables = false; }));
		yield return ("lexical", Options(static one => one.Lexical = true));
		yield return ("buffered engine", Options(static one => { one.Direct = false; one.BufferedInput = true; one.BufferSize = 1; }));
		yield return ("buffered direct", Options(static one => { one.BufferedInput = true; one.BufferSize = 1; }));
	}

	static GramCompilerOptions Options(Action<GramCompilerOptions> change)
	{
		var options = new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance };

		change(options);

		return options;
	}

	static bool Positional(Assembly assembly)
	{
		return assembly.GetType("Grammar")!.GetMethods().Any(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is { Length: 3 } taken && taken[1].ParameterType.IsByRef);
	}

	static bool Quietly(Assembly assembly, string input)
	{
		var type   = assembly.GetType("Grammar")!;
		var method = type.GetMethods().Single(static one =>
			one.Name == "TryParseStart" && one.ReturnType == typeof(bool) &&
			one.GetParameters() is { Length: 2 } taken && taken[0].ParameterType == typeof(string) && taken[1].IsOut);

		return (bool)method.Invoke(null, [input, null])!;
	}

	static bool Streamed(Assembly assembly, string input)
	{
		var type   = assembly.GetType("Grammar")!;
		var method = type.GetMethods().First(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is { Length: >= 1 } taken &&
			taken[0].ParameterType == typeof(TextReader));
		var arguments = method.GetParameters().Select(static one => one.HasDefaultValue ? one.DefaultValue : null).ToArray();

		arguments[0] = new Trickle(input);

		var match = method.Invoke(null, arguments)!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	/// <summary>A reader that hands over one character a call, so every buffer refills.</summary>
	sealed class Trickle(string text) : TextReader
	{
		int _at;

		public override int Peek()
		{
			return _at < text.Length ? text[_at] : -1;
		}

		public override int Read()
		{
			return _at < text.Length ? text[_at++] : -1;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			if (_at >= text.Length || count == 0)
				return 0;

			buffer[index] = text[_at++];

			return 1;
		}
	}

	// ── The generator ────────────────────────────────────────────────────────────

	static string Generate(Random random)
	{
		var rules = random.Next(1, 4);
		var text  = new StringBuilder();

		text.AppendLine("trivia = { ' '* }");

		for (var i = 0; i < rules; i++)
		{
			text.Append(i == 0 ? "Start = " : $"R{i} = ");
			text.AppendLine(Body(random, 0, i + 1, rules));
		}

		text.AppendLine("parse Start");

		return text.ToString();
	}

	static string Body(Random random, int depth, int callableFrom, int ruleCount)
	{
		var pick = depth >= 3 ? random.Next(3) : random.Next(9);

		switch (pick)
		{
			case 0:
			case 1:
			{
				var length  = random.Next(1, 4);
				var literal = new string(Enumerable.Range(0, length).Select(_ => Alphabet[random.Next(Alphabet.Length)]).ToArray());
				var folded  = random.Next(4) != 0 ? "i" : "";

				return (literal.Length == 1 ? $"'{literal}'" : $"\"{literal}\"") + folded;
			}

			case 2:
				return callableFrom < ruleCount ? $"R{random.Next(callableFrom, ruleCount)}" : "'a'i";

			case 3:
			case 4:
			{
				var parts = new string[random.Next(2, 4)];

				for (var i = 0; i < parts.Length; i++)
					parts[i] = Body(random, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" & ", parts) + ")";
			}

			case 5:
			case 6:
			{
				var alternatives = new string[random.Next(2, 5)];

				for (var i = 0; i < alternatives.Length; i++)
					alternatives[i] = Body(random, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" | ", alternatives) + ")";
			}

			case 7:
				return Body(random, depth + 1, callableFrom, ruleCount) + random.Next(4) switch
				{
					0 => "?",
					1 => "*",
					2 => "+",
					_ => "{1,2}",
				};

			default:
				return (random.Next(2) == 0 ? "?= " : "?! ") + Body(random, depth + 1, callableFrom, ruleCount);
		}
	}

	static string Input(Random random)
	{
		var length = random.Next(0, 6);
		var text   = new StringBuilder(length);

		for (var i = 0; i < length; i++)
			text.Append(random.Next(8) == 0 ? ' ' : Alphabet[random.Next(Alphabet.Length)]);

		return text.ToString();
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
