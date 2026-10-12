using System;

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
					if (variable.Name.Style != IdentifierStyle.Regular || !variable.Name.Text.StartsWith("@", StringComparison.Ordinal))
						throw Misplaced(variable, "Name", "that is not a regular name starting with @");

					PutIdentifier(variable.Name);
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

						PutName(bare.Name);
					}
					else
						PutExpression(escape.Value);

					Tight("}");
					return true;

				case DataType.Named named:
					PutName(named.Name);

					if (named.Arguments.Count > 0)
					{
						Call();
						Each(named.Arguments, PutExpression);
						Close();
					}

					return true;

				case DataType.Xml xml:
					Word("XML");

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
