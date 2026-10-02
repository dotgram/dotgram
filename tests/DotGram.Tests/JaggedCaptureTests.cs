using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A repeated capture of a rule whose value is an array collects an array of arrays, and the
/// array it is collected into is created with its length in the first rank: <c>new T[n][]</c>,
/// not <c>new T[][n]</c>, which is no C# at all (CS1586).
/// </summary>
public sealed class JaggedCaptureTests
{
	const string Helpers = """
		public static string Flat(int[][] groups)
		{
			var text = "";

			foreach (var group in groups)
				text += "[" + string.Join(",", group) + "]";

			return text;
		}

		public static System.Collections.Generic.List<int[]> Listed(int[] items)
		{
			var list = new System.Collections.Generic.List<int[]>();

			foreach (var item in items)
				list.Add(new[] { item, item });

			return list;
		}

		public static string Flat(System.Collections.Generic.List<int[]>[] groups)
		{
			var text = "";

			foreach (var group in groups)
			{
				text += "[";

				foreach (var pair in group)
					text += "(" + string.Join(",", pair) + ")";

				text += "]";
			}

			return text;
		}

		public static string Flat(string[][] groups)
		{
			var text = "";

			foreach (var group in groups)
				text += "[" + string.Join(",", group) + "]";

			return text;
		}
		""";

	/// <summary>
	/// The element is a collection of its own, read off the rule's captures as any collection is,
	/// and the shape Rfc5322's address list is written in: a first value, then the rest.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_repeated_capture_of_a_collection_rule_collects_its_arrays(bool direct)
	{
		var assembly = Compiled("""
			Item  : @int   = 'a' => @(1) | 'b' => @(2)
			Group : @int[] = ';' & Item*
			List  : @string = first: Item & rest: Group* => @(Grammar.Flat(rest) + first)
			parse List

			""", direct);

		Assert.Equal("[1,2][]1", EmittedCode.Match(assembly, "Grammar", "TryParseList", "a;ab;").Value);
		Assert.Equal("2", EmittedCode.Match(assembly, "Grammar", "TryParseList", "b").Value);
	}

	/// <summary>
	/// The same where the element's array is built by a factory over spans of the input, which the
	/// engine compiles in place at the call (a site) rather than as a call.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_repeated_capture_of_a_rule_building_an_array_collects_its_arrays(bool direct)
	{
		var assembly = Compiled("""
			Pair : @string[] = x: ['a'..'z'] & '=' & y: ['a'..'z'] & ';' => @(new[] { x, y })
			List : @string   = pairs: Pair* => @(Grammar.Flat(pairs))
			parse List

			""", direct);

		Assert.Equal("[a,b][c,d]", EmittedCode.Match(assembly, "Grammar", "TryParseList", "a=b;c=d;").Value);
		Assert.Equal("", EmittedCode.Match(assembly, "Grammar", "TryParseList", "").Value);
	}

	/// <summary>A guard reads the arrays it was handed as the construction does.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_guard_over_a_repeated_capture_of_a_collection_rule_reads_its_arrays(bool direct)
	{
		var assembly = Compiled("""
			Item  : @int    = 'a' => @(1) | 'b' => @(2)
			Group : @int[]  = ';' & Item*
			List  : @string = rest: Group* & when @(rest.Length < 3) => @(Grammar.Flat(rest))
			parse List

			""", direct);

		Assert.Equal("[1][2,1]", EmittedCode.Match(assembly, "Grammar", "TryParseList", ";a;ba").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseList", ";;;").IsSuccess);
	}

	/// <summary>
	/// A collection of arrays with no expression written (§4.1 case 2): the sequence its one
	/// repetition hands in, or nothing, and its parts counted into an array of arrays.
	/// </summary>
	[Theory]
	[InlineData("Groups : Group[] = Group*", ";a;ba", "[1][2,1]")]
	[InlineData("Groups : Group[] = Group*", "", "")]
	[InlineData("Groups : Group[] = Group & Group*", ";a;ba", "[1][2,1]")]
	[InlineData("Groups : Group[] = Group & Group*", ";", "[]")]
	public void A_collection_of_arrays_without_a_factory_collects_them(string groups, string input, string expected)
	{
		foreach (var direct in new[] { false, true })
		{
			var assembly = Compiled(
				"Item  : @int   = 'a' => @(1) | 'b' => @(2)\n" +
				"Group : @int[] = ';' & Item*\n" +
				groups + "\n" +
				"parse Groups\n",
				direct);

			var value = (int[][])EmittedCode.Match(assembly, "Grammar", "TryParseGroups", input).Value!;

			Assert.Equal(expected, string.Concat(value.Select(static group => "[" + string.Join(",", group) + "]")));
		}
	}

	/// <summary>
	/// An element type with an array inside a type argument keeps it there: the length goes before
	/// the ranks the type ends with, not before the first bracket. Read by the engine and by the
	/// direct reader on either carrier, a factory and a guard handed the array.
	/// </summary>
	[Theory]
	[InlineData(false, CarrierKind.Auto)]
	[InlineData(true, CarrierKind.Tape)]
	[InlineData(true, CarrierKind.Immediate)]
	public void An_array_inside_a_type_argument_stays_there(bool direct, CarrierKind carrier)
	{
		var (assembly, source) = Built("""
			Item  : @int = 'a' => @(1) | 'b' => @(2)
			Group : @System.Collections.Generic.List<int[]> = ';' & v: Item* => @(Grammar.Listed(v))
			List  : @string = rest: Group* & when @(rest.Length < 3) => @(Grammar.Flat(rest))
			parse List

			""", direct, carrier);

		Assert.Contains("new System.Collections.Generic.List<int[]>[", source, StringComparison.Ordinal);
		Assert.Equal("[(1,1)][(2,2)(1,1)]", EmittedCode.Match(assembly, "Grammar", "TryParseList", ";a;ba").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseList", ";;;").IsSuccess);
	}

	static Assembly Compiled(string grammar, bool direct)
	{
		return Built(grammar, direct, CarrierKind.Auto).Assembly;
	}

	static (Assembly Assembly, string Source) Built(string grammar, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Direct        = direct,
				Carrier       = carrier,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var source = result.Sources[0].Text;

		return (EmittedCode.Compile(source, declarationMembers: Helpers), source);
	}
}
