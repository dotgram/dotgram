using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Parsers;
using DotGram.Parsers.Sql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>docs/ast.md</c> says where every node of the tree comes from, and it has to keep
/// saying it.
/// </summary>
/// <remarks>
/// A node is called what the production it comes from is called, which is a claim about
/// two files at once: the record is in <c>SqlSyntax.cs</c> and the production it is named
/// after is in the table. Nothing in either enforces the other, so this does — the same
/// bargain <see cref="DiagnosticsReferenceTests"/> strikes for the diagnostics.
/// <para>
/// The tree has five roots and no root above them, so a node is named by its root as well
/// — <c>Statement.Select</c>, <c>Clause.Set</c> — and the roots themselves are checked:
/// a sixth added and not written down is a hierarchy the reference does not know about.
/// </para>
/// </remarks>
public sealed class AstReferenceTests
{
	[Fact]
	public void Every_node_of_the_tree_is_in_the_reference()
	{
		var documented = Documented();
		var missing    = Nodes().Where(one => !documented.Contains(one)).ToArray();

		Assert.Empty(missing);
	}

	/// <summary>And nothing is in the reference that is no longer a node.</summary>
	/// <remarks>
	/// The half that catches a rename: a row left behind says a node exists that does not,
	/// which is worse than no row at all — a reader trusts a table that is checked.
	/// </remarks>
	[Fact]
	public void And_nothing_in_the_reference_is_no_longer_a_node()
	{
		var nodes = Nodes().ToHashSet(StringComparer.Ordinal);
		var stale = Documented().Where(one => !nodes.Contains(one)).ToArray();

		Assert.Empty(stale);
	}

	/// <summary>Every node says which specification named it.</summary>
	[Fact]
	public void Every_row_names_a_source()
	{
		var allowed = new[] { "SQL-92", "SQL:1999", "SQL:2003", "SQL/PSM", "T-SQL" };

		foreach (var (node, source) in Rows())
			Assert.True(allowed.Contains(source), $"{node}: {source}");
	}

	/// <summary>The roots: every hierarchy the tree is made of.</summary>
	/// <remarks>
	/// Found rather than listed, so that a root added to <c>SqlSyntax.cs</c> and left out of
	/// the reference is caught by the same two tests every node is.
	/// </remarks>
	static IEnumerable<Type> Roots() =>
		typeof(Statement).Assembly
			.GetExportedTypes()
			.Where(one =>
				one.IsAbstract && !one.IsSealed && one.Namespace == typeof(Statement).Namespace &&
				one.BaseType == typeof(object) && one.GetNestedTypes(BindingFlags.Public).Length > 0);

	/// <summary>Every record of the tree, from the tree itself, named by its root.</summary>
	static IEnumerable<string> Nodes() =>
		Roots().SelectMany(root => root
			.GetNestedTypes(BindingFlags.Public)
			.Where(one => one.IsSealed && one.IsSubclassOf(root))
			.Select(one => root.Name + "." + one.Name));

	static HashSet<string> Documented() =>
		Rows().Select(one => one.Node).ToHashSet(StringComparer.Ordinal);

	static List<(string Node, string Source)> Rows()
	{
		var text = File.ReadAllText(Path.Combine(Root(AppContext.BaseDirectory), "docs", "ast.md"));
		var rows = new List<(string, string)>();

		foreach (Match row in Regex.Matches(text, @"^\| `([\w.]+)` \| ([^|]+?) \|", RegexOptions.Multiline))
			rows.Add((row.Groups[1].Value, row.Groups[2].Value.Trim()));

		return rows;
	}

	static string Root(string from)
	{
		var at = new DirectoryInfo(from);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? from;
	}
}
