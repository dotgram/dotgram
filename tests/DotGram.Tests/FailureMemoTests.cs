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

	/// <summary>
	/// What a grammar with a context is handed: a switch a guard can turn on, and the context the
	/// last one turned on, for a construction that reads it without naming <c>context</c>.
	/// </summary>
	const string Context =
		"public sealed class Ctx " +
		"{ " +
		"public static Ctx Current = new Ctx(); " +
		"public bool On; " +
		"public bool Set() { On = true; Current = this; return true; } " +
		"} " +
		"public static bool Allowed(System.ReadOnlySpan<char> input, ref int pos) { return Ctx.Current.On; }";

	/// <summary>
	/// <c>C</c> refused at the start while the context is off, the context turned on, and <c>C</c>
	/// entered again at the same token: read again, it reads. A rule that reaches the context is not
	/// remembered, whichever way it reaches it, so the memo answers what the reader without it does.
	/// </summary>
	[Theory]
	[InlineData("a guard",                       "C = '(' & C & ')' | Lexical.Name & when @(context.On)")]
	[InlineData("a guard in a rule below",       "C = '(' & C & ')' | Lexical.Name & Checked\nChecked = when @(context.On)")]
	[InlineData("a selector",                    "C = '(' & C & ')' | Lexical.Name & switch @(context.On ? 1 : 0) { case 1: none default: '#' }")]
	[InlineData("a construction a guard reads",  "C = '(' & C & ')' | b: Probe & when @(b)\nProbe : @bool = Lexical.Name => @(context.On)")]
	public void A_rule_that_reads_the_context_is_read_again_where_the_context_changed(string where, string rules)
	{
		var grammar =
			Lexical +
			"context : @Ctx\n" +
			"Start = (C & '!' | Flip & C) & eof\n" +
			"Flip = when @(context.Set())\n" +
			rules.Replace("\\n", "\n", StringComparison.Ordinal) + "\n" +
			"parse Start\n";

		var with    = Contextual(grammar, memo: true, "a");
		var without = Contextual(grammar, memo: false, "a");

		Assert.True(without.IsSuccess, $"{where}: the reader without the memo refused: {without.Error}");
		Assert.Equal(without, with);
		Assert.Contains("C (context)", Reported(grammar, members: Context), StringComparison.Ordinal);
	}

	/// <summary>
	/// The same where what changes is the reading's own state: <c>C</c> refused outside a mark, then
	/// entered at the same token inside one, where the construction its guard reads sees the mark.
	/// </summary>
	[Fact]
	public void A_rule_that_reads_the_reading_state_is_read_again_inside_a_mark()
	{
		const string grammar =
			Lexical +
			"state : @int\n" +
			"Start = (C & '!' | M) & eof\n" +
			"M = C with state @(1)\n" +
			"C = '(' & C & ')' | b: Probe & when @(b)\n" +
			"Probe : @bool = Lexical.Name => @(parserState.ToArray().Length > 0)\n" +
			"parse Start\n";

		var with    = EmittedCode.Match(Compiled(grammar, memo: true), "Grammar", "TryParseStart", "a");
		var without = EmittedCode.Match(Compiled(grammar, memo: false), "Grammar", "TryParseStart", "a");

		Assert.Equal(without, with);
		Assert.Contains("C (context)", Reported(grammar), StringComparison.Ordinal);
	}

	/// <summary>
	/// <b>The contract the memo rests on, broken on purpose.</b> A construction a guard reads runs
	/// during recognition; this one reads the context through a static alias rather than by its name,
	/// which nothing here can see. Turned on between two entries of <c>C</c> at one token, it would
	/// let the second read: the reader without the memo reads it, the reader with it answers with the
	/// failure it remembered. §7.2 forbids exactly this — a guard, a selector, a
	/// recognizer and a value one of them reads answer from their arguments, captures and the text —
	/// and this test holds the consequence where it is documented, so that a change to it is a
	/// decision and not an accident.
	/// </summary>
	[Theory]
	[InlineData("a construction a guard reads", "C = '(' & C & ')' | b: Probe & when @(b)\nProbe : @bool = Lexical.Name => @(Ctx.Current.On)")]
	public void State_read_behind_the_generators_back_breaks_the_contract_and_the_memo_shows_it(string where, string rules)
	{
		var grammar =
			Lexical +
			"context : @Ctx\n" +
			"Start = (C & '!' | Flip & C) & eof\n" +
			"Flip = when @(context.Set())\n" +
			rules + "\n" +
			"parse Start\n";

		Assert.True(Contextual(grammar, memo: false, "a").IsSuccess, $"{where}: the reader without the memo refused.");
		Assert.False(Contextual(grammar, memo: true, "a").IsSuccess, $"{where}: the reader with the memo read it.");
	}

	/// <summary>
	/// An external recognizer reading the same state behind the generator's back: it reads
	/// characters, so the grammar is not cut into tokens, and a reader over characters remembers
	/// nothing — the second entry is read again and both readers read it.
	/// </summary>
	[Fact]
	public void A_rule_that_calls_an_external_recognizer_is_read_over_characters_and_read_again()
	{
		const string grammar =
			Lexical +
			"context : @Ctx\n" +
			"Start = (C & '!' | Flip & C) & eof\n" +
			"Flip = when @(context.Set())\n" +
			"C = '(' & C & ')' | Lexical.Name & @Allowed\n" +
			"parse Start\n";

		Assert.Contains("C (characters)", Reported(grammar, members: Context), StringComparison.Ordinal);
		Assert.True(Contextual(grammar, memo: true, "a").IsSuccess);
		Assert.Equal(Contextual(grammar, memo: false, "a"), Contextual(grammar, memo: true, "a"));
	}

	/// <summary>
	/// A nest deep enough to be carried onto stacks of its own: <c>D</c> fails in a look first (where
	/// nothing is remembered), then outside it, where the failures deep in the nest are remembered on
	/// the stacks the reading was carried to; the second alternative steps over every bracket and
	/// meets the bit at the bottom after the reading has come back. Refused the same way, at the same
	/// place, with the same message, with the memo and without it.
	/// </summary>
	[Theory]
	[InlineData("+")]
	[InlineData("a")]
	public void A_reading_carried_onto_another_stack_remembers_and_answers_as_the_reader_without_it(string bottom)
	{
		const string grammar =
			Lexical +
			"Start = ?!(D & 'z') & D & eof | '('+ & D & '!' & eof\n" +
			"D = '(' & D & ')' | Lexical.Name\n" +
			"parse Start\n";

		var input   = new string('(', 4000) + bottom;
		var with    = OnSmallStack(Compiled(grammar, memo: true), input);
		var without = OnSmallStack(Compiled(grammar, memo: false), input);

		Assert.False(without.IsSuccess);
		Assert.Equal(without, with);
	}

	/// <summary>A reading on a thread with a quarter of a megabyte of stack: deep enough nests are carried off it.</summary>
	static (bool IsSuccess, object? Value, string? Error, long Position) OnSmallStack(Assembly assembly, string input)
	{
		(bool, object?, string?, long) answer = default;
		Exception? thrown = null;

		var thread = new System.Threading.Thread(() =>
		{
			try { answer = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input); }
			catch (Exception e) { thrown = e; }
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw new InvalidOperationException("the reading threw", thrown);

		return answer;
	}

	/// <summary>A grammar with a context read with a fresh one, and nothing left over from the reading before.</summary>
	static (bool IsSuccess, string? Error, long Position) Contextual(string grammar, bool memo, string input)
	{
		var assembly = Compiled(grammar, memo, members: Context);
		var context  = assembly.GetType("Grammar+Ctx")!;

		context.GetField("Current")!.SetValue(null, Activator.CreateInstance(context));

		var method = assembly.GetType("Grammar")!.GetMethods()
			.Single(static one => one.Name == "TryParseStart" && one.GetParameters() is [{ ParameterType: var text }, { ParameterType.Name: "Ctx" }] && text == typeof(string));
		var match  = method.Invoke(null, [input, Activator.CreateInstance(context)])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return ((bool)Read("IsSuccess")!, (string?)Read("Error"), (long)Read("Position")!);
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

	static Assembly Compiled(string grammar, bool memo, bool counts = false, string? members = null)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue((grammar, memo, counts), out var assembly))
				_compiled[(grammar, memo, counts)] = assembly = EmittedCode.Compile(
					Source(grammar, memo, counts, members), "Grammar", declarationMembers: members, symbols: counts ? ["DOTGRAM_COUNTS"] : null);

			return assembly;
		}
	}

	/// <param name="members">The host's own members, which the generator is told of as a build would tell it.</param>
	static string Source(string grammar, bool memo, bool counts = false, string? members = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName       = "Grammar",
			CSharpScanner   = RoslynCSharpScanner.Instance,
			SymbolResolver  = Resolver(members),
			Lexical         = true,
			CountRules      = counts,
			MemoiseFailures = memo,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return result.Sources[0].Text;
	}

	/// <summary>What the generator asks C# questions of: the host and its own members, as a build would have it.</summary>
	static RoslynSymbolResolver Resolver(string? members)
	{
		var host = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"Host",
			[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}")],
			EmittedCode.References);

		return new RoslynSymbolResolver(host, "Grammar");
	}

	/// <summary>The report's word on which rules are remembered.</summary>
	static string Reported(string grammar, bool lexical = true, string? members = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = Resolver(members),
			Lexical        = lexical,
			ReportCarriers = true,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return string.Join("\n", result.Carriers!.Where(static line => line.StartsWith("memo ", StringComparison.Ordinal)));
	}
}
