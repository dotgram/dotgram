using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §11's routines, user-defined types, casts, orderings, transforms, character
// sets, collations, transliterations and sequence generators, §12's roles and privileges, and what
// alters and drops each of them.
partial class HandSqlStandard
{
	// ── What `ALTER` and `DROP` name ───────────────────────────────────────────

	static bool AlterStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		switch (cursor.Word)
		{
			case SqlWord.Table:
			{
				cursor.Take();

				if (TableName(ref cursor, out var name) && AlterTableAction(ref cursor, out var action))
				{
					statement = new Statement.AlterTable { Name = name, Actions = [action] };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("DOMAIN"):
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name) && AlterDomainAction(ref cursor, out var action))
				{
					statement = new Statement.AlterDomain { Name = name, Action = action };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("SEQUENCE"):
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name))
				{
					var options = new List<SequenceOption>();

					while (AlterSequenceGeneratorOption(ref cursor, out var option))
						options.Add(option);

					if (options.Count > 0)
					{
						statement = new Statement.AlterSequence { Name = name, Options = options };

						return true;
					}
				}

				break;
			}

			default:
			{
				if (cursor.IsWord("TYPE"))
				{
					cursor.Take();

					if (Names(ref cursor, 3, out var name) && AlterTypeAction(ref cursor, out var action))
					{
						statement = new Statement.AlterType { Name = name, Action = action };

						return true;
					}

					break;
				}

				if (cursor.IsWord("TRANSFORM") || cursor.IsWord("TRANSFORMS"))
				{
					var plural = cursor.IsWord("TRANSFORMS");

					cursor.Take();

					if (cursor.Take(SqlWord.For) && Names(ref cursor, 3, out var name))
					{
						var groups = new List<TransformAlterGroup>();

						while (AlterGroup(ref cursor, out var group))
							groups.Add(group);

						if (groups.Count > 0)
						{
							statement = new Statement.AlterTransform { PluralKeyword = plural, TypeName = name, Groups = groups };

							return true;
						}
					}

					break;
				}

				// `ALTER <specific routine designator> <characteristics> RESTRICT`.
				if (SpecificRoutineDesignator(ref cursor, out var routine))
				{
					var characteristics = new List<RoutineCharacteristic>();

					while (AlterRoutineCharacteristic(ref cursor, out var characteristic))
						characteristics.Add(characteristic);

					if (characteristics.Count > 0 && cursor.TakeWord("RESTRICT"))
					{
						statement = new Statement.AlterRoutine
						{
							Routine         = routine,
							Characteristics = characteristics,
							Behavior        = AlterRoutineBehavior.Restrict,
						};

						return true;
					}
				}

				break;
			}
		}

		cursor = save;

		return false;
	}

	static bool DropStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		switch (cursor.Word)
		{
			case SqlWord.Name when cursor.IsWord("SCHEMA"):
			{
				cursor.Take();

				if (SchemaName(ref cursor, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropSchema { Names = [name], Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Table:
			{
				cursor.Take();

				if (TableName(ref cursor, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropTable { Names = [name], Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("VIEW"):
			{
				cursor.Take();

				if (TableName(ref cursor, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropView { Names = [name], Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("DOMAIN"):
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropDomain { Name = name, Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Character:
			{
				cursor.Take();

				if (cursor.Take(SqlWord.Set) && CharacterSetSpecification(ref cursor, out var name))
				{
					statement = new Statement.DropCharacterSet { Name = name! };

					return true;
				}

				break;
			}

			case SqlWord.Translation:
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name))
				{
					statement = new Statement.DropTranslation { Name = name };

					return true;
				}

				break;
			}

			case SqlWord.Trigger:
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name))
				{
					statement = new Statement.DropTrigger { Names = [name] };

					return true;
				}

				break;
			}

			case SqlWord.Cast:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && DataType(ref cursor, out var source) && cursor.Take(SqlWord.As) &&
					DataType(ref cursor, out var target) && cursor.Take(SqlTokenKind.RightParen) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropCast { SourceType = source, TargetType = target, Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("ORDERING"):
			{
				cursor.Take();

				if (cursor.Take(SqlWord.For) && Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropOrdering { TypeName = name, Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("SEQUENCE"):
			{
				cursor.Take();

				if (Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropSequence { Names = [name], Behavior = behaviour };

					return true;
				}

				break;
			}

			case SqlWord.Name when cursor.IsWord("ROLE"):
			{
				cursor.Take();

				if (Identifier(ref cursor, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					statement = new Statement.DropRole { Names = [name], Behavior = behaviour };

					return true;
				}

				break;
			}

			default:
			{
				if (cursor.IsWord("ASSERTION"))
				{
					cursor.Take();

					if (Names(ref cursor, 3, out var name))
					{
						statement = new Statement.DropAssertion { Name = name, Behavior = DropBehavior(ref cursor) };

						return true;
					}

					break;
				}

				if (cursor.IsWord("COLLATION"))
				{
					cursor.Take();

					if (Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
					{
						statement = new Statement.DropCollation { Name = new CollationName(name), Behavior = behaviour };

						return true;
					}

					break;
				}

				if (cursor.IsWord("TYPE"))
				{
					cursor.Take();

					if (Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
					{
						statement = new Statement.DropType { Names = [name], Behavior = behaviour };

						return true;
					}

					break;
				}

				if (cursor.IsWord("TRANSFORM") || cursor.IsWord("TRANSFORMS"))
				{
					var plural = cursor.IsWord("TRANSFORMS");

					cursor.Take();

					TransformDropTarget? target =
						cursor.Take(SqlWord.All) ? new TransformDropTarget.All() :
						Identifier(ref cursor, out var group) ? new TransformDropTarget.Group(group) :
						null;

					if (target is not null && cursor.Take(SqlWord.For) && Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
					{
						statement = new Statement.DropTransform
						{
							PluralKeyword = plural,
							Target        = target,
							TypeName      = name,
							Behavior      = behaviour,
						};

						return true;
					}

					break;
				}

				// `DROP <specific routine designator> <drop behavior>`.
				if (SpecificRoutineDesignator(ref cursor, out var routine) && DropBehavior(ref cursor) is { } dropped)
				{
					statement = new Statement.DropRoutine { Routines = [routine], Behavior = dropped };

					return true;
				}

				break;
			}
		}

		cursor = save;

		return false;
	}

	static DropBehavior? DropBehavior(ref SqlCursor cursor)
	{
		return cursor.TakeWord("CASCADE") ? Ast.DropBehavior.Cascade :
		cursor.TakeWord("RESTRICT") ? Ast.DropBehavior.Restrict :
		null;
	}

	// ── §11.10 Alter table ─────────────────────────────────────────────────────

	static bool AlterTableAction(ref SqlCursor cursor, out AlterTableAction action)
	{
		var save = cursor;

		action = null!;

		if (cursor.TakeWord("ADD"))
		{
			// `ADD [COLUMN] c …`, a constraint, a period, or system versioning.
			if (cursor.TakeWord("SYSTEM") && cursor.Take(SqlWord.Versioning))
			{
				action = new AlterTableAction.AddSystemVersioning();

				return true;
			}

			cursor = save;
			cursor.Take();

			if (cursor.Word == SqlWord.Period)
			{
				if (TablePeriodDefinition(ref cursor, out var period))
				{
					var added = new List<AlterTableAction.AddColumn>();
					var marked = cursor;

					while (added.Count < 2 && cursor.TakeWord("ADD"))
					{
						var keyword = cursor.Take(SqlWord.Column);

						if (!ColumnDefinition(ref cursor, out var column))
						{
							cursor = marked;
							added.Clear();

							break;
						}

						added.Add(new AlterTableAction.AddColumn(column, keyword));
						marked = cursor;
					}

					if (added.Count == 1)
					{
						cursor = marked;
						added.Clear();
					}

					action = new AlterTableAction.AddPeriod(period, added);

					return true;
				}

				cursor = save;

				return false;
			}

			var constraint = cursor;

			if (TableConstraintDefinition(ref cursor, out var written))
			{
				action = new AlterTableAction.AddConstraint(written);

				return true;
			}

			cursor = constraint;

			var column_ = cursor.Take(SqlWord.Column);

			if (ColumnDefinition(ref cursor, out var definition))
			{
				action = new AlterTableAction.AddColumn(definition, column_);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Alter))
		{
			if (cursor.Take(SqlWord.Constraint))
			{
				if (Names(ref cursor, 3, out var name) && ConstraintEnforcement(ref cursor) is { } enforced)
				{
					action = new AlterTableAction.AlterConstraint(name, enforced);

					return true;
				}

				cursor = save;

				return false;
			}

			var keyword = cursor.Take(SqlWord.Column);

			if (Identifier(ref cursor, out var column) && AlterColumnAction(ref cursor, out var inner))
			{
				action = new AlterTableAction.AlterColumn(column, inner, keyword);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Drop))
		{
			if (cursor.TakeWord("SYSTEM"))
			{
				if (cursor.Take(SqlWord.Versioning) && DropBehavior(ref cursor) is { } versioning)
				{
					action = new AlterTableAction.DropSystemVersioning(versioning);

					return true;
				}

				cursor = save;

				return false;
			}

			if (cursor.Take(SqlWord.Constraint))
			{
				if (Names(ref cursor, 3, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					action = new AlterTableAction.DropConstraint(name, behaviour);

					return true;
				}

				cursor = save;

				return false;
			}

			if (cursor.Word == SqlWord.Period)
			{
				if (PeriodForSpecification(ref cursor, out var kind, out var name) && DropBehavior(ref cursor) is { } behaviour)
				{
					action = new AlterTableAction.DropPeriod(kind, name, behaviour);

					return true;
				}

				cursor = save;

				return false;
			}

			var keyword = cursor.Take(SqlWord.Column);

			if (Identifier(ref cursor, out var column) && DropBehavior(ref cursor) is { } dropped)
			{
				action = new AlterTableAction.DropColumn(column, dropped, keyword);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool AlterColumnAction(ref SqlCursor cursor, out AlterColumnAction action)
	{
		var save = cursor;

		action = null!;

		if (cursor.Take(SqlWord.Set))
		{
			if (cursor.Take(SqlWord.Not))
			{
				if (cursor.Take(SqlWord.Null))
				{
					action = new AlterColumnAction.SetNotNull();

					return true;
				}

				cursor = save;

				return false;
			}

			if (cursor.TakeWord("DATA"))
			{
				if (cursor.TakeWord("TYPE") && DataType(ref cursor, out var type))
				{
					action = new AlterColumnAction.SetDataType(type);

					return true;
				}

				cursor = save;

				return false;
			}

			if (cursor.IsWord("GENERATED"))
			{
				cursor.Take();

				var always = cursor.IsWord("ALWAYS");

				if (always)
					cursor.Take();

				if (always || cursor.Take(SqlWord.By) && cursor.Take(SqlWord.Default))
				{
					var options = new List<SequenceOption>();

					while (AlterIdentityColumnOption(ref cursor, out var option))
						options.Add(option);

					action = new AlterColumnAction.SetIdentityGeneration(always ? IdentityGeneration.Always : IdentityGeneration.ByDefault, options);

					return true;
				}

				cursor = save;

				return false;
			}

			cursor = save;

			// `SET DEFAULT …`, whose `SET` is the action's and whose default is a clause.
			if (DefaultClauseAfterSet(ref cursor) is { } value)
			{
				action = new AlterColumnAction.SetDefault(value);

				return true;
			}

			cursor = save;
		}

		if (cursor.Take(SqlWord.Drop))
		{
			if (cursor.Take(SqlWord.Default))
			{
				action = new AlterColumnAction.DropDefault();

				return true;
			}

			if (cursor.Take(SqlWord.Not) && cursor.Take(SqlWord.Null))
			{
				action = new AlterColumnAction.DropNotNull();

				return true;
			}

			cursor = save;
			cursor.Take();

			if (cursor.Take(SqlWord.Scope))
			{
				if (DropBehavior(ref cursor) is { } behaviour)
				{
					action = new AlterColumnAction.DropScope(behaviour);

					return true;
				}

				cursor = save;

				return false;
			}

			if (cursor.Take(SqlWord.Identity))
			{
				action = new AlterColumnAction.DropIdentity();

				return true;
			}

			if (cursor.TakeWord("EXPRESSION"))
			{
				action = new AlterColumnAction.DropExpression();

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("ADD"))
		{
			if (cursor.Take(SqlWord.Scope) && TableName(ref cursor, out var table))
			{
				action = new AlterColumnAction.AddScope(table);

				return true;
			}

			cursor = save;

			return false;
		}

		// The identity options on their own.
		var options_ = new List<SequenceOption>();

		while (AlterIdentityColumnOption(ref cursor, out var written))
			options_.Add(written);

		if (options_.Count > 0)
		{
			action = new AlterColumnAction.IdentityOptions(options_);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool AlterIdentityColumnOption(ref SqlCursor cursor, out SequenceOption option)
	{
		var save = cursor;

		option = null!;

		if (cursor.TakeWord("RESTART"))
		{
			Expression? value = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.With))
			{
				if (SignedNumericLiteral(ref cursor, out var literal))
					value = new Expression.Literal(literal);
				else
					cursor = marked;
			}

			option = new SequenceOption.Restart(value);

			return true;
		}

		if (cursor.Take(SqlWord.Set))
		{
			if (BasicSequenceGeneratorOption(ref cursor, out option))
				return true;

			cursor = save;
		}

		return false;
	}

	static bool AlterDomainAction(ref SqlCursor cursor, out AlterDomainAction action)
	{
		var save = cursor;

		action = null!;

		if (cursor.Word == SqlWord.Set)
		{
			if (DefaultClauseAfterSet(ref cursor) is { } value)
			{
				action = new AlterDomainAction.SetDefault(value);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Drop))
		{
			if (cursor.Take(SqlWord.Default))
			{
				action = new AlterDomainAction.DropDefault();

				return true;
			}

			if (cursor.Take(SqlWord.Constraint) && Names(ref cursor, 3, out var name))
			{
				action = new AlterDomainAction.DropConstraint(name);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("ADD") && DomainConstraint(ref cursor, out var constraint))
		{
			action = new AlterDomainAction.AddConstraint(constraint);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary><c>SET DEFAULT …</c>, where the <c>SET</c> is the action's and the default a clause.</summary>
	static Expression? DefaultClauseAfterSet(ref SqlCursor cursor)
	{
		var save = cursor;

		cursor.Take();

		var value = DefaultClause(ref cursor);

		if (value is null)
			cursor = save;

		return value;
	}

	// ── §11 SQL-invoked routines ───────────────────────────────────────────────

	static bool Routine(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		if (InvokedRoutine(ref cursor, out var definition))
		{
			statement = new Statement.CreateRoutine { Definition = definition };

			return true;
		}

		cursor = save;

		return false;
	}

	static bool InvokedRoutine(ref SqlCursor cursor, out RoutineDefinition definition)
	{
		var save = cursor;

		definition = null!;

		if (cursor.Take(SqlWord.Procedure))
		{
			if (Names(ref cursor, 3, out var name) && SQLParameterDeclarationList(ref cursor, out var parameters))
			{
				var characteristics = RoutineCharacteristics(ref cursor);

				if (RoutineBody(ref cursor, out var body))
				{
					definition = new RoutineDefinition(RoutineKind.Procedure, name, parameters, null, characteristics, body);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Function))
		{
			if (Names(ref cursor, 3, out var name) && SQLParameterDeclarationList(ref cursor, out var parameters) &&
				ReturnsClause(ref cursor, out var returns))
			{
				var characteristics = RoutineCharacteristics(ref cursor);
				var dispatch        = cursor;
				var staticDispatch  = cursor.Take(SqlWord.Static) && cursor.TakeWord("DISPATCH");

				if (!staticDispatch)
					cursor = dispatch;

				if (RoutineBody(ref cursor, out var body))
				{
					definition = new RoutineDefinition(RoutineKind.Function, name, parameters, returns, characteristics, body, StaticDispatch: staticDispatch);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		// A method: `SPECIFIC METHOD n`, or `[INSTANCE|STATIC|CONSTRUCTOR] METHOD m (…) [RETURNS t] FOR type`.
		if (cursor.Take(SqlWord.Specific))
		{
			if (cursor.Take(SqlWord.Method) && Names(ref cursor, 3, out var name) && RoutineBody(ref cursor, out var body))
			{
				definition = new RoutineDefinition(RoutineKind.Method, name, [], null, [], body, SpecificMethod: true);

				return true;
			}

			cursor = save;

			return false;
		}

		var modifier = MethodModifier(ref cursor);

		if (cursor.Take(SqlWord.Method) && Identifier(ref cursor, out var method) && SQLParameterDeclarationList(ref cursor, out var arguments))
		{
			ReturnsDefinition? returns = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.Returns))
			{
				cursor = marked;

				if (!ReturnsClause(ref cursor, out returns))
				{
					cursor = save;

					return false;
				}
			}

			if (cursor.Take(SqlWord.For) && Names(ref cursor, 3, out var type) && RoutineBody(ref cursor, out var body))
			{
				definition = new RoutineDefinition(RoutineKind.Method, new QualifiedName([method]), arguments, returns, [], body, modifier, type);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static MethodModifier? MethodModifier(ref SqlCursor cursor)
	{
		return cursor.TakeWord("INSTANCE") ? Ast.MethodModifier.Instance :
		cursor.Take(SqlWord.Static) ? Ast.MethodModifier.Static :
		cursor.TakeWord("CONSTRUCTOR") ? Ast.MethodModifier.Constructor :
		null;
	}

	static bool SQLParameterDeclarationList(ref SqlCursor cursor, out IReadOnlyList<ParameterDefinition> parameters)
	{
		var save    = cursor;
		var written = new List<ParameterDefinition>();

		parameters = written;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return false;

		if (cursor.Take(SqlTokenKind.RightParen))
			return true;

		while (true)
		{
			if (!SQLParameterDeclaration(ref cursor, out var parameter))
			{
				cursor = save;

				return false;
			}

			written.Add(parameter);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return true;

		cursor = save;

		return false;
	}

	/// <summary>
	/// A parameter's name is asked before its type, which a user-defined type's name could be:
	/// <c>(a b)</c> is a parameter <c>a</c> of type <c>b</c>, and <c>(a)</c> one of type <c>a</c>.
	/// </summary>
	static bool SQLParameterDeclaration(ref SqlCursor cursor, out ParameterDefinition parameter)
	{
		var save = cursor;

		parameter = null!;

		var mode =
			cursor.Take(SqlWord.In)    ? ParameterMode.In :
			cursor.Take(SqlWord.Out)   ? ParameterMode.Out :
			cursor.Take(SqlWord.Inout) ? ParameterMode.InOut :
			(ParameterMode?)null;

		Identifier? name = null;

		var named = cursor;

		if (Identifier(ref cursor, out var written))
		{
			if (ParameterType(ref cursor, out var typed, out var located))
			{
				name = written;

				return Parameter(ref cursor, mode, name, typed, located, out parameter);
			}

			cursor = named;
		}

		if (ParameterType(ref cursor, out var type, out var locator))
			return Parameter(ref cursor, mode, null, type, locator, out parameter);

		cursor = save;

		return false;
	}

	static bool Parameter(ref SqlCursor cursor, ParameterMode? mode, Identifier? name, DataType type, bool locator, out ParameterDefinition parameter)
	{
		var result = cursor.TakeWord("RESULT");

		Expression? defaults = null;

		var save = cursor;

		if (cursor.Take(SqlWord.Default) && !ParameterDefault(ref cursor, out defaults))
		{
			cursor   = save;
			defaults = null;
		}

		parameter = new ParameterDefinition(mode, name, type, result, defaults, locator);

		return true;
	}

	static bool ParameterType(ref SqlCursor cursor, out DataType type, out bool locator)
	{
		var save = cursor;

		locator = false;
		type    = null!;

		// A data type is asked first, as the BNF asks it: `DESCRIPTOR` is no reserved word, and a
		// user-defined type may be named so.
		if (DataType(ref cursor, out type))
		{
			locator = LocatorIndication(ref cursor);

			return true;
		}

		if (cursor.Take(SqlWord.Table))
		{
			var through =
				cursor.TakeWord("PASS") && cursor.TakeWord("THROUGH") ? PassThroughMode.PassThrough :
				(PassThroughMode?)null;

			if (through is null)
			{
				cursor = save;
				cursor.Take();

				var marked = cursor;

				if (cursor.Take(SqlWord.No) && cursor.TakeWord("PASS") && cursor.TakeWord("THROUGH"))
					through = PassThroughMode.NoPassThrough;
				else
					cursor = marked;
			}

			TableSemantics? semantics = null;
			TablePruning? pruning = null;

			var written = cursor;

			if (cursor.Take(SqlWord.With))
			{
				if (cursor.Take(SqlWord.Row) && cursor.TakeWord("SEMANTICS"))
				{
					semantics = TableSemantics.Row;
				}
				else
				{
					cursor = written;

					if (cursor.Take(SqlWord.With) && cursor.Take(SqlWord.Set) && cursor.TakeWord("SEMANTICS"))
					{
						semantics = TableSemantics.Set;

						var pruned = cursor;

						if (cursor.TakeWord("PRUNE") && cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Empty))
							pruning = TablePruning.PruneOnEmpty;
						else
						{
							cursor = pruned;

							if (cursor.TakeWord("KEEP") && cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Empty))
								pruning = TablePruning.KeepOnEmpty;
							else
								cursor = pruned;
						}
					}
					else
					{
						cursor = written;
					}
				}
			}

			type = new DataType.GenericTable(through, semantics, pruning);

			return true;
		}

		if (cursor.TakeWord("DESCRIPTOR"))
		{
			type = new DataType.Descriptor();

			return true;
		}

		cursor = save;

		return false;
	}

	static bool LocatorIndication(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.As) && cursor.TakeWord("LOCATOR"))
			return true;

		cursor = save;

		return false;
	}

	static bool ParameterDefault(ref SqlCursor cursor, out Expression value)
	{
		// A descriptor is asked first: `DESCRIPTOR` is no reserved word, and a value expression
		// would read it as a column and end the parameter list.
		if (DescriptorValueConstructor(ref cursor, out value))
			return true;

		if (ValueExpression(ref cursor, out var read))
		{
			value = read.Node;

			return true;
		}

		return ContextuallyTypedValueSpecification(ref cursor, out value);
	}

	static bool DescriptorValueConstructor(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		if (!cursor.TakeWord("DESCRIPTOR") || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var columns = new List<DescriptorColumn>();

		while (true)
		{
			if (!Identifier(ref cursor, out var name))
				break;

			DataType? type = null;

			var marked = cursor;

			if (!DataType(ref cursor, out type!))
			{
				cursor = marked;
				type   = null;
			}

			columns.Add(new DescriptorColumn(name, type));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (columns.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.DescriptorConstructor(columns);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool ReturnsClause(ref SqlCursor cursor, out ReturnsDefinition returns)
	{
		var save = cursor;

		returns = null!;

		if (!cursor.Take(SqlWord.Returns))
			return false;

		if (cursor.Take(SqlWord.Table))
		{
			IReadOnlyList<FieldDefinition>? columns = null;

			var bracket = cursor;

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var written = new List<FieldDefinition>();

				while (true)
				{
					if (!Identifier(ref cursor, out var name) || !DataType(ref cursor, out var type))
						break;

					written.Add(new FieldDefinition(name, type));

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (written.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					columns = written;
				else
					cursor = bracket;
			}

			returns = new ReturnsDefinition(null, columns) { TableKeyword = true };

			return true;
		}

		if (cursor.Take(SqlWord.Only))
		{
			if (cursor.TakeWord("PASS") && cursor.TakeWord("THROUGH"))
			{
				returns = new ReturnsDefinition(null, null, true);

				return true;
			}

			cursor = save;

			return false;
		}

		if (DataType(ref cursor, out var type_))
		{
			var locator = LocatorIndication(ref cursor);

			DataType? castFrom = null;
			var castLocator = false;

			var marked = cursor;

			if (cursor.Take(SqlWord.Cast))
			{
				if (cursor.Take(SqlWord.From) && DataType(ref cursor, out castFrom!))
					castLocator = LocatorIndication(ref cursor);
				else
				{
					cursor   = marked;
					castFrom = null;
				}
			}

			returns = new ReturnsDefinition(type_, null) { Locator = locator, CastFrom = castFrom, CastFromLocator = castLocator };

			return true;
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<RoutineCharacteristic> RoutineCharacteristics(ref SqlCursor cursor)
	{
		var characteristics = new List<RoutineCharacteristic>();

		while (RoutineCharacteristic(ref cursor, out var characteristic))
			characteristics.Add(characteristic);

		return characteristics;
	}

	static bool RoutineCharacteristic(ref SqlCursor cursor, out RoutineCharacteristic characteristic)
	{
		var save = cursor;

		characteristic = null!;

		if (LanguageClause(ref cursor, out characteristic))
			return true;

		if (ParameterStyleClause(ref cursor) is { } style)
		{
			characteristic = new RoutineCharacteristic.ParameterStyle(style);

			return true;
		}

		if (cursor.Take(SqlWord.Specific))
		{
			if (Names(ref cursor, 3, out var name))
			{
				characteristic = new RoutineCharacteristic.Specific(name);

				return true;
			}

			cursor = save;

			return false;
		}

		var not = cursor.Take(SqlWord.Not);

		if (cursor.Take(SqlWord.Deterministic))
		{
			characteristic = new RoutineCharacteristic.Deterministic(!not);

			return true;
		}

		if (not)
		{
			cursor = save;

			return false;
		}

		if (SQLDataAccessIndication(ref cursor) is { } access)
		{
			characteristic = new RoutineCharacteristic.DataAccess(access);

			return true;
		}

		if (NullCallClause(ref cursor) is { } nulls)
		{
			characteristic = new RoutineCharacteristic.NullCall(nulls);

			return true;
		}

		if (ReturnedResultSets(ref cursor) is { } sets)
		{
			characteristic = new RoutineCharacteristic.DynamicResultSets(sets);

			return true;
		}

		var level = cursor;
		var over  = cursor.Take(SqlWord.New);

		if ((over || cursor.Take(SqlWord.Old)) && cursor.TakeWord("SAVEPOINT") && cursor.TakeWord("LEVEL"))
		{
			characteristic = new RoutineCharacteristic.SavepointLevel(over ? SavepointLevelKind.New : SavepointLevelKind.Old);

			return true;
		}

		cursor = level;

		return false;
	}

	static bool LanguageClause(ref SqlCursor cursor, out RoutineCharacteristic characteristic)
	{
		var save = cursor;

		characteristic = null!;

		if (!cursor.Take(SqlWord.Language))
			return false;

		foreach (var name in new[] { "ADA", "COBOL", "C", "FORTRAN", "MUMPS", "M", "PASCAL", "PLI", "SQL" })
		{
			if (!cursor.IsWord(name))
				continue;

			var written = cursor.TextOf(cursor.Token);

			cursor.Take();

			characteristic = new RoutineCharacteristic.Language(written);

			return true;
		}

		cursor = save;

		return false;
	}

	static ParameterStyleKind? ParameterStyleClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Parameter) && cursor.TakeWord("STYLE"))
		{
			if (cursor.Take(SqlWord.Sql))
				return ParameterStyleKind.Sql;

			if (cursor.TakeWord("GENERAL"))
				return ParameterStyleKind.General;
		}

		cursor = save;

		return null;
	}

	static SqlDataAccess? SQLDataAccessIndication(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.No))
		{
			if (cursor.Take(SqlWord.Sql))
				return SqlDataAccess.NoSql;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.Contains))
		{
			if (cursor.Take(SqlWord.Sql))
				return SqlDataAccess.ContainsSql;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.Reads))
		{
			if (cursor.Take(SqlWord.Sql) && cursor.TakeWord("DATA"))
				return SqlDataAccess.ReadsSqlData;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.Modifies))
		{
			if (cursor.Take(SqlWord.Sql) && cursor.TakeWord("DATA"))
				return SqlDataAccess.ModifiesSqlData;

			cursor = save;
		}

		return null;
	}

	static NullCallMode? NullCallClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Returns))
		{
			if (cursor.Take(SqlWord.Null) && cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Null) && cursor.TakeWord("INPUT"))
				return NullCallMode.ReturnsNullOnNullInput;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.Called))
		{
			if (cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Null) && cursor.TakeWord("INPUT"))
				return NullCallMode.CalledOnNullInput;

			cursor = save;
		}

		return null;
	}

	static int? ReturnedResultSets(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Dynamic) && cursor.Take(SqlWord.Result) && cursor.TakeWord("SETS") && Unsigned(ref cursor, out var count))
			return count;

		cursor = save;

		return null;
	}

	static bool RoutineBody(ref SqlCursor cursor, out RoutineBody body)
	{
		var save = cursor;

		body = null!;

		if (cursor.Take(SqlWord.External))
		{
			string? name = null;

			var marked = cursor;

			if (cursor.TakeWord("NAME"))
			{
				if (cursor.Kind == SqlTokenKind.String)
				{
					name = cursor.TextOf(cursor.Token);

					cursor.Take();
				}
				else if (Identifier(ref cursor, out var written))
				{
					name = written.Text;
				}
				else
				{
					cursor = marked;
				}
			}

			var style = ParameterStyleClause(ref cursor);
			var group = TransformGroupSpecification(ref cursor);

			ExternalSecurity? security = null;

			var secured = cursor;

			if (cursor.Take(SqlWord.External) && cursor.TakeWord("SECURITY"))
			{
				security =
					cursor.TakeWord("DEFINER") ? ExternalSecurity.Definer :
					cursor.TakeWord("INVOKER") ? ExternalSecurity.Invoker :
					cursor.TakeWord("IMPLEMENTATION") && cursor.TakeWord("DEFINED") ? ExternalSecurity.ImplementationDefined :
					(ExternalSecurity?)null;

				if (security is null)
					cursor = secured;
			}
			else
			{
				cursor = secured;
			}

			body = new RoutineBody.External(name, style, group, security);

			return true;
		}

		if (PolymorphicTableFunctionBody(ref cursor, out var polymorphic))
		{
			body = new RoutineBody.PolymorphicTableFunction(polymorphic);

			return true;
		}

		SqlSecurity? rights = null;

		var security_ = cursor;

		if (cursor.Take(SqlWord.Sql) && cursor.TakeWord("SECURITY"))
		{
			rights =
				cursor.TakeWord("INVOKER") ? SqlSecurity.Invoker :
				cursor.TakeWord("DEFINER") ? SqlSecurity.Definer :
				(SqlSecurity?)null;

			if (rights is null)
				cursor = security_;
		}
		else
		{
			cursor = security_;
		}

		if (SQLExecutableStatement(ref cursor, out var statement))
		{
			body = new RoutineBody.Sql(statement, rights);

			return true;
		}

		cursor = save;

		return false;
	}

	static TransformGroupSpecification? TransformGroupSpecification(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("TRANSFORM") || !cursor.Take(SqlWord.Group))
		{
			cursor = save;

			return null;
		}

		// The groups for types are asked before a single group, whose name begins them.
		var groups = new List<TransformGroupForType>();
		var marked = cursor;

		while (true)
		{
			if (!Identifier(ref cursor, out var group) || !cursor.Take(SqlWord.For) || !cursor.TakeWord("TYPE") ||
				!Names(ref cursor, 3, out var type))
			{
				cursor = marked;

				break;
			}

			groups.Add(new TransformGroupForType(group, type));

			marked = cursor;

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (groups.Count > 0)
			return new Ast.TransformGroupSpecification(null, groups);

		if (Identifier(ref cursor, out var single))
			return new Ast.TransformGroupSpecification(single, []);

		cursor = save;

		return null;
	}

	static bool PolymorphicTableFunctionBody(ref SqlCursor cursor, out PolymorphicTableFunctionBody body)
	{
		var save = cursor;

		body = null!;

		IReadOnlyList<ParameterDefinition>? parameters = null;
		var dataKeyword = false;

		if (cursor.TakeWord("PRIVATE"))
		{
			dataKeyword = cursor.TakeWord("DATA");

			if (!SQLParameterDeclarationList(ref cursor, out parameters!))
			{
				cursor = save;

				return false;
			}
		}

		var describe = With(ref cursor, "DESCRIBE");
		var start    = With(ref cursor, "START");

		if (!cursor.TakeWord("FULFILL") || !cursor.Take(SqlWord.With) || !SpecificRoutineDesignator(ref cursor, out var fulfill))
		{
			cursor = save;

			return false;
		}

		var finish = With(ref cursor, "FINISH");

		body = new Ast.PolymorphicTableFunctionBody(parameters, describe, start, fulfill, finish, dataKeyword);

		return true;

		static RoutineDesignator? With(ref SqlCursor cursor, string word)
		{
			var save = cursor;

			if (cursor.TakeWord(word) && cursor.Take(SqlWord.With) && SpecificRoutineDesignator(ref cursor, out var routine))
				return routine;

			cursor = save;

			return null;
		}
	}

	static bool AlterRoutineCharacteristic(ref SqlCursor cursor, out RoutineCharacteristic characteristic)
	{
		var save = cursor;

		characteristic = null!;

		if (LanguageClause(ref cursor, out characteristic))
			return true;

		if (ParameterStyleClause(ref cursor) is { } style)
		{
			characteristic = new RoutineCharacteristic.ParameterStyle(style);

			return true;
		}

		if (SQLDataAccessIndication(ref cursor) is { } access)
		{
			characteristic = new RoutineCharacteristic.DataAccess(access);

			return true;
		}

		if (NullCallClause(ref cursor) is { } nulls)
		{
			characteristic = new RoutineCharacteristic.NullCall(nulls);

			return true;
		}

		if (ReturnedResultSets(ref cursor) is { } sets)
		{
			characteristic = new RoutineCharacteristic.DynamicResultSets(sets);

			return true;
		}

		if (cursor.TakeWord("NAME"))
		{
			if (cursor.Kind == SqlTokenKind.String)
			{
				var text = cursor.TextOf(cursor.Token);

				cursor.Take();

				characteristic = new RoutineCharacteristic.ExternalName(text);

				return true;
			}

			if (Identifier(ref cursor, out var name))
			{
				characteristic = new RoutineCharacteristic.ExternalName(name.Text);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	/// <summary>A routine by its specific name, or by its name and its parameters' types.</summary>
	static bool SpecificRoutineDesignator(ref SqlCursor cursor, out RoutineDesignator designator)
	{
		var save = cursor;

		designator = null!;

		if (cursor.Take(SqlWord.Specific))
		{
			if (RoutineType(ref cursor, out var kind, out var modifier) && Names(ref cursor, 3, out var name))
			{
				designator = new RoutineDesignator(kind, name, null, null, true, modifier);

				return true;
			}

			cursor = save;

			return false;
		}

		if (RoutineType(ref cursor, out var routine, out var method) && Names(ref cursor, 3, out var named))
		{
			IReadOnlyList<DataType>? types = null;

			var bracket = cursor;

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var written = new List<DataType>();

				while (true)
				{
					if (!DataType(ref cursor, out var type))
						break;

					written.Add(type);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (cursor.Take(SqlTokenKind.RightParen))
					types = written;
				else
					cursor = bracket;
			}

			QualifiedName? forType = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.For) && !Names(ref cursor, 3, out forType!))
			{
				cursor  = marked;
				forType = null;
			}

			designator = new RoutineDesignator(routine, named, types, forType, false, method);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool RoutineType(ref SqlCursor cursor, out RoutineKind kind, out MethodModifier? modifier)
	{
		modifier = null;

		if (cursor.IsWord("ROUTINE"))
		{
			cursor.Take();

			kind = RoutineKind.Routine;

			return true;
		}

		if (cursor.Take(SqlWord.Function))
		{
			kind = RoutineKind.Function;

			return true;
		}

		if (cursor.Take(SqlWord.Procedure))
		{
			kind = RoutineKind.Procedure;

			return true;
		}

		var save = cursor;

		modifier = MethodModifier(ref cursor);

		if (cursor.Take(SqlWord.Method))
		{
			kind = RoutineKind.Method;

			return true;
		}

		cursor   = save;
		modifier = null;
		kind     = default;

		return false;
	}

	// ── §11 User-defined types ─────────────────────────────────────────────────

	static bool UserDefinedType(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		QualifiedName? under = null;

		var marked = cursor;

		if (cursor.TakeWord("UNDER") && !Names(ref cursor, 3, out under!))
		{
			cursor = marked;
			under  = null;
		}

		TypeRepresentation? representation = null;

		var asKeyword = cursor;

		if (cursor.Take(SqlWord.As))
		{
			if (!Representation(ref cursor, out representation))
			{
				cursor         = asKeyword;
				representation = null;
			}
		}

		var options = new List<UserTypeOption>();

		while (UserDefinedTypeOption(ref cursor, out var option))
			options.Add(option);

		var methods = new List<MethodSpecification>();
		var listed  = cursor;

		while (true)
		{
			if (!MethodSpecification(ref cursor, out var method))
			{
				cursor = listed;

				break;
			}

			methods.Add(method);
			listed = cursor;

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		statement = new Statement.CreateType
		{
			Definition = new UserDefinedTypeDefinition(name, under, representation, options, methods),
		};

		return true;
	}

	static bool Representation(ref SqlCursor cursor, out TypeRepresentation representation)
	{
		var save = cursor;

		representation = null!;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			cursor.Take();

			var attributes = new List<AttributeDefinition>();

			while (true)
			{
				if (!AttributeDefinition(ref cursor, out var attribute))
					break;

				attributes.Add(attribute);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (attributes.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
			{
				representation = new TypeRepresentation.Members(attributes);

				return true;
			}

			cursor = save;

			return false;
		}

		if (PredefinedType(ref cursor, out var predefined))
		{
			representation = new TypeRepresentation.Type(Collected(ref cursor, predefined));

			return true;
		}

		// A row, a reference or a user-defined type, which a representation holds only as a
		// collection's element.
		if (RepresentedElementType(ref cursor, out var element))
		{
			var collected = Collected(ref cursor, element);

			if (!ReferenceEquals(collected, element))
			{
				representation = new TypeRepresentation.Type(collected);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static DataType Collected(ref SqlCursor cursor, DataType type)
	{
		while (true)
		{
			if (cursor.Word == SqlWord.Array)
			{
				cursor.Take();

				int? cardinality = null;
				var  trigraphs   = false;

				if (cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
				{
					var save = cursor;

					trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;

					cursor.Take();

					if (!Unsigned(ref cursor, out cardinality) || !(cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph))
					{
						cursor      = save;
						cardinality = null;
						trigraphs   = false;
					}
					else
					{
						cursor.Take();
					}
				}

				type = new DataType.Array(type, cardinality, trigraphs);

				continue;
			}

			if (cursor.Take(SqlWord.Multiset))
			{
				type = new DataType.Multiset(type);

				continue;
			}

			return type;
		}
	}

	static bool RepresentedElementType(ref SqlCursor cursor, out DataType type)
	{
		if (cursor.Word == SqlWord.Row)
			return RowType(ref cursor, out type);

		if (cursor.Word == SqlWord.Ref)
			return ReferenceType(ref cursor, out type);

		if (Names(ref cursor, 3, out var name))
		{
			type = new DataType.UserDefined(name);

			return true;
		}

		type = null!;

		return false;
	}

	static bool AttributeDefinition(ref SqlCursor cursor, out AttributeDefinition attribute)
	{
		var save = cursor;

		attribute = null!;

		if (!Identifier(ref cursor, out var name) || !DataType(ref cursor, out var type))
		{
			cursor = save;

			return false;
		}

		var defaults = DefaultClause(ref cursor);

		CollateClause(ref cursor, out var collation);

		attribute = new Ast.AttributeDefinition(name, type, defaults, collation);

		return true;
	}

	static bool UserDefinedTypeOption(ref SqlCursor cursor, out UserTypeOption option)
	{
		var save = cursor;

		option = null!;

		var not = cursor.Take(SqlWord.Not);

		if (cursor.TakeWord("INSTANTIABLE"))
		{
			option = new UserTypeOption.Instantiable(!not);

			return true;
		}

		if (cursor.TakeWord("FINAL"))
		{
			option = new UserTypeOption.Final(!not);

			return true;
		}

		if (not)
		{
			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Ref))
		{
			if (cursor.Take(SqlWord.Using) && PredefinedType(ref cursor, out var type))
			{
				option = new UserTypeOption.Reference(new UserTypeReference.Using(type));

				return true;
			}

			cursor = save;
			cursor.Take();

			if (cursor.Take(SqlWord.From) && cursor.Take(SqlTokenKind.LeftParen))
			{
				var attributes = new List<Identifier>();

				while (true)
				{
					if (!Identifier(ref cursor, out var name))
						break;

					attributes.Add(name);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (attributes.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					option = new UserTypeOption.Reference(new UserTypeReference.FromAttributes(attributes));

					return true;
				}
			}

			cursor = save;
			cursor.Take();

			if (cursor.Take(SqlWord.Is) && cursor.TakeWord("SYSTEM") && cursor.TakeWord("GENERATED"))
			{
				option = new UserTypeOption.Reference(new UserTypeReference.SystemGenerated());

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Cast))
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && CastOptionKind(ref cursor) is { } kind && cursor.Take(SqlTokenKind.RightParen) &&
				cursor.Take(SqlWord.With) && Identifier(ref cursor, out var function))
			{
				option = new UserTypeOption.Cast(kind, function);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static UserTypeCastKind? CastOptionKind(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("SOURCE") && cursor.Take(SqlWord.As))
		{
			if (cursor.Take(SqlWord.Ref))
				return UserTypeCastKind.ToRef;

			if (cursor.Take(SqlWord.Distinct))
				return UserTypeCastKind.ToDistinct;

			cursor = save;

			return null;
		}

		cursor = save;

		if (cursor.Take(SqlWord.Ref) && cursor.Take(SqlWord.As) && cursor.TakeWord("SOURCE"))
			return UserTypeCastKind.ToType;

		cursor = save;

		if (cursor.Take(SqlWord.Distinct) && cursor.Take(SqlWord.As) && cursor.TakeWord("SOURCE"))
			return UserTypeCastKind.ToSource;

		cursor = save;

		return null;
	}

	static bool MethodSpecification(ref SqlCursor cursor, out MethodSpecification method)
	{
		var save = cursor;

		method = null!;

		if (cursor.TakeWord("OVERRIDING"))
		{
			if (PartialMethodSpecification(ref cursor, out method))
			{
				method = method with { Overriding = true };

				return true;
			}

			cursor = save;

			return false;
		}

		if (!PartialMethodSpecification(ref cursor, out method))
			return false;

		var result  = Self(ref cursor, "RESULT");
		var locator = Self(ref cursor, "LOCATOR");

		var characteristics = new List<RoutineCharacteristic>();

		while (MethodCharacteristic(ref cursor, out var characteristic))
			characteristics.Add(characteristic);

		method = method with { SelfAsResult = result, SelfAsLocator = locator, Characteristics = characteristics };

		return true;

		static bool Self(ref SqlCursor cursor, string word)
		{
			var save = cursor;

			if (cursor.TakeWord("SELF") && cursor.Take(SqlWord.As) && cursor.TakeWord(word))
				return true;

			cursor = save;

			return false;
		}
	}

	static bool PartialMethodSpecification(ref SqlCursor cursor, out MethodSpecification method)
	{
		var save = cursor;

		method = null!;

		var modifier = MethodModifier(ref cursor);

		if (!cursor.Take(SqlWord.Method) || !Identifier(ref cursor, out var name) || !SQLParameterDeclarationList(ref cursor, out var parameters) ||
			!ReturnsClause(ref cursor, out var returns))
		{
			cursor = save;

			return false;
		}

		QualifiedName? specific = null;

		var marked = cursor;

		if (cursor.Take(SqlWord.Specific) && !Names(ref cursor, 3, out specific!))
		{
			cursor   = marked;
			specific = null;
		}

		method = new Ast.MethodSpecification(modifier, name, parameters, returns, specific, false, false, []);

		return true;
	}

	static bool MethodCharacteristic(ref SqlCursor cursor, out RoutineCharacteristic characteristic)
	{
		var save = cursor;

		characteristic = null!;

		if (LanguageClause(ref cursor, out characteristic))
			return true;

		if (ParameterStyleClause(ref cursor) is { } style)
		{
			characteristic = new RoutineCharacteristic.ParameterStyle(style);

			return true;
		}

		var not = cursor.Take(SqlWord.Not);

		if (cursor.Take(SqlWord.Deterministic))
		{
			characteristic = new RoutineCharacteristic.Deterministic(!not);

			return true;
		}

		if (not)
		{
			cursor = save;

			return false;
		}

		if (SQLDataAccessIndication(ref cursor) is { } access)
		{
			characteristic = new RoutineCharacteristic.DataAccess(access);

			return true;
		}

		if (NullCallClause(ref cursor) is { } nulls)
		{
			characteristic = new RoutineCharacteristic.NullCall(nulls);

			return true;
		}

		return false;
	}

	static bool AlterTypeAction(ref SqlCursor cursor, out AlterTypeAction action)
	{
		var save = cursor;

		action = null!;

		if (cursor.TakeWord("ADD"))
		{
			if (cursor.TakeWord("ATTRIBUTE"))
			{
				if (AttributeDefinition(ref cursor, out var attribute))
				{
					action = new AlterTypeAction.AddAttribute(attribute);

					return true;
				}

				cursor = save;

				return false;
			}

			if (MethodSpecification(ref cursor, out var method))
			{
				action = new AlterTypeAction.AddMethod(method, method.Overriding);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Drop))
		{
			if (cursor.TakeWord("ATTRIBUTE"))
			{
				if (Identifier(ref cursor, out var name) && cursor.TakeWord("RESTRICT"))
				{
					action = new AlterTypeAction.DropAttribute(name);

					return true;
				}

				cursor = save;

				return false;
			}

			if (SpecificMethodSpecificationDesignator(ref cursor, out var designator) && cursor.TakeWord("RESTRICT"))
			{
				action = new AlterTypeAction.DropMethod(designator);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool SpecificMethodSpecificationDesignator(ref SqlCursor cursor, out MethodDesignator designator)
	{
		var save = cursor;

		designator = null!;

		var modifier = MethodModifier(ref cursor);

		if (!cursor.Take(SqlWord.Method) || !Identifier(ref cursor, out var name) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var types = new List<DataType>();

		while (true)
		{
			if (!DataType(ref cursor, out var type))
				break;

			types.Add(type);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			designator = new MethodDesignator(modifier, name, types);

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §11 Casts, orderings and transforms ────────────────────────────────────

	static bool Cast(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && DataType(ref cursor, out var source) && cursor.Take(SqlWord.As) &&
			DataType(ref cursor, out var target) && cursor.Take(SqlTokenKind.RightParen) && cursor.Take(SqlWord.With) &&
			SpecificRoutineDesignator(ref cursor, out var function))
		{
			var marked     = cursor;
			var assignment = cursor.Take(SqlWord.As) && cursor.TakeWord("ASSIGNMENT");

			if (!assignment)
				cursor = marked;

			statement = new Statement.CreateCast
			{
				SourceType   = source,
				TargetType   = target,
				Function     = function,
				AsAssignment = assignment,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool Ordering(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!cursor.Take(SqlWord.For) || !Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		var form =
			cursor.Take(SqlWord.Equals) && cursor.Take(SqlWord.Only) ? OrderingForm.EqualsOnly :
			(OrderingForm?)null;

		if (form is null)
		{
			var marked = cursor;

			if (cursor.Take(SqlWord.Order) && cursor.Take(SqlWord.Full))
				form = OrderingForm.Full;
			else
				cursor = marked;
		}

		if (form is null || !cursor.Take(SqlWord.By) || !OrderingCategory(ref cursor, out var category))
		{
			cursor = save;

			return false;
		}

		statement = new Statement.CreateOrdering { TypeName = name, Ordering = new OrderingDefinition(form.Value, category) };

		return true;
	}

	static bool OrderingCategory(ref SqlCursor cursor, out OrderingCategory category)
	{
		var save = cursor;

		category = null!;

		if (cursor.TakeWord("RELATIVE"))
		{
			if (cursor.Take(SqlWord.With) && SpecificRoutineDesignator(ref cursor, out var routine))
			{
				category = new OrderingCategory.Relative(routine);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("MAP"))
		{
			if (cursor.Take(SqlWord.With) && SpecificRoutineDesignator(ref cursor, out var routine))
			{
				category = new OrderingCategory.Map(routine);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("STATE"))
		{
			QualifiedName? name = null;

			var marked = cursor;

			if (!Names(ref cursor, 3, out name!))
			{
				cursor = marked;
				name   = null;
			}

			category = new OrderingCategory.State(name);

			return true;
		}

		return false;
	}

	static bool Transform(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		var plural = cursor.IsWord("TRANSFORMS");

		cursor.Take();

		if (!cursor.Take(SqlWord.For) || !Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		var groups = new List<TransformGroup>();

		while (TransformGroup(ref cursor, out var group))
			groups.Add(group);

		if (groups.Count == 0)
		{
			cursor = save;

			return false;
		}

		statement = new Statement.CreateTransform { PluralKeyword = plural, TypeName = name, Groups = groups };

		return true;
	}

	static bool TransformGroup(ref SqlCursor cursor, out TransformGroup group)
	{
		var save = cursor;

		group = null!;

		if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var elements = TransformElementList(ref cursor);

		if (elements is not null && cursor.Take(SqlTokenKind.RightParen))
		{
			group = new Ast.TransformGroup(name, elements);

			return true;
		}

		cursor = save;

		return false;
	}

	static IReadOnlyList<TransformElement>? TransformElementList(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!TransformElement(ref cursor, out var first))
			return null;

		var elements = new List<TransformElement> { first };

		var marked = cursor;

		if (cursor.Take(SqlTokenKind.Comma))
		{
			if (TransformElement(ref cursor, out var second))
				elements.Add(second);
			else
				cursor = marked;
		}

		return elements;
	}

	static bool TransformElement(ref SqlCursor cursor, out TransformElement element)
	{
		var save = cursor;

		element = null!;

		var direction =
			cursor.Take(SqlWord.To) && cursor.Take(SqlWord.Sql) ? TransformDirection.ToSql :
			(TransformDirection?)null;

		if (direction is null)
		{
			cursor = save;

			if (cursor.Take(SqlWord.From) && cursor.Take(SqlWord.Sql))
				direction = TransformDirection.FromSql;
		}

		if (direction is not null && cursor.Take(SqlWord.With) && SpecificRoutineDesignator(ref cursor, out var routine))
		{
			element = new Ast.TransformElement(direction.Value, routine);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool AlterGroup(ref SqlCursor cursor, out TransformAlterGroup group)
	{
		var save = cursor;

		group = null!;

		if (!Identifier(ref cursor, out var name) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var actions = new List<TransformAlterAction>();

		while (true)
		{
			if (!AlterTransformAction(ref cursor, out var action))
				break;

			actions.Add(action);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (actions.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
		{
			group = new TransformAlterGroup(name, actions);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool AlterTransformAction(ref SqlCursor cursor, out TransformAlterAction action)
	{
		var save = cursor;

		action = null!;

		if (cursor.TakeWord("ADD"))
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && TransformElementList(ref cursor) is { } elements && cursor.Take(SqlTokenKind.RightParen))
			{
				action = new TransformAlterAction(true, elements, [], null);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Drop) && cursor.Take(SqlTokenKind.LeftParen))
		{
			var kinds = new List<TransformDirection>();

			if (TransformKind(ref cursor, out var first))
			{
				kinds.Add(first);

				var marked = cursor;

				if (cursor.Take(SqlTokenKind.Comma))
				{
					if (TransformKind(ref cursor, out var second))
						kinds.Add(second);
					else
						cursor = marked;
				}

				if (DropBehavior(ref cursor) is { } behaviour && cursor.Take(SqlTokenKind.RightParen))
				{
					action = new TransformAlterAction(false, [], kinds, behaviour);

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}

	static bool TransformKind(ref SqlCursor cursor, out TransformDirection direction)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.To) && cursor.Take(SqlWord.Sql))
		{
			direction = TransformDirection.ToSql;

			return true;
		}

		cursor = save;

		if (cursor.Take(SqlWord.From) && cursor.Take(SqlWord.Sql))
		{
			direction = TransformDirection.FromSql;

			return true;
		}

		cursor    = save;
		direction = default;

		return false;
	}

	// ── §11 Character sets, collations and transliterations ────────────────────

	static bool CharacterSet(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!cursor.Take(SqlWord.Set) || !CharacterSetSpecification(ref cursor, out var name))
		{
			cursor = save;

			return false;
		}

		var keyword = cursor.Take(SqlWord.As);

		if (cursor.Take(SqlWord.Get) && CharacterSetSpecification(ref cursor, out var source))
		{
			CollateClause(ref cursor, out var collation);

			statement = new Statement.CreateCharacterSet
			{
				Name      = name!,
				AsKeyword = keyword,
				Source    = source!,
				Collation = collation,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool Collation(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (Names(ref cursor, 3, out var name) && cursor.Take(SqlWord.For) && CharacterSetSpecification(ref cursor, out var set) &&
			cursor.Take(SqlWord.From) && Names(ref cursor, 3, out var source))
		{
			PadCharacteristic? padding = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.No))
			{
				if (cursor.TakeWord("PAD"))
					padding = PadCharacteristic.NoPad;
				else
					cursor = marked;
			}
			else if (cursor.TakeWord("PAD"))
			{
				if (cursor.TakeWord("SPACE"))
					padding = PadCharacteristic.PadSpace;
				else
					cursor = marked;
			}

			statement = new Statement.CreateCollation
			{
				Name         = new CollationName(name),
				CharacterSet = set!,
				Source       = new CollationName(source),
				Padding      = padding,
			};

			return true;
		}

		cursor = save;

		return false;
	}

	static bool Translation(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (Names(ref cursor, 3, out var name) && cursor.Take(SqlWord.For) && CharacterSetSpecification(ref cursor, out var source) &&
			cursor.Take(SqlWord.To) && CharacterSetSpecification(ref cursor, out var target) && cursor.Take(SqlWord.From))
		{
			// A routine is asked first: `ROUTINE` is no reserved word.
			var marked = cursor;

			if (SpecificRoutineDesignator(ref cursor, out var routine))
			{
				statement = new Statement.CreateTranslation
				{
					Name               = name,
					SourceCharacterSet = source!,
					TargetCharacterSet = target!,
					Source             = new TranslationSource(null, routine),
				};

				return true;
			}

			cursor = marked;

			if (Names(ref cursor, 3, out var existing))
			{
				statement = new Statement.CreateTranslation
				{
					Name               = name,
					SourceCharacterSet = source!,
					TargetCharacterSet = target!,
					Source             = new TranslationSource(existing, null),
				};

				return true;
			}
		}

		cursor = save;

		return false;
	}

	// ── §11.74 Sequence generators ─────────────────────────────────────────────

	static bool Sequence(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (!Names(ref cursor, 3, out var name))
		{
			cursor = save;

			return false;
		}

		var options = new List<SequenceOption>();

		while (SequenceGeneratorOption(ref cursor, out var option))
			options.Add(option);

		statement = new Statement.CreateSequence { Name = name, Options = options };

		return true;
	}

	static bool SequenceGeneratorOption(ref SqlCursor cursor, out SequenceOption option)
	{
		var save = cursor;

		option = null!;

		if (cursor.Take(SqlWord.As))
		{
			if (DataType(ref cursor, out var type))
			{
				option = new SequenceOption.DataTypeOption(type);

				return true;
			}

			cursor = save;

			return false;
		}

		return CommonSequenceGeneratorOption(ref cursor, out option);
	}

	static bool CommonSequenceGeneratorOption(ref SqlCursor cursor, out SequenceOption option)
	{
		var save = cursor;

		option = null!;

		if (cursor.Take(SqlWord.Start))
		{
			if (cursor.Take(SqlWord.With) && SignedNumericLiteral(ref cursor, out var literal))
			{
				option = new SequenceOption.Start(new Expression.Literal(literal));

				return true;
			}

			cursor = save;

			return false;
		}

		return BasicSequenceGeneratorOption(ref cursor, out option);
	}

	static bool BasicSequenceGeneratorOption(ref SqlCursor cursor, out SequenceOption option)
	{
		var save = cursor;

		option = null!;

		if (cursor.TakeWord("INCREMENT"))
		{
			if (cursor.Take(SqlWord.By) && SignedNumericLiteral(ref cursor, out var literal))
			{
				option = new SequenceOption.Increment(new Expression.Literal(literal));

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("MAXVALUE"))
		{
			if (SignedNumericLiteral(ref cursor, out var literal))
			{
				option = new SequenceOption.Max(new Expression.Literal(literal));

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.TakeWord("MINVALUE"))
		{
			if (SignedNumericLiteral(ref cursor, out var literal))
			{
				option = new SequenceOption.Min(new Expression.Literal(literal));

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.No))
		{
			if (cursor.TakeWord("MAXVALUE"))
			{
				option = new SequenceOption.Max(null, true);

				return true;
			}

			if (cursor.TakeWord("MINVALUE"))
			{
				option = new SequenceOption.Min(null, true);

				return true;
			}

			if (cursor.Take(SqlWord.Cycle))
			{
				option = new SequenceOption.Cycle(false);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Cycle))
		{
			option = new SequenceOption.Cycle(true);

			return true;
		}

		return false;
	}

	static bool AlterSequenceGeneratorOption(ref SqlCursor cursor, out SequenceOption option)
	{
		var save = cursor;

		option = null!;

		if (cursor.TakeWord("RESTART"))
		{
			Expression? value = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.With))
			{
				if (SignedNumericLiteral(ref cursor, out var literal))
					value = new Expression.Literal(literal);
				else
					cursor = marked;
			}

			option = new SequenceOption.Restart(value);

			return true;
		}

		if (BasicSequenceGeneratorOption(ref cursor, out option))
			return true;

		cursor = save;

		return false;
	}

	/// <summary>A signed numeric literal, which a sequence option's value is.</summary>
	static bool SignedNumericLiteral(ref SqlCursor cursor, out LiteralValue literal)
	{
		var save = cursor;

		if (Literal(ref cursor, out literal) && literal is LiteralValue.Numeric)
			return true;

		cursor  = save;
		literal = null!;

		return false;
	}

	// ── §12 Roles and privileges ───────────────────────────────────────────────

	static bool Role(ref SqlCursor cursor, SqlCursor save, out Statement statement)
	{
		statement = null!;

		cursor.Take();

		if (Identifier(ref cursor, out var name))
		{
			Grantor? admin = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.With) && cursor.TakeWord("ADMIN"))
			{
				admin = Grantor(ref cursor);

				if (admin is null)
					cursor = marked;
			}
			else
			{
				cursor = marked;
			}

			statement = new Statement.CreateRole { Name = name, Admin = admin };

			return true;
		}

		cursor = save;

		return false;
	}

	static Grantor? Grantor(ref SqlCursor cursor)
	{
		return cursor.Take(SqlWord.CurrentUser) ? Ast.Grantor.CurrentUser :
		cursor.Take(SqlWord.CurrentRole) ? Ast.Grantor.CurrentRole :
		null;
	}

	static Grantor? GrantedBy(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("GRANTED") && cursor.Take(SqlWord.By))
		{
			var grantor = Grantor(ref cursor);

			if (grantor is not null)
				return grantor;
		}

		cursor = save;

		return null;
	}

	static bool GrantStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		// The privileges are asked first: they begin with key words, where roles are names.
		var marked = cursor;

		if (Privileges(ref cursor, out var privileges, out var target) && cursor.Take(SqlWord.To) && Grantees(ref cursor, out var grantees))
		{
			var hierarchy = Option(ref cursor, SqlWord.Name, "HIERARCHY");
			var option    = Option(ref cursor, SqlWord.Grant);

			statement = new Statement.Grant
			{
				Body = new GrantBody.Privileges(privileges, target, grantees, hierarchy, option, GrantedBy(ref cursor)),
			};

			return true;
		}

		cursor = marked;

		var roles = new List<Identifier>();

		while (true)
		{
			var comma = cursor;

			if (roles.Count > 0 && !cursor.Take(SqlTokenKind.Comma))
				break;

			if (!Identifier(ref cursor, out var role))
			{
				cursor = comma;

				break;
			}

			roles.Add(role);
		}

		if (roles.Count > 0 && cursor.Take(SqlWord.To) && Grantees(ref cursor, out var to))
		{
			var admin = Option(ref cursor, SqlWord.Name, "ADMIN");

			statement = new Statement.Grant { Body = new GrantBody.Roles(roles, to, admin, GrantedBy(ref cursor)) };

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary><c>WITH GRANT OPTION</c> and its fellows, where one was written.</summary>
	static bool Option(ref SqlCursor cursor, SqlWord word, string? spelling = null)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.With) && (spelling is null ? cursor.Take(word) : cursor.TakeWord(spelling)) && cursor.TakeWord("OPTION"))
			return true;

		cursor = save;

		return false;
	}

	static bool RevokeStatement(ref SqlCursor cursor, out Statement statement)
	{
		var save = cursor;

		statement = null!;

		cursor.Take();

		RevokeOption? revoked = null;

		var marked = cursor;

		if (cursor.Take(SqlWord.Grant) || cursor.TakeWord("HIERARCHY"))
		{
			var grant = true;

			cursor = marked;

			grant = cursor.Take(SqlWord.Grant);

			if (!grant)
				cursor.Take();

			if (cursor.TakeWord("OPTION") && cursor.Take(SqlWord.For))
				revoked = grant ? RevokeOption.GrantOptionFor : RevokeOption.HierarchyOptionFor;
			else
			{
				cursor  = marked;
				revoked = null;
			}
		}

		var privileges = cursor;

		if (Privileges(ref cursor, out var items, out var target) && cursor.Take(SqlWord.From) && Grantees(ref cursor, out var grantees))
		{
			var by = GrantedBy(ref cursor);

			if (DropBehavior(ref cursor) is { } behaviour)
			{
				statement = new Statement.Revoke
				{
					Body = new RevokeBody.Privileges(revoked, items, target, grantees, by, behaviour),
				};

				return true;
			}
		}

		cursor = privileges;

		var admin = false;
		var adminFor = cursor;

		if (cursor.TakeWord("ADMIN") && cursor.TakeWord("OPTION") && cursor.Take(SqlWord.For))
			admin = true;
		else
			cursor = adminFor;

		var roles = new List<Identifier>();

		while (true)
		{
			var comma = cursor;

			if (roles.Count > 0 && !cursor.Take(SqlTokenKind.Comma))
				break;

			if (!Identifier(ref cursor, out var role))
			{
				cursor = comma;

				break;
			}

			roles.Add(role);
		}

		if (roles.Count > 0 && cursor.Take(SqlWord.From) && Grantees(ref cursor, out var from))
		{
			var by = GrantedBy(ref cursor);

			if (DropBehavior(ref cursor) is { } behaviour)
			{
				statement = new Statement.Revoke { Body = new RevokeBody.Roles(admin, roles, from, by, behaviour) };

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool Privileges(ref SqlCursor cursor, out IReadOnlyList<Privilege> privileges, out PrivilegeObject target)
	{
		var save = cursor;

		privileges = [];
		target     = null!;

		if (cursor.Take(SqlWord.All))
		{
			if (!cursor.TakeWord("PRIVILEGES"))
			{
				cursor = save;

				return false;
			}

			privileges = [new Privilege(PrivilegeKind.AllPrivileges, [], [])];
		}
		else
		{
			var actions = new List<Privilege>();

			while (true)
			{
				if (!PrivilegeAction(ref cursor, out var action))
					break;

				actions.Add(action);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (actions.Count == 0)
			{
				cursor = save;

				return false;
			}

			privileges = actions;
		}

		if (cursor.Take(SqlWord.On) && ObjectName(ref cursor, out target))
			return true;

		cursor = save;

		return false;
	}

	static bool PrivilegeAction(ref SqlCursor cursor, out Privilege privilege)
	{
		var save = cursor;

		privilege = null!;

		if (cursor.Take(SqlWord.Select))
		{
			var bracket = cursor;

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				// A column's name could be a routine type's word, so the columns are asked first.
				if (ColumnNameList(ref cursor, out var columns) && cursor.Take(SqlTokenKind.RightParen))
				{
					privilege = new Privilege(PrivilegeKind.Select, columns, []);

					return true;
				}

				cursor = bracket;
				cursor.Take();

				var methods = new List<RoutineDesignator>();

				while (true)
				{
					if (!SpecificRoutineDesignator(ref cursor, out var method))
						break;

					methods.Add(method);

					if (!cursor.Take(SqlTokenKind.Comma))
						break;
				}

				if (methods.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					privilege = new Privilege(PrivilegeKind.Select, [], methods);

					return true;
				}

				cursor = bracket;
			}

			privilege = new Privilege(PrivilegeKind.Select, [], []);

			return true;
		}

		var listed =
			cursor.Take(SqlWord.Insert)     ? PrivilegeKind.Insert :
			cursor.Take(SqlWord.Update)     ? PrivilegeKind.Update :
			cursor.Take(SqlWord.References) ? PrivilegeKind.References :
			(PrivilegeKind?)null;

		if (listed is not null)
		{
			IReadOnlyList<Identifier>? columns = null;

			var bracket = cursor;

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				if (ColumnNameList(ref cursor, out var written) && cursor.Take(SqlTokenKind.RightParen))
					columns = written;
				else
					cursor = bracket;
			}

			privilege = new Privilege(listed.Value, columns ?? [], []);

			return true;
		}

		var alone =
			cursor.Take(SqlWord.Delete)  ? PrivilegeKind.Delete :
			cursor.TakeWord("USAGE")     ? PrivilegeKind.Usage :
			cursor.Take(SqlWord.Trigger) ? PrivilegeKind.Trigger :
			cursor.TakeWord("UNDER")   ? PrivilegeKind.Under :
			cursor.Take(SqlWord.Execute) ? PrivilegeKind.Execute :
			(PrivilegeKind?)null;

		if (alone is not null)
		{
			privilege = new Privilege(alone.Value, [], []);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool ObjectName(ref SqlCursor cursor, out PrivilegeObject target)
	{
		var save = cursor;

		target = null!;

		if (cursor.TakeWord("DOMAIN"))
		{
			if (Names(ref cursor, 3, out var name))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.Domain, name);

				return true;
			}

			cursor = save;
		}

		if (cursor.IsWord("COLLATION"))
		{
			cursor.Take();

			if (Names(ref cursor, 3, out var name))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.Collation, name);

				return true;
			}

			cursor = save;
		}

		if (cursor.Take(SqlWord.Character))
		{
			if (cursor.Take(SqlWord.Set) && CharacterSetSpecification(ref cursor, out var set))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.CharacterSet, set!.Name);

				return true;
			}

			cursor = save;
		}

		if (cursor.Take(SqlWord.Translation))
		{
			if (Names(ref cursor, 3, out var name))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.Translation, name);

				return true;
			}

			cursor = save;
		}

		if (cursor.IsWord("TYPE"))
		{
			cursor.Take();

			if (Names(ref cursor, 3, out var name))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.Type, name);

				return true;
			}

			cursor = save;
		}

		if (cursor.IsWord("SEQUENCE"))
		{
			cursor.Take();

			if (Names(ref cursor, 3, out var name))
			{
				target = new PrivilegeObject(PrivilegeObjectKind.Sequence, name);

				return true;
			}

			cursor = save;
		}

		var routine = cursor;

		if (SpecificRoutineDesignator(ref cursor, out var designator))
		{
			target = new PrivilegeObject(PrivilegeObjectKind.Routine, designator.Name, designator);

			return true;
		}

		cursor = routine;

		var keyword = cursor.Take(SqlWord.Table);

		if (TableName(ref cursor, out var table))
		{
			target = new PrivilegeObject(PrivilegeObjectKind.Table, table, null, keyword);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool Grantees(ref SqlCursor cursor, out IReadOnlyList<Grantee> grantees)
	{
		var save    = cursor;
		var written = new List<Grantee>();

		grantees = written;

		while (true)
		{
			if (cursor.TakeWord("PUBLIC"))
			{
				written.Add(new Grantee.Public());
			}
			else if (Identifier(ref cursor, out var name))
			{
				written.Add(new Grantee.Identifier(new AuthorizationIdentifier(name)));
			}
			else
			{
				cursor = save;

				return false;
			}

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		return true;
	}

	/// <summary>
	/// A predefined type, which a domain is of and a type may be represented by: no collection, no
	/// row, no reference and no user-defined type.
	/// </summary>
	static bool PredefinedType(ref SqlCursor cursor, out DataType type)
	{
		switch (cursor.Word)
		{
			case SqlWord.Character:
			case SqlWord.Char:
			case SqlWord.Varchar:
			case SqlWord.Clob:
				return CharacterStringType(ref cursor, out type);

			case SqlWord.National:
			case SqlWord.Nchar:
			case SqlWord.Nclob:
				return NationalCharacterStringType(ref cursor, out type);

			case SqlWord.Binary:
			case SqlWord.Varbinary:
			case SqlWord.Blob:
				return BinaryStringType(ref cursor, out type);

			case SqlWord.Numeric:
			case SqlWord.Decimal:
			case SqlWord.Dec:
			case SqlWord.Smallint:
			case SqlWord.Integer:
			case SqlWord.Int:
			case SqlWord.Bigint:
			case SqlWord.Real:
			case SqlWord.Float:
			case SqlWord.Double:
			case SqlWord.Decfloat:
				return NumericType(ref cursor, out type);

			case SqlWord.Boolean:
				cursor.Take();

				type = new DataType.Boolean();

				return true;

			case SqlWord.Date:
			case SqlWord.Time:
			case SqlWord.Timestamp:
				return DatetimeType(ref cursor, out type);

			case SqlWord.Interval:
			{
				var save = cursor;

				cursor.Take();

				if (IntervalQualifier(ref cursor, out var qualifier))
				{
					type = new DataType.Interval(qualifier);

					return true;
				}

				cursor = save;

				break;
			}

			case SqlWord.Json:
				cursor.Take();

				type = new DataType.Json();

				return true;
		}

		type = null!;

		return false;
	}
}
