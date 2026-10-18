using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Sql;
using DotGram.Sql.Standard;
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
	/// hash; and changing the deepest literal of one makes it unequal. The distinct statements hash
	/// apart, and so do the changed ones from their originals, as a 32-bit hash does: string hashes
	/// are seeded per process, so a collision is allowed at a rate a good hash stays far below
	/// (<see cref="Tolerated"/>), never demanded to be zero.
	/// </summary>
	[Fact]
	public void Every_corpus_statement_equals_its_own_reparse()
	{
		var root       = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files      = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var statements = 0;
		var changed    = 0;
		var collisions = 0;
		var distinct   = new HashSet<Ast.Statement>();

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

					var edited = (Ast.Statement)Replace(statement, literal, Retexted(literal));

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
		Assert.True(collisions <= Tolerated(changed), $"{collisions} of {changed} changed statements hash as before");
		Assert.True(distinct.Count > 3_500, $"{distinct.Count} distinct statements");

		// The common tree's records hash a member left out as its default and the same member written as it (`ROLLBACK`
		// and `ROLLBACK TRAN`, `VECTOR(1)` and `VECTOR(1, FLOAT32)`, `ORDER BY a` and `ORDER BY a ASC`), which equality
		// tells apart and the corpus writes both ways; a statement the tree has no place for is an extension, left out.
		var own    = distinct.Where(static one => one is not Ast.Statement.Extension).ToList();
		var hashed = own.Select(static one => one.GetHashCode()).Distinct().Count();

		Assert.True(own.Count - hashed <= Spelled(own.Count), $"{own.Count} distinct statements, {hashed} distinct hashes");
	}

	/// <summary>
	/// Every statement the corpus holds equals the reparse of what <see cref="Ast.TransactSqlWriter"/> wrote of
	/// it: not merely the same text by some other printer's judgment (<c>CorpusRoundTripTests</c>,
	/// held to ScriptDom's own), but the same tree TransactSqlParser itself reads back.
	/// </summary>
	[Fact]
	public void Every_corpus_statement_is_the_same_tree_after_the_writer_and_a_reparse()
	{
		var root  = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var read  = 0;
		var faults = new List<string>();

		Assert.True(files.Length > 1000, root);

		foreach (var file in files)
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var first = TransactSqlParser.TryParseSql(batch);

				if (!first.IsSuccess)
					continue;

				foreach (var statement in first.Value)
				{
					read++;

					// A statement the tree has no place for: the writer has it not to write.
					if (statement is Ast.Statement.Extension)
						continue;

					string written;

					try
					{
						written = Ast.TransactSqlWriter.Write(statement);
					}
					catch (Exception failure)
					{
						faults.Add($"the writer threw {failure.GetType().Name} on a statement it had just read: {failure.Message}");

						continue;
					}

					var reparsed = TransactSqlParser.TryParseStatement(written);

					if (!reparsed.IsSuccess)
						faults.Add($"printed as `{written}`, which the parser refuses to read back");
					else if (!Equals(statement, reparsed.Value))
						faults.Add($"printed as `{written}`, which reparses to a different tree");
				}
			}

		Assert.True(read > 6_000, $"{read} statements");
		Assert.Equal([], faults);
	}

	/// <summary>
	/// Every query the corpus holds that SQL-92 can read is the same tree after
	/// <see cref="Ast.Sql2023Writer"/> writes it and <see cref="Sql92Parser"/> reads it back.
	/// </summary>
	/// <remarks>
	/// SQL-92 has no cutter of its own for somebody else's T-SQL: this asks what
	/// <see cref="TransactSqlParser"/> already read and <see cref="Ast.TransactSqlWriter"/> already printed, in the
	/// walk above, which <see cref="Sql92Parser"/> happens to read as well — a query plain enough to be
	/// both. The corpus is T-SQL, so most of it is not; what SQL-92 refuses here is a dialect
	/// difference and not a defect, exactly as <c>Sql92ParserTests</c> already treats it.
	/// </remarks>
	[Fact]
	public void Every_SQL92_query_the_corpus_holds_is_the_same_tree_after_the_writer_and_a_reparse()
	{
		var root  = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var read  = 0;
		var faults = new List<string>();

		foreach (var file in files)
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var first = TransactSqlParser.TryParseSql(batch);

				if (!first.IsSuccess)
					continue;

				foreach (var statement in first.Value)
				{
					if (statement is not Ast.Statement.Select)
						continue;

					string candidate;

					try
					{
						candidate = Ast.TransactSqlWriter.Write(statement);
					}
					catch (Exception)
					{
						continue; // The writer's own fault, held above; not this grammar's to answer for.
					}

					var firstRead = Sql92Parser.TryParseSelect(candidate);

					if (!firstRead.IsSuccess)
						continue; // T-SQL syntax SQL-92 does not have: a dialect difference, not this test's business.

					read++;

					string written;

					try
					{
						written = Ast.Sql2023Writer.Write(firstRead.Value!);
					}
					catch (Exception failure)
					{
						faults.Add($"the writer threw {failure.GetType().Name} on `{candidate}`: {failure.Message}");

						continue;
					}

					var secondRead = Sql92Parser.TryParseSelect(written);

					if (!secondRead.IsSuccess)
						faults.Add($"`{candidate}` printed as `{written}`, which SQL-92 refuses to read back");
					else if (!Equals(firstRead.Value, secondRead.Value) && !Sql92TreeTests.IsWrittenAsAnotherTree(candidate, written, secondRead.Value!))
						faults.Add($"`{candidate}` printed as `{written}`, which reparses to a different tree");
				}
			}

		Assert.True(read > 50, $"{read} SQL-92 queries found in the corpus");
		Assert.Equal([], faults);
	}

	/// <summary>
	/// Every query the corpus holds that SQL:2023 can read is the same tree after
	/// <see cref="Ast.Sql2023Writer"/> writes it and <see cref="SqlStandardParser"/> reads it back.
	/// </summary>
	/// <remarks>
	/// The same corpus of T-SQL, asked the same way as the SQL-92 test above: most of it is not
	/// SQL:2023 either, and what is read here is only what both dialects happen to agree on.
	/// </remarks>
	[Fact]
	public void Every_SQL2023_query_the_corpus_holds_is_the_same_tree_after_the_writer_and_a_reparse()
	{
		var root  = Path.GetFullPath(Path.Combine(Here(), "..", "Corpus", "ScriptDom"));
		var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var read  = 0;
		var faults = new List<string>();

		foreach (var file in files)
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var first = TransactSqlParser.TryParseSql(batch);

				if (!first.IsSuccess)
					continue;

				foreach (var statement in first.Value)
				{
					if (statement is not Ast.Statement.Select)
						continue;

					string candidate;

					try
					{
						candidate = Ast.TransactSqlWriter.Write(statement);
					}
					catch (Exception)
					{
						continue; // The writer's own fault, held above; not this grammar's to answer for.
					}

					var firstRead = SqlStandardParser.TryParseStatement(candidate);

					if (!firstRead.IsSuccess)
						continue; // T-SQL syntax SQL:2023 does not have: a dialect difference, not this test's business.

					read++;

					string written;

					try
					{
						written = Ast.Sql2023Writer.Write(firstRead.Value!);
					}
					catch (Exception failure)
					{
						faults.Add($"the writer threw {failure.GetType().Name} on `{candidate}`: {failure.Message}");

						continue;
					}

					var secondRead = SqlStandardParser.TryParseStatement(written);

					if (!secondRead.IsSuccess)
						faults.Add($"`{candidate}` printed as `{written}`, which SQL:2023 refuses to read back");
					else if (!Equals(firstRead.Value, secondRead.Value))
						faults.Add($"`{candidate}` printed as `{written}`, which reparses to a different tree");
				}
			}

		Assert.True(read > 100, $"{read} SQL:2023 queries found in the corpus");
		Assert.Equal([], faults);
	}

	/// <summary>
	/// Statements that differ in one element in the middle of a list — what machine-written SQL
	/// varies in — hash apart, but for the odd collision a 32-bit hash may have. A hash of a list's
	/// length, or of its ends, gives each set one value.
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

		Assert.True(count - hashes.Count <= Tolerated(count), $"{hashes.Count} distinct hashes of {count} statements");
	}

	/// <summary>
	/// The collisions allowed among <paramref name="count"/> distinct values: one in a thousand,
	/// and at least one. A uniform 32-bit hash collides about count² / 2³³ times — 0.01 times
	/// among ten thousand — so a hash that skips elements fails by orders of magnitude, while
	/// the odd seeded collision does not fail the run.
	/// </summary>
	static int Tolerated(int count)
	{
		return Math.Max(1, count / 1000);
	}

	/// <summary>
	/// The collisions allowed among distinct statements of the corpus, which writes a good many of them twice, once
	/// with a member left at its default and once with it written out: about one in a hundred and fifty does.
	/// </summary>
	static int Spelled(int count)
	{
		return Math.Max(1, count / 100);
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
		Assert.True(list.Slice(3, 0).IsEmpty);
		Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(-1, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(4, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(1, 3));
		Assert.Throws<ArgumentOutOfRangeException>(() => default(SqlList<string>).Slice(1, 0));
		Assert.True(list.Exists(static one => one == "c"));
		Assert.False(list.TrueForAll(static one => one == "a"));
		Assert.Equal(list, new List<string> { "a", "b", "c" }.ToSqlList());
	}

	/// <summary>
	/// A factory that takes an array copies it: changing the array afterwards changes neither the
	/// node nor its hash.
	/// </summary>
	[Fact]
	public void A_factory_keeps_no_handle_on_the_array_it_was_given()
	{
		var parts = new[] { new Ast.Identifier("a"), new Ast.Identifier("b") };
		var name  = Ast.QualifiedName.Of(parts);
		var hash  = name.GetHashCode();

		parts[0] = new Ast.Identifier("z");

		Assert.Equal(Ast.QualifiedName.Of(new Ast.Identifier("a"), new Ast.Identifier("b")), name);
		Assert.Equal(hash, name.GetHashCode());
	}

	/// <summary>
	/// Two readings of one script give equal batches: where a batch was read from takes no part in
	/// its equality, as a span takes none in a node's.
	/// </summary>
	[Fact]
	public void Two_readings_of_a_script_give_equal_batches()
	{
		const string script = "SELECT 1\nGO\nSELECT a FROM t WHERE b IN (1, 2)\nGO 2\n";

		var first  = TransactSqlParser.ParseScript(script);
		var second = TransactSqlParser.ParseScript(script);

		Assert.Equal(2, first.Length);
		Assert.NotSame(first[1].Source, second[1].Source);
		Assert.Equal(first, second);
		Assert.Equal(first[1].GetHashCode(), second[1].GetHashCode());
		Assert.NotEqual(first[0], first[1]);
	}

	/// <summary>
	/// A copy made with <c>with</c> and changed is unequal; changed back, it is equal again and
	/// hashes alike, whatever the spans say.
	/// </summary>
	[Fact]
	public void A_copy_changed_and_changed_back_is_equal_again()
	{
		var located = (Ast.Statement.Update)TransactSqlParser.Located.ParseStatement("UPDATE t SET a = 1, b = 2 WHERE a = 1");
		var changed = located with { Assignments = [.. located.Assignments.Skip(1)] };
		var back    = changed with { Assignments = located.Assignments };

		Assert.NotEqual(located, changed);
		Assert.Equal(located, back);
		Assert.Equal(located.GetHashCode(), back.GetHashCode());
		Assert.Equal(located.Span, back.Span);
	}

	/// <summary>
	/// Every member through which a node can hold another of its own type, in either tree: found
	/// by reflection, so a record added later is asked too.
	/// </summary>
	public static TheoryData<string, string> RecursiveMembers()
	{
		var rows = new TheoryData<string, string>();

		foreach (var type in typeof(SqlList).Assembly.GetExportedTypes().Where(static type => type.IsClass && !type.IsAbstract && type.GetMethod("<Clone>$") is not null))
		{
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (BackingField(type, property) is not null && Element(property.PropertyType).IsAssignableFrom(type))
					rows.Add(type.FullName!, property.Name);
			}
		}

		return rows;
	}

	/// <summary>
	/// A chain through any such member deeper than the stack has room for never ends the process:
	/// GetHashCode throws a catchable exception, before the first node that has no room hashes
	/// what it holds — whatever else the nodes hold, and whether the member is a node or a list
	/// of them — and Equals of two such chains either throws the same or answers true.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The hash has to walk the whole chain: each node combines its child's hash after the call
	/// returns. Equals need not. Where the recursive member is the last thing a record compares —
	/// <c>Right</c> of a multiset operation, <c>Group</c> of a regular expression — the JIT may
	/// make that comparison a tail call, and then a chain of any depth is compared in constant
	/// stack: no exception, and the right answer. Whether it does differs between Linux and
	/// Windows, so Equals is held only to never crashing and to being right when it finishes.
	/// </para>
	/// <para>
	/// How deep is deep enough is found rather than assumed: the stack a thread is given is
	/// rounded up differently by each platform, so the depth doubles from twenty thousand until
	/// the hash throws, and a chain that still hashes at over a million nodes on a small thread
	/// is a failure. A record that missed its check would not get that far: it would overflow
	/// the stack and end the run.
	/// </para>
	/// </remarks>
	[Theory]
	[MemberData(nameof(RecursiveMembers))]
	public void A_chain_through_any_recursive_member_throws_a_catchable_exception(string typeName, string propertyName)
	{
		var type     = typeof(SqlList).Assembly.GetType(typeName)!;
		var property = type.GetProperty(propertyName)!;
		var depth    = 20_000;

		Exception? thrown;

		while ((thrown = OnSmallStack(Chain(type, property, depth).GetHashCode).Thrown) is null)
		{
			Assert.True(depth < 1_000_000, $"{depth} nodes hashed on a small thread without the stack running out");

			depth *= 2;
		}

		Assert.IsType<InsufficientExecutionStackException>(thrown);

		var left   = Chain(type, property, depth);
		var right  = Chain(type, property, depth);
		var equals = OnSmallStack(() => left.Equals(right));

		if (equals.Thrown is not null)
			Assert.IsType<InsufficientExecutionStackException>(equals.Thrown);
		else
			Assert.True(equals.Answer);

		Assert.Equal(Chain(type, property, 100), Chain(type, property, 100));
		Assert.Equal(Chain(type, property, 100).GetHashCode(), Chain(type, property, 100).GetHashCode());
	}

	/// <summary>
	/// Runs <paramref name="function"/> on a thread asked for a quarter of a megabyte of stack, and
	/// says what it answered or threw.
	/// </summary>
	static (T Answer, Exception? Thrown) OnSmallStack<T>(Func<T> function)
	{
		T          answer = default!;
		Exception? caught = null;

		var thread = new Thread(() =>
		{
			try
			{
				answer = function();
			}
			catch (Exception exception)
			{
				caught = exception;
			}
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		return (answer, caught);
	}

	/// <summary>
	/// <paramref name="depth"/> nodes of <paramref name="type"/>, each holding the one before it in
	/// <paramref name="property"/> and nothing else: every other member is left as a reading that
	/// never wrote it would leave it.
	/// </summary>
	static object Chain(Type type, PropertyInfo property, int depth)
	{
		var field = BackingField(type, property)!;

		object? made = null;

		for (var i = 0; i < depth; i++)
		{
			var node = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);

			if (made is not null)
				field.SetValue(node, Wrapped(made, property.PropertyType));

			made = node;
		}

		return made!;
	}

	/// <summary>A node as the value of a member of <paramref name="type"/>: itself, or a list of one.</summary>
	static object Wrapped(object node, Type type)
	{
		type = Nullable.GetUnderlyingType(type) ?? type;

		if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SqlList<>))
			return node;

		var element = type.GetGenericArguments()[0];
		var one     = Array.CreateInstance(element, 1);

		one.SetValue(Wrapped(node, element), 0);

		return typeof(SqlList).GetMethod(nameof(SqlList.From))!.MakeGenericMethod(element).Invoke(null, [one])!;
	}

	/// <summary>What a member holds: its type, through a Nullable and any number of lists.</summary>
	static Type Element(Type type)
	{
		type = Nullable.GetUnderlyingType(type) ?? type;

		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SqlList<>) ? Element(type.GetGenericArguments()[0]) : type;
	}

	static FieldInfo? BackingField(Type type, PropertyInfo property)
	{
		for (var at = type; at is not null; at = at.BaseType)
		{
			if (at.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) is { } field)
				return field;
		}

		return null;
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
	/// <summary>A literal of either tree with a digit more in its text.</summary>
	static ISqlSpan Retexted(ISqlSpan literal)
	{
		return literal switch
		{
			Expression.Literal old                       => old with { Text = old.Text + "0" },
			DotGram.Sql.Ast.LiteralValue.Numeric numeric => numeric with { Text = numeric.Text + "0" },
			DotGram.Sql.Ast.LiteralValue.String text     => text with { Text = text.Text + "0" },
			_                                            => throw new ArgumentException(literal.GetType().Name, nameof(literal)),
		};
	}

	/// <summary>
	/// The deepest literal of a tree — of this one, or of the common tree a statement whose rule builds it holds.
	/// </summary>
	static ISqlSpan? Deepest(ISqlSpan root)
	{
		ISqlSpan? found = null;
		var deepest = -1;

		Visit(root, 0);

		return found;

		void Visit(object value, int depth)
		{
			if (value is Expression.Literal { Kind: SqlLiteralKind.Number or SqlLiteralKind.Text or SqlLiteralKind.National } or DotGram.Sql.Ast.LiteralValue.Numeric or DotGram.Sql.Ast.LiteralValue.String && depth > deepest)
			{
				found   = (ISqlSpan)value;
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
