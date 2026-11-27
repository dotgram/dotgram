using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// The rule-value gate: every shape of <see cref="RuleShapes"/> compiled by the generator of this build and by the
/// published DotGram 0.3.0, in modes D, S and B, on one input per route, and each input classified by how the two
/// outcomes stand (<see cref="RuleDifference"/>).
/// </summary>
/// <remarks>
/// Environment:
/// <list type="bullet">
/// <item><c>DOTGRAM_RULEVALUES</c>: <c>full</c> (the bounded enumeration and the random tier), a number (that many shapes
/// spread evenly over the enumeration), or unset (only the landmarks below).</item>
/// <item><c>DOTGRAM_RULEVALUES_FILTER</c>: only shapes whose id contains this text.</item>
/// <item><c>DOTGRAM_RULEVALUES_SEED</c>, <c>DOTGRAM_RULEVALUES_RANDOM</c>: the random tier's seed (printed) and size.</item>
/// <item><c>DOTGRAM_RULEVALUES_BATCH</c>, <c>DOTGRAM_RULEVALUES_PARALLEL</c>: jobs per compilation, compilations at once.</item>
/// <item><c>DOTGRAM_RULEVALUES_OUT</c>: where the cells, the summary and the printed grammars are written.</item>
/// </list>
/// </remarks>
public sealed class RuleValueGateTests
{
	/// <summary>
	/// Shapes a reader can recognise, each with the probe it stands for. The ones marked as differences must come out as
	/// differences on the head the gate was built on, or the dimensions do not reach them.
	/// </summary>
	internal static readonly IReadOnlyList<(string Label, RuleShape Shape)> Landmarks =
	[
		Landmark("named operand beside filling names, host type (A04)",           ":Bx", "one", Atom.Implicit, null, NameKind.FillingAndOnOperand),
		Landmark("two operands beside filling names, host type (A03)",            ":Bx", "two", Atom.Implicit, Atom.Implicit, NameKind.Filling),
		Landmark("an alternative without an operand, filling names (A06)",        ":Bx", "alternative-without", Atom.Implicit, null, NameKind.Filling),
		Landmark("named operand beside filling names, generated type (A10)",      ":Item", "one", Atom.Implicit, null, NameKind.FillingAndOnOperand),
		Landmark("filling names on a group holding the operand (A16)",            ":Bx", "one", Atom.Implicit, null, NameKind.FillingOnGroup),
		Landmark("named built operand beside filling names, text (B01)",          ":Word", "one", Atom.Built, null, NameKind.FillingAndOnOperand),
		Landmark("two built operands beside filling names, text (B04)",           ":Word", "two", Atom.Built, Atom.Built, NameKind.Filling),
		Landmark("a built operand only in a lookahead, filling names (B09)",      ":Word", "ahead", Atom.Built, null, NameKind.Filling),
		Landmark("a built operand only in a negative lookahead (B10)",            ":Word", "not-ahead", Atom.Built, null, NameKind.Filling),
		Landmark("a built and an extent operand, no names (T01)",                 ":Word", "two", Atom.Built, Atom.Implicit, NameKind.None),
		Landmark("a built operand and an optional extent one (T03)",              ":Word", "optional-tail", Atom.Built, Atom.Implicit, NameKind.None),
		Landmark("own text around one built operand",                             "@string", "one", Atom.Built, null, NameKind.None),
		Landmark("own text around one extent operand",                            "@string", "one", Atom.Implicit, null, NameKind.None),
		Landmark("own span around one extent operand",                            "@SourceSpan", "one", Atom.Implicit, null, NameKind.None),
		Landmark("taken span around one extent operand",                          ":Span", "one", Atom.Implicit, null, NameKind.None),
		Landmark("own host type, filling names and one operand",                  "@Box", "one", Atom.Implicit, null, NameKind.Filling),
		Landmark("own text, filling names and one built operand",                 "@string", "one", Atom.Built, null, NameKind.Filling),
		Landmark("sequence of an extent and a built operand",                     "@string[]", "two", Atom.Implicit, Atom.Built, NameKind.None),
		Landmark("sequence, a named operand",                                     "@string[]", "two", Atom.Built, Atom.Built, NameKind.OnOperand),
		Landmark("taken text, an optional built operand",                         ":Word", "optional", Atom.Built, null, NameKind.None),
		Landmark("taken host type, filling names and an optional operand",        ":Bx", "optional", Atom.Implicit, null, NameKind.Filling),
		Landmark("own int around one operand",                                    "@int", "one", Atom.Built, null, NameKind.None),
		Landmark("taken text, two built operands",                                ":Word", "two", Atom.Built, Atom.Built, NameKind.None),
		Landmark("own text around an own text rule that calls a building one",    "@string", "one", Atom.OwnOverMaking, null, NameKind.None),
		Landmark("own span around an own span rule that calls a building one",    "@SourceSpan", "one", Atom.OwnOverMaking, null, NameKind.None),
		Landmark("taken text around an own text rule that calls a building one",  ":Word", "one", Atom.OwnOverMaking, null, NameKind.None),
		Landmark("taken text around a taken rule over a building one",            ":Word", "one", Atom.TakenOverMaking, null, NameKind.None),
		Landmark("taken span around a taken rule over a building one",            ":Sp", "one", Atom.TakenOverMaking, null, NameKind.None),
		Landmark("taken text around a rule with a building and a text route",     ":Word", "one", Atom.PerRoute, null, NameKind.None),
		Landmark("sequence of an own text rule over a building one and a built",  "@string[]", "two", Atom.OwnOverMaking, Atom.Built, NameKind.None),
		Landmark("sequence of a taken rule over a building one and a built",      "@string[]", "two", Atom.TakenOverMaking, Atom.Built, NameKind.None),
		Landmark("left-recursive taken text over a text operand",                 ":Word", RuleShapes.LeftRecursiveStructure, Atom.Implicit, null, NameKind.None),
		Landmark("left-recursive taken text over a building operand",             ":Word", RuleShapes.LeftRecursiveStructure, Atom.Built, null, NameKind.None),
		Landmark("left-recursive taken span over an extent operand",              ":Span", RuleShapes.LeftRecursiveStructure, Atom.Implicit, null, NameKind.None),
		Landmark("left-recursive taken span over a building operand",             ":Sp", RuleShapes.LeftRecursiveStructure, Atom.Built, null, NameKind.None),
		Landmark("left-recursive own text over a building operand",               "@string", RuleShapes.LeftRecursiveStructure, Atom.Built, null, NameKind.None),
		Landmark("left-recursive own span over a building operand",               "@SourceSpan", RuleShapes.LeftRecursiveStructure, Atom.Built, null, NameKind.None),
	];

