using System;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A mark an abandoned alternative placed stands over nothing afterwards, including where what
/// reads it is built while the text is still being read.
/// </summary>
/// <remarks>
/// <para>
/// The reader keeps the marks it has open rather than replaying them out of the log, and a
/// give-back has to undo the ones it discards. <b>Two conditions have to hold together before a
/// stack that only pushed and popped gets this wrong</b>, and that is what this file is arranged
/// around: one grammar per condition, and one with both. Only the third fails when the journal is
/// taken out, which is how it is known that the conjunction carries the defect rather than one half
/// of it. The other two are here to fail in that same experiment and do not — <b>a fixture for this
/// which satisfies one condition passes on the defect</b>, and both of the first two fixtures
/// written for it did.
/// </para>
/// <para>
/// <b>The failure must land INSIDE the marked node.</b> A mark's close is written as soon as the
/// node is read, so an alternative given up at something FOLLOWING the node discards the open and
/// the close together, and a stack that pushed on one and popped on the other is right about that
/// by arithmetic. It is the open going without its close that leaves such a stack standing one too
/// deep for the rest of the parse, pointing at a log slot the next alternative has since written
/// its own mark into.
/// </para>
/// <para>
/// <b>And the standing marks must be read while the reading is still going on</b>, which means a
/// guard: it builds a value in the middle of an alternative, and that is the only walk which
/// begins past a mark. A walk that begins at the log's start — every walk in a grammar with no
/// guard — finds the marks by stepping over the records it is reading anyway, and never consults
/// the kept stack at all. <c>MarkPositionsTests</c> satisfies neither condition, which is why it
/// passes with the journal out although it is the file about abandoned marks.
/// </para>
/// <para>
/// Each grammar reports which marks stood over the leaf, so a failure reads as the defect rather
/// than as a number: <c>a:1</c> is one mark standing and <c>a:1,1</c> is a discarded one still on
/// the stack, its value read back out of the slot the surviving mark now occupies.
/// </para>
/// </remarks>
public sealed class MarkGiveBackTests
{
	/// <summary>
	/// Both conditions: <c>Pair</c> is abandoned inside the mark, and the next alternative builds at
	/// a guard. <b>This is the one that fails without the journal.</b>
	/// </summary>
	const string Both =
		"state : @int\n" +
		"Start : @string\n" +
		"	= '(' & two: Pair  with state @(9) & ')'                                      => @(\"<\" + two + \">\")\n" +
		"	| '(' & one: Start with state @(1) & when @(!string.IsNullOrEmpty(one)) & ')'  => @(\"[\" + one + \"]\")\n" +
		"	| leaf: Word                                                                  => @(leaf)\n" +
		"Pair : @string = a: Word & ',' & b: Word => @(a + b)\n" +
		"Word : @string = text: ['a'..'z']+ => @(text + \":\" + string.Join(\",\", parserState.ToArray()))\n" +
		"parse Start\n";

	/// <summary>
	/// A guard, but the failure lands after the mark's close: the <c>;</c> the first alternative
	/// wants and will not get is outside the marked node, so a balanced pair is discarded.
	/// </summary>
	const string AfterTheClose =
		"state : @int\n" +
		"Start : @string\n" +
		"	= '(' & one: Start with state @(9) & when @(!string.IsNullOrEmpty(one)) & ';'  => @(\"<\" + one + \">\")\n" +
		"	| '(' & one: Start with state @(1) & when @(!string.IsNullOrEmpty(one)) & ')'  => @(\"[\" + one + \"]\")\n" +
		"	| leaf: Word                                                                  => @(leaf)\n" +
		"Word : @string = text: ['a'..'z']+ => @(text + \":\" + string.Join(\",\", parserState.ToArray()))\n" +
		"parse Start\n";

