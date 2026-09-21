using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §11, the schema and everything a schema holds — tables, views, domains,
// character sets, collations, transliterations, assertions, triggers, user-defined types, routines,
// casts, orderings, transforms and sequence generators — and §12, the roles and the privileges.
partial class HandSqlStandard
{
	// ── The publication ────────────────────────────────────────────────────────

	public static Statement ParseSQLSchemaStatement(string input)
	{
		return TryParseSQLSchemaStatement(input, out var value) ? value : throw Refused(input, "SQL schema statement");
	}

	public static bool TryParseSQLSchemaStatement(string input, out Statement value)
	{
		return Whole(input, SQLSchemaStatement, out value);
	}

	/// <summary>
	/// <c>&lt;SQL schema statement&gt;</c>: a definition or a manipulation. The first word says which
	/// of the four it is, and the second which object.
	/// </summary>
	static bool SQLSchemaStatement(ref SqlCursor cursor, out Statement statement)
	{
		switch (cursor.Word)
		{
			case SqlWord.Create: return CreateStatement(ref cursor, out statement);
			case SqlWord.Alter : return AlterStatement(ref cursor, out statement);
			case SqlWord.Drop  : return DropStatement(ref cursor, out statement);
			case SqlWord.Grant : return GrantStatement(ref cursor, out statement);
			case SqlWord.Revoke: return RevokeStatement(ref cursor, out statement);
		}

		statement = null!;

		return false;
	}

	static bool CreateStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		switch (cursor.Word)
		{
			case SqlWord.Name when cursor.IsWord("SCHEMA"): return Schema(ref cursor, save, out statement);
			case SqlWord.Table     : return Table(ref cursor, save, null, out statement);
			case SqlWord.Global    :
			case SqlWord.Local     : return Table(ref cursor, save, cursor.Word == SqlWord.Global ? TableScope.GlobalTemporary : TableScope.LocalTemporary, out statement);
			case SqlWord.Recursive : return View(ref cursor, save, out statement);
			case SqlWord.Name when cursor.IsWord("VIEW"): return View(ref cursor, save, out statement);
			case SqlWord.Name when cursor.IsWord("ROLE"): return Role(ref cursor, save, out statement);
			case SqlWord.Name when cursor.IsWord("DOMAIN"): return Domain(ref cursor, save, out statement);
			case SqlWord.Character : return CharacterSet(ref cursor, save, out statement);
			case SqlWord.Trigger   : return Trigger(ref cursor, save, out statement);
			case SqlWord.Procedure :
			case SqlWord.Function  :
			case SqlWord.Method    :
			case SqlWord.Name when cursor.IsWord("INSTANCE"):
			case SqlWord.Static    :
			case SqlWord.Specific  : return Routine(ref cursor, save, out statement);
			case SqlWord.Cast      : return Cast(ref cursor, save, out statement);
			case SqlWord.Name when cursor.IsWord("ORDERING"): return Ordering(ref cursor, save, out statement);
			case SqlWord.Name when cursor.IsWord("SEQUENCE"): return Sequence(ref cursor, save, out statement);
			case SqlWord.Translation: return Translation(ref cursor, save, out statement);
		}

		if (cursor.IsWord("ASSERTION"))
			return Assertion(ref cursor, save, out statement);

		if (cursor.IsWord("COLLATION"))
			return Collation(ref cursor, save, out statement);

		if (cursor.IsWord("TYPE"))
			return UserDefinedType(ref cursor, save, out statement);

		if (cursor.IsWord("CONSTRUCTOR"))
			return Routine(ref cursor, save, out statement);

		if (cursor.IsWord("TRANSFORM") || cursor.IsWord("TRANSFORMS"))
			return Transform(ref cursor, save, out statement);

		cursor = save;

