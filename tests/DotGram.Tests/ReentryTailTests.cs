using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The check at a rule that comes back to itself through a recognizer that does not move (docs/syntax.md §7.2): where
/// nothing is read after the call back, an entry where one already stands is cut at once; elsewhere there is no check,
/// and the rule is written as it is without one.
/// </summary>
/// <remarks>
/// The cut at once is held to the parser without the check on every input up to five items over each shape's alphabet:
/// wherever that one ends, the same verdict, end, outcome and message, and the cut ends wherever it does not. Where the
/// shape is not one the cut applies to, the generated parser is the one without the check, character for character.
/// </remarks>
public sealed class ReentryTailTests
{
	/// <summary>The recognizers, which count their calls: past a cap no answer that ends comes near, the parse went round.</summary>
	const string Host = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 20000) throw new System.InvalidOperationException("went round"); }
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Ws(System.ReadOnlySpan<char> input, ref int p) { Count(); while (p < input.Length && input[p] == ' ') p++; return true; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') p++; return true; }
		""";

	/// <summary>The recognizer that never moves, not counting, for long input: over a span, and over the input view for a reader and a stream.</summary>
	const string LongHost = """
		public static void Reset() { }
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { return true; }
		""";

	/// <summary>The same over the input view.</summary>
	const string ViewHost = """
		public static void Reset() { }
		static bool Still(ParserInput<char> input, ref int p) { return true; }
		static bool Still(ParserInput<byte> input, ref int p) { return true; }
		""";

	/// <summary>The shapes, each with whether nothing is read after its call back, and the items its inputs are made of.</summary>
	static readonly (string Name, string Grammar, bool Tail, string Alphabet)[] Shapes =
	[
		("self",       "T = @Still & T | 'a' & T | eof",                                  true,  "ab"),
		("mutual",     "T = @Still & U | 'a'\nU = @Still & T | 'b' & T",                  true,  "ab"),
		("half",       "T = @Half & T | eof",                                             true,  ".x"),
		("second",     "T = 'a' & T | @Still & T | 'b'",                                  true,  "ab"),
		("giveback",   "T = S & 'c'\nS = @Still & S | 'a' | 'a' & 'b'",                   true,  "abc"),
		("lookahead",  "T = ?!'b' & @Still & T | 'a' & T | 'b'",                          true,  "ab"),
		("list",       "T = S & eof\nS = @Ws & S | Num & ('+' & S)?\nNum = ['0'..'9']+",  true,  "1+ "),
		("eof",        "T = A & eof\nA = @Still & A | 'a' & A | 'b'",                     true,  "abc"),
		("value",      "T = d: A & when @(d >= 2)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", false, "a"),
		("sum",        "T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+", false, "1+ "),
		("abc",        "T = A & 'c'\nA = 'a' | @Still & A & 'b'",                         false, "abc"),
		("block",      "T = @Still & T & 'e' | 'x'",                                      false, "xe"),
		("mixed",      "T = A & eof\nA = @Still & A | 'a' & A | 'b' | @Still & A & 'z'",  false, "abz"),
	];

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var shape in Shapes)
			names.Add(shape.Name);

		return names;
	}

	/// <summary>
	/// Which cycles are cut at once: none where something is read after the call back, or what is read depends on more
	/// than the position; those are found all the same, and told.
	/// </summary>
	[Theory]
	[InlineData("T = @Still & T | 'a' & T | eof", true)]
	[InlineData("T = S & eof\nS = @Ws & S | Num & ('+' & S)?\nNum = ['0'..'9']+", true)]
	[InlineData("T = ?='.' & @Still & T | ?!'.' & 'x'", true)]
	[InlineData("T = x: A\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", true)]
	[InlineData("T = @Still & T & 'e' | 'x'", false)]
	[InlineData("T = (@Still & T | 'a') & 'b'", false)]
	[InlineData("T = ?=(@Still & T) & 'x' | 'y'", false)]
	[InlineData("T = 'x' | ?!'y' & (@Still & T)+", false)]
	[InlineData("T = { @Still & T } | 'a'", false)]
	[InlineData("T = d: A & when @(d >= 2)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", false)]
	[InlineData("T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'", false)]
	[InlineData("context : @Ctx\nT = A\nA = when @(context.Depth >= 5) & 'x' | when @(context.Bump() > 0) & @Still & A", false)]
	[InlineData("T = A & eof\nA = @Still & A | 'a' & A | 'b' | @Still & A & 'z'", false)]
	public void Cut_at_once_where_nothing_is_read_after_the_call_back(string grammar, bool tail)
	{
		var graph = ExponentialRepetitionTests.Graph(grammar + "\nparse T", lexical: false, out _);
		var found = Reentry.FoundRules(graph);

		Assert.NotEmpty(found);

		foreach (var rule in found)
			Assert.Equal(tail, Reentry.Marked(graph, rule));

		Assert.Equal(tail, Reentry.Any(graph));
	}

	/// <summary>
	/// The cut at once answers as the parser without the check does wherever that one ends, input for input, in the
	/// reader by methods and in the engine, and ends wherever it does not; where a shape is not cut at once, its parser
	/// is the one without the check.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void A_cut_at_once_answers_as_the_parser_without_it(string name)
	{
		var (_, grammar, tail, alphabet) = Shapes.Single(shape => shape.Name == name);

		var (ended, round) = (0, 0);

		foreach (var direct in new[] { true, false })
		{
			var (cut, cutSource)         = Build(grammar, direct, unchecked_: false);
			var (without, withoutSource) = Build(grammar, direct, unchecked_: true);

			Assert.Equal(tail, cutSource != withoutSource);

			if (!tail)
				continue;

			foreach (var input in Inputs(alphabet, 5))
			{
				var expected = Answer(without, input);
				var got      = Answer(cut, input);

				Assert.True(got != "LOOP", $"{name} {(direct ? "direct" : "engine")} \"{input}\": the cut goes round");

				if (expected == "LOOP")
				{
					round++;

					continue;
				}

				Assert.True(
					expected == got,
					$"{name} {(direct ? "direct" : "engine")} \"{input}\": without the check {expected} | cut {got}");

				ended++;
			}
		}

		TestContext.Current.SendDiagnosticMessage($"{name}: {ended} inputs answered alike, {round} where the parser without the check goes round");
	}

	/// <summary>
	/// A cycle cut at once reads long input in time linear in it, valid or not, by methods and in the engine, over a
	/// string, a reader and a stream: where the parser without the check goes round on the invalid input, and had the
	/// entries been counted, would go round about the square of the input with a deep stack.
	/// </summary>
	[Theory]
	[InlineData("T = @Still & T | 'a' & T | eof", "", "b", true)]
	[InlineData("T = @Still & T | 'a' & T | eof", "", "b", false)]
	[InlineData("T = A & eof\nA = @Still & A | 'a' & A | 'b'", "b", "c", true)]
	[InlineData("T = A & eof\nA = @Still & A | 'a' & A | 'b'", "b", "c", false)]
	public void A_cut_at_once_reads_long_input_in_linear_time(string grammar, string end, string wrong, bool direct)
	{
		var span = Build(grammar, direct, unchecked_: false, host: LongHost).Parser;
		var view = Build(grammar, direct, unchecked_: false, buffered: true).Parser;

		var said = new List<string>();

		foreach (var over in new[] { typeof(string), typeof(TextReader), typeof(Stream) })
		{
			var parser = over == typeof(string) ? span : view;

			// The engine's first reading compiles what it reads; time the second.
			Answer(parser, "aa" + end, over);

			foreach (var length in new[] { 4096, 16384, 65536 })
			{
				var valid   = new string('a', length) + end;
				var invalid = new string('a', length) + wrong;
				var watch   = Stopwatch.StartNew();

				Assert.Equal($"ok@{valid.Length}", Answer(parser, valid, over));

				var accepted = watch.Elapsed;

				watch.Restart();

				Assert.StartsWith($"no@{length} ", Answer(parser, invalid, over), StringComparison.Ordinal);

				var refused = watch.Elapsed;

				said.Add($"{over.Name} {length}: {accepted.TotalMilliseconds:F1} / {refused.TotalMilliseconds:F1} ms");

				// Far above what it takes (tens of milliseconds at the most), far below a search.
				Assert.True(accepted.TotalSeconds < 5 && refused.TotalSeconds < 5, $"{over.Name} {length}: {accepted} / {refused}");
			}

			// Short invalid input, where a count of the entries standing at a position would have multiplied.
			for (var length = 1; length <= 12; length++)
				Assert.StartsWith($"no@{length - 1} ", Answer(parser, new string('a', length - 1) + wrong, over), StringComparison.Ordinal);
		}

		TestContext.Current.SendDiagnosticMessage($"{(direct ? "direct" : "engine")}, valid / invalid: " + string.Join("; ", said));
	}

	/// <summary>
	/// A cycle with something read after the call back, or whose reading depends on a value it builds or on the context,
	/// is generated as the parser without the check is, character for character, in every reading form: no check, no
	/// count and nothing thrown; it reads as it always read.
	/// </summary>
	[Theory]
	[InlineData("T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+")]
	[InlineData("T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num | @Ws & Sum & '-' & Num\nNum = ['0'..'9']+")]
	[InlineData("T = A & eof\nA = B | @Ws & A & '+' & B\nB = Num | @Ws & B & '*' & Num\nNum = ['0'..'9']+")]
	[InlineData("T = Z* & eof\nZ = Sum & ';' | '1' | '+'\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+")]
	[InlineData("T = A & 'c'\nA = (@Still & A)? & @Still & A | 'b' & A")]
	[InlineData("T = A & eof\nA = @Still & A | 'a' & A | 'b' | @Still & A & 'z'")]
	[InlineData("T = A & eof\nA = 'a' & A | @Still & A | 'b' | B & 'z'\nB = @Still & A")]
	[InlineData("T = X & '#' & {Sum & '!'} & ';' | 'a'+ & '#' & {Sum & '!'} & eof\nX = 'a'* & 'a'* & 'a'* & 'a'* & 'a'* & 'a'* & 'a'* & 'a'*\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+")]
	[InlineData("T = d: A & when @(d >= 5)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)")]
	[InlineData("T = @Still & n: T & 'b' | 'x'+")]
	[InlineData("T = P('a', 'b')\nP(x, y) = @Still & P(y, x) & x | y")]
	[InlineData("T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'")]
	[InlineData("T = ((@Still & T & 'b' | 'x')* recover ',' & ';')* recover ';' & eof")]
	public void A_counted_cycle_is_written_as_without_the_check(string grammar)
	{
		var graph = ExponentialRepetitionTests.Graph(grammar + "\nparse T", lexical: false, out _);

		Assert.NotEmpty(Reentry.FoundRules(graph));
		Assert.False(Reentry.Any(graph));

		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Deferred })
				foreach (var buffered in new[] { false, true })
				{
					var with    = Generated(grammar, direct, carrier, buffered, unchecked_: false);
					var without = Generated(grammar, direct, carrier, buffered, unchecked_: true);

					Assert.Equal(without.Sources, with.Sources);
					Assert.Contains(with.Told, static one => one.Id == Reentry.Reenters);
				}
	}

	static (IReadOnlyList<string> Sources, IReadOnlyList<GramDiagnostic> Told) Generated(
		string grammar, bool direct, CarrierKind carrier, bool buffered, bool unchecked_)
	{
		var was = Reentry.Unchecked;

		try
		{
			Reentry.Unchecked = unchecked_;

			var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				Carrier        = carrier,
				BufferedInput  = buffered,
				BufferedBytes  = buffered,
				SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

			return ([.. result.Sources.Select(static one => one.Text)], result.Diagnostics);
		}
		finally
		{
			Reentry.Unchecked = was;
		}
	}

	static (Assembly Parser, string Source) Build(string grammar, bool direct, bool unchecked_, bool buffered = false, string? host = null)
	{
		GramCompilation result;

		var was = Reentry.Unchecked;

		Reentry.Unchecked = unchecked_;

		try
		{
			result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				BufferedInput  = buffered,
				BufferedBytes  = buffered,
				SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});
		}
		finally
		{
			Reentry.Unchecked = was;
		}

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		var parser = EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: host ?? (buffered ? ViewHost : Host),
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return (parser, result.Sources[0].Text);
	}

	/// <summary>The verdict, where it ended, and for a refusal its outcome and message; <c>LOOP</c> where the recognizers went past their cap.</summary>
	static string Answer(Assembly parser, string input, Type? over = null)
	{
		over ??= typeof(string);

		var type = parser.GetType("Probe")!;

		type.GetMethod("Reset")!.Invoke(null, null);

		object match;

		try
		{
			match = over == typeof(string)
				? type.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!
				: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)])!.Invoke(null,
				[
					over == typeof(Stream) ? new MemoryStream(Encoding.ASCII.GetBytes(input)) : new StringReader(input),
					null,
					1 << 20,
				])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException?.Message == "went round" || thrown.InnerException is InsufficientExecutionStackException)
		{
			return "LOOP";
		}

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!}"
			: $"no@{Read("Position")} {Read("Outcome")} {Read("Error")}";
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			level = [.. level.SelectMany(prefix => alphabet.Select(c => prefix + c))];

			foreach (var input in level)
				yield return input;
		}
	}
}
