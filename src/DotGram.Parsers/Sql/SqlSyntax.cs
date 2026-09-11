using System;

namespace DotGram.Parsers.Sql;

// ── What a SQL parser builds ───────────────────────────────────────────────────────────────
//
// Five hierarchies, one per category the standard has: `Statement`, `Query`, `Expression`,
// `TableReference` and `Clause`. They are named after the productions they come from and are
// shared by every parser of the language — `SqlStandard92`, `TransactSql`, and whatever reads
// the same grammar next. `docs/ast.md` lists each node and the specification it is named from.
//
// **Why five and not one.** A tree with a single root types nothing: a field of it accepts a
// statement where a value belongs, and the compiler cannot tell a reader they are wrong. The
// standard does not work that way — §7 puts a <query expression> where a table belongs and §6
// puts a <value expression> where a value belongs, and the two are not interchangeable. So the
// roots are the standard's own categories, and a field says which one it holds:
// `Statement.Insert.Rows` is a `Query`, `Expression.Subquery.Query` is a `Query`, and
// `Query.Specification.From` is a `TableReference[]`. Several things that used to typecheck and
// were wrong now do not compile.
//
// **Relations by aggregation, never by inheritance.** A subquery is not a kind of query; it is
// an expression that holds one. A statement that returns rows is not a kind of query; it is a
// statement that holds one. Nothing here derives from anything but its own root, and no root
// derives from another. Crossing between hierarchies is always a field.
//
// **One level under each root.** A consumer switches over the descendants of the root it holds
// and has seen all of them. A node under another node would be a node half its readers miss.
//
// **A record per production.** `DROP TABLE` and `DROP VIEW` are not one statement with a word
// in it, and a consumer reading that word to tell them apart is doing the parser's work twice.
// The same goes for the operators: the standard writes <numeric value expression> ::= … <plus
// sign> <term> as its own production, so `Expression.Add` is a record and not an enum value.
// The one place a word survives is where the standard itself has a production for the word
// — <comp op> — so a comparison is one record carrying a `SqlComparison`.
//
// **The tree knows how it is made.** The words a grammar matches and the lists it gathers
// become nodes through the statics on each root and on `Syntax`, so that any parser of this
// language — generated under any carrier, or written by hand — builds the same tree by the
// same code.
//
// **Nothing here is a position.** The tree says what was written, not where. A consumer that
// needs the text back cuts it from the input itself, which is what §7.6 of `docs/syntax.md`
// is for.

/// <summary>
/// Where a node was written, in characters of the input the parser was given.
/// </summary>
/// <remarks>
/// The node's own text and not the trivia around it, so that a comment falls between two
/// spans rather than inside one — which is what lets a second pass hand every comment to
/// the innermost node it stands in.
/// </remarks>
public readonly record struct SqlSpan(int At, int Length)
{
	/// <summary>One past the last character, for a reader that wants the other end.</summary>
	public int End => At + Length;

	/// <summary>Whether anything was recorded, which nothing is unless a parser asked for it.</summary>
	public bool Known => Length > 0 || At > 0;
}

/// <summary>
/// A node that can say where it was written.
/// </summary>
/// <remarks>
/// <para>
/// <b>A method and not a setter, because the policy is not the parser's.</b> The reader offers
/// every rule's range to the value that came out of it, innermost first, and what to do with
/// the offer is decided here.
/// </para>
/// <para>
/// <b>The last offer wins, so a span is the outermost rule that handed the value back.</b>
/// Where a rule hands back a value another rule made — <c>WhereClause = "WHERE"i &amp;
/// c: SearchCondition =&gt; @(c)</c> gives back the condition — the range covers the
/// keyword the value itself is not. That is a little wide and it is the safe direction:
/// keeping the first offer instead is a little wide nowhere and badly wrong somewhere,
/// because a value completed from a tail — <c>Syntax.Predicated</c> copies one with
/// <c>with</c> — would inherit the tail's range and claim <c>= 2</c> for <c>b = 2</c>.
/// </para>
/// <para>
/// It mutates, once, on a node the reader has just made and nothing has yet seen. An
/// <c>init</c> would mean <c>with</c>, and <c>with</c> would copy every node of every tree to
/// record a number that was already known.
/// </para>
/// <para>
/// A grammar asks for this by naming the interface —
/// <c>[Gram("X.gram", LocationType = typeof(ISqlSpan))]</c> — and a grammar that does not ask
/// pays nothing: <c>Rfc3986</c> and <c>ExpressionParser</c> are recognizers and want no
/// positions, which is what <c>docs/ast.md</c> says and stays true where it was right.
/// </para>
/// </remarks>
public interface ISqlSpan
{
	/// <summary>Where this was written, once a reader has said so.</summary>
	SqlSpan Span { get; }

	/// <summary>
	/// Offer a range: the reader calls this for every rule the value came out of, innermost
	/// first, and the last of them is the one kept.
	/// </summary>
	void Locate(int at, int length);
}

/// <summary>What a statement does, which a reader filters on without naming every record.</summary>
public enum StatementCategory
{
	/// <summary>A query: <c>SELECT</c>, and the clauses the statement wraps around it.</summary>
	Query,

	/// <summary>Rows changed: <c>INSERT</c>, <c>UPDATE</c>, <c>DELETE</c>, <c>MERGE</c>.</summary>
	Dml,

	/// <summary>An object defined, changed or removed: <c>CREATE</c>, <c>ALTER</c>, <c>DROP</c>.</summary>
	Ddl,

	/// <summary>
	/// Who may do what: <c>GRANT</c>, <c>DENY</c>, <c>REVOKE</c>, and the logins, users and roles
	/// they name.
	/// </summary>
	Dcl,

	/// <summary>
	/// The flow of a batch: a block, <c>IF</c>, <c>WHILE</c>, <c>TRY</c>, <c>GOTO</c>, <c>RETURN</c>,
	/// <c>WAITFOR</c>, and what a batch says as it goes — <c>PRINT</c>, <c>RAISERROR</c>, <c>THROW</c>.
	/// </summary>
	Control,

	/// <summary>A transaction begun, committed, rolled back or saved.</summary>
	Transaction,

	/// <summary>The session: an option <c>SET</c>, a database <c>USE</c>d, someone else run as.</summary>
	Session,

	/// <summary><c>EXECUTE</c>: a procedure called, or a string run.</summary>
	Execute,

	/// <summary>
	/// The server looked after: <c>BACKUP</c>, <c>RESTORE</c>, <c>CHECKPOINT</c>,
	/// <c>UPDATE STATISTICS</c>.
	/// </summary>
	Admin,

	/// <summary>A variable or a cursor declared, and a variable set.</summary>
	Declaration,
}

