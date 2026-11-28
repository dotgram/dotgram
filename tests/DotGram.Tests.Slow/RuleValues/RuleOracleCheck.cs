using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DotGram.Tests.RuleValues;

/// <summary>One input of one grammar in one mode, with what the published version and this build gave.</summary>
sealed record RuleCell(RuleShape Shape, Mode Mode, string Input, Outcome Package, Outcome Head);

/// <summary>
/// The two holds of the gate, over cells however they were had: this build held to the oracle, and the oracle held to the
/// published version through the allow-list. The gate runs both on the cells it has just made, and
/// <see cref="RuleOracleTests"/> on the cells of a run written to disk.
/// </summary>
static class RuleOracleCheck
{
	/// <summary>The result of holding this build to the oracle.</summary>
	internal sealed record HeadHold(int Green, int Red, string Report);

	/// <summary>The result of holding the oracle to the published version.</summary>
	internal sealed record PublishedHold(string Report, IReadOnlyList<string> Dead, int Unexplained, int Stops);

	enum PublishedFailure
	{
		/// <summary>A defect of 0.3.0: a C# error in generated code, GRAM0001, or a GRAM4008 that came from building a string from names.</summary>
		Defect,

		/// <summary>A refusal 0.3.0 documents: GRAM4026, GRAM4006, GRAM4007, GRAM3004.</summary>
		Documented,

		/// <summary>Anything else.</summary>
		Other,
	}

	/// <summary>The identifiers an outcome that is not built gave, in the order it gave them.</summary>
	static string[] Ids(Outcome outcome)
	{
		var head = outcome.Text.Split(' ')[0];

		// A C# error the runner could not attribute to one grammar is written with a word before its identifier.
		if (head == "UNOWNED")
			head = outcome.Text.Split(' ').Skip(1).FirstOrDefault() ?? "";

		return head.Split(',');
	}

	static PublishedFailure Failure(Outcome published)
	{
		if (published.Kind is "csharp" or "crash")
			return PublishedFailure.Defect;

		if (published.Kind != "gram")
			return PublishedFailure.Other;

		var ids = Ids(published);

		if (ids.Any(static id => id is "GRAM4026" or "GRAM4006" or "GRAM3004"))
			return PublishedFailure.Documented;

		// A GRAM4008 about a helper comes from building a string from names, which the rule removes. A GRAM4007 beside it is
		// held only where the oracle itself finds two kinds, and then the oracle does not ask for a value.
		if (ids.Contains("GRAM4008"))
			return PublishedFailure.Defect;

		return ids.Contains("GRAM4007") ? PublishedFailure.Documented : PublishedFailure.Other;
	}

	/// <summary>
	/// The diagnostic a refusal the oracle names is told by: a refusal of the rule's own (<c>E-…</c>) is GRAM4008, a GRAM or a C#
	/// identifier is itself.
	/// </summary>
	static string DiagnosticOf(string errorId)
	{
		return errorId.StartsWith("E-", StringComparison.Ordinal) ? "GRAM4008" : errorId;
	}

	/// <summary>
	/// Whether this build refused as the oracle expects: with a diagnostic of a fault the oracle finds, the generator's for a
	/// refusal of the generator and the C# compiler's for one of the compiler. A grammar with more than one fault is told one
	/// at a place, so any of them is the refusal. A C# error or a crash where the generator was to refuse is a generator that
	/// left the author code that does not compile, not a refusal.
	/// </summary>
	/// <remarks>
	/// A name inside a lookahead is told before anything else of the rule, so where the oracle finds one the head names it
	/// (GRAM4006), whatever else is wrong. A refusal of the rule's own whose message says its cause apart (a piece with no
	/// text, an array no call fills or one that could hold itself, a left-recursive value) is held to that cause too, where it
	/// is the only diagnostic the head gave; the rest are one GRAM4008.
	/// </remarks>
	static bool Refuses(Outcome head, Expectation expected)
	{
		var said     = Ids(head);
		var refusals = expected.Refusals.Count > 0 ? expected.Refusals : [expected.ErrorId!];

		if (refusals[0] == "GRAM4006")
			return head.Kind == "gram" && said.Contains("GRAM4006");

		return refusals.Any(refusal => DiagnosticOf(refusal) is var id && (id.StartsWith("CS", StringComparison.Ordinal)
			? head.Kind == "csharp" && said.Contains(id)
			: head.Kind == "gram" && said.Contains(id) && (said.Length > 1 || Cause(refusal) is not { } cause || head.Text.Contains(cause, StringComparison.Ordinal))));
	}

	/// <summary>What the message of a refusal of the rule's own says of its cause, where the cause is told apart.</summary>
	static string? Cause(string refusal)
	{
		return refusal switch
		{
			"E-NONTEXT"   => "which has no text to join",
			"E-SEQ"       => "and no call it reads gives one",
			"E-AMBIGUOUS" => "it is one element, or its elements in place",
			"E-LR"        => "is left-recursive and declares",
			_             => null,
		};
	}

	/// <summary>Whether this build refused with one of the documented diagnostics the published version gave.</summary>
	static bool RefusesAsPublished(Outcome head, Outcome published)
	{
		var wanted = Ids(published).Where(static id => id is "GRAM4026" or "GRAM4006" or "GRAM4007" or "GRAM3004").ToList();

		return head.Kind == "gram" && wanted.Count > 0 && wanted.Any(id => Ids(head).Contains(id));
	}

	static string Show(Outcome outcome)
	{
		var text = outcome.ToString();

		return text.Length > 110 ? text.Substring(0, 110) + "…" : text;
	}

