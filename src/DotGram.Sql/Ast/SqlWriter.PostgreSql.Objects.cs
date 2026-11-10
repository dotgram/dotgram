using System;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		/// <summary>The words a kind is named by, as gram.y's object_type_any_name, object_type_name and their kin write them.</summary>
		static string KindWords(ObjectKind kind)
		{
			return kind switch
			{
				ObjectKind.Table                   => "TABLE",
				ObjectKind.Sequence                => "SEQUENCE",
				ObjectKind.View                    => "VIEW",
				ObjectKind.MaterializedView        => "MATERIALIZED VIEW",
				ObjectKind.Index                   => "INDEX",
				ObjectKind.ForeignTable            => "FOREIGN TABLE",
				ObjectKind.Collation               => "COLLATION",
				ObjectKind.Conversion              => "CONVERSION",
				ObjectKind.Statistics              => "STATISTICS",
				ObjectKind.TextSearchParser        => "TEXT SEARCH PARSER",
				ObjectKind.TextSearchDictionary    => "TEXT SEARCH DICTIONARY",
				ObjectKind.TextSearchTemplate      => "TEXT SEARCH TEMPLATE",
				ObjectKind.TextSearchConfiguration => "TEXT SEARCH CONFIGURATION",
				ObjectKind.AccessMethod            => "ACCESS METHOD",
				ObjectKind.EventTrigger            => "EVENT TRIGGER",
				ObjectKind.Extension               => "EXTENSION",
				ObjectKind.ForeignDataWrapper      => "FOREIGN DATA WRAPPER",
				ObjectKind.Language                => "LANGUAGE",
				ObjectKind.Publication             => "PUBLICATION",
				ObjectKind.Schema                  => "SCHEMA",
				ObjectKind.Server                  => "SERVER",
				ObjectKind.Database                => "DATABASE",
				ObjectKind.Role                    => "ROLE",
				ObjectKind.Subscription            => "SUBSCRIPTION",
				ObjectKind.Tablespace              => "TABLESPACE",
				ObjectKind.Policy                  => "POLICY",
				ObjectKind.Rule                    => "RULE",
				ObjectKind.Trigger                 => "TRIGGER",
				ObjectKind.Constraint              => "CONSTRAINT",
				ObjectKind.Column                  => "COLUMN",
				ObjectKind.Type                    => "TYPE",
				ObjectKind.Domain                  => "DOMAIN",
				ObjectKind.Aggregate               => "AGGREGATE",
				ObjectKind.Function                => "FUNCTION",
				ObjectKind.Procedure               => "PROCEDURE",
				ObjectKind.Routine                 => "ROUTINE",
				ObjectKind.Operator                => "OPERATOR",
				ObjectKind.OperatorClass           => "OPERATOR CLASS",
				ObjectKind.OperatorFamily          => "OPERATOR FAMILY",
				ObjectKind.Cast                    => "CAST",
				ObjectKind.Transform               => "TRANSFORM",
				ObjectKind.LargeObject             => "LARGE OBJECT",
				_                                  => throw NoText(kind),
			};
		}

		/// <summary>The kinds gram.y names by any_name: a name of any number of parts.</summary>
		static bool NamedByAnyName(ObjectKind kind)
		{
			return kind is >= ObjectKind.Table and <= ObjectKind.TextSearchConfiguration;
		}

		/// <summary>The kinds gram.y names by a name of one part (object_type_name).</summary>
		static bool NamedByName(ObjectKind kind)
		{
			return kind is >= ObjectKind.AccessMethod and <= ObjectKind.Tablespace;
		}

		/// <summary>
		/// PostgreSQL's DROP of a kind no other record names: the kind, CONCURRENTLY for an index, IF EXISTS, the objects and the
		/// behavior. A kind with a record of its own is refused, since its text would read back as that record.
		/// </summary>
		void PutDropObject(Statement.DropObject drop)
		{
			OnlyPostgreSql(drop);

			if (drop.Objects.Count == 0)
				throw Empty(drop, nameof(drop.Objects));

			switch (drop.Kind)
			{
				case ObjectKind.Table or ObjectKind.Sequence or ObjectKind.View or ObjectKind.MaterializedView or ObjectKind.Schema or ObjectKind.Type or ObjectKind.Domain
					or ObjectKind.Function or ObjectKind.Procedure or ObjectKind.Routine or ObjectKind.Aggregate or ObjectKind.Trigger or ObjectKind.Role or ObjectKind.Database:
					throw Misplaced(drop, nameof(drop.Kind), "a kind whose DROP is a record of its own, which the text reads back as");

				case ObjectKind.Column or ObjectKind.Constraint or ObjectKind.LargeObject:
					throw Misplaced(drop, nameof(drop.Kind), "a kind that has no DROP of its own");
			}

			if (drop.Concurrently && drop.Kind != ObjectKind.Index)
				throw Misplaced(drop, nameof(drop.Concurrently), "on a kind other than an index");

			// One object, not a list: a cast, a transform, an operator class or family, an object on a table, a tablespace, a subscription.
			if (drop.Objects.Count > 1 && drop.Kind is ObjectKind.Cast or ObjectKind.Transform or ObjectKind.OperatorClass or ObjectKind.OperatorFamily
				or ObjectKind.Policy or ObjectKind.Rule or ObjectKind.Tablespace or ObjectKind.Subscription)
				throw Misplaced(drop, nameof(drop.Objects), "of more than one, which DROP of this kind does not read");

			if (drop.Kind == ObjectKind.Tablespace && drop.Behavior is not null)
				throw Misplaced(drop, nameof(drop.Behavior), "on DROP TABLESPACE, which reads none");

			Word("DROP");
			Word(KindWords(drop.Kind));

			if (drop.Concurrently)
				Word("CONCURRENTLY");

			PutIfExists(drop.IfExists);
			Each(drop.Objects, one => PutReferenced(drop, drop.Kind, one));
			PutBehavior(drop.Behavior);
		}

		/// <summary>One object as DROP names it after its kind's words: the shape the kind is named in, and nothing else.</summary>
		void PutReferenced(ISqlNode owner, ObjectKind kind, ObjectReference reference)
		{
			switch (reference)
			{
				case ObjectReference.Named named when named.Kind == kind && (NamedByAnyName(kind) || NamedByName(kind) && named.Name.Count == 1):
					PutName(named.Name);
					break;

				case ObjectReference.OnTable on when on.Kind == kind && kind is ObjectKind.Policy or ObjectKind.Rule or ObjectKind.Trigger or ObjectKind.Constraint:
					if (on.DomainWord && kind != ObjectKind.Constraint)
						throw Misplaced(on, nameof(on.DomainWord), "on an object other than a constraint");

					PutIdentifier(on.Name);
					Word("ON");

					if (on.DomainWord)
						Word("DOMAIN");

					PutName(on.Table);
					break;

				case ObjectReference.Operator @operator when kind == ObjectKind.Operator:
					PutPostgreSqlOperatorName(@operator, @operator.Name);
					Open();
					PutOperandType(@operator.Left);
					Tight(",");
					PutOperandType(@operator.Right);
					Close();
					break;

				case ObjectReference.Cast cast when kind == ObjectKind.Cast:
					Open();
					PutType(cast.From);
					Word("AS");
					PutType(cast.To);
					Close();
					break;

				case ObjectReference.OperatorClass operatorClass when operatorClass.Kind == kind && kind is ObjectKind.OperatorClass or ObjectKind.OperatorFamily:
					PutName(operatorClass.Name);
					Word("USING");
					PutIdentifier(operatorClass.Method);
					break;

				case ObjectReference.Transform transform when kind == ObjectKind.Transform:
					Word("FOR");
					PutType(transform.Type);
					Word("LANGUAGE");
					PutIdentifier(transform.Language);
					break;

				default:
					throw Misplaced(owner, "Objects", "with an object of another kind or shape than the statement's kind names");
			}
		}

		/// <summary>An operator's name as any_operator: the schemas and the symbol, which must be one PostgreSQL reads as an operator.</summary>
		void PutPostgreSqlOperatorName(ISqlNode owner, QualifiedName name)
		{
			var symbol = name.Last.Text;

			if (!PostgreSql.Nodes.IsOperatorRun(symbol))
				throw Misplaced(owner, "Name", "whose last part is no operator's symbol");

			PutName(name);
		}

		/// <summary>An operand's type, or NONE for the operand a prefix operator has not.</summary>
		void PutOperandType(DataType? type)
		{
			if (type is null)
				Word("NONE");
			else
				PutType(type);
		}

		/// <summary>PostgreSQL's COMMENT ON: the object with its kind's words, IS, and the text or NULL.</summary>
		void PutComment(Statement.Comment comment)
		{
			OnlyPostgreSql(comment);
			Word("COMMENT ON");

			switch (comment.Object)
			{
				case ObjectReference.Named named:
					if (named.Kind is ObjectKind.Column or ObjectKind.Type or ObjectKind.Domain || NamedByAnyName(named.Kind) || NamedByName(named.Kind))
					{
						if (NamedByName(named.Kind) && named.Name.Count != 1)
							throw Misplaced(named, nameof(named.Name), "of more than one part, where the kind is named by one");

						if (named.Kind == ObjectKind.Column && named.Name.Count < 2)
							throw Misplaced(named, nameof(named.Name), "of one part, where a column is named with its table");

						Word(KindWords(named.Kind));
						PutName(named.Name);
						break;
					}

					throw Misplaced(named, nameof(named.Kind), "that is not named by a name alone");

				case ObjectReference.Routine routine:
					if (routine.Designator.Kind is not (RoutineKind.Function or RoutineKind.Procedure or RoutineKind.Routine or RoutineKind.Aggregate))
						throw Misplaced(routine, nameof(routine.Designator), "of a kind PostgreSQL does not comment on");

					PutDesignator(routine.Designator);
					break;

				case ObjectReference.OnTable on:
					Word(KindWords(on.Kind));
					PutReferenced(comment, on.Kind, on);
					break;

				case ObjectReference.Operator @operator:
					Word("OPERATOR");
					PutReferenced(comment, ObjectKind.Operator, @operator);
					break;

				case ObjectReference.OperatorClass operatorClass:
					Word(KindWords(operatorClass.Kind));
					PutReferenced(comment, operatorClass.Kind, operatorClass);
					break;

				case ObjectReference.Transform transform:
					Word("TRANSFORM");
					PutReferenced(comment, ObjectKind.Transform, transform);
					break;

				case ObjectReference.LargeObject large:
					if (large.Id is not (LiteralValue.Numeric or Expression.Unary { Operand: LiteralValue.Numeric }))
						throw Misplaced(large, nameof(large.Id), "other than a number");

					Word("LARGE OBJECT");
					PutExpression(large.Id);
					break;

				case ObjectReference.Cast cast:
					Word("CAST");
					PutReferenced(comment, ObjectKind.Cast, cast);
					break;

				default:
					throw Unwritten(comment.Object);
			}

			Word("IS");

			if (comment.Text is { } text)
				PutLiteral(text);
			else
				Word("NULL");
		}
	}
}
