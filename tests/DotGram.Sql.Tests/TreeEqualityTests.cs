using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// Trees compare by what was written: a tree equals its own reparse, located or not, and a change
/// anywhere in it, however deep, makes it unequal.
/// </summary>
/// <remarks>
/// A record compares its fields, and the lists a node holds are <see cref="SqlList{T}"/>, which
/// compare element by element; the span is held where equality does not look. These tests hold the
/// whole of that to the corpus, the hash to sets of statements that differ in one element in the
/// middle of a list — where a hash that skipped elements would collide — and the recursion to a tree
/// deeper than any stack.
/// </remarks>
public sealed class TreeEqualityTests
{
	/// <summary>
	/// Every statement of the corpus equals its own reparse and its located reading, with an equal
	/// hash; the distinct statements hash apart; and changing the deepest literal of one makes it
	/// unequal, with a different hash.
	/// </summary>
	[Fact]
	public void Every_corpus_statement_equals_its_own_reparse()
	{
		var root       = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files      = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var statements = 0;
		var changed    = 0;
		var collisions = 0;
		var distinct   = new HashSet<Statement>();

		Assert.True(files.Length > 1000, root);

		foreach (var file in files)
		{
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var first   = TransactSqlParser.TryParseSql(batch);
				var second  = TransactSqlParser.TryParseSql(batch);
				var located = TransactSqlParser.Located.TryParseSql(batch);

				if (!first.IsSuccess)
					continue;

				Assert.Equal(first.Value.Length, second.Value.Length);
				Assert.Equal(first.Value.Length, located.Value.Length);

				for (var i = 0; i < first.Value.Length; i++)
				{
					var statement = first.Value[i];

					Assert.NotSame(statement, second.Value[i]);
					Assert.Equal(statement, second.Value[i]);
					Assert.Equal(statement.GetHashCode(), second.Value[i].GetHashCode());
					Assert.Equal(statement, located.Value[i]);
					Assert.Equal(statement.GetHashCode(), located.Value[i].GetHashCode());

					statements++;
					distinct.Add(statement);

					if (Deepest(statement) is not { } literal)
						continue;

					var edited = (Statement)Replace(statement, literal, literal with { Text = literal.Text + "0" });

					Assert.NotEqual(statement, edited);
					Assert.NotEqual(edited, second.Value[i]);

					changed++;

					if (edited.GetHashCode() == statement.GetHashCode())
						collisions++;
				}
			}
		}

