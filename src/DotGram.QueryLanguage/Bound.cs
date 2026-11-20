using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.QueryLanguage;

// What the binder makes of a text: every name resolved against the model, every value typed, every use of a
// field checked against its rights. Nothing here depends on the values of a call or on a context: one bound
// query serves every call with the same text, and the translator builds each call's tree from it.

/// <summary>A bound text: the entity, the clauses in order, the columns it returns, its parameters and warnings.</summary>
sealed class BoundQuery(
	ModelEntity          entity,
	BoundClause[]        clauses,
	Shape                final,
	BoundExpr[]          output,
	string[]             names,
	Parameter[]          parameters,
	QlWarning[]          warnings)
{
	public ModelEntity   Entity     { get; } = entity;
	public BoundClause[] Clauses    { get; } = clauses;

	/// <summary>The shape of a row after the last clause, which <see cref="Output"/> reads.</summary>
	public Shape Final { get; } = final;

	/// <summary>The columns a row returns, over <see cref="Final"/>.</summary>
	public BoundExpr[] Output { get; } = output;

	public string[]    Names      { get; } = names;
	public Parameter[] Parameters { get; } = parameters;
	public QlWarning[] Warnings   { get; } = warnings;

	/// <summary>What the text returns and needs, as the host is told it.</summary>
	public QlQueryInfo Info { get; } = new(
		[.. output.Select((value, i) => new QlColumnInfo(names[i], value.Type.Type, value.Type.Core, value.Type.Nullable))],
		[.. parameters.Select(p => new QlParameterInfo(p.Name, p.Type!.Value.Type, p.Type.Value.Core, p.IsList, p.Nullable && !p.IsCount))],
		warnings);
}

/// <summary>
/// What a row is at one point of the chain: the entity itself; the entity with the values of the <c>let</c>s
/// over it; or, after a <c>select</c>, those values alone.
/// </summary>
sealed class Shape(ShapeKind kind, Column[] columns)
{
	public ShapeKind Kind    { get; } = kind;
	public Column[]  Columns { get; } = columns;
}

enum ShapeKind
{
	Entity,
	EntityRow,
	Scalars,
}

/// <summary>A value a <c>let</c> or a <c>select</c> names: a column of the rows after it.</summary>
sealed class Column(string name, int position, BoundExpr value)
{
	public string     Name     { get; } = name;
	public int        Position { get; } = position;
	public BoundExpr  Value    { get; } = value;
	public ScalarType Type     => Value.Type;

	/// <summary>Whether a clause or the result reads it, directly or through another column.</summary>
	public bool Used { get; set; }

	/// <summary>The rights already checked for it, so that a column read many times is walked once a right.</summary>
	public QlUse Checked { get; set; }

	/// <summary>
	/// How many nodes the column's value is with every name in it replaced by what it names: what a name of this
	/// column costs at each use. Saturates at <see cref="int.MaxValue"/>.
	/// </summary>
	public long Expanded { get; set; } = 1;
}

/// <summary>One parameter of the text, typed by where it stands.</summary>
sealed class Parameter(string name, int position, int index)
{
	public string Name     { get; } = name;
	public int    Position { get; } = position;
	public int    Index    { get; } = index;

	/// <summary>The type of a value, or of an item of a list; null until a place types it.</summary>
	public ScalarType? Type { get; set; }

	public bool IsList   { get; set; }
	public bool IsCount  { get; set; }

	/// <summary>Whether a value may be null: only where every place it stands in takes a null.</summary>
	public bool Nullable { get; set; } = true;

	/// <summary>The CLR type of the box a call's value goes in.</summary>
	public Type BoxType
	{
		get
		{
			var item = Type!.Value.WithNullable(Nullable && !IsCount).Clr;

			return IsList ? item.MakeArrayType() : item;
		}
	}
}

// ── Clauses ─────────────────────────────────────────────────────────────────

abstract class BoundClause(Shape before, Shape after, int position)
{
	/// <summary>The shape of a row this clause reads.</summary>
	public Shape Before { get; } = before;

	/// <summary>The shape of a row after it.</summary>
	public Shape After { get; } = after;

	public int Position { get; } = position;
}

sealed class BoundWhere(Shape shape, BoundExpr condition, int position) : BoundClause(shape, shape, position)
{
	public BoundExpr Condition { get; } = condition;
}

/// <summary>A <c>let</c>: the row it reads, and the same row with one value more.</summary>
sealed class BoundLet(Shape before, Shape after, Column column, int position) : BoundClause(before, after, position)
{
	public Column Column { get; } = column;
}

sealed class BoundOrderKey(BoundExpr key, bool descending, bool? nullsFirst)
{
	public BoundExpr Key        { get; } = key;
	public bool      Descending { get; } = descending;
	public bool?     NullsFirst { get; } = nullsFirst;
}

sealed class BoundOrder(Shape shape, BoundOrderKey[] keys, int position) : BoundClause(shape, shape, position)
{
	public BoundOrderKey[] Keys { get; } = keys;
}

/// <summary>A <c>select</c>: new columns over the row before it, or, when they are aggregates, over all its rows at once.</summary>
sealed class BoundSelect(Shape before, Shape after, bool aggregate, int position) : BoundClause(before, after, position)
{
	public bool Aggregate { get; } = aggregate;
}

sealed class BoundPage(Shape shape, bool skip, BoundExpr count, int position) : BoundClause(shape, shape, position)
{
	public bool      Skip  { get; } = skip;
	public BoundExpr Count { get; } = count;
}

