using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

		static Ast.Statement Role(Statement.CreateRole old)
		{
			if (old.Tail is not null)
				throw BridgeGap.Missing(old, "Statement.CreateRole has no AUTHORIZATION owner (inventory Statement.CreateRole)");

			return new Ast.Statement.CreateRole { Name = Values.Part(old, old.Name) };
		}
	}
}
