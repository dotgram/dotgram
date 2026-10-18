using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Values family: variables, the typed built-ins, a date part, an ODBC escape, and the data types
		/// the standard does not spell. Answers false for every other node.
		/// </summary>
		bool TryPutValues(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: CONTAINS (Transact-SQL), FREETEXT (Transact-SQL). FullTextPredicate.
				case Expression.FullTextPredicate predicate:
					PutFullTextPredicate(predicate);
					return true;

				// The name holds its `@` or `@@`; any other name would be a column.
				case Expression.Variable variable:
					PutVariableName(variable, variable.Name);
					return true;

				case Expression.Convert convert:
					Word(convert.Try ? "TRY_CONVERT" : "CONVERT");
					Call();
					PutType(convert.Type);
					Tight(",");
					PutExpression(convert.Value);

					if (convert.Style is { } style)
					{
						Tight(",");
						PutExpression(style);
					}

					Close();
					return true;

				case Expression.Parse parse:
					Word(parse.Try ? "TRY_PARSE" : "PARSE");
					Call();
					PutExpression(parse.Value);
					Word("AS");
					PutType(parse.Type);

					if (parse.Culture is { } culture)
					{
						Word("USING");
						PutExpression(culture);
					}

					Close();
					return true;

				// T-SQL: DATEADD (Transact-SQL). The word of the table for the part and the spelling.
				case Expression.DatePart part:
					Word(DatePartWord(part.Kind, part.Spelling) ?? throw Misplaced(part, "Spelling", $"{part.Spelling} on {part.Kind}, which has no such word"));
					return true;

				// The braces and the escape's own word, and what stands in them: a call written without its
				// brackets where none were, since inside the braces the name is ODBC's.
				case Expression.OdbcEscape escape:
					// OdbcEscape: FN and a function called, one of the typed literals TS, D, T and GUID and a string, or ESCAPE of a LIKE.
					if (transactSql && (escape.Kind is not { Style: IdentifierStyle.Regular, UnicodeEscape: null } || !Plain(escape.Kind, "FN", "TS", "D", "T", "GUID", "ESCAPE")))
						throw Misplaced(escape, "Kind", "that is none of FN, TS, D, T, GUID or ESCAPE, the words an ODBC escape begins with");

					// `{fn v}` holds any value too (OdbcFunction's last branch); Called says a call is written with its brackets, and only a call is.
					if (transactSql && escape.Called && escape.Value is not Expression.Invocation)
						throw Misplaced(escape, "Called", "true on a value that is not a call, which reads back as one that was not called");

					// `{fn CONVERT (value, type)}` takes exactly the value and one of ODBC's type names.
					if (transactSql && escape.Called && escape.Value is Expression.Invocation { Name: [{ Style: IdentifierStyle.Regular } called] } converted && Plain(called, "CONVERT")
						&& !(converted.Arguments is [_, { Value: Expression.Reference { Name: [{ Style: IdentifierStyle.Regular } odbcType] } }] && OdbcTypes.Contains(odbcType.Text)))
						throw Misplaced(escape, "Value", "a CONVERT other than a value and one of ODBC's type names");

					if (transactSql && Plain(escape.Kind, "TS", "D", "T", "GUID") && escape.Value is not LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National })
						throw Misplaced(escape, "Value", "that is not a string, which the typed ODBC literals hold");

					Word("{");
					Hold();
					PutIdentifier(escape.Kind);

					if (!escape.Called && escape.Value is Expression.Invocation { Arguments.Count: 0 } bare)
					{
						// Only a call that is its name and nothing more is written without its brackets.
						if (!bare.Equals(new Expression.Invocation(bare.Name, [])))
							throw Misplaced(escape, "Called", "false on a call that holds more than its name");

						PutCallName(bare.Name, anyWord: true);
					}
					else if (escape.Called && escape.Value is Expression.Invocation call)
						PutInvocation(call, anyWord: true);
					else
						PutExpression(escape.Value);

					Tight("}");
					return true;

				case DataType.Named named:
					CheckNamedType(named);
					PutName(named.Name);

					if (named.Arguments.Count > 0)
					{
						Call();
						Each(named.Arguments, PutExpression);
						Close();
					}
					else if (named.Max)
					{
						Call();
						Word("MAX");
						Close();
					}

					return true;

				// T-SQL: Vector Data Type. VectorLength: whole dimensions and perhaps a base type, or MAX alone.
				case DataType.Vector vector:
					if (vector.Max && (vector.Dimensions is not null || vector.BaseType is not null))
						throw Misplaced(vector, "Max", "beside Dimensions or BaseType, where VectorLength reads MAX alone");

					if (vector.BaseType is not null && vector.Dimensions is null)
						throw Misplaced(vector, "BaseType", "without Dimensions, which it is written after");

					if (vector.Dimensions is < 0)
						throw Misplaced(vector, "Dimensions", "below zero, which Lexical.Digits does not write");

					PutTypeName(vector, "VECTOR");

					if (vector.Max)
					{
						Call();
						Word("MAX");
						Close();
					}
					else if (vector.Dimensions is { } dimensions)
					{
						Call();
						Word(dimensions.ToString(System.Globalization.CultureInfo.InvariantCulture));

						if (vector.BaseType is { } baseType)
						{
							Tight(",");
							Word(baseType switch
							{
								VectorBaseType.Float32 => "FLOAT32",
								VectorBaseType.Float16 => "FLOAT16",
								_                      => throw NoText(baseType),
							});
						}

						Close();
					}

					return true;

				case DataType.Xml xml:
					PutTypeName(xml, "XML");

					if (xml.Schema is { } schema)
					{
						Call();

						if (xml.Modifier is { } modifier)
							Word(modifier switch
							{
								XmlTypeModifier.Content  => "CONTENT",
								XmlTypeModifier.Document => "DOCUMENT",
								_                        => throw NoText(modifier),
							});

						PutName(schema);
						Close();
					}
					else if (xml.Modifier is not null)
						throw Misplaced(xml, "Modifier", "without a Schema, which alone writes it");

					return true;

				default:
					return false;
			}
		}

		/// <summary>The type names <c>{fn CONVERT (value, type)}</c> reads (<c>OdbcType</c>).</summary>
		static readonly HashSet<string> OdbcTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"SQL_BIGINT", "SQL_BINARY", "SQL_BIT", "SQL_CHAR", "SQL_DECIMAL", "SQL_DOUBLE", "SQL_FLOAT", "SQL_GUID", "SQL_INTEGER",
			"SQL_LONGVARBINARY", "SQL_LONGVARCHAR", "SQL_NUMERIC", "SQL_REAL", "SQL_SMALLINT", "SQL_DATE", "SQL_TIME", "SQL_TIMESTAMP",
			"SQL_TYPE_DATE", "SQL_TYPE_TIME", "SQL_TYPE_TIMESTAMP", "SQL_TINYINT", "SQL_VARBINARY", "SQL_VARCHAR", "SQL_WCHAR",
			"SQL_WLONGVARCHAR", "SQL_WVARCHAR",
		};

		/// <summary>
		/// T-SQL's system types, as of SQL Server 2025, and the one-word synonyms: a closed list, compared without case and without a
		/// name's delimiters. A type written by one of these names is <see cref="DataType.Named"/> (or the standard's own record), and
		/// any other name is <see cref="DataType.UserDefined"/>; <c>TransactSqlWriterTests</c> holds the list to the bridge's.
		/// </summary>
		internal static readonly HashSet<string> TransactSqlSystemTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"bigint", "binary", "bit", "char", "cursor", "date", "datetime", "datetime2", "datetimeoffset", "decimal",
			"float", "geography", "geometry", "hierarchyid", "image", "int", "json", "money", "nchar", "ntext",
			"numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant",
			"sysname", "table", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar",
			"vector", "xml", "dec", "integer", "character",
		};

		/// <summary>
		/// The system types the standard spells too, with the most arguments the standard's record takes: a type so spelt, with no more
		/// arguments than that, reads as the standard's record and never as <see cref="DataType.Named"/>.
		/// </summary>
		static readonly Dictionary<string, int> StandardTypeArguments = new(StringComparer.OrdinalIgnoreCase)
		{
			["int"] = 0, ["integer"] = 0, ["smallint"] = 0, ["bigint"] = 0, ["real"] = 0, ["date"] = 0, ["json"] = 0,
			["numeric"] = 2, ["decimal"] = 2, ["dec"] = 2, ["float"] = 1, ["time"] = 1,
			["char"] = 1, ["character"] = 1, ["varchar"] = 1, ["nvarchar"] = 1, ["nchar"] = 1, ["binary"] = 1, ["varbinary"] = 1,
		};

		/// <summary>Whether a name is a system type's: one part, or <c>sys</c> and one, the last of the list however it is quoted.</summary>
		static bool IsSystemTypeName(QualifiedName name, out string last)
		{
			last = "";

			if (name.Count is 0 or > 2 || name.Count == 2 && !Unquoted(name[0].Text).Equals("sys", StringComparison.OrdinalIgnoreCase))
				return false;

			last = Unquoted(name[name.Count - 1].Text).TrimEnd();

			return TransactSqlSystemTypes.Contains(last);
		}

		/// <summary>
		/// T-SQL: Data types (Transact-SQL). A <see cref="DataType.Named"/> is a system type of the list that the standard's record
		/// does not take, with whole numbers for its arguments; any other name reads as a user-defined type, <c>int</c> as the standard's
		/// integer, and <c>xml</c> as the xml type.
		/// </summary>
		/// <summary>
		/// T-SQL: CONTAINS (Transact-SQL), FREETEXT (Transact-SQL). FullTextColumns: <c>*</c> or <c>t.*</c> alone, bracketed
		/// or not; PROPERTY (c, 'p') alone and unbracketed; names, more than one only in brackets. FullTextSearch: a string
		/// or a variable. SearchLanguage: digits, a binary number, a string or a variable.
		/// </summary>
		void PutFullTextPredicate(Expression.FullTextPredicate predicate)
		{
			var columns = predicate.Columns;

			if (columns.Count == 0)
				throw Empty(predicate, "Columns");

			if (columns.Count > 1 && !predicate.ColumnsBracketed)
				throw Misplaced(predicate, "ColumnsBracketed", "false with more than one column");

			foreach (var column in columns)
			{
				var read = column switch
				{
					Expression.Reference { Name: var name } => name.Count > 0 && name.Parts.All(static part => part.Text.Length > 0),
					Expression.Asterisk or Expression.Wildcard { Kind: WildcardKind.Member } => columns.Count == 1,
					Expression.Invocation { Name: [var called] } when called.Text.Equals("PROPERTY", StringComparison.OrdinalIgnoreCase) => columns.Count == 1 && !predicate.ColumnsBracketed,
					_ => false,
				};

				if (!read)
					throw Misplaced(predicate, "Columns", "with a column other than a name, * or t.* alone, or PROPERTY (…) alone and unbracketed (FullTextColumns)");
			}

			if (predicate.Search is not (LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } or Expression.Variable))
				throw Misplaced(predicate, "Search", "other than a string or a variable (FullTextSearch)");

			if (predicate.Language is not (null or LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } or Expression.Variable or LiteralValue.Binary { ZeroX: true }
				or LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger, Text: [>= '0' and <= '9', ..] }))
				throw Misplaced(predicate, "Language", "other than digits, a binary number, a string or a variable (SearchLanguage)");

			Word(predicate.Kind switch
			{
				FullTextPredicateKind.Contains => "CONTAINS",
				FullTextPredicateKind.FreeText => "FREETEXT",
				_                              => throw NoText(predicate.Kind),
			});
			Call();

			if (predicate.ColumnsBracketed)
				Open();

			// FullTextAll: `*` and `t.*` stand in a full-text column list.
			Each(columns, column =>
			{
				star = true;
				PutExpression(column);
			});

			if (predicate.ColumnsBracketed)
				Close();

			Tight(",");
			PutExpression(predicate.Search);

			if (predicate.Language is { } language)
			{
				Tight(",");
				Word("LANGUAGE");
				PutExpression(language);
			}

			Close();
		}

		void CheckNamedType(DataType.Named named)
		{
			if (!transactSql)
				return;

			if (!IsSystemTypeName(named.Name, out var last))
				throw Misplaced(named, "Name", "that is no system type of T-SQL, which reads back as a user-defined type");

			if (last.Equals("xml", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(named, "Name", "xml, which reads back as the xml type");

			if (last.Equals("vector", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(named, "Name", "vector, which reads back as the vector type");

			if (named.Arguments.Count > 2 || named.Arguments.Any(static one => one is not LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger }))
				throw Misplaced(named, "Arguments", "other than one or two whole numbers");

			if (named.Max)
			{
				if (named.Arguments.Count > 0)
					throw Both(named, "Max", "Arguments");

				// AnyLength reads MAX after the types that have none of their own; a fixed string's is Msg 156, and a varying
				// one's is the standard's record.
				if (last.ToUpperInvariant() is "CHAR" or "CHARACTER" or "NCHAR" or "BINARY")
					throw Misplaced(named, "Max", $"on {last}, which never takes MAX (Msg 156)");

				if (last.ToUpperInvariant() is "VARCHAR" or "NVARCHAR" or "VARBINARY")
					throw Misplaced(named, "Max", $"on {last}, which reads back as the standard's record with its MAX");

				// ExactMax: an exact numeric sized MAX only where a column is declared or altered.
				if (last.ToUpperInvariant() is "DECIMAL" or "NUMERIC" or "DEC" && !columnType)
					throw Misplaced(named, "Max", $"on {last} outside a column, where it is Msg 156 (ExactMax)");
			}

			// A number too large for an int is no number to the standard's record, which reads such a type as Named.
			if (!named.Max && StandardTypeArguments.TryGetValue(last, out var most) && named.Arguments.Count <= most
				&& named.Arguments.All(static one => one is LiteralValue.Numeric numeric && int.TryParse(numeric.Text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _)))
				throw Misplaced(named, "Name", $"{last} with {named.Arguments.Count} arguments, which reads back as the standard's type");
		}

		/// <summary>
		/// T-SQL: DATEADD (Transact-SQL). A date part's word by its kind and spelling, or null where the kind has no
		/// such word.
		/// </summary>
		static string? DatePartWord(DatePartKind kind, DatePartSpelling spelling)
		{
			string[] words = kind switch
			{
				DatePartKind.Year        => ["YEAR", "YY", "YYYY"],
				DatePartKind.Quarter     => ["QUARTER", "QQ", "Q"],
				DatePartKind.Month       => ["MONTH", "MM", "M"],
				DatePartKind.DayOfYear   => ["DAYOFYEAR", "DY", "Y"],
				DatePartKind.Day         => ["DAY", "DD", "D"],
				DatePartKind.Week        => ["WEEK", "WK", "WW"],
				DatePartKind.Weekday     => ["WEEKDAY", "DW", "W"],
				DatePartKind.Hour        => ["HOUR", "HH"],
				DatePartKind.Minute      => ["MINUTE", "MI", "N"],
				DatePartKind.Second      => ["SECOND", "SS", "S"],
				DatePartKind.Millisecond => ["MILLISECOND", "MS"],
				DatePartKind.Microsecond => ["MICROSECOND", "MCS"],
				DatePartKind.Nanosecond  => ["NANOSECOND", "NS"],
				DatePartKind.TzOffset    => ["TZOFFSET", "TZ"],
				DatePartKind.IsoWeek     => ["ISO_WEEK", "ISOWK", "ISOWW"],
				_                        => throw NoText(kind),
			};

			var at = spelling switch
			{
				DatePartSpelling.Name               => 0,
				DatePartSpelling.Abbreviation       => 1,
				DatePartSpelling.SecondAbbreviation => 2,
				_                                   => throw NoText(spelling),
			};

			return at < words.Length ? words[at] : null;
		}
	}
}
