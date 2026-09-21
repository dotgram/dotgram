using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		// ── §11 Schema definition and manipulation, §12 Access control ─────────────

		void PutOtherStatement(Statement statement)
		{
			switch (statement)
			{
				case Statement.CreateSchema schema:
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
					Word("CREATE");

					if (table.Scope is { } scope)
						Word(scope == TableScope.GlobalTemporary ? "GLOBAL TEMPORARY" : "LOCAL TEMPORARY");

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
					Each(alter.Actions, PutAlterTable);
					break;

				case Statement.DropSchema drop:
					Word("DROP SCHEMA");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.DropTable drop:
					Word("DROP TABLE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.CreateView view:
					Word(view.Recursive ? "CREATE RECURSIVE VIEW" : "CREATE VIEW");
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

					Word("AS");
					PutQuery(view.Query);

					if (view.CheckOption is { } check)
						Word(check switch { CheckOption.Cascaded => "WITH CASCADED CHECK OPTION", CheckOption.Local => "WITH LOCAL CHECK OPTION", _ => "WITH CHECK OPTION" });

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
						Word(padding == PadCharacteristic.NoPad ? "NO PAD" : "PAD SPACE");

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
						PutName(translation.Source.Existing!);

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
					PutExpression(assertion.Condition);
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
					Word("CREATE SEQUENCE");
					PutName(sequence.Name);

					foreach (var option in sequence.Options)
						PutSequenceOption(option, false);

					break;

				case Statement.AlterSequence sequence:
					Word("ALTER SEQUENCE");
					PutName(sequence.Name);

					foreach (var option in sequence.Options)
						PutSequenceOption(option, false);

					break;

				case Statement.DropSequence drop:
					Word("DROP SEQUENCE");
					PutIfExists(drop.IfExists);
					Each(drop.Names, PutName);
					PutBehavior(drop.Behavior);
					break;

				case Statement.Grant { Body: GrantBody.Privileges privileges }:
					Word("GRANT");
					PutPrivileges(privileges.Items, privileges.Object);
					Word("TO");
					Each(privileges.Grantees, PutGrantee);

					if (privileges.HierarchyOption)
						Word("WITH HIERARCHY OPTION");

					if (privileges.GrantOption)
						Word("WITH GRANT OPTION");

					PutGrantedBy(privileges.GrantedBy);
					break;

				case Statement.Grant grant:
					var roles = (GrantBody.Roles)grant.Body;

					Word("GRANT");
					Each(roles.Names, PutIdentifier);
					Word("TO");
					Each(roles.Grantees, PutGrantee);

					if (roles.AdminOption)
						Word("WITH ADMIN OPTION");

					PutGrantedBy(roles.GrantedBy);
					break;

				case Statement.Revoke { Body: RevokeBody.Privileges revoked }:
					Word("REVOKE");

					if (revoked.Option is { } revokeOption)
						Word(revokeOption == RevokeOption.GrantOptionFor ? "GRANT OPTION FOR" : "HIERARCHY OPTION FOR");

					PutPrivileges(revoked.Items, revoked.Object);
					Word("FROM");
					Each(revoked.Grantees, PutGrantee);
					PutGrantedBy(revoked.GrantedBy);
					PutBehavior(revoked.Behavior);
					break;

				case Statement.Revoke revoke:
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

				case Statement.CreateRole role:
					Word("CREATE ROLE");
					PutIdentifier(role.Name);

					if (role.Admin is { } admin)
						Word(admin == Grantor.CurrentUser ? "WITH ADMIN CURRENT_USER" : "WITH ADMIN CURRENT_ROLE");

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
					Each(drop.Names, PutName);
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
					Word(ordering.Ordering.Form == OrderingForm.EqualsOnly ? "EQUALS ONLY BY" : "ORDER FULL BY");

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
								Each(action.DropKinds, kind => Word(kind == TransformDirection.ToSql ? "TO SQL" : "FROM SQL"));
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
					Word("CREATE");
					PutRoutine(routine.Definition);
					break;

				case Statement.AlterRoutine alter:
					Word("ALTER");
					PutDesignator(alter.Routine);

					foreach (var characteristic in alter.Characteristics)
						PutCharacteristic(characteristic);

					Word("RESTRICT");
					break;

				case Statement.DropRoutine drop:
					Word("DROP");
					Each(drop.Routines, PutDesignator);
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
			Word(behavior == DropBehavior.Cascade ? "CASCADE" : "RESTRICT");
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
				Word(one == TableCommitAction.Preserve ? "ON COMMIT PRESERVE ROWS" : "ON COMMIT DELETE ROWS");
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
			PutName(under);
		}

		void PutGeneration(ReferenceGeneration? generation)
		{
			if (generation is { } one)
				Word(one switch { ReferenceGeneration.SystemGenerated => "SYSTEM GENERATED", ReferenceGeneration.UserGenerated => "USER GENERATED", _ => "DERIVED" });
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

					if (query.Columns.Count > 0)
					{
						Open();
						Each(query.Columns, PutIdentifier);
						Close();
					}

					Word("AS");
					PutSubquery(query.Query);
					Word(query.Data == WithDataMode.WithData ? "WITH DATA" : "WITH NO DATA");
					break;
			}
		}

		void PutElements(IReadOnlyList<TableElement> elements)
		{
			Open();
			Each(elements, PutElement);
			Close();
		}

		void PutElement(TableElement element)
		{
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
							_                             => "EXCLUDING GENERATED",
						});

					break;

				case TableElement.SelfReference self:
					Word("REF IS");
					PutIdentifier(self.Name);
					PutGeneration(self.Generation);
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

			if (period.ApplicationName is { } name)
				PutIdentifier(name);
			else
				Word("SYSTEM_TIME");

			Open();
			PutIdentifier(period.BeginColumn);
			Tight(",");
			PutIdentifier(period.EndColumn);
			Close();
		}

		void PutColumn(ColumnDefinition column)
		{
			PutIdentifier(column.Name);

			if (column.Type is { } type)
				PutType(type);

			switch (column.Generation)
			{
				case null:
					break;

				case ColumnGeneration.Default @default:
					PutDefault(@default.Value);
					break;

				case ColumnGeneration.Identity identity:
					Word(identity.Generation == IdentityGeneration.Always ? "GENERATED ALWAYS AS IDENTITY" : "GENERATED BY DEFAULT AS IDENTITY");

					if (identity.Options.Count > 0)
					{
						Open();

						foreach (var option in identity.Options)
							PutSequenceOption(option, false);

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
				PutName(name);
			}

			switch (constraint)
			{
				case Constraint.NotNull:
					Word("NOT NULL");
					break;

				case Constraint.Unique { Kind: UniqueKind.UniqueValue }:
					Word("UNIQUE");
					Open();
					Word("VALUE");
					Close();
					break;

				case Constraint.Unique unique:
					Word(unique.Kind == UniqueKind.Unique ? "UNIQUE" : "PRIMARY KEY");

					if (unique.Nulls is { } nulls)
						Word(nulls == NullDistinctness.Distinct ? "NULLS DISTINCT" : "NULLS NOT DISTINCT");

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

				default:
					Word("CHECK");
					Open();
					PutExpression(((Constraint.Check)constraint).Condition);
					Close();
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
				Word(match switch { MatchType.Full => "MATCH FULL", MatchType.Partial => "MATCH PARTIAL", _ => "MATCH SIMPLE" });

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
				_                            => "NO ACTION",
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
				Word(one == ConstraintTiming.InitiallyDeferred ? "INITIALLY DEFERRED" : "INITIALLY IMMEDIATE");
		}

		void PutAlterTable(AlterTableAction action)
		{
			switch (action)
			{
				case AlterTableAction.AddColumn add:
					Word(add.ColumnKeyword ? "ADD COLUMN" : "ADD");
					PutColumn(add.Column);
					break;

				case AlterTableAction.AlterColumn alter:
					Word(alter.ColumnKeyword ? "ALTER COLUMN" : "ALTER");
					PutIdentifier(alter.Column);
					PutAlterColumn(alter.Action);
					break;

				case AlterTableAction.DropColumn drop:
					Word(drop.ColumnKeyword ? "DROP COLUMN" : "DROP");
					PutIdentifier(drop.Column);
					PutBehavior(drop.Behavior);
					break;

				case AlterTableAction.AddConstraint add:
					Word("ADD");
					PutConstraint(add.Constraint);
					break;

				case AlterTableAction.AlterConstraint alter:
					Word("ALTER CONSTRAINT");
					PutName(alter.Name);
					Word(alter.Enforced ? "ENFORCED" : "NOT ENFORCED");
					break;

				case AlterTableAction.DropConstraint drop:
					Word("DROP CONSTRAINT");
					PutName(drop.Name);
					PutBehavior(drop.Behavior);
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
					Word("DROP PERIOD FOR");

					if (drop.ApplicationName is { } name)
						PutIdentifier(name);
					else
						Word("SYSTEM_TIME");

					PutBehavior(drop.Behavior);
					break;

				case AlterTableAction.AddSystemVersioning:
					Word("ADD SYSTEM VERSIONING");
					break;

				case AlterTableAction.DropSystemVersioning drop:
					Word("DROP SYSTEM VERSIONING");
					PutBehavior(drop.Behavior);
					break;

				default:
					throw Unwritten(action);
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
					Word("SET DATA TYPE");
					PutType(type.Type);
					break;

				case AlterColumnAction.SetIdentityGeneration generation:
					Word(generation.Generation == IdentityGeneration.Always ? "SET GENERATED ALWAYS" : "SET GENERATED BY DEFAULT");

					foreach (var option in generation.Options)
						PutSequenceOption(option, true);

					break;

				case AlterColumnAction.IdentityOptions options:
					foreach (var option in options.Options)
						PutSequenceOption(option, true);

					break;

				case AlterColumnAction.DropIdentity:
					Word("DROP IDENTITY");
					break;

				default:
					Word("DROP EXPRESSION");
					break;
			}
		}

		// An identity column's altered options say `SET` before each but a restart.
		void PutSequenceOption(SequenceOption option, bool altered)
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
					PutExpression(start.Value);
					break;

				case SequenceOption.Increment increment:
					Word("INCREMENT BY");
					PutExpression(increment.Value);
					break;

				case SequenceOption.Max max:
					if (max.No)
						Word("NO MAXVALUE");
					else
					{
						Word("MAXVALUE");
						PutExpression(max.Value!);
					}

					break;

				case SequenceOption.Min min:
					if (min.No)
						Word("NO MINVALUE");
					else
					{
						Word("MINVALUE");
						PutExpression(min.Value!);
					}

					break;

				case SequenceOption.Cycle cycle:
					Word(cycle.Value ? "CYCLE" : "NO CYCLE");
					break;

				default:
					Word("RESTART");

					if (((SequenceOption.Restart)option).Value is { } value)
					{
						Word("WITH");
						PutExpression(value);
					}

					break;
			}
		}

		// ── §12 Privileges ─────────────────────────────────────────────────────────

		void PutPrivileges(IReadOnlyList<Privilege> privileges, PrivilegeObject on)
		{
			if (privileges is [{ Kind: PrivilegeKind.AllPrivileges }])
				Word("ALL PRIVILEGES");
			else
				Each(privileges, privilege =>
				{
					Word(privilege.Kind switch
					{
						PrivilegeKind.Select     => "SELECT",
						PrivilegeKind.Delete     => "DELETE",
						PrivilegeKind.Insert     => "INSERT",
						PrivilegeKind.Update     => "UPDATE",
						PrivilegeKind.References => "REFERENCES",
						PrivilegeKind.Usage      => "USAGE",
						PrivilegeKind.Trigger    => "TRIGGER",
						PrivilegeKind.Under      => "UNDER",
						_                        => "EXECUTE",
					});

					if (privilege.Columns.Count > 0)
					{
						Open();
						Each(privilege.Columns, PutIdentifier);
						Close();
					}
					else if (privilege.Methods.Count > 0)
					{
						Open();
						Each(privilege.Methods, PutDesignator);
						Close();
					}
				});

			Word("ON");

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
						_                                => "SEQUENCE",
					});
					break;
			}

			PutName(on.Name);
		}

		void PutGrantee(Grantee grantee)
		{
			if (grantee is Grantee.Identifier identifier)
				PutIdentifier(identifier.Value.Name);
			else
				Word("PUBLIC");
		}

		void PutGrantedBy(Grantor? grantor)
		{
			if (grantor is { } one)
				Word(one == Grantor.CurrentUser ? "GRANTED BY CURRENT_USER" : "GRANTED BY CURRENT_ROLE");
		}

		// ── §11.49 Triggers ────────────────────────────────────────────────────────

		void PutTrigger(Statement.CreateTrigger trigger)
		{
			Word("CREATE TRIGGER");
			PutName(trigger.Name);
			Word(trigger.Time switch { TriggerTime.Before => "BEFORE", TriggerTime.After => "AFTER", _ => "INSTEAD OF" });
			Word(trigger.Event.Kind switch { TriggerEventKind.Insert => "INSERT", TriggerEventKind.Delete => "DELETE", _ => "UPDATE" });

			if (trigger.Event.Columns.Count > 0)
			{
				Word("OF");
				Each(trigger.Event.Columns, PutIdentifier);
			}

			Word("ON");
			PutName(trigger.Table);

			if (trigger.Referencing.Count > 0)
			{
				Word("REFERENCING");

				foreach (var reference in trigger.Referencing)
				{
					var old   = reference.Kind is TransitionKind.OldRow or TransitionKind.OldTable;
					var table = reference.Kind is TransitionKind.OldTable or TransitionKind.NewTable;

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

			var action = trigger.Action;

			if (action.Granularity is { } granularity)
				Word(granularity == TriggerGranularity.Row ? "FOR EACH ROW" : "FOR EACH STATEMENT");

			if (action.When is { } when)
			{
				Word("WHEN");
				Open();
				PutExpression(when);
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
					PutReturns(routine.Returns!);
					PutCharacteristics(routine.Characteristics);

					if (routine.StaticDispatch)
						Word("STATIC DISPATCH");

					break;

				default:
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
					PutName(routine.ForType!);
					break;
			}

			PutBody(routine.Body);
		}

		void PutModifier(MethodModifier? modifier)
		{
			if (modifier is { } one)
				Word(one switch { MethodModifier.Instance => "INSTANCE", MethodModifier.Static => "STATIC", _ => "CONSTRUCTOR" });
		}

		void PutParameters(IReadOnlyList<ParameterDefinition> parameters)
		{
			Open();
			Each(parameters, parameter =>
			{
				if (parameter.Mode is { } mode)
					Word(mode switch { ParameterMode.In => "IN", ParameterMode.Out => "OUT", _ => "INOUT" });

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

		void PutCharacteristics(IReadOnlyList<RoutineCharacteristic> characteristics)
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
					Word(style.Style == ParameterStyleKind.Sql ? "PARAMETER STYLE SQL" : "PARAMETER STYLE GENERAL");
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
						_                          => "MODIFIES SQL DATA",
					});
					break;

				case RoutineCharacteristic.NullCall call:
					Word(call.Value == NullCallMode.ReturnsNullOnNullInput ? "RETURNS NULL ON NULL INPUT" : "CALLED ON NULL INPUT");
					break;

				case RoutineCharacteristic.DynamicResultSets sets:
					Word("DYNAMIC RESULT SETS");
					Number(sets.Maximum);
					break;

				case RoutineCharacteristic.SavepointLevel level:
					Word(level.Value == SavepointLevelKind.New ? "NEW SAVEPOINT LEVEL" : "OLD SAVEPOINT LEVEL");
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
						Word(security == SqlSecurity.Invoker ? "SQL SECURITY INVOKER" : "SQL SECURITY DEFINER");

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
						Word(style == ParameterStyleKind.Sql ? "PARAMETER STYLE SQL" : "PARAMETER STYLE GENERAL");

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
							_                        => "EXTERNAL SECURITY IMPLEMENTATION DEFINED",
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

		void PutDesignator(RoutineDesignator designator)
		{
			if (designator.SpecificKeyword)
				Word("SPECIFIC");

			if (designator.Kind == RoutineKind.Method)
				PutModifier(designator.MethodModifier);

			Word(designator.Kind switch { RoutineKind.Routine => "ROUTINE", RoutineKind.Function => "FUNCTION", RoutineKind.Procedure => "PROCEDURE", _ => "METHOD" });
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
							_                           => "DISTINCT AS SOURCE",
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
			Word(element.Direction == TransformDirection.ToSql ? "TO SQL WITH" : "FROM SQL WITH");
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
							_                             => "RELATIVE",
						});

						if (orientation.Offset is { } offset)
							PutExpression(offset);
					}

					if (fetch.FromKeyword)
						Word("FROM");

					PutCursor(fetch.Cursor);
					Word("INTO");
					PutArguments(fetch.Into!);
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
							Word(sensitivity switch { CursorSensitivity.Sensitive => "SENSITIVE", CursorSensitivity.Insensitive => "INSENSITIVE", _ => "ASENSITIVE" });

						if (properties.Scrollability is { } scrollability)
							Word(scrollability == CursorScrollability.Scroll ? "SCROLL" : "NO SCROLL");

						Word("CURSOR");

						if (properties.Holdability is { } holdability)
							Word(holdability == CursorHoldability.WithHold ? "WITH HOLD" : "WITHOUT HOLD");

						if (properties.Returnability is { } returnability)
							Word(returnability == CursorReturnability.WithReturn ? "WITH RETURN" : "WITHOUT RETURN");
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

					if (@return.Value is { } value)
						PutExpression(value);
					else
						Word("NULL");

					break;

				case Statement.StartTransaction start:
					Word("START TRANSACTION");
					Each(start.Modes, PutTransactionMode);
					break;

				case Statement.SetTransaction set:
					Word(set.Local ? "SET LOCAL TRANSACTION" : "SET TRANSACTION");
					Each(set.Modes, PutTransactionMode);
					break;

				case Statement.SetConstraints constraints:
					Word("SET CONSTRAINTS");

					if (constraints.Target is ConstraintTarget.Names listed)
						Each(listed.Values, PutName);
					else
						Word("ALL");

					Word(constraints.Timing == ConstraintTiming.Deferred ? "DEFERRED" : "IMMEDIATE");
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

					if (commit.Work)
						Word("WORK");

					PutChain(commit.Chain);
					break;

				case Statement.Rollback rollback:
					Word("ROLLBACK");

					if (rollback.Work)
						Word("WORK");

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
						PutName(group.Value.ForType!);
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
								Word(Snake(item.Name.ToString()));
							});
							break;

						case DiagnosticsInformation.Condition condition:
							Word("CONDITION");
							PutExpression(condition.Number);
							Each(condition.Items, item =>
							{
								PutExpression(item.Target);
								Word("=");
								Word(Snake(item.Name.ToString()));
							});
							break;

						default:
							var all = (DiagnosticsInformation.All)diagnostics.Information;

							PutExpression(all.Target);
							Word("= ALL");

							if (all.Qualifier == AllInformationQualifier.Statement)
								Word("STATEMENT");
							else if (all.Qualifier == AllInformationQualifier.Condition)
							{
								Word("CONDITION");

								if (all.ConditionNumber is { } number)
									PutExpression(number);
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
				Word(one == ChainMode.Chain ? "AND CHAIN" : "AND NO CHAIN");
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
						_                              => "ISOLATION LEVEL SERIALIZABLE",
					});
					break;

				case TransactionMode.Access access:
					Word(access.Mode == TransactionAccess.ReadOnly ? "READ ONLY" : "READ WRITE");
					break;

				default:
					Word("DIAGNOSTICS SIZE");
					PutExpression(((TransactionMode.DiagnosticsSize)mode).Size);
					break;
			}
		}

		/// <summary>An enum member back as the key word it was read from: `ReturnedSqlstate` is `RETURNED_SQLSTATE`.</summary>
		static string Snake(string name)
		{
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
						Each(item.Options, option => Word(option switch { DescriptorCopyOption.Name => "NAME", DescriptorCopyOption.Type => "TYPE", _ => "DATA" }));
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
			Word(Snake(read.Item.ToString()));
		}

		void PutWrite(DescriptorWrite write)
		{
			Word(Snake(write.Item.ToString()));
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
