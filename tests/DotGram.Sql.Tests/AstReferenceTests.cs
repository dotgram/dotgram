using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>docs/ast.md</c> says where every node of the tree comes from, and it has to keep
/// saying it.
/// </summary>
/// <remarks>
/// A node is called what the production it comes from is called, which is a claim about
/// two files at once: the record is in <c>Ast/SqlAst*.cs</c> and the production it is named
/// after is in the table. Nothing in either enforces the other, so this does — the same
/// bargain <see cref="DiagnosticsReferenceTests"/> strikes for the diagnostics.
/// <para>
/// The tree is families of records — <c>Statement</c>, <c>Query</c>, <c>Expression</c>,
/// <c>TableSource</c> and a hundred more — with no root above them, so a node is named by its
/// family as well: <c>Statement.Select</c>, <c>TableSource.Named</c>, and through the record it is
/// nested in where it is. The families are found in the assembly rather than listed, so one added
/// and not written down is caught like any other node.
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
		var allowed = new[] { "SQL-92", "SQL:2023", "T-SQL" };

		foreach (var (node, source) in Rows())
			Assert.True(allowed.Contains(source), $"{node}: {source}");
	}

	/// <summary>Every statement says which group it is in, and the reference says the same.</summary>
	/// <remarks>
	/// The category is asked of the statement's type (<see cref="StatementCategories"/>), and is a
	/// column of the table, which nothing but this asks of. A statement is made without its
	/// constructor to be asked: the category is the type's, and none of its values are needed.
	/// </remarks>
	[Fact]
	public void Every_statement_says_its_category_as_the_reference_does()
	{
		var written = new Dictionary<string, string>(StringComparer.Ordinal);
		var text    = File.ReadAllText(Path.Combine(Root(AppContext.BaseDirectory), "docs", "ast.md"));

		foreach (Match row in Regex.Matches(text, @"^\| `(Statement\.[\w.]+)` \|[^|]+\|[^|]+\| (\w+) \|", RegexOptions.Multiline))
			written[row.Groups[1].Value] = row.Groups[2].Value;

		foreach (var (type, path) in Records(typeof(Statement), typeof(Statement), nameof(Statement)))
		{
			var statement = (Statement)RuntimeHelpers.GetUninitializedObject(type);

			Assert.True(written.TryGetValue(path, out var category), $"{path} has no category in the reference");
			Assert.Equal(statement.Category().ToString(), category);
		}
	}

	/// <summary>The families: every hierarchy the tree is made of.</summary>
	/// <remarks>
	/// Found rather than listed, so that a family added to the tree and left out of the
	/// reference is caught by the same two tests every node is. A family is any abstract record of
	/// the namespace's own whose records are nested in it, whatever it derives from: a
	/// <c>LiteralValue</c> is an <c>Expression</c>, and its records are its own, not the expression's.
	/// </remarks>
	static IEnumerable<Type> Roots()
	{
		return typeof(Statement).Assembly
			.GetExportedTypes()
			.Where(one =>
				one.IsAbstract && !one.IsSealed && !one.IsNested && one.Namespace == typeof(Statement).Namespace &&
				one.GetNestedTypes(BindingFlags.Public).Any(nested => nested.IsSubclassOf(one)));
	}

	/// <summary>Every record of the tree, from the tree itself, named by its family.</summary>
	static IEnumerable<string> Nodes()
	{
		return Roots().SelectMany(root => Records(root, root, root.Name)).Select(static one => one.Path);
	}

	/// <summary>
	/// The records under a type that can be made, however deeply nested, each named by the path of the
	/// records it is nested in.
	/// </summary>
	static IEnumerable<(Type Type, string Path)> Records(Type root, Type holder, string path)
	{
		foreach (var one in holder.GetNestedTypes(BindingFlags.Public).Where(one => one.IsSubclassOf(root)))
		{
			if (!one.IsAbstract)
				yield return (one, path + "." + one.Name);

			foreach (var deeper in Records(root, one, path + "." + one.Name))
				yield return deeper;
		}
	}

	static HashSet<string> Documented()
	{
		return Rows().Select(one => one.Node).ToHashSet(StringComparer.Ordinal);
	}

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
