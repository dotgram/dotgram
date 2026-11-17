using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>
/// Resolves the text against the model. It sees nothing but the model: a name is a field of the
/// root exposed for the use it is put to, or it is unknown, and the error does not say which.
/// </summary>
sealed class Binder
{
	Binder(QlBuiltModel model, QlLimits limits)
	{
		_model  = model;
		_limits = limits;
	}

	readonly QlBuiltModel                                   _model;
	readonly QlLimits                                       _limits;
	readonly List<Slot>                                     _slots      = [];
	readonly Dictionary<string, Slot>                       _parameters = new(StringComparer.Ordinal);
	QlEntity                                                _entity     = null!;
	string                                                  _qualifier  = "";

	public static BoundQuery Bind(QuerySyntax query, QlBuiltModel model, QlLimits limits)
	{
		return new Binder(model, limits).BindQuery(query);
	}

	static QlException Error(QlErrorKind kind, string message, int position)
	{
		return new QlException(new QlError(kind, message, position));
	}

	BoundQuery BindQuery(QuerySyntax query)
	{
		_entity = _model.FindEntity(query.Root.Text) ?? throw Error(QlErrorKind.UnknownName, $"Unknown source '{query.Root.Text}'.", query.Root.Position);

		if (query.Alias is { } alias)
		{
			if (_entity.FindColumn(alias.Text) is not null)
				throw Error(QlErrorKind.UnknownName, $"The alias '{alias.Text}' is also the name of a field; choose another.", alias.Position);

			_qualifier = alias.Text;
		}
		else
		{
			_qualifier = _entity.Name;
		}

		var outputs = BindSelect(query);
		var where   = query.Where is null ? null : BindPredicate(query.Where);
		var order   = BindOrder(query, outputs);
		var limit   = query.Limit  is null ? null : BindCount(query.Limit, true);
		var offset  = query.Offset is null ? null : BindCount(query.Offset, false);

		return new BoundQuery(_entity, outputs, where, order, limit, offset, _slots);
	}

	List<BoundOutput> BindSelect(QuerySyntax query)
	{
		var outputs = new List<BoundOutput>();
		var names   = new HashSet<string>(QlNames.Comparer);

		if (query.Select.Length == 0)
		{
			foreach (var column in _entity.Columns)
				if ((column.Uses & QlUse.Select) != 0)
					outputs.Add(new BoundOutput(column.Name, column));
		}
		else
		{
			foreach (var item in query.Select)
			{
				var column = ResolveColumn(item.Column, QlUse.Select);
				var name   = item.Alias?.Text ?? column.Name;

				if (!names.Add(name))
					throw Error(QlErrorKind.UnknownName, $"The result has two fields named '{name}'.", item.Alias?.Position ?? item.Column.Position);

				outputs.Add(new BoundOutput(name, column));
			}
		}

		if (outputs.Count > _limits.MaxColumns)
			throw Error(QlErrorKind.Limit, $"The result has {outputs.Count} fields; at most {_limits.MaxColumns} are allowed.", query.Root.Position);

		return outputs;
	}

	List<BoundOrder> BindOrder(QuerySyntax query, List<BoundOutput> outputs)
	{
		var order = new List<BoundOrder>();

		foreach (var item in query.Order)
		{
			QlColumn? column = null;

			// A name of the result first, as SQL has it: the alias of a field selected.
			if (item.Column.Qualifier is null)
				foreach (var output in outputs)
					if (QlNames.Comparer.Equals(output.Name, item.Column.Name.Text) && (output.Column.Uses & QlUse.Sort) != 0)
						column = output.Column;

			column ??= ResolveColumn(item.Column, QlUse.Sort);

			order.Add(new BoundOrder(column, item.Descending));
		}

		return order;
	}

