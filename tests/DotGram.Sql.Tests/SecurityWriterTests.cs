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

	static Option Opt(string name, Expression? value = null, params Option[] options)
	{
		return new Option(new Identifier(name), value, [.. options]);
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
	/// Each statement's tree, written and read back by TransactSqlParser, carried by the bridge to the same tree.
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

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	/// <summary>
	/// A tree whose text would not read back as the same tree is refused; each row names the rule or the engine's
	/// message that does not read the text.
	/// </summary>
	[Theory]
	[InlineData("CREATE ROLE with WITH ADMIN")]
	[InlineData("a schema of no name and no owner")]
	[InlineData("a schema named by two parts")]
	[InlineData("a schema with a character set")]
	[InlineData("a schema with an element of another kind")]
	[InlineData("an application role of no setting")]
	[InlineData("an application role with a setting it has not")]
	[InlineData("an application role password that is a name")]
	[InlineData("a setting with no value")]
	[InlineData("a setting written without its =")]
	[InlineData("a setting with a value it has none of")]
	[InlineData("a setting in brackets")]
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
			// PrincipalStatement reads a name or AUTHORIZATION, and CREATE SCHEMA alone is Msg 102.
			"a schema of no name and no owner" => new Statement.CreateSchema(),
			// The name is an Identifier: `CREATE SCHEMA a.b` is Msg 102 near '.'.
			"a schema named by two parts" => new Statement.CreateSchema { Name = Q },
			// The standard's DEFAULT CHARACTER SET is no part of T-SQL's schema.
			"a schema with a character set" => new Statement.CreateSchema { Name = T, DefaultCharacterSet = new CharacterSetName(new QualifiedName([X])) },
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
}
