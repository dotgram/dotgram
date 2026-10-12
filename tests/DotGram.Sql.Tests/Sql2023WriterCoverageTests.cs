using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the OLD tree's, since `DotGram.Sql` encloses
// `DotGram.Sql.Tests`. Both trees have an `Expression`, a `DataType` and a `Statement`, so the names
// are bound here as Both.cs binds them. This is the hazard SKILL.md names as "two trees with the
// same names", met from inside the repository rather than by a consumer.
using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// That the writer has text for every value of every enum it prints, and that every flag it is given
/// changes what it prints.
/// </summary>
/// <remarks>
/// <para>
/// Written because it was not there. <c>Sql2023Writer</c> ends seventeen switches in a catch-all that
/// returns a literal and throws nowhere, so a value added to one of those enums printed as the last
/// arm: <c>ComparisonOperator.NotLess</c> and <c>NotGreater</c>, added on 2026-09-26, both wrote as
/// <c>&gt;=</c>. Valid SQL with another meaning, which a round trip accepts as readily as the right
/// text, and nothing failed because nothing built those values yet.
/// </para>
/// <para>
/// So: every value of a covered enum writes something, and no two values of one enum write the same
/// text unless this file says which two and why. A flag is the same defect wearing different clothes
/// — an arm nothing reaches and a flag nothing reads are both a fact the tree holds and the text
/// loses — so each one is asserted to change the output.
/// </para>
/// </remarks>
public sealed class Sql2023WriterCoverageTests
{
	static readonly Expression A = new Expression.Reference(new QualifiedName([new Identifier("a")]));
	static readonly Expression B = new Expression.Reference(new QualifiedName([new Identifier("b")]));

	static readonly RoutineDesignator Routine =
		new(RoutineKind.Procedure, new QualifiedName([new Identifier("p")]));

	static readonly TransactionName Saved = new(new Identifier("s"));

	static readonly QualifiedName T = new([new Identifier("t")]);
	static readonly QualifiedName U = new([new Identifier("u")]);

	/// <summary><c>SELECT a FROM</c> the source given.</summary>
	static Query.Select Joined(TableSource source)
	{
		return Selected with { From = new FromClause([source]) };
	}

	/// <summary><c>SELECT a FROM t TABLESAMPLE …</c>, with SYSTEM written or left out, and the unit given.</summary>
	static Query.Select Sampled(bool omitted, SampleUnit? unit = null)
	{
		return Joined(new TableSource.Named(T)
		{
			Sample = new SampleClause(SampleMethod.System, A) { MethodOmitted = omitted, Unit = unit },
		});
	}

	/// <summary><c>SELECT a WHERE MATCH (…)</c> over the patterns given.</summary>
	static Query.Select Matched(params GraphPattern[] patterns)
	{
		return Selected with { Where = new Expression.GraphMatch([.. patterns]) };
	}

	static GraphNode Node(string alias, bool last = false)
	{
		return new GraphNode(new Identifier(alias), last);
	}

	/// <summary><c>SHORTEST_PATH (a(-(e)->b)+)</c>, or with the anchor last, and the quantifier and step node given.</summary>
	static GraphPattern.ShortestPath Shortest(GraphQuantifier quantifier, bool anchorLast = false, GraphNode? step = null)
	{
		return new GraphPattern.ShortestPath(Node("a"), [new GraphStep(new GraphEdge(new Identifier("e"), GraphDirection.Right), step ?? Node("b"))], quantifier, anchorLast);
	}

	/// <summary><c>a = 1</c>, an assignment of SET.</summary>
	static Assignment Assigned()
	{
		return new Assignment([new AssignmentTarget(new QualifiedName([new Identifier("a")]))], B);
	}

	/// <summary><c>MERGE INTO t USING u ON a WHEN MATCHED THEN DELETE</c>.</summary>
	static Statement.Merge Merged()
	{
		return new Statement.Merge
		{
			Target      = new TableSource.Named(T),
			SourceTable = new TableSource.Named(U),
			On          = A,
			Clauses     = [new MergeClause.Matched(new MergeMatchedAction.Delete())],
		};
	}

	/// <summary><c>c</c>, a full-text rowset's column.</summary>
	static Expression.Reference Searched(string name = "c")
	{
		return new Expression.Reference(new QualifiedName([new Identifier(name)]));
	}

	/// <summary><c>SELECT a</c>, the smallest query there is, for a clause to be put on.</summary>
	static readonly Query.Select Selected = new() { Columns = [new SelectColumn.ExpressionItem(A)] };

	/// <summary><c>SELECT a op SELECT b</c>.</summary>
	static Query.Select Combined(SetOperator op, bool distinct = false)
	{
		return Selected with
		{
			SetOperations = [new SetOperation(op, new Query.Select { Columns = [new SelectColumn.ExpressionItem(B)] }) { Distinct = distinct }],
		};
	}

	/// <summary>A revoked SELECT on one table, written with the page's FROM or its TO.</summary>
	static RevokeBody Revoked(bool to)
	{
		return new RevokeBody.Privileges(
			null,
			[new Privilege(PrivilegeKind.Select, [], [])],
			new PrivilegeObject.Named(PrivilegeObjectKind.Table, new QualifiedName([new Identifier("t")])),
			[new Grantee.Public()],
			null,
			null,
			To: to);
	}

	/// <summary>
	/// A GRANT of the privileges given, on the securable given or on none, to PUBLIC.
	/// </summary>
	static Statement.Grant Granted(PrivilegeObject? on, params Privilege[] privileges)
	{
		return new Statement.Grant
		{
			Body = new GrantBody.Privileges([.. privileges], on, [new Grantee.Public()], false, false, null),
		};
	}

	/// <summary>
	/// A securable of one name, written with T-SQL's class or without one.
	/// </summary>
	static PrivilegeObject Securable(SecurableClass? of)
	{
		var name = new QualifiedName([new Identifier("s")]);

		return of is { } written
			? new PrivilegeObject.Classed(written, name)
			: new PrivilegeObject.Named(PrivilegeObjectKind.Table, name);
	}

	/// <summary>A cursor's properties with NO SCROLL, written as that or as T-SQL's FORWARD_ONLY.</summary>
	static CursorProperties Scrolling(bool forwardOnly)
	{
		return new CursorProperties(null, CursorScrollability.NoScroll, null, null) { ForwardOnly = forwardOnly };
	}

