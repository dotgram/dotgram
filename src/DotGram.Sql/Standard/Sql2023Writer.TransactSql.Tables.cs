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

				case AlterTableAction.Add add:
					if (add.Elements.Count < 2)
						throw Misplaced(add, "Elements", "fewer than two, which the standard's AddColumn or AddConstraint holds");

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

					if (rebuild.Partition is Expression.Reference { Name.Parts: [{ Style: IdentifierStyle.Regular } word] } && word.Text.Equals("ALL", StringComparison.OrdinalIgnoreCase))
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
		/// (…) [INCLUDE (…)] [WHERE …] [WITH (…)] [ON …]</c>, at the table or on a column.
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

			if (index.Columns.Count > 0)
			{
				Open();
				PutSorts(new OrderByClause(index.Columns));
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

			if (index.On is { } on)
				PutPlacement(on, PlacementKind.On);
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
	}
}
