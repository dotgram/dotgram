using System;
using System.Collections;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The value of a rule without <c>=&gt;</c>, by the rule of its type (§4.1): a string is its pieces joined, a span the
/// bounds of what it matched, an array the values of the calls of its element type, any other type the one call of its
/// type on each route or the constructor its names fill; a left-recursive rule is its seed and its steps.
/// </summary>
/// <remarks>
/// The grammars a left-recursive rule broke on before: a generator crash for a left-recursive span and array, a
/// construction the C# compiler refused for a string typed by a string rule and for a rule typed by a generated one.
/// </remarks>
public sealed class ComposedValueTests
{
	const string Item = "\nItem : @string = n: Num => @(\"<\" + n + \">\")\nNum : string = ['0'..'9']+";

	[Theory]
	[InlineData("1.", "[0..1)")]
	[InlineData("1+2.", "[0..3)")]
	public void A_left_recursive_span_is_its_own_bounds(string input, string expected)
	{
		Assert.Equal(expected, Parsed(
			"Start : @string = s: Sum & '.' & eof => @(s.ToString())\nSum : @SourceSpan = Num | Sum & '+' & Num\nNum = ['0'..'9']+",
			input));
	}

	/// <summary>The value so far of a step is the elements so far: a call of the array's own type gives its elements in place.</summary>
	[Theory]
	[InlineData("L : @string[] = Item | L & ',' & Item", "1", "<1>")]
	[InlineData("L : @string[] = Item | L & ',' & Item", "1,2", "<1>,<2>")]
	[InlineData("L : @string[] = Item & (',' & L)?", "1,2", "<1>,<2>")]
	[InlineData("L : @string[] = Item & (',' & L)?", "1,2,3", "<1>,<2>,<3>")]
	public void An_array_of_its_own_type_gives_its_elements_in_place(string rule, string input, string expected)
	{
		Assert.Equal(expected, Parsed("Start : @string = s: L & eof => @(string.Join(\",\", s))\n" + rule + Item, input));
	}

	[Theory]
	[InlineData("1", "<1>")]
	[InlineData("1+2", "<1>+<2>")]
	[InlineData("1+2+3", "<1>+<2>+<3>")]
	public void A_left_recursive_string_joins_its_steps(string input, string expected)
	{
		Assert.Equal(expected, Parsed("Start : @string = s: Sum & eof => @(s)\nSum : Item = Item | Sum & '+' & Item" + Item, input));
	}

	/// <summary>The seed is built from its names, and the step hands the value so far on.</summary>
	[Theory]
	[InlineData("1=2", "1=2")]
	[InlineData("1=2+3", "1=2")]
	[InlineData("1=2+3+4", "1=2")]
	public void A_rule_of_a_generated_type_builds_its_seed_and_hands_it_on(string input, string expected)
	{
		Assert.Equal(expected, Parsed(
			"Start : @string = e: E & eof => @(e.A + \"=\" + e.B)\nE : P = E & '+' & Num | a: Num & '=' & b: Num\n" +
			"P = a: Num & '=' & b: Num\nQ = p: P\nNum : string = ['0'..'9']+",
			input));
	}

	/// <summary>
	/// A rule of another type takes the call of its type that every route reads, or else the construction its names fill on
	/// every alternative: where the recursion is not on the left, an alternative that names nothing leaves it unbuilt.
	/// </summary>
	[Fact]
	public void A_rule_built_from_names_on_some_alternatives_only_is_told()
	{
		var found = Refused(
			"E : P = @Ws & E & '+' & Num | a: Num & '=' & b: Num\nP = a: Num & '=' & b: Num\nQ = p: P\nNum : string = ['0'..'9']+");

		Assert.Contains("'E' is built by 'P(a, b)' from its names, and alternative 1 of its 2", found.Message, StringComparison.Ordinal);
		Assert.Contains("'=>", found.Message, StringComparison.Ordinal);
	}

	/// <summary>A step's call of the rule itself is its one call of the type, whatever else the step names.</summary>
	[Theory]
	[InlineData("1=2", "1=2")]
	[InlineData("1=2+34+56", "1=2")]
	public void A_left_recursive_step_hands_the_value_so_far_on(string input, string expected)
	{
		Assert.Equal(expected, Parsed(
			"Start : @string = e: E & eof => @(e.A + \"=\" + e.B)\nE : P = E & '+' & a: Num & b: Num | a: Num & '=' & b: Num\n" +
			"P = a: Num & '=' & b: Num\nQ = p: P\nNum : string = ['0'..'9']",
			input));
	}

