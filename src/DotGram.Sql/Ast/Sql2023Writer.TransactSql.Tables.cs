using System;
using System.Linq;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Tables family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutTables(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: the WITH clause of the CREATE and ALTER statements. A value and its unit, `10 MINUTES`.
				case Expression.Measured measured:
					PutExpression(measured.Value);
					PutIdentifier(measured.Unit);
					return true;

				// T-SQL: CREATE TABLE (Transact-SQL). An index written at the table.
				case TableElement.Index index:
					PutIndexDefinition(index.Definition);
					return true;

				case Constraint.Default @default:
					Word("DEFAULT");
					PutExpression(@default.Value);
					Word("FOR");
					PutIdentifier(@default.Column);
					return true;

				// T-SQL: CREATE TABLE (SQL Graph). The pairs, then the one action the tree holds.
				case Constraint.Connection connection:
					if (connection.Connections.Count == 0)
						throw Empty(connection, "Connections");

					Word("CONNECTION");
					Open();
					Each(connection.Connections, pair =>
					{
						PutName(pair.From);
						Word("TO");
						PutName(pair.To);
					});
					Close();

					if (connection.OnDeleteCascade)
						Word("ON DELETE CASCADE");

					return true;

				// T-SQL: ALTER TABLE (Transact-SQL). One index may be added alone; one column or constraint is the standard's.
				case AlterTableAction.Add add:
					if (add.Elements is not ({ Count: > 1 } or [TableElement.Index]))
						throw Misplaced(add, "Elements", "fewer than two and not one index, which the standard's AddColumn or AddConstraint holds");

					Word("ADD");
					Each(add.Elements, PutElement);
					return true;

				case AlterTableAction.Drop drop:
					PutDropList(drop);
					return true;

				case AlterTableAction.Switch @switch:
					Word("SWITCH");
					PutPartition(@switch.Partition);
					Word("TO");
					PutName(@switch.Target);
					PutPartition(@switch.TargetPartition);
					PutOptions(@switch.Options);
					return true;

				case AlterTableAction.SetOptions set:
					if (set.Options.Count == 0)
						throw Empty(set, "Options");

					Word("SET");
					Open();
					Each(set.Options, PutOption);
					Close();
					return true;

				// T-SQL: ALTER TABLE (Transact-SQL). `PARTITION = ALL` and `PARTITION = n` are each written, and neither may be both.
				case AlterTableAction.Rebuild rebuild:
					if (rebuild.All && rebuild.Partition is not null)
						throw Both(rebuild, "All", "Partition");

					if (rebuild.Partition is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular } word] } && word.Text.Equals("ALL", StringComparison.OrdinalIgnoreCase))
						throw Misplaced(rebuild, "Partition", "that is a plain ALL, which reads back as All");

					Word("REBUILD");

					if (rebuild.All)
						Word("PARTITION = ALL");
					else if (rebuild.Partition is { } partition)
					{
						Word("PARTITION =");
						PutExpression(partition);
					}

					PutOptions(rebuild.Options);
					return true;

				case AlterTableAction.CheckConstraints check:
					Word(check.Check == ConstraintCheck.Check ? "CHECK" : check.Check == ConstraintCheck.NoCheck ? "NOCHECK" : throw NoText(check.Check));
					Word("CONSTRAINT");
					PutNamesOrAll(check, check.All, check.Names);
					return true;

				case AlterTableAction.Triggers triggers:
					Word(triggers.Enable ? "ENABLE TRIGGER" : "DISABLE TRIGGER");
					PutNamesOrAll(triggers, triggers.All, triggers.Names);
					return true;

				// T-SQL: ALTER TABLE (Transact-SQL). Switched off, change tracking takes no option.
				case AlterTableAction.ChangeTracking tracking:
					if (!tracking.Enable && tracking.Options.Count > 0)
						throw Misplaced(tracking, "Options", "on DISABLE CHANGE_TRACKING");

					Word(tracking.Enable ? "ENABLE CHANGE_TRACKING" : "DISABLE CHANGE_TRACKING");
					PutOptions(tracking.Options);
					return true;

				// T-SQL: ALTER TABLE (Transact-SQL). `ADD MASKED WITH (FUNCTION = 'f()')` alone takes a function.
				case AlterColumnAction.Property property:
					if ((property.MaskFunction is not null) != (property.Add && property.Kind == ColumnPropertyKind.Masked))
						throw Misplaced(property, "MaskFunction", "other than on ADD MASKED, which must have one");

					// Four properties change offline only: `DROP NOT FOR REPLICATION WITH (ONLINE = ON)` is not read, `ONLINE = OFF` is.
					if (property.Kind is ColumnPropertyKind.RowGuidCol or ColumnPropertyKind.NotForReplication or ColumnPropertyKind.Persisted or ColumnPropertyKind.Hidden
						&& property.Options.Any(static one => Plain(one.Name, "ONLINE") && one.Value is Expression.Reference { Name: [var on] } && Plain(on, "ON")))
						throw Misplaced(property, "Options", $"with ONLINE = ON on {property.Kind}, which changes offline only");

					Word(property.Add ? "ADD" : "DROP");
					Word(property.Kind switch
					{
						ColumnPropertyKind.RowGuidCol        => "ROWGUIDCOL",
						ColumnPropertyKind.Persisted         => "PERSISTED",
						ColumnPropertyKind.NotForReplication => "NOT FOR REPLICATION",
						ColumnPropertyKind.Sparse            => "SPARSE",
						ColumnPropertyKind.Hidden            => "HIDDEN",
						ColumnPropertyKind.Masked            => "MASKED",
						_                                    => throw NoText(property.Kind),
					});

					if (property.MaskFunction is { } function)
						PutMask(property, function);

					PutOptions(property.Options);
					return true;

				// T-SQL: ALTER TABLE (Transact-SQL). A memory-optimized table's index, rebuilt with options and nothing else.
				case AlterTableAction.AlterIndex alter:
					if (alter.Action is not IndexAlteration.Rebuild { Partition: null, AllPartitions: false, Options.Count: > 0 })
						throw Misplaced(alter, "Action", "other than a REBUILD with options and no partition, which is all ALTER TABLE reads");

					Word("ALTER INDEX");
					PutIdentifier(alter.Name);
					PutIndexAlteration(alter.Action);
					return true;

				case AlterTableAction.DropIndex drop:
					Word("DROP INDEX");
					PutIdentifier(drop.Name);
					return true;

				case Statement.CreateIndex create:
					PutCreateIndex(create);
					return true;

				// T-SQL: CREATE XML INDEX (Transact-SQL). A secondary index says what it is built over and which it is;
				// a primary one says neither.
				case Statement.CreateXmlIndex xml:
					if (xml.Primary && xml.Using is not null)
						throw Both(xml, "Primary", "Using");

					if ((xml.Using is null) != (xml.For is null))
						throw Misplaced(xml, "Using or For", "without the other, which a secondary index says together");

					Word(xml.Primary ? "CREATE PRIMARY XML INDEX" : "CREATE XML INDEX");
					PutIdentifier(xml.Name);
					Word("ON");
					PutName(xml.Table);
					Open();
					PutIdentifier(xml.Column);
					Close();

					if (xml.Using is { } over)
					{
						Word("USING XML INDEX");
						PutIdentifier(over);
						Word(xml.For switch
						{
							XmlIndexFor.Value    => "FOR VALUE",
							XmlIndexFor.Path     => "FOR PATH",
							XmlIndexFor.Property => "FOR PROPERTY",
							_                    => throw NoText(xml.For!.Value),
						});
					}

					PutOptions(xml.Options);
					return true;

				// T-SQL: CREATE SPATIAL INDEX (Transact-SQL). One column, the grid, the options, and a filegroup alone.
				case Statement.CreateSpatialIndex spatial:
					if (spatial.Placements is not ([] or [{ Kind: PlacementKind.On, Columns: null }]))
						throw Misplaced(spatial, "Placements", "other than one ON a filegroup");

					Word("CREATE SPATIAL INDEX");
					PutIdentifier(spatial.Name);
					Word("ON");
					PutName(spatial.Table);
					Open();
					PutIdentifier(spatial.Column);
					Close();

					if (spatial.Using is { } grid)
						Word(grid switch
						{
							SpatialTessellation.GeometryGrid      => "USING GEOMETRY_GRID",
							SpatialTessellation.GeographyGrid     => "USING GEOGRAPHY_GRID",
							SpatialTessellation.GeometryAutoGrid  => "USING GEOMETRY_AUTO_GRID",
							SpatialTessellation.GeographyAutoGrid => "USING GEOGRAPHY_AUTO_GRID",
							_                                     => throw NoText(grid),
						});

					CheckSpatialOptions(spatial);
					PutOptions(spatial.Options);

					foreach (var placement in spatial.Placements)
						PutPlacement(placement);

					return true;

				// T-SQL: ALTER INDEX (Transact-SQL). One index or ALL, never both or neither.
				case Statement.AlterIndex alter:
					if (alter.All == (alter.Name is not null))
						throw alter.All ? Both(alter, "All", "Name") : Empty(alter, "All", "Name");

					if (alter.Name is { } name && Plain(name, "ALL"))
						throw Misplaced(alter, "Name", "that is a plain ALL, which reads back as All");

					Word("ALTER INDEX");

					if (alter.All)
						Word("ALL");
					else
						PutIdentifier(alter.Name!);

					Word("ON");
					PutName(alter.Table);
					PutIndexAlteration(alter.Action);
					return true;

				case Statement.CreateFullTextIndex fullText:
					PutCreateFullTextIndex(fullText);
					return true;

				case Statement.AlterFullTextIndex alter:
					Word("ALTER FULLTEXT INDEX ON");
					PutName(alter.Table);
					PutFullTextAlteration(alter.Action);
					return true;

				// T-SQL: CREATE STATISTICS (Transact-SQL). The columns, the filter, and the options without brackets.
				case Statement.CreateStatistics statistics:
					if (statistics.Columns.Count == 0)
						throw Empty(statistics, "Columns");

					Word("CREATE STATISTICS");
					PutIdentifier(statistics.Name);
					Word("ON");
					PutName(statistics.Table);
					Open();
					Each(statistics.Columns, PutIdentifier);
					Close();

					if (statistics.Where is { } where)
					{
						Word("WHERE");
						PutExpression(where);
					}

					PutStatisticsOptions(statistics.Options, update: false);
					return true;

				// T-SQL: UPDATE STATISTICS (Transact-SQL). Names in brackets, or one without them.
				case Statement.UpdateStatistics update:
					if (update.NameUnbracketed && update.Names.Count != 1)
						throw Misplaced(update, "NameUnbracketed", "with other than one name");

					Word("UPDATE STATISTICS");
					PutName(update.Table);

					if (update.NameUnbracketed)
						PutIdentifier(update.Names[0]);
					else if (update.Names.Count > 0)
					{
						Open();
						Each(update.Names, PutIdentifier);
						Close();
					}

					PutStatisticsOptions(update.Options, update: true);
					return true;

				case Statement.CreateTableType type:
					if (type.Elements.Count == 0)
						throw Empty(type, "Elements");

					CheckTypeName(type, type.Name);
					CheckTableTyped(type);

					// The one option a table type reads: MEMORY_OPTIMIZED = ON or OFF, once.
					if (type.Options.Count > 1 || type.Options.Any(static one => !(Plain(one.Name, "MEMORY_OPTIMIZED") && IsWord(one.Value, "ON", "OFF")
						&& one.Options.Count == 0 && one.Order is null && one.Partitions is null)))
						throw Misplaced(type, "Options", "other than one MEMORY_OPTIMIZED = ON or OFF, the only option a table type reads");

					Word("CREATE TYPE");
					PutName(type.Name);
					Word("AS TABLE");
					PutElements(type.Elements);
					PutOptions(type.Options);
					return true;

				case Statement.CreateAliasType alias:
					CheckTypeName(alias, alias.Name);
					Word("CREATE TYPE");
					PutName(alias.Name);
					Word("FROM");
					PutType(alias.Base);

					if (alias.Nullability is { } nullability)
						Word(nullability == ColumnNullability.Null ? "NULL" : nullability == ColumnNullability.NotNull ? "NOT NULL" : throw NoText(nullability));

					return true;

				case Statement.CreateSynonym synonym:
					Word("CREATE SYNONYM");
					PutName(synonym.Name);
					Word("FOR");
					PutName(synonym.Target);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL), CREATE TABLE AS SELECT (Microsoft Fabric and Azure Synapse Analytics).
		/// A table with T-SQL's parts: its kind, its placements, its options, or the query written without the
		/// standard's <c>WITH [NO] DATA</c>. False for a table the standard's arm writes.
		/// </summary>
		bool PutTransactSqlTable(Statement.CreateTable table)
		{
			if (table is { Kind: null, Placements.Count: 0, Options.Count: 0 } && table.Contents is not TableContents.AsQuery { Data: null })
				return false;

			if (table.Scope is not null || table.SystemVersioning || table.OnCommit is not null)
				throw Misplaced(table, "Scope, SystemVersioning or OnCommit", "beside T-SQL's Kind, Placements, Options or a query without WITH DATA");

			Word("CREATE TABLE");
			PutName(table.Name);

			switch (table.Contents)
			{
				// The query's own form: options before the AS, and the query bare after it. Without options the
				// columns are what tell it from a table declared.
				case TableContents.AsQuery { Data: null } query:
					if (table.Kind is not null || table.Placements.Count > 0)
						throw Misplaced(table, "Kind or Placements", "on a table made of a query");

					if (query.Columns.Count == 0 && table.Options.Count == 0)
						throw Empty(query, "Columns", "Options");

					if (query.Columns.Count > 0)
					{
						Open();
						Each(query.Columns, PutIdentifier);
						Close();
					}

					PutOptions(table.Options);
					Word("AS");
					PutQuery(query.Query);
					return true;

				// A file table says what it is before its columns, a graph table after them; either may have none.
				case TableContents.Elements elements:
					if (table.Kind == TableKind.FileTable)
						Word("AS FILETABLE");

					if (elements.Items.Count > 0)
						PutElements(elements.Items);
					else if (table.Kind is null)
						throw Empty(elements, "Items");

					if (table.Kind is TableKind.Node or TableKind.Edge)
						Word(table.Kind == TableKind.Node ? "AS NODE" : "AS EDGE");
					else if (table.Kind is { } kind && kind != TableKind.FileTable)
						throw NoText(kind);

					foreach (var placement in table.Placements)
						PutPlacement(placement);

					PutOptions(table.Options);
					return true;

				default:
					throw Misplaced(table, "Contents", "other than columns, or a query without WITH DATA, beside T-SQL's parts");
			}
		}

		/// <summary>
		/// T-SQL: column_definition (Transact-SQL), computed_column_definition (Transact-SQL). A column with T-SQL's
		/// options, in the order written, and none of the standard's members. False for a column with none.
		/// </summary>
		bool PutTransactSqlColumn(ColumnDefinition column)
		{
			if (column.Options.Count == 0)
				return false;

			if (column.Generation is not null || column.Constraints.Count > 0 || column.Collation is not null)
				throw Misplaced(column, "Options", "beside the standard's Generation, Constraints or Collation");

			// A collation is an option of a T-SQL column, and one in its type as well would be a second.
			if (column.Type is DataType.Character { Collation: not null })
				throw Misplaced(column, "Options", "beside a type with a collation of its own");

			Tailed(column);
			PutIdentifier(column.Name);

			for (var at = 0; at < column.Options.Count; at++)
			{
				var option = column.Options[at];

				// The value a computed column is, in the type's place: first, and only without a type.
				if (option is ColumnOption.Computed computed)
				{
					if (at > 0 || column.Type is not null)
						throw Misplaced(computed, "Computed", "after another option or beside a type");

					if (computed.Name is not null)
						throw Misplaced(computed, "Name", "on an option other than DEFAULT or NULL");

					Word("AS");
					PutExpression(computed.Value);
					continue;
				}

				if (at == 0 && column.Type is { } type)
					PutType(type);

				PutColumnOption(option);
			}

			return true;
		}

		/// <summary>
		/// T-SQL: column_definition (Transact-SQL). A column's options in the order the engine reads them, which is
		/// the only order the text reads back in. A typed column's begin with its storage: a collation, then a
		/// generation and its <c>HIDDEN</c>, or <c>FILESTREAM</c> and <c>SPARSE</c>, each once and in either order,
		/// then a mask or an encryption, not after <c>FILESTREAM</c>. A computed column's begin with
		/// <c>PERSISTED</c>. A free list follows, in any order: a default, an identity, a nullability,
		/// <c>ROWGUIDCOL</c>, the constraints and an index, with <c>NOT FOR REPLICATION</c> directly after an
		/// identity. A column with neither type nor value has the free list alone, and <c>FILESTREAM</c>,
		/// <c>SPARSE</c> and <c>PERSISTED</c> in it.
		/// </summary>
		static void Tailed(ColumnDefinition column)
		{
			var options = column.Options;
			var at      = 0;

			if (options[0] is ColumnOption.Computed)
			{
				at++;

				if (at < options.Count && options[at] is ColumnOption.Persisted)
					at++;
			}
			else if (column.Type is not null)
			{
				if (options[at] is ColumnOption.Collation)
					at++;

				if (at < options.Count && options[at] is ColumnOption.GeneratedAlways)
				{
					at++;

					if (at < options.Count && options[at] is ColumnOption.Hidden)
						at++;
				}
				else
				{
					var filestream = false;
					var sparse     = false;

					for (; at < options.Count; at++)
					{
						if (!filestream && options[at] is ColumnOption.Filestream)
							filestream = true;
						else if (!sparse && options[at] is ColumnOption.Sparse)
							sparse = true;
						else
							break;
					}

					if (at < options.Count && (options[at] is ColumnOption.Masked || !filestream && options[at] is ColumnOption.Encrypted))
						at++;
				}
			}

			var untyped = column.Type is null && options[0] is not ColumnOption.Computed;

			for (; at < options.Count; at++)
			{
				var option = options[at];

				if (option is ColumnOption.Collation or ColumnOption.GeneratedAlways or ColumnOption.Hidden or ColumnOption.Masked or ColumnOption.Encrypted or ColumnOption.Computed
					|| !untyped && option is ColumnOption.Filestream or ColumnOption.Sparse or ColumnOption.Persisted)
					throw Misplaced(option, option.GetType().Name, "where the column's options read back in another order");

				if (option is ColumnOption.NotForReplication && (untyped || at == 0 || options[at - 1] is not ColumnOption.Identity))
					throw Misplaced(option, "NotForReplication", "not directly after an Identity, or on a column with no type");
			}
		}

		void PutColumnOption(ColumnOption option)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, option, static (into, node) => into.PutColumnOption(node));

				return;
			}

			// T-SQL: column_definition (Transact-SQL). A name for a default or a nullability; a constraint's is its own.
			if (option.Name is { } name)
			{
				if (option is not (ColumnOption.Default or ColumnOption.Null))
					throw Misplaced(option, "Name", "on an option other than DEFAULT or NULL");

				Word("CONSTRAINT");
				PutIdentifier(name);
			}

			switch (option)
			{
				case ColumnOption.Default @default:
					PutDefault(@default.Value);
					break;

				case ColumnOption.Identity identity:
					if (identity.Seed is null && identity.Increment is not null)
						throw Misplaced(identity, "Increment", "without Seed");

					Word("IDENTITY");

					if (identity.Seed is { } seed)
					{
						Open();
						PutExpression(seed);

						if (identity.Increment is { } increment)
						{
							Tight(",");
							PutExpression(increment);
						}

						Close();
					}

					break;

				case ColumnOption.NotForReplication:
					Word("NOT FOR REPLICATION");
					break;

				case ColumnOption.Collation collation:
					PutCollate(collation.Value);
					break;

				case ColumnOption.Constraint { Value: Constraint.Default or Constraint.Connection } table:
					throw Misplaced(table, "Value", "a constraint only a table writes");

				case ColumnOption.Constraint constraint:
					PutConstraint(constraint.Value);
					break;

				case ColumnOption.Null:
					Word("NULL");
					break;

				case ColumnOption.Persisted:
					Word("PERSISTED");
					break;

				case ColumnOption.Sparse:
					Word("SPARSE");
					break;

				case ColumnOption.Filestream:
					Word("FILESTREAM");
					break;

				case ColumnOption.RowGuidCol:
					Word("ROWGUIDCOL");
					break;

				case ColumnOption.Hidden:
					Word("HIDDEN");
					break;

				case ColumnOption.Masked masked:
					Word("MASKED");
					PutMask(masked, masked.Function);
					break;

				case ColumnOption.Encrypted encrypted:
					if (encrypted.Options.Count == 0)
						throw Empty(encrypted, "Options");

					Word("ENCRYPTED");
					PutOptions(encrypted.Options);
					break;

				case ColumnOption.GeneratedAlways generated:
					Word(generated.Kind switch
					{
						GeneratedAlwaysKind.RowStart            => "GENERATED ALWAYS AS ROW START",
						GeneratedAlwaysKind.RowEnd              => "GENERATED ALWAYS AS ROW END",
						GeneratedAlwaysKind.TransactionIdStart  => "GENERATED ALWAYS AS TRANSACTION_ID START",
						GeneratedAlwaysKind.TransactionIdEnd    => "GENERATED ALWAYS AS TRANSACTION_ID END",
						GeneratedAlwaysKind.SequenceNumberStart => "GENERATED ALWAYS AS SEQUENCE_NUMBER START",
						GeneratedAlwaysKind.SequenceNumberEnd   => "GENERATED ALWAYS AS SEQUENCE_NUMBER END",
						_                                       => throw NoText(generated.Kind),
					});
					break;

				case ColumnOption.Index index:
					PutIndexDefinition(index.Definition);
					break;

				case ColumnOption.Computed computed:
					throw Misplaced(computed, "Computed", "after another option or beside a type");

				default:
					throw Unwritten(option);
			}
		}

		/// <summary>T-SQL: column_definition (Transact-SQL). <c>WITH (FUNCTION = 'f()')</c>: a mask is a function named in a string.</summary>
		void PutMask(ISqlNode owner, Expression function)
		{
			if (function is not Expression.Literal { Value: LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } })
				throw Misplaced(owner, "Function", "that is not a string, which is all a mask names its function in");

			Word("WITH");
			Open();
			Word("FUNCTION =");
			PutExpression(function);
			Close();
		}

		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL). <c>INDEX i [UNIQUE] [CLUSTERED | NONCLUSTERED] [COLUMNSTORE | HASH]
		/// (…) [ORDER (…)] [INCLUDE (…)] [WHERE …] [WITH (…)] [ON …] [FILESTREAM_ON …]</c>, at the table or on a column.
		/// </summary>
		void PutIndexDefinition(IndexDefinition index)
		{
			Word("INDEX");
			PutIdentifier(index.Name);

			if (index.Unique)
				Word("UNIQUE");

			PutClustering(index.Clustering);

			if (index.Storage is { } storage)
				Word(storage == IndexStorage.Columnstore ? "COLUMNSTORE" : storage == IndexStorage.Hash ? "HASH" : throw NoText(storage));

			PutIndexBody(index);
		}

		void PutClustering(IndexClustering? clustering)
		{
			if (clustering is { } one)
				Word(one == IndexClustering.Clustered ? "CLUSTERED" : one == IndexClustering.Nonclustered ? "NONCLUSTERED" : throw NoText(one));
		}

		/// <summary>
		/// T-SQL: table_constraint (Transact-SQL). What a PRIMARY KEY or a UNIQUE adds after its word: the clustering,
		/// the keys with their directions, which T-SQL holds in place of the standard's columns, the options, bracketed
		/// or the old bare pair, and the placement. False where it has none of them.
		/// </summary>
		bool PutTransactSqlKey(Constraint.Unique unique)
		{
			if (unique is { Clustering: null, Keys: null, Options.Count: 0, OptionsUnbracketed: false, On: null })
				return false;

			if (unique.Columns.Count > 0 || unique.Nulls is not null || unique.WithoutOverlapsPeriod is not null || unique.Kind == UniqueKind.UniqueValue)
				throw Misplaced(unique, "Clustering, Keys, Options or On", "beside the standard's Columns, Nulls, WithoutOverlapsPeriod or UNIQUE (VALUE)");

			if (unique.OptionsUnbracketed && unique.Options.Count == 0)
				throw Misplaced(unique, "OptionsUnbracketed", "without Options");

			// Without brackets a key reads two options at most, each once: FILLFACTOR with its value, and
			// SORTED_DATA or SORTED_DATA_REORG alone.
			if (unique.OptionsUnbracketed && (unique.Options.Count > 2 || unique.Options.Any(static one => !Unbracketed(one))
				|| unique.Options.Count == 2 && unique.Options[0].Name.Text.Equals(unique.Options[1].Name.Text, StringComparison.OrdinalIgnoreCase)))
				throw Misplaced(unique, "OptionsUnbracketed", "with options other than one FILLFACTOR and one SORTED_DATA");

			Word(unique.Kind == UniqueKind.Unique ? "UNIQUE" : "PRIMARY KEY");
			PutClustering(unique.Clustering);

			if (unique.Keys is { } keys)
			{
				if (keys.Count == 0)
					throw Empty(unique, "Keys");

				Open();
				PutSorts(new OrderByClause(keys));
				Close();
			}

			// The old spelling is the brackets left out and the options side by side, `WITH SORTED_DATA FILLFACTOR = 12`.
			if (unique.OptionsUnbracketed)
			{
				Word("WITH");

				foreach (var option in unique.Options)
					PutOption(option);
			}
			else
				PutOptions(unique.Options);

			if (unique.On is { } on)
				PutPlacement(on, PlacementKind.On);

			return true;
		}

		/// <summary>T-SQL: table_constraint (Transact-SQL). An option a key may write without brackets.</summary>
		static bool Unbracketed(Option option)
		{
			if (option is not { Order: null, Options.Count: 0, Name.Style: IdentifierStyle.Regular })
				return false;

			var name = option.Name.Text;

			return name.Equals("FILLFACTOR", StringComparison.OrdinalIgnoreCase)
				? option.Value is not null
				: option.Value is null && (name.Equals("SORTED_DATA", StringComparison.OrdinalIgnoreCase) || name.Equals("SORTED_DATA_REORG", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// T-SQL: ALTER TABLE (Transact-SQL). A DROP of several items, grouped by their words: an item without one is
		/// of the kind before it, and one first in the list is a constraint.
		/// </summary>
		void PutDropList(AlterTableAction.Drop drop)
		{
			if (drop.Items.Count < 2)
				throw Misplaced(drop, "Items", "fewer than two, which the standard's DropColumn or DropConstraint holds");

			Word("DROP");

			AlterTableAction? before = null;

			Each(drop.Items, item =>
			{
				// `DROP COLUMN a CASCADE, b` is not read, and a behavior has no place in a list.
				if (item is AlterTableAction.DropColumn { Behavior: not null } or AlterTableAction.DropConstraint { Behavior: not null })
					throw Misplaced(item, "Behavior", "on an item of a DROP list");

				switch (item)
				{
					case AlterTableAction.DropColumn column:
						if (!column.ColumnKeyword && before is not AlterTableAction.DropColumn)
							throw Misplaced(column, "ColumnKeyword", "false where the item before is not a column");

						PutDroppedColumn(column);
						break;

					case AlterTableAction.DropConstraint constraint:
						if (constraint.ConstraintKeywordOmitted && before is AlterTableAction.DropColumn)
							throw Misplaced(constraint, "ConstraintKeywordOmitted", "after a column, where the name would be one");

						PutDroppedConstraint(constraint);
						break;

					default:
						throw Misplaced(item, "Items", "holding other than a dropped column or constraint");
				}

				before = item;
			});
		}

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). A column dropped: the word, IF EXISTS, the name; no behavior in a list.</summary>
		void PutDroppedColumn(AlterTableAction.DropColumn column)
		{
			if (column.Options.Count > 0)
				throw Misplaced(column, "Options", "on a column dropped, which T-SQL gives none");

			if (column.ColumnKeyword)
				Word("COLUMN");
			else if (column.IfExists)
				throw Misplaced(column, "IfExists", "without the word COLUMN");

			PutIfExists(column.IfExists);
			PutIdentifier(column.Column);
			PutBehavior(column.Behavior);
		}

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). A constraint dropped: the word, IF EXISTS, the name, the options.</summary>
		void PutDroppedConstraint(AlterTableAction.DropConstraint constraint)
		{
			// `DROP c CASCADE` is the standard's column dropped.
			if (constraint.ConstraintKeywordOmitted && constraint.Behavior is not null)
				throw Misplaced(constraint, "Behavior", "with ConstraintKeywordOmitted, which reads back as a column dropped");

			// `DROP s.c` does not read: a constraint without its word is named by one part.
			if (constraint.ConstraintKeywordOmitted && constraint.Name.Count > 1)
				throw Misplaced(constraint, "Name", "of more than one part with ConstraintKeywordOmitted");

			if (!constraint.ConstraintKeywordOmitted)
				Word("CONSTRAINT");
			else if (constraint.IfExists)
				throw Misplaced(constraint, "IfExists", "without the word CONSTRAINT");

			PutIfExists(constraint.IfExists);
			PutName(constraint.Name);
			PutOptions(constraint.Options);
			PutBehavior(constraint.Behavior);
		}

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>PARTITION n</c> on either side of a SWITCH.</summary>
		void PutPartition(Expression? partition)
		{
			if (partition is null)
				return;

			Word("PARTITION");
			PutExpression(partition);
		}

		/// <summary>T-SQL: ALTER TABLE (Transact-SQL). <c>ALL</c>, or the names, and never both or neither.</summary>
		void PutNamesOrAll(ISqlNode owner, bool all, SqlList<Identifier> names)
		{
			if (all == (names.Count > 0))
				throw all ? Both(owner, "All", "Names") : Empty(owner, "All", "Names");

			if (names.Any(static one => one is { Style: IdentifierStyle.Regular } && one.Text.Equals("ALL", StringComparison.OrdinalIgnoreCase)))
				throw Misplaced(owner, "Names", "with a plain ALL, which reads back as All");

			if (all)
				Word("ALL");
			else
				Each(names, PutIdentifier);
		}

		/// <summary>
		/// T-SQL: ALTER TABLE (Transact-SQL). The type an altered column is given, written alone, with its collation,
		/// its nullability last and its options. False for the standard's <c>SET DATA TYPE</c>.
		/// </summary>
		bool PutTransactSqlType(AlterColumnAction.SetDataType type)
		{
			if (!type.SetDataTypeOmitted)
			{
				if (type.Collation is not null || type.Nullability is not null || type.Options.Count > 0)
					throw Misplaced(type, "Collation, Nullability or Options", "beside the standard's SET DATA TYPE");

				return false;
			}

			PutType(type.Type);
			PutCollate(type.Collation);

			if (type.Nullability is { } nullability)
				Word(nullability == ColumnNullability.Null ? "NULL" : nullability == ColumnNullability.NotNull ? "NOT NULL" : throw NoText(nullability));

			PutOptions(type.Options);
			return true;
		}

		/// <summary>The <c>WITH (…)</c> of a T-SQL statement or clause, where it has options.</summary>
		void PutOptions(SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(options, PutOption);
			Close();
		}

		/// <summary>
		/// T-SQL: the <c>WITH (…)</c> of the CREATE and ALTER statements (proposal 20). One option: a name, <c>= value</c>
		/// where it has one, and the options of its own in brackets after that, as in <c>SYSTEM_VERSIONING = ON
		/// (HISTORY_TABLE = h)</c> and <c>WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTES)</c>.
		/// </summary>
		void PutOption(Option option)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, option, static (into, node) => into.PutOption(node));

				return;
			}

			if (option.Order is not null)
				throw Misplaced(option, "Order", "outside a bulk insert's options");

			PutIdentifier(option.Name);

			if (option.Value is { } value)
			{
				Word("=");
				PutExpression(value);
			}

			// `DATA_COMPRESSION = PAGE ON PARTITIONS (1)`: the partitions after the value, and no options of its own beside them.
			// Only the two compressions read ON PARTITIONS in a WITH list (RESAMPLE does in UPDATE STATISTICS, which writes it itself).
			if (option.Partitions is not null && !(option.Value is not null && Plain(option.Name, "DATA_COMPRESSION", "XML_COMPRESSION")))
				throw Misplaced(option, "Partitions", "on an option other than DATA_COMPRESSION or XML_COMPRESSION with a value, the only ones that read ON PARTITIONS");

			if (option.Partitions is not null && option.Options.Count > 0)
				throw Both(option, "Partitions", "Options");

			PutRanges(option.Partitions);

			if (option.Options.Count > 0)
			{
				Open();
				Each(option.Options, PutOption);
				Close();
			}
		}

		/// <summary>
		/// T-SQL: CREATE TABLE (Transact-SQL). <c>ON</c>, <c>TEXTIMAGE_ON</c> or <c>FILESTREAM_ON</c>, then the
		/// filegroup, a scheme and its column, a quoted name or a string.
		/// </summary>
		void PutPlacement(Placement placement)
		{
			PutPlacement(placement, placement.Kind);
		}

		void PutPlacement(Placement placement, PlacementKind word)
		{
			if (placement.Kind != word)
				throw Misplaced(placement, "Kind", $"other than {word} where only {word} is written");

			Word(placement.Kind switch
			{
				PlacementKind.On           => "ON",
				PlacementKind.TextImageOn  => "TEXTIMAGE_ON",
				PlacementKind.FilestreamOn => "FILESTREAM_ON",
				_                          => throw NoText(placement.Kind),
			});

			if (placement.Target.Style == IdentifierStyle.String)
				Word("'" + placement.Target.Text.Replace("'", "''") + "'");
			else
				PutIdentifier(placement.Target);

			if (placement.Columns is { } columns)
			{
				if (columns.Count == 0)
					throw Empty(placement, "Columns");

				if (placement.Kind == PlacementKind.TextImageOn)
					throw Misplaced(placement, "Columns", "on TEXTIMAGE_ON, which takes a filegroup alone");

				if (placement.Target is { Style: IdentifierStyle.Regular } target && target.Text.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase))
					throw Misplaced(placement, "Columns", "on the DEFAULT filegroup, which is not a partition scheme");

				Open();
				Each(columns, PutIdentifier);
				Close();
			}
		}

		/// <summary>
		/// T-SQL: CREATE INDEX (Transact-SQL), CREATE COLUMNSTORE INDEX (Transact-SQL). A relational index, its keys
		/// required and no order; or a columnstore one, its columns and order free and no UNIQUE, INCLUDE or
		/// FILESTREAM_ON. A hash index is declared only at the table.
		/// </summary>
		void PutCreateIndex(Statement.CreateIndex create)
		{
			var index = create.Index;

			switch (index.Storage)
			{
				case null:
					if (index.Columns.Count == 0)
						throw Empty(index, "Columns");

					if (index.Order.Count > 0)
						throw Misplaced(index, "Order", "on a relational index, which CREATE INDEX writes none for");

					Word("CREATE");

					if (index.Unique)
						Word("UNIQUE");

					PutClustering(index.Clustering);
					Word("INDEX");
					break;

				case IndexStorage.Columnstore:
					if (index.Unique || index.Include.Count > 0)
						throw Misplaced(index, "Unique or Include", "on a columnstore index");

					if (index.Placements.Any(static one => one.Kind == PlacementKind.FilestreamOn))
						throw Misplaced(index, "Placements", "with FILESTREAM_ON on a columnstore index");

					Word("CREATE");
					PutClustering(index.Clustering);
					Word("COLUMNSTORE INDEX");
					break;

				default:
					throw Misplaced(index, "Storage", "HASH, which CREATE INDEX has no word for");
			}

			PutIdentifier(index.Name);
			Word("ON");
			PutName(create.Table);
			PutIndexBody(index);
		}

		/// <summary>
		/// T-SQL: CREATE INDEX (Transact-SQL). What follows an index's header, at the table or in its own statement:
		/// <c>(…) [ORDER (…)] [INCLUDE (…)] [WHERE …] [WITH (…)] [ON …] [FILESTREAM_ON …]</c>.
		/// </summary>
		void PutIndexBody(IndexDefinition index)
		{
			// A key is a column of one part, with a direction at most: IndexColumn reads no more.
			if (index.Columns.Any(static key => key.NullOrdering is not null || !IsName(key.Key)))
				throw Misplaced(index, "Columns", "that are not columns of one part, or have a null ordering, which an index key does not read");

			if (index.Columns.Count > 0)
			{
				Open();
				PutSorts(new OrderByClause(index.Columns));
				Close();
			}

			if (index.Order.Count > 0)
			{
				Word("ORDER");
				Open();
				Each(index.Order, PutIdentifier);
				Close();
			}

			if (index.Include.Count > 0)
			{
				Word("INCLUDE");
				Open();
				Each(index.Include, PutIdentifier);
				Close();
			}

			if (index.Where is { } where)
			{
				Word("WHERE");
				PutExpression(where);
			}

			PutOptions(index.Options);

			// ON before FILESTREAM_ON, each once, as the grammar reads them.
			if (index.Placements is not ([] or [{ Kind: PlacementKind.On or PlacementKind.FilestreamOn }] or [{ Kind: PlacementKind.On }, { Kind: PlacementKind.FilestreamOn }]))
				throw Misplaced(index, "Placements", "other than ON and FILESTREAM_ON, each once and in that order");

			foreach (var placement in index.Placements)
				PutPlacement(placement);
		}

		/// <summary>
		/// T-SQL: ALTER INDEX (Transact-SQL). The action: a rebuild of one partition, of all said so, or of neither; a
		/// reorganization; the options set; a resumable rebuild disabled, resumed, paused or abandoned.
		/// </summary>
		void PutIndexAlteration(IndexAlteration action)
		{
			switch (action)
			{
				case IndexAlteration.Rebuild rebuild:
					if (rebuild.AllPartitions && rebuild.Partition is not null)
						throw Both(rebuild, "AllPartitions", "Partition");

					if (rebuild.Partition is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular } word] } && word.Text.Equals("ALL", StringComparison.OrdinalIgnoreCase))
						throw Misplaced(rebuild, "Partition", "that is a plain ALL, which reads back as AllPartitions");

					Word("REBUILD");

					if (rebuild.AllPartitions)
						Word("PARTITION = ALL");
					else if (rebuild.Partition is { } partition)
					{
						Word("PARTITION =");
						PutExpression(partition);
					}

					PutOptions(rebuild.Options);
					break;

				case IndexAlteration.Reorganize reorganize:
					Word("REORGANIZE");

					if (reorganize.Partition is { } part)
					{
						Word("PARTITION =");
						PutExpression(part);
					}

					PutOptions(reorganize.Options);
					break;

				case IndexAlteration.Disable:
					Word("DISABLE");
					break;

				case IndexAlteration.SetOptions set:
					if (set.Options.Count == 0)
						throw Empty(set, "Options");

					Word("SET");
					Open();
					Each(set.Options, PutOption);
					Close();
					break;

				case IndexAlteration.Resume resume:
					Word("RESUME");
					PutOptions(resume.Options);
					break;

				case IndexAlteration.Pause:
					Word("PAUSE");
					break;

				case IndexAlteration.Abort:
					Word("ABORT");
					break;

				default:
					throw Unwritten(action);
			}
		}

		/// <summary>
		/// T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). The columns, the key index, the catalog alone or in brackets with
		/// a filegroup, and the options, bracketed or not.
		/// </summary>
		void PutCreateFullTextIndex(Statement.CreateFullTextIndex index)
		{
			if (index.OptionsUnbracketed && index.Options.Count == 0)
				throw Misplaced(index, "OptionsUnbracketed", "without Options");

			CheckFullTextOptions(index);
			Word("CREATE FULLTEXT INDEX ON");
			PutName(index.Table);

			if (index.Columns.Count > 0)
			{
				Open();
				Each(index.Columns, PutFullTextColumn);
				Close();
			}

			Word("KEY INDEX");
			PutIdentifier(index.KeyIndex);

			if (index.FileGroup is { } fileGroup)
			{
				Word("ON");
				Open();

				if (index.Catalog is { } catalog)
				{
					PutIdentifier(catalog);
					Tight(",");
				}

				Word("FILEGROUP");
				PutIdentifier(fileGroup);
				Close();
			}
			else if (index.Catalog is { } catalog)
			{
				Word("ON");
				PutIdentifier(catalog);
			}

			if (index.OptionsUnbracketed)
			{
				Word("WITH");
				Each(index.Options, PutOption);
			}
			else
				PutOptions(index.Options);
		}

		/// <summary>
		/// T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). The grammar's FullTextOptions: CHANGE_TRACKING with MANUAL, AUTO or
		/// OFF, and STOPLIST with OFF, SYSTEM or a name of one part, each at most once and each with its value.
		/// </summary>
		static void CheckFullTextOptions(Statement.CreateFullTextIndex index)
		{
			foreach (var option in index.Options)
			{
				var read = option.Options.Count == 0 && option.Order is null && option.Partitions is null
					&& (Plain(option.Name, "CHANGE_TRACKING") && IsWord(option.Value, "MANUAL", "AUTO", "OFF")
						|| Plain(option.Name, "STOPLIST") && IsName(option.Value));

				if (!read)
					throw Misplaced(index, "Options", "other than CHANGE_TRACKING = MANUAL, AUTO or OFF, and STOPLIST = OFF, SYSTEM or a name of one part, the only ones the statement reads");
			}

			if (index.Options.Select(static one => one.Name.Text.ToUpperInvariant()).Distinct().Count() < index.Options.Count)
				throw Misplaced(index, "Options", "that repeat a name, which the statement reads once each");
		}

		/// <summary>
		/// T-SQL: CREATE SPATIAL INDEX (Transact-SQL). The grammar's SpatialValue: a number, a string, a binary string,
		/// NULL, ON, OFF or a name of one part, set to a name with an equals sign; nothing of an expression.
		/// </summary>
		static void CheckSpatialOptions(Statement.CreateSpatialIndex index)
		{
			foreach (var option in index.Options)
			{
				if (option.Options.Count > 0 || option.Order is not null || option.Partitions is not null || !SpatialAtom(option.Value))
					throw Misplaced(index, "Options", "set to other than a number, a string, a binary string, NULL, ON, OFF or a name of one part, the values a spatial option reads");
			}
		}

		static bool SpatialAtom(Expression? value)
		{
			return value switch
			{
				Expression.Literal { Value: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal or NumericLiteralKind.Approximate or NumericLiteralKind.Money } } => true,
				Expression.Literal { Value: LiteralValue.Null } => true,
				Expression.Literal { Value: LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } } => true,
				Expression.Literal { Value: LiteralValue.Binary { ZeroX: true } } => true,
				_ => IsName(value) && !IsWord(value, "NULL"),
			};
		}

		/// <summary>The value is a name of one part, written as a name: the grammar's word after an equals sign.</summary>
		static bool IsName(Expression? value)
		{
			return value is Expression.Reference { Name: [{ Text.Length: > 0, Style: not (IdentifierStyle.String or IdentifierStyle.NationalString) }] };
		}

		/// <summary>The value is one of the words given, written plain and alone.</summary>
		static bool IsWord(Expression? value, params string[] words)
		{
			return value is Expression.Reference { Name: [var part] } && Plain(part, words);
		}

		/// <summary>T-SQL: CREATE TYPE (Transact-SQL). A type's name has no empty part, as a synonym's may.</summary>
		static void CheckTypeName(ISqlNode owner, QualifiedName name)
		{
			if (name.Count == 0 || name.Parts.Any(static part => part.Text.Length == 0))
				throw Misplaced(owner, "Name", "with an empty part, which a type's name is not read with");
		}

		/// <summary>
		/// T-SQL: CREATE TYPE (Transact-SQL). What a table type's body may hold, after <c>Syntax.TableTyped</c>: no name
		/// for a constraint, no foreign key, no SPARSE, FILESTREAM, ENCRYPTED or NOT FOR REPLICATION, only the options a
		/// key may be told, and a stored column wherever there is a computed one.
		/// </summary>
		static void CheckTableTyped(Statement.CreateTableType type)
		{
			var computed = false;
			var stored   = false;

			foreach (var element in type.Elements)
			{
				switch (element)
				{
					case TableElement.Column column:
						var isComputed = column.Definition.Options.Any(static one => one is ColumnOption.Computed);

						computed |= isComputed;
						stored   |= !isComputed;

						foreach (var option in column.Definition.Options)
							CheckTypedOption(type, option);

						foreach (var constraint in column.Definition.Constraints)
							CheckTypedConstraint(type, constraint);

						break;

					case TableElement.TableConstraint constraint:
						CheckTypedConstraint(type, constraint.Value);
						break;

					case TableElement.Index index:
						CheckTypedKey(type, index.Definition.Options, bracketed: true);
						break;
				}
			}

			if (computed && !stored)
				throw Misplaced(type, "Elements", "that are computed columns alone, where a table type needs a stored column");
		}

		static void CheckTypedOption(Statement.CreateTableType type, ColumnOption option)
		{
			if (option.Name is not null)
				throw Misplaced(type, "Elements", "with a named constraint, which a table type does not read");

			switch (option)
			{
				case ColumnOption.Sparse or ColumnOption.Filestream or ColumnOption.Encrypted or ColumnOption.NotForReplication:
					throw Misplaced(type, "Elements", "with a column that is SPARSE, FILESTREAM, ENCRYPTED or NOT FOR REPLICATION, which a table type does not read");

				case ColumnOption.Constraint constraint:
					CheckTypedConstraint(type, constraint.Value);
					break;

				case ColumnOption.Index index:
					CheckTypedKey(type, index.Definition.Options, bracketed: true);
					break;
			}
		}

		static void CheckTypedConstraint(Statement.CreateTableType type, Constraint constraint)
		{
			if (constraint.Name is not null)
				throw Misplaced(type, "Elements", "with a named constraint, which a table type does not read");

			switch (constraint)
			{
				case Constraint.ForeignKey:
					throw Misplaced(type, "Elements", "with a foreign key, which a table type does not read");

				case Constraint.Unique unique:
					CheckTypedKey(type, unique.Options, !unique.OptionsUnbracketed);
					break;
			}
		}

		/// <summary>A key or an index of a table type is told IGNORE_DUP_KEY (ON or OFF) and BUCKET_COUNT (a number) in brackets, SORTED_DATA and SORTED_DATA_REORG out of them.</summary>
		static void CheckTypedKey(Statement.CreateTableType type, SqlList<Option> options, bool bracketed)
		{
			var words = bracketed ? new[] { "IGNORE_DUP_KEY", "BUCKET_COUNT" } : new[] { "SORTED_DATA", "SORTED_DATA_REORG" };

			// Each with its value: a switch, a bucket count, or the bare word of the old spelling.
			var read = static (Option one) => one.Options.Count == 0 && one.Order is null && one.Partitions is null
				&& (Plain(one.Name, "IGNORE_DUP_KEY") ? IsWord(one.Value, "ON", "OFF")
					: Plain(one.Name, "BUCKET_COUNT") ? IsUnsigned(one.Value, decimals: true)
					: one.Value is null);

			if (options.Any(one => !Plain(one.Name, words) || !read(one)))
				throw Misplaced(type, "Elements", "with a key told options a table type does not read");
		}

		/// <summary>T-SQL: CREATE FULLTEXT INDEX (Transact-SQL). <c>c [TYPE COLUMN t] [LANGUAGE l] [STATISTICAL_SEMANTICS]</c>.</summary>
		void PutFullTextColumn(FullTextIndexColumn column)
		{
			PutIdentifier(column.Column);

			if (column.TypeColumn is { } type)
			{
				Word("TYPE COLUMN");
				PutIdentifier(type);
			}

			if (column.Language is { } language)
			{
				Word("LANGUAGE");
				PutExpression(language);
			}

			if (column.StatisticalSemantics)
				Word("STATISTICAL_SEMANTICS");
		}

		/// <summary>
		/// T-SQL: ALTER FULLTEXT INDEX (Transact-SQL). One action. The only <c>WITH</c> the statement reads is
		/// <c>WITH NO POPULATION</c>, which no <see cref="Option"/> of one name says, so an action with options is refused.
		/// </summary>
		void PutFullTextAlteration(FullTextAlteration action)
		{
			if (action.Options.Count > 0)
				throw Misplaced(action, "Options", "where ALTER FULLTEXT INDEX reads only WITH NO POPULATION");

			switch (action)
			{
				case FullTextAlteration.Enable:
					Word("ENABLE");
					break;

				case FullTextAlteration.Disable:
					Word("DISABLE");
					break;

				// MANUAL, AUTO or OFF, and no other word.
				case FullTextAlteration.SetChangeTracking tracking:
					if (tracking.Mode is not { Style: IdentifierStyle.Regular, Text: var mode }
						|| !(mode.Equals("MANUAL", StringComparison.OrdinalIgnoreCase) || mode.Equals("AUTO", StringComparison.OrdinalIgnoreCase) || mode.Equals("OFF", StringComparison.OrdinalIgnoreCase)))
						throw Misplaced(tracking, "Mode", "other than MANUAL, AUTO or OFF");

					Word("SET CHANGE_TRACKING");
					PutEquals(tracking.EqualsOmitted);
					PutIdentifier(tracking.Mode);
					break;

				case FullTextAlteration.AddColumns add:
					if (add.Columns.Count == 0)
						throw Empty(add, "Columns");

					Word("ADD");
					Open();
					Each(add.Columns, PutFullTextColumn);
					Close();
					break;

				case FullTextAlteration.AlterColumn alter:
					if (alter.Column is not { TypeColumn: null, Language: null, StatisticalSemantics: false })
						throw Misplaced(alter, "Column", "with more than its name");

					Word("ALTER COLUMN");
					PutIdentifier(alter.Column.Column);
					Word(alter.Add ? "ADD STATISTICAL_SEMANTICS" : "DROP STATISTICAL_SEMANTICS");
					break;

				case FullTextAlteration.DropColumns drop:
					if (drop.Columns.Count == 0)
						throw Empty(drop, "Columns");

					Word("DROP");
					Open();
					Each(drop.Columns, PutIdentifier);
					Close();
					break;

				// A kind is said only when a population is started, and then always.
				case FullTextAlteration.Population population:
					if ((population.Action == PopulationAction.Start) != (population.Kind is not null))
						throw Misplaced(population, "Kind", population.Kind is null ? "missing from START" : "on other than START");

					Word(population.Action switch
					{
						PopulationAction.Start  => "START",
						PopulationAction.Stop   => "STOP",
						PopulationAction.Pause  => "PAUSE",
						PopulationAction.Resume => "RESUME",
						_                       => throw NoText(population.Action),
					});

					if (population.Kind is { } kind)
						Word(kind switch
						{
							PopulationKind.Full        => "FULL",
							PopulationKind.Incremental => "INCREMENTAL",
							PopulationKind.Update      => "UPDATE",
							_                          => throw NoText(kind),
						});

					Word("POPULATION");
					break;

				case FullTextAlteration.SetStopList stopList:
					if ((stopList.Name is not null ? 1 : 0) + (stopList.Off ? 1 : 0) + (stopList.System ? 1 : 0) != 1)
						throw Misplaced(stopList, "Name, Off and System", "other than exactly one of them");

					if (stopList.Name is { } list && Plain(list, "OFF", "SYSTEM"))
						throw Misplaced(stopList, "Name", "that is a plain OFF or SYSTEM, which reads back as the word");

					Word("SET STOPLIST");
					PutEquals(stopList.EqualsOmitted);

					if (stopList.Name is { } named)
						PutIdentifier(named);
					else
						Word(stopList.Off ? "OFF" : "SYSTEM");

					break;

				case FullTextAlteration.SetSearchPropertyList propertyList:
					if ((propertyList.Name is not null) == propertyList.Off)
						throw propertyList.Off ? Both(propertyList, "Name", "Off") : Empty(propertyList, "Name", "Off");

					if (propertyList.Name is { } property && Plain(property, "OFF"))
						throw Misplaced(propertyList, "Name", "that is a plain OFF, which reads back as Off");

					Word("SET SEARCH PROPERTY LIST");
					PutEquals(propertyList.EqualsOmitted);

					if (propertyList.Name is { } properties)
						PutIdentifier(properties);
					else
						Word("OFF");

					break;

				default:
					throw Unwritten(action);
			}
		}

		void PutEquals(bool omitted)
		{
			if (!omitted)
				Word("=");
		}

		/// <summary>Whether a name is one of the words given, written plain, where the grammar would read the word.</summary>
		static bool Plain(Identifier name, params string[] words)
		{
			return name.Style == IdentifierStyle.Regular && words.Any(one => name.Text.Equals(one, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// T-SQL: CREATE STATISTICS (Transact-SQL), UPDATE STATISTICS (Transact-SQL). <c>WITH FULLSCAN, SAMPLE 50 PERCENT,
		/// …</c>: no brackets, <c>SAMPLE</c> followed by its measured value with no <c>=</c>, and the options the
		/// grammar lists for the statement: <c>RESAMPLE</c>, <c>ALL</c>, <c>COLUMNS</c>, <c>INDEX</c>, <c>ROWCOUNT</c> and
		/// <c>PAGECOUNT</c> only in UPDATE STATISTICS, and <c>ON PARTITIONS</c> only on <c>RESAMPLE</c>.
		/// </summary>
		void PutStatisticsOptions(SqlList<Option> options, bool update)
		{
			if (options.Count == 0)
				return;

			var scanned   = false;
			var persisted = false;

			foreach (var option in options)
			{
				if (option.Options.Count > 0 || option.Order is not null)
					throw Misplaced(option, "Options or Order", "on a statistics option");

				var name  = option.Name.Style == IdentifierStyle.Regular ? option.Name.Text.ToUpperInvariant() : "";
				var value = option.Value;

				// SAMPLE n PERCENT and SAMPLE n ROWS, the one measured value statistics read, and SAMPLE has no other.
				var read = name switch
				{
					"FULLSCAN" or "NORECOMPUTE"                                 => value is null,
					"SAMPLE"                                                    => value is Expression.Measured { Value: var size, Unit: var unit } && IsNumber(size) && Plain(unit, "PERCENT", "ROWS"),
					"PERSIST_SAMPLE_PERCENT" or "INCREMENTAL" or "AUTO_DROP"    => IsWord(value, "ON", "OFF"),
					"MAXDOP"                                                    => IsNumber(value),
					"STATS_STREAM"                                              => value is Expression.Literal { Value: LiteralValue.Binary { ZeroX: true } },
					"RESAMPLE" or "ALL" or "COLUMNS" or "INDEX"                 => update && value is null,
					"ROWCOUNT" or "PAGECOUNT"                                   => update && IsNumber(value),
					_                                                           => false,
				};

				if (!read)
					throw Misplaced(option, "Name or Value", (update ? "that UPDATE STATISTICS" : "that CREATE STATISTICS") + " does not read, or in a form the option does not read");

				if (option.Partitions is not null && name != "RESAMPLE")
					throw Misplaced(option, "Partitions", "on an option other than RESAMPLE, the only one of statistics that reads ON PARTITIONS");

				scanned   |= name is "FULLSCAN" or "SAMPLE" or "RESAMPLE";
				persisted |= name == "PERSIST_SAMPLE_PERCENT";
			}

			// A sample kept is kept from a scan.
			if (persisted && !scanned)
				throw Misplaced(options[0], "Name", "PERSIST_SAMPLE_PERCENT without FULLSCAN, SAMPLE or RESAMPLE in the same list, which is not read");

			Word("WITH");
			Each(options, option =>
			{
				if (Plain(option.Name, "SAMPLE"))
				{
					PutIdentifier(option.Name);
					PutExpression(option.Value!);
				}
				else if (Plain(option.Name, "RESAMPLE"))
				{
					PutIdentifier(option.Name);
					PutRanges(option.Partitions);
				}
				else
					PutOption(option);
			});
		}

		/// <summary>An unsigned number written as one: the grammar's Number, with a point or an exponent or without.</summary>
		static bool IsNumber(Expression? value)
		{
			return value is Expression.Literal { Value: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.Decimal or NumericLiteralKind.Approximate, Text: { Length: > 0 } text } }
				&& (char.IsDigit(text[0]) || text[0] == '.');
		}

		/// <summary>T-SQL: ALTER TABLE index_option (Transact-SQL). <c>ON PARTITIONS (1, 3 TO 5)</c>, where there are any.</summary>
		void PutRanges(SqlList<PartitionRange>? partitions)
		{
			if (partitions is not { } ranges)
				return;

			if (ranges.Count == 0)
				throw Empty(ranges, "Partitions");

			Word("ON PARTITIONS");
			Open();
			PutRangeList(ranges);
			Close();
		}

		void PutRangeList(SqlList<PartitionRange> partitions)
		{
			Each(partitions, partition =>
			{
				PutExpression(partition.From);

				if (partition.To is { } to)
				{
					Word("TO");
					PutExpression(to);
				}
			});
		}
	}
}
