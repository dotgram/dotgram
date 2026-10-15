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

	[Theory]
	[InlineData("CREATE ASYMMETRIC KEY k AUTHORIZATION o WITH ALGORITHM = RSA_2048 ENCRYPTION BY PASSWORD = 'p'", 0)]
	[InlineData("CREATE ASYMMETRIC KEY k FROM PROVIDER p WITH PROVIDER_KEY_NAME = 'n'", 1)]
	[InlineData("CREATE ASYMMETRIC KEY k FROM FILE = 'f'", 2)]
	[InlineData("CREATE ASYMMETRIC KEY k FROM EXECUTABLE FILE = 'f'", 3)]
	[InlineData("CREATE ASYMMETRIC KEY k FROM ASSEMBLY a", 4)]
	[InlineData("ALTER ASYMMETRIC KEY k REMOVE PRIVATE KEY", 5)]
	[InlineData("ALTER ASYMMETRIC KEY k WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'a', ENCRYPTION BY PASSWORD = 'b')", 6)]
	[InlineData("ALTER ASYMMETRIC KEY k ATTESTED BY 'a'", 7)]
	[InlineData("ALTER ASYMMETRIC KEY k REMOVE ATTESTED OPTION", 8)]
	[InlineData("CREATE SYMMETRIC KEY k WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE c, PASSWORD = 'p', SYMMETRIC KEY s, ASYMMETRIC KEY a", 9)]
	[InlineData("CREATE SYMMETRIC KEY #k FROM PROVIDER p", 10)]
	[InlineData("ALTER SYMMETRIC KEY k ADD ENCRYPTION BY PASSWORD = 'p'", 11)]
	[InlineData("ALTER SYMMETRIC KEY k DROP ENCRYPTION BY PASSWORD = 'p'", 12)]
	[InlineData("CREATE CERTIFICATE c ENCRYPTION BY PASSWORD = 'p' WITH SUBJECT = 's' ACTIVE FOR BEGIN_DIALOG = ON", 13)]
	[InlineData("CREATE CERTIFICATE c WITH SUBJECT = 's' ACTIVE FOR BEGIN_DIALOG = OFF", 14)]
	[InlineData("CREATE CERTIFICATE c WITH SUBJECT = 's'", 15)]
	[InlineData("CREATE CERTIFICATE c FROM ASSEMBLY a", 16)]
	[InlineData("CREATE CERTIFICATE c FROM FILE = 'f'", 17)]
	[InlineData("CREATE CERTIFICATE c FROM EXECUTABLE FILE = 'f' WITH FORMAT = 'PFX', PRIVATE KEY (FILE = 'k')", 18)]
	[InlineData("CREATE CERTIFICATE c FROM BINARY = 0x01 WITH PRIVATE KEY (BINARY = 0x02)", 19)]
	[InlineData("ALTER CERTIFICATE c WITH ACTIVE FOR BEGIN_DIALOG = ON", 20)]
	[InlineData("ALTER CERTIFICATE c WITH PRIVATE KEY (FILE = 'k')", 21)]
	[InlineData("ALTER CERTIFICATE c REMOVE PRIVATE KEY", 22)]
	[InlineData("ALTER CERTIFICATE c ATTESTED BY 'a'", 23)]
	[InlineData("ALTER CERTIFICATE c REMOVE ATTESTED OPTION", 24)]
	[InlineData("CREATE MASTER KEY", 25)]
	[InlineData("CREATE MASTER KEY ENCRYPTION BY PASSWORD = N'p'", 26)]
	[InlineData("ALTER MASTER KEY REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'", 27)]
	[InlineData("ALTER MASTER KEY FORCE REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'", 28)]
	[InlineData("ALTER MASTER KEY ADD ENCRYPTION BY SERVICE MASTER KEY", 29)]
	[InlineData("ALTER MASTER KEY DROP ENCRYPTION BY SERVICE MASTER KEY", 30)]
	[InlineData("ALTER MASTER KEY ADD ENCRYPTION BY PASSWORD = 'p'", 31)]
	[InlineData("ALTER SERVICE MASTER KEY REGENERATE", 32)]
	[InlineData("ALTER SERVICE MASTER KEY FORCE REGENERATE", 33)]
	[InlineData("ALTER SERVICE MASTER KEY WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p'", 34)]
	[InlineData("ALTER SERVICE MASTER KEY WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p'", 35)]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_192 ENCRYPTION BY SERVER CERTIFICATE c", 36)]
	[InlineData("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = TRIPLE_DES_3KEY ENCRYPTION BY SERVER ASYMMETRIC KEY k", 37)]
	[InlineData("ALTER DATABASE ENCRYPTION KEY REGENERATE WITH ALGORITHM = AES_128", 38)]
	[InlineData("ALTER DATABASE ENCRYPTION KEY ENCRYPTION BY SERVER CERTIFICATE c", 39)]
	[InlineData("CREATE COLUMN ENCRYPTION KEY k WITH VALUES (COLUMN_MASTER_KEY = m, ALGORITHM = 'a', ENCRYPTED_VALUE = 0x01), (COLUMN_MASTER_KEY = n, ALGORITHM = 'a', ENCRYPTED_VALUE = 0x02)", 40)]
	[InlineData("ALTER COLUMN ENCRYPTION KEY k ADD VALUE (COLUMN_MASTER_KEY = m, ALGORITHM = 'a', ENCRYPTED_VALUE = 0x01)", 41)]
	[InlineData("ALTER COLUMN ENCRYPTION KEY k DROP VALUE (COLUMN_MASTER_KEY = m)", 42)]
	[InlineData("CREATE COLUMN MASTER KEY k WITH (KEY_STORE_PROVIDER_NAME = 'p', KEY_PATH = 'x', ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01))", 43)]
	[InlineData("CREATE CREDENTIAL c WITH IDENTITY = 'i', SECRET = 's' FOR CRYPTOGRAPHIC PROVIDER p", 44)]
	[InlineData("ALTER CREDENTIAL c WITH IDENTITY = 'i'", 45)]
	[InlineData("CREATE DATABASE SCOPED CREDENTIAL c WITH IDENTITY = 'i', SECRET = 's'", 46)]
	[InlineData("ALTER DATABASE SCOPED CREDENTIAL c WITH IDENTITY = 'i'", 47)]
	[InlineData("CREATE CRYPTOGRAPHIC PROVIDER p FROM FILE = 'f'", 48)]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p ENABLE", 49)]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p DISABLE", 50)]
	[InlineData("ALTER CRYPTOGRAPHIC PROVIDER p FROM FILE = 'f'", 51)]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c", 52)]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY CERTIFICATE c WITH PASSWORD = 'p'", 53)]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY ASYMMETRIC KEY a WITH PASSWORD = 'p'", 54)]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY SYMMETRIC KEY s", 55)]
	[InlineData("OPEN SYMMETRIC KEY k DECRYPTION BY PASSWORD = 'p'", 56)]
	[InlineData("OPEN MASTER KEY DECRYPTION BY PASSWORD = 'p'", 57)]
	[InlineData("CLOSE SYMMETRIC KEY k", 58)]
	[InlineData("CLOSE ALL SYMMETRIC KEYS", 59)]
	[InlineData("CLOSE MASTER KEY", 60)]
	[InlineData("CREATE SECURITY POLICY p ADD FILTER PREDICATE dbo.f(c) ON t", 61)]
	[InlineData("CREATE SECURITY POLICY p ALTER FILTER PREDICATE dbo.f(c) ON t", 62)]
	[InlineData("CREATE SECURITY POLICY p ADD BLOCK PREDICATE dbo.f() ON t AFTER INSERT", 63)]
	[InlineData("CREATE SECURITY POLICY p ALTER BLOCK PREDICATE dbo.f(1, 2) ON t BEFORE UPDATE", 64)]
	[InlineData("CREATE SECURITY POLICY p WITH (STATE = ON, SCHEMABINDING = OFF) NOT FOR REPLICATION", 65)]
	[InlineData("ALTER SECURITY POLICY p DROP FILTER PREDICATE ON t", 66)]
	[InlineData("ALTER SECURITY POLICY p DROP BLOCK PREDICATE ON t AFTER UPDATE", 67)]
	[InlineData("ALTER SECURITY POLICY p DROP BLOCK PREDICATE ON t BEFORE DELETE", 68)]
	[InlineData("ALTER SECURITY POLICY p WITH (STATE = OFF)", 69)]
	[InlineData("ALTER SECURITY POLICY p ADD NOT FOR REPLICATION", 70)]
	[InlineData("ALTER SECURITY POLICY p DROP NOT FOR REPLICATION", 71)]
	[InlineData("ADD SENSITIVITY CLASSIFICATION TO t.c, dbo.t.d WITH (LABEL = 'l', RANK = HIGH)", 72)]
	[InlineData("ADD SIGNATURE TO p BY CERTIFICATE c", 73)]
	[InlineData("ADD COUNTER SIGNATURE TO OBJECT::p BY CERTIFICATE c", 74)]
	[InlineData("ADD SIGNATURE TO ASSEMBLY::a BY CERTIFICATE c WITH PASSWORD = 'p'", 75)]
	[InlineData("ADD SIGNATURE TO DATABASE::d BY ASYMMETRIC KEY k WITH SIGNATURE = 0x01", 76)]
	[InlineData("ADD SIGNATURE TO p BY SYMMETRIC KEY k, PASSWORD = 'x'", 77)]
	[InlineData("DROP SIGNATURE FROM p BY CERTIFICATE c WITH PASSWORD = 'p'", 78)]
	[InlineData("DROP COUNTER SIGNATURE FROM p BY ASYMMETRIC KEY k WITH SIGNATURE = 0x", 79)]
	[InlineData("DROP SIGNATURE FROM p BY SYMMETRIC KEY k", 80)]
	[InlineData("DROP SIGNATURE FROM p BY PASSWORD = N'x'", 81)]
	public void The_bridge_builds_the_key_record_the_text_means(string text, int row)
	{
		var made = Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text);

		var holds = row switch
		{
			0 => made is Ast.Statement.CreateAsymmetricKey { Authorization.Text: "o", Source: null, Options: [{ Name.Text: "ALGORITHM", Value: Ast.Expression.Reference }], EncryptedBy.Value.Text: "'p'" },
			1 => made is Ast.Statement.CreateAsymmetricKey { Source: Ast.AsymmetricKeySource.Provider { Name.Text: "p" }, Options: [{ Name.Text: "PROVIDER_KEY_NAME" }] },
			2 => made is Ast.Statement.CreateAsymmetricKey { Source: Ast.AsymmetricKeySource.File { Executable: false } },
			3 => made is Ast.Statement.CreateAsymmetricKey { Source: Ast.AsymmetricKeySource.File { Executable: true } },
			4 => made is Ast.Statement.CreateAsymmetricKey { Source: Ast.AsymmetricKeySource.Assembly { Name.Parts.Count: 1 } },
			5 => made is Ast.Statement.AlterAsymmetricKey { Change: Ast.AsymmetricKeyChange.RemovePrivateKey },
			6 => made is Ast.Statement.AlterAsymmetricKey { Change: Ast.AsymmetricKeyChange.WithPrivateKey { Options: [{ Name.Text: "DECRYPTION", Words: [{ Text: "BY" }, { Text: "PASSWORD" }] }, { Name.Text: "ENCRYPTION" }] } },
			7 => made is Ast.Statement.AlterAsymmetricKey { Change: Ast.AsymmetricKeyChange.AttestedBy },
			8 => made is Ast.Statement.AlterAsymmetricKey { Change: Ast.AsymmetricKeyChange.RemoveAttestedOption },
			9 => made is Ast.Statement.CreateSymmetricKey { Provider: null, EncryptedBy: [Ast.KeyEncryptor.Certificate { Server: false, WithPassword: null }, Ast.KeyEncryptor.Password, Ast.KeyEncryptor.SymmetricKey, Ast.KeyEncryptor.AsymmetricKey] },
			10 => made is Ast.Statement.CreateSymmetricKey { Name.Text: "#k", Provider.Text: "p", Options: [], EncryptedBy: [] },
			11 => made is Ast.Statement.AlterSymmetricKey { Change: Ast.SymmetricKeyChange.AddEncryption { By: [Ast.KeyEncryptor.Password] } },
			12 => made is Ast.Statement.AlterSymmetricKey { Change: Ast.SymmetricKeyChange.DropEncryption { By: [Ast.KeyEncryptor.Password] } },
			13 => made is Ast.Statement.CreateCertificate { Source: null, EncryptedBy: not null, Options: [{ Name.Text: "SUBJECT" }], ActiveForBeginDialog: true },
			14 => made is Ast.Statement.CreateCertificate { EncryptedBy: null, ActiveForBeginDialog: false },
			15 => made is Ast.Statement.CreateCertificate { ActiveForBeginDialog: null },
			16 => made is Ast.Statement.CreateCertificate { Source: Ast.CertificateSource.Assembly },
			17 => made is Ast.Statement.CreateCertificate { Source: Ast.CertificateSource.File { Executable: false, Format: null, PrivateKey: [] } },
			18 => made is Ast.Statement.CreateCertificate { Source: Ast.CertificateSource.File { Executable: true, Format: not null, PrivateKey: [{ Name.Text: "FILE" }] } },
			19 => made is Ast.Statement.CreateCertificate { Source: Ast.CertificateSource.Binary { Value.Text: "01", PrivateKey: [{ Name.Text: "BINARY" }] } },
			20 => made is Ast.Statement.AlterCertificate { Change: Ast.CertificateChange.ActiveForBeginDialog { On: true } },
			21 => made is Ast.Statement.AlterCertificate { Change: Ast.CertificateChange.WithPrivateKey },
			22 => made is Ast.Statement.AlterCertificate { Change: Ast.CertificateChange.RemovePrivateKey },
			23 => made is Ast.Statement.AlterCertificate { Change: Ast.CertificateChange.AttestedBy },
			24 => made is Ast.Statement.AlterCertificate { Change: Ast.CertificateChange.RemoveAttestedOption },
			25 => made is Ast.Statement.CreateMasterKey { EncryptedBy: null },
			26 => made is Ast.Statement.CreateMasterKey { EncryptedBy.Value.Kind: Ast.StringLiteralKind.National },
			27 => made is Ast.Statement.AlterMasterKey { Change: Ast.MasterKeyChange.Regenerate { Force: false } },
			28 => made is Ast.Statement.AlterMasterKey { Change: Ast.MasterKeyChange.Regenerate { Force: true } },
			29 => made is Ast.Statement.AlterMasterKey { Change: Ast.MasterKeyChange.AddEncryption { By: Ast.KeyEncryptor.ServiceMasterKey } },
			30 => made is Ast.Statement.AlterMasterKey { Change: Ast.MasterKeyChange.DropEncryption { By: Ast.KeyEncryptor.ServiceMasterKey } },
			31 => made is Ast.Statement.AlterMasterKey { Change: Ast.MasterKeyChange.AddEncryption { By: Ast.KeyEncryptor.Password } },
			32 => made is Ast.Statement.AlterServiceMasterKey { Change: Ast.ServiceMasterKeyChange.Regenerate { Force: false } },
			33 => made is Ast.Statement.AlterServiceMasterKey { Change: Ast.ServiceMasterKeyChange.Regenerate { Force: true } },
			34 => made is Ast.Statement.AlterServiceMasterKey { Change: Ast.ServiceMasterKeyChange.OldAccount },
			35 => made is Ast.Statement.AlterServiceMasterKey { Change: Ast.ServiceMasterKeyChange.NewAccount },
			36 => made is Ast.Statement.CreateDatabaseEncryptionKey { Algorithm: Ast.DatabaseKeyAlgorithm.Aes192, By: Ast.KeyEncryptor.Certificate { Server: true } },
			37 => made is Ast.Statement.CreateDatabaseEncryptionKey { Algorithm: Ast.DatabaseKeyAlgorithm.TripleDes3Key, By: Ast.KeyEncryptor.AsymmetricKey { Server: true } },
			38 => made is Ast.Statement.AlterDatabaseEncryptionKey { Change: Ast.DatabaseEncryptionKeyChange.Regenerate { Algorithm: Ast.DatabaseKeyAlgorithm.Aes128 } },
			39 => made is Ast.Statement.AlterDatabaseEncryptionKey { Change: Ast.DatabaseEncryptionKeyChange.EncryptedBy { By: Ast.KeyEncryptor.Certificate { Server: true } } },
			40 => made is Ast.Statement.CreateColumnEncryptionKey { Values: [{ Settings.Count: 3 }, { Settings.Count: 3 }] },
			41 => made is Ast.Statement.AlterColumnEncryptionKey { Change: Ast.ColumnKeyChange.AddValue { Value.Settings.Count: 3 } },
			42 => made is Ast.Statement.AlterColumnEncryptionKey { Change: Ast.ColumnKeyChange.DropValue { Value.Settings.Count: 1 } },
			43 => made is Ast.Statement.CreateColumnMasterKey { Options: [{ Name.Text: "KEY_STORE_PROVIDER_NAME" }, { Name.Text: "KEY_PATH" }, { Name.Text: "ENCLAVE_COMPUTATIONS", Options: [{ Name.Text: "SIGNATURE", Value: Ast.LiteralValue.Binary }] }] },
			44 => made is Ast.Statement.CreateCredential { Secret: not null, Provider.Text: "p" },
			45 => made is Ast.Statement.AlterCredential { Secret: null, Provider: null },
			46 => made is Ast.Statement.CreateDatabaseScopedCredential { Secret: not null },
			47 => made is Ast.Statement.AlterDatabaseScopedCredential { Secret: null },
			48 => made is Ast.Statement.CreateCryptographicProvider,
			49 => made is Ast.Statement.AlterCryptographicProvider { Change: Ast.CryptographicProviderChange.Enable },
			50 => made is Ast.Statement.AlterCryptographicProvider { Change: Ast.CryptographicProviderChange.Disable },
			51 => made is Ast.Statement.AlterCryptographicProvider { Change: Ast.CryptographicProviderChange.FromFile },
			52 => made is Ast.Statement.OpenSymmetricKey { DecryptedBy: Ast.KeyEncryptor.Certificate { WithPassword: null, Server: false } },
			53 => made is Ast.Statement.OpenSymmetricKey { DecryptedBy: Ast.KeyEncryptor.Certificate { WithPassword: not null } },
			54 => made is Ast.Statement.OpenSymmetricKey { DecryptedBy: Ast.KeyEncryptor.AsymmetricKey { WithPassword: not null } },
			55 => made is Ast.Statement.OpenSymmetricKey { DecryptedBy: Ast.KeyEncryptor.SymmetricKey },
			56 => made is Ast.Statement.OpenSymmetricKey { DecryptedBy: Ast.KeyEncryptor.Password },
			57 => made is Ast.Statement.OpenMasterKey,
			58 => made is Ast.Statement.CloseSymmetricKey { Name.Text: "k" },
			59 => made is Ast.Statement.CloseAllSymmetricKeys,
			60 => made is Ast.Statement.CloseMasterKey,
			61 => made is Ast.Statement.CreateSecurityPolicy { Predicates: [Ast.PolicyPredicate.AddFilter { Function.Arguments.Count: 1 }] },
			62 => made is Ast.Statement.CreateSecurityPolicy { Predicates: [Ast.PolicyPredicate.AlterFilter] },
			63 => made is Ast.Statement.CreateSecurityPolicy { Predicates: [Ast.PolicyPredicate.AddBlock { Function.Arguments.Count: 0, When: Ast.BlockOperation.AfterInsert }] },
			64 => made is Ast.Statement.CreateSecurityPolicy { Predicates: [Ast.PolicyPredicate.AlterBlock { When: Ast.BlockOperation.BeforeUpdate }] },
			65 => made is Ast.Statement.CreateSecurityPolicy { Predicates: [], Options: [{ Name.Text: "STATE" }, { Name.Text: "SCHEMABINDING" }], NotForReplication: true },
			66 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.Predicates { Items: [Ast.PolicyPredicate.DropFilter] } },
			67 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.Predicates { Items: [Ast.PolicyPredicate.DropBlock { When: Ast.BlockOperation.AfterUpdate }] } },
			68 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.Predicates { Items: [Ast.PolicyPredicate.DropBlock { When: Ast.BlockOperation.BeforeDelete }] } },
			69 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.State { On: false } },
			70 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.AddNotForReplication },
			71 => made is Ast.Statement.AlterSecurityPolicy { Change: Ast.SecurityPolicyChange.DropNotForReplication },
			72 => made is Ast.Statement.AddSensitivityClassification { Columns.Count: 2, Options: [{ Name.Text: "LABEL", Value: Ast.LiteralValue.String }, { Name.Text: "RANK", Value: Ast.Expression.Reference }] },
			73 => made is Ast.Statement.AddSignature { Counter: false, Class: null, By: [Ast.SignedBy.Certificate { Proof: null }] },
			74 => made is Ast.Statement.AddSignature { Counter: true, Class: Ast.SecurableClass.Object },
			75 => made is Ast.Statement.AddSignature { Class: Ast.SecurableClass.Assembly, By: [Ast.SignedBy.Certificate { Proof: Ast.SignatureProof.Password }] },
			76 => made is Ast.Statement.AddSignature { Class: Ast.SecurableClass.Database, By: [Ast.SignedBy.AsymmetricKey { Proof: Ast.SignatureProof.Signature { Value.Text: "01" } }] },
			77 => made is Ast.Statement.AddSignature { By: [Ast.SignedBy.SymmetricKey { Name.Text: "k" }, Ast.SignedBy.Password] },
			78 => made is Ast.Statement.DropSignature { Counter: false, By: [Ast.SignedBy.Certificate { Proof: Ast.SignatureProof.Password }] },
			79 => made is Ast.Statement.DropSignature { Counter: true, By: [Ast.SignedBy.AsymmetricKey { Proof: Ast.SignatureProof.Signature { Value.Text: "" } }] },
			80 => made is Ast.Statement.DropSignature { By: [Ast.SignedBy.SymmetricKey] },
			81 => made is Ast.Statement.DropSignature { By: [Ast.SignedBy.Password { Value.Kind: Ast.StringLiteralKind.National }] },
			_ => false,
		};

		Assert.True(holds, $"{text} built {made}");

		var outcome = BridgeOracle.Run(TransactSqlParser.Located.ParseStatement(text), text);

		Assert.True(outcome.Status == BridgeOracle.Status.Converted, $"{text}: {outcome.Status} {outcome.Reason} {outcome.Detail}");
	}
}