		return false;
	}

	// ── §11.1 Schema definition ────────────────────────────────────────────────

	static bool Schema(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		QualifiedName? name = null;
		AuthorizationIdentifier? authorization = null;

		if (cursor.Take(SqlWord.Authorization))
		{
			if (!Identifier(ref cursor, out var only))
			{
				cursor = save;

				return false;
			}

			authorization = new AuthorizationIdentifier(only);
		}
		else if (SchemaName(ref cursor, out var written))
		{
			name = written;

			var marked = cursor;

			if (cursor.Take(SqlWord.Authorization))
			{
				if (Identifier(ref cursor, out var who))
					authorization = new AuthorizationIdentifier(who);
				else
					cursor = marked;
			}
		}
		else
		{
			cursor = save;

			return false;
		}

		// A default character set and a path, in either order.
		CharacterSetName? characterSet = null;
		PathSpecification? path = null;
		var pathFirst = false;

		characterSet = SchemaCharacterSetSpecification(ref cursor);

		if (characterSet is not null)
		{
			path = SchemaPath(ref cursor);
		}
		else
		{
			path = SchemaPath(ref cursor);

			if (path is not null)
			{
				pathFirst    = true;
				characterSet = SchemaCharacterSetSpecification(ref cursor);
			}
		}

		var elements = new List<Statement>();

		while (SchemaElement(ref cursor, out var element))
			elements.Add(element);

		statement = new Statement.CreateSchema
		{
			Name                = name,
			Authorization       = authorization,
			DefaultCharacterSet = characterSet,
			Path                = path,
			PathFirst           = pathFirst,
			Elements            = elements,
		};

		return true;
	}

	static CharacterSetName? SchemaCharacterSetSpecification(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Default) && cursor.Take(SqlWord.Character) && cursor.Take(SqlWord.Set) &&
			CharacterSetSpecification(ref cursor, out var name))
			return name;

		cursor = save;

		return null;
	}

	static PathSpecification? SchemaPath(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("PATH"))
			return null;

		var schemas = new List<QualifiedName>();

		while (true)
		{
			if (!SchemaName(ref cursor, out var name))
			{
				cursor = save;

				return null;
			}

			schemas.Add(name);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return new PathSpecification(schemas);
	}

	/// <summary>What a schema holds: everything but the schema itself and what alters or drops one.</summary>
	static bool SchemaElement(ref SqlCursor cursor, out Statement element)
	{
		var save = cursor;

		element = null!;

		if (cursor.Word is not (SqlWord.Create or SqlWord.Grant))
			return false;

		if (cursor.Word == SqlWord.Grant)
			return GrantStatement(ref cursor, out element);

		var marked = cursor;

		cursor.Take();

		// A schema holds no schema.
		if (cursor.IsWord("SCHEMA"))
		{
			cursor = save;

			return false;
		}

		cursor = marked;

		return CreateStatement(ref cursor, out element);
	}

	// ── §11.3 Table definition ─────────────────────────────────────────────────

	static bool Table(ref SqlCursor cursor, SqlCursor save, TableScope? scope, out Statement statement)
	{
		statement = null!;

		if (scope is not null)
		{
			cursor.Take();

			if (!cursor.TakeWord("TEMPORARY"))
			{
				cursor = save;

				return false;
			}
		}

		if (!cursor.Take(SqlWord.Table) || !TableName(ref cursor, out var name) || !TableContentsSource(ref cursor, out var contents))
		{
			cursor = save;

			return false;
		}

		var versioning = false;
		var marked     = cursor;

		if (cursor.Take(SqlWord.With))
		{
			versioning = cursor.TakeWord("SYSTEM") && cursor.Take(SqlWord.Versioning);

			if (!versioning)
				cursor = marked;
		}

		statement = new Statement.CreateTable
		{
			Scope            = scope,
			Name             = name,
			Contents         = contents,
			SystemVersioning = versioning,
			OnCommit         = OnCommit(ref cursor),
		};

		return true;
	}

	static bool TableContentsSource(ref SqlCursor cursor, out TableContents contents)
	{
		var save = cursor;

		contents = null!;

		// A column list before a query is an element list in shape, so the query is asked first.
		IReadOnlyList<Identifier>? columns = null;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		if (cursor.Take(SqlWord.As) && TableSubquery(ref cursor, out var query) && cursor.Take(SqlWord.With))
		{
			var no = cursor.Take(SqlWord.No);

			if (cursor.TakeWord("DATA"))
			{
				contents = new TableContents.AsQuery(columns ?? [], query, no ? WithDataMode.WithNoData : WithDataMode.WithData);

				return true;
			}
		}

		cursor = save;

		if (TableElementList(ref cursor, out var elements))
		{
			contents = new TableContents.Elements(elements);

			return true;
		}

		if (cursor.Take(SqlWord.Of))
		{
			if (Names(ref cursor, 3, out var type))
			{
				QualifiedName? under = null;

				var marked = cursor;

				if (cursor.TakeWord("UNDER") && !TableName(ref cursor, out under!))
				{
					cursor = marked;
					under  = null;
				}

				var typed = TypedTableElementList(ref cursor);

				contents = new TableContents.Typed(type, under, typed ?? []);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<TableElement>? TypedTableElementList(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return null;

		var elements = new List<TableElement>();

		while (true)
		{
			if (!TypedTableElement(ref cursor, out var element))
			{
				cursor = save;

				return null;
			}

			elements.Add(element);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return elements;

		cursor = save;

		return null;
	}

	static bool TypedTableElement(ref SqlCursor cursor, out TableElement element)
	{
		var save = cursor;

		element = null!;

		if (cursor.Take(SqlWord.Ref))
		{
			if (cursor.Take(SqlWord.Is) && Identifier(ref cursor, out var name))
			{
				element = new TableElement.SelfReference(name, ReferenceGeneration(ref cursor));

				return true;
			}

			cursor = save;

			return false;
		}

		if (TableConstraintDefinition(ref cursor, out var constraint))
		{
			element = new TableElement.TableConstraint(constraint);

			return true;
		}

		return ColumnOptions(ref cursor, out element);
	}

	static ReferenceGeneration? ReferenceGeneration(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("SYSTEM"))
		{
			if (cursor.TakeWord("GENERATED"))
				return Ast.ReferenceGeneration.SystemGenerated;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.User))
		{
			if (cursor.TakeWord("GENERATED"))
				return Ast.ReferenceGeneration.UserGenerated;

			cursor = save;

			return null;
		}

		if (cursor.TakeWord("DERIVED"))
			return Ast.ReferenceGeneration.Derived;

		return null;
	}

	static bool ColumnOptions(ref SqlCursor cursor, out TableElement element)
	{
		var save = cursor;

		element = null!;

		if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlWord.With) || !cursor.TakeWord("OPTIONS"))
		{
			cursor = save;

			return false;
		}

		QualifiedName? scope = null;

		var marked = cursor;

		if (cursor.Take(SqlWord.Scope) && !TableName(ref cursor, out scope!))
		{
			cursor = marked;
			scope  = null;
		}

		var defaults    = DefaultClause(ref cursor);
		var constraints = new List<Constraint>();

		while (ColumnConstraintDefinition(ref cursor, out var constraint))
			constraints.Add(constraint);

		element = new TableElement.ColumnOptions(name, new ColumnOptionList(scope, defaults, constraints));

		return true;
	}

	static bool TableElementList(ref SqlCursor cursor, out IReadOnlyList<TableElement> elements)
	{
		var save    = cursor;
		var written = new List<TableElement>();

		elements = written;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return false;

		while (true)
		{
			if (!TableElement(ref cursor, out var element))
			{
				cursor = save;

				return false;
			}

			written.Add(element);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return true;

		cursor = save;

		return false;
	}

	static bool TableElement(ref SqlCursor cursor, out TableElement element)
	{
		var save = cursor;

		element = null!;

		if (cursor.Take(SqlWord.Like))
		{
			if (TableName(ref cursor, out var table))
			{
				var options = new List<LikeOption>();

				while (LikeOption(ref cursor, out var option))
					options.Add(option);

				element = new TableElement.Like(table, options);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Word == SqlWord.Period)
		{
			if (TablePeriodDefinition(ref cursor, out var period))
			{
				element = new TableElement.Period(period);

				return true;
			}

			cursor = save;

			return false;
		}

		// A column begins with its name, which none of a constraint's key words can be.
		if (cursor.Word is SqlWord.Constraint or SqlWord.Unique or SqlWord.Primary or SqlWord.Foreign or SqlWord.Check)
		{
			if (TableConstraintDefinition(ref cursor, out var constraint))
			{
				element = new TableElement.TableConstraint(constraint);

				return true;
			}

			cursor = save;

			return false;
		}

		if (ColumnDefinition(ref cursor, out var column))
		{
			element = new TableElement.Column(column);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool LikeOption(ref SqlCursor cursor, out LikeOption option)
	{
		var save = cursor;

		option = default;

		var including = cursor.TakeWord("INCLUDING");

		if (!including && !cursor.TakeWord("EXCLUDING"))
			return false;

		var kind =
			cursor.Take(SqlWord.Identity)  ? 'i' :
			cursor.TakeWord("DEFAULTS")    ? 'd' :
			cursor.TakeWord("GENERATED")   ? 'g' :
			'\0';

		if (kind == '\0')
		{
			cursor = save;

			return false;
		}

		option = kind switch
		{
			'i' => including ? Ast.LikeOption.IncludingIdentity : Ast.LikeOption.ExcludingIdentity,
			'd' => including ? Ast.LikeOption.IncludingDefaults : Ast.LikeOption.ExcludingDefaults,
			_   => including ? Ast.LikeOption.IncludingGenerated : Ast.LikeOption.ExcludingGenerated,
		};

		return true;
	}

	static bool TablePeriodDefinition(ref SqlCursor cursor, out PeriodDefinition period)
	{
		var save = cursor;

		period = null!;

		if (!PeriodForSpecification(ref cursor, out var kind, out var name))
			return false;

		if (cursor.Take(SqlTokenKind.LeftParen) && Identifier(ref cursor, out var begin) && cursor.Take(SqlTokenKind.Comma) &&
			Identifier(ref cursor, out var end) && cursor.Take(SqlTokenKind.RightParen))
		{
			period = new PeriodDefinition(kind, name, begin, end);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool PeriodForSpecification(ref SqlCursor cursor, out PeriodKind kind, out Identifier? name)
	{
		var save = cursor;

		kind = PeriodKind.SystemTime;
		name = null;

		if (!cursor.Take(SqlWord.Period) || !cursor.Take(SqlWord.For))
		{
			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.SystemTime))
			return true;

		if (Identifier(ref cursor, out name))
		{
			kind = PeriodKind.ApplicationTime;

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §11.4 Column definition ────────────────────────────────────────────────

	static bool ColumnDefinition(ref SqlCursor cursor, out ColumnDefinition column)
	{
		var save = cursor;

		column = null!;

		if (!Identifier(ref cursor, out var name))
			return false;

		// `GENERATED` is no reserved word, and a user-defined type may be named so; a type is not
		// taken where `GENERATED ALWAYS` or `GENERATED BY` follows.
		DataType? type = null;

		var typed = cursor;

		if (!Generation(ref cursor) && DataType(ref cursor, out var written))
			type = written;
		else
			cursor = typed;

		var source = ColumnValueSource(ref cursor);
		var constraints = new List<Constraint>();

		while (ColumnConstraintDefinition(ref cursor, out var constraint))
			constraints.Add(constraint);

		CollateClause(ref cursor, out var collation);

		column = new ColumnDefinition(name, type, source, constraints, collation);

		return true;
	}

	/// <summary>Whether what stands here is a generation clause rather than a type's name.</summary>
	static bool Generation(ref SqlCursor cursor)
	{
		var save = cursor;
		var generated = cursor.IsWord("GENERATED");

		if (!generated)
			return false;

		cursor.Take();

		var goes = cursor.IsWord("ALWAYS") || cursor.Word == SqlWord.By;

		cursor = save;

		return goes;
	}

	static ColumnGeneration? ColumnValueSource(ref SqlCursor cursor)
	{
		var save = cursor;

		if (DefaultClause(ref cursor) is { } value)
			return new ColumnGeneration.Default(value);

		if (!cursor.IsWord("GENERATED"))
			return null;

		cursor.Take();

		var always = cursor.IsWord("ALWAYS");

		if (always)
			cursor.Take();

		if (!always && cursor.Take(SqlWord.By) && cursor.Take(SqlWord.Default) || always)
		{
			var identity = cursor;

			if (cursor.Take(SqlWord.As) && cursor.Take(SqlWord.Identity))
			{
				var options = new List<SequenceOption>();

				var bracket = cursor;

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					while (CommonSequenceGeneratorOption(ref cursor, out var option))
						options.Add(option);

					if (options.Count == 0 || !cursor.Take(SqlTokenKind.RightParen))
					{
						cursor = bracket;
						options.Clear();
					}
				}

				return new ColumnGeneration.Identity(always ? IdentityGeneration.Always : IdentityGeneration.ByDefault, options);
			}

			if (always)
			{
				cursor = identity;

				if (cursor.Take(SqlWord.As))
				{
					if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var expression) && cursor.Take(SqlTokenKind.RightParen))
						return new ColumnGeneration.Generated(expression);

					cursor = identity;
					cursor.Take();

					if (cursor.Take(SqlWord.Row))
					{
						if (cursor.Take(SqlWord.Start))
							return new ColumnGeneration.RowStart();

						if (cursor.Take(SqlWord.End))
							return new ColumnGeneration.RowEnd();
					}
				}
			}
		}

		cursor = save;

		return null;
	}

	static Expression? DefaultClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.Default))
			return null;

		if (Literal(ref cursor, out var literal))
			return new Expression.Literal(literal);

		if (DatetimeValueFunction(ref cursor, out var function))
			return function;

		var current =
			cursor.Take(SqlWord.User)                        ? CurrentValue.User :
			cursor.Take(SqlWord.CurrentUser)                 ? CurrentValue.CurrentUser :
			cursor.Take(SqlWord.CurrentRole)                 ? CurrentValue.Role :
			cursor.Take(SqlWord.SessionUser)                 ? CurrentValue.SessionUser :
			cursor.Take(SqlWord.SystemUser)                  ? CurrentValue.SystemUser :
			cursor.Take(SqlWord.CurrentCatalog)              ? CurrentValue.Catalog :
			cursor.Take(SqlWord.CurrentSchema)               ? CurrentValue.Schema :
			cursor.Take(SqlWord.CurrentPath)                 ? CurrentValue.Path :
			(CurrentValue?)null;

		if (current is not null)
			return new Expression.Current(current.Value);

		if (ImplicitlyTypedValueSpecification(ref cursor, out var implicitly))
			return implicitly;

		cursor = save;

		return null;
	}

	// ── §11.6 Constraints ──────────────────────────────────────────────────────

	static bool ColumnConstraintDefinition(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		var name = ConstraintNameDefinition(ref cursor);

		if (!ColumnConstraint(ref cursor, out constraint))
		{
			cursor = save;

			return false;
		}

		constraint = constraint with { Name = name, Characteristics = ConstraintCharacteristics(ref cursor) };

		return true;
	}

	static bool ColumnConstraint(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		if (cursor.Take(SqlWord.Not))
		{
			if (cursor.Take(SqlWord.Null))
			{
				constraint = new Constraint.NotNull();

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Unique))
		{
			constraint = new Constraint.Unique(UniqueKind.Unique, null, []);

			return true;
		}

		if (cursor.Take(SqlWord.Primary))
		{
			if (cursor.TakeWord("KEY"))
			{
				constraint = new Constraint.Unique(UniqueKind.PrimaryKey, null, []);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Word == SqlWord.References)
		{
			if (ReferencesSpecification(ref cursor, out var references))
			{
				constraint = new Constraint.ForeignKey([], null, references);

				return true;
			}

			cursor = save;

			return false;
		}

		return CheckConstraintDefinition(ref cursor, out constraint);
	}

	static bool TableConstraintDefinition(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		var name = ConstraintNameDefinition(ref cursor);

		if (!TableConstraint(ref cursor, out constraint))
		{
			cursor = save;

			return false;
		}

		constraint = constraint with { Name = name, Characteristics = ConstraintCharacteristics(ref cursor) };

		return true;
	}

	static bool TableConstraint(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		if (cursor.Word is SqlWord.Unique or SqlWord.Primary)
		{
			var unique = cursor.Word == SqlWord.Unique;

			cursor.Take();

			if (!unique && !cursor.TakeWord("KEY"))
			{
				cursor = save;

				return false;
			}

			// `UNIQUE (VALUE)`, and otherwise the columns.
			if (unique)
			{
				var marked = cursor;

				if (cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlWord.Value) && cursor.Take(SqlTokenKind.RightParen))
				{
					constraint = new Constraint.Unique(UniqueKind.UniqueValue, null, []);

					return true;
				}

				cursor = marked;
			}

			NullDistinctness? nulls = null;

			var distinct = cursor;

			if (cursor.TakeWord("NULLS"))
			{
				var not = cursor.Take(SqlWord.Not);

				if (cursor.Take(SqlWord.Distinct))
					nulls = not ? NullDistinctness.NotDistinct : NullDistinctness.Distinct;
				else
					cursor = distinct;
			}

			if (cursor.Take(SqlTokenKind.LeftParen) && ColumnNameList(ref cursor, out var columns))
			{
				Identifier? period = null;

				var overlaps = cursor;

				if (columns.Count > 1 && cursor.Take(SqlWord.Without) && cursor.TakeWord("OVERLAPS"))
				{
					// The list took the period's name, which is its last: `(a, p WITHOUT OVERLAPS)`.
					var written = new List<Identifier>(columns);

					period = written[written.Count - 1];

					written.RemoveAt(written.Count - 1);

					columns = written;
				}
				else
				{
					cursor = overlaps;
				}

				if (cursor.Take(SqlTokenKind.RightParen))
				{
					constraint = new Constraint.Unique(unique ? UniqueKind.Unique : UniqueKind.PrimaryKey, nulls, columns, period);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Foreign))
		{
			if (cursor.TakeWord("KEY") && cursor.Take(SqlTokenKind.LeftParen) && ColumnNameList(ref cursor, out var keys))
			{
				Identifier? period = null;

				var marked = cursor;

				if (cursor.Take(SqlTokenKind.Comma))
				{
					if (!cursor.Take(SqlWord.Period) || !Identifier(ref cursor, out period))
					{
						cursor = marked;
						period = null;
					}
				}

				if (cursor.Take(SqlTokenKind.RightParen) && ReferencesSpecification(ref cursor, out var references))
				{
					constraint = new Constraint.ForeignKey(keys, period, references);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		return CheckConstraintDefinition(ref cursor, out constraint);
	}

	static QualifiedName? ConstraintNameDefinition(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Constraint) && Names(ref cursor, 3, out var name))
			return name;

		cursor = save;

		return null;
	}

	static ConstraintCharacteristics? ConstraintCharacteristics(ref SqlCursor cursor)
	{
		var timing = ConstraintCheckTime(ref cursor);

		if (timing is not null)
		{
			bool? deferrable = null;

			var save = cursor;
			var not  = cursor.Take(SqlWord.Not);

			if (cursor.TakeWord("DEFERRABLE"))
				deferrable = !not;
			else
				cursor = save;

			return new Ast.ConstraintCharacteristics(timing, deferrable, ConstraintEnforcement(ref cursor));
		}

		var marked = cursor;
		var negated = cursor.Take(SqlWord.Not);

		if (cursor.TakeWord("DEFERRABLE"))
			return new Ast.ConstraintCharacteristics(ConstraintCheckTime(ref cursor), !negated, ConstraintEnforcement(ref cursor), true);

		cursor = marked;

		var enforced = ConstraintEnforcement(ref cursor);

		return enforced is null ? null : new Ast.ConstraintCharacteristics(null, null, enforced);
	}

	static ConstraintTiming? ConstraintCheckTime(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("INITIALLY"))
		{
			if (cursor.TakeWord("DEFERRED"))
				return ConstraintTiming.InitiallyDeferred;

			if (cursor.TakeWord("IMMEDIATE"))
				return ConstraintTiming.InitiallyImmediate;

			cursor = save;
		}

		return null;
	}

	static bool? ConstraintEnforcement(ref SqlCursor cursor)
	{
		var save = cursor;
		var not  = cursor.Take(SqlWord.Not);

		if (cursor.TakeWord("ENFORCED"))
			return !not;

		cursor = save;

		return null;
	}

	static bool ReferencesSpecification(ref SqlCursor cursor, out ReferencesSpecification references)
	{
		var save = cursor;

		references = null!;

		if (!cursor.Take(SqlWord.References) || !TableName(ref cursor, out var table))
		{
			cursor = save;

			return false;
		}

		IReadOnlyList<Identifier>? columns = null;
		Identifier? period = null;

		var bracket = cursor;

		if (cursor.Take(SqlTokenKind.LeftParen))
		{
			if (ColumnNameList(ref cursor, out var written))
			{
				var marked = cursor;

				if (cursor.Take(SqlTokenKind.Comma))
				{
					if (!cursor.Take(SqlWord.Period) || !Identifier(ref cursor, out period))
					{
						cursor = marked;
						period = null;
					}
				}

				if (cursor.Take(SqlTokenKind.RightParen))
					columns = written;
				else
					cursor = bracket;
			}
			else
			{
				cursor = bracket;
			}
		}

		Ast.MatchType? match = null;

		var matched = cursor;

		if (cursor.Take(SqlWord.Match))
		{
			match =
				cursor.TakeWord("FULL")    ? Ast.MatchType.Full :
				cursor.TakeWord("PARTIAL") ? Ast.MatchType.Partial :
				cursor.TakeWord("SIMPLE")  ? Ast.MatchType.Simple :
				(Ast.MatchType?)null;

			if (match is null)
				cursor = matched;
		}

		// An update rule, a delete rule, or both in either order.
		ReferentialAction? onUpdate = null, onDelete = null;
		var deleteFirst = false;

		var update = ReferentialRule(ref cursor, true);

		if (update is not null)
		{
			onUpdate = update;
			onDelete = ReferentialRule(ref cursor, false);
		}
		else
		{
			onDelete = ReferentialRule(ref cursor, false);

			if (onDelete is not null)
			{
				deleteFirst = true;
				onUpdate    = ReferentialRule(ref cursor, true);
			}
		}

		references = new Ast.ReferencesSpecification(table, columns ?? [], period, match, onUpdate, onDelete, deleteFirst);

		return true;
	}

	static ReferentialAction? ReferentialRule(ref SqlCursor cursor, bool update)
	{
		var save = cursor;

		if (!cursor.Take(SqlWord.On) || !(update ? cursor.Take(SqlWord.Update) : cursor.Take(SqlWord.Delete)))
		{
			cursor = save;

			return null;
		}

		if (cursor.TakeWord("CASCADE"))
			return ReferentialAction.Cascade;

		if (cursor.Take(SqlWord.Set))
		{
			if (cursor.Take(SqlWord.Null))
				return ReferentialAction.SetNull;

			if (cursor.Take(SqlWord.Default))
				return ReferentialAction.SetDefault;

			cursor = save;

			return null;
		}

		if (cursor.TakeWord("RESTRICT"))
			return ReferentialAction.Restrict;

		if (cursor.Take(SqlWord.No) && cursor.TakeWord("ACTION"))
			return ReferentialAction.NoAction;

		cursor = save;

		return null;
	}

	static bool CheckConstraintDefinition(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		if (cursor.Take(SqlWord.Check) && cursor.Take(SqlTokenKind.LeftParen) && SearchCondition(ref cursor, out var condition) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			constraint = new Constraint.Check(condition);

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §11.22 View definition ─────────────────────────────────────────────────

	static bool View(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		var recursive = cursor.Take(SqlWord.Recursive);

		if (!cursor.TakeWord("VIEW") || !TableName(ref cursor, out var name) || !ViewSpecification(ref cursor, out var specification) ||
			!cursor.Take(SqlWord.As) || !QueryExpression(ref cursor, out var query))
		{
			cursor = save;

			return false;
		}

		CheckOption? check = null;

		var marked = cursor;

		if (cursor.Take(SqlWord.With))
		{
			var cascaded = cursor.Take(SqlWord.Cascaded);
			var local    = !cascaded && cursor.Take(SqlWord.Local);

			if (cursor.Take(SqlWord.Check) && cursor.TakeWord("OPTION"))
				check = cascaded ? CheckOption.Cascaded : local ? CheckOption.Local : CheckOption.Unqualified;
			else
				cursor = marked;
		}

		statement = new Statement.CreateView
		{
			Recursive     = recursive,
			Name          = name,
			Specification = specification,
			Query         = query,
			CheckOption   = check,
		};

		return true;
	}

	static bool ViewSpecification(ref SqlCursor cursor, out ViewSpecification specification)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Of))
		{
			if (Names(ref cursor, 3, out var type))
			{
				QualifiedName? under = null;

				var marked = cursor;

				if (cursor.TakeWord("UNDER") && !TableName(ref cursor, out under!))
				{
					cursor = marked;
					under  = null;
				}

				var elements = ViewElementList(ref cursor);

				specification = new ViewSpecification.Referenceable(type, under, elements ?? []);

				return true;
			}

			cursor = save;
			specification = null!;

			return false;
		}

		IReadOnlyList<Identifier>? columns = null;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
				columns = written;
			else
				cursor = bracket;
		}

		specification = new ViewSpecification.Regular(columns ?? []);

		return true;
	}

	static IReadOnlyList<ViewElement>? ViewElementList(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return null;

		var elements = new List<ViewElement>();

		while (true)
		{
			if (!ViewElement(ref cursor, out var element))
			{
				cursor = save;

				return null;
			}

			elements.Add(element);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return elements;

		cursor = save;

		return null;
	}

	static bool ViewElement(ref SqlCursor cursor, out ViewElement element)
	{
		var save = cursor;

		element = null!;

		if (cursor.Take(SqlWord.Ref))
		{
			if (cursor.Take(SqlWord.Is) && Identifier(ref cursor, out var name))
			{
				element = new ViewElement.SelfReference(name, ReferenceGeneration(ref cursor));

				return true;
			}

			cursor = save;

			return false;
		}

		if (Identifier(ref cursor, out var column) && cursor.Take(SqlWord.With) && cursor.TakeWord("OPTIONS") &&
			cursor.Take(SqlWord.Scope) && TableName(ref cursor, out var scope))
		{
			element = new ViewElement.ColumnOption(column, scope);

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §11.24 Domain, §11.47 assertion, §11.49 trigger ────────────────────────

	static bool Domain(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		var keyword = cursor.Take(SqlWord.As);

		if (!PredefinedType(ref cursor, out var type))
		{
			cursor = save;

			return false;
		}

		var defaults    = DefaultClause(ref cursor);
		var constraints = new List<Constraint>();

		while (DomainConstraint(ref cursor, out var constraint))
			constraints.Add(constraint);

		CollateClause(ref cursor, out var collation);

		statement = new Statement.CreateDomain
		{
			Name        = name,
			AsKeyword   = keyword,
			Type        = type,
			Default     = defaults,
			Constraints = constraints,
			Collation   = collation,
		};

		return true;
	}

	static bool DomainConstraint(ref SqlCursor cursor, out Constraint constraint)
	{
		var save = cursor;

		constraint = null!;

		var name = ConstraintNameDefinition(ref cursor);

		if (!CheckConstraintDefinition(ref cursor, out constraint))
		{
			cursor = save;

			return false;
		}

		constraint = constraint with { Name = name, Characteristics = ConstraintCharacteristics(ref cursor) };

		return true;
	}

	static bool Assertion(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (Names(ref cursor, 3, out var name) && CheckConstraintDefinition(ref cursor, out var check))
		{
			statement = new Statement.CreateAssertion
			{
				Name            = name,
				Condition       = ((Constraint.Check)check).Condition,
				Characteristics = ConstraintCharacteristics(ref cursor),
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool Trigger(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		var time =
			cursor.TakeWord("BEFORE") ? TriggerTime.Before :
			cursor.TakeWord("AFTER")  ? TriggerTime.After :
			cursor.TakeWord("INSTEAD") && cursor.Take(SqlWord.Of) ? TriggerTime.InsteadOf :
			(TriggerTime?)null;

		if (time is null || !TriggerEvent(ref cursor, out var written) || !cursor.Take(SqlWord.On) || !TableName(ref cursor, out var table))
		{
			cursor = save;

			return false;
		}

		var references = new List<TransitionReference>();

		if (cursor.Take(SqlWord.Referencing))
		{
			while (TransitionTableOrVariable(ref cursor, out var reference))
				references.Add(reference);

			if (references.Count == 0)
			{
				cursor = save;

				return false;
			}
		}

		if (TriggeredAction(ref cursor, out var action))
		{
			statement = new Statement.CreateTrigger
			{
				Name        = name,
				Time        = time.Value,
				Event       = written,
				Table       = table,
				Referencing = references,
				Action      = action,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool TriggerEvent(ref SqlCursor cursor, out TriggerEvent written)
	{
		if (cursor.Take(SqlWord.Insert))
		{
			written = new Ast.TriggerEvent(TriggerEventKind.Insert, []);

			return true;
		}

		if (cursor.Take(SqlWord.Delete))
		{
			written = new Ast.TriggerEvent(TriggerEventKind.Delete, []);

			return true;
		}

		if (cursor.Take(SqlWord.Update))
		{
			IReadOnlyList<Identifier>? columns = null;

			var save = cursor;

			if (cursor.Take(SqlWord.Of) && !ColumnNameList(ref cursor, out columns!))
			{
				cursor  = save;
				columns = null;
			}

			written = new Ast.TriggerEvent(TriggerEventKind.Update, columns ?? []);

			return true;
		}

		written = null!;

		return false;
	}

	static bool TransitionTableOrVariable(ref SqlCursor cursor, out TransitionReference reference)
	{
		var save = cursor;

		reference = null!;

		var old = cursor.Take(SqlWord.Old);

		if (!old && !cursor.Take(SqlWord.New))
			return false;

		// A table is asked first: `TABLE` is reserved and no variable's name.
		var table = cursor;

		if (cursor.Take(SqlWord.Table))
		{
			var keyword = cursor.Take(SqlWord.As);

			if (Identifier(ref cursor, out var named))
			{
				reference = new Ast.TransitionReference(old ? TransitionKind.OldTable : TransitionKind.NewTable, named, false, keyword);

				return true;
			}

			cursor = table;
		}

		var row = cursor.Take(SqlWord.Row);
		var asKeyword = cursor.Take(SqlWord.As);

		if (Identifier(ref cursor, out var name))
		{
			reference = new Ast.TransitionReference(old ? TransitionKind.OldRow : TransitionKind.NewRow, name, row, asKeyword);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool TriggeredAction(ref SqlCursor cursor, out TriggerAction action)
	{
		var save = cursor;

		action = null!;

		TriggerGranularity? granularity = null;

		if (cursor.Take(SqlWord.For))
		{
			if (!cursor.Take(SqlWord.Each))
			{
				cursor = save;

				return false;
			}

			granularity =
				cursor.Take(SqlWord.Row)     ? TriggerGranularity.Row :
				cursor.TakeWord("STATEMENT") ? TriggerGranularity.Statement :
				(TriggerGranularity?)null;

			if (granularity is null)
			{
				cursor = save;

				return false;
			}
		}

		Expression? when = null;

		var marked = cursor;

		if (cursor.Take(SqlWord.When))
		{
			if (!cursor.Take(SqlTokenKind.LeftParen) || !SearchCondition(ref cursor, out when) || !cursor.Take(SqlTokenKind.RightParen))
			{
				cursor = marked;
				when   = null;
			}
		}

		// `BEGIN ATOMIC …; END`, or one statement.
		var block = cursor;

		if (cursor.Take(SqlWord.Begin))
		{
			if (cursor.Take(SqlWord.Atomic))
			{
				var statements = new List<Statement>();

				while (true)
				{
					if (!SQLExecutableStatement(ref cursor, out var statement) || !cursor.Take(SqlTokenKind.Semicolon))
						break;

					statements.Add(statement);
				}

				if (statements.Count > 0 && cursor.Take(SqlWord.End))
				{
					action = new TriggerAction(granularity, when, statements, true);

					return true;
				}
			}

			cursor = block;
		}

		if (SQLExecutableStatement(ref cursor, out var one))
		{
			action = new TriggerAction(granularity, when, [one], false);

			return true;
		}

		cursor = save;

		return false;
	}
}