	/// <summary>
	/// The failure inside the marked node, but nothing reads the marks mid-reading: no <c>when</c>,
	/// so every walk begins at the log's start and reads the marks off the log in front of it.
	/// </summary>
	const string NoGuard =
		"state : @int\n" +
		"Start : @string\n" +
		"	= '(' & two: Pair  with state @(9) & ')'  => @(\"<\" + two + \">\")\n" +
		"	| '(' & one: Start with state @(1) & ')'  => @(\"[\" + one + \"]\")\n" +
		"	| leaf: Word                             => @(leaf)\n" +
		"Pair : @string = a: Word & ',' & b: Word => @(a + b)\n" +
		"Word : @string = text: ['a'..'z']+ => @(text + \":\" + string.Join(\",\", parserState.ToArray()))\n" +
		"parse Start\n";

	public static TheoryData<string> Readings => new() { "engine", "tape", "immediate" };

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_mark_abandoned_inside_its_node_does_not_stand_over_what_a_guard_builds_next(string reading)
	{
		var probe = Compiled(Both, "Both", reading);

		// The two ends this sits between, so that a reading which got those wrong could not pass
		// here by being consistently wrong: no mark at all, and the first alternative reading
		// through to its close.
		Assert.Equal("a:",       Value(probe, "Both", "a"));
		Assert.Equal("<a:9b:9>", Value(probe, "Both", "(a,b)"));

		// And the case. `Pair` fails at the comma it wants, so the first alternative is given up
		// with its mark open; the second places its own and builds `a` under it at the guard.
		Assert.Equal("[a:1]", Value(probe, "Both", "(a)"));

		// Twice over: a stack one too deep after the first give-back is two too deep here, so this
		// cannot pass by arriving at the right depth for some other reason.
		Assert.Equal("[[a:1,1]]", Value(probe, "Both", "((a))"));
	}

	/// <remarks>
	/// One condition and not the other, on purpose. The assertion is the same as above and it holds
	/// with the journal taken out, which is what makes this file's claim about the conjunction a
	/// measurement rather than a reading of the code.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_mark_given_up_after_its_close_was_written_costs_the_stack_nothing(string reading)
	{
		var probe = Compiled(AfterTheClose, "AfterTheClose", reading);

		Assert.Equal("a:",    Value(probe, "AfterTheClose", "a"));
		Assert.Equal("<a:9>", Value(probe, "AfterTheClose", "(a;"));
		Assert.Equal("[a:1]", Value(probe, "AfterTheClose", "(a)"));
	}

	/// <remarks>The other half, and the same: one condition, and it passes with the journal out.</remarks>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_mark_abandoned_inside_its_node_is_read_off_the_log_where_no_guard_builds(string reading)
	{
		var probe = Compiled(NoGuard, "NoGuard", reading);

		Assert.Equal("a:",        Value(probe, "NoGuard", "a"));
		Assert.Equal("<a:9b:9>",  Value(probe, "NoGuard", "(a,b)"));
		Assert.Equal("[a:1]",     Value(probe, "NoGuard", "(a)"));
		Assert.Equal("[[a:1,1]]", Value(probe, "NoGuard", "((a))"));
	}

	static string? Value(Assembly probe, string name, string input)
	{
		var match = EmittedCode.Match(probe, "Marked." + name, "TryParseStart", input);

		Assert.True(match.IsSuccess, $"\"{input}\": {match.Error}");

		return match.Value?.ToString();
	}

	static Assembly Compiled(string grammar, string name, string reading)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = name,
			Namespace     = "Marked",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = reading != "engine",
			Carrier       = reading == "immediate" ? CarrierKind.Immediate : CarrierKind.Tape,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		// The tape reading has to be the one that keeps a stack of standing marks, or none of this
		// asserts anything about it: the walk which reads such a stack is the materializer's, and
		// the tape is the only carrier with one.
		if (reading == "tape")
			Assert.Contains("ways.MarksBackTo(", source, StringComparison.Ordinal);
		else
			Assert.DoesNotContain("ways.MarksBackTo(", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, name, "Marked");
	}
}
