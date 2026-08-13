using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The same language written twice, and every short input asked of both.
/// </summary>
/// <remarks>
/// <para>
/// The project's claim is that regex-shaped notation has regex semantics
/// (docs/syntax.md, opening). Over the subset where the two notations mean the same
/// thing, that claim is checkable rather than aspirational: compile the grammar,
/// compile the equivalent anchored pattern, and run both over every string up to a
/// given length. A disagreement on any of them is a defect in one of the two, and it
/// is not going to be in <c>System.Text.RegularExpressions</c>.
/// </para>
/// <para>
/// This is the test that would have found the backtracking defect in seconds, and it
/// exists because it did not.
/// </para>
/// </remarks>
public sealed class RegexDifferentialTests
{
	/// <summary>Every string over this alphabet up to <see cref="Length"/> characters.</summary>
	const string Alphabet = "abc";
	const int    Length   = 5;

	[Theory]
	// Sequence and choice.
	[InlineData("'a' & 'b'",                        "ab")]
	[InlineData("'a' | 'b'",                        "a|b")]
	[InlineData("\"ab\" | \"abc\"",                 "ab|abc")]
	[InlineData("('a' | \"ab\") & 'b'",             "(a|ab)b")]
	[InlineData("(\"ab\" | 'a') & 'b'",             "(ab|a)b")]

	// Quantifiers, and the giving back that makes them work next to anything else.
	[InlineData("'a'? & 'a'",                       "a?a")]
	[InlineData("'a'* & 'a'",                       "a*a")]
	[InlineData("'a'+ & 'a'",                       "a+a")]
	[InlineData("'a'* & 'a' & 'b'",                 "a*ab")]
	[InlineData("'a'{2} & 'a'",                     "a{2}a")]
	[InlineData("'a'{1,3} & 'a'",                   "a{1,3}a")]
	[InlineData("'a'{0,2} & 'a'{0,2}",              "a{0,2}a{0,2}")]
	[InlineData("('a' | 'b')* & 'c'",               "(a|b)*c")]
	[InlineData("('a'+)+ & 'b'",                    "(a+)+b")]
	[InlineData("('a' & 'b')* & 'a'",               "(ab)*a")]

	// Element sets.
	[InlineData("['a'..'b']+ & 'b'",                "[a-b]+b")]
	[InlineData("[^ 'a']* & 'c'",                   "[^a]*c")]
	[InlineData("['a' | 'c']{1,2} & any",           "[ac]{1,2}[\\s\\S]")]

	// Lookahead.
	[InlineData("?='a' & ['a'..'c']+",              "(?=a)[a-c]+")]
	[InlineData("?!'a' & ['a'..'c']+",              "(?!a)[a-c]+")]
	[InlineData("['a'..'c']* & ?!'c' & any",        "[a-c]*(?!c)[\\s\\S]")]

	// Nesting, where an engine that only half backtracks comes apart.
	[InlineData("(('a' | 'b')+ & 'c'?)+ & 'b'",     "((a|b)+c?)+b")]
	[InlineData("('a' & 'b'?)* & 'a' & 'c'",        "(ab?)*ac")]

	// Not here, deliberately: a repetition of something nullable — `(a?b?)*`. .NET
	// accepts it and stops the loop by a rule of its own; .Gram refuses the grammar
	// (GRAM4001), because a body that can match nothing makes the repetition mean
	// nothing. A divergence by decision is not a divergence to test for.
	public void Agrees_with_the_equivalent_pattern(string grammar, string pattern)
	{
		var recognize = Compile($"Start = {grammar}\nparse Start");
		var regex     = new Regex("^(?:" + pattern + ")$", RegexOptions.CultureInvariant);
		var wrong     = new List<string>();

		foreach (var input in Inputs())
			if (recognize(input) != regex.IsMatch(input))
				wrong.Add($"\"{input}\": .Gram says {recognize(input)}, regex says {regex.IsMatch(input)}");

		Assert.True(
			wrong.Count == 0,
			$"'{grammar}' and /{pattern}/ disagree on {wrong.Count} of {Inputs().Count()} inputs:\n" +
			string.Join("\n", wrong.Take(10)));
	}

	/// <summary>Compiles a grammar once and returns something that runs it.</summary>
	static Func<string, bool> Compile(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Empty(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		return input => EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess;
	}

	/// <summary>Every string over the alphabet, shortest first, up to the length limit.</summary>
	static IEnumerable<string> Inputs()
	{
		var current = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= Length; length++)
		{
			var next = new List<string>(current.Count * Alphabet.Length);

			foreach (var prefix in current)
				foreach (var letter in Alphabet)
					next.Add(prefix + letter);

			foreach (var input in next)
				yield return input;

			current = next;
		}
	}
}
