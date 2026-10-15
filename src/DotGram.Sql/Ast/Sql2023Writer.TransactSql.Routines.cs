using System;
using System.Collections.Generic;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Routines family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutRoutines(ISqlNode node)
		{
			switch (node)
			{
				// T-SQL: EXECUTE (Transact-SQL). ExecuteBody's second form, or BareCall: without the word only as the
				// statement being written, a batch's first, with no AT, no return status and a module held by name
				// (?!TSql.Variable), never THROW, which reads as that statement.
				case Statement.ExecuteModule execute:
					PutExecuteModule(execute);
					return true;

				// T-SQL: EXECUTE (Transact-SQL). ExecuteBody's first form.
				case Statement.ExecuteString execute:
					PutExecuteString(execute);
					return true;

				// T-SQL: INSERT (Transact-SQL). <c>INSERT t EXEC …</c>: the call follows the target, never bare.
				case InsertSource.ExecuteModule source:
					if (source.Statement.Keyword is null)
						throw Misplaced(source, "Statement", "a call without EXEC, which only a batch's first statement is");

					PutExecuteModule(source.Statement);
					return true;

				case InsertSource.ExecuteString source:
					PutExecuteString(source.Statement);
					return true;

				// T-SQL: ENABLE TRIGGER (Transact-SQL), DISABLE TRIGGER (Transact-SQL).
				case Statement.EnableTrigger enable:
					PutTriggerSwitch(enable, "ENABLE TRIGGER", enable.Names, enable.All, enable.On, enable.Scope);
					return true;

				case Statement.DisableTrigger disable:
					PutTriggerSwitch(disable, "DISABLE TRIGGER", disable.Names, disable.All, disable.On, disable.Scope);
					return true;

				// T-SQL: CREATE ASSEMBLY (Transact-SQL). One value at least after FROM, each NULL or a value (SetValue).
				case Statement.CreateAssembly create:
					if (create.From.Count == 0)
						throw Empty(create, "From");

					Word("CREATE ASSEMBLY");
					PutIdentifier(create.Name);

					if (create.Authorization is { } owner)
					{
						Word("AUTHORIZATION");
						PutIdentifier(owner);
					}

					Word("FROM");
					Each(create.From, value => PutValue(create, value));

					if (create.PermissionSet is { } permission)
					{
						Word("WITH PERMISSION_SET =");
						Word(PermissionSet(permission));
					}

					return true;

				// T-SQL: ALTER ASSEMBLY (Transact-SQL). One part at least, in the grammar's order; the options each once;
				// DROP FILE ALL or names, never both; the names and a file's name strings (Quoted).
				case Statement.AlterAssembly alter:
					if (alter is { From: null, PermissionSet: null, Visibility: null, UncheckedData: false, DropAllFiles: false, DropFiles.Count: 0, AddFiles.Count: 0 })
						throw Empty(alter, "From", "PermissionSet", "Visibility", "UncheckedData", "DropAllFiles", "DropFiles", "AddFiles");

					if (alter.DropAllFiles && alter.DropFiles.Count > 0)
						throw Both(alter, "DropAllFiles", "DropFiles");

					if (alter.DropFiles.Any(static one => !Quoted(one)) || alter.AddFiles.Any(static one => one.Name is { } name && !Quoted(name)))
						throw Misplaced(alter, "DropFiles or AddFiles", "a file named other than by a string (Quoted)");

					Word("ALTER ASSEMBLY");
					PutIdentifier(alter.Name);

					if (alter.From is { } from)
					{
						Word("FROM");
						PutValue(alter, from);
					}

					if (alter.PermissionSet is not null || alter.Visibility is not null || alter.UncheckedData)
					{
						Word("WITH");
						var first = true;

						if (alter.PermissionSet is { } altered)
						{
							Word("PERMISSION_SET =");
							Word(PermissionSet(altered));
							first = false;
						}

						if (alter.Visibility is { } visible)
						{
							if (!first)
								Tight(",");

							Word(visible ? "VISIBILITY = ON" : "VISIBILITY = OFF");
							first = false;
						}

						if (alter.UncheckedData)
						{
							if (!first)
								Tight(",");

							Word("UNCHECKED DATA");
						}
					}

					if (alter.DropAllFiles)
						Word("DROP FILE ALL");
					else if (alter.DropFiles.Count > 0)
					{
						Word("DROP FILE");
						Each(alter.DropFiles, PutExpression);
					}

					if (alter.AddFiles.Count > 0)
					{
						Word("ADD FILE FROM");
						Each(alter.AddFiles, file =>
						{
							PutValue(alter, file.Value);

							if (file.Name is { } name)
							{
								Word("AS");
								PutExpression(name);
							}
						});
					}

					return true;

				// T-SQL: CREATE RULE (Transact-SQL). Two parts at most (TwoPartName, Msg 166); one variable, or a subquery
				// where there is none, and nothing drawn from a sequence outside one (Syntax.Binds, Msg 160, 161, 11719).
				case Statement.CreateRule rule:
					CheckTwoParts(rule, rule.Name);

					if (!Binds(rule.Condition, rule: true))
						throw Misplaced(rule, "Condition", "with no variable and no subquery, two variables, or NEXT VALUE FOR outside a subquery (Syntax.Binds)");

					Word("CREATE RULE");
					PutName(rule.Name);
					Word("AS");
					PutCondition(rule, rule.Condition);
					return true;

				// T-SQL: CREATE DEFAULT (Transact-SQL). Two parts at most; NULL or a value (SetValue), drawing from no sequence
				// outside a subquery (Syntax.Binds, Msg 11719).
				case Statement.CreateDefault @default:
					CheckTwoParts(@default, @default.Name);

					if (!Binds(@default.Value, rule: false))
						throw Misplaced(@default, "Value", "with NEXT VALUE FOR outside a subquery (Syntax.Binds, Msg 11719)");

					Word("CREATE DEFAULT");
					PutName(@default.Name);
					Word("AS");
					PutValue(@default, @default.Value);
					return true;

				// T-SQL: CREATE AGGREGATE (Transact-SQL). Two parts at most; one parameter at least, a name, AS or not, a type
				// and a nullability and nothing else (AggregateParameter, Msg 10726, 181); the class by one part or two.
				case Statement.CreateAggregate aggregate:
					CheckTwoParts(aggregate, aggregate.Name);
					CheckTwoParts(aggregate, aggregate.ExternalName);

					if (aggregate.Parameters.Count == 0)
						throw Empty(aggregate, "Parameters");

					Word("CREATE AGGREGATE");
					PutName(aggregate.Name);
					Open();
					Each(aggregate.Parameters, parameter =>
					{
						if (parameter is not { Name: { } name, Mode: null, Default: null, Result: false, Locator: false, ReadOnly: false, Varying: false, ShortOutput: false })
							throw Misplaced(parameter, "Mode, Default or a T-SQL word", "on an aggregate's parameter, which has a name, a type and a nullability (AggregateParameter)");

						PutVariableName(parameter, name);

						if (parameter.AsKeyword)
							Word("AS");

						PutType(parameter.Type);

						if (parameter.Nullable is { } nullable)
							Word(nullable ? "NULL" : "NOT NULL");
					});
					Close();
					Word("RETURNS");
					PutType(aggregate.Returns);
					Word("EXTERNAL NAME");
					PutName(aggregate.ExternalName);
					return true;

				// T-SQL: CREATE XML SCHEMA COLLECTION (Transact-SQL), ALTER XML SCHEMA COLLECTION (Transact-SQL). Any value.
				case Statement.CreateXmlSchemaCollection collection:
					Word("CREATE XML SCHEMA COLLECTION");
					PutName(collection.Name);
					Word("AS");
					PutValue(collection, collection.Schema);
					return true;

				case Statement.AlterXmlSchemaCollection collection:
					Word("ALTER XML SCHEMA COLLECTION");
					PutName(collection.Name);
					Word("ADD");
					PutValue(collection, collection.Schema);
					return true;

				// T-SQL: CREATE TYPE (Transact-SQL). An assembly's class by one part or two.
				case Statement.CreateClrType type:
					CheckTwoParts(type, type.ExternalName);
					Word("CREATE TYPE");
					PutName(type.Name);
					Word("EXTERNAL NAME");
					PutName(type.ExternalName);
					return true;

				default:
					return false;
			}
		}

		/// <summary>T-SQL: CREATE ASSEMBLY (Transact-SQL). AssemblyPermission.</summary>
		static string PermissionSet(AssemblyPermissionSet permission)
		{
			return permission switch
			{
				AssemblyPermissionSet.Safe           => "SAFE",
				AssemblyPermissionSet.ExternalAccess => "EXTERNAL_ACCESS",
				AssemblyPermissionSet.Unsafe         => "UNSAFE",
				_                                    => throw NoText(permission),
			};
		}

		/// <summary>The grammar's <c>Quoted</c>: a string, national or not.</summary>
		static bool Quoted(Expression value)
		{
			return value is LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null };
		}

		/// <summary>TwoPartName: one part or two, neither empty (<c>Msg 166</c>).</summary>
		static void CheckTwoParts(ISqlNode owner, QualifiedName name)
		{
			if (name.Count > 2 || name.Parts.Any(static part => part.Text.Length == 0))
				throw Misplaced(owner, "Name", "of more than two parts or an empty one (TwoPartName, Msg 166)");
		}

		/// <summary>
		/// T-SQL: CREATE RULE (Transact-SQL), CREATE DEFAULT (Transact-SQL). <c>Syntax.Binds</c> restated over this tree:
		/// outside every subquery, nothing drawn from a sequence; and a rule's condition names one variable, whatever its
		/// case and never an <c>@@</c> one, or none and holds a subquery.
		/// </summary>
		static bool Binds(Expression value, bool rule)
		{
			var inside    = new HashSet<ISqlSpan>();
			var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var queried   = false;
			var drawn     = false;

			SqlWalker.Walk(value, node =>
			{
				if (inside.Contains(node))
					return true;

				switch (node)
				{
					case Query query:
						queried = true;
						SqlWalker.Walk(query, held => inside.Add(held) || true);
						break;

					case Expression.Variable { Name.Text: var name } when !name.StartsWith("@@", StringComparison.Ordinal):
						variables.Add(name);
						break;

					case Expression.NextValue:
						drawn = true;
						break;
				}

				return true;
			});

			return !drawn && (!rule || variables.Count == 1 || variables.Count == 0 && queried);
		}

		/// <summary>
		/// T-SQL: ENABLE TRIGGER (Transact-SQL). TriggerNames, ALL or names and never both, ON an object, DATABASE or ALL
		/// SERVER: exactly one target.
		/// </summary>
		void PutTriggerSwitch(ISqlNode owner, string words, SqlList<QualifiedName> names, bool all, QualifiedName? on, TriggerScope? scope)
		{
			if (all == names.Count > 0)
				throw all ? Both(owner, "All", "Names") : Empty(owner, "All", "Names");

			if (on is not null == scope is not null)
				throw on is not null ? Both(owner, "On", "Scope") : Empty(owner, "On", "Scope");

			Word(words);

			if (all)
				Word("ALL");
			else
				Each(names, PutName);

			Word("ON");

			if (on is { } named)
				PutName(named);
			else
				PutTriggerScope(scope!.Value);
		}

		void PutTriggerScope(TriggerScope scope)
		{
			Word(scope switch { TriggerScope.Database => "DATABASE", TriggerScope.AllServer => "ALL SERVER", _ => throw NoText(scope) });
		}

		/// <summary>
		/// T-SQL: CREATE TRIGGER (Transact-SQL). A trigger with T-SQL's body is written in T-SQL's form (TriggerStatement),
		/// true then; a standard trigger answers false once it has its Event, Table and Action and none of T-SQL's members.
		/// T-SQL's is ON a table or view, or DATABASE or ALL SERVER, one of them; WITH; FOR, AFTER or INSTEAD OF; INSERT,
		/// UPDATE, DELETE on a table and event names on a scope; WITH APPEND, NOT FOR REPLICATION, AS and the body.
		/// </summary>
		bool PutTransactSqlTrigger(Statement.CreateTrigger trigger)
		{
			var members = trigger.Verb is not null || trigger.Scope is not null || trigger.Events.Count > 0 || trigger.EventNames.Count > 0
				|| trigger.For || trigger.Options.Count > 0 || trigger.Append || trigger.NotForReplication;

			if (trigger.Body is not { } body)
			{
				if (members)
					throw Misplaced(trigger, "a T-SQL member", "beside the standard's Action");

				if (trigger.Event is null || trigger.Table is null || trigger.Action is null)
					throw Empty(trigger, "Event, Table and Action", "Body");

				return false;
			}

			if (trigger.Event is not null || trigger.Action is not null || trigger.Referencing.Count > 0)
				throw Misplaced(trigger, "Event, Action or Referencing", "beside T-SQL's Body");

			if (trigger.Scope is not null == trigger.Table is not null)
				throw trigger.Scope is not null ? Both(trigger, "Scope", "Table") : Empty(trigger, "Scope", "Table");

			if (trigger.Scope is not null ? trigger.Events.Count > 0 || trigger.EventNames.Count == 0 : trigger.EventNames.Count > 0 || trigger.Events.Count == 0)
				throw Misplaced(trigger, "Events or EventNames", "other than statements on a table and event names on a scope");

			if (trigger.Time == TriggerTime.Before || trigger.For && trigger.Time != TriggerTime.After)
				throw Misplaced(trigger, "Time or For", "BEFORE, or FOR beside other than AFTER (TriggerWhen)");

			if (trigger.Events.Any(static one => one.Columns.Count > 0))
				throw Misplaced(trigger, "Events", "with columns, which only the standard's UPDATE OF names");

			if (trigger.EventNames.Any(static one => one.Style == IdentifierStyle.Regular && one.Text.ToUpperInvariant() is "INSERT" or "UPDATE" or "DELETE"))
				throw Misplaced(trigger, "EventNames", "INSERT, UPDATE or DELETE as a name, which reads as the statement (TriggerEvent)");

			if (body is RoutineBody.Inline)
				throw Misplaced(trigger, "Body", "an inline query, which only a function returns");

			Word(trigger.Verb switch
			{
				null                         => "CREATE",
				DefinitionVerb.Alter         => "ALTER",
				DefinitionVerb.CreateOrAlter => "CREATE OR ALTER",
				DefinitionVerb.Create        => throw Misplaced(trigger, "Verb", "Create, which is the same text as null"),
				var verb                     => throw NoText(verb.Value),
			});

			Word("TRIGGER");
			PutName(trigger.Name);
			Word("ON");

			if (trigger.Table is { } table)
				PutName(table);
			else
				PutTriggerScope(trigger.Scope!.Value);

			PutRoutineOptions(trigger, trigger.Options);
			Word(trigger.Time == TriggerTime.InsteadOf ? "INSTEAD OF" : trigger.For ? "FOR" : "AFTER");

			if (trigger.Events.Count > 0)
				Each(trigger.Events, one => Word(one.Kind switch
				{
					TriggerEventKind.Insert => "INSERT",
					TriggerEventKind.Update => "UPDATE",
					TriggerEventKind.Delete => "DELETE",
					_                       => throw NoText(one.Kind),
				}));
			else
				Each(trigger.EventNames, PutIdentifier);

			if (trigger.Append)
				Word("WITH APPEND");

			if (trigger.NotForReplication)
				Word("NOT FOR REPLICATION");

			PutModuleBody(trigger, trigger.Options, body);
			return true;
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL), CREATE FUNCTION (Transact-SQL). A routine whose body is one of T-SQL's —
		/// statements, an external name, an inline query — written in T-SQL's form throughout; true then. Any other body is
		/// the standard's, and false comes back once no T-SQL member is set beside it.
		/// </summary>
		bool PutTransactSqlRoutine(Statement.CreateRoutine create)
		{
			var routine = create.Definition;

			if (routine.Body is not (RoutineBody.Statements or RoutineBody.ExternalName or RoutineBody.Inline))
			{
				if (create.Verb is not null || TransactSqlMembers(routine))
					throw Misplaced(create, "Verb, Options, ForReplication, Number, Proc, Order or a T-SQL parameter", "beside the standard's body");

				return false;
			}

			if (routine.Characteristics.Count > 0 || routine.MethodModifier is not null || routine.ForType is not null || routine.StaticDispatch || routine.SpecificMethod)
				throw Misplaced(create, "Characteristics or a method's members", "beside T-SQL's body; T-SQL writes its options in WITH");

			Word(create.Verb switch
			{
				null                         => "CREATE",
				DefinitionVerb.Alter         => "ALTER",
				DefinitionVerb.CreateOrAlter => "CREATE OR ALTER",
				DefinitionVerb.Create        => throw Misplaced(create, "Verb", "Create, which is the same text as null"),
				var verb                     => throw NoText(verb.Value),
			});

			switch (routine.Kind)
			{
				case RoutineKind.Procedure:
					PutTransactSqlProcedure(routine);
					break;

				case RoutineKind.Function:
					PutTransactSqlFunction(routine);
					break;

				default:
					throw Misplaced(create, "Kind", "other than a procedure or a function, with T-SQL's body");
			}

			return true;
		}

		/// <summary>Whether a standard routine carries a member only T-SQL's form writes.</summary>
		static bool TransactSqlMembers(RoutineDefinition routine)
		{
			return routine.Options.Count > 0 || routine.ForReplication || routine.Number is not null || routine.Proc || routine.Order.Count > 0
				|| routine.Returns is { Variable: not null } or { Elements: not null }
				|| routine.Parameters.Any(static one => one.ShortOutput || one.AsKeyword || one.ReadOnly || one.Varying || one.Nullable is not null);
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). ProcedureStatement: the name and <c>;n</c>, the parameters, WITH, FOR
		/// REPLICATION, and AS a body of statements, an atomic block or an external name; an external name takes EXECUTE AS
		/// alone (<c>Syntax.Externally</c>, <c>Msg 155</c>).
		/// </summary>
		void PutTransactSqlProcedure(RoutineDefinition routine)
		{
			if (routine.Returns is not null || routine.Order.Count > 0)
				throw Misplaced(routine, "Returns or Order", "on a procedure");

			if (routine.Body is RoutineBody.Inline)
				throw Misplaced(routine, "Body", "an inline query, which only a function returns");

			if (routine.Body is RoutineBody.ExternalName && routine.Options.Any(static one => one is not RoutineOption.ExecuteAs))
				throw Misplaced(routine, "Options", "other than EXECUTE AS beside EXTERNAL NAME (Syntax.Externally, Msg 155)");

			Word(routine.Proc ? "PROC" : "PROCEDURE");
			PutName(routine.Name);

			if (routine.Number is { } number)
			{
				if (number.Kind != NumericLiteralKind.DecimalInteger || !Unsigned(number.Text, NumericLiteralKind.DecimalInteger))
					throw Misplaced(routine, "Number", "other than digits (ProcedureStatement)");

				Tight(";");
				Hold();
				PutExpression(number);
			}

			PutTransactSqlParameters(routine.Parameters, procedure: true);
			PutRoutineOptions(routine, routine.Options);

			if (routine.ForReplication)
				Word("FOR REPLICATION");

			PutTransactSqlBody(routine);
		}

		/// <summary>
		/// T-SQL: CREATE FUNCTION (Transact-SQL). FunctionStatement's four shapes, by what RETURNS says: a type; TABLE and an
		/// inline query, told nothing of a null argument (<c>Syntax.Inlined</c>, <c>Msg 487</c>); TABLE and its columns, a
		/// CLR function's, with ORDER; or a variable and its table, a body of statements (<c>Syntax.Declared</c>). No
		/// parameter is a cursor (<c>Syntax.HasCursorParameter</c>, <c>Msg 5306</c>), and a returned table keeps no
		/// column sparse, FILESTREAM or REFERENCES alone (<c>Syntax.Returned</c>).
		/// </summary>
		void PutTransactSqlFunction(RoutineDefinition routine)
		{
			if (routine.ForReplication || routine.Number is not null || routine.Proc)
				throw Misplaced(routine, "ForReplication, Number or Proc", "on a function");

			if (routine.Returns is not { } returns || returns.OnlyPassThrough || returns.Locator || returns.CastFrom is not null || returns.CastFromLocator || returns.TableColumns is not null)
				throw Misplaced(routine, "Returns", "missing, or with the standard's members beside T-SQL's body");

			if (routine.Parameters.Any(static one => one.Varying))
				throw Misplaced(routine, "Parameters", "a cursor, which a function takes none of (Syntax.HasCursorParameter, Msg 5306)");

			var inline   = routine.Body is RoutineBody.Inline;
			var external = routine.Body is RoutineBody.ExternalName;

			switch (returns)
			{
				case { Variable: null, Elements: null, TableKeyword: false, Type: not null }:
					if (inline)
						throw Misplaced(routine, "Body", "an inline query, which a scalar function does not return");

					break;

				case { Variable: null, Elements: null, TableKeyword: true, Type: null }:
					if (!inline)
						throw Misplaced(routine, "Body", "other than RETURN and a query after RETURNS TABLE");

					if (routine.Options.Any(static one => one is RoutineOption.NullCall))
						throw Misplaced(routine, "Options", "ON NULL INPUT of an inline function (Syntax.Inlined, Msg 487)");

					break;

				case { Variable: null, Elements: { Count: > 0 } columns, TableKeyword: true, Type: null }:
					if (!external)
						throw Misplaced(routine, "Body", "other than EXTERNAL NAME after RETURNS TABLE and its columns");

					CheckReturned(routine, columns);
					break;

				case { Variable: { } variable, Elements: { Count: > 0 } columns, TableKeyword: false, Type: null }:
					if (inline || external)
						throw Misplaced(routine, "Body", "other than statements after RETURNS a table variable");

					if (!variable.Text.StartsWith("@", StringComparison.Ordinal) || variable.Style != IdentifierStyle.Regular)
						throw Misplaced(routine, "Returns.Variable", "that is not a variable");

					CheckDeclared(new VariableDeclaration.Table(variable, columns));
					CheckReturned(routine, columns);
					break;

				default:
					throw Misplaced(routine, "Returns", "of none of the four shapes FunctionReturns reads");
			}

			if (routine.Order.Count > 0 && !(external && returns.Elements is not null))
				throw Misplaced(routine, "Order", "on other than a CLR table-valued function");

			Word("FUNCTION");
			PutName(routine.Name);
			PutTransactSqlParameters(routine.Parameters, procedure: false);
			Word("RETURNS");

			if (returns.Variable is { } named)
			{
				PutIdentifier(named);
				Word("TABLE");
				PutElements(returns.Elements!.Value);
			}
			else if (returns.TableKeyword)
			{
				Word("TABLE");

				if (returns.Elements is { } elements)
					PutElements(elements);
			}
			else
				PutType(returns.Type!);

			PutRoutineOptions(routine, routine.Options);

			if (routine.Order.Count > 0)
			{
				Word("ORDER");
				Open();
				Each(routine.Order, item =>
				{
					// IndexColumns: a column by its name and its direction, never NULLS FIRST or LAST.
					if (item is not { Key: Expression.Reference { Name.Count: 1 }, NullOrdering: null })
						throw Misplaced(item, "Key or NullOrdering", "other than a column's name and its direction (IndexColumns)");

					PutExpression(item.Key);

					if (item.Direction is { } direction)
						Word(direction == SortDirection.Asc ? "ASC" : "DESC");
				});
				Close();
			}

			PutTransactSqlBody(routine);
		}

		/// <summary>
		/// T-SQL: CREATE FUNCTION (Transact-SQL). <c>Syntax.Returned</c> restated: a returned table's column is not sparse,
		/// not FILESTREAM, takes no identity's NOT FOR REPLICATION and no REFERENCES without FOREIGN KEY (<c>Msg 102</c>).
		/// </summary>
		static void CheckReturned(ISqlNode owner, SqlList<TableElement> columns)
		{
			foreach (var element in columns)
				if (element is TableElement.Column { Definition.Options: var options }
					&& options.Any(static one => one is ColumnOption.Sparse or ColumnOption.Filestream or ColumnOption.NotForReplication
						|| one is ColumnOption.Constraint { Value: Constraint.ForeignKey { ForeignKeyWords: false } }))
					throw Misplaced(owner, "Returns.Elements", "a column sparse, FILESTREAM, NOT FOR REPLICATION or with REFERENCES alone (Syntax.Returned)");
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). RoutineParameters: <c>@a [AS] type [NULL | NOT NULL] [= value] [OUTPUT |
		/// OUT] [READONLY]</c>, or a cursor's <c>@c [AS] CURSOR VARYING OUTPUT</c> and nothing more (<c>Msg 1051</c>). A
		/// function's are in brackets; a procedure's are written so as well, which reads the same.
		/// </summary>
		void PutTransactSqlParameters(SqlList<ParameterDefinition> parameters, bool procedure)
		{
			if (parameters.Count == 0 && procedure)
				return;

			Open();
			Each(parameters, parameter =>
			{
				if (parameter.Name is not { } name)
					throw Misplaced(parameter, "Name", "missing; every T-SQL parameter is named");

				if (parameter.Mode is ParameterMode.In or ParameterMode.InOut || parameter.Result || parameter.Locator)
					throw Misplaced(parameter, "Mode, Result or Locator", "the standard's, beside T-SQL's form");

				if (parameter.ShortOutput && parameter.Mode != ParameterMode.Out)
					throw Misplaced(parameter, "ShortOutput", "without Mode Out, which it is the spelling of");

				var cursor = parameter.Type is DataType.Named { Name: [{ Text: "CURSOR", Style: IdentifierStyle.Regular }], Arguments.Count: 0 };

				if (cursor != parameter.Varying
					|| cursor && (parameter.Mode != ParameterMode.Out || parameter.Default is not null || parameter.Nullable is not null || parameter.ReadOnly))
					throw Misplaced(parameter, "Varying", "other than on CURSOR VARYING OUTPUT alone, which a cursor parameter is and nothing else (Msg 1051)");

				if (!cursor && TypeWord(parameter.Type) is not null)
					throw Misplaced(parameter, "Type", "named CURSOR or TABLE other than as the one-part Named CURSOR, which reads back as the word");

				PutVariableName(parameter, name);

				if (parameter.AsKeyword)
					Word("AS");

				if (cursor)
					Word("CURSOR VARYING");
				else
				{
					PutType(parameter.Type);

					if (parameter.Nullable is { } nullable)
						Word(nullable ? "NULL" : "NOT NULL");

					if (parameter.Default is { } value)
					{
						if (value is Expression.Comparison)
							throw Misplaced(parameter, "Default", "a comparison, which no value is");

						Word("=");
						PutExpression(value);
					}
				}

				if (parameter.Mode == ParameterMode.Out)
					Word(parameter.ShortOutput ? "OUT" : "OUTPUT");

				if (parameter.ReadOnly)
					Word("READONLY");
			});
			Close();
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). RoutineOptions: <c>WITH</c> and the options, unbracketed. EXECUTE AS is
		/// CALLER, SELF or OWNER with no name, or a user's name as a string (ExecuteAsWho); a setting is one word and what
		/// OptionTail reads after it, never a word that begins the other forms.
		/// </summary>
		void PutRoutineOptions(ISqlNode owner, SqlList<RoutineOption> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Each(options, option =>
			{
				switch (option)
				{
					case RoutineOption.ExecuteAs { Context: var context }:
						Word("EXECUTE AS");

						switch (context)
						{
							case { Kind: ExecutionContextKind.Caller, Name: null }:
								Word("CALLER");
								break;

							case { Kind: ExecutionContextKind.Self, Name: null }:
								Word("SELF");
								break;

							case { Kind: ExecutionContextKind.Owner, Name: null }:
								Word("OWNER");
								break;

							case { Kind: ExecutionContextKind.User, Name: LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } user }:
								PutExpression(user);
								break;

							default:
								throw Misplaced(option, "Context", "other than CALLER, SELF, OWNER or a user's name as a string (ExecuteAsWho)");
						}

						break;

					case RoutineOption.NullCall call:
						Word(call.Mode switch
						{
							NullCallMode.ReturnsNullOnNullInput => "RETURNS NULL ON NULL INPUT",
							NullCallMode.CalledOnNullInput      => "CALLED ON NULL INPUT",
							_                                   => throw NoText(call.Mode),
						});
						break;

					case RoutineOption.Setting { Option: var setting }:
						if (setting.Name.Style == IdentifierStyle.Regular && setting.Name.Text.ToUpperInvariant() is "EXECUTE" or "RETURNS" or "CALLED")
							throw Misplaced(option, "Option", "named " + setting.Name.Text + ", which begins another of RoutineOption's forms");

						// One word, unlike a bracketed list's (the AS would be taken); a value may stand with no =, INLINE ON.
						if (setting.Words.Count > 0 || setting.EqualsOmitted && setting.Options.Count > 0)
							throw Misplaced(option, "Option", "of several words, or with its = left out before a list (RoutineOption)");

						PutOption(setting, multiWord: true);
						break;

					default:
						throw Unwritten(option);
				}
			});
		}

		/// <summary>
		/// T-SQL: CREATE PROCEDURE (Transact-SQL). <c>AS</c> and the body: statements, each ended by <c>;</c>, one at least;
		/// an atomic block alone exactly where NATIVE_COMPILATION is set (<c>Syntax.Natively</c>, <c>Msg 10783</c>,
		/// <c>10782</c>); <c>EXTERNAL NAME</c>; or <c>RETURN</c> and a query.
		/// </summary>
		void PutTransactSqlBody(RoutineDefinition routine)
		{
			PutModuleBody(routine, routine.Options, routine.Body);
		}

		void PutModuleBody(ISqlNode routine, SqlList<RoutineOption> options, RoutineBody body)
		{
			var native = options.Any(static one => one is RoutineOption.Setting { Option.Name: { Style: IdentifierStyle.Regular } name } && name.Text.Equals("NATIVE_COMPILATION", StringComparison.OrdinalIgnoreCase));
			var atomic = body is RoutineBody.Statements { List: [Statement.Compound { Atomic: not null }] };

			if (native != atomic)
				throw Misplaced(routine, "Body", native ? "other than one atomic block with NATIVE_COMPILATION (Msg 10783)" : "an atomic block without NATIVE_COMPILATION (Msg 10782)");

			Word("AS");

			switch (body)
			{
				case RoutineBody.Statements statements:
					if (statements.List.Count == 0)
						throw Empty(statements, "List");

					if (statements.List.Count > 1 && statements.List.Any(static one => one is Statement.Compound { Atomic: not null }))
						throw Misplaced(routine, "Body", "an atomic block among other statements (Msg 10782)");

					PutStatements(statements.List);
					break;

				case RoutineBody.ExternalName external:
					Word("EXTERNAL NAME");
					PutName(external.Name);
					break;

				case RoutineBody.Inline inline:
					if (inline.Query.Hints.Count > 0)
						throw Misplaced(routine, "Body", "an inline query with OPTION (…), which InlineReturn does not read");

					Word("RETURN");
					PutStatement(inline.Query);
					break;
			}
		}

		void PutExecuteModule(Statement.ExecuteModule execute)
		{
			if (execute.Keyword is { } keyword)
				Word(keyword switch { ExecuteKeyword.Exec => "EXEC", ExecuteKeyword.Execute => "EXECUTE", _ => throw NoText(keyword) });
			else if (written.Length > 0)
				throw Misplaced(execute, "Keyword", "null inside another statement, where only a batch's first statement is a bare call");
			else if (execute.At is not null || execute.ReturnStatus is not null || execute.Module is ModuleReference.Variable)
				throw Misplaced(execute, "Keyword", "null with AT, a return status or a module held in a variable, which BareCall does not read (?!TSql.Variable)");
			else if (execute.Module is ModuleReference.Named { DataSource: null, Name: [{ Style: IdentifierStyle.Regular } first, ..] } && first.Text.Equals("THROW", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(execute, "Keyword", "null before a module named THROW, which reads as the THROW statement (FirstPiece tries a statement first)");

			if (execute.ReturnStatus is { } status)
			{
				PutVariableName(execute, status);
				Word("=");
			}

			switch (execute.Module)
			{
				case ModuleReference.Named named:
					if (named.DataSource is { } source)
					{
						if (source.Provider is not LiteralValue.String || source.InitString is not LiteralValue.String || named.Name.Last.Text.Length == 0)
							throw Misplaced(named, "DataSource", "with other than two strings, or a name ending empty (AdHocServer, AdHocObject)");

						Word("OPENDATASOURCE");
						Call();
						PutExpression(source.Provider);
						Tight(",");
						PutExpression(source.InitString);
						Close();
						Tight(".");
						Hold();
					}

					PutName(named.Name);
					break;

				case ModuleReference.Variable variable:
					if (execute.Number is not null)
						throw Misplaced(execute, "Number", "after a module held in a variable (ExecTarget)");

					PutVariableName(variable, variable.Name);
					break;

				default:
					throw Unwritten(execute.Module);
			}

			if (execute.Number is { } number)
			{
				if (number.Kind != NumericLiteralKind.DecimalInteger || !Unsigned(number.Text, NumericLiteralKind.DecimalInteger))
					throw Misplaced(execute, "Number", "other than digits (ExecNumber)");

				Tight(";");
				Hold();
				PutExpression(number);
			}

			Each(execute.Arguments, PutExecuteArgument);

			if (execute.At is { } at)
			{
				Word("AT");
				PutIdentifier(at);
			}

			PutExecuteOptions(execute.Options);
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecArgument: <c>[@name =] value [OUTPUT | OUT]</c>, the value of a form
		/// ExecValue reads, passed back only from a variable (<c>Syntax.Passes</c>, <c>Msg 179</c>); a plain AT by
		/// position reads as the AT clause.
		/// </summary>
		void PutExecuteArgument(ExecuteArgument argument)
		{
			if (!ExecValue(argument.Value))
				throw Misplaced(argument, "Value", "of a form ExecValue does not read: an expression, a sign but on a number or money, a name of two parts");

			if (argument.Output is not null && argument.Value is not Expression.Variable)
				throw Misplaced(argument, "Output", "on a value that is not a variable (Syntax.Passes, Msg 179)");

			if (argument.Name is { } name)
			{
				PutVariableName(argument, name);
				Word("=");
			}
			else if (argument.Value is Expression.Reference { Name: [{ Style: IdentifierStyle.Regular } word] } && word.Text.Equals("AT", StringComparison.OrdinalIgnoreCase))
				throw Misplaced(argument, "Value", "the word AT by position, which reads as the AT clause (ExecArgument's ?!\"AT\")");

			// ExecValue keeps a signed number, and a signed money, as one token (DbccValue).
			PutSignedNumber(argument, argument.Value);

			if (argument.Output is { } output)
				Word(output switch { OutputKeyword.Output => "OUTPUT", OutputKeyword.Out => "OUT", _ => throw NoText(output) });
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecValue: DEFAULT, an ODBC literal, a number or money with its sign in the
		/// literal, a string, a binary string, NULL, a variable, or a name of one part — not an expression.
		/// </summary>
		static bool ExecValue(Expression value)
		{
			return value switch
			{
				Expression.Default or LiteralValue.Null or Expression.Variable => true,
				LiteralValue.Numeric { Kind: NumericLiteralKind.Money } => true,
				LiteralValue.Numeric number => Unsigned(number.Text.StartsWith("-", StringComparison.Ordinal) ? number.Text.Substring(1) : number.Text, number.Kind),
				LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null } => true,
				LiteralValue.Binary { ZeroX: true } => true,
				Expression.OdbcEscape { Called: false, Kind.Text: var kind, Value: LiteralValue.String { Kind: StringLiteralKind.Character } }
					=> kind.ToUpperInvariant() is "TS" or "D" or "T" or "GUID",
				Expression.Reference { Name: [{ Style: IdentifierStyle.Regular or IdentifierStyle.Bracketed or IdentifierStyle.Delimited, Text.Length: > 0 }] } => true,
				_ => false,
			};
		}

		void PutExecuteString(Statement.ExecuteString execute)
		{
			if (execute.At is not null && execute.DataSource is not null)
				throw Both(execute, "At", "DataSource");

			if (execute.Values.Count > 0 && execute.At is null && execute.DataSource is null)
				throw Misplaced(execute, "Values", "without AT or AT DATA_SOURCE, which only a linked server or a data source takes");

			Word(execute.Exec ? "EXEC" : "EXECUTE");
			Open();
			PutExpression(execute.Sql);

			foreach (var value in execute.Values)
			{
				Tight(",");
				PutExpression(value);
			}

			Close();

			if (execute.Context is { } context)
			{
				if (context.Kind is not (ExecutionContextKind.Login or ExecutionContextKind.User)
					|| context.Name is not LiteralValue.String { Kind: StringLiteralKind.Character or StringLiteralKind.National, CharacterSet: null, UnicodeEscape: null })
					throw Misplaced(execute, "Context", "other than LOGIN or USER and a string (ExecContext)");

				Word(context.Kind == ExecutionContextKind.Login ? "AS LOGIN =" : "AS USER =");
				PutExpression(context.Name);
			}

			if (execute.DataSource is { } source)
			{
				Word("AT DATA_SOURCE");
				PutIdentifier(source);
			}

			if (execute.At is { } at)
			{
				Word("AT");
				PutIdentifier(at);
			}

			PutExecuteOptions(execute.Options);
		}

		/// <summary>
		/// T-SQL: EXECUTE (Transact-SQL). ExecWith: words, and RESULT SETS UNDEFINED, NONE or described, its sets
		/// written exactly when described; a set's columns each with its type (ResultColumns, <c>Syntax.Typed</c>).
		/// </summary>
		void PutExecuteOptions(SqlList<ExecuteOption> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Each(options, option =>
			{
				switch (option)
				{
					case ExecuteOption.Named named:
						PutIdentifier(named.Name);
						break;

					case ExecuteOption.ResultSets sets:
						if ((sets.Kind == ResultSetsKind.Described) != (sets.Sets.Count > 0))
							throw Misplaced(sets, "Sets", "written other than exactly when described");

						Word("RESULT SETS");

						switch (sets.Kind)
						{
							case ResultSetsKind.Undefined:
								Word("UNDEFINED");
								break;

							case ResultSetsKind.None:
								Word("NONE");
								break;

							case ResultSetsKind.Described:
								Open();
								Each(sets.Sets, PutResultSet);
								Close();
								break;

							default:
								throw NoText(sets.Kind);
						}

						break;

					default:
						throw Unwritten(option);
				}
			});
		}

		void PutResultSet(ResultSet set)
		{
			switch (set)
			{
				case ResultSet.Columns columns:
					if (columns.Elements.Count == 0)
						throw Empty(columns, "Elements");

					if (columns.Elements.Any(static one => one is not TableElement.Column { Definition.Type: not null }))
						throw Misplaced(columns, "Elements", "other than columns each with its type (ResultColumns, Syntax.Typed)");

					PutElements(columns.Elements);
					break;

				case ResultSet.AsObject named:
					Word("AS OBJECT");
					PutName(named.Name);
					break;

				case ResultSet.AsType typed:
					Word("AS TYPE");
					PutName(typed.Name);
					break;

				case ResultSet.AsForXml:
					Word("AS FOR XML");
					break;

				default:
					throw Unwritten(set);
			}
		}
	}
}
