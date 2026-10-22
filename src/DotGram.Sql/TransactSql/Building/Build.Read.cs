using System;
using System.Collections.Generic;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// What the statement rules gather and hand their builders: the parts of a statement, a query, a table source and a
// setting as the rules read them, before a builder makes the common tree of them in the same action. They are the
// shapes the T-SQL tree had, kept as this assembly's own records with their values on the common tree, so that the
// builders that take them apart read them as they read that tree; nothing outside the builders sees one. Each is told
// where it was written in every reading, located or not, since a builder reads the words the record leaves out from
// the text by its span.
static partial class Build
{
	/// <summary>A record of what a rule read, told where it was written: the base of the builders' own small records.</summary>
	internal abstract record Read : ISqlSpan, ISqlLocatable
	{
		/// <summary>The span, held where the record's equality does not look (<see cref="SqlSyntaxLocation"/>).</summary>
		SqlSyntaxLocation _location;

		/// <summary>A record not yet read from anywhere.</summary>
		protected Read()
		{
		}

		/// <summary>A copy, which keeps where the original was written and says it is stale.</summary>
		protected Read(Read original)
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
	}

	/// <summary>A statement: §13 of the standard, and most of a dialect's reference.</summary>
	internal abstract record Statement : ISqlSpan, ISqlLocatable
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
			TableReference? Target, SqlList<Clause> Set, SqlList<TableReference> From, Ast.Expression? Where,
			SqlList<Clause>? With = null, Clause? Top = null, Clause? Output = null, SqlList<Clause>? Options = null) : Statement;

		/// <summary>§14.9 rows removed, and the same two ways of saying which.</summary>
		public sealed record Delete(
			TableReference? Target, SqlList<TableReference> From, Ast.Expression? Where,
			SqlList<Clause>? With = null, Clause? Top = null, Clause? Output = null, SqlList<Clause>? Options = null) : Statement;

		/// <summary>
		/// §14.12 one statement that inserts, updates and deletes, according to what a join found.
		/// </summary>
		public sealed record Merge(
			TableReference? Target, TableReference Using, Ast.Expression On, SqlList<Clause> Whens,
			SqlList<Clause>? With = null, Clause? Top = null, string? Alias = null, Clause? Output = null, SqlList<Clause>? Options = null,
			bool Into = true) : Statement;

		// ---- the procedural level ----------------------------------------------------------------
		//
		// Structure gets a record and shapelessness does not. A block holds statements, a
		// conditional holds two, a declaration holds a list — each of those is a shape and each
		// has one.

		/// <summary><c>BEGIN … END</c>, and the body of anything that has one.</summary>
		public sealed record Compound(SqlList<Statement> Statements, SqlList<Clause>? Atomic = null) : Statement;

		/// <summary><c>IF … ELSE</c>, where either arm is one statement and a block is one.</summary>
		public sealed record If(Ast.Expression Condition, Statement Then, Statement? Else) : Statement;

		/// <summary><c>WHILE</c>, and the one statement it repeats.</summary>
		public sealed record While(Ast.Expression Condition, Statement Body) : Statement;

		/// <summary><c>BEGIN TRY … END TRY BEGIN CATCH … END CATCH</c>.</summary>
		public sealed record TryCatch(SqlList<Statement> Tried, SqlList<Statement> Caught) : Statement;

		/// <summary>One <c>DECLARE</c>, which may declare several.</summary>
		public sealed record Declare(SqlList<Clause> Variables) : Statement;

		/// <summary>A transaction begun, committed, rolled back or saved, and its name.</summary>
		/// <param name="Word"><c>TRANSACTION</c>, <c>TRAN</c> or <c>WORK</c>, as written.</param>
		/// <param name="Tail">A mark or a durability, as written.</param>
		public sealed record Transaction(
			string Kind, string? Name, string? Word = null, string? Tail = null) : Statement;

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
			string? Into, string Name, SqlList<Ast.Expression> Arguments,
			Clause? Context = null, string? At = null, string? DataSource = null, string? Tail = null,
			Values.Call? Server = null, bool Bare = false, string? Number = null) : Statement;

		// ---- the tables ---------------------------------------------------------------------------

		/// <summary>
		/// §11.1 a table declared: its name, what kind of table T-SQL says it is where it says
		/// one (<c>AS FILETABLE</c>, <c>AS NODE</c>, <c>AS EDGE</c>), the columns, constraints and
		/// indexes in it, where it is placed (<c>ON</c>, <c>TEXTIMAGE_ON</c>, <c>FILESTREAM_ON</c>),
		/// and the options written after it.
		/// </summary>
		public sealed record TableDefinition(
			string Name, string? Kind, SqlList<Clause> Elements, SqlList<Clause> Placements, SqlList<Clause> Options, bool External = false) : Statement;

		/// <summary>
		/// A table whose columns are whatever a query returns — <c>CREATE TABLE … AS SELECT</c>,
		/// and the external table written the same way: the column names where they were given,
		/// the options between the name and the <c>AS</c>, and the query.
		/// </summary>
		public sealed record CreateTableAsSelect(
			string Name, SqlList<string>? Columns, SqlList<Clause> Options, Statement Body, bool External = false) : Statement;

		public abstract record Definition(string Name) : Statement
		{
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

		/// <summary>
		/// A statement its rule built on the common tree, standing where the rules hold a statement: in a text of
		/// statements, a block, a body. Taken out as what it holds.
		/// </summary>
		/// <param name="Made">The statement on the common tree, or null where building it found a gap.</param>
		/// <param name="Gap">What the common tree has no place for yet, which taking the statement throws.</param>
		/// <param name="Given">
		/// What the rule handed its builder: a <c>Build.Given</c>, which names the record the statement is said under, or
		/// the record the rule's parts were gathered into. Held as an object, so that nothing walks into it.
		/// </param>
		internal sealed record Built(Ast.Statement? Made, BuildGap? Gap, object Given) : Statement
		{
			/// <summary>
			/// The builder of a statement whose conversion reads the text the statement was written in, waiting for the extent
			/// the statement ends with: that of the rule that reads the WITH in front of it and the `;` after it, which is
			/// where it is finished (`Build.Dml.Preceded`). Null once it is built.
			/// </summary>
			internal Func<object, DotGram.Sql.Ast.Statement>? Pending { get; init; }

			/// <summary>The record the rule gathered, under which a gap in the statement is said.</summary>
			public string Record => Given is Build.Given given ? given.Record : NameOf(Given);

			/// <summary>
			/// Equal where both hold equal statements, or gaps that say the same; one held until it is carried
			/// (<see cref="Pending"/>), where both were handed what is equal.
			/// </summary>
			public bool Equals(Built? other)
			{
				return other is not null && Equals(Made, other.Made) && Gap?.Message == other.Gap?.Message &&
					(Pending is null && other.Pending is null || Equals(Given, other.Given));
			}

			/// <inheritdoc/>
			public override int GetHashCode()
			{
				return Pending is null ? (Made?.GetHashCode() ?? 0) * 31 + (Gap?.Message.GetHashCode() ?? 0) : Given.GetHashCode();
			}
		}

		/// <param name="Tail">What the action was given, where the tree keeps it as written.</param>
		public sealed record AlterTable(
			string Name, string Action, SqlList<Clause> Elements, SqlList<Clause>? Options = null, string? Tail = null) : Statement;

		// ---- the routines --------------------------------------------------------------------------

		/// <summary>
		/// A procedure: its name, what it takes, and what it does — or the method in an assembly
		/// that does it — with the options between, the number after a <c>;</c>, and whether it
		/// is for replication.
		/// </summary>
		public sealed record CreateProcedure(
			string Name, SqlList<Clause> Parameters, SqlList<Statement> Body,
			SqlList<Clause>? Options = null, bool ForReplication = false, string? External = null, string? Number = null,
			string Verb = "CREATE") : Statement;

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
			SqlList<Clause>? Order = null, string? External = null, string Verb = "CREATE") : Statement;

		/// <summary>A trigger: what it is on, what fires it, and what it does then.</summary>
		/// <param name="When"><c>AFTER</c>, <c>FOR</c> or <c>INSTEAD OF</c>, as written.</param>
		/// <param name="External">The method in an assembly, where that is the body.</param>
		public sealed record CreateTrigger(
			string Name, string On, SqlList<string> Events, SqlList<Statement> Body,
			string When = "AFTER", SqlList<Clause>? Options = null, bool Append = false,
			bool NotForReplication = false, string? External = null, string Verb = "CREATE") : Statement;

		/// <summary>§11.32 a view, which is a name given to a query.</summary>
		/// <param name="Options">The <c>WITH</c> between the name and the <c>AS</c>: <c>SCHEMABINDING</c>, a materialized view's distribution.</param>
		/// <param name="CheckOption">The <c>WITH CHECK OPTION</c> after the query.</param>
		public sealed record ViewDefinition(
			string Name, SqlList<string>? Columns, Statement Body,
			SqlList<Clause>? Options = null, bool CheckOption = false, bool Materialized = false,
			string Verb = "CREATE") : Statement;

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
			string On, Clause Index, string? Kind = null, string? Using = null) : Statement;

		/// <summary>An index changed: which one, on what, and what is being done to it.</summary>
		/// <summary>
		/// <c>ALTER INDEX</c>: the index or <c>ALL</c>, the table, the action, and what the action
		/// was given — the partition, the options, and for a selective XML index the paths it
		/// promotes, each kept as written, with the namespaces in front of them.
		/// </summary>
		public sealed record AlterIndex(
			string Name, string On, string Action,
			Ast.Expression? Partition = null, SqlList<Clause>? Options = null, SqlList<string>? Paths = null, string? Namespaces = null) : Statement;

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
			string Handle, bool Conversation, string From, Ast.Expression To, Ast.Expression? Instance,
			string? Contract, SqlList<Clause> Options) : Statement;

		/// <summary><c>BEGIN CONVERSATION TIMER</c>: how long a conversation waits before it is told.</summary>
		public sealed record ConversationTimer(Ast.Expression Handle, Ast.Expression Timeout) : Statement;

		/// <summary><c>END CONVERSATION</c>, with an error said or the conversation cleaned up.</summary>
		/// <param name="Handle">The conversation, a value.</param>
		/// <param name="Error">The failure's code, where one was given; a description always with it.</param>
		/// <param name="Description">What the failure was.</param>
		/// <param name="Cleanup">Whether <c>WITH CLEANUP</c> ended it without telling the other side.</param>
		public sealed record EndConversation(
			Ast.Expression Handle, Ast.Expression? Error = null, Ast.Expression? Description = null, bool Cleanup = false) : Statement;

		/// <summary><c>MOVE CONVERSATION</c>: a conversation put in another group.</summary>
		public sealed record MoveConversation(Ast.Expression Handle, Ast.Expression Group) : Statement;

		/// <summary><c>SEND</c>: a message sent on one conversation or several.</summary>
		/// <param name="Conversations">The handles it is sent on.</param>
		/// <param name="Bracketed">Whether they stood in brackets, which one may and several must.</param>
		/// <param name="MessageType">The message's type, where one was named.</param>
		/// <param name="Body">What it says, where it says anything.</param>
		public sealed record Send(
			SqlList<Ast.Expression> Conversations, bool Bracketed, string? MessageType = null, Ast.Expression? Body = null) : Statement;

		/// <summary><c>RECEIVE</c>: messages taken off a queue.</summary>
		/// <param name="Top">How many, where it said.</param>
		/// <param name="Columns">What is returned of each, as a select list says it.</param>
		/// <param name="Queue">The queue, by name.</param>
		/// <param name="Into">The table variable they go into, where they go into one.</param>
		/// <param name="Where">
		/// Which conversation or group they are from: one comparison, of one of those two columns.
		/// </param>
		public sealed record Receive(
			Clause? Top, SqlList<Ast.SelectColumn> Columns, string Queue, string? Into = null, Ast.Expression? Where = null) : Statement;

		/// <summary><c>GET CONVERSATION GROUP</c>: the group of a queue's next message, into a variable.</summary>
		public sealed record GetConversationGroup(string Group, string Queue) : Statement;

		/// <summary>
		/// <c>WAITFOR ( … )</c> around a <see cref="Receive"/> or a <see cref="GetConversationGroup"/>:
		/// the statement waited on, and for how long.
		/// </summary>
		/// <param name="Statement">What is waited on.</param>
		/// <param name="Timeout">The <c>TIMEOUT</c>, where one was given.</param>
		public sealed record WaitForStatement(Statement Statement, Ast.Expression? Timeout = null) : Statement;

		// ---- a table emptied, a trigger switched, a session killed, a column classified ------------
		//
		// Four statements, each its own shape, as agreed.

		/// <summary><c>TRUNCATE TABLE</c>: every row of a table removed, or of some of its partitions.</summary>
		/// <param name="Partitions">
		/// What <c>WITH (PARTITIONS (…))</c> named, a <see cref="Clause.PartitionRange"/> each;
		/// null where it named none.
		/// </param>
		public sealed record Truncate(string Table, SqlList<Clause>? Partitions = null) : Statement;

		/// <summary><c>ENABLE TRIGGER</c> or <c>DISABLE TRIGGER</c>.</summary>
		/// <param name="Triggers">The triggers by name; null for <c>ALL</c>.</param>
		/// <param name="On">What they are on: an object's name, <c>DATABASE</c> or <c>ALL SERVER</c>.</param>
		public sealed record TriggerSwitch(bool Enable, SqlList<string>? Triggers, string On) : Statement;

		/// <summary><c>KILL</c>: a session or a unit of work ended, or a subscription, or a statistics job.</summary>
		/// <param name="Kind">
		/// Empty for a session or a unit of work; <c>QUERY NOTIFICATION SUBSCRIPTION</c> or
		/// <c>STATS JOB</c> for the other two.
		/// </param>
		/// <param name="Target">The session, the unit of work, the subscription or the job; <c>ALL</c> for every subscription.</param>
		/// <param name="With"><c>STATUSONLY</c>, <c>COMMIT</c> or <c>ROLLBACK</c>, where one was said.</param>
		public sealed record Kill(string Kind, Ast.Expression Target, string? With = null) : Statement;

		/// <summary><c>ADD SENSITIVITY CLASSIFICATION</c>: columns labelled for what they hold.</summary>
		/// <param name="Add">Whether the classification is added; its removal is <c>DropSensitivityClassification</c>.</param>
		/// <param name="Columns">The columns, by two parts or three.</param>
		/// <param name="Options">What followed <c>WITH</c>, a <see cref="Clause.Option"/> each.</param>
		public sealed record Classification(bool Add, SqlList<string> Columns, SqlList<Clause> Options) : Statement;

		/// <summary><c>READTEXT</c>: part of a <c>text</c>, <c>ntext</c> or <c>image</c> value, read through its pointer.</summary>
		/// <param name="Column">The column, by two parts to four.</param>
		/// <param name="Pointer">The text pointer: a variable, or a binary string.</param>
		/// <param name="Offset">Where the reading starts.</param>
		/// <param name="Size">How much is read.</param>
		/// <param name="HoldLock">Whether <c>HOLDLOCK</c> was said.</param>
		public sealed record ReadText(
			string Column, Ast.Expression Pointer, Ast.Expression Offset, Ast.Expression Size, bool HoldLock = false) : Statement;

		/// <summary><c>WRITETEXT</c>: a <c>text</c>, <c>ntext</c> or <c>image</c> value replaced through its pointer.</summary>
		/// <param name="Data">
		/// What it is replaced with; null where nothing was written, as after <c>BULK</c>, where
		/// the data comes by the bulk protocol instead.
		/// </param>
		/// <param name="Bulk">Whether <c>BULK</c> was said.</param>
		/// <param name="Log">Whether <c>WITH LOG</c> was said.</param>
		/// <param name="Timestamp">The <c>TIMESTAMP = 0x…</c> the row must still carry, where one was said.</param>
		public sealed record WriteText(
			string Column, Ast.Expression Pointer, Ast.Expression? Data, bool Bulk = false, bool Log = false,
			Ast.Expression? Timestamp = null) : Statement;

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
			string Column, Ast.Expression Pointer, Ast.Expression Offset, Ast.Expression Length, Ast.Expression? Data = null,
			string? Source = null, Ast.Expression? SourcePointer = null, bool Bulk = false, bool Log = false,
			Ast.Expression? Timestamp = null) : Statement;

		/// <summary><c>RECONFIGURE</c>: the configuration just changed, taken up.</summary>
		/// <param name="Override">Whether <c>WITH OVERRIDE</c> was said.</param>
		public sealed record Reconfigure(bool Override = false) : Statement;

		/// <summary><c>SHUTDOWN</c>: the server stopped.</summary>
		/// <param name="NoWait">Whether <c>WITH NOWAIT</c> was said, which skips the checkpoint.</param>
		public sealed record Shutdown(bool NoWait = false) : Statement;

		/// <summary><c>LINENO</c>: the line number the next line of the batch is to be counted as.</summary>
		/// <param name="Line">The number, a whole one.</param>
		public sealed record LineNumber(Ast.Expression Line) : Statement;

		// ---- DBCC ----------------------------------------------------------------------------------
		/// <summary><c>DBCC</c>: a console command, what stood in its brackets, and its options.</summary>
		/// <remarks>
		/// One record for every command, the documented and the rest, because the engine reads them
		/// all one way: what a command means is its own business, not the tree's.
		/// </remarks>
		/// <param name="Command">The command as written, <c>CHECKDB</c> or a library's name.</param>
		/// <param name="Arguments">
		/// A value each, or a <see cref="Values.NamedArgument"/>. Null where there were no
		/// brackets, and empty where they held nothing.
		/// </param>
		/// <param name="Options">What followed <c>WITH</c>, a <see cref="Clause.Option"/> each.</param>
		public sealed record Dbcc(string Command, SqlList<Ast.Expression>? Arguments, SqlList<Clause> Options) : Statement;

		// ---- cursors -------------------------------------------------------------------------------
		//
		// T-SQL's cursors: one declared, or set to a variable, and what is done with one. Opening,
		// closing and letting one go are a word and a cursor, and one record says which; a fetch
		// says which row, and where it goes.

		/// <summary>A cursor declared by name, and what it is over.</summary>
		public sealed record DeclareCursor(string Name, Clause.CursorDefinition Cursor) : Statement;

		/// <summary>
		/// A cursor variable set to a cursor defined where it stands, <c>SET @c = CURSOR … FOR …</c>.
		/// Set to another cursor, it is <see cref="SetVariable"/>.
		/// </summary>
		public sealed record SetCursor(string Name, Clause.CursorDefinition Cursor) : Statement;

		/// <summary>A cursor opened, closed or let go.</summary>
		/// <param name="Verb"><c>OPEN</c>, <c>CLOSE</c> or <c>DEALLOCATE</c>.</param>
		/// <param name="Cursor">A cursor's name, or a variable holding one.</param>
		/// <param name="Global">
		/// Whether <c>GLOBAL</c> said the name is the connection's rather than the batch's. Never
		/// before a variable, which the engine refuses.
		/// </param>
		public sealed record CursorAction(string Verb, Ast.Expression Cursor, bool Global) : Statement;

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
			string? Orientation, Ast.Expression? Offset, bool From, Ast.Expression Cursor, bool Global,
			SqlList<Ast.Expression>? Into) : Statement;

		// ---- a word and some values ----------------------------------------------------------------
		//
		// Ten statements that are a word and what follows it. They shared one record until the
		// tree was made to say what the grammar says, and a reader who wanted the `PRINT` had to
		// look at a string to find it.

		/// <summary>One value printed.</summary>
		public sealed record Print(Ast.Expression Value) : Statement;

		/// <summary>A routine left, with a code where one was given.</summary>
		public sealed record Return(Ast.Expression? Value) : Statement;

		/// <summary>An error raised, or the caught one raised again.</summary>
		public sealed record Throw(SqlList<Ast.Expression> Arguments) : Statement;

		/// <summary>A jump to a label.</summary>
		/// <remarks>
		/// The parameter is <c>Name</c> and not <c>Label</c> because <see cref="Label"/> is a
		/// statement of its own here, and a nested type takes the name from a member of the same
		/// class.
		/// </remarks>
		public sealed record GoTo(Ast.Expression Name) : Statement;

		/// <summary>A loop left.</summary>
		public sealed record Break : Statement
		{
		}

		/// <summary>A loop begun again.</summary>
		public sealed record Continue : Statement
		{
		}

		/// <summary>The log written out.</summary>
		public sealed record Checkpoint(Ast.Expression? Value) : Statement;

		/// <summary>The database the rest of the batch is read against.</summary>
		public sealed record Use(Ast.Expression Name) : Statement;

		/// <param name="Tail">The <c>WITH LOG, NOWAIT, SETERROR</c> after it, as written.</param>
		public sealed record RaiseError(SqlList<Ast.Expression> Arguments, string? Tail = null) : Statement;

		/// <summary>A delay, or a time to wait until.</summary>
		/// <param name="Kind">
		/// <c>DELAY</c> or <c>TIME</c> — how long to wait against when to stop waiting, which
		/// the one value cannot say.
		/// </param>
		public sealed record WaitFor(Ast.Expression Value, string Kind = "DELAY") : Statement;

		/// <summary>T-SQL's label, the place a <c>GOTO</c> goes: a name and a colon.</summary>
		/// <remarks>
		/// Not a command, whatever it stands among: nothing is done here, and the name is the
		/// whole of it. <c>GOTO finish</c> names one and <c>finish:</c> is one.
		/// </remarks>
		public sealed record Label(string Name) : Statement;

		/// <summary><c>EXECUTE AS</c>: whom the session runs as until a <c>REVERT</c> says otherwise.</summary>
		/// <param name="Kind"><c>CALLER</c>, <c>USER</c> or <c>LOGIN</c>.</param>
		/// <param name="Name">Whom, for a user or a login: any expression the engine will take.</param>
		/// <param name="NoRevert"><c>WITH NO REVERT</c>: the context is not given back.</param>
		/// <param name="Cookie">The variable <c>WITH COOKIE INTO</c> names, which a <c>REVERT</c> must show.</param>
		public sealed record ExecuteAs(string Kind, Ast.Expression? Name, bool NoRevert = false, Ast.Expression? Cookie = null) : Statement;

		/// <summary><c>REVERT</c>: the context an <c>EXECUTE AS</c> changed, given back.</summary>
		/// <param name="Cookie">What <c>WITH COOKIE =</c> shows for it.</param>
		public sealed record Revert(Ast.Expression? Cookie = null) : Statement;

		/// <summary><c>SETUSER</c>: a user impersonated, the way before <c>EXECUTE AS</c>.</summary>
		/// <param name="Name">The user, or nobody, which puts the original back.</param>
		/// <param name="NoReset"><c>WITH NORESET</c>.</param>
		public sealed record SetUser(Ast.Expression? Name = null, bool NoReset = false) : Statement;

		/// <summary><c>EVENT SESSION</c>.</summary>
		/// <summary>
		/// <c>CREATE</c> or <c>ALTER EVENT SESSION</c>: the session, where it is (<c>SERVER</c> or
		/// <c>DATABASE</c>), the events and targets added and dropped, its options, and the state
		/// it is put in.
		/// </summary>
		public sealed record EventSessionDefinition(
			string Name, string Verb = "CREATE", string On = "SERVER", SqlList<Clause>? Pieces = null,
			SqlList<Clause>? Options = null, string? State = null) : Statement;


		/// <summary><c>ENDPOINT</c>.</summary>
		/// <summary>
		/// <c>CREATE</c> or <c>ALTER ENDPOINT</c>: the owner, the state and what was written
		/// beside it, how it is reached (<c>AS TCP (…)</c>) and what it speaks (<c>FOR TSQL
		/// ()</c>) — each protocol a bracket of options belonging to it.
		/// </summary>
		public sealed record EndpointDefinition(
			string Name, string Verb = "CREATE", string? Owner = null, string? State = null,
			SqlList<Clause>? StateOptions = null, string? Protocol = null, SqlList<Clause>? ProtocolOptions = null,
			string? Payload = null, SqlList<Clause>? PayloadOptions = null) : Statement;

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
			string? Tail = null, SqlList<Clause>? Options = null, SqlList<Clause>? With = null) : Statement;

		/// <summary>
		/// <c>ALTER DATABASE SCOPED CONFIGURATION</c>: <c>SET</c> and what it was set to, or
		/// <c>CLEAR PROCEDURE_CACHE</c> and the plan handle where one was given; for the secondary
		/// where it says so.
		/// </summary>
		public sealed record AlterDatabaseScopedConfiguration(
			string Name, string Action, SqlList<Clause> Settings, bool Secondary = false, Ast.Expression? Argument = null) : Statement;

		// ---- the SET statements --------------------------------------------------------------------

		/// <summary>
		/// The SET statements: what follows <c>SET</c>, as <see cref="SetExpression"/>s — one setting,
		/// or several of one form, <c>SET ANSI_NULLS, NOCOUNT ON</c>, <c>SET DATEFIRST 1, DATEFORMAT dmy</c>.
		/// A variable assigned is <see cref="SetVariable"/>, the other construction of the word.
		/// </summary>
		public sealed record SetStatement(SqlList<SetExpression> Items) : Statement
		{

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
			string Name, Ast.Expression Value, string? Operator = null, string? Through = null) : Statement;

		// ---- §12.1 the permissions -----------------------------------------------------------------

		/// <summary><c>GRANT</c>: what is being said about, and to whom.</summary>
		/// <param name="On">The securable, class and all, as written after <c>ON</c>.</param>
		/// <param name="As">The principal the statement is run as.</param>
		public sealed record Grant(
			SqlList<string> Privileges, SqlList<string> Principals,
			string? On = null, bool GrantOption = false, string? As = null) : Statement;

		/// <summary><c>DENY</c>: what is being said about, and to whom.</summary>
		public sealed record Deny(
			SqlList<string> Privileges, SqlList<string> Principals,
			string? On = null, bool Cascade = false, string? As = null) : Statement;

		/// <summary><c>REVOKE</c>: what is being said about, and to whom — or from whom.</summary>
		public sealed record Revoke(
			SqlList<string> Privileges, SqlList<string> Principals,
			string? On = null, bool GrantOptionFor = false, bool From = false, bool Cascade = false, string? As = null) : Statement;

		// ---- how a parser makes these ------------------------------------------------------------
		//
		// The statements build through these rather than inline in their `=>`, and that is not
		// style: `GRAM5003` said the materializing method for the statement entry point was past
		// the size at which the JIT stops optimizing, and the diagnostic's own advice is to build
		// the value in a method of your own.

		/// <summary>What a call with no statements is handed, once rather than per call.</summary>
		public static readonly SqlList<Statement> None = [];

		/// <summary>The statement the word names, for the ten that are a word and a value.</summary>
		public static Statement Commanded(string word, Ast.Expression? value)
		{
			return word switch
			{
				"PRINT" => new Print(value!),
				"RETURN" => new Return(value),
				"THROW" => new Throw(value is null ? Values.None : [value]),
				"GOTO" => new GoTo(value!),
				"BREAK" => new Break(),
				"CONTINUE" => new Continue(),
				"CHECKPOINT" => new Checkpoint(value),
				"USE" => new Use(value!),
				"RAISERROR" => new RaiseError(value is null ? Values.None : [value]),
				"WAITFOR" => new WaitFor(value!),
				_ => throw Syntax.Unknown(word),
			};
		}

		/// <summary>The same where the values arrived as a list that may not be there.</summary>
		public static Statement Commanded(string word, SqlList<Ast.Expression>? values)
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

		/// <summary>A list held under one name: a private key's settings, an Always Encrypted value's parts.</summary>
		public static Clause[] Holding(string name, SqlList<Clause>? settings)
		{
			return settings is not { } held ? [] : [new Clause.Option(name, null, held)];
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
	internal abstract record SetExpression : ISqlSpan, ISqlLocatable
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
			public sealed record DateFirst(Ast.Expression Value) : DateAndTime;

			/// <summary><c>SET DATEFORMAT dmy</c>: a constant, a word or a variable.</summary>
			public sealed record DateFormat(Ast.Expression Value) : DateAndTime;
		}

		/// <summary>The locks' settings.</summary>
		public abstract record Locking : SetExpression
		{
			/// <inheritdoc/>
			public override SetCategory Category => SetCategory.Locking;

			/// <summary><c>SET DEADLOCK_PRIORITY LOW</c>: a word, a number or a variable.</summary>
			public sealed record DeadlockPriority(Ast.Expression Value) : Locking;

			/// <summary><c>SET LOCK_TIMEOUT 1000</c>: a whole number, and nothing else.</summary>
			public sealed record LockTimeout(Ast.Expression Value) : Locking;
		}

		/// <summary>What shapes how a query runs.</summary>
		public abstract record QueryExecution : SetExpression
		{
			/// <inheritdoc/>
			public override SetCategory Category => SetCategory.QueryExecution;

			/// <summary><c>SET NOCOUNT ON</c> and the other switches of the group.</summary>
			public sealed record Switch(string Option, bool On) : QueryExecution;

			/// <summary><c>SET ROWCOUNT 10</c>: a number that is not negative, or a variable.</summary>
			public sealed record RowCount(Ast.Expression Value) : QueryExecution;

			/// <summary><c>SET TEXTSIZE 2048</c>: a whole number.</summary>
			public sealed record TextSize(Ast.Expression Value) : QueryExecution;

			/// <summary><c>SET QUERY_GOVERNOR_COST_LIMIT 10</c>: a number.</summary>
			public sealed record QueryGovernorCostLimit(Ast.Expression Value) : QueryExecution;
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
			public sealed record Language(Ast.Expression Value) : Miscellaneous;

			/// <summary><c>SET FIPS_FLAGGER 'FULL'</c>: a string, or <c>OFF</c>.</summary>
			public sealed record FipsFlagger(Ast.Expression Level) : Miscellaneous;

			/// <summary><c>SET CONTEXT_INFO 0x01</c>: a constant or a variable.</summary>
			public sealed record ContextInfo(Ast.Expression Value) : Miscellaneous;

			/// <summary><c>SET IDENTITY_INSERT t ON</c>.</summary>
			public sealed record IdentityInsert(string Table, bool On) : Miscellaneous;

			/// <summary>One keyword of <c>SET OFFSETS SELECT, FROM ON</c>.</summary>
			public sealed record Offset(string Keyword, bool On) : Miscellaneous;

			/// <summary><c>SET ERRLVL 1</c>, which no page describes and the engine reads.</summary>
			public sealed record ErrorLevel(Ast.Expression Value) : Miscellaneous;
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
	internal abstract record Query : ISqlSpan, ISqlLocatable
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
			SqlList<Ast.SelectColumn> Columns,
			Clause? Into,
			SqlList<TableReference> From,
			Ast.Expression? Where,
			Clause? GroupBy,
			Ast.Expression? Having,
			SqlList<Clause>? Windows = null) : Query;

		/// <summary>§7.3 <c>VALUES (…), (…)</c> — a table written out.</summary>
		public sealed record TableValueConstructor(SqlList<Ast.Expression> Rows) : Query;

		/// <summary>§7.13 a query in brackets, for <see cref="Ast.Expression.Parenthesized"/>'s reason.</summary>
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
		public sealed record FromFile(Ast.Expression File, SqlList<Clause>? Options = null) : Query;

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

		/// <summary>
		/// A query whose rule builds it on the common tree (<see cref="DotGram.Sql.Ast.Query"/>) already, standing where a
		/// query of this tree is held: under a value, a table source, a statement's rows. It is taken as what it holds.
		/// </summary>
		/// <remarks>
		/// What the rules that hold a query ask of it is kept with it (<see cref="Facts"/>), since the parts it was built
		/// from are not; so is what a reading of the text <c>SELECT value</c> takes apart, a specification's select list
		/// and condition, and a table value constructor's rows. None of its members is public, so that a walk of this tree
		/// takes it for a leaf.
		/// </remarks>
		internal sealed record Built : Query
		{
			Ast.Query? _made;

			/// <param name="made">The query on the common tree, or null where building it found a gap.</param>
			/// <param name="gap">What the common tree has no place for yet, which carrying the query throws.</param>
			/// <param name="record">The record the rule gathered before, under which a gap in it is said.</param>
			/// <param name="facts">What the rules that hold the query ask of it.</param>
			internal Built(Ast.Query? made, BuildGap? gap, string record, Facts facts)
			{
				_made  = made;
				Gap    = gap;
				Record = record;
				Is     = facts;
			}

			/// <summary>What the rules that hold a query ask of it, of the query and of what its brackets and operators lead to.</summary>
			[Flags]
			internal enum Facts
			{
				None = 0,

				/// <summary>A select that assigns a variable in its select list, in brackets or not (<see cref="Rules.Assigns"/>).</summary>
				Assigns = 1,

				/// <summary>A select that assigns a variable and has nothing to select from (<see cref="Rules.AssignsFromNothing"/>).</summary>
				AssignsFromNothing = 2,

				/// <summary>A query a cursor may be declared for (<see cref="Rules.Cursorable"/>).</summary>
				Cursorable = 4,

				/// <summary>A query that combines a select that assigns a variable (<see cref="Rules.AssignsCombined"/>).</summary>
				AssignsCombined = 8,
			}

			/// <summary>
			/// The query on the common tree, or null where building it found a gap; a table value constructor's is made of its
			/// rows when it is first asked for, since the rows of an <c>INSERT</c> are taken without it.
			/// </summary>
			internal Ast.Query? Made => _made ??= Values is { } rows ? new Ast.Query.Select { Values = rows } : null;

			/// <summary>What the common tree has no place for yet, which carrying the query throws.</summary>
			internal BuildGap? Gap { get; }

			/// <summary>The record of this tree the rule built before.</summary>
			internal string Record { get; }

			/// <summary>What the rules that hold the query ask of it.</summary>
			internal Facts Is { get; }

			/// <summary>
			/// A specification's select list, as the rules read it: its values as read, which building the query resolves;
			/// the query's own list where none needed resolving.
			/// </summary>
			internal SqlList<Ast.SelectColumn> Columns { get; init; }

			/// <summary>A specification's condition, as the rules read it.</summary>
			internal Ast.Expression? Where { get; init; }

			/// <summary>A table value constructor's rows, as the rules read them.</summary>
			internal SqlList<Ast.Expression> Rows { get; init; }

			/// <summary>A table value constructor's rows on the common tree, where they were built.</summary>
			internal SqlList<Ast.RowValue>? Values { get; init; }

			/// <summary>Equal where both hold equal queries, or gaps that say the same.</summary>
			public bool Equals(Built? other)
			{
				return other is not null && Equals(Made, other.Made) && Gap?.Message == other.Gap?.Message;
			}

			/// <inheritdoc/>
			public override int GetHashCode()
			{
				return (Made?.GetHashCode() ?? 0) * 31 + (Gap?.Message.GetHashCode() ?? 0);
			}
		}

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

	/// <summary>
	/// §7.6 what a <c>FROM</c> clause is read over: a table by name, a query standing where one
	/// does, and the joins between them.
	/// </summary>
	internal abstract record TableReference : ISqlSpan, ISqlLocatable
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
			Clause? Sample, SqlList<Clause> Hints, Values.Call? Server = null) : TableReference;

		/// <summary>
		/// T-SQL's <c>PIVOT</c>: a source, the aggregate to turn its rows into columns with,
		/// the column whose values name them, and which of those values to keep.
		/// </summary>
		public sealed record Pivot(
			TableReference Of, Ast.Expression Aggregate, string For, SqlList<string> In,
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
			Values.Call Function, string? Name, SqlList<string>? Columns,
			SqlList<Clause> Schema) : TableReference;

		/// <summary>§7.6 a source in brackets, for <see cref="Ast.Expression.Parenthesized"/>'s reason.</summary>
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
			Ast.Expression? On = null, SqlList<string>? Using = null, string? Hint = null) : TableReference;

		/// <summary>
		/// A source whose rule builds it on the common tree (<see cref="DotGram.Sql.Ast.TableSource"/>) already, standing
		/// where a source of this tree is held: in a <c>FROM</c>, a join, a <c>MERGE</c>'s <c>USING</c>. It is taken as
		/// what it holds.
		/// </summary>
		/// <remarks>
		/// What the rules that hold a source ask of it is kept with it (<see cref="Shape"/>, <see cref="Bracketable"/>). None
		/// of its members is public, so that a walk of this tree takes it for a leaf.
		/// </remarks>
		internal sealed record Built : TableReference
		{
			/// <param name="made">The source on the common tree, or null where building it found a gap.</param>
			/// <param name="gap">What the common tree has no place for yet, which carrying the source throws.</param>
			/// <param name="record">The record the rule gathered before, under which a gap in it is said.</param>
			/// <param name="shape">The record the rule gathered, by kind.</param>
			/// <param name="bracketable">Whether it may stand in a source's brackets (<see cref="Rules.Bracketable"/>).</param>
			internal Built(Ast.TableSource? made, BuildGap? gap, string record, Kind shape, bool bracketable)
			{
				Made        = made;
				Gap         = gap;
				Record      = record;
				Shape       = shape;
				Bracketable = bracketable;
			}

			/// <summary>The kinds of source the rules that hold one tell apart.</summary>
			internal enum Kind
			{
				Other,
				Joined,
				Parenthesized,
				OdbcJoin,
				Pivot,
				Unpivot,
			}

			/// <summary>The source on the common tree, or null where building it found a gap.</summary>
			internal Ast.TableSource? Made { get; }

			/// <summary>What the common tree has no place for yet, which carrying the source throws.</summary>
			internal BuildGap? Gap { get; }

			/// <summary>The record of this tree the rule built before.</summary>
			internal string Record { get; }

			/// <summary>The record the rule built before, by kind.</summary>
			internal Kind Shape { get; }

			/// <summary>Whether it may stand in a source's brackets.</summary>
			internal bool Bracketable { get; }

			/// <summary>Equal where both hold equal sources, or gaps that say the same.</summary>
			public bool Equals(Built? other)
			{
				return other is not null && Equals(Made, other.Made) && Gap?.Message == other.Gap?.Message;
			}

			/// <inheritdoc/>
			public override int GetHashCode()
			{
				return (Made?.GetHashCode() ?? 0) * 31 + (Gap?.Message.GetHashCode() ?? 0);
			}
		}

		/// <summary>What a clause with no sources is handed, once rather than per call.</summary>
		public static readonly SqlList<TableReference> None = [];

		/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
		public static Joined Joining(
			string? kind, string? natural, TableReference left, TableReference right,
			Ast.Expression? on, SqlList<string>? columns, string? hint = null)
		{
			return new(Syntax.Joined(kind), Syntax.Outer(kind), natural is not null, left, right, on, columns, hint);
		}
	}

	/// <summary>
	/// The pieces a statement, a query or an expression is made of that are none of the three —
	/// what the standard writes as a clause, a specification or a definition of one element.
	/// </summary>
	internal abstract record Clause : ISqlSpan, ISqlLocatable
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

		/// <summary>§10.10 one <c>ORDER BY</c> entry: what to sort by, and which way.</summary>
		/// <remarks>
		/// <see cref="Order"/> is what was written and not what it means: the standard makes
		/// ascending the default, and a statement that says so and one that does not are two
		/// different texts.
		/// </remarks>
		public sealed record SortSpecification(Ast.Expression Value, SqlOrder Order) : Clause;

		/// <summary>
		/// §10.10 the whole <c>ORDER BY</c>, with the two clauses the reference writes inside it:
		/// how many rows to step over, and how many to take.
		/// </summary>
		public sealed record OrderBy(SqlList<Clause> By, Ast.Expression? Offset, Ast.Expression? Fetch) : Clause;

		/// <summary>
		/// T-SQL's <c>TOP (n) PERCENT WITH TIES</c> — how many rows a query specification hands
		/// back, which the standard says with <see cref="OrderBy.Fetch"/> instead.
		/// </summary>
		public sealed record Top(Ast.Expression Value, bool Percent, string? With) : Clause;

		/// <summary>
		/// §7.9 <c>GROUP BY</c>: what the rows are grouped by, and the two things T-SQL writes
		/// around the list — <c>ALL</c> in front and <c>WITH CUBE</c> or <c>WITH ROLLUP</c> after.
		/// </summary>
		public sealed record GroupBy(bool All, SqlList<Ast.Expression> By, string? With) : Clause;

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
			string? Name, SqlList<Ast.Expression> PartitionBy, Clause? By, string? Frame) : Clause;

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
		public sealed record SystemTime(string Kind, SqlList<Ast.Expression> At) : Clause;

		/// <summary>
		/// T-SQL's <c>TABLESAMPLE</c>: how much of a table to read, in rows or in percent, and
		/// the seed that makes the answer the same twice.
		/// </summary>
		public sealed record TableSample(
			bool System, Ast.Expression Value, string? Unit, Ast.Expression? Repeatable) : Clause;

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
			string Name, Ast.Expression? Value, SqlList<Clause> Options, string? Partitions = null, bool Bare = false) : Clause;

		/// <summary>
		/// Where a table or an index is put: the word that says which placement, the filegroup
		/// or partition scheme, and the column a partition scheme is applied on.
		/// </summary>
		public sealed record Placement(string Kind, string Target, SqlList<string>? Columns) : Clause;

		/// <summary>§6.12 one <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
		public sealed record When(Ast.Expression Test, Ast.Expression Result) : Clause;

		/// <summary>
		/// §14.14 one entry of a <c>SET</c>: what is assigned, the operator it was assigned with
		/// where that was not a plain <c>=</c>, and the value.
		/// </summary>
		/// <param name="Operator">The assignment as written — <c>=</c>, <c>+=</c>, …; empty for <c>.WRITE (…)</c>, whose value is the call's arguments.</param>
		/// <param name="Through">The column of <c>SET @v = column op= value</c>, which assigns both.</param>
		public sealed record Set(string Target, string? Operator, Ast.Expression Value, string? Through = null) : Clause;

		/// <summary>
		/// T-SQL's <c>OUTPUT</c>: what is written out, the table it goes into where it goes
		/// anywhere, that table's columns, and a second <c>OUTPUT</c> after it where there is one.
		/// </summary>
		public sealed record Output(
			SqlList<Ast.SelectColumn> Items, TableReference? Target = null, SqlList<string>? Columns = null, Clause? Next = null) : Clause;

		/// <summary>
		/// T-SQL's <c>EXECUTE ('…') AS USER = 'u'</c>: whom a string is run as, <c>LOGIN</c> or
		/// <c>USER</c>, and the name, which is a string and nothing else.
		/// </summary>
		public sealed record ExecutionContext(string Kind, Ast.Expression Name) : Clause;

		/// <summary>
		/// §14.12 one arm of a merge: whether it fired on a match, which side the match was missing
		/// from, what else had to be true, and what to do.
		/// </summary>
		public sealed record MergeWhen(
			bool OnMatch, string? By, Ast.Expression? Condition, Statement Action) : Clause;

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
			string Name, string? Type, Ast.Expression? Value, SqlList<Clause>? Elements = null, string? Nullability = null,
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
		public sealed record PartitionRange(Ast.Expression From, Ast.Expression? To = null) : Clause;

		/// <summary>
		/// One parameter of a routine: its name, its type, and its default — and the words around
		/// them, <c>VARYING</c> and the nullability before the default, <c>OUTPUT</c> and
		/// <c>READONLY</c> after it.
		/// </summary>
		public sealed record ParameterDeclaration(
			string Name, string? Type, Ast.Expression? Value,
			string? Nullability = null, bool Varying = false, SqlList<string>? Ways = null) : Clause;

		/// <summary>
		/// §11.4 one column: its name, the type as written, the expression where it is computed
		/// rather than stored, and everything said about it after that — its options, its
		/// constraints, and the index written on it — in the order it was said.
		/// </summary>
		public sealed record ColumnDefinition(
			string Name, string? Type, Ast.Expression? Computed, SqlList<Clause> Options) : Clause;

		/// <summary>
		/// One thing said about a column after its type that is not a constraint: which thing
		/// (<c>SPARSE</c>, <c>NOT NULL</c>, <c>COLLATE</c>, <c>IDENTITY</c>, <c>MASKED</c>, …), what
		/// it was given in brackets or after it, the options where it carries a <c>WITH</c>, and
		/// the constraint name where the language lets a nullability have one.
		/// </summary>
		public sealed record ColumnOption(
			string Kind, SqlList<Ast.Expression> Arguments, SqlList<Clause> Options, string? ConstraintName) : Clause;

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
			string? Name, string Kind, SqlList<Clause> Columns, Ast.Expression? Check,
			string? Clustering = null, bool Hash = false, bool Columnstore = false,
			SqlList<string>? Order = null, SqlList<string>? Include = null, Ast.Expression? Filter = null,
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
			string kind, SqlList<Clause>? columns, Ast.Expression? check)
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
		public static ConstraintDefinition Defaulted(Ast.Expression value, string? tail)
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
			SqlList<Clause>? columns, SqlList<string>? order, SqlList<string>? include, Ast.Expression? filter,
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
		public static ColumnOption Optioned(string kind, Ast.Expression? argument = null)
		{
			return new(Syntax.Squared(kind), argument is null ? Values.None : [argument], None, null);
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
		public static Top Topped(Ast.Expression value, string? suffix)
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
}
