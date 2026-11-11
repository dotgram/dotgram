using System;

using DotGram.Sql.Ast;

namespace DotGram.Sql.PostgreSql;

// The factories of the generic object statements: DROP of every kind, and the shapes an object is named in.
static partial class Nodes
{
	/// <summary>
	/// A DROP of a kind named by any_name or by a name of one part: the standard's or T-SQL's record where one names the kind
	/// — TABLE, SEQUENCE, VIEW, MATERIALIZED VIEW, SCHEMA, COLLATION — and a DropObject of the kind for every other.
	/// </summary>
	public static Statement Dropped(ObjectKind kind, bool ifExists, QualifiedName first, QualifiedName[]? rest, DropBehavior? behavior)
	{
		var names = Names(first, rest);

		return kind switch
		{
			ObjectKind.Table            => new Statement.DropTable { Names = names, IfExists = ifExists, Behavior = behavior },
			ObjectKind.Sequence         => new Statement.DropSequence { Names = names, IfExists = ifExists, Behavior = behavior },
			ObjectKind.View             => new Statement.DropView { Names = names, IfExists = ifExists, Behavior = behavior },
			ObjectKind.MaterializedView => new Statement.DropView { Names = names, IfExists = ifExists, Behavior = behavior, Materialized = true },
			ObjectKind.Schema           => new Statement.DropSchema { Names = names, IfExists = ifExists, Behavior = behavior },
			ObjectKind.Collation        => new Statement.DropCollation { Names = Collations(names), IfExists = ifExists, Behavior = behavior },
			_                           => new Statement.DropObject(kind, Referenced(kind, names), ifExists, behavior),
		};
	}

	/// <summary>Each name as a collation's.</summary>
	static SqlList<CollationName> Collations(SqlList<QualifiedName> names)
	{
		var collations = new CollationName[names.Count];

		for (var at = 0; at < collations.Length; at++)
			collations[at] = new CollationName(names[at]);

		return SqlList.Own(collations);
	}

	/// <summary>A DROP of a kind named by names of one part.</summary>
	public static Statement Dropped(ObjectKind kind, bool ifExists, Identifier first, Identifier[]? rest, DropBehavior? behavior)
	{
		var names = Syntax.Listed(first, rest);
		var whole = new QualifiedName[names.Length];

		for (var at = 0; at < names.Length; at++)
			whole[at] = new QualifiedName(names[at]);

		var others = new QualifiedName[whole.Length - 1];

		System.Array.Copy(whole, 1, others, 0, others.Length);

		return Dropped(kind, ifExists, whole[0], others, behavior);
	}

	/// <summary><c>DROP INDEX CONCURRENTLY</c>.</summary>
	public static Statement DroppedConcurrently(bool ifExists, QualifiedName first, QualifiedName[]? rest, DropBehavior? behavior)
	{
		return new Statement.DropObject(ObjectKind.Index, Referenced(ObjectKind.Index, Names(first, rest)), ifExists, behavior, true);
	}

	/// <summary>Each name as an object of the kind.</summary>
	static SqlList<ObjectReference> Referenced(ObjectKind kind, SqlList<QualifiedName> names)
	{
		var references = new ObjectReference[names.Count];

		for (var at = 0; at < references.Length; at++)
			references[at] = new ObjectReference.Named(kind, names[at]);

		return SqlList.Own(references);
	}

	/// <summary><c>DROP POLICY p ON t</c>, <c>DROP RULE r ON t</c>.</summary>
	public static Statement DroppedOnTable(string word, bool ifExists, Identifier name, QualifiedName table, DropBehavior? behavior)
	{
		var kind = OnTableKindOf(word);

		return new Statement.DropObject(kind, [new ObjectReference.OnTable(kind, name, table)], ifExists, behavior);
	}

