using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Grammars of deep and wide call graphs for GRAM4052, each asked on a thread with the smallest
/// stack a process starts with, a megabyte, and held to a time and an allocation: the walks that
/// follow calls go a bounded depth or with a stack of their own, ask each rule once, and charge
/// every step. Each is asked with what it should find, so a check that gave up early would not
/// pass: a shape that reads its run two ways is told through the same walks. Read by the fast
/// suite at depths past the limits and by the slow one thousands deep.
/// </summary>
internal static class DeepCallGraphs
{
	/// <param name="shape">Which grammar (<see cref="Grammar"/>).</param>
	/// <param name="chain">How many calls long a chain is.</param>
	internal static void Ask(string shape, int chain, int loops, int told, int skipped)
	{
		var (graph, split) = Built(Grammar(shape, chain));

		ExponentialRepetition.Result? result = null;

		var steps = 0;
		var bytes = 0L;
		var watch = Stopwatch.StartNew();

		OnThread(
			() =>
			{
				bytes  = GC.GetAllocatedBytesForCurrentThread();
				result = ExponentialRepetition.Check(graph, split, out steps);
				bytes  = GC.GetAllocatedBytesForCurrentThread() - bytes;
			},
			1 << 20);

		TestContext.Current.SendDiagnosticMessage(
			$"{shape}, {chain}: {result!.Loops} loops, {result.Skipped} skipped, {result.Found.Count} told, {steps} steps, {bytes} bytes, {watch.Elapsed.TotalMilliseconds:F1} ms");

		Assert.Equal(loops, result.Loops);
		Assert.Equal(told, result.Found.Count);
		Assert.Equal(skipped, result.Skipped);
		Assert.True(steps > 0, "No work was counted.");
		Assert.True(steps <= 1 << 20, $"{steps} steps");
		Assert.True(bytes < 32 << 20, $"{bytes} bytes allocated");
		Assert.True(watch.Elapsed.TotalSeconds < 5, $"{watch.Elapsed.TotalMilliseconds:F0} ms");
	}

	/// <summary>The grammars of <see cref="Ask"/>, the chains <paramref name="chain"/> calls long.</summary>
	internal static string Grammar(string shape, int chain)
	{
		var grammar = new StringBuilder();

		// `@` stands for the rule before, and `@(` is C#, a guard's and not a call of that rule.
		void Rules(int from, int to, string body)
		{
			for (var i = from; i <= to; i++)
				grammar.Append($"A{i} = {Regex.Replace(body, @"@(?!\()", $"A{i - 1}")}\n");
		}

		switch (shape)
		{
			case "look over a diamond":
				grammar.Append("A0 = 'a'\n");
				Rules(1, 24, "@ | @ & any");
				grammar.Append("R = (?!A24 & 'x')* & 'b'\n");
				break;

			case "look over a diamond, then a run read two ways":
				grammar.Append("A0 = 'a'\n");
				Rules(1, 24, "@ | @ & any");
				grammar.Append("R = (?!A24 & 'x') & ('c' | 'c' & 'c')+ & 'b'\n");
				break;

			case "chain of guards":
				grammar.Append("A0 = when @(true)\n");
				Rules(1, chain, "when @(true) & @");
				grammar.Append($"R = (A{chain} & 'a')* & 'b'\n");
				break;

			case "chain of guards, then a run read two ways":
				grammar.Append("A0 = when @(true)\n");
				Rules(1, chain, "when @(true) & @");
				grammar.Append($"R = (A{chain} & ('a' | 'a' & 'a')+)* & 'b'\n");
				break;

			case "chain of aliases repeated":
				grammar.Append("A0 = 'a'\n");
				Rules(1, chain, "@");
				grammar.Append($"R = A{chain}* & 'b'\n");
				break;

			case "diamond of aliases repeated":
				grammar.Append("A0 = 'a'\n");
				Rules(1, 18, "@ | @");
				grammar.Append("R = A18* & 'b'\n");
				break;

			// Every path down the diamond meets the depth before it meets a rule that cannot fail:
			// asked of each rule once, from the bottom, whatever the depth.
			case "braces before a diamond under aliases":
				grammar.Append("A0 = 'a'\n");
				Rules(1, 47, "@");
				Rules(48, 65, "@ | @ & any");
				grammar.Append("R = { ('x' | 'x' & 'x')+ & A65 } & 'b'\n");
				break;
		}

		return grammar.Append("parse R\n").ToString();
	}

	/// <summary>The graph of a grammar, built on a thread with room for the normalizer's own walks down a deep chain.</summary>
	static (RecognitionGraph Graph, LexicalSplit? Split) Built(string grammar)
	{
		RecognitionGraph? graph = null;

		OnThread(
			() =>
			{
				var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);

				Assert.DoesNotContain(parsed.Diagnostics, static one => one.Severity == GramSeverity.Error);

				graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(parsed.File, null, grammar.Length), scanner: RoslynCSharpScanner.Instance);

				Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);
			},
			256 << 20);

		return (graph!, null);
	}

	internal static void OnThread(Action run, int stack)
	{
		Exception? failed = null;

		var thread = new System.Threading.Thread(
			() =>
			{
				try
				{
					run();
				}
				catch (Exception error)
				{
					failed = error;
				}
			},
			stack);

		thread.Start();
		thread.Join();

		if (failed is not null)
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failed).Throw();
	}
}
