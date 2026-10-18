using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Bridge
{
	/// <summary>
	/// Permissions, principals, schemas, keys, certificates, credentials and the classifications and signatures on objects.
	/// </summary>
	/// <remarks>
	/// Every record has an arm. The old tree keeps what follows the name of a definition as text (<c>Tail</c>), which the arms read
	/// token by token as the grammar's rules read it; a text the arm does not read to its end is a gap. A text kept as written
	/// that the writer would print otherwise is a gap too (<see cref="Written"/>), never a difference.
	/// <para>
	/// Their rules build the common tree, so the family carries nothing: it owns the records those rules built before,
	/// under which their statements are counted.
	/// </para>
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

		/// <summary>
		/// <c>CREATE SCHEMA [s] [AUTHORIZATION owner] [element …]</c>. The old tree keeps what follows the name as text:
		/// the owner is read from its tokens, and the elements, which are whole statements, are read again as a script is
		/// (docs/design/sql-tsql-bridge.md, "Definitions kept as text").
		/// </summary>
		internal static Ast.Statement Schema(Defining old)
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
			// Read with that text as the one its rules carry their parts with, as the elements are carried below.
			var read = Inside(text, () => TransactSql.TransactSqlParser.Located.TryParseSyntaxSql(text));

			if (!read.IsSuccess || read.Value is not { Length: > 0 } elements)
				throw BridgeGap.Open(old, "inventory Statement.SchemaDefinition (elements that a script does not read)");

			return made with { Elements = Inside(text, () => elements.Select(StatementOf).ToSqlList()) };
		}

		// ── GRANT, DENY and REVOKE ─────────────────────────────────────────────────

		/// <summary>GRANT, on the common tree.</summary>
		internal static Statement Grant(SqlList<string> privileges, SqlList<string> principals, string? on, bool grantOption, string? @as)
		{
			return Carry(new Statement.Grant(privileges, principals, on, grantOption, @as), Grant);
		}

		/// <summary>DENY, on the common tree.</summary>
		internal static Statement Deny(SqlList<string> privileges, SqlList<string> principals, string? on, bool cascade, string? @as)
		{
			return Carry(new Statement.Deny(privileges, principals, on, cascade, @as), Deny);
		}

		/// <summary>REVOKE, on the common tree.</summary>
		internal static Statement Revoke(
			SqlList<string> privileges, SqlList<string> principals, string? on, bool grantOptionFor, bool from, bool cascade, string? @as)
		{
			return Carry(new Statement.Revoke(privileges, principals, on, grantOptionFor, from, cascade, @as), Revoke);
		}

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
		internal static Ast.Identifier? Owner(object old, string? tail)
		{
			if (tail is null)
				return null;

			var tokens = SqlTokens.Of(tail);

			return Is(tokens, 0, "AUTHORIZATION") && IsName(tokens, 1) && tokens.Count == 2 ? Values.Part(old, tokens[1].Text) : throw Unread(old, tail);
		}

		internal static Ast.RoleAction RoleAction(object old, string? tail)
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

		internal static Ast.Statement AlterSchema(Defining old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);

			if (!Is(tokens, 0, "TRANSFER"))
				throw Unread(old, tail);

			var (kind, rest) = Classed(old, tokens[1..], tail);
			var name         = Dotted(rest) ?? throw Unread(old, tail);

			return new Ast.Statement.AlterSchema(Values.Part(old, old.Name), Values.Name(old, name)) { Class = kind };
		}

		internal static Ast.Statement AlterAuthorization(Defining old)
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

		internal static Ast.Statement ApplicationRole(Defining old, string? tail, bool create)
		{
			var tokens = SqlTokens.Of(tail ?? "");

			if (!Is(tokens, 0, "WITH"))
				throw Unread(old, tail);

			var settings = Settings(old, tokens, 1, tail!);
			var name     = Values.Part(old, old.Name);

			return create ? new Ast.Statement.CreateApplicationRole(name, settings) : new Ast.Statement.AlterApplicationRole(name, settings);
		}

		internal static Ast.Statement CreateLogin(Defining old)
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

		internal static Ast.Statement AlterLogin(Defining old)
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

		internal static Ast.Statement CreateUser(Defining old)
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

		internal static Ast.Statement AlterUser(Defining old)
		{
			var tail   = old.Tail ?? "";
			var tokens = SqlTokens.Of(tail);

			return Is(tokens, 0, "WITH")
				? new Ast.Statement.AlterUser(Values.Part(old, old.Name), Settings(old, tokens, 1, tail, password: true))
				: throw Unread(old, tail);
		}

		// ── Keys, certificates, credentials ────────────────────────────────────────

		/// <summary>
		/// What follows a definition's name, read from left to right: each step is checked, and a text that is not read to its
		/// end, or not as the grammar reads it, is a gap.
		/// </summary>
		sealed class Reader(object old, string? tail)
		{
			readonly List<SqlTokens.Token> _tokens = SqlTokens.Of(tail ?? "");
			int _at;

			public bool End
			{
				get { return _at >= _tokens.Count; }
			}

			public SqlTokens.Token NextToken()
			{
				return _tokens[_at++];
			}

			public BridgeGap Unread()
			{
				return BridgeGap.Open(old, $"inventory {NameOf(old)} (`{tail}` after the name)");
			}

			public void Finish()
			{
				if (!End)
					throw Unread();
			}

			bool WordAt(int at, string word)
			{
				return at < _tokens.Count && _tokens[at].Kind == SqlTokens.Kind.Word && _tokens[at].Text.Equals(word, StringComparison.OrdinalIgnoreCase);
			}

			public bool Is(params string[] words)
			{
				for (var at = 0; at < words.Length; at++)
					if (!WordAt(_at + at, words[at]))
						return false;

				return true;
			}

			public bool Take(params string[] words)
			{
				if (!Is(words))
					return false;

				_at += words.Length;

				return true;
			}

			public void Expect(params string[] words)
			{
				if (!Take(words))
					throw Unread();
			}

			public bool IsPunctuation(string text)
			{
				return _at < _tokens.Count && _tokens[_at] is { Kind: SqlTokens.Kind.Punctuation } token && token.Text == text;
			}

			public bool TakePunctuation(string text)
			{
				if (!IsPunctuation(text))
					return false;

				_at++;

				return true;
			}

			public void ExpectPunctuation(string text)
			{
				if (!TakePunctuation(text))
					throw Unread();
			}

			public Ast.Identifier Name()
			{
				if (_at >= _tokens.Count || _tokens[_at].Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
					throw Unread();

				return Values.Part(old, _tokens[_at++].Text);
			}

			public Ast.QualifiedName Qualified()
			{
				var start = _at;
				var dot   = true;

				while (_at < _tokens.Count)
				{
					if (_tokens[_at] is { Kind: SqlTokens.Kind.Punctuation, Text: "." })
						dot = true;
					else if (dot && _tokens[_at].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
						dot = false;
					else
						break;

					_at++;
				}

				return Dotted(_tokens.GetRange(start, _at - start)) is { } text ? Values.Name(old, text) : throw Unread();
			}

			public Ast.LiteralValue.String String()
			{
				if (_at >= _tokens.Count || _tokens[_at].Kind != SqlTokens.Kind.String)
					throw Unread();

				return (Ast.LiteralValue.String)Values.String(_tokens[_at++].Text);
			}

			public Ast.LiteralValue.Binary Hex()
			{
				if (_at >= _tokens.Count || _tokens[_at].Kind != SqlTokens.Kind.Number || !_tokens[_at].Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					throw Unread();

				return (Ast.LiteralValue.Binary)Values.Of(new Expression.Literal(SqlLiteralKind.Hex, _tokens[_at++].Text));
			}

			public Ast.Identifier? Authorization()
			{
				return Take("AUTHORIZATION") ? Name() : null;
			}

			/// <summary>One setting: its words, and after <c>=</c> a value of one token, or after nothing a bracketed list of its own.</summary>
			public Ast.Option Setting()
			{
				var words = new List<SqlTokens.Token>();

				while (_at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Word)
					words.Add(_tokens[_at++]);

				if (words.Count == 0)
					throw Unread();

				var name  = new Ast.Identifier(words[0].Text);
				var extra = words.Skip(1).Select(static one => new Ast.Identifier(one.Text)).ToSqlList();

				if (IsPunctuation("("))
					return new Ast.Option(name, null, Bracketed()) { Words = extra };

				ExpectPunctuation("=");

				if (_at >= _tokens.Count)
					throw Unread();

				return new Ast.Option(name, SettingValue(old, _tokens[_at++]), []) { Words = extra };
			}

			/// <summary>Settings joined by commas.</summary>
			public SqlList<Ast.Option> Settings()
			{
				var made = new List<Ast.Option> { Setting() };

				while (TakePunctuation(","))
					made.Add(Setting());

				return made.ToSqlList();
			}

			/// <summary>Settings in brackets.</summary>
			public SqlList<Ast.Option> Bracketed()
			{
				ExpectPunctuation("(");

				var made = Settings();

				ExpectPunctuation(")");

				return made;
			}

			/// <summary><c>PASSWORD = 'p'</c>.</summary>
			public Ast.KeyEncryptor.Password PasswordEncryptor()
			{
				Expect("PASSWORD");
				ExpectPunctuation("=");

				return new Ast.KeyEncryptor.Password(String());
			}

			/// <summary>
			/// One of the family: a password, a certificate or an asymmetric key (each perhaps with a password), a symmetric key, or the
			/// service master key. The server's certificate or asymmetric key is the database encryption key's, read by
			/// <c>ServerEncryption</c>.
			/// </summary>
			public Ast.KeyEncryptor Encryptor()
			{
				if (Is("PASSWORD"))
					return PasswordEncryptor();

				if (Take("CERTIFICATE"))
				{
					var name = Qualified();

					return new Ast.KeyEncryptor.Certificate(name) { WithPassword = WithPassword() };
				}

				if (Take("ASYMMETRIC", "KEY"))
				{
					var name = Qualified();

					return new Ast.KeyEncryptor.AsymmetricKey(name) { WithPassword = WithPassword() };
				}

				if (Take("SYMMETRIC", "KEY"))
					return new Ast.KeyEncryptor.SymmetricKey(Qualified());

				if (Take("SERVICE", "MASTER", "KEY"))
					return new Ast.KeyEncryptor.ServiceMasterKey();

				throw Unread();
			}

			Ast.LiteralValue.String? WithPassword()
			{
				if (!Take("WITH", "PASSWORD"))
					return null;

				ExpectPunctuation("=");

				return String();
			}

			public SqlList<Ast.KeyEncryptor> Encryptors()
			{
				var made = new List<Ast.KeyEncryptor> { Encryptor() };

				while (TakePunctuation(","))
					made.Add(Encryptor());

				return made.ToSqlList();
			}

			public bool? OnOff()
			{
				if (Take("ON"))
					return true;

				if (Take("OFF"))
					return false;

				throw Unread();
			}
		}

		internal static Ast.Statement CreateAsymmetricKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var made   = new Ast.Statement.CreateAsymmetricKey(Values.Part(old, old.Name)) { Authorization = reader.Authorization() };

			if (reader.Take("FROM", "PROVIDER"))
				made = made with { Source = new Ast.AsymmetricKeySource.Provider(reader.Name()), Options = reader.Take("WITH") ? reader.Settings() : [] };
			else if (reader.Take("FROM", "EXECUTABLE", "FILE"))
			{
				reader.ExpectPunctuation("=");
				made = made with { Source = new Ast.AsymmetricKeySource.File(reader.String()) { Executable = true } };
			}
			else if (reader.Take("FROM", "FILE"))
			{
				reader.ExpectPunctuation("=");
				made = made with { Source = new Ast.AsymmetricKeySource.File(reader.String()) };
			}
			else if (reader.Take("FROM", "ASSEMBLY"))
				made = made with { Source = new Ast.AsymmetricKeySource.Assembly(reader.Qualified()) };
			else if (reader.Take("WITH"))
				made = made with { Options = reader.Settings() };

			if (reader.Take("ENCRYPTION", "BY"))
				made = made with { EncryptedBy = reader.PasswordEncryptor() };

			reader.Finish();

			return made;
		}

		internal static Ast.Statement AlterAsymmetricKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.AsymmetricKeyChange change;

			if (reader.Take("REMOVE", "PRIVATE", "KEY"))
				change = new Ast.AsymmetricKeyChange.RemovePrivateKey();
			else if (reader.Take("WITH", "PRIVATE", "KEY"))
				change = new Ast.AsymmetricKeyChange.WithPrivateKey(reader.Bracketed());
			else if (reader.Take("ATTESTED", "BY"))
				change = new Ast.AsymmetricKeyChange.AttestedBy(reader.String());
			else if (reader.Take("REMOVE", "ATTESTED", "OPTION"))
				change = new Ast.AsymmetricKeyChange.RemoveAttestedOption();
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterAsymmetricKey(Values.Part(old, old.Name), change);
		}

		internal static Ast.Statement CreateSymmetricKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var made   = new Ast.Statement.CreateSymmetricKey(Values.Part(old, old.Name)) { Authorization = reader.Authorization() };

			if (reader.Take("FROM", "PROVIDER"))
				made = made with { Provider = reader.Name(), Options = reader.Take("WITH") ? reader.Settings() : [] };
			else if (reader.Take("WITH"))
				made = made with { Options = reader.Settings() };

			if (reader.Take("ENCRYPTION", "BY"))
				made = made with { EncryptedBy = reader.Encryptors() };

			reader.Finish();

			return made;
		}

		internal static Ast.Statement AlterSymmetricKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var add    = reader.Take("ADD");

			if (!add)
				reader.Expect("DROP");

			reader.Expect("ENCRYPTION", "BY");

			var by = reader.Encryptors();

			reader.Finish();

			return new Ast.Statement.AlterSymmetricKey(Values.Part(old, old.Name), add ? new Ast.SymmetricKeyChange.AddEncryption(by) : new Ast.SymmetricKeyChange.DropEncryption(by));
		}

		internal static Ast.Statement CreateCertificate(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var made   = new Ast.Statement.CreateCertificate(Values.Part(old, old.Name)) { Authorization = reader.Authorization() };

			if (reader.Take("FROM"))
				made = made with { Source = CertificateSource(reader) };
			else
			{
				if (reader.Take("ENCRYPTION", "BY"))
					made = made with { EncryptedBy = reader.PasswordEncryptor() };

				reader.Expect("WITH");
				made = made with { Options = reader.Settings() };
			}

			if (reader.Take("ACTIVE", "FOR", "BEGIN_DIALOG"))
			{
				reader.ExpectPunctuation("=");
				made = made with { ActiveForBeginDialog = reader.OnOff() };
			}

			reader.Finish();

			return made;
		}

		static Ast.CertificateSource CertificateSource(Reader reader)
		{
			if (reader.Take("ASSEMBLY"))
				return new Ast.CertificateSource.Assembly(reader.Qualified());

			if (reader.Take("BINARY"))
			{
				reader.ExpectPunctuation("=");

				var value = reader.Hex();

				return new Ast.CertificateSource.Binary(value) { PrivateKey = PrivateKey(reader, out _) };
			}

			var executable = reader.Take("EXECUTABLE");

			reader.Expect("FILE");
			reader.ExpectPunctuation("=");

			var path        = reader.String();
			var privateKey  = PrivateKey(reader, out var format);

			return new Ast.CertificateSource.File(path) { Executable = executable, Format = format, PrivateKey = privateKey };
		}

		/// <summary><c>WITH [FORMAT = 'f',] PRIVATE KEY (…)</c>, or nothing.</summary>
		static SqlList<Ast.Option> PrivateKey(Reader reader, out Ast.LiteralValue.String? format)
		{
			format = null;

			if (!reader.Take("WITH"))
				return [];

			if (reader.Take("FORMAT"))
			{
				reader.ExpectPunctuation("=");
				format = reader.String();
				reader.ExpectPunctuation(",");
			}

			reader.Expect("PRIVATE", "KEY");

			return reader.Bracketed();
		}

		internal static Ast.Statement AlterCertificate(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.CertificateChange change;

			if (reader.Take("REMOVE", "PRIVATE", "KEY"))
				change = new Ast.CertificateChange.RemovePrivateKey();
			else if (reader.Take("WITH", "PRIVATE", "KEY"))
				change = new Ast.CertificateChange.WithPrivateKey(reader.Bracketed());
			else if (reader.Take("WITH", "ACTIVE", "FOR", "BEGIN_DIALOG"))
			{
				reader.ExpectPunctuation("=");
				change = new Ast.CertificateChange.ActiveForBeginDialog(reader.OnOff() == true);
			}
			else if (reader.Take("ATTESTED", "BY"))
				change = new Ast.CertificateChange.AttestedBy(reader.String());
			else if (reader.Take("REMOVE", "ATTESTED", "OPTION"))
				change = new Ast.CertificateChange.RemoveAttestedOption();
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterCertificate(Values.Part(old, old.Name), change);
		}

		internal static Ast.Statement CreateMasterKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var made   = new Ast.Statement.CreateMasterKey();

			if (reader.Take("ENCRYPTION", "BY"))
				made = made with { EncryptedBy = reader.PasswordEncryptor() };

			reader.Finish();

			return made;
		}

		internal static Ast.Statement AlterMasterKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.MasterKeyChange change;

			var force = reader.Take("FORCE");

			if (reader.Take("REGENERATE"))
			{
				reader.Expect("WITH", "ENCRYPTION", "BY");
				change = new Ast.MasterKeyChange.Regenerate(reader.PasswordEncryptor()) { Force = force };
			}
			else if (!force && reader.Take("ADD", "ENCRYPTION", "BY"))
				change = new Ast.MasterKeyChange.AddEncryption(reader.Encryptor());
			else if (!force && reader.Take("DROP", "ENCRYPTION", "BY"))
				change = new Ast.MasterKeyChange.DropEncryption(reader.Encryptor());
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterMasterKey(change);
		}

		internal static Ast.Statement AlterServiceMasterKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.ServiceMasterKeyChange change;

			var force = reader.Take("FORCE");

			if (reader.Take("REGENERATE"))
				change = new Ast.ServiceMasterKeyChange.Regenerate { Force = force };
			else if (!force && (reader.Is("WITH", "OLD_ACCOUNT") || reader.Is("WITH", "NEW_ACCOUNT")))
			{
				reader.Expect("WITH");

				var old_ = reader.Take("OLD_ACCOUNT");

				if (!old_)
					reader.Expect("NEW_ACCOUNT");

				reader.ExpectPunctuation("=");

				var account = reader.String();

				reader.ExpectPunctuation(",");
				reader.Expect(old_ ? "OLD_PASSWORD" : "NEW_PASSWORD");
				reader.ExpectPunctuation("=");

				var password = reader.String();

				change = old_ ? new Ast.ServiceMasterKeyChange.OldAccount(account, password) : new Ast.ServiceMasterKeyChange.NewAccount(account, password);
			}
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterServiceMasterKey(change);
		}

		static Ast.DatabaseKeyAlgorithm DatabaseKeyAlgorithm(Reader reader)
		{
			reader.Expect("ALGORITHM");
			reader.ExpectPunctuation("=");

			if (reader.Take("AES_128"))
				return Ast.DatabaseKeyAlgorithm.Aes128;

			if (reader.Take("AES_192"))
				return Ast.DatabaseKeyAlgorithm.Aes192;

			if (reader.Take("AES_256"))
				return Ast.DatabaseKeyAlgorithm.Aes256;

			if (reader.Take("TRIPLE_DES_3KEY"))
				return Ast.DatabaseKeyAlgorithm.TripleDes3Key;

			throw reader.Unread();
		}

		static Ast.KeyEncryptor ServerEncryption(Reader reader)
		{
			reader.Expect("ENCRYPTION", "BY", "SERVER");

			if (reader.Take("CERTIFICATE"))
				return new Ast.KeyEncryptor.Certificate(reader.Qualified());

			reader.Expect("ASYMMETRIC", "KEY");

			return new Ast.KeyEncryptor.AsymmetricKey(reader.Qualified());
		}

		internal static Ast.Statement CreateDatabaseEncryptionKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);

			reader.Expect("WITH");

			var algorithm = DatabaseKeyAlgorithm(reader);
			var by        = ServerEncryption(reader);

			reader.Finish();

			return new Ast.Statement.CreateDatabaseEncryptionKey(algorithm, by);
		}

		internal static Ast.Statement AlterDatabaseEncryptionKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.DatabaseEncryptionKeyChange change;

			if (reader.Take("REGENERATE", "WITH"))
				change = new Ast.DatabaseEncryptionKeyChange.Regenerate(DatabaseKeyAlgorithm(reader));
			else
				change = new Ast.DatabaseEncryptionKeyChange.EncryptedBy(ServerEncryption(reader));

			reader.Finish();

			return new Ast.Statement.AlterDatabaseEncryptionKey(change);
		}

		static Ast.ColumnKeyValue ColumnKeyValue(Reader reader)
		{
			return new Ast.ColumnKeyValue(reader.Bracketed());
		}

		internal static Ast.Statement CreateColumnEncryptionKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);

			reader.Expect("WITH", "VALUES");

			var values = new List<Ast.ColumnKeyValue> { ColumnKeyValue(reader) };

			while (reader.TakePunctuation(","))
				values.Add(ColumnKeyValue(reader));

			reader.Finish();

			return new Ast.Statement.CreateColumnEncryptionKey(Values.Part(old, old.Name), values.ToSqlList());
		}

		internal static Ast.Statement AlterColumnEncryptionKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			Ast.ColumnKeyChange change;

			if (reader.Take("ADD", "VALUE"))
				change = new Ast.ColumnKeyChange.AddValue(ColumnKeyValue(reader));
			else if (reader.Take("DROP", "VALUE"))
				change = new Ast.ColumnKeyChange.DropValue(ColumnKeyValue(reader));
			else
				throw reader.Unread();

			reader.Finish();

			return new Ast.Statement.AlterColumnEncryptionKey(Values.Part(old, old.Name), change);
		}

		internal static Ast.Statement CreateColumnMasterKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);

			reader.Expect("WITH");

			var options = reader.Bracketed();

			reader.Finish();

			return new Ast.Statement.CreateColumnMasterKey(Values.Part(old, old.Name), options);
		}

		/// <summary><c>WITH IDENTITY = 'i' [, SECRET = 's']</c>.</summary>
		static (Ast.LiteralValue.String Identity, Ast.LiteralValue.String? Secret) Credentials(Reader reader)
		{
			reader.Expect("WITH", "IDENTITY");
			reader.ExpectPunctuation("=");

			var identity = reader.String();
			Ast.LiteralValue.String? secret = null;

			if (reader.TakePunctuation(","))
			{
				reader.Expect("SECRET");
				reader.ExpectPunctuation("=");
				secret = reader.String();
			}

			return (identity, secret);
		}

		internal static Ast.Statement Credential(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var (identity, secret) = Credentials(reader);
			var provider = reader.Take("FOR", "CRYPTOGRAPHIC", "PROVIDER") ? reader.Name() : null;

			reader.Finish();

			return old.Verb switch
			{
				"CREATE" => new Ast.Statement.CreateCredential(name, identity) { Secret = secret, Provider = provider },
				"ALTER"  => new Ast.Statement.AlterCredential(name, identity) { Secret = secret, Provider = provider },
				_        => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the verb `{old.Verb}`)"),
			};
		}

		internal static Ast.Statement DatabaseScopedCredential(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var (identity, secret) = Credentials(reader);

			reader.Finish();

			return old.Verb switch
			{
				"CREATE" => new Ast.Statement.CreateDatabaseScopedCredential(name, identity) { Secret = secret },
				"ALTER"  => new Ast.Statement.AlterDatabaseScopedCredential(name, identity) { Secret = secret },
				_        => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the verb `{old.Verb}`)"),
			};
		}

		internal static Ast.Statement CryptographicProvider(Defining old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (old.Verb == "CREATE")
			{
				reader.Expect("FROM", "FILE");
				reader.ExpectPunctuation("=");

				var file = reader.String();

				reader.Finish();

				return new Ast.Statement.CreateCryptographicProvider(name, file);
			}

			if (old.Verb != "ALTER")
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the verb `{old.Verb}`)");

			Ast.CryptographicProviderChange change;

			if (reader.Take("ENABLE"))
				change = new Ast.CryptographicProviderChange.Enable();
			else if (reader.Take("DISABLE"))
				change = new Ast.CryptographicProviderChange.Disable();
			else
			{
				reader.Expect("FROM", "FILE");
				reader.ExpectPunctuation("=");
				change = new Ast.CryptographicProviderChange.FromFile(reader.String());
			}

			reader.Finish();

			return new Ast.Statement.AlterCryptographicProvider(name, change);
		}

		internal static Ast.Statement OpenSymmetricKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);

			reader.Expect("DECRYPTION", "BY");

			var by = reader.Encryptor();

			reader.Finish();

			return new Ast.Statement.OpenSymmetricKey(Values.Part(old, old.Name), by);
		}

		internal static Ast.Statement OpenMasterKey(Defining old)
		{
			var reader = new Reader(old, old.Tail);

			reader.Expect("DECRYPTION", "BY");

			var by = reader.PasswordEncryptor();

			reader.Finish();

			return new Ast.Statement.OpenMasterKey(by);
		}

		// ── Security policies ──────────────────────────────────────────────────────

		/// <summary>
		/// A policy's function, <c>dbo.f(a, b)</c>: read again as the one value it is, a call of a name with plain arguments.
		/// </summary>
		static Ast.PolicyFunction PolicyFunction(ISqlSpan old, Reader reader, string tail)
		{
			var name = reader.Qualified();

			// The arguments are the text between the brackets, read again as the values of a call.
			reader.ExpectPunctuation("(");

			var arguments = new List<string>();
			var depth     = 0;
			var current   = new StringBuilder();

			while (true)
			{
				if (reader.End)
					throw reader.Unread();

				var token = reader.NextToken();

				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" } && depth == 0)
					break;

				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;

				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "," } && depth == 0)
				{
					arguments.Add(current.ToString());
					current.Clear();

					continue;
				}

				current.Append(token.Text).Append(' ');
			}

			if (current.Length > 0 || arguments.Count > 0)
				arguments.Add(current.ToString());

			var values = arguments.Select(one => ValueOf(old, one)).ToSqlList();

			return new Ast.PolicyFunction(name, values);
		}

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(object owner, string text)
		{
			return Queries.Specified("SELECT " + text.Trim()) is { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] }
				? Values.Of(value)
				: throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (a value `{text.Trim()}`)");
		}

		static Ast.BlockOperation? BlockOperation(Reader reader)
		{
			if (reader.Take("AFTER", "INSERT"))
				return Ast.BlockOperation.AfterInsert;

			if (reader.Take("AFTER", "UPDATE"))
				return Ast.BlockOperation.AfterUpdate;

			if (reader.Take("BEFORE", "UPDATE"))
				return Ast.BlockOperation.BeforeUpdate;

			if (reader.Take("BEFORE", "DELETE"))
				return Ast.BlockOperation.BeforeDelete;

			return null;
		}

		static Ast.PolicyPredicate PolicyPredicate(ISqlSpan old, Reader reader, string tail)
		{
			var action = reader.Take("ADD") ? "ADD" : reader.Take("ALTER") ? "ALTER" : reader.Take("DROP") ? "DROP" : throw reader.Unread();
			var filter = reader.Take("FILTER");

			if (!filter)
				reader.Expect("BLOCK");

			reader.Expect("PREDICATE");

			if (action == "DROP")
			{
				reader.Expect("ON");

				var table = reader.Qualified();

				return filter ? new Ast.PolicyPredicate.DropFilter(table) : new Ast.PolicyPredicate.DropBlock(table, BlockOperation(reader));
			}

			var function = PolicyFunction(old, reader, tail);

			reader.Expect("ON");

			var on = reader.Qualified();

			return (filter, action) switch
			{
				(true, "ADD")    => new Ast.PolicyPredicate.AddFilter(function, on),
				(true, _)        => new Ast.PolicyPredicate.AlterFilter(function, on),
				(false, "ADD")   => new Ast.PolicyPredicate.AddBlock(function, on, BlockOperation(reader)),
				(false, _)       => new Ast.PolicyPredicate.AlterBlock(function, on, BlockOperation(reader)),
			};
		}

		static SqlList<Ast.PolicyPredicate> PolicyPredicates(ISqlSpan old, Reader reader, string tail)
		{
			var made = new List<Ast.PolicyPredicate> { PolicyPredicate(old, reader, tail) };

			while (reader.TakePunctuation(","))
				made.Add(PolicyPredicate(old, reader, tail));

			return made.ToSqlList();
		}

		internal static Ast.Statement SecurityPolicy(Defining old)
		{
			var tail   = old.Tail ?? "";
			var reader = new Reader(old, tail);
			var name   = Values.Name(old, old.Name);

			if (old.Verb == "CREATE")
			{
				var made = new Ast.Statement.CreateSecurityPolicy(name);

				if (reader.Is("ADD") || reader.Is("ALTER") || reader.Is("DROP"))
					made = made with { Predicates = PolicyPredicates(old, reader, tail) };

				if (reader.Take("WITH"))
					made = made with { Options = reader.Bracketed() };

				if (reader.Take("NOT", "FOR", "REPLICATION"))
					made = made with { NotForReplication = true };

				reader.Finish();

				return made;
			}

			if (old.Verb != "ALTER")
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the verb `{old.Verb}`)");

			Ast.SecurityPolicyChange change;

			if (reader.Take("ADD", "NOT", "FOR", "REPLICATION"))
				change = new Ast.SecurityPolicyChange.AddNotForReplication();
			else if (reader.Take("DROP", "NOT", "FOR", "REPLICATION"))
				change = new Ast.SecurityPolicyChange.DropNotForReplication();
			else if (reader.Take("WITH"))
			{
				reader.ExpectPunctuation("(");
				reader.Expect("STATE");
				reader.ExpectPunctuation("=");
				change = new Ast.SecurityPolicyChange.State(reader.OnOff() == true);
				reader.ExpectPunctuation(")");
			}
			else
				change = new Ast.SecurityPolicyChange.Predicates(PolicyPredicates(old, reader, tail));

			reader.Finish();

			return new Ast.Statement.AlterSecurityPolicy(name, change);
		}

		// ── Classifications and signatures ────────────────────────────────────────

		static SqlList<Ast.Option> ClassificationOptions(Statement.Classification old, string text)
		{
			var reader = new Reader(old, text);

			while (!reader.Is("WITH"))
			{
				if (reader.End)
					throw reader.Unread();

				reader.NextToken();
			}

			reader.Expect("WITH");

			var options = reader.Bracketed();

			// The statement's span may take the terminator with it.
			reader.TakePunctuation(";");
			reader.Finish();

			return options;
		}

		/// <summary>ADD SENSITIVITY CLASSIFICATION, on the common tree: its columns and options, where the rule says it was written.</summary>
		internal static Statement Classification(SqlList<string> columns, SqlList<Clause> options, SqlSpan at)
		{
			return Carry(new Statement.Classification(true, columns, options), at, Classification);
		}

		static Ast.Statement Classification(Statement.Classification old)
		{
			if (!old.Add)
				throw BridgeGap.Open(old, "inventory Statement.Classification (not an ADD)");

			// The old tree upper-cases the option names and the rank word; the words as written are read from the text.
			var options = Recover("Statement.Classification.Options", Spelled(old), text => ClassificationOptions(old, text), Each(old.Options, one => one is Clause.Option(var name, var value, _, _, _) && value is not null
				? new Ast.Option(new Ast.Identifier(name), Values.Of(value), [])
				: throw BridgeGap.Open(old, "inventory Statement.Classification (an option that is not a name and a value)")));

			return new Ast.Statement.AddSensitivityClassification(Each(old.Columns, one => Values.Name(old, one)), options);
		}

		/// <summary>
		/// A signed module, which the old tree keeps as text with its class before <c>::</c>: its class and its name, and the text
		/// as the writer prints it.
		/// </summary>
		internal static (Ast.SecurableClass? Class, Ast.QualifiedName Module) SignedModule(object old, string? text, string member)
		{
			var module = SqlTokens.Of(text ?? "");
			Ast.SecurableClass? kind = null;
			var name = module;

			if (module is [{ Kind: SqlTokens.Kind.Word, Text: var word }, { Text: ":" }, { Text: ":" }, ..])
			{
				kind = word.ToUpperInvariant() switch
				{
					"OBJECT"   => Ast.SecurableClass.Object,
					"ASSEMBLY" => Ast.SecurableClass.Assembly,
					"DATABASE" => Ast.SecurableClass.Database,
					_          => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the class {word})"),
				};
				name = module.Skip(3).ToList();
			}

			var dotted = Dotted(name) ?? throw BridgeGap.Open(old, $"inventory {NameOf(old)} (`{text}`)");

			Written(old, member, text!, kind is null ? dotted : $"{module[0].Text.ToUpperInvariant()}::{dotted}");

			return (kind, Values.Name(old, dotted));
		}

		/// <summary>
		/// A text the old tree keeps as written, which the round trip compares as the oracle does (<see cref="Same"/>): where it
		/// is written otherwise than the writer prints it, the statement is a gap, never a difference.
		/// </summary>
		static void Written(object old, string member, string text, string written)
		{
			if (!Same(old, member, written, text))
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (`{text}` kept as written, against `{written}` as the writer prints it, which the old tree compares exactly)");
		}

		/// <summary>
		/// What signs a module, kept by the old tree as one column reference's text each: <c>CERTIFICATE c WITH PASSWORD = 'p'</c>.
		/// </summary>
		internal static SqlList<Ast.SignedBy> Signers(object old, SqlList<Expression> by)
		{
			return Each(by, one =>
			{
				var text   = one is Expression.ColumnReference(var words) ? words : throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a signer that is {NameOf(one)})");
				var reader = new Reader(old, text);
				Ast.SignedBy signed;
				var written = new List<string>();

				if (reader.Take("CERTIFICATE"))
				{
					var name = reader.Name();

					written.Add($"CERTIFICATE {name.Text}");
					signed = new Ast.SignedBy.Certificate(new Ast.QualifiedName([name])) { Proof = Proof(reader, written) };
				}
				else if (reader.Take("ASYMMETRIC", "KEY"))
				{
					var name = reader.Name();

					written.Add($"ASYMMETRIC KEY {name.Text}");
					signed = new Ast.SignedBy.AsymmetricKey(new Ast.QualifiedName([name])) { Proof = Proof(reader, written) };
				}
				else if (reader.Take("SYMMETRIC", "KEY"))
				{
					var name = reader.Name();

					written.Add($"SYMMETRIC KEY {name.Text}");
					signed = new Ast.SignedBy.SymmetricKey(name);
				}
				else if (reader.Take("PASSWORD"))
				{
					reader.ExpectPunctuation("=");

					var password = reader.String();

					written.Add($"PASSWORD = {Printed(password)}");
					signed = new Ast.SignedBy.Password(password);
				}
				else
					throw reader.Unread();

				reader.Finish();
				Written(old, BridgeText.SignerName, text, string.Join(' ', written));

				return signed;
			});
		}

		/// <summary>A string as the writer prints it: a national one with its <c>N</c>, which its text leaves out.</summary>
		static string Printed(Ast.LiteralValue.String value)
		{
			return (value.Kind == Ast.StringLiteralKind.National ? "N" : "") + value.Text;
		}

		static Ast.SignatureProof? Proof(Reader reader, List<string> written)
		{
			if (!reader.Take("WITH"))
				return null;

			if (reader.Take("PASSWORD"))
			{
				reader.ExpectPunctuation("=");

				var password = reader.String();

				written.Add($"WITH PASSWORD = {Printed(password)}");

				return new Ast.SignatureProof.Password(password);
			}

			reader.Expect("SIGNATURE");
			reader.ExpectPunctuation("=");

			var signature = reader.Hex();

			written.Add($"WITH SIGNATURE = 0x{signature.Text}");

			return new Ast.SignatureProof.Signature(signature);
		}

		/// <summary>ADD SIGNATURE, on the common tree: what signs, the module it signs and whether it countersigns.</summary>
		internal static Statement AddSignature(SqlList<Expression> by, string? to, bool counter)
		{
			var old = new Defining("Statement.AddSignature", "");

			return Hold(old, (by, to, counter), static (old, state) =>
			{
				var (kind, module) = SignedModule(old, state.to, "Statement.AddSignature.To");

				return new Ast.Statement.AddSignature(state.counter, module, Signers(old, state.by)) { Class = kind };
			});
		}

		internal static Ast.Statement Role(Defining old)
		{
			return new Ast.Statement.CreateRole { Name = Values.Part(old, old.Name), Authorization = Owner(old, old.Tail) };
		}
	}
}
