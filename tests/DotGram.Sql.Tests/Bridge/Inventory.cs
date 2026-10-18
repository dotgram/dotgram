using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Sql.TransactSql;

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

	/// <summary>
	/// Every gap the bridge throws for a record it has no arm for, in this assembly, says what the inventory says.
	/// </summary>
	[ModuleInitializer]
	internal static void Explain()
	{
		Bridge.Explained = Unconverted;
	}

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
	/// Whether <c>Sql2023Writer</c> has an arm for a record of the new tree, asked of the writer itself: a node
	/// made without its constructor is handed to it, and a writer with no arm refuses it with
	/// <see cref="NotSupportedException"/> before it reads a member, where one with an arm reads a member that
	/// is null and fails some other way.
	/// </summary>
	public static bool Written(Type type)
	{
		try
		{
			Ast.Sql2023Writer.Write((Ast.ISqlNode)RuntimeHelpers.GetUninitializedObject(type));

			return true;
		}
		catch (NotSupportedException)
		{
			return false;
		}
		catch (Exception)
		{
			return true;
		}
	}

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
			"C" when Declared(target) is { } type && !Written(type)   => BridgeGap.Unwritten(old, target!),
			"C" when Declared(target) is not null                     => BridgeGap.Unbridged(old, target!),
			"C"                                                        => BridgeGap.Missing(old, target ?? row.Target),
			"B"                                                        => BridgeGap.Open(old, $"inventory {name}"),
			_                                                          => BridgeGap.Unbridged(old, $"inventory {name} (category {row.Category})"),
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
	/// The record of the new tree a name like <c>Statement.CreateIndex</c> names, or null.
	/// </summary>
	public static Type? Declared(string? name)
	{
		if (name is null)
			return null;

		var parts = name.Split('.');
		var type  = typeof(Ast.Statement).Assembly.GetType("DotGram.Sql.Ast." + parts[0]);

		for (var at = 1; type is not null && at < parts.Length; at++)
			type = type.GetNestedType(parts[at]);

		return type;
	}
}