// ── Values ──────────────────────────────────────────────────────────────────

abstract class BoundExpr(ScalarType type, int position)
{
	public ScalarType Type     { get; } = type;
	public int        Position { get; } = position;

	/// <summary>Whether the value reads a row; one that does not is the caller's, computed before the query runs.</summary>
	public abstract bool ReadsRow { get; }
}

/// <summary>A field of the entity, read from the row.</summary>
sealed class BoundField(ModelField field, string written, int position, string? qualifier) : BoundExpr(field.Value, position)
{
	public ModelField Field { get; } = field;

	/// <summary>The name as the text spells it.</summary>
	public string Written { get; } = written;

	/// <summary>The alias it was written after, if it was: an error about it is said the way the text said it.</summary>
	public string? Qualifier { get; } = qualifier;

	public override bool ReadsRow => true;
}

/// <summary>A column of a <c>let</c> or a <c>select</c>, read from the row.</summary>
sealed class BoundColumn(Column column, int position) : BoundExpr(column.Type, position)
{
	public Column Column { get; } = column;

	public override bool ReadsRow => true;
}

sealed class BoundLiteral(object? value, ScalarType type, int position) : BoundExpr(type, position)
{
	public object? Value { get; } = value;

	public override bool ReadsRow => false;
}

sealed class BoundParameter(Parameter parameter, ScalarType type, int position) : BoundExpr(type, position)
{
	public Parameter Parameter { get; } = parameter;

	public override bool ReadsRow => false;
}

/// <summary>A value of another type the language converts to without loss: <c>int</c> to <c>long</c>, <c>T</c> to <c>T?</c>.</summary>
sealed class BoundConvert(BoundExpr operand, ScalarType type) : BoundExpr(type, operand.Position)
{
	public BoundExpr Operand { get; } = operand;

	public override bool ReadsRow => Operand.ReadsRow;
}

sealed class BoundUnary(ExpressionType kind, BoundExpr operand, ScalarType type, int position) : BoundExpr(type, position)
{
	/// <summary><see cref="ExpressionType.Negate"/>, <see cref="ExpressionType.NegateChecked"/> or <see cref="ExpressionType.Not"/>.</summary>
	public ExpressionType Kind    { get; } = kind;
	public BoundExpr      Operand { get; } = operand;

	public override bool ReadsRow => Operand.ReadsRow;
}

sealed class BoundBinary(ExpressionType kind, BoundExpr left, BoundExpr right, ScalarType type, int position) : BoundExpr(type, position)
{
	/// <summary>An arithmetic, a comparison, <see cref="ExpressionType.AndAlso"/> or <see cref="ExpressionType.OrElse"/>.</summary>
	public ExpressionType Kind  { get; } = kind;
	public BoundExpr      Left  { get; } = left;
	public BoundExpr      Right { get; } = right;

	/// <summary>Strings are ordered by <c>string.Compare(left, right) op 0</c>, which providers read as the comparison.</summary>
	public bool ComparesStrings => Left.Type.Type == QlType.String && Kind is ExpressionType.LessThan or ExpressionType.LessThanOrEqual or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual;

	public override bool ReadsRow => Left.ReadsRow || Right.ReadsRow;
}

/// <summary><c>x in (a, b)</c> or <c>x in @list</c>.</summary>
sealed class BoundIn(BoundExpr operand, BoundExpr[]? items, BoundParameter? list, ScalarType type, int position) : BoundExpr(type, position)
{
	public BoundExpr       Operand { get; } = operand;
	public BoundExpr[]?    Items   { get; } = items;
	public BoundParameter? List    { get; } = list;

	public override bool ReadsRow => Operand.ReadsRow;
}

/// <summary>A member of the closed table: <c>name.Length</c>, <c>name.StartsWith(p)</c>, <c>created.Year</c>.</summary>
sealed class BoundMember(BoundExpr target, MemberInfo member, BoundExpr[] arguments, ScalarType type, int position) : BoundExpr(type, position)
{
	public BoundExpr   Target    { get; } = target;
	public MemberInfo  Member    { get; } = member;
	public BoundExpr[] Arguments { get; } = arguments;

	public override bool ReadsRow => true;
}

enum AggregateKind
{
	CountAll,
	Count,
	Sum,
	Average,
	Min,
	Max,
}

/// <summary>An aggregate over all the rows of the shape before the <c>select</c> it is a column of.</summary>
sealed class BoundAggregate(AggregateKind kind, BoundExpr? argument, ScalarType type, int position) : BoundExpr(type, position)
{
	public AggregateKind Kind     { get; } = kind;
	public BoundExpr?    Argument { get; } = argument;

	public override bool ReadsRow => true;
}

/// <summary>Walks the fields and columns a value reads.</summary>
static class Reads
{
	public static IEnumerable<BoundExpr> Children(BoundExpr node)
	{
		switch (node)
		{
			case BoundConvert convert:
				yield return convert.Operand;
				break;

			case BoundUnary unary:
				yield return unary.Operand;
				break;

			case BoundBinary binary:
				yield return binary.Left;
				yield return binary.Right;
				break;

			case BoundIn @in:
				yield return @in.Operand;

				foreach (var item in @in.Items ?? [])
					yield return item;

				break;

			case BoundMember member:
				yield return member.Target;

				foreach (var argument in member.Arguments)
					yield return argument;

				break;

			case BoundAggregate { Argument: { } argument }:
				yield return argument;
				break;
		}
	}
}
