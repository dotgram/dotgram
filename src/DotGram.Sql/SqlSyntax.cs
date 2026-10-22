using System;

namespace DotGram.Sql;

// ── What a SQL parser builds ───────────────────────────────────────────────────────────────
//
// Five hierarchies, one per category the standard has: `Statement`, `Query`, `Expression`,
// `TableReference` and `Clause`. They are named after the productions they come from and are
// shared by every parser of the language — `Sql92Parser`, `TransactSqlParser`, and whatever reads
// the same grammar next. `docs/ast.md` lists each node and the specification it is named from.
//
// **Why five and not one.** A tree with a single root types nothing: a field of it accepts a
// statement where a value belongs, and the compiler cannot tell a reader they are wrong. The
// standard does not work that way — §7 puts a <query expression> where a table belongs and §6
// puts a <value expression> where a value belongs, and the two are not interchangeable. So the
// roots are the standard's own categories, and a field says which one it holds:
// `Statement.Insert.Rows` is a `Query`, `Expression.Subquery.Query` is a `Query`, and
// `Query.Specification.From` is a `SqlList<TableReference>`. Several things that used to
// typecheck and were wrong now do not compile.
//
// **Lists compare by what is in them.** Every list a node holds is a `SqlList<T>` and not an
// array, so the equality the compiler writes for each record compares trees by content: a tree
// equals its own reparse, located or not.
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

