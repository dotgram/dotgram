using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What may stand after <c>=&gt;</c>: a name, a call, a literal, or C# after <c>@</c>. A group, a
/// sequence, a choice or a repetition is what a rule reads, not a value, and used to go across to the
/// generated file as the compiler's own spelling of the tree (CS0246 on <c>Sequence</c>, CS0708 on
/// <c>Reference</c>) with no diagnostic. Each is now refused at its place in the grammar.
/// </summary>
public sealed class ArrowGroupTests
{
	const string Prefix = "A : @int = ['0'..'9']+ => @int.Parse(parserText)\nB : @int = ['a'..'z']+ => @(parserText.Length)\n";

	/// <summary>A body that is a value: the parser compiles and builds what the body says.</summary>
	[Theory]
	[InlineData("l", "12x", 12)]
	[InlineData("@(l)", "12x", 12)]
	[InlineData("@(l + r)", "12xyz", 15)]
	[InlineData("@(r - (l))", "12xyz", -9)]
	[InlineData("@System.Math.Max(l, r)", "12xyz", 12)]
	[InlineData("@int.Parse(\"7\")", "12x", 7)]
	public void A_value_after_the_arrow_builds_what_it_says(string body, string input, int expected)
	{
		var result = Compile(Prefix + "R : @int = l: A & r: B => " + body + "\nparse R");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var parser = EmittedCode.Compile(result.Sources[0].Text, declarationMembers: "", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var (success, value, _, _) = EmittedCode.Match(parser, "Grammar", "TryParseR", input);

		Assert.True(success);
		Assert.Equal(expected, value);
	}

	/// <summary>
	/// A group, in any of the shapes a grammar gives one, is GRAM4065 at the group, and the generator
	/// writes nothing it would not compile.
	/// </summary>
	[Theory]
	[InlineData("(l)")]
	[InlineData("((l))")]
	[InlineData("(l & r)")]
	[InlineData("(r & l)")]
	[InlineData("((l & r))")]
	[InlineData("(l & (r))")]
	[InlineData("(l | r)")]
	[InlineData("(l | 'x')")]
	[InlineData("(l?)")]
	[InlineData("(l*)")]
	[InlineData("(l+)")]
	[InlineData("('x')")]
	[InlineData("(l & 'x')")]
	[InlineData("({ l })")]
	[InlineData("(?=l)")]
	[InlineData("(?!l)")]
	[InlineData("(l{2})")]
	[InlineData("Pair((l & r), r)")]
	[InlineData("@System.Math.Max((l), r)")]
	public void A_group_after_the_arrow_is_refused(string body)
	{
		var result = Compile(Prefix + "R : @int = l: A & r: B => " + body + "\nparse R");

		var refusal = Assert.Single(result.Diagnostics, one => one.Id == "GRAM4065");

		Assert.Equal(GramSeverity.Error, refusal.Severity);
		Assert.Contains("@(", refusal.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// What is not a group at all is refused as it always was, and none of it reaches the generated
	/// file either: the body is read as the start of what follows the alternative.
	/// </summary>
	[Theory]
	[InlineData("(l + r)")]
	[InlineData("(l, r)")]
	[InlineData("(l ?? r)")]
	[InlineData("()")]
	[InlineData("(!l)")]
	[InlineData("(l) & r")]
	[InlineData("l & r")]
	[InlineData("(l & r)?")]
	[InlineData("(l & r)*")]
	[InlineData("(l & r) | l")]
	public void Another_body_is_refused_with_a_diagnostic(string body)
	{
		var result = Compile(Prefix + "R : @int = l: A & r: B => " + body + "\nparse R");

		Assert.Contains(result.Diagnostics, one => one.Severity == GramSeverity.Error);
	}

	/// <summary>The same places that take a value on <c>=&gt;</c>: a guard, a mark and a recovery's construction.</summary>
	[Theory]
	[InlineData("R : @int = l: A & when (l) => l\nparse R")]
	[InlineData("R : @int = l: A & when @F((l)) => l\nparse R")]
	[InlineData("R : @int = l: A & when (l & l) => l\nparse R")]
	[InlineData("R : @int[] = l: A* recover ['x'] => (l)\nparse R")]
	[InlineData("R : @int[] = l: A* recover ['x'] => (l & l)\nparse R")]
	public void A_group_in_another_value_place_is_refused(string grammar)
	{
		var result = Compile(Prefix + grammar);

		Assert.Contains(result.Diagnostics, one => one.Severity == GramSeverity.Error);
	}

	/// <summary>A group at the bottom of calls nested as deep as the parser reads is found.</summary>
	[Fact]
	public void A_group_at_the_bottom_of_nested_calls_is_refused()
	{
		const int Depth = 50;

		var result = Compile(Prefix + "R : @int = l: A => " + string.Concat(Enumerable.Repeat("@F(", Depth)) + "(l)" + new string(')', Depth) + "\nparse R");

		Assert.Contains(result.Diagnostics, one => one.Id == "GRAM4065");
	}

	/// <summary>A group that constructs, and one that only groups, keep their meaning.</summary>
	[Fact]
	public void A_constructing_group_still_builds()
	{
		var result = Compile(Prefix + "R : @int = (l: A & r: B => @(l + r)) | l: A => l\nparse R");

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var parser = EmittedCode.Compile(result.Sources[0].Text, declarationMembers: "", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var (success, value, _, _) = EmittedCode.Match(parser, "Grammar", "TryParseR", "12xyz");

		Assert.True(success);
		Assert.Equal(15, value);
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}
}
