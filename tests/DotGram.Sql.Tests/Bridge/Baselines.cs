using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Sql.Tests;

/// <summary>
/// The ratchet's files: <c>Baseline.&lt;Family&gt;.txt</c>, one line per old record a family owns with what
/// became of the statements counted under it — <c>converted/gap/refused/crashed/unread/different/shape</c> —
/// and <c>Baseline.Counters.txt</c>, the counters that are debts rather than results.
/// </summary>
static class Baselines
{
	public const string Variable = "DOTGRAM_RECORD_BRIDGE_BASELINE";

	public sealed record Counts(int Converted, int Gap, int Refused, int Crashed, int Unread, int Different, int Shape)
	{
		public override string ToString()
		{
			return $"{Converted}/{Gap}/{Refused}/{Crashed}/{Unread}/{Different}/{Shape}";
		}

		public static Counts Parse(string text)
		{
			var parts = text.Split('/').Select(int.Parse).ToArray();

			return parts.Length == 7
				? new Counts(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6])
				: throw new FormatException($"`{text}` is not converted/gap/refused/crashed/unread/different/shape");
		}
	}

	public static readonly Counts None = new(0, 0, 0, 0, 0, 0, 0);

	public static string Folder([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}

	public static string File(string name)
	{
		return Path.Combine(Folder(), $"Baseline.{name}.txt");
	}

	/// <summary>
	/// A baseline's rows, <c>key = value</c>, the key everything before the last <c> = </c>.
	/// </summary>
	public static Dictionary<string, string> Rows(string name)
	{
		var rows = new Dictionary<string, string>(StringComparer.Ordinal);
		var path = File(name);

		if (!System.IO.File.Exists(path))
			return rows;

		foreach (var line in System.IO.File.ReadAllLines(path))
		{
			if (line.Length == 0 || line[0] == '#')
				continue;

			var at = line.LastIndexOf(" = ", StringComparison.Ordinal);

			if (at < 0)
				throw new FormatException($"{path}: `{line}` is not `key = value`.");

			rows[line[..at]] = line[(at + 3)..];
		}

		return rows;
	}

	public static Dictionary<string, Counts> Read(string family)
	{
		return Rows(family).ToDictionary(static one => one.Key, static one => Counts.Parse(one.Value), StringComparer.Ordinal);
	}

	public static void Write(string family, IEnumerable<string> owns, IReadOnlyDictionary<string, Counts> counts)
	{
		var text = new StringBuilder();

		text.Append("# What became of the statements counted under each old record the ").Append(family).Append(" family owns:\r\n");
		text.Append("# `Record = converted/gap/refused/crashed/unread/different/shape` (docs/design/sql-tsql-bridge.md). A\r\n");
		text.Append("# statement is counted under the record its first gap names, or else under its own. BridgeTests fails\r\n");
		text.Append("# where `converted` falls or any of the last five rises, and prints a gain; this file is rewritten\r\n");
		text.Append("# deliberately, by a run with ").Append(Variable).Append("=1, and never by a passing one.\r\n");
		text.Append("\r\n");

		foreach (var record in owns.OrderBy(static one => one, StringComparer.Ordinal))
			text.Append(record).Append(" = ").Append(counts.TryGetValue(record, out var count) ? count : None).Append("\r\n");

		System.IO.File.WriteAllText(File(family), text.ToString());
	}

	/// <summary>
	/// <c>Baseline.Counters.txt</c>: how often a member passed only with key words' case aside, how often one
	/// was read from the text, how often each dialect pattern exempted a subtree. Debts, each no higher than
	/// recorded.
	/// </summary>
	public static void WriteCounters(IReadOnlyDictionary<string, int> counters)
	{
		var text = new StringBuilder();

		text.Append("# Counters that are debts, each held no higher than here by BridgeTests (docs/design/sql-tsql-bridge.md):\r\n");
		text.Append("#   ByCase <member>     - converted only with the case of a key word in that member aside\r\n");
		text.Append("#   Recovered <member>  - a member of the new tree read from the text, which the old tree does not keep\r\n");
		text.Append("#   Meaning <pattern>   - a difference exempted from the shape check, inside the subtree the pattern names\r\n");
		text.Append("# Rewritten deliberately, by a run with ").Append(Variable).Append("=1.\r\n");
		text.Append("\r\n");

		foreach (var (key, count) in counters.OrderBy(static one => one.Key, StringComparer.Ordinal))
			text.Append(key).Append(" = ").Append(count).Append("\r\n");

		System.IO.File.WriteAllText(File("Counters"), text.ToString());
	}
}
