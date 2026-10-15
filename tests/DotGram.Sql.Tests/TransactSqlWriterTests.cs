using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the OLD tree's; see Sql2023WriterCoverageTests.
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using Option = DotGram.Sql.Ast.Option;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// T-SQL's writer (TransactSqlWriter): the trees T-SQL would read as others refused, which Sql2023Writer.Write still
/// writes, the same trees with their names bracketed written and read back, and every place that writes a key word into
/// a name or value slot written and read back.
/// </summary>
public sealed class TransactSqlWriterTests
{
	// ── Reserved words ───────────────────────────────────────────────────────────

	/// <summary>
	/// The writer's reserved words are the grammar's <c>TSqlReserved</c>, both ways: a word added to the grammar and not to
	/// the writer fails here, and so does a rule reshaped so that this reading would see fewer words.
	/// </summary>
	[Fact]
	public void The_reserved_words_are_the_grammar_s()
	{
		var lines = File.ReadAllLines(Path.Combine(Repository(), "src", "DotGram.Sql", "TransactSql", "TransactSql.gram"))
			.SkipWhile(static one => !one.StartsWith("TSqlReserved = (", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(lines);

		var grammar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var depth   = 0;

		foreach (var raw in lines)
		{
			var line = raw.StartsWith("TSqlReserved = ", StringComparison.Ordinal) ? raw.Substring("TSqlReserved = ".Length) : raw;

			if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
				continue;

			// A line that is anything but quoted words, `|` and brackets would be a rule this reading cannot expand.
			Assert.True(
				Regex.Replace(line, "\"([A-Za-z_]+)\"i|[|()]", "").Trim().Length == 0,
				"TSqlReserved has a line this test does not read: " + raw.Trim());

			foreach (Match word in Regex.Matches(line, "\"([A-Za-z_]+)\"i"))
				grammar.Add(word.Groups[1].Value.ToUpperInvariant());

			depth += line.Count(static one => one == '(') - line.Count(static one => one == ')');

			if (depth == 0)
				break;
		}

		var writer = Reserved();

		Assert.Equal(179, grammar.Count);
		Assert.Empty(grammar.Except(writer, StringComparer.OrdinalIgnoreCase));
		Assert.Empty(writer.Except(grammar, StringComparer.OrdinalIgnoreCase));
	}

	/// <summary>The writer's list, read by reflection: it is internal to the package.</summary>
	static HashSet<string> Reserved()
	{
		var text  = typeof(Sql2023Writer).GetNestedType("Text", BindingFlags.NonPublic)!;
		var field = text.GetField("TransactSqlReserved", BindingFlags.NonPublic | BindingFlags.Static)!;

		return (HashSet<string>)field.GetValue(null)!;
	}

	static string Repository([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
	}

	static Identifier Plain(string text)
	{
		return new Identifier(text);
	}

	static Identifier Bracketed(string text)
	{
		return new Identifier("[" + text + "]", IdentifierStyle.Bracketed);
	}

	static QualifiedName Q(Identifier name)
	{
		return new QualifiedName([name]);
	}

	static Expression Ref(Identifier name)
	{
		return new Expression.Reference(Q(name));
	}

	/// <summary>A tree with a name that is a reserved word, written plain or bracketed.</summary>
	static ISqlNode Named(string which, Func<string, Identifier> name)
	{
		return which switch
		{
			"a table named DATABASE"       => new Statement.DropTable { Names = [Q(name("DATABASE"))] },
			"a procedure named IF"         => new Statement.DropRoutine { Routines = [new RoutineDesignator(RoutineKind.Procedure, Q(name("IF")))] },
			"a table named ON"             => new Statement.DropTable { Names = [Q(name("ON"))] },
			"a view named WITH"            => new Statement.DropView { Names = [Q(name("WITH"))] },
			"a transaction named SELECT"   => new Statement.BeginTransaction(Tran: true, Name: new TransactionName(name("SELECT"))),
			"a module named WAITFOR"       => new Statement.ExecuteModule(new ModuleReference.Named(Q(name("WAITFOR"))), []) { Keyword = ExecuteKeyword.Exec },
			"a module named KILL"          => new Statement.ExecuteModule(new ModuleReference.Named(Q(name("KILL"))), []) { Keyword = ExecuteKeyword.Exec },
			"an argument named DEFAULT"    => new Statement.ExecuteModule(new ModuleReference.Named(Q(Plain("p"))), [new ExecuteArgument(Ref(name("DEFAULT")))]) { Keyword = ExecuteKeyword.Exec },
			"an argument named NULL"       => new Statement.ExecuteModule(new ModuleReference.Named(Q(Plain("p"))), [new ExecuteArgument(Ref(name("NULL")))]) { Keyword = ExecuteKeyword.Exec },
			"a trigger named ALL"          => new Statement.EnableTrigger([Q(name("ALL"))]) { On = Q(Plain("t")) },
			"DATEFIRST set to a name ON"   => new Statement.SetOptions([new SetOption.Value(Plain("DATEFIRST"), Ref(name("ON")))]),
			"a call qualified to dbo.LEFT" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.Invocation(new QualifiedName([Plain("dbo"), name("LEFT")]), [new Argument(new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger))]))] } },
			_                              => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>
	/// A reserved word written plain where a name stands is refused by T-SQL's writer, which T-SQL reads as the key word
	/// (TSqlReserved, TSqlIdentifier); the standard's writer has no reserved-word check and writes it.
	/// </summary>
	[Theory]
	[InlineData("a table named DATABASE")]
	[InlineData("a procedure named IF")]
	[InlineData("a table named ON")]
	[InlineData("a view named WITH")]
	[InlineData("a transaction named SELECT")]
	[InlineData("a module named WAITFOR")]
	[InlineData("a module named KILL")]
	[InlineData("an argument named DEFAULT")]
	[InlineData("an argument named NULL")]
	[InlineData("a trigger named ALL")]
	[InlineData("DATEFIRST set to a name ON")]
	[InlineData("a call qualified to dbo.LEFT")]
	public void A_reserved_word_written_plain_is_refused(string which)
	{
		var tree = Named(which, Plain);

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));
		Assert.NotEmpty(Sql2023Writer.Write(tree));
	}

	/// <summary>The same trees with the name bracketed: written, and read back by TransactSqlParser to the same tree.</summary>
	[Theory]
	[InlineData("a table named DATABASE")]
	[InlineData("a procedure named IF")]
	[InlineData("a table named ON")]
	[InlineData("a view named WITH")]
	[InlineData("a transaction named SELECT")]
	[InlineData("a module named WAITFOR")]
	[InlineData("a module named KILL")]
	[InlineData("an argument named DEFAULT")]
	[InlineData("an argument named NULL")]
	[InlineData("a trigger named ALL")]
	[InlineData("DATEFIRST set to a name ON")]
	public void A_reserved_word_bracketed_is_a_name(string which)
	{
		var tree = Named(which, Bracketed);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>An EXEC argument of DEFAULT or NULL is the key word, which is what reads there.</summary>
	[Theory]
	[InlineData("EXEC p DEFAULT")]
	[InlineData("EXEC p NULL")]
	public void An_argument_of_DEFAULT_or_NULL_is_the_key_word(string text)
	{
		var tree = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(tree is Statement.ExecuteModule { Arguments: [{ Value: Expression.Default or LiteralValue.Null }] }, $"{text} built {tree}");
		Assert.Equal(text, TransactSqlWriter.Write(tree));
	}

	/// <summary>
	/// An atomic block's LANGUAGE set to DEFAULT written plain is refused (AtomicConstant reads a name, and DEFAULT is
	/// reserved); bracketed it is a name.
	/// </summary>
	[Fact]
	public void An_atomic_language_of_DEFAULT_written_plain_is_refused()
	{
		Statement.Compound Atomic(Identifier language)
		{
			return new Statement.Compound([new Statement.Break()])
			{
				Atomic = [new AtomicOption.Isolation(IsolationLevel.Snapshot), new AtomicOption.Setting(new Option(Plain("LANGUAGE"), Ref(language), []))],
			};
		}

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(Atomic(Plain("DEFAULT"))));
		Assert.NotEmpty(Sql2023Writer.Write(Atomic(Plain("DEFAULT"))));

		var tree   = Atomic(Bracketed("DEFAULT"));
		var source = "CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS " + TransactSqlWriter.Write(tree);
		var body   = ((DotGram.Sql.Statement.CreateProcedure)TransactSqlParser.Located.ParseStatement(source)).Body[0];

		Assert.Equal(tree, Bridge.ToAst(body, source));
	}

	/// <summary>
	/// Every place whose grammar reads a key word into a name or value slot writes it through PutKeyWord: the text, read
	/// and carried by the bridge, is written back as it was and reads as the same tree.
	/// </summary>
	[Theory]
	[InlineData("CREATE INDEX i ON t (a) WITH (PAD_INDEX = ON, FILLFACTOR = 80)")]
	[InlineData("CREATE TABLE t (a INT) ON DEFAULT")]
	[InlineData("BULK INSERT t FROM 'f' WITH (ORDER (a ASC))")]
	[InlineData("CREATE STATISTICS s ON t (a) WITH SAMPLE 50 PERCENT")]
	[InlineData("ALTER DATABASE d SET ENCRYPTION ON, QUERY_STORE CLEAR ALL, HADR AVAILABILITY GROUP = g")]
	[InlineData("CREATE DATABASE d WITH TRUSTWORTHY OFF")]
	[InlineData("BACKUP DATABASE d TO a WITH ENCRYPTION (ALGORITHM = AES_256, SERVER ASYMMETRIC KEY = k)")]
	[InlineData("RESTORE DATABASE d FROM a WITH FILE = 2")]
	[InlineData("SELECT a FROM t WITH (HOLDLOCK, INDEX (1))")]
	[InlineData("SELECT LEFT('a', 1), RIGHT('a', 1), CONVERT(INT, '1'), COALESCE(a, 1), NULLIF(a, 1)")]
	[InlineData("SELECT {fn DATABASE()}, {fn CONVERT('1', SQL_BIGINT)}")]
	[InlineData("SELECT a FROM t WHERE CONTAINS(a, 'x') AND FREETEXT(a, 'y')")]
	[InlineData("SELECT * FROM SEMANTICKEYPHRASETABLE(t, *) AS k")]
	[InlineData("SELECT IDENTITYCOL, t.ROWGUIDCOL FROM t")]
	[InlineData("SET ROWCOUNT 10")]
	[InlineData("SET OFFSETS SELECT, FROM ON")]
	[InlineData("SET FIPS_FLAGGER OFF")]
	[InlineData("ALTER FULLTEXT INDEX ON t SET CHANGE_TRACKING OFF")]
	[InlineData("ALTER INDEX i ON t SET (ALLOW_ROW_LOCKS = ON)")]
	public void A_key_word_place_writes_its_word(string text)
	{
		var tree = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.Equal(text, TransactSqlWriter.Write(tree));
		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>A trigger's UPDATE(c) and an atomic block's DELAYED_DURABILITY = ON, inside the statements that read them.</summary>
	[Theory]
	[InlineData("CREATE TRIGGER r ON t AFTER UPDATE AS IF UPDATE(a) PRINT 1;")]
	[InlineData("CREATE PROCEDURE p WITH NATIVE_COMPILATION, SCHEMABINDING AS BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english', DELAYED_DURABILITY = ON) SELECT 1; END;")]
	public void A_key_word_place_inside_a_module_writes_its_word(string text)
	{
		var tree = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.Equal(text, TransactSqlWriter.Write(tree));
	}

	// ── Condition against value ──────────────────────────────────────────────────

	/// <summary>The condition every row's text holds at its place, <c>@a = 1</c>, and the value put in its stead, <c>@a</c>.</summary>
	static readonly Expression Marker = new Expression.Comparison(new Expression.Variable(Plain("@a")), ComparisonOperator.Equal, new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger));

	static readonly Expression Value = new Expression.Variable(Plain("@a"));

	/// <summary>
	/// A tree with every node equal to <paramref name="from"/> replaced by <paramref name="to"/>: a record copied through its
	/// clone and its properties set, a list rebuilt item by item.
	/// </summary>
	static object Swap(object node, object from, object to)
	{
		if (node.Equals(from))
			return to;

		var type = node.GetType();

		if (type.Namespace != typeof(Statement).Namespace || !type.IsClass || type.GetMethod("<Clone>$") is not { } clone)
			return node;

		var copy = clone.Invoke(node, null)!;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!property.CanWrite || property.GetIndexParameters().Length > 0 || property.GetValue(node) is not { } value)
				continue;

			if (value is ISqlNode)
				property.SetValue(copy, Swap(value, from, to));
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(SqlList<>))
			{
				var item  = property.PropertyType.GetGenericArguments()[0];
				var items = ((System.Collections.IEnumerable)value).Cast<object>().Select(one => Swap(one, from, to)).ToArray();
				var typed = Array.CreateInstance(item, items.Length);

				Array.Copy(items, typed, items.Length);
				property.SetValue(copy, typeof(SqlList).GetMethod(nameof(SqlList.From))!.MakeGenericMethod(item).Invoke(null, [typed]));
			}
		}

		return copy;
	}

	static ISqlNode Read(string text)
	{
		return Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);
	}

	/// <summary>
	/// Every place T-SQL reads a search condition at writes it through PutCondition: the condition is written and read
	/// back; a value in its place is refused (Msg 4145, "An expression of non-boolean type specified in a context where a
	/// condition is expected"), and Sql2023Writer.Write, which has no such check, writes it.
	/// </summary>
	[Theory]
	[InlineData("IF @a = 1 PRINT 1")]
	[InlineData("WHILE @a = 1 BREAK")]
	[InlineData("SELECT a FROM t WHERE @a = 1")]
	[InlineData("SELECT a FROM t GROUP BY a HAVING @a = 1")]
	[InlineData("SELECT a FROM t INNER JOIN u ON @a = 1")]
	[InlineData("SELECT CASE WHEN @a = 1 THEN 2 END")]
	[InlineData("SELECT IIF(@a = 1, 2, 3)")]
	[InlineData("CREATE TABLE t (a INT, CHECK (@a = 1))")]
	[InlineData("CREATE INDEX i ON t (a) WHERE @a = 1")]
	[InlineData("CREATE STATISTICS s ON t (a) WHERE @a = 1")]
	[InlineData("CREATE RULE r AS @a = 1")]
	[InlineData("MERGE INTO t USING u ON @a = 1 WHEN MATCHED THEN DELETE")]
	[InlineData("MERGE INTO t USING u ON t.a = u.a WHEN MATCHED AND @a = 1 THEN DELETE")]
	[InlineData("DELETE FROM t WHERE @a = 1")]
	[InlineData("UPDATE t SET a = 1 WHERE @a = 1")]
	public void A_condition_place_writes_a_condition_and_refuses_a_value(string text)
	{
		var tree = Read(text);

		Assert.Equal(text, TransactSqlWriter.Write(tree));

		var swapped = (ISqlNode)Swap(tree, Marker, Value);

		Assert.NotEqual(tree, swapped);
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(swapped));
		Assert.NotEmpty(Sql2023Writer.Write(swapped));
	}

	/// <summary>
	/// A condition where T-SQL reads a value is refused: a SET's value, a select column, a comparison's operand, PRINT's
	/// value (each read by ValueExpression, which reads no predicate); the value itself is written and read back.
	/// </summary>
	[Theory]
	[InlineData("SET @v = @a")]
	[InlineData("SELECT @a")]
	[InlineData("SELECT a FROM t WHERE @a = 2")]
	[InlineData("PRINT @a")]
	public void A_value_place_refuses_a_condition(string text)
	{
		var tree = Read(text);

		Assert.Equal(text, TransactSqlWriter.Write(tree));

		var swapped = (ISqlNode)Swap(tree, Value, Marker);

		Assert.NotEqual(tree, swapped);
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(swapped));
	}

	/// <summary>A hand-built IF whose condition is AND, OR, NOT and brackets of conditions, written and read back.</summary>
	[Fact]
	public void A_logical_condition_is_written_and_read_back()
	{
		var a   = new Expression.Variable(Plain("@a"));
		var one = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger);
		var tree = new Statement.If(
			new Expression.Binary(
				new Expression.Unary(UnaryOperator.Not, new Expression.Comparison(a, ComparisonOperator.Equal, one)),
				BinaryOperator.Or,
				new Expression.Parenthesized(new Expression.Binary(new Expression.IsNull(a, true), BinaryOperator.And, new Expression.Exists(new Query.Select { Columns = [new SelectColumn.ExpressionItem(one)] })))),
			new Statement.Print(one),
			null);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal("IF NOT @a = 1 OR (@a IS NOT NULL AND EXISTS (SELECT 1)) PRINT 1", text);
		Assert.Equal(tree, Read(text));
	}

	/// <summary>
	/// AND and NOT of values, and a bracket of a value, where a condition stands, are refused (Msg 4145); the standard's
	/// writer still writes a predicate as a select column.
	/// </summary>
	[Fact]
	public void A_logical_operator_over_values_is_refused()
	{
		var a   = new Expression.Variable(Plain("@a"));
		var one = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger);
		var print = new Statement.Print(one);

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(new Statement.If(new Expression.Binary(a, BinaryOperator.And, one), print, null)));
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(new Statement.If(new Expression.Unary(UnaryOperator.Not, a), print, null)));
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(new Statement.If(new Expression.Parenthesized(a), print, null)));

		var select = new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.Comparison(new Expression.Reference(Q(Plain("a"))), ComparisonOperator.Greater, one))] } };

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(select));
		Assert.Equal("SELECT a > 1", Sql2023Writer.Write(select));
	}

	// ── Signed numbers ───────────────────────────────────────────────────────────

	static readonly Expression Signed = new LiteralValue.Numeric("-1", NumericLiteralKind.DecimalInteger);

	static readonly Expression Negated = new Expression.Unary(UnaryOperator.Minus, new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger));

	/// <summary>
	/// Every place whose grammar keeps a signed number as one token writes it through PutSignedNumber: the number, -1, is
	/// written and read back; a minus over the number in its place is refused (T-SQL reads only the token there), where
	/// the place's own catalogue had not refused it already.
	/// </summary>
	[Theory]
	[InlineData("SET LOCK_TIMEOUT -1")]                                          // SetConstant
	[InlineData("SET TEXTSIZE -1")]                                              // SetInteger
	[InlineData("EXECUTE p -1")]                                                 // ExecValue, DbccValue
	[InlineData("FETCH RELATIVE -1 FROM c")]                                     // FetchRow
	[InlineData("CREATE SEQUENCE s START WITH -1 INCREMENT BY -1 MINVALUE -1")] // the sequence's numbers
	[InlineData("ALTER SEQUENCE s RESTART WITH -1")]
	[InlineData("UPDATETEXT t.c 0 -1 @v")]                                       // the text statements' place
	[InlineData("CREATE SPATIAL INDEX i ON t (g) WITH (CELLS_PER_OBJECT = -1)")] // SpatialItem
	public void A_signed_number_place_writes_the_token_and_refuses_a_minus(string text)
	{
		var tree = Read(text);

		Assert.Equal(text, TransactSqlWriter.Write(tree));

		var swapped = (ISqlNode)Swap(tree, Signed, Negated);

		Assert.NotEqual(tree, swapped);
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(swapped));
	}

	/// <summary>
	/// <c>SET DEADLOCK_PRIORITY -5</c>, built by hand, is a number with its sign (SetConstant) and reads back so; a minus
	/// over 5 in its place is refused.
	/// </summary>
	[Fact]
	public void A_signed_number_is_one_token_where_the_grammar_keeps_it()
	{
		var tree = new Statement.SetOptions([new SetOption.Value(Plain("DEADLOCK_PRIORITY"), new LiteralValue.Numeric("-5", NumericLiteralKind.DecimalInteger))]);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal("SET DEADLOCK_PRIORITY -5", text);
		Assert.Equal(tree, Read(text));

		var minus = new Statement.SetOptions([new SetOption.Value(Plain("DEADLOCK_PRIORITY"), new Expression.Unary(UnaryOperator.Minus, new LiteralValue.Numeric("5", NumericLiteralKind.DecimalInteger)))]);

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(minus));
	}

	/// <summary>
	/// <c>SELECT -1</c> is a minus over 1 where an expression is read, and reads back so; a number whose text begins with a
	/// sign in its place is refused (T-SQL reads it as the minus), which Sql2023Writer.Write still writes.
	/// </summary>
	[Fact]
	public void A_minus_over_a_number_is_an_expression_where_one_is_read()
	{
		Statement.Select Selected(Expression value)
		{
			return new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(value)] } };
		}

		var tree = Selected(Negated);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal("SELECT -1", text);
		Assert.Equal(tree, Read(text));
		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(Selected(Signed)));
		Assert.Equal("SELECT -1", Sql2023Writer.Write(Selected(Signed)));
	}

	// ── Option placement and the round's spellings ───────────────────────────────

	/// <summary>Every node of type <typeparamref name="T"/> in a tree, in the order its properties hold them.</summary>
	static IEnumerable<T> All<T>(object node)
	{
		if (node is T found)
			yield return found;

		var type = node.GetType();

		if (type.Namespace != typeof(Statement).Namespace || !type.IsClass)
			yield break;

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length > 0 || property.Name == "Span" || property.GetValue(node) is not { } value)
				continue;

			if (value is ISqlNode)
			{
				foreach (var one in All<T>(value))
					yield return one;
			}
			else if (value is System.Collections.IEnumerable items and not string)
			{
				foreach (var item in items)
				{
					if (item is ISqlNode)
					{
						foreach (var one in All<T>(item))
							yield return one;
					}
				}
			}
		}
	}

	/// <summary>The tree <paramref name="text"/> reads as, its one placed option changed so.</summary>
	static ISqlNode Placed(string text, Func<Option, Option> change)
	{
		var tree   = Read(text);
		var option = All<Option>(tree).Single(static one => one.Place is not null);

		return (ISqlNode)Swap(tree, option, change(option));
	}

	/// <summary>The tree <paramref name="text"/> reads as, its routine changed so.</summary>
	static ISqlNode Defined(string text, Func<RoutineDefinition, RoutineDefinition> change)
	{
		var tree    = Read(text);
		var routine = All<RoutineDefinition>(tree).Single();

		return (ISqlNode)Swap(tree, routine, change(routine));
	}

	/// <summary>
	/// <c>MOVE TO</c> and DROP INDEX's <c>FILESTREAM_ON</c> put their target in the option's place (DropOption,
	/// DropIndexOption), never in its value: a filegroup, a scheme and its column, a quoted name. Written and read back.
	/// </summary>
	[Theory]
	[InlineData("DROP INDEX i ON t WITH (MOVE TO fg)",                         PlacementKind.MoveTo,       false)]
	[InlineData("DROP INDEX i ON t WITH (MOVE TO ps (c))",                     PlacementKind.MoveTo,       true)]
	[InlineData("DROP INDEX i ON t WITH (MOVE TO 'fg')",                       PlacementKind.MoveTo,       false)]
	[InlineData("DROP INDEX i ON t WITH (FILESTREAM_ON fg)",                   PlacementKind.FilestreamOn, false)]
	[InlineData("ALTER TABLE t DROP CONSTRAINT k WITH (MOVE TO fg)",           PlacementKind.MoveTo,       false)]
	[InlineData("ALTER TABLE t DROP CONSTRAINT k WITH (MOVE TO ps (c))",       PlacementKind.MoveTo,       true)]
	[InlineData("ALTER TABLE t DROP CONSTRAINT k WITH (MOVE TO 'fg')",         PlacementKind.MoveTo,       false)]
	public void An_option_that_moves_puts_its_target_in_its_place(string text, PlacementKind kind, bool columns)
	{
		var tree = Read(text);

		Assert.True(All<Option>(tree).ToList() is [{ Place: { } place, Value: null, Options.Count: 0 }] && place.Kind == kind && (place.Columns is not null) == columns, $"{text} built {tree}");
		Assert.Equal(text, TransactSqlWriter.Write(tree));
	}

	/// <summary>
	/// ALTER TABLE's DROP CONSTRAINT … WITH reads MAXDOP, ONLINE, MOVE TO and WAIT_AT_LOW_PRIORITY (DropOption), not
	/// FILESTREAM_ON, which is DROP INDEX's alone; so the writer has no FILESTREAM_ON place there.
	/// </summary>
	[Fact]
	public void A_dropped_constraint_reads_no_filestream_place()
	{
		Assert.ThrowsAny<FormatException>(() => TransactSqlParser.Located.ParseStatement("ALTER TABLE t DROP CONSTRAINT k WITH (FILESTREAM_ON fg)"));

		var tree = Placed("ALTER TABLE t DROP CONSTRAINT k WITH (MOVE TO fg)", static option => new Option(new Identifier("FILESTREAM_ON"), null, [])
		{
			Place = new Placement(PlacementKind.FilestreamOn, new Identifier("fg"), null),
		});

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));
	}

	/// <summary>A MOVE TO a scheme and its column, built by hand: written, and read back to the same tree.</summary>
	[Fact]
	public void A_hand_built_move_reads_back()
	{
		var move = new Option(Plain("MOVE"), null, [])
		{
			Words = [Plain("TO")],
			Place = new Placement(PlacementKind.MoveTo, Plain("ps"), [Plain("c")]),
		};
		var tree = new Statement.DropIndex([new DroppedIndex.Named(Plain("i"), Q(Plain("t")), [move])], false);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal("DROP INDEX i ON t WITH (MOVE TO ps (c))", text);
		Assert.Equal(tree, Read(text));
	}

	/// <summary>
	/// What an option's place cannot be: beside a value or options of its own (Both), of a kind the option's words do not
	/// say, the DEFAULT filegroup written plain after MOVE TO (DropOption's <c>?!"DEFAULT"</c>), columns after FILESTREAM_ON
	/// (DropIndexOption reads a target alone), missing on MOVE TO, or on an option that has none.
	/// </summary>
	[Theory]
	[InlineData("a value beside the place")]
	[InlineData("options beside the place")]
	[InlineData("a place of kind On")]
	[InlineData("a place of kind TextImageOn")]
	[InlineData("MOVE TO DEFAULT written plain")]
	[InlineData("FILESTREAM_ON with columns")]
	[InlineData("MOVE TO with no place")]
	[InlineData("a place on ONLINE")]
	[InlineData("a place on an option of CREATE INDEX")]
	[InlineData("a place on an option of ALTER DATABASE")]
	public void A_place_where_none_reads_is_refused(string which)
	{
		var target = new Placement(PlacementKind.MoveTo, Plain("fg"), null);
		var one    = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger);

		ISqlNode tree = which switch
		{
			"a value beside the place"              => Placed("DROP INDEX i ON t WITH (MOVE TO fg)", option => option with { Value = one }),
			"options beside the place"              => Placed("DROP INDEX i ON t WITH (MOVE TO fg)", option => option with { Options = [new Option(Plain("x"), one, [])] }),
			"a place of kind On"                    => Placed("DROP INDEX i ON t WITH (MOVE TO fg)", option => option with { Place = target with { Kind = PlacementKind.On } }),
			"a place of kind TextImageOn"           => Placed("ALTER TABLE t DROP CONSTRAINT k WITH (MOVE TO fg)", option => option with { Place = target with { Kind = PlacementKind.TextImageOn } }),
			"MOVE TO DEFAULT written plain"         => Placed("DROP INDEX i ON t WITH (MOVE TO fg)", option => option with { Place = target with { Target = Plain("DEFAULT") } }),
			"FILESTREAM_ON with columns"            => Placed("DROP INDEX i ON t WITH (FILESTREAM_ON fg)", option => option with { Place = option.Place! with { Columns = [Plain("c")] } }),
			"MOVE TO with no place"                 => Placed("DROP INDEX i ON t WITH (MOVE TO fg)", option => option with { Place = null }),
			"a place on ONLINE"                     => (ISqlNode)Swap(Read("DROP INDEX i ON t WITH (ONLINE = ON)"), All<Option>(Read("DROP INDEX i ON t WITH (ONLINE = ON)")).Single(), All<Option>(Read("DROP INDEX i ON t WITH (ONLINE = ON)")).Single() with { Place = target }),
			"a place on an option of CREATE INDEX"  => (ISqlNode)Swap(Read("CREATE INDEX i ON t (a) WITH (ONLINE = ON)"), All<Option>(Read("CREATE INDEX i ON t (a) WITH (ONLINE = ON)")).Single(), All<Option>(Read("CREATE INDEX i ON t (a) WITH (ONLINE = ON)")).Single() with { Place = target }),
			"a place on an option of ALTER DATABASE" => (ISqlNode)Swap(Read("ALTER DATABASE d SET ENCRYPTION ON"), All<Option>(Read("ALTER DATABASE d SET ENCRYPTION ON")).Single(), All<Option>(Read("ALTER DATABASE d SET ENCRYPTION ON")).Single() with { Place = target }),
			_                                       => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));
	}

	/// <summary>
	/// A quoted <c>"default"</c> after MOVE TO is the name DropOption reads (it refuses only the plain word), and is
	/// written so.
	/// </summary>
	[Fact]
	public void A_move_to_a_quoted_default_is_a_name()
	{
		const string text = "DROP INDEX i ON t WITH (MOVE TO \"default\")";

		var tree = Read(text);

		Assert.True(All<Option>(tree).Single().Place is { Kind: PlacementKind.MoveTo, Target.Style: IdentifierStyle.Delimited }, $"{text} built {tree}");
		Assert.Equal(text, TransactSqlWriter.Write(tree));
	}

	/// <summary>
	/// COMMIT and ROLLBACK name their transaction by a name or by a variable, never both (TransactionName): PutTransactionName
	/// refuses the two together.
	/// </summary>
	[Theory]
	[InlineData("COMMIT TRANSACTION")]
	[InlineData("ROLLBACK TRANSACTION")]
	public void A_transaction_named_both_ways_is_refused(string verb)
	{
		var both = new TransactionName(Plain("x"), new Expression.Variable(Plain("@x")));

		ISqlNode tree = verb == "COMMIT TRANSACTION"
			? (Statement.Commit)Read("COMMIT TRANSACTION x") with { Name = both }
			: (Statement.Rollback)Read("ROLLBACK TRANSACTION x") with { Name = both };

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));
		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(tree));
	}

	/// <summary>
	/// The round's spellings, each pinned both ways: EXEC AS against EXECUTE AS (SessionStatement's ExecWord), a procedure's
	/// parameters without their brackets against with them (ProcedureStatement), a function's body and inline RETURN with no
	/// AS against with it (FunctionStatement). The flag the text says, and the text written back.
	/// </summary>
	[Theory]
	[InlineData("EXEC AS USER = 'u'",                                                false, "Exec")]
	[InlineData("EXECUTE AS USER = 'u'",                                             true,  "Exec")]
	[InlineData("CREATE PROCEDURE p @a INT AS SELECT 1;",                             false, "Unbracketed")]
	[InlineData("CREATE PROCEDURE p (@a INT) AS SELECT 1;",                           true,  "Unbracketed")]
	[InlineData("CREATE PROCEDURE p AS SELECT 1;",                                    true,  "Unbracketed")]
	[InlineData("CREATE FUNCTION f () RETURNS INT BEGIN RETURN 1; END;",               false, "AsOmitted")]
	[InlineData("CREATE FUNCTION f () RETURNS INT AS BEGIN RETURN 1; END;",            true,  "AsOmitted")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE RETURN SELECT 1 AS a",             false, "AsOmitted")]
	[InlineData("CREATE FUNCTION f () RETURNS TABLE AS RETURN SELECT 1 AS a",          true,  "AsOmitted")]
	public void A_spelling_of_the_round_reads_and_writes_as_written(string text, bool fixedForm, string flag)
	{
		var tree = Read(text);
		var set  = flag switch
		{
			"Exec"        => ((Statement.ExecuteAs)tree).Exec,
			"Unbracketed" => All<RoutineDefinition>(tree).Single().Unbracketed,
			"AsOmitted"   => All<RoutineDefinition>(tree).Single().AsOmitted,
			_             => throw new ArgumentOutOfRangeException(nameof(flag), flag, null),
		};

		Assert.Equal(!fixedForm, set);
		Assert.Equal(text, TransactSqlWriter.Write(tree));
	}

	/// <summary>
	/// Where a spelling of the round has no text to say: brackets left out of a function's parameters (FunctionStatement
	/// requires them) or of a procedure with none; AS left out of a procedure (ProcedureStatement requires it) or of a CLR
	/// table function's EXTERNAL NAME (FunctionReturns); either beside the standard's body.
	/// </summary>
	[Theory]
	[InlineData("Unbracketed on a function")]
	[InlineData("Unbracketed on a procedure of no parameters")]
	[InlineData("AsOmitted on a procedure")]
	[InlineData("AsOmitted on a CLR table function")]
	[InlineData("Unbracketed beside the standard's body")]
	[InlineData("AsOmitted beside the standard's body")]
	public void A_spelling_where_none_reads_is_refused(string which)
	{
		var tree = which switch
		{
			"Unbracketed on a function"                   => Defined("CREATE FUNCTION f(@a INT) RETURNS INT AS BEGIN RETURN 1; END;", static one => one with { Unbracketed = true }),
			"Unbracketed on a procedure of no parameters" => Defined("CREATE PROCEDURE p AS SELECT 1;", static one => one with { Unbracketed = true }),
			"AsOmitted on a procedure"                    => Defined("CREATE PROCEDURE p AS SELECT 1;", static one => one with { AsOmitted = true }),
			"AsOmitted on a CLR table function"           => Defined("CREATE FUNCTION f () RETURNS TABLE (a INT) AS EXTERNAL NAME x.y.z", static one => one with { AsOmitted = true }),
			"Unbracketed beside the standard's body"      => Defined("CREATE FUNCTION f(@a INT) RETURNS INT AS BEGIN RETURN 1; END;", static one => one with { Unbracketed = true, Body = new RoutineBody.Sql(new Statement.Return { Value = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger) }) }),
			"AsOmitted beside the standard's body"        => Defined("CREATE FUNCTION f(@a INT) RETURNS INT AS BEGIN RETURN 1; END;", static one => one with { AsOmitted = true, Body = new RoutineBody.Sql(new Statement.Return { Value = new LiteralValue.Numeric("1", NumericLiteralKind.DecimalInteger) }) }),
			_                                             => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(tree));
	}
}