	internal static string Printed(RuleShape shape)
	{
		return RulePrinter.Print(shape).Rules.TrimEnd('\n').Replace("\n", "\n    ", StringComparison.Ordinal);
	}

	static void Example(StringBuilder report, RuleCell cell, string older, Expectation expected, string label)
	{
		report.Append("    ").Append(cell.Shape.Id).Append(" mode ").Append(cell.Mode).Append(" input ").Append(cell.Input).Append('\n');
		report.Append("    ").Append(Printed(cell.Shape)).Append('\n');
		report.Append("      ").Append(label).Append(older).Append('\n');
		report.Append("      oracle: ").Append(expected).Append('\n');
	}

	/// <summary>This build held to the oracle: a value where it expects one, the refusal it names where it expects one.</summary>
	internal static HeadHold HoldHead(IReadOnlyList<RuleCell> cells)
	{
		var report = new StringBuilder();
		var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var red    = new SortedDictionary<string, List<(RuleCell Cell, Expectation Expected)>>(StringComparer.Ordinal);

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
				// Where the published version gave a value, the head gives the same. Where it refused with a documented diagnostic,
				// the head refuses with it. Where it failed by a defect of its own, an untyped rule with names gives a value of its
				// generated type (over bytes a raw capture is refused), and a name captured twice on one route has nothing to be
				// held to. A diagnostic that is on neither list is a stop.
				var failure = Failure(cell.Package);
				var other   = cell.Package.Kind != "value" && failure == PublishedFailure.Other;

				if (!other && (cell.Package.Kind == "value" ? head.Kind == "value" && head.Text == cell.Package.Text
					: expected.ErrorId is not null ? Refuses(head, expected)
					: failure == PublishedFailure.Documented ? RefusesAsPublished(head, cell.Package)
					: expected.Value is not null ? head.Kind == "value" && head.Text == expected.Value
					: expected.Row == "O-twice" ? head.Kind is "value" or "fail" or "gram"
					: head.Kind == "value"))
				{
					counts["green"] = counts.GetValueOrDefault("green") + 1;
					continue;
				}

				counts["red"] = counts.GetValueOrDefault("red") + 1;

				var held = (other ? "STOP, 0.3.0 gave a diagnostic not on the list (" + cell.Package.Kind + " " + (cell.Package.Text.Length > 40 ? cell.Package.Text[..40] : cell.Package.Text) + "): " : "other, rule O: held to 0.3.0 (untyped rule with names, or a name captured twice), ")
					+ "head " + (head.Kind == "value" ? "gave a different value" : head.Kind);

				if (!red.TryGetValue(held, out var heldList))
					red[held] = heldList = [];

				heldList.Add((cell, expected));
				continue;
			}

			if (expected.Source == Source.NoMatch ? head.Kind == "fail"
				: expected.IsValue ? head.Kind == "value" && head.Text == expected.Value
				: Refuses(head, expected))
			{
				counts["green"] = counts.GetValueOrDefault("green") + 1;
				continue;
			}

			counts["red"] = counts.GetValueOrDefault("red") + 1;

			// The class of a red cell: the entry of the allow-list whose grammars it is, or the rule that decides it.
			var covering = RuleAllowList.Covering(new AllowCase(cell.Shape, cell.Mode, head, Newer.Of(expected), RuleOracle.Describe(cell.Shape)) { AsIfDifferent = true }).FirstOrDefault();
			var key = (covering is null ? "other, rule " + expected.Row : covering.Impact.ToString().ToLowerInvariant() + " " + covering.Name)
				+ ": expected " + (expected.IsError ? "error " + expected.ErrorId : expected.Source.ToString())
				+ ", head " + (head.Kind == "value" ? "gave a different value" : head.Kind == "gram" && expected.IsError ? "gram, another diagnostic" : head.Kind);

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

		return new HeadHold(counts.GetValueOrDefault("green"), counts.GetValueOrDefault("red"), report.ToString());
	}

	/// <summary>
	/// The oracle held to the published version: every cell where they differ is covered by an entry of the allow-list. An
	/// entry that covers nothing is dead only over the whole enumeration, so a run of part of it reports the dead entries and
	/// leaves the asking to the caller.
	/// </summary>
	internal static PublishedHold HoldPublished(IReadOnlyList<RuleCell> cells, int shapeCount, int missing)
	{
		var report      = new StringBuilder();
		var counts      = new SortedDictionary<string, int>(StringComparer.Ordinal);
		var matched     = RuleAllowList.Entries.ToDictionary(static e => e.Name, static _ => new List<RuleCell>());
		var agreeing    = RuleAllowList.Entries.ToDictionary(static e => e.Name, static _ => 0);
		var unexplained = new List<(RuleCell Cell, Expectation Expected)>();
		var unmatched   = new List<(RuleCell Cell, Expectation Expected)>();
		var undecided   = new SortedDictionary<string, (int Count, RuleCell Example)>(StringComparer.Ordinal);
		var stops       = 0;

		report.Append(CultureInfo.InvariantCulture, $"cells {cells.Count}, shapes {shapeCount}, cells with no outcome in the files {missing}\n");

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
				category = old.Kind == "value" ? "same as 0.3.0 by decision (an untyped rule that writes names, or a name captured twice on one route)"
					: Failure(old) switch
					{
						PublishedFailure.Documented => "refused by both: a documented refusal of 0.3.0 is held",
						PublishedFailure.Defect     => "0.3.0 failed: the generated type's value is expected",
						_                           => "STOP: 0.3.0 gave a diagnostic not on the list",
					};

				if (old.Kind != "value" && Failure(old) == PublishedFailure.Other)
					stops++;
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

		return new PublishedHold(report.ToString(), dead, unexplained.Count, stops);
	}
}
