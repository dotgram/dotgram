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
/// </remarks>
[Collection(typeof(Alone))]
public sealed class DeepChainMatrixTests
{
	const int Depth = 4000;

	const int StackBytes = 512 * 1024;

	/// <summary>A way one rule refers to the one below: the bottom rule, a level, and what publishes the top.</summary>
	/// <remarks>
	/// <paramref name="Depth"/> where a kind costs time that grows faster than the chain — a chain of
	/// rebindings, each specializing what is below it, and the readers a rebinding has to check — so
	/// that it is asked on a shorter chain and a smaller stack, which asks the same question.
	/// </remarks>
	sealed record Kind(string Name, string Bottom, Func<int, string> Level, Func<int, string> Top, int Depth = DeepChainMatrixTests.Depth, int Stack = StackBytes);

	static readonly Kind[] Kinds =
	[
		new("call", "A0 = 'x'", static i => $"A{i} = 'x' & A{i - 1}", static n => $"R = A{n} & 'z'"),
		new("alias", "A0 = 'x'", static i => $"A{i} = A{i - 1}", static n => $"R = A{n} & 'z'"),
		new("forwarder", "A0 : @int = 'b' => @(1)", static i => $"A{i} : @int = v:A{i - 1} => @(v)",
			static n => $"R : @int = v:A{n} => @(v)"),
		new("transparent", "A0 : @int = 'b' => @(1)", static i => $"A{i} : A{i - 1} = v:A{i - 1} => @(v)",
			static n => $"R : A{n} = v:A{n} => @(v)"),
		new("with", "B = 'b'\nC = 'c'\nA0 = B", static i => $"A{i} = A{i - 1} with (B = C)", static n => $"R = A{n} & 'z'",
			1500, 256 * 1024),
		new("reader", "B : @int = 'b' => @(1)\nD : @string = 'd' => @(\"d\")\nA0 : B = when @(true) & B",
			static i => $"A{i} : A{i - 1} = when @(true) & A{i - 1}", static n => $"R : A{n} = A{n}", 1500, 256 * 1024),
		new("guard", "A0 = when @(true)", static i => $"A{i} = when @(true) & A{i - 1}", static n => $"R = A{n} & 'z'"),
		new("guard after", "A0 = \"a\" | \"ab\"", static i => $"A{i} = A{i - 1} & when @(true)",
			static n => $"Row : @string = 'r' => @(\"r\")\nR : @string[] = A{n} & 'b' & Row*"),
		new("lookahead", "A0 = 'x'", static i => $"A{i} = (?!'q' & A{i - 1}) & 'x'", static n => $"R = A{n} & 'z'"),
		new("condition", "L0 = 'x'", static i => $"L{i} = L{i - 1}", static n => $"R = when L{n} is 'x' & 'z'"),
		new("values", "A0 : @int = 'x' => @(0)", static i => $"A{i} : @int = 'x' & v:A{i - 1} => @(v + 1)",
			static n => $"R : @int = v:A{n} => @(v)"),
		new("negative look over values",
			"Word : @string = 'a' => @(\"a\")\nA0 : @string[] = (?!('a' & 'b') & Word)* & when @(true)",
			static i => $"A{i} : A{i - 1} = A{i - 1} & when @(true)", static n => $"R : A{n} = A{n} & 'a' & 'b'"),
	];

	/// <summary>How the top of the chain is published: as written, under a repetition, behind a negative look.</summary>
	static readonly string[] Tops = ["plain", "repeated", "looked"];

	/// <summary>Every case: a kind, the order the rules are declared in, a seam or none, characters or cut in two, a top.</summary>
	public static TheoryData<string, bool> Groups()
	{
		var data = new TheoryData<string, bool>();

		foreach (var kind in Kinds)
		{
			data.Add(kind.Name, false);
			data.Add(kind.Name, true);
		}

		return data;
	}

	/// <summary>One kind and one order, every other dimension of it at once.</summary>
	[Theory]
	[MemberData(nameof(Groups))]
	public void Every_shape_of_a_chain_compiles_on_a_small_stack(string kind, bool descending)
	{
		var cases =
			from trivia in new[] { false, true }
			from lexical in new[] { false, true }
			from top in Tops
			select (Trivia: trivia, Lexical: lexical, Top: top);

		var failures = new System.Collections.Concurrent.ConcurrentBag<string>();

		Parallel.ForEach(cases, new ParallelOptions { MaxDegreeOfParallelism = 6 }, one =>
		{
			var shape   = Kinds.Single(k => k.Name == kind);
			var grammar = Grammar(shape, descending, one.Trivia, one.Top, shape.Depth);
			var errors  = OnSmallStack(shape.Stack, () => GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = one.Lexical,
			}))
				.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Select(static d => d.ToString()).ToArray();

			if (errors.Length > 0)
				failures.Add($"{kind}, {(descending ? "descending" : "ascending")}, trivia {one.Trivia}, lexical {one.Lexical}, {one.Top}: {errors[0]}");
		});

		Assert.Empty(failures);
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

		var published = kind.Top(depth);

		published = top switch
		{
			"repeated" => published.Replace($"A{depth} & ", $"(A{depth} & 'q')* & ", StringComparison.Ordinal),
			"looked"   => published.Replace($"R = A{depth}", $"R = (?!A{depth} & 'q') & A{depth}", StringComparison.Ordinal),
			_          => published,
		};

		text.Append(published).Append('\n');

		return text.Append(kind.Name == "reader" ? "parse R with (B = D)\n" : "parse R\n").ToString();
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
