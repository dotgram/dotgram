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
}