/// <summary>A statement: §13 of the standard, and most of a dialect's reference.</summary>
public abstract record Statement : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	/// <summary>
	/// What the statement does — a query, rows changed, an object defined, a permission, the flow
	/// of a batch — for a reader that wants some statements and not others.
	/// </summary>
	public abstract StatementCategory Category { get; }

	// ---- §14 the data statements -------------------------------------------------------------

	/// <summary>§14.1 a query, and the clauses the statement wraps it in.</summary>
	/// <remarks>
	/// The parts in the order the reference writes them: the named queries in front, the
	/// query itself, the order its rows are asked for in, what shape they come back in, and
	/// how the whole thing is to be run. Four of the five are the statement's and not the
	/// query's — a <c>UNION</c> of two selects has one <c>ORDER BY</c> between them.
	/// </remarks>
	public sealed record Select(
		Clause[] With, Query Of, Clause? OrderBy, Clause[] For, Clause[] Options) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Query;
	}

	/// <summary>A select with nothing around it, which is the whole of the standard's.</summary>
	public static Select Selected(Query of, Clause? by = null) =>
		new(Clause.None, of, by, Clause.None, Clause.None);

	/// <summary>§14.11 rows written into a table, from a list, a query or nothing at all.</summary>
	/// <remarks>
	/// <see cref="Rows"/> is a <see cref="Query.TableValueConstructor"/> where they were written
	/// out, a <see cref="Query.Specification"/> where they come from one, a
	/// <see cref="Query.FromExecute"/> for T-SQL's <c>INSERT … EXEC</c>, and
	/// <see cref="Query.DefaultValues"/> for <c>DEFAULT VALUES</c>.
	/// </remarks>
	/// <param name="With">The common table expressions in front of it.</param>
	/// <param name="Top">T-SQL's <c>TOP</c>, where one was written.</param>
	/// <param name="Output">T-SQL's <c>OUTPUT</c> clause, where one was written.</param>
	/// <param name="Into">Whether the optional <c>INTO</c> was written, which is a spelling the tree keeps.</param>
	public sealed record Insert(
		TableReference? Target, string[]? Columns, Query Rows,
		Clause[]? With = null, Clause? Top = null, Clause? Output = null, bool Into = true) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;

		/// <summary>
		/// T-SQL's <c>OPTION (…)</c> after the rows, where they are a query, a <c>VALUES</c>
		/// or <c>DEFAULT VALUES</c> — the engine refuses it after an <c>EXEC</c>.
		/// </summary>
		public Clause[]? Options { get; init; }
	}

	/// <summary>§14.14 rows changed in place: what to change, to what, and which rows.</summary>
	/// <remarks>
	/// <see cref="From"/> is T-SQL's extension and not the standard's: a second <c>FROM</c>
	/// naming the tables the rows to change are found by joining.
	/// </remarks>
	public sealed record Update(
		TableReference? Target, Clause[] Set, TableReference[] From, Expression? Where,
		Clause[]? With = null, Clause? Top = null, Clause? Output = null, Clause[]? Options = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary>§14.9 rows removed, and the same two ways of saying which.</summary>
	public sealed record Delete(
		TableReference? Target, TableReference[] From, Expression? Where,
		Clause[]? With = null, Clause? Top = null, Clause? Output = null, Clause[]? Options = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary>
	/// §14.12 one statement that inserts, updates and deletes, according to what a join found.
	/// </summary>
	public sealed record Merge(
		TableReference? Target, TableReference Using, Expression On, Clause[] Whens,
		Clause[]? With = null, Clause? Top = null, string? Alias = null, Clause? Output = null, Clause[]? Options = null,
		bool Into = true) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	// ---- the procedural level ----------------------------------------------------------------
	//
	// Structure gets a record and shapelessness does not. A block holds statements, a
	// conditional holds two, a declaration holds a list — each of those is a shape and each
	// has one.

	/// <summary><c>BEGIN … END</c>, and the body of anything that has one.</summary>
	public sealed record Compound(Statement[] Statements, Clause[]? Atomic = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>IF … ELSE</c>, where either arm is one statement and a block is one.</summary>
	public sealed record If(Expression Condition, Statement Then, Statement? Else) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>WHILE</c>, and the one statement it repeats.</summary>
	public sealed record While(Expression Condition, Statement Body) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>BEGIN TRY … END TRY BEGIN CATCH … END CATCH</c>.</summary>
	public sealed record TryCatch(Statement[] Tried, Statement[] Caught) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>One <c>DECLARE</c>, which may declare several.</summary>
	public sealed record Declare(Clause[] Variables) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Declaration;
	}

	/// <summary>A transaction begun, committed, rolled back or saved, and its name.</summary>
	/// <param name="Word"><c>TRANSACTION</c>, <c>TRAN</c> or <c>WORK</c>, as written.</param>
	/// <param name="Tail">A mark or a durability, as written.</param>
	public sealed record Transaction(
		string Kind, string? Name, string? Word = null, string? Tail = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Transaction;
	}

	/// <summary>
	/// <c>EXECUTE</c>: what is called, with what, and the variable the return code goes to.
	/// </summary>
	/// <param name="Context">The login or the user a string runs as, <see cref="Clause.ExecutionContext"/>.</param>
	/// <param name="At">The linked server it runs on.</param>
	/// <param name="DataSource">The external data source it runs on instead, <c>AT DATA_SOURCE ds</c>.</param>
	/// <param name="Tail">The <c>WITH</c> after it — <c>RECOMPILE</c>, <c>RESULT SETS …</c>.</param>
	public sealed record Execute(
		string? Into, string Name, Expression[] Arguments,
		Clause? Context = null, string? At = null, string? DataSource = null, string? Tail = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Execute;
	}

	// ---- the tables ---------------------------------------------------------------------------

	/// <summary>
	/// §11.1 a table declared: its name, what kind of table T-SQL says it is where it says
	/// one (<c>AS FILETABLE</c>, <c>AS NODE</c>, <c>AS EDGE</c>), the columns, constraints and
	/// indexes in it, where it is placed (<c>ON</c>, <c>TEXTIMAGE_ON</c>, <c>FILESTREAM_ON</c>),
	/// and the options written after it.
	/// </summary>
	public sealed record TableDefinition(
		string Name, string? Kind, Clause[] Elements, Clause[] Placements, Clause[] Options, bool External = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// A table whose columns are whatever a query returns — <c>CREATE TABLE … AS SELECT</c>,
	/// and the external table written the same way: the column names where they were given,
	/// the options between the name and the <c>AS</c>, and the query.
	/// </summary>
	public sealed record CreateTableAsSelect(
		string Name, string[]? Columns, Clause[] Options, Statement Body, bool External = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// §11.10 a table changed: its name, what is being done, to what, and the options the
	/// action was given.
	/// </summary>
	/// <summary>
	/// A statement that is a word, a name, and a catalogue's own words after them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A hundred and forty statements of SQL Server are a phrase, a thing's name, and then a
	/// clause belonging to that thing rather than to the language: <c>CREATE WORKLOAD GROUP g
	/// WITH (IMPORTANCE = HIGH) USING pool</c>, <c>BACKUP DATABASE d TO DISK = 'x' WITH
	/// COMPRESSION</c>, <c>CREATE FULLTEXT CATALOG c ON FILEGROUP f AS DEFAULT</c>. Which
	/// statement it is is the record's own name; what is written after the name is
	/// <see cref="Tail"/>, kept as the words it was written as.
	/// </para>
	/// <para>
	/// Text and not a shape, for <see cref="Clause.Hint"/>'s reason and with the same
	/// bargain: it loses nothing and claims nothing. The vocabulary here is a catalogue —
	/// two hundred backup options, the file formats of every external source SQL Server has
	/// ever spoken to — and a catalogue is not a language. A statement whose parts a tool
	/// really needs gets them the way <c>CREATE TABLE</c> did: a record of its own, filled
	/// in where somebody needs it.
	/// </para>
	/// </remarks>
	/// <summary>
	/// A statement that is <c>DROP</c>, a phrase, and the names it drops — with whatever the
	/// syntax writes after them, as it was written.
	/// </summary>
	/// <remarks>
	/// The same bargain <see cref="Definition"/> strikes, for the same family of statements
	/// seen from the other end: sixty-five of them, each a phrase and a list, and a few with
	/// a clause of their own — <c>ON SERVER</c> for an event notification, <c>ON t WITH
	/// (…)</c> for an index.
	/// </remarks>
	public abstract record Removal(Expression[] Names) : Statement
	{
		/// <summary>Removing an object is DDL; the few that remove a principal say so themselves.</summary>
		public override StatementCategory Category => StatementCategory.Ddl;

		/// <summary>What was written after the names, or null where nothing was.</summary>
		public string? Tail { get; init; }

		/// <summary>
		/// Whether <c>IF EXISTS</c> stood between the phrase and the names. It is a word of
		/// the statement and not of the names, which is why it is here and not in the tail:
		/// forty of the published blocks offer it, and dropping it changes what the script
		/// does the second time it is run.
		/// </summary>
		public bool IfExists { get; init; }
	}

	public abstract record Definition(string Name) : Statement
	{
		/// <summary>
		/// Defining an object is DDL; a principal, a backup, a restore and a statistics update say
		/// otherwise themselves.
		/// </summary>
		public override StatementCategory Category => StatementCategory.Ddl;

		/// <summary>What was written after the name, or null where nothing was.</summary>
		public string? Tail { get; init; }

		/// <summary>
		/// The settings written among <see cref="Tail"/>, as nodes — where a check needs them,
		/// and null where nothing has asked.
		/// </summary>
		/// <remarks>
		/// The words stay in <see cref="Tail"/> and are what prints; these are the same settings
		/// seen as parts, which is what a question about them — one written twice — has to
		/// match. They came for the keys and the certificates first, because a repeated
		/// <c>SUBJECT</c> or <c>ALGORITHM</c> is what the engine refuses there, and a list
		/// nested inside one — a private key's, an Always Encrypted key's value — is an
		/// option holding it.
		/// </remarks>
		public Clause[]? Options { get; init; }

		/// <summary>
		/// The word the statement opened with, where the syntax lets it be more than one —
		/// <c>CREATE</c>, <c>ALTER</c>, <c>CREATE OR ALTER</c>. Null where the record's own
		/// name says which, the two verbs having a record each.
		/// </summary>
		public string? Verb { get; init; }
	}

	/// <param name="Tail">What the action was given, where the tree keeps it as written.</param>
	public sealed record AlterTable(
		string Name, string Action, Clause[] Elements, Clause[]? Options = null, string? Tail = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	// ---- the routines --------------------------------------------------------------------------

	/// <summary>
	/// A procedure: its name, what it takes, and what it does — or the method in an assembly
	/// that does it — with the options between, the number after a <c>;</c>, and whether it
	/// is for replication.
	/// </summary>
	public sealed record CreateProcedure(
		string Name, Clause[] Parameters, Statement[] Body,
		Clause[]? Options = null, bool ForReplication = false, string? External = null, string? Number = null,
		string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// A function, and what it returns says which of the three shapes it is: a type for a
	/// scalar, <c>TABLE</c> for either of the two that return rows — the inline one whose body
	/// is one <c>RETURN</c> and a query, and the one that declares the table it fills,
	/// <c>@variable TABLE (…)</c>, with <see cref="Columns"/> and the variable's name. The
	/// options stand between the return and the <c>AS</c>; <see cref="External"/> is the
	/// method in an assembly where that is the body; <see cref="Order"/> is a CLR table
	/// function's <c>ORDER (…)</c>.
	/// </summary>
	public sealed record CreateFunction(
		string Name, Clause[] Parameters, string? Returns, Statement[] Body,
		Clause[]? Options = null, Clause[]? Columns = null, string? Variable = null,
		Clause[]? Order = null, string? External = null, string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>A trigger: what it is on, what fires it, and what it does then.</summary>
	/// <param name="When"><c>AFTER</c>, <c>FOR</c> or <c>INSTEAD OF</c>, as written.</param>
	/// <param name="External">The method in an assembly, where that is the body.</param>
	public sealed record CreateTrigger(
		string Name, string On, string[] Events, Statement[] Body,
		string When = "AFTER", Clause[]? Options = null, bool Append = false,
		bool NotForReplication = false, string? External = null, string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>§11.32 a view, which is a name given to a query.</summary>
	/// <param name="Options">The <c>WITH</c> between the name and the <c>AS</c>: <c>SCHEMABINDING</c>, a materialized view's distribution.</param>
	/// <param name="CheckOption">The <c>WITH CHECK OPTION</c> after the query.</param>
	public sealed record ViewDefinition(
		string Name, string[]? Columns, Statement Body,
		Clause[]? Options = null, bool CheckOption = false, bool Materialized = false,
		string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	// ---- indexes -------------------------------------------------------------------------------

	/// <summary>An index declared: its name, what it is on, and the columns it is over.</summary>
	/// <summary>
	/// <c>CREATE INDEX</c>: the table, and the index — the same node an index written inside
	/// a table is, since it is the same thing said in the same words.
	/// </summary>
	/// <param name="Kind">
	/// The words between <c>CREATE</c> and <c>INDEX</c> that are not the index's own —
	/// <c>PRIMARY XML</c>, <c>XML</c>, <c>SELECTIVE XML</c> — where the index is one of those.
	/// </param>
	/// <param name="Using">
	/// The primary XML index a secondary one is built on — <c>USING XML INDEX i FOR PATH</c>.
	/// </param>
	public sealed record CreateIndex(
		string On, Clause Index, string? Kind = null, string? Using = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>An index changed: which one, on what, and what is being done to it.</summary>
	/// <summary>
	/// <c>ALTER INDEX</c>: the index or <c>ALL</c>, the table, the action, and what the action
	/// was given — the partition, the options, and for a selective XML index the paths it
	/// promotes, each kept as written, with the namespaces in front of them.
	/// </summary>
	public sealed record AlterIndex(
		string Name, string On, string Action,
		Expression? Partition = null, Clause[]? Options = null, string[]? Paths = null, string? Namespaces = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>CREATE STATISTICS</c>.</summary>
	public sealed record StatisticsDefinition(string Name) : Definition(Name);

	/// <summary><c>UPDATE STATISTICS</c>, which names the table rather than the statistics.</summary>
	public sealed record UpdateStatistics(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	// ---- Service Broker's conversations --------------------------------------------------------
	//
	// A conversation begun, timed, sent on, received from, moved and ended. Each is a shape of
	// its own, and the word they share says nothing a record would.

	/// <summary><c>BEGIN DIALOG</c>: a conversation begun from one service to another.</summary>
	/// <param name="Handle">The variable the conversation's handle is put in.</param>
	/// <param name="Conversation">Whether <c>CONVERSATION</c> followed <c>DIALOG</c>.</param>
	/// <param name="From">The service it is begun from, by name.</param>
	/// <param name="To">The service it is begun to: a string or a variable, never a name.</param>
	/// <param name="Instance">Which broker that service is on, where it was said.</param>
	/// <param name="Contract">The contract it keeps to, where one was named.</param>
	/// <param name="Options">What followed <c>WITH</c>, a <see cref="Clause.Option"/> each.</param>
	public sealed record BeginDialog(
		string Handle, bool Conversation, string From, Expression To, Expression? Instance,
		string? Contract, Clause[] Options) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>BEGIN CONVERSATION TIMER</c>: how long a conversation waits before it is told.</summary>
	public sealed record ConversationTimer(Expression Handle, Expression Timeout) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>END CONVERSATION</c>, with an error said or the conversation cleaned up.</summary>
	/// <param name="Handle">The conversation, a value.</param>
	/// <param name="Error">The failure's code, where one was given; a description always with it.</param>
	/// <param name="Description">What the failure was.</param>
	/// <param name="Cleanup">Whether <c>WITH CLEANUP</c> ended it without telling the other side.</param>
	public sealed record EndConversation(
		Expression Handle, Expression? Error = null, Expression? Description = null, bool Cleanup = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>MOVE CONVERSATION</c>: a conversation put in another group.</summary>
	public sealed record MoveConversation(Expression Handle, Expression Group) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>SEND</c>: a message sent on one conversation or several.</summary>
	/// <param name="Conversations">The handles it is sent on.</param>
	/// <param name="Bracketed">Whether they stood in brackets, which one may and several must.</param>
	/// <param name="MessageType">The message's type, where one was named.</param>
	/// <param name="Body">What it says, where it says anything.</param>
	public sealed record Send(
		Expression[] Conversations, bool Bracketed, string? MessageType = null, Expression? Body = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary><c>RECEIVE</c>: messages taken off a queue.</summary>
	/// <param name="Top">How many, where it said.</param>
	/// <param name="Columns">What is returned of each, as a select list says it.</param>
	/// <param name="Queue">The queue, by name.</param>
	/// <param name="Into">The table variable they go into, where they go into one.</param>
	/// <param name="Where">
	/// Which conversation or group they are from: one comparison, of one of those two columns.
	/// </param>
	public sealed record Receive(
		Clause? Top, Clause[] Columns, string Queue, string? Into = null, Expression? Where = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary><c>GET CONVERSATION GROUP</c>: the group of a queue's next message, into a variable.</summary>
	public sealed record GetConversationGroup(string Group, string Queue) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary>
	/// <c>WAITFOR ( … )</c> around a <see cref="Receive"/> or a <see cref="GetConversationGroup"/>:
	/// the statement waited on, and for how long.
	/// </summary>
	/// <param name="Statement">What is waited on.</param>
	/// <param name="Timeout">The <c>TIMEOUT</c>, where one was given.</param>
	public sealed record WaitForStatement(Statement Statement, Expression? Timeout = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	// ---- DBCC ----------------------------------------------------------------------------------
	/// <summary><c>DBCC</c>: a console command, what stood in its brackets, and its options.</summary>
	/// <remarks>
	/// One record for every command, the documented and the rest, because the engine reads them
	/// all one way: what a command means is its own business, not the tree's.
	/// </remarks>
	/// <param name="Command">The command as written, <c>CHECKDB</c> or a library's name.</param>
	/// <param name="Arguments">
	/// A value each, or a <see cref="Expression.NamedArgument"/>. Null where there were no
	/// brackets, and empty where they held nothing.
	/// </param>
	/// <param name="Options">What followed <c>WITH</c>, a <see cref="Clause.Option"/> each.</param>
	public sealed record Dbcc(string Command, Expression[]? Arguments, Clause[] Options) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	// ---- cursors -------------------------------------------------------------------------------
	//
	// T-SQL's cursors: one declared, or set to a variable, and what is done with one. Opening,
	// closing and letting one go are a word and a cursor, and one record says which; a fetch
	// says which row, and where it goes.

	/// <summary>A cursor declared by name, and what it is over.</summary>
	public sealed record DeclareCursor(string Name, Clause.CursorDefinition Cursor) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Declaration;
	}

	/// <summary>
	/// A cursor variable set to a cursor defined where it stands, <c>SET @c = CURSOR … FOR …</c>.
	/// Set to another cursor, it is <see cref="SetVariable"/>.
	/// </summary>
	public sealed record SetCursor(string Name, Clause.CursorDefinition Cursor) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Declaration;
	}

	/// <summary>A cursor opened, closed or let go.</summary>
	/// <param name="Verb"><c>OPEN</c>, <c>CLOSE</c> or <c>DEALLOCATE</c>.</param>
	/// <param name="Cursor">A cursor's name, or a variable holding one.</param>
	/// <param name="Global">
	/// Whether <c>GLOBAL</c> said the name is the connection's rather than the batch's. Never
	/// before a variable, which the engine refuses.
	/// </param>
	public sealed record CursorAction(string Verb, Expression Cursor, bool Global) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A row fetched from a cursor, and the variables it is read into.</summary>
	/// <param name="Orientation">
	/// Which row: <c>NEXT</c>, <c>PRIOR</c>, <c>FIRST</c>, <c>LAST</c>, <c>ABSOLUTE</c> or
	/// <c>RELATIVE</c>. Null where none was said, which is <c>NEXT</c>.
	/// </param>
	/// <param name="Offset">
	/// The row of <c>ABSOLUTE</c> and <c>RELATIVE</c>: a number, negative or not, or a variable.
	/// </param>
	/// <param name="From">
	/// Whether <c>FROM</c> was written. It must be after an orientation and may be without one.
	/// </param>
	/// <param name="Cursor">A cursor's name, or a variable holding one.</param>
	/// <param name="Global">Whether <c>GLOBAL</c> stood before the name.</param>
	/// <param name="Into">The variables the row's columns go into, where it goes into any.</param>
	public sealed record Fetch(
		string? Orientation, Expression? Offset, bool From, Expression Cursor, bool Global,
		Expression[]? Into) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	// ---- a word and some values ----------------------------------------------------------------
	//
	// Ten statements that are a word and what follows it. They shared one record until the
	// tree was made to say what the grammar says, and a reader who wanted the `PRINT` had to
	// look at a string to find it.

	/// <summary>One value printed.</summary>
	public sealed record Print(Expression Value) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A routine left, with a code where one was given.</summary>
	public sealed record Return(Expression? Value) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>An error raised, or the caught one raised again.</summary>
	public sealed record Throw(Expression[] Arguments) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A jump to a label.</summary>
	public sealed record GoTo(Expression Label) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A loop left.</summary>
	public sealed record Break : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A loop begun again.</summary>
	public sealed record Continue : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>The log written out.</summary>
	public sealed record Checkpoint(Expression? Value) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary>The database the rest of the batch is read against.</summary>
	public sealed record Use(Expression Name) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <param name="Tail">The <c>WITH LOG, NOWAIT, SETERROR</c> after it, as written.</param>
	public sealed record RaiseError(Expression[] Arguments, string? Tail = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A delay, or a time to wait until.</summary>
	/// <param name="Kind">
	/// <c>DELAY</c> or <c>TIME</c> — how long to wait against when to stop waiting, which
	/// the one value cannot say.
	/// </param>
	public sealed record WaitFor(Expression Value, string Kind = "DELAY") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary><c>EXECUTE AS</c>: whom the session runs as until a <c>REVERT</c> says otherwise.</summary>
	/// <param name="Kind"><c>CALLER</c>, <c>USER</c> or <c>LOGIN</c>.</param>
	/// <param name="Name">Whom, for a user or a login: any expression the engine will take.</param>
	/// <param name="NoRevert"><c>WITH NO REVERT</c>: the context is not given back.</param>
	/// <param name="Cookie">The variable <c>WITH COOKIE INTO</c> names, which a <c>REVERT</c> must show.</param>
	public sealed record ExecuteAs(string Kind, Expression? Name, bool NoRevert = false, Expression? Cookie = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>REVERT</c>: the context an <c>EXECUTE AS</c> changed, given back.</summary>
	/// <param name="Cookie">What <c>WITH COOKIE =</c> shows for it.</param>
	public sealed record Revert(Expression? Cookie = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>SETUSER</c>: a user impersonated, the way before <c>EXECUTE AS</c>.</summary>
	/// <param name="Name">The user, or nobody, which puts the original back.</param>
	/// <param name="NoReset"><c>WITH NORESET</c>.</param>
	public sealed record SetUser(Expression? Name = null, bool NoReset = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	// ---- who may connect, what lives outside, and what the server watches ----------------------
	//
	// One record each, and the name only: what these are set to is an option list the grammar
	// reads and drops, and a field nobody reads is a field that drifts. When one of them is
	// wanted it is one field on one record here, which is what having a record each is for.

	/// <summary><c>CREATE LOGIN</c>.</summary>
	public sealed record CreateLogin(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>ALTER LOGIN</c>.</summary>
	public sealed record AlterLogin(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>CREATE USER</c>.</summary>
	public sealed record CreateUser(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>ALTER USER</c>.</summary>
	public sealed record AlterUser(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>CREATE ROLE</c>.</summary>
	public sealed record CreateRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>ALTER ROLE</c>.</summary>
	public sealed record AlterRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>CREATE SERVER ROLE</c>: a role of the server rather than of a database.</summary>
	public sealed record CreateServerRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>ALTER SERVER ROLE</c>, likewise.</summary>
	public sealed record AlterServerRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>CREATE APPLICATION ROLE</c>.</summary>
	public sealed record CreateApplicationRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>ALTER APPLICATION ROLE</c>.</summary>
	public sealed record AlterApplicationRole(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary>§11.1 <c>CREATE SCHEMA</c>.</summary>
	public sealed record SchemaDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER SCHEMA</c>.</summary>
	public sealed record AlterSchema(string Name) : Definition(Name);

	/// <summary><c>ALTER AUTHORIZATION</c>.</summary>
	public sealed record AlterAuthorization(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record ExternalDataSourceDefinition(string Name) : Definition(Name);

	/// <summary><c>EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record ExternalFileFormatDefinition(string Name) : Definition(Name);

	/// <summary><c>EXTERNAL LIBRARY</c>.</summary>
	public sealed record ExternalLibraryDefinition(string Name) : Definition(Name);

	/// <summary><c>EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record ExternalResourcePoolDefinition(string Name) : Definition(Name);

	/// <summary><c>RESOURCE POOL</c>.</summary>
	public sealed record ResourcePoolDefinition(string Name) : Definition(Name);

	/// <summary><c>WORKLOAD GROUP</c>.</summary>
	public sealed record WorkloadGroupDefinition(string Name) : Definition(Name);

	/// <summary><c>SERVER AUDIT</c>.</summary>
	public sealed record ServerAuditDefinition(string Name) : Definition(Name);

	/// <summary><c>AUDIT SPECIFICATION</c>.</summary>
	public sealed record AuditSpecificationDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE</c> or <c>ALTER DATABASE AUDIT SPECIFICATION</c>: the other scope.</summary>
	public sealed record DatabaseAuditSpecificationDefinition(string Name) : Definition(Name);

	/// <summary><c>EVENT SESSION</c>.</summary>
	/// <summary>
	/// <c>CREATE</c> or <c>ALTER EVENT SESSION</c>: the session, where it is (<c>SERVER</c> or
	/// <c>DATABASE</c>), the events and targets added and dropped, its options, and the state
	/// it is put in.
	/// </summary>
	public sealed record EventSessionDefinition(
		string Name, string Verb = "CREATE", string On = "SERVER", Clause[]? Pieces = null,
		Clause[]? Options = null, string? State = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>EVENT NOTIFICATION</c>.</summary>
	public sealed record EventNotificationDefinition(string Name) : Definition(Name);

	/// <summary><c>MESSAGE TYPE</c>, a kind of message a conversation may carry.</summary>
	public sealed record MessageTypeDefinition(string Name) : Definition(Name);

	/// <summary><c>CONTRACT</c>: which messages a conversation holds, and who sends each.</summary>
	public sealed record ContractDefinition(string Name) : Definition(Name);

	/// <summary><c>QUEUE</c>, where a service's messages wait.</summary>
	public sealed record QueueDefinition(string Name) : Definition(Name);

	/// <summary><c>SERVICE</c>: a queue, and the contracts it answers to.</summary>
	public sealed record ServiceDefinition(string Name) : Definition(Name);

	/// <summary><c>ROUTE</c>: where a service's messages are sent.</summary>
	public sealed record RouteDefinition(string Name) : Definition(Name);

	/// <summary><c>REMOTE SERVICE BINDING</c>: whose credentials a remote service is spoken to with.</summary>
	public sealed record RemoteServiceBindingDefinition(string Name) : Definition(Name);

	/// <summary><c>BROKER PRIORITY</c>: which conversations go first.</summary>
	public sealed record BrokerPriorityDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER RESOURCE GOVERNOR</c>, which names nothing: the server's one governor.</summary>
	public sealed record AlterResourceGovernor(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>ALTER SERVER CONFIGURATION</c>, which names nothing: one setting of the server's.</summary>
	public sealed record AlterServerConfiguration(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>ENDPOINT</c>.</summary>
	/// <summary>
	/// <c>CREATE</c> or <c>ALTER ENDPOINT</c>: the owner, the state and what was written
	/// beside it, how it is reached (<c>AS TCP (…)</c>) and what it speaks (<c>FOR TSQL
	/// ()</c>) — each protocol a bracket of options belonging to it.
	/// </summary>
	public sealed record EndpointDefinition(
		string Name, string Verb = "CREATE", string? Owner = null, string? State = null,
		Clause[]? StateOptions = null, string? Protocol = null, Clause[]? ProtocolOptions = null,
		string? Payload = null, Clause[]? PayloadOptions = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	// ---- the database --------------------------------------------------------------------------

	/// <summary><c>CREATE DATABASE</c>, and the files it is made of.</summary>
	/// <summary>
	/// <c>CREATE DATABASE</c>: the file groups and files it is made of, the log files, its
	/// containment, its collation, and the tail that says what it is made from — <c>WITH
	/// …</c>, <c>FOR ATTACH (…)</c>, <c>AS SNAPSHOT OF x</c>, <c>AS COPY OF x (…)</c> — or the
	/// bracketed list of options an Azure database is written with instead.
	/// </summary>
	public sealed record CreateDatabase(
		string Name, Clause[] Files, bool Primary = false, Clause[]? Log = null,
		string? Containment = null, string? Collation = null,
		string? Tail = null, Clause[]? Options = null, Clause[]? With = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// <c>ALTER DATABASE … SET</c>: what it was set to, and how the sessions in the way are
	/// dealt with — <c>WITH ROLLBACK AFTER 10 SECONDS</c>, <c>WITH NO_WAIT</c>.
	/// </summary>
	public sealed record AlterDatabaseSet(
		string Name, Clause[] Settings, string? Termination = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// <c>ALTER DATABASE SCOPED CONFIGURATION</c>: <c>SET</c> and what it was set to, or
	/// <c>CLEAR PROCEDURE_CACHE</c> and the plan handle where one was given; for the secondary
	/// where it says so.
	/// </summary>
	public sealed record AlterDatabaseScopedConfiguration(
		string Name, string Action, Clause[] Settings, bool Secondary = false, Expression? Argument = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>ALTER DATABASE … COLLATE</c>.</summary>
	public sealed record AlterDatabaseCollate(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … MODIFY NAME</c>.</summary>
	public sealed record AlterDatabaseModifyName(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … MODIFY FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseModifyFileGroup(string Name) : Definition(Name);


	/// <summary><c>ALTER DATABASE … MODIFY FILE</c>.</summary>
	public sealed record AlterDatabaseModifyFile(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … MODIFY</c>.</summary>
	public sealed record AlterDatabaseModify(
		string Name, Clause[]? Options = null, Clause[]? With = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>ALTER DATABASE … ADD FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseAddFileGroup(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … ADD LOG FILE</c>.</summary>
	public sealed record AlterDatabaseAddLogFile(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … ADD FILE</c>.</summary>
	public sealed record AlterDatabaseAddFile(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … REMOVE FILEGROUP</c>.</summary>
	public sealed record AlterDatabaseRemoveFileGroup(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … REMOVE FILE</c>.</summary>
	public sealed record AlterDatabaseRemoveFile(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … REBUILD LOG</c>.</summary>
	public sealed record AlterDatabaseRebuildLog(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE … PERFORM_CUTOVER</c>.</summary>
	public sealed record AlterDatabasePerformCutover(string Name) : Definition(Name);

	// ---- the SET statements --------------------------------------------------------------------

	/// <summary>
	/// The SET statements: what follows <c>SET</c>, as <see cref="SetExpression"/>s — one setting,
	/// or several of one form, <c>SET ANSI_NULLS, NOCOUNT ON</c>, <c>SET DATEFIRST 1, DATEFORMAT dmy</c>.
	/// A variable assigned is <see cref="SetVariable"/>, the other construction of the word.
	/// </summary>
	public sealed record SetStatement(SetExpression[] Items) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;

		/// <summary>
		/// The groups Microsoft's reference puts the settings in, together: one for most
		/// statements, several where a list mixes them.
		/// </summary>
		public SetCategory SetCategory
		{
			get
			{
				var all = SetCategory.None;

				foreach (var one in Items)
					all |= one.Category;

				return all;
			}
		}
	}

	/// <summary>
	/// A variable assigned — <c>SET @a = 1</c>, which is a statement and not a
	/// <see cref="Clause.Set"/>: what a <c>SET</c> clause belongs to is an <c>UPDATE</c>.
	/// </summary>
	/// <param name="Operator">The assignment as written — <c>=</c>, <c>+=</c>, …</param>
	/// <param name="Through">The column of <c>SET @v = column op= value</c>, which assigns both.</param>
	public sealed record SetVariable(
		string Name, Expression Value, string? Operator = null, string? Through = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Declaration;
	}

	// ---- §12.1 the permissions -----------------------------------------------------------------

	/// <summary><c>GRANT</c>: what is being said about, and to whom.</summary>
	/// <param name="On">The securable, class and all, as written after <c>ON</c>.</param>
	/// <param name="As">The principal the statement is run as.</param>
	public sealed record Grant(
		string[] Privileges, string[] Principals,
		string? On = null, bool GrantOption = false, string? As = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DENY</c>: what is being said about, and to whom.</summary>
	public sealed record Deny(
		string[] Privileges, string[] Principals,
		string? On = null, bool Cascade = false, string? As = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>REVOKE</c>: what is being said about, and to whom — or from whom.</summary>
	public sealed record Revoke(
		string[] Privileges, string[] Principals,
		string? On = null, bool GrantOptionFor = false, bool From = false, bool Cascade = false, string? As = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	// ---- the full-text catalogue -----------------------------------------------------------------

	/// <summary><c>CREATE FULLTEXT INDEX</c>, which is named by the table it is on.</summary>
	public sealed record FullTextIndexDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER FULLTEXT INDEX</c>.</summary>
	public sealed record AlterFullTextIndex(string Name) : Definition(Name);

	/// <summary><c>CREATE FULLTEXT CATALOG</c>.</summary>
	public sealed record FullTextCatalogDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER FULLTEXT CATALOG</c>.</summary>
	public sealed record AlterFullTextCatalog(string Name) : Definition(Name);

	/// <summary><c>CREATE FULLTEXT STOPLIST</c>.</summary>
	public sealed record FullTextStopListDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER FULLTEXT STOPLIST</c>.</summary>
	public sealed record AlterFullTextStopList(string Name) : Definition(Name);

	/// <summary><c>CREATE SEARCH PROPERTY LIST</c>.</summary>
	public sealed record SearchPropertyListDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER SEARCH PROPERTY LIST</c>.</summary>
	public sealed record AlterSearchPropertyList(string Name) : Definition(Name);

	// ---- backup and restore ----------------------------------------------------------------------
	//
	// Two statements and one shape between them: what is being copied, the devices it goes to
	// or comes from, and a long option list. Seventeen records because there are seventeen
	// statements — a log is not a database and a header is not a file list, and the reference
	// gives each its own page.

	/// <summary><c>BACKUP DATABASE</c>.</summary>
	public sealed record BackupDatabase(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP LOG</c>.</summary>
	public sealed record BackupTransactionLog(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP SERVER</c>, which names nothing: there is one.</summary>
	public sealed record BackupServer(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP GROUP</c>.</summary>
	public sealed record BackupGroup(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP CERTIFICATE</c>.</summary>
	public sealed record BackupCertificate(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP MASTER KEY</c>.</summary>
	public sealed record BackupMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP SERVICE MASTER KEY</c>.</summary>
	public sealed record BackupServiceMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>BACKUP SYMMETRIC KEY</c>.</summary>
	public sealed record BackupSymmetricKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE DATABASE</c>.</summary>
	public sealed record RestoreDatabase(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE LOG</c>.</summary>
	public sealed record RestoreLog(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE FILELISTONLY</c>.</summary>
	public sealed record RestoreFileListOnly(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE HEADERONLY</c>.</summary>
	public sealed record RestoreHeaderOnly(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE LABELONLY</c>.</summary>
	public sealed record RestoreLabelOnly(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE REWINDONLY</c>.</summary>
	public sealed record RestoreRewindOnly(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE VERIFYONLY</c>.</summary>
	public sealed record RestoreVerifyOnly(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE MASTER KEY</c>.</summary>
	public sealed record RestoreMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE SERVICE MASTER KEY</c>.</summary>
	public sealed record RestoreServiceMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>RESTORE SYMMETRIC KEY</c>.</summary>
	public sealed record RestoreSymmetricKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	// ---- the keys, and what is locked with them --------------------------------------------------

	/// <summary><c>CREATE ASYMMETRIC KEY</c>.</summary>
	public sealed record AsymmetricKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER ASYMMETRIC KEY</c>.</summary>
	public sealed record AlterAsymmetricKey(string Name) : Definition(Name);

	/// <summary><c>CREATE SYMMETRIC KEY</c>.</summary>
	public sealed record SymmetricKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER SYMMETRIC KEY</c>.</summary>
	public sealed record AlterSymmetricKey(string Name) : Definition(Name);

	/// <summary><c>CREATE CERTIFICATE</c>.</summary>
	public sealed record CertificateDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER CERTIFICATE</c>.</summary>
	public sealed record AlterCertificate(string Name) : Definition(Name);

	/// <summary><c>CREATE MASTER KEY</c>.</summary>
	public sealed record MasterKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER MASTER KEY</c>.</summary>
	public sealed record AlterMasterKey(string Name) : Definition(Name);

	/// <summary><c>CREATE DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DatabaseEncryptionKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record AlterDatabaseEncryptionKey(string Name) : Definition(Name);

	/// <summary><c>CREATE COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record ColumnEncryptionKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record AlterColumnEncryptionKey(string Name) : Definition(Name);

	/// <summary><c>CREATE COLUMN MASTER KEY</c>.</summary>
	public sealed record ColumnMasterKeyDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE/ALTER CREDENTIAL</c>.</summary>
	public sealed record CredentialDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE/ALTER DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DatabaseScopedCredentialDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE/ALTER SECURITY POLICY</c>.</summary>
	public sealed record SecurityPolicyDefinition(string Name) : Definition(Name);

	// ---- what a statement drops ----------------------------------------------------------------
	//
	// Sixty-six records of one shape, because sixty-six statements of one shape is what the
	// reference has: `DROP TABLE` and `DROP VIEW` are spelled alike and are not the same
	// statement. The grammar reads the shape once and `Dropped` turns the word into the record,
	// so the catalogue of names is here, in C#, and the grammar still says one thing.

	/// <summary><c>DROP AGGREGATE</c>.</summary>
	public sealed record DropAggregate(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP APPLICATION ROLE</c>.</summary>
	public sealed record DropApplicationRole(Expression[] Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP AVAILABILITY GROUP</c>.</summary>
	public sealed record DropAvailabilityGroup(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP BROKER PRIORITY</c>.</summary>
	public sealed record DropBrokerPriority(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP CERTIFICATE</c>.</summary>
	public sealed record DropCertificate(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record DropColumnEncryptionKey(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP COLUMN MASTER KEY</c>.</summary>
	public sealed record DropColumnMasterKey(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP CONTRACT</c>.</summary>
	public sealed record DropContract(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP CREDENTIAL</c>.</summary>
	public sealed record DropCredential(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP CRYPTOGRAPHIC PROVIDER</c>.</summary>
	public sealed record DropCryptographicProvider(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP DATABASE AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropDatabaseAuditSpecification(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DropDatabaseScopedCredential(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP DATABASE</c>.</summary>
	public sealed record DropDatabase(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP DEFAULT</c>.</summary>
	public sealed record DropDefault(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP ENDPOINT</c>.</summary>
	public sealed record DropEndpoint(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record DropExternalDataSource(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record DropExternalFileFormat(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL LANGUAGE</c>.</summary>
	public sealed record DropExternalLanguage(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL MODEL</c>.</summary>
	public sealed record DropExternalModel(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record DropExternalResourcePool(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL TABLE</c>.</summary>
	public sealed record DropExternalTable(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT CATALOG</c>.</summary>
	public sealed record DropFulltextCatalog(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT STOPLIST</c>.</summary>
	public sealed record DropFulltextStoplist(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP FUNCTION</c>.</summary>
	public sealed record DropFunction(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP LOGIN</c>.</summary>
	public sealed record DropLogin(Expression[] Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP MESSAGE TYPE</c>.</summary>
	public sealed record DropMessageType(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP PARTITION FUNCTION</c>.</summary>
	public sealed record DropPartitionFunction(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP PARTITION SCHEME</c>.</summary>
	public sealed record DropPartitionScheme(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP PROCEDURE</c>.</summary>
	public sealed record DropProcedure(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP QUEUE</c>.</summary>
	public sealed record DropQueue(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP REMOTE SERVICE BINDING</c>.</summary>
	public sealed record DropRemoteServiceBinding(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP RESOURCE POOL</c>.</summary>
	public sealed record DropResourcePool(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP ROLE</c>.</summary>
	public sealed record DropRole(Expression[] Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP ROUTE</c>.</summary>
	public sealed record DropRoute(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP RULE</c>.</summary>
	public sealed record DropRule(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SCHEMA</c>.</summary>
	public sealed record DropSchema(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SEARCH PROPERTY LIST</c>.</summary>
	public sealed record DropSearchPropertyList(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SECURITY POLICY</c>.</summary>
	public sealed record DropSecurityPolicy(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SEQUENCE</c>.</summary>
	public sealed record DropSequence(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SERVER AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropServerAuditSpecification(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SERVER AUDIT</c>.</summary>
	public sealed record DropServerAudit(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SERVER ROLE</c>.</summary>
	public sealed record DropServerRole(Expression[] Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP SERVICE</c>.</summary>
	public sealed record DropService(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP STATISTICS</c>.</summary>
	public sealed record DropStatistics(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SYNONYM</c>.</summary>
	public sealed record DropSynonym(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP TABLE</c>.</summary>
	public sealed record DropTable(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP TYPE</c>.</summary>
	public sealed record DropType(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP USER</c>.</summary>
	public sealed record DropUser(Expression[] Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP VIEW</c>.</summary>
	public sealed record DropView(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP WORKLOAD CLASSIFIER</c>.</summary>
	public sealed record DropWorkloadClassifier(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP WORKLOAD GROUP</c>.</summary>
	public sealed record DropWorkloadGroup(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP XML SCHEMA COLLECTION</c>.</summary>
	public sealed record DropXmlSchemaCollection(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP ASYMMETRIC KEY</c>.</summary>
	public sealed record DropAsymmetricKey(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SYMMETRIC KEY</c>.</summary>
	public sealed record DropSymmetricKey(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP ASSEMBLY</c>.</summary>
	public sealed record DropAssembly(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL LIBRARY</c>.</summary>
	public sealed record DropExternalLibrary(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EVENT SESSION</c>.</summary>
	public sealed record DropEventSession(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP EVENT NOTIFICATION</c>.</summary>
	public sealed record DropEventNotification(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT INDEX</c>.</summary>
	public sealed record DropFulltextIndex(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP INDEX</c>.</summary>
	public sealed record DropIndex(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP SIGNATURE</c>.</summary>
	/// <param name="From">The module the signature is dropped off.</param>
	/// <param name="Counter">Whether it is a counter signature.</param>
	public sealed record DropSignature(
		Expression[] Names, string? From = null, bool Counter = false) : Removal(Names);

	/// <summary><c>DROP SENSITIVITY CLASSIFICATION</c>.</summary>
	public sealed record DropSensitivityClassification(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP TRIGGER</c>.</summary>
	public sealed record DropTrigger(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP MASTER KEY</c>.</summary>
	public sealed record DropMasterKey(Expression[] Names) : Removal(Names);

	/// <summary><c>DROP DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DropDatabaseEncryptionKey(Expression[] Names) : Removal(Names);

	// ---- how a parser makes these ------------------------------------------------------------
	//
	// The statements build through these rather than inline in their `=>`, and that is not
	// style: `GRAM5003` said the materializing method for the statement entry point was past
	// the size at which the JIT stops optimizing, and the diagnostic's own advice is to build
	// the value in a method of your own.

	/// <summary>What a call with no statements is handed, once rather than per call.</summary>
	public static readonly Statement[] None = [];

	/// <summary>The statement the word names, for the ten that are a word and a value.</summary>
	public static Statement Commanded(string word, Expression? value) =>
		word switch
		{
			"PRINT"      => new Print(value!),
			"RETURN"     => new Return(value),
			"THROW"      => new Throw(value is null ? Expression.None : [value]),
			"GOTO"       => new GoTo(value!),
			"BREAK"      => new Break(),
			"CONTINUE"   => new Continue(),
			"CHECKPOINT" => new Checkpoint(value),
			"USE"        => new Use(value!),
			"RAISERROR"  => new RaiseError(value is null ? Expression.None : [value]),
			"WAITFOR"    => new WaitFor(value!),
			_            => throw Syntax.Unknown(word),
		};

	/// <summary>The same where the values arrived as a list that may not be there.</summary>
	public static Statement Commanded(string word, Expression[]? values) =>
		word switch
		{
			"PRINT"      => new Print(values is { Length: > 0 } some ? some[0] : null!),
			"RETURN"     => new Return(values is { Length: > 0 } some ? some[0] : null),
			"THROW"      => new Throw(values ?? Expression.None),
			"GOTO"       => new GoTo(values is { Length: > 0 } some ? some[0] : null!),
			"BREAK"      => new Break(),
			"CONTINUE"   => new Continue(),
			"CHECKPOINT" => new Checkpoint(values is { Length: > 0 } some ? some[0] : null),
			"USE"        => new Use(values is { Length: > 0 } some ? some[0] : null!),
			"RAISERROR"  => new RaiseError(values ?? Expression.None),
			"WAITFOR"    => new WaitFor(values is { Length: > 0 } some ? some[0] : null!),
			_            => throw Syntax.Unknown(word),
		};

	/// <summary>What is being backed up, as the statement it is.</summary>
	public static Statement BackedUp(string what, string? name, string? tail = null) =>
		BackedUp(what, name ?? "") with { Tail = tail };

	static Definition BackedUp(string what, string name) =>
		what switch
		{
			"DATABASE"           => new BackupDatabase(name),
			"LOG"                => new BackupTransactionLog(name),
			"SERVER"             => new BackupServer(name),
			"GROUP"              => new BackupGroup(name),
			"CERTIFICATE"        => new BackupCertificate(name),
			"MASTER KEY"         => new BackupMasterKey(name),
			"SERVICE MASTER KEY" => new BackupServiceMasterKey(name),
			"SYMMETRIC KEY"      => new BackupSymmetricKey(name),
			_                    => throw Syntax.Unknown(what),
		};

	/// <summary>What is being restored, likewise.</summary>
	public static Statement Restored(string what, string? name, string? tail = null) =>
		Restored(what, name ?? "") with { Tail = tail };

	static Definition Restored(string what, string name) =>
		what switch
		{
			"DATABASE"           => new RestoreDatabase(name),
			"LOG"                => new RestoreLog(name),
			"FILELISTONLY"       => new RestoreFileListOnly(name),
			"HEADERONLY"         => new RestoreHeaderOnly(name),
			"LABELONLY"          => new RestoreLabelOnly(name),
			"REWINDONLY"         => new RestoreRewindOnly(name),
			"VERIFYONLY"         => new RestoreVerifyOnly(name),
			"MASTER KEY"         => new RestoreMasterKey(name),
			"SERVICE MASTER KEY" => new RestoreServiceMasterKey(name),
			"SYMMETRIC KEY"      => new RestoreSymmetricKey(name),
			_                    => throw Syntax.Unknown(what),
		};

	/// <summary>The definition the words name, where the tree keeps the name and no more.</summary>
	public static Statement Defined(
		string what, string name, string? tail = null, string? verb = null, Clause[]? options = null) =>
		Named(what, name) with { Tail = tail, Verb = verb is null ? null : Syntax.Squared(verb), Options = options };

	/// <summary>A list held under one name: a private key's settings, an Always Encrypted value's parts.</summary>
	public static Clause[] Holding(string name, Clause[]? settings) =>
		settings is null ? Clause.None : [new Clause.Option(name, null, settings)];

	static Definition Named(string what, string name) =>
		what switch
		{
			"CREATE LOGIN"            => new CreateLogin(name),
			"ALTER LOGIN"             => new AlterLogin(name),
			"CREATE USER"             => new CreateUser(name),
			"ALTER USER"              => new AlterUser(name),
			"CREATE ROLE"             => new CreateRole(name),
			"ALTER ROLE"              => new AlterRole(name),
			"CREATE SERVER ROLE"      => new CreateServerRole(name),
			"ALTER SERVER ROLE"       => new AlterServerRole(name),
			"CREATE APPLICATION ROLE" => new CreateApplicationRole(name),
			"ALTER APPLICATION ROLE"  => new AlterApplicationRole(name),
			"CREATE SCHEMA"           => new SchemaDefinition(name),
			"ALTER SCHEMA"            => new AlterSchema(name),
			"ALTER AUTHORIZATION"     => new AlterAuthorization(name),
			"EXTERNAL DATA SOURCE"    => new ExternalDataSourceDefinition(name),
			"EXTERNAL FILE FORMAT"    => new ExternalFileFormatDefinition(name),
			"EXTERNAL LIBRARY"        => new ExternalLibraryDefinition(name),
			"EXTERNAL RESOURCE POOL"  => new ExternalResourcePoolDefinition(name),
			"RESOURCE POOL"           => new ResourcePoolDefinition(name),
			"WORKLOAD GROUP"          => new WorkloadGroupDefinition(name),
			"SERVER AUDIT"            => new ServerAuditDefinition(name),
			"AUDIT SPECIFICATION"     => new AuditSpecificationDefinition(name),
			"DATABASE AUDIT SPECIFICATION" => new DatabaseAuditSpecificationDefinition(name),
			"EVENT NOTIFICATION"      => new EventNotificationDefinition(name),
			"MESSAGE TYPE"            => new MessageTypeDefinition(name),
			"CONTRACT"                => new ContractDefinition(name),
			"QUEUE"                   => new QueueDefinition(name),
			"SERVICE"                 => new ServiceDefinition(name),
			"ROUTE"                   => new RouteDefinition(name),
			"REMOTE SERVICE BINDING"  => new RemoteServiceBindingDefinition(name),
			"BROKER PRIORITY"         => new BrokerPriorityDefinition(name),
			"ALTER RESOURCE GOVERNOR"    => new AlterResourceGovernor(name),
			"ALTER SERVER CONFIGURATION" => new AlterServerConfiguration(name),

			"FULLTEXT INDEX"             => new FullTextIndexDefinition(name),
			"ALTER FULLTEXT INDEX"       => new AlterFullTextIndex(name),
			"FULLTEXT CATALOG"           => new FullTextCatalogDefinition(name),
			"ALTER FULLTEXT CATALOG"     => new AlterFullTextCatalog(name),
			"FULLTEXT STOPLIST"          => new FullTextStopListDefinition(name),
			"ALTER FULLTEXT STOPLIST"    => new AlterFullTextStopList(name),
			"SEARCH PROPERTY LIST"       => new SearchPropertyListDefinition(name),
			"ALTER SEARCH PROPERTY LIST" => new AlterSearchPropertyList(name),

			"ASYMMETRIC KEY"                => new AsymmetricKeyDefinition(name),
			"ALTER ASYMMETRIC KEY"          => new AlterAsymmetricKey(name),
			"SYMMETRIC KEY"                 => new SymmetricKeyDefinition(name),
			"ALTER SYMMETRIC KEY"           => new AlterSymmetricKey(name),
			"CERTIFICATE"                   => new CertificateDefinition(name),
			"ALTER CERTIFICATE"             => new AlterCertificate(name),
			"MASTER KEY"                    => new MasterKeyDefinition(name),
			"ALTER MASTER KEY"              => new AlterMasterKey(name),
			"DATABASE ENCRYPTION KEY"       => new DatabaseEncryptionKeyDefinition(name),
			"ALTER DATABASE ENCRYPTION KEY" => new AlterDatabaseEncryptionKey(name),
			"COLUMN ENCRYPTION KEY"         => new ColumnEncryptionKeyDefinition(name),
			"ALTER COLUMN ENCRYPTION KEY"   => new AlterColumnEncryptionKey(name),
			"COLUMN MASTER KEY"             => new ColumnMasterKeyDefinition(name),
			"CREDENTIAL"                    => new CredentialDefinition(name),
			"DATABASE SCOPED CREDENTIAL"    => new DatabaseScopedCredentialDefinition(name),
			"SECURITY POLICY"               => new SecurityPolicyDefinition(name),
			_                               => throw Syntax.Unknown(what),
		};

	/// <summary>What is being done to a database, as the statement it is.</summary>
	public static Statement OfDatabase(string name, string action, Clause[]? settings, string? tail = null) =>
		OfDatabase(name, action, settings) is Definition made ? made with { Tail = Syntax.Tail(tail) } : OfDatabase(name, action, settings);

	static Statement OfDatabase(string name, string action, Clause[]? settings) =>
		action switch
		{
			"CREATE"               => new CreateDatabase(name, settings ?? Clause.None),
			"SET"                  => new AlterDatabaseSet(name, settings ?? Clause.None),
			"COLLATE"              => new AlterDatabaseCollate(name),
			"MODIFY NAME"          => new AlterDatabaseModifyName(name),
			"MODIFY FILEGROUP"     => new AlterDatabaseModifyFileGroup(name),
			"MODIFY FILE"          => new AlterDatabaseModifyFile(name),
			"MODIFY"               => new AlterDatabaseModify(name),
			"ADD FILEGROUP"        => new AlterDatabaseAddFileGroup(name),
			"ADD LOG FILE"         => new AlterDatabaseAddLogFile(name),
			"ADD FILE"             => new AlterDatabaseAddFile(name),
			"REMOVE FILEGROUP"     => new AlterDatabaseRemoveFileGroup(name),
			"REMOVE FILE"          => new AlterDatabaseRemoveFile(name),
			"REBUILD LOG"          => new AlterDatabaseRebuildLog(name),
			"PERFORM_CUTOVER"      => new AlterDatabasePerformCutover(name),
			_                      => throw Syntax.Unknown(action),
		};


	/// <summary>An alteration before the table it is applied to is known.</summary>
	public static AlterTable Altered(
		string action, Clause[]? elements, Clause[]? options = null, string? tail = null) =>
		new("", action, elements ?? Clause.None, options, Syntax.Tail(tail));

	/// <summary>A column altered by one word — <c>ADD SPARSE</c>, <c>DROP PERSISTED</c>.</summary>
	public static AlterTable Flagged(string name, string flag, Clause[]? options) =>
		new("", "ALTER COLUMN", [new Clause.ColumnDefinition(name, null, null, [Clause.Optioned(flag)])], options);

	/// <summary>The <c>DROP</c> the word names, which is what a <c>DROP</c> statement is.</summary>
	/// <remarks>
	/// The word arrives as it was written — the case the author used, and whatever spacing
	/// stood between its parts — so it is squared up before it is asked about. This and the
	/// grammar's own <c>DropKind</c> are two spellings of one catalogue and have to agree; where
	/// they have drifted apart this says so rather than quietly building the wrong node, which
	/// is a defect in this file and not in anybody's SQL.
	/// </remarks>
	public static Statement Dropped(
		string kind, Expression[]? some, string? tail = null, string? ifExists = null) =>
		Removed(kind, some) switch
		{
			Removal made => made with { Tail = Syntax.Tail(tail), IfExists = ifExists is not null },
			var other    => other,
		};

	static Statement Removed(string kind, Expression[]? some)
	{
		var names = some ?? Expression.None;

		return Syntax.Squared(kind) switch
		{
			"AGGREGATE"                    => new DropAggregate(names),
			"APPLICATION ROLE"             => new DropApplicationRole(names),
			"AVAILABILITY GROUP"           => new DropAvailabilityGroup(names),
			"BROKER PRIORITY"              => new DropBrokerPriority(names),
			"CERTIFICATE"                  => new DropCertificate(names),
			"COLUMN ENCRYPTION KEY"        => new DropColumnEncryptionKey(names),
			"COLUMN MASTER KEY"            => new DropColumnMasterKey(names),
			"CONTRACT"                     => new DropContract(names),
			"CREDENTIAL"                   => new DropCredential(names),
			"CRYPTOGRAPHIC PROVIDER"       => new DropCryptographicProvider(names),
			"DATABASE AUDIT SPECIFICATION" => new DropDatabaseAuditSpecification(names),
			"DATABASE SCOPED CREDENTIAL"   => new DropDatabaseScopedCredential(names),
			"DATABASE"                     => new DropDatabase(names),
			"DEFAULT"                      => new DropDefault(names),
			"ENDPOINT"                     => new DropEndpoint(names),
			"EXTERNAL DATA SOURCE"         => new DropExternalDataSource(names),
			"EXTERNAL FILE FORMAT"         => new DropExternalFileFormat(names),
			"EXTERNAL LANGUAGE"            => new DropExternalLanguage(names),
			"EXTERNAL MODEL"               => new DropExternalModel(names),
			"EXTERNAL RESOURCE POOL"       => new DropExternalResourcePool(names),
			"EXTERNAL TABLE"               => new DropExternalTable(names),
			"FULLTEXT CATALOG"             => new DropFulltextCatalog(names),
			"FULLTEXT STOPLIST"            => new DropFulltextStoplist(names),
			"FUNCTION"                     => new DropFunction(names),
			"LOGIN"                        => new DropLogin(names),
			"MESSAGE TYPE"                 => new DropMessageType(names),
			"PARTITION FUNCTION"           => new DropPartitionFunction(names),
			"PARTITION SCHEME"             => new DropPartitionScheme(names),
			"PROCEDURE"                    => new DropProcedure(names),
			"PROC"                         => new DropProcedure(names),
			"QUEUE"                        => new DropQueue(names),
			"REMOTE SERVICE BINDING"       => new DropRemoteServiceBinding(names),
			"RESOURCE POOL"                => new DropResourcePool(names),
			"ROLE"                         => new DropRole(names),
			"ROUTE"                        => new DropRoute(names),
			"RULE"                         => new DropRule(names),
			"SCHEMA"                       => new DropSchema(names),
			"SEARCH PROPERTY LIST"         => new DropSearchPropertyList(names),
			"SECURITY POLICY"              => new DropSecurityPolicy(names),
			"SEQUENCE"                     => new DropSequence(names),
			"SERVER AUDIT SPECIFICATION"   => new DropServerAuditSpecification(names),
			"SERVER AUDIT"                 => new DropServerAudit(names),
			"SERVER ROLE"                  => new DropServerRole(names),
			"SERVICE"                      => new DropService(names),
			"STATISTICS"                   => new DropStatistics(names),
			"SYNONYM"                      => new DropSynonym(names),
			"TABLE"                        => new DropTable(names),
			"TYPE"                         => new DropType(names),
			"USER"                         => new DropUser(names),
			"VIEW"                         => new DropView(names),
			"WORKLOAD CLASSIFIER"          => new DropWorkloadClassifier(names),
			"WORKLOAD GROUP"               => new DropWorkloadGroup(names),
			"XML SCHEMA COLLECTION"        => new DropXmlSchemaCollection(names),
			"ASYMMETRIC KEY"               => new DropAsymmetricKey(names),
			"SYMMETRIC KEY"                => new DropSymmetricKey(names),
			"ASSEMBLY"                     => new DropAssembly(names),
			"EXTERNAL LIBRARY"             => new DropExternalLibrary(names),
			"EVENT SESSION"                => new DropEventSession(names),
			"EVENT NOTIFICATION"           => new DropEventNotification(names),
			"FULLTEXT INDEX"               => new DropFulltextIndex(names),
			"INDEX"                        => new DropIndex(names),
			"SIGNATURE"                    => new DropSignature(names),
			"SENSITIVITY CLASSIFICATION"   => new DropSensitivityClassification(names),
			"TRIGGER"                      => new DropTrigger(names),
			"MASTER KEY"                   => new DropMasterKey(names),
			"DATABASE ENCRYPTION KEY"      => new DropDatabaseEncryptionKey(names),
			_ => throw new ArgumentOutOfRangeException(
				nameof(kind), kind,
				"The grammar reads this after DROP and the tree has no record for it."),
		};
	}
}

/// <summary>
/// What a client sends the server in one call: the statements of one batch of a script, and
/// the <c>GO</c> line that ended it as it was written — <c>GO</c>, or <c>GO 5</c> for a batch
/// sent five times — or null where nothing did, which is the last batch of a file.
/// </summary>
/// <remarks>
/// Not a statement and not a node of the other four kinds: a script is a client's idea, and
/// the server never sees the line a batch ends at. So it is a record of its own, holding the
/// statements, and it is what <c>ParseScript</c> hands back a list of.
/// </remarks>
public sealed record Batch(Statement[] Statements, string? Go = null);

/// <summary>
/// The groups Microsoft's page of the SET statements puts them in, as flags, so that a
/// statement whose list mixes them can say all of its groups.
/// </summary>
[Flags]
public enum SetCategory
{
	/// <summary>No setting.</summary>
	None = 0,

	/// <summary><c>DATEFIRST</c>, <c>DATEFORMAT</c>.</summary>
	DateAndTime = 1,

	/// <summary><c>DEADLOCK_PRIORITY</c>, <c>LOCK_TIMEOUT</c>.</summary>
	Locking = 2,

	/// <summary>The rest: a language, a table's identity, the FIPS flagger and others.</summary>
	Miscellaneous = 4,

	/// <summary><c>NOCOUNT</c>, <c>PARSEONLY</c>, <c>ROWCOUNT</c>, <c>TEXTSIZE</c> and the others that shape how a query runs.</summary>
	QueryExecution = 8,

	/// <summary>The ISO settings: <c>ANSI_NULLS</c>, <c>ANSI_PADDING</c>, <c>ANSI_WARNINGS</c> and theirs.</summary>
	IsoSettings = 16,

	/// <summary><c>STATISTICS IO</c>, <c>SHOWPLAN_XML</c>, <c>FORCEPLAN</c> and the others that report on a query.</summary>
	Statistics = 32,

	/// <summary><c>TRANSACTION ISOLATION LEVEL</c>, <c>XACT_ABORT</c>, <c>IMPLICIT_TRANSACTIONS</c>.</summary>
	Transactions = 64,
}

/// <summary>
/// What follows <c>SET</c> in a SET statement: one setting, in the group Microsoft's reference
/// puts it in, each group with the settings of its own shapes.
/// </summary>
/// <remarks>
/// A statement's list holds one form: switches turned on or off together, settings given
/// values, the statistics reported, the offsets returned — the engine refuses a switch among
/// values, or a variable among either. A switch is one setting to a node, so a list of them
/// may hold several groups, and the writer puts the one <c>ON</c> after them all again.
/// </remarks>
public abstract record SetExpression : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	/// <summary>The group Microsoft's page of the SET statements puts the setting in.</summary>
	public abstract SetCategory Category { get; }

	/// <summary>The date's settings.</summary>
	public abstract record DateAndTime : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.DateAndTime;

		/// <summary><c>SET DATEFIRST 7</c>: a constant, a word or a variable.</summary>
		public sealed record DateFirst(Expression Value) : DateAndTime;

		/// <summary><c>SET DATEFORMAT dmy</c>: a constant, a word or a variable.</summary>
		public sealed record DateFormat(Expression Value) : DateAndTime;
	}

	/// <summary>The locks' settings.</summary>
	public abstract record Locking : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.Locking;

		/// <summary><c>SET DEADLOCK_PRIORITY LOW</c>: a word, a number or a variable.</summary>
		public sealed record DeadlockPriority(Expression Value) : Locking;

		/// <summary><c>SET LOCK_TIMEOUT 1000</c>: a whole number, and nothing else.</summary>
		public sealed record LockTimeout(Expression Value) : Locking;
	}

	/// <summary>What shapes how a query runs.</summary>
	public abstract record QueryExecution : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.QueryExecution;

		/// <summary><c>SET NOCOUNT ON</c> and the other switches of the group.</summary>
		public sealed record Switch(string Option, bool On) : QueryExecution;

		/// <summary><c>SET ROWCOUNT 10</c>: a number that is not negative, or a variable.</summary>
		public sealed record RowCount(Expression Value) : QueryExecution;

		/// <summary><c>SET TEXTSIZE 2048</c>: a whole number.</summary>
		public sealed record TextSize(Expression Value) : QueryExecution;

		/// <summary><c>SET QUERY_GOVERNOR_COST_LIMIT 10</c>: a number.</summary>
		public sealed record QueryGovernorCostLimit(Expression Value) : QueryExecution;
	}

	/// <summary>The ISO settings, all of them switches.</summary>
	public abstract record IsoSettings : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.IsoSettings;

		/// <summary><c>SET ANSI_NULLS ON</c> and the others.</summary>
		public sealed record Switch(string Option, bool On) : IsoSettings;
	}

	/// <summary>What reports on a query.</summary>
	public abstract record Statistics : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.Statistics;

		/// <summary><c>SET SHOWPLAN_XML ON</c>, <c>SET FORCEPLAN ON</c>.</summary>
		public sealed record Switch(string Option, bool On) : Statistics;

		/// <summary>One word of <c>SET STATISTICS IO, TIME ON</c>: <c>IO</c>, <c>PROFILE</c>, <c>TIME</c> or <c>XML</c>.</summary>
		public sealed record Report(string Kind, bool On) : Statistics;
	}

	/// <summary>The transactions' settings.</summary>
	public abstract record Transactions : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.Transactions;

		/// <summary><c>SET XACT_ABORT ON</c> and the other switches of the group.</summary>
		public sealed record Switch(string Option, bool On) : Transactions;

		/// <summary>§19.4 <c>SET TRANSACTION ISOLATION LEVEL</c>, T-SQL's <c>TRAN</c> read as well.</summary>
		public sealed record IsolationLevel(string Level) : Transactions;
	}

	/// <summary>The rest.</summary>
	public abstract record Miscellaneous : SetExpression
	{
		/// <inheritdoc/>
		public override SetCategory Category => SetCategory.Miscellaneous;

		/// <summary>
		/// <c>SET QUOTED_IDENTIFIER ON</c>, <c>CONCAT_NULL_YIELDS_NULL</c>, <c>CURSOR_CLOSE_ON_COMMIT</c>,
		/// <c>NO_BROWSETABLE</c>, which no page describes and the engine knows, and Fabric's <c>RECOMMENDATIONS</c>.
		/// </summary>
		public sealed record Switch(string Option, bool On) : Miscellaneous;

		/// <summary><c>SET LANGUAGE us_english</c>: a constant, a name or a variable.</summary>
		public sealed record Language(Expression Value) : Miscellaneous;

		/// <summary><c>SET FIPS_FLAGGER 'FULL'</c>: a string, or <c>OFF</c>.</summary>
		public sealed record FipsFlagger(Expression Level) : Miscellaneous;

		/// <summary><c>SET CONTEXT_INFO 0x01</c>: a constant or a variable.</summary>
		public sealed record ContextInfo(Expression Value) : Miscellaneous;

		/// <summary><c>SET IDENTITY_INSERT t ON</c>.</summary>
		public sealed record IdentityInsert(string Table, bool On) : Miscellaneous;

		/// <summary>One keyword of <c>SET OFFSETS SELECT, FROM ON</c>.</summary>
		public sealed record Offset(string Keyword, bool On) : Miscellaneous;

		/// <summary><c>SET ERRLVL 1</c>, which no page describes and the engine reads.</summary>
		public sealed record ErrorLevel(Expression Value) : Miscellaneous;
	}

	/// <summary>Switches turned on or off together, each in the group its name is in.</summary>
	public static SetExpression[] Switches(string[] options, bool on)
	{
		var made = new SetExpression[options.Length];

		for (var at = 0; at < options.Length; at++)
			made[at] = Syntax.Squared(options[at]) switch
			{
				"ARITHABORT" or "ARITHIGNORE" or "FMTONLY" or "NOCOUNT" or "NOEXEC" or "NUMERIC_ROUNDABORT"
					or "PARSEONLY" or "RESULT_SET_CACHING"
					=> new QueryExecution.Switch(options[at], on),
				"ANSI_DEFAULTS" or "ANSI_NULL_DFLT_OFF" or "ANSI_NULL_DFLT_ON" or "ANSI_NULLS" or "ANSI_PADDING"
					or "ANSI_WARNINGS"
					=> new IsoSettings.Switch(options[at], on),
				"FORCEPLAN" or "SHOWPLAN_ALL" or "SHOWPLAN_TEXT" or "SHOWPLAN_XML"
					=> new Statistics.Switch(options[at], on),
				"IMPLICIT_TRANSACTIONS" or "REMOTE_PROC_TRANSACTIONS" or "XACT_ABORT"
					=> new Transactions.Switch(options[at], on),
				_ => new Miscellaneous.Switch(options[at], on),
			};

		return made;
	}

	/// <summary>The words of <c>SET STATISTICS …</c>, each a node.</summary>
	public static SetExpression[] Reported(string[] kinds, bool on)
	{
		var made = new SetExpression[kinds.Length];

		for (var at = 0; at < kinds.Length; at++)
			made[at] = new Statistics.Report(kinds[at], on);

		return made;
	}

	/// <summary>The keywords of <c>SET OFFSETS …</c>, each a node.</summary>
	public static SetExpression[] Offsets(string[] keywords, bool on)
	{
		var made = new SetExpression[keywords.Length];

		for (var at = 0; at < keywords.Length; at++)
			made[at] = new Miscellaneous.Offset(keywords[at], on);

		return made;
	}
}

/// <summary>
/// §7 the table level: what produces rows. A statement holds one, an expression holds one,
/// and a <c>FROM</c> clause holds the <see cref="TableReference"/>s it is read over.
/// </summary>
public abstract record Query : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	/// <summary>§7.12 <c>SELECT</c>, and the clauses under it.</summary>
	/// <remarks>
	/// One record for the whole of a query specification and its table expression, because
	/// the clauses are optional rather than alternative: what tells one query from another is
	/// which of them are empty. <see cref="From"/> holds the table references the standard
	/// writes as a comma list, which is a cross join said the older way.
	/// </remarks>
	public sealed record Specification(
		string? Quantifier,
		Clause? Top,
		Clause[] Columns,
		Clause? Into,
		TableReference[] From,
		Expression? Where,
		Clause? GroupBy,
		Expression? Having,
		Clause[]? Windows = null) : Query;

	/// <summary>§7.3 <c>VALUES (…), (…)</c> — a table written out.</summary>
	public sealed record TableValueConstructor(Expression[] Rows) : Query;

	/// <summary>§7.13 a query in brackets, for <see cref="Expression.Parenthesized"/>'s reason.</summary>
	public sealed record Parenthesized(Query Query) : Query;

	/// <summary>
	/// T-SQL's query with an order and a shape of its own, where no statement holds them: a
	/// subquery's <c>ORDER BY</c> under its <c>TOP</c>, a derived table's <c>FOR XML</c>, an
	/// <c>INSERT … SELECT … ORDER BY</c>, a named query's order.
	/// </summary>
	/// <remarks>
	/// It wraps the query rather than being a field of a specification because the order is
	/// the whole query's: a <c>UNION</c> has one <c>ORDER BY</c>, after the last of its parts.
	/// </remarks>
	public sealed record Ordered(Query Query, Clause.OrderBy? By, Clause? For) : Query;

	/// <summary>§7.4 <c>TABLE t</c>, which is every column and every row of one table.</summary>
	public sealed record ExplicitTable(string Name) : Query;

	/// <summary>§7.13 <c>UNION</c>, and whether it keeps duplicates.</summary>
	public sealed record Union(Query Left, Query Right, bool All) : Query;

	/// <summary>§7.13 <c>EXCEPT</c>, and whether it keeps duplicates.</summary>
	public sealed record Except(Query Left, Query Right, bool All) : Query;

	/// <summary>§7.13 <c>INTERSECT</c>, and whether it keeps duplicates.</summary>
	public sealed record Intersect(Query Left, Query Right, bool All) : Query;

	/// <summary>T-SQL's <c>INSERT … DEFAULT VALUES</c>: a row of nothing but defaults.</summary>
	public sealed record DefaultValues : Query;

	/// <summary>T-SQL's <c>BULK INSERT</c>: a file standing where a query does.</summary>
	public sealed record FromFile(Expression File, Clause[]? Options = null) : Query;

	/// <summary>
	/// T-SQL's <c>INSERT … EXEC</c>: a procedure standing where a query stands.
	/// </summary>
	/// <remarks>
	/// An aggregation and not a kind of statement — what makes the rows here is a statement,
	/// and what the <c>INSERT</c> needs is something that makes rows, so the query level holds
	/// the statement rather than the two hierarchies meeting.
	/// </remarks>
	public sealed record FromExecute(Statement.Execute Execute) : Query;

	/// <summary>Whether a set quantifier asked for distinct rows, for a reader that wants to know.</summary>
	public static bool IsDistinct(string? quantifier) =>
		quantifier is not null && (quantifier[0] | 0x20) == 'd';

	/// <summary>Which set operator was written, and whether it keeps duplicates.</summary>
	public static Query Combined(string operatorText, string? all, Query left, Query right) =>
		(operatorText[0] | 0x20) switch
		{
			'u' => new Union(left, right, all is not null),
			'e' => new Except(left, right, all is not null),
			_   => new Intersect(left, right, all is not null),
		};
}

/// <summary>§6 the value level, and §8 the predicates: what stands where a value does.</summary>
public abstract record Expression : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	// ---- §6.39 the boolean tower ---------------------------------------------------------------

	/// <summary><c>OR</c>.</summary>
	public sealed record Or(Expression Left, Expression Right) : Expression;

	/// <summary><c>AND</c>.</summary>
	public sealed record And(Expression Left, Expression Right) : Expression;

	/// <summary><c>NOT</c>.</summary>
	public sealed record Not(Expression Operand) : Expression;

	/// <summary>
	/// <c>x IS NOT TRUE</c> and its fellows: what is tested, and what it is tested against.
	/// </summary>
	public sealed record IsTruth(Expression Operand, bool Negated, SqlTruth Truth) : Expression;

	// ---- §6.30 the arithmetic tower --------------------------------------------------------------
	//
	// A record per operator, because the standard writes a production per operator:
	// <numeric value expression> ::= … | <numeric value expression> <plus sign> <term>.

	/// <summary><c>+</c>.</summary>
	public sealed record Add(Expression Left, Expression Right) : Expression;

	/// <summary><c>-</c>.</summary>
	public sealed record Subtract(Expression Left, Expression Right) : Expression;

	/// <summary>§6.31 <c>||</c>, and T-SQL's <c>+</c> over strings, which is the same node.</summary>
	public sealed record Concatenate(Expression Left, Expression Right) : Expression;

	/// <summary><c>*</c>.</summary>
	public sealed record Multiply(Expression Left, Expression Right) : Expression;

	/// <summary><c>/</c>.</summary>
	public sealed record Divide(Expression Left, Expression Right) : Expression;

	/// <summary>A unary <c>-</c>.</summary>
	public sealed record Negate(Expression Operand) : Expression;

	/// <summary>A unary <c>+</c>, which the standard keeps and which changes nothing.</summary>
	public sealed record Plus(Expression Operand) : Expression;

	/// <summary>T-SQL's <c>%</c>, the remainder, as strong as <c>*</c> and <c>/</c> and read left to right with them.</summary>
	public sealed record Modulo(Expression Left, Expression Right) : Expression;

	/// <summary>
	/// T-SQL's <c>&amp;</c>, as weak as <c>+</c> and <c>-</c> and read left to right with them:
	/// the engine answers <c>2 + 5 &amp; 4</c> with 4 and <c>5 &amp; 4 + 2</c> with 6.
	/// </summary>
	public sealed record BitwiseAnd(Expression Left, Expression Right) : Expression;

	/// <summary>T-SQL's <c>|</c>, as weak as <c>+</c>.</summary>
	public sealed record BitwiseOr(Expression Left, Expression Right) : Expression;

	/// <summary>T-SQL's <c>^</c>, as weak as <c>+</c>.</summary>
	public sealed record BitwiseXor(Expression Left, Expression Right) : Expression;

	/// <summary>T-SQL's <c>~</c>, which binds as a sign does: <c>~2 * 3</c> is -9.</summary>
	public sealed record BitwiseNot(Expression Operand) : Expression;

	// ---- §8 the predicates ------------------------------------------------------------------------
	//
	// A record each, with the operands named rather than numbered. The one word that survives
	// is the comparison operator, and only because <comp op> is a production of its own.

	/// <summary>§8.2 <c>a = b</c> and its five fellows.</summary>
	public sealed record Comparison(
		Expression Left, SqlComparison Operator, Expression Right) : Expression;

	/// <summary>§8.9 <c>a = ALL (…)</c> — a comparison against every row of a subquery.</summary>
	public sealed record Quantified(
		Expression Left, SqlComparison Operator, string Quantifier, Query Query) : Expression;

	/// <summary>§8.3 <c>a BETWEEN low AND high</c>.</summary>
	public sealed record Between(
		Expression Value, bool Negated, Expression Low, Expression High) : Expression;

	/// <summary>§8.4 <c>a IN (…)</c>, whose right side is a row of values or a subquery.</summary>
	public sealed record In(Expression Value, bool Negated, Expression Source) : Expression;

	/// <summary>§8.5 <c>a LIKE p ESCAPE e</c>.</summary>
	public sealed record Like(
		Expression Value, bool Negated, Expression Pattern, Expression? Escape) : Expression;

	/// <summary>§8.7 <c>a IS NULL</c>.</summary>
	public sealed record IsNull(Expression Value, bool Negated) : Expression;

	/// <summary>§8.10 <c>EXISTS (…)</c>.</summary>
	public sealed record Exists(Query Query) : Expression;

	/// <summary>§8.11 <c>UNIQUE (…)</c>.</summary>
	public sealed record Unique(Query Query) : Expression;

	/// <summary>§8.13 <c>a MATCH UNIQUE PARTIAL (…)</c>, and the words it may be qualified by.</summary>
	public sealed record Match(Expression Value, string? Qualifier, Query Query) : Expression;

	/// <summary>
	/// T-SQL's graph <c>MATCH (…)</c>: the drawing inside the brackets, as written.
	/// </summary>
	/// <remarks>
	/// A pattern is a language of its own — nodes, arrows, <c>SHORTEST_PATH</c>, its
	/// quantifiers — and the grammar reads every bit of it to say whether it is one. What the
	/// tree keeps is the text, for <see cref="Clause.Hint"/>'s reason: it loses nothing and
	/// claims nothing, and a reader who wants the drawing taken apart is asking the graph's
	/// question rather than the language's.
	/// </remarks>
	public sealed record GraphMatch(string Pattern) : Expression;

	/// <summary>§8.15 <c>a OVERLAPS b</c>, whose two sides are rows.</summary>
	public sealed record Overlaps(Expression Left, Expression Right) : Expression;

	/// <summary>
	/// SQL:1999's <c>&lt;distinct predicate&gt;</c> — <c>a IS NOT DISTINCT FROM b</c>, which
	/// is a comparison that calls two nulls equal.
	/// </summary>
	public sealed record IsDistinctFrom(
		Expression Left, bool Negated, Expression Right) : Expression;

	// ---- §6 the rest of the value level ------------------------------------------------------------

	/// <summary>
	/// A function, a set function or a cast: the name as written, its arguments, and the one
	/// word a few of them carry — <c>DISTINCT</c>, a datetime field, a trim specification, or
	/// the type a cast names.
	/// </summary>
	public sealed record RoutineInvocation(
		string Name, Expression[] Arguments, string? Word = null) : Expression;

	/// <summary>
	/// §6.12 a <c>CASE</c>, simple where it has an operand and searched where it does not.
	/// </summary>
	public sealed record Case(Expression? Operand, Clause[] Whens, Expression? Else) : Expression;

	/// <summary>§6.7 a column reference, as written, dots and all.</summary>
	public sealed record ColumnReference(string Text) : Expression;

	/// <summary>
	/// §5.3 a literal, a parameter, or one of the words that stand where a value does —
	/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
	/// </summary>
	public sealed record Literal(SqlLiteralKind Kind, string Text) : Expression;

	/// <summary>§7.1 a row of several values, <c>(a, b)</c>.</summary>
	public sealed record RowValueConstructor(Expression[] Values) : Expression;

	/// <summary>
	/// The <c>ORDER (c1 ASC, c2 DESC) UNIQUE</c> a bulk rowset is declared to arrive in,
	/// which is an argument of <c>OPENROWSET</c> and not a clause of the query.
	/// </summary>
	public sealed record RowsetOrder(Clause[] By, bool IsUnique) : Expression;

	/// <summary>
	/// SQL:2003's <c>&lt;window function&gt;</c>: a call, and the window it is computed over.
	/// </summary>
	/// <remarks>
	/// <see cref="Within"/> holds what may stand between the call and its window —
	/// <c>IGNORE NULLS</c>, <c>WITHIN GROUP (ORDER BY …)</c> — as the words it was written
	/// as, for <see cref="Clause.Hint"/>'s reason: it says how the call sees its rows.
	/// </remarks>
	public sealed record WindowFunction(
		Expression Function, string? Within, Clause? Over) : Expression;

	/// <summary>
	/// A member of a value, or a method called on one — <c>a.b</c>, <c>a::b</c>,
	/// <c>a.f(1)</c>. T-SQL writes both separators and they are not the same text.
	/// </summary>
	public sealed record Member(
		Expression Of, string By, string Name, Expression[]? Arguments) : Expression;

	/// <summary>
	/// §6.11 a value and the collation it is compared under, which says how it is compared
	/// rather than what it is — and is part of the text all the same.
	/// </summary>
	public sealed record Collated(Expression Value, string Collation) : Expression;

	/// <summary>
	/// An argument with a word in front of it, which several of T-SQL's table-valued
	/// functions write — <c>BULK 'f'</c>, <c>CHANGES t</c>, <c>LANGUAGE 1033</c>.
	/// </summary>
	public sealed record Prefixed(string Word, Expression Value) : Expression;

	/// <summary>
	/// A value and the unit written after it — <c>10 MINUTES</c>, <c>50 PERCENT</c>,
	/// <c>4 GB</c> — where an option or a sample says how much of what.
	/// </summary>
	public sealed record Measured(Expression Value, string Unit) : Expression;

	/// <summary>
	/// A value with the words written after it that say how the engine is to treat it,
	/// kept as written — <c>GROUP BY c WITH (DISTRIBUTED_AGG)</c>, <c>JSON_ARRAY(… NULL ON
	/// NULL)</c>, <c>JSON_OBJECT(… RETURNING json)</c>.
	/// </summary>
	public sealed record Hinted(Expression Value, string Words) : Expression;

	/// <summary>
	/// <c>key : value</c> in a JSON constructor — the one place T-SQL joins two values with a
	/// colon.
	/// </summary>
	public sealed record JsonPair(Expression Key, Expression Value) : Expression;

	/// <summary>
	/// An ODBC escape, <c>{ FN … }</c>, <c>{ d '…' }</c>, <c>{ ts '…' }</c>: the word and what
	/// stands after it, which SQL Server reads and the tree keeps in its braces.
	/// </summary>
	/// <param name="Called">
	/// Whether the brackets of a call were written. <c>{ fn current_time }</c> and <c>{ fn
	/// current_time () }</c> are the same function reached two ways, and inside the braces
	/// the name is ODBC's — nothing here may supply the brackets or take them away.
	/// </param>
	public sealed record OdbcEscape(string Kind, Expression Value, bool Called = false) : Expression;

	/// <summary>
	/// An argument given by name rather than by position — <c>@p = 1</c> in an
	/// <c>EXECUTE</c>, <c>FORMATFILE = '…'</c> in a rowset function.
	/// </summary>
	public sealed record NamedArgument(string Name, Expression Value) : Expression;

	/// <summary>
	/// One argument written as several pieces with semicolons between them — <c>OPENROWSET
	/// ('SQLOLEDB', 'server'; 'user'; 'password', …)</c>, that function's oldest spelling and
	/// the one place T-SQL joins two values with a semicolon.
	/// </summary>
	public sealed record Pieced(Expression[] Parts) : Expression;

	/// <summary>
	/// §6.28 <c>&lt;parenthesized value expression&gt;</c>, and §8.1's boolean one: brackets
	/// somebody wrote.
	/// </summary>
	/// <remarks>
	/// The standard has a production for it and so does the tree, which is not the same
	/// reason it is here. A tree that records only the brackets meaning needs cannot say
	/// whether <c>(a) + b</c> or <c>a + b</c> was written, and a formatter that rewrites one
	/// into the other is changing text nobody asked it to change. Everything the author typed
	/// survives; how it is laid out is the formatter's own business.
	/// </remarks>
	public sealed record Parenthesized(Expression Value) : Expression;

	/// <summary>
	/// §7.15 a subquery standing where a value does — the standard's scalar and row subqueries,
	/// which differ by how many columns they return and not by how they are written.
	/// </summary>
	public sealed record Subquery(Query Query) : Expression;

	// ---- how a parser makes these -----------------------------------------------------------------

	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	public static readonly Expression[] None = [];

	/// <summary>The two words that stand where a value does.</summary>
	/// <remarks>
	/// One node each rather than one shared node, which is what they used to be. A shared node
	/// cannot say where it was written — every <c>NULL</c> in a statement would be the same
	/// object, and the first of them to be offered a range would keep it for all the rest.
	/// A literal is two fields and the saving was never the point.
	/// </remarks>
	public static Expression NullValue => new Literal(SqlLiteralKind.Null, "NULL");

	/// <inheritdoc cref="NullValue"/>
	public static Expression DefaultValue => new Literal(SqlLiteralKind.Default, "DEFAULT");

	/// <summary>
	/// An additive operator and its two operands, as the node the operator names — T-SQL's
	/// bitwise three among them, which bind as weakly.
	/// </summary>
	public static Expression Additive(string operatorText, Expression left, Expression right) =>
		operatorText switch
		{
			"+" => new Add(left, right),
			"-" => new Subtract(left, right),
			"&" => new BitwiseAnd(left, right),
			"|" => new BitwiseOr(left, right),
			"^" => new BitwiseXor(left, right),
			_   => new Concatenate(left, right),
		};

	/// <summary>Likewise for the multiplicative operators, T-SQL's <c>%</c> among them.</summary>
	public static Expression Multiplicative(string operatorText, Expression left, Expression right) =>
		operatorText switch
		{
			"*" => new Multiply(left, right),
			"/" => new Divide(left, right),
			_   => new Modulo(left, right),
		};

	/// <summary>Likewise for the sign in front of one operand, T-SQL's <c>~</c> among them.</summary>
	public static Expression Signed(string sign, Expression operand) =>
		sign switch
		{
			"-" => new Negate(operand),
			"~" => new BitwiseNot(operand),
			_   => new Plus(operand),
		};

	/// <summary>
	/// The left operand written into the tail the predicate was read as.
	/// </summary>
	/// <remarks>
	/// The row is read once for all the predicates that begin with one, so the tail is built
	/// without it and the slot it left is filled here. A <c>with</c> rather than a write: the
	/// tail was built with its left side null and no one has seen it, so the copy costs one
	/// allocation and the array the operands used to live in costs none.
	/// </remarks>
	public static Expression Predicated(Expression left, Expression tail) =>
		tail switch
		{
			Comparison c => c with { Left  = left },
			Quantified q => q with { Left  = left },
			Between    b => b with { Value = left },
			In         i => i with { Value = left },
			Like       l => l with { Value = left },
			IsNull     n => n with { Value = left },
			Match      m => m with { Value = left },
			Overlaps   o => o with { Left  = left },
			_            => throw new ArgumentOutOfRangeException(
				nameof(tail), tail,
				"The grammar read a predicate tail this method does not know how to complete."),
		};
}

/// <summary>
/// §7.6 what a <c>FROM</c> clause is read over: a table by name, a query standing where one
/// does, and the joins between them.
/// </summary>
public abstract record TableReference : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	/// <summary>
	/// §7.6 a table named, the name it is known by there, and the names its columns are given.
	/// </summary>
	/// <remarks>
	/// The parts in the order the reference writes them: the table, which version of it,
	/// whether it is read as a graph's path, what it is called here, the names its columns
	/// are given, how much of it to read, and how to read it. <c>FOR PATH</c> is a mark on
	/// the table rather than a clause: it takes nothing, and it makes the alias an ordered
	/// collection that a <c>SHORTEST_PATH</c> may repeat.
	/// </remarks>
	public sealed record Named(
		string Table, Clause? SystemTime, bool ForPath, string? Name, string[]? Columns,
		Clause? Sample, Clause[] Hints) : TableReference;

	/// <summary>
	/// T-SQL's <c>PIVOT</c>: a source, the aggregate to turn its rows into columns with,
	/// the column whose values name them, and which of those values to keep.
	/// </summary>
	public sealed record Pivot(
		TableReference Of, Expression Aggregate, string For, string[] In,
		string? Name) : TableReference;

	/// <summary>T-SQL's <c>UNPIVOT</c>, which is the same thing said backwards.</summary>
	public sealed record Unpivot(
		TableReference Of, string Value, string For, string[] In, string? Name) : TableReference;

	/// <summary>
	/// §7.6 a query standing where a table does, T-SQL's <c>FOR PATH</c> after it as after a
	/// table named.
	/// </summary>
	public sealed record Derived(Query Query, bool ForPath, string? Name, string[]? Columns) : TableReference;

	/// <summary>
	/// §7.6 a function standing where a table does — T-SQL's rowset functions, and any
	/// table-valued function called in a <c>FROM</c> clause.
	/// </summary>
	public sealed record FunctionCall(
		Expression.RoutineInvocation Function, string? Name, string[]? Columns,
		Clause[] Schema) : TableReference;

	/// <summary>§7.6 a source in brackets, for <see cref="Expression.Parenthesized"/>'s reason.</summary>
	public sealed record Parenthesized(TableReference Of) : TableReference;

	/// <summary>
	/// ODBC's outer-join escape, <c>{ OJ t1 LEFT JOIN t2 ON … }</c>: the braces every driver
	/// has written since before this language had a standard, and which SQL Server still
	/// reads. What is inside them is an ordinary join, and the braces are kept because they
	/// were written.
	/// </summary>
	public sealed record OdbcJoin(TableReference Of) : TableReference;

	/// <summary>§7.7 two sources and the join between them.</summary>
	/// <remarks>
	/// <see cref="On"/> where the join was qualified by a condition, <see cref="Using"/> where
	/// it named columns, and neither for a cross or a natural join.
	/// </remarks>
	/// <param name="Hint">T-SQL's join hint, <c>HASH</c>, <c>LOOP</c>, <c>MERGE</c>, …, written before the <c>JOIN</c>.</param>
	public sealed record Joined(
		SqlJoin Kind, bool Outer, bool Natural, TableReference Left, TableReference Right,
		Expression? On = null, string[]? Using = null, string? Hint = null) : TableReference;

	/// <summary>What a clause with no sources is handed, once rather than per call.</summary>
	public static readonly TableReference[] None = [];

	/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
	public static Joined Joining(
		string? kind, string? natural, TableReference left, TableReference right,
		Expression? on, string[]? columns, string? hint = null) =>
		new(Syntax.Joined(kind), Syntax.Outer(kind), natural is not null, left, right, on, columns, hint);
}

/// <summary>
/// The pieces a statement, a query or an expression is made of that are none of the three —
/// what the standard writes as a clause, a specification or a definition of one element.
/// </summary>
public abstract record Clause : ISqlSpan
{
	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span { get; private set; }

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length) => Span = new SqlSpan(at, length);

	/// <summary>§7.12 one entry of a select list: what it is, and what it is called.</summary>
	public sealed record DerivedColumn(Expression Value, string? Name) : Clause;

	/// <summary>§7.12 <c>*</c>, or <c>t.*</c> — which is no column, so it is not one.</summary>
	public sealed record QualifiedAsterisk(string? Qualifier) : Clause;

	/// <summary>§10.10 one <c>ORDER BY</c> entry: what to sort by, and which way.</summary>
	/// <remarks>
	/// <see cref="Order"/> is what was written and not what it means: the standard makes
	/// ascending the default, and a statement that says so and one that does not are two
	/// different texts.
	/// </remarks>
	public sealed record SortSpecification(Expression Value, SqlOrder Order) : Clause;

	/// <summary>
	/// §10.10 the whole <c>ORDER BY</c>, with the two clauses the reference writes inside it:
	/// how many rows to step over, and how many to take.
	/// </summary>
	public sealed record OrderBy(Clause[] By, Expression? Offset, Expression? Fetch) : Clause;

	/// <summary>
	/// T-SQL's <c>TOP (n) PERCENT WITH TIES</c> — how many rows a query specification hands
	/// back, which the standard says with <see cref="OrderBy.Fetch"/> instead.
	/// </summary>
	public sealed record Top(Expression Value, bool Percent, string? With) : Clause;

	/// <summary>
	/// §7.9 <c>GROUP BY</c>: what the rows are grouped by, and the two things T-SQL writes
	/// around the list — <c>ALL</c> in front and <c>WITH CUBE</c> or <c>WITH ROLLUP</c> after.
	/// </summary>
	public sealed record GroupBy(bool All, Expression[] By, string? With) : Clause;

	/// <summary>
	/// SQL:2003's <c>&lt;window specification&gt;</c>: which rows a call sees, and in what
	/// order.
	/// </summary>
	/// <remarks>
	/// <see cref="Frame"/> is the words <c>ROWS BETWEEN 1 PRECEDING AND CURRENT ROW</c> were
	/// written as. A frame is a language of its own and says how far either side of the
	/// current row the window reaches; the text loses nothing and claims nothing.
	/// </remarks>
	public sealed record Window(
		string? Name, Expression[] PartitionBy, Clause? By, string? Frame) : Clause;

	/// <summary>
	/// One entry of a query's <c>WINDOW</c> clause: the name, and the window it stands for —
	/// whose own <see cref="Window.Name"/> is the window it is built on, where it is.
	/// </summary>
	public sealed record WindowDefinition(string Name, Clause Specification) : Clause;

	/// <summary>
	/// T-SQL's <c>SELECT … INTO t ON filegroup</c>: the table the rows are written to
	/// instead of coming back, and where its pages are to live.
	/// </summary>
	public sealed record Into(string Table, string? On) : Clause;

	/// <summary>
	/// T-SQL's <c>FOR SYSTEM_TIME</c>: which version of a temporal table is being read, and
	/// the one or two times that say which.
	/// </summary>
	public sealed record SystemTime(string Kind, Expression[] At) : Clause;

	/// <summary>
	/// T-SQL's <c>TABLESAMPLE</c>: how much of a table to read, in rows or in percent, and
	/// the seed that makes the answer the same twice.
	/// </summary>
	public sealed record TableSample(
		bool System, Expression Value, string? Unit, Expression? Repeatable) : Clause;

	/// <summary>
	/// One column of the schema a rowset function is read under —
	/// <c>OPENJSON (…) WITH (c INT '$.a.b')</c>.
	/// </summary>
	/// <remarks>
	/// <see cref="Type"/> is null where the whole schema was named rather than written out,
	/// which is <c>OPENXML</c>'s spelling: <c>WITH tablename</c>.
	/// </remarks>
	public sealed record JsonColumn(
		string Name, string? Type, string? Path, bool AsJson) : Clause;

	/// <summary>§7.17 a named query, written in front of the statement that uses it.</summary>
	public sealed record CommonTableExpression(
		string Name, string[]? Columns, Query Query) : Clause;

	/// <summary>
	/// T-SQL's <c>FOR XML</c>, <c>FOR JSON</c> and <c>FOR BROWSE</c>: what shape the rows come
	/// back in rather than what they are.
	/// </summary>
	public sealed record For(string Kind, string[] Options) : Clause;

	/// <summary>One hint, as it was written.</summary>
	/// <remarks>
	/// A hint is a language of its own — <c>OPTIMIZE FOR (@v = 20)</c>,
	/// <c>TABLE HINT (t, FORCESEEK (ix (a, b)))</c>, <c>MAXDOP 2</c> — and every one of them
	/// says how a statement is to be run rather than what it means. The text is what the tree
	/// keeps, which loses nothing and claims nothing: a reader who wants a hint taken apart is
	/// asking the engine's question, not the language's.
	/// </remarks>
	public sealed record Hint(string Text) : Clause;

	/// <summary>
	/// One option of the many lists T-SQL writes them in: its name, what it is set to where
	/// it is set to anything, the options nested in it where it holds a list of its own, and
	/// the partitions it applies to where it says which.
	/// </summary>
	/// <remarks>
	/// <c>WITH (DATA_COMPRESSION = PAGE ON PARTITIONS (1))</c>, <c>SET (LOCK_ESCALATION =
	/// AUTO)</c>, <c>MASKED WITH (FUNCTION = 'default()')</c>, and the settings of
	/// <c>ALTER DATABASE</c>. Which names there are is the grammar's catalogue and not the
	/// tree's, so one node holds them all: what an option means is the engine's question. <see cref="Bare"/> says the
	/// value stood after the name with no <c>=</c> between — <c>SET ENCRYPTION ON</c>, <c>WITH
	/// PAD_INDEX ON</c> — which is a spelling the tree keeps because some statements accept
	/// only that one.
	/// </remarks>
	public sealed record Option(
		string Name, Expression? Value, Clause[] Options, string? Partitions = null, bool Bare = false) : Clause;

	/// <summary>
	/// Where a table or an index is put: the word that says which placement, the filegroup
	/// or partition scheme, and the column a partition scheme is applied on.
	/// </summary>
	public sealed record Placement(string Kind, string Target, string[]? Columns) : Clause;

	/// <summary>
	/// T-SQL's assignment written in a select list — <c>SELECT @a += 1</c>, which takes the
	/// value rather than returning it and is not a column however much it looks like one.
	/// </summary>
	public sealed record VariableAssignment(
		string Variable, string Operator, Expression Value) : Clause;

	/// <summary>§6.12 one <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
	public sealed record When(Expression Test, Expression Result) : Clause;

	/// <summary>
	/// §14.14 one entry of a <c>SET</c>: what is assigned, the operator it was assigned with
	/// where that was not a plain <c>=</c>, and the value.
	/// </summary>
	/// <param name="Operator">The assignment as written — <c>=</c>, <c>+=</c>, …; empty for <c>.WRITE (…)</c>, whose value is the call's arguments.</param>
	/// <param name="Through">The column of <c>SET @v = column op= value</c>, which assigns both.</param>
	public sealed record Set(string Target, string? Operator, Expression Value, string? Through = null) : Clause;

	/// <summary>
	/// T-SQL's <c>OUTPUT</c>: what is written out, the table it goes into where it goes
	/// anywhere, that table's columns, and a second <c>OUTPUT</c> after it where there is one.
	/// </summary>
	public sealed record Output(
		Clause[] Items, TableReference? Target = null, string[]? Columns = null, Clause? Next = null) : Clause;

	/// <summary>
	/// T-SQL's <c>EXECUTE ('…') AS USER = 'u'</c>: whom a string is run as, <c>LOGIN</c> or
	/// <c>USER</c>, and the name, which is a string and nothing else.
	/// </summary>
	public sealed record ExecutionContext(string Kind, Expression Name) : Clause;

	/// <summary>
	/// §14.12 one arm of a merge: whether it fired on a match, which side the match was missing
	/// from, what else had to be true, and what to do.
	/// </summary>
	public sealed record MergeWhen(
		bool OnMatch, string? By, Expression? Condition, Statement Action) : Clause;

	/// <summary>
	/// One variable: its name, the type as written, and what it was given to start with.
	/// </summary>
	/// <param name="As">
	/// Whether <c>AS</c> stood between the name and the type. It is optional everywhere and
	/// only a table variable needs it remembered: <c>DECLARE @v INT</c> and <c>DECLARE @v AS
	/// INT</c> are read and written as one statement, <c>DECLARE @v TABLE (…)</c> and
	/// <c>DECLARE @v AS TABLE (…)</c> as two.
	/// </param>
	public sealed record VariableDeclaration(
		string Name, string? Type, Expression? Value, Clause[]? Elements = null, string? Nullability = null,
		bool As = false) : Clause;

	/// <summary>
	/// What a cursor is: the words it was declared with, the query it is over, and whether its
	/// rows may be changed through it.
	/// </summary>
	/// <param name="Before">
	/// The standard's words before <c>CURSOR</c>, <c>INSENSITIVE</c> and <c>SCROLL</c>. Empty in
	/// T-SQL's form, and so always in a cursor a variable is set to.
	/// </param>
	/// <param name="Options">T-SQL's words after <c>CURSOR</c>, empty in the standard's form.</param>
	/// <param name="With">The common table expressions, or the XML namespaces, before the query.</param>
	/// <param name="Query">The query, with its order where it has one.</param>
	/// <param name="Access">
	/// What may be done through it, <see cref="CursorFor"/>, where that was written.
	/// </param>
	/// <param name="Hints">The query's <c>OPTION (…)</c>.</param>
	/// <param name="AccessFirst">
	/// Whether the <c>FOR</c> stood before the <c>OPTION</c>. The engine reads the two either way
	/// round, and the text says which.
	/// </param>
	public sealed record CursorDefinition(
		string[] Before, string[] Options, Clause[] With, Query Query, Clause? Access, Clause[] Hints,
		bool AccessFirst = false) : Clause;

	/// <summary><c>FOR READ ONLY</c>, or <c>FOR UPDATE</c> and the columns it is kept to.</summary>
	public sealed record CursorFor(bool Update, string[]? Columns = null) : Clause;

	/// <summary>
	/// One parameter of a routine: its name, its type, and its default — and the words around
	/// them, <c>VARYING</c> and the nullability before the default, <c>OUTPUT</c> and
	/// <c>READONLY</c> after it.
	/// </summary>
	public sealed record ParameterDeclaration(
		string Name, string? Type, Expression? Value,
		string? Nullability = null, bool Varying = false, string[]? Ways = null) : Clause;

	/// <summary>
	/// §11.4 one column: its name, the type as written, the expression where it is computed
	/// rather than stored, and everything said about it after that — its options, its
	/// constraints, and the index written on it — in the order it was said.
	/// </summary>
	public sealed record ColumnDefinition(
		string Name, string? Type, Expression? Computed, Clause[] Options) : Clause;

	/// <summary>
	/// One thing said about a column after its type that is not a constraint: which thing
	/// (<c>SPARSE</c>, <c>NOT NULL</c>, <c>COLLATE</c>, <c>IDENTITY</c>, <c>MASKED</c>, …), what
	/// it was given in brackets or after it, the options where it carries a <c>WITH</c>, and
	/// the constraint name where the language lets a nullability have one.
	/// </summary>
	public sealed record ColumnOption(
		string Kind, Expression[] Arguments, Clause[] Options, string? ConstraintName) : Clause;

	/// <summary>
	/// §11.6 a constraint or an index, written on a column or on the table: its name where it
	/// was given one, which kind it is, and the columns it names — and everything T-SQL
	/// writes after those, each where it was written and nothing where it was not.
	/// </summary>
	/// <param name="Columns">Sort specifications: a column and the direction it was given.</param>
	/// <param name="Clustering"><c>CLUSTERED</c> or <c>NONCLUSTERED</c>, as written.</param>
	/// <param name="Check">The condition of a <c>CHECK</c>, or the value of a <c>DEFAULT</c>.</param>
	/// <param name="Filter">The <c>WHERE</c> of a filtered index.</param>
	/// <param name="Referenced">The <c>REFERENCES</c> of a foreign key.</param>
	/// <param name="Options">The <c>WITH (…)</c>; for <c>CONNECTION</c>, the pairs and the actions.</param>
	/// <param name="Enforced">Null where nothing was said, false for <c>NOT ENFORCED</c>.</param>
	/// <param name="ForColumn">The column a <c>DEFAULT … FOR</c> names.</param>
	/// <param name="Bracketed">
	/// Whether the <c>WITH</c> had brackets. The two spellings are two syntaxes and not two
	/// layouts: <c>WITH FILLFACTOR = 23, PAD_INDEX</c> lets an option stand as a word alone,
	/// and <c>WITH (…)</c> does not — inside the brackets it has to be <c>PAD_INDEX = ON</c>.
	/// </param>
	public sealed record ConstraintDefinition(
		string? Name, string Kind, Clause[] Columns, Expression? Check,
		string? Clustering = null, bool Hash = false, bool Columnstore = false,
		string[]? Order = null, string[]? Include = null, Expression? Filter = null,
		Clause? Referenced = null, Clause[]? Options = null, Clause[]? Placements = null,
		bool? Enforced = null, string? ForColumn = null, bool Bracketed = true) : Clause;

	/// <summary>
	/// §11.8 what a foreign key refers to: the table, its columns where they are named, what
	/// happens on delete and on update, and whether replication is told to leave it alone.
	/// </summary>
	public sealed record References(
		string Table, string[]? Columns, string? OnDelete, string? OnUpdate, bool NotForReplication) : Clause;

	/// <summary>One pair of node tables an edge may connect.</summary>
	public sealed record Connection(string From, string To) : Clause;

	/// <summary>
	/// One thing an <c>ALTER TABLE … DROP</c> names: which kind it is, and its name where it
	/// has one — the words in the order the drop writes them, which is not the order a
	/// definition writes them in.
	/// </summary>
	/// <param name="Tail">What the drop was given — <c>WITH (…)</c>, and the words of a period.</param>
	public sealed record Dropped(
		string Kind, string? Name, bool IfExists = false, string? Tail = null) : Clause;

	/// <summary>One file of a database: the bracketed options that describe it.</summary>
	public sealed record DatabaseFile(Clause[] Options) : Clause;

	/// <summary>
	/// A file group in a <c>CREATE DATABASE</c>: its name, what it contains where it says,
	/// whether it is the default, and its files.
	/// </summary>
	public sealed record FileGroup(string Name, string? Contains, bool Default, Clause[] Files) : Clause;

	/// <summary>
	/// One piece of an event session — <c>ADD EVENT</c>, <c>DROP EVENT</c>, <c>ADD TARGET</c>,
	/// <c>DROP TARGET</c> — with what an added one was given: its settings, its actions, and
	/// its predicate, which is a language of its own and is kept as written.
	/// </summary>
	public sealed record EventPiece(
		string Action, string Name, Clause[]? Settings = null, string[]? Actions = null, string? Where = null) : Clause;

	/// <summary>What a statement with no clauses is handed, once rather than per call.</summary>
	public static readonly Clause[] None = [];

	/// <summary>A constraint written without a name, which is most of them.</summary>
	public static ConstraintDefinition Constrained(
		string kind, Clause[]? columns, Expression? check) => new(null, kind, columns ?? None, check);

	/// <summary>A named thing dropped, where only the kind and the name matter.</summary>
	public static Dropped Marked(string kind, string? name) => new(kind, name);

	/// <summary>A dropped thing with what was written around it.</summary>
	public static Dropped Marked(string kind, string? name, string? ifExists, string? tail) =>
		new(kind, name, ifExists is not null, Syntax.Tail(tail));

	/// <summary>
	/// A key or a uniqueness: the kind, and everything T-SQL lets stand after it.
	/// </summary>
	public static ConstraintDefinition Keyed(
		string kind, string? clustering, string? hash, Clause[]? columns,
		Clause[]? options, Clause? placement, string? enforced) =>
		new(null, kind, columns ?? None, null,
			Clustering: clustering, Hash: hash is not null,
			Options: options, Placements: placement is null ? null : [placement],
			Enforced: Syntax.Enforced(enforced));

	/// <summary>An index written inside a table or on its own, with everything that may follow it.</summary>
	public static ConstraintDefinition Indexed(
		string name, string? unique, string? clustering, string? columnstore, string? hash,
		Clause[]? columns, string[]? order, string[]? include, Expression? filter,
		Clause[]? options, Placement? on, Placement? filestream) =>
		new(name, unique is null ? "INDEX" : "UNIQUE INDEX", columns ?? None, null,
			Clustering: clustering, Hash: hash is not null, Columnstore: columnstore is not null,
			Order: order, Include: include, Filter: filter, Options: options,
			Placements: on is null && filestream is null
				? null
				: [.. new[] { on, filestream }.OfType<Clause>()]);

	/// <summary>What a column is told after its type: one word or several, and its argument.</summary>
	public static ColumnOption Optioned(string kind, Expression? argument = null) =>
		new(Syntax.Squared(kind), argument is null ? Expression.None : [argument], None, null);

	/// <summary>A column's option or constraint, given the name written in front of it.</summary>
	public static Clause Named(Clause one, string name) => one switch
	{
		ColumnOption option             => option with { ConstraintName = name },
		ConstraintDefinition constraint => constraint with { Name = name },
		_                               => one,
	};

	/// <summary>
	/// A <c>TOP</c> from the words written after it, which are two questions and one rule:
	/// whether it is a share rather than a count, and what it does about a tie.
	/// </summary>
	public static Top Topped(Expression value, string? suffix)
	{
		if (suffix is null)
			return new Top(value, false, null);

		var squared = Syntax.Squared(suffix);
		var percent = squared.StartsWith("PERCENT", StringComparison.Ordinal);
		var at      = squared.IndexOf("WITH ", StringComparison.Ordinal);

		return new Top(value, percent, at < 0 ? null : squared[(at + 5)..]);
	}

	/// <summary>
	/// The order clause and the two the reference writes inside it, gathered where they were
	/// read apart. Nothing where nothing was written.
	/// </summary>
	public static OrderBy? Ordered(Clause[]? by, OrderBy? window) =>
		by is null && window is null
			? null
			: new OrderBy(by ?? None, window?.Offset, window?.Fetch);
}

/// <summary>
/// What every hierarchy needs and none of them owns: the lists a grammar gathers, and the
/// words it matched read back as the constants they stand for.
/// </summary>
/// <remarks>
/// The words are told apart by length and first letter, which costs nothing and allocates
/// nothing — the alternative is the matched text, and a capture cuts a string.
/// </remarks>
public static class Syntax
{
	/// <summary>What a call with no names is handed, once rather than per call.</summary>
	public static readonly string[] NoNames = [];

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static T[] Listed<T>(T first, T[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest) => Listed(first, rest);

	/// <summary>Names as sort specifications with no direction, which is how a column list stands.</summary>
	public static Clause[] Columns(string[]? names) =>
		names is null ? Clause.None : [.. names.Select(static one => new Clause.SortSpecification(new Expression.ColumnReference(one), SqlOrder.Unspecified))];

	/// <summary>One word as an option with nothing set, or nothing where none was written.</summary>
	public static Clause[] Unit(string? word) =>
		word is null ? Clause.None : [new Clause.Option(Squared(word), null, Clause.None)];

	/// <summary>A grouping column with what was written after it: a collation, a hint.</summary>
	public static Expression Grouped(Expression column, string? collation, string? hint)
	{
		var made = collation is null ? column : new Expression.Collated(column, collation);

		return hint is null ? made : new Expression.Hinted(made, Spaced(hint));
	}

	/// <summary>
	/// A routine with the word it was opened with, where that is not the record's own.
	/// </summary>
	/// <remarks>
	/// The four are one syntax with four bodies — <c>CREATE OR ALTER</c> stands in front of
	/// all of them and is read once — so which word was written is a fact about the statement
	/// rather than about which of the four it is.
	/// </remarks>
	public static Statement Verbed(Statement routine, string verb) => routine switch
	{
		Statement.CreateProcedure  one => one with { Verb = Squared(verb) },
		Statement.CreateFunction   one => one with { Verb = Squared(verb) },
		Statement.CreateTrigger    one => one with { Verb = Squared(verb) },
		Statement.ViewDefinition   one => one with { Verb = Squared(verb) },
		_                              => routine,
	};

	/// <summary>A table source rooted at a variable, once the variable's name is known.</summary>
	/// <remarks>
	/// The two shapes share their first token and so are read by one rule, which cannot know
	/// the name until the caller hands it over: a table variable is a name, and a
	/// table-valued method on a variable of a user-defined type is a call whose name begins
	/// with one.
	/// </remarks>
	public static TableReference OfVariable(string name, TableReference source) => source switch
	{
		TableReference.Named one        => one with { Table = name },
		TableReference.FunctionCall two => two with
		{
			Function = two.Function with { Name = name + two.Function.Name },
		},
		_                               => source,
	};

	/// <summary>A value with words after it, or the value alone where there were none.</summary>
	public static Expression Hinted(Expression value, string? words) =>
		words is null || words.Length == 0 ? value : new Expression.Hinted(value, Spaced(words));

	/// <summary>
	/// What a catalogue statement wrote after its name, as the words were written — or null
	/// where it wrote nothing (<see cref="Statement.Definition.Tail"/>).
	/// </summary>
	public static string? Tail(string? words) =>
		words is null || words.Length == 0 ? null : Spaced(words);

	/// <summary>Words as written, with the whitespace between them made one space.</summary>
	public static string Spaced(string words) => Run(words, false);

	/// <summary>Text with every whitespace character taken out: <c>(1 . 2 . 3 . 4)</c> as <c>(1.2.3.4)</c>.</summary>
	/// <summary>
	/// Whether a name may be called as a function: a delimited name of one part may not. The
	/// engine refuses <c>[LEN]('a')</c> and <c>"LEN"('a')</c>, and reads <c>dbo.[LEN]('a')</c>.
	/// </summary>
	public static bool Callable(string name)
	{
		if (name.Length == 0 || (name[0] != '[' && name[0] != '"'))
			return true;

		var close = name[0] == '[' ? ']' : '"';

		for (var at = 1; at < name.Length; at++)
		{
			if (name[at] != close)
				continue;

			if (at + 1 < name.Length && name[at + 1] == close)
			{
				at++;
				continue;
			}

			return at + 1 < name.Length;
		}

		return true;
	}

	/// <summary>
	/// The names a date part is written with: the published ones and their abbreviations, and
	/// <c>w</c> beside them, which the engine reads for <c>weekday</c> and the table leaves out.
	/// </summary>
	static readonly HashSet<string> DateParts = new(StringComparer.OrdinalIgnoreCase)
	{
		"year", "yy", "yyyy", "quarter", "qq", "q", "month", "mm", "m", "dayofyear", "dy", "y",
		"day", "dd", "d", "week", "wk", "ww", "weekday", "dw", "w", "hour", "hh", "minute", "mi",
		"n", "second", "ss", "s", "millisecond", "ms", "microsecond", "mcs", "nanosecond", "ns",
		"tzoffset", "tz", "iso_week", "isowk", "isoww",
	};

	/// <summary>The functions whose first argument is a date part.</summary>
	static readonly HashSet<string> DatePartFunctions = new(StringComparer.OrdinalIgnoreCase)
	{
		"DATEPART", "DATENAME", "DATEADD", "DATEDIFF", "DATEDIFF_BIG", "DATETRUNC", "DATE_BUCKET",
	};

	/// <summary>Whether a name, bracketed, quoted or neither, is a date part's.</summary>
	public static bool IsDatePart(string? name) => name is not null && DateParts.Contains(Unquoted(name));

	/// <summary>Whether a name of one part is a function whose first argument is a date part.</summary>
	public static bool IsDatePartFunction(string name) => DatePartFunctions.Contains(name);

	/// <summary>Whether a value is a column and nothing more, brackets aside.</summary>
	public static bool IsColumn(Expression? value) => value switch
	{
		Expression.ColumnReference    => true,
		Expression.Parenthesized(var inner) => IsColumn(inner),
		_                             => false,
	};

	static string Unquoted(string name) =>
		name.Length >= 2 && (name[0] == '[' && name[^1] == ']' || name[0] == '"' && name[^1] == '"')
			? name[1..^1]
			: name;

	/// <summary>
	/// Whether a select list holds T-SQL's <c>IDENTITY(…)</c>, which only a <c>SELECT … INTO</c>
	/// may: the engine refuses one without the <c>INTO</c> when it compiles the statement,
	/// <c>Msg 177</c>.
	/// </summary>
	public static bool HasIdentity(Clause[] columns)
	{
		foreach (var one in columns)
			if (one is Clause.DerivedColumn { Value: Expression.RoutineInvocation { Name: var name, Word: not null } }
				&& string.Equals(name, "IDENTITY", StringComparison.OrdinalIgnoreCase))
				return true;

		return false;
	}

	public static string Compacted(string text)
	{
		var kept = new char[text.Length];
		var at   = 0;

		foreach (var one in text)
			if (!char.IsWhiteSpace(one))
				kept[at++] = one;

		return new string(kept, 0, at);
	}

	/// <summary>
	/// Whether a bracket holds an IPv4 address: four runs of digits and the three dots between
	/// them, however spaced, and after a colon four more for the mask — which is how an
	/// endpoint's <c>LISTENER_IP</c> is written. The lexer reads <c>1.2</c> as one number, so
	/// the parts are counted here and not by a rule.
	/// </summary>
	public static bool IsAddress(string text)
	{
		var inside = Compacted(text);

		if (inside.Length < 2 || inside[0] != '(' || inside[inside.Length - 1] != ')')
			return false;

		var halves = inside.Substring(1, inside.Length - 2).Split(':');

		return halves.Length <= 2 && Array.TrueForAll(halves, IsFourPart);
	}

	/// <summary>Four runs of digits with a dot between each two, and nothing else.</summary>
	static bool IsFourPart(string text)
	{
		var parts  = 1;
		var digits = 0;

		foreach (var one in text)
		{
			if (one == '.')
			{
				if (digits == 0)
					return false;

				parts++;
				digits = 0;
			}
			else if (one is >= '0' and <= '9')
				digits++;
			else
				return false;
		}

		return parts == 4 && digits > 0;
	}

	/// <summary>The arguments that were written, in order, leaving out the optional ones that were not.</summary>
	public static Expression[] Written(params Expression?[] arguments) =>
		arguments.Where(static one => one is not null).Select(static one => one!).ToArray();

	/// <summary>
	/// A run of words, one space between them — and what stands inside quotes copied
	/// character for character.
	/// </summary>
	/// <remarks>
	/// A literal is not a run of words. <c>'a  b'</c> is one value with two spaces in it and
	/// <c>'default()'</c> is not a keyword to be raised, so neither the spacing nor the case of
	/// anything between quotes is this function's to change — which is what <c>MASKED WITH
	/// (FUNCTION = 'default()')</c> came back from as <c>'DEFAULT()'</c>.
	/// </remarks>
	static string Run(string words, bool raised)
	{
		var made  = new System.Text.StringBuilder(words.Length);
		var until = '\0';

		foreach (var c in words)
		{
			if (until != '\0')
			{
				made.Append(c);

				if (c == until)
					until = '\0';

				continue;
			}

			// A quote of any of the three kinds, and what it is closed by.
			if (c is '\'' or '"' or '[')
			{
				made.Append(c);
				until = c == '[' ? ']' : c;

				continue;
			}

			if (char.IsWhiteSpace(c))
			{
				if (made.Length > 0 && made[made.Length - 1] != ' ')
					made.Append(' ');
			}
			else
			{
				made.Append(raised ? char.ToUpperInvariant(c) : c);
			}
		}

		return made.ToString().TrimEnd();
	}

	/// <summary>
	/// Arguments with the clauses written after the last of them — a JSON constructor's
	/// order, null treatment and return type — hung on that last argument, since they are
	/// written where it ends.
	/// </summary>
	public static Expression[] Tailed(Expression[] items, params string?[] clauses)
	{
		var words = string.Join(" ", clauses.Where(static one => one is not null).Select(static one => Spaced(one!)));

		if (words.Length == 0)
			return items;

		if (items.Length == 0)
			return [new Expression.ColumnReference(words)];

		var made = (Expression[])items.Clone();

		made[^1] = new Expression.Hinted(made[^1], words);

		return made;
	}

	/// <summary>A value with the unit after it, or the value alone where none was written.</summary>
	public static Expression Measured(Expression value, string? unit) =>
		unit is null ? value : new Expression.Measured(value, Squared(unit));

	/// <summary>Whether a key is enforced: null where nothing was said, false for <c>NOT ENFORCED</c>.</summary>
	public static bool? Enforced(string? words) =>
		words is null ? null : !Squared(words).StartsWith("NOT", StringComparison.Ordinal);

	/// <summary>A <c>REFERENCES</c> from its parts, the actions read back from the words.</summary>
	public static Clause.References Referenced(string table, string[]? columns, string[]? actions)
	{
		string? onDelete = null, onUpdate = null;
		var replication = false;

		foreach (var action in actions ?? NoNames)
		{
			var squared = Squared(action);

			if (squared.StartsWith("ON DELETE ", StringComparison.Ordinal))
				onDelete = squared[10..];
			else if (squared.StartsWith("ON UPDATE ", StringComparison.Ordinal))
				onUpdate = squared[10..];
			else
				replication = true;
		}

		return new Clause.References(table, columns, onDelete, onUpdate, replication);
	}

	/// <summary>A <c>CONNECTION</c>'s pairs and the actions after them, as one list.</summary>
	public static Clause[] Connected(Clause first, Clause[]? rest, string[]? actions) =>
		[.. Listed(first, rest), .. (actions ?? NoNames).Select(static one => (Clause)new Clause.Option(Squared(one), null, Clause.None))];

	/// <summary>Whether a word that is <c>ON</c> or <c>OFF</c> was the first of the two.</summary>
	public static bool Switched(string word) => (word[0] | 0x20) == 'o' && word.Length == 2;

	/// <summary>
	/// A query with the order and the shape written inside its brackets, where either was;
	/// the query alone where neither was.
	/// </summary>
	public static Query Ordered(Query query, Clause.OrderBy? by, Clause? shape) =>
		by is null && shape is null ? query : new Query.Ordered(query, by, shape);

	/// <summary>Which way a sort specification asked for its rows (§10.10).</summary>
	public static SqlOrder Ordered(string? order) =>
		order is null   ? SqlOrder.Unspecified :
		(order[0] | 0x20) == 'a' ? SqlOrder.Ascending
		                         : SqlOrder.Descending;

	/// <summary>Which comparison operator was written (§8.2's <c>&lt;comp op&gt;</c>).</summary>
	public static SqlComparison Compared(string operatorText) => Compacted(operatorText) switch
	{
		"="  => SqlComparison.Equal,
		"<>" => SqlComparison.NotEqual,
		"!=" => SqlComparison.NotEqualBang,
		"<"  => SqlComparison.Less,
		"<=" => SqlComparison.LessOrEqual,
		"!<" => SqlComparison.NotLess,
		">"  => SqlComparison.Greater,
		"!>" => SqlComparison.NotGreater,
		_    => SqlComparison.GreaterOrEqual,
	};

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.7).</remarks>
	/// <summary>Whether <c>OUTER</c> was written, which is a word and not a meaning.</summary>
	public static bool Outer(string? kind) =>
		kind is not null && kind.EndsWith("OUTER", StringComparison.OrdinalIgnoreCase);

	public static SqlJoin Joined(string? kind)
	{
		// The text is the whole of what was written, `LEFT OUTER` and not `LEFT`, so the
		// first word is what is asked about.
		if (kind is null)
			return SqlJoin.Unspecified;

		if (kind.StartsWith("LEFT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Left;

		if (kind.StartsWith("RIGHT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Right;

		return kind.StartsWith("FULL", StringComparison.OrdinalIgnoreCase)
			? SqlJoin.Full
			: SqlJoin.Inner;
	}

	/// <summary>
	/// The named queries written in front of a statement, put where they belong.
	/// </summary>
	/// <remarks>
	/// A <c>WITH</c> may precede five statements, and each of the five keeps it; any other
	/// statement has no <c>WITH</c> to be given, and the grammar does not offer it one.
	/// </remarks>
	public static Statement Preceded(Clause[]? with, Statement statement) =>
		with is null || with.Length == 0 ? statement : statement switch
		{
			Statement.Select select => select with { With = with },
			Statement.Insert insert => insert with { With = with },
			Statement.Update update => update with { With = with },
			Statement.Delete delete => delete with { With = with },
			Statement.Merge  merge  => merge  with { With = with },
			_                       => statement,
		};

	/// <summary>
	/// The batches of a script from what its grammar reads: the text before the first
	/// <c>GO</c>, and each later text with the <c>GO</c> in front of it — which is moved onto
	/// the batch before, since a <c>GO</c> ends a batch rather than beginning one.
	/// </summary>
	/// <remarks>
	/// A <c>GO</c> with nothing before it ends a batch with nothing in it, and is kept as one
	/// so that the line is not lost. The last batch, which nothing ended, is left out when it
	/// is empty: a file that ends with <c>GO</c> has no batch after it.
	/// </remarks>
	public static Batch[] Scripted(Statement[] first, Batch[]? rest, string? last = null)
	{
		var batches    = new List<Batch>();
		var statements = first;

		foreach (var next in rest ?? System.Array.Empty<Batch>())
		{
			batches.Add(new Batch(statements, next.Go));
			statements = next.Statements;
		}

		if (statements.Length > 0 || last is not null)
			batches.Add(new Batch(statements, last));

		return [.. batches];
	}

	/// <summary>
	/// A <c>GO</c> line as what it says: <c>GO</c>, and the count where one is written. What
	/// else closed the line — a <c>;</c>, a comment — is trivia, which the tree keeps nowhere.
	/// </summary>
	public static string GoLine(string line)
	{
		var at = 2;

		while (at < line.Length && (line[at] == ' ' || line[at] == '\t'))
			at++;

		var digits = at;

		while (digits < line.Length && char.IsDigit(line[digits]))
			digits++;

		return digits > at ? "GO " + line[at..digits] : "GO";
	}

	/// <summary>
	/// A table primary and everything written around it, as the one node it is.
	/// </summary>
	/// <remarks>
	/// A name and a bracketed argument list is a table-valued function and not a table, so
	/// which record comes out is decided by whether the brackets were there.
	/// </remarks>
	public static TableReference Sourced(
		string name, Clause? when, bool path, string? call, Expression[]? arguments,
		string? alias, string[]? columns, Clause? sample, Clause[]? hints, TableReference? pivot)
	{
		TableReference source = call is null
			? new TableReference.Named(name, when, path, alias, columns, sample, hints ?? Clause.None)
			: new TableReference.FunctionCall(
				new Expression.RoutineInvocation(name, arguments ?? Expression.None),
				alias, columns, Clause.None);

		return Pivoted(source, pivot);
	}

	/// <summary>A source and the pivot applied to it, where one was written.</summary>
	public static TableReference Pivoted(TableReference source, TableReference? pivot) =>
		pivot switch
		{
			TableReference.Pivot turned   => turned with { Of = source },
			TableReference.Unpivot turned => turned with { Of = source },
			_                             => source,
		};

	/// <summary>A call and what was written after it, where anything was.</summary>
	public static Expression Called(Expression call, Expression.WindowFunction? tail) =>
		tail is null ? call : tail with { Function = call };

	/// <summary>
	/// A primary and what was reached through it: the members written after it, and the zones
	/// it is read in, each with the collation written in front of it. Each member and each
	/// zone was built with its own left side null, the way a predicate tail is, so the chain
	/// is closed here.
	/// </summary>
	public static Expression Reached(
		Expression primary, Expression.Member[]? members, Expression[]? zones)
	{
		if (members is not null)
			foreach (var member in members)
				primary = member with { Of = primary };

		if (zones is not null)
			foreach (var zone in zones)
			{
				var at = (Expression.RoutineInvocation)zone;
				var of = at.Arguments[0] is Expression.Collated how ? how with { Value = primary } : primary;

				primary = at with { Arguments = [of, at.Arguments[1]] };
			}

		return primary;
	}

	/// <summary>A word the grammar matched, as the one constant that stands for it.</summary>
	public static SqlTruth TruthOf(string word) =>
		(word[0] | 0x20) switch
		{
			't' => SqlTruth.True,
			'f' => SqlTruth.False,
			_   => SqlTruth.Unknown,
		};

	public static string Aggregate(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => "AVG",
			's' => "SUM",
			'c' => "COUNT",
			'm' => (word[1] | 0x20) == 'a' ? "MAX" : "MIN",
			_   => word,
		};

	public static string Quantified(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
			's' => "SOME",
			_   => word,
		};

	/// <summary>
	/// A set quantifier, as it was written.
	/// </summary>
	/// <remarks>
	/// It used to come back as one of two constants, and that was a small lie of the kind
	/// this tree is not allowed to tell: <c>SELECT ALL a</c> and <c>SELECT a</c> are two
	/// texts, and a parser that answers <c>null</c> for the first has decided something the
	/// author did not ask it to decide. What a missing <c>ALL</c> <em>means</em> is a question
	/// for whatever reads the tree.
	/// </remarks>
	public static string? Distinctly(string? word) => word;

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	public static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
	}

	/// <summary>Whether a cursor's words agree with each other and with its <c>FOR</c>.</summary>
	/// <remarks>
	/// <para>
	/// As the engine answers them, every pair put to it (<c>Msg 1048</c>). A word may be said
	/// twice, and two words of one group may not — where the cursor is seen, which way it moves,
	/// what it holds, how its rows are locked. <c>FAST_FORWARD</c> is a way of moving and a way
	/// of holding at once, and refuses <c>SCROLL</c> and every lock but <c>READ_ONLY</c>.
	/// </para>
	/// <para>
	/// <c>FOR UPDATE</c> refuses what cannot be changed through — <c>STATIC</c>,
	/// <c>FAST_FORWARD</c>, <c>READ_ONLY</c> and the standard's <c>INSENSITIVE</c> — and
	/// <c>FOR READ ONLY</c> refuses the locks. A declared cursor refuses <c>READ_ONLY</c> beside
	/// it as well (<c>Msg 1058</c>); a cursor a variable is set to reads the two together.
	/// </para>
	/// </remarks>
	public static bool CursorAgrees(string[]? words, Clause? @for, bool declared = true)
	{
		if (words is null)
			return true;

		foreach (var one in words)
		{
			foreach (var other in words)
				if (one != other && (Group(one) is > 0 and var group && group == Group(other) || Faster(one, other)))
					return false;

			if (@for is Clause.CursorFor(var update, _) && (update ? Fixed(one) : Locked(one, declared)))
				return false;
		}

		return true;

		static int Group(string word) =>
			word switch
			{
				"LOCAL" or "GLOBAL"                                 => 1,
				"FORWARD_ONLY" or "SCROLL"                          => 2,
				"STATIC" or "KEYSET" or "DYNAMIC" or "FAST_FORWARD" => 3,
				"READ_ONLY" or "SCROLL_LOCKS" or "OPTIMISTIC"       => 4,
				_                                                   => 0,
			};

		static bool Faster(string one, string other) =>
			one == "FAST_FORWARD" && other is "SCROLL" or "SCROLL_LOCKS" or "OPTIMISTIC";

		static bool Fixed(string word) => word is "STATIC" or "FAST_FORWARD" or "READ_ONLY" or "INSENSITIVE";

		static bool Locked(string word, bool declared) =>
			word is "SCROLL_LOCKS" or "OPTIMISTIC" || declared && word == "READ_ONLY";
	}

	/// <summary>Whether a query may be a cursor's.</summary>
	/// <remarks>
	/// A select, and one that makes no table: the engine refuses <c>INTO</c> in a cursor's query
	/// (<c>Msg 154</c>) and <c>VALUES</c> standing for one (<c>Msg 156</c>), in brackets or in a
	/// union as well.
	/// </remarks>
	public static bool Cursorable(Query? query) =>
		query switch
		{
			null                                    => false,
			Query.Specification { Into: not null }  => false,
			Query.TableValueConstructor             => false,
			Query.Union(var left, var right, _)     => Cursorable(left) && Cursorable(right),
			Query.Except(var left, var right, _)    => Cursorable(left) && Cursorable(right),
			Query.Intersect(var left, var right, _) => Cursorable(left) && Cursorable(right),
			Query.Parenthesized(var inner)          => Cursorable(inner),
			Query.Ordered(var inner, _, _)          => Cursorable(inner),
			_                                       => true,
		};

	/// <summary>Whether a routine's parameters pass a cursor, which a function may not.</summary>
	public static bool HasCursorParameter(Clause[]? parameters) =>
		parameters is not null &&
		Array.Exists(parameters, static one => one is Clause.ParameterDeclaration(_, "CURSOR", _, _, _, _));

	/// <summary>Whether a name was written bare, rather than in brackets or quotes.</summary>
	public static bool IsBare(string? name) => name is { Length: > 0 } && name[0] is not ('[' or '"');

	/// <summary>Whether no option of a list is named twice.</summary>
	public static bool NamedOnce(Clause? first, Clause[]? rest)
	{
		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		return Fresh(first) && Array.TrueForAll(rest ?? [], Fresh);

		bool Fresh(Clause? one) => one is not Clause.Option { Name: var name } || seen.Add(name);
	}

	/// <summary>Whether a list of options names one.</summary>
	public static bool HasOption(Clause[]? options, string name) =>
		options is not null &&
		Array.Exists(options, one => one is Clause.Option { Name: var named } &&
			string.Equals(named, name, StringComparison.OrdinalIgnoreCase));

	/// <summary>Whether a queue created has what its activation needs.</summary>
	/// <remarks>
	/// A procedure, how many read at once and whom they run as, all three, where an altered
	/// queue may change any of them or drop the activation altogether.
	/// </remarks>
	public static bool Activates(Clause[]? options)
	{
		if (options is null)
			return true;

		foreach (var one in options)
			if (one is Clause.Option { Name: "ACTIVATION", Options: var inner } &&
				!(HasOption(inner, "PROCEDURE_NAME") && HasOption(inner, "MAX_QUEUE_READERS") &&
				  HasOption(inner, "EXECUTE AS")))
			{
				return false;
			}

		return true;
	}

	/// <summary>Whether a dialog's options are each said once, and relate it one way at most.</summary>
	public static bool DialogAgrees(Clause[]? options) =>
		NamedOnce(null, options) &&
		!(HasOption(options, "RELATED_CONVERSATION") && HasOption(options, "RELATED_CONVERSATION_GROUP"));

	/// <summary>A cursor's query and what stands around it, its words yet to be said.</summary>
	public static Clause.CursorDefinition Cursor(
		Clause[]? with, Query query, Clause? @for, Clause[]? hints, bool forFirst) =>
		new([], [], with ?? Clause.None, query, @for, hints ?? Clause.None, forFirst);

	/// <summary>Whether a variable is one of the server's, <c>@@ROWCOUNT</c> and its kind.</summary>
	/// <remarks>
	/// They read as values and refuse to be anything else: a cursor, a row's destination or a
	/// row's number (<c>Msg 102</c>), every one of the published names put to the engine. A name
	/// the server has not heard of, <c>@@x</c>, is a variable like any other.
	/// </remarks>
	public static bool IsServerVariable(string? variable) =>
		variable is not null && ServerVariables.Contains(variable);

	static readonly HashSet<string> ServerVariables = new(StringComparer.OrdinalIgnoreCase)
	{

		"@@CONNECTIONS", "@@CPU_BUSY", "@@CURSOR_ROWS", "@@DATEFIRST", "@@DBTS", "@@DEFAULT_LANGID",
		"@@DEF_SORTORDER_ID", "@@ERROR", "@@FETCH_STATUS", "@@IDENTITY", "@@IDLE", "@@IO_BUSY",
		"@@LANGID", "@@LANGUAGE", "@@LOCK_TIMEOUT", "@@MAX_CONNECTIONS", "@@MAX_PRECISION",
		"@@MICROSOFTVERSION", "@@NESTLEVEL", "@@OPTIONS", "@@PACKET_ERRORS", "@@PACK_RECEIVED",
		"@@PACK_SENT", "@@PROCID", "@@REMSERVER", "@@ROWCOUNT", "@@SERVERNAME", "@@SERVICENAME",
		"@@SPID", "@@TEXTSIZE", "@@TIMETICKS", "@@TOTAL_ERRORS", "@@TOTAL_READ", "@@TOTAL_WRITE",
		"@@TRANCOUNT", "@@VERSION",
	};

	/// <summary>A run of words as one upper-case word per space.</summary>
	public static string Squared(string words) => Run(words, true);

	/// <summary>
	/// One setting of a catalogue's list as a node, from the words its line read: the name up
	/// to the first <c>=</c> outside brackets, and the rest as the value's words.
	/// </summary>
	/// <remarks>
	/// For a statement that keeps its words as a tail and its settings beside them as nodes,
	/// so that a check has names to match. <c>DECRYPTION BY PASSWORD = 'p'</c> is named by
	/// its three words; <c>ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01)</c>, with no <c>=</c> of
	/// its own, by the word before its bracket.
	/// </remarks>
	public static Clause.Option Setting(string words)
	{
		var depth = 0;

		for (var at = 0; at < words.Length; at++)
			switch (words[at])
			{
				case '(':
					depth++;
					break;

				case ')':
					depth--;
					break;

				case '=' when depth == 0:
					return new Clause.Option(
						Squared(words.Substring(0, at)), new Expression.ColumnReference(words.Substring(at + 1).Trim()), Clause.None);
			}

		var open = words.IndexOf('(');

		return new Clause.Option(Squared(open < 0 ? words : words.Substring(0, open)), null, Clause.None);
	}

	/// <summary>A word the grammar read and this file has no record for.</summary>
	/// <remarks>
	/// The grammar and the switches in this file are two spellings of one catalogue and have to
	/// agree; where they have drifted apart this says so rather than quietly building the wrong
	/// node. A defect in this file, not in anybody's SQL.
	/// </remarks>
	public static ArgumentOutOfRangeException Unknown(string word) =>
		new(nameof(word), word, "The grammar reads this and the tree has no record for it.");
}

/// <summary>§8.2 <c>&lt;comp op&gt;</c>, which the standard writes as a production of its own.</summary>
public enum SqlComparison
{
	Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual,

	/// <summary>T-SQL's <c>!=</c>, which means what <c>&lt;&gt;</c> does and is kept apart because it is written apart.</summary>
	NotEqualBang,

	/// <summary>T-SQL's <c>!&lt;</c>, not less than.</summary>
	NotLess,

	/// <summary>T-SQL's <c>!&gt;</c>, not greater than.</summary>
	NotGreater,
}

/// <summary>Which join (§7.7), and the two T-SQL adds.</summary>
/// <remarks>
/// An apply reads its right side once per row of its left, which is a question about
/// evaluation rather than shape — but it is written differently and refused where a join
/// would be read, so the tree says which was written.
/// </remarks>
public enum SqlJoin
{
	/// <summary>
	/// No word was written — a bare <c>JOIN</c>. What that means is an inner join, and
	/// meaning it is not the parser's to say: <c>JOIN</c> and <c>INNER JOIN</c> are two
	/// texts, and only one of them was typed.
	/// </summary>
	Unspecified,

	Cross, Inner, Left, Right, Full, Union, CrossApply, OuterApply,
}

/// <summary>Which way a sort was asked for, and whether it was asked for at all (§10.10).</summary>
public enum SqlOrder
{
	Unspecified, Ascending, Descending,
}

/// <summary>What a literal is, where the text alone does not say.</summary>
public enum SqlLiteralKind
{
	Number, Text, National, Bit, Hex, Date, Time, Timestamp, Interval,
	Null, Default, Parameter, Special,
}

/// <summary>The three truth values a test compares against (§6.39).</summary>
public enum SqlTruth
{
	True, False, Unknown,
}
