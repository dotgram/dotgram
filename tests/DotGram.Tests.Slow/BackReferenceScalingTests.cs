using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the "until" scan over a binding costs, <c>(?!$q &amp; any)*</c> and its delimited forms,
/// held to the classes docs/syntax.md §3.5.1 gives it: the length of the text times the length of
/// the binding where a crafted text makes every position a near miss, and linear where the
/// binding is delimited, so that a near miss fails at its first differing item.
/// </summary>
/// <remarks>
/// <para>
/// Every row grows the binding with the text — the opening is as long as a block of the body —
/// because that is the shape which tells the two classes apart: with the binding fixed, the text
/// times the binding is linear in the text too. Four times the text is then four times the work
/// where the scan is linear and sixteen times where it is the product, an exponent of 1 against 2.
/// </para>
/// <para>
/// The bounds are exponents, 1.6 for a linear row and 2.5 for the product, each half-way to the
/// next class up. That is tighter than the 2.0 the reader's own rows keep for a linear shape,
/// because there the cost of a wrong row is a superlinear reading nobody would see, and here a
/// linear row reading 2 is exactly the defect the row exists for. A control row, linear by
/// construction, gates the class as in <c>ReaderScalingTests</c>: where it does not read linear,
/// the machine is noisy and every row is skipped as inconclusive rather than failed.
/// </para>
/// <para>
/// Skipped until the reader generates code for bindings (<see cref="BackReferenceEmission"/>).
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class BackReferenceScalingTests
{
	const double Linear = 1.6;

	const double Product = 2.5;

	/// <summary>How many blocks of near misses each body holds.</summary>
	const int Blocks = 8;

	const string ControlWhat = "a raw string whose body holds no quote";

	const string RawString = "RawString = { $q: '\"'{3,} } & { (?!$q & any)* } & $q & ?!'\"'\nparse RawString";

	[Theory]
	[InlineData(ControlWhat, Linear)]
	[InlineData("a Lua long bracket whose body holds closings one '=' short", Linear)]
	[InlineData("a PostgreSQL dollar quote whose body holds tags one letter short", Linear)]
	[InlineData("a heredoc whose body holds lines one letter short of the tag", Linear)]
	[InlineData("a raw string whose body holds runs one quote short of the opening", Product)]
	public void The_until_scan_costs_what_the_section_says(string what, double bound)
	{
		BackReferenceEmission.RequireReader();

		var control = Control.Value;

		if (!control.Within)
		{
			Assert.Skip(
				$"The control row (\"{ControlWhat}\") read an exponent of {control.Exponent(4.0):F2} against its " +
				$"bound of {Linear:F2} on every one of {ScalingClock.Attempts} measurements ({control.Readings}); the machine " +
				"is noisy and none of this class's rows can be trusted this run.");

			return;
		}

		if (what == ControlWhat)
			return;

		var (grammar, rule, text) = Shape(what);
		var parser                = Compiled(grammar, rule);
		var shorter               = text(250);
		var longer                = text(1000);

		Parse(parser, rule, shorter);
		Parse(parser, rule, longer);

		var reading = ScalingClock.Settle(() => Parse(parser, rule, shorter), () => Parse(parser, rule, longer), Math.Pow(4.0, bound));

		Assert.True(
			reading.Within,
			$"With {what}, four times the text took {reading.Ratio:F1} times as long " +
			$"({reading.Shorter:F0} µs against {reading.Longer:F0} µs), an exponent of {reading.Exponent(4.0):F2} " +
			$"against {bound:F2}, on every one of {ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	/// <summary>The grammar, the rule it publishes and the text at a size, for each row.</summary>
	static (string Grammar, string Rule, Func<int, string> Text) Shape(string what)
	{
		switch (what)
		{
			case ControlWhat:
				return (RawString, "RawString", static size =>
					Quotes(size) + new string('x', Blocks * size) + Quotes(size));

			case "a Lua long bracket whose body holds closings one '=' short":
				return (
					"LongString = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'\nparse LongString",
					"LongString",
					static size =>
						"[" + new string('=', size) + "[" +
						Repeated("]" + new string('=', size - 1) + "]") +
						"]" + new string('=', size) + "]");

			case "a PostgreSQL dollar quote whose body holds tags one letter short":
				return (
					"DollarQuoted = '$' & $tag: (Tag?) & '$' & { (?!('$' & $tag & '$') & any)* } & '$' & $tag & '$'\n" +
					"Tag          = [\\p{L} | '_'] & [\\p{L} | \\p{Nd} | '_']*\n" +
					"parse DollarQuoted",
					"DollarQuoted",
					static size =>
						"$" + new string('a', size) + "$" +
						Repeated("$" + new string('a', size - 1) + "$") +
						"$" + new string('a', size) + "$");

			case "a heredoc whose body holds lines one letter short of the tag":
				return (
					"Heredoc = \"<<\" & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine\n" +
					"Word    = ['A'..'Z' | 'a'..'z' | '_']+\n" +
					"Line    = [^ '\\r' | '\\n']+ & EndLine | eol\n" +
					"EndLine = eol | eof\n" +
					"parse Heredoc",
					"Heredoc",
					static size =>
						"<<" + new string('E', size) + "\n" +
						Repeated(new string('E', size - 1) + "\n") +
						new string('E', size));

			case "a raw string whose body holds runs one quote short of the opening":
				// Each run stands after an 'x', so that the opening ends where its own quotes do.
				return (RawString, "RawString", static size =>
					Quotes(size) + Repeated("x" + Quotes(size - 1)) + "x" + Quotes(size));

			default:
				throw new ArgumentOutOfRangeException(nameof(what), what, null);
		}
	}

	/// <summary>The control's reading, measured once for the process and shared by every row.</summary>
	static readonly Lazy<ScalingClock.Reading> Control = new(
		static () =>
		{
			var (grammar, rule, text) = Shape(ControlWhat);
			var parser                = Compiled(grammar, rule);
			var shorter               = text(250);
			var longer                = text(1000);

			Parse(parser, rule, shorter);
			Parse(parser, rule, longer);

			return ScalingClock.Settle(() => Parse(parser, rule, shorter), () => Parse(parser, rule, longer), Math.Pow(4.0, Linear));
		});

	static string Quotes(int count)
	{
		return new string('"', count);
	}

	static string Repeated(string block)
	{
		return string.Concat(Enumerable.Repeat(block, Blocks));
	}

	static MethodInfo Compiled(string grammar, string rule)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = true,
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text)
			.GetType("Grammar")!
			.GetMethod("TryParse" + rule, [typeof(string)])!;
	}

	/// <summary>Reads the text, which every row's text is built to be.</summary>
	static void Parse(MethodInfo parser, string rule, string text)
	{
		var match = parser.Invoke(null, [text])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!, $"{rule} refused its own text.");
	}
}
