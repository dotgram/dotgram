using System;
using System.Linq;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
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

	static Expression Str(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Expression Hex(string digits)
	{
		return new LiteralValue.Binary(digits) { ZeroX = true };
	}

	static Expression Word(string word)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(word)]));
	}

	static Expression Digits(string digits)
	{
		return new LiteralValue.Numeric(digits, NumericLiteralKind.DecimalInteger);
	}

	static QualifiedName One(Identifier name)
	{
		return new QualifiedName([name]);
	}

	static Option Opt(string name, Expression? value = null, params Option[] options)
	{
		return new Option(new Identifier(name), value, [.. options]);
	}

	static Statement Procedure()
	{
		const string text = "CREATE PROCEDURE p AS SELECT 1";

		return TransactSqlParser.Located.ParseStatement(text);
	}

	static Statement Trigger()
	{
		const string text = "CREATE TRIGGER tr ON t AFTER INSERT AS SELECT 1";

		return TransactSqlParser.Located.ParseStatement(text);
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
	/// Each statement's tree, written and read back by TransactSqlParser, as the same tree.
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

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
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

	/// <summary>A tree for each statement of the principals and schemas group.</summary>
	static Statement PrincipalTree(string which)
	{
		return which switch
		{
			"Create role" => new Statement.CreateRole { Name = X, Authorization = Y },
			"Create role alone" => new Statement.CreateRole { Name = X },
			"Alter role add" => new Statement.AlterRole(X, new RoleAction.AddMember(Y)),
			"Alter role drop" => new Statement.AlterRole(X, new RoleAction.DropMember(Y)),
			"Alter role name" => new Statement.AlterRole(X, new RoleAction.WithName(Y)),
			"Create server role" => new Statement.CreateServerRole(X) { Authorization = Y },
			"Alter server role" => new Statement.AlterServerRole(X, new RoleAction.AddMember(Y)),
			"Create application role" => new Statement.CreateApplicationRole(X, [Opt("PASSWORD", Str("p")), Opt("DEFAULT_SCHEMA", Word("s"))]),
			"Alter application role" => new Statement.AlterApplicationRole(X, [Opt("NAME", Word("n")), Opt("LOGIN", Word("l")), Opt("DEFAULT_LANGUAGE", Word("us_english"))]),
			"Create login password" => new Statement.CreateLogin(X, new LoginSource.Password(Str("p")) { Hashed = true, MustChange = true })
			{
				Options = [Opt("SID", Hex("01")), Opt("DEFAULT_DATABASE", Word("d")), Opt("CHECK_POLICY", Word("OFF")), Opt("CREDENTIAL", Word("c"))],
			},
			"Create login hash" => new Statement.CreateLogin(X, new LoginSource.Password(Hex("0100")) { Hashed = true, MustChange = true, MustChangeFirst = true }),
			"Create login windows" => new Statement.CreateLogin(X, new LoginSource.Windows()) { Options = [Opt("DEFAULT_DATABASE", Word("d")), Opt("DEFAULT_LANGUAGE", Word("l"))] },
			"Create login external" => new Statement.CreateLogin(X, new LoginSource.ExternalProvider()) { Options = [Opt("OBJECT_ID", Str("o")), Opt("TYPE", Word("E"))] },
			// ExternalLoginOption is read as often as it is written: the engine objects afterwards, the grammar does not.
			"Create login repeated" => new Statement.CreateLogin(X, new LoginSource.ExternalProvider()) { Options = [Opt("TYPE", Word("E")), Opt("TYPE", Word("X"))] },
			"Create login certificate" => new Statement.CreateLogin(X, new LoginSource.Certificate(Y, new Identifier("cr"))),
			"Create login asymmetric key" => new Statement.CreateLogin(X, new LoginSource.AsymmetricKey(Y, null)),
			"Alter login enable" => new Statement.AlterLogin(X, new AlterLoginAction.Enable()),
			"Alter login disable" => new Statement.AlterLogin(X, new AlterLoginAction.Disable()),
			"Alter login with" => new Statement.AlterLogin(X, new AlterLoginAction.With(
				[Opt("PASSWORD", Str("p"), Opt("HASHED"), Opt("UNLOCK"), Opt("MUST_CHANGE")), Opt("NAME", Word("n")), new Option(new Identifier("NO"), null, []) { Words = [new Identifier("CREDENTIAL")] }])),
			"Alter login old password" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Str("p"), Opt("OLD_PASSWORD", Str("q")))])),
			"Alter login hash" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Hex("01"), Opt("MUST_CHANGE"), Opt("HASHED"))])),
			"Alter login add credential" => new Statement.AlterLogin(X, new AlterLoginAction.AddCredential(Y)),
			"Alter login drop credential" => new Statement.AlterLogin(X, new AlterLoginAction.DropCredential(Y)),
			"Create user" => new Statement.CreateUser(X),
			"Create user for login" => new Statement.CreateUser(X) { Source = new UserSource.Login(Y), Options = [Opt("DEFAULT_SCHEMA", Word("s")), Opt("DEFAULT_LANGUAGE", Digits("1033"))] },
			"Create user from login" => new Statement.CreateUser(X) { Source = new UserSource.Login(Y) { From = true } },
			"Create user from certificate" => new Statement.CreateUser(X) { Source = new UserSource.Certificate(Y) },
			"Create user for asymmetric key" => new Statement.CreateUser(X) { Source = new UserSource.AsymmetricKey(Y) { From = true } },
			"Create user external" => new Statement.CreateUser(X) { Source = new UserSource.ExternalProvider { From = true }, Options = [Opt("OBJECT_ID", Str("o"))] },
			"Create user without login" => new Statement.CreateUser(X) { Source = new UserSource.WithoutLogin(), Options = [Opt("ALLOW_ENCRYPTED_VALUE_MODIFICATIONS", Word("ON"))] },
			"Create user with password" => new Statement.CreateUser(X) { Options = [Opt("PASSWORD", Str("p")), Opt("SID", Hex("0A")), Opt("TYPE", Word("E"))] },
			"Alter user" => new Statement.AlterUser(X, [Opt("NAME", Word("n")), Opt("DEFAULT_SCHEMA", new LiteralValue.Null()), Opt("LOGIN", Word("l")), Opt("PASSWORD", Str("p"), Opt("OLD_PASSWORD", Str("q")))]),
			"Alter schema" => new Statement.AlterSchema(X, Q),
			"Alter schema class" => new Statement.AlterSchema(X, T) { Class = SecurableClass.XmlSchemaCollection },
			"Alter authorization" => new Statement.AlterAuthorization(Q, new AuthorizationTo.Principal(Y)),
			"Alter authorization class" => new Statement.AlterAuthorization(T, new AuthorizationTo.SchemaOwner()) { Class = SecurableClass.Object },
			"Create schema" => new Statement.CreateSchema { Name = T, Authorization = new AuthorizationIdentifier(Y), Elements = [Tree("Grant"), new Statement.CreateTable { Name = Q, Contents = new TableContents.Elements([new TableElement.Column(new ColumnDefinition(X, new DataType.Numeric(NumericTypeKind.Int), null, [], null))]) }] },
			"Create schema owner only" => new Statement.CreateSchema { Authorization = new AuthorizationIdentifier(Y) },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each statement written as the text of its kind.</summary>
	[Theory]
	[InlineData("Create role", "CREATE ROLE x AUTHORIZATION y")]
	[InlineData("Create role alone", "CREATE ROLE x")]
	[InlineData("Alter role add", "ALTER ROLE x ADD MEMBER y")]
	[InlineData("Alter role drop", "ALTER ROLE x DROP MEMBER y")]
	[InlineData("Alter role name", "ALTER ROLE x WITH NAME = y")]
	[InlineData("Create server role", "CREATE SERVER ROLE x AUTHORIZATION y")]
	[InlineData("Alter server role", "ALTER SERVER ROLE x ADD MEMBER y")]
	[InlineData("Create application role", "CREATE APPLICATION ROLE x WITH PASSWORD = 'p', DEFAULT_SCHEMA = s")]
	[InlineData("Alter application role", "ALTER APPLICATION ROLE x WITH NAME = n, LOGIN = l, DEFAULT_LANGUAGE = us_english")]
	[InlineData("Create login password", "CREATE LOGIN x WITH PASSWORD = 'p' HASHED MUST_CHANGE, SID = 0x01, DEFAULT_DATABASE = d, CHECK_POLICY = OFF, CREDENTIAL = c")]
	[InlineData("Create login hash", "CREATE LOGIN x WITH PASSWORD = 0x0100 MUST_CHANGE HASHED")]
	[InlineData("Create login windows", "CREATE LOGIN x FROM WINDOWS WITH DEFAULT_DATABASE = d, DEFAULT_LANGUAGE = l")]
	[InlineData("Create login external", "CREATE LOGIN x FROM EXTERNAL PROVIDER WITH OBJECT_ID = 'o', TYPE = E")]
	[InlineData("Create login repeated", "CREATE LOGIN x FROM EXTERNAL PROVIDER WITH TYPE = E, TYPE = X")]
	[InlineData("Create login certificate", "CREATE LOGIN x FROM CERTIFICATE y WITH CREDENTIAL = cr")]
	[InlineData("Create login asymmetric key", "CREATE LOGIN x FROM ASYMMETRIC KEY y")]
	[InlineData("Alter login enable", "ALTER LOGIN x ENABLE")]
	[InlineData("Alter login disable", "ALTER LOGIN x DISABLE")]
	[InlineData("Alter login with", "ALTER LOGIN x WITH PASSWORD = 'p' HASHED UNLOCK MUST_CHANGE, NAME = n, NO CREDENTIAL")]
	[InlineData("Alter login old password", "ALTER LOGIN x WITH PASSWORD = 'p' OLD_PASSWORD = 'q'")]
	[InlineData("Alter login hash", "ALTER LOGIN x WITH PASSWORD = 0x01 MUST_CHANGE HASHED")]
	[InlineData("Alter login add credential", "ALTER LOGIN x ADD CREDENTIAL y")]
	[InlineData("Alter login drop credential", "ALTER LOGIN x DROP CREDENTIAL y")]
	[InlineData("Create user", "CREATE USER x")]
	[InlineData("Create user for login", "CREATE USER x FOR LOGIN y WITH DEFAULT_SCHEMA = s, DEFAULT_LANGUAGE = 1033")]
	[InlineData("Create user from login", "CREATE USER x FROM LOGIN y")]
	[InlineData("Create user from certificate", "CREATE USER x FOR CERTIFICATE y")]
	[InlineData("Create user for asymmetric key", "CREATE USER x FROM ASYMMETRIC KEY y")]
	[InlineData("Create user external", "CREATE USER x FROM EXTERNAL PROVIDER WITH OBJECT_ID = 'o'")]
	[InlineData("Create user without login", "CREATE USER x WITHOUT LOGIN WITH ALLOW_ENCRYPTED_VALUE_MODIFICATIONS = ON")]
	[InlineData("Create user with password", "CREATE USER x WITH PASSWORD = 'p', SID = 0x0A, TYPE = E")]
	[InlineData("Alter user", "ALTER USER x WITH NAME = n, DEFAULT_SCHEMA = NULL, LOGIN = l, PASSWORD = 'p' OLD_PASSWORD = 'q'")]
	[InlineData("Alter schema", "ALTER SCHEMA x TRANSFER dbo.t")]
	[InlineData("Alter schema class", "ALTER SCHEMA x TRANSFER XML SCHEMA COLLECTION::t")]
	[InlineData("Alter authorization", "ALTER AUTHORIZATION ON dbo.t TO y")]
	[InlineData("Alter authorization class", "ALTER AUTHORIZATION ON OBJECT::t TO SCHEMA OWNER")]
	[InlineData("Create schema", "CREATE SCHEMA t AUTHORIZATION y GRANT SELECT, UPDATE (a, b) ON dbo.t TO u, PUBLIC, NULL WITH GRANT OPTION AS keeper CREATE TABLE dbo.t (x INT)")]
	[InlineData("Create schema owner only", "CREATE SCHEMA AUTHORIZATION y")]
	public void Each_principal_statement_writes_its_kind(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(PrincipalTree(which)));
	}

	/// <summary>
	/// Each statement's tree, written and read back by TransactSqlParser, as the same tree.
	/// </summary>
	[Theory]
	[InlineData("Create role")]
	[InlineData("Create role alone")]
	[InlineData("Alter role add")]
	[InlineData("Alter role drop")]
	[InlineData("Alter role name")]
	[InlineData("Create server role")]
	[InlineData("Alter server role")]
	[InlineData("Create application role")]
	[InlineData("Alter application role")]
	[InlineData("Create login password")]
	[InlineData("Create login hash")]
	[InlineData("Create login windows")]
	[InlineData("Create login external")]
	[InlineData("Create login repeated")]
	[InlineData("Create login certificate")]
	[InlineData("Create login asymmetric key")]
	[InlineData("Alter login enable")]
	[InlineData("Alter login disable")]
	[InlineData("Alter login with")]
	[InlineData("Alter login old password")]
	[InlineData("Alter login hash")]
	[InlineData("Alter login add credential")]
	[InlineData("Alter login drop credential")]
	[InlineData("Create user")]
	[InlineData("Create user for login")]
	[InlineData("Create user from login")]
	[InlineData("Create user from certificate")]
	[InlineData("Create user for asymmetric key")]
	[InlineData("Create user external")]
	[InlineData("Create user without login")]
	[InlineData("Create user with password")]
	[InlineData("Alter user")]
	[InlineData("Alter schema")]
	[InlineData("Alter schema class")]
	[InlineData("Alter authorization")]
	[InlineData("Alter authorization class")]
	[InlineData("Create schema")]
	[InlineData("Create schema owner only")]
	public void Each_principal_statement_reads_back_as_itself(string which)
	{
		var tree = PrincipalTree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("CREATE ROLE with WITH ADMIN")]
	[InlineData("CREATE ROLE with an owner and WITH ADMIN")]
	[InlineData("a role owner that is a reserved word")]
	[InlineData("a default schema that is a reserved word")]
	[InlineData("a schema of no name and no owner")]
	[InlineData("a schema named by two parts")]
	[InlineData("a schema with a character set")]
	[InlineData("a schema with an element of another kind")]
	[InlineData("a procedure that is not the last element")]
	[InlineData("a trigger that is not the last element")]
	[InlineData("an application role of no setting")]
	[InlineData("an application role with a setting it has not")]
	[InlineData("an application role password that is a name")]
	[InlineData("a setting with no value")]
	[InlineData("a setting written without its =")]
	[InlineData("a setting with a value it has none of")]
	[InlineData("a setting in brackets")]
	[InlineData("a setting name with a blank")]
	[InlineData("a setting word with a blank")]
	[InlineData("a setting with an order")]
	[InlineData("a login password that is a name")]
	[InlineData("a login hash without HASHED")]
	[InlineData("MUST_CHANGE first without HASHED")]
	[InlineData("a Windows login with a SID")]
	[InlineData("a certificate login with settings")]
	[InlineData("a login that is a reserved word")]
	[InlineData("a login altered by no setting")]
	[InlineData("OLD_PASSWORD beside HASHED")]
	[InlineData("a password word twice")]
	[InlineData("a password hash without HASHED")]
	[InlineData("OLD_PASSWORD with a hash")]
	[InlineData("a password word that is none")]
	[InlineData("NO CREDENTIAL with a value")]
	[InlineData("a user language that is a string")]
	[InlineData("a created user with NULL for a schema")]
	[InlineData("a user object id that is a hash")]
	[InlineData("a user altered with HASHED")]
	[InlineData("a user altered by nothing")]
	[InlineData("a user name of two parts")]
	[InlineData("a securable with the class of nothing")]
	public void A_principal_statement_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// CREATE ROLE reads AUTHORIZATION owner: WITH ADMIN is the standard's.
			"CREATE ROLE with WITH ADMIN" => new Statement.CreateRole { Name = X, Admin = Grantor.CurrentUser },
			// AUTHORIZATION owner and WITH ADMIN are different constructs of two dialects, never written together.
			"CREATE ROLE with an owner and WITH ADMIN" => new Statement.CreateRole { Name = X, Admin = Grantor.CurrentUser, Authorization = Y },
			// The owner is an Identifier, which is no reserved word plain.
			"a role owner that is a reserved word" => new Statement.CreateRole { Name = X, Authorization = new Identifier("select") },
			// UserOption reads DEFAULT_SCHEMA = Identifier, never a reserved word plain.
			"a default schema that is a reserved word" => new Statement.CreateUser(X) { Options = [Opt("DEFAULT_SCHEMA", Word("select"))] },
			// PrincipalStatement reads a name or AUTHORIZATION, and CREATE SCHEMA alone is Msg 102.
			"a schema of no name and no owner" => new Statement.CreateSchema(),
			// The name is an Identifier: `CREATE SCHEMA a.b` is Msg 102 near '.'.
			"a schema named by two parts" => new Statement.CreateSchema { Name = Q },
			// The standard's DEFAULT CHARACTER SET is no part of T-SQL's schema.
			"a schema with a character set" => new Statement.CreateSchema { Name = T, DefaultCharacterSet = new CharacterSetName(new QualifiedName([X])) },
			// A procedure's body is a StatementList, which reads on: the GRANT after it would be part of it.
			"a procedure that is not the last element" => new Statement.CreateSchema { Name = T, Elements = [Procedure(), Tree("Grant")] },
			// A trigger's body is a StatementList, which reads on.
			"a trigger that is not the last element" => new Statement.CreateSchema { Name = T, Elements = [Trigger(), Tree("Grant")] },
			// SchemaElement reads a table, a routine or a permission statement.
			"a schema with an element of another kind" => new Statement.CreateSchema { Name = T, Elements = [new Statement.DropTable { Names = [T] }] },
			// CreateRoleOptions reads one setting at least.
			"an application role of no setting" => new Statement.CreateApplicationRole(X, []),
			// CreateRoleOption is PASSWORD or DEFAULT_SCHEMA: `WITH SID = 0x01` is Msg 102.
			"an application role with a setting it has not" => new Statement.CreateApplicationRole(X, [Opt("SID", Hex("01"))]),
			// CreateRoleOption reads PASSWORD = Quoted.
			"an application role password that is a name" => new Statement.CreateApplicationRole(X, [Opt("PASSWORD", Word("p"))]),
			// Every setting of the family reads `= value`: a name alone is Msg 102.
			"a setting with no value" => new Statement.CreateApplicationRole(X, [Opt("PASSWORD")]),
			// The settings are written with their =: the omitted form belongs to the generic option rule.
			"a setting written without its =" => new Statement.CreateApplicationRole(X, [Opt("PASSWORD", Str("p")) with { EqualsOmitted = true }]),
			// NO CREDENTIAL is written alone.
			"a setting with a value it has none of" => new Statement.AlterLogin(X, new AlterLoginAction.With([new Option(new Identifier("NO"), Str("p"), []) { Words = [new Identifier("CREDENTIAL")] }])),
			// A setting is read as words: `[PASSWORD]` is no key word.
			"a setting in brackets" => new Statement.CreateApplicationRole(X, [new Option(new Identifier("[PASSWORD]", IdentifierStyle.Bracketed), Str("p"), [])]),
			// A setting is words: `NO CREDENTIAL` as one word would read back as two.
			"a setting name with a blank" => new Statement.AlterLogin(X, new AlterLoginAction.With([new Option(new Identifier("NO CREDENTIAL"), null, [])])),
			// ENCRYPTION BY PASSWORD is three words, never one that holds a blank.
			"a setting word with a blank" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.WithPrivateKey([new Option(new Identifier("DECRYPTION"), Str("a"), []) { Words = [new Identifier("BY PASSWORD")] }])),
			// A setting is a name and a value: ORDER (…) is OPENROWSET's.
			"a setting with an order" => new Statement.CreateApplicationRole(X, [Opt("PASSWORD", Str("p")) with { Order = new RowOrder([], false) }]),
			// CreatePassword reads a string, or a hash in hex.
			"a login password that is a name" => new Statement.CreateLogin(X, new LoginSource.Password(Word("p"))),
			// CreatePassword: a hash in hex has to say it is one (CreateHashed).
			"a login hash without HASHED" => new Statement.CreateLogin(X, new LoginSource.Password(Hex("01"))),
			// The order of two words is only the order of two that are written.
			"MUST_CHANGE first without HASHED" => new Statement.CreateLogin(X, new LoginSource.Password(Str("p")) { MustChange = true, MustChangeFirst = true }),
			// LoginSource FROM WINDOWS reads LoginPlace only.
			"a Windows login with a SID" => new Statement.CreateLogin(X, new LoginSource.Windows()) { Options = [Opt("SID", Hex("01"))] },
			// KeyCredential is the one setting of a certificate's login.
			"a certificate login with settings" => new Statement.CreateLogin(X, new LoginSource.Certificate(Y, null)) { Options = [Opt("DEFAULT_DATABASE", Word("d"))] },
			// A login's name is an Identifier, never a reserved word plain.
			"a login that is a reserved word" => new Statement.CreateLogin(new Identifier("select"), new LoginSource.Windows()),
			// AlterLoginBody reads one setting at least after WITH.
			"a login altered by no setting" => new Statement.AlterLogin(X, new AlterLoginAction.With([])),
			// AlterPassword reads OLD_PASSWORD or the words, not both.
			"OLD_PASSWORD beside HASHED" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Str("p"), Opt("OLD_PASSWORD", Str("q")), Opt("HASHED"))])),
			// PasswordWords reads each word once.
			"a password word twice" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Str("p"), Opt("UNLOCK"), Opt("UNLOCK"))])),
			// HashedWords: a hash in hex has to say it is one.
			"a password hash without HASHED" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Hex("01"), Opt("UNLOCK"))])),
			// AlterPassword reads two strings around OLD_PASSWORD.
			"OLD_PASSWORD with a hash" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Hex("01"), Opt("OLD_PASSWORD", Str("q")))])),
			// PasswordWords: HASHED, MUST_CHANGE and UNLOCK only.
			"a password word that is none" => new Statement.AlterLogin(X, new AlterLoginAction.With([Opt("PASSWORD", Str("p"), Opt("SID"))])),
			"NO CREDENTIAL with a value" => new Statement.AlterLogin(X, new AlterLoginAction.With([new Option(new Identifier("NO"), Word("c"), []) { Words = [new Identifier("CREDENTIAL")] }])),
			// UserLanguage is a name or digits, not a string.
			"a user language that is a string" => new Statement.CreateUser(X) { Options = [Opt("DEFAULT_LANGUAGE", Str("l"))] },
			// UserOption reads DEFAULT_SCHEMA = Identifier: NULL is AlterUserOption's.
			"a created user with NULL for a schema" => new Statement.CreateUser(X) { Options = [Opt("DEFAULT_SCHEMA", new LiteralValue.Null())] },
			// UserOption reads OBJECT_ID = Quoted or a name.
			"a user object id that is a hash" => new Statement.CreateUser(X) { Options = [Opt("OBJECT_ID", Hex("01"))] },
			// AlterUserOption reads PASSWORD = Quoted [OLD_PASSWORD = Quoted] and nothing else.
			"a user altered with HASHED" => new Statement.AlterUser(X, [Opt("PASSWORD", Str("p"), Opt("HASHED"))]),
			// AlterUserBody reads one setting at least.
			"a user altered by nothing" => new Statement.AlterUser(X, []),
			// A user's name is one part.
			"a user name of two parts" => new Statement.CreateUser(new Identifier("a.b", IdentifierStyle.Regular)) { Source = null, Options = [Opt("DEFAULT_SCHEMA", new Expression.Reference(Q))] },
			// SecurableClass has no member of the number 99.
			"a securable with the class of nothing" => new Statement.AlterSchema(X, T) { Class = (SecurableClass)99 },
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}

	static LiteralValue.String Quoted(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static KeyEncryptor.Password PasswordOf(string text)
	{
		return new KeyEncryptor.Password(Quoted(text));
	}

	static PolicyFunction Function(string name, params Expression[] arguments)
	{
		return new PolicyFunction(new QualifiedName([new Identifier("dbo"), new Identifier(name)]), [.. arguments]);
	}

	/// <summary>A tree for each statement of the keys, credentials, policies and signatures group.</summary>
	static Statement KeyTree(string which)
	{
		return which switch
		{
			"Create asymmetric key" => new Statement.CreateAsymmetricKey(X) { Authorization = Y, Options = [Opt("ALGORITHM", Word("RSA_2048"))], EncryptedBy = PasswordOf("p") },
			"Create asymmetric key alone" => new Statement.CreateAsymmetricKey(X),
			"Create asymmetric key provider" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.Provider(Y), Options = [Opt("ALGORITHM", Word("RC2")), Opt("PROVIDER_KEY_NAME", Str("kn")), Opt("CREATION_DISPOSITION", Word("CREATE_NEW"))] },
			"Create asymmetric key file" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.File(Quoted("f")) },
			"Create asymmetric key executable" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.File(Quoted("f")) { Executable = true }, EncryptedBy = PasswordOf("p") },
			"Create asymmetric key assembly" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.Assembly(Q) },
			"Alter asymmetric key remove" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.RemovePrivateKey()),
			"Alter asymmetric key private" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.WithPrivateKey([new Option(new Identifier("DECRYPTION"), Str("a"), []) { Words = [new Identifier("BY"), new Identifier("PASSWORD")] }, new Option(new Identifier("ENCRYPTION"), Str("b"), []) { Words = [new Identifier("BY"), new Identifier("PASSWORD")] }])),
			"Alter asymmetric key attested" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.AttestedBy(Quoted("a"))),
			"Alter asymmetric key remove attested" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.RemoveAttestedOption()),
			"Create symmetric key" => new Statement.CreateSymmetricKey(X)
			{
				Authorization = Y,
				Options = [Opt("KEY_SOURCE", Str("ks")), Opt("ALGORITHM", Word("AES_256"))],
				EncryptedBy = [new KeyEncryptor.Certificate(T), PasswordOf("p"), new KeyEncryptor.SymmetricKey(Q), new KeyEncryptor.AsymmetricKey(T)],
			},
			"Create symmetric key provider" => new Statement.CreateSymmetricKey(X) { Provider = Y, Options = [Opt("CREATION_DISPOSITION", Word("OPEN_EXISTING"))], EncryptedBy = [new KeyEncryptor.Certificate(T)] },
			"Create symmetric key provider alone" => new Statement.CreateSymmetricKey(X) { Provider = Y },
			"Alter symmetric key add" => new Statement.AlterSymmetricKey(X, new SymmetricKeyChange.AddEncryption([PasswordOf("p"), new KeyEncryptor.SymmetricKey(T)])),
			"Alter symmetric key drop" => new Statement.AlterSymmetricKey(X, new SymmetricKeyChange.DropEncryption([new KeyEncryptor.AsymmetricKey(T)])),
			"Create certificate" => new Statement.CreateCertificate(X) { Authorization = Y, EncryptedBy = PasswordOf("p"), Options = [Opt("SUBJECT", Str("s")), Opt("START_DATE", Str("2020")), Opt("EXPIRY_DATE", Str("2030"))], ActiveForBeginDialog = true },
			"Create certificate assembly" => new Statement.CreateCertificate(X) { Source = new CertificateSource.Assembly(T), ActiveForBeginDialog = false },
			"Create certificate file" => new Statement.CreateCertificate(X)
			{
				Source = new CertificateSource.File(Quoted("f")) { Executable = true, Format = Quoted("PFX"), PrivateKey = [Opt("FILE", Str("k")), new Option(new Identifier("DECRYPTION"), Str("p"), []) { Words = [new Identifier("BY"), new Identifier("PASSWORD")] }] },
			},
			"Create certificate file alone" => new Statement.CreateCertificate(X) { Source = new CertificateSource.File(Quoted("f")) },
			"Create certificate binary" => new Statement.CreateCertificate(X) { Source = new CertificateSource.Binary(new LiteralValue.Binary("01") { ZeroX = true }) { PrivateKey = [Opt("BINARY", Hex("02"))] } },
			"Alter certificate remove" => new Statement.AlterCertificate(X, new CertificateChange.RemovePrivateKey()),
			"Alter certificate private" => new Statement.AlterCertificate(X, new CertificateChange.WithPrivateKey([Opt("FILE", Str("k"))])),
			"Alter certificate active" => new Statement.AlterCertificate(X, new CertificateChange.ActiveForBeginDialog(false)),
			"Alter certificate attested" => new Statement.AlterCertificate(X, new CertificateChange.AttestedBy(Quoted("a"))),
			"Alter certificate remove attested" => new Statement.AlterCertificate(X, new CertificateChange.RemoveAttestedOption()),
			"Create master key" => new Statement.CreateMasterKey { EncryptedBy = PasswordOf("p") },
			"Create master key alone" => new Statement.CreateMasterKey(),
			"Alter master key regenerate" => new Statement.AlterMasterKey(new MasterKeyChange.Regenerate(PasswordOf("p")) { Force = true }),
			"Alter master key add" => new Statement.AlterMasterKey(new MasterKeyChange.AddEncryption(new KeyEncryptor.ServiceMasterKey())),
			"Alter master key drop" => new Statement.AlterMasterKey(new MasterKeyChange.DropEncryption(PasswordOf("p"))),
			"Alter service master key regenerate" => new Statement.AlterServiceMasterKey(new ServiceMasterKeyChange.Regenerate { Force = true }),
			"Alter service master key old" => new Statement.AlterServiceMasterKey(new ServiceMasterKeyChange.OldAccount(Quoted("a"), Quoted("p"))),
			"Alter service master key new" => new Statement.AlterServiceMasterKey(new ServiceMasterKeyChange.NewAccount(Quoted("a"), Quoted("p"))),
			"Create database encryption key" => new Statement.CreateDatabaseEncryptionKey(DatabaseKeyAlgorithm.TripleDes3Key, new KeyEncryptor.Certificate(T)),
			"Create database encryption key asymmetric" => new Statement.CreateDatabaseEncryptionKey(DatabaseKeyAlgorithm.Aes128, new KeyEncryptor.AsymmetricKey(T)),
			"Alter database encryption key regenerate" => new Statement.AlterDatabaseEncryptionKey(new DatabaseEncryptionKeyChange.Regenerate(DatabaseKeyAlgorithm.Aes256)),
			"Alter database encryption key encrypted" => new Statement.AlterDatabaseEncryptionKey(new DatabaseEncryptionKeyChange.EncryptedBy(new KeyEncryptor.Certificate(T))),
			"Create column encryption key" => new Statement.CreateColumnEncryptionKey(X, [new ColumnKeyValue([Opt("COLUMN_MASTER_KEY", Word("m")), Opt("ALGORITHM", Str("RSA_OAEP")), Opt("ENCRYPTED_VALUE", Hex("01"))]), new ColumnKeyValue([Opt("ALGORITHM", Str("a")), Opt("ENCRYPTED_VALUE", Hex("02")), Opt("COLUMN_MASTER_KEY", Word("n"))])]),
			"Alter column encryption key add" => new Statement.AlterColumnEncryptionKey(X, new ColumnKeyChange.AddValue(new ColumnKeyValue([Opt("COLUMN_MASTER_KEY", Word("m")), Opt("ALGORITHM", Str("a")), Opt("ENCRYPTED_VALUE", Hex("01"))]))),
			"Alter column encryption key drop" => new Statement.AlterColumnEncryptionKey(X, new ColumnKeyChange.DropValue(new ColumnKeyValue([Opt("COLUMN_MASTER_KEY", Word("m"))]))),
			"Create column master key" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_PATH", Str("p")), Opt("KEY_STORE_PROVIDER_NAME", Str("s")), new Option(new Identifier("ENCLAVE_COMPUTATIONS"), null, [Opt("SIGNATURE", Hex("01"))])]),
			"Create column master key two" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_STORE_PROVIDER_NAME", Str("s")), Opt("KEY_PATH", Str("p"))]),
			"Create credential" => new Statement.CreateCredential(X, Quoted("i")) { Secret = Quoted("s"), Provider = Y },
			"Create credential alone" => new Statement.CreateCredential(X, Quoted("i")),
			"Alter credential" => new Statement.AlterCredential(X, Quoted("i")) { Secret = Quoted("s") },
			"Create database scoped credential" => new Statement.CreateDatabaseScopedCredential(X, Quoted("i")) { Secret = Quoted("s") },
			"Alter database scoped credential" => new Statement.AlterDatabaseScopedCredential(X, Quoted("i")),
			"Create cryptographic provider" => new Statement.CreateCryptographicProvider(X, Quoted("f")),
			"Alter cryptographic provider enable" => new Statement.AlterCryptographicProvider(X, new CryptographicProviderChange.Enable()),
			"Alter cryptographic provider disable" => new Statement.AlterCryptographicProvider(X, new CryptographicProviderChange.Disable()),
			"Alter cryptographic provider file" => new Statement.AlterCryptographicProvider(X, new CryptographicProviderChange.FromFile(Quoted("f"))),
			"Open symmetric key certificate" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.Certificate(T) { WithPassword = Quoted("p") }),
			"Open symmetric key asymmetric key" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.AsymmetricKey(T) { WithPassword = Quoted("p") }),
			"Open symmetric key symmetric key" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.SymmetricKey(T)),
			"Open symmetric key password" => new Statement.OpenSymmetricKey(X, PasswordOf("p")),
			"Open master key" => new Statement.OpenMasterKey(PasswordOf("p")),
			"Close symmetric key" => new Statement.CloseSymmetricKey(X),
			"Close all symmetric keys" => new Statement.CloseAllSymmetricKeys(),
			"Close master key" => new Statement.CloseMasterKey(),
			"Create security policy" => new Statement.CreateSecurityPolicy(Q)
			{
				Predicates = [new PolicyPredicate.AddFilter(Function("f", Word("c")), T), new PolicyPredicate.AddBlock(Function("g"), Q, BlockOperation.AfterInsert), new PolicyPredicate.AlterBlock(Function("g", Digits("1")), Q, null)],
				Options = [Opt("STATE", Word("ON")), Opt("SCHEMABINDING", Word("OFF"))],
				NotForReplication = true,
			},
			"Create security policy alone" => new Statement.CreateSecurityPolicy(T),
			"Alter security policy predicates" => new Statement.AlterSecurityPolicy(Q, new SecurityPolicyChange.Predicates([new PolicyPredicate.AlterFilter(Function("f", Word("c")), T), new PolicyPredicate.DropFilter(T), new PolicyPredicate.DropBlock(Q, BlockOperation.BeforeDelete), new PolicyPredicate.DropBlock(T, null)])),
			"Alter security policy state" => new Statement.AlterSecurityPolicy(T, new SecurityPolicyChange.State(true)),
			"Alter security policy add" => new Statement.AlterSecurityPolicy(T, new SecurityPolicyChange.AddNotForReplication()),
			"Alter security policy drop" => new Statement.AlterSecurityPolicy(T, new SecurityPolicyChange.DropNotForReplication()),
			"Add sensitivity classification" => new Statement.AddSensitivityClassification([Q, new QualifiedName([new Identifier("db"), new Identifier("dbo"), new Identifier("t")])], [Opt("LABEL", Str("l")), Opt("RANK", Word("HIGH"))]),
			"Add signature" => new Statement.AddSignature(false, Q, [new SignedBy.Certificate(T) { Proof = new SignatureProof.Password(Quoted("p")) }, new SignedBy.AsymmetricKey(T) { Proof = new SignatureProof.Signature(new LiteralValue.Binary("01") { ZeroX = true }) }, new SignedBy.SymmetricKey(Y), new SignedBy.Password(Quoted("x"))]),
			"Add counter signature" => new Statement.AddSignature(true, T, [new SignedBy.Certificate(One(Y))]) { Class = SecurableClass.Object },
			"Drop signature proof" => new Statement.DropSignature(false, T, [new SignedBy.AsymmetricKey(One(Y)) { Proof = new SignatureProof.Password(Quoted("p")) }, new SignedBy.SymmetricKey(X)]),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	/// <summary>Each statement written as the text of its kind.</summary>
	[Theory]
	[InlineData("Create asymmetric key", "CREATE ASYMMETRIC KEY x AUTHORIZATION y WITH ALGORITHM = RSA_2048 ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("Create asymmetric key alone", "CREATE ASYMMETRIC KEY x")]
	[InlineData("Create asymmetric key provider", "CREATE ASYMMETRIC KEY x FROM PROVIDER y WITH ALGORITHM = RC2, PROVIDER_KEY_NAME = 'kn', CREATION_DISPOSITION = CREATE_NEW")]
	[InlineData("Create asymmetric key file", "CREATE ASYMMETRIC KEY x FROM FILE = 'f'")]
	[InlineData("Create asymmetric key executable", "CREATE ASYMMETRIC KEY x FROM EXECUTABLE FILE = 'f' ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("Create asymmetric key assembly", "CREATE ASYMMETRIC KEY x FROM ASSEMBLY dbo.t")]
	[InlineData("Alter asymmetric key remove", "ALTER ASYMMETRIC KEY x REMOVE PRIVATE KEY")]
	[InlineData("Alter asymmetric key private", "ALTER ASYMMETRIC KEY x WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'a', ENCRYPTION BY PASSWORD = 'b')")]
	[InlineData("Alter asymmetric key attested", "ALTER ASYMMETRIC KEY x ATTESTED BY 'a'")]
	[InlineData("Alter asymmetric key remove attested", "ALTER ASYMMETRIC KEY x REMOVE ATTESTED OPTION")]
	[InlineData("Create symmetric key", "CREATE SYMMETRIC KEY x AUTHORIZATION y WITH KEY_SOURCE = 'ks', ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE t, PASSWORD = 'p', SYMMETRIC KEY dbo.t, ASYMMETRIC KEY t")]
	[InlineData("Create symmetric key provider", "CREATE SYMMETRIC KEY x FROM PROVIDER y WITH CREATION_DISPOSITION = OPEN_EXISTING ENCRYPTION BY CERTIFICATE t")]
	[InlineData("Create symmetric key provider alone", "CREATE SYMMETRIC KEY x FROM PROVIDER y")]
	[InlineData("Alter symmetric key add", "ALTER SYMMETRIC KEY x ADD ENCRYPTION BY PASSWORD = 'p', SYMMETRIC KEY t")]
	[InlineData("Alter symmetric key drop", "ALTER SYMMETRIC KEY x DROP ENCRYPTION BY ASYMMETRIC KEY t")]
	[InlineData("Create certificate", "CREATE CERTIFICATE x AUTHORIZATION y ENCRYPTION BY PASSWORD = 'p' WITH SUBJECT = 's', START_DATE = '2020', EXPIRY_DATE = '2030' ACTIVE FOR BEGIN_DIALOG = ON")]
	[InlineData("Create certificate assembly", "CREATE CERTIFICATE x FROM ASSEMBLY t ACTIVE FOR BEGIN_DIALOG = OFF")]
	[InlineData("Create certificate file", "CREATE CERTIFICATE x FROM EXECUTABLE FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p')")]
	[InlineData("Create certificate file alone", "CREATE CERTIFICATE x FROM FILE = 'f'")]
	[InlineData("Create certificate binary", "CREATE CERTIFICATE x FROM BINARY = 0x01 WITH PRIVATE KEY (BINARY = 0x02)")]
	[InlineData("Alter certificate remove", "ALTER CERTIFICATE x REMOVE PRIVATE KEY")]
	[InlineData("Alter certificate private", "ALTER CERTIFICATE x WITH PRIVATE KEY (FILE = 'k')")]
	[InlineData("Alter certificate active", "ALTER CERTIFICATE x WITH ACTIVE FOR BEGIN_DIALOG = OFF")]
	[InlineData("Alter certificate attested", "ALTER CERTIFICATE x ATTESTED BY 'a'")]
	[InlineData("Alter certificate remove attested", "ALTER CERTIFICATE x REMOVE ATTESTED OPTION")]
	[InlineData("Create master key", "CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("Create master key alone", "CREATE MASTER KEY")]
	[InlineData("Alter master key regenerate", "ALTER MASTER KEY FORCE REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("Alter master key add", "ALTER MASTER KEY ADD ENCRYPTION BY SERVICE MASTER KEY")]
	[InlineData("Alter master key drop", "ALTER MASTER KEY DROP ENCRYPTION BY PASSWORD = 'p'")]
	[InlineData("Alter service master key regenerate", "ALTER SERVICE MASTER KEY FORCE REGENERATE")]
	[InlineData("Alter service master key old", "ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p'")]
	[InlineData("Alter service master key new", "ALTER SERVICE MASTER KEY WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p'")]
	[InlineData("Create database encryption key", "CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = TRIPLE_DES_3KEY ENCRYPTION BY SERVER CERTIFICATE t")]
	[InlineData("Create database encryption key asymmetric", "CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_128 ENCRYPTION BY SERVER ASYMMETRIC KEY t")]
	[InlineData("Alter database encryption key regenerate", "ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_256")]
	[InlineData("Alter database encryption key encrypted", "ALTER DATABASE ENCRYPTION KEY ENCRYPTION BY SERVER CERTIFICATE t")]
	[InlineData("Create column encryption key", "CREATE COLUMN ENCRYPTION KEY x WITH VALUES (COLUMN_MASTER_KEY = m, ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = 0x01), (ALGORITHM = 'a', ENCRYPTED_VALUE = 0x02, COLUMN_MASTER_KEY = n)")]
	[InlineData("Alter column encryption key add", "ALTER COLUMN ENCRYPTION KEY x ADD VALUE (COLUMN_MASTER_KEY = m, ALGORITHM = 'a', ENCRYPTED_VALUE = 0x01)")]
	[InlineData("Alter column encryption key drop", "ALTER COLUMN ENCRYPTION KEY x DROP VALUE (COLUMN_MASTER_KEY = m)")]
	[InlineData("Create column master key", "CREATE COLUMN MASTER KEY x WITH (KEY_PATH = 'p', KEY_STORE_PROVIDER_NAME = 's', ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01))")]
	[InlineData("Create column master key two", "CREATE COLUMN MASTER KEY x WITH (KEY_STORE_PROVIDER_NAME = 's', KEY_PATH = 'p')")]
	[InlineData("Create credential", "CREATE CREDENTIAL x WITH IDENTITY = 'i', SECRET = 's' FOR CRYPTOGRAPHIC PROVIDER y")]
	[InlineData("Create credential alone", "CREATE CREDENTIAL x WITH IDENTITY = 'i'")]
	[InlineData("Alter credential", "ALTER CREDENTIAL x WITH IDENTITY = 'i', SECRET = 's'")]
	[InlineData("Create database scoped credential", "CREATE DATABASE SCOPED CREDENTIAL x WITH IDENTITY = 'i', SECRET = 's'")]
	[InlineData("Alter database scoped credential", "ALTER DATABASE SCOPED CREDENTIAL x WITH IDENTITY = 'i'")]
	[InlineData("Create cryptographic provider", "CREATE CRYPTOGRAPHIC PROVIDER x FROM FILE = 'f'")]
	[InlineData("Alter cryptographic provider enable", "ALTER CRYPTOGRAPHIC PROVIDER x ENABLE")]
	[InlineData("Alter cryptographic provider disable", "ALTER CRYPTOGRAPHIC PROVIDER x DISABLE")]
	[InlineData("Alter cryptographic provider file", "ALTER CRYPTOGRAPHIC PROVIDER x FROM FILE = 'f'")]
	[InlineData("Open symmetric key certificate", "OPEN SYMMETRIC KEY x DECRYPTION BY CERTIFICATE t WITH PASSWORD = 'p'")]
	[InlineData("Open symmetric key asymmetric key", "OPEN SYMMETRIC KEY x DECRYPTION BY ASYMMETRIC KEY t WITH PASSWORD = 'p'")]
	[InlineData("Open symmetric key symmetric key", "OPEN SYMMETRIC KEY x DECRYPTION BY SYMMETRIC KEY t")]
	[InlineData("Open symmetric key password", "OPEN SYMMETRIC KEY x DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("Open master key", "OPEN MASTER KEY DECRYPTION BY PASSWORD = 'p'")]
	[InlineData("Close symmetric key", "CLOSE SYMMETRIC KEY x")]
	[InlineData("Close all symmetric keys", "CLOSE ALL SYMMETRIC KEYS")]
	[InlineData("Close master key", "CLOSE MASTER KEY")]
	[InlineData("Create security policy", "CREATE SECURITY POLICY dbo.t ADD FILTER PREDICATE dbo.f(c) ON t, ADD BLOCK PREDICATE dbo.g() ON dbo.t AFTER INSERT, ALTER BLOCK PREDICATE dbo.g(1) ON dbo.t WITH (STATE = ON, SCHEMABINDING = OFF) NOT FOR REPLICATION")]
	[InlineData("Create security policy alone", "CREATE SECURITY POLICY t")]
	[InlineData("Alter security policy predicates", "ALTER SECURITY POLICY dbo.t ALTER FILTER PREDICATE dbo.f(c) ON t, DROP FILTER PREDICATE ON t, DROP BLOCK PREDICATE ON dbo.t BEFORE DELETE, DROP BLOCK PREDICATE ON t")]
	[InlineData("Alter security policy state", "ALTER SECURITY POLICY t WITH (STATE = ON)")]
	[InlineData("Alter security policy add", "ALTER SECURITY POLICY t ADD NOT FOR REPLICATION")]
	[InlineData("Alter security policy drop", "ALTER SECURITY POLICY t DROP NOT FOR REPLICATION")]
	[InlineData("Add sensitivity classification", "ADD SENSITIVITY CLASSIFICATION TO dbo.t, db.dbo.t WITH (LABEL = 'l', RANK = HIGH)")]
	[InlineData("Add signature", "ADD SIGNATURE TO dbo.t BY CERTIFICATE t WITH PASSWORD = 'p', ASYMMETRIC KEY t WITH SIGNATURE = 0x01, SYMMETRIC KEY y, PASSWORD = 'x'")]
	[InlineData("Add counter signature", "ADD COUNTER SIGNATURE TO OBJECT::t BY CERTIFICATE y")]
	[InlineData("Drop signature proof", "DROP SIGNATURE FROM t BY ASYMMETRIC KEY y WITH PASSWORD = 'p', SYMMETRIC KEY x")]
	public void Each_key_statement_writes_its_kind(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(KeyTree(which)));
	}

	/// <summary>
	/// Each statement's tree, written and read back by TransactSqlParser, as the same tree.
	/// </summary>
	[Theory]
	[InlineData("Create asymmetric key")]
	[InlineData("Create asymmetric key alone")]
	[InlineData("Create asymmetric key provider")]
	[InlineData("Create asymmetric key file")]
	[InlineData("Create asymmetric key executable")]
	[InlineData("Create asymmetric key assembly")]
	[InlineData("Alter asymmetric key remove")]
	[InlineData("Alter asymmetric key private")]
	[InlineData("Alter asymmetric key attested")]
	[InlineData("Alter asymmetric key remove attested")]
	[InlineData("Create symmetric key")]
	[InlineData("Create symmetric key provider")]
	[InlineData("Create symmetric key provider alone")]
	[InlineData("Alter symmetric key add")]
	[InlineData("Alter symmetric key drop")]
	[InlineData("Create certificate")]
	[InlineData("Create certificate assembly")]
	[InlineData("Create certificate file")]
	[InlineData("Create certificate file alone")]
	[InlineData("Create certificate binary")]
	[InlineData("Alter certificate remove")]
	[InlineData("Alter certificate private")]
	[InlineData("Alter certificate active")]
	[InlineData("Alter certificate attested")]
	[InlineData("Alter certificate remove attested")]
	[InlineData("Create master key")]
	[InlineData("Create master key alone")]
	[InlineData("Alter master key regenerate")]
	[InlineData("Alter master key add")]
	[InlineData("Alter master key drop")]
	[InlineData("Alter service master key regenerate")]
	[InlineData("Alter service master key old")]
	[InlineData("Alter service master key new")]
	[InlineData("Create database encryption key")]
	[InlineData("Create database encryption key asymmetric")]
	[InlineData("Alter database encryption key regenerate")]
	[InlineData("Alter database encryption key encrypted")]
	[InlineData("Create column encryption key")]
	[InlineData("Alter column encryption key add")]
	[InlineData("Alter column encryption key drop")]
	[InlineData("Create column master key")]
	[InlineData("Create column master key two")]
	[InlineData("Create credential")]
	[InlineData("Create credential alone")]
	[InlineData("Alter credential")]
	[InlineData("Create database scoped credential")]
	[InlineData("Alter database scoped credential")]
	[InlineData("Create cryptographic provider")]
	[InlineData("Alter cryptographic provider enable")]
	[InlineData("Alter cryptographic provider disable")]
	[InlineData("Alter cryptographic provider file")]
	[InlineData("Open symmetric key certificate")]
	[InlineData("Open symmetric key asymmetric key")]
	[InlineData("Open symmetric key symmetric key")]
	[InlineData("Open symmetric key password")]
	[InlineData("Open master key")]
	[InlineData("Close symmetric key")]
	[InlineData("Close all symmetric keys")]
	[InlineData("Close master key")]
	[InlineData("Create security policy")]
	[InlineData("Create security policy alone")]
	[InlineData("Alter security policy predicates")]
	[InlineData("Alter security policy state")]
	[InlineData("Alter security policy add")]
	[InlineData("Alter security policy drop")]
	[InlineData("Add sensitivity classification")]
	[InlineData("Add signature")]
	[InlineData("Add counter signature")]
	[InlineData("Drop signature proof")]
	public void Each_key_statement_reads_back_as_itself(string which)
	{
		var tree = KeyTree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, TransactSqlParser.Located.ParseStatement(text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("an asymmetric key from a file with settings")]
	[InlineData("an asymmetric key made here with a provider setting")]
	[InlineData("an asymmetric key made here with two algorithms")]
	[InlineData("an algorithm the grammar has none of")]
	[InlineData("a path that is a Unicode string")]
	[InlineData("a private key of no setting")]
	[InlineData("an asymmetric key private key file")]
	[InlineData("a symmetric key made here with no setting")]
	[InlineData("a symmetric key made here with no encryptor")]
	[InlineData("a symmetric key setting it has not")]
	[InlineData("a symmetric key locked by the service master key")]
	[InlineData("a symmetric key locked by a certificate and its password")]
	[InlineData("a symmetric key altered by none")]
	[InlineData("a certificate made here with no setting")]
	[InlineData("a certificate setting it has not")]
	[InlineData("a certificate from an assembly with settings")]
	[InlineData("a certificate from an assembly locked by a password")]
	[InlineData("a certificate format without a private key")]
	[InlineData("a certificate private key setting it has not")]
	[InlineData("a certificate binary written X'…'")]
	[InlineData("a certificate private key of no setting")]
	[InlineData("a master key added by a certificate")]
	[InlineData("a database key locked by a password")]
	[InlineData("a database key changed to a symmetric key")]
	[InlineData("a database key locked by a certificate and its password")]
	[InlineData("a database key algorithm that is none")]
	[InlineData("a column key of no value")]
	[InlineData("a column key value of two settings")]
	[InlineData("a column key value setting it has not")]
	[InlineData("a column key value dropped by two settings")]
	[InlineData("a column master key of one setting")]
	[InlineData("a column master key path twice")]
	[InlineData("an enclave that is not the third")]
	[InlineData("an enclave with a name for its signature")]
	[InlineData("a credential identity that is a Unicode string")]
	[InlineData("a cryptographic provider file that is a Unicode string")]
	[InlineData("a key opened by a symmetric key of two parts")]
	[InlineData("a key opened by a certificate of two parts")]
	[InlineData("a key opened by the service master key")]
	[InlineData("a policy function with a variable")]
	[InlineData("a policy table named with an @")]
	[InlineData("a policy function named with an @")]
	[InlineData("a policy setting it has not")]
	[InlineData("a policy setting that is not ON or OFF")]
	[InlineData("an altered policy of no predicate")]
	[InlineData("a classification of no column")]
	[InlineData("a classified column of one part")]
	[InlineData("a classification of no option")]
	[InlineData("a classification option twice")]
	[InlineData("a classification rank that is none")]
	[InlineData("a classification label that is a name")]
	[InlineData("a signature by nothing")]
	[InlineData("a signature to a module of another class")]
	[InlineData("a signature by a certificate of two parts")]
	[InlineData("a signature by a key of two parts")]
	[InlineData("a signature proof that is a binary string written X'…'")]
	[InlineData("a signature password that is a Unicode string")]
	public void A_key_statement_with_no_text_is_refused(string which)
	{
		ISqlNode node = which switch
		{
			// AsymmetricBody: FROM AsymmetricSource takes no settings.
			"an asymmetric key from a file with settings" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.File(Quoted("f")), Options = [Opt("ALGORITHM", Word("RC2"))] },
			// AsymmetricBody: a key made here takes AlgorithmSetting only.
			"an asymmetric key made here with a provider setting" => new Statement.CreateAsymmetricKey(X) { Options = [Opt("PROVIDER_KEY_NAME", Str("n"))] },
			// AsymmetricBody: a key made here takes an algorithm at most.
			"an asymmetric key made here with two algorithms" => new Statement.CreateAsymmetricKey(X) { Options = [Opt("ALGORITHM", Word("RC2")), Opt("ALGORITHM", Word("DES"))] },
			// KeyAlgorithm is a closed list: `ALGORITHM = FOO` is Msg 102.
			"an algorithm the grammar has none of" => new Statement.CreateAsymmetricKey(X) { Options = [Opt("ALGORITHM", Word("FOO"))] },
			// Quoted reads a character or a national string.
			"a path that is a Unicode string" => new Statement.CreateAsymmetricKey(X) { Source = new AsymmetricKeySource.File(new LiteralValue.String("'f'", StringLiteralKind.Unicode)) },
			// KeyPasswordSetting is read once at least inside the brackets.
			"a private key of no setting" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.WithPrivateKey([])),
			// ALTER ASYMMETRIC KEY reads KeyPassword in the brackets, not FILE.
			"an asymmetric key private key file" => new Statement.AlterAsymmetricKey(X, new AsymmetricKeyChange.WithPrivateKey([Opt("FILE", Str("k"))])),
			// SymmetricBody: WITH reads one setting at least for a key made here.
			"a symmetric key made here with no setting" => new Statement.CreateSymmetricKey(X) { EncryptedBy = [PasswordOf("p")] },
			// SymmetricBody: a key made here is locked by one thing at least.
			"a symmetric key made here with no encryptor" => new Statement.CreateSymmetricKey(X) { Options = [Opt("ALGORITHM", Word("DES"))] },
			// SymmetricOption has no SUBJECT.
			"a symmetric key setting it has not" => new Statement.CreateSymmetricKey(X) { Options = [Opt("SUBJECT", Str("s"))], EncryptedBy = [PasswordOf("p")] },
			// KeyEncryptor has no SERVICE MASTER KEY: Msg 102.
			"a symmetric key locked by the service master key" => new Statement.CreateSymmetricKey(X) { Options = [Opt("ALGORITHM", Word("DES"))], EncryptedBy = [new KeyEncryptor.ServiceMasterKey()] },
			// KeyEncryptor reads WITH PASSWORD nowhere: KeyDecryptor does.
			"a symmetric key locked by a certificate and its password" => new Statement.CreateSymmetricKey(X) { Options = [Opt("ALGORITHM", Word("DES"))], EncryptedBy = [new KeyEncryptor.Certificate(T) { WithPassword = Quoted("p") }] },
			// ALTER SYMMETRIC KEY reads one encryptor at least.
			"a symmetric key altered by none" => new Statement.AlterSymmetricKey(X, new SymmetricKeyChange.AddEncryption([])),
			// CertificateBody: WITH reads one setting at least.
			"a certificate made here with no setting" => new Statement.CreateCertificate(X),
			// CertificateOption is SUBJECT, START_DATE and EXPIRY_DATE.
			"a certificate setting it has not" => new Statement.CreateCertificate(X) { Options = [Opt("ALGORITHM", Word("DES"))] },
			// CertificateSource reads no settings of its own.
			"a certificate from an assembly with settings" => new Statement.CreateCertificate(X) { Source = new CertificateSource.Assembly(T), Options = [Opt("SUBJECT", Str("s"))] },
			// CertificateBody reads ENCRYPTION BY only before WITH settings.
			"a certificate from an assembly locked by a password" => new Statement.CreateCertificate(X) { Source = new CertificateSource.Assembly(T), EncryptedBy = PasswordOf("p") },
			// CertificateSource reads FORMAT only before PRIVATE KEY.
			"a certificate format without a private key" => new Statement.CreateCertificate(X) { Source = new CertificateSource.File(Quoted("f")) { Format = Quoted("PFX") } },
			// PrivateKeyOption is FILE, BINARY and the two passwords.
			"a certificate private key setting it has not" => new Statement.CreateCertificate(X) { Source = new CertificateSource.File(Quoted("f")) { PrivateKey = [Opt("SUBJECT", Str("s"))] } },
			// TSql.BinaryLiteral reads 0x….
			"a certificate binary written X'…'" => new Statement.CreateCertificate(X) { Source = new CertificateSource.Binary(new LiteralValue.Binary("'01'")) },
			// PrivateKeyList reads one setting at least.
			"a certificate private key of no setting" => new Statement.AlterCertificate(X, new CertificateChange.WithPrivateKey([])),
			// MasterKeyChange adds a service master key or a password.
			"a master key added by a certificate" => new Statement.AlterMasterKey(new MasterKeyChange.AddEncryption(new KeyEncryptor.Certificate(T))),
			// ServerEncryption reads a certificate or an asymmetric key, at both of its places: CREATE's and ALTER's.
			"a database key locked by a password" => new Statement.CreateDatabaseEncryptionKey(DatabaseKeyAlgorithm.Aes128, PasswordOf("p")),
			"a database key changed to a symmetric key" => new Statement.AlterDatabaseEncryptionKey(new DatabaseEncryptionKeyChange.EncryptedBy(new KeyEncryptor.SymmetricKey(T))),
			// ServerEncryption reads no WITH PASSWORD: KeyDecryptor does.
			"a database key locked by a certificate and its password" => new Statement.CreateDatabaseEncryptionKey(DatabaseKeyAlgorithm.Aes128, new KeyEncryptor.Certificate(T) { WithPassword = Quoted("p") }),
			// DatabaseKeyAlgorithm is a closed list of four.
			"a database key algorithm that is none" => new Statement.CreateDatabaseEncryptionKey((DatabaseKeyAlgorithm)9, new KeyEncryptor.Certificate(T)),
			// CREATE COLUMN ENCRYPTION KEY reads one value at least.
			"a column key of no value" => new Statement.CreateColumnEncryptionKey(X, []),
			// ColumnKeyValue reads three settings in the brackets.
			"a column key value of two settings" => new Statement.CreateColumnEncryptionKey(X, [new ColumnKeyValue([Opt("COLUMN_MASTER_KEY", Word("m")), Opt("ALGORITHM", Str("a"))])]),
			// ColumnKeyPart is the master key, the algorithm and the encrypted value.
			"a column key value setting it has not" => new Statement.AlterColumnEncryptionKey(X, new ColumnKeyChange.DropValue(new ColumnKeyValue([Opt("SUBJECT", Str("a"))]))),
			// DROP VALUE reads one ColumnKeySetting in its brackets.
			"a column key value dropped by two settings" => new Statement.AlterColumnEncryptionKey(X, new ColumnKeyChange.DropValue(new ColumnKeyValue([Opt("COLUMN_MASTER_KEY", Word("m")), Opt("ALGORITHM", Str("a"))]))),
			// MasterKeyLocation reads both parts.
			"a column master key of one setting" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_PATH", Str("p"))]),
			// MasterKeyLocation reads each part once.
			"a column master key path twice" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_PATH", Str("p")), Opt("KEY_PATH", Str("q"))]),
			// MasterKeyEnclave stands only after the location.
			"an enclave that is not the third" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_PATH", Str("p")), Opt("KEY_STORE_PROVIDER_NAME", Str("s")), Opt("SIGNATURE", Hex("01"))]),
			// MasterKeyEnclave reads SIGNATURE = binary.
			"an enclave with a name for its signature" => new Statement.CreateColumnMasterKey(X, [Opt("KEY_PATH", Str("p")), Opt("KEY_STORE_PROVIDER_NAME", Str("s")), new Option(new Identifier("ENCLAVE_COMPUTATIONS"), null, [Opt("SIGNATURE", Word("s"))])]),
			// CredentialWith reads Quoted.
			"a credential identity that is a Unicode string" => new Statement.CreateCredential(X, new LiteralValue.String("'i'", StringLiteralKind.Unicode)),
			// CREATE CRYPTOGRAPHIC PROVIDER reads FROM FILE = Quoted.
			"a cryptographic provider file that is a Unicode string" => new Statement.CreateCryptographicProvider(X, new LiteralValue.String("'f'", StringLiteralKind.Unicode)),
			// KeyDecryptor reads an Identifier.
			"a key opened by a symmetric key of two parts" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.SymmetricKey(Q)),
			// KeyDecryptor reads an Identifier.
			"a key opened by a certificate of two parts" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.Certificate(Q)),
			// KeyDecryptor has no SERVICE MASTER KEY.
			"a key opened by the service master key" => new Statement.OpenSymmetricKey(X, new KeyEncryptor.ServiceMasterKey()),
			// WithoutVariable: `dbo.f (c, @user_id)` is Msg 112.
			"a policy function with a variable" => new Statement.CreateSecurityPolicy(T) { Predicates = [new PolicyPredicate.AddFilter(Function("f", new Expression.Variable(new Identifier("@u"))), T)] },
			// PolicySetting is STATE and SCHEMABINDING.
			// WithoutVariable holds the whole tail: `DROP FILTER PREDICATE ON [@t]` is Msg 112.
			"a policy table named with an @" => new Statement.AlterSecurityPolicy(T, new SecurityPolicyChange.Predicates([new PolicyPredicate.DropFilter(One(new Identifier("[@t]", IdentifierStyle.Bracketed)))])),
			// The function's name too: `ADD FILTER PREDICATE [@f](c) ON t`.
			"a policy function named with an @" => new Statement.CreateSecurityPolicy(T) { Predicates = [new PolicyPredicate.AddFilter(new PolicyFunction(One(new Identifier("[@f]", IdentifierStyle.Bracketed)), [Word("c")]), T)] },
			"a policy setting it has not" => new Statement.CreateSecurityPolicy(T) { Options = [Opt("ENABLED", Word("ON"))] },
			// PolicySetting reads OnOff.
			"a policy setting that is not ON or OFF" => new Statement.CreateSecurityPolicy(T) { Options = [Opt("STATE", Digits("1"))] },
			// PolicyAlteration reads one predicate at least.
			"an altered policy of no predicate" => new Statement.AlterSecurityPolicy(T, new SecurityPolicyChange.Predicates([])),
			// ADD SENSITIVITY CLASSIFICATION TO reads one column at least.
			"a classification of no column" => new Statement.AddSensitivityClassification([], [Opt("LABEL", Str("l"))]),
			// ClassifiedColumn is two or three parts.
			"a classified column of one part" => new Statement.AddSensitivityClassification([T], [Opt("LABEL", Str("l"))]),
			// ADD SENSITIVITY CLASSIFICATION reads one option at least.
			"a classification of no option" => new Statement.AddSensitivityClassification([Q], []),
			// NamedOnce: Msg 153 for a repeated option.
			"a classification option twice" => new Statement.AddSensitivityClassification([Q], [Opt("LABEL", Str("a")), Opt("LABEL", Str("b"))]),
			// ClassificationOption reads NONE, LOW, MEDIUM, HIGH or CRITICAL.
			"a classification rank that is none" => new Statement.AddSensitivityClassification([Q], [Opt("RANK", Word("HUGE"))]),
			// ClassificationOption reads a BrokerString.
			"a classification label that is a name" => new Statement.AddSensitivityClassification([Q], [Opt("LABEL", Word("l"))]),
			// SignatureCrypto is read once at least.
			"a signature by nothing" => new Statement.AddSignature(false, T, []),
			// SignedModule reads OBJECT, ASSEMBLY or DATABASE.
			"a signature to a module of another class" => new Statement.AddSignature(false, T, [new SignedBy.SymmetricKey(X)]) { Class = SecurableClass.Schema },
			// SignatureCrypto reads an Identifier.
			"a signature by a certificate of two parts" => new Statement.AddSignature(false, T, [new SignedBy.Certificate(Q)]),
			// SignatureCrypto reads an Identifier.
			"a signature by a key of two parts" => new Statement.AddSignature(false, T, [new SignedBy.AsymmetricKey(Q)]),
			// TSql.BinaryLiteral reads 0x….
			"a signature proof that is a binary string written X'…'" => new Statement.AddSignature(false, T, [new SignedBy.Certificate(One(X)) { Proof = new SignatureProof.Signature(new LiteralValue.Binary("'01'")) }]),
			// SignatureCrypto reads PASSWORD = Quoted.
			"a signature password that is a Unicode string" => new Statement.AddSignature(false, T, [new SignedBy.Password(new LiteralValue.String("'p'", StringLiteralKind.Unicode))]),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
