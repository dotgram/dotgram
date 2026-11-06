using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The memo of successes (<c>Machine.Memo.cs</c>) over small grammars: what it takes off a nest an
/// ordered choice reads once per alternative at every level, that it answers what the reader
/// without it answers and hands the value the first reading built, the hint that names the choice
/// (GRAM4054) and says whether the recursion is remembered, and a grammar without the shape, which
/// is written exactly as it was.
/// </summary>
public sealed class SuccessMemoTests
{
	/// <summary>A split grammar: the memo is for a reader over tokens.</summary>
	const string Lexical =
		"trivia = { ' '* }\n" +
		"namespace Lexical\n" +
		"{\n" +
		"\ttrivia = none\n" +
		"\tName = ['a'..'z']+\n" +
		"}\n";

	/// <summary>
	/// The textbook shape: two alternatives that read a bracketed nest of the rule itself and are
	/// told apart only by the sign after the bracket. <c>((a)-)-</c> reads the inner bracket as
	/// <c>B</c> first, which fails at the sign, and then as <c>C</c>, which reads it again — at every
	/// level, two to the depth, unless the second entry is answered from the first.
	/// </summary>
	const string Textbook =
		Lexical +
		"Start = A & eof\n" +
		"A = B | C | Lexical.Name\n" +
		"B = '(' & A & ')' & '+'\n" +
		"C = '(' & A & ')' & '-'\n" +
		"parse Start\n";

	/// <summary>The same, building a value: the sign of each level in front of the name.</summary>
	const string Valued =
		Lexical +
		"Start : @string = a: A & eof => @(a)\n" +
		"A : @string = b: B => @(b) | c: C => @(c) | n: Lexical.Name => @(n)\n" +
		"B : @string = '(' & a: A & ')' & '+' => @(\"+\" + a)\n" +
		"C : @string = '(' & a: A & ')' & '-' => @(\"-\" + a)\n" +
		"parse Start\n";

	static string Nest(int depth, string sign)
	{
		return string.Concat(Enumerable.Repeat("(", depth)) + "a" + string.Concat(Enumerable.Repeat(")" + sign, depth));
	}

	[Fact]
	public void A_nest_read_once_per_alternative_costs_its_depth()
	{
		var with    = Compiled(Textbook, keeps: true, counts: true);
		var without = Compiled(Textbook, keeps: false, counts: true);

		long[] remembered = [.. new[] { 4, 8, 16, 32 }.Select(n => Entries(with, Nest(n, "-")))];
		long[] forgotten  = [.. new[] { 4, 8, 12 }.Select(n => Entries(without, Nest(n, "-")))];

		// Twice the depth, at most twice the entries and a little for the ends: linear.
		Assert.True(
			remembered[3] <= 2.2 * remembered[2] && remembered[2] <= 2.2 * remembered[1] && remembered[1] <= 2.2 * remembered[0],
			$"With the memo: {string.Join(", ", remembered)} rule entries at 4, 8, 16 and 32 levels.");

		// And without it, four levels more cost far more than twice as much: the doubling the memo takes away.
		Assert.True(
			forgotten[1] > 8 * forgotten[0] && forgotten[2] > 8 * forgotten[1],
			$"Without the memo: {string.Join(", ", forgotten)} rule entries at 4, 8 and 12 levels.");
	}

	[Theory]
	[InlineData("((a)-)-")]
	[InlineData("((a)+)-")]
	[InlineData("((a)-)+")]
	[InlineData("(((a)-)-)")]
	[InlineData("((a)-")]
	[InlineData("((a))")]
	[InlineData("((a)-)-)")]
	[InlineData("(a")]
	[InlineData("")]
	public void The_memo_answers_what_the_reader_without_it_answers(string input)
	{
		Assert.Equal(
			EmittedCode.Match(Compiled(Textbook, keeps: false), "Grammar", "TryParseStart", input),
			EmittedCode.Match(Compiled(Textbook, keeps: true), "Grammar", "TryParseStart", input));
	}

