using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>Baseline.&lt;Family&gt;.txt</c>: one line per old record a family owns, with what became of the
/// statements counted under it — <c>converted/gap/refused/unread/different/shape</c>.
/// </summary>
static class Baselines
{
	public const string Variable = "DOTGRAM_RECORD_BRIDGE_BASELINE";

	public sealed record Counts(int Converted, int Gap, int Refused, int Unread, int Different, int Shape)
	{
		public override string ToString()
		{
			return $"{Converted}/{Gap}/{Refused}/{Unread}/{Different}/{Shape}";
		}

		public static Counts Parse(string text)
		{
			var parts = text.Split('/').Select(int.Parse).ToArray();

			return parts.Length == 6
				? new Counts(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5])
				: throw new FormatException($"`{text}` is not converted/gap/refused/unread/different/shape");
		}
	}

	public static readonly Counts None = new(0, 0, 0, 0, 0, 0);

	public static string Folder([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}

	public static string File(string family)
	{
		return Path.Combine(Folder(), $"Baseline.{family}.txt");
	}

	public static Dictionary<string, Counts> Read(string family)
	{
		var rows = new Dictionary<string, Counts>(StringComparer.Ordinal);
		var path = File(family);

		if (!System.IO.File.Exists(path))
			return rows;

		foreach (var line in System.IO.File.ReadAllLines(path))
		{
			if (line.Length == 0 || line[0] == '#')
				continue;

			var at = line.LastIndexOf(" = ", StringComparison.Ordinal);

			if (at < 0)
				throw new FormatException($"{path}: `{line}` is not `Record = converted/gap/refused/unread/different/shape`.");

			rows[line[..at]] = Counts.Parse(line[(at + 3)..]);
		}

		return rows;
	}

	public static void Write(string family, IEnumerable<string> owns, IReadOnlyDictionary<string, Counts> counts)
	{
		var text = new StringBuilder();

		text.Append("# What became of the statements counted under each old record the ").Append(family).Append(" family owns:\r\n");
		text.Append("# `Record = converted/gap/refused/unread/different/shape` (docs/design/sql-tsql-bridge.md). A statement is\r\n");
		text.Append("# counted under the record its gap names, or else under its own. BridgeTests fails where `converted`\r\n");
		text.Append("# falls or `refused`, `unread`, `different` or `shape` rises, and prints a gain; this file is rewritten\r\n");
		text.Append("# deliberately, by a run with ").Append(Variable).Append("=1, and never by a passing one.\r\n");
		text.Append("\r\n");

		foreach (var record in owns.OrderBy(static one => one, StringComparer.Ordinal))
			text.Append(record).Append(" = ").Append(counts.TryGetValue(record, out var count) ? count : None).Append("\r\n");

		System.IO.File.WriteAllText(File(family), text.ToString());
	}
}
