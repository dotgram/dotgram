using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The Security family's bridge (Bridge.Security.cs): which record and members it builds for a text, where another record
/// or a flag set the other way writes the same or nearly the same text, and that each converts.
/// </summary>
public sealed class SecurityBridgeTests
{
	[Theory]
	[InlineData("GRANT SELECT ON t TO u",                                  "Grant")]
	[InlineData("DENY SELECT ON t TO u",                                   "Deny")]
	[InlineData("REVOKE SELECT ON t FROM u",                               "Revoke from")]
	[InlineData("REVOKE SELECT ON t TO u",                                 "Revoke to")]
	[InlineData("REVOKE GRANT OPTION FOR SELECT ON t FROM u CASCADE",      "Revoke option")]
	[InlineData("GRANT SELECT ON t TO u WITH GRANT OPTION",                "Grant option")]
	[InlineData("DENY SELECT ON t TO u CASCADE",                           "Deny cascade")]
	[InlineData("GRANT SELECT ON t TO u AS keeper",                        "Grant as")]
	[InlineData("GRANT SELECT ON OBJECT::t TO u",                          "Classed object")]
	[InlineData("GRANT SELECT ON t TO u",                                  "Named")]
	[InlineData("GRANT CONTROL ON SCHEMA::s TO u",                         "Classed schema")]
	[InlineData("GRANT CONTROL ON CERTIFICATE::c TO u",                    "Classed certificate")]
	[InlineData("GRANT ALTER ON SERVER ROLE::r TO u",                      "Classed two words")]
	[InlineData("GRANT ALL TO u",                                          "All")]
	[InlineData("GRANT ALL PRIVILEGES TO u",                               "All privileges")]
	[InlineData("GRANT EXEC ON t TO u",                                    "Exec")]
	[InlineData("GRANT EXECUTE ON t TO u",                                 "Execute")]
	[InlineData("GRANT SELECT (a, b) ON t TO u",                           "Columns of the permission")]
	[InlineData("GRANT SELECT ON t (a, b) TO u",                           "Columns of the securable")]
	[InlineData("GRANT SELECT ON SCHEMA::s (a) TO u",                      "Columns of a class")]
	[InlineData("GRANT SELECT ON ..t TO u",                                "Empty parts")]
	[InlineData("GRANT SELECT ON t TO PUBLIC, NULL, [PUBLIC], u",          "Grantees")]
	[InlineData("GRANT VIEW SERVER STATE TO u",                            "Server permission")]
	[InlineData("grant alter on server role::s to u",                      "Lower case")]
	public void The_bridge_builds_the_permission_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Grant" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { GrantOption: false, AsPrincipal: null, Object: Ast.PrivilegeObject.Named { Kind: Ast.PrivilegeObjectKind.Table, Columns: null }, Items: [{ Kind: Ast.PrivilegeKind.Select, Abbreviated: false }] } },
			"Deny" => made is Ast.Statement.Deny { Cascade: false, AsPrincipal: null, Items: [{ Kind: Ast.PrivilegeKind.Select }] },
			"Revoke from" => made is Ast.Statement.Revoke { Body: Ast.RevokeBody.Privileges { To: false, Option: null, Behavior: null } },
			"Revoke to" => made is Ast.Statement.Revoke { Body: Ast.RevokeBody.Privileges { To: true } },
			"Revoke option" => made is Ast.Statement.Revoke { Body: Ast.RevokeBody.Privileges { Option: Ast.RevokeOption.GrantOptionFor, Behavior: Ast.DropBehavior.Cascade, To: false } },
			"Grant option" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { GrantOption: true, HierarchyOption: false } },
			"Deny cascade" => made is Ast.Statement.Deny { Cascade: true },
			"Grant as" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { AsPrincipal.Text: "keeper", GrantOption: false } },
			"Classed object" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Classed { Class: Ast.SecurableClass.Object } } },
			"Named" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Named } },
			"Classed schema" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Classed { Class: Ast.SecurableClass.Schema, Name.Parts: [{ Text: "s" }] } } },
			"Classed certificate" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Classed { Class: Ast.SecurableClass.Certificate } } },
			"Classed two words" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Classed { Class: Ast.SecurableClass.ServerRole } } },
			"All" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Kind: Ast.PrivilegeKind.AllPrivileges, Abbreviated: true }] } },
			"All privileges" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Kind: Ast.PrivilegeKind.AllPrivileges, Abbreviated: false }] } },
			"Exec" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Kind: Ast.PrivilegeKind.Execute, Abbreviated: true }] } },
			"Execute" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Kind: Ast.PrivilegeKind.Execute, Abbreviated: false }] } },
			"Columns of the permission" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Columns.Count: 2 }], Object: Ast.PrivilegeObject.Named { Columns: null } } },
			"Columns of the securable" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Columns.Count: 0 }], Object: Ast.PrivilegeObject.Named { Columns.Count: 2 } } },
			"Columns of a class" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Classed { Columns.Count: 1 } } },
			"Empty parts" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: Ast.PrivilegeObject.Named { Name.Parts: [{ Text: "" }, { Text: "" }, { Text: "t" }] } } },
			"Grantees" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Grantees: [Ast.Grantee.Public, Ast.Grantee.Null, Ast.Grantee.Identifier { Value.Name: { Text: "[PUBLIC]", Style: Ast.IdentifierStyle.Bracketed } }, Ast.Grantee.Identifier { Value.Name.Text: "u" }] } },
			"Server permission" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Object: null, Items: [{ Kind: Ast.PrivilegeKind.ViewServerState }] } },
			"Lower case" => made is Ast.Statement.Grant { Body: Ast.GrantBody.Privileges { Items: [{ Kind: Ast.PrivilegeKind.Alter }], Object: Ast.PrivilegeObject.Classed { Class: Ast.SecurableClass.ServerRole } } },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}

	[Theory]
	[InlineData("CREATE ROLE r AUTHORIZATION o",                           "Role")]
	[InlineData("CREATE SERVER ROLE r AUTHORIZATION o",                    "Server role")]
	[InlineData("ALTER ROLE r ADD MEMBER m",                               "Role add")]
	[InlineData("ALTER SERVER ROLE r ADD MEMBER m",                        "Server role add")]
	[InlineData("ALTER ROLE r DROP MEMBER m",                              "Role drop")]
	[InlineData("ALTER ROLE r WITH NAME = n",                              "Role name")]
	[InlineData("CREATE APPLICATION ROLE r WITH PASSWORD = 'p', DEFAULT_SCHEMA = s", "Application role")]
	[InlineData("ALTER APPLICATION ROLE r WITH NAME = n, PASSWORD = N'p'",  "Application role altered")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' HASHED MUST_CHANGE, SID = 0x01", "Login password hashed first")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' MUST_CHANGE HASHED",   "Login password must change first")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 'p' MUST_CHANGE",          "Login password must change")]
	[InlineData("CREATE LOGIN l WITH PASSWORD = 0x01 HASHED",              "Login hash")]
	[InlineData("CREATE LOGIN l FROM WINDOWS WITH DEFAULT_DATABASE = d",   "Login windows")]
	[InlineData("CREATE LOGIN l FROM EXTERNAL PROVIDER WITH TYPE = E",     "Login external")]
	[InlineData("CREATE LOGIN l FROM CERTIFICATE c WITH CREDENTIAL = cr",  "Login certificate")]
	[InlineData("CREATE LOGIN l FROM ASYMMETRIC KEY k",                    "Login asymmetric key")]
	[InlineData("ALTER LOGIN l ENABLE",                                    "Alter login enable")]
	[InlineData("ALTER LOGIN l DISABLE",                                   "Alter login disable")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' OLD_PASSWORD = 'q', NAME = n", "Alter login old password")]
	[InlineData("ALTER LOGIN l WITH PASSWORD = 'p' UNLOCK MUST_CHANGE, NO CREDENTIAL", "Alter login words")]
	[InlineData("ALTER LOGIN l ADD CREDENTIAL c",                          "Alter login add")]
	[InlineData("ALTER LOGIN l DROP CREDENTIAL c",                         "Alter login drop")]
	[InlineData("CREATE USER u FOR LOGIN l",                               "User for login")]
	[InlineData("CREATE USER u FROM LOGIN l",                              "User from login")]
	[InlineData("CREATE USER u FOR CERTIFICATE c",                         "User certificate")]
	[InlineData("CREATE USER u FROM ASYMMETRIC KEY k WITH DEFAULT_SCHEMA = s", "User asymmetric key")]
	[InlineData("CREATE USER u FOR EXTERNAL PROVIDER",                     "User external provider for")]
	[InlineData("CREATE USER u FROM EXTERNAL PROVIDER",                    "User external provider from")]
	[InlineData("CREATE USER u WITHOUT LOGIN",                             "User without login")]
	[InlineData("CREATE USER u WITH PASSWORD = 'p', DEFAULT_LANGUAGE = 1033", "User with password")]
	[InlineData("CREATE USER u",                                           "User alone")]
	[InlineData("ALTER USER u WITH DEFAULT_SCHEMA = NULL, PASSWORD = 'p' OLD_PASSWORD = 'q'", "Alter user")]
	[InlineData("ALTER SCHEMA s TRANSFER dbo.t",                           "Alter schema")]
	[InlineData("ALTER SCHEMA s TRANSFER TYPE::t",                         "Alter schema class")]
	[InlineData("ALTER AUTHORIZATION ON dbo.t TO u",                       "Authorization principal")]
	[InlineData("ALTER AUTHORIZATION ON OBJECT::t TO SCHEMA OWNER",        "Authorization schema owner")]
	[InlineData("CREATE SCHEMA s",                                         "Schema named")]
	[InlineData("CREATE SCHEMA AUTHORIZATION o",                           "Schema owned")]
	[InlineData("CREATE SCHEMA s AUTHORIZATION o CREATE TABLE t (a INT) GRANT SELECT ON t TO u", "Schema with elements")]
	[InlineData("CREATE SCHEMA s CREATE VIEW v AS SELECT 1 AS a",          "Schema elements without owner")]
	public void The_bridge_builds_the_principal_record_the_text_means(string text, string expected)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = expected switch
		{
			"Role" => made is Ast.Statement.CreateRole { Name.Text: "r", Authorization.Text: "o", Admin: null },
			"Server role" => made is Ast.Statement.CreateServerRole { Name.Text: "r", Authorization.Text: "o" },
			"Role add" => made is Ast.Statement.AlterRole { Action: Ast.RoleAction.AddMember { Member.Text: "m" } },
			"Server role add" => made is Ast.Statement.AlterServerRole { Action: Ast.RoleAction.AddMember { Member.Text: "m" } },
			"Role drop" => made is Ast.Statement.AlterRole { Action: Ast.RoleAction.DropMember },
			"Role name" => made is Ast.Statement.AlterRole { Action: Ast.RoleAction.WithName { NewName.Text: "n" } },
			"Application role" => made is Ast.Statement.CreateApplicationRole { Options: [{ Name.Text: "PASSWORD", Value: Ast.LiteralValue.String }, { Name.Text: "DEFAULT_SCHEMA", Value: Ast.Expression.Reference }] },
			"Application role altered" => made is Ast.Statement.AlterApplicationRole { Options: [{ Name.Text: "NAME" }, { Name.Text: "PASSWORD", Value: Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } }] },
			"Login password hashed first" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Password { Hashed: true, MustChange: true, MustChangeFirst: false }, Options: [{ Name.Text: "SID", Value: Ast.LiteralValue.Binary { ZeroX: true } }] },
			"Login password must change first" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Password { Hashed: true, MustChange: true, MustChangeFirst: true } },
			"Login password must change" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Password { Hashed: false, MustChange: true, MustChangeFirst: false } },
			"Login hash" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Password { Value: Ast.LiteralValue.Binary, Hashed: true, MustChange: false } },
			"Login windows" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Windows, Options: [{ Name.Text: "DEFAULT_DATABASE" }] },
			"Login external" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.ExternalProvider, Options: [{ Name.Text: "TYPE" }] },
			"Login certificate" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.Certificate { Name.Text: "c", Credential.Text: "cr" }, Options: [] },
			"Login asymmetric key" => made is Ast.Statement.CreateLogin { Source: Ast.LoginSource.AsymmetricKey { Name.Text: "k", Credential: null } },
			"Alter login enable" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.Enable },
			"Alter login disable" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.Disable },
			"Alter login old password" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.With { Options: [{ Name.Text: "PASSWORD", Options: [{ Name.Text: "OLD_PASSWORD", Value: Ast.LiteralValue.String }] }, { Name.Text: "NAME" }] } },
			"Alter login words" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.With { Options: [{ Name.Text: "PASSWORD", Options: [{ Name.Text: "UNLOCK" }, { Name.Text: "MUST_CHANGE" }] }, { Name.Text: "NO", Words: [{ Text: "CREDENTIAL" }], Value: null }] } },
			"Alter login add" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.AddCredential { Name.Text: "c" } },
			"Alter login drop" => made is Ast.Statement.AlterLogin { Action: Ast.AlterLoginAction.DropCredential { Name.Text: "c" } },
			"User for login" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.Login { From: false, Name.Text: "l" }, Options: [] },
			"User from login" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.Login { From: true } },
			"User certificate" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.Certificate { From: false } },
			"User asymmetric key" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.AsymmetricKey { From: true }, Options: [{ Name.Text: "DEFAULT_SCHEMA" }] },
			"User external provider for" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.ExternalProvider { From: false } },
			"User external provider from" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.ExternalProvider { From: true } },
			"User without login" => made is Ast.Statement.CreateUser { Source: Ast.UserSource.WithoutLogin },
			"User with password" => made is Ast.Statement.CreateUser { Source: null, Options: [{ Name.Text: "PASSWORD" }, { Name.Text: "DEFAULT_LANGUAGE", Value: Ast.LiteralValue.Numeric }] },
			"User alone" => made is Ast.Statement.CreateUser { Source: null, Options: [] },
			"Alter user" => made is Ast.Statement.AlterUser { Options: [{ Name.Text: "DEFAULT_SCHEMA", Value: Ast.LiteralValue.Null }, { Name.Text: "PASSWORD", Options: [{ Name.Text: "OLD_PASSWORD" }] }] },
			"Alter schema" => made is Ast.Statement.AlterSchema { Class: null, Securable.Parts.Count: 2 },
			"Alter schema class" => made is Ast.Statement.AlterSchema { Class: Ast.SecurableClass.Type },
			"Authorization principal" => made is Ast.Statement.AlterAuthorization { Class: null, To: Ast.AuthorizationTo.Principal { Name.Text: "u" } },
			"Authorization schema owner" => made is Ast.Statement.AlterAuthorization { Class: Ast.SecurableClass.Object, To: Ast.AuthorizationTo.SchemaOwner },
			"Schema named" => made is Ast.Statement.CreateSchema { Name.Parts: [{ Text: "s" }], Authorization: null, Elements: [] },
			"Schema owned" => made is Ast.Statement.CreateSchema { Name: null, Authorization.Name.Text: "o", Elements: [] },
			"Schema with elements" => made is Ast.Statement.CreateSchema { Authorization.Name.Text: "o", Elements: [Ast.Statement.CreateTable, Ast.Statement.Grant] },
			"Schema elements without owner" => made is Ast.Statement.CreateSchema { Authorization: null, Elements: [Ast.Statement.CreateView] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
