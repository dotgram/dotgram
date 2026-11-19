using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What building the tree costs the generator, measured on the largest grammar there is: the
/// lowering run over T-SQL's recognition graph, in allocated bytes and in time.
/// </summary>
/// <remarks>
/// The lowering does not cover all of T-SQL — what it does not lower becomes one node where the
/// graph has a subtree — so the numbers are a floor for a full lowering and are reported with
/// the share of rules and of graph nodes they stand for. The graph is the generator's: read with
/// the standard library, bound, normalized, and cut into its lexical and syntactic halves, the
/// second of which is what the reader is printed from.
/// </remarks>
public sealed class AllocationTests(ITestOutputHelper output)
{
	/// <summary>The bytes the generator allocates for T-SQL before any of this, and what may be added to them.</summary>
	const long Baseline = 2_705_104_184;
	const long Budget   = Baseline / 20;

	[Fact]
	public void LoweringTransactSqlStaysInsideTheAllocationBudget()
	{
		var text  = File.ReadAllText(Path.Combine(Graphs.Repository, "src", "DotGram.Sql", "TransactSql", "TransactSql.gram"));
		var whole = Graphs.OfShipped(text);

		Assert.False(whole.HasErrors, string.Join("\n", whole.Diagnostics.Take(5)));

		var split = LexicalSplit.Of(whole);
		var lines = new List<string>();

		Measure("the whole graph, over characters", whole);

		// The other half of the cut is the whole graph again, which the lexer is made from.
		if (split is not null)
			Measure("the syntactic half, over kinds", split.Syntax);

		foreach (var line in lines)
			output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllLines(Path.Combine(report, "allocation.txt"), lines);

		void Measure(string name, RecognitionGraph graph)
		{
			// The analyses the lowering asks are the generator's own, which it has run by the time
			// a lowering would: the first run pays for them, the second is the lowering alone.
			var (first, firstBytes, firstTime)    = Lowered(graph);
			var (second, secondBytes, secondTime) = Lowered(graph);

			var complete = first.Functions.Count(static function => function.Complete);

			lines.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: {graph.Rules.Count} rules, {first.GraphNodes} graph nodes"));
			lines.Add(string.Create(CultureInfo.InvariantCulture, $"  lowered whole: {complete} of {first.Functions.Count} functions ({100.0 * complete / first.Functions.Count:F1} %); graph nodes with no lowering: {first.UnsupportedNodes} ({100.0 * first.UnsupportedNodes / Math.Max(1, first.GraphNodes):F2} %)"));
			lines.Add(string.Create(CultureInfo.InvariantCulture, $"  tree: {first.NodeCount} nodes, {first.Functions.Sum(static function => function.SlotCount)} slots, {first.Fragments.Count} fragments, {first.Expected.Count} expected names"));
			lines.Add(string.Create(CultureInfo.InvariantCulture, $"  first run (with the proofs it asks):  {firstBytes / 1e6:F1} MB, {100.0 * firstBytes / Baseline:F2} % of the baseline, {firstTime.TotalMilliseconds:F0} ms"));
			lines.Add(string.Create(CultureInfo.InvariantCulture, $"  second run (proofs already answered): {secondBytes / 1e6:F1} MB, {100.0 * secondBytes / Baseline:F2} % of the baseline, {secondTime.TotalMilliseconds:F0} ms"));

			foreach (var kind in first.UnsupportedKinds.OrderByDescending(static one => one.Value))
				lines.Add(string.Create(CultureInfo.InvariantCulture, $"    not lowered: {kind.Value} x {kind.Key}"));

			Assert.Equal(first.NodeCount, second.NodeCount);
			Assert.InRange(firstBytes, 1, Budget);
		}

		// The lowering runs on this thread alone, so the bytes this thread allocated round it are
		// the lowering's. The process-wide count would add whatever the test classes running beside
		// this one allocate meanwhile, and the budget would fail on their account.
		static (IrProgram Program, long Bytes, TimeSpan Time) Lowered(RecognitionGraph graph)
		{
			var before  = GC.GetAllocatedBytesForCurrentThread();
			var watch   = Stopwatch.StartNew();
			var program = IrLowering.Lower(graph, null, everyRule: true);

			watch.Stop();

			return (program, GC.GetAllocatedBytesForCurrentThread() - before, watch.Elapsed);
		}
	}
}
