using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A host recognizer that refuses inside a lookahead is the lookahead's answer, not the parse's:
/// <c>?!@Odd</c> goes on where <c>Odd</c> refuses. The refusal used to leave for the parse's own
/// failure door, so the negative form refused everything and wrote code after it that nothing could
/// reach (CS0162, CS0164).
/// </summary>
public sealed class ExternalLookaheadTests
{
	const string Host = """
		static bool Odd(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == 'o') { p++; return true; } return false; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == 'x') p++; return true; }
		""";

	/// <summary>The same over the input view, which is what a parser that reads from a reader asks the host for.</summary>
	const string ViewHost = """
		static bool Odd(ParserInput<char> input, ref int p) { if (input.Peek(p, out var c) && c == 'o') { p++; return true; } return false; }
		static bool Half(ParserInput<char> input, ref int p) { if (input.Peek(p, out var c) && c == 'x') p++; return true; }
		""";

	/// <summary>Each grammar with the inputs it takes and the ones it refuses.</summary>
	public static TheoryData<string, string, string> Rows => new()
	{
		{ "T = ?!@Odd & 'a' & eof",           "a",          "|oa|x|ao" },
		{ "T = ?!@Odd & @Half & eof",         "|x",         "o|ox|xx|xo" },
		{ "T = ?=@Odd & 'o' & eof",           "o",          "|a|oo" },
		{ "T = ?=@Odd & @Half & 'o' & eof",   "o",          "|a|xo" },
		{ "T = (?!@Odd & any)* & eof",        "|a|abc",     "o|ao|abo" },
		{ "T = 'a' & (?!@Odd & 'b')* & eof",  "a|ab|abb",   "ao|abo|b" },
		{ "T = (?!@Odd & 'a' | 'o')* & eof",  "|a|ao|oo",   "b|aob" },
	};

	[Theory]
	[MemberData(nameof(Rows))]
	public void A_lookahead_over_a_host_recognizer_answers_for_the_lookahead(string grammar, string taken, string refused)
	{
		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
				foreach (var buffered in new[] { false, true })
				{
					var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
					{
						ClassName      = "Probe",
						Direct         = direct,
						Carrier        = carrier,
						BufferedInput  = buffered,
						SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
						CSharpScanner  = RoslynCSharpScanner.Instance,
					});

					Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

					// Warnings count as failures there: no label nothing jumps to, no code after a branch that leaves.
					var assembly = EmittedCode.Compile(
						result.Sources[0].Text, className: "Probe", declarationMembers: buffered ? ViewHost : Host,
						sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

					foreach (var input in taken.Split('|'))
						Assert.True(Read(assembly, input, buffered), $"direct {direct}, {carrier}, buffered {buffered}: \"{input}\" is refused");

					foreach (var input in refused.Split('|'))
						Assert.False(Read(assembly, input, buffered), $"direct {direct}, {carrier}, buffered {buffered}: \"{input}\" is taken");
				}
	}

	static bool Read(Assembly assembly, string input, bool buffered)
	{
		if (!buffered)
			return EmittedCode.Match(assembly, "Probe", "TryParseT", input).IsSuccess;

		var match = assembly.GetType("Probe")!.GetMethod("TryParseT", [typeof(TextReader), typeof(int?), typeof(int?)])!
			.Invoke(null, [new StringReader(input), null, null])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}
}
