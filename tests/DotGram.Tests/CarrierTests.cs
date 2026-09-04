using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every carrier a grammar can be compiled with answers the same on every input — the same
/// verdict, and the same value where there is one.
/// </summary>
/// <remarks>
/// <para>
/// The carriers differ in when the author's constructions run and where the pieces of a
/// value wait; they may not differ in what comes out. This is the agreement test the
/// hand-written readings in <c>benchmarks/DotGram.HandDeferred</c> ran on themselves,
/// asked of the generator: the tape is the reference, because it is the one that keeps
/// §7.3, and everything else has to agree with it or it does not ship.
/// </para>
/// <para>
/// The grammars are the shapes the second carrier had to learn one at a time: a fold, a
/// cycle, members gathered across turns as records and as text, a guard that reads a
/// captured value, and a member that may be missing.
/// </para>
/// </remarks>
public sealed class CarrierTests
{
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes =
	[
		("a fold",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", ""]),

		("a cycle",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"               | '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + (b = 2 + c = 3)", "((a = 1))", "(a = 1", "a = 1 + (b = 2"]),

		("records gathered",
			"Start : @string = first: Name & (',' & rest: Name)* => @(first + \"|\" + string.Join(\"|\", rest))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a,b,c", "a", "a,", ",a"]),

		("text gathered",
			"Start : @string = (parts: ['a'..'z']+ & ','?)+ => @(parts)\n" +
			"parse Start\n",
			["ab,cd,e", "ab", "ab,,cd", ""]),

		("a guard over a record",
			"Start : @string = d: Digits & when @(d.Length < 3) => @(\"<\" + d + \">\")\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["12", "1234", "", "x"]),

		("a member that may be missing",
			"Start : @string = a: Name & b: Digits? => @(a + (b ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "ab", "12", ""]),
	];

	public static IEnumerable<object[]> Every() => Shapes.Select(one => new object[] { one.Name });

	/// <summary>The eager carrier agrees with the tape on every shape and every input.</summary>
	[Theory]
	[MemberData(nameof(Every))]
	public void Eager_agrees_with_the_tape(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape  = Compiled(grammar, CarrierKind.Tape);
		var eager = Compiled(grammar, CarrierKind.Eager);

		Assert.Contains("EagerValues", eager.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", eager.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", eager.Source, StringComparison.Ordinal);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(eager.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, eager says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	static string? ValueOf(object match) =>
		match.GetType().GetProperty("Value")?.GetValue(match)?.ToString();

	static (string Source, Assembly Assembly) Compiled(string grammar, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
		});

		Assert.Empty(result.Diagnostics.Where(one => one.Severity == GramSeverity.Error));

		var source = Assert.Single(result.Sources).Text;

		return (source, EmittedCode.Compile(source, "Probe", "Carried"));
	}
}
