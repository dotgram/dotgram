using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A way back taken after something was read and built: what the parse answers then is what it
/// would answer had it read the alternative it came back for in the first place, on every
/// carrier and with the direct reader on or off.
/// </summary>
public sealed class BacktrackedValueTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	/// <summary>
	/// Literal alternatives read as one run keep no way back between them, which holds only
	/// where nothing after the run can begin where one of them did: <c>"ccc" | 'a'</c> is such a
	/// run, but <c>"cc"</c> after it is read only by coming back from <c>"ccc"</c>. And where a
	/// member continues another, <c>'c' | "ca"</c>, coming back for the longer one is reading it
	/// from past the shorter, where nothing after the run may be read.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_literal_run_keeps_its_way_back_to_a_later_alternative_that_begins_alike(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = ("ccc" | 'a' | "cc") & 'c' => @(parserText)
			S : @string = ("ab" | 'x' | ['a'..'b']) & "bc" => @(parserText)
			T : @string = ('c' | "ca" | ['b']) => @(parserText)
			""", carrier, direct);

		Assert.Equal((true, "ccc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ccc")));
		Assert.Equal((true, "ac"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ac")));
		Assert.Equal((true, "cccc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "cccc")));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "cc").IsSuccess);
		Assert.Equal((true, "abc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abc")));
		Assert.Equal((true, "abbc"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "abbc")));

		// A continuation of a member, "ca" after 'c', that does not fit leaves nothing after the
		// run to be read from where it stands.
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "cb").IsSuccess);
		Assert.Equal((true, "ca"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "ca")));
		Assert.Equal((true, "b"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseT", "b")));
	}

	/// <summary>
	/// A guard that names a rule's value builds it while reading; where the parse comes back into
	/// that rule for another of its alternatives, what was built for the first is not the value
	/// any more — neither for the guard asked again nor for the result.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_value_a_guard_built_is_built_again_once_its_rule_reads_another_way(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = v: Two & when @(v == "2") => @(v)
			Two : @string = 'b' => @("1") | 'b' => @("2")
			S : @string = v: Longer & when @(v != "") => @(v)
			Longer : @string = 'b' => @("1") | "bc" => @("2")
			""", carrier, direct);

		Assert.Equal((true, "2"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseR", "b")));
		Assert.Equal((true, "1"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "b")));
		Assert.Equal((true, "2"), Answer(EmittedCode.Match(assembly, "Grammar", "TryParseS", "bc")));
	}

	/// <summary>
	/// An alternative that can read nothing in front of a literal run, where what comes after it
	/// may begin anything — a look, or a literal of no characters — has nothing after it to turn
	/// it away by, and the engine wrote that as a test of no ranges, which did not compile.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_choice_whose_later_alternatives_may_begin_anything_compiles(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = ('b'? | "b" | "cb" | ?='c') => @(parserText)
			S : @string = ('b'? | "b" | "cb" | "") => @(parserText)
			""", carrier, direct);

		foreach (var rule in new[] { "TryParseR", "TryParseS" })
		{
			Assert.Equal((true, "b"), Answer(EmittedCode.Match(assembly, "Grammar", rule, "b")));
			Assert.Equal((true, "cb"), Answer(EmittedCode.Match(assembly, "Grammar", rule, "cb")));
			Assert.Equal((true, ""), Answer(EmittedCode.Match(assembly, "Grammar", rule, "")));
			Assert.False(EmittedCode.Match(assembly, "Grammar", rule, "c").IsSuccess);
		}
	}

	static (bool, object?) Answer((bool IsSuccess, object? Value, string? Error, long Position) match)
	{
		return (match.IsSuccess, match.Value);
	}

	static System.Reflection.Assembly Compile(string rules, CarrierKind carrier, bool direct)
	{
		var publications = string.Join("\n", rules.Split('\n')
			.Select(static line => line.Trim())
			.Where(static line => line.Length > 0 && char.IsUpper(line[0]) && line.Split(' ')[0] is "R" or "S" or "T")
			.Select(static line => "parse " + line.Split(' ')[0]));

		var result = GramCompiler.Compile(rules + "\n" + publications + "\n", new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
