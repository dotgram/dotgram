using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a generated parser says about input it refuses — the outcome, the position and the
/// whole message, which carries every set expected there and every set tied with it — and
/// the record of what it said before, in every rendering.
/// </summary>
/// <remarks>
/// <para>
/// A parse may be read in more than one way before it answers: a first reading that records
/// nothing, and a second that records, run only where the first refused (Q7.2,
/// docs/design/diagnostics-off-the-hot-path-2026-09-18.md). Whatever the generator does to get
/// there, a refusal has to come out as it did when a single reading recorded as it went. So
/// the answers are kept in <c>RefusalTests.txt</c>, written by the generator as it was, and
/// every change since is held to it.
/// </para>
/// <para>
/// The inputs are made from the accepted ones rather than written: every proper prefix of
/// each, each with one character left out, and each with one character too many. That is
/// where refusals are that are neither at the first character nor at the end — a tie between
/// alternatives, an `on fail` spoken where a rule was entered, a look that got further than
/// the parse.
/// </para>
/// <para>
/// Held two ways, because what costs is compiling a parser for every shape in every rendering
/// (D12): <c>DotGram.Tests</c> compiles one in five of them, a different rendering for each
/// shape, and holds it to its lines of the record; <c>DotGram.Tests.Slow</c>, which links this
/// file, compiles them all and holds the whole record, and is what a change to how failures
/// are recorded is gated on.
/// </para>
/// </remarks>
static class RefusalCorpus
{
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes =
	[
		.. CarrierShapes.All,

		("alternatives that fail at one place",
			"trivia = ' '*\n" +
			"Start : @string = a: Name & '=' & v: Digits => @(a + \"=\" + v)\n" +
			"                | a: Name & ':' & v: Name => @(a + \":\" + v)\n" +
			"                | a: Name & '(' & v: Digits & ')' => @(a + \"(\" + v + \")\")\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1", "a : b", "a ( 12 )"]),

