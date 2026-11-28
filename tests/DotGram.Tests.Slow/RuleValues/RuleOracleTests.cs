using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Xunit;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// The oracle held to the published version, and this build held to the oracle, over the cells a run of the gate wrote
/// (<c>outcomes.tsv</c>): nothing is compiled here, so that the oracle can be changed and read again in seconds.
/// </summary>
/// <remarks>
/// Environment: <c>DOTGRAM_RULEVALUES_OUTCOMES</c> is the <c>outcomes.tsv</c> files, separated by semicolons, and
/// <c>DOTGRAM_RULEVALUES</c>, <c>DOTGRAM_RULEVALUES_SEED</c> and <c>DOTGRAM_RULEVALUES_RANDOM</c> are the ones of the run
/// that wrote them (so that the same shapes are enumerated). The reports are written beside the first file.
/// </remarks>
public sealed class RuleOracleTests
{
	static (List<RuleCell> Cells, Dictionary<string, RuleShape> Shapes, int Missing, string Directory) Load()
	{
		var paths = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_OUTCOMES");

		if (string.IsNullOrEmpty(paths))
			Assert.Skip("DOTGRAM_RULEVALUES_OUTCOMES names no file: run the gate first.");

		var files = paths.Split(';', StringSplitOptions.RemoveEmptyEntries);
		var rows  = new Dictionary<string, (Outcome Package, Outcome Head)>(StringComparer.Ordinal);

		foreach (var file in files)
			foreach (var line in File.ReadLines(file).Skip(1))
			{
				var f = line.Split('\t');

				if (f.Length >= 9)
					rows[f[0] + "\t" + f[1] + "\t" + f[2]] = (new Outcome(f[3], f[4], f[5]), new Outcome(f[6], f[7], f[8]));
			}

		var seed   = int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_SEED"), NumberStyles.None, CultureInfo.InvariantCulture, out var s) ? s : 20261125;
		var random = int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_RANDOM"), NumberStyles.None, CultureInfo.InvariantCulture, out var r) ? r : 2000;
		var shapes = RuleValueGateTests.BuildShapes(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES"), null, seed, random);
		var cells  = new List<RuleCell>();
		var missing = 0;

		foreach (var shape in shapes)
			foreach (var route in RulePrinter.Print(shape).Routes)
				foreach (var mode in Enum.GetValues<Mode>())
					if (rows.TryGetValue(shape.Id + "\t" + mode + "\t" + route.Input, out var outcome))
						cells.Add(new RuleCell(shape, mode, route.Input, outcome.Package, outcome.Head));
					else
						missing++;

		return (cells, shapes.GroupBy(static x => x.Id).ToDictionary(static g => g.Key, static g => g.First()), missing, Path.GetDirectoryName(files[0])!);
	}

	/// <summary>The rows of the rule's own table, as the oracle reads them: the input's letters are put into the value.</summary>
	[Theory]
	[InlineData("string, a built string between literals", 0, "\"<W:{0}>\"")]
	[InlineData("taken text, a built operand between literals", 0, "\"<W:{0}>\"")]
	[InlineData("taken text, a built operand and a literal", 0, "\"W:{0};\"")]
	[InlineData("string, a built string in a recursive rule", 0, "\"(W:{0})\"")]
	[InlineData("string, an optional literal and a text rule", 0, "\"-{0}\"")]
	[InlineData("string, a char rule between quotes", 0, "\"\"\\n\"\"")]
	[InlineData("string, an untyped wrapper of a built string", 0, "\"<(W:{0})>\"")]
	[InlineData("string, a template call over a built string", 0, "\"<(W:{0})>\"")]
	[InlineData("string, a binding and its reference", 0, "\"W:{0},{0}\"")]
	public void The_oracle_reads_the_table_of_the_rule(string label, int route, string pattern)
	{
		var shape = RuleShapes.Fixed().FirstOrDefault(f => f.Id == "fixed/" + label);

		Assert.NotNull(shape);

		var input = RulePrinter.Print(shape!).Routes[route].Input;
		var word  = input.Where(char.IsLetter).Take(2).ToArray();
		var value = RuleOracle.Expected(shape!, Mode.D, input);

		Assert.True(value.IsValue, label + ": " + value);
		Assert.Equal(string.Format(CultureInfo.InvariantCulture, pattern, new string(word)), value.Value);
	}

	[Fact]
	public void The_oracle_joins_an_array_and_a_string_by_turns()
	{
		string Value(string label, int route)
		{
			var shape = RuleShapes.Fixed().First(f => f.Id == "fixed/" + label);

			return RuleOracle.Expected(shape, Mode.D, RulePrinter.Print(shape).Routes[route].Input).Value!;
		}

		var left  = RulePrinter.Print(RuleShapes.Fixed().First(f => f.Id == "fixed/left recursion, array")).Routes[1].Input.Split(',');
		var right = RulePrinter.Print(RuleShapes.Fixed().First(f => f.Id == "fixed/right recursion, array")).Routes[0].Input.Split(',');

		Assert.Equal("[\"W:" + left[0] + "\",\"W:" + left[1] + "\"]", Value("left recursion, array", 1));
		Assert.Equal("[\"W:" + right[0] + "\",\"W:" + right[1] + "\"]", Value("right recursion, array", 0));
		Assert.Equal("[0..5)", Value("left recursion, span", 1));
		Assert.StartsWith("Pair{A=", Value("left recursion, a type with a constructing seed", 0));

		var spaced = RuleShapes.Fixed().First(f => f.Id == "fixed/trivia, a string rule over two text rules");
		var text   = RulePrinter.Print(spaced).Routes[0].Input;

		Assert.Equal("\"" + text.Replace(" ", "", StringComparison.Ordinal) + "\"", Value("trivia, a string rule over two text rules", 0));
	}

	[Fact]
	public void The_oracle_agrees_with_the_published_version_where_the_rule_says_the_same()
	{
		var (cells, shapes, missing, directory) = Load();
		var held = RuleOracleCheck.HoldPublished(cells, shapes.Count, missing);

		File.WriteAllText(Path.Combine(directory, "oracle-checkpoint.txt"), held.Report);

		Assert.Equal(0, held.Stops);
		Assert.Equal(0, held.Unexplained);

		// An entry that covers nothing is dead only where the whole enumeration was run.
		if (Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES") == "full")
			Assert.Empty(held.Dead);
	}

	[Fact]
	public void This_build_is_held_to_the_oracle()
	{
		var (cells, _, _, directory) = Load();
		var held = RuleOracleCheck.HoldHead(cells);

		File.WriteAllText(Path.Combine(directory, "oracle-head.txt"), held.Report);

		Assert.Equal(0, held.Red);
	}

	[Fact]
	public void The_invariants_hold_for_the_oracle_and_are_counted_for_both_versions()
	{
		var (cells, shapes, _, directory) = Load();

		var report = new StringBuilder();
		var table  = cells.ToDictionary(static c => c.Shape.Id + "\t" + c.Mode + "\t" + c.Input, static c => c);

		Observer ObserveOutcome(Func<RuleCell, Outcome> pick)
		{
			return (shape, mode, input) =>
			{
				if (!table.TryGetValue(shape.Id + "\t" + mode + "\t" + input, out var cell))
					return null;

				var outcome = pick(cell);

				return outcome.Kind switch
				{
					"value" => outcome.Text,
					"fail"  => "NO MATCH",
					_       => "REFUSED",
				};
			};
		}

		Observer observeOracle = (shape, mode, input) =>
		{
			var expected = RuleOracle.Expected(shape, mode, input);

			return expected.IsValue ? expected.Value : expected.IsError ? "REFUSED" : expected.Source == Source.NoMatch ? "NO MATCH" : null;
		};

		var oracleViolations = 0;

		foreach (var (name, observe) in new (string, Observer)[] { ("oracle", observeOracle), ("published version 0.3.0", ObserveOutcome(static c => c.Package)), ("this build", ObserveOutcome(static c => c.Head)) })
		{
			var all = new List<RuleInvariants.Violation>();

			all.AddRange(RuleInvariants.TakenIsDeclared(shapes, observe));
			all.AddRange(RuleInvariants.ExtractionKeepsTheValue(shapes, observe));
			all.AddRange(RuleInvariants.NamesAndReadersChangeNothing(shapes, observe));

			if (name == "oracle")
			{
				all.AddRange(RuleInvariants.PureRuleIsItsSlice(shapes));
				all.AddRange(RuleInvariants.SourceIsTheSameInEveryMode(shapes));
				oracleViolations = all.Count;
			}

			report.Append(CultureInfo.InvariantCulture, $"\n===== {name}: {all.Count} violations =====\n");

			foreach (var group in all.GroupBy(static v => v.Invariant).OrderBy(static g => g.Key, StringComparer.Ordinal))
			{
				report.Append(CultureInfo.InvariantCulture, $"\n{group.Count(),8} {group.Key} ({group.Select(static v => v.Shape).Distinct().Count()} shapes)\n");

				foreach (var v in group.GroupBy(static v => v.Shape).Select(static g => g.First()).Take(4))
				{
					report.Append("    ").Append(v.Shape).Append(" / ").Append(v.Other).Append(" mode ").Append(v.Mode).Append(" input ").Append(v.Input).Append('\n');
					report.Append("    ").Append(RuleOracleCheck.Printed(shapes[v.Shape])).Append('\n');
					report.Append("      ").Append(v.A).Append("  vs  ").Append(v.B).Append('\n');
				}
			}
		}

		File.WriteAllText(Path.Combine(directory, "oracle-invariants.txt"), report.ToString());

		// What holds of the oracle holds by its construction; a violation is a defect of the oracle.
		Assert.Equal(0, oracleViolations);
	}
}
