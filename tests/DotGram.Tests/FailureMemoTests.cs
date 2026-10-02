using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The memo of failures (<c>Machine.Memo.cs</c>) over small grammars: what it takes off a nest of
/// brackets tried three ways, that it answers what the reader without it answers, a failure inside
/// a look it must not remember, the rules it must not remember at all, and a grammar it leaves
/// exactly as it was.
/// </summary>
public sealed class FailureMemoTests
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
	/// A bracket read three ways, each of them reading everything inside it first: an unclosed nest
	/// of n is 3^n readings of the innermost one where nothing remembers that it failed.
	/// </summary>
	const string Tried =
		Lexical +
		"Start = C & eof\n" +
		"C = P & '+' | P & '-' | P\n" +
		"P = '(' & C & ')' | Lexical.Name\n" +
		"parse Start\n";

	[Fact]
	public void A_nest_tried_three_ways_costs_its_depth()
	{
		var with    = Compiled(Tried, memo: true, counts: true);
		var without = Compiled(Tried, memo: false, counts: true);

		long[] remembered = [.. new[] { 4, 8, 16, 32 }.Select(n => Entries(with, new string('(', n) + "a"))];
		long[] forgotten  = [.. new[] { 4, 8 }.Select(n => Entries(without, new string('(', n) + "a"))];

		// Each doubling adds twice what the one before did: linear.
		Assert.True(
			remembered[3] - remembered[2] <= 2.2 * (remembered[2] - remembered[1]) &&
			remembered[2] - remembered[1] <= 2.2 * (remembered[1] - remembered[0]),
			$"With the memo: {string.Join(", ", remembered)} rule entries at 4, 8, 16 and 32 levels.");

		// And without it, four levels more cost far more than twice as much: the test has a cube to take away.
		Assert.True(
			forgotten[1] > 20 * forgotten[0],
			$"Without the memo: {string.Join(", ", forgotten)} rule entries at 4 and 8 levels.");
	}

	[Theory]
	[InlineData("((((a")]
	[InlineData("((((a)")]
	[InlineData("((a)+)-")]
	[InlineData("(((a)+)-)")]
	[InlineData("((a)+")]
	[InlineData("a +")]
	[InlineData("")]
	public void The_memo_answers_what_the_reader_without_it_answers(string input)
	{
		Same(Tried, input);
	}

	/// <summary>
	/// <c>C</c> is read inside the look first and fails there, where nothing is recorded; read again
	/// outside it, it has to say what it expected, deep in the nest, and not leave the refusal to the
	/// <c>'z'</c> the look's alternative wanted after the first bracket. Both alternatives begin with
	/// one, so the choice cannot pass the first by its token.
	/// </summary>
	[Theory]
	[InlineData("((+")]
	[InlineData("((a")]
	[InlineData("z")]
	[InlineData("((a))")]
	public void A_failure_inside_a_look_is_not_remembered(string input)
	{
		const string looked =
			Lexical +
			"Start = ?!C & '(' & 'z' | C & eof\n" +
			"C = P & '+' | P & '-' | P\n" +
			"P = '(' & C & ')' | Lexical.Name\n" +
			"parse Start\n";

		Assert.Contains("// Nothing a look refuses is recorded", Source(looked, memo: true), StringComparison.Ordinal);

		Same(looked, input);
	}

	[Theory]
	[InlineData("a guard",                "P = '(' & C & ')' & when @(context != null) | Lexical.Name")]
	[InlineData("a construction",         "P : @string = '(' & c: C & ')' => @(context.ToString()) | n: Lexical.Name => @(n)")]
	[InlineData("a guard in a rule below", "P = '(' & C & ')' & Checked | Lexical.Name\nChecked = when @(context != null)")]
	public void A_rule_that_reaches_a_hook_naming_context_is_not_remembered(string where, string rule)
	{
		var grammar =
			Lexical +
			"context : @object\n" +
			"Start = C & eof\n" +
			"C = P & '+' | P & '-' | P\n" +
			rule + "\n" +
			"parse Start\n";

		var said = Reported(grammar);

		Assert.True(said.Contains("(context)", StringComparison.Ordinal), $"{where}: {said}");
		Assert.Contains("remembered 0", said, StringComparison.Ordinal);
	}

	/// <summary>The same name as a capture, where no context is declared, is only a name.</summary>
	[Fact]
	public void A_capture_named_context_is_remembered()
	{
		var said = Reported(
			Lexical +
			"Start = C & eof\n" +
			"C = P & '+' | P & '-' | P\n" +
			"P : @string = '(' & context: C & ')' => @(context) | n: Lexical.Name => @(n)\n" +
			"parse Start\n");

		Assert.DoesNotContain("(context)", said, StringComparison.Ordinal);
		Assert.DoesNotContain("remembered 0", said, StringComparison.Ordinal);
	}

	[Fact]
	public void A_rule_that_gives_back_is_not_remembered()
	{
		var said = Reported(
			Lexical +
			"Start = C & eof\n" +
			"C? = P & '+' | P & '-' | P\n" +
			"P? = '(' & C & ')' | Lexical.Name\n" +
			"parse Start\n");

		Assert.Contains("(gives back)", said, StringComparison.Ordinal);
		Assert.Contains("remembered 0", said, StringComparison.Ordinal);
	}

	[Fact]
	public void No_rule_of_a_reading_that_recovers_is_remembered()
	{
		const string recovering =
			Lexical +
			"Start = Row* recover ';' & eof\n" +
			"Row = C & ';'\n" +
			"C = P & '+' | P & '-' | P\n" +
			"P = '(' & C & ')' | Lexical.Name\n" +
			"parse Start\n";

		// Whichever reads it: a reader of methods says why it remembers nothing, and the engine
		// remembers nothing anyway.
		var said = Reported(recovering);

		Assert.DoesNotContain("MemoFailed(pos", Source(recovering, memo: true), StringComparison.Ordinal);
		Assert.True(said.Length == 0 || said.Contains("(recover)", StringComparison.Ordinal), said);
	}

	[Fact]
	public void A_reading_over_characters_is_not_remembered()
	{
		var said = Reported(
			"Start = C & eof\n" +
			"C = P & '+' | P & '-' | P\n" +
			"P = '(' & C & ')' | ['a'..'z']+\n" +
			"parse Start\n",
			lexical: false);

		Assert.Contains("(characters)", said, StringComparison.Ordinal);
		Assert.Contains("remembered 0", said, StringComparison.Ordinal);
	}

	/// <summary>Nothing reaches itself, so nothing is remembered, and the parser is written as it was.</summary>
	[Fact]
	public void A_grammar_without_recursion_is_written_as_it_was()
	{
		const string flat =
			Lexical +
			"Start = Pair & (',' & Pair)* & eof\n" +
			"Pair = Lexical.Name & '=' & Lexical.Name | Lexical.Name\n" +
			"parse Start\n";

		Assert.Equal(Source(flat, memo: false), Source(flat, memo: true));
	}

	/// <summary>The grammar compiled with the memo and without it answers each input alike.</summary>
	static void Same(string grammar, string input)
	{
		var with    = Compiled(grammar, memo: true);
		var without = Compiled(grammar, memo: false);

		Assert.Equal(
			EmittedCode.Match(without, "Grammar", "TryParseStart", input),
			EmittedCode.Match(with, "Grammar", "TryParseStart", input));
	}

	/// <summary>Rule entries of one reading, from a fresh zero; the reading refuses.</summary>
	static long Entries(Assembly assembly, string input)
	{
		var counters = assembly.GetType("Grammar")!.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	static readonly Dictionary<(string, bool, bool), Assembly> _compiled = [];

	static Assembly Compiled(string grammar, bool memo, bool counts = false)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue((grammar, memo, counts), out var assembly))
				_compiled[(grammar, memo, counts)] = assembly = EmittedCode.Compile(
					Source(grammar, memo, counts), "Grammar", symbols: counts ? ["DOTGRAM_COUNTS"] : null);

			return assembly;
		}
	}

	static string Source(string grammar, bool memo, bool counts = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName       = "Grammar",
			CSharpScanner   = RoslynCSharpScanner.Instance,
			Lexical         = true,
			CountRules      = counts,
			MemoiseFailures = memo,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return result.Sources[0].Text;
	}

	/// <summary>The report's word on which rules are remembered.</summary>
	static string Reported(string grammar, bool lexical = true)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			Lexical        = lexical,
			ReportCarriers = true,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return string.Join("\n", result.Carriers!.Where(static line => line.StartsWith("memo ", StringComparison.Ordinal)));
	}
}