	static (string, RuleShape) Landmark(string label, string type, string structure, Atom? first, Atom? second, NameKind names)
	{
		return (label, RuleShapes.Make("landmark", RuleType.Named(type), structure, first, second, names)
			?? throw new InvalidOperationException("no such shape: " + label));
	}

	[Fact]
	public void The_gate_compiles_every_shape_with_both_generators()
	{
		var setting = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES");
		var filter  = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_FILTER");
		var seed    = Number("DOTGRAM_RULEVALUES_SEED", 20261125);
		var shapes  = BuildShapes(setting, filter, seed, Number("DOTGRAM_RULEVALUES_RANDOM", 2000));

		var output = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_OUT") ?? Path.Combine(Path.GetTempPath(), "dotgram-rulevalues");

		Directory.CreateDirectory(output);

		var jobs = new List<Job>();

		foreach (var shape in shapes)
		{
			var printed = RulePrinter.Print(shape);

			foreach (var mode in Enum.GetValues<Mode>())
				jobs.Add(new Job(shape, printed, mode, "G" + jobs.Count.ToString("D6", CultureInfo.InvariantCulture)));
		}

		// Jobs by their class number, for going back to a batch: "19136-19199,4480-4543".
		if (Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_JOBS") is { } ranges)
		{
			var keep = ranges.Split(',').Select(static r => r.Split('-')).Select(static r => (From: int.Parse(r[0], CultureInfo.InvariantCulture), To: int.Parse(r[1], CultureInfo.InvariantCulture))).ToList();

			jobs.RemoveAll(j => !keep.Any(r => int.Parse(j.ClassName.AsSpan(1), CultureInfo.InvariantCulture) is var n && n >= r.From && n <= r.To));
		}

		var size     = Number("DOTGRAM_RULEVALUES_BATCH", 64);
		var parallel = Number("DOTGRAM_RULEVALUES_PARALLEL", Math.Max(1, Environment.ProcessorCount / 2));
		var batches  = jobs.Chunk(size).ToList();
		var watch    = Stopwatch.StartNew();
		var package  = GeneratorSide.PackageSide.Value;
		var failures = new ConcurrentBag<string>();

		// What is running, so that a run the process does not survive (a parser that overflows its stack) says where.
		using var progress = new StreamWriter(Path.Combine(output, "progress.log")) { AutoFlush = true };

		void Log(string line)
		{
			lock (progress)
				progress.WriteLine(line);
		}

		RuleRunner.Trace = Log;

		Parallel.ForEach(batches, new ParallelOptions { MaxDegreeOfParallelism = parallel }, batch =>
		{
			try
			{
				Log("start " + batch[0].ClassName + " " + batch[0].Shape.Id);
				RuleRunner.Run(batch, package);
				Log("package " + batch[0].ClassName);
				RuleRunner.Run(batch, GeneratorSide.HeadSide);
				Log("done " + batch[0].ClassName);
			}
			catch (Exception e)
			{
				failures.Add(batch[0].ClassName + ": " + e);
			}
		});

		watch.Stop();

		Write(output, jobs, seed, watch.Elapsed, batches.Count);

		Assert.Empty(failures);
		Assert.All(jobs, static j => Assert.True(j.Package.Length == j.Printed.Routes.Length && j.Head.Length == j.Printed.Routes.Length, j.Shape.Id + " " + j.Mode));
		Assert.All(jobs, static j => Assert.NotEmpty(j.Printed.Routes));
	}

	/// <summary>Whether the operands a shape reads are text rules, making rules, both, or none.</summary>
	static string OperandKind(IReadOnlyList<Helper> operands)
	{
		var text   = operands.Any(static h => h.Kind == HelperKind.Text);
		var making = operands.Any(static h => h.Kind == HelperKind.Making);

		return (text, making) switch
		{
			(true, true)  => "both",
			(true, false) => "text",
			(false, true) => "making",
			_             => "none",
		};
	}

	/// <summary>An outcome as three tab-separated fields, with no tab or line break in them.</summary>
	static string Field(Outcome outcome)
	{
		return string.Join('\t', outcome.Kind, Clean(outcome.Text), outcome.Warnings);
	}

	static string Clean(string text)
	{
		return text.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
	}

	/// <summary>
	/// The shapes a run covers: the landmarks and the fixed grammars always; with <c>full</c> the bounded enumeration and
	/// the random tier, with a number that many shapes of the enumeration; and each shape that calls a text rule once more
	/// with the rule's body written in place.
	/// </summary>
	internal static List<RuleShape> BuildShapes(string? setting, string? filter, int seed, int randomCount)
	{
		var shapes = new List<RuleShape>();

		shapes.AddRange(Landmarks.Select(static l => l.Shape));
		shapes.AddRange(RuleShapes.Fixed());

		if (setting == "full")
		{
			shapes.AddRange(RuleShapes.Bounded());
			shapes.AddRange(RuleShapes.Random(seed, randomCount));
		}
		else if (int.TryParse(setting, NumberStyles.None, CultureInfo.InvariantCulture, out var sample))
		{
			var all  = RuleShapes.Bounded();
			var step = Math.Max(1, all.Count / Math.Max(1, sample));

			shapes.AddRange(all.Where((_, i) => i % step == 0).Take(sample));
		}

		// Each shape that calls a text rule, once more with the rule's body written in place.
		shapes.AddRange(shapes.Select(RuleShapes.Inlined).OfType<RuleShape>().ToList());

		if (filter is not null)
			shapes.RemoveAll(s => !s.Id.Contains(filter, StringComparison.Ordinal));

		return shapes;
	}

	static int Number(string name, int otherwise)
	{
		return int.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : otherwise;
	}

	/// <summary>
	/// Writes <c>cells.tsv</c> (one line per shape, mode and input), <c>grammars.txt</c> (each shape's grammar as printed
	/// in mode D, with its inputs) and <c>summary.txt</c> (counts by class, by class and type, by class and mode, and the
	/// landmarks).
	/// </summary>
	static void Write(string output, List<Job> jobs, int seed, TimeSpan elapsed, int batches)
	{
		var cells   = new StringBuilder();
		var counts  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var byType  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var byMode  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var byTier  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var byHelp  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var byKind  = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var newWarn = 0;

		void Count(SortedDictionary<string, int> into, string key)
		{
			into[key] = into.GetValueOrDefault(key) + 1;
		}

		var outcomes = new StringBuilder();

		cells.Append("id\tmode\tinput\tclass\tv0.3.0\thead\tpair\n");
		outcomes.Append("id\tmode\tinput\tpackage kind\tpackage text\tpackage warnings\thead kind\thead text\thead warnings\n");

		foreach (var job in jobs)
		{
			var operands = RuleShapes.Operands(job.Shape);
			var kind     = OperandKind(operands);

			for (var r = 0; r < job.Printed.Routes.Length; r++)
			{
				var difference = RuleDifference.Classify(job.Package[r], job.Head[r]);

				Count(byKind, difference + "\t" + kind);

				foreach (var helper in operands.DefaultIfEmpty())
					Count(byHelp, difference + "\t" + (helper is null ? "(none)" : helper.Name + " (" + helper.Kind.ToString().ToLowerInvariant() + ")"));

				Count(counts, difference);
				Count(byType, difference + "\t" + job.Shape.Type.Name);
				Count(byMode, difference + "\t" + job.Mode);
				Count(byTier, difference + "\t" + job.Shape.Tier);

				if (RuleDifference.HeadWarnsNew(job.Package[r], job.Head[r]))
					newWarn++;

				cells.Append(job.Shape.Id).Append('\t').Append(job.Mode).Append('\t').Append(job.Printed.Routes[r].Input).Append('\t')
					.Append(difference).Append('\t').Append(job.Package[r]).Append('\t').Append(job.Head[r]).Append('\t')
					.Append(job.Shape.InlinedFrom?.Id).Append('\n');

				outcomes.Append(job.Shape.Id).Append('\t').Append(job.Mode).Append('\t').Append(job.Printed.Routes[r].Input).Append('\t')
					.Append(Field(job.Package[r])).Append('\t').Append(Field(job.Head[r])).Append('\n');
			}
		}

		File.WriteAllText(Path.Combine(output, "cells.tsv"), cells.ToString());
		File.WriteAllText(Path.Combine(output, "outcomes.tsv"), outcomes.ToString());

		var grammars = new StringBuilder();

		foreach (var job in jobs.Where(static j => j.Mode == Mode.D))
		{
			grammars.Append("## ").Append(job.Shape.Id).Append('\n').Append(job.Printed.Rules);
			grammars.Append("inputs: ").Append(string.Join(" | ", job.Printed.Routes.Select(static r => r.Input))).Append("\n\n");
		}

		File.WriteAllText(Path.Combine(output, "grammars.txt"), grammars.ToString());

		var summary = new StringBuilder();

		summary.Append(CultureInfo.InvariantCulture, $"shapes {jobs.Select(static j => j.Shape).Distinct().Count()}, jobs {jobs.Count}, cells {counts.Values.Sum()}, batches {batches}, ");
		summary.Append(CultureInfo.InvariantCulture, $"seed {seed}, elapsed {elapsed.TotalSeconds:F0} s, ");
		summary.Append(CultureInfo.InvariantCulture, $"whole-batch crashes {RuleRunner.WholeBatchCrashes}, unattributed errors {RuleRunner.Unattributed}, head warns new {newWarn}\n");
		summary.Append(CultureInfo.InvariantCulture, $"the bounded enumeration: core {RuleShapes.Core().Count()}, deviations {RuleShapes.Deviations().Count()}, atoms {RuleShapes.Atoms().Count()}, left recursion {RuleShapes.LeftRecursion().Count()}, ");
		summary.Append(CultureInfo.InvariantCulture, $"extraction pairs in this run {jobs.Where(static j => j.Mode == Mode.D && j.Shape.InlinedFrom is not null).Count()}\n\n");

		foreach (var difference in RuleDifference.All)
			summary.Append(difference).Append('\t').Append(counts.GetValueOrDefault(difference)).Append('\n');

		summary.Append("\nby mode\n");

		foreach (var (key, value) in byMode)
			summary.Append(key).Append('\t').Append(value).Append('\n');

		summary.Append("\nby tier\n");

		foreach (var (key, value) in byTier)
			summary.Append(key).Append('\t').Append(value).Append('\n');

		summary.Append("\nby operand kind (the helpers a shape reads as operands: text, making, both, or none)\n");

		foreach (var (key, value) in byKind)
			summary.Append(key).Append('\t').Append(value).Append('\n');

		summary.Append("\nby operand helper (a cell counts once for each helper it reads)\n");

		foreach (var (key, value) in byHelp)
			summary.Append(key).Append('\t').Append(value).Append('\n');

		summary.Append("\nby type\n");

		foreach (var (key, value) in byType)
			summary.Append(key).Append('\t').Append(value).Append('\n');

		summary.Append("\nlandmarks (mode D)\n");

		foreach (var (label, shape) in Landmarks)
		{
			var job = jobs.FirstOrDefault(j => j.Shape == shape && j.Mode == Mode.D);

			if (job is null)
				continue;

			for (var r = 0; r < job.Printed.Routes.Length; r++)
				summary.Append(label).Append('\t').Append(job.Printed.Routes[r].Input).Append('\t')
					.Append(RuleDifference.Classify(job.Package[r], job.Head[r])).Append('\t')
					.Append(job.Package[r]).Append("\t->\t").Append(job.Head[r]).Append('\n');

			summary.Append('\t').Append(job.Printed.Rules.Replace("\n", "\n\t", StringComparison.Ordinal)).Append('\n');
		}

		File.WriteAllText(Path.Combine(output, "summary.txt"), summary.ToString());
		File.WriteAllLines(Path.Combine(output, "warnings.txt"), RuleRunner.WarningMessages.OrderBy(static p => p.Key, StringComparer.Ordinal).Select(static p => p.Key + "\t" + p.Value));
	}
}