	QlColumn ResolveColumn(ColumnSyntax syntax, QlUse use)
	{
		if (syntax.Qualifier is { } qualifier && !QlNames.Comparer.Equals(qualifier.Text, _qualifier))
			throw Error(QlErrorKind.UnknownName, $"Unknown source '{qualifier.Text}'.", qualifier.Position);

		var column = _entity.FindColumn(syntax.Name.Text);

		// Not there, and there but not for this use, are one answer.
		if (column is null || (column.Uses & use) == 0)
			throw Error(QlErrorKind.UnknownName, $"Unknown field '{syntax.Name.Text}'.", syntax.Name.Position);

		return column;
	}

	Slot BindCount(ExprSyntax syntax, bool isLimit)
	{
		var word = isLimit ? "LIMIT" : "OFFSET";

		switch (syntax)
		{
			case LiteralSyntax { Kind: LiteralKind.Number } literal when int.TryParse(literal.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var count):
			{
				var max = isLimit ? int.MaxValue : _limits.MaxOffset;

				if (count > max)
					throw Error(QlErrorKind.Limit, $"{word} is at most {max}.", literal.Position);

				return AddSlot(new Slot(_slots.Count, SlotKind.Count, QlType.Int, false, literal.Position) { Literal = count });
			}

			case ParameterSyntax parameter:
				return Parameter(parameter, SlotKind.Count, QlType.Int, false);

			default:
				throw Error(QlErrorKind.Type, $"{word} takes a non-negative integer or a parameter.", syntax.Position);
		}
	}

	Slot AddSlot(Slot slot)
	{
		_slots.Add(slot);

		return slot;
	}

	/// <summary>
	/// A parameter's slot. Its type comes from where it stands; a name used twice must stand
	/// where the same type is wanted both times.
	/// </summary>
	Slot Parameter(ParameterSyntax syntax, SlotKind kind, QlType type, bool nullable)
	{
		if (_parameters.TryGetValue(syntax.Name, out var existing))
		{
			if (existing.Kind != kind || existing.Type != type)
				throw Error(QlErrorKind.Type, $"@{syntax.Name} is used as {Describe(existing.Kind, existing.Type)} and as {Describe(kind, type)}.", syntax.Position);

			return existing;
		}

		var slot = AddSlot(new Slot(_slots.Count, kind, type, nullable, syntax.Position) { Parameter = syntax.Name });

		_parameters.Add(syntax.Name, slot);

		return slot;
	}

	static string Describe(SlotKind kind, QlType type)
	{
		return kind switch
		{
			SlotKind.List        => "a list of " + QlTypes.Name(type),
			SlotKind.LikePattern => "a LIKE pattern",
			SlotKind.Count       => "a row count",
			_                    => QlTypes.Name(type),
		};
	}

	// ── Conditions ──────────────────────────────────────────────────────────

	BoundExpr BindPredicate(ExprSyntax syntax)
	{
		if (syntax.Depth > _limits.MaxSyntaxDepth)
			throw Error(QlErrorKind.Limit, $"The condition nests {syntax.Depth} deep; at most {_limits.MaxSyntaxDepth} is allowed.", syntax.Position);

		var bound = BindValue(syntax, QlType.Bool);

		if (bound.Type != QlType.Bool)
			throw Error(QlErrorKind.Type, $"A condition is wanted here, not a {QlTypes.Name(bound.Type)}.", syntax.Position);

		return bound;
	}

