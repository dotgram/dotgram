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
				// The name holds its `@` or `@@`.
				case Expression.Variable variable:
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

				case Expression.DatePart part:
					PutIdentifier(part.Name);
					return true;

				// The braces and the escape's own word, and what stands in them: a call written without its
				// brackets where none were, since inside the braces the name is ODBC's.
				case Expression.OdbcEscape escape:
					Word("{");
					Hold();
					PutIdentifier(escape.Kind);

					if (!escape.Called && escape.Value is Expression.Invocation { Arguments.Count: 0 } bare)
						PutName(bare.Name);
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

						if (xml.Content is { } content)
							Word(content switch
							{
								XmlContent.Content  => "CONTENT",
								XmlContent.Document => "DOCUMENT",
								_                   => throw NoText(content),
							});

						PutName(schema);
						Close();
					}
					else if (xml.Content is not null)
						throw Misplaced(xml, "Content", "without a Schema, which alone writes it");

					return true;

				default:
					return false;
			}
		}
	}
}