		("a rule that says what it is",
			"trivia = ' '*\n" +
			"Start : @string = n: Name & ',' & m: Name => @(n + m)\n" +
			"Name : @string on fail \"Expected a name.\" = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a , b", "ab,cd"]),

		// A loop that ends where its turn cannot begin, and the end refused after it: the turn's
		// refusal is half of what is said there, and the reader, which does not try a turn it
		// can see will not begin, used to say only that the input did not match.
		("turns before the end",
			"trivia = ' '*\n" +
			"Item : @string = t: 'b' => @(t)\n" +
			"Start : @string[] = '[' & (Item & ';')* & eof\n" +
			"parse Start\n",
			["[b;b;", "[ b ; b ; ", "["]),

		("turns before the end, unspaced",
			"Item : @string = t: 'b' => @(t)\n" +
			"Start : @string[] = (Item & ';')* & eof\n" +
			"parse Start\n",
			["b;b;", ""]),

		("turns of two beginnings before the end",
			"trivia = ' '*\n" +
			"Item : @string = t: ['a'..'z']+ => @(t) | n: ['0'..'9']+ => @(n)\n" +
			"Start : @string[] = (Item & ',')* & eof\n" +
			"parse Start\n",
			["ab, 12 ,", "a,"]),

		("a look ahead",
			"trivia = ' '*\n" +
			"Start : @string = ?=(Name & '=') & n: Name & '=' & v: Name => @(n + v)\n" +
			"                | n: Name & ?!'=' => @(n)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a = b", "a"]),

		// A choice of literals is refused at the deepest character any of them agreed with,
		// naming those still agreeing there; a lone literal, where it stopped fitting.
		("literals that part late",
			"Start = Word | '[' & Start & ']' | \"lone\" & '!'\n" +
			"Word = \"abcdef\" | \"abcxyz\" | \"abq\"\n" +
			"parse Start\n",
			["abcdef", "[abcxyz]", "abq", "lone!"]),
	];

	static readonly (string Name, bool Direct, CarrierKind Carrier)[] Renderings =
	[
		("engine",    false, CarrierKind.Tape),
		("tape",      true,  CarrierKind.Tape),
		("immediate", true,  CarrierKind.Immediate),
	];

	/// <summary>Every shape in every rendering, over characters and, where it has trivia, over tokens, in the record's order.</summary>
	static IEnumerable<(string Name, string Grammar, string[] Inputs, bool Lexical, (string Name, bool Direct, CarrierKind Carrier) Rendering)> Readings()
	{
		foreach (var (name, grammar, inputs) in Shapes)
			foreach (var lexical in grammar.Contains("trivia", StringComparison.Ordinal) ? new[] { false, true } : [false])
				foreach (var rendering in Renderings)
					yield return (name, grammar, inputs, lexical, rendering);
	}

	/// <summary>The whole record, compiled and read again: nothing may have moved.</summary>
	public static void AssertWhole()
	{
		var actual = Answers(Readings());

		if (!File.Exists(Expected))
		{
			File.WriteAllText(Expected, actual, Utf8);

			Assert.Fail($"No record of the refusals; wrote one to {Expected}. Read it, and commit it if it is right.");
		}

		if (File.ReadAllText(Expected).Replace("\r\n", "\n") == actual)
			return;

		File.WriteAllText(Expected + ".actual", actual, Utf8);

		Assert.Fail($"A refusal is not the one it was; what it is now is in {Expected}.actual.");
	}

	/// <summary>One reading in five, held to its own lines of the record.</summary>
	public static void AssertSample()
	{
		var sampled = Readings().Where((_, index) => index % 5 == 0).ToList();
		var keys    = new HashSet<string>(sampled.Select(Key), StringComparer.Ordinal);
		var actual  = Answers(sampled);

		// The record's lines of the same readings, in the record's order, which is the order
		// they are read in: its first two fields are the shape and the rendering.
		var recorded = string.Concat(
			File.ReadAllText(Expected).Replace("\r\n", "\n").Split('\n')
				.Where(line => line.Length > 0 && keys.Contains(KeyOf(line)))
				.Select(line => line + "\n"));

		Assert.True(
			recorded == actual,
			$"A refusal is not the one it was; DotGram.Tests.Slow runs the whole record and writes what differs beside {Expected}.");
	}

	static string Key((string Name, string Grammar, string[] Inputs, bool Lexical, (string Name, bool Direct, CarrierKind Carrier) Rendering) reading) =>
		reading.Name + " | " + reading.Rendering.Name + (reading.Lexical ? " over tokens" : "");

	static string KeyOf(string line)
	{
		var first = line.IndexOf(" | ", StringComparison.Ordinal);

		return line.Substring(0, line.IndexOf(" | ", first + 3, StringComparison.Ordinal));
	}

	static string Answers(IEnumerable<(string Name, string Grammar, string[] Inputs, bool Lexical, (string Name, bool Direct, CarrierKind Carrier) Rendering)> readings)
	{
		var answers = new StringBuilder();

		foreach (var reading in readings)
		{
			var probe = Compiled(reading.Grammar, reading.Rendering.Direct, reading.Rendering.Carrier, reading.Lexical);

			foreach (var input in Refused(reading.Inputs))
				answers
					.Append(Key(reading)).Append(" | ")
					.Append(Escaped(input)).Append(" | ")
					.Append(Answer(probe, input))
					.Append('\n');
		}

		return answers.ToString();
	}

	/// <summary>Every input made from the accepted ones, refused or not, each once and in order.</summary>
	static IEnumerable<string> Refused(string[] inputs)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var input in inputs)
		{
			for (var length = 0; length < input.Length; length++)
				if (seen.Add(input.Substring(0, length)))
					yield return input.Substring(0, length);

			for (var at = 0; at < input.Length; at++)
				if (seen.Add(input.Remove(at, 1)))
					yield return input.Remove(at, 1);

			if (input.Length == 0)
				continue;

			foreach (var extra in new[] { input + "!", input + input.Substring(input.Length - 1) })
				if (seen.Add(extra))
					yield return extra;
		}
	}

	/// <summary>What the parse said: its value where it accepted, its outcome, position and message where it did not.</summary>
	static string Answer(Assembly probe, string input)
	{
		try
		{
			var match = EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", input);

			return match.IsSuccess
				? "accepted"
				: $"{EmittedCode.Outcome(probe, "Refused.Probe", "TryParseStart", input)} at {match.Position}: {match.Error}";
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			return "threw " + inner.GetType().Name;
		}
	}

	static string Escaped(string input) => "\"" + input.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

	static Assembly Compiled(string grammar, bool direct, CarrierKind carrier, bool lexical)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Refused",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
			Carrier       = carrier,
			Lexical       = lexical,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, "Probe", "Refused");
	}

	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	/// <summary>The record, beside this file wherever it is compiled from.</summary>
	static string Expected => Path.Combine(Path.GetDirectoryName(ThisFile)!, "RefusalTests.txt");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
