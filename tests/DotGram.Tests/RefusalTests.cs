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
/// whole message, which carries every set expected there and every set tied with it — held
/// against what it said before, in every rendering.
/// </summary>
/// <remarks>
/// <para>
/// A parse may be read in more than one way before it answers: a first reading that records
/// nothing, and a second that records, run only where the first refused (Q7.2,
/// docs/design/diagnostics-off-the-hot-path-2026-09-18.md). Whatever the generator does to get
/// there, a refusal has to come out as it did when a single reading recorded as it went. So
/// the answers are kept in a file beside this one, written by the generator as it was, and
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
/// A file that does not exist is written and the test fails, as a snapshot does; a file that
/// differs is written beside it as <c>.actual</c>, for a diff.
/// </para>
/// </remarks>
public sealed class RefusalTests
{
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes =
	[
		.. CarrierTests.Shapes,

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

		("a look ahead",
			"trivia = ' '*\n" +
			"Start : @string = ?=(Name & '=') & n: Name & '=' & v: Name => @(n + v)\n" +
			"                | n: Name & ?!'=' => @(n)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a = b", "a"]),
	];

	static readonly (string Name, bool Direct, CarrierKind Carrier)[] Renderings =
	[
		("engine",    false, CarrierKind.Tape),
		("tape",      true,  CarrierKind.Tape),
		("immediate", true,  CarrierKind.Immediate),
	];

	[Fact]
	public void Every_refusal_is_the_one_it_was()
	{
		var answers = new StringBuilder();

		foreach (var (name, grammar, inputs) in Shapes)
			foreach (var lexical in grammar.Contains("trivia", StringComparison.Ordinal) ? new[] { false, true } : [false])
				foreach (var rendering in Renderings)
				{
					var probe = Compiled(grammar, rendering.Direct, rendering.Carrier, lexical);

					foreach (var input in Refused(inputs))
						answers
							.Append(name).Append(" | ")
							.Append(rendering.Name).Append(lexical ? " over tokens" : "").Append(" | ")
							.Append(Escaped(input)).Append(" | ")
							.Append(Answer(probe, input))
							.Append('\n');
				}

		var actual = answers.ToString();

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

	/// <summary>
	/// A refusal is read twice, and on the immediate carrier what the first reading ran the
	/// second runs again — once more, and only where the parse refused.
	/// </summary>
	[Fact]
	public void A_refusal_runs_a_construction_once_more_and_an_acceptance_does_not()
	{
		// Recursive, so that it is read by methods and not lowered to a flat rendering, which
		// runs its constructions only once it has accepted.
		const string Counting =
			"Start : @string = n: Name & '=' => @(n)\n" +
			"                | '(' & s: Start & ')' => @(s)\n" +
			"Name : @string = t: ['a'..'z']+ => @(Hit(t))\n" +
			"parse Start\n";

		var result = GramCompiler.Compile(Counting, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Refused",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = CarrierKind.Immediate,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);

		var probe = EmittedCode.Compile(
			source, "Probe", "Refused",
			"public static int Hits; static string Hit(string t) { Hits++; return t; }");

		var hits = probe.GetType("Refused.Probe")!.GetField("Hits")!;

		Assert.True(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab=").IsSuccess);
		Assert.Equal(1, (int)hits.GetValue(null)!);

		hits.SetValue(null, 0);

		Assert.False(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab").IsSuccess);
		Assert.Equal(2, (int)hits.GetValue(null)!);
	}

	/// <summary>
	/// A grammar with a context keeps the one recording reading: what its guards write into
	/// the context would still be there for a second one.
	/// </summary>
	[Fact]
	public void A_grammar_with_a_context_reads_once_and_records()
	{
		var result = GramCompiler.Compile(
			"context : @System.Text.StringBuilder\n" +
			"Start = t: ['a'..'z']+ & when @(context.Append(t) != null) & '='\n" +
			"parse Start\n",
			new GramCompilerOptions
			{
				ClassName     = "Probe",
				Namespace     = "Refused",
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		var source = Assert.Single(result.Sources).Text;

		Assert.DoesNotContain("Quiet = true", source, StringComparison.Ordinal);
		Assert.Contains("var failure = new Failure();", source, StringComparison.Ordinal);
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

	static string Expected => Path.Combine(Path.GetDirectoryName(ThisFile)!, "RefusalTests.txt");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
