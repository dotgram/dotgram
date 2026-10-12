using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Queries family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutQueries(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: FROM clause plus JOIN, APPLY, PIVOT (T-SQL).
				case TableSource.Pivot pivot:
					PutSource(pivot.Source);
					Word("PIVOT");
					Open();
					PutExpression(pivot.Aggregate);
					Word("FOR");
					PutIdentifier(pivot.For);
					Word("IN");
					Open();
					Each(pivot.In, PutIdentifier);
					Close();
					Close();
					PutAlias(pivot.Alias);
					return true;

				case TableSource.Unpivot unpivot:
					PutSource(unpivot.Source);
					Word("UNPIVOT");
					Open();
					PutIdentifier(unpivot.Value);
					Word("FOR");
					PutIdentifier(unpivot.For);
					Word("IN");
					Open();
					Each(unpivot.In, PutIdentifier);
					Close();
					Close();
					PutAlias(unpivot.Alias);
					return true;

				// T-SQL: ODBC Scalar Functions (Transact-SQL).
				case TableSource.OdbcJoin odbc:
					Word("{");
					Hold();
					Word("OJ");
					PutSource(odbc.Source);
					Tight("}");
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// T-SQL: FOR Clause (Transact-SQL). The mode, then each option: a name alone (<c>TYPE</c>), a name
		/// and its value in brackets (<c>ROOT ('r')</c>), or a name and the one word it takes
		/// (<c>ELEMENTS XSINIL</c>, <c>BINARY BASE64</c>), which is an option of its own inside it.
		/// </summary>
		void PutFor(ForClause clause)
		{
			switch (clause)
			{
				case ForClause.Xml xml:
					Word("FOR XML");
					PutIdentifier(xml.Mode);
					PutForOptions(xml.Options);
					break;

				case ForClause.Json json:
					Word("FOR JSON");
					PutIdentifier(json.Mode);
					PutForOptions(json.Options);
					break;

				case ForClause.Browse:
					Word("FOR BROWSE");
					break;

				default:
					throw Unwritten(clause);
			}
		}

		void PutForOptions(SqlList<Option> options)
		{
			foreach (var option in options)
			{
				Tight(",");
				PutIdentifier(option.Name);

				if (option.Value is { } value)
				{
					if (option.Options.Count > 0)
						throw Both(option, "Value", "Options");

					Open();
					PutExpression(value);
					Close();
				}

				foreach (var word in option.Options)
				{
					if (word.Value is not null || word.Options.Count > 0)
						throw Misplaced(word, "Value or Options", "on the word a FOR clause's option takes");

					PutIdentifier(word.Name);
				}
			}
		}

		/// <summary>T-SQL: OPTION clause (Transact-SQL). <c>OPTION (RECOMPILE, MAXDOP (2))</c>.</summary>
		void PutQueryHints(SqlList<QueryHint> hints)
		{
			if (hints.Count == 0)
				return;

			Word("OPTION");
			Open();
			Each(hints, hint =>
			{
				if (hint is not QueryHint.Named named)
					throw Unwritten(hint);

				PutHint(named.Name, named.Arguments);
			});
			Close();
		}

		/// <summary>T-SQL: Table Hints (Transact-SQL). <c>WITH (NOLOCK, INDEX (ix))</c>.</summary>
		void PutTableHints(SqlList<TableHint> hints)
		{
			if (hints.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(hints, hint =>
			{
				if (hint is not TableHint.Named named)
					throw Unwritten(hint);

				PutHint(named.Name, named.Arguments);
			});
			Close();
		}

		void PutHint(Identifier name, SqlList<Expression> arguments)
		{
			PutIdentifier(name);

			if (arguments.Count > 0)
			{
				Open();
				Each(arguments, PutExpression);
				Close();
			}
		}

		/// <summary>T-SQL: WITH XMLNAMESPACES (Transact-SQL).</summary>
		void PutXmlNamespaces(SqlList<XmlNamespace> namespaces)
		{
			Word("XMLNAMESPACES");
			Open();
			Each(namespaces, one =>
			{
				if (one.Prefix is { } prefix)
				{
					PutExpression(one.Uri);
					Word("AS");
					PutIdentifier(prefix);
				}
				else
				{
					Word("DEFAULT");
					PutExpression(one.Uri);
				}
			});
			Close();
		}
	}
}