	/// <summary>A second entry is handed the value the first reading built, and the levels above it build on that.</summary>
	[Theory]
	[InlineData("((a)-)-", "--a")]
	[InlineData("(((a)+)-)-", "--+a")]
	[InlineData("((((a)-)-)-)-", "----a")]
	public void A_second_entry_is_handed_the_first_readings_value(string input, string value)
	{
		var with    = EmittedCode.Match(Compiled(Valued, keeps: true), "Grammar", "TryParseStart", input);
		var without = EmittedCode.Match(Compiled(Valued, keeps: false), "Grammar", "TryParseStart", input);

		Assert.True(with.IsSuccess, with.Error);
		Assert.Equal(value, with.Value);
		Assert.Equal(without, with);
	}

	/// <summary>
	/// A construction that may throw: what a reading held of that is let go with the reading, so
	/// nothing is remembered where the grammar says <c>ConstructionsMayThrow</c>, and the second
	/// entry throws as the first did. <c>((x)-)-</c> read the inner bracket as <c>B</c> first, whose
	/// <c>Name</c> threw into the held exception, and dropped it with the alternative; remembered,
	/// the second alternative would have been handed the value and nothing thrown.
	/// </summary>
	[Theory]
	[InlineData("((x)-)-")]
	[InlineData("(((x)-)-)-")]
	[InlineData("((a)-)-")]
	public void Where_a_construction_may_throw_nothing_is_remembered_and_the_held_exception_is_thrown(string input)
	{
		const string throwing =
			Lexical +
			"Start : @string = a: A & eof => @(a)\n" +
			"A : @string = b: B => @(b) | c: C => @(c) | n: Lexical.Name => @(n == \"x\" ? throw new System.InvalidOperationException(\"boom\") : n)\n" +
			"B : @string = '(' & a: A & ')' & '+' => @(\"+\" + a)\n" +
			"C : @string = '(' & a: A & ')' & '-' => @(\"-\" + a)\n" +
			"parse Start\n";

		var (diagnostics, source) = Compile(throwing, keeps: true, mayThrow: true);
		var hint = Assert.Single(diagnostics, static one => one.Id == GramCompiler.SharedRecursions);

		Assert.Contains("a construction may throw (ConstructionsMayThrow)", hint.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("Kept(pos", source, StringComparison.Ordinal);

		var with    = Answer(Compiled(throwing, keeps: true, mayThrow: true), input);
		var without = Answer(Compiled(throwing, keeps: false, mayThrow: true), input);

		Assert.Equal(without, with);
		Assert.Equal(input.Contains('x', StringComparison.Ordinal) ? "threw InvalidOperationException" : "--a", with);
	}

	/// <summary>
	/// A success read unbuilt — the first alternative calls <c>A</c> for nothing, so <c>A</c> is read
	/// with its constructions off — is remembered as that, and the second alternative, which builds,
	/// reads it again rather than take a value that was never built.
	/// </summary>
	[Theory]
	[InlineData("((a)-)-", "--a")]
	[InlineData("(((a)+)-)-", "--+a")]
	public void A_success_read_unbuilt_is_read_again_where_the_value_is_needed(string input, string value)
	{
		const string unused =
			Lexical +
			"Start : @string = A & '!' => @(\"!\") | a: A & eof => @(a)\n" +
			"A : @string = b: B => @(b) | c: C => @(c) | n: Lexical.Name => @(n)\n" +
			"B : @string = '(' & a: A & ')' & '+' => @(\"+\" + a)\n" +
			"C : @string = '(' & a: A & ')' & '-' => @(\"-\" + a)\n" +
			"parse Start\n";

		var with = EmittedCode.Match(Compiled(unused, keeps: true), "Grammar", "TryParseStart", input);

		Assert.True(with.IsSuccess, with.Error);
		Assert.Equal(value, with.Value);
		Assert.Equal(EmittedCode.Match(Compiled(unused, keeps: false), "Grammar", "TryParseStart", input), with);
	}

	/// <summary>A climbing rule is remembered at strength zero only: entered as an operand of its own operator, it is read.</summary>
	[Theory]
	[InlineData("((a)-)- + a", "--a+a")]
	[InlineData("a + ((a)-)- + (a)-", "a+--a+-a")]
	[InlineData("((a)-)- +", null)]
	public void A_climbing_rule_remembers_at_strength_zero_only(string input, string? value)
	{
		const string climbing =
			Lexical +
			"Start : @string = a: A & eof => @(a)\n" +
			"A : @string = left: A & '+' & right: A << 1 => @(left + \"+\" + right) | b: B => @(b) | c: C => @(c) | n: Lexical.Name => @(n)\n" +
			"B : @string = '(' & a: A & ')' & '+' => @(\"+\" + a)\n" +
			"C : @string = '(' & a: A & ')' & '-' => @(\"-\" + a)\n" +
			"parse Start\n";

		var with    = EmittedCode.Match(Compiled(climbing, keeps: true), "Grammar", "TryParseStart", input);
		var without = EmittedCode.Match(Compiled(climbing, keeps: false), "Grammar", "TryParseStart", input);

		Assert.Equal(without, with);
		Assert.Equal(value is not null, with.IsSuccess);

		if (value is not null)
			Assert.Equal(value, with.Value);
	}

	/// <summary>A nest read inside a look first is not remembered there, and the reading outside the look answers as the reader without the memo does.</summary>
	[Theory]
	[InlineData("((a)-)-")]
	[InlineData("((a)-)+")]
	[InlineData("((a)-")]
	[InlineData("((a)+)-!")]
	public void A_reading_inside_a_look_is_not_remembered(string input)
	{
		const string looked =
			Lexical +
			"Start : @string = ?=(A & '!') & a: A & '!' => @(a + \"!\") | a: A & eof => @(a)\n" +
			"A : @string = b: B => @(b) | c: C => @(c) | n: Lexical.Name => @(n)\n" +
			"B : @string = '(' & a: A & ')' & '+' => @(\"+\" + a)\n" +
			"C : @string = '(' & a: A & ')' & '-' => @(\"-\" + a)\n" +
			"parse Start\n";

		Assert.Equal(
			EmittedCode.Match(Compiled(looked, keeps: false), "Grammar", "TryParseStart", input),
			EmittedCode.Match(Compiled(looked, keeps: true), "Grammar", "TryParseStart", input));
	}

	/// <summary>What a reading answers, or the exception it threw, as one word.</summary>
	static string Answer(Assembly assembly, string input)
	{
		try
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			return match.IsSuccess ? (string)match.Value! : "refused: " + match.Error;
		}
		catch (TargetInvocationException thrown)
		{
			return "threw " + thrown.InnerException?.GetType().Name;
		}
		catch (InvalidOperationException)
		{
			return "threw InvalidOperationException";
		}
	}

