using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a capture holds where docs/syntax.md says one thing and every carrier another. Each test
/// states the documented value; it is skipped until the generator and the document agree.
/// </summary>
public sealed class CapturedValueGapTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	/// <summary>
	/// §7.3: a run that matched nothing is <c>""</c> and only an optional capture that never ran is
	/// null. A bounded run that may take no turn, <c>c: 'a'{0,2}</c>, is <c>""</c> in a rule of one
	/// alternative and null in a rule of several, where the capture is absent from the others.
	/// </summary>
	[Theory(Skip = "Open: a text capture of a bounded run that took no turn reads null where its rule has another alternative.")]
	[MemberData(nameof(Readings))]
	public void A_bounded_run_that_took_no_turn_is_empty_text_in_a_rule_of_several_alternatives(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			One : @string = c: 'a'{0,2} => @(c ?? "null")
			Two : @string = c: 'a'{0,2} => @(c ?? "null") | 'c' => @("c")
			parse One
			parse Two
			""", carrier, direct);

		Assert.Equal("", EmittedCode.Match(assembly, "Grammar", "TryParseOne", "").Value);
		Assert.Equal("", EmittedCode.Match(assembly, "Grammar", "TryParseTwo", "").Value);
	}

	/// <summary>
	/// §3.4: <c>?=X</c> produces <c>X</c>'s value, which can be captured. Captured, a lookahead at a
	/// rule that builds a value is handed the text the lookahead saw instead — and where the rule's
	/// type is not <c>string</c>, the example of §3.6 (<c>n: ?=Number &amp; when @IsSmall(n)</c>)
	/// does not compile.
	/// </summary>
	[Theory(Skip = "Open: a captured lookahead at a valued rule is handed the text it saw, not the rule's value.")]
	[MemberData(nameof(Readings))]
	public void A_captured_lookahead_at_a_valued_rule_holds_the_rules_value(CarrierKind carrier, bool direct)
	{
		var assembly = Compile(
			"""
			R : @string = v: ?=W & W => @(v)
			W : @string = 'b' => @("built")
			parse R
			""", carrier, direct);

		Assert.Equal("built", EmittedCode.Match(assembly, "Grammar", "TryParseR", "b").Value);
	}

	static System.Reflection.Assembly Compile(string grammar, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
