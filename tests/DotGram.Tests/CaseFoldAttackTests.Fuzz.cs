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
/// What the fuzz of the ignore-case attacks is made of: the renderings, the random grammars and
/// inputs, and the forms of call. Shared with DotGram.Tests.Slow, which runs the seeds the
/// ordinary run leaves out.
/// </summary>
public sealed partial class CaseFoldAttackTests
{
	/// <summary>
	/// s/S/ſ, k/K/Kelvin, σ/ς/Σ, the titlecase triple, ß/ẞ, i/I/ı/İ, μ/Μ/micro, and one
	/// letter with an ordinary pair.
	/// </summary>
	const string Unpaired = "sS\u017FkK\u212A\u03C3\u03C2\u03A3\u01C5\u01C6\u01C4\u00DF\u1E9EiI\u0131\u0130\u00B5\u03BC\u039Ca";

	static void Fuzz(int seed, string alphabet)
	{
		var random   = new Random(seed * 104729);
		// Inputs draw from a stream of their own, so the grammars a seed makes do not depend on how
		// many draws its inputs took.
		var drawing  = new Random(seed * 7919 + 1);
		var compiled = 0;
		var failures = new List<string>();

		while (compiled < 25 && failures.Count < 20)
		{
			var literals = new List<string>();
			var grammar  = Generate(random, alphabet, literals);
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
				var input  = Input(drawing, alphabet, literals);
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

	/// <summary>A grammar of up to three rules; every literal it writes is added to <paramref name="literals"/>.</summary>
	static string Generate(Random random, string alphabet, List<string> literals)
	{
		var rules = random.Next(1, 4);
		var text  = new StringBuilder();

		text.AppendLine("trivia = { ' '* }");

		for (var i = 0; i < rules; i++)
		{
			text.Append(i == 0 ? "Start = " : $"R{i} = ");
			text.AppendLine(Body(random, alphabet, literals, 0, i + 1, rules));
		}

		text.AppendLine("parse Start");

		return text.ToString();
	}

	static string Body(Random random, string alphabet, List<string> literals, int depth, int callableFrom, int ruleCount)
	{
		var pick = depth >= 3 ? random.Next(3) : random.Next(9);

		switch (pick)
		{
			case 0:
			case 1:
			{
				var length  = random.Next(1, 4);
				var literal = new string(Enumerable.Range(0, length).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
				var folded  = random.Next(4) != 0 ? "i" : "";

				literals.Add(literal);

				return (literal.Length == 1 ? $"'{literal}'" : $"\"{literal}\"") + folded;
			}

			case 2:
				return callableFrom < ruleCount ? $"R{random.Next(callableFrom, ruleCount)}" : "'a'i";

			case 3:
			case 4:
			{
				var parts = new string[random.Next(2, 4)];

				for (var i = 0; i < parts.Length; i++)
					parts[i] = Body(random, alphabet, literals, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" & ", parts) + ")";
			}

			case 5:
			case 6:
			{
				var alternatives = new string[random.Next(2, 5)];

				for (var i = 0; i < alternatives.Length; i++)
					alternatives[i] = Body(random, alphabet, literals, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" | ", alternatives) + ")";
			}

			case 7:
				return Body(random, alphabet, literals, depth + 1, callableFrom, ruleCount) + random.Next(4) switch
				{
					0 => "?",
					1 => "*",
					2 => "+",
					_ => "{1,2}",
				};

			default:
				return (random.Next(2) == 0 ? "?= " : "?! ") + Body(random, alphabet, literals, depth + 1, callableFrom, ruleCount);
		}
	}

	/// <summary>
	/// An input: half the time characters of the alphabet at random, which a grammar mostly
	/// refuses; otherwise one to three of the grammar's own literals, each character of them
	/// spelled as itself, as its upper or lower case, or now and then as any letter of the
	/// alphabet — so that the fuzz asks what a literal accepts, and what only nearly matches it.
	/// </summary>
	static string Input(Random random, string alphabet, List<string> literals)
	{
		var text = new StringBuilder();

		if (literals.Count == 0 || random.Next(2) == 0)
		{
			var length = random.Next(0, 6);

			for (var i = 0; i < length; i++)
				text.Append(random.Next(8) == 0 ? ' ' : alphabet[random.Next(alphabet.Length)]);

			return text.ToString();
		}

		var count = random.Next(1, 4);

		for (var i = 0; i < count; i++)
		{
			foreach (var one in literals[random.Next(literals.Count)])
			{
				text.Append(random.Next(8) switch
				{
					0 or 1 => CaseFold.Upper(one),
					2 or 3 => CaseFold.Lower(one),
					4      => alphabet[random.Next(alphabet.Length)],
					_      => one,
				});
			}

			if (random.Next(4) == 0)
				text.Append(' ');
		}

		return text.ToString();
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