	BoundExpr BindValue(ExprSyntax syntax, QlType? expected)
	{
		switch (syntax)
		{
			case ParenthesizedSyntax parenthesized:
				return BindValue(parenthesized.Inner, expected);

			case ColumnSyntax column:
				return new BoundColumn(ResolveColumn(column, QlUse.Filter));

			case LiteralSyntax literal:
				return Literal(literal, expected);

			case ParameterSyntax parameter:
			{
				if (expected is not { } type)
					throw Error(QlErrorKind.Type, $"The type of @{parameter.Name} cannot be told from where it stands; compare it with a field.", parameter.Position);

				return new BoundSlot(Parameter(parameter, SlotKind.Scalar, type, true));
			}

			case LogicalSyntax logical:
			{
				var items = new BoundExpr[logical.Items.Length];

				for (var i = 0; i < items.Length; i++)
					items[i] = Condition(logical.Items[i]);

				return new BoundLogical(logical.Operator, items);
			}

			case NotSyntax not:
				return new BoundNot(Condition(not.Operand));

			case NegateSyntax negate:
				return Negate(negate, expected);

			case ArithmeticSyntax arithmetic:
				return Arithmetic(arithmetic, expected);

			case CompareSyntax compare:
				return Compare(compare);

			case IsNullSyntax isNull:
			{
				var operand = BindTyped(isNull.Operand, null);

				return new BoundIsNull(operand, isNull.Negated);
			}

			case InSyntax @in:
				return In(@in);

			case LikeSyntax like:
				return Like(like);

			case BetweenSyntax between:
			{
				// x BETWEEN a AND b is x >= a AND x <= b; the operand is read twice, which the
				// node cap of the built tree counts.
				var low  = Compare(new CompareSyntax(between.Operand, CompareOperator.GreaterOrEqual, between.Low));
				var high = Compare(new CompareSyntax(between.Operand, CompareOperator.LessOrEqual,    between.High));
				var both = new BoundLogical(LogicalOperator.And, [low, high]);

				return between.Negated ? new BoundNot(both) : both;
			}

			default:
				throw Error(QlErrorKind.Syntax, "Not supported here.", syntax.Position);
		}
	}

	BoundExpr Condition(ExprSyntax syntax)
	{
		var bound = BindValue(syntax, QlType.Bool);

		if (bound.Type != QlType.Bool)
			throw Error(QlErrorKind.Type, $"A condition is wanted here, not a {QlTypes.Name(bound.Type)}.", syntax.Position);

		return bound;
	}

	/// <summary>
	/// An operand that must have a type of its own: a field, or an expression over one.
	/// </summary>
	BoundExpr BindTyped(ExprSyntax syntax, QlType? expected)
	{
		return BindValue(syntax, expected);
	}

	static bool IsUntyped(ExprSyntax syntax)
	{
		while (syntax is ParenthesizedSyntax parenthesized)
			syntax = parenthesized.Inner;

		return syntax is ParameterSyntax or LiteralSyntax { Kind: LiteralKind.Number or LiteralKind.Null } or NegateSyntax { Operand: LiteralSyntax { Kind: LiteralKind.Number } };
	}

	BoundExpr Compare(CompareSyntax compare)
	{
		BoundExpr left;
		BoundExpr right;

		// The side that has a type of its own decides the other's: `total > 100` makes 100 a
		// decimal, `id = @p` makes @p an int.
		if (IsUntyped(compare.Left) && !IsUntyped(compare.Right))
		{
			right = BindTyped(compare.Right, null);
			left  = BindValue(compare.Left, right.Type);
		}
		else
		{
			left  = BindTyped(compare.Left, null);
			right = BindValue(compare.Right, left.Type);
		}

		if (left.Type == QlType.Bool && compare.Operator is not (CompareOperator.Equal or CompareOperator.NotEqual))
			throw Error(QlErrorKind.Type, "Booleans compare only for equality.", compare.Position);

		if (left.Type == QlType.Guid && compare.Operator is not (CompareOperator.Equal or CompareOperator.NotEqual))
			throw Error(QlErrorKind.Type, "Guids compare only for equality.", compare.Position);

		(left, right) = Meet(left, right, compare.Position);

		return new BoundCompare(compare.Operator, left, right);
	}

	(BoundExpr Left, BoundExpr Right) Meet(BoundExpr left, BoundExpr right, int position)
	{
		if (left.Type == right.Type)
			return (left, right);

		if (QlTypes.IsNumeric(left.Type) && QlTypes.IsNumeric(right.Type) && QlTypes.Widen(left.Type, right.Type) is { } common)
		{
			if (left.Type != common)
				left = new BoundConvert(left, common);

			if (right.Type != common)
				right = new BoundConvert(right, common);

			return (left, right);
		}

		throw Error(QlErrorKind.Type, $"A {QlTypes.Name(left.Type)} and a {QlTypes.Name(right.Type)} cannot be compared or combined.", position);
	}

