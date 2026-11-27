using System;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule that hands back a value another rule made — <c>=&gt; @(e)</c> — locates nothing, whether the
/// locations are compiled in or asked for per call: the value keeps the range of the rule that built it,
/// and is told it once (docs/syntax.md §6.8). A construction that makes a value, a record copied with
/// <c>with</c> among them, is told the range of its own rule.
/// </summary>
/// <remarks>
/// A reading that compiled its locations in told a handed-back value the range of every rule it came
/// out of, the outermost last, so the brackets around an expression were inside the expression's span,
/// where a per-call reading of the same grammar left them out.
/// </remarks>
public sealed class PassThroughLocationTests
{
	static string Source(string carrier, bool lexical, bool perCall)
	{
		var located = perCall ? "" : ", LocationType = typeof(ILoc)";
		var variant = perCall ? "[GramParserVariant(\"Located\", LocationType = typeof(ILoc), UsePerCallLocations = true)]" : "";

		return $$""""
			using DotGram;

			namespace PassThroughProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);
				}

				public sealed record Node(string Text) : ILoc
				{
					public int At = -1;

					public int Length = -1;

					public int Offers;

					public void Locate(int at, int length)
					{
						At     = at;
						Length = length;
						Offers++;
					}
				}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					Expr : @Node = '(' & e: Expr & ')' => @(e)
						| '[' & e: Expr & ']' => @(e with { })
						| w: Identifier       => @(new Node(w))

					parse Expr
					""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}}{{located}})]
				{{variant}}
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData("Deferred", true, false)]
	[InlineData("Deferred", false, false)]
	[InlineData("Immediate", true, false)]
	[InlineData("Immediate", false, false)]
	[InlineData("Deferred", true, true)]
	[InlineData("Deferred", false, true)]
	[InlineData("Immediate", true, true)]
	[InlineData("Immediate", false, true)]
	public void A_value_handed_back_keeps_the_range_of_the_rule_that_made_it(string carrier, bool lexical, bool perCall)
	{
		var assembly = GeneratorDriverTests.Build(Source(carrier, lexical, perCall), permittedWarning: "GRAM5007");
		var host     = assembly.GetType("PassThroughProbe.P")!;
		var door     = perCall ? host.GetNestedType("Located")! : host;

		// Handed back through two pairs of brackets, the name is told its own range, once.
		Assert.Equal((4, 2, 1), Location(Parse(door, "( ( ab ) )")));

		// A copy is a value made by the rule that copied it, and is told that rule's range: the
		// brackets are inside it. The copy keeps its original's offer, so it counts two.
		Assert.Equal((0, 6, 2), Location(Parse(door, "[ ab ]")));
		Assert.Equal((0, 10, 2), Location(Parse(door, "[ ( ab ) ]")));
		Assert.Equal((2, 6, 2), Location(Parse(door, "( [ ab ] )")));
	}

	static object Parse(Type door, string text)
	{
		return door.GetMethod("ParseExpr", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!.Invoke(null, [text])!;
	}

	static (int, int, int) Location(object node)
	{
		return (Field(node, "At"), Field(node, "Length"), Field(node, "Offers"));
	}

	static int Field(object node, string name)
	{
		return (int)node.GetType().GetField(name)!.GetValue(node)!;
	}
}
