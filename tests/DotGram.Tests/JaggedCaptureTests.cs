using System;
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

	static Assembly Compiled(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Direct        = direct,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: Helpers);
	}
}