	BoundExpr Arithmetic(ArithmeticSyntax syntax, QlType? expected)
	{
		var (operands, depth) = Chain(syntax);

		if (operands > _limits.MaxArithmeticOperands)
			throw Error(QlErrorKind.Limit, $"The arithmetic has {operands} operands; at most {_limits.MaxArithmeticOperands} are allowed.", syntax.Position);

		if (depth > _limits.MaxArithmeticDepth)
			throw Error(QlErrorKind.Limit, $"The arithmetic nests {depth} deep; at most {_limits.MaxArithmeticDepth} is allowed.", syntax.Position);

		BoundExpr left;
		BoundExpr right;

		if (IsUntyped(syntax.Left) && !IsUntyped(syntax.Right))
		{
			right = BindValue(syntax.Right, expected);
			left  = BindValue(syntax.Left, right.Type);
		}
		else
		{
			left  = BindValue(syntax.Left, expected);
			right = BindValue(syntax.Right, left.Type);
		}

		if (!QlTypes.IsNumeric(left.Type) || !QlTypes.IsNumeric(right.Type))
			throw Error(QlErrorKind.Type, "Arithmetic is on numbers only.", syntax.Position);

		if (syntax.Operator == ArithmeticOperator.Modulo && (left.Type is QlType.Double or QlType.Decimal || right.Type is QlType.Double or QlType.Decimal))
			throw Error(QlErrorKind.Type, "% is on integers only.", syntax.Position);

		(left, right) = Meet(left, right, syntax.Position);

		return new BoundArithmetic(syntax.Operator, left, right, left.Type);
	}

	/// <summary>
	/// The operands of an arithmetic sub-tree and its depth, through parentheses and minus signs:
	/// parentheses do not change the shape linq2db is given, so they do not reset the count.
	/// Iterative; the syntax depth was checked before binding began.
	/// </summary>
	static (int Operands, int Depth) Chain(ExprSyntax root)
	{
		var operands = 0;
		var deepest  = 0;
		var stack    = new Stack<(ExprSyntax Node, int Depth)>();

		stack.Push((root, 1));

		while (stack.Count > 0)
		{
			var (node, depth) = stack.Pop();

			switch (node)
			{
				case ParenthesizedSyntax parenthesized:
					stack.Push((parenthesized.Inner, depth));
					break;

				case NegateSyntax negate:
					stack.Push((negate.Operand, depth + 1));
					break;

				case ArithmeticSyntax arithmetic:
					stack.Push((arithmetic.Left,  depth + 1));
					stack.Push((arithmetic.Right, depth + 1));
					break;

				default:
					operands++;

					if (depth > deepest)
						deepest = depth;

					break;
			}
		}

		return (operands, deepest);
	}

	BoundExpr Negate(NegateSyntax syntax, QlType? expected)
	{
		if (syntax.Operand is LiteralSyntax { Kind: LiteralKind.Number } literal)
			return Literal(literal, expected, negative: true);

		var operand = BindValue(syntax.Operand, expected);

		if (!QlTypes.IsNumeric(operand.Type))
			throw Error(QlErrorKind.Type, "Minus is on numbers only.", syntax.Position);

		return new BoundNegate(operand);
	}

	BoundExpr In(InSyntax syntax)
	{
		var operand = BindTyped(syntax.Operand, null);

		Slot list;

		if (syntax.Parameter is { } parameter)
		{
			list = Parameter(parameter, SlotKind.List, operand.Type, operand.Nullable);
		}
		else
		{
			var items = syntax.Items!;

			if (items.Length > _limits.MaxInList)
				throw Error(QlErrorKind.Limit, $"The list has {items.Length} values; at most {_limits.MaxInList} are allowed.", syntax.Position);

			var values = new SlotItem[items.Length];

			for (var i = 0; i < items.Length; i++)
				values[i] = ListItem(items[i], operand.Type);

			list = AddSlot(new Slot(_slots.Count, SlotKind.List, operand.Type, operand.Nullable, syntax.Position) { Items = values });
		}

		return new BoundIn(operand, list, syntax.Negated);
	}