	/// <summary>DROP TYPE or DROP DOMAIN, the standard's records.</summary>
	public static Statement DroppedTypes(string word, bool ifExists, QualifiedName first, QualifiedName[]? rest, DropBehavior? behavior)
	{
		var names = Names(first, rest);

		return (word[0] | 0x20) == 't'
			? new Statement.DropType { Names = names, IfExists = ifExists, Behavior = behavior }
			: new Statement.DropDomain { Names = names, IfExists = ifExists, Behavior = behavior };
	}

	/// <summary>DROP FUNCTION, PROCEDURE, ROUTINE or AGGREGATE: the standard's DropRoutine, each designator of the kind.</summary>
	public static Statement DroppedRoutines(string word, bool ifExists, RoutineDesignator first, RoutineDesignator[]? rest, DropBehavior? behavior)
	{
		var kind     = RoutineKindOf(word);
		var routines = Syntax.Listed(first, rest);

		for (var at = 0; at < routines.Length; at++)
			routines[at] = routines[at] with { Kind = kind };

		return new Statement.DropRoutine { Routines = SqlList.Own(routines), IfExists = ifExists, Behavior = behavior };
	}

	/// <summary>DROP OPERATOR, each operator with its operands' types.</summary>
	public static Statement DroppedOperators(bool ifExists, ObjectReference first, ObjectReference[]? rest, DropBehavior? behavior)
	{
		return new Statement.DropObject(ObjectKind.Operator, SqlList.Own(Syntax.Listed(first, rest)), ifExists, behavior);
	}

	/// <summary>DROP OPERATOR CLASS or FAMILY.</summary>
	public static Statement DroppedClass(string word, bool ifExists, QualifiedName name, Identifier method, DropBehavior? behavior)
	{
		var kind = ClassKindOf(word);

		return new Statement.DropObject(kind, [new ObjectReference.OperatorClass(kind, name, method)], ifExists, behavior);
	}

	/// <summary>DROP ROLE, USER or GROUP: the standard's DropRole, with the word written.</summary>
	public static Statement DroppedRoles(string word, bool ifExists, Identifier first, Identifier[]? rest)
	{
		return new Statement.DropRole
		{
			Names    = SqlList.Own(Syntax.Listed(first, rest)),
			IfExists = ifExists,
			Word     = (word[0] | 0x20) switch { 'u' => RoleWord.User, 'g' => RoleWord.Group, _ => null },
		};
	}

	/// <summary>An argument's name, where one was written, and its type.</summary>
	public sealed record ArgumentType(Identifier? Name, DataType Type);

	/// <summary>One argument of function_with_argtypes: its mode, its name and its type.</summary>
	public static ParameterDefinition Argument(string? mode, Identifier? name, DataType type)
	{
		var words = mode?.ToUpperInvariant();

		ParameterMode? parameterMode = words switch
		{
			null       => null,
			"IN"       => ParameterMode.In,
			"OUT"      => ParameterMode.Out,
			"INOUT"    => ParameterMode.InOut,
			"VARIADIC" => ParameterMode.Variadic,
			_          => ParameterMode.InOut,
		};

		return new ParameterDefinition(parameterMode, name, type, false, null) { InOutTwoWords = parameterMode == ParameterMode.InOut && words != "INOUT" };
	}

	public static RoutineKind RoutineKindOf(string word)
	{
		return (word[0] | 0x20) switch
		{
			'f' => RoutineKind.Function,
			'p' => RoutineKind.Procedure,
			'a' => RoutineKind.Aggregate,
			_   => RoutineKind.Routine,
		};
	}

	public static ObjectKind ClassKindOf(string word)
	{
		return (word[0] | 0x20) == 'c' ? ObjectKind.OperatorClass : ObjectKind.OperatorFamily;
	}

	public static ObjectKind OnTableKindOf(string word)
	{
		return (word[0] | 0x20) switch { 'p' => ObjectKind.Policy, 'r' => ObjectKind.Rule, _ => ObjectKind.Trigger };
	}
}
