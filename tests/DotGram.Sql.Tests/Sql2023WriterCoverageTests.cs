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

	/// <summary>A revoked SELECT on one table, written with the page's FROM or its TO.</summary>
	static RevokeBody Revoked(bool to)
	{
		return new RevokeBody.Privileges(
			null,
			[new Privilege(PrivilegeKind.Select, [], [])],
			new PrivilegeObject(PrivilegeObjectKind.Table, new QualifiedName([new Identifier("t")])),
			[new Grantee.Public()],
			null,
			null,
			To: to);
	}

	/// <summary>A GRANT of the privileges given, on the securable given or on none, to PUBLIC.</summary>
	static Statement.Grant Granted(PrivilegeObject? on, params Privilege[] privileges)
	{
		return new Statement.Grant
		{
			Body = new GrantBody.Privileges(privileges, on, [new Grantee.Public()], false, false, null),
		};
	}

	/// <summary>A securable of one name, written with T-SQL's class or without one.</summary>
	static PrivilegeObject Securable(SecurableClass? of)
	{
		return new PrivilegeObject(PrivilegeObjectKind.Table, new QualifiedName([new Identifier("s")]), Class: of);
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
				new Statement.Select { Items = [new SelectItem.ExpressionItem(A)] },
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
		[typeof(NumericLiteralKind)] = v => new Expression.Literal(new LiteralValue.Numeric("1", (NumericLiteralKind)v)),
		[typeof(DateTimeLiteralKind)] = v => new Expression.Literal(new LiteralValue.DateTime((DateTimeLiteralKind)v, "2026-09-26")),
		[typeof(NumericTypeKind)]    = v => new DataType.Numeric((NumericTypeKind)v),
		[typeof(DateTimeTypeKind)]   = v => new DataType.DateTime((DateTimeTypeKind)v),
		[typeof(CharacterTypeKind)]  = v => new DataType.Character((CharacterTypeKind)v),
		[typeof(BinaryTypeKind)]     = v => new DataType.Binary((BinaryTypeKind)v),
		[typeof(IdentifierStyle)]    = v => new Identifier("x", (IdentifierStyle)v),
		[typeof(TransactionKeyword)] = v => new Statement.Commit { Keyword = (TransactionKeyword)v },
		// A hundred and seventy permissions and twenty-eight classes, generated from the catalogue (D148):
		// this is what says each one has words of its own, where PermissionCatalogueTests says which.
		[typeof(PrivilegeKind)]      = v => Granted(null, new Privilege((PrivilegeKind)v, [], [])),
		[typeof(SecurableClass)]     = v => Granted(Securable((SecurableClass)v), new Privilege(PrivilegeKind.Control, [], [])),
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
		var flags = marked.Where(one => one.PropertyType == typeof(bool))
			.Select(one => one.DeclaringType!.Name + "." + one.Name)
			.Distinct(StringComparer.Ordinal)
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToList();

		Assert.Equal(flags, pairs.Keys.OrderBy(one => one, StringComparer.Ordinal));

		foreach (var one in marked.Where(one => one.PropertyType != typeof(bool)))
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
	public void A_flag_changes_what_is_written(string which)
	{
		var (without, with) = which switch
		{
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
				new PrivilegeObject(PrivilegeObjectKind.Table, table),
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
					new PrivilegeObject(PrivilegeObjectKind.Table, table, Columns: [new Identifier("a"), new Identifier("b")]),
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
	/// That the two T-SQL parts of a permission statement that say something only of certain values are
	/// refused where they say nothing: a shorter spelling of a permission that has none, and a class beside
	/// the standard's kind.
	/// </summary>
	/// <remarks>
	/// <c>Abbreviated</c> is <c>ALL</c> and <c>EXEC</c>; on <c>SELECT</c> there is no text for it, and
	/// printing <c>SELECT</c> would lose the flag. A class is the word before the name, where the
	/// standard's <c>DOMAIN</c> or <c>TABLE</c> would stand, so a securable with both has no text either.
	/// </remarks>
	[Fact]
	public void A_permission_part_with_no_text_is_refused()
	{
		var select = new Privilege(PrivilegeKind.Select, [], []);

		Assert.Throws<ArgumentOutOfRangeException>(() => Sql2023Writer.Write(Granted(null, select with { Abbreviated = true })));

		Assert.Contains("ON OBJECT::s", Sql2023Writer.Write(Granted(Securable(SecurableClass.Object), select)), StringComparison.Ordinal);
		Assert.Contains("ON s", Sql2023Writer.Write(Granted(Securable(null), select)), StringComparison.Ordinal);

		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(Granted(Securable(SecurableClass.Object) with { Kind = PrivilegeObjectKind.Domain }, select)));
		Assert.Throws<ArgumentException>(() => Sql2023Writer.Write(Granted(Securable(SecurableClass.Object) with { TableKeyword = true }, select)));
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