	SlotItem ListItem(ExprSyntax item, QlType type)
	{
		switch (item)
		{
			case ParameterSyntax parameter:
			{
				// A parameter inside a written list is a scalar of the list's type.
				Parameter(parameter, SlotKind.Scalar, type, false);

				return new SlotItem(null, parameter.Name, parameter.Position);
			}

			case LiteralSyntax literal:
				return ListLiteral(literal, type, negative: false);

			case NegateSyntax { Operand: LiteralSyntax { Kind: LiteralKind.Number } negated }:
				return ListLiteral(negated, type, negative: true);

			default:
				throw Error(QlErrorKind.Type, "An IN list holds literals and parameters only.", item.Position);
		}
	}

	SlotItem ListLiteral(LiteralSyntax literal, QlType type, bool negative)
	{
		var (actual, value) = LiteralValue(literal, type, negative);

		if (actual != type)
			throw Error(QlErrorKind.Type, $"The list wants values of type {QlTypes.Name(type)}.", literal.Position);

		return new SlotItem(value, null, literal.Position);
	}

	BoundExpr Like(LikeSyntax syntax)
	{
		var operand = BindTyped(syntax.Operand, null);

		if (operand.Type != QlType.String)
			throw Error(QlErrorKind.Type, "LIKE is on strings only.", syntax.Position);

		char? escape = null;

		if (syntax.Escape is not null)
		{
			if (syntax.Escape is not LiteralSyntax { Kind: LiteralKind.String, Text.Length: 1 } escapeLiteral)
				throw Error(QlErrorKind.Type, "ESCAPE takes one character in quotes.", syntax.Escape.Position);

			escape = escapeLiteral.Text[0];
		}

		Slot pattern;

		switch (syntax.Pattern)
		{
			case LiteralSyntax { Kind: LiteralKind.String } literal:
			{
				if (LikePatterns.Check(literal.Text, escape) is { } problem)
					throw Error(QlErrorKind.Type, problem, literal.Position);

				if (literal.Text.Length > _limits.MaxStringLength)
					throw Error(QlErrorKind.Limit, $"The pattern is longer than {_limits.MaxStringLength} characters.", literal.Position);

				pattern = AddSlot(new Slot(_slots.Count, SlotKind.LikePattern, QlType.String, false, literal.Position) { Literal = literal.Text, Escape = escape });

				break;
			}

			case ParameterSyntax parameter:
			{
				if (_parameters.ContainsKey(parameter.Name))
					throw Error(QlErrorKind.Type, $"@{parameter.Name} is a LIKE pattern and may stand only there.", parameter.Position);

				pattern = AddSlot(new Slot(_slots.Count, SlotKind.LikePattern, QlType.String, false, parameter.Position) { Parameter = parameter.Name, Escape = escape });
				_parameters.Add(parameter.Name, pattern);

				break;
			}

			default:
				throw Error(QlErrorKind.Type, "A LIKE pattern is a string literal or a parameter.", syntax.Pattern.Position);
		}

		return new BoundLike(operand, pattern, syntax.Negated);
	}

	// ── Literals ───────────────────────────────────────────────────────────

	BoundExpr Literal(LiteralSyntax literal, QlType? expected, bool negative = false)
	{
		var (type, value) = LiteralValue(literal, expected, negative);

		return new BoundSlot(AddSlot(new Slot(_slots.Count, SlotKind.Scalar, type, false, literal.Position) { Literal = value }));
	}

