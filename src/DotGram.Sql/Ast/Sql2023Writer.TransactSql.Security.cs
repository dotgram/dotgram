using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.TransactSql;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Security family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutSecurity(ISqlNode node)
		{
			switch (node)
			{
				case Statement.AlterRole alter:
					Word("ALTER ROLE");
					PutIdentifier(alter.Name);
					PutRoleAction(alter, alter.Action);
					return true;

				case Statement.CreateServerRole create:
					Word("CREATE SERVER ROLE");
					PutIdentifier(create.Name);
					PutAuthorization(create.Authorization);
					return true;

				case Statement.AlterServerRole alter:
					Word("ALTER SERVER ROLE");
					PutIdentifier(alter.Name);
					PutRoleAction(alter, alter.Action);
					return true;

				case Statement.CreateApplicationRole create:
					Word("CREATE APPLICATION ROLE");
					PutIdentifier(create.Name);
					PutSettings(create, create.Options, ApplicationRoleSettings, "CREATE APPLICATION ROLE", introducer: "WITH", required: true);
					return true;

				case Statement.AlterApplicationRole alter:
					Word("ALTER APPLICATION ROLE");
					PutIdentifier(alter.Name);
					PutSettings(alter, alter.Options, AlterApplicationRoleSettings, "ALTER APPLICATION ROLE", introducer: "WITH", required: true);
					return true;

				case Statement.CreateLogin create:
					PutCreateLogin(create);
					return true;

				case Statement.AlterLogin alter:
					PutAlterLogin(alter);
					return true;

				case Statement.CreateUser create:
					PutCreateUser(create);
					return true;

				case Statement.AlterUser alter:
					Word("ALTER USER");
					PutIdentifier(alter.Name);
					PutSettings(alter, alter.Options, AlterUserSettings, "ALTER USER", introducer: "WITH", required: true);
					return true;

				case Statement.AlterSchema alter:
					Word("ALTER SCHEMA");
					PutIdentifier(alter.Name);
					Word("TRANSFER");
					PutClass(alter.Class);
					PutName(alter.Securable);
					return true;

				case Statement.AlterAuthorization alter:
					Word("ALTER AUTHORIZATION ON");
					PutClass(alter.Class);
					PutName(alter.Entity);
					Word("TO");

					switch (alter.To)
					{
						case AuthorizationTo.Principal principal:
							PutIdentifier(principal.Name);
							break;

						case AuthorizationTo.SchemaOwner:
							Word("SCHEMA OWNER");
							break;

						default:
							throw Unwritten(alter.To);
					}

					return true;

				case Statement.CreateAsymmetricKey create:
					PutCreateAsymmetricKey(create);
					return true;

				case Statement.AlterAsymmetricKey alter:
					PutAlterAsymmetricKey(alter);
					return true;

				case Statement.CreateSymmetricKey create:
					PutCreateSymmetricKey(create);
					return true;

				case Statement.AlterSymmetricKey alter:
					PutAlterSymmetricKey(alter);
					return true;

				case Statement.CreateCertificate create:
					PutCreateCertificate(create);
					return true;

				case Statement.AlterCertificate alter:
					PutAlterCertificate(alter);
					return true;

				case Statement.CreateMasterKey create:
					Word("CREATE MASTER KEY");
					PutEncryptionPassword(create, create.EncryptedBy, "CREATE MASTER KEY");
					return true;

				case Statement.AlterMasterKey alter:
					PutAlterMasterKey(alter);
					return true;

				case Statement.AlterServiceMasterKey alter:
					PutAlterServiceMasterKey(alter);
					return true;

				case Statement.CreateDatabaseEncryptionKey create:
					Word("CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM");
					Word("=");
					Word(AlgorithmText(create.Algorithm));
					PutServerEncryption(create, create.By, "CREATE DATABASE ENCRYPTION KEY");
					return true;

				case Statement.AlterDatabaseEncryptionKey alter:
					PutAlterDatabaseEncryptionKey(alter);
					return true;

				case Statement.CreateColumnEncryptionKey create:
					if (create.Values.Count == 0)
						throw Empty(create, "Values");

					Word("CREATE COLUMN ENCRYPTION KEY");
					PutIdentifier(create.Name);
					Word("WITH VALUES");
					Each(create.Values, value => PutColumnKeyValue(value, 3, "CREATE COLUMN ENCRYPTION KEY"));
					return true;

				case Statement.AlterColumnEncryptionKey alter:
					PutAlterColumnEncryptionKey(alter);
					return true;

				case Statement.CreateColumnMasterKey create:
					PutCreateColumnMasterKey(create);
					return true;

				case Statement.CreateCredential create:
					PutCredential(create, "CREATE CREDENTIAL", create.Name, create.Identity, create.Secret, create.Provider);
					return true;

				case Statement.AlterCredential alter:
					PutCredential(alter, "ALTER CREDENTIAL", alter.Name, alter.Identity, alter.Secret, alter.Provider);
					return true;

				case Statement.CreateDatabaseScopedCredential create:
					PutCredential(create, "CREATE DATABASE SCOPED CREDENTIAL", create.Name, create.Identity, create.Secret, null);
					return true;

				case Statement.AlterDatabaseScopedCredential alter:
					PutCredential(alter, "ALTER DATABASE SCOPED CREDENTIAL", alter.Name, alter.Identity, alter.Secret, null);
					return true;

				case Statement.CreateSecurityPolicy create:
					PutCreateSecurityPolicy(create);
					return true;

				case Statement.AlterSecurityPolicy alter:
					PutAlterSecurityPolicy(alter);
					return true;

				case Statement.CreateCryptographicProvider create:
					Word("CREATE CRYPTOGRAPHIC PROVIDER");
					PutIdentifier(create.Name);
					Word("FROM FILE");
					Word("=");
					PutQuoted(create, create.File, "CREATE CRYPTOGRAPHIC PROVIDER");
					return true;

				case Statement.AlterCryptographicProvider alter:
					PutAlterCryptographicProvider(alter);
					return true;

				case Statement.OpenSymmetricKey open:
					Word("OPEN SYMMETRIC KEY");
					PutIdentifier(open.Name);
					Word("DECRYPTION BY");
					PutEncryptor(open, open.DecryptedBy, EncryptorPlace.Open, "OPEN SYMMETRIC KEY");
					return true;

				case Statement.OpenMasterKey open:
					Word("OPEN MASTER KEY DECRYPTION BY");
					PutPasswordEncryptor(open, open.DecryptedBy, "OPEN MASTER KEY");
					return true;

				case Statement.CloseSymmetricKey close:
					Word("CLOSE SYMMETRIC KEY");
					PutIdentifier(close.Name);
					return true;

				case Statement.CloseAllSymmetricKeys:
					Word("CLOSE ALL SYMMETRIC KEYS");
					return true;

				case Statement.CloseMasterKey:
					Word("CLOSE MASTER KEY");
					return true;

				case Statement.AddSensitivityClassification add:
					PutClassification(add);
					return true;

				case Statement.AddSignature add:
					PutAddSignature(add);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// The class written before a securable, <c>OBJECT::</c>: the grammar's <c>SecurableClass</c>.
		/// </summary>
		void PutClass(SecurableClass? kind)
		{
			if (kind is not { } one)
				return;

			Word(PermissionWords.Text(one) ?? throw NoText(one));
			Tight("::");
			Hold();
		}

		void PutRoleAction(ISqlNode owner, RoleAction action)
		{
			switch (action)
			{
				case RoleAction.AddMember add:
					Word("ADD MEMBER");
					PutIdentifier(add.Member);
					break;

				case RoleAction.DropMember drop:
					Word("DROP MEMBER");
					PutIdentifier(drop.Member);
					break;

				case RoleAction.WithName rename:
					Word("WITH NAME");
					Word("=");
					PutIdentifier(rename.NewName);
					break;

				default:
					throw Unwritten(action);
			}
		}

		/// <summary>
		/// What the standard's GRANT, REVOKE and the rest write for a statement, held to what T-SQL's grammar reads in the
		/// same place (<c>PermissionStatement</c>): the standard's arm calls it first, and in the standard's own reading
		/// it asks nothing.
		/// </summary>
		void CheckSecurity(ISqlNode node)
		{
			if (!transactSql)
				return;

			switch (node)
			{
				case Statement.Grant { Body: GrantBody.Privileges privileges } grant:
					CheckPermissions(grant, privileges.Items, privileges.Object, privileges.Grantees);

					if (privileges.HierarchyOption)
						throw Misplaced(grant, "HierarchyOption", "in T-SQL's GRANT, which reads only WITH GRANT OPTION");

					if (privileges.GrantedBy is not null)
						throw Misplaced(grant, "GrantedBy", "in T-SQL's GRANT, which reads only AS principal");

					break;

				case Statement.Grant grant:
					throw Misplaced(grant, "role names", "in T-SQL's GRANT, which grants permissions and reads no role (PermissionStatement)");

				case Statement.Deny deny:
					CheckPermissions(deny, deny.Items, deny.Object, deny.Grantees);
					break;

				case Statement.Revoke { Body: RevokeBody.Privileges revoked } revoke:
					CheckPermissions(revoke, revoked.Items, revoked.Object, revoked.Grantees);

					if (revoked.Option == RevokeOption.HierarchyOptionFor)
						throw Misplaced(revoke, "HierarchyOptionFor", "in T-SQL's REVOKE, which reads only GRANT OPTION FOR");

					if (revoked.GrantedBy is not null)
						throw Misplaced(revoke, "GrantedBy", "in T-SQL's REVOKE, which reads only AS principal");

					if (revoked.Behavior == DropBehavior.Restrict)
						throw Misplaced(revoke, "RESTRICT", "in T-SQL's REVOKE, which reads CASCADE or nothing");

					break;

				case Statement.Revoke revoke:
					throw Misplaced(revoke, "role names", "in T-SQL's REVOKE, which takes back permissions and reads no role (PermissionStatement)");

				case Statement.CreateRole role:
					if (role.Admin is not null)
						throw Misplaced(role, "Admin", "in T-SQL's CREATE ROLE, which reads AUTHORIZATION owner and no WITH ADMIN");

					break;

				case Statement.CreateSchema schema:
					CheckSchema(schema);
					break;
			}
		}

		/// <summary>
		/// T-SQL's <c>CREATE SCHEMA</c>: a name of one part and the owner, one at least, and the elements the grammar lists
		/// (<c>SchemaElement</c>): a table, a routine, a view or a trigger, or a permission statement.
		/// </summary>
		void CheckSchema(Statement.CreateSchema schema)
		{
			if (schema.Name is null && schema.Authorization is null)
				throw Empty(schema, "Name", "Authorization");

			if (schema.Name is { Count: not 1 })
				throw Misplaced(schema, "Name", "of more than one part, which T-SQL's schema name is not (PrincipalStatement reads an Identifier)");

			if (schema.PathFirst || schema.DefaultCharacterSet is not null || schema.Path is not null)
				throw Misplaced(schema, "a character set or a path", "in T-SQL's CREATE SCHEMA, which reads an owner and elements only");

			for (var at = 0; at < schema.Elements.Count; at++)
			{
				var element = schema.Elements[at];

				// A procedure's or a trigger's body is a statement list that reads on to the end: what follows would be part of it.
				if (at < schema.Elements.Count - 1 && element is Statement.CreateTrigger or Statement.CreateRoutine { Definition.Kind: RoutineKind.Procedure })
					throw Misplaced(schema, "a procedure or a trigger that is not the last element", "in T-SQL's CREATE SCHEMA, where its body (StatementList) reads the elements after it");

				// SchemaElement is CreateTableStatement, CreateRoutine or PermissionStatement.
				if (element is not (Statement.CreateTable or Statement.CreateView or Statement.CreateRoutine or Statement.CreateTrigger or Statement.Grant or Statement.Deny or Statement.Revoke))
					throw Misplaced(schema, "an element of another kind", "in T-SQL's CREATE SCHEMA, whose elements are a table, a routine, a view, a trigger or a permission (SchemaElement)");
			}
		}

		/// <summary>
		/// The parts GRANT, DENY and REVOKE share in T-SQL: permissions the catalogue knows, of the securable's class, to
		/// principals, as <c>PermissionStatement</c> reads them.
		/// </summary>
		void CheckPermissions(ISqlNode statement, SqlList<Privilege> items, PrivilegeObject? on, SqlList<Grantee> grantees)
		{
			if (items.Count == 0)
				throw Empty(statement, "Items");

			if (grantees.Count == 0)
				throw Empty(statement, "Grantees");

			string? named = null;

			switch (on)
			{
				case null:
					break;

				// `ON TABLE t`, `ON DOMAIN d`, a routine's designator: the standard's words, which T-SQL's PermissionOn does not read.
				case PrivilegeObject.Named { Kind: not PrivilegeObjectKind.Table } or PrivilegeObject.Named { TableKeyword: true } or PrivilegeObject.Named { Routine: not null }:
					throw Misplaced(statement, "the standard's securable", "in T-SQL, whose ON reads a class and a name (PermissionOn)");

				// T-SQL reads a name with no class as OBJECT.
				case PrivilegeObject.Named:
					named = "OBJECT";
					break;

				case PrivilegeObject.Classed classed:
					named = PermissionWords.Text(classed.Class) ?? throw NoText(classed.Class);
					break;

				default:
					throw Unwritten(on);
			}

			foreach (var privilege in items)
			{
				if (privilege.Methods.Count > 0)
					throw Misplaced(privilege, "Methods", "in T-SQL, whose permission takes a column list and no routine (Permission)");

				// The engine reads the pair while parsing: `GRANT SELECT ON ENDPOINT::x` is Msg 102 (the guard of PermissionStatement).
				if (named is not null && !PermissionCatalogue.Allows(named, Spelled(privilege)))
					throw Misplaced(privilege, "Kind", $"on a {named}, whose permissions do not include it (Msg 102 at the permission)");
			}
		}

		// ── Settings ──────────────────────────────────────────────────────────────

		/// <summary>
		/// The forms of value a setting is read with, as the grammar's rules spell them: <c>Identifier</c>, <c>Quoted</c>,
		/// <c>TSql.BinaryLiteral</c>, <c>OnOff</c>, <c>Sql92.Lexical.Digits</c>, <c>NULL</c>; or none.
		/// </summary>
		[Flags]
		enum Forms
		{
			None   = 0,
			Name   = 1,
			String = 2,
			Hex    = 4,
			OnOff  = 8,
			Digits = 16,
			Null   = 32,
			Word   = 64,
		}

		/// <summary>One setting a place reads: the forms of its value, whether the place lets it stand once only, and the words of its value where it is one of a closed list.</summary>
		readonly record struct Entry(Forms Value, bool Once = false, string[]? Words = null);

		static readonly Entry AName   = new(Forms.Name);
		static readonly Entry AString = new(Forms.String);
		static readonly Entry AHex    = new(Forms.Hex);
		static readonly Entry AnOnOff = new(Forms.OnOff);

		/// <summary>CreateRoleOption: <c>PASSWORD = 'p'</c>, <c>DEFAULT_SCHEMA = s</c>.</summary>
		static readonly Dictionary<string, Entry> ApplicationRoleSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PASSWORD"]       = AString,
			["DEFAULT_SCHEMA"] = AName,
		};

		/// <summary>AlterRoleOption: CreateRoleOption, <c>NAME</c>, <c>LOGIN</c> and <c>DEFAULT_LANGUAGE</c>, all names.</summary>
		static readonly Dictionary<string, Entry> AlterApplicationRoleSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PASSWORD"]         = AString,
			["DEFAULT_SCHEMA"]   = AName,
			["NAME"]             = AName,
			["LOGIN"]            = AName,
			["DEFAULT_LANGUAGE"] = AName,
		};

		/// <summary>LoginPlace: a login's database and language, both names.</summary>
		static readonly Dictionary<string, Entry> LoginPlaceSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["DEFAULT_DATABASE"] = AName,
			["DEFAULT_LANGUAGE"] = AName,
		};

		/// <summary>CreateLoginOption, which follows a password.</summary>
		static readonly Dictionary<string, Entry> CreateLoginSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["SID"]              = AHex,
			["DEFAULT_DATABASE"] = AName,
			["DEFAULT_LANGUAGE"] = AName,
			["CHECK_EXPIRATION"] = AnOnOff,
			["CHECK_POLICY"]     = AnOnOff,
			["CREDENTIAL"]       = AName,
		};

		/// <summary>ExternalLoginOption: the Entra login's settings.</summary>
		static readonly Dictionary<string, Entry> ExternalLoginSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["OBJECT_ID"]        = AString,
			["SID"]              = AHex,
			["TYPE"]             = AName,
			["DEFAULT_DATABASE"] = AName,
			["DEFAULT_LANGUAGE"] = AName,
		};

		/// <summary>AlterLoginOption. A password keeps the words after its value as its own options (<see cref="PutPasswordWords"/>).</summary>
		static readonly Dictionary<string, Entry> AlterLoginSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PASSWORD"]         = new(Forms.String | Forms.Hex),
			["DEFAULT_DATABASE"] = AName,
			["DEFAULT_LANGUAGE"] = AName,
			["NAME"]             = AName,
			["CHECK_EXPIRATION"] = AnOnOff,
			["CHECK_POLICY"]     = AnOnOff,
			["CREDENTIAL"]       = AName,
			["NO CREDENTIAL"]    = new(Forms.None),
			["TYPE"]             = AName,
		};

		/// <summary>UserOption, which CREATE USER reads whatever the source.</summary>
		static readonly Dictionary<string, Entry> CreateUserSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["PASSWORD"]                            = AString,
			["DEFAULT_SCHEMA"]                      = AName,
			["DEFAULT_LANGUAGE"]                    = new(Forms.Name | Forms.Digits),
			["SID"]                                 = AHex,
			["ALLOW_ENCRYPTED_VALUE_MODIFICATIONS"] = AnOnOff,
			["TYPE"]                                = AName,
			["OBJECT_ID"]                           = new(Forms.String | Forms.Name),
		};

		/// <summary>AlterUserOption. A password keeps its <c>OLD_PASSWORD</c> as its own option.</summary>
		static readonly Dictionary<string, Entry> AlterUserSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["NAME"]                                = AName,
			["DEFAULT_SCHEMA"]                      = new(Forms.Name | Forms.Null),
			["LOGIN"]                               = AName,
			["PASSWORD"]                            = AString,
			["DEFAULT_LANGUAGE"]                    = new(Forms.Name | Forms.Digits),
			["ALLOW_ENCRYPTED_VALUE_MODIFICATIONS"] = AnOnOff,
		};

		/// <summary>
		/// A list of settings, <c>WITH a = v, b = w</c>, held to the place's catalogue: each setting a name the place reads, once
		/// where the place reads it once, with a value of a form it reads for that name and nothing the generic option has more.
		/// </summary>
		/// <param name="owner">The statement.</param>
		/// <param name="options">The settings.</param>
		/// <param name="catalogue">The place's settings, by the words of the name.</param>
		/// <param name="place">Where the settings stand, for the refusal.</param>
		/// <param name="introducer">The word before the first, or null where the caller has written it.</param>
		/// <param name="required">Whether the place reads one at least.</param>
		/// <param name="bracketed">Whether the settings stand in brackets.</param>
		void PutSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, Entry> catalogue, string place, string? introducer, bool required, bool bracketed = false)
		{
			if (options.Count == 0)
			{
				if (required)
					throw Empty(owner, "Options");

				return;
			}

			if (introducer is not null)
				Word(introducer);

			if (bracketed)
				Open();

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			for (var at = 0; at < options.Count; at++)
			{
				if (at > 0)
					Tight(",");

				PutEntry(options[at], catalogue, place, seen);
			}

			if (bracketed)
				Close();
		}

		/// <summary>
		/// One setting of a catalogue: its name and further words, <c>=</c> and its value, and the options a password keeps.
		/// </summary>
		void PutEntry(Option option, Dictionary<string, Entry> catalogue, string place, HashSet<string> seen)
		{
			var key = KeyOf(option, place);

			if (!catalogue.TryGetValue(key, out var setting))
				throw Misplaced(option, "Name", $"in {place}, which reads no setting {key}");

			if (!seen.Add(key) && setting.Once)
				throw Misplaced(option, "Name", $"twice in {place}, which reads {key} once");

			if (option.EqualsOmitted)
				throw Misplaced(option, "EqualsOmitted", $"in {place}, whose settings are written with = or no value, never the omitted =");

			if (option.Order is not null || option.Partitions is not null || option.Place is not null)
				throw Misplaced(option, "an order, partitions or a place", $"in {place}, whose settings are a name and a value");

			PutKeyWord(option.Name);

			foreach (var word in option.Words)
				PutKeyWord(word);

			if (setting.Value == Forms.None)
			{
				if (option.Value is not null || option.Options.Count > 0)
					throw Misplaced(option, "a value", $"after {key} in {place}, which is written alone");

				return;
			}

			if (option.Value is not { } value)
				throw Empty(option, "Value");

			Word("=");
			PutValue(option, value, setting, key, place);

			if (key == "PASSWORD" && place is "ALTER LOGIN" or "ALTER USER")
				PutPasswordWords(option, value, place);
			else if (option.Options.Count > 0)
				throw Misplaced(option, "Options", $"after {key} in {place}, which reads none");
		}

		/// <summary>
		/// The words after a password's value, which the grammar reads with no comma: <c>OLD_PASSWORD = 'q'</c> alone (a login's
		/// and a user's, both strings), or, for a login, <c>HASHED</c>, <c>MUST_CHANGE</c> and <c>UNLOCK</c> each once in any order,
		/// <c>HASHED</c> among them where the value is a hash in hex.
		/// </summary>
		void PutPasswordWords(Option password, Expression value, string place)
		{
			var words = password.Options;

			if (place == "ALTER USER")
			{
				if (words.Count == 0)
					return;

				if (words is not [{ Name: var old, Value: LiteralValue.String, Words.Count: 0, Options.Count: 0 } oldPassword] || !Plain(old, "OLD_PASSWORD"))
					throw Misplaced(password, "Options", "after PASSWORD in ALTER USER, which reads OLD_PASSWORD = 'p' and nothing else (AlterUserOption)");

				PutOldPassword(oldPassword);

				return;
			}

			if (words.Count == 1 && Plain(words[0].Name, "OLD_PASSWORD"))
			{
				if (value is not LiteralValue.String || words[0] is not { Value: LiteralValue.String, Words.Count: 0, Options.Count: 0 })
					throw Misplaced(password, "OLD_PASSWORD", "beside a password that is not a string, or with a value that is not one (AlterPassword reads two strings)");

				PutOldPassword(words[0]);

				return;
			}

			var kinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var word in words)
			{
				if (word is not { Value: null, Words.Count: 0, Options.Count: 0 } || !Plain(word.Name, "HASHED", "MUST_CHANGE", "UNLOCK") || !kinds.Add(word.Name.Text))
					throw Misplaced(password, "Options", "after PASSWORD in ALTER LOGIN, which reads OLD_PASSWORD = 'p', or HASHED, MUST_CHANGE and UNLOCK each once (AlterPassword)");

				Word(word.Name.Text);
			}

			if (value is LiteralValue.Binary && !kinds.Contains("HASHED"))
				throw Misplaced(password, "Value", "a hash in hex without HASHED, which AlterPassword wants beside it");
		}

		void PutOldPassword(Option old)
		{
			PutKeyWord(old.Name);
			Word("=");
			PutExpression(old.Value!);
		}

		/// <summary>The words a setting is named by, in capitals: <c>NO CREDENTIAL</c>.</summary>
		string KeyOf(Option option, string place)
		{
			if (option.Name.Style != IdentifierStyle.Regular || option.Name.UnicodeEscape is not null)
				throw Misplaced(option, "Name", $"that is not a plain word, in {place}");

			if (option.Words.Any(static word => word.Style != IdentifierStyle.Regular || word.UnicodeEscape is not null))
				throw Misplaced(option, "Words", $"that are not plain words, in {place}");

			// One word each: a word that holds a blank would read back as two.
			if (option.Words.Prepend(option.Name).Any(static word => word.Text.Length == 0 || word.Text.Any(char.IsWhiteSpace)))
				throw Misplaced(option, "Words", $"that hold a blank, in {place}, where a name is words and each is one");

			return string.Join(" ", option.Words.Select(static word => word.Text).Prepend(option.Name.Text)).ToUpperInvariant();
		}

		/// <summary>
		/// A setting's value, of a form the catalogue reads for its name: a name of one part, a string, a binary string written
		/// <c>0x…</c>, ON or OFF, a number of digits, or NULL.
		/// </summary>
		void PutValue(Option option, Expression value, Entry entry, string key, string place)
		{
			var forms = entry.Value;

			switch (value)
			{
				case Expression.Reference { Name: [{ } part] } when forms.HasFlag(Forms.OnOff) && Plain(part, "ON", "OFF"):
					PutKeyWord(part);
					return;

				case Expression.Reference { Name: [{ } part] } when forms.HasFlag(Forms.Word) && entry.Words is { } words && Plain(part, words):
					PutKeyWord(part);
					return;

				case Expression.Reference { Name: [{ } part] } when forms.HasFlag(Forms.Name):
					PutIdentifier(part);
					return;

				case LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } when forms.HasFlag(Forms.String):
				case LiteralValue.Binary { ZeroX: true } when forms.HasFlag(Forms.Hex):
				case LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger } numeric when forms.HasFlag(Forms.Digits) && numeric.Text.All(static digit => digit is >= '0' and <= '9') && numeric.Text.Length > 0:
				case LiteralValue.Null when forms.HasFlag(Forms.Null):
					PutExpression(value);
					return;
			}

			throw Misplaced(option, "Value", $"of a form {key} is not read with in {place}");
		}

		// ── Logins and users ──────────────────────────────────────────────────────

		void PutCreateLogin(Statement.CreateLogin create)
		{
			Word("CREATE LOGIN");
			PutIdentifier(create.Name);

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			switch (create.Source)
			{
				case LoginSource.Password password:
					Word("WITH PASSWORD");
					Word("=");

					switch (password.Value)
					{
						// CreatePassword: a string, which may say it is a hash, or a hash in hex, which has to.
						case LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null }:
							break;

						case LiteralValue.Binary { ZeroX: true } when password.Hashed:
							break;

						default:
							throw Misplaced(password, "Value", "that is a string, or a hash in hex with HASHED (CreatePassword)");
					}

					if (password.MustChangeFirst && !(password.Hashed && password.MustChange))
						throw Misplaced(password, "MustChangeFirst", "without both HASHED and MUST_CHANGE, whose order it is");

					PutExpression(password.Value);

					if (password.MustChangeFirst)
					{
						Word("MUST_CHANGE");
						Word("HASHED");
					}
					else
					{
						if (password.Hashed)
							Word("HASHED");

						if (password.MustChange)
							Word("MUST_CHANGE");
					}

					if (create.Options.Count > 0)
					{
						Tight(",");
						PutSettings(create, create.Options, CreateLoginSettings, "CREATE LOGIN WITH PASSWORD", introducer: null, required: false);
					}

					break;

				case LoginSource.Windows:
					Word("FROM WINDOWS");
					PutSettings(create, create.Options, LoginPlaceSettings, "CREATE LOGIN FROM WINDOWS", introducer: "WITH", required: false);
					break;

				case LoginSource.ExternalProvider:
					Word("FROM EXTERNAL PROVIDER");
					PutSettings(create, create.Options, ExternalLoginSettings, "CREATE LOGIN FROM EXTERNAL PROVIDER", introducer: "WITH", required: false);
					break;

				case LoginSource.Certificate certificate:
					Word("FROM CERTIFICATE");
					PutIdentifier(certificate.Name);
					PutKeyCredential(create, certificate.Credential);
					break;

				case LoginSource.AsymmetricKey key:
					Word("FROM ASYMMETRIC KEY");
					PutIdentifier(key.Name);
					PutKeyCredential(create, key.Credential);
					break;

				default:
					throw Unwritten(create.Source);
			}
		}

		void PutKeyCredential(Statement.CreateLogin create, Identifier? credential)
		{
			// KeyCredential is the only setting of a certificate's or a key's login.
			if (create.Options.Count > 0)
				throw Misplaced(create, "Options", "after FROM CERTIFICATE or FROM ASYMMETRIC KEY, whose one setting is its credential (KeyCredential)");

			if (credential is null)
				return;

			Word("WITH CREDENTIAL");
			Word("=");
			PutIdentifier(credential);
		}

		void PutAlterLogin(Statement.AlterLogin alter)
		{
			Word("ALTER LOGIN");
			PutIdentifier(alter.Name);

			switch (alter.Action)
			{
				case AlterLoginAction.Enable:
					Word("ENABLE");
					break;

				case AlterLoginAction.Disable:
					Word("DISABLE");
					break;

				case AlterLoginAction.With with:
					PutSettings(with, with.Options, AlterLoginSettings, "ALTER LOGIN", introducer: "WITH", required: true);
					break;

				case AlterLoginAction.AddCredential add:
					Word("ADD CREDENTIAL");
					PutIdentifier(add.Name);
					break;

				case AlterLoginAction.DropCredential drop:
					Word("DROP CREDENTIAL");
					PutIdentifier(drop.Name);
					break;

				default:
					throw Unwritten(alter.Action);
			}
		}

		void PutCreateUser(Statement.CreateUser create)
		{
			Word("CREATE USER");
			PutIdentifier(create.Name);

			switch (create.Source)
			{
				case null:
					break;

				case UserSource.Login login:
					Word(login.From ? "FROM LOGIN" : "FOR LOGIN");
					PutIdentifier(login.Name);
					break;

				case UserSource.Certificate certificate:
					Word(certificate.From ? "FROM CERTIFICATE" : "FOR CERTIFICATE");
					PutIdentifier(certificate.Name);
					break;

				case UserSource.AsymmetricKey key:
					Word(key.From ? "FROM ASYMMETRIC KEY" : "FOR ASYMMETRIC KEY");
					PutIdentifier(key.Name);
					break;

				case UserSource.ExternalProvider external:
					Word(external.From ? "FROM EXTERNAL PROVIDER" : "FOR EXTERNAL PROVIDER");
					break;

				case UserSource.WithoutLogin:
					Word("WITHOUT LOGIN");
					break;

				default:
					throw Unwritten(create.Source);
			}

			PutSettings(create, create.Options, CreateUserSettings, "CREATE USER", introducer: "WITH", required: false);
		}

		// ── Keys, certificates, credentials ──────────────────────────────────────

		static readonly string[] KeyAlgorithms =
		[
			"AES_128", "AES_192", "AES_256", "RSA_4096", "RSA_3072", "RSA_2048", "DES", "TRIPLE_DES", "TRIPLE_DES_3KEY", "RC2", "DESX",
			"RSA_1024", "RSA_512", "RC4", "RC4_128",
		];

		static readonly Entry AlgorithmEntry   = new(Forms.Word, Words: KeyAlgorithms);
		static readonly Entry DispositionEntry = new(Forms.Word, Words: ["CREATE_NEW", "OPEN_EXISTING"]);

		/// <summary>AlgorithmSetting: the one setting of a key made here.</summary>
		static readonly Dictionary<string, Entry> GeneratedKeySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ALGORITHM"] = AlgorithmEntry with { Once = true },
		};

		/// <summary>ProviderKeyOption and ProviderOption: what a provider's key takes.</summary>
		static readonly Dictionary<string, Entry> ProviderKeySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["ALGORITHM"]         = AlgorithmEntry,
			["PROVIDER_KEY_NAME"] = AString,
			["CREATION_DISPOSITION"] = DispositionEntry,
		};

		/// <summary>SymmetricOption, and ProviderOption's ALGORITHM.</summary>
		static readonly Dictionary<string, Entry> SymmetricKeySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["KEY_SOURCE"]           = AString,
			["ALGORITHM"]            = AlgorithmEntry,
			["IDENTITY_VALUE"]       = AString,
			["PROVIDER_KEY_NAME"]    = AString,
			["CREATION_DISPOSITION"] = DispositionEntry,
		};

		/// <summary>CertificateOption.</summary>
		static readonly Dictionary<string, Entry> CertificateSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["SUBJECT"]     = AString,
			["START_DATE"]  = AString,
			["EXPIRY_DATE"] = AString,
		};

		/// <summary>KeyPassword: the passwords of a private key.</summary>
		static readonly Dictionary<string, Entry> KeyPasswordSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["DECRYPTION BY PASSWORD"] = AString,
			["ENCRYPTION BY PASSWORD"] = AString,
		};

		/// <summary>PrivateKeyOption: a file, a binary string, or the passwords.</summary>
		static readonly Dictionary<string, Entry> PrivateKeySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["FILE"]                   = AString,
			["BINARY"]                 = AHex,
			["DECRYPTION BY PASSWORD"] = AString,
			["ENCRYPTION BY PASSWORD"] = AString,
		};

		/// <summary>ColumnKeyPart: what one value of a column encryption key holds.</summary>
		static readonly Dictionary<string, Entry> ColumnKeySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["COLUMN_MASTER_KEY"] = AName,
			["ALGORITHM"]         = AString,
			["ENCRYPTED_VALUE"]   = AHex,
		};

		/// <summary>MasterKeyProvider and MasterKeyPath.</summary>
		static readonly Dictionary<string, Entry> MasterKeyLocation = new(StringComparer.OrdinalIgnoreCase)
		{
			["KEY_STORE_PROVIDER_NAME"] = new(Forms.String, Once: true),
			["KEY_PATH"]                = new(Forms.String, Once: true),
		};

		/// <summary>ClassificationOption: each at most once.</summary>
		static readonly Dictionary<string, Entry> ClassificationSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["LABEL"]               = new(Forms.String, Once: true),
			["LABEL_ID"]            = new(Forms.String, Once: true),
			["INFORMATION_TYPE"]    = new(Forms.String, Once: true),
			["INFORMATION_TYPE_ID"] = new(Forms.String, Once: true),
			["RANK"]                = new(Forms.Word, Once: true, Words: ["NONE", "LOW", "MEDIUM", "HIGH", "CRITICAL"]),
		};

		/// <summary>PolicySetting.</summary>
		static readonly Dictionary<string, Entry> PolicySettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["STATE"]         = AnOnOff,
			["SCHEMABINDING"] = AnOnOff,
		};

		/// <summary>
		/// A string literal as the grammar's <c>Quoted</c> reads it: a character string or a national one, with no character set
		/// and no escape.
		/// </summary>
		void PutQuoted(ISqlNode owner, LiteralValue.String value, string where)
		{
			if (value is not { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null })
				throw Misplaced(owner, "a string of another kind", $"in {where}, which reads a character or national string (Quoted)");

			PutExpression(value);
		}

		/// <summary>
		/// A binary string as <c>TSql.BinaryLiteral</c> reads it: written <c>0x…</c>.
		/// </summary>
		void PutHex(ISqlNode owner, LiteralValue.Binary value, string where)
		{
			if (!value.ZeroX)
				throw Misplaced(owner, "a binary string written X'…'", $"in {where}, which reads 0x… (BinaryLiteral)");

			PutExpression(value);
		}

		/// <summary>
		/// <c>PASSWORD = 'p'</c>, the one thing some keys are locked with (<c>PasswordEncryptor</c>).
		/// </summary>
		void PutPasswordEncryptor(ISqlNode owner, KeyEncryptor.Password password, string where)
		{
			Word("PASSWORD");
			Word("=");
			PutQuoted(owner, password.Value, where);
		}

		/// <summary>The places a key's encryptor stands in, which read some of the family's members.</summary>
		enum EncryptorPlace
		{
			/// <summary>KeyEncryptor: <c>ENCRYPTION BY</c> of CREATE and ALTER SYMMETRIC KEY: a certificate, a password, a symmetric key or an asymmetric key.</summary>
			Symmetric,

			/// <summary>MasterKeyChange: a password or the service master key.</summary>
			MasterKey,

			/// <summary>KeyDecryptor: <c>DECRYPTION BY</c> of OPEN SYMMETRIC KEY: a name of one part, and a password with a certificate or an asymmetric key.</summary>
			Open,

			/// <summary>ServerEncryption: a server's certificate or asymmetric key.</summary>
			Server,
		}

		void PutEncryptor(ISqlNode owner, KeyEncryptor encryptor, EncryptorPlace place, string where)
		{
			switch (encryptor)
			{
				case KeyEncryptor.Password password when place is EncryptorPlace.Symmetric or EncryptorPlace.MasterKey or EncryptorPlace.Open:
					PutPasswordEncryptor(owner, password, where);
					break;

				case KeyEncryptor.Certificate certificate when place is EncryptorPlace.Symmetric or EncryptorPlace.Open or EncryptorPlace.Server:
					CheckEncryptor(certificate, certificate.Name, certificate.WithPassword, certificate.Server, place, where);
					Word(certificate.Server ? "SERVER CERTIFICATE" : "CERTIFICATE");
					PutName(certificate.Name);
					PutWithPassword(owner, certificate.WithPassword, where);
					break;

				case KeyEncryptor.AsymmetricKey key when place is EncryptorPlace.Symmetric or EncryptorPlace.Open or EncryptorPlace.Server:
					CheckEncryptor(key, key.Name, key.WithPassword, key.Server, place, where);
					Word(key.Server ? "SERVER ASYMMETRIC KEY" : "ASYMMETRIC KEY");
					PutName(key.Name);
					PutWithPassword(owner, key.WithPassword, where);
					break;

				case KeyEncryptor.SymmetricKey key when place is EncryptorPlace.Symmetric or EncryptorPlace.Open:
					if (place == EncryptorPlace.Open && key.Name.Count != 1)
						throw Misplaced(key, "Name", $"of more than one part in {where}, whose KeyDecryptor reads an Identifier");

					Word("SYMMETRIC KEY");
					PutName(key.Name);
					break;

				case KeyEncryptor.ServiceMasterKey when place == EncryptorPlace.MasterKey:
					Word("SERVICE MASTER KEY");
					break;

				default:
					throw Misplaced(encryptor, "this encryptor", $"in {where}, which does not read it");
			}
		}

		/// <summary>
		/// What a certificate or an asymmetric key may carry where it stands: the server's word only for the database encryption key
		/// and always there, a password only where a key is opened, a name of one part where the grammar reads an Identifier.
		/// </summary>
		void CheckEncryptor(KeyEncryptor encryptor, QualifiedName name, LiteralValue.String? withPassword, bool server, EncryptorPlace place, string where)
		{
			if (server != (place == EncryptorPlace.Server))
				throw Misplaced(encryptor, "Server", $"{(server ? "outside" : "missing in")} {where}, whose ServerEncryption wants SERVER and which reads it nowhere else");

			if (withPassword is not null && place != EncryptorPlace.Open)
				throw Misplaced(encryptor, "WithPassword", $"in {where}, which reads WITH PASSWORD only where a key is opened (KeyDecryptor)");

			if (place == EncryptorPlace.Open && name.Count != 1)
				throw Misplaced(encryptor, "Name", $"of more than one part in {where}, whose KeyDecryptor reads an Identifier");
		}

		void PutWithPassword(ISqlNode owner, LiteralValue.String? password, string where)
		{
			if (password is null)
				return;

			Word("WITH PASSWORD");
			Word("=");
			PutQuoted(owner, password, where);
		}

		/// <summary>
		/// <c>ENCRYPTION BY encryptor, …</c> after a key's other parts, where any is written.
		/// </summary>
		void PutEncryptedBy(ISqlNode owner, SqlList<KeyEncryptor> by, EncryptorPlace place, string where, string introducer = "ENCRYPTION BY")
		{
			if (by.Count == 0)
				return;

			Word(introducer);
			Each(by, one => PutEncryptor(owner, one, place, where));
		}

		void PutEncryptionPassword(ISqlNode owner, KeyEncryptor.Password? password, string where)
		{
			if (password is null)
				return;

			Word("ENCRYPTION BY");
			PutPasswordEncryptor(owner, password, where);
		}

		void PutCreateAsymmetricKey(Statement.CreateAsymmetricKey create)
		{
			const string where = "CREATE ASYMMETRIC KEY";

			Word(where);
			PutIdentifier(create.Name);
			PutAuthorization(create.Authorization);

			switch (create.Source)
			{
				// AsymmetricBody: a key made here takes an algorithm at most.
				case null:
					PutSettings(create, create.Options, GeneratedKeySettings, where, introducer: "WITH", required: false);
					break;

				case AsymmetricKeySource.Provider provider:
					Word("FROM PROVIDER");
					PutIdentifier(provider.Name);
					PutSettings(create, create.Options, ProviderKeySettings, where + " FROM PROVIDER", introducer: "WITH", required: false);
					break;

				case AsymmetricKeySource.File file:
					NoSettings(create, create.Options, where + " FROM FILE");
					Word(file.Executable ? "FROM EXECUTABLE FILE" : "FROM FILE");
					Word("=");
					PutQuoted(create, file.Path, where);
					break;

				case AsymmetricKeySource.Assembly assembly:
					NoSettings(create, create.Options, where + " FROM ASSEMBLY");
					Word("FROM ASSEMBLY");
					PutName(assembly.Name);
					break;

				default:
					throw Unwritten(create.Source);
			}

			PutEncryptionPassword(create, create.EncryptedBy, where);
		}

		/// <summary>The settings of a place that reads none.</summary>
		void NoSettings(ISqlNode owner, SqlList<Option> options, string place)
		{
			if (options.Count > 0)
				throw Misplaced(owner, "Options", $"in {place}, which reads no setting");
		}

		void PutAlterAsymmetricKey(Statement.AlterAsymmetricKey alter)
		{
			const string where = "ALTER ASYMMETRIC KEY";

			Word(where);
			PutIdentifier(alter.Name);

			switch (alter.Change)
			{
				case AsymmetricKeyChange.RemovePrivateKey:
					Word("REMOVE PRIVATE KEY");
					break;

				case AsymmetricKeyChange.WithPrivateKey with:
					Word("WITH PRIVATE KEY");
					PutSettings(with, with.Options, KeyPasswordSettings, where + " WITH PRIVATE KEY", introducer: null, required: true, bracketed: true);
					break;

				case AsymmetricKeyChange.AttestedBy attested:
					Word("ATTESTED BY");
					PutQuoted(attested, attested.Attestation, where);
					break;

				case AsymmetricKeyChange.RemoveAttestedOption:
					Word("REMOVE ATTESTED OPTION");
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutCreateSymmetricKey(Statement.CreateSymmetricKey create)
		{
			const string where = "CREATE SYMMETRIC KEY";

			Word(where);
			PutIdentifier(create.Name);
			PutAuthorization(create.Authorization);

			// SymmetricBody: a key a provider holds may say nothing more; one made here says at least one setting and is locked.
			if (create.Provider is { } provider)
			{
				Word("FROM PROVIDER");
				PutIdentifier(provider);
				PutSettings(create, create.Options, SymmetricKeySettings, where + " FROM PROVIDER", introducer: "WITH", required: false);
			}
			else
			{
				if (create.EncryptedBy.Count == 0)
					throw Empty(create, "EncryptedBy");

				PutSettings(create, create.Options, SymmetricKeySettings, where, introducer: "WITH", required: true);
			}

			PutEncryptedBy(create, create.EncryptedBy, EncryptorPlace.Symmetric, where);
		}

		void PutAlterSymmetricKey(Statement.AlterSymmetricKey alter)
		{
			const string where = "ALTER SYMMETRIC KEY";

			Word(where);
			PutIdentifier(alter.Name);

			switch (alter.Change)
			{
				case SymmetricKeyChange.AddEncryption add:
					if (add.By.Count == 0)
						throw Empty(add, "By");

					Word("ADD");
					PutEncryptedBy(add, add.By, EncryptorPlace.Symmetric, where);
					break;

				case SymmetricKeyChange.DropEncryption drop:
					if (drop.By.Count == 0)
						throw Empty(drop, "By");

					Word("DROP");
					PutEncryptedBy(drop, drop.By, EncryptorPlace.Symmetric, where);
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutCreateCertificate(Statement.CreateCertificate create)
		{
			const string where = "CREATE CERTIFICATE";

			Word(where);
			PutIdentifier(create.Name);
			PutAuthorization(create.Authorization);

			switch (create.Source)
			{
				// CertificateBody: a certificate made here takes its settings in any order, and at least one.
				case null:
					PutEncryptionPassword(create, create.EncryptedBy, where);
					PutSettings(create, create.Options, CertificateSettings, where, introducer: "WITH", required: true);
					break;

				// A certificate read from somewhere takes its private key's settings, and is locked by nothing.
				default:
					if (create.EncryptedBy is not null)
						throw Misplaced(create, "EncryptedBy", "beside a source, which CertificateBody reads without ENCRYPTION BY");

					NoSettings(create, create.Options, where + " FROM");
					Word("FROM");
					PutCertificateSource(create.Source, where);
					break;
			}

			if (create.ActiveForBeginDialog is { } active)
			{
				Word("ACTIVE FOR BEGIN_DIALOG");
				Word("=");
				Word(active ? "ON" : "OFF");
			}
		}

		void PutCertificateSource(CertificateSource source, string where)
		{
			switch (source)
			{
				case CertificateSource.Assembly assembly:
					Word("ASSEMBLY");
					PutName(assembly.Name);
					break;

				case CertificateSource.File file:
					Word(file.Executable ? "EXECUTABLE FILE" : "FILE");
					Word("=");
					PutQuoted(file, file.Path, where);

					if (file.PrivateKey.Count == 0 && file.Format is not null)
						throw Misplaced(file, "Format", "without a private key, which CertificateSource reads it before");

					PutPrivateKey(file, file.PrivateKey, file.Format, where);
					break;

				case CertificateSource.Binary binary:
					Word("BINARY");
					Word("=");
					PutHex(binary, binary.Value, where);
					PutPrivateKey(binary, binary.PrivateKey, null, where);
					break;

				default:
					throw Unwritten(source);
			}
		}

		/// <summary>
		/// <c>WITH [FORMAT = 'f',] PRIVATE KEY (…)</c> after a certificate's file or binary string, where a private key is written.
		/// </summary>
		void PutPrivateKey(ISqlNode owner, SqlList<Option> settings, LiteralValue.String? format, string where)
		{
			if (settings.Count == 0)
				return;

			Word("WITH");

			if (format is not null)
			{
				Word("FORMAT");
				Word("=");
				PutQuoted(owner, format, where);
				Tight(",");
			}

			Word("PRIVATE KEY");
			PutSettings(owner, settings, PrivateKeySettings, where + " PRIVATE KEY", introducer: null, required: true, bracketed: true);
		}

		void PutAlterCertificate(Statement.AlterCertificate alter)
		{
			const string where = "ALTER CERTIFICATE";

			Word(where);
			PutIdentifier(alter.Name);

			switch (alter.Change)
			{
				case CertificateChange.RemovePrivateKey:
					Word("REMOVE PRIVATE KEY");
					break;

				case CertificateChange.WithPrivateKey with:
					Word("WITH PRIVATE KEY");
					PutSettings(with, with.Options, PrivateKeySettings, where + " WITH PRIVATE KEY", introducer: null, required: true, bracketed: true);
					break;

				case CertificateChange.ActiveForBeginDialog active:
					Word("WITH ACTIVE FOR BEGIN_DIALOG");
					Word("=");
					Word(active.On ? "ON" : "OFF");
					break;

				case CertificateChange.AttestedBy attested:
					Word("ATTESTED BY");
					PutQuoted(attested, attested.Attestation, where);
					break;

				case CertificateChange.RemoveAttestedOption:
					Word("REMOVE ATTESTED OPTION");
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutAlterMasterKey(Statement.AlterMasterKey alter)
		{
			const string where = "ALTER MASTER KEY";

			Word(where);

			switch (alter.Change)
			{
				case MasterKeyChange.Regenerate regenerate:
					if (regenerate.Force)
						Word("FORCE");

					Word("REGENERATE WITH ENCRYPTION BY");
					PutPasswordEncryptor(regenerate, regenerate.By, where);
					break;

				case MasterKeyChange.AddEncryption add:
					Word("ADD ENCRYPTION BY");
					PutEncryptor(add, add.By, EncryptorPlace.MasterKey, where);
					break;

				case MasterKeyChange.DropEncryption drop:
					Word("DROP ENCRYPTION BY");
					PutEncryptor(drop, drop.By, EncryptorPlace.MasterKey, where);
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutAlterServiceMasterKey(Statement.AlterServiceMasterKey alter)
		{
			const string where = "ALTER SERVICE MASTER KEY";

			Word(where);

			switch (alter.Change)
			{
				case ServiceMasterKeyChange.Regenerate regenerate:
					if (regenerate.Force)
						Word("FORCE");

					Word("REGENERATE");
					break;

				case ServiceMasterKeyChange.OldAccount old:
					PutAccount(old, "OLD", old.Account, old.Password, where);
					break;

				case ServiceMasterKeyChange.NewAccount @new:
					PutAccount(@new, "NEW", @new.Account, @new.Password, where);
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutAccount(ISqlNode owner, string which, LiteralValue.String account, LiteralValue.String password, string where)
		{
			Word("WITH " + which + "_ACCOUNT");
			Word("=");
			PutQuoted(owner, account, where);
			Tight(",");
			Word(which + "_PASSWORD");
			Word("=");
			PutQuoted(owner, password, where);
		}

		static string AlgorithmText(DatabaseKeyAlgorithm algorithm)
		{
			return algorithm switch
			{
				DatabaseKeyAlgorithm.Aes128       => "AES_128",
				DatabaseKeyAlgorithm.Aes192       => "AES_192",
				DatabaseKeyAlgorithm.Aes256       => "AES_256",
				DatabaseKeyAlgorithm.TripleDes3Key => "TRIPLE_DES_3KEY",
				_                                 => throw NoText(algorithm),
			};
		}

		void PutServerEncryption(ISqlNode owner, KeyEncryptor by, string where)
		{
			Word("ENCRYPTION BY");
			PutEncryptor(owner, by, EncryptorPlace.Server, where);
		}

		void PutAlterDatabaseEncryptionKey(Statement.AlterDatabaseEncryptionKey alter)
		{
			const string where = "ALTER DATABASE ENCRYPTION KEY";

			Word(where);

			switch (alter.Change)
			{
				case DatabaseEncryptionKeyChange.Regenerate regenerate:
					Word("REGENERATE WITH ALGORITHM");
					Word("=");
					Word(AlgorithmText(regenerate.Algorithm));
					break;

				case DatabaseEncryptionKeyChange.EncryptedBy encrypted:
					PutServerEncryption(encrypted, encrypted.By, where);
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		/// <summary>One value of a column encryption key: three settings in brackets, or the one a value is dropped by.</summary>
		void PutColumnKeyValue(ColumnKeyValue value, int count, string where)
		{
			if (value.Settings.Count != count)
				throw Misplaced(value, "Settings", $"of {value.Settings.Count}, where {where} reads {count} (ColumnKeyValue)");

			PutSettings(value, value.Settings, ColumnKeySettings, where, introducer: null, required: true, bracketed: true);
		}

		void PutAlterColumnEncryptionKey(Statement.AlterColumnEncryptionKey alter)
		{
			const string where = "ALTER COLUMN ENCRYPTION KEY";

			Word(where);
			PutIdentifier(alter.Name);

			switch (alter.Change)
			{
				case ColumnKeyChange.AddValue add:
					Word("ADD VALUE");
					PutColumnKeyValue(add.Value, 3, where);
					break;

				case ColumnKeyChange.DropValue drop:
					Word("DROP VALUE");
					PutColumnKeyValue(drop.Value, 1, where);
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		void PutCreateColumnMasterKey(Statement.CreateColumnMasterKey create)
		{
			const string where = "CREATE COLUMN MASTER KEY";

			Word(where);
			PutIdentifier(create.Name);
			Word("WITH");
			Open();

			// MasterKeyLocation: both parts, in either order; then MasterKeyEnclave, which stands only after them.
			var options = create.Options;
			var enclave = options.Count == 3 ? options[2] : null;

			if (options.Count is not (2 or 3) || (options.Count == 3 && !(Plain(enclave!.Name, "ENCLAVE_COMPUTATIONS") && enclave.Words.Count == 0)))
				throw Misplaced(create, "Options", "other than KEY_STORE_PROVIDER_NAME and KEY_PATH in either order, and ENCLAVE_COMPUTATIONS after them (MasterKeyLocation)");

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			PutEntry(options[0], MasterKeyLocation, where, seen);
			Tight(",");
			PutEntry(options[1], MasterKeyLocation, where, seen);

			if (enclave is not null)
			{
				Tight(",");

				if (enclave.Value is not null || enclave.EqualsOmitted || enclave.Options is not [{ Value: LiteralValue.Binary signature } one] || !Plain(one.Name, "SIGNATURE") || one.Words.Count != 0 || one.Options.Count != 0)
					throw Misplaced(enclave, "Options", "other than SIGNATURE = 0x… (MasterKeyEnclave)");

				PutKeyWord(enclave.Name);
				Open();
				PutKeyWord(one.Name);
				Word("=");
				PutHex(enclave, signature, where);
				Close();
			}

			Close();
		}

		void PutCredential(ISqlNode owner, string head, Identifier name, LiteralValue.String identity, LiteralValue.String? secret, Identifier? provider)
		{
			Word(head);
			PutIdentifier(name);
			Word("WITH IDENTITY");
			Word("=");
			PutQuoted(owner, identity, head);

			if (secret is not null)
			{
				Tight(",");
				Word("SECRET");
				Word("=");
				PutQuoted(owner, secret, head);
			}

			if (provider is not null)
			{
				Word("FOR CRYPTOGRAPHIC PROVIDER");
				PutIdentifier(provider);
			}
		}

		void PutAlterCryptographicProvider(Statement.AlterCryptographicProvider alter)
		{
			Word("ALTER CRYPTOGRAPHIC PROVIDER");
			PutIdentifier(alter.Name);

			switch (alter.Change)
			{
				case CryptographicProviderChange.Enable:
					Word("ENABLE");
					break;

				case CryptographicProviderChange.Disable:
					Word("DISABLE");
					break;

				case CryptographicProviderChange.FromFile file:
					Word("FROM FILE");
					Word("=");
					PutQuoted(file, file.Path, "ALTER CRYPTOGRAPHIC PROVIDER");
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		// ── Security policies ─────────────────────────────────────────────────────

		void PutPolicyPredicates(ISqlNode owner, SqlList<PolicyPredicate> predicates)
		{
			Each(predicates, one => PutPolicyPredicate(owner, one));
		}

		void PutPolicyPredicate(ISqlNode owner, PolicyPredicate predicate)
		{
			switch (predicate)
			{
				case PolicyPredicate.AddFilter add:
					CheckPolicyName(add, add.Function.Name);
					CheckPolicyName(add, add.On);
					Word("ADD FILTER PREDICATE");
					PutPolicyFunction(add.Function);
					Word("ON");
					PutName(add.On);
					break;

				case PolicyPredicate.AlterFilter alter:
					CheckPolicyName(alter, alter.Function.Name);
					CheckPolicyName(alter, alter.On);
					Word("ALTER FILTER PREDICATE");
					PutPolicyFunction(alter.Function);
					Word("ON");
					PutName(alter.On);
					break;

				case PolicyPredicate.DropFilter drop:
					CheckPolicyName(drop, drop.On);
					Word("DROP FILTER PREDICATE ON");
					PutName(drop.On);
					break;

				case PolicyPredicate.AddBlock add:
					CheckPolicyName(add, add.Function.Name);
					CheckPolicyName(add, add.On);
					Word("ADD BLOCK PREDICATE");
					PutPolicyFunction(add.Function);
					Word("ON");
					PutName(add.On);
					PutBlockOperation(add.When);
					break;

				case PolicyPredicate.AlterBlock alter:
					CheckPolicyName(alter, alter.Function.Name);
					CheckPolicyName(alter, alter.On);
					Word("ALTER BLOCK PREDICATE");
					PutPolicyFunction(alter.Function);
					Word("ON");
					PutName(alter.On);
					PutBlockOperation(alter.When);
					break;

				case PolicyPredicate.DropBlock drop:
					CheckPolicyName(drop, drop.On);
					Word("DROP BLOCK PREDICATE ON");
					PutName(drop.On);
					PutBlockOperation(drop.When);
					break;

				default:
					throw Unwritten(predicate);
			}
		}

		void PutBlockOperation(BlockOperation? when)
		{
			if (when is not { } one)
				return;

			Word(one switch
			{
				BlockOperation.AfterInsert  => "AFTER INSERT",
				BlockOperation.AfterUpdate  => "AFTER UPDATE",
				BlockOperation.BeforeUpdate => "BEFORE UPDATE",
				BlockOperation.BeforeDelete => "BEFORE DELETE",
				_                           => throw NoText(one),
			});
		}

		/// <summary>
		/// <c>dbo.f(c, …)</c>: a name and its arguments, with no variable among them (<c>Msg 112</c>, which the grammar's
		/// <c>WithoutVariable</c> holds the whole statement to).
		/// </summary>
		void PutPolicyFunction(PolicyFunction function)
		{
			PutName(function.Name);
			Call();
			Each(function.Arguments, PutExpression);
			Close();

			if (!Syntax.WithoutVariable(Sql2023Writer.Write(function.Arguments.Count == 0 ? new Expression.Reference(function.Name) : new Expression.Row(function.Arguments), transactSql: true)))
				throw Misplaced(function, "Arguments", "with a local variable, which a security policy does not take (Msg 112)");
		}

		/// <summary>
		/// A name in a policy's predicate: the grammar holds the whole tail to <c>WithoutVariable</c>, which takes an <c>@</c> inside
		/// a bracketed name for a variable as well.
		/// </summary>
		void CheckPolicyName(ISqlNode owner, QualifiedName name)
		{
			if (!Syntax.WithoutVariable(Dotted(name)))
				throw Misplaced(owner, "a name with an @", "in a security policy's predicate, whose tail takes no variable (Msg 112)");
		}

		void PutCreateSecurityPolicy(Statement.CreateSecurityPolicy create)
		{
			const string where = "CREATE SECURITY POLICY";

			Word(where);
			PutName(create.Name);
			PutPolicyPredicates(create, create.Predicates);
			PutSettings(create, create.Options, PolicySettings, where, introducer: "WITH", required: false, bracketed: true);

			if (create.NotForReplication)
				Word("NOT FOR REPLICATION");
		}

		void PutAlterSecurityPolicy(Statement.AlterSecurityPolicy alter)
		{
			Word("ALTER SECURITY POLICY");
			PutName(alter.Name);

			switch (alter.Change)
			{
				case SecurityPolicyChange.Predicates predicates:
					if (predicates.Items.Count == 0)
						throw Empty(predicates, "Items");

					PutPolicyPredicates(predicates, predicates.Items);
					break;

				case SecurityPolicyChange.State state:
					Word("WITH");
					Open();
					Word("STATE");
					Word("=");
					Word(state.On ? "ON" : "OFF");
					Close();
					break;

				case SecurityPolicyChange.AddNotForReplication:
					Word("ADD NOT FOR REPLICATION");
					break;

				case SecurityPolicyChange.DropNotForReplication:
					Word("DROP NOT FOR REPLICATION");
					break;

				default:
					throw Unwritten(alter.Change);
			}
		}

		// ── Classifications and signatures ────────────────────────────────────────

		void PutClassification(Statement.AddSensitivityClassification add)
		{
			if (add.Columns.Count == 0)
				throw Empty(add, "Columns");

			// ClassifiedColumn: a table and a column, or a schema, a table and a column.
			if (add.Columns.Any(static column => column.Count is not (2 or 3)))
				throw Misplaced(add, "Columns", "of other than two or three parts (ClassifiedColumn)");

			Word("ADD SENSITIVITY CLASSIFICATION TO");
			Each(add.Columns, PutName);
			PutSettings(add, add.Options, ClassificationSettings, "ADD SENSITIVITY CLASSIFICATION", introducer: "WITH", required: true, bracketed: true);
		}

		/// <summary>
		/// <c>[class ::] module</c> of a signed module, as <c>SignedModule</c> reads it: OBJECT, ASSEMBLY or DATABASE.
		/// </summary>
		void PutSignedModule(ISqlNode owner, SecurableClass? kind, QualifiedName module)
		{
			if (kind is { } written && written is not (SecurableClass.Object or SecurableClass.Assembly or SecurableClass.Database))
				throw Misplaced(owner, "Class", "other than OBJECT, ASSEMBLY or DATABASE, which SignedModule does not read");

			PutClass(kind);
			PutName(module);
		}

		void PutAddSignature(Statement.AddSignature add)
		{
			if (add.By.Count == 0)
				throw Empty(add, "By");

			Word(add.Counter ? "ADD COUNTER SIGNATURE TO" : "ADD SIGNATURE TO");
			PutSignedModule(add, add.Class, add.Module);
			Word("BY");
			PutSignedBy(add, add.By);
		}

		/// <summary>
		/// What signs a module, ADD SIGNATURE's and DROP SIGNATURE's alike (<c>SignatureCrypto</c>): a certificate or an asymmetric key
		/// by a name of one part, each with its proof, a symmetric key, or a password.
		/// </summary>
		void PutSignedBy(ISqlNode owner, SqlList<SignedBy> by)
		{
			Each(by, signed =>
			{
				switch (signed)
				{
					case SignedBy.Certificate { Name.Count: not 1 } or SignedBy.AsymmetricKey { Name.Count: not 1 }:
						throw Misplaced(owner, "By", "with a name of more than one part, which SignatureCrypto does not read");

					case SignedBy.Certificate certificate:
						Word("CERTIFICATE");
						PutName(certificate.Name);
						PutSignatureProof(owner, certificate.Proof);
						break;

					case SignedBy.AsymmetricKey key:
						Word("ASYMMETRIC KEY");
						PutName(key.Name);
						PutSignatureProof(owner, key.Proof);
						break;

					case SignedBy.SymmetricKey key:
						Word("SYMMETRIC KEY");
						PutIdentifier(key.Name);
						break;

					case SignedBy.Password password:
						Word("PASSWORD");
						Word("=");
						PutQuoted(owner, password.Value, "a signature");
						break;

					default:
						throw Unwritten(signed);
				}
			});
		}

		void PutSignatureProof(ISqlNode owner, SignatureProof? proof)
		{
			switch (proof)
			{
				case null:
					break;

				case SignatureProof.Password password:
					Word("WITH PASSWORD");
					Word("=");
					PutQuoted(owner, password.Value, "a signature");
					break;

				case SignatureProof.Signature signature:
					Word("WITH SIGNATURE");
					Word("=");
					PutHex(owner, signature.Value, "a signature");
					break;

				default:
					throw Unwritten(proof);
			}
		}
	}
}
