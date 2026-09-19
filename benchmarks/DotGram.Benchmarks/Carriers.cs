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
/// A build with <c>-p:DotGramReportGeneration=true</c> writes beside each grammar's generated file
/// a <c>*.DotGramReport.g.cs</c>: a line of what was generated, and under it what GRAM5012 rested
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

	public static void Run(string? output)
	{
		var root = Root();

		output ??= Path.Combine(root, "docs", "carriers.md");

		var grammars = new List<Grammar>();

		foreach (var file in Directory.EnumerateFiles(root, "*.DotGramReport.g.cs", SearchOption.AllDirectories))
		{
			if (!file.Replace('\\', '/').Contains("/obj/GeneratedFiles/", StringComparison.Ordinal))
				continue;

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
			Console.WriteLine("No reports. Build with -p:DotGramReportGeneration=true first.");

			return;
		}

		var text = new StringBuilder();

		text.AppendLine("# Which carrier each grammar is read with, and why");
		text.AppendLine();
		text.AppendLine("Every grammar of the solution, as the last build with `-p:DotGramReportGeneration=true` compiled");
		text.AppendLine("it: the carrier `Auto` took (GRAM5012), and for a grammar kept on the tape, the gate that kept it");
		text.AppendLine("and each rule held there. Written by `--carriers` (`benchmarks/DotGram.Benchmarks/Carriers.cs`)");
		text.AppendLine("from the reports that build left; run again rather than edited.");
		text.AppendLine();
		text.AppendLine("**Carrier** is what `Auto` took: `immediate`, `tape`, or the author's own choice. **Gate** is what");
		text.AppendLine("kept a grammar on the tape: `replay` — a building rule read where the reading may not stand");
		text.AppendLine("(`Replay`) — or `read again` — a rule the reader can be asked again after it answered, which is");
		text.AppendLine("asked only where the first gate let everything through. **Direct** is how many of the replayed");
		text.AppendLine("rules have a cause of their own; the rest are under one of them.");
		text.AppendLine();
		text.AppendLine("| Grammar | Carrier | Gate | Building | Replayed | Direct | Read again |");
		text.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: |");

		foreach (var grammar in grammars)
			text.AppendLine(
				$"| {grammar.Name} | {Carrier(grammar)} | {grammar.Field("gate")} | {grammar.Field("building")} | " +
				$"{grammar.Field("replayed")} | {grammar.Field("direct")} | {grammar.Field("read again")} |");

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

		Console.WriteLine($"{grammars.Count} grammars, {grammars.Count(static one => Carrier(one) == "tape")} on the tape, written to {output}");
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