	/// <summary>A cursor declared over the smallest query there is, for a flag to be read out of.</summary>
	static Statement.DeclareCursor Declared(
		CursorProperties properties, bool updatabilityFirst = false, bool underscored = false)
	{
		return new Statement.DeclareCursor
		{
			Cursor      = new CursorReference(new QualifiedName([new Identifier("c")])),
			Properties  = properties,
			SourceValue = new CursorSource.Query(
				new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(A)] } },
				new UpdatabilityClause(true, [], underscored),
				updatabilityFirst),
		};
	}

	/// <summary>The enums this file covers, each with the smallest node that puts a value in the text.</summary>
	static readonly Dictionary<Type, Func<object, ISqlNode>> Covered = new()
	{
		[typeof(ComparisonOperator)] = v => new Expression.Comparison(A, (ComparisonOperator)v, B),
		[typeof(BinaryOperator)]     = v => new Expression.Binary(A, (BinaryOperator)v, B),
		[typeof(UnaryOperator)]      = v => new Expression.Unary((UnaryOperator)v, A),
		[typeof(LikeKind)]           = v => new Expression.Like(A, false, (LikeKind)v, B),
		[typeof(TriggerScope)]       = v => new Statement.DropTrigger { Names = [T], On = (TriggerScope)v },
		[typeof(NumericLiteralKind)] = v => new Expression.Literal(new LiteralValue.Numeric("1", (NumericLiteralKind)v)),
		[typeof(DateTimeLiteralKind)] = v => new Expression.Literal(new LiteralValue.DateTime((DateTimeLiteralKind)v, "2026-09-26")),
		[typeof(NumericTypeKind)]    = v => new DataType.Numeric((NumericTypeKind)v),
		[typeof(DateTimeTypeKind)]   = v => new DataType.DateTime((DateTimeTypeKind)v),
		[typeof(CharacterTypeKind)]  = v => new DataType.Character((CharacterTypeKind)v),
		[typeof(BinaryTypeKind)]     = v => new DataType.Binary((BinaryTypeKind)v),
		[typeof(IdentifierStyle)]    = v => new Identifier("x", (IdentifierStyle)v),
		[typeof(TopWith)]            = v => Selected with { Top = new TopClause(A, true, false, (TopWith)v) },
		[typeof(TransactionKeyword)] = v => new Statement.Commit { Keyword = (TransactionKeyword)v },
		[typeof(SetOperator)]        = v => Combined((SetOperator)v),
		[typeof(LockingMode)]        = v => Selected with { Locking = [new LockingClause((LockingMode)v, [])] },
		[typeof(RowWord)]            = v => Selected with { Limit = new Limit { Offset = A, OffsetRowWord = (RowWord)v, Count = B, FetchRowWord = (RowWord)v } },
		[typeof(FetchPosition)]      = v => Selected with { Limit = new Limit { Count = A, FetchRowWord = RowWord.Rows, Position = (FetchPosition)v } },
		// A hundred and seventy permissions and twenty-eight classes, generated from the catalogue (D148):
		// this is what says each one has words of its own, where PermissionCatalogueTests says which.
		[typeof(PrivilegeKind)]      = v => Granted(null, new Privilege((PrivilegeKind)v, [], [])),
		[typeof(SecurableClass)]     = v => Granted(Securable((SecurableClass)v), new Privilege(PrivilegeKind.Control, [], [])),
		[typeof(XmlTypeModifier)]    = v => new DataType.Xml((XmlTypeModifier)v, T),
		[typeof(DatePartKind)]       = v => new Expression.DatePart((DatePartKind)v),
		// Year is the kind with all three words, so it is the one every spelling is asked of.
		[typeof(DatePartSpelling)]   = v => new Expression.DatePart(DatePartKind.Year, (DatePartSpelling)v),
		[typeof(JoinKind)]           = v => Joined(new TableSource.Join { Left = new TableSource.Named(T), Right = new TableSource.Named(U), Kind = (JoinKind)v }),
		[typeof(JoinHint)]           = v => Joined(new TableSource.Join { Left = new TableSource.Named(T), Right = new TableSource.Named(U), Kind = JoinKind.Inner, Hint = (JoinHint)v }),
		[typeof(GroupByWith)]        = v => Selected with { GroupBy = new GroupByClause(null, [new GroupingElement.Ordinary([A])]) { With = (GroupByWith)v } },
		[typeof(SampleUnit)]         = v => Sampled(false, (SampleUnit)v),
		[typeof(GraphDirection)]     = v => Matched(new GraphPattern.Chain(Node("a"), [new GraphStep(new GraphEdge(new Identifier("e"), (GraphDirection)v), Node("b"))])),
		[typeof(AssignmentOperator)] = v => new Query.Select { Columns = [new SelectColumn.VariableAssignment(new Identifier("@v"), (AssignmentOperator)v, A)] },
		[typeof(IntoKeyword)]        = v => new Statement.Insert { Target = new TableSource.Named(T), Into = (IntoKeyword)v },
		[typeof(HintArgumentForm)]   = v => Joined(new TableSource.Named(T) { Hints = [new TableHint.Named(new Identifier("INDEX"), [A]) { Form = (HintArgumentForm)v }] }),
		[typeof(QueryHintPhrase)]    = v => new Statement.Select { Query = Selected, Hints = [new QueryHint.Phrase((QueryHintPhrase)v)] },
		[typeof(XmlMode)]            = v => Selected with { For = new ForClause.Xml((XmlMode)v, null, []) },
		[typeof(JsonMode)]           = v => Selected with { For = new ForClause.Json((JsonMode)v, []) },
		[typeof(ForDirectiveKind)]   = v => Selected with
		{
			For = (ForDirectiveKind)v is ForDirectiveKind.IncludeNullValues or ForDirectiveKind.WithoutArrayWrapper
				? new ForClause.Json(JsonMode.Auto, [new ForDirective((ForDirectiveKind)v)])
				: new ForClause.Xml(XmlMode.Auto, null, [new ForDirective((ForDirectiveKind)v)]),
		},
	};

	/// <summary>
	/// Enums whose values do not decide the text, and why. Distinctness is not asked of them.
	/// </summary>
	/// <remarks>
	/// A kind that the record's own text already says is not a fact the writer has to print: a
	/// <c>LiteralValue.Numeric</c> carries <c>"0xFF"</c> and the writer prints that, so
	/// <c>HexInteger</c> and <c>Decimal</c> write the same thing for the same text and should. Which
	/// also says something useful about the amendments: <c>NumericLiteralKind.Money</c> needs no arm in
	/// the writer at all, where <c>BinaryOperator.Modulo</c> needed one.
	/// </remarks>
	static readonly Dictionary<string, string> TextDecidesInstead = new(StringComparer.Ordinal)
	{
		["NumericLiteralKind"] = "LiteralValue.Numeric carries the literal's text and the kind is derivable from it",
		["IdentifierStyle"] = "the grammar captures a delimited name WITH its delimiters -- `DelimitedIdentifier = '\"' & ... & '\"'` -- so Identifier.Text holds them and the style is derivable from the first character, which is why PutIdentifier prints the text raw and is right to",
	};

	[Fact]
	public void Every_value_of_a_covered_enum_writes_text()
	{
		var silent = new List<string>();

		foreach (var (type, build) in Covered)
			foreach (var value in Enum.GetValues(type))
				if (string.IsNullOrWhiteSpace(Write(build, value)))
					silent.Add(type.Name + "." + value);

		Assert.Empty(silent);
	}

	[Fact]
	public void No_two_values_of_one_enum_write_the_same_text()
	{
		var shared = new List<string>();

		foreach (var (type, build) in Covered)
		{
			if (TextDecidesInstead.ContainsKey(type.Name))
				continue;

			var seen = new Dictionary<string, string>(StringComparer.Ordinal);

			foreach (var value in Enum.GetValues(type))
			{
				var text = Write(build, value) ?? "";
				var name = type.Name + "." + value;

				if (seen.TryGetValue(text, out var first))
					shared.Add(name + " writes what " + first + " writes: " + text);
				else
					seen[text] = name;
			}
		}

		Assert.Empty(shared);
	}

	/// <summary>A supplement, not the gate: that no switch in the writer ends in a catch-all.</summary>
	/// <remarks>
	/// <b>This is not the check that protects the writer.</b> It reads the source for <c>_ =&gt; "..."</c>,
	/// and it passes on <c>Word(op == UnaryOperator.Minus ? "-" : "+")</c>, which wrote T-SQL's <c>~</c>
	/// as <c>+</c> — the same defect with no catch-all in it. The gate is
	/// <see cref="No_two_values_of_one_enum_write_the_same_text"/>, which walks values through the writer
	/// and reads what comes out. This one only keeps a catch-all from being reintroduced, which the
	/// output check cannot see until an enum gains a value.
	/// </remarks>
	[Fact]
	public void No_switch_in_the_writer_ends_in_a_catch_all()
	{
		// EVERY file of the writer, not the one named after it. `Sql2023Writer` is three partial files,
		// and reading only the first said "no catch-alls left" while 25 stood in the other two.
		var written = Directory.GetFiles(
			Path.Combine(Repository(), "src", "DotGram.Sql", "Standard"), "Sql2023Writer*.cs");

		Assert.True(written.Length >= 3, "the writer is in " + written.Length + " files; this check expects all of them");

		var lines = written
			.SelectMany(one => File.ReadAllText(one)
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Split('\n')
				.Select((text, at) => Path.GetFileName(one) + "(" + (at + 1) + "): " + text))
			.ToList();

		var loose = new List<string>();

		// Padded to align with the arms above it, so the spelling is matched loosely: a detector that
		// only knew `_ => "` counted 17 of these where there were 31.
		foreach (var line in lines)
			if (System.Text.RegularExpressions.Regex.IsMatch(line, "_ +=> +\"|_ => \""))
				loose.Add(line.Trim());

		Assert.Empty(loose);
	}

	static string Repository([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
	}

	/// <summary>That each named exception is true of the writer, and not merely asserted by this file.</summary>
	/// <remarks>
	/// An exception says the enum does not decide the text because the record carries it. That claim is
	/// testable: change the text and the output must change. Without this the list could excuse a real
	/// defect by describing one, which is how a skip with a reason becomes a skip.
	/// </remarks>
	[Fact]
	public void Where_the_text_decides_it_decides()
	{
		var claims = new Dictionary<string, (ISqlNode One, ISqlNode Two)>(StringComparer.Ordinal)
		{
			["NumericLiteralKind"] = (
				new Expression.Literal(new LiteralValue.Numeric("1", NumericLiteralKind.Decimal)),
				new Expression.Literal(new LiteralValue.Numeric("0xFF", NumericLiteralKind.Decimal))),
			["IdentifierStyle"] = (
				new Identifier("a", IdentifierStyle.Delimited),
				new Identifier("\"my column\"", IdentifierStyle.Delimited)),
		};

		// Every exception is asserted and nothing else is: a new one cannot be added without its claim.
		Assert.Equal(
			TextDecidesInstead.Keys.OrderBy(one => one, StringComparer.Ordinal),
			claims.Keys.OrderBy(one => one, StringComparer.Ordinal));

		foreach (var (name, pair) in claims)
			Assert.NotEqual(Sql2023Writer.Write(pair.One), Sql2023Writer.Write(pair.Two));

		// And the delimiters the style is derived from are in the text, which is why they are written.
		Assert.Contains("\"", Sql2023Writer.Write(claims["IdentifierStyle"].Two), StringComparison.Ordinal);
	}

	/// <summary>
	/// That every spelling the tree keeps reaches the text, and that a flag joins this check by being a
	/// flag rather than by anyone remembering to add a row.
	/// </summary>
	/// <remarks>
	/// The pairs are found by reflection over the properties marked <c>[Spelling]</c>, and the test
	/// refuses to pass while a marked property has no pair here — so adding a flag and forgetting it is a
	/// failure, not a gap. What is asserted of each is total: either the writer does not take the node at
	/// all yet, and both spellings raise its <c>NotSupportedException</c>, or it takes it and the two must
	/// differ. The one case that fails is the one that matters — written, and the spelling lost.
	/// </remarks>
	[Fact]
	public void Every_spelling_flag_reaches_the_text()
	{
		var pairs = new Dictionary<string, (ISqlNode Off, ISqlNode On)>(StringComparer.Ordinal)
		{
			["Comparison.Exclamation"] = (
				new Expression.Comparison(A, ComparisonOperator.NotEqual, B),
				new Expression.Comparison(A, ComparisonOperator.NotEqual, B, Exclamation: true)),
			["Character.Max"] = (
				new DataType.Character(CharacterTypeKind.Varchar),
				new DataType.Character(CharacterTypeKind.Varchar, Max: true)),
			["Binary.Max"] = (
				new DataType.Binary(BinaryTypeKind.Varbinary),
				new DataType.Binary(BinaryTypeKind.Varbinary, Max: true)),
			["Updatability.Underscored"] = (
				new FileGroupChange.Updatability(FileGroupUpdatability.ReadOnly, false),
				new FileGroupChange.Updatability(FileGroupUpdatability.ReadOnly, true)),
			["DropRoutine.Proc"] = (
				new Statement.DropRoutine { Routines = [Routine] },
				new Statement.DropRoutine { Routines = [Routine], Proc = true }),
			["SetTransaction.Tran"] = (
				new Statement.SetTransaction(),
				new Statement.SetTransaction { Tran = true }),
			["BeginTransaction.Tran"] = (
				new Statement.BeginTransaction(),
				new Statement.BeginTransaction(Tran: true)),
			["SaveTransaction.Tran"] = (
				new Statement.SaveTransaction(Saved),
				new Statement.SaveTransaction(Saved, Tran: true)),
			// The three cursor flags are asserted on the statement that writes them and not on the node
			// that holds them: a spelling only reaches the text through DECLARE CURSOR, so a pair of bare
			// CursorProperties would be two nodes the writer does not take, which this test passes and
			// which proves nothing about the flag.
			["Privileges.To"] = (
				new Statement.Revoke { Body = Revoked(false) },
				new Statement.Revoke { Body = Revoked(true) }),
			["Privilege.Abbreviated"] = (
				Granted(null, new Privilege(PrivilegeKind.AllPrivileges, [], [])),
				Granted(null, new Privilege(PrivilegeKind.AllPrivileges, [], [], Abbreviated: true))),
			["DeclareCursor.Extended"] = (
				Declared(Scrolling(false)),
				Declared(Scrolling(false)) with { Extended = true }),
			["CursorProperties.ForwardOnly"] = (
				Declared(Scrolling(false)),
				Declared(Scrolling(true))),
			["Query.UpdatabilityBeforeOptions"] = (
				Declared(new CursorProperties(null, null, null, null), false),
				Declared(new CursorProperties(null, null, null, null), true)),
			["Select.Table"] = (
				new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.Named(T)]) },
				new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.Named(T)]), Table = true }),
			["SetOperation.Distinct"] = (
				Combined(SetOperator.Union),
				Combined(SetOperator.Union, distinct: true)),
			// T-SQL's `0x0A` is the digits after its introducer; the standard's is X and the quoted digits.
			["Binary.ZeroX"] = (
				new Expression.Literal(new LiteralValue.Binary("'0A'")),
				new Expression.Literal(new LiteralValue.Binary("0A") { ZeroX = true })),
			// T-SQL's type named as an identifier, the same type as its key word.
			["DataType.WrittenName"] = (
				new DataType.Numeric(NumericTypeKind.Int),
				new DataType.Numeric(NumericTypeKind.Int) { WrittenName = new QualifiedName([new Identifier("[int]", IdentifierStyle.Bracketed)]) }),
			// T-SQL's escape in ODBC's braces.
			["Like.BracedEscape"] = (
				new Expression.Like(A, false, LikeKind.Like, B, A),
				new Expression.Like(A, false, LikeKind.Like, B, A) { BracedEscape = true }),
			// T-SQL's TABLESAMPLE may leave its only method out.
			["SampleClause.MethodOmitted"] = (
				Sampled(false),
				Sampled(true)),
			// T-SQL's full-text rowsets read one column in brackets or without them.
			["ContainsTable.ColumnsBracketed"] = (
				Joined(new TableSource.ContainsTable(T, [Searched()], A, null, null)),
				Joined(new TableSource.ContainsTable(T, [Searched()], A, null, null) { ColumnsBracketed = true })),
			["FreetextTable.ColumnsBracketed"] = (
				Joined(new TableSource.FreetextTable(T, [new Expression.Asterisk()], A, null, null)),
				Joined(new TableSource.FreetextTable(T, [new Expression.Asterisk()], A, null, null) { ColumnsBracketed = true })),
			// T-SQL's DELETE may leave its FROM out, and MERGE its INTO.
			["Delete.FromOmitted"] = (
				new Statement.Delete { Target = new TableSource.Named(T) },
				new Statement.Delete { Target = new TableSource.Named(T), FromOmitted = true }),
			["NotMatched.ByTarget"] = (
				Merged() with { Clauses = [new MergeClause.NotMatched(new MergeInsertAction([], null, [A]))] },
				Merged() with { Clauses = [new MergeClause.NotMatched(new MergeInsertAction([], null, [A])) { ByTarget = true }] }),
			["Merge.IntoOmitted"] = (
				Merged(),
				Merged() with { IntoOmitted = true }),
			// T-SQL's + after SHORTEST_PATH's steps is {1,} written short.
			["GraphQuantifier.Plus"] = (
				Matched(Shortest(new GraphQuantifier(1, null, false))),
				Matched(Shortest(new GraphQuantifier(1, null, true)))),
			// T-SQL's `x = a`, the alias first.
			["ExpressionItem.AliasFirst"] = (
				Selected with { Columns = [new SelectColumn.ExpressionItem(A, new Identifier("x"))] },
				Selected with { Columns = [new SelectColumn.ExpressionItem(A, new Identifier("x")) { AliasFirst = true }] }),
			// T-SQL's ELEMENTS ABSENT, which means what ELEMENTS alone means.
			["ForDirective.Absent"] = (
				Selected with { For = new ForClause.Xml(XmlMode.Auto, null, [new ForDirective(ForDirectiveKind.Elements)]) },
				Selected with { For = new ForClause.Xml(XmlMode.Auto, null, [new ForDirective(ForDirectiveKind.Elements, Absent: true)]) }),
			["UpdatabilityClause.Underscored"] = (
				Declared(new CursorProperties(null, null, null, null), false, false),
				Declared(new CursorProperties(null, null, null, null), false, true)),
		};

		var marked = typeof(Statement).Assembly.GetTypes()
			.SelectMany(one => one.GetProperties())
			.Where(one => one.GetCustomAttributesData()
				.Any(attribute => attribute.AttributeType.Name == "SpellingAttribute"))
			.ToList();

		// A marked property is either a flag, which has two spellings and is asserted as a pair here,
		// or an enum, whose every value is a spelling -- so it belongs to the distinctness check over
		// Covered instead, and demanding a pair of it would ask the wrong question. What both roads
		// share is that a marked property nobody asserted fails: the enum has to be COVERED, not merely
		// mentioned.
		// A flag, or a spelling held as text — a type's name written as an identifier — is asserted as a pair.
		static bool Paired(System.Reflection.PropertyInfo one)
		{
			return one.PropertyType == typeof(bool) || !(Nullable.GetUnderlyingType(one.PropertyType) ?? one.PropertyType).IsEnum;
		}

		var flags = marked.Where(Paired)
			.Select(one => one.DeclaringType!.Name + "." + one.Name)
			.Distinct(StringComparer.Ordinal)
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToList();

		Assert.Equal(flags, pairs.Keys.OrderBy(one => one, StringComparer.Ordinal));

		foreach (var one in marked.Where(one => !Paired(one)))
		{
			// An optional enum in this tree is nullable, so what is covered is the enum inside it.
			var named = Nullable.GetUnderlyingType(one.PropertyType) ?? one.PropertyType;

			Assert.True(
				named.IsEnum && Covered.ContainsKey(named),
				one.DeclaringType!.Name + "." + one.Name + " is marked [Spelling] and is a "
					+ named.Name + ", so add it to Covered, where every value of it is asked for its own text.");
		}

		foreach (var (name, pair) in pairs)
		{
			var off = Written(pair.Off);
			var on  = Written(pair.On);

			Assert.True(
				(off is null && on is null) || off != on,
				name + ": the writer takes the node and writes the same text either way, so the spelling is lost");
		}
	}

	/// <summary>What the writer makes of a node, or null where it does not take the node at all.</summary>
	static string? Written(ISqlNode node)
	{
		try
		{
			return Sql2023Writer.Write(node);
		}
		catch (NotSupportedException)
		{
			return null;
		}
	}

	[Theory]
	[InlineData("Max on a character type")]
	[InlineData("Max on a binary type")]
	[InlineData("Exclamation on a comparison")]
	[InlineData("Try on a cast")]
	[InlineData("Try on a convert")]
	[InlineData("Try on a parse")]
	[InlineData("DefaultValues on a MERGE insert")]
	[InlineData("NotMatchedBySource against Matched")]
	[InlineData("ForceSeek on CHANGETABLE CHANGES")]
	[InlineData("ForceSeek on CHANGETABLE VERSION")]
	[InlineData("ForPath on a table")]
	[InlineData("ForPath on a subquery")]
	[InlineData("AnchorLast on a shortest path")]
	[InlineData("LastNode on a graph node")]
	[InlineData("Over on a next value")]
	[InlineData("GraphPath on a call")]
	public void A_flag_changes_what_is_written(string which)
	{
		var type = new DataType.Numeric(NumericTypeKind.Int);
		var s    = new QualifiedName([new Identifier("s")]);

		var (without, with) = which switch
		{
			"Try on a cast" => (
				Sql2023Writer.Write(new Expression.Cast(A, type)),
				Sql2023Writer.Write(new Expression.Cast(A, type) { Try = true })),
			"Try on a convert" => (
				Sql2023Writer.Write(new Expression.Convert(type, A)),
				Sql2023Writer.Write(new Expression.Convert(type, A, Try: true))),
			"Try on a parse" => (
				Sql2023Writer.Write(new Expression.Parse(A, type)),
				Sql2023Writer.Write(new Expression.Parse(A, type, Try: true))),
			"DefaultValues on a MERGE insert" => (
				Sql2023Writer.Write(Merged() with { Clauses = [new MergeClause.NotMatched(new MergeInsertAction([], null, []))] }),
				Sql2023Writer.Write(Merged() with { Clauses = [new MergeClause.NotMatched(new MergeInsertAction([], null, []) { DefaultValues = true })] })),
			"NotMatchedBySource against Matched" => (
				Sql2023Writer.Write(Merged()),
				Sql2023Writer.Write(Merged() with { Clauses = [new MergeClause.NotMatchedBySource(new MergeMatchedAction.Delete())] })),
			"ForceSeek on CHANGETABLE CHANGES" => (
				Sql2023Writer.Write(Joined(new TableSource.ChangeTableChanges(T, A, false))),
				Sql2023Writer.Write(Joined(new TableSource.ChangeTableChanges(T, A, true)))),
			"ForceSeek on CHANGETABLE VERSION" => (
				Sql2023Writer.Write(Joined(new TableSource.ChangeTableVersion(T, [new Identifier("k")], [A], false))),
				Sql2023Writer.Write(Joined(new TableSource.ChangeTableVersion(T, [new Identifier("k")], [A], true)))),
			"ForPath on a table" => (
				Sql2023Writer.Write(Joined(new TableSource.Named(T))),
				Sql2023Writer.Write(Joined(new TableSource.Named(T) { ForPath = true }))),
			"ForPath on a subquery" => (
				Sql2023Writer.Write(Joined(new TableSource.Subquery(Selected))),
				Sql2023Writer.Write(Joined(new TableSource.Subquery(Selected) { ForPath = true }))),
			"AnchorLast on a shortest path" => (
				Sql2023Writer.Write(Matched(Shortest(new GraphQuantifier(1, 3, false)))),
				Sql2023Writer.Write(Matched(Shortest(new GraphQuantifier(1, 3, false), anchorLast: true)))),
			"LastNode on a graph node" => (
				Sql2023Writer.Write(Matched(new GraphPattern.Chain(Node("a"), [new GraphStep(new GraphEdge(new Identifier("e"), GraphDirection.Right), Node("b"))]))),
				Sql2023Writer.Write(Matched(new GraphPattern.Chain(Node("a", true), [new GraphStep(new GraphEdge(new Identifier("e"), GraphDirection.Right), Node("b"))])))),
			"GraphPath on a call" => (
				Sql2023Writer.Write(new Expression.Invocation(s, [new Argument(A)])),
				Sql2023Writer.Write(new Expression.Invocation(s, [new Argument(A)]) { GraphPath = true })),
			"Over on a next value" => (
				Sql2023Writer.Write(new Expression.NextValue(s)),
				Sql2023Writer.Write(new Expression.NextValue(s) { Over = new WindowSpecification(null, [], new OrderByClause([new SortItem(A)]), null) })),
			"Max on a character type" => (
				Sql2023Writer.Write(new DataType.Character(CharacterTypeKind.Varchar)),
				Sql2023Writer.Write(new DataType.Character(CharacterTypeKind.Varchar, Max: true))),
			"Max on a binary type" => (
				Sql2023Writer.Write(new DataType.Binary(BinaryTypeKind.Varbinary)),
				Sql2023Writer.Write(new DataType.Binary(BinaryTypeKind.Varbinary, Max: true))),
			_ => (
				Sql2023Writer.Write(new Expression.Comparison(A, ComparisonOperator.NotEqual, B)),
				Sql2023Writer.Write(new Expression.Comparison(A, ComparisonOperator.NotEqual, B, Exclamation: true))),
		};

		Assert.NotEqual(without, with);
	}

	/// <summary>
	/// That a node holding two constructs where at most one may be written is refused, and that each of
	/// the two on its own is written.
	/// </summary>
	/// <remarks>
	/// <c>Rollback</c> has T-SQL's <c>Name</c> — a transaction or a savepoint, which its page does not
	/// distinguish — and the standard's <c>ToSavepoint</c>, which prints <c>TO SAVEPOINT s</c>. Two
	/// nullable properties of which at most one is set is a shape worth objecting to, and it earns its
	/// place only because something checks it: whatever the writer printed for a node with both would
	/// lose one of them, so it prints nothing. What the grammars may set is the grammars' own assertion,
	/// and belongs with the commit that teaches them to build these nodes.
	/// </remarks>
	[Fact]
	public void A_rollback_naming_a_transaction_and_a_savepoint_is_refused()
	{
		var named = new Statement.Rollback { Keyword = TransactionKeyword.Transaction, Name = Saved };
		var saved = new Statement.Rollback { ToSavepoint = new Identifier("s") };

		Assert.Contains("ROLLBACK TRANSACTION s", Sql2023Writer.Write(named), StringComparison.Ordinal);
		Assert.Contains("TO SAVEPOINT s", Sql2023Writer.Write(saved), StringComparison.Ordinal);

		var both = named with { ToSavepoint = new Identifier("s") };

		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(both));
	}

	/// <summary>
	/// That the three parts T-SQL's permission statements added reach the text: DENY, which the standard
	/// has no word for, a securable left out, and the column list on the securable.
	/// </summary>
	/// <remarks>
	/// No grammar builds these yet — T-SQL still reads its permissions into the old tree — so without a
	/// test here the writer's DENY arm is code nothing reaches, which is the state the enum arms were in
	/// when <c>NotLess</c> printed as <c>&gt;=</c>.
	/// </remarks>
	[Fact]
	public void The_permission_statements_write_what_T_SQL_adds()
	{
		var select = new Privilege(PrivilegeKind.Select, [], []);
		var table  = new QualifiedName([new Identifier("t")]);

		Assert.Equal(
			"DENY SELECT ON t TO PUBLIC CASCADE AS keeper",
			Sql2023Writer.Write(new Statement.Deny(
				[select],
				[new Grantee.Public()],
				new PrivilegeObject.Named(PrivilegeObjectKind.Table, table),
				Cascade: true,
				AsPrincipal: new Identifier("keeper"))).TrimEnd(';', ' '));

		// A server permission names no securable, so there is no ON at all.
		Assert.Equal(
			"GRANT SELECT TO PUBLIC",
			Sql2023Writer.Write(new Statement.Grant
			{
				Body = new GrantBody.Privileges([select], null, [new Grantee.Public()], false, false, null),
			}).TrimEnd(';', ' '));

		// And the column list that follows the SECURABLE rather than the permission.
		Assert.Contains(
			"ON t (a, b)",
			Sql2023Writer.Write(new Statement.Grant
			{
				Body = new GrantBody.Privileges(
					[select],
					new PrivilegeObject.Named(PrivilegeObjectKind.Table, table, Columns: [new Identifier("a"), new Identifier("b")]),
					[new Grantee.Public()], false, false, null),
			}),
			StringComparison.Ordinal);

		// T-SQL's permissions, its two shorter spellings, a securable's class, and NULL among the grantees.
		Assert.Equal(
			"GRANT ALL, EXEC, EXECUTE, VIEW DEFINITION (c) ON SCHEMA::s TO u, NULL WITH GRANT OPTION",
			Sql2023Writer.Write(new Statement.Grant
			{
				Body = new GrantBody.Privileges(
					[
						new Privilege(PrivilegeKind.AllPrivileges, [], [], Abbreviated: true),
						new Privilege(PrivilegeKind.Execute, [], [], Abbreviated: true),
						new Privilege(PrivilegeKind.Execute, [], []),
						new Privilege(PrivilegeKind.ViewDefinition, [new Identifier("c")], []),
					],
					Securable(SecurableClass.Schema),
					[new Grantee.Identifier(new AuthorizationIdentifier(new Identifier("u"))), new Grantee.Null()],
					false,
					true,
					null),
			}).TrimEnd(';', ' '));

		Assert.Equal(
			"DENY ALTER ON DATABASE SCOPED CREDENTIAL::s TO NULL",
			Sql2023Writer.Write(new Statement.Deny(
				[new Privilege(PrivilegeKind.Alter, [], [])],
				[new Grantee.Null()],
				Securable(SecurableClass.DatabaseScopedCredential))).TrimEnd(';', ' '));
	}

	/// <summary>
	/// That T-SQL's shorter spelling is refused on a permission that has none, and that a securable is
	/// written with its class or without one.
	/// </summary>
	/// <remarks>
	/// <c>Abbreviated</c> is <c>ALL</c> and <c>EXEC</c>; on <c>SELECT</c> there is no text for it, and
	/// printing <c>SELECT</c> would lose the flag. A class beside the standard's kind word has no text
	/// either, and needs no refusal: <c>PrivilegeObject.Classed</c> has no kind to set.
	/// </remarks>
	[Fact]
	public void A_permission_part_with_no_text_is_refused()
	{
		var select = new Privilege(PrivilegeKind.Select, [], []);

		Assert.Throws<ArgumentOutOfRangeException>(() => Sql2023Writer.Write(Granted(null, select with { Abbreviated = true })));

		Assert.Contains("ON OBJECT::s", Sql2023Writer.Write(Granted(Securable(SecurableClass.Object), select)), StringComparison.Ordinal);
		Assert.Contains("ON s TO",      Sql2023Writer.Write(Granted(Securable(null), select)), StringComparison.Ordinal);
		Assert.Contains(
			"ON SCHEMA::s (c)",
			Sql2023Writer.Write(Granted(new PrivilegeObject.Classed(SecurableClass.Schema, new QualifiedName([new Identifier("s")]), [new Identifier("c")]), select)),
			StringComparison.Ordinal);
	}

	/// <summary>The two binary literals each write their own introducer and digits.</summary>
	[Fact]
	public void A_binary_literal_writes_its_own_introducer()
	{
		Assert.Equal("X'0A'", Sql2023Writer.Write(new Expression.Literal(new LiteralValue.Binary("'0A'"))));
		Assert.Equal("0x0A", Sql2023Writer.Write(new Expression.Literal(new LiteralValue.Binary("0A") { ZeroX = true })));
	}

	/// <summary>
	/// That a T-SQL value the writer has no text for is refused rather than written as something else.
	/// </summary>
	[Theory]
	[InlineData("Modifier on xml without a schema")]
	[InlineData("BracedEscape without an escape")]
	[InlineData("BracedEscape on SIMILAR TO")]
	[InlineData("a spelling a date part has no word for")]
	[InlineData("a variable that is not one")]
	[InlineData("TRY_CAST with a format")]
	[InlineData("an ODBC call without brackets that holds more than its name")]
	[InlineData("a 0x literal in quotes")]
	[InlineData("an X literal without quotes")]
	[InlineData("a written name that is not the key word")]
	[InlineData("a written name qualified by other than sys")]
	[InlineData("a written name on a type of several words")]
	[InlineData("a written name on a named type")]
	[InlineData("GraphPath with a WITHIN GROUP order")]
	[InlineData("an alias on an INSERT's target")]
	[InlineData("a sample on a DELETE's target")]
	[InlineData("a subquery as an UPDATE's target")]
	[InlineData("an alias on a table variable target")]
	[InlineData("an OUTPUT with no item")]
	[InlineData("an OUTPUT's columns without INTO")]
	[InlineData("a second OUTPUT without INTO")]
	[InlineData("an empty second FROM")]
	[InlineData("an INSERT word that is none")]
	[InlineData("DEFAULT VALUES with values in a MERGE")]
	[InlineData("a written name on xml qualified by dbo")]
	[InlineData("a 0x literal that is not hex digits")]
	[InlineData("a table variable with hints in a FROM")]
	[InlineData("a table variable with a sample in a FROM")]
	public void A_value_with_no_text_is_refused(string which)
	{
		var type = new DataType.Numeric(NumericTypeKind.Int);

		ISqlNode node = which switch
		{
			"Modifier on xml without a schema"      => new DataType.Xml(XmlTypeModifier.Content),
			"BracedEscape without an escape"        => new Expression.Like(A, false, LikeKind.Like, B) { BracedEscape = true },
			"BracedEscape on SIMILAR TO"            => new Expression.Like(A, false, LikeKind.Similar, B, A) { BracedEscape = true },
			"a spelling a date part has no word for" => new Expression.DatePart(DatePartKind.Millisecond, DatePartSpelling.SecondAbbreviation),
			"a variable that is not one"            => new Expression.Variable(new Identifier("x")),
			"TRY_CAST with a format"                => new Expression.Cast(A, type, "'YYYY'") { Try = true },
			"an ODBC call without brackets that holds more than its name" =>
				new Expression.OdbcEscape(new Identifier("fn"), new Expression.Invocation(T, []) { Quantifier = SetQuantifier.Distinct }, false),
			"a 0x literal in quotes"                => new Expression.Literal(new LiteralValue.Binary("'0A'") { ZeroX = true }),
			"a written name that is not the key word" => new DataType.Numeric(NumericTypeKind.Int) { WrittenName = new QualifiedName([new Identifier("[integer]", IdentifierStyle.Bracketed)]) },
			"a written name qualified by other than sys" => new DataType.Numeric(NumericTypeKind.Int) { WrittenName = new QualifiedName([new Identifier("dbo"), new Identifier("int")]) },
			"a written name on a type of several words" => new DataType.Numeric(NumericTypeKind.DoublePrecision) { WrittenName = new QualifiedName([new Identifier("[double precision]", IdentifierStyle.Bracketed)]) },
			"a written name on a named type"        => new DataType.Named(T, []) { WrittenName = T },
			"an alias on an INSERT's target"        => new Statement.Insert { Target = new TableSource.Named(T) { Alias = new Alias(new Identifier("x")) } },
			"a sample on a DELETE's target"         => new Statement.Delete { Target = new TableSource.Named(T) { Sample = new SampleClause(SampleMethod.System, A) } },
			"a subquery as an UPDATE's target"      => new Statement.Update { Target = new TableSource.Subquery(Selected), Assignments = [Assigned()] },
			"an alias on a table variable target"   => new Statement.Delete { Target = new TableSource.Variable(new Identifier("@t")) { Alias = new Alias(new Identifier("x")) } },
			"an OUTPUT with no item"                => new Statement.Delete { Target = new TableSource.Named(T), Output = new OutputClause([], null, null, null) },
			"an OUTPUT's columns without INTO"      => new Statement.Delete { Target = new TableSource.Named(T), Output = new OutputClause([new SelectColumn.ExpressionItem(A)], null, [new Identifier("a")], null) },
			"a second OUTPUT without INTO"          => new Statement.Delete
			{
				Target = new TableSource.Named(T),
				Output = new OutputClause([new SelectColumn.ExpressionItem(A)], null, null, new OutputClause([new SelectColumn.ExpressionItem(A)], null, null, null)),
			},
			"an empty second FROM"                  => new Statement.Delete { Target = new TableSource.Named(T), From = new FromClause([]) },
			"DEFAULT VALUES with values in a MERGE" => Merged() with { Clauses = [new MergeClause.NotMatched(new MergeInsertAction([], null, [A]) { DefaultValues = true })] },
			"an INSERT word that is none"           => new Statement.Insert { Target = new TableSource.Named(T), Into = (IntoKeyword)9 },
			"a written name on xml qualified by dbo" => new DataType.Xml() { WrittenName = new QualifiedName([new Identifier("dbo"), new Identifier("xml")]) },
			"a 0x literal that is not hex digits"   => new Expression.Literal(new LiteralValue.Binary("0x0A") { ZeroX = true }),
			"GraphPath with a WITHIN GROUP order"   => new Expression.Invocation(T, [new Argument(A)]) { GraphPath = true, WithinGroup = new WithinGroupClause(new OrderByClause([new SortItem(A)])) },
			"a table variable with hints in a FROM" => Joined(new TableSource.Variable(new Identifier("@t")) { Hints = [new TableHint.Named(new Identifier("NOLOCK"), [])] }),
			"a table variable with a sample in a FROM" => Joined(new TableSource.Variable(new Identifier("@t")) { Sample = new SampleClause(SampleMethod.System, A) }),
			_                                       => new Expression.Literal(new LiteralValue.Binary("0A")),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}

	/// <summary>A pivot of t, for a refusal to be asked of.</summary>
	static TableSource.Pivot Pivoted(TableSource source, SqlList<QualifiedName>? names = null, Alias? alias = null)
	{
		return new TableSource.Pivot(source, new Expression.Invocation(T, [new Argument(A)]), new QualifiedName([new Identifier("b")]), names ?? [new QualifiedName([new Identifier("[1]", IdentifierStyle.Bracketed)])], alias ?? new Alias(new Identifier("p")));
	}

	/// <summary><c>'s'</c>, a string written out.</summary>
	static readonly Expression.Literal Str = new(new LiteralValue.String("'s'", StringLiteralKind.Character));

	static readonly OpenDataSource Server = new(new Expression.Literal(new LiteralValue.String("'p'", StringLiteralKind.Character)), new Expression.Literal(new LiteralValue.String("'i'", StringLiteralKind.Character)));

	static RowsetColumn Column(string? path = null, int? ordinal = null, bool typed = true)
	{
		return new RowsetColumn(new Identifier("c"), typed ? new DataType.Numeric(NumericTypeKind.Int) : null, path, false, ordinal);
	}

	/// <summary>That a T-SQL part of a query the writer has no text for is refused, rather than written as another.</summary>
	[Theory]
	[InlineData("a table variable whose name is not a variable")]
	[InlineData("a variable assignment whose name is not a variable")]
	[InlineData("a named table of one part starting with @")]
	[InlineData("a join hint on a CROSS JOIN")]
	[InlineData("a join hint without a join type")]
	[InlineData("a PIVOT whose alias has columns")]
	[InlineData("a PIVOT with no names")]
	[InlineData("an ODBC join directly over a PIVOT")]
	[InlineData("a table variable whose alias has columns")]
	[InlineData("a data source with hints")]
	[InlineData("a data source with a sample")]
	[InlineData("a data source with a system time")]
	[InlineData("a data source with alias columns")]
	[InlineData("a data source with ONLY")]
	[InlineData("GROUP BY ALL with WITH CUBE")]
	[InlineData("GROUP BY ALL with a quantifier")]
	[InlineData("XMLNAMESPACES with RECURSIVE")]
	[InlineData("a query hint with arguments in brackets")]
	[InlineData("a hint written bare with two arguments")]
	[InlineData("a hint written with = and no argument")]
	[InlineData("an OPTIMIZE FOR variable with a value and UNKNOWN")]
	[InlineData("a FOR XML mode that is none")]
	[InlineData("a FOR directive that is none")]
	[InlineData("a name on a FOR directive other than ROOT")]
	[InlineData("ABSENT on a FOR directive other than ELEMENTS")]
	[InlineData("an XML directive under FOR JSON")]
	[InlineData("a JSON directive under FOR XML")]
	[InlineData("INTO variables and INTO a table")]
	[InlineData("an APPLY with NATURAL")]
	[InlineData("an APPLY with OUTER")]
	[InlineData("an APPLY with a hint")]
	[InlineData("an APPLY with ON")]
	[InlineData("OPENROWSET with a provider string and a login")]
	[InlineData("OPENROWSET with neither a provider string nor a login")]
	[InlineData("OPENROWSET with a provider that is not a string")]
	[InlineData("OPENROWSET with a login part that is not a string")]
	[InlineData("an element name that is not a string")]
	[InlineData("a FOR directive's name that is not a string")]
	[InlineData("USE HINT with a hint that is not a string")]
	[InlineData("a query hint named for a phrase's first word")]
	[InlineData("a query hint whose name is not a regular identifier")]
	[InlineData("FORCESEEK with arguments as a named hint")]
	[InlineData("an ORDER of no column")]
	[InlineData("OPENDATASOURCE with FOR PATH")]
	[InlineData("a string as a table's name")]
	[InlineData("a string as a column reference")]
	[InlineData("the alias first with no alias")]
	[InlineData("the alias first with AS")]
	[InlineData("the alias first that is a variable's name")]
	[InlineData("a BULK option with options")]
	[InlineData("a BULK ORDER option with a value")]
	[InlineData("OPENXML with a schema and a schema table")]
	[InlineData("T-SQL's sample flags on a source not a table's name")]
	[InlineData("SYSTEM left out of a BERNOULLI sample")]
	[InlineData("an empty schema")]
	[InlineData("a schema column without a type")]
	[InlineData("a schema column with a path and an ordinal")]
	[InlineData("a schema column whose path is not one string")]
	[InlineData("CHANGETABLE VERSION with no key column")]
	[InlineData("CHANGETABLE VERSION with no key value")]
	[InlineData("CONTAINSTABLE with no column")]
	[InlineData("CONTAINSTABLE with two columns unbracketed")]
	[InlineData("CONTAINSTABLE with * among other columns")]
	[InlineData("CONTAINSTABLE with PROPERTY in brackets")]
	[InlineData("FREETEXTTABLE with a qualified column")]
	[InlineData("FREETEXTTABLE with a value for a column")]
	[InlineData("a MATCH with no pattern")]
	[InlineData("a graph chain with no step")]
	[InlineData("a shortest path with no step")]
	[InlineData("a shortest path whose step is a last node")]
	[InlineData("+ with bounds other than {1,}")]
	[InlineData("a quantifier whose most is below its least")]
	[InlineData("a graph edge pointing nowhere")]
	[InlineData("FOR PATH on a LATERAL subquery")]
	public void A_query_part_with_no_text_is_refused(string which)
	{
		var named = new TableSource.Named(T);
		var json  = (SqlList<RowsetColumn>? schema) => Joined(new TableSource.OpenJson(A, null, schema));

		ISqlNode node = which switch
		{
			"a table variable whose name is not a variable"      => Joined(new TableSource.Variable(new Identifier("t"))),
			"a variable assignment whose name is not a variable" => new Query.Select { Columns = [new SelectColumn.VariableAssignment(new Identifier("x"), AssignmentOperator.Assign, A)] },
			"a named table of one part starting with @"          => Joined(new TableSource.Named(new QualifiedName([new Identifier("@t")]))),
			"a join hint on a CROSS JOIN"                        => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Kind = JoinKind.Cross, Hint = JoinHint.Hash }),
			"a join hint without a join type"                    => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Hint = JoinHint.Loop, Specification = new JoinSpecification.On(A) }),
			"a PIVOT whose alias has columns"                    => Joined(Pivoted(named, alias: new Alias(new Identifier("p"), [new Identifier("x")]))),
			"a PIVOT with no names"                              => Joined(Pivoted(named, [])),
			"an ODBC join directly over a PIVOT"                 => Joined(new TableSource.OdbcJoin(Pivoted(named))),
			"a table variable whose alias has columns"           => Joined(new TableSource.Variable(new Identifier("@t")) { Alias = new Alias(new Identifier("x"), [new Identifier("c")]) }),
			"a data source with hints"                           => Joined(named with { DataSource = Server, Hints = [new TableHint.Named(new Identifier("NOLOCK"), [])] }),
			"a data source with a sample"                        => Joined(named with { DataSource = Server, Sample = new SampleClause(SampleMethod.System, A) }),
			"a data source with a system time"                   => Joined(named with { DataSource = Server, SystemTime = new SystemTimeSpecification.All() }),
			"a data source with alias columns"                   => Joined(named with { DataSource = Server, Alias = new Alias(new Identifier("x"), [new Identifier("c")]) }),
			"a data source with ONLY"                            => Joined(named with { DataSource = Server, Only = true }),
			"GROUP BY ALL with WITH CUBE"                        => Selected with { GroupBy = new GroupByClause(null, [new GroupingElement.Ordinary([A])]) { AllKeyword = true, With = GroupByWith.Cube } },
			"GROUP BY ALL with a quantifier"                     => Selected with { GroupBy = new GroupByClause(SetQuantifier.All, [new GroupingElement.Ordinary([A])]) { AllKeyword = true } },
			"XMLNAMESPACES with RECURSIVE"                       => Selected with { With = new WithClause(true, []) { XmlNamespaces = [new XmlNamespace(new Expression.Literal(new LiteralValue.String("'u'", StringLiteralKind.Character)), null)] } },
			"a query hint with arguments in brackets"            => new Statement.Select { Query = Selected, Hints = [new QueryHint.Named(new Identifier("MAXDOP"), [A])] },
			"a hint written bare with two arguments"             => new Statement.Select { Query = Selected, Hints = [new QueryHint.Named(new Identifier("MAXDOP"), [A, B]) { Form = HintArgumentForm.Bare }] },
			"a hint written with = and no argument"              => Joined(named with { Hints = [new TableHint.Named(new Identifier("INDEX"), []) { Form = HintArgumentForm.Assigned }] }),
			"an OPTIMIZE FOR variable with a value and UNKNOWN"  => new Statement.Select { Query = Selected, Hints = [new QueryHint.OptimizeFor([new OptimizeForVariable(new Identifier("@a"), A, true)])] },
			"a FOR XML mode that is none"                        => Selected with { For = new ForClause.Xml((XmlMode)99, null, []) },
			"a FOR directive that is none"                       => Selected with { For = new ForClause.Xml(XmlMode.Raw, null, [new ForDirective((ForDirectiveKind)99)]) },
			"a name on a FOR directive other than ROOT"          => Selected with { For = new ForClause.Xml(XmlMode.Raw, null, [new ForDirective(ForDirectiveKind.Type, A)]) },
			"ABSENT on a FOR directive other than ELEMENTS"      => Selected with { For = new ForClause.Xml(XmlMode.Raw, null, [new ForDirective(ForDirectiveKind.ElementsXsiNil, Absent: true)]) },
			"an XML directive under FOR JSON"                    => Selected with { For = new ForClause.Json(JsonMode.Path, [new ForDirective(ForDirectiveKind.XmlSchema)]) },
			"a JSON directive under FOR XML"                     => Selected with { For = new ForClause.Xml(XmlMode.Raw, null, [new ForDirective(ForDirectiveKind.WithoutArrayWrapper)]) },
			"INTO variables and INTO a table"                    => Selected with { Into = new IntoClause([A]), IntoTable = new SelectIntoTable(T, null) },
			"an APPLY with NATURAL"                              => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Kind = JoinKind.CrossApply, Natural = true }),
			"an APPLY with OUTER"                                => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Kind = JoinKind.OuterApply, OuterKeyword = true }),
			"an APPLY with a hint"                               => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Kind = JoinKind.CrossApply, Hint = JoinHint.Hash }),
			"an APPLY with ON"                                   => Joined(new TableSource.Join { Left = named, Right = new TableSource.Named(U), Kind = JoinKind.CrossApply, Specification = new JoinSpecification.On(A) }),
			"OPENROWSET with a provider string and a login"      => Joined(new TableSource.OpenRowset(Str, Str, new RowsetLogin(Str, Str, Str), A)),
			"OPENROWSET with a provider that is not a string"    => Joined(new TableSource.OpenRowset(A, Str, null, A)),
			"OPENROWSET with a login part that is not a string"  => Joined(new TableSource.OpenRowset(Str, null, new RowsetLogin(Str, A, Str), A)),
			"an element name that is not a string"               => Selected with { For = new ForClause.Xml(XmlMode.Path, A, []) },
			"a FOR directive's name that is not a string"        => Selected with { For = new ForClause.Xml(XmlMode.Raw, null, [new ForDirective(ForDirectiveKind.Root, A)]) },
			"USE HINT with a hint that is not a string"          => new Statement.Select { Query = Selected, Hints = [new QueryHint.UseHint([A])] },
			"a query hint named for a phrase's first word"       => new Statement.Select { Query = Selected, Hints = [new QueryHint.Named(new Identifier("HASH"), [])] },
			"a query hint whose name is not a regular identifier" => new Statement.Select { Query = Selected, Hints = [new QueryHint.Named(new Identifier("[KEEP PLAN]", IdentifierStyle.Bracketed), [])] },
			"FORCESEEK with arguments as a named hint"           => Joined(named with { Hints = [new TableHint.Named(new Identifier("FORCESEEK"), [A])] }),
			"an ORDER of no column"                              => Joined(new TableSource.OpenRowsetBulk(A, [new Option(new Identifier("ORDER"), null, []) { Order = new RowOrder([], false) }], null)),
			"a string as a table's name"                         => Joined(new TableSource.Named(new QualifiedName([new Identifier("t", IdentifierStyle.String)]))),
			"a string as a column reference"                     => Selected with { Columns = [new SelectColumn.ExpressionItem(new Expression.Reference(new QualifiedName([new Identifier("a", IdentifierStyle.NationalString)])))] },
			"the alias first with no alias"                      => Selected with { Columns = [new SelectColumn.ExpressionItem(A) { AliasFirst = true }] },
			"the alias first with AS"                            => Selected with { Columns = [new SelectColumn.ExpressionItem(A, new Identifier("x"), true) { AliasFirst = true }] },
			"the alias first that is a variable's name"          => Selected with { Columns = [new SelectColumn.ExpressionItem(A, new Identifier("@x")) { AliasFirst = true }] },
			"OPENDATASOURCE with FOR PATH"                       => Joined(named with { DataSource = Server, ForPath = true }),
			"OPENROWSET with neither a provider string nor a login" => Joined(new TableSource.OpenRowset(Str, null, null, A)),
			"a BULK option with options"                         => Joined(new TableSource.OpenRowsetBulk(A, [new Option(new Identifier("FORMAT"), A, [new Option(new Identifier("X"), null, [])])], null)),
			"a BULK ORDER option with a value"                   => Joined(new TableSource.OpenRowsetBulk(A, [new Option(new Identifier("ORDER"), A, []) { Order = new RowOrder([new SortItem(A)], false) }], null)),
			"OPENXML with a schema and a schema table"           => Joined(new TableSource.OpenXml(A, B, null, [Column()], T)),
			"T-SQL's sample flags on a source not a table's name" => Joined(new TableSource.Function(new Expression.Invocation(T, [])) { Sample = new SampleClause(SampleMethod.System, A) { Unit = SampleUnit.Rows } }),
			"SYSTEM left out of a BERNOULLI sample"              => Joined(named with { Sample = new SampleClause(SampleMethod.Bernoulli, A) { MethodOmitted = true } }),
			"CHANGETABLE VERSION with no key column"             => Joined(new TableSource.ChangeTableVersion(T, [], [A], false)),
			"CHANGETABLE VERSION with no key value"              => Joined(new TableSource.ChangeTableVersion(T, [new Identifier("k")], [], false)),
			"CONTAINSTABLE with no column"                       => Joined(new TableSource.ContainsTable(T, [], A, null, null) { ColumnsBracketed = true }),
			"CONTAINSTABLE with two columns unbracketed"         => Joined(new TableSource.ContainsTable(T, [Searched("a"), Searched("b")], A, null, null)),
			"CONTAINSTABLE with * among other columns"           => Joined(new TableSource.ContainsTable(T, [Searched(), new Expression.Asterisk()], A, null, null) { ColumnsBracketed = true }),
			"CONTAINSTABLE with PROPERTY in brackets"            => Joined(new TableSource.ContainsTable(T, [new Expression.Invocation(new QualifiedName([new Identifier("PROPERTY")]), [new Argument(Searched()), new Argument(A)])], A, null, null) { ColumnsBracketed = true }),
			"FREETEXTTABLE with a qualified column"              => Joined(new TableSource.FreetextTable(T, [new Expression.Reference(new QualifiedName([new Identifier("t"), new Identifier("c")]))], A, null, null)),
			"FREETEXTTABLE with a value for a column"            => Joined(new TableSource.FreetextTable(T, [new Expression.Variable(new Identifier("@v"))], A, null, null)),
			"a MATCH with no pattern"                            => Matched(),
			"a graph chain with no step"                         => Matched(new GraphPattern.Chain(Node("a"), [])),
			"a shortest path with no step"                       => Matched(new GraphPattern.ShortestPath(Node("a"), [], new GraphQuantifier(1, null, true), false)),
			"a shortest path whose step is a last node"          => Matched(Shortest(new GraphQuantifier(1, null, true), step: Node("b", true))),
			"+ with bounds other than {1,}"                      => Matched(Shortest(new GraphQuantifier(1, 3, true))),
			"a quantifier whose most is below its least"         => Matched(Shortest(new GraphQuantifier(3, 1, false))),
			"a graph edge pointing nowhere"                      => Matched(new GraphPattern.Chain(Node("a"), [new GraphStep(new GraphEdge(new Identifier("e"), (GraphDirection)9), Node("b"))])),
			"FOR PATH on a LATERAL subquery"                     => Joined(new TableSource.Subquery(Selected) { Lateral = true, ForPath = true }),
			"an empty schema"                                    => json([]),
			"a schema column without a type"                     => json([Column(typed: false)]),
			"a schema column with a path and an ordinal"         => json([Column("'$.a'", 1)]),
			_                                                    => json([Column("'$.a') AS j; DROP TABLE t; --")]),
		};

		Assert.ThrowsAny<ArgumentException>(() => Sql2023Writer.Write(node));
	}

	/// <summary>
	/// That the writer brackets what the grammar would otherwise read another way, and writes the T-SQL parts of a
	/// named table even where TABLE t would otherwise stand for it.
	/// </summary>
	[Theory]
	[InlineData("a PIVOT of a join",            "(t CROSS JOIN u) PIVOT (t(a) FOR b IN ([1])) p")]
	[InlineData("an APPLY of a join",           "t OUTER APPLY (u LEFT JOIN v ON a)")]
	[InlineData("TABLE t with a data source",   "OPENDATASOURCE('p', 'i').t")]
	[InlineData("xml written as a name",        "[xml](CONTENT s)")]
	[InlineData("xml qualified by sys",         "sys.xml")]
	[InlineData("a string alias",               "SELECT a 'b c'")]
	[InlineData("a national string alias first", "SELECT N'h''i' = a")]
	public void The_writer_brackets_and_keeps_what_the_grammar_needs(string which, string written)
	{
		var t = new TableSource.Named(T);
		var u = new TableSource.Named(U);
		var v = new TableSource.Named(new QualifiedName([new Identifier("v")]));

		ISqlNode node = which switch
		{
			"a PIVOT of a join"  => Joined(Pivoted(new TableSource.Join { Left = t, Right = u, Kind = JoinKind.Cross })),
			"an APPLY of a join" => Joined(new TableSource.Join
			{
				Left  = t,
				Right = new TableSource.Join { Left = u, Right = v, Kind = JoinKind.Left, Specification = new JoinSpecification.On(A) },
				Kind  = JoinKind.OuterApply,
			}),
			"xml written as a name" => new DataType.Xml(XmlTypeModifier.Content, new QualifiedName([new Identifier("s")]))
			{
				WrittenName = new QualifiedName([new Identifier("[xml]", IdentifierStyle.Bracketed)]),
			},
			"a string alias"       => Selected with { Columns = [new SelectColumn.ExpressionItem(A, new Identifier("b c", IdentifierStyle.String))] },
			"a national string alias first" => Selected with
			{
				Columns = [new SelectColumn.ExpressionItem(A, new Identifier("h'i", IdentifierStyle.NationalString)) { AliasFirst = true }],
			},
			"xml qualified by sys" => new DataType.Xml() { WrittenName = new QualifiedName([new Identifier("sys"), new Identifier("xml")]) },
			_                    => new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([t with { DataSource = Server }]), Table = true },
		};

		Assert.Contains(written, Sql2023Writer.Write(node), StringComparison.Ordinal);
	}

	/// <summary>That a name which is neither a name nor a variable is refused rather than skipped.</summary>
	[Fact]
	public void A_transaction_name_that_names_nothing_is_refused()
	{
		var empty = new Statement.Commit { Keyword = TransactionKeyword.Transaction, Name = new TransactionName(null) };

		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(empty));
	}

	// A value the writer refuses outright is not a silent one, so a throw is read as "it has text for
	// this and declines to print it here", which is what the throwing default is for.
	static string? Write(Func<object, ISqlNode> build, object value)
	{
		try
		{
			return Sql2023Writer.Write(build(value));
		}
		catch (Exception)
		{
			return "(refused)";
		}
	}
}