	[Theory]
	[InlineData("Start : @string = '<' & Word & '>'", "<ab>", "<W:ab>")]
	[InlineData("Start : Word = '<' & Word & '>'", "<ab>", "<W:ab>")]
	[InlineData("Start : Word = Word & ';'", "ab;", "W:ab;")]
	[InlineData("Start : Word = Word & ' ' & Word", "ab cd", "W:ab W:cd")]
	[InlineData("Start : @string = '(' & Start & ')' | Word", "((ab))", "((W:ab))")]
	[InlineData("Start : @string = '<' & P2 & '>'\nP2 = '(' & Word & ')'", "<(ab)>", "<(W:ab)>")]
	[InlineData("Start : Word = '<' & Word? & '>'", "<>", "<>")]
	[InlineData("Start : @string = '<' & Word & (',' & Word)* & '>'", "<ab,cd>", "<W:ab,W:cd>")]
	[InlineData("Start : @string = $q: Word & ',' & $q", "ab,ab", "W:ab,ab")]
	[InlineData("Start : @string = '\"' & (Esc | Bare)* & '\"'", "\"a\\nb\"", "\"a\nb\"")]
	[InlineData("Start : @string = '-'? & Bare", "-ab", "-ab")]
	[InlineData("Start : @string = { '<' & Word & '>' }", "<ab>", "<W:ab>")]
	[InlineData("Start : @string = { '<' & Word & '>' | Bare }", "cd", "cd")]
	[InlineData("trivia = [' ']*\nStart : @string = Bare & '.' & Bare", "a . b", "a.b")]
	public void A_string_is_its_pieces_joined(string grammar, string input, string expected)
	{
		const string helpers =
			"\nWord : @string = c: ['a'..'z']+ => @(\"W:\" + c.ToString())\nBare : @string = ['a'..'z']+" +
			"\nEsc : @char = '\\\\' & 'n' => @('\\n')";

		Assert.Equal(expected, Parsed(grammar + helpers, input));
	}

	[Theory]
	[InlineData("Start : @string[] = Bare & ',' & Word", "abc,de", "abc;W:de")]
	[InlineData("Start : @string[] = Bare & (',' & Bare)*", "ab,cd", "ab;cd")]
	[InlineData("Start : @string[] = w: Word & ';' & Word", "ab;cd", "W:ab;W:cd")]
	[InlineData("Start : @string[] = Word & x: ';' & Word", "ab;cd", "W:ab;W:cd")]
	[InlineData("Start : @string[] = Line*\nLine = Bare & ';'", "a;b;", "a;b")]
	public void An_array_collects_the_calls_of_its_element_type(string grammar, string input, string expected)
	{
		var value = Parsed(grammar + "\nWord : @string = c: ['a'..'z']+ => @(\"W:\" + c.ToString())\nBare : @string = ['a'..'z']+", input, true);

		Assert.Equal(expected, string.Join(";", ((IEnumerable)value!).Cast<object>()));
	}

	/// <summary>An element type that can hold the array itself leaves a call of the array's type with two readings.</summary>
	[Fact]
	public void A_call_that_is_an_element_and_an_array_of_them_is_told()
	{
		var found = Refused("Start : @object[] = O\nO : @object[] = w: ['a'..'z']+ => @(new object[] { w.ToString() })");

		Assert.Contains("'=>'", found.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("Start : @string = '<' & Num & '>'\nNum : @int = d: ['0'..'9']+ => @(d.Length)", "'Num' is 'int'")]
	[InlineData("Start : @string = '<' & Span & '>'\nSpan : @SourceSpan = ['a'..'z']+", "'Span' is 'SourceSpan'")]
	public void A_piece_with_no_text_is_told_to_write_the_construction(string grammar, string fact)
	{
		var found = Refused(grammar);

		Assert.Contains(fact, found.Message, StringComparison.Ordinal);
		Assert.Contains("=> @(\"<\" + n + \">\")", found.Message, StringComparison.Ordinal);
	}

	/// <summary>A rule that reaches itself on the left through a rule of its own is refused rather than guessed at.</summary>
	[Fact]
	public void An_indirect_left_recursion_is_refused()
	{
		var result = Compile("Start : A = A\nA : @string = B & 'x' | 'y'\nB : @string = A & 'z' | 'w'\nparse Start");

		Assert.Contains(result.Diagnostics, static one => one.Severity == GramSeverity.Error && one.Id == GrammarNormalizer.LeftRecursion);
	}

	static GramDiagnostic Refused(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");

		return Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error && one.Id == GrammarNormalizer.UnbuiltConstruction);
	}

	static string? Parsed(string grammar, string input, string members = "")
	{
		return (string?)Parsed(grammar, input, true, members);
	}

	static object? Parsed(string grammar, string input, bool any, string members = "")
	{
		var result = Compile(grammar + "\nparse Start");

		// The helpers a row does not call are told unused (GRAM4018), which is no question of these.
		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != "GRAM4018"));

		var source = result.Sources[0].Text;

		if (members.Length > 0)
			source += "\npublic partial class Grammar { " + members + " }\n";

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(source, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		Assert.True(isSuccess, error);

		return value;
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}
}
