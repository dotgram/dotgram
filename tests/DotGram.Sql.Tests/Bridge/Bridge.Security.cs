using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Permissions, principals, schemas, keys, certificates, credentials and the classifications and signatures on objects.
	/// </summary>
	/// <remarks>
	/// <c>CREATE SCHEMA</c> and <c>CREATE ROLE</c> have arms, as the two A rows here; every other record throws
	/// the gap its inventory row names.
	/// </remarks>
	internal static class Security
	{
		public static readonly string[] Owns =
		[
			"Statement.Grant", "Statement.Deny", "Statement.Revoke",
			"Statement.CreateRole", "Statement.AlterRole", "Statement.CreateServerRole",
			"Statement.AlterServerRole", "Statement.CreateApplicationRole", "Statement.AlterApplicationRole",
			"Statement.CreateLogin", "Statement.AlterLogin", "Statement.CreateUser",
			"Statement.AlterUser", "Statement.SchemaDefinition", "Statement.AlterSchema",
			"Statement.AlterAuthorization", "Statement.AsymmetricKeyDefinition", "Statement.AlterAsymmetricKey",
			"Statement.SymmetricKeyDefinition", "Statement.AlterSymmetricKey", "Statement.CertificateDefinition",
			"Statement.AlterCertificate", "Statement.MasterKeyDefinition", "Statement.AlterMasterKey",
			"Statement.AlterServiceMasterKey", "Statement.DatabaseEncryptionKeyDefinition", "Statement.AlterDatabaseEncryptionKey",
			"Statement.ColumnEncryptionKeyDefinition", "Statement.AlterColumnEncryptionKey", "Statement.ColumnMasterKeyDefinition",
			"Statement.CredentialDefinition", "Statement.DatabaseScopedCredentialDefinition", "Statement.SecurityPolicyDefinition",
			"Statement.CryptographicProviderDefinition", "Statement.OpenSymmetricKey", "Statement.OpenMasterKey",
			"Statement.CloseSymmetricKey", "Statement.CloseAllSymmetricKeys", "Statement.CloseMasterKey",
			"Statement.Classification", "Statement.AddSignature",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.SchemaDefinition schema => Schema(schema),
				Statement.CreateRole role         => Role(role),
				Statement.Grant grant             => Grant(grant),
				Statement.Deny deny               => Deny(deny),
				Statement.Revoke revoke           => Revoke(revoke),
				_ when Owned(Owns, old)           => throw Inventory.Unconverted(old),
				_                                 => null,
			};

			return made is not null;
		}

		/// <summary>
		/// <c>CREATE SCHEMA s [AUTHORIZATION owner]</c>. The old tree keeps what follows the name as text, and the
		/// schema's elements in it cannot be read back from that here.
		/// </summary>
		static Ast.Statement Schema(Statement.SchemaDefinition old)
		{
			// `CREATE SCHEMA AUTHORIZATION o` names no schema, which the old tree keeps as an empty name.
			var made = new Ast.Statement.CreateSchema { Name = old.Name.Length > 0 ? Values.Name(old, old.Name) : null };

			if (old.Tail is not { } tail)
				return made;

			// Exactly AUTHORIZATION and one name; anything after it is a schema element, kept as text.
			if (SqlTokens.Of(tail) is not [{ Kind: SqlTokens.Kind.Word } authorization, { Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } name]
				|| !authorization.Text.Equals("AUTHORIZATION", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, "inventory Statement.SchemaDefinition (elements kept as text)");

			var owner = name.Text;

			return made with { Authorization = new Ast.AuthorizationIdentifier(Values.Part(old, owner)) };
		}

		// ── GRANT, DENY and REVOKE ─────────────────────────────────────────────────

		static Ast.Statement Grant(Statement.Grant old)
		{
			return new Ast.Statement.Grant
			{
				Body = new Ast.GrantBody.Privileges(
					Privileges(old, old.Privileges), Securable(old, old.On), Grantees(old, old.Principals),
					HierarchyOption: false, GrantOption: old.GrantOption, GrantedBy: null, AsPrincipal: AsPrincipal(old, old.As)),
			};
		}

		static Ast.Statement Deny(Statement.Deny old)
		{
			return new Ast.Statement.Deny(Privileges(old, old.Privileges), Grantees(old, old.Principals), Securable(old, old.On), old.Cascade, AsPrincipal(old, old.As));
		}

		static Ast.Statement Revoke(Statement.Revoke old)
		{
			return new Ast.Statement.Revoke
			{
				Body = new Ast.RevokeBody.Privileges(
					old.GrantOptionFor ? Ast.RevokeOption.GrantOptionFor : null,
					Privileges(old, old.Privileges), Securable(old, old.On), Grantees(old, old.Principals),
					GrantedBy: null, Behavior: old.Cascade ? Ast.DropBehavior.Cascade : null, AsPrincipal: AsPrincipal(old, old.As), To: !old.From),
			};
		}

		/// <summary>
		/// The words the writer prints for each permission and class, asked of the writer as the catalogue's tests ask
		/// it: <c>VIEW DEFINITION</c> against its member. <c>ALL</c> and <c>EXEC</c> are the abbreviations of two.
		/// </summary>
		static readonly Lazy<Dictionary<string, Ast.PrivilegeKind>> PermissionWords = new(static () =>
		{
			var words = new Dictionary<string, Ast.PrivilegeKind>(StringComparer.Ordinal);

			foreach (var kind in Enum.GetValues<Ast.PrivilegeKind>())
			{
				var granted = Ast.Sql2023Writer.Write(new Ast.Statement.Grant
				{
					Body = new Ast.GrantBody.Privileges([new Ast.Privilege(kind, [], [])], null, [new Ast.Grantee.Public()], false, false, null),
				});

				words[Regex.Match(granted, "^GRANT (.+) TO PUBLIC").Groups[1].Value] = kind;
			}

			return words;
		});

		static readonly Lazy<Dictionary<string, Ast.SecurableClass>> ClassWords = new(static () =>
		{
			var words = new Dictionary<string, Ast.SecurableClass>(StringComparer.Ordinal);

			foreach (var kind in Enum.GetValues<Ast.SecurableClass>())
			{
				var granted = Ast.Sql2023Writer.Write(new Ast.Statement.Grant
				{
					Body = new Ast.GrantBody.Privileges(
						[new Ast.Privilege(Ast.PrivilegeKind.Control, [], [])],
						new Ast.PrivilegeObject.Classed(kind, new Ast.QualifiedName([new Ast.Identifier("s")])), [new Ast.Grantee.Public()], false, false, null),
				});

				words[Regex.Match(granted, " ON (.+)::s TO ").Groups[1].Value] = kind;
			}

			return words;
		});

		static SqlList<Ast.Privilege> Privileges(object old, SqlList<string> permissions)
		{
			return Each(permissions, one => Privilege(old, one));
		}

		/// <summary>
		/// One permission, which the old tree keeps as its words and a column list: <c>VIEW DEFINITION</c>,
		/// <c>SELECT (a, b)</c>. <c>ALL</c> and <c>EXEC</c> are the flag's spelling of <c>ALL PRIVILEGES</c> and
		/// <c>EXECUTE</c>.
		/// </summary>
		static Ast.Privilege Privilege(object old, string text)
		{
			var tokens = SqlTokens.Of(text);
			var open   = tokens.FindIndex(static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: "(" });
			var head   = open < 0 ? tokens : tokens[..open];

			if (head.Count == 0 || head.Any(static one => one.Kind != SqlTokens.Kind.Word))
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the permission `{text}`)");

			var words        = string.Join(' ', head.Select(static one => one.Text.ToUpperInvariant()));
			var abbreviation = words is "ALL" or "EXEC";
			var kind         = words switch
			{
				"ALL"  => Ast.PrivilegeKind.AllPrivileges,
				"EXEC" => Ast.PrivilegeKind.Execute,
				_      => PermissionWords.Value.TryGetValue(words, out var found) ? found : throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the permission `{text}`)"),
			};

			var columns = open < 0 ? [] : Columns(old, tokens, open, text);

			return new Ast.Privilege(kind, columns, [], abbreviation);
		}

		/// <summary>
		/// A bracketed column list that runs to the end of the tokens, from the bracket at <paramref name="open"/>.
		/// </summary>
		static SqlList<Ast.Identifier> Columns(object old, List<SqlTokens.Token> tokens, int open, string text)
		{
			var columns = new List<Ast.Identifier>();
			var expect  = true;

			for (var at = open + 1; at < tokens.Count - 1; at++)
			{
				var token = tokens[at];

				if (expect && token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
					columns.Add(Values.Part(old, token.Text));
				else if (!expect && token is { Kind: SqlTokens.Kind.Punctuation, Text: "," })
				{
				}
				else
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the column list of `{text}`)");

				expect = !expect;
			}

			if (expect || tokens[^1] is not { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the column list of `{text}`)");

			return columns.ToSqlList();
		}

		/// <summary>
		/// What follows ON: the class and <c>::</c>, a name, and the column list after it, in the words the old tree keeps.
		/// </summary>
		static Ast.PrivilegeObject? Securable(object old, string? on)
		{
			if (on is null)
				return null;

			var tokens = SqlTokens.Of(on);
			var colons = tokens.FindIndex(static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: ":" });
			Ast.SecurableClass? kind = null;
			var from = 0;

			if (colons >= 0)
			{
				var written = string.Join(' ', tokens[..colons].Select(static one => one.Text.ToUpperInvariant()));

				if (colons + 1 >= tokens.Count || tokens[colons + 1] is not { Kind: SqlTokens.Kind.Punctuation, Text: ":" }
					|| !ClassWords.Value.TryGetValue(written, out var found))
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the securable `{on}`)");

				kind = found;
				from = colons + 2;
			}

			var open = tokens.FindIndex(from, static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: "(" });
			var name = Dotted(open < 0 ? tokens[from..] : tokens[from..open]) ?? throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the securable `{on}`)");
			SqlList<Ast.Identifier>? columns = open < 0 ? null : Columns(old, tokens, open, on);
			var qualified = Values.Name(old, name);

			return kind is { } one
				? new Ast.PrivilegeObject.Classed(one, qualified, columns)
				: new Ast.PrivilegeObject.Named(Ast.PrivilegeObjectKind.Table, qualified, null, false, columns);
		}

		/// <summary>
		/// A qualified name read from tokens, its parts and the dots between them as written, or null where the tokens are
		/// not one.
		/// </summary>
		static string? Dotted(IEnumerable<SqlTokens.Token> tokens)
		{
			var text = new StringBuilder();
			var dot  = true;

			foreach (var token in tokens)
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "." })
					dot = true;
				else if (token.Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited && dot)
					dot = false;
				else
					return null;

				text.Append(token.Text);
			}

			return text.Length == 0 ? null : text.ToString();
		}

		/// <summary>
		/// The principals: the old tree has normalised <c>PUBLIC</c> and <c>NULL</c> to their capitals, which no name
		/// written plain can be, since both are reserved.
		/// </summary>
		static SqlList<Ast.Grantee> Grantees(object old, SqlList<string> principals)
		{
			return Each<string, Ast.Grantee>(principals, one => one switch
			{
				"PUBLIC" => new Ast.Grantee.Public(),
				"NULL"   => new Ast.Grantee.Null(),
				_        => new Ast.Grantee.Identifier(new Ast.AuthorizationIdentifier(Values.Part(old, one))),
			});
		}

		/// <summary>
		/// <c>AS principal</c>. The grammar reads PUBLIC there, which is no name the new tree writes plain.
		/// </summary>
		static Ast.Identifier? AsPrincipal(object old, string? principal)
		{
			if (principal is null)
				return null;

			return principal == "PUBLIC"
				? throw BridgeGap.Open(old, $"inventory {NameOf(old)} (AS PUBLIC, which a plain name cannot be: PUBLIC is reserved)")
				: Values.Part(old, principal);
		}

		static Ast.Statement Role(Statement.CreateRole old)
		{
			if (old.Tail is not null)
				throw BridgeGap.Missing(old, "Statement.CreateRole has no AUTHORIZATION owner (inventory Statement.CreateRole)");

			return new Ast.Statement.CreateRole { Name = Values.Part(old, old.Name) };
		}
	}
}
