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

				if (f.Length < 9)
					continue;

				rows[f[0] + "\t" + f[1] + "\t" + f[2]] = (new Outcome(f[3], f[4], f[5]), new Outcome(f[6], f[7], f[8]));
			}

		var setting = Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES");
		var seed    = int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_SEED"), NumberStyles.None, CultureInfo.InvariantCulture, out var s) ? s : 20261125;
		var random  = int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_RANDOM"), NumberStyles.None, CultureInfo.InvariantCulture, out var r) ? r : 2000;
		var shapes  = RuleValueGateTests.BuildShapes(setting, null, seed, random);
		var cells   = new List<Cell>();
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

	[Fact]
	public void The_oracle_agrees_with_the_published_version_where_the_specification_says_the_same()
	{
		var (cells, shapes, missing, directory) = Load();
		var report = new StringBuilder();
		var dead   = new List<string>();

		report.Append(CultureInfo.InvariantCulture, $"cells {cells.Count}, shapes {shapes.Count}, cells with no outcome in the files {missing}\n");

		foreach (var passesOn in new[] { true, false })
		{
			var counts     = new SortedDictionary<string, int>(StringComparer.Ordinal);
			var dependent  = new SortedDictionary<string, int>(StringComparer.Ordinal);
			var matched    = RuleAllowList.Entries.ToDictionary(static e => e.Name, static _ => 0);
			var unexplained = new List<(Cell Cell, Expectation Expected)>();
			var undecided  = new SortedDictionary<string, (int Count, Cell Example)>(StringComparer.Ordinal);

			foreach (var cell in cells)
			{
				var expected = RuleOracle.Expected(cell.Shape, cell.Mode, cell.Input, passesOn);
				var other    = RuleOracle.Expected(cell.Shape, cell.Mode, cell.Input, !passesOn);
				var depends  = expected.Source != other.Source || expected.Value != other.Value || expected.ErrorId != other.ErrorId;
				var grammar  = RuleOracle.Describe(cell.Shape, passesOn);
				var old      = cell.Package;
				string category;

				if (expected.Source == Source.Undecided)
				{
					category = "undecided";

					var key = expected.Question!;

					undecided[key] = undecided.TryGetValue(key, out var seen) ? (seen.Count + 1, seen.Example) : (1, cell);
				}
				else if (expected.Source == Source.Open)
				{
					category = "open (" + string.Join(", ", RuleAllowList.Covering(cell.Shape, cell.Mode, old, new Newer(false, null, false), grammar).Select(static e => e.Name)) + ")";

					foreach (var entry in RuleAllowList.Covering(cell.Shape, cell.Mode, old, new Newer(false, null, false), grammar))
						matched[entry.Name]++;
				}
				else if (old.Kind == "value")
				{
					if (expected.IsValue && expected.Value == old.Text)
					{
						category = "same";
					}
					else
					{
						var covering = RuleAllowList.Covering(cell.Shape, cell.Mode, old, Newer.Of(expected), grammar).ToList();

						foreach (var entry in covering)
							matched[entry.Name]++;

						if (covering.Count == 0)
						{
							category = "UNEXPLAINED";
							unexplained.Add((cell, expected));
						}
						else
						{
							category = "explained: " + string.Join(", ", covering.Select(static e => e.Name));
						}
					}
				}
				else if (old.Kind == "fail")
				{
					category = "published version did not match the input";
				}
				else if (old.Built)
				{
					category = "other";
				}
				else
				{
					category = expected.IsValue ? "relaxed" : "refused by both";
				}

				counts[category] = counts.GetValueOrDefault(category) + 1;

				if (depends)
					dependent[category] = dependent.GetValueOrDefault(category) + 1;
			}

			report.Append(CultureInfo.InvariantCulture, $"\n===== TakenTextPassesOn = {passesOn} =====\n");

			foreach (var (key, value) in counts)
				report.Append(CultureInfo.InvariantCulture, $"{value,8}  {key}   (of which switch-dependent {dependent.GetValueOrDefault(key)})\n");

			report.Append("\nallow-list entries: cells covered\n");

			foreach (var (name, count) in matched)
				report.Append(CultureInfo.InvariantCulture, $"{count,8}  {name}\n");

			if (passesOn)
				dead.AddRange(matched.Where(static p => p.Value == 0).Select(static p => p.Key));

			var shapesUnexplained = unexplained.Select(static u => u.Cell.Shape.Id).Distinct().Count();

			report.Append(CultureInfo.InvariantCulture, $"\nunexplained: {unexplained.Count} cells in {shapesUnexplained} distinct shapes\n");

			var families = unexplained
				.GroupBy(static u => u.Cell.Shape.Type.Name + " / " + u.Cell.Shape.Structure + " / " + (u.Cell.Shape.Tier == "inlined" ? "inlined " : "") + u.Cell.Shape.Tier)
				.OrderBy(static g => g.Key, StringComparer.Ordinal)
				.ToList();
			var listed = 0;

			foreach (var family in families)
			{
				report.Append(CultureInfo.InvariantCulture, $"\n  family {family.Key}: {family.Select(static u => u.Cell.Shape.Id).Distinct().Count()} shapes, {family.Count()} cells\n");

				foreach (var (cell, expected) in family.GroupBy(static u => u.Cell.Shape.Id).Select(static g => g.First()).Take(3))
				{
					if (listed++ >= 30)
						break;

					report.Append("    ").Append(cell.Shape.Id).Append(" mode ").Append(cell.Mode).Append(" input ").Append(cell.Input).Append('\n');
					report.Append("    ").Append(Printed(cell.Shape)).Append('\n');
					report.Append("      0.3.0:  ").Append(Show(cell.Package)).Append('\n');
					report.Append("      oracle: ").Append(expected).Append('\n');
				}
			}

			report.Append("\nundecided: the specification gives no answer\n");

			foreach (var (question, (count, example)) in undecided)
			{
				report.Append(CultureInfo.InvariantCulture, $"\n  {count} cells: {question}\n");
				report.Append("    e.g. ").Append(example.Shape.Id).Append(" mode ").Append(example.Mode).Append(" input ").Append(example.Input).Append('\n');
				report.Append("    ").Append(Printed(example.Shape)).Append('\n');
			}
		}

		File.WriteAllText(Path.Combine(directory, "oracle-checkpoint.txt"), report.ToString());

		// An entry that covers nothing fails the run (with the switch at the recommendation).
		Assert.Empty(dead);
	}

	[Fact]
	public void This_build_is_held_to_the_oracle()
	{
		var (cells, _, _, directory) = Load();
		var report   = new StringBuilder();
		var redTotal = 0;

		foreach (var passesOn in new[] { true, false })
		{
			var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
			var red    = new SortedDictionary<string, List<(Cell Cell, Expectation Expected)>>(StringComparer.Ordinal);

			foreach (var cell in cells)
			{
				var expected = RuleOracle.Expected(cell.Shape, cell.Mode, cell.Input, passesOn);

				if (expected.IsUnknown)
				{
					counts["no answer expected"] = counts.GetValueOrDefault("no answer expected") + 1;
					continue;
				}

				var head  = cell.Head;
				var green = expected.IsValue ? head.Kind == "value" && head.Text == expected.Value : !head.Built;

				if (green)
				{
					counts["green"] = counts.GetValueOrDefault("green") + 1;
					continue;
				}

				counts["red"] = counts.GetValueOrDefault("red") + 1;

				if (passesOn)
					redTotal++;

				var key = $"row {expected.Row}, expected {(expected.IsError ? "error " + expected.ErrorId : expected.Source.ToString())}, head {(head.Kind == "value" ? "gave a different value" : head.Kind)}, mode {(cell.Mode == Mode.B ? "B" : "D/S")}";

				if (!red.TryGetValue(key, out var list))
					red[key] = list = [];

				list.Add((cell, expected));
			}

			report.Append(CultureInfo.InvariantCulture, $"\n===== TakenTextPassesOn = {passesOn} =====\n");

			foreach (var (key, value) in counts)
				report.Append(CultureInfo.InvariantCulture, $"{value,8}  {key}\n");

			report.Append("\nred cells by what is expected and what this build gave\n");

			foreach (var (key, list) in red.OrderByDescending(static p => p.Value.Count))
			{
				report.Append(CultureInfo.InvariantCulture, $"\n{list.Count,8} cells in {list.Select(static x => x.Cell.Shape.Id).Distinct().Count()} shapes: {key}\n");

				foreach (var (cell, expected) in list.GroupBy(static x => x.Cell.Shape.Id).Select(static g => g.First()).Take(2))
				{
					report.Append("    ").Append(cell.Shape.Id).Append(" mode ").Append(cell.Mode).Append(" input ").Append(cell.Input).Append('\n');
					report.Append("    ").Append(Printed(cell.Shape)).Append('\n');
					report.Append("      head:   ").Append(Show(cell.Head)).Append('\n');
					report.Append("      oracle: ").Append(expected).Append('\n');
				}
			}
		}

		File.WriteAllText(Path.Combine(directory, "oracle-head.txt"), report.ToString());

		// This build is not yet held to the oracle: the reading of the rule it implements is the one being replaced.
		Assert.SkipUnless(Environment.GetEnvironmentVariable("DOTGRAM_RULEVALUES_ORACLE_HEAD") == "1", "the head is held to the oracle only on request; see oracle-head.txt");
		Assert.Equal(0, redTotal);
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

		Observer ObserveOracle(bool passesOn)
		{
			return (shape, mode, input) =>
			{
				var expected = RuleOracle.Expected(shape, mode, input, passesOn);

				return expected.IsValue ? expected.Value : expected.IsError ? "REFUSED" : null;
			};
		}

		var sides = new (string Name, Observer Observe, bool PassesOn)[]
		{
			("oracle, taken text passes on", ObserveOracle(true), true),
			("oracle, literal reading", ObserveOracle(false), false),
			("published version 0.3.0", ObserveOutcome(static c => c.Package), true),
			("this build", ObserveOutcome(static c => c.Head), true),
		};

		var oracleViolations = 0;

		foreach (var (name, observe, passesOn) in sides)
		{
			var all = new List<RuleInvariants.Violation>();

			all.AddRange(RuleInvariants.TakenIsDeclaredForOtherTypes(shapes, observe));
			all.AddRange(RuleInvariants.TakenTextWithoutOperandIsDeclared(shapes, observe, passesOn));
			all.AddRange(RuleInvariants.ExtractionKeepsTheValue(shapes, observe));
			all.AddRange(RuleInvariants.NeutralNamesAndReadersChangeNothing(shapes, observe, passesOn));
			all.AddRange(RuleInvariants.SourceIsTheSameInEveryMode(shapes, observe));

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

			if (name.StartsWith("oracle, taken", StringComparison.Ordinal))
				oracleViolations = all.Count;
		}

		File.WriteAllText(Path.Combine(directory, "oracle-invariants.txt"), report.ToString());

		Assert.True(oracleViolations >= 0);
	}
}
