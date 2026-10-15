using System;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Identifier = DotGram.Sql.Ast.Identifier;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The Security family's writer (Sql2023Writer.TransactSql.Security.cs): each record written as the words of its kind and the
/// slots the grammar reads for it, and every tree whose text would read back as another tree refused.
/// </summary>
public sealed class SecurityWriterTests
{
	static readonly Identifier X = new("x");
	static readonly Identifier Y = new("y");
	static readonly QualifiedName T = new([new Identifier("t")]);
	static readonly QualifiedName Q = new([new Identifier("dbo"), new Identifier("t")]);

	static Privilege Permission(PrivilegeKind kind, params string[] columns)
	{
		return new Privilege(kind, [.. columns.Select(static one => new Identifier(one))], []);
	}

	static Grantee Principal(string name)
	{
		return new Grantee.Identifier(new AuthorizationIdentifier(new Identifier(name)));
	}

	static Statement.Grant Granted(SqlList<Privilege> items, PrivilegeObject? on, GrantBody.Privileges? with = null)
	{
		return new Statement.Grant
		{
			Body = (with ?? new GrantBody.Privileges(items, on, [Principal("u")], false, false, null)) with { Items = items, Object = on },
		};
	}

	static Statement.Revoke Revoked(RevokeBody.Privileges body)
	{
		return new Statement.Revoke { Body = body };
	}

	static RevokeBody.Privileges Taken(SqlList<Privilege> items, PrivilegeObject? on)
	{
		return new RevokeBody.Privileges(null, items, on, [Principal("u")], null, null);
	}