	/// <summary>The hint at the rule: information, naming the choice, the recursion's rules remembered, and the rewrite.</summary>
	[Fact]
	public void The_hint_names_the_choice_and_the_rules_remembered()
	{
		var (diagnostics, source) = Compile(Textbook, keeps: true);
		var hint = Assert.Single(diagnostics, static one => one.Id == GramCompiler.SharedRecursions);

		Assert.Equal(GramSeverity.Info, hint.Severity);
		Assert.StartsWith("'A' tries its alternatives 1 and 2 in turn", hint.Message, StringComparison.Ordinal);
		Assert.Contains("The parser remembers where 'A' succeeds", hint.Message, StringComparison.Ordinal);
		Assert.Contains("factor the prefix the alternatives share", hint.Message, StringComparison.Ordinal);
		Assert.Contains("Kept(pos", source, StringComparison.Ordinal);
		Assert.Contains("kept 1: A", Reported(Textbook), StringComparison.Ordinal);
	}

	/// <summary>Over characters nothing is remembered, and the hint says so and that the rewrite is the remedy.</summary>
	[Fact]
	public void Over_characters_the_hint_says_the_rewrite_is_the_remedy()
	{
		const string characters =
			"Start = A & eof\n" +
			"A = B | C | ['a'..'z']+\n" +
			"B = '(' & A & ')' & '+'\n" +
			"C = '(' & A & ')' & '-'\n" +
			"parse Start\n";

		var (diagnostics, source) = Compile(characters, keeps: true, lexical: false);
		var hint = Assert.Single(diagnostics, static one => one.Id == GramCompiler.SharedRecursions);

		Assert.Contains("the parser cannot remember the recursion's successes here (the rules are read over characters)", hint.Message, StringComparison.Ordinal);
		Assert.Contains("Write the alternatives to read the shared text once", hint.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("Kept(pos", source, StringComparison.Ordinal);
	}

	/// <summary>Switched off, the hint says so, and nothing is remembered.</summary>
	[Fact]
	public void Switched_off_nothing_is_remembered()
	{
		var (diagnostics, source) = Compile(Textbook, keeps: false);
		var hint = Assert.Single(diagnostics, static one => one.Id == GramCompiler.SharedRecursions);

		Assert.Contains("the memo of successes is off", hint.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("Kept(pos", source, StringComparison.Ordinal);
	}

	/// <summary>A recursion no choice reads twice has no success remembered, and the reader is written as it was.</summary>
	[Fact]
	public void A_grammar_without_the_shape_is_written_as_it_was()
	{
		const string plain =
			Lexical +
			"Start = A & eof\n" +
			"A = '(' & A & ')' | Lexical.Name & '+' | Lexical.Name\n" +
			"parse Start\n";

		var (diagnostics, source) = Compile(plain, keeps: true);

		Assert.DoesNotContain(diagnostics, static one => one.Id == GramCompiler.SharedRecursions);
		Assert.DoesNotContain("Kept(pos", source, StringComparison.Ordinal);
		Assert.Equal(Compile(plain, keeps: false).Source, source);
	}

	/// <summary>Rule entries of one reading, from a fresh zero.</summary>
	static long Entries(Assembly assembly, string input)
	{
		var counters = assembly.GetType("Grammar")!.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	static readonly Dictionary<(string, bool, bool, bool), Assembly> _compiled = [];

	static Assembly Compiled(string grammar, bool keeps, bool counts = false, bool mayThrow = false)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue((grammar, keeps, counts, mayThrow), out var assembly))
				_compiled[(grammar, keeps, counts, mayThrow)] = assembly = EmittedCode.Compile(
					Compile(grammar, keeps, counts, mayThrow: mayThrow).Source, "Grammar", symbols: counts ? ["DOTGRAM_COUNTS"] : null);

			return assembly;
		}
	}

	static (IReadOnlyList<GramDiagnostic> Diagnostics, string Source) Compile(string grammar, bool keeps, bool counts = false, bool lexical = true, bool mayThrow = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName             = "Grammar",
			CSharpScanner         = RoslynCSharpScanner.Instance,
			SymbolResolver        = Resolver(),
			Lexical               = lexical,
			CountRules            = counts,
			MemoiseSuccesses      = keeps,
			ConstructionsMayThrow = mayThrow,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return (result.Diagnostics, result.Sources[0].Text);
	}

	/// <summary>What the generator asks C# questions of: the host as a build would have it.</summary>
	static RoslynSymbolResolver Resolver()
	{
		var host = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"Host",
			[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("public partial class Grammar { }")],
			EmittedCode.References);

		return new RoslynSymbolResolver(host, "Grammar");
	}

	/// <summary>The report's word on which rules are remembered, and which keep their successes.</summary>
	static string Reported(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = Resolver(),
			Lexical        = true,
			ReportCarriers = true,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return string.Join("\n", result.Carriers!.Where(static line => line.StartsWith("memo ", StringComparison.Ordinal)));
	}
}