	(QlType Type, object Value) LiteralValue(LiteralSyntax literal, QlType? expected, bool negative)
	{
		var (type, value) = literal.Kind switch
		{
			LiteralKind.Number    => Number(literal, expected, negative),
			LiteralKind.String    => (QlType.String, (object)literal.Text),
			LiteralKind.True      => (QlType.Bool, true),
			LiteralKind.False     => (QlType.Bool, false),
			LiteralKind.Date      => (QlType.Timestamp, DateLiteral(literal, "yyyy-MM-dd")),
			LiteralKind.Timestamp => (QlType.Timestamp, DateLiteral(literal, "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF")),
			_                     => throw Error(QlErrorKind.Type, "NULL is compared with IS NULL.", literal.Position),
		};

		if (value is string text && text.Length > _limits.MaxStringLength)
			throw Error(QlErrorKind.Limit, $"The string is longer than {_limits.MaxStringLength} characters.", literal.Position);

		return (type, value);
	}

	object DateLiteral(LiteralSyntax literal, params string[] formats)
	{
		if (DateTime.TryParseExact(literal.Text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
			return value;

		throw Error(QlErrorKind.Type, $"'{literal.Text}' is not a {(literal.Kind == LiteralKind.Date ? "date" : "timestamp")} in ISO 8601.", literal.Position);
	}

	/// <summary>
	/// A number takes the type wanted where it stands when it fits that type exactly, and its
	/// own type otherwise: digits alone an int (or a long, or a decimal), with a point a
	/// decimal, with an exponent a double.
	/// </summary>
	(QlType, object) Number(LiteralSyntax literal, QlType? expected, bool negative)
	{
		var text = negative ? "-" + literal.Text : literal.Text;

		if (literal.Text.IndexOfAny(['e', 'E']) >= 0)
		{
			if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || double.IsInfinity(d))
				throw Error(QlErrorKind.Type, $"{text} is not a finite number.", literal.Position);

			return (QlType.Double, d);
		}

		if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
			throw Error(QlErrorKind.Type, $"{text} is out of range.", literal.Position);

		var integral = literal.Text.IndexOf('.') < 0;

		switch (expected)
		{
			case QlType.Int when integral && value is >= int.MinValue and <= int.MaxValue:
				return (QlType.Int, (int)value);

			case QlType.Long when integral && value is >= long.MinValue and <= long.MaxValue:
				return (QlType.Long, (long)value);

			case QlType.Decimal:
				return (QlType.Decimal, value);

			case QlType.Double when (decimal)(double)value == value:
				return (QlType.Double, (double)value);
		}

		if (!integral)
			return (QlType.Decimal, value);

		if (value is >= int.MinValue and <= int.MaxValue)
			return (QlType.Int, (int)value);

		if (value is >= long.MinValue and <= long.MaxValue)
			return (QlType.Long, (long)value);

		return (QlType.Decimal, value);
	}
}

/// <summary>
/// The portable LIKE of the language: '%' and '_' and one escape character the text names.
/// Every other character stands for itself, whatever the database would make of it.
/// </summary>
static class LikePatterns
{
	public static string? Check(string pattern, char? escape)
	{
		if (escape is not { } e)
			return null;

		for (var i = 0; i < pattern.Length; i++)
		{
			if (pattern[i] != e)
				continue;

			if (i + 1 == pattern.Length)
				return "The pattern ends in its escape character.";

			var next = pattern[i + 1];

			if (next != '%' && next != '_' && next != e)
				return "The escape character stands before '%', '_' or itself only.";

			i++;
		}

		return null;
	}

	/// <summary>
	/// The pattern as runs of literal text and wildcards: '%' is <c>null</c> text with
	/// <c>Any</c>, '_' is <c>One</c>.
	/// </summary>
	public static List<(string? Text, bool Any)> Split(string pattern, char? escape)
	{
		var parts   = new List<(string?, bool)>();
		var literal = new System.Text.StringBuilder();

		for (var i = 0; i < pattern.Length; i++)
		{
			var c = pattern[i];

			if (escape is { } e && c == e)
			{
				literal.Append(pattern[++i]);
				continue;
			}

			if (c is '%' or '_')
			{
				if (literal.Length > 0)
				{
					parts.Add((literal.ToString(), false));
					literal.Clear();
				}

				parts.Add((null, c == '%'));
				continue;
			}

			literal.Append(c);
		}

		if (literal.Length > 0)
			parts.Add((literal.ToString(), false));

		return parts;
	}
}
