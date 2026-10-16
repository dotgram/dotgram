using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// A value an enum does not name, put where the writer reads one: no text says it, so none may be written. Each statement
/// is read and carried to the common tree, and then every enum member of it in turn is given a value no enum of the tree
/// has; each writer, T-SQL's and the standard's, refuses the tree, or the test names the member it wrote text for.
/// </summary>
public sealed class UndefinedEnumTests
{
	/// <summary>Statements that between them hold the enums that choose a word: an order, a bound, a kind of key, a row word, a quantifier.</summary>
	public static readonly string[] Statements =
	[
		"SELECT t::a, t2::f(), (a.b()).A",
		"SELECT id, COUNT(c) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND 1 FOLLOWING), SUM(DISTINCT a) FROM t ORDER BY id DESC",
		"SELECT DISTINCT a FROM t WHERE a != 1 AND b < 2 ORDER BY a OFFSET 5 ROWS FETCH NEXT 2 ROWS ONLY",
		"SELECT FIRST_VALUE(m) IGNORE NULLS OVER ()",
		"SELECT JSON_OBJECT('a' : 1 ABSENT ON NULL), JSON_ARRAY(1 ABSENT ON NULL)",
		"SELECT JSON_OBJECTAGG(c1 : c2 ABSENT ON NULL), JSON_ARRAYAGG(c1 ORDER BY c1 NULL ON NULL) FROM t",
		"SELECT * FROM fun2 TABLESAMPLE SYSTEM (12 PERCENT)",
		"SELECT Title FROM Production.Document WHERE FREETEXT((t2.*), N'abc')",
		"CREATE TABLE t (a INT PRIMARY KEY, b INT UNIQUE, d DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL, e DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL, PERIOD FOR SYSTEM_TIME (d, e), INDEX i (a DESC))",
		"CREATE PROCEDURE p @a INT OUTPUT AS SELECT 1",
		"CREATE TRIGGER dbo.tr ON dbo.t AFTER INSERT AS RETURN",
		"DECLARE c1 INSENSITIVE SCROLL CURSOR FOR SELECT * FROM t1",
		"DROP FUNCTION x, y",
		"REVOKE GRANT OPTION FOR EXECUTE ON EXTERNAL MODEL::MyModel TO tempuser CASCADE",
	];

	/// <summary>The members the writer does not yet refuse, as <c>Record.Member</c>, each with what it writes instead.</summary>
	static readonly Dictionary<string, string> Unrefused = new(StringComparer.Ordinal)
	{
		// The standard's writer takes the style and the kind of a literal from its text, which holds the delimiters and the digits; T-SQL's refuses them.
		["Identifier.Style (the standard)"] = "the identifier's text",
		["Numeric.Kind (the standard)"]     = "the numeric literal's text",
	};

	const int Beyond = 1000;

	/// <summary>The tree with the <paramref name="at"/>th enum member (counted in walk order) given a value beyond the enum's.</summary>
	static ISqlNode? Mutated(ISqlNode tree, int at, out string site)
	{
		var seen  = 0;
		var found = "";
		var made  = Map(tree, "", (value, where) =>
		{
			if (seen++ != at)
				return null;

			found = where;

			return Enum.ToObject(value.GetType(), Beyond);
		});

		site = found;

		return found.Length == 0 ? null : (ISqlNode)made;
	}

	/// <summary>The number of enum members a tree holds.</summary>
	static int Members(ISqlNode tree)
	{
		var count = 0;

		Map(tree, "", (_, _) =>
		{
			count++;

			return null;
		});

		return count;
	}

	/// <summary>
	/// A copy of a node with every enum member passed to <paramref name="change"/>, which answers the value to put there or null to leave it:
	/// a record cloned and its properties set, a list rebuilt item by item, a name rebuilt part by part.
	/// </summary>
	static object Map(object node, string owner, Func<object, string, object?> change)
	{
		if (node is QualifiedName name)
			return new QualifiedName(SqlList.From(Enumerable.Range(0, name.Count).Select(at => (Identifier)Map(name[at], owner, change)).ToArray()));

		var type = node.GetType();

		if (node is not ISqlNode || type.Namespace != typeof(DotGram.Sql.Ast.Statement).Namespace || !type.IsClass || type.GetMethod("<Clone>$") is not { } clone)
			return node;

		object? copy = null;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!property.CanWrite || property.GetIndexParameters().Length > 0 || property.GetValue(node) is not { } value)
				continue;

			var where = type.Name + "." + property.Name;
			object? made = null;

			if (value.GetType().IsEnum)
				made = change(value, where);
			else if (value is ISqlNode)
				made = Map(value, where, change);
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(SqlList<>))
			{
				var item   = property.PropertyType.GetGenericArguments()[0];
				var items  = ((IEnumerable)value).Cast<object>().ToArray();
				var mapped = items.Select(one => one is ISqlNode ? Map(one, where, change) : one).ToArray();

				if (!mapped.Zip(items, ReferenceEquals).All(static same => same))
				{
					var typed = Array.CreateInstance(item, mapped.Length);

					Array.Copy(mapped, typed, mapped.Length);
					made = typeof(SqlList).GetMethod(nameof(SqlList.From))!.MakeGenericMethod(item).Invoke(null, [typed]);
				}
			}

			if (made is not null && !ReferenceEquals(made, value))
			{
				copy ??= clone.Invoke(node, null)!;
				property.SetValue(copy, made);
			}
		}

		return copy ?? node;
	}

	[Fact]
	public void An_enum_value_no_enum_names_is_refused()
	{
		var written = new List<string>();
		var members = 0;

		foreach (var text in Statements)
		{
			var tree  = (ISqlNode)Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
			var count = Members(tree);

			Assert.True(count > 0, text);
			members += count;

			for (var at = 0; at < count; at++)
			{
				var changed = Mutated(tree, at, out var site)!;

				foreach (var (write, writer) in new (Func<ISqlNode, string>, string)[] { (TransactSqlWriter.Write, "T-SQL"), (Sql2023Writer.Write, "the standard") })
					try
					{
						written.Add(site + " (" + writer + ") -> " + write(changed));
					}
					catch (Exception thrown) when (thrown is ArgumentException or NotSupportedException or InvalidOperationException)
					{
					}
			}
		}

		Assert.True(members > 40, "the statements hold " + members + " enum members; this test expects them to hold the enums that choose a word");

		var sites      = written.Select(static one => one.Substring(0, one.IndexOf(" -> ", StringComparison.Ordinal))).Distinct().ToList();
		var unexpected = sites.Where(one => !Unrefused.ContainsKey(one)).Select(one => written.First(text => text.StartsWith(one + " -> ", StringComparison.Ordinal))).ToList();
		var fixedNow   = Unrefused.Keys.Where(one => !sites.Contains(one)).ToList();

		Assert.True(unexpected.Count == 0, "the writer wrote text for a value no enum names:\n" + string.Join("\n", unexpected.Order(StringComparer.Ordinal)));
		Assert.True(fixedNow.Count == 0, "the writer now refuses these, so they are no longer unrefused:\n" + string.Join("\n", fixedNow));
	}
}
