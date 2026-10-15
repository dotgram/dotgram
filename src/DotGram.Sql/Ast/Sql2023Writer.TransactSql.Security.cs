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

			foreach (var element in schema.Elements)
			{
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
		}

		/// <summary>One setting a place reads: the forms of its value, and whether the place lets it stand twice.</summary>
		readonly record struct Entry(Forms Value, bool Once = false);

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
		void PutSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, Entry> catalogue, string place, string? introducer, bool required)
		{
			if (options.Count == 0)
			{
				if (required)
					throw Empty(owner, "Options");

				return;
			}

			if (introducer is not null)
				Word(introducer);

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			for (var at = 0; at < options.Count; at++)
			{
				if (at > 0)
					Tight(",");

				PutEntry(options[at], catalogue, place, seen);
			}
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
			PutValue(option, value, setting.Value, key, place);

			if (key == "PASSWORD" && place != "CREATE APPLICATION ROLE" && place != "ALTER APPLICATION ROLE" && place != "CREATE USER")
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

			return string.Join(' ', option.Words.Select(static word => word.Text).Prepend(option.Name.Text)).ToUpperInvariant();
		}

		/// <summary>
		/// A setting's value, of a form the catalogue reads for its name: a name of one part, a string, a binary string written
		/// <c>0x…</c>, ON or OFF, a number of digits, or NULL.
		/// </summary>
		void PutValue(Option option, Expression value, Forms forms, string key, string place)
		{
			switch (value)
			{
				case Expression.Reference { Name: [{ } part] } when forms.HasFlag(Forms.OnOff) && Plain(part, "ON", "OFF"):
					PutKeyWord(part);
					return;

				case Expression.Reference { Name: [{ } part] } when forms.HasFlag(Forms.Name):
					PutIdentifier(part);
					return;

				case LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } when forms.HasFlag(Forms.String):
				case LiteralValue.Binary { ZeroX: true } when forms.HasFlag(Forms.Hex):
				case LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger } numeric when forms.HasFlag(Forms.Digits) && numeric.Text.All(char.IsAsciiDigit) && numeric.Text.Length > 0:
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
	}
}
