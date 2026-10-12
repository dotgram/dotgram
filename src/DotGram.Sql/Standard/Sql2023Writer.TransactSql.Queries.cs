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

				// T-SQL: OPENJSON (Transact-SQL).
				case TableSource.OpenJson json:
					Word("OPENJSON");
					Call();
					PutExpression(json.Json);

					if (json.Path is { } path)
					{
						Tight(",");
						PutExpression(path);
					}

					Close();
					PutSchema(json.Schema);
					PutAlias(json.Alias);
					return true;

				// T-SQL: OPENXML (Transact-SQL).
				case TableSource.OpenXml xml:
					Word("OPENXML");
					Call();
					PutExpression(xml.Document);
					Tight(",");
					PutExpression(xml.RowPattern);

					if (xml.Flags is { } flags)
					{
						Tight(",");
						PutExpression(flags);
					}

					Close();

					if (xml.SchemaTable is { } table)
					{
						if (xml.Schema is not null)
							throw Both(xml, "Schema", "SchemaTable");

						Word("WITH");
						PutName(table);
					}
					else
						PutSchema(xml.Schema);

					PutAlias(xml.Alias);
					return true;

				// T-SQL: OPENQUERY (Transact-SQL).
				case TableSource.OpenQuery query:
					Word("OPENQUERY");
					Call();
					PutIdentifier(query.Server);
					Tight(",");
					PutExpression(query.Query);
					Close();
					PutAlias(query.Alias);
					return true;

				// T-SQL: OPENROWSET (Transact-SQL). The three parts of a connection are apart by semicolons.
				case TableSource.OpenRowset rowset:
					if (rowset.Connection.Count is not (1 or 3))
						throw Misplaced(rowset, "Connection", "with other than a provider string or a data source, a user and a password");

					Word("OPENROWSET");
					Call();
					PutExpression(rowset.Provider);
					Tight(",");

					for (var at = 0; at < rowset.Connection.Count; at++)
					{
						if (at > 0)
							Tight(";");

						PutExpression(rowset.Connection[at]);
					}

					Tight(",");
					PutExpression(rowset.Object);
					Close();
					PutAlias(rowset.Alias);
					return true;

				// T-SQL: OPENROWSET BULK (Transact-SQL). An option is a word, or a word and its value.
				case TableSource.OpenRowsetBulk bulk:
					Word("OPENROWSET");
					Call();
					Word("BULK");
					PutExpression(bulk.File);

					foreach (var option in bulk.Options)
					{
						if (option.Options.Count > 0)
							throw Misplaced(option, "Options", "on an option of OPENROWSET BULK");

						Tight(",");
						PutIdentifier(option.Name);

						if (option.Value is { } value)
						{
							Word("=");
							PutExpression(value);
						}
					}

					Close();
					PutSchema(bulk.Schema);
					PutAlias(bulk.Alias);
					return true;

				// T-SQL: PREDICT (Transact-SQL).
				case TableSource.Predict predict:
					Word("PREDICT");
					Call();
					Word("MODEL =");
					PutExpression(predict.Model);
					Tight(",");
					Word("DATA =");
					PutSource(predict.Data);

					if (predict.Runtime is { } runtime)
					{
						Tight(",");
						Word("RUNTIME =");
						PutIdentifier(runtime);
					}

					Close();
					PutSchema(predict.Schema);
					PutAlias(predict.Alias);
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

		/// <summary>
		/// T-SQL: OPENJSON, OPENXML, OPENROWSET BULK (Transact-SQL). A rowset function's <c>WITH (…)</c>: each
		/// column's name and type, then its path or its ordinal, then <c>AS JSON</c>.
		/// </summary>
		void PutSchema(SqlList<RowsetColumn>? schema)
		{
			if (schema is not { } columns)
				return;

			if (columns.Count == 0)
				throw Empty(columns, "Schema");

			Word("WITH");
			Open();
			Each(columns, column =>
			{
				if (column.Type is null)
					throw Empty(column, "Type");

				if (column.Path is not null && column.Ordinal is not null)
					throw Both(column, "Path", "Ordinal");

				PutIdentifier(column.Name);
				PutType(column.Type);

				if (column.Path is { } path)
					Word(path);

				if (column.Ordinal is { } ordinal)
					Number(ordinal);

				if (column.AsJson)
					Word("AS JSON");
			});
			Close();
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
