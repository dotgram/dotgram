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
	sealed record Cell(RuleShape Shape, Mode Mode, string Input, Outcome Package, Outcome Head);

	static (List<Cell> Cells, Dictionary<string, RuleShape> Shapes, int Missing, string Directory) Load()
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
		var cells  = new List<Cell>();
		var missing = 0;

		foreach (var shape in shapes)
			foreach (var route in RulePrinter.Print(shape).Routes)
				foreach (var mode in Enum.GetValues<Mode>())
					if (rows.TryGetValue(shape.Id + "\t" + mode + "\t" + route.Input, out var outcome))
						cells.Add(new Cell(shape, mode, route.Input, outcome.Package, outcome.Head));
					else
						missing++;

		return (cells, shapes.GroupBy(static x => x.Id).ToDictionary(static g => g.Key, static g => g.First()), missing, Path.GetDirectoryName(files[0])!);
	}

	static string Show(Outcome outcome)
	{
		var text = outcome.ToString();

		return text.Length > 110 ? text.Substring(0, 110) + "…" : text;
	}

	static string Printed(RuleShape shape)
	{
		return RulePrinter.Print(shape).Rules.TrimEnd('\n').Replace("\n", "\n    ", StringComparison.Ordinal);
	}

	static void Example(StringBuilder report, Cell cell, string older, Expectation expected, string label)
	{
		report.Append("    ").Append(cell.Shape.Id).Append(" mode ").Append(cell.Mode).Append(" input ").Append(cell.Input).Append('\n');
		report.Append("    ").Append(Printed(cell.Shape)).Append('\n');
		report.Append("      ").Append(label).Append(older).Append('\n');
		report.Append("      oracle: ").Append(expected).Append('\n');
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

		var report    = new StringBuilder();
		var counts    = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var matched   = RuleAllowList.Entries.ToDictionary(static e => e.Name, static _ => new List<Cell>());
		var agreeing  = RuleAllowList.Entries.ToDictionary(static e => e.Name, static _ => 0);
		var unexplained = new List<(Cell Cell, Expectation Expected)>();
		var unmatched = new List<(Cell Cell, Expectation Expected)>();
		var undecided = new SortedDictionary<string, (int Count, Cell Example)>(StringComparer.Ordinal);

		report.Append(CultureInfo.InvariantCulture, $"cells {cells.Count}, shapes {shapes.Count}, cells with no outcome in the files {missing}\n");

		foreach (var cell in cells)
		{
			var expected = RuleOracle.Expected(cell.Shape, cell.Mode, cell.Input);
			var old      = cell.Package;
			string category;

			if (expected.IsUnknown)
			{
				category = "undecided";
				undecided[expected.Question!] = undecided.TryGetValue(expected.Question!, out var seen) ? (seen.Count + 1, seen.Example) : (1, cell);
			}
			else if (expected.Source == Source.SamePublished)
			{
				category = "same as 0.3.0 by decision (an untyped rule that writes names)";
			}
			else if (expected.Source == Source.NoMatch)
			{
				category = old.Kind == "fail" ? "agree" : "oracle reads no match, 0.3.0 " + old.Kind;

				if (old.Kind == "value")
					unmatched.Add((cell, expected));
			}
			else if (old.Kind == "value" && expected.IsValue && expected.Value == old.Text)
			{
				category = "agree";

				foreach (var entry in RuleAllowList.Entries.Where(e => e.Matches(new AllowCase(cell.Shape, cell.Mode, old, Newer.Of(expected), RuleOracle.Describe(cell.Shape)) { AsIfDifferent = true })))
					agreeing[entry.Name]++;
			}
			else if (old.Kind == "value")
			{
				var covering = RuleAllowList.Covering(new AllowCase(cell.Shape, cell.Mode, old, Newer.Of(expected), RuleOracle.Describe(cell.Shape))).ToList();

				foreach (var entry in covering)
					matched[entry.Name].Add(cell);

				if (covering.Count == 0)
				{
					category = "UNEXPLAINED";
					unexplained.Add((cell, expected));
				}
				else
				{
					category = "allowed: " + covering[0].Name;
				}
			}
			else if (old.Kind == "fail")
			{
				category = "published version did not match the input";
			}
			else
			{
				category = old.Built ? "other" : expected.IsValue ? "published version refused, oracle builds" : "refused by both";
			}

			counts[category] = counts.GetValueOrDefault(category) + 1;
		}

		report.Append("\ncells by what the oracle says against 0.3.0\n");

		foreach (var (key, value) in counts)
			report.Append(CultureInfo.InvariantCulture, $"{value,8}  {key}\n");

		report.Append("\nthe allow-list, silent classes first: cells (and shapes) where 0.3.0 differs from the oracle on purpose\n");

		foreach (var entry in RuleAllowList.Entries.OrderBy(static e => e.Impact))
			report.Append(CultureInfo.InvariantCulture, $"{matched[entry.Name].Count,8} differ ({matched[entry.Name].Select(static c => c.Shape.Id).Distinct().Count(),5} shapes), {agreeing[entry.Name],6} agree  {entry.Impact,-8}  {entry.Name}\n");

		var dead = matched.Where(static p => p.Value.Count == 0).Select(static p => p.Key).ToList();

		report.Append(CultureInfo.InvariantCulture, $"\nunexplained: {unexplained.Count} cells in {unexplained.Select(static u => u.Cell.Shape.Id).Distinct().Count()} distinct shapes\n");

		foreach (var family in unexplained
			.GroupBy(static u => u.Cell.Shape.Type.Name + " / " + u.Cell.Shape.Structure + " / " + u.Cell.Shape.Tier + " / " + u.Expected.Source)
			.OrderBy(static g => g.Key, StringComparer.Ordinal))
		{
			report.Append(CultureInfo.InvariantCulture, $"\n  family {family.Key}: {family.Select(static u => u.Cell.Shape.Id).Distinct().Count()} shapes, {family.Count()} cells\n");

			foreach (var (cell, expected) in family.GroupBy(static u => u.Cell.Shape.Id).Select(static g => g.First()).Take(1))
				Example(report, cell, Show(cell.Package), expected, "0.3.0:  ");
		}

		report.Append("\nthe oracle reads no match where 0.3.0 gave a value, one per shape\n");

		foreach (var (cell, expected) in unmatched.GroupBy(static u => u.Cell.Shape.Id).Select(static g => g.First()))
			Example(report, cell, Show(cell.Package), expected, "0.3.0:  ");

		report.Append("\nundecided: the rule gives no answer\n");

		foreach (var (question, (count, example)) in undecided)
		{
			report.Append(CultureInfo.InvariantCulture, $"\n  {count} cells: {question}\n");
			report.Append("    e.g. ").Append(example.Shape.Id).Append(" mode ").Append(example.Mode).Append(" input ").Append(example.Input).Append('\n');
			report.Append("    ").Append(Printed(example.Shape)).Append('\n');
		}

		report.Append("\nallowed cells, one example per class\n");

		foreach (var entry in RuleAllowList.Entries.OrderBy(static e => e.Impact))
			if (matched[entry.Name].FirstOrDefault() is { } example)
			{
				report.Append("\n  ").Append(entry.Name).Append(": ").Append(entry.Description).Append('\n');
				Example(report, example, Show(example.Package), RuleOracle.Expected(example.Shape, example.Mode, example.Input), "0.3.0:  ");
			}

		File.WriteAllText(Path.Combine(directory, "oracle-checkpoint.txt"), report.ToString());

		// An entry that covers nothing fails the run.
		Assert.Empty(dead);
	}

	[Fact]
	public void This_build_is_held_to_the_oracle()
	{
		var (cells, _, _, directory) = Load();

		var report = new StringBuilder();
		var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var red    = new SortedDictionary<string, List<(Cell Cell, Expectation Expected)>>(StringComparer.Ordinal);

		foreach (var cell in cells)
		{
			var expected = RuleOracle.Expected(cell.Shape, cell.Mode, cell.Input);

			if (expected.IsUnknown)
			{
				counts["no answer expected"] = counts.GetValueOrDefault("no answer expected") + 1;
				continue;
			}

			var head = cell.Head;

			// An untyped rule that writes names is held to what the published version gives, whatever that is.
			if (expected.Source == Source.SamePublished)
			{
				if (head.Kind == cell.Package.Kind && head.Text == cell.Package.Text)
				{
					counts["green"] = counts.GetValueOrDefault("green") + 1;
					continue;
				}

				counts["red"] = counts.GetValueOrDefault("red") + 1;

				var held = "other, rule O: held to 0.3.0 (untyped rule with names), head " + (head.Kind == "value" ? "gave a different value" : head.Kind);

				if (!red.TryGetValue(held, out var heldList))
					red[held] = heldList = [];

				heldList.Add((cell, expected));
				continue;
			}

			if (expected.Source == Source.NoMatch ? !head.Built || head.Kind == "fail" : expected.IsValue ? head.Kind == "value" && head.Text == expected.Value : !head.Built)
			{
				counts["green"] = counts.GetValueOrDefault("green") + 1;
				continue;
			}

			counts["red"] = counts.GetValueOrDefault("red") + 1;

			// The class of a red cell: the entry of the allow-list whose grammars it is, or the rule that decides it.
			var covering = RuleAllowList.Covering(new AllowCase(cell.Shape, cell.Mode, head, Newer.Of(expected), RuleOracle.Describe(cell.Shape)) { AsIfDifferent = true }).FirstOrDefault();
			var key = (covering is null ? "other, rule " + expected.Row : covering.Impact.ToString().ToLowerInvariant() + " " + covering.Name)
				+ ": expected " + (expected.IsError ? "error " + expected.ErrorId : expected.Source.ToString())
				+ ", head " + (head.Kind == "value" ? "gave a different value" : head.Kind);

			if (!red.TryGetValue(key, out var list))
				red[key] = list = [];

			list.Add((cell, expected));
		}

		foreach (var (key, value) in counts)
			report.Append(CultureInfo.InvariantCulture, $"{value,8}  {key}\n");

		report.Append("\nred cells by class (silent classes first), and what is expected and what this build gave\n");

		foreach (var (key, list) in red.OrderBy(static p => p.Key.StartsWith("silent", StringComparison.Ordinal) ? 0 : p.Key.StartsWith("loud", StringComparison.Ordinal) ? 1 : p.Key.StartsWith("warnonly", StringComparison.Ordinal) ? 2 : 3).ThenByDescending(static p => p.Value.Count))
		{
			report.Append(CultureInfo.InvariantCulture, $"\n{list.Count,8} cells in {list.Select(static x => x.Cell.Shape.Id).Distinct().Count()} shapes: {key}\n");

			foreach (var (cell, expected) in list.GroupBy(static x => x.Cell.Shape.Id).Select(static g => g.First()).Take(2))
				Example(report, cell, Show(cell.Head), expected, "head:   ");
		}

		File.WriteAllText(Path.Combine(directory, "oracle-head.txt"), report.ToString());

		// This build is not yet held to the oracle: the reading of the rule it implements is the one being replaced.
		Assert.SkipUnless(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_ORACLE_HEAD") == "1", "the head is held to the oracle only on request; see oracle-head.txt");
		Assert.Equal(0, counts.GetValueOrDefault("red"));
	}

	[Fact]
	public void The_invariants_hold_for_the_oracle_and_are_counted_for_both_versions()
	{
		var (cells, shapes, _, directory) = Load();

		var report = new StringBuilder();
		var table  = cells.ToDictionary(static c => c.Shape.Id + "\t" + c.Mode + "\t" + c.Input, static c => c);

		Observer ObserveOutcome(Func<Cell, Outcome> pick)
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
					report.Append("    ").Append(Printed(shapes[v.Shape])).Append('\n');
					report.Append("      ").Append(v.A).Append("  vs  ").Append(v.B).Append('\n');
				}
			}
		}

		File.WriteAllText(Path.Combine(directory, "oracle-invariants.txt"), report.ToString());

		// What holds of the oracle holds by its construction; a violation is a defect of the oracle.
		Assert.Equal(0, oracleViolations);
	}
}