		Assert.True(statements > 6_000, $"{statements} statements");
		Assert.True(changed > 2_000, $"{changed} statements with a literal");
		Assert.Equal(0, collisions);
		Assert.True(distinct.Count > 4_000, $"{distinct.Count} distinct statements");
		Assert.Equal(distinct.Count, distinct.Select(static one => one.GetHashCode()).Distinct().Count());
	}

	/// <summary>
	/// Statements that differ in one element in the middle of a list — what machine-written SQL
	/// varies in — all hash apart. A hash of a list's length, or of its ends, gives each set one value.
	/// </summary>
	[Theory]
	[InlineData("SELECT c FROM t WHERE id = {0}", 10_000)]
	[InlineData("SELECT c FROM t WHERE id IN (1, {0}, 3)", 10_000)]
	[InlineData("SELECT a, b, {0}, d, e FROM t", 5_000)]
	[InlineData("INSERT INTO t VALUES (0), (1), (2), (3), (4), (5), ({0}), (7), (8), (9), (10), (11), (12), (13), (14), (15), (16), (17), (18), (19)", 5_000)]
	[InlineData("SELECT f(1, 2, 'x{0}', 4) FROM t", 5_000)]
	public void Statements_that_differ_in_one_middle_element_hash_apart(string template, int count)
	{
		var hashes = new HashSet<int>();

		for (var i = 0; i < count; i++)
			hashes.Add(TransactSqlParser.ParseStatement(string.Format(template, i)).GetHashCode());

		Assert.Equal(count, hashes.Count);
	}

	/// <summary>
	/// The list's hash is ordered and counts every element: the order of two elements matters, and
	/// two equal elements do not cancel, as they would under an exclusive or.
	/// </summary>
	[Fact]
	public void A_list_hash_is_ordered_and_counts_duplicates()
	{
		var a = new Expression.Literal(SqlLiteralKind.Number, "1");
		var b = new Expression.Literal(SqlLiteralKind.Number, "2");

		SqlList<Expression> ab  = [a, b];
		SqlList<Expression> ba  = [b, a];
		SqlList<Expression> aa  = [a, a];
		SqlList<Expression> bb  = [b, b];
		SqlList<Expression> aab = [a, a, b];
		SqlList<Expression> one = [b];

		Assert.NotEqual(ab, ba);
		Assert.NotEqual(ab.GetHashCode(), ba.GetHashCode());
		Assert.NotEqual(aa.GetHashCode(), bb.GetHashCode());
		Assert.NotEqual(aab.GetHashCode(), one.GetHashCode());
		Assert.NotEqual(new Expression.RowValueConstructor(ab), new Expression.RowValueConstructor(ba));
		Assert.NotEqual(new Expression.RowValueConstructor(ab).GetHashCode(), new Expression.RowValueConstructor(ba).GetHashCode());
		Assert.Equal(new Expression.RowValueConstructor(ab), new Expression.RowValueConstructor([new Expression.Literal(SqlLiteralKind.Number, "1"), b]));

		SqlList<int> numbers  = [1, 2];
		SqlList<int> reversed = [2, 1];
		SqlList<int> zeros    = [0, 0];
		SqlList<int> ones     = [1, 1];

		Assert.NotEqual(numbers.GetHashCode(), reversed.GetHashCode());
		Assert.NotEqual(zeros.GetHashCode(), ones.GetHashCode());
	}

	/// <summary>
	/// <c>default</c> is the empty list, equal to <c>[]</c> and hashing alike; a list a record may
	/// leave out is a <see cref="Nullable{T}"/>, and there null is not empty.
	/// </summary>
	[Fact]
	public void The_default_list_is_empty_and_null_is_not()
	{
		SqlList<string>  empty   = [];
		SqlList<string>  none    = default;
		SqlList<string>? missing = null;

		Assert.Equal(empty, none);
		Assert.True(empty == none);
		Assert.Equal(empty.GetHashCode(), none.GetHashCode());
		Assert.Equal(empty.GetHashCode(), SqlList.From(Array.Empty<string>()).GetHashCode());
		Assert.True(none.IsEmpty);
		Assert.Empty(none);
		Assert.Equal("[]", none.ToString());
		Assert.Throws<IndexOutOfRangeException>(() => none[0]);
		Assert.NotEqual<SqlList<string>?>(empty, missing);

		var table   = new TableReference.Named("t", null, false, null, null, null, Clause.None);
		var query   = new Query.TableValueConstructor([new Expression.RowValueConstructor([new Expression.Literal(SqlLiteralKind.Number, "1")])]);
		var listed  = new Statement.Insert(table, [], query);
		var omitted = new Statement.Insert(table, null, query);

		Assert.NotEqual(listed, omitted);
		Assert.Equal(omitted, new Statement.Insert(table, null, query));
		Assert.Equal(listed, new Statement.Insert(table, default(SqlList<string>), query));
	}

	/// <summary>
	/// A list is read only and nobody else holds its array: both ways of making one copy, so changing
	/// what it was made from changes nothing in it.
	/// </summary>
	[Fact]
	public void A_list_is_a_copy_of_what_it_was_made_from()
	{
		var source = new[] { "a", "b", "c" };
		var list   = SqlList.From(source);

		source[0] = "z";

		Assert.Equal(new[] { "a", "b", "c" }, list);
		Assert.Equal(3, list.Length);
		Assert.Equal("b", list[1]);
		Assert.Equal("[a, b, c]", list.ToString());
		Assert.Equal(new[] { "b", "c" }, list.AsSpan()[1..].ToArray());

		var tail = list is ["a", .. var rest] ? rest : default;

		Assert.Equal(SqlList.From(new[] { "b", "c" }), tail);
		Assert.True(list.Exists(static one => one == "c"));
		Assert.False(list.TrueForAll(static one => one == "a"));
		Assert.Equal(list, new List<string> { "a", "b", "c" }.ToSqlList());
	}

	/// <summary>
	/// A copy made with <c>with</c> and changed is unequal; changed back, it is equal again and
	/// hashes alike, whatever the spans say.
	/// </summary>
	[Fact]
	public void A_copy_changed_and_changed_back_is_equal_again()
	{
		var located = (Query.Specification)((Statement.Select)TransactSqlParser.Located.ParseStatement("SELECT a, b FROM t WHERE a = 1")).Of;
		var changed = located with { Columns = [.. located.Columns.Skip(1)] };
		var back    = changed with { Columns = located.Columns };

		Assert.NotEqual(located, changed);
		Assert.Equal(located, back);
		Assert.Equal(located.GetHashCode(), back.GetHashCode());
		Assert.True(back.Span.IsStale);
	}

	/// <summary>
	/// Comparing or hashing a tree deeper than the stack throws an exception a caller can catch,
	/// instead of a stack overflow that ends the process; a deep tree that fits compares as usual.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_tree_deeper_than_the_stack_throws_a_catchable_exception(bool hash)
	{
		var left  = Chain(200_000);
		var right = Chain(200_000);

		Exception? caught = null;

		var thread = new Thread(() =>
		{
			try
			{
				if (hash)
					left.GetHashCode();
				else
					left.Equals(right);
			}
			catch (Exception exception)
			{
				caught = exception;
			}
		}, 1024 * 1024);

		thread.Start();
		thread.Join();

		Assert.IsType<InsufficientExecutionStackException>(caught);

		Assert.Equal(Chain(1_000), Chain(1_000));
		Assert.Equal(Chain(1_000).GetHashCode(), Chain(1_000).GetHashCode());
		Assert.NotEqual(Chain(1_000), Chain(1_001));
	}

	static Expression Chain(int depth)
	{
		Expression made = new Expression.Literal(SqlLiteralKind.Number, "1");

		for (var i = 0; i < depth; i++)
			made = new Expression.Add(made, new Expression.Literal(SqlLiteralKind.Number, "2"));

		return made;
	}

	/// <summary>The deepest numeric or text literal of a tree, or null where it has none.</summary>
	static Expression.Literal? Deepest(ISqlSpan root)
	{
		Expression.Literal? found = null;
		var deepest = -1;

		Visit(root, 0);

		return found;

		void Visit(object value, int depth)
		{
			if (value is Expression.Literal { Kind: SqlLiteralKind.Number or SqlLiteralKind.Text or SqlLiteralKind.National } literal && depth > deepest)
			{
				found   = literal;
				deepest = depth;
			}

			foreach (var held in Held(value))
				Visit(held, depth + 1);
		}
	}

	/// <summary>
	/// The tree with <paramref name="target"/> replaced, each node on the way to it copied with
	/// <c>with</c> — the way a rewrite would build it — and everything else shared.
	/// </summary>
	static object Replace(object value, ISqlSpan target, ISqlSpan replacement)
	{
		if (ReferenceEquals(value, target))
			return replacement;

		if (value is ISqlSpan node)
		{
			object? copy = null;

			foreach (var property in Writable(node.GetType()))
			{
				var held = property.GetValue(node);

				if (held is null)
					continue;

				var made = Replace(held, target, replacement);

				if (ReferenceEquals(made, held))
					continue;

				copy ??= node.GetType().GetMethod("<Clone>$")!.Invoke(node, null);
				property.SetValue(copy, made);
			}

			return copy ?? node;
		}

		if (value.GetType() is { IsGenericType: true } type && type.GetGenericTypeDefinition() == typeof(SqlList<>))
		{
			var items   = ((IEnumerable)value).Cast<object>().ToArray();
			var changed = false;

			for (var i = 0; i < items.Length; i++)
			{
				var made = Replace(items[i], target, replacement);

				changed |= !ReferenceEquals(made, items[i]);
				items[i] = made;
			}

			if (!changed)
				return value;

			var element = type.GetGenericArguments()[0];
			var typed   = Array.CreateInstance(element, items.Length);

			Array.Copy(items, typed, items.Length);

			return typeof(SqlList).GetMethod(nameof(SqlList.From))!.MakeGenericMethod(element).Invoke(null, [typed])!;
		}

		return value;
	}

	/// <summary>The nodes and lists a node holds directly.</summary>
	static IEnumerable<object> Held(object value)
	{
		if (value is ISqlSpan node)
		{
			foreach (var property in Writable(node.GetType()))
				if (property.GetValue(node) is { } held && held is not string)
					yield return held;
		}
		else if (value is IEnumerable list and not string)
		{
			foreach (var item in list)
				if (item is not null)
					yield return item;
		}
	}

	static IEnumerable<PropertyInfo> Writable(Type type)
	{
		return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(static property => property.CanWrite && property.GetIndexParameters().Length == 0 && Holds(property.PropertyType));
	}

	static bool Holds(Type type)
	{
		type = Nullable.GetUnderlyingType(type) ?? type;

		return typeof(ISqlSpan).IsAssignableFrom(type) ||
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SqlList<>) && Holds(type.GetGenericArguments()[0]);
	}

	static string Here([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}
}
