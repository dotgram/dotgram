using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The rows of <c>docs/design/sql-tsql-tree-inventory.md</c>: each old record's category and the target it
/// names, read from the page rather than copied, so that the gap a family throws for a record it has no arm
/// for says what the inventory says.
/// </summary>
static class Inventory
{
	public sealed record Row(string Name, string Category, string Target);

	static readonly Lazy<Dictionary<string, Row>> Rows = new(Read);

	public static string Page([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "..", "docs", "design", "sql-tsql-tree-inventory.md"));
	}

	public static Row? Of(string name)
	{
		return Rows.Value.TryGetValue(name, out var row) ? row : null;
	}

	static Dictionary<string, Row> Read()
	{
		var rows = new Dictionary<string, Row>(StringComparer.Ordinal);

		foreach (var line in File.ReadAllLines(Page()))
		{
			if (!line.StartsWith("| `", StringComparison.Ordinal))
				continue;

			var cells = line.Split('|').Select(static one => one.Trim()).ToArray();

			if (cells.Length < 6)
				continue;

			var name = cells[1].Trim('`');

			rows.TryAdd(name, new Row(name, cells[3], cells[4]));
		}

		return rows;
	}

	/// <summary>
	/// The T-SQL statements the new tree declares (<c>Sql2023Ast.TransactSql.cs</c>) and <c>Sql2023Writer</c>
	/// has no arm for, which a target named by the inventory may be one of.
	/// </summary>
	static readonly HashSet<string> Unwritten = new(StringComparer.Ordinal)
	{
		"Statement.DropObject", "Statement.DropAssembly", "Statement.DropEventSession", "Statement.DropEventNotification",
		"Statement.DropExternalLibrary", "Statement.DropFulltextIndex", "Statement.DropSensitivityClassification",
		"Statement.DropMasterKey", "Statement.DropDatabaseEncryptionKey", "Statement.DropIndex", "Statement.DropSignature",
		"Statement.DeallocateCursor", "Statement.SetCursorVariable", "Statement.BeginTransaction", "Statement.SaveTransaction",
	};

	/// <summary>
	/// The gap for an old record a family owns and has no arm for, from its inventory row: a record only
	/// T-SQL has is missing from the new tree, or there and unwritten; a shape that differs needs its
	/// decision; and a record whose concept is there waits for its arm.
	/// </summary>
	public static BridgeGap Unconverted(object old)
	{
		var name = Bridge.NameOf(old);

		if (Of(name) is not { } row)
			return BridgeGap.Open(old, $"no inventory row for {name}");

		var target = Target(row.Target);

		return row.Category switch
		{
			"C" when target is not null && Unwritten.Contains(target) => BridgeGap.Unwritten(old, target),
			"C" when target is not null && Declared(target)           => BridgeGap.Open(old, $"inventory {name} (target {target} declared, no arm yet)"),
			"C"                                                        => BridgeGap.Missing(old, target ?? row.Target),
			"B"                                                        => BridgeGap.Open(old, $"inventory {name}"),
			_                                                          => BridgeGap.Open(old, $"inventory {name} (category {row.Category}, no arm yet)"),
		};
	}

	/// <summary>
	/// The first name in backticks in a target cell, as <c>Statement.CreateIndex</c>.
	/// </summary>
	static string? Target(string cell)
	{
		var start = cell.IndexOf('`');
		var end   = start < 0 ? -1 : cell.IndexOf('`', start + 1);

		return end > start ? cell.Substring(start + 1, end - start - 1) : null;
	}

	/// <summary>
	/// Whether a name like <c>Statement.CreateIndex</c> is a record of the new tree.
	/// </summary>
	static bool Declared(string name)
	{
		var parts = name.Split('.');
		var type  = typeof(Ast.Statement).Assembly.GetType("DotGram.Sql.Ast." + parts[0]);

		for (var at = 1; type is not null && at < parts.Length; at++)
			type = type.GetNestedType(parts[at]);

		return type is not null;
	}
}
