using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's External family (the external-resource statements): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutExternal(ISqlNode node)
		{
			switch (node)
			{
				case Statement.CreateExternalDataSource create:
					Word("CREATE EXTERNAL DATA SOURCE");
					PutExternalName(create.Name, "CREATE EXTERNAL DATA SOURCE");
					PutExternalWith(create, create.Options, CreateDataSourceSettings, "CREATE EXTERNAL DATA SOURCE", always: true);
					return true;

				// DataSourceBody's other arm: `SET` and the settings with no brackets, one at least.
				case Statement.AlterExternalDataSource alter:
					Word("ALTER EXTERNAL DATA SOURCE");
					PutExternalName(alter.Name, "ALTER EXTERNAL DATA SOURCE");

					if (alter.Options.Count == 0)
						throw Empty(alter, "Options");

					Word("SET");
					PutExternalSettings(alter, alter.Options, AlterDataSourceSettings, "ALTER EXTERNAL DATA SOURCE");
					return true;

				case Statement.CreateExternalFileFormat create:
					Word("CREATE EXTERNAL FILE FORMAT");
					PutExternalName(create.Name, "CREATE EXTERNAL FILE FORMAT");
					PutExternalWith(create, create.Options, FileFormatSettings, "CREATE EXTERNAL FILE FORMAT", always: true);
					return true;

				case Statement.CreateExternalTable table:
					PutExternalTable(table);
					return true;

				case Statement.CreateExternalLibrary create:
					Word("CREATE EXTERNAL LIBRARY");
					PutExternalName(create.Name, "CREATE EXTERNAL LIBRARY");
					PutExternalAuthorization(create.Authorization, "CREATE EXTERNAL LIBRARY");
					Word("FROM");
					PutExternalFiles(create, create.Files, LibraryFileSettings, "CREATE EXTERNAL LIBRARY", required: ["CONTENT"], most: int.MaxValue, distinct: false);
					PutExternalLanguage(create, create.Language, "CREATE EXTERNAL LIBRARY");
					return true;

				case Statement.AlterExternalLibrary alter:
					Word("ALTER EXTERNAL LIBRARY");
					PutExternalName(alter.Name, "ALTER EXTERNAL LIBRARY");
					PutExternalAuthorization(alter.Authorization, "ALTER EXTERNAL LIBRARY");

					switch (alter.Change)
					{
						case ExternalLibraryChange.Set set:
							Word("SET");
							PutExternalFiles(set, set.Files, LibraryFileSettings, "ALTER EXTERNAL LIBRARY", required: [], most: int.MaxValue, distinct: false);
							break;

						case ExternalLibraryChange.Add add:
							Word("ADD");
							PutExternalFiles(add, add.Files, LibraryFileSettings, "ALTER EXTERNAL LIBRARY", required: [], most: int.MaxValue, distinct: false);
							break;

						default:
							throw Unwritten(alter.Change);
					}

					PutExternalLanguage(alter, alter.Language, "ALTER EXTERNAL LIBRARY");
					return true;

				case Statement.CreateExternalLanguage create:
					Word("CREATE EXTERNAL LANGUAGE");
					PutExternalName(create.Name, "CREATE EXTERNAL LANGUAGE");
					PutExternalAuthorization(create.Authorization, "CREATE EXTERNAL LANGUAGE");
					Word("FROM");
					PutExternalFiles(create, create.Files, LanguageFileSettings, "CREATE EXTERNAL LANGUAGE", required: ["CONTENT", "FILE_NAME"], most: 2, distinct: true);
					return true;

				case Statement.AlterExternalLanguage alter:
					Word("ALTER EXTERNAL LANGUAGE");
					PutExternalName(alter.Name, "ALTER EXTERNAL LANGUAGE");
					PutExternalAuthorization(alter.Authorization, "ALTER EXTERNAL LANGUAGE");

					switch (alter.Change)
					{
						case ExternalLanguageChange.Set set:
							Word("SET");
							PutExternalFile(set.File, LanguageFileSettings, "ALTER EXTERNAL LANGUAGE");
							break;

						case ExternalLanguageChange.Add add:
							Word("ADD");
							PutExternalFile(add.File, LanguageFileSettings, "ALTER EXTERNAL LANGUAGE");
							break;

						case ExternalLanguageChange.RemovePlatform remove:
							Word("REMOVE PLATFORM");
							Word(remove.Platform switch
							{
								ExternalPlatform.Windows => "WINDOWS",
								ExternalPlatform.Linux   => "LINUX",
								_                        => throw NoText(remove.Platform),
							});
							break;

						default:
							throw Unwritten(alter.Change);
					}

					return true;

				case Statement.CreateExternalModel create:
					Word("CREATE EXTERNAL MODEL");
					PutExternalName(create.Name, "CREATE EXTERNAL MODEL");
					PutExternalAuthorization(create.Authorization, "CREATE EXTERNAL MODEL");
					PutExternalWith(create, create.Options, ModelSettings, "CREATE EXTERNAL MODEL", always: true);
					return true;

				// ModelOptions after SET: the same settings in brackets, none allowed.
				case Statement.AlterExternalModel alter:
					Word("ALTER EXTERNAL MODEL");
					PutExternalName(alter.Name, "ALTER EXTERNAL MODEL");
					Word("SET");
					Open();
					PutExternalSettings(alter, alter.Options, ModelSettings, "ALTER EXTERNAL MODEL");
					Close();
					return true;

				// A pool made need not be told anything; a pool altered is.
				case Statement.CreateExternalResourcePool create:
					Word("CREATE EXTERNAL RESOURCE POOL");
					PutExternalName(create.Name, "CREATE EXTERNAL RESOURCE POOL");
					PutExternalWith(create, create.Options, ExternalPoolSettings, "CREATE EXTERNAL RESOURCE POOL", always: false);
					return true;

				case Statement.AlterExternalResourcePool alter:
					Word("ALTER EXTERNAL RESOURCE POOL");
					PutExternalName(alter.Name, "ALTER EXTERNAL RESOURCE POOL");

					if (alter.Options.Count == 0)
						throw Empty(alter, "Options");

					PutExternalWith(alter, alter.Options, ExternalPoolSettings, "ALTER EXTERNAL RESOURCE POOL", always: false);
					return true;

				default:
					return false;
			}
		}

		// ── The catalogues ───────────────────────────────────────────────────────

		/// <summary>
		/// The forms of value a setting is read with, as the grammar's rules spell them: <c>ExternalName</c> (<c>Identifier</c> and
		/// not a dotted name), <c>Quoted</c>, <c>TSql.BinaryLiteral</c>, <c>Sql92.Lexical.Digits</c>, <c>ExactNumber</c>,
		/// <c>Sql92.Lexical.UnsignedNumericLiteral</c>, a word out of a list, a value expression; <c>Current</c> is the key
		/// word <c>VALUE</c>, which an expression reads as the current value.
		/// </summary>
		[Flags]
		enum ExternalShape
		{
			None   = 0,
			Name   = 1,
			String = 2,
			Hex    = 4,
			Digits = 8,
			Exact  = 16,
			Number = 32,
			Words  = 64,
			Value  = 128,
			Call   = 256,
			Current = 512,
			Ranges = 1024,

			// With Number: the exponent of an approximate number may have no digits (the grammar's TSql.NumericLiteral).
			OpenExponent = 2048,
		}

		/// <summary>
		/// One setting a place reads: the forms of its value, the words of its value where it is one of a closed list, whether the place
		/// lets it stand once only, and the settings of its own where its value is a bracketed list with no <c>=</c>.
		/// </summary>
		readonly record struct ExternalEntry(ExternalShape Shape, string[]? Words = null, bool Once = true, Dictionary<string, ExternalEntry>? Nested = null);

		static readonly ExternalEntry ExternalString = new(ExternalShape.String);
		static readonly ExternalEntry ExternalName   = new(ExternalShape.Name);

		/// <summary>A name the old grammar reads through the expression rule, which reads <c>VALUE</c> as the current value.</summary>
		static readonly ExternalEntry ExternalReadName = new(ExternalShape.Name | ExternalShape.Current);

		/// <summary>DataSourceShared: what a data source takes whether it is made or altered.</summary>
		static readonly Dictionary<string, ExternalEntry> DataSourceShared = new(StringComparer.OrdinalIgnoreCase)
		{
			["LOCATION"]           = ExternalString,
			["CONNECTION_OPTIONS"] = ExternalString,
			["CREDENTIAL"]         = ExternalName,
			["PUSHDOWN"]           = new(ExternalShape.Words, ["ON", "OFF"]),
		};

		/// <summary>DataSourceSyntax: a data source made, with the types this server knows and elastic query's and Hadoop's.</summary>
		static readonly Dictionary<string, ExternalEntry> CreateDataSourceSettings = new(DataSourceShared, StringComparer.OrdinalIgnoreCase)
		{
			["TYPE"]                      = new(ExternalShape.Words, ["BLOB_STORAGE", "HADOOP", "RDBMS", "SHARD_MAP_MANAGER"]),
			["RESOURCE_MANAGER_LOCATION"] = ExternalString,
			["DATABASE_NAME"]             = ExternalString,
			["SHARD_MAP_NAME"]            = ExternalString,
		};

		/// <summary>DataSourceChangeSyntax: a data source altered, which takes two types and the resource manager.</summary>
		static readonly Dictionary<string, ExternalEntry> AlterDataSourceSettings = new(DataSourceShared, StringComparer.OrdinalIgnoreCase)
		{
			["TYPE"]                      = new(ExternalShape.Words, ["BLOB_STORAGE", "HADOOP"]),
			["RESOURCE_MANAGER_LOCATION"] = ExternalString,
		};

		/// <summary>FormatOption: a delimited text's settings, each as often as wanted.</summary>
		static readonly Dictionary<string, ExternalEntry> FormatOptionSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["FIELD_TERMINATOR"]  = new(ExternalShape.String, Once: false),
			["STRING_DELIMITER"]  = new(ExternalShape.String, Once: false),
			["DATE_FORMAT"]       = new(ExternalShape.String, Once: false),
			["ENCODING"]          = new(ExternalShape.String, Once: false),
			["PARSER_VERSION"]    = new(ExternalShape.String, Once: false),
			["FIRST_ROW"]         = new(ExternalShape.Digits, Once: false),
			["USE_TYPE_DEFAULT"]  = new(ExternalShape.Words, ["TRUE", "FALSE"], Once: false),
		};

		/// <summary>FileFormatOption. The type twice, an option twice and the brackets twice are all read.</summary>
		static readonly Dictionary<string, ExternalEntry> FileFormatSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["FORMAT_TYPE"]     = new(ExternalShape.Words, ["DELIMITEDTEXT", "RCFILE", "ORC", "PARQUET", "DELTA", "JSON"], Once: false),
			["DATA_COMPRESSION"] = new(ExternalShape.String, Once: false),
			["SERDE_METHOD"]    = new(ExternalShape.String, Once: false),
			["FORMAT_OPTIONS"]  = new(ExternalShape.None, Once: false, Nested: FormatOptionSettings),
		};

		/// <summary>
		/// ExternalTableSyntax: an external table's settings, each once. A location is a string, a data source and a file format
		/// one part of a name, a rejection <c>VALUE</c> or <c>PERCENTAGE</c> and its numbers unsigned with no exponent; elastic
		/// query's and Synapse serverless's follow.
		/// </summary>
		static readonly Dictionary<string, ExternalEntry> ExternalTableSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["LOCATION"]              = ExternalString,
			["REJECTED_ROW_LOCATION"] = ExternalString,
			["DATA_SOURCE"]           = ExternalReadName,
			["FILE_FORMAT"]           = ExternalReadName,
			["REJECT_TYPE"]           = new(ExternalShape.Words | ExternalShape.Current, ["PERCENTAGE"]),
			["REJECT_VALUE"]          = new(ExternalShape.Exact),
			["REJECT_SAMPLE_VALUE"]   = new(ExternalShape.Exact),
			["SCHEMA_NAME"]           = ExternalString,
			["OBJECT_NAME"]           = ExternalString,
			["TABLE_OPTIONS"]         = ExternalString,
			["DISTRIBUTION"]          = new(ExternalShape.Words | ExternalShape.Call, ["REPLICATED", "ROUND_ROBIN"]),
		};

		/// <summary>LibraryFileOption: a file's content — a string, a binary string, NONE, a name or a number — and the platform it is for.</summary>
		static readonly Dictionary<string, ExternalEntry> LibraryFileSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["CONTENT"]  = new(ExternalShape.String | ExternalShape.Hex | ExternalShape.Number | ExternalShape.Name | ExternalShape.Words, ["NONE"], Once: false),
			["PLATFORM"] = new(ExternalShape.Words, ["WINDOWS", "LINUX"], Once: false),
		};

		/// <summary>LanguageOption: a file's content, which is any value, its names as strings, and the platform it is for.</summary>
		static readonly Dictionary<string, ExternalEntry> LanguageFileSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["CONTENT"]               = new(ExternalShape.Value, Once: false),
			["FILE_NAME"]             = new(ExternalShape.String, Once: false),
			["PARAMETERS"]            = new(ExternalShape.String, Once: false),
			["ENVIRONMENT_VARIABLES"] = new(ExternalShape.String, Once: false),
			["PLATFORM"]              = new(ExternalShape.Words, ["WINDOWS", "LINUX"], Once: false),
		};

		/// <summary>ModelOption: seven settings and no other, each once.</summary>
		static readonly Dictionary<string, ExternalEntry> ModelSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["MODEL_TYPE"]         = new(ExternalShape.Words, ["EMBEDDINGS"]),
			["CREDENTIAL"]         = ExternalName,
			["LOCATION"]           = ExternalString,
			["API_FORMAT"]         = ExternalString,
			["MODEL"]              = ExternalString,
			["PARAMETERS"]         = ExternalString,
			["LOCAL_RUNTIME_PATH"] = ExternalString,
		};

		/// <summary>ExternalPoolOption: the percentages and processes, and the affinity to CPUs or to NUMA nodes, as often as wanted.</summary>
		static readonly Dictionary<string, ExternalEntry> ExternalPoolSettings = new(StringComparer.OrdinalIgnoreCase)
		{
			["MAX_CPU_PERCENT"]      = new(ExternalShape.Digits, Once: false),
			["MAX_MEMORY_PERCENT"]   = new(ExternalShape.Digits, Once: false),
			["MAX_PROCESSES"]        = new(ExternalShape.Digits, Once: false),
			["AFFINITY CPU"]         = new(ExternalShape.Words | ExternalShape.Ranges, ["AUTO"], Once: false),
			["AFFINITY NUMANODE"]    = new(ExternalShape.Ranges, Once: false),
		};

		// ── Names ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// A name of one part, as the grammar's <c>Identifier</c> reads it: a plain word that is neither a reserved word nor a
		/// variable (<c>@x</c> is a variable, and a statement written with it does not read), a name in brackets or in double quotes.
		/// </summary>
		void PutExternalName(Identifier name, string place)
		{
			CheckExternalName(name, place);
			PutIdentifier(name);
		}

		/// <summary>
		/// The text of an identifier as the grammar's <c>Identifier</c> reads one: <c>TSqlIdentifier</c> is a regular word, a name in
		/// double quotes, a name in brackets or a temporary name, and a string, a Unicode-delimited name and a variable are none.
		/// </summary>
		static void CheckExternalName(Identifier name, string place)
		{
			var text = name.Text;

			switch (name.Style)
			{
				case IdentifierStyle.Regular when name.UnicodeEscape is null && IsRegularName(text):
					return;

				case IdentifierStyle.Bracketed when name.UnicodeEscape is null && IsDelimitedName(text, '[', ']'):
				case IdentifierStyle.Delimited when name.UnicodeEscape is null && IsDelimitedName(text, '"', '"'):
					return;
			}

			throw Misplaced(name, "Text", $"that no name in {place} reads: a plain name begins with a letter or _ (after # or ## for a temporary name) and goes on with letters, digits and _ @ # $; @x is a variable, and a name in brackets or double quotes is closed");
		}

		/// <summary>
		/// A plain name: a letter or <c>_</c> and then letters, digits and <c>_ @ # $</c> (<c>TSql.RegularIdentifier</c>), or a temporary
		/// name, which is that after one <c>#</c> or two (<c>TSql.TemporaryName</c>), or a pseudo-column, which is that after a
		/// <c>$</c> (<c>TSql.PseudoColumn</c>: <c>$ACTION</c>, <c>$node_id</c>), or a system function's, after <c>::</c>.
		/// The grammar reads UTF-16 units, so a letter outside the basic plane is none.
		/// </summary>
		static bool IsRegularName(string text)
		{
			var at = 0;

			if (text.StartsWith("::", StringComparison.Ordinal))
				at = 2;
			else if (text.Length > 0 && text[0] == '#')
				at = text.Length > 1 && text[1] == '#' ? 2 : 1;
			else if (text.Length > 0 && text[0] == '$')
				at = 1;

			if (at >= text.Length || !(char.IsLetter(text[at]) || text[at] == '_'))
				return false;

			for (at++; at < text.Length; at++)
				if (!(char.IsLetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
					return false;

			return true;
		}

		/// <summary>A name from its opening to its closing delimiter, a delimiter inside it doubled.</summary>
		static bool IsDelimitedName(string text, char open, char close)
		{
			if (text.Length < 2 || text[0] != open || text[text.Length - 1] != close)
				return false;

			for (var at = 1; at < text.Length - 1; at++)
			{
				if (text[at] != close)
					continue;

				if (at + 1 >= text.Length - 1 || text[at + 1] != close)
					return false;

				at++;
			}

			return true;
		}

		/// <summary>
		/// A string as the grammar's <c>Quoted</c> reads it: the plain or the national one, closed, its quotes inside doubled; no
		/// character set, no Unicode escape.
		/// </summary>
		static bool IsQuoted(LiteralValue.String text)
		{
			if (text.Kind is not (StringLiteralKind.Character or StringLiteralKind.National) || text.CharacterSet is not null || text.UnicodeEscape is not null)
				return false;

			return IsDelimitedName(text.Text, '\'', '\'');
		}

		// ── Libraries, languages and pools ────────────────────────────────────────

		/// <summary><c>AUTHORIZATION owner</c>, or nothing where there is no owner.</summary>
		void PutExternalAuthorization(Identifier? owner, string place)
		{
			if (owner is null)
				return;

			Word("AUTHORIZATION");
			PutExternalName(owner, place);
		}

		/// <summary>
		/// <c>WITH (LANGUAGE = 'language')</c>: the language as the grammar's <c>Quoted</c> reads it.
		/// </summary>
		void PutExternalLanguage(ISqlNode owner, LiteralValue.String language, string place)
		{
			if (!IsQuoted(language))
				throw Misplaced(owner, "Language", $"that is no string the grammar's Quoted reads, in {place}");

			Word("WITH");
			Open();
			Word("LANGUAGE");
			Word("=");
			PutExpression(language);
			Close();
		}

		/// <summary>
		/// The files of a library or a language, <c>(setting, …), …</c>: one at least and <paramref name="most"/> at most, each
		/// naming the settings the place wants in it (<paramref name="required"/>), and, where <paramref name="distinct"/> says so,
		/// two files for two platforms that differ.
		/// </summary>
		void PutExternalFiles(ISqlNode owner, SqlList<ExternalFile> files, Dictionary<string, ExternalEntry> catalogue, string place, string[] required, int most, bool distinct)
		{
			if (files.Count == 0)
				throw Empty(owner, "Files");

			if (files.Count > most)
				throw Misplaced(owner, "Files", $"more than {most} in {place}");

			foreach (var file in files)
			{
				if (file.Options.Count == 0)
					throw Empty(file, "Options");

				foreach (var name in required)
					if (!file.Options.Any(one => one.Words.Count == 0 && Plain(one.Name, name)))
						throw Misplaced(file, "Options", $"without {name}, which {place} reads in every file");
			}

			// The engine reads the platforms of a file as a set, and a file that names none is for Windows (Msg 39112).
			if (distinct && files.Count == 2 && ExternalPlatforms(files[0]) == ExternalPlatforms(files[1]))
				throw Misplaced(owner, "Files", $"for the same platforms in {place}, which reads two files for two (Msg 39112)");

			for (var at = 0; at < files.Count; at++)
			{
				if (at > 0)
					Tight(",");

				PutExternalFile(files[at], catalogue, place);
			}
		}

		/// <summary>A file's settings in brackets, one at least.</summary>
		void PutExternalFile(ExternalFile file, Dictionary<string, ExternalEntry> catalogue, string place)
		{
			if (file.Options.Count == 0)
				throw Empty(file, "Options");

			Open();
			PutExternalSettings(file, file.Options, catalogue, place);
			Close();
		}

		/// <summary>
		/// The platforms a file is for as a set: one bit for Windows and one for Linux, and Windows for a file that names none
		/// (<c>Syntax.LanguageFiles</c>).
		/// </summary>
		static int ExternalPlatforms(ExternalFile file)
		{
			var platforms = 0;

			foreach (var option in file.Options)
				if (option.Words.Count == 0 && Plain(option.Name, "PLATFORM") && option.Value is Expression.Reference { Name: [{ } platform] })
					platforms |= Plain(platform, "LINUX") ? 2 : 1;

			return platforms == 0 ? 1 : platforms;
		}

		/// <summary>The ranges of an affinity, <c>(0 TO 3, 5)</c>: one at least, each a value or two joined by TO.</summary>
		void PutPoolRanges(Expression.PoolRanges ranges)
		{
			if (ranges.Ranges.Count == 0)
				throw Empty(ranges, "Ranges");

			Open();

			for (var at = 0; at < ranges.Ranges.Count; at++)
			{
				if (at > 0)
					Tight(",");

				CheckExternalValue(ranges.Ranges[at].From, "an affinity");
				PutExpression(ranges.Ranges[at].From);

				if (ranges.Ranges[at].To is { } to)
				{
					CheckExternalValue(to, "an affinity");
					Word("TO");
					PutExpression(to);
				}
			}

			Close();
		}

		// ── External tables ───────────────────────────────────────────────────────

		/// <summary>
		/// T-SQL: CREATE EXTERNAL TABLE (Transact-SQL), CREATE EXTERNAL TABLE AS SELECT (CETAS) (Transact-SQL). The columns as the
		/// grammar reads any table's and then where they are, or the settings and the query that says the columns; the
		/// settings' brackets are always written.
		/// </summary>
		void PutExternalTable(Statement.CreateExternalTable table)
		{
			Word("CREATE EXTERNAL TABLE");
			PutExternalTableName(table.Name);

			switch (table.Contents)
			{
				case TableContents.Elements elements:
					if (elements.Items.Count == 0)
						throw Empty(elements, "Items");

					PutElements(elements.Items);
					PutExternalWith(table, table.Options, ExternalTableSettings, "CREATE EXTERNAL TABLE", always: true);
					break;

				// The query's own form: the name, the settings and the query bare after AS, with no columns before it.
				case TableContents.AsQuery query:
					if (query.Columns.Count > 0)
						throw Misplaced(query, "Columns", "on an external table made of a query, whose columns the query says");

					if (query.Data is not null)
						throw Misplaced(query, "Data", "on an external table made of a query, which writes no WITH [NO] DATA");

					PutExternalWith(table, table.Options, ExternalTableSettings, "CREATE EXTERNAL TABLE AS SELECT", always: true);
					Word("AS");
					PutQuery(query.Query);
					break;

				default:
					throw Misplaced(table, "Contents", "other than columns or a query");
			}
		}

		/// <summary>
		/// A table's name as the grammar's <c>QualifiedName</c> reads it: names of the kinds <see cref="CheckExternalName"/> reads, and
		/// an empty part between two dots (<c>db..t</c>) where a part is left out; the last is always named.
		/// </summary>
		void PutExternalTableName(QualifiedName name)
		{
			for (var at = 0; at < name.Count; at++)
			{
				var part = name[at];

				if (part.Text.Length == 0 && part.Style == IdentifierStyle.Regular && at < name.Count - 1)
					continue;

				CheckExternalName(part, "CREATE EXTERNAL TABLE");
			}

			PutName(name);
		}

		// ── Settings ──────────────────────────────────────────────────────────────

		/// <summary>
		/// <c>WITH (setting, …)</c> with the brackets, held to the place's catalogue. <paramref name="always"/> says the place
		/// writes the brackets however few settings there are; otherwise nothing is written for no setting.
		/// </summary>
		void PutExternalWith(ISqlNode owner, SqlList<Option> options, Dictionary<string, ExternalEntry> catalogue, string place, bool always)
		{
			if (options.Count == 0 && !always)
				return;

			Word("WITH");
			Open();
			PutExternalSettings(owner, options, catalogue, place);
			Close();
		}

		/// <summary>A list of settings joined by commas, each a name the catalogue reads, once where the place reads it once.</summary>
		void PutExternalSettings(ISqlNode owner, SqlList<Option> options, Dictionary<string, ExternalEntry> catalogue, string place)
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			for (var at = 0; at < options.Count; at++)
			{
				if (at > 0)
					Tight(",");

				PutExternalEntry(options[at], catalogue, place, seen);
			}
		}

		void PutExternalEntry(Option option, Dictionary<string, ExternalEntry> catalogue, string place, HashSet<string> seen)
		{
			// A bracketed list of settings holds settings, so the walk goes a level deeper with each; the catalogues are shallow, but
			// the cycle is there to be checked.
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, option, (catalogue, place, seen), static (into, node, state) => into.PutExternalEntry(node, state.catalogue, state.place, state.seen));

				return;
			}

			var key = KeyOf(option, place);

			if (!catalogue.TryGetValue(key, out var entry))
				throw Misplaced(option, "Name", $"in {place}, which reads no setting {key}");

			if (!seen.Add(key) && entry.Once)
				throw Misplaced(option, "Name", $"twice in {place}, which reads {key} once");

			if (option.EqualsOmitted)
				throw Misplaced(option, "EqualsOmitted", $"in {place}, whose settings are written with = (or with a bracketed list and no =), never the omitted =");

			if (option.Order is not null || option.Partitions is not null || option.Place is not null)
				throw Misplaced(option, "an order, partitions or a place", $"in {place}, whose settings are a name and a value");

			PutKeyWord(option.Name);

			foreach (var word in option.Words)
				PutKeyWord(word);

			if (entry.Nested is { } nested)
			{
				if (option.Value is not null)
					throw Misplaced(option, "Value", $"after {key} in {place}, which is written with a bracketed list and no =");

				Open();
				PutExternalSettings(option, option.Options, nested, place + " " + key);
				Close();

				return;
			}

			if (option.Options.Count > 0)
				throw Misplaced(option, "Options", $"after {key} in {place}, which reads none");

			if (option.Value is not { } value)
				throw Empty(option, "Value");

			Word("=");
			PutExternalValue(option, value, entry, key, place);
		}

		/// <summary>A setting's value, of a form the catalogue reads for its name.</summary>
		void PutExternalValue(Option option, Expression value, ExternalEntry entry, string key, string place)
		{
			var shape = entry.Shape;

			switch (value)
			{
				case Expression.Reference { Name: [{ } part] } when shape.HasFlag(ExternalShape.Words) && entry.Words is { } words && Plain(part, words):
					PutKeyWord(part);
					return;

				// VALUE is the key word the expressions read as the standard's current value, and only so: the plain name VALUE is that
				// key word too, and a tree that holds the name reads back as the other.
				case Expression.Current { Kind: CurrentValue.Value, TypeName: null, Precision: null } when shape.HasFlag(ExternalShape.Current):
					PutExpression(value);
					return;

				case Expression.Reference { Name: [{ } part] } when shape.HasFlag(ExternalShape.Name) && !(shape.HasFlag(ExternalShape.Current) && Plain(part, "VALUE")):
					PutExternalName(part, place);
					return;

				// DISTRIBUTION = SHARDED (column): a call of one column by name, and nothing else a call holds.
				case Expression.Invocation { Name: [{ } sharded], Arguments: [{ Value: var column, Name: null, NamedAssignment: false }] } call
					when shape.HasFlag(ExternalShape.Call) && Plain(sharded, "SHARDED") && call.Equals(new Expression.Invocation(call.Name, call.Arguments)):
					PutKeyWord(sharded);
					Call();

					switch (column)
					{
						case Expression.Reference { Name: [{ } part] } when !Plain(part, "VALUE"):
							PutExternalName(part, place);
							break;

						case Expression.Current { Kind: CurrentValue.Value, TypeName: null, Precision: null }:
							PutExpression(column);
							break;

						default:
							throw Misplaced(option, "Value", $"whose call holds other than a column of one name, in {place}");
					}

					Close();
					return;

				case Expression.PoolRanges ranges when shape.HasFlag(ExternalShape.Ranges):
					PutPoolRanges(ranges);
					return;

				case LiteralValue.String text when shape.HasFlag(ExternalShape.String) && IsQuoted(text):
				case LiteralValue.Binary { ZeroX: true } when shape.HasFlag(ExternalShape.Hex):
				case LiteralValue.Numeric numeric when IsNumberOf(numeric, shape):
					PutExpression(value);
					return;

				// A value expression, which the grammar reads whole (SetValue): checked down to its literals and names.
				case Expression when shape.HasFlag(ExternalShape.Value):
					CheckExternalValue(value, place);
					PutExpression(value);
					return;
			}

			throw Misplaced(option, "Value", $"of a form {key} is not read with in {place}");
		}

		/// <summary>
		/// A value expression at a place whose grammar reads any (<c>ValueExpression</c>), checked down to what the shared writer
		/// leaves to its caller: a literal T-SQL does not read, a number whose kind is not the one its text reads as, a name that is
		/// no <c>Identifier</c> (a variable written as a name, a blank in a plain word, an unclosed bracket), a variable that is
		/// none, and the plain name <c>VALUE</c>, which an expression reads as the current value.
		/// </summary>
		static void CheckExternalValue(Expression value, string place)
		{
			var checkedParts = new List<Identifier>();

			SqlWalker.Walk(value, node =>
			{
				switch (node)
				{
					// A variable's name is the text with its @.
					case Expression.Variable variable:
						if (!Regex.IsMatch(variable.Name.Text, @"^@@?[\p{L}_#][\p{L}\p{N}_@#$]*$") || variable.Name.Style != IdentifierStyle.Regular)
							throw Misplaced(variable, "Name", $"that is no variable in {place}: @ and a word");

						checkedParts.Add(variable.Name);
						break;

					// The parts of a name: a part may be left out between two dots, and a name does not begin with the key word VALUE.
					case Expression.Reference reference:
						for (var at = 0; at < reference.Name.Count; at++)
						{
							var part = reference.Name[at];

							checkedParts.Add(part);

							if (!(part.Text.Length == 0 && part.Style == IdentifierStyle.Regular && at < reference.Name.Count - 1))
								CheckExternalName(part, place);
						}

						if (Plain(reference.Name[0], "VALUE"))
							throw Misplaced(reference, "Name", $"that begins with VALUE in {place}, which an expression reads as the current value");

						break;

					case LiteralValue literal:
						if (!IsTransactSqlLiteral(literal))
							throw Misplaced(literal, "Text", $"that T-SQL does not read as a literal in {place}");

						break;

					case Identifier identifier when !checkedParts.Any(one => ReferenceEquals(one, identifier)):
						CheckExternalName(identifier, place);
						break;

					// Nodes the standard's grammar builds and a T-SQL value expression does not read: a row, `*`, `t.*`, a name given to an
					// argument, a column read as a member (but for one of the current value, <c>VALUE.a</c>), and a current value with a type or a precision or other than T-SQL's.
					case Expression.Row or Expression.Asterisk or Expression.Wildcard:
					case Argument { Name: not null }:
					case Expression.Member { Kind: MemberAccessKind.Dot, Arguments: null } member when member.Target is not Expression.Current { Kind: CurrentValue.Value, TypeName: null, Precision: null }:
					case Expression.Current { TypeName: not null }:
					case Expression.Current { Precision: not null }:
					case Expression.Current { Kind: not (CurrentValue.User or CurrentValue.SessionUser or CurrentValue.SystemUser or CurrentValue.CurrentUser or CurrentValue.Value or CurrentValue.Date or CurrentValue.Time or CurrentValue.Timestamp) }:
						throw Misplaced(node, "a node", $"that no value expression of T-SQL reads, in {place}");
				}

				return true;
			});
		}

		/// <summary>
		/// Whether T-SQL reads a literal: a number (its exponent may have no digits; a money amount has a closed list of currency signs,
		/// then blanks and a sign may come before its digits; a hexadecimal, octal or binary integer is a binary string to it), a plain or a
		/// national string with no character set, a binary string written <c>0x…</c>, or NULL.
		/// </summary>
		static bool IsTransactSqlLiteral(LiteralValue literal)
		{
			return literal switch
			{
				LiteralValue.Numeric { Kind: NumericLiteralKind.Money } money => Regex.IsMatch(money.Text, @"^[$¢-¥৲-৳฿៛₠-₱﷼﹩＄￠-￡￥-￦][ ]?[+-]?[0-9]+(?:\.[0-9]*)?\z"),
				LiteralValue.Numeric numeric => IsNumberOf(numeric, ExternalShape.Number | ExternalShape.OpenExponent),
				LiteralValue.String text     => IsQuoted(text),
				LiteralValue.Binary binary   => binary.ZeroX,
				LiteralValue.Null            => true,
				_                            => false,
			};
		}

		/// <summary>
		/// Whether a number is one the shape reads: digits (<c>Sql92.Lexical.Digits</c>), an exact number with no exponent
		/// (<c>ExactNumber</c>) or any unsigned numeric literal; the kind is the one its text reads as, so that the text read
		/// back is the same literal.
		/// </summary>
		static bool IsNumberOf(LiteralValue.Numeric numeric, ExternalShape shape)
		{
			var text = numeric.Text;

			if (text.Length > 0 && text.All(static one => one is >= '0' and <= '9'))
				return numeric.Kind == NumericLiteralKind.DecimalInteger && (shape & (ExternalShape.Digits | ExternalShape.Exact | ExternalShape.Number)) != 0;

			// A point with digits on either side, or an integer, and an exponent only where the shape reads one.
			var exponent = shape.HasFlag(ExternalShape.OpenExponent) ? "*" : "+";

			if (!Regex.IsMatch(text, @"^(?:[0-9]+\.[0-9]*|\.[0-9]+)(?:[eE][+-]?[0-9]" + exponent + @")?\z|^[0-9]+[eE][+-]?[0-9]" + exponent + @"\z"))
				return false;

			if (text.IndexOfAny(['e', 'E']) >= 0)
				return numeric.Kind == NumericLiteralKind.Approximate && (shape & ExternalShape.Number) != 0;

			return numeric.Kind == NumericLiteralKind.Decimal && (shape & (ExternalShape.Exact | ExternalShape.Number)) != 0;
		}
	}
}
