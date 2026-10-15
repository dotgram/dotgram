using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using ColumnDefinition = DotGram.Sql.Ast.ColumnDefinition;
using ColumnOption = DotGram.Sql.Ast.ColumnOption;
using Constraint = DotGram.Sql.Ast.Constraint;
using DataType = DotGram.Sql.Ast.DataType;
using ExecutionContext = DotGram.Sql.Ast.ExecutionContext;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using InsertSource = DotGram.Sql.Ast.InsertSource;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;
using TableElement = DotGram.Sql.Ast.TableElement;

/// <summary>
/// The Database family's writer (Sql2023Writer.TransactSql.Database.cs): each record written and read back by
/// TransactSqlParser to the same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class DatabaseWriterTests
{
	static readonly Identifier D = new("d");

	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Sized(string number, string unit)
	{
		return new Expression.Measured(Number(number), Name(unit));
	}

	static Option Set(string name, Expression? value = null)
	{
		return new Option(Name(name), value, []);
	}

	static DatabaseFile File(params Option[] options)
	{
		return new DatabaseFile([.. options]);
	}

	static Statement.AlterDatabase Alter(AlterDatabaseAction action)
	{
		return new Statement.AlterDatabase(D, action);
	}

	/// <summary>A tree for each record of the family, and where it reads: alone, or as a natively compiled body.</summary>
	static Statement Tree(string which)
	{
		return which switch
		{
			"COLLATE" => Alter(new AlterDatabaseAction.Collate(new CollationName(new QualifiedName([Name("Latin1_General_CI_AS")])))),
			"MODIFY NAME on CURRENT" => new Statement.AlterDatabase(null, new AlterDatabaseAction.ModifyName(Name("e"))) { Current = true },
			"MODIFY" => Alter(new AlterDatabaseAction.Modify([Set("EDITION", Text("Premium")), Set("MAXSIZE", Sized("500", "GB")), new Option(Name("SERVICE_OBJECTIVE"), Ref("ELASTIC_POOL"), [Set("NAME", Ref("p"))])], true)),
			"MODIFY BACKUP_STORAGE_REDUNDANCY" => Alter(new AlterDatabaseAction.ModifyBackupStorageRedundancy(Text("GEO"))),
			"PERFORM_CUTOVER" => Alter(new AlterDatabaseAction.PerformCutover()),
			"REBUILD LOG" => Alter(new AlterDatabaseAction.RebuildLog()),
			"ADD FILE" => Alter(new AlterDatabaseAction.AddFile([File(Set("NAME", Ref("a")), Set("FILENAME", Text("a.ndf")), Set("SIZE", Sized("10", "MB")), Set("MAXSIZE", Ref("UNLIMITED")), Set("FILEGROWTH", Sized("10", "%"))), File(Set("NAME", Text("b")), Set("SIZE", Number("5")), Set("OFFLINE"))], Name("g"))),
			"ADD LOG FILE" => Alter(new AlterDatabaseAction.AddLogFile([File(Set("NAME", Ref("l")))])),
			"MODIFY FILE" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("NAME", Ref("a")), Set("NEWNAME", Ref("b")), Set("MAXSIZE", Sized("2", "TB"))))),
			"REMOVE FILE" => Alter(new AlterDatabaseAction.RemoveFile(Name("a"))),
			"ADD FILEGROUP" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), FileGroupContents.MemoryOptimizedData)),
			"ADD FILEGROUP FILESTREAM" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), FileGroupContents.Filestream)),
			"MODIFY FILEGROUP READ_ONLY" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability(FileGroupUpdatability.ReadOnly, true))),
			"MODIFY FILEGROUP READWRITE" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability(FileGroupUpdatability.ReadWrite, false))),
			"MODIFY FILEGROUP DEFAULT" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Default())),
			"MODIFY FILEGROUP NAME" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Rename(Name("h")))),
			"MODIFY FILEGROUP AUTOGROW" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.AutoGrow(FileGroupAutoGrow.AllFiles))),
			"REMOVE FILEGROUP" => Alter(new AlterDatabaseAction.RemoveFileGroup(Name("g"))),
			"a database called CURRENT" => new Statement.AlterDatabase(new Identifier("[CURRENT]", IdentifierStyle.Bracketed), new AlterDatabaseAction.RemoveFile(Name("a"))),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each record written as its text.</summary>
	[Theory]
	[InlineData("COLLATE", "ALTER DATABASE d COLLATE Latin1_General_CI_AS")]
	[InlineData("MODIFY NAME on CURRENT", "ALTER DATABASE CURRENT MODIFY NAME = e")]
	[InlineData("MODIFY", "ALTER DATABASE d MODIFY (EDITION = 'Premium', MAXSIZE = 500 GB, SERVICE_OBJECTIVE = ELASTIC_POOL (NAME = p)) WITH MANUAL_CUTOVER")]
	[InlineData("MODIFY BACKUP_STORAGE_REDUNDANCY", "ALTER DATABASE d MODIFY BACKUP_STORAGE_REDUNDANCY = 'GEO'")]
	[InlineData("PERFORM_CUTOVER", "ALTER DATABASE d PERFORM_CUTOVER")]
	[InlineData("REBUILD LOG", "ALTER DATABASE d REBUILD LOG")]
	[InlineData("ADD FILE", "ALTER DATABASE d ADD FILE (NAME = a, FILENAME = 'a.ndf', SIZE = 10 MB, MAXSIZE = UNLIMITED, FILEGROWTH = 10 %), (NAME = 'b', SIZE = 5, OFFLINE) TO FILEGROUP g")]
	[InlineData("ADD LOG FILE", "ALTER DATABASE d ADD LOG FILE (NAME = l)")]
	[InlineData("MODIFY FILE", "ALTER DATABASE d MODIFY FILE (NAME = a, NEWNAME = b, MAXSIZE = 2 TB)")]
	[InlineData("REMOVE FILE", "ALTER DATABASE d REMOVE FILE a")]
	[InlineData("ADD FILEGROUP", "ALTER DATABASE d ADD FILEGROUP g CONTAINS MEMORY_OPTIMIZED_DATA")]
	[InlineData("ADD FILEGROUP FILESTREAM", "ALTER DATABASE d ADD FILEGROUP g CONTAINS FILESTREAM")]
	[InlineData("MODIFY FILEGROUP READ_ONLY", "ALTER DATABASE d MODIFY FILEGROUP g READ_ONLY")]
	[InlineData("MODIFY FILEGROUP READWRITE", "ALTER DATABASE d MODIFY FILEGROUP g READWRITE")]
	[InlineData("MODIFY FILEGROUP DEFAULT", "ALTER DATABASE d MODIFY FILEGROUP g DEFAULT")]
	[InlineData("MODIFY FILEGROUP NAME", "ALTER DATABASE d MODIFY FILEGROUP g NAME = h")]
	[InlineData("MODIFY FILEGROUP AUTOGROW", "ALTER DATABASE d MODIFY FILEGROUP g AUTOGROW_ALL_FILES")]
	[InlineData("REMOVE FILEGROUP", "ALTER DATABASE d REMOVE FILEGROUP g")]
	[InlineData("a database called CURRENT", "ALTER DATABASE [CURRENT] REMOVE FILE a")]
	public void Each_record_writes_its_text(string which, string text)
	{
		Assert.Equal(text, Sql2023Writer.Write(Tree(which)));
	}

	/// <summary>
	/// Each record's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree.
	/// </summary>
	[Theory]
	[InlineData("COLLATE")]
	[InlineData("MODIFY NAME on CURRENT")]
	[InlineData("MODIFY")]
	[InlineData("MODIFY BACKUP_STORAGE_REDUNDANCY")]
	[InlineData("PERFORM_CUTOVER")]
	[InlineData("REBUILD LOG")]
	[InlineData("ADD FILE")]
	[InlineData("ADD LOG FILE")]
	[InlineData("MODIFY FILE")]
	[InlineData("REMOVE FILE")]
	[InlineData("ADD FILEGROUP")]
	[InlineData("ADD FILEGROUP FILESTREAM")]
	[InlineData("MODIFY FILEGROUP READ_ONLY")]
	[InlineData("MODIFY FILEGROUP READWRITE")]
	[InlineData("MODIFY FILEGROUP DEFAULT")]
	[InlineData("MODIFY FILEGROUP NAME")]
	[InlineData("MODIFY FILEGROUP AUTOGROW")]
	[InlineData("REMOVE FILEGROUP")]
	[InlineData("a database called CURRENT")]
	public void Each_record_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = Sql2023Writer.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's message
	/// that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("a name and CURRENT")]
	[InlineData("neither name nor CURRENT")]
	[InlineData("a database named CURRENT unbracketed")]
	[InlineData("MODIFY of nothing")]
	[InlineData("MODIFY of a name it does not read")]
	[InlineData("MODIFY MAXSIZE in KB")]
	[InlineData("MODIFY EDITION of a name")]
	[InlineData("BACKUP_STORAGE_REDUNDANCY of a word")]
	[InlineData("ADD FILE of nothing")]
	[InlineData("a file of no option")]
	[InlineData("a file option it does not read")]
	[InlineData("SIZE of a fraction")]
	[InlineData("SIZE in percent")]
	[InlineData("SIZE in PB")]
	[InlineData("OFFLINE with a value")]
	[InlineData("NAME of a number")]
	[InlineData("a file option with its = left out")]
	[InlineData("an ADD FILEGROUP with no text for what it holds")]
	[InlineData("a MODIFY FILEGROUP with no text for its updatability")]
	public void A_database_tree_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// DatabaseTarget: CURRENT or a name.
			"a name and CURRENT" => new Statement.AlterDatabase(D, new AlterDatabaseAction.RebuildLog()) { Current = true },
			// AlterDatabaseStatement: DatabaseTarget.
			"neither name nor CURRENT" => new Statement.AlterDatabase(null, new AlterDatabaseAction.RebuildLog()),
			// DatabaseTarget: the word CURRENT.
			"a database named CURRENT unbracketed" => new Statement.AlterDatabase(Name("current"), new AlterDatabaseAction.RebuildLog()),
			// EditionOptions: one at least.
			"MODIFY of nothing" => Alter(new AlterDatabaseAction.Modify([], false)),
			// EditionSyntax.
			"MODIFY of a name it does not read" => Alter(new AlterDatabaseAction.Modify([Set("FOO", Text("x"))], false)),
			// EditionSyntax: MB or GB.
			"MODIFY MAXSIZE in KB" => Alter(new AlterDatabaseAction.Modify([Set("MAXSIZE", Sized("5", "KB"))], false)),
			// EditionSyntax: Quoted.
			"MODIFY EDITION of a name" => Alter(new AlterDatabaseAction.Modify([Set("EDITION", Ref("Premium"))], false)),
			// DatabaseModify: = Quoted.
			"BACKUP_STORAGE_REDUNDANCY of a word" => Alter(new AlterDatabaseAction.ModifyBackupStorageRedundancy(Ref("GEO"))),
			// DatabaseAdd: FileSpecText, one at least.
			"ADD FILE of nothing" => Alter(new AlterDatabaseAction.AddFile([], null)),
			// FileSpecText: one option at least.
			"a file of no option" => Alter(new AlterDatabaseAction.ModifyFile(File())),
			// FileOptionSyntax, Msg 153.
			"a file option it does not read" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("FOO", Number("1"))))),
			// FileOptionSyntax: SIZE = Digits.
			"SIZE of a fraction" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", new LiteralValue.Numeric("10.5", NumericLiteralKind.Decimal))))),
			// FileOptionSyntax: SizeUnit, SIZE = 10 % is refused.
			"SIZE in percent" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Sized("10", "%"))))),
			// SizeUnit: KB, MB, GB, TB.
			"SIZE in PB" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Sized("10", "PB"))))),
			// FileOptionSyntax: OFFLINE alone.
			"OFFLINE with a value" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("OFFLINE", Ref("ON"))))),
			// FileOptionSyntax: Quoted or Identifier.
			"NAME of a number" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("NAME", Number("1"))))),
			// FileOptionSyntax: name = value.
			"a file option with its = left out" => Alter(new AlterDatabaseAction.ModifyFile(File(Set("SIZE", Number("1")) with { EqualsOmitted = true }))),
			// FileGroupHolds.
			"an ADD FILEGROUP with no text for what it holds" => Alter(new AlterDatabaseAction.AddFileGroup(Name("g"), (FileGroupContents)9)),
			// FileGroupAction.
			"a MODIFY FILEGROUP with no text for its updatability" => Alter(new AlterDatabaseAction.ModifyFileGroup(Name("g"), new FileGroupChange.Updatability((FileGroupUpdatability)9, false))),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}
}
