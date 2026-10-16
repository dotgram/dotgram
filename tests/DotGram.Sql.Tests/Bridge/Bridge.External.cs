using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// External resources: external data sources, file formats, libraries, languages, models and resource pools.
	/// </summary>
	/// <remarks>
	/// The old tree keeps what follows the name of a definition as text (<c>Tail</c>), which the arms read token by token as the
	/// grammar's rules read it; a text the arm does not read to its end is a gap. The external tables are the Tables family's
	/// records, which call the arms here for the external case.
	/// </remarks>
	internal static class External
	{
		public static readonly string[] Owns =
		[
			"Statement.ExternalDataSourceDefinition", "Statement.ExternalFileFormatDefinition", "Statement.ExternalLibraryDefinition",
			"Statement.ExternalLanguageDefinition", "Statement.ExternalModelDefinition", "Statement.ExternalResourcePoolDefinition",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.ExternalDataSourceDefinition source => DataSource(source),
				Statement.ExternalFileFormatDefinition format => FileFormat(format),
				_ when Owned(Owns, old)                       => throw Inventory.Unconverted(old),
				_                                             => null,
			};

			return made is not null;
		}

		// ── Data sources and file formats ─────────────────────────────────────────

		/// <summary>
		/// <c>CREATE EXTERNAL DATA SOURCE n WITH (…)</c> and <c>ALTER EXTERNAL DATA SOURCE n SET a = v, …</c>. The grammar's
		/// <c>DataSourceBody</c> reads either body after either verb; the tree has the creating one's brackets and the altering
		/// one's list, and the other two are gaps.
		/// </summary>
		static Ast.Statement DataSource(Statement.ExternalDataSourceDefinition old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (old.Verb == "CREATE" && reader.Take("WITH"))
			{
				var options = reader.Bracketed();

				reader.Finish();

				return new Ast.Statement.CreateExternalDataSource(name, options);
			}

			if (old.Verb == "ALTER" && reader.Take("SET"))
			{
				var options = reader.Settings();

				reader.Finish();

				return new Ast.Statement.AlterExternalDataSource(name, options);
			}

			throw BridgeGap.Open(old, $"inventory Statement.ExternalDataSourceDefinition ({old.Verb} with `{old.Tail}`, which the grammar reads and the records do not hold)");
		}

		static Ast.Statement FileFormat(Statement.ExternalFileFormatDefinition old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			reader.Expect("WITH");

			var options = reader.Bracketed();

			reader.Finish();

			return new Ast.Statement.CreateExternalFileFormat(name, options);
		}

		// ── External tables ───────────────────────────────────────────────────────

		/// <summary>
		/// <c>CREATE EXTERNAL TABLE t (columns) WITH (…)</c>, called by the Tables family for a table declared with
		/// <c>External</c> set, with the elements it has carried. The old tree keeps the settings as options, which the Tables
		/// family carries for every <c>WITH (…)</c>.
		/// </summary>
		internal static Ast.Statement Table(Statement.TableDefinition old, SqlList<Ast.TableElement> elements)
		{
			if (old.Kind is not null || old.Placements.Length > 0)
				throw BridgeGap.Open(old, "inventory Statement.TableDefinition (CREATE EXTERNAL TABLE with a kind or a placement, which the grammar does not read)");

			return new Ast.Statement.CreateExternalTable(Values.Name(old, old.Name), new Ast.TableContents.Elements(elements))
			{
				Options = Each(old.Options, Tables.Option),
			};
		}

		/// <summary>
		/// <c>CREATE EXTERNAL TABLE t WITH (…) AS SELECT …</c>, called by the Tables family for a table made of a query with
		/// <c>External</c> set: the query alone, with no columns and no <c>WITH [NO] DATA</c>.
		/// </summary>
		internal static Ast.Statement TableAsSelect(Statement.CreateTableAsSelect old)
		{
			if (old.Columns is not null)
				throw BridgeGap.Open(old, "inventory Statement.CreateTableAsSelect (CREATE EXTERNAL TABLE with columns before AS, which the grammar does not read)");

			if (StatementOf(old.Body) is not Ast.Statement.Select { Hints.Count: 0 } select)
				throw BridgeGap.Open(old, "inventory Statement.CreateTableAsSelect (a query with OPTION, which TableContents.AsQuery cannot hold)");

			return new Ast.Statement.CreateExternalTable(Values.Name(old, old.Name), new Ast.TableContents.AsQuery([], select.Query, null))
			{
				Options = Each(old.Options, Tables.Option),
			};
		}

		// ── What follows a name ──────────────────────────────────────────────────

		/// <summary>
		/// A value of a setting, one token: a string, a number or a binary string, NULL, or a name.
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

			/// <summary>One setting: its words, and after <c>=</c> a value of one token, or after nothing a bracketed list of its own.</summary>
			public Ast.Option Setting()
			{
				var words = new List<SqlTokens.Token>();

				while (_at < _tokens.Count && _tokens[_at].Kind == SqlTokens.Kind.Word)
					words.Add(_tokens[_at++]);

				if (words.Count == 0)
					throw Unread();

				var name  = Values.Part(old, words[0].Text);
				var extra = words.Skip(1).Select(one => Values.Part(old, one.Text)).ToSqlList();

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

			/// <summary>
			/// Settings in brackets. The grammar lets a list of the PolyBase objects take commas loose — before the first setting,
			/// twice running, after the last — and the tree keeps nothing for one, so a text with such a comma is a gap.
			/// </summary>
			public SqlList<Ast.Option> Bracketed()
			{
				ExpectPunctuation("(");

				var made = new List<Ast.Option>();

				if (IsPunctuation(","))
					throw Stray();

				if (!IsPunctuation(")"))
				{
					while (true)
					{
						made.Add(Setting());

						if (!TakePunctuation(","))
							break;

						if (IsPunctuation(",") || IsPunctuation(")"))
							throw Stray();
					}
				}

				ExpectPunctuation(")");

				return made.ToSqlList();
			}

			BridgeGap Stray()
			{
				return BridgeGap.Missing(old, "a stray comma in a bracketed list of the PolyBase objects, which the tree keeps nothing for");
			}
		}
	}
}