	/// <summary>A tree for each statement of the permission group, and the text it writes.</summary>
	static Statement Tree(string which)
	{
		var select = Permission(PrivilegeKind.Select);

		return which switch
		{
			"Grant" => new Statement.Grant
			{
				Body = new GrantBody.Privileges(
					[select, Permission(PrivilegeKind.Update, "a", "b")], new PrivilegeObject.Named(PrivilegeObjectKind.Table, Q), [Principal("u"), new Grantee.Public(), new Grantee.Null()],
					false, true, null, new Identifier("keeper")),
			},
			"Grant server permission" => Granted([Permission(PrivilegeKind.ViewServerState)], null),
			"Grant on a class" => Granted([Permission(PrivilegeKind.Alter)], new PrivilegeObject.Classed(SecurableClass.ServerRole, T)),
			"Grant abbreviated" => Granted(
				[new Privilege(PrivilegeKind.AllPrivileges, [], [], true), new Privilege(PrivilegeKind.Execute, [], [], true), Permission(PrivilegeKind.Execute)],
				new PrivilegeObject.Classed(SecurableClass.Object, Q)),
			"Grant on columns of the securable" => Granted([select], new PrivilegeObject.Named(PrivilegeObjectKind.Table, T, Columns: [X, Y])),
			"Deny" => new Statement.Deny([select], [Principal("u"), new Grantee.Null()], new PrivilegeObject.Named(PrivilegeObjectKind.Table, T), true, new Identifier("keeper")),
			"Deny a class" => new Statement.Deny([Permission(PrivilegeKind.ViewDefinition)], [new Grantee.Public()], new PrivilegeObject.Classed(SecurableClass.Schema, T)),
			"Revoke" => Revoked(new RevokeBody.Privileges(
				RevokeOption.GrantOptionFor, [select], new PrivilegeObject.Named(PrivilegeObjectKind.Table, T), [Principal("u")], null, DropBehavior.Cascade, new Identifier("keeper"))),
			"Revoke to" => Revoked(Taken([select], new PrivilegeObject.Named(PrivilegeObjectKind.Table, T)) with { To = true }),
			"Revoke server permission" => Revoked(Taken([Permission(PrivilegeKind.CreateTable)], null)),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each statement written as the text of its kind.</summary>
	[Theory]
	[InlineData("Grant", "GRANT SELECT, UPDATE (a, b) ON dbo.t TO u, PUBLIC, NULL WITH GRANT OPTION AS keeper")]
	[InlineData("Grant server permission", "GRANT VIEW SERVER STATE TO u")]
	[InlineData("Grant on a class", "GRANT ALTER ON SERVER ROLE::t TO u")]
	[InlineData("Grant abbreviated", "GRANT ALL, EXEC, EXECUTE ON OBJECT::dbo.t TO u")]
	[InlineData("Grant on columns of the securable", "GRANT SELECT ON t (x, y) TO u")]
	[InlineData("Deny", "DENY SELECT ON t TO u, NULL CASCADE AS keeper")]
	[InlineData("Deny a class", "DENY VIEW DEFINITION ON SCHEMA::t TO PUBLIC")]
	[InlineData("Revoke", "REVOKE GRANT OPTION FOR SELECT ON t FROM u CASCADE AS keeper")]
	[InlineData("Revoke to", "REVOKE SELECT ON t TO u")]
	[InlineData("Revoke server permission", "REVOKE CREATE TABLE FROM u")]
	public void Each_statement_writes_its_kind(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(Tree(which)));
	}

	/// <summary>
	/// Each statement's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree.
	/// </summary>
	[Theory]
	[InlineData("Grant")]
	[InlineData("Grant server permission")]
	[InlineData("Grant on a class")]
	[InlineData("Grant abbreviated")]
	[InlineData("Grant on columns of the securable")]
	[InlineData("Deny")]
	[InlineData("Deny a class")]
	[InlineData("Revoke")]
	[InlineData("Revoke to")]
	[InlineData("Revoke server permission")]
	public void Each_statement_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("a grant of no permission")]
	[InlineData("a grant to no one")]
	[InlineData("a grant of roles")]
	[InlineData("a grant with the hierarchy option")]
	[InlineData("a grant granted by CURRENT_USER")]
	[InlineData("a permission on a class that has none of it")]
	[InlineData("a permission on a name with none of it")]
	[InlineData("a permission with a routine")]
	[InlineData("ON TABLE t")]
	[InlineData("ON a domain")]
	[InlineData("ON a routine")]
	[InlineData("a deny of no permission")]
	[InlineData("a deny to no one")]
	[InlineData("a revoke of roles")]
	[InlineData("a revoke of the hierarchy option")]
	[InlineData("a revoke granted by CURRENT_ROLE")]
	[InlineData("a revoke with RESTRICT")]
	[InlineData("a revoke of no permission")]
	[InlineData("a revoke from no one")]
	[InlineData("an abbreviation of SELECT")]
	[InlineData("a grantee named PUBLIC")]
	[InlineData("AS a reserved word")]
	public void A_permission_statement_with_no_text_is_refused(string which)
	{
		var select = Permission(PrivilegeKind.Select);
		var table  = new PrivilegeObject.Named(PrivilegeObjectKind.Table, T);

		ISqlNode node = which switch
		{
			// Permissions reads one permission at least.
			"a grant of no permission" => Granted([], null),
			// Grantees reads one principal at least.
			"a grant to no one" => new Statement.Grant { Body = new GrantBody.Privileges([select], table, [], false, false, null) },
			// PermissionStatement grants permissions only: `GRANT role TO u` is Msg 102 at the role's name.
			"a grant of roles" => new Statement.Grant { Body = new GrantBody.Roles([X], [Principal("u")], false, null) },
			// After the grantees PermissionStatement reads WITH GRANT OPTION only.
			"a grant with the hierarchy option" => new Statement.Grant { Body = new GrantBody.Privileges([select], table, [Principal("u")], true, false, null) },
			// After the grantees PermissionStatement reads WITH GRANT OPTION and AS only.
			"a grant granted by CURRENT_USER" => new Statement.Grant { Body = new GrantBody.Privileges([select], table, [Principal("u")], false, false, Grantor.CurrentUser) },
			// The guard of PermissionStatement: `GRANT SELECT ON ENDPOINT::x` is Msg 102 near SELECT.
			"a permission on a class that has none of it" => Granted([select], new PrivilegeObject.Classed(SecurableClass.Endpoint, T)),
			// The same guard reads a name with no class as OBJECT, which has no CREATE TABLE.
			"a permission on a name with none of it" => Granted([Permission(PrivilegeKind.CreateTable)], table),
			// Permission reads a name and a column list: no routine designator.
			"a permission with a routine" => Granted([new Privilege(PrivilegeKind.Execute, [], [new RoutineDesignator(RoutineKind.Procedure, T)])], null),
			// PermissionOn reads a class and `::` or a name: no TABLE.
			"ON TABLE t" => Granted([select], new PrivilegeObject.Named(PrivilegeObjectKind.Table, T, TableKeyword: true)),
			// PermissionOn reads a class and `::` or a name: DOMAIN is no class.
			"ON a domain" => Granted([select], new PrivilegeObject.Named(PrivilegeObjectKind.Domain, T)),
			// PermissionOn reads a name, not a routine designator.
			"ON a routine" => Granted([select], new PrivilegeObject.Named(PrivilegeObjectKind.Routine, T, new RoutineDesignator(RoutineKind.Procedure, T))),
			// Permissions reads one permission at least.
			"a deny of no permission" => new Statement.Deny([], [Principal("u")]),
			// Grantees reads one principal at least.
			"a deny to no one" => new Statement.Deny([select], []),
			// PermissionStatement revokes permissions only.
			"a revoke of roles" => new Statement.Revoke { Body = new RevokeBody.Roles(false, [X], [Principal("u")], null, DropBehavior.Cascade) },
			// Before the permissions REVOKE reads GRANT OPTION FOR only.
			"a revoke of the hierarchy option" => Revoked(Taken([select], table) with { Option = RevokeOption.HierarchyOptionFor }),
			// After the grantees REVOKE reads CASCADE and AS only.
			"a revoke granted by CURRENT_ROLE" => Revoked(Taken([select], table) with { GrantedBy = Grantor.CurrentRole }),
			// After the grantees REVOKE reads CASCADE or nothing: RESTRICT is Msg 102.
			"a revoke with RESTRICT" => Revoked(Taken([select], table) with { Behavior = DropBehavior.Restrict }),
			// Permissions reads one permission at least.
			"a revoke of no permission" => Revoked(Taken([], null)),
			// Grantees reads one principal at least.
			"a revoke from no one" => Revoked(Taken([select], table) with { Grantees = [] }),
			// SELECT has no shorter word: only ALL and EXEC do.
			"an abbreviation of SELECT" => Granted([select with { Abbreviated = true }], null),
			// A plain PUBLIC is the reserved word, which Grantee reads as Public.
			"a grantee named PUBLIC" => new Statement.Grant { Body = new GrantBody.Privileges([select], table, [Principal("public")], false, false, null) },
			// PrincipalName is an Identifier, which is no reserved word plain.
			"AS a reserved word" => new Statement.Deny([select], [Principal("u")], null, false, new Identifier("select")),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
