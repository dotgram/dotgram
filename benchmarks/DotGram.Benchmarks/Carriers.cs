using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Benchmarks;

/// <summary>
/// `--carriers [output]`: which carrier `Auto` took for every grammar of the solution, and why a
/// grammar is on the tape — gathered from the reports a build writes and put in one file.
/// </summary>
/// <remarks>
/// <para>
/// A build with <c>-p:DotGramReportGeneration=full</c> writes beside each grammar's generated file
/// a <c>*.DotGramReportDetail.g.cs</c>: a line of what it was given, and under it what GRAM5012 rested
/// on — the carrier, the gate that kept the grammar on the tape, and each rule kept there with its
/// cause. `Replay`'s gate names the place a cause was found and what around it lets the reading be
/// replaced; the reader's gate names the rules that can be read again after answering. This builds
/// nothing: it reads what the last such build left in every <c>obj/GeneratedFiles</c>, so run it
/// after one.
/// </para>
/// <para>
/// Written, never edited, and a run behind main by construction. A change that means to move a
/// grammar off the tape is measured by the difference in this file before and after it.
/// </para>
/// </remarks>
static class Carriers
{
	sealed record Grammar(string Name, string Summary, List<string> Lines)
	{
		public string Head => Lines.FirstOrDefault(static line => line.StartsWith("carrier:", StringComparison.Ordinal)) ?? "";

		public string Field(string name)
		{
			foreach (var part in Head.Split(';'))
			{
				var kv = part.Trim().Split(':', 2);

				if (kv.Length == 2 && kv[0].Trim() == name)
					return kv[1].Trim();
			}

			return "";
		}
	}

	/// <summary>"one project", "four projects" — a count that reads the way a sentence does.</summary>
	static string Say(int count) => count == 1 ? "one project" : $"{count} projects";

	/// <summary>When the reports were written: one time, or the oldest and the newest.</summary>
	/// <remarks>
	/// Both ends rather than the newest, because the newest is the one that is right and the oldest
	/// is the one worth seeing. Reports of a single build land within the same minute.
	/// </remarks>
	static string Span(IEnumerable<DateTime> times)
	{
		var all = times.OrderBy(static one => one).ToList();

		return all.Count == 0 || all[0].ToString("yyyy-MM-dd HH:mm") == all[^1].ToString("yyyy-MM-dd HH:mm")
			? $"written {all[^1]:yyyy-MM-dd HH:mm}"
			: $"written {all[0]:yyyy-MM-dd HH:mm} to {all[^1]:yyyy-MM-dd HH:mm}";
	}

