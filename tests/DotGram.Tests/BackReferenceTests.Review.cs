using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a binding leaves of a capture over the same span, once a greedy binding has given items
/// back; and what a search over a long text with bindings finds.
/// </summary>
public sealed partial class BackReferenceTests
{
	public static TheoryData<CarrierKind, bool> EveryCarrierAndBackEnd()
	{
		var data = new TheoryData<CarrierKind, bool>();

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
				data.Add(carrier, direct);

		return data;
	}

	/// <summary>
	/// The capture over a binding holds the text the binding holds when the reference after it
	/// matches, not the longer text it held before giving items back.
	/// </summary>
	[Theory]
	[MemberData(nameof(EveryCarrierAndBackEnd))]
	public void A_capture_over_a_binding_that_gave_items_back_holds_what_it_kept(CarrierKind carrier, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(
			"""
			R : @string = q: ($q: W) & r: $q => @(q + "/" + r)
			W           = ['a'..'z']+
			parse R
			""",
			carrier,
			direct);

		Assert.Equal("ab/ab", EmittedCode.Match(assembly, "Grammar", "TryParseR", "abab").Value);
		Assert.Equal("abc/abc", EmittedCode.Match(assembly, "Grammar", "TryParseR", "abcabc").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "aba").IsSuccess);
	}

	/// <summary>
	/// A capture over a binding that gives a turn back holds what it holds without the binding
	/// (a parenthesised run of a valued rule is its text), shortened by the turn given back.
	/// </summary>
	[Theory]
	[MemberData(nameof(EveryCarrierAndBackEnd))]
	public void Values_built_inside_a_binding_that_gave_a_turn_back_are_not_kept(CarrierKind carrier, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(
			"""
			R : @string = c: ($q: Item+) & l: Item & ';' & $q => @(string.Join(",", c) + "|" + l)
			Item : @int = ['0'..'9'] => @(parserText[0] - '0')
			parse R
			""",
			carrier,
			direct);

		Assert.Equal("12|3", EmittedCode.Match(assembly, "Grammar", "TryParseR", "123;12").Value);
		Assert.Equal("1|2", EmittedCode.Match(assembly, "Grammar", "TryParseR", "12;1").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "123;123").IsSuccess);
	}

	/// <summary>
	/// A rule that binds, published as it is and under a namespace rebinding: two copies of the
	/// rule, each reading its own binding.
	/// </summary>
	[Theory]
	[MemberData(nameof(EveryCarrierAndBackEnd))]
	public void A_rule_that_binds_published_under_a_namespace_rebinding_binds_in_each_copy(CarrierKind carrier, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(
			"""
			B = 'b'
			A = $q: B+ & ':' & $q

			parse A as DefaultA

			D = 'd'

			namespace Ns with (B = D)
			{
				parse A as NamespaceA
			}
			""",
			carrier,
			direct);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "bb:bb").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "bb:b").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryDefaultA", "dd:dd").IsSuccess);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "dd:dd").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "dd:ddd").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNamespaceA", "bb:bb").IsSuccess);
	}

	/// <summary>
	/// A search over a long text full of raw strings and elements finds as many occurrences as the
	/// text holds, four times as many in four times the text. How the time grows with the text is
	/// held in Tests.Slow (<c>BackReferenceSearchScalingTests</c>).
	/// </summary>
	[Theory]
	[InlineData("RawString = { $q: '\"'{3,} } & { (?!$q & any)* } & $q & ?!'\"'", "RawString", "x = \"\"\"ab\"c\"\"\"; y = \"\"\"\"d\"\"\"e\"\"\"\"; ", true)]
	[InlineData("RawString = { $q: '\"'{3,} } & { (?!$q & any)* } & $q & ?!'\"'", "RawString", "x = \"\"\"ab\"c\"\"\"; y = \"\"\"\"d\"\"\"e\"\"\"\"; ", false)]
	[InlineData("Element = '<' & $n: Name & '>' & (Element | Text)* & \"</\" & $n & '>'\nName = ['a'..'z']+\nText = { [^ '<']+ }", "Element", "t <ab>x<c>y</c></ab> <d></e> ", true)]
	[InlineData("Element = '<' & $n: Name & '>' & (Element | Text)* & \"</\" & $n & '>'\nName = ['a'..'z']+\nText = { [^ '<']+ }", "Element", "t <ab>x<c>y</c></ab> <d></e> ", false)]
	public void A_search_over_a_long_text_finds_what_the_text_holds(string grammar, string rule, string block, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(grammar + $"\nfind {rule}\n", CarrierKind.Auto, direct);
		var shorter  = string.Concat(Enumerable.Repeat(block, 25_000 / block.Length));
		var longer   = string.Concat(Enumerable.Repeat(block, 100_000 / block.Length));

		var few  = EmittedCode.FoundAt(assembly, "Grammar", "Find" + rule, shorter).Length;
		var many = EmittedCode.FoundAt(assembly, "Grammar", "Find" + rule, longer).Length;

		Assert.True(few > 0);
		Assert.Equal(longer.Length / block.Length * few / (shorter.Length / block.Length), many);
	}
}
