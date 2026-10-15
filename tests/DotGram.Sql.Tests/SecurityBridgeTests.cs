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
}
