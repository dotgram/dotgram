using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The stack of open marks a reader keeps is, after every step, what a replay of the log would
/// find — including where a give-back discards a mark's close and leaves its open standing.
/// </summary>
/// <remarks>
/// <para>
/// The walk that materialises needs the marks standing over where it begins (§7.8). It used to find
/// them by replaying every open and close from the log's start, which cost the whole log in front of
/// the walk; the reader keeps them instead. <b>Keeping them is only correct if a give-back undoes
/// exactly the changes it discards</b>, and a stack that pushed on open and popped on close cannot:
/// the pop leaves no trace, so a truncation that takes a close away cannot put its mark back.
/// </para>
/// <para>
/// <b>Why this is a test of the journal and not of a grammar.</b> The state is reached by grammars —
/// an alternative abandoned inside a marked node discards its open with nothing having closed it,
/// measured at net=-1 in <c>MarkGiveBackTests</c>' <c>Both</c> and <c>NoGuard</c> — but no walk any
/// suite makes today CONSULTS the stack in that state, because every such walk begins at log
/// position zero, where the marks are read off the log and the kept stack is not asked. So a bare
/// stack is wrong in a reachable state and harmless only by a property of today's grammars, not of
/// the reader. Three fixtures written against the grammar surface failed to reach it; this reaches
/// it by construction, which is the point of testing the structure where it lives.
/// </para>
/// <para>
/// <b>Three implementations, which is what makes this a gate rather than a description.</b>
/// <c>Kept</c> is the real one, read out of a generated parser by reflection. <c>Replayed</c> is the
/// pass it replaced, recomputed from the log. <c>Bare</c> is a stack that only pushes and pops. The
/// first two must agree after every step of every script; the third must disagree somewhere, or the
/// scripts do not reach the case and the agreement above would prove nothing.
/// </para>
/// </remarks>
public sealed class MarkJournalTests
{
	/// <summary>A grammar whose only job is to make a generated <c>Ways</c> that carries the journal.</summary>
	/// <remarks>
	/// It declares a <c>state</c>, so the journal's region of the support is kept, and is read with
	/// the tape, which is the carrier that has a walk to tell. What it reads does not matter: nothing
	/// below parses anything.
	/// </remarks>
	const string Grammar =
		"state : @int\n" +
		"Start : @string = one: Word with state @(1) => @(one)\n" +
		"Word : @string = t: ['a'..'z']+ => @(t + string.Join(\",\", parserState.ToArray()))\n" +
		"parse Start\n";

	/// <summary>One step of a script: what the reader did to the log.</summary>
	enum Step
	{
		/// <summary>A mark opened here.</summary>
		Open,

		/// <summary>And closed.</summary>
		Close,

		/// <summary>The log put back to the position named beside it.</summary>
		Back,
	}

	static readonly string[] Everything =
	[
		// Balanced, which any stack gets right: the control.
		"open close",

		// The state the grammars reach: an open discarded with nothing having closed it.
		"open back:0",

		// A give-back that discards a CLOSE and leaves its open standing. This is the one a bare
		// stack cannot survive, and it is why the journal records closes and not only opens.
		"open open close back:10",

		// The same, with the slot reused in between: A opens at slot 0 and closes, B takes slot 0,
		// and the give-back lands between A's open and its close. Undoing B has to put A's position
		// back before undoing A's close raises the depth over it.
		"open close open close back:5",

		// Three deep, given back into the middle of the nest.
		"open open open close close back:15",

		// And a longer one, so that the journal is undone in several bites rather than one.
		"open open close open close close open back:5",
	];

	public static TheoryData<string> Scripts
	{
		get
		{
			var scripts = new TheoryData<string>();

			foreach (var one in Everything)
				scripts.Add(one);

			return scripts;
		}
	}

	[Theory]
	[MemberData(nameof(Scripts))]
	public void The_kept_stack_is_what_a_replay_of_the_log_would_find(string script)
	{
		var ways = Instance();
		var step = 0;

		foreach (var (what, where) in Parsed(script))
		{
			Apply(ways, what, where);

			step++;

			Assert.Equal(
				Shown(Replayed(ways)),
				Shown(Kept(ways)));
		}

		Assert.True(step > 0, "the script did nothing.");
	}