/// <summary>A statement: §13 of the standard, and most of a dialect's reference.</summary>
public abstract record Statement : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected Statement()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected Statement(Statement original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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
		SqlList<Clause> With, Query Of, Clause? OrderBy, SqlList<Clause> For, SqlList<Clause> Options) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Query;
	}

	/// <summary>A select with nothing around it, which is the whole of the standard's.</summary>
	public static Select Selected(Query of, Clause? by = null)
	{
		return new(Clause.None, of, by, Clause.None, Clause.None);
	}

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
	/// <param name="Into">
	/// The word written before the target, which the tree keeps because two of them stand
	/// there: <c>INTO</c>, and <c>OVER</c> that no page describes and the engine reads —
	/// <c>INSERT OVER t1 DEFAULT VALUES</c>. Null where neither was written.
	/// </param>
	public sealed record Insert(
		TableReference? Target, SqlList<string>? Columns, Query Rows,
		SqlList<Clause>? With = null, Clause? Top = null, Clause? Output = null, string? Into = "INTO") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;

		/// <summary>
		/// T-SQL's <c>OPTION (…)</c> after the rows, where they are a query, a <c>VALUES</c>
		/// or <c>DEFAULT VALUES</c> — the engine refuses it after an <c>EXEC</c>.
		/// </summary>
		public SqlList<Clause>? Options { get; init; }
	}

	/// <summary>§14.14 rows changed in place: what to change, to what, and which rows.</summary>
	/// <remarks>
	/// <see cref="From"/> is T-SQL's extension and not the standard's: a second <c>FROM</c>
	/// naming the tables the rows to change are found by joining.
	/// </remarks>
	public sealed record Update(
		TableReference? Target, SqlList<Clause> Set, SqlList<TableReference> From, Expression? Where,
		SqlList<Clause>? With = null, Clause? Top = null, Clause? Output = null, SqlList<Clause>? Options = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary>§14.9 rows removed, and the same two ways of saying which.</summary>
	public sealed record Delete(
		TableReference? Target, SqlList<TableReference> From, Expression? Where,
		SqlList<Clause>? With = null, Clause? Top = null, Clause? Output = null, SqlList<Clause>? Options = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary>
	/// §14.12 one statement that inserts, updates and deletes, according to what a join found.
	/// </summary>
	public sealed record Merge(
		TableReference? Target, TableReference Using, Expression On, SqlList<Clause> Whens,
		SqlList<Clause>? With = null, Clause? Top = null, string? Alias = null, Clause? Output = null, SqlList<Clause>? Options = null,
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
	public sealed record Compound(SqlList<Statement> Statements, SqlList<Clause>? Atomic = null) : Statement
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
	public sealed record TryCatch(SqlList<Statement> Tried, SqlList<Statement> Caught) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>One <c>DECLARE</c>, which may declare several.</summary>
	public sealed record Declare(SqlList<Clause> Variables) : Statement
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
	/// <param name="Server">
	/// <c>OPENDATASOURCE (…)</c>, where it stood for the server the procedure is on, with
	/// <see cref="Name"/> the rest of the name after it.
	/// </param>
	/// <param name="Bare">
	/// Whether the word was left out, as it may be where the call is the first statement of a
	/// batch: <c>sp_who</c>, <c>dbo.p 1</c>.
	/// </param>
	/// <param name="Number">
	/// The number after the name, <c>EXECUTE dbo.p;1</c>: one of a group of procedures sharing
	/// a name, which <see cref="CreateProcedure"/> declares the same way. A whole number from
	/// one — <c>;0</c> is <c>Msg 1005</c> and <c>;1.5</c> is <c>Msg 102</c>.
	/// </param>
	public sealed record Execute(
		string? Into, string Name, SqlList<Expression> Arguments,
		Clause? Context = null, string? At = null, string? DataSource = null, string? Tail = null,
		Expression.RoutineInvocation? Server = null, bool Bare = false, string? Number = null) : Statement
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
		string Name, string? Kind, SqlList<Clause> Elements, SqlList<Clause> Placements, SqlList<Clause> Options, bool External = false) : Statement
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
		string Name, SqlList<string>? Columns, SqlList<Clause> Options, Statement Body, bool External = false) : Statement
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
	public abstract record Removal(SqlList<Expression> Names) : Statement
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
		public SqlList<Clause>? Options { get; init; }

		/// <summary>
		/// The word the statement opened with, where the syntax lets it be more than one —
		/// <c>CREATE</c>, <c>ALTER</c>, <c>CREATE OR ALTER</c>. Null where the record's own
		/// name says which, the two verbs having a record each.
		/// </summary>
		public string? Verb { get; init; }
	}

	/// <param name="Tail">What the action was given, where the tree keeps it as written.</param>
	public sealed record AlterTable(
		string Name, string Action, SqlList<Clause> Elements, SqlList<Clause>? Options = null, string? Tail = null) : Statement
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
		string Name, SqlList<Clause> Parameters, SqlList<Statement> Body,
		SqlList<Clause>? Options = null, bool ForReplication = false, string? External = null, string? Number = null,
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
		string Name, SqlList<Clause> Parameters, string? Returns, SqlList<Statement> Body,
		SqlList<Clause>? Options = null, SqlList<Clause>? Columns = null, string? Variable = null,
		SqlList<Clause>? Order = null, string? External = null, string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>A trigger: what it is on, what fires it, and what it does then.</summary>
	/// <param name="When"><c>AFTER</c>, <c>FOR</c> or <c>INSTEAD OF</c>, as written.</param>
	/// <param name="External">The method in an assembly, where that is the body.</param>
	public sealed record CreateTrigger(
		string Name, string On, SqlList<string> Events, SqlList<Statement> Body,
		string When = "AFTER", SqlList<Clause>? Options = null, bool Append = false,
		bool NotForReplication = false, string? External = null, string Verb = "CREATE") : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>§11.32 a view, which is a name given to a query.</summary>
	/// <param name="Options">The <c>WITH</c> between the name and the <c>AS</c>: <c>SCHEMABINDING</c>, a materialized view's distribution.</param>
	/// <param name="CheckOption">The <c>WITH CHECK OPTION</c> after the query.</param>
	public sealed record ViewDefinition(
		string Name, SqlList<string>? Columns, Statement Body,
		SqlList<Clause>? Options = null, bool CheckOption = false, bool Materialized = false,
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
		Expression? Partition = null, SqlList<Clause>? Options = null, SqlList<string>? Paths = null, string? Namespaces = null) : Statement
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
		string? Contract, SqlList<Clause> Options) : Statement
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
		SqlList<Expression> Conversations, bool Bracketed, string? MessageType = null, Expression? Body = null) : Statement
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
		Clause? Top, SqlList<Clause> Columns, string Queue, string? Into = null, Expression? Where = null) : Statement
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

	// ---- a table emptied, a trigger switched, a session killed, a column classified ------------
	//
	// Four statements, each its own shape, as agreed.

	/// <summary><c>TRUNCATE TABLE</c>: every row of a table removed, or of some of its partitions.</summary>
	/// <param name="Partitions">
	/// What <c>WITH (PARTITIONS (…))</c> named, a <see cref="Clause.PartitionRange"/> each;
	/// null where it named none.
	/// </param>
	public sealed record Truncate(string Table, SqlList<Clause>? Partitions = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>ENABLE TRIGGER</c> or <c>DISABLE TRIGGER</c>.</summary>
	/// <param name="Triggers">The triggers by name; null for <c>ALL</c>.</param>
	/// <param name="On">What they are on: an object's name, <c>DATABASE</c> or <c>ALL SERVER</c>.</param>
	public sealed record TriggerSwitch(bool Enable, SqlList<string>? Triggers, string On) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>KILL</c>: a session or a unit of work ended, or a subscription, or a statistics job.</summary>
	/// <param name="Kind">
	/// Empty for a session or a unit of work; <c>QUERY NOTIFICATION SUBSCRIPTION</c> or
	/// <c>STATS JOB</c> for the other two.
	/// </param>
	/// <param name="Target">The session, the unit of work, the subscription or the job; <c>ALL</c> for every subscription.</param>
	/// <param name="With"><c>STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>, where one was said.</param>
	public sealed record Kill(string Kind, Expression Target, string? With = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>ADD SENSITIVITY CLASSIFICATION</c>: columns labelled for what they hold.</summary>
	/// <param name="Add">Whether the classification is added; its removal is <c>DropSensitivityClassification</c>.</param>
	/// <param name="Columns">The columns, by two parts or three.</param>
	/// <param name="Options">What followed <c>WITH</c>, a <see cref="Clause.Option"/> each.</param>
	public sealed record Classification(bool Add, SqlList<string> Columns, SqlList<Clause> Options) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>ADD SIGNATURE</c>: a module signed, the mirror of <see cref="DropSignature"/>.</summary>
	/// <param name="By">
	/// What signs it, as written: a certificate or an asymmetric key with the password or the
	/// signature after it, a symmetric key, or a password.
	/// </param>
	/// <param name="To">The module, with its class where one was said: <c>OBJECT::dbo.p</c>.</param>
	/// <param name="Counter">Whether it is a counter signature.</param>
	public sealed record AddSignature(SqlList<Expression> By, string To, bool Counter = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>READTEXT</c>: part of a <c>text</c>, <c>ntext</c> or <c>image</c> value, read through its pointer.</summary>
	/// <param name="Column">The column, by two parts to four.</param>
	/// <param name="Pointer">The text pointer: a variable, or a binary string.</param>
	/// <param name="Offset">Where the reading starts.</param>
	/// <param name="Size">How much is read.</param>
	/// <param name="HoldLock">Whether <c>HOLDLOCK</c> was said.</param>
	public sealed record ReadText(
		string Column, Expression Pointer, Expression Offset, Expression Size, bool HoldLock = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary><c>WRITETEXT</c>: a <c>text</c>, <c>ntext</c> or <c>image</c> value replaced through its pointer.</summary>
	/// <param name="Data">
	/// What it is replaced with; null where nothing was written, as after <c>BULK</c>, where
	/// the data comes by the bulk protocol instead.
	/// </param>
	/// <param name="Bulk">Whether <c>BULK</c> was said.</param>
	/// <param name="Log">Whether <c>WITH LOG</c> was said.</param>
	/// <param name="Timestamp">The <c>TIMESTAMP = 0x…</c> the row must still carry, where one was said.</param>
	public sealed record WriteText(
		string Column, Expression Pointer, Expression? Data, bool Bulk = false, bool Log = false,
		Expression? Timestamp = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary><c>UPDATETEXT</c>: part of a <c>text</c>, <c>ntext</c> or <c>image</c> value replaced through its pointer.</summary>
	/// <param name="Offset">Where the change starts; <c>NULL</c> for the end.</param>
	/// <param name="Length">How much is taken out; <c>NULL</c> for all of it to the end.</param>
	/// <param name="Data">What is put in, where it is a value.</param>
	/// <param name="Source">
	/// Another column what is put in is taken from instead, with <see cref="SourcePointer"/>
	/// its pointer.
	/// </param>
	/// <param name="Bulk">Whether <c>BULK</c> was said, the data then coming by the bulk protocol.</param>
	/// <param name="Log">Whether <c>WITH LOG</c> was said.</param>
	/// <param name="Timestamp">The <c>TIMESTAMP = 0x…</c> the row must still carry, where one was said.</param>
	public sealed record UpdateText(
		string Column, Expression Pointer, Expression Offset, Expression Length, Expression? Data = null,
		string? Source = null, Expression? SourcePointer = null, bool Bulk = false, bool Log = false,
		Expression? Timestamp = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dml;
	}

	/// <summary><c>RECONFIGURE</c>: the configuration just changed, taken up.</summary>
	/// <param name="Override">Whether <c>WITH OVERRIDE</c> was said.</param>
	public sealed record Reconfigure(bool Override = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>SHUTDOWN</c>: the server stopped.</summary>
	/// <param name="NoWait">Whether <c>WITH NOWAIT</c> was said, which skips the checkpoint.</param>
	public sealed record Shutdown(bool NoWait = false) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Admin;
	}

	/// <summary><c>LINENO</c>: the line number the next line of the batch is to be counted as.</summary>
	/// <param name="Line">The number, a whole one.</param>
	public sealed record LineNumber(Expression Line) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
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
	public sealed record Dbcc(string Command, SqlList<Expression>? Arguments, SqlList<Clause> Options) : Statement
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
		SqlList<Expression>? Into) : Statement
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
	public sealed record Throw(SqlList<Expression> Arguments) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Control;
	}

	/// <summary>A jump to a label.</summary>
	/// <remarks>
	/// The parameter is <c>Name</c> and not <c>Label</c> because <see cref="Label"/> is a
	/// statement of its own here, and a nested type takes the name from a member of the same
	/// class.
	/// </remarks>
	public sealed record GoTo(Expression Name) : Statement
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
	public sealed record RaiseError(SqlList<Expression> Arguments, string? Tail = null) : Statement
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

	/// <summary>T-SQL's label, the place a <c>GOTO</c> goes: a name and a colon.</summary>
	/// <remarks>
	/// Not a command, whatever it stands among: nothing is done here, and the name is the
	/// whole of it. <c>GOTO finish</c> names one and <c>finish:</c> is one.
	/// </remarks>
	public sealed record Label(string Name) : Statement
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
		string Name, string Verb = "CREATE", string On = "SERVER", SqlList<Clause>? Pieces = null,
		SqlList<Clause>? Options = null, string? State = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary><c>EVENT NOTIFICATION</c>.</summary>
	public sealed record EventNotificationDefinition(string Name) : Definition(Name);

	/// <summary>
	/// <c>CREATE</c> or <c>ALTER AVAILABILITY GROUP</c>: a group of databases that fail over
	/// together, its replicas, its listener and what it is told, kept as the words after the
	/// name.
	/// </summary>
	public sealed record AvailabilityGroupDefinition(string Name) : Definition(Name);

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

	/// <summary><c>OPEN SYMMETRIC KEY</c>: a key opened for the session, and what it is decrypted by.</summary>
	public sealed record OpenSymmetricKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>OPEN MASTER KEY</c>: the database's master key opened for the session.</summary>
	public sealed record OpenMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>CLOSE SYMMETRIC KEY</c>: one key the session opened, closed.</summary>
	public sealed record CloseSymmetricKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>CLOSE ALL SYMMETRIC KEYS</c>: every key the session opened, closed.</summary>
	public sealed record CloseAllSymmetricKeys(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>CLOSE MASTER KEY</c>.</summary>
	public sealed record CloseMasterKey(string Name) : Definition(Name)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Session;
	}

	/// <summary><c>CREATE PARTITION FUNCTION</c>: which partition a value goes to.</summary>
	public sealed record PartitionFunctionDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER PARTITION FUNCTION</c>: a boundary split or merged.</summary>
	public sealed record AlterPartitionFunction(string Name) : Definition(Name);

	/// <summary><c>CREATE PARTITION SCHEME</c>: which filegroup each partition is stored in.</summary>
	public sealed record PartitionSchemeDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER PARTITION SCHEME</c>: the filegroup the next partition goes to.</summary>
	public sealed record AlterPartitionScheme(string Name) : Definition(Name);

	/// <summary><c>SEQUENCE</c>: numbers handed out in order, created or altered.</summary>
	public sealed record SequenceDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE TYPE</c>: a type made of another, of an assembly's class, or of a table.</summary>
	public sealed record TypeDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE XML SCHEMA COLLECTION</c>: the schemas an <c>xml</c> is typed by.</summary>
	public sealed record XmlSchemaCollectionDefinition(string Name) : Definition(Name);

	/// <summary><c>ALTER XML SCHEMA COLLECTION</c>: a schema added to one.</summary>
	public sealed record AlterXmlSchemaCollection(string Name) : Definition(Name);

	/// <summary><c>CREATE SYNONYM</c>: another name for something, here or elsewhere.</summary>
	public sealed record SynonymDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE</c> or <c>ALTER ASSEMBLY</c>: a .NET assembly loaded into the database, or changed there.</summary>
	public sealed record AssemblyDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE</c> or <c>ALTER CRYPTOGRAPHIC PROVIDER</c>: a key management provider registered, switched or replaced.</summary>
	public sealed record CryptographicProviderDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE</c> or <c>ALTER EXTERNAL LANGUAGE</c>: a language extension and the files it is made of.</summary>
	public sealed record ExternalLanguageDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE</c> or <c>ALTER EXTERNAL MODEL</c>: a model served from outside the server, and where and how it is reached.</summary>
	public sealed record ExternalModelDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE RULE</c>: a condition a column bound to it must meet.</summary>
	public sealed record RuleDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE DEFAULT</c>: the value a column bound to it takes where it is given none.</summary>
	public sealed record DefaultDefinition(string Name) : Definition(Name);

	/// <summary><c>CREATE AGGREGATE</c>: an aggregate an assembly's class computes.</summary>
	public sealed record AggregateDefinition(string Name) : Definition(Name);


	/// <summary><c>ENDPOINT</c>.</summary>
	/// <summary>
	/// <c>CREATE</c> or <c>ALTER ENDPOINT</c>: the owner, the state and what was written
	/// beside it, how it is reached (<c>AS TCP (…)</c>) and what it speaks (<c>FOR TSQL
	/// ()</c>) — each protocol a bracket of options belonging to it.
	/// </summary>
	public sealed record EndpointDefinition(
		string Name, string Verb = "CREATE", string? Owner = null, string? State = null,
		SqlList<Clause>? StateOptions = null, string? Protocol = null, SqlList<Clause>? ProtocolOptions = null,
		string? Payload = null, SqlList<Clause>? PayloadOptions = null) : Statement
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
		string Name, SqlList<Clause> Files, bool Primary = false, SqlList<Clause>? Log = null,
		string? Containment = null, string? Collation = null,
		string? Tail = null, SqlList<Clause>? Options = null, SqlList<Clause>? With = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Ddl;
	}

	/// <summary>
	/// <c>ALTER DATABASE … SET</c>: what it was set to, and how the sessions in the way are
	/// dealt with — <c>WITH ROLLBACK AFTER 10 SECONDS</c>, <c>WITH NO_WAIT</c>.
	/// </summary>
	public sealed record AlterDatabaseSet(
		string Name, SqlList<Clause> Settings, string? Termination = null) : Statement
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
		string Name, string Action, SqlList<Clause> Settings, bool Secondary = false, Expression? Argument = null) : Statement
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
		string Name, SqlList<Clause>? Options = null, SqlList<Clause>? With = null) : Statement
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

	/// <summary><c>ALTER DATABASE … MODIFY BACKUP_STORAGE_REDUNDANCY</c>, Azure SQL Database's.</summary>
	public sealed record AlterDatabaseModifyBackupStorageRedundancy(string Name) : Definition(Name);

	// ---- the SET statements --------------------------------------------------------------------

	/// <summary>
	/// The SET statements: what follows <c>SET</c>, as <see cref="SetExpression"/>s — one setting,
	/// or several of one form, <c>SET ANSI_NULLS, NOCOUNT ON</c>, <c>SET DATEFIRST 1, DATEFORMAT dmy</c>.
	/// A variable assigned is <see cref="SetVariable"/>, the other construction of the word.
	/// </summary>
	public sealed record SetStatement(SqlList<SetExpression> Items) : Statement
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
	/// <param name="Name">
	/// The variable, and the property or method of it where one was named: <c>@g.STSrid</c>,
	/// <c>@x.modify</c>, as <see cref="Clause.Set"/> names a column's.
	/// </param>
	/// <param name="Operator">
	/// The assignment as written — <c>=</c>, <c>+=</c>, …; empty where a method was called, whose
	/// value is then the call's arguments.
	/// </param>
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
		SqlList<string> Privileges, SqlList<string> Principals,
		string? On = null, bool GrantOption = false, string? As = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DENY</c>: what is being said about, and to whom.</summary>
	public sealed record Deny(
		SqlList<string> Privileges, SqlList<string> Principals,
		string? On = null, bool Cascade = false, string? As = null) : Statement
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>REVOKE</c>: what is being said about, and to whom — or from whom.</summary>
	public sealed record Revoke(
		SqlList<string> Privileges, SqlList<string> Principals,
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

	/// <summary>
	/// <c>ALTER SERVICE MASTER KEY</c>: the server's own key regenerated, or told the account
	/// the service runs under has changed.
	/// </summary>
	public sealed record AlterServiceMasterKey(string Name) : Definition(Name);


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
	public sealed record DropAggregate(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP APPLICATION ROLE</c>.</summary>
	public sealed record DropApplicationRole(SqlList<Expression> Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP AVAILABILITY GROUP</c>.</summary>
	public sealed record DropAvailabilityGroup(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP BROKER PRIORITY</c>.</summary>
	public sealed record DropBrokerPriority(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP CERTIFICATE</c>.</summary>
	public sealed record DropCertificate(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP COLUMN ENCRYPTION KEY</c>.</summary>
	public sealed record DropColumnEncryptionKey(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP COLUMN MASTER KEY</c>.</summary>
	public sealed record DropColumnMasterKey(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP CONTRACT</c>.</summary>
	public sealed record DropContract(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP CREDENTIAL</c>.</summary>
	public sealed record DropCredential(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP CRYPTOGRAPHIC PROVIDER</c>.</summary>
	public sealed record DropCryptographicProvider(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP DATABASE AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropDatabaseAuditSpecification(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP DATABASE SCOPED CREDENTIAL</c>.</summary>
	public sealed record DropDatabaseScopedCredential(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP DATABASE</c>.</summary>
	public sealed record DropDatabase(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP DEFAULT</c>.</summary>
	public sealed record DropDefault(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP ENDPOINT</c>.</summary>
	public sealed record DropEndpoint(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL DATA SOURCE</c>.</summary>
	public sealed record DropExternalDataSource(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL FILE FORMAT</c>.</summary>
	public sealed record DropExternalFileFormat(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL LANGUAGE</c>.</summary>
	public sealed record DropExternalLanguage(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL MODEL</c>.</summary>
	public sealed record DropExternalModel(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL RESOURCE POOL</c>.</summary>
	public sealed record DropExternalResourcePool(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL TABLE</c>.</summary>
	public sealed record DropExternalTable(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT CATALOG</c>.</summary>
	public sealed record DropFulltextCatalog(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT STOPLIST</c>.</summary>
	public sealed record DropFulltextStoplist(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP FUNCTION</c>.</summary>
	public sealed record DropFunction(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP LOGIN</c>.</summary>
	public sealed record DropLogin(SqlList<Expression> Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP MESSAGE TYPE</c>.</summary>
	public sealed record DropMessageType(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP PARTITION FUNCTION</c>.</summary>
	public sealed record DropPartitionFunction(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP PARTITION SCHEME</c>.</summary>
	public sealed record DropPartitionScheme(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP PROCEDURE</c>.</summary>
	public sealed record DropProcedure(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP QUEUE</c>.</summary>
	public sealed record DropQueue(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP REMOTE SERVICE BINDING</c>.</summary>
	public sealed record DropRemoteServiceBinding(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP RESOURCE POOL</c>.</summary>
	public sealed record DropResourcePool(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP ROLE</c>.</summary>
	public sealed record DropRole(SqlList<Expression> Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP ROUTE</c>.</summary>
	public sealed record DropRoute(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP RULE</c>.</summary>
	public sealed record DropRule(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SCHEMA</c>.</summary>
	public sealed record DropSchema(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SEARCH PROPERTY LIST</c>.</summary>
	public sealed record DropSearchPropertyList(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SECURITY POLICY</c>.</summary>
	public sealed record DropSecurityPolicy(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SEQUENCE</c>.</summary>
	public sealed record DropSequence(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SERVER AUDIT SPECIFICATION</c>.</summary>
	public sealed record DropServerAuditSpecification(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SERVER AUDIT</c>.</summary>
	public sealed record DropServerAudit(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SERVER ROLE</c>.</summary>
	public sealed record DropServerRole(SqlList<Expression> Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP SERVICE</c>.</summary>
	public sealed record DropService(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP STATISTICS</c>.</summary>
	public sealed record DropStatistics(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SYNONYM</c>.</summary>
	public sealed record DropSynonym(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP TABLE</c>.</summary>
	public sealed record DropTable(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP TYPE</c>.</summary>
	public sealed record DropType(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP USER</c>.</summary>
	public sealed record DropUser(SqlList<Expression> Names) : Removal(Names)
	{
		/// <inheritdoc/>
		public override StatementCategory Category => StatementCategory.Dcl;
	}

	/// <summary><c>DROP VIEW</c>.</summary>
	public sealed record DropView(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP WORKLOAD CLASSIFIER</c>.</summary>
	public sealed record DropWorkloadClassifier(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP WORKLOAD GROUP</c>.</summary>
	public sealed record DropWorkloadGroup(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP XML SCHEMA COLLECTION</c>.</summary>
	public sealed record DropXmlSchemaCollection(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP ASYMMETRIC KEY</c>.</summary>
	public sealed record DropAsymmetricKey(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SYMMETRIC KEY</c>.</summary>
	public sealed record DropSymmetricKey(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP ASSEMBLY</c>.</summary>
	public sealed record DropAssembly(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EXTERNAL LIBRARY</c>.</summary>
	public sealed record DropExternalLibrary(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EVENT SESSION</c>.</summary>
	public sealed record DropEventSession(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP EVENT NOTIFICATION</c>.</summary>
	public sealed record DropEventNotification(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP FULLTEXT INDEX</c>.</summary>
	public sealed record DropFulltextIndex(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP INDEX</c>.</summary>
	public sealed record DropIndex(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP SIGNATURE</c>.</summary>
	/// <param name="From">The module the signature is dropped off.</param>
	/// <param name="Counter">Whether it is a counter signature.</param>
	public sealed record DropSignature(
		SqlList<Expression> Names, string? From = null, bool Counter = false) : Removal(Names);

	/// <summary><c>DROP SENSITIVITY CLASSIFICATION</c>.</summary>
	public sealed record DropSensitivityClassification(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP TRIGGER</c>.</summary>
	public sealed record DropTrigger(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP MASTER KEY</c>.</summary>
	public sealed record DropMasterKey(SqlList<Expression> Names) : Removal(Names);

	/// <summary><c>DROP DATABASE ENCRYPTION KEY</c>.</summary>
	public sealed record DropDatabaseEncryptionKey(SqlList<Expression> Names) : Removal(Names);

	// ---- how a parser makes these ------------------------------------------------------------
	//
	// The statements build through these rather than inline in their `=>`, and that is not
	// style: `GRAM5003` said the materializing method for the statement entry point was past
	// the size at which the JIT stops optimizing, and the diagnostic's own advice is to build
	// the value in a method of your own.

	/// <summary>What a call with no statements is handed, once rather than per call.</summary>
	public static readonly SqlList<Statement> None = [];

	/// <summary>The statement the word names, for the ten that are a word and a value.</summary>
	public static Statement Commanded(string word, Expression? value)
	{
		return word switch
		{
			"PRINT" => new Print(value!),
			"RETURN" => new Return(value),
			"THROW" => new Throw(value is null ? Expression.None : [value]),
			"GOTO" => new GoTo(value!),
			"BREAK" => new Break(),
			"CONTINUE" => new Continue(),
			"CHECKPOINT" => new Checkpoint(value),
			"USE" => new Use(value!),
			"RAISERROR" => new RaiseError(value is null ? Expression.None : [value]),
			"WAITFOR" => new WaitFor(value!),
			_ => throw Syntax.Unknown(word),
		};
	}

	/// <summary>The same where the values arrived as a list that may not be there.</summary>
	public static Statement Commanded(string word, SqlList<Expression>? values)
	{
		return word switch
		{
			"PRINT" => new Print(values is { Length: > 0 } some ? some[0] : null!),
			"RETURN" => new Return(values is { Length: > 0 } some ? some[0] : null),
			"THROW" => new Throw(values ?? []),
			"GOTO" => new GoTo(values is { Length: > 0 } some ? some[0] : null!),
			"BREAK" => new Break(),
			"CONTINUE" => new Continue(),
			"CHECKPOINT" => new Checkpoint(values is { Length: > 0 } some ? some[0] : null),
			"USE" => new Use(values is { Length: > 0 } some ? some[0] : null!),
			"RAISERROR" => new RaiseError(values ?? []),
			"WAITFOR" => new WaitFor(values is { Length: > 0 } some ? some[0] : null!),
			_ => throw Syntax.Unknown(word),
		};
	}

	/// <summary>What is being backed up, as the statement it is.</summary>
	public static Statement BackedUp(string what, string? name, string? tail = null)
	{
		return BackedUp(what, name ?? "") with { Tail = tail };
	}

	static Definition BackedUp(string what, string name)
	{
		return what switch
		{
			"DATABASE" => new BackupDatabase(name),
			"LOG" => new BackupTransactionLog(name),
			"SERVER" => new BackupServer(name),
			"GROUP" => new BackupGroup(name),
			"CERTIFICATE" => new BackupCertificate(name),
			"MASTER KEY" => new BackupMasterKey(name),
			"SERVICE MASTER KEY" => new BackupServiceMasterKey(name),
			"SYMMETRIC KEY" => new BackupSymmetricKey(name),
			_ => throw Syntax.Unknown(what),
		};
	}

	/// <summary>What is being restored, likewise.</summary>
	public static Statement Restored(string what, string? name, string? tail = null)
	{
		return Restored(what, name ?? "") with { Tail = tail };
	}

	static Definition Restored(string what, string name)
	{
		return what switch
		{
			"DATABASE" => new RestoreDatabase(name),
			"LOG" => new RestoreLog(name),
			"FILELISTONLY" => new RestoreFileListOnly(name),
			"HEADERONLY" => new RestoreHeaderOnly(name),
			"LABELONLY" => new RestoreLabelOnly(name),
			"REWINDONLY" => new RestoreRewindOnly(name),
			"VERIFYONLY" => new RestoreVerifyOnly(name),
			"MASTER KEY" => new RestoreMasterKey(name),
			"SERVICE MASTER KEY" => new RestoreServiceMasterKey(name),
			"SYMMETRIC KEY" => new RestoreSymmetricKey(name),
			_ => throw Syntax.Unknown(what),
		};
	}

	/// <summary>The definition the words name, where the tree keeps the name and no more.</summary>
	public static Statement Defined(
		string what, string name, string? tail = null, string? verb = null, SqlList<Clause>? options = null)
	{
		return Named(what, name) with { Tail = tail, Verb = verb is null ? null : Syntax.Squared(verb), Options = options };
	}

	/// <summary>A list held under one name: a private key's settings, an Always Encrypted value's parts.</summary>
	public static Clause[] Holding(string name, SqlList<Clause>? settings)
	{
		return settings is not { } held ? [] : [new Clause.Option(name, null, held)];
	}

	static Definition Named(string what, string name)
	{
		return what switch
		{
			"CREATE LOGIN" => new CreateLogin(name),
			"ALTER LOGIN" => new AlterLogin(name),
			"CREATE USER" => new CreateUser(name),
			"ALTER USER" => new AlterUser(name),
			"CREATE ROLE" => new CreateRole(name),
			"ALTER ROLE" => new AlterRole(name),
			"CREATE SERVER ROLE" => new CreateServerRole(name),
			"ALTER SERVER ROLE" => new AlterServerRole(name),
			"CREATE APPLICATION ROLE" => new CreateApplicationRole(name),
			"ALTER APPLICATION ROLE" => new AlterApplicationRole(name),
			"CREATE SCHEMA" => new SchemaDefinition(name),
			"ALTER SCHEMA" => new AlterSchema(name),
			"ALTER AUTHORIZATION" => new AlterAuthorization(name),
			"EXTERNAL DATA SOURCE" => new ExternalDataSourceDefinition(name),
			"EXTERNAL FILE FORMAT" => new ExternalFileFormatDefinition(name),
			"EXTERNAL LIBRARY" => new ExternalLibraryDefinition(name),
			"EXTERNAL RESOURCE POOL" => new ExternalResourcePoolDefinition(name),
			"RESOURCE POOL" => new ResourcePoolDefinition(name),
			"WORKLOAD GROUP" => new WorkloadGroupDefinition(name),
			"SERVER AUDIT" => new ServerAuditDefinition(name),
			"AUDIT SPECIFICATION" => new AuditSpecificationDefinition(name),
			"DATABASE AUDIT SPECIFICATION" => new DatabaseAuditSpecificationDefinition(name),
			"EVENT NOTIFICATION" => new EventNotificationDefinition(name),
			"AVAILABILITY GROUP" => new AvailabilityGroupDefinition(name),
			"MESSAGE TYPE" => new MessageTypeDefinition(name),
			"CONTRACT" => new ContractDefinition(name),
			"QUEUE" => new QueueDefinition(name),
			"SERVICE" => new ServiceDefinition(name),
			"ROUTE" => new RouteDefinition(name),
			"REMOTE SERVICE BINDING" => new RemoteServiceBindingDefinition(name),
			"BROKER PRIORITY" => new BrokerPriorityDefinition(name),
			"ALTER RESOURCE GOVERNOR" => new AlterResourceGovernor(name),
			"ALTER SERVER CONFIGURATION" => new AlterServerConfiguration(name),
			"OPEN SYMMETRIC KEY" => new OpenSymmetricKey(name),
			"OPEN MASTER KEY" => new OpenMasterKey(name),
			"CLOSE SYMMETRIC KEY" => new CloseSymmetricKey(name),
			"CLOSE ALL SYMMETRIC KEYS" => new CloseAllSymmetricKeys(name),
			"CLOSE MASTER KEY" => new CloseMasterKey(name),
			"PARTITION FUNCTION" => new PartitionFunctionDefinition(name),
			"ALTER PARTITION FUNCTION" => new AlterPartitionFunction(name),
			"PARTITION SCHEME" => new PartitionSchemeDefinition(name),
			"ALTER PARTITION SCHEME" => new AlterPartitionScheme(name),
			"SEQUENCE" => new SequenceDefinition(name),
			"TYPE" => new TypeDefinition(name),
			"XML SCHEMA COLLECTION" => new XmlSchemaCollectionDefinition(name),
			"ALTER XML SCHEMA COLLECTION" => new AlterXmlSchemaCollection(name),
			"SYNONYM" => new SynonymDefinition(name),
			"ASSEMBLY" => new AssemblyDefinition(name),
			"CRYPTOGRAPHIC PROVIDER" => new CryptographicProviderDefinition(name),
			"EXTERNAL LANGUAGE" => new ExternalLanguageDefinition(name),
			"EXTERNAL MODEL" => new ExternalModelDefinition(name),
			"RULE" => new RuleDefinition(name),
			"DEFAULT" => new DefaultDefinition(name),
			"AGGREGATE" => new AggregateDefinition(name),

			"FULLTEXT INDEX" => new FullTextIndexDefinition(name),
			"ALTER FULLTEXT INDEX" => new AlterFullTextIndex(name),
			"FULLTEXT CATALOG" => new FullTextCatalogDefinition(name),
			"ALTER FULLTEXT CATALOG" => new AlterFullTextCatalog(name),
			"FULLTEXT STOPLIST" => new FullTextStopListDefinition(name),
			"ALTER FULLTEXT STOPLIST" => new AlterFullTextStopList(name),
			"SEARCH PROPERTY LIST" => new SearchPropertyListDefinition(name),
			"ALTER SEARCH PROPERTY LIST" => new AlterSearchPropertyList(name),

			"ASYMMETRIC KEY" => new AsymmetricKeyDefinition(name),
			"ALTER ASYMMETRIC KEY" => new AlterAsymmetricKey(name),
			"SYMMETRIC KEY" => new SymmetricKeyDefinition(name),
			"ALTER SYMMETRIC KEY" => new AlterSymmetricKey(name),
			"CERTIFICATE" => new CertificateDefinition(name),
			"ALTER CERTIFICATE" => new AlterCertificate(name),
			"MASTER KEY" => new MasterKeyDefinition(name),
			"ALTER MASTER KEY" => new AlterMasterKey(name),
			"ALTER SERVICE MASTER KEY" => new AlterServiceMasterKey(name),
			"DATABASE ENCRYPTION KEY" => new DatabaseEncryptionKeyDefinition(name),
			"ALTER DATABASE ENCRYPTION KEY" => new AlterDatabaseEncryptionKey(name),
			"COLUMN ENCRYPTION KEY" => new ColumnEncryptionKeyDefinition(name),
			"ALTER COLUMN ENCRYPTION KEY" => new AlterColumnEncryptionKey(name),
			"COLUMN MASTER KEY" => new ColumnMasterKeyDefinition(name),
			"CREDENTIAL" => new CredentialDefinition(name),
			"DATABASE SCOPED CREDENTIAL" => new DatabaseScopedCredentialDefinition(name),
			"SECURITY POLICY" => new SecurityPolicyDefinition(name),
			_ => throw Syntax.Unknown(what),
		};
	}

	/// <summary>What is being done to a database, as the statement it is.</summary>
	public static Statement OfDatabase(string name, string action, SqlList<Clause>? settings, string? tail = null)
	{
		return OfDatabase(name, action, settings) is Definition made ? made with { Tail = Syntax.Tail(tail) } : OfDatabase(name, action, settings);
	}

	static Statement OfDatabase(string name, string action, SqlList<Clause>? settings)
	{
		return action switch
		{
			"CREATE" => new CreateDatabase(name, settings ?? []),
			"SET" => new AlterDatabaseSet(name, settings ?? []),
			"COLLATE" => new AlterDatabaseCollate(name),
			"MODIFY NAME" => new AlterDatabaseModifyName(name),
			"MODIFY FILEGROUP" => new AlterDatabaseModifyFileGroup(name),
			"MODIFY FILE" => new AlterDatabaseModifyFile(name),
			"MODIFY" => new AlterDatabaseModify(name),
			"ADD FILEGROUP" => new AlterDatabaseAddFileGroup(name),
			"ADD LOG FILE" => new AlterDatabaseAddLogFile(name),
			"ADD FILE" => new AlterDatabaseAddFile(name),
			"REMOVE FILEGROUP" => new AlterDatabaseRemoveFileGroup(name),
			"REMOVE FILE" => new AlterDatabaseRemoveFile(name),
			"REBUILD LOG" => new AlterDatabaseRebuildLog(name),
			"PERFORM_CUTOVER" => new AlterDatabasePerformCutover(name),
			"MODIFY BACKUP_STORAGE_REDUNDANCY" => new AlterDatabaseModifyBackupStorageRedundancy(name),
			_ => throw Syntax.Unknown(action),
		};
	}


	/// <summary>An alteration before the table it is applied to is known.</summary>
	public static AlterTable Altered(
		string action, SqlList<Clause>? elements, SqlList<Clause>? options = null, string? tail = null)
	{
		return new("", action, elements ?? [], options, Syntax.Tail(tail));
	}

	/// <summary>A column altered by one word — <c>ADD SPARSE</c>, <c>DROP PERSISTED</c>.</summary>
	public static AlterTable Flagged(string name, string flag, SqlList<Clause>? options)
	{
		return new("", "ALTER COLUMN", [new Clause.ColumnDefinition(name, null, null, [Clause.Optioned(flag)])], options);
	}

	/// <summary>The <c>DROP</c> the word names, which is what a <c>DROP</c> statement is.</summary>
	/// <remarks>
	/// The word arrives as it was written — the case the author used, and whatever spacing
	/// stood between its parts — so it is squared up before it is asked about. This and the
	/// grammar's own <c>DropKind</c> are two spellings of one catalogue and have to agree; where
	/// they have drifted apart this says so rather than quietly building the wrong node, which
	/// is a defect in this file and not in anybody's SQL.
	/// </remarks>
	public static Statement Dropped(
		string kind, SqlList<Expression>? some, string? tail = null, string? ifExists = null)
	{
		return Removed(kind, some) switch
		{
			Removal made => made with { Tail = Syntax.Tail(tail), IfExists = ifExists is not null },
			var other => other,
		};
	}

	static Statement Removed(string kind, SqlList<Expression>? some)
	{
		var names = some ?? [];

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
/// What follows <c>SET</c> in a SET statement: one setting, in the group Microsoft's reference
/// puts it in, each group with the settings of its own shapes.
/// </summary>
/// <remarks>
/// A statement's list holds one form: switches turned on or off together, settings given
/// values, the statistics reported, the offsets returned — the engine refuses a switch among
/// values, or a variable among either. A switch is one setting to a node, so a list of them
/// may hold several groups, and the writer puts the one <c>ON</c> after them all again.
/// </remarks>
public abstract record SetExpression : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected SetExpression()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected SetExpression(SetExpression original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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
public abstract record Query : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected Query()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected Query(Query original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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
		SqlList<Clause> Columns,
		Clause? Into,
		SqlList<TableReference> From,
		Expression? Where,
		Clause? GroupBy,
		Expression? Having,
		SqlList<Clause>? Windows = null) : Query;

	/// <summary>§7.3 <c>VALUES (…), (…)</c> — a table written out.</summary>
	public sealed record TableValueConstructor(SqlList<Expression> Rows) : Query;

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
	public sealed record FromFile(Expression File, SqlList<Clause>? Options = null) : Query;

	/// <summary>T-SQL's <c>INSERT BULK</c>: the rows a client streams, named by the columns.</summary>
	/// <remarks>
	/// No file and no values: the statement says what the columns are and the rows arrive over
	/// the connection. A <see cref="FromFile"/> without a file would say the same thing and
	/// leave every reader of one to ask whether the file is there, so it is its own record.
	///
	/// The columns are held here and not in <see cref="Statement.Insert.Columns"/>, which is a
	/// list of names: these carry a type, and may carry only a name where the column is the
	/// engine's own.
	/// </remarks>
	public sealed record FromStream(SqlList<Clause>? Columns = null, SqlList<Clause>? Options = null) : Query;

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
	public static bool IsDistinct(string? quantifier)
	{
		return quantifier is not null && (quantifier[0] | 0x20) == 'd';
	}

	/// <summary>Which set operator was written, and whether it keeps duplicates.</summary>
	public static Query Combined(string operatorText, string? all, Query left, Query right)
	{
		return (operatorText[0] | 0x20) switch
		{
			'u' => new Union(left, right, all is not null),
			'e' => new Except(left, right, all is not null),
			_ => new Intersect(left, right, all is not null),
		};
	}
}

/// <summary>§6 the value level, and §8 the predicates: what stands where a value does.</summary>
public abstract record Expression : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected Expression()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected Expression(Expression original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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

	/// <summary>
	/// T-SQL's <c>&lt;&lt;</c>, as weak as <c>+</c> and read left to right with it: the engine
	/// answers <c>1 &lt;&lt; 1 + 1</c> with 3 and <c>1 + 1 &lt;&lt; 1</c> with 4.
	/// </summary>
	public sealed record ShiftLeft(Expression Left, Expression Right) : Expression;

	/// <summary>T-SQL's <c>&gt;&gt;</c>, as weak as <c>+</c>: <c>8 &gt;&gt; 1 + 1</c> is 5.</summary>
	public sealed record ShiftRight(Expression Left, Expression Right) : Expression;

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
		string Name, SqlList<Expression> Arguments, string? Word = null) : Expression;

	/// <summary>
	/// §6.12 a <c>CASE</c>, simple where it has an operand and searched where it does not.
	/// </summary>
	public sealed record Case(Expression? Operand, SqlList<Clause> Whens, Expression? Else) : Expression;

	/// <summary>§6.7 a column reference, as written, dots and all.</summary>
	public sealed record ColumnReference(string Text) : Expression;

	/// <summary>
	/// §5.3 a literal, a parameter, or one of the words that stand where a value does —
	/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
	/// </summary>
	public sealed record Literal(SqlLiteralKind Kind, string Text) : Expression;

	/// <summary>§7.1 a row of several values, <c>(a, b)</c>.</summary>
	public sealed record RowValueConstructor(SqlList<Expression> Values) : Expression;

	/// <summary>
	/// The <c>ORDER (c1 ASC, c2 DESC) UNIQUE</c> a bulk rowset is declared to arrive in,
	/// which is an argument of <c>OPENROWSET</c> and not a clause of the query.
	/// </summary>
	public sealed record RowsetOrder(SqlList<Clause> By, bool IsUnique) : Expression;

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
		Expression Of, string By, string Name, SqlList<Expression>? Arguments) : Expression;

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
	/// A value and the name it is given where it stands — <c>DATA = dbo.t AS d</c> in a
	/// <c>PREDICT</c>, the one argument a name may follow.
	/// </summary>
	/// <param name="As">Whether <c>AS</c> stood between the two.</param>
	public sealed record Aliased(Expression Value, string Alias, bool As) : Expression;

	/// <summary>
	/// One argument written as several pieces with semicolons between them — <c>OPENROWSET
	/// ('SQLOLEDB', 'server'; 'user'; 'password', …)</c>, that function's oldest spelling and
	/// the one place T-SQL joins two values with a semicolon.
	/// </summary>
	public sealed record Pieced(SqlList<Expression> Parts) : Expression;

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
	public static readonly SqlList<Expression> None = [];

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
	/// bitwise three and its two shifts among them, which bind as weakly.
	/// </summary>
	public static Expression Additive(string operatorText, Expression left, Expression right)
	{
		return operatorText switch
		{
			"+" => new Add(left, right),
			"-" => new Subtract(left, right),
			"&" => new BitwiseAnd(left, right),
			"|" => new BitwiseOr(left, right),
			"^" => new BitwiseXor(left, right),
			"<<" => new ShiftLeft(left, right),
			">>" => new ShiftRight(left, right),
			_ => new Concatenate(left, right),
		};
	}

	/// <summary>Likewise for the multiplicative operators, T-SQL's <c>%</c> among them.</summary>
	public static Expression Multiplicative(string operatorText, Expression left, Expression right)
	{
		return operatorText switch
		{
			"*" => new Multiply(left, right),
			"/" => new Divide(left, right),
			_ => new Modulo(left, right),
		};
	}

	/// <summary>Likewise for the sign in front of one operand, T-SQL's <c>~</c> among them.</summary>
	public static Expression Signed(string sign, Expression operand)
	{
		return sign switch
		{
			"-" => new Negate(operand),
			"~" => new BitwiseNot(operand),
			_ => new Plus(operand),
		};
	}

	/// <summary>
	/// The left operand written into the tail the predicate was read as.
	/// </summary>
	/// <remarks>
	/// The row is read once for all the predicates that begin with one, so the tail is built
	/// without it and the slot it left is filled here. A <c>with</c> rather than a write: the
	/// tail was built with its left side null and no one has seen it, so the copy costs one
	/// allocation and the array the operands used to live in costs none.
	/// </remarks>
	public static Expression Predicated(Expression left, Expression tail)
	{
		return tail switch
		{
			Comparison c => c with { Left = left },
			Quantified q => q with { Left = left },
			Between b => b with { Value = left },
			In i => i with { Value = left },
			Like l => l with { Value = left },
			IsNull n => n with { Value = left },
			Match m => m with { Value = left },
			Overlaps o => o with { Left = left },
			_ => throw new ArgumentOutOfRangeException(
				nameof(tail), tail,
				"The grammar read a predicate tail this method does not know how to complete."),
		};
	}
}

/// <summary>
/// §7.6 what a <c>FROM</c> clause is read over: a table by name, a query standing where one
/// does, and the joins between them.
/// </summary>
public abstract record TableReference : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected TableReference()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected TableReference(TableReference original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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
	/// <param name="Server">
	/// <c>OPENDATASOURCE (…)</c>, where it stood for the server the table is on: the call,
	/// with <see cref="Table"/> the rest of the name after it.
	/// </param>
	public sealed record Named(
		string Table, Clause? SystemTime, bool ForPath, string? Name, SqlList<string>? Columns,
		Clause? Sample, SqlList<Clause> Hints, Expression.RoutineInvocation? Server = null) : TableReference;

	/// <summary>
	/// T-SQL's <c>PIVOT</c>: a source, the aggregate to turn its rows into columns with,
	/// the column whose values name them, and which of those values to keep.
	/// </summary>
	public sealed record Pivot(
		TableReference Of, Expression Aggregate, string For, SqlList<string> In,
		string? Name) : TableReference;

	/// <summary>T-SQL's <c>UNPIVOT</c>, which is the same thing said backwards.</summary>
	public sealed record Unpivot(
		TableReference Of, string Value, string For, SqlList<string> In, string? Name) : TableReference;

	/// <summary>
	/// §7.6 a query standing where a table does, T-SQL's <c>FOR PATH</c> after it as after a
	/// table named.
	/// </summary>
	public sealed record Derived(Query Query, bool ForPath, string? Name, SqlList<string>? Columns) : TableReference;

	/// <summary>
	/// A statement standing where a table does, its <c>OUTPUT</c> the rows it is read as: <c>INSERT
	/// … SELECT … FROM (MERGE … OUTPUT …) AS c</c>, which only an <c>INSERT</c>'s rows may be.
	/// </summary>
	public sealed record Changed(Statement Statement, string Name, SqlList<string>? Columns) : TableReference;


	/// <summary>
	/// §7.6 a function standing where a table does — T-SQL's rowset functions, and any
	/// table-valued function called in a <c>FROM</c> clause.
	/// </summary>
	public sealed record FunctionCall(
		Expression.RoutineInvocation Function, string? Name, SqlList<string>? Columns,
		SqlList<Clause> Schema) : TableReference;

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
		Expression? On = null, SqlList<string>? Using = null, string? Hint = null) : TableReference;

	/// <summary>What a clause with no sources is handed, once rather than per call.</summary>
	public static readonly SqlList<TableReference> None = [];

	/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
	public static Joined Joining(
		string? kind, string? natural, TableReference left, TableReference right,
		Expression? on, SqlList<string>? columns, string? hint = null)
	{
		return new(Syntax.Joined(kind), Syntax.Outer(kind), natural is not null, left, right, on, columns, hint);
	}
}

/// <summary>
/// The pieces a statement, a query or an expression is made of that are none of the three —
/// what the standard writes as a clause, a specification or a definition of one element.
/// </summary>
public abstract record Clause : ISqlSpan, ISqlLocatable
{
	/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
	SqlSyntaxLocation _location;

	/// <summary>A node not yet read from anywhere.</summary>
	protected Clause()
	{
	}

	/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
	protected Clause(Clause original)
	{
		_location = original._location.Copied();
	}

	/// <inheritdoc cref="ISqlSpan.Span"/>
	public SqlSpan Span => _location.Span;

	/// <inheritdoc cref="ISqlSpan.Locate"/>
	public void Locate(int at, int length)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length));
	}

	void ISqlLocatable.Locate(int at, int length, int gapStart)
	{
		_location = new SqlSyntaxLocation(new SqlSpan(at, length, gapStart));
	}

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
	public sealed record OrderBy(SqlList<Clause> By, Expression? Offset, Expression? Fetch) : Clause;

	/// <summary>
	/// T-SQL's <c>TOP (n) PERCENT WITH TIES</c> — how many rows a query specification hands
	/// back, which the standard says with <see cref="OrderBy.Fetch"/> instead.
	/// </summary>
	public sealed record Top(Expression Value, bool Percent, string? With) : Clause;

	/// <summary>
	/// §7.9 <c>GROUP BY</c>: what the rows are grouped by, and the two things T-SQL writes
	/// around the list — <c>ALL</c> in front and <c>WITH CUBE</c> or <c>WITH ROLLUP</c> after.
	/// </summary>
	public sealed record GroupBy(bool All, SqlList<Expression> By, string? With) : Clause;

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
		string? Name, SqlList<Expression> PartitionBy, Clause? By, string? Frame) : Clause;

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
	public sealed record SystemTime(string Kind, SqlList<Expression> At) : Clause;

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
	/// <param name="Ordinal">
	/// Which column of the file it is, where a bulk rowset numbers them; null where none was
	/// said. A column has this or a path, never both: the number is `OPENROWSET (BULK …)`'s and
	/// the path is `OPENXML`'s pattern or `OPENJSON`'s.
	/// </param>
	public sealed record JsonColumn(
		string Name, string? Type, string? Path, bool AsJson, string? Ordinal = null) : Clause;

	/// <summary>§7.17 a named query, written in front of the statement that uses it.</summary>
	public sealed record CommonTableExpression(
		string Name, SqlList<string>? Columns, Query Query) : Clause;

	/// <summary>
	/// T-SQL's <c>FOR XML</c>, <c>FOR JSON</c> and <c>FOR BROWSE</c>: what shape the rows come
	/// back in rather than what they are.
	/// </summary>
	public sealed record For(string Kind, SqlList<string> Options) : Clause;

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
		string Name, Expression? Value, SqlList<Clause> Options, string? Partitions = null, bool Bare = false) : Clause;

	/// <summary>
	/// Where a table or an index is put: the word that says which placement, the filegroup
	/// or partition scheme, and the column a partition scheme is applied on.
	/// </summary>
	public sealed record Placement(string Kind, string Target, SqlList<string>? Columns) : Clause;

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
		SqlList<Clause> Items, TableReference? Target = null, SqlList<string>? Columns = null, Clause? Next = null) : Clause;

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
		string Name, string? Type, Expression? Value, SqlList<Clause>? Elements = null, string? Nullability = null,
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
		SqlList<string> Before, SqlList<string> Options, SqlList<Clause> With, Query Query, Clause? Access, SqlList<Clause> Hints,
		bool AccessFirst = false) : Clause;

	/// <summary><c>FOR READ ONLY</c>, or <c>FOR UPDATE</c> and the columns it is kept to.</summary>
	public sealed record CursorFor(bool Update, SqlList<string>? Columns = null) : Clause;

	/// <summary>A partition by its number, or a range of them: <c>3</c>, <c>3 TO 5</c>.</summary>
	public sealed record PartitionRange(Expression From, Expression? To = null) : Clause;

	/// <summary>
	/// One parameter of a routine: its name, its type, and its default — and the words around
	/// them, <c>VARYING</c> and the nullability before the default, <c>OUTPUT</c> and
	/// <c>READONLY</c> after it.
	/// </summary>
	public sealed record ParameterDeclaration(
		string Name, string? Type, Expression? Value,
		string? Nullability = null, bool Varying = false, SqlList<string>? Ways = null) : Clause;

	/// <summary>
	/// §11.4 one column: its name, the type as written, the expression where it is computed
	/// rather than stored, and everything said about it after that — its options, its
	/// constraints, and the index written on it — in the order it was said.
	/// </summary>
	public sealed record ColumnDefinition(
		string Name, string? Type, Expression? Computed, SqlList<Clause> Options) : Clause;

	/// <summary>
	/// One thing said about a column after its type that is not a constraint: which thing
	/// (<c>SPARSE</c>, <c>NOT NULL</c>, <c>COLLATE</c>, <c>IDENTITY</c>, <c>MASKED</c>, …), what
	/// it was given in brackets or after it, the options where it carries a <c>WITH</c>, and
	/// the constraint name where the language lets a nullability have one.
	/// </summary>
	public sealed record ColumnOption(
		string Kind, SqlList<Expression> Arguments, SqlList<Clause> Options, string? ConstraintName) : Clause;

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
	/// layouts: without them an option stands as a word alone — <c>PAD_INDEX</c>, and
	/// <c>PAD_INDEX = ON</c> is <c>Msg 156</c> — while inside the brackets it has to be
	/// <c>PAD_INDEX = ON</c>. What separates them differs too, and by the statement rather
	/// than by the layout: an index writes commas (<c>WITH FILLFACTOR = 23, PAD_INDEX</c>,
	/// and without one the two words are <c>Msg 102</c>), a key writes none (<c>WITH
	/// sorted_data FILLFACTOR = 12</c>, and a comma there closes the column instead).
	/// </param>
	public sealed record ConstraintDefinition(
		string? Name, string Kind, SqlList<Clause> Columns, Expression? Check,
		string? Clustering = null, bool Hash = false, bool Columnstore = false,
		SqlList<string>? Order = null, SqlList<string>? Include = null, Expression? Filter = null,
		Clause? Referenced = null, SqlList<Clause>? Options = null, SqlList<Clause>? Placements = null,
		bool? Enforced = null, string? ForColumn = null, bool Bracketed = true) : Clause;

	/// <summary>
	/// §11.8 what a foreign key refers to: the table, its columns where they are named, what
	/// happens on delete and on update, and whether replication is told to leave it alone.
	/// </summary>
	public sealed record References(
		string Table, SqlList<string>? Columns, string? OnDelete, string? OnUpdate, bool NotForReplication) : Clause;

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
	public sealed record DatabaseFile(SqlList<Clause> Options) : Clause;

	/// <summary>
	/// A file group in a <c>CREATE DATABASE</c>: its name, what it contains where it says,
	/// whether it is the default, and its files.
	/// </summary>
	public sealed record FileGroup(string Name, string? Contains, bool Default, SqlList<Clause> Files) : Clause;

	/// <summary>
	/// One piece of an event session — <c>ADD EVENT</c>, <c>DROP EVENT</c>, <c>ADD TARGET</c>,
	/// <c>DROP TARGET</c> — with what an added one was given: its settings, its actions, and
	/// its predicate, which is a language of its own and is kept as written.
	/// </summary>
	public sealed record EventPiece(
		string Action, string Name, SqlList<Clause>? Settings = null, SqlList<string>? Actions = null, string? Where = null) : Clause;

	/// <summary>What a statement with no clauses is handed, once rather than per call.</summary>
	public static readonly SqlList<Clause> None = [];

	/// <summary>A constraint written without a name, which is most of them.</summary>
	public static ConstraintDefinition Constrained(
		string kind, SqlList<Clause>? columns, Expression? check)
	{
		return new(null, kind, columns ?? [], check);
	}

	/// <summary>A default and what was written after it: the column it is for, and whether the rows already there take it.</summary>
	/// <remarks>
	/// <c>WITH VALUES</c> is kept as the one option of the constraint, unbracketed, which is
	/// how the writer puts it back after the <c>FOR</c>: <c>ALTER TABLE t ADD CONSTRAINT d
	/// DEFAULT 1 FOR c WITH VALUES</c> is the order the engine reads. Asked of the tail as
	/// written, since the grammar reads it as words and only where a column is added.
	/// </remarks>
	public static ConstraintDefinition Defaulted(Expression value, string? tail)
	{
		const string Valued = "WITH VALUES";

		var words = tail is null ? "" : Syntax.Spaced(tail).Trim();
		var kept  = words.EndsWith(Valued, StringComparison.OrdinalIgnoreCase);

		if (kept)
			words = words.Substring(0, words.Length - Valued.Length).TrimEnd();

		var made = Constrained("DEFAULT", null, value) with
		{
			ForColumn = words.StartsWith("FOR ", StringComparison.OrdinalIgnoreCase) ? words.Substring("FOR ".Length) : null,
		};

		return kept ? made with { Options = [new Option("VALUES", null, None)], Bracketed = false } : made;
	}

	/// <summary>A named thing dropped, where only the kind and the name matter.</summary>
	public static Dropped Marked(string kind, string? name)
	{
		return new(kind, name);
	}

	/// <summary>A dropped thing with what was written around it.</summary>
	public static Dropped Marked(string kind, string? name, string? ifExists, string? tail)
	{
		return new(kind, name, ifExists is not null, Syntax.Tail(tail));
	}

	/// <summary>
	/// A key or a uniqueness: the kind, and everything T-SQL lets stand after it.
	/// </summary>
	public static ConstraintDefinition Keyed(
		string kind, string? clustering, string? hash, SqlList<Clause>? columns,
		SqlList<Clause>? options, Clause? placement, string? enforced)
	{
		return new(null, kind, columns ?? [], null,
			Clustering: clustering, Hash: hash is not null,
			Options: options, Placements: placement is null ? null : [placement],
			Enforced: Syntax.Enforced(enforced));
	}

	/// <summary>An index written inside a table or on its own, with everything that may follow it.</summary>
	public static ConstraintDefinition Indexed(
		string name, string? unique, string? clustering, string? columnstore, string? hash,
		SqlList<Clause>? columns, SqlList<string>? order, SqlList<string>? include, Expression? filter,
		SqlList<Clause>? options, Placement? on, Placement? filestream)
	{
		return new(name, unique is null ? "INDEX" : "UNIQUE INDEX", columns ?? [], null,
			Clustering: clustering, Hash: hash is not null, Columnstore: columnstore is not null,
			Order: order, Include: include, Filter: filter, Options: options,
			Placements: on is null && filestream is null
				? null
				: [.. new[] { on, filestream }.OfType<Clause>()]);
	}

	/// <summary>What a column is told after its type: one word or several, and its argument.</summary>
	public static ColumnOption Optioned(string kind, Expression? argument = null)
	{
		return new(Syntax.Squared(kind), argument is null ? Expression.None : [argument], None, null);
	}

	/// <summary>A column's option or constraint, given the name written in front of it.</summary>
	public static Clause Named(Clause one, string name)
	{
		return one switch
		{
			ColumnOption option => option with { ConstraintName = name },
			ConstraintDefinition constraint => constraint with { Name = name },
			_ => one,
		};
	}

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
	public static OrderBy? Ordered(SqlList<Clause>? by, OrderBy? window)
	{
		return by is null && window is null
			? null
			: new OrderBy(by ?? [], window?.Offset, window?.Fetch);
	}
}
