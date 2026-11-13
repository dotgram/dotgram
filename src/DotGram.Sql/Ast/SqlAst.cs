using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

/// <summary>
/// What every node is. It introduces no class inheritance level: every polymorphic family is an
/// independent one-level hierarchy (Statement -> Statement.Select, Expression -> Expression.Binary).
/// </summary>
/// <remarks>
/// Where a node was written is <see cref="ISqlSpan"/>, as in the tree built today: a family's abstract
/// record keeps the span for all its members, and a record of no family keeps its own. It is a property
/// of the node, not a base class.
/// <para>
/// A node compares by what was written and not where: the span is held in a field whose values are
/// all equal, and every list is a <see cref="SqlList{T}"/>, which compares element by element. So a
/// tree equals its own reparse, located or not.
/// </para>
/// </remarks>
public interface ISqlNode : ISqlSpan;

/// <summary>
/// Marks a property that records WHICH of two spellings was written, where the reference publishes two
/// words for one meaning.
/// </summary>
/// <remarks>
/// <para>
/// The tree keeps one node for one meaning and the spelling beside it, so that a round trip gives the
/// text back as written: <c>!=</c> against <c>&lt;&gt;</c>, <c>MAX</c> against a number,
/// <c>READ_ONLY</c> against <c>READONLY</c>, <c>PROC</c> against <c>PROCEDURE</c>. Without the flag the
/// two spellings are one tree and the writer has to pick one, which loses the other.
/// </para>
/// <para>
/// It is here so that the check can find them. <c>SqlWriterCoverageTests</c> asks of every property
/// marked this way that turning it on changes what the writer prints, and refuses to pass while one of
/// them has no such row — so a flag added later joins the check by being a flag rather than by anyone
/// remembering it. Internal, so it is not part of what a consumer sees; the test reads it by name.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class SpellingAttribute : Attribute;

