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
				Statement.AlterRole alter         => new Ast.Statement.AlterRole(Values.Part(alter, alter.Name), RoleAction(alter, alter.Tail)),
				Statement.CreateServerRole create => new Ast.Statement.CreateServerRole(Values.Part(create, create.Name)) { Authorization = Owner(create, create.Tail) },
				Statement.AlterServerRole alter   => new Ast.Statement.AlterServerRole(Values.Part(alter, alter.Name), RoleAction(alter, alter.Tail)),
				Statement.CreateApplicationRole create => ApplicationRole(create, create.Tail, create: true),
				Statement.AlterApplicationRole alter   => ApplicationRole(alter, alter.Tail, create: false),
				Statement.CreateLogin create      => CreateLogin(create),
				Statement.AlterLogin alter        => AlterLogin(alter),
				Statement.CreateUser create       => CreateUser(create),
				Statement.AlterUser alter         => AlterUser(alter),
				Statement.AlterSchema alter       => AlterSchema(alter),
				Statement.AlterAuthorization alter => AlterAuthorization(alter),
				Statement.Grant grant             => Grant(grant),
				Statement.Deny deny               => Deny(deny),
				Statement.Revoke revoke           => Revoke(revoke),
				_ when Owned(Owns, old)           => throw Inventory.Unconverted(old),
				_                                 => null,
			};

			return made is not null;
		}

		/// <summary>
		/// <c>CREATE SCHEMA [s] [AUTHORIZATION owner] [element …]</c>. The old tree keeps what follows the name as text:
		/// the owner is read from its tokens, and the elements, which are whole statements, are read again as a script is
		/// (docs/design/sql-tsql-bridge.md, "Definitions kept as text").
		/// </summary>
		static Ast.Statement Schema(Statement.SchemaDefinition old)
		{
			// `CREATE SCHEMA AUTHORIZATION o` names no schema, which the old tree keeps as an empty name.
			var made = new Ast.Statement.CreateSchema { Name = old.Name.Length > 0 ? Values.Name(old, old.Name) : null };

			if (old.Tail is not { } tail)
				return made;

			var tokens = Located(tail);
			var at     = 0;

			if (tokens.Count > 0 && tokens[0].Token.Kind == SqlTokens.Kind.Word && tokens[0].Token.Text.Equals("AUTHORIZATION", StringComparison.OrdinalIgnoreCase))
			{
				if (tokens.Count < 2 || tokens[1].Token.Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
					throw BridgeGap.Open(old, $"inventory Statement.SchemaDefinition (`{tail}`)");

				made = made with { Authorization = new Ast.AuthorizationIdentifier(Values.Part(old, tokens[1].Token.Text)) };
				at   = 2;
			}

			if (at == tokens.Count)
				return made;

			// The elements are the five statements a schema declares in its own breath: read as a script, the text after the owner.
			var text = tail[tokens[at].At..];
			var read = TransactSql.TransactSqlParser.Located.TryParseSql(text);

			if (!read.IsSuccess || read.Value is not { Length: > 0 } elements)
				throw BridgeGap.Open(old, "inventory Statement.SchemaDefinition (elements that a script does not read)");

			return made with { Elements = Inside(text, () => elements.Select(StatementOf).ToSqlList()) };
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

		// ── Roles, schemas, logins and users ───────────────────────────────────────

		/// <summary>
		/// A text cut into tokens, each with the position it starts at: comments and blanks skipped as the tokens
		/// skip them.
		/// </summary>
		static List<(SqlTokens.Token Token, int At)> Located(string text)
		{
			var located = new List<(SqlTokens.Token, int)>();
			var at      = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				at = SkipBlanks(text, at);
				located.Add((token, at));
				at += token.Text.Length;
			}

			return located;
		}

		static int SkipBlanks(string text, int at)
		{
			while (at < text.Length)
			{
				if (char.IsWhiteSpace(text[at]))
					at++;
				else if (text[at] == '-' && at + 1 < text.Length && text[at + 1] == '-')
					at = text.IndexOf('\n', at) is var end && end >= 0 ? end + 1 : text.Length;
				else if (text[at] == '/' && at + 1 < text.Length && text[at + 1] == '*')
				{
					var depth = 0;

					while (at < text.Length)
					{
						if (text[at] == '/' && at + 1 < text.Length && text[at + 1] == '*')
						{
							depth++;
							at += 2;
						}
						else if (text[at] == '*' && at + 1 < text.Length && text[at + 1] == '/')
						{
							at += 2;

							if (--depth == 0)
								break;
						}
						else
							at++;
					}
				}
				else
					break;
			}

			return at;
		}

		static bool Is(List<SqlTokens.Token> tokens, int at, string word)
		{
			return at < tokens.Count && tokens[at].Kind == SqlTokens.Kind.Word && tokens[at].Text.Equals(word, StringComparison.OrdinalIgnoreCase);
		}

		static bool IsName(List<SqlTokens.Token> tokens, int at)
		{
			return at < tokens.Count && tokens[at].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited;
		}

		static bool IsPunctuation(List<SqlTokens.Token> tokens, int at, string text)
		{
			return at < tokens.Count && tokens[at] is { Kind: SqlTokens.Kind.Punctuation } token && token.Text == text;
		}

		static BridgeGap Unread(object old, string? tail)
		{
			return BridgeGap.Open(old, $"inventory {NameOf(old)} (`{tail}` after the name)");
		}

		/// <summary>
		/// <c>AUTHORIZATION owner</c> and nothing else after a name, or null where nothing is written.
		/// </summary>
		static Ast.Identifier? Owner(object old, string? tail)
		{
			if (tail is null)
				return null;

			var tokens = SqlTokens.Of(tail);

			return Is(tokens, 0, "AUTHORIZATION") && IsName(tokens, 1) && tokens.Count == 2 ? Values.Part(old, tokens[1].Text) : throw Unread(old, tail);
		}

		static Ast.RoleAction RoleAction(object old, string? tail)
		{
			var tokens = SqlTokens.Of(tail ?? "");

			if (Is(tokens, 0, "ADD") && Is(tokens, 1, "MEMBER") && IsName(tokens, 2) && tokens.Count == 3)
				return new Ast.RoleAction.AddMember(Values.Part(old, tokens[2].Text));

			if (Is(tokens, 0, "DROP") && Is(tokens, 1, "MEMBER") && IsName(tokens, 2) && tokens.Count == 3)
				return new Ast.RoleAction.DropMember(Values.Part(old, tokens[2].Text));

			if (Is(tokens, 0, "WITH") && Is(tokens, 1, "NAME") && IsPunctuation(tokens, 2, "=") && IsName(tokens, 3) && tokens.Count == 4)
				return new Ast.RoleAction.WithName(Values.Part(old, tokens[3].Text));

			throw Unread(old, tail);
		}

		/// <summary>
		/// A class and <c>::</c> before a name, as <c>SecurableClass</c> reads them, and the name's tokens after: the class,
		/// or null where none is written.
		/// </summary>
		static (Ast.SecurableClass? Class, List<SqlTokens.Token> Name) Classed(object old, List<SqlTokens.Token> tokens, string tail)
		{
			var colons = tokens.FindIndex(static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: ":" });

			if (colons < 0)
				return (null, tokens);

			var written = string.Join(' ', tokens[..colons].Select(static one => one.Text.ToUpperInvariant()));

			if (!IsPunctuation(tokens, colons + 1, ":") || !ClassWords.Value.TryGetValue(written, out var kind))
				throw Unread(old, tail);

			return (kind, tokens[(colons + 2)..]);
		}

		static Ast.Statement AlterSchema(Statement.AlterSchema old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);

			if (!Is(tokens, 0, "TRANSFER"))
				throw Unread(old, tail);

			var (kind, rest) = Classed(old, tokens[1..], tail);
			var name         = Dotted(rest) ?? throw Unread(old, tail);

			return new Ast.Statement.AlterSchema(Values.Part(old, old.Name), Values.Name(old, name)) { Class = kind };
		}

		static Ast.Statement AlterAuthorization(Statement.AlterAuthorization old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);
			var to     = tokens.FindLastIndex(static one => one.Kind == SqlTokens.Kind.Word && one.Text.Equals("TO", StringComparison.OrdinalIgnoreCase));

			if (!Is(tokens, 0, "ON") || to < 2)
				throw Unread(old, tail);

			var (kind, rest) = Classed(old, tokens[1..to], tail);
			var entity       = Dotted(rest) ?? throw Unread(old, tail);
			var target       = tokens[(to + 1)..];

			Ast.AuthorizationTo owner = target switch
			{
				[{ Kind: SqlTokens.Kind.Word } schema, { Kind: SqlTokens.Kind.Word } word] when schema.Text.Equals("SCHEMA", StringComparison.OrdinalIgnoreCase) && word.Text.Equals("OWNER", StringComparison.OrdinalIgnoreCase)
					=> new Ast.AuthorizationTo.SchemaOwner(),
				[{ Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited } principal]
					=> new Ast.AuthorizationTo.Principal(Values.Part(old, principal.Text)),
				_ => throw Unread(old, tail),
			};

			return new Ast.Statement.AlterAuthorization(Values.Name(old, entity), owner) { Class = kind };
		}

		/// <summary>
		/// A value of a setting, one token: a string, a number or a hash in hex, NULL, or a name.
		/// </summary>
		static Ast.Expression SettingValue(object old, SqlTokens.Token token)
		{
			switch (token)
			{
				case { Kind: SqlTokens.Kind.String }:
					return Values.String(token.Text);

				case { Kind: SqlTokens.Kind.Number } when token.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase):
					return Values.Of(new Expression.Literal(SqlLiteralKind.Hex, token.Text));

				case { Kind: SqlTokens.Kind.Number }:
					return Values.Of(new Expression.Literal(SqlLiteralKind.Number, token.Text));

				case { Kind: SqlTokens.Kind.Word } when token.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase):
					return Values.Of(new Expression.Literal(SqlLiteralKind.Null, token.Text));

				case { Kind: SqlTokens.Kind.Word or SqlTokens.Kind.Delimited }:
					return new Ast.Expression.Reference(new Ast.QualifiedName([Values.Part(old, token.Text)]));

				default:
					throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the value `{token.Text}`)");
			}
		}

		/// <summary>
		/// The tokens cut at the commas outside brackets, from a position on.
		/// </summary>
		static List<List<SqlTokens.Token>> Pieces(List<SqlTokens.Token> tokens, int from)
		{
			var pieces = new List<List<SqlTokens.Token>> { new() };
			var depth  = 0;

			for (var at = from; at < tokens.Count; at++)
			{
				if (tokens[at] is { Kind: SqlTokens.Kind.Punctuation } punctuation)
				{
					if (punctuation.Text == "(")
						depth++;
					else if (punctuation.Text == ")")
						depth--;
					else if (punctuation.Text == "," && depth == 0)
					{
						pieces.Add([]);

						continue;
					}
				}

				pieces[^1].Add(tokens[at]);
			}

			return pieces;
		}

		/// <summary>
		/// The settings of a list, <c>a = v, b = w</c>, from a position on: each a name of one word or more and, after <c>=</c>,
		/// one value. A <c>PASSWORD</c> of ALTER LOGIN and ALTER USER keeps the words after its value as its options.
		/// </summary>
		static SqlList<Ast.Option> Settings(object old, List<SqlTokens.Token> tokens, int from, string tail, bool password = false)
		{
			var made = new List<Ast.Option>();

			foreach (var piece in Pieces(tokens, from))
				made.Add(Setting(old, piece, tail, password));

			return made.ToSqlList();
		}

		static Ast.Option Setting(object old, List<SqlTokens.Token> piece, string tail, bool password)
		{
			var equals = piece.FindIndex(static one => one is { Kind: SqlTokens.Kind.Punctuation, Text: "=" });
			var name   = equals < 0 ? piece : piece[..equals];

			if (name.Count == 0 || name.Any(static one => one.Kind != SqlTokens.Kind.Word))
				throw Unread(old, tail);

			Ast.Expression? value = null;
			var options = new List<Ast.Option>();

			if (equals >= 0)
			{
				if (equals + 1 >= piece.Count)
					throw Unread(old, tail);

				value = SettingValue(old, piece[equals + 1]);

				var rest = piece[(equals + 2)..];

				if (rest.Count > 0)
				{
					if (!password || !name[0].Text.Equals("PASSWORD", StringComparison.OrdinalIgnoreCase) || name.Count != 1)
						throw Unread(old, tail);

					if (Is(rest, 0, "OLD_PASSWORD"))
					{
						if (rest.Count != 3 || !IsPunctuation(rest, 1, "=") || rest[2].Kind != SqlTokens.Kind.String)
							throw Unread(old, tail);

						options.Add(new Ast.Option(new Ast.Identifier(rest[0].Text), SettingValue(old, rest[2]), []));
					}
					else
					{
						foreach (var word in rest)
						{
							if (word.Kind != SqlTokens.Kind.Word)
								throw Unread(old, tail);

							options.Add(new Ast.Option(new Ast.Identifier(word.Text), null, []));
						}
					}
				}
			}

			return new Ast.Option(new Ast.Identifier(name[0].Text), value, options.ToSqlList()) { Words = name.Skip(1).Select(static one => new Ast.Identifier(one.Text)).ToSqlList() };
		}

		static Ast.Statement ApplicationRole(object old, string? tail, bool create)
		{
			var tokens = SqlTokens.Of(tail ?? "");

			if (!Is(tokens, 0, "WITH"))
				throw Unread(old, tail);

			var settings = Settings(old, tokens, 1, tail!);
			var name     = Values.Part(old, ((Statement.Definition)old).Name);

			return create ? new Ast.Statement.CreateApplicationRole(name, settings) : new Ast.Statement.AlterApplicationRole(name, settings);
		}

		static Ast.Statement CreateLogin(Statement.CreateLogin old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);
			var name   = Values.Part(old, old.Name);

			// WITH PASSWORD = value [HASHED] [MUST_CHANGE], then `, setting`.
			if (Is(tokens, 0, "WITH") && Is(tokens, 1, "PASSWORD") && IsPunctuation(tokens, 2, "=") && tokens.Count > 3)
			{
				var pieces = Pieces(tokens, 3);
				var first  = pieces[0];

				if (first.Count == 0 || first.Skip(1).Any(static one => one.Kind != SqlTokens.Kind.Word))
					throw Unread(old, tail);

				var words  = first.Skip(1).Select(static one => one.Text.ToUpperInvariant()).ToList();
				var source = words switch
				{
					[] => new Ast.LoginSource.Password(SettingValue(old, first[0])),
					["HASHED"] => new Ast.LoginSource.Password(SettingValue(old, first[0])) { Hashed = true },
					["MUST_CHANGE"] => new Ast.LoginSource.Password(SettingValue(old, first[0])) { MustChange = true },
					["HASHED", "MUST_CHANGE"] => new Ast.LoginSource.Password(SettingValue(old, first[0])) { Hashed = true, MustChange = true },
					["MUST_CHANGE", "HASHED"] => new Ast.LoginSource.Password(SettingValue(old, first[0])) { Hashed = true, MustChange = true, MustChangeFirst = true },
					_ => throw Unread(old, tail),
				};

				var options = pieces.Skip(1).Select(piece => Setting(old, piece, tail, password: false)).ToSqlList();

				return new Ast.Statement.CreateLogin(name, source) { Options = options };
			}

			if (!Is(tokens, 0, "FROM"))
				throw Unread(old, tail);

			if (Is(tokens, 1, "WINDOWS"))
				return new Ast.Statement.CreateLogin(name, new Ast.LoginSource.Windows()) { Options = WithSettings(old, tokens, 2, tail) };

			if (Is(tokens, 1, "EXTERNAL") && Is(tokens, 2, "PROVIDER"))
				return new Ast.Statement.CreateLogin(name, new Ast.LoginSource.ExternalProvider()) { Options = WithSettings(old, tokens, 3, tail) };

			var key = Is(tokens, 1, "CERTIFICATE") ? 2 : Is(tokens, 1, "ASYMMETRIC") && Is(tokens, 2, "KEY") ? 3 : -1;

			if (key < 0 || !IsName(tokens, key))
				throw Unread(old, tail);

			var credential = Credential(old, tokens, key + 1, tail);
			var signer     = Values.Part(old, tokens[key].Text);

			return new Ast.Statement.CreateLogin(name, key == 2
				? new Ast.LoginSource.Certificate(signer, credential)
				: new Ast.LoginSource.AsymmetricKey(signer, credential));
		}

		/// <summary>
		/// <c>WITH CREDENTIAL = c</c> at a position, or null where nothing stands there.
		/// </summary>
		static Ast.Identifier? Credential(object old, List<SqlTokens.Token> tokens, int at, string tail)
		{
			if (at == tokens.Count)
				return null;

			return Is(tokens, at, "WITH") && Is(tokens, at + 1, "CREDENTIAL") && IsPunctuation(tokens, at + 2, "=") && IsName(tokens, at + 3) && tokens.Count == at + 4
				? Values.Part(old, tokens[at + 3].Text)
				: throw Unread(old, tail);
		}

		/// <summary>
		/// <c>WITH setting, …</c> at a position, or none where nothing stands there.
		/// </summary>
		static SqlList<Ast.Option> WithSettings(object old, List<SqlTokens.Token> tokens, int at, string tail)
		{
			if (at == tokens.Count)
				return [];

			return Is(tokens, at, "WITH") ? Settings(old, tokens, at + 1, tail) : throw Unread(old, tail);
		}

		static Ast.Statement AlterLogin(Statement.AlterLogin old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);
			var name   = Values.Part(old, old.Name);

			Ast.AlterLoginAction action = tokens switch
			{
				_ when Is(tokens, 0, "ENABLE") && tokens.Count == 1   => new Ast.AlterLoginAction.Enable(),
				_ when Is(tokens, 0, "DISABLE") && tokens.Count == 1  => new Ast.AlterLoginAction.Disable(),
				_ when Is(tokens, 0, "WITH")                          => new Ast.AlterLoginAction.With(Settings(old, tokens, 1, tail, password: true)),
				_ when Is(tokens, 0, "ADD") && Is(tokens, 1, "CREDENTIAL") && IsName(tokens, 2) && tokens.Count == 3
					=> new Ast.AlterLoginAction.AddCredential(Values.Part(old, tokens[2].Text)),
				_ when Is(tokens, 0, "DROP") && Is(tokens, 1, "CREDENTIAL") && IsName(tokens, 2) && tokens.Count == 3
					=> new Ast.AlterLoginAction.DropCredential(Values.Part(old, tokens[2].Text)),
				_ => throw Unread(old, tail),
			};

			return new Ast.Statement.AlterLogin(name, action);
		}

		static Ast.Statement CreateUser(Statement.CreateUser old)
		{
			var tail   = old.Tail;
			var tokens = SqlTokens.Of(tail ?? "");
			var name   = Values.Part(old, old.Name);
			var at     = 0;
			Ast.UserSource? source = null;

			if (Is(tokens, 0, "WITHOUT") && Is(tokens, 1, "LOGIN"))
			{
				source = new Ast.UserSource.WithoutLogin();
				at     = 2;
			}
			else if (Is(tokens, 0, "FOR") || Is(tokens, 0, "FROM"))
			{
				var from = Is(tokens, 0, "FROM");

				if (Is(tokens, 1, "LOGIN") && IsName(tokens, 2))
				{
					source = new Ast.UserSource.Login(Values.Part(old, tokens[2].Text)) { From = from };
					at     = 3;
				}
				else if (Is(tokens, 1, "CERTIFICATE") && IsName(tokens, 2))
				{
					source = new Ast.UserSource.Certificate(Values.Part(old, tokens[2].Text)) { From = from };
					at     = 3;
				}
				else if (Is(tokens, 1, "ASYMMETRIC") && Is(tokens, 2, "KEY") && IsName(tokens, 3))
				{
					source = new Ast.UserSource.AsymmetricKey(Values.Part(old, tokens[3].Text)) { From = from };
					at     = 4;
				}
				else if (Is(tokens, 1, "EXTERNAL") && Is(tokens, 2, "PROVIDER"))
				{
					source = new Ast.UserSource.ExternalProvider { From = from };
					at     = 3;
				}
				else
					throw Unread(old, tail);
			}

			return new Ast.Statement.CreateUser(name) { Source = source, Options = WithSettings(old, tokens, at, tail ?? "") };
		}

		static Ast.Statement AlterUser(Statement.AlterUser old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);

			return Is(tokens, 0, "WITH")
				? new Ast.Statement.AlterUser(Values.Part(old, old.Name), Settings(old, tokens, 1, tail, password: true))
				: throw Unread(old, tail);
		}

		static Ast.Statement Role(Statement.CreateRole old)
		{
			return new Ast.Statement.CreateRole { Name = Values.Part(old, old.Name), Authorization = Owner(old, old.Tail) };
		}
	}
}
