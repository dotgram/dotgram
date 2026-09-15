using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <c>Sql2023Ast.BnfMap.csv</c> says where every rule of the SQL:2023 BNF goes in the tree, and it has
/// to keep saying it (docs/design/sql-ast.md, requirement 21).
/// </summary>
/// <remarks>
/// Its <c>AST</c> column names, for every rule, a type (<c>Expression.Cast</c>), a property
/// (<c>Expression.Invocation.Quantifier</c>), an enum or one of its members (<c>SortDirection.Desc</c>),
/// several of those separated by semicolons, or one of two words: <c>erased</c>, a rule that carries no
/// information of its own, and <c>token</c>, a fixed spelling its parent's node already says. The names
/// are read against the tree by reflection, so a node renamed or removed leaves a row that fails here.
/// </remarks>
public sealed class SqlStandardAstMapTests
{
	[Fact]
	public void Every_rule_of_the_BNF_has_a_row()
	{
		var mapped  = Rows().Select(one => one.Rule).ToHashSet(StringComparer.Ordinal);
		var missing = Productions().Where(one => !mapped.Contains(one)).ToArray();

		Assert.Empty(missing);
	}

	/// <summary>And no row is for a rule the BNF does not have.</summary>
	[Fact]
	public void And_every_row_is_a_rule_of_the_BNF()
	{
		var productions = Productions().ToHashSet(StringComparer.Ordinal);
		var stale       = Rows().Select(one => one.Rule).Where(one => !productions.Contains(one)).ToArray();

		Assert.Empty(stale);
	}

	[Fact]
	public void Every_row_says_where_its_rule_goes()
	{
		var unsaid = Rows().Where(one => one.Targets.Length == 0).Select(one => one.Rule).ToArray();

		Assert.Empty(unsaid);
	}

	[Fact]
	public void Every_place_a_row_names_is_in_the_tree()
	{
		var unresolved = Rows()
			.SelectMany(row => row.Targets.Where(target => target is not ("erased" or "token") && !Resolves(target)).Select(target => row.Rule + " -> " + target))
			.ToArray();

		Assert.Empty(unresolved);
	}

	/// <summary>A type, a nested type, a property, or an enum's member, followed from the namespace down.</summary>
	static bool Resolves(string path)
	{
		var parts = path.Split('.');
		var type  = typeof(ISqlNode).Assembly.GetType(typeof(ISqlNode).Namespace + "." + parts[0]);

		if (type is null)
			return false;

		for (var at = 1; at < parts.Length; at++)
		{
			var nested = type.GetNestedType(parts[at], BindingFlags.Public);

			if (nested is not null)
			{
				type = nested;
				continue;
			}

			if (at != parts.Length - 1)
				return false;

			return type.IsEnum
				? Enum.GetNames(type).Contains(parts[at], StringComparer.Ordinal)
				: type.GetProperty(parts[at], BindingFlags.Public | BindingFlags.Instance) is not null;
		}

		return true;
	}

	/// <summary>Every production of the Foundation's BNF, by the name between its angle brackets.</summary>
	static IEnumerable<string> Productions()
	{
		var text = File.ReadAllText(Path.Combine(Standard(), "Specification", "ISO_IEC_9075-2(E)_Foundation.bnf.txt"));

		return Regex.Matches(text, @"^<([^>]+)> ::=", RegexOptions.Multiline).Select(one => one.Groups[1].Value).Distinct(StringComparer.Ordinal);
	}

	static List<(string Rule, string[] Targets)> Rows()
	{
		var lines  = File.ReadAllLines(Path.Combine(Standard(), "Specification", "Sql2023Ast.BnfMap.csv"));
		var header = Fields(lines[0]);
		var rule   = Array.IndexOf(header, "BNF rule");
		var ast    = Array.IndexOf(header, "AST");
		var rows   = new List<(string, string[])>();

		foreach (var line in lines.Skip(1).Where(one => one.Length > 0))
		{
			var fields  = Fields(line);
			var targets = fields[ast].Split(';').Select(one => one.Trim()).Where(one => one.Length > 0).ToArray();

			rows.Add((fields[rule], targets));
		}

		return rows;
	}

	/// <summary>One line of the map, whose fields may be quoted and hold commas and doubled quotes.</summary>
	static string[] Fields(string line)
	{
		var fields = new List<string>();
		var field  = new System.Text.StringBuilder();
		var quoted = false;

		for (var at = 0; at < line.Length; at++)
		{
			var c = line[at];

			if (quoted)
			{
				if (c == '"' && at + 1 < line.Length && line[at + 1] == '"')
				{
					field.Append('"');
					at++;
				}
				else if (c == '"')
					quoted = false;
				else
					field.Append(c);
			}
			else if (c == '"')
				quoted = true;
			else if (c == ',')
			{
				fields.Add(field.ToString());
				field.Clear();
			}
			else
				field.Append(c);
		}

		fields.Add(field.ToString());

		return fields.ToArray();
	}

	static string Standard()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return Path.Combine(at?.FullName ?? AppContext.BaseDirectory, "src", "DotGram.Sql", "Standard");
	}
}