/// <summary>
/// An identifier: a name written in a statement, such as a table, a column, an alias or a parameter.
/// </summary>
/// <param name="Text">The identifier as written, its quotes included.</param>
/// <param name="Style">How the identifier was written: plain, or delimited in one of the ways the dialect allows.</param>
/// <param name="UnicodeEscape">
/// The escape character of a Unicode delimited identifier: <c>!</c> in <c>U&amp;"a" UESCAPE '!'</c>; null where
/// none was written.
/// </param>
public sealed record Identifier(string Text, IdentifierStyle Style = IdentifierStyle.Regular, char? UnicodeEscape = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// How an <see cref="Identifier"/> was written.
/// </summary>
/// <remarks>
/// T-SQL: Database Identifiers (Transact-SQL). <see cref="Bracketed"/> is T-SQL's <c>[x]</c>.
/// T-SQL: SELECT Clause (Transact-SQL). <see cref="String"/> and <see cref="NationalString"/>: a column alias
/// written as a string, <c>'x'</c> or <c>N'x'</c>, whose text is the string's content with its doubled quotes
/// undone.
/// </remarks>
public enum IdentifierStyle
{
	/// <summary>A plain, undelimited identifier.</summary>
	Regular,

	/// <summary>A delimited identifier in double quotes: <c>"a"</c>.</summary>
	Delimited,

	/// <summary>A Unicode delimited identifier: <c>U&amp;"a"</c>, optionally with <c>UESCAPE</c>.</summary>
	UnicodeDelimited,

	/// <summary>T-SQL: an identifier in square brackets: <c>[a]</c>.</summary>
	Bracketed,

	/// <summary>T-SQL: a column alias written as a string: <c>'a'</c>.</summary>
	String,

	/// <summary>T-SQL: a column alias written as a national string: <c>N'a'</c>.</summary>
	NationalString,
}

/// <summary>
/// A statement: one of the records nested in this one, such as <see cref="Select"/>, <see cref="Insert"/>,
/// <see cref="Update"/>, <see cref="Delete"/>, <see cref="Merge"/> or <see cref="CreateTable"/>. What
/// <c>ParseStatement</c> returns, and what a <see cref="Batch"/> holds.
/// </summary>
/// <remarks>
/// BNF: the top-level SQL statement containers and the executable, schema, data, control, transaction,
/// session and dynamic statement families. Switch on the nested type, or walk the tree with
/// <c>SqlWalker</c>.
/// </remarks>
public abstract partial record Statement : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected Statement()
	{
	}

	/// <summary>
	/// BNF: <c>&lt;direct select statement: multiple rows&gt;</c>, <c>&lt;cursor specification&gt;</c>,
	/// <c>&lt;select statement: single row&gt;</c>, <c>&lt;dynamic single row select statement&gt;</c>. A
	/// query as a statement: the query, and what only a statement has.
	/// </summary>
	/// <remarks>
	/// Everything a query says is in <see cref="Query"/>, which is what a subquery, a derived table, a
	/// common table expression and an INSERT's source hold as well; a statement adds only what no
	/// subquery may write.
	/// </remarks>
	public sealed partial record Select : Statement
	{
		/// <summary>
		/// The query: its clauses, its set operations and its ORDER BY.
		/// </summary>
		public required Query Query { get; init; }

		/// <summary>T-SQL: Query Hints. The statement's <c>OPTION (…)</c>, which a subquery may not write.</summary>
		public SqlList<QueryHint> Hints { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;insert statement&gt;</c>. <c>INSERT INTO t (a, b) …</c>: the table, the columns named, and
	/// the rows, from <c>VALUES</c>, a query or <c>DEFAULT VALUES</c>.
	/// </summary>
	public sealed partial record Insert : Statement
	{
		/// <summary>T-SQL: TOP. <c>INSERT TOP (n) INTO t …</c>.</summary>
		public TopClause? Top { get; init; }

		/// <summary>
		/// The table the rows go into.
		/// </summary>
		public required TableSource Target { get; init; }

		/// <summary>
		/// The columns named after the table, in order; empty where none were.
		/// </summary>
		public SqlList<Identifier> Columns { get; init; } = [];

		/// <summary>
		/// <c>OVERRIDING USER VALUE</c> or <c>OVERRIDING SYSTEM VALUE</c>; null where neither was written.
		/// </summary>
		public OverrideKind? Override { get; init; }

		/// <summary>
		/// Where the rows come from: a query, a <c>VALUES</c> list, or <c>DEFAULT VALUES</c>, the default.
		/// </summary>
		public InsertSource SourceValue { get; init; } = new InsertSource.DefaultValues();
	}

	/// <summary>
	/// BNF: <c>&lt;update statement: searched&gt;</c>, <c>&lt;update statement: positioned&gt;</c>, and the
	/// dynamic and preparable positioned updates. <c>UPDATE t SET a = 1 WHERE …</c>, or
	/// <c>… WHERE CURRENT OF c</c>.
	/// </summary>
	public sealed partial record Update : Statement
	{
		/// <summary>T-SQL: TOP. <c>UPDATE TOP (n) t SET …</c>.</summary>
		public TopClause? Top { get; init; }

		/// <summary>
		/// The table updated; null in a preparable positioned statement, which may leave its table out.
		/// </summary>
		public TableSource? Target { get; init; }

		/// <summary>
		/// <c>FOR PORTION OF p FROM x TO y</c>: the part of an application-time period updated.
		/// </summary>
		public PeriodPortion? Portion { get; init; }

		/// <summary>
		/// The name the table is given after it (<c>UPDATE t AS u SET …</c>).
		/// </summary>
		public Alias? Alias { get; init; }

		/// <summary>
		/// The <c>SET</c> list, in order.
		/// </summary>
		public SqlList<Assignment> Assignments { get; init; } = [];

		/// <summary>
		/// The <c>WHERE</c> condition of a searched update; null where none was written.
		/// </summary>
		public Expression? Where { get; init; }

		/// <summary>
		/// The cursor of <c>WHERE CURRENT OF c</c>, in a positioned update.
		/// </summary>
		public CursorReference? CurrentOf { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;delete statement: searched&gt;</c>, <c>&lt;delete statement: positioned&gt;</c>, and the
	/// dynamic and preparable positioned deletes. <c>DELETE FROM t WHERE …</c>, or
	/// <c>… WHERE CURRENT OF c</c>.
	/// </summary>
	public sealed partial record Delete : Statement
	{
		/// <summary>T-SQL: TOP. <c>DELETE TOP (n) FROM t …</c>.</summary>
		public TopClause? Top { get; init; }

		/// <summary>
		/// The table rows are deleted from; null in a preparable positioned statement, which may leave its
		/// table out.
		/// </summary>
		public TableSource? Target { get; init; }

		/// <summary>
		/// <c>FOR PORTION OF p FROM x TO y</c>: the part of an application-time period deleted.
		/// </summary>
		public PeriodPortion? Portion { get; init; }

		/// <summary>
		/// The name the table is given after it (<c>DELETE FROM t AS u …</c>).
		/// </summary>
		public Alias? Alias { get; init; }

		/// <summary>
		/// The <c>WHERE</c> condition of a searched delete; null where none was written.
		/// </summary>
		public Expression? Where { get; init; }

		/// <summary>
		/// The cursor of <c>WHERE CURRENT OF c</c>, in a positioned delete.
		/// </summary>
		public CursorReference? CurrentOf { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;merge statement&gt;</c>. <c>MERGE INTO t USING s ON … WHEN MATCHED THEN …</c>: a table
	/// changed row by row from another, as each row matches or does not.
	/// </summary>
	public sealed partial record Merge : Statement
	{
		/// <summary>T-SQL: TOP. <c>MERGE TOP (n) INTO t …</c>.</summary>
		public TopClause? Top { get; init; }

		/// <summary>
		/// The table changed.
		/// </summary>
		public required TableSource Target { get; init; }

		/// <summary>
		/// The name the target is given after it.
		/// </summary>
		public Alias? Alias { get; init; }

		/// <summary>
		/// The table source of <c>USING</c>, which the rows come from.
		/// </summary>
		public required TableSource SourceTable { get; init; }

		/// <summary>
		/// The <c>ON</c> condition, which says whether a source row matches a target row.
		/// </summary>
		public required Expression On { get; init; }

		/// <summary>
		/// The <c>WHEN … THEN …</c> clauses, in the order written.
		/// </summary>
		public SqlList<MergeClause> Clauses { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;truncate table statement&gt;</c>. <c>TRUNCATE TABLE t [CONTINUE IDENTITY | RESTART IDENTITY]</c>:
	/// every row of a table removed at once.
	/// </summary>
	public sealed partial record TruncateTable : Statement
	{
		/// <summary>
		/// The table emptied.
		/// </summary>
		public required TableSource Target { get; init; }

		/// <summary>
		/// <c>CONTINUE IDENTITY</c> or <c>RESTART IDENTITY</c>: whether identity columns keep their counters;
		/// null where neither was written.
		/// </summary>
		public IdentityRestart? Identity { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;schema definition&gt;</c>. <c>CREATE SCHEMA [name] [AUTHORIZATION owner] [DEFAULT CHARACTER SET c]
	/// [PATH …] element …</c>: a schema and the statements that create its objects.
	/// </summary>
	public sealed record CreateSchema : Statement
	{
		/// <summary>
		/// The schema's name; null where the schema is named only by its <see cref="Authorization"/>.
		/// </summary>
		public QualifiedName? Name { get; init; }

		/// <summary>
		/// BNF: <c>&lt;schema character set or path&gt;</c>, written either way round: true where <c>PATH</c> was
		/// written before <c>DEFAULT CHARACTER SET</c>.
		/// </summary>
		public bool PathFirst { get; init; }

		/// <summary>
		/// The <c>AUTHORIZATION</c> owner of the schema; null where none was written.
		/// </summary>
		public AuthorizationIdentifier? Authorization { get; init; }

		/// <summary>
		/// The <c>DEFAULT CHARACTER SET</c> of the schema; null where none was written.
		/// </summary>
		public CharacterSetName? DefaultCharacterSet { get; init; }

		/// <summary>
		/// The <c>PATH</c> of schemas searched for unqualified routine names; null where none was written.
		/// </summary>
		public PathSpecification? Path { get; init; }

		/// <summary>
		/// The statements that follow, which create the objects of the schema, in the order written.
		/// </summary>
		public SqlList<Statement> Elements { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;table definition&gt;</c>. <c>CREATE TABLE t (…)</c>: the table's name, and its columns
	/// and constraints, or the query it is made of.
	/// </summary>
	public sealed partial record CreateTable : Statement
	{
		/// <summary>
		/// <c>GLOBAL TEMPORARY</c> or <c>LOCAL TEMPORARY</c>; null for a persistent table.
		/// </summary>
		public TableScope? Scope { get; init; }

		/// <summary>
		/// The table's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// What the table is made of: its column and constraint definitions, a structured type it is of, or
		/// a query.
		/// </summary>
		public required TableContents Contents { get; init; }

		/// <summary>
		/// The standard's <c>WITH SYSTEM VERSIONING</c> was written.
		/// </summary>
		public bool SystemVersioning { get; init; }

		/// <summary>
		/// A temporary table's <c>ON COMMIT PRESERVE ROWS</c> or <c>ON COMMIT DELETE ROWS</c>.
		/// </summary>
		public TableCommitAction? OnCommit { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;alter table statement&gt;</c>. <c>ALTER TABLE t action</c>: a change to a table's columns or
	/// constraints.
	/// </summary>
	public sealed partial record AlterTable : Statement
	{
		/// <summary>
		/// The table altered, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The actions, in the order written. The standard's statement has one; T-SQL may write several in one
		/// statement.
		/// </summary>
		public SqlList<AlterTableAction> Actions { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;drop schema statement&gt;</c>. <c>DROP SCHEMA [IF EXISTS] s [CASCADE | RESTRICT]</c>. T-SQL's
	/// drops name a list, say <c>IF EXISTS</c>, and may leave the behavior out.
	/// </summary>
	public sealed record DropSchema : Statement
	{
		/// <summary>
		/// The schemas dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop table statement&gt;</c>. <c>DROP TABLE [IF EXISTS] t [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropTable : Statement
	{
		/// <summary>
		/// The tables dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;view definition&gt;</c>. <c>CREATE [RECURSIVE] VIEW v [(columns)] AS query [WITH CHECK OPTION]</c>.
	/// </summary>
	public sealed partial record CreateView : Statement
	{
		/// <summary>
		/// <c>RECURSIVE</c> was written before <c>VIEW</c>.
		/// </summary>
		public bool Recursive { get; init; }

		/// <summary>
		/// The view's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// What follows the name: a list of column names, or the type a typed view is of; null where none was
		/// written.
		/// </summary>
		public ViewSpecification? Specification { get; init; }

		/// <summary>
		/// The query after <c>AS</c> that the view is made of.
		/// </summary>
		public required Query Query { get; init; }

		/// <summary>
		/// <c>WITH [CASCADED | LOCAL] CHECK OPTION</c>; null where it was not written.
		/// </summary>
		public CheckOption? CheckOption { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop view statement&gt;</c>. <c>DROP VIEW [IF EXISTS] v [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropView : Statement
	{
		/// <summary>
		/// The views dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;domain definition&gt;</c>. <c>CREATE DOMAIN d [AS] type [DEFAULT x] [constraint …] [COLLATE c]</c>:
	/// a named data type with a default and constraints.
	/// </summary>
	public sealed record CreateDomain : Statement
	{
		/// <summary>
		/// The domain's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The optional <c>AS</c> after the name was written.
		/// </summary>
		public bool AsKeyword { get; init; }

		/// <summary>
		/// The data type the domain is based on.
		/// </summary>
		public required DataType Type { get; init; }

		/// <summary>
		/// The <c>DEFAULT</c> value; null where none was written.
		/// </summary>
		public Expression? Default { get; init; }

		/// <summary>
		/// The domain's constraints, in the order written.
		/// </summary>
		public SqlList<Constraint> Constraints { get; init; } = [];

		/// <summary>
		/// The <c>COLLATE</c> collation; null where none was written.
		/// </summary>
		public CollationName? Collation { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;alter domain statement&gt;</c>. <c>ALTER DOMAIN d action</c>: a change to a domain's default or
	/// constraints.
	/// </summary>
	public sealed record AlterDomain : Statement
	{
		/// <summary>
		/// The domain altered, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The change: <c>SET DEFAULT</c>, <c>DROP DEFAULT</c>, <c>ADD constraint</c> or <c>DROP CONSTRAINT</c>.
		/// </summary>
		public required AlterDomainAction Action { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop domain statement&gt;</c>. <c>DROP DOMAIN d CASCADE | RESTRICT</c>.
	/// </summary>
	public sealed record DropDomain : Statement
	{
		/// <summary>
		/// The domain dropped, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>.
		/// </summary>
		public required DropBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;character set definition&gt;</c>. <c>CREATE CHARACTER SET c [AS] GET source [COLLATE x]</c>:
	/// a character set made from another.
	/// </summary>
	public sealed record CreateCharacterSet : Statement
	{
		/// <summary>
		/// The character set created.
		/// </summary>
		public required CharacterSetName Name { get; init; }

		/// <summary>
		/// The optional <c>AS</c> after the name was written.
		/// </summary>
		public bool AsKeyword { get; init; }

		/// <summary>
		/// The character set named after <c>GET</c>, which the new one is taken from.
		/// </summary>
		public required CharacterSetName Source { get; init; }

		/// <summary>
		/// The <c>COLLATE</c> collation; null where none was written.
		/// </summary>
		public CollationName? Collation { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop character set statement&gt;</c>. <c>DROP CHARACTER SET c</c>.
	/// </summary>
	public sealed record DropCharacterSet : Statement
	{
		/// <summary>
		/// The character set dropped.
		/// </summary>
		public required CharacterSetName Name { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;collation definition&gt;</c>. <c>CREATE COLLATION c FOR set FROM source [NO PAD | PAD SPACE]</c>:
	/// a collation for a character set, made from another collation.
	/// </summary>
	public sealed record CreateCollation : Statement
	{
		/// <summary>
		/// The collation created.
		/// </summary>
		public required CollationName Name { get; init; }

		/// <summary>
		/// The character set after <c>FOR</c>, which the collation orders.
		/// </summary>
		public required CharacterSetName CharacterSet { get; init; }

		/// <summary>
		/// The collation after <c>FROM</c>, which the new one is taken from.
		/// </summary>
		public required CollationName Source { get; init; }

		/// <summary>
		/// <c>NO PAD</c> or <c>PAD SPACE</c>; null where neither was written.
		/// </summary>
		public PadCharacteristic? Padding { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop collation statement&gt;</c>. <c>DROP COLLATION c CASCADE | RESTRICT</c>.
	/// </summary>
	public sealed record DropCollation : Statement
	{
		/// <summary>
		/// The collation dropped.
		/// </summary>
		public required CollationName Name { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>.
		/// </summary>
		public required DropBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;transliteration definition&gt;</c>. <c>CREATE TRANSLATION t FOR source TO target FROM x</c>: a
	/// conversion between two character sets.
	/// </summary>
	public sealed record CreateTranslation : Statement
	{
		/// <summary>
		/// The translation created, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The character set after <c>FOR</c>, which the translation converts from.
		/// </summary>
		public required CharacterSetName SourceCharacterSet { get; init; }

		/// <summary>
		/// The character set after <c>TO</c>, which the translation converts to.
		/// </summary>
		public required CharacterSetName TargetCharacterSet { get; init; }

		/// <summary>
		/// What follows <c>FROM</c>: the routine that performs the conversion, or an existing translation.
		/// </summary>
		public required TranslationSource Source { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop transliteration statement&gt;</c>. <c>DROP TRANSLATION t</c>.
	/// </summary>
	public sealed record DropTranslation : Statement
	{
		/// <summary>
		/// The translation dropped, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;assertion definition&gt;</c>. <c>CREATE ASSERTION a CHECK (condition)</c>: a condition the
	/// database must hold at all times, not tied to one table.
	/// </summary>
	public sealed record CreateAssertion : Statement
	{
		/// <summary>
		/// The assertion's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The condition in <c>CHECK (…)</c>.
		/// </summary>
		public required Expression Condition { get; init; }

		/// <summary>
		/// The <c>[NOT] DEFERRABLE</c> and <c>INITIALLY …</c> characteristics; null where none was written.
		/// </summary>
		public ConstraintCharacteristics? Characteristics { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop assertion statement&gt;</c>. <c>DROP ASSERTION a [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropAssertion : Statement
	{
		/// <summary>
		/// The assertion dropped, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;trigger definition&gt;</c>. <c>CREATE TRIGGER t BEFORE | AFTER | INSTEAD OF event ON table …</c>:
	/// statements run when a table changes. Also T-SQL's <c>CREATE TRIGGER</c>, which holds its own members
	/// beside these and leaves <see cref="Event"/> and <see cref="Action"/> null.
	/// </summary>
	public sealed partial record CreateTrigger : Statement
	{
		/// <summary>
		/// The trigger's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// When the trigger fires relative to the event: <c>BEFORE</c>, <c>AFTER</c> or <c>INSTEAD OF</c>.
		/// </summary>
		public required TriggerTime Time { get; init; }

		/// <summary>
		/// The event that fires the trigger (<c>INSERT</c>, <c>DELETE</c> or <c>UPDATE [OF columns]</c>); null in
		/// a T-SQL trigger, which lists its events separately.
		/// </summary>
		public required TriggerEvent? Event { get; init; }

		/// <summary>
		/// The table after <c>ON</c>; null in a T-SQL trigger on the database or the server.
		/// </summary>
		public required QualifiedName? Table { get; init; }

		/// <summary>
		/// The <c>REFERENCING</c> aliases for the old and new rows or tables, in the order written.
		/// </summary>
		public SqlList<TransitionReference> Referencing { get; init; } = [];

		/// <summary>
		/// What runs: the <c>FOR EACH</c> granularity, the <c>WHEN</c> condition and the statements; null in a
		/// T-SQL trigger, whose body is held by its own member.
		/// </summary>
		public required TriggerAction? Action { get; init; }
	}

	/// <summary>
	/// T-SQL: DROP TRIGGER (Transact-SQL). <c>DROP TRIGGER [IF EXISTS] t, … [ON DATABASE | ON ALL SERVER]</c>.
	/// </summary>
	public sealed record DropTrigger : Statement
	{
		/// <summary>
		/// The triggers dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// The DDL trigger's scope, which the page publishes as <c>ON { DATABASE | ALL SERVER }</c> and requires
		/// there; null is the DML form, which has no scope.
		/// </summary>
		public TriggerScope? On { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;user-defined type definition&gt;</c>. <c>CREATE TYPE t …</c>: a distinct or structured type.
	/// </summary>
	public sealed record CreateType : Statement
	{
		/// <summary>
		/// The type's name, representation and members.
		/// </summary>
		public required UserDefinedTypeDefinition Definition { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;alter type statement&gt;</c>. <c>ALTER TYPE t ADD | DROP ATTRIBUTE | METHOD …</c>: a change to
	/// a structured type's attributes or methods.
	/// </summary>
	public sealed record AlterType : Statement
	{
		/// <summary>
		/// The type altered, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The change: an attribute or a method added or dropped.
		/// </summary>
		public required AlterTypeAction Action { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop data type statement&gt;</c>. <c>DROP TYPE [IF EXISTS] t [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropType : Statement
	{
		/// <summary>
		/// The types dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;user-defined cast definition&gt;</c>. <c>CREATE CAST (source AS target) WITH FUNCTION f [AS ASSIGNMENT]</c>:
	/// a conversion between two types performed by a function.
	/// </summary>
	public sealed record CreateCast : Statement
	{
		/// <summary>
		/// The type converted from.
		/// </summary>
		public required DataType SourceType { get; init; }

		/// <summary>
		/// The type converted to.
		/// </summary>
		public required DataType TargetType { get; init; }

		/// <summary>
		/// The routine after <c>WITH</c>, which performs the conversion.
		/// </summary>
		public required RoutineDesignator Function { get; init; }

		/// <summary>
		/// <c>AS ASSIGNMENT</c> was written, so the cast is also applied implicitly in assignments.
		/// </summary>
		public bool AsAssignment { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop user-defined cast statement&gt;</c>. <c>DROP CAST (source AS target) CASCADE | RESTRICT</c>.
	/// </summary>
	public sealed record DropCast : Statement
	{
		/// <summary>
		/// The type converted from.
		/// </summary>
		public required DataType SourceType { get; init; }

		/// <summary>
		/// The type converted to.
		/// </summary>
		public required DataType TargetType { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>.
		/// </summary>
		public required DropBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;user-defined ordering definition&gt;</c>. <c>CREATE ORDERING FOR t EQUALS ONLY | ORDER FULL BY …</c>:
	/// how values of a user-defined type are compared.
	/// </summary>
	public sealed record CreateOrdering : Statement
	{
		/// <summary>
		/// The user-defined type the ordering is for.
		/// </summary>
		public required QualifiedName TypeName { get; init; }

		/// <summary>
		/// The ordering: whether it covers equality only or the full order, and how it is computed
		/// (<c>RELATIVE</c>, <c>MAP</c> or <c>STATE</c>).
		/// </summary>
		public required OrderingDefinition Ordering { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop user-defined ordering statement&gt;</c>. <c>DROP ORDERING FOR t CASCADE | RESTRICT</c>.
	/// </summary>
	public sealed record DropOrdering : Statement
	{
		/// <summary>
		/// The user-defined type whose ordering is dropped.
		/// </summary>
		public required QualifiedName TypeName { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>.
		/// </summary>
		public required DropBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;transform definition&gt;</c>. <c>CREATE TRANSFORM FOR t group (TO SQL WITH f, FROM SQL WITH g)</c>:
	/// functions that convert a user-defined type to and from host-language values, grouped by name.
	/// </summary>
	public sealed record CreateTransform : Statement
	{
		/// <summary>
		/// <c>TRANSFORMS</c> was written in place of <c>TRANSFORM</c>.
		/// </summary>
		public bool PluralKeyword { get; init; }

		/// <summary>
		/// The user-defined type the transforms are for.
		/// </summary>
		public required QualifiedName TypeName { get; init; }

		/// <summary>
		/// The named groups of transform functions, in the order written.
		/// </summary>
		public SqlList<TransformGroup> Groups { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;alter transform statement&gt;</c>. <c>ALTER TRANSFORM FOR t group (ADD (…) | DROP (…))</c>: a change
	/// to the transform functions of a user-defined type.
	/// </summary>
	public sealed record AlterTransform : Statement
	{
		/// <summary>
		/// <c>TRANSFORMS</c> was written in place of <c>TRANSFORM</c>.
		/// </summary>
		public bool PluralKeyword { get; init; }

		/// <summary>
		/// The user-defined type the transforms are for.
		/// </summary>
		public required QualifiedName TypeName { get; init; }

		/// <summary>
		/// The groups altered, each with its additions and removals, in the order written.
		/// </summary>
		public SqlList<TransformAlterGroup> Groups { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;drop transform statement&gt;</c>. <c>DROP TRANSFORM group | ALL FOR t CASCADE | RESTRICT</c>.
	/// </summary>
	public sealed record DropTransform : Statement
	{
		/// <summary>
		/// <c>TRANSFORMS</c> was written in place of <c>TRANSFORM</c>.
		/// </summary>
		public bool PluralKeyword { get; init; }

		/// <summary>
		/// What is dropped: one named group, or <c>ALL</c> groups.
		/// </summary>
		public required TransformDropTarget Target { get; init; }

		/// <summary>
		/// The user-defined type the transforms are for.
		/// </summary>
		public required QualifiedName TypeName { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>.
		/// </summary>
		public required DropBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;schema routine&gt;</c>. <c>CREATE PROCEDURE | FUNCTION | …</c>: a stored routine, with its
	/// parameters, characteristics and body.
	/// </summary>
	public sealed partial record CreateRoutine : Statement
	{
		/// <summary>
		/// The routine's kind, name, parameters, characteristics and body.
		/// </summary>
		public required RoutineDefinition Definition { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;alter routine statement&gt;</c>. <c>ALTER routine characteristic … RESTRICT</c>: a change to a
	/// routine's characteristics.
	/// </summary>
	public sealed record AlterRoutine : Statement
	{
		/// <summary>
		/// The routine altered: its kind and its name, with the parameter types where they tell routines apart.
		/// </summary>
		public required RoutineDesignator Routine { get; init; }

		/// <summary>
		/// The characteristics set, in the order written.
		/// </summary>
		public SqlList<RoutineCharacteristic> Characteristics { get; init; } = [];

		/// <summary>
		/// The trailing <c>RESTRICT</c>.
		/// </summary>
		public required AlterRoutineBehavior Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop routine statement&gt;</c>. <c>DROP PROCEDURE | FUNCTION | … r [CASCADE | RESTRICT]</c>.
	/// T-SQL's list, <c>DROP PROCEDURE IF EXISTS p, q</c>, names the kind once, the first routine's, and
	/// <c>IF EXISTS</c> after it; the standard's statement drops one routine and has no <c>IF EXISTS</c>.
	/// </summary>
	public sealed record DropRoutine : Statement
	{
		/// <summary>
		/// The routines dropped, in the order written; all of one kind.
		/// </summary>
		public SqlList<RoutineDesignator> Routines { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }

		/// <summary>
		/// T-SQL: DROP PROCEDURE (Transact-SQL). The page publishes <c>DROP { PROC | PROCEDURE }</c>, two words
		/// for one statement, so which was written is kept beside it or <c>DROP PROC p</c> writes back as
		/// <c>DROP PROCEDURE p</c>. True where <c>PROC</c> was written.
		/// </summary>
		[Spelling] public bool Proc { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;sequence generator definition&gt;</c>. <c>CREATE SEQUENCE s [option …]</c>: a generator of
	/// consecutive numbers.
	/// </summary>
	public sealed record CreateSequence : Statement
	{
		/// <summary>
		/// The sequence's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The options (<c>START WITH</c>, <c>INCREMENT BY</c>, <c>MAXVALUE</c>, …), in the order written.
		/// </summary>
		public SqlList<SequenceOption> Options { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;alter sequence generator statement&gt;</c>. <c>ALTER SEQUENCE s option …</c>: a change to a
	/// sequence's options.
	/// </summary>
	public sealed record AlterSequence : Statement
	{
		/// <summary>
		/// The sequence altered, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The options changed (<c>RESTART</c>, <c>INCREMENT BY</c>, …), in the order written.
		/// </summary>
		public SqlList<SequenceOption> Options { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;drop sequence generator statement&gt;</c>. <c>DROP SEQUENCE [IF EXISTS] s [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropSequence : Statement
	{
		/// <summary>
		/// The sequences dropped, in the order written.
		/// </summary>
		public SqlList<QualifiedName> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;grant statement&gt;</c>. <c>GRANT privileges ON object TO grantees</c>, or <c>GRANT roles TO grantees</c>.
	/// </summary>
	public sealed record Grant : Statement
	{
		/// <summary>
		/// What is granted and to whom: privileges on an object, or roles.
		/// </summary>
		public required GrantBody Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;revoke statement&gt;</c>. <c>REVOKE privileges ON object FROM grantees</c>, or <c>REVOKE roles FROM grantees</c>.
	/// </summary>
	public sealed record Revoke : Statement
	{
		/// <summary>
		/// What is revoked and from whom: privileges on an object, or roles.
		/// </summary>
		public required RevokeBody Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;role definition&gt;</c>. <c>CREATE ROLE r [WITH ADMIN CURRENT_USER | CURRENT_ROLE]</c>.
	/// </summary>
	public sealed partial record CreateRole : Statement
	{
		/// <summary>
		/// The role created.
		/// </summary>
		public required Identifier Name { get; init; }

		/// <summary>
		/// The <c>WITH ADMIN</c> grantor, <c>CURRENT_USER</c> or <c>CURRENT_ROLE</c>; null where none was written.
		/// </summary>
		public Grantor? Admin { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;drop role statement&gt;</c>. <c>DROP ROLE [IF EXISTS] r [CASCADE | RESTRICT]</c>.
	/// </summary>
	public sealed record DropRole : Statement
	{
		/// <summary>
		/// The roles dropped, in the order written.
		/// </summary>
		public SqlList<Identifier> Names { get; init; } = [];

		/// <summary>
		/// <c>IF EXISTS</c> was written.
		/// </summary>
		public bool IfExists { get; init; }

		/// <summary>
		/// <c>CASCADE</c> or <c>RESTRICT</c>; null where neither was written.
		/// </summary>
		public DropBehavior? Behavior { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;declare cursor&gt;</c>. <c>DECLARE c [INSENSITIVE] [SCROLL] CURSOR FOR query</c>. T-SQL: DECLARE
	/// CURSOR (Transact-SQL) publishes two forms: the ISO one writes the properties before the word
	/// <c>CURSOR</c>, the extended one after it.
	/// </summary>
	public sealed record DeclareCursor : Statement
	{
		/// <summary>
		/// The cursor declared.
		/// </summary>
		public required CursorReference Cursor { get; init; }

		/// <summary>
		/// The cursor's sensitivity, scrollability, holdability and returnability; each is null where it was
		/// not written.
		/// </summary>
		public required CursorProperties Properties { get; init; }

		/// <summary>
		/// What the cursor reads: the query after <c>FOR</c>, with its updatability.
		/// </summary>
		public required CursorSource SourceValue { get; init; }

		/// <summary>
		/// T-SQL: the extended form was written, with the properties after the word <c>CURSOR</c>. With no
		/// property written at all the two forms print the same, so the flag only decides anything once one
		/// was.
		/// </summary>
		[Spelling] public bool Extended { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;open statement&gt;</c>, <c>&lt;dynamic open statement&gt;</c>. <c>OPEN c [USING args]</c>.
	/// </summary>
	public sealed record OpenCursor : Statement
	{
		/// <summary>
		/// The cursor opened.
		/// </summary>
		public required CursorReference Cursor { get; init; }

		/// <summary>
		/// The arguments after <c>USING</c>, which a dynamic cursor binds to its parameters; null where none
		/// were written.
		/// </summary>
		public DynamicArguments? Using { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;fetch statement&gt;</c>, <c>&lt;dynamic fetch statement&gt;</c>. <c>FETCH [orientation] [FROM] c [INTO targets]</c>:
	/// one row read from a cursor.
	/// </summary>
	public sealed record FetchCursor : Statement
	{
		/// <summary>
		/// The orientation (<c>NEXT</c>, <c>PRIOR</c>, <c>FIRST</c>, <c>LAST</c>, <c>ABSOLUTE n</c> or <c>RELATIVE n</c>);
		/// null where none was written.
		/// </summary>
		public FetchOrientation? Orientation { get; init; }

		/// <summary>
		/// <c>FROM</c> was written before the cursor.
		/// </summary>
		public bool FromKeyword { get; init; }

		/// <summary>
		/// The cursor fetched from.
		/// </summary>
		public required CursorReference Cursor { get; init; }

		/// <summary>
		/// The targets after <c>INTO</c>, or <c>INTO [SQL] DESCRIPTOR d</c>; null where <c>INTO</c> was left out,
		/// as T-SQL may, and the row goes to the client.
		/// </summary>
		public DynamicArguments? Into { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;close statement&gt;</c>. <c>CLOSE c</c>.
	/// </summary>
	public sealed record CloseCursor : Statement
	{
		/// <summary>
		/// The cursor closed.
		/// </summary>
		public required CursorReference Cursor { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;allocate received cursor statement&gt;</c>, <c>&lt;allocate extended dynamic cursor statement&gt;</c>.
	/// <c>ALLOCATE c [properties] CURSOR FOR statement | routine</c>: a cursor taken from a prepared
	/// statement or from a routine's result.
	/// </summary>
	public sealed record AllocateCursor : Statement
	{
		/// <summary>
		/// The cursor allocated.
		/// </summary>
		public required CursorReference Cursor { get; init; }

		/// <summary>
		/// The word <c>CURSOR</c> was written; where <see cref="Properties"/> is set it is always written.
		/// </summary>
		public bool CursorKeyword { get; init; }

		/// <summary>
		/// The properties written before and after the word <c>CURSOR</c>; null where none were.
		/// </summary>
		public CursorProperties? Properties { get; init; }

		/// <summary>
		/// What follows <c>FOR</c>: a prepared statement, or a routine whose result is received.
		/// </summary>
		public required CursorAllocationSource SourceValue { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;temporary table declaration&gt;</c>. <c>DECLARE LOCAL TEMPORARY TABLE t (…) [ON COMMIT …]</c>.
	/// </summary>
	public sealed record DeclareLocalTemporaryTable : Statement
	{
		/// <summary>
		/// The table's name, as written.
		/// </summary>
		public required QualifiedName Name { get; init; }

		/// <summary>
		/// The column and constraint definitions, in the order written.
		/// </summary>
		public SqlList<TableElement> Elements { get; init; } = [];

		/// <summary>
		/// <c>ON COMMIT PRESERVE ROWS</c> or <c>ON COMMIT DELETE ROWS</c>; null where neither was written.
		/// </summary>
		public TableCommitAction? OnCommit { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;get diagnostics statement&gt;</c>. <c>GET DIAGNOSTICS target = item, …</c>: details of the
	/// last statement or of a condition it raised, read into variables.
	/// </summary>
	public sealed record GetDiagnostics : Statement
	{
		/// <summary>
		/// What is read: the statement's information items, or the items of one numbered condition.
		/// </summary>
		public required DiagnosticsInformation Information { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;free locator statement&gt;</c>. <c>FREE LOCATOR l, …</c>: LOB locators released.
	/// </summary>
	public sealed record FreeLocator : Statement
	{
		/// <summary>
		/// The locators freed, in the order written.
		/// </summary>
		public SqlList<Expression> Locators { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;hold locator statement&gt;</c>. <c>HOLD LOCATOR l, …</c>: LOB locators kept past the end of the
	/// transaction.
	/// </summary>
	public sealed record HoldLocator : Statement
	{
		/// <summary>
		/// The locators held, in the order written.
		/// </summary>
		public SqlList<Expression> Locators { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;call statement&gt;</c>. <c>CALL p(args)</c>: a procedure invoked.
	/// </summary>
	public sealed record Call : Statement
	{
		/// <summary>
		/// The routine invocation: its name and arguments.
		/// </summary>
		public required Expression.Invocation Invocation { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;return statement&gt;</c>. <c>RETURN value</c> or <c>RETURN NULL</c>: a routine's result. T-SQL's
	/// bare <c>RETURN</c> has neither.
	/// </summary>
	public sealed record Return : Statement
	{
		/// <summary>
		/// The value returned; null for <c>RETURN NULL</c> and for a bare <c>RETURN</c>.
		/// </summary>
		public Expression? Value { get; init; }

		/// <summary>
		/// <c>NULL</c> was written as a key word of its own rather than as a value (<c>RETURN NULL</c>).
		/// </summary>
		public bool NullKeyword { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;start transaction statement&gt;</c>. <c>START TRANSACTION [mode, …]</c>.
	/// </summary>
	public sealed record StartTransaction : Statement
	{
		/// <summary>
		/// The modes (<c>ISOLATION LEVEL …</c>, <c>READ ONLY</c>, <c>READ WRITE</c>, <c>DIAGNOSTICS SIZE n</c>), in the order
		/// written.
		/// </summary>
		public SqlList<TransactionMode> Modes { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;set transaction statement&gt;</c>. <c>SET [LOCAL] TRANSACTION mode, …</c>. T-SQL: SET TRANSACTION
	/// ISOLATION LEVEL (Transact-SQL).
	/// </summary>
	public sealed record SetTransaction : Statement
	{
		/// <summary>
		/// <c>LOCAL</c> was written: the modes apply to the next transaction of this connection only.
		/// </summary>
		public bool Local { get; init; }

		/// <summary>
		/// The modes (<c>ISOLATION LEVEL …</c>, <c>READ ONLY</c>, <c>READ WRITE</c>, <c>DIAGNOSTICS SIZE n</c>), in the order
		/// written.
		/// </summary>
		public SqlList<TransactionMode> Modes { get; init; } = [];

		/// <summary>
		/// T-SQL: <c>TRAN</c> was written where the page publishes it for <c>TRANSACTION</c>.
		/// </summary>
		[Spelling] public bool Tran { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set constraints mode statement&gt;</c>. <c>SET CONSTRAINTS ALL | c, … DEFERRED | IMMEDIATE</c>.
	/// </summary>
	public sealed record SetConstraints : Statement
	{
		/// <summary>
		/// The constraints affected: <c>ALL</c>, which is the default, or a list of names.
		/// </summary>
		public ConstraintTarget Target { get; init; } = new ConstraintTarget.All();

		/// <summary>
		/// <c>DEFERRED</c> or <c>IMMEDIATE</c>.
		/// </summary>
		public required ConstraintTiming Timing { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;savepoint statement&gt;</c>. <c>SAVEPOINT s</c>: a point in the transaction to roll back to.
	/// </summary>
	public sealed record Savepoint : Statement
	{
		/// <summary>
		/// The savepoint's name.
		/// </summary>
		public required Identifier Name { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;release savepoint statement&gt;</c>. <c>RELEASE SAVEPOINT s</c>.
	/// </summary>
	public sealed record ReleaseSavepoint : Statement
	{
		/// <summary>
		/// The savepoint released.
		/// </summary>
		public required Identifier Name { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;commit statement&gt;</c>. <c>COMMIT [WORK] [AND [NO] CHAIN]</c>. T-SQL: COMMIT TRANSACTION, COMMIT
	/// WORK (Transact-SQL), which also names the transaction and may set delayed durability.
	/// </summary>
	public sealed record Commit : Statement
	{
		/// <summary>
		/// <c>WORK</c> was written.
		/// </summary>
		public bool Work { get; init; }

		/// <summary>
		/// <c>AND CHAIN</c> or <c>AND NO CHAIN</c>; null where neither was written.
		/// </summary>
		public ChainMode? Chain { get; init; }

		/// <summary>
		/// T-SQL: the word after <c>COMMIT</c>, which the page makes optional: <c>COMMIT</c> and
		/// <c>COMMIT TRANSACTION</c> are both text and are not the same text. Null is no word and each value is
		/// one. A flag for the word beside a flag for its spelling would have had a state no text says:
		/// <c>TRAN</c> written and no keyword.
		/// </summary>
		[Spelling] public TransactionKeyword? Keyword { get; init; }

		/// <summary>
		/// T-SQL: the transaction a commit may name; null where none was written.
		/// </summary>
		public TransactionName? Name { get; init; }

		/// <summary>
		/// T-SQL: <c>WITH (DELAYED_DURABILITY = { OFF | ON })</c>: true for <c>ON</c>, false for <c>OFF</c>; null
		/// is no <c>WITH</c> at all.
		/// </summary>
		public bool? DelayedDurability { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;rollback statement&gt;</c>. <c>ROLLBACK [WORK] [AND [NO] CHAIN] [TO SAVEPOINT s]</c>. T-SQL:
	/// ROLLBACK TRANSACTION, ROLLBACK WORK (Transact-SQL).
	/// </summary>
	public sealed record Rollback : Statement
	{
		/// <summary>
		/// <c>WORK</c> was written.
		/// </summary>
		public bool Work { get; init; }

		/// <summary>
		/// <c>AND CHAIN</c> or <c>AND NO CHAIN</c>; null where neither was written.
		/// </summary>
		public ChainMode? Chain { get; init; }

		/// <summary>
		/// The standard's <c>TO SAVEPOINT s</c>, a different construct from <see cref="Name"/> that knows what it
		/// names; null where it was not written. At most one of the two is set: the writer refuses a node with
		/// both.
		/// </summary>
		public Identifier? ToSavepoint { get; init; }

		/// <summary>
		/// T-SQL: the word after <c>ROLLBACK</c>, as <see cref="Commit.Keyword"/> is.
		/// </summary>
		[Spelling] public TransactionKeyword? Keyword { get; init; }

		/// <summary>
		/// T-SQL: what is written after <c>ROLLBACK TRAN</c>, which its page says may be a transaction or a
		/// savepoint and which the syntax cannot tell apart; null where nothing was written.
		/// </summary>
		public TransactionName? Name { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;connect statement&gt;</c>. <c>CONNECT TO server [AS name] [USER user]</c>, or
	/// <c>CONNECT TO DEFAULT</c>.
	/// </summary>
	public sealed record Connect : Statement
	{
		/// <summary>
		/// The server, connection name and user connected as, or <c>DEFAULT</c>.
		/// </summary>
		public required ConnectionTarget Target { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set connection statement&gt;</c>. <c>SET CONNECTION name</c> or <c>SET CONNECTION DEFAULT</c>.
	/// </summary>
	public sealed record SetConnection : Statement
	{
		/// <summary>
		/// The connection made current: a named one, or the default.
		/// </summary>
		public required ConnectionObject Connection { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;disconnect statement&gt;</c>. <c>DISCONNECT name | DEFAULT | CURRENT | ALL</c>.
	/// </summary>
	public sealed record Disconnect : Statement
	{
		/// <summary>
		/// What is disconnected: a named connection, the default one, the current one, or all of them.
		/// </summary>
		public required DisconnectObject Object { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set session user identifier statement&gt;</c>. <c>SET SESSION AUTHORIZATION user</c>.
	/// </summary>
	public sealed record SetSessionAuthorization : Statement
	{
		/// <summary>
		/// The user the session continues as.
		/// </summary>
		public required Expression Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set role statement&gt;</c>. <c>SET ROLE role</c> or <c>SET ROLE NONE</c>.
	/// </summary>
	public sealed record SetRole : Statement
	{
		/// <summary>
		/// The role set; null where <see cref="None"/> is true.
		/// </summary>
		public Expression? Value { get; init; }

		/// <summary>
		/// <c>NONE</c> was written, which sets no role.
		/// </summary>
		public bool None { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set local time zone statement&gt;</c>. <c>SET TIME ZONE interval</c> or <c>SET TIME ZONE LOCAL</c>.
	/// </summary>
	public sealed record SetTimeZone : Statement
	{
		/// <summary>
		/// The time zone offset set; null where <see cref="Local"/> is true.
		/// </summary>
		public Expression? Value { get; init; }

		/// <summary>
		/// <c>LOCAL</c> was written, which restores the system's time zone.
		/// </summary>
		public bool Local { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set catalog statement&gt;</c>. <c>SET CATALOG name</c>: the default catalog of the session.
	/// </summary>
	public sealed record SetCatalog : Statement
	{
		/// <summary>
		/// The catalog name, as a value.
		/// </summary>
		public required Expression Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set schema statement&gt;</c>. <c>SET SCHEMA name</c>: the default schema of the session.
	/// </summary>
	public sealed record SetSchema : Statement
	{
		/// <summary>
		/// The schema name, as a value.
		/// </summary>
		public required Expression Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set names statement&gt;</c>. <c>SET NAMES name</c>: the default character set of the session.
	/// </summary>
	public sealed record SetNames : Statement
	{
		/// <summary>
		/// The character set name, as a value.
		/// </summary>
		public required Expression Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set path statement&gt;</c>. <c>SET PATH list</c>: the schemas searched for unqualified routine
	/// names.
	/// </summary>
	public sealed record SetPath : Statement
	{
		/// <summary>
		/// The path, as a value.
		/// </summary>
		public required Expression Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set transform group statement&gt;</c>. <c>SET DEFAULT TRANSFORM GROUP g</c> or
	/// <c>SET TRANSFORM GROUP FOR TYPE t g</c>.
	/// </summary>
	public sealed record SetTransformGroup : Statement
	{
		/// <summary>
		/// The group name, and the type it is for where <c>FOR TYPE</c> was written.
		/// </summary>
		public required TransformGroupCharacteristic Value { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set session collation statement&gt;</c>. <c>SET COLLATION c [FOR sets]</c> or
	/// <c>SET NO COLLATION [FOR sets]</c>.
	/// </summary>
	public sealed record SetCollation : Statement
	{
		/// <summary>
		/// <c>SET NO COLLATION</c> was written; <see cref="Value"/> is null then.
		/// </summary>
		public bool NoCollation { get; init; }

		/// <summary>
		/// The collation set, as a value; null where <see cref="NoCollation"/> is true.
		/// </summary>
		public Expression? Value { get; init; }

		/// <summary>
		/// The character sets after <c>FOR</c>, which the collation applies to; empty where none were written.
		/// </summary>
		public SqlList<CharacterSetName> ForCharacterSets { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;set session characteristics statement&gt;</c>. <c>SET SESSION CHARACTERISTICS AS TRANSACTION mode, …</c>:
	/// the default transaction modes of the session.
	/// </summary>
	public sealed record SetSessionCharacteristics : Statement
	{
		/// <summary>
		/// One list of modes per <c>TRANSACTION …</c> written, in the order written.
		/// </summary>
		public SqlList<SqlList<TransactionMode>> Characteristics { get; init; } = [];
	}

	/// <summary>
	/// BNF: <c>&lt;allocate descriptor statement&gt;</c>. <c>ALLOCATE [SQL] DESCRIPTOR d [WITH MAX n]</c>: a descriptor
	/// area for dynamic SQL.
	/// </summary>
	public sealed record AllocateDescriptor : Statement
	{
		/// <summary>
		/// The optional <c>SQL</c> before <c>DESCRIPTOR</c> was written.
		/// </summary>
		public bool SqlKeyword { get; init; }

		/// <summary>
		/// The descriptor allocated.
		/// </summary>
		public required DescriptorReference Descriptor { get; init; }

		/// <summary>
		/// The number of items after <c>WITH MAX</c>; null where it was not written.
		/// </summary>
		public Expression? Max { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;deallocate descriptor statement&gt;</c>. <c>DEALLOCATE [SQL] DESCRIPTOR d</c>.
	/// </summary>
	public sealed record DeallocateDescriptor : Statement
	{
		/// <summary>
		/// The optional <c>SQL</c> before <c>DESCRIPTOR</c> was written.
		/// </summary>
		public bool SqlKeyword { get; init; }

		/// <summary>
		/// The descriptor deallocated.
		/// </summary>
		public required DescriptorReference Descriptor { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;get descriptor statement&gt;</c>. <c>GET [SQL] DESCRIPTOR d target = field, …</c> or
	/// <c>… VALUE n target = field, …</c>: fields of a descriptor area read into variables.
	/// </summary>
	public sealed record GetDescriptor : Statement
	{
		/// <summary>
		/// The optional <c>SQL</c> before <c>DESCRIPTOR</c> was written.
		/// </summary>
		public bool SqlKeyword { get; init; }

		/// <summary>
		/// The descriptor read.
		/// </summary>
		public required DescriptorReference Descriptor { get; init; }

		/// <summary>
		/// What is read: header fields, or the fields of the item after <c>VALUE</c>.
		/// </summary>
		public required DescriptorGet Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;set descriptor statement&gt;</c>. <c>SET [SQL] DESCRIPTOR d field = value, …</c> or
	/// <c>… VALUE n field = value, …</c>: fields of a descriptor area written.
	/// </summary>
	public sealed record SetDescriptor : Statement
	{
		/// <summary>
		/// The optional <c>SQL</c> before <c>DESCRIPTOR</c> was written.
		/// </summary>
		public bool SqlKeyword { get; init; }

		/// <summary>
		/// The descriptor written.
		/// </summary>
		public required DescriptorReference Descriptor { get; init; }

		/// <summary>
		/// What is written: header fields, or the fields of the item after <c>VALUE</c>.
		/// </summary>
		public required DescriptorSet Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;copy descriptor statement&gt;</c>. <c>COPY d1 TO d2</c>, or <c>COPY d1 VALUE n (options) TO d2 VALUE m</c>:
	/// a descriptor area, or one item of it, copied.
	/// </summary>
	public sealed record CopyDescriptor : Statement
	{
		/// <summary>
		/// What is copied: a whole descriptor, or one item with the fields chosen.
		/// </summary>
		public required DescriptorCopy Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;prepare statement&gt;</c>. <c>PREPARE s [ATTRIBUTES a] FROM sql</c>: a dynamic statement made
	/// ready to execute.
	/// </summary>
	public sealed record Prepare : Statement
	{
		/// <summary>
		/// The name the prepared statement is given.
		/// </summary>
		public required StatementReference Statement { get; init; }

		/// <summary>
		/// The cursor attributes string after <c>ATTRIBUTES</c>; null where it was not written.
		/// </summary>
		public Expression? Attributes { get; init; }

		/// <summary>
		/// The text of the statement, as a value.
		/// </summary>
		public required Expression Sql { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;deallocate prepared statement&gt;</c>. <c>DEALLOCATE PREPARE s</c>.
	/// </summary>
	public sealed record DeallocatePrepare : Statement
	{
		/// <summary>
		/// The prepared statement released.
		/// </summary>
		public required StatementReference Statement { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;describe statement&gt;</c>. <c>DESCRIBE [OUTPUT] s USING [SQL] DESCRIPTOR d</c> or
	/// <c>DESCRIBE INPUT s …</c>: the parameters or the result columns of a prepared statement, or the structure
	/// of a cursor, written to a descriptor area.
	/// </summary>
	public sealed record Describe : Statement
	{
		/// <summary>
		/// What is described: a statement's input, or its output or a cursor's structure, with the descriptor
		/// receiving it.
		/// </summary>
		public required DescribeBody Body { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;execute statement&gt;</c>. <c>EXECUTE s [INTO targets] [USING arguments]</c>: a prepared
	/// statement run.
	/// </summary>
	public sealed record Execute : Statement
	{
		/// <summary>
		/// The prepared statement run.
		/// </summary>
		public required StatementReference Statement { get; init; }

		/// <summary>
		/// The targets after <c>INTO</c>, which receive the result row; null where it was not written.
		/// </summary>
		public DynamicArguments? Result { get; init; }

		/// <summary>
		/// The arguments after <c>USING</c>, bound to the statement's parameters; null where it was not written.
		/// </summary>
		public DynamicArguments? Parameters { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;execute immediate statement&gt;</c>. <c>EXECUTE IMMEDIATE sql</c>: a statement given as text,
	/// prepared and run at once.
	/// </summary>
	public sealed record ExecuteImmediate : Statement
	{
		/// <summary>
		/// The text of the statement, as a value.
		/// </summary>
		public required Expression Sql { get; init; }
	}

	/// <summary>
	/// <c>PIPE ROW d</c>: a row handed back from a table function, taken from a descriptor area.
	/// </summary>
	public sealed record PipeRow : Statement
	{
		/// <summary>
		/// The descriptor area that holds the row.
		/// </summary>
		public required DescriptorReference Descriptor { get; init; }
	}

	// BNF: embedded WHENEVER and implementation-defined preparable statement escape hatch
	/// <summary>
	/// An embedded SQL <c>WHENEVER &lt;condition&gt; &lt;action&gt;</c> declaration: what the host program does
	/// when a statement raises the condition, such as <c>WHENEVER NOT FOUND GOTO done</c>.
	/// </summary>
	public sealed record Whenever : Statement
	{
		/// <summary>The condition watched: an exception, a warning, not found, an SQLSTATE or a constraint.</summary>
		public required SqlCondition Condition { get; init; }

		/// <summary>What is done when the condition is raised: <c>CONTINUE</c> or a jump to a label.</summary>
		public required ConditionAction Action { get; init; }
	}

	/// <summary>
	/// A statement a dialect's reader recognised but the common tree has no place for yet. The reader returns
	/// it instead of failing, naming what was left out.
	/// </summary>
	public sealed record Extension : Statement
	{
		/// <summary>The dialect whose reader made the node, such as <c>T-SQL</c>.</summary>
		public required string Dialect { get; init; }

		/// <summary>What the reader says the statement is and why the common tree cannot hold it.</summary>
		public required string Kind { get; init; }

		/// <summary>The nodes the extension carries; empty where the reader keeps none, as T-SQL's does.</summary>
		public SqlList<ISqlNode> Parts { get; init; } = [];
	}
}

/// <summary>
/// BNF: <c>&lt;query expression&gt;</c>, <c>&lt;query expression body&gt;</c>, <c>&lt;query term&gt;</c>,
/// <c>&lt;query primary&gt;</c>. A query, wherever one stands: a SELECT statement's, a subquery's, a
/// derived table's, a common table expression's, a view's, an INSERT's source.
/// </summary>
/// <remarks>
/// <para>
/// <b>Set operations are a list on the query they follow.</b> <c>A UNION B EXCEPT C</c> is
/// <c>A</c> with <c>[UNION B, EXCEPT C]</c> in <see cref="SetOperations"/>, read as a left fold:
/// <c>(A UNION B) EXCEPT C</c>. What binds tighter is an operand with a list of its own and no
/// brackets: <c>A UNION B INTERSECT C</c> is <c>A [UNION B [INTERSECT C]]</c>, and
/// <c>A INTERSECT B UNION C</c> is the flat <c>A [INTERSECT B, UNION C]</c>.
/// </para>
/// <para>
/// <b>Which clauses are whose.</b> A query's <see cref="With"/>, <see cref="OrderBy"/>,
/// <see cref="Limit"/>, <see cref="For"/> and <see cref="Locking"/> apply to its whole result — the
/// query followed by its <see cref="SetOperations"/> — and are written before the first operand
/// (<c>WITH</c>) or after the last (the rest). The specification's members of a
/// <see cref="Select"/> (<c>TOP</c>, the columns, <c>FROM</c>, <c>WHERE</c> and the rest) are the
/// first operand's. So <c>SELECT a FROM t UNION SELECT b FROM u ORDER BY 1</c> sorts the union, and
/// <c>(SELECT a FROM t ORDER BY a) UNION …</c> keeps its own ORDER BY inside a
/// <see cref="Parenthesized"/>. Two consequences: T-SQL's <c>INTO</c> is written in the first operand
/// but receives the whole result's rows; and adding set operations to a query that has an ORDER BY or
/// a limit makes them the compound's — <see cref="Combine"/> brackets the query first to keep them its
/// own.
/// </para>
/// <para>
/// <b>Brackets are a node.</b> A pair written around a query is a <see cref="Parenthesized"/>; the
/// brackets a subquery, a derived table or <c>IN (…)</c> needs are that construct's own and are not
/// one — only a second pair, <c>x IN ((SELECT 1))</c>, is.
/// </para>
/// </remarks>
public abstract partial record Query : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	// The family is closed: Select and Parenthesized are every query there is.
	private protected Query()
	{
	}

	/// <summary>
	/// <c>WITH …</c>, written before the first operand. Of the whole result: the query and its
	/// <see cref="SetOperations"/>.
	/// </summary>
	/// <remarks>
	/// Only on the outermost query of a query expression — a statement's, a subquery's, a common table
	/// expression's, a view's, an INSERT's source. The brackets of a <see cref="Parenthesized"/> and the
	/// operand of a set operation take none, and the writer refuses a tree that has one there.
	/// </remarks>
	public WithClause? With { get; init; }

	/// <summary>
	/// The operations after this query, in text order, each with its operand: a left fold. Empty on a
	/// query with no set operator.
	/// </summary>
	[WalkedLast] public SqlList<SetOperation> SetOperations { get; init; } = [];

	/// <summary>
	/// <c>ORDER BY …</c>. Of the whole result: where <see cref="SetOperations"/> is not empty it sorts
	/// the compound, not the first operand. An operand's own ORDER BY stands inside a
	/// <see cref="Parenthesized"/>.
	/// </summary>
	[WalkedLast] public OrderByClause? OrderBy { get; init; }

	/// <summary>
	/// <c>OFFSET … FETCH …</c>. Of the whole result: where <see cref="SetOperations"/> is not empty it
	/// limits the compound, not the first operand.
	/// </summary>
	[WalkedLast] public Limit? Limit { get; init; }

	/// <summary>
	/// T-SQL: FOR Clause. <c>FOR XML</c>, <c>FOR JSON</c>, <c>FOR BROWSE</c>. Of the whole result, as
	/// <see cref="OrderBy"/> is.
	/// </summary>
	[WalkedLast] public ForClause? For { get; init; }

	/// <summary>
	/// <c>FOR UPDATE [OF …]</c>, <c>FOR READ ONLY</c> and the other locking clauses. Of the whole
	/// result, as <see cref="OrderBy"/> is.
	/// </summary>
	[WalkedLast] public SqlList<LockingClause> Locking { get; init; } = [];

	/// <summary>
	/// BNF: <c>&lt;query specification&gt;</c>, <c>&lt;table value constructor&gt;</c>,
	/// <c>&lt;explicit table&gt;</c>. One <c>SELECT … FROM …</c>, a <c>VALUES</c> list, or
	/// <c>TABLE t</c>, which is <c>SELECT * FROM t</c> with <see cref="Table"/> set.
	/// </summary>
	/// <remarks>
	/// Its members are the first operand's where the query has set operations; the clauses on
	/// <see cref="Query"/> are the whole result's.
	/// </remarks>
	public sealed partial record Select : Query
	{
		/// <summary>T-SQL: TOP. Of this operand alone, not of the whole result.</summary>
		public TopClause? Top { get; init; }

		/// <summary>
		/// <c>DISTINCT</c> or <c>ALL</c> after <c>SELECT</c>; null where neither was written.
		/// </summary>
		public SetQuantifier? Quantifier { get; init; }

		/// <summary>
		/// The select list: expressions with optional aliases, and wildcards (<c>*</c>, <c>t.*</c>); a
		/// wildcard entry stands for several result columns.
		/// </summary>
		public SqlList<SelectColumn> Columns { get; init; } = [];

		/// <summary>
		/// <c>VALUES (1), (2)</c>: the rows, written out. A query with rows has no columns, FROM or the
		/// clauses that go with them.
		/// </summary>
		public SqlList<RowValue> Values { get; init; } = [];

		/// <summary>
		/// <c>TABLE t</c> was written for <c>SELECT * FROM t</c>. Written so while the query is exactly
		/// that; a query changed into anything else is written in full.
		/// </summary>
		[Spelling] public bool Table { get; init; }

		/// <summary>
		/// <c>INTO …</c>. In T-SQL it is written in the first operand and receives the whole result's
		/// rows.
		/// </summary>
		public IntoClause? Into { get; init; }

		/// <summary>
		/// The <c>FROM</c> clause; null where none was written (<c>SELECT 1</c>).
		/// </summary>
		public FromClause? From { get; init; }

		/// <summary>
		/// The <c>WHERE</c> condition; null where none was written.
		/// </summary>
		public Expression? Where { get; init; }

		/// <summary>
		/// The <c>GROUP BY</c> clause; null where none was written.
		/// </summary>
		public GroupByClause? GroupBy { get; init; }

		/// <summary>
		/// The <c>HAVING</c> condition; null where none was written.
		/// </summary>
		public Expression? Having { get; init; }

		/// <summary>
		/// The <c>WINDOW</c> clause, the windows named for this operand's <c>OVER</c>s.
		/// </summary>
		public WindowClause? Window { get; init; }
	}

	/// <summary>
	/// A pair of brackets written around a query: <c>(SELECT a FROM t ORDER BY a) UNION …</c>,
	/// <c>((SELECT 1))</c>. What is inside is <see cref="Query"/>, with its own clauses; this node's
	/// members are outside the brackets.
	/// </summary>
	public sealed record Parenthesized(Query Query) : Query;

	/// <summary>
	/// The query specifications, <c>VALUES</c> lists and explicit tables this query combines, in text
	/// order: itself or what its brackets hold, then the operand of every set operation, through every
	/// level of nesting. Not the queries of subqueries inside them.
	/// </summary>
	public IEnumerable<Select> Operands()
	{
		var pending = new Stack<Query>();

		pending.Push(this);

		while (pending.Count > 0)
		{
			var query = pending.Pop();

			for (var at = query.SetOperations.Count - 1; at >= 0; at--)
				pending.Push(query.SetOperations[at].Query);

			if (query is Parenthesized parenthesized)
				pending.Push(parenthesized.Query);
			else
				yield return (Select)query;
		}
	}

	/// <summary>
	/// This query followed by one more set operation. Where it has an ORDER BY, a limit, a FOR or a
	/// locking clause, which would then be the compound's, it is bracketed first so that they stay its
	/// own.
	/// </summary>
	/// <remarks>
	/// A query with a WITH is refused. Its WITH would be the whole result's, and a table it names would
	/// then capture the same name in the added operand: <c>WITH q AS (TABLE a) TABLE q</c> combined with
	/// <c>UNION ALL TABLE q</c> would read the CTE twice rather than the table <c>q</c> once. Where the
	/// WITH is meant to be shared, combine the query without it and put it on the result:
	/// <c>(left with { With = null }).Combine(operation) with { With = left.With }</c>.
	/// </remarks>
	/// <exception cref="ArgumentException">This query has a WITH.</exception>
	public Query Combine(SetOperation operation)
	{
		if (operation is null)
			throw new ArgumentNullException(nameof(operation));

		if (With is not null)
			throw new ArgumentException(
				"The query has a WITH, which would become the combined result's and could capture a name in the added operand. "
				+ "Combine the query without it and put it on the result explicitly: (left with { With = null }).Combine(operation) with { With = left.With }.",
				nameof(operation));

		if (OrderBy is not null || Limit is not null || For is not null || Locking.Count > 0)
			return new Parenthesized(this) { SetOperations = [operation] };

		return this with { SetOperations = SetOperations.Appended(operation) };
	}
}

/// <summary>BNF: <c>&lt;select list&gt;</c>, <c>&lt;select sublist&gt;</c>, <c>&lt;derived column&gt;</c>, <c>&lt;qualified asterisk&gt;</c>, <c>&lt;all fields reference&gt;</c>.</summary>
public abstract partial record SelectColumn : ISqlNode, ISqlLocatable
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected SelectColumn()
	{
	}

	// The T-SQL reader builds the items of a select list in its actions, as it builds values, and tells each where it
	// was written; the span is everybody's only where the reading was asked for locations (SqlLocation.Told).
	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = SqlLocation.Told(at, length);
	}

	/// <summary>The bare <c>*</c> of a select list: every column of the table sources.</summary>
	public sealed record All : SelectColumn;

	/// <summary>One column of a select list: an expression with an optional alias, <c>a + 1 AS b</c>.</summary>
	/// <param name="Expression">The value selected.</param>
	/// <param name="Alias">The name given to the column; null where none was written.</param>
	/// <param name="AsKeyword"><c>AS</c> was written before the alias.</param>
	public sealed partial record ExpressionItem(Expression Expression, Identifier? Alias = null, bool AsKeyword = false) : SelectColumn;

	/// <summary>All the fields of one table or row value, <c>t.*</c>, standing for several result columns.</summary>
	/// <param name="Qualifier">What stands before <c>.*</c>: a table's name or a row-valued expression.</param>
	/// <param name="RenamedFields">The names given to the fields in <c>t.* AS (a, b)</c>; null where no list was written.</param>
	public sealed record QualifiedAll(Expression Qualifier, SqlList<Identifier>? RenamedFields = null) : SelectColumn;
}

/// <summary>The quantifier of a set function, a select list or a grouping: <c>ALL</c> or <c>DISTINCT</c>.</summary>
public enum SetQuantifier
{
	/// <summary><c>ALL</c>: duplicates are kept.</summary>
	All,

	/// <summary><c>DISTINCT</c>: duplicates are removed.</summary>
	Distinct,
}

/// <summary>
/// The six set operations. <c>ALL</c> changes the result — a bag rather than a set — so each kind with
/// it is an operation of its own; a <c>DISTINCT</c> written out changes nothing, and is
/// <see cref="SetOperation.Distinct"/>.
/// </summary>
public enum SetOperator
{
	/// <summary><c>UNION</c>: the rows of either operand, without duplicates.</summary>
	Union,

	/// <summary><c>UNION ALL</c>: the rows of both operands, duplicates kept.</summary>
	UnionAll,

	/// <summary><c>INTERSECT</c>: the rows in both operands, without duplicates.</summary>
	Intersect,

	/// <summary><c>INTERSECT ALL</c>: the rows in both operands, as often as the fewer of the two has them.</summary>
	IntersectAll,

	/// <summary><c>EXCEPT</c>: the rows of the left operand not in the right, without duplicates.</summary>
	Except,

	/// <summary><c>EXCEPT ALL</c>: the rows of the left operand, less as many as the right has of each.</summary>
	ExceptAll,
}

/// <summary>
/// BNF: the <c>UNION</c>, <c>EXCEPT</c> and <c>INTERSECT</c> of <c>&lt;query expression body&gt;</c> and
/// <c>&lt;query term&gt;</c>. One operation of a query's <see cref="Query.SetOperations"/> and the
/// operand after it.
/// </summary>
public sealed record SetOperation(SetOperator Operator, Query Query) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>UNION DISTINCT</c> was written for <c>UNION</c>. Only with a kind without <c>ALL</c>.</summary>
	[Spelling] public bool Distinct { get; init; }

	/// <summary>
	/// <c>CORRESPONDING [BY (…)]</c>: the operands are matched by column name rather than position; null
	/// where it was not written.
	/// </summary>
	public CorrespondingClause? Corresponding { get; init; }
}

/// <summary>
/// <c>WITH [RECURSIVE] …</c>: the common table expressions a query can name. See <see cref="Query.With"/>.
/// </summary>
/// <param name="Recursive"><c>RECURSIVE</c> was written.</param>
/// <param name="Items">The common table expressions, in text order.</param>
public sealed partial record WithClause(bool Recursive, SqlList<CommonTableExpression> Items) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// One common table expression of a <c>WITH</c>: <c>name (a, b) AS (query)</c>, with the standard's
/// <c>SEARCH</c> and <c>CYCLE</c> clauses after it.
/// </summary>
/// <param name="Name">The name the query is given.</param>
/// <param name="Columns">The column names listed after the name; empty where none were listed.</param>
/// <param name="Query">The query in the brackets after <c>AS</c>.</param>
/// <param name="Search">The <c>SEARCH</c> clause; null where none was written.</param>
/// <param name="Cycle">The <c>CYCLE</c> clause; null where none was written.</param>
public sealed record CommonTableExpression(Identifier Name, SqlList<Identifier> Columns, Query Query, SearchClause? Search = null, CycleClause? Cycle = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// BNF: <c>&lt;search clause&gt;</c>, <c>&lt;recursive search order&gt;</c>, <c>&lt;sequence column&gt;</c>.
/// <c>SEARCH DEPTH FIRST BY a, b SET seq</c> of a recursive common table expression: numbers its rows in the
/// order a traversal visits them.
/// </summary>
/// <param name="Order">Whether the traversal goes depth first or breadth first.</param>
/// <param name="Columns">The columns the rows are ordered by within a level (the <c>BY</c> list).</param>
/// <param name="SequenceColumn">The column that receives the visit number (the <c>SET</c> name).</param>
public sealed record SearchClause(SearchOrder Order, SqlList<Identifier> Columns, Identifier SequenceColumn) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The order a <see cref="SearchClause"/> numbers the rows of a recursive query in.</summary>
public enum SearchOrder
{
	/// <summary><c>DEPTH FIRST</c>: a row is followed by its descendants before its siblings.</summary>
	DepthFirst,

	/// <summary><c>BREADTH FIRST</c>: all the rows of one level before the next.</summary>
	BreadthFirst,
}

/// <summary>
/// BNF: <c>&lt;cycle clause&gt;</c>, <c>&lt;cycle column list&gt;</c>, <c>&lt;cycle mark column&gt;</c>,
/// <c>&lt;cycle mark option&gt;</c>, <c>&lt;path column&gt;</c>. <c>CYCLE a SET m TO 1 DEFAULT 0 USING p</c> of a
/// recursive common table expression: marks a row that revisits a row already on its path.
/// </summary>
/// <param name="Columns">The columns whose values identify a row on the path.</param>
/// <param name="MarkColumn">The column that receives the mark (the <c>SET</c> name).</param>
/// <param name="MarkValue">The value marking a cyclic row (after <c>TO</c>); null where the mark values were left out.</param>
/// <param name="NonCycleMarkValue">The value of every other row (after <c>DEFAULT</c>); null where the mark values were left out.</param>
/// <param name="PathColumn">The column that receives the path (the <c>USING</c> name).</param>
public sealed record CycleClause(SqlList<Identifier> Columns, Identifier MarkColumn, Expression? MarkValue, Expression? NonCycleMarkValue, Identifier PathColumn) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The <c>FROM</c> clause of a query specification.</summary>
/// <param name="Sources">The table sources, comma separated, in text order.</param>
public sealed record FromClause(SqlList<TableSource> Sources) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The standard's <c>INTO :a, :b</c> of a query specification: the targets its row is assigned to.</summary>
/// <param name="Targets">The targets, in text order.</param>
public sealed record IntoClause(SqlList<Expression> Targets) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The <c>GROUP BY</c> clause of a query specification.</summary>
/// <param name="Quantifier"><c>ALL</c> or <c>DISTINCT</c> written after <c>GROUP BY</c>; null where neither was.</param>
/// <param name="Items">The grouping elements: expressions, <c>ROLLUP</c>, <c>CUBE</c>, grouping sets, and the rest.</param>
public sealed partial record GroupByClause(SetQuantifier? Quantifier, SqlList<GroupingElement> Items) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The <c>WINDOW</c> clause of a query specification: windows named for the <c>OVER</c>s that use them.</summary>
/// <param name="Windows">The named window definitions, in text order.</param>
public sealed record WindowClause(SqlList<WindowDefinition> Windows) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>An <c>ORDER BY</c> clause.</summary>
/// <param name="Items">The sort items, in text order.</param>
public sealed record OrderByClause(SqlList<SortItem> Items) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// BNF: <c>&lt;result offset clause&gt;</c>, <c>&lt;fetch first clause&gt;</c>. How many rows a query
/// skips and how many it returns, however the dialect spells it.
/// </summary>
/// <remarks>
/// <c>FETCH</c> is written where <see cref="Count"/> or <see cref="FetchRowWord"/> is set, <c>OFFSET</c>
/// where <see cref="Offset"/> is.
/// </remarks>
public sealed record Limit : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>The rows skipped, <c>OFFSET m ROWS</c>; null where no OFFSET was written.</summary>
	public Expression? Offset { get; init; }

	/// <summary>The rows returned, <c>FETCH FIRST n ROWS ONLY</c>; null where none was written, as in <c>FETCH FIRST ROW ONLY</c>.</summary>
	public Expression? Count { get; init; }

	/// <summary><c>FETCH FIRST n PERCENT ROWS ONLY</c>.</summary>
	public bool Percent { get; init; }

	/// <summary><c>WITH TIES</c> rather than <c>ONLY</c>.</summary>
	public bool WithTies { get; init; }

	/// <summary><c>OFFSET m ROW</c> or <c>ROWS</c>; null where no OFFSET was written.</summary>
	[Spelling] public RowWord? OffsetRowWord { get; init; }

	/// <summary><c>FETCH … ROW</c> or <c>ROWS</c>; null where no FETCH was written.</summary>
	[Spelling] public RowWord? FetchRowWord { get; init; }

	/// <summary><c>FETCH FIRST</c> or <c>FETCH NEXT</c>.</summary>
	[Spelling] public FetchPosition Position { get; init; }
}
/// <summary>Which of the two interchangeable words follows a count in <c>OFFSET</c> and <c>FETCH</c>.</summary>
public enum RowWord
{
	/// <summary><c>ROW</c>.</summary>
	Row,

	/// <summary><c>ROWS</c>.</summary>
	Rows,
}

/// <summary>Which of the two interchangeable words follows <c>FETCH</c>.</summary>
public enum FetchPosition
{
	/// <summary><c>FIRST</c>.</summary>
	First,

	/// <summary><c>NEXT</c>.</summary>
	Next,
}

/// <summary>
/// BNF: <c>&lt;updatability clause&gt;</c> of a <c>&lt;cursor specification&gt;</c>, and the row locks
/// other dialects write in its place. One of a query's <see cref="Query.Locking"/>: <c>FOR UPDATE OF a</c>.
/// </summary>
/// <param name="Mode">Which lock or access is asked for.</param>
/// <param name="Of">The names after <c>OF</c>; empty where there is no <c>OF</c>.</param>
public sealed record LockingClause(LockingMode Mode, SqlList<Identifier> Of) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>What a <see cref="LockingClause"/> asks for.</summary>
public enum LockingMode
{
	/// <summary><c>FOR READ ONLY</c>: the cursor is not updatable.</summary>
	ReadOnly,

	/// <summary><c>FOR UPDATE</c>: the rows are locked for update.</summary>
	Update,

	/// <summary><c>FOR NO KEY UPDATE</c>: locked for update, without blocking the shares of their keys.</summary>
	NoKeyUpdate,

	/// <summary><c>FOR SHARE</c>: the rows are locked against update by others.</summary>
	Share,

	/// <summary><c>FOR KEY SHARE</c>: locked against changes to their keys only.</summary>
	KeyShare,
}
/// <summary>
/// BNF: <c>&lt;corresponding spec&gt;</c>. <c>CORRESPONDING</c> or <c>CORRESPONDING BY (a, b)</c> after a set
/// operator: the operands are matched by column name rather than by position.
/// </summary>
/// <param name="By"><c>BY</c> and its column list were written.</param>
/// <param name="Columns">The columns of the <c>BY</c> list; empty where there is none.</param>
public sealed record CorrespondingClause(bool By, SqlList<Identifier> Columns) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>One item of an <c>ORDER BY</c>: <c>a DESC NULLS LAST</c>.</summary>
/// <param name="Key">The value sorted by.</param>
/// <param name="Direction"><c>ASC</c> or <c>DESC</c>; null where neither was written.</param>
/// <param name="NullOrdering"><c>NULLS FIRST</c> or <c>NULLS LAST</c>; null where neither was written.</param>
public sealed record SortItem(Expression Key, SortDirection? Direction = null, NullOrdering? NullOrdering = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The direction of a <see cref="SortItem"/>.</summary>
public enum SortDirection
{
	/// <summary><c>ASC</c>: smallest first.</summary>
	Asc,

	/// <summary><c>DESC</c>: largest first.</summary>
	Desc,
}

/// <summary>Where a <see cref="SortItem"/> puts null.</summary>
public enum NullOrdering
{
	/// <summary><c>NULLS FIRST</c>.</summary>
	First,

	/// <summary><c>NULLS LAST</c>.</summary>
	Last,
}

/// <summary>
/// A table source: what a <c>FROM</c> lists and a statement changes. One of the records nested in this
/// one — a table by name (<see cref="Named"/>), a derived table (<see cref="Subquery"/>), a
/// <see cref="Join"/> of two others, a table-valued function, and the rest.
/// </summary>
/// <remarks>
/// BNF: <c>&lt;table reference&gt;</c>, <c>&lt;table factor&gt;</c>, <c>&lt;table primary&gt;</c>,
/// <c>&lt;joined table&gt;</c>, and the derived, collection, function and JSON tables.
/// </remarks>
public abstract partial record TableSource : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected TableSource()
	{
	}

	/// <summary>
	/// BNF: <c>&lt;table factor&gt;</c>'s <c>&lt;sample clause&gt;</c>, <c>TABLESAMPLE …</c>, which may follow
	/// any table primary.
	/// </summary>
	public SampleClause? Sample { get; init; }

	/// <summary>
	/// A table or view by its name: <c>dbo.t AS x</c>.
	/// </summary>
	/// <param name="Name">The name, as written.</param>
	public sealed partial record Named(QualifiedName Name) : TableSource
	{
		/// <summary>
		/// <c>FOR SYSTEM_TIME …</c>: the rows of a system-versioned table at a time or over a span.
		/// </summary>
		public SystemTimeSpecification? SystemTime { get; init; }

		/// <summary>
		/// The name the table is given after it.
		/// </summary>
		public Alias? Alias { get; init; }

		/// <summary>
		/// The standard's <c>ONLY (t)</c>: the table without its subtables.
		/// </summary>
		public bool Only { get; init; }
	}

	/// <summary>
	/// A derived table: a query in brackets, <c>(SELECT …) AS d</c>.
	/// </summary>
	/// <param name="Query">The query inside the brackets.</param>
	public sealed partial record Subquery(Query Query) : TableSource
	{
		/// <summary>
		/// <c>LATERAL</c> was written: the query may name the table sources before it.
		/// </summary>
		public bool Lateral { get; init; }

		/// <summary>
		/// The name the derived table is given, and its columns' names where they are listed.
		/// </summary>
		public Alias? Alias { get; init; }
	}

	/// <summary>
	/// Two table sources joined: <c>t JOIN u ON …</c>, <c>t CROSS JOIN u</c>, <c>t CROSS APPLY f(t.a)</c>.
	/// </summary>
	public sealed partial record Join : TableSource
	{
		/// <summary>
		/// The table source before the join's words.
		/// </summary>
		public required TableSource Left { get; init; }

		/// <summary>
		/// The table source after them.
		/// </summary>
		public required TableSource Right { get; init; }

		/// <summary>
		/// BNF: <c>&lt;join type&gt;</c>. Null where a bare <c>JOIN</c> was written, which is not the same
		/// text as <c>INNER JOIN</c>.
		/// </summary>
		public JoinKind? Kind { get; init; }

		/// <summary>
		/// <c>NATURAL</c> was written.
		/// </summary>
		public bool Natural { get; init; }

		/// <summary>
		/// <c>OUTER</c> was written after <c>LEFT</c>, <c>RIGHT</c> or <c>FULL</c>.
		/// </summary>
		public bool OuterKeyword { get; init; }

		/// <summary>
		/// BNF: <c>&lt;partitioned join table&gt;</c>'s <c>&lt;join column list&gt;</c> after the left source,
		/// <c>t PARTITION BY (a) LEFT JOIN u</c>; null where none was written.
		/// </summary>
		public SqlList<Expression>? LeftPartition { get; init; }

		/// <summary>
		/// The same <c>PARTITION BY (…)</c> after the right source, <c>t LEFT JOIN u PARTITION BY (a)</c>; null
		/// where none was written.
		/// </summary>
		public SqlList<Expression>? RightPartition { get; init; }

		/// <summary>
		/// The <c>ON</c> condition or the <c>USING</c> columns; null for a join that takes neither.
		/// </summary>
		public JoinSpecification? Specification { get; init; }

		/// <summary>
		/// T-SQL: Join hints (Transact-SQL). How the engine is told to perform the join, the word inside the join
		/// type: <c>INNER LOOP JOIN</c>. Null where none was written.
		/// </summary>
		public JoinHint? Hint { get; init; }
	}

	/// <summary>A table made of arrays or multisets: <c>UNNEST (a, b) WITH ORDINALITY AS t</c>.</summary>
	/// <param name="Expressions">The collections unnested, in text order.</param>
	/// <param name="WithOrdinality"><c>WITH ORDINALITY</c> was written: each row also carries its position.</param>
	/// <param name="Alias">The name given to the table and its columns; null where none was written.</param>
	public sealed record Unnest(SqlList<Expression> Expressions, bool WithOrdinality, Alias? Alias) : TableSource;

	/// <summary>A call of a table-valued function used as a table: <c>f(a) AS t</c>.</summary>
	/// <param name="Invocation">The call.</param>
	/// <param name="Alias">The name given to the result and its columns; null where none was written.</param>
	public sealed partial record Function(Expression.Invocation Invocation, Alias? Alias = null) : TableSource;

	/// <summary>
	/// BNF: <c>&lt;table function derived table&gt;</c>, <c>TABLE (f (a))</c>: a collection-valued expression read as a table.
	/// </summary>
	/// <param name="Value">The expression inside the brackets.</param>
	/// <param name="Alias">The name given to the table; null where none was written.</param>
	public sealed record TableFunction(Expression Value, Alias? Alias = null) : TableSource;

	/// <summary>
	/// BNF: <c>&lt;row pattern recognition clause&gt;</c> after a table primary, <c>t MATCH_RECOGNIZE (…) AS m</c>:
	/// the rows of a table matched against a pattern.
	/// </summary>
	/// <param name="Source">The table primary the clause follows.</param>
	/// <param name="Clause">The <c>MATCH_RECOGNIZE</c> clause.</param>
	/// <param name="Alias">The name given to the result; null where none was written.</param>
	public sealed record RowPatternRecognition(TableSource Source, RowPatternClause Clause, Alias? Alias = null) : TableSource;
	/// <summary>A <c>JSON_TABLE (…)</c>: a JSON document read as a table.</summary>
	/// <param name="Definition">The document, path, columns and plan inside the brackets.</param>
	/// <param name="Alias">The name given to the table; null where none was written.</param>
	public sealed record JsonTable(JsonTableDefinition Definition, Alias? Alias = null) : TableSource;

	/// <summary>
	/// A data change statement used as a table: <c>FINAL TABLE (INSERT …)</c>, whose rows are those the statement
	/// affected. T-SQL: INSERT (Transact-SQL). Its form is <c>(MERGE … OUTPUT …) AS c</c>, where the statement's
	/// <c>OUTPUT</c> is the rows and <paramref name="Option"/> is null.
	/// </summary>
	/// <param name="Option">Which version of the rows is read: <c>FINAL</c>, <c>NEW</c> or <c>OLD</c>; null in T-SQL's form.</param>
	/// <param name="Change">The <c>INSERT</c>, <c>UPDATE</c>, <c>DELETE</c> or <c>MERGE</c> in the brackets.</param>
	/// <param name="Alias">The name given to the rows; null where none was written.</param>
	public sealed record DataChange(ResultOption? Option, Statement Change, Alias? Alias = null) : TableSource;
	/// <summary>A pair of brackets around a table source: <c>(t JOIN u ON …)</c>.</summary>
	/// <param name="Source">What the brackets hold.</param>
	public sealed record Parenthesized(TableSource Source) : TableSource;

	/// <summary>
	/// A table source a dialect's reader recognised but the common tree has no place for yet. The reader
	/// returns it instead of failing, naming what was left out.
	/// </summary>
	/// <param name="Dialect">The dialect whose reader made the node, such as <c>T-SQL</c>.</param>
	/// <param name="Kind">What the reader says the source is and why the common tree cannot hold it.</param>
	/// <param name="Parts">The nodes the extension carries; empty where the reader keeps none, as T-SQL's does.</param>
	public sealed record Extension(string Dialect, string Kind, SqlList<ISqlNode> Parts) : TableSource;
}

/// <summary>
/// The kind of a <see cref="TableSource.Join"/>.
/// </summary>
/// <remarks>
/// T-SQL: FROM plus JOIN, APPLY, PIVOT, UNPIVOT (Transact-SQL). <c>CrossApply</c> and <c>OuterApply</c> join a
/// table source to one computed from each of its rows, which no standard join does.
/// SQL-92: §7.5 <c>&lt;joined table&gt;</c>, <c>Union</c>, <c>t UNION JOIN u</c>; removed in SQL:2003.
/// </remarks>
public enum JoinKind
{
	/// <summary><c>CROSS JOIN</c>: every pairing of the two sources' rows.</summary>
	Cross,

	/// <summary><c>INNER JOIN</c>: the pairs that meet the condition.</summary>
	Inner,

	/// <summary><c>LEFT JOIN</c>: the inner pairs, and each left row with no pair, completed with nulls.</summary>
	Left,

	/// <summary><c>RIGHT JOIN</c>: the inner pairs, and each right row with no pair, completed with nulls.</summary>
	Right,

	/// <summary><c>FULL JOIN</c>: the inner pairs, and every row of either side with no pair, completed with nulls.</summary>
	Full,

	/// <summary>T-SQL: <c>CROSS APPLY</c>: the right source is computed from each left row; a left row it gives nothing for is dropped.</summary>
	CrossApply,

	/// <summary>T-SQL: <c>OUTER APPLY</c>: as <see cref="CrossApply"/>, but a left row it gives nothing for is kept, completed with nulls.</summary>
	OuterApply,

	/// <summary>SQL-92: <c>UNION JOIN</c>: the rows of both sources, each completed with nulls for the other's columns.</summary>
	Union,
}

/// <summary>What a <see cref="TableSource.Join"/> joins on: <c>ON …</c> or <c>USING (…)</c>.</summary>
public abstract record JoinSpecification : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JoinSpecification()
	{
	}

	/// <summary><c>ON condition</c>.</summary>
	/// <param name="Condition">The condition the joined rows meet.</param>
	public sealed record On(Expression Condition) : JoinSpecification;

	/// <summary><c>USING (a, b) [AS c]</c>: the columns the two sources are joined on by name.</summary>
	/// <param name="Columns">The columns listed.</param>
	/// <param name="Correlation">The name after <c>AS</c> that the joined columns go by; null where none was written.</param>
	public sealed record Using(SqlList<Identifier> Columns, Identifier? Correlation = null) : JoinSpecification;
}
/// <summary>The name a table source is given: <c>[AS] t [(a, b)]</c>.</summary>
/// <param name="Name">The table's name.</param>
/// <param name="Columns">The names given to its columns; null where no list was written.</param>
/// <param name="AsKeyword"><c>AS</c> was written before the name.</param>
public sealed record Alias(Identifier Name, SqlList<Identifier>? Columns = null, bool AsKeyword = false) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// <c>TABLESAMPLE BERNOULLI (10) REPEATABLE (7)</c>: a random sample of a table source's rows.
/// </summary>
/// <param name="Method">The sampling method.</param>
/// <param name="Percentage">The size of the sample; a percentage unless <c>Unit</c> says otherwise.</param>
/// <param name="Repeat">The seed in <c>REPEATABLE (…)</c>; null where none was written.</param>
public sealed partial record SampleClause(SampleMethod Method, Expression Percentage, Expression? Repeat = null) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How a <see cref="SampleClause"/> picks rows.</summary>
public enum SampleMethod
{
	/// <summary><c>BERNOULLI</c>: each row is chosen on its own.</summary>
	Bernoulli,

	/// <summary><c>SYSTEM</c>: whole blocks of rows are chosen.</summary>
	System,
}

/// <summary>Which version of the affected rows a <see cref="TableSource.DataChange"/> reads.</summary>
public enum ResultOption
{
	/// <summary><c>FINAL TABLE</c>: the rows as the statement, and its triggers, leave them.</summary>
	Final,

	/// <summary><c>NEW TABLE</c>: the rows as the statement made them, before triggers fire.</summary>
	New,

	/// <summary><c>OLD TABLE</c>: the rows as they were before the statement.</summary>
	Old,
}

/// <summary>
/// A value or a condition: one of the records nested in this one — a column or variable by name
/// (<see cref="Reference"/>), a literal, an operator over others (<see cref="Unary"/>,
/// <see cref="Binary"/>), a function call, a subquery, a predicate. What <c>ParseExpression</c> and
/// <c>ParseSearchCondition</c> return.
/// </summary>
/// <remarks>
/// BNF: <c>&lt;value expression&gt;</c> and its numeric, string, datetime, interval, boolean, predicate and
/// value-primary subgrammars. A search condition is an expression too: <c>WHERE</c> holds one.
/// </remarks>
public abstract partial record Expression : ISqlNode, ISqlLocatable
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected Expression()
	{
	}

	// The T-SQL reader builds values of this tree in its actions and tells each where it was written, as it told the
	// records it built before; the span is everybody's only where the reading was asked for locations (SqlLocation.Told).
	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = SqlLocation.Told(at, length);
	}

	/// <summary>Where a reading said the value was written, whether the reading published locations or not.</summary>
	internal SqlSpan Read => _location.Read;

	/// <summary>
	/// A name standing for a value: a column, <c>a</c> or <c>t.a</c>.
	/// </summary>
	/// <param name="Name">The name, as written, with every part.</param>
	public sealed record Reference(QualifiedName Name) : Expression;

	/// <summary>
	/// BNF: <c>&lt;host parameter specification&gt;</c>, <c>&lt;embedded variable specification&gt;</c>,
	/// <c>&lt;dynamic parameter specification&gt;</c>. A value supplied from outside the statement: <c>:a</c> or <c>?</c>.
	/// </summary>
	/// <param name="Kind">Which kind of parameter it is.</param>
	/// <param name="Name">The parameter's name, without the colon; null for a dynamic <c>?</c>.</param>
	public sealed record Parameter(ParameterKind Kind, Identifier? Name = null) : Expression
	{
		/// <summary>
		/// BNF: <c>&lt;indicator parameter&gt;</c>, <c>&lt;indicator variable&gt;</c>. The variable after the
		/// parameter that tells whether it is null, <c>:a INDICATOR :i</c> or <c>:a :i</c>; null where none was written.
		/// </summary>
		public Identifier? Indicator { get; init; }

		/// <summary>The word <c>INDICATOR</c> was written before the indicator variable.</summary>
		public bool IndicatorKeyword { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;general value specification&gt;</c>'s key words, <c>&lt;datetime value function&gt;</c>. A value
	/// the session or the clock supplies, written as a word: <c>CURRENT_USER</c>, <c>CURRENT_TIMESTAMP (3)</c>.
	/// </summary>
	/// <param name="Kind">Which value it is.</param>
	/// <param name="TypeName">The type named by <c>CURRENT_TRANSFORM_GROUP_FOR_TYPE</c>; null for every other value.</param>
	/// <param name="Precision">The fractional-seconds precision in <c>CURRENT_TIME (3)</c>; null where none was written.</param>
	public sealed record Current(CurrentValue Kind, QualifiedName? TypeName = null, Expression? Precision = null) : Expression;

	/// <summary>
	/// An operator before its operand: <c>-x</c>, <c>NOT p</c>.
	/// </summary>
	/// <param name="Operator">The operator.</param>
	/// <param name="Operand">What it applies to.</param>
	public sealed record Unary(UnaryOperator Operator, Expression Operand) : Expression;

	/// <summary>
	/// An operator between two operands: <c>a + b</c>, <c>a || b</c>, <c>p AND q</c>. The grouping is the
	/// tree's: <c>a - b - c</c> is a <c>Binary</c> whose left operand is <c>a - b</c>.
	/// </summary>
	/// <param name="Left">The operand before the operator.</param>
	/// <param name="Operator">The operator.</param>
	/// <param name="Right">The operand after it.</param>
	public sealed record Binary(Expression Left, BinaryOperator Operator, Expression Right) : Expression;

	/// <summary>
	/// BNF: <c>&lt;multiset value expression&gt;</c>, <c>&lt;multiset term&gt;</c>: <c>a MULTISET UNION ALL b</c>.
	/// </summary>
	/// <param name="Left">The operand before the operator.</param>
	/// <param name="Operator">The operator after <c>MULTISET</c>.</param>
	/// <param name="Quantifier"><c>ALL</c> or <c>DISTINCT</c>; null where neither was written.</param>
	/// <param name="Right">The operand after it.</param>
	public sealed record MultisetOperation(Expression Left, MultisetOperator Operator, SetQuantifier? Quantifier, Expression Right) : Expression;

	/// <summary>A row value constructor: <c>(1, 'a')</c> or <c>ROW (1, 'a')</c>.</summary>
	/// <param name="Items">The fields, in text order.</param>
	/// <param name="RowKeyword">The word <c>ROW</c> was written before the brackets.</param>
	public sealed record Row(SqlList<Expression> Items, bool RowKeyword = false) : Expression;

	/// <summary>A pair of brackets around an expression: <c>(a + b)</c>. It is a node so that the text is kept.</summary>
	/// <param name="Value">What the brackets hold.</param>
	public sealed record Parenthesized(Expression Value) : Expression;

	/// <summary>A query used as a value, in its own brackets: <c>(SELECT MAX(a) FROM t)</c>.</summary>
	/// <param name="Query">The query inside the brackets.</param>
	public sealed record Subquery(Query Query) : Expression;

	/// <summary>
	/// BNF: <c>&lt;case expression&gt;</c>: <c>CASE [operand] WHEN … THEN … [ELSE …] END</c>. In a simple CASE a
	/// when operand may be a predicate without its left side, <c>WHEN &lt; 5</c>, whose missing operand is
	/// written as <see cref="CaseOperand"/>.
	/// </summary>
	/// <param name="Operand">The value every <c>WHEN</c> is compared with; null in a searched CASE, whose <c>WHEN</c>s are conditions.</param>
	/// <param name="Whens">The <c>WHEN … THEN …</c> branches, in text order.</param>
	/// <param name="Else">The value after <c>ELSE</c>; null where there is no <c>ELSE</c>.</param>
	public sealed record Case(Expression? Operand, SqlList<CaseWhen> Whens, Expression? Else) : Expression;

	/// <summary>
	/// BNF: <c>&lt;case operand&gt;</c> standing where a <c>&lt;when operand&gt;</c> leaves it out. Nothing is
	/// written for it.
	/// </summary>
	public sealed record CaseOperand : Expression;

	/// <summary>A conversion to a type: <c>CAST (a AS t [FORMAT 'f'])</c>.</summary>
	/// <param name="Value">The value converted.</param>
	/// <param name="Target">The type converted to.</param>
	/// <param name="Format">The text after <c>FORMAT</c>, as written; null where there is no <c>FORMAT</c>.</param>
	public sealed partial record Cast(Expression Value, DataType Target, string? Format = null) : Expression;

	/// <summary>
	/// BNF: <c>&lt;routine invocation&gt;</c>, <c>&lt;aggregate function&gt;</c>, <c>&lt;window function&gt;</c>,
	/// <c>&lt;row pattern navigation operation&gt;</c>, and the functions whose arguments are a plain comma list:
	/// a call, <c>f (a, b)</c>, with whatever clauses follow its brackets.
	/// </summary>
	/// <param name="Name">The function's name, as written.</param>
	/// <param name="Arguments">The arguments, in text order.</param>
	public sealed partial record Invocation(QualifiedName Name, SqlList<Argument> Arguments) : Expression
	{
		/// <summary>
		/// BNF: <c>&lt;set quantifier&gt;</c> in a <c>&lt;general set function&gt;</c>, <c>COUNT (DISTINCT a)</c>; null
		/// where neither <c>ALL</c> nor <c>DISTINCT</c> was written.
		/// </summary>
		public SetQuantifier? Quantifier { get; init; }

		/// <summary>
		/// BNF: <c>&lt;sort specification list&gt;</c> inside the brackets, <c>ARRAY_AGG (a ORDER BY b)</c>; null
		/// where there is none.
		/// </summary>
		public OrderByClause? OrderBy { get; init; }

		/// <summary>BNF: <c>&lt;listagg overflow clause&gt;</c>, <c>ON OVERFLOW …</c> inside the brackets; null where there is none.</summary>
		public ListaggOverflow? Overflow { get; init; }

		/// <summary>BNF: <c>&lt;from first or last&gt;</c>, <c>FROM FIRST</c> or <c>FROM LAST</c> after the brackets; null where neither was written.</summary>
		public FromFirstOrLast? From { get; init; }

		/// <summary>BNF: <c>&lt;null treatment&gt;</c>, <c>RESPECT NULLS</c> or <c>IGNORE NULLS</c> after the brackets; null where neither was written.</summary>
		public NullTreatment? Nulls { get; init; }

		/// <summary>BNF: <c>&lt;row pattern navigation operation&gt;</c>'s <c>RUNNING</c> or <c>FINAL</c> before the name; null where neither was written.</summary>
		public RowPatternSemantics? Semantics { get; init; }

		/// <summary>The <c>FILTER (WHERE …)</c> of an aggregate; null where there is none.</summary>
		public FilterClause? Filter { get; init; }

		/// <summary>The <c>OVER</c> of a window function: a window's name or its specification; null where there is none.</summary>
		public WindowReference? Over { get; init; }

		/// <summary>The <c>WITHIN GROUP (ORDER BY …)</c> of an ordered-set aggregate; null where there is none.</summary>
		public WithinGroupClause? WithinGroup { get; init; }

		/// <summary>
		/// BNF: <c>&lt;co-partition clause&gt;</c>: <c>COPARTITION (a, b), (c, d)</c>, each group a list of names; null
		/// where there is none.
		/// </summary>
		public SqlList<SqlList<QualifiedName>>? Copartition { get; init; }

		/// <summary>
		/// A row pattern measure used as a window function, <c>m OVER w</c>, is written without brackets: the name
		/// stands with no argument list.
		/// </summary>
		public bool WithoutParentheses { get; init; }
	}

	/// <summary>BNF: <c>COUNT (*)</c>'s <c>&lt;asterisk&gt;</c>, as the one argument.</summary>
	public sealed record Asterisk : Expression;

	/// <summary>
	/// BNF: <c>&lt;method invocation&gt;</c>, <c>&lt;attribute or method reference&gt;</c>,
	/// <c>&lt;static method invocation&gt;</c> (<c>T::m</c>), <c>&lt;dereference operation&gt;</c>: <c>(a).m</c>,
	/// <c>(a).m (x)</c>, <c>a -&gt; m</c>, <c>T::m (x)</c>.
	/// </summary>
	/// <param name="Target">The value or type whose member is named.</param>
	/// <param name="Kind">The operator between the target and the name.</param>
	/// <param name="Name">The member's name.</param>
	/// <param name="Arguments">The arguments of a method call; null for an attribute, which has no brackets.</param>
	public sealed record Member(Expression Target, MemberAccessKind Kind, Identifier Name, SqlList<Argument>? Arguments = null) : Expression;

	/// <summary>
	/// BNF: <c>&lt;generalized expression&gt;</c>, the <c>(a AS t)</c> of <c>(a AS t).m ()</c>: a value taken as
	/// an instance of a user-defined type, so that its methods can be called.
	/// </summary>
	/// <param name="Value">The value.</param>
	/// <param name="Type">The type it is taken as.</param>
	public sealed record Generalized(Expression Value, DataType Type) : Expression;

	/// <summary>BNF: <c>&lt;subtype treatment&gt;</c>, <c>TREAT (a AS t)</c>: a value taken as an instance of a subtype.</summary>
	/// <param name="Value">The value.</param>
	/// <param name="Target">The subtype it is taken as.</param>
	public sealed record Treat(Expression Value, DataType Target) : Expression;

	/// <summary>BNF: <c>&lt;new specification&gt;</c>, <c>NEW t (a, b)</c>: a new instance of a user-defined type.</summary>
	/// <param name="TypeName">The type's name.</param>
	/// <param name="Arguments">The constructor's arguments, in text order.</param>
	public sealed record New(QualifiedName TypeName, SqlList<Argument> Arguments) : Expression;

	/// <summary>BNF: <c>&lt;reference resolution&gt;</c>, <c>DEREF (r)</c>: the value a reference refers to.</summary>
	/// <param name="Value">The reference.</param>
	public sealed record Dereference(Expression Value) : Expression;

	/// <summary>An array constructor from a list of values: <c>ARRAY [1, 2, 3]</c>.</summary>
	/// <param name="Items">The elements, in text order.</param>
	/// <param name="Trigraphs">The brackets were written as the trigraphs <c>??(</c> and <c>??)</c>.</param>
	public sealed record Array(SqlList<Expression> Items, bool Trigraphs = false) : Expression;

	/// <summary>A multiset constructor from a list of values: <c>MULTISET [1, 2, 3]</c>.</summary>
	/// <param name="Items">The elements, in text order.</param>
	/// <param name="Trigraphs">The brackets were written as the trigraphs <c>??(</c> and <c>??)</c>.</param>
	public sealed record Multiset(SqlList<Expression> Items, bool Trigraphs = false) : Expression;

	/// <summary>A collection built from a query: <c>ARRAY (SELECT …)</c>, <c>MULTISET (SELECT …)</c>, <c>TABLE (SELECT …)</c>.</summary>
	/// <param name="Kind">The collection built.</param>
	/// <param name="Query">The query inside the brackets.</param>
	public sealed record CollectionQuery(CollectionKind Kind, Query Query) : Expression;

	/// <summary>
	/// BNF: <c>&lt;array element reference&gt;</c>, and a JSON simplified accessor's <c>[1 TO 3]</c>: an element of a
	/// collection by its position, <c>a[i]</c>.
	/// </summary>
	/// <param name="Collection">The array or other collection value that is indexed.</param>
	/// <param name="Index">The position, or the first position of a <c>TO</c> range.</param>
	/// <param name="To">The last position after <c>TO</c>; null for a single element.</param>
	/// <param name="Trigraphs">The brackets were written as the trigraphs <c>??(</c> and <c>??)</c>.</param>
	public sealed record Element(Expression Collection, Expression Index, Expression? To = null, bool Trigraphs = false) : Expression;

	/// <summary>
	/// BNF: a JSON simplified accessor's <c>.*</c> and <c>[*]</c>.
	/// T-SQL: <c>t.*</c> as an argument (CHECKSUM, BINARY_CHECKSUM, CONTAINS, FREETEXT): all columns of <c>t</c>; the target is the table's reference, as in <see cref="SelectColumn.QualifiedAll"/>.
	/// </summary>
	/// <param name="Target">The value or table whose members or elements are all taken.</param>
	/// <param name="Kind">Which wildcard was written: <c>.*</c> or <c>[*]</c>.</param>
	public sealed record Wildcard(Expression Target, WildcardKind Kind) : Expression;

	/// <summary>
	/// BNF: a JSON simplified accessor's <c>&lt;JSON array accessor&gt;</c>, whose subscripts are in the path language: <c>a[$ to last]</c>.
	/// </summary>
	/// <param name="Target">The value the accessor is applied to.</param>
	/// <param name="Accessor">The accessor: a member, a wildcard or a subscript list in the JSON path language.</param>
	public sealed record JsonAccessor(Expression Target, JsonPathAccessor Accessor) : Expression;

	/// <param name="Sequence">The sequence the next value is taken from: <c>NEXT VALUE FOR s</c>.</param>
	public sealed partial record NextValue(QualifiedName Sequence) : Expression;

	/// <summary>
	/// BNF: <c>&lt;collate clause&gt;</c> on a character factor: <c>a COLLATE c</c>.
	/// </summary>
	/// <param name="Value">The character value whose collation is set.</param>
	/// <param name="Collation">The collation named after <c>COLLATE</c>.</param>
	public sealed record Collate(Expression Value, CollationName Collation) : Expression;

	/// <summary>
	/// BNF: <c>&lt;time zone&gt;</c>: <c>a AT TIME ZONE z</c>, and <c>a AT LOCAL</c>.
	/// </summary>
	/// <param name="Value">The datetime value that is converted.</param>
	/// <param name="Zone">The time zone, an interval or a name; null for <c>AT LOCAL</c>.</param>
	public sealed record AtTimeZone(Expression Value, Expression? Zone) : Expression;

	/// <summary>
	/// BNF: <c>&lt;interval value expression&gt;</c>'s <c>(a - b) DAY TO SECOND</c>: a datetime difference given the
	/// interval qualifier it is taken in.
	/// </summary>
	/// <param name="Value">The difference, usually a bracketed subtraction of two datetime values.</param>
	/// <param name="Qualifier">The interval qualifier written after it.</param>
	public sealed record IntervalQualified(Expression Value, IntervalQualifier Qualifier) : Expression;

	// BNF: <string value function>s whose arguments are key words and not a comma list.

	/// <summary>
	/// <c>SUBSTRING (a FROM b [FOR c] [USING CHARACTERS | OCTETS])</c>, the standard's character substring function.
	/// </summary>
	/// <param name="Value">The string a part is taken of.</param>
	/// <param name="From">The position the part starts at.</param>
	/// <param name="For">The length of the part; null where the part runs to the end.</param>
	/// <param name="Using">The unit of the positions and the length; null where none was written.</param>
	public sealed record Substring(Expression Value, Expression From, Expression? For, CharacterLengthUnits? Using) : Expression;

	/// <summary>
	/// <c>SUBSTRING (a SIMILAR p ESCAPE e)</c>, the regular substring function.
	/// </summary>
	/// <param name="Value">The string searched.</param>
	/// <param name="Pattern">The <c>SIMILAR</c> pattern whose marked part is returned.</param>
	/// <param name="Escape">The escape character of the pattern.</param>
	public sealed record SubstringSimilar(Expression Value, Expression Pattern, Expression Escape) : Expression;

	/// <summary>
	/// <c>TRIM ([LEADING | TRAILING | BOTH] [c] [FROM] a)</c>: a string with characters removed from its ends.
	/// </summary>
	/// <param name="Specification">Which end is trimmed; null where none was written.</param>
	/// <param name="Character">The character that is removed; null where the blank is meant.</param>
	/// <param name="FromKeyword">The <c>FROM</c> key word was written before <paramref name="Source"/>.</param>
	/// <param name="Source">The string that is trimmed.</param>
	public sealed record Trim(TrimSpecification? Specification, Expression? Character, bool FromKeyword, Expression Source) : Expression;

	/// <summary>
	/// <c>OVERLAY (a PLACING b FROM c [FOR d] [USING CHARACTERS | OCTETS])</c>: a string with a part replaced.
	/// </summary>
	/// <param name="Value">The string whose part is replaced.</param>
	/// <param name="Placing">The string put in place of the part.</param>
	/// <param name="From">The position the replaced part starts at.</param>
	/// <param name="For">The length of the replaced part; null where it is the length of <paramref name="Placing"/>.</param>
	/// <param name="Using">The unit of the position and the length; null where none was written.</param>
	public sealed record Overlay(Expression Value, Expression Placing, Expression From, Expression? For, CharacterLengthUnits? Using) : Expression;

	/// <summary>
	/// <c>POSITION (a IN b [USING CHARACTERS | OCTETS])</c>: where a string is found in another.
	/// </summary>
	/// <param name="Value">The string that is looked for.</param>
	/// <param name="Within">The string that is searched.</param>
	/// <param name="Using">The unit the position is counted in; null where none was written.</param>
	public sealed record Position(Expression Value, Expression Within, CharacterLengthUnits? Using) : Expression;

	/// <summary>
	/// <c>CHAR_LENGTH (a)</c>, <c>CHARACTER_LENGTH (a)</c> or <c>OCTET_LENGTH (a)</c>: the length of a string.
	/// </summary>
	/// <param name="Function">Which of the three functions was written.</param>
	/// <param name="Value">The string measured.</param>
	/// <param name="Using">The unit of the length, <c>USING CHARACTERS | OCTETS</c>; null where none was written.</param>
	public sealed record Length(LengthFunction Function, Expression Value, CharacterLengthUnits? Using) : Expression;

	/// <summary>
	/// <c>EXTRACT (YEAR FROM a)</c>: one field of a datetime or interval value.
	/// </summary>
	/// <param name="Field">The field that is taken.</param>
	/// <param name="Source">The datetime or interval value it is taken from.</param>
	public sealed record Extract(ExtractField Field, Expression Source) : Expression;

	/// <summary>
	/// BNF: <c>&lt;normalize function&gt;</c>: <c>NORMALIZE (a, NFC, 10)</c>; a result length may carry a multiplier and its units, <c>10 K CHARACTERS</c>.
	/// </summary>
	/// <param name="Value">The string that is normalized.</param>
	/// <param name="Form">The normal form; null where none was written.</param>
	/// <param name="MaxLength">The largest length of the result; null where none was written, and always null without a <paramref name="Form"/>.</param>
	public sealed record Normalize(Expression Value, NormalForm? Form, Expression? MaxLength) : Expression
	{
		/// <summary>
		/// The multiplier letter written against the length, <c>K</c>, <c>M</c> or <c>G</c>; null where there is none.
		/// </summary>
		public char? MaxLengthMultiplier { get; init; }

		/// <summary>
		/// The unit the length is counted in, <c>CHARACTERS</c> or <c>OCTETS</c>; null where none was written.
		/// </summary>
		public LengthUnit? MaxLengthUnit { get; init; }
	}

	/// <summary>
	/// <c>CONVERT (a USING n)</c> and <c>TRANSLATE (a USING n)</c>: a string converted by a named conversion.
	/// </summary>
	/// <param name="Function">Which of the two functions was written.</param>
	/// <param name="Value">The string that is converted.</param>
	/// <param name="Name">The name of the conversion after <c>USING</c>.</param>
	public sealed record TranslateUsing(TranslateFunction Function, Expression Value, QualifiedName Name) : Expression;

	/// <summary>
	/// BNF: <c>&lt;regex occurrences function&gt;</c>, <c>&lt;regex position expression&gt;</c>, <c>&lt;regex substring function&gt;</c>, <c>&lt;regex transliteration&gt;</c>:
	/// <c>SUBSTRING_REGEX (p FLAG f IN a FROM n OCCURRENCE k GROUP g)</c>. Only the clauses the function takes are set.
	/// </summary>
	/// <param name="Function">Which of the four regular expression functions was written.</param>
	/// <param name="Pattern">The regular expression.</param>
	/// <param name="Value">The string searched, after <c>IN</c>.</param>
	public sealed record Regex(RegexFunction Function, Expression Pattern, Expression Value) : Expression
	{
		/// <summary>
		/// The flags of the pattern, after <c>FLAG</c>; null where none was written.
		/// </summary>
		public Expression? Flag { get; init; }

		/// <summary>
		/// The position the search starts at, after <c>FROM</c>; null where none was written.
		/// </summary>
		public Expression? From { get; init; }

		/// <summary>
		/// The unit of the positions, <c>USING CHARACTERS | OCTETS</c>; null where none was written.
		/// </summary>
		public CharacterLengthUnits? Using { get; init; }

		/// <summary>
		/// <c>START</c> or <c>AFTER</c> in a position function: whether the position is where the match starts or where it ends; null elsewhere.
		/// </summary>
		public RegexPositionStartOrAfter? StartOrAfter { get; init; }

		/// <summary>
		/// The string put in place of a match, after <c>WITH</c>, in a transliteration; null elsewhere.
		/// </summary>
		public Expression? Replacement { get; init; }

		/// <summary>
		/// The number of the match that is taken, after <c>OCCURRENCE</c>; null where none was written or <see cref="AllOccurrences"/> is set.
		/// </summary>
		public Expression? Occurrence { get; init; }

		/// <summary>
		/// <c>OCCURRENCE ALL</c> was written: every match is replaced.
		/// </summary>
		public bool AllOccurrences { get; init; }

		/// <summary>
		/// The number of the capture group that is taken, after <c>GROUP</c>; null where none was written.
		/// </summary>
		public Expression? Group { get; init; }
	}

	// BNF predicate families collapsed into Expression children rather than a BooleanExpression hierarchy.

	/// <summary>
	/// A comparison predicate, <c>a = b</c>.
	/// T-SQL: Comparison Operators (Transact-SQL). <see cref="Exclamation"/> is true only for <c>!=</c>, which is
	/// <see cref="ComparisonOperator.NotEqual"/> written with <c>!</c>; <c>!&lt;</c> and <c>!&gt;</c> are operators of their own,
	/// <see cref="ComparisonOperator.NotLess"/> and <see cref="ComparisonOperator.NotGreater"/>. A spelling is kept as a flag beside the value here as
	/// <c>Trigraphs</c> is on <c>AssignmentTarget</c>, so one meaning stays one operator and the text still writes back.
	/// </summary>
	/// <param name="Left">The left operand.</param>
	/// <param name="Operator">The comparison operator.</param>
	/// <param name="Right">The right operand.</param>
	/// <param name="Exclamation">The inequality was written <c>!=</c> and not <c>&lt;&gt;</c>.</param>
	public sealed record Comparison(Expression Left, ComparisonOperator Operator, Expression Right, [property: Spelling] bool Exclamation = false) : Expression;

	/// <summary>
	/// A range predicate: <c>a [NOT] BETWEEN [ASYMMETRIC | SYMMETRIC] b AND c</c>.
	/// </summary>
	/// <param name="Value">The value that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Symmetry">The <c>ASYMMETRIC</c> or <c>SYMMETRIC</c> key word; null where none was written.</param>
	/// <param name="Lower">The bound after <c>BETWEEN</c>.</param>
	/// <param name="Upper">The bound after <c>AND</c>.</param>
	public sealed record Between(Expression Value, bool Not, BetweenSymmetry? Symmetry, Expression Lower, Expression Upper) : Expression;

	/// <summary>
	/// A membership predicate: <c>a [NOT] IN (1, 2)</c> or <c>a [NOT] IN (SELECT …)</c>.
	/// </summary>
	/// <param name="Value">The value that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="SourceValue">What the value is looked for in: a list of values or a subquery.</param>
	public sealed record In(Expression Value, bool Not, InSource SourceValue) : Expression;

	/// <summary>
	/// BNF: <c>&lt;like predicate&gt;</c>, <c>&lt;similar predicate&gt;</c>, <c>&lt;regex like predicate&gt;</c> — whose <c>FLAG</c> is <paramref name="Flag"/>:
	/// <c>a [NOT] LIKE p [ESCAPE e]</c>, <c>a [NOT] SIMILAR TO p</c>, <c>a [NOT] LIKE_REGEX p [FLAG f]</c>.
	/// </summary>
	/// <param name="Value">The string that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Kind">Which of the three predicates was written.</param>
	/// <param name="Pattern">The pattern the string is matched with.</param>
	/// <param name="Escape">The escape character of the pattern; null where none was written.</param>
	/// <param name="Flag">The flags of a <c>LIKE_REGEX</c> pattern; null where none was written.</param>
	public sealed partial record Like(Expression Value, bool Not, LikeKind Kind, Expression Pattern, Expression? Escape = null, Expression? Flag = null) : Expression;

	/// <summary>
	/// A null predicate: <c>a IS [NOT] NULL</c>.
	/// </summary>
	/// <param name="Value">The value that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	public sealed record IsNull(Expression Value, bool Not) : Expression;

	/// <summary>
	/// BNF: <c>&lt;boolean test&gt;</c>, <c>a IS NOT UNKNOWN</c>: a truth value tested for one of the three truth values.
	/// </summary>
	/// <param name="Value">The condition that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Truth">The truth value it is compared with: <c>TRUE</c>, <c>FALSE</c> or <c>UNKNOWN</c>.</param>
	public sealed record IsTruth(Expression Value, bool Not, BooleanLiteral Truth) : Expression;

	/// <summary>
	/// A quantified comparison predicate: <c>a = ANY (SELECT …)</c>, <c>a &lt; ALL (SELECT …)</c>.
	/// </summary>
	/// <param name="Left">The value compared with every row of the subquery.</param>
	/// <param name="Operator">The comparison operator.</param>
	/// <param name="Quantifier">The key word that says how many rows must satisfy it: <c>ALL</c>, <c>SOME</c> or <c>ANY</c>.</param>
	/// <param name="Query">The subquery.</param>
	public sealed record QuantifiedComparison(Expression Left, ComparisonOperator Operator, Quantifier Quantifier, Query Query) : Expression;

	/// <summary>
	/// An existence predicate: <c>EXISTS (SELECT …)</c>.
	/// </summary>
	/// <param name="Query">The subquery whose rows are looked for.</param>
	public sealed record Exists(Query Query) : Expression;

	/// <summary>
	/// A uniqueness predicate: <c>UNIQUE [NULLS [NOT] DISTINCT] (SELECT …)</c>.
	/// </summary>
	/// <param name="Nulls">Whether nulls are distinct from one another; null where none was written.</param>
	/// <param name="Query">The subquery whose rows are compared with one another.</param>
	public sealed record Unique(NullDistinctness? Nulls, Query Query) : Expression;

	/// <summary>
	/// BNF: <c>&lt;match predicate&gt;</c>: <c>a MATCH [UNIQUE] [SIMPLE | PARTIAL | FULL] (SELECT …)</c>.
	/// </summary>
	/// <param name="Value">The row value that is looked for.</param>
	/// <param name="UniqueKeyword">The <c>UNIQUE</c> key word was written.</param>
	/// <param name="Type">The kind of match; null where none was written.</param>
	/// <param name="Query">The subquery the row is looked for in.</param>
	public sealed record Match(Expression Value, bool UniqueKeyword, MatchType? Type, Query Query) : Expression;

	/// <summary>
	/// BNF: <c>&lt;overlaps predicate&gt;</c>, whose two sides are rows: <c>(a, b) OVERLAPS (c, d)</c>.
	/// </summary>
	/// <param name="Left">The row of the first period.</param>
	/// <param name="Right">The row of the second period.</param>
	public sealed record Overlaps(Expression Left, Expression Right) : Expression;

	/// <summary>
	/// A distinctness predicate: <c>a IS [NOT] DISTINCT FROM b</c>, which treats nulls as equal values.
	/// </summary>
	/// <param name="Left">The left operand.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Right">The right operand.</param>
	public sealed partial record IsDistinct(Expression Left, bool Not, Expression Right) : Expression;

	/// <summary>
	/// A normalization predicate: <c>a IS [NOT] [NFC | NFD | NFKC | NFKD] NORMALIZED</c>.
	/// </summary>
	/// <param name="Value">The string that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Form">The normal form; null where none was written.</param>
	public sealed record IsNormalized(Expression Value, bool Not, NormalForm? Form) : Expression;

	/// <summary>
	/// A member predicate: <c>a [NOT] MEMBER [OF] m</c>.
	/// </summary>
	/// <param name="Value">The value that is looked for.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="OfKeyword">The <c>OF</c> key word was written.</param>
	/// <param name="Collection">The multiset the value is looked for in.</param>
	public sealed record MemberOf(Expression Value, bool Not, bool OfKeyword, Expression Collection) : Expression;

	/// <summary>
	/// A submultiset predicate: <c>a [NOT] SUBMULTISET [OF] m</c>.
	/// </summary>
	/// <param name="Value">The multiset that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="OfKeyword">The <c>OF</c> key word was written.</param>
	/// <param name="Collection">The multiset that must contain it.</param>
	public sealed record SubmultisetOf(Expression Value, bool Not, bool OfKeyword, Expression Collection) : Expression;

	/// <summary>
	/// A set predicate: <c>a IS [NOT] A SET</c>, which is true for a multiset without duplicates.
	/// </summary>
	/// <param name="Value">The multiset that is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	public sealed record IsSet(Expression Value, bool Not) : Expression;

	/// <summary>
	/// A type predicate: <c>a IS [NOT] OF (ONLY t, u)</c>.
	/// </summary>
	/// <param name="Value">The value whose type is tested.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Types">The types it is tested against, each with an optional <c>ONLY</c>.</param>
	public sealed record IsOf(Expression Value, bool Not, SqlList<TypeTest> Types) : Expression;

	/// <summary>
	/// A period predicate: <c>p CONTAINS q</c>, <c>p OVERLAPS q</c>, <c>p IMMEDIATELY PRECEDES q</c> and the like.
	/// </summary>
	/// <param name="Left">The period on the left.</param>
	/// <param name="Operator">The period operator.</param>
	/// <param name="Right">The period or the point on the right.</param>
	public sealed record PeriodPredicate(PeriodValue Left, PeriodOperator Operator, PeriodRight Right) : Expression;

	/// <summary>
	/// A JSON predicate: <c>a [FORMAT JSON] IS [NOT] JSON [VALUE | ARRAY | OBJECT | SCALAR] [WITH | WITHOUT UNIQUE KEYS]</c>.
	/// </summary>
	/// <param name="Value">The value that is tested.</param>
	/// <param name="Input">The <c>FORMAT</c> clause after the value; null where none was written.</param>
	/// <param name="Not">The predicate is negated with <c>NOT</c>.</param>
	/// <param name="Type">The kind of JSON text required; null where none was written.</param>
	/// <param name="Uniqueness">Whether object keys must be unique; null where none was written.</param>
	public sealed record JsonPredicate(Expression Value, JsonInputClause? Input, bool Not, JsonPredicateType? Type, JsonKeyUniqueness? Uniqueness) : Expression;

	/// <summary>
	/// <c>JSON_EXISTS (a, p [PASSING …] [TRUE | FALSE | UNKNOWN | ERROR ON ERROR])</c>: whether a path finds an item.
	/// </summary>
	/// <param name="Common">The context value, the path and the <c>PASSING</c> arguments that every JSON query function takes.</param>
	/// <param name="OnError">What the function does on an error; null where no <c>ON ERROR</c> was written.</param>
	public sealed record JsonExists(JsonApiCommon Common, JsonExistsErrorBehavior? OnError) : Expression;

	// BNF: <JSON value function>, <JSON query>, the <JSON value constructor>s and <JSON aggregate function>s.

	/// <summary>
	/// <c>JSON_VALUE (a, p [RETURNING t] [… ON EMPTY] [… ON ERROR])</c>: a scalar taken from a JSON value by a path.
	/// </summary>
	/// <param name="Common">The context value, the path and the <c>PASSING</c> arguments.</param>
	/// <param name="Returning">The type of the result; null where no <c>RETURNING</c> was written.</param>
	/// <param name="OnEmpty">What the function returns when the path finds nothing; null where none was written.</param>
	/// <param name="OnError">What the function does on an error; null where none was written.</param>
	public sealed record JsonValue(JsonApiCommon Common, DataType? Returning, JsonValueBehavior? OnEmpty, JsonValueBehavior? OnError) : Expression;

	/// <summary>
	/// <c>JSON_QUERY (a, p [RETURNING t] [WITH [CONDITIONAL | UNCONDITIONAL] WRAPPER] [… QUOTES] [… ON EMPTY] [… ON ERROR])</c>: an object or an array taken from a JSON value by a path.
	/// </summary>
	/// <param name="Common">The context value, the path and the <c>PASSING</c> arguments; T-SQL's one-argument form has no path.</param>
	/// <param name="Output">The <c>RETURNING</c> clause: the type and the <c>FORMAT</c> of the result; null where none was written.</param>
	/// <param name="Wrapper">Whether the result is wrapped in an array; null where no <c>WRAPPER</c> was written.</param>
	/// <param name="Quotes">Whether a string result keeps its quotes; null where no <c>QUOTES</c> was written.</param>
	/// <param name="OnEmpty">What the function returns when the path finds nothing; null where none was written.</param>
	/// <param name="OnError">What the function does on an error; null where none was written.</param>
	public sealed record JsonQuery(JsonApiCommon Common, JsonOutput? Output, JsonWrapperBehavior? Wrapper, JsonQuotes? Quotes, JsonQueryBehavior? OnEmpty, JsonQueryBehavior? OnError) : Expression;

	/// <summary>
	/// <c>JSON_OBJECT (k : v, … [NULL | ABSENT ON NULL] [WITH | WITHOUT UNIQUE KEYS] [RETURNING t])</c>: a JSON object built from key and value pairs.
	/// </summary>
	/// <param name="Members">The key and value pairs; empty for <c>JSON_OBJECT ()</c>.</param>
	/// <param name="Nulls">What a null value does: kept as a JSON null or left out; null where none was written.</param>
	/// <param name="Uniqueness">Whether the keys must be unique; null where none was written.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	public sealed record JsonObject(SqlList<JsonMember> Members, JsonNullHandling? Nulls, JsonKeyUniqueness? Uniqueness, JsonOutput? Output) : Expression;

	/// <summary>
	/// <c>JSON_ARRAY (a, b, … [NULL | ABSENT ON NULL] [RETURNING t])</c>: a JSON array built from values.
	/// </summary>
	/// <param name="Elements">The values; empty for <c>JSON_ARRAY ()</c>.</param>
	/// <param name="Nulls">What a null value does: kept as a JSON null or left out; null where none was written.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	public sealed record JsonArray(SqlList<JsonElement> Elements, JsonNullHandling? Nulls, JsonOutput? Output) : Expression;

	/// <summary>
	/// <c>JSON_ARRAY (SELECT … [FORMAT JSON] [NULL | ABSENT ON NULL] [RETURNING t])</c>: a JSON array built from the rows of a query.
	/// </summary>
	/// <param name="Query">The query whose rows become the elements.</param>
	/// <param name="Format">The <c>FORMAT</c> clause after the query; null where none was written.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	/// <param name="Nulls">What a null value does: kept as a JSON null or left out; null where none was written.</param>
	public sealed record JsonArrayQuery(Query Query, JsonInputClause? Format, JsonOutput? Output, JsonNullHandling? Nulls = null) : Expression;

	/// <summary>
	/// <c>JSON_OBJECTAGG (k : v [NULL | ABSENT ON NULL] [WITH | WITHOUT UNIQUE KEYS] [RETURNING t])</c>: an aggregate that builds one JSON object from the pairs of a group or window.
	/// </summary>
	/// <param name="Pair">The key and value taken from each row.</param>
	/// <param name="Nulls">What a null value does: kept as a JSON null or left out; null where none was written.</param>
	/// <param name="Uniqueness">Whether the keys must be unique; null where none was written.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	public sealed record JsonObjectAggregate(JsonMember Pair, JsonNullHandling? Nulls, JsonKeyUniqueness? Uniqueness, JsonOutput? Output) : Expression
	{
		/// <summary>
		/// BNF: <c>&lt;aggregate function&gt;</c>'s <c>&lt;filter clause&gt;</c>, <c>FILTER (WHERE c)</c>; null where none was written.
		/// </summary>
		public FilterClause? Filter { get; init; }

		/// <summary>
		/// A <c>&lt;window function&gt;</c>'s window, <c>OVER w</c> or <c>OVER (…)</c>; null for a plain aggregate.
		/// </summary>
		public WindowReference? Over { get; init; }

		/// <summary>
		/// A navigation's <c>RUNNING</c> or <c>FINAL</c> before the function in a row pattern; null where none was written.
		/// </summary>
		public RowPatternSemantics? Semantics { get; init; }
	}

	/// <summary>
	/// <c>JSON_ARRAYAGG (a [ORDER BY …] [NULL | ABSENT ON NULL] [RETURNING t])</c>: an aggregate that builds one JSON array from the values of a group or window.
	/// </summary>
	/// <param name="Item">The value taken from each row.</param>
	/// <param name="OrderBy">The order of the elements; null where none was written.</param>
	/// <param name="Nulls">What a null value does: kept as a JSON null or left out; null where none was written.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	public sealed record JsonArrayAggregate(JsonElement Item, OrderByClause? OrderBy, JsonNullHandling? Nulls, JsonOutput? Output) : Expression
	{
		/// <summary>
		/// BNF: <c>&lt;aggregate function&gt;</c>'s <c>&lt;filter clause&gt;</c>, <c>FILTER (WHERE c)</c>; null where none was written.
		/// </summary>
		public FilterClause? Filter { get; init; }

		/// <summary>
		/// A <c>&lt;window function&gt;</c>'s window, <c>OVER w</c> or <c>OVER (…)</c>; null for a plain aggregate.
		/// </summary>
		public WindowReference? Over { get; init; }

		/// <summary>
		/// A navigation's <c>RUNNING</c> or <c>FINAL</c> before the function in a row pattern; null where none was written.
		/// </summary>
		public RowPatternSemantics? Semantics { get; init; }
	}

	/// <summary>
	/// <c>JSON (a [FORMAT …] [WITH | WITHOUT UNIQUE KEYS])</c>: JSON text parsed into a JSON value.
	/// </summary>
	/// <param name="Value">The text that is parsed.</param>
	/// <param name="Input">The <c>FORMAT</c> clause after the value; null where none was written.</param>
	/// <param name="Uniqueness">Whether object keys must be unique; null where none was written.</param>
	public sealed record JsonParse(Expression Value, JsonInputClause? Input, JsonKeyUniqueness? Uniqueness) : Expression;

	/// <summary>
	/// <c>JSON_SCALAR (a)</c>: an SQL value turned into a JSON scalar.
	/// </summary>
	/// <param name="Value">The SQL value that is converted.</param>
	public sealed record JsonScalar(Expression Value) : Expression;

	/// <summary>
	/// <c>JSON_SERIALIZE (a [RETURNING t])</c>: a JSON value written out as character or binary text.
	/// </summary>
	/// <param name="Value">The JSON value that is serialized.</param>
	/// <param name="Output">The <c>RETURNING</c> clause; null where none was written.</param>
	public sealed record JsonSerialize(Expression Value, JsonOutput? Output) : Expression;

	/// <summary>
	/// BNF: <c>&lt;table argument&gt;</c>: <c>TABLE (t)</c>, <c>TABLE (SELECT ...)</c> or a call, with its name, partitioning, pruning and order;
	/// an argument of a polymorphic table function.
	/// </summary>
	/// <param name="Source">The table: a table name, a subquery or a table function call.</param>
	/// <param name="TableKeyword">The <c>TABLE</c> key word was written before the source.</param>
	public sealed record TableArgument(Expression Source, bool TableKeyword) : Expression
	{
		/// <summary>
		/// The correlation name the table is given, with its column names where they are listed; null where none was written.
		/// </summary>
		public Alias? Alias { get; init; }

		/// <summary>
		/// The columns of <c>PARTITION BY</c>; null where no partitioning was written.
		/// </summary>
		public SqlList<Expression>? PartitionBy { get; init; }

		/// <summary>
		/// The partitioning columns were written in brackets, <c>PARTITION BY (a, b)</c>, which is the only way to write none or several.
		/// </summary>
		public bool PartitionParenthesized { get; init; }

		/// <summary>
		/// <c>PRUNE WHEN EMPTY</c> or <c>KEEP WHEN EMPTY</c>; null where none was written.
		/// </summary>
		public TablePruning? Pruning { get; init; }

		/// <summary>
		/// The sort items of <c>ORDER BY</c>; null where no ordering was written.
		/// </summary>
		public SqlList<SortItem>? OrderBy { get; init; }

		/// <summary>
		/// The ordering items were written in brackets, <c>ORDER BY (a, b)</c>.
		/// </summary>
		public bool OrderByParenthesized { get; init; }
	}

	/// <summary>
	/// BNF: <c>&lt;descriptor value constructor&gt;</c>, <c>DESCRIPTOR (a INT, b)</c>: a list of column names, each with an optional type,
	/// passed to a polymorphic table function.
	/// </summary>
	/// <param name="Columns">The columns of the descriptor.</param>
	public sealed record DescriptorConstructor(SqlList<DescriptorColumn> Columns) : Expression;

	/// <summary>
	/// BNF: <c>&lt;current collation specification&gt;</c>, <c>COLLATION FOR (x)</c>: the collation of a value.
	/// </summary>
	/// <param name="Value">The value whose collation is asked.</param>
	public sealed record CollationFor(Expression Value) : Expression;

	/// <summary>
	/// BNF: <c>&lt;default specification&gt;</c>, <c>DEFAULT</c> where a value stands: the default of a column or a parameter.
	/// </summary>
	public sealed record Default : Expression;

	/// <summary>
	/// BNF: <c>&lt;row marker expression&gt;</c>, <c>END_FRAME - 1</c>: a row of a window, with an offset from it.
	/// </summary>
	/// <param name="Kind">The row the marker names.</param>
	/// <param name="DeltaSign">The sign of the offset, <see cref="UnaryOperator.Plus"/> or <see cref="UnaryOperator.Minus"/>; null where there is no offset.</param>
	/// <param name="Delta">The number of rows of the offset; null where there is no offset.</param>
	public sealed record RowMarker(RowMarkerKind Kind, UnaryOperator? DeltaSign = null, Expression? Delta = null) : Expression;

	/// <summary>
	/// BNF: <c>&lt;value_of expression at row&gt;</c>, <c>VALUE_OF (x AT BEGIN_FRAME, d)</c>: the value of an expression taken at another row of the window.
	/// </summary>
	/// <param name="Value">The expression that is evaluated.</param>
	/// <param name="At">The row it is evaluated at, usually a <see cref="RowMarker"/>.</param>
	/// <param name="Otherwise">The value used where there is no such row; null where none was written.</param>
	public sealed record ValueOf(Expression Value, Expression At, Expression? Otherwise = null) : Expression;

	/// <summary>
	/// An expression of a dialect's own that no other node models.
	/// </summary>
	/// <param name="Dialect">The name of the dialect that defines the construct.</param>
	/// <param name="Kind">The name of the construct within the dialect.</param>
	/// <param name="Parts">The nodes the construct is made of.</param>
	public sealed record Extension(string Dialect, string Kind, SqlList<ISqlNode> Parts) : Expression;
}

/// <summary>
/// What a <see cref="Expression.Parameter"/> stands for.
/// </summary>
public enum ParameterKind
{
	/// <summary>A host parameter, <c>:a</c>.</summary>
	Host,

	/// <summary>An SQL parameter, which has no text of its own to write.</summary>
	Sql,

	/// <summary>A dynamic parameter, <c>?</c>, whose value is given when the statement runs.</summary>
	Dynamic,

	/// <summary>An embedded variable of the host program, in embedded SQL.</summary>
	Embedded,
}

/// <summary>
/// Which value a <see cref="Expression.Current"/> reads: a key word of the standard with no argument, or one with a precision.
/// </summary>
public enum CurrentValue
{
	/// <summary><c>CURRENT_CATALOG</c>: the current catalog.</summary>
	Catalog,

	/// <summary><c>CURRENT_DATE</c>: the current date.</summary>
	Date,

	/// <summary><c>CURRENT_DEFAULT_TRANSFORM_GROUP</c>: the transform group used by default.</summary>
	DefaultTransformGroup,

	/// <summary><c>CURRENT_PATH</c>: the schema search path.</summary>
	Path,

	/// <summary><c>CURRENT_ROLE</c>: the current role.</summary>
	Role,

	/// <summary><c>CURRENT_SCHEMA</c>: the current schema.</summary>
	Schema,

	/// <summary><c>CURRENT_TIME [(p)]</c>: the current time with a time zone.</summary>
	Time,

	/// <summary><c>CURRENT_TIMESTAMP [(p)]</c>: the current timestamp with a time zone.</summary>
	Timestamp,

	/// <summary><c>USER</c>: the current user, as <c>CURRENT_USER</c>.</summary>
	User,

	/// <summary><c>SESSION_USER</c>: the user of the session.</summary>
	SessionUser,

	/// <summary><c>SYSTEM_USER</c>: the user as the operating system knows them.</summary>
	SystemUser,

	/// <summary><c>VALUE</c>: the value under check in a domain's constraint.</summary>
	Value,

	/// <summary><c>LOCALTIME [(p)]</c>: the current time without a time zone.</summary>
	LocalTime,

	/// <summary><c>LOCALTIMESTAMP [(p)]</c>: the current timestamp without a time zone.</summary>
	LocalTimestamp,

	/// <summary><c>CURRENT_USER</c>: the current user.</summary>
	CurrentUser,

	/// <summary><c>CURRENT_TRANSFORM_GROUP_FOR_TYPE t</c>: the transform group used for the named type.</summary>
	TransformGroupForType,
}

/// <summary>
/// A unary operator.
/// T-SQL: Bitwise Operators (Transact-SQL). <see cref="BitwiseNot"/> is <c>~</c>.
/// </summary>
public enum UnaryOperator
{
	/// <summary>The unary plus, <c>+a</c>.</summary>
	Plus,

	/// <summary>The unary minus, <c>-a</c>.</summary>
	Minus,

	/// <summary>The logical negation, <c>NOT a</c>.</summary>
	Not,

	/// <summary>T-SQL: the bitwise complement, <c>~a</c>.</summary>
	BitwiseNot,
}

/// <summary>
/// A binary operator.
/// T-SQL: Arithmetic Operators, Bitwise Operators (Transact-SQL). From <see cref="Modulo"/> on: <c>%</c>, <c>&amp;</c>, <c>|</c>, <c>^</c>,
/// <c>&lt;&lt;</c> and <c>&gt;&gt;</c>. <c>%</c> binds as <c>*</c> does.
/// </summary>
public enum BinaryOperator
{
	/// <summary>Addition, <c>a + b</c>.</summary>
	Add,

	/// <summary>Subtraction, <c>a - b</c>.</summary>
	Subtract,

	/// <summary>Multiplication, <c>a * b</c>.</summary>
	Multiply,

	/// <summary>Division, <c>a / b</c>.</summary>
	Divide,

	/// <summary>Concatenation, <c>a || b</c>.</summary>
	Concatenate,

	/// <summary>The logical conjunction, <c>a AND b</c>.</summary>
	And,

	/// <summary>The logical disjunction, <c>a OR b</c>.</summary>
	Or,

	/// <summary>T-SQL: the remainder, <c>a % b</c>.</summary>
	Modulo,

	/// <summary>T-SQL: the bitwise AND, <c>a &amp; b</c>.</summary>
	BitwiseAnd,

	/// <summary>T-SQL: the bitwise OR, <c>a | b</c>.</summary>
	BitwiseOr,

	/// <summary>T-SQL: the bitwise exclusive OR, <c>a ^ b</c>.</summary>
	BitwiseXor,

	/// <summary>T-SQL: the left shift, <c>a &lt;&lt; b</c>.</summary>
	ShiftLeft,

	/// <summary>T-SQL: the right shift, <c>a &gt;&gt; b</c>.</summary>
	ShiftRight,
}

/// <summary>
/// The operator of a multiset combination, <c>a MULTISET UNION b</c>.
/// </summary>
public enum MultisetOperator
{
	/// <summary><c>MULTISET UNION</c>.</summary>
	Union,

	/// <summary><c>MULTISET EXCEPT</c>.</summary>
	Except,

	/// <summary><c>MULTISET INTERSECT</c>.</summary>
	Intersect,
}

/// <summary>
/// A comparison operator.
/// T-SQL: Comparison Operators (Transact-SQL). <see cref="NotLess"/> is <c>!&lt;</c> and <see cref="NotGreater"/> is <c>!&gt;</c>, which the
/// standard does not have. T-SQL's <c>!=</c> is <see cref="NotEqual"/> written differently, not an operator of its own.
/// </summary>
public enum ComparisonOperator
{
	/// <summary><c>=</c>.</summary>
	Equal,

	/// <summary><c>&lt;&gt;</c>.</summary>
	NotEqual,

	/// <summary><c>&lt;</c>.</summary>
	Less,

	/// <summary><c>&gt;</c>.</summary>
	Greater,

	/// <summary><c>&lt;=</c>.</summary>
	LessOrEqual,

	/// <summary><c>&gt;=</c>.</summary>
	GreaterOrEqual,

	/// <summary>T-SQL: <c>!&lt;</c>, not less than.</summary>
	NotLess,

	/// <summary>T-SQL: <c>!&gt;</c>, not greater than.</summary>
	NotGreater,
}

/// <summary>
/// Whether the bounds of a <c>BETWEEN</c> may be given in either order.
/// </summary>
public enum BetweenSymmetry
{
	/// <summary><c>ASYMMETRIC</c>: the lower bound must come first.</summary>
	Asymmetric,

	/// <summary><c>SYMMETRIC</c>: the bounds are put in order before the test.</summary>
	Symmetric,
}

/// <summary>
/// The key word of a quantified comparison, <c>a = ANY (SELECT …)</c>.
/// </summary>
public enum Quantifier
{
	/// <summary><c>ALL</c>: the comparison holds for every row.</summary>
	All,

	/// <summary><c>SOME</c>: the comparison holds for at least one row, as <c>ANY</c>.</summary>
	Some,

	/// <summary><c>ANY</c>: the comparison holds for at least one row, as <c>SOME</c>.</summary>
	Any,
}

/// <summary>
/// The kind of a <see cref="Expression.Like"/> predicate.
/// </summary>
public enum LikeKind
{
	/// <summary><c>LIKE</c>.</summary>
	Like,

	/// <summary><c>SIMILAR TO</c>.</summary>
	Similar,

	/// <summary><c>LIKE_REGEX</c>.</summary>
	Regex,
}

/// <summary>
/// The kind of collection a <see cref="Expression.CollectionQuery"/> builds from a subquery.
/// </summary>
public enum CollectionKind
{
	/// <summary><c>ARRAY (SELECT …)</c>.</summary>
	Array,

	/// <summary><c>MULTISET (SELECT …)</c>.</summary>
	Multiset,

	/// <summary><c>TABLE (SELECT …)</c>.</summary>
	Table,
}

/// <summary>
/// How a <see cref="Expression.Member"/> is reached from its target.
/// </summary>
public enum MemberAccessKind
{
	/// <summary><c>a.m</c>: an attribute or a method of a value.</summary>
	Dot,

	/// <summary><c>a -&gt; m</c>: a member of the value a reference points to.</summary>
	Dereference,

	/// <summary><c>T::m</c>: a static method of a type.</summary>
	StaticMethod,
}

/// <summary>
/// Which wildcard a <see cref="Expression.Wildcard"/> is.
/// </summary>
public enum WildcardKind
{
	/// <summary><c>.*</c>: every member.</summary>
	Member,

	/// <summary><c>[*]</c>: every element.</summary>
	Array,
}

/// <summary>
/// A Unicode normal form.
/// </summary>
public enum NormalForm
{
	/// <summary><c>NFC</c>: canonical composition.</summary>
	NFC,

	/// <summary><c>NFD</c>: canonical decomposition.</summary>
	NFD,

	/// <summary><c>NFKC</c>: compatibility composition.</summary>
	NFKC,

	/// <summary><c>NFKD</c>: compatibility decomposition.</summary>
	NFKD,
}

/// <summary>
/// Which end a window function's <c>NTH_VALUE</c> counts from: <c>FROM FIRST</c> or <c>FROM LAST</c>.
/// </summary>
public enum FromFirstOrLast
{
	/// <summary><c>FROM FIRST</c>: the count starts at the first row of the window frame.</summary>
	First,

	/// <summary><c>FROM LAST</c>: the count starts at the last row of the window frame.</summary>
	Last,
}

/// <summary>
/// How a window function treats null values: <c>RESPECT NULLS</c> or <c>IGNORE NULLS</c>.
/// </summary>
public enum NullTreatment
{
	/// <summary><c>RESPECT NULLS</c>: null values are counted as rows.</summary>
	RespectNulls,

	/// <summary><c>IGNORE NULLS</c>: rows with a null value are skipped.</summary>
	IgnoreNulls,
}

/// <summary>
/// The semantics of a navigation in a row pattern.
/// </summary>
public enum RowPatternSemantics
{
	/// <summary><c>RUNNING</c>: the value as of the current row of the match.</summary>
	Running,

	/// <summary><c>FINAL</c>: the value as of the last row of the match.</summary>
	Final,
}

/// <summary>
/// The unit a string function counts in: <c>USING CHARACTERS</c> or <c>USING OCTETS</c>.
/// </summary>
public enum CharacterLengthUnits
{
	/// <summary><c>CHARACTERS</c>: characters.</summary>
	Characters,

	/// <summary><c>OCTETS</c>: bytes.</summary>
	Octets,
}

/// <summary>
/// Which end of a string <c>TRIM</c> removes characters from.
/// </summary>
public enum TrimSpecification
{
	/// <summary><c>LEADING</c>: the start.</summary>
	Leading,

	/// <summary><c>TRAILING</c>: the end.</summary>
	Trailing,

	/// <summary><c>BOTH</c>: both ends.</summary>
	Both,
}

/// <summary>
/// The function a <see cref="Expression.Length"/> was written with.
/// </summary>
public enum LengthFunction
{
	/// <summary><c>CHAR_LENGTH</c>: the length in characters.</summary>
	CharLength,

	/// <summary><c>CHARACTER_LENGTH</c>: the length in characters, as <c>CHAR_LENGTH</c>.</summary>
	CharacterLength,

	/// <summary><c>OCTET_LENGTH</c>: the length in octets.</summary>
	OctetLength,
}

/// <summary>
/// The function a <see cref="Expression.TranslateUsing"/> was written with.
/// </summary>
public enum TranslateFunction
{
	/// <summary><c>CONVERT (a USING n)</c>: a conversion of the character set.</summary>
	Convert,

	/// <summary><c>TRANSLATE (a USING n)</c>: a transliteration.</summary>
	Translate,
}

/// <summary>
/// A field of a datetime or an interval, as <c>EXTRACT</c> takes it.
/// </summary>
public enum ExtractField
{
	/// <summary><c>YEAR</c>.</summary>
	Year,

	/// <summary><c>MONTH</c>.</summary>
	Month,

	/// <summary><c>DAY</c>.</summary>
	Day,

	/// <summary><c>HOUR</c>.</summary>
	Hour,

	/// <summary><c>MINUTE</c>.</summary>
	Minute,

	/// <summary><c>SECOND</c>.</summary>
	Second,

	/// <summary><c>TIMEZONE_HOUR</c>: the hour part of the time zone offset.</summary>
	TimezoneHour,

	/// <summary><c>TIMEZONE_MINUTE</c>: the minute part of the time zone offset.</summary>
	TimezoneMinute,
}

/// <summary>
/// The regular expression function an <see cref="Expression.Regex"/> was written with.
/// </summary>
public enum RegexFunction
{
	/// <summary><c>OCCURRENCES_REGEX</c>: the number of matches.</summary>
	OccurrencesRegex,

	/// <summary><c>POSITION_REGEX</c>: the position of a match.</summary>
	PositionRegex,

	/// <summary><c>SUBSTRING_REGEX</c>: the text of a match.</summary>
	SubstringRegex,

	/// <summary><c>TRANSLATE_REGEX</c>: the string with matches replaced.</summary>
	TranslateRegex,
}

/// <summary>
/// Which end of a match <c>POSITION_REGEX</c> reports.
/// </summary>
public enum RegexPositionStartOrAfter
{
	/// <summary><c>START</c>: the position where the match starts.</summary>
	Start,

	/// <summary><c>AFTER</c>: the position just after the match.</summary>
	After,
}

/// <summary>
/// BNF: <c>&lt;descriptor column specification&gt;</c>: one column of a <c>DESCRIPTOR (a INT, b)</c> constructor.
/// </summary>
/// <param name="Name">The column's name.</param>
/// <param name="Type">The column's data type, or null where only the name was written.</param>
public sealed record DescriptorColumn(Identifier Name, DataType? Type = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// BNF: <c>&lt;row marker&gt;</c>: the key word of a <see cref="Expression.RowMarker"/>, which names a row of the
/// window by its place in the partition or the frame.
/// </summary>
public enum RowMarkerKind
{
	/// <summary><c>BEGIN_PARTITION</c>: the first row of the partition.</summary>
	BeginPartition,

	/// <summary><c>BEGIN_FRAME</c>: the first row of the window frame.</summary>
	BeginFrame,

	/// <summary><c>CURRENT_ROW</c>: the row the window function is evaluated for.</summary>
	CurrentRow,

	/// <summary><c>FRAME_ROW</c>: the row of the frame the enclosing function is reading.</summary>
	FrameRow,

	/// <summary><c>END_FRAME</c>: the last row of the window frame.</summary>
	EndFrame,

	/// <summary><c>END_PARTITION</c>: the last row of the partition.</summary>
	EndPartition,
}

/// <summary>
/// BNF: <c>&lt;listagg overflow clause&gt;</c>: what <c>LISTAGG</c> does when its result does not fit,
/// <c>ON OVERFLOW ERROR</c>, or <c>ON OVERFLOW TRUNCATE [filler] WITH|WITHOUT COUNT</c>.
/// </summary>
/// <param name="Truncate"><c>true</c> for <c>TRUNCATE</c>, <c>false</c> for <c>ERROR</c>, where the other two parts are not written.</param>
/// <param name="Filler">The text put where the result is cut, or null where none was written (the default filler).</param>
/// <param name="WithCount"><c>true</c> for <c>WITH COUNT</c>, which appends how many values were cut, and <c>false</c> or null for <c>WITHOUT COUNT</c>; read after <c>TRUNCATE</c> only.</param>
public sealed record ListaggOverflow(bool Truncate, Expression? Filler = null, bool? WithCount = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>
/// A literal: a value written out. It is an <see cref="Expression"/> itself wherever a value stands, so a
/// literal costs one node; a position that takes only a literal (a default, an option, a length) is typed
/// <see cref="LiteralValue"/>.
/// </summary>
public abstract partial record LiteralValue : Expression
{
	private protected LiteralValue()
	{
	}

	/// <summary>A number: <c>42</c>, <c>1.5</c>, <c>2E3</c>, <c>0xFF</c>.</summary>
	/// <param name="Text">The number as written, with its sign where it was written with one.</param>
	/// <param name="Kind">The form the number is written in: a whole number of some radix, with a point, with an exponent, or a currency amount.</param>
	public sealed record Numeric(string Text, NumericLiteralKind Kind) : LiteralValue;

	/// <summary>A character string: <c>'abc'</c>, <c>N'abc'</c>, <c>U&amp;'\0061'</c>, <c>_utf8'abc'</c>.</summary>
	/// <param name="Text">
	/// The literal's spelling as written, from its first quote to its last: the segments of <c>'a' 'b'</c>
	/// and the separators between them included, the introducer not.
	/// </param>
	/// <param name="Kind">Which prefix it was written with, if any.</param>
	/// <param name="CharacterSet">The introducer, <c>_utf8</c> in <c>_utf8'abc'</c>, or null where none was written.</param>
	/// <param name="UnicodeEscape">The escape character of <c>UESCAPE 'x'</c> after a Unicode literal, or null where none was written.</param>
	public sealed record String(string Text, StringLiteralKind Kind, CharacterSetName? CharacterSet = null, char? UnicodeEscape = null) : LiteralValue;
	/// <summary>A binary string: <c>X'1F'</c>, or in T-SQL <c>0x1F</c>.</summary>
	/// <param name="Text">The quoted hexadecimal digits for the <c>X'..'</c> form, and the bare digits after <c>0x</c> for the T-SQL form.</param>
	// `new`: Expression.Binary, an operator applied to two values, is another node, which a literal's own
	// Binary hides inside LiteralValue.
	public new sealed partial record Binary(string Text) : LiteralValue;

	/// <summary>A truth value: <c>TRUE</c>, <c>FALSE</c> or <c>UNKNOWN</c>.</summary>
	/// <param name="Value">Which of the three.</param>
	public sealed record Boolean(BooleanLiteral Value) : LiteralValue;

	/// <summary>A date, time or timestamp: <c>DATE '2024-01-31'</c>.</summary>
	/// <param name="Kind">The key word before the quoted text.</param>
	/// <param name="Text">The quoted text, quotes included.</param>
	public sealed record DateTime(DateTimeLiteralKind Kind, string Text) : LiteralValue;

	/// <summary>An interval: <c>INTERVAL '1' DAY</c>, <c>INTERVAL -'1-2' YEAR TO MONTH</c>.</summary>
	/// <param name="Text">The quoted text, quotes included.</param>
	/// <param name="Qualifier">The fields the text counts, <c>DAY</c> or <c>YEAR TO MONTH</c>.</param>
	/// <param name="Sign">The sign written before the quoted text, as in <c>INTERVAL -'1' DAY</c>, or null where none was written.</param>
	public sealed record Interval(string Text, IntervalQualifier Qualifier, UnaryOperator? Sign = null) : LiteralValue;

	/// <summary>The <c>NULL</c> literal.</summary>
	public sealed record Null : LiteralValue;
}
/// <summary>The form a number is written in.</summary>
public enum NumericLiteralKind
{
	/// <summary>A whole number in decimal digits: <c>42</c>.</summary>
	DecimalInteger,

	/// <summary>A whole number in hexadecimal: <c>0xFF</c>.</summary>
	HexInteger,

	/// <summary>A whole number in octal: <c>0o17</c>.</summary>
	OctalInteger,

	/// <summary>A whole number in binary: <c>0b101</c>.</summary>
	BinaryInteger,

	/// <summary>An exact number with a decimal point: <c>1.5</c>.</summary>
	Decimal,

	/// <summary>An approximate number with an exponent: <c>1.5E3</c>.</summary>
	Approximate,

	/// <summary>T-SQL: money and smallmoney (Transact-SQL). A number written with a currency sign: <c>$12.50</c>.</summary>
	Money,
}

/// <summary>The prefix a character string is written with.</summary>
public enum StringLiteralKind
{
	/// <summary>No prefix: <c>'abc'</c>.</summary>
	Character,

	/// <summary>The national prefix: <c>N'abc'</c>.</summary>
	National,

	/// <summary>The Unicode prefix, with escapes: <c>U&amp;'\0061'</c>.</summary>
	Unicode,
}

/// <summary>A truth value literal.</summary>
public enum BooleanLiteral
{
	/// <summary><c>TRUE</c>.</summary>
	True,

	/// <summary><c>FALSE</c>.</summary>
	False,

	/// <summary><c>UNKNOWN</c>.</summary>
	Unknown,
}

/// <summary>The key word a date or time literal starts with.</summary>
public enum DateTimeLiteralKind
{
	/// <summary><c>DATE '2024-01-31'</c>.</summary>
	Date,

	/// <summary><c>TIME '12:30:00'</c>.</summary>
	Time,

	/// <summary><c>TIMESTAMP '2024-01-31 12:30:00'</c>.</summary>
	Timestamp,
}

/// <summary>
/// BNF: <c>&lt;simple when clause&gt;</c>, <c>&lt;searched when clause&gt;</c>: one <c>WHEN .. THEN ..</c> branch of a
/// <c>CASE</c> expression.
/// </summary>
/// <param name="When">
/// What the branch tests. A simple <c>CASE</c> lists the values to compare with its operand, <c>WHEN 1, 2</c>;
/// a searched one holds a single condition.
/// </param>
/// <param name="Then">The result when the branch is taken.</param>
public sealed record CaseWhen(SqlList<Expression> When, Expression Then) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>An argument of a routine call: <c>x</c>, or <c>name =&gt; x</c>.</summary>
/// <param name="Value">The value passed.</param>
/// <param name="Name">The parameter's name in <c>name =&gt; x</c>, or null for an argument passed by position.</param>
/// <param name="NamedAssignment"><c>true</c> where the argument was written with <c>=&gt;</c>; set exactly when <paramref name="Name"/> is.</param>
public sealed record Argument(Expression Value, Identifier? Name = null, bool NamedAssignment = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>BNF: <c>&lt;filter clause&gt;</c>: <c>FILTER (WHERE condition)</c> after an aggregate.</summary>
/// <param name="Condition">The condition a row must meet to be aggregated.</param>
public sealed record FilterClause(Expression Condition) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>BNF: <c>&lt;within group specification&gt;</c>: <c>WITHIN GROUP (ORDER BY ..)</c> after an ordered-set aggregate.</summary>
/// <param name="OrderBy">The ordering the aggregate reads its rows in.</param>
public sealed record WithinGroupClause(OrderByClause OrderBy) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>What <c>OVER</c> names: a window defined elsewhere, or a window written in place.</summary>
public abstract record WindowReference : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected WindowReference()
	{
	}

	/// <summary>A window by name: <c>OVER w</c>, which a <c>WINDOW</c> clause defines.</summary>
	/// <param name="Name">The window's name.</param>
	public sealed record NameRef(Identifier Name) : WindowReference;

	/// <summary>A window written in place: <c>OVER (PARTITION BY a ORDER BY b)</c>.</summary>
	/// <param name="Value">The window's specification.</param>
	public sealed record Specification(WindowSpecification Value) : WindowReference;
}
/// <summary>
/// BNF: <c>&lt;window specification details&gt;</c>: what stands between the brackets of
/// <c>OVER (existing PARTITION BY .. ORDER BY .. frame)</c>.
/// </summary>
/// <param name="Existing">The window this one extends, or null where none was named.</param>
/// <param name="PartitionBy">The <c>PARTITION BY</c> expressions, empty where there is no such clause.</param>
/// <param name="OrderBy">The <c>ORDER BY</c> clause, or null where there is none.</param>
/// <param name="Frame">The frame, or null where none was written.</param>
public sealed record WindowSpecification(Identifier? Existing, SqlList<Expression> PartitionBy, OrderByClause? OrderBy, WindowFrame? Frame) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>BNF: <c>&lt;window definition&gt;</c>: one entry of a <c>WINDOW</c> clause, <c>w AS (..)</c>.</summary>
/// <param name="Name">The name the window is given.</param>
/// <param name="Specification">What the window is.</param>
public sealed record WindowDefinition(Identifier Name, WindowSpecification Specification) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>BNF: <c>&lt;window frame clause&gt;</c>: <c>ROWS BETWEEN 1 PRECEDING AND CURRENT ROW EXCLUDE TIES</c>.</summary>
/// <param name="Unit">What the bounds count: rows, a value range or peer groups.</param>
/// <param name="Extent">The bound, or the two bounds, of the frame.</param>
/// <param name="Exclusion">The <c>EXCLUDE</c> option, or null where none was written.</param>
/// <param name="Pattern">The row pattern measures and common syntax written with the frame, or null where there are none.</param>
public sealed record WindowFrame(WindowFrameUnit Unit, WindowFrameExtent Extent, WindowFrameExclusion? Exclusion, RowPatternClause? Pattern) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>What the bounds of a window frame count.</summary>
public enum WindowFrameUnit
{
	/// <summary><c>ROWS</c>: a number of rows.</summary>
	Rows,

	/// <summary><c>RANGE</c>: a difference in the sort key's value.</summary>
	Range,

	/// <summary><c>GROUPS</c>: a number of groups of peer rows.</summary>
	Groups,
}
/// <summary>BNF: <c>&lt;window frame extent&gt;</c>: where a window frame starts and ends.</summary>
public abstract record WindowFrameExtent : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected WindowFrameExtent()
	{
	}

	/// <summary>A frame given by its start alone: <c>ROWS 1 PRECEDING</c>, which ends at the current row.</summary>
	/// <param name="Bound">The start of the frame.</param>
	public sealed record Single(WindowFrameBound Bound) : WindowFrameExtent;

	/// <summary>A frame given by both ends: <c>BETWEEN 1 PRECEDING AND 1 FOLLOWING</c>.</summary>
	/// <param name="From">The start of the frame.</param>
	/// <param name="To">The end of the frame.</param>
	public sealed record Between(WindowFrameBound From, WindowFrameBound To) : WindowFrameExtent;
}
/// <summary>BNF: <c>&lt;window frame start&gt;</c>, <c>&lt;window frame bound&gt;</c>: one end of a window frame.</summary>
/// <param name="Kind">Where the end lies.</param>
/// <param name="Offset">How far from the current row, set for <see cref="WindowBoundKind.Preceding"/> and <see cref="WindowBoundKind.Following"/> and null for the other kinds.</param>
public sealed record WindowFrameBound(WindowBoundKind Kind, Expression? Offset = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Where one end of a window frame lies.</summary>
public enum WindowBoundKind
{
	/// <summary><c>UNBOUNDED PRECEDING</c>: the first row of the partition.</summary>
	UnboundedPreceding,

	/// <summary><c>n PRECEDING</c>: <c>n</c> before the current row.</summary>
	Preceding,

	/// <summary><c>CURRENT ROW</c>.</summary>
	CurrentRow,

	/// <summary><c>n FOLLOWING</c>: <c>n</c> after the current row.</summary>
	Following,

	/// <summary><c>UNBOUNDED FOLLOWING</c>: the last row of the partition.</summary>
	UnboundedFollowing,
}

/// <summary>Which rows a window frame leaves out after its bounds are applied.</summary>
public enum WindowFrameExclusion
{
	/// <summary><c>EXCLUDE CURRENT ROW</c>.</summary>
	CurrentRow,

	/// <summary><c>EXCLUDE GROUP</c>: the current row and its peers.</summary>
	Group,

	/// <summary><c>EXCLUDE TIES</c>: the peers of the current row, but not the row itself.</summary>
	Ties,

	/// <summary><c>EXCLUDE NO OTHERS</c>: nothing is left out.</summary>
	NoOthers,
}

/// <summary>BNF: <c>&lt;in predicate value&gt;</c>: what the right side of <c>IN</c> holds.</summary>
public abstract record InSource : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected InSource()
	{
	}

	/// <summary>A subquery: <c>IN (SELECT ..)</c>.</summary>
	/// <param name="Query">The subquery, without its brackets.</param>
	public sealed record Subquery(Query Query) : InSource;

	/// <summary>A list of values: <c>IN (1, 2, 3)</c>.</summary>
	/// <param name="Items">The values, in the order written.</param>
	public sealed record Values(SqlList<Expression> Items) : InSource;
}
/// <summary>One type of an <c>IS OF (..)</c> predicate: <c>t</c> or <c>ONLY t</c>.</summary>
/// <param name="TypeName">The type's name.</param>
/// <param name="Only"><c>true</c> for <c>ONLY t</c>, which matches that type and not its subtypes.</param>
public sealed record TypeTest(QualifiedName TypeName, bool Only) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>BNF: <c>&lt;period predicand&gt;</c>: a period in a period predicate.</summary>
public abstract record PeriodValue : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected PeriodValue()
	{
	}

	/// <summary>A period by name: the period of a table, <c>t.p</c>.</summary>
	/// <param name="Name">The period's name, with every part.</param>
	public sealed record Reference(QualifiedName Name) : PeriodValue;

	/// <summary>A period written out: <c>PERIOD (a, b)</c>.</summary>
	/// <param name="Start">The value the period starts at.</param>
	/// <param name="End">The value the period ends at.</param>
	public sealed record Range(Expression Start, Expression End) : PeriodValue;
}
/// <summary>The right side of a period predicate: a period, or a single point in time.</summary>
public abstract record PeriodRight : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected PeriodRight()
	{
	}

	/// <summary>A period: <c>t.p</c> or <c>PERIOD (a, b)</c>.</summary>
	/// <param name="Value">The period.</param>
	public sealed record Period(PeriodValue Value) : PeriodRight;

	/// <summary>A single value, which <c>CONTAINS</c> takes: <c>PERIOD (a, b) CONTAINS x</c>.</summary>
	/// <param name="Value">The datetime value.</param>
	public sealed record Point(Expression Value) : PeriodRight;
}
/// <summary>The operator of a period predicate.</summary>
public enum PeriodOperator
{
	/// <summary><c>OVERLAPS</c>: the periods share some time.</summary>
	Overlaps,

	/// <summary><c>EQUALS</c>: the periods start and end together.</summary>
	Equals,

	/// <summary><c>CONTAINS</c>: the left period holds the right one, or a point.</summary>
	Contains,

	/// <summary><c>PRECEDES</c>: the left period ends before the right one starts.</summary>
	Precedes,

	/// <summary><c>SUCCEEDS</c>: the left period starts after the right one ends.</summary>
	Succeeds,

	/// <summary><c>IMMEDIATELY PRECEDES</c>: the left period ends where the right one starts.</summary>
	ImmediatelyPrecedes,

	/// <summary><c>IMMEDIATELY SUCCEEDS</c>: the left period starts where the right one ends.</summary>
	ImmediatelySucceeds,
}

/// <summary>BNF: <c>&lt;data type&gt;</c> and all predefined/row/reference/collection/user-defined type rules.</summary>
public abstract partial record DataType : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected DataType()
	{
	}

	/// <summary>
	/// A character string type: <c>CHAR (10)</c>, <c>VARCHAR (20 CHARACTERS) CHARACTER SET utf8 COLLATE c</c>,
	/// <c>CLOB (2K)</c>. T-SQL: char and varchar, nchar and nvarchar (Transact-SQL).
	/// </summary>
	/// <param name="Kind">The key word or words the type is named by.</param>
	/// <param name="Length">The length in brackets, or null where none was written or <paramref name="LargeObject"/> or <paramref name="Max"/> holds it.</param>
	/// <param name="Unit">The unit of the length, <c>CHARACTERS</c> or <c>OCTETS</c>, or null where none was written.</param>
	/// <param name="CharacterSet">The <c>CHARACTER SET</c> clause, or null where there is none.</param>
	/// <param name="Collation">The <c>COLLATE</c> clause, or null where there is none.</param>
	/// <param name="LargeObject">The length of a large object type with its multiplier, <c>2K</c>, or null where none was written.</param>
	/// <param name="Max">T-SQL: <c>true</c> where the length was written as <c>MAX</c>, in which case <paramref name="Length"/> is null.</param>
	public sealed record Character(CharacterTypeKind Kind, int? Length = null, LengthUnit? Unit = null, CharacterSetName? CharacterSet = null, CollationName? Collation = null, LargeObjectSize? LargeObject = null, [property: Spelling] bool Max = false) : DataType;
	/// <summary>
	/// A binary string type: <c>BINARY (16)</c>, <c>BLOB (2K)</c>. T-SQL: binary and varbinary (Transact-SQL).
	/// </summary>
	/// <param name="Kind">The key word or words the type is named by.</param>
	/// <param name="Length">The length in brackets, or null where none was written or <paramref name="LargeObject"/> or <paramref name="Max"/> holds it.</param>
	/// <param name="LargeObject">The length of a large object type with its multiplier, <c>2K</c>, or null where none was written.</param>
	/// <param name="Max">T-SQL: <c>true</c> where the length was written as <c>MAX</c>, in which case <paramref name="Length"/> is null.</param>
	public sealed record Binary(BinaryTypeKind Kind, int? Length = null, LargeObjectSize? LargeObject = null, [property: Spelling] bool Max = false) : DataType;
	/// <summary>A number type: <c>INTEGER</c>, <c>DECIMAL (10, 2)</c>, <c>FLOAT (24)</c>.</summary>
	/// <param name="Kind">The key word or words the type is named by.</param>
	/// <param name="Precision">The precision in brackets, or null where none was written.</param>
	/// <param name="Scale">The scale after the precision, or null where none was written.</param>
	public sealed record Numeric(NumericTypeKind Kind, int? Precision = null, int? Scale = null) : DataType;

	/// <summary>The <c>BOOLEAN</c> type.</summary>
	public sealed record Boolean : DataType;

	/// <summary>A date or time type: <c>DATE</c>, <c>TIME (3) WITH TIME ZONE</c>, <c>TIMESTAMP (6)</c>.</summary>
	/// <param name="Kind">Which of the three.</param>
	/// <param name="Precision">The precision of the fractional seconds in brackets, or null where none was written.</param>
	/// <param name="TimeZone">The <c>WITH TIME ZONE</c> or <c>WITHOUT TIME ZONE</c> clause, or null where there is none.</param>
	public sealed record DateTime(DateTimeTypeKind Kind, int? Precision = null, TimeZoneMode? TimeZone = null) : DataType;

	/// <summary>An interval type: <c>INTERVAL DAY TO SECOND (3)</c>.</summary>
	/// <param name="Qualifier">The fields the interval counts.</param>
	public sealed record Interval(IntervalQualifier Qualifier) : DataType;

	/// <summary>A row type: <c>ROW (a INT, b VARCHAR (10))</c>.</summary>
	/// <param name="Fields">The fields, in the order written.</param>
	public sealed record Row(SqlList<FieldDefinition> Fields) : DataType;

	/// <summary>A reference type: <c>REF (t) SCOPE s</c>.</summary>
	/// <param name="ReferencedType">The structured type the reference points to.</param>
	/// <param name="Scope">The table the reference is scoped to, or null where there is no <c>SCOPE</c> clause.</param>
	public sealed record Reference(QualifiedName ReferencedType, QualifiedName? Scope = null) : DataType;

	/// <summary>An array type: <c>INT ARRAY</c>, <c>INT ARRAY [10]</c>.</summary>
	/// <param name="ElementType">The type of the elements.</param>
	/// <param name="MaximumCardinality">The most elements the array holds, written in brackets, or null where none was written.</param>
	/// <param name="Trigraphs">
	/// <c>true</c> where the brackets were written as the trigraphs <c>??(</c> and <c>??)</c>; it changes the text and not the meaning.
	/// </param>
	public sealed record Array(DataType ElementType, int? MaximumCardinality = null, bool Trigraphs = false) : DataType;

	/// <summary>A multiset type: <c>INT MULTISET</c>.</summary>
	/// <param name="ElementType">The type of the elements.</param>
	public sealed record Multiset(DataType ElementType) : DataType;

	/// <summary>A type of the user's, by its name: <c>dbo.mytype</c>.</summary>
	/// <param name="Name">The type's name, with every part.</param>
	public sealed partial record UserDefined(QualifiedName Name) : DataType;

	/// <summary>The <c>JSON</c> type.</summary>
	public sealed record Json : DataType;

	/// <summary>
	/// A domain used as a type, by its name. No rule of the standard grammar builds it, and the writer has no text for it: a
	/// domain's name read from text is a <see cref="UserDefined"/>.
	/// </summary>
	/// <param name="Name">The domain's name.</param>
	public sealed record Domain(QualifiedName Name) : DataType;

	/// <summary>The <c>DESCRIPTOR</c> type, of an argument that is a list of column names.</summary>
	public sealed record Descriptor : DataType;

	/// <summary>
	/// The type of a table passed to a polymorphic table function: <c>TABLE PASS THROUGH WITH ROW SEMANTICS PRUNE ON EMPTY</c>.
	/// </summary>
	/// <param name="PassThrough">The <c>PASS THROUGH</c> or <c>NO PASS THROUGH</c> option, or null where none was written.</param>
	/// <param name="Semantics">The <c>WITH ROW SEMANTICS</c> or <c>WITH SET SEMANTICS</c> option, or null where none was written.</param>
	/// <param name="Pruning">The <c>PRUNE ON EMPTY</c> or <c>KEEP ON EMPTY</c> option, or null where none was written.</param>
	public sealed record GenericTable(PassThroughMode? PassThrough, TableSemantics? Semantics, TablePruning? Pruning) : DataType;

	/// <summary>
	/// A type that belongs to a dialect and has no node of its own. The standard writer has no text for it; a writer of the
	/// dialect gives it one.
	/// </summary>
	/// <param name="Dialect">The dialect that defines the type.</param>
	/// <param name="Name">The type's name in that dialect.</param>
	/// <param name="Arguments">The type's arguments, nodes of that dialect, in the order written.</param>
	public sealed record Extension(string Dialect, string Name, SqlList<ISqlNode> Arguments) : DataType;
}

/// <summary>The spelling of a character string type's name.</summary>
public enum CharacterTypeKind
{
	/// <summary><c>CHARACTER</c>.</summary>
	Character,
	/// <summary><c>CHAR</c>.</summary>
	Char,
	/// <summary><c>CHARACTER VARYING</c>.</summary>
	CharacterVarying,
	/// <summary><c>CHAR VARYING</c>.</summary>
	CharVarying,
	/// <summary><c>VARCHAR</c>.</summary>
	Varchar,
	/// <summary><c>CHARACTER LARGE OBJECT</c>.</summary>
	CharacterLargeObject,
	/// <summary><c>CHAR LARGE OBJECT</c>.</summary>
	CharLargeObject,
	/// <summary><c>CLOB</c>.</summary>
	Clob,
	/// <summary><c>NATIONAL CHARACTER</c>.</summary>
	NationalCharacter,
	/// <summary><c>NATIONAL CHAR</c>.</summary>
	NationalChar,
	/// <summary><c>NCHAR</c>.</summary>
	Nchar,
	/// <summary><c>NATIONAL CHARACTER VARYING</c>.</summary>
	NationalCharacterVarying,
	/// <summary><c>NATIONAL CHAR VARYING</c>.</summary>
	NationalCharVarying,
	/// <summary><c>NCHAR VARYING</c>.</summary>
	NcharVarying,
	/// <summary><c>NATIONAL CHARACTER LARGE OBJECT</c>.</summary>
	NationalCharacterLargeObject,
	/// <summary><c>NCHAR LARGE OBJECT</c>.</summary>
	NcharLargeObject,
	/// <summary><c>NCLOB</c>.</summary>
	Nclob,

	/// <summary>T-SQL: nchar and nvarchar (Transact-SQL), a page syntax.md does not transcribe.</summary>
	Nvarchar,
}
/// <summary>The spelling of a binary string type's name.</summary>
public enum BinaryTypeKind
{
	/// <summary><c>BINARY</c>.</summary>
	Binary,
	/// <summary><c>BINARY VARYING</c>.</summary>
	BinaryVarying,
	/// <summary><c>VARBINARY</c>.</summary>
	Varbinary,
	/// <summary><c>BINARY LARGE OBJECT</c>.</summary>
	BinaryLargeObject,
	/// <summary><c>BLOB</c>.</summary>
	Blob,
}
/// <summary>The spelling of a numeric type's name.</summary>
public enum NumericTypeKind
{
	/// <summary><c>NUMERIC</c>.</summary>
	Numeric,
	/// <summary><c>DECIMAL</c>.</summary>
	Decimal,
	/// <summary><c>DEC</c>.</summary>
	Dec,
	/// <summary><c>SMALLINT</c>.</summary>
	SmallInt,
	/// <summary><c>INTEGER</c>.</summary>
	Integer,
	/// <summary><c>INT</c>.</summary>
	Int,
	/// <summary><c>BIGINT</c>.</summary>
	BigInt,
	/// <summary><c>FLOAT</c>.</summary>
	Float,
	/// <summary><c>REAL</c>.</summary>
	Real,
	/// <summary><c>DOUBLE PRECISION</c>.</summary>
	DoublePrecision,
	/// <summary><c>DECFLOAT</c>, the standard's decimal floating-point type.</summary>
	DecFloat,
}
/// <summary>Which datetime type a <see cref="DataType.DateTime"/> is.</summary>
public enum DateTimeTypeKind
{
	/// <summary><c>DATE</c>.</summary>
	Date,
	/// <summary><c>TIME</c>.</summary>
	Time,
	/// <summary><c>TIMESTAMP</c>.</summary>
	Timestamp,
}
/// <summary>Whether a <c>TIME</c> or <c>TIMESTAMP</c> type carries a time zone.</summary>
public enum TimeZoneMode
{
	/// <summary><c>WITH TIME ZONE</c>.</summary>
	With,
	/// <summary><c>WITHOUT TIME ZONE</c>.</summary>
	Without,
}
/// <summary>The unit a character string's length is counted in.</summary>
public enum LengthUnit
{
	/// <summary><c>CHARACTERS</c>.</summary>
	Characters,
	/// <summary><c>OCTETS</c>.</summary>
	Octets,
}
/// <summary>
/// A large object's length: <c>2K</c>, <c>10M</c>.
/// </summary>
/// <param name="Value">The number.</param>
/// <param name="Multiplier">The multiplier written against the number (<c>K</c>, <c>M</c>, <c>G</c>, <c>T</c> or <c>P</c>); null where none is written.</param>
public readonly record struct LargeObjectSize(long Value, char? Multiplier);
/// <summary>
/// A field of a row type: <c>a INT</c>.
/// </summary>
/// <param name="Name">The field's name.</param>
/// <param name="Type">The field's type.</param>
public sealed record FieldDefinition(Identifier Name, DataType Type) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// The fields an interval holds: <c>YEAR</c>, <c>DAY(3) TO SECOND(2)</c>, <c>SECOND(5, 3)</c>.
/// </summary>
/// <param name="Start">The first field, or the only one.</param>
/// <param name="LeadingPrecision">The precision in the brackets after the first field; null where none is written. For a lone <c>SECOND</c> it is the digits before the point.</param>
/// <param name="End">The field after <c>TO</c>; null where the interval has one field.</param>
/// <param name="FractionalPrecision">The digits after the point of the seconds: the second number of a lone <c>SECOND(p, f)</c>, or the bracket after <c>TO SECOND</c>; null where none is written.</param>
public sealed record IntervalQualifier(DateTimeField Start, int? LeadingPrecision = null, DateTimeField? End = null, int? FractionalPrecision = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>A field of a datetime or an interval, as an interval qualifier names it.</summary>
public enum DateTimeField
{
	/// <summary><c>YEAR</c>.</summary>
	Year,
	/// <summary><c>MONTH</c>.</summary>
	Month,
	/// <summary><c>DAY</c>.</summary>
	Day,
	/// <summary><c>HOUR</c>.</summary>
	Hour,
	/// <summary><c>MINUTE</c>.</summary>
	Minute,
	/// <summary><c>SECOND</c>.</summary>
	Second,
}

/// <summary>BNF: table element/column/period/constraint/LIKE/typed-table/as-subquery structures.</summary>
public abstract partial record TableContents : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected TableContents()
	{
	}

	/// <summary>
	/// A table defined by a list of elements: <c>(a INT, PRIMARY KEY (a))</c>.
	/// </summary>
	/// <param name="Items">The elements, in order.</param>
	public sealed record Elements(SqlList<TableElement> Items) : TableContents;
	/// <summary>
	/// A table of a structured type: <c>OF t UNDER s (REF IS id SYSTEM GENERATED, …)</c>.
	/// </summary>
	/// <param name="TypeName">The structured type.</param>
	/// <param name="Under">The supertable after <c>UNDER</c>; null where none is written.</param>
	/// <param name="Items">The elements in the brackets; empty where the brackets are omitted.</param>
	public sealed record Typed(QualifiedName TypeName, QualifiedName? Under, SqlList<TableElement> Items) : TableContents;
	/// <summary>
	/// A table made from a query: <c>(a, b) AS (SELECT …) WITH DATA</c>.
	/// </summary>
	/// <param name="Columns">The column names listed before <c>AS</c>; empty where none are.</param>
	/// <param name="Query">The query the table is made from.</param>
	/// <param name="Data">Whether the query's rows are kept. T-SQL: CREATE TABLE AS SELECT (Microsoft Fabric and Azure Synapse Analytics) writes no <c>WITH [NO] DATA</c>, so it is null there.</param>
	public sealed partial record AsQuery(SqlList<Identifier> Columns, Query Query, WithDataMode? Data) : TableContents;
}
/// <summary>Whether a table made from a query takes the query's rows.</summary>
public enum WithDataMode
{
	/// <summary><c>WITH DATA</c>: the table is filled with the query's rows.</summary>
	WithData,
	/// <summary><c>WITH NO DATA</c>: the table takes the query's columns and no rows.</summary>
	WithNoData,
}
/// <summary>An element in the list that defines a table: a column, a period, a constraint, a <c>LIKE</c> clause.</summary>
public abstract partial record TableElement : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected TableElement()
	{
	}

	/// <summary>
	/// A column.
	/// </summary>
	/// <param name="Definition">The column.</param>
	public sealed record Column(ColumnDefinition Definition) : TableElement;
	/// <summary>
	/// A period: <c>PERIOD FOR SYSTEM_TIME (a, b)</c>.
	/// </summary>
	/// <param name="Definition">The period.</param>
	public sealed record Period(PeriodDefinition Definition) : TableElement;
	/// <summary>
	/// A constraint written among the elements, over the table's columns.
	/// </summary>
	/// <param name="Value">The constraint.</param>
	public sealed record TableConstraint(Constraint Value) : TableElement;
	/// <summary>
	/// <c>LIKE t INCLUDING DEFAULTS</c>: the columns of another table.
	/// </summary>
	/// <param name="Table">The table copied from.</param>
	/// <param name="Options">What is copied and what is left out; empty where none are written.</param>
	public sealed record Like(QualifiedName Table, SqlList<LikeOption> Options) : TableElement;
	/// <summary>
	/// <c>REF IS id SYSTEM GENERATED</c>: the self-referencing column of a typed table.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Generation">How its values are made; null where none is written.</param>
	public sealed record SelfReference(Identifier Name, ReferenceGeneration? Generation) : TableElement;
	/// <summary>
	/// <c>c WITH OPTIONS SCOPE t DEFAULT 1 NOT NULL</c>: options for a column a typed table inherits from its type.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Options">The scope, default and constraints written.</param>
	public sealed record ColumnOptions(Identifier Name, ColumnOptionList Options) : TableElement;
}
/// <summary>
/// A column: <c>a INT NOT NULL DEFAULT 0</c>.
/// </summary>
/// <param name="Name">The column's name.</param>
/// <param name="Type">The column's type; null where none is written, as for a generated column.</param>
/// <param name="Generation">How the column's values are made: its default, identity or generation expression; null where none is written.</param>
/// <param name="Constraints">The constraints written on the column, in order; empty where none are.</param>
/// <param name="Collation">The <c>COLLATE</c> written after the constraints; null where none is.</param>
public sealed partial record ColumnDefinition(Identifier Name, DataType? Type, ColumnGeneration? Generation, SqlList<Constraint> Constraints, CollationName? Collation) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How the values of a column are made.</summary>
public abstract record ColumnGeneration : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ColumnGeneration()
	{
	}

	/// <summary>
	/// <c>DEFAULT value</c>.
	/// </summary>
	/// <param name="Value">The value a missing one is replaced with.</param>
	public sealed record Default(Expression Value) : ColumnGeneration;
	/// <summary>
	/// <c>GENERATED ALWAYS AS IDENTITY (START WITH 1)</c>: an identity column.
	/// </summary>
	/// <param name="Generation">Whether a value may be given.</param>
	/// <param name="Options">The sequence options in the brackets; empty where none are written.</param>
	public sealed record Identity(IdentityGeneration Generation, SqlList<SequenceOption> Options) : ColumnGeneration;
	/// <summary>
	/// <c>GENERATED ALWAYS AS (expression)</c>: a column computed from the row.
	/// </summary>
	/// <param name="Expression">The expression.</param>
	public sealed record Generated(Expression Expression) : ColumnGeneration;
	/// <summary><c>GENERATED ALWAYS AS ROW START</c>: the start of a row's period of system time.</summary>
	public sealed record RowStart : ColumnGeneration;
	/// <summary><c>GENERATED ALWAYS AS ROW END</c>: the end of a row's period of system time.</summary>
	public sealed record RowEnd : ColumnGeneration;
}
/// <summary>When a database generates the value of an identity column.</summary>
public enum IdentityGeneration
{
	/// <summary><c>GENERATED ALWAYS</c>: always; a value cannot be given.</summary>
	Always,
	/// <summary><c>GENERATED BY DEFAULT</c>: unless a value is given.</summary>
	ByDefault,
}
/// <summary>
/// The options of an inherited column: <c>SCOPE t DEFAULT 1 NOT NULL</c>.
/// </summary>
/// <param name="Scope">The table after <c>SCOPE</c>; null where none is written.</param>
/// <param name="Default">The default value; null where none is written.</param>
/// <param name="Constraints">The constraints, in order; empty where none are.</param>
public sealed record ColumnOptionList(QualifiedName? Scope, Expression? Default, SqlList<Constraint> Constraints) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>What a <c>LIKE</c> table element copies from the other table, or leaves out.</summary>
public enum LikeOption
{
	/// <summary><c>INCLUDING IDENTITY</c>.</summary>
	IncludingIdentity,
	/// <summary><c>EXCLUDING IDENTITY</c>.</summary>
	ExcludingIdentity,
	/// <summary><c>INCLUDING DEFAULTS</c>.</summary>
	IncludingDefaults,
	/// <summary><c>EXCLUDING DEFAULTS</c>.</summary>
	ExcludingDefaults,
	/// <summary><c>INCLUDING GENERATED</c>.</summary>
	IncludingGenerated,
	/// <summary><c>EXCLUDING GENERATED</c>.</summary>
	ExcludingGenerated,
}
/// <summary>How the values of a typed table's self-referencing column are generated.</summary>
public enum ReferenceGeneration
{
	/// <summary><c>SYSTEM GENERATED</c>.</summary>
	SystemGenerated,
	/// <summary><c>USER GENERATED</c>.</summary>
	UserGenerated,
	/// <summary><c>DERIVED</c>.</summary>
	Derived,
}

/// <summary>A constraint of a column or a table. One with no columns of its own is a column's.</summary>
public abstract partial record Constraint : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected Constraint()
	{
	}

	/// <summary>The name after <c>CONSTRAINT</c>; null where none is written.</summary>
	public QualifiedName? Name { get; init; }

	/// <summary>When the constraint is checked and whether it is enforced; null where none is written.</summary>
	public ConstraintCharacteristics? Characteristics { get; init; }

	/// <summary><c>NOT NULL</c>.</summary>
	public sealed record NotNull : Constraint;
	/// <summary>
	/// <c>UNIQUE</c>, <c>PRIMARY KEY</c> or <c>UNIQUE (VALUE)</c>.
	/// </summary>
	/// <param name="Kind">Which key it is.</param>
	/// <param name="Nulls"><c>NULLS DISTINCT</c> or <c>NULLS NOT DISTINCT</c> as written; null where neither is.</param>
	/// <param name="Columns">The columns in the brackets; empty on a column's own constraint.</param>
	/// <param name="WithoutOverlapsPeriod">The period written last in the brackets, <c>(a, p WITHOUT OVERLAPS)</c>; null where none is.</param>
	public sealed partial record Unique(UniqueKind Kind, NullDistinctness? Nulls, SqlList<Identifier> Columns, Identifier? WithoutOverlapsPeriod = null) : Constraint;
	/// <summary>
	/// <c>FOREIGN KEY (a) REFERENCES t (b)</c>.
	/// </summary>
	/// <param name="Columns">The referencing columns; empty on a column's own constraint.</param>
	/// <param name="ReferencingPeriod">The period written last in the brackets, <c>(a, PERIOD p)</c>; null where none is.</param>
	/// <param name="References">The referenced table and what a change to it does.</param>
	public sealed partial record ForeignKey(SqlList<Identifier> Columns, Identifier? ReferencingPeriod, ReferencesSpecification References) : Constraint;
	/// <summary>
	/// <c>CHECK (condition)</c>.
	/// </summary>
	/// <param name="Condition">The condition the rows satisfy.</param>
	public sealed partial record Check(Expression Condition) : Constraint;
}
/// <summary>Which key a <see cref="Constraint.Unique"/> constraint declares.</summary>
public enum UniqueKind
{
	/// <summary><c>UNIQUE</c>.</summary>
	Unique,
	/// <summary><c>PRIMARY KEY</c>.</summary>
	PrimaryKey,
	/// <summary><c>UNIQUE (VALUE)</c>, the standard's uniqueness of a whole row; it names no column.</summary>
	UniqueValue,
}
/// <summary>Whether null values count as equal under a uniqueness constraint.</summary>
public enum NullDistinctness
{
	/// <summary><c>NULLS DISTINCT</c>: nulls are distinct, so several may repeat.</summary>
	Distinct,
	/// <summary><c>NULLS NOT DISTINCT</c>: nulls are equal, so only one may stand.</summary>
	NotDistinct,
}
/// <summary>
/// <c>REFERENCES t (a) MATCH FULL ON UPDATE CASCADE ON DELETE SET NULL</c>.
/// </summary>
/// <param name="Table">The referenced table.</param>
/// <param name="Columns">The referenced columns; empty where none are written.</param>
/// <param name="Period">The period written last in the brackets, <c>(a, PERIOD p)</c>; null where none is.</param>
/// <param name="Match">The <c>MATCH</c> type; null where none is written.</param>
/// <param name="OnUpdate">The action of <c>ON UPDATE</c>; null where none is written.</param>
/// <param name="OnDelete">The action of <c>ON DELETE</c>; null where none is written.</param>
/// <param name="DeleteRuleFirst"><c>ON DELETE</c> is written before <c>ON UPDATE</c>.</param>
public sealed partial record ReferencesSpecification(QualifiedName Table, SqlList<Identifier> Columns, Identifier? Period, MatchType? Match, ReferentialAction? OnUpdate, ReferentialAction? OnDelete, bool DeleteRuleFirst = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How a foreign key matches a row with null columns against the referenced rows.</summary>
public enum MatchType
{
	/// <summary><c>MATCH FULL</c>.</summary>
	Full,
	/// <summary><c>MATCH PARTIAL</c>.</summary>
	Partial,
	/// <summary><c>MATCH SIMPLE</c>.</summary>
	Simple,
}
/// <summary>What a foreign key does to a row when the referenced row is updated or deleted.</summary>
public enum ReferentialAction
{
	/// <summary><c>CASCADE</c>: the change is carried to the row.</summary>
	Cascade,
	/// <summary><c>SET NULL</c>: the row's columns are set to null.</summary>
	SetNull,
	/// <summary><c>SET DEFAULT</c>: the row's columns are set to their defaults.</summary>
	SetDefault,
	/// <summary><c>RESTRICT</c>: the change is refused.</summary>
	Restrict,
	/// <summary><c>NO ACTION</c>: the change is refused where the constraint is checked.</summary>
	NoAction,
}
/// <summary>
/// How a constraint is checked: <c>DEFERRABLE INITIALLY DEFERRED NOT ENFORCED</c>.
/// </summary>
/// <param name="Timing"><c>INITIALLY DEFERRED</c> or <c>INITIALLY IMMEDIATE</c> as written; null where neither is.</param>
/// <param name="Deferrable"><c>DEFERRABLE</c> (true) or <c>NOT DEFERRABLE</c> (false); null where neither is written.</param>
/// <param name="Enforced"><c>ENFORCED</c> (true) or <c>NOT ENFORCED</c> (false); null where neither is written.</param>
/// <param name="DeferrableFirst"><c>DEFERRABLE INITIALLY DEFERRED</c> is written rather than <c>INITIALLY DEFERRED DEFERRABLE</c>.</param>
public sealed record ConstraintCharacteristics(ConstraintTiming? Timing, bool? Deferrable, bool? Enforced, bool DeferrableFirst = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>When a deferrable constraint is checked.</summary>
public enum ConstraintTiming
{
	/// <summary><c>INITIALLY DEFERRED</c>: at the end of the transaction, until set otherwise.</summary>
	InitiallyDeferred,
	/// <summary><c>INITIALLY IMMEDIATE</c>: at the end of each statement, until set otherwise.</summary>
	InitiallyImmediate,
	/// <summary><c>DEFERRED</c>, as <c>SET CONSTRAINTS</c> says: the constraints are checked at the end of the transaction.</summary>
	Deferred,
	/// <summary><c>IMMEDIATE</c>, as <c>SET CONSTRAINTS</c> says: the constraints are checked at the end of each statement.</summary>
	Immediate,
}

/// <summary>One action of an <c>ALTER TABLE</c> statement.</summary>
public abstract partial record AlterTableAction : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected AlterTableAction()
	{
	}

	/// <summary>
	/// <c>ADD [COLUMN] a INT</c>.
	/// </summary>
	/// <param name="Column">The column added.</param>
	/// <param name="ColumnKeyword"><c>COLUMN</c> is written.</param>
	public sealed record AddColumn(ColumnDefinition Column, bool ColumnKeyword = false) : AlterTableAction;
	/// <summary>
	/// <c>ALTER [COLUMN] a SET DEFAULT 0</c>.
	/// </summary>
	/// <param name="Column">The column changed.</param>
	/// <param name="Action">The change.</param>
	/// <param name="ColumnKeyword"><c>COLUMN</c> is written.</param>
	public sealed record AlterColumn(Identifier Column, AlterColumnAction Action, bool ColumnKeyword = false) : AlterTableAction;
	/// <summary>
	/// <c>DROP [COLUMN] a CASCADE</c>.
	/// </summary>
	/// <param name="Column">The column dropped.</param>
	/// <param name="Behavior">What happens to what depends on the column. T-SQL: ALTER TABLE (Transact-SQL) writes no behavior, so it is null there; the standard's parser always sets it.</param>
	/// <param name="ColumnKeyword"><c>COLUMN</c> is written.</param>
	public sealed partial record DropColumn(Identifier Column, DropBehavior? Behavior, bool ColumnKeyword = false) : AlterTableAction;
	/// <summary>
	/// <c>ADD CONSTRAINT c CHECK (…)</c>.
	/// </summary>
	/// <param name="Constraint">The constraint added.</param>
	public sealed record AddConstraint(Constraint Constraint) : AlterTableAction;
	/// <summary>
	/// <c>ALTER CONSTRAINT c [NOT] ENFORCED</c>.
	/// </summary>
	/// <param name="Name">The constraint changed.</param>
	/// <param name="Enforced"><c>ENFORCED</c> (true) or <c>NOT ENFORCED</c> (false).</param>
	public sealed record AlterConstraint(QualifiedName Name, bool Enforced) : AlterTableAction;
	/// <summary>
	/// <c>DROP CONSTRAINT c CASCADE</c>.
	/// </summary>
	/// <param name="Name">The constraint dropped.</param>
	/// <param name="Behavior">What happens to what depends on the constraint. T-SQL: ALTER TABLE (Transact-SQL) writes no behavior, so it is null there; the standard's parser always sets it.</param>
	public sealed partial record DropConstraint(QualifiedName Name, DropBehavior? Behavior) : AlterTableAction;
	/// <summary>
	/// <c>ADD PERIOD FOR p (a, b)</c>, with the columns it adds.
	/// </summary>
	/// <param name="Period">The period added.</param>
	/// <param name="AddedColumns">The <c>ADD COLUMN</c> parts written after the period; empty where none are.</param>
	public sealed record AddPeriod(PeriodDefinition Period, SqlList<AddColumn> AddedColumns) : AlterTableAction;
	/// <summary>
	/// <c>DROP PERIOD FOR p CASCADE</c>.
	/// </summary>
	/// <param name="Kind">Which period it is.</param>
	/// <param name="ApplicationName">The period's name; null for the system time, which has none.</param>
	/// <param name="Behavior">What happens to what depends on the period. T-SQL: ALTER TABLE (Transact-SQL). <c>DROP PERIOD FOR SYSTEM_TIME</c> writes no behavior, which the standard demands: the behavior is null there, and each writer refuses the form its own grammar does not read.</param>
	public sealed record DropPeriod(PeriodKind Kind, Identifier? ApplicationName, DropBehavior? Behavior) : AlterTableAction;
	/// <summary><c>ADD SYSTEM VERSIONING</c>.</summary>
	public sealed record AddSystemVersioning : AlterTableAction;
	/// <summary>
	/// <c>DROP SYSTEM VERSIONING CASCADE</c>.
	/// </summary>
	/// <param name="Behavior">What happens to what depends on the history.</param>
	public sealed record DropSystemVersioning(DropBehavior Behavior) : AlterTableAction;
	/// <summary>
	/// An action of one dialect that the shared tree has no node for.
	/// </summary>
	/// <param name="Dialect">The dialect that defines the action.</param>
	/// <param name="Kind">Which action it is, in the dialect's terms.</param>
	/// <param name="Parts">The nodes the action is made of, in order.</param>
	public sealed record Extension(string Dialect, string Kind, SqlList<ISqlNode> Parts) : AlterTableAction;
}
/// <summary>The change an <c>ALTER COLUMN</c> makes to a column.</summary>
public abstract partial record AlterColumnAction : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected AlterColumnAction()
	{
	}

	/// <summary>
	/// <c>SET DEFAULT value</c>.
	/// </summary>
	/// <param name="Value">The new default.</param>
	public sealed record SetDefault(Expression Value) : AlterColumnAction;
	/// <summary><c>DROP DEFAULT</c>.</summary>
	public sealed record DropDefault : AlterColumnAction;
	/// <summary><c>SET NOT NULL</c>.</summary>
	public sealed record SetNotNull : AlterColumnAction;
	/// <summary><c>DROP NOT NULL</c>.</summary>
	public sealed record DropNotNull : AlterColumnAction;
	/// <summary>
	/// <c>ADD SCOPE t</c>: the table a reference column is scoped to.
	/// </summary>
	/// <param name="Table">The table.</param>
	public sealed record AddScope(QualifiedName Table) : AlterColumnAction;
	/// <summary>
	/// <c>DROP SCOPE CASCADE</c>.
	/// </summary>
	/// <param name="Behavior">What happens to what depends on the scope.</param>
	public sealed record DropScope(DropBehavior Behavior) : AlterColumnAction;
	/// <summary>
	/// <c>SET DATA TYPE t</c>.
	/// </summary>
	/// <param name="Type">The new type.</param>
	public sealed partial record SetDataType(DataType Type) : AlterColumnAction;
	/// <summary>
	/// <c>SET GENERATED ALWAYS SET INCREMENT BY 2</c>: the identity's generation, with options changed after it.
	/// </summary>
	/// <param name="Generation">Whether a value may be given.</param>
	/// <param name="Options">The sequence options changed; empty where none are written.</param>
	public sealed record SetIdentityGeneration(IdentityGeneration Generation, SqlList<SequenceOption> Options) : AlterColumnAction;
	/// <summary>
	/// <c>SET INCREMENT BY 2 RESTART</c>: sequence options of an identity column, changed alone.
	/// </summary>
	/// <param name="Options">The sequence options changed.</param>
	public sealed record IdentityOptions(SqlList<SequenceOption> Options) : AlterColumnAction;
	/// <summary><c>DROP IDENTITY</c>.</summary>
	public sealed record DropIdentity : AlterColumnAction;
	/// <summary><c>DROP EXPRESSION</c>: a generated column becomes an ordinary one.</summary>
	public sealed record DropExpression : AlterColumnAction;
}
/// <summary>
/// A period: <c>PERIOD FOR SYSTEM_TIME (a, b)</c>, <c>PERIOD FOR p (a, b)</c>.
/// </summary>
/// <param name="Kind">Which period it is.</param>
/// <param name="ApplicationName">The period's name; null for the system time, which has none.</param>
/// <param name="BeginColumn">The column that holds the start.</param>
/// <param name="EndColumn">The column that holds the end.</param>
public sealed record PeriodDefinition(PeriodKind Kind, Identifier? ApplicationName, Identifier BeginColumn, Identifier EndColumn) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Which kind of period a period definition declares.</summary>
public enum PeriodKind
{
	/// <summary><c>SYSTEM_TIME</c>: the period of system versioning, which has no name of its own.</summary>
	SystemTime,
	/// <summary>An application-time period, <c>PERIOD FOR name</c>.</summary>
	ApplicationTime,
}
/// <summary>What a drop does to the objects that depend on the dropped one.</summary>
public enum DropBehavior
{
	/// <summary><c>CASCADE</c>: they are dropped too.</summary>
	Cascade,
	/// <summary><c>RESTRICT</c>: the drop is refused while they exist.</summary>
	Restrict,
}
/// <summary>The scope of a temporary table.</summary>
public enum TableScope
{
	/// <summary><c>GLOBAL TEMPORARY</c>.</summary>
	GlobalTemporary,
	/// <summary><c>LOCAL TEMPORARY</c>.</summary>
	LocalTemporary,
}
/// <summary>What happens to the rows of a temporary table when a transaction commits.</summary>
public enum TableCommitAction
{
	/// <summary><c>ON COMMIT PRESERVE ROWS</c>.</summary>
	Preserve,
	/// <summary><c>ON COMMIT DELETE ROWS</c>.</summary>
	Delete,
}

/// <summary>
/// BNF: <c>&lt;grouping element&gt;</c>. One item of a <c>GROUP BY</c> list: a column or expression, a
/// <c>ROLLUP</c>, <c>CUBE</c> or <c>GROUPING SETS</c> list, or the empty set <c>()</c>. Switch on the nested type.
/// </summary>
public abstract partial record GroupingElement : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected GroupingElement()
	{
	}

	/// <summary>
	/// A plain grouping: one expression, <c>a</c>, or a list in brackets, <c>(a, b)</c>.
	/// </summary>
	/// <param name="Expressions">The expressions grouped by: one where the item is written without brackets, the list inside the brackets otherwise.</param>
	/// <param name="Parenthesized">The list was written in brackets, <c>(a, b)</c>, so it is one grouping set and not several items; false for a bare expression.</param>
	public sealed partial record Ordinary(SqlList<Expression> Expressions, bool Parenthesized = false) : GroupingElement;
	/// <summary>
	/// <c>ROLLUP (a, (b, c))</c>: the grouping sets that drop the trailing items one by one, down to the empty set.
	/// </summary>
	/// <param name="Items">The items inside the brackets.</param>
	public sealed record Rollup(SqlList<GroupingElement> Items) : GroupingElement;
	/// <summary>
	/// <c>CUBE (a, b)</c>: every grouping set that can be made of the items.
	/// </summary>
	/// <param name="Items">The items inside the brackets.</param>
	public sealed record Cube(SqlList<GroupingElement> Items) : GroupingElement;
	/// <summary>
	/// <c>GROUPING SETS (a, (b, c), ROLLUP (d), ())</c>: the grouping sets written out, each of which may itself be a <c>ROLLUP</c>, <c>CUBE</c> or <c>GROUPING SETS</c>.
	/// </summary>
	/// <param name="Items">The sets inside the brackets.</param>
	public sealed record Sets(SqlList<GroupingElement> Items) : GroupingElement;
	/// <summary>
	/// The empty grouping set <c>()</c>: all rows as one group.
	/// </summary>
	public sealed record Empty : GroupingElement;
}

/// <summary>
/// BNF: <c>&lt;row pattern&gt;</c>. The regular expression of a <c>MATCH_RECOGNIZE</c> clause's <c>PATTERN (…)</c>,
/// over the names of its <c>DEFINE</c> variables. A language of its own, kept apart from the SQL expressions;
/// switch on the nested type.
/// </summary>
public abstract record RowPattern : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected RowPattern()
	{
	}

	/// <summary>
	/// A pattern variable: <c>A</c>, one row that meets its <c>DEFINE</c> condition.
	/// </summary>
	/// <param name="Name">The variable's name.</param>
	public sealed record Variable(Identifier Name) : RowPattern;
	/// <summary>
	/// <c>^</c>: the start of the partition.
	/// </summary>
	public sealed record StartAnchor : RowPattern;
	/// <summary>
	/// <c>$</c>: the end of the partition.
	/// </summary>
	public sealed record EndAnchor : RowPattern;
	/// <summary>
	/// Patterns one after another: <c>A B+ C</c>.
	/// </summary>
	/// <param name="Items">The patterns, in the order the rows must match them.</param>
	public sealed record Sequence(SqlList<RowPattern> Items) : RowPattern;
	/// <summary>
	/// Alternatives: <c>A | B</c>, the match takes one of them.
	/// </summary>
	/// <param name="Items">The alternatives, in the order written.</param>
	public sealed record Alternation(SqlList<RowPattern> Items) : RowPattern;
	/// <summary>
	/// A pattern repeated: <c>A+</c>, <c>(A B){2,3}?</c>.
	/// </summary>
	/// <param name="Pattern">The pattern repeated.</param>
	/// <param name="Quantifier">How many times, and whether the fewest is preferred.</param>
	public sealed record Quantified(RowPattern Pattern, RowPatternQuantifier Quantifier) : RowPattern;
	/// <summary>
	/// A pattern in brackets: <c>(A B)</c>, or the empty pattern <c>()</c>.
	/// </summary>
	/// <param name="Pattern">The pattern inside the brackets; null for <c>()</c>.</param>
	public sealed record Parenthesized(RowPattern? Pattern) : RowPattern;
	/// <summary>
	/// <c>{- A -}</c>: rows that match the pattern but are left out of the output of <c>ALL ROWS PER MATCH</c>.
	/// </summary>
	/// <param name="Pattern">The pattern between the markers.</param>
	public sealed record Excluded(RowPattern Pattern) : RowPattern;
	/// <summary>
	/// <c>PERMUTE (A, B, C)</c>: the patterns in any order, as the alternation of all their sequences.
	/// </summary>
	/// <param name="Items">The patterns to permute.</param>
	public sealed record Permute(SqlList<RowPattern> Items) : RowPattern;
}
/// <summary>
/// BNF: <c>&lt;row pattern quantifier&gt;</c>. How often a pattern repeats: <c>*</c>, <c>+</c>, <c>?</c>,
/// <c>{n}</c>, <c>{n,m}</c>, with a trailing <c>?</c> for the reluctant form.
/// </summary>
/// <param name="Kind">Which spelling it is.</param>
/// <param name="Min">The lower bound of <c>{n,m}</c> and the count of <c>{n}</c>; null where the kind has none, or the range leaves it open.</param>
/// <param name="Max">The upper bound of <c>{n,m}</c>; null where the kind has none, or the range leaves it open.</param>
/// <param name="Reluctant">A <c>?</c> follows the quantifier: the match takes as few repetitions as it can, not as many.</param>
public sealed record RowPatternQuantifier(RowPatternQuantifierKind Kind, int? Min = null, int? Max = null, bool Reluctant = false) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Which spelling a <see cref="RowPatternQuantifier"/> is.</summary>
public enum RowPatternQuantifierKind
{
	/// <summary><c>*</c>: zero or more times.</summary>
	ZeroOrMore,

	/// <summary><c>+</c>: one or more times.</summary>
	OneOrMore,

	/// <summary><c>?</c>: at most once.</summary>
	ZeroOrOne,

	/// <summary><c>{n,m}</c>: between <see cref="RowPatternQuantifier.Min"/> and <see cref="RowPatternQuantifier.Max"/> times.</summary>
	Range,

	/// <summary><c>{n}</c>: exactly <see cref="RowPatternQuantifier.Min"/> times.</summary>
	Exact,
}
/// <summary>
/// BNF: <c>&lt;row pattern recognition clause&gt;</c>. <c>MATCH_RECOGNIZE (…)</c> after a table: the rows of a
/// partition that match a pattern, and what is returned for each match.
/// </summary>
/// <param name="PartitionBy">The <c>PARTITION BY</c> expressions; empty where none was written.</param>
/// <param name="OrderBy">The <c>ORDER BY</c> of the rows within a partition; null where none was written.</param>
/// <param name="Measures">The <c>MEASURES</c>: the values computed for a match; empty where none was written.</param>
/// <param name="RowsPerMatch">The <c>ONE ROW PER MATCH</c> or <c>ALL ROWS PER MATCH</c> form; null where it was not written.</param>
/// <param name="AfterMatch">The <c>AFTER MATCH SKIP …</c> clause; null where it was not written.</param>
/// <param name="Initial">The <c>INITIAL</c> or <c>SEEK</c> keyword; null where it was not written.</param>
/// <param name="Pattern">The <c>PATTERN (…)</c>; null for the empty pattern.</param>
/// <param name="Subsets">The <c>SUBSET</c> unions of variables; empty where none was written.</param>
/// <param name="Definitions">The <c>DEFINE</c> conditions, one per variable.</param>
public sealed record RowPatternClause(SqlList<Expression> PartitionBy, OrderByClause? OrderBy, SqlList<RowPatternMeasure> Measures, RowsPerMatch? RowsPerMatch, RowPatternSkip? AfterMatch, RowPatternInitial? Initial, RowPattern? Pattern, SqlList<RowPatternSubset> Subsets, SqlList<RowPatternDefinition> Definitions) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// One of a <c>MEASURES</c> list: <c>LAST(A.price) AS last_price</c>.
/// </summary>
/// <param name="Expression">The value computed for a match.</param>
/// <param name="Name">The name the column gets.</param>
public sealed record RowPatternMeasure(Expression Expression, Identifier Name) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// One of a <c>SUBSET</c> list: <c>S = (A, B)</c>, a name for the rows of several variables together.
/// </summary>
/// <param name="Name">The name of the union.</param>
/// <param name="Variables">The variables it joins.</param>
public sealed record RowPatternSubset(Identifier Name, SqlList<Identifier> Variables) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// One of a <c>DEFINE</c> list: <c>A AS price &gt; 10</c>.
/// </summary>
/// <param name="Variable">The pattern variable defined.</param>
/// <param name="Condition">The condition a row meets to be one of the variable's rows.</param>
public sealed record RowPatternDefinition(Identifier Variable, Expression Condition) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How many rows a match returns, from <c>… ROW(S) PER MATCH</c>.</summary>
public enum RowsPerMatch
{
	/// <summary><c>ONE ROW PER MATCH</c>.</summary>
	One,

	/// <summary><c>ALL ROWS PER MATCH</c>.</summary>
	All,

	/// <summary><c>ALL ROWS PER MATCH SHOW EMPTY MATCHES</c>.</summary>
	AllShowEmpty,

	/// <summary><c>ALL ROWS PER MATCH OMIT EMPTY MATCHES</c>.</summary>
	AllOmitEmpty,

	/// <summary><c>ALL ROWS PER MATCH WITH UNMATCHED ROWS</c>.</summary>
	AllWithUnmatched,
}
/// <summary>Where the search for a match starts, from <c>INITIAL</c> and <c>SEEK</c>.</summary>
public enum RowPatternInitial
{
	/// <summary><c>INITIAL</c>: a match must start at the first row it is tried from.</summary>
	Initial,

	/// <summary><c>SEEK</c>: the search moves on to later rows to find a match.</summary>
	Seek,
}
/// <summary>
/// BNF: <c>&lt;row pattern skip to&gt;</c>. <c>AFTER MATCH SKIP …</c>: where the search for the next match starts.
/// </summary>
/// <param name="Kind">Which skip it is.</param>
/// <param name="Variable">The variable named by <c>SKIP TO FIRST</c>, <c>SKIP TO LAST</c> and <c>SKIP TO</c>; null for the other two.</param>
public sealed record RowPatternSkip(RowPatternSkipKind Kind, Identifier? Variable = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Which <c>AFTER MATCH SKIP</c> a <see cref="RowPatternSkip"/> is.</summary>
public enum RowPatternSkipKind
{
	/// <summary><c>SKIP TO NEXT ROW</c>.</summary>
	NextRow,

	/// <summary><c>SKIP PAST LAST ROW</c>.</summary>
	PastLastRow,

	/// <summary><c>SKIP TO FIRST</c> a variable.</summary>
	First,

	/// <summary><c>SKIP TO LAST</c> a variable.</summary>
	Last,

	/// <summary><c>SKIP TO</c> a variable: the last row of its rows, as <c>LAST</c>, the standard's plain form.</summary>
	Variable,
}

/// <summary>
/// BNF: <c>&lt;JSON path wff&gt;</c>. An expression of the SQL/JSON path language, the language of a JSON
/// path string and of a simplified accessor's subscripts: a separate language embedded in SQL, not the
/// SQL <see cref="Expression"/>. Switch on the nested type.
/// </summary>
public abstract record JsonPathExpression : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonPathExpression()
	{
	}

	/// <summary>
	/// A literal of the path language: <c>"a"</c>, <c>1.5</c>.
	/// </summary>
	/// <param name="Text">The literal as written.</param>
	/// <param name="Kind">What kind of literal it is.</param>
	public sealed record Literal(string Text, JsonPathLiteralKind Kind) : JsonPathExpression;
	/// <summary>
	/// A variable of the path language: <c>$</c>, <c>$name</c>, <c>@</c>, <c>last</c>.
	/// </summary>
	/// <param name="Kind">Which variable it is.</param>
	/// <param name="Name">The variable's name for a <see cref="JsonPathVariableKind.Named"/> one; null for the others.</param>
	public sealed record Variable(JsonPathVariableKind Kind, Identifier? Name = null) : JsonPathExpression;
	/// <summary>
	/// A sign before an operand: <c>-$.a</c>.
	/// </summary>
	/// <param name="Operator">The sign.</param>
	/// <param name="Operand">What it applies to.</param>
	public sealed record Unary(JsonPathUnaryOperator Operator, JsonPathExpression Operand) : JsonPathExpression;
	/// <summary>
	/// Arithmetic between two operands: <c>$.a + 1</c>. The grouping is the tree's.
	/// </summary>
	/// <param name="Left">The operand before the operator.</param>
	/// <param name="Operator">The operator.</param>
	/// <param name="Right">The operand after it.</param>
	public sealed record Binary(JsonPathExpression Left, JsonPathBinaryOperator Operator, JsonPathExpression Right) : JsonPathExpression;
	/// <summary>
	/// A step taken from a value: <c>$.a</c>, <c>$[*]</c>, <c>$.a ? (@ &gt; 1)</c>, <c>$.a.size()</c>.
	/// </summary>
	/// <param name="Target">The value the step starts from.</param>
	/// <param name="Accessor">The step.</param>
	public sealed record Access(JsonPathExpression Target, JsonPathAccessor Accessor) : JsonPathExpression;
	/// <summary>
	/// A predicate used as a value.
	/// </summary>
	/// <param name="Value">The predicate.</param>
	public sealed record Predicate(JsonPathPredicate Value) : JsonPathExpression;
	/// <summary>
	/// An expression in brackets: <c>($.a + 1) * 2</c>.
	/// </summary>
	/// <param name="Value">The expression inside the brackets.</param>
	public sealed record Parenthesized(JsonPathExpression Value) : JsonPathExpression;
}
/// <summary>What a <see cref="JsonPathExpression.Literal"/> is.</summary>
public enum JsonPathLiteralKind
{
	/// <summary>A string, in double quotes.</summary>
	String,

	/// <summary>A number.</summary>
	Numeric,

	/// <summary>Any other literal: <c>true</c>, <c>false</c>, <c>null</c>.</summary>
	Other,
}
/// <summary>Which variable a <see cref="JsonPathExpression.Variable"/> is.</summary>
public enum JsonPathVariableKind
{
	/// <summary><c>$</c>: the context item the path is applied to.</summary>
	Context,

	/// <summary><c>$name</c>: a variable passed in by <c>PASSING</c>.</summary>
	Named,

	/// <summary><c>@</c>: the item a filter is testing.</summary>
	Current,

	/// <summary><c>last</c>: the index of the last element of the array being subscripted.</summary>
	Last,
}
/// <summary>The sign of a <see cref="JsonPathExpression.Unary"/>.</summary>
public enum JsonPathUnaryOperator
{
	/// <summary><c>+</c>.</summary>
	Plus,

	/// <summary><c>-</c>.</summary>
	Minus,
}
/// <summary>The operator of a <see cref="JsonPathExpression.Binary"/>.</summary>
public enum JsonPathBinaryOperator
{
	/// <summary><c>+</c>.</summary>
	Add,

	/// <summary><c>-</c>.</summary>
	Subtract,

	/// <summary><c>*</c>.</summary>
	Multiply,

	/// <summary><c>/</c>.</summary>
	Divide,

	/// <summary><c>%</c>.</summary>
	Modulo,
}
/// <summary>
/// BNF: <c>&lt;JSON accessor op&gt;</c>. One step of a path taken from a value: a member, an array element,
/// a filter or an item method. Switch on the nested type.
/// </summary>
public abstract record JsonPathAccessor : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonPathAccessor()
	{
	}

	/// <summary>
	/// <c>.name</c> or <c>."name"</c>: the member of an object.
	/// </summary>
	/// <param name="Name">The member's name, without any quotes.</param>
	/// <param name="Quoted">The name was written in double quotes.</param>
	public sealed record Member(string Name, bool Quoted) : JsonPathAccessor;
	/// <summary>
	/// <c>.*</c>: every member of an object.
	/// </summary>
	public sealed record WildcardMember : JsonPathAccessor;
	/// <summary>
	/// <c>[1, 3 TO 5]</c>: elements of an array by index.
	/// </summary>
	/// <param name="Subscripts">The indexes and ranges of indexes between the brackets.</param>
	public sealed record Array(SqlList<JsonSubscript> Subscripts) : JsonPathAccessor;
	/// <summary>
	/// <c>[*]</c>: every element of an array.
	/// </summary>
	public sealed record WildcardArray : JsonPathAccessor;
	/// <summary>
	/// <c>? (@.a &gt; 1)</c>: keeps the items that meet a condition.
	/// </summary>
	/// <param name="Predicate">The condition in the brackets.</param>
	public sealed record Filter(JsonPathPredicate Predicate) : JsonPathAccessor;
	/// <summary>
	/// <c>.size()</c>: an item method called on the value.
	/// </summary>
	/// <param name="Value">The method.</param>
	public sealed record Method(JsonMethod Value) : JsonPathAccessor;
}
/// <summary>
/// One subscript of an array accessor: <c>1</c> or <c>2 TO 4</c>.
/// </summary>
/// <param name="From">The index, or the first index of a range.</param>
/// <param name="To">The last index of a range; null for a single index.</param>
public sealed record JsonSubscript(JsonPathExpression From, JsonPathExpression? To = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;JSON path predicate&gt;</c>. A condition of the path language, as in a filter: an existence
/// test, a comparison, a pattern match or a combination of them. Switch on the nested type.
/// </summary>
public abstract record JsonPathPredicate : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonPathPredicate()
	{
	}

	/// <summary>
	/// <c>EXISTS (path)</c>: the path yields at least one item.
	/// </summary>
	/// <param name="Expression">The path tested.</param>
	public sealed record Exists(JsonPathExpression Expression) : JsonPathPredicate;
	/// <summary>
	/// A comparison of two path values: <c>@.a &lt;&gt; 1</c>.
	/// </summary>
	/// <param name="Left">The value before the operator.</param>
	/// <param name="Operator">The comparison.</param>
	/// <param name="Right">The value after it.</param>
	public sealed record Comparison(JsonPathExpression Left, JsonPathComparisonOperator Operator, JsonPathExpression Right) : JsonPathPredicate;
	/// <summary>
	/// <c>value like_regex "pattern" flag "i"</c>: the value matches a regular expression.
	/// </summary>
	/// <param name="Value">The value tested.</param>
	/// <param name="Pattern">The regular expression.</param>
	/// <param name="Flags">The <c>flag</c> letters; null where there is no <c>flag</c> clause.</param>
	public sealed record LikeRegex(JsonPathExpression Value, string Pattern, string? Flags) : JsonPathPredicate;
	/// <summary>
	/// <c>value starts with initial</c>: the string value begins with another string.
	/// </summary>
	/// <param name="Value">The value tested.</param>
	/// <param name="Initial">The prefix.</param>
	public sealed record StartsWith(JsonPathExpression Value, JsonPathExpression Initial) : JsonPathPredicate;
	/// <summary>
	/// <c>(predicate) IS UNKNOWN</c>: the predicate is neither true nor false.
	/// </summary>
	/// <param name="Predicate">The predicate tested.</param>
	public sealed record IsUnknown(JsonPathPredicate Predicate) : JsonPathPredicate;
	/// <summary>
	/// <c>!(predicate)</c>: the predicate is false.
	/// </summary>
	/// <param name="Predicate">The predicate negated.</param>
	public sealed record Not(JsonPathPredicate Predicate) : JsonPathPredicate;
	/// <summary>
	/// <c>p &amp;&amp; q</c>: every predicate is true.
	/// </summary>
	/// <param name="Items">The predicates joined.</param>
	public sealed record And(SqlList<JsonPathPredicate> Items) : JsonPathPredicate;
	/// <summary>
	/// <c>p || q</c>: at least one predicate is true.
	/// </summary>
	/// <param name="Items">The predicates joined.</param>
	public sealed record Or(SqlList<JsonPathPredicate> Items) : JsonPathPredicate;
	/// <summary>
	/// A predicate in brackets: <c>(p || q) &amp;&amp; r</c>.
	/// </summary>
	/// <param name="Predicate">The predicate inside the brackets.</param>
	public sealed record Parenthesized(JsonPathPredicate Predicate) : JsonPathPredicate;
}
/// <summary>The operator of a <see cref="JsonPathPredicate.Comparison"/>.</summary>
public enum JsonPathComparisonOperator
{
	/// <summary><c>==</c>.</summary>
	Equal,

	/// <summary><c>&lt;&gt;</c>.</summary>
	NotEqual,

	/// <summary><c>!=</c>, the other spelling of not equal.</summary>
	NotEqualBang,

	/// <summary><c>&lt;</c>.</summary>
	Less,

	/// <summary><c>&gt;</c>.</summary>
	Greater,

	/// <summary><c>&lt;=</c>.</summary>
	LessOrEqual,

	/// <summary><c>&gt;=</c>.</summary>
	GreaterOrEqual,
}
/// <summary>
/// An item method of the path language: <c>size()</c>, <c>decimal(10, 2)</c>, <c>timestamp_tz(3)</c>.
/// </summary>
/// <param name="Kind">Which method it is.</param>
/// <param name="Precision">The first argument of <c>decimal</c> and the only one of the <c>time</c> and <c>timestamp</c> methods; null where none was written.</param>
/// <param name="Scale">The second argument of <c>decimal</c>; null where none was written.</param>
/// <param name="DateTimeTemplate">The template of <c>datetime</c>; null where none was written.</param>
public sealed record JsonMethod(JsonMethodKind Kind, int? Precision = null, int? Scale = null, string? DateTimeTemplate = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Which item method a <see cref="JsonMethod"/> is, by its name in the path language.</summary>
public enum JsonMethodKind
{
	/// <summary><c>type()</c>.</summary>
	Type,

	/// <summary><c>size()</c>.</summary>
	Size,

	/// <summary><c>double()</c>.</summary>
	Double,

	/// <summary><c>ceiling()</c>.</summary>
	Ceiling,

	/// <summary><c>floor()</c>.</summary>
	Floor,

	/// <summary><c>abs()</c>.</summary>
	Abs,

	/// <summary><c>datetime()</c>.</summary>
	DateTime,

	/// <summary><c>keyvalue()</c>.</summary>
	KeyValue,

	/// <summary><c>bigint()</c>.</summary>
	BigInt,

	/// <summary><c>boolean()</c>.</summary>
	Boolean,

	/// <summary><c>date()</c>.</summary>
	Date,

	/// <summary><c>decimal()</c>.</summary>
	Decimal,

	/// <summary><c>integer()</c>.</summary>
	Integer,

	/// <summary><c>number()</c>.</summary>
	Number,

	/// <summary><c>string()</c>.</summary>
	String,

	/// <summary><c>time()</c>.</summary>
	Time,

	/// <summary><c>time_tz()</c>.</summary>
	TimeTz,

	/// <summary><c>timestamp()</c>.</summary>
	Timestamp,

	/// <summary><c>timestamp_tz()</c>.</summary>
	TimestampTz,
}
/// <summary>
/// An SQL/JSON path in full: <c>strict $.a[*]</c>.
/// </summary>
/// <param name="Mode">The mode written before the expression.</param>
/// <param name="Expression">The path expression.</param>
public sealed record JsonPath(JsonPathMode Mode, JsonPathExpression Expression) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How a path treats a structural error, from the word before the path.</summary>
public enum JsonPathMode
{
	/// <summary><c>strict</c>: a structural error is raised.</summary>
	Strict,

	/// <summary><c>lax</c>: a structural error is suppressed where it can be, by adapting the value.</summary>
	Lax,
}

// --- JSON SQL structures ----------------------------------------------------
/// <summary>
/// BNF: <c>&lt;JSON API common syntax&gt;</c>. What the standard's JSON functions and <c>JSON_TABLE</c> begin with:
/// the context item, the path and its arguments, <c>x FORMAT JSON, '$.a' AS p PASSING 1 AS v</c>.
/// </summary>
/// <param name="Context">The context item, the JSON value the path is applied to.</param>
/// <param name="Path">
/// The path. The standard's is a character string literal, a value like any other; T-SQL takes any value there
/// (<c>JSON_VALUE (@j, @p)</c>). Null only for T-SQL's one-argument <c>JSON_QUERY (x)</c>.
/// </param>
/// <param name="PathName"><c>AS name</c> after the path: the name by which a nested path or a plan refers to it; null where none was written.</param>
/// <param name="Passing">The <c>PASSING</c> arguments, the values the path's named variables take; empty where none was written.</param>
/// <param name="ContextFormat">The context item's <c>FORMAT JSON</c>; null where none was written.</param>
public sealed record JsonApiCommon(Expression Context, Expression? Path, Identifier? PathName, SqlList<JsonArgument> Passing, JsonInputClause? ContextFormat = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// One of a <c>PASSING</c> list: <c>1 AS v</c>, a value bound to a variable of the path.
/// </summary>
/// <param name="Value">The value passed.</param>
/// <param name="Name">The name of the path variable that takes it, as <c>$name</c>.</param>
/// <param name="Format">The <c>FORMAT JSON</c> after the value; null where none was written.</param>
public sealed record JsonArgument(Expression Value, Identifier Name, JsonInputClause? Format = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;JSON input clause&gt;</c>. <c>FORMAT JSON [ENCODING UTF8]</c>: says that a value is JSON text.
/// </summary>
/// <param name="Representation">The representation named after <c>FORMAT</c>.</param>
public sealed record JsonInputClause(JsonRepresentation Representation) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;JSON representation&gt;</c>. <c>JSON</c>, or <c>JSON ENCODING UTF8</c>: the form a JSON value is held in.
/// </summary>
/// <param name="Encoding">The <c>ENCODING</c> written; null where none was.</param>
/// <param name="ImplementationDefined">The text of the standard's implementation-defined representation option in place of <c>JSON</c>; null for <c>JSON</c> itself.</param>
public sealed record JsonRepresentation(JsonEncoding? Encoding = null, string? ImplementationDefined = null) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The character encoding of a <see cref="JsonRepresentation"/>.</summary>
public enum JsonEncoding
{
	/// <summary><c>ENCODING UTF8</c>.</summary>
	Utf8,

	/// <summary><c>ENCODING UTF16</c>.</summary>
	Utf16,

	/// <summary><c>ENCODING UTF32</c>.</summary>
	Utf32,
}
/// <summary>The kind of JSON value an <c>IS JSON</c> predicate tests for.</summary>
public enum JsonPredicateType
{
	/// <summary><c>IS JSON VALUE</c>: any JSON value.</summary>
	Value,

	/// <summary><c>IS JSON ARRAY</c>.</summary>
	Array,

	/// <summary><c>IS JSON OBJECT</c>.</summary>
	Object,

	/// <summary><c>IS JSON SCALAR</c>: a number, a string, a boolean or <c>null</c>.</summary>
	Scalar,
}
/// <summary>Whether the keys of a JSON object must be unique, from <c>WITH UNIQUE KEYS</c> and its forms.</summary>
public enum JsonKeyUniqueness
{
	/// <summary><c>WITH UNIQUE KEYS</c>.</summary>
	WithUniqueKeys,

	/// <summary><c>WITH UNIQUE</c>, the same without the word <c>KEYS</c>.</summary>
	WithUnique,

	/// <summary><c>WITHOUT UNIQUE KEYS</c>.</summary>
	WithoutUniqueKeys,

	/// <summary><c>WITHOUT UNIQUE</c>, the same without the word <c>KEYS</c>.</summary>
	WithoutUnique,
}
/// <summary>What <c>JSON_EXISTS</c> returns on an error, from <c>… ON ERROR</c>.</summary>
public enum JsonExistsErrorBehavior
{
	/// <summary><c>TRUE ON ERROR</c>.</summary>
	True,

	/// <summary><c>FALSE ON ERROR</c>.</summary>
	False,

	/// <summary><c>UNKNOWN ON ERROR</c>.</summary>
	Unknown,

	/// <summary><c>ERROR ON ERROR</c>: the error is raised.</summary>
	Error,
}
/// <summary>
/// BNF: <c>&lt;JSON table&gt;</c>. <c>JSON_TABLE (value, path COLUMNS (…) PLAN (…) ERROR ON ERROR)</c>: a JSON
/// value read as the rows of a table.
/// </summary>
/// <param name="Common">The context item, the row path and its <c>PASSING</c> arguments.</param>
/// <param name="Columns">The <c>COLUMNS</c> list.</param>
/// <param name="Plan">The <c>PLAN</c> or <c>PLAN DEFAULT</c> clause; null where none was written.</param>
/// <param name="OnError">The <c>ERROR ON ERROR</c> or <c>EMPTY ON ERROR</c> clause; null where none was written.</param>
/// <param name="Primitive">The function was <c>JSON_TABLE_PRIMITIVE</c>, the form with a column list of its own and no plan, not <c>JSON_TABLE</c>.</param>
public sealed record JsonTableDefinition(JsonApiCommon Common, SqlList<JsonTableColumn> Columns, JsonTablePlan? Plan, JsonTableErrorBehavior? OnError, bool Primitive = false) : ISqlNode {
	SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;JSON table column definition&gt;</c>. One entry of a <c>JSON_TABLE</c>'s <c>COLUMNS</c> list: an
/// ordinality counter, a column of a value or of JSON text, or a nested path with columns of its own.
/// Switch on the nested type.
/// </summary>
public abstract record JsonTableColumn : ISqlNode
{
	SqlLocation _location;

	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonTableColumn()
	{
	}

	/// <summary>
	/// A <c>name FOR ORDINALITY</c> column of <c>JSON_TABLE</c>: the number of the row among the rows the path produced.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	public sealed record Ordinality(Identifier Name) : JsonTableColumn;
	/// <summary>
	/// A column of <c>JSON_TABLE</c> that holds one scalar picked out by a path: <c>name type [PATH 'p'] [v ON EMPTY] [v ON ERROR]</c>.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Type">The data type the value is returned as.</param>
	/// <param name="Path">The path literal after <c>PATH</c>, as written; null where there is no <c>PATH</c> and the column's name is the path.</param>
	/// <param name="OnEmpty">The <c>ON EMPTY</c> behavior, null where none was written.</param>
	/// <param name="OnError">The <c>ON ERROR</c> behavior, null where none was written.</param>
	public sealed record Regular(Identifier Name, DataType Type, string? Path, JsonValueBehavior? OnEmpty, JsonValueBehavior? OnError) : JsonTableColumn;
	/// <summary>
	/// A column of <c>JSON_TABLE</c> that holds a JSON fragment: <c>name type [FORMAT JSON] [PATH 'p'] [wrapper WRAPPER] [quotes] [b ON EMPTY] [b ON ERROR]</c>.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Type">The data type the fragment is returned as.</param>
	/// <param name="Format">The <c>FORMAT</c> clause with its encoding, null where none was written.</param>
	/// <param name="Path">The path literal after <c>PATH</c>, as written; null where there is no <c>PATH</c>.</param>
	/// <param name="Wrapper">The array wrapper behavior, null where no <c>WRAPPER</c> clause was written.</param>
	/// <param name="Quotes">The <c>KEEP QUOTES</c> or <c>OMIT QUOTES</c> clause, null where none was written.</param>
	/// <param name="OnEmpty">The <c>ON EMPTY</c> behavior, null where none was written.</param>
	/// <param name="OnError">The <c>ON ERROR</c> behavior, null where none was written.</param>
	public sealed record Formatted(Identifier Name, DataType Type, JsonRepresentation? Format, string? Path, JsonWrapperBehavior? Wrapper, JsonQuotes? Quotes, JsonQueryBehavior? OnEmpty, JsonQueryBehavior? OnError) : JsonTableColumn;
	/// <summary>
	/// A <c>NESTED [PATH] 'p' [AS name] COLUMNS (...)</c> group: columns taken from the items an inner path selects, one row for each.
	/// </summary>
	/// <param name="Path">The path literal, as written.</param>
	/// <param name="Name">The path name after <c>AS</c>, which a plan can refer to; null where none was written.</param>
	/// <param name="Columns">The columns of the group, which may nest further.</param>
	/// <param name="PathKeyword"><c>PATH</c> was written after <c>NESTED</c>.</param>
	public sealed record Nested(string Path, Identifier? Name, SqlList<JsonTableColumn> Columns, bool PathKeyword = false) : JsonTableColumn;
	/// <summary>
	/// A <c>name FOR CHAINING</c> column, a form of the <c>JSON_TABLE_PRIMITIVE</c> column list that stands beside <c>FOR ORDINALITY</c>.
	/// </summary>
	/// <param name="Name">The column's name.</param>
	public sealed record Chaining(Identifier Name) : JsonTableColumn;
}
/// <summary>
/// BNF: <c>&lt;JSON table plan&gt;</c>. How the rows of the nested paths of a <c>JSON_TABLE</c> are joined to those of their parents: the text inside <c>PLAN (...)</c>.
/// </summary>
public abstract record JsonTablePlan : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonTablePlan()
	{
	}

	/// <summary>A plan that is a path name alone, as given by <c>AS name</c>.</summary>
	/// <param name="Value">The path name.</param>
	public sealed record Name(Identifier Value) : JsonTablePlan;
	/// <summary>
	/// A <c>parent OUTER child</c> plan: every row of the parent path is kept, and gets null columns where the child path found nothing.
	/// </summary>
	/// <param name="Parent">The path name of the parent.</param>
	/// <param name="Child">The plan of the nested paths.</param>
	public sealed record Outer(Identifier Parent, JsonTablePlan Child) : JsonTablePlan;
	/// <summary>
	/// A <c>parent INNER child</c> plan: only the rows of the parent path the child path found something for are kept.
	/// </summary>
	/// <param name="Parent">The path name of the parent.</param>
	/// <param name="Child">The plan of the nested paths.</param>
	public sealed record Inner(Identifier Parent, JsonTablePlan Child) : JsonTablePlan;
	/// <summary>A <c>a UNION b</c> plan: the rows of sibling paths follow one another.</summary>
	/// <param name="Items">The sibling plans, in the order they were written.</param>
	public sealed record Union(SqlList<JsonTablePlan> Items) : JsonTablePlan;
	/// <summary>A <c>a CROSS b</c> plan: every row of one sibling path is paired with every row of the others.</summary>
	/// <param name="Items">The sibling plans, in the order they were written.</param>
	public sealed record Cross(SqlList<JsonTablePlan> Items) : JsonTablePlan;
	/// <summary>
	/// A <c>PLAN DEFAULT (INNER, UNION)</c> plan: one join choice for every nested path instead of a plan written out.
	/// </summary>
	/// <param name="InnerOuter">The <c>INNER</c> or <c>OUTER</c> choice, null where it was left out.</param>
	/// <param name="UnionCross">The <c>UNION</c> or <c>CROSS</c> choice, null where it was left out.</param>
	/// <param name="UnionCrossFirst">The <c>UNION</c> or <c>CROSS</c> choice was written before the <c>INNER</c> or <c>OUTER</c> one.</param>
	public sealed record Default(JsonTableDefaultInnerOuter? InnerOuter, JsonTableDefaultUnionCross? UnionCross, bool UnionCrossFirst = false) : JsonTablePlan;
	/// <summary>A plan in brackets.</summary>
	/// <param name="Value">The plan inside the brackets.</param>
	public sealed record Parenthesized(JsonTablePlan Value) : JsonTablePlan;
}
/// <summary>The join <c>PLAN DEFAULT</c> uses between a path and its nested paths.</summary>
public enum JsonTableDefaultInnerOuter
{
	/// <summary><c>INNER</c>: a row of the parent path is kept only if the nested path found something for it.</summary>
	Inner,
	/// <summary>
	/// <c>OUTER</c>: a row of the parent path is kept, with null columns where the nested path found nothing.
	/// </summary>
	Outer,
}
/// <summary>The combination <c>PLAN DEFAULT</c> uses between sibling nested paths.</summary>
public enum JsonTableDefaultUnionCross
{
	/// <summary><c>UNION</c>: the rows of one sibling follow those of the other.</summary>
	Union,
	/// <summary><c>CROSS</c>: every row of one sibling is paired with every row of the other.</summary>
	Cross,
}
/// <summary>The trailing <c>ON ERROR</c> behavior of a <c>JSON_TABLE</c>.</summary>
public enum JsonTableErrorBehavior
{
	/// <summary><c>ERROR ON ERROR</c>: an error is raised.</summary>
	Error,
	/// <summary><c>EMPTY ON ERROR</c>: the result is an empty table.</summary>
	Empty,
}
/// <summary>
/// BNF: <c>&lt;JSON value empty/error behavior&gt;</c>: what <c>JSON_VALUE</c> or a <c>JSON_TABLE</c> column returns
/// where the path finds nothing or fails, the words before <c>ON EMPTY</c> or <c>ON ERROR</c>.
/// </summary>
public abstract record JsonValueBehavior : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected JsonValueBehavior()
	{
	}

	/// <summary><c>ERROR</c>: an error is raised.</summary>
	public sealed record Error : JsonValueBehavior;

	/// <summary><c>NULL</c>: the result is null.</summary>
	public sealed record Null : JsonValueBehavior;

	/// <summary><c>DEFAULT v</c>: the result is the value of an expression.</summary>
	/// <param name="Value">The expression after <c>DEFAULT</c>.</param>
	public sealed record Default(Expression Value) : JsonValueBehavior;
}
/// <summary>
/// The <c>WRAPPER</c> clause of <c>JSON_QUERY</c> or a <c>JSON_TABLE</c> column: whether the items found are wrapped in an array.
/// </summary>
public enum JsonWrapperBehavior
{
	/// <summary><c>WITHOUT WRAPPER</c>.</summary>
	Without,
	/// <summary><c>WITHOUT ARRAY WRAPPER</c>.</summary>
	WithoutArray,
	/// <summary><c>WITH WRAPPER</c>.</summary>
	With,
	/// <summary><c>WITH CONDITIONAL WRAPPER</c>: wrapped unless the path found a single array or object.</summary>
	WithConditional,
	/// <summary><c>WITH UNCONDITIONAL WRAPPER</c>: always wrapped.</summary>
	WithUnconditional,
	/// <summary><c>WITH ARRAY WRAPPER</c>.</summary>
	WithArray,
	/// <summary><c>WITH CONDITIONAL ARRAY WRAPPER</c>.</summary>
	WithConditionalArray,
	/// <summary><c>WITH UNCONDITIONAL ARRAY WRAPPER</c>.</summary>
	WithUnconditionalArray,
}
/// <summary>Whether the quotes around a scalar string are kept in the result of <c>JSON_QUERY</c>.</summary>
public enum JsonQuotesBehavior
{
	/// <summary><c>KEEP QUOTES</c>.</summary>
	Keep,
	/// <summary><c>OMIT QUOTES</c>.</summary>
	Omit,
}
/// <summary>
/// What <c>JSON_QUERY</c> or a <c>JSON_TABLE</c> column returns where the path finds nothing or fails: the word before <c>ON EMPTY</c> or <c>ON ERROR</c>.
/// </summary>
public enum JsonQueryBehavior
{
	/// <summary><c>ERROR</c>: an error is raised.</summary>
	Error,
	/// <summary><c>NULL</c>.</summary>
	Null,
	/// <summary><c>EMPTY ARRAY</c>: <c>[]</c>.</summary>
	EmptyArray,
	/// <summary><c>EMPTY OBJECT</c>: <c>{}</c>.</summary>
	EmptyObject,
}

// BNF: <JSON output clause>: `RETURNING t FORMAT JSON ENCODING UTF8`.
/// <summary>
/// BNF: <c>&lt;JSON output clause&gt;</c>: <c>RETURNING t FORMAT JSON ENCODING UTF8</c>, the type a JSON function returns.
/// </summary>
/// <param name="Type">The data type after <c>RETURNING</c>.</param>
/// <param name="Format">The <c>FORMAT</c> clause with its encoding, null where none was written.</param>
public sealed record JsonOutput(DataType Type, JsonRepresentation? Format = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// BNF: <JSON query quotes behavior>: `KEEP QUOTES ON SCALAR STRING`.
/// <summary>BNF: <c>&lt;JSON query quotes behavior&gt;</c>: <c>KEEP QUOTES ON SCALAR STRING</c>.</summary>
/// <param name="Behavior">Whether the quotes around a scalar string are kept or omitted.</param>
/// <param name="OnScalarString"><c>ON SCALAR STRING</c> was written.</param>
public sealed record JsonQuotes(JsonQuotesBehavior Behavior, bool OnScalarString) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// BNF: <JSON name and value>: `KEY k VALUE v`, `k VALUE v` or `k : v`, with the value's format.
/// <summary>
/// BNF: <c>&lt;JSON name and value&gt;</c>: <c>KEY k VALUE v</c>, <c>k VALUE v</c> or <c>k : v</c>, with the value's format. A member of <c>JSON_OBJECT</c> or <c>JSON_OBJECTAGG</c>.
/// </summary>
/// <param name="Key">The member's name.</param>
/// <param name="Value">The member's value.</param>
/// <param name="Syntax">The words that join the key and the value.</param>
/// <param name="Format">The <c>FORMAT</c> clause of the value, null where none was written.</param>
public sealed record JsonMember(Expression Key, Expression Value, JsonMemberSyntax Syntax, JsonInputClause? Format = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The words that join the key and the value of a <see cref="JsonMember"/>.</summary>
public enum JsonMemberSyntax
{
	/// <summary><c>KEY k VALUE v</c>.</summary>
	KeyValue,
	/// <summary><c>k VALUE v</c>.</summary>
	Value,
	/// <summary><c>k : v</c>.</summary>
	Colon,
}

// BNF: <JSON value expression> in an array constructor or aggregate, with its format.
/// <summary>
/// BNF: <c>&lt;JSON value expression&gt;</c> in an array constructor or aggregate, with its format: <c>v [FORMAT JSON]</c>.
/// </summary>
/// <param name="Value">The element's value.</param>
/// <param name="Format">The <c>FORMAT</c> clause of the value, null where none was written.</param>
public sealed record JsonElement(Expression Value, JsonInputClause? Format = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>BNF: <c>&lt;JSON constructor null clause&gt;</c>: what a JSON constructor does with a null value.</summary>
public enum JsonNullHandling
{
	/// <summary><c>NULL ON NULL</c>: the null is written as a JSON null.</summary>
	NullOnNull,
	/// <summary><c>ABSENT ON NULL</c>: the member or element is left out.</summary>
	AbsentOnNull,
}

// --- DML helper families ----------------------------------------------------
/// <summary>
/// BNF: <c>&lt;insert columns and source&gt;</c>. Where an <c>INSERT</c> takes its rows from: a query, a <c>VALUES</c> list, or <c>DEFAULT VALUES</c>.
/// </summary>
public abstract partial record InsertSource : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected InsertSource()
	{
	}

	/// <summary>A query as the source, <c>INSERT INTO t SELECT ...</c>.</summary>
	/// <param name="Query">The query whose rows are inserted.</param>
	public sealed record Subquery(Query Query) : InsertSource;
	/// <summary>A <c>VALUES (...), (...)</c> list as the source.</summary>
	/// <param name="Rows">The rows, each written in brackets.</param>
	public sealed record Values(SqlList<RowValue> Rows) : InsertSource;
	/// <summary>A <c>DEFAULT VALUES</c> source: one row in which every column takes its default.</summary>
	public sealed record DefaultValues : InsertSource;
}
/// <summary>One row of a <c>VALUES</c> list: <c>(1, DEFAULT)</c> or <c>ROW (1, DEFAULT)</c>.</summary>
/// <param name="Items">The values of the row, in column order.</param>
/// <param name="RowKeyword"><c>ROW</c> was written before the brackets.</param>
public sealed record RowValue(SqlList<Expression> Items, bool RowKeyword = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// The <c>OVERRIDING</c> clause of <c>INSERT</c> or <c>MERGE</c>: which values an identity column takes.
/// </summary>
public enum OverrideKind
{
	/// <summary><c>OVERRIDING USER VALUE</c>: the value supplied is ignored and the column generates one.</summary>
	UserValue,
	/// <summary><c>OVERRIDING SYSTEM VALUE</c>: the value supplied is used.</summary>
	SystemValue,
}
/// <summary>
/// BNF: <c>&lt;merge when clause&gt;</c>. One <c>WHEN [NOT] MATCHED [AND c] THEN ...</c> clause of a <c>MERGE</c> statement.
/// </summary>
public abstract partial record MergeClause : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected MergeClause()
	{
	}

	/// <summary>The condition after <c>AND</c> in the clause's head, <c>WHEN MATCHED AND c</c>; null where none was written.</summary>
	public Expression? Condition { get; init; }
	/// <summary>A <c>WHEN MATCHED</c> clause: what is done with a target row that a source row matched.</summary>
	/// <param name="Action">The action after <c>THEN</c>, an update or a delete.</param>
	public sealed record Matched(MergeMatchedAction Action) : MergeClause;
	/// <summary>A <c>WHEN NOT MATCHED</c> clause: the insert done for a source row that no target row matched.</summary>
	/// <param name="Action">The <c>INSERT</c> after <c>THEN</c>.</param>
	public sealed partial record NotMatched(MergeInsertAction Action) : MergeClause;
}
/// <summary>What a <c>MERGE</c> clause does to a target row it found: <c>UPDATE SET ...</c> or <c>DELETE</c>.</summary>
public abstract record MergeMatchedAction : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected MergeMatchedAction()
	{
	}

	/// <summary>An <c>UPDATE SET a = 1, b = 2</c> action.</summary>
	/// <param name="Assignments">The assignments of the <c>SET</c> list.</param>
	public sealed record Update(SqlList<Assignment> Assignments) : MergeMatchedAction;
	/// <summary>A <c>DELETE</c> action.</summary>
	public sealed record Delete : MergeMatchedAction;
}
/// <summary>
/// The <c>INSERT [(columns)] [OVERRIDING ...] VALUES (...)</c> of a <c>WHEN NOT MATCHED</c> clause of <c>MERGE</c>.
/// </summary>
/// <param name="Columns">The columns listed in brackets, empty where none were listed.</param>
/// <param name="Override">The <c>OVERRIDING</c> clause, null where none was written.</param>
/// <param name="Values">The values after <c>VALUES</c>, one for each listed column.</param>
public sealed partial record MergeInsertAction(SqlList<Identifier> Columns, OverrideKind? Override, SqlList<Expression> Values) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
// BNF: <set clause>, <multiple column assignment>. Parenthesized where the targets were written in brackets: `(a) = (1)` is not `a = (1)`.
/// <summary>
/// BNF: <c>&lt;set clause&gt;</c>, <c>&lt;multiple column assignment&gt;</c>. One <c>target = value</c> of the <c>SET</c> list of <c>UPDATE</c> or <c>MERGE</c>, or <c>(a, b) = (1, 2)</c>.
/// </summary>
/// <param name="Targets">The columns assigned, one unless several were written in brackets.</param>
/// <param name="Value">The new value; a row or a subquery where there are several targets.</param>
/// <param name="Parenthesized">The targets were written in brackets: <c>(a) = (1)</c> is not <c>a = (1)</c>.</param>
public sealed partial record Assignment(SqlList<AssignmentTarget> Targets, Expression Value, bool Parenthesized = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// BNF: <set target>, <update target>, <mutated set clause>. Trigraphs are `??(` and `??)` around the index.
/// <summary>
/// BNF: <c>&lt;set target&gt;</c>, <c>&lt;update target&gt;</c>, <c>&lt;mutated set clause&gt;</c>. The left side of an assignment: a column, an element of an array column, or an attribute of a structured one.
/// </summary>
/// <param name="Name">The column's name.</param>
/// <param name="Index">The array index in <c>[...]</c> after the name, null where none was written.</param>
/// <param name="MutationPath">The attribute names written after the column, <c>c.x.y</c>, null where none were written.</param>
/// <param name="Trigraphs">The index was written with the trigraphs <c>??(</c> and <c>??)</c> in place of the square brackets.</param>
public sealed record AssignmentTarget(QualifiedName Name, Expression? Index = null, SqlList<Identifier>? MutationPath = null, bool Trigraphs = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;update/delete target&gt;</c>'s <c>FOR PORTION OF period FROM a TO b</c>: the part of a row's application-time period an <c>UPDATE</c> or <c>DELETE</c> applies to.
/// </summary>
/// <param name="Name">The name of the period.</param>
/// <param name="From">The start of the portion.</param>
/// <param name="To">The end of the portion.</param>
public sealed record PeriodPortion(Identifier Name, Expression From, Expression To) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The identity clause of <c>TRUNCATE TABLE</c>.</summary>
public enum IdentityRestart
{
	/// <summary><c>CONTINUE IDENTITY</c>: identity columns go on from where they were.</summary>
	Continue,
	/// <summary><c>RESTART IDENTITY</c>: identity columns start again.</summary>
	Restart,
}

// --- Names and small value objects -----------------------------------------
//
// A value of two references: the first part, and the others, if any, in an array of their own, so a
// one-part name costs nothing beyond what holds it and its identifier. Its span is its parts' first to
// last; it stores none. default(QualifiedName) has no parts and is no name; the writer and the walker
// refuse it.

/// <summary>
/// A name of one part or several, <c>t</c>, <c>s.t</c>, <c>c.s.t</c>: a value of its first
/// <see cref="Identifier"/> and an array of the others, where there are any.
/// </summary>
/// <remarks>
/// <para>
/// A value and not a record, so that a name costs the record holding it no object of its own: a
/// one-part name is its identifier alone, and a longer one adds an array. A position that may be left
/// out is a <c>QualifiedName?</c>, null where nothing was written.
/// </para>
/// <para>
/// <c>default(QualifiedName)</c> has no parts and is no name: no parser builds one,
/// <see cref="SqlWriter"/> refuses to write one, and <see cref="SqlWalker"/> to walk one.
/// </para>
/// <para>
/// It stores no span of its own: <see cref="Span"/> runs from its first part's start to its last
/// part's end where both are known, and is unknown otherwise, and <see cref="Locate"/> keeps nothing.
/// Equality is its parts', which leave where they were written out of theirs.
/// </para>
/// </remarks>
public readonly record struct QualifiedName : ISqlNode
{
	readonly Identifier    _first;
	readonly Identifier[]? _rest;

	/// <summary>A one-part name.</summary>
	public QualifiedName(Identifier first)
	{
		_first = first ?? throw new ArgumentNullException(nameof(first));
	}

	/// <summary>
	/// A name of <paramref name="first"/> and the parts after it. The array is kept as it is, not
	/// copied: it is the name's from then on, and changing it changes the name.
	/// </summary>
	public QualifiedName(Identifier first, Identifier[]? rest)
	{
		_first = first ?? throw new ArgumentNullException(nameof(first));
		_rest  = rest is { Length: > 0 } ? rest : null;
	}

	/// <summary>A name of the parts of <paramref name="parts"/>, which has one at least.</summary>
	public QualifiedName(SqlList<Identifier> parts)
	{
		if (parts.IsEmpty)
			throw new ArgumentException("A name has a part at least.", nameof(parts));

		_first = parts[0];
		_rest  = parts.Length > 1 ? parts.AsSpan().Slice(1).ToArray() : null;
	}

	internal static QualifiedName Of(params Identifier[] parts)
	{
		return new(SqlList.From(parts));
	}

	/// <summary>How many parts the name has; none only for <c>default</c>.</summary>
	public int Count => _first is null ? 0 : 1 + (_rest?.Length ?? 0);

	/// <summary>The part at <paramref name="index"/>, the first at nought.</summary>
	public Identifier this[int index]
	{
		get
		{
			if (index == 0 && _first is not null)
				return _first;

			if (_rest is not null && (uint)(index - 1) < (uint)_rest.Length)
				return _rest[index - 1];

			throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	/// <summary>The first part: the catalog's or the schema's name where there are several, null only for <c>default</c>.</summary>
	public Identifier First => _first;

	/// <summary>The last part, the name itself; null only for <c>default</c>.</summary>
	public Identifier Last => _rest is { } rest ? rest[rest.Length - 1] : _first;

	/// <summary>
	/// The parts as a list, made on each call for a name of two parts or more. The indexer and
	/// <c>foreach</c> read them without making one.
	/// </summary>
	public SqlList<Identifier> Parts
	{
		get
		{
			if (_first is null)
				return default;

			var parts = new Identifier[1 + (_rest?.Length ?? 0)];

			parts[0] = _first;
			_rest?.CopyTo(parts, 1);

			return SqlList.Own(parts);
		}
	}

	/// <summary>
	/// From the first part's start to the last part's end, where a reading said where both were
	/// written; unknown otherwise, never a part of the extent.
	/// </summary>
	public SqlSpan Span
	{
		get
		{
			if (_first is null)
				return default;

			var first = _first.Span;

			if (_rest is null || !first.Known)
				return first;

			var last = _rest[_rest.Length - 1].Span;

			if (!last.Known || last.End < first.At)
				return default;

			return new SqlSpan(first.At, last.End - first.At);
		}
	}

	/// <summary>Keeps nothing: the span is the parts'.</summary>
	public void Locate(int at, int length)
	{
	}

	/// <summary>Whether both names have the same parts; where they were written is not compared.</summary>
	public bool Equals(QualifiedName other)
	{
		if (!EqualityComparer<Identifier>.Default.Equals(_first, other._first))
			return false;

		return new SqlList<Identifier>(_rest).Equals(new SqlList<Identifier>(other._rest));
	}

	/// <summary>A hash of the parts, which leaves their positions out as <see cref="Equals(QualifiedName)"/> does.</summary>
	public override int GetHashCode()
	{
		return SqlHash.Finish(SqlHash.Add(SqlHash.Add(SqlHash.Start(Count), _first?.GetHashCode() ?? 0), new SqlList<Identifier>(_rest).GetHashCode()));
	}

	bool PrintMembers(System.Text.StringBuilder builder)
	{
		builder.Append("Parts = ");
		builder.Append(Parts.ToString());
		builder.Append(", Span = ");
		builder.Append(Span.ToString());

		return true;
	}

	/// <summary>The enumerator <c>foreach</c> walks the parts with, the first part first.</summary>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>What <c>foreach</c> over a name walks with.</summary>
	public struct Enumerator
	{
		readonly QualifiedName _name;
		int                    _at;

		internal Enumerator(QualifiedName name)
		{
			_name = name;
			_at   = -1;
		}

		/// <summary>The part the enumeration stands at.</summary>
		public readonly Identifier Current => _name[_at];

		/// <summary>Moves to the next part; false after the last.</summary>
		public bool MoveNext()
		{
			return ++_at < _name.Count;
		}
	}
}
/// <summary>The name of a character set, <c>UTF8</c> or <c>s.latin1</c>, after <c>CHARACTER SET</c>.</summary>
/// <param name="Name">The name, with every part written.</param>
public sealed record CharacterSetName(QualifiedName Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The name of a collation, as it follows <c>COLLATE</c>.</summary>
/// <param name="Name">The name, with every part written.</param>
public sealed record CollationName(QualifiedName Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The owner named by <c>AUTHORIZATION</c> in <c>CREATE SCHEMA</c>.</summary>
/// <param name="Name">The owner's name.</param>
public sealed record AuthorizationIdentifier(Identifier Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>
/// BNF: <c>&lt;path specification&gt;</c>: <c>PATH s1, s2</c>, the schemas that unqualified routine names are looked up in.
/// </summary>
/// <param name="Schemas">The schemas, in the order they are searched.</param>
public sealed record PathSpecification(SqlList<QualifiedName> Schemas) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// --- Temporal/table source --------------------------------------------------
/// <summary>
/// BNF: <c>FOR SYSTEM_TIME ...</c>, the period of a system-versioned table whose rows a table reference asks for.
/// </summary>
public abstract partial record SystemTimeSpecification : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected SystemTimeSpecification()
	{
	}

	/// <summary>A <c>FOR SYSTEM_TIME AS OF p</c> specification: the rows as they were at one point in time.</summary>
	/// <param name="Point">The point in time.</param>
	public sealed record AsOf(Expression Point) : SystemTimeSpecification;
	/// <summary>
	/// A <c>FOR SYSTEM_TIME BETWEEN [ASYMMETRIC | SYMMETRIC] a AND b</c> specification: the rows that were current at some time in a range, whose end is included.
	/// </summary>
	/// <param name="From">The start of the range.</param>
	/// <param name="To">The end of the range.</param>
	/// <param name="Symmetry">The <c>ASYMMETRIC</c> or <c>SYMMETRIC</c> word, null where none was written.</param>
	public sealed record Between(Expression From, Expression To, BetweenSymmetry? Symmetry) : SystemTimeSpecification;
	/// <summary>
	/// A <c>FOR SYSTEM_TIME FROM a TO b</c> specification: the rows that were current at some time in a range, whose end is left out.
	/// </summary>
	/// <param name="From">The start of the range.</param>
	/// <param name="To">The end of the range.</param>
	public sealed record FromTo(Expression From, Expression To) : SystemTimeSpecification;
}

// --- ALTER DOMAIN -----------------------------------------------------------
/// <summary>BNF: <c>&lt;alter domain action&gt;</c>. What an <c>ALTER DOMAIN</c> statement does.</summary>
public abstract record AlterDomainAction : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected AlterDomainAction()
	{
	}

	/// <summary>An <c>ALTER DOMAIN d SET DEFAULT e</c> action.</summary>
	/// <param name="Value">The new default.</param>
	public sealed record SetDefault(Expression Value) : AlterDomainAction;
	/// <summary>An <c>ALTER DOMAIN d DROP DEFAULT</c> action.</summary>
	public sealed record DropDefault : AlterDomainAction;
	/// <summary>An <c>ALTER DOMAIN d ADD CONSTRAINT ...</c> action.</summary>
	/// <param name="Constraint">The constraint added.</param>
	public sealed record AddConstraint(Constraint Constraint) : AlterDomainAction;
	/// <summary>An <c>ALTER DOMAIN d DROP CONSTRAINT c</c> action.</summary>
	/// <param name="Name">The constraint's name.</param>
	public sealed record DropConstraint(QualifiedName Name) : AlterDomainAction;
}

// --- Views -----------------------------------------------------------------
/// <summary>
/// BNF: <c>&lt;view specification&gt;</c>. The part of <c>CREATE VIEW</c> between the name and <c>AS</c> that says what kind of view it is.
/// </summary>
public abstract record ViewSpecification : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ViewSpecification()
	{
	}

	/// <summary>A view of the ordinary kind, <c>CREATE VIEW v (a, b) AS ...</c>.</summary>
	/// <param name="Columns">The column names listed in brackets, empty where none were listed.</param>
	public sealed record Regular(SqlList<Identifier> Columns) : ViewSpecification;
	/// <summary>
	/// A typed view, <c>CREATE VIEW v OF t [UNDER u] (elements) AS ...</c>, whose rows are values of a structured type.
	/// </summary>
	/// <param name="TypeName">The structured type after <c>OF</c>.</param>
	/// <param name="Under">The view after <c>UNDER</c> this one is a subview of, null where none was written.</param>
	/// <param name="Elements">The elements in brackets, empty where there are none.</param>
	public sealed record Referenceable(QualifiedName TypeName, QualifiedName? Under, SqlList<ViewElement> Elements) : ViewSpecification;
}
/// <summary>
/// BNF: <c>&lt;view element&gt;</c>. One entry of the bracketed list of a typed view: its self-referencing column or an
/// option for a column.
/// </summary>
public abstract record ViewElement : ISqlNode
{
	SqlLocation _location;
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ViewElement()
	{
	}

	/// <summary>The <c>REF IS name [SYSTEM GENERATED | USER GENERATED | DERIVED]</c> entry: the column that holds each row's reference.</summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Generation">How the references are made, null where none was written.</param>
	public sealed record SelfReference(Identifier Name, ReferenceGeneration? Generation) : ViewElement;

	/// <summary>A <c>column WITH OPTIONS SCOPE t</c> entry: the table a reference column points into.</summary>
	/// <param name="Name">The column's name.</param>
	/// <param name="Scope">The table after <c>SCOPE</c>.</param>
	public sealed record ColumnOption(Identifier Name, QualifiedName Scope) : ViewElement;
}
/// <summary>The check option of <c>CREATE VIEW</c>: <c>WITH [CASCADED | LOCAL] CHECK OPTION</c>.</summary>
public enum CheckOption
{
	/// <summary>
	/// <c>WITH CASCADED CHECK OPTION</c>: rows written through the view must satisfy its condition and those of the views below it.
	/// </summary>
	Cascaded,
	/// <summary><c>WITH LOCAL CHECK OPTION</c>: rows written through the view must satisfy its own condition.</summary>
	Local,
	/// <summary><c>WITH CHECK OPTION</c>, with neither word; T-SQL's only form.</summary>
	Unqualified,
}

// --- Triggers ---------------------------------------------------------------
/// <summary>When a trigger fires relative to the statement that fires it.</summary>
public enum TriggerTime
{
	/// <summary><c>BEFORE</c>.</summary>
	Before,
	/// <summary><c>AFTER</c>; in T-SQL also <c>FOR</c>.</summary>
	After,
	/// <summary><c>INSTEAD OF</c>: the trigger runs in place of the statement.</summary>
	InsteadOf,
}
/// <summary>
/// BNF: <c>&lt;trigger event&gt;</c>: <c>INSERT</c>, <c>DELETE</c> or <c>UPDATE [OF a, b]</c>, the statement that fires a trigger.
/// </summary>
/// <param name="Kind">The kind of statement.</param>
/// <param name="Columns">The columns after <c>OF</c>, empty where none were listed.</param>
public sealed record TriggerEvent(TriggerEventKind Kind, SqlList<Identifier> Columns) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>The kind of statement that fires a trigger.</summary>
public enum TriggerEventKind
{
	/// <summary><c>INSERT</c>.</summary>
	Insert,
	/// <summary><c>DELETE</c>.</summary>
	Delete,
	/// <summary><c>UPDATE</c>.</summary>
	Update,
}
/// <summary>
/// BNF: <c>&lt;transition table or variable&gt;</c>: one entry of a trigger's <c>REFERENCING</c> list, <c>OLD ROW AS o</c> or <c>NEW TABLE n</c>.
/// </summary>
/// <param name="Kind">Which of the four transition values is named.</param>
/// <param name="Name">The correlation name the trigger body uses for it.</param>
/// <param name="RowKeyword"><c>ROW</c> was written after <c>OLD</c> or <c>NEW</c>.</param>
/// <param name="AsKeyword"><c>AS</c> was written before the name.</param>
public sealed record TransitionReference(TransitionKind Kind, Identifier Name, bool RowKeyword = false, bool AsKeyword = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>Which transition value of a trigger a <see cref="TransitionReference"/> names.</summary>
public enum TransitionKind
{
	/// <summary><c>OLD [ROW]</c>: the row as it was before the statement.</summary>
	OldRow,
	/// <summary><c>NEW [ROW]</c>: the row as it is after the statement.</summary>
	NewRow,
	/// <summary><c>OLD TABLE</c>: all affected rows as they were before.</summary>
	OldTable,
	/// <summary><c>NEW TABLE</c>: all affected rows as they are after.</summary>
	NewTable,
}
/// <summary>
/// BNF: <c>&lt;triggered action&gt;</c>: <c>[FOR EACH ROW | FOR EACH STATEMENT] [WHEN (c)] statement</c> or <c>BEGIN ATOMIC ... END</c>, what a trigger does when it fires.
/// </summary>
/// <param name="Granularity">Whether the action runs for each row or each statement, null where none was written.</param>
/// <param name="When">The condition in <c>WHEN (c)</c>, null where none was written.</param>
/// <param name="Statements">The statements of the action: those between <c>BEGIN ATOMIC</c> and <c>END</c>, or the one statement written alone.</param>
/// <param name="AtomicBlock">The statements were written as a <c>BEGIN ATOMIC ... END</c> block.</param>
public sealed record TriggerAction(TriggerGranularity? Granularity, Expression? When, SqlList<Statement> Statements, bool AtomicBlock = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType());
	/// <inheritdoc/>
	public SqlSpan Span => _location.Span;
	/// <inheritdoc/>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
/// <summary>How often a trigger's action runs.</summary>
public enum TriggerGranularity
{
	/// <summary><c>FOR EACH ROW</c>.</summary>
	Row,
	/// <summary><c>FOR EACH STATEMENT</c>.</summary>
	Statement,
}

// --- UDT/routines/transforms ------------------------------------------------
public sealed record UserDefinedTypeDefinition(QualifiedName Name, QualifiedName? Under, TypeRepresentation? Representation, SqlList<UserTypeOption> Options, SqlList<MethodSpecification> Methods) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public abstract record TypeRepresentation : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected TypeRepresentation() { } public sealed record Type(DataType Value) : TypeRepresentation; public sealed record Members(SqlList<AttributeDefinition> Attributes) : TypeRepresentation; /* T-SQL: CREATE TYPE (Transact-SQL). `CREATE TYPE t AS TABLE (...)`, which the standard has no representation for. */ public sealed record Table(SqlList<TableElement> Elements) : TypeRepresentation; }
public sealed record AttributeDefinition(Identifier Name, DataType Type, Expression? Default, CollationName? Collation) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public abstract record UserTypeOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected UserTypeOption()
	{
	}

	public sealed record Instantiable(bool Value) : UserTypeOption;
	public sealed record Final(bool Value) : UserTypeOption;
	public sealed record Reference(UserTypeReference Representation) : UserTypeOption;
	public sealed record Cast(UserTypeCastKind Kind, Identifier FunctionName) : UserTypeOption;
}
public abstract record UserTypeReference : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected UserTypeReference() { } public sealed record Using(DataType Type) : UserTypeReference; public sealed record FromAttributes(SqlList<Identifier> Attributes) : UserTypeReference; public sealed record SystemGenerated : UserTypeReference; }
public enum UserTypeCastKind { ToRef, ToType, ToDistinct, ToSource }
public abstract record AlterTypeAction : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected AlterTypeAction() { } public sealed record AddAttribute(AttributeDefinition Attribute) : AlterTypeAction; public sealed record DropAttribute(Identifier Name) : AlterTypeAction; public sealed record AddMethod(MethodSpecification Method, bool Overriding) : AlterTypeAction; public sealed record DropMethod(MethodDesignator Method) : AlterTypeAction; }
public sealed record MethodSpecification(MethodModifier? Modifier, Identifier Name, SqlList<ParameterDefinition> Parameters, ReturnsDefinition Returns, QualifiedName? SpecificName, bool SelfAsResult, bool SelfAsLocator, SqlList<RoutineCharacteristic> Characteristics, bool Overriding = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum MethodModifier { Instance, Static, Constructor }
public sealed record MethodDesignator(MethodModifier? Modifier, Identifier Name, SqlList<DataType> ParameterTypes) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// BNF: <schema procedure>, <schema function>, <method specification designator>. StaticDispatch is <dispatch clause>.
public sealed partial record RoutineDefinition(RoutineKind Kind, QualifiedName Name, SqlList<ParameterDefinition> Parameters, ReturnsDefinition? Returns, SqlList<RoutineCharacteristic> Characteristics, RoutineBody Body, MethodModifier? MethodModifier = null, QualifiedName? ForType = null, bool StaticDispatch = false, bool SpecificMethod = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum RoutineKind { Routine, Procedure, Function, Method }
// BNF: <SQL parameter declaration>. Locator is <locator indication>, `AS LOCATOR`.
// T-SQL: CREATE PROCEDURE, CREATE FUNCTION (Transact-SQL). `ReadOnly`, `Varying` and `Nullable` are T-SQL's;
// its `OUTPUT` is `ParameterMode.Out` and needs nothing of its own.
public sealed partial record ParameterDefinition(ParameterMode? Mode, Identifier? Name, DataType Type, bool Result, Expression? Default, bool Locator = false, bool ReadOnly = false, bool Varying = false, bool? Nullable = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum ParameterMode { In, Out, InOut }
// BNF: <returns clause>, <returns type>, <result cast>. `RETURNS TABLE` with no columns is TableKeyword alone.
public sealed partial record ReturnsDefinition(DataType? Type, SqlList<FieldDefinition>? TableColumns, bool OnlyPassThrough = false) : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	public bool TableKeyword { get; init; }
	public bool Locator { get; init; }
	public DataType? CastFrom { get; init; }
	public bool CastFromLocator { get; init; }
}
public abstract record RoutineCharacteristic : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected RoutineCharacteristic()
	{
	}

	public sealed record Language(string Name) : RoutineCharacteristic;
	public sealed record ParameterStyle(ParameterStyleKind Style) : RoutineCharacteristic;
	public sealed record Specific(QualifiedName Name) : RoutineCharacteristic;
	public sealed record Deterministic(bool Value) : RoutineCharacteristic;
	public sealed record DataAccess(SqlDataAccess Value) : RoutineCharacteristic;
	public sealed record NullCall(NullCallMode Value) : RoutineCharacteristic;
	public sealed record DynamicResultSets(int Maximum) : RoutineCharacteristic;
	public sealed record SavepointLevel(SavepointLevelKind Value) : RoutineCharacteristic;
	// BNF: `NAME <external routine name>`. Name is written as it stands, quotes included: `NAME x` and `NAME 'x'` differ.
	public sealed record ExternalName(string Name) : RoutineCharacteristic;
}
public enum ParameterStyleKind { Sql, General }
public enum SqlDataAccess { NoSql, ContainsSql, ReadsSqlData, ModifiesSqlData }
public enum NullCallMode { ReturnsNullOnNullInput, CalledOnNullInput }
public enum SavepointLevelKind { New, Old }
public abstract partial record RoutineBody : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected RoutineBody()
	{
	}

	// BNF: <SQL routine spec>: the rights it runs with and one <SQL procedure statement>.
	public sealed record Sql(Statement Statement, SqlSecurity? Security = null) : RoutineBody;
	// BNF: <external body reference>. Name is the <external routine name> as written, quotes included.
	public sealed record External(string? Name, ParameterStyleKind? ParameterStyle, TransformGroupSpecification? TransformGroup, ExternalSecurity? Security) : RoutineBody;
	public sealed record PolymorphicTableFunction(PolymorphicTableFunctionBody Body) : RoutineBody;
}
public enum SqlSecurity { Invoker, Definer }
public enum ExternalSecurity { Definer, Invoker, ImplementationDefined }
// PrivateParameters is null where no `PRIVATE` was written; PrivateDataKeyword is `PRIVATE DATA`.
public sealed record PolymorphicTableFunctionBody(SqlList<ParameterDefinition>? PrivateParameters, RoutineDesignator? Describe, RoutineDesignator? Start, RoutineDesignator Fulfill, RoutineDesignator? Finish, bool PrivateDataKeyword = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record RoutineDesignator(RoutineKind Kind, QualifiedName Name, SqlList<DataType>? ParameterTypes = null, QualifiedName? ForType = null, bool SpecificKeyword = false, MethodModifier? MethodModifier = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum AlterRoutineBehavior { Restrict }

public sealed record TranslationSource(QualifiedName? Existing, RoutineDesignator? Routine) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record OrderingDefinition(OrderingForm Form, OrderingCategory Category) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum OrderingForm { EqualsOnly, Full }
public abstract record OrderingCategory : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected OrderingCategory() { } public sealed record Relative(RoutineDesignator Function) : OrderingCategory; public sealed record Map(RoutineDesignator Function) : OrderingCategory; public sealed record State(QualifiedName? SpecificName) : OrderingCategory; }
public sealed record TransformGroup(Identifier Name, SqlList<TransformElement> Elements) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record TransformElement(TransformDirection Direction, RoutineDesignator Function) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum TransformDirection { ToSql, FromSql }
public sealed record TransformAlterGroup(Identifier Name, SqlList<TransformAlterAction> Actions) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record TransformAlterAction(bool Add, SqlList<TransformElement> Elements, SqlList<TransformDirection> DropKinds, DropBehavior? Behavior) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public abstract record TransformDropTarget : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected TransformDropTarget() { } public sealed record All : TransformDropTarget; public sealed record Group(Identifier Name) : TransformDropTarget; }
public sealed record TransformGroupSpecification(Identifier? SingleGroup, SqlList<TransformGroupForType> Groups) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
// BNF: <group specification>, a transform group for a type.
public sealed record TransformGroupForType(Identifier Group, QualifiedName Type) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// --- Sequences --------------------------------------------------------------
public abstract partial record SequenceOption : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected SequenceOption()
	{
	}

	public sealed record DataTypeOption(DataType Type) : SequenceOption;
	public sealed record Start(Expression Value) : SequenceOption;
	public sealed record Increment(Expression Value) : SequenceOption;
	public sealed record Max(Expression? Value, bool No = false) : SequenceOption;
	public sealed record Min(Expression? Value, bool No = false) : SequenceOption;
	public sealed record Cycle(bool Value) : SequenceOption;
	public sealed record Restart(Expression? Value) : SequenceOption;
}

// --- Privileges/roles -------------------------------------------------------
public abstract record GrantBody : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected GrantBody()
	{
	}

	// T-SQL: GRANT (Transact-SQL). `Object` is nullable because T-SQL's `ON` is optional — a server
	// permission is granted to a principal and names nothing — and `AsPrincipal` is its `AS p`, which is
	// not `GrantedBy`: that is the standard's `GRANTED BY { CURRENT_USER | CURRENT_ROLE }`, an enum of
	// two, where this is a principal's name.
	public sealed record Privileges(SqlList<Privilege> Items, PrivilegeObject? Object, SqlList<Grantee> Grantees, bool HierarchyOption, bool GrantOption, Grantor? GrantedBy, Identifier? AsPrincipal = null) : GrantBody;
	public sealed record Roles(SqlList<Identifier> Names, SqlList<Grantee> Grantees, bool AdminOption, Grantor? GrantedBy) : GrantBody;
}
public abstract record RevokeBody : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected RevokeBody()
	{
	}

	// T-SQL: REVOKE (Transact-SQL). `Object` and `AsPrincipal` are as GrantBody's. `Behavior` is nullable
	// because T-SQL writes CASCADE or nothing where the standard always writes one of the two words, and
	// `To` is the page's other spelling: it publishes `{ TO | FROM }` for the same clause, and FROM is
	// the standard's, so the flag marks the deviation and not the norm.
	public sealed record Privileges(RevokeOption? Option, SqlList<Privilege> Items, PrivilegeObject? Object, SqlList<Grantee> Grantees, Grantor? GrantedBy, DropBehavior? Behavior, Identifier? AsPrincipal = null, [property: Spelling] bool To = false) : RevokeBody;
	public sealed record Roles(bool AdminOptionFor, SqlList<Identifier> Names, SqlList<Grantee> Grantees, Grantor? GrantedBy, DropBehavior Behavior) : RevokeBody;
}
// BNF: <action>. PrivilegeKind is written by TransactSql/permissions.py, in SqlAst.Permissions.cs,
// because T-SQL's members are its permission catalogue (D148).
// T-SQL: GRANT (Transact-SQL). `Abbreviated` is the page's shorter word for the same permission: `ALL`
// for ALL PRIVILEGES and `EXEC` for EXECUTE. It has no text on any other kind, and the writer refuses
// it there.
public sealed record Privilege(PrivilegeKind Kind, SqlList<Identifier> Columns, SqlList<RoutineDesignator> Methods, [property: Spelling] bool Abbreviated = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
// What a privilege is ON: the standard's <object name>, or T-SQL's securable written with its class. Two
// members rather than one record with a nullable class, so that a class beside the standard's kind word,
// its TABLE or its routine -- a node with no text -- cannot be built (Igor, D148).
public abstract record PrivilegeObject : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected PrivilegeObject()
	{
	}

	// BNF: <object name>. TableKeyword says `ON TABLE t` rather than `ON t`.
	// T-SQL: GRANT Object Permissions (Transact-SQL). `ON t`, with no class written, which T-SQL reads as
	// OBJECT. `Columns` is the column list that page lets follow the SECURABLE — `ON t (a, b)` — and not
	// only a permission, which is `Privilege.Columns`. The engine reads both and objects afterwards where
	// both are written (1019), so the tree holds them apart.
	public sealed record Named(PrivilegeObjectKind Kind, QualifiedName Name, RoutineDesignator? Routine = null, bool TableKeyword = false, SqlList<Identifier>? Columns = null) : PrivilegeObject;

	// T-SQL: GRANT (Transact-SQL). `ON class::securable`, `SCHEMA::s`; `OBJECT::t` is the class written out.
	// The grammar reads a column list after any class, `ON SCHEMA::s (c1)`, which the engine objects to
	// afterwards (1019), so `Columns` is here too.
	public sealed record Classed(SecurableClass Class, QualifiedName Name, SqlList<Identifier>? Columns = null) : PrivilegeObject;
}
public enum PrivilegeObjectKind { Table, Domain, Collation, CharacterSet, Translation, Type, Sequence, Routine }
// T-SQL: GRANT (Transact-SQL). `Null` is the `NULL` the engine reads anywhere among the principals of GRANT,
// DENY and REVOKE (and refuses after AS), which the standard has no word for.
public abstract record Grantee : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected Grantee() { } public sealed record Public : Grantee; public sealed record Identifier(AuthorizationIdentifier Value) : Grantee; public sealed record Null : Grantee; }
public enum Grantor { CurrentUser, CurrentRole }
public enum RevokeOption { GrantOptionFor, HierarchyOptionFor }

// --- Cursors/dynamic SQL/descriptors ---------------------------------------
// BNF: <cursor name> (a <local qualified name>, `MODULE.c`), <extended cursor name>, and a dynamic cursor's <scope option>.
// Name is null where the cursor is named by an extended name alone, `GLOBAL :c`.
public sealed record CursorReference(QualifiedName? Name, Expression? ExtendedName = null, bool Ptf = false, bool Global = false, bool Local = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
// T-SQL: DECLARE CURSOR, SET @local_variable (Transact-SQL). The extended form's words after CURSOR are
// properties here rather than records of their own: `Scope` is `LOCAL | GLOBAL`, `Type` the cursor's kind
// (`STATIC | KEYSET | DYNAMIC | FAST_FORWARD`, a different axis from the standard's sensitivity, which is
// why STATIC is not Insensitive), `Concurrency` is `READ_ONLY | SCROLL_LOCKS | OPTIMISTIC`, and
// `TypeWarning` is `TYPE_WARNING`. `ForwardOnly` is the word `FORWARD_ONLY` written where the standard
// writes `NO SCROLL`: one meaning, two words, so Scrollability stays NoScroll and the flag says which
// was written.
public sealed record CursorProperties(CursorSensitivity? Sensitivity, CursorScrollability? Scrollability, CursorHoldability? Holdability, CursorReturnability? Returnability) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public CursorScope? Scope { get; init; }
	public CursorType? Type { get; init; }
	public CursorConcurrency? Concurrency { get; init; }
	public bool TypeWarning { get; init; }
	[Spelling] public bool ForwardOnly { get; init; }

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum CursorSensitivity { Sensitive, Insensitive, Asensitive }
public enum CursorScrollability { Scroll, NoScroll }
public enum CursorHoldability { WithHold, WithoutHold }
public enum CursorReturnability { WithReturn, WithoutReturn }
// T-SQL: DECLARE CURSOR (Transact-SQL). `UpdatabilityBeforeOptions` says which of the query's `OPTION (…)`
// and the cursor's `FOR UPDATE` was written first, which the engine reads either way round. It is a flag
// about two children of this node — the Select whose OPTION it is, and the updatability beside it — and not
// about a sibling's text, which is why it is allowed to live here.
public abstract record CursorSource : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected CursorSource() { } public sealed record Query(Statement.Select Select, UpdatabilityClause? Updatability, [property: Spelling] bool UpdatabilityBeforeOptions = false) : CursorSource; public sealed record Prepared(StatementReference Statement) : CursorSource; }
// T-SQL: DECLARE CURSOR (Transact-SQL). `Underscored` is `READ_ONLY`, which the ISO form of that page
// publishes where the standard and T-SQL's `SET @c = CURSOR` write `READ ONLY`.
public sealed record UpdatabilityClause(bool ReadOnly, SqlList<Identifier> UpdateColumns, [property: Spelling] bool Underscored = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record FetchOrientation(FetchOrientationKind Kind, Expression? Offset = null) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum FetchOrientationKind { Next, Prior, First, Last, Absolute, Relative }
public abstract record CursorAllocationSource : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected CursorAllocationSource() { } public sealed record Prepared(StatementReference Statement) : CursorAllocationSource; public sealed record Routine(RoutineDesignator Designator) : CursorAllocationSource; }
public sealed record StatementReference(Identifier? Name, Expression? ExtendedName = null, bool Global = false, bool Local = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record DescriptorReference(Identifier? Name, Expression? ExtendedName = null, bool Ptf = false, bool Global = false, bool Local = false) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public abstract record DescriptorGet : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DescriptorGet() { } public sealed record Header(SqlList<DescriptorRead> Items) : DescriptorGet; public sealed record Value(Expression Index, SqlList<DescriptorRead> Items) : DescriptorGet; }
public abstract record DescriptorSet : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DescriptorSet() { } public sealed record Header(SqlList<DescriptorWrite> Items) : DescriptorSet; public sealed record Value(Expression Index, SqlList<DescriptorWrite> Items) : DescriptorSet; }
public sealed record DescriptorRead(Expression Target, DescriptorItem Item) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record DescriptorWrite(DescriptorItem Item, Expression Value) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum DescriptorItem { Count, KeyType, DynamicFunction, DynamicFunctionCode, TopLevelCount, Cardinality, CharacterSetCatalog, CharacterSetName, CharacterSetSchema, CollationCatalog, CollationName, CollationSchema, Data, DatetimeIntervalCode, DatetimeIntervalPrecision, Degree, Indicator, KeyMember, Length, Level, Name, Nullable, NullOrdering, OctetLength, ParameterMode, ParameterOrdinalPosition, ParameterSpecificCatalog, ParameterSpecificName, ParameterSpecificSchema, Precision, ReturnedCardinality, ReturnedLength, ReturnedOctetLength, Scale, ScopeCatalog, ScopeName, ScopeSchema, SortDirection, Type, Unnamed, UserDefinedTypeCatalog, UserDefinedTypeName, UserDefinedTypeSchema, UserDefinedTypeCode }
public abstract record DescriptorCopy : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DescriptorCopy() { } public sealed record Whole(DescriptorReference Source, DescriptorReference Target) : DescriptorCopy; public sealed record Item(DescriptorReference Source, Expression SourceIndex, SqlList<DescriptorCopyOption> Options, DescriptorReference Target, Expression TargetIndex) : DescriptorCopy; }
public enum DescriptorCopyOption { Name, Type, Data }
public abstract record DescribeBody : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DescribeBody() { } public sealed record Input(StatementReference Statement, DescriptorReference Descriptor, bool? WithNesting, bool SqlKeyword = false) : DescribeBody; public sealed record Output(DescribeObject Object, DescriptorReference Descriptor, bool? WithNesting, bool OutputKeyword, bool SqlKeyword = false) : DescribeBody; }
public abstract record DescribeObject : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DescribeObject() { } public sealed record Statement(StatementReference Value) : DescribeObject; public sealed record Cursor(CursorReference Value) : DescribeObject; }
public abstract record DynamicArguments : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DynamicArguments() { } public sealed record Values(SqlList<Expression> Items) : DynamicArguments; public sealed record Descriptor(DescriptorReference Value, bool SqlKeyword) : DynamicArguments; }

// --- Transactions/connections ---------------------------------------------
public abstract record TransactionMode : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected TransactionMode() { } public sealed record Isolation(IsolationLevel Level) : TransactionMode; public sealed record Access(TransactionAccess Mode) : TransactionMode; public sealed record DiagnosticsSize(Expression Size) : TransactionMode; }
// T-SQL: SET TRANSACTION ISOLATION LEVEL (Transact-SQL). `Snapshot` is T-SQL's.
public enum IsolationLevel { ReadUncommitted, ReadCommitted, RepeatableRead, Serializable, Snapshot }
public enum TransactionAccess { ReadOnly, ReadWrite }
public abstract record ConstraintTarget : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected ConstraintTarget() { } public sealed record All : ConstraintTarget; public sealed record Names(SqlList<QualifiedName> Values) : ConstraintTarget; }
public enum ChainMode { Chain, NoChain }
public sealed record ConnectionTarget(Expression? Server, Expression? Name, Expression? User, bool Default) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public abstract record ConnectionObject : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected ConnectionObject() { } public sealed record Default : ConnectionObject; public sealed record Named(Expression Name) : ConnectionObject; }
public abstract record DisconnectObject : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected DisconnectObject() { } public sealed record Connection(ConnectionObject Value) : DisconnectObject; public sealed record All : DisconnectObject; public sealed record Current : DisconnectObject; }
public sealed record TransformGroupCharacteristic(bool DefaultGroup, QualifiedName? ForType, Expression Value) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// --- Diagnostics ------------------------------------------------------------

// BNF: <SQL diagnostics information>: a statement's items, a condition's items, or all of either.
public abstract record DiagnosticsInformation : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected DiagnosticsInformation()
	{
	}

	public sealed record Statement(SqlList<StatementInformationItem> Items) : DiagnosticsInformation;
	public sealed record Condition(Expression Number, SqlList<ConditionInformationItem> Items) : DiagnosticsInformation;

	// BNF: <all information>, `:t = ALL CONDITION 1`; Qualifier null where none was written.
	public sealed record All(Expression Target, AllInformationQualifier? Qualifier, Expression? ConditionNumber = null) : DiagnosticsInformation;
}
public sealed record StatementInformationItem(Expression Target, StatementInformationItemName Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public sealed record ConditionInformationItem(Expression Target, ConditionInformationItemName Name) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum AllInformationQualifier { Statement, Condition }
public enum StatementInformationItemName { Number, More, CommandFunction, CommandFunctionCode, DynamicFunction, DynamicFunctionCode, RowCount, TransactionsCommitted, TransactionsRolledBack, TransactionActive }
public enum ConditionInformationItemName { CatalogName, ClassOrigin, ColumnName, ConditionNumber, ConnectionName, ConstraintCatalog, ConstraintName, ConstraintSchema, CursorName, MessageLength, MessageOctetLength, MessageText, ParameterMode, ParameterName, ParameterOrdinalPosition, ReturnedSqlstate, RoutineCatalog, RoutineName, RoutineSchema, SchemaName, ServerName, SpecificName, SubclassOrigin, TableName, TriggerCatalog, TriggerName, TriggerSchema }

// --- SQL conditions / WHENEVER ---------------------------------------------
public abstract record SqlCondition : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected SqlCondition() { } public sealed record Major(SqlConditionMajor Value) : SqlCondition; public sealed record State(string ClassCode, string? SubclassCode) : SqlCondition; public sealed record Constraint(QualifiedName Name) : SqlCondition; }
public enum SqlConditionMajor { Exception, Warning, NotFound }
public abstract record ConditionAction : ISqlNode { SqlLocation _location; public SqlSpan Span => _location.Span; public void Locate(int at, int length) { _location = new SqlLocation(new SqlSpan(at, length)); } private protected ConditionAction() { } public sealed record Continue : ConditionAction; public sealed record GoTo(string Target, bool TwoWordKeyword) : ConditionAction; }

// --- Embedded SQL -----------------------------------------------------------
/// <summary>BNF: embedded SQL host-program sections. Kept out of the core SQL families.</summary>
public abstract record EmbeddedSql : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected EmbeddedSql()
	{
	}

	public sealed record Statement(EmbeddedPrefix Prefix, DotGram.Sql.Ast.Statement Value, EmbeddedTerminator? Terminator) : EmbeddedSql;
	public sealed record DeclareSection(EmbeddedPrefix Prefix, CharacterSetName? CharacterSet, SqlList<HostDeclaration> Declarations, EmbeddedTerminator? Terminator, EmbeddedPrefix? EndPrefix = null, EmbeddedTerminator? EndTerminator = null) : EmbeddedSql;
	public sealed record Authorization(EmbeddedAuthorization Value) : EmbeddedSql;
	public sealed record Path(PathSpecification Value) : EmbeddedSql;
	public sealed record Transform(TransformGroupSpecification Value) : EmbeddedSql;
	public sealed record Collation(SqlList<ModuleCollation> Values) : EmbeddedSql;
}
public enum EmbeddedPrefix { ExecSql, AmpersandSqlParen }
public enum EmbeddedTerminator { EndExec, Semicolon, RightParen }
public abstract record HostDeclaration : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected HostDeclaration()
	{
	}

	public sealed record Ada(string Text) : HostDeclaration;
	public sealed record C(string Text) : HostDeclaration;
	public sealed record Cobol(string Text) : HostDeclaration;
	public sealed record Fortran(string Text) : HostDeclaration;
	public sealed record Mumps(string Text) : HostDeclaration;
	public sealed record Pascal(string Text) : HostDeclaration;
	public sealed record Pli(string Text) : HostDeclaration;
}
public sealed record EmbeddedAuthorization(QualifiedName? Schema, AuthorizationIdentifier? Authorization, EmbeddedStaticMode? StaticMode) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}
public enum EmbeddedStaticMode { StaticOnly, StaticAndDynamic }
public sealed record ModuleCollation(CollationName Collation, SqlList<CharacterSetName> CharacterSets) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

// --- SQL-client modules -----------------------------------------------------

// BNF: <SQL-client module definition>: its name and character set, language, authorization, path, transform groups,
// collations, temporary tables, and what it contains.
public sealed record SqlClientModule : ISqlNode
{
	SqlLocation _location;
	Type EqualityContract => SqlLocation.Guard(GetType());
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	public Identifier? Name { get; init; }
	public CharacterSetName? NamesAre { get; init; }
	public required string Language { get; init; }
	public required EmbeddedAuthorization Authorization { get; init; }
	public PathSpecification? Path { get; init; }
	public TransformGroupSpecification? TransformGroup { get; init; }
	public SqlList<ModuleCollation> Collations { get; init; } = [];
	public SqlList<Statement.DeclareLocalTemporaryTable> TemporaryTables { get; init; } = [];
	public SqlList<ModuleContent> Contents { get; init; } = [];
}

// BNF: <module contents>: a cursor declared, statically or dynamically, or an <externally-invoked procedure>.
public abstract record ModuleContent : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ModuleContent()
	{
	}

	public sealed record Cursor(Statement.DeclareCursor Declaration) : ModuleContent;
	public sealed record Procedure(Identifier Name, SqlList<HostParameterDeclaration> Parameters, Statement Body) : ModuleContent;
}

// BNF: <host parameter declaration>: a host parameter with its type, or the status parameter SQLSTATE.
public abstract record HostParameterDeclaration : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected HostParameterDeclaration()
	{
	}

	public sealed record Parameter(Identifier Name, DataType Type, bool Locator = false) : HostParameterDeclaration;
	public sealed record Status : HostParameterDeclaration;
}

// --- Misc ------------------------------------------------------------------
public enum PassThroughMode { PassThrough, NoPassThrough }
public enum TableSemantics { Row, Set }
public enum TablePruning { PruneOnEmpty, KeepOnEmpty }
public enum PadCharacteristic { NoPad, PadSpace }
