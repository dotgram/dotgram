using System;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A text capture a guard and the rule's construction both read is cut once: the construction
/// is handed the string the guard was handed, where it stands on the same positions, and cuts
/// its own where it does not (the immediate carrier, <c>ImmediateCarrier.PutText</c>).
/// </summary>
/// <remarks>
/// Which string arrives is seen by reference: the guard keeps what it was handed, and the
/// factory says whether it was handed the same one — <c>=</c> where it was, <c>!</c> where it
/// was cut again.
/// </remarks>
public sealed class GuardTextReuseTests
{
	const string Members = """
		static string seenFirst = "", seenSecond = "";
		static bool Seen(string first, string second) { seenFirst = first; seenSecond = second; return true; }
		static bool Seen(string first) { seenFirst = first; seenSecond = ""; return true; }
		static string Same(string first, string second) =>
			first + "," + second + (ReferenceEquals(first, seenFirst) && ReferenceEquals(second, seenSecond) ? "=" : "!");
		static string Same(string first) => first + (ReferenceEquals(first, seenFirst) ? "=" : "!");
		""";

	/// <summary>RFC 3339's shape: two fields cut for the guard, and the construction takes both.</summary>
	[Fact]
	public void The_construction_takes_what_the_guard_was_handed()
	{
		Assert.Equal("12,03=", Parse(
			"""
			Digit = ['0'..'9']
			Two   = Digit{2}
			Date : @string = y: Two & '-' & m: Two & when @(Seen(y!, m!)) => @(Same(y, m))
			parse Date
			""", "12-03"));
	}

	/// <summary>
	/// A capture over a repeated rule (<c>y: D{2}</c>): the guard is handed the whole field, not
	/// its last turn, and the construction takes the same string.
	/// </summary>
	[Fact]
	public void A_capture_over_a_repeated_rule_is_whole_for_the_guard_and_the_construction()
	{
		Assert.Equal("12=", Parse(
			"""
			D    = ['0'..'9']
			Date : @string = y: D{2} & when @(Seen(y!)) => @(Same(y))
			parse Date
			""", "12"));
	}

	/// <summary>
	/// The capture written again after the guard: the first alternative's guard cut <c>a</c> and
	/// the alternative failed after it, the second captured <c>ab</c>, and the construction cuts
	/// its own. Where the guard's alternative stood it cuts its own as well: the alternatives
	/// are methods of their own, and what a guard keeps is seen only in the method it is in.
	/// </summary>
	[Theory]
	[InlineData("abr", "ab!")]
	[InlineData("aq", "a!")]
	public void A_capture_written_again_after_the_guard_is_cut_again(string input, string expected)
	{
		Assert.Equal(expected, Parse(
			"""
			Pick : @string = (x: ['a'..'z'] & when @(Seen(x!)) & 'q' | x: (['a'..'z'] & ['a'..'z']) & 'r') => @(Same(x))
			parse Pick
			""", input));
	}

	static string? Parse(string grammar, string input)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Immediate, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;
		var host   = EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;
		var rule   = grammar.Substring(grammar.IndexOf("parse ", StringComparison.Ordinal) + 6).Trim();
		var match  = host.GetMethod("TryParse" + rule, [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? (string?)match.GetType().GetProperty("Value")!.GetValue(match)
			: "<refused " + match.GetType().GetProperty("Error")!.GetValue(match) + ">";
	}
}
