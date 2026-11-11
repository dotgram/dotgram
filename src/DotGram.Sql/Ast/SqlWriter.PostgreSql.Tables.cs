using System;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		/// <summary>
		/// One key as PostgreSQL's index_elem reads it: a column's name, a call by name or by a key word, or an expression in
		/// brackets — any other expression gets the brackets, and reads back inside them — then its collation, its operator
		/// class with that class's options, its direction and the place of its nulls.
		/// </summary>
		void PutIndexElement(IndexElement element)
		{
			switch (element.Key)
			{
				case Expression.Reference { Name.Count: 1 }:
				case Expression.Invocation { Filter: null, WithinGroup: null, Over: null }:
				case Expression.Parenthesized:
				case Expression.Cast or Expression.Extract or Expression.Treat or Expression.Trim or Expression.Position
					or Expression.Substring or Expression.Overlay or Expression.Normalize or Expression.CollationFor:
					PutExpression(element.Key);
					break;

				default:
					Open();
					PutExpression(element.Key);
					Close();
					break;
			}

			PutCollate(element.Collation);

			if (element.OperatorClass is { } operatorClass)
			{
				PutName(operatorClass);

				if (element.OperatorClassOptions.Count > 0)
				{
					Open();
					Each(element.OperatorClassOptions, PutDefinitionOption);
					Close();
				}
			}
			else if (element.OperatorClassOptions.Count > 0)
			{
				throw Misplaced(element, nameof(element.OperatorClassOptions), "without an operator class, whose options they are");
			}

			if (element.Direction is { } direction)
				Word(SortWord(direction));

			if (element.Nulls is { } nulls)
				Word(nulls switch { NullOrdering.First => "NULLS FIRST", NullOrdering.Last => "NULLS LAST", _ => throw NoText(nulls) });
		}

		/// <summary>
		/// One option of a <c>WITH (…)</c> as PostgreSQL's reloption_elem and def_elem read it: a name, and <c>= value</c> where
		/// one was written — a number, a string, a word, or the boolean literals.
		/// </summary>
		void PutDefinitionOption(Option option)
		{
			if (option.Options.Count > 0 || option.Words.Count > 0 || option.EqualsOmitted)
				throw Misplaced(option, nameof(option.Options), "of its own, further words or a left-out =, which a storage parameter does not read");

			PutIdentifier(option.Name);

			if (option.Value is not { } value)
				return;

			Word("=");

			switch (value)
			{
				case LiteralValue.Boolean or LiteralValue.String:
					PutExpression(value);
					break;

				case LiteralValue.Numeric or Expression.Unary { Operator: UnaryOperator.Minus or UnaryOperator.Plus, Operand: LiteralValue.Numeric }:
					PutExpression(value);
					break;

				case Expression.Reference { Name.Count: > 0 } reference:
					PutName(reference.Name);
					break;

				default:
					throw Misplaced(option, nameof(option.Value), "other than a number, a string, a word or a boolean, which are what a storage parameter is set to");
			}
		}

		/// <summary>PostgreSQL's <c>WITH (…)</c> of storage parameters, where any were written.</summary>
		void PutStorageParameters(SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(options, PutDefinitionOption);
			Close();
		}
	

		// ── CREATE TABLE ───────────────────────────────────────────────────────────

		/// <summary>
		/// PostgreSQL's CREATE TABLE: the temporary or unlogged word, IF NOT EXISTS, the name, the elements, OF a type or
		/// PARTITION OF a parent, then INHERITS, PARTITION BY, USING, WITH or WITHOUT OIDS, ON COMMIT and TABLESPACE; or, for a
		/// table made of a query, the column names, the same clauses but INHERITS and PARTITION BY, and AS the query.
		/// </summary>
		void PutPostgreSqlTable(Statement.CreateTable table)
		{
			if (table.Kind is not null || table.Placements.Count > 0 || table.SystemVersioning)
				throw Misplaced(table, "Kind, Placements or SystemVersioning", "which PostgreSQL's CREATE TABLE does not read");

			if (table.Options.Count > 0 && table.WithoutOids)
				throw Both(table, nameof(table.Options), nameof(table.WithoutOids));

			if (table.Name.Count > 3)
				throw Misplaced(table, nameof(table.Name), "of more than three parts, which PostgreSQL refuses (improper qualified name)");

			Word("CREATE");

			if (table.Unlogged)
			{
				if (table.Scope is not null)
					throw Both(table, nameof(table.Unlogged), nameof(table.Scope));

				Word("UNLOGGED");
			}

			if (table.Scope is { } scope)
			{
				if (scope != TableScope.Temporary)
					Word(scope switch { TableScope.GlobalTemporary => "GLOBAL", TableScope.LocalTemporary => "LOCAL", _ => throw NoText(scope) });

				Word(table.TempWord ? "TEMP" : "TEMPORARY");
			}
			else if (table.TempWord)
			{
				throw Misplaced(table, nameof(table.TempWord), "without a temporary scope, whose spelling it is");
			}

			Word("TABLE");

			if (table.IfNotExists)
				Word("IF NOT EXISTS");

			PutName(table.Name);

			switch (table.Contents)
			{
				case TableContents.AsQuery query:
					if (table.PartitionOf is not null || table.PartitionBy is not null || table.Inherits.Count > 0)
						throw Misplaced(table, "PartitionOf, PartitionBy or Inherits", "on a table made of a query, which takes none of them");

					if (query.Hints.Count > 0)
						throw Misplaced(query, nameof(query.Hints), "which PostgreSQL's CREATE TABLE AS does not read");

					if (query.Columns.Count > 0)
					{
						Open();
						Each(query.Columns, PutIdentifier);
						Close();
					}

					PutTableClauses(table);
					Word("AS");
					PutTableQuery(query);

					if (query.Data is { } data)
						Word(data switch { WithDataMode.WithData => "WITH DATA", WithDataMode.WithNoData => "WITH NO DATA", _ => throw NoText(data) });

					return;

				case TableContents.Typed typed:
					if (typed.Under is not null)
						throw Misplaced(typed, nameof(typed.Under), "which PostgreSQL's CREATE TABLE OF does not read");

					if (table.PartitionOf is not null || table.Inherits.Count > 0)
						throw Misplaced(table, "PartitionOf or Inherits", "on a table of a type, which takes neither");

					Word("OF");
					PutName(typed.TypeName);
					PutTypedElements(typed.Items);
					break;

				case TableContents.Elements elements when table.PartitionOf is { } parent:
					if (table.Inherits.Count > 0)
						throw Misplaced(table, nameof(table.Inherits), "on a partition, which inherits from its parent alone");

					if (parent.Parent.Count > 3)
						throw Misplaced(parent, nameof(parent.Parent), "of more than three parts, which PostgreSQL refuses (improper qualified name)");

					Word("PARTITION OF");
					PutName(parent.Parent);
					PutTypedElements(elements.Items);
					PutPartitionBound(parent.Bound);
					break;

				case TableContents.Elements elements:
					Open();
					Each(elements.Items, PutPostgreSqlElement);
					Close();

					if (table.Inherits.Count > 0)
					{
						Word("INHERITS");
						Open();
						Each(table.Inherits, PutRelationName);
						Close();
					}

					break;

				default:
					throw Unwritten(table.Contents);
			}

			if (table.PartitionBy is { } by)
			{
				Word("PARTITION BY");
				PutIdentifier(by.Strategy);

				if (by.Keys.Count == 0)
					throw Empty(by, nameof(by.Keys));

				Open();
				Each(by.Keys, key =>
				{
					if (key.Direction is not null || key.Nulls is not null || key.OperatorClassOptions.Count > 0)
						throw Misplaced(key, "Direction, Nulls or OperatorClassOptions", "on a partition key, which takes a collation and an operator class alone");

					PutIndexElement(key);
				});
				Close();
			}

			PutTableClauses(table);
		}

		/// <summary>
		/// A table's query after AS, bare where the brackets were left out and in its own brackets where not. A bare query in
		/// brackets that enclose the whole of it would read back as the record's brackets, and is refused.
		/// </summary>
		void PutTableQuery(TableContents.AsQuery query)
		{
			if (!query.BracketsOmitted)
			{
				PutSubquery(query.Query);
				return;
			}

			if (TableContents.AsQuery.Enclosed(query.Query) is not null)
				throw Misplaced(query, nameof(query.BracketsOmitted), "with a query in brackets that enclose the whole of it, which are the record's own and read back as BracketsOmitted false");

			PutQuery(query.Query);
		}

		/// <summary>USING, WITH or WITHOUT OIDS, ON COMMIT and TABLESPACE, as CREATE TABLE and CREATE TABLE AS end.</summary>
		void PutTableClauses(Statement.CreateTable table)
		{
			if (table.AccessMethod is { } method)
			{
				Word("USING");
				PutIdentifier(method);
			}

			PutStorageParameters(table.Options);

			if (table.WithoutOids)
				Word("WITHOUT OIDS");

			if (table.OnCommit is { } commit)
				Word(commit switch { TableCommitAction.Preserve => "ON COMMIT PRESERVE ROWS", TableCommitAction.Delete => "ON COMMIT DELETE ROWS", TableCommitAction.Drop => "ON COMMIT DROP", _ => throw NoText(commit) });

			if (table.Tablespace is { } space)
			{
				Word("TABLESPACE");
				PutIdentifier(space);
			}
		}

		/// <summary>A table's name where gram.y reads a qualified_name: three parts at most.</summary>
		void PutRelationName(QualifiedName name)
		{
			if (name.Count > 3)
				throw new ArgumentException("A table's name of more than three parts, which PostgreSQL refuses (improper qualified name).", nameof(name));

			PutName(name);
		}

		/// <summary>The bracket of a typed table's or a partition's elements, where it has any: column options and constraints.</summary>
		void PutTypedElements(SqlList<TableElement> items)
		{
			if (items.Count == 0)
				return;

			Open();
			Each(items, item =>
			{
				switch (item)
				{
					case TableElement.ColumnOptions options:
						if (options.Options.Scope is not null)
							throw Misplaced(options, "Options.Scope", "which PostgreSQL does not read");

						PutIdentifier(options.Name);

						if (!options.WithOptionsOmitted)
							Word("WITH OPTIONS");

						if (options.Options.Default is { } value)
							PutPostgreSqlDefault(value);

						PutColumnConstraints(options.Options.Constraints);
						PutCollate(options.Options.Collation);
						break;

					case TableElement.TableConstraint constraint:
						PutTableConstraint(constraint.Value);
						break;

					default:
						throw Misplaced(item, "Kind", "other than column options or a constraint, which alone a typed table's or a partition's bracket holds");
				}
			});
			Close();
		}

		/// <summary>gram.y PartitionBoundSpec.</summary>
		void PutPartitionBound(PartitionBound bound)
		{
			switch (bound)
			{
				case PartitionBound.In @in:
					if (@in.Values.Count == 0)
						throw Empty(@in, nameof(@in.Values));

					Word("FOR VALUES IN");
					Open();
					Each(@in.Values, PutExpression);
					Close();
					break;

				case PartitionBound.Range range:
					if (range.From.Count == 0 || range.To.Count == 0)
						throw Empty(range, nameof(range.From), nameof(range.To));

					Word("FOR VALUES FROM");
					Open();
					Each(range.From, PutExpression);
					Close();
					Word("TO");
					Open();
					Each(range.To, PutExpression);
					Close();
					break;

				case PartitionBound.Hash hash:
					if (!PostgreSql.Nodes.HashFits(hash.Options))
						throw Misplaced(hash, nameof(hash.Options), "other than one MODULUS and one REMAINDER, each with a whole number");

					Word("FOR VALUES WITH");
					Open();
					Each(hash.Options, option =>
					{
						PutIdentifier(option.Name);
						PutExpression(option.Value!);
					});
					Close();
					break;

				case PartitionBound.Default:
					Word("DEFAULT");
					break;

				default:
					throw Unwritten(bound);
			}
		}

		/// <summary>One of a table's elements in PostgreSQL's text: a column, a constraint, or LIKE.</summary>
		void PutPostgreSqlElement(TableElement element)
		{
			switch (element)
			{
				case TableElement.Column column:
					PutPostgreSqlColumn(column.Definition);
					break;

				case TableElement.TableConstraint constraint:
					PutTableConstraint(constraint.Value);
					break;

				case TableElement.Like like:
					Word("LIKE");
					PutRelationName(like.Table);

					foreach (var option in like.Options)
						PutLikeOption(option);

					break;

				default:
					throw Misplaced(element, "Kind", "other than a column, a constraint or LIKE, which PostgreSQL's table element is");
			}
		}

		/// <summary>gram.y columnDef: the name, the type, STORAGE, COMPRESSION, the default or generation, the constraints, the collation.</summary>
		void PutPostgreSqlColumn(ColumnDefinition column)
		{
			if (column.Options.Count > 0)
				throw Misplaced(column, nameof(column.Options), "T-SQL's, which PostgreSQL does not read");

			if (column.Type is not { } type)
				throw Empty(column, nameof(column.Type));

			PutIdentifier(column.Name);
			PutColumnType(type);

			if (column.Storage is { } storage)
			{
				Word("STORAGE");
				PutIdentifier(storage);
			}

			if (column.Compression is { } compression)
			{
				Word("COMPRESSION");
				PutIdentifier(compression);
			}

			if (column.Generation?.Name is { } named)
			{
				Word("CONSTRAINT");
				PutOnePartName(named, "a default's or a generation's name");
			}

			switch (column.Generation)
			{
				case null:
					break;

				case ColumnGeneration.Default @default:
					PutPostgreSqlDefault(@default.Value);
					break;

				case ColumnGeneration.Identity identity:
					Word(identity.Generation switch { IdentityGeneration.Always => "GENERATED ALWAYS AS IDENTITY", IdentityGeneration.ByDefault => "GENERATED BY DEFAULT AS IDENTITY", _ => throw NoText(identity.Generation) });

					if (identity.Options.Count > 0)
					{
						Open();

						foreach (var option in identity.Options)
							PutPostgreSqlSequenceOption(option);

						Close();
					}

					break;

				case ColumnGeneration.Generated generated:
					Word("GENERATED ALWAYS AS");
					Open();
					PutExpression(generated.Expression);
					Close();

					if (generated.Storage is { } kept)
						Word(kept switch { GenerationStorage.Stored => "STORED", GenerationStorage.Virtual => "VIRTUAL", _ => throw NoText(kept) });

					break;

				default:
					throw Misplaced(column.Generation, "Kind", "a row start or end, which PostgreSQL does not read");
			}

			PutColumnConstraints(column.Constraints);
			PutCollate(column.Collation);
		}

		/// <summary>
		/// DEFAULT and its value, which gram.y reads as a b_expr: a value that is not one — a COLLATE, which would read as the
		/// column's, an AND, a predicate — is written in brackets, and reads back inside them.
		/// </summary>
		void PutPostgreSqlDefault(Expression value)
		{
			Word("DEFAULT");

			if (PostgreSql.Nodes.IsBExpr(value))
			{
				PutExpression(value);
				return;
			}

			Open();
			PutExpression(value);
			Close();
		}

		/// <summary>A column's constraints, each with the attributes the server attaches to it.</summary>
		void PutColumnConstraints(SqlList<Constraint> constraints)
		{
			foreach (var constraint in constraints)
				PutColumnConstraint(constraint);
		}

		/// <summary>gram.y ColConstraintElem and the ConstraintAttr after it.</summary>
		void PutColumnConstraint(Constraint constraint)
		{
			if (constraint.NotValid)
				throw Misplaced(constraint, nameof(constraint.NotValid), "on a column's constraint, which gram.y reads at the table alone");

			if (constraint.NoInherit && constraint is not (Constraint.NotNull or Constraint.Check))
				throw Misplaced(constraint, nameof(constraint.NoInherit), "on a column's constraint other than NOT NULL and CHECK");

			PutConstraintName(constraint);

			switch (constraint)
			{
				case Constraint.NotNull notNull:
					if (notNull.Column is not null)
						throw Misplaced(notNull, nameof(notNull.Column), "on a column's own NOT NULL, which names no column");

					Word("NOT NULL");
					break;

				case Constraint.Null:
					Word("NULL");
					break;

				case Constraint.Unique unique:
					if (unique.Columns.Count > 0 || unique.WithoutOverlapsPeriod is not null || unique.Include.Count > 0 || unique.ExistingIndex is not null)
						throw Misplaced(unique, "Columns, WithoutOverlapsPeriod, Include or ExistingIndex", "on a column's own key, which names nothing");

					PutKeyWords(unique);
					PutKeyIndex(unique.Options, unique.IndexTablespace);
					break;

				case Constraint.Check check:
					if (check.NotForReplication)
						throw Misplaced(check, nameof(check.NotForReplication), "T-SQL's, which PostgreSQL does not read");

					Word("CHECK");
					Open();
					PutExpression(check.Condition);
					Close();
					break;

				case Constraint.ForeignKey foreign:
					if (foreign.Columns.Count > 0 || foreign.ReferencingPeriod is not null || foreign.ForeignKeyWords || foreign.References.Period is not null)
						throw Misplaced(foreign, "Columns, ReferencingPeriod, ForeignKeyWords or References.Period", "on a column's own REFERENCES");

					PutPostgreSqlReferences(foreign.References);
					break;

				default:
					throw Misplaced(constraint, "Kind", "that a column does not take in PostgreSQL's text");
			}

			if (constraint.NoInherit)
				Word("NO INHERIT");

			// transformConstraintAttrs: DEFERRABLE and INITIALLY after a key or a foreign key, ENFORCED after a foreign key or a CHECK.
			if (constraint.Characteristics is { } characteristics)
			{
				if ((characteristics.Deferrable is not null || characteristics.Timing is not null) && constraint is not (Constraint.Unique or Constraint.ForeignKey))
					throw Misplaced(constraint, nameof(constraint.Characteristics), "deferrable or timed on a column's constraint other than a key or a foreign key");

				if (characteristics.Enforced is not null && constraint is not (Constraint.ForeignKey or Constraint.Check))
					throw Misplaced(constraint, nameof(constraint.Characteristics), "enforced or not on a column's constraint other than a foreign key or a CHECK");

				PutPostgreSqlCharacteristics(constraint, characteristics);
			}
		}

		/// <summary>A constraint's name, CONSTRAINT and one part.</summary>
		void PutConstraintName(Constraint constraint)
		{
			if (constraint.Name is { } name)
			{
				Word("CONSTRAINT");
				PutOnePartName(name, "a constraint's name");
			}
		}

		/// <summary>UNIQUE with its NULLS treatment, or PRIMARY KEY, refusing T-SQL's members.</summary>
		void PutKeyWords(Constraint.Unique unique)
		{
			if (unique is not { Clustering: null, Storage: null, Keys: null, OptionsUnbracketed: false, On: null })
				throw Misplaced(unique, "Clustering, Storage, Keys, OptionsUnbracketed or On", "T-SQL's, which PostgreSQL does not read");

			switch (unique.Kind)
			{
				case UniqueKind.Unique:
					Word("UNIQUE");

					if (unique.Nulls is { } nulls)
						Word(nulls switch { NullDistinctness.Distinct => "NULLS DISTINCT", NullDistinctness.NotDistinct => "NULLS NOT DISTINCT", _ => throw NoText(nulls) });

					break;

				case UniqueKind.PrimaryKey:
					if (unique.Nulls is not null)
						throw Misplaced(unique, nameof(unique.Nulls), "on a primary key, which takes no NULLS treatment");

					Word("PRIMARY KEY");
					break;

				default:
					throw Misplaced(unique, nameof(unique.Kind), "UNIQUE (VALUE), which PostgreSQL does not read");
			}
		}

		/// <summary>A key's or an exclusion's index: its storage parameters and its tablespace.</summary>
		void PutKeyIndex(SqlList<Option> options, Identifier? tablespace)
		{
			PutStorageParameters(options);

			if (tablespace is { } space)
			{
				Word("USING INDEX TABLESPACE");
				PutIdentifier(space);
			}
		}

		/// <summary>The attributes of a constraint: the standard's characteristics, which PostgreSQL reads in any order.</summary>
		void PutPostgreSqlCharacteristics(Constraint constraint, ConstraintCharacteristics characteristics)
		{
			if (characteristics.Timing is ConstraintTiming.Deferred or ConstraintTiming.Immediate)
				throw Misplaced(constraint, nameof(constraint.Characteristics), "timed by SET CONSTRAINTS' words, which a constraint does not take");

			if (characteristics.Deferrable == false && characteristics.Timing == ConstraintTiming.InitiallyDeferred)
				throw Misplaced(constraint, nameof(constraint.Characteristics), "INITIALLY DEFERRED and NOT DEFERRABLE, which the server refuses");

			if (characteristics.DeferrableFirst && characteristics.Deferrable is null)
				throw Misplaced(constraint, nameof(constraint.Characteristics), "DeferrableFirst with no deferrability to stand first");

			PutCharacteristics(characteristics);
		}

		/// <summary>gram.y TableConstraint: a constraint at the table, with its attributes.</summary>
		void PutTableConstraint(Constraint constraint)
		{
			PutConstraintName(constraint);

			// processCASbits: what each kind takes besides NOT DEFERRABLE and INITIALLY IMMEDIATE.
			bool deferrable, enforced, notValid, noInherit;

			switch (constraint)
			{
				case Constraint.Check check:
					if (check.NotForReplication)
						throw Misplaced(check, nameof(check.NotForReplication), "T-SQL's, which PostgreSQL does not read");

					Word("CHECK");
					Open();
					PutExpression(check.Condition);
					Close();
					(deferrable, enforced, notValid, noInherit) = (false, true, true, true);
					break;

				case Constraint.NotNull notNull:
					if (notNull.Column is not { } column)
						throw Empty(notNull, nameof(notNull.Column));

					Word("NOT NULL");
					PutIdentifier(column);
					(deferrable, enforced, notValid, noInherit) = (false, false, true, true);
					break;

				case Constraint.Unique unique:
					PutKeyWords(unique);

					if (unique.ExistingIndex is { } index)
					{
						if (unique.Columns.Count > 0 || unique.WithoutOverlapsPeriod is not null || unique.Include.Count > 0 || unique.Options.Count > 0 || unique.IndexTablespace is not null || unique.Nulls is not null)
							throw Misplaced(unique, nameof(unique.ExistingIndex), "beside columns, a period, INCLUDE, options, a tablespace or NULLS, which an index already there has its own of");

						Word("USING INDEX");
						PutIdentifier(index);
					}
					else
					{
						if (unique.Columns.Count == 0)
							throw Empty(unique, nameof(unique.Columns));

						Open();
						Each(unique.Columns, PutIdentifier);

						if (unique.WithoutOverlapsPeriod is { } period)
						{
							Tight(",");
							PutIdentifier(period);
							Word("WITHOUT OVERLAPS");
						}

						Close();
						PutInclude(unique.Include);
						PutKeyIndex(unique.Options, unique.IndexTablespace);
					}

					(deferrable, enforced, notValid, noInherit) = (true, false, false, false);
					break;

				case Constraint.Exclude exclude:
					if (exclude.Elements.Count == 0)
						throw Empty(exclude, nameof(exclude.Elements));

					Word("EXCLUDE");

					if (exclude.Method is { } method)
					{
						Word("USING");
						PutIdentifier(method);
					}

					Open();
					Each(exclude.Elements, element =>
					{
						PutIndexElement(element.Element);
						Word("WITH");
						PutOperatorName(element.Operator, element.OperatorKeyword, quantified: false, any: true);
					});
					Close();
					PutInclude(exclude.Include);
					PutKeyIndex(exclude.Options, exclude.IndexTablespace);

					if (exclude.Where is { } where)
					{
						Word("WHERE");
						Open();
						PutExpression(where);
						Close();
					}

					(deferrable, enforced, notValid, noInherit) = (true, false, false, false);
					break;

				case Constraint.ForeignKey foreign:
					if (foreign.Columns.Count == 0 || foreign.ForeignKeyWords)
						throw Misplaced(foreign, "Columns or ForeignKeyWords", "on a table's FOREIGN KEY, which names its columns");

					Word("FOREIGN KEY");
					Open();
					Each(foreign.Columns, PutIdentifier);

					if (foreign.ReferencingPeriod is { } referencing)
					{
						Tight(",");
						Word("PERIOD");
						PutIdentifier(referencing);
					}

					Close();
					PutPostgreSqlReferences(foreign.References);
					(deferrable, enforced, notValid, noInherit) = (true, true, true, false);
					break;

				default:
					throw Misplaced(constraint, "Kind", "that a table does not take in PostgreSQL's text");
			}

			if (constraint.Characteristics is { } characteristics)
			{
				if (!deferrable && (characteristics.Deferrable == true || characteristics.Timing == ConstraintTiming.InitiallyDeferred))
					throw Misplaced(constraint, nameof(constraint.Characteristics), "deferrable on a constraint the server holds to be checked at once");

				if (!enforced && characteristics.Enforced is not null)
					throw Misplaced(constraint, nameof(constraint.Characteristics), "enforced or not on a constraint that takes neither");

				PutPostgreSqlCharacteristics(constraint, characteristics);
			}

			if (constraint.NotValid)
			{
				if (!notValid)
					throw Misplaced(constraint, nameof(constraint.NotValid), "on a constraint that takes no NOT VALID");

				Word("NOT VALID");
			}

			if (constraint.NoInherit)
			{
				if (!noInherit)
					throw Misplaced(constraint, nameof(constraint.NoInherit), "on a constraint that takes no NO INHERIT");

				Word("NO INHERIT");
			}
		}

		/// <summary><c>INCLUDE (…)</c>, where columns were named.</summary>
		void PutInclude(SqlList<Identifier> include)
		{
			if (include.Count == 0)
				return;

			Word("INCLUDE");
			Open();
			Each(include, PutIdentifier);
			Close();
		}

		/// <summary>gram.y's REFERENCES: the table, its columns and period, MATCH, and the actions with ON DELETE's columns.</summary>
		void PutPostgreSqlReferences(ReferencesSpecification references)
		{
			if (references.NotForReplication)
				throw Misplaced(references, nameof(references.NotForReplication), "T-SQL's, which PostgreSQL does not read");

			if (references.OnDeleteColumns.Count > 0 && references.OnDelete is not (ReferentialAction.SetNull or ReferentialAction.SetDefault))
				throw Misplaced(references, nameof(references.OnDeleteColumns), "with an ON DELETE other than SET NULL or SET DEFAULT, whose columns they are");

			if (references.Period is not null && references.Columns.Count == 0)
				throw Misplaced(references, nameof(references.Period), "with no columns before it");

			Word("REFERENCES");
			PutRelationName(references.Table);

			if (references.Columns.Count > 0)
			{
				Open();
				Each(references.Columns, PutIdentifier);

				if (references.Period is { } period)
				{
					Tight(",");
					Word("PERIOD");
					PutIdentifier(period);
				}

				Close();
			}

			if (references.Match is { } match)
				Word(match switch { MatchType.Full => "MATCH FULL", MatchType.Partial => "MATCH PARTIAL", MatchType.Simple => "MATCH SIMPLE", _ => throw NoText(match) });

			if (references.DeleteRuleFirst)
			{
				PutDeleteRule(references);
				PutRule("ON UPDATE", references.OnUpdate);
			}
			else
			{
				PutRule("ON UPDATE", references.OnUpdate);
				PutDeleteRule(references);
			}
		}

		/// <summary>ON DELETE and its action, with the columns SET NULL or SET DEFAULT names.</summary>
		void PutDeleteRule(ReferencesSpecification references)
		{
			PutRule("ON DELETE", references.OnDelete);

			if (references.OnDeleteColumns.Count > 0)
			{
				Open();
				Each(references.OnDeleteColumns, PutIdentifier);
				Close();
			}
		}

		/// <summary>One of LIKE's options, every one of gram.y's TableLikeOption.</summary>
		void PutLikeOption(LikeOption option)
		{
			Word(option switch
			{
				LikeOption.IncludingIdentity     => "INCLUDING IDENTITY",
				LikeOption.ExcludingIdentity     => "EXCLUDING IDENTITY",
				LikeOption.IncludingDefaults     => "INCLUDING DEFAULTS",
				LikeOption.ExcludingDefaults     => "EXCLUDING DEFAULTS",
				LikeOption.IncludingGenerated    => "INCLUDING GENERATED",
				LikeOption.ExcludingGenerated    => "EXCLUDING GENERATED",
				LikeOption.IncludingComments     => "INCLUDING COMMENTS",
				LikeOption.ExcludingComments     => "EXCLUDING COMMENTS",
				LikeOption.IncludingCompression  => "INCLUDING COMPRESSION",
				LikeOption.ExcludingCompression  => "EXCLUDING COMPRESSION",
				LikeOption.IncludingConstraints  => "INCLUDING CONSTRAINTS",
				LikeOption.ExcludingConstraints  => "EXCLUDING CONSTRAINTS",
				LikeOption.IncludingIndexes      => "INCLUDING INDEXES",
				LikeOption.ExcludingIndexes      => "EXCLUDING INDEXES",
				LikeOption.IncludingStatistics   => "INCLUDING STATISTICS",
				LikeOption.ExcludingStatistics   => "EXCLUDING STATISTICS",
				LikeOption.IncludingStorage      => "INCLUDING STORAGE",
				LikeOption.ExcludingStorage      => "EXCLUDING STORAGE",
				LikeOption.IncludingAll          => "INCLUDING ALL",
				LikeOption.ExcludingAll          => "EXCLUDING ALL",
				_                                => throw NoText(option),
			});
		}

		/// <summary>gram.y SeqOptElem.</summary>
		void PutPostgreSqlSequenceOption(SequenceOption option)
		{
			switch (option)
			{
				case SequenceOption.DataTypeOption type:
					Word("AS");
					PutType(type.Type);
					break;

				case SequenceOption.Cache { Size: { } size } cache:
					Word("CACHE");
					PutSignedNumber(cache, size);
					break;

				case SequenceOption.Cycle cycle:
					Word(cycle.Value ? "CYCLE" : "NO CYCLE");
					break;

				case SequenceOption.Increment increment:
					Word(increment.ByOmitted ? "INCREMENT" : "INCREMENT BY");
					PutSignedNumber(increment, increment.Value);
					break;

				case SequenceOption.Logged logged:
					Word(logged.Value ? "LOGGED" : "UNLOGGED");
					break;

				case SequenceOption.Max max:
					if (max.No)
					{
						if (max.Value is not null)
							throw Both(max, nameof(max.No), nameof(max.Value));

						Word("NO MAXVALUE");
					}
					else
					{
						Word("MAXVALUE");
						PutSignedNumber(max, max.Value ?? throw Empty(max, nameof(max.Value)));
					}

					break;

				case SequenceOption.Min min:
					if (min.No)
					{
						if (min.Value is not null)
							throw Both(min, nameof(min.No), nameof(min.Value));

						Word("NO MINVALUE");
					}
					else
					{
						Word("MINVALUE");
						PutSignedNumber(min, min.Value ?? throw Empty(min, nameof(min.Value)));
					}

					break;

				case SequenceOption.OwnedBy owned:
					Word("OWNED BY");
					PutName(owned.Column);
					break;

				case SequenceOption.SequenceName named:
					Word("SEQUENCE NAME");
					PutName(named.Name);
					break;

				case SequenceOption.Start start:
					Word(start.WithOmitted ? "START" : "START WITH");
					PutSignedNumber(start, start.Value);
					break;

				case SequenceOption.Restart restart:
					Word("RESTART");

					if (restart.Value is { } value)
					{
						if (!restart.WithOmitted)
							Word("WITH");

						PutSignedNumber(restart, value);
					}
					else if (restart.WithOmitted)
					{
						throw Misplaced(restart, nameof(restart.WithOmitted), "with no value after RESTART");
					}

					break;

				default:
					throw Misplaced(option, "Kind", "that PostgreSQL's sequence options do not read");
			}
		}

		/// <summary>
		/// The members PostgreSQL alone writes on CREATE TABLE and its parts, refused for any other reading, which would drop
		/// them: the standard's and T-SQL's writers ask this before they write a table.
		/// </summary>
		void CheckNotPostgreSqlTable(Statement.CreateTable table)
		{
			if (table.Unlogged || table.IfNotExists || table.TempWord || table.PartitionOf is not null || table.PartitionBy is not null || table.Inherits.Count > 0
				|| table.AccessMethod is not null || table.WithoutOids || table.Tablespace is not null || table.Scope == TableScope.Temporary || table.OnCommit == TableCommitAction.Drop)
				throw Misplaced(table, "Unlogged, IfNotExists, TempWord, PartitionOf, PartitionBy, Inherits, AccessMethod, WithoutOids, Tablespace, Scope or OnCommit", "which only PostgreSQL reads on CREATE TABLE");
		}

		/// <summary>The members PostgreSQL alone writes on a column, refused for any other reading.</summary>
		void CheckNotPostgreSqlColumn(ColumnDefinition column)
		{
			if (column.Storage is not null || column.Compression is not null || column.Generation is ColumnGeneration.Generated { Storage: not null } or { Name: not null })
				throw Misplaced(column, "Storage, Compression, Generation.Storage or Generation.Name", "which only PostgreSQL reads on a column");

			foreach (var constraint in column.Constraints)
				CheckNotPostgreSqlConstraint(constraint);
		}

		/// <summary>The members PostgreSQL alone writes on a constraint, refused for any other reading.</summary>
		void CheckNotPostgreSqlConstraint(Constraint constraint)
		{
			if (constraint is Constraint.Null or Constraint.Exclude)
				OnlyPostgreSql(constraint);

			if (constraint.NoInherit || constraint.NotValid
				|| constraint is Constraint.NotNull { Column: not null } or Constraint.Unique { Include.Count: > 0 } or Constraint.Unique { IndexTablespace: not null } or Constraint.Unique { ExistingIndex: not null }
				|| constraint is Constraint.ForeignKey { References.OnDeleteColumns.Count: > 0 })
				throw Misplaced(constraint, "NoInherit, NotValid, Column, Include, IndexTablespace, ExistingIndex or OnDeleteColumns", "which only PostgreSQL reads on a constraint");
		}
	}
}