	public static void Run(string? output)
	{
		var root = Root();

		output ??= Path.Combine(root, "docs", "carriers.md");

		var grammars = new List<Grammar>();

		// Which projects left a report, and when. A project built below the full level leaves none,
		// and the difference between "this grammar is not on the tape" and "this grammar was never
		// looked at" cannot be seen in a row, so it is said above the table instead.
		//
		// The time is there for the other end of the same defect: a report is a file, a file outlives
		// the build that wrote it, and a project built at `none` — or not built at all — keeps the one
		// an older build left. Read today, it answers as today's, and nothing in a row says otherwise.
		// A date beside the project's name says it at a glance.
		var projects = new SortedDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

		foreach (var file in Directory.EnumerateFiles(root, "*.DotGramReportDetail.g.cs", SearchOption.AllDirectories))
		{
			var at = file.Replace('\\', '/').IndexOf("/obj/GeneratedFiles/", StringComparison.Ordinal);

			if (at < 0)
				continue;

			var project = Path.GetFileName(file.Substring(0, at));
			var written = File.GetLastWriteTime(file);

			// The newest of a project's reports: one build wrote them all, and a project built for two
			// target frameworks writes the same report twice.
			if (!projects.TryGetValue(project, out var already) || already < written)
				projects[project] = written;

			var lines = File.ReadAllLines(file).Select(static line => line.TrimStart('﻿').TrimStart('/', ' ')).ToList();

			if (lines.Count == 0 || !lines[0].StartsWith("DotGram: ", StringComparison.Ordinal))
				continue;

			var name = lines[0].Substring("DotGram: ".Length).Split(',')[0];

			grammars.Add(new Grammar(name, lines[0], lines.Skip(1).Where(static line => line.Length > 0).ToList()));
		}

		// One grammar may be built for several target frameworks; the report is the same.
		grammars = [.. grammars
			.GroupBy(static one => one.Name, StringComparer.Ordinal)
			.Select(static group => group.First())
			.OrderBy(static one => one.Name, StringComparer.Ordinal)];

		if (grammars.Count == 0)
		{
			Console.WriteLine("No reports. Build with -p:DotGramReportGeneration=full first.");

			return;
		}

		var text = new StringBuilder();

		text.AppendLine("# Which carrier each grammar is read with, and why");
		text.AppendLine();
		text.AppendLine("Every grammar the last build with `-p:DotGramReportGeneration=full` compiled: the carrier");
		text.AppendLine("`Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it and each rule");
		text.AppendLine("held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`) from the reports");
		text.AppendLine("that build left; run again rather than edited.");
		text.AppendLine();
		text.AppendLine($"Read from {Say(projects.Count)}, and written when each one was last compiled with the");
		text.AppendLine("report on. A project built below that level leaves no report and is absent here rather than");
		text.AppendLine("empty, so a short table is a short build and not a grammar with nothing to say; a project whose");
		text.AppendLine("time is older than the rest was not in the last build, and its rows are that build's answer.");
		text.AppendLine();
		text.AppendLine("| Read from | Report written |");
		text.AppendLine("| --- | --- |");

		foreach (var (project, written) in projects)
			text.AppendLine($"| {project} | {written:yyyy-MM-dd HH:mm} |");

		text.AppendLine();
		text.AppendLine("**Carrier** is what `Auto` took: `immediate`, `tape`, or the author's own choice. **Gate** is what");
		text.AppendLine("kept a grammar on the tape: `replay` — a building rule read where the reading may not stand");
		text.AppendLine("(`Replay`) — or `read again` — a rule the reader can be asked again after it answered, which is");
		text.AppendLine("asked only where the first gate let everything through. **Direct** is how many of the replayed");
		text.AppendLine("rules have a cause of their own; the rest are under one of them. **Points** is how many of the");
		text.AppendLine("sites that build — a call whose value is built, a construction — have a point past which what they");
		text.AppendLine("read is settled (`Commit`), of how many there are: what building at that point could take off the");
		text.AppendLine("tape. **Refused** is how many of a grammar's machines the immediate carrier refuses before any gate");
		text.AppendLine("is asked (gate `refused` where nothing else kept it), each named under the grammar with its");
		text.AppendLine("reason; their building rules count in **Building**. **Alone** is how many of those neither gate");
		text.AppendLine("would keep on the tape: what lifting the refusal would move.");
		text.AppendLine();
		text.AppendLine("A carrier is chosen per machine, and a grammar has one machine per publication group and input");
		text.AppendLine("form, so the grammar's row is a **summary** and shows the worst of its machines. What a machine");
		text.AppendLine("answers is in the table below it, and that is the row to read before expecting anything of a");
		text.AppendLine("change: a grammar on the tape may have a machine that is not.");
		text.AppendLine();
		text.AppendLine("| Grammar (summary) | Machines | On the tape | Carrier | Gate | Building | Replayed | Direct | Read again | Refused | Alone | Points |");
		text.AppendLine("| --- | ---: | ---: | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");

		foreach (var grammar in grammars)
			text.AppendLine(
				$"| {grammar.Name} | {grammar.Field("machines")} | {grammar.Field("on the tape")} | {Carrier(grammar)} | " +
				$"{grammar.Field("gate")} | {grammar.Field("building")} | " +
				$"{grammar.Field("replayed")} | {grammar.Field("direct")} | {grammar.Field("read again")} | " +
				$"{grammar.Field("refused")} | {grammar.Field("alone")} | {grammar.Field("points")} |");

		text.AppendLine();
		text.AppendLine("## Machine by machine");
		text.AppendLine();
		text.AppendLine("One row a machine: what it publishes, the form it reads (`whole` for a text held whole, `buffered`");
		text.AppendLine("for a reader, `bytes` for a byte stream), and its own carrier, gate and counts. The counts are the");
		text.AppendLine("machine's own, and so are the points: a site belongs to the machine that reads the rule it stands in.");
		text.AppendLine();
		text.AppendLine("| Grammar | Publishes | Form | Carrier | Gate | Building | Replayed | Read again | Refused | Points |");
		text.AppendLine("| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var grammar in grammars)
			foreach (var line in grammar.Lines.Where(static line => line.StartsWith("machine ", StringComparison.Ordinal)))
			{
				var head  = line.Substring("machine ".Length);
				var split = head.IndexOf("]: ", StringComparison.Ordinal);

				if (split < 0)
					continue;

				var named = head.Substring(0, split);
				var rest  = head.Substring(split + "]: ".Length);
				var form  = named.Substring(named.LastIndexOf('[') + 1);
				var names = named.Substring(0, named.LastIndexOf('[')).Trim();
				var parts = names.Split(", ");
				var shown = parts.Length > 3
					? string.Join(", ", parts.Take(3)) + $" and {parts.Length - 3} more"
					: names;

				string Field(string name)
				{
					foreach (var part in rest.Split(';'))
					{
						var pair = part.Trim().Split(':', 2);

						if (pair.Length == 2 && pair[0].Trim() == name)
							return pair[1].Trim();
					}

					return "";
				}

				text.AppendLine(
					$"| {grammar.Name} | {shown} | {form} | {Field("carrier")} | {Field("gate")} | {Field("building")} | " +
					$"{Field("replayed")} | {Field("read again")} | {Field("refused")} | {Field("points")} |");
			}

		// The second gate by the shape that opened the way and why the way stays: `open` lines are
		// `open Rule: shape; why; how the rule is called; node`.
		var opens = grammars
			.SelectMany(static grammar => grammar.Lines
				.Where(static line => line.StartsWith("open ", StringComparison.Ordinal))
				.Select(line =>
				{
					var colon = line.IndexOf(": ", StringComparison.Ordinal);
					var parts = line.Substring(colon + 2).Split("; ", 4);
					var shape = parts[0].Replace(", captured", "", StringComparison.Ordinal);

					return (Grammar: grammar.Name, Rule: line.Substring(5, colon - 5), Class: shape + "; " + parts[1],
						Captured: parts[0].EndsWith(", captured", StringComparison.Ordinal), Sealed: parts[2] != "open", Node: parts[3]);
				}))
			.ToList();

		if (opens.Count > 0)
		{
			text.AppendLine();
			text.AppendLine("## Where the second gate's ways are opened");
			text.AppendLine();
			text.AppendLine("Each place a rule's own reading opens a way, by its shape — a choice over characters, a run of");
			text.AppendLine("one character, the turns of a repetition — and why the way could not be left out. **Places**");
			text.AppendLine("counts each once per grammar; **captured** is how many of them capture inside the turn, and");
			text.AppendLine("**sealed** how many are in a rule every call of which is inside an atomic group or a lookahead,");
			text.AppendLine("or which nothing calls, so that no caller asks it again.");
			text.AppendLine();
			text.AppendLine("| Shape | Why the way stays | Grammars | Rules | Places | Captured | Sealed | For example |");
			text.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |");

			foreach (var group in opens.GroupBy(static one => one.Class).OrderByDescending(static group => group.Count()))
			{
				var first = group.First();
				var split = group.Key.Split("; ", 2);

				text.AppendLine(
					$"| {split[0]} | {split[1]} | {group.Select(static one => one.Grammar).Distinct().Count()} | " +
					$"{group.Select(static one => one.Grammar + " " + one.Rule).Distinct().Count()} | {group.Count()} | " +
					$"{group.Count(static one => one.Captured)} | {group.Count(static one => one.Sealed)} | {first.Grammar.Split('.')[^1]}.{first.Rule}: " +
					$"`{first.Node.Replace("|", "\\|", StringComparison.Ordinal)}` |");
			}
		}

		foreach (var grammar in grammars.Where(static one => one.Lines.Count > 1))
		{
			text.AppendLine();
			text.AppendLine($"## {grammar.Name}");
			text.AppendLine();

			// The rules with a cause of their own first, then what hangs under them.
			foreach (var line in grammar.Lines.Skip(1).OrderBy(static line => line.Contains(": under ", StringComparison.Ordinal) ? 1 : 0))
				text.AppendLine($"- {line}");
		}

		File.WriteAllText(output, text.ToString().Replace("\r\n", "\n"));

		Console.WriteLine(
			$"{grammars.Count} grammars from {Say(projects.Count)} ({Span(projects.Values)}), " +
			$"{grammars.Count(static one => Carrier(one) == "tape")} on the tape, written to {output}");
	}

	static string Carrier(Grammar grammar)
	{
		var head = grammar.Head;

		if (head.Length == 0)
			return "";

		var carrier = head.Substring("carrier:".Length).Split(';')[0].Trim();

		return carrier.EndsWith(", the author's", StringComparison.Ordinal)
			? carrier.Substring(0, carrier.Length - ", the author's".Length) + " (author)"
			: carrier;
	}

	static string Root()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? ".";
	}
}
