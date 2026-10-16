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
		void CheckNamedType(DataType.Named named)
		{
			if (!transactSql)
				return;

			if (!IsSystemTypeName(named.Name, out var last))
				throw Misplaced(named, "Name", "that is no system type of T-SQL, which reads back as a user-defined type");

			if (last.Equals("xml", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(named, "Name", "xml, which reads back as the xml type");

			if (named.Arguments.Count > 2 || named.Arguments.Any(static one => one is not LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger }))
				throw Misplaced(named, "Arguments", "other than one or two whole numbers");

			if (StandardTypeArguments.TryGetValue(last, out var most) && named.Arguments.Count <= most)
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
