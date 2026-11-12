using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every way one rule can refer to the next, as a chain thousands long, declared both ways round,
/// with and without a seam, read over characters and cut in two, and published plainly, under a
/// repetition and behind a look: each compiled on a stack of half a megabyte.
/// </summary>
/// <remarks>
/// <para>
/// A chain of rules is walked by many passes, and a pass that follows a reference by recursion
/// takes a frame per rule of it. The passes were found one by one, by a shape someone thought to
/// try — and the shape that found the next one was always one nobody had: the rules declared
/// from the top down rather than from the bottom up, a reference through a type rather than a
/// call, the reader of a rebinding, a lookahead cut in two. So the shapes are generated here as a
/// cross product, and a pass that follows some kind of reference by recursion takes the process
/// down on one of them.
/// </para>
/// <para>
/// <b>Half a megabyte and four thousand levels</b> rather than a megabyte and ten thousand: the
/// question is whether a walk's depth grows with the chain, and a smaller stack asks it of a
/// shorter chain — any walk with a frame of more than about a hundred and thirty bytes a level
/// overflows here — in a fraction of the time. The walks that move to a fresh stack
/// (<c>DeepStack</c>) move as they would on any other.
/// </para>
/// <para>
/// Not Alone, and in three classes so that the kinds are read beside each other: the case builds
/// its own grammar and runs on a thread of its own with its own stack, and shares nothing with another
/// case; what is asked is whether a walk's depth grows with the chain, which no neighbour changes.
/// </para>
/// </remarks>
static class DeepChainMatrix
{
	const int Depth = 4000;

	const int StackBytes = 512 * 1024;

	/// <summary>A way one rule refers to the one below: the bottom rule, a level, and what publishes the top.</summary>
	/// <remarks>
	/// <paramref name="Depth"/> where a kind costs time that grows faster than the chain — a chain of
	/// rebindings, each specializing what is below it, and the readers a rebinding has to check — so
	/// that it is asked on a shorter chain and a smaller stack, which asks the same question.
	/// </remarks>
	/// <param name="Head">The publication's head, <c>R = </c> or a typed one; <c>TOP</c> in it is the top of the chain.</param>
	/// <param name="Use">What the publication reads of the top of the chain, given its name; the wrappers go in front.</param>
	/// <param name="Chain">The letter the chain's rules are named with.</param>
	/// <param name="Extra">Rules the publication reads besides the chain.</param>
	/// <param name="Lexical">Whether the kind may be cut in two at all: a grammar that names <c>eos</c> may not (GRAM4049).</param>
	sealed record Kind(
		string Name, string Bottom, Func<int, string> Level, string Head, Func<string, string> Use, string Chain = "A",
		string Extra = "", bool Lexical = true, int Depth = DeepChainMatrix.Depth, int Stack = StackBytes);

	static readonly Kind[] Kinds =
	[
		new("call", "A0 = 'x'", static i => $"A{i} = 'x' & A{i - 1}", "R = ", static x => $"{x} & 'z'"),
		new("alias", "A0 = 'x'", static i => $"A{i} = A{i - 1}", "R = ", static x => $"{x} & 'z'"),
		new("forwarder", "A0 : @int = 'b' => @(1)", static i => $"A{i} : @int = v:A{i - 1} => @(v)",
			"R : @int = ", static x => $"v:{x} => @(v)"),
		new("transparent", "A0 : @int = 'b' => @(1)", static i => $"A{i} : A{i - 1} = v:A{i - 1} => @(v)",
			"R : @int = ", static x => $"v:{x} => @(v)"),
		new("with", "B = 'b'\nC = 'c'\nA0 = B", static i => $"A{i} = A{i - 1} with (B = C)", "R = ", static x => $"{x} & 'z'",
			Depth: 1500, Stack: 256 * 1024),
		new("reader", "B : @int = 'b' => @(1)\nD : @string = 'd' => @(\"d\")\nA0 : B = when @(true) & B",
			static i => $"A{i} : A{i - 1} = when @(true) & A{i - 1}", "R : TOP = ", static x => $"v:{x} => @(v)",
			Depth: 1500, Stack: 256 * 1024),
		new("guard", "A0 = when @(true)", static i => $"A{i} = when @(true) & A{i - 1}", "R = ", static x => $"{x} & 'z'"),
		new("guard after", "A0 = \"a\" | \"ab\"", static i => $"A{i} = A{i - 1} & when @(true)",
			"R : @string[] = ", static x => $"{x} & 'b' & Row*", Extra: "Row : @string = 'r' => @(\"r\")\n"),
		new("lookahead", "A0 = 'x'", static i => $"A{i} = (?!'q' & A{i - 1}) & 'x'", "R = ", static x => $"{x} & 'z'"),
		new("condition", "L0 = 'x'", static i => $"L{i} = L{i - 1}", "R = ", static x => $"when {x} is 'x' & 'z'", Chain: "L"),
		new("values", "A0 : @int = 'x' => @(0)", static i => $"A{i} : @int = 'x' & v:A{i - 1} => @(v + 1)",
			"R : @int = ", static x => $"v:{x} => @(v)"),
		new("negative look over values",
			"Word : @string = 'a' => @(\"a\")\nA0 : @string[] = (?!('a' & 'b') & Word)* & when @(true)",
			static i => $"A{i} : A{i - 1} = A{i - 1} & when @(true)", "R : @string[] = ", static x => $"v:{x} & 'a' & 'b' => @(v)"),
		new("segment", "A0 = 'x'", static i => $"A{i} = 'x' & A{i - 1}", "R : @int = ", static x => $"{x} & eos => @(1)",
			Lexical: false),
	];

