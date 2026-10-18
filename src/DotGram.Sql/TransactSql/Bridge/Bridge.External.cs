using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

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
				Statement.ExternalLibraryDefinition library   => Library(library),
				Statement.ExternalLanguageDefinition language => Language(language),
				Statement.ExternalModelDefinition model       => Model(model),
				Statement.ExternalResourcePoolDefinition pool => Pool(pool),
				_ when Owned(Owns, old)                       => throw Unconverted(old),
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

		// ── Libraries, languages, models and resource pools ───────────────────────

		/// <summary>
		/// <c>CREATE EXTERNAL LIBRARY n [AUTHORIZATION o] FROM (file), … WITH (LANGUAGE = 'l')</c> and <c>ALTER EXTERNAL LIBRARY n
		/// [AUTHORIZATION o] { SET | ADD } (file), … WITH (LANGUAGE = 'l')</c>.
		/// </summary>
		static Ast.Statement Library(Statement.ExternalLibraryDefinition old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var owner  = reader.Authorization();

			if (old.Verb == "CREATE")
			{
				reader.Expect("FROM");

				var files    = reader.Files(language: false);
				var language = reader.Language();

				reader.Finish();

				return new Ast.Statement.CreateExternalLibrary(name, files, language) { Authorization = owner };
			}

			if (old.Verb == "ALTER")
			{
				var set = reader.Take("SET");

				if (!set)
					reader.Expect("ADD");

				var files    = reader.Files(language: false);
				var language = reader.Language();

				reader.Finish();

				return new Ast.Statement.AlterExternalLibrary(name, set ? new Ast.ExternalLibraryChange.Set(files) : new Ast.ExternalLibraryChange.Add(files), language) { Authorization = owner };
			}

			throw reader.Unread();
		}

		/// <summary>
		/// <c>CREATE EXTERNAL LANGUAGE n [AUTHORIZATION o] FROM (file) [, (file)]</c> and <c>ALTER EXTERNAL LANGUAGE n [AUTHORIZATION o]
		/// { SET (file) | ADD (file) | REMOVE PLATFORM p }</c>.
		/// </summary>
		static Ast.Statement Language(Statement.ExternalLanguageDefinition old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);
			var owner  = reader.Authorization();

			if (old.Verb == "CREATE")
			{
				reader.Expect("FROM");

				var files = reader.Files(language: true);

				reader.Finish();

				return new Ast.Statement.CreateExternalLanguage(name, files) { Authorization = owner };
			}

			if (old.Verb == "ALTER")
			{
				Ast.ExternalLanguageChange change;

				if (reader.Take("REMOVE", "PLATFORM"))
				{
					change = new Ast.ExternalLanguageChange.RemovePlatform(reader.Take("WINDOWS") ? Ast.ExternalPlatform.Windows : reader.Take("LINUX") ? Ast.ExternalPlatform.Linux : throw reader.Unread());
				}
				else
				{
					var set = reader.Take("SET");

					if (!set)
						reader.Expect("ADD");

					var file = reader.File(language: true);

					change = set ? new Ast.ExternalLanguageChange.Set(file) : new Ast.ExternalLanguageChange.Add(file);
				}

				reader.Finish();

				return new Ast.Statement.AlterExternalLanguage(name, change) { Authorization = owner };
			}

			throw reader.Unread();
		}

		/// <summary>
		/// <c>CREATE EXTERNAL MODEL n [AUTHORIZATION o] WITH (settings)</c> and <c>ALTER EXTERNAL MODEL n SET (settings)</c>.
		/// </summary>
		static Ast.Statement Model(Statement.ExternalModelDefinition old)
		{
			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			if (old.Verb == "CREATE")
			{
				var owner = reader.Authorization();

				reader.Expect("WITH");

				var options = reader.Bracketed();

				reader.Finish();

				return new Ast.Statement.CreateExternalModel(name, options) { Authorization = owner };
			}

			if (old.Verb == "ALTER")
			{
				reader.Expect("SET");

				var options = reader.Bracketed();

				reader.Finish();

				return new Ast.Statement.AlterExternalModel(name, options);
			}

			throw reader.Unread();
		}

		/// <summary>
		/// <c>CREATE EXTERNAL RESOURCE POOL n [WITH (settings)]</c> and <c>ALTER EXTERNAL RESOURCE POOL n WITH (settings)</c>. A
		/// pool the grammar lets be named by a string has no place in the tree yet.
		/// </summary>
		static Ast.Statement Pool(Statement.ExternalResourcePoolDefinition old)
		{
			if (old.Name.StartsWith('\''))
				throw BridgeGap.Open(old, "inventory Statement.ExternalResourcePoolDefinition (a pool named by a string, `" + old.Name + "`, which the name of a record does not hold)");

			var reader = new Reader(old, old.Tail);
			var name   = Values.Part(old, old.Name);

			SqlList<Ast.Option> options = [];

			if (!reader.End)
			{
				reader.Expect("WITH");
				options = reader.Bracketed(pool: true);
			}

			reader.Finish();

			return old.Verb switch
			{
				"CREATE" => new Ast.Statement.CreateExternalResourcePool(name) { Options = options },
				"ALTER"  => new Ast.Statement.AlterExternalResourcePool(name, options),
				_        => throw reader.Unread(),
			};
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

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(object owner, string text)
		{
			return TransactSql.TransactSqlParser.TryParseSql("SELECT " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] } }] }
				? Values.Of(value)
				: throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (a value `{text.Trim()}`)");
		}

		/// <summary>The position each token of a text starts at, comments and blanks skipped as the tokens skip them.</summary>
		static List<int> Starts(string text)
		{
			var starts = new List<int>();
			var at     = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				at = SkipBlanks(text, at);
				starts.Add(at);
				at += token.Text.Length;
			}

			return starts;
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

		/// <summary>
		/// What follows a definition's name, read from left to right: each step is checked, and a text that is not read to its
		/// end, or not as the grammar reads it, is a gap.
		/// </summary>
		sealed class Reader
		{
			readonly List<SqlTokens.Token> _tokens;
			readonly List<int> _starts;
			int _at;

			/// <summary>
			/// The tokens and where each starts. A number with a signed exponent, <c>1E-3</c>, is cut at its sign by the tokens
			/// and is one number to the grammar: its three tokens are put together.
			/// </summary>
			public Reader(object old, string? tail)
			{
				this.old  = old;
				this.tail = tail;

				var tokens = SqlTokens.Of(tail ?? "");
				var starts = Starts(tail ?? "");

				_tokens = [];
				_starts = [];

				for (var at = 0; at < tokens.Count; at++)
				{
					if (tokens[at].Kind == SqlTokens.Kind.Number && System.Text.RegularExpressions.Regex.IsMatch(tokens[at].Text, "^(?:[0-9]+[.]?[0-9]*|[.][0-9]+)[eE]$")
						&& at + 2 < tokens.Count && tokens[at + 1] is { Kind: SqlTokens.Kind.Punctuation, Text: "-" or "+" } && tokens[at + 2].Kind == SqlTokens.Kind.Number
						&& System.Text.RegularExpressions.Regex.IsMatch(tokens[at + 2].Text, "^[0-9]+$") && starts[at + 1] == starts[at] + tokens[at].Text.Length && starts[at + 2] == starts[at + 1] + 1)
					{
						_tokens.Add(new SqlTokens.Token(SqlTokens.Kind.Number, tokens[at].Text + tokens[at + 1].Text + tokens[at + 2].Text));
						_starts.Add(starts[at]);
						at += 2;
					}
					else
					{
						_tokens.Add(tokens[at]);
						_starts.Add(starts[at]);
					}
				}
			}

			readonly object old;
			readonly string? tail;

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

			/// <summary>
			/// One setting: its words, and after <c>=</c> a value of one token, or after nothing a bracketed list of its own. In a
			/// language's file the value of CONTENT is a value expression, and in a pool the value of an affinity is ranges.
			/// </summary>
			public Ast.Option Setting(bool language = false, bool pool = false)
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

				if (language && words.Count == 1 && words[0].Text.Equals("CONTENT", StringComparison.OrdinalIgnoreCase))
					return new Ast.Option(name, ValueUntil(stopAtTo: false), []);

				if (pool && IsPunctuation("("))
					return new Ast.Option(name, Ranges(), []) { Words = extra };

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
			public SqlList<Ast.Option> Bracketed(bool language = false, bool pool = false)
			{
				ExpectPunctuation("(");

				var made = new List<Ast.Option>();

				if (IsPunctuation(","))
					throw Stray();

				if (!IsPunctuation(")"))
				{
					while (true)
					{
						made.Add(Setting(language, pool));

						if (!TakePunctuation(","))
							break;

						if (IsPunctuation(",") || IsPunctuation(")"))
							throw Stray();
					}
				}

				ExpectPunctuation(")");

				return made.ToSqlList();
			}

			/// <summary><c>AUTHORIZATION owner</c>, or null where none is written.</summary>
			public Ast.Identifier? Authorization()
			{
				if (!Take("AUTHORIZATION"))
					return null;

				return _at < _tokens.Count && _tokens[_at].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited ? Values.Part(old, _tokens[_at++].Text) : throw Unread();
			}

			/// <summary>One file, <c>(setting, …)</c>.</summary>
			public Ast.ExternalFile File(bool language)
			{
				ExpectPunctuation("(");

				var made = new List<Ast.Option> { Setting(language) };

				while (TakePunctuation(","))
					made.Add(Setting(language));

				ExpectPunctuation(")");

				return new Ast.ExternalFile(made.ToSqlList());
			}

			/// <summary>One file or more, joined by commas.</summary>
			public SqlList<Ast.ExternalFile> Files(bool language)
			{
				var made = new List<Ast.ExternalFile> { File(language) };

				while (TakePunctuation(","))
					made.Add(File(language));

				return made.ToSqlList();
			}

			/// <summary><c>WITH (LANGUAGE = 'language')</c>.</summary>
			public Ast.LiteralValue.String Language()
			{
				Expect("WITH");
				ExpectPunctuation("(");
				Expect("LANGUAGE");
				ExpectPunctuation("=");

				if (_at >= _tokens.Count || _tokens[_at].Kind != SqlTokens.Kind.String)
					throw Unread();

				var language = (Ast.LiteralValue.String)Values.String(_tokens[_at++].Text);

				ExpectPunctuation(")");

				return language;
			}

			/// <summary><c>(a TO b, c)</c>: the ranges of an affinity, each bound a value expression.</summary>
			public Ast.Expression.PoolRanges Ranges()
			{
				ExpectPunctuation("(");

				var ranges = new List<Ast.PartitionRange>();

				while (true)
				{
					var from = ValueUntil(stopAtTo: true);
					var to   = Take("TO") ? ValueUntil(stopAtTo: false) : null;

					ranges.Add(new Ast.PartitionRange(from, to));

					if (!TakePunctuation(","))
						break;
				}

				ExpectPunctuation(")");

				return new Ast.Expression.PoolRanges(ranges.ToSqlList());
			}

			/// <summary>
			/// A value expression read from the tokens up to the next comma or closing bracket outside brackets, or up to the word TO,
			/// as a select list reads one.
			/// </summary>
			Ast.Expression ValueUntil(bool stopAtTo)
			{
				var start = _at;
				var depth = 0;

				for (; _at < _tokens.Count; _at++)
				{
					var token = _tokens[_at];

					if (token.Kind == SqlTokens.Kind.Punctuation)
					{
						if (token.Text == "(")
							depth++;
						else if (token.Text == ")" && depth == 0)
							break;
						else if (token.Text == ")")
							depth--;
						else if (token.Text == "," && depth == 0)
							break;
					}
					else if (stopAtTo && depth == 0 && token.Kind == SqlTokens.Kind.Word && token.Text.Equals("TO", StringComparison.OrdinalIgnoreCase))
						break;
				}

				if (_at == start)
					throw Unread();

				// The text as it was written, from the first token to the end of the last.
				var from = _starts[start];
				var to   = _starts[_at - 1] + _tokens[_at - 1].Text.Length;

				return ValueOf(old, tail![from..to]);
			}

			BridgeGap Stray()
			{
				return BridgeGap.Missing(old, "a stray comma in a bracketed list of the PolyBase objects, which the tree keeps nothing for");
			}
		}
	}
}
