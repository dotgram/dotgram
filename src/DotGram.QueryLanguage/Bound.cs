using System;
using System.Collections.Generic;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

// The bound tree: names resolved against the model, every operand typed, every value of the text
// a slot. It depends on the text and the model only, so it is what the query cache keeps; values
// and the root's policy arrive per request, at translation.

enum SlotKind
{
	/// <summary>One value, written or passed.</summary>
	Scalar,

	/// <summary>The values of an IN: written as a list, or one parameter holding an array.</summary>
	List,

	/// <summary>A LIKE pattern, rewritten into the provider's dialect per request.</summary>
	LikePattern,

	/// <summary>A row count: LIMIT or OFFSET.</summary>
	Count,
}

/// <summary>
/// A value of the query. A literal of the text is a slot like a parameter is, so that two texts
/// differing only in their literals are one shape for linq2db's cache and every value reaches
/// the database as a parameter.
/// </summary>
sealed class Slot(int index, SlotKind kind, QlType type, bool nullable, int position)
{
	public int      Index    { get; } = index;
	public SlotKind Kind     { get; } = kind;
	public QlType   Type     { get; } = type;
	public bool     Nullable { get; } = nullable;
	public int      Position { get; } = position;

	/// <summary>The parameter's name, without '@'; null for a literal.</summary>
	public string? Parameter { get; init; }

	/// <summary>The literal's value, already of the slot's CLR type.</summary>
	public object? Literal { get; init; }

	/// <summary>For a written list: each item a literal value or a parameter name.</summary>
	public SlotItem[]? Items { get; init; }

	/// <summary>For a LIKE pattern: the escape character the text named.</summary>
	public char? Escape { get; init; }

	/// <summary>The CLR type of the value the slot holds: an array for a list.</summary>
	public Type ClrType
	{
		get
		{
			var element = QlTypes.Clr(Type, Nullable);

			return Kind == SlotKind.List ? element.MakeArrayType() : element;
		}
	}
}

readonly record struct SlotItem(object? Literal, string? Parameter, int Position);

abstract class BoundExpr(QlType type, bool nullable)
{
	public QlType Type     { get; } = type;
	public bool   Nullable { get; } = nullable;
}

sealed class BoundColumn(QlColumn column) : BoundExpr(column.Type, column.Nullable)
{
	public QlColumn Column { get; } = column;
}

sealed class BoundSlot(Slot slot) : BoundExpr(slot.Type, slot.Nullable)
{
	public Slot Slot { get; } = slot;
}

/// <summary>An implicit widening, int to long and the like; never a narrowing.</summary>
sealed class BoundConvert(BoundExpr operand, QlType type) : BoundExpr(type, operand.Nullable)
{
	public BoundExpr Operand { get; } = operand;
}

sealed class BoundNegate(BoundExpr operand) : BoundExpr(operand.Type, operand.Nullable)
{
	public BoundExpr Operand { get; } = operand;
}

sealed class BoundArithmetic(ArithmeticOperator op, BoundExpr left, BoundExpr right, QlType type)
	: BoundExpr(type, left.Nullable || right.Nullable)
{
	public ArithmeticOperator Operator { get; } = op;
	public BoundExpr          Left     { get; } = left;
	public BoundExpr          Right    { get; } = right;
}

sealed class BoundCompare(CompareOperator op, BoundExpr left, BoundExpr right) : BoundExpr(QlType.Bool, false)
{
	public CompareOperator Operator { get; } = op;
	public BoundExpr       Left     { get; } = left;
	public BoundExpr       Right    { get; } = right;
}

/// <summary>A chain of AND or OR, kept as a list: the translator builds it balanced.</summary>
sealed class BoundLogical(LogicalOperator op, BoundExpr[] items) : BoundExpr(QlType.Bool, false)
{
	public LogicalOperator Operator { get; } = op;
	public BoundExpr[]     Items    { get; } = items;
}

sealed class BoundNot(BoundExpr operand) : BoundExpr(QlType.Bool, false)
{
	public BoundExpr Operand { get; } = operand;
}

sealed class BoundIsNull(BoundExpr operand, bool negated) : BoundExpr(QlType.Bool, false)
{
	public BoundExpr Operand { get; } = operand;
	public bool      Negated { get; } = negated;
}

sealed class BoundIn(BoundExpr operand, Slot list, bool negated) : BoundExpr(QlType.Bool, false)
{
	public BoundExpr Operand { get; } = operand;
	public Slot      List    { get; } = list;
	public bool      Negated { get; } = negated;
}

sealed class BoundLike(BoundExpr operand, Slot pattern, bool negated) : BoundExpr(QlType.Bool, false)
{
	public BoundExpr Operand { get; } = operand;
	public Slot      Pattern { get; } = pattern;
	public bool      Negated { get; } = negated;
}

sealed class BoundOutput(string name, QlColumn column)
{
	public string   Name   { get; } = name;
	public QlColumn Column { get; } = column;
}

sealed class BoundOrder(QlColumn column, bool descending)
{
	public QlColumn Column     { get; } = column;
	public bool     Descending { get; } = descending;
}

sealed class BoundQuery(
	QlEntity                   entity,
	IReadOnlyList<BoundOutput> outputs,
	BoundExpr?                 where,
	IReadOnlyList<BoundOrder>  order,
	Slot?                      limit,
	Slot?                      offset,
	IReadOnlyList<Slot>        slots)
{
	public QlEntity                   Entity  { get; } = entity;
	public IReadOnlyList<BoundOutput> Outputs { get; } = outputs;
	public BoundExpr?                 Where   { get; } = where;
	public IReadOnlyList<BoundOrder>  Order   { get; } = order;
	public Slot?                      Limit   { get; } = limit;
	public Slot?                      Offset  { get; } = offset;
	public IReadOnlyList<Slot>        Slots   { get; } = slots;
}
