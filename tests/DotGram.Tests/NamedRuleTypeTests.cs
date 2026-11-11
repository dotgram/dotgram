using System;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The type the grammar generates for a rule that captures (§4.1 case 3) is named by the rule: another
/// rule may take it as its own type, <c>Swapped : Pair</c>, and a construction may build one,
/// <c>=&gt; Pair(right, left)</c> (§3.7). The first was declared to give back a string (CS0029 in the
/// generated file) and the second went across as a call of a type (CS1955).
/// </summary>
public sealed class NamedRuleTypeTests
{
	const string Numbers =
		"Number : int = ['0'..'9']+ => @int.Parse(parserText)\n" +
		"Pair = left: Number & ',' & right: Number\n";

	static GramCompilation Compile(string grammar, CarrierKind carrier = CarrierKind.Auto, bool direct = true)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	static Assembly Built(string grammar, CarrierKind carrier = CarrierKind.Auto, bool direct = true)
	{
		var result = Compile(grammar, carrier, direct);

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text);
	}

	static string Shown(object? value)
	{
		if (value is null)
			return "null";

		var type = value.GetType();

		if (type.IsArray)
			return "[" + string.Join(", ", ((Array)value).Cast<object?>().Select(Shown)) + "]";

		var left  = type.GetProperty("Left");
		var right = type.GetProperty("Right");

		return left is null || right is null
			? value.ToString() ?? ""
			: $"{type.Name} {{ Left = {left.GetValue(value)}, Right = {right.GetValue(value)} }}";
	}

	static GramDiagnostic[] Errors(string grammar)
	{
		return [.. Compile(grammar).Diagnostics.Where(static one => one.Severity == GramSeverity.Error)];
	}

	public static TheoryData<CarrierKind, bool> Renderings()
	{
		return new()
		{
			{ CarrierKind.Immediate, true },
			{ CarrierKind.Tape,      true },
			{ CarrierKind.Tape,      false },
		};
	}

	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_construction_builds_a_rules_generated_type_by_its_name(CarrierKind carrier, bool direct)
	{
		var assembly = Built(
			Numbers +
			"Swapped : Pair = left: Number & ';' & right: Number => Pair(right, left)\n" +
			"parse Pair\nparse Swapped", carrier, direct);

		Assert.Equal("Pair { Left = 1, Right = 2 }", Shown(EmittedCode.Match(assembly, "Grammar", "TryParsePair", "1,2").Value));
		Assert.Equal("Pair { Left = 2, Right = 1 }", Shown(EmittedCode.Match(assembly, "Grammar", "TryParseSwapped", "1;2").Value));
	}

	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_rule_typed_by_another_takes_its_generated_type(CarrierKind carrier, bool direct)
	{
		// In C#, as the guide first wrote it; passed through, §4.1 case 3; and filled from the
		// captures by name, as a declared type's constructor is (§7.3).
		var assembly = Built(
			Numbers +
			"Swapped  : Pair   = left: Number & ';' & right: Number => @(new Pair(right, left))\n" +
			"Wrapped  : Pair   = '[' & Pair & ']'\n" +
			"Named    : Pair   = right: Number & '-' & left: Number\n" +
			"Many     : Pair[] = Pair & (';' & Pair)*\n" +
			"parse Swapped\nparse Wrapped\nparse Named\nparse Many", carrier, direct);

		Assert.Equal("Pair { Left = 2, Right = 1 }", Shown(EmittedCode.Match(assembly, "Grammar", "TryParseSwapped", "1;2").Value));
		Assert.Equal("Pair { Left = 3, Right = 4 }", Shown(EmittedCode.Match(assembly, "Grammar", "TryParseWrapped", "[3,4]").Value));
		Assert.Equal("Pair { Left = 6, Right = 5 }", Shown(EmittedCode.Match(assembly, "Grammar", "TryParseNamed", "5-6").Value));
		Assert.Equal(
			"[Pair { Left = 1, Right = 2 }, Pair { Left = 3, Right = 4 }]",
			Shown(EmittedCode.Match(assembly, "Grammar", "TryParseMany", "1,2;3,4").Value));
	}

	[Fact]
	public void A_named_type_whose_name_is_taken_asks_for_the_rule_to_be_renamed()
	{
		// The rule's type would be FailureValue, Failure being a type the class writes: the grammar
		// names it Failure, so the rule is renamed instead of the type.
		var said = Assert.Single(
			Errors("Number : int = ['0'..'9']+ => @int.Parse(parserText)\nFailure = a: Number\nX : Failure = a: Number\nparse X"),
			static one => one.Id == "GRAM5018");

		Assert.Contains("'Failure'", said.Message, StringComparison.Ordinal);
	}
}