	/// <summary>How the top of the chain is published: as written, under a repetition, behind a negative look.</summary>
	static readonly string[] Tops = ["plain", "repeated", "looked"];

	/// <summary>Every case: a kind, the order the rules are declared in, a seam or none, characters or cut in two, a top.</summary>
	public static TheoryData<string, bool> Groups(int part)
	{
		var data = new TheoryData<string, bool>();

		for (var i = part; i < Kinds.Length; i += Parts)
		{
			data.Add(Kinds[i].Name, false);
			data.Add(Kinds[i].Name, true);
		}

		return data;
	}

	/// <summary>The classes the kinds are shared out between, beside each other.</summary>
	public const int Parts = 3;

	/// <summary>One kind and one order, every other dimension of it at once.</summary>
	public static void Run(string kind, bool descending)
	{
		var cases =
			from trivia in new[] { false, true }
			from lexical in new[] { false, true }
			from top in Tops
			select (Trivia: trivia, Lexical: lexical, Top: top);

		var failures = new System.Collections.Concurrent.ConcurrentBag<string>();

		Parallel.ForEach(cases, new ParallelOptions { MaxDegreeOfParallelism = 4 }, one =>
		{
			var shape = Kinds.Single(k => k.Name == kind);

			if (one.Lexical && !shape.Lexical)
				return;

			var grammar = Grammar(shape, descending, one.Trivia, one.Top, shape.Depth);
			var name    = $"{shape.Chain}{shape.Depth}";

			// Each wrapper is in the text it names, so that no two cases are one grammar.
			if (!grammar.Contains(Head(shape, name) + Wrapper(one.Top, name) + shape.Use(name), StringComparison.Ordinal))
				failures.Add($"{kind}, {one.Top}: the publication was not built as named");
			var errors  = OnSmallStack(shape.Stack, () => GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = one.Lexical,
			}))
				.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Select(static d => d.ToString()).ToArray();

			if (errors.Length > 0)
				failures.Add($"{kind}, {(descending ? "descending" : "ascending")}, trivia {one.Trivia}, lexical {one.Lexical}, {one.Top}: {errors[0]}");
		});

		Assert.True(failures.IsEmpty, string.Join("\n", failures.Order(StringComparer.Ordinal)));
	}

	/// <summary>The grammar of one case, <paramref name="depth"/> rules long.</summary>
	static string Grammar(Kind kind, bool descending, bool trivia, string top, int depth)
	{
		var text   = new StringBuilder();
		var levels = Enumerable.Range(1, depth).Select(kind.Level).ToList();

		if (trivia)
			text.Append("trivia = { ' '* }\n");

		// From the top down, the bottom last: the order a reader of the grammar meets the rules
		// in is the order several passes resolve them in.
		if (descending)
		{
			levels.Reverse();
			levels.ForEach(level => text.Append(level).Append('\n'));
			text.Append(kind.Bottom).Append('\n');
		}
		else
		{
			text.Append(kind.Bottom).Append('\n');
			levels.ForEach(level => text.Append(level).Append('\n'));
		}

		var name = $"{kind.Chain}{depth}";

		text.Append(kind.Extra).Append(Head(kind, name)).Append(Wrapper(top, name)).Append(kind.Use(name)).Append('\n');

		return text.Append(kind.Name == "reader" ? "parse R with (B = D)\n" : "parse R\n").ToString();
	}

	static string Head(Kind kind, string name)
	{
		return kind.Head.Replace("TOP", name, StringComparison.Ordinal);
	}

	/// <summary>What stands in front of what the publication reads: nothing, a repetition of the top, or a look at it.</summary>
	static string Wrapper(string top, string name)
	{
		return top switch
		{
			"repeated" => $"({name} & 'q')* & ",
			"looked"   => $"(?!{name} & 'q') & ",
			_          => "",
		};
	}

	static T OnSmallStack<T>(int stack, Func<T> work)
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
			stack);

		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw new InvalidOperationException("The work threw on the small stack.", thrown);

		return result!;
	}
}

/// <inheritdoc cref="DeepChainMatrix"/>
public sealed class DeepChainMatrixTests
{
	public static TheoryData<string, bool> Groups()
	{
		return DeepChainMatrix.Groups(0);
	}

	/// <summary>One kind and one order, every other dimension of it at once.</summary>
	[Theory]
	[MemberData(nameof(Groups))]
	public void Every_shape_of_a_chain_compiles_on_a_small_stack(string kind, bool descending)
	{
		DeepChainMatrix.Run(kind, descending);
	}
}

/// <inheritdoc cref="DeepChainMatrix"/>
public sealed class DeepChainMatrixSecondTests
{
	public static TheoryData<string, bool> Groups()
	{
		return DeepChainMatrix.Groups(1);
	}

	/// <summary>One kind and one order, every other dimension of it at once.</summary>
	[Theory]
	[MemberData(nameof(Groups))]
	public void Every_shape_of_a_chain_compiles_on_a_small_stack(string kind, bool descending)
	{
		DeepChainMatrix.Run(kind, descending);
	}
}

/// <inheritdoc cref="DeepChainMatrix"/>
public sealed class DeepChainMatrixThirdTests
{
	public static TheoryData<string, bool> Groups()
	{
		return DeepChainMatrix.Groups(2);
	}

	/// <summary>One kind and one order, every other dimension of it at once.</summary>
	[Theory]
	[MemberData(nameof(Groups))]
	public void Every_shape_of_a_chain_compiles_on_a_small_stack(string kind, bool descending)
	{
		DeepChainMatrix.Run(kind, descending);
	}
}
