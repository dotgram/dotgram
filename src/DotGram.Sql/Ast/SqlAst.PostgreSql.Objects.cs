using System;

namespace DotGram.Sql.Ast;

// PostgreSQL's generic object statements: the statements that differ only by the word that names their object share a
// record that says the kind (DropObject, Comment), and every object they name is an ObjectReference. Where the standard or
// T-SQL has a record of its own for a kind — DROP TABLE, DROP VIEW, DROP FUNCTION — that record is built instead, so a
// text two parsers read is one tree; the generic records carry the kinds no other record names.

/// <summary>
/// PostgreSQL: DROP, COMMENT. The kinds of object the generic statements name, in the words gram.y's object_type_any_name,
/// drop_type_name, object_type_name and object_type_name_on_any_name list, and the kinds COMMENT names besides.
/// </summary>
public enum ObjectKind
{
	Table, Sequence, View, MaterializedView, Index, ForeignTable, Collation, Conversion, Statistics,
	TextSearchParser, TextSearchDictionary, TextSearchTemplate, TextSearchConfiguration,
	AccessMethod, EventTrigger, Extension, ForeignDataWrapper, Language, Publication, Schema, Server,
	Database, Role, Subscription, Tablespace,
	Policy, Rule, Trigger, Constraint,
	Column, Type, Domain, Aggregate, Function, Procedure, Routine, Operator, OperatorClass, OperatorFamily,
	Cast, Transform, LargeObject,
}

/// <summary>PostgreSQL: DROP, COMMENT. One object a generic statement names, in the shape its kind is named in.</summary>
public abstract record ObjectReference : ISqlNode
{
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	private protected ObjectReference()
	{
	}

	/// <summary>PostgreSQL: DROP, COMMENT. An object named by its name, <c>TABLE s.t</c>, <c>COLUMN t.c</c>, <c>SCHEMA s</c>.</summary>
	public sealed record Named(ObjectKind Kind, QualifiedName Name) : ObjectReference;

	/// <summary>PostgreSQL: DROP, COMMENT. A function, a procedure, a routine or an aggregate, with its parameters where written.</summary>
	public sealed record Routine(RoutineDesignator Designator) : ObjectReference;

	/// <summary>
	/// PostgreSQL: DROP, COMMENT. An object named within a table, <c>TRIGGER tr ON t</c>, <c>CONSTRAINT c ON t</c>; a constraint
	/// of a domain is <c>CONSTRAINT c ON DOMAIN d</c>, where <see cref="DomainWord"/> says so.
	/// </summary>
	public sealed record OnTable(ObjectKind Kind, Identifier Name, QualifiedName Table) : ObjectReference
	{
		/// <summary>PostgreSQL: COMMENT. <c>ON DOMAIN d</c>: the constraint is a domain's.</summary>
		public bool DomainWord { get; init; }
	}

	/// <summary>PostgreSQL: DROP CAST, COMMENT. <c>CAST (from AS to)</c>.</summary>
	public sealed record Cast(DataType From, DataType To) : ObjectReference;

	/// <summary>
	/// PostgreSQL: DROP OPERATOR, COMMENT. <c>OPERATOR s.+ (int, int)</c>: the operator and its operands' types, null where
	/// <c>NONE</c> was written for a prefix operator's left.
	/// </summary>
	public sealed record Operator(QualifiedName Name, DataType? Left, DataType? Right) : ObjectReference;

	/// <summary>PostgreSQL: COMMENT. <c>LARGE OBJECT 12345</c>.</summary>
	public sealed record LargeObject(Expression Id) : ObjectReference;

	/// <summary>PostgreSQL: DROP OPERATOR CLASS, DROP OPERATOR FAMILY, COMMENT. <c>OPERATOR CLASS s.c USING btree</c>.</summary>
	public sealed record OperatorClass(ObjectKind Kind, QualifiedName Name, Identifier Method) : ObjectReference;

	/// <summary>PostgreSQL: DROP TRANSFORM, COMMENT. <c>TRANSFORM FOR type LANGUAGE l</c>.</summary>
	public sealed record Transform(DataType Type, Identifier Language) : ObjectReference;
}

public abstract partial record Statement
{
	/// <summary>
	/// PostgreSQL: DROP. <c>DROP kind [CONCURRENTLY] [IF EXISTS] object, … [CASCADE | RESTRICT]</c> for the kinds no record of
	/// the standard's or T-SQL's names: an index, an extension, a cast, a rule, a server and the rest. Every object is of the
	/// statement's <see cref="Kind"/>; <see cref="Concurrently"/> is an index's alone.
	/// </summary>
	public sealed record DropObject(ObjectKind Kind, SqlList<ObjectReference> Objects, bool IfExists = false, DropBehavior? Behavior = null, bool Concurrently = false) : Statement;

	/// <summary>PostgreSQL: COMMENT. <c>COMMENT ON object IS 'text'</c>; <c>IS NULL</c>, which takes the comment away, is a null text.</summary>
	public sealed record Comment(ObjectReference Object, LiteralValue.String? Text) : Statement;

	public sealed partial record DropView
	{
		/// <summary>PostgreSQL: DROP MATERIALIZED VIEW. The views are materialized ones.</summary>
		public bool Materialized { get; init; }
	}

	public sealed partial record DropTrigger
	{
		/// <summary>PostgreSQL: DROP TRIGGER. <c>ON t</c>: the table the trigger is on.</summary>
		public QualifiedName? Table { get; init; }

		/// <summary>PostgreSQL: DROP TRIGGER. <c>CASCADE</c> or <c>RESTRICT</c>.</summary>
		public DropBehavior? Behavior { get; init; }
	}

	public sealed partial record DropRole
	{
		/// <summary>PostgreSQL: DROP ROLE. Written <c>DROP USER</c> or <c>DROP GROUP</c>, older words for the same statement; null is ROLE.</summary>
		[Spelling] public RoleWord? Word { get; init; }
	}
}

/// <summary>PostgreSQL: DROP ROLE. The words that name a role besides ROLE.</summary>
public enum RoleWord { User, Group }
