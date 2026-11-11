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

	/// <summary>The grammar with its last rule published, which is the rule under test.</summary>
	static string Published(string grammar)
	{
		var last = grammar.Substring(grammar.LastIndexOf('\n') + 1);

		return grammar + "\nparse " + last.Substring(0, last.IndexOfAny([' ', ':'])).Trim();
	}

	public static TheoryData<CarrierKind, bool> Renderings()
	{
		return new()
		{
			{ CarrierKind.Immediate, true },
			{ CarrierKind.Deferred,      true },
			{ CarrierKind.Deferred,      false },
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

	[Theory]
	[InlineData(Numbers + "Swapped : Pair = left: Number & ';' & right: Number => Pair(right)",           "Pair(right)")]
	[InlineData(Numbers + "Swapped : Pair = left: Number & ';' & right: Number => Pair(right, left, left)", "Pair(right, left, left)")]
	[InlineData(Numbers + "Mixed : Pair = t: ['a'..'z']+ & ';' & n: Number => Pair(t, n)",                 "t")]
	[InlineData(Numbers + "Other = a: Number & '-' & b: Number\nSwapped : Pair = o: Other => Pair(o, 1)",   "o")]
	[InlineData(Numbers + "Plain = 'p'\nX : @string = a: Number => Plain(a)",                              "Plain(a)")]
	[InlineData(Numbers + "Made : @string = 'm' => @(\"m\")\nX : @string = a: Number => Made(a)",            "Made(a)")]
	[InlineData("Item : @string = 'a' => @(\"A\")\nItems : @string[] = items: { Item }* => @(items)",           "(items)")]
	[InlineData("Item : @string = 'a' => @(\"A\")\nOne : @string = items: Item+ => items",                   "items")]
	[InlineData(Numbers + "One : @string = p: Pair => @(p)",                                                "(p)")]
	[InlineData(Numbers + "One : int = p: Pair => p",                                                       "p")]
	public void A_value_that_cannot_be_its_type_is_said_on_the_grammar(string grammar, string at)
	{
		var full = Published(grammar);
		var said = Assert.Single(Errors(full), static one => one.Id == GrammarNormalizer.ConstructionTypeMismatch);

		Assert.StartsWith(at, full.Substring(said.Position), StringComparison.Ordinal);
		Assert.DoesNotContain(Errors(full), static one => one.Id != GrammarNormalizer.ConstructionTypeMismatch);
	}

	[Theory]
	[InlineData("Item : @string = 'a' => @(\"A\")\nItems : @string[] = items: Item* => @(items)")]
	[InlineData("Item : @string = 'a' => @(\"A\")\nItems : @object = items: { Item }* => @(items)")]
	[InlineData("Item : @string = 'a' => @(\"A\")\nItems : @System.Collections.Generic.IEnumerable<string> = items: Item* => @(items)")]
	[InlineData(Numbers + "One : @object = p: Pair => @(p)")]
	[InlineData(Numbers + "One : Pair = p: Pair => @(p)")]
	[InlineData(Numbers + "One : int = p: Pair => @(p.Left)")]
	public void While_one_that_can_is_left_to_compile(string grammar)
	{
		Assert.Empty(Errors(Published(grammar)));
	}

	/// <summary>
	/// A type of the host's may declare a conversion from what is handed back, on its own side: the
	/// grammar cannot see one, and leaves the question to the C# compiler, which answers yes.
	/// </summary>
	[Theory]
	[InlineData(
		"Pair = x: 'a'\nWrapped : @Box = p: Pair => p\nparse Wrapped",
		"public sealed class Box { public string Held = \"\"; public static implicit operator Box(Pair p) { return new Box { Held = p.X }; } }",
		"TryParseWrapped", "a", "Held", "a")]
	[InlineData(
		"Item : @string = 'a' => @(\"A\")\nItems : @String = items: Item+ => items\nparse Items",
		"public sealed class String { public string Held = \"\"; public static implicit operator String(string[] items) { return new String { Held = string.Join(\",\", items) }; } }",
		"TryParseItems", "aa", "Held", "A,A")]
	[InlineData(
		"Unused = x: 'a'\nA : @Unused = x: 'a' => x\nparse A",
		"public sealed class Unused { public string Held = \"\"; public static implicit operator Unused(string s) { return new Unused { Held = s }; } }",
		"TryParseA", "a", "Held", "a")]
	public void A_conversion_the_host_declares_is_left_to_the_csharp_compiler(
		string grammar, string members, string method, string input, string field, string expected)
	{
		// A rule nothing reaches, Unused, is pruned and its type never written: the name is the host's.
		var result = Compile(grammar);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: members);
		var value    = EmittedCode.Match(assembly, "Grammar", method, input).Value!;

		Assert.Equal(expected, value.GetType().GetField(field)!.GetValue(value));
	}

	/// <summary>A construction in a call's argument, built where the argument is read, as it always was.</summary>
	[Fact]
	public void A_construction_in_an_argument_still_builds_the_arguments_value()
	{
		var assembly = Built("A : int = v: L('a' => @(1)) => v\nL(x) : int = x\nparse A");

		Assert.Equal(1, EmittedCode.Match(assembly, "Grammar", "TryParseA", "a").Value);
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
