using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Grammars whose rules call each other in a chain ten thousand long, and a thread of one
/// megabyte — the stack a Windows process starts with — to compile or read them on.
/// </summary>
/// <remarks>
/// <para>
/// Every stage walks the call graph: the normalizer asks what reaches what, the fixpoints over
/// first sets, follow sets, nullability, types and extents run rule by rule, the checks look
/// down every path, and the emitter lowers what it reaches; the editor binds, classifies and
/// reports over the same rules. Written as recursion, each of those walks takes a frame per
/// level of the chain, and the length of the chain is the author's to choose — so a valid
/// grammar took the process down, and a stack overflow cannot be caught, reported or survived.
/// </para>
/// <para>
/// <b>What a test over these asserts is the process still being alive.</b> As with
/// <c>StackDepthTests</c>, an overflow ends the process, so a test passing is the process
/// having outlived the shape; and each is also held to an answer, so that a walk that survived
/// by stopping early would not pass for one that ran. Written once here and linked into
/// <c>DotGram.Language.Tests.Slow</c>, which reads the same shapes with the editor.
/// </para>
/// </remarks>
static class DeepChains
{
	/// <summary>Long enough that a frame per level is several megabytes, on any frame size.</summary>
	public const int Depth = 10_000;

	/// <summary>What a Windows thread starts with, and the smallest stack a host is likely to give.</summary>
	public const int StackBytes = 1024 * 1024;

	/// <summary>The shapes a chain is written in, each walked by a different part of the generator.</summary>
	public static IReadOnlyDictionary<string, Func<int, string>> Shapes { get; } =
		new Dictionary<string, Func<int, string>>(StringComparer.Ordinal)
		{
			// A sequence: first sets and extents, every call a step of the one above.
			["sequence"] = static depth => Chain(depth, "A0 = 'x'", static i => $"A{i} = 'x' & A{i - 1}", $"parse A{depth}"),

			// Calls at the start: what a rule begins with is what the next one down begins with.
			["leading"] = static depth => Chain(depth, "A0 = 'x'", static i => $"A{i} = A{i - 1} & 'y'", $"parse A{depth}"),

			// The same under a repetition: what a turn begins with, and whether it runs code before
			// its first item, asked down the whole chain.
			["leading, repeated"] = static depth => Chain(
				depth, "A0 = 'x'", static i => $"A{i} = A{i - 1} & 'y'", $"R = A{depth}* & 'x'\nparse R"),

			// Rules that only forward, under a repetition: collapsed, and judged for exponential
			// readings through every alias.
			["aliases, repeated"] = static depth => Chain(
				depth, "A0 = 'a'", static i => $"A{i} = A{i - 1}", $"R = A{depth}* & 'b'\nparse R"),

			// A choice: each alternative's first set, and a forwarder at every level.
			["choice"] = static depth => Chain(depth, "A0 = 'x'", static i => $"A{i} = A{i - 1} | 'y'", $"parse A{depth}"),

			// Nested repetitions: nullability and follow sets, each loop around the one below.
			["repetition"] = static depth => Chain(
				depth, "A0 = 'x'", static i => $"A{i} = ('y' & A{i - 1})* & 'x'", $"parse A{depth}"),

			// Nullable all the way down, under a repetition that must still be told it advances.
			["guard"] = static depth => Chain(
				depth, "A0 = when @(true)", static i => $"A{i} = when @(true) & A{i - 1}",
				$"R = (A{depth} & 'a')* & 'b'\nparse R"),

			// The guards again, then a run read two ways inside the repetition: told (GRAM4052), and
			// asked down the whole chain to be told.
			["guards, then a run"] = static depth => Chain(
				depth, "A0 = when @(true)", static i => $"A{i} = when @(true) & A{i - 1}",
				$"R = (A{depth} & ('a' | 'a' & 'a')+)* & 'b'\nparse R"),

			// Aliases under a seam: each rule is judged for whether it is one item, through the
			// rule it forwards to, for the seams a lexeme leaves out.
			["aliases, spaced"] = static depth => Chain(
				depth, "trivia = { ' '* }\nA0 = ['x']", static i => $"A{i} = A{i - 1}", $"R = A{depth} & 'z'\nparse R"),

			// Every kind of walk at once, over chains with a seam: guards down one chain, a condition
			// on the whole of another (aliases, which the condition can answer), a rebinding of what
			// the first one's bottom calls, and a recovering repetition over that.
			["everything"] = static depth => Chain(
				depth, "trivia = { ' '* }\nB = 'b'\nA0 = ['x'] & B\nL0 = 'x'",
				static i => $"A{i} = when @(true) & A{i - 1}\nL{i} = L{i - 1}",
				$"R = when L{depth} is 'x' & (A{depth} with (B = 'c'))* recover ';' & 'z'\nparse R"),

			// Values: the type of each rule is the type of the one below, and the publication at
			// the top is the end of a chain of inferred types.
			["values"] = static depth => Chain(
				depth, "A0 : @int = 'x' => @(0)", static i => $"A{i} : @int = 'x' & v:A{i - 1} => @(v + 1)",
				$"parse A{depth}"),

			// Captured text: extents, each the text of the one below and one more character.
			["capture"] = static depth => Chain(
				depth, "A0 = 'x'", static i => $"A{i} = 'x' & A{i - 1}",
				$"R : @string = t:A{depth} => @(t)\nparse R"),
		};

	/// <summary>One rule nested <paramref name="depth"/> groups deep, which is not a chain and is refused (GRAM2011).</summary>
	public static string Nested(int depth)
	{
		return "A = " + new string('(', depth) + "'x'" + new string(')', depth) + "\nparse A\n";
	}

	/// <summary>The names of <see cref="Shapes"/>, for a theory.</summary>
	public static TheoryData<string> Names()
	{
		return new TheoryData<string>(Shapes.Keys);
	}

	/// <summary>The names of <see cref="Shapes"/>, each read by methods and by the engine.</summary>
	public static TheoryData<string, bool> NamesAndReadings()
	{
		var data = new TheoryData<string, bool>();

		foreach (var name in Shapes.Keys)
		{
			data.Add(name, true);
			data.Add(name, false);
		}

		return data;
	}

	static string Chain(int depth, string first, Func<int, string> level, string tail)
	{
		var text = new StringBuilder();

		text.Append(first).Append('\n');

		for (var i = 1; i <= depth; i++)
			text.Append(level(i)).Append('\n');

		return text.Append(tail).Append('\n').ToString();
	}

	/// <summary>Runs <paramref name="work"/> on a thread of <see cref="StackBytes"/> and hands back what it answered.</summary>
	public static T OnSmallStack<T>(Func<T> work)
	{
		var result = default(T);
		var thrown = default(Exception);

		var thread = new Thread(
			() =>
			{
				try
				{
					result = work();
				}
				catch (Exception caught)
				{
					thrown = caught;
				}
			},
			StackBytes);

		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw new InvalidOperationException("The work threw on the small stack.", thrown);

		return result!;
	}
}