	/// <summary>
	/// And the other half: a stack that only pushes and pops disagrees with the replay on at least
	/// one of those scripts, so they reach what the test above is about.
	/// </summary>
	/// <remarks>
	/// Without this the agreement above would hold just as well of scripts that never discard a mark,
	/// and the file would be a description of the defect rather than a gate on it. The disagreement is
	/// asserted over the set rather than per script because most of the scripts are controls.
	/// </remarks>
	[Fact]
	public void A_stack_that_only_pushes_and_pops_disagrees_with_the_replay()
	{
		var broken = new List<string>();

		foreach (var script in Everything)
		{
			var ways = Instance();
			var bare = new List<int>();

			foreach (var (what, where) in Parsed(script))
			{
				Apply(ways, what, where);

				// The same step against a stack with no journal: a close pops and a give-back, having
				// nothing recorded, does nothing at all.
				if (what == Step.Open)
					bare.Add(Length(ways) - 5);
				else if (what == Step.Close)
					bare.RemoveAt(bare.Count - 1);

				if (Shown(bare) != Shown(Replayed(ways)))
				{
					broken.Add($"{script} (at {what}{(what == Step.Back ? ":" + where : "")}: " +
						$"bare {Shown(bare)}, replayed {Shown(Replayed(ways))})");

					break;
				}
			}
		}

		Assert.NotEmpty(broken);

		// Named, so that a change which makes the scripts stop reaching the case reads as that and
		// not as a stack that got better.
		Assert.Contains(broken, one => one.StartsWith("open open close back:10", StringComparison.Ordinal));
	}

	/// <summary>The script, as steps and the position a give-back names.</summary>
	static IEnumerable<(Step What, int Where)> Parsed(string script)
	{
		foreach (var word in script.Split(' ', StringSplitOptions.RemoveEmptyEntries))
		{
			if (word == "open")
				yield return (Step.Open, 0);
			else if (word == "close")
				yield return (Step.Close, 0);
			else if (word.StartsWith("back:", StringComparison.Ordinal))
				yield return (Step.Back, int.Parse(word.Substring("back:".Length)));
			else
				throw new InvalidOperationException($"'{word}' is not a step.");
		}
	}

	/// <summary>One step, done to the real thing.</summary>
	/// <remarks>
	/// A give-back is both halves of what the reader does — the journal unwound and the log
	/// truncated — because either alone is a state the reader is never in.
	/// </remarks>
	static void Apply(object ways, Step what, int where)
	{
		switch (what)
		{
			case Step.Open:
				Call(ways, "Mark", -1, 7, 0);
				break;

			case Step.Close:
				Call(ways, "Mark", -2, 7, 0);
				break;

			case Step.Back:
				Call(ways, "MarksBackTo", where);
				Field(ways, "LogCount").SetValue(ways, where);
				break;
		}
	}

	/// <summary>The stack the reader is keeping: the positions of the marks it has open.</summary>
	static List<int> Kept(object ways)
	{
		var open  = (int[])Field(ways, "MarkOpen").GetValue(ways)!;
		var depth = (int)Field(ways, "MarkDepth").GetValue(ways)!;

		return open.Take(depth).ToList();
	}

	/// <summary>And what the pass this replaced would find, read out of the log itself.</summary>
	static List<int> Replayed(object ways)
	{
		var log   = (int[])Field(ways, "Log").GetValue(ways)!;
		var count = Length(ways);
		var open  = new List<int>();

		for (var at = 0; at < count; at += log[at])
		{
			if (log[at + 1] == -1)
				open.Add(at);
			else if (log[at + 1] < 0)
				open.RemoveAt(open.Count - 1);
		}

		return open;
	}

	static int Length(object ways)
	{
		return (int)Field(ways, "LogCount").GetValue(ways)!;
	}

	/// <summary>A failure that says which stacks differed rather than that two lists were unequal.</summary>
	static string Shown(IEnumerable<int> stack)
	{
		return "[" + string.Join(",", stack) + "]";
	}

	static void Call(object ways, string name, params object[] arguments)
	{
		ways.GetType()
			.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!
			.Invoke(ways, arguments);
	}

	static FieldInfo Field(object ways, string name)
	{
		return ways.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!;
	}

	/// <summary>
	/// A <c>Ways</c> of a real generated parser, by reflection and on purpose.
	/// </summary>
	/// <remarks>
	/// The journal is emitted, not written by hand, so a copy of it in this file would be a test of
	/// the copy. What is asserted is a property of what a consumer's build produces.
	/// </remarks>
	static object Instance()
	{
		var result = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Journal",
			Namespace     = "Marked",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = true,
			Carrier       = CarrierKind.Tape,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		// Or there is nothing here to test, and every assertion below would be about a stack the
		// generator no longer writes.
		Assert.Contains("internal void MarksBackTo", source, StringComparison.Ordinal);

		var parser = EmittedCode.Compile(source, "Journal", "Marked").GetType("Marked.Journal")
			?? throw new InvalidOperationException("the probe compiled to no such type.");

		var ways = parser.GetNestedType("Ways", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("the generated parser has no Ways any more.");

		return Activator.CreateInstance(ways, nonPublic: true)!;
	}
}
