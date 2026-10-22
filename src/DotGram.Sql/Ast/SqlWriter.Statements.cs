using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		// ── §11 Schema definition and manipulation, §12 Access control ─────────────

		void PutOtherStatement(Statement statement)
		{
			switch (statement)
			{
				case Statement.CreateSchema schema:
					CheckSecurity(schema);
					Word("CREATE SCHEMA");

					if (schema.Name is { } schemaName)
						PutName(schemaName);

					if (schema.Authorization is { } authorization)
					{
						Word("AUTHORIZATION");
						PutIdentifier(authorization.Name);
					}

					if (schema.PathFirst)
					{
						PutPath(schema.Path);
						PutDefaultCharacterSet(schema.DefaultCharacterSet);
					}
					else
					{
						PutDefaultCharacterSet(schema.DefaultCharacterSet);
						PutPath(schema.Path);
					}

					foreach (var element in schema.Elements)
						PutStatement(element);

					break;

				case Statement.CreateTable table:
					// T-SQL: CREATE TABLE (Transact-SQL). A kind, placements, options, or a query without WITH DATA.
					if (PutTransactSqlTable(table))
						break;

					Word("CREATE");

					if (table.Scope is { } scope)
						Word(scope switch { TableScope.GlobalTemporary => "GLOBAL TEMPORARY", TableScope.LocalTemporary => "LOCAL TEMPORARY", _ => throw NoText(scope) });

					Word("TABLE");
					PutName(table.Name);
					PutContents(table.Contents);

					if (table.SystemVersioning)
						Word("WITH SYSTEM VERSIONING");

					PutCommit(table.OnCommit);
					break;

				case Statement.AlterTable alter:
					Word("ALTER TABLE");
					PutName(alter.Name);

					// T-SQL: ALTER TABLE (Transact-SQL). One action, and WITH CHECK only before what it checks.
					if (alter.Check is { } checking)
					{
						if (alter.Actions is not [AlterTableAction.AddColumn or AlterTableAction.AddConstraint or AlterTableAction.Add or AlterTableAction.CheckConstraints])
							throw Misplaced(alter, "Check", "other than before one ADD or one CHECK CONSTRAINT");

						Word(checking == ConstraintCheck.Check ? "WITH CHECK" : checking == ConstraintCheck.NoCheck ? "WITH NOCHECK" : throw NoText(checking));
					}

					if (alter.Actions.Count > 1 && alter.Actions.Any(static one => one is AlterTableAction.Add or AlterTableAction.Drop
						or AlterTableAction.Switch or AlterTableAction.SetOptions or AlterTableAction.Rebuild or AlterTableAction.CheckConstraints
						or AlterTableAction.Triggers or AlterTableAction.ChangeTracking))
						throw Misplaced(alter, "Actions", "more than one beside one of T-SQL's, which ALTER TABLE takes alone");

					Each(alter.Actions, PutAlterTable);
					break;

				case Statement.DropSchema drop:
					Word("DROP SCHEMA");
					PutIfExists(drop.IfExists);

					// T-SQL: DROP SCHEMA (Transact-SQL). One name of one part (DropSingleKind): a list or a dot is Msg 102.
					if (transactSql && drop.Names.Count > 1)
						throw Misplaced(drop, "Names", "of more than one, which DropSingleKind does not read");

					Each(drop.Names, name => PutOnePartName(name, "DROP SCHEMA (DropSingleKind)"));
					PutBehavior(drop.Behavior);
					break;

				case Statement.DropTable drop:
					Word("DROP TABLE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateView view:
					// T-SQL: CREATE VIEW (Transact-SQL). ALTER and CREATE OR ALTER, and the attributes, beside a view the standard's grammar reads.
					// Null is the plain CREATE; Create would be the same text, read back as null.
					if (view.Verb == DefinitionVerb.Create)
						throw Misplaced(view, "Verb", "Create, which is written as null, the standard's plain CREATE");

					if (view.Verb is not null || view.Attributes.Count > 0)
					{
						if (view.Recursive || view.Specification is ViewSpecification.Referenceable)
							throw Misplaced(view, "Verb or Attributes", "beside RECURSIVE or a typed view");

						if (view.CheckOption is CheckOption.Cascaded or CheckOption.Local)
							throw Misplaced(view, "Verb or Attributes", "beside a CASCADED or LOCAL check option");
					}

					Word(view.Verb switch
					{
						null                         => view.Recursive ? "CREATE RECURSIVE VIEW" : "CREATE VIEW",
						DefinitionVerb.Alter         => "ALTER VIEW",
						DefinitionVerb.CreateOrAlter => "CREATE OR ALTER VIEW",
						_                            => throw NoText(view.Verb.Value),
					});
					PutName(view.Name);

					switch (view.Specification)
					{
						case ViewSpecification.Regular { Columns.Count: > 0 } regular:
							Open();
							Each(regular.Columns, PutIdentifier);
							Close();
							break;

						case ViewSpecification.Referenceable referenceable:
							Word("OF");
							PutName(referenceable.TypeName);
							PutUnder(referenceable.Under);

							if (referenceable.Elements.Count > 0)
							{
								Open();
								Each(referenceable.Elements, element =>
								{
									if (element is ViewElement.SelfReference self)
									{
										Word("REF IS");
										PutIdentifier(self.Name);
										PutGeneration(self.Generation);
									}
									else
									{
										var option = (ViewElement.ColumnOption)element;

										PutIdentifier(option.Name);
										Word("WITH OPTIONS SCOPE");
										PutName(option.Scope);
									}
								});
								Close();
							}

							break;
					}

					if (view.Attributes.Count > 0)
					{
						Word("WITH");
						Each(view.Attributes, attribute => Word(attribute switch
						{
							ViewAttribute.Encryption    => "ENCRYPTION",
							ViewAttribute.SchemaBinding => "SCHEMABINDING",
							ViewAttribute.ViewMetadata  => "VIEW_METADATA",
							_                           => throw NoText(attribute),
						}));
					}

					Word("AS");
					PutQuery(view.Query);

					if (view.CheckOption is { } check)
						Word(check switch { CheckOption.Cascaded => "WITH CASCADED CHECK OPTION", CheckOption.Local => "WITH LOCAL CHECK OPTION", CheckOption.Unqualified => "WITH CHECK OPTION", _ => throw NoText(check) });

					break;

				case Statement.DropView drop:
					Word("DROP VIEW");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateDomain domain:
					Word("CREATE DOMAIN");
					PutName(domain.Name);

					if (domain.AsKeyword)
						Word("AS");

					PutType(domain.Type);
					PutDefault(domain.Default);

					foreach (var constraint in domain.Constraints)
						PutConstraint(constraint);

					PutCollate(domain.Collation);
					break;

				case Statement.AlterDomain alter:
					Word("ALTER DOMAIN");
					PutName(alter.Name);

					switch (alter.Action)
					{
						case AlterDomainAction.SetDefault set:
							Word("SET");
							PutDefault(set.Value);
							break;

						case AlterDomainAction.DropDefault:
							Word("DROP DEFAULT");
							break;

						case AlterDomainAction.AddConstraint add:
							Word("ADD");
							PutConstraint(add.Constraint);
							break;

						default:
							Word("DROP CONSTRAINT");
							PutName(((AlterDomainAction.DropConstraint)alter.Action).Name);
							break;
					}

					break;

				case Statement.DropDomain drop:
					Word("DROP DOMAIN");
					PutName(drop.Name);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateCharacterSet set:
					Word("CREATE CHARACTER SET");
					PutCharacterSet(set.Name);

					if (set.AsKeyword)
						Word("AS");

					Word("GET");
					PutCharacterSet(set.Source);
					PutCollate(set.Collation);
					break;

				case Statement.DropCharacterSet drop:
					Word("DROP CHARACTER SET");
					PutCharacterSet(drop.Name);
					break;

				case Statement.CreateCollation collation:
					Word("CREATE COLLATION");
					PutCollation(collation.Name);
					Word("FOR");
					PutCharacterSet(collation.CharacterSet);
					Word("FROM");
					PutCollation(collation.Source);

					if (collation.Padding is { } padding)
						Word(padding switch { PadCharacteristic.NoPad => "NO PAD", PadCharacteristic.PadSpace => "PAD SPACE", _ => throw NoText(padding) });

					break;

				case Statement.DropCollation drop:
					Word("DROP COLLATION");
					PutCollation(drop.Name);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateTranslation translation:
					Word("CREATE TRANSLATION");
					PutName(translation.Name);
					Word("FOR");
					PutCharacterSet(translation.SourceCharacterSet);
					Word("TO");
					PutCharacterSet(translation.TargetCharacterSet);
					Word("FROM");

					if (translation.Source.Routine is { } fromRoutine)
						PutDesignator(fromRoutine);
					else
						PutName(translation.Source.Existing!.Value);

					break;

				case Statement.DropTranslation drop:
					Word("DROP TRANSLATION");
					PutName(drop.Name);
					break;

				case Statement.CreateAssertion assertion:
					Word("CREATE ASSERTION");
					PutName(assertion.Name);
					Word("CHECK");
					Open();
					PutCondition(assertion, assertion.Condition);
					Close();
					PutCharacteristics(assertion.Characteristics);
					break;

				case Statement.DropAssertion drop:
					Word("DROP ASSERTION");
					PutName(drop.Name);

					if (drop.Behavior is { } behavior)
						PutBehavior(behavior);

					break;

				case Statement.CreateSequence sequence:
					CheckSequenceOptions(sequence, sequence.Options);
					Word("CREATE SEQUENCE");
					PutName(sequence.Name);

					foreach (var option in sequence.Options)
						PutSequenceOption(option, false, true);

					break;

				case Statement.AlterSequence sequence:
					CheckSequenceOptions(sequence, sequence.Options);
					Word("ALTER SEQUENCE");
					PutName(sequence.Name);

					foreach (var option in sequence.Options)
						PutSequenceOption(option, false, true);

					break;

				case Statement.DropSequence drop:
					Word("DROP SEQUENCE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.Grant { Body: GrantBody.Privileges privileges } grantPrivileges:
					CheckSecurity(grantPrivileges);
					Word("GRANT");
					PutPrivileges(privileges.Items, privileges.Object);
					Word("TO");
					Each(privileges.Grantees, PutGrantee);

					if (privileges.HierarchyOption)
						Word("WITH HIERARCHY OPTION");

					if (privileges.GrantOption)
						Word("WITH GRANT OPTION");

					PutGrantedBy(privileges.GrantedBy);
					PutAsPrincipal(privileges.AsPrincipal);
					break;

				case Statement.Deny deny:
					CheckSecurity(deny);
					Word("DENY");
					PutPrivileges(deny.Items, deny.Object);
					Word("TO");
					Each(deny.Grantees, PutGrantee);

					if (deny.Cascade)
						Word("CASCADE");

					PutAsPrincipal(deny.AsPrincipal);
					break;

				case Statement.Grant grant:
					CheckSecurity(grant);

					var roles = (GrantBody.Roles)grant.Body;

					Word("GRANT");
					Each(roles.Names, PutIdentifier);
					Word("TO");
					Each(roles.Grantees, PutGrantee);

					if (roles.AdminOption)
						Word("WITH ADMIN OPTION");

					PutGrantedBy(roles.GrantedBy);
					break;

				case Statement.Revoke { Body: RevokeBody.Privileges revoked } revokePrivileges:
					CheckSecurity(revokePrivileges);
					Word("REVOKE");

					if (revoked.Option is { } revokeOption)
						Word(revokeOption switch { RevokeOption.GrantOptionFor => "GRANT OPTION FOR", RevokeOption.HierarchyOptionFor => "HIERARCHY OPTION FOR", _ => throw NoText(revokeOption) });

					PutPrivileges(revoked.Items, revoked.Object);
					// The page publishes `{ TO | FROM }` for one clause, and FROM is the standard's word.
					Word(revoked.To ? "TO" : "FROM");
					Each(revoked.Grantees, PutGrantee);
					PutGrantedBy(revoked.GrantedBy);
					PutBehavior(revoked.Behavior);
					PutAsPrincipal(revoked.AsPrincipal);
					break;

				case Statement.Revoke revoke:
					CheckSecurity(revoke);

					var revokedRoles = (RevokeBody.Roles)revoke.Body;

					Word("REVOKE");

					if (revokedRoles.AdminOptionFor)
						Word("ADMIN OPTION FOR");

					Each(revokedRoles.Names, PutIdentifier);
					Word("FROM");
					Each(revokedRoles.Grantees, PutGrantee);
					PutGrantedBy(revokedRoles.GrantedBy);
					PutBehavior(revokedRoles.Behavior);
					break;

				case Statement.CreateRole { Authorization: not null, Admin: not null } both:
					throw Both(both, "Authorization", "Admin");

				case Statement.CreateRole role:
					CheckSecurity(role);
					Word("CREATE ROLE");
					PutIdentifier(role.Name);
					PutAuthorization(role.Authorization);

					if (role.Admin is { } admin)
						Word(admin switch { Grantor.CurrentUser => "WITH ADMIN CURRENT_USER", Grantor.CurrentRole => "WITH ADMIN CURRENT_ROLE", _ => throw NoText(admin) });

					break;

				case Statement.DropRole drop:
					Word("DROP ROLE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutIdentifier);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateTrigger trigger:
					PutTrigger(trigger);
					break;

				case Statement.DropTrigger drop:
					Word("DROP TRIGGER");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);

					if (drop.On is { } on)
					{
						Word(on switch
						{
							TriggerScope.Database  => "ON DATABASE",
							TriggerScope.AllServer => "ON ALL SERVER",
							_                      => throw NoText(on),
						});
					}

					break;

				case Statement.CreateType type:
					PutTypeDefinition(type.Definition);
					break;

				case Statement.AlterType alter:
					Word("ALTER TYPE");
					PutName(alter.Name);

					switch (alter.Action)
					{
						case AlterTypeAction.AddAttribute add:
							Word("ADD ATTRIBUTE");
							PutAttribute(add.Attribute);
							break;

						case AlterTypeAction.AddMethod method:
							Word("ADD");
							PutMethod(method.Method);
							break;

						case AlterTypeAction.DropAttribute drop:
							Word("DROP ATTRIBUTE");
							PutIdentifier(drop.Name);
							Word("RESTRICT");
							break;

						default:
							var designator = ((AlterTypeAction.DropMethod)alter.Action).Method;

							Word("DROP");
							PutModifier(designator.Modifier);
							Word("METHOD");
							PutIdentifier(designator.Name);
							Open();
							Each(designator.ParameterTypes, PutType);
							Close();
							Word("RESTRICT");
							break;
					}

					break;

				case Statement.DropType drop:
					Word("DROP TYPE");
					PutIfExists(drop.IfExists);

					// T-SQL: DROP TYPE (Transact-SQL). One name that begins with a part (DropLedName), never `.t`.
					if (transactSql && drop.Names.Count > 1)
						throw Misplaced(drop, "Names", "of more than one, which DropLedName does not read");

					Each(drop.Names, name => PutLedName(name, "DROP TYPE (DropLedName)"));
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateCast cast:
					Word("CREATE CAST");
					Open();
					PutType(cast.SourceType);
					Word("AS");
					PutType(cast.TargetType);
					Close();
					Word("WITH");
					PutDesignator(cast.Function);

					if (cast.AsAssignment)
						Word("AS ASSIGNMENT");

					break;

				case Statement.DropCast drop:
					Word("DROP CAST");
					Open();
					PutType(drop.SourceType);
					Word("AS");
					PutType(drop.TargetType);
					Close();
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateOrdering ordering:
					Word("CREATE ORDERING FOR");
					PutName(ordering.TypeName);
					Word(ordering.Ordering.Form switch { OrderingForm.EqualsOnly => "EQUALS ONLY BY", OrderingForm.Full => "ORDER FULL BY", _ => throw NoText(ordering.Ordering.Form) });

					switch (ordering.Ordering.Category)
					{
						case OrderingCategory.Relative relative:
							Word("RELATIVE WITH");
							PutDesignator(relative.Function);
							break;

						case OrderingCategory.Map map:
							Word("MAP WITH");
							PutDesignator(map.Function);
							break;

						default:
							Word("STATE");

							if (((OrderingCategory.State)ordering.Ordering.Category).SpecificName is { } specific)
								PutName(specific);

							break;
					}

					break;

				case Statement.DropOrdering drop:
					Word("DROP ORDERING FOR");
					PutName(drop.TypeName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateTransform transform:
					Word(transform.PluralKeyword ? "CREATE TRANSFORMS FOR" : "CREATE TRANSFORM FOR");
					PutName(transform.TypeName);

					foreach (var group in transform.Groups)
					{
						PutIdentifier(group.Name);
						Open();
						Each(group.Elements, PutTransformElement);
						Close();
					}

					break;

				case Statement.AlterTransform transform:
					Word(transform.PluralKeyword ? "ALTER TRANSFORMS FOR" : "ALTER TRANSFORM FOR");
					PutName(transform.TypeName);

					foreach (var group in transform.Groups)
					{
						PutIdentifier(group.Name);
						Open();
						Each(group.Actions, action =>
						{
							if (action.Add)
							{
								Word("ADD");
								Open();
								Each(action.Elements, PutTransformElement);
								Close();
							}
							else
							{
								Word("DROP");
								Open();
								Each(action.DropKinds, kind => Word(kind switch { TransformDirection.ToSql => "TO SQL", TransformDirection.FromSql => "FROM SQL", _ => throw NoText(kind) }));
								PutBehavior(action.Behavior!.Value);
								Close();
							}
						});
						Close();
					}

					break;

				case Statement.DropTransform drop:
					Word(drop.PluralKeyword ? "DROP TRANSFORMS" : "DROP TRANSFORM");

					if (drop.Target is TransformDropTarget.Group dropped)
						PutIdentifier(dropped.Name);
					else
						Word("ALL");

					Word("FOR");
					PutName(drop.TypeName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateRoutine routine:
					if (PutTransactSqlRoutine(routine))
						break;

					Word("CREATE");
					PutRoutine(routine.Definition);
					break;

				case Statement.AlterRoutine alter:
					Word("ALTER");
					PutDesignator(alter.Routine);

					foreach (var characteristic in alter.Characteristics)
						PutCharacteristic(characteristic);

					Word(alter.Behavior switch { AlterRoutineBehavior.Restrict => "RESTRICT", _ => throw NoText(alter.Behavior) });
					break;

				// T-SQL's list, `DROP PROCEDURE IF EXISTS p, q`, names the kind once, the first routine's, and
				// IF EXISTS after it; the standard's statement drops one routine and has no IF EXISTS.
				case Statement.DropRoutine drop:
					Word("DROP");

					if (drop.Routines.Count > 0)
						PutDesignatorKind(drop.Routines[0], drop.Proc);

					// The kind is named once, so a routine of another kind would be written as the first's.
					if (drop.Routines.Any(one => one.Kind != drop.Routines[0].Kind))
						throw Misplaced(drop, "Routines", "of more than one kind, which the statement names once");

					PutIfExists(drop.IfExists);
					Each(drop.Routines, PutDesignatorName);
					PutBehavior(drop.Behavior);
					break;

				default:
					PutExecutableStatement(statement);
					break;
			}
		}

		void PutPath(PathSpecification? path)
		{
			if (path is null)
				return;

			Word("PATH");
			Each(path.Schemas, PutName);
		}

		void PutDefaultCharacterSet(CharacterSetName? set)
		{
			if (set is null)
				return;

			Word("DEFAULT CHARACTER SET");
			PutCharacterSet(set);
		}

		void PutBehavior(DropBehavior behavior)
		{
			Word(behavior switch { DropBehavior.Cascade => "CASCADE", DropBehavior.Restrict => "RESTRICT", _ => throw NoText(behavior) });
		}

		void PutBehavior(DropBehavior? behavior)
		{
			if (behavior is { } one)
				PutBehavior(one);
		}

		void PutIfExists(bool ifExists)
		{
			if (ifExists)
				Word("IF EXISTS");
		}

		void PutCommit(TableCommitAction? commit)
		{
			if (commit is { } one)
				Word(one switch { TableCommitAction.Preserve => "ON COMMIT PRESERVE ROWS", TableCommitAction.Delete => "ON COMMIT DELETE ROWS", _ => throw NoText(one) });
		}

		void PutDefault(Expression? value)
		{
			if (value is null)
				return;

			Word("DEFAULT");
			PutExpression(value);
		}

		void PutCollate(CollationName? collation)
		{
			if (collation is null)
				return;

			Word("COLLATE");
			PutCollation(collation);
		}

		void PutUnder(QualifiedName? under)
		{
			if (under is null)
				return;

			Word("UNDER");
			PutName(under.Value);
		}

		void PutGeneration(ReferenceGeneration? generation)
		{
			if (generation is { } one)
				Word(one switch { ReferenceGeneration.SystemGenerated => "SYSTEM GENERATED", ReferenceGeneration.UserGenerated => "USER GENERATED", ReferenceGeneration.Derived => "DERIVED", _ => throw NoText(one) });
		}

		// ── §11.3 Tables, columns and constraints ──────────────────────────────────

		void PutContents(TableContents contents)
		{
			switch (contents)
			{
				case TableContents.Elements elements:
					PutElements(elements.Items);
					break;

				case TableContents.Typed typed:
					Word("OF");
					PutName(typed.TypeName);
					PutUnder(typed.Under);

					if (typed.Items.Count > 0)
						PutElements(typed.Items);

					break;

				default:
					var query = (TableContents.AsQuery)contents;

					if (query.Hints.Count > 0)
						throw Misplaced(query, "Hints", "on the standard's table made of a query, which reads no OPTION");

					if (query.Columns.Count > 0)
					{
						Open();
						Each(query.Columns, PutIdentifier);
						Close();
					}

					Word("AS");
					PutSubquery(query.Query);
					Word(query.Data switch
					{
						WithDataMode.WithData   => "WITH DATA",
						WithDataMode.WithNoData => "WITH NO DATA",
						null                    => throw Misplaced(query, "Data", "null, which only T-SQL's table made of a query is written with"),
						_                       => throw NoText(query.Data.Value),
					});
					break;
			}
		}

		void PutElements(SqlList<TableElement> elements)
		{
			Open();
			Each(elements, PutElement);
			Close();
		}

		void PutElement(TableElement element)
		{
			// T-SQL: CREATE TABLE (Transact-SQL). An element holds constraints, which reach elements again through T-SQL's.
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, element, static (into, node) => into.PutElement(node));

				return;
			}

			switch (element)
			{
				case TableElement.Column column:
					PutColumn(column.Definition);
					break;

				case TableElement.Period period:
					PutPeriodDefinition(period.Definition);
					break;

				case TableElement.TableConstraint constraint:
					PutConstraint(constraint.Value);
					break;

				case TableElement.Like like:
					Word("LIKE");
					PutName(like.Table);

					foreach (var option in like.Options)
						Word(option switch
						{
							LikeOption.IncludingIdentity  => "INCLUDING IDENTITY",
							LikeOption.ExcludingIdentity  => "EXCLUDING IDENTITY",
							LikeOption.IncludingDefaults  => "INCLUDING DEFAULTS",
							LikeOption.ExcludingDefaults  => "EXCLUDING DEFAULTS",
							LikeOption.IncludingGenerated => "INCLUDING GENERATED",
							LikeOption.ExcludingGenerated                             => "EXCLUDING GENERATED",
							_                             => throw NoText(option),
						});

					break;

				case TableElement.SelfReference self:
					Word("REF IS");
					PutIdentifier(self.Name);
					PutGeneration(self.Generation);
					break;

				case TableElement.Index:
					if (!TryPutTransactSql(element))
						throw Unwritten(element);

					break;

				default:
					var options = (TableElement.ColumnOptions)element;

					PutIdentifier(options.Name);
					Word("WITH OPTIONS");

					if (options.Options.Scope is { } scope)
					{
						Word("SCOPE");
						PutName(scope);
					}

					PutDefault(options.Options.Default);

					foreach (var constraint in options.Options.Constraints)
						PutConstraint(constraint);

					break;
			}
		}

		void PutPeriodDefinition(PeriodDefinition period)
		{
			Word("PERIOD FOR");

			// The kind says which of the two it is, and the name only the application's.
			switch (period.Kind)
			{
				case PeriodKind.SystemTime when period.ApplicationName is null:
					Word("SYSTEM_TIME");
					break;

				case PeriodKind.ApplicationTime when period.ApplicationName is { } name:
					PutIdentifier(name);
					break;

				case PeriodKind.SystemTime:
				case PeriodKind.ApplicationTime:
					throw Misplaced(period, "ApplicationName", "that does not agree with Kind: only an application time is named");

				default:
					throw NoText(period.Kind);
			}

			Open();
			PutIdentifier(period.BeginColumn);
			Tight(",");
			PutIdentifier(period.EndColumn);
			Close();
		}

		void PutColumn(ColumnDefinition column)
		{
			// T-SQL: column_definition (Transact-SQL). Options in the order written.
			if (PutTransactSqlColumn(column))
				return;

			PutIdentifier(column.Name);

			if (column.Type is { } type)
				PutColumnType(type);

			switch (column.Generation)
			{
				case null:
					break;

				case ColumnGeneration.Default @default:
					PutDefault(@default.Value);
					break;

				case ColumnGeneration.Identity identity:
					Word(identity.Generation switch { IdentityGeneration.Always => "GENERATED ALWAYS AS IDENTITY", IdentityGeneration.ByDefault => "GENERATED BY DEFAULT AS IDENTITY", _ => throw NoText(identity.Generation) });

					if (identity.Options.Count > 0)
					{
						Open();

						foreach (var option in identity.Options)
							PutSequenceOption(option, false, false);

						Close();
					}

					break;

				case ColumnGeneration.Generated generated:
					Word("GENERATED ALWAYS AS");
					Open();
					PutExpression(generated.Expression);
					Close();
					break;

				case ColumnGeneration.RowStart:
					Word("GENERATED ALWAYS AS ROW START");
					break;

				default:
					Word("GENERATED ALWAYS AS ROW END");
					break;
			}

			foreach (var constraint in column.Constraints)
				PutConstraint(constraint);

			PutCollate(column.Collation);
		}

		// A constraint with no columns is a column's; a table's names them.
		void PutConstraint(Constraint constraint)
		{
			if (constraint.Name is { } name)
			{
				Word("CONSTRAINT");
				PutOnePartName(name, "a constraint's name");
			}

			switch (constraint)
			{
				case Constraint.NotNull:
					Word("NOT NULL");
					break;

				case Constraint.Unique { Kind: UniqueKind.UniqueValue } uniqueValue:
					// T-SQL reads `UNIQUE (VALUE)` as a key over a column named VALUE; the standard's has no column.
					if (transactSql || uniqueValue.Columns.Count > 0 || uniqueValue.Nulls is not null || uniqueValue.WithoutOverlapsPeriod is not null)
						throw Misplaced(uniqueValue, "Kind", "UniqueValue with a column, NULLS, a period or in T-SQL, which write and read `UNIQUE (VALUE)` as something else");

					Word("UNIQUE");
					Open();
					Word("VALUE");
					Close();
					break;

				case Constraint.Unique unique:
					// T-SQL: table_constraint (Transact-SQL). Clustering, keys, options and a placement.
					if (PutTransactSqlKey(unique))
						break;

					Word(unique.Kind switch { UniqueKind.Unique => "UNIQUE", UniqueKind.PrimaryKey => "PRIMARY KEY", _ => throw NoText(unique.Kind) });

					if (unique.Nulls is { } nulls)
						Word(nulls switch { NullDistinctness.Distinct => "NULLS DISTINCT", NullDistinctness.NotDistinct => "NULLS NOT DISTINCT", _ => throw NoText(nulls) });

					if (unique.Columns.Count > 0)
					{
						Open();
						Each(unique.Columns, PutIdentifier);

						if (unique.WithoutOverlapsPeriod is { } period)
						{
							Tight(",");
							PutIdentifier(period);
							Word("WITHOUT OVERLAPS");
						}

						Close();
					}

					break;

				case Constraint.ForeignKey foreign:
					// T-SQL: column_constraint (Transact-SQL). A column's FOREIGN KEY, which names no columns.
					if (foreign.ForeignKeyWords)
					{
						if (foreign.Columns.Count > 0)
							throw Misplaced(foreign, "ForeignKeyWords", "with columns, which write the words already");

						Word("FOREIGN KEY");
					}

					if (foreign.Columns.Count > 0)
					{
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
					}

					PutReferences(foreign.References);
					break;

				case Constraint.Check check:
					Word("CHECK");

					// T-SQL: table_constraint (Transact-SQL).
					if (check.NotForReplication)
						Word("NOT FOR REPLICATION");

					Open();
					PutCondition(check, check.Condition);
					Close();
					break;

				default:
					if (!TryPutTransactSql(constraint))
						throw Unwritten(constraint);

					break;
			}

			PutCharacteristics(constraint.Characteristics);
		}

		void PutReferences(ReferencesSpecification references)
		{
			Word("REFERENCES");
			PutName(references.Table);

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
				PutRule("ON DELETE", references.OnDelete);
				PutRule("ON UPDATE", references.OnUpdate);
			}
			else
			{
				PutRule("ON UPDATE", references.OnUpdate);
				PutRule("ON DELETE", references.OnDelete);
			}

			// T-SQL: column_constraint (Transact-SQL), table_constraint (Transact-SQL). Last, after the actions.
			if (references.NotForReplication)
				Word("NOT FOR REPLICATION");
		}

		void PutRule(string rule, ReferentialAction? action)
		{
			if (action is not { } one)
				return;

			Word(rule);
			Word(one switch
			{
				ReferentialAction.Cascade    => "CASCADE",
				ReferentialAction.SetNull    => "SET NULL",
				ReferentialAction.SetDefault => "SET DEFAULT",
				ReferentialAction.Restrict   => "RESTRICT",
				ReferentialAction.NoAction                            => "NO ACTION",
				_                            => throw NoText(one),
			});
		}

		void PutCharacteristics(ConstraintCharacteristics? characteristics)
		{
			if (characteristics is null)
				return;

			if (characteristics.DeferrableFirst)
			{
				PutDeferrable(characteristics.Deferrable);
				PutTiming(characteristics.Timing);
			}
			else
			{
				PutTiming(characteristics.Timing);
				PutDeferrable(characteristics.Deferrable);
			}

			if (characteristics.Enforced is { } enforced)
				Word(enforced ? "ENFORCED" : "NOT ENFORCED");
		}

		void PutDeferrable(bool? deferrable)
		{
			if (deferrable is { } one)
				Word(one ? "DEFERRABLE" : "NOT DEFERRABLE");
		}

		void PutTiming(ConstraintTiming? timing)
		{
			if (timing is { } one)
				Word(one switch { ConstraintTiming.InitiallyDeferred => "INITIALLY DEFERRED", ConstraintTiming.InitiallyImmediate => "INITIALLY IMMEDIATE", _ => throw NoText(one) });
		}

		void PutAlterTable(AlterTableAction action)
		{
			switch (action)
			{
				case AlterTableAction.AddColumn add:
					Word(add.ColumnKeyword ? "ADD COLUMN" : "ADD");
					adding = true;
					PutColumn(add.Column);
					adding = false;
					break;

				case AlterTableAction.AlterColumn alter:
					Word(alter.ColumnKeyword ? "ALTER COLUMN" : "ALTER");
					PutIdentifier(alter.Column);
					PutAlterColumn(alter.Action);
					break;

				// T-SQL: ALTER TABLE (Transact-SQL). Without a behavior T-SQL reads `DROP a` as a constraint.
				case AlterTableAction.DropColumn drop:
					if (drop is { ColumnKeyword: false, Behavior: null })
						throw Misplaced(drop, "ColumnKeyword", "false with no Behavior, which T-SQL reads as a constraint dropped");

					Word("DROP");
					PutDroppedColumn(drop);
					break;

				case AlterTableAction.AddConstraint add:
					Word("ADD");
					adding = true;
					PutConstraint(add.Constraint);
					adding = false;
					break;

				case AlterTableAction.AlterConstraint alter:
					Word("ALTER CONSTRAINT");
					PutName(alter.Name);
					Word(alter.Enforced ? "ENFORCED" : "NOT ENFORCED");
					break;

				case AlterTableAction.DropConstraint drop:
					Word("DROP");
					PutDroppedConstraint(drop);
					break;

				case AlterTableAction.AddPeriod add:
					Word("ADD");
					PutPeriodDefinition(add.Period);

					foreach (var column in add.AddedColumns)
					{
						Word(column.ColumnKeyword ? "ADD COLUMN" : "ADD");
						PutColumn(column.Column);
					}

					break;

				case AlterTableAction.DropPeriod drop:
					// T-SQL reads SYSTEM_TIME and no behavior (DropTarget); the standard demands a behavior.
					if (transactSql ? drop.Behavior is not null || drop.Kind != PeriodKind.SystemTime : drop.Behavior is null)
						throw Misplaced(drop, "Behavior or Kind", transactSql ? "a behavior or an application time, which T-SQL's DROP PERIOD does not read (DropTarget)" : "no behavior, which the standard's DROP PERIOD demands");

					Word("DROP PERIOD FOR");

					switch (drop.Kind)
					{
						case PeriodKind.SystemTime when drop.ApplicationName is null:
							Word("SYSTEM_TIME");
							break;

						case PeriodKind.ApplicationTime when drop.ApplicationName is { } name:
							PutIdentifier(name);
							break;

						case PeriodKind.SystemTime:
						case PeriodKind.ApplicationTime:
							throw Misplaced(drop, "ApplicationName", "that does not agree with Kind: only an application time is named");

						default:
							throw NoText(drop.Kind);
					}

					if (drop.Behavior is { } behavior)
						PutBehavior(behavior);

					break;

				case AlterTableAction.AddSystemVersioning:
					Word("ADD SYSTEM VERSIONING");
					break;

				case AlterTableAction.DropSystemVersioning drop:
					Word("DROP SYSTEM VERSIONING");
					PutBehavior(drop.Behavior);
					break;

				default:
					if (!TryPutTransactSql(action))
						throw Unwritten(action);

					break;
			}
		}

		void PutAlterColumn(AlterColumnAction action)
		{
			switch (action)
			{
				case AlterColumnAction.SetDefault set:
					Word("SET");
					PutDefault(set.Value);
					break;

				case AlterColumnAction.DropDefault:
					Word("DROP DEFAULT");
					break;

				case AlterColumnAction.SetNotNull:
					Word("SET NOT NULL");
					break;

				case AlterColumnAction.DropNotNull:
					Word("DROP NOT NULL");
					break;

				case AlterColumnAction.AddScope add:
					Word("ADD SCOPE");
					PutName(add.Table);
					break;

				case AlterColumnAction.DropScope drop:
					Word("DROP SCOPE");
					PutBehavior(drop.Behavior);
					break;

				case AlterColumnAction.SetDataType type:
					// T-SQL: ALTER TABLE (Transact-SQL). The type alone, with its collation, nullability and options.
					if (PutTransactSqlType(type))
						break;

					Word("SET DATA TYPE");
					PutType(type.Type);
					break;

				case AlterColumnAction.SetIdentityGeneration generation:
					CheckSequenceOptions(generation, generation.Options);
					Word(generation.Generation switch { IdentityGeneration.Always => "SET GENERATED ALWAYS", IdentityGeneration.ByDefault => "SET GENERATED BY DEFAULT", _ => throw NoText(generation.Generation) });

					foreach (var option in generation.Options)
						PutSequenceOption(option, true, false);

					break;

				case AlterColumnAction.IdentityOptions options:
					CheckSequenceOptions(options, options.Options);

					foreach (var option in options.Options)
						PutSequenceOption(option, true, false);

					break;

				case AlterColumnAction.DropIdentity:
					Word("DROP IDENTITY");
					break;

				case AlterColumnAction.DropExpression:
					Word("DROP EXPRESSION");
					break;

				default:
					if (!TryPutTransactSql(action))
						throw Unwritten(action);

					break;
			}
		}

		// A sequence's options stand in any order, each group once: a value and its NO are one group, and so are CACHE and NO CACHE.
		static void CheckSequenceOptions(ISqlNode owner, SqlList<SequenceOption> options)
		{
			var groups = new HashSet<string>();

			foreach (var option in options)
			{
				var group = option switch
				{
					SequenceOption.DataTypeOption => "AS",
					SequenceOption.Start          => "START",
					SequenceOption.Increment      => "INCREMENT",
					SequenceOption.Max            => "MAXVALUE",
					SequenceOption.Min            => "MINVALUE",
					SequenceOption.Cycle          => "CYCLE",
					SequenceOption.Restart        => "RESTART",
					SequenceOption.Cache or SequenceOption.NoCache => "CACHE",
					_                             => option.GetType().Name,
				};

				if (!groups.Add(group))
					throw Misplaced(owner, "Options", "with two of one group (" + group + "), which the statement reads once");
			}
		}

		// An identity column's altered options say `SET` before each but a restart. `sequence` says the options are a sequence's own,
		// which alone has a cache.
		void PutSequenceOption(SequenceOption option, bool altered, bool sequence)
		{
			if (altered && option is not SequenceOption.Restart)
				Word("SET");

			switch (option)
			{
				case SequenceOption.DataTypeOption type:
					Word("AS");
					PutType(type.Type);
					break;

				case SequenceOption.Start start:
					Word("START WITH");
					PutSignedNumber(start, start.Value);
					break;

				case SequenceOption.Increment increment:
					Word("INCREMENT BY");
					PutSignedNumber(increment, increment.Value);
					break;

				case SequenceOption.Max max:
					if (max.No)
						Word("NO MAXVALUE");
					else
					{
						Word("MAXVALUE");
						PutSignedNumber(max, max.Value!);
					}

					break;

				case SequenceOption.Min min:
					if (min.No)
						Word("NO MINVALUE");
					else
					{
						Word("MINVALUE");
						PutSignedNumber(min, min.Value!);
					}

					break;

				case SequenceOption.Cycle cycle:
					Word(cycle.Value ? "CYCLE" : "NO CYCLE");
					break;

				case SequenceOption.Restart restart:
					Word("RESTART");

					if (restart.Value is { } value)
					{
						Word("WITH");
						PutSignedNumber(restart, value);
					}

					break;

				// T-SQL: CREATE SEQUENCE (Transact-SQL). A sequence's cache, which an identity column has none of.
				case SequenceOption.Cache cache when sequence:
					Word("CACHE");

					// An unsigned exact number: not a name, a variable, an expression or an exponent.
					if (cache.Size is { } size)
					{
						if (!IsUnsigned(size, decimals: true))
							throw Misplaced(cache, "Size", "that is not an unsigned exact number, the only value CACHE reads");

						PutExpression(size);
					}

					break;

				case SequenceOption.NoCache when sequence:
					Word("NO CACHE");
					break;

				case SequenceOption.Cache or SequenceOption.NoCache:
					throw Misplaced(option, "Cache", "on an identity column, which has no cache");

				default:
					throw Unwritten(option);
			}
		}

		// ── §12 Privileges ─────────────────────────────────────────────────────────

		void PutPrivileges(SqlList<Privilege> privileges, PrivilegeObject? on)
		{
			Each(privileges, privilege =>
			{
				Word(Spelled(privilege));

				if (privilege.Columns.Count > 0)
				{
					Open();
					Each(privilege.Columns, PutIdentifier);
					Close();
				}
				else if (privilege.Methods.Count > 0)
				{
					Open();
					Each(privilege.Methods, one => PutDesignator(one));
					Close();
				}
			});

			// T-SQL's ON is optional: a server permission names no securable.
			if (on is null)
				return;

			Word("ON");

			switch (on)
			{
				// T-SQL's class, `SCHEMA::s`, the word before the name.
				case PrivilegeObject.Classed classed:
					Word(PermissionWords.Text(classed.Class) ?? throw NoText(classed.Class));
					Tight("::");
					Hold();
					PutName(classed.Name);
					PutSecurableColumns(classed.Columns);
					break;

				case PrivilegeObject.Named named:
					PutObjectName(named);
					break;

				default:
					throw Unwritten(on);
			}
		}

		void PutObjectName(PrivilegeObject.Named on)
		{
			// The designator is the routine's name, and is written for a routine alone.
			if ((on.Kind == PrivilegeObjectKind.Routine) != (on.Routine is not null))
				throw Misplaced(on, "Routine", on.Routine is null ? "missing on a routine" : "on an object that is not a routine");

			switch (on.Kind)
			{
				case PrivilegeObjectKind.Routine:
					PutDesignator(on.Routine!);
					return;

				case PrivilegeObjectKind.Table:
					if (on.TableKeyword)
						Word("TABLE");

					break;

				default:
					Word(on.Kind switch
					{
						PrivilegeObjectKind.Domain       => "DOMAIN",
						PrivilegeObjectKind.Collation    => "COLLATION",
						PrivilegeObjectKind.CharacterSet => "CHARACTER SET",
						PrivilegeObjectKind.Translation  => "TRANSLATION",
						PrivilegeObjectKind.Type         => "TYPE",
						PrivilegeObjectKind.Sequence     => "SEQUENCE",
						// Table and Routine are the cases above this default.
						_                                => throw NoText(on.Kind),
					});
					break;
			}

			PutName(on.Name);
			PutSecurableColumns(on.Columns);
		}

		/// <summary>
		/// T-SQL's column list on the SECURABLE, <c>ON t (a, b)</c>, which is not a permission's.
		/// </summary>
		void PutSecurableColumns(SqlList<Identifier>? columns)
		{
			if (columns is { Count: > 0 } list)
			{
				Open();
				Each(list, PutIdentifier);
				Close();
			}
		}

		/// <summary>
		/// A privilege's words, or T-SQL's shorter ones where it was written so.
		/// </summary>
		static string Spelled(Privilege privilege)
		{
			if (privilege.Abbreviated)
				return privilege.Kind switch
				{
					PrivilegeKind.AllPrivileges => "ALL",
					PrivilegeKind.Execute       => "EXEC",
					// No other permission has a second spelling, so the flag says nothing a text can keep.
					_                           => throw NoText(privilege.Kind),
				};

			// The words are the catalogue's, written by permissions.py beside the enum (D148).
			return PermissionWords.Text(privilege.Kind) ?? throw NoText(privilege.Kind);
		}

		/// <summary>T-SQL's <c>AS principal</c>, the principal whose authority the statement is under.</summary>
		void PutAsPrincipal(Identifier? principal)
		{
			if (principal is null)
				return;

			Word("AS");
			PutIdentifier(principal);
		}

		void PutGrantee(Grantee grantee)
		{
			switch (grantee)
			{
				case Grantee.Identifier identifier: PutIdentifier(identifier.Value.Name); break;
				case Grantee.Public:                Word("PUBLIC"); break;
				// T-SQL's: the engine reads NULL among the principals.
				case Grantee.Null:                  Word("NULL"); break;
				default:                            throw Unwritten(grantee);
			}
		}

		void PutGrantedBy(Grantor? grantor)
		{
			if (grantor is { } one)
				Word(one switch { Grantor.CurrentUser => "GRANTED BY CURRENT_USER", Grantor.CurrentRole => "GRANTED BY CURRENT_ROLE", _ => throw NoText(one) });
		}

		// ── §11.49 Triggers ────────────────────────────────────────────────────────

		void PutTrigger(Statement.CreateTrigger trigger)
		{
			// T-SQL's trigger, or the standard's checked for its Event, Table and Action, which T-SQL's leaves out.
			if (PutTransactSqlTrigger(trigger))
				return;

			Word("CREATE TRIGGER");
			PutName(trigger.Name);
			Word(trigger.Time switch { TriggerTime.Before => "BEFORE", TriggerTime.After => "AFTER", TriggerTime.InsteadOf => "INSTEAD OF", _ => throw NoText(trigger.Time) });
			var @event = trigger.Event!;

			Word(@event.Kind switch { TriggerEventKind.Insert => "INSERT", TriggerEventKind.Delete => "DELETE", TriggerEventKind.Update => "UPDATE", _ => throw NoText(@event.Kind) });

			if (@event.Columns.Count > 0)
			{
				Word("OF");
				Each(@event.Columns, PutIdentifier);
			}

			Word("ON");
			PutName(trigger.Table!.Value);

			if (trigger.Referencing.Count > 0)
			{
				Word("REFERENCING");

				foreach (var reference in trigger.Referencing)
				{
					var (old, table) = reference.Kind switch
					{
						TransitionKind.OldRow   => (true, false),
						TransitionKind.NewRow   => (false, false),
						TransitionKind.OldTable => (true, true),
						TransitionKind.NewTable => (false, true),
						_                       => throw NoText(reference.Kind),
					};

					Word(old ? "OLD" : "NEW");

					if (table)
						Word("TABLE");
					else if (reference.RowKeyword)
						Word("ROW");

					if (reference.AsKeyword)
						Word("AS");

					PutIdentifier(reference.Name);
				}
			}

			var action = trigger.Action!;

			if (action.Granularity is { } granularity)
				Word(granularity switch { TriggerGranularity.Row => "FOR EACH ROW", TriggerGranularity.Statement => "FOR EACH STATEMENT", _ => throw NoText(granularity) });

			if (action.When is { } when)
			{
				Word("WHEN");
				Open();
				PutCondition(action, when);
				Close();
			}

			if (action.AtomicBlock)
			{
				Word("BEGIN ATOMIC");

				foreach (var statement in action.Statements)
				{
					PutStatement(statement);
					Tight(";");
				}

				Word("END");
			}
			else
				PutStatement(action.Statements[0]);
		}

		// ── §11 Routines ───────────────────────────────────────────────────────────

		void PutRoutine(RoutineDefinition routine)
		{
			switch (routine.Kind)
			{
				case RoutineKind.Procedure:
					Word("PROCEDURE");
					PutName(routine.Name);
					PutParameters(routine.Parameters);
					PutCharacteristics(routine.Characteristics);
					break;

				case RoutineKind.Function:
					Word("FUNCTION");
					PutName(routine.Name);
					PutParameters(routine.Parameters);
					PutReturns(routine.Returns ?? throw Empty(routine, "Returns"));
					PutCharacteristics(routine.Characteristics);

					if (routine.StaticDispatch)
						Word("STATIC DISPATCH");

					break;

				case RoutineKind.Method:
					if (routine.SpecificMethod)
					{
						Word("SPECIFIC METHOD");
						PutName(routine.Name);
						break;
					}

					PutModifier(routine.MethodModifier);
					Word("METHOD");
					PutName(routine.Name);
					PutParameters(routine.Parameters);

					if (routine.Returns is { } returns)
						PutReturns(returns);

					Word("FOR");
					PutName(routine.ForType!.Value);
					break;

				default:
					throw NoText(routine.Kind);
			}

			PutBody(routine.Body);
		}

		void PutModifier(MethodModifier? modifier)
		{
			if (modifier is { } one)
				Word(one switch { MethodModifier.Instance => "INSTANCE", MethodModifier.Static => "STATIC", MethodModifier.Constructor => "CONSTRUCTOR", _ => throw NoText(one) });
		}

		void PutParameters(SqlList<ParameterDefinition> parameters)
		{
			Open();
			Each(parameters, parameter =>
			{
				if (parameter.Mode is { } mode)
					Word(mode switch { ParameterMode.In => "IN", ParameterMode.Out => "OUT", ParameterMode.InOut => "INOUT", _ => throw NoText(mode) });

				if (parameter.Name is { } name)
					PutIdentifier(name);

				PutType(parameter.Type);

				if (parameter.Locator)
					Word("AS LOCATOR");

				if (parameter.Result)
					Word("RESULT");

				PutDefault(parameter.Default);
			});
			Close();
		}

		void PutReturns(ReturnsDefinition returns)
		{
			Word("RETURNS");

			if (returns.OnlyPassThrough)
			{
				Word("ONLY PASS THROUGH");
				return;
			}

			if (returns.TableKeyword)
			{
				Word("TABLE");

				if (returns.TableColumns is { } columns)
				{
					Open();
					Each(columns, column =>
					{
						PutIdentifier(column.Name);
						PutType(column.Type);
					});
					Close();
				}

				return;
			}

			PutType(returns.Type!);

			if (returns.Locator)
				Word("AS LOCATOR");

			if (returns.CastFrom is { } from)
			{
				Word("CAST FROM");
				PutType(from);

				if (returns.CastFromLocator)
					Word("AS LOCATOR");
			}
		}

		void PutCharacteristics(SqlList<RoutineCharacteristic> characteristics)
		{
			foreach (var characteristic in characteristics)
				PutCharacteristic(characteristic);
		}

		void PutCharacteristic(RoutineCharacteristic characteristic)
		{
			switch (characteristic)
			{
				case RoutineCharacteristic.Language language:
					Word("LANGUAGE");
					Word(language.Name);
					break;

				case RoutineCharacteristic.ParameterStyle style:
					Word(style.Style switch { ParameterStyleKind.Sql => "PARAMETER STYLE SQL", ParameterStyleKind.General => "PARAMETER STYLE GENERAL", _ => throw NoText(style.Style) });
					break;

				case RoutineCharacteristic.Specific specific:
					Word("SPECIFIC");
					PutName(specific.Name);
					break;

				case RoutineCharacteristic.Deterministic deterministic:
					Word(deterministic.Value ? "DETERMINISTIC" : "NOT DETERMINISTIC");
					break;

				case RoutineCharacteristic.DataAccess access:
					Word(access.Value switch
					{
						SqlDataAccess.NoSql        => "NO SQL",
						SqlDataAccess.ContainsSql  => "CONTAINS SQL",
						SqlDataAccess.ReadsSqlData => "READS SQL DATA",
						SqlDataAccess.ModifiesSqlData                          => "MODIFIES SQL DATA",
						_                          => throw NoText(access.Value),
					});
					break;

				case RoutineCharacteristic.NullCall call:
					Word(call.Value switch { NullCallMode.ReturnsNullOnNullInput => "RETURNS NULL ON NULL INPUT", NullCallMode.CalledOnNullInput => "CALLED ON NULL INPUT", _ => throw NoText(call.Value) });
					break;

				case RoutineCharacteristic.DynamicResultSets sets:
					Word("DYNAMIC RESULT SETS");
					Number(sets.Maximum);
					break;

				case RoutineCharacteristic.SavepointLevel level:
					Word(level.Value switch { SavepointLevelKind.New => "NEW SAVEPOINT LEVEL", SavepointLevelKind.Old => "OLD SAVEPOINT LEVEL", _ => throw NoText(level.Value) });
					break;

				default:
					Word("NAME");
					Word(((RoutineCharacteristic.ExternalName)characteristic).Name);
					break;
			}
		}

		void PutBody(RoutineBody body)
		{
			switch (body)
			{
				case RoutineBody.Sql sql:
					if (sql.Security is { } security)
						Word(security switch { SqlSecurity.Invoker => "SQL SECURITY INVOKER", SqlSecurity.Definer => "SQL SECURITY DEFINER", _ => throw NoText(security) });

					PutStatement(sql.Statement);
					break;

				case RoutineBody.External external:
					Word("EXTERNAL");

					if (external.Name is { } name)
					{
						Word("NAME");
						Word(name);
					}

					if (external.ParameterStyle is { } style)
						Word(style switch { ParameterStyleKind.Sql => "PARAMETER STYLE SQL", ParameterStyleKind.General => "PARAMETER STYLE GENERAL", _ => throw NoText(style) });

					if (external.TransformGroup is { } group)
					{
						Word("TRANSFORM GROUP");

						if (group.SingleGroup is { } single)
							PutIdentifier(single);
						else
							Each(group.Groups, one =>
							{
								PutIdentifier(one.Group);
								Word("FOR TYPE");
								PutName(one.Type);
							});
					}

					if (external.Security is { } externalSecurity)
						Word(externalSecurity switch
						{
							ExternalSecurity.Definer => "EXTERNAL SECURITY DEFINER",
							ExternalSecurity.Invoker => "EXTERNAL SECURITY INVOKER",
							ExternalSecurity.ImplementationDefined                        => "EXTERNAL SECURITY IMPLEMENTATION DEFINED",
							_                        => throw NoText(externalSecurity),
						});

					break;

				default:
					var function = ((RoutineBody.PolymorphicTableFunction)body).Body;

					if (function.PrivateParameters is { } parameters)
					{
						Word(function.PrivateDataKeyword ? "PRIVATE DATA" : "PRIVATE");
						PutParameters(parameters);
					}

					PutComponent("DESCRIBE WITH", function.Describe);
					PutComponent("START WITH", function.Start);
					PutComponent("FULFILL WITH", function.Fulfill);
					PutComponent("FINISH WITH", function.Finish);
					break;
			}
		}

		void PutComponent(string words, RoutineDesignator? designator)
		{
			if (designator is null)
				return;

			Word(words);
			PutDesignator(designator);
		}

		// `proc` is T-SQL's `DROP PROC` against `DROP PROCEDURE`: the reference publishes two words for
		// one statement, so which was written is the caller's to say and this prints it.
		void PutDesignator(RoutineDesignator designator, bool proc = false)
		{
			PutDesignatorKind(designator, proc);
			PutDesignatorName(designator);
		}

		/// <summary>What a designator names, `SPECIFIC FUNCTION`, before the name.</summary>
		void PutDesignatorKind(RoutineDesignator designator, bool proc)
		{
			if (designator.SpecificKeyword)
				Word("SPECIFIC");

			if (designator.Kind == RoutineKind.Method)
				PutModifier(designator.MethodModifier);

			Word(designator.Kind switch
			{
				RoutineKind.Routine   => "ROUTINE",
				RoutineKind.Function  => "FUNCTION",
				RoutineKind.Procedure => proc ? "PROC" : "PROCEDURE",
				RoutineKind.Method    => "METHOD",
				_                     => throw NoText(designator.Kind),
			});
		}

		/// <summary>A designator's name, with the parameter types and the type it is for where written.</summary>
		void PutDesignatorName(RoutineDesignator designator)
		{
			PutName(designator.Name);

			if (designator.ParameterTypes is { } types)
			{
				Open();
				Each(types, PutType);
				Close();
			}

			if (designator.ForType is { } forType)
			{
				Word("FOR");
				PutName(forType);
			}
		}

		// ── §11 User-defined types, casts, orderings and transforms ────────────────

		void PutTypeDefinition(UserDefinedTypeDefinition type)
		{
			Word("CREATE TYPE");
			PutName(type.Name);
			PutUnder(type.Under);

			switch (type.Representation)
			{
				case TypeRepresentation.Members members:
					Word("AS");
					Open();
					Each(members.Attributes, PutAttribute);
					Close();
					break;

				case TypeRepresentation.Type represented:
					Word("AS");
					PutType(represented.Value);
					break;
			}

			foreach (var option in type.Options)
				switch (option)
				{
					case UserTypeOption.Instantiable instantiable:
						Word(instantiable.Value ? "INSTANTIABLE" : "NOT INSTANTIABLE");
						break;

					case UserTypeOption.Final final:
						Word(final.Value ? "FINAL" : "NOT FINAL");
						break;

					case UserTypeOption.Reference { Representation: UserTypeReference.Using @using }:
						Word("REF USING");
						PutType(@using.Type);
						break;

					case UserTypeOption.Reference { Representation: UserTypeReference.FromAttributes attributes }:
						Word("REF FROM");
						Open();
						Each(attributes.Attributes, PutIdentifier);
						Close();
						break;

					case UserTypeOption.Reference:
						Word("REF IS SYSTEM GENERATED");
						break;

					default:
						var cast = (UserTypeOption.Cast)option;

						Word("CAST");
						Open();
						Word(cast.Kind switch
						{
							UserTypeCastKind.ToRef      => "SOURCE AS REF",
							UserTypeCastKind.ToDistinct => "SOURCE AS DISTINCT",
							UserTypeCastKind.ToType     => "REF AS SOURCE",
							UserTypeCastKind.ToSource                           => "DISTINCT AS SOURCE",
							_                           => throw NoText(cast.Kind),
						});
						Close();
						Word("WITH");
						PutIdentifier(cast.FunctionName);
						break;
				}

			Each(type.Methods, PutMethod);
		}

		void PutAttribute(AttributeDefinition attribute)
		{
			PutIdentifier(attribute.Name);
			PutType(attribute.Type);
			PutDefault(attribute.Default);
			PutCollate(attribute.Collation);
		}

		void PutMethod(MethodSpecification method)
		{
			if (method.Overriding)
				Word("OVERRIDING");

			PutModifier(method.Modifier);
			Word("METHOD");
			PutIdentifier(method.Name);
			PutParameters(method.Parameters);
			PutReturns(method.Returns);

			if (method.SpecificName is { } specific)
			{
				Word("SPECIFIC");
				PutName(specific);
			}

			if (method.SelfAsResult)
				Word("SELF AS RESULT");

			if (method.SelfAsLocator)
				Word("SELF AS LOCATOR");

			PutCharacteristics(method.Characteristics);
		}

		void PutTransformElement(TransformElement element)
		{
			Word(element.Direction switch { TransformDirection.ToSql => "TO SQL WITH", TransformDirection.FromSql => "FROM SQL WITH", _ => throw NoText(element.Direction) });
			PutDesignator(element.Function);
		}

		// ── §14 Cursors, §16–§20, §22, §23 ─────────────────────────────────────────

		void PutExecutableStatement(Statement statement)
		{
			switch (statement)
			{
				case Statement.OpenCursor open:
					Word("OPEN");
					PutCursor(open.Cursor);

					if (open.Using is { } @using)
					{
						Word("USING");
						PutArguments(@using);
					}

					break;

				case Statement.FetchCursor fetch:
					Word("FETCH");

					if (fetch.Orientation is { } orientation)
					{
						Word(orientation.Kind switch
						{
							FetchOrientationKind.Next     => "NEXT",
							FetchOrientationKind.Prior    => "PRIOR",
							FetchOrientationKind.First    => "FIRST",
							FetchOrientationKind.Last     => "LAST",
							FetchOrientationKind.Absolute => "ABSOLUTE",
							FetchOrientationKind.Relative                             => "RELATIVE",
							_                             => throw NoText(orientation.Kind),
						});

						// FetchRow keeps a signed number as one token.
						if (orientation.Offset is { } offset)
							PutSignedNumber(orientation, offset);
					}

					if (fetch.FromKeyword)
						Word("FROM");

					PutCursor(fetch.Cursor);

					// T-SQL's FETCH may leave INTO out, and the row goes to the client.
					if (fetch.Into is { } into)
					{
						Word("INTO");
						PutArguments(into);
					}

					break;

				case Statement.CloseCursor close:
					Word("CLOSE");
					PutCursor(close.Cursor);
					break;

				case Statement.AllocateCursor allocate:
					Word("ALLOCATE");
					PutCursor(allocate.Cursor);

					if (allocate.Properties is { } properties)
					{
						if (properties.Sensitivity is { } sensitivity)
							Word(sensitivity switch { CursorSensitivity.Sensitive => "SENSITIVE", CursorSensitivity.Insensitive => "INSENSITIVE", CursorSensitivity.Asensitive => "ASENSITIVE", _ => throw NoText(sensitivity) });

						if (properties.Scrollability is { } scrollability)
							Word(scrollability switch { CursorScrollability.Scroll => "SCROLL", CursorScrollability.NoScroll => "NO SCROLL", _ => throw NoText(scrollability) });

						Word("CURSOR");

						if (properties.Holdability is { } holdability)
							Word(holdability switch { CursorHoldability.WithHold => "WITH HOLD", CursorHoldability.WithoutHold => "WITHOUT HOLD", _ => throw NoText(holdability) });

						if (properties.Returnability is { } returnability)
							Word(returnability switch { CursorReturnability.WithReturn => "WITH RETURN", CursorReturnability.WithoutReturn => "WITHOUT RETURN", _ => throw NoText(returnability) });
					}
					else if (allocate.CursorKeyword)
						Word("CURSOR");

					Word("FOR");

					if (allocate.SourceValue is CursorAllocationSource.Prepared prepared)
						PutStatementReference(prepared.Statement);
					else
						PutDesignator(((CursorAllocationSource.Routine)allocate.SourceValue).Designator);

					break;

				case Statement.DeclareLocalTemporaryTable temporary:
					Word("DECLARE LOCAL TEMPORARY TABLE");
					PutName(temporary.Name);
					PutElements(temporary.Elements);
					PutCommit(temporary.OnCommit);
					break;

				case Statement.FreeLocator free:
					Word("FREE LOCATOR");
					Each(free.Locators, PutExpression);
					break;

				case Statement.HoldLocator hold:
					Word("HOLD LOCATOR");
					Each(hold.Locators, PutExpression);
					break;

				case Statement.Call call:
					Word("CALL");
					PutExpression(call.Invocation);
					break;

				case Statement.Return @return:
					Word("RETURN");

					// The standard writes `RETURN NULL` as a key word of its own rather than a value, which
					// `NullKeyword` keeps; T-SQL's bare `RETURN` has neither.
					if (@return.Value is { } value)
						PutExpression(value);
					else if (@return.NullKeyword)
						Word("NULL");

					break;

				case Statement.StartTransaction start:
					Word("START TRANSACTION");
					Each(start.Modes, PutTransactionMode);
					break;

				case Statement.SetTransaction set:
					Word(set.Local ? "SET LOCAL" : "SET");
					Word(set.Tran ? "TRAN" : "TRANSACTION");
					Each(set.Modes, PutTransactionMode);
					break;

				case Statement.SetConstraints constraints:
					Word("SET CONSTRAINTS");

					if (constraints.Target is ConstraintTarget.Names listed)
						Each(listed.Values, PutName);
					else
						Word("ALL");

					Word(constraints.Timing switch { ConstraintTiming.Deferred => "DEFERRED", ConstraintTiming.Immediate => "IMMEDIATE", _ => throw NoText(constraints.Timing) });
					break;

				case Statement.Savepoint savepoint:
					Word("SAVEPOINT");
					PutIdentifier(savepoint.Name);
					break;

				case Statement.ReleaseSavepoint release:
					Word("RELEASE SAVEPOINT");
					PutIdentifier(release.Name);
					break;

				case Statement.Commit commit:
					Word("COMMIT");
					PutTransactionKeyword(commit.Keyword);

					if (commit.Work)
						Word("WORK");

					PutTransactionName(commit.Name);
					PutChain(commit.Chain);

					if (commit.DelayedDurability is { } durable)
						Word(durable ? "WITH (DELAYED_DURABILITY = ON)" : "WITH (DELAYED_DURABILITY = OFF)");

					break;

				case Statement.Rollback rollback:
					// The two are different constructs and the tree may hold at most one: T-SQL's name,
					// which its page says may be a transaction or a savepoint, and the standard's
					// `TO SAVEPOINT s`, which knows. A node with both would have to lose one of them.
					if (rollback.Name is not null && rollback.ToSavepoint is not null)
						throw Both(rollback, nameof(rollback.Name), nameof(rollback.ToSavepoint));

					Word("ROLLBACK");
					PutTransactionKeyword(rollback.Keyword);

					if (rollback.Work)
						Word("WORK");

					PutTransactionName(rollback.Name);
					PutChain(rollback.Chain);

					if (rollback.ToSavepoint is { } to)
					{
						Word("TO SAVEPOINT");
						PutIdentifier(to);
					}

					break;

				case Statement.Connect connect:
					Word("CONNECT TO");

					if (connect.Target.Default)
						Word("DEFAULT");
					else
					{
						PutExpression(connect.Target.Server!);

						if (connect.Target.Name is { } connection)
						{
							Word("AS");
							PutExpression(connection);
						}

						if (connect.Target.User is { } user)
						{
							Word("USER");
							PutExpression(user);
						}
					}

					break;

				case Statement.SetConnection set:
					Word("SET CONNECTION");
					PutConnection(set.Connection);
					break;

				case Statement.Disconnect disconnect:
					Word("DISCONNECT");

					switch (disconnect.Object)
					{
						case DisconnectObject.All:
							Word("ALL");
							break;

						case DisconnectObject.Current:
							Word("CURRENT");
							break;

						default:
							PutConnection(((DisconnectObject.Connection)disconnect.Object).Value);
							break;
					}

					break;

				case Statement.SetSessionAuthorization authorization:
					Word("SET SESSION AUTHORIZATION");
					PutExpression(authorization.Value);
					break;

				case Statement.SetRole role:
					Word("SET ROLE");

					if (role.None)
						Word("NONE");
					else
						PutExpression(role.Value!);

					break;

				case Statement.SetTimeZone zone:
					Word("SET TIME ZONE");

					if (zone.Local)
						Word("LOCAL");
					else
						PutExpression(zone.Value!);

					break;

				case Statement.SetCatalog catalog:
					Word("SET CATALOG");
					PutExpression(catalog.Value);
					break;

				case Statement.SetSchema schema:
					Word("SET SCHEMA");
					PutExpression(schema.Value);
					break;

				case Statement.SetNames names:
					Word("SET NAMES");
					PutExpression(names.Value);
					break;

				case Statement.SetPath path:
					Word("SET PATH");
					PutExpression(path.Value);
					break;

				case Statement.SetTransformGroup group:
					if (group.Value.DefaultGroup)
						Word("SET DEFAULT TRANSFORM GROUP");
					else
					{
						Word("SET TRANSFORM GROUP FOR TYPE");
						PutName(group.Value.ForType!.Value);
					}

					PutExpression(group.Value.Value);
					break;

				case Statement.SetCollation collation:
					if (collation.NoCollation)
						Word("SET NO COLLATION");
					else
					{
						Word("SET COLLATION");
						PutExpression(collation.Value!);
					}

					if (collation.ForCharacterSets.Count > 0)
					{
						Word("FOR");
						Each(collation.ForCharacterSets, PutCharacterSet);
					}

					break;

				case Statement.SetSessionCharacteristics characteristics:
					Word("SET SESSION CHARACTERISTICS AS");
					Each(characteristics.Characteristics, modes =>
					{
						Word("TRANSACTION");
						Each(modes, PutTransactionMode);
					});
					break;

				case Statement.GetDiagnostics diagnostics:
					Word("GET DIAGNOSTICS");

					switch (diagnostics.Information)
					{
						case DiagnosticsInformation.Statement items:
							Each(items.Items, item =>
							{
								PutExpression(item.Target);
								Word("=");
								Word(Snake(item.Name));
							});
							break;

						case DiagnosticsInformation.Condition condition:
							Word("CONDITION");
							PutExpression(condition.Number);
							Each(condition.Items, item =>
							{
								PutExpression(item.Target);
								Word("=");
								Word(Snake(item.Name));
							});
							break;

						default:
							var all = (DiagnosticsInformation.All)diagnostics.Information;

							PutExpression(all.Target);
							Word("= ALL");

							switch (all.Qualifier)
							{
								case null:
									break;

								case AllInformationQualifier.Statement:
									Word("STATEMENT");
									break;

								case AllInformationQualifier.Condition:
									Word("CONDITION");

									if (all.ConditionNumber is { } number)
										PutExpression(number);

									break;

								default:
									throw NoText(all.Qualifier.Value);
							}

							break;
					}

					break;

				default:
					PutDynamicStatement(statement);
					break;
			}
		}

		void PutChain(ChainMode? chain)
		{
			if (chain is { } one)
				Word(one switch { ChainMode.Chain => "AND CHAIN", ChainMode.NoChain => "AND NO CHAIN", _ => throw NoText(one) });
		}

		/// <summary>T-SQL's word after COMMIT or ROLLBACK, where the page lets one be written.</summary>
		void PutTransactionKeyword(TransactionKeyword? keyword)
		{
			switch (keyword)
			{
				case null:
					break;

				case TransactionKeyword.Tran:
					Word("TRAN");
					break;

				case TransactionKeyword.Transaction:
					Word("TRANSACTION");
					break;

				default:
					throw NoText(keyword.Value);
			}
		}

		/// <summary>The transaction or savepoint a T-SQL statement names: a name, or a variable.</summary>
		void PutTransactionName(TransactionName? name)
		{
			if (name is null)
				return;

			// T-SQL: COMMIT TRANSACTION, ROLLBACK TRANSACTION (Transact-SQL). TranName: a name or a variable, never both, as
			// BEGIN and SAVE already refuse.
			if (name is { Name: not null, Variable: not null })
				throw Both(name, nameof(name.Name), nameof(name.Variable));

			// T-SQL: TranName's third form, '-'? & Lexical.Digits & ':' & two names: set together, and alone.
			if (name.Number is not null || name.Qualified is not null)
			{
				if (name.Number is not { Kind: NumericLiteralKind.DecimalInteger, Text: var number } || number.TrimStart('-').Length is 0 || number.Length - number.TrimStart('-').Length > 1 || !number.TrimStart('-').All(char.IsDigit))
					throw Misplaced(name, nameof(name.Number), "other than a whole number with a minus sign or none (TranName)");

				if (name.Qualified is not { Count: 2 } qualified || qualified.Parts.Any(static part => part.Text.Length == 0))
					throw Misplaced(name, nameof(name.Qualified), "other than a name of two parts, none empty (TranName)");

				if (name.Name is not null || name.Variable is not null)
					throw Misplaced(name, "Number and Qualified", "beside a Name or a Variable");

				Word(number);
				Word(":");
				PutName(qualified);
				return;
			}

			if (name.Name is { } named)
				PutIdentifier(named);
			else if (name.Variable is { } variable)
				PutExpression(variable);
			else
				throw Empty(name, nameof(name.Name), nameof(name.Variable));
		}

		void PutConnection(ConnectionObject connection)
		{
			if (connection is ConnectionObject.Named named)
				PutExpression(named.Name);
			else
				Word("DEFAULT");
		}

		void PutTransactionMode(TransactionMode mode)
		{
			switch (mode)
			{
				case TransactionMode.Isolation isolation:
					Word(isolation.Level switch
					{
						IsolationLevel.ReadUncommitted => "ISOLATION LEVEL READ UNCOMMITTED",
						IsolationLevel.ReadCommitted   => "ISOLATION LEVEL READ COMMITTED",
						IsolationLevel.RepeatableRead  => "ISOLATION LEVEL REPEATABLE READ",
						IsolationLevel.Serializable    => "ISOLATION LEVEL SERIALIZABLE",
						IsolationLevel.Snapshot        => "ISOLATION LEVEL SNAPSHOT",
						_                              => throw NoText(isolation.Level),
					});
					break;

				case TransactionMode.Access access:
					Word(access.Mode switch { TransactionAccess.ReadOnly => "READ ONLY", TransactionAccess.ReadWrite => "READ WRITE", _ => throw NoText(access.Mode) });
					break;

				default:
					Word("DIAGNOSTICS SIZE");
					PutExpression(((TransactionMode.DiagnosticsSize)mode).Size);
					break;
			}
		}

		/// <summary>An enum member's name, where it is the key word it was read from (`NFC`); a value no member names has none.</summary>
		static string Spelled<T>(T member) where T : struct, Enum
		{
			if (!Enum.IsDefined(typeof(T), member))
				throw NoText(member);

			return member.ToString();
		}

		/// <summary>An enum member back as the key word it was read from: `ReturnedSqlstate` is `RETURNED_SQLSTATE`.</summary>
		static string Snake<T>(T member) where T : struct, Enum
		{
			var name = Spelled(member);
			var text = new StringBuilder(name.Length + 4);

			for (var at = 0; at < name.Length; at++)
			{
				if (at > 0 && char.IsUpper(name[at]))
					text.Append('_');

				text.Append(char.ToUpperInvariant(name[at]));
			}

			return text.ToString();
		}

		// ── §20 Dynamic SQL ────────────────────────────────────────────────────────

		void PutDynamicStatement(Statement statement)
		{
			switch (statement)
			{
				case Statement.AllocateDescriptor allocate:
					Word(allocate.SqlKeyword ? "ALLOCATE SQL DESCRIPTOR" : "ALLOCATE DESCRIPTOR");
					PutDescriptor(allocate.Descriptor);

					if (allocate.Max is { } max)
					{
						Word("WITH MAX");
						PutExpression(max);
					}

					break;

				case Statement.DeallocateDescriptor deallocate:
					Word(deallocate.SqlKeyword ? "DEALLOCATE SQL DESCRIPTOR" : "DEALLOCATE DESCRIPTOR");
					PutDescriptor(deallocate.Descriptor);
					break;

				case Statement.GetDescriptor get:
					Word(get.SqlKeyword ? "GET SQL DESCRIPTOR" : "GET DESCRIPTOR");
					PutDescriptor(get.Descriptor);

					if (get.Body is DescriptorGet.Value getValue)
					{
						Word("VALUE");
						PutExpression(getValue.Index);
						Each(getValue.Items, PutRead);
					}
					else
						Each(((DescriptorGet.Header)get.Body).Items, PutRead);

					break;

				case Statement.SetDescriptor set:
					Word(set.SqlKeyword ? "SET SQL DESCRIPTOR" : "SET DESCRIPTOR");
					PutDescriptor(set.Descriptor);

					if (set.Body is DescriptorSet.Value setValue)
					{
						Word("VALUE");
						PutExpression(setValue.Index);
						Each(setValue.Items, PutWrite);
					}
					else
						Each(((DescriptorSet.Header)set.Body).Items, PutWrite);

					break;

				case Statement.CopyDescriptor copy:
					Word("COPY");

					if (copy.Body is DescriptorCopy.Item item)
					{
						PutDescriptor(item.Source);
						Word("VALUE");
						PutExpression(item.SourceIndex);
						Open();
						Each(item.Options, option => Word(option switch { DescriptorCopyOption.Name => "NAME", DescriptorCopyOption.Type => "TYPE", DescriptorCopyOption.Data => "DATA", _ => throw NoText(option) }));
						Close();
						Word("TO");
						PutDescriptor(item.Target);
						Word("VALUE");
						PutExpression(item.TargetIndex);
					}
					else
					{
						var whole = (DescriptorCopy.Whole)copy.Body;

						PutDescriptor(whole.Source);
						Word("TO");
						PutDescriptor(whole.Target);
					}

					break;

				case Statement.PipeRow pipe:
					Word("PIPE ROW");
					PutDescriptor(pipe.Descriptor);
					break;

				case Statement.Prepare prepare:
					Word("PREPARE");
					PutStatementReference(prepare.Statement);

					if (prepare.Attributes is { } attributes)
					{
						Word("ATTRIBUTES");
						PutExpression(attributes);
					}

					Word("FROM");
					PutExpression(prepare.Sql);
					break;

				case Statement.DeallocatePrepare deallocate:
					Word("DEALLOCATE PREPARE");
					PutStatementReference(deallocate.Statement);
					break;

				case Statement.Describe { Body: DescribeBody.Input input }:
					Word("DESCRIBE INPUT");
					PutStatementReference(input.Statement);
					PutUsingDescriptor(input.SqlKeyword, input.Descriptor);
					PutNesting(input.WithNesting);
					break;

				case Statement.Describe describe:
					var output = (DescribeBody.Output)describe.Body;

					Word(output.OutputKeyword ? "DESCRIBE OUTPUT" : "DESCRIBE");

					if (output.Object is DescribeObject.Cursor cursor)
					{
						Word("CURSOR");
						PutCursor(cursor.Value);
						Word("STRUCTURE");
					}
					else
						PutStatementReference(((DescribeObject.Statement)output.Object).Value);

					PutUsingDescriptor(output.SqlKeyword, output.Descriptor);
					PutNesting(output.WithNesting);
					break;

				case Statement.Execute execute:
					Word("EXECUTE");
					PutStatementReference(execute.Statement);

					if (execute.Result is { } result)
					{
						Word("INTO");
						PutArguments(result);
					}

					if (execute.Parameters is { } parameters)
					{
						Word("USING");
						PutArguments(parameters);
					}

					break;

				case Statement.ExecuteImmediate immediate:
					Word("EXECUTE IMMEDIATE");
					PutExpression(immediate.Sql);
					break;

				default:
					if (TryPutTransactSql(statement))
						break;

					throw Unwritten(statement);
			}
		}

		void PutArguments(DynamicArguments arguments)
		{
			if (arguments is DynamicArguments.Descriptor descriptor)
			{
				Word(descriptor.SqlKeyword ? "SQL DESCRIPTOR" : "DESCRIPTOR");
				PutDescriptor(descriptor.Value);
			}
			else
				Each(((DynamicArguments.Values)arguments).Items, PutExpression);
		}

		void PutUsingDescriptor(bool sql, DescriptorReference descriptor)
		{
			Word(sql ? "USING SQL DESCRIPTOR" : "USING DESCRIPTOR");
			PutDescriptor(descriptor);
		}

		void PutNesting(bool? nesting)
		{
			if (nesting is { } one)
				Word(one ? "WITH NESTING" : "WITHOUT NESTING");
		}

		void PutRead(DescriptorRead read)
		{
			PutExpression(read.Target);
			Word("=");
			Word(Snake(read.Item));
		}

		void PutWrite(DescriptorWrite write)
		{
			Word(Snake(write.Item));
			Word("=");
			PutExpression(write.Value);
		}

		void PutStatementReference(StatementReference reference)
		{
			PutScope(reference.Global, reference.Local);

			if (reference.Name is { } name)
				PutIdentifier(name);
			else
				PutExpression(reference.ExtendedName!);
		}

		void PutDescriptor(DescriptorReference descriptor)
		{
			PutScope(descriptor.Global, descriptor.Local);

			if (descriptor.Ptf)
				Word("PTF");

			if (descriptor.Name is { } name)
				PutIdentifier(name);
			else
				PutExpression(descriptor.ExtendedName!);
		}

		void PutScope(bool global, bool local)
		{
			if (global)
				Word("GLOBAL");

			if (local)
				Word("LOCAL");
		}
	}
}
